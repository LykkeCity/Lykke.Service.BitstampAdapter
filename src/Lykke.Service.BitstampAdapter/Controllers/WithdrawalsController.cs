using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
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
        private readonly ILog _log;

        public WithdrawalsController(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        protected ApiClient Api => this.GetRestApi<ApiClient>();

        /// <response code="200">A collection of withdrawals</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<WithdrawalModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<WithdrawalModel>> GetAsync(DateTime dateFrom)
        {
            IReadOnlyCollection<Withdrawal> withdrawals = await Api.GetWithdrawalRequestsAsync(dateFrom);

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

        /// <response code="200">A identifier of the withdrawal</response>
        [HttpPost]
        [ProducesResponseType(typeof(WithdrawalIdModel), (int) HttpStatusCode.OK)]
        public async Task<WithdrawalIdModel> CreateAsync([FromBody] CreateWithdrawalModel model)
        {
            WithdrawalId withdrawalId = null;

            try
            {
                if (model.Asset.ToUpper() == "BTC")
                    withdrawalId = await Api.CreateBitcoinWithdrawalAsync(model.Amount, model.Address,
                        model.SupportBitGo ?? false);
                else if (model.Asset.ToUpper() == "ETH")
                    withdrawalId = await Api.CreateEthWithdrawalAsync(model.Amount, model.Address);
                else if (model.Asset.ToUpper() == "LTC")
                    withdrawalId = await Api.CreateLitecoinWithdrawalAsync(model.Amount, model.Address);
                else if (model.Asset.ToUpper() == "BCH")
                    withdrawalId = await Api.CreateBchWithdrawalAsync(model.Amount, model.Address);
                else if (model.Asset.ToUpper() == "XRP")
                    withdrawalId = await Api.CreateXrpWithdrawalAsync(model.Amount, model.Address,
                        model.XrpDestinationTag);
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, model);
                throw;
            }

            if (withdrawalId == null)
                throw new ValidationApiException($"Asset '{model.Asset}' not supported");

            return new WithdrawalIdModel {Id = withdrawalId.Id};
        }
    }
}
