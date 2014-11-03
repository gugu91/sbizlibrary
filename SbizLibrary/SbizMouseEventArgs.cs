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
        private System.Drawing.Point _location;
        private int _x;
        private int _y;
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
                return _location;
            }
        }
        public int X
        {
            get
            {
                return _x;
            }
        }
        public int Y
        {
            get
            {
                return _y;
            }
        }

        #endregion

        #region Constructors
        public SbizMouseEventArgs(MouseButtons button, int clicks, int delta, System.Drawing.Point location, int x, int y)
        {
            _button = button;
            _clicks = clicks;
            _delta = delta;
            _location = location;
            _x = x;
            _y = y;
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
