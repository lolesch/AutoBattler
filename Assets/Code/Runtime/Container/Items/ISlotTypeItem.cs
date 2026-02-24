using System;

namespace Code.Runtime.Container.Items
{
    public interface ISlotTypeItem<out T> where T : Enum
    {
        T SlotType { get; }
    }
}