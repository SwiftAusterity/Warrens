using System.Collections.Generic;

namespace NetMud.DataStructure.Base.PlayerConfiguration
{
    /// <summary>
    /// Players' account modular ui configuration to load with the client
    /// </summary>
    public interface IModularUIConfig
    {
        /// <summary>
        /// The modules to load. Module, quadrant
        /// </summary>
        IDictionary<IUIModule, int> Modules { get; set; }
    }
}
