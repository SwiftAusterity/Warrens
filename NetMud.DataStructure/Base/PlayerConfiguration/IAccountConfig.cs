using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.PlayerConfiguration
{
    /// <summary>
    /// The account configuration for a player
    /// </summary>
    public interface IAccountConfig : IData
    {
        /// <summary>
        /// Account data object unique key
        /// </summary>
        string AccountHandle { get; set; }

        /// <summary>
        /// What account owns this config
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// Whether or not the person wants the tutorial tooltips on; false = off
        /// </summary>
        bool UITutorialMode { get; set; }

        /// <summary>
        /// The ui config for this player
        /// </summary>
        IModularUIConfig UIConfig { get; set; }
    }
}
