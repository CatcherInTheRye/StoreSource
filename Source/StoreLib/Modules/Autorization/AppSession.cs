using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using StoreLib.Model;
using StoreLib.Model.Classes;
using StoreLib.Modules.Application;
using StoreLib.Modules.Helpers;
using StoreLib.Services.Interface;

namespace StoreLib.Modules.Autorization
{
    #region Session Keys

    public class SessionKeys
    {
        public const string User = "CurrentUser";
        public const string CartUser = "ShoppingCartUser";
        public const string Cart = "ShoppingCart";
        public const string CartPackage = "ShoppingCartPackage";
        public const string GiftCart = "ShoppingGiftCards";

        public const string ShoppingCart = "ShoppingCartN";
        public const string ShoppingCartPackage = "ShoppingCartPackageN";
        public const string ShoppingGiftCart = "ShoppingGiftCardsN";
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
                            StoreIdentity Identity = principal.StoreIdentity;
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

        //CartUser
        public static SessionUser CartUser
        {
            get { return HttpContext.Current.Session[SessionKeys.CartUser] as SessionUser; }
            set { HttpContext.Current.Session[SessionKeys.CartUser] = value; }
        }

        //UserCart
        public static SessionCart UserCart
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null) return new SessionCart();
                SessionCart cart = HttpContext.Current.Session[SessionKeys.Cart] as SessionCart;
                if (cart == null)
                {
                    cart = new SessionCart();
                    HttpContext.Current.Session[SessionKeys.Cart] = cart;
                }
                return cart;
            }
        }

        //CartPackage
        public static SessionPackageCart CartPackage
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null) return new SessionPackageCart();
                SessionPackageCart cart = HttpContext.Current.Session[SessionKeys.CartPackage] as SessionPackageCart;
                if (cart == null)
                {
                    cart = new SessionPackageCart();
                    HttpContext.Current.Session[SessionKeys.CartPackage] = cart;
                }
                return cart;
            }
            set { HttpContext.Current.Session[SessionKeys.CartPackage] = value; }
        }
    }

    public class AppSessionN
    {
        //IsAuthenticated
        public static bool IsAuthenticated
        {
            get { return (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated); }
        }

        //CurrentUser
        public static SessionUser CurrentUser
        {
            get
            {
                try
                {
                    if (!IsAuthenticated) return null;
                    SessionUser user = HttpContext.Current.Session[SessionKeys.User] as SessionUser;
                    if (user == null)
                    {
                        StorePrincipal principal = (HttpContext.Current.User as StorePrincipal);
                        if (principal != null)
                        {
                            StoreIdentity Identity = principal.StoreIdentity;
                            User usr = DependencyResolver.Current.GetService<IUserService>().GetUserActiveAndApproved(Identity.Id, Identity.Name);
                            if (usr == null)
                            {
                                IFormsAuthenticationService formsService = new FormsAuthenticationService();
                                formsService.SignOut();
                            }
                            else
                                user = new SessionUser(usr, DependencyResolver.Current.GetService<IGeneralServiceOld>().StoreUsersGetStoresByUser(usr.Id).Select(t => new IdTitleDescription { Id = t.Id, Title = t.Title, Description = t.Color }).ToList());
                        }
                        else
                        {
                            IFormsAuthenticationService formsService = new FormsAuthenticationService();
                            formsService.SignOut();
                        }
                        HttpContext.Current.Session[SessionKeys.User] = user;
                    }
                    return user;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[SessionKeys.User] = value;
            }
        }

        //UserCart
        public static SessionShoppingCart UserCart
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null) return new SessionShoppingCart();
                SessionShoppingCart cart = HttpContext.Current.Session[SessionKeys.ShoppingCart] as SessionShoppingCart;
                if (cart == null)
                {
                    cart = new SessionShoppingCart();
                    HttpContext.Current.Session[SessionKeys.ShoppingCart] = cart;
                }
                return cart;
            }
        }

        //CartPackage
        public static SessionPackageCart CartPackage
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null) return new SessionPackageCart();
                SessionPackageCart cart = HttpContext.Current.Session[SessionKeys.ShoppingCartPackage] as SessionPackageCart;
                if (cart == null)
                {
                    cart = new SessionPackageCart();
                    HttpContext.Current.Session[SessionKeys.ShoppingCartPackage] = cart;
                }
                return cart;
            }
            set { HttpContext.Current.Session[SessionKeys.ShoppingCartPackage] = value; }
        }

        //GiftCart
        public static SessionGiftCart GiftCart
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null) return new SessionGiftCart();
                SessionGiftCart cart = HttpContext.Current.Session[SessionKeys.ShoppingGiftCart] as SessionGiftCart;
                if (cart == null)
                {
                    cart = new SessionGiftCart();
                    HttpContext.Current.Session[SessionKeys.ShoppingGiftCart] = cart;
                }
                return cart;
            }
            set { HttpContext.Current.Session[SessionKeys.ShoppingGiftCart] = value; }
        }
    }

    #endregion App Session
}
