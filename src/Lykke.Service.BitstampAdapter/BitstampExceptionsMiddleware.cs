using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.BitstampAdapter
{
    public static class BitstampExceptionsMiddleware
    {
        public static void UseForwardBitstampExceptionsMiddleware(this IApplicationBuilder app)
        {
            app.Use(SetStatusOnError);
        }

        private static async Task SetStatusOnError(HttpContext httpContext, Func<Task> next)
        {
            try
            {
                await next();
            }
            catch (BitstampApiException ex)
            {
                using (var body = new MemoryStream(Encoding.UTF8.GetBytes(ex.Message)))
                {
                    httpContext.Response.ContentType = "text/plain";
                    httpContext.Response.StatusCode = 500;
                    body.CopyTo(httpContext.Response.Body);
                }
            }
        }
    }
}
