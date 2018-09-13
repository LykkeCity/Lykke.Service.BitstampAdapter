using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ApiClient _api;

        public DepositsController()
        {
            _api = this.GetRestApi<ApiClient>();
        }

        [HttpGet("{asset}/address")]
        public async Task<DepositAddressModel> GetAddressAsync(string asset)
        {
            string address = null;

            if (asset.ToUpper() == "BTC")
                address = await _api.GetBitcoinDepositAddressAsync();
            else if (asset.ToUpper() == "LTC")
                address = await _api.GetLitecoinDepositAddressAsync();
            else if (asset.ToUpper() == "ETH")
                address = await _api.GetEthDepositAddressAsync();
            else if (asset.ToUpper() == "BCH")
                address = await _api.GetBchDepositAddressAsync();
            else if (asset.ToUpper() == "XRP")
                address = await _api.GetXrpDepositAddressAsync();

            if (string.IsNullOrEmpty(address))
                throw new Exception($"Unknown asset '{asset}'");

            return new DepositAddressModel {Address = address};
        }

        [HttpGet("BTC/unconfirmed")]
        public async Task<IReadOnlyCollection<DepositModel>> GetUnconfirmedAsync()
        {
            IReadOnlyCollection<UnconfirmedBitcoinDeposit> deposits =
                await _api.GetUnconfirmedBitcoinDepositsAsync();

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
