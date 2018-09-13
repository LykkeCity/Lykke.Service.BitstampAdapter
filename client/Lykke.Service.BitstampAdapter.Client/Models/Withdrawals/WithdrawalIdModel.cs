using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Withdrawals
{
    /// <summary>
    /// Describes a withdrawal id details
    /// </summary>
    [PublicAPI]
    public class WithdrawalIdModel
    {
        /// <summary>
        /// The identifier of withdrawal
        /// </summary>
        public string Id { get; set; }
    }
}
