using NetMud.Data.Reference;
using NetMud.DataAccess; 
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    [Serializable]
    public class RoomData : EntityBackingDataPartial, IRoomData
    {
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

        [JsonProperty("Medium")]
        private long _medium { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        public IMaterial Medium
        {
            get
            {
                return BackingDataCache.Get<IMaterial>(_medium);
            }
            set
            {
                if(value != null)
                    _medium = value.ID;
            }
        }

        [JsonProperty("ZoneAffiliation")]
        private long _zoneAffiliation { get; set; }

        /// <summary>
        /// What zone this belongs to
        /// </summary>
        [ScriptIgnore]
        public IZone ZoneAffiliation
        {
            get
            {
                return BackingDataCache.Get<IZone>(_zoneAffiliation);
            }
            set
            {
                if(value != null)
                    _zoneAffiliation = value.ID;
            }
        }

        /// <summary>
        /// What walls are made of
        /// </summary>
        [JsonProperty("Borders")]
        private IDictionary<string, long> _borders { get; set; }

        /// <summary>
        /// The list of internal compositions for separate/explosion/sharding
        /// </summary>
        [ScriptIgnore]
        public IDictionary<string, IMaterial> Borders
        {
            get
            {
                if (_borders != null)
                    return _borders.ToDictionary(k => k.Key, k => BackingDataCache.Get<IMaterial>(k.Value));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _borders = value.ToDictionary(k => k.Key, k => k.Value.ID);
            }
        }

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

            var mediumId = DataUtility.GetFromDataRow<long>(dr, "Medium");
            Medium = ReferenceWrapper.GetOne<IMaterial>(mediumId);

            var zoneId = DataUtility.GetFromDataRow<long>(dr, "Zone");
            ZoneAffiliation = ReferenceWrapper.GetOne<IZone>(zoneId);

            Borders = DeserializeBorders(DataUtility.GetFromDataRow<string>(dr, "Borders"));

            Model = new DimensionalModel(dr);
        }

        public IDictionary<string, IMaterial> DeserializeBorders(string json)
        {
            var returntionary = new Dictionary<string, IMaterial>();

            dynamic borders = JsonConvert.DeserializeObject(json);

            foreach (dynamic border in borders)
            {
                long objId = long.Parse(border.Value);
                string name = border.Key;

                var material = ReferenceWrapper.GetOne<IMaterial>(objId);

                returntionary.Add(name, material);
            }

            return returntionary;
        }

        public string SerializeBorders()
        {
            var materialComps = new List<Tuple<string, long>>();

            foreach (var kvp in Borders)
                materialComps.Add(new Tuple<string, long>(kvp.Key, kvp.Value.ID));

            return JsonConvert.SerializeObject(materialComps);
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
            sql.Append("insert into [dbo].[RoomData]([Name], [DimensionalModelLength], [DimensionalModelHeight], [DimensionalModelWidth], [Medium], [Borders], [Zone])");
            sql.Append(" values(@Name,@DimensionalModelLength,@DimensionalModelHeight,@DimensionalModelWidth,@Medium,@Borders,@Zone)");
            sql.Append(" select * from [dbo].[RoomData] where ID = Scope_Identity()");

            parms.Add("Name", Name);
            parms.Add("DimensionalModelLength", Model.Length);
            parms.Add("DimensionalModelHeight", Model.Height);
            parms.Add("DimensionalModelWidth", Model.Width);
            parms.Add("Medium", Medium.ID);
            parms.Add("Borders", SerializeBorders());
            parms.Add("Zone", ZoneAffiliation.ID);

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
            sql.Append(", [Borders] =  @Borders ");
            sql.Append(", [Medium] =  @Medium ");
            sql.Append(", [Zone] =  @Zone ");
            sql.Append(", [DimensionalModelLength] =  @DimensionalModelLength ");
            sql.Append(", [DimensionalModelHeight] =  @DimensionalModelHeight ");
            sql.Append(", [DimensionalModelWidth] =  @DimensionalModelWidth ");
            sql.Append(" , [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("Medium", Medium.ID);
            parms.Add("Borders", SerializeBorders());
            parms.Add("Zone", ZoneAffiliation.ID);
            parms.Add("DimensionalModelLength", Model.Length);
            parms.Add("DimensionalModelHeight", Model.Height);
            parms.Add("DimensionalModelWidth", Model.Width);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text, parms);

            return true;
        }
    }
}
