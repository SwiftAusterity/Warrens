namespace NetMud.DataStructure.Gaia
{
    public interface ICelestialPosition
    {
        ICelestial CelestialObject { get; set; }
        float Position { get; set; }
    }
}
