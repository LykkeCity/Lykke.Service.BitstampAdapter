using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Service.BitstampAdapter.Client.Api;
using Lykke.Service.BitstampAdapter.Client.Models.Deposits;
using Lykke.Service.BitstampAdapter.Extensions;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BitstampAdapter.Controllers
{
    [XApiKeyAuth]
    [Route("/api/[controller]")]
    public class DepositsController : Controller, IDepositsApi
    {
        protected ApiClient Api => this.GetRestApi<ApiClient>();

        /// <response code="200">An address of asset deposit</response>
        [HttpGet("{asset}/address")]
        [ProducesResponseType(typeof(DepositAddressModel), (int) HttpStatusCode.OK)]
        public async Task<DepositAddressModel> GetAddressAsync(string asset)
        {
            string address = null;

            if (asset.ToUpper() == "BTC")
                address = await Api.GetBitcoinDepositAddressAsync();
            else if (asset.ToUpper() == "LTC")
                address = await Api.GetLitecoinDepositAddressAsync();
            else if (asset.ToUpper() == "ETH")
                address = await Api.GetEthDepositAddressAsync();
            else if (asset.ToUpper() == "BCH")
                address = await Api.GetBchDepositAddressAsync();
            else if (asset.ToUpper() == "XRP")
                address = await Api.GetXrpDepositAddressAsync();

            if (string.IsNullOrEmpty(address))
                throw new ValidationApiException($"Unknown asset '{asset}'");

            return new DepositAddressModel {Address = address};
        }

        /// <response code="200">A collection of unconfirmed deposits</response>
        [HttpGet("BTC/unconfirmed")]
        [ProducesResponseType(typeof(IReadOnlyCollection<DepositModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<DepositModel>> GetUnconfirmedAsync()
        {
            IReadOnlyCollection<UnconfirmedBitcoinDeposit> deposits =
                await Api.GetUnconfirmedBitcoinDepositsAsync();

            return deposits.Select(o => new DepositModel
                {
                    Address = o.Address,
                    Amount = o.Amount,
                    Confirmations = o.Confirmations
                })
                .ToArray();
        }
    }
}
