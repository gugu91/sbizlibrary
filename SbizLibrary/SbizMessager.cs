﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Sbiz.Library;

namespace Sbiz.Library
{
    public delegate void SbizMessageHandle_Delegate(SbizMessage m);
    public class SbizMessager
    {
        #region Attributes
        private Socket s_listen;
        private Socket s_conn;
        private IPAddress _ip_add;
        private int _tcp_port;
        private IntPtr _view_handle;
        #endregion

        #region Thread Safe Properties
        private const int YES = 1;
        private const int NO = 0;
        private int _connected = NO; //NB never refer to this object as it is not thread safe
        private int _listening = NO;
        private static int _authenticated = NO;
        public static bool Authenticated
        {
            get
            {
                if (_authenticated == YES) return true;
                else return false;
            }
            set
            {
                if (value)
                {
                    System.Threading.Interlocked.Exchange(ref _authenticated, YES);
                }
                else
                {
                    System.Threading.Interlocked.Exchange(ref _authenticated, NO);
                }
            }
        } //NB If connected is set to false also authenticated automatically
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
                    if (_connected == NO)
                    {
                        SbizClipboardHandler.RegisterSbizMessageSendingDelegate(this.SendMessage);//Start Sniffing the clipboard
                        System.Threading.Interlocked.Exchange(ref _connected, YES);
                    }

                }
                else
                {
                    if (_connected == YES)
                    {
                        SbizClipboardHandler.UnregisterSbizMessageSendingDelegate(this.SendMessage);//Stop Sniffing the clipboard
                        System.Threading.Interlocked.Exchange(ref _connected, NO);
                        Authenticated = false;
                    }

                }
            }
        }  //NB If Listening is set to false also authenticated automatically
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
                    Connected = false;
                    System.Threading.Interlocked.Exchange(ref _listening, NO);
                }
            }
        }
        #endregion

        #region Properties
        public string Identifier
        {
            get
            {
                if (_ip_add == null) return null;
                return _ip_add.ToString() + ":" + _tcp_port.ToString();
            }
        }
        #endregion

        #region Message Handle Delegate
        private SbizMessageHandle_Delegate _message_handle;
        public void RegisterMessageHandle(SbizMessageHandle_Delegate del)
        {
            _message_handle += del;
        }
        public void UnregisterMessageHandle(SbizMessageHandle_Delegate del)
        {
            _message_handle -= del;
        }
        #endregion

        #region Constructors
        public SbizMessager() // SERVER call this construstor to istanciate a server
        {
            s_listen = null;
            s_conn = null;
            Authenticated = false;
            Connected = false;
            Listening = false;
        }
        public SbizMessager(IPAddress ip, int tcp_port) // CLIENT call this constructor to istanciate a client
        {
            _ip_add = ip;
            _tcp_port = tcp_port;
            s_listen = null;
            s_conn = null;
            Authenticated = false;
            Connected = false;
            Listening = false;
        }
        #endregion

        #region Client Methods
        public void ConnectToServer(SbizModelChanged_Delegate model_changed, IntPtr view_handle, string key)
        {
            IPEndPoint ipe = new IPEndPoint(_ip_add, _tcp_port);

            s_conn = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (model_changed != null) model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.TRYING));
            try
            {
                var state = new StateObject(s_conn, model_changed, view_handle, key);
                s_conn.BeginConnect(ipe, ConnectCallback, state);
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
                Connected = false;
                s_conn.Shutdown(SocketShutdown.Both);
                s_conn.Close();

                if (model_changed != null) model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.NOT_CONNECTED,
                    "Not connected to server", this.Identifier));
            }
        } // CLIENT shuts down connection with the server
        #endregion

        #region Senders
        public void SendData(byte[] data, SbizModelChanged_Delegate model_changed)
        {
            try
            {
                /* NB there was previously a protocol error as size of the data buffer was not sent, causing
                 * some data to not be processed by server.
                 */
                byte[] buffer = SbizNetUtils.EncapsulateInt32inByteArray(data, data.Length);
                s_conn.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, new StateObject(s_conn, model_changed, _view_handle));
                //s_conn.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            catch (SocketException)
            {
                if (Connected)
                {
                    Connected = false;

                    if (model_changed != null) model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.PEER_SHUTDOWN, 
                        "Remote endpoint disconnected", this.Identifier));
                }
            }
        }
        public void SendMessage(SbizMessage m, SbizModelChanged_Delegate model_changed)
        {
            SendData(m.ToByteArray(), model_changed);
        }
        #endregion

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
                    if (state.model_changed != null) state.model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.PEER_SHUTDOWN,
                        "Remote endpoint disconnected", this.Identifier));
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
                SendData(SbizMessage.AuthenticationMessage(state.key, (IPEndPoint)s.LocalEndPoint), state.model_changed);
                BeginReceiveMessageSize(s, state.model_changed, state.view_handle, state.key);
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
                Connected = true;
                BeginReceiveMessageSize(handler, state.model_changed, state.view_handle, state.key);
            }
        }
        private void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.socket;

            if (Connected)
            {
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
                    if (state.size_message) //Received the size of the subsequent message
                    {
                        StateObject state_out = new StateObject(handler, state.model_changed, state.view_handle, state.key);

                        state_out.datasize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(state.buffer, 0));
                        state_out.seek = 0;
                        state_out.size_message = false;
                        if (state_out.datasize >= 1024) state_out.buffer = new byte[1024];
                        else state_out.buffer = new byte[state_out.datasize];
                        state_out.data = new byte[state_out.datasize];
                        SbizBeginReceive(handler, state_out);
                    }
                    else //received part of a sbiz message
                    {
                        Array.Copy(state.buffer, 0, state.data, state.seek, bytesRead);
                        state.seek += bytesRead;
                        if (state.seek >= state.datasize) //received the whole message
                        {
                            var m = new SbizMessage(state.data);
                            if(HandleReceivedSbizMessage(m, state))
                                BeginReceiveMessageSize(handler, state.model_changed, state.view_handle, state.key);
                            else //auth failed, accept new conection
                            {
                                if (Listening)
                                    s_listen.BeginAccept(AcceptCallback, 
                                        new StateObject(s_listen, state.model_changed, state.view_handle, state.key));
                                return;
                            }
                        }
                        else //still missing some data
                        {
                            int bytes_left = state.datasize - state.seek;
                            if (bytes_left < 1024) state.buffer = new byte[bytes_left];
                            SbizBeginReceive(handler, state);
                        }
                    }            
                }
                else//peershutdown
                {
                    if (state.model_changed != null) state.model_changed(this, 
                        new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.PEER_SHUTDOWN,"Remote endpoint disconnected", this.Identifier));
                    Connected = false;
                    if (Listening)
                        s_listen.BeginAccept(AcceptCallback, 
                            new StateObject(s_listen, state.model_changed, state.view_handle, state.key));
                }
            }
            //if (Listening && !Connected) s_listen.BeginAccept(AcceptCallback, new StateObject(s_listen, state.model_changed, state.view_handle, state.key));
        }

        private void SbizBeginReceive(Socket handler, StateObject state_out)
        {
            try
            {
                handler.ReceiveTimeout = 1000;
                handler.BeginReceive(state_out.buffer, 0, state_out.buffer.Count(), 0,
                new AsyncCallback(ReadCallback), state_out);
            }
            catch (Exception)
            {
                if (state_out.model_changed != null) state_out.model_changed(this,
                new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.PEER_SHUTDOWN, "Remote endpoint disconnected", this.Identifier));
                Connected = false;
            }
        }

        private void BeginReceiveMessageSize(Socket handler, SbizModelChanged_Delegate model_changed, IntPtr view_handle, string key)
        {
            // Create the state object.
            StateObject state_out = new StateObject(handler, model_changed, view_handle, key);
            state_out.socket = handler;
            state_out.datasize = sizeof(Int32);
            state_out.seek = 0;
            state_out.size_message = true;
            state_out.buffer = new byte[sizeof(Int32)];
            SbizBeginReceive(handler, state_out);
        }

        private bool HandleReceivedSbizMessage(SbizMessage m, StateObject state)
        {
            if (!Authenticated)
            {
                if (!AuthenticateClient(m, state.key, (IPEndPoint)state.socket.RemoteEndPoint))
                {
                    if (Listening) //This is a server, send this message so that authentication with client will fail
                    {
                        SendData(SbizMessage.AuthenticationMessage(state.key, (IPEndPoint)state.socket.LocalEndPoint), state.model_changed);
                    }
                    if(state.model_changed != null) state.model_changed(this,
                        new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.AUTH_FAILED, "Authentication failed, probably incorrect password", this.Identifier));
                    
                    if (Connected)
                    {
                        s_conn.Close();
                        Connected = false;
                    }
                    return false;
                }
                else
                {
                    if (Listening) //This is a server
                    {
                        SendData(SbizMessage.AuthenticationMessage(state.key, (IPEndPoint)state.socket.LocalEndPoint), state.model_changed);
                    }
                    if (state.model_changed != null)
                        state.model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.CONNECTED,
                        "Connected to remote endpoint", this.Identifier));
                }
            }
            else
            {
                if (SbizMessageConst.IsClipboardConst(m.Code))
                {
                    SbizClipboardHandler.HandleClipboardSbizMessage(m, state.view_handle);
                }
                else if (_message_handle != null)
                {
                    _message_handle(m);
                }
            }
            return true;
        }

        public bool AuthenticateClient (SbizMessage m, string key, IPEndPoint ipe)
        {
            byte[] valid = SbizMessage.AutenticationPayload(key, ipe);
            if (m.Code == SbizMessageConst.AUTHENTICATE)

                if (System.Linq.Enumerable.SequenceEqual(m.Data, valid))
                {
                    Authenticated = true;
                    return true;
                }


            return  false;
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
            catch (Exception e)//TODO handle bind exception (alert the user)
            {
                throw e;
            }
        }
        /// <summary>
        /// Starts accepting and serving connections
        /// </summary>
        public void StartServer(SbizModelChanged_Delegate model_changed, IntPtr view_handle, string key)
        {
            Listening = true;
            Connected = false;

            var state = new StateObject(s_listen, model_changed, view_handle);
            state.key = key;
            s_listen.BeginAccept(AcceptCallback, state);
        }
        /// <summary>
        /// Stops accepting and serving connections
        /// </summary>
        public void StopServer(SbizModelChanged_Delegate model_changed)
        {
            if (Connected)
            {
                try
                {
                    s_conn.Close();
                }
                catch (Exception)
                {
                    //Already closed by peer do nothing
                }
                Connected = false;
            }
            if (Listening)
            {
                s_listen.Close();
                Listening = false;
            }
            model_changed(this, new SbizModelChanged_EventArgs(SbizModelChanged_EventArgs.NOT_LISTENING));
        }

        #endregion

        #region Internal Class State Object
        private class StateObject
        {
            public Socket socket;
            public int datasize;
            public int seek;
            public bool size_message;
            public IntPtr view_handle;
            public SbizModelChanged_Delegate model_changed;
            public string key;
            // Receive buffer.
            public byte[] data;
            public byte[] buffer;

            public StateObject(Socket socket, SbizModelChanged_Delegate model_changed, IntPtr view_handle, string key = null)
            {
                this.socket = socket;
                this.model_changed = model_changed;
                this.view_handle = view_handle;
                this.key = key;
            }
        }
        #endregion
    }
}
