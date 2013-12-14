using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VStore.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        ////Second page
        //[HttpGet]
        //public JsonResult ContentGet(int menuTipId)
        //{
        //    ContentWrapper content = Library.ContentGet((MenuTips)menuTipId);
        //    return JSON(content);
        //}
    }
}
