using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SbizLibrary
{
    class SbizMessageConst
    {
        public static int KEY_PRESS = 1;
        //vari tipi di messaggi da mandare
    }

    [Serializable]
    class SbizMessage
    {
        private int _code;
        public int Code
        {
            get
            {
                return _code;
            }
        }

        private byte[] _data;
        public byte[] Data
        {
            get
            {
                return _data;
            }
        }

        public SbizMessage(int code, byte[] data)
        {
            _code = code;
            _data = data;
        }

        public SbizMessage(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            BinaryFormatter bf = new BinaryFormatter();

            ms.Position = 0;

            SbizMessage o = bf.Deserialize(ms) as SbizMessage;

            if (o == null)
            {
                throw new ArgumentNullException();
            }

            this._code = o.Code;
            this._data = o.Data;
        }

        public byte[] ToByteArray()
        {
            if (this == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }
    }
}
