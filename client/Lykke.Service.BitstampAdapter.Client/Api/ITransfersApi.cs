using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BitstampAdapter.Client.Models.Transfers;
using Refit;

namespace Lykke.Service.BitstampAdapter.Client.Api
{
    /// <summary>
    /// Provides methods to work with transfers
    /// </summary>
    [PublicAPI]
    public interface ITransfersApi
    {
        /// <summary>
        /// Creates transfer from a sub-account to the main account.
        /// </summary>
        /// <param name="model">The transfer details</param>
        /// <returns>The status of transfer</returns>
        [Post("/api/transfers/fromSubToMain")]
        Task<TransferStatusModel> FromSubToMainAsync([Body] TransferModel model);

        /// <summary>
        /// Creates transfer from the main account to a sub-account.
        /// </summary>
        /// <param name="model">The transfer details</param>
        /// <returns>The status of transfer</returns>
        [Post("/api/transfers/fromMainToSub")]
        Task<TransferStatusModel> FromMainToSubAsync([Body] TransferModel model);
    }
}
