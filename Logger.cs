using System;
using System.IO;
using System.Text;

namespace WawaEditor
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "wawaeditor.log");
        private static readonly object LockObj = new object();

        public static void Log(string message)
        {
            try
            {
                lock (LockObj)
                {
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // 忽略日志写入错误
            }
        }
    }
}