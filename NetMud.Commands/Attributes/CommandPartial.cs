using NetMud.CentralControl;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Commands.Attributes
{
    public abstract class CommandPartial : ICommand, IHelpful
    {
        /// <summary>
        /// The command word originally used to find this command
        /// </summary>
        public string CommandWord { get; set; }

        /// <summary>
        /// The original input that spawned this
        /// </summary>
        public string OriginalInput { get; set; }

        /// <summary>
        /// The entity invoking the command
        /// </summary>
        public IActor Actor { get; set; }

        /// <summary>
        /// The entity the command refers to
        /// </summary>
        public object Subject { get; set; }

        /// <summary>
        /// When there is a predicate parameter, the entity that is being targetting (subject become "with")
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Any tertiary entity being referenced in command parameters
        /// </summary>
        public object Supporting { get; set; }

        /// <summary>
        /// The delay this invokes when executing. Initially is "begun" and actually takes affect at the end.
        /// </summary>
        public virtual int ExecutionDelay => 0;

        /// <summary>
        /// The delay this invokes after being executed
        /// </summary>
        public virtual int CooldownDelay => 0;

        /// <summary>
        /// Container the Actor is in when the command is invoked
        /// </summary>
        public IGlobalPosition OriginLocation { get; set; }

        /// <summary>
        /// Valid containers by range from OriginLocation
        /// </summary>
        public IEnumerable<ILocation> Surroundings { get; set; }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public abstract MarkdownString HelpText { get; set; }

        public virtual void Execute(Func<string, IActor, bool> nextAction)
        {
            if (ExecutionDelay + CooldownDelay == 0 || GetType().GetCustomAttributes(true).Any(attr => attr.GetType() == typeof(CommandQueueSkip)))
            {
                //No delay, no queues just do it
                ExecutionBody();
                return;
            }

            if (!string.IsNullOrWhiteSpace(Actor.CurrentAction))
            {
                Actor.InputBuffer.Add(OriginalInput);
                Actor.WriteTo(new string[] { "Command added to queue. (PEEK to see queue, FLUSH to empty queue, HALT to end execution, STOP to halt and flush)" });
                return;
            }

            //Take care of what we're doing and what we will do, the Interpreter handles the actual queue and if your command goes through or not. If it's here its allowed.
            Actor.CurrentAction = OriginalInput;

            var nextInput = Actor.InputBuffer.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(nextInput) && nextInput == OriginalInput)
            {
                Actor.InputBuffer = Actor.InputBuffer.Skip(1).ToList();
            }

            if (Actor.InputBuffer.Count() == 0)
            {
                Processor.StartSingeltonLoop(string.Format("CommandExecution_{0}_{1}", Actor.TemplateName, CommandWord)
                    , ExecutionDelay, CooldownDelay, (ExecutionDelay + CooldownDelay) * 10, () => ExecutionBody());
            }
            else
            {
                Func<bool> next = () => nextAction(Actor.InputBuffer.FirstOrDefault(), Actor);

                Processor.StartSingeltonLoop(string.Format("CommandExecution_{0}_{1}", Actor.TemplateName, CommandWord)
                    , ExecutionDelay, CooldownDelay, (ExecutionDelay + CooldownDelay) * 10, () => ExecutionBody(), next, next);

            }
        }

        internal abstract bool ExecutionBody();

        public abstract IEnumerable<string> RenderSyntaxHelp();

        public virtual IEnumerable<string> RenderHelpBody()
        {
            List<string> sb = new List<string>
            {
                HelpText
            };

            return sb;
        }

        public virtual void RenderError(string error)
        {
            Message messagingObject = new Message(error);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null, 0);
        }
    }
}
