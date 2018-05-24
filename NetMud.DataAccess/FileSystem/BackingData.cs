using NetMud.DataStructure.Base.System;
using System;
using System.IO;
using System.Web.Hosting;

namespace NetMud.DataAccess.FileSystem
{
    public class BackingData : FileAccessor
    {
        /// <summary>
        /// Root directory where all the backup stuff gets saved too
        /// </summary>
        public override string BaseDirectory
        {
            get
            {
                return HostingEnvironment.MapPath(base.BaseDirectory + "BackingData/");
            }
        }

        /// <summary>
        /// The default directory name for when files are rolled over or archived
        /// </summary>
        public override string DatedBackupDirectory
        {
            get
            {
                return String.Format("{0}{1}{2}{3}_{4}{5}/",
                                        ArchiveDirectoryName
                                        , DateTime.Now.Year
                                        , DateTime.Now.Month
                                        , DateTime.Now.Day
                                        , DateTime.Now.Hour
                                        , DateTime.Now.Minute);
            }
        }

        public IData ReadEntity(FileInfo file, Type entityType)
        {
            var fileData = ReadFile(file);
            var blankEntity = Activator.CreateInstance(entityType) as IData;

            return blankEntity.FromBytes(fileData) as IData;
        }

        /// <summary>
        /// Write one backing data entity out
        /// </summary>
        /// <param name="entity">the thing to write out to current</param>
        public void WriteEntity(IData entity)
        {
            var dirName = BaseDirectory + CurrentDirectoryName + entity.GetType().Name + "/";

            if (!VerifyDirectory(dirName))
                throw new Exception("Unable to locate or create base live data directory.");

            var entityFileName = GetEntityFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dirName + entityFileName;
            var archiveFileDirectory = BaseDirectory + DatedBackupDirectory + entity.GetType().Name + "/";

            try
            {
                RollingArchiveFile(fullFileName, archiveFileDirectory + entityFileName, archiveFileDirectory);
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
        public void ArchiveEntity(IData entity)
        {
            var dirName = BaseDirectory + CurrentDirectoryName + entity.GetType().Name + "/";

            if (!VerifyDirectory(dirName))
                throw new Exception("Unable to locate or create base live data directory.");

            var entityFileName = GetEntityFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dirName + entityFileName;
            var archiveFileDirectory = BaseDirectory + DatedBackupDirectory + entity.GetType().Name + "/";

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

            File.Move(currentFile, archiveFile);
        }

        /// <summary>
        /// Archives everything
        /// </summary>
        public void ArchiveFull()
        {
            //wth, no current directory? Noithing to move then
            if (VerifyDirectory(BaseDirectory + CurrentDirectoryName, false) && VerifyDirectory(BaseDirectory + ArchiveDirectoryName))
            {
                var currentRoot = new DirectoryInfo(BaseDirectory + CurrentDirectoryName);

                //move is literal move, no need to delete afterwards
                currentRoot.MoveTo(BaseDirectory + DatedBackupDirectory);
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
        private string GetEntityFilename(IData entity)
        {
            return String.Format("{0}.{1}", entity.Id, entity.GetType().Name);
        }
    }
}
