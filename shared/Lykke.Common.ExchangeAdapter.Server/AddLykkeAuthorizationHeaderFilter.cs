using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Common.ExchangeAdapter.Server
{
    public sealed class AddLykkeAuthorizationHeaderFilter : IOperationFilter
    {
        private static readonly IParameter[] Params = {
            new NonBodyParameter
            {
                Name = ClientTokenMiddleware.ClientTokenHeader,
                In = "header",
                Description = "Lykke authentication token",
                Required = true,
                Type = "string"
            }
        };

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is XApiKeyAuthAttribute))
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = Params;
                }
                else
                {
                    foreach (var p in Params)
                    {
                        operation.Parameters.Add(p);
                    }
                }
            }
        }
    }
}
