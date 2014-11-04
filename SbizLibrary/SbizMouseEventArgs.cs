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
                //SbizLogger.Logger = X + ", " + Y;
                return new System.Drawing.Point(X, Y);
            }
        }
        /// <summary>
        /// Readonly, X is in screen coordinates
        /// </summary>
        public int X
        {
            get
            {
                return (int)Math.Round(_rel_x * (float)Screen.PrimaryScreen.Bounds.Width);
            }
        }
        /// <summary>
        /// Readonly, Y is in screen coordinates
        /// </summary>
        public int Y
        {
            get
            {
                return (int)Math.Round(_rel_y * (float)Screen.PrimaryScreen.Bounds.Height);
            }
        }

        #endregion

        #region Constructors
        public SbizMouseEventArgs(MouseButtons button, int clicks, int delta, int x, int y, int x_bound, int y_bound)
        {
            BaseConstructor(button, clicks, delta, x, y, x_bound, y_bound);
        }
        public void BaseConstructor(MouseButtons button, int clicks, int delta, int x, int y, int x_bound, int y_bound)
        {
            _button = button;
            _clicks = clicks;
            _delta = delta;
            //SbizLogger.Logger = screen_x + ", " + screen_y;
            _rel_x = ((float)x) / ((float)x_bound);
            _rel_y = ((float)y) / ((float)y_bound/*ex. Screen.PrimaryScreen.Bounds.Height*/);
        }

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
            
        }
        #endregion

        #region InstanceMethods
        public byte[] ToByteArray()
        {
            return SbizNetUtils.SerializeObject(this);
        }
        #endregion
    }
}
