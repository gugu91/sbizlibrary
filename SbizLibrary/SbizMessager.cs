using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Sbiz.Library;

namespace Sbiz.Client
{
    public delegate void SbizMessageHandle_Delegate(SbizMessage m);
    public class SbizMessager
    {
        #region Attributes
        private Socket s_listen;
        private Socket s_conn;
        private IPAddress _ip_add;
        private int _tcp_port;
        private SbizMessageHandle_Delegate _message_handle;
        #endregion

        #region Thread Safe Properties
        private int _connected; //NB never refer to this object as it is not thread safe
        private int _listening;
        private const int YES = 1;
        private const int NO = 0;
        public bool Connected
        {
            get
            {
                if (_connected == YES)
                {
                    return true;
                }
                else return false;
            }
            set
            {
                if (value)
                {
                    System.Threading.Interlocked.Exchange(ref _connected, YES);
                }
                else
                {
                    System.Threading.Interlocked.Exchange(ref _connected, NO);
                }
            }
        }
        public bool Listening
        {
            get
            {
                if (_listening == YES) return true;
                else return false;
            }
            set
            {
                if (value)
                {
                    System.Threading.Interlocked.Exchange(ref _listening, YES);
                }
                else
                {
                    System.Threading.Interlocked.Exchange(ref _listening, NO);
                }
            }
        }
        #endregion

        public string Identifier
        {
            get
            {
                return _ip_add.ToString() + ":" + _tcp_port.ToString();
            }
        }

        #region Constructors
        public SbizMessager() // SERVER call this construstor to istanciate a server
        {
            s_listen = null;
            s_conn = null;
        }
        public SbizMessager(IPAddress ip, int tcp_port) // CLIENT call this constructor to istanciate a client
        {
            _ip_add = ip;
            _tcp_port = tcp_port;
            Connected = false;
        }
        #endregion

