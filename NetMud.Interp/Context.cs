using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMud.Data.Behaviors.Rendering;
using NetMud.Data.Base.System;
using System.Reflection;
using NetMud.Data.System;
using NetMud.DataAccess;

namespace NetMud.Interp
{
    public class Context
    {
        public string OriginalCommandString { get; private set; }
        public IEnumerable<string> CommandStringRemainder { get; private set; }

        private Assembly commandsAssembly;
        private Assembly entitiesAssembly;

        private LiveCache liveWorld;

        public IActor Actor { get; private set; }
        public ICommand Command { get; private set; }
        public object Subject { get; private set; }
        public object Target { get; private set; }
        public object Container { get; private set; }
        public ILocation Location { get; private set; }
        public IEnumerable<ILocation> Surroundings { get; private set; }

        public IList<string> AccessErrors { get; private set; }

        private IEnumerable<Type> LoadedCommands;

        public Context(string fullCommand, IActor actor)
        {
            commandsAssembly = Assembly.GetAssembly(typeof(ICommand));
            entitiesAssembly = Assembly.GetAssembly(typeof(IEntity));
            liveWorld = new LiveCache();

            OriginalCommandString = fullCommand;
            Actor = actor;
            AccessErrors = new List<string>();
            CommandStringRemainder = Enumerable.Empty<string>();

            LoadedCommands = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICommand)));

            //find out command's type
            var commandType = ParseCommand();
            if (commandType == null)
            {
                AccessErrors.Add("Unknown Command."); //TODO: Add generic errors class for rando error messages
                return;
            }

            //TODO: This works for commands targetting things not existing in the world
            //      existing objects must have an alternate path
            try
            {
                //find the parameters
                var neededParms = commandType.GetCustomAttributes<CommandParameterAttribute>();
                if (neededParms.Count() == 0)
                {
                    Command = Activator.CreateInstance(commandType) as ICommand;
                }
                else
                {
                    var parms = ParseParamaters(commandType, neededParms);

                    if (parms.Count() < neededParms.Count())
                    {
                        Command = Activator.CreateInstance(commandType) as ICommand;

                        AccessErrors.Add("Invalid command targets specified.");
                        AccessErrors = AccessErrors.Concat(Command.RenderSyntaxHelp()).ToList();
                        return;
                    }

                    //Invoke the constructor
                    Command = Activator.CreateInstance(commandType, parms) as ICommand;
                }
            }
            catch (MethodAccessException mEx)
            {
                Command = Activator.CreateInstance(commandType) as ICommand;
                AccessErrors.Add(mEx.Message);
                AccessErrors = AccessErrors.Concat(Command.RenderSyntaxHelp()).ToList();
            }
        }

        private Type ParseCommand()
        {
            var currentString = OriginalCommandString;

            //Dummy check empty strings
            if (string.IsNullOrWhiteSpace(currentString))
            {
                AccessErrors.Add("You have to tell me something first."); //TODO: Add generic errors class for rando error messages
                return null;
            }

            //Find the command we're trying to do

            //Split out the string
            var parsedWords = currentString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var commandWords = parsedWords.Length;
            Type command = null;

            while (commandWords > 0)
            {
                var currentCommandString = String.Join(" ", parsedWords.Take(commandWords)).ToLower();

                var validCommands = LoadedCommands.Where(comm => comm.GetCustomAttributes<CommandKeywordAttribute>().Any(att => att.Keyword.Equals(currentCommandString)));

                if (validCommands.Count() > 1)
                {
                    //TODO: Need to disambiguate help text here
                    AccessErrors.Add("DISAMBIGUATE ME");
                    break;
                }
                else if (validCommands.Count() == 1)
                {
                    command = validCommands.First();
                    CommandStringRemainder = parsedWords.Skip(commandWords);
                    break;
                }

                commandWords--;
            }

            return command;
        }

        private IEnumerable<object> ParseParamaters(Type commandType, IEnumerable<CommandParameterAttribute> neededParms)
        {
            var returnedParms = new List<object>();

            //Flip through each remaining word and parse them
            foreach (var currentNeededParm in neededParms)
            {
                foreach (var seekType in currentNeededParm.CacheTypes)
                {
                    switch (seekType)
                    {
                        case CacheReferenceType.Code:
                            SeekInCode(returnedParms, currentNeededParm);
                            break;
                        case CacheReferenceType.Entity:
                            //So damn ugly, make this not use reflection if possible
                            MethodInfo entityMethod = GetType().GetMethod("SeekInLiveWorld")
                                                         .MakeGenericMethod(new Type[] { currentNeededParm.ParameterType });
                            entityMethod.Invoke(this, new object[] { returnedParms, currentNeededParm, commandType.GetCustomAttribute<CommandRangeAttribute>() });
                            break;
                        case CacheReferenceType.Reference:
                            //So damn ugly, make this not use reflection if possible
                            MethodInfo referenceMethod = GetType().GetMethod("SeekInReferenceData")
                                                         .MakeGenericMethod(new Type[] { currentNeededParm.ParameterType });
                            referenceMethod.Invoke(this, new object[] { returnedParms, currentNeededParm }); 
                            break;
                        case CacheReferenceType.Help:
                            SeekInReferenceData<NetMud.Data.ReferenceData.Help>(returnedParms, currentNeededParm);
                            break;
                    }
                }
            }

            return returnedParms;
        }

        public void SeekInCode(IList<object> returnedParms, CommandParameterAttribute currentNeededParm)
        {
            var validTargetTypes = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(currentNeededParm.ParameterType));
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();
            Type parm = null;

            while (parmWords > 0)
            {
                var currentParmString = String.Join(" ", internalCommandString.Take(parmWords));

                var validParms = validTargetTypes.Where(comm => comm.GetCustomAttributes<CommandKeywordAttribute>().Any(att => att.Keyword.Equals(currentParmString)));

                if (validParms.Count() > 1)
                {
                    //TODO: Need to disambiguate help text here
                    AccessErrors.Add("DISAMBIGUATE ME");
                    break;
                }
                else if (validParms.Count() == 1)
                {
                    parm = validParms.First();

                    if (parm != null)
                    {
                        switch (currentNeededParm.Usage)
                        {
                            case CommandUsage.Container:
                                Container = Activator.CreateInstance(parm);
                                returnedParms.Add(Container);
                                break;
                            case CommandUsage.Subject:
                                Subject = Activator.CreateInstance(parm);
                                returnedParms.Add(Subject);
                                break;
                            case CommandUsage.Target:
                                Target = Activator.CreateInstance(parm);
                                returnedParms.Add(Target);
                                break;
                        }
                    }

                    internalCommandString = internalCommandString.Skip(parmWords).ToList();
                    parmWords = internalCommandString.Count();
                    return;
                }

                parmWords--;
            }
        }

        //TODO: Make this work, we need far more stuff to do this (location for one)
        public void SeekInLiveWorld<T>(IList<object> returnedParms, CommandParameterAttribute currentNeededParm, CommandRangeAttribute seekRange) where T : Type
        {
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();
            Type parm = null;

            while (parmWords > 0)
            {
                var currentParmString = String.Join(" ", internalCommandString.Take(parmWords));

                var validObjects = liveWorld.GetAll<T>();

                if (validObjects.Count<T>() > 0)
                {
                    if (validObjects.Count<T>() > 1)
                    {
                        //TODO: Need to disambiguate help text here
                        AccessErrors.Add("DISAMBIGUATE ME");
                        break;
                    }
                    else if (validObjects.Count<T>() == 1)
                    {
                        parm = validObjects.First<T>();

                        if (parm != null)
                        {
                            switch (currentNeededParm.Usage)
                            {
                                case CommandUsage.Container:
                                    Container = Activator.CreateInstance(parm);
                                    returnedParms.Add(Container);
                                    break;
                                case CommandUsage.Subject:
                                    Subject = Activator.CreateInstance(parm);
                                    returnedParms.Add(Subject);
                                    break;
                                case CommandUsage.Target:
                                    Target = Activator.CreateInstance(parm);
                                    returnedParms.Add(Target);
                                    break;
                            }
                        }

                        internalCommandString = internalCommandString.Skip(parmWords).ToList();
                        parmWords = internalCommandString.Count();
                        return;
                    }

                    parmWords--;
                }
            }

        }

        public void SeekInReferenceData<T>(IList<object> returnedParms, CommandParameterAttribute currentNeededParm) where T : IReference
        {
            var referenceContext = new ReferenceAccess();
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                var currentParmString = String.Join(" ", internalCommandString.Take(parmWords));

                var validObject = referenceContext.GetOneReference<T>(currentParmString);

                if (validObject != null && !validObject.Equals(default(T)))
                {
                    switch (currentNeededParm.Usage)
                    {
                        case CommandUsage.Container:
                            Container = validObject;
                            returnedParms.Add(validObject);
                            break;
                        case CommandUsage.Subject:
                             Subject = validObject;
                           returnedParms.Add(validObject);
                            break;
                        case CommandUsage.Target:
                            Target = validObject;
                            returnedParms.Add(validObject);
                            break;
                    }

                    internalCommandString = internalCommandString.Skip(parmWords).ToList();
                    parmWords = internalCommandString.Count();
                    return;
                }

                parmWords--;
            }

        }
    }
}
