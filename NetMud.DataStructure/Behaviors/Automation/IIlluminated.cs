using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Behavioral pattern for things that can produce light
    /// </summary>
    public interface IIlluminated
    {
        /// <summary>
        /// How much illumination this produces
        /// </summary>
        int Luminosity { get; set; }

        /// <summary>
        /// How much illumination this is giving off including other factors
        /// </summary>
        /// <returns>lumins</returns>
        int EffectiveLuminosoity();
    }
}
