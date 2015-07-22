using NetMud.Data.EntityBackingData;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
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
    public class Intelligence : EntityPartial, IIntelligence
    {
        public Intelligence()
        {
            //IDatas need parameterless constructors
            Inventory = new EntityContainer<IObject>();
        }

        public Intelligence(INonPlayerCharacter backingStore)
        {
            Inventory = new EntityContainer<IObject>();
            DataTemplate = backingStore;
            SpawnNewInWorld();
        }

        public Intelligence(INonPlayerCharacter backingStore, IContains spawnTo)
        {
            Inventory = new EntityContainer<IObject>();
            DataTemplate = backingStore;
            SpawnNewInWorld(spawnTo);
        }

        #region Rendering
        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();
            var ch = (INonPlayerCharacter)DataTemplate;

            sb.Add(string.Format("This is {0}", ch.FullName()));

            return sb;
        }
        #endregion

        #region Container
        public EntityContainer<IObject> Inventory { get; set; }

        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IObject)))
                return GetContents<T>("objects");

            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> GetContents<T>(string containerName)
        {
            switch (containerName)
            {
                case "objects":
                    return Inventory.EntitiesContained.Select(ent => (T)ent);
            }

            return Enumerable.Empty<T>();
        }

        public string MoveInto<T>(T thing)
        {
            return MoveInto<T>(thing, string.Empty);
        }

        public string MoveInto<T>(T thing, string containerName)
        {
            if (typeof(T).GetInterfaces().Contains(typeof(IObject)))
            {
                var obj = (IObject)thing;

                if (Inventory.Contains(obj))
                    return "That is already in the container";

                Inventory.Add(obj);
                obj.CurrentLocation = this;
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
            if (typeof(T).GetInterfaces().Contains(typeof(IObject)))
            {
                var obj = (IObject)thing;

                if (!Inventory.Contains(obj))
                    return "That is not in the container";

                Inventory.Remove(obj);
                obj.CurrentLocation = null;
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        #region SpawnBehavior
        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException("NPCs can't spawn to nothing");
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            //We can't even try this until we know if the data is there
            if (DataTemplate == null)
                throw new InvalidOperationException("Missing backing data store on NPC spawn event.");

            var liveWorld = new LiveCache();
            var backingStore = (INonPlayerCharacter)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(backingStore);
            Keywords = new string[] { backingStore.Name.ToLower() };
            Birthdate = DateTime.Now;

            //TODO: People get a base spawn but live objects need to be spawnable to a specific location or not at all really
            if (spawnTo == null)
            {
                throw new NotImplementedException("NPCs can't spawn to nothing");
            }

            CurrentLocation = spawnTo;

            spawnTo.MoveInto<IIntelligence>(this);

            Inventory = new EntityContainer<IObject>();

            liveWorld.Add(this);
        }
        #endregion

        #region HotBackup
        public override byte[] Serialize()
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
            var charData = (INonPlayerCharacter)DataTemplate;

            var entityData = new XDocument(
                                new XElement("root",
                                    new XAttribute("Birthmark", BirthMark),
                                    new XAttribute("Birthdate", Birthdate),
                                    new XElement("BackingData",
                                        new XAttribute("ID", charData.ID),
                                        new XAttribute("Name", charData.Name),
                                        new XAttribute("Surname", charData.SurName),
                                        new XAttribute("LastRevised", charData.LastRevised),
                                        new XAttribute("Created", charData.Created),
                                        new XAttribute("Gender", charData.Gender)),
                                    new XElement("LiveData",
                                        new XAttribute("Keywords", string.Join(",", Keywords))),
                                    new XElement("Inventory")
                                    ));

            foreach(var item in Inventory.EntitiesContained)
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

        public override IEntity DeSerialize(byte[] bytes)
        {
            var entityBinaryConvert = new DataUtility.EntityFileData(bytes);
            var xDoc = entityBinaryConvert.XDoc;

            var backingData = new NonPlayerCharacter();
            var newEntity = new Intelligence();

            newEntity.BirthMark = xDoc.Root.Attribute("Birthmark").Value;
            newEntity.Birthdate = DateTime.Parse(xDoc.Root.Attribute("Birthdate").Value);

            backingData.ID = long.Parse(xDoc.Root.Element("BackingData").Attribute("ID").Value);
            backingData.Name = xDoc.Root.Element("BackingData").Attribute("Name").Value;
            backingData.SurName = xDoc.Root.Element("BackingData").Attribute("Surname").Value;
            backingData.LastRevised = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("LastRevised").Value);
            backingData.Created = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("Created").Value);
            backingData.Gender = xDoc.Root.Element("BackingData").Attribute("Gender").Value;

            //Add a fake entity to get the birthmark over to the next place
            foreach (var item in xDoc.Root.Element("Inventory").Elements("Item"))
            {
                var obj = new Object();
                obj.BirthMark = item.Value;

                newEntity.Inventory.Add(obj);
            }

            newEntity.DataTemplate = backingData;

            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").Attribute("Keywords").Value.Split(new char[] { ',' });

            return newEntity;
        }
        #endregion
    }
}
