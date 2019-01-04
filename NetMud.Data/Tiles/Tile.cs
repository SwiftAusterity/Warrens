using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using NetMud.Gaia.Geographical;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.Tiles
{
    /// <summary>
    /// Places entities are (most of the time)
    /// </summary>
    [Serializable]
    public class Tile : ITile
    {
        [JsonProperty("ParentLocation")]
        private LiveCacheKey _parentLocation { get; set; }

        /// <summary>
        /// The locale this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Tiles must have a zone affiliation.")]
        public IZone ParentLocation
        {
            get
            {
                return LiveCache.Get<IZone>(_parentLocation);
            }
            set
            {
                if (value != null)
                    _parentLocation = new LiveCacheKey(value);
            }
        }

        public Coordinate Coordinate { get; set; }

        [JsonProperty("Type")]
        private TemplateCacheKey _type { get; set; }

        /// <summary>
        /// The locale this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public ITileTemplate Type
        {
            get
            {
                return TemplateCache.Get<ITileTemplate>(_type);
            }
            set
            {
                if (value != null)
                    _type = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// List of live qualities of this entity
        /// </summary>
        public HashSet<IQuality> Qualities { get; set; }

        /// <summary>
        /// Interactions for players against tiles
        /// </summary>
        public HashSet<IDecayEvent> DecayEvents { get; set; }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Tile()
        {
            Qualities = new HashSet<IQuality>();
            DecayEvents = new HashSet<IDecayEvent>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Tile(IZone parentLocation, ITileTemplate type, Coordinate coordinate)
        {
            ParentLocation = parentLocation;
            Type = type;
            Coordinate = coordinate;
            Qualities = new HashSet<IQuality>();

            DecayEvents = _type == null ? new HashSet<IDecayEvent>() : Type.DecayEvents;
        }

        /// <summary>
        /// Is something in this tile? Only one thing at a time
        /// </summary>
        public IEntity TopContents()
        {
            //TODO: must be a more efficient way to do this
            IInanimate inanimate = LiveCache.GetAll<IInanimate>().FirstOrDefault(thing => thing.CurrentLocation != null
                && thing.CurrentLocation.GetTile() == this
                && thing.CurrentLocation.CurrentContainer == null);

            if (inanimate != null)
                return inanimate;

            INonPlayerCharacter npc = LiveCache.GetAll<INonPlayerCharacter>().FirstOrDefault(thing => thing.CurrentLocation != null
                && thing.CurrentLocation.GetTile() == this
                && thing.CurrentLocation.CurrentContainer == null);

            if (npc != null)
                return npc;

            IPlayer player = LiveCache.GetAll<IPlayer>().FirstOrDefault(thing => thing.CurrentLocation != null
                && thing.CurrentLocation.GetTile() == this
                && thing.CurrentLocation.CurrentContainer == null);

            return player;
        }

        /// <summary>
        /// Get the full stack of contents, only applicable with inanimates
        /// </summary>
        public IEnumerable<IInanimate> StackedContents()
        {
            if (!(TopContents() is IInanimate topContent))
                return null;

            return LiveCache.GetAll<IInanimate>().Where(thing => thing.CurrentLocation != null
                && thing.CurrentLocation.GetTile() == this
                && thing.CurrentLocation.CurrentContainer == null);
        }

        /// <summary>
        /// Does this tile have a pathway?
        /// </summary>
        /// <returns>the pathway</returns>
        public IPathway Pathway()
        {
            return ParentLocation.GetPathway(Coordinate.X, Coordinate.Y);
        }

        /// <summary>
        /// Check for a quality
        /// </summary>
        /// <param name="name">Gets the value of the request quality</param>
        /// <returns>The value</returns>
        public int GetQuality(string name)
        {
            IQuality currentQuality = Qualities.FirstOrDefault(qual => qual.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (currentQuality == null)
                return 0;

            return currentQuality.Value;
        }

        /// <summary>
        /// Add a quality (can be negative)
        /// </summary>
        /// <param name="value">The value you're adding</param>
        /// <param name="additive">Is this additive or replace-ive</param>
        /// <returns>The new value</returns>
        public int SetQuality(int value, string quality, bool additive)
        {
            IQuality currentQuality = Qualities.FirstOrDefault(qual => qual.Name.Equals(quality, StringComparison.InvariantCultureIgnoreCase));

            if (currentQuality == null)
            {
                Qualities.Add(new Quality()
                {
                    Name = quality,
                    Type = QualityType.Aspect,
                    Visible = true,
                    Value = value
                });

                return value;
            }

            if (additive)
                currentQuality.Value += value;
            else
                currentQuality.Value = value;

            return value;
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer)
        {
            bool canSeeSky = GeographicalUtilities.IsOutside(ParentLocation.GetBiome());

            //if (!canSeeSky)
            //    return Enumerable.Empty<ICelestial>();

            //The zone knows about the celestial positioning
            return ParentLocation.GetVisibileCelestials(viewer);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public float GetCurrentLuminosity()
        {
            float lumins = ParentLocation.GetCurrentLuminosity();

            return lumins;
        }

        #region rendering
        /// <summary>
        /// Renders out an ascii map of this room plus all rooms in the radius
        /// </summary>
        /// <param name="radius">how far away to render</param>
        /// <returns>the string</returns>
        public string RenderCenteredMap(short radiusX, short radiusY, bool visibleOnly, IActor protagonist, bool adminOnly)
        {
            //TODO: fix visibility
            return Cartography.Rendering.RenderRadiusMap(ParentLocation, this, radiusX, radiusY, protagonist, adminOnly);
        }

        /// <summary>
        /// Renders HTML for the info card popups
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output HTML</returns>
        public string RenderToInfo(IEntity viewer)
        {
            if (viewer == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            StaffRank rank = viewer.ImplementsType<IPlayer>() ? viewer.Template<IPlayerTemplate>().GamePermissionsRank : StaffRank.Player;

            sb.Append("<div class='helpItem'>");

            sb.AppendFormat("<h3><span style='color: {2}; background-color: {3}'>{1}</span> {0} ({4},{5})</h3>",
                Type.Name, Type.AsciiCharacter, Type.HexColorCode, Type.BackgroundHexColor, Coordinate.X, Coordinate.Y);
            sb.Append("<hr />");

            List<string> pathability = new List<string>();

            if (Type.Pathable)
                pathability.Add("Pathable");

            if (Type.Aquatic)
                pathability.Add("Aquatic");

            if (Type.Air)
                pathability.Add("Air");

            sb.AppendFormat("<div>{0}</div>", string.Join(",", pathability));

            IPathway pathway = Pathway();
            if (pathway != null)
            {
                sb.AppendFormat("<h4 style='color: {0};'>Pathway Destinations</h4>", pathway.BorderHexColor);
                sb.AppendFormat("<div>{0}</div>", string.Join(",", pathway.Destinations.Select(q => string.Format("({0}:{1})", q.Name, q.Destination.Name))));
            }

            if (Qualities.Count > 0)
            {
                sb.Append("<h4>Qualities</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", Qualities.Select(q => string.Format("({0}:{1})", q.Name, q.Value))));
            }

            if (Type.Interactions.Count > 0)
            {
                sb.Append("<h4>Actions</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", Type.Interactions.Select(i => i.Name)));
            }

            if (rank > StaffRank.Player && DecayEvents.Count > 0)
            {
                sb.Append("<h4>Timed Events</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", DecayEvents.Select(i => i.Name)));
            }

            IEntity intruder = TopContents();
            if (intruder != null)
            {
                sb.Append("<hr/>");
                sb.Append(intruder.RenderToInfo(viewer));
            }

            sb.Append("</div>");

            return sb.ToString();
        }
        #endregion

        #region Processes
        public void ProcessDecayEvents()
        {
            //We check once per day to see if any dormant decay events need to start
            foreach (IDecayEvent decayEvent in DecayEvents)
            {
                bool skipMe = false;
                //Check the requirements and remove anything from the list that does not belong
                foreach (IActionCriteria criterion in decayEvent.Criteria.Where(crit => crit.Target == ActionTarget.Self))
                {
                    if (string.IsNullOrWhiteSpace(criterion.Quality))
                        continue;

                    if (!Qualities.Any(key =>
                         key.Type == QualityType.Aspect
                         && (criterion.ValueRange.Low <= 0 || key.Value.IsBetweenOrEqual(criterion.ValueRange.Low, criterion.ValueRange.High))
                         && key.Name.Equals(criterion.Quality, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        //We have to move on, can't decrement the timer if we don't meet the criterion
                        skipMe = true;
                    }
                }

                if (skipMe)
                    continue;

                decayEvent.CurrentTime--;

                //fire event
                if (decayEvent.CurrentTime <= 0)
                {
                    decayEvent.Invoke(null, this, null);
                    decayEvent.CurrentTime = decayEvent.Timer;
                }
            }
        }
        #endregion
    }
}
