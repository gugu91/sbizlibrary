using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Sbiz.Library
{
    [Serializable]
    public class SbizMouseEventArgs
    {
        #region Attributes
        private MouseButtons _button;
        private Int32 _clicks;
        private Int32 _delta;
        private double _rel_x;//contents are stored relatively for interoperability but the external interface is unaware of this 
        private double _rel_y;
        #endregion

        #region Properties
        public MouseButtons Button
        {
            get
            {
                return _button;
            }
        }
        public Int32 Clicks
        {
            get
            {
                return _clicks;
            }
        }
        public Int32 Delta
        {
            get
            {
                return _delta;
            }
        }
        public System.Drawing.Point Location
        {
            get
            {
                //SbizLogger.Logger = X + ", " + Y;
                return new System.Drawing.Point(X, Y);
            }
        }
        /// <summary>
        /// Readonly, X is in screen coordinates
        /// </summary>
        public Int32 X
        {
            get
            {
                return (int)Math.Round(_rel_x * (double)Screen.PrimaryScreen.Bounds.Width);
            }
        }
        /// <summary>
        /// Readonly, Y is in screen coordinates
        /// </summary>
        public int Y
        {
            get
            {
                return (Int32)Math.Round(_rel_y * (double)Screen.PrimaryScreen.Bounds.Height);
            }
        }

        #endregion

        #region Constructors
        public SbizMouseEventArgs(MouseButtons button, Int32 clicks, Int32 delta, Int32 x, Int32 y, Int32 x_bound, Int32 y_bound)
        {
            BaseConstructor(button, clicks, delta, x, y, x_bound, y_bound);
        }
        public void BaseConstructor(MouseButtons button, Int32 clicks, Int32 delta, Int32 x, Int32 y, Int32 x_bound, Int32 y_bound)
        {
            _button = button;
            _clicks = clicks;
            _delta = delta;
            //SbizLogger.Logger = screen_x + ", " + screen_y;
            _rel_x = ((double)x) / ((double)x_bound);
            _rel_y = ((double)y) / ((double)y_bound/*ex. Screen.PrimaryScreen.Bounds.Height*/);
        }
        /*
        public SbizMouseEventArgs(byte[] data)
        {
            SbizMouseEventArgs m = SbizNetUtils.DeserializeByteArray(data) as SbizMouseEventArgs;           

            if (m == null)
            {
                throw new ArgumentNullException();
            }
            _button = m._button;
            _clicks = m._clicks;
            _delta = m._delta;
            _rel_x = m._rel_x;
            _rel_y = m._rel_y;
            
        }*/
        public SbizMouseEventArgs(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            _rel_y = BitConverter.Int64BitsToDouble(SbizNetUtils.DecapsulateInt64FromByteArray(ref data));
            _rel_x = BitConverter.Int64BitsToDouble(SbizNetUtils.DecapsulateInt64FromByteArray(ref data));
            _delta = SbizNetUtils.DecapsulateInt32FromByteArray(ref data);
            _clicks = SbizNetUtils.DecapsulateInt32FromByteArray(ref data);
            _button = (MouseButtons)SbizNetUtils.DecapsulateInt32FromByteArray(ref data);
        }
        #endregion

        #region InstanceMethods
        public byte[] ToByteArray()
        {
            byte[] buffer = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((Int32)Button));
            buffer = SbizNetUtils.EncapsulateInt32inByteArray(buffer, _clicks);
            buffer = SbizNetUtils.EncapsulateInt32inByteArray(buffer, _delta);
            buffer = SbizNetUtils.EncapsulateInt64inByteArray(buffer, BitConverter.DoubleToInt64Bits(_rel_x));
            buffer = SbizNetUtils.EncapsulateInt64inByteArray(buffer, BitConverter.DoubleToInt64Bits(_rel_y));

            return buffer;
        }
        #endregion
    }
}
