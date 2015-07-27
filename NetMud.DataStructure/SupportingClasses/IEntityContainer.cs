using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.SupportingClasses
{
    public interface IEntityContainer<T>
    {
        long CapacityVolume { get; set; }
        long CapacityWeight { get; set; }

        IEnumerable<T> EntitiesContained { get; }

        bool Add(T entity);
        bool Contains(T entity);
        bool Remove(T entity);
        bool Remove(string birthMark);
        int Count();
    }
}
