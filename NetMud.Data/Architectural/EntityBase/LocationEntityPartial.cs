using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NaturalResource;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Partial template for live Location entities
    /// </summary>
    public abstract class LocationEntityPartial : EntityPartial, ILocation
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<ILocationData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(ILocationData), TemplateId));
        }

        /// <summary>
        /// Current base humidity
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Humidity", Description = "The current barometric pressure.")]
        [DataType(DataType.Text)]
        public virtual int Humidity { get; set; }

        /// <summary>
        /// Base temperature
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature", Description = "The current ambient temperature.")]
        [DataType(DataType.Text)]
        public virtual int Temperature { get; set; }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        [UIHint("FaunaResourceSpawnCollection")]
        public HashSet<INaturalResourceSpawn<IFauna>> FaunaNaturalResources { get; set; }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        [UIHint("FloraResourceSpawnCollection")]
        public HashSet<INaturalResourceSpawn<IFlora>> FloraNaturalResources { get; set; }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        [UIHint("MineralResourceSpawnCollection")]
        public HashSet<INaturalResourceSpawn<IMineral>> MineralNaturalResources { get; set; }

        #region Container
        /// <summary>
        /// Inanimates contained in this
        /// </summary>
        public IEntityContainer<IInanimate> Contents { get; set; }

        /// <summary>
        /// Any mobiles (players, npcs) contained in this
        /// </summary>
        public IEntityContainer<IMobile> MobilesInside { get; set; }

        public int Capacity => 1;

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            List<T> contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                contents.AddRange(MobilesInside.EntitiesContained().Select(ent => (T)ent));
            }

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                contents.AddRange(Contents.EntitiesContained().Select(ent => (T)ent));
            }

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
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            List<T> contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                contents.AddRange(MobilesInside.EntitiesContained(containerName).Select(ent => (T)ent));
            }

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                contents.AddRange(Contents.EntitiesContained(containerName).Select(ent => (T)ent));
            }

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
            return MoveInto(thing, string.Empty);
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
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (Contents.Contains(obj, containerName))
                {
                    return "That is already in the container";
                }

                Contents.Add(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                IMobile obj = (IMobile)thing;

                if (MobilesInside.Contains(obj, containerName))
                {
                    return "That is already in the container";
                }

                MobilesInside.Add(obj, containerName);
                UpsertToLiveWorldCache();

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
            return MoveFrom(thing, string.Empty);
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
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (!Contents.Contains(obj, containerName))
                {
                    return "That is not in the container";
                }

                Contents.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                IMobile obj = (IMobile)thing;

                if (!MobilesInside.Contains(obj, containerName))
                {
                    return "That is not in the container";
                }

                MobilesInside.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        public IEnumerable<IPathway> GetPathways(bool inward = false)
        {
            return LiveCache.GetAll<IPathway>().Where(path => path.Destination != null
                                                            && path.Origin != null
                                                            && (path.Origin.Equals(this) || (inward && path.Destination.Equals(this))));
        }

        /// <summary>
        /// Pathways leading out of (or into) this that are a zone
        /// </summary>
        public IEnumerable<IPathway> GetZonePathways(bool inward = false)
        {
            return GetPathways(inward).Where(path => path.Destination.GetType().GetInterfaces().Contains(typeof(IZone))
                                                  || (inward && path.Origin.GetType().GetInterfaces().Contains(typeof(IZone)) && path.Origin != this));
        }

        /// <summary>
        /// Pathways leading out of (or into) this that are from a different locale
        /// </summary>
        public IEnumerable<IPathway> GetLocalePathways(bool inward = false)
        {
            return GetPathways(inward).Where(path => path.Destination.GetType().GetInterfaces().Contains(typeof(IRoom))
                                            && (GetType().GetInterfaces().Contains(typeof(IZone))
                                                || ((IRoom)path.Destination).ParentLocation.BirthMark != ((IRoom)this).ParentLocation.BirthMark));
        }

        /// <summary>
        /// Get the surrounding locations based on a strength radius
        /// </summary>
        /// <param name="strength">number of places to go out</param>
        /// <returns>list of valid surrounding locations</returns>
        public virtual IEnumerable<ILocation> GetSurroundings(int strength)
        {
            List<ILocation> radiusLocations = new List<ILocation>();
            IEnumerable<IPathway> paths = GetPathways();

            //If we don't have any paths out what can we even do
            if (paths.Count() == 0)
            {
                return radiusLocations;
            }

            int currentRadius = 0;
            while (currentRadius <= strength && paths.Count() > 0)
            {
                IEnumerable<ILocation> currentLocsSet = paths.Select(path => path.Destination);

                if (currentLocsSet.Count() == 0)
                {
                    break;
                }

                radiusLocations.AddRange(currentLocsSet);
                paths = currentLocsSet.SelectMany(ro => ro.GetPathways());

                currentRadius++;
            }

            return radiusLocations;
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public abstract IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer);

        /// <summary>
        /// "Functional" Humiditiy
        /// </summary>
        /// <returns></returns>
        public virtual int EffectiveHumidity()
        {
            //TODO: More stuff
            return Humidity;
        }

        /// <summary>
        /// Functional temperature
        /// </summary>
        /// <returns></returns>
        public virtual int EffectiveTemperature()
        {
            //TODO: More Stuff
            return Temperature;
        }

        /// <summary>
        /// Are we out doors?
        /// </summary>
        /// <returns>if we are outside</returns>
        public virtual bool IsOutside()
        {
            //TODO: make this work
            return true;// GeographicalUtilities.IsOutside(GetBiome());
        }

        /// <summary>
        /// Get the biome
        /// </summary>
        /// <returns>the biome</returns>
        public virtual Biome GetBiome()
        {
            return Biome.Fabricated;
        }

        public abstract IGlobalPosition GetContainerAsLocation();
    }
}
