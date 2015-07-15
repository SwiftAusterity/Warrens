using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.Behaviors.Rendering;
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

        }

        public Room(IRoom room)
        {
            //Yes it's its own datatemplate and currentLocation
            DataTemplate = room;
            
            ID = room.ID;
            Created = room.Created;
            LastRevised = room.LastRevised;
            Title = room.Title;

            CurrentLocation = room;
            GetFromWorldOrSpawn();
        }

        public IEntityContainer<IObject> ObjectsInRoom { get; set; }

        public IEntityContainer<IMobile> MobilesInRoom { get; set; }

        public string BirthMark { get; private set; }

        public DateTime Birthdate { get; private set; }

        public string Keywords { get; set; }

        public IData DataTemplate { get; private set; }

        public ILocation CurrentLocation { get; set; }

        public string MoveTo<T>(T thing)
        {
            return MoveTo<T>(thing, String.Empty);
        }

        public string MoveTo<T>(T thing, string containerName)
        {
            if(typeof(T) == typeof(IObject))
            {
                var obj = (IObject)thing;

                if(ObjectsInRoom.Contains(obj))
                    return "That is already in the container";

                ObjectsInRoom.Add(obj);
                return String.Empty;
            }
            else if(typeof(T) == typeof(IMobile))
            {
                var mob = (IMobile)thing;

                if(MobilesInRoom.Contains(mob))
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
            if (typeof(T) == typeof(IObject))
            {
                var obj = (IObject)thing;

                if(!ObjectsInRoom.Contains(obj))
                    return "That is not in the container";

                ObjectsInRoom.Remove(obj);
                return String.Empty;
            }
            else if (typeof(T) == typeof(IMobile))
            {
                var mob = (IMobile)thing;

                if(!MobilesInRoom.Contains(mob))
                    return "That is not in the container";

                MobilesInRoom.Remove(mob);
                return String.Empty;
            }

            return "Invalid type to move from container.";
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
            Keywords = String.Format("{0}", roomTemplate.Title);
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
