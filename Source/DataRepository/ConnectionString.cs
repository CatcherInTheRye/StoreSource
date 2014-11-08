using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataRepository.Properties;

namespace DataRepository
{
    public static class ConnectionString
    {
        public static string DbConnectionString
        {
            get { return Settings.Default.PCSConnectionString; }
        }
    }
}
