using NetMud.DataStructure.SupportingClasses;
using System;

namespace NetMud.Cartography
{

    /// <summary>
    /// General utilities for map rendering and parsing
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Translates degreesFromNorth into direction words for pathways
        /// </summary>
        /// <param name="degreesFromNorth">the value to translate</param>
        /// <param name="inclineGrade">the value to translate</param>
        /// <param name="reverse">reverse the direction or not</param>
        /// <returns>translated text</returns>
        public static MovementDirectionType TranslateToDirection(int degreesFromNorth, int inclineGrade = 0, bool reverse = false)
        {
            var trueDegrees = degreesFromNorth;

            if (trueDegrees < 0)
                return MovementDirectionType.None;

            if (reverse)
                trueDegrees = degreesFromNorth < 180 ? degreesFromNorth + 180 : degreesFromNorth - 180;

            if (trueDegrees > 22 && trueDegrees < 67)
            {
                if (inclineGrade > 0)
                    return MovementDirectionType.UpNorthEast;
                else if (inclineGrade < 0)
                    return MovementDirectionType.DownNorthEast;

                return MovementDirectionType.NorthEast;
            }

            if (trueDegrees > 66 && trueDegrees < 111)
            {
                if (inclineGrade > 0)
                    return MovementDirectionType.UpEast;
                else if (inclineGrade < 0)
                    return MovementDirectionType.DownEast;

                return MovementDirectionType.East;
            }

            if (trueDegrees > 110 && trueDegrees < 155)
            {
                if (inclineGrade > 0)
                    return MovementDirectionType.UpSouthEast;
                else if (inclineGrade < 0)
                    return MovementDirectionType.DownSouthEast;

                return MovementDirectionType.SouthEast;
            }

            if (trueDegrees > 154 && trueDegrees < 199)
            {
                if (inclineGrade > 0)
                    return MovementDirectionType.UpSouth;
                else if (inclineGrade < 0)
                    return MovementDirectionType.DownSouth;

                return MovementDirectionType.South;
            }

            if (trueDegrees > 198 && trueDegrees < 243)
            {
                if (inclineGrade > 0)
                    return MovementDirectionType.UpSouthWest;
                else if (inclineGrade < 0)
                    return MovementDirectionType.DownSouthWest;

                return MovementDirectionType.SouthWest;
            }

            if (trueDegrees > 242 && trueDegrees < 287)
            {
                if (inclineGrade > 0)
                    return MovementDirectionType.UpWest;
                else if (inclineGrade < 0)
                    return MovementDirectionType.DownWest;

                return MovementDirectionType.West;
            }

            if (trueDegrees > 286 && trueDegrees < 331)
            {
                if (inclineGrade > 0)
                    return MovementDirectionType.UpNorthWest;
                else if (inclineGrade < 0)
                    return MovementDirectionType.DownNorthWest;

                return MovementDirectionType.NorthWest;
            }

            if (inclineGrade > 0)
                return MovementDirectionType.UpNorth;
            else if (inclineGrade < 0)
                return MovementDirectionType.DownNorth;

            return MovementDirectionType.North;
        }

        /// <summary>
        /// Translates direction words into degreesFromNorth and inclineGrade for pathways, returns the absolute default value
        /// </summary>
        /// <param name="direction">the value to translate</param>
        /// <returns>degrees from north, incline grade</returns>
        public static Tuple<int,int> TranslateDirectionToDegrees(MovementDirectionType direction)
        {
            switch (direction)
            {
                case MovementDirectionType.East:
                    return new Tuple<int, int>(90, 0); 
                case MovementDirectionType.North:
                    return new Tuple<int, int>(0, 0);
                case MovementDirectionType.NorthEast:
                    return new Tuple<int, int>(45, 0); 
                case MovementDirectionType.NorthWest:
                    return new Tuple<int, int>(315, 0);
                case MovementDirectionType.South:
                    return new Tuple<int, int>(180, 0);
                case MovementDirectionType.SouthEast:
                    return new Tuple<int, int>(135, 0);
                case MovementDirectionType.SouthWest:
                    return new Tuple<int, int>(225, 0);
                case MovementDirectionType.West:
                    return new Tuple<int, int>(270, 0);
                case MovementDirectionType.Up:
                    return new Tuple<int, int>(-1, 25);
                case MovementDirectionType.Down:
                    return new Tuple<int, int>(-1, -25);
                case MovementDirectionType.UpEast:
                    return new Tuple<int, int>(90, 23);
                case MovementDirectionType.UpNorth:
                    return new Tuple<int, int>(0, 25);
                case MovementDirectionType.UpNorthEast:
                    return new Tuple<int, int>(45, 25);
                case MovementDirectionType.UpNorthWest:
                    return new Tuple<int, int>(315, 25);
                case MovementDirectionType.UpSouth:
                    return new Tuple<int, int>(180, 25);
                case MovementDirectionType.UpSouthEast:
                    return new Tuple<int, int>(135, 25);
                case MovementDirectionType.UpSouthWest:
                    return new Tuple<int, int>(225, 25);
                case MovementDirectionType.UpWest:
                    return new Tuple<int, int>(270, 25);
                case MovementDirectionType.DownEast:
                    return new Tuple<int, int>(90, -25);
                case MovementDirectionType.DownNorth:
                    return new Tuple<int, int>(0, -25);
                case MovementDirectionType.DownNorthEast:
                    return new Tuple<int, int>(45, -25);
                case MovementDirectionType.DownNorthWest:
                    return new Tuple<int, int>(315, -25);
                case MovementDirectionType.DownSouth:
                    return new Tuple<int, int>(180, -25);
                case MovementDirectionType.DownSouthEast:
                    return new Tuple<int, int>(135, -25);
                case MovementDirectionType.DownSouthWest:
                    return new Tuple<int, int>(225, -25);
                case MovementDirectionType.DownWest:
                    return new Tuple<int, int>(270, -25);
            }

            //return none, neutral for anything not counted
            return new Tuple<int, int>(-1, 0);
        }

