using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoreLib.Model;

namespace StoreLib.Modules.Autorization
{
    public interface IFormsAuthenticationService
    {
        void SignIn(string userName, bool createPersistantCookie, User user);
        void SignOut();
    }
}
