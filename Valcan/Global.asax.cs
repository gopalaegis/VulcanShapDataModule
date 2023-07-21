using Hangfire;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Valcan
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            FileInfo f = new FileInfo(MapPath("~/log4net.config"));
            if (f.Exists)
                log4net.Config.XmlConfigurator.ConfigureAndWatch(f);

            log4net.Config.XmlConfigurator.Configure();
            
          
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Session_Start(object sender, EventArgs e)
        {
            HttpContext.Current.Session.Timeout = 40;
        }
        private static string MapPath(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                return HostingEnvironment.MapPath(path);
            }
            else
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
                return Path.Combine(baseDirectory, path);
            }
        }
    }
}
