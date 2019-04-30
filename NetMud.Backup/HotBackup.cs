using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NetMud.Backup
{
    /// <summary>
    /// The engine behind the system that constantly writes out live data so we can reboot into the prior state if needs be
    /// BaseDirectory should end with a trailing slash
    /// </summary>
    public class HotBackup
    {
        /// <summary>
        /// Something went wrong with restoring the live backup, this loads all persistence singeltons from the database (rooms, paths, spawns)
        /// </summary>
        /// <returns>success state</returns>
        public bool NewWorldFallback()
        {
            LiveData liveDataAccessor = new LiveData();

            //This means we delete the entire Current livedata dir since we're falling back.
            string currentLiveDirectory = liveDataAccessor.BaseDirectory + liveDataAccessor.CurrentDirectoryName;

            //No backup directory? No live data.
            if (Directory.Exists(currentLiveDirectory))
            {
                DirectoryInfo currentDir = new DirectoryInfo(currentLiveDirectory);

                LoggingUtility.Log("Current Live directory deleted during New World Fallback Procedures.", LogChannels.Backup, true);

                try
                {
                    currentDir.Delete(true);
                }
                catch
                {
                    //occasionally will be pissy in an async situation
                }
            }

            LoggingUtility.Log("World restored from data fallback.", LogChannels.Backup, true);

            return true;
        }

        /// <summary>
        /// Dumps everything of a single type into the cache from the database for BackingData
        /// </summary>
        /// <typeparam name="T">the type to get and store</typeparam>
        /// <returns>success status</returns>
        public bool PreLoadAll<T>() where T : IKeyedData
        {
            ITemplate backingClass = Activator.CreateInstance(typeof(T)) as ITemplate;

            Type implimentingEntityClass = backingClass.EntityClass;

            foreach (IKeyedData thing in TemplateCache.GetAll<T>())
            {
                try
                {
                    IEntity entityThing = Activator.CreateInstance(implimentingEntityClass, new object[] { (T)thing }) as IEntity;

                    ((ISpawnAsSingleton<T>)entityThing).GetFromWorldOrSpawn();
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return true;
        }

        /// <summary>
        /// Writes the current live world content (entities, positions, etc) to the Current backup; archives whatever was already considered current
        /// </summary>
        /// <returns>Success state</returns>
        public bool WriteLiveBackup()
        {
            return WriteLiveBackup("");
        }

        /// <summary>
        /// Writes the current live world content (entities, positions, etc) to the Current backup; archives whatever was already considered current
        /// </summary>
        /// <returns>Success state</returns>
        public bool WriteLiveBackup(string backupName)
        {
            LiveData liveDataAccessor = new LiveData();

            try
            {
                LoggingUtility.Log("World backup to current INITIATED.", LogChannels.Backup, true);

                liveDataAccessor.ArchiveFull(backupName);

                LoggingUtility.Log("Current live world written to archive.", LogChannels.Backup, true);

                return WritePlayers();
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return false;
        }

        /// <summary>
        /// Players are written to their own private directories, with the full current/dated backup cycle for each dude
        /// </summary>
        /// <returns>whether or not it succeeded</returns>
        public bool WritePlayers()
        {
            PlayerData playerAccessor = new PlayerData();
            try
            {
                LoggingUtility.Log("All Players backup INITIATED.", LogChannels.Backup, true);

                //Get all the players
                IEnumerable<IPlayer> entities = LiveCache.GetAll<IPlayer>();

                foreach (IPlayer entity in entities)
                {
                    playerAccessor.WriteOnePlayer(entity);
                }

                LoggingUtility.Log("All players written.", LogChannels.Backup, true);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Restores live entity backup from Current
        /// </summary>
        /// <returns>Success state</returns>
        public bool RestoreLiveBackup()
        {
            LiveData liveDataAccessor = new LiveData();

            string currentBackupDirectory = liveDataAccessor.BaseDirectory + liveDataAccessor.CurrentDirectoryName;

            //No backup directory? No live data.
            if (!Directory.Exists(currentBackupDirectory))
            {
                return false;
            }

            LoggingUtility.Log("World restored from current live INITIATED.", LogChannels.Backup, false);

            try
            {
                //dont load players here
                List<IEntity> entitiesToLoad = new List<IEntity>();
                IEnumerable<Type> implimentedTypes = typeof(EntityPartial).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IEntity))
                                                                                                && ty.IsClass
                                                                                                && !ty.IsAbstract
                                                                                                && !ty.GetCustomAttributes<IgnoreAutomatedBackupAttribute>().Any());

                //Shove them all into the live system first
                foreach (IEntity entity in entitiesToLoad.OrderBy(ent => ent.Birthdate))
                {
                    entity.UpsertToLiveWorldCache();
                    entity.KickoffProcesses();
                }

                LoggingUtility.Log("World restored from current live.", LogChannels.Backup, false);
                return true;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return false;
        }
    }
}
