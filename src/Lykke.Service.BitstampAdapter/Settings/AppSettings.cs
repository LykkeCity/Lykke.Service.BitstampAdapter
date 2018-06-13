using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.BitstampAdapter.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public BitstampAdapterSettings BitstampAdapterService { get; set; }
    }
}
