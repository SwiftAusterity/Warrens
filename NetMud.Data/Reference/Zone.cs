using NetMud.DataAccess; using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NetMud.Data.Reference
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : ReferenceDataPartial, IZone
    {
        /// <summary>
        /// The midline elevation point "sea level" for this zone
        /// </summary>
        public int BaseElevation { get; set; }

        /// <summary>
        /// The fudge value for temperature variance
        /// </summary>
        public int TemperatureCoefficient { get; set; }

        /// <summary>
        /// The fudge value for pressure (weather pattern) variance
        /// </summary>
        public int PressureCoefficient { get; set; }

        /// <summary>
        /// Who currently owns this zone
        /// </summary>
        public long Owner { get; set; }

        /// <summary>
        /// Is this zone ownership malleable
        /// </summary>
        public bool Claimable { get; set; }

        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Zone()
        {
            ID = -1;
            Created = DateTime.UtcNow;
            LastRevised = DateTime.UtcNow;
            Name = "NotImpl";

            BaseElevation = 0;
            TemperatureCoefficient = 0;
            PressureCoefficient = 0;
            Owner = -1;
            Claimable = false;
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

            BaseElevation = DataUtility.GetFromDataRow<int>(dr, "BaseElevation");
            TemperatureCoefficient = DataUtility.GetFromDataRow<int>(dr, "TemperatureCoefficient");
            PressureCoefficient = DataUtility.GetFromDataRow<int>(dr, "PressureCoefficient");
            Owner = DataUtility.GetFromDataRow<long>(dr, "Owner");
            Claimable = DataUtility.GetFromDataRow<bool>(dr, "Claimable");
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            var parms = new Dictionary<string, object>();

            Zone returnValue = default(Zone);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Zone]([Name], [BaseElevation], [TemperatureCoefficient], [PressureCoefficient], [Owner], [Claimable])");
            sql.Append(" values(@Name,@BaseElevation,@TemperatureCoefficient,@PressureCoefficient,@Owner,@Claimable)");
            sql.Append(" select * from [dbo].[Zone] where ID = Scope_Identity()");

            parms.Add("Name", Name);
            parms.Add("BaseElevation", BaseElevation);
            parms.Add("TemperatureCoefficient", TemperatureCoefficient);
            parms.Add("PressureCoefficient", PressureCoefficient);
            parms.Add("Owner", Owner);
            parms.Add("Claimable", Claimable);

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
            sql.Append("delete from [dbo].[Zone] where ID = @id");

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
            sql.Append("update [dbo].[Zone] set ");
            sql.Append(" [Name] = @Name ");
            sql.Append(" , [BaseElevation] = @BaseElevation ");
            sql.Append(" , [TemperatureCoefficient] = @TemperatureCoefficient ");
            sql.Append(" , [PressureCoefficient] = @PressureCoefficient ");
            sql.Append(" , [Owner] = @Owner ");
            sql.Append(" , [Claimable] = @Claimable ");
            sql.Append(" , [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("BaseElevation", BaseElevation);
            parms.Add("TemperatureCoefficient", TemperatureCoefficient);
            parms.Add("PressureCoefficient", PressureCoefficient);
            parms.Add("Owner", Owner);
            parms.Add("Claimable", Claimable);

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

            sb.Add(Name);

            return sb;
        }
    }
}
