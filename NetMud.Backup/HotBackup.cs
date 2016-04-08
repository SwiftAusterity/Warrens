using NetMud.Data.EntityBackingData;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.EntityBackingData;
using System.Reflection;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.Backup
{
    /// <summary>
    /// The engine behind the system that constantly writes out live data so we can reboot into the prior state if needs be
    /// BaseDirectory should end with a trailing slash
    /// </summary>
    public class HotBackup
    {
        /// <summary>
        /// Root directory where all the backup stuff gets saved too
        /// </summary>
        public string BaseDirectory { get; private set; }

        /// <summary>
        /// Create an instance of the hotbackup utility
        /// </summary>
        /// <param name="baseDirectory">Root directory where all the backup stuff gets saved too</param>
        public HotBackup(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
        }

        /// <summary>
        /// Something went wrong with restoring the live backup, this loads all persistence singeltons from the database (rooms, paths, spawns)
        /// </summary>
        /// <returns>success state</returns>
        public bool NewWorldFallback()
        {
            //Only load in stuff that is static and spawns as singleton
            LiveCache.PreLoadAll<RoomData>();
            LiveCache.PreLoadAll<PathwayData>();

            LoggingUtility.Log("World restored from data fallback.", LogChannels.Backup, true);

            return true;
        }

        /// <summary>
        /// Writes the current live world content (entities, positions, etc) to the Current backup; archives whatever was already considered current
        /// </summary>
        /// <returns>Success state</returns>
        public bool WriteLiveBackup()
        {
            try
            {
                LoggingUtility.Log("World backup to current INITIATED.", LogChannels.Backup, true);

                //Need the base dir
                if (!Directory.Exists(BaseDirectory))
                    Directory.CreateDirectory(BaseDirectory);

                //Wipe out the existing one so we can create all new files
                if (Directory.Exists(BaseDirectory + "Current/"))
                {
                    var currentRoot = new DirectoryInfo(BaseDirectory + "Current/");

                    if (!Directory.Exists(BaseDirectory + "Backups/"))
                        Directory.CreateDirectory(BaseDirectory + "Backups/");

                    var newBackupName = String.Format("{0}Backups/{1}{2}{3}_{4}{5}{6}/",
                                        BaseDirectory
                                        , DateTime.Now.Year
                                        , DateTime.Now.Month
                                        , DateTime.Now.Day
                                        , DateTime.Now.Hour
                                        , DateTime.Now.Minute
                                        , DateTime.Now.Second);

                    //move is literal move, no need to delete afterwards
                    currentRoot.MoveTo(newBackupName);
                }

                var currentBackupDirectory = BaseDirectory + "Current/";
                Directory.CreateDirectory(currentBackupDirectory);

                //Get all the entities (which should be a ton of stuff)
                var entities = LiveCache.GetAll();

                //Dont save players to the hot section, there's another place for them
                foreach (var entity in entities.Where(ent => ent.GetType() != typeof(Player)))
                {
                    var baseTypeName = entity.GetType().Name;

                    DirectoryInfo entityDirectory;

                    //Is there a directory for this entity type? If not, then create it
                    if (!Directory.Exists(currentBackupDirectory + baseTypeName))
                        entityDirectory = Directory.CreateDirectory(currentBackupDirectory + baseTypeName);
                    else
                        entityDirectory = new DirectoryInfo(currentBackupDirectory + baseTypeName);

                    //Don't write objects that are on live players, player backup does that itself
                    if (entity.CurrentLocation != null && entity.CurrentLocation.GetType() == typeof(Player))
                        continue;

                    WriteEntity(entityDirectory, entity);
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
            try
            {
                LoggingUtility.Log("All Players backup INITIATED.", LogChannels.Backup, true);

                //Need the base dir
                if (!Directory.Exists(BaseDirectory))
                    Directory.CreateDirectory(BaseDirectory);

                var playersDir = BaseDirectory + "Players/";

                //and the base players dir
                if (!Directory.Exists(playersDir))
                    Directory.CreateDirectory(playersDir);

                //Get all the players
                var entities = LiveCache.GetAll<Player>();

                foreach (var entity in entities)
                    WriteOnePlayer(entity, false);

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
        /// Writes one player out to disk
        /// </summary>
        /// <param name="player">the player to write</param>
        /// <param name="checkDirectories">skip checking directories or not</param>
        /// <returns></returns>
        public bool WriteOnePlayer(Player entity, bool checkDirectories = true)
        {
            var playersDir = BaseDirectory + "Players/";

            //don't do double duty with the directory checking please
            if (checkDirectories)
            {
                LoggingUtility.Log("Backing up player character " + entity.DataTemplate.ID + ".", LogChannels.Backup, true);

                //Need the base dir
                if (!Directory.Exists(BaseDirectory))
                    Directory.CreateDirectory(BaseDirectory);

                //and the base players dir
                if (!Directory.Exists(playersDir))
                    Directory.CreateDirectory(playersDir);
            }

            try
            {
                var charData = (ICharacter)entity.DataTemplate;

                var charDirName = playersDir + charData.AccountHandle + "/" + charData.ID + "/";

                if (!Directory.Exists(charDirName))
                    Directory.CreateDirectory(charDirName);

                //Wipe out the existing one so we can create all new files
                if (Directory.Exists(charDirName + "Current/"))
                {
                    var currentRoot = new DirectoryInfo(charDirName + "Current/");

                    if (!Directory.Exists(charDirName + "Backups/"))
                        Directory.CreateDirectory(charDirName + "Backups/");

                    var newBackupName = String.Format("{0}Backups/{1}{2}{3}_{4}{5}{6}/",
                                        charDirName
                                        , DateTime.Now.Year
                                        , DateTime.Now.Month
                                        , DateTime.Now.Day
                                        , DateTime.Now.Hour
                                        , DateTime.Now.Minute
                                        , DateTime.Now.Second);

                    //move is literal move, no need to delete afterwards
                    currentRoot.MoveTo(newBackupName);
                }

                var currentBackupDirectory = charDirName + "Current/";
                DirectoryInfo entityDirectory = Directory.CreateDirectory(currentBackupDirectory);

                WritePlayer(entityDirectory, entity);
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
            var currentBackupDirectory = BaseDirectory + "Current/";

            //No backup directory? No live data.
            if (!Directory.Exists(currentBackupDirectory))
                return false;

            LoggingUtility.Log("World restored from current live INITIATED.", LogChannels.Backup, false);

            try
            {
                //dont load players here
                var entitiesToLoad = new List<IEntity>();
                var implimentedTypes = typeof(EntityPartial).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IEntity)) && ty.IsClass && !ty.IsAbstract && !ty.Name.Equals("Player"));

                foreach (var type in implimentedTypes)
                {
                    if (!Directory.Exists(currentBackupDirectory + type.Name))
                        continue;

                    var entityFilesDirectory = new DirectoryInfo(currentBackupDirectory + type.Name);

                    foreach (var file in entityFilesDirectory.EnumerateFiles())
                    {
                        var blankEntity = Activator.CreateInstance(type) as IEntity;

                        using (var stream = file.Open(FileMode.Open))
                        {
                            byte[] bytes = new byte[stream.Length];
                            stream.Read(bytes, 0, (int)stream.Length);
                            entitiesToLoad.Add(blankEntity.DeSerialize(bytes));
                        }
                    }
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
                        entity.MoveFrom<IInanimate>((IInanimate)obj.Item2);
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

                LoggingUtility.Log("World restored from current live.", LogChannels.Backup, false);
                return true;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return false;
        }

        /// <summary>
        /// Restores one character from their Current backup
        /// </summary>
        /// <param name="accountHandle">Global Account Handle for the account</param>
        /// <param name="charID">Which character to load</param>
        /// <returns></returns>
        public Player RestorePlayer(string accountHandle, long charID)
        {
            Player newPlayerToLoad = null;

            try
            {
                var currentBackupDirectory = BaseDirectory + "Players/" + accountHandle + "/" + charID.ToString() + "/Current/";

                //No backup directory? No live data.
                if (!Directory.Exists(currentBackupDirectory))
                    return null;

                var playerDirectory = new DirectoryInfo(currentBackupDirectory);

                //no player file to load, derp
                if (!File.Exists(playerDirectory + charID.ToString() + ".Player"))
                    return null;

                var blankEntity = Activator.CreateInstance(typeof(Player)) as Player;

                using (var stream = File.OpenRead(playerDirectory + charID.ToString() + ".Player"))
                {
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    newPlayerToLoad = (Player)blankEntity.DeSerialize(bytes);
                }

                //bad load, dump it
                if (newPlayerToLoad == null)
                    return null;

                //abstract this out to a helper maybe?
                var locationAssembly = Assembly.GetAssembly(typeof(EntityPartial));

                var ch = (ICharacter)newPlayerToLoad.DataTemplate;
                if (ch.LastKnownLocationType == null)
                    ch.LastKnownLocationType = typeof(Room).Name;

                var lastKnownLocType = locationAssembly.DefinedTypes.FirstOrDefault(tp => tp.Name.Equals(ch.LastKnownLocationType));

                ILocation lastKnownLoc = null;
                if (lastKnownLocType != null && !string.IsNullOrWhiteSpace(ch.LastKnownLocation))
                {
                    if (lastKnownLocType.GetInterfaces().Contains(typeof(ISpawnAsSingleton)))
                    {
                        long lastKnownLocID = long.Parse(ch.LastKnownLocation);
                        lastKnownLoc = LiveCache.Get<ILocation>(lastKnownLocID, lastKnownLocType);
                    }
                    else
                    {
                        var cacheKey = new LiveCacheKey(lastKnownLocType, ch.LastKnownLocation);
                        lastKnownLoc = LiveCache.Get<ILocation>(cacheKey);
                    }
                }

                newPlayerToLoad.CurrentLocation = lastKnownLoc;

                //We have the player in live cache now so make it move to the right place
                newPlayerToLoad.GetFromWorldOrSpawn();

                newPlayerToLoad.UpsertToLiveWorldCache();

                //We'll need one of these per container on players
                if (Directory.Exists(playerDirectory + "Inventory/"))
                {
                    var inventoryDirectory = new DirectoryInfo(playerDirectory + "Inventory/");

                    foreach (var file in inventoryDirectory.EnumerateFiles())
                    {
                        var blankObject = Activator.CreateInstance(typeof(Inanimate)) as Inanimate;

                        using (var stream = file.Open(FileMode.Open))
                        {
                            byte[] bytes = new byte[stream.Length];
                            stream.Read(bytes, 0, (int)stream.Length);
                            var newObj = (IInanimate)blankObject.DeSerialize(bytes);
                            newObj.UpsertToLiveWorldCache();
                            newPlayerToLoad.MoveInto<IInanimate>(newObj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return newPlayerToLoad;
        }

        /// <summary>
        /// Writes one entity to Current backup (not players)
        /// </summary>
        /// <param name="dir">Root directory to write to</param>
        /// <param name="entity">The entity to write out</param>
        private void WriteEntity(DirectoryInfo dir, IEntity entity)
        {
            var entityFileName = GetEntityFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dir.FullName + "/" + entityFileName;

            FileStream entityFile = null;

            try
            {
                if (File.Exists(fullFileName))
                    entityFile = File.Open(fullFileName, FileMode.Truncate);
                else
                    entityFile = File.Create(fullFileName);

                var bytes = entity.Serialize();
                entityFile.Write(bytes, 0, bytes.Length);

                //Don't forget to write the file out
                entityFile.Flush();
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
            finally
            {
                //dont not do this everEVERVERRFCFEVVEEV
                if (entityFile != null)
                    entityFile.Dispose();
            }
        }

        /// <summary>
        /// Writes one player out (and only one character) and their inventory to Current and archives whatever used to be Current
        /// </summary>
        /// <param name="dir">Directory to write to</param>
        /// <param name="entity">The player to write</param>
        private void WritePlayer(DirectoryInfo dir, IPlayer entity)
        {
            var entityFileName = GetPlayerFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dir.FullName + "/" + entityFileName;

            FileStream entityFile = null;

            try
            {
                if (File.Exists(fullFileName))
                    entityFile = File.Open(fullFileName, FileMode.Truncate);
                else
                    entityFile = File.Create(fullFileName);

                var bytes = entity.Serialize();
                entityFile.Write(bytes, 0, bytes.Length);

                //Don't forget to write the file out
                entityFile.Flush();

                //We also need to write out all the inventory
                foreach (var obj in entity.Inventory.EntitiesContained())
                {
                    var baseTypeName = "Inventory";

                    DirectoryInfo entityDirectory;

                    //Is there a directory for this entity type? If not, then create it
                    if (!Directory.Exists(dir.FullName + baseTypeName))
                        entityDirectory = Directory.CreateDirectory(dir.FullName + baseTypeName);
                    else
                        entityDirectory = new DirectoryInfo(dir.FullName + baseTypeName);

                    WriteEntity(entityDirectory, obj);
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
            finally
            {
                //dont not do this everEVERVERRFCFEVVEEV
                if (entityFile != null)
                    entityFile.Dispose();
            }
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

        /// <summary>
        /// Gets the statically formatted filename for a player
        /// </summary>
        /// <param name="entity">The player in question</param>
        /// <returns>the filename</returns>
        private string GetPlayerFilename(IPlayer entity)
        {
            var charData = (ICharacter)entity.DataTemplate;

            return string.Format("{0}.Player", charData.ID);
        }
    }
}
