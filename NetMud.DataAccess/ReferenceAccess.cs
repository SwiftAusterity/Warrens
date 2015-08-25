using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace NetMud.DataAccess
{
    /// <summary>
    /// Provides accessor methods into Reference Data in the db
    /// </summary>
    public static class ReferenceWrapper
    {
        /// <summary>
        /// Get all of the reference data in the table
        /// </summary>
        /// <typeparam name="T">The system type of the data</typeparam>
        /// <returns>A list of all of the data for that type</returns>
        public static IEnumerable<T> GetAll<T>() where T : IReferenceData
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
                        var newValue = Activator.CreateInstance(baseType) as IReferenceData;
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

            parms.Add("name", keyword);

            try
            {
                var baseType = GetDataTableName(typeof(T));
                var sql = string.Format("select * from [dbo].[{0}] where Name = @name", baseType.Name);
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        returnValue = Activator.CreateInstance(baseType) as IReferenceData;
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
                        returnValue = Activator.CreateInstance(baseType) as IReferenceData;
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

                var instance = Activator.CreateInstance(implimentedTypes.First()) as IReferenceData;

                return instance.GetType();
            }

            return dataType;
        }
    }
}
