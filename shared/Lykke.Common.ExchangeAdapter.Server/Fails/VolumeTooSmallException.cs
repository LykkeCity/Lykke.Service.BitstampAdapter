using System;
using System.Text.RegularExpressions;

namespace Lykke.Common.ExchangeAdapter.Server.Fails
{
    public class VolumeTooSmallException: Exception
    {
        public VolumeTooSmallException()
        {
        }

        public VolumeTooSmallException(string message)
            : base(message)
        {
        }

        public VolumeTooSmallException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public static bool TryParse(string message, out VolumeTooSmallException ex)
        {
            var m = Regex.Match(message, @"^Min amount each order:(?<amount>.*)$");

            if (m.Success)
            {

                ex = new VolumeTooSmallException(message);
                return true;
            }

            ex = null;
            return false;
        }
    }
}
