using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataStructure.SupportingClasses
{
    public class EntityContainer<T> where T : IEntity
    {
        public long CapacityVolume { get; set; }
        public long CapacityWeight { get; set; }

        public IList<IEntity> EntitiesContained { get; set; }

        public EntityContainer()
        {
            EntitiesContained = new List<IEntity>();
        }

        public bool Add(IEntity entity)
        {
            EntitiesContained.Add(entity);

            return true;
        }

        public bool Contains(IEntity entity)
        {
            return EntitiesContained.Contains(entity);
        }

        public bool Remove(IEntity entity)
        {
            EntitiesContained.Remove(entity);

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
