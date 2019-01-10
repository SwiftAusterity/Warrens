using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Architectural.ActorBase
{
    public interface ICanBeWounded
    {
        /// <summary>
        /// Maximum hitpoints
        /// </summary>
        int CurrentLife { get; set; }

        /// <summary>
        /// Inflict a wound here
        /// </summary>
        /// <param name="source">the origin, nullable</param>
        /// <param name="victim">the victim</param>
        /// <returns>success or failure</returns>
        bool Inflict(IEntity source);

        /// <summary>
        /// Attempt to cure this of something
        /// </summary>
        /// <param name="source">the source trying to fix the problem, nullable</param>
        /// <param name="strength">How strong the attempt is</param>
        /// <returns>Was something cured</returns>
        string Cure(IEntity source, int strength);
    }
}
