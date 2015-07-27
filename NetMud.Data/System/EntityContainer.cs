using NetMud.DataAccess;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.System
{
    public class EntityContainer<T> : IEntityContainer<T> where T : IEntity
    {
        public long CapacityVolume { get; set; }
        public long CapacityWeight { get; set; }

        private HashSet<string> Birthmarks;
        public IEnumerable<T> EntitiesContained 
        { 
            get
            {
                if(Count() > 0)
                    return LiveCache.GetMany<T>(Birthmarks);

                return Enumerable.Empty<T>();
            }
        }

        public EntityContainer()
        {
            Birthmarks = new HashSet<string>();
        }

        public bool Add(T entity)
        {
            return Birthmarks.Add(entity.BirthMark);
        }

        public bool Contains(T entity)
        {
            return Birthmarks.Contains(entity.BirthMark);
        }

        public bool Remove(T entity)
        {
            return Birthmarks.Remove(entity.BirthMark);
        }

        public bool Remove(string birthMark)
        {
            return Birthmarks.Remove(birthMark);
        }

        public int Count()
        {
            return Birthmarks.Count;
        }
    }
}
