using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// World for holding world maps
    /// </summary>
    [Serializable]
    [IgnoreAutomatedBackup]
    public class World : BackingDataPartial, IWorld
    {
        public IMap WorldMap { get; private set; }

        public long FullDiameter { get; set; }

        public HashSet<IStratum> Strata { get; set; }

        public HashSet<IChunk> Chunks { get; set; }

        public World(IMap worldMap, string name)
        {
            WorldMap = worldMap;
            Name = name;
            Created = DateTime.UtcNow;
            LastRevised = DateTime.UtcNow;

            //Set the id right now
            GetNextId();
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (WorldMap == null)
                dataProblems.Add("World Map is null.");

            return dataProblems;
        }
    }
}
