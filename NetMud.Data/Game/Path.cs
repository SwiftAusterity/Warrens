using NetMud.DataAccess;
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
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Game
{
    public class Path : IPath
    {
        public ILocation ToLocation { get; set; }
        public ILocation FromLocation { get; set; }
        public MessageCluster Enter { get; set; }
        public MovementDirectionType MovementDirection { get; private set; }

        public string BirthMark { get; private set; }
        public DateTime Birthdate { get; private set; }
        public string[] Keywords { get; set; }
        public IData DataTemplate { get; private set; }

        public ILocation CurrentLocation { get; set; }

        public Path()
        {
            Enter = new MessageCluster();
        }

        public Path(IPathData backingStore)
        {
            Enter = new MessageCluster();
            DataTemplate = backingStore;
            GetFromWorldOrSpawn();
        }

        public void GetFromWorldOrSpawn()
        {
            var liveWorld = new LiveCache();

            //Try to see if they are already there
            var me = liveWorld.Get<Path>(DataTemplate.ID);

            //Isn't in the world currently
            if (me == default(IPath))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Keywords = me.Keywords;
                Birthdate = me.Birthdate;
                CurrentLocation = me.CurrentLocation;
                DataTemplate = me.DataTemplate;
                FromLocation = me.FromLocation;
                ToLocation = me.ToLocation;
                Enter = me.Enter;
                MovementDirection = me.MovementDirection;
            }
        }

        public void SpawnNewInWorld()
        {
            var liveWorld = new LiveCache();
            var bS = (IPathData)DataTemplate;

            SpawnNewInWorld(null);
        }

        public void SpawnNewInWorld(ILocation spawnTo)
        {
            var liveWorld = new LiveCache();
            var bS = (IPathData)DataTemplate;
            var locationAssembly = Assembly.GetAssembly(typeof(ILocation));

            MovementDirection = RenderUtility.TranslateDegreesToDirection(bS.DegreesFromNorth);

            BirthMark = Birthmarker.GetBirthmark(bS);
            Keywords = new string[] { bS.Name.ToLower(), MovementDirection.ToString().ToLower() };
            Birthdate = DateTime.Now;

            //paths need two locations
            ILocation fromLocation = null;
            var fromLocationType = locationAssembly.DefinedTypes.FirstOrDefault(tp => tp.Name.Equals(bS.FromLocationType));

            if (fromLocationType != null && !String.IsNullOrWhiteSpace(bS.FromLocationID))
            {
                if (fromLocationType.GetInterfaces().Contains(typeof(ISpawnAsSingleton)))
                {
                    long fromLocationID = long.Parse(bS.FromLocationID);
                    fromLocation = liveWorld.Get<ILocation>(fromLocationID, fromLocationType);
                }
                else
                {
                    var cacheKey = new LiveCacheKey(fromLocationType, bS.FromLocationID);
                    fromLocation = liveWorld.Get<ILocation>(cacheKey);
                }
            }

            ILocation toLocation = null;
            var toLocationType = locationAssembly.DefinedTypes.FirstOrDefault(tp => tp.Name.Equals(bS.ToLocationType));

            if (toLocationType != null && !String.IsNullOrWhiteSpace(bS.ToLocationID))
            {
                if (toLocationType.GetInterfaces().Contains(typeof(ISpawnAsSingleton)))
                {
                    long toLocationID = long.Parse(bS.ToLocationID);
                    toLocation = liveWorld.Get<ILocation>(toLocationID, toLocationType);
                }
                else
                {
                    var cacheKey = new LiveCacheKey(toLocationType, bS.ToLocationID);
                    toLocation = liveWorld.Get<ILocation>(cacheKey);
                }
            }

            CurrentLocation = fromLocation;
            FromLocation = fromLocation;
            ToLocation = toLocation;

            Enter = new MessageCluster(bS.MessageToActor, String.Empty, String.Empty, bS.MessageToOrigin, bS.MessageToDestination);

            fromLocation.MoveTo<IPath>(this);

            liveWorld.Add<IPath>(this);
        }

        public IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();
            var bS = (IPathData)DataTemplate;

            sb.Add(string.Format("{0} heads in the direction of {1} from {2} to {3}", bS.Name, MovementDirection.ToString(), FromLocation.DataTemplate.Name, ToLocation.DataTemplate.Name));

            return sb;
        }

        public int CompareTo(IEntity other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != typeof(Path))
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
                    return other.GetType() == typeof(Path) && other.BirthMark.Equals(this.BirthMark);
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
