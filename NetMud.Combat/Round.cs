using NetMud.Communication.Messaging;
using NetMud.DataStructure.Combat;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Combat
{
    public static class Round
    {
        /// <summary>
        /// Execute an attack round
        /// </summary>
        /// <param name="actor">the person attacking</param>
        /// <param name="target">the person defending</param>
        public static bool ExecuteRound(IPlayer actor, IPlayer target)
        {
            //Stagger clearance comes before ending a fight
            if (actor.Stagger > 0)
            {
                actor.Stagger -= 1;
                return true;
            }

            if (!actor.IsFighting())
            {
                return false;
            }

            if (actor == target)
            {
                target = null;
            }

            IFightingArt attack = actor.LastAttack;

            //If we lack an attack or we're on the tail end of the attack just find a new one and start it
            if (attack == null || !actor.Executing)
            {
                IFightingArtCombination myCombo = actor.LastCombo;
                if (myCombo == null)
                {
                    ulong distance = 0;
                    if (target != null)
                    {
                        distance = (ulong)Math.Abs((double)(actor.CurrentLocation.CurrentSection - target.CurrentLocation.CurrentSection));
                    }

                    var validCombos = actor.Combos.Where(combo => combo.IsValid(actor, target, distance));

                    if (validCombos.Count() == 0)
                    {
                        myCombo = actor.Combos.FirstOrDefault();
                    }
                    else
                    {
                        myCombo = validCombos.FirstOrDefault();
                    }
                }

                //uhh k
                if (myCombo == null)
                {
                    return false;
                }

                attack = myCombo.GetNext(actor.LastAttack);

                actor.Stagger = attack.Setup;
                actor.Sturdy = attack.Armor;
                actor.LastCombo = myCombo;
                actor.LastAttack = attack;
                actor.Executing = true;

                if (actor.Stagger > 0)
                {
                    actor.Stagger -= 1;
                    return true;
                }
                //else we just run right into the combo if there's no setup
            }

            //execute the attack
            actor.Executing = false;

            //basics
            actor.Stagger = attack.Recovery;
            actor.LastAttack = attack;
            actor.LastCombo = null;
            actor.Balance = attack.DistanceChange;

            //numbers damage
            if (attack.Health.Actor > 0)
            {
                actor.Harm((ulong)attack.Health.Actor);
            }

            if (attack.Stamina.Actor > 0)
            {
                actor.Exhaust(attack.Stamina.Actor);
            }

            var targetGlyph = target == null ? "a shadow" : "$T$";

            string toOrigin = string.Format("$A$ {0}s {1}.", attack.Name, targetGlyph);
            string toActor = string.Format("You {0} {1}.", attack.Name, targetGlyph);
            string toTarget = "";

            targetGlyph = target == null ? "your shadow" : "$T$";

            //messaging

            if (target != null)
            {
                var rand = new Random();
                target.Balance = -1 * attack.DistanceChange;

                toTarget = string.Format("$A$ {0}s you.", attack.Name);

                var targetReadiness = ReadinessState.Offensive;
                if (target.LastAttack != null)
                {
                    targetReadiness = target.LastAttack.Readiness;
                }

                //TODO: for now we're just doing flat chances for avoidance states since we have no stats to base anything on
                var avoided = false;
                double impact = attack.Impact;
                double damage = attack.Health.Victim;
                double staminaDrain = attack.Stamina.Victim;

                switch (targetReadiness)
                {
                    case ReadinessState.Circle: //circle is dodge essentially
                        avoided = rand.Next(0, 100) >= Math.Abs(target.Balance) / 4 * 100;

                        if(avoided)
                        {
                            toActor = string.Format("{0} dodges your attack!", targetGlyph);
                            toTarget = string.Format("You dodge $A$s {0}.", attack.Name);
                            toOrigin = string.Format("$A$ {0}s {1} but they dodge!.", attack.Name, targetGlyph);
                        }
                        break;
                    case ReadinessState.Block:
                        if (rand.Next(0, 100) >= Math.Abs(target.Balance) / 2 * 100)
                        {
                            impact *= .25;
                            damage *= .50;
                            staminaDrain *= .50;

                            toOrigin = string.Format("$A$ {0}s {1} but they block.", attack.Name, targetGlyph);
                            toActor = string.Format("You {0} {1} but they block!", attack.Name, targetGlyph);
                            toTarget = string.Format("$A$ {0}s you but you block it.", attack.Name);
                        };
                        break;
                    case ReadinessState.Deflect:
                        if (rand.Next(0, 100) >= Math.Abs(target.Balance) / 2 * 100)
                        {
                            impact *= .50;
                            damage *= .25;
                            staminaDrain *= .25;

                            toOrigin = string.Format("$A$ {0}s {1} but they deflect the blow.", attack.Name, targetGlyph);
                            toActor = string.Format("You {0} {1} but they deflect it!", attack.Name, targetGlyph);
                            toTarget = string.Format("$A$ {0}s you but you deflect it.", attack.Name);
                        }
                        break;
                    case ReadinessState.Redirect:
                        avoided = rand.Next(0, 100) >= Math.Abs(target.Balance) / 20 * 100;

                        if (avoided)
                        {
                            toActor = string.Format("{0} redirects your attack back to you!", targetGlyph);
                            toTarget = string.Format("You redirect $A$s {0} back at them.", attack.Name);
                            toOrigin = string.Format("$A$ {0}s {1} but they redirect the attack back at them!.", attack.Name, targetGlyph);
                        }
                        break;
                }

                if (!avoided)
                {
                    var victStagger = target.Sturdy - (int)Math.Truncate(impact);

                    //Affect the victim, sturdy/armor absorbs stagger impact and the remainder gets added to victim stagger
                    target.Sturdy = Math.Max(0, victStagger);
                    target.Stagger += Math.Abs(Math.Min(0, victStagger));

                    if (damage > 0)
                    {
                        target.Harm((ulong)Math.Truncate(damage));
                    }

                    if (staminaDrain > 0)
                    {
                        target.Exhaust((int)Math.Truncate(staminaDrain));
                    }

                    //affect qualities
                    if (attack.QualityValue != 0 && !string.IsNullOrWhiteSpace(attack.ResultQuality))
                    {
                        target.SetQuality(attack.QualityValue, attack.ResultQuality, attack.AdditiveQuality);
                    }
                }
            }

            var msg = new Message(toActor)
            {
                ToOrigin = new string[] { toOrigin },
                ToTarget = new string[] { toTarget }
            };


            msg.ExecuteMessaging(actor, null, target, actor.CurrentLocation, null, 3, true);

            return true;
        }
    }
}
