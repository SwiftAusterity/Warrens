using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Data;
using System.Text;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for pathways
    /// </summary>
    public class PathwayData : IPathwayData
    {    
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        public Type EntityClass
        {
            get { return typeof(NetMud.Data.Game.Pathway); }
        }

        /// <summary>
        /// Numerical iterative ID in the db
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// When this was first created in the db
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// When this was last revised in the db
        /// </summary>
        public DateTime LastRevised { get; set; }

        /// <summary>
        /// The unique name for this entry (also part of the accessor keywords)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// How wide this pathway portal is
        /// </summary>
        public long PassingWidth { get; set; }

        /// <summary>
        /// How high the pathway portal is
        /// </summary>
        public long PassingHeight { get; set; }

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
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public void Fill(global::System.Data.DataRow dr)
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

            string outTitle = default(string);
            DataUtility.GetFromDataRow<string>(dr, "Name", ref outTitle);
            Name = outTitle;


            string outToRoomID = default(string);
            DataUtility.GetFromDataRow<string>(dr, "ToLocationID", ref outToRoomID);
            ToLocationID = outToRoomID;

            string outFromRoomID = default(string);
            DataUtility.GetFromDataRow<string>(dr, "FromLocationID", ref outFromRoomID);
            FromLocationID = outFromRoomID;

            string outToRoomType = default(string);
            DataUtility.GetFromDataRow<string>(dr, "ToLocationType", ref outToRoomType);
            ToLocationType = outToRoomType;

            string outFromRoomType = default(string);
            DataUtility.GetFromDataRow<string>(dr, "FromLocationType", ref outFromRoomType);
            FromLocationType = outFromRoomType;

            string outMessageToDestination = default(string);
            DataUtility.GetFromDataRow<string>(dr, "MessageToDestination", ref outMessageToDestination);
            MessageToDestination = outMessageToDestination;

            string outMessageToOrigin = default(string);
            DataUtility.GetFromDataRow<string>(dr, "MessageToOrigin", ref outMessageToOrigin);
            MessageToOrigin = outMessageToOrigin;

            string outMessageToActor = default(string);
            DataUtility.GetFromDataRow<string>(dr, "MessageToActor", ref outMessageToActor);
            MessageToActor = outMessageToActor;

            string outAudibleToSurroundings = default(string);
            DataUtility.GetFromDataRow<string>(dr, "AudibleToSurroundings", ref outAudibleToSurroundings);
            AudibleToSurroundings = outAudibleToSurroundings;

            string outVisibleToSurroundings = default(string);
            DataUtility.GetFromDataRow<string>(dr, "VisibleToSurroundings", ref outVisibleToSurroundings);
            VisibleToSurroundings = outVisibleToSurroundings;

            int outAudibleStrength = default(int);
            DataUtility.GetFromDataRow<int>(dr, "AudibleStrength", ref outAudibleStrength);
            AudibleStrength = outAudibleStrength;

            int outVisibleStrength = default(int);
            DataUtility.GetFromDataRow<int>(dr, "VisibleStrength", ref outVisibleStrength);
            VisibleStrength = outVisibleStrength;
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public IData Create()
        {
            IPathwayData returnValue = default(IPathwayData);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[PathwayData]([Name],[ToLocationID],[FromLocationID],[ToLocationType],[FromLocationType],[MessageToDestination],[MessageToOrigin]");
            sql.Append(",[MessageToActor],[AudibleToSurroundings],[VisibleToSurroundings],[AudibleStrength],[VisibleStrength])");
            sql.AppendFormat(" values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',{10},{11})"
                , Name, ToLocationID, FromLocationID, ToLocationType, FromLocationType, MessageToDestination, MessageToOrigin
                , MessageToActor, AudibleToSurroundings, VisibleToSurroundings, AudibleStrength, VisibleStrength);
            sql.Append(" select * from [dbo].[PathwayData] where ID = Scope_Identity()");

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
        public bool Remove()
        {
            //TODO: Exits too?
            var sql = new StringBuilder();
            sql.AppendFormat("delete from [dbo].[PathwayData] where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public bool Save()
        {
            var sql = new StringBuilder();
            sql.Append("update [dbo].[PathwayData] set ");
            sql.AppendFormat(" [Name] = '{0}' ", Name);
            sql.AppendFormat(", [ToLocationID] = '{0}' ", ToLocationID);
            sql.AppendFormat(", [FromLocationID] = '{0}' ", FromLocationID);
            sql.AppendFormat(", [ToLocationType] = '{0}' ", ToLocationType);
            sql.AppendFormat(", [FromLocationType] = '{0}' ", FromLocationType);
            sql.AppendFormat(", [MessageToDestination] = '{0}' ", MessageToDestination);
            sql.AppendFormat(", [MessageToOrigin] = '{0}' ", MessageToOrigin);
            sql.AppendFormat(", [MessageToActor] = '{0}' ", MessageToActor);
            sql.AppendFormat(", [AudibleToSurroundings] = '{0}' ", AudibleToSurroundings);
            sql.AppendFormat(", [VisibleToSurroundings] = '{0}' ", VisibleToSurroundings);
            sql.AppendFormat(", [AudibleStrength] = {0} ", AudibleStrength);
            sql.AppendFormat(", [VisibleStrength] = {0} ", VisibleStrength);
            sql.AppendFormat(", [LastRevised] = GetUTCDate()");
            sql.AppendFormat(" where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(IData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != typeof(Room))
                        return -1;

                    if (other.ID.Equals(this.ID))
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IData other)
        {
            if (other != default(IData))
            {
                try
                {
                    return other.GetType() == typeof(Room) && other.ID.Equals(this.ID);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
    }
}
