using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sbiz.Library
{
    public static class SbizClipboardHandler
    {
        private static SbizMessageSending_Delegate _message_sender;
        public static void RegisterSbizMessageSendingDelegate(SbizMessageSending_Delegate del)
        {
            _message_sender += del;
        }
        public static void UnregisterSbizMessageSendingDelegate(SbizMessageSending_Delegate del)
        {
            _message_sender -= del;
        }

        public static void HandleClipboardData(IDataObject data)
        {
            /* Depending on the clipboard's current data format we can process the data differently.*/

            if (data.GetDataPresent(DataFormats.UnicodeText))
            {
                UnicodeTextSend((string)data.GetData(DataFormats.Text));
                //label.Text = "Updating Server Clipboard...";



                // do something with it
            }
            /*
        else if (iData.GetDataPresent(DataFormats.Bitmap))
        {
            Bitmap image = (Bitmap)iData.GetData(DataFormats.Bitmap);
            // do something with it
        }*/
        }

        public static void UnicodeTextSend(string text)
        {
            byte[] data = Encoding.BigEndianUnicode.GetBytes(text); //Network byte order il big endian

            SbizMessage m = new SbizMessage(SbizMessageConst.CLIPBOARD_UNICODETEXT, data);
            if(_message_sender != null) _message_sender(m);
        }

        public static bool ReceiveClipboardSbizMessage(SbizMessage m)
        {
            IDataObject data = new DataObject();
            bool recognized = false;

            #region Not Supported Formats
            if (m.Code == SbizMessageConst.CLIPBOARD_AUDIO)
            {
                recognized = true;
            }
            if (m.Code == SbizMessageConst.CLIPBOARD_FILE) 
            {
                recognized = true;
            }
            if (m.Code == SbizMessageConst.CLIPBOARD_IMG)
            {
                recognized = true;
            }
            #endregion

            if (m.Code == SbizMessageConst.CLIPBOARD_UNICODETEXT)
            {
                string text = Encoding.BigEndianUnicode.GetString(m.Data); //Network byte order is big endian

                data.SetData(DataFormats.UnicodeText, text); //format, object
                recognized = true;
            }

            Clipboard.SetDataObject(data, true);

            return recognized;
        }
    }
}
