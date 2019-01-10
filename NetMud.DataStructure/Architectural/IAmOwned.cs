using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Player;

namespace NetMud.DataStructure.Architectural
{
    /// <summary>
    /// Methods for determining ownership and permissions to modify live things
    /// </summary>
    public interface IAmOwned
    {
        /// <summary>
        /// Who created this thing, their GlobalAccountHandle
        /// </summary>
        string CreatorHandle { get; }

        /// <summary>
        /// Who created this thing
        /// </summary>
        IAccount Creator { get; }

        /// <summary>
        /// The creator's account permissions level
        /// </summary>
        StaffRank CreatorRank { get; }
    }
}
