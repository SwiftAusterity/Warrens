using NetMud.DataStructure.NaturalResource;

namespace NetMud.DataStructure.Zone
{
    public interface INaturalResourceSpawn<T> where T : INaturalResource
    {
        /// <summary>
        /// The resource at hand
        /// </summary>
        T Resource { get; set; }

        /// <summary>
        /// The factor in how much and how frequently these respawn on their own
        /// </summary>
        int RateFactor { get; set; }
    }
}
