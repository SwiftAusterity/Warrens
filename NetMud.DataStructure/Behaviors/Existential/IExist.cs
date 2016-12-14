using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Behaviors.Existential
{
    /// <summary>
    /// Encapsulates position in the world
    /// </summary>
    public interface IExist : IHasPositioning
    {
        /// <summary>
        /// Handles returning container's position if inside of something
        /// </summary>
        /// <returns>positional coordinates</returns>
        long[, ,] AbsolutePosition();

        /// <summary>
        /// Change the position of this
        /// </summary>
        /// <param name="direction">the 0-360 direction we're moving</param>
        /// <param name="incline">-90 to 90 incline are we moving up or down as well? Terrain will take care of natural incline changes</param>
        /// <param name="distance">how far are we moving</param>
        /// <param name="changeFacing">should the thing's facing rotate towards the direction?</param>
        /// <returns>was this thing moved?</returns>
        bool Move(int direction, int incline, int distance, bool changeFacing);

        /// <summary>
        /// Reposition entirely without moving
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="z">z coordinate</param>
        /// <param name="facing">where the thing should be facing in the end</param>
        /// <returns>success</returns>
        bool Reposition(long x, long y, long z, Tuple<int, int> facing);

        /// <summary>
        /// Spawns a new instance of this entity in the live world into a default position
        /// </summary>
        void SpawnNewInWorld();

        /// <summary>
        /// Spawn a new instance of this entity into the live world in a set position
        /// </summary>
        /// <param name="position">x,y,z coordinates to spawn into</param>
        void SpawnNewInWorld(long[, ,] position);

        /// <summary>
        /// Spawns a new instance of this entity in the live world to a specified container
        /// </summary>
        /// <param name="spawnTo">the container to spawn this into</param>
        void SpawnNewInWorld(IContains spawnTo);
    }
}
