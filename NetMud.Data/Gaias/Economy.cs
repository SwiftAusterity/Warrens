using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Gaias
{
    /// <summary>
    /// Economy parent object for keeping track of economic trends
    /// </summary>
    [Serializable]
    public class Economy : IEconomy
    {
        /// <summary>
        /// Item type basises
        /// </summary>
        public HashSet<IEconomicBasis> Bases { get; set; }

        /// <summary>
        /// Quality valuation trends
        /// </summary>
        public HashSet<IEconomicTrend> Trends { get; set; }

        public Economy()
        {
            //blank constructor
            Bases = new HashSet<IEconomicBasis>();
            Trends = new HashSet<IEconomicTrend>();
        }

        public Economy(IGaiaTemplate world)
        {
            Bases = new HashSet<IEconomicBasis>();
            Trends = new HashSet<IEconomicTrend>();

            //We'll generate a new full set economy here
            IEnumerable<IInanimateTemplate> items = TemplateCache.GetAllOfMine<IInanimateTemplate>(world);

            foreach(DataStructure.Architectural.EntityBase.IQuality quality in items.SelectMany(obj => obj.Qualities))
            {
                MakeValuation(quality.Name);
            }

            foreach(IInanimateTemplate item in items)
            {
                MakeValuation(item);
            }
        }

        /// <summary>
        /// Forces the system to valuate the object type and adds it to the Bases
        /// </summary>
        /// <param name="basis">the item to value</param>
        /// <returns>the new value</returns>
        public decimal MakeValuation(IInanimateTemplate basis)
        {
            IEconomicBasis basi = Bases.FirstOrDefault(bas => bas.ItemType == basis);

            if (basi != null)
                return basi.Basis * basi.Adjustment;

            int newBaseValue = basis.Name.Length;

            if (basis.Qualities.Any())
            {
                newBaseValue += Math.Max(1, basis.Qualities.Sum(quality => quality.Value));
            }

            newBaseValue *= basis.Uses.Count();
            newBaseValue *= Math.Max(1, basis.Interactions.Count() / 2);
            newBaseValue *= Math.Max(1, basis.DecayEvents.Count() / 5);

            EconomicBasis newBasis = new EconomicBasis()
            {
                Adjustment = 1,
                Basis = newBaseValue,
                ItemType = basis,
                Trend = 1
            };

            Bases.Add(newBasis);

            return newBasis.Basis * newBasis.Adjustment;
        }

        /// <summary>
        /// Forces the system to valuate the quality and adds it to the Trends
        /// </summary>
        /// <param name="trend">the quality to value</param>
        /// <returns>the new value</returns>
        public decimal MakeValuation(string trend)
        {
            IEconomicTrend trnd = Trends.FirstOrDefault(bas => bas.Quality.Equals(trend));

            if (trnd != null)
                return trnd.Basis * trnd.Adjustment;

            int newBaseValue = trend.Length;

            char[] ones = new char[9] { 'a', 'e', 'i', 'o', 'u', 't', 'r', 's', 'n' };
            char[] twos = new char[2] { 'd', 'g' };
            char[] threes = new char[9] { 'c', 'm', 'b', 'p', 'h', 'f', 'w', 'y', 'v' };
            char[] fives = new char[3] { 'j', 'x', 'k' };
            char[] eights = new char[2] { 'z', 'q' };

            newBaseValue += trend.Count(letter => ones.Contains(letter));
            newBaseValue += trend.Count(letter => twos.Contains(letter)) * 2;
            newBaseValue += trend.Count(letter => threes.Contains(letter)) * 3;
            newBaseValue += trend.Count(letter => fives.Contains(letter)) * 5;
            newBaseValue += trend.Count(letter => eights.Contains(letter)) * 8;

            EconomicTrend newTrend = new EconomicTrend()
            {
                Adjustment = 1,
                Basis = newBaseValue,
                Quality = trend,
                Trend = 1
            };

            Trends.Add(newTrend);

            return newTrend.Basis * newTrend.Adjustment;
        }

        /// <summary>
        /// Adjust the bases
        /// </summary>
        public void Adjust(IInanimateTemplate basis, decimal movement)
        {
            IEnumerable<IEconomicBasis> myBases = Bases.Where(basi => basi.ItemType == basis);

            foreach (IEconomicBasis basi in myBases)
            {
                basi.Adjustment *= movement;

                if (movement > 1)
                    basi.Trend += 1;
                else if (movement < 1)
                    basi.Trend -= 1;
            }
        }

        /// <summary>
        /// Adjust the trends
        /// </summary>
        public void Adjust(string trend, decimal movement)
        {
            IEnumerable<IEconomicTrend> myTrends = Trends.Where(trnd => trnd.Quality.Equals(trend));

            foreach (IEconomicTrend trnd in myTrends)
            {
                trnd.Adjustment *= movement;

                if (movement > 1)
                    trnd.Trend += 1;
                else if (movement < 1)
                    trnd.Trend -= 1;
            }
        }
    }
}
