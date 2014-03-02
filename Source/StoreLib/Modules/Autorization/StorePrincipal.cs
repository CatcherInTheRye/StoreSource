using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace StoreLib.Modules.Autorization
{
    public class StorePrincipal : IPrincipal
    {
        private StoreIdentity Identity;

        public StorePrincipal(StoreIdentity Identity)
        {
            this.Identity = Identity;
        }

        public StoreIdentity StoreIdentity { get { return Identity; } }

        public IIdentity Identity { get { return Identity; } }

        public bool IsInRole(string role)
        {
            return string.Compare(Identity.UserType.ToLower(), role, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public bool IsNeedToCheckStatus(TimeSpan checkTime)
        {
            return DateTime.UtcNow.Subtract(Identity.LastCheckTime) > checkTime;
        }
    }
}
