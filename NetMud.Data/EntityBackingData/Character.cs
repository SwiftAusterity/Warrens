using NetMud.Data.Reference;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    [Serializable]
    public class Character : EntityBackingDataPartial, ICharacter
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        public override Type EntityClass
        {
            get { return typeof(Game.Player); }
        }

        /// <summary>
        /// Gender data string for player characters
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// "family name" for player character
        /// </summary>
        public string SurName { get; set; }

        public Boolean StillANoob { get; set; }

        public IRace RaceData { get; set; }

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
        [NonSerialized]
        private IAccount _account;

        /// <summary>
        /// Account data object this is owned by
        /// </summary>
        public IAccount Account
        {
            get
            {
                if (_account == null && !string.IsNullOrWhiteSpace(AccountHandle))
                    _account = Authentication.Account.GetByHandle(AccountHandle);

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
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            var height = RaceData.Head.Model.Height + RaceData.Torso.Model.Height + RaceData.Legs.Item1.Model.Height;
            var length = RaceData.Torso.Model.Length;
            var width = RaceData.Torso.Model.Width;

            return new Tuple<int, int, int>(height, length, width);
        }

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public override void Fill(DataRow dr)
        {
            ID = DataUtility.GetFromDataRow<long>(dr, "ID");
            Created = DataUtility.GetFromDataRow<DateTime>(dr, "Created");
            LastRevised = DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised");
            Name = DataUtility.GetFromDataRow<string>(dr, "Name");

            SurName = DataUtility.GetFromDataRow<string>(dr, "SurName"); ;
            Gender = DataUtility.GetFromDataRow<string>(dr, "Gender"); ;

            StillANoob = DataUtility.GetFromDataRow<bool>(dr, "StillANoob"); ;

            var raceId = DataUtility.GetFromDataRow<long>(dr, "Race"); ;
            RaceData = ReferenceWrapper.GetOne<Race>(raceId);

            AccountHandle = DataUtility.GetFromDataRow<string>(dr, "AccountHandle");
            GamePermissionsRank = DataUtility.GetFromDataRow<StaffRank>(dr, "GamePermissionsRank");

            LastKnownLocation = DataUtility.GetFromDataRow<string>(dr, "LastKnownLocation");
            LastKnownLocationType = DataUtility.GetFromDataRow<string>(dr, "LastKnownLocationType");
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            var parms = new Dictionary<string, object>();

            ICharacter returnValue = default(ICharacter);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Character]([SurName], [Name], [AccountHandle], [Gender], [GamePermissionsRank], [Race], [LastKnownLocation], [LastKnownLocationType], [StillANoob])");
            sql.Append(" values(@SurName, @Name, @AccountHandle, @Gender, @GamePermissionsRank, @Race, @LastKnownLocation, @LastKnownLocationType, @StillANoob)");
            sql.Append(" select * from [dbo].[Character] where ID = Scope_Identity()");

            parms.Add("Name", Name);
            parms.Add("SurName", SurName);
            parms.Add("Gender", Gender);
            parms.Add("Race", RaceData.ID);
            parms.Add("AccountHandle", AccountHandle);
            parms.Add("GamePermissionsRank", (short)GamePermissionsRank);
            parms.Add("LastKnownLocation", LastKnownLocation);
            parms.Add("LastKnownLocationType", LastKnownLocationType);
            parms.Add("StillANoob", StillANoob);

            try
            {
                var ds = SqlWrapper.RunDataset(sql.ToString(), CommandType.Text, parms);

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
        public override bool Remove()
        {
            var parms = new Dictionary<string, object>();

            var sql = new StringBuilder();
            sql.AppendFormat("delete from [dbo].[Character] where ID = @id", ID);

            parms.Add("id", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text, parms);

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool Save()
        {
            var parms = new Dictionary<string, object>();

            var sql = new StringBuilder();
            sql.Append("update [dbo].[Character] set ");
            sql.Append(" [SurName] = @SurName ");
            sql.Append(" , [Name] = @Name ");
            sql.Append(" , [AccountHandle] = @AccountHandle ");
            sql.Append(" , [Gender] = @Gender ");
            sql.Append(" , [Race] = @Race ");
            sql.Append(" , [StillANoob] = @StillANoob ");
            sql.Append(" , [GamePermissionsRank] = @GamePermissionsRank ");
            sql.Append(" , [LastKnownLocation] = @LastKnownLocation ");
            sql.Append(" , [LastKnownLocationType] = @LastKnownLocationType ");
            sql.Append(" , [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("SurName", SurName);
            parms.Add("Gender", Gender);
            parms.Add("Race", RaceData.ID);
            parms.Add("AccountHandle", AccountHandle);
            parms.Add("GamePermissionsRank", (short)GamePermissionsRank);
            parms.Add("LastKnownLocation", LastKnownLocation);
            parms.Add("LastKnownLocationType", LastKnownLocationType);
            parms.Add("StillANoob", StillANoob);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text, parms);

            return true;
        }
    }
}
