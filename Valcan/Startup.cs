using Hangfire;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Valcan.CommandClass;

[assembly: OwinStartup(typeof(Valcan.Startup))]
namespace Valcan
{
    public partial class Startup
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Startup));  //Declaring Log4Net
        public void Configuration(IAppBuilder app)
        {
            Log.Error("Startup");
            try
            {
                GlobalConfiguration.Configuration
                .UseSqlServerStorage(System.Configuration.ConfigurationManager.AppSettings["Connectionstring"]);

                app.UseHangfireDashboard();
                app.UseHangfireServer();

                RecurringJob.AddOrUpdate("Job1", () => Utility.FileUploadJob(), Cron.Daily(Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["HangfireJobHour"]), Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["HangfireJobMinutes"])),TimeZoneInfo.Local);
                //RecurringJob.AddOrUpdate("Job3", () => Utility.FileUploadJob(), "*/2 * * * *");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
            
        }

        
    }
}