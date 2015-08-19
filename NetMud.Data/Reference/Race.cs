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

            var armsJson = DataUtility.GetFromDataRow<string>(dr, "Arms");
            Arms = DeserializeLimbs(armsJson);

            var legsJson = DataUtility.GetFromDataRow<string>(dr, "Legs");
            Legs = DeserializeLimbs(legsJson);

            var torsoId = DataUtility.GetFromDataRow<long>(dr, "Torso");
            Torso = DataWrapper.GetOne<IInanimateData>(torsoId);

            var headId = DataUtility.GetFromDataRow<long>(dr, "Head");
            Head = DataWrapper.GetOne<IInanimateData>(headId);

            var bodyPartJson = DataUtility.GetFromDataRow<string>(dr, "BodyParts");
            BodyParts = DeserializeBodyParts(bodyPartJson);

            DietaryNeeds = (DietType)DataUtility.GetFromDataRow<short>(dr, "DietaryNeeds");

            var bloodId = DataUtility.GetFromDataRow<long>(dr, "SanguinaryMaterial");
            SanguinaryMaterial = ReferenceWrapper.GetOne<IMaterial>(bloodId);

            var visionJson = DataUtility.GetFromDataRow<string>(dr, "VisionRange");
            VisionRange = DeserializeRange(visionJson);

            var tempJson = DataUtility.GetFromDataRow<string>(dr, "TemperatureTolerance");
            TemperatureTolerance = DeserializeRange(tempJson);

            Breathes = (RespiratoryType)DataUtility.GetFromDataRow<short>(dr, "Breathes");

            TeethType = (DamageType)DataUtility.GetFromDataRow<short>(dr, "TeethType");

            var startRoomId = DataUtility.GetFromDataRow<long>(dr, "StartingLocation");
            StartingLocation = DataWrapper.GetOne<IRoomData>(startRoomId);

            var recallRoomId = DataUtility.GetFromDataRow<long>(dr, "EmergencyLocation");
            EmergencyLocation = DataWrapper.GetOne<IRoomData>(recallRoomId);
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

        private Tuple<IInanimateData, short> DeserializeLimbs(string partJson)
        {
            dynamic parts = JsonConvert.DeserializeObject(partJson);

            long objId = long.Parse(parts.First);
            short amount = parts.Second;

            return new Tuple<IInanimateData, short>(DataWrapper.GetOne<IInanimateData>(objId), amount);
        }

        private Tuple<short, short> DeserializeRange(string rangeJson)
        {
            dynamic parts = JsonConvert.DeserializeObject(rangeJson);

            short low = parts.First;
            short high = parts.Second;

            return new Tuple<short, short>(low, high);
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
            sql.Append("insert into [dbo].[Race]([Name],[Arms],[Legs],[Torso],[Head],[BodyParts]");
            sql.Append(",[DietaryNeeds],[SanguinaryMaterial],[VisionRange],[TemperatureTolerance],[Breathes],[TeethType],[StartingLocation],[EmergencyLocation])");
            sql.AppendFormat(" values('{0}','{1}','{2}',{3},{4},'{5}',{6},{7},'{8}','{9}',{10},{11},{12},{13})", Name, Arms, Legs, Torso.ID, Head.ID, BodyParts,
                (short)DietaryNeeds, SanguinaryMaterial.ID, VisionRange, TemperatureTolerance, (short)Breathes, (short)TeethType, StartingLocation.ID, EmergencyLocation.ID);
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

            sql.AppendFormat(", [Arms] = '{0}' ", Arms);
            sql.AppendFormat(", [Legs] = '{0}' ", Legs);
            sql.AppendFormat(", [Torso] = {0} ",  Torso.ID);
            sql.AppendFormat(", [Head] = {0} ", Head.ID);
            sql.AppendFormat(", [BodyParts] = '{0}' ", BodyParts);
            sql.AppendFormat(", [DietaryNeeds] = {0} ", (short)DietaryNeeds);
            sql.AppendFormat(", [SanguinaryMaterial] = {0} ", SanguinaryMaterial.ID);
            sql.AppendFormat(", [VisionRange] = '{0}' ", VisionRange);
            sql.AppendFormat(", [TemperatureTolerance] = '{0}' ", TemperatureTolerance);
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
