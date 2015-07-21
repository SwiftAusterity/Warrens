using NetMud.DataAccess;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Game
{
    public abstract class EntityPartial : IEntity
    {
        #region Data and live tracking properties
        public string BirthMark { get; internal set; }
        public DateTime Birthdate { get; internal set; }
        public IData DataTemplate { get; internal set; }

        private string[] _keywords;
        public string[] Keywords
        {
            get { return _keywords; }
            set
            {
                _keywords = value;
                UpsertToLiveWorldCache();
            }
        }
        #endregion

        public Func<IEnumerable<string>, bool> WriteTo { get; set; }

        private IContains _currentLocation;
        public IContains CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                _currentLocation = value;
                UpsertToLiveWorldCache();
            }
        }

        public abstract void SpawnNewInWorld();

        public abstract void SpawnNewInWorld(IContains spawnTo);

        public void UpsertToLiveWorldCache()
        {
            var liveWorld = new LiveCache();
            liveWorld.Add(this);
        }

        public abstract IEnumerable<string> RenderToLook();

        #region Equality Functions
        public int CompareTo(IEntity other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != this.GetType())
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
                    return other.GetType() == this.GetType() && other.BirthMark.Equals(this.BirthMark);
                }
                catch
                {
                    //Minor error logging
                }
            }

            return false;
        }
        #endregion
    }
}
