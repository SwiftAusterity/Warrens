using NetMud.DataStructure.Base.System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

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

            try
            {
                var baseType = GetDataTableName(typeof(T));
                var sql = string.Format("select * from [dbo].[{0}]", baseType.Name);

                var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        var newValue = Activator.CreateInstance(baseType) as IData;
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

        public static IList GetAll(Type t)
        {
            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(t);

            var returnList = (IList)Activator.CreateInstance(constructedListType);

            try
            {
                var baseType = GetDataTableName(t);
                var sql = string.Format("select * from [dbo].[{0}]", baseType.Name);

                var ds = SqlWrapper.RunDataset(sql, CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        var newValue = Activator.CreateInstance(baseType) as IData;
                        newValue.Fill(dr);
                        returnList.Add(Convert.ChangeType(newValue, t));
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

            parms.Add("value", sharedKeyValue);

            try
            {
                var baseType = GetDataTableName(typeof(T));
                var sql = string.Format("select * from [dbo].[{0}] where {1} = @value", baseType.Name, sharedKeyName);

                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        var newValue = Activator.CreateInstance(baseType) as IData;
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

            parms.Add("value", sharedKeyValue);

            try
            {
                var baseType = GetDataTableName(typeof(T));
                var sql = string.Format("select * from [dbo].[{0}] where {1} = @value", baseType.Name, sharedKeyName);
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    if (ds.Rows.Count > 1)
                        throw new InvalidOperationException("More than one row returned for shared key.");

                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(baseType) as IData;
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
            var parms = new Dictionary<string, object>();

            parms.Add("id", id);

            try
            {
                var baseType = GetDataTableName(typeof(T));
                var sql = string.Format("select * from [dbo].[{0}] where ID = @id", baseType.Name);
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(baseType) as IData;
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

        private static Type GetDataTableName(Type dataType)
        {
            if (dataType.IsInterface)
            {
                var dataAssembly = Assembly.Load("NetMud.Data");
                var implimentedTypes = dataAssembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(dataType) && !ty.IsInterface);

                if (implimentedTypes.Count() < 0)
                    throw new InvalidOperationException("Requested bad data type.");

                var instance = Activator.CreateInstance(implimentedTypes.First()) as IData;

                return instance.GetType();
            }

            return dataType;
        }
    }
}
