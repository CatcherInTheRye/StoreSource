using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace StoreLib.Modules.Autorization
{
    public class StorePrincipal : IPrincipal
    {
        private StoreIdentity identity;

        public StorePrincipal(StoreIdentity identity)
        {
            this.identity = identity;
        }

        public StoreIdentity StoreIdentity { get { return identity; } }

        public IIdentity Identity { get { return identity; } }

        public bool IsInRole(string role)
        {
            return string.Compare(identity.UserType.ToLower(), role, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public bool IsNeedToCheckStatus(TimeSpan checkTime)
        {
            return DateTime.UtcNow.Subtract(identity.LastCheckTime) > checkTime;
        }
    }
}
