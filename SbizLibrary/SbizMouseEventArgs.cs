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
        private int _clicks;
        private int _delta;
        private float _rel_x;//contents are stored relatively for interoperability but the external interface is unaware of this 
        private float _rel_y;
        #endregion

        #region Properties
        public MouseButtons Button
        {
            get
            {
                return _button;
            }
        }
        public int Clicks
        {
            get
            {
                return _clicks;
            }
        }
        public int Delta
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
                return new System.Drawing.Point(X, Y);
            }
        }
        public int X
        {
            get
            {
                return (int)Math.Round(_rel_x * Screen.PrimaryScreen.Bounds.Width);
            }
        }
        public int Y
        {
            get
            {
                return (int)Math.Round(_rel_y * Screen.PrimaryScreen.Bounds.Height);
            }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new istance of class SbizMouseEventArgs. Pay attention tha x and y coordinates of screen_point
        /// need to be in screen coordinates
        /// </summary>
        /// <param name="button"></param>
        /// <param name="clicks"></param>
        /// <param name="delta"></param>
        /// <param name="screen_point">point in screen coordinates</param>
        public SbizMouseEventArgs(MouseButtons button, int clicks, int delta, System.Drawing.Point screen_point)
        {
            _button = button;
            _clicks = clicks;
            _delta = delta;
            _rel_x = screen_point.X;
            _rel_y = screen_point.Y;
        }
        /// <summary>
        /// Creates a new istance of class SbizMouseEventArgs. Pay attention tha x and y coordinates need to be in screen coordinates
        /// </summary>
        /// <param name="button"></param>
        /// <param name="clicks"></param>
        /// <param name="delta"></param>
        /// <param name="screen_x">x position in screen coordinates</param>
        /// <param name="screen_y">y position in screen coordinates</param>
        public SbizMouseEventArgs(MouseButtons button, int clicks, int delta, int screen_x, int screen_y)
        {
            _button = button;
            _clicks = clicks;
            _delta = delta;
            _rel_x = screen_x / Screen.PrimaryScreen.Bounds.Width;
            _rel_y = screen_y / Screen.PrimaryScreen.Bounds.Height;
        }

        public SbizMouseEventArgs(byte[] data)
        {
            SbizMouseEventArgs m = SbizBasic.DeserializeByteArray(data) as SbizMouseEventArgs;           

            if (m == null)
            {
                throw new ArgumentNullException();
            }
            _button = m._button;
            _clicks = m._clicks;
            _delta = m._delta;
            _rel_x = m._rel_x;
            _rel_y = m._rel_y;
            
        }
        #endregion

        #region InstanceMethods
        public byte[] ToByteArray()
        {
            return SbizBasic.SerializeObject(this);
        }
        #endregion
    }
}
