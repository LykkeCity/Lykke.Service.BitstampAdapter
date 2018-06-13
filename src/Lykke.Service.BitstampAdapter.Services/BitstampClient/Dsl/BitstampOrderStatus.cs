using System.Runtime.Serialization;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl
{
    public enum BitstampOrderStatus
    {
        [EnumMember(Value = "Open")] Open,
        [EnumMember(Value = "Queue")] Queue,
        [EnumMember(Value = "Finished")] Finished,
        [EnumMember(Value = "Canceled")] Canceled
    }
}