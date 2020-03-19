using System;
using System.IO;
using System.Web.Hosting;

namespace NetMud.DataAccess
{
    public static class ContentUtility
    {
        /// <summary>
        /// Verifies the existence of or creates a new directory, also creates the base directory if necessary
        /// </summary>
        /// <param name="directoryName">the directory to create</param>
        /// <param name="createIfMissing">creates the directory if it doesn't already exist</param>
        /// <returns>success</returns>
        private static bool VerifyDirectory(string directoryName)
        {
            string mappedName = directoryName;

            if (!mappedName.EndsWith("/"))
            {
                mappedName += "/";
            }

            try
            {
                return Directory.Exists(HostingEnvironment.MapPath(mappedName));
            }
            catch (Exception ex)
            {
                //Log any filesystem errors
                LoggingUtility.LogError(ex, false);
            }

            return false;
        }
    }
}
