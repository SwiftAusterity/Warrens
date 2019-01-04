using NetMud.CentralControl;
using NetMud.Communication;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural.Serialization;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.NPC.IntelligenceControl;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public virtual bool WriteTo(IEnumerable<string> input)
        {
            IEnumerable<string> strings = MessagingUtility.TranslateColorVariables(input.ToArray(), this);

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
                    _internalDescriptor = new InternalChannel();

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
        /// spawn interactions to pass onto entities as they are popped
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public HashSet<IDecayEvent> DecayEvents { get; set; }

        /// <summary>
        /// spawn interactions to pass onto entities as they are popped
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public HashSet<IInteraction> Interactions { get; set; }

        /// <summary>
        /// List of live qualities of this entity
        /// </summary>
        public HashSet<IQuality> Qualities { get; set; }

        /// <summary>
        /// Where in the live world this is
        /// </summary>
        [JsonConverter(typeof(ConcreteTypeConverter<GlobalPosition>))]
        public IGlobalPosition CurrentLocation { get; set; }

        public virtual IGlobalPosition AbsolutePosition()
        {
            //TODO: Default to emergency location
            if (CurrentLocation?.CurrentZone == null)
                return null;

            return CurrentLocation;
        }

        /// <summary>
        /// Check for a quality
        /// </summary>
        /// <param name="name">Gets the value of the request quality</param>
        /// <returns>The value</returns>
        public virtual int GetQuality(string name)
        {
            IQuality currentQuality = Qualities.FirstOrDefault(qual => qual.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (currentQuality == null)
                return 0;

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
                currentQuality.Value += value;
            else
                currentQuality.Value = value;

            return value;
        }

        #region Movement
        /// <summary>
        /// Move this inside of something
        /// </summary>
        /// <param name="container">The container to move into</param>
        /// <returns>was this thing moved?</returns>
        public virtual string TryMoveTo(IContains container)
        {
            return TryMoveTo(new GlobalPosition(container));
        }

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

        internal virtual IGlobalPosition IntruderSlide(IGlobalPosition newPosition)
        {
            //we don't care about containers or bad positions
            if (newPosition.CurrentContainer != null || newPosition.CurrentZone == null)
                return newPosition;

            //bad coordinates
            if(newPosition.CurrentCoordinates == null 
                || !newPosition.CurrentCoordinates.X.IsBetweenOrEqual(0, 99) 
                || !newPosition.CurrentCoordinates.Y.IsBetweenOrEqual(0, 99))
            {
                if (!newPosition.CurrentZone.BaseCoordinates.X.IsBetweenOrEqual(0, 99)
                || !newPosition.CurrentZone.BaseCoordinates.Y.IsBetweenOrEqual(0, 99))
                {
                    //zone has bad base coordinates?
                    newPosition.CurrentCoordinates = new Coordinate(0, 0);
                }
                else
                {
                    newPosition.CurrentCoordinates = newPosition.CurrentZone.BaseCoordinates;
                }

                //recur for more checks
                return IntruderSlide(newPosition);
            }

            //Now we get to slide
            if(newPosition.GetTile().TopContents() != null)
            {
                if(newPosition.CurrentCoordinates.X > 0)
                {
                    newPosition.CurrentCoordinates = new Coordinate((short)(newPosition.CurrentCoordinates.X - 1), newPosition.CurrentCoordinates.Y);
                }
                else if (newPosition.CurrentCoordinates.X < 100)
                {
                    newPosition.CurrentCoordinates = new Coordinate((short)(newPosition.CurrentCoordinates.X + 1), newPosition.CurrentCoordinates.Y);
                }
                else if(newPosition.CurrentCoordinates.Y > 0)
                {
                    newPosition.CurrentCoordinates = new Coordinate(newPosition.CurrentCoordinates.X, (short)(newPosition.CurrentCoordinates.Y - 1));
                }
                else if (newPosition.CurrentCoordinates.X < 100)
                {
                    newPosition.CurrentCoordinates = new Coordinate(newPosition.CurrentCoordinates.X, (short)(newPosition.CurrentCoordinates.Y + 1));
                }
                else
                {
                    //bad base coordinates?
                    newPosition.CurrentCoordinates = new Coordinate(0, 0);
                }

                //recur for more checks
                return IntruderSlide(newPosition);
            }

            return newPosition;
        }
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

        /// <summary>
        /// Save this to the filesystem in Current
        /// </summary>
        /// <returns>Success</returns>
        public virtual bool Save()
        {
            try
            {
                //Dont save player inventories
                if (CurrentLocation.CurrentContainer != null && CurrentLocation.CurrentContainer.ImplementsType<IPlayer>())
                {
                    return true;
                }

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

        #region Generic Rendering
        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<string> GetFullDescription(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                Enumerable.Empty<string>();

            ITemplate dt = Template<ITemplate>();

            return new List<string>() { dt.Description };
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<string> GetImmediateDescription(IEntity viewer)
        {
            return new List<string>() { GetDescribableName(viewer) };
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetDescribableName(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                Enumerable.Empty<string>();

            return TemplateName;
        }
        #endregion

        #region Visual Rendering
        /// <summary>
        /// Gets the actual vision Range taking into account blindness and other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetVisualRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this visible to the viewer
        /// </summary>
        /// <param name="viewer">the viewing entity</param>
        /// <returns>If this is visible</returns>
        public virtual bool IsVisibleTo(IEntity viewer)
        {
            float value = GetCurrentLuminosity();
            ValueRange<float> range = viewer.GetVisualRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<string> RenderToLook(IEntity viewer)
        {
            return GetFullDescription(viewer);
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual IEnumerable<string> RenderToInspect(IEntity viewer)
        {
            //TODO: Make this double power
            return GetFullDescription(viewer);
        }
        #endregion

        #region Containment Rendering
        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<string> RenderAsContents(IEntity viewer)
        {
            return GetImmediateDescription(viewer); ;
        }

        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="holder">entity holding the thing</param>
        /// <returns>the output</returns>
        public virtual IEnumerable<string> RenderAsHeld(IEntity viewer, IEntity holder)
        {
            return GetImmediateDescription(viewer);
        }

        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="wearer">entity wearing the item</param>
        /// <returns>the output</returns>

        public virtual IEnumerable<string> RenderAsWorn(IEntity viewer, IEntity wearer)
        {
            return GetImmediateDescription(viewer);
        }
        #endregion

        /// <summary>
        /// Renders HTML for the info card popups
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output HTML</returns>
        public virtual string RenderToInfo(IEntity viewer)
        {
            if (viewer == null)
            {
                return string.Empty;
            }

            ITemplate dt = Template<ITemplate>();
            StringBuilder sb = new StringBuilder();
            StaffRank rank = viewer.ImplementsType<IPlayer>() ? viewer.Template<IPlayerTemplate>().GamePermissionsRank : StaffRank.Player;

            sb.Append("<div class='helpItem'>");

            sb.AppendFormat("<h3><span style='color: {2}'>{1}</span> {0}</h3>", GetDescribableName(viewer), dt.AsciiCharacter, dt.HexColorCode);
            sb.Append("<hr />");
            if (Qualities != null && Qualities.Count > 0)
            {
                sb.Append("<h4>Qualities</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", Qualities.Select(q => string.Format("({0}:{1})", q.Name, q.Value))));
            }

            if (dt.Interactions.Count > 0)
            {
                sb.Append("<h4>Actions</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", dt.Interactions.Select(i => i.Name)));
            }

            if (rank > StaffRank.Player && dt.DecayEvents.Count > 0)
            {
                sb.Append("<h4>Timed Events</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", dt.DecayEvents.Select(i => i.Name)));
            }

            sb.Append("</div>");

            return sb.ToString();
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public abstract float GetCurrentLuminosity();

        #region Processes
        internal virtual void KickoffProcesses()
        {
            Processor.StartSubscriptionLoop("Decay", () => ProcessDecayStart(), 1 * 60, false);
        }

        private bool ProcessDecayStart()
        {
            //We check once per day to see if any dormant decay events need to start
            foreach (IDecayEvent decayEvent in DecayEvents)
            {
                decayEvent.CurrentTime--;

                //fire event
                if (decayEvent.CurrentTime <= 0)
                {
                    decayEvent.Invoke(this, CurrentLocation.GetTile(), null);
                    decayEvent.CurrentTime = decayEvent.Timer;
                }
            }

            Save();

            return true;
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
                        return -1;

                    if (other.BirthMark.Equals(BirthMark))
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
            // var movementCommand = NetMud.Interp.Interpret.

            return true;
        }
        #endregion

        public abstract object Clone();
    }
}
