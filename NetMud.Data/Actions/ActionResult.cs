using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Tile;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Action
{
    /// <summary>
    /// Results of using an action
    /// </summary>
    [Serializable]
    public class ActionResult : IActionResult
    {
        /// <summary>
        /// Target type of the criteria, what are we checking against
        /// </summary>
        public ActionTarget Target { get; set; }

        /// <summary>
        /// What occurrence group is this a part of
        /// </summary>
        public short OccurrenceChanceGroupId { get; set; }

        /// <summary>
        /// Percentage modifier (1-100) of being chosen as the prize within the occurrence group
        /// </summary>
        public decimal OccurrenceChanceRate { get; set; }

        /// <summary>
        /// The quality we're checking for
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// Is this quality additive or replace
        /// </summary>
        public bool AdditiveQuality { get; set; }

        /// <summary>
        /// The value we're adding to the quality
        /// </summary>
        public int QualityValue { get; set; }

        /// <summary>
        /// Is the thing we're looking at consumed entirely
        /// </summary>
        public bool Consumes { get; set; }

        /// <summary>
        /// Health damage done to the target
        /// </summary>
        public int HealthDamage { get; set; }

        /// <summary>
        /// Stamina damage done to the target
        /// </summary>
        public int StaminaDamage { get; set; }

        [JsonProperty("Produces")]
        private TemplateCacheKey _produces { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        public IInanimateTemplate Produces
        {
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_produces);
            }
            set
            {
                if (value != null)
                    _produces = new TemplateCacheKey(value);
                else
                    _produces = null;
            }
        }

        /// <summary>
        /// Does this produce items to the actor's inventory
        /// </summary>
        public bool ProducesToInventory { get; set; }

        /// <summary>
        /// How many items does it produce
        /// </summary>
        public int ProducesAmount { get; set; }

        [JsonProperty("Result")]
        private TemplateCacheKey _result { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        public ITileTemplate Result
        {
            get
            {
                return TemplateCache.Get<ITileTemplate>(_result);
            }
            set
            {
                if (value != null)
                    _result = new TemplateCacheKey(value);
                else
                    _result = null;
            }
        }

        public ActionResult()
        {
        }

        public object Clone()
        {
            ActionResult returnValue = new ActionResult
            {
                AdditiveQuality = AdditiveQuality,
                Consumes = Consumes,
                HealthDamage = HealthDamage,
                Produces = Produces,
                ProducesAmount = ProducesAmount,
                ProducesToInventory = ProducesToInventory,
                Quality = Quality,
                Result = Result,
                StaminaDamage = StaminaDamage,
                Target = Target
            };

            return returnValue;
        }
    }
}
