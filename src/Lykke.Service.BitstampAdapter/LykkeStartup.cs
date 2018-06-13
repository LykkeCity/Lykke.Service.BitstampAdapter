using System;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.MonitoringServiceApiCaller;
using Lykke.Sdk;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.BitstampAdapter
{
    public abstract class LykkeStartup<TSettings> where TSettings : BaseAppSettings
    {
        protected abstract void ConfigureImpl(IApplicationBuilder app);
        protected abstract void BuildServiceProvilder(LykkeServiceOptions<TSettings> options);

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<TSettings>(
                BuildServiceProvilder,
                swagger => swagger.ConfigureSwagger());
        }

        public void Configure(IApplicationBuilder app)
        {
            var options = new LykkeConfigurationOptions();

            var env = app.ApplicationServices.GetService<IHostingEnvironment>();

            var appName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            app.UseLykkeMiddleware(appName, options.DefaultErrorHandler);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var log = app.ApplicationServices.GetService<ILog>();

            try
            {
                var appLifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
                var configurationRoot = app.ApplicationServices.GetService<IConfigurationRoot>();

                if (configurationRoot == null)
                    throw new ApplicationException("Configuration root must be registered in the container");

                var monitoringSettings = app.ApplicationServices.GetService<IReloadingManager<MonitoringServiceClientSettings>>();

                var startupManager = app.ApplicationServices.GetService<IStartupManager>();
                var shutdownManager = app.ApplicationServices.GetService<IShutdownManager>();
                var hostingEnvironment = app.ApplicationServices.GetService<IHostingEnvironment>();

                appLifetime.ApplicationStarted.Register(() =>
                {
                    try
                    {
                        startupManager?.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                        log.WriteMonitor("StartApplication", null, "Application started");

                        if (!hostingEnvironment.IsDevelopment())
                        {
                            if (monitoringSettings?.CurrentValue == null)
                                throw new ApplicationException("Monitoring settings is not provided.");

                            AutoRegistrationInMonitoring.RegisterAsync(configurationRoot, monitoringSettings.CurrentValue.MonitoringServiceUrl, log).GetAwaiter().GetResult();
                        }

                    }
                    catch (Exception ex)
                    {
                        log.WriteFatalError("StartApplication", "", ex);
                        throw;
                    }
                });

                appLifetime.ApplicationStopping.Register(() =>
                {
                    try
                    {
                        shutdownManager?.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        log?.WriteFatalError("StopApplication", "", ex);

                        throw;
                    }
                });

                app.UseLykkeForwardedHeaders();

                ConfigureImpl(app);

                app.UseStaticFiles();
                app.UseMvc();

                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
            }
            catch (Exception ex)
            {
                log?.WriteFatalError("Startup", "", ex);
                throw;
            }
        }
    }
}
