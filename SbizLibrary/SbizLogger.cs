using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sbiz.Library
{
    public static class SbizLogger
    {
        #region Attributes

        private const string _dirPath = "log";
        private const string _sbizLogFileName = "LogFile.txt";

        #endregion

        #region Properties
        private static string DirPath
        {
            get
            {
                if (!Directory.Exists(_dirPath)) Directory.CreateDirectory(_dirPath);
                return _dirPath;
            }
        }

        private static string SbizLoggerPath
        {
            get
            {
                return DirPath + "\\" + _sbizLogFileName;
            }
        }

        public static string Logger
        {
            set
            {
                UpdateLogFile(value);
            }
        }

        #endregion

        private static void UpdateLogFile(string line)
        {
            StreamWriter sw = new StreamWriter(SbizLoggerPath, true);
            string log_line = DateTime.Now.ToString("G");

            log_line += " - ";
            log_line += line;

            sw.WriteLine(log_line);
            sw.Close();
        }
        
    }
}
