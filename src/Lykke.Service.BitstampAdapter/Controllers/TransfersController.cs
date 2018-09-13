using System;
using System.Threading.Tasks;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Microsoft.AspNetCore.Mvc;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Service.BitstampAdapter.Client.Api;
using Lykke.Service.BitstampAdapter.Client.Models.Transfers;
using Lykke.Service.BitstampAdapter.Extensions;

namespace Lykke.Service.BitstampAdapter.Controllers
{
    [XApiKeyAuth]
    [Route("/api/[controller]")]
    public class TransfersController : Controller, ITransfersApi
    {
        private readonly ApiClient _api;

        public TransfersController()
        {
            _api = this.GetRestApi<ApiClient>();
        }

        [HttpPost("fromSubToMain")]
        public async Task<TransferStatusModel> FromSubToMainAsync([FromBody] TransferModel model)
        {
            if (string.IsNullOrEmpty(model.Currency))
                throw new Exception("Currency is required");

            if (model.Amount <=0 )
                throw new Exception("Amount should greater than zero");

            var res = await _api.TransferSubToMainAsync(model.Account, model.Amount, model.Currency);

            return new TransferStatusModel
            {
                Reason = res.Reason,
                Status = res.Status
            };
        }

        [HttpPost("fromMainToSub")]
        public async Task<TransferStatusModel> FromMainToSubAsync([FromBody] TransferModel model)
        {
            if (string.IsNullOrEmpty(model.Account))
                throw new Exception("Request.SubAccount cannot be null");

            if (string.IsNullOrEmpty(model.Currency))
                throw new Exception("Request.Currency cannot be null");

            if (model.Amount <= 0)
                throw new Exception("Request.Amount must be more zero");

            var res = await _api.TransferMainToSubAsync(model.Account, model.Amount, model.Currency);

            return new TransferStatusModel
            {
                Reason = res.Reason,
                Status = res.Status
            };
        }
    }
}
