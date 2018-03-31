using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Game
{
    public class Locale : LocationEntityPartial, ILocale
    {
        public IMap InteriorMap => throw new NotImplementedException();

        public IMap Interior { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IRoom CentralRoom(int zIndex = -1)
        {
            throw new NotImplementedException();
        }

        public Tuple<int, int, int> Diameter()
        {
            throw new NotImplementedException();
        }

        public Tuple<int, int, int> FullDimensions()
        {
            throw new NotImplementedException();
        }

        public void GetFromWorldOrSpawn()
        {
            throw new NotImplementedException();
        }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            throw new NotImplementedException();
        }

        public Dictionary<IRoom, IZone> LocaleExitPoints()
        {
            throw new NotImplementedException();
        }

        public string RenderMap(int zIndex, bool forAdmin = false)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IRoom> Rooms()
        {
            throw new NotImplementedException();
        }

        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException();
        }

        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            throw new NotImplementedException();
        }

        public Dictionary<IRoom, IZone> ZoneExitPoints()
        {
            throw new NotImplementedException();
        }
    }
}
