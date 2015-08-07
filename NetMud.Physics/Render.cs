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
             * We start by looking at the "front" of the model which starts at Y=11, Z=1, X=11 (the upper most left corner node) and contains all the Z=1 nodes of the entire thing
             * 
             * YAW = pivot on X
             * Positive Yaw rotates the object counter-clockwise
             * yaw 1-10 - length = model xAxis 11-1, height = yAxis 11-1, depth = zAxis 1-11
             * yaw 11-21 - length = model zAxis 1-11, height = yAxis 11-1, depth = xAxis 1-11
             * yaw 22-32 - length = model xAxis 1-11, height = yAxis 11-1, depth = zAxis 11-1
             * yaw 33-43 - length = model zAxis 11-1, height = yAxis 11-1, depth = xAxis 11-1
             * 
             * PITCH = pivot on Z
             * Positive pitch rotates the object forward and back
             * pitch 1-10 - length = model xAxis 11-1, height = yAxis 11-1, depth = zAxis 1-11
             * pitch 11-21 - length = model xAxis 11-1, height = zAxis 1-11, depth = yAxis 1-11
             * pitch 22-32 - length = model xAxis 11-1, height = yAxis 1-11, depth = zAxis 11-1
             * pitch 33-43 - length = model xAxis 11-1, height = zAxis 11-1, depth = yAxis 11-1
             * 
             * ROLL = pivot on Y
             * Positive roll "spins" the object diagonally
             * roll 1-10 - length = model xAxis 11-1, height = yAxis 11-1, depth = zAxis 1-11
             * roll 11-21 - length = model yAxis 1-11, height = xAxis 1-1, depth = zAxis 1-11
             * roll 22-32 - length = model xAxis 1-11, height = yAxis 1-1, depth = zAxis 1-11
             * roll 33-43 - length = model yAxis 11-1, height = xAxis 11-1, depth = zAxis 1-11
             * 
             */

            //Figure out the change. We need to "advance" by Length and Height here (where as the "find behind node" function advances Depth only)
            var heightChanges = new short[] { 0, 0, 0 }; // X, Y, Z
            var lengthChanges = new short[] { 0, 0, 0 }; // X, Y, Z
            int startXAxis, startYAxis, startZAxis;

            //Math.DivRem(yaw, 10, out startXAxis);
            //startXAxis = 11 - startXAxis;

            //Math.DivRem(roll, 10, out startYAxis);
            //startYAxis = 11 - startYAxis;

            //Math.DivRem(pitch, 10, out startZAxis);
            //startZAxis = 1 + startZAxis;

            if (yaw > 0)
            {
                if (yaw <= 10)
                {
                    heightChanges[1]--;
                    lengthChanges[0]--;
                }
                else if (yaw <= 21)
                {
                    heightChanges[1]--;
                    lengthChanges[2]++;
                }
                else if (yaw <= 32)
                {
                    heightChanges[1]++;
                    lengthChanges[0]--;
                }
                else
                {
                    heightChanges[1]--;
                    lengthChanges[2]--;
                }
            }

            if (pitch > 0)
            {
                if (pitch <= 10)
                {
                    heightChanges[1]--;
                    lengthChanges[0]--;
                }
                else if (pitch <= 21)
                {
                    heightChanges[2]++;
                    lengthChanges[0]--;
                }
                else if (pitch <= 32)
                {
                    heightChanges[1]++;
                    lengthChanges[0]--;
                }
                else
                {
                    heightChanges[2]--;
                    lengthChanges[0]--;
                }
            }

            if (roll > 0)
            {
                if (roll <= 10)
                {
                    heightChanges[1]--;
                    lengthChanges[0]--;
                }
                else if (roll <= 21)
                {
                    heightChanges[0]++;
                    lengthChanges[1]++;
                }
                else if (roll <= 32)
                {
                    heightChanges[1]++;
                    lengthChanges[0]++;
                }
                else
                {
                    heightChanges[0]--;
                    lengthChanges[1]--;
                }
            }

            if (roll == 0 && yaw == 0 && pitch == 0)
            {
                heightChanges[1]--;
                lengthChanges[0]--;
            }

            if (heightChanges[0] > 1)
                heightChanges[0] = 1;
            if (heightChanges[0] < -1)
                heightChanges[0] = -1;

            if (heightChanges[1] > 1)
                heightChanges[1] = 1;
            if (heightChanges[1] < -1)
                heightChanges[1] = -1;

            if (heightChanges[2] > 1)
                heightChanges[2] = 1;
            if (heightChanges[2] < -1)
                heightChanges[2] = -1;

            if (lengthChanges[0] > 1)
                lengthChanges[0] = 1;
            if (lengthChanges[0] < -1)
                lengthChanges[0] = -1;

            if (lengthChanges[1] > 1)
                lengthChanges[1] = 1;
            if (lengthChanges[1] < -1)
                lengthChanges[1] = -1;

            if (lengthChanges[2] > 1)
                lengthChanges[2] = 1;
            if (lengthChanges[2] < -1)
                lengthChanges[2] = -1;

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

            short xAxis, yAxis, zAxis, xI, yI;
            for (yI = 0; yI < 11; yI++)
            {
                xAxis = (short)(startXAxis - (heightChanges[0] * yI));
                yAxis = (short)(startYAxis - (heightChanges[1] * yI));
                zAxis = (short)(startZAxis - (heightChanges[2] * yI));

                for (xI = 0; xI < 11; xI++)
                {
                    xAxis = (short)(startXAxis - (lengthChanges[0] * xI));
                    yAxis = (short)(startYAxis - (lengthChanges[1] * xI));
                    zAxis = (short)(startZAxis - (lengthChanges[2] * xI));

                    if (xAxis <= 0)
                        xAxis = (short)(11 + xAxis);
                    if (yAxis <= 0)
                        yAxis = (short)(11 + yAxis);
                    if (zAxis <= 0)
                        zAxis = (short)(11 + zAxis);

                    if (xAxis > 11)
                        xAxis = (short)(xAxis - 11); 
                    if (yAxis > 11)           
                        yAxis = (short)(yAxis - 11);
                    if (zAxis > 11)          
                        zAxis = (short)(zAxis - 11);

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
