using Code.Data.Enums;
using Code.Data.Items.Reactor;

namespace Code.Runtime.Inventory
{
    public sealed class ReactorItem : TetrisItem, IReactorItem
    {
        public ReactorType ReactorType { get; }

        public ReactorItem(ReactorConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            ReactorType = config.ReactorType;
        }

        public override void Use() { }
    }

    public interface IReactorItem : ITetrisItem
    {
        ReactorType ReactorType { get; }
    }
}