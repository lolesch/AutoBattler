using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Reactor
{
    [CreateAssetMenu(fileName = "ReactorConfig", menuName = Const.ItemConfig + "Reactor")]
    public sealed class ReactorConfig : AttachmentItemConfig
    {
        [field: Header("Chain — Event")]
        [field: SerializeField] public ReactorType             reactorType        { get; private set; }
        [field: SerializeField] public WeaponInputStatModConfig inputStatMod { get; private set; }
        
        ////TODO: rework conditions!
        //[field: Header("Chain — Condition")]
        //[field: SerializeField] public ConditionType ConditionType      { get; private set; }
        //[field: SerializeField] public float         ConditionThreshold { get; private set; }

        public override int MaxConnectors => 1;
    }
}