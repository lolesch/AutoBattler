namespace Code.Data.Enums
{
    public enum ConditionType
    {
        None,
        
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