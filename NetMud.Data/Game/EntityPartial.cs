using NetMud.Communication;
using NetMud.Communication.Messaging;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;
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

        #region "World Positioning"
        [ScriptIgnore]
        [JsonIgnore]
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
        /// What this is inside of if it is inside of something
        /// </summary>
        [JsonProperty("InsideOf")]
        private string _currentLocationBirthmark;

        /// <summary>
        /// What this is inside of if it is inside of something
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public virtual IContains InsideOf
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
        /// x,y,z position in the world
        /// </summary>
        public Tuple<long, long, long> Position { get; private set; }

        /// <summary>
        /// What direction is this facing, 0/360 = north
        /// </summary>
        public int Facing { get; private set; }

        /// <summary>
        /// Handles returning container's position if inside of something
        /// </summary>
        /// <returns>positional coordinates</returns>
        public long[, ,] AbsolutePosition()
        {
            var returnValue = Position;
            var container = InsideOf;

            short recursionPrevention = 0;
            while(container != null && recursionPrevention < 10)
            {
                returnValue = container.Position;
                container = container.InsideOf;
                recursionPrevention++;
            }

            return returnValue;
        }

        /// <summary>
        /// Change the position of this
        /// </summary>
        /// <param name="direction">the 0-360 direction we're moving</param>
        /// <param name="incline">-90 to 90 incline are we moving up or down as well? Terrain will take care of natural incline changes</param>
        /// <param name="distance">how far are we moving</param>
        /// <param name="changeFacing">should the thing's facing rotate towards the direction?</param>
        /// <returns>was this thing moved?</returns>
        public bool Move(int direction, int incline, int distance, bool changeFacing = false)
        {
            //We're assuming messaging happens in what asked us to move
            var x = Position.Item1;
            var y = Position.Item2;
            var z = Position.Item3;

            //TODO: Get world geometry to see what the floor incline rate is

            var xyChanges = NetMud.Cartography.Utilities.TranslateToDirection(direction, distance);
            x += xyChanges.Item1;
            y += xyChanges.Item2;
            z += (incline / 90) * distance;
            
            //TODO: Get world geometry to see if you can even move here, otherwise move to up till you can't anymore

            //TODO: Triggers

            Position = new Tuple<long, long, long>(x, y, z);

            if(changeFacing)
                Facing = new Tuple<int, int>(direction, incline);

            //TODO: Falling
        }

        /// <summary>
        /// Reposition entirely without moving
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="z">z coordinate</param>
        /// <param name="facing">where the thing should be facing in the end</param>
        /// <returns>success</returns>
        bool Reposition(long x, long y, long z, Tuple<int, int> facing)
        {
            //We're assuming messaging happens in what asked us to move

            //TODO: Get world geometry to see if you can even move here, otherwise move to up till you can't anymore

            //TODO: Triggers
            Position = new Tuple<long, long, long>(x, y, z);
            Facing = facing;

            //TODO: Falling
        }
        #endregion

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
            if(affect == null)
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
        public AffectResistType DispelAffect(string affectTarget, Tuple<AffectType, int> dispellationMethod)
        {
            var returnValue = AffectResistType.Success;

            if (HasAffect(affectTarget))
            {
                //They have the affect so lets try and remove it
                var affects = Affects.Where(aff => aff.Target.Equals(affectTarget, StringComparison.InvariantCultureIgnoreCase));

                foreach (var affect in affects)
                {
                    switch (affect.Type)
                    {
                        case AffectType.Pure:
                            returnValue = AffectResistType.Immune;
                            break;
                        default:
                            //TODO: This is kind of a stub, needs more stuff possibly int rolls or luck and such
                            if (dispellationMethod.Item1 == affect.Type && dispellationMethod.Item2 < affect.Value * affect.Duration)
                                returnValue = AffectResistType.Resisted;
                            else
                                returnValue = AffectResistType.Success;

                            break;
                    }

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
        /// Spawn a new instance of this entity into the live world in a set position
        /// </summary>
        /// <param name="position">x,y,z coordinates to spawn into</param>
        public abstract void SpawnNewInWorld(long[, ,] position);

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
    }
}
