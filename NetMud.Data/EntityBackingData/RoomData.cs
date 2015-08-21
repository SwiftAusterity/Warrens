using NetMud.Data.Reference;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public class RoomData : EntityBackingDataPartial, IRoomData
    {
        public override string DataTableName { get { return "RoomData"; } }

        /// <summary>
        /// The system type of data this attaches to
        /// </summary>
        public override Type EntityClass
        {
            get { return typeof(NetMud.Data.Game.Room); }
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
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


            Model = new DimensionalModel(dr);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            var parms = new Dictionary<string, object>();

            IRoomData returnValue = default(IRoomData);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[RoomData]([Name], [DimensionalModelLength], [DimensionalModelHeight], [DimensionalModelWidth], [DimensionalModelID], [DimensionalModelMaterialCompositions])");
            sql.Append(" values(@Name,@DimensionalModelLength,@DimensionalModelHeight,@DimensionalModelWidth,@DimensionalModelID,@DimensionalModelMaterialCompositions)");
            sql.Append(" select * from [dbo].[RoomData] where ID = Scope_Identity()");

            parms.Add("Name", Name);
            parms.Add("DimensionalModelLength", Model.Length);
            parms.Add("DimensionalModelHeight", Model.Height);
            parms.Add("DimensionalModelWidth", Model.Width);
            parms.Add("DimensionalModelID", Model.ModelBackingData.ID);
            parms.Add("DimensionalModelMaterialCompositions", Model.SerializeMaterialCompositions());

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
            //TODO: Exits too?
            var sql = new StringBuilder();
            sql.Append("delete from [dbo].[RoomData] where ID = @id");

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
            sql.Append("update [dbo].[RoomData] set ");
            sql.Append(" [Name] =  @Name ");
            sql.Append(", [DimensionalModelLength] =  @DimensionalModelLength ");
            sql.Append(", [DimensionalModelHeight] =  @DimensionalModelHeight ");
            sql.Append(", [DimensionalModelWidth] =  @DimensionalModelWidth ");
            sql.Append(", [DimensionalModelMaterialCompositions] =  @DimensionalModelMaterialCompositions ");
            sql.Append(", [DimensionalModelId] = @DimensionalModelId "); 
            sql.Append(" , [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("DimensionalModelLength", Model.Length);
            parms.Add("DimensionalModelHeight", Model.Height);
            parms.Add("DimensionalModelWidth", Model.Width);
            parms.Add("DimensionalModelID", Model.ModelBackingData.ID);
            parms.Add("DimensionalModelMaterialCompositions", Model.SerializeMaterialCompositions());


            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text, parms);

            return true;
        }
    }
}
