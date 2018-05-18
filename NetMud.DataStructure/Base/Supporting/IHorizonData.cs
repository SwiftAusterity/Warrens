using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Pathway/Exit for going from and to larger structures
    /// </summary>
    public interface IHorizonData<T> where T : IEntityBackingData
    {
        /// <summary>
        /// Location this pathway leads to
        /// </summary>
        T ToLocationData { get; set; }

        /// <summary>
        /// The visual output of using this path
        /// </summary>
        ILexica VisualOutput { get; set; }

        /// <summary>
        /// The auditory output of using this path
        /// </summary>
        ILexica AuditoryOutput { get; set; }

        /// <summary>
        /// The auditory output of using this path
        /// </summary>
        ILexica OlefactoryOutput { get; set; }

    }
}
