using NetMud.Data.DataIntegrity;
using NetMud.Data.Lexical;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Animal spawns
    /// </summary>
    [Serializable]
    public class Fauna : NaturalResourceDataPartial, IFauna
    {
        /// <summary>
        /// What is the % chance of generating a female instead of a male on birth
        /// </summary>
        [IntDataIntegrity("Female to male ratio must be greater than 0.", 1)]
        public int FemaleRatio { get; set; }

        /// <summary>
        /// The absolute hard cap to natural population growth
        /// </summary>
        [IntDataIntegrity("Population Hard Cap must be greater than 0.", 1)]
        public int PopulationHardCap { get; set; }

        [JsonProperty("Race")]
        private BackingDataCacheKey _race { get; set; }

        /// <summary>
        /// What we're spawning
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [NonNullableDataIntegrity("Race must be set.")]
        public IRace Race
        {
            get
            {
                return BackingDataCache.Get<IRace>(_race);
            }
            set
            {
                _race = new BackingDataCacheKey(value);
            }
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Race", Race.Name);
            returnList.Add("Female Ratio", FemaleRatio.ToString());
            returnList.Add("Population Cap", PopulationHardCap.ToString());

            return returnList;
        }

        #region Rendering
        /// <summary>
        /// Render a natural resource collection to a viewer
        /// </summary>
        /// <param name="viewer">the entity looking</param>
        /// <param name="amount">How much of it there is</param>
        /// <returns>a view string</returns>
        public override IOccurrence RenderResourceCollection(IEntity viewer, int amount)
        {
            if (!IsVisibleTo(viewer))
                return null;

            var me = GetSelf(MessagingType.Visible);
            var collectiveNoun = new Lexica(LexicalType.Noun, GrammaticalType.Descriptive, Race.CollectiveNoun);
            collectiveNoun.TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, amount.ToString()));
            me.Event.TryModify(collectiveNoun);

            return me;
        }
        #endregion
    }
}
