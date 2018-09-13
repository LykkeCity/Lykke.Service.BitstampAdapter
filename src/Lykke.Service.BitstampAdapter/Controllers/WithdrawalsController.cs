using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Service.BitstampAdapter.Client.Api;
using Lykke.Service.BitstampAdapter.Client.Models.Withdrawals;
using Lykke.Service.BitstampAdapter.Extensions;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BitstampAdapter.Controllers
{
    [XApiKeyAuth]
    [Route("/api/[controller]")]
    public class WithdrawalsController : Controller, IWithdrawalsApi
    {
        private readonly ApiClient _api;

        public WithdrawalsController()
        {
            _api = this.GetRestApi<ApiClient>();
        }

        [HttpGet("BTC")]
        public async Task<IReadOnlyCollection<WithdrawalModel>> GetAsync(DateTime dateFrom)
        {
            IReadOnlyCollection<Withdrawal> withdrawals = await _api.GetWithdrawalRequestsAsync(dateFrom);

            return withdrawals.Select(o =>
                new WithdrawalModel
                {
                    Id = o.Id,
                    Datetime = o.Datetime,
                    Type = (WithdrawalType) o.Type,
                    Currency = o.Currency,
                    Amount = o.Amount,
                    Status = (WithdrawalStatus) o.Status,
                    Address = o.Address,
                    TransactionId = o.TransactionId
                }).ToArray();
        }

        [HttpPost]
        public async Task<WithdrawalIdModel> CreateAsync([FromBody] CreateWithdrawalModel model)
        {
            if (string.IsNullOrEmpty(model.Address))
                throw new Exception("Address is required");

            if (model.Amount <= 0)
                throw new Exception("Amount should be greater than zero");

            WithdrawalId withdrawalId = null;

            if (model.Asset.ToUpper() == "BTC")
                withdrawalId = await _api.CreateBitcoinWithdrawalAsync(model.Amount, model.Address,
                    model.SupportBitGo ?? false);
            else if (model.Asset.ToUpper() == "ETH")
                withdrawalId = await _api.CreateEthWithdrawalAsync(model.Amount, model.Address);
            else if (model.Asset.ToUpper() == "LTC")
                withdrawalId = await _api.CreateLitecoinWithdrawalAsync(model.Amount, model.Address);
            else if (model.Asset.ToUpper() == "BCH")
                withdrawalId = await _api.CreateBchWithdrawalAsync(model.Amount, model.Address);
            else if (model.Asset.ToUpper() == "XRP")
                withdrawalId = await _api.CreateXrpWithdrawalAsync(model.Amount, model.Address,
                    model.XrpDestinationTag);
            
            if (withdrawalId == null)
                throw new Exception($"Asset '{model.Asset}' not supported");

            return new WithdrawalIdModel {Id = withdrawalId.Id};
        }
    }
}
