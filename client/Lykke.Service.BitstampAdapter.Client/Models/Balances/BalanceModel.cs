using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Balances
{
    /// <summary>
    /// Describes a asset balance
    /// </summary>
    [PublicAPI]
    public class BalanceModel
    {
        /// <summary>
        /// The asset id
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// The current amount
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// The reserved amount
        /// </summary>
        public decimal Reserved { get; set; }
    }
}
