namespace NetMud.DataStructure.Inanimate
{
    /// <summary>
    /// For things that can bunch together
    /// </summary>
    public interface ICanAccumulate
    {
        /// <summary>
        /// How many of the thing is there
        /// </summary>
        int AccumulationCap { get; set; }
    }
}
