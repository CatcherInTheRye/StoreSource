using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Model.Classes
{
    [Serializable]
    public class PackageCartLine : CartLine
    {
        public int MaxPackageCount { get { return SKU.Quantity / Quantity; } }
    }
}
