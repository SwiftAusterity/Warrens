using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Game
{
    public class Object : IObject
    {
        public Object()
        {
            //IDatas need parameterless constructors
            Contents = new EntityContainer<IObject>();
        }

        public Object(IObjectData backingStore)
        {
            Contents = new EntityContainer<IObject>();
            DataTemplate = backingStore;
            GetFromWorldOrSpawn();
        }

        public Object(IObjectData backingStore, ILocation spawnTo)
        {
            Contents = new EntityContainer<IObject>();
            DataTemplate = backingStore;
            GetFromWorldOrSpawn(spawnTo);
        }

        public string BirthMark { get; private set; }
        public DateTime Birthdate { get; private set; }
        public string[] Keywords { get; set; }

        public IData DataTemplate { get; private set; }

        public ILocation CurrentLocation { get; set; }
        public long LastKnownLocation { get; set; }
        public string LastKnownLocationType { get; set; }

        #region Container
        public EntityContainer<IObject> Contents { get; set; }

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
                    return Contents.EntitiesContained.Select(ent => (T)ent);
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

                if (Contents.Contains(obj))
                    return "That is already in the container";

                Contents.Add(obj);
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

                if (!Contents.Contains(obj))
                    return "That is not in the container";

                Contents.Remove(obj);
                return String.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        public void GetFromWorldOrSpawn()
        {
            var liveWorld = new LiveCache();

            //Try to see if they are already there
            var me = liveWorld.Get<IObject>(DataTemplate.ID, typeof(IObject));

            //Isn't in the world currently
            if (me == default(IObject))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Keywords = me.Keywords;
                Birthdate = me.Birthdate;
                CurrentLocation = me.CurrentLocation;
                DataTemplate = me.DataTemplate;
                CurrentLocation.MoveTo<IObject>(this);
            }
        }

        public void GetFromWorldOrSpawn(ILocation spawnTo)
        {
            var liveWorld = new LiveCache();

            //Try to see if they are already there
            var me = liveWorld.Get<IObject>(DataTemplate.ID, typeof(IObject));

            //Isn't in the world currently
            if (me == default(IObject))
                SpawnNewInWorld(spawnTo);
            else
            {
                BirthMark = me.BirthMark;
                Keywords = me.Keywords;
                Birthdate = me.Birthdate;
                CurrentLocation = me.CurrentLocation;
                DataTemplate = me.DataTemplate;
                CurrentLocation.MoveTo<IObject>(this);
            }
        }

        public void SpawnNewInWorld()
        {
            throw new NotImplementedException("Objects can't spawn to nothing");
        }

        public void SpawnNewInWorld(ILocation spawnTo)
        {
            var liveWorld = new LiveCache();
            var backingStore = (IObjectData)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(backingStore);
            Keywords = new string[] { backingStore.Name.ToLower() };
            Birthdate = DateTime.Now;

            //TODO: People get a base spawn but live objects need to be spawnable to a specific location or not at all really
            if (spawnTo == null)
            {
                throw new NotImplementedException("Objects can't spawn to nothing");
            }

            CurrentLocation = spawnTo;

            spawnTo.MoveTo<IObject>(this);

            Contents = new EntityContainer<IObject>();

            liveWorld.Add<IObject>(this);
        }

        public IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();
            var backingStore = (IObjectData)DataTemplate;

            sb.Add(string.Format("There is a {0} here", backingStore.Name));

            return sb;
        }

        public int CompareTo(IEntity other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != typeof(Object))
                        return -1;

                    if (other.BirthMark.Equals(this.BirthMark))
                        return 1;

                    return 0;
                }
                catch
                {
                    //Minor error logging
                }
            }

            return -99;
        }

        public bool Equals(IEntity other)
        {
            if (other != default(IEntity))
            {
                try
                {
                    return other.GetType() == typeof(Object) && other.BirthMark.Equals(this.BirthMark);
                }
                catch
                {
                    //Minor error logging
                }
            }

            return false;
        }
    }
}
