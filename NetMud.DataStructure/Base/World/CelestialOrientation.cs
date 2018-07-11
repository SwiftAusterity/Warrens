namespace NetMud.DataStructure.Base.World
{
    /// <summary>
    /// Types of orbits
    /// </summary>
    public enum CelestialOrientation : short
    {
        HelioCentric = 0,
        Fixed = 1,
        EllipticalOrbit = 2,
        GeostationaryOrbit = 3,
        GeosynchronousOrbit = 4
    }
}
