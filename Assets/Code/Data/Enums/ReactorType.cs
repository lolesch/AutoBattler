namespace Code.Data.Enums
{
    public enum ReactorType : byte
    {
        // see EventType.cs as reference
        OnSelfHit,
        OnManaDeplete,
        OnEnemyDeath,
        
        OnAllyAttacks,
        OnAllyKills,
        OnNearbyEnemyDies,
    }
}