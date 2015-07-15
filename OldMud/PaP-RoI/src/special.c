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

/* call some stuff from fight.c */
extern void set_fighting     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
extern void trip	args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );


/*
 * The following special functions are available for mobiles.
 */
DECLARE_SPEC_FUN( spec_assassin 			);
DECLARE_SPEC_FUN( spec_executioner 			);
DECLARE_SPEC_FUN( spec_fido 				);
DECLARE_SPEC_FUN( spec_guard 				);
DECLARE_SPEC_FUN( spec_janitor 				);
DECLARE_SPEC_FUN( spec_mayor 				);
DECLARE_SPEC_FUN( spec_thief 				);
DECLARE_SPEC_FUN( spec_wanderer 			);
DECLARE_SPEC_FUN( spec_snake_charm			);
DECLARE_SPEC_FUN( spec_healer				);
DECLARE_SPEC_FUN( spec_utility_repairman 		);
DECLARE_SPEC_FUN( spec_utility_artifactor		);
DECLARE_SPEC_FUN( spec_utility_orthopedic_surgeon	);
DECLARE_SPEC_FUN( spec_utility_laser_surgeon		);
DECLARE_SPEC_FUN( spec_utility_smith			);
DECLARE_SPEC_FUN( spec_fighting_caster_wizard		);
DECLARE_SPEC_FUN( spec_fighting_caster_cleric		);
DECLARE_SPEC_FUN( spec_fighting_caster_illusionist	);
DECLARE_SPEC_FUN( spec_fighting_caster_necromancer	);
DECLARE_SPEC_FUN( spec_fighting_caster_druid		);

/*
 * Special Functions Table.	OLC
 */
const	struct	spec_type	spec_table	[ ] =
{
    /*
     * Special function commands.
     */
    { "spec_assassin",			spec_assassin			},
    { "spec_executioner",		spec_executioner		},
    { "spec_fido",			spec_fido			},
    { "spec_guard",			spec_guard			},
    { "spec_janitor",			spec_janitor			},
    { "spec_mayor",			spec_mayor			},
    { "spec_thief",			spec_thief			},
    { "spec_healer",            	spec_healer			},
    { "spec_wanderer",          	spec_wanderer           	},
    { "spec_snake_charmer",		spec_snake_charm		},
    { "spec_utility_repairman",		spec_utility_repairman		},
    { "spec_utility_artifactor",	spec_utility_artifactor		},
    { "spec_utility_orthopedic_surgeon",spec_utility_orthopedic_surgeon	},
    { "spec_utility_laser_surgeon",	spec_utility_laser_surgeon	},
    { "spec_utility_smith",		spec_utility_smith		},
    { "spec_fighting_caster_wizard",	spec_fighting_caster_wizard	},
    { "spec_fighting_caster_druid",	spec_fighting_caster_druid	},
    { "spec_fighting_caster_necromancer", spec_fighting_caster_necromancer },
    { "spec_fighting_caster_illusionist", spec_fighting_caster_illusionist },
    { "spec_fighting_caster_cleric",	spec_fighting_caster_cleric	},

    /*
     * End of list.
     */
    { "",			0	}
};

/*****************************************************************************
 Name:		spec_string
 Purpose:	Given a function, return the appropriate name.
 Called by:	<???>
 ****************************************************************************/
char *spec_string( SPEC_FUN *fun )	/* OLC */
{
    int cmd;
    
    for ( cmd = 0; *spec_table[cmd].spec_fun; cmd++ )	/* OLC 1.1b */
	if ( fun == spec_table[cmd].spec_fun )
	    return spec_table[cmd].spec_name;

    return 0;
}

/*****************************************************************************
 Name:		spec_lookup
 Purpose:	Given a name, return the appropriate spec fun.
 Called by:	do_mset(act_wiz.c) load_specials,reset_area(db.c)
 ****************************************************************************/
SPEC_FUN *spec_lookup( const char *name )	/* OLC */
{
    int cmd;
    
    for ( cmd = 0; *spec_table[cmd].spec_name; cmd++ )	/* OLC 1.1b */
	if ( !str_cmp( name, spec_table[cmd].spec_name ) )
	    return spec_table[cmd].spec_fun;

    return 0;
}

/*
 * Special procedures for mobiles.
 */

/* This function was written by Rox of Farside, Permission to
 * use is granted provided this header is retained 
 */

