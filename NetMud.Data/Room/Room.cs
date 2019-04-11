using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Room;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Room
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
        public override string TemplateName
        {
            get
            {
                return Template<IRoomTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IRoomTemplate), TemplateId));
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Room()
        {
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Room(IRoomTemplate room)
        {
            TemplateId = room.Id;

            GetFromWorldOrSpawn();
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
        public override string RenderToVisible(IEntity viewer)
        {
            return GetFullDescription(viewer);
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public override string GetFullDescription(IEntity viewer)
        {
            return Description;
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            IRoom me = LiveCache.Get<IRoom>(TemplateId, typeof(RoomTemplate));

            //Isn't in the world currently
            if (me == default(IRoom))
            {
                SpawnNewInWorld();
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition());
        }


        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            IRoomTemplate bS = Template<IRoomTemplate>() ?? throw new InvalidOperationException("Missing backing data store on room spawn event.");

            Keywords = new string[] { bS.Name.ToLower() };
            Qualities = bS.Qualities;
            Description = bS.Description;

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            UpsertToLiveWorldCache(true);

            CurrentLocation = spawnTo;

            UpsertToLiveWorldCache(true);

            Save();
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
