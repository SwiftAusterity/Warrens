
namespace NetMud
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = false;

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/marked.js",
                      "~/Scripts/bootstrap-markdown.js",
                      "~/Scripts/moment.js",
                      "~/Scripts/bootstrap-datetimepicker.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/spectrum.js",
                      "~/Scripts/js-cookie.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.lumen.css",
                      "~/Content/bootstrap-markdown.min.css",
                      "~/Content/bootstrap-datetimepicker.min.css",
                      "~/Content/spectrum.css",
                      "~/Content/popper.css",
                      "~/Content/pretty-checkbox.min.css"));

            bundles.Add(new StyleBundle("~/Content/jqueryui-css").Include(
                      "~/Content/jquery-ui.css",
                      "~/Content/jquery-ui-structure.css",
                      "~/Content/jquery-ui-theme.css"));
        }
    }
}
