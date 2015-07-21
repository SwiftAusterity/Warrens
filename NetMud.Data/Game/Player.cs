using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetMud.Data.Game
{
    public class Player : EntityPartial, IPlayer
    {
        public Player(ICharacter character)
        {
            Inventory = new EntityContainer<IObject>();
            DataTemplate = character;
            GetFromWorldOrSpawn();
        }

        public string DescriptorID { get; set; }
        public DescriptorType Descriptor { get; set; }

        #region Rendering
        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();
            var ch = (ICharacter)DataTemplate;

            sb.Add(string.Format("This is {0}", ch.FullName()));

            return sb;
        }
        #endregion

        #region Container
        public EntityContainer<IObject> Inventory { get; set; }

        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IObject)))
                return GetContents<T>("objects");

            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> GetContents<T>(string containerName)
        {
            switch (containerName)
            {
                case "objects":
                    return Inventory.EntitiesContained.Select(ent => (T)ent);
            }

            return Enumerable.Empty<T>();
        }

        public string MoveTo<T>(T thing)
        {
            return MoveTo<T>(thing, String.Empty);
        }

        public string MoveTo<T>(T thing, string containerName)
        {
            if (typeof(T).GetInterfaces().Contains(typeof(IObject)))
            {
                var obj = (IObject)thing;

                if (Inventory.Contains(obj))
                    return "That is already in the container";

                Inventory.Add(obj);
                obj.CurrentLocation = this;
                return String.Empty;
            }

            return "Invalid type to move to container.";
        }

        public string MoveFrom<T>(T thing)
        {
            return MoveFrom<T>(thing, String.Empty);
        }

        public string MoveFrom<T>(T thing, string containerName)
        {
            if (typeof(T).GetInterfaces().Contains(typeof(IObject)))
            {
                var obj = (IObject)thing;

                if (!Inventory.Contains(obj))
                    return "That is not in the container";

                Inventory.Remove(obj);
                obj.CurrentLocation = null;
                return String.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        #region SpawnBehavior
        public void GetFromWorldOrSpawn()
        {
            var liveWorld = new LiveCache();

            //Try to see if they are already there
            var me = liveWorld.Get<Player>(DataTemplate.ID);

            //Isn't in the world currently
            if (me == default(IPlayer))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Keywords = me.Keywords;
                Birthdate = me.Birthdate;
                CurrentLocation = me.CurrentLocation;
                DataTemplate = me.DataTemplate;
            }
        }

        public override void SpawnNewInWorld()
        {
            var liveWorld = new LiveCache();
            var ch = (ICharacter)DataTemplate;
            var locationAssembly = Assembly.GetAssembly(typeof(ILocation));

            if (ch.LastKnownLocationType == null)
                ch.LastKnownLocationType = typeof(IRoom).Name;

            var lastKnownLocType = locationAssembly.DefinedTypes.FirstOrDefault(tp => tp.Name.Equals(ch.LastKnownLocationType));

            ILocation lastKnownLoc = null;
            if (lastKnownLocType != null && !String.IsNullOrWhiteSpace(ch.LastKnownLocation))
            {
                if (lastKnownLocType.GetInterfaces().Contains(typeof(ISpawnAsSingleton)))
                {
                    long lastKnownLocID = long.Parse(ch.LastKnownLocation);
                    lastKnownLoc = liveWorld.Get<ILocation>(lastKnownLocID, lastKnownLocType);
                }
                else
                {
                    var cacheKey = new LiveCacheKey(lastKnownLocType, ch.LastKnownLocation);
                    lastKnownLoc = liveWorld.Get<ILocation>(cacheKey);
                }
            }
            SpawnNewInWorld(lastKnownLoc);
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            var liveWorld = new LiveCache();
            var ch = (ICharacter)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(ch);
            Keywords = new string[] { ch.Name.ToLower(), ch.SurName.ToLower() };
            Birthdate = DateTime.Now;

            //TODO: Not hardcode the zeroth room
            if (spawnTo == null)
            {
                spawnTo = liveWorld.Get<ILocation>(1, typeof(IRoom));
            }

            CurrentLocation = spawnTo;

            //Set the data context's stuff too so we don't have to do this over again
            ch.LastKnownLocation = spawnTo.DataTemplate.ID.ToString();
            ch.LastKnownLocationType = spawnTo.GetType().Name;
            ch.Save();

            spawnTo.MoveTo<IPlayer>(this);

            Inventory = new EntityContainer<IObject>();

            liveWorld.Add(this);
        }
        #endregion

        public override byte[] Serialize()
        {
            return new byte[0];
        }

        public override IEntity DeSerialize(byte[] bytes)
        {
            return null;
        }
    }
}