/*Converted for PaP code from Rom 2.4 code by -Flux. */
bool spec_assassin( CHAR_DATA *ch )
{
    char buf[MAX_STRING_LENGTH];
    CHAR_DATA *victim;
    CHAR_DATA *v_next;
         int rnd_say;

    if ( ch->fighting != NULL )
                return FALSE;

    for ( victim = ch->in_room->people; victim != NULL; victim = v_next )
    {
     v_next = victim->next_in_room;
               /* this should kill mobs as well as players */
                        if (is_class( victim, CLASS_ASSASSIN))  /* asns */
                                break;
    }

    if ( victim == NULL || victim == ch || victim->level >= LEVEL_IMMORTAL )
        return FALSE;
    if ( victim->level > ch->level + 7 || IS_NPC(victim))
        return FALSE;

   rnd_say = number_range (1, 10);
                
   if ( rnd_say <= 5)
                sprintf( buf, "Death to is the true end...");
   else if ( rnd_say <= 6)
                sprintf( buf, "Time to die....");
   else if ( rnd_say <= 7)
                sprintf( buf, "Cabrone...."); 
   else if ( rnd_say <= 8)
                sprintf( buf, "Welcome to your fate....");
   else if ( rnd_say <= 9)
         ;
   else if ( rnd_say <= 10)
                sprintf( buf, "Ever dance with the devil...."); 

    do_say( ch, buf );
    multi_hit( ch, victim, gsn_backstab );
    return TRUE;
}

bool spec_utility_artifactor( CHAR_DATA *ch )
{
 return TRUE;
}

bool spec_utility_orthopedic_surgeon( CHAR_DATA *ch )
{
 return TRUE;
}

bool spec_utility_laser_surgeon( CHAR_DATA *ch )
{
 return TRUE;
}

bool spec_utility_smith( CHAR_DATA *ch )
{
 return TRUE;
}

bool spec_healer( CHAR_DATA *ch )
{
    CHAR_DATA *victim;

    if ( ch->fighting )
	return FALSE;

    for ( victim = ch->in_room->people; victim; victim = victim->next_in_room )
    {
	if ( victim != ch && can_see( ch, victim ) && number_bits( 1 ) == 0 )
	    break;
    }

    if ( !victim || IS_NPC( victim ) )
	return FALSE;


    switch ( number_bits( 4 ) )
    {
    case 0:
	act(C_DEFAULT, "$n utters the word 'rodalaht'.", ch, NULL, NULL, TO_ROOM );
	spell_armor( skill_lookup( "armor" ), ch->level, ch, victim );
	return TRUE;

    case 1:
	act(C_DEFAULT, "$n utters the word 'igna'.",    ch, NULL, NULL, TO_ROOM );
	spell_bless( skill_lookup( "bless" ), ch->level, ch, victim );
	return TRUE;

    case 2:
	act(C_DEFAULT, "$n utters the word 'naeralkced'.",   ch, NULL, NULL, TO_ROOM );
	spell_heal_blindness( skill_lookup( "heal blindness" ),
			     ch->level, ch, victim );
	return TRUE;

    case 3:
	act(C_DEFAULT, "$n utters the words 'obasi'.",  ch, NULL, NULL, TO_ROOM );
	spell_heal_poison( skill_lookup( "heal poison" ),
			  ch->level, ch, victim );
	return TRUE;

    case 4:
	act(C_DEFAULT, "$n utters the words 'irehx'.",
         ch, NULL, NULL, TO_ROOM );
	spell_refresh( skill_lookup( "refresh" ), ch->level, ch, victim );
	return TRUE;

    case 5:
	act(C_DEFAULT, "$n utters the words 'ecnanep'.", ch, NULL, NULL, TO_ROOM );
	spell_shield( skill_lookup( "shield" ), ch->level, ch, victim );
	return TRUE;

    case 6:
	act(C_DEFAULT, "$n utters the words 'ealha eriousa'.",
         ch, NULL, NULL, TO_ROOM );
	spell_refresh( skill_lookup( "heal serious" ), ch->level, ch, victim );
	return TRUE;
    }

    return FALSE;
}

bool spec_executioner( CHAR_DATA *ch )
{
    CHAR_DATA *guard;
    CHAR_DATA *victim;
    char      *crime;
    char       buf [ MAX_STRING_LENGTH ];

    if ( !IS_AWAKE( ch ) || ch->fighting )
	return FALSE;

    crime = "";
    for ( victim = ch->in_room->people; victim; victim = victim->next_in_room )
    {
        if ( victim->deleted )
	    continue;

	if ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_KILLER ) )
	    { crime = "KILLER"; break; }
    }

    if ( !victim )
	return FALSE;

    sprintf( buf, "%s is a %s!  JUSTICE WILL PREVAIL! I SENTENCE %s TO DEATH!!!",
	    victim->name, crime, victim->name );
    do_yell( ch, buf );

         multi_hit( ch, victim, TYPE_UNDEFINED );

         guard = create_mobile( get_mob_index( MOB_VNUM_CITYGUARD ) );
         char_to_room( guard, ch->in_room );
         guard->fighting = ch->fighting;
         guard->position = POS_FIGHTING;
         guard->summon_timer = 15;

         guard = create_mobile( get_mob_index( MOB_VNUM_CITYGUARD ) );
         char_to_room( guard, ch->in_room );
         guard->fighting = ch->fighting;
         guard->position = POS_FIGHTING;
         guard->summon_timer = 15;
	 

     return TRUE;
}

