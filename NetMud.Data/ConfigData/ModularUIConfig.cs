using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;

namespace NetMud.Data.ConfigData
{
    /// <summary>
    /// Players' account modular ui configuration to load with the client
    /// </summary>
    [Serializable]
    public class ModularUIConfig : ConfigData, IModularUIConfig
    {
        /// <summary>
        /// The type of data this is (for storage)
        /// </summary>
        public override ConfigDataType Type { get { return ConfigDataType.GameWorld; } }

        /// <summary>
        /// The modules to load. Module, quadrant
        /// </summary>
        public IDictionary<IUIModule, int> Modules { get; set; }

        public ModularUIConfig()
        {
            Modules = new Dictionary<IUIModule, int>();
        }
    }
}
