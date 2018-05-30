using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMud.Physics
{
    /// <summary>
    /// Render engine for dimensional models
    /// </summary>
    public static class Render
    {
        /// <summary>
        /// Flattens a dimensional model for display on the web (with material tooltips)
        /// </summary>
        /// <param name="model">the model to flatten</param>
        /// <returns>the flattened view</returns>
        public static string FlattenModelForWeb(IDimensionalModelData model)
        {
            switch(model.ModelType)
            {
                case DimensionalModelType.Flat:
                    return FlattenFlatModel(model, true);
            }

            return string.Empty;
        }

        /// <summary>
        /// Flattens a dimensional model for display
        /// </summary>
        /// <param name="model">the model to flatten</param>
        /// <returns>the flattened view</returns>
        public static string FlattenModel(IDimensionalModelData model)
        {
            switch (model.ModelType)
            {
                case DimensionalModelType.Flat:
                    return FlattenFlatModel(model);
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts damage types to characters for use in model rendering
        /// </summary>
        /// <param name="type">the damage type to convert</param>
        /// <param name="reverseCharacter">whether or not to display the left-of-center character</param>
        /// <returns></returns>
        public static string DamageTypeToCharacter(DamageType type, bool leftOfCenter = false)
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
                        returnString = @"\";
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
                case @"\":
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

        private static string FlattenFlatModel(IDimensionalModelData model, bool forWeb = false)
        {
            var flattenedModel = new StringBuilder();

            //load the plane up with blanks
            List<string[]> flattenedPlane = new List<string[]>
            {
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " }
            };

            short xI, yI;
            for (yI = 0; yI < 11; yI++)
            {
                for (xI = 0; xI < 11; xI++)
                {
                    short xIs = (short)(xI + 1);
                    short yIs = (short)(yI + 1);

                    var node = model.GetNode(xIs, yIs);

                    var nodeString = DamageTypeToCharacter(node.Style, xI < 5);

                    if (forWeb)
                        nodeString = string.Format("<a title='{0}'>{1}</a>"
                            , node.Composition == null ? string.Empty : node.Composition.Name 
                            , nodeString);

                    flattenedPlane[yI][xI] = nodeString;
                }
            }

            flattenedModel.AppendLine();

            //Write out the flattened view to the string builder with line terminators
            foreach (var nodes in flattenedPlane)
                flattenedModel.AppendLine(string.Join("", nodes));

            flattenedModel.AppendLine();

            return flattenedModel.ToString();
        }
    }
}
