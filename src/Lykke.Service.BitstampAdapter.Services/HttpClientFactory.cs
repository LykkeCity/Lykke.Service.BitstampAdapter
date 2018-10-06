using System;
using System.Collections.Generic;
using System.Net.Http;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl;

namespace Lykke.Service.BitstampAdapter.Services
{
    public class HttpClientFactory
    {
        private readonly object _sync = new object();

        private readonly ILog _log;
        
        private readonly IDictionary<string, HttpClient> _clients = new Dictionary<string, HttpClient>();
        private readonly IDictionary<string, HttpClient> _clientsV1 = new Dictionary<string, HttpClient>();
        
        public HttpClientFactory(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }
        
        public HttpClient GetClient(IApiCredentials credentials, string internalApiKey)
        {
            lock (_sync)
            {
                if (!_clients.ContainsKey(internalApiKey))
                    _clients[internalApiKey] = CreateClient(credentials, "https://www.bitstamp.net/api/v2/");
                
                return _clients[internalApiKey];
            }
        }
        
        public HttpClient GetClientV1(IApiCredentials credentials, string internalApiKey)
        {
            lock (_sync)
            {
                if (!_clientsV1.ContainsKey(internalApiKey))
                    _clientsV1[internalApiKey] = CreateClient(credentials, "https://www.bitstamp.net/api/");
                
                return _clientsV1[internalApiKey];
            }
        }

        private HttpClient CreateClient(IApiCredentials credentials, string baseUrl)
        {
            HttpMessageHandler handler = new LoggingHandler(_log, new HttpClientHandler());

            if (credentials != null)
            {
                handler = new AuthenticationHandler(
                    credentials.UserId,
                    credentials.Key,
                    credentials.Secret,
                    handler);
            }

            return new HttpClient(handler) {BaseAddress = new Uri(baseUrl)};
        }
    }
}
