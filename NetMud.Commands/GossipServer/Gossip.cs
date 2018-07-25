using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.Data.Lexical;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Commands.GossipServer
{
    [CommandKeyword("gossip", false, true, true)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), new CacheReferenceType[] { CacheReferenceType.Entity }, "[a-zA-z]+@[a-zA-z]+", true)]
    [CommandParameter(CommandUsage.Subject, typeof(string), new CacheReferenceType[] { CacheReferenceType.Entity }, "@[a-zA-z]+", true)]
    [CommandParameter(CommandUsage.Target, typeof(string), new CacheReferenceType[] { CacheReferenceType.Text }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Gossip : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Gossip()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var sb = new List<string>();
            IPlayer playerActor = Actor.GetType().GetInterfaces().Contains(typeof(IPlayer)) ? Actor as IPlayer : null;

            if (playerActor != null && !playerActor.DataTemplate<ICharacter>().Account.Config.GossipSubscriber)
                sb.Add(string.Format("You have disabled the Gossip network.", Subject));
            else
            {
                var directTarget = string.Empty;
                var directTargetGame = string.Empty;

                if (Subject != null)
                {
                    var names = Subject.ToString().Split(new char[] { '@' });

                    if(names.Count() == 2)
                    {
                        directTarget = names[0];
                        directTargetGame = names[1];
                    }
                    else if(names.Count() == 1)
                    {
                        directTarget = names[0];
                    }
                }

                var gossipClient = LiveCache.Get<IGossipClient>("GossipWebClient");

                var userName = Actor.DataTemplateName;

                if (playerActor != null)
                    userName = playerActor.AccountHandle;

                if (!string.IsNullOrWhiteSpace(directTarget) && !string.IsNullOrWhiteSpace(directTargetGame))
                {
                    gossipClient.SendDirectMessage(userName, directTargetGame, directTarget, Target.ToString());
                    sb.Add(string.Format("You tell {1}@{2} '{0}'", Target, directTarget, directTargetGame));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(directTarget))
                    {
                        gossipClient.SendMessage(userName, Target.ToString());
                        sb.Add(string.Format("You gossip '{0}'", Target));
                    }
                    else
                    {
                        gossipClient.SendMessage(userName, Target.ToString(), directTarget);
                        sb.Add(string.Format("You {1} '{0}'", Target, directTarget));
                    }
                }
            }

            var toActor = new Message(MessagingType.Audible, new Occurrence() { Strength = 1 })
            {
                Override = sb
            };

            //TODO: language outputs
            var messagingObject = new MessageCluster(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                "Valid Syntax: gossip &lt;text&gt;",
                "gossip @&lt;channel&gt; &lt;text&gt;".PadWithString(14, "&nbsp;", true),
                "gossip &lt;username&gt;@&lt;gamename&gt; &lt;text&gt;".PadWithString(14, "&nbsp;", true)
            };

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override MarkdownString HelpText
        {
            get
            {
                return string.Format("Gossip allows you to speak over the gossip inter-mud chat network.");
            }
            set { }
        }
    }
}
