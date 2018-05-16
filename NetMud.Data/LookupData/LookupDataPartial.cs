using NetMud.Data.System;
using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.Data.LookupData
{
    public abstract class LookupDataPartial : BackingDataPartial, ILookupData
    {
        public LookupDataPartial()
        {
            //empty instance for getting the dataTableName
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (string.IsNullOrWhiteSpace(HelpText))
                dataProblems.Add("Help text empty.");

            return dataProblems;
        }

        public string HelpText { get; set; }

        public virtual IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>
            {
                HelpText
            };

            return sb;
        }
    }
}
