namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Tracked statistics and quest progress
    /// </summary>
    public interface IQuality
    {
        string Name { get; set; }

        int Value { get; set; }

        bool Visible { get; set; }

        QualityType Type { get; set; }
    }
}
