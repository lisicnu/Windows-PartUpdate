using System;
using Eden.Share.DataStructs;

namespace Eden.Update
{
    
    public static class CommonUnitity
    {
        public static event EventHandler<EventArgs<string>> Log;

        public static readonly string SystemBinUrl = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exitCode">0 normal finish. 1 success updated. 2 has update, but skipped. 3 update error.</param>
        public static void ExitApp(ExitCode exitCode)
        {
//            Process.Start(Application.ExecutablePath);
            Environment.Exit((int) exitCode);
        }

        public static void OnLog(object sender, EventArgs<string> e)
        {
            if (e != null)
            {
                DoLog(e.Value, null);
            }

            var handler = Log;
            if (handler != null) handler(sender, e);
        }

        static CommonUnitity()
        {
            ForceUpdate = false;
        }

        /// <summary>
        /// log app message.
        /// </summary>
        /// <param name="msg"></param>
        public static void LogMsg(object msg)
        {
            if (msg is Exception)
            {
                DoLog("", msg as Exception);
            }
            else
            {
                DoLog(msg, null);
            }
        }

        /// <summary>
        /// log app message.
        /// </summary>
        /// <param name="msg"></param>
        private static void DoLog(object msg, Exception e)
        {
            Console.WriteLine("{0}:  {1}, {2}", DateTime.Now.ToString("hh:mm:ss.fff"), msg, e);
        }

        public static bool ForceUpdate { get; set; }
    }
}
