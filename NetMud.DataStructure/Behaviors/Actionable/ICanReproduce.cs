namespace NetMud.DataStructure.Behaviors.Actionable
{
    /// <summary>
    /// Provides methods for generational spawned entities
    /// </summary>
    public interface ICanReproduce
    {
        /// <summary>
        /// How can something reproduce
        /// </summary>
        ReproductionMethod ReproductionType { get; set; }

        /// <summary>
        /// How many babies does this thing usually make
        /// </summary>
        short SpawningCoefficient { get; set; }

        /// <summary>
        /// How likely are we to get entirely foreign skill/stat values
        /// </summary>
        short TraitMutationCoefficient { get; set; }

        /// <summary>
        /// How much skill/stats are passed on from the parent(s)
        /// </summary>
        short TraitInheritanceCoefficient { get; set; }

        /// <summary>
        /// Do the deed
        /// </summary>
        /// <param name="mate">the thing we're schtuping</param>
        /// <returns>success</returns>
        bool Schtup(ICanReproduce mate);
    }

    /// <summary>
    /// How can something reproduce
    /// </summary>
    public enum ReproductionMethod
    {
        ASexual,
        Sexual,
        Division,
        None
    }
}
