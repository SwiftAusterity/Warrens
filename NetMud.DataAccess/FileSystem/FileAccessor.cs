using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace NetMud.DataAccess.FileSystem
{
    public abstract class FileAccessor
    {
        internal virtual string BaseDirectory
        {
            get
            {
                return HostingEnvironment.MapPath("/");
            }
        }

        internal virtual string CurrentDirectoryName
        {
            get
            {
                return "Current/";
            }
        }

        internal virtual string ArchiveDirectoryName
        {
            get
            {
                return "Backups/";
            }
        }

        internal virtual string DatedBackupDirectory
        {
            get
            {
                return String.Format("{0}{1}{2}{3}{4}_{5}{6}{7}/",
                                        BaseDirectory
                                        , ArchiveDirectoryName
                                        , DateTime.Now.Year
                                        , DateTime.Now.Month
                                        , DateTime.Now.Day
                                        , DateTime.Now.Hour
                                        , DateTime.Now.Minute
                                        , DateTime.Now.Second);
            }
        }

        internal byte[] ReadFile(FileInfo file)
        {
            byte[] bytes = new byte[0];

            using (var stream = file.Open(FileMode.Open))
            {
                bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
            }

            return bytes;
        }

        internal bool VerifyDirectory(string directoryName, bool createIfMissing = true)
        {
            var mappedName = String.Format("{0}{1}/", BaseDirectory, directoryName);

            try
            {
                if (Directory.Exists(mappedName))
                    return true;

                if (createIfMissing)
                    return Directory.CreateDirectory(mappedName) != null;
            }
            catch (Exception ex)
            {
                //Log any filesystem errors
                LoggingUtility.LogError(ex);
            }

            return false;
        }

        internal void WriteToFile(string fullFileName, byte[] bytes, bool backupFirst = false, FileMode writeMode = FileMode.Truncate)
        {
            FileStream entityFile = null;

            try
            {
                if (File.Exists(fullFileName))
                {
                    if(backupFirst)
                    {
                        ArchiveFile(fullFileName);
                        entityFile = File.Create(fullFileName);
                    }
                    else
                        entityFile = File.Open(fullFileName, writeMode);
                }
                else
                    entityFile = File.Create(fullFileName);

                entityFile.Write(bytes, 0, bytes.Length);

                //Don't forget to write the file out
                entityFile.Flush();
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
            finally
            {
                if (entityFile != null)
                    entityFile.Dispose();
            }
        }

        internal bool ArchiveFile(string fileName)
        {
            var currentFileName = String.Format("{0}{1}{2}", BaseDirectory, CurrentDirectoryName, fileName);
            var archiveFileName = String.Format("{0}{1}{2}", BaseDirectory, ArchiveDirectoryName, fileName);

            //Why backup something that doesnt exist
            if (!VerifyDirectory(BaseDirectory)
                || !VerifyDirectory(BaseDirectory + CurrentDirectoryName)
                || !VerifyDirectory(BaseDirectory + ArchiveDirectoryName)
                || !File.Exists(currentFileName))
                return false;

            File.Move(currentFileName, archiveFileName);
            return true;
        }
    }
}
