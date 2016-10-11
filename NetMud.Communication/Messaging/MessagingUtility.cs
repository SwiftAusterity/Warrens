using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System.Collections.Generic;
using System.Linq;
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
            int i = 0;
            for (i = 0; i < messages.Length; i++)
                messages[i] = TranslateColorVariables(messages[i], recipient);

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
            int i = 0;
            for(i = 0; i < messages.Length; i++)
                messages[i] = TranslateEntityVariables(messages[i], entities);

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
                        continue;

                    thing = kvp.Value[0];
                }

                switch (kvp.Key)
                {
                    case MessagingTargetType.Actor:
                        message = message.Replace("$A$", thing.DataTemplateName);
                        break;
                    case MessagingTargetType.DestinationLocation:
                        message = message.Replace("$D$", thing.DataTemplateName);
                        break;
                    case MessagingTargetType.OriginLocation:
                         message = message.Replace("$O$", thing.DataTemplateName);
                        break;
                    case MessagingTargetType.Subject:
                        message = message.Replace("$S$", thing.DataTemplateName);
                        break;
                    case MessagingTargetType.Target:
                        message = message.Replace("$T$", thing.DataTemplateName);
                        break;
                    case MessagingTargetType.GenderPronoun:
                        if (!thing.GetType().GetInterfaces().Contains(typeof(IGender)))
                            break;

                        IGender chr = (IGender)thing;
                        message = message.Replace("$G$", chr.Gender);
                        break;
                    case MessagingTargetType.AmountOfSubject:
                        message = message.Replace("$#S$", kvp.Value.Length.ToString());
                        break;
                    case MessagingTargetType.AmountOfTarget:
                        message = message.Replace("$#T$", kvp.Value.Length.ToString());
                        break;
                    case MessagingTargetType.Direction:
                    case MessagingTargetType.ReverseDirection:
                        if (!thing.GetType().GetInterfaces().Contains(typeof(IPathway)))
                            break;

                        var pathData = thing.DataTemplate<IPathwayData>();
                        message = message.Replace("$DIR$", RenderUtility.TranslateDegreesToDirection(pathData.DegreesFromNorth, kvp.Key == MessagingTargetType.ReverseDirection).ToString());
                        break;
                }
            }

            return message;
        }
    }
}
