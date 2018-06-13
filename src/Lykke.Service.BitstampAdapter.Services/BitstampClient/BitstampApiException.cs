using System;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient
{
    public class BitstampApiException : Exception
    {
        public BitstampApiException(string message): base(message) {}
    }
}