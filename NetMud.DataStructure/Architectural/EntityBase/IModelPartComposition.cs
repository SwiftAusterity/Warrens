namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// What model parts are made of
    /// </summary>
    public interface IModelPartComposition
    {
        /// <summary>
        /// The name of the part section on the model
        /// </summary>
        string PartName { get; set; }

        /// <summary>
        /// The material it's made of
        /// </summary>
        IMaterial Material { get; set; }
    }
}
