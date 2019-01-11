using NetMud.Cartography;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Room
{
    /// <summary>
    /// Portals between locations
    /// </summary>
    [Serializable]
    public class Pathway : EntityPartial, IPathway
    {
        public bool IsPlayer()
        {
            return false;
        }

        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<IPathwayTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IPathwayTemplate), TemplateId));
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Movement messages trigger when moved through
        /// </summary>
        public IMessageCluster Enter { get; set; }

        /// <summary>
        /// Cardinality direction this points towards
        /// </summary>
        public MovementDirectionType MovementDirection { get; set; }

        /// <summary>
        /// Birthmark of live location this points into
        /// </summary>
        [JsonProperty("Destination")]
        private LiveCacheKey _currentDestinationBirthmark;

        /// <summary>
        /// Restful live location this points into
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("To Location must be valid.")]
        [Display(Name = "To Room", Description = "The room this leads to.")]
        [DataType(DataType.Text)]
        public ILocation Destination
        {
            get
            {
                if (_currentDestinationBirthmark != null)
                    return (ILocation)LiveCache.Get(_currentDestinationBirthmark);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _currentDestinationBirthmark = new LiveCacheKey(value);
            }
        }

        /// <summary>
        /// Birthmark of live location this points out of
        /// </summary>
        [JsonProperty("Origin")]
        private LiveCacheKey _currentOriginBirthmark;

        /// <summary>
        /// Restful live location this points out of
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("From Location must be valid.")]
        [Display(Name = "From Room", Description = "The room this originates from.")]
        [DataType(DataType.Text)]
        public ILocation Origin
        {
            get
            {
                if (_currentOriginBirthmark != null)
                    return (ILocation)LiveCache.Get(_currentOriginBirthmark);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _currentOriginBirthmark = new LiveCacheKey(value);
            }
        }

        public MovementDirectionType DirectionType { get; set; }

        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North", Description = "The direction on a 360 plane. 360 and 0 are both directional north. 90 is east, 180 is south, 270 is west.")]
        [DataType(DataType.Text)]
        public int DegreesFromNorth { get; set; }

        public int InclineGrade { get; set; }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Pathway()
        {
            Enter = new MessageCluster();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Pathway(IPathwayTemplate backingStore)
        {
            Enter = new MessageCluster();
            TemplateId = backingStore.Id;
            MovementDirection = Utilities.TranslateToDirection(backingStore.DegreesFromNorth, backingStore.InclineGrade);
            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;

            //TODO: ???

            return lumins;
        }

        #region spawning
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<Pathway>(TemplateId);

            //Isn't in the world currently
            if (me == default(Pathway))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Keywords = me.Keywords;
                Birthdate = me.Birthdate;
                CurrentLocation = me.CurrentLocation;
                TemplateId = me.Template<IPathwayTemplate>().Id;
                Origin = me.Origin;
                Destination = me.Destination;
                Enter = me.Enter;
                MovementDirection = me.MovementDirection;
                Model = me.Model;
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(null);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition position)
        {
            //We can't even try this until we know if the data is there
            var bS = Template<IPathwayTemplate>() ?? throw new InvalidOperationException("Missing backing data store on pathway spawn event.");

            Keywords = new string[] { bS.Name.ToLower(), MovementDirection.ToString().ToLower() };

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            MovementDirection = Utilities.TranslateToDirection(bS.DegreesFromNorth, bS.InclineGrade);

            //paths need two locations
            Origin = bS.Origin.GetLiveInstance();
            Destination = bS.Destination.GetLiveInstance();

            CurrentLocation = Origin.CurrentLocation;
            Model = bS.Model;

            //Enter = new MessageCluster(new string[] { bS.MessageToActor }, new string[] { "$A$ enters you" }, new string[] { }, new string[] { bS.MessageToOrigin }, new string[] { bS.MessageToDestination });
            //Enter.ToSurrounding.Add(MessagingType.Visible, new Tuple<int, IEnumerable<string>>(bS.VisibleStrength, new string[] { bS.VisibleToSurroundings }));
            //Enter.ToSurrounding.Add(MessagingType.Audible, new Tuple<int, IEnumerable<string>>(bS.AudibleStrength, new string[] { bS.AudibleToSurroundings }));

            UpsertToLiveWorldCache(true);
        }
        #endregion

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IOccurrence RenderToLook(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return null;

            var bS = Template<IPathwayTemplate>();
            var me = GetSelf(MessagingType.Visible);

            if (bS.Descriptives.Any())
            {
                foreach (var desc in bS.Descriptives)
                    me.Event.TryModify(desc.Event);
            }
            else
            {
                var verb = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "leads");

                //Fallback to using names
                if (MovementDirection == MovementDirectionType.None)
                {
                    var origin = new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, Origin.TemplateName);
                    origin.TryModify(new Lexica(LexicalType.Noun, GrammaticalType.IndirectObject, Destination.TemplateName));
                    verb.TryModify(origin);
                }
                else
                {
                    var direction = new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, MovementDirection.ToString());
                    var origin = new Lexica(LexicalType.Noun, GrammaticalType.IndirectObject, Origin.TemplateName);
                    origin.TryModify(new Lexica(LexicalType.Noun, GrammaticalType.IndirectObject, Destination.TemplateName));
                    direction.TryModify(origin);
                }

                me.Event.TryModify(verb);
            }

            return me;
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IPathway GetLiveInstance()
        {
            return this;
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
