using System;
using System.IO;
using System.Linq;

namespace NetMud.DataAccess.FileSystem
{
    public abstract class FileAccessor
    {
        /// <summary>
        /// The base directory for these files, should be overriden
        /// </summary>
        public virtual string BaseDirectory
        {
            get
            {
                return "/FileStore/";
            }
        }

        /// <summary>
        /// Directory name for whatever is "current", should probably be left alone
        /// </summary>
        public virtual string CurrentDirectoryName
        {
            get
            {
                return "Current/";
            }
        }

        /// <summary>
        /// Directory for where archived files are moved to
        /// </summary>
        public virtual string ArchiveDirectoryName
        {
            get
            {
                return "Backups/";
            }
        }

        /// <summary>
        /// The default directory name for when files are rolled over or archived
        /// </summary>
        public virtual string DatedBackupDirectory
        {
            get
            {
                return string.Format("{0}{1}{2}{3}{4}_{5}{6}{7}/",
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

        /// <summary>
        /// Reads a file from the "current" directory by a filename
        /// </summary>
        /// <param name="fileName">the file to read in</param>
        /// <returns>the file's data</returns>
        public byte[] ReadCurrentFileByPath(string fileName)
        {
            byte[] bytes = new byte[0];
            string filePath = BaseDirectory + CurrentDirectoryName + fileName;

            if (VerifyDirectory(BaseDirectory)
                || VerifyDirectory(BaseDirectory + CurrentDirectoryName)
                || File.Exists(BaseDirectory + CurrentDirectoryName + fileName))
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open))
                {
                    bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                }
            }

            return bytes;
        }

        /// <summary>
        /// Reads the contents of a file already opened
        /// </summary>
        /// <param name="file">the file to read from</param>
        /// <returns>the file's data</returns>
        public byte[] ReadFile(FileInfo file)
        {
            byte[] bytes = new byte[0];

            using (FileStream stream = file.Open(FileMode.Open))
            {
                bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
            }

            return bytes;
        }

        /// <summary>
        /// Verifies the existence of or creates a new directory, also creates the base directory if necessary
        /// </summary>
        /// <param name="directoryName">the directory to create</param>
        /// <param name="createIfMissing">creates the directory if it doesn't already exist</param>
        /// <returns>success</returns>
        public bool VerifyDirectory(string directoryName, bool createIfMissing = true)
        {
            string mappedName = directoryName;

            if (!directoryName.Contains(BaseDirectory))
            {
                mappedName = string.Format("{0}{1}", BaseDirectory, directoryName);
            }

            if (!mappedName.EndsWith("/"))
            {
                mappedName += "/";
            }

            try
            {
                //always create the base dir
                if (!Directory.Exists(BaseDirectory))
                {
                    Directory.CreateDirectory(BaseDirectory);
                }

                if (Directory.Exists(mappedName))
                {
                    return true;
                }

                if (createIfMissing)
                {
                    return Directory.CreateDirectory(mappedName) != null;
                }
            }
            catch (Exception ex)
            {
                //Log any filesystem errors
                LoggingUtility.LogError(ex, false);
            }

            return false;
        }

        /// <summary>
        /// Writes a bytestream to a specific file
        /// </summary>
        /// <param name="fullFileName">the fully qualified filename (with pathing)</param>
        /// <param name="bytes">the data to write</param>
        /// <param name="backupFirst">should this file be archived first using the default archiving directory structure</param>
        /// <param name="writeMode">should this file be overwritten or appended to</param>
        public bool WriteToFile(string fullFileName, byte[] bytes, FileMode writeMode = FileMode.Truncate)
        {
            FileStream entityFile = null;
            bool success = true;

            try
            {
                if (File.Exists(fullFileName))
                {
                    entityFile = File.Open(fullFileName, writeMode);
                }
                else
                {
                    entityFile = File.Create(fullFileName);
                }

                entityFile.Write(bytes, 0, bytes.Length);

                //Don't forget to write the file out
                entityFile.Flush();
            }
            catch (IOException)
            {
                //TODO: want to retry this one, def dont log errors
                success = false;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                success = false;
            }
            finally
            {
                if (entityFile != null)
                {
                    entityFile.Dispose();
                }
            }

            return success;
        }

        /// <summary>
        /// Rolls over a file using the default archive settings
        /// </summary>
        /// <param name="currentFileName">the file to be rolled over</param>
        /// <param name="archiveFileName">the filename to be rolled over to</param>
        /// <returns>success</returns>
        public bool ArchiveFile(string currentFileName, string archiveFileName)
        {
            currentFileName = string.Format("{0}{1}{2}", BaseDirectory, CurrentDirectoryName, currentFileName);
            archiveFileName = string.Format("{0}{1}{2}", BaseDirectory, ArchiveDirectoryName, archiveFileName);

            //Why backup something that doesnt exist
            if (!VerifyDirectory(BaseDirectory + CurrentDirectoryName)
                || !VerifyDirectory(BaseDirectory + ArchiveDirectoryName)
                || !File.Exists(currentFileName))
            {
                return false;
            }

            CullDirectoryCount(BaseDirectory + ArchiveDirectoryName);

            File.Move(currentFileName, archiveFileName);

            return true;
        }

        /// <summary>
        /// Archives a file into a date formatted directory
        /// </summary>
        /// <param name="fileName">the file to roll over</param>
        /// <param name="dateFormattedDirectory">overrides the default DatedBackupDirectory setting</param>
        /// <returns>success</returns>
        public bool ArchiveDatedFile(string fileName, string dateFormattedDirectory = "")
        {
            if (string.IsNullOrWhiteSpace(dateFormattedDirectory))
            {
                dateFormattedDirectory = DatedBackupDirectory;
            }

            string currentFileName = string.Format("{0}{1}{2}", BaseDirectory, CurrentDirectoryName, fileName);
            string archiveFileName = string.Format("{0}{1}", dateFormattedDirectory, fileName);

            //Why backup something that doesnt exist
            if (!VerifyDirectory(BaseDirectory + CurrentDirectoryName)
                || !VerifyDirectory(BaseDirectory + ArchiveDirectoryName)
                || !File.Exists(currentFileName))
            {
                return false;
            }

            CullDirectoryCount(BaseDirectory + ArchiveDirectoryName);

            File.Move(currentFileName, archiveFileName);
            return true;
        }

        internal void CullDirectoryCount(string baseDirectoryPath)
        {
            try
            {
                var dirs = Directory.GetDirectories(baseDirectoryPath);

                if (dirs.Count() > 10)
                {
                    IOrderedEnumerable<string> backupDirs = dirs.Where(dir => char.IsNumber(dir.Last())).OrderByDescending(dirName => dirName);

                    //Remove some
                    foreach (string dirName in backupDirs.Skip(10))
                    {
                        Directory.Delete(dirName, true);
                    }
                }
            }
            catch
            {
                //just eat it for now
            }
        }
    }
}
