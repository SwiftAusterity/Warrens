using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetMud.Data.Game
{
    public class Intelligence : EntityPartial, IIntelligence
    {
        public Intelligence()
        {
            //IDatas need parameterless constructors
            Inventory = new EntityContainer<IObject>();
        }

        public Intelligence(INonPlayerCharacter backingStore)
        {
            Inventory = new EntityContainer<IObject>();
            DataTemplate = backingStore;
            SpawnNewInWorld();
        }

        public Intelligence(INonPlayerCharacter backingStore, IContains spawnTo)
        {
            Inventory = new EntityContainer<IObject>();
            DataTemplate = backingStore;
            SpawnNewInWorld(spawnTo);
        }

        #region Rendering
        public override IEnumerable<string> RenderToLook()
        {
            var sb = new List<string>();
            var ch = (INonPlayerCharacter)DataTemplate;

            sb.Add(string.Format("This is {0}", ch.FullName()));

            return sb;
        }
        #endregion

        #region Container
        public EntityContainer<IObject> Inventory { get; set; }

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
                    return Inventory.EntitiesContained.Select(ent => (T)ent);
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

                if (Inventory.Contains(obj))
                    return "That is already in the container";

                Inventory.Add(obj);
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

                if (!Inventory.Contains(obj))
                    return "That is not in the container";

                Inventory.Remove(obj);
                obj.CurrentLocation = null;
                return String.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        #region SpawnBehavior
        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException("NPCs can't spawn to nothing");
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            //We can't even try this until we know if the data is there
            if (DataTemplate == null)
                throw new InvalidOperationException("Missing backing data store on NPC spawn event.");

            var liveWorld = new LiveCache();
            var backingStore = (INonPlayerCharacter)DataTemplate;

            BirthMark = Birthmarker.GetBirthmark(backingStore);
            Keywords = new string[] { backingStore.Name.ToLower() };
            Birthdate = DateTime.Now;

            //TODO: People get a base spawn but live objects need to be spawnable to a specific location or not at all really
            if (spawnTo == null)
            {
                throw new NotImplementedException("NPCs can't spawn to nothing");
            }

            CurrentLocation = spawnTo;

            spawnTo.MoveTo<IIntelligence>(this);

            Inventory = new EntityContainer<IObject>();

            liveWorld.Add(this);
        }
        #endregion
    }
}
