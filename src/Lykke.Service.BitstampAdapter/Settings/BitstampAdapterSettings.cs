using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BitstampAdapter.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BitstampAdapterSettings
    {
        public DbSettings Db { get; set; }
    }
}
