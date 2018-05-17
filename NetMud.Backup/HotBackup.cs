using NetMud.Data.EntityBackingData;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NetMud.DataStructure.Base.Place;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.Cartography;

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
            PreLoadAll<ZoneData>();
            PreLoadAll<LocaleData>();
            PreLoadAll<RoomData>();
            PreLoadAll<PathwayData>();

            ParseDimension();

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

                entityThing.UpsertToLiveWorldCache();
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
                    if (liveEntity.CurrentLocation?.CurrentLocation != null && liveEntity.CurrentLocation.CurrentLocation.GetType() == typeof(Player))
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
                if (!entitiesToLoad.Any(ent => ent.GetType() == typeof(Zone)))
                    throw new Exception("No zones found, failover.");

                //We need to pick up any places that aren't already live from the file system incase someone added them during the last session\
                foreach (var thing in BackingDataCache.GetAll<IZoneData>().Where(dt => !entitiesToLoad.Any(ent => ent.DataTemplateId.Equals(dt.ID))))
                {
                    var entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IZone;

                    entityThing.UpsertToLiveWorldCache();
                    entityThing.SpawnNewInWorld();
                }

                foreach (var thing in BackingDataCache.GetAll<ILocaleData>().Where(dt => !entitiesToLoad.Any(ent => ent.DataTemplateId.Equals(dt.ID))))
                {
                    var entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as ILocale;

                    entityThing.UpsertToLiveWorldCache();
                    entityThing.SpawnNewInWorld();
                }

                foreach (var thing in BackingDataCache.GetAll<IRoomData>().Where(dt => !entitiesToLoad.Any(ent => ent.DataTemplateId.Equals(dt.ID))))
                {
                    var entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IRoom;

                    entityThing.UpsertToLiveWorldCache();
                    entityThing.SpawnNewInWorld();
                }

                foreach (var thing in BackingDataCache.GetAll<IPathwayData>().Where(dt => !entitiesToLoad.Any(ent => ent.DataTemplateId.Equals(dt.ID))))
                {
                    var entityThing = Activator.CreateInstance(thing.EntityClass, new object[] { thing }) as IPathway;

                    entityThing.UpsertToLiveWorldCache();
                }

                //We have the containers contents and the birthmarks from the deserial
                //I don't know how we can even begin to do this type agnostically since the collections are held on type specific objects without some super ugly reflection
                foreach(Zone entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Zone)))
                {
                    if(entity.CurrentLocation == null)
                        entity.SpawnNewInWorld();

                    foreach (ILocale obj in entity.Contents.EntitiesContained())
                    {
                        var fullObj = LiveCache.Get<ILocale>(new LiveCacheKey(typeof(Locale), obj.BirthMark));
                        entity.MoveFrom(obj);
                        entity.MoveInto(fullObj);
                    }
                }

                foreach (Room entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Room)))
                {
                    if (entity.CurrentLocation == null)
                        entity.SpawnNewInWorld();

                    foreach (IInanimate obj in entity.Contents.EntitiesContained())
                    {
                        var fullObj = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(Inanimate), obj.BirthMark));
                        entity.MoveFrom(obj);
                        entity.MoveInto(fullObj);
                    }

                    foreach (IIntelligence obj in entity.MobilesInside.EntitiesContained())
                    {
                        var fullObj = LiveCache.Get<IIntelligence>(new LiveCacheKey(typeof(Intelligence), obj.BirthMark));
                        entity.MoveFrom(obj);
                        entity.MoveInto(fullObj);
                    }
                }

                foreach (Intelligence entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Intelligence)))
                {
                    foreach (IInanimate obj in entity.Inventory.EntitiesContained())
                    {
                        var fullObj = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(Inanimate), obj.BirthMark));
                        entity.MoveFrom(obj);
                        entity.MoveInto(fullObj);
                    }
                }

                foreach (Inanimate entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Inanimate)))
                {
                    foreach (var obj in entity.Contents.EntitiesContainedByName())
                    {
                        var fullObj = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(Inanimate), obj.Item2.BirthMark));
                        entity.MoveFrom(obj.Item2);
                        entity.MoveInto(fullObj, obj.Item1);
                    }

                    foreach (var obj in entity.MobilesInside.EntitiesContainedByName())
                    {
                        var fullObj = LiveCache.Get<IIntelligence>(new LiveCacheKey(typeof(Intelligence), obj.Item2.BirthMark));
                        entity.MoveFrom((IIntelligence)obj.Item2);
                        entity.MoveInto(fullObj, obj.Item1);
                    }
                }

                //paths load themselves to their appropriate location
                foreach (Pathway entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Pathway)))
                {
                    var roomTo = LiveCache.Get<IRoom>(new LiveCacheKey(typeof(IRoom), entity.ToLocation.BirthMark));
                    var roomFrom = LiveCache.Get<IRoom>(new LiveCacheKey(typeof(IRoom), entity.FromLocation.BirthMark));

                    if (roomTo != null && roomFrom != null)
                    {
                        entity.ToLocation = roomTo;
                        entity.FromLocation = roomFrom;
                        entity.CurrentLocation = roomFrom.CurrentLocation;
                        roomFrom.MoveInto<IPathway>(entity);
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
            var zonePool = new HashSet<IZoneData>(BackingDataCache.GetAll<IZoneData>());
            var localePool = new HashSet<ILocaleData>(BackingDataCache.GetAll<ILocaleData>());
            var roomPool = new HashSet<IRoomData>(BackingDataCache.GetAll<IRoomData>());

            //This will cycle through every room building massive (in theory) maps and spitting out the remaining items to make more worlds from.
            //If your world is highly disconnected you will end up with a ton of world maps
            while (roomPool.Count() > 0)
            {
                var currentRoom = roomPool.FirstOrDefault();

                if (currentRoom == null)
                    continue;

                GenerateWorld(currentRoom, roomPool);

                //BackingDataCache.Add(newWorld);
            }
        }

        //TODO: a method that takes a room, 
        //Would need to both recenter and shrink before the end otherwise we'll have gigantic arrays
        /// <summary>
        /// Builds the entire connected world out of the starting room 
        /// </summary>
        /// <param name="startingRoom">The room to start with</param>
        /// <param name="remainingRooms">The list of remaining rooms to work against (will remove used rooms from this)</param>
        /// <returns>A whole new world</returns>
        private void GenerateWorld(IRoomData startingRoom, HashSet<IRoomData> remainingRooms)
        {
            if (startingRoom == null || remainingRooms.Count() == 0)
                throw new InvalidOperationException("Invalid inputs.");

            //We're kind of faking array size for radius, it will be shrunk later
            var returnMap = Cartographer.GenerateMapFromRoom(startingRoom, remainingRooms.Count() / 2, remainingRooms, true);

            //This zone gets to choose the world name if any
            //var world = new World(new Map(returnMap, false), startingRoom.ZoneAffiliation.WorldName);

            //if (String.IsNullOrWhiteSpace(world.Name))
            //    world.Name = "Dimension " + world.ID.ToString();
        }
    }
}
