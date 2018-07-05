using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.PlayerConfiguration
{
    /// <summary>
    /// Messages to players
    /// </summary>
    public interface IPlayerMessage : IConfigData
    {
        //Name = SenderHandle

        /// <summary>
        /// The body of the message
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// Subject of the message
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Account of the sender
        /// </summary>
        IAccount Sender { get; set; }

        /// <summary>
        /// Name of the recipient character (can be blank)
        /// </summary>
        string RecipientName { get; set; }

        /// <summary>
        /// Recipeint character
        /// </summary>
        ICharacter Recipient { get; set; }

        /// <summary>
        /// Is this important? Does it make the UI bell ring
        /// </summary>
        bool Important { get; set; }
    }
}
