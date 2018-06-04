using System;
using Autofac;
using Lykke.Sdk;
using Lykke.Service.BitstampAdapter.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.BitstampAdapter
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {                                   
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                // options.ApiVersion = "v1";
                options.ApiTitle = "BitstampAdapter API";
                // options.LogsConnectionStringFactory = ctx => ctx.Resolve<IReloadingManager<AppSettings>>().ConnectionString(x => x.BitstampAdapterService.Db.LogsConnString);
                options.LogsConnectionStringFactory = ctx => ctx.Nested(x => x.BitstampAdapterService.Db.LogsConnString);
                options.LogsTableName = "BitstampAdapterLog";
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration();
        }
    }
}
