using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.Service.BitstampAdapter.Client.Api;

namespace Lykke.Service.BitstampAdapter.Client
{
    /// <summary>
    /// Bitstamp adapter service client.
    /// </summary>
    [PublicAPI]
    public class BitstampAdapterServiceClient : IBitstampAdapterServiceClient
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BitstampAdapterServiceClient"/> with <param name="httpClientGenerator"></param>.
        /// </summary>
        public BitstampAdapterServiceClient(IHttpClientGenerator httpClientGenerator)
        {
            Balances = httpClientGenerator.Generate<IBalancesApi>();
            Deposits = httpClientGenerator.Generate<IDepositsApi>();
            Transfers = httpClientGenerator.Generate<ITransfersApi>();
            Withdrawals = httpClientGenerator.Generate<IWithdrawalsApi>();
        }

        public IBalancesApi Balances { get; }

        public IDepositsApi Deposits { get; }

        public ITransfersApi Transfers { get; }

        public IWithdrawalsApi Withdrawals { get; }
    }
}
