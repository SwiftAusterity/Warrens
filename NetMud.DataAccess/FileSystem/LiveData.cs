using NetMud.DataStructure.Base.System;
using System;
using System.IO;
using System.Web.Hosting;

namespace NetMud.DataAccess.FileSystem
{
    public class LiveData : FileAccessor
    {
        /// <summary>
        /// Root directory where all the backup stuff gets saved too
        /// </summary>
        public override string BaseDirectory
        {
            get
            {
                return HostingEnvironment.MapPath(base.BaseDirectory + "LiveData/");
            }
        }

        public IEntity ReadEntity(FileInfo file, Type entityType)
        {
            var fileData = ReadFile(file);
            var blankEntity = Activator.CreateInstance(entityType) as IEntity;

            return blankEntity.FromBytes(fileData) as IEntity;
        }

        public void WriteEntity(IEntity entity)
        {
            var baseTypeName = entity.GetType().Name;
            var dirName = BaseDirectory + CurrentDirectoryName + baseTypeName;

            if (!VerifyDirectory(dirName))
                throw new Exception("Unable to locate or create base live data directory.");

            WriteSpecificEntity(new DirectoryInfo(dirName), entity);
        }

        /// <summary>
        /// Writes one entity to Current backup (not players)
        /// </summary>
        /// <param name="dir">Root directory to write to</param>
        /// <param name="entity">The entity to write out</param>
        public void WriteSpecificEntity(DirectoryInfo dir, IEntity entity)
        {
            var entityFileName = GetEntityFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dir.FullName + "/" + entityFileName;

            try
            {
                WriteToFile(fullFileName, entity.ToBytes());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        public void ArchiveFull()
        {
            //wth, no current directory? Noithing to move then
            if (VerifyDirectory(CurrentDirectoryName, false) && VerifyDirectory(ArchiveDirectoryName))
            {
                var currentRoot = new DirectoryInfo(BaseDirectory + CurrentDirectoryName);

                //move is literal move, no need to delete afterwards
                currentRoot.MoveTo(DatedBackupDirectory);
            }

            //something very wrong is happening, it'll get logged
            if (!VerifyDirectory(CurrentDirectoryName))
                throw new Exception("Can not locate or verify current live data directory.");
        }

        /// <summary>
        /// Gets the statically formatted filename for an entity
        /// </summary>
        /// <param name="entity">The entity in question</param>
        /// <returns>the filename</returns>
        private string GetEntityFilename(IEntity entity)
        {
            return string.Format("{0}.{1}", entity.BirthMark, entity.GetType().Name);
        }
    }
}
