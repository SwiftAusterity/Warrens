using NetMud.DataStructure.Inanimate;
using NetMud.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMud.Data.Inanimates
{
    /// <summary>
    /// A stack of Items
    /// </summary>
    public class ItemStack : IItemStack
    {
        /// <summary>
        /// The item template
        /// </summary>
        public IInanimateTemplate Item { get; set; }

        /// <summary>
        /// The size of the entire stack
        /// </summary>
        public int FullStackSize { get; set; }

        /// <summary>
        /// The rendered description (including quality breakdowns)
        /// </summary>
        public string Description { get; set; }

        public ItemStack()
        {
            FullStackSize = 1;
        }

        public ItemStack(IEnumerable<IInanimate> thePile)
        {
            if(thePile == null || thePile.Count() == 0)
            {
                FullStackSize = 0;
                Description = string.Empty;
                Item = null;
            }

            FullStackSize = thePile.Count();
            Item = thePile.First().Template<IInanimateTemplate>();

            StringBuilder sb = new StringBuilder();

            IInanimate plainItem = thePile.FirstOrDefault(item => item.Qualities == null || item.Qualities.Count() == 0);

            if (plainItem != null)
            {
                sb.AppendFormattedLine("({0}) {1}", thePile.Count(item => item.Qualities == null || item.Qualities.Count() == 0), Item.Name);
            }

            foreach(IGrouping<string, IInanimate> currentPair in thePile
                .Where(item => item.Qualities != null && item.Qualities.Count() > 0)
                .GroupBy(item => string.Join(",", item.Qualities.Select(quality => string.Format("{0}:{1}", quality.Name, quality.Value)))))
            {
                int count = currentPair.Count();
                string qualities = currentPair.Key;

                sb.AppendFormattedLine("({0}) {1} [{2}]", count, Item.Name, qualities);
            }

            Description = sb.ToString();
        }
    }
}
