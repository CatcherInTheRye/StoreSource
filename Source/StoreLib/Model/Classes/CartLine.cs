using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Model.Classes
{
    [Serializable]
    public class CartLine
    {
        public ProductShort Product { get; set; }
        public SkuDetail SKU { get; set; }
        public List<SkuAttr> Attributes { get; set; }
        public int Quantity { get; set; }
        public int InitQuantity { get; set; }
        public int BackorderQty { get; set; }

        public decimal Discount { get; set; }
        public long? DiscountReason_Id { get; set; }

        public bool IsTaxable { get { return Product.IsTaxable; } }
        public decimal Tax { get; set; }

        public long? Coupon_Id { get; set; }
        public long? CouponType_Id { get; set; }
        public decimal CouponDiscount { get; set; }

        public decimal TiersDiscount { get; set; }

        public IdTitle Package { get; set; }

        public int TotalQty
        {
            get { return Quantity + BackorderQty; }
        }

        //TotalPrice
        public decimal TotalPrice
        {
            get { return SKU.FinalPrice * TotalQty; }
        }

        //Shipping
        public decimal TotalShipping
        {
            get { return SKU.IsFreeShipping ? 0 : SKU.Shipping * TotalQty; }
        }

        //Tax
        public decimal TotalTax
        {
            get { return IsTaxable ? Tax * TotalQty : 0; }
        }

        //TotalCost
        public decimal TotalCost
        {
            get { return TotalPrice + TotalShipping + TotalTax; }
        }

        //AmountDue
        public decimal AmountDue
        {
            get { return Math.Max(TotalCost - Discount - CouponDiscount, 0); }
        }

        //Attributes
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
    }
}
