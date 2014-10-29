using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Sbiz.Library
{
    public static class SbizBasic
    {
        public static byte[] SerializeObject(Object o)
        {
            if (o == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                return ms.ToArray();
            }
        }
        public static Object DeserializeByteArray(byte[] array)
        {
            MemoryStream ms = new MemoryStream(array);
            BinaryFormatter bf = new BinaryFormatter();

            ms.Position = 0;

            Object o;

            try
            {
                o = bf.Deserialize(ms);
            }
            catch (Exception e)
            {
                throw e;
            }

            return o;
        }
    }
}
