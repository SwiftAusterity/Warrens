using NetMud.Cartography;
using NetMud.Communication.Messaging;
using NetMud.Data.DataIntegrity;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
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
        public override string DataTemplateName
        {
            get
            {
                if (DataTemplate<IPathwayData>() == null)
                    return String.Empty;

                return DataTemplate<IPathwayData>().Name;
            }
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
        public MovementDirectionType MovementDirection { get; private set; }

        /// <summary>
        /// Birthmark of live location this points into
        /// </summary>
        [JsonProperty("ToLocation")]
        private string _currentToLocationBirthmark;

        /// <summary>
        /// Restful live location this points into
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("From Location must be valid.")]
        public IRoom ToLocation
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_currentToLocationBirthmark))
                    return LiveCache.Get<IRoom>(new LiveCacheKey(typeof(IRoom), _currentToLocationBirthmark));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _currentToLocationBirthmark = value.BirthMark;
                UpsertToLiveWorldCache();
            }
        }

        /// <summary>
        /// Birthmark of live location this points out of
        /// </summary>
        [JsonProperty("FromLocation")]
        private string _currentFromLocationBirthmark;

        /// <summary>
        /// Restful live location this points out of
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("From Location must be valid.")]
        public IRoom FromLocation
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_currentFromLocationBirthmark))
                    return LiveCache.Get<IRoom>(new LiveCacheKey(typeof(IRoom), _currentFromLocationBirthmark));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _currentFromLocationBirthmark = value.BirthMark;
                UpsertToLiveWorldCache();
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
            DataTemplateId = backingStore.ID;
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
        /// Tries to find this entity in the world based on its ID or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<Pathway>(DataTemplateId);

            //Isn't in the world currently
            if (me == default(IPathway))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Keywords = me.Keywords;
                Birthdate = me.Birthdate;
                CurrentLocation = me.CurrentLocation;
                DataTemplateId = me.DataTemplate<IPathwayData>().ID;
                FromLocation = me.FromLocation;
                ToLocation = me.ToLocation;
                Enter = me.Enter;
                MovementDirection = me.MovementDirection;
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            var bS = DataTemplate<IPathwayData>(); ;

            SpawnNewInWorld(null);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition position)
        {
            var bS = DataTemplate<IPathwayData>(); ;
            var locationAssembly = Assembly.GetAssembly(typeof(Room));

            MovementDirection = Utilities.TranslateToDirection(bS.DegreesFromNorth, bS.InclineGrade);

            BirthMark = LiveCache.GetUniqueIdentifier(bS);
            Keywords = new string[] { bS.Name.ToLower(), MovementDirection.ToString().ToLower() };
            Birthdate = DateTime.Now;

            //paths need two locations
            FromLocation = LiveCache.Get<IRoom>(bS.FromLocation.ID);
            ToLocation = LiveCache.Get<IRoom>(bS.ToLocation.ID);

            CurrentLocation = FromLocation.CurrentLocation; 

            //Enter = new MessageCluster(new string[] { bS.MessageToActor }, new string[] { "$A$ enters you" }, new string[] { }, new string[] { bS.MessageToOrigin }, new string[] { bS.MessageToDestination });
            //Enter.ToSurrounding.Add(MessagingType.Visible, new Tuple<int, IEnumerable<string>>(bS.VisibleStrength, new string[] { bS.VisibleToSurroundings }));
            //Enter.ToSurrounding.Add(MessagingType.Audible, new Tuple<int, IEnumerable<string>>(bS.AudibleStrength, new string[] { bS.AudibleToSurroundings }));

            FromLocation.MoveInto<IPathway>(this);
        }
        #endregion

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>();
            var bS = DataTemplate<IPathwayData>(); ;

            sb.Add(string.Format("{0} heads in the direction of {1} from {2} to {3}", bS.Name, MovementDirection.ToString(), FromLocation.DataTemplate<IRoomData>().Name, ToLocation.DataTemplate<IRoomData>().Name));

            return sb;
        }
    }
}
