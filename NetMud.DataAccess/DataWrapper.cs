using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Data;

namespace NetMud.DataAccess
{
    /// <summary>
    /// Provides accessor methods into Backing Data in the db
    /// </summary>
    public static class DataWrapper
    {
        /// <summary>
        /// Get all of the data in the table
        /// </summary>
        /// <typeparam name="T">The system type of the data</typeparam>
        /// <returns>A list of all of the data for that type</returns>
        public static IEnumerable<T> GetAll<T>() where T : IData
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
                        var newValue = Activator.CreateInstance(typeof(T)) as IData;
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
        /// Get all of the data in a table that matches a key value
        /// </summary>
        /// <typeparam name="T">The system type of the data</typeparam>
        /// <typeparam name="sharedKeyName">Thet db column name to check against</typeparam>
        /// <typeparam name="sharedKeyValue">The value to match (exact match)</typeparam>
        /// <returns>A list of all of the data returned</returns>
        public static IEnumerable<T> GetAllBySharedKey<T>(string sharedKeyName, string sharedKeyValue) where T : IData
        {
            var returnList = new List<T>();
            var parms = new Dictionary<string, object>();
            var sql = string.Format("select * from [dbo].[{0}] where {1} = @value", typeof(T).Name, sharedKeyName, sharedKeyValue);

            parms.Add("value", sharedKeyValue);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        var newValue = Activator.CreateInstance(typeof(T)) as IData;
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
        /// Get a single entry from a table that matches a key value
        /// </summary>
        /// <typeparam name="T">The system type of the data</typeparam>
        /// <typeparam name="sharedKeyName">Thet db column name to check against</typeparam>
        /// <typeparam name="sharedKeyValue">The value to match (exact match)</typeparam>
        /// <returns>The one data that matches of the data returned</returns>
        public static T GetOneBySharedKey<T>(string sharedKeyName, string sharedKeyValue) where T : IData
        {
            IData returnValue = default(T);
            var parms = new Dictionary<string, object>();
            var sql = string.Format("select * from [dbo].[{0}] where {1} = @value", typeof(T).Name, sharedKeyName, sharedKeyValue);

            parms.Add("value", sharedKeyValue);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    if (ds.Rows.Count > 1)
                        throw new InvalidOperationException("More than one row returned for shared key.");

                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(typeof(T)) as IData;
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
        /// Gets a single backing data entry by ID
        /// </summary>
        /// <typeparam name="T">The system type of the data</typeparam>
        /// <param name="id">The "ID" of the reference data entry</param>
        /// <returns>One data entry, if it matches</returns>
        public static T GetOne<T>(long id) where T : IData
        {
            IData returnValue = default(T);
            var sql = string.Format("select * from [dbo].[{0}] where ID = {1}", typeof(T).Name, id);

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(typeof(T)) as IData;
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
        /// Scrubs interface type names
        /// </summary>
        /// <param name="t">the type to find the right name for</param>
        /// <returns>the name</returns>
        private static string ScrubTypeName(Type t)
        {
            if (t.IsInterface)
                return t.Name.Substring(1);

            return t.Name;
        }
    }
}
