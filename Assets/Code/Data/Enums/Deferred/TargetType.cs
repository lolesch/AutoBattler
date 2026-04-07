using System;

namespace Code.Data.Enums.Deferred
{
    [Flags]
    public enum TargetType
    {
        None = 0,
        Self = 1 << 0,
        Allies = 1 << 1,
        AlliesAndSelf = Self | Allies,
        Target = 1 << 2,
        AllEnemies = 1 << 3,
    }
}