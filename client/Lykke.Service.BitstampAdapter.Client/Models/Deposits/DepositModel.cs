using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Deposits
{
    /// <summary>
    /// Describes a deposit
    /// </summary>
    [PublicAPI]
    public class DepositModel
    {
        /// <summary>
        /// The deposit amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The deposit address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The number of confirmations of transaction
        /// </summary>
        public int Confirmations { get; set; }
    }
}
