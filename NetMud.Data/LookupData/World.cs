﻿using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
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

            FillRoomDimensions();
        }

        private void FillRoomDimensions()
        {
            if (WorldMap == null || WorldMap.CoordinatePlane == null)
                return;

            int x, y, z;
            for (x = 0; x <= WorldMap.CoordinatePlane.GetUpperBound(0); x++)
                for (y = 0; y <= WorldMap.CoordinatePlane.GetUpperBound(1); y++)
                    for (z = 0; z <= WorldMap.CoordinatePlane.GetUpperBound(2); z++)
                    {
                        if (WorldMap.CoordinatePlane[x, y, z] <= 0)
                            continue;

                        var room = BackingDataCache.Get<IRoomData>(WorldMap.CoordinatePlane[x, y, z]);

                        if (room == null)
                            continue;

                        room.Coordinates = new Tuple<int, int, int>(x, y, z);
                    }
        }
    }
}