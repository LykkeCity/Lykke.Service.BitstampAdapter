using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Withdrawals
{
    /// <summary>
    /// Specifies a withdrawal status
    /// </summary>
    [PublicAPI]
    public enum WithdrawalType
    {
        /// <summary>
        /// Sepa
        /// </summary>
        Sepa = 0,

        /// <summary>
        /// Bitcoin
        /// </summary>
        Bitcoin = 1,

        /// <summary>
        /// Wire transfer
        /// </summary>
        WireTransfer = 2,

        /// <summary>
        /// Xrp
        /// </summary>
        Xrp = 14,

        /// <summary>
        /// Litecoin
        /// </summary>
        Litecoin = 15,

        /// <summary>
        /// Ethereum
        /// </summary>
        Ethereum = 16
    }
}
