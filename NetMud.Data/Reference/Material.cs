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
        public override string DataTableName { get { return "Material"; } }

        public bool Conductive { get; set; }
        public bool Magnetic { get; set; }
        public bool Flammable { get; set; }
         
        public short Viscosity { get; set; }
        public short Density { get; set; }
        public short Mallebility { get; set; }
        public short Ductility { get; set; }
        public short Porosity { get; set; }
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
                long compId = long.Parse(comp.Name);
                short amount = comp.Value;

                compositions.Add(ReferenceWrapper.GetOne<Material>(compId), amount);
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
        public override void Fill(DataRow dr)
        {
            ID = DataUtility.GetFromDataRow<long>(dr, "ID");
            Created = DataUtility.GetFromDataRow<DateTime>(dr, "Created");
            LastRevised = DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised");
            Name = DataUtility.GetFromDataRow<string>(dr, "Name");

            Conductive = DataUtility.GetFromDataRow<bool>(dr, "Conductive");
            Magnetic = DataUtility.GetFromDataRow<bool>(dr, "Magnetic");
            Flammable = DataUtility.GetFromDataRow<bool>(dr, "Flammable");

            Viscosity = DataUtility.GetFromDataRow<short>(dr, "Viscosity");
            Density = DataUtility.GetFromDataRow<short>(dr, "Density");
            Mallebility = DataUtility.GetFromDataRow<short>(dr, "Mallebility");
            Ductility = DataUtility.GetFromDataRow<short>(dr, "Ductility");
            Porosity = DataUtility.GetFromDataRow<short>(dr, "Porosity");
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
            var parms = new Dictionary<string, object>();

            Material returnValue = default(Material);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Material]([Name],[Conductive],[Magnetic],[Flammable]");
            sql.Append(",[Viscosity],[Density],[Mallebility],[Ductility],[Porosity],[SolidPoint],[GasPoint],[TemperatureRetention],[Resistance],[Composition])");
            sql.Append(" values(@Name,@Conductive,@Magnetic,@Flammable,@Viscosity,@Density,@Mallebility,@Ductility,@Porosity,@SolidPoint,@GasPoint,@TemperatureRetention,@Resistance,@Composition)");
            sql.Append(" select * from [dbo].[Material] where ID = Scope_Identity()");

            parms.Add("Name", Name);
            parms.Add("Conductive", Conductive);
            parms.Add("Magnetic", Magnetic);
            parms.Add("Flammable", Flammable);
            parms.Add("Viscosity", Viscosity);
            parms.Add("Density", Density);
            parms.Add("Mallebility", Mallebility);
            parms.Add("Ductility", Ductility);
            parms.Add("Porosity", Porosity);
            parms.Add("SolidPoint", SolidPoint);
            parms.Add("GasPoint", GasPoint);
            parms.Add("TemperatureRetention", TemperatureRetention);
            parms.Add("Resistance", SerializeResistances());
            parms.Add("Composition", SerializeCompositions());

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
            sql.Append("delete from [dbo].[Material] where ID = @id");

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
            sql.Append("update [dbo].[Material] set ");
            sql.Append(" [Name] = @Name ");

            sql.Append(", [Conductive] = @Conductive ");
            sql.Append(", [Magnetic] = @Magnetic ");
            sql.Append(", [Flammable] = @Flammable ");
            sql.Append(", [Viscosity] = @Viscosity ");
            sql.Append(", [Density] = @Density ");
            sql.Append(", [Mallebility] = @Mallebility ");
            sql.Append(", [Ductility] = @Ductility ");
            sql.Append(", [Porosity] = @Porosity ");
            sql.Append(", [SolidPoint] = @SolidPoint ");
            sql.Append(", [GasPoint] = @GasPoint ");
            sql.Append(", [TemperatureRetention] = @TemperatureRetention ");
            sql.Append(", [Resistance] = @Resistance ");
            sql.Append(", [Composition] = @Composition ");
            sql.Append(", [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("Conductive", Conductive);
            parms.Add("Magnetic", Magnetic);
            parms.Add("Flammable", Flammable);
            parms.Add("Viscosity", Viscosity);
            parms.Add("Density", Density);
            parms.Add("Mallebility", Mallebility);
            parms.Add("Ductility", Ductility);
            parms.Add("Porosity", Porosity);
            parms.Add("SolidPoint", SolidPoint);
            parms.Add("GasPoint", GasPoint);
            parms.Add("TemperatureRetention", TemperatureRetention);
            parms.Add("Resistance", SerializeResistances());
            parms.Add("Composition", SerializeCompositions());

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
