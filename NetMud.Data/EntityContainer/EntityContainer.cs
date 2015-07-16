using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data
{
    public class EntityContainer<T> : IEntityContainer<IEntity>
    {
        public long CapacityVolume { get; set; }
        public long CapacityWeight { get; set; }

        public IEnumerable<IEntity> EntitiesContained { get; set; }

        public EntityContainer()
        {
            EntitiesContained = Enumerable.Empty<IEntity>();
        }

        public bool Add(IEntity entity)
        {
            EntitiesContained.ToList().Add(entity);

            return true;
        }

        public bool Contains(IEntity entity)
        {
            return EntitiesContained.Contains(entity);
        }

        public bool Remove(IEntity entity)
        {
            EntitiesContained.ToList().Remove(entity);

            return true;
        }

        public bool Remove(string birthMark)
        {
            if (string.IsNullOrWhiteSpace(birthMark))
                return false;

            var removed = EntitiesContained.FirstOrDefault(ent => ((IEntity)ent).BirthMark.Equals(birthMark));

            if (removed == null)
                return false;

            return Remove(removed);
        }
    }
}
