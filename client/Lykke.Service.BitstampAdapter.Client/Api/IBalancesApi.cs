using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BitstampAdapter.Client.Models.Balances;
using Refit;

namespace Lykke.Service.BitstampAdapter.Client.Api
{
    /// <summary>
    /// Provides methods to work with balances
    /// </summary>
    [PublicAPI]
    public interface IBalancesApi
    {
        /// <summary>
        /// Returns current balances.
        /// </summary>
        /// <returns>A collection of balances</returns>
        [Get("/api/balances")]
        Task<IReadOnlyCollection<BalanceModel>> GetAsync();
    }
}
