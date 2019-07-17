using NetMud.Communication;
using NetMud.Data.Architectural.Serialization;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        private string[] _keywords;

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public string[] Keywords
        {
            get
            {
                if (_keywords == null)
                {
                    if (Template<ITemplate>() == null)
                    {
                        if (string.IsNullOrWhiteSpace(TemplateName))
                        {
                            _keywords = new string[0];
                        }
                        else
                        {
                            _keywords = new string[] { TemplateName.ToLower() };
                        }
                    }
                    else
                    {
                        _keywords = Template<ITemplate>().Keywords;
                    }
                }

                return _keywords;
            }
            set
            {
                _keywords = value;
            }
        }
        #endregion

        #region Connection Stuff
        /// <summary>
        /// Method by which this entity has output (from commands and events) "shown" to it
        /// </summary>
        public virtual bool WriteTo(IEnumerable<string> output, bool delayed = false)
        {
            return true;
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

        #region Command Queue
        /// <summary>
        /// Buffer of output to send to clients via WriteTo
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IList<IEnumerable<string>> OutputBuffer { get; set; }

        /// <summary>
        /// Buffer of command string input sent from the client
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IList<string> InputBuffer { get; set; }

        /// <summary>
        /// What is currently being executed
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public string CurrentAction { get; set; }

        /// <summary>
        /// Stops whatever is being executed and clears the input buffer
        /// </summary>
        public void StopInput()
        {
            HaltInput();
            FlushInput();
        }

        /// <summary>
        /// Stops whatever is being executed, does not clear the input buffer
        /// </summary>
        public void HaltInput()
        {
            CurrentAction = string.Empty;
        }

        /// <summary>
        /// Clears the input buffer
        /// </summary>
        public void FlushInput()
        {
            InputBuffer = new List<string>();
        }

        /// <summary>
        /// Returns whats in the input buffer
        /// </summary>
        /// <returns>Any strings still in the input buffer</returns>
        public IEnumerable<string> PeekInput()
        {
            var newList = new List<string>() { string.Format("Acting: {0}", CurrentAction) };

            newList.AddRange(InputBuffer);

            return newList;
        }
        #endregion

        /// <summary>
        /// The description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of live qualities of this entity
        /// </summary>
        [UIHint("QualityList")]
        public HashSet<IQuality> Qualities { get; set; }

        /// <summary>
        /// Where in the live world this is
        /// </summary>
        [JsonConverter(typeof(ConcreteTypeConverter<GlobalPosition>))]
        public IGlobalPosition CurrentLocation { get; set; }

        /// <summary>
        /// Check for a quality
        /// </summary>
        /// <param name="name">Gets the value of the request quality</param>
        /// <returns>The value</returns>
        public virtual int GetQuality(string name)
        {
            IQuality currentQuality = Qualities.FirstOrDefault(qual => qual.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (currentQuality == null)
            {
                return 0;
            }

            return currentQuality.Value;
        }

        /// <summary>
        /// Add a quality (can be negative)
        /// </summary>
        /// <param name="value">The value you're adding</param>
        /// <param name="additive">Is this additive or replace-ive</param>
        /// <returns>The new value</returns>
        public int SetQuality(int value, string quality, bool additive)
        {
            IQuality currentQuality = Qualities.FirstOrDefault(qual => qual.Name.Equals(quality, StringComparison.InvariantCultureIgnoreCase));

            if (currentQuality == null)
            {
                Qualities.Add(new Quality()
                {
                    Name = quality,
                    Type = QualityType.Aspect,
                    Visible = true,
                    Value = value
                });

                return value;
            }

            if (additive)
            {
                currentQuality.Value += value;
            }
            else
            {
                currentQuality.Value = value;
            }

            return value;
        }

        public EntityPartial()
        {
            OutputBuffer = new List<IEnumerable<string>>();
            InputBuffer = new List<string>();
        }

        #region Movement
        /// <summary>
        /// Change the position of this without physical movement
        /// </summary>
        /// <param name="newPosition">The new position the thing is in, will return with the original one if nothing moved</param>
        /// <returns>was this thing moved?</returns>
        public virtual string TryTeleport(IGlobalPosition newPosition)
        {
            return TryMoveTo(newPosition);
        }

        public abstract string TryMoveTo(IGlobalPosition newPosition);
        #endregion

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
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public abstract void SpawnNewInWorld(IGlobalPosition spawnTo);

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

        #region Data Management
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
        #endregion

        #region Generic Rendering
        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetFullDescription(IEntity viewer)
        {
            return Description;
            //Self becomes the first sense in the list
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetImmediateDescription(IEntity viewer)
        {
            return Description;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetDescribableName(IEntity viewer)
        {
            return TemplateName;
        }
        #endregion

        #region Visual Rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string RenderToVisible(IEntity viewer)
        {
            return GetFullDescription(viewer);
        }
        #endregion

        #region Containment Rendering
        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string RenderAsContents(IEntity viewer)
        {
            //Add the existential modifiers
            return GetImmediateDescription(viewer);
        }

        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="holder">entity holding the thing</param>
        /// <returns>the output</returns>
        public virtual string RenderAsHeld(IEntity viewer, IEntity holder)
        {
            return GetImmediateDescription(viewer);
        }
        #endregion  

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

        public bool TryMoveDirection(MovementDirectionType direction, IGlobalPosition newPosition)
        {
            return true;
        }
        #endregion

        public abstract object Clone();
    }
}
