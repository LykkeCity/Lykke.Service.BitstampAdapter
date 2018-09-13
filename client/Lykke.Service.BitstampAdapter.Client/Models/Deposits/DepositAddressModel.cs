using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Deposits
{
    /// <summary>
    /// Describes a deposit address
    /// </summary>
    [PublicAPI]
    public class DepositAddressModel
    {
        /// <summary>
        /// The deposit address
        /// </summary>
        public string Address { get; set; }
    }
}
