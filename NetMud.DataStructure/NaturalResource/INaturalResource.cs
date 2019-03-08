using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Zone;
using System.Collections.Generic;

namespace NetMud.DataStructure.NaturalResource
{
    /// <summary>
    /// Natural resources (minerals, flora, fauna)
    /// </summary>
    public interface INaturalResource : ILookupData, ILookable, ISmellable, ITouchable, ITasteable, IAudible, ISensible, IRenderInLocation
    {
        /// <summary>
        /// How much spawns in one place in one spawn tick
        /// </summary>
        int AmountMultiplier { get; set; }

        /// <summary>
        /// How rare this is to spawn even in its optimal range
        /// </summary>
        int Rarity { get; set; }

        /// <summary>
        /// How much the spawned puissance varies
        /// </summary>
        int PuissanceVariance { get; set; }

        /// <summary>
        /// Spawns in elevations within this range
        /// </summary>
        ValueRange<int> ElevationRange { get; set; }

        /// <summary>
        /// Spawns in temperatures within this range
        /// </summary>
        ValueRange<int> TemperatureRange { get; set; }

        /// <summary>
        /// Spawns in humidities within this range
        /// </summary>
        ValueRange<int> HumidityRange { get; set; }

        /// <summary>
        /// What biomes this can spawn in
        /// </summary>
        HashSet<Biome> OccursIn { get; set; }

        /// <summary>
        /// Can spawn in system zones like non-player owned cities
        /// </summary>
        bool CanSpawnInSystemAreas { get; set; }

        /// <summary>
        /// Can this resource potentially spawn in this room
        /// </summary>
        /// <param name="room">The room to spawn in</param>
        /// <returns>if this can spawn there</returns>
        bool CanSpawnIn(IGlobalPosition room);

        /// <summary>
        /// Should this resource spawn in this room. Combines the "can" logic with checks against total local population
        /// </summary>
        /// <param name="room">The room to spawn in</param>
        /// <returns>if this should spawn there</returns>
        bool ShouldSpawnIn(IGlobalPosition room);

        /// <summary>
        /// Render a natural resource collection to a viewer
        /// </summary>
        /// <param name="viewer">the entity looking</param>
        /// <param name="amount">How much of it there is</param>
        /// <returns>a view string</returns>
        ILexicalParagraph RenderResourceCollection(IEntity viewer, int amount);
    }
}
