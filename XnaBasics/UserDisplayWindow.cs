using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    public class UserDisplayWindow : Form
    {
        IntPtr canvas;
        Panel displaypanel;

        public Panel DisplayPanel
        {
            get { return displaypanel; }
            set { displaypanel = value; }
        }

        public IntPtr Canvas
        {
            get { return canvas; }
            set { canvas = value; }
        }

        public UserDisplayWindow(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            displaypanel = new Panel();
            displaypanel.Dock = DockStyle.Fill;
            displaypanel.Width = width;
            displaypanel.Height = height;

            this.canvas = displaypanel.Handle;
            this.Controls.Add(displaypanel);
        }
    }
}
