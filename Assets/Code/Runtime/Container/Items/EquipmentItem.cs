using System;

namespace Code.Runtime.Container.Items
{
    public sealed class EquipmentItem : RectItem, ISlotTypeItem<EquipmentType>
    {
        public EquipmentItem( IItemData itemData, EquipmentType equipmentType ) : base( itemData, StackLimitType.Single )
        {
            SlotType = equipmentType;
        }

        public EquipmentType SlotType { get; }
        
        public override void Use() => throw new NotImplementedException();

        public override void Revert() => throw new NotImplementedException();
    }
}