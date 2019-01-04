namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Functional properties for stuff that can be seen
    /// </summary>
    public interface IVisible
    {
        /// <summary>
        /// The character displayed on the visual map
        /// </summary>
        string AsciiCharacter { get; set; }

        /// <summary>
        /// The hex code of the color of the ascii character
        /// </summary>
        string HexColorCode { get; set; }

        /// <summary>
        /// The long description of the entity
        /// </summary>
        string Description { get; set; }
    }
}
