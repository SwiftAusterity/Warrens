using NetMud.Data.EntityBackingData;
using NetMud.Data.Reference;
using NetMud.Data.System;
using NetMud.DataAccess;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;

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
        public new IRoomData DataTemplate
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
            var me = LiveCache.Get<IRoom>(DataTemplate.ID, typeof(IRoom));

            //Isn't in the world currently
            if (me == default(IRoom))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplate.ID;
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

        #region HotBackup
        private const int liveDataVersion = 1;
        /*
        /// <summary>
        /// Serialize this entity's live data to a binary stream
        /// </summary>
        /// <returns>the binary stream</returns>
        public override byte[] Serialize()
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
            var charData = (IRoomData)DataTemplate;

            var entityData = new XDocument(
                                new XElement("root",
                                    new XAttribute("formattingVersion", liveDataVersion),
                                    new XAttribute("Birthmark", BirthMark),
                                    new XAttribute("Birthdate", Birthdate),
                                    new XElement("BackingData",
                                        new XAttribute("ID", charData.ID),
                                        new XAttribute("Name", charData.Name),
                                        new XAttribute("Medium", charData.Medium.ID),
                                        new XAttribute("Zone", charData.ZoneAffiliation.ID),
                                        new XAttribute("LastRevised", charData.LastRevised),
                                        new XAttribute("Created", charData.Created),
                                        new XElement("Borders", charData.SerializeBorders())),
                                   new XElement("LiveData",
                                        new XAttribute("Keywords", string.Join(",", Keywords)),
                                        new XElement("DimensionalModel",
                                            new XAttribute("Length", Model.Length),
                                            new XAttribute("Height", Model.Height),
                                            new XAttribute("Width", Model.Width))),
                                    new XElement("ObjectsInRoom"),
                                    new XElement("MobilesInside")
                                    ));

            foreach (var item in ObjectsInRoom.EntitiesContained())
                entityData.Root.Element("ObjectsInRoom").Add(new XElement("Item", item.BirthMark));

            foreach (var item in MobilesInside.EntitiesContained().Where(ent => ent.GetType() != typeof(Player)))
                entityData.Root.Element("MobilesInside").Add(new XElement("Item", item.BirthMark));

            //pathways will load themselves

            var entityBinaryConvert = new DataUtility.EntityFileData(entityData);

            using (var memoryStream = new MemoryStream())
            using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
            {
                entityData.WriteTo(xmlWriter);
                xmlWriter.Flush();
                entityBinaryConvert.XmlBinary = memoryStream.ToArray();
            }

            return entityBinaryConvert.XmlBinary;
        }

        /// <summary>
        /// Deserialize binary stream to this entity
        /// </summary>
        /// <param name="bytes">the binary to turn into an entity</param>
        /// <returns>the entity</returns>
        public override IEntity DeSerialize(byte[] bytes)
        {
            var entityBinaryConvert = new DataUtility.EntityFileData(bytes);
            var xDoc = entityBinaryConvert.XDoc;

            var backingData = new RoomData();
            var newEntity = new Room();

            var versionFormat = xDoc.Root.GetSafeAttributeValue<int>("formattingVersion");

            //This block represents "version zero" stuff we get from the xml
            newEntity.BirthMark = xDoc.Root.GetSafeAttributeValue("Birthmark");
            newEntity.Birthdate = xDoc.Root.GetSafeAttributeValue<DateTime>("Birthdate");

            backingData.ID = xDoc.Root.Element("BackingData").GetSafeAttributeValue<long>("ID");
            backingData.Name = xDoc.Root.Element("BackingData").GetSafeAttributeValue("Name");
            backingData.LastRevised = xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("LastRevised");
            backingData.Created = xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("Created");

            //Add a fake entity to get the birthmark over to the next place
            foreach(var item in xDoc.Root.Element("ObjectsInRoom").Elements("Item"))
            {
                var obj = new Inanimate();
                obj.BirthMark = item.Value;

                newEntity.ObjectsInRoom.Add(obj);
            }

            //Add a fake entity to get the birthmark over to the next place
            foreach (var item in xDoc.Root.Element("MobilesInside").Elements("Item"))
            {
                var obj = new Intelligence();
                obj.BirthMark = item.Value;

                newEntity.MobilesInside.Add(obj);
            }

            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").GetSafeAttributeValue<string>("Keywords").Split(new char[] { ',' });

            //Add new version transformations here, they are meant to be iterative, hence < 1
            Transform_V1(backingData, newEntity, xDoc.Root, versionFormat < 1);

            newEntity.DataTemplate = backingData;

            return newEntity;
        }
        */
        private void Transform_V1(RoomData backingData, Room newEntity, XElement docRoot, bool older)
        {
            if (!older)
            {
                //We added dim mods in v1
                var dimModelLength = docRoot.Element("LiveData").Element("DimensionalModel").GetSafeAttributeValue<int>("Length");
                var dimModelHeight = docRoot.Element("LiveData").Element("DimensionalModel").GetSafeAttributeValue<int>("Height");
                var dimModelWidth = docRoot.Element("LiveData").Element("DimensionalModel").GetSafeAttributeValue<int>("Width");

                backingData.Model = new DimensionalModel(dimModelLength, dimModelHeight, dimModelWidth);
                newEntity.Model = backingData.Model;

                //Added medium, zone and wall materials in v1
                var mediumId = docRoot.Element("BackingData").GetSafeAttributeValue<long>("Medium");
                backingData.Medium = ReferenceWrapper.GetOne<IMaterial>(mediumId);

                var zoneId = docRoot.Element("BackingData").GetSafeAttributeValue<long>("Zone");
                backingData.ZoneAffiliation = ReferenceWrapper.GetOne<IZone>(zoneId);

                backingData.Borders = backingData.DeserializeBorders(docRoot.Element("LiveData").GetSafeElementValue("Borders"));
            }
            else //what if we're older
            {
                //Get it from the db
                var backD = DataWrapper.GetOne<RoomData>(backingData.ID);
                backingData.Model = backD.Model;
                newEntity.Model = backD.Model;
                backingData.Borders = backD.Borders;
                backingData.Medium = backD.Medium;
                backingData.ZoneAffiliation = backD.ZoneAffiliation;
            }
        }
        #endregion
    }
}
