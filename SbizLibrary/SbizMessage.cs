﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Sbiz.Library
{
    public delegate void SbizMessageSending_Delegate(SbizMessage m, SbizModelChanged_Delegate del);

    public static class SbizMessageConst
    {
        #region General (0, 98, 99)
        public const Int32 ANNOUNCE = 0;
        public const Int32 AUTHENTICATE = 10;
        public const Int32 TARGET = 99;
        public const Int32 NOT_TARGET = 98;
        #endregion

        #region Keyboard (1-3)
        public const Int32 KEY_PRESS = 1;
        public const Int32 KEY_DOWN = 2;
        public const Int32 KEY_UP = 3;
        #endregion

        #region Mouse (20-26)
        public const Int32 MOUSE_ENTER = 20;
        public const Int32 MOUSE_MOVE = 21;
        public const Int32 MOUSE_HOVER = 22;
        public const Int32 MOUSE_DOWN = 23;
        public const Int32 MOUSE_WHEEL = 24;
        public const Int32 MOUSE_UP = 25;
        public const Int32 MOUSE_LEAVE = 26;
        #endregion

        #region Clipboard (30-36)
        public const Int32 CLIPBOARD_AUDIO = 30;
        public const Int32 CLIPBOARD_FILE = 31;
        public const Int32 CLIPBOARD_IMG = 32;
        public const Int32 CLIPBOARD_UNICODETEXT = 33;
        public const Int32 CLIPBOARD_TEXT = 36;
        public const Int32 CLIPBOARD_START = 34;
        public const Int32 CLIPBOARD_STOP = 35;
        #endregion

        public static bool IsClipboardConst(Int32 c)
        {
            if (c == SbizMessageConst.CLIPBOARD_UNICODETEXT || c == SbizMessageConst.CLIPBOARD_IMG||
                c == SbizMessageConst.CLIPBOARD_FILE || c == SbizMessageConst.CLIPBOARD_AUDIO||
                c == SbizMessageConst.CLIPBOARD_START || c == SbizMessageConst.CLIPBOARD_STOP ||
                c == SbizMessageConst.CLIPBOARD_TEXT)
            {
                return true;
            }

            return false;
        }
        public static bool IsMouseConst(Int32 c)
        {
            if (c == SbizMessageConst.MOUSE_MOVE || c == SbizMessageConst.MOUSE_UP ||
                c == SbizMessageConst.MOUSE_DOWN || c == SbizMessageConst.MOUSE_WHEEL)
            {
                return true;
            }

            return false;
        }
        public static bool IsKeyConst(Int32 c)
        {
            if (c == SbizMessageConst.KEY_PRESS || c == SbizMessageConst.KEY_DOWN ||
                c == SbizMessageConst.KEY_UP)
            {
                return true;
            }

            return false;
        }
    }

    public class SbizAnnounce
    {
        #region Attributes
        private Int32 _tcp_port;
        private string _name;
        #endregion

        #region Properties
        public Int32 TCPPort
        {
            get
            {
                return _tcp_port;
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
        }
        #endregion

        #region Constructor
        public SbizAnnounce(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            SbizMessage m = new SbizMessage(data);
            byte[] buffer = m.Data;
            _tcp_port = SbizNetUtils.DecapsulateInt32FromByteArray(ref buffer);
            _name = Encoding.UTF8.GetString(buffer);   
        }
        #endregion

        #region StaticMethods
        public static byte[] NewToByteArray(String name, Int32 TCPport)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(name);
            buffer = SbizNetUtils.EncapsulateInt32inByteArray(buffer, TCPport);
            SbizMessage kam = new SbizMessage(SbizMessageConst.ANNOUNCE, buffer);
            

            return kam.ToByteArray();
        }
        #endregion
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

        #region Static Methods
        public static byte[] TargetToByteArray()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("TARGET");
            SbizMessage kam = new SbizMessage(SbizMessageConst.TARGET, buffer);

            return kam.ToByteArray();
        }
        public static byte[] NotTargetToByteArray()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("NOT_TARGET");
            SbizMessage kam = new SbizMessage(SbizMessageConst.NOT_TARGET, buffer);

            return kam.ToByteArray();
        }

        public static byte[] AuthenticationMessage(string key, System.Net.IPEndPoint ipe)
        {
            SbizMessage kam = new SbizMessage(SbizMessageConst.AUTHENTICATE, AutenticationPayload(key, ipe));
            return kam.ToByteArray();
        }

        public static byte[] AutenticationPayload(string key, System.Net.IPEndPoint ipe)
        {
            string secret_payload = ipe.Address.ToString() + ":" + ipe.Port.ToString();
            using (HMACMD5 hmac = new HMACMD5(Encoding.UTF8.GetBytes(key)))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(secret_payload));
            }
        }
        #endregion

    }
}
