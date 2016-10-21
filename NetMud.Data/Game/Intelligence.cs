using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Game
{
    /// <summary>
    /// NPCs
    /// </summary>
    [Serializable]
    public class Intelligence : EntityPartial, IIntelligence
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        public override string DataTemplateName
        {
            get
            {
                if (DataTemplate<INonPlayerCharacter>() == null)
                    return String.Empty;

                return DataTemplate<INonPlayerCharacter>().Name;
            }
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Intelligence()
        {
            //IDatas need parameterless constructors
            Inventory = new EntityContainer<IInanimate>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Intelligence(INonPlayerCharacter backingStore)
        {
            Inventory = new EntityContainer<IInanimate>();
            DataTemplateId = backingStore.ID;
            SpawnNewInWorld();
        }

        /// <summary>
        /// News up an entity with its backing data and where to spawn it into
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        /// <param name="spawnTo">where to spawn this into</param>
        public Intelligence(INonPlayerCharacter backingStore, IContains spawnTo)
        {
            Inventory = new EntityContainer<IInanimate>();
            DataTemplateId = backingStore.ID;
            SpawnNewInWorld(spawnTo);
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            var charData = DataTemplate<INonPlayerCharacter>(); ;
            var height = charData.RaceData.Head.Model.Height + charData.RaceData.Torso.Model.Height + charData.RaceData.Legs.Item1.Model.Height;
            var length = charData.RaceData.Torso.Model.Length;
            var width = charData.RaceData.Torso.Model.Width;

            return new Tuple<int, int, int>(height, length, width);
        }

        #region Rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>();
            var ch = DataTemplate<INonPlayerCharacter>(); ;

            sb.Add(string.Format("This is {0}", ch.FullName()));

            return sb;
        }
        #endregion

        #region Container
        /// <summary>
        /// Inanimates contained in this
        /// </summary>
        public IEntityContainer<IInanimate> Inventory { get; set; }

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Inventory.EntitiesContained().Select(ent => (T)ent));

            return contents;
        }

        /// <summary>
        /// Get all of the entities matching a type inside this in a named container
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        /// <param name="containerName">the name of the container</param>
        public IEnumerable<T> GetContents<T>(string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Inventory.EntitiesContained(containerName).Select(ent => (T)ent));

            return contents;
        }

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing)
        {
            return MoveInto<T>(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity into a named container in this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (Inventory.Contains(obj, containerName))
                    return "That is already in the container";

                Inventory.Add(obj, containerName);
                obj.CurrentLocation = this;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move to container.";
        }

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing)
        {
            return MoveFrom<T>(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity out of this' named container
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (!Inventory.Contains(obj, containerName))
                    return "That is not in the container";

                Inventory.Remove(obj, containerName);
                obj.CurrentLocation = null;
                this.UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        #region SpawnBehavior
        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException("NPCs can't spawn to nothing");
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IContains spawnTo)
        {
            //We can't even try this until we know if the data is there
            if (DataTemplate<INonPlayerCharacter>() == null)
                throw new InvalidOperationException("Missing backing data store on NPC spawn event.");

            var bS = DataTemplate<INonPlayerCharacter>(); ;

            BirthMark = LiveCache.GetUniqueIdentifier(bS);
            Keywords = new string[] { bS.Name.ToLower() };
            Birthdate = DateTime.Now;

            if (spawnTo == null)
            {
                throw new NotImplementedException("NPCs can't spawn to nothing");
            }

            CurrentLocation = spawnTo;

            spawnTo.MoveInto<IIntelligence>(this);

            Inventory = new EntityContainer<IInanimate>();

            LiveCache.Add(this);
        }
        #endregion
    }
}
