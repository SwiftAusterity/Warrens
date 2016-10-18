using NetMud.Data.EntityBackingData;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NetMud.DataStructure.Base.Place;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.SupportingClasses;

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
            //Only load in stuff that is static and spawns as singleton
            PreLoadAll<RoomData>();
            PreLoadAll<PathwayData>();

            //TODO: Need to new up all the dimensional maps here

            LoggingUtility.Log("World restored from data fallback.", LogChannels.Backup, true);

            return true;
        }

        /// <summary>
        /// Dumps everything of a single type into the cache from the database for BackingData
        /// </summary>
        /// <typeparam name="T">the type to get and store</typeparam>
        /// <returns>success status</returns>
        public bool PreLoadAll<T>() where T : IData
        {
            var backingClass = Activator.CreateInstance(typeof(T)) as IEntityBackingData;

            var implimentingEntityClass = backingClass.EntityClass;

            foreach (IData thing in BackingDataCache.GetAll<T>())
            {
                var entityThing = Activator.CreateInstance(implimentingEntityClass, new object[] { (T)thing }) as IEntity;

                LiveCache.Add(entityThing);
            }

            return true;
        }


        /// <summary>
        /// Writes the current live world content (entities, positions, etc) to the Current backup; archives whatever was already considered current
        /// </summary>
        /// <returns>Success state</returns>
        public bool WriteLiveBackup()
        {
            var liveDataAccessor = new LiveData();

            try
            {
                LoggingUtility.Log("World backup to current INITIATED.", LogChannels.Backup, true);

                liveDataAccessor.ArchiveFull();

                //Get all the entities (which should be a ton of stuff)
                var entities = LiveCache.GetAll();

                //Dont save players to the hot section, there's another place for them
                foreach (var entity in entities.Where(ent => !ent.GetType().GetCustomAttributes<IgnoreAutomatedBackupAttribute>().Any()))
                {
                    var liveEntity = entity as IEntity;

                    //Don't write objects that are on live players, player backup does that itself
                    if (liveEntity.CurrentLocation != null && liveEntity.CurrentLocation.GetType() == typeof(Player))
                        continue;

                    liveDataAccessor.WriteEntity(liveEntity);
                }

                LoggingUtility.Log("Live world written to current.", LogChannels.Backup, true);

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
            var playerAccessor = new PlayerData();
            try
            {
                LoggingUtility.Log("All Players backup INITIATED.", LogChannels.Backup, true);

                //Get all the players
                var entities = LiveCache.GetAll<Player>();

                foreach (var entity in entities)
                    playerAccessor.WriteOnePlayer(entity);

                LoggingUtility.Log("All players written.", LogChannels.Backup, true);
            }
            catch(Exception ex)
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
            var liveDataAccessor = new LiveData();

            var currentBackupDirectory = liveDataAccessor.BaseDirectory + liveDataAccessor.CurrentDirectoryName;

            //No backup directory? No live data.
            if (!Directory.Exists(currentBackupDirectory))
                return false;

            LoggingUtility.Log("World restored from current live INITIATED.", LogChannels.Backup, false);

            try
            {
                //dont load players here
                var entitiesToLoad = new List<IEntity>();
                var implimentedTypes = typeof(EntityPartial).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IEntity)) 
                                                                                                && ty.IsClass 
                                                                                                && !ty.IsAbstract 
                                                                                                && !ty.GetCustomAttributes<IgnoreAutomatedBackupAttribute>().Any());

                foreach (var type in implimentedTypes)
                {
                    if (!Directory.Exists(currentBackupDirectory + type.Name))
                        continue;

                    var entityFilesDirectory = new DirectoryInfo(currentBackupDirectory + type.Name);

                    foreach (var file in entityFilesDirectory.EnumerateFiles())
                        entitiesToLoad.Add(liveDataAccessor.ReadEntity(file, type));
                }

                //Shove them all into the live system first
                foreach (var entity in entitiesToLoad.OrderBy(ent => ent.Birthdate))
                    entity.UpsertToLiveWorldCache();

                //Check we found actual data
                if (!entitiesToLoad.Any(ent => ent.GetType() == typeof(Room) || ent.GetType() == typeof(Pathway)))
                    throw new Exception("No rooms or pathways found, failover.");

                //We have the containers contents and the birthmarks from the deserial
                //I don't know how we can even begin to do this type agnostically since the collections are held on type specific objects without some super ugly reflection
                foreach (Room entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Room)))
                {
                    foreach (IInanimate obj in entity.ObjectsInRoom.EntitiesContained())
                    {
                        var fullObj = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(Inanimate), obj.BirthMark));
                        entity.MoveFrom<IInanimate>(obj);
                        entity.MoveInto<IInanimate>(fullObj);
                    }

                    foreach (IIntelligence obj in entity.MobilesInside.EntitiesContained())
                    {
                        var fullObj = LiveCache.Get<IIntelligence>(new LiveCacheKey(typeof(Intelligence), obj.BirthMark));
                        entity.MoveFrom<IIntelligence>(obj);
                        entity.MoveInto<IIntelligence>(fullObj);
                    }
                }

                foreach (Intelligence entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Intelligence)))
                {
                    foreach (IInanimate obj in entity.Inventory.EntitiesContained())
                    {
                        var fullObj = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(Inanimate), obj.BirthMark));
                        entity.MoveFrom<IInanimate>(obj);
                        entity.MoveInto<IInanimate>(fullObj);
                    }
                }

                foreach (Inanimate entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Inanimate)))
                {
                    foreach (var obj in entity.Contents.EntitiesContainedByName())
                    {
                        var fullObj = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(Inanimate), obj.Item2.BirthMark));
                        entity.MoveFrom<IInanimate>(obj.Item2);
                        entity.MoveInto<IInanimate>(fullObj, obj.Item1);
                    }
             
                    foreach (var obj in entity.MobilesInside.EntitiesContainedByName())
                    {
                        var fullObj = LiveCache.Get<IIntelligence>(new LiveCacheKey(typeof(Intelligence), obj.Item2.BirthMark));
                        entity.MoveFrom<IIntelligence>((IIntelligence)obj.Item2);
                        entity.MoveInto<IIntelligence>(fullObj, obj.Item1);
                    }
                }

                //paths load themselves to their room
                foreach (Pathway entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Pathway)))
                {
                    ILocation roomTo = LiveCache.Get<ILocation>(new LiveCacheKey(entity.ToLocation.GetType(), entity.ToLocation.BirthMark));
                    ILocation roomFrom = LiveCache.Get<ILocation>(new LiveCacheKey(entity.FromLocation.GetType(), entity.FromLocation.BirthMark));

                    if (roomTo != null && roomFrom != null)
                    {
                        entity.ToLocation = roomTo;
                        entity.FromLocation = roomFrom;
                        entity.CurrentLocation = roomFrom;
                        roomFrom.MoveInto<IPathway>(entity);
                    }
                }

                //TODO: We need to poll the WorldMaps here and give all the rooms their coordinates as well as the zones their sub-maps

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
