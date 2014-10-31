using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbiz.Library
{
    public class SbizMessageConst
    {
        public static int KEEPALIVE = 0;
        public static int KEY_PRESS = 1;
        //vari tipi di messaggi da mandare
    }

    [Serializable]
    public class SbizMessage
    {
        #region Attributes

        private int _code;
        private byte[] _data;

        #endregion


        #region Properties
        public int Code
        {
            get
            {
                return _code;
            }
        }
        public byte[] Data
        {
            get
            {
                return _data;
            }
        }
        #endregion


        #region StaticMethods
        public static byte[] KeepAliveMessage(){
            SbizMessage kam = new SbizMessage(SbizMessageConst.KEEPALIVE, Encoding.UTF8.GetBytes("alive"));

            return kam.ToByteArray();
        }
        #endregion


        #region Constructors
        public SbizMessage(int code, byte[] data)
        {
            _code = code;
            _data = data;
        }
        public SbizMessage(byte[] data)
        {
            SbizMessage m = SbizBasic.DeserializeByteArray(data) as SbizMessage;           

            if (m == null)
            {
                throw new ArgumentNullException();
            }

            this._code = m.Code;
            this._data = m.Data;
        }
        #endregion


        #region InstanceMethods
        public byte[] ToByteArray()
        {
            return SbizBasic.SerializeObject(this);
        }
        #endregion

    }
}
