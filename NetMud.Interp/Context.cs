using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using NutMud.Commands.Attributes;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Base.Place;

namespace NetMud.Interp
{
    /// <summary>
    /// The parsing system for command input
    /// </summary>
    public class Context
    {
        /// <summary>
        /// The original input we recieved to parse
        /// </summary>
        public string OriginalCommandString { get; private set; }

        /// <summary>
        /// Internal list that keeps track of what words are left to parse from the command
        /// </summary>
        public IEnumerable<string> CommandStringRemainder { get; private set; }

        /// <summary>
        /// The system assembly for where commands live
        /// </summary>
        private Assembly commandsAssembly;

        /// <summary>
        /// The system assembly for where live entity classes live
        /// </summary>
        private Assembly entitiesAssembly;

        /// <summary>
        /// The entity invoking the command
        /// </summary>
        public IActor Actor { get; private set; }

        /// <summary>
        /// The entity the command refers to
        /// </summary>
        public object Subject { get; private set; }

        /// <summary>
        /// When there is a predicate parameter, the entity that is being targetting (subject become "with")
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// Any tertiary entity being referenced in command parameters
        /// </summary>
        public object Supporting { get; private set; }

        /// <summary>
        /// Container the Actor is in when the command is invoked
        /// </summary>
        public IGlobalPosition Position { get; private set; }

        /// <summary>
        /// Valid containers by range from OriginLocation
        /// </summary>
        public IEnumerable<ILocation> Surroundings { get; private set; }

        /// <summary>
        /// The command (method) we found after parsing
        /// </summary>
        public ICommand Command { get; private set; }

        /// <summary>
        /// Rolling list of errors encountered during parsing
        /// </summary>
        public List<string> AccessErrors { get; private set; }

        /// <summary>
        /// All commands in the system
        /// </summary>
        private IEnumerable<Type> LoadedCommands;

        /// <summary>
        /// regex for finding disambiguated entities (stuff that has the same keywords)
        /// </summary>
        private const string LiveWorldDisambiguationSyntax = "[0-9]{1,9}[.][a-zA-z0-9]+";

