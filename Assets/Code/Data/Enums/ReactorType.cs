namespace Code.Data.Enums
{
    public enum ReactorType
    {
        OnSelfHit,    // fires when owning pawn takes damage
        OnManaDeplete,// fires when owning pawn's mana depletes
        OnEnemyDeath, // fires when current target dies
        // TODO: requires coordinator access to other pawns
        OnAllyAttacks,
        OnAllyKills,
        OnNearbyEnemyDies,
        // Deferred: OnAllyAttack, OnCrit, OnPawnEffectApplied, OnAllyChainFire
        // Deferred: Counter-based (every N) — requires Activator pairing, see design notes
    }
}