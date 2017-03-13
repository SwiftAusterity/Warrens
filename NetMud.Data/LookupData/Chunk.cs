using NetMud.Data.Game;
using NetMud.Data.System;
using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;

namespace NetMud.Data.LookupData
{
    [Serializable]
    public class Chunk : LocationEntityPartial, IChunk
    {
        /// <summary>
        /// Upper x,y,z
        /// </summary>
        public Tuple<long, long, long> UpperBounds { get; set; }

        /// <summary>
        /// Lower x,y,z
        /// </summary>
        public Tuple<long, long, long> LowerBounds { get; set; }

        /// <summary>
        /// What world do these belong to
        /// </summary>
        public IWorld World { get; set; }

        public void GetFromWorldOrSpawn()
        {
            throw new NotImplementedException();
        }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            throw new NotImplementedException();
        }

        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException();
        }

        public override void SpawnNewInWorld(IGlobalPosition position)
        {
            throw new NotImplementedException();
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<String> RenderToLook(IEntity actor)
        {
            throw new NotImplementedException();
        }
    }
}
