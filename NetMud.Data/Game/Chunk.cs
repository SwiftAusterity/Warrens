using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    [Serializable]
    public class Chunk : LocationEntityPartial, IChunk
    {
        public Tuple<long, long, long> UpperBounds { get; set; }

        public Tuple<long, long, long> LowerBounds { get; set; }

        [JsonProperty("World")]
        private long _worldId { get; set; }

        [JsonIgnore]
        [ScriptIgnore]
        public IWorld World
        {
            get
            {
                return BackingDataCache.Get<IWorld>(_worldId);
            }
            set
            {
                _worldId = value.ID;
            }
        }

        public Chunk()
        {

        }

        public Chunk(Tuple<long, long, long> upperBounds, Tuple<long, long, long> lowerBounds)
        {
            UpperBounds = upperBounds;
            LowerBounds = lowerBounds;
        }

        public void GetFromWorldOrSpawn()
        {
            throw new NotImplementedException();
        }

        public override void SpawnNewInWorld()
        {

        }

        public override void SpawnNewInWorld(IGlobalPosition position)
        {
            throw new NotImplementedException("Chunks can't spawn to a position.");
        }

        public override void SpawnNewInWorld(IContains spawnTo)
        {
            throw new NotImplementedException("Chunks can't spawn to a container.");
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            int x = DataUtility.TryConvert<int>(UpperBounds.Item1 - LowerBounds.Item1);
            int y = DataUtility.TryConvert<int>(UpperBounds.Item2 - LowerBounds.Item2);
            int z = DataUtility.TryConvert<int>(UpperBounds.Item3 - LowerBounds.Item3);

            return new Tuple<int, int, int>(y, x, z);
        }

        #region Rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity actor)
        {
            var sb = new List<string>();

            return sb;
        }
        #endregion
    }
}
