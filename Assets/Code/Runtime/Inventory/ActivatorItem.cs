using Code.Data.Enums;
using Code.Data.Items.Activator;

namespace Code.Runtime.Inventory
{
    /// <summary>
    /// Applies a conditional stat modifier to the attached weapon's AttackSpeed or ResourceCost.
    /// The weapon always fires — the Activator modifies how expensive or fast that firing is
    /// while its condition holds.
    /// </summary>
    public sealed class ActivatorItem : TetrisItem, IActivatorItem
    {
        public FiringStatType         WeaponStat          { get; }
        public float                  Value               { get; }
        public ModifierType           ModifierType        { get; }
        public ActivatorConditionType ConditionType       { get; }
        public float                  ConditionThreshold  { get; }

        public ActivatorItem(ActivatorConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            WeaponStat         = config.WeaponStat;
            Value              = config.Value;
            ModifierType       = config.ModifierType;
            ConditionType      = config.ConditionType;
            ConditionThreshold = config.ConditionThreshold;
        }

        public override void Use() { }
    }

    public interface IActivatorItem : ITetrisItem
    {
        FiringStatType         WeaponStat         { get; }
        float                  Value              { get; }
        ModifierType           ModifierType       { get; }
        ActivatorConditionType ConditionType      { get; }
        float                  ConditionThreshold { get; }
    }
}