bool spec_fido( CHAR_DATA *ch )
{
    OBJ_DATA *obj;
    OBJ_DATA *obj_next;
    OBJ_DATA *corpse;
    OBJ_DATA *corpse_next;

    if ( !IS_AWAKE( ch ) )
	return FALSE;

    for ( corpse = ch->in_room->contents; corpse; corpse = corpse_next )
    {
        corpse_next = corpse->next_content;
        if ( corpse->deleted )
	    continue;
	if ( corpse->item_type != ITEM_CORPSE_NPC )
	    continue;

	act(C_DEFAULT, "$n savagely devours a corpse.", ch, NULL, NULL, TO_ROOM );
	for ( obj = corpse->contains; obj; obj = obj_next )
	{
	    obj_next = obj->next_content;
	    if ( obj->deleted )
	        continue;
	    obj_from_obj( obj, FALSE );
	    obj_to_room( obj, ch->in_room );
	}
	extract_obj( corpse );
	return TRUE;
    }

    return FALSE;
}

bool spec_guard( CHAR_DATA *ch )
{
    CHAR_DATA *victim;
    CHAR_DATA *ech;
    char      *crime;
    char       crimename [MAX_STRING_LENGTH ];
    char       buf [ MAX_STRING_LENGTH ];
    int        max_guilty, max_framed;
    if ( !IS_AWAKE( ch ) || ch->fighting )
	return FALSE;

    max_guilty = -1000;
    max_framed = 1000;
    ech      = NULL;
    crime    = "";

    for ( victim = ch->in_room->people; victim; victim = victim->next_in_room )
    {
        if ( victim->deleted )
	    continue;

	if ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_KILLER ) )
	    { crime = "KILLER"; break; }

	if ( victim->fighting
	    && ( IS_FRAMED( ch ) || IS_LOS( ch ) )
	    && victim->fighting != ch
	    && victim->alignment < max_guilty )
	{
	    max_guilty = victim->alignment;
	    ech      = victim;
	}
        if ( victim->fighting
	    && IS_GUILTY( ch )
	    && victim->fighting != ch
	    && victim->alignment > max_framed )
	{
	    max_framed = victim->alignment;
	    ech	     = victim;
	}
    }

    if ( victim )
    {
        strcpy( crimename, "" );
        if (!str_cmp("KILLER",crime))
	strcpy( crimename, "KILLERS" );
        if (!str_cmp("THIEF",crime))
        strcpy( crimename, "THIEVES" );
	sprintf( buf, "%s is a %s!  JUSTICE WILL PREVAIL!! DEATH TO %s!!!",
		victim->name, crime, crimename );
	do_yell( ch, buf );
	multi_hit( ch, victim, TYPE_UNDEFINED );
	return TRUE;
    }

    if ( ech )
    {
	if ( IS_GUILTY( ch ) )
	  sprintf( buf, "$n screams 'EVIL MUST PREVAIL!!'" );
	else
	  sprintf( buf, "$n screams 'PROTECT THE INNOCENT!!'" );
	act(C_DEFAULT, buf, ch, NULL, NULL, TO_ROOM );
	multi_hit( ch, ech, TYPE_UNDEFINED );
	return TRUE;
    }

    return FALSE;
}

bool spec_janitor( CHAR_DATA *ch )
{
    OBJ_DATA *trash;
    OBJ_DATA *trash_next;

    if ( !IS_AWAKE( ch ) )
	return FALSE;

    for ( trash = ch->in_room->contents; trash; trash = trash_next )
    {
        trash_next = trash->next_content;
        if ( trash->deleted )
	    continue;
	if ( !IS_SET( trash->wear_flags, ITEM_TAKE ) )
	    continue;
	act(C_DEFAULT, "$n picks up some unsightly garbage.", ch, NULL, NULL, TO_ROOM );
        act(C_DEFAULT, "You pick up $p.", ch, trash, NULL, TO_CHAR );
	obj_from_room( trash );
	obj_to_char( trash, ch );
	return TRUE;
    }

    return FALSE;
}

bool spec_mayor( CHAR_DATA *ch )
{
    static const char *path;
    static const char  open_path  [ ] =
	"W3a3003b33000c111d0d111Oe333333Oe22c222112212111a1S.";
    static const char  close_path [ ] =
	"W3a3003b33000c111d0d111CE333333CE22c222112212111a1S.";
    static       int   pos;
    static       bool  move;

    if ( !move )
    {
	if ( time_info.hour ==  6 )
	{
	    path = open_path;
	    move = TRUE;
	    pos  = 0;
	}

	if ( time_info.hour == 20 )
	{
	    path = close_path;
	    move = TRUE;
	    pos  = 0;
	}
    }

    if ( !move || ch->position < POS_SLEEPING )
	return FALSE;

    switch ( path[pos] )
    {
    case '0':
    case '1':
    case '2':
    case '3':
	move_char( ch, path[pos] - '0', FALSE, FALSE );
	break;

    case 'W':
	ch->position = POS_STANDING;
	act(C_DEFAULT, "$n awakens and groans loudly.", ch, NULL, NULL, TO_ROOM );
	break;

    case 'S':
	ch->position = POS_SLEEPING;
	act(C_DEFAULT, "$n lies down and falls asleep.", ch, NULL, NULL, TO_ROOM );
	break;

    case 'a':
	act(C_DEFAULT, "$n says 'Hello Honey!'", ch, NULL, NULL, TO_ROOM );
	break;

    case 'b':
	act(C_DEFAULT, "$n says 'What a view!  I must do something about that dump!'",
	    ch, NULL, NULL, TO_ROOM );
	break;

    case 'c':
	act(C_DEFAULT, "$n says 'Vandals!  Youngsters have no respect for anything!'",
	    ch, NULL, NULL, TO_ROOM );
	break;

    case 'd':
	act(C_DEFAULT, "$n says 'Good day, citizens!'", ch, NULL, NULL, TO_ROOM );
	break;

    case 'e':
	act(C_DEFAULT, "$n says 'I hereby declare the city of Bethaven open!'",
	    ch, NULL, NULL, TO_ROOM );
	break;

    case 'E':
	act(C_DEFAULT, "$n says 'I hereby declare the city of Bethaven closed!'",
	    ch, NULL, NULL, TO_ROOM );
	break;

    case 'O':
	do_unlock( ch, "gate" );
	do_open  ( ch, "gate" );
	break;

    case 'C':
	do_close ( ch, "gate" );
	do_lock  ( ch, "gate" );
	break;

    case '.' :
	move = FALSE;
	break;
    }

    pos++;
    return FALSE;
}

