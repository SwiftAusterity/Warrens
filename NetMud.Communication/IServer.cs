using System.Collections.Generic;

namespace NetMud.Communication
{
    /// <summary>
    /// Interface for communication servers
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// Port this server is bound to
        /// </summary>
        int PortNumber { get; }

        /// <summary>
        /// Sends a message to all live descriptors everywhere
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <returns>success status</returns>
        bool Broadcast(string message);

        /// <summary>
        /// Get all connected clients
        /// </summary>
        /// <returns>All clients connected through this server/port</returns>
        IList<IDescriptor> ConnectedClients { get; }

        /// <summary>
        /// Registers this for a service on a port
        /// </summary>
        /// <param name="portNumber">the port it is listening on</param>
        void Launch(int portNumber);

        /// <summary>
        /// Shuts down the port, or all if -1
        /// </summary>
        /// <param name="portNumber">Port to close, otherwise -1 shuts entire channel down</param>
        void Shutdown();

        /// <summary>
        /// Gets an active listener service
        /// </summary>
        /// <param name="portNumber">the port it is listening on</param>
        /// <returns>the service</returns>
        T GetActiveService<T>();
    }
}
