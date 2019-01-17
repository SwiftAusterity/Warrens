using NetMud.DataStructure.Inanimate;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Lookup Data for what things are made of
    /// </summary>
    public interface IMaterial : ILookupData, ICanAccumulate, IDescribable
    {
        /// <summary>
        /// Is this material energy conduction
        /// </summary>     
        bool Conductive { get; set; }

        /// <summary>
        /// Is this material magnetic
        /// </summary>       
        bool Magnetic { get; set; }

        /// <summary>
        /// Is this material flammable
        /// </summary>     
        bool Flammable { get; set; }

        /// <summary>
        /// How viscous is this material (higher = more viscous)
        /// </summary>      
        short Viscosity { get; set; }

        /// <summary>
        /// How dense is this material
        /// </summary>     
        short Density { get; set; }

        /// <summary>
        /// How well does this material bend without breaking
        /// </summary>     
        short Mallebility { get; set; }

        /// <summary>
        /// How stretchable is this material
        /// </summary>
        short Ductility { get; set; }

        /// <summary>
        /// How porous is this material
        /// </summary>
        short Porosity { get; set; }

        /// <summary>
        /// What is the freezing point of this material
        /// </summary>
        short SolidPoint { get; set; }

        /// <summary>
        /// What is the temperature gasous point of this material
        /// </summary>
        short GasPoint { get; set; }

        /// <summary>
        /// How well does this material hold temperature changes
        /// </summary>
        short TemperatureRetention { get; set; }

        /// <summary>
        /// Any elemental resistances the material has
        /// </summary>
        HashSet<DamageResistance> Resistance { get; set; }

        /// <summary>
        /// Is this material an alloy of other materials
        /// </summary>
        HashSet<IMaterialComposition> Composition { get; set; }
    }
}
