using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Transfers
{
    /// <summary>
    /// Describes a transfer status
    /// </summary>
    [PublicAPI]
    public class TransferStatusModel
    {
        /// <summary>
        /// The status of transfer
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The reason of status
        /// </summary>
        public string Reason { get; set; }
    }
}
