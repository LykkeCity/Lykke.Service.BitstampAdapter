﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Lykke.Service.BitstampAdapter.Services.Settings;

namespace Lykke.Service.BitstampAdapter.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class BitstampAdapterSettings
    {
        public DbSettings Db { get; set; }
        public OrderbookSettings Orderbooks { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public InstrumentSettings Instruments { get; set; }
        public IReadOnlyCollection<ApiCredentials> Clients { get; set; }
    }

}