bool spec_thief( CHAR_DATA *ch )
{
    CHAR_DATA *victim;
    MONEY_DATA amount;
    
    if ( ch->position == POS_FIGHTING )
	{
	if ( ch->level < 40 )
	  return FALSE;
	victim = ch->fighting;
	switch( number_range( 0, 2 ) )
	  {
	  case 0: do_circle( ch, "" ); return TRUE;
	  case 1: do_gouge( ch, "" ); return TRUE;
	  case 2: trip( ch, victim ); return TRUE;
	  }
	}
    if ( ch->position != POS_STANDING )
	return FALSE;

    for ( victim = ch->in_room->people; victim;
	 victim = victim->next_in_room )
    {
	if ( IS_NPC( victim )
	    || victim->level >= LEVEL_IMMORTAL
	    || number_bits( 3 ) != 0
	    || !can_see( ch, victim ) )	/* Thx Glop */
	    continue;

	if ( IS_AWAKE( victim ) && number_percent( ) >= ch->level )
	{
	    act(C_DEFAULT, "You discover $n's hands in your wallet!",
		ch, NULL, victim, TO_VICT );
	    act(C_DEFAULT, "$N discovers $n's hands in $S wallet!",
		ch, NULL, victim, TO_NOTVICT );
	    return TRUE;
	}
	else
	{
	    amount.gold = amount.silver = amount.copper = 0;
	    amount.gold   = ( victim->money.gold   * number_range( 1, 20 ) / 100 );
	    if ( number_percent() > 50 )
	    {
	       amount.silver = ( victim->money.silver * number_range( 1, 20 ) / 100 );
	       amount.copper = ( victim->money.copper * number_range( 1, 20 ) / 100 );
	    }
	    ch->money.gold   += 7 * amount.gold   / 8;
	    ch->money.silver += 7 * amount.silver / 8;
	    ch->money.copper += 7 * amount.copper / 8;
	    sub_money( &victim->money, &amount );
	    return TRUE;
	}
    }

    return FALSE;
}

bool spec_wanderer( CHAR_DATA *ch )
{
   CHAR_DATA *victim;
   char      *spell;
   int        sn;
    
    if ( ch->position != POS_STANDING )
	return FALSE;
    
    for ( victim = ch->in_room->people; victim;
	 victim = victim->next_in_room )
    {
	if ( IS_NPC( victim )
           && number_bits( 1 ) == 0 )
           return spec_thief( ch );
    }
           
    switch ( number_bits( 4 ) )
    {
    case 0:
    case 1: spell = "sanctuary"; break;
    case 4: spell = "haste"; break;
    case 5: spell = "invis"; break;
    case 7:
    case 8: spell = "fireshield"; break;
    case 9: spell = "chaos field"; break;
    case 10: 
    default: spell = "psychic healing"; break;
    }
       if ( ( sn = skill_lookup( spell ) ) < 0 )
  	 return FALSE;
       (*skill_table[sn].spell_fun) ( sn, ch->level, ch, ch );
         return TRUE;

    return FALSE;
}

/*
 * spec_fun to repair bashed doors by Thelonius for EnvyMud.
 */
