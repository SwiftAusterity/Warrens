using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Zone;
using System;

namespace NetMud.Gaia.Geographical
{
    public static class AstronomicalUtilities
    {
        public static float GetCelestialLuminosityModifier(ICelestial celestial, float celestialOrbitPosition, float rotationalPosition, float orbitalPosition
                                                         , HemispherePlacement hemisphere, float rotationalAngle)
        {
            //wtf nonono
            if (celestial.Apogee == 0 || celestial.Perigree == 0)
            {
                return 0;
            }

            //TODO: This only works for things orbiting the world (or heliocentric) right now
            float distanceFromWorld = (float)celestial.Apogee;
            int orbitalRadius = (celestial.Apogee + celestial.Perigree) / 2;
            float fullOrbitDistance = (float)Math.PI * (float)Math.Pow(orbitalRadius, 2);

            if (celestial.OrientationType != CelestialOrientation.SolarBody && celestial.OrientationType != CelestialOrientation.ExtraSolar)
            {
                distanceFromWorld = Math.Min(celestial.Perigree, (fullOrbitDistance / celestialOrbitPosition) * orbitalRadius);
            }
            else //in fixedPosition world orbits you! This is sort of a hack to force the multiplier against rotational position to = 1
            {
                celestialOrbitPosition = fullOrbitDistance;
            }

            /*
            * So we're taking the planetary rotation to mean some things here:
            * 
            * 0/360 = Eastern Hemisphere is facing out, Western is facing the sun. North/South depends on the angle.
            * 
            */

            float portionalModifier = (float)Math.Max(.001, celestialOrbitPosition / fullOrbitDistance) * (1 + rotationalPosition / 90);

            return portionalModifier * (1000 / distanceFromWorld);
        }
    }
}
