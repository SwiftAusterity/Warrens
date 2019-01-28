using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace NetMud.DataAccess
{

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
                IEnumerable<IPlayer> peeps = LiveCache.GetAll<IPlayer>().Where(peep => peep.Template<IPlayerTemplate>() != null && peep.Template<IPlayerTemplate>().Account != null && 
                                                                        peep.Template<IPlayerTemplate>().Account.LogChannelSubscriptions.Contains(channel));

                foreach (IPlayer peep in peeps)
                {
                    peep.WriteTo(new string[] { content });
                }
            }
        }

        /// <summary>
        /// Gets all the current log file names in Current
        /// </summary>
        /// <returns>a list of the log file names</returns>
        public IEnumerable<string> GetCurrentLogNames()
        {
            IEnumerable<string> names = Enumerable.Empty<string>();

            if (VerifyDirectory(CurrentDirectoryName, false))
            {
                names = Directory.EnumerateFiles(BaseDirectory + "Current/", "*.txt", SearchOption.TopDirectoryOnly);
            }

            return names.Select(nm => nm.Substring(nm.LastIndexOf('/') + 1, nm.Length - nm.LastIndexOf('/') - 5));
        }

        /// <summary>
        /// Archives a log file
        /// </summary>
        /// <param name="channel">the log file to archive</param>
        /// <returns>success status</returns>
        public bool RolloverLog(string channel)
        {
            string archiveLogName = string.Format("{0}_{1}{2}{3}_{4}{5}{6}.txt",
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
            string content = string.Empty;

            byte[] bytes = ReadCurrentFileByPath(channel + ".txt");

            if(bytes.Length > 0)
            {
                content = Encoding.UTF8.GetString(bytes);
            }

            return content;
        }

        /// <summary>
        /// Writes content to a log file
        /// </summary>
        /// <param name="content">the content to write</param>
        /// <param name="channel">the log file to append it to</param>
        private void WriteLine(string content, string channel)
        {
            string dirName = BaseDirectory + CurrentDirectoryName;

            if (!VerifyDirectory(CurrentDirectoryName))
            {
                throw new Exception("Unable to locate or create base live logs directory.");
            }

            string fileName = channel + ".txt";
            string timeStamp = string.Format("[{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}]:  ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            //Add a line terminator PLEASE
            content += Environment.NewLine;

            byte[] bytes = Encoding.UTF8.GetBytes(timeStamp + content);

            WriteToFile(dirName + fileName, bytes, FileMode.Append);
        }
    }
}