using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Server.Fails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Lykke.Common.ExchangeAdapter.Server
{
    public static class HandleBusinessExceptionsMiddleware
    {
        public static void UseHandleBusinessExceptionsMiddleware(this IApplicationBuilder app)
        {
            app.Use(SetStatusOnBusinessError);
        }

        public static async Task SetStatusOnBusinessError(HttpContext httpContext, Func<Task> next)
        {
            try
            {
                await next();
            }
            catch (OrderNotFoundException)
            {
                MakeBadRequest(httpContext, "notFound");
            }
            catch (VolumeTooSmallException ex)
            {
                MakeBadRequest(httpContext, $"volumeTooSmall: {ex.Message}");
            }
            catch (InvalidOrderPriceException ex)
            {
                MakeBadRequest(httpContext, $"priceError: {ex.Message}");
            }
            catch (InvalidOrderIdException)
            {
                MakeBadRequest(httpContext, "orderIdFormat");
            }
            catch (InsufficientBalanceException)
            {
                MakeBadRequest(httpContext, "notEnoughBalance");
            }
            catch (InvalidInstrumentException)
            {
                MakeBadRequest(httpContext, "instrumentIsNotSupported");
            }
            catch (NotImplementedException)
            {
                MakeBadRequest(httpContext, "notImplemented", HttpStatusCode.NotImplemented);
            }
        }

        private static void MakeBadRequest(
            HttpContext httpContext,
            string error,
            HttpStatusCode code = HttpStatusCode.BadRequest)
        {
            using (var body = new MemoryStream(Encoding.UTF8.GetBytes(error)))
            {
                httpContext.Response.ContentType = "text/plain";
                httpContext.Response.StatusCode = (int)code;
                body.CopyTo(httpContext.Response.Body);
            }
        }
    }
}
