using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.System
{
    public interface IEntity
    {
        /// <summary>
        /// Indelible guid that helps the system figure out where stuff is, generated when the object is spawned into the world
        /// </summary>
        string BirthMark { get; }
        DateTime Birthdate { get; }
        string Keywords { get; set; }

        IReference ReferenceTemplate { get; }
    }
}
