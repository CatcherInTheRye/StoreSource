using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DataDynamics.ActiveReports;
using DataDynamics.ActiveReports.Export.Pdf;
using DataDynamics.ActiveReports.Export.Xls;
using DataRepository;
using DataRepository.DataContracts;
using PCS.Reports;
using PCSMvc.Global.Auth;
using PCSMvc.Models;
using PCSMvc.Models.ViewModels;
using PCS.DataRepository.DataContracts;

namespace PCSMvc.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (CurrentUser != null)
            {
                switch (CurrentUser.UserRole)
                {
                    case (int)UserRoleEnum.PCSManager:
                        return RedirectToAction("PCSManager");
                        break;
                    case (int)UserRoleEnum.Specialist:
                        return RedirectToAction("Specialist");
                        break;
                    case (int)UserRoleEnum.SchoolUser:
                        return RedirectToAction("SchoolUser");
                        break;
                    case (int)UserRoleEnum.DistrictSysAdmin:
                        return RedirectToAction("DistrictSysAdmin");
                        break;
                    default:
                        return RedirectToAction("Login");
                        break;
                }
            }

            return RedirectToAction("Login");
        }

        #region Login

        public ActionResult UserLogin()
        {
            return View(CurrentUser);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginView());
        }

        [HttpPost]
        public ActionResult Login(LoginView loginView)
        {
            if (ModelState.IsValid)
            {
                var user = Auth.Login(loginView.UserName, loginView.Password, loginView.IsPersistent);
                if (user != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState["Password"].Errors.Add("Passwords doesn't match");
            }
            return View(loginView);
        }

        public ActionResult Logout()
        {
            Auth.LogOut();
            return RedirectToAction("Login", "Home");
        }

        //AccessDenied
        public ActionResult AccessDenied()
        {
            return View();
        }

        #endregion Login

    }
}