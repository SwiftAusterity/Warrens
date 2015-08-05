using NetMud.DataAccess;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Data;
using System.Text;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    public class Character : ICharacter
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        public Type EntityClass
        {
            get { return typeof(Game.Player); }
        }

        /// <summary>
        /// Numerical iterative ID in the db
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// When this was first created in the db
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// When this was last revised in the db
        /// </summary>
        public DateTime LastRevised { get; set; }

        /// <summary>
        /// The unique name for this entry (also part of the accessor keywords)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gender data string for player characters
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// "family name" for player character
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// Account handle (user) this belongs to
        /// </summary>
        public string AccountHandle { get; set; }

        /// <summary>
        /// The "user" level for commands and accessibility
        /// </summary>
        public StaffRank GamePermissionsRank { get; set; }

        /// <summary>
        /// The last known location ID this character was seen in by system (for restore/backup purposes)
        /// </summary>
        public string LastKnownLocation { get; set; }
        /// <summary>
        /// The system type of the ast known location this character was seen in by system (for restore/backup purposes)
        /// </summary>
        public string LastKnownLocationType { get; set; }

        /// <summary>
        /// Account data object this is owned by
        /// </summary>
        private IAccount _account;

        /// <summary>
        /// Account data object this is owned by
        /// </summary>
        public IAccount Account
        {
            get
            {
                if (_account == null && !string.IsNullOrWhiteSpace(AccountHandle))
                    _account = System.Account.GetByHandle(AccountHandle);

                return _account;
            }
        }

        /// <summary>
        /// Full name to refer to this NPC with
        /// </summary>
        /// <returns>the full name string</returns>
        public string FullName()
        {
            return string.Format("{0} {1}", Name, SurName);
        }

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public void Fill(global::System.Data.DataRow dr)
        {
            long outId = default(long);
            DataUtility.GetFromDataRow<long>(dr, "ID", ref outId);
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
            DataUtility.GetFromDataRow<string>(dr, "Name", ref outGivenName);
            Name = outGivenName;

            string outGender = default(string);
            DataUtility.GetFromDataRow<string>(dr, "Gender", ref outGender);
            Gender = outGender;

            StaffRank outRank = StaffRank.Player;
            DataUtility.GetFromDataRow<StaffRank>(dr, "GamePermissionsRank", ref outRank);
            GamePermissionsRank = outRank;

            string outLKL = default(string);
            DataUtility.GetFromDataRow<string>(dr, "LastKnownLocation", ref outLKL);
            LastKnownLocation = outLKL;

            string outLKLT = default(string);
            DataUtility.GetFromDataRow<string>(dr, "LastKnownLocationType", ref outLKLT);
            LastKnownLocationType = outLKLT;
        }

        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IData other)
        {
            if (other != default(IData))
            {
                try
                {
                    return other.GetType() == typeof(Character) && other.ID.Equals(this.ID);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public IData Create()
        {
            ICharacter returnValue = default(ICharacter);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Character]([SurName], [Name], [AccountHandle], [Gender], [GamePermissionsRank])");
            sql.AppendFormat(" values('{0}','{1}','{2}', '{3}', {4})", SurName, Name, AccountHandle, Gender, (short)GamePermissionsRank);
            sql.Append(" select * from [dbo].[Character] where ID = Scope_Identity()");

            try
            {
                var ds = SqlWrapper.RunDataset(sql.ToString(), CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        Fill(dr);
                        returnValue = this;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return returnValue;
        }

        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public bool Remove()
        {
            var sql = new StringBuilder();
            sql.AppendFormat("delete from [dbo].[Character] where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public bool Save()
        {
            var sql = new StringBuilder();
            sql.Append("update [dbo].[Character] set ");
            sql.AppendFormat(" [SurName] = '{0}' ", SurName);
            sql.AppendFormat(" , [Name] = '{0}' ", Name);
            sql.AppendFormat(" , [AccountHandle] = '{0}' ", AccountHandle);
            sql.AppendFormat(" , [Gender] = '{0}' ", Gender);
            sql.AppendFormat(" , [GamePermissionsRank] = {0} ", (short)GamePermissionsRank);
            sql.AppendFormat(" , [LastKnownLocation] = '{0}' ", LastKnownLocation);
            sql.AppendFormat(" , [LastKnownLocationType] = '{0}' ", LastKnownLocationType);
            sql.AppendFormat(" , [LastRevised] = GetUTCDate()");
            sql.AppendFormat(" where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }
    }
}
