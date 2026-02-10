using System.ComponentModel;

namespace Code.Runtime.Statistics
{
    public enum ModifierType : byte
    {
        // WORDING
        // - Overwrite   => absolute, explicit, fix
        // - FlatAdd     => additional, additive, bonus, 
        // - PercentAdd  => more/less
        // - PercentMult => multiplicative
        
        // OTHER TYPES:
        // - OverwriteBase
        // - OverwriteRange

        /// Values are the order the modifiers are applied
        [Description( "Sets the stat to a fixed value" )]
        Overwrite = 0,

        [Description( "Adds a flat value to the stat" )]
        FlatAdd = 1,

        [Description( "Adds a percentage to the stat" )]
        PercentAdd = 2,

        [Description( "Multiplies the total by a percentage" )]
        PercentMult = 3,
    }
}