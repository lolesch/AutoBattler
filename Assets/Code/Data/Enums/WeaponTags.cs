using System;

namespace Code.Data.Enums
{
    [Flags]
    public enum WeaponTags
    {
        None = 0,
 
        // Delivery — how the attack reaches its target
        Melee  = 1 << 0,  // contact range
        Ranged = 1 << 1,  // distance attack
        Aoe    = 1 << 2,  // hex radius spread
        Path   = 1 << 3,  // line from caster to target
        Pierce = 1 << 4,  // continues through targets along a path
        Chain  = 1 << 5,  // bounces between targets
 
        // Interaction — what the weapon does on contact
        Status  = 1 << 6,  // applies any status effect
        Dot     = 1 << 7,  // damage over time (Burn, Poison, Bleed)
        Terrain = 1 << 8,  // creates or modifies terrain
        Control = 1 << 9,  // displacement or CC (push, pull, stun, root)
 
        // Targeting origin
        OriginSelf = 1 << 10,  // effect originates from caster's hex
    }
}