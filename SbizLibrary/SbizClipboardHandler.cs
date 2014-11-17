using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace Sbiz.Library
{
    public static class SbizClipboardHandler
    {
        private const long MAX_FILELENGTH = 10*1024*1024; //10 MB, which is 1 sec @ 80 Kbps, still an annoying delay
        private static IDataObject _data_object_buffer;

        #region Message Sender Delegate
        private static SbizMessageSending_Delegate _message_sender;
        public static void RegisterSbizMessageSendingDelegate(SbizMessageSending_Delegate del)
        {
            _message_sender += del;
        }
        public static void UnregisterSbizMessageSendingDelegate(SbizMessageSending_Delegate del)
        {
            _message_sender -= del;
        }
        #endregion

        #region File Utils
        public static long FileSystemSize(String path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(path);

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return MAX_FILELENGTH * 2;
            }
            else
            {
                FileInfo fi = new FileInfo(path);
                return fi.Length;
            }
        }
        public static long DirectorySize(DirectoryInfo d)
        {
            long Size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += DirectorySize(di);
            }
            return (Size);
        }
        #endregion

        public static void SendClipboardData(IDataObject data, SbizModelChanged_Delegate model_changed, IntPtr view_handle)
        {
            /* Depending on the clipboard's current data format we can process the data differently.*/
            //Start clipboard burst
            byte[] delimiter = Encoding.UTF8.GetBytes("clipboard_burst_start");
            var m = new SbizMessage(SbizMessageConst.CLIPBOARD_START, delimiter);
            if (_message_sender != null) _message_sender(m, model_changed);

            //Send the data
            if (data.GetDataPresent(DataFormats.UnicodeText))
                UnicodeTextSend((string)data.GetData(DataFormats.UnicodeText), model_changed);
            if (data.GetDataPresent(DataFormats.Text))
                AnsiTextSend((string)data.GetData(DataFormats.Text), model_changed);
            if (data.GetDataPresent(DataFormats.Bitmap))
                BitmapSend((Bitmap)data.GetData(DataFormats.Bitmap), model_changed);
            if (data.GetDataPresent(DataFormats.FileDrop))
                FileDropSend((string[])data.GetData(DataFormats.FileDrop), model_changed);

            //End Clipboard burst
            delimiter = Encoding.UTF8.GetBytes("clipboard_burst_end");
            m = new SbizMessage(SbizMessageConst.CLIPBOARD_STOP, delimiter);
            if (_message_sender != null) _message_sender(m, model_changed);
        }

        #region Specific Format Senders
        /// <summary>
        /// Directories and files bigger than MAX_FILELENGTH are not supported
        /// </summary>
        /// <param name="all_paths"></param>
        /// <param name="model_changed"></param>
        public static void FileDropSend(string[] all_paths, SbizModelChanged_Delegate model_changed)
        {
             long totalsize=0;

                foreach (string path in all_paths)
                {
                    totalsize += FileSystemSize(path);
                }

                SbizFileEntry[] all_entries = new SbizFileEntry[all_paths.Count()];

                if (totalsize < MAX_FILELENGTH) //if it exceeds maximum length do nothing
                {
                    int i = 0;
                    foreach (string path in all_paths)
                    {
                        SbizFileEntry sfe = new SbizFileEntry(Path.GetFileName(path), File.ReadAllBytes(path));
                        all_entries[i] = sfe;
                        i++;
                    }

                    var m = new SbizMessage(SbizMessageConst.CLIPBOARD_FILE, SbizNetUtils.SerializeObject(all_entries));
                    if (_message_sender != null) _message_sender(m, model_changed);
                }
        }
        public static void UnicodeTextSend(string text, SbizModelChanged_Delegate model_changed)
        {
            byte[] data = Encoding.Unicode.GetBytes(text);

            SbizMessage m = new SbizMessage(SbizMessageConst.CLIPBOARD_UNICODETEXT, data);
            if(_message_sender != null) _message_sender(m, model_changed);
        }
        public static void AnsiTextSend(string text, SbizModelChanged_Delegate model_changed)
        {
            byte[] data = Encoding.Default.GetBytes(text);

            SbizMessage m = new SbizMessage(SbizMessageConst.CLIPBOARD_TEXT, data);
            if (_message_sender != null) _message_sender(m, model_changed);
        }
        public static void BitmapSend(Bitmap img, SbizModelChanged_Delegate model_changed)
        {
            ImageConverter converter = new ImageConverter();
            byte[] data = (byte[])converter.ConvertTo(img, typeof(byte[]));
            SbizMessage m = new SbizMessage(SbizMessageConst.CLIPBOARD_IMG, data);
            if (_message_sender != null) _message_sender(m, model_changed);

        }
        #endregion

        public static bool HandleClipboardSbizMessage(SbizMessage m, IntPtr view_handle)
        {
            
            bool recognized = false;

            #region Not Supported Formats
            if (m.Code == SbizMessageConst.CLIPBOARD_AUDIO)
            {
                recognized = true;
            }
            #endregion

            #region CLIPBOARD_START
            if (m.Code == SbizMessageConst.CLIPBOARD_START)
            {
                _data_object_buffer = new DataObject();

                recognized = true;
            }
            #endregion

            #region TEXT && UNICODE_TEXT
            else if (m.Code == SbizMessageConst.CLIPBOARD_UNICODETEXT && _data_object_buffer != null)
            {
                string text = Encoding.Unicode.GetString(m.Data);

                _data_object_buffer.SetData(DataFormats.UnicodeText, text); //format, object

                recognized = true;
            }

            else if (m.Code == SbizMessageConst.CLIPBOARD_TEXT && _data_object_buffer != null)
            {
                string text = Encoding.Default.GetString(m.Data);

                _data_object_buffer.SetData(DataFormats.Text, text); //format, object

                recognized = true;
            }
            #endregion

            #region FILEDROP
            else if (m.Code == SbizMessageConst.CLIPBOARD_FILE && _data_object_buffer != null)
            {
                SbizFileEntry[] all_entries = (SbizFileEntry[])SbizNetUtils.DeserializeByteArray(m.Data);
                string[] filedrop = new string[all_entries.Count()];
                SbizConf.ResetTmpDir();
                int i = 0;
                foreach (SbizFileEntry sfe in all_entries)
                {
                    string filepath = SbizConf.SbizTmpFilePath(sfe.file_name);
                    File.WriteAllBytes(filepath, sfe.file_bytes);
                    filedrop[i] = Path.GetFullPath(filepath);
                    i++;
                }
                _data_object_buffer.SetData(DataFormats.FileDrop, filedrop);

                recognized = true;
            }
            #endregion

            #region IMG
            else if (m.Code == SbizMessageConst.CLIPBOARD_IMG && _data_object_buffer != null)
            {
                var ms = new MemoryStream(m.Data);
                Image img = Image.FromStream(ms);
                _data_object_buffer.SetData(DataFormats.Bitmap, img);

                recognized = true;
            }
            #endregion

            #region CLIPBOARD_STOP
            else if (m.Code == SbizMessageConst.CLIPBOARD_STOP && _data_object_buffer != null)
            {
                NativeImport.RemoveClipboardFormatListener(view_handle); //Stop sniffing to avoid loops
                System.Threading.Thread thread = 
                    new System.Threading.Thread(() => Clipboard.SetDataObject(_data_object_buffer, true, 3, 100));
                thread.SetApartmentState(System.Threading.ApartmentState.STA); //Set the thread to STA
                thread.Start();
                thread.Join();
                NativeImport.AddClipboardFormatListener(view_handle);

                recognized = true;
            }
            #endregion


            return recognized;
        }
    }

    #region Internal class SbizFileEntry
    [Serializable]
    class SbizFileEntry
    {
        public string file_name;
        public byte[] file_bytes;

        public SbizFileEntry(string file_name, byte[] file_bytes)
        {
            this.file_bytes = file_bytes;
            this.file_name = file_name;
        }

        public SbizFileEntry(byte[] data)
        {
            SbizFileEntry tmp = (SbizFileEntry)SbizNetUtils.DeserializeByteArray(data);

            this.file_name = tmp.file_name;
            this.file_bytes = tmp.file_bytes;
        }

        public byte[] ToByteArray()
        {
            return SbizNetUtils.SerializeObject(this);
        }
    }
    #endregion
}
