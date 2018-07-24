using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Linq;

namespace NetMud.Gaia.Geographical
{
    public static class GeographicalUtilities
    {
        public static CrowdSizeDescription GetCrowdSize(int crowd)
        {
            if (crowd > 100)
                return CrowdSizeDescription.Tremendous;

            if (crowd > 50)
                return CrowdSizeDescription.Huge;

            if (crowd > 25)
                return CrowdSizeDescription.Large;

            if (crowd > 12)
                return CrowdSizeDescription.Moderate;

            if (crowd > 4)
                return CrowdSizeDescription.Small;

            return CrowdSizeDescription.Intimate;
        }


        public static bool IsOutside(Biome biome)
        {
            var returnValue = true;

            switch (biome)
            {
                case Biome.Aquatic:
                case Biome.AquaticFloor:
                case Biome.Cavernous:
                case Biome.Fabricated:
                    returnValue = false;
                    break;
                case Biome.Air:
                case Biome.AquaticSurface:
                case Biome.Desert:
                case Biome.Forest:
                case Biome.Mountainous:
                case Biome.Plains:
                case Biome.Rainforest:
                case Biome.Swamp:
                    break;
            }

            return returnValue;
        }

        public static DimensionalSizeDescription ConvertSizeToType(Tuple<int, int, int> Dimensions, Type entityType)
        {
            //x,y,z in inches currently
            var volume = Dimensions.Item1 * Dimensions.Item2 * Dimensions.Item3;

            //TODO: Include variances for "tall" or "long" versus "wide"
            if (entityType.GetInterfaces().Contains(typeof(IMobile)))
            {
                //TODO: variance for race, might need to add expected size to Race and pinion off that
                if (volume < 8640)
                    return DimensionalSizeDescription.Microscopic;
                else if (volume < 5640)
                    return DimensionalSizeDescription.Miniscule;
                else if (volume < 6640)
                    return DimensionalSizeDescription.Tiny;
                else if (volume < 7640)
                    return DimensionalSizeDescription.Small;
                else if (volume < 8640)
                    return DimensionalSizeDescription.Medium;
                else if (volume < 9640)
                    return DimensionalSizeDescription.Large;
                else if (volume < 10640)
                    return DimensionalSizeDescription.Huge;
                else if (volume < 11640)
                    return DimensionalSizeDescription.Massive;

                return DimensionalSizeDescription.Titanic;
            }
            else if (entityType.GetInterfaces().Contains(typeof(IInanimate)))
            {
                //TODO: variance for item type, probably need to make an entire handler for that and add a data type
                if (volume < 64)
                    return DimensionalSizeDescription.Microscopic;
                else if (volume < 84)
                    return DimensionalSizeDescription.Miniscule;
                else if (volume < 104)
                    return DimensionalSizeDescription.Tiny;
                else if (volume < 124)
                    return DimensionalSizeDescription.Small;
                else if (volume < 144)
                    return DimensionalSizeDescription.Medium;
                else if (volume < 164)
                    return DimensionalSizeDescription.Large;
                else if (volume < 184)
                    return DimensionalSizeDescription.Huge;
                else if (volume < 204)
                    return DimensionalSizeDescription.Massive;

                return DimensionalSizeDescription.Titanic;
            }
            else if (entityType.GetInterfaces().Contains(typeof(IRoom)))
            {
                if (volume < 15887872)
                    return DimensionalSizeDescription.Microscopic;
                else if (volume < 17887872)
                    return DimensionalSizeDescription.Miniscule;
                else if (volume < 19887872)
                    return DimensionalSizeDescription.Tiny;
                else if (volume < 21887872)
                    return DimensionalSizeDescription.Small;
                else if (volume < 23887872)
                    return DimensionalSizeDescription.Medium;
                else if (volume < 25887872)
                    return DimensionalSizeDescription.Large;
                else if (volume < 27887872)
                    return DimensionalSizeDescription.Huge;
                else if (volume < 29887872)
                    return DimensionalSizeDescription.Massive;

                return DimensionalSizeDescription.Titanic;
            }
            else if (entityType.GetInterfaces().Contains(typeof(IZone)))
            {
                if (volume < 500000000)
                    return DimensionalSizeDescription.Microscopic;
                else if (volume < 700000000)
                    return DimensionalSizeDescription.Miniscule;
                else if (volume < 900000000)
                    return DimensionalSizeDescription.Tiny;
                else if (volume < 1100000000)
                    return DimensionalSizeDescription.Small;
                else if (volume < 1300000000)
                    return DimensionalSizeDescription.Medium;
                else if (volume < 1500000000)
                    return DimensionalSizeDescription.Large;
                else if (volume < 1700000000)
                    return DimensionalSizeDescription.Huge;
                else if (volume < 2000000000)
                    return DimensionalSizeDescription.Massive;

                return DimensionalSizeDescription.Titanic;
            }


            return DimensionalSizeDescription.Medium;
        }

    }
}
