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
    public class Help : IReferenceData
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
        /// Help text for the body of the render to help command
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public void Fill(global::System.Data.DataRow dr)
        {
            int outId = default(int);
            DataUtility.GetFromDataRow<int>(dr, "ID", ref outId);
            ID = outId;

            DateTime outCreated = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "Created", ref outCreated);
            Created = outCreated;

            DateTime outRevised = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised", ref outRevised);
            LastRevised = outRevised;

            string outName = default(string);
            DataUtility.GetFromDataRow<string>(dr, "Name", ref outName);
            Name = outName;

            string outHelpText = default(string);
            DataUtility.GetFromDataRow<string>(dr, "HelpText", ref outHelpText);
            HelpText = outHelpText;
        }

         /// <summary>
         /// -99 = null input
         /// -1 = wrong type
         /// 0 = same type, wrong id
         /// 1 = same reference (same id, same type)
         /// </summary>
         /// <param name="obj"></param>
         /// <returns></returns>
        public int CompareTo(IData obj)
        {
            if (obj != null)
            {
                try
                {
                    if (obj.GetType() != typeof(Help))
                        return -1;

                    if (obj.ID.Equals(this.ID))
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
                    return other.GetType() == typeof(Help) && other.ID.Equals(this.ID);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Renders the help text for this data object
        /// </summary>
        /// <returns>help text</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(HelpText);

            return sb;
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public IData Create()
        {
            Help returnValue = default(Help);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Help]([Name], [HelpText])");
            sql.AppendFormat(" values('{0}','{1}')", Name, HelpText);
            sql.Append(" select * from [dbo].[Help] where ID = Scope_Identity()");

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
            sql.AppendFormat("delete from [dbo].[Help] where ID = {0}", ID);

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
            sql.Append("update [dbo].[Help] set ");
            sql.AppendFormat(" [Name] = '{0}' ", Name);
            sql.AppendFormat(" , [HelpText] = '{0}' ", HelpText);
            sql.AppendFormat(" , [LastRevised] = GetUTCDate()");
            sql.AppendFormat(" where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }
    }
}
