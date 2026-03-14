namespace Code.Data.Enums
{
    public enum ActivatorType
    {
        ModifyCooldown,   // fires at weapon speed * CooldownMultiplier
        FireWhenManaFull, // fires when owning pawn's mana is full, then depletes it
        // Deferred: burst patterns, counter-based (see design notes — needs Reactor pairing)
    }
}