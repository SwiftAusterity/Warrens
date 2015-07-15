using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Behaviors.Automation
{
    public interface IEntityContainer<T> where T : IEntity
    {
        long CapacityVolume { get; set; }
        long CapacityWeight { get; set; }

        IEnumerable<T> EntitiesContained { get; }

        bool Remove(string birthMark);
        bool Remove(T entity);
        bool Add(T entity);
        bool Contains(T entity);
    }
}
