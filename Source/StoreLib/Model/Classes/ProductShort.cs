using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Model.Classes
{
    [Serializable]
    public class ProductShort
    {
        public IdTitle Product { get; set; }
        public IdTitle SKU { get; set; }
        public string ThumbnailImage { get; set; }
        public string MediumImage { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal Price { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal MSRP { get; set; }
        public DateTime DateIn { get; set; }
        public string FullCategoryTitle { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public int Quantity { get; set; }
        public int Priority { get; set; }
        public bool Separator { get; set; }
        public string Filters { get; set; }
        public string AttributeImages { get; set; }
        public bool IsTaxable { get; set; }
        public bool TaxByShipping { get; set; }
        public int? iCustom1 { get; set; }
        public List<SkuAttr> Attributes { get; set; }

        public string AttributesLine
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                Attributes.ForEach(a => sb.AppendFormat("{0}: {1} |", a.AttributeType.Title.ToUpper(), a.AttributeValue.Title));
                if (sb.Length > 0) sb.Remove(sb.Length - 2, 2);
                return sb.ToString();
            }
        }

        public List<ProductAttributeImages> AttributeImagesList
        {
            get
            {
                List<ProductAttributeImages> result = new List<ProductAttributeImages>();
                if (String.IsNullOrEmpty(AttributeImages)) return result;
                foreach (string attributeImage in AttributeImages.Split('|'))
                {
                    List<string> i = attributeImage.Split(':').ToList();
                    if (i.Count != 4) continue;
                    result.Add(new ProductAttributeImages { Attribute_Id = long.Parse(i[0]), AttributeValue = i[1], AttributeImage = i[2], ProductDefaultImage = i[3] });
                }
                return result;
            }
        }

        public Dictionary<long, List<long>> FiltersList
        {
            get
            {
                Dictionary<long, List<long>> result = new Dictionary<long, List<long>>();
                if (String.IsNullOrEmpty(Filters)) return result;
                foreach (string filter in Filters.Split('|'))
                {
                    List<string> i = filter.Split('_').ToList();
                    if (i.Count != 2) continue;
                    long attributetype_Id = long.Parse(i[0]);
                    long attributevalue_Id = long.Parse(i[1]);
                    if (!result.ContainsKey(attributetype_Id))
                    {
                        result.Add(attributetype_Id, new List<long> { attributevalue_Id });
                        continue;
                    }
                    List<long> t = result[attributetype_Id];
                    t.Add(attributevalue_Id);
                    result[attributetype_Id] = t;
                }
                return result;
            }
        }

        public decimal FinalPrice
        {
            get { return SalesPrice > 0 ? SalesPrice : Price; }
        }

        public decimal SavedAmount
        {
            get { return MSRP > FinalPrice ? MSRP - FinalPrice : (SalesPrice > 0 ? Price - SalesPrice : 0); }
        }

        public decimal SavedPercent
        {
            get { return MSRP > FinalPrice ? 100 - FinalPrice / MSRP * 100 : (SalesPrice > 0 ? 100 - SalesPrice / Price * 100 : 0); }
        }
    }
}
