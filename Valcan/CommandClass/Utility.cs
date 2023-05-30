using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Valcan.CommandClass
{
    public class Utility
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Utility));  //Declaring Log4Net

        public static void FileUploadJob()
        {
            Log.Error("Hangfire Job start");
            CommonMethod cm = new CommonMethod();
            bool res = cm.LoadALlExcelData();
            //bool res = true;
            if (res)
            {
                Log.Error("Hangfire Job Excel uploaded successfully");
            }
            else
            {
                Log.Error("Hangfire Job Excel uploaded not successfully.Please check log for more details");
            }
            
        }
    }
}