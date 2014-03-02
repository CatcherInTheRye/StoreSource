using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoreLib.Data.Repositories.Interfaces;
using StoreLib.Model.Classes;

namespace StoreLib.Data.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private VStoreDBDataContext dataContext;

        public ProductRepository(string connectionString)
        {
            dataContext = new VStoreDBDataContext(connectionString);
        }

        public List<ProductShort> ProductShortsGet()
        {
            throw new NotImplementedException();
        }

        public ProductShort ProductShortGet(int id)
        {
            throw new NotImplementedException();
        }
    }
}
