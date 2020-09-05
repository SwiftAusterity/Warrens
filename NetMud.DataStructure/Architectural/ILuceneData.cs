using NetMud.DataStructure.Administrative;

namespace NetMud.DataStructure.Architectural
{
    /// <summary>
    /// Lucene mapped data. Only one of these spawns forever
    /// </summary>
    public interface ILuceneData : IData, INeedApproval
    {
        /// <summary>
        /// The unique key used to identify, store and retrieve data
        /// </summary>
        string UniqueKey { get; }

        /// <summary>
        /// The unique name of this configuration data
        /// </summary>
        string Name { get; set; }
    }
}
