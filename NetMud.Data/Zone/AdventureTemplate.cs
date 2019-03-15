using NetMud.Data.Architectural;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Zone;

namespace NetMud.Data.Zone
{
    public class AdventureTemplate : LookupDataPartial, IAdventureTemplate
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

    }
}
