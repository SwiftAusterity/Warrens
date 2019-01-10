using NetMud.DataStructure.Administrative;

namespace NetMud.DataStructure.Architectural
{
    /// <summary>
    /// Configuration data. Only one of these spawns forever
    /// </summary>
    public interface IConfigData : IData, INeedApproval
    {
        /// <summary>
        /// The unique key used to identify, store and retrieve data
        /// </summary>
        string UniqueKey { get; }

        /// <summary>
        /// The unique name of this configuration data
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The type of data this is (for storage)
        /// </summary>
        ConfigDataType Type { get; }
    }
}
