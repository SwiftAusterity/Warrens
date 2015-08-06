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
        public static string FlattenModel(IDimensionalModelData model, short pitch, short yaw, short roll)
        {
            var flattenedModel = new StringBuilder();

            /*
             * We start by looking at the "front" of the model which starts at Y=12, Z=1, X=12 (the upper most left corner node) and contains all the Z=1 nodes of the entire thing
             * 
             * YAW = pivot on X
             * Positive Yaw rotates the object counter-clockwise which means we start at X=(12 - Yaw) 
             * 0 Yaw - we are looking at the front face
             * 11 Yaw - we are looking at the object rotated 90 degrees (the "right" face)
             * -22/22 Yaw - we are looking at the back face
             * -11 Yaw - we are looking at the object rotated 90 degrees to the "left"
             * 
             * PITCH = pivot on Z
             * Positive pitch rotates the object forward and back which means we start at Y=(12 - Pitch)
             * 0 Pitch - we are looking at the front face
             * 12 Pitch - we are looking at the object rotated forward 90 degrees (the "bottom" face)
             * -24/24 Pitch - we are looking at the back face, upsidedown
             * -12 Pitch - we are looking at the object rotated 90 degrees backwards (the "top" face)
             * 
             * ROLL = pivot on Y
             * Positive roll "spins" the object diagonally which means we start at Z=(12 - Roll)
             * 0 Roll - we are looking at the front face
             * 12 Roll - we are looking at the object rotated diagonally 90 degrees (front face - sideways)
             * -24/24 Roll - we are looking at the object rotated diagonally 180 degrees (upsidedown)
             * -12 Roll - we are looking at the object rotated diagonally 90 degrees the other direction (sideways the other way)
             * 
             */

            //load the plane up with blanks
            List<string[]> flattenedPlane = new List<string[]>();
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });
            flattenedPlane.Add(new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " });

            //TODO: handle pitch, yaw and roll
            short startXAxis = 11;
            short startYAxis = 11;
            short startZAxis = 1;

            short xAxis;
            short yAxis;
            short zAxis;

            int xI, yI;
            for (yI = 0; yI < 11; yI++)
            {
                yAxis = (short)(startYAxis - roll - yI);
                zAxis = (short)(startZAxis - pitch);

                for (xI = 0; xI < 11; xI++)
                {
                    xAxis = (short)(startXAxis - yaw - xI);
                    zAxis = (short)(startZAxis - pitch);

                    if (xAxis < 0)
                        xAxis = (short)(11 + xAxis);
                    if (yAxis < 0)
                        yAxis = (short)(11 + yAxis);
                    if (zAxis < 0)
                        zAxis = (short)(11 + zAxis);

                    var node = model.GetNode(xAxis, yAxis, zAxis);

                    while (node != null && String.IsNullOrWhiteSpace(flattenedPlane[yI][xI]))
                    {
                        flattenedPlane[yI][xI] = DamageTypeToCharacter(node.Style, xI < 5);

                        xAxis = node.XAxis;
                        yAxis = node.YAxis;
                        zAxis = node.ZAxis;

                        node = model.GetNodeBehindNode(xAxis, yAxis, zAxis, pitch, yaw, roll);
                    }
                }
            }

            /*
            for( ; (zAxis <= endZAxis && zAxis >= startZAxis) || (zAxis >= endZAxis && zAxis <= startZAxis); )
            {
                for( ; (yAxis <= endYAxis && yAxis >= startYAxis) || (yAxis >= endYAxis && yAxis <= startYAxis); )
                {
                    for( ; (xAxis <= endXAxis && xAxis >= startXAxis) || (xAxis >= endXAxis && xAxis <= startXAxis); )
                    {
                        var node = model.GetNode(xAxis, yAxis, zAxis);

                        //We can't replace stuff we can already see, it'd be obfuscated visually
                        if (String.IsNullOrWhiteSpace(flattenedPlane[yAxis - 1][xAxis - 1]))
                            flattenedPlane[yAxis - 1][xAxis - 1] = DamageTypeToCharacter(node.Style, xAxis < 6);

                        if (xAxis.Equals(endXAxis))
                            break;

                        xAxis = xAxis < endXAxis ? (short)(xAxis + 1) : (short)(xAxis - 1);
                    }

                    xAxis = startXAxis;

                    if (yAxis.Equals(endYAxis))
                        break;

                    yAxis = yAxis < endYAxis ? (short)(yAxis + 1) : (short)(yAxis - 1);
                }

                yAxis = startYAxis;

                if (zAxis.Equals(endZAxis))
                    break;

                zAxis = zAxis < endZAxis ? (short)(zAxis + 1) : (short)(zAxis - 1);
            }

            //the system is basically upsidedown in the data so we have to flip the Y axis to get it to display correctly.
            flattenedPlane.Reverse();
            */

            flattenedModel.AppendLine();

            //Write out the flattened view to the string builder with line terminators
            foreach (var nodes in flattenedPlane)
                flattenedModel.AppendLine(string.Join("", nodes));

            flattenedModel.AppendLine();

            return flattenedModel.ToString();
        }

        /// <summary>
        /// Converts damage types to characters for use in model rendering
        /// </summary>
        /// <param name="type">the damage type to convert</param>
        /// <param name="reverseCharacter">whether or not to display the left-of-center character</param>
        /// <returns></returns>
        public static string DamageTypeToCharacter(DamageType type, bool leftOfCenter)
        {
            string returnString = " ";

            switch (type)
            {
                default: //also "none" case
                    break;
                case DamageType.Blunt:
                    returnString = "#";
                    break;
                case DamageType.Sharp:
                    if (leftOfCenter)
                        returnString = "/";
                    else
                        returnString = "\\";
                    break;
                case DamageType.Pierce:
                    returnString = "^";
                    break;
                case DamageType.Shred:
                    if (leftOfCenter)
                        returnString = "<";
                    else
                        returnString = ">";
                    break;
                case DamageType.Chop:
                    if (leftOfCenter)
                        returnString = "{";
                    else
                        returnString = "}";
                    break;
                case DamageType.Acidic:
                    returnString = "A";
                    break;
                case DamageType.Base:
                    returnString = "B";
                    break;
                case DamageType.Heat:
                    returnString = "H";
                    break;
                case DamageType.Cold:
                    returnString = "C";
                    break;
                case DamageType.Electric:
                    returnString = "E";
                    break;
                case DamageType.Positronic:
                    returnString = "P";
                    break;
                case DamageType.Endergonic:
                    returnString = "N";
                    break;
                case DamageType.Exergonic:
                    returnString = "X";
                    break;
                case DamageType.Hypermagnetic:
                    returnString = "M";
                    break;
            }

            return returnString;
        }

        /// <summary>
        /// Converts render characters to damage types
        /// </summary>
        /// <param name="chr">actually a string, the character to convert</param>
        /// <returns>the damage type</returns>
        public static DamageType CharacterToDamageType(string chr)
        {
            var returnValue = DamageType.None;

            switch (chr)
            {
                default: //also "none" case
                    break;
                case "#":
                    returnValue = DamageType.Blunt;
                    break;
                case "\\":
                case "/":
                    returnValue = DamageType.Sharp;
                    break;
                case "^":
                    returnValue = DamageType.Pierce;
                    break;
                case "<":
                case ">":
                    returnValue = DamageType.Shred;
                    break;
                case "{":
                case "}":
                    returnValue = DamageType.Chop;
                    break;
                case "A":
                    returnValue = DamageType.Acidic;
                    break;
                case "B":
                    returnValue = DamageType.Base;
                    break;
                case "H":
                    returnValue = DamageType.Heat;
                    break;
                case "C":
                    returnValue = DamageType.Cold;
                    break;
                case "E":
                    returnValue = DamageType.Electric;
                    break;
                case "P":
                    returnValue = DamageType.Positronic;
                    break;
                case "N":
                    returnValue = DamageType.Endergonic;
                    break;
                case "X":
                    returnValue = DamageType.Exergonic;
                    break;
                case "M":
                    returnValue = DamageType.Hypermagnetic;
                    break;
            }

            return returnValue;
        }
    }
}
