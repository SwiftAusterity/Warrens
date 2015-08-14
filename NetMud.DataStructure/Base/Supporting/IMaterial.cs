using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Reference data for what things are made of
    /// </summary>
    public interface IMaterial : IReferenceData
    {
        bool Raw { get; set; }
        bool Conductive { get; set; }
        bool Magnetic { get; set; }
        bool Flammable { get; set; }

        short Viscosity { get; set; }
        short Density { get; set; }
        short Mallebility { get; set; }
        short Ductility { get; set; }
        short Porosity { get; set; }
        short UnitMass { get; set; }
        short SolidPoint { get; set; }
        short GasPoint { get; set; }

        short TemperatureRetention { get; set; }
        IDictionary<DamageType, short> Resistance { get; set; }
        IDictionary<IMaterial, short> Composition { get; set; }
    }
}
