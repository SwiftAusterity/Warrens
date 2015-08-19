using NetMud.Data.Reference;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Data;
using System.Text;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for NPCs
    /// </summary>
    public class NonPlayerCharacter : EntityBackingDataPartial, INonPlayerCharacter
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        public override Type EntityClass
        {
            get { return typeof(Game.Intelligence); }
        }

        /// <summary>
        /// Gender data string for NPCs
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// "family name" for NPCs
        /// </summary>
        public string SurName { get; set; }

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

            Model = new DimensionalModel(dr);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            INonPlayerCharacter returnValue = default(INonPlayerCharacter);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[NonPlayerCharacter]([SurName], [Name], [Gender]");
            sql.Append(", [DimensionalModelLength], [DimensionalModelHeight], [DimensionalModelWidth], [DimensionalModelID], [DimensionalModelMaterialCompositions])");
            sql.AppendFormat(" values('{0}','{1}','{2}', {3}, {4}, {5}, {6}, '{7}')"
                , SurName, Name, Gender
                , Model.Height, Model.Length, Model.Width, Model.ModelBackingData.ID, Model.SerializeMaterialCompositions());
            sql.Append(" select * from [dbo].[NonPlayerCharacter] where ID = Scope_Identity()");

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
            sql.AppendFormat("delete from [dbo].[NonPlayerCharacter] where ID = {0}", ID);

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
            sql.Append("update [dbo].[NonPlayerCharacter] set ");
            sql.AppendFormat(" [SurName] = '{0}' ", SurName);
            sql.AppendFormat(" , [Name] = '{0}' ", Name);
            sql.AppendFormat(" , [Gender] = '{0}' ", Gender);
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
