using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoreLib.Model.Classes;

namespace StoreLib.Data.Repositories.Interfaces
{
    public interface IProductRepository
    {
        List<ProductShort> ProductShortsGet();
        ProductShort ProductShortGet(int id);
    }
}
