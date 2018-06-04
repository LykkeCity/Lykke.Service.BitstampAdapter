using System;
using Common.Log;

namespace Lykke.Service.BitstampAdapter.Client
{
    public class BitstampAdapterClient : IBitstampAdapterClient, IDisposable
    {
        private readonly ILog _log;

        public BitstampAdapterClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
