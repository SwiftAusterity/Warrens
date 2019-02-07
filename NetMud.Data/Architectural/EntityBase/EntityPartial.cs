using NetMud.Communication;
using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural.Serialization;
using NetMud.Data.Linguistic;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
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
        /// List of live qualities of this entity
        /// </summary>
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
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<ISensoryEvent> Descriptives { get; set; }

        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        public virtual ISensoryEvent RenderToTrack(IEntity actor)
        {
            //Default for "tracking" is null
            return null;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes = null)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            //Self becomes the first sense in the list
            ISensoryEvent self = null;
            foreach (MessagingType sense in sensoryTypes)
            {
                switch (sense)
                {
                    case MessagingType.Audible:
                        if (!IsAudibleTo(viewer))
                        {
                            continue;
                        }

                        if (self == null)
                        {
                            self = GetSelf(sense);
                        }

                        self.TryModify(GetAudibleDescriptives(viewer));
                        break;
                    case MessagingType.Olefactory:
                        if (!IsSmellableTo(viewer))
                        {
                            continue;
                        }

                        if (self == null)
                        {
                            self = GetSelf(sense);
                        }

                        self.TryModify(GetSmellableDescriptives(viewer));
                        break;
                    case MessagingType.Psychic:
                        if (!IsSensibleTo(viewer))
                        {
                            continue;
                        }

                        if (self == null)
                        {
                            self = GetSelf(sense);
                        }

                        self.TryModify(GetPsychicDescriptives(viewer));
                        break;
                    case MessagingType.Tactile:
                        if (!IsTouchableTo(viewer))
                        {
                            continue;
                        }

                        if (self == null)
                        {
                            self = GetSelf(sense);
                        }

                        self.TryModify(GetTouchDescriptives(viewer));
                        break;
                    case MessagingType.Taste:
                        if (!IsTastableTo(viewer))
                        {
                            continue;
                        }

                        if (self == null)
                        {
                            self = GetSelf(sense);
                        }

                        self.TryModify(GetTasteDescriptives(viewer));
                        break;
                    case MessagingType.Visible:
                        if (!IsVisibleTo(viewer))
                        {
                            continue;
                        }

                        if (self == null)
                        {
                            self = GetSelf(sense);
                        }

                        self.TryModify(GetVisibleDescriptives(viewer));
                        break;
                }
            }

            return self;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent GetImmediateDescription(IEntity viewer, MessagingType sense)
        {
            ISensoryEvent me = GetSelf(sense);
            switch (sense)
            {
                case MessagingType.Audible:
                    if (!IsAudibleTo(viewer))
                    {
                        return new SensoryEvent(sense);
                    }

                    me.TryModify(GetAudibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Olefactory:
                    if (!IsSmellableTo(viewer))
                    {
                        return new SensoryEvent(sense);
                    }

                    me.TryModify(GetSmellableDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Psychic:
                    if (!IsSensibleTo(viewer))
                    {
                        return new SensoryEvent(sense);
                    }

                    me.TryModify(GetPsychicDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Tactile:
                    if (!IsTouchableTo(viewer))
                    {
                        return new SensoryEvent(sense);
                    }

                    me.TryModify(GetTouchDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Taste:
                    if (!IsTastableTo(viewer))
                    {
                        return new SensoryEvent(sense);
                    }

                    me.TryModify(GetTasteDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Visible:
                    if (!IsVisibleTo(viewer))
                    {
                        return new SensoryEvent(sense);
                    }

                    me.TryModify(GetVisibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
            }

            return me;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetDescribableName(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
            {
                return string.Empty;
            }

            return GetSelf(MessagingType.Visible).ToString();
        }

        internal ISensoryEvent GetSelf(MessagingType type, int strength = 100)
        {
            return new SensoryEvent()
            {
                SensoryType = type,
                Strength = strength,
                Event = new Lexica() { Phrase = TemplateName, Type = LexicalType.ProperNoun, Role = GrammaticalType.Subject }
            };
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
        public virtual ISensoryEvent RenderToLook(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
            {
                return new SensoryEvent(MessagingType.Visible);
            }

            return GetFullDescription(viewer);
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual ISensoryEvent RenderToScan(IEntity viewer)
        {
            //TODO: Make this half power
            if (!IsVisibleTo(viewer))
            {
                return new SensoryEvent(MessagingType.Visible);
            }

            return GetImmediateDescription(viewer, MessagingType.Visible);
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual ISensoryEvent RenderToInspect(IEntity viewer)
        {
            //TODO: Make this double power
            if (!IsVisibleTo(viewer))
            {
                return new SensoryEvent(MessagingType.Visible);
            }

            return GetFullDescription(viewer);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetVisibleDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Visible);
        }
        #endregion

        #region Auditory Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetAuditoryRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsAudibleTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetAuditoryRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent RenderToAudible(IEntity viewer)
        {
            if (!IsAudibleTo(viewer))
            {
                return new SensoryEvent(MessagingType.Audible);
            }

            ISensoryEvent self = GetSelf(MessagingType.Audible);

            foreach (ISensoryEvent descriptive in GetAudibleDescriptives(viewer))
            {
                self.Event.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetAudibleDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Audible);
        }
        #endregion

        #region Psychic (sense) Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetPsychicRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsSensibleTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetPsychicRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent RenderToSense(IEntity viewer)
        {
            if (!IsSensibleTo(viewer))
            {
                return new SensoryEvent(MessagingType.Psychic);
            }

            ISensoryEvent self = GetSelf(MessagingType.Psychic);

            foreach (ISensoryEvent descriptive in GetPsychicDescriptives(viewer))
            {
                self.Event.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetPsychicDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Psychic);
        }
        #endregion

        #region Taste Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetTasteRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsTastableTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetTasteRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent RenderToTaste(IEntity viewer)
        {
            if (!IsTastableTo(viewer))
            {
                return new SensoryEvent(MessagingType.Taste);
            }

            ISensoryEvent self = GetSelf(MessagingType.Taste);

            foreach (ISensoryEvent descriptive in GetTasteDescriptives(viewer))
            {
                self.Event.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetTasteDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Taste);
        }
        #endregion

        #region Smell Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetOlefactoryRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsSmellableTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetOlefactoryRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent RenderToSmell(IEntity viewer)
        {
            if (!IsSmellableTo(viewer))
            {
                return new SensoryEvent(MessagingType.Olefactory);
            }

            ISensoryEvent self = GetSelf(MessagingType.Olefactory);

            foreach (ISensoryEvent descriptive in GetSmellableDescriptives(viewer))
            {
                self.Event.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetSmellableDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Olefactory);
        }
        #endregion

        #region Touch Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetTactileRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsTouchableTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetTactileRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent RenderToTouch(IEntity viewer)
        {
            if (!IsTouchableTo(viewer))
            {
                return new SensoryEvent(MessagingType.Tactile);
            }

            ISensoryEvent self = GetSelf(MessagingType.Tactile);

            foreach (ISensoryEvent descriptive in GetTouchDescriptives(viewer))
            {
                self.Event.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetTouchDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Tactile);
        }
        #endregion

        #region Containment Rendering
        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent RenderAsContents(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            //Add the existential modifiers
            ISensoryEvent me = GetImmediateDescription(viewer, sensoryTypes[0]);
            me.TryModify(LexicalType.Conjunction, GrammaticalType.Verb, "is")
                .TryModify(LexicalType.Noun, GrammaticalType.DirectObject, "here");

            return me;
        }

        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="holder">entity holding the thing</param>
        /// <returns>the output</returns>
        public virtual ISensoryEvent RenderAsHeld(IEntity viewer, IEntity holder)
        {
            return GetImmediateDescription(viewer, MessagingType.Visible);
        }

        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="wearer">entity wearing the item</param>
        /// <returns>the output</returns>

        public virtual ISensoryEvent RenderAsWorn(IEntity viewer, IEntity wearer)
        {
            return new SensoryEvent()
            {
                SensoryType = MessagingType.Visible,
                Strength = 30,
                Event = new Lexica() { Phrase = TemplateName, Type = LexicalType.Noun, Role = GrammaticalType.Subject }
            };
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

            sb.AppendFormat("<h3>{0}</h3>", GetDescribableName(viewer));
            sb.Append("<hr />");
            if (Qualities != null && Qualities.Count > 0)
            {
                sb.Append("<h4>Qualities</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", Qualities.Select(q => string.Format("({0}:{1})", q.Name, q.Value))));
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

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public abstract Dimensions GetModelDimensions();

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public virtual float GetModelVolume()
        {
            Dimensions dimensions = GetModelDimensions();

            return Math.Max(1, dimensions.Height) * Math.Max(1, dimensions.Width) * Math.Max(1, dimensions.Length);
        }
    }
}
