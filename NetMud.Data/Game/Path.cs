using NetMud.Data.EntityBackingData;
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
    public class Path : EntityPartial, IPath
    {
        public ILocation ToLocation { get; set; }
        public ILocation FromLocation { get; set; }
        public MessageCluster Enter { get; set; }
        public MovementDirectionType MovementDirection { get; private set; }

        public Path()
        {
            Enter = new MessageCluster();
        }

        public Path(IPathData backingStore)
        {
            Enter = new MessageCluster();
            DataTemplate = backingStore;
            GetFromWorldOrSpawn();
        }

        public void GetFromWorldOrSpawn()
        {
            var liveWorld = new LiveCache();

            //Try to see if they are already there
            var me = liveWorld.Get<Path>(DataTemplate.ID);

            //Isn't in the world currently
            if (me == default(IPath))
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

        public override void SpawnNewInWorld()
        {
            var liveWorld = new LiveCache();
            var bS = (IPathData)DataTemplate;

            SpawnNewInWorld(null);
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            var liveWorld = new LiveCache();
            var bS = (IPathData)DataTemplate;
            var locationAssembly = Assembly.GetAssembly(typeof(ILocation));

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
                    fromLocation = liveWorld.Get<ILocation>(fromLocationID, fromLocationType);
                }
                else
                {
                    var cacheKey = new LiveCacheKey(fromLocationType, bS.FromLocationID);
                    fromLocation = liveWorld.Get<ILocation>(cacheKey);
                }
            }

            ILocation toLocation = null;
            var toLocationType = locationAssembly.DefinedTypes.FirstOrDefault(tp => tp.Name.Equals(bS.ToLocationType));

            if (toLocationType != null && !string.IsNullOrWhiteSpace(bS.ToLocationID))
            {
                if (toLocationType.GetInterfaces().Contains(typeof(ISpawnAsSingleton)))
                {
                    long toLocationID = long.Parse(bS.ToLocationID);
                    toLocation = liveWorld.Get<ILocation>(toLocationID, toLocationType);
                }
                else
                {
                    var cacheKey = new LiveCacheKey(toLocationType, bS.ToLocationID);
                    toLocation = liveWorld.Get<ILocation>(cacheKey);
                }
            }

            FromLocation = fromLocation;
            ToLocation = toLocation;
            CurrentLocation = fromLocation;

            Enter = new MessageCluster(bS.MessageToActor, "$A$ enters you", string.Empty, bS.MessageToOrigin, bS.MessageToDestination);
            Enter.ToSurrounding.Add(bS.VisibleStrength, new Tuple<MessagingType, string>(MessagingType.Visible, bS.VisibleToSurroundings));
            Enter.ToSurrounding.Add(bS.AudibleStrength, new Tuple<MessagingType, string>(MessagingType.Visible, bS.AudibleToSurroundings));

            fromLocation.MoveInto<IPath>(this);
        }

        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();
            var bS = (IPathData)DataTemplate;

            sb.Add(string.Format("{0} heads in the direction of {1} from {2} to {3}", bS.Name, MovementDirection.ToString(), FromLocation.DataTemplate.Name, ToLocation.DataTemplate.Name));

            return sb;
        }

        #region HotBackup
        public override byte[] Serialize()
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
            var charData = (IPathData)DataTemplate;

            var entityData = new XDocument(
                                new XElement("root",
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
                                        new XAttribute("RoomFrom", FromLocation.BirthMark))
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

        public override IEntity DeSerialize(byte[] bytes)
        {
            var entityBinaryConvert = new DataUtility.EntityFileData(bytes);
            var xDoc = entityBinaryConvert.XDoc;

            var backingData = new PathData();
            var newEntity = new Path();

            newEntity.BirthMark = xDoc.Root.Attribute("Birthmark").Value;
            newEntity.Birthdate = DateTime.Parse(xDoc.Root.Attribute("Birthdate").Value);

            backingData.ID = long.Parse(xDoc.Root.Element("BackingData").Attribute("ID").Value);
            backingData.Name = xDoc.Root.Element("BackingData").Attribute("Name").Value;
            backingData.LastRevised = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("LastRevised").Value);
            backingData.Created = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("Created").Value);
            backingData.PassingWidth = long.Parse(xDoc.Root.Element("BackingData").Attribute("PassingWidth").Value);
            backingData.PassingHeight = long.Parse(xDoc.Root.Element("BackingData").Attribute("PassingHeight").Value);
            backingData.DegreesFromNorth = int.Parse(xDoc.Root.Element("BackingData").Attribute("DegreesFromNorth").Value);
            backingData.ToLocationID = xDoc.Root.Element("BackingData").Attribute("ToLocationID").Value;
            backingData.ToLocationType = xDoc.Root.Element("BackingData").Attribute("ToLocationType").Value;
            backingData.FromLocationID = xDoc.Root.Element("BackingData").Attribute("FromLocationID").Value;
            backingData.FromLocationType = xDoc.Root.Element("BackingData").Attribute("FromLocationType").Value;
            backingData.MessageToActor = xDoc.Root.Element("BackingData").Attribute("MessageToActor").Value;
            backingData.MessageToOrigin = xDoc.Root.Element("BackingData").Attribute("MessageToOrigin").Value;
            backingData.MessageToDestination = xDoc.Root.Element("BackingData").Attribute("MessageToDestination").Value;
            backingData.AudibleToSurroundings = xDoc.Root.Element("BackingData").Attribute("AudibleToSurroundings").Value;
            backingData.AudibleStrength = int.Parse(xDoc.Root.Element("BackingData").Attribute("AudibleStrength").Value);
            backingData.VisibleToSurroundings = xDoc.Root.Element("BackingData").Attribute("VisibleToSurroundings").Value;
            backingData.VisibleStrength = int.Parse(xDoc.Root.Element("BackingData").Attribute("VisibleStrength").Value);

            newEntity.DataTemplate = backingData;

            var obj = new Room();
            obj.BirthMark = xDoc.Root.Element("LiveData").Attribute("RoomFrom").Value;

            newEntity.FromLocation = obj;

            var toObj = new Room();
            toObj.BirthMark = xDoc.Root.Element("LiveData").Attribute("RoomTo").Value;

            newEntity.ToLocation = toObj;

            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").Attribute("Keywords").Value.Split(new char[] { ',' });

            return newEntity;
        }
        #endregion
    }
}
