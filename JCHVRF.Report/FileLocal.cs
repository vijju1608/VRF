//********************************************************************
// 文件名: WordUtil.cs
// 描述: 封装对 Word 的一些操作
// 作者: clh
// 创建时间: 2012-04-13 
// 修改历史: 
//********************************************************************

using System;
using System.Collections.Generic;
using System.IO;

using JCBase.Util;
using JCBase.UI;
using System.Drawing;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCBase.Utility;

namespace JCHVRF.Report
{
    public class FileLocal
    {
        /// 获取指定系统的配管图截图文件名称，用于报告中插入图片
        /// <summary>
        /// 获取指定系统的配管图截图文件名称，用于报告中插入图片
        /// </summary>
        /// <param name="systemNO"></param>
        /// <returns></returns>
        //public static string GetNamePathPipingPicture(string systemNO, string dir = null)
        //{
        //    FileUtil util = new FileUtil();
        //    FileLocal objFileLocal = new FileLocal();
        //    string reportPathPipingImage = objFileLocal.GetPipingImagePath();
        //    return reportPathPipingImage;
        //}

        //public string GetPipingImagePath()
        //{
        //    string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        //    string navigateToFolder = "..\\..\\Report\\ProjectFiles\\1_piping.jpeg";
        //    string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
        //    return sourceDir;
        //}

        //read value from App.config Value

        public static string GetNamePathPipingPicture(string systemNO, string dir = null)
        {
            FileUtil util = new FileUtil();
            string name = systemNO + "_piping.svg";
            if (string.IsNullOrEmpty(dir))
            {
                dir = MyConfig.ProjectFileDirectory;
            }
            return util.GetFullPathName(dir, name);
        }
        #region 
        public static string  GetBinDirectoryPath(string AppSettingPath)
        {
            string binDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').ToString();
            binDirectory += AppSettingPath;
            return binDirectory;
        }
        #endregion


        public string GetWiringImagePath()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\ProjectFiles\\1_wiring.svg";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        /// 获取指定系统的带路径的配线图文件名，用于报告中插入图片
        /// <summary>
        /// 获取指定系统的带路径的配线图文件名，用于报告中插入图片
        /// </summary>
        /// <param name="systemNO">系统ID属性</param>
        /// <returns></returns>
        //public static string GetNamePathWiringPicture(string systemNO, string dir = null)
        //{
        //    FileUtil util = new FileUtil();
        //    FileLocal objFileLocal = new FileLocal();
        //    string reportPathWiringImage = objFileLocal.GetWiringImagePath();
        //    return reportPathWiringImage;
        //    //string name = systemNO + "_wiring.jpeg";
        //    //if (string.IsNullOrEmpty(dir))
        //    //{
        //    //    dir = MyConfig.ProjectFileDirectory;
        //    //}
        //    // return util.GetFullPathName(dir, name);
        //}
        public static string GetNamePathWiringPicture(string systemNO, string dir = null)
        {
            FileUtil util = new FileUtil();
            string name = systemNO + "_wiring.svg";
            if (string.IsNullOrEmpty(dir))
            {
                dir = MyConfig.ProjectFileDirectory;
            }
            return util.GetFullPathName(dir, name);
        }



        /// 获取控制器界面截图文件，用于报告中插入图片
        /// <summary>
        /// 获取控制器界面截图文件，用于报告中插入图片
        /// </summary>
        /// <returns></returns>
        public static string[] GetNamePathControllerPictures()
        {
            string[] nameArr = Directory.GetFiles(MyConfig.ProjectFileDirectory, "*Controller_Drawing_*.jpeg");
            return nameArr;
        }

        /// 获取指定序号的控制器界面截图文件
        /// <summary>
        /// 获取指定序号的控制器界面截图文件
        /// </summary>
        /// <param name="NO"></param>
        /// <returns></returns>
        public static string GetNamePathControllerPicture(string NO)
        {
            FileUtil util = new FileUtil();
            string name = "Controller_Drawing_" + NO + ".jpeg";
            return util.GetFullPathName(MyConfig.ProjectFileDirectory, name);
        }

