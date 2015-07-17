﻿using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.EntityBackingData
{
    public class PathData : IPathData
    {    
        public Type EntityClass
        {
            get { return typeof(NetMud.Data.Game.Path); }
        }

        public long ID { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastRevised { get; set; }
        public string Name { get; set; }

        public long ToLocationID { get; set; }
        public string ToLocationType { get; set; }


        public long FromLocationID { get; set; }
        public string FromLocationType { get; set; }

        public string MessageToActor { get; set; }
        public string MessageToOrigin { get; set; }
        public string MessageToDestination { get; set; }
        public string AudibleToSurroundings { get; set; }
        public int AudibleStrength { get; set; }
        public string VisibleToSurroundings { get; set; }
        public int VisibleStrength { get; set; }

        public void Fill(global::System.Data.DataRow dr)
        {
            int outId = default(int);
            DataUtility.GetFromDataRow<int>(dr, "ID", ref outId);
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


            long outToRoomID = default(long);
            DataUtility.GetFromDataRow<long>(dr, "ToLocationID", ref outToRoomID);
            ToLocationID = outToRoomID;

            long outFromRoomID = default(long);
            DataUtility.GetFromDataRow<long>(dr, "FromLocationID", ref outFromRoomID);
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

        public IData Create()
        {
            IPathData returnValue = default(IPathData);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[Path]([Name],[ToLocationID],[FromLocationID],[ToLocationType],[FromLocationType],[MessageToDestination],[MessageToOrigin]");
            sql.Append(",[MessageToActor],[AudibleToSurroundings],[VisibleToSurroundings],[AudibleStrength],[VisibleStrength])");
            sql.AppendFormat(" values('{0}',{1},{2},'{3}','{4}','{5}','{6}','{7}','{8}','{9}',{10},{11})"
                , Name, ToLocationID, FromLocationID, ToLocationType, FromLocationType, MessageToDestination, MessageToOrigin
                , MessageToActor, AudibleToSurroundings, VisibleToSurroundings, AudibleStrength, VisibleStrength);
            sql.Append(" select * from [dbo].[Path] where ID = Scope_Identity()");

            var ds = SqlWrapper.RunDataset(sql.ToString(), CommandType.Text);

            if (ds.HasErrors)
            {
                //TODO: Error handling logging?
            }
            else if (ds.Rows != null)
            {
                foreach (DataRow dr in ds.Rows)
                {
                    try
                    {
                        Fill(dr);
                        returnValue = this;
                    }
                    catch
                    {
                        //error logging
                    }
                }
            }

            return returnValue;
        }

        public bool Remove()
        {
            //TODO: Exits too?
            var sql = new StringBuilder();
            sql.AppendFormat("remove from [dbo].[Path] where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        public bool Save()
        {
            var sql = new StringBuilder();
            sql.Append("update [dbo].[Path] set ");
            sql.AppendFormat(" [Name] = '{0}' ", Name);
            sql.AppendFormat(", [ToLocationID] = {0} ", ToLocationID);
            sql.AppendFormat(", [FromLocationID] = {0} ", FromLocationID);
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
                catch
                {
                    //Minor error logging
                }
            }

            return -99;
        }

        public bool Equals(IData other)
        {
            if (other != default(IData))
            {
                try
                {
                    return other.GetType() == typeof(Room) && other.ID.Equals(this.ID);
                }
                catch
                {
                    //Minor error logging
                }
            }

            return false;
        }
    }
}
