using NetMud.Authentication;
using NetMud.Data.Action;
using NetMud.DataAccess;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Tile;
using System;
using System.Linq;
using System.Security.Principal;

namespace NetMud.Models
{
    public static class ActionUtility
    {
        #region Interaction
        public static string AddInteraction(ITileTemplate origin, IInteraction dataObject, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || dataObject == null)
            {
                return "Invalid source";
            }

            if (origin.Interactions.Any(path => path.Name == dataObject.Name))
            {
                return "Interaction with that name already exists.";
            }
            else
            {
                var newAction = new Interaction()
                {
                    Name = dataObject.Name,
                    ToActorMessage = dataObject.ToActorMessage,
                    ToLocalMessage = dataObject.ToLocalMessage,
                    FoleyUri = dataObject.FoleyUri,
                    StaminaCost = dataObject.StaminaCost,
                    HealthCost = dataObject.HealthCost,
                    Criteria = dataObject.Criteria,
                    Results = dataObject.Results,
                };

                origin.Interactions.Add(newAction);

                if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                {
                    return "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddInteraction[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return "Interaction Added.";
        }

        public static string EditInteraction(ITileTemplate origin, IInteraction oldAction, IInteraction newAction, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || newAction == null || oldAction == null)
            {
                return "Invalid source";
            }

            var action = new Interaction()
            {
                Name = newAction.Name,
                ToActorMessage = newAction.ToActorMessage,
                ToLocalMessage = newAction.ToLocalMessage,
                FoleyUri = newAction.FoleyUri,
                StaminaCost = newAction.StaminaCost,
                HealthCost = newAction.HealthCost,
                Criteria = newAction.Criteria,
                Results = newAction.Results,
            };

            origin.Interactions.Remove(oldAction);

            origin.Interactions.Add(action);

            if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
            {
                return "Error; Edit failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditInteraction[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return "Interaction modified.";
        }

        public static string RemoveInteraction(ITileTemplate origin, string interactionName, ApplicationUser authedUser, IPrincipal user)
        {
            if (!string.IsNullOrWhiteSpace(interactionName))
            {
                if (origin == null)
                {
                    return "That origin data does not exist";
                }
                else
                {
                    var existingInteraction = origin.Interactions.FirstOrDefault(path => path.Name.Equals(interactionName, StringComparison.InvariantCultureIgnoreCase));

                    if (existingInteraction != null)
                    {
                        origin.Interactions.Remove(existingInteraction);

                        if (origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                        {
                            LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveInteraction[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                            return "Delete Successful.";
                        }
                        else
                        {
                            return "Error; Removal failed.";
                        }
                    }
                    else
                    {
                        return "That does not exist";
                    }
                }
            }

            return "Invalid interaction.";
        }

        
        public static string AddInteraction(ITemplate origin, IInteraction dataObject, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || dataObject == null)
            {
                return "Invalid source";
            }

            if (origin.Interactions.Any(path => path.Name == dataObject.Name))
            {
                return "Interaction with that name already exists.";
            }
            else
            {
                var newAction = new Interaction()
                {
                    Name = dataObject.Name,
                    ToActorMessage = dataObject.ToActorMessage,
                    ToLocalMessage = dataObject.ToLocalMessage,
                    FoleyUri = dataObject.FoleyUri,
                    StaminaCost = dataObject.StaminaCost,
                    HealthCost = dataObject.HealthCost,
                    Criteria = dataObject.Criteria,
                    Results = dataObject.Results,
                };

                origin.Interactions.Add(newAction);

                if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                {
                    return "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddInteraction[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return "Interaction Added.";
        }

        public static string EditInteraction(ITemplate origin, IInteraction oldAction, IInteraction newAction, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || newAction == null || oldAction == null)
            {
                return "Invalid source";
            }

            var action = new Interaction()
            {
                Name = newAction.Name,
                ToActorMessage = newAction.ToActorMessage,
                ToLocalMessage = newAction.ToLocalMessage,
                FoleyUri = newAction.FoleyUri,
                StaminaCost = newAction.StaminaCost,
                HealthCost = newAction.HealthCost,
                Criteria = newAction.Criteria,
                Results = newAction.Results,
            };

            origin.Interactions.Remove(oldAction);

            origin.Interactions.Add(action);

            if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
            {
                return "Error; Edit failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditInteraction[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return "Interaction modified.";
        }

        public static string RemoveInteraction(ITemplate origin, string interactionName, ApplicationUser authedUser, IPrincipal user)
        {
            if (!string.IsNullOrWhiteSpace(interactionName))
            {
                if (origin == null)
                {
                    return "That origin data does not exist";
                }
                else
                {
                    var existingInteraction = origin.Interactions.FirstOrDefault(path => path.Name.Equals(interactionName, StringComparison.InvariantCultureIgnoreCase));

                    if (existingInteraction != null)
                    {
                        origin.Interactions.Remove(existingInteraction);

                        if (origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                        {
                            LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveInteraction[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                            return "Delete Successful.";
                        }
                        else
                        {
                            return "Error; Removal failed.";
                        }
                    }
                    else
                    {
                        return "That does not exist";
                    }
                }
            }

            return "Invalid interaction.";
        }
        #endregion

        #region Decay Timer
        public static string AddDecayEvent(ITileTemplate origin, IDecayEvent dataObject, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null)
            {
                return "Invalid source";
            }

            if (origin.DecayEvents.Any(path => path.Name == dataObject.Name))
            {
                return "Interaction with that name already exists.";
            }
            else
            {
                var action = new DecayEvent()
                {
                    Name = dataObject.Name,
                    ToActorMessage = dataObject.ToActorMessage,
                    ToLocalMessage = dataObject.ToLocalMessage,
                    FoleyUri = dataObject.FoleyUri,
                    StaminaCost = dataObject.StaminaCost,
                    HealthCost = dataObject.HealthCost,
                    Criteria = dataObject.Criteria,
                    Results = dataObject.Results,
                    Timer = dataObject.Timer
                };

                origin.DecayEvents.Add(action);

                if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                {
                    return "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddDecayEvent[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return "Interaction Added.";
        }

        public static string EditDecayEvent(ITileTemplate origin, IDecayEvent oldAction, IDecayEvent newAction, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || newAction == null || oldAction == null)
            {
                return "Invalid source";
            }

            var action = new DecayEvent()
            {
                Name = newAction.Name,
                ToActorMessage = newAction.ToActorMessage,
                ToLocalMessage = newAction.ToLocalMessage,
                FoleyUri = newAction.FoleyUri,
                StaminaCost = newAction.StaminaCost,
                HealthCost = newAction.HealthCost,
                Criteria = newAction.Criteria,
                Results = newAction.Results,
                Timer = newAction.Timer
            };

            origin.DecayEvents.Remove(oldAction);

            origin.DecayEvents.Add(action);

            if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
            {
                return "Error; Edit failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditDecayEvent[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return "Interaction modified.";
        }

        public static string RemoveDecayEvent(ITileTemplate origin, string interactionName, ApplicationUser authedUser, IPrincipal user)
        {
            if (!string.IsNullOrWhiteSpace(interactionName))
            {
                if (origin == null)
                {
                    return "That origin data does not exist";
                }
                else
                {
                    var existingInteraction = origin.DecayEvents.FirstOrDefault(path => path.Name.Equals(interactionName, StringComparison.InvariantCultureIgnoreCase));

                    if (existingInteraction != null)
                    {
                        origin.DecayEvents.Remove(existingInteraction);

                        if (origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                        {
                            LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveInteraction[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                            return "Delete Successful.";
                        }
                        else
                        {
                            return "Error; Removal failed.";
                        }
                    }
                    else
                    {
                        return "That does not exist";
                    }
                }
            }

            return "Invalid interaction.";
        }

        public static string AddDecayEvent(ITemplate origin, IDecayEvent dataObject, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null)
            {
                return "Invalid source";
            }

            if (origin.DecayEvents.Any(path => path.Name == dataObject.Name))
            {
                return "Interaction with that name already exists.";
            }
            else
            {
                var action = new DecayEvent()
                {
                    Name = dataObject.Name,
                    ToActorMessage = dataObject.ToActorMessage,
                    ToLocalMessage = dataObject.ToLocalMessage,
                    FoleyUri = dataObject.FoleyUri,
                    StaminaCost = dataObject.StaminaCost,
                    HealthCost = dataObject.HealthCost,
                    Criteria = dataObject.Criteria,
                    Results = dataObject.Results,
                    Timer = dataObject.Timer
                };

                origin.DecayEvents.Add(action);

                if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                {
                    return "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddDecayEvent[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return "Interaction Added.";
        }

        public static string EditDecayEvent(ITemplate origin, IDecayEvent oldAction, IDecayEvent newAction, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || newAction == null || oldAction == null)
            {
                return "Invalid source";
            }

            var action = new DecayEvent()
            {
                Name = newAction.Name,
                ToActorMessage = newAction.ToActorMessage,
                ToLocalMessage = newAction.ToLocalMessage,
                FoleyUri = newAction.FoleyUri,
                StaminaCost = newAction.StaminaCost,
                HealthCost = newAction.HealthCost,
                Criteria = newAction.Criteria,
                Results = newAction.Results,
                Timer = newAction.Timer
            };

            origin.DecayEvents.Remove(oldAction);

            origin.DecayEvents.Add(action);

            if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
            {
                return "Error; Edit failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditDecayEvent[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return "Interaction modified.";
        }

        public static string RemoveDecayEvent(ITemplate origin, string interactionName, ApplicationUser authedUser, IPrincipal user)
        {
            if (!string.IsNullOrWhiteSpace(interactionName))
            {
                if (origin == null)
                {
                    return "That origin data does not exist";
                }
                else
                {
                    var existingInteraction = origin.DecayEvents.FirstOrDefault(path => path.Name.Equals(interactionName, StringComparison.InvariantCultureIgnoreCase));

                    if (existingInteraction != null)
                    {
                        origin.DecayEvents.Remove(existingInteraction);

                        if (origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                        {
                            LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveDecayEvent[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                            return "Delete Successful.";
                        }
                        else
                        {
                            return "Error; Removal failed.";
                        }
                    }
                    else
                    {
                        return "That does not exist";
                    }
                }
            }

            return "Invalid interaction.";
        }
        #endregion

        #region Use
        public static string AddUse(IInanimateTemplate origin, IUse dataObject, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || dataObject == null)
            {
                return "Invalid source";
            }

            if (origin.Uses.Any(path => path.Name == dataObject.Name))
            {
                return "Interaction with that name already exists.";
            }
            else
            {
                var action = new Use()
                {
                    Name = dataObject.Name,
                    ToActorMessage = dataObject.ToActorMessage,
                    ToLocalMessage = dataObject.ToLocalMessage,
                    FoleyUri = dataObject.FoleyUri,
                    StaminaCost = dataObject.StaminaCost,
                    HealthCost = dataObject.HealthCost,
                    Criteria = dataObject.Criteria,
                    Results = dataObject.Results,
                    AffectPattern = dataObject.AffectPattern
                };


                origin.Uses.Add(action);

                if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                {
                    return "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddUse[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return "Interaction Added.";
        }

        public static string EditUse(IInanimateTemplate origin, IUse oldAction, IUse newAction, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || oldAction == null || newAction == null)
            {
                return "Invalid source";
            }

            var action = new Use()
            {
                Name = newAction.Name,
                ToActorMessage = newAction.ToActorMessage,
                ToLocalMessage = newAction.ToLocalMessage,
                FoleyUri = newAction.FoleyUri,
                StaminaCost = newAction.StaminaCost,
                HealthCost = newAction.HealthCost,
                Criteria = newAction.Criteria,
                Results = newAction.Results,
                AffectPattern = newAction.AffectPattern
            };

            origin.Uses.Remove(oldAction);

            origin.Uses.Add(action);

            if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
            {
                return "Error; Edit failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditUse[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return "Interaction modified.";
        }

        public static string RemoveUse(IInanimateTemplate origin, string interactionName, ApplicationUser authedUser, IPrincipal user)
        {
            if (!string.IsNullOrWhiteSpace(interactionName))
            {
                if (origin == null)
                {
                    return "That origin data does not exist";
                }
                else
                {
                    var existingInteraction = origin.Uses.FirstOrDefault(path => path.Name.Equals(interactionName, StringComparison.InvariantCultureIgnoreCase));

                    if (existingInteraction != null)
                    {
                        origin.Uses.Remove(existingInteraction);

                        if (origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                        {
                            LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveUse[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                            return "Delete Successful.";
                        }
                        else
                        {
                            return "Error; Removal failed.";
                        }
                    }
                    else
                    {
                        return "That does not exist";
                    }
                }
            }

            return "Invalid interaction.";
        }

        public static string AddUse(INonPlayerCharacterTemplate origin, IUse dataObject, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || dataObject == null)
            {
                return "Invalid source";
            }

            if (origin.UsableAbilities.Any(path => path.Name == dataObject.Name))
            {
                return "Interaction with that name already exists.";
            }
            else
            {
                var action = new Use()
                {
                    Name = dataObject.Name,
                    ToActorMessage = dataObject.ToActorMessage,
                    ToLocalMessage = dataObject.ToLocalMessage,
                    FoleyUri = dataObject.FoleyUri,
                    StaminaCost = dataObject.StaminaCost,
                    HealthCost = dataObject.HealthCost,
                    Criteria = dataObject.Criteria,
                    Results = dataObject.Results,
                    AffectPattern = dataObject.AffectPattern
                };


                origin.UsableAbilities.Add(action);

                if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                {
                    return "Error; Creation failed.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddUse[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return "Interaction Added.";
        }

        public static string EditUse(INonPlayerCharacterTemplate origin, IUse oldAction, IUse newAction, ApplicationUser authedUser, IPrincipal user)
        {
            if (origin == null || oldAction == null || newAction == null)
            {
                return "Invalid source";
            }

            var action = new Use()
            {
                Name = newAction.Name,
                ToActorMessage = newAction.ToActorMessage,
                ToLocalMessage = newAction.ToLocalMessage,
                FoleyUri = newAction.FoleyUri,
                StaminaCost = newAction.StaminaCost,
                HealthCost = newAction.HealthCost,
                Criteria = newAction.Criteria,
                Results = newAction.Results,
                AffectPattern = newAction.AffectPattern
            };

            origin.UsableAbilities.Remove(oldAction);

            origin.UsableAbilities.Add(action);

            if (!origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
            {
                return "Error; Edit failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditUse[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return "Interaction modified.";
        }

        public static string RemoveUse(INonPlayerCharacterTemplate origin, string interactionName, ApplicationUser authedUser, IPrincipal user)
        {
            if (!string.IsNullOrWhiteSpace(interactionName))
            {
                if (origin == null)
                {
                    return "That origin data does not exist";
                }
                else
                {
                    var existingInteraction = origin.UsableAbilities.FirstOrDefault(path => path.Name.Equals(interactionName, StringComparison.InvariantCultureIgnoreCase));

                    if (existingInteraction != null)
                    {
                        origin.UsableAbilities.Remove(existingInteraction);

                        if (origin.Save(authedUser.GameAccount, authedUser.GetStaffRank(user)))
                        {
                            LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveUse[" + origin.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                            return "Delete Successful.";
                        }
                        else
                        {
                            return "Error; Removal failed.";
                        }
                    }
                    else
                    {
                        return "That does not exist";
                    }
                }
            }

            return "Invalid interaction.";
        }
        #endregion
    }
}