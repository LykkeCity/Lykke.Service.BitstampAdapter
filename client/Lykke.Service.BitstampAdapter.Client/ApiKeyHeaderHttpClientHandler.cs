using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Service.BitstampAdapter.Client
{
    /// <summary>
    /// Adds api-key header to the request
    /// </summary>
    public class ApiKeyHeaderHttpClientHandler : DelegatingHandler
    {
        private readonly string _headerName;
        private readonly string _apiKey;

        /// <summary>
        /// Creates a new instance of the <see cref="ApiKeyHeaderHttpClientHandler"></see> class with a specific header name and api key value
        /// </summary>
        /// <param name="headerName">The api key header name</param>
        /// <param name="apiKey">The api key</param>
        /// <exception cref="ArgumentNullException">Header name is null or empty</exception>
        /// <exception cref="ArgumentNullException">Api key is null or empty</exception>
        public ApiKeyHeaderHttpClientHandler(string headerName, string apiKey)
        {
            if (string.IsNullOrEmpty(headerName))
                throw new ArgumentNullException(nameof(headerName));

            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            _headerName = headerName;
            _apiKey = apiKey;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation(_headerName, _apiKey);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
