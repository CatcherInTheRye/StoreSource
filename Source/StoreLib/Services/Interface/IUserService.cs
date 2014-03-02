using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoreLib.Model;

namespace StoreLib.Services.Interface
{
    public interface IUserService
    {
        #region Get user(s)

        User GetUser(int userId, bool extended, bool fromcache);
        User GetUser(string login, bool extended, bool fromcache);
        User GetUserByEmail(string email, bool extended, bool fromcache);
        User GetUserByConfirmationCode(string confirmationcode);

        #endregion Get user(s)
    }
}
