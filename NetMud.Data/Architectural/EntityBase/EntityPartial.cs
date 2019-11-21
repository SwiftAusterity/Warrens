using NetMud.Communication;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural.Serialization;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.NPC.IntelligenceControl;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural.EntityBase
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
        /// An internal date for checking the last time this was saved
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        internal DateTime CleanUntil { get; set; } = DateTime.Now;

        /// <summary>
        /// The Id for the backing data
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public abstract string TemplateName { get; }

        /// <summary>
        /// The backing data for this live entity
        /// </summary>
        public abstract T Template<T>() where T : IKeyedData;

        public virtual bool IsPlayer()
        {
            return false;
        }
        #endregion

        #region Connection Stuff
        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public virtual bool WriteTo(IEnumerable<string> output, bool delayed = false)
        {
            //null output means send wrapper to players
            if(output == null)
            {
                if(IsPlayer())
                {
                    var thisPlayer = (IPlayer)this;

                    thisPlayer.Descriptor.SendWrapper();
                }

                return true;
            }

            IEnumerable<string> strings = MessagingUtility.TranslateColorVariables(output.ToArray(), this);

            return TriggerAIAction(strings);
        }

        private IChannelType _internalDescriptor;

        [ScriptIgnore]
        [JsonIgnore]
        public virtual IChannelType ConnectionType
        {
            get
            {
                if (_internalDescriptor == null)
                {
                    _internalDescriptor = new InternalChannel();
                }

                return _internalDescriptor;
            }
        }
        #endregion

        #region Ownership
        /// <summary>
        /// Who created this thing, their GlobalAccountHandle
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public string CreatorHandle { get { return Template<ITemplate>().CreatorHandle; } }

        /// <summary>
        /// Who created this thing
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IAccount Creator { get { return Template<ITemplate>().Creator; } }

        /// <summary>
        /// The creator's account permissions level
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public StaffRank CreatorRank { get { return Template<ITemplate>().CreatorRank; } }
        #endregion

        /// <summary>
        /// The description
        /// </summary>
        public string Description { get; set; }

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public virtual CacheType CachingType => CacheType.Live;

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public abstract void SpawnNewInWorld();

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool PersistToCache()
        {
            try
            {
                UpsertToLiveWorldCache();
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update this entry to the live world cache
        /// </summary>
        public void UpsertToLiveWorldCache(bool forceSave = false)
        {
            LiveCache.Add(this);

            DateTime now = DateTime.Now;

            if (CleanUntil < now.AddMinutes(-5) || forceSave)
            {
                CleanUntil = now;
                Save();
            }
        }
        #endregion

        /// <summary>
        /// Save this to the filesystem in Current
        /// </summary>
        /// <returns>Success</returns>
        public virtual bool Save()
        {
            try
            {
                LiveData dataAccessor = new LiveData();
                dataAccessor.WriteEntity(this);
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Save this to the filesystem in Current
        /// </summary>
        /// <returns>Success</returns>
        public virtual bool Remove()
        {
            try
            {
                LiveData dataAccessor = new LiveData();
                dataAccessor.RemoveEntity(this);
                LiveCache.Remove(new LiveCacheKey(this));
            }
            catch
            {
                return false;
            }

            return true;
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

        #region Processes
        public virtual void KickoffProcesses()
        {
        }
        #endregion

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(ILiveData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.BirthMark.Equals(BirthMark))
                    {
                        return 1;
                    }

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
        public bool Equals(ILiveData other)
        {
            if (other != default(ILiveData))
            {
                try
                {
                    return other.GetType() == GetType() && other.BirthMark.Equals(BirthMark);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILiveData x, ILiveData y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(ILiveData obj)
        {
            return obj.GetType().GetHashCode() + obj.BirthMark.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + BirthMark.GetHashCode();
        }
        #endregion

        public abstract object Clone();
    }
}