        #region Client Methods
        public void ConnectToServer(SbizModelChanged_Delegate model_changed)
        {
            IPEndPoint ipe = new IPEndPoint(_ip_add, _tcp_port);

            s_conn = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (model_changed != null) model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.TRYING));
            try
            {
                s_conn.BeginConnect(ipe, ConnectCallback, new StateObject(s_conn, model_changed));
            }
            catch(SocketException)
            {
                Connected = false;
                if(model_changed != null) model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.ERROR, 
                    "There is no server listening on this port", this.Identifier));
            }
        } // CLIENT connects to an other istance of this class configured as server
        public void ShutdownConnectionWithServer(SbizModelChanged_Delegate model_changed)
        {
            if (Connected)
            {
                SbizClipboardHandler.UnregisterSbizMessageSendingDelegate(this.SendMessage);
                s_conn.Shutdown(SocketShutdown.Both);
                s_conn.Close();

                Connected = false;
                if (model_changed != null) model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.NOT_CONNECTED,
                    "Not connected to server", this.Identifier));
            }
        } // CLIENT shuts down connection with the server
        #endregion

        public void SendData(byte[] data, SbizModelChanged_Delegate model_changed)
        {
            try
            {
                /* NB there was previously a protocol error as size of the data buffer was not sent, causing
                 * some data to not be processed by server.
                 */
                byte[] buffer = SbizNetUtils.EncapsulateInt32inByteArray(data, data.Length);
                s_conn.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, new StateObject(s_conn, model_changed));
                //s_conn.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            catch (SocketException)
            {
                if (Connected)
                {
                    Connected = false;
                    if (model_changed != null) model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.ERROR, 
                        "Server disconnected", this.Identifier));
                }
            }
        }

        public void SendMessage(SbizMessage m, SbizModelChanged_Delegate model_changed)
        {
            SendData(m.ToByteArray(), model_changed);
        }

        #region Async Callbacks
        private void SendCallback(IAsyncResult ar)
        {
            if (Connected)
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.socket;
                int nbyte;
                try
                {
                    nbyte = handler.EndSend(ar);
                }
                catch (SocketException)
                {
                    nbyte = -1;
                }

                if (nbyte < 0 && Connected)
                {
                    Connected = false;
                    if (state.model_changed != null) state.model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.ERROR,
                        "Server disconnected", this.Identifier));
                }
            }
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            try
            {
                Socket s = state.socket;
                s.EndConnect(ar);
                s.NoDelay = true;
                Connected = true;
                SbizClipboardHandler.RegisterSbizMessageSendingDelegate(this.SendMessage);//Start Sniffing the clipboard
                if (state.model_changed != null) state.model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.CONNECTED,
                    "Connected to server", this.Identifier));
            }
            catch (SocketException)
            {
                Connected = false;
                if (state.model_changed != null) state.model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.ERROR, 
                    "There is no server listening on this port", this.Identifier));
            }
            catch (ObjectDisposedException)
            {
                Connected = false;
                //User shutdown connection, do nothing
            }
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            if (Listening)
            {
                StateObject state = (StateObject) ar.AsyncState;
                Socket listener = state.socket;
                Socket handler = null;
                try
                {
                    handler = listener.EndAccept(ar);
                }
                catch (ObjectDisposedException ode) //user changed port
                {
                    SbizLogger.Logger = "User changed port";
                    return;
                }
                
                s_conn = handler;

                if(state.model_changed != null) state.model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.CONNECTED));

                ReceiveMessageSize(handler, state.model_changed);
            }
        }
        private void ReadCallback(IAsyncResult ar)
        {
            if (Listening)
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;

                Socket handler = state.socket;

                int bytesRead;
                try
                {
                    // Read data from the client socket. 
                    bytesRead = handler.EndReceive(ar);
                }

                catch (Exception) //user stopped connection
                {
                    SbizLogger.Logger = "User stopped connection";
                    bytesRead = -1;
                }

                if (bytesRead > 0)
                {
                    /* NB there was previously a protocol error as size of the data buffer was not sent, causing
                     * some data to not be processed by server.
                     */
                    StateObject state_out = new StateObject(handler, state.model_changed);

                    if (state.seek == 0) //Received the size of the subsequent message
                    { 
                        state_out.datasize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(state.data, 0));
                        state_out.seek = sizeof(Int32);
                    }
                    else if(state.seek == sizeof(Int32)) //received a sbiz message
                    {
                        var m = new SbizMessage(state.data);
                        if (SbizMessageConst.IsClipboardConst(m.Code)) SbizClipboardHandler.HandleClipboardSbizMessage(m);
                        else if(_message_handle != null) _message_handle(m);

                        state_out.datasize = sizeof(Int32);
                        state_out.seek = 0;
                    }

                    state_out.data = new byte[state_out.datasize];

                    handler.BeginReceive(state_out.data, 0, state_out.datasize, 0,
                        new AsyncCallback(ReadCallback), state_out);
                }
                else//clientshutdown
                {
                    if (state.model_changed != null) state.model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.NOT_CONNECTED));
                    s_conn = null;
                    s_listen.BeginAccept(AcceptCallback, new StateObject(s_listen, state.model_changed));
                }
            }
        }
        private void ReceiveMessageSize(Socket handler, SbizModelChanged_Delegate model_changed)
        {
            // Create the state object.
            StateObject state_out = new StateObject(handler, model_changed);
            state_out.socket = handler;
            state_out.datasize = sizeof(Int32);
            state_out.seek = 0;
            state_out.data = new byte[state_out.datasize];
            handler.BeginReceive(state_out.data, 0, state_out.datasize, 0,
                new AsyncCallback(ReadCallback), state_out);
        }
        #endregion

        #region Server Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port_p">TCP port on which listen</param>
        public void Listen(int port_p)
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, port_p);
            s_listen = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                s_listen.Bind(ipe);
                s_listen.Listen(100);
            }
            catch (Exception e)//TODO handle bind exception
            {
                throw e;
            }
        }
        /// <summary>
        /// Starts accepting and serving connections
        /// </summary>
        public void StartServer(SbizModelChanged_Delegate model_changed)
        {
            Listening = true;

            var state = new StateObject(s_listen, model_changed);
            s_listen.BeginAccept(AcceptCallback, state);
        }
        /// <summary>
        /// Stops accepting and serving connections
        /// </summary>
        public void StopServer(SbizModelChanged_Delegate model_changed)
        {
            Listening = false;

            if (s_conn != null)
            {
                s_conn.Shutdown(SocketShutdown.Both);
                s_conn.Close();
                s_conn = null;
            }
            if (s_listen != null)
            {
                s_listen.Close();
            }
            model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.NOT_LISTENING));
        }
        #endregion

        private class StateObject
        {
            public Socket socket;
            public int datasize;
            public int seek;
            public SbizModelChanged_Delegate model_changed;
            // Receive buffer.
            public byte[] data;

            public StateObject(Socket socket, SbizModelChanged_Delegate model_changed)
            {
                this.socket = socket;
                this.model_changed = model_changed;
            }
        }
    }
}
