using NetMud.Cartography;
using NetMud.Communication.Messaging;
using NetMud.Data.DataIntegrity;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Portals between locations
    /// </summary>
    [Serializable]
    public class Pathway : EntityPartial, IPathway
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string DataTemplateName
        {
            get
            {
                return DataTemplate<IPathwayData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T DataTemplate<T>()
        {
            return (T)BackingDataCache.Get(new BackingDataCacheKey(typeof(IPathwayData), DataTemplateId));
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
        public Pathway(IPathwayData backingStore)
        {
            Enter = new MessageCluster();
            DataTemplateId = backingStore.Id;
            MovementDirection = Utilities.TranslateToDirection(backingStore.DegreesFromNorth, backingStore.InclineGrade);
            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
        }

        #region spawning
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<Pathway>(DataTemplateId);

            //Isn't in the world currently
            if (me == default(Pathway))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Keywords = me.Keywords;
                Birthdate = me.Birthdate;
                CurrentLocation = me.CurrentLocation;
                DataTemplateId = me.DataTemplate<IPathwayData>().Id;
                Origin = me.Origin;
                Destination = me.Destination;
                Enter = me.Enter;
                MovementDirection = me.MovementDirection;
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
            var bS = DataTemplate<IPathwayData>() ?? throw new InvalidOperationException("Missing backing data store on pathway spawn event.");

            Keywords = new string[] { bS.Name.ToLower(), MovementDirection.ToString().ToLower() };

            if (String.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            MovementDirection = Utilities.TranslateToDirection(bS.DegreesFromNorth, bS.InclineGrade);

            //paths need two locations
            Origin = bS.Origin.GetLiveInstance();
            Destination = bS.Destination.GetLiveInstance();

            CurrentLocation = Origin.CurrentLocation;

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
        public override IEnumerable<string> RenderToLook(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return Enumerable.Empty<string>();

            var sb = new List<string>();
            var bS = DataTemplate<IPathwayData>();

            if (bS.Descriptives.Any())
            {
                foreach(var desc in bS.Descriptives)
                {
                    sb.Add(desc.Event.ToString());
                }
            }
            else
            {
                //Fallback to using names
                if (MovementDirection == MovementDirectionType.None)
                    sb.Add(string.Format("{0} leads from {2} to {3}.", DataTemplateName, MovementDirection.ToString(),
                        Origin.DataTemplateName, Destination.DataTemplateName));
                else
                    sb.Add(string.Format("{0} heads in the direction of {1} from {2} to {3}.", DataTemplateName, MovementDirection.ToString(),
                        Origin.DataTemplateName, Destination.DataTemplateName));
            }

            return sb;
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IPathway GetLiveInstance()
        {
            return this;
        }
    }
}
