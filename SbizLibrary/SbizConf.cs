using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SbizServer
{
    public static class SbizConf
    {
        private const string _dirPath = "conf";
        private const string _sbizSocketFilename = "socketconf.txt";
        private const int _defaultPort = 15001;
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

    }
}
