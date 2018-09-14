using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.BitstampAdapter.Client.Api;
using Lykke.Service.BitstampAdapter.Client.Models.Balances;
using Lykke.Service.BitstampAdapter.Extensions;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BitstampAdapter.Controllers
{
    [XApiKeyAuth]
    [Route("/api/[controller]")]
    public class BalancesController : Controller, IBalancesApi
    {
        protected ApiClient Api => this.GetRestApi<ApiClient>();

        [HttpGet]
        public async Task<IReadOnlyCollection<BalanceModel>> GetAsync()
        {
            IReadOnlyCollection<WalletBalanceModel> balances = await Api.GetBalanceAsync();

            return balances.Select(o => new BalanceModel
                {
                    Asset = o.Asset,
                    Balance = o.Balance,
                    Reserved = o.Reserved
                })
                .ToArray();
        }
    }
}
