using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JCHVRF.BLL;
using JCHVRF.Model;
using JCBase.Utility;
using Registr;
using System.IO;
using System.Threading.Tasks;

namespace JCHVRF
{
    public partial class frmRegionBrand : JCBase.UI.JCForm
    {
        public bool ISVALID = false;// 当前用户的注册信息是否有效
        frmStart splashScreen;

        MyDictionary dicBrand = null;
        string FactoryCode = "";
        string brandCode = "";

        public frmRegionBrand()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Minimized;
            this.Text += "_Ver." + MyConfig.Version;   //添加版本号   add on 20180605 by Vince

            #region 兼容2.2.0之前旧项目的Region 20180214 by Yunxiao Lin
            ProjectBLL _bll = new ProjectBLL(null);
            string errMsg = _bll.CompatOldProjectRegion();
            #endregion

            #region 启动 Start 窗口
            timer1.Enabled = true;
            timer1.Interval = 3000;
            splashScreen = new frmStart();
            if (splashScreen.ISVALID_Region && splashScreen.ISVALID_Date)
            {
                splashScreen.Show();
                this.ISVALID = true;
            }
            else
            {
                splashScreen.ShowDialog();
                this.ISVALID = false;
                if (splashScreen != null)
                {
                    timer1.Enabled = false;
                    splashScreen.Dispose();
                    this.WindowState = FormWindowState.Normal;
                }
            }
            #endregion
        }

        #region 将日志记录的内容写入指定的文件
        /// <summary>
        /// 将系统日志记录的内容写入指定的文件
        /// </summary>
        /// <param name="logList">系统日志记录</param>
        /// <param name="logPath"></param>
        private void WriteToLogFile(List<string> logList, string logPath)
        {
            if (logList == null || logList.Count == 0)
                return;
            StreamWriter streamWriter = new StreamWriter(logPath);
            for (int i = 0; i <= logList.Count - 1; i++)
            {
                streamWriter.WriteLine(logList[i]);
            }
            streamWriter.Flush();
            streamWriter.Close();
        }
        #endregion

        private void frmRegionBrand_Load(object sender, EventArgs e)
        {
            if (!(splashScreen.ISVALID_Region && splashScreen.ISVALID_Date))
                Close();

            LoadInfo();

            //启动线程，检查有没有更新
            //Task tsk = Task.Factory.StartNew(() =>
            //{
            string useAutoUpdate = "true";
            useAutoUpdate = ConfigurationManager.AppSettings["UseAutoUpdate"]==null ? "true" : ConfigurationManager.AppSettings["UseAutoUpdate"].ToString(); //Add switch for use auto update 20181226 by Yunxiao Lin
            if (useAutoUpdate.ToLower() != "false")
            {
                try
                {
                    AutoUpdate();
                }
                catch { }
            }
            //});
        }

        private void LoadInfo()
        {
            //this.gbBrand.Enabled = false;
            //this.gbSource.Enabled = false;
            //this.pnlOK.Enabled = false;

            //新增缓存表
            CachTableBLL CommonBll = new CachTableBLL();
            CommonBll.CreateCachTable();

            BindCmbRegion();
            if (Registration.IsSuperUser())
            {
                this.jccmbRegion.SelectedIndex = 0;
                this.jccmbRegion.Enabled = true;
            }
            else
            {
                string regionCode = Registration.GetRegionCode();
                brandCode = Registration.GetBrandCode();
                if (!string.IsNullOrEmpty(regionCode))
                {
                    this.jccmbRegion.SelectedValue = regionCode;
                    this.jccmbRegion.Enabled = false;
                }
            }
            //this.jccmbRegion.SelectedValue = SystemSetting.UserSetting.defaultSetting.region;
            BindCmbSubRegion();
            //this.jccmbSubRegion.SelectedValue = SystemSetting.UserSetting.defaultSetting.subRegion;

            if (!string.IsNullOrEmpty(this.jccmbSubRegion.Text))
            {
                BindBrandList();
            }
        }

