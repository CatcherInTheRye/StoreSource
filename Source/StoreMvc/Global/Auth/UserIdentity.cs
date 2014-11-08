using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using DataRepository;

namespace PCSMvc.Global.Auth
{
    /// <summary>
    /// Реализация интерфейса для идентификации пользователя
    /// </summary>
    public class UserIndentity : IIdentity, IUserProvider
    {
        /// <summary>
        /// CurrentUser
        /// </summary>
        public user User { get; set; }

        /// <summary>
        /// Class type for user
        /// </summary>
        public string AuthenticationType
        {
            get
            {
                return typeof(user).ToString();
            }
        }

        /// <summary>
        /// Authenticate or not
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return User != null;
            }
        }

        /// <summary>
        /// User's unique name - userName
        /// </summary>
        public string Name
        {
            get
            {
                if (User != null)
                {
                    return User.userName;
                }
                return "anonym";
            }
        }

        /// <summary>
        /// username init
        /// </summary>
        /// <param name="userName">login - userName</param>
        public void Init(string userName, IRepository repository)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                User = repository.UserGetByLogin(userName);
            }
        }
    }
}