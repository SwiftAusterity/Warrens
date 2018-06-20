using NetMud.DataStructure.Base.System;
using System;
using System.IO;
using System.Web.Hosting;

namespace NetMud.DataAccess.FileSystem
{
    public class ConfigData : FileAccessor
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

        public IConfigData ReadEntity(FileInfo file, Type entityType)
        {
            var fileData = ReadFile(file);
            var blankEntity = Activator.CreateInstance(entityType) as IConfigData;

            return blankEntity.FromBytes(fileData) as IConfigData;
        }

        /// <summary>
        /// Write one backing data entity out
        /// </summary>
        /// <param name="entity">the thing to write out to current</param>
        public void WriteEntity(IConfigData entity)
        {
            try
            {
                var dirName = GetCurrentDirectoryForEntity(entity);

                if (!VerifyDirectory(dirName))
                    throw new Exception("Unable to locate or create base backing data directory.");

                var entityFileName = GetEntityFilename(entity);

                if (string.IsNullOrWhiteSpace(entityFileName))
                    return;

                var fullFileName = dirName + entityFileName;

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
        public void ArchiveEntity(IConfigData entity)
        {
            var dirName = GetCurrentDirectoryForEntity(entity);

            if (!VerifyDirectory(dirName))
                throw new Exception("Unable to locate or create base live data directory.");

            var entityFileName = GetEntityFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dirName + entityFileName;
            var archiveFileDirectory = GetArchiveDirectoryForEntity(entity);

            try
            {
                RollingArchiveFile(fullFileName, archiveFileDirectory + entityFileName, archiveFileDirectory);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        public string GetCurrentDirectoryForEntity(IConfigData entity)
        {
            var dirName = BaseDirectory;

            switch(entity.Type)
            {
                case ConfigDataType.GameWorld:
                    dirName += "WorldConfig/" + CurrentDirectoryName;
                    break;
                case ConfigDataType.Player:
                    dirName += "Players/" + entity.Name + "/" + CurrentDirectoryName;
                    break;
            }


            return dirName;
        }

        private string GetArchiveDirectoryForEntity(IConfigData entity)
        {
            var dirName = BaseDirectory;

            switch (entity.Type)
            {
                case ConfigDataType.GameWorld:
                    dirName += "WorldConfig/" + DatedBackupDirectory;
                    break;
                case ConfigDataType.Player:
                    dirName += BaseDirectory + "Players/" + entity.Name + "/" + DatedBackupDirectory;
                    break;
            }

            return dirName;
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
        /// Gets the statically formatted filename for an entity
        /// </summary>
        /// <param name="entity">The entity in question</param>
        /// <returns>the filename</returns>
        public string GetEntityFilename(IConfigData entity)
        {
            return string.Format("{0}.{1}", entity.Name, entity.GetType().Name);
        }
    }
}
