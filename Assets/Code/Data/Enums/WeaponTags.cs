using System;

namespace Code.Data.Enums
{
    [Flags]
    public enum WeaponTags
    {
        None = 0,

        // Delivery
        Instant = 1 << 0,
        Path = 1 << 1,
        Aoe = 1 << 2,
        Entity = 1 << 3,

        // Target
        OriginSelf = 1 << 11,
        OriginTarget = 1 << 12,
    }
}