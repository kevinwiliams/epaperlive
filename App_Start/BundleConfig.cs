using System.Web;
using System.Web.Optimization;

namespace ePaperLive
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new Bundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/moment.min.js",
                        "~/Scripts/cleave.min.js",
                        "~/Scripts/cleave-phone.i18n.js"));

            bundles.Add(new Bundle("~/bundles/jqueryval").Include(
                      "~/Scripts/jquery.validate.min.js",
                      "~/Scripts/jquery.validate.unobtrusive.min.js",
                      "~/Scripts/jquery.unobtrusive-ajax.min.js")); //

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            //bundles.Add(new Bundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/Scripts/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new Bundle("~/Scripts/js").Include(
                      "~/Content/aos/aos.js",
                      "~/Content/glightbox/js/glightbox.min.js",
                      "~/Scripts/isotope-layout/isotope.pkgd.min.js",
                      "~/Content/swiper/swiper-bundle.min.js",
                      "~/Scripts/sweetalert.min.js",
                      "~/Scripts/main.js"));

            bundles.Add(new StyleBundle("~/Content/vendor").Include(
                     "~/Content/aos/aos.css",
                     "~/Content/bootstrap-icons/bootstrap-icons.css",
                     "~/Content/boxicons/css/boxicons.min.css",
                     "~/Content/glightbox/css/glightbox.min.css",
                     "~/Content/swiper/swiper-bundle.min.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                     "~/Content/bootstrap.css",
                     "~/Styles/sweetalert.css",
                     "~/Content/Site.css",
                     "~/Content/style.css"));

            BundleTable.EnableOptimizations = true;
        }
    }
}
