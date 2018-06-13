using System;

namespace Lykke.Common.ExchangeAdapter.Server.Fails
{
    public class OrderNotFoundException : Exception
    {
        public OrderNotFoundException()
        {
        }

        public OrderNotFoundException(string message): base(message)
        {
        }
    }
}
