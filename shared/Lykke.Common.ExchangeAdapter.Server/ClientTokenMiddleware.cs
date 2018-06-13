using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Common.ExchangeAdapter.Server
{
    public static class ClientTokenMiddleware
    {
        private const string CredsKey = "api-credentials";

        internal static T RestApi<T>(this Controller controller)
        {
            if (controller.HttpContext.Items.TryGetValue(CredsKey, out var creds))
            {
                return (T) creds;
            }

            return default(T);
        }

        public const string ClientTokenHeader = "X-API-KEY";

        public static void ConfigureSwagger(this SwaggerGenOptions swagger)
        {
            swagger.OperationFilter<AddLykkeAuthorizationHeaderFilter>();
        }

        public static void UseAuthenticationMiddleware<T>(
            this IApplicationBuilder app,
            Func<string, T> createClient)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Headers.TryGetValue(ClientTokenHeader, out var token))
                {
                    context.Items[CredsKey] = createClient(token.FirstOrDefault());
                }
                else
                {
                    context.Items[CredsKey] = null;
                }

                await next();
            });
        }
    }
}
