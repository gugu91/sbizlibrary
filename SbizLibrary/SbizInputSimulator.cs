using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using System.Windows.Forms;

namespace Sbiz.Library
{
    /*
     * Note: this is a wrapper class of this project http://inputsimulator.codeplex.com/SourceControl/latest#README.md
     * All credits go to the author of the original product
     */
    public static class SbizInputSimulator
    {
        private static InputSimulator handler = new InputSimulator();
        public static void SimulateEvent(SbizMouseEventArgs smea){
            Cursor.Position = smea.Location;
            if (smea.Button == MouseButtons.Left)
            {
                for (int i = 0; i < smea.Clicks; i++ ) handler.Mouse.LeftButtonClick();
            }
        }
    }
}
