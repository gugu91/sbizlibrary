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
        private enum SbizSocketConfLine { address, port}
        public static string MyIP
        {
            get
            {
                IPHostEntry host;
                string myIP = "?";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !ip.Equals(IPAddress.Loopback))
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

        private static IPAddress DefaultAddress
        {
            get
            {
                return IPAddress.Parse(_defaultAddress);
            }
        }
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
                return DirPath + "\\" + _sbizSocketFilename;
            }
        }

        public static Int32 SbizSocketPort
        {
            get{
                Int32 retValue;

                try{
                    string port_ascii = ReadSocketConfFileAt(SbizSocketConfLine.port);
                    retValue = Int32.Parse(port_ascii);
                } catch {
                    UpdateSocketConfFile(_defaultPort, SbizSocketAddress);
                    return _defaultPort;
                }

                return retValue;
            }
            set
            {
                UpdateSocketConfFile(value, SbizSocketAddress);
            }
        }

        public static IPAddress SbizSocketAddress
        {
            get
            {
                IPAddress address;

                try
                {
                    address = IPAddress.Parse(ReadSocketConfFileAt(SbizSocketConfLine.address));
                }
                catch
                {
                    UpdateSocketConfFile(_defaultPort, DefaultAddress);
                    return DefaultAddress;
                }

                return address;
            }
            set
            {
                UpdateSocketConfFile(SbizSocketPort, value);
            }
        }

        private static void UpdateSocketConfFile(int new_port, IPAddress new_address){
            StreamWriter sw = new StreamWriter(SbizSocketPath);

            var lines = Enum.GetValues(typeof(SbizSocketConfLine)).Cast<SbizSocketConfLine>();
            foreach (var line in lines)
            {
                if (line == SbizSocketConfLine.port) sw.WriteLine(new_port);
                if (line == SbizSocketConfLine.address) sw.WriteLine(new_address.ToString());
            }

            sw.Close();
        }
        private static string ReadSocketConfFileAt(SbizSocketConfLine index){
            string buffer;
            StreamReader sr;

            try
            {
               sr = new StreamReader(SbizSocketPath);
            }
            catch (Exception e)
            {
                throw e;
            }

            var lines = Enum.GetValues(typeof(SbizSocketConfLine)).Cast<SbizSocketConfLine>();
            foreach (var line in lines)
            {
                buffer = sr.ReadLine();
                if (line == index)
                {
                    sr.Close();
                    return buffer;
                }
            }

            sr.Close();
            return null;
        }

    }
}
