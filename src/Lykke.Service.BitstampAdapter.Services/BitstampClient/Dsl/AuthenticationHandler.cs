using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Lykke.Common.ExchangeAdapter;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl
{
    internal sealed class AuthenticationHandler : DelegatingHandler
    {
        private readonly string _customerId;
        private readonly string _key;
        private readonly byte[] _secret;

        public AuthenticationHandler(string customerId, string key, byte[] secret, HttpMessageHandler next): base(next)
        {
            _customerId = customerId;
            _key = key;
            _secret = secret;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Post && request.Content is FormUrlEncodedContent form)
            {           
                var nv = await form.ReadAsFormDataAsync(cancellationToken);

                var original = nv.AllKeys.ToDictionary(x => x, x => nv[x]);

                return await EpochNonce.Lock(_key, async nonce =>
                {
                    var strNonce = nonce.ToString(CultureInfo.InvariantCulture);

                    var message = $"{strNonce}{_customerId}{_key}";

                    string signature;

                    using (var hmac = new HMACSHA256(_secret))
                    {
                        signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(message)).ToHexString().ToUpper();
                    }

                    var authParams = new Dictionary<string, string>
                    {
                        { "key", _key },
                        { "nonce", strNonce },
                        { "signature", signature }
                    };

                    request.Content = new FormUrlEncodedContent(original.Concat(authParams));

                    return await base.SendAsync(request, cancellationToken);
                });
            }
            else
            {
                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}
