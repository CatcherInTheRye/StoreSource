using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using StoreLib.Model;
using StoreLib.Modules.Application;

namespace StoreLib.Modules.Autorization
{
    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string userName, bool createPersistentCookie, User user)
        {
            if (string.IsNullOrEmpty(userName) || user == null)
                throw new ArgumentException("Value can not be null or empty", "user");
            DateTime now = DateTime.Now;
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, userName, now,
                                                                             now.AddMinutes(
                                                                                 ApplicationHelper.
                                                                                     FormsAuthenticationTicketTime),
                                                                             createPersistentCookie,
                                                                             string.Format("{0}|{1}|{2}|{3}", user.Id,
                                                                                           DateTime.UtcNow,
                                                                                           createPersistentCookie,
                                                                                           Security.Encryption.
                                                                                               PasswordEncrypt(
                                                                                                   user.UserType.
                                                                                                       ToString(),
                                                                                                   ApplicationHelper.
                                                                                                       EncryptionKey)),
                                                                             FormsAuthentication.FormsCookiePath);
            string encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
            AppSession.CurrentUser = new SessionUser(user);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Session.Abandon();
        }
    }
}
