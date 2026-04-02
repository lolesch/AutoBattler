namespace Code.Data.Enums
{
    public enum PawnStatType : byte
    {
        None = 0,
        
        LifeMax,
        LifeRegen,
        
        ManaMax,
        ManaRegen,
        
        
        // WEAPON BONI
        // ResourceCostReduction,
        // CooldownReduction,
        // AdditionalDamage, // weapon base damage? TBD
        // Leech?
        
        // Defense Layer TBD
        // Presence <- influences target finding, so tanks have high presence therefore attract enemies.
        // sight radius => implement fog of war
    }
}