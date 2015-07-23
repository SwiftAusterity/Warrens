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

namespace NetMud.LiveData
{
    /// <summary>
    /// The engine behind the system that constantly writes out live data so we can reboot into the prior state if needs be
    /// BaseDirectory should end with a trailing slash
    /// </summary>
    public class HotBackup
    {
        public LiveCache LiveWorld { get; private set; }
        public string BaseDirectory { get; private set; }

        public HotBackup(string baseDirectory)
        {
            LiveWorld = new LiveCache();
            BaseDirectory = baseDirectory;
        }

        public bool NewWorldFallback()
        {
            //Only load in stuff that is static and spawns as singleton
            LiveWorld.PreLoadAll<RoomData>();
            LiveWorld.PreLoadAll<PathwayData>();

            return true;
        }

        public bool WriteLiveBackup()
        {
            try
            {
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
                var entities = LiveWorld.GetAll();

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

                return WritePlayers();
            }
            catch
            {
                //logging
                return false;
            }
        }

        /// <summary>
        /// Players are written to their own private directories, with the full current/dated backup cycle for each dude
        /// </summary>
        /// <returns></returns>
        public bool WritePlayers()
        {
            try
            {
                //Need the base dir
                if (!Directory.Exists(BaseDirectory))
                    Directory.CreateDirectory(BaseDirectory);

                var playersDir = BaseDirectory + "Players/";

                //and the base players dir
                if (!Directory.Exists(playersDir))
                    Directory.CreateDirectory(playersDir);

                //Get all the players
                var entities = LiveWorld.GetAll<Player>();

                foreach (var entity in entities)
                {
                    var charData = (ICharacter)entity.DataTemplate;

                    var myDirName = playersDir + charData.AccountHandle + "/";

                    //Wipe out the existing one so we can create all new files
                    if (Directory.Exists(myDirName + "Current/"))
                    {
                        var currentRoot = new DirectoryInfo(myDirName + "Current/");

                        if (!Directory.Exists(myDirName + "Backups/"))
                            Directory.CreateDirectory(myDirName + "Backups/");

                        var newBackupName = String.Format("{0}Backups/{1}{2}{3}_{4}{5}{6}/",
                                            myDirName
                                            , DateTime.Now.Year
                                            , DateTime.Now.Month
                                            , DateTime.Now.Day
                                            , DateTime.Now.Hour
                                            , DateTime.Now.Minute
                                            , DateTime.Now.Second);

                        //move is literal move, no need to delete afterwards
                        currentRoot.MoveTo(newBackupName);
                    }

                    var currentBackupDirectory = myDirName + "Current/";
                    DirectoryInfo entityDirectory = Directory.CreateDirectory(currentBackupDirectory);

                    WritePlayer(entityDirectory, entity);
                }
            }
            catch
            {
                //let the upper caller handle the error itself
                throw;
            }

            return true;
        }

        public bool RestoreLiveBackup()
        {
            var currentBackupDirectory = BaseDirectory + "Current/";

            //No backup directory? No live data.
            if (!Directory.Exists(currentBackupDirectory))
                return false;

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

                foreach (var entity in entitiesToLoad.OrderBy(ent => ent.Birthdate))
                    entity.UpsertToLiveWorldCache();

                //We have the containers contents and the birthmarks from the deserial
                //I don't know how we can even begin to do this type agnostically since the collections are held on type specific objects without some super ugly reflection
                foreach (Room entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Room)))
                {
                    IEntity[] objectsContained = new IEntity[entity.ObjectsInRoom.EntitiesContained.Count];
                    entity.ObjectsInRoom.EntitiesContained.CopyTo(objectsContained, 0);

                    foreach (IInanimate obj in objectsContained)
                    {
                        var fullObj = LiveWorld.Get<IInanimate>(new LiveCacheKey(typeof(NetMud.Data.Game.Inanimate), obj.BirthMark));
                        entity.MoveFrom<IInanimate>(obj);
                        entity.MoveInto<IInanimate>(fullObj);
                    }

                    IEntity[] mobilesContained = new IEntity[entity.MobilesInRoom.EntitiesContained.Count];
                    entity.MobilesInRoom.EntitiesContained.CopyTo(mobilesContained, 0);

                    foreach (IIntelligence obj in mobilesContained)
                    {
                        var fullObj = LiveWorld.Get<IIntelligence>(new LiveCacheKey(typeof(Intelligence), obj.BirthMark));
                        entity.MoveFrom<IIntelligence>(obj);
                        entity.MoveInto<IIntelligence>(fullObj);
                    }
                }

