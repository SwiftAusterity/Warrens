using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.System
{
    public interface IEntity : ILookable, IComparable<IEntity>, IEquatable<IEntity>
    {
        /// <summary>
        /// Indelible guid that helps the system figure out where stuff is, generated when the object is spawned into the world
        /// </summary>
        string BirthMark { get; }
        DateTime Birthdate { get; }
        string Keywords { get; set; }

        IData DataTemplate { get; }

        ILocation CurrentLocation { get; set; }

        void GetFromWorldOrSpawn();
        void SpawnNewInWorld();
    }
}
