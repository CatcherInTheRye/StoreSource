using System.Web;
using System.Web.Optimization;

namespace VStore
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.valIdate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Css/css").Include(
                "~/Css/Main.css",
                "~/Css/content.css",
                "~/Css/jpreloader.css"));

            bundles.Add(new StyleBundle("~/Css/themes/base/css").Include(
                        "~/Css/themes/base/jquery.ui.core.css",
                        "~/Css/themes/base/jquery.ui.resizable.css",
                        "~/Css/themes/base/jquery.ui.selectable.css",
                        "~/Css/themes/base/jquery.ui.accordion.css",
                        "~/Css/themes/base/jquery.ui.autocomplete.css",
                        "~/Css/themes/base/jquery.ui.button.css",
                        "~/Css/themes/base/jquery.ui.dialog.css",
                        "~/Css/themes/base/jquery.ui.slIder.css",
                        "~/Css/themes/base/jquery.ui.tabs.css",
                        "~/Css/themes/base/jquery.ui.datepicker.css",
                        "~/Css/themes/base/jquery.ui.progressbar.css",
                        "~/Css/themes/base/jquery.ui.theme.css"));
        }
    }
}