using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.SpotController;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Common.ExchangeAdapter.Server
{
    [XApiKeyAuth]
    [Route("spot")]
    public abstract class SpotControllerBase<TExchangeClient> : Controller, ISpotController
    {
        protected TExchangeClient Api => this.RestApi<TExchangeClient>();

        [HttpGet("getWallets")]
        public virtual Task<GetWalletsResponse> GetWalletBalancesAsync()
        {
            throw new System.NotImplementedException();
        }

        [HttpGet("GetLimitOrders")]
        public virtual Task<GetLimitOrdersResponse> GetLimitOrdersAsync()
        {
            throw new System.NotImplementedException();
        }

        [HttpGet("limitOrderStatus")]
        public virtual Task<OrderModel> LimitOrderStatusAsync(string orderId)
        {
            throw new System.NotImplementedException();
        }

        [HttpGet("marketOrderStatus")]
        public virtual Task<OrderModel> MarketOrderStatusAsync(string orderId)
        {
            throw new System.NotImplementedException();
        }

        [HttpPost("createLimitOrder")]
        public virtual Task<OrderIdResponse> CreateLimitOrderAsync([FromBody]LimitOrderRequest request)
        {
            throw new System.NotImplementedException();
        }

        [HttpPost("cancelOrder")]
        public virtual Task<CancelLimitOrderResponse> CancelLimitOrderAsync([FromBody]CancelLimitOrderRequest request)
        {
            throw new System.NotImplementedException();
        }

        [HttpPost("replaceLimitOrder")]
        public virtual Task<OrderIdResponse> ReplaceLimitOrderAsync([FromBody]ReplaceLimitOrderRequest request)
        {
            throw new System.NotImplementedException();
        }

        [HttpPost("createMarketOrder")]
        public virtual Task<OrderIdResponse> CreateMarketOrderAsync([FromBody]MarketOrderRequest request)
        {
            throw new System.NotImplementedException();
        }

        [HttpGet("getOrdersHistory")]
        public virtual Task<GetOrdersHistoryResponse> GetOrdersHistoryAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
