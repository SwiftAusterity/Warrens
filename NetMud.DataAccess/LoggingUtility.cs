using System;
using System.Collections.Generic;

namespace NetMud.DataAccess
{
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
        public static void LogError(Exception ex, LogChannels specificLogName, bool keepItQuiet = true)
        {
            string errorContent = string.Format("{0}: {1}{2}{3}", ex.GetType().Name, ex.Message, Environment.NewLine, ex.StackTrace);

            CommitLog(errorContent, specificLogName.ToString(), keepItQuiet);
        }

        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="ex">the exception</param>
        /// <param name="nonImportant">Should we get cross about this or not? Defaults to not.</param>
        public static void LogError(Exception ex, bool nonImportant = true)
        {
            if(nonImportant)
            {
                LogError(ex, LogChannels.SystemWarnings);
            }
            else
            {
                LogError(ex, LogChannels.SystemErrors, false);
            }
        }

        /// <summary>
        /// Log an admin command being used
        /// </summary>
        /// <param name="commandString">the command being used</param>
        /// <param name="accountName">the account using it (user not character)</param>
        public static void LogAdminCommandUsage(string commandString, string accountName)
        {
            string content = string.Format("{0}: {1}", accountName, commandString);

            CommitLog(content, "AdminCommandUse", true);
        }

        /// <summary>
        /// Gets all the current log file names in Current
        /// </summary>
        /// <returns>a list of the log file names</returns>
        public static IEnumerable<string> GetCurrentLogNames()
        {
            Logger logger = new Logger();

            return logger.GetCurrentLogNames();
        }

        /// <summary>
        /// Gets a current log file's contents
        /// </summary>
        /// <param name="channel">the log file name</param>
        /// <returns>the content</returns>
        public static string GetCurrentLogContent(string channel)
        {
            Logger logger = new Logger();

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
            Logger logger = new Logger();

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
            Logger logger = new Logger();

            logger.WriteToLog(content, channel, keepItQuiet);
        }
    }
}