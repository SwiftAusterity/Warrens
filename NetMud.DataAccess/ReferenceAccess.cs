using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Data;

namespace NetMud.DataAccess
{
    public static class ReferenceAccess
    {
        public static IEnumerable<T> GetAll<T>() where T : IReference
        {
            var returnList = new List<T>();
            var sql = string.Format("select * from [dbo].[{0}]", typeof(T).Name);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        var newValue = Activator.CreateInstance(typeof(T)) as IReference;
                        newValue.Fill(dr);
                        returnList.Add((T)newValue);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return returnList;
        }

        public static T GetOne<T>(string keyword) where T : IReference
        {
            IReference returnValue = default(T);
            var parms = new Dictionary<string, object>();
            var sql = string.Format("select * from [dbo].[{0}] where Name = @name", typeof(T).Name);

            parms.Add("name", keyword);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(typeof(T)) as IReference;
                        returnValue.Fill(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return (T)returnValue;
        }

        public static T GetOne<T>(long id) where T : IReference
        {
            IReference returnValue = default(T);
            var parms = new Dictionary<string, object>();
            var sql = string.Format("select * from [dbo].[{0}] where ID = @id", typeof(T).Name);

            parms.Add("id", id);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(typeof(T)) as IReference;
                        returnValue.Fill(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return (T)returnValue;
        }
    }
}
