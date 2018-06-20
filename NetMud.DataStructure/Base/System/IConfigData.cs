namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Configuration data. Only one of these spawns forever
    /// </summary>
    public interface IConfigData : IData
    {
        /// <summary>
        /// The unique name of this configuration data
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The type of data this is (for storage)
        /// </summary>
        ConfigDataType Type { get; set; }
    }
}
