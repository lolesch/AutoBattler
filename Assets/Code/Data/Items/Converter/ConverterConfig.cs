using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Converter
{
    [CreateAssetMenu(fileName = "ConverterConfig", menuName = Const.ItemConfig + "Converter")]
    public sealed class ConverterConfig : AttachmentItemConfig, IConverterConfig
    {
        /* "LifeLink' convert manaCost into healthCost
         * sourceStatType -> converted to -> targetStatType
         * and then define the conversion like:
         * - convert 2 manaCost into 1 healthCost
         * - convert damage type physical into fire
         * so do all converters have a conversion rate? If it is just a type, then just convert.
         * So the conversionRate converter might be a special case. for now just convert manaCost to HealthCost
         */ 
        [field: Header("Chained")]
        [field: SerializeField] public UsageStatType from { get; private set; }
        [field: SerializeField] public UsageStatType to { get; private set; }
      
        public override int MaxConnectors => 2;
    }

    public interface IConverterConfig
    {
    }
}