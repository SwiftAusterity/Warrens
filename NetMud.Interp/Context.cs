using NetMud.Commands.Attributes;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NetMud.Interp
{
    /// <summary>
    /// The parsing system for command input
    /// </summary>
    public class Context : IContext
    {
        /// <summary>
        /// The original input we recieved to parse
        /// </summary>
        public string OriginalCommandString { get; private set; }

        /// <summary>
        /// The system assembly for where commands live
        /// </summary>
        private readonly Assembly commandsAssembly;

        /// <summary>
        /// The system assembly for where live entity classes live
        /// </summary>
        private readonly Assembly entitiesAssembly;

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
            //Dummy check empty strings
            if (string.IsNullOrWhiteSpace(fullCommand))
            {
                AccessErrors.Add("You have to tell me something first."); //TODO: Add generic errors class for rando error messages
                return;
            }

            commandsAssembly = Assembly.GetAssembly(typeof(CommandParameterAttribute));
            entitiesAssembly = Assembly.GetAssembly(typeof(IEntity));

            OriginalCommandString = fullCommand;
            Actor = actor;

            Position = (IGlobalPosition)Actor.CurrentLocation.Clone();

            AccessErrors = new List<string>();

            LoadedCommands = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICommand)));

            //NPCs can't use anything player rank can't use
            StaffRank effectiveRank = StaffRank.Player;

            if (Actor.GetType().GetInterfaces().Contains(typeof(IPlayer)))
            {
                effectiveRank = Actor.Template<IPlayerTemplate>().GamePermissionsRank;
            }

            LoadedCommands = LoadedCommands.Where(comm => comm.GetCustomAttributes<CommandPermissionAttribute>().Any(att => att.MinimumRank <= effectiveRank));

            //find out command's type
            IEnumerable<CommandPackage> potentialCommands = ParseCommand();

            if (potentialCommands == null || potentialCommands.Count() == 0)
            {
                AccessErrors.Add("Unknown Command."); //TODO: Add generic errors class for rando error messages
                return;
            }

            IList<ICommand> validCommands = new List<ICommand>();
            foreach (CommandPackage currentCommand in potentialCommands)
            {
                bool requiresZoneOwnership = currentCommand.CommandType.GetCustomAttributes<CommandPermissionAttribute>().Any(att => att.RequiresTargetOwnership);

                //if (requiresZoneOwnership && !Actor.CurrentLocation.CurrentZone.HasOwnershipRights(Actor))
                //    continue;

                IEnumerable<string> commandRemainder = currentCommand.InputRemainder;

                //Log people using and even attempting to use admin commands in game
                if (currentCommand.CommandType.GetCustomAttributes<CommandPermissionAttribute>().Any(att => att.MinimumRank == StaffRank.Admin))
                {
                    LoggingUtility.LogAdminCommandUsage(OriginalCommandString, Actor.Template<IPlayerTemplate>().AccountHandle);
                }

                try
                {
                    //find the parameters
                    IEnumerable<CommandParameterAttribute> parmList = currentCommand.CommandType.GetCustomAttributes<CommandParameterAttribute>();

                    bool hasContainer = parmList.Any(parm => parm.CacheTypes.Any(crt => crt == CacheReferenceType.Inventory) && !parm.Optional);

                    //why bother if we have no parms to find?
                    if (currentCommand.InputRemainder.Count() > 0)
                    {
                        ParseParamaters(currentCommand, parmList, hasContainer);
                    }

                    //Did we get errors from the parameter parser? if so bail
                    if (AccessErrors.Count > 0)
                    {
                        continue;
                    }

                    ICommand command = Activator.CreateInstance(currentCommand.CommandType) as ICommand;

                    if (
                        (parmList.Any(parm => !parm.Optional && parm.Usage == CommandUsage.Subject) && Subject == null)
                        || (parmList.Any(parm => !parm.Optional && parm.Usage == CommandUsage.Target) && Target == null)
                        || (parmList.Any(parm => !parm.Optional && parm.Usage == CommandUsage.Supporting) && Supporting == null)
                        )
                    {
                        continue;
                    }

                    //Parms we got doesn't equal parms we loaded
                    if (currentCommand.InputRemainder.Count() != 0)
                    {
                        continue;
                    }

                    //double check container stuff
                    if (hasContainer && Subject != null && Subject.GetType().GetInterfaces().Any(typ => typ == typeof(ICollection)))
                    {
                        ICollection<IEntity> collection = (ICollection<IEntity>)Subject;
                        if (collection.Count() == 1)
                        {
                            Subject = collection.First();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    command.CommandWord = currentCommand.CommandPhrase;
                    command.Actor = Actor;
                    command.OriginLocation = Position;

                    command.Subject = Subject;
                    command.Target = Target;
                    command.Supporting = Supporting;

                    validCommands.Add(command);
                }
                catch (MethodAccessException mEx)
                {
                    LoggingUtility.LogError(mEx);
                }
            }

            int validCount = validCommands.Count();
            if (validCount == 0)
            {
                AccessErrors.Add("Unknown Command."); //TODO: Add generic errors class for rando error messages
                return;
            }
            else if (validCount > 1)
            {
                AccessErrors.Add(string.Format("There are {0} potential commands with that name and parameter structure.", validCommands.Count()));

                foreach (ICommand currentCommand in validCommands)
                {
                    AccessErrors.AddRange(currentCommand.RenderSyntaxHelp());
                }

                return;
            }

            //We only have one valid command, that's good
            Command = validCommands.First();
        }

        /// <summary>
        /// Tries to find the command method we're after
        /// </summary>
        /// <returns>The command method's system type</returns>
        private IEnumerable<CommandPackage> ParseCommand()
        {
            //Split out the string
            IEnumerable<string> parsedWords = ParseInitialCommandString();

            int commandWords = parsedWords.Count();
            IList<CommandPackage> commands = new List<CommandPackage>();

            while (commandWords > 0)
            {
                string currentCommandString = string.Join(" ", RemoveGrammaticalNiceities(parsedWords.Take(commandWords))).ToLower();

                IEnumerable<Type> validCommands = LoadedCommands.Where(comm => comm.GetCustomAttributes<CommandKeywordAttribute>()
                                            .Any(att => att.Aliases.Any(alias => alias.Equals(currentCommandString, StringComparison.InvariantCultureIgnoreCase))));

                foreach (Type commandType in validCommands)
                {
                    //Distinct this stuff
                    if (commands.Any(com => com.CommandType == commandType))
                    {
                        continue;
                    }

                    CommandKeywordAttribute keywordAttribute = commandType.GetCustomAttributes<CommandKeywordAttribute>().Single(att => att.Aliases.Any(alias =>
                                                            alias.Equals(currentCommandString, StringComparison.InvariantCultureIgnoreCase)));

                    CommandPackage newCommand = new CommandPackage
                    {
                        CommandType = commandType,
                        CommandPhrase = keywordAttribute.Keyword
                    };

                    //Kinda janky but we need a way to tell the system "north" is both the command and the target
                    if (!keywordAttribute.IsAlsoSubject)
                    {
                        newCommand.InputRemainder = parsedWords.Skip(commandWords);
                    }
                    else
                    {
                        newCommand.InputRemainder = parsedWords;
                    }

                    commands.Add(newCommand);
                }

                commandWords--;
            }

            return commands;
        }

        /// <summary>
        /// Tries to parse the parameters for the command
        /// </summary>
        /// <param name="commandType">the command's method type</param>
        /// <param name="neededParms">what paramaters are considered required by the command</param>
        private void ParseParamaters(CommandPackage command, IEnumerable<CommandParameterAttribute> neededParms, bool hasContainer)
        {
            //Flip through each remaining word and parse them
            foreach (CommandParameterAttribute currentNeededParm in neededParms.OrderBy(parm => parm.Usage))
            {
                //why continue if we ran out of stuff
                if (command.InputRemainder.Count() == 0)
                {
                    break;
                }

                //Short circuit the process if we lack something we need
                if (currentNeededParm.RequiresPreviousParameter)
                {
                    switch (currentNeededParm.Usage)
                    {
                        case CommandUsage.Supporting:
                            if (Target == null)
                            {
                                return;
                            }

                            break;
                        case CommandUsage.Target:
                            if (Subject == null)
                            {
                                return;
                            }

                            break;
                    }
                }

                foreach (Type paramType in currentNeededParm.ParameterTypes)
                {
                    foreach (CacheReferenceType seekType in currentNeededParm.CacheTypes)
                    {
                        switch (seekType)
                        {
                            case CacheReferenceType.Code:
                                SeekInCode(command, currentNeededParm);
                                break;
                            case CacheReferenceType.Entity:
                                //So damn ugly, make this not use reflection if possible
                                MethodInfo entityMethod = GetType().GetMethod("SeekInLiveWorld")
                                                             .MakeGenericMethod(new Type[] { paramType });
                                entityMethod.Invoke(this, new object[] { command, currentNeededParm, command.CommandType.GetCustomAttribute<CommandRangeAttribute>(), hasContainer });
                                break;
                            case CacheReferenceType.Pathway:
                                MethodInfo pathwayMethod = GetType().GetMethod("SeekInPathway")
                                                            .MakeGenericMethod(new Type[] { paramType });
                                pathwayMethod.Invoke(this, new object[] { command, currentNeededParm, command.CommandType.GetCustomAttribute<CommandRangeAttribute>(), hasContainer });
                                break;
                            case CacheReferenceType.Direction:
                                SeekInDirection(command, paramType, currentNeededParm);
                                break;
                            case CacheReferenceType.Interaction:
                                MethodInfo interMethod = GetType().GetMethod("SeekInInteractions")
                                                          .MakeGenericMethod(new Type[] { paramType });
                                interMethod.Invoke(this, new object[] { command, currentNeededParm, command.CommandType.GetCustomAttribute<CommandRangeAttribute>(), hasContainer });
                                break;
                            case CacheReferenceType.Use:
                                MethodInfo useMethod = GetType().GetMethod("SeekInUses")
                                                          .MakeGenericMethod(new Type[] { paramType });
                                useMethod.Invoke(this, new object[] { command, currentNeededParm, command.CommandType.GetCustomAttribute<CommandRangeAttribute>(), hasContainer });
                                break;
                            case CacheReferenceType.TrainerKnowledge:
                                MethodInfo trainerKnowledgeMethod = GetType().GetMethod("SeekInTrainerKnowledge")
                                                             .MakeGenericMethod(new Type[] { paramType });
                                trainerKnowledgeMethod.Invoke(this, new object[] { command, currentNeededParm });
                                break;
                            case CacheReferenceType.MerchantStock:
                                MethodInfo merchantStockMethod = GetType().GetMethod("SeekInMerchantStock")
                                                             .MakeGenericMethod(new Type[] { paramType });
                                merchantStockMethod.Invoke(this, new object[] { command, currentNeededParm });
                                break;
                            case CacheReferenceType.Inventory:
                                MethodInfo inventoryMethod = GetType().GetMethod("SeekInPlayerInventory")
                                                             .MakeGenericMethod(new Type[] { paramType });
                                inventoryMethod.Invoke(this, new object[] { command, currentNeededParm });
                                break;
                            case CacheReferenceType.LookupData:
                                MethodInfo referenceMethod = GetType().GetMethod("SeekInLookupData")
                                                             .MakeGenericMethod(new Type[] { paramType });
                                referenceMethod.Invoke(this, new object[] { command, currentNeededParm });
                                break;
                            case CacheReferenceType.Help:
                                SeekInLookupData<IHelp>(command, currentNeededParm);
                                break;
                            case CacheReferenceType.Data:
                                MethodInfo dataMethod = GetType().GetMethod("SeekInBackingData")
                                                             .MakeGenericMethod(new Type[] { paramType });
                                dataMethod.Invoke(this, new object[] { command, currentNeededParm });
                                break;
                            case CacheReferenceType.String:
                                SeekInString(command, currentNeededParm);
                                break;
                            case CacheReferenceType.Greedy:
                                //Greedy just means grab the remainder of the command string and dump it into a param
                                switch (currentNeededParm.Usage)
                                {
                                    case CommandUsage.Supporting:
                                        Supporting = string.Join(" ", command.InputRemainder);
                                        break;
                                    case CommandUsage.Subject:
                                        Subject = string.Join(" ", command.InputRemainder);
                                        break;
                                    case CommandUsage.Target:
                                        Target = string.Join(" ", command.InputRemainder);
                                        break;
                                }

                                //empty the remainder
                                command.InputRemainder = Enumerable.Empty<string>();
                                //We return here to end the parsing
                                return;
                        }
                    }
                }
            }
        }

        public void SeekInDirection(CommandPackage command, Type paramType, CommandParameterAttribute currentNeededParm)
        {
            //String just means grab the first thing that matches the regex
            IEnumerable<string> internalDirectionCommandString = command.InputRemainder;

            int parmDirWords = internalDirectionCommandString.Count();

            while (parmDirWords > 0)
            {
                string currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalDirectionCommandString.Take(parmDirWords))).ToLower();
                int disambiguator = -1;

                //We have disambiguation here, we need to pick the first object we get back in the list
                if (Regex.IsMatch(currentParmString, LiveWorldDisambiguationSyntax))
                {
                    disambiguator = int.Parse(currentParmString.Substring(0, currentParmString.IndexOf(".")));
                    currentParmString = currentParmString.Substring(currentParmString.IndexOf(".") + 1);
                }

                if (!Enum.TryParse(currentParmString, true, out MovementDirectionType dirType) && !Enum.TryParse(command.CommandPhrase, true, out dirType))
                {
                    parmDirWords--;
                    continue;
                }

                object thing = null;

                if (paramType == typeof(IPathway))
                {
                    IEnumerable<IPathway> validPaths = Actor.CurrentLocation.CurrentLocation().GetPathways().Where(dest => dest.DirectionType == dirType);

                    if (validPaths.Count() > 0)
                    {
                        //Skip everything up to the right guy and then take the one we want so we don't have to horribly alter the following logic flows
                        if (disambiguator > -1 && validPaths.Count() > 1)
                            validPaths = validPaths.Skip(disambiguator - 1).Take(1).ToList();

                        if (validPaths.Count() > 1)
                        {
                            AccessErrors.Add(string.Format("There are {0} potential targets with that name for the {1} command. Try using one of the following disambiguators:", validPaths.Count(), command.CommandPhrase));

                            int iterator = 1;
                            foreach (IPathway obj in validPaths)
                            {
                                IEntity entityObject = obj;

                                AccessErrors.Add(string.Format("{0}.{1}", iterator++, entityObject.TemplateName));
                            }

                            break;
                        }
                        else if (validPaths.Count() == 1)
                        {
                            thing = validPaths.First();
                        }
                    }

                }
                else
                {
                    thing = dirType;
                }

                if(thing == null)
                {
                    parmDirWords--;
                    continue;
                }

                switch (currentNeededParm.Usage)
                {
                    case CommandUsage.Supporting:
                        Supporting = thing;
                        break;
                    case CommandUsage.Subject:
                        Subject = thing;
                        break;
                    case CommandUsage.Target:
                        Target = thing;
                        break;
                }

                command.InputRemainder = command.InputRemainder.Skip(parmDirWords);
                break;
            }
        }

        public void SeekInString(CommandPackage command, CommandParameterAttribute currentNeededParm)
        {
            //String just means grab the first thing that matches the regex
            IEnumerable<string> internalStringCommandString = command.InputRemainder;

            int parmStringWords = internalStringCommandString.Count();

            while (parmStringWords > 0)
            {
                string currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalStringCommandString.Take(parmStringWords))).ToLower();

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmStringWords--;
                    continue;
                }

                switch (currentNeededParm.Usage)
                {
                    case CommandUsage.Supporting:
                        Supporting = string.Join(" ", internalStringCommandString.Take(parmStringWords));
                        break;
                    case CommandUsage.Subject:
                        Subject = string.Join(" ", internalStringCommandString.Take(parmStringWords));
                        break;
                    case CommandUsage.Target:
                        Target = string.Join(" ", internalStringCommandString.Take(parmStringWords));
                        break;
                }

                command.InputRemainder = command.InputRemainder.Skip(parmStringWords);
                break;
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
        public void SeekInPathway<T>(CommandPackage command, CommandParameterAttribute currentNeededParm, CommandRangeAttribute seekRange, bool hasContainer)
        {
            List<string> internalCommandString = command.InputRemainder.ToList();
            int disambiguator = -1;
            int parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                string currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords))).ToLower();

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

                List<IPathway> validPaths = new List<IPathway>();

                validPaths.AddRange(Actor.CurrentLocation.CurrentLocation().GetPathways().Where(dest => dest.TemplateName.Equals(currentParmString, StringComparison.InvariantCultureIgnoreCase)));

                if (validPaths.Count() > 0)
                {
                    //Skip everything up to the right guy and then take the one we want so we don't have to horribly alter the following logic flows
                    if (disambiguator > -1 && validPaths.Count() > 1)
                        validPaths = validPaths.Skip(disambiguator - 1).Take(1).ToList();

                    if (validPaths.Count() > 1)
                    {
                        AccessErrors.Add(string.Format("There are {0} potential targets with that name for the {1} command. Try using one of the following disambiguators:", validPaths.Count(), command.CommandPhrase));

                        int iterator = 1;
                        foreach (IPathway obj in validPaths)
                        {
                            IEntity entityObject = obj;

                            AccessErrors.Add(string.Format("{0}.{1}", iterator++, entityObject.TemplateName));
                        }

                        break;
                    }
                    else if (validPaths.Count() == 1)
                    {
                        IPathway parm = validPaths.First();

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

                        command.InputRemainder = command.InputRemainder.Skip(parmWords);
                        return;
                    }
                }

                parmWords--;
            }

        }

        /// <summary>
        /// Find a paramater's target in code (methods)
        /// </summary>
        /// <param name="commandType">the system type of the command</param>
        /// <param name="currentNeededParm">the paramater we are after</param>
        public void SeekInCode(CommandPackage command, CommandParameterAttribute currentNeededParm)
        {
            IEnumerable<Type> validTargetTypes = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Any(interf => currentNeededParm.ParameterTypes.Contains(interf)));
            List<string> internalCommandString = command.InputRemainder.ToList();

            int parmWords = internalCommandString.Count();
            Type parm = null;

            while (parmWords > 0)
            {
                string currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords))).ToLower();

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                IEnumerable<Type> validParms = validTargetTypes.Where(comm => comm.GetCustomAttributes<CommandKeywordAttribute>()
                                                                .Any(att => att.Keyword.Equals(currentParmString, StringComparison.InvariantCultureIgnoreCase) && att.DisplayInHelpAndCommands)
                                                                || (comm.GetCustomAttribute<CommandSuppressName>() != null) && comm.Name.Equals(currentParmString, StringComparison.InvariantCultureIgnoreCase));


                if (validParms.Count() > 1)
                {
                    AccessErrors.Add(string.Format("There are {0} potential targets with that name for the {1} command.", validParms.Count(), command.CommandPhrase));

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

                    command.InputRemainder = command.InputRemainder.Skip(parmWords);
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
        public void SeekInLiveWorld<T>(CommandPackage command, CommandParameterAttribute currentNeededParm, CommandRangeAttribute seekRange, bool hasContainer)
        {
            List<string> internalCommandString = command.InputRemainder.ToList();
            int disambiguator = -1;
            int parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                string currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords))).ToLower();

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

                List<T> validObjects = new List<T>();

                switch (seekRange.Type)
                {
                    case CommandRangeType.Self:
                        validObjects.Add((T)Actor);
                        break;
                    case CommandRangeType.Touch:
                        validObjects.AddRange(Position.CurrentLocation().GetContents<T>().Where(ent => ((IEntity)ent).Keywords.Any(key => key.Contains(currentParmString))));

                        //Add the pathways
                        if (DataUtility.GetAllImplimentingedTypes(typeof(IPathway)).Contains(typeof(T)) && Position.CurrentLocation().GetType().GetInterfaces().Contains(typeof(ILocation)))
                        {
                            ILocation location = Position.CurrentLocation();
                            IEnumerable<IPathway> validPathways = location.GetPathways();

                            if (validPathways.Any())
                            {
                                validObjects.AddRange(validPathways.Select(path => (T)path));
                            }
                        }

                        if (Actor.GetType().GetInterfaces().Any(typ => typ == typeof(IContains)))
                        {
                            validObjects.AddRange(((IContains)Actor).GetContents<T>().Where(ent => ((IEntity)ent).Keywords != null && ((IEntity)ent).Keywords.Any(key => key.Contains(currentParmString))));
                        }

                        //Containers only matter for touch usage subject paramaters, actor's inventory is already handled
                        //Don't sift through another NonPlayerCharacter's stuff
                        //TODO: write "does entity have permission to another entity's inventories" function on IEntity
                        if (hasContainer && currentNeededParm.Usage == CommandUsage.Subject)
                        {
                            foreach (IContains thing in Position.CurrentLocation().GetContents<T>().Where(ent => ent.GetType().GetInterfaces().Any(intf => intf == typeof(IContains))
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

                if (hasContainer && currentNeededParm.Usage == CommandUsage.Subject && validObjects.Count() > 0)
                {
                    Subject = validObjects;
                    command.InputRemainder = command.InputRemainder.Skip(parmWords);
                    return;
                }
                else if (validObjects.Count() > 0)
                {
                    //Skip everything up to the right guy and then take the one we want so we don't have to horribly alter the following logic flows
                    if (disambiguator > -1 && validObjects.Count() > 1)
                    {
                        validObjects = validObjects.Skip(disambiguator - 1).Take(1).ToList();
                    }

                    if (validObjects.Count() > 1)
                    {
                        AccessErrors.Add(string.Format("There are {0} potential targets with that name for the {1} command. Try using one of the following disambiguators:", validObjects.Count(), command.CommandPhrase));

                        int iterator = 1;
                        foreach (T obj in validObjects)
                        {
                            IEntity entityObject = (IEntity)obj;

                            AccessErrors.Add(string.Format("{0}.{1}", iterator++, entityObject.TemplateName));
                        }

                        break;
                    }
                    else if (validObjects.Count() == 1)
                    {
                        T parm = validObjects.First();

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

                        command.InputRemainder = command.InputRemainder.Skip(parmWords);
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
        public void SeekInLookupData<T>(CommandPackage command, CommandParameterAttribute currentNeededParm) where T : ILookupData
        {
            List<string> internalCommandString = command.InputRemainder.ToList();

            int parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                string currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords)));

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                T validObject = TemplateCache.GetByName<T>(currentParmString);

                if (validObject != null && !validObject.Equals(default(T)) && validObject.State == ApprovalState.Approved)
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

                    command.InputRemainder = command.InputRemainder.Skip(parmWords);
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
        public void SeekInBackingData<T>(CommandPackage command, CommandParameterAttribute currentNeededParm) where T : ITemplate
        {
            List<string> internalCommandString = command.InputRemainder.ToList();

            int parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                string currentParmString = string.Join(" ", internalCommandString.Take(parmWords));

                if (!currentNeededParm.MatchesPattern(currentParmString))
                {
                    parmWords--;
                    continue;
                }

                T validObject;
                long parmID;
                if (!long.TryParse(currentParmString, out parmID))
                {
                    validObject = TemplateCache.GetByKeywords<T>(currentParmString);
                }
                else
                {
                    validObject = TemplateCache.Get<T>(parmID);
                }

                if (validObject != null && !validObject.Equals(default(T)) && validObject.State == ApprovalState.Approved)
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

                    command.InputRemainder = command.InputRemainder.Skip(parmWords);
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
        public void SeekInLiveWorldContainer<T>(CommandPackage command, CommandParameterAttribute currentNeededParm, Type subjectType)
        {
            //Borked it here, we found nothing viable earlier
            if (Subject == null || !((ICollection<IEntity>)Subject).Any())
            {
                return;
            }

            ICollection<IEntity> subjectCollection = (ICollection<IEntity>)Subject;

            //Containers are touch range only
            List<string> internalCommandString = command.InputRemainder.ToList();
            int disambiguator = -1;
            int parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                string currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords))).ToLower();

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

                List<T> validObjects = new List<T>();

                validObjects.AddRange(subjectCollection.Select(sbj => sbj.CurrentLocation.CurrentLocation())
                                                        .Where(cl => cl.Keywords.Any(key => key.Contains(currentParmString))).Select(ent => (T)ent));

                if (validObjects.Count() > 0)
                {
                    //Skip everything up to the right guy and then take the one we want so we don't have to horribly alter the following logic flows
                    if (disambiguator > -1 && validObjects.Count() > 1)
                    {
                        validObjects = validObjects.Skip(disambiguator - 1).Take(1).ToList();
                    }

                    if (validObjects.Count() > 1)
                    {
                        AccessErrors.Add(string.Format("There are {0} potential containers with that name for the {1} command. Try using one of the following disambiguators:", validObjects.Count(), command.CommandPhrase));

                        int iterator = 1;
                        foreach (T obj in validObjects)
                        {
                            IEntity entityObject = (IEntity)obj;

                            AccessErrors.Add(string.Format("{0}.{1}", iterator++, entityObject.TemplateName));
                        }

                        break;
                    }
                    else if (validObjects.Count() == 1)
                    {
                        T parm = validObjects.First();

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

                        command.InputRemainder = command.InputRemainder.Skip(parmWords);

                        //Now try to set the subject
                        IContains container = (IContains)parm;

                        List<IEntity> validSubjects = new List<IEntity>();

                        validSubjects.AddRange(subjectCollection.Where(sbj => sbj.CurrentLocation.CurrentLocation().Equals(container)));

                        if (validSubjects.Count() > 1)
                        {
                            AccessErrors.Add(string.Format("There are {0} potential targets with that name inside {1} for the {2} command. Try using one of the following disambiguators:"
                                , validObjects.Count(), parmWords, command.CommandPhrase));

                            int iterator = 1;
                            foreach (IEntity obj in validSubjects)
                            {
                                AccessErrors.Add(string.Format("{0}.{1}", iterator++, obj.TemplateName));
                            }
                        }
                        else if (validObjects.Count() == 1)
                        {
                            Subject = validSubjects.First();
                        }

                        return;
                    }
                }

                parmWords--;
            }

        }

        /// <summary>
        /// Seeks an item in a merchant's inventory, requires the merchant tile to already be in the subject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="currentNeededParm"></param>
        public void SeekInMerchantStock<T>(CommandPackage command, CommandParameterAttribute currentNeededParm) where T : IEntity
        {
            //Borked it here, we found nothing viable earlier
            if (Actor == null || Subject == null)
            {
                return;
            }

            INonPlayerCharacter merchant = (INonPlayerCharacter)Subject;

            //No merchant? We only use this to find things they sell so we can use the DoISellThings
            if (merchant == null || (!merchant.DoISellThings()))
            {
                return;
            }

            IEnumerable<T> subjectCollection = merchant.GetContents<T>();

            //Containers are touch range only
            List<string> internalCommandString = command.InputRemainder.ToList();
            int disambiguator = -1;
            int parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                string currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords))).ToLower();

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

                List<T> validObjects = new List<T>();

                validObjects.AddRange(subjectCollection.Where(cl => cl.Keywords != null && cl.Keywords.Any(key => key.Equals(currentParmString, StringComparison.InvariantCultureIgnoreCase))).Select(ent => ent));

                if (validObjects.Count() > 0)
                {
                    //Skip everything up to the right guy and then take the one we want so we don't have to horribly alter the following logic flows
                    if (disambiguator > -1 && validObjects.Count() > 1)
                    {
                        validObjects = validObjects.Skip(disambiguator - 1).Take(1).ToList();
                    }

                    if (validObjects.Count() > 1)
                    {
                        AccessErrors.Add(string.Format("There are {0} potential items with that name for the {1} command. Try using one of the following disambiguators:", validObjects.Count(), command.CommandPhrase));

                        int iterator = 1;
                        foreach (T obj in validObjects)
                        {
                            IEntity entityObject = obj;

                            AccessErrors.Add(string.Format("{0}.{1}", iterator++, entityObject.TemplateName));
                        }

                        break;
                    }
                    else if (validObjects.Count() == 1)
                    {
                        T parm = validObjects.First();

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

                        command.InputRemainder = command.InputRemainder.Skip(parmWords);
                        return;
                    }
                }

                parmWords--;
            }

        }

        /// <summary>
        /// Seeks a use or quality in trainer knowledge
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="currentNeededParm"></param>
        public void SeekInTrainerKnowledge<T>(CommandPackage command, CommandParameterAttribute currentNeededParm)
        {
            //Borked it here, we found nothing viable earlier
            if (Actor == null || Subject == null)
            {
                return;
            }

            INonPlayerCharacter trainer = (INonPlayerCharacter)Subject;

            //No trainer? We only use this to find things they sell so we can use the DoITeachThings
            if (trainer == null || (!trainer.DoITeachThings()))
            {
                return;
            }

            //Containers are touch range only
            List<string> internalCommandString = command.InputRemainder.ToList();
            int disambiguator = -1;
            int parmWords = internalCommandString.Count();

            while (parmWords > 0)
            {
                string currentParmString = string.Join(" ", RemoveGrammaticalNiceities(internalCommandString.Take(parmWords))).ToLower();

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

                List<T> validObjects = new List<T>();

                if (typeof(T) == typeof(IQuality))
                {
                    validObjects.AddRange(trainer.TeachableProficencies.Where(use => use.Name.Equals(currentParmString, StringComparison.InvariantCultureIgnoreCase))
                                                                 .Select(use => (T)use));
                }

                if (validObjects.Count() > 0)
                {
                    //Skip everything up to the right guy and then take the one we want so we don't have to horribly alter the following logic flows
                    if (disambiguator > -1 && validObjects.Count() > 1)
                    {
                        validObjects = validObjects.Skip(disambiguator - 1).Take(1).ToList();
                    }

                    if (validObjects.Count() > 1)
                    {
                        AccessErrors.Add(string.Format("There are {0} potential items with that name for the {1} command. Try using one of the following disambiguators:", validObjects.Count(), command.CommandPhrase));

                        int iterator = 1;
                        foreach (T obj in validObjects)
                        {
                            IEntity entityObject = (IEntity)obj;

                            AccessErrors.Add(string.Format("{0}.{1}", iterator++, entityObject.TemplateName));
                        }

                        break;
                    }
                    else if (validObjects.Count() == 1)
                    {
                        T parm = validObjects.First();

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

                        command.InputRemainder = command.InputRemainder.Skip(parmWords);
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
            List<string> parmList = currentParams.ToList();

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
            List<string> originalStrings = new List<string>();
            string baseString = OriginalCommandString;

            int foundStringIterator = 0;
            List<string> foundStrings = new List<string>();

            //Do we have magic string collectors? quotation marks demarcate a single parameter being passed in
            while (baseString.Contains("\""))
            {
                int firstQuoteIndex = baseString.IndexOf('"');
                int secondQuoteIndex = baseString.IndexOf('"', firstQuoteIndex + 1);

                //What? Why would this even happen
                if (firstQuoteIndex < 0)
                {
                    break;
                }

                //Only one means let's just kill the stupid quotemark and move on
                if (secondQuoteIndex < 0)
                {
                    baseString = baseString.Replace("\"", string.Empty);
                    break;
                }

                string foundString = baseString.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);

                foundStrings.Add(foundString);
                baseString = baseString.Replace(string.Format("\"{0}\"", foundString), "%%" + foundStringIterator.ToString() + "%%");
                foundStringIterator++;
            }

            originalStrings.AddRange(baseString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            if (foundStringIterator == 0)
            {
                return originalStrings;
            }

            //Either add the modified one or add the normal one
            int iterator = 0;
            List<string> returnStrings = new List<string>();
            foreach (string returnString in originalStrings)
            {
                if (iterator >= originalStrings.Count)
                {
                    break;
                }

                if (returnString.Contains("%%" + iterator.ToString() + "%%"))
                {
                    returnStrings.Add(foundStrings[iterator]);
                    iterator++;
                }
                else
                {
                    returnStrings.Add(returnString);
                }
            }

            return returnStrings;
        }
    }
}
