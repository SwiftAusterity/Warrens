using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Web.Configuration;

namespace NetMud.Logging
{
    public static class LoggingUtility
    {
        private string BaseDirectory = WebConfigurationManager.AppSettings("LogPath");

        public static void WriteToLog(string content, bool beQuiet = false)
        {
            WriteToLog(content, "General", beQuiet);
        }

        public static void WriteToLog(string content, string channel, bool beQuiet = false)
        {
            //Write to the log file first
            WriteToFile(content, channel);

            //Quiet means only write to file, non-quiet means write to whomever is subscribed
            if(!beQuiet)
            {
                //TODO: write to people in game
                //TODO: Write to some source that can push to the web
            }
        }

        private static void WriteToFile(string content, string channel)
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

                var bytes = Encoding.UTF8.GetBytes(content);
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