bool spec_utility_repairman( CHAR_DATA *ch )
{
		 EXIT_DATA       *pexit;
		 EXIT_DATA       *pexit_rev;
                 ROOM_INDEX_DATA *to_room;
    extern const int              rev_dir [ ];
		 int              door;

    if ( !IS_AWAKE( ch ) )
	return FALSE;

    door = number_range( 0, 5 );
    /*
     *  Could search through all doors randomly, but deathtraps would 
     *  freeze the game!  And I'd prefer not to go through from 1 to 6...
     *  too boring.  Instead, just check one direction at a time.  There's
     *  a 51% chance they'll find the door within 4 tries anyway.
     *  -- Thelonius (Monk)
     */
    if ( !( pexit = ch->in_room->exit[door] ) )
	return FALSE;

    if ( IS_SET( pexit->exit_info, EX_BASHED ) )
    {
	REMOVE_BIT( pexit->exit_info, EX_BASHED );
	act(C_DEFAULT, "You repair the $d.", ch, NULL, pexit->keyword, TO_CHAR );
	act(C_DEFAULT, "$n repairs the $d.", ch, NULL, pexit->keyword, TO_ROOM );

	/* Don't forget the other side! */
	if (   ( to_room   = pexit->to_room               )
	    && ( pexit_rev = to_room->exit[rev_dir[door]] )
	    && pexit_rev->to_room == ch->in_room )
	{
	    CHAR_DATA *rch;

	    REMOVE_BIT( pexit_rev->exit_info, EX_BASHED );

	    for ( rch = to_room->people; rch; rch = rch->next_in_room )
		act(C_DEFAULT, "The $d is set back on its hinges.",
		    rch, NULL, pexit_rev->keyword, TO_CHAR );
	}

	return TRUE;
    }

    return FALSE;
}

/* Written by esnible@goodnet.com, Converted for PaP by Flux. */
bool spec_snake_charm( CHAR_DATA *ch )
{
    CHAR_DATA *victim;
    CHAR_DATA *v_next;

    if ( ch->position != POS_FIGHTING )
       {
       switch ( number_bits( 3 ) ) {
       case 0:
          do_order( ch, "all sing charmer" ); /* a chance to get free here */
          break;
       case 1:
          do_order( ch,
             "all chat 'The snake charmer area is pretty cool.  "
             "I'm getting a lot of experience really fast!" );
          break;
       case 2:
          do_order( ch,
             "all chat 'YES!  I just got 327xp for killing the snake charmer!");
          break;
       case 3:
          do_order( ch, "all remove dagger" );
          do_order( ch, "all give dagger charmer" );
          break;
       case 4:
          do_order( ch, "all remove sword" );
          do_order( ch, "all give sword charmer" );
          break;
       case 5:
          do_order( ch, "all remove mace" );
          do_order( ch, "all give mace charmer" );
          break;
       case 6:
          do_order( ch, "all drop all" );
          break;
       case 7:
          do_order( ch, "all cast 'heal light' charmer" );
          break;
       };

       return TRUE;
       }

    for ( victim = ch->in_room->people; victim != NULL; victim = v_next )
    {
	v_next = victim->next_in_room;
	if ( victim->fighting == ch && number_bits( 2 ) == 0 )
	    break;
    }

    if ( victim == NULL )
	return FALSE;

	act( AT_LBLUE, "$n begins playing a new, beautiful song.", ch, NULL, NULL, TO_ROOM );
    spell_charm_person(gsn_charm_person, ch->level, ch, victim );
    if (IS_AFFECTED(victim, AFF_CHARM))
       stop_fighting( victim, TRUE );

    return TRUE;
}

bool spec_fighting_caster_wizard( CHAR_DATA *ch )
{
   CHAR_DATA *victim;
   char      *spell = NULL;
   int        sn;
   int        levelswitch;
    
   if ( !(victim = ch->fighting ) )
    return TRUE;

   if ( ch->in_room != ch->fighting->in_room )
    return TRUE;

   if ( ch->level < 100 )
    levelswitch = 5;
   else
    levelswitch = 6;

   if ( ch->level < 75 )
    levelswitch = 4;
   if ( ch->level < 50 )
    levelswitch = 3;
   if ( ch->level < 25 )
    levelswitch = 2;
   if ( ch->level < 10 )
    levelswitch = 1;
   if ( ch->level < 5 )
    levelswitch = 0;

  if ( ch->fighting_style == FIGHTING_STYLE_WARLOCK )
  {
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "icy snap"; break;
      case 10: break;
      default: break;
     }
    break;

    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "icy snap"; break;
      case 4: break;
      case 5: spell = "electric snap"; break;
      case 7: break;
      case 8: spell = "fire snap"; break;
      case 9: break;
      case 10: break;
      case 11: spell = "positronic snap"; break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "fire snap"; break;
      case 4: spell = "icy snap"; break;
      case 5: spell = "electric snap"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "acidic snap"; break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: spell = "fire blast"; break;
      case 5: spell = "icy snap"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "poison"; break;
      case 10: break;
      case 11: spell = "electric snap"; break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "fire blast"; break;
      case 4: spell = "icy blast"; break;
      case 5: spell = "incinerate"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "electric blast"; break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "fire blast"; break;
      case 4: spell = "icy blast"; break;
      case 5: spell = "incinerate"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "positronic blast"; break;
      case 10: break;
      case 11: spell = "electric blast"; break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "electric blast"; break;
      case 4: spell = "weaken"; break;
      case 5: spell = "icy blast"; break;
      case 7: break;
      case 8: spell = "fire blast"; break;
      case 9: spell = "meteor swarm"; break;
      case 10: break;
      case 11: spell = "positronic blast"; break;
      default: break;
     }
    break;
   }
  }
  else
  if ( ch->fighting_style == FIGHTING_STYLE_CLERIC )
  {
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "weaken"; break;
      case 10: break;
      default: break;
     }
    break;
    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: spell = "weaken"; break;
      case 7: break;
      case 8: break;
      case 9: spell = "blindness"; break;
      case 10: break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: spell = "weaken"; break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: spell = "incinerate"; break;
      case 5: break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "poison"; break;
      case 10: break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: spell = "incinerate"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: break; 
      case 11: spell = "plasma burst"; break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "plasma burst"; break;
      case 4: spell = "poison"; break;
      case 5: spell = "incinerate"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "incinerate"; break;
      case 4: spell = "weaken"; break;
      case 5: spell = "poison"; break;
      case 7: break;
      case 8: spell = "fumble"; break;
      case 9: spell = "confusion"; break;
      case 10: break;
      case 11: spell = "blindness"; break;
      default: break;
     }
    break;
   }
  }

  if ( !(spell) )
   return FALSE;

  if ( ( sn = skill_lookup( spell ) ) < 0 )
   return FALSE;

  (*skill_table[sn].spell_fun) ( sn, ch->level, ch, victim );
 
 return TRUE;
}

