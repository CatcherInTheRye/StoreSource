using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Model.Classes
{
    [Serializable]
    public class SkuDetail
    {
        public IdTitle IdTitle { get; set; }
        public ProductShort Product { get; set; }
        public bool IsActive { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int MinOrderQty { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal MSRP { get; set; }
        public bool IsFreeShipping { get; set; }
        public decimal Shipping { get; set; }
        public decimal WeightLBS { get; set; }
        public decimal WeightOZ { get; set; }
        public string Model { get; set; }
        public string ManufactModel { get; set; }
        public int DisplayOrder { get; set; }
        public int Priority { get; set; }
        public string Thumbnail { get; set; }

        public LazyList<SkuAttr> Attributes { get; set; }

        //FinalPrice
        public decimal FinalPrice
        {
            get { return SalesPrice > 0 ? SalesPrice : Price; }
        }

        //SavedAmount
        public decimal SavedAmount
        {
            get { return MSRP > FinalPrice ? MSRP - FinalPrice : (SalesPrice > 0 ? Price - SalesPrice : 0); }
        }

        //SavedPercent
        public decimal SavedPercent
        {
            get { return MSRP > FinalPrice ? 100 - FinalPrice / MSRP * 100 : (SalesPrice > 0 ? Math.Round(100 - SalesPrice / Price * 100, 0) : 0); }
        }

        //Attributes
        public string AttributesLine
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                Attributes.ToList().ForEach(a => sb.AppendFormat("{0}: {1} |", a.AttributeType.Title.ToUpper(), a.AttributeValue.Title));
                if (sb.Length > 0) sb.Remove(sb.Length - 2, 2);
                return sb.ToString();
            }
        }
    }
}
