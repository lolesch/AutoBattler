using Code.Data.Items.Converter;

namespace Code.Runtime.Inventory
{
    public sealed class ConverterItem : AttachmentItem, IConverterItem
    {
        public ConverterItem(ConverterConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {}
    }

    public interface IConverterItem : ITetrisItem
    {
        
    }
}