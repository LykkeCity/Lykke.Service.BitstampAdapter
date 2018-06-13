using System;

namespace Lykke.Common.ExchangeAdapter.Server.Fails
{
    public class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException()
        {
        }

        public InsufficientBalanceException(string message) : base(message)
        {
        }
    }
}
