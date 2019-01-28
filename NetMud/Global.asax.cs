using NetMud.Models;
using System;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace NetMud
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GameConfig.PreloadSupportingEntities();

            ModelBinders.Binders.DefaultBinder = new InterfaceModelBinder();

            _SetupRefreshJob();
        }

        private static void _pingSite()
        {
            using (WebClient refresh = new WebClient())
            {
                try
                {
                    refresh.DownloadString("https://netmud.swiftausterity.com/");
                }
                catch 
                {
                }
            }
        }

        private static void _SetupRefreshJob()
        {
            if (HttpContext.Current == null)
            {
                return;
            }

            //remove a previous job
            Action remove = HttpContext.Current.Cache["Refresh"] as Action;
            if (remove is Action)
            {
                HttpContext.Current.Cache.Remove("Refresh");
                remove.EndInvoke(null);
            }

            //get the worker
            Action work = () =>
            {
                while (true)
                {
                    Thread.Sleep(10000);
                    _pingSite();
                }
            };
            work.BeginInvoke(null, null);

            //add this job to the cache
            HttpContext.Current.Cache.Add("Refresh",
                         work,
                         null,
                         Cache.NoAbsoluteExpiration,
                         Cache.NoSlidingExpiration,
                         CacheItemPriority.Normal,
                         (s, o, r) => { _SetupRefreshJob(); }
              );
        }

        private void Application_End(object sender, EventArgs e)
        {
            // Force the App to be restarted immediately
            _pingSite();
        }
    }
}
