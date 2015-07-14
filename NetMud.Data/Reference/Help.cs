using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Reference
{
    public class Help : IReference
    {
        public Help()
        {
            ID = -1;
            Created = DateTime.UtcNow;
            LastRevised = DateTime.UtcNow;
            Name = "NotImpl";
            HelpText = "NotImpl";
        }

        public long ID { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastRevised { get; set; }
        public string Name { get; set; }

        public string HelpText { get; private set; }

        public void Fill(global::System.Data.DataRow dr)
        {
            int outId = default(int);
            DataUtility.GetFromDataRow<int>(dr, "ID", ref outId);
            ID = outId;

            DateTime outCreated = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "Created", ref outCreated);
            Created = outCreated;

            DateTime outRevised = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised", ref outRevised);
            LastRevised = outRevised;

            string outName = default(string);
            DataUtility.GetFromDataRow<string>(dr, "Name", ref outName);
            Name = outName;

            string outHelpText = default(string);
            DataUtility.GetFromDataRow<string>(dr, "HelpText", ref outHelpText);
            HelpText = outHelpText;
        }

         /// <summary>
         /// -99 = null input
         /// -1 = wrong type
         /// 0 = same type, wrong id
         /// 1 = same reference (same id, same type)
         /// </summary>
         /// <param name="obj"></param>
         /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj != null)
            {
                try
                {
                    if (obj.GetType() != typeof(Help))
                        return -1;

                    IReference otherObj = obj as IReference;

                    if (otherObj.ID.Equals(this.ID))
                        return 1;

                    return 0;
                }
                catch
                {
                    //Minor error logging
                }
            }

            return -99;
        }

        public bool Equals(IReference other)
        {
            if (other != default(IReference))
            {
                try
                {
                    return other.GetType() == typeof(Help) && other.ID.Equals(this.ID);
                }
                catch
                {
                    //Minor error logging
                }
            }

            return false;
        }

        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(HelpText);

            return sb;
        }
    }
}
