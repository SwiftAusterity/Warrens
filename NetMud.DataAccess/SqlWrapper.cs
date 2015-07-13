using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataAccess
{
    public static class SqlWrapper
    {
        private const string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ReferenceData"];

        public void RunNonQuery(string sqlText, CommandType commandType, IDictionary<string, object> args)
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

        public T RunScalar<T>(string storedProcName, CommandType commandType, IDictionary<string, object> args)
        {
            T returnThing = null;
 
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = storedProcName;
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

        public DataSet RunDataset(string storedProcName, CommandType commandType, IDictionary<string, object> args)
        {
            DataSet ds = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = storedProcName;
                cmd.CommandType = commandType;

                foreach (KeyValuePair<string, object> kvp in args)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    ds.Load(reader);
                }
            }

            return ds;
        }
    }
}
