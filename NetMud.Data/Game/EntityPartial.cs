using NetMud.Communication;
using NetMud.Communication.Messaging;
using NetMud.Data.Serialization;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.Behaviors.Existential;
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
        /// The name of the object in the data template
        /// </summary>
        public virtual string DataTemplateName
        {
            get
            {
                return DataTemplate<IData>().Name;
            }
        }

        /// <summary>
        /// The backing data for this live entity
        /// </summary>
        public virtual T DataTemplate<T>() where T : IData
        {
            return BackingDataCache.Get<T>(DataTemplateId);
        }

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
            if(String.IsNullOrWhiteSpace(affectTarget))
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
            if(CurrentLocation?.CurrentLocation != null)
                CurrentLocation.CurrentLocation.MoveFrom(this);

            CurrentLocation = newPosition;

            return true;
        }

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
        public bool Equals(IEntity other)
        {
            if (other != default(IEntity))
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
        #endregion
    }
}
