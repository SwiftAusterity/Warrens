using NetMud.Data.EntityBackingData;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
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
    public class Inanimate : EntityPartial, IInanimate
    {
        public Inanimate()
        {
            //IDatas need parameterless constructors
            Contents = new EntityContainer<IInanimate>();
        }

        public Inanimate(IInanimateData backingStore)
        {
            Contents = new EntityContainer<IInanimate>();
            DataTemplate = backingStore;
            SpawnNewInWorld();
        }

        public Inanimate(IInanimateData backingStore, IContains spawnTo)
        {
            Contents = new EntityContainer<IInanimate>();
            DataTemplate = backingStore;
            SpawnNewInWorld(spawnTo);
        }
     
        public long LastKnownLocation { get; set; }
        public string LastKnownLocationType { get; set; }

        #region Container
        public IEntityContainer<IInanimate> Contents { get; set; }

        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
                return GetContents<T>("objects");

            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> GetContents<T>(string containerName)
        {
            switch (containerName)
            {
                case "objects":
                    return Contents.EntitiesContained.Select(ent => (T)ent);
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

                if (Contents.Contains(obj))
                    return "That is already in the container";

                Contents.Add(obj);
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

                if (!Contents.Contains(obj))
                    return "That is not in the container";

                Contents.Remove(obj);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException("Objects can't spawn to nothing");
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            //We can't even try this until we know if the data is there
            if (DataTemplate == null)
                throw new InvalidOperationException("Missing backing data store on object spawn event.");

            var backingStore = (IInanimateData)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(backingStore);
            Keywords = new string[] { backingStore.Name.ToLower() };
            Birthdate = DateTime.Now;

            //TODO: People get a base spawn but live objects need to be spawnable to a specific location or not at all really
            if (spawnTo == null)
            {
                throw new NotImplementedException("Objects can't spawn to nothing");
            }

            CurrentLocation = spawnTo;

            spawnTo.MoveInto<IInanimate>(this);

            Contents = new EntityContainer<IInanimate>();

            LiveCache.Add(this);
        }

        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();
            var backingStore = (IInanimateData)DataTemplate;

            sb.Add(string.Format("There is a {0} here", backingStore.Name));

            return sb;
        }

        #region HotBackup
        public override byte[] Serialize()
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
            var charData = (IInanimateData)DataTemplate;

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
                                    new XElement("Contents")
                                    ));

            foreach (var item in Contents.EntitiesContained)
                entityData.Root.Element("Contents").Add(new XElement("Item", item.BirthMark));

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

            var backingData = new InanimateData();
            var newEntity = new Inanimate();

            newEntity.BirthMark = xDoc.Root.Attribute("Birthmark").Value;
            newEntity.Birthdate = DateTime.Parse(xDoc.Root.Attribute("Birthdate").Value);

            backingData.ID = long.Parse(xDoc.Root.Element("BackingData").Attribute("ID").Value);
            backingData.Name = xDoc.Root.Element("BackingData").Attribute("Name").Value;
            backingData.LastRevised = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("LastRevised").Value);
            backingData.Created = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("Created").Value);

            //Add a fake entity to get the birthmark over to the next place
            foreach (var item in xDoc.Root.Element("Contents").Elements("Item"))
            {
                var obj = new Inanimate();
                obj.BirthMark = item.Value;

                newEntity.Contents.Add(obj);
            }

            newEntity.DataTemplate = backingData;

            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").Attribute("Keywords").Value.Split(new char[] { ',' });

            return newEntity;
        }
        #endregion
    }
}
