using System;
using System.Windows.Forms;

namespace SoundpadVoiceControl
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Logic logic = new Logic();
            logic.init();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
