using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Configuration;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.Utility;

namespace NetMud.DataAccess
{
    /// <summary>
    /// Static expected log types for easier log coalation
    /// </summary>
    public enum LogChannels
    {
        CommandUse,
        Restore,
        Backup,
        BackingDataAccess,
        AccountActivity,
        Authentication,
        ProcessingLoops,
        SocketCommunication
    }

    /// <summary>
    /// Publically available wrapper for logging
    /// </summary>
    public static class LoggingUtility
    {
        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="ex">the exception</param>
        public static void LogError(Exception ex)
        {
            var errorContent = String.Format("{0}: {1}{2}{3}", ex.GetType().Name, ex.Message, Environment.NewLine, ex.StackTrace);

            CommitLog(errorContent, "SystemError", true);
        }

        /// <summary>
        /// Log an admin command being used
        /// </summary>
        /// <param name="commandString">the command being used</param>
        /// <param name="accountName">the account using it (user not character)</param>
        public static void LogAdminCommandUsage(string commandString, string accountName)
        {
            var content = String.Format("{0}: {1}", accountName, commandString);

            CommitLog(content, "AdminCommandUse", true);
        }

        /// <summary>
        /// Gets all the current log file names in Current
        /// </summary>
        /// <returns>a list of the log file names</returns>
        public static IEnumerable<string> GetCurrentLogNames()
        {
            var logger = new Logger(WebConfigurationManager.AppSettings["LogPath"]);

            return logger.GetCurrentLogNames();
        }

        /// <summary>
        /// Gets a current log file's contents
        /// </summary>
        /// <param name="channel">the log file name</param>
        /// <returns>the content</returns>
        public static string GetCurrentLogContent(string channel)
        {
            var logger = new Logger(WebConfigurationManager.AppSettings["LogPath"]);

            return logger.GetCurrentLogContent(channel);
        }

        /// <summary>
        /// Log one entry to a pre-determined channel
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="channel">which log to append it to</param>
        /// <param name="keepItQuiet">Announce it in game or not</param>
        public static void Log(string content, LogChannels channel, bool keepItQuiet = false)
        {
            CommitLog(content, channel.ToString(), keepItQuiet);
        }

        /// <summary>
        /// Archives a log file
        /// </summary>
        /// <param name="channel">the log file to archive</param>
        /// <returns>success status</returns>
        public static bool RolloverLog(string channel)
        {
            var logger = new Logger(WebConfigurationManager.AppSettings["LogPath"]);

            return logger.RolloverLog(channel);
        }

        /// <summary>
        /// commits content to a log channel
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="channel">which log to append it to</param>
        /// <param name="keepItQuiet">Announce it in game or not</param>
        private static void CommitLog(string content, string channel, bool keepItQuiet)
        {
            var logger = new Logger(WebConfigurationManager.AppSettings["LogPath"]);

            logger.WriteToLog(content, channel, keepItQuiet);
        }
    }

    /// <summary>
    /// Internal file access for logging
    /// </summary>
    internal class Logger
    {
        /// <summary>
        /// Base directory to push logs to
        /// </summary>
        private string BaseDirectory;

        /// <summary>
        /// Create an instance of the logger
        /// </summary>
        /// <param name="logDirectoryPath">Base directory to push logs to</param>
        public Logger(string logDirectoryPath)
        {
            BaseDirectory = logDirectoryPath;
        }

        /// <summary>
        /// Write to a log
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="beQuiet">Announce it in game or not</param>
        public void WriteToLog(string content, bool beQuiet = false)
        {
            WriteToLog(content, "General", beQuiet);
        }

        /// <summary>
        /// Write to a log
        /// </summary>
        /// <param name="content">the content to log</param>
        /// <param name="channel">which log to append it to</param>
        /// <param name="keepItQuiet">Announce it in game or not</param>
        public void WriteToLog(string content, string channel, bool beQuiet = false)
        {
            //Write to the log file first
            WriteToFile(content, channel);

            //Quiet means only write to file, non-quiet means write to whomever is subscribed
            if (!beQuiet)
            {
                //write to people in game
                var peeps = LiveCache.GetAll<IPlayer>().Where(peep => ((ICharacter)peep.DataTemplate).Account.LogChannelSubscriptions.Contains(channel));

                foreach (var peep in peeps)
                    peep.WriteTo(new string[] { content });

                //Low Priority TODO: Write to some source that can push to the web
            }
        }

