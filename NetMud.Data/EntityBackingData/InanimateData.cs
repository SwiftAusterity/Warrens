using NetMud.Data.Reference;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for Inanimate objects
    /// </summary>
    [Serializable]
    public class InanimateData : EntityBackingDataPartial, IInanimateData
    {
        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        public InanimateData()
        {
            MobileContainers = new HashSet<IEntityContainerData<IMobile>>();
            InanimateContainers = new HashSet<IEntityContainerData<IInanimate>>();
            InternalComposition = new Dictionary<IInanimateData, short>();
        }

        public InanimateData(string internalCompositionJson)
        {
            MobileContainers = new HashSet<IEntityContainerData<IMobile>>();
            InanimateContainers = new HashSet<IEntityContainerData<IInanimate>>();

            InternalComposition = DeserializeInternalCompositions(internalCompositionJson);
        }

        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        [NonSerialized]
        public override Type EntityClass
        {
            get { return typeof(Game.Inanimate); }
        }

        /// <summary>
        /// Definition for the room's capacity for mobiles
        /// </summary>
        public HashSet<IEntityContainerData<IMobile>> MobileContainers { get; set; }

        /// <summary>
        /// Definition for the room's capacity for inanimates
        /// </summary>
        public HashSet<IEntityContainerData<IInanimate>> InanimateContainers { get; set; }


        public IDictionary<IInanimateData, short> InternalComposition { get; set; }

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

            string mobileContainerJson = DataUtility.GetFromDataRow<string>(dr, "MobileContainers");
            
            dynamic mobileContainers = JsonConvert.DeserializeObject(mobileContainerJson);
            foreach(dynamic mobileContainer in mobileContainers)
            {
                var newContainer = new EntityContainerData<IMobile>();
                newContainer.CapacityVolume = mobileContainer.CapacityVolume;
                newContainer.CapacityWeight = mobileContainer.CapacityWeight;
                newContainer.Name = mobileContainer.Name;

                MobileContainers.Add(newContainer);
            }

            string inanimateContainerJson = DataUtility.GetFromDataRow<string>(dr, "InanimateContainers");
         
            dynamic inanimateContainers = JsonConvert.DeserializeObject(inanimateContainerJson);
            foreach (dynamic inanimateContainer in inanimateContainers)
            {
                var newContainer = new EntityContainerData<IInanimate>();
                newContainer.CapacityVolume = inanimateContainer.CapacityVolume;
                newContainer.CapacityWeight = inanimateContainer.CapacityWeight;
                newContainer.Name = inanimateContainer.Name;

                InanimateContainers.Add(newContainer);
            }

            string internalCompositionJson = DataUtility.GetFromDataRow<string>(dr, "InternalComposition");
            InternalComposition = DeserializeInternalCompositions(internalCompositionJson);

            Model = new DimensionalModel(dr);
        }

        private IDictionary<IInanimateData, short> DeserializeInternalCompositions(string compJson)
        {
            var composition = new Dictionary<IInanimateData, short> ();

            dynamic comps = JsonConvert.DeserializeObject(compJson);

            foreach (dynamic comp in comps)
            {
                long id = comp.Item1;
                short percentage = comp.Item2;

                var objData = DataWrapper.GetOne<InanimateData>(id);

                if (objData != null && percentage > 0)
                    composition.Add(objData, percentage);
            }

            return composition;
        }

        public string SerializeInternalCompositions()
        {
            var materialComps = new List<Tuple<long, short>>();

            foreach (var kvp in InternalComposition)
                materialComps.Add(new Tuple<long, short>(kvp.Key.ID, kvp.Value));

            return JsonConvert.SerializeObject(materialComps);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            var parms = new Dictionary<string, object>();

            IInanimateData returnValue = default(IInanimateData);

            var inanimateContainersJson = JsonConvert.SerializeObject(InanimateContainers);
            var mobileContainersJson = JsonConvert.SerializeObject(MobileContainers);

            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[InanimateData]([Name], [MobileContainers], [InanimateContainers]");
            sql.Append(", [DimensionalModelLength], [DimensionalModelHeight], [DimensionalModelWidth], [DimensionalModelID], [DimensionalModelMaterialCompositions]");
            sql.Append(", [InternalComposition])");
            sql.AppendFormat(" values('{0}', '{1}', '{2}', {3}, {4}, {5}, {6}, '{7}', '{8}')"
                , Name, mobileContainersJson, inanimateContainersJson
                , Model.Height, Model.Length, Model.Width, Model.ModelBackingData.ID, Model.SerializeMaterialCompositions()
                , SerializeInternalCompositions());
            sql.Append(" select * from [dbo].[InanimateData] where ID = Scope_Identity()");

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
            sql.Append("delete from [dbo].[InanimateData] where ID = @id");

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

            var inanimateContainersJson = JsonConvert.SerializeObject(InanimateContainers);
            var mobileContainersJson = JsonConvert.SerializeObject(MobileContainers);

            sql.Append("update [dbo].[InanimateData] set ");
            sql.Append(" [Name] = @Name ");
            sql.Append(" , [MobileContainers] = @MobileContainers ");
            sql.Append(" , [InanimateContainers] = @InanimateContainers ");
            sql.Append(" , [InternalComposition] = @InternalComposition ");
            sql.Append(" , [DimensionalModelLength] = @DimensionalModelLength");
            sql.Append(" , [DimensionalModelHeight] = @DimensionalModelHeight ");
            sql.Append(" , [DimensionalModelWidth] = @DimensionalModelWidth ");
            sql.Append(" , [DimensionalModelMaterialCompositions] = @DimensionalModelMaterialCompositions ");
            sql.Append(" , [DimensionalModelId] = @DimensionalModelId "); 
            sql.Append(" , [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("MobileContainers", mobileContainersJson);
            parms.Add("InanimateContainers", inanimateContainersJson);
            parms.Add("InternalComposition", SerializeInternalCompositions());
            parms.Add("DimensionalModelLength",  Model.Length);
            parms.Add("DimensionalModelHeight", Model.Height);
            parms.Add("DimensionalModelWidth", Model.Width);
            parms.Add("DimensionalModelMaterialCompositions", Model.SerializeMaterialCompositions());
            parms.Add("DimensionalModelId", Model.ModelBackingData.ID);


            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text, parms);

            return true;
        }
    }
}
