using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Infrastructure;

namespace Lykke.Service.BitstampAdapter.Client
{
    /// <summary>
    /// Autofac extension for service client.
    /// </summary>
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        /// Registers <see cref="IBitstampAdapterServiceClient"/> in Autofac container using <see cref="BitstampAdapterServiceClientSettings"/>.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">MarketMakerReports client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="HttpClientGeneratorBuilder"/> configure handler.</param>
        public static void RegisterMarketMakerReportsClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] BitstampAdapterServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.",
                    nameof(BitstampAdapterServiceClientSettings.ServiceUrl));

            HttpClientGeneratorBuilder clientBuilder = HttpClientGenerator.HttpClientGenerator
                .BuildForUrl(settings.ServiceUrl)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper())
                .WithAdditionalDelegatingHandler(new ApiKeyHeaderHttpClientHandler("X-API-KEY", settings.ApiKey));

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder.WithoutRetries();

            builder.RegisterInstance(new BitstampAdapterServiceClient(clientBuilder.Create()))
                .As<IBitstampAdapterServiceClient>()
                .SingleInstance();
        }
    }
}
