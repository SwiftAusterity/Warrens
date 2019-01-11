using NetMud.DataStructure.NaturalResource;

namespace NetMud.DataStructure.Zone
{
    public interface INaturalResourceSpawn
    {
        /// <summary>
        /// The resource at hand
        /// </summary>
        INaturalResource Resource { get; set; }

        /// <summary>
        /// The factor in how much and how frequently these respawn on their own
        /// </summary>
        int RateFactor { get; set; }
    }
}
