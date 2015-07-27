using NetMud.Data.EntityBackingData;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace NetMud.Data.Game
{
    public class Room : EntityPartial, IRoom
    {
        public Room()
        {
            ObjectsInRoom = new EntityContainer<IInanimate>();
            MobilesInRoom = new EntityContainer<IMobile>();
            Pathways = new EntityContainer<IPathway>();
        }

        public Room(IRoomData room)
        {
            ObjectsInRoom = new EntityContainer<IInanimate>();
            MobilesInRoom = new EntityContainer<IMobile>();
            Pathways = new EntityContainer<IPathway>();

            //Yes it's its own datatemplate and currentLocation
            DataTemplate = room;

            GetFromWorldOrSpawn();
        }

        #region Container
        public IEntityContainer<IInanimate> ObjectsInRoom { get; set; }
        public IEntityContainer<IMobile> MobilesInRoom { get; set; }
        public IEntityContainer<IPathway> Pathways { get; set; }

        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
                contents.AddRange(GetContents<T>("mobiles"));
            
            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(GetContents<T>("objects"));

            if (implimentedTypes.Contains(typeof(IPathway)))
                contents.AddRange(GetContents<T>("pathways"));

            return contents;
        }

        public IEnumerable<T> GetContents<T>(string containerName)
        {
            switch (containerName)
            {
                case "mobiles":
                    return MobilesInRoom.EntitiesContained.Select(ent => (T)ent);
                case "objects":
                    return ObjectsInRoom.EntitiesContained.Select(ent => (T)ent);
                case "pathways":
                    return Pathways.EntitiesContained.Select(ent => (T)ent);
            }

            return Enumerable.Empty<T>();
        }

        public string MoveInto<T>(T thing)
        {
            return MoveInto<T>(thing, string.Empty);
        }

        public string MoveInto<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (ObjectsInRoom.Contains(obj))
                    return "That is already in the container";

                ObjectsInRoom.Add(obj);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (MobilesInRoom.Contains(obj))
                    return "That is already in the container";

                MobilesInRoom.Add(obj);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IPathway)))
            {
                var obj = (IPathway)thing;

                if (Pathways.Contains(obj))
                    return "That is already in the container";

                Pathways.Add(obj);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }


            return "Invalid type to move to container.";
        }

        public string MoveFrom<T>(T thing)
        {
            return MoveFrom<T>(thing, string.Empty);
        }

        public string MoveFrom<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (!ObjectsInRoom.Contains(obj))
                    return "That is not in the container";

                ObjectsInRoom.Remove(obj);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (!MobilesInRoom.Contains(obj))
                    return "That is not in the container";

                MobilesInRoom.Remove(obj);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IPathway)))
            {
                var obj = (IPathway)thing;

                if (!Pathways.Contains(obj))
                    return "That is not in the container";

                Pathways.Remove(obj);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();

            sb.Add(string.Format("%O%{0}%O%", DataTemplate.Name));
            sb.Add(string.Empty.PadLeft(DataTemplate.Name.Length, '-'));

            return sb;
        }

        #region Spawning
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
                DataTemplate = me.DataTemplate;
                ObjectsInRoom = me.ObjectsInRoom;
                MobilesInRoom = me.MobilesInRoom;
                Pathways = me.Pathways;
                Keywords = me.Keywords;
                CurrentLocation = me.CurrentLocation;
            }
        }

        public override void SpawnNewInWorld()
        {
            //TODO: will rooms ever be contained by something else?
            SpawnNewInWorld(this);
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            var roomTemplate = (IRoomData)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(roomTemplate);
            Keywords = new string[] { roomTemplate.Name.ToLower() };
            Birthdate = DateTime.Now;
            CurrentLocation = spawnTo;
        }
        #endregion

        #region HotBackup
        public override byte[] Serialize()
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
            var charData = (IRoomData)DataTemplate;

            var entityData = new XDocument(
                                new XElement("root",
                                    new XAttribute("Birthmark", BirthMark),
                                    new XAttribute("Birthdate", Birthdate),
                                    new XElement("BackingData",
                                        new XAttribute("ID", charData.ID),
                                        new XAttribute("Name", charData.Name),
                                        new XAttribute("LastRevised", charData.LastRevised),
                                        new XAttribute("Created", charData.Created)),
                                    new XElement("LiveData",
                                        new XAttribute("Keywords", string.Join(",", Keywords))),
                                    new XElement("ObjectsInRoom"),
                                    new XElement("MobilesInRoom")
                                    ));

            foreach (var item in ObjectsInRoom.EntitiesContained)
                entityData.Root.Element("ObjectsInRoom").Add(new XElement("Item", item.BirthMark));

            foreach (var item in MobilesInRoom.EntitiesContained.Where(ent => ent.GetType() != typeof(Player)))
                entityData.Root.Element("MobilesInRoom").Add(new XElement("Item", item.BirthMark));

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

        public override IEntity DeSerialize(byte[] bytes)
        {
            var entityBinaryConvert = new DataUtility.EntityFileData(bytes);
            var xDoc = entityBinaryConvert.XDoc;

            var backingData = new RoomData();
            var newEntity = new Room();

            newEntity.BirthMark = xDoc.Root.Attribute("Birthmark").Value;
            newEntity.Birthdate = DateTime.Parse(xDoc.Root.Attribute("Birthdate").Value);

            backingData.ID = long.Parse(xDoc.Root.Element("BackingData").Attribute("ID").Value);
            backingData.Name = xDoc.Root.Element("BackingData").Attribute("Name").Value;
            backingData.LastRevised = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("LastRevised").Value);
            backingData.Created = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("Created").Value);

            newEntity.DataTemplate = backingData;

            //Add a fake entity to get the birthmark over to the next place
            foreach(var item in xDoc.Root.Element("ObjectsInRoom").Elements("Item"))
            {
                var obj = new Inanimate();
                obj.BirthMark = item.Value;

                newEntity.ObjectsInRoom.Add(obj);
            }

            //Add a fake entity to get the birthmark over to the next place
            foreach (var item in xDoc.Root.Element("MobilesInRoom").Elements("Item"))
            {
                var obj = new Intelligence();
                obj.BirthMark = item.Value;

                newEntity.MobilesInRoom.Add(obj);
            }

            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").Attribute("Keywords").Value.Split(new char[] { ',' });

            return newEntity;
        }
        #endregion
    }
}
