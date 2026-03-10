namespace Code.Data.Enums
{
    public enum PayloadConditionType
    {
        OnHit,
        OnKill,
        HealthBelow,   // fires if target HP% is below PayloadConditionThreshold at time of hit
        ResourceFull,  // fires if firing pawn's resource is full at time of hit
    }
}