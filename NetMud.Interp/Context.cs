using System;
using System.Collections.Generic;
using System.Linq;

using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Base.System;
using System.Reflection;
using NetMud.DataAccess;
using NutMud.Commands.Attributes;

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
        public object Supporting { get; private set; }
        public ILocation Location { get; private set; }
        public IEnumerable<ILocation> Surroundings { get; private set; }

        public IList<string> AccessErrors { get; private set; }

        private IEnumerable<Type> LoadedCommands;

        public Context(string fullCommand, IActor actor)
        {
            commandsAssembly = Assembly.GetAssembly(typeof(CommandParameterAttribute));
            entitiesAssembly = Assembly.GetAssembly(typeof(IEntity));
            liveWorld = new LiveCache();

            OriginalCommandString = fullCommand;
            Actor = actor;

            Location = Actor.CurrentLocation;

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
                var parmList = commandType.GetCustomAttributes<CommandParameterAttribute>();
                ParseParamaters(commandType, parmList);

                //Did we get errors from the parameter parser? if so bail
                if(AccessErrors.Count > 0)
                    return;

                Command = Activator.CreateInstance(commandType) as ICommand;

                if (
                    (parmList.Any(parm => !parm.Optional && parm.Usage == CommandUsage.Subject) && Subject == null)
                    || (parmList.Any(parm => !parm.Optional && parm.Usage == CommandUsage.Target) && Target == null)
                    || (parmList.Any(parm => !parm.Optional && parm.Usage == CommandUsage.Supporting) && Supporting == null)
                    )
                {
                    AccessErrors.Add("Invalid command targets specified.");
                    AccessErrors = AccessErrors.Concat(Command.RenderSyntaxHelp()).ToList();
                    return;
                }

                //Parms we got doesn't equal parms we loaded
                if (CommandStringRemainder.Count() != 0)
                {
                    AccessErrors.Add(String.Format("I could not find {0}.", String.Join(" ", CommandStringRemainder)));
                    AccessErrors = AccessErrors.Concat(Command.RenderSyntaxHelp()).ToList();
                    return;
                }
                

                Command.OriginLocation = Location;
                Command.Surroundings = Surroundings;

                Command.Subject = Subject;
                Command.Target = Target;
                Command.Supporting = Supporting;
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

        private void ParseParamaters(Type commandType, IEnumerable<CommandParameterAttribute> neededParms)
        {
            //Flip through each remaining word and parse them
            foreach (var currentNeededParm in neededParms.OrderBy(parm => parm.Usage))
            {
                foreach (var seekType in currentNeededParm.CacheTypes)
                {
                    switch (seekType)
                    {
                        case CacheReferenceType.Code:
                            SeekInCode(currentNeededParm);
                            break;
                        case CacheReferenceType.Entity:
                            //So damn ugly, make this not use reflection if possible
                            MethodInfo entityMethod = GetType().GetMethod("SeekInLiveWorld")
                                                         .MakeGenericMethod(new Type[] { currentNeededParm.ParameterType });
                            entityMethod.Invoke(this, new object[] { currentNeededParm, commandType.GetCustomAttribute<CommandRangeAttribute>() });
                            break;
                        case CacheReferenceType.Reference:
                            MethodInfo referenceMethod = GetType().GetMethod("SeekInReferenceData")
                                                         .MakeGenericMethod(new Type[] { currentNeededParm.ParameterType });
                            referenceMethod.Invoke(this, new object[] { currentNeededParm });
                            break;
                        case CacheReferenceType.Help:
                            SeekInReferenceData<Data.Reference.Help>(currentNeededParm);
                            break;
                        case CacheReferenceType.Data:
                            MethodInfo dataMethod = GetType().GetMethod("SeekInBackingData")
                                                         .MakeGenericMethod(new Type[] { currentNeededParm.ParameterType });
                            dataMethod.Invoke(this, new object[] { currentNeededParm });
                            break;
                    }
                }
            }
        }

        public void SeekInCode(CommandParameterAttribute currentNeededParm)
        {
            var validTargetTypes = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(currentNeededParm.ParameterType));
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();
            Type parm = null;

            while (parmWords > 0)
            {
                var currentParmString = String.Join(" ", internalCommandString.Take(parmWords)).ToLower();

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

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
                            case CommandUsage.Supporting:
                                Supporting = Activator.CreateInstance(parm);
                                break;
                            case CommandUsage.Subject:
                                Subject = Activator.CreateInstance(parm);
                                break;
                            case CommandUsage.Target:
                                Target = Activator.CreateInstance(parm);
                                break;
                        }
                    }

                    CommandStringRemainder = CommandStringRemainder.Skip(parmWords);
                    return;
                }

                parmWords--;
            }
        }

        //TODO: Make this work, we need far more stuff to do this (location for one)
        public void SeekInLiveWorld<T>(CommandParameterAttribute currentNeededParm, CommandRangeAttribute seekRange)
        {
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                var currentParmString = String.Join(" ", internalCommandString.Take(parmWords)).ToLower();

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                var validObjects = new List<T>();

                switch(seekRange.Type)
                {
                    case CommandRangeType.Self:
                        validObjects.Add((T)Actor);
                        break;
                    case CommandRangeType.Touch:
                        validObjects.AddRange(Actor.CurrentLocation.GetContents<T>().Where(ent => ((IEntity)ent).Keywords.Contains(currentParmString)));
                        break;
                    case CommandRangeType.Local: //requires Range to be working
                        break;
                    case CommandRangeType.Regional: //requires range to be working
                        break;
                    case CommandRangeType.Global:
                        validObjects.AddRange(liveWorld.GetAll<T>());
                        break;
                }

                if (validObjects.Count() > 0)
                {
                    if (validObjects.Count() > 1)
                    {
                        //TODO: Need to disambiguate help text here
                        AccessErrors.Add("DISAMBIGUATE ME");
                        break;
                    }
                    else if (validObjects.Count() == 1)
                    {
                        var parm = validObjects.First();

                        if (parm != null)
                        {
                            switch (currentNeededParm.Usage)
                            {
                                case CommandUsage.Supporting:
                                    Supporting = parm;
                                    break;
                                case CommandUsage.Subject:
                                    Subject = parm;
                                    break;
                                case CommandUsage.Target:
                                    Target = parm;
                                    break;
                            }
                        }

                        CommandStringRemainder = CommandStringRemainder.Skip(parmWords);
                        return;
                    }
                }

                parmWords--;
            }

        }

        public void SeekInReferenceData<T>(CommandParameterAttribute currentNeededParm) where T : IReference
        {
            var referenceContext = new ReferenceAccess();
            
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                var currentParmString = String.Join(" ", internalCommandString.Take(parmWords));

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                var validObject = referenceContext.GetOneReference<T>(currentParmString);

                if (validObject != null && !validObject.Equals(default(T)))
                {
                    switch (currentNeededParm.Usage)
                    {
                        case CommandUsage.Supporting:
                            Supporting = validObject;
                            break;
                        case CommandUsage.Subject:
                            Subject = validObject;
                            break;
                        case CommandUsage.Target:
                            Target = validObject;
                            break;
                    }

                    CommandStringRemainder = CommandStringRemainder.Skip(parmWords);
                    return;
                }

                parmWords--;
            }
        }

        public void SeekInBackingData<T>(CommandParameterAttribute currentNeededParm) where T : IData
        {
            var dataContext = new DataWrapper();
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                var currentParmString = String.Join(" ", internalCommandString.Take(parmWords));

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                var validObject = default(T);

                long parmID = -1;
                if(!long.TryParse(currentParmString, out parmID))
                    validObject = dataContext.GetOneBySharedKey<T>("Name", currentParmString);
                else
                    validObject = dataContext.GetOne<T>(parmID);

                if (validObject != null && !validObject.Equals(default(T)))
                {
                    switch (currentNeededParm.Usage)
                    {
                        case CommandUsage.Supporting:
                            Supporting = validObject;
                            break;
                        case CommandUsage.Subject:
                            Subject = validObject;
                            break;
                        case CommandUsage.Target:
                            Target = validObject;
                            break;
                    }

                    CommandStringRemainder = CommandStringRemainder.Skip(parmWords);
                    return;
                }

                parmWords--;
            }
        }
    }
}
