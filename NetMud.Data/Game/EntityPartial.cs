using NetMud.DataAccess;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Abstract that tries to keep the entity classes cleaner
    /// </summary>
    public abstract class EntityPartial : IEntity
    {
        #region Data and live tracking properties
        /// <summary>
        /// Unique string for this live entity
        /// </summary>
        public string BirthMark { get; internal set; }

        /// <summary>
        /// When this entity was born to the world
        /// </summary>
        public DateTime Birthdate { get; internal set; }

        /// <summary>
        /// The backing data for this live entity
        /// </summary>
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
        private Func<IEnumerable<string>, bool> _writeTo;

        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public Func<IEnumerable<string>, bool> WriteTo 
        { 
            get
            {
                if (_writeTo != null)
                {
                    var pred = new Predicate<IEnumerable<string>>(_writeTo);
                    return new Func<IEnumerable<string>, bool>(pred);
                }

                return (input) => TriggerAIAction(input);
            }
            set
            {
                _writeTo = value;
            }
        }

        /// <summary>
        /// Where in the live world this is
        /// </summary>
        private string _currentLocationBirthmark;

        /// <summary>
        /// Where in the live world this is
        /// </summary>
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

        /// <summary>
        /// Serialize this entity's live data to a binary stream
        /// </summary>
        /// <returns>the binary stream</returns>
        public abstract byte[] Serialize();

        /// <summary>
        /// Deserialize binary stream to this entity
        /// </summary>
        /// <param name="bytes">the binary to turn into an entity</param>
        /// <returns>the entity</returns>
        public abstract IEntity DeSerialize(byte[] bytes);
    }
}
