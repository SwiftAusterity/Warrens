using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Utility
{
    public static class DataUtility
    {
        public static bool TryConvert<T>(object thing, ref T newThing)
        {
            try
            {
                if (thing == null)
                    return false;

                newThing = (T)thing;

                return true;
            }
            catch
            {
                //Def some logging here
            }

            return false;
        }

        public static bool GetFromDataRow<T>(DataRow dr, string columnName, ref T thing)
        {
            try
            {
                if (dr == null || !dr.Table.Columns.Contains(columnName))
                    return false;

                return TryConvert<T>(dr[columnName], ref thing);
            }
            catch
            {
                //Def some logging here
            }

            return false;
        }

        public static IEnumerable<Type> GetAllImplimentingedTypes(Type t)
        {
            var implimentedTypes = t.Assembly.GetTypes().Where(ty => ty.GetInterfaces().Contains(t) || ty == t);
            return implimentedTypes.Concat(t.GetInterfaces());
        }
    }
}
