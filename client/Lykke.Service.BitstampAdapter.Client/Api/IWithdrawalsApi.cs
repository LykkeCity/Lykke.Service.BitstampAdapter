using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BitstampAdapter.Client.Models.Withdrawals;
using Refit;

namespace Lykke.Service.BitstampAdapter.Client.Api
{
    /// <summary>
    /// Provides methods to work with withdrawals
    /// </summary>
    [PublicAPI]
    public interface IWithdrawalsApi
    {
        /// <summary>
        /// Returns a collection of withdrawals from specified date
        /// </summary>
        /// <param name="dateFrom">The start date of current period</param>
        /// <returns>A collection of withdrawals</returns>
        [Get("/api/withdrawals")]
        Task<IReadOnlyCollection<WithdrawalModel>> GetAsync([Query] DateTime dateFrom);

        /// <summary>
        /// Creates withdrawal
        /// </summary>
        /// <param name="model">The withdrawal details</param>
        /// <returns>The withdrawal id</returns>
        [Post("/api/withdrawals")]
        Task<WithdrawalIdModel> CreateAsync([Body] CreateWithdrawalModel model);
    }
}
