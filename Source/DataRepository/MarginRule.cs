//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataRepository
{
    using System;
    using System.Collections.Generic;
    
    public partial class MarginRule
    {
        public MarginRule()
        {
            this.Products = new HashSet<Product>();
        }
    
        public int Id { get; set; }
        public decimal Margin { get; set; }
        public bool IsPercent { get; set; }
    
        public virtual ICollection<Product> Products { get; set; }
    }
}
