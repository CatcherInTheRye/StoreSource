using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Model.Classes
{
    [Serializable]
    public class SessionPackageCart
    {
        public string cartGuId { get; set; }
        public Dictionary<long, PackageCart> Lines { get; private set; }

        public SessionPackageCart()
        {
            Lines = new Dictionary<long, PackageCart>();
        }

        //IsEmpty
        public bool IsEmpty
        {
            get { return Lines.Count() == 0; }
        }

        //AddItem
        public void AddItem(PackageCart package)
        {
            long key = package.IdTitle.Id;
            if (!Lines.ContainsKey(key))
                Lines.Add(key, package);
            else
            {
                Lines[key] = package;
            }
        }

        //RemoveItem
        public void RemoveItem(long package_Id)
        {
            Lines.Remove(package_Id);
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

        //Shipping
        public decimal Shipping
        {
            get { return Lines.Sum(l => l.Value.TotalShipping); }
        }

        //Tax
        public decimal Tax
        {
            get { return Lines.Sum(l => l.Value.TotalTax); }
        }

        //TotalCost
        public decimal TotalCost
        {
            get { return TotalPrice + Shipping + Tax; }
        }
    }
}
