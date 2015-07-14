using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.SystemData
{
    public class Account : IAccount
    {
        public string GlobalIdentityHandle { get; set; }

        private IList<ICharacter> _characters;

        public IEnumerable<ICharacter> Characters { get; set; }
    }
}
