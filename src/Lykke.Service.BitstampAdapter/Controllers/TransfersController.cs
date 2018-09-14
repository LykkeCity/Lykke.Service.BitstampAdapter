using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Microsoft.AspNetCore.Mvc;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
using Lykke.Service.BitstampAdapter.Client.Api;
using Lykke.Service.BitstampAdapter.Client.Models.Transfers;
using Lykke.Service.BitstampAdapter.Extensions;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer;

namespace Lykke.Service.BitstampAdapter.Controllers
{
    [XApiKeyAuth]
    [Route("/api/[controller]")]
    public class TransfersController : Controller, ITransfersApi
    {
        private readonly ILog _log;

        public TransfersController(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        protected ApiClient Api => this.GetRestApi<ApiClient>();

        /// <response code="200">The status of transfer</response>
        [HttpPost("fromSubToMain")]
        [ProducesResponseType(typeof(TransferStatusModel), (int) HttpStatusCode.OK)]
        public async Task<TransferStatusModel> FromSubToMainAsync([FromBody] TransferModel model)
        {
            TransferResult transferResult;

            try
            {
                transferResult = await Api.TransferSubToMainAsync(model.Account, model.Amount, model.Currency);
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, model);
                throw;
            }

            return new TransferStatusModel
            {
                Reason = transferResult.Reason,
                Status = transferResult.Status
            };
        }

        /// <response code="200">The status of transfer</response>
        [HttpPost("fromMainToSub")]
        [ProducesResponseType(typeof(TransferStatusModel), (int) HttpStatusCode.OK)]
        public async Task<TransferStatusModel> FromMainToSubAsync([FromBody] TransferModel model)
        {
            TransferResult transferResult;

            try
            {
                transferResult = await Api.TransferMainToSubAsync(model.Account, model.Amount, model.Currency);
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, model);
                throw;
            }

            return new TransferStatusModel
            {
                Reason = transferResult.Reason,
                Status = transferResult.Status
            };
        }
    }
}
