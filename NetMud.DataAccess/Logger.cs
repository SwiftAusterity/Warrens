using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataAccess.FileSystem;
using System.Web.Hosting;
using NetMud.DataAccess.Cache;

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
        /// <param name="nonImportant">Should we get cross about this or not? Defaults to not.</param>
        public static void LogError(Exception ex, bool nonImportant = true)
        {
            var errorContent = String.Format("{0}: {1}{2}{3}", ex.GetType().Name, ex.Message, Environment.NewLine, ex.StackTrace);

            if(nonImportant)
                CommitLog(errorContent, "SystemFailures", true);
            else
                CommitLog(errorContent, "SystemError", false);
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
            var logger = new Logger();

            return logger.GetCurrentLogNames();
        }

        /// <summary>
        /// Gets a current log file's contents
        /// </summary>
        /// <param name="channel">the log file name</param>
        /// <returns>the content</returns>
        public static string GetCurrentLogContent(string channel)
        {
            var logger = new Logger();

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
            var logger = new Logger();

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
            var logger = new Logger();

            logger.WriteToLog(content, channel, keepItQuiet);
        }
    }

    /// <summary>
    /// Internal file access for logging
    /// </summary>
    internal class Logger : FileAccessor
    {
        /// <summary>
        /// Base directory to push logs to
        /// </summary>
        public override string BaseDirectory
        {
            get
            {
                return HostingEnvironment.MapPath(base.BaseDirectory + "Logs/");
            }
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
            WriteLine(content, channel);

            //Quiet means only write to file, non-quiet means write to whomever is subscribed
            if (!beQuiet)
            {
                //write to people in game
                var peeps = LiveCache.GetAll<IPlayer>().Where(peep => peep.DataTemplate<ICharacter>().Account.LogChannelSubscriptions.Contains(channel));

                foreach (var peep in peeps)
                    peep.WriteTo(new string[] { content });
            }
        }

        /// <summary>
        /// Gets all the current log file names in Current
        /// </summary>
        /// <returns>a list of the log file names</returns>
        public IEnumerable<string> GetCurrentLogNames()
        {
            var names = Enumerable.Empty<string>();

            if (VerifyDirectory(CurrentDirectoryName, false))
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
            var archiveLogName = String.Format("{0}_{1}{2}{3}_{4}{5}{6}.txt",
                    channel
                    , DateTime.Now.Year
                    , DateTime.Now.Month
                    , DateTime.Now.Day
                    , DateTime.Now.Hour
                    , DateTime.Now.Minute
                    , DateTime.Now.Second);

            return ArchiveFile(channel + ".txt", archiveLogName);
        }

        /// <summary>
        /// Gets a current log file's contents
        /// </summary>
        /// <param name="channel">the log file name</param>
        /// <returns>the content</returns>
        public string GetCurrentLogContent(string channel)
        {
            var content = String.Empty;

            var bytes = ReadCurrentFileByPath(channel + ".txt");

            if(bytes.Length > 0)
                content = Encoding.UTF8.GetString(bytes);

            return content;
        }

        /// <summary>
        /// Writes content to a log file
        /// </summary>
        /// <param name="content">the content to write</param>
        /// <param name="channel">the log file to append it to</param>
        private void WriteLine(string content, string channel)
        {
            var dirName = BaseDirectory + CurrentDirectoryName;

            if (!VerifyDirectory(CurrentDirectoryName))
                throw new Exception("Unable to locate or create base live logs directory.");

            var fileName = channel + ".txt";
            var timeStamp = String.Format("[{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}]:  ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            //Add a line terminator PLEASE
            content += Environment.NewLine;

            var bytes = Encoding.UTF8.GetBytes(timeStamp + content);

            WriteToFile(dirName + fileName, bytes, FileMode.Append);
        }
    }
}