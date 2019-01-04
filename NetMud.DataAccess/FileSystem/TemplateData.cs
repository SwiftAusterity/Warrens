using NetMud.DataStructure.Architectural;
using NetMud.Utility;
using System;
using System.IO;
using System.Web.Hosting;

namespace NetMud.DataAccess.FileSystem
{
    public class TemplateData : FileAccessor
    {
        /// <summary>
        /// Root directory where all the backup stuff gets saved too
        /// </summary>
        public override string BaseDirectory
        {
            get
            {
                return HostingEnvironment.MapPath(base.BaseDirectory + "Templates/");
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

        public IKeyedData ReadEntity(FileInfo file, Type entityType)
        {
            byte[] fileData = ReadFile(file);
            IKeyedData blankEntity = Activator.CreateInstance(entityType) as IKeyedData;

            return blankEntity.FromBytes(fileData) as IKeyedData;
        }

        /// <summary>
        /// Write one backing data entity out
        /// </summary>
        /// <param name="entity">the thing to write out to current</param>
        public void WriteEntity(IKeyedData entity)
        {
            try
            {
                if (entity.FitnessProblems)
                    throw new Exception("Attempt to write bad entity.");

                string dirName = BaseDirectory + CurrentDirectoryName + entity.GetType().Name + "/";

                if (!VerifyDirectory(dirName))
                    throw new Exception("Unable to locate or create base backing data directory.");

                string entityFileName = GetEntityFilename(entity);

                if (string.IsNullOrWhiteSpace(entityFileName))
                    return;

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
        public void ArchiveEntity(IKeyedData entity)
        {
            string dirName = BaseDirectory + CurrentDirectoryName + entity.GetType().Name + "/";

            if (!VerifyDirectory(dirName))
                throw new Exception("Unable to locate or create base live data directory.");

            string entityFileName = GetEntityFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            string fullFileName = dirName + entityFileName;
            string archiveFileDirectory = BaseDirectory + DatedBackupDirectory + entity.GetType().Name + "/";

            CullDirectoryCount(BaseDirectory + ArchiveDirectoryName);

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
        /// Creates rolling files since backing data is dated by minute
        /// </summary>
        /// <param name="currentFile">full path of current file name</param>
        /// <param name="archiveFile">full path of archive file name</param>
        /// <param name="archiveDirectory">archive directory</param>
        private void RollingArchiveFile(string currentFile, string archiveFile, string archiveDirectory)
        {
            if (!File.Exists(currentFile))
                return;

            VerifyDirectory(archiveDirectory);

            if (File.Exists(archiveFile))
                File.Delete(archiveFile);

            File.Copy(currentFile, archiveFile, true);
        }

        /// <summary>
        /// Archives everything
        /// </summary>
        public void ArchiveFull()
        {
            //wth, no current directory? Noithing to move then
            if (VerifyDirectory(BaseDirectory + CurrentDirectoryName, false) && VerifyDirectory(BaseDirectory + ArchiveDirectoryName))
            {
                CullDirectoryCount(BaseDirectory + ArchiveDirectoryName);

                DirectoryInfo currentRoot = new DirectoryInfo(BaseDirectory + CurrentDirectoryName);

                currentRoot.CopyTo(BaseDirectory + DatedBackupDirectory);
            }

            //something very wrong is happening, it'll get logged
            if (!VerifyDirectory(BaseDirectory + CurrentDirectoryName))
                throw new Exception("Can not locate or verify current data directory.");
        }

        /// <summary>
        /// Gets the statically formatted filename for an entity
        /// </summary>
        /// <param name="entity">The entity in question</param>
        /// <returns>the filename</returns>
        private string GetEntityFilename(IKeyedData entity)
        {
            return string.Format("{0}.{1}", entity.Id, entity.GetType().Name);
        }
    }
}
