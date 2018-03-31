namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Land ownership concerns for locales
    /// </summary>
    public interface IHomesteading
    {
        /// <summary>
        /// Base boolean for if homesteaded locales can be connected to here
        /// </summary>
        bool Homestead { get; set; }
    }
}
