using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using StoreLib.Modules.Application;

namespace StoreLib.Modules.Autorization
{
    public class StoreIdentity : IIdentity
    {
        private bool isAuthenticated;
        private string userName;
        private int userId;
        private DateTime lastCheckTime;
        private string userType;
        private bool rememberMe;

        public StoreIdentity()
        {
            userName = string.Empty;
            userId = 0;
            userType = null;
            rememberMe = false;
        }

        public StoreIdentity(string userName, string userData) : this()
        {
            this.userName = userName;
            string[] arrayStr = userData.Split('|');
            if (arrayStr.Count() >= 4)
            {
                int.TryParse(arrayStr[0], out userId);
                DateTime.TryParse(arrayStr[1], out lastCheckTime);
                bool.TryParse(arrayStr[2], out rememberMe);
                userType = arrayStr[3];
            }
            isAuthenticated = userId > 0 && !string.IsNullOrEmpty(userName);
        }

        public string AuthenticationType { get { return "Forms"; } }

        public string UserType { get
        {
            return (IsAuthenticated && !string.IsNullOrEmpty(userType))
                       ? Security.Encryption.PasswordDecrypt(userType, ApplicationHelper.EncryptionKey)
                       : string.Empty;
        } }

        public bool IsAuthenticated { get { return isAuthenticated; } }

        public string Name
        {
            get { return IsAuthenticated ? userName : string.Empty; }
        }

        public int Id
        {
            get { return IsAuthenticated ? userId : 0; }
        }

        public DateTime LastCheckTime
        {
            get { return lastCheckTime; }
            set { lastCheckTime = value; }
        }

        public bool RememberMe
        {
            get { return rememberMe; }
        }
    }
}
