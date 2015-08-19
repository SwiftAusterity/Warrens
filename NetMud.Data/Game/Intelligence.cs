using NetMud.Data.EntityBackingData;
using NetMud.Data.Reference;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Actionable;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace NetMud.Data.Game
{
    /// <summary>
    /// NPCs
    /// </summary>
    public class Intelligence : EntityPartial, IIntelligence
    {
        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Intelligence()
        {
            //IDatas need parameterless constructors
            Inventory = new EntityContainer<IInanimate>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Intelligence(INonPlayerCharacter backingStore)
        {
            Inventory = new EntityContainer<IInanimate>();
            DataTemplate = backingStore;
            SpawnNewInWorld();
        }

        /// <summary>
        /// News up an entity with its backing data and where to spawn it into
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        /// <param name="spawnTo">where to spawn this into</param>
        public Intelligence(INonPlayerCharacter backingStore, IContains spawnTo)
        {
            Inventory = new EntityContainer<IInanimate>();
            DataTemplate = backingStore;
            SpawnNewInWorld(spawnTo);
        }

        #region Rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>();
            var ch = (INonPlayerCharacter)DataTemplate;

            sb.Add(string.Format("This is {0}", ch.FullName()));

            return sb;
        }
        #endregion

        #region Container
        /// <summary>
        /// Inanimates contained in this
        /// </summary>
        public IEntityContainer<IInanimate> Inventory { get; set; }

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Inventory.EntitiesContained().Select(ent => (T)ent));

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

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Inventory.EntitiesContained(containerName).Select(ent => (T)ent));

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

                if (Inventory.Contains(obj, containerName))
                    return "That is already in the container";

                Inventory.Add(obj, containerName);
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

                if (!Inventory.Contains(obj, containerName))
                    return "That is not in the container";

                Inventory.Remove(obj, containerName);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        #region SpawnBehavior
        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException("NPCs can't spawn to nothing");
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IContains spawnTo)
        {
            //We can't even try this until we know if the data is there
            if (DataTemplate == null)
                throw new InvalidOperationException("Missing backing data store on NPC spawn event.");

            var backingStore = (INonPlayerCharacter)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(backingStore);
            Keywords = new string[] { backingStore.Name.ToLower() };
            Birthdate = DateTime.Now;

            if (spawnTo == null)
            {
                throw new NotImplementedException("NPCs can't spawn to nothing");
            }

            CurrentLocation = spawnTo;

            spawnTo.MoveInto<IIntelligence>(this);

            Inventory = new EntityContainer<IInanimate>();

            LiveCache.Add(this);
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
            var charData = (INonPlayerCharacter)DataTemplate;

            var entityData = new XDocument(
                                new XElement("root",
                                    new XAttribute("formattingVersion", liveDataVersion),
                                    new XAttribute("Birthmark", BirthMark),
                                    new XAttribute("Birthdate", Birthdate),
                                    new XElement("BackingData",
                                        new XAttribute("ID", charData.ID),
                                        new XAttribute("Name", charData.Name),
                                        new XAttribute("Surname", charData.SurName),
                                        new XAttribute("LastRevised", charData.LastRevised),
                                        new XAttribute("Created", charData.Created),
                                        new XAttribute("Gender", charData.Gender),
                                        new XElement("Race", 
                                            new XAttribute("ID", charData.RaceData.ID),
                                            new XAttribute("Head", charData.RaceData.Head.ID),
                                            new XAttribute("Torso", charData.RaceData.Torso.ID),
                                            new XAttribute("SanguinaryMaterial", charData.RaceData.SanguinaryMaterial.ID),
                                            new XAttribute("Breathes", (short)charData.RaceData.Breathes),
                                            new XAttribute("DietaryNeeds", (short)charData.RaceData.DietaryNeeds),
                                            new XAttribute("EmergencyLocation", charData.RaceData.EmergencyLocation.ID),
                                            new XAttribute("StartingLocation", charData.RaceData.StartingLocation.ID),
                                            new XAttribute("TeethType", (short)charData.RaceData.TeethType),
                                            new XAttribute("TemperatureToleranceLow", charData.RaceData.TemperatureTolerance.Item1),
                                            new XAttribute("TemperatureToleranceHigh", charData.RaceData.TemperatureTolerance.Item2),
                                            new XAttribute("VisionRangeLow", charData.RaceData.VisionRange.Item1),
                                            new XAttribute("VisionRangeHigh", charData.RaceData.VisionRange.Item2),
                                            new XElement("Arms", 
                                                new XAttribute("ID", charData.RaceData.Arms.Item1.ID),
                                                new XAttribute("Amount", charData.RaceData.Arms.Item2)),
                                            new XElement("Legs", 
                                                new XAttribute("ID", charData.RaceData.Legs.Item1.ID),
                                                new XAttribute("Amount", charData.RaceData.Legs.Item2)),
                                            new XElement("BodyParts", JsonConvert.SerializeObject(charData.RaceData.BodyParts)))),
                                    new XElement("LiveData",
                                        new XAttribute("Keywords", string.Join(",", Keywords)),
                                        new XElement("DimensionalModel",
                                            new XAttribute("Length", Model.Length),
                                            new XAttribute("Height", Model.Height),
                                            new XAttribute("Width", Model.Width),
                                            new XAttribute("ID", charData.Model.ModelBackingData.ID),
                                            new XElement("ModellingData", Model.ModelBackingData.DeserializeModel()),
                                            new XElement("MaterialCompositions", Model.SerializeMaterialCompositions()))),
                                    new XElement("Inventory")
                                    ));

            foreach(var item in Inventory.EntitiesContained())
                entityData.Root.Element("Inventory").Add(new XElement("Item", item.BirthMark));

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

            var backingData = new NonPlayerCharacter();
            var newEntity = new Intelligence();

            var versionFormat = xDoc.Root.GetSafeAttributeValue<int>("formattingVersion");

            newEntity.BirthMark = xDoc.Root.GetSafeAttributeValue("Birthmark");
            newEntity.Birthdate = xDoc.Root.GetSafeAttributeValue<DateTime>("Birthdate");

            backingData.ID = xDoc.Root.Element("BackingData").GetSafeAttributeValue<long>("ID");
            backingData.Name = xDoc.Root.Element("BackingData").GetSafeAttributeValue("Name");
            backingData.SurName = xDoc.Root.Element("BackingData").GetSafeAttributeValue("Surname");
            backingData.LastRevised = xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("LastRevised");
            backingData.Created = xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("Created");
            backingData.Gender = xDoc.Root.Element("BackingData").GetSafeAttributeValue("Gender");

            //Add a fake entity to get the birthmark over to the next place
            foreach (var item in xDoc.Root.Element("Inventory").Elements("Item"))
            {
                var obj = new Inanimate();
                obj.BirthMark = item.Value;

                newEntity.Inventory.Add(obj);
            }


            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").GetSafeAttributeValue("Keywords").Split(new char[] { ',' });

            //Add new version transformations here, they are meant to be iterative, hence < 1
            Transform_V1(backingData, newEntity, xDoc.Root, versionFormat < 1);

            newEntity.DataTemplate = backingData;

            return newEntity;
        }

        private void Transform_V1(NonPlayerCharacter backingData, Intelligence newEntity, XElement docRoot, bool older)
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

                // We added Race in v1 as well
                var raceID = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<long>("ID");
                var raceHeadID = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<long>("Head");
                var raceTorsoID = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<long>("Torso");
                var raceSanguinaryMaterialID = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<long>("SanguinaryMaterial");
                var raceBreathes = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<short>("Breathes");
                var raceDietaryNeeds = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<short>("DietaryNeeds");
                var raceEmergencyLocationID = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<long>("EmergencyLocation");
                var raceStartingLocationID = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<long>("StartingLocation");
                var raceTeethType = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<short>("TeethType");
                var raceTemperatureToleranceLow = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<short>("TemperatureToleranceLow");
                var raceTemperatureToleranceHigh = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<short>("TemperatureToleranceHigh");
                var raceVisionRangeLow = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<short>("VisionRangeLow");
                var raceVisionRangeHigh = docRoot.Element("BackingData").Element("Race").GetSafeAttributeValue<short>("VisionRangeHigh");
                var raceArmsId = docRoot.Element("BackingData").Element("Race").Element("Arms").GetSafeAttributeValue<long>("ID");
                var raceArmsAmount = docRoot.Element("BackingData").Element("Race").Element("Arms").GetSafeAttributeValue<short>("Amount");
                var raceLegsId = docRoot.Element("BackingData").Element("Race").Element("Legs").GetSafeAttributeValue<long>("ID");
                var raceLegsAmount = docRoot.Element("BackingData").Element("Race").Element("Legs").GetSafeAttributeValue<short>("Amount");
                var raceBodyPartsJson = docRoot.Element("BackingData").Element("Race").GetSafeElementValue("BodyParts");

                var raceData = new Race(raceBodyPartsJson);

                raceData.ID = raceID;
                raceData.Head = DataWrapper.GetOne<InanimateData>(raceHeadID);
                raceData.Torso = DataWrapper.GetOne<InanimateData>(raceTorsoID);
                raceData.SanguinaryMaterial = ReferenceWrapper.GetOne<IMaterial>(raceSanguinaryMaterialID);
                raceData.Breathes = (RespiratoryType)raceBreathes;
                raceData.DietaryNeeds = (DietType)raceDietaryNeeds;
                raceData.EmergencyLocation = DataWrapper.GetOne<RoomData>(raceEmergencyLocationID);
                raceData.StartingLocation = DataWrapper.GetOne<RoomData>(raceStartingLocationID);
                raceData.TeethType = (DamageType)raceDietaryNeeds;
                raceData.TemperatureTolerance = new Tuple<short, short>(raceTemperatureToleranceLow, raceTemperatureToleranceHigh);
                raceData.VisionRange = new Tuple<short, short>(raceVisionRangeLow, raceVisionRangeHigh);
                raceData.Arms = new Tuple<IInanimateData, short>(DataWrapper.GetOne<InanimateData>(raceArmsId), raceArmsAmount);
                raceData.Legs = new Tuple<IInanimateData, short>(DataWrapper.GetOne<InanimateData>(raceLegsId), raceLegsAmount);
            }
            else //what if we're older
            {
                //Get it from the db
                var backD = DataWrapper.GetOne<NonPlayerCharacter>(backingData.ID);
                backingData.Model = backD.Model;
                backingData.RaceData = backD.RaceData;
                newEntity.Model = backD.Model;
            }
        }
        #endregion
    }
}
