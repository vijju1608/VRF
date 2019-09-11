using System;
using System.Configuration;
using System.Reflection;

namespace JCHVRF.DALFactory
{
    public class GetDatabase
    {
        public IDataAccessObject GetDataAccessObject()
        {
            string assemblyName = ConfigurationManager.AppSettings["AssemblyName"];
            return (IDataAccessObject)Assembly.Load(assemblyName).CreateInstance(ConfigurationManager.AppSettings["DAO"], false);
        }
    }
}
