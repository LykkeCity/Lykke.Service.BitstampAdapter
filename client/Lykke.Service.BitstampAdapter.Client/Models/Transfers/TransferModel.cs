using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Transfers
{
    /// <summary>
    /// Describes a deposit
    /// </summary>
    [PublicAPI]
    public class TransferModel
    {
        /// <summary>
        /// The account
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// The requested amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency of transfer 
        /// </summary>
        public string Currency { get; set; }
    }
}
