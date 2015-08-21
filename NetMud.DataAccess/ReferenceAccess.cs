using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Data;

namespace NetMud.DataAccess
{
    /// <summary>
    /// Provides accessor methods into Reference Data in the db
    /// </summary>
    public static class ReferenceWrapper
    {
        private static string GetDataTableName(Type dataType)
        {
            var instance = Activator.CreateInstance(dataType) as IReferenceData;

            return instance.DataTableName;
        }

        /// <summary>
        /// Get all of the reference data in the table
        /// </summary>
        /// <typeparam name="T">The system type of the data</typeparam>
        /// <returns>A list of all of the data for that type</returns>
        public static IEnumerable<T> GetAll<T>() where T : IReferenceData
        {
            var returnList = new List<T>();
            var sql = string.Format("select * from [dbo].[{0}]", GetDataTableName(typeof(T)));

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        var newValue = Activator.CreateInstance(typeof(T)) as IReferenceData;
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

        /// <summary>
        /// Gets a single reference data entry by name
        /// </summary>
        /// <typeparam name="T">The system type of the data</typeparam>
        /// <param name="keyword">The "Name" of the reference data entry</param>
        /// <returns>One data entry, if it matches</returns>
        public static T GetOne<T>(string keyword) where T : IReferenceData
        {
            IReferenceData returnValue = default(T);
            var parms = new Dictionary<string, object>();
            var sql = string.Format("select * from [dbo].[{0}] where Name = @name", GetDataTableName(typeof(T)));

            parms.Add("name", keyword);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(typeof(T)) as IReferenceData;
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

        /// <summary>
        /// Gets a single reference data entry by ID
        /// </summary>
        /// <typeparam name="T">The system type of the data</typeparam>
        /// <param name="id">The "ID" of the reference data entry</param>
        /// <returns>One data entry, if it matches</returns>
        public static T GetOne<T>(long id) where T : IReferenceData
        {
            IReferenceData returnValue = default(T);
            var parms = new Dictionary<string, object>();
            var sql = string.Format("select * from [dbo].[{0}] where ID = @id", GetDataTableName(typeof(T)));

            parms.Add("id", id);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(typeof(T)) as IReferenceData;
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
