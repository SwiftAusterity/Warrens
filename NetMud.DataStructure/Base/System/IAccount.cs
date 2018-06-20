using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.PlayerConfiguration;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// A player's "user"
    /// </summary>
    public interface IAccount
    {
        /// <summary>
        /// Unique string key for player user accounts
        /// </summary>
        string GlobalIdentityHandle { get; set; }

        /// <summary>
        /// What log channels a player is subscribe to listen to
        /// </summary>
        IList<string> LogChannelSubscriptions { get; set; }

        /// <summary>
        /// What characters this account owns
        /// </summary>
        IList<ICharacter> Characters { get; set; }

        /// <summary>
        /// The config values for this account
        /// </summary>
        IAccountConfig Config { get; set; }

        /// <summary>
        /// Id for currently selected character
        /// </summary>
        long CurrentlySelectedCharacter { get; set; }

        /// <summary>
        /// Add a character to this user
        /// </summary>
        /// <param name="newCharacter">the character to add</param>
        /// <returns>success status</returns>
        string AddCharacter(ICharacter newCharacter);
    }
}
