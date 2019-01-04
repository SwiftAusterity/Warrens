using System;
using System.Collections.Generic;

namespace NetMud.Gossip
{
    /// <summary>
    /// A user of the gossip network in your system
    /// </summary>
    public class Member
    {
        /// <summary>
        /// Their username
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The function that allows messages to be sent to them
        /// </summary>
        public Action<string> WriteTo { get; set; }

        /// <summary>
        /// What usernames are on the block list
        /// </summary>
        public IEnumerable<string> BlockedMembers { get; set; }

        /// <summary>
        /// What friends are there (friends get notifications)
        /// </summary>
        public IEnumerable<string> Friends { get; set; }

        public Member()
        {
            BlockedMembers = new string[0];
            Friends = new string[0];
        }
    }
}
