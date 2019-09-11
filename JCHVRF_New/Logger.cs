using JCHVRF_New.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Language = JCHVRF_New.LanguageData.LanguageViewModel;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace JCHVRF_New
{
    public class Logger
    {
        //Declare an instance for log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void LogProjectError(int? id, Exception ex, bool flagShowMessage = true)
        {
            string subRegion = JCHVRF.Model.Project.GetProjectInstance?.SubRegionCode;
            string region = JCHVRF.Model.Project.GetProjectInstance?.RegionCode;
            string format = string.Format("Project ID: {0} , Region : {1} , SubRegion: {2},Exception : {3}", id.HasValue ? id.Value.ToString() : "Not Found", string.IsNullOrEmpty(region) ? "Not Found" : region, string.IsNullOrEmpty(subRegion) ? "Not Found" : subRegion, ex != null ? ex.StackTrace.ToString() : "No exception");
            //log.Error(format, ex);
            log.Error(format);
            if (flagShowMessage)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                   {
                       JCHMessageBox.Show(Language.Current.GetMessage("ALERT_CONTACT"), MessageType.Error);
                   }));
            }
            //SendEmailtoContacts();
        }
        public static void LogProjectInfo(String Message)
        {
            log.Error(Message.ToString());
        }
        //Start code to send log file to support/developer.
        private static void SendEmailtoContacts()
        {
            //Outlook.MailItem mailItem = (Outlook.MailItem)
            // this.Application.CreateItem(Outlook.OlItemType.olMailItem);
            Outlook.Application app = new Outlook.Application();
            Outlook.MailItem mailItem = app.CreateItem(Outlook.OlItemType.olMailItem);
            mailItem.Subject = "Crash Log File";
            mailItem.To = "jchvrnnextgensupport@jci.com";
            mailItem.Body = "PFA log file.";
            mailItem.Attachments.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JCHVRFNextGen", "ErrorInfoLogs.log"));//logPath is a string holding path to the log.txt file
            mailItem.Importance = Outlook.OlImportance.olImportanceHigh;
            mailItem.Display(false);
            mailItem.Send();

        }
        //End code to send log file to support/developer.

    }
}
