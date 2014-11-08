using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using PCSMvc.Models;

namespace PCSMvc.Global.Auth
{
    public class PermissionManager
    {
        public bool ValidatePermissions(string controller, string action, string user, UserIndentity userIndentity, UserRoleEnum role)
        {
            if (userIndentity == null)
            {
                return false;
            }
            bool result = false;

            try
            {
                result = userIndentity.User.UserRole == (int)role;
            }
            catch (Exception)
            {
                result = false;
            }

            //if (user == "user1" && controller == "Home")
            //{
            //    switch (action)
            //    {
            //        case "Test":
            //            isUserAccess = true;
            //            break;
            //    }
            //}

            //if (user == "user2" && controller == "Home")
            //{
            //    switch (action)
            //    {
            //        case "Edit":
            //            isUserAccess = true;
            //            break;
            //    }
            //}

            //// Незарегистрированных ползователей пускаем на главную и "О проекте"
            //if (controller == "Home" && (action == "Index" || action == "About"))
            //{
            //    isUserAccess = true;
            //}

            return result;
        }
    }

    public class DynamicAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        public UserRoleEnum Role { get; set; }

        private string accessDenyURL = "AccessDenied";
        public string AccessDenyURL
        {
            get { return accessDenyURL; }
            set { accessDenyURL = value; }
        }

        private string logOnURL = "Login";
        public string LogOnURL
        {
            get { return logOnURL; }
            set { logOnURL = value; }
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            PermissionManager permissionManager = new PermissionManager();
            string action = filterContext.ActionDescriptor.ActionName;
            string controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string user = filterContext.HttpContext.User.Identity.Name;

            UserIndentity userIndentity = filterContext.HttpContext.User.Identity as UserIndentity;

            if (!permissionManager.ValidatePermissions(controller, action, user, userIndentity, Role))
            {
                //throw new UnauthorizedAccessException("User is not allowed to perform this action");
                if (!string.IsNullOrEmpty(AccessDenyURL) && (userIndentity != null && userIndentity.IsAuthenticated))
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary 
                        { 
                            { "controller", "Home" }, 
                            { "action", AccessDenyURL } 
                        }
                    );
                    //filterContext.HttpContext.Response.Redirect(AccessDenyURL);
                    return;
                }
                if (!string.IsNullOrEmpty(LogOnURL))
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary 
                        { 
                            { "controller", "Home" }, 
                            { "action", LogOnURL } 
                        }
                    );
                    //filterContext.HttpContext.Response.Redirect(LogOnURL);
                }
            }
        }
    }
}