bool spec_fighting_caster_cleric( CHAR_DATA *ch )
{
   CHAR_DATA *victim;
   char      *spell = NULL;
   int        sn;
   int        levelswitch;
    
   if ( !(victim = ch->fighting ) )
    return TRUE;

   if ( ch->in_room != ch->fighting->in_room )
    return TRUE;

   if ( ch->level < 100 )
    levelswitch = 5;
   else
    levelswitch = 6;

   if ( ch->level < 75 )
    levelswitch = 4;
   if ( ch->level < 50 )
    levelswitch = 3;
   if ( ch->level < 25 )
    levelswitch = 2;
   if ( ch->level < 10 )
    levelswitch = 1;
   if ( ch->level < 5 )
    levelswitch = 0;

  if ( ch->fighting_style == FIGHTING_STYLE_WARLOCK )
  {
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "icy snap"; break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: spell = "icy snap"; break;
      case 5: spell = "electric snap"; break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      case 11: spell = "curse"; break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "divine snap"; break;
      case 4: spell = "curse"; break;
      case 5: spell = "icy snap"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "curse"; break;
      case 4: spell = "icy snap"; break;
      case 5: spell = "divine blast"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "electric blast"; break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "curse"; break;
      case 4: spell = "icy blast"; break;
      case 5: spell = "divine blast"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: break;
      case 10: break;
      case 11: spell = "electric blast"; break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "electric blast"; break;
      case 4: spell = "weaken"; break;
      case 5: spell = "icy blast"; break;
      case 7: break;
      case 8: spell = "divine whisper"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "divine blast"; break;
      default: break;
     }
    break;
   }
   if ( !(spell) )
    return FALSE;

   if ( ( sn = skill_lookup( spell ) ) < 0 )
    return FALSE;

   (*skill_table[sn].spell_fun) ( sn, ch->level, ch, victim );
  }
  else
  if ( ch->fighting_style == FIGHTING_STYLE_CLERIC )
  {
   bool self = FALSE;
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "heal light"; self = TRUE; break;
      case 10: break;
      default: break;
     }
    break;
    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: spell = "heal poison"; self = TRUE; break;
      case 11: spell = "heal light"; self = TRUE; break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "heal serious"; self = TRUE; break;
      case 10: spell = "heal poison"; self = TRUE; break;
      case 11: spell = "heal light"; self = TRUE; break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: spell = "heal critical"; self = TRUE; break;
      case 9: spell = "heal serious"; self = TRUE; break;
      case 10: spell = "heal poison"; self = TRUE; break;
      case 11: spell = "heal light"; self = TRUE; break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: spell = "blindness"; break;
      case 8: spell = "heal critical"; self = TRUE; break;
      case 9: spell = "heal serious"; self = TRUE; break;
      case 10: spell = "heal poison"; self = TRUE; break;
      case 11: spell = "heal light"; self = TRUE; break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: spell = "weaken"; break;
      case 5: spell = "blindness"; break;
      case 8: spell = "heal critical"; self = TRUE; break;
      case 9: spell = "heal serious"; self = TRUE; break;
      case 10: spell = "heal poison"; self = TRUE; break;
      case 11: spell = "heal light"; self = TRUE; break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "restoration"; break;
      case 4: spell = "weaken"; break;
      case 5: spell = "blindness"; break;
      case 8: spell = "heal critical"; self = TRUE; break;
      case 9: spell = "heal serious"; self = TRUE; break;
      case 10: spell = "heal poison"; self = TRUE; break;
      case 11: spell = "heal light"; self = TRUE; break;
      default: break;
     }
    break;
   }

   if ( !(spell) )
    return FALSE;

   if ( ( sn = skill_lookup( spell ) ) < 0 )
    return FALSE;

   if ( (self) )
    (*skill_table[sn].spell_fun) ( sn, ch->level, ch, ch );
   else
    (*skill_table[sn].spell_fun) ( sn, ch->level, ch, victim );
  }
 
 return TRUE;
}

