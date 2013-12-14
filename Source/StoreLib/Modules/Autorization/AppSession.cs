using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using StoreLib.Model;
using StoreLib.Modules.Application;
using StoreLib.Modules.Helpers;

namespace StoreLib.Modules.Autorization
{
    #region Session Keys

    public class SessionKeys
    {
        public const string User = "CurrentUser";
    }

    #endregion Session Keys

    #region Session User

    [Serializable]
    public class SessionUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Statuses Status { get; set; }
        public UserTypes UserType { get; set; }

        public string FullName { get { return StringHelper.FullName(FirstName, LastName); } }

        public bool IsBuyer { get { return UserType == UserTypes.Buyer; } }
        public bool IsSeller { get { return UserType == UserTypes.Seller; } }
        public bool IsSellerBuyer { get { return UserType == UserTypes.SellerBuyer; } }
        public bool IsBuyerType { get { return IsBuyer || IsSellerBuyer; } }
        public bool IsSellerType { get { return IsSeller || IsSellerBuyer; } }
        public bool IsSalesPerson { get { return UserType == UserTypes.SalesPerson; } }
        public bool IsContentManager { get { return UserType == UserTypes.ContentManager; } }
        public bool IsAdmin { get { return UserType == UserTypes.Admin; } }
        public bool IsRoot { get { return UserType == UserTypes.Root; } }
        public bool IsAdminType { get { return IsAdmin || IsRoot; } }
        public bool IsBackendAllowed { get { return IsRoot || IsAdmin || IsContentManager || IsSalesPerson; } }

        public SessionUser(User user)
        {
            Id = user.Id;
            Login = user.Login;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            Status = user.Status;
            UserType = user.UserType;
        }
    }

    #endregion Session User

    #region App Session

    public static class AppSession
    {
        private static bool IsAuthenticated
        {
            get
            {
                return (HttpContext.Current != null && HttpContext.Current.User != null &&
                        HttpContext.Current.User.Identity.IsAuthenticated);
            }
        }

        public static SessionUser CurrentUser
        {
            get
            {
                try
                {
                    if (!IsAuthenticated)
                    {
                        return null;
                    }
                    SessionUser user = HttpContext.Current.Session[SessionKeys.User] as SessionUser;
                    if (user == null)
                    {
                        StorePrincipal principal = HttpContext.Current.User as StorePrincipal;
                        if (principal != null)
                        {
                            StoreIdentity identity = principal.StoreIdentity;
                            //TODO : получить пользователя из базы
                            string adminLogin = ApplicationHelper.AdminLogin;
                            string adminPassword = ApplicationHelper.AdminPassword;
                            if (!string.IsNullOrEmpty(adminLogin) && !string.IsNullOrEmpty(adminPassword))
                            {
                                user = new SessionUser(new User{Id = 1, Login = adminLogin});
                            }
                            else
                            {
                                IFormsAuthenticationService formsAuthenticationService = new FormsAuthenticationService();
                                formsAuthenticationService.SignOut();
                            }
                        }
                        else
                        {
                            //sign out
                            IFormsAuthenticationService formsAuthenticationService = new FormsAuthenticationService();
                            formsAuthenticationService.SignOut();
                        }
                    }
                    return user;
                }
                catch
                {
                    return null;
                }
            }
            set { HttpContext.Current.Session[SessionKeys.User] = value; }
        }
    }

    #endregion App Session
}
