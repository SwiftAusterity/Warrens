using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Data;

namespace NetMud.DataAccess
{
    public class ReferenceAccess
    {
        //Not even sure what to do here yet
        public ReferenceAccess()
        {
        }

        public IEnumerable<T> GetAllReference<T>() where T : IReference
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

        public T GetOneReference<T>(string keyword) where T : IReference
        {
            IReference returnValue = default(T);
            var sql = string.Format("select * from [dbo].[{0}] where Name = '{1}'", typeof(T).Name, keyword);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

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
