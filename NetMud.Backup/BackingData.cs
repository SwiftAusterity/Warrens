using NetMud.Data.EntityBackingData;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NetMud.Backup
{
    /// <summary>
    /// Responsible for retrieving backingdata and putting it into the cache
    /// </summary>
    public static class BackingData
    {
        /// <summary>
        /// Writes everything in the cache back to the file system
        /// </summary>
        /// <returns>full or partial success</returns>
        public static bool WriteFullBackup()
        {
            var fileAccessor = new DataAccess.FileSystem.BackingData();

            try
            {
                LoggingUtility.Log("World BackingData backup to current INITIATED.", LogChannels.Backup, true);

                fileAccessor.ArchiveFull();

                LoggingUtility.Log("Entire backing data set archived.", LogChannels.Backup, true);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads all the backing data in the current directories to the cache
        /// </summary>
        /// <returns>full or partial success</returns>
        public static bool LoadEverythingToCache()
        {
            var implimentedTypes = typeof(EntityBackingDataPartial).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IKeyedData))
                                                                                && ty.IsClass
                                                                                && !ty.IsAbstract
                                                                                && !ty.GetCustomAttributes<IgnoreAutomatedBackupAttribute>().Any());

            foreach (var t in implimentedTypes.OrderByDescending(type => type == typeof(ZoneData) ? 5 :
                                                                            type == typeof(LocaleData) ? 4 :
                                                                            type == typeof(RoomData) ? 3 :
                                                                            type == typeof(PathwayData) ? 2 :
                                                                            type.GetInterfaces().Contains(typeof(ILookupData)) ? 1 : 0))
                LoadAllToCache(t);

            return true;
        }

        /// <summary>
        /// Dumps everything of a single type into the cache from the filesystem for BackingData
        /// </summary>
        /// <typeparam name="T">the type to get and store</typeparam>
        /// <returns>full or partial success</returns>
        public static bool LoadAllToCache(Type objectType)
        {
            if (!objectType.GetInterfaces().Contains(typeof(IKeyedData)))
                return false;

            var fileAccessor = new DataAccess.FileSystem.BackingData();
            var typeDirectory = fileAccessor.BaseDirectory + fileAccessor.CurrentDirectoryName + objectType.Name + "/";

            if (!fileAccessor.VerifyDirectory(typeDirectory, false))
            {
                return false;
            }

            var filesDirectory = new DirectoryInfo(typeDirectory);

            foreach (var file in filesDirectory.EnumerateFiles())
            {
                try
                {
                    var entity = fileAccessor.ReadEntity(file, objectType);

                    if (entity != null)
                        entity.PersistToCache();
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                    //Let it keep going
                }
            }

            return true;
        }
    }
}