        /// <summary>
        /// Where we do the parsing, creates the context and parsed everything on creation
        /// </summary>
        /// <param name="fullCommand">the initial unparsed input string</param>
        /// <param name="actor">the entity issuing the command</param>
        public Context(string fullCommand, IActor actor)
        {
            commandsAssembly = Assembly.GetAssembly(typeof(CommandParameterAttribute));
            entitiesAssembly = Assembly.GetAssembly(typeof(IEntity));

            OriginalCommandString = fullCommand;
            Actor = actor;

            Position = Actor.CurrentLocation;

            AccessErrors = new List<string>();
            CommandStringRemainder = Enumerable.Empty<string>();

            LoadedCommands = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICommand)));

            //NPCs can't use anything player rank can't use
            var effectiveRank = StaffRank.Player;

            if (Actor.GetType().GetInterfaces().Contains(typeof(IPlayer)))
                effectiveRank = Actor.DataTemplate<ICharacter>().GamePermissionsRank;

            LoadedCommands = LoadedCommands.Where(comm => comm.GetCustomAttributes<CommandPermissionAttribute>().Any(att => att.MinimumRank <= effectiveRank));

            //find out command's type
            var commandType = ParseCommand();

            if (commandType == null)
            {
                AccessErrors.Add("Unknown Command."); //TODO: Add generic errors class for rando error messages
                return;
            }

            //Log people using and even attempting to use admin commands in game
            if (commandType.GetCustomAttributes<CommandPermissionAttribute>().Any(att => att.MinimumRank == StaffRank.Admin))
                LoggingUtility.LogAdminCommandUsage(OriginalCommandString, Actor.DataTemplate<ICharacter>().AccountHandle);

            try
            {
                //find the parameters
                var parmList = commandType.GetCustomAttributes<CommandParameterAttribute>();

                var hasContainer = parmList.Any(parm => parm.CacheTypes.Any(crt => crt == CacheReferenceType.Container));
                //why bother if we have no parms to find?
                if (CommandStringRemainder.Count() > 0)
                    ParseParamaters(commandType, parmList, hasContainer);

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
                    AccessErrors.AddRange(Command.RenderSyntaxHelp());
                    return;
                }

                //Parms we got doesn't equal parms we loaded
                if (CommandStringRemainder.Count() != 0)
                {
                    AccessErrors.Add(string.Format("I could not find {0}.", string.Join(" ", CommandStringRemainder)));
                    AccessErrors.AddRange(Command.RenderSyntaxHelp());
                    return;
                }

                //double check container stuff
                if(hasContainer && Subject != null && Subject.GetType().GetInterfaces().Any(typ => typ == typeof(ICollection)))
                {
                    var collection = (ICollection<IEntity>)Subject;
                    if (collection.Count() == 1)
                        Subject = collection.First();
                    else
                    {
                        AccessErrors.Add("Invalid command targets specified.");
                        AccessErrors.AddRange(Command.RenderSyntaxHelp());
                        return;
                    }
                }

                Command.Actor = Actor;
                Command.OriginLocation = Position;
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

        /// <summary>
        /// Tries to find the command method we're after
        /// </summary>
        /// <returns>The command method's system type</returns>
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
            var parsedWords = ParseInitialCommandString();

            var commandWords = parsedWords.Count();
            Type command = null;

            while (commandWords > 0)
            {
                var currentCommandString = string.Join(" ", RemoveGrammaticalNiceities(parsedWords.Take(commandWords))).ToLower();

                var validCommands = LoadedCommands.Where(comm => comm.GetCustomAttributes<CommandKeywordAttribute>()
                                                                .Any(att => att.Keyword.Equals(currentCommandString, StringComparison.InvariantCultureIgnoreCase)));

                if (validCommands.Count() > 1)
                {
                    AccessErrors.Add(string.Format("There are {0} potential commands with that name and parameter structure.", validCommands.Count()));

                    foreach(var cmd in validCommands)
                    {
                        var currentCommand = Activator.CreateInstance(cmd) as ICommand;

                        AccessErrors.AddRange(currentCommand.RenderSyntaxHelp());
                        AccessErrors.Add(string.Empty);
                    }

                    break;
                }
                else if (validCommands.Count() == 1)
                {
                    command = validCommands.First();

                    //Kinda janky but we need a way to tell the system "north" is both the command and the target
                    if (!command.GetCustomAttributes<CommandKeywordAttribute>().Single(att => att.Keyword.Equals(currentCommandString, StringComparison.InvariantCultureIgnoreCase)).IsAlsoSubject)
                        CommandStringRemainder = parsedWords.Skip(commandWords);
                    else
                        CommandStringRemainder = parsedWords;

                    break;
                }

                commandWords--;
            }

            return command;
        }

        /// <summary>
        /// Tries to parse the parameters for the command
        /// </summary>
        /// <param name="commandType">the command's method type</param>
        /// <param name="neededParms">what paramaters are considered required by the command</param>
        private void ParseParamaters(Type commandType, IEnumerable<CommandParameterAttribute> neededParms, bool hasContainer)
        {
            //HUGE conceit for CacheReferenceType.Container usage
            // : We need to store all the found CommandUsage.Subjects and validate them once we get to the container
            Type subjectType = typeof(IEntity);

            //Flip through each remaining word and parse them
            foreach (var currentNeededParm in neededParms.OrderBy(parm => parm.Usage))
            {
                //why continue if we ran out of stuff
                if (CommandStringRemainder.Count() == 0)
                    break;

                if (hasContainer && currentNeededParm.Usage == CommandUsage.Subject)
                    subjectType = currentNeededParm.ParameterType;

                foreach (var seekType in currentNeededParm.CacheTypes)
                {
                    switch (seekType)
                    {
                        case CacheReferenceType.Code:
                            SeekInCode(commandType, currentNeededParm);
                            break;
                        case CacheReferenceType.Entity:
                            //So damn ugly, make this not use reflection if possible
                            MethodInfo entityMethod = GetType().GetMethod("SeekInLiveWorld")
                                                         .MakeGenericMethod(new Type[] { currentNeededParm.ParameterType });
                            entityMethod.Invoke(this, new object[] { commandType, currentNeededParm, commandType.GetCustomAttribute<CommandRangeAttribute>(), hasContainer });
                            break;
                        case CacheReferenceType.Container:
                            MethodInfo containerMethod = GetType().GetMethod("SeekInLiveWorldContainer")
                                                         .MakeGenericMethod(new Type[] { currentNeededParm.ParameterType });
                            containerMethod.Invoke(this, new object[] { commandType, currentNeededParm, subjectType });
                            break;
                        case CacheReferenceType.LookupData:
                            MethodInfo referenceMethod = GetType().GetMethod("SeekInLookupData")
                                                         .MakeGenericMethod(new Type[] { currentNeededParm.ParameterType });
                            referenceMethod.Invoke(this, new object[] { commandType, currentNeededParm });
                            break;
                        case CacheReferenceType.Help:
                            SeekInLookupData<Help>(commandType, currentNeededParm);
                            break;
                        case CacheReferenceType.Data:
                            MethodInfo dataMethod = GetType().GetMethod("SeekInBackingData")
                                                         .MakeGenericMethod(new Type[] { currentNeededParm.ParameterType });
                            dataMethod.Invoke(this, new object[] { commandType, currentNeededParm });
                            break;
                        case CacheReferenceType.Text:
                            //Text just means grab the remainder of the command string and dump it into a param
                            switch (currentNeededParm.Usage)
                            {
                                case CommandUsage.Supporting:
                                    Supporting = string.Join(" ", CommandStringRemainder);
                                    break;
                                case CommandUsage.Subject:
                                    Subject = string.Join(" ", CommandStringRemainder);
                                    break;
                                case CommandUsage.Target:
                                    Target = string.Join(" ", CommandStringRemainder);
                                    break;
                            }

                            //empty the remainder
                            CommandStringRemainder = Enumerable.Empty<string>();
                            //We return here to end the parsing
                            return;
                    }
                }
            }
        }

        /// <summary>
        /// Find a paramater's target in code (methods)
        /// </summary>
        /// <param name="commandType">the system type of the command</param>
        /// <param name="currentNeededParm">the paramater we are after</param>
        public void SeekInCode(Type commandType, CommandParameterAttribute currentNeededParm)
        {
            var validTargetTypes = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(currentNeededParm.ParameterType));
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();
            Type parm = null;

            while (parmWords > 0)
            {
                var currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords))).ToLower();

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                var validParms = validTargetTypes.Where(comm => comm.GetCustomAttributes<CommandKeywordAttribute>()
                                                                .Any(att => att.Keyword.Equals(currentParmString, StringComparison.InvariantCultureIgnoreCase) && att.DisplayInHelpAndCommands)
                                                                || (comm.GetCustomAttribute<CommandSuppressName>() != null) && comm.Name.Equals(currentParmString, StringComparison.InvariantCultureIgnoreCase));


                if (validParms.Count() > 1)
                {
                    AccessErrors.Add(string.Format("There are {0} potential targets with that name for the {1} command.", validParms.Count(), commandType.Name));

                    AccessErrors.AddRange(validParms.Select(typ => typ.Name));

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

        /// <summary>
        /// Find a parameter target in the live world (entity)
        /// </summary>
        /// <typeparam name="T">the system type of the entity</typeparam>
        /// <param name="commandType">the system type of the command</param>
        /// <param name="currentNeededParm">the conditions for the parameter we're after</param>
        /// <param name="hasContainer">does the command need a container to look for things in</param>
        /// <param name="seekRange">how far we can look</param>
        public void SeekInLiveWorld<T>(Type commandType, CommandParameterAttribute currentNeededParm, CommandRangeAttribute seekRange, bool hasContainer)
        {
            var internalCommandString = CommandStringRemainder.ToList();
            var disambiguator = -1;
            var parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                var currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords))).ToLower();

                //We have disambiguation here, we need to pick the first object we get back in the list
                if (Regex.IsMatch(currentParmString, LiveWorldDisambiguationSyntax))
                {
                    disambiguator = int.Parse(currentParmString.Substring(0, currentParmString.IndexOf(".")));
                    currentParmString = currentParmString.Substring(currentParmString.IndexOf(".") + 1);
                }

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                var validObjects = new List<T>();

                switch (seekRange.Type)
                {
                    case CommandRangeType.Self:
                        validObjects.Add((T)Actor);
                        break;
                    case CommandRangeType.Touch:
                        validObjects.AddRange(Position.CurrentLocation.GetContents<T>().Where(ent => ((IEntity)ent).Keywords.Any(key => key.Contains(currentParmString))));

                        //Add the pathways
                        if(Utility.DataUtility.GetAllImplimentingedTypes(typeof(IPathway)).Contains(typeof(T)) && Position.CurrentLocation.GetType().GetInterfaces().Contains(typeof(ILocation)))
                        {
                            var location = (ILocation)Position.CurrentLocation;
                            var validPathways = location.GetPathways();

                            if (validPathways.Any())
                            {
                                validObjects.AddRange(validPathways.Select(path => (T)path));
                            }
                        }

                        if (Actor.GetType().GetInterfaces().Any(typ => typ == typeof(IContains)))
                        {
                            validObjects.AddRange(((IContains)Actor).GetContents<T>().Where(ent => ((IEntity)ent).Keywords.Any(key => key.Contains(currentParmString))));
                        }

                        //Containers only matter for touch usage subject paramaters, actor's inventory is already handled
                        //Don't sift through another intelligence's stuff
                        //TODO: write "does entity have permission to another entity's inventories" function on IEntity
                        if (hasContainer && currentNeededParm.Usage == CommandUsage.Subject)
                        {
                            foreach (IContains thing in Position.CurrentLocation.GetContents<T>().Where(ent => ent.GetType().GetInterfaces().Any(intf => intf == typeof(IContains))
                                                                                                 && !ent.GetType().GetInterfaces().Any(intf => intf == typeof(IMobile))
                                                                                                 && !ent.Equals(Actor)))
                            {
                                validObjects.AddRange(thing.GetContents<T>().Where(ent => ((IEntity)ent).Keywords.Any(key => key.Contains(currentParmString))));
                            }
                        }
                        break;
                    case CommandRangeType.Local: //requires Range to be working
                        break;
                    case CommandRangeType.Regional: //requires range to be working
                        break;
                    case CommandRangeType.Global:
                        validObjects.AddRange(LiveCache.GetAll<T>().Where(ent => ((IEntity)ent).Keywords.Any(key => key.Contains(currentParmString))));
                        break;
                }

                if(hasContainer && currentNeededParm.Usage == CommandUsage.Subject && validObjects.Count() > 0)
                {
                    Subject = validObjects;
                    CommandStringRemainder = CommandStringRemainder.Skip(parmWords);
                    return;
                }
                else if (validObjects.Count() > 0)
                {
                    //Skip everything up to the right guy and then take the one we want so we don't have to horribly alter the following logic flows
                    if (disambiguator > -1 && validObjects.Count() > 1)
                        validObjects = validObjects.Skip(disambiguator - 1).Take(1).ToList();

                    if (validObjects.Count() > 1)
                    {
                        AccessErrors.Add(string.Format("There are {0} potential targets with that name for the {1} command. Try using one of the following disambiguators:", validObjects.Count(), commandType.Name));

                        int iterator = 1;
                        foreach(var obj in validObjects)
                        {
                            var entityObject = (IEntity)obj;

                            AccessErrors.Add(string.Format("{0}.{1}", iterator++, entityObject.DataTemplateName));
                        }

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


        /// <summary>
        /// Find a parameter target in Lookup Data
        /// </summary>
        /// <typeparam name="T">the system type of the data</typeparam>
        /// <param name="commandType">the system type of the command</param>
        /// <param name="currentNeededParm">the conditions for the parameter we're after</param>
        public void SeekInLookupData<T>(Type commandType, CommandParameterAttribute currentNeededParm) where T : ILookupData
        {
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                var currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords)));

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                var validObject = BackingDataCache.GetByName<T>(currentParmString);

                if (validObject != null && !validObject.Equals(default(T)) && validObject.Approved)
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

        /// <summary>
        /// Find a parameter target in backing data
        /// </summary>
        /// <typeparam name="T">the system type of the data</typeparam>
        /// <param name="commandType">the system type of the command</param>
        /// <param name="currentNeededParm">the conditions for the parameter we're after</param>
        public void SeekInBackingData<T>(Type commandType, CommandParameterAttribute currentNeededParm) where T : IEntityBackingData
        {
            var internalCommandString = CommandStringRemainder.ToList();

            var parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                var currentParmString = string.Join(" ", internalCommandString.Take(parmWords));

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                var validObject = default(T);

                long parmID = -1;
                if(!long.TryParse(currentParmString, out parmID))
                    validObject = BackingDataCache.GetByKeywords<T>(currentParmString);
                else
                    validObject = BackingDataCache.Get<T>(parmID);

                if (validObject != null && !validObject.Equals(default(T)) && validObject.Approved)
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

        /// <summary>
        /// Find a parameter target in the live world (entity)
        /// </summary>
        /// <typeparam name="T">the system type of the entity</typeparam>
        /// <param name="commandType">the system type of the command</param>
        /// <param name="currentNeededParm">the conditions for the parameter we're after</param>
        public void SeekInLiveWorldContainer<T>(Type commandType, CommandParameterAttribute currentNeededParm, Type subjectType)
        {
            //Borked it here, we found nothing viable earlier
            if (Subject == null || !((ICollection<IEntity>)Subject).Any())
                return;

            var subjectCollection = (ICollection<IEntity>)Subject;

            //Containers are touch range only
            var internalCommandString = CommandStringRemainder.ToList();
            var disambiguator = -1;
            var parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                var currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords))).ToLower();

                //We have disambiguation here, we need to pick the first object we get back in the list
                if (Regex.IsMatch(currentParmString, LiveWorldDisambiguationSyntax))
                {
                    disambiguator = int.Parse(currentParmString.Substring(0, currentParmString.IndexOf(".")));
                    currentParmString = currentParmString.Substring(currentParmString.IndexOf(".") + 1);
                }

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                var validObjects = new List<T>();

                validObjects.AddRange(subjectCollection.Select(sbj => sbj.CurrentLocation.CurrentLocation)
                                                        .Where(cl => cl.Keywords.Any(key => key.Contains(currentParmString))).Select(ent => (T)ent));

                if (validObjects.Count() > 0)
                {
                    //Skip everything up to the right guy and then take the one we want so we don't have to horribly alter the following logic flows
                    if (disambiguator > -1 && validObjects.Count() > 1)
                        validObjects = validObjects.Skip(disambiguator - 1).Take(1).ToList();

                    if (validObjects.Count() > 1)
                    {
                        AccessErrors.Add(string.Format("There are {0} potential containers with that name for the {1} command. Try using one of the following disambiguators:", validObjects.Count(), commandType.Name));

                        int iterator = 1;
                        foreach (var obj in validObjects)
                        {
                            var entityObject = (IEntity)obj;

                            AccessErrors.Add(string.Format("{0}.{1}", iterator++, entityObject.DataTemplateName));
                        }

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

                        //Now try to set the subject
                        var container = (IContains)parm;

                        var validSubjects = new List<IEntity>();

                        validSubjects.AddRange(subjectCollection.Where(sbj => sbj.CurrentLocation.CurrentLocation.Equals(container)));

                        if (validSubjects.Count() > 1)
                        {
                            AccessErrors.Add(string.Format("There are {0} potential targets with that name inside {1} for the {2} command. Try using one of the following disambiguators:"
                                , validObjects.Count(), parmWords, commandType.Name));

                            int iterator = 1;
                            foreach (var obj in validSubjects)
                                AccessErrors.Add(string.Format("{0}.{1}", iterator++, obj.DataTemplateName));
                        }
                        else if (validObjects.Count() == 1)
                            Subject = validSubjects.First();

                        return;
                    }
                }

                parmWords--;
            }

        }


        /// <summary>
        /// Massages the original command string
        /// </summary>
        /// <returns>the right parameters</returns>
        private IEnumerable<string> ParseInitialCommandString()
        {
            List<string> returnParams = ParseQuotesOut();

            //TODOs: Pluralizations, "all.", targetting more than one thing at once
            //singularities - a, an
            //plurals - all, ends in S, ends in ES, number pattern (get 2 swords)

            return returnParams;
        }

        /// <summary>
        /// Removes stuff we don't care about like to, into, the, etc
        /// </summary>
        /// <param name="currentParams">The current set of params</param>
        /// <returns>the scrubbed params</returns>
        private IList<string> RemoveGrammaticalNiceities(IEnumerable<string> currentParams)
        {
            var parmList = currentParams.ToList();

            parmList.RemoveAll(str => str.Equals("the", StringComparison.InvariantCulture)
                                        || str.Equals("of", StringComparison.InvariantCulture)
                                        || str.Equals("to", StringComparison.InvariantCulture)
                                        || str.Equals("into", StringComparison.InvariantCulture)
                                        || str.Equals("in", StringComparison.InvariantCulture)
                                        || str.Equals("from", StringComparison.InvariantCulture)
                                        || str.Equals("inside", StringComparison.InvariantCulture)
                                        || str.Equals("at", StringComparison.InvariantCulture)
                                        || str.Equals("a", StringComparison.InvariantCulture)
                                        || str.Equals("an", StringComparison.InvariantCulture)
                                  );

            return parmList;
        }

        /// <summary>
        /// Scrubs "s out and figures out what the parameters really are
        /// </summary>
        /// <returns>the right parameters</returns>
        private List<string> ParseQuotesOut()
        { 
            var originalStrings = new List<string>();
            var baseString = OriginalCommandString;

            int foundStringIterator = 0;
            var foundStrings = new List<string>();

            //Do we have magic string collectors? quotation marks demarcate a single parameter being passed in
            while(baseString.Contains("\""))
            {
                var firstQuoteIndex = baseString.IndexOf('"');
                var secondQuoteIndex = baseString.IndexOf('"', firstQuoteIndex + 1);

                //What? Why would this even happen
                if (firstQuoteIndex < 0)
                    break;

                //Only one means let's just kill the stupid quotemark and move on
                if (secondQuoteIndex < 0)
                {
                    baseString = baseString.Replace("\"", string.Empty);
                    break;
                }

                var foundString = baseString.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);

                foundStrings.Add(foundString);
                baseString = baseString.Replace(string.Format("\"{0}\"", foundString), "%%" + foundStringIterator.ToString() + "%%");
                foundStringIterator++;
            }

            originalStrings.AddRange(baseString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            //Either add the modified one or add the normal one
            var iterator = 0;
            var returnStrings = new List<string>();
            foreach(var returnString in originalStrings)
            {
                if (returnString.Equals("%%" + iterator.ToString() + "%%"))
                {
                    returnStrings.Add(foundStrings[iterator]);
                    iterator++;
                }
                else
                    returnStrings.Add(returnString);
            }

            return returnStrings;
        }
    }
}
