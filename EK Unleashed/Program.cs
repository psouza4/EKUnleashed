using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EKUnleashed
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!System.IO.Directory.Exists(Utils.AppFolder))
                System.IO.Directory.CreateDirectory(Utils.AppFolder);

            string[] args = Utils.GetCommandLineArgs();
            if (args.Length > 1)
                Utils.SettingsProfile = args[args.Length - 1].Trim().Replace("\\", "-");

            Application.Run(new frmMain());
        }
    }
}
