using NetMud.DataStructure.Base.EntityBackingData;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    public interface IAccount
    {
        string GlobalIdentityHandle { get; set; }

        IList<ICharacter> Characters { get; set; }
        long CurrentlySelectedCharacter { get; set; }

        string AddCharacter(ICharacter newCharacter);
    }
}
