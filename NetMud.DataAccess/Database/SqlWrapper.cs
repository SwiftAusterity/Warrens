using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace NetMud.DataAccess.Database
{
    /// <summary>
    /// Internal wrapper class for invoking sql queries
    /// </summary>
    public static class SqlWrapper
    {
        /// <summary>
        /// The connection string to the db
        /// </summary>
        private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        /// <summary>
        /// Run a query with no expected return value and no parameters
        /// </summary>
        /// <param name="sqlText">The sql for the query</param>
        /// <param name="commandType">What type of sql query are we running</param>
        public static void RunNonQuery(string sqlText, CommandType commandType)
        {
            RunNonQuery(sqlText, commandType, new Dictionary<string, object>());
        }

        /// <summary>
        /// Run a query with no expected return value
        /// </summary>
        /// <param name="sqlText">The sql for the query</param>
        /// <param name="commandType">What type of sql query are we running</param>
        /// <param name="args">parameters being passed to the query</param>
        public static void RunNonQuery(string sqlText, CommandType commandType, IDictionary<string, object> args)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sqlText;
                cmd.CommandType = commandType;

                foreach (KeyValuePair<string, object> kvp in args)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Run a query with a base type return value and no parameters
        /// </summary>
        /// <param name="sqlText">The sql for the query</param>
        /// <param name="commandType">What type of sql query are we running</param>
        public static T RunScalar<T>(string sqlText, CommandType commandType)
        {
            return RunScalar<T>(sqlText, commandType, new Dictionary<string, object>());
        }

        /// <summary>
        /// Run a query with base type return value
        /// </summary>
        /// <param name="sqlText">The sql for the query</param>
        /// <param name="commandType">What type of sql query are we running</param>
        /// <param name="args">parameters being passed to the query</param>
        public static T RunScalar<T>(string sqlText, CommandType commandType, IDictionary<string, object> args)
        {
            T returnThing;
 
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sqlText;
                cmd.CommandType = commandType;

                foreach (KeyValuePair<string, object> kvp in args)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                conn.Open();
                returnThing = (T)cmd.ExecuteScalar();
            }

            return returnThing;
        }

        /// <summary>
        /// Run a query with table data return values and no parameters
        /// </summary>
        /// <param name="sqlText">The sql for the query</param>
        /// <param name="commandType">What type of sql query are we running</param>
        public static DataTable RunDataset(string sqlString, CommandType commandType)
        {
            return RunDataset(sqlString, commandType, new Dictionary<string, object>());
        }

        /// <summary>
        /// Run a query with table data return values
        /// </summary>
        /// <param name="sqlText">The sql for the query</param>
        /// <param name="commandType">What type of sql query are we running</param>
        /// <param name="args">parameters being passed to the query</param>
        public static DataTable RunDataset(string sqlString, CommandType commandType, IDictionary<string, object> args)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sqlString;
                cmd.CommandType = commandType;

                foreach (KeyValuePair<string, object> kvp in args)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }
    }
}
