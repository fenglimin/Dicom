using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection;

namespace EK.Capture.Dicom.DicomToolKit
{
    public enum LogLevel
    {
        Unknown,
        Error,
        Warning,
        Info,
        Verbose,
        Dump
    }

    public class LoggingEventArgs : EventArgs
    {
        private LogLevel level;
        private string message;

        public LoggingEventArgs(LogLevel level, string message)
        {
            this.level = level;
            this.message = message;
        }

        public LogLevel Level
        {
            get
            {
                return level;
            }
        }

        public string Message
        {
            get
            {
                return message;
            }
        }
    }

    public delegate void LoggingEventHandler(object sender, LoggingEventArgs e);

    public static class Logging
    {
        private static LogLevel level = LogLevel.Info;
        private static List<string> cache = new List<string>();
        private const int MaximumCachedMessages = 12;
        private static object sentry = new object();

        public static event LoggingEventHandler LogMessage;

        /// <summary>
        /// Determine if a certain level of logging is currently enabled.
        /// </summary>
        /// <param name="level">The level to check.</param>
        /// <returns>True if at least this level of logging is enabled.</returns>
        /// <remarks>Knowing that a particular level of logging is enabled or disabled can be helpful 
        /// in avoiding the computational overhead involved in generating the text for a logging call when that
        /// logging level is not enabled.  This is useful in some situations, but works against the caching
        /// of log messages.</remarks>
        public static bool IsEnabled(LogLevel level)
        {
            return (Logging.level >= level);
        }

        public static LogLevel LogLevel
        {
            get
            {
                return Logging.level;
            }
            set
            {
                cache.Clear();
                Logging.level = value;
            }
        }

        public static string Log(string message)
        {
            string result = String.Empty;
            try
            {
                result = Log(LogLevel.Verbose, message);
            }
            catch
            {
                Debug.WriteLine("LoggingError:"+message);
            }
            return result;
        }

        public static string Log(string format, params object[] list)
        {
            string result = String.Empty;
            try
            {
                result = String.Format(format, list);
                result = Log(LogLevel.Verbose, result);
            }
            catch
            {
                Debug.WriteLine("LoggingError:" + result);
            }
            return result;
        }

        public static string Log(LogLevel level, string format, params object[] list)
        {
            string result = String.Empty;
            try
            {
                result = String.Format(format, list);
                result = Log(level, result);
            }
            catch
            {
                Debug.WriteLine("LoggingError:" + result);
            }
            return result;
        }

        public static string Log(string message, byte[] bytes)
        {
            string result = String.Empty;
            try
            {
                result = Log(LogLevel.Verbose, message, bytes);
            }
            catch
            {
                Debug.WriteLine("LoggingError:" + message);
            }
            return result;
        }

        public static string Log(LogLevel level, string message, byte[] bytes)
        {
            string result = String.Empty;
            try
            {
                result = DicomObject.ToText(bytes);
                result = result + "\r\n" + result;
                result = Log(level, result);
            }
            catch
            {
                Debug.WriteLine("LoggingError:" + result);
            }
            return result;
        }

        public static string Log(DicomObject @object)
        {
            string result = String.Empty;
            try
            {
                if (@object != null)
                {
                    result = Log(LogLevel.Dump, @object.GetType().Name, @object.ToArray());
                }
            }
            catch
            {
                Debug.WriteLine("LoggingError:" + (string)((@object != null) ? @object.GetType().Name : ""));
            }
            return result;
        }

        public static string Log(LogLevel level, string message)
        {
            string result = String.Empty;
            try
            {
                HandleCachedMessages(level, message);
                result = Log(new LoggingEventArgs(level, message));
            }
            catch
            {
                Debug.WriteLine("LoggingError:" + message);
            }
            return result;
        }

        public static string Log(Exception ex)
        {
            string result = String.Empty;
            try
            {
                result = Log(null, ex);
            }
            catch
            {
                Debug.WriteLine("LoggingError:" + ex.Message);
            }
            return result;
        }

        public static string Log(string message, Exception ex)
        {
            string result = String.Empty;
            try
            {
                StringBuilder text = new StringBuilder();
                if (message != null && message.Length >= 0)
                {
                    text.Append(message);
                    text.Append("\r\n");
                }
                text.Append(ExpandMessage(ex));
                result = Log(LogLevel.Error, text.ToString());
            }
            catch
            {
                Debug.WriteLine("LoggingError:" + message + "\r\n" + ex.Message);
            }
            return result;
        }

