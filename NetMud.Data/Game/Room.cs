using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Places entities are (most of the time)
    /// </summary>
    [Serializable]
    public class Room : EntityPartial, IRoom
    {
        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IRoomData DataTemplate
        {
            get
            {
                return BackingDataCache.Get<IRoomData>(DataTemplateId);
            }
            internal set
            {
                DataTemplateId = value.ID;
            }
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Room()
        {
            ObjectsInRoom = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
            Pathways = new EntityContainer<IPathway>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Room(IRoomData room)
        {
            ObjectsInRoom = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
            Pathways = new EntityContainer<IPathway>();

            DataTemplate = room;

            GetFromWorldOrSpawn();
        }

        #region Container
        /// <summary>
        /// Any inanimates contained in this (on the floor)
        /// </summary>
        public IEntityContainer<IInanimate> ObjectsInRoom { get; set; }

        /// <summary>
        /// Any mobiles (players, npcs) contained in this
        /// </summary>
        public IEntityContainer<IMobile> MobilesInside { get; set; }

        /// <summary>
        /// Pathways leading out of this
        /// </summary>
        public IEntityContainer<IPathway> Pathways { get; set; }

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
                contents.AddRange(MobilesInside.EntitiesContained().Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(ObjectsInRoom.EntitiesContained().Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IPathway)))
                contents.AddRange(Pathways.EntitiesContained().Select(ent => (T)ent));

            return contents;
        }

        /// <summary>
        /// Get all of the entities matching a type inside this in a named container
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        /// <param name="containerName">the name of the container</param>
        public IEnumerable<T> GetContents<T>(string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
                contents.AddRange(MobilesInside.EntitiesContained(containerName).Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(ObjectsInRoom.EntitiesContained(containerName).Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IPathway)))
                contents.AddRange(Pathways.EntitiesContained(containerName).Select(ent => (T)ent));

            return contents;
        }

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing)
        {
            return MoveInto<T>(thing, string.Empty);
        }


        /// <summary>
        /// Move an entity into a named container in this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (ObjectsInRoom.Contains(obj, containerName))
                    return "That is already in the container";

                ObjectsInRoom.Add(obj, containerName);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (MobilesInside.Contains(obj, containerName))
                    return "That is already in the container";

                MobilesInside.Add(obj, containerName);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IPathway)))
            {
                var obj = (IPathway)thing;

                if (Pathways.Contains(obj, containerName))
                    return "That is already in the container";

                Pathways.Add(obj, containerName);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move to container.";
        }

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing)
        {
            return MoveFrom<T>(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity out of this' named container
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (!ObjectsInRoom.Contains(obj, containerName))
                    return "That is not in the container";

                ObjectsInRoom.Remove(obj, containerName);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }


            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (!MobilesInside.Contains(obj, containerName))
                    return "That is not in the container";

                MobilesInside.Remove(obj, containerName);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IPathway)))
            {
                var obj = (IPathway)thing;

                if (!Pathways.Contains(obj, containerName))
                    return "That is not in the container";

                Pathways.Remove(obj, containerName);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        #region rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>();

            sb.Add(string.Format("%O%{0}%O%", DataTemplate.Name));
            sb.Add(string.Empty.PadLeft(DataTemplate.Name.Length, '-'));

            return sb;
        }

        /// <summary>
        /// Get the surrounding locations based on a strength radius
        /// </summary>
        /// <param name="strength">number of places to go out</param>
        /// <returns>list of valid surrounding locations</returns>
        public IEnumerable<ILocation> GetSurroundings(int strength)
        {
            var radiusLocations = new List<ILocation>();

            //If we don't have any paths out what can we even do
            if (Pathways.Count() == 0)
                return radiusLocations;

            var currentRadius = 0;
            var currentPathsSet = Pathways.EntitiesContained();
            while(currentRadius <= strength && currentPathsSet.Count() > 0)
            {
                var currentLocsSet = currentPathsSet.Select(path => path.ToLocation);

                if (currentLocsSet.Count() == 0)
                    break;

                radiusLocations.AddRange(currentLocsSet);
                currentPathsSet = currentLocsSet.SelectMany(ro => ro.Pathways.EntitiesContained());

                currentRadius++;
            }

            return radiusLocations;
        }

        /// <summary>
        /// Renders out an ascii map of this room plus all rooms in the radius
        /// </summary>
        /// <param name="radius">how far away to render</param>
        /// <returns>the string</returns>
        public string RenderCenteredMap(int radius, bool visibleOnly)
        {
            var sb = new StringBuilder();

            return sb.ToString();
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Tries to find this entity in the world based on its ID or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<IRoom>(DataTemplateId, typeof(IRoom));

            //Isn't in the world currently
            if (me == default(IRoom))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplateId;
                ObjectsInRoom = me.ObjectsInRoom;
                MobilesInside = me.MobilesInside;
                Pathways = me.Pathways;
                Keywords = me.Keywords;
                CurrentLocation = me.CurrentLocation;
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(this);
        }


        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IContains spawnTo)
        {
            var roomTemplate = (IRoomData)DataTemplate;

            BirthMark = LiveCache.GetUniqueIdentifier(roomTemplate);
            Keywords = new string[] { roomTemplate.Name.ToLower() };
            Birthdate = DateTime.Now;
            CurrentLocation = spawnTo;
        }
        #endregion
    }
}
