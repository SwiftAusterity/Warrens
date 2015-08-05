using NetMud.DataAccess;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Reference
{    
    /// <summary>
    /// Backing data for physical models
    /// </summary>
    public class DimensionalModelData : ReferenceDataPartial, IDimensionalModelData
    {
        /// <summary>
        /// The 11 planes that compose the physical model
        /// </summary>
        public HashSet<IDimensionalModelPlane> ModelPlanes { get; set; }

        /// <summary>
        /// Turn the modelPlanes into a json string we can store in the db
        /// </summary>
        /// <returns></returns>
        private string DeserializeModel()
        {
            return JsonConvert.SerializeObject(ModelPlanes);
        }

        /// <summary>
        /// Turn the json we store in the db into the modelplanes
        /// </summary>
        /// <param name="modelJson">json we store in the db</param>
        private void SerializeModel(string modelJson)
        {
            dynamic planes = JsonConvert.DeserializeObject(modelJson);

            foreach (dynamic plane in planes)
            {
                var newPlane = new DimensionalModelPlane();
                newPlane.TagName = plane.TagName;
                newPlane.ModelNodes = plane.ModelNodes;

                ModelPlanes.Add(newPlane);
            }

        }

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public override void Fill(global::System.Data.DataRow dr)
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

            string outModel = default(string);
            DataUtility.GetFromDataRow<string>(dr, "Model", ref outName);
            SerializeModel(outModel);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            DimensionalModelData returnValue = default(DimensionalModelData);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[DimensionalModelData]([Name], [Model])");
            sql.AppendFormat(" values('{0}','{1}')", Name, DeserializeModel());
            sql.Append(" select * from [dbo].[DimensionalModelData] where ID = Scope_Identity()");

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
            sql.AppendFormat("delete from [dbo].[DimensionalModelData] where ID = {0}", ID);

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
            sql.Append("update [dbo].[DimensionalModelData] set ");
            sql.AppendFormat(" [Name] = '{0}' ", Name);
            sql.AppendFormat(" , [Model] = '{0}' ", DeserializeModel());
            sql.AppendFormat(" , [LastRevised] = GetUTCDate()");
            sql.AppendFormat(" where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        /// <summary>
        /// Renders the help text for this data object
        /// </summary>
        /// <returns>help text</returns>
        public override IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            //TODO: Render the actual model flattened in ascii, probably require a fair bit of work so just returning name for now
            sb.Add(Name);

            return sb;
        }
    }
}
