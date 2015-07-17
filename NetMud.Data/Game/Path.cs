using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Game
{
    public class Path : IPath
    {
        public IRoom ToRoom
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IRoom FromRoom
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DataStructure.SupportingClasses.MessageCluster Enter
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string BirthMark
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime Birthdate
        {
            get { throw new NotImplementedException(); }
        }

        public string[] Keywords
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DataStructure.Base.System.IData DataTemplate
        {
            get { throw new NotImplementedException(); }
        }

        public DataStructure.Behaviors.Rendering.ILocation CurrentLocation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void GetFromWorldOrSpawn()
        {
            throw new NotImplementedException();
        }

        public void SpawnNewInWorld()
        {
            throw new NotImplementedException();
        }

        public void SpawnNewInWorld(DataStructure.Behaviors.Rendering.ILocation spawnTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> RenderToLook()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(DataStructure.Base.System.IEntity other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(DataStructure.Base.System.IEntity other)
        {
            throw new NotImplementedException();
        }
    }
}
