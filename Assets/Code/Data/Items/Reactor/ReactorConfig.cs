using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Reactor
{
    [CreateAssetMenu(fileName = "ReactorConfig", menuName = "Configs/Items/Reactor")]
    public sealed class ReactorConfig : ItemConfig
    {
        [Header("Reactor Properties")]
        [field: SerializeField] public ReactorType ReactorType { get; private set; }

        protected override int MaxConnectors => 1;
    }
}