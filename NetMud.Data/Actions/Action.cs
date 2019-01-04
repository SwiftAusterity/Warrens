using NetMud.Cartography;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Data.Action
{
    /// <summary>
    /// Interactions for players against tiles
    /// </summary>
    [Serializable]
    public abstract class Action : IAction
    {
        /// <summary>
        /// Name and keyword for the interaction
        /// </summary>
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Action", Description = "The invokable name of the action.")]
        [DataType(DataType.Text)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// How much stamina does this cost to use
        /// </summary>
        [Display(Name = "Stamina Cost", Description = "How much stamina this costs to use.")]
        [DataType(DataType.Text)]
        public int StaminaCost { get; set; }

        /// <summary>
        /// How much health does this cost to use
        /// </summary>
        [Display(Name = "Health Cost", Description = "How much stamina this costs to use.")]
        [DataType(DataType.Text)]
        public int HealthCost { get; set; }

        /// <summary>
        /// Does this interaction have an embedded sound file that plays when it's done
        /// </summary>
        [Display(Name = "Foley URI", Description = "Web address for the sound file this uses when the action is undertaken.")]
        [DataType(DataType.Text)]
        public string FoleyUri { get; set; }

        /// <summary>
        /// Message to send to the origin location of the command/event
        /// </summary>

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 10)]
        [Display(Name = "Zone Response", Description = "Response text sent back to the zone on success for this action.")]
        [DataType(DataType.Text)]
        public string ToLocalMessage { get; set; }

        /// <summary>
        /// Message to send to the acting entity
        /// </summary>
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 10)]
        [Display(Name = "User Response", Description = "Response text sent back to the user on success for this action.")]
        [DataType(DataType.Text)]
        public string ToActorMessage { get; set; }

        /// <summary>
        /// The pattern this affects, can hit multiple things potentially. (viewed as a collection of (X,Y) deltas)
        /// </summary>

        [Display(Name = "Affect Pattern", Description = "The pattern this action affects. Center is the User. (unaffectable, use a Self target result for that) Field rotates depending on direction faced. Up on this field is 'forward'.")]
        [UIHint("AffectPattern")]
        [AffectPatternDataBinder]
        public HashSet<Coordinate> AffectPattern { get; set; }

        /// <summary>
        /// List of criteria to use this
        /// </summary>
        public HashSet<IActionCriteria> Criteria { get; set; }

        /// <summary>
        /// List of results of using this 
        /// </summary>
        public HashSet<IActionResult> Results { get; set; }

        public Action()
        {
            Criteria = new HashSet<IActionCriteria>();
            Results = new HashSet<IActionResult>();
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public abstract object Clone();

        /// <summary>
        /// Invokes this action
        /// </summary>
        /// <param name="actor">The one doing the action</param>
        /// <param name="initialTarget">The initial target</param>
        /// <param name="tool">The tool or use-item</param>
        /// <returns>An error message (or blank for success)</returns>
        public virtual string Invoke(IEntity actor, ITile initialTarget, IInanimate tool)
        {
            string errorMessage = string.Empty;
            var originLocation = (IGlobalPosition)actor.CurrentLocation.Clone();

            IEntity targetItem = null;
            if (initialTarget != null && initialTarget.Type != null)
            {
                targetItem = initialTarget.TopContents();
            }
            else
            {
                initialTarget = actor.CurrentLocation.GetTile();
            }

            var currentPosition = originLocation.Clone(initialTarget.Coordinate);

            List<ITile> affectedTiles = new List<ITile>();
            List<IEntity> affectedItems = new List<IEntity>();

            if (AffectPattern != null)
            {
                MovementDirectionType directionFrom = Utilities.DirectionFrom(originLocation.CurrentCoordinates, initialTarget.Coordinate);
                foreach (Coordinate coordinate in AffectPattern)
                {
                    Coordinate rotationalVariations = Utilities.RotateCoordinateDeltas(coordinate, directionFrom);
                    Coordinate newCoords = new Coordinate((short)(originLocation.CurrentCoordinates.X + rotationalVariations.X),
                                                        (short)(originLocation.CurrentCoordinates.Y + rotationalVariations.Y));
                    var pos = currentPosition.Clone(newCoords);
                    var tile = pos.GetTile();

                    if (tile == null)
                        continue;

                    var intruder = tile.TopContents();

                    if (intruder != null)
                        affectedItems.Add(intruder);
                    else
                        affectedTiles.Add(tile);
                }
            }

            //Check the requirements and remove anything from the list that does not belong
            foreach (IActionCriteria criterion in Criteria.Where(crit => crit.Target != ActionTarget.Tile && crit.Target != ActionTarget.Tool && crit.Target != ActionTarget.Self))
            {
                if (string.IsNullOrWhiteSpace(criterion.Quality))
                    continue;

                switch (criterion.Target)
                {
                    case ActionTarget.Tile:
                        if (criterion.AffectsMemberId > -1)
                        {
                            affectedTiles.RemoveAll(tile => tile.Type.Id != criterion.AffectsMemberId);
                        }

                        if (!string.IsNullOrWhiteSpace(criterion.Quality) && criterion.ValueRange.Low > 0)
                        {
                            affectedTiles.RemoveAll(tile => !tile.Qualities.Any(key =>
                                     key.Type == QualityType.Aspect
                                     && key.Name.Equals(criterion.Quality, StringComparison.InvariantCultureIgnoreCase)
                                     && key.Value.IsBetweenOrEqual(criterion.ValueRange.Low, criterion.ValueRange.High)));
                        }
                        break;
                    case ActionTarget.Player:
                        if (!string.IsNullOrWhiteSpace(criterion.Quality) && criterion.ValueRange.Low > 0)
                        {
                            affectedItems.RemoveAll(intruder => intruder.ImplementsType<IPlayer>()
                                 && !intruder.Qualities.Any(key => key.Type == QualityType.Aspect
                                 && key.Value.IsBetweenOrEqual(criterion.ValueRange.Low, criterion.ValueRange.High)
                                 && key.Name.Equals(criterion.Quality, StringComparison.InvariantCultureIgnoreCase)));
                        }
                        break;
                    case ActionTarget.NPC:
                        if (criterion.AffectsMemberId > -1)
                        {
                            affectedItems.RemoveAll(intruder => intruder.TemplateId != criterion.AffectsMemberId);
                        }

                        if (!string.IsNullOrWhiteSpace(criterion.Quality) && criterion.ValueRange.Low > 0)
                        {
                            affectedItems.RemoveAll(intruder => intruder.ImplementsType<INonPlayerCharacterTemplate>()
                                 && !intruder.Qualities.Any(key => key.Type == QualityType.Aspect
                                 && key.Value.IsBetweenOrEqual(criterion.ValueRange.Low, criterion.ValueRange.High)
                                 && key.Name.Equals(criterion.Quality, StringComparison.InvariantCultureIgnoreCase)));
                        }
                        break;
                    case ActionTarget.Item:
                        if (criterion.AffectsMemberId > -1)
                        {
                            affectedItems.RemoveAll(intruder => intruder.TemplateId != criterion.AffectsMemberId);
                        }

                        if (!string.IsNullOrWhiteSpace(criterion.Quality) && criterion.ValueRange.Low > 0)
                        {
                            affectedItems.RemoveAll(intruder => intruder.ImplementsType<IInanimate>()
                                 && !intruder.Qualities.Any(key => key.Type == QualityType.Aspect
                                 && key.Value.IsBetweenOrEqual(criterion.ValueRange.Low, criterion.ValueRange.High)
                                 && key.Name.Equals(criterion.Quality, StringComparison.InvariantCultureIgnoreCase)));
                        }
                        break;
                }
            }

            HashSet<Coordinate> tileUpdates = new HashSet<Coordinate>();

            foreach (IActionResult result in Results.Where(result => result.Target == ActionTarget.Tool))
            {
                if (!string.IsNullOrWhiteSpace(result.Quality))
                    tool.SetQuality(result.QualityValue, result.Quality, result.AdditiveQuality);

                if (result.Consumes)
                    tool.Remove();
            }

            Random rand = new Random();
            foreach (IGrouping<short, IActionResult> resultGroup in Results.GroupBy(result => result.OccurrenceChanceGroupId))
            {
                if (resultGroup.Count() == 0)
                    continue;

                IEnumerable<IActionResult> ourResults;

                //ungrouped
                if (resultGroup.Key > 0)
                {
                    ourResults = resultGroup;    
                }
                else
                {
                    //limit it to the first one of the group
                    ourResults = resultGroup.OrderByDescending(result => rand.Next(0, result.OccurrenceChanceRate)).Take(1);
                }

                foreach (IActionResult result in resultGroup)
                {
                    if (result.Target == ActionTarget.Self && !string.IsNullOrWhiteSpace(result.Quality))
                    {
                        actor.SetQuality(result.QualityValue, result.Quality, result.AdditiveQuality);
                    }
                    else if (result.Target == ActionTarget.Tile)
                    {
                        foreach (ITile tile in affectedTiles)
                        {
                            ApplyInteraction(tile, actor.CurrentLocation.CurrentZone, (IContains)actor, result);
                            tileUpdates.Add(tile.Coordinate);
                        }
                    }
                    else if (result.Target == ActionTarget.Player || result.Target == ActionTarget.NPC || result.Target == ActionTarget.Item)
                    {
                        foreach (IEntity item in affectedItems)
                        {
                            ApplyInteraction(item, actor.CurrentLocation.CurrentZone, (IContains)actor, result);
                            tileUpdates.Add(item.CurrentLocation.CurrentCoordinates);
                            item.Save();
                        }
                    }
                }
            }

            if (actor.ImplementsType<IMobile>())
            {
                var dude = (IMobile)actor;

                if (StaminaCost > 0)
                    dude.Exhaust(StaminaCost);
                if (HealthCost > 0)
                    dude.Harm(HealthCost);
            }

            List<string> sb = new List<string>();
            if (string.IsNullOrWhiteSpace(ToActorMessage))
                sb.Add(string.Format("You interact with {0}.", targetItem == null ? initialTarget.Type.Name : targetItem.GetDescribableName(actor)));
            else
                sb.Add(ToActorMessage);

            string toRoomMessage = string.Format("$A$ interacts with {0}.", targetItem == null ? initialTarget.Type.Name : targetItem.GetDescribableName(actor));

            if (!string.IsNullOrWhiteSpace(ToLocalMessage))
                toRoomMessage = ToLocalMessage;

            Message toactor = new Message()
            {
                Body = sb
            };

            Message toOrigin = new Message()
            {
                Body = new string[] { toRoomMessage }
            };

            MessageCluster messagingObject = new MessageCluster(toactor)
            {
                ToOrigin = new List<IMessage> { toOrigin }
            };

            messagingObject.ExecuteMessaging(actor, null, null, originLocation.CurrentZone, null);

            //play a sound
            if (!string.IsNullOrWhiteSpace(FoleyUri))
            {
                IEnumerable<IPlayer> viewers = LiveCache.GetAll<IPlayer>().Where(pl => pl.CurrentLocation?.CurrentZone == actor.CurrentLocation.CurrentZone);

                foreach (IPlayer viewer in viewers)
                {
                    viewer.Descriptor.SendSound(FoleyUri);
                }
            }

            tileUpdates.Add(currentPosition.CurrentCoordinates);

            //Cause a map delta
            Utilities.SendMapUpdatesToZone(actor.CurrentLocation.CurrentZone, tileUpdates);
            actor.Save();
            actor.CurrentLocation.CurrentZone.Save();

            return errorMessage;
        }

        internal void ApplyInteraction(ITile tile, IZone currentZone, IContains actor, IActionResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.Quality))
                tile.SetQuality(result.QualityValue, result.Quality, result.AdditiveQuality);

            if (result.Result != null)
            {
                tile.Type = result.Result;
            }

            if (result.Produces != null)
            {
                int i = 0;
                while (i < result.ProducesAmount)
                {
                    var newThing = Activator.CreateInstance(result.Produces.EntityClass, new object[] { result.Produces }) as IEntity;

                    //Add all the items to the player inventory
                    if (result.ProducesToInventory && actor != null)
                    {
                        newThing.SpawnNewInWorld(actor.GetContainerAsLocation());
                    }
                    else
                    {
                        // or the floor
                    }

                    i++;
                }
            }
        }

        internal void ApplyInteraction(IEntity entity, IZone currentZone, IContains actor, IActionResult result)
        {
            if ((entity.ImplementsType<IPlayer>() && result.Target == ActionTarget.Player) ||
                (entity.ImplementsType<INonPlayerCharacterTemplate>() && result.Target == ActionTarget.NPC) ||
                (entity.ImplementsType<IInanimate>() && result.Target == ActionTarget.Item))
            {
                if (result.Produces != null)
                {
                    int i = 0;
                    while (i < result.ProducesAmount)
                    {
                        var newThing = Activator.CreateInstance(result.Produces.EntityClass, new object[] { result.Produces }) as IEntity;

                        //Add all the items to the player inventory
                        if (result.ProducesToInventory && actor != null)
                        {
                            newThing.SpawnNewInWorld(actor.GetContainerAsLocation());
                        }
                        else
                        {
                            // or the floor
                        }

                        i++;
                    }
                }

                if (result.Consumes && (entity.ImplementsType<INonPlayerCharacterTemplate>() || entity.ImplementsType<IInanimate>()))
                {
                    entity.Remove();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(result.Quality))
                    entity.SetQuality(result.QualityValue, result.Quality, result.AdditiveQuality);

                if (entity.ImplementsType<IMobile>())
                {
                    var dude = (IMobile)entity;

                    if (StaminaCost > 0)
                        dude.Exhaust(StaminaCost);
                    if (HealthCost > 0)
                        dude.Harm(HealthCost);
                }
            }
        }

    }
}
