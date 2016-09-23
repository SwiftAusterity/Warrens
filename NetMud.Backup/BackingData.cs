using NetMud.DataStructure.Base.System;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess;
using NetMud.Data.EntityBackingData;
using NetMud.Data.Game;

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
            var fileAccessor = new NetMud.DataAccess.FileSystem.BackingData();

            try
            {
                LoggingUtility.Log("World BackingData backup to current INITIATED.", LogChannels.Backup, true);

                fileAccessor.ArchiveFull();

                //Get all the entities (which should be a ton of stuff)
                var entities = BackingDataCache.GetAll();

                foreach (var entity in entities)
                    fileAccessor.WriteEntity(entity as IData);

                LoggingUtility.Log("Entire backing data set written to current.", LogChannels.Backup, true);
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
            var implimentedTypes = typeof(EntityBackingDataPartial).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IData))
                                                                                && ty.IsClass
                                                                                && !ty.IsAbstract
                                                                                && ty != typeof(Character));

            foreach (var t in implimentedTypes)
                LoadAllToCache(t);

            return true;
        }

        /// <summary>
        /// Loads all the backing data in the current directories to the cache
        /// </summary>
        /// <returns>full or partial success</returns>
        public static bool LoadEverythingToCacheFromDatabase()
        {
            var implimentedTypes = typeof(EntityBackingDataPartial).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IData))
                                                                                && ty.IsClass
                                                                                && !ty.IsAbstract
                                                                                && ty != typeof(Character));

            foreach (var t in implimentedTypes)
                LoadAllToCacheFromDatabase(t);

            return true;
        }

        /// <summary>
        /// Dumps everything of a single type into the cache from the filesystem for BackingData
        /// </summary>
        /// <typeparam name="T">the type to get and store</typeparam>
        /// <returns>full or partial success</returns>
        public static bool LoadAllToCache(Type objectType)
        {
            if (!objectType.GetInterfaces().Contains(typeof(IData)))
                return false;

            var fileAccessor = new NetMud.DataAccess.FileSystem.BackingData();
            var typeDirectory = fileAccessor.BaseDirectory + fileAccessor.CurrentDirectoryName + objectType.Name + "/";

            if (!fileAccessor.VerifyDirectory(typeDirectory, false))
            {
                LoggingUtility.LogError(new AccessViolationException(String.Format("Current directory for type {0} does not exist.", objectType.Name)));
                return false;
            }

            var filesDirectory = new DirectoryInfo(typeDirectory);

            foreach (var file in filesDirectory.EnumerateFiles())
            {
                try
                {
                    BackingDataCache.Add(fileAccessor.ReadEntity(file, objectType));
                }
                catch(Exception ex)
                {
                    LoggingUtility.LogError(ex);
                    //Let it keep going
                }
            }

            return true;
        }

        /// <summary>
        /// Dumps everything of a single type into the cache from the database for BackingData
        /// </summary>
        /// <typeparam name="T">the type to get and store</typeparam>
        /// <returns>full or partial success</returns>
        public static bool LoadAllToCacheFromDatabase(Type objectType)
        {
            foreach (IData thing in NetMud.DataAccess.DataWrapper.GetAll(objectType))
            {
                try
                {
                    BackingDataCache.Add(thing, new BackingDataCacheKey(objectType, thing.ID).KeyHash());
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
