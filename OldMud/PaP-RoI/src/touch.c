/***************************************************************************
 *  Original Diku Mud copyright (C) 1990, 1991 by Sebastian Hammer,        *
 *  Michael Seifert, Hans Henrik St{rfeldt, Tom Madsen, and Katja Nyboe.   *
 *                                                                         *
 *  Merc Diku Mud improvments copyright (C) 1992, 1993 by Michael          *
 *  Chastain, Michael Quan, and Mitchell Tse.                              *
 *                                                                         *
 *  Envy Diku Mud improvements copyright (C) 1994 by Michael Quan, David   *
 *  Love, Guilherme 'Willie' Arnold, and Mitchell Tse.                     *
 *                                                                         *
 *  In order to use any part of this Envy Diku Mud, you must comply with   *
 *  the original Diku license in 'license.doc', the Merc license in        *
 *  'license.txt', as well as the Envy license in 'license.nvy'.           *
 *  In particular, you may not remove either of these copyright notices.   *
 *                                                                         *
 *  Much time and thought has gone into this software and you are          *
 *  benefitting.  We hope that you share your changes too.  What goes      *
 *  around, comes around.                                                  *
 ***************************************************************************/

/* This is all me baby. Since I hate all the spell-classes having the same
   function for spell casting, I decided to seperate them into 
   power-word(normal casting/warlocks) and touch(clerics). Hopefully
   others will like what I've done. Touch casting is similar, but it requires
   the caster to have his/her hands free (duh), it has no spoken words, and
   it forces the player to stop hitting the other person in combat for a round.
   The benefits of touch casting, besides being able to do it through mute,
   are that they bypass save_spell, they bypass magical dampeners (which only
   exist in this mud anyways) and if the person is too weak to break the touch,
   the spell can last for many rounds( 1 tick ). This stops normal combat
   hitting on both
   sides. If you don't like it, I left all the equilevants as normal spells, so
   just change the slist for cleric/heretic around and fix the
   (spell_x, touch_x) line in
   the cast function. - Flux (original concept -Swift) */

#define linux 1
#if defined( macintosh )
#include <types.h>
#else
#include <sys/types.h>
#endif
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include "merc.h"

int sc_dam args( (CHAR_DATA *ch, int dam, int element ) );

/* touch functions */

void touch_null( int sn, int level, CHAR_DATA *ch, void *vo )
{
    send_to_char(AT_WHITE, "That's not an invokation.\n\r", ch );
    return;
}

void touch_shocking_grasp( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_NEGATIVE );
    damage( ch, victim, dam, sn, DAM_NEGATIVE, TRUE );
 
    return;
}

void touch_cure_blindness( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( !is_affected( victim, gsn_blindness ) )
	return;

    affect_strip( victim, gsn_blindness );

    act(AT_BLUE, "You touch $N and $s eyes are healed.", 
     ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n touches $N's eyes, and they are healed.", 
     ch, NULL, victim, TO_NOTVICT );
    act(AT_BLUE, "$n touches your eyes, and they are healed.", 
     ch, NULL, victim, TO_VICT );
    return;
}

void touch_cure_critical( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        heal;

    heal = dice( 3, 8 ) + level / 2;
    if ( (victim->hit + heal) > MAX_HIT(victim) )
    heal = MAX_HIT(victim) - victim->hit;  

    if ( IS_NPC(victim) 
     && !IS_SET(victim->act, ACT_UNDEAD))
    {
     victim->hit += heal;
     send_to_char( AT_WHITE, "Ok.\n\r", ch );
    }
    else 
     damage( ch, victim, heal, sn, DAM_REGEN, TRUE  );

   if ( ch != victim )
    act(AT_BLUE, "You touch $N and $E is healed.", 
     ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n touches $N, and $E is healed.", 
     ch, NULL, victim, TO_NOTVICT );
   if ( ch != victim )
    act(AT_BLUE, "$n touches you, and you are healed.", 
     ch, NULL, victim, TO_VICT );
    update_pos( victim, ch );
    return;
}



