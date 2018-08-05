using NetMud.Data.ConfigData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
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
    public static class ConfigData
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
            var implimentedTypes = typeof(Data.ConfigData.ConfigData).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IConfigData))
                                                                                && ty.IsClass
                                                                                && !ty.IsAbstract
                                                                                && !ty.GetCustomAttributes<IgnoreAutomatedBackupAttribute>().Any());

            foreach (var t in implimentedTypes.OrderByDescending(type => type == typeof(Dictata) ? 5 : 0))
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
            if (!objectType.GetInterfaces().Contains(typeof(IConfigData)))
                return false;

            var fileAccessor = new DataAccess.FileSystem.ConfigData();
            var typeDirectory = fileAccessor.GetCurrentDirectoryForType(objectType);

            if (!fileAccessor.VerifyDirectory(typeDirectory, false))
            {
                return false;
            }

            var filesDirectory = new DirectoryInfo(typeDirectory);

            foreach (var file in filesDirectory.EnumerateFiles())
            {
                try
                {
                     ConfigDataCache.Add(fileAccessor.ReadEntity(file, objectType));
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
