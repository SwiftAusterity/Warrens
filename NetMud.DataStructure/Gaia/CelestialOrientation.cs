namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Types of orbits
    /// </summary>
    public enum CelestialOrientation : short
    {
        SolarBody = 0, //is the solar body
        ExtraSolar = 1, //extra-solar bodies like constellations
        EllipticalOrbit = 2,
        GeostationaryOrbit = 3,
        GeosynchronousOrbit = 4,
        HelioCentric = 5 //orbits the solar body of the system
    }
}
