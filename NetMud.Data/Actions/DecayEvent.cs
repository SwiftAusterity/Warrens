using NetMud.Cartography;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
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
    public class DecayEvent : Action, IDecayEvent
    {
        /// <summary>
        /// The number of seconds this will last for until it fires
        /// </summary>
        [Display(Name = "Timer", Description = "Number of seconds this timer spawns with before the event fires.")]
        [DataType(DataType.Text)]
        public int Timer { get; set; }

        /// <summary>
        /// The current time of the timer
        /// </summary>
        public int CurrentTime { get; set; }

        public DecayEvent() : base()
        {
            CurrentTime = Timer;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new DecayEvent
            {
                FoleyUri = FoleyUri,
                Name = Name,
                StaminaCost = StaminaCost,
                ToLocalMessage = ToLocalMessage,
                ToActorMessage = ToActorMessage,
                HealthCost = HealthCost,
                Criteria = Criteria,
                Results = Results,
                Timer = Timer,
                CurrentTime = Timer,
            };
        }

        /// <summary>
        /// Invokes this action
        /// </summary>
        /// <param name="actor">The one doing the action</param>
        /// <param name="initialTarget">The initial target</param>
        /// <param name="tool">The tool or use-item</param>
        /// <returns>An error message (or blank for success)</returns>
        public override string Invoke(IEntity actor, ITile initialTarget, IInanimate tool)
        {
            string errorMessage = string.Empty;

            if (initialTarget == null || initialTarget.Type == null)
            {
                return "That is an invalid direction to interact with.";
            }

            IGlobalPosition originLocation = new GlobalPosition(initialTarget.ParentLocation) { CurrentCoordinates = initialTarget.Coordinate };

            if (actor != null)
                originLocation = (IGlobalPosition)actor.CurrentLocation.Clone();

            var currentPosition = originLocation.Clone(initialTarget.Coordinate);
            var targetItem = initialTarget.TopContents();

            List<ITile> affectedTiles = new List<ITile>();
            List<IEntity> affectedItems = new List<IEntity>();
            HashSet<Coordinate> tileUpdates = new HashSet<Coordinate>();

            if (AffectPattern != null)
            {
                foreach (var coordinate in AffectPattern)
                {
                    Coordinate newCoords = new Coordinate((short)(currentPosition.CurrentCoordinates.X + coordinate.X), (short)(currentPosition.CurrentCoordinates.X + coordinate.Y));
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
                        affectedTiles.RemoveAll(tile => tile.Qualities.Any(key =>
                                 key.Type == QualityType.Aspect
                                 && (criterion.ValueRange.Low <= 0 || key.Value.IsBetweenOrEqual(criterion.ValueRange.Low, criterion.ValueRange.High))
                                 && key.Name.Equals(criterion.Quality, StringComparison.InvariantCultureIgnoreCase)));
                        break;
                    case ActionTarget.Player:
                        affectedItems.RemoveAll(intruder => intruder.ImplementsType<IPlayer>()
                            && intruder.Qualities.Any(key => key.Type == QualityType.Aspect
                                 && (criterion.ValueRange.Low <= 0 || key.Value.IsBetweenOrEqual(criterion.ValueRange.Low, criterion.ValueRange.High))
                                 && key.Name.Equals(criterion.Quality, StringComparison.InvariantCultureIgnoreCase)));
                        break;
                    case ActionTarget.NPC:
                        affectedItems.RemoveAll(intruder => intruder.ImplementsType<INonPlayerCharacterTemplate>()
                            && intruder.Qualities.Any(key => key.Type == QualityType.Aspect
                                 && (criterion.ValueRange.Low <= 0 || key.Value.IsBetweenOrEqual(criterion.ValueRange.Low, criterion.ValueRange.High))
                                 && key.Name.Equals(criterion.Quality, StringComparison.InvariantCultureIgnoreCase)));
                        break;
                    case ActionTarget.Item:
                        affectedItems.RemoveAll(intruder => intruder.ImplementsType<IInanimate>()
                            && intruder.Qualities.Any(key => key.Type == QualityType.Aspect
                                 && (criterion.ValueRange.Low <= 0 || key.Value.IsBetweenOrEqual(criterion.ValueRange.Low, criterion.ValueRange.High))
                                 && key.Name.Equals(criterion.Quality, StringComparison.InvariantCultureIgnoreCase)));
                        break;
                }
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
                    if (actor != null && result.Target == ActionTarget.Self && !string.IsNullOrWhiteSpace(result.Quality))
                    {
                        actor.SetQuality(result.QualityValue, result.Quality, result.AdditiveQuality);
                    }
                    else if (result.Target == ActionTarget.Tile)
                    {
                        foreach (ITile tile in affectedTiles)
                        {
                            ApplyInteraction(tile, currentPosition.CurrentZone, actor as IContains, result);
                            tileUpdates.Add(tile.Coordinate);
                        }
                    }
                    else if (result.Target == ActionTarget.Player || result.Target == ActionTarget.NPC || result.Target == ActionTarget.Item)
                    {
                        foreach (IEntity item in affectedItems)
                        {
                            ApplyInteraction(item, currentPosition.CurrentZone, actor as IContains, result);
                            tileUpdates.Add(item.CurrentLocation.CurrentCoordinates);
                            item.Save();
                        }
                    }
                }
            }

            MessageCluster messagingObject = new MessageCluster();
            if (actor != null)
            {
                if (actor.ImplementsType<IMobile>())
                {
                    var dude = (IMobile)actor;

                    if (StaminaCost > 0)
                        dude.Exhaust(StaminaCost);
                    if (HealthCost > 0)
                        dude.Harm(HealthCost);
                }

                string toActorString = ToActorMessage;
                if (string.IsNullOrWhiteSpace(ToActorMessage))
                    toActorString = string.Format("You interact with {0}.", targetItem == null ? initialTarget.Type.Name : targetItem.GetDescribableName(actor));

                Message toactor = new Message()
                {
                    Body = new string[] { toActorString }
                };

                messagingObject = new MessageCluster(toactor);
            }

            string toRoomMessage = string.Format("$A$ interacts with {0}.", targetItem == null ? initialTarget.Type.Name : targetItem.GetDescribableName(actor));

            if (!string.IsNullOrWhiteSpace(ToLocalMessage))
                toRoomMessage = ToLocalMessage;

            Message toOrigin = new Message()
            {
                Body = new string[] { toRoomMessage }
            };

            messagingObject.ToOrigin = new List<IMessage> { toOrigin };

            messagingObject.ExecuteMessaging(actor, null, null, originLocation.CurrentZone, null);

            //play a sound
            if (!string.IsNullOrWhiteSpace(FoleyUri))
            {
                IEnumerable<IPlayer> viewers = LiveCache.GetAll<IPlayer>().Where(pl => pl.CurrentLocation?.CurrentZone == currentPosition.CurrentZone);

                foreach (IPlayer viewer in viewers)
                {
                    viewer.Descriptor.SendSound(FoleyUri);
                }
            }

            //Cause a map delta
            Utilities.SendMapUpdatesToZone(currentPosition.CurrentZone, new HashSet<Coordinate>() { currentPosition.CurrentCoordinates });
            originLocation.CurrentZone.Save();

            return errorMessage;
        }

    }
}
