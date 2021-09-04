using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundpadVoiceControl.Properties;

namespace SoundpadVoiceControl
{
    public partial class MainForm : Form
    {
        private delegate void EnableDelegate(string text);
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set window location
            if (Settings.Default.WindowLocation != null)
            {
                this.Location = Settings.Default.WindowLocation;
                Console.WriteLine("test");
            }

            // Set window size
            if (Settings.Default.WindowSize != null)
            {
                this.Size = Settings.Default.WindowSize;
                Console.WriteLine("test2");
            }
        }

        public void MainForm_FormClosing()
        {
            // Copy window location to app settings
            Settings.Default.WindowLocation = this.Location;

            // Copy window size to app settings
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowSize = this.Size;
            }
            else
            {
                Settings.Default.WindowSize = this.RestoreBounds.Size;
            }

            // Save settings
            Settings.Default.Save();
        }

        public void SetMainLabel(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new EnableDelegate(SetMainLabel), new object[] { text });
                return;
            }
            mainlabel.Text = text;
        }

        public void SetErrorLabel(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new EnableDelegate(SetErrorLabel), new object[] { text });
                return;
            }
            errorlabel.Text = text;
        }

        public void SetListLabel(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new EnableDelegate(SetListLabel), new object[] { text });
                return;
            }
            listlabel.Text = text;
        }
    }
}
