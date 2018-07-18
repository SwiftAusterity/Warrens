using NetMud.Communication;
using NetMud.Communication.Messaging;
using NetMud.Data.Serialization;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
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
        /// An internal date for checking the last time this was saved
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        internal DateTime CleanUntil { get; set; }

        /// <summary>
        /// The Id for the backing data
        /// </summary>
        public long DataTemplateId { get; set; }

        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public abstract string DataTemplateName { get; }

        /// <summary>
        /// The backing data for this live entity
        /// </summary>
        public abstract T DataTemplate<T>() where T : IKeyedData;

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public abstract Tuple<int, int, int> GetModelDimensions();

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
            get { return _keywords; }
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
            var strings = MessagingUtility.TranslateColorVariables(input.ToArray(), this);

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

        /// <summary>
        /// Where in the live world this is
        /// </summary>
        [JsonConverter(typeof(ConcreteTypeConverter<GlobalPosition>))]
        public IGlobalPosition CurrentLocation { get; set; }

        public virtual IGlobalPosition AbsolutePosition()
        {
            //TODO: Default to emergency location
            if (CurrentLocation == null || CurrentLocation.CurrentLocation == null)
                return null;

            return CurrentLocation;
        }

        #region Affects
        /// <summary>
        /// Affects to add to a live entity when it is spawned
        /// </summary>
        public HashSet<IAffect> Affects { get; set; }

        /// <summary>
        /// Does this data have this affect
        /// </summary>
        /// <param name="affectTarget">the target of the affect</param>
        /// <returns>the affect</returns>
        public bool HasAffect(string affectTarget)
        {
            if (string.IsNullOrWhiteSpace(affectTarget))
                return false;

            return Affects.Any(aff => aff.Target.Equals(affectTarget, StringComparison.InvariantCultureIgnoreCase)
                                        && (aff.Duration > 0 || aff.Duration == -1));
        }

        /// <summary>
        /// Attempts to apply the affect
        /// </summary>
        /// <param name="affectToApply">the affect to apply</param>
        /// <returns>what type of resist happened (or success)</returns>
        public AffectResistType ApplyAffect(IAffect affectToApply)
        {
            if (affectToApply == null)
                throw new ArgumentNullException("Affect to apply can not be null during application.");

            var affect = Affects.FirstOrDefault(aff => aff.Equals(affectToApply));

            //Are we combining affects or not
            if (affect == null)
            {
                //TODO: Resistance roll
                Affects.Add(affectToApply);
            }
            else
            {
                //TODO: Better math
                affect.Duration += affectToApply.Duration;
                affect.Value = Math.Max(affect.Value, affectToApply.Value);
            }

            return AffectResistType.Success;
        }

        /// <summary>
        /// Attempt to dispel the affect
        /// </summary>
        /// <param name="affectTarget">the thing attempting to be dispeled</param>
        /// <param name="dispellationMethod">the dispellation methodology. [TypeOfMethod, strength]</param>
        /// <returns>reisst type</returns>
        public AffectResistType DispelAffect(string affectTarget, int dispellationStrength)
        {
            var returnValue = AffectResistType.Success;

            if (HasAffect(affectTarget))
            {
                //They have the affect so lets try and remove it
                var affects = Affects.Where(aff => aff.Target.Equals(affectTarget, StringComparison.InvariantCultureIgnoreCase));

                foreach (var affect in affects)
                {
                    //TODO: This is kind of a stub, needs more stuff possibly int rolls or luck and such
                    if (dispellationStrength >= affect.DispelResistance)
                        returnValue = AffectResistType.Resisted;
                    else
                        returnValue = AffectResistType.Success;

                    if (returnValue == AffectResistType.Success)
                        affect.Duration = 0;
                }
            }

            return returnValue;
        }
        #endregion

        #region Movement
        /// <summary>
        /// Change the position of this
        /// </summary>
        /// <param name="direction">the 0-360 direction we're moving</param>
        /// <param name="newPosition">The new position the thing is in, will return with the original one if nothing moved</param>
        /// <returns>was this thing moved?</returns>
        public virtual bool TryMoveDirection(int direction, IGlobalPosition newPosition)
        {
            //TODO: Check for directions, trigger movement stuff
            return TryMoveTo(newPosition);
        }

        /// <summary>
        /// Move this inside of something
        /// </summary>
        /// <param name="container">The container to move into</param>
        /// <returns>was this thing moved?</returns>
        public virtual bool TryMoveInto(IContains container)
        {
            return TryMoveTo(new GlobalPosition(container));
        }

        /// <summary>
        /// Change the position of this without physical movement
        /// </summary>
        /// <param name="newPosition">The new position the thing is in, will return with the original one if nothing moved</param>
        /// <returns>was this thing moved?</returns>
        public virtual bool TryTeleport(IGlobalPosition newPosition)
        {
            return TryMoveTo(newPosition);
        }

        internal virtual bool TryMoveTo(IGlobalPosition newPosition)
        {
            //validate position
            if (CurrentLocation?.CurrentLocation != null)
                CurrentLocation.CurrentLocation.MoveFrom(this);

            CurrentLocation = newPosition;

            return true;
        }
        #endregion

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
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

            var now = DateTime.Now;

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
        internal virtual bool Save()
        {
            try
            {
                var dataAccessor = new LiveData();
                dataAccessor.WriteEntity(this);
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
        public HashSet<IOccurrence> Descriptives { get; set; }

        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        public virtual IOccurrence RenderToTrack(IEntity actor)
        {
            //Default for "tracking" is null
            return null;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (!IsVisibleTo(viewer))
                return null;

            if (sensoryTypes == null || sensoryTypes.Count() == 0)
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };

            var self = GetSelf(MessagingType.Visible);

            foreach (var descriptive in GetVisibleDescriptives(viewer))
                self.Event.TryModify(descriptive.Event);

            return self;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence GetImmediateDescription(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (!IsVisibleTo(viewer))
                return null;

            if (sensoryTypes == null || sensoryTypes.Count() == 0)
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };

            return GetSelf(MessagingType.Visible);
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetDescribableName(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return string.Empty;

            return GetSelf(MessagingType.Visible).ToString();
        }

        internal IOccurrence GetSelf(MessagingType type, int strength = 100)
        {
            return new Occurrence()
            {
                SensoryType = type,
                Strength = strength,
                Event = new Lexica() { Phrase = DataTemplateName, Type = LexicalType.Noun, Role = GrammaticalType.Subject }
            };
        }
        #endregion

        #region Visual Rendering
        /// <summary>
        /// Gets the actual vision modifier taking into account blindness and other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public virtual float GetVisionModifier()
        {
            //Base is "infinite" for things like rocks and zones
            return 999999;
        }

        /// <summary>
        /// Is this visible to the viewer
        /// </summary>
        /// <param name="viewer">the viewing entity</param>
        /// <returns>If this is visible</returns>
        public virtual bool IsVisibleTo(IEntity viewer)
        {
            return GetCurrentLuminosity() > viewer.GetVisionModifier();
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence RenderToLook(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return null;

            return GetFullDescription(viewer, new [] { MessagingType.Visible });
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual IOccurrence RenderToScan(IEntity viewer)
        {
            //TODO: Make this half power
            if (!IsVisibleTo(viewer))
                return null;

            return GetImmediateDescription(viewer, new[] { MessagingType.Visible });
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual IOccurrence RenderToInspect(IEntity viewer)
        {
            //TODO: Make this double power
            if (!IsVisibleTo(viewer))
                return null;

            return GetFullDescription(viewer, new[] { MessagingType.Visible });
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<IOccurrence> GetVisibleDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Visible);
        }
        #endregion

        #region Auditory Rendering
        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public virtual float GetAuditoryModifier()
        {
            //Base is "infinite" for things like rocks and zones
            return 999999;
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsAudibleTo(IEntity viewer)
        {
            //TODO: Do detection lowering stuff
            return viewer.GetAuditoryModifier() > 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence RenderToAudible(IEntity viewer)
        {
            if (!IsAudibleTo(viewer))
                return null;

            var self = GetSelf(MessagingType.Audible);

            foreach (var descriptive in GetAudibleDescriptives(viewer))
                self.Event.TryModify(descriptive.Event);

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<IOccurrence> GetAudibleDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Audible);
        }
        #endregion

        #region Psychic (sense) Rendering
        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public virtual float GetPsychicModifier()
        {
            //Base is "infinite" for things like rocks and zones
            return 999999;
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsSensibleTo(IEntity viewer)
        {
            //TODO: Do detection lowering stuff
            return viewer.GetPsychicModifier() > 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence RenderToSense(IEntity viewer)
        {
            if (!IsSensibleTo(viewer))
                return null;

            var self = GetSelf(MessagingType.Psychic);

            foreach (var descriptive in GetPsychicDescriptives(viewer))
                self.Event.TryModify(descriptive.Event);

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<IOccurrence> GetPsychicDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Psychic);
        }
        #endregion

        #region Taste Rendering
        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public virtual float GetTasteModifier()
        {
            //Base is "infinite" for things like rocks and zones
            return 999999;
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsTastableTo(IEntity viewer)
        {
            //TODO: Do detection lowering stuff
            return viewer.GetTasteModifier() > 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence RenderToTaste(IEntity viewer)
        {
            if (!IsTastableTo(viewer))
                return null;

            var self = GetSelf(MessagingType.Taste);

            foreach (var descriptive in GetTasteDescriptives(viewer))
                self.Event.TryModify(descriptive.Event);

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<IOccurrence> GetTasteDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Taste);
        }
        #endregion

        #region Smell Rendering
        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public virtual float GetOlefactoryModifier()
        {
            //Base is "infinite" for things like rocks and zones
            return 999999;
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsSmellableTo(IEntity viewer)
        {
            //TODO: Do detection lowering stuff
            return viewer.GetOlefactoryModifier() > 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence RenderToSmell(IEntity viewer)
        {
            if (!IsSmellableTo(viewer))
                return null;

            var self = GetSelf(MessagingType.Olefactory);

            foreach (var descriptive in GetSmellableDescriptives(viewer))
                self.Event.TryModify(descriptive.Event);

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<IOccurrence> GetSmellableDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Olefactory);
        }
        #endregion

        #region Touch Rendering
        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public virtual float GetTactileModifier()
        {
            //Base is "infinite" for things like rocks and zones
            return 999999;
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsTouchableTo(IEntity viewer)
        {
            //TODO: Do detection lowering stuff
            return viewer.GetTactileModifier() > 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence RenderToTouch(IEntity viewer)
        {
            if (!IsTouchableTo(viewer))
                return null;

            var self = GetSelf(MessagingType.Tactile);

            foreach (var descriptive in GetTouchDescriptives(viewer))
                self.Event.TryModify(descriptive.Event);

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<IOccurrence> GetTouchDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Tactile);
        }
        #endregion

        #region Containment Rendering
        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence RenderAsContents(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if(sensoryTypes == null || sensoryTypes.Count() == 0)
                 sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };

            return GetImmediateDescription(viewer, sensoryTypes);
        }

        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="holder">entity holding the thing</param>
        /// <returns>the output</returns>
        public virtual IOccurrence RenderAsHeld(IEntity viewer, IEntity holder)
        {
            return GetImmediateDescription(viewer, new[] { MessagingType.Visible });
        }

        /// <summary>
        /// Renders output for this entity when it is held by something they are looking at
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="wearer">entity wearing the item</param>
        /// <returns>the output</returns>

        public virtual IOccurrence RenderAsWorn(IEntity viewer, IEntity wearer)
        {
            return new Occurrence()
            {
                SensoryType = MessagingType.Visible,
                Strength = 30,
                Event = new Lexica() { Phrase = DataTemplateName, Type = LexicalType.Noun, Role = GrammaticalType.Subject }
            };
        }
        #endregion  

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public abstract float GetCurrentLuminosity();

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
        #endregion
    }
}
