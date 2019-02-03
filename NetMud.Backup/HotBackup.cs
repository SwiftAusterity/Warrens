using NetMud.Cartography;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Gaia;
using NetMud.Data.Inanimate;
using NetMud.Data.NPC;
using NetMud.Data.Room;
using NetMud.Data.Zone;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.Zone;
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
                currentDir.Delete(true);
            }

            //Only load in stuff that is static and spawns as singleton
            //We need to pick up any places that aren't already live from the file system incase someone added them during the last session\
            foreach (IGaiaTemplate thing in TemplateCache.GetAll<IGaiaTemplate>())
            {
                IGaia entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IGaia;
            }

            foreach (IZoneTemplate thing in TemplateCache.GetAll<IZoneTemplate>())
            {
                IZone entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IZone;
            }

            foreach (ILocaleTemplate thing in TemplateCache.GetAll<ILocaleTemplate>())
            {
                ILocale entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as ILocale;

                entityThing.ParentLocation = entityThing.Template<ILocaleTemplate>().ParentLocation.GetLiveInstance();
                entityThing.GetFromWorldOrSpawn();
            }

            foreach (IRoomTemplate thing in TemplateCache.GetAll<IRoomTemplate>())
            {
                IRoom entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IRoom;

                entityThing.ParentLocation = entityThing.Template<IRoomTemplate>().ParentLocation.GetLiveInstance();
                entityThing.GetFromWorldOrSpawn();
            }

            foreach (IPathwayTemplate thing in TemplateCache.GetAll<IPathwayTemplate>())
            {
                IPathway entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IPathway;
            }

            ParseDimension();

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

                    if(typeof(T).GetInterfaces().Contains(typeof(ISpawnAsMultiple)))
                    {
                        entityThing.SpawnNewInWorld();
                    }
                    else
                    {
                        ((ISpawnAsSingleton<T>)entityThing).GetFromWorldOrSpawn();
                    }
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
            LiveData liveDataAccessor = new LiveData();

            try
            {
                LoggingUtility.Log("World backup to current INITIATED.", LogChannels.Backup, true);

                liveDataAccessor.ArchiveFull();

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

                foreach (Type type in implimentedTypes)
                {
                    if (!Directory.Exists(currentBackupDirectory + type.Name))
                    {
                        continue;
                    }

                    DirectoryInfo entityFilesDirectory = new DirectoryInfo(currentBackupDirectory + type.Name);

                    foreach (FileInfo file in entityFilesDirectory.EnumerateFiles())
                    {
                        entitiesToLoad.Add(liveDataAccessor.ReadEntity(file, type));
                    }
                }

                //Shove them all into the live system first
                foreach (IEntity entity in entitiesToLoad.OrderBy(ent => ent.Birthdate))
                {
                    entity.UpsertToLiveWorldCache();
                }

                //Check we found actual data
                if (!entitiesToLoad.Any(ent => ent.GetType() == typeof(Gaia)))
                {
                    throw new Exception("No Worlds found, failover.");
                }

                if (!entitiesToLoad.Any(ent => ent.GetType() == typeof(Zone)))
                {
                    throw new Exception("No zones found, failover.");
                }

                //We need to pick up any places that aren't already live from the file system incase someone added them during the last session\
                foreach (IGaiaTemplate thing in TemplateCache.GetAll<IGaiaTemplate>().Where(dt => !entitiesToLoad.Any(ent => ent.TemplateId.Equals(dt.Id))))
                {
                    IGaia entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IGaia;

                    entityThing.GetFromWorldOrSpawn();
                }

                foreach (IZoneTemplate thing in TemplateCache.GetAll<IZoneTemplate>().Where(dt => !entitiesToLoad.Any(ent => ent.TemplateId.Equals(dt.Id))))
                {
                    IZone entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IZone;

                    entityThing.GetFromWorldOrSpawn();
                }

                foreach (ILocaleTemplate thing in TemplateCache.GetAll<ILocaleTemplate>().Where(dt => !entitiesToLoad.Any(ent => ent.TemplateId.Equals(dt.Id))))
                {
                    ILocale entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as ILocale;

                    entityThing.ParentLocation = entityThing.ParentLocation.GetLiveInstance();
                    entityThing.UpsertToLiveWorldCache();
                }

                foreach (IRoomTemplate thing in TemplateCache.GetAll<IRoomTemplate>().Where(dt => !entitiesToLoad.Any(ent => ent.TemplateId.Equals(dt.Id))))
                {
                    IRoom entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IRoom;

                    entityThing.ParentLocation = entityThing.Template<IRoomTemplate>().ParentLocation.GetLiveInstance();
                    entityThing.GetFromWorldOrSpawn();
                }

                foreach (IPathwayTemplate thing in TemplateCache.GetAll<IPathwayTemplate>().Where(dt => !entitiesToLoad.Any(ent => ent.TemplateId.Equals(dt.Id))))
                {
                    IPathway entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IPathway;

                    entityThing.GetFromWorldOrSpawn();
                }

                //We have the containers contents and the birthmarks from the deserial
                //I don't know how we can even begin to do this type agnostically since the collections are held on type specific objects without some super ugly reflection
                foreach (Room entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Room)))
                {
                    foreach (IInanimate obj in entity.Contents.EntitiesContained())
                    {
                        IInanimate fullObj = LiveCache.Get<IInanimate>(new LiveCacheKey(obj));
                        entity.MoveFrom(obj);
                        entity.MoveInto(fullObj);
                    }

                    foreach (INonPlayerCharacter obj in entity.MobilesInside.EntitiesContained())
                    {
                        INonPlayerCharacter fullObj = LiveCache.Get<INonPlayerCharacter>(new LiveCacheKey(obj));
                        entity.MoveFrom(obj);
                        entity.MoveInto(fullObj);
                    }
                }

                foreach (NonPlayerCharacter entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(NonPlayerCharacter)))
                {
                    foreach (IInanimate obj in entity.Inventory.EntitiesContained())
                    {
                        IInanimate fullObj = LiveCache.Get<IInanimate>(new LiveCacheKey(obj));
                        entity.MoveFrom(obj);
                        entity.MoveInto(fullObj);
                    }
                }

                foreach (Inanimate entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Inanimate)))
                {
                    foreach (Tuple<string, IInanimate> obj in entity.Contents.EntitiesContainedByName())
                    {
                        IInanimate fullObj = LiveCache.Get<IInanimate>(new LiveCacheKey(obj.Item2));
                        entity.MoveFrom(obj.Item2);
                        entity.MoveInto(fullObj, obj.Item1);
                    }

                    foreach (Tuple<string, IInanimate> obj in entity.Contents.EntitiesContainedByName())
                    {
                        INonPlayerCharacter fullObj = LiveCache.Get<INonPlayerCharacter>(new LiveCacheKey(obj.Item2));
                        entity.MoveFrom((INonPlayerCharacter)obj.Item2);
                        entity.MoveInto(fullObj, obj.Item1);
                    }
                }

                //We need to poll the WorldMaps here and give all the rooms their coordinates as well as the zones their sub-maps
                ParseDimension();

                LoggingUtility.Log("World restored from current live.", LogChannels.Backup, false);
                return true;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return false;
        }

        private void ParseDimension()
        {
            HashSet<ILocaleTemplate> localePool = new HashSet<ILocaleTemplate>(TemplateCache.GetAll<ILocaleTemplate>());

            foreach (ILocaleTemplate locale in localePool)
            {
                locale.RemapInterior();
            }
        }
    }
}