bool spec_fighting_caster_necromancer( CHAR_DATA *ch )
{
   CHAR_DATA *victim;
   char      *spell = NULL;
   int        sn;
   int        levelswitch;
    
   if ( !(victim = ch->fighting ) )
    return TRUE;

   if ( ch->in_room != ch->fighting->in_room )
    return TRUE;

   if ( ch->level < 100 )
    levelswitch = 5;
   else
    levelswitch = 6;

   if ( ch->level < 75 )
    levelswitch = 4;
   if ( ch->level < 50 )
    levelswitch = 3;
   if ( ch->level < 25 )
    levelswitch = 2;
   if ( ch->level < 10 )
    levelswitch = 1;
   if ( ch->level < 5 )
    levelswitch = 0;

  if ( ch->fighting_style == FIGHTING_STYLE_WARLOCK )
  {
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "acidic snap"; break;
      case 10: break;
      default: break;
     }
    break;
    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "acidic snap"; break;
      case 4: break;
      case 5: spell = "electric snap"; break;
      case 7: break;
      case 8: spell = "fire snap"; break;
      case 9: break;
      case 10: break;
      case 11: spell = "poison"; break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "fire snap"; break;
      case 4: spell = "acidic snap"; break;
      case 5: spell = "electric snap"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "acidic snap"; break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: spell = "fire blast"; break;
      case 5: spell = "acidic snap"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "poison"; break;
      case 10: break;
      case 11: spell = "spread disease"; break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "fire blast"; break;
      case 4: spell = "acidic blast"; break;
      case 5: spell = "incinerate"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "spread disease"; break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "fire blast"; break;
      case 4: spell = "acidic blast"; break;
      case 5: spell = "incinerate"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "spread disease"; break;
      case 10: break;
      case 11: spell = "poison"; break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "hex"; break;
      case 4: spell = "weaken"; break;
      case 5: spell = "acidic blast"; break;
      case 7: break;
      case 8: spell = "blindness"; break;
      case 9: spell = "plasma burst"; break;
      case 10: break;
      case 11: spell = "poison"; break;
      default: break;
     }
    break;
   }
  }
  else
  if ( ch->fighting_style == FIGHTING_STYLE_CLERIC )
  {
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "poison"; break;
      case 10: break;
      default: break;
     }
    break;
    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "poison"; break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: spell = "weaken"; break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      case 11: spell = "poison"; break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: spell = "incinerate"; break;
      case 5: break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: break;
      case 10: break;
      case 11: spell = "poison"; break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: spell = "incinerate"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: spell = "spread disease"; break;
      case 11: spell = "poison"; break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "hex"; break;
      case 4: spell = "spread disease"; break;
      case 5: spell = "incinerate"; break;
      case 7: break;
      case 8: spell = "weaken"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "poison"; break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "incinerate"; break;
      case 4: spell = "weaken"; break;
      case 5: spell = "poison"; break;
      case 7: break;
      case 8: spell = "hex"; break;
      case 9: spell = "spread disease"; break;
      case 10: break;
      case 11: spell = "blindness"; break;
      default: break;
     }
    break;
   }
  }

  if ( !(spell) )
   return FALSE;

  if ( ( sn = skill_lookup( spell ) ) < 0 )
   return FALSE;

  (*skill_table[sn].spell_fun) ( sn, ch->level, ch, victim );
 
 return TRUE;
}

