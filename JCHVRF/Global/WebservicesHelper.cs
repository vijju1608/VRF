using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.CodeDom;
using System.Web.Services;
using System.Web.Services.Description;
using JCBase.Utility;

namespace JCHVRF
{
    //webservices操作类
    public static class WebservicesHelper
    {
        /*
          *  调用实例
             object[] args = new object[1];
              args[0] = planid;
              object result = WebservicesHelper.InvokeWebService("http://c-wuxts01:8888/PRM_API/PRM_API.asmx", "Return_PlanData", args);
             return (DataSet)result;
      */
        /// < summary>           
        /// 动态调用web服务         
        /// < /summary>          
        /// < param name="url">WSDL服务地址< /param> 
        /// < param name="methodname">方法名< /param>           
        /// < param name="args">参数< /param>           
        /// < returns>< /returns>          
        public static object InvokeWebService(string url, string methodname, object[] args)
        {
            try
            {
                return WebservicesHelper.InvokeWebService(url, null, methodname, args);
            }
            catch (Exception ex)
            {
                LogHelp.WriteLog("InvokeWebService异常,url:" + url + "methodname" + methodname, ex);
                return null;
            }
        }


        /// <summary>
        /// 动态调用web服务
        /// </summary>
        /// <param name="url">WSDL服务地址</param>
        /// <param name="classname">服务接口类名</param>
        /// <param name="methodname">方法名</param>
        /// <param name="args">参数值</param>
        /// <returns></returns>
        public static object InvokeWebService(string url, string classname, string methodname, object[] args)
        {

            string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";
            if ((classname == null) || (classname == ""))
            {
                classname = WebservicesHelper.GetWsClassName(url);
            }
            try
            {

                //获取WSDL   
                WebClient wc = new WebClient();
                Stream stream = wc.OpenRead(url + "?WSDL");
                ServiceDescription sd = ServiceDescription.Read(stream);
                //注意classname一定要赋值获取 
                classname = sd.Services[0].Name;

                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                CodeNamespace cn = new CodeNamespace(@namespace);

                //生成客户端代理类代码          
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider icc = new CSharpCodeProvider();


                //设定编译参数                 
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");
                //编译代理类                 
                CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }
                //生成代理实例，并调用方法                 
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                Type t = assembly.GetType(@namespace + "." + classname, true, true);
                object obj = Activator.CreateInstance(t);
                System.Reflection.MethodInfo mi = t.GetMethod(methodname);
                return mi.Invoke(obj, args);

            }
            catch (Exception ex)
            {
                LogHelp.WriteLog("InvokeWebService异常", ex);
                return null;
            }
        }

        private static string GetWsClassName(string wsUrl)
        {
            string[] parts = wsUrl.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');
            return pps[0];
        }
    }
}