        /// <summary>
        /// Translates hard directions to ascii characters. UP inclines are always brackets open to the left, DOWN is always bracket open to the right
        /// </summary>
        /// <param name="direction">the hard direction to turn into a character</param>
        /// <returns>a single ascii character in a string</returns>
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
                case MovementDirectionType.Up:
                    return "^";
                case MovementDirectionType.UpEast:
                case MovementDirectionType.UpWest:
                    return "3";
                case MovementDirectionType.UpSouth:
                case MovementDirectionType.UpNorth:
                    return "}";
                case MovementDirectionType.UpSouthWest:
                case MovementDirectionType.UpNorthEast:
                    return ">";
                case MovementDirectionType.UpSouthEast:
                case MovementDirectionType.UpNorthWest:
                    return "]";
                case MovementDirectionType.Down:
                    return "v";
                case MovementDirectionType.DownEast:
                case MovementDirectionType.DownWest:
                    return "E";
                case MovementDirectionType.DownSouth:
                case MovementDirectionType.DownNorth:
                    return "{";
                case MovementDirectionType.DownSouthWest:
                case MovementDirectionType.DownNorthEast:
                    return "<";
                case MovementDirectionType.DownSouthEast:
                case MovementDirectionType.DownNorthWest:
                    return "[";
            }
        }

        /// <summary>
        /// X, Y, Z
        /// </summary>
        /// <param name="transversalDirection">The direction being faced</param>
        /// <returns>the coordinates for the direction needed to move one unit "forward"</returns>
        public static Tuple<int, int, int> GetDirectionStep(MovementDirectionType transversalDirection)
        {
            switch (transversalDirection)
            {
                default: //We already defaulted to 0,0,0
                    break;
                case MovementDirectionType.East:
                    return new Tuple<int, int, int>(1, 0, 0);
                case MovementDirectionType.North:
                    return new Tuple<int, int, int>(0, 1, 0);
                case MovementDirectionType.NorthEast:
                    return new Tuple<int, int, int>(1, 1, 0);
                case MovementDirectionType.NorthWest:
                    return new Tuple<int, int, int>(-1, 1, 0);
                case MovementDirectionType.South:
                    return new Tuple<int, int, int>(0, -1, 0);
                case MovementDirectionType.SouthEast:
                    return new Tuple<int, int, int>(1, -1, 0);
                case MovementDirectionType.SouthWest:
                    return new Tuple<int, int, int>(-1, -1, 0);
                case MovementDirectionType.West:
                    return new Tuple<int, int, int>(-1, 0, 0);
                case MovementDirectionType.Up:
                    return new Tuple<int, int, int>(0, 0, 1);
                case MovementDirectionType.Down:
                    return new Tuple<int, int, int>(0, 0, -1);
                case MovementDirectionType.UpEast:
                    return new Tuple<int, int, int>(1, 0, 1);
                case MovementDirectionType.UpNorth:
                    return new Tuple<int, int, int>(0, 1, 1);
                case MovementDirectionType.UpNorthEast:
                    return new Tuple<int, int, int>(1, 1, 1);
                case MovementDirectionType.UpNorthWest:
                    return new Tuple<int, int, int>(-1, 1, 1);
                case MovementDirectionType.UpSouth:
                    return new Tuple<int, int, int>(0, -1, 1);
                case MovementDirectionType.UpSouthEast:
                    return new Tuple<int, int, int>(1, -1, 1);
                case MovementDirectionType.UpSouthWest:
                    return new Tuple<int, int, int>(-1, -1, 1);
                case MovementDirectionType.UpWest:
                    return new Tuple<int, int, int>(-1, 0, 1);
                case MovementDirectionType.DownEast:
                    return new Tuple<int, int, int>(1, 0, -1);
                case MovementDirectionType.DownNorth:
                    return new Tuple<int, int, int>(0, 1, -1);
                case MovementDirectionType.DownNorthEast:
                    return new Tuple<int, int, int>(1, 1, -1);
                case MovementDirectionType.DownNorthWest:
                    return new Tuple<int, int, int>(-1, 1, -1);
                case MovementDirectionType.DownSouth:
                    return new Tuple<int, int, int>(0, -1, -1);
                case MovementDirectionType.DownSouthEast:
                    return new Tuple<int, int, int>(1, -1, -1);
                case MovementDirectionType.DownSouthWest:
                    return new Tuple<int, int, int>(-1, -1, -1);
                case MovementDirectionType.DownWest:
                    return new Tuple<int, int, int>(-1, 0, -1);
            }

            return new Tuple<int, int, int>(0, 0, 0);
        }
    }
}
