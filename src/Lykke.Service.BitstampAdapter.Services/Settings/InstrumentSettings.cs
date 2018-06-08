using System.Collections.Generic;

namespace Lykke.Service.BitstampAdapter.Services.Settings
{
    public sealed class InstrumentSettings
    {
        public IReadOnlyCollection<string> Orderbooks { get; set; }
    }
}