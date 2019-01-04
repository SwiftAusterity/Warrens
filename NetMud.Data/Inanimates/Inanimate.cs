using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Player;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.Inanimates
{
    /// <summary>
    /// "Object" class
    /// </summary>
    [Serializable]
    public class Inanimate : EntityPartial, IInanimate
    {
        #region Template and Framework Values
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<IInanimateTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IInanimateTemplate), TemplateId));
        }

        /// <summary>
        /// How many of this can be in a stack
        /// </summary>
        public int AccumulationCap { get; set; }

        /// <summary>
        /// Character->tile interactions
        /// </summary>
        public HashSet<IUse> Uses { get; set; }
        #endregion

        [JsonConstructor]
        public Inanimate()
        {
            //IDatas need parameterless constructors
            Contents = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();
            Interactions = new HashSet<IInteraction>();
            DecayEvents = new HashSet<IDecayEvent>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Inanimate(IInanimateTemplate backingStore)
        {
            Contents = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();
            Interactions = new HashSet<IInteraction>();
            DecayEvents = new HashSet<IDecayEvent>();

            TemplateId = backingStore.Id;
        }

        /// <summary>
        /// News up an entity with its backing data and where to spawn it into
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        /// <param name="spawnTo">where to spawn this into</param>
        public Inanimate(IInanimateTemplate backingStore, IGlobalPosition spawnTo)
        {
            Contents = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();

            TemplateId = backingStore.Id;
            SpawnNewInWorld(spawnTo);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;
            foreach (IInanimate thing in Contents.EntitiesContained())
                lumins += thing.GetCurrentLuminosity();

            return lumins;
        }

        #region Container
        /// <summary>
        /// Inanimates contained in this
        /// </summary>
        public IEntityContainer<IInanimate> Contents { get; set; }
        public int Capacity => 50;

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            List<T> contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Contents.EntitiesContained().Select(ent => (T)ent));

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
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));
            string error = "Invalid type to move to container.";

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (Contents.Contains(obj))
                    return "That is already in the container";

                IEnumerable<IInanimate> contents = Contents.EntitiesContained();

                //Should we even check? we have 100 slots make sure there's not one just open anyways
                if (contents.Count() == 100)
                {
                    //Check for stacking
                    int sameType = contents.Where(entity => entity.TemplateId.Equals(obj.TemplateId)).Count();
                    int numStacks = Math.DivRem(sameType, obj.Template<IInanimateTemplate>().AccumulationCap, out int extraSpace);

                    //We need an extra slot for this to go in, we're operating on 100 slots of inventory and they're all full and no room in an existing stack so error
                    if (extraSpace == 0)
                    {
                        return TemplateName + " is full and no stacks inside it have room.";
                    }
                }

                Contents.Add(obj);
                UpsertToLiveWorldCache();
                return string.Empty;
            }

            return error;
        }

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (!Contents.Contains(obj))
                    return "That is not in the container";

                Contents.Remove(obj);
                obj.TryMoveTo(null);

                UpsertToLiveWorldCache();

                return string.Empty;
            }

            return "Invalid type to move from container.";
        }

        /// <summary>
        /// Returns this entity as a container position
        /// </summary>
        /// <returns></returns>
        public IGlobalPosition GetContainerAsLocation()
        {
            return new GlobalPosition(this);
        }

        /// <summary>
        /// Show the stacks in this container, only for inanimates
        /// </summary>
        /// <returns>A list of the item stacks</returns>
        public HashSet<IItemStack> ShowStacks(IEntity observer)
        {
            HashSet<IItemStack> stacks = new HashSet<IItemStack>();

            foreach (IGrouping<long, IInanimate> itemGroup in Contents.EntitiesContained().GroupBy(item => item.TemplateId))
            {
                stacks.Add(new ItemStack(itemGroup));
            }

            return stacks;
        }
        #endregion

        #region spawning
        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            if (CurrentLocation == null)
                throw new NotImplementedException("Objects can't spawn to nothing");

            SpawnNewInWorld(CurrentLocation);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            IInanimateTemplate bS = Template<IInanimateTemplate>() ?? throw new InvalidOperationException("Missing backing data store on object spawn event.");

            Keywords = bS.Keywords;

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            Qualities = bS.Qualities;
            DecayEvents = bS.DecayEvents;
            Interactions = bS.Interactions;
            Uses = bS.Uses;
            AccumulationCap = bS.AccumulationCap;

            TryMoveTo(spawnTo);

            if (CurrentLocation == null)
                throw new NotImplementedException("Objects can't spawn to nothing");

            UpsertToLiveWorldCache(true);

            KickoffProcesses();
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            string error = string.Empty;

            if (CurrentLocation?.CurrentContainer != null)
            {
                error = CurrentLocation.CurrentContainer.MoveFrom(this);
            }

            //validate position
            if (newPosition != null && string.IsNullOrEmpty(error))
            {
                if (newPosition.CurrentContainer != null)
                {
                    error = newPosition.CurrentContainer.MoveInto(this);
                }

                if (string.IsNullOrEmpty(error))
                {
                    //Check for intruders
                    newPosition = IntruderSlide(newPosition);

                    CurrentLocation = newPosition;
                    UpsertToLiveWorldCache();
                    error = string.Empty;
                }
            }
            else
            {
                error = "Cannot move to an invalid location";
            }

            return error;
        }

        internal override IGlobalPosition IntruderSlide(IGlobalPosition newPosition)
        {
            //we don't care about containers or bad positions
            if (newPosition.CurrentContainer != null || newPosition.CurrentZone == null)
                return newPosition;

            //bad coordinates
            if (newPosition.CurrentCoordinates == null
                || !newPosition.CurrentCoordinates.X.IsBetweenOrEqual(0, 99)
                || !newPosition.CurrentCoordinates.Y.IsBetweenOrEqual(0, 99))
            {
                if (!newPosition.CurrentZone.BaseCoordinates.X.IsBetweenOrEqual(0, 99)
                || !newPosition.CurrentZone.BaseCoordinates.Y.IsBetweenOrEqual(0, 99))
                {
                    //zone has bad base coordinates?
                    newPosition.CurrentCoordinates = new Coordinate(0, 0);
                }
                else
                {
                    newPosition.CurrentCoordinates = newPosition.CurrentZone.BaseCoordinates;
                }

                //recur for more checks
                return IntruderSlide(newPosition);
            }

            //Now we get to slide
            IEntity currentContents = newPosition.GetTile().TopContents();
            if (currentContents != null)
            {
                //have to account for item stacking
                IEnumerable<IInanimate> stack = newPosition.GetTile().StackedContents();

                //No stack cause it wasnt an inanimate intruder or the stack isnt the same item or the stack is at cap already
                if (stack == null || currentContents.TemplateId != TemplateId || Template<IInanimateTemplate>().AccumulationCap <= stack.Count())
                {
                    if (newPosition.CurrentCoordinates.X > 0)
                    {
                        newPosition.CurrentCoordinates = new Coordinate((short)(newPosition.CurrentCoordinates.X - 1), newPosition.CurrentCoordinates.Y);
                    }
                    else if (newPosition.CurrentCoordinates.X < 100)
                    {
                        newPosition.CurrentCoordinates = new Coordinate((short)(newPosition.CurrentCoordinates.X + 1), newPosition.CurrentCoordinates.Y);
                    }
                    else if (newPosition.CurrentCoordinates.Y > 0)
                    {
                        newPosition.CurrentCoordinates = new Coordinate(newPosition.CurrentCoordinates.X, (short)(newPosition.CurrentCoordinates.Y - 1));
                    }
                    else if (newPosition.CurrentCoordinates.X < 100)
                    {
                        newPosition.CurrentCoordinates = new Coordinate(newPosition.CurrentCoordinates.X, (short)(newPosition.CurrentCoordinates.Y + 1));
                    }
                    else
                    {
                        //bad base coordinates?
                        newPosition.CurrentCoordinates = new Coordinate(0, 0);
                    }

                    //recur for more checks
                    return IntruderSlide(newPosition);
                }
            }

            return newPosition;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new Inanimate
            {
                Qualities = Qualities,
                TemplateId = TemplateId
            };
        }
        #endregion

        #region rendering
        /// <summary>
        /// Renders HTML for the info card popups
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output HTML</returns>
        public override string RenderToInfo(IEntity viewer)
        {
            if (viewer == null)
            {
                return string.Empty;
            }

            IInanimateTemplate dt = Template<IInanimateTemplate>();
            StringBuilder sb = new StringBuilder();
            StaffRank rank = viewer.ImplementsType<IPlayer>() ? viewer.Template<IPlayerTemplate>().GamePermissionsRank : StaffRank.Player;

            sb.Append("<div class='helpItem'>");

            sb.AppendFormat("<h3><span style='color: {2}'>{1}</span> {0}</h3>", GetDescribableName(viewer), dt.AsciiCharacter, dt.HexColorCode);
            sb.Append("<hr />");

            if (Qualities.Count > 0)
            {
                sb.Append("<h4>Qualities</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", Qualities.Select(q => string.Format("({0}:{1})", q.Name, q.Value))));
            }

            if (dt.Interactions.Count > 0)
            {
                sb.Append("<h4>Actions</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", dt.Interactions.Select(i => i.Name)));
            }

            if (dt.Uses.Count > 0)
            {
                sb.Append("<h4>Uses</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", dt.Uses.Select(i => i.Name)));
            }

            if (rank > StaffRank.Player && dt.DecayEvents.Count > 0)
            {
                sb.Append("<h4>Timed Events</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", dt.DecayEvents.Select(i => i.Name)));
            }

            sb.Append("</div>");

            return sb.ToString();
        }
        #endregion

        public bool IsPlayer()
        {
             return false;
        }
    }
}
