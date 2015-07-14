using NetMud.Data.Base.System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var sql = "";

            var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

            if (ds.HasErrors)
            {
                //TODO: Error handling logging?
            }
            else if(ds.Rows != null)
            {
                foreach (DataRow dr in ds.Rows)
                {
                    try
                    {
                        var newValue = Activator.CreateInstance(typeof(T)) as IReference;
                        newValue.Fill(dr);
                        returnList.Add((T)newValue);
                    }
                    catch
                    {
                        //error logging
                    }
                }
            }

            return returnList;
        }

        public T GetOneReference<T>(string keyword) where T : IReference
        {
            IReference returnValue = default(T);
            var sql = "";

            var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

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
                        returnValue = Activator.CreateInstance(typeof(T)) as IReference;
                        returnValue.Fill(dr);
                    }
                    catch
                    {
                        //error logging
                    }
                }
            }

            return (T)returnValue;
        }
    }
}