bool spec_fighting_caster_druid( CHAR_DATA *ch )
{
   CHAR_DATA *victim;
   char      *spell = NULL;
   int        sn;
   int        levelswitch;
    
   if ( !(victim = ch->fighting ) )
    return TRUE;

   if ( ch->in_room != ch->fighting->in_room )
    return TRUE;

   if ( ch->level < 100 )
    levelswitch = 5;
   else
    levelswitch = 6;

   if ( ch->level < 75 )
    levelswitch = 4;
   if ( ch->level < 50 )
    levelswitch = 3;
   if ( ch->level < 25 )
    levelswitch = 2;
   if ( ch->level < 10 )
    levelswitch = 1;
   if ( ch->level < 5 )
    levelswitch = 0;

  if ( ch->fighting_style == FIGHTING_STYLE_WARLOCK )
  {
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "icy snap"; break;
      case 10: break;
      default: break;
     }
    break;
    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "icy snap"; break;
      case 4: break;
      case 5: spell = "electric snap"; break;
      case 7: break;
      case 8: spell = "fire snap"; break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "fire snap"; break;
      case 4: spell = "icy snap"; break;
      case 5: spell = "electric snap"; break;
      case 7: break;
      case 8: spell = "poison"; break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: spell = "fire blast"; break;
      case 5: spell = "icy snap"; break;
      case 7: break;
      case 8: break;
      case 9: spell = "poison"; break;
      case 10: break;
      case 11: spell = "electric snap"; break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "fire blast"; break;
      case 4: spell = "icy blast"; break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      case 11: spell = "electric blast"; break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "fire blast"; break;
      case 4: spell = "icy blast"; break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "strike of thorns"; break;
      case 10: break;
      case 11: spell = "electric blast"; break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "electric blast"; break;
      case 4: spell = "strike of thorns"; break;
      case 5: spell = "icy blast"; break;
      case 7: break;
      case 8: spell = "fire blast"; break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
   }
  }
  else
  if ( ch->fighting_style == FIGHTING_STYLE_CLERIC )
  {
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "faerie fire"; break;
      case 10: break;
      default: break;
     }
    break;
    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: spell = "faerie fire"; break;
      case 7: break;
      case 8: break;
      case 9: spell = "poison"; break;
      case 10: break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "faerie fire"; break;
      case 4: spell = "poison"; break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "web"; break;
      case 4: spell = "faerie fire"; break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "poison"; break;
      case 10: break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: spell = "faerie fire"; break;
      case 7: break;
      case 8: spell = "web"; break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "poison"; break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: spell = "poison"; break;
      case 5: spell = "web"; break;
      case 7: break;
      case 8: spell = "faerie fire"; break;
      case 9: spell = "incinerate"; break;
      case 10: break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "incinerate"; break;
      case 4: spell = "curse of nature"; break;
      case 5: spell = "poison"; break;
      case 7: break;
      case 8: spell = "faerie fire"; break;
      case 9: spell = "web"; break;
      case 10: break;
      case 11: spell = "blindness"; break;
      default: break;
     }
    break;
   }
  }

  if ( !(spell) )
   return FALSE;

  if ( ( sn = skill_lookup( spell ) ) < 0 )
   return FALSE;

  (*skill_table[sn].spell_fun) ( sn, ch->level, ch, victim );
 
 return TRUE;
}

bool spec_fighting_caster_illusionist( CHAR_DATA *ch )
{
   CHAR_DATA *victim;
   char      *spell = NULL;
   int        sn;
   int        levelswitch;
    
   if ( !(victim = ch->fighting ) )
    return TRUE;

   if ( ch->in_room != ch->fighting->in_room )
    return TRUE;

   if ( ch->level < 100 )
    levelswitch = 5;
   else
    levelswitch = 6;

   if ( ch->level < 75 )
    levelswitch = 4;
   if ( ch->level < 50 )
    levelswitch = 3;
   if ( ch->level < 25 )
    levelswitch = 2;
   if ( ch->level < 10 )
    levelswitch = 1;
   if ( ch->level < 5 )
    levelswitch = 0;

  if ( ch->fighting_style == FIGHTING_STYLE_WARLOCK )
  {
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "color snap"; break;
      case 10: break;
      default: break;
     }
    break;
    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "color snap"; break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      case 11: spell = "positronic snap"; break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "positronic snap"; break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "color snap"; break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: spell = "color blast"; break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      case 11: spell = "positronic snap"; break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "phantom razor"; break;
      case 4: spell = "color blast"; break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "blindness"; break;
      case 10: break;
      case 11: spell = "positronic blast"; break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "color blast"; break;
      case 4: spell = "blindness"; break;
      case 5: spell = "phantom razor"; break;
      case 7: break;
      case 8: break;
      case 9: spell = "positronic blast"; break;
      case 10: break;
      case 11: spell = "confusion"; break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "color blast"; break;
      case 4: spell = "blindness"; break;
      case 5: spell = "phantom razor"; break;
      case 7: break;
      case 8: break;
      case 9: spell = "positronic blast"; break;
      case 10: spell = "army of illusion"; break;
      case 11: spell = "confusion"; break;
      default: break;
     }
    break;
   }
  }
  else
  if ( ch->fighting_style == FIGHTING_STYLE_CLERIC )
  {
   switch( levelswitch )
   {
    case 0:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: spell = "blindness"; break;
      case 10: break;
      default: break;
     }
    break;
    case 1:
     switch ( dice( 1, 12 ) )
     {
      case 0: break; 
      case 1: break;
      case 4: break;
      case 5: spell = "blindness"; break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
    case 2:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
    case 3:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: spell = "blindness"; break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: break;
      case 9: break;
      case 10: break;
      default: break;
     }
    break;
    case 4:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: spell = "confusion"; break;
      case 7: break;
      case 8: break;
      case 9: spell = "blindness"; break;
      case 10: break;
      default: break;
     }
    break;
    case 5:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: spell = "confusion"; break;
      case 7: break;
      case 8: break;
      case 9: spell = "blindness"; break;
      case 10: break;
      default: break;
     }
    break;
    case 6:
     switch ( dice( 1, 12 ) )
     {
      case 0: break;
      case 1: break;
      case 4: break;
      case 5: break;
      case 7: break;
      case 8: spell = "fumble"; break;
      case 9: spell = "confusion"; break;
      case 10: break;
      case 11: spell = "blindness"; break;
      default: break;
     }
    break;
   }
  }

  if ( !(spell) )
   return FALSE;

  if ( ( sn = skill_lookup( spell ) ) < 0 )
   return FALSE;

  (*skill_table[sn].spell_fun) ( sn, ch->level, ch, victim );
 
 return TRUE;
}
