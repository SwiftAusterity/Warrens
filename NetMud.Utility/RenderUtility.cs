using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NetMud.Utility
{
    public static class RenderUtility
    {
        private const string encapsulationElement = "p";
        private const string bumperElement = "<br />";

        public static string EncapsulateOutput(IEnumerable<string> lines)
        {
            var returnString = new StringBuilder();

            foreach (var line in lines)
            {
                if (!String.IsNullOrWhiteSpace(line))
                    returnString.AppendFormat("<{0}>{1}</{0}>", encapsulationElement, line);
                else
                    returnString.Append(bumperElement); //blank strings mean carriage returns
            }

            return returnString.ToString();
        }

        public static MovementDirectionType TranslateDegreesToDirection(int degreesFromNorth)
        {
            if (degreesFromNorth > 22 && degreesFromNorth < 67)
                return MovementDirectionType.NorthEast;
            if (degreesFromNorth > 66 && degreesFromNorth < 111)
                return MovementDirectionType.East;
            if (degreesFromNorth > 110 && degreesFromNorth < 155)
                return MovementDirectionType.SouthEast;
            if (degreesFromNorth > 154 && degreesFromNorth < 199)
                return MovementDirectionType.South;
            if (degreesFromNorth > 198 && degreesFromNorth < 243)
                return MovementDirectionType.SouthWest;
            if (degreesFromNorth > 242 && degreesFromNorth < 287)
                return MovementDirectionType.West;
            if (degreesFromNorth > 286 && degreesFromNorth < 331)
                return MovementDirectionType.NorthWest;

            return MovementDirectionType.North;
        }

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
    }
}
