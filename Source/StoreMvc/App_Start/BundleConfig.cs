using System;
using System.Web;
using System.Web.Optimization;

namespace PCSMvc
{
    public class BundleConfig
    {
        //public static void AddDefaultIgnorePatterns(IgnoreList ignoreList)
        //{
        //    if (ignoreList == null)
        //        throw new ArgumentNullException("ignoreList");
        //    ignoreList.Ignore("*.intellisense.js");
        //    ignoreList.Ignore("*-vsdoc.js");
        //    ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
        //    ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
        //    ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
        //}

        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            //bundles.IgnoreList.Clear();
            //AddDefaultIgnorePatterns(bundles.IgnoreList);

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/neededscripts").Include(
                "~/Scripts/jquery.mobile-1.4.3.js",
                //"~/Scripts/tinymce.min.js",
                //"~/Scripts/KendoUICore/js/kendo.web.min.js",
                "~/Scripts/kendo.web.min.js",
                "~/Scripts/jquery.lightbox_me.js",
                "~/Scripts/select2.min.js",
                "~/Scripts/script.js",
                "~/js/Common.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/themes/default/jquery.mobile-1.4.3.min.css",
                "~/Content/kendo.common.min.css",
                "~/Content/kendo.rtl.min.css",
                "~/Content/kendo.default.min.css",
                "~/Content/select2.css",
                //"~/Scripts/KendoUICore/styles/kendo.common.min.css",
                //"~/Scripts/KendoUICore/styles/kendo.rtl.min.css",
                //"~/Scripts/KendoUICore/styles/kendo.default.min.css",
                //"~/Content/lightbox_me_styles.css",
                "~/Content/main.css"
                ));

            //bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
            //            "~/Content/themes/base/jquery.ui.core.css",
            //            "~/Content/themes/base/jquery.ui.resizable.css",
            //            "~/Content/themes/base/jquery.ui.selectable.css",
            //            "~/Content/themes/base/jquery.ui.accordion.css",
            //            "~/Content/themes/base/jquery.ui.autocomplete.css",
            //            "~/Content/themes/base/jquery.ui.button.css",
            //            "~/Content/themes/base/jquery.ui.dialog.css",
            //            "~/Content/themes/base/jquery.ui.slider.css",
            //            "~/Content/themes/base/jquery.ui.tabs.css",
            //            "~/Content/themes/base/jquery.ui.datepicker.css",
            //            "~/Content/themes/base/jquery.ui.progressbar.css",
            //            "~/Content/themes/base/jquery.ui.theme.css"));
        }
    }
}