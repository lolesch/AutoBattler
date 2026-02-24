using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.Container.Items
{
    internal sealed class WeightItem : AbstractItem, IWeightItem
    {
        internal WeightItem( IItemData itemData, StackLimitType stackLimit, float weight ) : base( itemData, stackLimit )
        {
            this.weight = weight;
        }

        public float weight { get; }

        public override void Use() => throw new NotImplementedException();

        public override void Revert() => throw new NotImplementedException();
    }

    public interface IWeightItem
    {
        float weight { get; }
    }
}