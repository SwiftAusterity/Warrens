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

        public long LastKnownLocation { get; set; }
        public string LastKnownLocationType { get; set; }

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

            int outLKL = default(int);
            DataUtility.GetFromDataRow<int>(dr, "LastKnownLocation", ref outLKL);
            LastKnownLocation = outLKL;

            string outLKLT = default(string);
            DataUtility.GetFromDataRow<string>(dr, "LastKnownLocationType", ref outLKLT);
            LastKnownLocationType = outLKLT;
        }


        public int CompareTo(IData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != typeof(Character))
                        return -1;

                    if (other.ID.Equals(this.ID))
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

        public bool Equals(IData other)
        {
            if (other != default(IData))
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
            var sql = new StringBuilder();
            sql.AppendFormat("remove from [dbo].[Character] where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        public bool Save()
        {
            var sql = new StringBuilder();
            sql.Append("update [dbo].[Character] set ");
            sql.AppendFormat(" [SurName] = '{0}' ", SurName);
            sql.AppendFormat(" , [GivenName] = '{0}' ", GivenName);
            sql.AppendFormat(" , [AccountHandle] = '{0}' ", AccountHandle);
            sql.AppendFormat(" , [LastKnownLocation] = {0} ", LastKnownLocation);
            sql.AppendFormat(" , [LastKnownLocationType] = '{0}' ", LastKnownLocationType);
            sql.AppendFormat(" , [LastRevised] = GetUTCDate()");
            sql.AppendFormat(" where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }
    }
}
