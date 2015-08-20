using NetMud.Data.EntityBackingData;
using NetMud.Data.Reference;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
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
    /// Portals between locations
    /// </summary>
    public class Pathway : EntityPartial, IPathway
    {
        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Pathway()
        {
            Enter = new MessageCluster();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Pathway(IPathwayData backingStore)
        {
            Enter = new MessageCluster();
            DataTemplate = backingStore;
            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Birthmark of live location this points into
        /// </summary>
        private string _currentToLocationBirthmark;

        /// <summary>
        /// Restful live location this points into
        /// </summary>
        public ILocation ToLocation
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_currentToLocationBirthmark))
                    return LiveCache.Get<ILocation>(new LiveCacheKey(typeof(ILocation), _currentToLocationBirthmark));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _currentToLocationBirthmark = value.BirthMark;
                UpsertToLiveWorldCache();
            }
        }

        /// <summary>
        /// Birthmark of live location this points out of
        /// </summary>
        private string _currentFromLocationBirthmark;

        /// <summary>
        /// Restful live location this points out of
        /// </summary>
        public ILocation FromLocation
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_currentFromLocationBirthmark))
                    return LiveCache.Get<ILocation>(new LiveCacheKey(typeof(ILocation), _currentFromLocationBirthmark));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _currentFromLocationBirthmark = value.BirthMark;
                UpsertToLiveWorldCache();
            }
        }
        
        /// <summary>
        /// Movement messages trigger when moved through
        /// </summary>
        public MessageCluster Enter { get; set; }

        /// <summary>
        /// Cardinality direction this points towards
        /// </summary>
        public MovementDirectionType MovementDirection { get; private set; }

        #region spawning
        /// <summary>
        /// Tries to find this entity in the world based on its ID or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<Pathway>(DataTemplate.ID);

            //Isn't in the world currently
            if (me == default(IPathway))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Keywords = me.Keywords;
                Birthdate = me.Birthdate;
                CurrentLocation = me.CurrentLocation;
                DataTemplate = me.DataTemplate;
                FromLocation = me.FromLocation;
                ToLocation = me.ToLocation;
                Enter = me.Enter;
                MovementDirection = me.MovementDirection;
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            var bS = (IPathwayData)DataTemplate;

            SpawnNewInWorld(null);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IContains spawnTo)
        {
            var bS = (IPathwayData)DataTemplate;
            var locationAssembly = Assembly.GetAssembly(typeof(Room));

            MovementDirection = MessagingUtility.TranslateDegreesToDirection(bS.DegreesFromNorth);

            BirthMark = Birthmarker.GetBirthmark(bS);
            Keywords = new string[] { bS.Name.ToLower(), MovementDirection.ToString().ToLower() };
            Birthdate = DateTime.Now;

            //paths need two locations
            ILocation fromLocation = null;
            var fromLocationType = locationAssembly.DefinedTypes.FirstOrDefault(tp => tp.Name.Equals(bS.FromLocationType));

            if (fromLocationType != null && !string.IsNullOrWhiteSpace(bS.FromLocationID))
            {
                if (fromLocationType.GetInterfaces().Contains(typeof(ISpawnAsSingleton)))
                {
                    long fromLocationID = long.Parse(bS.FromLocationID);
                    fromLocation = LiveCache.Get<ILocation>(fromLocationID, fromLocationType);
                }
                else
                {
                    var cacheKey = new LiveCacheKey(fromLocationType, bS.FromLocationID);
                    fromLocation = LiveCache.Get<ILocation>(cacheKey);
                }
            }

            ILocation toLocation = null;
            var toLocationType = locationAssembly.DefinedTypes.FirstOrDefault(tp => tp.Name.Equals(bS.ToLocationType));

            if (toLocationType != null && !string.IsNullOrWhiteSpace(bS.ToLocationID))
            {
                if (toLocationType.GetInterfaces().Contains(typeof(ISpawnAsSingleton)))
                {
                    long toLocationID = long.Parse(bS.ToLocationID);
                    toLocation = LiveCache.Get<ILocation>(toLocationID, toLocationType);
                }
                else
                {
                    var cacheKey = new LiveCacheKey(toLocationType, bS.ToLocationID);
                    toLocation = LiveCache.Get<ILocation>(cacheKey);
                }
            }

            FromLocation = fromLocation;
            ToLocation = toLocation;
            CurrentLocation = fromLocation;

            Enter = new MessageCluster(new string[] { bS.MessageToActor }, new string[] { "$A$ enters you" }, new string[] { }, new string[] { bS.MessageToOrigin }, new string[] { bS.MessageToDestination });
            Enter.ToSurrounding.Add(bS.VisibleStrength, new Tuple<MessagingType, IEnumerable<string>>(MessagingType.Visible, new string[] { bS.VisibleToSurroundings }));
            Enter.ToSurrounding.Add(bS.AudibleStrength, new Tuple<MessagingType, IEnumerable<string>>(MessagingType.Visible, new string[] { bS.AudibleToSurroundings }));

            fromLocation.MoveInto<IPathway>(this);
        }
        #endregion

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>();
            var bS = (IPathwayData)DataTemplate;

            sb.Add(string.Format("{0} heads in the direction of {1} from {2} to {3}", bS.Name, MovementDirection.ToString(), FromLocation.DataTemplate.Name, ToLocation.DataTemplate.Name));

            return sb;
        }

        #region HotBackup
        private const int liveDataVersion = 1;

        /// <summary>
        /// Serialize this entity's live data to a binary stream
        /// </summary>
        /// <returns>the binary stream</returns>
        public override byte[] Serialize()
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
            var charData = (IPathwayData)DataTemplate;

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
                                        new XAttribute("PassingWidth", charData.PassingWidth),
                                        new XAttribute("PassingHeight", charData.PassingHeight),
                                        new XAttribute("DegreesFromNorth", charData.DegreesFromNorth),
                                        new XAttribute("ToLocationID", charData.ToLocationID),
                                        new XAttribute("ToLocationType", charData.ToLocationType),
                                        new XAttribute("FromLocationID", charData.FromLocationID),
                                        new XAttribute("FromLocationType", charData.FromLocationType),
                                        new XAttribute("MessageToActor", charData.MessageToActor),
                                        new XAttribute("MessageToOrigin", charData.MessageToOrigin),
                                        new XAttribute("MessageToDestination", charData.MessageToDestination),
                                        new XAttribute("AudibleToSurroundings", charData.AudibleToSurroundings),
                                        new XAttribute("AudibleStrength", charData.AudibleStrength),
                                        new XAttribute("VisibleToSurroundings", charData.VisibleToSurroundings),
                                        new XAttribute("VisibleStrength", charData.VisibleStrength)),
                                    new XElement("LiveData",
                                        new XAttribute("Keywords", string.Join(",", Keywords)),
                                        new XAttribute("RoomTo", ToLocation.BirthMark),
                                        new XAttribute("RoomFrom", FromLocation.BirthMark),
                                        new XElement("DimensionalModel",
                                            new XAttribute("Length", Model.Length),
                                            new XAttribute("Height", Model.Height),
                                            new XAttribute("Width", Model.Width),
                                            new XAttribute("ID", charData.Model.ModelBackingData.ID),
                                            new XElement("ModellingData", Model.ModelBackingData.SerializeModel()),
                                            new XElement("MaterialCompositions", Model.SerializeMaterialCompositions())))
                                    ));

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

            var backingData = new PathwayData();
            var newEntity = new Pathway();

            var versionFormat = xDoc.Root.GetSafeAttributeValue<int>("formattingVersion");

            newEntity.BirthMark = xDoc.Root.GetSafeAttributeValue("Birthmark");
            newEntity.Birthdate = xDoc.Root.GetSafeAttributeValue<DateTime>("Birthdate");

            backingData.ID = xDoc.Root.Element("BackingData").GetSafeAttributeValue<long>("ID");
            backingData.Name = xDoc.Root.Element("BackingData").GetSafeAttributeValue("Name");
            backingData.LastRevised = xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("LastRevised");
            backingData.Created = xDoc.Root.Element("BackingData").GetSafeAttributeValue<DateTime>("Created");
            backingData.PassingWidth = xDoc.Root.Element("BackingData").GetSafeAttributeValue<long>("PassingWidth");
            backingData.PassingHeight = xDoc.Root.Element("BackingData").GetSafeAttributeValue<long>("PassingHeight");
            backingData.DegreesFromNorth = xDoc.Root.Element("BackingData").GetSafeAttributeValue<int>("DegreesFromNorth");
            backingData.ToLocationID = xDoc.Root.Element("BackingData").GetSafeAttributeValue("ToLocationID");
            backingData.ToLocationType = xDoc.Root.Element("BackingData").GetSafeAttributeValue("ToLocationType");
            backingData.FromLocationID = xDoc.Root.Element("BackingData").GetSafeAttributeValue("FromLocationID");
            backingData.FromLocationType = xDoc.Root.Element("BackingData").GetSafeAttributeValue("FromLocationType");
            backingData.MessageToActor = xDoc.Root.Element("BackingData").GetSafeAttributeValue("MessageToActor");
            backingData.MessageToOrigin = xDoc.Root.Element("BackingData").GetSafeAttributeValue("MessageToOrigin");
            backingData.MessageToDestination = xDoc.Root.Element("BackingData").GetSafeAttributeValue("MessageToDestination");
            backingData.AudibleToSurroundings = xDoc.Root.Element("BackingData").GetSafeAttributeValue("AudibleToSurroundings");
            backingData.AudibleStrength = xDoc.Root.Element("BackingData").GetSafeAttributeValue<int>("AudibleStrength");
            backingData.VisibleToSurroundings = xDoc.Root.Element("BackingData").GetSafeAttributeValue("VisibleToSurroundings");
            backingData.VisibleStrength = xDoc.Root.Element("BackingData").GetSafeAttributeValue<int>("VisibleStrength");

            var obj = new Room();
            obj.BirthMark = xDoc.Root.Element("LiveData").GetSafeAttributeValue("RoomFrom");

            newEntity.FromLocation = obj;

            var toObj = new Room();
            toObj.BirthMark = xDoc.Root.Element("LiveData").GetSafeAttributeValue("RoomTo");

            newEntity.ToLocation = toObj;

            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").GetSafeAttributeValue("Keywords").Split(new char[] { ',' });

            //Add new version transformations here, they are meant to be iterative, hence < 1
            Transform_V1(backingData, newEntity, xDoc.Root, versionFormat < 1);

            newEntity.DataTemplate = backingData;

            return newEntity;
        }

        private void Transform_V1(PathwayData backingData, Pathway newEntity, XElement docRoot, bool older)
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
