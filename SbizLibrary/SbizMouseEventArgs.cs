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
        private int _rel_x;
        private int _rel_y;
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
                int x = _rel_x * Screen.PrimaryScreen.Bounds.Width;
                int y = _rel_y * Screen.PrimaryScreen.Bounds.Height;
                return new System.Drawing.Point(x, y);
            }
        }
        public int RelX
        {
            get
            {
                return _rel_x;
            }
        }
        public int RelY
        {
            get
            {
                return _rel_y;
            }
        }

        #endregion

        #region Constructors
        public SbizMouseEventArgs(MouseButtons button, int clicks, int delta, int rel_x, int rel_y)
        {
            _button = button;
            _clicks = clicks;
            _delta = delta;
            _rel_x = rel_x;
            _rel_y = rel_y;
        }

        public SbizMouseEventArgs(byte[] data)
        {
            SbizMouseEventArgs m = SbizBasic.DeserializeByteArray(data) as SbizMouseEventArgs;           

            if (m == null)
            {
                throw new ArgumentNullException();
            }
            _button = m.Button;
            _clicks = m.Clicks;
            _delta = m.Delta;
            _rel_x = m.RelX;
            _rel_y = m.RelY;
            
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
