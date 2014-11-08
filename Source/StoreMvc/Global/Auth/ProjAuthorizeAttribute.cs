using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PCSMvc.Models;

namespace PCSMvc.Global.Auth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ProjAuthorizeAttribute : AuthorizeAttribute
    {
        public new string/*List<UserRoleEnum>*/ Roles { get; set; }
        public string AccessDenyURL { get; set; }
        public string LogOnURL { get; set; }


    }
}