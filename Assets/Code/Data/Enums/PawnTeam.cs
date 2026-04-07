using System;

namespace Code.Data.Enums
{
    [Flags]
    public enum PawnTeam
    {
        None = 0,
        Player = 1 << 0, 
        Enemy  = 1 << 1,
    }
}