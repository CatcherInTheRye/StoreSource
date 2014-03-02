using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Model.Classes
{
    [Serializable]
    public class PackageCart
    {
        public IdTitle IdTitle { get; set; }
        public ImageDetail Image { get; set; }
        public List<PackageCartLine> Products { get; set; }
        public int InitQuantity { get; set; }
        public int Quantity { get { return Math.Min(Products.Min(p => p.MaxPackageCount), InitQuantity); } }

        //Price
        public decimal Price
        {
            get { return Products.Sum(p => p.TotalPrice); }
        }

        //TotalPrice
        public decimal TotalPrice
        {
            get { return Products.Sum(p => p.TotalPrice) * Quantity; }
        }

        //Shipping
        public decimal TotalShipping
        {
            get { return Products.Sum(p => p.TotalShipping) * Quantity; }
        }

        //Tax
        public decimal TotalTax
        {
            get { return Products.Sum(p => p.TotalTax) * Quantity; }
        }

        //TotalCost
        public decimal TotalCost
        {
            get { return TotalPrice + TotalShipping + TotalTax; }
        }

        //AmountDue
        public decimal AmountDue
        {
            get { return Math.Max(TotalCost - Products.Sum(p => p.Discount) - Products.Sum(p => p.CouponDiscount), 0); }
        }

        public PackageCart()
        {
            Products = new List<PackageCartLine>();
        }
    }
}
