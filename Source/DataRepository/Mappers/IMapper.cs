using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCSMvc.Mappers
{
    public interface IMapper
    {
        object Map(object source, Type sourceType, Type destinationType);
    }
}
