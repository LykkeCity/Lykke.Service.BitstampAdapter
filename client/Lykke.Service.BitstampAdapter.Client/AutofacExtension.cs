﻿using System;
using Autofac;
using Common.Log;

namespace Lykke.Service.BitstampAdapter.Client
{
    public static class AutofacExtension
    {
        public static void RegisterBitstampAdapterClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterType<BitstampAdapterClient>()
                .WithParameter("serviceUrl", serviceUrl)
                .As<IBitstampAdapterClient>()
                .SingleInstance();
        }

        public static void RegisterBitstampAdapterClient(this ContainerBuilder builder, BitstampAdapterServiceClientSettings settings, ILog log)
        {
            builder.RegisterBitstampAdapterClient(settings?.ServiceUrl, log);
        }
    }
}