void touch_cure_light( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        heal;

    heal = dice( 1, 8 ) + level / 5;
    if ( (victim->hit + heal) > MAX_HIT(victim) )
    heal = MAX_HIT(victim) - victim->hit;  

    if ( IS_NPC(victim) 
     && !IS_SET(victim->act, ACT_UNDEAD))
    {
    victim->hit += heal;
    send_to_char( AT_WHITE, "Ok.\n\r", ch );
    }
    else 
     damage( ch, victim, heal, sn, DAM_REGEN, TRUE  );

    act(AT_BLUE, "You touch $N and $E is healed.", 
     ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n touches $N, and $E is healed.", 
     ch, NULL, victim, TO_NOTVICT );
    act(AT_BLUE, "$n touches you, and you are healed.", 
     ch, NULL, victim, TO_VICT );
    update_pos( victim, ch );
    return;
}



void touch_cure_poison( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( !is_affected( victim, gsn_poison ) )
        return;

    affect_strip( victim, gsn_poison );


    act(AT_BLUE, "You touch $N and $E is relieved of the poison.", 
     ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n touches $N, and $E is healed.", 
     ch, NULL, victim, TO_NOTVICT );
    act(AT_BLUE, "$n touches you, and the poison is purged from your body.", 
     ch, NULL, victim, TO_VICT );

    return;
}

void touch_cure_disease( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( !is_affected( victim, gsn_plague ) )
        return;

    affect_strip( victim, gsn_plague );
    REMOVE_BIT( victim->affected_by2, AFF_DISEASED );

    act(AT_BLUE, "You touch $N and $E is relieved of the disease.", 
     ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n touches $N, and $E is healed.", 
     ch, NULL, victim, TO_NOTVICT );
    act(AT_BLUE, "$n touches you, and the disease is purged from your body.", 
     ch, NULL, victim, TO_VICT );

    return;
}

void touch_remove_plasma( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( !is_affected( victim, gsn_plasma) )
        return;

    affect_strip( victim, gsn_plasma );
    REMOVE_BIT( victim->affected_by2, AFF_PLASMA );

    act(AT_BLUE, "You touch $N and $E is relieved of the plasma.", 
     ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n touches $N, and $E is healed.", 
     ch, NULL, victim, TO_NOTVICT );
    act(AT_BLUE, "$n touches you, and the plasma is purged from your body.", 
     ch, NULL, victim, TO_VICT );

    return;
}

void touch_cure_serious( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        heal;

    heal = dice( 2, 8 ) + level / 3;
    if ( (victim->hit + heal) > MAX_HIT(victim) )
    heal = MAX_HIT(victim) - victim->hit;  

    if ( IS_NPC(victim) 
     && !IS_SET(victim->act, ACT_UNDEAD))
    {
    victim->hit += heal;
    send_to_char( AT_WHITE, "Ok.\n\r", ch );
    }
    else 
     damage( ch, victim, heal, sn, DAM_REGEN, TRUE  );

    act(AT_BLUE, "You touch $N and $E is healed.", 
     ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n touches $N, and $E is healed.", 
     ch, NULL, victim, TO_NOTVICT );
    act(AT_BLUE, "$n touches you, and you are healed.", 
     ch, NULL, victim, TO_VICT );
    update_pos( victim, ch );
    return;
}



void touch_curse( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_CURSE ) || saves_spell( level, victim ) )
    {
	send_to_char(AT_RED, "You have failed.\n\r", ch );
	return;
    }

    af.type      = sn;
    af.level	 = level;
    af.duration  = 4 * level;
    af.location  = APPLY_HITROLL;
    af.modifier  = -1;
    af.bitvector = AFF_CURSE;
    affect_to_char( victim, &af );

    af.location  = APPLY_MDAMP;
    af.modifier  = -10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    act(AT_YELLOW, "You touch $N and $E cursed.", 
     ch, NULL, victim, TO_CHAR );
    act(AT_RED, "$n touches $N, and $E is cursed.", 
     ch, NULL, victim, TO_NOTVICT );
    act(AT_RED, "$n touches you, and you are cursed!", 
     ch, NULL, victim, TO_VICT );

    return;
}

void touch_truesight( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED2( victim, AFF_TRUESIGHT ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level / 4;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_TRUESIGHT;
    affect_to_char2( victim, &af );

    if ( ch != victim )
    act(AT_BLUE, "You touch $N's eyes and $E sees all.", 
     ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n touches $N's eyes and $E is granted true sight.", 
     ch, NULL, victim, TO_NOTVICT );
    act(AT_BLUE, "$n touches your eyes and you see all.", 
     ch, NULL, victim, TO_VICT );

    return;
}
