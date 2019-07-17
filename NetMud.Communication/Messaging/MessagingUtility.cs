using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NetMud.Communication.Messaging
{
    /// <summary>
    /// Utility methods for messaging output and translation
    /// </summary>
    public static class MessagingUtility
    {
        /// <summary>
        /// regex pattern color codes take
        /// </summary>
        private const string colorPattern = "\\%[a-zA-z]+\\%";

        public static Dictionary<string, SupportedColors> ColorGlyphs = new Dictionary<string, SupportedColors> 
        {
            { "%ST%",   SupportedColors.Bold        },
            { "%IT%",   SupportedColors.Italics     },
            { "%B%",    SupportedColors.Blue        },
            { "%b%",    SupportedColors.LightBlue   },
            { "%O%",    SupportedColors.Orange      },
            { "%o%",    SupportedColors.LightOrange },
            { "%Y%",    SupportedColors.Yellow      },
            { "%y%",    SupportedColors.LightYellow },
            { "%G%",    SupportedColors.Green       },
            { "%g%",    SupportedColors.LightGreen  },
            { "%I%",    SupportedColors.Indigo      },
            { "%i%",    SupportedColors.LightPurple },
            { "%R%",    SupportedColors.Red         },
            { "%r%",    SupportedColors.LightRed    },
            { "%P%",    SupportedColors.Pink        },
            { "%p%",    SupportedColors.LightPink   }
        };

        /// <summary>
        /// Translates output text with entity variables (he, she, it, names, directions, etc)
        /// </summary>
        /// <param name="messages">the text array to translate</param>
        /// <param name="recipient">the target entity of the message</param>
        /// <returns>translated text</returns>
        public static IEnumerable<string> TranslateColorVariables(string[] messages, IEntity recipient)
        {
            int i;
            for (i = 0; i < messages.Length; i++)
            {
                messages[i] = TranslateColorVariables(messages[i], recipient);
            }

            return messages;
        }

        /// <summary>
        /// Translates output text with color codes into proper output
        /// </summary>
        /// <param name="message">the text to translate</param>
        /// <param name="recipient">the target entity of the message</param>
        /// <returns>translated text</returns>
        public static string TranslateColorVariables(string message, IEntity recipient)
        {
            bool stillFound = true;
            Match currentMatch;

            while (stillFound &&
                    (currentMatch = Regex.Match(message, colorPattern)).Success)
            {
                //Need a way to short-circut some bozo creating an infinite loop
                stillFound = recipient.ConnectionType.ReplaceColor(ColorGlyphs[currentMatch.Value], currentMatch.Value, ref message);
            }

            return message;
        }

        /// <summary>
        /// Translates output text with entity variables (he, she, it, names, directions, etc)
        /// </summary>
        /// <param name="messages">the text array to translate</param>
        /// <returns>translated text</returns>
        public static IEnumerable<string> TranslateEntityVariables(string[] messages, Dictionary<MessagingTargetType, IEntity[]> entities)
        {
            for(int i = 0; i < messages.Length; i++)
            {
                messages[i] = TranslateEntityVariables(messages[i], entities);
            }

            return messages;
        }

        /// <summary>
        /// Translates output text with entity variables (he, she, it, names, directions, etc)
        /// </summary>
        /// <param name="message">the text to translate</param>
        /// <returns>translated text</returns>
        public static string TranslateEntityVariables(string message, Dictionary<MessagingTargetType, IEntity[]> entities)
        {
            foreach (KeyValuePair<MessagingTargetType, IEntity[]> kvp in entities)
            {
                IEntity thing = null;

                if (kvp.Value.Length.Equals(1))
                {
                    if (kvp.Value[0] == null)
                    {
                        continue;
                    }

                    thing = kvp.Value[0];
                }

                switch (kvp.Key)
                {
                    case MessagingTargetType.Actor:
                        message = message.Replace("$A$", thing.TemplateName);
                        message = message.Replace("$@A$", "it");
                        message = message.Replace("$V@A$", "its");
                        message = message.Replace("$P@A$", "them"); //TODO: gender construct, not in this version
                        message = message.Replace("$VP@A$", "their");
                        break;
                    case MessagingTargetType.DestinationLocation:
                        message = message.Replace("$D$", thing.TemplateName);
                        break;
                    case MessagingTargetType.OriginLocation:
                         message = message.Replace("$O$", thing.TemplateName);
                        break;
                    case MessagingTargetType.Subject:
                        message = message.Replace("$S$", thing.TemplateName);
                        message = message.Replace("$@S$", "it");
                        message = message.Replace("$V@S$", "its");
                        message = message.Replace("$P@S$", "them");
                        message = message.Replace("$VP@S$", "their");
                        break;
                    case MessagingTargetType.Target:
                        message = message.Replace("$T$", thing.TemplateName);
                        message = message.Replace("$@T$", "it");
                        message = message.Replace("$V@T$", "its");
                        message = message.Replace("$P@T$", "them");
                        message = message.Replace("$VP@T$", "their");
                        break;
                    case MessagingTargetType.AmountOfSubject:
                        message = message.Replace("$#S$", kvp.Value.Length.ToString());
                        break;
                    case MessagingTargetType.AmountOfTarget:
                        message = message.Replace("$#T$", kvp.Value.Length.ToString());
                        break;
                }
            }

            return message;
        }
    }
}
