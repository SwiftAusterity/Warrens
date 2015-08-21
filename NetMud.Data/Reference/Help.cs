using NetMud.DataAccess;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NetMud.Data.Reference
{
    /// <summary>
    /// Referred to as Help Files in the UI, extra help content for the help command
    /// </summary>
    public class Help : ReferenceDataPartial, IReferenceData
    {
        /// <summary>
        /// New up a "blank" help entry
        /// </summary>
        public Help()
        {
            ID = -1;
            Created = DateTime.UtcNow;
            LastRevised = DateTime.UtcNow;
            Name = "NotImpl";
            HelpText = "NotImpl";
        }

        /// <summary>
        /// Help text for the body of the render to help command
        /// </summary>
        public string HelpText { get; set; }

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

            HelpText = DataUtility.GetFromDataRow<string>(dr, "HelpText");
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            var parms = new Dictionary<string, object>();

            Help returnValue = default(Help);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Help]([Name], [HelpText])");
            sql.Append(" values(@Name,@HelpText)");
            sql.Append(" select * from [dbo].[Help] where ID = Scope_Identity()");

            parms.Add("Name", Name);
            parms.Add("HelpText", HelpText);

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
            sql.Append("delete from [dbo].[Help] where ID = @id");

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
            sql.Append("update [dbo].[Help] set ");
            sql.Append(" [Name] = @Name ");
            sql.Append(" , [HelpText] = @HelpText ");
            sql.Append(" , [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("HelpText", HelpText);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text, parms);

            return true;
        }

        /// <summary>
        /// Renders the help text for this data object
        /// </summary>
        /// <returns>help text</returns>
        public override IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(HelpText);

            return sb;
        }
    }
}
