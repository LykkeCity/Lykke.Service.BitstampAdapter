using System;

namespace Lykke.Common.ExchangeAdapter.Server.Fails
{
    public class NoBalanceException : Exception
    {
        public NoBalanceException()
        {
        }

        public NoBalanceException(string message)
            : base(message)
        {
        }

        public NoBalanceException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public static bool TryParse(string code, out NoBalanceException ex)
        {
            if (code == "NO_BALANCE")
            {
                ex = new NoBalanceException();
                return true;
            }

            ex = null;
            return false;
        }
    }
}