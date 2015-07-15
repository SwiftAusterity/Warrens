using NetMud.DataAccess;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Game
{
    public class Character : ICharacter
    {
        public long ID { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastRevised { get; set; }

        public string SurName { get; set; }
        public string GivenName { get; set; }
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

        public string FullName()
        {
            return String.Format("{0} {1}", GivenName, SurName);
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

            string outSurName = default(string);
            DataUtility.GetFromDataRow<string>(dr, "SurName", ref outSurName);
            SurName = outSurName;

            string outGivenName = default(string);
            DataUtility.GetFromDataRow<string>(dr, "GivenName", ref outGivenName);
            GivenName = outGivenName;
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

        public IData Create()
        {
            ICharacter returnValue = default(ICharacter);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Character]([SurName], [GivenName], [AccountHandle])");
            sql.AppendFormat(" values('{0}','{1}','{2}')", SurName, GivenName, AccountHandle);
            sql.Append(" select * from [dbo].[Character] where ID = Scope_Identity()");

            var ds = SqlWrapper.RunDataset(sql.ToString(), CommandType.Text);

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
                        Fill(dr);
                        returnValue = this;
                    }
                    catch
                    {
                        //error logging
                    }
                }
            }

            return returnValue;

        }

        public bool Remove()
        {
            throw new NotImplementedException();
        }

        public string BirthMark
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime Birthdate
        {
            get { throw new NotImplementedException(); }
        }

        public string Keywords
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IReference ReferenceTemplate
        {
            get { throw new NotImplementedException(); }
        }
    }
}
