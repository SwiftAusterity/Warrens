using NetMud.Communication;
using NetMud.Communication.Messaging;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Abstract that tries to keep the entity classes cleaner
    /// </summary>
    [Serializable]
    public abstract class EntityPartial : SerializableDataPartial, IEntity
    {
        #region Data and live tracking properties
        /// <summary>
        /// Unique string for this live entity
        /// </summary>
        public string BirthMark { get; set; }

        /// <summary>
        /// When this entity was born to the world
        /// </summary>
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// The Id for the backing data
        /// </summary>
        public long DataTemplateId { get; set; }

        /// <summary>
        /// The backing data for this live entity
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IData DataTemplate { get; internal set; }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public abstract Tuple<int, int, int> GetModelDimensions();

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        private string[] _keywords;

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        public string[] Keywords
        {
            get { return _keywords; }
            set
            {
                _keywords = value;
                UpsertToLiveWorldCache();
            }
        }
        #endregion

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public virtual bool WriteTo(IEnumerable<string> input)
        {
            var strings = MessagingUtility.TranslateColorVariables(input.ToArray(), this);

            return TriggerAIAction(strings);
        }

        private IChannelType _internalDescriptor;
        public virtual IChannelType ConnectionType
        {
            get 
            { 
                if(_internalDescriptor == null)
                    _internalDescriptor = new InternalChannel();

                return _internalDescriptor;
            }
        }

        /// <summary>
        /// Where in the live world this is
        /// </summary>
        [JsonProperty("CurrentLocation")]
        private string _currentLocationBirthmark;

        /// <summary>
        /// Where in the live world this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public virtual IContains CurrentLocation
        {
            get 
            { 
                if(!String.IsNullOrWhiteSpace(_currentLocationBirthmark))
                    return LiveCache.Get<IContains>(new LiveCacheKey(typeof(IContains), _currentLocationBirthmark));

                return null; 
            }
            set
            {
                if (value == null)
                    return;

                _currentLocationBirthmark = value.BirthMark;
                UpsertToLiveWorldCache();
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public abstract void SpawnNewInWorld();

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public abstract void SpawnNewInWorld(IContains spawnTo);

        /// <summary>
        /// Update this entry to the live world cache
        /// </summary>
        public void UpsertToLiveWorldCache()
        {
            LiveCache.Add(this);
        }

        /// <summary>
        /// For non-player entities - accepts output "shown" to it by the parser as a result of commands and events
        /// </summary>
        /// <param name="input">the output strings</param>
        /// <param name="trigger">the methodology type (heard, seen, etc)</param>
        /// <returns></returns>
        public bool TriggerAIAction(IEnumerable<string> input, AITriggerType trigger = AITriggerType.Seen)
        {
            //TODO: Actual AI code
            return true;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public abstract IEnumerable<string> RenderToLook(IEntity actor);

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(IEntity other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != this.GetType())
                        return -1;

                    if (other.BirthMark.Equals(this.BirthMark))
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IEntity other)
        {
            if (other != default(IEntity))
            {
                try
                {
                    return other.GetType() == this.GetType() && other.BirthMark.Equals(this.BirthMark);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
        #endregion
 
        /*
         * Override these for live things since live things can contain other things
         */
        #region "Serialization"
        /*
        public override IEntity DeSerialize(string jsonData)
        {
            //Need to deserialize the entire object but also find things inside of the object and put them back
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
        }

        public override string Serialize()
        {
            //Need to serialize the entire object but also add anything contained inside of the objects
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
        }
        */
        #endregion
    }
}
