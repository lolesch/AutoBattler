namespace Code.Data.Enums
{
    public enum PayloadConditionType
    {
        None,              // always fires as payload

        // Owning unit state
        HealthBelow,       // owning unit HP% < threshold
        HealthAbove,       // owning unit HP% > threshold
        ResourceFull,      // owning unit resource is full
        ResourceBelow,     // owning unit resource% < threshold
        ResourceAbove,     // owning unit resource% > threshold

        // Root weapon outcome (this chain pass)
        RootDamageAbove,   // root weapon dealt >= threshold damage this hit
        RootKilledTarget,  // root weapon killed the target

        // Target state
        TargetHealthBelow,    // target HP% < threshold — execute condition
        TargetHealthAbove,    // target HP% > threshold — opening condition
        TargetHasStatusEffect, // stub — always false until status system exists
    }
}