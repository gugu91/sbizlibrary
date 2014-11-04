using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Sbiz.Library
{
    public static class SbizNetUtils
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
                //TODO non propagare l'eccezione ma scrivere sul log formato dati non corretto
                
                throw e;
            }

            return o;
        }

        public static byte[] EncapsulateInt32inByteArray(byte[] data, Int32 n)
        {
            byte[] n_tobytearray = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(n));//size of the message is sent
            byte[] buffer = new byte[n_tobytearray.Length + data.Length];
            n_tobytearray.CopyTo(buffer, 0);
            data.CopyTo(buffer, n_tobytearray.Length);

            return buffer;
        }

        public static Int32 DecapsulateInt32FromByteArray(ref byte[] data)
        {
            int seek = 0;
            byte[] n_tobytearray = new byte[sizeof(Int32)];
            Array.Copy(data, seek, n_tobytearray, 0, n_tobytearray.Length);
            seek += n_tobytearray.Length;
            Int32 n = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(n_tobytearray, 0));

            byte[] buffer = new byte[n];
            Array.Copy(data, seek, buffer, 0, n);

            data = buffer;

            return n;
        }
    }
}
