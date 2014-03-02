using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using StoreLib.Model;
using StoreLib.Modules.Application;

namespace StoreLib.Modules.Autorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class StoreAuthorizeAttribute : AuthorizeAttribute
    {
        public new string Roles { get; set; }
        public string AccessDenyUrl { get; set; }
        public string LogOnUrl { get; set; }
        public string IsBackendAllowed { get; set; }

        private void AddNoOutputCacheHeaders(AuthorizationContext filterContext)
        {
            HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
            cachePolicy.SetExpires(DateTime.UtcNow.AddDays(-1));
            cachePolicy.SetValIdUntilExpires(false);
            cachePolicy.AppendCacheExtension("must-revalIdate, proxy-revalIdate");
            cachePolicy.SetCacheability(HttpCacheability.NoCache);
            cachePolicy.SetNoStore();
        }

        protected virtual bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }
            StorePrincipal principal = httpContext.User as StorePrincipal;
            return principal != null;
        }

        private void NotAuthorized(AuthorizationContext filterContext, bool isAuthorizedUser = true)
        {
            AddNoOutputCacheHeaders(filterContext);
            if (!string.IsNullOrEmpty(AccessDenyUrl) && isAuthorizedUser)
            {
                filterContext.HttpContext.Response.Redirect(AccessDenyUrl);
            }
            if (!string.IsNullOrEmpty(LogOnUrl))
            {
                filterContext.HttpContext.Response.Redirect(LogOnUrl);
            }
            else
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        public void LogOutUser(AuthorizationContext filterContext)
        {
            IFormsAuthenticationService formsService = new FormsAuthenticationService();
            formsService.SignOut();
            AddNoOutputCacheHeaders(filterContext);
            if (!string.IsNullOrEmpty(LogOnUrl))
            {
                filterContext.HttpContext.Response.Redirect(LogOnUrl);
            }
            else
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        public overrIde void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (AuthorizeCore(filterContext.HttpContext))
            {
                StorePrincipal principal = filterContext.HttpContext.User as StorePrincipal;
                if (principal == null)
                {
                    LogOutUser(filterContext);
                    return;
                }
                StoreIdentity Identity = principal.StoreIdentity;

                SessionUser user = filterContext.HttpContext.Session[SessionKeys.User] as SessionUser;
                if (user == null)
                {
                    LogOutUser(filterContext);
                    return;
                }

                bool isNeedToCheckStatus = principal.IsNeedToCheckStatus(ApplicationHelper.StatusCheckTime);
                if (isNeedToCheckStatus)
                {
                    //TODO : заменить на получения пользователя из базы
                    User userFromService = new User { Id = 1, Login = ApplicationHelper.AdminLogin, Status = Statuses.Active };
                    if (userFromService != null && userFromService.Status == Statuses.Active)
                    {
                        IFormsAuthenticationService formsService = new FormsAuthenticationService();
                        formsService.SignIn(userFromService.Login, Identity.RememberMe, userFromService);
                    }
                    else
                    {
                        LogOutUser(filterContext);
                        return;
                    }
                }

                bool backendAllowed = false;
                if (!string.IsNullOrEmpty(IsBackendAllowed) && bool.TryParse(IsBackendAllowed, out backendAllowed) && backendAllowed && !user.IsAdminType)
                {
                    LogOutUser(filterContext);
                    return;
                }
                if (!string.IsNullOrEmpty(Roles))
                {
                    string[] roles = Roles.ToLower().Split(',');
                    bool isInRole = false;
                    foreach (string role in roles)
                    {
                        isInRole = principal.IsInRole(role);
                        if (isInRole) break;
                    }
                    if (!isInRole)
                    {
                        NotAuthorized(filterContext);
                    }
                }
            }
            else if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                NotAuthorized(filterContext, false);
            }
        }
    }
}
