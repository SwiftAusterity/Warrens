using NetMud.DataStructure.SupportingClasses;
namespace NetMud.Utility
{
    /// <summary>
    /// Utilities for rendering output
    /// </summary>
    public static class RenderUtility
    {
        #region Extensions
        /// <summary>
        /// Pads a string with characters
        /// </summary>
        /// <param name="str">the string to pad</param>
        /// <param name="padAmount">how many to pad with</param>
        /// <param name="padString">what you're padding with</param>
        /// <param name="toTheLeft">is this to the left or right</param>
        /// <returns>the padded string</returns>
        public static string PadWithString(this string str, int padAmount, string padString, bool toTheLeft)
        {
            while (padAmount > 0)
            {
                if (toTheLeft)
                    str = padString + str;
                else
                    str = str + padString;

                padAmount--;
            }

            return str;
        }
        #endregion

        #region "AsciiMapping"
        /// <summary>
        /// Translates degreesFromNorth into direction words for pathways
        /// </summary>
        /// <param name="degreesFromNorth">the value to translate</param>
        /// <param name="reverse">reverse the direction or not</param>
        /// <returns>translated text</returns>
        public static MovementDirectionType TranslateDegreesToDirection(int degreesFromNorth, bool reverse = false)
        {
            var trueDegrees = degreesFromNorth;

            if (trueDegrees < 0)
                return MovementDirectionType.None;

            if (reverse)
                trueDegrees = degreesFromNorth < 180 ? degreesFromNorth + 180 : degreesFromNorth - 180;

            if (trueDegrees > 22 && trueDegrees < 67)
                return MovementDirectionType.NorthEast;
            if (trueDegrees > 66 && trueDegrees < 111)
                return MovementDirectionType.East;
            if (trueDegrees > 110 && trueDegrees < 155)
                return MovementDirectionType.SouthEast;
            if (trueDegrees > 154 && trueDegrees < 199)
                return MovementDirectionType.South;
            if (trueDegrees > 198 && trueDegrees < 243)
                return MovementDirectionType.SouthWest;
            if (trueDegrees > 242 && trueDegrees < 287)
                return MovementDirectionType.West;
            if (trueDegrees > 286 && trueDegrees < 331)
                return MovementDirectionType.NorthWest;

            return MovementDirectionType.North;
        }

        /// <summary>
        /// Translates direction words into degreesFromNorth for pathways
        /// </summary>
        /// <param name="direction">the value to translate</param>
        /// <returns>degrees from north</returns>
        public static int TranslateDirectionToDegrees(MovementDirectionType direction)
        {
            switch (direction)
            {
                default:
                    return -1;
                case MovementDirectionType.East:
                    return 90;
                case MovementDirectionType.North:
                    return 0;
                case MovementDirectionType.NorthEast:
                    return 45;
                case MovementDirectionType.NorthWest:
                    return 315;
                case MovementDirectionType.South:
                    return 180;
                case MovementDirectionType.SouthEast:
                    return 135;
                case MovementDirectionType.SouthWest:
                    return 225;
                case MovementDirectionType.West:
                    return 270;
            }
        }

        public static string TranslateDirectionToAsciiCharacter(MovementDirectionType direction)
        {
            switch (direction)
            {
                default:
                    return "#";
                case MovementDirectionType.West:
                case MovementDirectionType.East:
                    return "-";
                case MovementDirectionType.South:
                case MovementDirectionType.North:
                    return "|";
                case MovementDirectionType.SouthWest:
                case MovementDirectionType.NorthEast:
                    return "/";
                case MovementDirectionType.SouthEast:
                case MovementDirectionType.NorthWest:
                    return @"\";
            }
        }
        #endregion
    }
}
