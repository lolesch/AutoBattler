namespace Code.Data.Enums
{
    public enum PawnStatType : byte
    {
        None = 0,
        
        MaxLife,
        MaxMana,
        
        RegenLife,
        RegenMana,
        
        // sight radius => implement fog of war
    }
}