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
using System.Web.Hosting;
using NetMud.Utility;
using NetMud.DataStructure.Base.Place; 

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

        public bool WriteLiveBackup()
        {
            //No backup directory? Make it
            if (!Directory.Exists(BaseDirectory))
                Directory.CreateDirectory(BaseDirectory);

            //Get all the entities (which should be a ton of stuff)
            var entities = LiveWorld.GetAll();

            //Dont save players to the hot section
            foreach(var entity in entities.Where(ent => !ent.GetType().GetInterfaces().Contains(typeof(IPlayer))))
            {
                var baseTypeName = entity.GetType().Name;

                DirectoryInfo entityDirectory;

                //Is there a directory for this entity type? If not, then create it
                if (!Directory.Exists(HostingEnvironment.MapPath(BaseDirectory + baseTypeName)))
                    entityDirectory = Directory.CreateDirectory(BaseDirectory + baseTypeName);
                else
                    entityDirectory = new DirectoryInfo(BaseDirectory + baseTypeName);

                WriteEntity(entityDirectory, entity);
            }

            return true;
        }

        private void WriteEntity(DirectoryInfo dir, IEntity entity)
        {
            var entityFileName = GetEntityFilename(entity);

            if(string.IsNullOrWhiteSpace(entityFileName))
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
                if(entityFile != null)
                    entityFile.Dispose();
            }
        }

        private string GetEntityFilename(IEntity entity)
        {
             return string.Format("{0}.{1}", entity.BirthMark, entity.GetType().Name);
        }

        public bool RestoreLiveBackup()
        {
            //No backup directory? No live data.
            if (!Directory.Exists(BaseDirectory))
                return false;

            try
            {

                var entitiesToLoad = new List<IEntity>();
                var implimentedTypes = typeof(EntityPartial).Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(typeof(IEntity)) && ty.IsClass && !ty.IsAbstract);

                foreach (var type in implimentedTypes)
                {
                    if (!Directory.Exists(BaseDirectory + type.Name))
                        continue;

                    var entityFilesDirectory = new DirectoryInfo(BaseDirectory + type.Name);

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
                {
                    entity.UpsertToLiveWorldCache();
                }

                //We have the containers contents and the birthmarks from the deserial
                //I don't know how we can even begin to do this type agnostically since the collections are held on type specific objects without some super ugly reflection
                foreach (Room entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(Room)))
                {
                    IEntity[] objectsContained = new IEntity[entity.ObjectsInRoom.EntitiesContained.Count];
                    entity.ObjectsInRoom.EntitiesContained.CopyTo(objectsContained, 0);

                    foreach (IObject obj in objectsContained)
                    {
                        var fullObj = LiveWorld.Get<IObject>(new LiveCacheKey(typeof(NetMud.Data.Game.Object), obj.BirthMark));
                        entity.MoveFrom<IObject>(obj);
                        entity.MoveInto<IObject>(fullObj);
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

                    foreach (IObject obj in objectsContained)
                    {
                        var fullObj = LiveWorld.Get<IObject>(new LiveCacheKey(typeof(NetMud.Data.Game.Object), obj.BirthMark));
                        entity.MoveFrom<IObject>(obj);
                        entity.MoveInto<IObject>(fullObj);
                    }
                }

                foreach (NetMud.Data.Game.Object entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(NetMud.Data.Game.Object)))
                {
                    IEntity[] objectsContained = new IEntity[entity.Contents.EntitiesContained.Count];
                    entity.Contents.EntitiesContained.CopyTo(objectsContained, 0);

                    foreach (IObject obj in objectsContained)
                    {
                        var fullObj = LiveWorld.Get<IObject>(new LiveCacheKey(typeof(NetMud.Data.Game.Object), obj.BirthMark));
                        entity.MoveFrom<IObject>(obj);
                        entity.MoveInto<IObject>(fullObj);
                    }
                }

                //paths load themselves to their room
                foreach (NetMud.Data.Game.Path entity in entitiesToLoad.Where(ent => ent.GetType() == typeof(NetMud.Data.Game.Path)))
                {
                    IRoom roomTo = LiveWorld.Get<IRoom>(new LiveCacheKey(typeof(NetMud.Data.Game.Room), entity.ToLocation.BirthMark));
                    IRoom roomFrom = LiveWorld.Get<IRoom>(new LiveCacheKey(typeof(NetMud.Data.Game.Room), entity.FromLocation.BirthMark));

                    if (roomTo != null && roomFrom != null)
                    {
                        entity.ToLocation = roomTo;
                        entity.FromLocation = roomFrom;
                        entity.CurrentLocation = roomFrom;
                        roomFrom.MoveInto<IPath>(entity);
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

        public bool NewWorldFallback()
        {
            LiveWorld.PreLoadAll<RoomData>();
            LiveWorld.PreLoadAll<PathData>();

            return true;
        }
    }
}
