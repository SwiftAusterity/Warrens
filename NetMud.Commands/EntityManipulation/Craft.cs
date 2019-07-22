using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.Utility;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("craft", false, new string[] { "make", "create" })]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), CacheReferenceType.String, @"\?$", true)]
    [CommandParameter(CommandUsage.Target, typeof(string), CacheReferenceType.String, false)]
    [CommandParameter(CommandUsage.Target, typeof(IInanimateTemplate), CacheReferenceType.Data, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Craft : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Craft()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            List<string> sb = new List<string>();

            if (!(Actor is IContains actorContainer))
            {
                RenderError("You can't craft if you don't have an inventory.");
                return false;
            }

            //We're after a list, not the actual command here
            if (Subject != null)
            {
                IEnumerable<IInanimateTemplate> itemsToMake = TemplateCache.GetAll<IInanimateTemplate>().Where(item => item.Produces > 0 && item.Components.Count() > 0);

                //Just a full list of all the things we can make then
                if (Target == null)
                {
                    foreach (IInanimateTemplate item in itemsToMake)
                    {
                        IEnumerable<IInanimate> myItems = actorContainer.GetContents<IInanimate>();
                        if (item.Components.Any(component => myItems.Any(myItem => myItem.TemplateId == component.Item.Id)))
                        {
                            sb.Add(item.RenderBlueprints(Actor));
                        }
                    }
                }
                else
                {
                    //A list of everything that matches keyword
                    string keyword = Target.ToString();

                    foreach (IInanimateTemplate item in itemsToMake.Where(itm => itm.Name.Contains(keyword)))
                    {
                        sb.Add(item.RenderBlueprints(Actor));
                    }
                }

                ILexicalParagraph toActor = new LexicalParagraph(sb.ToString());

                Message messagingObject = new Message(toActor);

                messagingObject.ExecuteMessaging(Actor, null, null, null, null);

                return true;
            }

            IInanimateTemplate itemToMake = (IInanimateTemplate)Target;

            string errorCrafting = itemToMake.Craft(Actor);

            if (!string.IsNullOrWhiteSpace(errorCrafting))
            {
                RenderError(errorCrafting);
            }
            else
            {
                sb.Add(string.Format("You craft {0} {1}{2}.", itemToMake.Produces, itemToMake.Name, itemToMake.Produces > 1 ? "s" : ""));

                ILexicalParagraph toActor = new LexicalParagraph(sb.ToString());

                ILexicalParagraph toOrigin = new LexicalParagraph(string.Format("$A$ crafts {0} {1}{2}.", itemToMake.Produces, itemToMake.Name, itemToMake.Produces > 1 ? "s" : ""));

                Message messagingObject = new Message(toActor)
                {
                    ToOrigin = new List<ILexicalParagraph> { toOrigin }
                };

                messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentZone, null);

                Actor.Save();
            }

            return true;
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: craft &lt;item name&gt;",
                "craft &lt;?&gt;".PadWithString(14, "&nbsp;", true),
                "craft &lt;?&gt; &lt;keyword&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Crafts a new item to your inventory using component parts. Use 'craft ? keyword' to find available things to craft, 'craft ?' to find things you can craft with what you have on hand or check the Items feature page in the top nav.");
            }
            set { }
        }
    }
}
