using NetMud.Data.EntityBackingData;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Game
{
    public class Locale : EntityPartial, ILocale
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        public override string DataTemplateName
        {
            get
            {
                if (DataTemplate<ILocaleData>() == null)
                    return String.Empty;

                return DataTemplate<ILocaleData>().Name;
            }
        }

        public IZone Affiliation { get; set; }
        public IMap Interior { get; set; }
        public IEnumerable<IRoom> Rooms { get; set; }
        public IEntityContainer<IPathway> Pathways { get; set; }

        public IRoom CentralRoom(int zIndex = -1)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Locale()
        {
            Pathways = new EntityContainer<IPathway>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Locale(ILocaleData locale)
        {
            Pathways = new EntityContainer<IPathway>();

            DataTemplateId = locale.ID;

            GetFromWorldOrSpawn();
        }

        public Tuple<int, int, int> Diameter()
        {
            //TODO
            return new Tuple<int, int, int>(1, 1, 1);
        }

        public Tuple<int, int, int> FullDimensions()
        {
            //TODO
            return new Tuple<int, int, int>(1, 1, 1);
        }

        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            yield return string.Empty;
        }

        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition { CurrentLocation = Affiliation, CurrentZone = Affiliation });
        }

        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            var dataTemplate = DataTemplate<ILocaleData>();

            BirthMark = LiveCache.GetUniqueIdentifier(dataTemplate);
            Keywords = new string[] { dataTemplate.Name.ToLower() };
            Birthdate = DateTime.Now;
        }

        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<ILocale>(DataTemplateId, typeof(LocaleData));

            //Isn't in the world currently
            if (me == default(ILocale))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplateId;
                Pathways = me.Pathways;
                Keywords = me.Keywords;
                Position = me.Position;
            }
        }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            return FullDimensions();
        }

        public string RenderMap(int zIndex, bool forAdmin = false)
        {
            throw new NotImplementedException();
        }

        public Dictionary<IRoom, ILocale> LocaleExitPoints()
        {
            return Pathways.EntitiesContained()
                    .Where(path => path.ToLocation.GetType() == typeof(ILocale))
                    .ToDictionary(path => (IRoom)path.FromLocation, vpath => (ILocale)vpath.ToLocation);
        }

        public Dictionary<IRoom, IZone> ZoneExitPoints()
        {
            return Pathways.EntitiesContained()
                    .Where(path => path.ToLocation.GetType() == typeof(IZone))
                    .ToDictionary(path => (IRoom)path.FromLocation, vpath => (IZone)vpath.ToLocation);
        }

        public IEnumerable<ILocation> GetSurroundings()
        {
            var locales = LocaleExitPoints().Select(pair => pair.Value);
            var zones = ZoneExitPoints().Select(pair => pair.Value);

            return locales.Select(locale => (ILocation)locale)
                .Union(zones.Select(zone => (ILocation)zone));
        }
    }
}
