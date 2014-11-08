using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbiz.Library
{
    public static class SbizMessageConst
    {
        public const Int32 ANNOUNCE = 0;
        public const Int32 KEY_PRESS = 1;
        public const Int32 KEY_DOWN = 2;
        public const Int32 KEY_UP = 3;
        //Other Key events...
        public const Int32 MOUSE_ENTER = 20;
        public const Int32 MOUSE_MOVE = 21;
        public const Int32 MOUSE_HOVER = 22;
        public const Int32 MOUSE_DOWN = 23;
        public const Int32 MOUSE_WHEEL = 24;
        public const Int32 MOUSE_UP = 25;
        public const Int32 MOUSE_LEAVE = 26;
        //CLipboard Events...
    }

    [Serializable]
    public class SbizMessage
    {
        #region Attributes

        private Int32 _code;
        private byte[] _data;

        #endregion


        #region Properties
        public Int32 Code
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
        public static byte[] AnnounceMessage(Int32 TCPport){
            byte[] buffer = new byte[0];
            SbizNetUtils.EncapsulateInt32inByteArray(buffer, TCPport);
            SbizMessage kam = new SbizMessage(SbizMessageConst.ANNOUNCE, buffer);

            return kam.ToByteArray();
        }
        #endregion


        #region Constructors
        public SbizMessage(Int32 code, byte[] data)
        {
            _code = code;
            _data = data;
        }
        public SbizMessage(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            int code = SbizNetUtils.DecapsulateInt32FromByteArray(ref data);

            this._code = code;
            this._data = data;
        }
        #endregion


        #region InstanceMethods
        public byte[] ToByteArray()
        {
            return SbizNetUtils.EncapsulateInt32inByteArray(_data, _code);
        }
        #endregion

    }
}
