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
    public class Material : ReferenceDataPartial, IMaterial
    {
        public bool Raw { get; set; }
        public bool Conductive { get; set; }
        public bool Magnetic { get; set; }
        public bool Flammable { get; set; }
         
        public short Viscosity { get; set; }
        public short Density { get; set; }
        public short Mallebility { get; set; }
        public short Ductility { get; set; }
        public short Porosity { get; set; }
        public short UnitMass { get; set; }
        public short SolidPoint { get; set; }
        public short GasPoint { get; set; }
         
        public short TemperatureRetention { get; set; }
        public IDictionary<DamageType, short> Resistance { get; set; }
        public IDictionary<IMaterial, short> Composition { get; set; }

        public Material()
        {
            Resistance = new Dictionary<DamageType, short>();
            Composition = new Dictionary<IMaterial, short>();
        }

        private IDictionary<DamageType, short> DeserializeResistances(string compJson)
        {
            var resistances = new Dictionary<DamageType, short>();

            dynamic comps = JsonConvert.DeserializeObject(compJson);

            foreach (dynamic comp in comps)
            {
                DamageType type = Enum.Parse(typeof(DamageType), comp.Name);
                short amount = comp.Value;

                resistances.Add(type, amount);
            }

            return resistances;
        }

        private string SerializeResistances()
        {
            return JsonConvert.SerializeObject(Resistance);
        }

        private IDictionary<IMaterial, short> DeserializeCompositions(string compJson)
        {
            var compositions = new Dictionary<IMaterial, short>();

            dynamic comps = JsonConvert.DeserializeObject(compJson);

            foreach (dynamic comp in comps)
            {
                long compId = comp.Name;
                short amount = comp.Value;

                compositions.Add(ReferenceWrapper.GetOne<IMaterial>(compId), amount);
            }

            return compositions;
        }

        private string SerializeCompositions()
        {
            var dbObject = new Dictionary<long, short>();

            foreach (var kvp in Composition)
                dbObject.Add(kvp.Key.ID, kvp.Value);

            return JsonConvert.SerializeObject(dbObject);
        }

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public override void Fill(global::System.Data.DataRow dr)
        {
            ID = DataUtility.GetFromDataRow<long>(dr, "ID");
            Created = DataUtility.GetFromDataRow<DateTime>(dr, "Created");
            LastRevised = DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised");
            Name = DataUtility.GetFromDataRow<string>(dr, "Name");

            Raw = DataUtility.GetFromDataRow<bool>(dr, "Raw");
            Conductive = DataUtility.GetFromDataRow<bool>(dr, "Conductive");
            Magnetic = DataUtility.GetFromDataRow<bool>(dr, "Magnetic");
            Flammable = DataUtility.GetFromDataRow<bool>(dr, "Flammable");

            Viscosity = DataUtility.GetFromDataRow<short>(dr, "Viscosity");
            Density = DataUtility.GetFromDataRow<short>(dr, "Density");
            Mallebility = DataUtility.GetFromDataRow<short>(dr, "Mallebility");
            Ductility = DataUtility.GetFromDataRow<short>(dr, "Ductility");
            Porosity = DataUtility.GetFromDataRow<short>(dr, "Porosity");
            UnitMass = DataUtility.GetFromDataRow<short>(dr, "UnitMass");
            SolidPoint = DataUtility.GetFromDataRow<short>(dr, "SolidPoint");
            GasPoint = DataUtility.GetFromDataRow<short>(dr, "GasPoint");

            TemperatureRetention = DataUtility.GetFromDataRow<short>(dr, "TemperatureRetention");

            var resistanceJson = DataUtility.GetFromDataRow<string>(dr, "Resistance");
            Resistance = DeserializeResistances(resistanceJson);

            var compJson = DataUtility.GetFromDataRow<string>(dr, "Composition");
            Composition = DeserializeCompositions(compJson);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            //TODO: parameterize the whole insert business
            Material returnValue = default(Material);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Material]([Name],[Raw],[Conductive],[Magnetic],[Flammable]");
            sql.Append(",[Viscosity],[Density],[Mallebility],[Ductility],[Porosity],[UnitMass],[SolidPoint],[GasPoint],[TemperatureRetention],[Resistance],[Composition])");
            sql.AppendFormat(" values('{0}',{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},'{14}','{15}')", Name
                , Raw ? 1 : 0
                , Conductive ? 1 : 0
                , Magnetic ? 1 : 0
                , Flammable ? 1 : 0
                , Viscosity,Density,Mallebility,Ductility,Porosity,UnitMass,SolidPoint,GasPoint,TemperatureRetention
                ,SerializeResistances(),SerializeCompositions());
            sql.Append(" select * from [dbo].[Material] where ID = Scope_Identity()");

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
            sql.AppendFormat("delete from [dbo].[Material] where ID = {0}", ID);

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
            sql.Append("update [dbo].[Material] set ");
            sql.AppendFormat(" [Name] = '{0}' ", Name);

            sql.AppendFormat(", [Raw] = {0} ", Raw ? 1 : 0);
            sql.AppendFormat(", [Conductive] = {0} ", Conductive ? 1 : 0);
            sql.AppendFormat(", [Magnetic] = {0} ", Magnetic ? 1 : 0);
            sql.AppendFormat(", [Flammable] = {0} ", Flammable ? 1 : 0);
            sql.AppendFormat(", [Viscosity] = {0} ", Viscosity);
            sql.AppendFormat(", [Density] = {0} ", Density);
            sql.AppendFormat(", [Mallebility] = {0} ", Mallebility);
            sql.AppendFormat(", [Ductility] = {0} ", Ductility);
            sql.AppendFormat(", [Porosity] = {0} ", Porosity);
            sql.AppendFormat(", [UnitMass] = {0} ", UnitMass);
            sql.AppendFormat(", [SolidPoint] = {0} ", SolidPoint);
            sql.AppendFormat(", [GasPoint] = {0} ", GasPoint);
            sql.AppendFormat(", [TemperatureRetention] = {0} ", TemperatureRetention);
            sql.AppendFormat(", [Resistance] = '{0}' ", SerializeResistances());
            sql.AppendFormat(", [Composition] = '{0}' ", SerializeCompositions());
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
