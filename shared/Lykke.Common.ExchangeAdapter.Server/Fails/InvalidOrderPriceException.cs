using System;

namespace Lykke.Common.ExchangeAdapter.Server.Fails
{
    public sealed class InvalidOrderPriceException : Exception
    {
        public InvalidOrderPriceException()
        {

        }

        public InvalidOrderPriceException(string message): base(message)
        {

        }
    }
}