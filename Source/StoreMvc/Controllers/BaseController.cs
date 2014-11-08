using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using DataRepository;
using Ninject;
using PCSMvc.App_Start;
using PCSMvc.Global.Auth;
using PCSMvc.Models;

namespace PCSMvc.Controllers
{
    [ValidateInput(false)]
    public abstract class BaseController : Controller
    {
        protected JsonResult JSON(object obj)
        {
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [Inject]
        public IRepository Repository { get; set; }

        [Inject]
        public IAuthentication Auth { get; set; }

        public JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

        protected JsonResult Json(object obj)
        {
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public user CurrentUser
        {
            get
            {
                return ((IUserProvider)Auth.CurrentUser.Identity).User;
            }
        }

        
    }
}
