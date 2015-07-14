using NetMud.DataAccess;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.System
{
    public class Account : IAccount
    {
        public string GlobalIdentityHandle { get; set; }

        private IList<ICharacter> _characters;
        public IEnumerable<ICharacter> Characters 
        { 
            get
            {
                if(_characters == null)
                {
                    var refWrapper = new ReferenceAccess();
                    var systemChars = refWrapper.GetAllReference<ICharacter>();

                    _characters = systemChars.Where(c => c.AccountHandle.Equals(GlobalIdentityHandle)).ToList();
                }

                return _characters;
            }
            set
            {
                _characters = value.ToList();
            }
        }

        public static IAccount GetByHandle(string handle)
        {
            var sql = String.Format("select * from [dbo].[Account] where GlobalIdentityHandle = '{0}'", handle);

            var ds = SqlWrapper.RunDataset(sql, CommandType.Text);
            IAccount account = null;

            if (ds.HasErrors)
            {
                //TODO: Error handling logging?
            }
            else if (ds.Rows != null)
            {
                foreach (DataRow dr in ds.Rows)
                {
                    try
                    {
                        account = Fill(dr);
                    }
                    catch
                    {
                        //error logging
                    }
                }
            }

            return account;
        }

        private static IAccount Fill(DataRow dr)
        {
            string outHandle = default(string);
            DataUtility.GetFromDataRow<string>(dr, "GlobalIdentityHandle", ref outHandle);

            var account = new Account();
            account.GlobalIdentityHandle = outHandle;
            return account;
        }
    }
}
