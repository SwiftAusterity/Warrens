using NetMud.Data.Linguistic;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
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
            DataAccess.FileSystem.TemplateData fileAccessor = new DataAccess.FileSystem.TemplateData();

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
            System.Collections.Generic.IEnumerable<Type> implimentedTypes = typeof(Data.Architectural.ConfigData).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IConfigData))
                                                                                && ty.IsClass
                                                                                && !ty.IsAbstract
                                                                                && !ty.GetCustomAttributes<IgnoreAutomatedBackupAttribute>().Any());

            foreach (Type t in implimentedTypes.OrderByDescending(type => type == typeof(Dictata) ? 5 : 0))
            {
                LoadAllToCache(t);
            }

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
            {
                return false;
            }

            DataAccess.FileSystem.ConfigData fileAccessor = new DataAccess.FileSystem.ConfigData();
            string typeDirectory = fileAccessor.GetCurrentDirectoryForType(objectType);

            if (!fileAccessor.VerifyDirectory(typeDirectory, false))
            {
                return false;
            }

            DirectoryInfo filesDirectory = new DirectoryInfo(typeDirectory);

            foreach (FileInfo file in filesDirectory.EnumerateFiles())
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
