using Code.Data.Items;
using Code.Data.Enums;

namespace Code.Runtime.Inventory
{
    public sealed class ConverterItem : TetrisItem, IConverterItem
    {
        public ConverterItem(ItemConfig config, RotationType rotation) : base(config, rotation)
        {
        }
    }

    public interface IConverterItem : ITetrisItem
    {
        
    }
}