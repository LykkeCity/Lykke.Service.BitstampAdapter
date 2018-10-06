using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BitstampAdapter.Client.Models.Deposits;
using Refit;

namespace Lykke.Service.BitstampAdapter.Client.Api
{
    /// <summary>
    /// Provides methods to work with deposits
    /// </summary>
    [PublicAPI]
    public interface IDepositsApi
    {
        /// <summary>
        /// Returns an address of asset deposit
        /// </summary>
        /// <param name="asset">The asset id.</param>
        /// <returns>An address of asset deposit</returns>
        [Get("/api/deposits/{asset}/address")]
        Task<DepositAddressModel> GetAddressAsync(string asset);

        /// <summary>
        /// Returns a collection of unconfirmed deposits
        /// </summary>
        /// <returns>A collection of unconfirmed deposits</returns>
        [Get("/api/deposits/BTC/unconfirmed")]
        Task<IReadOnlyCollection<DepositModel>> GetUnconfirmedAsync();
    }
}
