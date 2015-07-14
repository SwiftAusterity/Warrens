using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Game
{
    public class Character : ICharacter
    {
        public string SurName { get; set; }
        public string GivenName { get; set; }
        public long ID { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastRevised { get; set; }
        public string Name { get; set; }
        public string AccountHandle { get; set; }

        private IAccount _account;
        public IAccount Account
        {
            get 
            {
                if (_account == null && !String.IsNullOrWhiteSpace(AccountHandle))
                    _account = NetMud.Data.System.Account.GetByHandle(AccountHandle);

                return _account;
            }
        }
        public void Fill(global::System.Data.DataRow dr)
        {
            int outId = default(int);
            DataUtility.GetFromDataRow<int>(dr, "ID", ref outId);
            ID = outId;

            string outAccountHandle = default(string);
            DataUtility.GetFromDataRow<string>(dr, "AccountHandle", ref outAccountHandle);
            AccountHandle = outAccountHandle;

            DateTime outCreated = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "Created", ref outCreated);
            Created = outCreated;

            DateTime outRevised = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised", ref outRevised);
            LastRevised = outRevised;

            string outName = default(string);
            DataUtility.GetFromDataRow<string>(dr, "Name", ref outName);
            Name = outName;

            string outSurName = default(string);
            DataUtility.GetFromDataRow<string>(dr, "SurName", ref outSurName);
            SurName = outSurName;

            string outGivenName = default(string);
            DataUtility.GetFromDataRow<string>(dr, "GivenName", ref outGivenName);
            GivenName = outGivenName;
        }

        public IEnumerable<string> RenderHelpBody()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object obj)
        {
            if (obj != null)
            {
                try
                {
                    if (obj.GetType() != typeof(Character))
                        return -1;

                    IReference otherObj = obj as IReference;

                    if (otherObj.ID.Equals(this.ID))
                        return 1;

                    return 0;
                }
                catch
                {
                    //Minor error logging
                }
            }

            return -99;
        }

        public bool Equals(IReference other)
        {
            if (other != default(IReference))
            {
                try
                {
                    return other.GetType() == typeof(Character) && other.ID.Equals(this.ID);
                }
                catch
                {
                    //Minor error logging
                }
            }

            return false;
        }
    }
}
