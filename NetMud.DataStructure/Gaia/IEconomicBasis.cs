using NetMud.DataStructure.Inanimate;

namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Item type basises
    /// </summary>
    public interface IEconomicBasis
    {
        IInanimateTemplate ItemType { get; set; }

        int Basis { get; set; }

        short Trend { get; set; }

        decimal Adjustment { get; set; }
    }
}
