using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;

namespace JCHVRF
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            JCBase.Utility.MyConfig.AppPath = Application.StartupPath;
            //将数字和日期格式强制设为英美模式。add on 20180503 by Shen Junjie
            JCBase.Util.CultureHelper.SetCurrentCultrue(new CultureInfo("en-US"));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmRegionBrand());
        }
    }
}
