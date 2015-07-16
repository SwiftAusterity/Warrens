using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Game
{
    public class Room : IRoom
    {
        //DataFields

        public long ID { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastRevised { get; set; }
        public string Title { get; set; }

        public Room()
        {
            ObjectsInRoom = new EntityContainer<IObject>();
            MobilesInRoom = new EntityContainer<IMobile>();
        }

        public Room(IRoom room)
        {
            ObjectsInRoom = new EntityContainer<IObject>();
            MobilesInRoom = new EntityContainer<IMobile>();

            //Yes it's its own datatemplate and currentLocation
            DataTemplate = room;

            ID = room.ID;
            Created = room.Created;
            LastRevised = room.LastRevised;
            Title = room.Title;

            CurrentLocation = room;

            GetFromWorldOrSpawn();
        }

        public string BirthMark { get; private set; }

        public DateTime Birthdate { get; private set; }

        public string[] Keywords { get; set; }

        public IData DataTemplate { get; private set; }

        public ILocation CurrentLocation { get; set; }

        #region Container
        public EntityContainer<IObject> ObjectsInRoom { get; set; }
        public EntityContainer<IMobile> MobilesInRoom { get; set; }

        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IEntity)))
                return GetContents<T>("mobiles").Concat(GetContents<T>("objects"));
            else if (implimentedTypes.Contains(typeof(IMobile)))
                return GetContents<T>("mobiles");
            else if (implimentedTypes.Contains(typeof(IObject)))
                return GetContents<T>("objects");

            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> GetContents<T>(string containerName)
        {
            switch (containerName)
            {
                case "mobiles":
                    return MobilesInRoom.EntitiesContained.Select(ent => (T)ent);
                case "objects":
                    return ObjectsInRoom.EntitiesContained.Select(ent => (T)ent);
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

                if (ObjectsInRoom.Contains(obj))
                    return "That is already in the container";

                ObjectsInRoom.Add(obj);
                return String.Empty;
            }
            else if (typeof(T).GetInterfaces().Contains(typeof(IMobile)))
            {
                var mob = (IMobile)thing;

                if (MobilesInRoom.Contains(mob))
                    return "That is already in the container";

                MobilesInRoom.Add(mob);
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

                if (!ObjectsInRoom.Contains(obj))
                    return "That is not in the container";

                ObjectsInRoom.Remove(obj);
                return String.Empty;
            }
            if (typeof(T).GetInterfaces().Contains(typeof(IMobile)))
            {
                var mob = (IMobile)thing;

                if (!MobilesInRoom.Contains(mob))
                    return "That is not in the container";

                MobilesInRoom.Remove(mob);
                return String.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        public IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();

            sb.Add(string.Format("<span style=\"color: orange\">{0}</span>", Title));
            sb.Add(string.Empty.PadLeft(Title.Length, '-'));

            return sb;
        }

        public void GetFromWorldOrSpawn()
        {
            var liveWorld = new LiveCache();

            //Try to see if they are already there
            var me = liveWorld.Get<IRoom>(DataTemplate.ID, typeof(IRoom));

            //Isn't in the world currently
            if (me == default(IRoom))
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

        public void SpawnNewInWorld()
        {
            var liveWorld = new LiveCache();
            var roomTemplate = (IRoom)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(roomTemplate);
            Keywords = new string[] { roomTemplate.Title.ToLower() };
            Birthdate = DateTime.Now;

            liveWorld.Add<IRoom>(this);
        }

        public int CompareTo(IEntity other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != typeof(Room))
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
                    return other.GetType() == typeof(Room) && other.BirthMark.Equals(this.BirthMark);
                }
                catch
                {
                    //Minor error logging
                }
            }

            return false;
        }

        public void Fill(global::System.Data.DataRow dr)
        {
            int outId = default(int);
            DataUtility.GetFromDataRow<int>(dr, "ID", ref outId);
            ID = outId;

            DateTime outCreated = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "Created", ref outCreated);
            Created = outCreated;

            DateTime outRevised = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised", ref outRevised);
            LastRevised = outRevised;

            string outTitle = default(string);
            DataUtility.GetFromDataRow<string>(dr, "Title", ref outTitle);
            Title = outTitle;
        }

        public IData Create()
        {
            throw new NotImplementedException();
        }

        public bool Remove()
        {
            throw new NotImplementedException();
        }

        public bool Save()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(IData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != typeof(Room))
                        return -1;

                    if (other.ID.Equals(this.ID))
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

        public bool Equals(IData other)
        {
            if (other != default(IData))
            {
                try
                {
                    return other.GetType() == typeof(Room) && other.ID.Equals(this.ID);
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