        /// <summary>
        /// Gets all the current log file names in Current
        /// </summary>
        /// <returns>a list of the log file names</returns>
        public IEnumerable<string> GetCurrentLogNames()
        {
            var names = Enumerable.Empty<string>();

            if (!String.IsNullOrWhiteSpace(BaseDirectory) && Directory.Exists(BaseDirectory) && Directory.Exists(BaseDirectory + "Current/"))
                names = Directory.EnumerateFiles(BaseDirectory + "Current/", "*.txt", SearchOption.TopDirectoryOnly);

            return names.Select(nm => nm.Substring(nm.LastIndexOf('/') + 1, nm.Length - nm.LastIndexOf('/') - 5));
        }

        /// <summary>
        /// Archives a log file
        /// </summary>
        /// <param name="channel">the log file to archive</param>
        /// <returns>success status</returns>
        public bool RolloverLog(string channel)
        {
            var currentLogName = BaseDirectory + "Current/" + channel + ".txt";
            var archiveLogName = String.Format("{0}Archive/{1}_{2}{3}{4}_{5}{6}{7}.txt",
                    BaseDirectory
                    , channel
                    , DateTime.Now.Year
                    , DateTime.Now.Month
                    , DateTime.Now.Day
                    , DateTime.Now.Hour
                    , DateTime.Now.Minute
                    , DateTime.Now.Second);


            if (!String.IsNullOrWhiteSpace(BaseDirectory)
                && Directory.Exists(BaseDirectory)
                && Directory.Exists(BaseDirectory + "Current/")
                && File.Exists(currentLogName))
            {
                if (!Directory.Exists(BaseDirectory + "Archive/"))
                    Directory.CreateDirectory(BaseDirectory + "Archive/");

                File.Move(currentLogName, archiveLogName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a current log file's contents
        /// </summary>
        /// <param name="channel">the log file name</param>
        /// <returns>the content</returns>
        public string GetCurrentLogContent(string channel)
        {
            var content = String.Empty;

            if (!String.IsNullOrWhiteSpace(BaseDirectory) 
                && Directory.Exists(BaseDirectory) 
                && Directory.Exists(BaseDirectory + "Current/") 
                && File.Exists(BaseDirectory + "Current/" + channel + ".txt"))
            { 
                using (var logFile = File.Open(BaseDirectory + "Current/" + channel + ".txt", FileMode.Open))
                {
                    byte[] bytes = new byte[logFile.Length];
                    logFile.Read(bytes, 0, (int)logFile.Length);
                    content = Encoding.UTF8.GetString(bytes);
                }
            }

            return content;
        }

        /// <summary>
        /// Writes content to a log file
        /// </summary>
        /// <param name="content">the content to write</param>
        /// <param name="channel">the log file to append it to</param>
        private void WriteToFile(string content, string channel)
        {
            FileStream thisLog = null;

            try
            {
                //Bail, why is there no base directory?
                if (String.IsNullOrWhiteSpace(BaseDirectory))
                    return;

                if (!Directory.Exists(BaseDirectory))
                    Directory.CreateDirectory(BaseDirectory);

                var currentDirectory = BaseDirectory + "Current/";
                if (!Directory.Exists(currentDirectory))
                    Directory.CreateDirectory(currentDirectory);

                if (!File.Exists(currentDirectory + channel + ".txt"))
                    thisLog = File.Create(currentDirectory + channel + ".txt");
                else
                    thisLog = File.Open(currentDirectory + channel + ".txt", FileMode.Append);
                //Add a line terminator PLEASE
                content += Environment.NewLine;
                var timeStamp = String.Format("[{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}]:  ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                var bytes = Encoding.UTF8.GetBytes(timeStamp + content);
                thisLog.Write(bytes, 0, bytes.Length);

                //Don't forget to write the file out
                thisLog.Flush();
            }
            catch
            {
                //dont throw on trying to write the log
            }
            finally
            {
                //dont not do this everEVERVERRFCFEVVEEV
                if (thisLog != null)
                    thisLog.Dispose();
            }
        }
    }
}