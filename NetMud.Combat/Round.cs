using NetMud.Communication.Messaging;
using NetMud.DataStructure.Architectural.ActorBase;
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

            if (actor == target)
            {
                target = null;
            }
            else
            {
                if (!actor.IsFighting() && target == null)
                {
                    return false;
                }
                else
                {
                    actor.StartFighting(target);
                }
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

            IEnumerable<string> toOrigin = new string[] { string.Format("$A$ {0} {1}.", attack.Name, targetGlyph) };

            targetGlyph = target == null ? "your shadow" : "$T$";

            //messaging
            var msg = new Message(string.Format("You {0} {1}.", attack.Name, targetGlyph))
            {
                ToOrigin = toOrigin
            };

            if (target != null)
            {
                target.Balance = -1 * attack.DistanceChange;

                msg.ToTarget = new string[] { string.Format("$A$ {0} you.", attack.Name) };

                //Affect the victim
                if (target.Sturdy > 0)
                {
                    target.Sturdy = Math.Max(0, target.Sturdy - attack.Impact);
                }
                else
                {
                    target.Stagger += attack.Impact;
                }

                if (attack.Health.Victim > 0)
                {
                    target.Harm((ulong)attack.Health.Victim);
                }

                if (attack.Stamina.Victim > 0)
                {
                    target.Exhaust(attack.Stamina.Victim);
                }

                //affect qualities
                if (attack.QualityValue != 0 && !string.IsNullOrWhiteSpace(attack.ResultQuality))
                {
                    target.SetQuality(attack.QualityValue, attack.ResultQuality, attack.AdditiveQuality);
                }
            }

            msg.ExecuteMessaging(actor, null, target, actor.CurrentLocation, null, 3);

            return true;
        }
    }
}
