using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;

namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// The account configuration for a player
    /// </summary>
    public interface IAccountConfig : IConfigData
    {
        /// <summary>
        /// What account owns this config
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// The UI language for output purposes
        /// </summary>
        ILanguage UILanguage { get; set; }

        /// <summary>
        /// Attempt to restore the config from file
        /// </summary>
        /// <returns>False = no file, True = file</returns>
        bool RestoreConfig(IAccount account);
    }
}
