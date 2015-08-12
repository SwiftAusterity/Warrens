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
    public class InanimateData : EntityBackingDataPartial, IInanimateData
    {
        public InanimateData()
        {
            MobileContainers = new HashSet<IEntityContainerData<IMobile>>();
            InanimateContainers = new HashSet<IEntityContainerData<IInanimate>>();
        }

        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
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

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public override void Fill(global::System.Data.DataRow dr)
        {
            long outId = default(long);
            DataUtility.GetFromDataRow<long>(dr, "ID", ref outId);
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

            string mobileContainerJson = default(string);
            DataUtility.GetFromDataRow<string>(dr, "MobileContainers", ref mobileContainerJson);
            dynamic mobileContainers = JsonConvert.DeserializeObject(mobileContainerJson);

            foreach(dynamic mobileContainer in mobileContainers)
            {
                var newContainer = new EntityContainerData<IMobile>();
                newContainer.CapacityVolume = mobileContainer.CapacityVolume;
                newContainer.CapacityWeight = mobileContainer.CapacityWeight;
                newContainer.Name = mobileContainer.Name;

                MobileContainers.Add(newContainer);
            }

            string inanimateContainerJson = default(string);
            DataUtility.GetFromDataRow<string>(dr, "InanimateContainers", ref inanimateContainerJson);
            dynamic inanimateContainers = JsonConvert.DeserializeObject(inanimateContainerJson);

            foreach (dynamic inanimateContainer in inanimateContainers)
            {
                var newContainer = new EntityContainerData<IInanimate>();
                newContainer.CapacityVolume = inanimateContainer.CapacityVolume;
                newContainer.CapacityWeight = inanimateContainer.CapacityWeight;
                newContainer.Name = inanimateContainer.Name;

                InanimateContainers.Add(newContainer);
            }
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            IInanimateData returnValue = default(IInanimateData);

            var inanimateContainersJson = JsonConvert.SerializeObject(InanimateContainers);
            var mobileContainersJson = JsonConvert.SerializeObject(MobileContainers);

            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[InanimateData]([Name], [MobileContainers], [InanimateContainers])");
            sql.AppendFormat(" values('{0}', '{1}', '{2}')", Name, mobileContainersJson, inanimateContainersJson);
            sql.Append(" select * from [dbo].[InanimateData] where ID = Scope_Identity()");

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
            sql.AppendFormat("delete from [dbo].[InanimateData] where ID = {0}", ID);

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

            var inanimateContainersJson = JsonConvert.SerializeObject(InanimateContainers);
            var mobileContainersJson = JsonConvert.SerializeObject(MobileContainers);

            sql.Append("update [dbo].[InanimateData] set ");
            sql.AppendFormat(" [Name] = '{0}' ", Name);
            sql.AppendFormat(" , [MobileContainers] = '{0}' ", mobileContainersJson);
            sql.AppendFormat(" , [InanimateContainers] = '{0}' ", inanimateContainersJson);
            sql.AppendFormat(" , [LastRevised] = GetUTCDate()");
            sql.AppendFormat(" where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }
    }
}
