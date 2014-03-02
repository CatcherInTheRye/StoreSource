using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoreLib.Model;
using StoreLib.Services.Interface;

namespace StoreLib.Services.Service
{
    public class UserService : IUserService
    {
        //TODO : сейчас это заплатка. Вставить обращение в репозиторий
        public User GetUser(int userId, bool extended, bool fromcache)
        {
            return new User {Id = userId};
        }

        public User GetUser(string login, bool extended, bool fromcache)
        {
            return new User { Id = 1, Email = login };
        }

        public User GetUserByEmail(string email, bool extended, bool fromcache)
        {
            return new User { Id = 1, Email = email };
        }

        public User GetUserByConfirmationCode(string confirmationcode)
        {
            throw new NotImplementedException();
        }
    }
}
