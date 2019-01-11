namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Framework for entities having a gender
    /// </summary>
    public interface IGender
    {
        /// <summary>
        /// Freeform string for entity gender
        /// </summary>      
        string Gender { get; set; }
    }
}