        private static void HandleCachedMessages(LogLevel level, string message)
        {
            lock (sentry)
            {
                if (level == LogLevel.Error)
                {
                    foreach (string text in cache)
                    {
                        Log(new LoggingEventArgs(LogLevel.Unknown, "*"+text));
                    }
                    cache.Clear();
                }
                else if (LogLevel < level)
                {
                    cache.Add(message);
                    if (cache.Count > MaximumCachedMessages)
                    {
                        cache.RemoveAt(0);
                    }
                }
            }
        }

        private static string Log(LoggingEventArgs e)
        {
            string result = String.Empty;
            try
            {
                if (LogMessage != null)
                {
                    LogMessage(null, e);
                }
                else
                {
                    Debug.WriteLine(e.Message);
                }
            }
            catch
            {
                Debug.WriteLine("LoggingError:" + e.Message);
            }
            return e.Message;
        }

        /// <summary>
        /// Return the message of the exception concatenated with the message text from all inner exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns>The text of all the messages.</returns>
        public static string ExpandMessage(Exception ex)
        {
            StringBuilder text = new StringBuilder();
            Exception temp = ex;
            while (temp != null)
            {
                text.Append(temp.Message + "\n\n");
                temp = temp.InnerException;
            }
            text.Append(ex.StackTrace);
            return text.ToString();
        }
    }

    public sealed class Log
    {
        static readonly Log instance = new Log();
        static readonly object sentry = new object();
        static FileStream stream;
        static List<long> index;
        static Encoding encoding;
        public static event LoggingEventHandler LogMessage;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Log()
        {
        }

        // private and inaccessible
        private Log()
        {
        }

        public static long Count
        {
            get
            {
                return index.Count;
            }
        }

        private static string GetText(long start, long count)
        {
            byte[] bytes = null;
            lock (sentry)
            {
                if (index.Count == 0)
                {
                    return String.Empty;
                }
                if (start + count < index.Count)
                {
                    bytes = new byte[(int)index[(int)(start + count)] - index[(int)start]];
                }
                else
                {
                    long eof = stream.Seek(0, SeekOrigin.End);
                    bytes = new byte[eof - index[(int)start]];
                }
                stream.Seek(index[(int)start], SeekOrigin.Begin);
                stream.Read(bytes, 0, bytes.Length);
            }
            return encoding.GetString(bytes).TrimEnd();
        }

        public static string[] GetLines(long start, long count)
        {
            string text = GetText(start, count);
            return text.Split("\n".ToCharArray());
        }

        public static string GetText()
        {
            return GetText(0, index.Count);
        }

        public static void OnLogMessage(object sender, LoggingEventArgs e)
        {
            // TOD get this parsing onto a background thread
            lock (sentry)
            {
                long offset = stream.Seek(0, SeekOrigin.End);

                int crlf = "\r\n".Length;
                int count = e.Message.Length + crlf;

                byte[] array = encoding.GetBytes((e.Message + "\r\n").ToCharArray());
                stream.Write(array, 0, count);
                stream.Flush();

                string[] lines = e.Message.Split("\n".ToCharArray());
                foreach (string line in lines)
                {
                    index.Add(offset);
                    offset += line.Length + crlf;
                }
            }
            LogMessage(instance, e);
        }

        public static void Start(LogLevel level)
        {
            lock (sentry)
            {
                if (index == null)
                {
                    encoding = Encoding.Default;

                    string dtkLogDir = ".\\";
                    try
                    {
                        dtkLogDir = ConfigurationManager.OpenExeConfiguration(Assembly.GetCallingAssembly().Location).AppSettings.Settings["DtkLogDir"].Value;
                    }
                    catch (Exception)
                    {
                        // If can't get the DtkLogDir from the configuration file, use the default dtkLogDir.
                    }

                    string dtkLogPath = Path.Combine(dtkLogDir, string.Format("{0}_dtk.log", Process.GetCurrentProcess().ProcessName));
                    stream = new FileStream(dtkLogPath, FileMode.Create, FileAccess.ReadWrite);
                    index = new List<long>();

                    Logging.LogLevel = level;
                    Logging.LogMessage += new LoggingEventHandler(OnLogMessage);
                }
            }
        }

        public static void Stop()
        {
            lock (sentry)
            {

                Logging.LogLevel = LogLevel.Info;
                Logging.LogMessage -= new LoggingEventHandler(OnLogMessage);

                index = null;
                stream.Close();
                stream.Dispose();
            }
        }
    }
}
