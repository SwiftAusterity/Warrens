using NetMud.DataStructure.Architectural.EntityBase;
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
            switch (model.ModelType)
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
                    {
                        returnString = "/";
                    }
                    else
                    {
                        returnString = @"\";
                    }

                    break;
                case DamageType.Pierce:
                    returnString = "^";
                    break;
                case DamageType.Shred:
                    if (leftOfCenter)
                    {
                        returnString = "<";
                    }
                    else
                    {
                        returnString = ">";
                    }

                    break;
                case DamageType.Chop:
                    if (leftOfCenter)
                    {
                        returnString = "{";
                    }
                    else
                    {
                        returnString = "}";
                    }

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
            DamageType returnValue = DamageType.None;

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
            StringBuilder flattenedModel = new();

            //load the plane up with blanks
            List<string[]> flattenedPlane = new()
            {
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " },
                new string[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " }
            };

            short yI;
            for (yI = 0; yI < 21; yI++)
            {
                short xI = 0;

                flattenedPlane[yI][xI] = GetNodeString(xI, yI, model, forWeb);
                flattenedPlane[yI][xI + 1] = GetNodeString(xI + 1, yI, model, forWeb);
                flattenedPlane[yI][xI + 2] = GetNodeString(xI + 2, yI, model, forWeb);
                flattenedPlane[yI][xI + 3] = GetNodeString(xI + 3, yI, model, forWeb);
                flattenedPlane[yI][xI + 4] = GetNodeString(xI + 4, yI, model, forWeb);
                flattenedPlane[yI][xI + 5] = GetNodeString(xI + 5, yI, model, forWeb);
                flattenedPlane[yI][xI + 6] = GetNodeString(xI + 6, yI, model, forWeb);
                flattenedPlane[yI][xI + 7] = GetNodeString(xI + 7, yI, model, forWeb);
                flattenedPlane[yI][xI + 8] = GetNodeString(xI + 8, yI, model, forWeb);
                flattenedPlane[yI][xI + 9] = GetNodeString(xI + 9, yI, model, forWeb);
                flattenedPlane[yI][xI + 10] = GetNodeString(xI + 10, yI, model, forWeb);
                flattenedPlane[yI][xI + 11] = GetNodeString(xI + 11, yI, model, forWeb);
                flattenedPlane[yI][xI + 12] = GetNodeString(xI + 12, yI, model, forWeb);
                flattenedPlane[yI][xI + 13] = GetNodeString(xI + 13, yI, model, forWeb);
                flattenedPlane[yI][xI + 14] = GetNodeString(xI + 14, yI, model, forWeb);
                flattenedPlane[yI][xI + 15] = GetNodeString(xI + 15, yI, model, forWeb);
                flattenedPlane[yI][xI + 16] = GetNodeString(xI + 16, yI, model, forWeb);
                flattenedPlane[yI][xI + 17] = GetNodeString(xI + 17, yI, model, forWeb);
                flattenedPlane[yI][xI + 18] = GetNodeString(xI + 18, yI, model, forWeb);
                flattenedPlane[yI][xI + 19] = GetNodeString(xI + 19, yI, model, forWeb);
                flattenedPlane[yI][xI + 20] = GetNodeString(xI + 20, yI, model, forWeb);
            }

            flattenedModel.AppendLine();

            //Write out the flattened view to the string builder with line terminators
            foreach (string[] nodes in flattenedPlane)
            {
                flattenedModel.AppendLine(string.Join("", nodes));
            }

            flattenedModel.AppendLine();

            return flattenedModel.ToString();
        }

        private static string GetNodeString(int x, short y, IDimensionalModelData model, bool forWeb)
        {
            IDimensionalModelNode node = model.GetNode((short)(x + 1), (short)(y + 1));

            string nodeString = string.Empty;

            if (node != null)
            {
                nodeString = DamageTypeToCharacter(node.Style, x < 5);

                if (forWeb)
                {
                    nodeString = string.Format("<a title='{0}'>{1}</a>"
                        , node.Composition == null ? string.Empty : node.Composition.Name
                        , nodeString);
                }
            }
            else if (forWeb)
            {
                nodeString = "<a title=''> </a>";
            }

            return nodeString;
        }
    }
}
