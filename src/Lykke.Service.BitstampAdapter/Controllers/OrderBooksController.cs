using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Service.BitstampAdapter.Services;

namespace Lykke.Service.BitstampAdapter.Controllers
{
    public class OrderBooksController : OrderBookControllerBase
    {
        protected override OrderBooksSession Session { get; }

        public OrderBooksController(OrderbookPublishingService obService)
        {
            Session = obService.Session;
        }
    }
}
