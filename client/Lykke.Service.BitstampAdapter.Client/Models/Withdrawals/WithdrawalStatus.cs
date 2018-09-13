using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Withdrawals
{
    /// <summary>
    /// Specifies a withdrawal status
    /// </summary>
    [PublicAPI]
    public enum WithdrawalStatus
    {
        /// <summary>
        /// Opened
        /// </summary>
        Open = 0,

        /// <summary>
        /// In progress
        /// </summary>
        InProcess = 1,

        /// <summary>
        /// Finished
        /// </summary>
        Finished = 2,

        /// <summary>
        /// Canceled
        /// </summary>
        Canceled = 3,

        /// <summary>
        /// Failed
        /// </summary>
        Failed = 4
    }
}
