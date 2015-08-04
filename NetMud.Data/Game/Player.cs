using NetMud.Data.EntityBackingData;
using NetMud.Data.System;
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

        #region Rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook()
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
                                        new XAttribute("GamePermissionsRank", charData.GamePermissionsRank)),
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

            newEntity.BirthMark = xDoc.Root.Attribute("Birthmark").Value;
            newEntity.Birthdate = DateTime.Parse(xDoc.Root.Attribute("Birthdate").Value);

            backingData.ID = long.Parse(xDoc.Root.Element("BackingData").Attribute("ID").Value);

            //we have the ID, we don't want anything else from here we just want to go get the object from the db for player characters.
            var backChar = DataWrapper.GetOne<Character>(backingData.ID);

            if (backChar != null)
                backingData = backChar;
            else
            {
                //we can still use this as a failover to restore data from backups
                backingData.Name = xDoc.Root.Element("BackingData").Attribute("Name").Value;
                backingData.SurName = xDoc.Root.Element("BackingData").Attribute("Surname").Value;
                backingData.AccountHandle = xDoc.Root.Element("BackingData").Attribute("AccountHandle").Value;
                backingData.LastRevised = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("LastRevised").Value);
                backingData.Created = DateTime.Parse(xDoc.Root.Element("BackingData").Attribute("Created").Value);
                backingData.Gender = xDoc.Root.Element("BackingData").Attribute("Gender").Value;
                backingData.GamePermissionsRank = (StaffRank)Enum.ToObject(typeof(StaffRank), short.Parse(xDoc.Root.Element("BackingData").Attribute("GamePermissionsRank").Value));
                backingData.LastKnownLocation = xDoc.Root.Element("BackingData").Attribute("LastKnownLocation").Value;
                backingData.LastKnownLocationType = xDoc.Root.Element("BackingData").Attribute("LastKnownLocationType").Value;
            }

            newEntity.DataTemplate = backingData;

            //keywords is last
            newEntity.Keywords = xDoc.Root.Element("LiveData").Attribute("Keywords").Value.Split(new char[] { ',' });

            return newEntity;
        }
        #endregion
    }
}
