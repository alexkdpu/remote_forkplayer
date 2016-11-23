using System;
using System.IO;
using System.Text;

namespace RemoteFork {
    public static class Logger {
        private const string LOG_FILE_NAME = "log.txt";
        private const string BACKUP_LOG_FILE_NAME = "log-{0}.txt";
        private const int LOG_FILE_LIMIT_SIZE = 10485760;

        public static LogLevel Level = LogLevel.NONE;

        public enum LogLevel {
            NONE = 0,
            INFO = 1,
            ERROR = 2,
            DEBUG = 3
        }

        public static void Info(string message) {
            if (Level >= LogLevel.INFO) {
                WriteToFile(LogLevel.INFO, message);
            }
        }

        public static void Info(string format, params object[] args) {
            if (Level >= LogLevel.INFO) {
                WriteToFile(LogLevel.INFO, string.Format(format, args));
            }
        }

        public static void Error(string message) {
            if (Level >= LogLevel.ERROR) {
                WriteToFile(LogLevel.ERROR, message);
            }
        }

        public static void Error(string format, params object[] args) {
            if (Level >= LogLevel.ERROR) {
                WriteToFile(LogLevel.ERROR, string.Format(format, args));
            }
        }

        public static void Debug(string message) {
            if (Level >= LogLevel.DEBUG) {
                WriteToFile(LogLevel.DEBUG, message);
            }
        }

        public static void Debug(string format, params object[] args) {
            if (Level >= LogLevel.DEBUG) {
                WriteToFile(LogLevel.DEBUG, string.Format(format, args));
            }
        }

        private static void WriteToFile(LogLevel level, string text) {
            try {
                if (!File.Exists(LOG_FILE_NAME)) {
                    File.Create(LOG_FILE_NAME).Close();
                }
                text = string.Format("[{0}] {1:G}: {2}{3}", level, DateTime.Now, text, Environment.NewLine);
                //Console.WriteLine(text);
                File.AppendAllText(LOG_FILE_NAME, text, Encoding.UTF8);
                var info = new FileInfo(LOG_FILE_NAME);
                if (info.Length > LOG_FILE_LIMIT_SIZE) {
                    File.Move(LOG_FILE_NAME,
                        string.Format(BACKUP_LOG_FILE_NAME, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")));
                }
            } catch {
                // ignored
            }
        }
    }
}
