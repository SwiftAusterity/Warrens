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
                var entities = new List<T>();

                foreach(var birthmark in Birthmarks)
                {
                    var liveCacheKey = new LiveCacheKey(typeof(T), birthmark);

                    entities.Add(LiveCache.Get<T>(liveCacheKey));
                }

                return entities;
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
