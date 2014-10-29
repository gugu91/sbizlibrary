using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Sbiz.Library
{
    public static class SbizConf
    {
        public static string MyIP
        {
            get
            {
                IPHostEntry host;
                string myIP = "?";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && ip.Equals(IPAddress.Loopback))
                    {
                        myIP = ip.ToString();
                    }
                }
                return myIP;
            }
        }

        private const string _dirPath = "conf";
        private const string _sbizSocketFilename = "socketconf.txt";
        private const int _defaultPort = 15001;
        private const string _defaultAddress = "192.168.0.1";
        private static string DirPath
        {
            get
            {
                if (!Directory.Exists(_dirPath)) Directory.CreateDirectory(_dirPath);
                return _dirPath;
            }
        }

        private static string SbizSocketPath
        {
            get
            {
                string path = DirPath + "\\" + _sbizSocketFilename;
                if (!File.Exists(path))
                {
                    StreamWriter sw = new StreamWriter(path);
                    sw.Write(_defaultPort);
                    sw.Close();
                }

                return path;
            }
        }

        public static Int32 SbizSocketPort
        {
            get{
                Int32 retValue;

                StreamReader sr = new StreamReader(SbizSocketPath);

                try{
                    string port_ascii = sr.ReadLine();
                    sr.Close();
                    retValue = Int32.Parse(port_ascii);
                } catch (Exception e){
                    StreamWriter sw = new StreamWriter(SbizSocketPath);
                    sw.Write(_defaultPort);
                    sw.Close();
                    retValue = _defaultPort;
                }

                return retValue;
            }
            set
            {
                StreamWriter sw = new StreamWriter(SbizSocketPath);
                sw.Write(value);
                sw.Close();
            }
        }

        public static string SbizSocketAddress
        {
            get
            {
                string retValue;

                StreamReader sr = new StreamReader(SbizSocketPath);

                try
                {
                    retValue = sr.ReadLine();
                    sr.Close();
                }
                catch (Exception e)
                {
                    StreamWriter sw = new StreamWriter(SbizSocketPath);
                    sw.Write(_defaultAddress);
                    sw.Close();

                    return _defaultAddress;
                }

                return retValue;
            }
            set
            {
                StreamWriter sw = new StreamWriter(SbizSocketPath);
                sw.Write(value);
                sw.Close();
            }
        }

    }
}
