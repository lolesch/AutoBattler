namespace Code.Data.Enums
{
    public enum ConditionType
    {
        None,              // always fires as payload
        
        Always,
        
        // Pawn
        ResourceBelow,
        ResourceAbove,
        ResourceFull,
        ResourceDepleted,
        HasStatusEffect,

        // Chain
        DamageBelow,
        DamageAbove,
        ResourceGenBelow,
        ResourceGenAbove,
    }
}