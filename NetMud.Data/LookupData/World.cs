using NetMud.Data.System;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.SupportingClasses;
using System;

namespace NetMud.Data.LookupData
{
    [Serializable]
    [IgnoreAutomatedBackup]
    public class World : BackingDataPartial, IWorld
    {
        public IMap WorldMap { get; private set; }

        public World(IMap worldMap)
        {
            WorldMap = worldMap;
        }
    }
}