        private void jccmbRegion_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //SystemSetting.UserSetting.defaultSetting.region = this.jccmbRegion.SelectedValue.ToString();
            BindCmbSubRegion();
            //SystemSetting.UserSetting.defaultSetting.subRegion = this.jccmbSubRegion.SelectedValue.ToString();
            //if (!string.IsNullOrEmpty(this.jccmbSubRegion.Text))
            //{
                BindBrandList();
            //}
            SystemSetting.Serialize();
        }

        private void jccmbSubRegion_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.jccmbSubRegion.Text))
            {
                //SystemSetting.UserSetting.defaultSetting.subRegion = this.jccmbSubRegion.SelectedValue.ToString();
                SystemSetting.Serialize();
                BindBrandList();
            }
        }

        private void Brand_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.jccmbSubRegion.Text))
            {
                RadioButton rbtn = sender as RadioButton;
                if (rbtn != null && rbtn.Checked)
                {
                    string name = rbtn.Name;
                    string code = name.Substring(name.IndexOf('_') + 1);
                    BindFactoryList(code);
                    dicBrand = (new MyDictionaryBLL()).GetItem(MyDictionary.DictionaryType.Brand, code);
                }
            }
        }

        private void Factory_Click(object sender, EventArgs e)
        {
            RadioButton rbtn = sender as RadioButton;
            if (rbtn != null && rbtn.Checked)
            {
                FactoryCode = rbtn.Name.Substring(rbtn.Name.IndexOf('_') + 1);
                this.pnlOK.Enabled = true;
            }
        }

        private void BindCmbRegion()
        {
            RegionBLL bll = new RegionBLL();
            DataTable dt = bll.GetParentRegionTable();
            this.jccmbRegion.DisplayMember = "Region";
            this.jccmbRegion.ValueMember = "Code";
            this.jccmbRegion.DataSource = dt;
        }

        private void BindCmbSubRegion()
        {
            RegionBLL bll = new RegionBLL();

            //RegionBLL bll = new RegionBLL();
            //DataTable dt = bll.GetSubRegionList(this.jccmbRegion.SelectedValue.ToString());
            //this.jccmbSubRegion.DisplayMember = "Region";
            //this.jccmbSubRegion.ValueMember = "Code";
            //this.jccmbSubRegion.DataSource = dt;

            #region 根据品牌限制过滤子区域   --modify on 20170522 by Lingjia Qiu
            DataTable dt = bll.GetSubRegionList(this.jccmbRegion.SelectedValue.ToString());
            List<string> regionList = getRegionListByBrandLimit(dt);

            this.jccmbSubRegion.DisplayMember = "Region";
            this.jccmbSubRegion.ValueMember = "Code";
            if (regionList != null && regionList.Count != 0)
            {
                DataView dv = new DataView(dt);
                string regionStr = "";
                foreach (string s in regionList)
                {
                    regionStr +="'" +s + "',";
                }
                regionStr = regionStr.Substring(0, regionStr.Length - 1);
                dv.RowFilter = "Code not in ("+regionStr+")";
                this.jccmbSubRegion.DataSource = dv;
            }
            else
                this.jccmbSubRegion.DataSource = dt;
            #endregion

        }

        private void BindBrandList()
        {
            this.pnlBrand.Controls.Clear();
            this.pnlSource.Controls.Clear();

            MyProductTypeBLL bll = new MyProductTypeBLL();
            MyDictionaryBLL dicBll = new MyDictionaryBLL();
            List<string> list = bll.GetBrandCodeList(this.jccmbSubRegion.SelectedValue.ToString());
            if (list != null && list.Count > 0)
            {
                //this.gbBrand.Enabled = true;
                int x = 174;
                int y = 15;
                foreach (string s in list)
                {
                    if (!string.IsNullOrEmpty(brandCode) && !brandCode.Equals("ALL"))
                    {
                        if (!s.Equals(brandCode))  //根据品牌限制显示品牌信息
                            continue;
                    }
                   
                    MyDictionary dic = dicBll.GetItem(MyDictionary.DictionaryType.Brand, s);

                    RadioButton rbtn = new RadioButton();
                    rbtn.AutoSize = false;
                    rbtn.Size = new Size(136, 32);
                    rbtn.Location = new Point(x, y);
                    y += 38;

                    rbtn.Text = "";
                    rbtn.Name = "rbtnBrand_" + s;
                    if (s == "Y")
                    {
                        rbtn.Image = Properties.Resources.YORK_logo;
                    }
                    else if (s == "H")
                    {
                        rbtn.Image = Properties.Resources.Hitachi_logo;
                    }
                    rbtn.CheckedChanged += new EventHandler(Brand_Click);
                    this.pnlBrand.Controls.Add(rbtn);

                    if (s == list[0] || s.Equals(brandCode))
                    {
                        rbtn.Checked = true;
                    }
                }
            }
        }

        private void BindFactoryList(string brandCode)
        {
            this.pnlSource.Controls.Clear();
            MyProductTypeBLL bll = new MyProductTypeBLL();
            MyDictionaryBLL dicBll = new MyDictionaryBLL();
            List<string> list = bll.GetFactoryCodeList(brandCode, this.jccmbSubRegion.SelectedValue.ToString());

            if (list != null && list.Count > 0)
            {
                //this.gbSource.Enabled = true;

                int x = 157;
                int y = 26;

                if (!list.Contains("G"))
                {
                    RadioButton rbtn = new RadioButton();
                    this.pnlSource.Controls.Add(rbtn);
                    rbtn.AutoSize = true;
                    rbtn.Font = new System.Drawing.Font("Arial", 12, GraphicsUnit.Pixel);
                    rbtn.Text = "Johnson Controls - Hitachi";
                    rbtn.Name = "rbtnSource_H";
                    rbtn.CheckedChanged += new EventHandler(Factory_Click);
                    rbtn.Location = new Point(x, y);
                    rbtn.Checked = true;
                }
                else
                {
                    if (list.Count > 1)
                    {
                        RadioButton rbtn = new RadioButton();
                        this.pnlSource.Controls.Add(rbtn);
                        rbtn.AutoSize = true;
                        rbtn.Font = new System.Drawing.Font("Arial", 12, GraphicsUnit.Pixel);
                        rbtn.Text = "Johnson Controls - Hitachi";
                        rbtn.Name = "rbtnSource_H";
                        rbtn.CheckedChanged += new EventHandler(Factory_Click);
                        rbtn.Location = new Point(x, y);

                        y += 38;
                        rbtn.Checked = true;
                    }
                    RadioButton rbtn1 = new RadioButton();
                    rbtn1.AutoSize = true;
                    this.pnlSource.Controls.Add(rbtn1);
                    rbtn1.Font = new System.Drawing.Font("Arial", 12, GraphicsUnit.Pixel);
                    rbtn1.Text = "YORK Guangzhou Factory";
                    rbtn1.Name = "rbtnSource_G";
                    rbtn1.CheckedChanged += new EventHandler(Factory_Click);
                    rbtn1.Location = new Point(x, y);

                    if (list.Count == 1)
                        rbtn1.Checked = true;
                }
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (splashScreen != null && this.ISVALID)
            {
                timer1.Enabled = false;
                splashScreen.Dispose();
                this.WindowState = FormWindowState.Normal;
            }
        }


        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            if (dicBrand != null)
            {
                Registration.SelectedBrand = dicBrand;
                Registration.SelectedSubRegion = (new RegionBLL()).GetItem(this.jccmbSubRegion.SelectedValue.ToString());
                Registration.SelectedFactoryCode = FactoryCode;

                if (FactoryCode == "G")
                {
                    // 启动 SVRF 程序
                    //JCBase.UI.JCMsg.ShowInfoOK("will go to Standard-alone VRF!");
                    string file = "NewVRF.exe";
                    //if (CDL.CDL.CheckExistProcess("NewVRF"))
                    //{
                    try
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = file;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.Start();
                    }
                    catch(Exception ex)
                    {
                        JCBase.UI.JCMsg.ShowErrorOK("Failed to start NewVRF.");
                    }
                    //}
                }
                else
                {
                    // 启动 Hitachi 程序
                    frmMain f = new frmMain();
                    f.StartPosition = FormStartPosition.CenterScreen;
                    //this.AddOwnedForm(f);
                    //this.Hide();
                    //在打开MainForm前隐藏Region窗体，关闭MainForm后再打开。 20170818 by Yunxiao Lin4
                    int currentTop = this.Top;
                    this.Top = -65535;
                    this.ShowInTaskbar = false;
                    f.ShowDialog();
                    this.ShowInTaskbar = true;
                    this.Top = currentTop;
                }

            }
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void jcbtnLogOff_Click(object sender, EventArgs e)
        {
            Registration.LogOff();
            frmStart f = new frmStart();
            if (f.ShowDialog() == DialogResult.OK)
                LoadInfo();
        }
        #region 访问远程自动更新接口 20161219 by Yunxiao Lin

        #region ping网络IP
        private static bool Ping(string ip, int TimeOut)
        {
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
            options.DontFragment = true;
            string data = "Test network connection!";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            System.Net.NetworkInformation.PingReply reply = p.Send(ip, TimeOut, buffer, options);
            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success) { return true; }
            else { return false; }
        }
        #endregion
        private void AutoUpdate()
        {
            //调用函数
            //参数集合
            try
            {
                string version = MyConfig.Version;
                if (version != null)
                {
                    //去掉version中的非数字字符
                    string lastch = version.Substring(version.Length - 1, 1);
                    while (string.CompareOrdinal(lastch, "0") < 0 || string.CompareOrdinal(lastch, "9") > 0)
                    {
                        version = version.Remove(version.Length - 1);
                        lastch = version.Substring(version.Length - 1, 1);
                    }
                    ////去掉version中的最后一个"."，如将3.2.0变为3.20，不然更新服务器不识别版本 by Yunxiao Lin 20180526
                    //string[] versionNumbers = version.Split('.');
                    //if (versionNumbers.Count() == 3)
                    //    version = versionNumbers[0] + "." + versionNumbers[1] + versionNumbers[2];
                }
                object[] args = new object[4];
                args[0] = version;
                if (Registration.IsSuperUser())
                    args[1] = "All"; //注意All必须第一个字母大写，后面的小写，不然不更新。
                else
                    args[1] = Registration.GetRegionCode();
                args[2] = "VRF";
                string file = "PatchUpdate.exe";
                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.StartupPath + @"\" + file);
                if (config.AppSettings.Settings["FileType"] == null)
                {
                    return;
                }
                args[3] = config.AppSettings.Settings["FileType"].Value.ToString();

                //20180628 更新服务接口配置文件结构变更 20180628 by Yunxiao Lin
                var kWSRoot = config.AppSettings.Settings["WSPath"];
                var kPatchPath = config.AppSettings.Settings["RootPath"];
                var kIps = config.AppSettings.Settings["IpList"];
                var kTimeOut = config.AppSettings.Settings["TimeOut"];
                if (kWSRoot == null || kPatchPath == null || kIps == null || kTimeOut == null)
                {
                    return;
                }

                string WSRoot = kWSRoot.Value.ToString();//webservics地址
                string PatchPath = kPatchPath.Value.ToString();//补丁下载路径
                string Ips = kIps.Value.ToString();//ip集合
                int TimeOut = int.Parse(kTimeOut.Value.ToString());//链接超时

                List<string> IpList = Ips.ToString().Split(';').ToList();
                bool HasConifgrue = false;
                //在连接之前先ping
                foreach (string ip in IpList)
                {
                    if (ip != "" && Ping(ip, TimeOut))
                    {
                        WSRoot = string.Format(WSRoot, ip);
                        PatchPath = string.Format(PatchPath, ip);
                        HasConifgrue = true;
                        break;
                    }
                }
                //如果都ping不通则退出更新
                if (!HasConifgrue)
                {
                    return;
                }

                //设置参数
                string pargs = args[0] + " " + args[1] + " " + args[2];

                //添加代理，不然在外网会连不上服务
                System.Net.WebProxy webProxy = new System.Net.WebProxy(WSRoot, true);
                webProxy.Credentials = new System.Net.NetworkCredential("1", "1");
                System.Net.WebRequest.DefaultWebProxy = webProxy;

                //连接服务
                object result = WebservicesHelper.InvokeWebService(WSRoot, "HasPatch", args);

                if (result != null)
                {
                    bool HasPatch = (bool)result;
                    if (HasPatch)
                    {
                        //启动自动更新客户端
                        //MessageBox.Show("AutoUpdate");
                        Process.Start(Application.StartupPath + @"\" + file, pargs);
                        //Process.Start(@"C:\SVN\GVRF\DesktopVRF\PatchUpdate\bin\Debug\"+file, pargs);
                        //Process.Start(@"C:\SVN\GVRF\DesktopVRF\PatchUpdate\bin\Debug\" + file);
                    }
                }
            }
            catch (Exception)
            { 
                return;
            }
        }
        #endregion

        /// 根据品牌限制获取过滤区域
        /// <summary>
        /// 根据品牌限制获取过滤区域   --add on 20170522 by Lingjia Qiu
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private List<string> getRegionListByBrandLimit(DataTable dt)
        {
            List<string> regionList = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                MyProductTypeBLL bll = new MyProductTypeBLL();                
                string regionCode = dr["Code"].ToString();
                List<string> brandList = bll.GetBrandCodeList(regionCode);
                if (brandList.Count == 1)
                {
                    if (!string.IsNullOrEmpty(brandCode) && !brandCode.Equals("ALL"))
                    {
                        if (!brandCode.Equals(brandList[0].ToString()))
                            regionList.Add(regionCode);
                    }
                    else
                        return null;                       
                }

            }

            return regionList;
        }
    }
}
