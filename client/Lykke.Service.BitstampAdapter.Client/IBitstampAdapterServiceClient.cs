using JetBrains.Annotations;
using Lykke.Service.BitstampAdapter.Client.Api;

namespace Lykke.Service.BitstampAdapter.Client
{
    /// <summary>
    /// Bitstamp adapter service client.
    /// </summary>
    [PublicAPI]
    public interface IBitstampAdapterServiceClient
    {
        /// <summary>
        /// The balances api
        /// </summary>
        IBalancesApi Balances { get; }

        /// <summary>
        /// The deposits api
        /// </summary>
        IDepositsApi Deposits { get; }

        /// <summary>
        /// The transfers api
        /// </summary>
        ITransfersApi Transfers { get; }

        /// <summary>
        /// The withdrawal api
        /// </summary>
        IWithdrawalsApi Withdrawals { get; }
    }
}
