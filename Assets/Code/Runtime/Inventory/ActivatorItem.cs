using Code.Data.Enums;
using Code.Data.Items.Activator;

namespace Code.Runtime.Inventory
{
    public sealed class ActivatorItem : TetrisItem, IActivatorItem
    {
        public ActivatorType ActivatorType      { get; }
        public float         CooldownMultiplier { get; }

        public ActivatorItem(ActivatorConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            ActivatorType      = config.ActivatorType;
            CooldownMultiplier = config.CooldownMultiplier;
        }

        public override void Use() { }
    }
    
    public interface IActivatorItem : ITetrisItem
    {
        ActivatorType ActivatorType      { get; }
        float         CooldownMultiplier { get; }
    }
}