                foreach (Intelligence entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Intelligence)))
                {
                    IEntity[] objectsContained = new IEntity[entity.Inventory.EntitiesContained.Count];
                    entity.Inventory.EntitiesContained.CopyTo(objectsContained, 0);

                    foreach (IInanimate obj in objectsContained)
                    {
                        var fullObj = LiveWorld.Get<IInanimate>(new LiveCacheKey(typeof(NetMud.Data.Game.Inanimate), obj.BirthMark));
                        entity.MoveFrom<IInanimate>(obj);
                        entity.MoveInto<IInanimate>(fullObj);
                    }
                }

                foreach (NetMud.Data.Game.Inanimate entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(NetMud.Data.Game.Inanimate)))
                {
                    IEntity[] objectsContained = new IEntity[entity.Contents.EntitiesContained.Count];
                    entity.Contents.EntitiesContained.CopyTo(objectsContained, 0);

                    foreach (IInanimate obj in objectsContained)
                    {
                        var fullObj = LiveWorld.Get<IInanimate>(new LiveCacheKey(typeof(NetMud.Data.Game.Inanimate), obj.BirthMark));
                        entity.MoveFrom<IInanimate>(obj);
                        entity.MoveInto<IInanimate>(fullObj);
                    }
                }

                //paths load themselves to their room
                foreach (NetMud.Data.Game.Pathway entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(NetMud.Data.Game.Pathway)))
                {
                    IRoom roomTo = LiveWorld.Get<IRoom>(new LiveCacheKey(typeof(NetMud.Data.Game.Room), entity.ToLocation.BirthMark));
                    IRoom roomFrom = LiveWorld.Get<IRoom>(new LiveCacheKey(typeof(NetMud.Data.Game.Room), entity.FromLocation.BirthMark));

                    if (roomTo != null && roomFrom != null)
                    {
                        entity.ToLocation = roomTo;
                        entity.FromLocation = roomFrom;
                        entity.CurrentLocation = roomFrom;
                        roomFrom.MoveInto<IPathway>(entity);
                    }
                }
            }
            catch
            {
                //TODO: Logging
                return false;
            }

            return true;
        }

        public Player RestorePlayer(string accountHandle)
        {
            Player newPlayerToLoad = null;

            try
            {
                var currentBackupDirectory = BaseDirectory + "Players/" + accountHandle + "/Current/";

                //No backup directory? No live data.
                if (!Directory.Exists(currentBackupDirectory))
                    return null;

                var playerDirectory = new DirectoryInfo(currentBackupDirectory);

                //no player file to load, derp
                if (!File.Exists(playerDirectory + accountHandle + ".Player"))
                    return null;

                var blankEntity = Activator.CreateInstance(typeof(Player)) as Player;

                using (var stream = File.OpenRead(playerDirectory + accountHandle + ".Player"))
                {
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    newPlayerToLoad = (Player)blankEntity.DeSerialize(bytes);
                }

                //bad load, dump it
                if (newPlayerToLoad == null)
                    return null;

                //abstract this out to a helper maybe?
                var locationAssembly = Assembly.GetAssembly(typeof(ILocation));

                var ch = (ICharacter)newPlayerToLoad.DataTemplate;
                if (ch.LastKnownLocationType == null)
                    ch.LastKnownLocationType = typeof(IRoom).Name;

                var lastKnownLocType = locationAssembly.DefinedTypes.FirstOrDefault(tp => tp.Name.Equals(ch.LastKnownLocationType));

                ILocation lastKnownLoc = null;
                if (lastKnownLocType != null && !string.IsNullOrWhiteSpace(ch.LastKnownLocation))
                {
                    if (lastKnownLocType.GetInterfaces().Contains(typeof(ISpawnAsSingleton)))
                    {
                        long lastKnownLocID = long.Parse(ch.LastKnownLocation);
                        lastKnownLoc = LiveWorld.Get<ILocation>(lastKnownLocID, lastKnownLocType);
                    }
                    else
                    {
                        var cacheKey = new LiveCacheKey(lastKnownLocType, ch.LastKnownLocation);
                        lastKnownLoc = LiveWorld.Get<ILocation>(cacheKey);
                    }
                }

                newPlayerToLoad.CurrentLocation = lastKnownLoc;

                //We have the player in live cache now so make it move to the right place
                newPlayerToLoad.GetFromWorldOrSpawn();

                newPlayerToLoad.UpsertToLiveWorldCache();

                //remove all the ghost objects in the player inventory
                IEntity[] objectsContained = new IEntity[newPlayerToLoad.Inventory.EntitiesContained.Count];
                newPlayerToLoad.Inventory.EntitiesContained.CopyTo(objectsContained, 0);

                foreach (IInanimate obj in objectsContained)
                    newPlayerToLoad.MoveFrom<IInanimate>(obj);

                //We'll need one of these per container on players
                if (Directory.Exists(playerDirectory + "Inventory/"))
                {
                    var inventoryDirectory = new DirectoryInfo(playerDirectory + "Inventory/");

                    foreach (var file in inventoryDirectory.EnumerateFiles())
                    {
                        var blankObject = Activator.CreateInstance(typeof(NetMud.Data.Game.Inanimate)) as NetMud.Data.Game.Inanimate;

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
            catch
            {
                //logging
            }

            return newPlayerToLoad;
        }

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
            catch
            {
                //boogey boogey
            }
            finally
            {
                //dont not do this everEVERVERRFCFEVVEEV
                if (entityFile != null)
                    entityFile.Dispose();
            }
        }

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
                foreach (var obj in entity.Inventory.EntitiesContained)
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
            catch
            {
                //boogey boogey
            }
            finally
            {
                //dont not do this everEVERVERRFCFEVVEEV
                if (entityFile != null)
                    entityFile.Dispose();
            }
        }

        private string GetEntityFilename(IEntity entity)
        {
            return string.Format("{0}.{1}", entity.BirthMark, entity.GetType().Name);
        }

        private string GetPlayerFilename(IPlayer entity)
        {
            var charData = (ICharacter)entity.DataTemplate;

            return string.Format("{0}.Player", charData.AccountHandle);
        }
    }
}
