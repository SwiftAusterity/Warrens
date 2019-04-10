using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.Physics
{
    /// <summary>
    /// Render engine for dimensional models
    /// </summary>
    public static class Render
    {
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
    }
}
