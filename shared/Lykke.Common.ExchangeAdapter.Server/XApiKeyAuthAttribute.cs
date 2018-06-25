using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lykke.Common.ExchangeAdapter.Server
{
    public sealed class XApiKeyAuthAttribute : ActionFilterAttribute
    {
        public static IReadOnlyDictionary<string, object> Credentials { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(ClientTokenMiddleware.ClientTokenHeader, out var h)
                || h.Count != 1)
            {
                context.Result = new BadRequestObjectResult(
                    $"Header {ClientTokenMiddleware.ClientTokenHeader} with single value is required");
                return;
            }

            if (!Credentials.TryGetValue(h[0], out var creds))
            {
                context.Result = new BadRequestObjectResult($"Uknown {ClientTokenMiddleware.ClientTokenHeader}");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
