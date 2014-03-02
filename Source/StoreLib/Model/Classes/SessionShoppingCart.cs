using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Model.Classes
{
    [Serializable]
    public class SessionShoppingCart
    {
        public Dictionary<long, CartItem> Lines { get; private set; }

        public SessionShoppingCart()
        {
            Lines = new Dictionary<long, CartItem>();
        }

        //IsEmpty
        public bool IsEmpty
        {
            get { return !Lines.Any(); }
        }

        //AddItem
        public void AddItem(ProductShort product, SkuDetail sku, List<SkuAttr> attr, int qty, int backQty,
                            decimal? tier = null, decimal? tax = null, decimal? discount = null,
                            long? discountreason_Id = null)
        {
            long key = sku.IdTitle.Id;
            if (!Lines.ContainsKey(key))
                Lines.Add(key,
                          new CartItem
                              {
                                  Product = product,
                                  SKU = sku,
                                  Attributes = attr,
                                  Quantity = qty,
                                  BackorderQty = backQty,
                                  InitQuantity = qty,
                                  /*Tax = tax.GetValueOrDefault(0),*/ Discount = discount.GetValueOrDefault(0),
                                  DiscountReason_Id = discountreason_Id,
                                  TiersDiscount = tier.GetValueOrDefault(0)
                              });
            else
            {
                Lines[key].Quantity = Lines[key].InitQuantity = qty;
                Lines[key].BackorderQty = backQty;
                Lines[key].Product = product;
                Lines[key].SKU = sku;
                Lines[key].Attributes = attr;
                //Lines[key].Tax = tax.GetValueOrDefault(0);
                Lines[key].Discount = discount.GetValueOrDefault(0);
                Lines[key].DiscountReason_Id = discountreason_Id.GetValueOrDefault(0);
                Lines[key].TiersDiscount = tier.GetValueOrDefault(0);
            }
            if (Lines[key].SKU.Quantity < Lines[key].Quantity)
                Lines[key].Quantity = Lines[key].InitQuantity = Lines[key].SKU.Quantity;
        }

        //addPaIdItem
        public void addPaIdItem(ProductShort product, SkuDetail sku, List<SkuAttr> attr, int qty,
                                decimal? tax = null, decimal? discount = null, long? discountreason_Id = null,
                                decimal? couponDiscount = null, long? couponType_Id = null)
        {
            long key = sku.IdTitle.Id;
            if (!Lines.ContainsKey(key))
                Lines.Add(key,
                          new CartItem
                              {
                                  Product = product,
                                  SKU = sku,
                                  Attributes = attr,
                                  Quantity = qty,
                                  InitQuantity = qty,
                                  /*Tax = tax.GetValueOrDefault(0),*/ Discount = discount.GetValueOrDefault(0),
                                  DiscountReason_Id = discountreason_Id,
                                  CouponDiscount = couponDiscount.GetValueOrDefault(0),
                                  CouponType_Id = couponType_Id
                              });
            else
            {
                Lines[key].Quantity = Lines[key].InitQuantity = qty;
                Lines[key].Product = product;
                Lines[key].SKU = sku;
                Lines[key].Attributes = attr;
                //Lines[key].Tax = tax.GetValueOrDefault(0);
                Lines[key].Discount = discount.GetValueOrDefault(0);
                Lines[key].DiscountReason_Id = discountreason_Id.GetValueOrDefault(0);
                Lines[key].CouponDiscount = couponDiscount.GetValueOrDefault(0);
                Lines[key].CouponType_Id = couponType_Id;
            }
        }

        //RemoveItem
        public void RemoveItem(long sku_Id)
        {
            Lines.Remove(sku_Id);
        }

        //ClearCart
        public void ClearCart()
        {
            Lines.Clear();
        }

        //TotalPrice
        public decimal TotalPrice
        {
            get { return Lines.Sum(l => (l.Value.TotalPrice)); }
        }
    }
}
