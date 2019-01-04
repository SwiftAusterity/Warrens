using NetMud.DataStructure.Architectural;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.System
{
    /// <summary>
    /// Interface defining players connected to a server
    /// </summary>
    public interface IDescriptor : ILiveData
    {
        /// <summary>
        /// The assigned radius of the client app of tiles to show
        /// </summary>
        ValueRange<short> MapRadius { get; set; }

        string _userId { get; set; }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        void Open();

        /// <summary>
        /// Send just ui updates like inventory
        /// </summary>
        /// <returns>success</returns>
        bool SendUIUpdates();

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="strings">the output</param>
        /// <returns>success status</returns>
        bool SendOutput(IEnumerable<string> strings);

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="str">the output</param>
        /// <returns>success status</returns>
        bool SendOutput(string str);

        /// <summary>
        /// Sends the full map to the client
        /// </summary>
        /// <returns>Success</returns>
        bool SendMap();

        /// <summary>
        /// Sends map deltas to the client
        /// </summary>
        /// <param name="partialUpdates">the deltas for the map</param>
        /// <returns>success</returns>
        bool SendMapDeltas(HashSet<Tuple<long, long, string>> partialUpdates);

        /// <summary>
        /// Send a sound file to a player
        /// </summary>
        /// <param name="soundUri"></param>
        /// <returns></returns>
        bool SendSound(string soundUri);

        /// <summary>
        /// Disconnects this descriptor forcibly
        /// </summary>
        /// <param name="finalMessage">the final string to send the client</param>
        void Disconnect(string finalMessage);
    }
}
