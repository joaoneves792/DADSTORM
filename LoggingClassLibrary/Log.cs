using System;
using System.IO;
using System.Diagnostics;

namespace LoggingClassLibrary
{
    public enum LogStatus {
        DEBUG,
        ROUTINE,
        CRITICAL,
        //todo: eliminate irrelevant logging levels
        FULL,
        LIGHT
    }

    public class Log {
        private static LogStatus _logStatus = LogStatus.CRITICAL;

        public static LogStatus LogStatus {
            get { return _logStatus; }
            set { _logStatus = value; }
        }

        public static void WriteLine(LogStatus logStatus, String message) {
            //TODO: Increase condition readability (too much low-leveled)
            if (logStatus >= _logStatus) {
                StreamWriter w = File.AppendText(Process.GetCurrentProcess().Id + "log.txt");
                w.WriteLine("[{0}][{1}] {2}",
                    DateTime.Now.ToLongDateString(),
                    DateTime.Now.ToLongTimeString(),
                    message);
                w.Flush();
                w.Close();

                Console.WriteLine("[{0}][{1}] {2}",
                    DateTime.Now.ToLongDateString(),
                    DateTime.Now.ToLongTimeString(),
                    message);
            }
        }

        ///<summary>
        /// Write message without changing line
        ///</summary>
        public static void Write(LogStatus logStatus, String message)
        {
            //TODO: Increase condition readability (too much low-leveled)
            if (logStatus >= _logStatus)
            {
                StreamWriter w = File.AppendText(Process.GetCurrentProcess().Id + "log.txt");
                w.Write("[{0}][{1}] {2}",
                    DateTime.Now.ToLongDateString(),
                    DateTime.Now.ToLongTimeString(),
                    message);
                w.Flush();
                w.Close();

                Console.Write("[{0}][{1}] {2}",
                    DateTime.Now.ToLongDateString(),
                    DateTime.Now.ToLongTimeString(),
                    message);
            }
        }

        public static void WriteDone(LogStatus logStatus)
        {
            //TODO: Increase condition readability (too much low-leveled)
            if (logStatus >= _logStatus)
            {
                StreamWriter w = File.AppendText(Process.GetCurrentProcess().Id + "log.txt");
                w.WriteLine(" Done");
                w.Flush();
                w.Close();

                Console.WriteLine(" Done");
            }
        }

        public static void WriteError(LogStatus logStatus)
        {
            //TODO: Increase condition readability (too much low-leveled)
            if (logStatus >= _logStatus)
            {
                StreamWriter w = File.AppendText(Process.GetCurrentProcess().Id + "log.txt");
                w.WriteLine(" Error");
                w.Flush();
                w.Close();

                Console.WriteLine(" Error");
            }
        }
    }
}
