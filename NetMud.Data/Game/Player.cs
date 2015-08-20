using NetMud.Data.EntityBackingData;
using NetMud.Data.Reference;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Actionable;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace NetMud.Data.Game
{
    /// <summary>
    /// live player character entities
    /// </summary>
    public class Player : EntityPartial, IPlayer
    {
        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Player()
        {
            Inventory = new EntityContainer<IInanimate>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="character">the backing data</param>
        public Player(ICharacter character)
        {
            Inventory = new EntityContainer<IInanimate>();
            DataTemplate = character;
            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// ID from the descriptor connection for this player
        /// </summary>
        public string DescriptorID { get; set; }

        /// <summary>
        /// Type of descriptor this player connected with
        /// </summary>
        public DescriptorType Descriptor { get; set; }

        /// <summary>
        /// Function used to close this connection
        /// </summary>
        public Func<bool> CloseConnection { get; set; }


        /// <summary>
        /// Birthmark for current live location of this
        /// </summary>
        private string _currentLocationBirthmark;

        /// <summary>
        /// Restful location container this is inside of
        /// </summary>
        public override IContains CurrentLocation
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_currentLocationBirthmark))
                    return LiveCache.Get<IContains>(new LiveCacheKey(typeof(IContains), _currentLocationBirthmark));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _currentLocationBirthmark = value.BirthMark;
                UpsertToLiveWorldCache();

                //We save character data to ensure the player remains where it was on last known change
                var ch = (Character)DataTemplate;
                ch.LastKnownLocation = value.DataTemplate.ID.ToString();
                ch.LastKnownLocationType = value.GetType().Name;
                ch.Save();
            }
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            var charData = (Character)DataTemplate;
            var height = charData.RaceData.Head.Model.Height + charData.RaceData.Torso.Model.Height + charData.RaceData.Legs.Item1.Model.Height;
            var length = charData.RaceData.Torso.Model.Length;
            var width = charData.RaceData.Torso.Model.Width;

            return new Tuple<int, int, int>(height, length, width);
        }

        #region Rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>();
            var ch = (ICharacter)DataTemplate;

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
        /// Tries to find this entity in the world based on its ID or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<Player>(DataTemplate.ID);

            //Isn't in the world currently
            if (me == default(IPlayer))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplate = me.DataTemplate;
                Inventory = me.Inventory;
                Keywords = me.Keywords;

                if (me.CurrentLocation == null)
                {
                    var newLoc = GetBaseSpawn();
                    newLoc.MoveInto<IPlayer>(this);
                }
                else
                    me.CurrentLocation.MoveInto<IPlayer>(this);
            }
        }

        /// <summary>
        /// Find the emergency we dont know where to spawn this guy spawn location
        /// </summary>
        /// <returns>The emergency spawn location</returns>
        private IContains GetBaseSpawn()
        {
            //TODO: Not hardcode the zeroth room
            return LiveCache.Get<Room>(1);
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            var ch = (ICharacter)DataTemplate;
            var locationAssembly = Assembly.GetAssembly(typeof(ILocation));

            if (ch.LastKnownLocationType == null)
                ch.LastKnownLocationType = typeof(IRoom).Name;

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

            SpawnNewInWorld(lastKnownLoc);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IContains spawnTo)
        {
            var ch = (ICharacter)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(ch);
            Keywords = new string[] { ch.Name.ToLower(), ch.SurName.ToLower() };
            Birthdate = DateTime.Now;

            if (spawnTo == null)
                spawnTo = GetBaseSpawn();

            CurrentLocation = spawnTo;

            //Set the data context's stuff too so we don't have to do this over again
            ch.LastKnownLocation = spawnTo.DataTemplate.ID.ToString();
            ch.LastKnownLocationType = spawnTo.GetType().Name;
            ch.Save();

            spawnTo.MoveInto<IPlayer>(this);

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
            var charData = (ICharacter)DataTemplate;

            var entityData = new XDocument(
                                new XElement("root",
                                    new XAttribute("formattingVersion", liveDataVersion),
                                    new XAttribute("Birthmark", BirthMark),
                                    new XAttribute("Birthdate", Birthdate),
                                    new XElement("BackingData",
                                        new XAttribute("ID", charData.ID),
                                        new XAttribute("Name", charData.Name),
                                        new XAttribute("Surname", charData.SurName),
                                        new XAttribute("AccountHandle", charData.AccountHandle),
                                        new XAttribute("LastRevised", charData.LastRevised),
                                        new XAttribute("Created", charData.Created),
                                        new XAttribute("Gender", charData.Gender),
                                        new XAttribute("LastKnownLocationType", charData.LastKnownLocationType),
                                        new XAttribute("LastKnownLocation", charData.LastKnownLocation),
                                        new XAttribute("GamePermissionsRank", charData.GamePermissionsRank),
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
                                        new XAttribute("Keywords", string.Join(",", Keywords))),
                                    new XElement("Inventory")
                                    ));

            foreach (var item in Inventory.EntitiesContained())
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

            var backingData = new Character();
            var newEntity = new Player();

            var versionFormat = xDoc.Root.GetSafeAttributeValue<int>("formattingVersion");

            newEntity.BirthMark = xDoc.Root.GetSafeAttributeValue("Birthmark");
            newEntity.Birthdate = xDoc.Root.GetSafeAttributeValue<DateTime>("Birthdate");

            backingData.ID = xDoc.Root.Element("BackingData").GetSafeAttributeValue<long>("ID");

            //we have the ID, we don't want anything else from here we just want to go get the object from the db for player characters.
            var backChar = DataWrapper.GetOne<Character>(backingData.ID);

            if (backChar != null)
                backingData = backChar;
            else
            {
                //we can still use this as a failover to restore data from backups
                backingData.Name = xDoc.Root.Element("BackingData").GetSafeAttributeValue("Name");
                backingData.SurName = xDoc.Root.Element("BackingData").GetSafeAttributeValue("Surname");
                backingData.AccountHandle = xDoc.Root.Element("BackingData").GetSafeAttributeValue("AccountHandle");
                backingData.LastRevised = xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("LastRevised");
                backingData.Created = xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("Created");
                backingData.Gender = xDoc.Root.Element("BackingData").GetSafeAttributeValue("Gender");
                backingData.GamePermissionsRank = (StaffRank)Enum.ToObject(typeof(StaffRank), xDoc.Root.Element("BackingData").GetSafeAttributeValue<short>("GamePermissionsRank"));
                backingData.LastKnownLocation = xDoc.Root.Element("BackingData").GetSafeAttributeValue("LastKnownLocation");
                backingData.LastKnownLocationType = xDoc.Root.Element("BackingData").GetSafeAttributeValue("LastKnownLocationType");
            }

            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").Attribute("Keywords").Value.Split(new char[] { ',' });

            //Add new version transformations here, they are meant to be iterative, hence < 1
            Transform_V1(backingData, newEntity, xDoc.Root, versionFormat < 1);

            newEntity.DataTemplate = backingData;

            return newEntity;
        }

        private void Transform_V1(Character backingData, Player newEntity, XElement docRoot, bool older)
        {
            if (!older)
            {
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
                raceData.SanguinaryMaterial = ReferenceWrapper.GetOne<Material>(raceSanguinaryMaterialID);
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
                backingData.RaceData = backD.RaceData;
            }
        }
        #endregion
    }
}
