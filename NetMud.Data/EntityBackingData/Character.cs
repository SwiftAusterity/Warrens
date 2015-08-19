using NetMud.Data.Reference;
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
        public override void Fill(DataRow dr)
        {
            ID = DataUtility.GetFromDataRow<long>(dr, "ID");
            Created = DataUtility.GetFromDataRow<DateTime>(dr, "Created");
            LastRevised = DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised");
            Name = DataUtility.GetFromDataRow<string>(dr, "Name");

            SurName = DataUtility.GetFromDataRow<string>(dr, "SurName"); ;
            Gender = DataUtility.GetFromDataRow<string>(dr, "Gender"); ;

            AccountHandle = DataUtility.GetFromDataRow<string>(dr, "AccountHandle");
            GamePermissionsRank = DataUtility.GetFromDataRow<StaffRank>(dr, "GamePermissionsRank");

            LastKnownLocation = DataUtility.GetFromDataRow<string>(dr, "LastKnownLocation");
            LastKnownLocationType = DataUtility.GetFromDataRow<string>(dr, "LastKnownLocationType");

            Model = new DimensionalModel(dr);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            ICharacter returnValue = default(ICharacter);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Character]([SurName], [Name], [AccountHandle], [Gender], [GamePermissionsRank]");
            sql.Append(", [DimensionalModelLength], [DimensionalModelHeight], [DimensionalModelWidth], [DimensionalModelID], [DimensionalModelMaterialCompositions])");
            sql.AppendFormat(" values('{0}','{1}','{2}', '{3}', {4}, {5}, {6}, {7}, {8}, '{9}')"
                , SurName, Name, AccountHandle, Gender, (short)GamePermissionsRank
                , Model.Height, Model.Length, Model.Width, Model.ModelBackingData.ID, Model.SerializeMaterialCompositions());
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
        public override bool Remove()
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
        public override bool Save()
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
            sql.AppendFormat(" , [DimensionalModelLength] = {0} ", Model.Length);
            sql.AppendFormat(" , [DimensionalModelHeight] = {0} ", Model.Height);
            sql.AppendFormat(" , [DimensionalModelWidth] = {0} ", Model.Width);
            sql.AppendFormat(" , [DimensionalModelMaterialCompositions] = '{0}' ", Model.SerializeMaterialCompositions());
            sql.AppendFormat(" , [DimensionalModelId] = {0} ", Model.ModelBackingData.ID);
            sql.AppendFormat(" , [LastRevised] = GetUTCDate()");
            sql.AppendFormat(" where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }
    }
}
