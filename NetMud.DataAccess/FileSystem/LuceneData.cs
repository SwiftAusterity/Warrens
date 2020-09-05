using NetMud.DataStructure.Architectural;
using NetMud.Utility;
using System;
using System.IO;
using System.Web.Hosting;

namespace NetMud.DataAccess.FileSystem
{
    public class LuceneData : FileAccessor
    {
        /// <summary>
        /// Root directory where all the backup stuff gets saved too
        /// </summary>
        public override string BaseDirectory
        {
            get
            {
                //Base dir is weird for config as it might be anywhere (but not outside the base dir)
                return HostingEnvironment.MapPath(base.BaseDirectory);
            }
        }

        /// <summary>
        /// The default directory name for when files are rolled over or archived
        /// </summary>
        public override string DatedBackupDirectory
        {
            get
            {
                return string.Format("{0}{1}{2}{3}_{4}{5}/",
                                        ArchiveDirectoryName
                                        , DateTime.Now.Year
                                        , DateTime.Now.Month
                                        , DateTime.Now.Day
                                        , DateTime.Now.Hour
                                        , DateTime.Now.Minute);
            }
        }

        public ILuceneData ReadEntity(FileInfo file, Type entityType)
        {
            byte[] fileData = ReadFile(file);
            ILuceneData blankEntity = Activator.CreateInstance(entityType) as ILuceneData;

            return blankEntity.FromBytes(fileData) as ILuceneData;
        }

        /// <summary>
        /// Write one backing data entity out
        /// </summary>
        /// <param name="entity">the thing to write out to current</param>
        public void WriteEntity(ILuceneData entity)
        {
            try
            {
                string dirName = GetCurrentDirectoryForEntity(entity);

                if (!VerifyDirectory(dirName))
                {
                    throw new Exception("Unable to locate or create base backing data directory.");
                }

                string entityFileName = GetEntityFilename(entity);

                if (string.IsNullOrWhiteSpace(entityFileName))
                {
                    return;
                }

                string fullFileName = dirName + entityFileName;

                ArchiveEntity(entity);
                WriteToFile(fullFileName, entity.ToBytes());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Archive a backing data entity
        /// </summary>
        /// <param name="entity">the thing to archive</param>
        public void ArchiveEntity(ILuceneData entity)
        {
            string dirName = GetCurrentDirectoryForEntity(entity);

            if (!VerifyDirectory(dirName))
            {
                throw new Exception("Unable to locate or create base live data directory.");
            }

            string entityFileName = GetEntityFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
            {
                return;
            }

            string fullFileName = dirName + entityFileName;
            string archiveFileDirectory = GetArchiveDirectoryForEntity(entity);

            CullDirectoryCount(GetBaseBackupDirectoryForEntity(entity));

            try
            {
                RollingArchiveFile(fullFileName, archiveFileDirectory + entityFileName, archiveFileDirectory);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Removes a entity from the system and files
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool RemoveEntity(ILuceneData entity)
        {
            string fileName = GetEntityFilename(entity);
            return ArchiveFile(fileName, fileName);
        }

        public string GetCurrentDirectoryForEntity(ILuceneData entity)
        {
            string dirName = BaseDirectory + "Lucene/";

            dirName += "/" + CurrentDirectoryName;

            return dirName;
        }

        public string GetCurrentDirectoryForType(Type entityType)
        {
            ILuceneData entityThing = Activator.CreateInstance(entityType) as ILuceneData;

            return GetCurrentDirectoryForEntity(entityThing);
        }

        private string GetBaseBackupDirectoryForEntity(ILuceneData entity)
        {
            string dirName = BaseDirectory + "Lucene/";

            dirName += "/" + ArchiveDirectoryName;

            return dirName;
        }

        private string GetArchiveDirectoryForEntity(ILuceneData entity)
        {
            string dirName = BaseDirectory + "Lucene/";

            dirName += "/" + DatedBackupDirectory;

            return dirName;
        }

        /// <summary>
        /// Archives everything
        /// </summary>
        public void ArchiveFull(string backupName = "")
        {
            string dirName = BaseDirectory + "Lucene/";

            string currentDirName = dirName + "/" + CurrentDirectoryName;
            string archivedDirName = dirName + "/" + ArchiveDirectoryName;

            //wth, no current directory? Noithing to move then
            if (VerifyDirectory(currentDirName, false) && VerifyDirectory(archivedDirName))
            {
                CullDirectoryCount(archivedDirName);

                DirectoryInfo currentRoot = new DirectoryInfo(currentDirName);

                string backupDir = archivedDirName + DatedBackupDirectory;
                if (!string.IsNullOrWhiteSpace(backupName))
                {
                    backupDir = string.Format("{0}{1}/", archivedDirName, backupName);
                }

                currentRoot.CopyTo(backupDir);
            }

            //something very wrong is happening, it'll get logged
            if (!VerifyDirectory(currentDirName))
            {
                throw new Exception("Can not locate or verify current data directory.");
            }
        }

        /// <summary>
        /// Creates rolling files since backing data is dated by minute
        /// </summary>
        /// <param name="currentFile">full path of current file name</param>
        /// <param name="archiveFile">full path of archive file name</param>
        /// <param name="archiveDirectory">archive directory</param>
        private void RollingArchiveFile(string currentFile, string archiveFile, string archiveDirectory)
        {
            if (!File.Exists(currentFile))
            {
                return;
            }

            VerifyDirectory(archiveDirectory);

            if (File.Exists(archiveFile))
            {
                File.Delete(archiveFile);
            }

            File.Copy(currentFile, archiveFile, true);
        }

        /// <summary>
        /// Gets the statically formatted filename for an entity
        /// </summary>
        /// <param name="entity">The entity in question</param>
        /// <returns>the filename</returns>
        public string GetEntityFilename(ILuceneData entity)
        {
            return string.Format("{0}", entity.GetType().Name);
        }
    }
}
