using NetMud.Data.EntityBackingData;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Actionable;
using NetMud.DataStructure.Behaviors.Automation;
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
    [Serializable]
    public class Race : ReferenceDataPartial, IRace
    {
        /// <summary>
        /// The arm objects
        /// </summary>
        public Tuple<IInanimateData, short> Arms { get; set; }

        /// <summary>
        /// The leg objects
        /// </summary>
        public Tuple<IInanimateData, short> Legs { get; set; }

        /// <summary>
        /// the torso object
        /// </summary>
        public IInanimateData Torso { get; set; }

        /// <summary>
        /// The head object
        /// </summary>
        public IInanimateData Head { get; set; }

        /// <summary>
        /// The list of additional body parts used by this race. Part Object, Amount, Name
        /// </summary>
        public IEnumerable<Tuple<IInanimateData, short, string>> BodyParts { get; set; }

        /// <summary>
        /// Dietary type of this race
        /// </summary>
        public DietType DietaryNeeds { get; set; }

        /// <summary>
        /// Material that is the blood
        /// </summary>
        public IMaterial SanguinaryMaterial { get; set; }

        /// <summary>
        /// Low and High luminosity vision range
        /// </summary>
        public Tuple<short, short> VisionRange { get; set; }

        /// <summary>
        /// Low and High temperature range before damage starts to occur
        /// </summary>
        public Tuple<short, short> TemperatureTolerance { get; set; }

        /// <summary>
        /// What mode of breathing
        /// </summary>
        public RespiratoryType Breathes { get; set; }

        /// <summary>
        /// The type of damage biting inflicts
        /// </summary>
        public DamageType TeethType { get; set; }

        /// <summary>
        /// What is the starting room of new players
        /// </summary>
        public IRoomData StartingLocation { get; set; }

        /// <summary>
        /// When a player loads without a location where do we send them
        /// </summary>
        public IRoomData EmergencyLocation { get; set; }

        public Race()
        {
            BodyParts = Enumerable.Empty<Tuple<IInanimateData, short, string>>();
        }

        public Race(string bodyPartsJson)
        {
            BodyParts = DeserializeBodyParts(bodyPartsJson);
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

            var armsId = DataUtility.GetFromDataRow<long>(dr, "ArmsId");
            var armsAmount = DataUtility.GetFromDataRow<short>(dr, "ArmsAmount");
            Arms = new Tuple<IInanimateData, short>(DataWrapper.GetOne<InanimateData>(armsId), armsAmount);

            var LegsId = DataUtility.GetFromDataRow<long>(dr, "LegsId");
            var LegsAmount = DataUtility.GetFromDataRow<short>(dr, "LegsAmount");
            Legs = new Tuple<IInanimateData, short>(DataWrapper.GetOne<InanimateData>(LegsId), LegsAmount);

            var torsoId = DataUtility.GetFromDataRow<long>(dr, "Torso");
            Torso = DataWrapper.GetOne<InanimateData>(torsoId);

            var headId = DataUtility.GetFromDataRow<long>(dr, "Head");
            Head = DataWrapper.GetOne<InanimateData>(headId);

            var bodyPartJson = DataUtility.GetFromDataRow<string>(dr, "BodyParts");
            BodyParts = DeserializeBodyParts(bodyPartJson);

            DietaryNeeds = (DietType)DataUtility.GetFromDataRow<short>(dr, "DietaryNeeds");

            var bloodId = DataUtility.GetFromDataRow<long>(dr, "SanguinaryMaterial");
            SanguinaryMaterial = ReferenceWrapper.GetOne<Material>(bloodId);

            var visionLow = DataUtility.GetFromDataRow<short>(dr, "VisionRangeLow");
            var visionHigh = DataUtility.GetFromDataRow<short>(dr, "VisionRangeHigh");
            VisionRange = new Tuple<short, short>(visionLow, visionHigh);

            var tempLow = DataUtility.GetFromDataRow<short>(dr, "TemperatureToleranceLow");
            var tempHigh = DataUtility.GetFromDataRow<short>(dr, "TemperatureToleranceHigh");
            TemperatureTolerance = new Tuple<short, short>(tempLow, tempHigh);

            Breathes = (RespiratoryType)DataUtility.GetFromDataRow<short>(dr, "Breathes");

            TeethType = (DamageType)DataUtility.GetFromDataRow<short>(dr, "TeethType");

            var startRoomId = DataUtility.GetFromDataRow<long>(dr, "StartingLocation");
            StartingLocation = DataWrapper.GetOne<RoomData>(startRoomId);

            var recallRoomId = DataUtility.GetFromDataRow<long>(dr, "EmergencyLocation");
            EmergencyLocation = DataWrapper.GetOne<RoomData>(recallRoomId);
        }

        private IEnumerable<Tuple<IInanimateData, short, string>> DeserializeBodyParts(string partJson)
        {
            var bodyParts = new List<Tuple<IInanimateData, short, string>>();

            dynamic parts = JsonConvert.DeserializeObject(partJson);

            foreach (dynamic part in parts)
            {
                long objId = long.Parse(part.First);
                short amount = part.Second;
                string partName = part.Third;

                bodyParts.Add(new Tuple<IInanimateData, short, string>(DataWrapper.GetOne<IInanimateData>(objId), amount, partName));
            }

            return bodyParts;
        }

        public string SerializeBodyParts()
        {
            var parts = new List<Tuple<long, short, string>>();

            foreach (var tpl in BodyParts)
                parts.Add(new Tuple<long, short, string>(tpl.Item1.ID, tpl.Item2, tpl.Item3));

            return JsonConvert.SerializeObject(parts);
        }


        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            //TODO: parameterize the whole insert business
            Race returnValue = default(Race);
            var parms = new Dictionary<string, object>();

            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Race]([Name],[ArmsId],[ArmsAmount],[LegsId],[LegsAmount],[Torso],[Head],[BodyParts]");
            sql.Append(",[DietaryNeeds],[SanguinaryMaterial],[VisionRangeLow],[VisionRangeHigh],[TemperatureToleranceLow],[TemperatureToleranceHigh],[Breathes],[TeethType],[StartingLocation],[EmergencyLocation])");
            sql.Append(" values(@Name,@ArmsId,@ArmsAmount,@LegsId,@LegsAmount,@Torso,@Head,@BodyParts");
            sql.Append(",@DietaryNeeds,@SanguinaryMaterial,@VisionRangeLow,@VisionRangeHigh,@TemperatureToleranceLow,@TemperatureToleranceHigh,@Breathes,@TeethType,@StartingLocation,@EmergencyLocation)");
            sql.Append(" select * from [dbo].[Race] where ID = Scope_Identity()");

            parms.Add("Name", Name);
            parms.Add("ArmsId", Arms.Item1.ID);
            parms.Add("ArmsAmount", Arms.Item2);
            parms.Add("LegsId", Legs.Item1.ID);
            parms.Add("LegsAmount", Legs.Item2);
            parms.Add("Torso", Torso.ID);
            parms.Add("Head", Head.ID);
            parms.Add("BodyParts", SerializeBodyParts());
            parms.Add("DietaryNeeds", (short)DietaryNeeds);
            parms.Add("SanguinaryMaterial", SanguinaryMaterial.ID);
            parms.Add("VisionRangeLow", VisionRange.Item1);
            parms.Add("VisionRangeHigh", VisionRange.Item2);
            parms.Add("TemperatureToleranceLow", TemperatureTolerance.Item1);
            parms.Add("TemperatureToleranceHigh", TemperatureTolerance.Item2);
            parms.Add("Breathes", (short)Breathes);
            parms.Add("TeethType", (short)TeethType);
            parms.Add("StartingLocation", StartingLocation.ID);
            parms.Add("EmergencyLocation", EmergencyLocation.ID);

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
            sql.Append("delete from [dbo].[Race] where ID = @id");

            parms.Add("id", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

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
            sql.Append("update [dbo].[Race] set ");
            sql.AppendFormat(" [Name] = @Name ");
            sql.Append(", [ArmsId] = @ArmsId ");
            sql.Append(", [ArmsAmount] = @ArmsAmount ");
            sql.Append(", [LegsId] = @LegsId ");
            sql.Append(", [LegsAmount] = @LegsAmount ");
            sql.Append(", [Torso] = @Torso ");
            sql.Append(", [Head] = @Head ");
            sql.Append(", [BodyParts] = @BodyParts ");
            sql.Append(", [DietaryNeeds] = @DietaryNeeds ");
            sql.Append(", [SanguinaryMaterial] = @SanguinaryMaterial ");
            sql.Append(", [VisionRangeLow] = @VisionRangeLow ");
            sql.Append(", [VisionRangeHigh] = @VisionRangeHigh ");
            sql.Append(", [TemperatureToleranceLow] = @TemperatureToleranceLow ");
            sql.Append(", [TemperatureToleranceHigh] = @TemperatureToleranceHigh ");
            sql.Append(", [Breathes] = @Breathes ");
            sql.Append(", [TeethType] = @TeethType ");
            sql.Append(", [StartingLocation] = @StartingLocation ");
            sql.Append(", [EmergencyLocation] = @EmergencyLocation ");
            sql.Append(", [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("ArmsId", Arms.Item1.ID);
            parms.Add("ArmsAmount", Arms.Item2);
            parms.Add("LegsId", Legs.Item1.ID);
            parms.Add("LegsAmount", Legs.Item2);
            parms.Add("Torso", Torso.ID);
            parms.Add("Head", Head.ID);
            parms.Add("BodyParts", SerializeBodyParts());
            parms.Add("DietaryNeeds", (short)DietaryNeeds);
            parms.Add("SanguinaryMaterial", SanguinaryMaterial.ID);
            parms.Add("VisionRangeLow", VisionRange.Item1);
            parms.Add("VisionRangeHigh", VisionRange.Item2);
            parms.Add("TemperatureToleranceLow", TemperatureTolerance.Item1);
            parms.Add("TemperatureToleranceHigh", TemperatureTolerance.Item2);
            parms.Add("Breathes", (short)Breathes);
            parms.Add("TeethType", (short)TeethType);
            parms.Add("StartingLocation", StartingLocation.ID);
            parms.Add("EmergencyLocation", EmergencyLocation.ID);

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

            sb.Add(Name);

            return sb;
        }
    }
}
