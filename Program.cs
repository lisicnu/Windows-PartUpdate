using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Eden.Share.Extensions;
using Eden.Share.Misc;

namespace Eden.Update
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var process = RuningInstance();
            if (process != null)
            {
                Console.WriteLine("Updater is running.");
                return 0;
            }

            if (args != null && args.IsNotEmpty())
            {
                CommandLineArguments command = new CommandLineArguments(args);

                if (command.ContainKey("help") || command.ContainKey("?"))
                {
                    string msg =
                   new StringBuilder().Append("Usage command:")
                       .Append(Environment.NewLine)
                       .Append("-ForceUpdate if contains this means force update, otherwise the update can be applied.")
                       .ToString();

                    MessageBox.Show(msg, "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }

                if (command.ContainKey("forceUpdate"))
                    CommonUnitity.ForceUpdate = true;
            }

            CommonUnitity.LogMsg("Forceupdate = " + CommonUnitity.ForceUpdate);

            ExitCode exitCode = ExitCode.Default;
            IAutoUpdater autoUpdater = null;
            try
            {
                autoUpdater = new AutoUpdater();
                exitCode = autoUpdater.Update();
            }
            catch (Exception exp)
            {
                CommonUnitity.LogMsg(exp);
                if (autoUpdater != null)
                {
                    autoUpdater.RollBack();
                }
                MessageBox.Show("Update failed, roll back files.");
            }
            return (int) exitCode;
        }

        private static Process RuningInstance()
        {
            var curProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(curProcess.ProcessName);

            return
                processes.Where(p => p.Id != curProcess.Id)
                    .FirstOrDefault(
                        p =>
                            Assembly.GetExecutingAssembly().Location.Replace("/ ", "\\ ") ==
                            curProcess.MainModule.FileName);
        }
    }
}
