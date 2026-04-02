using Code.Data.Enums;
using Code.Data.Items.Reactor;

namespace Code.Runtime.Inventory
{
    public sealed class ReactorItem : AttachmentItem, IReactorItem
    {
        public ReactorType    ReactorType        { get; }
        public ConditionType  ConditionType      { get; }
        public float          ConditionThreshold { get; }

        public ReactorItem(ReactorConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            ReactorType        = config.ReactorType;
            ConditionType      = config.ConditionType;
            ConditionThreshold = config.ConditionThreshold;
        }
    }

    public interface IReactorItem : ITetrisItem
    {
        ReactorType   ReactorType        { get; }
        ConditionType ConditionType      { get; }
        float         ConditionThreshold { get; }
    }
}