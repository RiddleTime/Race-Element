using System;
using System.Collections.Generic;
using System.IO;

namespace ACCManager.Util
{
    /// <summary>
    /// Simple thread safe logging helper
    /// </summary>
    public class LogWriter
    {

#pragma warning disable IDE1006 // Naming Styles
        private static LogWriter _instance { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        /// <summary>
        /// Single instance of logwriter
        /// </summary>
        private static LogWriter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogWriter();
                    _instance.LogQueue = new Queue<Log>();
                    FlushedAt = DateTime.Now;
                }
                return _instance;
            }
        }

        /// <summary>
        /// Queue used to store logs
        /// </summary>
        private Queue<Log> LogQueue;

        /// <summary>
        /// Path to save log files
        /// </summary>
        private static string LogPath = FileUtil.AccManagerLogPath;

        /// <summary>
        /// Lof file name
        /// </summary>
        private static string LogFile = "ACC_Manager_Log.txt";

        /// <summary>
        /// Flush log when time reached
        /// </summary>
        private static int FlushAtAge = 1000 * 60 * 60;

        /// <summary>
        /// Flush log when quantity reached
        /// </summary>
        private static int FlushAtQty = 0;

        /// <summary>
        /// Timestamp of last flush
        /// </summary>
        private static DateTime FlushedAt;

        /// <summary>
        /// Private constructor -> prevent instantiation
        /// </summary>
        private LogWriter() { }

        /// <summary>
        /// Log message
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void WriteToLog(string message)
        {
            lock (Instance.LogQueue)
            {
                // Create log
                Log log = new Log(message);
                Instance.LogQueue.Enqueue(log);

                // Check if should flush
                if (Instance.LogQueue.Count >= FlushAtQty || CheckTimeToFlush())
                {
                    FlushLogToFile();
                }

            }
        }

        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="e">Exception to log</param>
        public static void WriteToLog(Exception e)
        {
            lock (Instance.LogQueue)
            {
                // Create log
                Log msg = new Log(e.Source.ToString().Trim() + " " + e.Message.ToString().Trim());
                Log stack = new Log("Stack: " + e.StackTrace.ToString().Trim());
                Instance.LogQueue.Enqueue(msg);
                Instance.LogQueue.Enqueue(stack);

                // Check if should flush
                if (Instance.LogQueue.Count >= FlushAtQty || CheckTimeToFlush())
                {
                    FlushLogToFile();
                }

            }
        }

        /// <summary>
        /// Force flush of log queue
        /// </summary>
        public static void ForceFlush()
        {
            FlushLogToFile();
        }

        /// <summary>
        /// Check if time to flush to file
        /// </summary>
        /// <returns></returns>
        private static bool CheckTimeToFlush()
        {
            TimeSpan time = DateTime.Now - FlushedAt;
            if (time.TotalSeconds >= FlushAtAge)
            {
                FlushedAt = DateTime.Now;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Flush log queue to file
        /// </summary>
        private static void FlushLogToFile()
        {
            while (Instance.LogQueue.Count > 0)
            {
                // Get entry to log
                Log entry = Instance.LogQueue.Dequeue();
                string path = LogPath + entry.GetDate() + "_" + LogFile;


                FileInfo fileInfo = new FileInfo(path);

                if (!fileInfo.Directory.Exists)
                    new DirectoryInfo(fileInfo.DirectoryName).Create();

                FileStream stream;
                if (!fileInfo.Exists)
                    stream = new FileStream(new FileInfo(path).FullName, FileMode.Append, FileAccess.Write);
                else
                    stream = new FileStream(path, FileMode.Append, FileAccess.Write);
                // Crete filestream

                using (var writer = new StreamWriter(stream))
                {
                    // Log to file
                    writer.WriteLine($"{entry.GetTime()}\t{entry.GetMessage()}");
                }

                stream.Close();
            }
        }

    }

    /// <summary>
    /// Log container object
    /// </summary>
    public class Log
    {

        /// <summary>
        /// Content of log
        /// </summary>
        private string LogMessage;

        /// <summary>
        /// Log timestamp
        /// </summary>
        private DateTime LogTime;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Logged message</param>
        public Log(string message)
        {
            LogMessage = message;
            LogTime = DateTime.Now;
        }

        /// <summary>
        /// Log message accessor
        /// </summary>
        /// <returns>Log message</returns>
        public string GetMessage()
        {
            return LogMessage;
        }

        /// <summary>
        /// Get the time from log timestamp
        /// </summary>
        /// <returns>Time</returns>
        public string GetTime()
        {
            return LogTime.ToString("hh:mm:ss.fff tt");
        }

        /// <summary>
        /// Get the date from log timestamp
        /// </summary>
        /// <returns>Date</returns>
        public string GetDate()
        {
            return LogTime.ToString("yyyy-MM-dd");
        }

    }
}
