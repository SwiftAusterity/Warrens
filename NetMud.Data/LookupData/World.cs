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

        public World(IMap worldMap, string name)
        {
            WorldMap = worldMap;
            Name = name;
            Created = DateTime.UtcNow;
            LastRevised = DateTime.UtcNow;

            //Set the id right now
            GetNextId();
        }
    }
}
