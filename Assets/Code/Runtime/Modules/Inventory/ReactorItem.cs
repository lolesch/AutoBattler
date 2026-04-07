using Code.Data.Enums;
using Code.Data.Items.Reactor;
using Code.Runtime.Modules.Statistics;

namespace Code.Runtime.Modules.Inventory
{
    public sealed class ReactorItem : AttachmentItem, IReactorItem
    {
        public ReactorType    ReactorType        { get; }
        public WeaponInputModifier inputMod { get; }

        public ReactorItem(ReactorConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            ReactorType        = config.reactorType;
            
            inputMod = new WeaponInputModifier(
                config.inputStatMod.stat,
                new Modifier(config.inputStatMod.value, config.inputStatMod.type, Guid));
        }
    }

    public interface IReactorItem : ITetrisItem
    {
        ReactorType   ReactorType        { get; }
        WeaponInputModifier inputMod { get; }
    }
}