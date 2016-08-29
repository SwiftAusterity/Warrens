using NetMud.Data.Reference;
using NetMud.DataAccess; using NetMud.DataAccess.Cache;
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
    /// Backing data for pathways
    /// </summary>
    [Serializable]
    public class PathwayData : EntityBackingDataPartial, IPathwayData
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        public override Type EntityClass
        {
            get { return typeof(NetMud.Data.Game.Pathway); }
        }

        /// <summary>
        /// 0->360 degrees with 0 being absolute north (meaning 90 is west, 180 south, etc) -1 means no cardinality
        /// </summary>
        public int DegreesFromNorth { get; set; }

        /// <summary>
        /// The container this points into
        /// </summary>
        public string ToLocationID { get; set; }

        /// <summary>
        /// The system type of the container this points into
        /// </summary>
        public string ToLocationType { get; set; }

        /// <summary>
        /// The container this starts in
        /// </summary>
        public string FromLocationID { get; set; }

        /// <summary>
        /// The system type of the container this starts in
        /// </summary>
        public string FromLocationType { get; set; }

        /// <summary>
        /// Output message format the Actor recieves upon moving
        /// </summary>
        public string MessageToActor { get; set; }

        /// <summary>
        /// Output message format the originating location's entities recieve upon moving
        /// </summary>
        public string MessageToOrigin { get; set; }

        /// <summary>
        /// Output message format the destination location's entities recieve upon moving
        /// </summary>
        public string MessageToDestination { get; set; }

        /// <summary>
        /// Audible (heard) message sent to surrounding locations of both origin and destination
        /// </summary>
        public string AudibleToSurroundings { get; set; }

        /// <summary>
        /// Strength of audible message to surroundings
        /// </summary>
        public int AudibleStrength { get; set; }

        /// <summary>
        /// Visible message sent to surrounding locations of both origin and destination
        /// </summary>
        public string VisibleToSurroundings { get; set; }

        /// <summary>
        /// Strength of visible message to surroundings
        /// </summary>
        public int VisibleStrength { get; set; }

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

            ToLocationID = DataUtility.GetFromDataRow<string>(dr, "ToLocationID");
            FromLocationID = DataUtility.GetFromDataRow<string>(dr, "FromLocationID");
            ToLocationType = DataUtility.GetFromDataRow<string>(dr, "ToLocationType");
            FromLocationType = DataUtility.GetFromDataRow<string>(dr, "FromLocationType");
            MessageToDestination = DataUtility.GetFromDataRow<string>(dr, "MessageToDestination");
            MessageToOrigin = DataUtility.GetFromDataRow<string>(dr, "MessageToOrigin");
            MessageToActor = DataUtility.GetFromDataRow<string>(dr, "MessageToActor");
            AudibleToSurroundings = DataUtility.GetFromDataRow<string>(dr, "AudibleToSurroundings");
            VisibleToSurroundings = DataUtility.GetFromDataRow<string>(dr, "VisibleToSurroundings");
            AudibleStrength = DataUtility.GetFromDataRow<int>(dr, "AudibleStrength");
            VisibleStrength = DataUtility.GetFromDataRow<int>(dr, "VisibleStrength");

            Model = new DimensionalModel(dr);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            var parms = new Dictionary<string, object>();

            IPathwayData returnValue = default(IPathwayData);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[PathwayData]([Name],[ToLocationID],[FromLocationID],[ToLocationType],[FromLocationType],[MessageToDestination],[MessageToOrigin]");
            sql.Append(",[MessageToActor],[AudibleToSurroundings],[VisibleToSurroundings],[AudibleStrength],[VisibleStrength]");
            sql.Append(", [DimensionalModelLength], [DimensionalModelHeight], [DimensionalModelWidth], [DimensionalModelID], [DimensionalModelMaterialCompositions])");
            sql.Append(" values(@Name,@ToLocationID,@FromLocationID,@ToLocationType,@FromLocationType,@MessageToDestination,@MessageToOrigin");
            sql.Append(", @MessageToActor,@AudibleToSurroundings,@VisibleToSurroundings,@AudibleStrength,@VisibleStrength");
            sql.Append(", @DimensionalModelLength, @DimensionalModelHeight, @DimensionalModelWidth, @DimensionalModelID, @DimensionalModelMaterialCompositions)");
            sql.Append(" select * from [dbo].[PathwayData] where ID = Scope_Identity()");

            parms.Add("Name", Name);
            parms.Add("ToLocationID", ToLocationID);
            parms.Add("FromLocationID", FromLocationID);
            parms.Add("ToLocationType", ToLocationType);
            parms.Add("FromLocationType", FromLocationType);
            parms.Add("MessageToDestination", MessageToDestination);
            parms.Add("MessageToOrigin", MessageToOrigin);
            parms.Add("MessageToActor", MessageToActor);
            parms.Add("AudibleToSurroundings", AudibleToSurroundings);
            parms.Add("VisibleToSurroundings", VisibleToSurroundings);
            parms.Add("AudibleStrength", AudibleStrength);
            parms.Add("VisibleStrength", VisibleStrength);
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
            //TODO: Exits too?
            var parms = new Dictionary<string, object>();
            var sql = new StringBuilder();
            sql.AppendFormat("delete from [dbo].[PathwayData] where ID = @id", ID);

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
            sql.Append("update [dbo].[PathwayData] set ");
            sql.Append(" [Name] =  @Name ");
            sql.Append(", [ToLocationID] =  @ToLocationID ");
            sql.Append(", [FromLocationID] =  @FromLocationID' ");
            sql.Append(", [ToLocationType] =  @ToLocationType ");
            sql.Append(", [FromLocationType] =  @FromLocationType ");
            sql.Append(", [MessageToDestination] =  @MessageToDestination ");
            sql.Append(", [MessageToOrigin] = @MessageToOrigin' ");
            sql.Append(", [MessageToActor] = @MessageToActor ");
            sql.Append(", [AudibleToSurroundings] = @AudibleToSurroundings ");
            sql.Append(", [VisibleToSurroundings] = @VisibleToSurroundings ");
            sql.Append(", [AudibleStrength] = @AudibleStrength ");
            sql.Append(", [VisibleStrength] = @VisibleStrength ");
            sql.Append(", [DimensionalModelLength] =  @DimensionalModelLength ");
            sql.Append(", [DimensionalModelHeight] =  @DimensionalModelHeight ");
            sql.Append(", [DimensionalModelWidth] =  @DimensionalModelWidth ");
            sql.Append(", [DimensionalModelMaterialCompositions] =  @DimensionalModelMaterialCompositions ");
            sql.Append(", [DimensionalModelId] = @DimensionalModelId "); 
            sql.Append(", [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("ToLocationID", ToLocationID);
            parms.Add("FromLocationID", FromLocationID);
            parms.Add("ToLocationType", ToLocationType);
            parms.Add("FromLocationType", FromLocationType);
            parms.Add("MessageToDestination", MessageToDestination);
            parms.Add("MessageToOrigin", MessageToOrigin);
            parms.Add("MessageToActor", MessageToActor);
            parms.Add("AudibleToSurroundings", AudibleToSurroundings);
            parms.Add("VisibleToSurroundings", VisibleToSurroundings);
            parms.Add("AudibleStrength", AudibleStrength);
            parms.Add("VisibleStrength", VisibleStrength);
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
