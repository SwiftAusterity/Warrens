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
    public class RoomData : IRoomData
    {
        public Type EntityClass
        {
            get { return typeof(NetMud.Data.Game.Room); }
        }

        public long ID { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastRevised { get; set; }
        public string Name { get; set; }

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
        }

        public IData Create()
        {
            IRoomData returnValue = default(IRoomData);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[RoomData]([Name])");
            sql.AppendFormat(" values('{0}')", Name);
            sql.Append(" select * from [dbo].[RoomData] where ID = Scope_Identity()");

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

        public bool Remove()
        {
            //TODO: Exits too?
            var sql = new StringBuilder();
            sql.AppendFormat("delete from [dbo].[RoomData] where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        public bool Save()
        {
            var sql = new StringBuilder();
            sql.Append("update [dbo].[RoomData] set ");
            sql.AppendFormat(" [Name] = '{0}' ", Name);
            sql.AppendFormat(" , [LastRevised] = GetUTCDate()");
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
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
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
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
    }
}
