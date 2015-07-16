using NetMud.DataStructure.Base.System;

using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Automation
{
    public interface IEntityContainer<T> where T : IEntity
    {
        long CapacityVolume { get; set; }
        long CapacityWeight { get; set; }

        IEnumerable<T> EntitiesContained { get; set; }

        bool Remove(string birthMark);
        bool Remove(T entity);
        bool Add(T entity);
        bool Contains(T entity);
    }
}
