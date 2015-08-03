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
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Inanimate(IInanimateData backingStore)
        {
            Contents = new EntityContainer<IInanimate>();
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
            Contents = new EntityContainer<IInanimate>();
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
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
                return GetContents<T>("objects");

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Get all of the entities matching a type inside this in a named container
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        /// <param name="containerName">the name of the container</param>
        public IEnumerable<T> GetContents<T>(string containerName)
        {
            switch (containerName)
            {
                case "objects":
                    return Contents.EntitiesContained.Select(ent => (T)ent);
            }

            return Enumerable.Empty<T>();
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

                if (Contents.Contains(obj))
                    return "That is already in the container";

                Contents.Add(obj);
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
        #endregion

        #region rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();
            var backingStore = (IInanimateData)DataTemplate;

            sb.Add(string.Format("There is a {0} here", backingStore.Name));

            return sb;
        }
        #endregion

        #region HotBackup
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

        /// <summary>
        /// Deserialize binary stream to this entity
        /// </summary>
        /// <param name="bytes">the binary to turn into an entity</param>
        /// <returns>the entity</returns>
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
