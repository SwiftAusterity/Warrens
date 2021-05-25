using System.Threading.Tasks;

namespace NetMud.Authentication
{
    /// <summary>
    /// Service that emails users things (pwd reset mails, two-step verifications, etc)
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        /// <summary>
        /// Send a message via email to a user
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <returns>this function's results</returns>
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }
}
