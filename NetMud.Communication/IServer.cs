namespace NetMud.Communication
{
    /// <summary>
    /// Interface for communication servers
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// Sends a message to all live descriptors everywhere
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <returns>success status</returns>
        bool Broadcast(string message);

        /// <summary>
        /// Sends a message to all live descriptors on a port
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="portNumber">the port of the connection to send to</param>
        /// <returns>success status</returns>
        bool Broadcast(string message, int portNumber);

        /// <summary>
        /// Registers this for a service on a port
        /// </summary>
        /// <param name="portNumber">the port it is listening on</param>
        void Launch(int portNumber);

        /// <summary>
        /// Shuts down the port, or all if -1
        /// </summary>
        /// <param name="portNumber">Port to close, otherwise -1 shuts entire channel down</param>
        void Shutdown(int portNumber = -1);

        /// <summary>
        /// Gets an active listener service
        /// </summary>
        /// <param name="portNumber">the port it is listening on</param>
        /// <returns>the service</returns>
        T GetActiveService<T>(int portNumber);
    }
}
