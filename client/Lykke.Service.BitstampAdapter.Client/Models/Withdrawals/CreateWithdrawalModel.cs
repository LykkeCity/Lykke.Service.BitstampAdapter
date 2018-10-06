using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Withdrawals
{
    /// <summary>
    /// Describes a withdrawal creation details
    /// </summary>
    [PublicAPI]
    public class CreateWithdrawalModel
    {
        /// <summary>
        /// The asset id
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// The amount of withdrawal request
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The destination address 
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// ???
        /// </summary>
        public string XrpDestinationTag { get; set; }

        /// <summary>
        /// ???
        /// </summary>
        public bool? SupportBitGo { get; set; }
    }
}
