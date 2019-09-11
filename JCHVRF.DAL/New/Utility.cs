using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace JCHVRF.DAL.New
{
    public static class Utility
    {
        /// <summary>
        /// Function to Deserialize Blob Data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] param)
        {
            using (MemoryStream ms = new MemoryStream(param))
            {              
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }

        public static byte[] Serialize<T>(T obj)
        {
            byte[] serializedData;
            using (MemoryStream objstream = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(objstream, obj);
                serializedData = objstream.ToArray();
            }
            return serializedData;
        }


    }
}
