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
    public class Race : ReferenceDataPartial, IRace
    {
        public Tuple<IInanimateData, short> Arms { get; set; }

        public Tuple<IInanimateData, short> Legs { get; set; }

        public IInanimateData Torso { get; set; }

        public IInanimateData Head { get; set; }

        public IEnumerable<Tuple<IInanimateData, short, string>> BodyParts { get; set; }

        public DietType DietaryNeeds { get; set; }

        public IMaterial SanguinaryMaterial { get; set; }

        public Tuple<short, short> VisionRange { get; set; }

        public Tuple<short, short> TemperatureTolerance { get; set; }

        public RespiratoryType Breathes { get; set; }

        public DamageType TeethType { get; set; }

        public IRoomData StartingLocation { get; set; }

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

            var tempLow = DataUtility.GetFromDataRow<short>(dr, "TemperatureTolerance");
            var tempHigh = DataUtility.GetFromDataRow<short>(dr, "TemperatureTolerance");
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

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            //TODO: parameterize the whole insert business
            Race returnValue = default(Race);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Race]([Name],[ArmsId],[ArmsAmount],[LegsId],[LegsAmount],[Torso],[Head],[BodyParts]");
            sql.Append(",[DietaryNeeds],[SanguinaryMaterial],[VisionRangeLow],[VisionRangeHigh],[TemperatureToleranceLow],[TemperatureToleranceHigh],[Breathes],[TeethType],[StartingLocation],[EmergencyLocation])");
            sql.AppendFormat(" values('{0}',{1},{2},{3},{4},{5},{6},'{7}',{8},{9},{10},{11},{12},{13},{14},{15},{16},{17})", Name
                , Arms.Item1.ID, Arms.Item2, Legs.Item1.ID, Legs.Item2, Torso.ID, Head.ID, JsonConvert.SerializeObject(BodyParts)
                , (short)DietaryNeeds, SanguinaryMaterial.ID, VisionRange.Item1, VisionRange.Item2, TemperatureTolerance.Item1, TemperatureTolerance.Item2
                , (short)Breathes, (short)TeethType, StartingLocation.ID, EmergencyLocation.ID);
            sql.Append(" select * from [dbo].[Race] where ID = Scope_Identity()");

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
            sql.AppendFormat("delete from [dbo].[Race] where ID = {0}", ID);

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
            sql.Append("update [dbo].[Race] set ");
            sql.AppendFormat(" [Name] = '{0}' ", Name);

            sql.AppendFormat(", [ArmsId] = {0} ", Arms.Item1.ID);
            sql.AppendFormat(", [ArmsAmount] = {0} ", Arms.Item2);
            sql.AppendFormat(", [LegsId] = {0} ", Legs.Item1.ID);
            sql.AppendFormat(", [LegsAmount] = {0} ", Legs.Item2);
            sql.AppendFormat(", [Torso] = {0} ", Torso.ID);
            sql.AppendFormat(", [Head] = {0} ", Head.ID);
            sql.AppendFormat(", [BodyParts] = '{0}' ", BodyParts);
            sql.AppendFormat(", [DietaryNeeds] = {0} ", (short)DietaryNeeds);
            sql.AppendFormat(", [SanguinaryMaterial] = {0} ", SanguinaryMaterial.ID);
            sql.AppendFormat(", [VisionRangeLow] = {0} ", VisionRange.Item1);
            sql.AppendFormat(", [VisionRangeHigh] = {0} ", VisionRange.Item2);
            sql.AppendFormat(", [TemperatureToleranceLow] = '{0}' ", TemperatureTolerance.Item1);
            sql.AppendFormat(", [TemperatureToleranceHigh] = '{0}' ", TemperatureTolerance.Item2);
            sql.AppendFormat(", [Breathes] = {0} ", (short)Breathes);
            sql.AppendFormat(", [TeethType] = {0} ", (short)TeethType);
            sql.AppendFormat(", [StartingLocation] = {0} ", StartingLocation.ID);
            sql.AppendFormat(", [EmergencyLocation] = {0} ", EmergencyLocation.ID);
            sql.AppendFormat(", [LastRevised] = GetUTCDate()");
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

            sb.Add(Name);

            return sb;
        }
    }
}
