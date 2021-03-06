﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbiz.Library
{
    #region Delegates
    public delegate void SbizModelChanged_Delegate(object sender, SbizModelChanged_EventArgs args);
    public delegate void SbizUpdateView_Delegate(object sender, SbizModelChanged_EventArgs args);
    #endregion

    public class SbizModelChanged_EventArgs : EventArgs
    {
        private int _status;
        private string _error_message;
        private Object _extra_arg;

        public const int CONNECTED = 1;
        public const int TRYING = 2;
        public const int NOT_CONNECTED = 0;
        public const int DISCOVERED_SERVER = 3;
        public const int CLIPBOARD_UPDATE = 30;
        public const int TARGET = 10;
        public const int NOT_TARGET = 11;
        #region Errors
        public const int NOT_LISTENING = -2;
        public const int ERROR = -1;
        public const int PEER_SHUTDOWN = -3;
        public const int AUTH_FAILED = -4;
        #endregion
        public int Status
        {
            get
            {
                return _status;
            }
        }

        public string Error_message
        {
            get
            {
                return _error_message;
            }
        }

        public Object ExtraArg
        {
            get
            {
                return _extra_arg;
            }
        }

        public SbizModelChanged_EventArgs(int status)
        {
            this._status = status;
        }

        public SbizModelChanged_EventArgs(int status, string error_message)
        {
            //TODO: logger
            this._status = status;
            this._error_message = error_message;
        }

        public SbizModelChanged_EventArgs(int status, string error_message, object extra_arg)
        {
            //TODO: logger
            this._status = status;
            this._error_message = error_message;
            this._extra_arg = extra_arg;
        }
    }
}
