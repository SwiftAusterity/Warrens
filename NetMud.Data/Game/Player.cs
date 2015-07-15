using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Game
{
    public class Player : IPlayer
    {
        public Player(ICharacter character)
        {
            DataTemplate = character;
            GetFromWorldOrSpawn();
        }

        public string BirthMark { get; private set; }

        public DateTime Birthdate { get; private set; }

        public string Keywords { get; set; }

        public IData DataTemplate { get; private set; }

        public ILocation CurrentLocation { get; set; }

        public void GetFromWorldOrSpawn()
        {
            var liveWorld = new LiveCache();

            //Try to see if they are already there
            var me = liveWorld.Get<IPlayer>(DataTemplate.ID);

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
                CurrentLocation.MoveTo<IPlayer>(this);
            }
        }

        public void SpawnNewInWorld()
        {
            var liveWorld = new LiveCache();
            var ch = (ICharacter)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(ch);
            Keywords = String.Format("{0}, {1}", ch.GivenName, ch.SurName);
            Birthdate = DateTime.Now;

            if (ch.LastKnownLocationType == null)
                ch.LastKnownLocationType = typeof(IRoom).Name;

            var lastKnownLocType = Type.GetType(ch.LastKnownLocationType);
            ILocation lastKnownLoc = liveWorld.Get<ILocation>(ch.LastKnownLocation, lastKnownLocType);

            if(lastKnownLoc == null)
            {
                //TODO: Not hardcode the zeroth room
                lastKnownLoc = liveWorld.Get<ILocation>(0, typeof(IRoom));

                //Set the data context's stuff too so we don't have to do this over again
                ch.LastKnownLocation = lastKnownLoc.DataTemplate.ID;
                ch.LastKnownLocationType = lastKnownLoc.GetType().Name;
                lastKnownLoc.MoveTo<IPlayer>(this);
            }

            CurrentLocation = lastKnownLoc;

            liveWorld.Add<IPlayer>(this);
        }

        public int CompareTo(IEntity other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != typeof(Player))
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
                    return other.GetType() == typeof(Player) && other.BirthMark.Equals(this.BirthMark);
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