        /// 获取指定序号的控制器界面截图文件
        /// <summary>
        /// 获取指定序号的控制器界面截图文件
        /// </summary>
        /// <param name="NO"></param>
        /// <returns></returns>
        public static string GetNamePathControllerWiringPicture(string groupId, string dir = null)
        {
            FileUtil util = new FileUtil();
            string name = "Controller_Wiring_" + groupId + ".jpeg";
            if (string.IsNullOrEmpty(dir))
            {
                dir = MyConfig.ProjectFileDirectory;
            }
            return util.GetFullPathName(dir, name);
        }

        /*
        /// <summary>
        /// 导入某项目文件至 ProjectFile 文件夹
        /// </summary>
        /// <param name="prjFileWithPath">需要导入的项目文件</param>
        /// <returns>返回目标文件路径</returns>
        //public static string ImportProjFile(string prjFileWithPath)
        //{
        //    string dstDir = MyConfig.ProjectFileDirectory;
        //    return ExportProjFile(prjFileWithPath, dstDir);
        //}

        /// <summary>
        /// 导出某项目文件至指定文件夹
        /// </summary>
        /// <param name="prjFileWithPath">源文件完整路径名</param>
        /// <param name="dstDir">目标路径</param>
        /// <returns>返回目标路径下的文件完整路径名</returns>
        //public static string ExportProjFile(string prjFileWithPath, string dstDir)
        //{
        //    FileUtil util = new FileUtil();
        //    // 得到当前项目的文件名
        //    string fname = Path.GetFileName(prjFileWithPath);
        //    string fnameWithoutExt = Path.GetFileNameWithoutExtension(prjFileWithPath);
        //    string fDir = Path.GetDirectoryName(prjFileWithPath);
        //    // 目标路径名
        //    string dstFullName = util.GetFullPathName(dstDir, fname);

        //    // 源文件与目标路径不相同
        //    if (!util.IsSameDir(fDir, dstDir))
        //    {
        //        // 若目标路径下存在同名项目文件
        //        if (File.Exists(dstFullName))
        //        {
        //            // 确认是否允许覆盖，则退出！
        //            if (JCMsg.ShowConfirmOKCancel(Msg.Confirm_File_OverWrite(fname)) 
        //                == System.Windows.Forms.DialogResult.OK)
        //                DeleteProjFile(dstFullName);
        //            else
        //                return prjFileWithPath;
        //        }
        //        util.ImportFiles(Directory.GetFiles(fDir, fnameWithoutExt + "*"), dstDir);
        //        return dstFullName;
        //    }

        //    return prjFileWithPath;
        //}

        /// <summary>
        /// 删除指定的项目文件，连同相关的配管图文件一起删除
        /// </summary>
        /// <param name="fileWithPath"></param>
        //public static void DeleteProjFile(string fileWithPath)
        //{
        //    string dir = Path.GetDirectoryName(fileWithPath);
        //    string fnameWithoutExt = Path.GetFileNameWithoutExtension(fileWithPath);
        //    // 删除目标路径下已存在的项目文件，包括配管文件
        //    string[] files = Directory.GetFiles(dir, fnameWithoutExt + "*");
        //    foreach (string f in files)
        //        File.Delete(f);
        //}

        /// <summary>
        /// 判断是否有同名项目文件存在！
        /// </summary>
        /// <param name="projName"></param>
        /// <param name="showWarningMsg"></param>
        /// <returns></returns>
        //public static bool IsProjNameExist(string projName, bool showWarningMsg)
        //{
        //    bool ret = File.Exists(GetProjNamePath(projName));
        //    if (ret && showWarningMsg)
        //        JCMsg.ShowWarningOK(Msg.EXIST(projName));
        //    return ret;
        //}

        /// <summary>
        /// 得到带路径的项目名称
        /// </summary>
        /// <param name="projName"></param>
        /// <returns></returns>
        //public static string GetProjNamePath(string projName)
        //{
        //    FileUtil util = new FileUtil();
        //    if (!projName.Contains(".vsp"))
        //        projName += ".vsp";
        //    return util.GetFullPathName(MyConfig.ProjectFileDirectory, projName);
        //}

        // 获取VRF项目文件名数组
        /// <summary>
        /// 获取VRF项目文件名数组
        /// </summary>
        /// <returns></returns>
        //public static string[] GetProjFiles()
        //{
        //    string dir = MyConfig.ProjectFileDirectory;
        //    return Directory.GetFiles(dir, "*.vsp");
        //}
*/



    }
}
