using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System;

namespace NetMud.DataStructure.Base.PlayerConfiguration
{
    /// <summary>
    /// Messages to players
    /// </summary>
    public interface IPlayerMessage : IConfigData
    {
        //Name = recipientAccountName
        /// <summary>
        /// The account recieving this
        /// </summary>
        IAccount RecipientAccount { get; set; }

        /// <summary>
        /// The body of the message
        /// </summary>
        MarkdownString Body { get; set; }

        /// <summary>
        /// Subject of the message
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Name of the sender character
        /// </summary>
        string SenderName { get; set; }

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

        /// <summary>
        /// Has this been read yet?
        /// </summary>
        bool Read { get; set; }

        /// <summary>
        /// When this was sent
        /// </summary>
        DateTime Sent { get; set; }
    }
}
