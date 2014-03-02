using System;
using System.Web.Mvc;
using StoreLib.Modules.Autorization;
using StoreLib.Modules.Performance;
using VStore.Controllers;
using VStore.Models;
using WebMatrix.WebData;

namespace VStore.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Admin/Home/
        [HttpGet, StoreAuthorize(AccessDenyUrl = "/Admin/AccessDenied", Roles = "Root,Admin,ContentManager")]
        public ActionResult Index()
        {
            return View();
        }

        //TODO : require SSL filter attribute
        //[AllowAnonymous]
        [HttpGet, Compress]
        public ActionResult Login(/*string returnUrl*/)
        {
            //ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, Compress]
        //[AllowAnonymous, ValIdateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValId && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provIded is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValIdateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
