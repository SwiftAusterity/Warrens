using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace NetMud.DataAccess
{
    public static class SqlWrapper
    {
        private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public static void RunNonQuery(string sqlText, CommandType commandType)
        {
            RunNonQuery(sqlText, commandType, new Dictionary<string, object>());
        }

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
        public static T RunScalar<T>(string sqlText, CommandType commandType)
        {
            return RunScalar<T>(sqlText, commandType, new Dictionary<string, object>());
        }

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
        public static DataTable RunDataset(string sqlString, CommandType commandType)
        {
            return RunDataset(sqlString, commandType, new Dictionary<string, object>());
        }

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
