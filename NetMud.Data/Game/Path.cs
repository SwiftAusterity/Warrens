using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Game
{
    public class Path : IPath
    {
        public ILocation ToLocation { get; set; }
        public ILocation FromLocation { get; set; }
        public MessageCluster Enter { get; set; }

        public string BirthMark { get; private set; }
        public DateTime Birthdate { get; private set; }
        public string[] Keywords { get; set; }
        public IData DataTemplate { get; private set; }

        public ILocation CurrentLocation { get; set; }

        public void GetFromWorldOrSpawn()
        {
            throw new NotImplementedException();
        }

        public void SpawnNewInWorld()
        {
            throw new NotImplementedException();
        }

        public void SpawnNewInWorld(ILocation spawnTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> RenderToLook()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(IEntity other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IEntity other)
        {
            throw new NotImplementedException();
        }
    }
}
