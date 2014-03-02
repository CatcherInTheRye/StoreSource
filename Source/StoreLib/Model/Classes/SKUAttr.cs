using System;

namespace StoreLib.Model.Classes
{
    [Serializable]
    public class SkuAttr
    {
        public int SkuId { get; set; }
        public IdTitle AttributeType { get; set; }
        public IdTitle AttributeValue { get; set; }
    }
}
