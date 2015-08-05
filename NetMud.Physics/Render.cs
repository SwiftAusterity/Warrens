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

            //TODO: handle pitch, yaw and roll
            short xAxis = 11;
            short yAxis = 11;
            short zAxis = 1;

            short endXAxis = 1;
            short endYAxis = 1;
            short endZAxis = 11;
            
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

            for( ; zAxis != endZAxis; )
            {
                for( ; yAxis != endYAxis; )
                {
                    for( ; xAxis != endXAxis; )
                    {
                        var node = model.GetNode(xAxis, yAxis, zAxis);

                        //We can't replace stuff we can already see, it'd be obfuscated visually
                        if (String.IsNullOrWhiteSpace(flattenedPlane[yAxis - 1][xAxis - 1]))
                            flattenedPlane[yAxis - 1][xAxis - 1] = DamageTypeToCharacter(node.Style);

                        xAxis = xAxis < endXAxis ? (short)(xAxis + 1) : (short)(xAxis - 1);
                    }

                    yAxis = yAxis < endYAxis ? (short)(yAxis + 1) : (short)(yAxis - 1);
                }

                zAxis = zAxis < endZAxis ? (short)(zAxis + 1) : (short)(zAxis - 1);
            }
            
            //Write out the flattened view to the string builder with line terminators
            foreach (var nodes in flattenedPlane)
                flattenedModel.AppendLine(string.Join("", nodes));

            return flattenedModel.ToString();
        }

        public static string DamageTypeToCharacter(DamageType type)
        {
            string returnString = " ";

            switch(type)
            {
                default: //also "none" case
                    break;
                case DamageType.Blunt:
                    returnString = "@";
                    break;
                case DamageType.Sharp:
                    returnString = "/";
                    break;
                case DamageType.Pierce:
                    returnString = "^";
                    break;
                case DamageType.Shred:
                    returnString = ">";
                    break;
                case DamageType.Chop:
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

        public static DamageType CharacterToDamageType(string chr)
        {
            var returnValue = DamageType.None;

            switch (chr)
            {
                default: //also "none" case
                    break;
                case "@":
                    returnValue = DamageType.Blunt;
                    break;
                case "/":
                    returnValue = DamageType.Sharp;
                    break;
                case "^":
                    returnValue = DamageType.Pierce;
                    break;
                case ">":
                    returnValue = DamageType.Shred;
                    break;
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
