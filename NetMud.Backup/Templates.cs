using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Gaia;
using NetMud.Data.Locale;
using NetMud.Data.Room;
using NetMud.Data.Zone;
using NetMud.DataAccess;
using NetMud.DataAccess.FileSystem;
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
    public static class Templates
    {
        /// <summary>
        /// Writes everything in the cache back to the file system
        /// </summary>
        /// <returns>full or partial success</returns>
        public static bool WriteFullBackup()
        {
            return WriteFullBackup("");
        }

        /// <summary>
        /// Writes everything in the cache back to the file system
        /// </summary>
        /// <returns>full or partial success</returns>
        public static bool WriteFullBackup(string backupName)
        {
            TemplateData fileAccessor = new TemplateData();

            try
            {
                LoggingUtility.Log("World BackingData backup to current INITIATED.", LogChannels.Backup, true);

                fileAccessor.ArchiveFull(backupName);

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
            System.Collections.Generic.IEnumerable<Type> implimentedTypes = typeof(EntityTemplatePartial).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IKeyedData))
                                                                                && ty.IsClass
                                                                                && !ty.IsAbstract
                                                                                && !ty.GetCustomAttributes<IgnoreAutomatedBackupAttribute>().Any());

            foreach (Type t in implimentedTypes.OrderByDescending(type => type == typeof(GaiaTemplate) ? 6 :
                                                                            type == typeof(ZoneTemplate) ? 5 :
                                                                            type == typeof(LocaleTemplate) ? 3 :
                                                                            type == typeof(RoomTemplate) ? 3 :
                                                                            type == typeof(PathwayTemplate) ? 2 :
                                                                            type.GetInterfaces().Contains(typeof(ILookupData)) ? 1 : 0))
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
            if (!objectType.GetInterfaces().Contains(typeof(IKeyedData)))
            {
                return false;
            }

            TemplateData fileAccessor = new TemplateData();
            string typeDirectory = fileAccessor.BaseDirectory + fileAccessor.CurrentDirectoryName + objectType.Name + "/";

            if (!fileAccessor.VerifyDirectory(typeDirectory, false))
            {
                return false;
            }

            DirectoryInfo filesDirectory = new DirectoryInfo(typeDirectory);

            foreach (FileInfo file in filesDirectory.EnumerateFiles())
            {
                try
                {
                    IKeyedData entity = fileAccessor.ReadEntity(file, objectType);

                    if (entity != null)
                    {
                        entity.PersistToCache();
                    }
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
