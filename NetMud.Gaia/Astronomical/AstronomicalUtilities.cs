using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.World;
using System;

namespace NetMud.Gaia.Geographical
{
    public static class AstronomicalUtilities
    {
        public static float GetCelestialLuminosityModifier(ICelestial celestial, float celestialOrbitPosition, float rotationalPosition, float orbitalPosition
                                                         , HemispherePlacement hemisphere, float rotationalAngle)
        {
            //TODO: This only works for things orbiting the world (or heliocentric) right now
            var distance = (float)celestial.Apogee;
            var orbitalRadius = (celestial.Apogee + celestial.Perigree) / 2;
            float fullOrbitDistance = (float)Math.PI * (orbitalRadius ^ 2);

            if (celestial.OrientationType != CelestialOrientation.SolarBody && celestial.OrientationType != CelestialOrientation.ExtraSolar)
            {
                distance = Math.Min(celestial.Perigree, (fullOrbitDistance / celestialOrbitPosition) * orbitalRadius);
            }

            /*
             * So we're taking the planetary rotation to mean some things here:
             * 
             * 0/360 = Eastern Hemisphere is facing out, Western is facing the sun. North/South depends on the angle.
             * 
             */

            var portionalModifier = (celestialOrbitPosition / fullOrbitDistance) * (rotationalPosition / 360);

            return portionalModifier * (distance / 10000);
        }
    }
}
