using NetMud.Data.EntityBackingData;
using NetMud.Data.Reference;
using NetMud.Data.System;
using NetMud.DataAccess;
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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace NetMud.Data.Game
{
    /// <summary>
    /// "Object" class
    /// </summary>
    public class Inanimate : EntityPartial, IInanimate
    {
        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Inanimate()
        {
            //IDatas need parameterless constructors
            Contents = new EntityContainer<IInanimate>();
            Pathways = new EntityContainer<IPathway>();
            MobilesInside = new EntityContainer<IMobile>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Inanimate(IInanimateData backingStore)
        {
            Contents = new EntityContainer<IInanimate>(backingStore.InanimateContainers);
            Pathways = new EntityContainer<IPathway>();
            MobilesInside = new EntityContainer<IMobile>(backingStore.MobileContainers);

            DataTemplate = backingStore;
            SpawnNewInWorld();
        }

        /// <summary>
        /// News up an entity with its backing data and where to spawn it into
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        /// <param name="spawnTo">where to spawn this into</param>
        public Inanimate(IInanimateData backingStore, IContains spawnTo)
        {
            Contents = new EntityContainer<IInanimate>(backingStore.InanimateContainers);
            Pathways = new EntityContainer<IPathway>();
            MobilesInside = new EntityContainer<IMobile>(backingStore.MobileContainers);

            DataTemplate = backingStore;
            SpawnNewInWorld(spawnTo);
        }
     
        /// <summary>
        /// Last known location this was loaded to
        /// </summary>
        public long LastKnownLocation { get; set; }

        /// <summary>
        /// Last known location type this was loaded to
        /// </summary>
        public string LastKnownLocationType { get; set; }

        #region Container
        /// <summary>
        /// Inanimates contained in this
        /// </summary>
        public IEntityContainer<IInanimate> Contents { get; set; }

        /// <summary>
        /// Pathways leading out of this
        /// </summary>
        public IEntityContainer<IPathway> Pathways { get; set; }

        /// <summary>
        /// Any mobiles (players, npcs) contained in this
        /// </summary>
        public IEntityContainer<IMobile> MobilesInside { get; set; }

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
                contents.AddRange(Contents.EntitiesContained().Select(ent => (T)ent));

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
                contents.AddRange(Contents.EntitiesContained(containerName).Select(ent => (T)ent));

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

                if (Contents.Contains(obj, containerName))
                    return "That is already in the container";

                Contents.Add(obj, containerName);
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

                if (!Contents.Contains(obj, containerName))
                    return "That is not in the container";

                Contents.Remove(obj, containerName);
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

        #region spawning
        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException("Objects can't spawn to nothing");
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IContains spawnTo)
        {
            //We can't even try this until we know if the data is there
            if (DataTemplate == null)
                throw new InvalidOperationException("Missing backing data store on object spawn event.");

            var backingStore = (IInanimateData)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(backingStore);
            Keywords = new string[] { backingStore.Name.ToLower() };
            Birthdate = DateTime.Now;

            if (spawnTo == null)
            {
                throw new NotImplementedException("Objects can't spawn to nothing");
            }

            CurrentLocation = spawnTo;

            spawnTo.MoveInto<IInanimate>(this);

            Contents = new EntityContainer<IInanimate>();

            LiveCache.Add(this);
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
            var backingStore = (IInanimateData)DataTemplate;

            sb.Add(string.Format("There is a {0} here", backingStore.Name));

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
            while (currentRadius <= strength && currentPathsSet.Count() > 0)
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
        #endregion

        #region HotBackup
        private const int liveDataVersion = 1;
        
        /// <summary>
        /// Serialize this entity's live data to a binary stream
        /// </summary>
        /// <returns>the binary stream</returns>
        public override byte[] Serialize()
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
            var charData = (IInanimateData)DataTemplate;

            var entityData = new XDocument(
                                new XElement("root",
                                    new XAttribute("formattingVersion", liveDataVersion),
                                    new XAttribute("Birthmark", BirthMark),
                                    new XAttribute("Birthdate", Birthdate),
                                    new XElement("BackingData",
                                        new XAttribute("ID", charData.ID),
                                        new XAttribute("Name", charData.Name),
                                        new XAttribute("LastRevised", charData.LastRevised),
                                        new XAttribute("Created", charData.Created),
                                        new XElement("MobileContainers"),
                                        new XElement("InanimateContainers"),
                                        new XElement("InternalComposition", JsonConvert.SerializeObject(charData.InternalComposition))),
                                    new XElement("LiveData",
                                        new XAttribute("Keywords", string.Join(",", Keywords)),
                                        new XElement("DimensionalModel",
                                            new XAttribute("Length", Model.Length),
                                            new XAttribute("Height", Model.Height),
                                            new XAttribute("Width", Model.Width),
                                            new XAttribute("ID", charData.Model.ModelBackingData.ID),
                                            new XElement("ModellingData", Model.ModelBackingData.DeserializeModel()),
                                            new XElement("MaterialCompositions", Model.SerializeMaterialCompositions()))),
                                    new XElement("Contents"),
                                    new XElement("MobilesInside")
                                    ));

            foreach (var item in Contents.EntitiesContainedByName())
                entityData.Root.Element("Contents").Add(new XElement("Item",
                                                            new XAttribute("Birthmark", item.Item2.BirthMark),
                                                            new XAttribute("Container", item.Item1)));

            foreach (var item in MobilesInside.EntitiesContainedByName().Where(ent => ent.Item2.GetType() != typeof(Player)))
                entityData.Root.Element("MobilesInside").Add(new XElement("Item",
                                                            new XAttribute("Birthmark", item.Item2.BirthMark),
                                                            new XAttribute("Container", item.Item1)));

            foreach (var item in charData.MobileContainers)
                entityData.Root.Element("BackingData").Element("MobileContainers").Add(new XElement("Container",
                                                                                                    new XAttribute("Name", item.Name),
                                                                                                    new XAttribute("CapacityVolume", item.CapacityVolume),
                                                                                                    new XAttribute("CapacityWeight", item.CapacityWeight)));
            foreach (var item in charData.InanimateContainers)
                entityData.Root.Element("BackingData").Element("InanimateContainers").Add(new XElement("Container",
                                                                                                    new XAttribute("Name", item.Name),
                                                                                                    new XAttribute("CapacityVolume", item.CapacityVolume),
                                                                                                    new XAttribute("CapacityWeight", item.CapacityWeight)));

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

            var newEntity = new Inanimate();

            var versionFormat = xDoc.Root.GetSafeAttributeValue<int>("formattingVersion");

            newEntity.BirthMark = xDoc.Root.GetSafeAttributeValue("Birthmark");
            newEntity.Birthdate = xDoc.Root.GetSafeAttributeValue<DateTime>("Birthdate");

            var internalCompositions = xDoc.Root.Element("BackingData").GetSafeAttributeValue("InternalComposition");
            var backingData = new InanimateData(internalCompositions);

            backingData.ID = xDoc.Root.Element("BackingData").GetSafeAttributeValue<long>("ID");
            backingData.Name = xDoc.Root.Element("BackingData").GetSafeAttributeValue("Name");
            backingData.LastRevised =xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("LastRevised");
            backingData.Created = xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("Created");

            foreach (var item in xDoc.Root.Element("BackingData").Element("InanimateContainers").Elements("Item"))
            {
                var newContainer = new EntityContainerData<IInanimate>();
                newContainer.CapacityVolume = item.GetSafeAttributeValue<long>("CapacityVolume");
                newContainer.CapacityWeight = item.GetSafeAttributeValue<long>("CapacityWeight");
                newContainer.Name = item.GetSafeAttributeValue("Name");

                backingData.InanimateContainers.Add(newContainer);
            }

            //Add a fake entity to get the birthmark over to the next place
            foreach (var item in xDoc.Root.Element("BackingData").Element("MobileContainers").Elements("Item"))
            {
                var newContainer = new EntityContainerData<IMobile>();
                newContainer.CapacityVolume = item.GetSafeAttributeValue<long>("CapacityVolume");
                newContainer.CapacityWeight = item.GetSafeAttributeValue<long>("CapacityWeight");
                newContainer.Name = item.GetSafeAttributeValue("Name");

                backingData.MobileContainers.Add(newContainer);
            }

            //Add a fake entity to get the birthmark over to the next place
            foreach (var item in xDoc.Root.Element("MobilesInside").Elements("Item"))
            {
                var obj = new Intelligence();
                obj.BirthMark = item.GetSafeAttributeValue("Birthmark");
                var containerName = item.GetSafeAttributeValue("Container");

                if (!String.IsNullOrWhiteSpace(containerName))
                    newEntity.MobilesInside.Add(obj, containerName);
                else
                    newEntity.MobilesInside.Add(obj);
            }


            //Add a fake entity to get the birthmark over to the next place
            foreach (var item in xDoc.Root.Element("Contents").Elements("Item"))
            {
                var obj = new Inanimate();
                obj.BirthMark = item.GetSafeAttributeValue("Birthmark");
                var containerName = item.GetSafeAttributeValue("Container");

                if (!String.IsNullOrWhiteSpace(containerName))
                    newEntity.Contents.Add(obj, containerName);
                else
                    newEntity.Contents.Add(obj);
            }

            //Add new version transformations here, they are meant to be iterative, hence < 1
            Transform_V1(backingData, newEntity, xDoc.Root, versionFormat < 1);

            newEntity.DataTemplate = backingData;

            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").Attribute("Keywords").Value.Split(new char[] { ',' });

            return newEntity;
        }

        private void Transform_V1(InanimateData backingData, Inanimate newEntity, XElement docRoot, bool older)
        {
            if (!older)
            {
                //We added dim mods in v1
                var dimModelId = docRoot.Element("LiveData").Element("DimensionalModel").GetSafeAttributeValue<long>("ID");
                var dimModelLength = docRoot.Element("LiveData").Element("DimensionalModel").GetSafeAttributeValue<int>("Length");
                var dimModelHeight = docRoot.Element("LiveData").Element("DimensionalModel").GetSafeAttributeValue<int>("Height");
                var dimModelWidth = docRoot.Element("LiveData").Element("DimensionalModel").GetSafeAttributeValue<int>("Width");
                var dimModelJson = docRoot.Element("LiveData").Element("DimensionalModel").GetSafeElementValue("ModellingData");
                var dimModelCompJson = docRoot.Element("LiveData").Element("DimensionalModel").GetSafeElementValue("MaterialCompositions");

                backingData.Model = new DimensionalModel(dimModelLength, dimModelHeight, dimModelWidth, dimModelId, dimModelCompJson);
                newEntity.Model = new DimensionalModel(dimModelLength, dimModelHeight, dimModelWidth, dimModelJson, dimModelId, dimModelCompJson);
            }
            else //what if we're older
            {
                //Get it from the db
                var backD = DataWrapper.GetOne<NonPlayerCharacter>(backingData.ID);
                backingData.Model = backD.Model;
                newEntity.Model = backD.Model;
            }
        }
        #endregion
    }
}
