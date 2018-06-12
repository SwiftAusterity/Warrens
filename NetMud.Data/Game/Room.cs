using NetMud.Data.DataIntegrity;
using NetMud.Data.EntityBackingData;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Places entities are (most of the time)
    /// </summary>
    [Serializable]
    public class Room : LocationEntityPartial, IRoom
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
                return DataTemplate<IRoomData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T DataTemplate<T>()
        {
            return (T)BackingDataCache.Get(new BackingDataCacheKey(typeof(IRoomData), DataTemplateId));
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        [JsonProperty("ParentLocation")]
        private LiveCacheKey _parentLocation { get; set; }

        /// <summary>
        /// The locale this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Rooms must have a zone affiliation.")]
        public ILocale ParentLocation
        {
            get
            {
                return LiveCache.Get<ILocale>(_parentLocation);
            }
            set
            {
                if (value != null)
                    _parentLocation = new LiveCacheKey(value);
            }
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Room()
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Room(IRoomData room)
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();

            DataTemplateId = room.Id;

            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Gets the remaining distance and next "step" to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) and the next path you'd have to use</returns>
        public Tuple<int, IPathway> GetDistanceAndNextStepDestination(ILocation destination)
        {
            var distance = -1;
            IPathway nextStep = null;

            return new Tuple<int, IPathway>(distance, nextStep);
        }


        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IRoom GetLiveInstance()
        {
            return this;
        }

        #region rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return Enumerable.Empty<string>();

            var sb = new List<string>
            {
                GetFullShortDescription(viewer)
            };

            if (NaturalResources != null)
                sb.AddRange(NaturalResources.Select(kvp => kvp.Key.RenderResourceCollection(viewer, kvp.Value)));

            sb.AddRange(GetPathways().SelectMany(path => path.RenderAsContents(viewer)));
            sb.AddRange(GetContents<IInanimate>().SelectMany(path => path.RenderAsContents(viewer)));
            sb.AddRange(GetContents<IMobile>().Where(player => !player.Equals(viewer)).SelectMany(path => path.RenderAsContents(viewer)));

            return sb;
        }

        /// <summary>
        /// Renders out an ascii map of this room plus all rooms in the radius
        /// </summary>
        /// <param name="radius">how far away to render</param>
        /// <returns>the string</returns>
        public string RenderCenteredMap(int radius, bool visibleOnly)
        {
            var sb = new StringBuilder();

            //TODO: This

            return sb.ToString();
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<IRoom>(DataTemplateId, typeof(RoomData));

            //Isn't in the world currently
            if (me == default(IRoom))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplateId;
                Contents = me.Contents;
                MobilesInside = me.MobilesInside;
                Keywords = me.Keywords;
                NaturalResources = me.NaturalResources;
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition(this));
        }


        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            var bS = DataTemplate<IRoomData>() ?? throw new InvalidOperationException("Missing backing data store on room spawn event.");

            Keywords = new string[] { bS.Name.ToLower() };

            if (NaturalResources == null)
                NaturalResources = new Dictionary<INaturalResource, int>();

            if (String.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            CurrentLocation = spawnTo;

            UpsertToLiveWorldCache();
        }
        #endregion
    }
}
