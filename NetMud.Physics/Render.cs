using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Physics
{
    /// <summary>
    /// Render engine for dimensional models
    /// </summary>
    public static class Render
    {
        /// <summary>
        /// Flattens a dimensional model for display
        /// </summary>
        /// <param name="model">the model to flatten</param>
        /// <param name="pitch">rotation on the z-axis</param>
        /// <param name="yaw">rotation on the Y-axis</param>
        /// <param name="roll">rotation on the x-axis</param>
        /// <returns>the flattened view</returns>
        public static string FlattenModel(IDimensionalModel model, short pitch, short yaw, short roll)
        {
            var flattenedModel = new StringBuilder();

            /*
             * We start by looking at the "front" of the model which starts at Y=12, Z=1, X=12 (the upper most left corner node) and contains all the Z=1 nodes of the entire thing
             * 
             * YAW
             * Positive Yaw rotates the object counter-clockwise which means we start at X=(12 - Yaw) 
             * 0 Yaw - we are looking at the front face
             * 12 Yaw - we are looking at the object rotated 90 degrees (the "right" face)
             * -24/24 Yaw - we are looking at the back face
             * -12 Yaw - we are looking at the object rotated 90 degrees to the "left"
             * 
             * PITCH
             * Positive pitch rotates the object forward and back which means we start at Y=(12 - Pitch)
             * 0 Pitch - we are looking at the front face
             * 12 Pitch - we are looking at the object rotated forward 90 degrees (the "bottom" face)
             * -24/24 Pitch - we are looking at the back face, upsidedown
             * -12 Pitch - we are looking at the object rotated 90 degrees backwards (the "top" face)
             * 
             * ROLL
             * Positive roll "spins" the object diagonally which means we start at Z=(12 - Roll)
             * 0 Roll - we are looking at the front face
             * 12 Roll - we are looking at the object rotated diagonally 90 degrees (front face - sideways)
             * -24/24 Roll - we are looking at the object rotated diagonally 180 degrees (upsidedown)
             * -12 Roll - we are looking at the object rotated diagonally 90 degrees the other direction (sideways the other way)
             * 
             */

            return flattenedModel.ToString();
        }
    }
}
