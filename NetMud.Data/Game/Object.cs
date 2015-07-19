using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NetMud.Data.Game
{
    public class Object : EntityPartial, IObject
    {
        public Object()
        {
            //IDatas need parameterless constructors
            Contents = new EntityContainer<IObject>();
        }

        public Object(IObjectData backingStore)
        {
            Contents = new EntityContainer<IObject>();
            DataTemplate = backingStore;
            SpawnNewInWorld();
        }

        public Object(IObjectData backingStore, IContains spawnTo)
        {
            Contents = new EntityContainer<IObject>();
            DataTemplate = backingStore;
            SpawnNewInWorld(spawnTo);
        }
     
        public long LastKnownLocation { get; set; }
        public string LastKnownLocationType { get; set; }

        #region Container
        public EntityContainer<IObject> Contents { get; set; }

        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IObject)))
                return GetContents<T>("objects");

            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> GetContents<T>(string containerName)
        {
            switch (containerName)
            {
                case "objects":
                    return Contents.EntitiesContained.Select(ent => (T)ent);
            }

            return Enumerable.Empty<T>();
        }

        public string MoveTo<T>(T thing)
        {
            return MoveTo<T>(thing, String.Empty);
        }

        public string MoveTo<T>(T thing, string containerName)
        {
            if (typeof(T).GetInterfaces().Contains(typeof(IObject)))
            {
                var obj = (IObject)thing;

                if (Contents.Contains(obj))
                    return "That is already in the container";

                Contents.Add(obj);
                obj.CurrentLocation = this;
                return String.Empty;
            }

            return "Invalid type to move to container.";
        }

        public string MoveFrom<T>(T thing)
        {
            return MoveFrom<T>(thing, String.Empty);
        }

        public string MoveFrom<T>(T thing, string containerName)
        {
            if (typeof(T).GetInterfaces().Contains(typeof(IObject)))
            {
                var obj = (IObject)thing;

                if (!Contents.Contains(obj))
                    return "That is not in the container";

                Contents.Remove(obj);
                obj.CurrentLocation = null;
                return String.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException("Objects can't spawn to nothing");
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            //We can't even try this until we know if the data is there
            if (DataTemplate == null)
                throw new InvalidOperationException("Missing backing data store on object spawn event.");

            var liveWorld = new LiveCache();
            var backingStore = (IObjectData)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(backingStore);
            Keywords = new string[] { backingStore.Name.ToLower() };
            Birthdate = DateTime.Now;

            //TODO: People get a base spawn but live objects need to be spawnable to a specific location or not at all really
            if (spawnTo == null)
            {
                throw new NotImplementedException("Objects can't spawn to nothing");
            }

            CurrentLocation = spawnTo;

            spawnTo.MoveTo<IObject>(this);

            Contents = new EntityContainer<IObject>();

            liveWorld.Add(this);
        }

        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();
            var backingStore = (IObjectData)DataTemplate;

            sb.Add(string.Format("There is a {0} here", backingStore.Name));

            return sb;
        }
    }
}
