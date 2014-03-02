using System.Web.Mvc;

namespace VStore.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public overrIde string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public overrIde void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{Id}",
                new { action = "Index", Id = UrlParameter.Optional }
            );
        }
    }
}
