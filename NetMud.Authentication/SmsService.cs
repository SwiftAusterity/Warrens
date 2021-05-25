using System.Threading.Tasks;

namespace NetMud.Authentication
{
    /// <summary>
    /// Sends a message via sms to a user
    /// </summary>
    public class SmsService : IIdentityMessageService
    {
        /// <summary>
        /// Send a message via sms to a user
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <returns>this function's results</returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
