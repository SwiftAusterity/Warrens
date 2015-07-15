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


/*
 * External functions.
 */
bool    is_safe     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
void	set_fighting args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );

/*
 * Local functions.
 */
void	say_spell	args( ( CHAR_DATA *ch, int sn ) );
int     blood_count     args( ( OBJ_DATA *list, int amount ) ); 
void    magic_mob       args( ( CHAR_DATA *ch, OBJ_DATA *obj, int vnum ) );
int     slot_lookup     args( ( int slot ) );
int     sc_dam          args( ( CHAR_DATA *ch, int dam, int element ) );

/* For ranged spell casting */
void	ranged_spell	args( ( CHAR_DATA *ch, CHAR_DATA *victim, int sn, int dir ) );

/*
 * "Permament sn's": slot loading for objects -- Altrag
 */
int slot_lookup( int slot )
{
  int sn;

  for ( sn = 0; skill_table[sn].name[0] != '\0'; sn++ )
    if ( skill_table[sn].slot == slot )
      return sn;

  bug( "Slot_lookup: no such slot #%d", slot );
  return 0;
}

/*
 * Replacement for MAX_SKILL -- Altrag
 */
bool is_sn( int sn )
{
  int cnt;

  for ( cnt = 0; skill_table[cnt].name[0] != '\0'; cnt++ )
    if ( cnt == sn )
      return TRUE;

  return FALSE;
}

void magic_mob ( CHAR_DATA *ch, OBJ_DATA *obj, int vnum )
{
   CHAR_DATA      *victim;
   CHAR_DATA      *zombie;
   MOB_INDEX_DATA *ZombIndex;
   MOB_INDEX_DATA *pMobIndex;
   char           *name;
   char            buf [MAX_STRING_LENGTH];
 
    if ( !( pMobIndex = get_mob_index( vnum ) ) )
    {
     send_to_char(AT_BLUE, "Nothing happens.\n\r", ch);
     return;
    }

    ZombIndex = get_mob_index( 3 );
    victim = create_mobile( pMobIndex );
    zombie = create_mobile( ZombIndex );
    name = victim->short_descr;
    sprintf( buf, zombie->short_descr, name );
    free_string( zombie->short_descr );
    zombie->short_descr = str_dup(buf);
    sprintf( buf, zombie->long_descr, name );
    free_string( zombie->long_descr );
    zombie->long_descr = str_dup(buf);
    victim->perm_hit /= 2;
    victim->hit = MAX_HIT(victim);
    zombie->mod_hit = victim->mod_hit;
    zombie->perm_hit = victim->perm_hit;
    zombie->hit = victim->hit;
    zombie->level = victim->level;
    SET_BIT( zombie->act, ACT_UNDEAD );
    SET_BIT( zombie->act, ACT_PET );
    SET_BIT( zombie->affected_by, AFF_CHARM );
    char_to_room( zombie, ch->in_room );
    add_follower( zombie, ch );
    update_pos( zombie, ch );
    act( AT_BLUE, "$n passes $s hands over $p, $E slowly rises to serve $S new master.", ch, obj, zombie, TO_ROOM );
    act( AT_BLUE, "You animate $p, it rises to serve you.", ch, obj, NULL, TO_CHAR );
    char_to_room( victim, ch->in_room );
    extract_char ( victim, TRUE );
    return;
}

void update_skpell( CHAR_DATA *ch, int sn )
{

  int xp = 0;
  char buf[MAX_STRING_LENGTH];
  int adept;

  if ( IS_NPC( ch ) )
    return;

  adept = IS_NPC( ch ) ? 100 :
	  class_table[prime_class( ch )].skill_adept;
  
  if ( ch->pcdata->learned[sn] <= 0 
    || ch->pcdata->learned[sn] >= adept )
      return;

  ch->pcdata->learned[sn] += ( get_curr_int( ch ) / 5 );

  if ( ch->pcdata->learned[sn] > adept )
    ch->pcdata->learned[sn] = adept;

   xp = 15;
   if (!(ch->fighting))
   { 
     sprintf( buf, "You gain %d experience for your success with %s.\n\r",
      	      xp, skill_table[sn].name );
     send_to_char( C_DEFAULT, buf, ch );
   }  
  gain_exp( ch, xp );
  
   return;
}

int skill_lookup( const char *name )
{
    int sn;

    for ( sn = 0; skill_table[sn].name[0] != '\0'; sn++ )
    {
	if ( !skill_table[sn].name )
	    break;
	if ( LOWER( name[0] ) == LOWER( skill_table[sn].name[0] )
	    && !str_prefix( name, skill_table[sn].name ) )
	    return sn;
    }

    return -1;
}



/*
 * Utter mystical words for an sn.
 */
void say_spell( CHAR_DATA *ch, int sn )
{
                        CHAR_DATA *rch;
			char      *pName;
			char       buf       [ MAX_STRING_LENGTH ];
			char       buf2      [ MAX_STRING_LENGTH ];
			int        iSyl;
			int        length;

	       	 struct syl_type
	         {
		        char *	   old;
		        char *	   new;
		 };

    static const struct syl_type   syl_table [ ] =
    {
	{ " ",		" "		},
	{ "ar",		"abra"		},
	{ "au",		"kada"		},
	{ "bless",	"fido"		},
	{ "blind",	"nose"		},
	{ "bur",	"mosa"		},
	{ "cu",		"judi"		},
	{ "de",		"oculo"		},
	{ "en",		"unso"		},
	{ "light",	"dies"		},
	{ "lo",		"hi"		},
	{ "mor",	"zak"		},
	{ "move",	"sido"		},
	{ "ness",	"lacri"		},
	{ "ning",	"illa"		},
	{ "per",	"duda"		},
	{ "ra",		"gru"		},
	{ "re",		"candus"	},
	{ "son",	"sabru"		},
	{ "tect",	"infra"		},
	{ "tri",	"cula"		},
	{ "ven",	"nofo"		},
	{ "a", "a" }, { "b", "b" }, { "c", "q" }, { "d", "e" },
	{ "e", "z" }, { "f", "y" }, { "g", "o" }, { "h", "p" },
	{ "i", "u" }, { "j", "y" }, { "k", "t" }, { "l", "r" },
	{ "m", "w" }, { "n", "i" }, { "o", "a" }, { "p", "s" },
	{ "q", "d" }, { "r", "f" }, { "s", "g" }, { "t", "h" },
	{ "u", "j" }, { "v", "z" }, { "w", "x" }, { "x", "n" },
	{ "y", "l" }, { "z", "k" },
	{ "", "" }
    };

    buf[0]	= '\0';
    for ( pName = skill_table[sn].name; *pName != '\0'; pName += length )
    {
	for ( iSyl = 0;
	     ( length = strlen( syl_table[iSyl].old ) ) != 0;
	     iSyl++ )
	{
	    if ( !str_prefix( syl_table[iSyl].old, pName ) )
	    {
		strcat( buf, syl_table[iSyl].new );
		break;
	    }
	}

	if ( length == 0 )
	    length = 1;
    }
/*
    if ( IS_NPC(ch) )
    {
    sprintf( buf2, "$n utters the words, '%s'.", buf );
    sprintf( buf,  "$n utters the words, '%s'.", skill_table[sn].name );
    }
    else
    {
     if ( IS_AFFECTED( ch, AFF_INVISIBLE ) || IS_SET( ch->act, PLR_WIZINVIS ) )
     {
    sprintf( buf2, "Someone %s the words, '%s'.",
	     "utters", buf );
    sprintf( buf,  "Someone %s the words, '%s'.",
	     "utters", skill_table[sn].name );
     }
     else 
     {*/
    sprintf( buf2, "$n utters the words, '%s'.", 
	     buf );
    sprintf( buf,  "$n utters the words, '%s'.", 
	     skill_table[sn].name );
/*     }
    }*/
    for ( rch = ch->in_room->people; rch; rch = rch->next_in_room )
    {
	if ( rch != ch )
	    act(AT_BLUE, 
	    is_class( rch, prime_class(ch) )
	    ? buf 
	    : buf2, ch, NULL, rch, TO_VICT );
    }

    return;
}



/*
 * Compute a saving throw.
 * Negative apply's make saving throw better.
 */
bool saves_spell( int level, CHAR_DATA *victim )
{
    int savebase = 0;
    
    if ( IS_NPC( victim ) )
     savebase = victim->pIndexData->m_damp + (victim->level / 4);

    if ( !IS_NPC( victim ) )
     savebase = victim->m_damp;

  return number_percent( ) < savebase;
}



/*
 * The kludgy global is for spells who want more stuff from command line.
 */
char *target_name;

void do_acspell ( CHAR_DATA *ch, OBJ_DATA *pObj, char *argument )
{
    void      *vo;
    OBJ_DATA  *obj = NULL;
    CHAR_DATA *victim;
    char       arg1 [ MAX_INPUT_LENGTH ];
    char       arg2 [ MAX_INPUT_LENGTH ];
    int        sn;
    int        spec;
 
    spec = skill_lookup( "astral walk" );
    target_name = one_argument( argument, arg1 );
    one_argument( target_name, arg2 );

    if ( IS_NPC( ch ) )
      if ( IS_SET( ch->affected_by, AFF_CHARM ) )
        return;

    if ( ( sn = skill_lookup( arg1 ) ) < 0) 
    {
	send_to_char(AT_BLUE, "You can't do that.\n\r", ch );
	return;
    }
  
    if ( ( sn == spec )  && ( is_name( ch, arg2, ch->name ) ) )
       {
         send_to_char( AT_BLUE, "You are already in the same room as yourself.\n\r", ch );
         return;
       }

    /*
     * Locate targets.
     */
    victim	= NULL;
    obj		= NULL;
    vo		= NULL;
      
    switch ( skill_table[sn].target )
    {
    default:
	bug( "Do_cast: bad target for sn %d.", sn );
	return;

    case TAR_GROUP_OFFENSIVE:
    case TAR_GROUP_DEFENSIVE:
    case TAR_GROUP_ALL:
    case TAR_GROUP_OBJ:
    case TAR_GROUP_IGNORE:
	group_cast( sn, URANGE( 1, ch->level, LEVEL_HERO ), ch, arg2 );
	return;

    case TAR_IGNORE:
	break;

    case TAR_CHAR_OFFENSIVE:
    if ( IS_SET( ch->in_room->room_flags, ROOM_NO_OFFENSIVE ) )
      {
       send_to_char( AT_BLUE, "You failed.\n\r", ch );
       return;
      }	   
    	if ( arg2[0] == '\0' )
	{
	    if ( !( victim = ch->fighting ) )
	    {
		send_to_char(AT_BLUE, "Cast the spell on whom?\n\r", ch );
		return;
	    }
	}
	else
	{
	    if ( !( victim = get_char_room( ch, arg2 ) ) )
	    {
		send_to_char(AT_BLUE, "They aren't here.\n\r", ch );
		return;
	    }
	}

    if ( IS_AFFECTED(victim, AFF_PEACE) )
    {
      send_to_char(AT_WHITE, "A wave of peace overcomes you.\n\r", ch);
      return;
    }
    if ( IS_AFFECTED( ch, AFF_PEACE) )
    {
	    affect_strip( ch, skill_lookup("aura of peace") );
	    REMOVE_BIT( ch->affected_by, AFF_PEACE );
    }
	if ( is_safe(ch, victim) )
	{
	  send_to_char(AT_BLUE,"You failed.\n\r",ch);
	  return;
	}
    if ( IS_AFFECTED2( victim, AFF_MMIRROR) )
    {
     if ( !IS_NPC(victim) && number_percent( ) > 85 )
     {
      send_to_char(AT_BLOOD,"Your spell is reflected back to you !\n\r",ch);
      send_to_char(AT_YELLOW,"You reflect their spell!\n\r", victim);
      vo = (void *) ch;
     }
     else
	vo = (void *) victim;
    }    
    else
	vo = (void *) victim;
    break;

    case TAR_CHAR_DEFENSIVE:
	if ( arg2[0] == '\0' )
	{
	    victim = ch;
	}
	else
	{
	    if ( !( victim = get_char_room( ch, arg2 ) ) )
	    {
		send_to_char(AT_BLUE, "They aren't here.\n\r", ch );
		return;
	    }
	}

	vo = (void *) victim;
	break;

    case TAR_CHAR_SELF:
	if ( arg2[0] != '\0' && !is_name( ch, arg2, ch->name ) )
	{
	    send_to_char(AT_BLUE, "You cannot cast this spell on another.\n\r", ch );
	    return;
	}

	vo = (void *) ch;
	break;

    case TAR_OBJ_INV:
	if ( arg2[0] == '\0' )
	{
	    send_to_char(AT_BLUE, "What should the spell be cast upon?\n\r", ch );
	    return;
	}

	if ( !( obj = get_obj_carry( ch, arg2 ) ) )
	{
	    send_to_char(AT_BLUE, "You are not carrying that.\n\r", ch );
	    return;
	}

	vo = (void *) obj;
	break;
    }


      
   if ( IS_NPC(ch) )
   {
    if ( ch->pIndexData->vnum != MOB_VNUM_SUPERMOB )
     WAIT_STATE( ch, skill_table[sn].beats );
   }
   else
     WAIT_STATE( ch, skill_table[sn].beats );

    if ( IS_SET( ch->in_room->room_flags, ROOM_NO_MAGIC ) )
      {
       send_to_char( AT_BLUE, "You failed.\n\r", ch );
       return;
      }	   
      if ( !IS_NPC( ch ) )
      update_skpell( ch, sn );
    (*skill_table[sn].spell_fun) ( sn, URANGE( 1, ch->level, LEVEL_HERO ),
				   ch, vo );

    if ( vo )
    {
      oprog_invoke_trigger( pObj, ch, vo );
      if ( skill_table[sn].target == TAR_OBJ_INV )
	oprog_cast_sn_trigger( obj, ch, sn, vo );
      rprog_cast_sn_trigger( ch->in_room, ch, sn, vo );
    }

    if ( skill_table[sn].target == TAR_CHAR_OFFENSIVE
	&& victim->master != ch && victim != ch && IS_AWAKE( victim ) )

    {
	CHAR_DATA *vch;

	for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
	{
	    if ( vch->deleted )
	        continue;
	    if ( victim == vch && !victim->fighting )
	    {
		multi_hit( victim, ch, TYPE_UNDEFINED );
		break;
	    }
	}
    }

    return;
}

void do_touch( CHAR_DATA *ch, char *argument )
{
    void      *vo;
    OBJ_DATA  *obj;
    CHAR_DATA *victim;
    AFFECT_DATA af;
    char       arg1 [ MAX_INPUT_LENGTH ];
    char       arg2 [ MAX_INPUT_LENGTH ];
    int        mana;
    int        sn = 0;
    bool	IS_DIVINE;

    IS_DIVINE = FALSE;

    target_name = one_argument( argument, arg1 );

    if ( arg1[0] != '\0' )
     if ( !str_prefix( arg1, "divine" ) && ch->level >= LEVEL_IMMORTAL )
     {
       IS_DIVINE = TRUE;
       target_name = one_argument( target_name, arg1 );
     }

    one_argument( target_name, arg2 );

    if ( time_info.phase_white == 0
      && time_info.phase_shadow == 0
      && time_info.phase_blood == 0 )
    {
     send_to_char(AT_BLUE, "The eclipse fizzles your invokation.\n\r", ch );
     return;
    }


    if ( arg1[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Touch what where?\n\r", ch );
	return;
    }

    if ( IS_NPC( ch ) )
      if ( IS_SET( ch->affected_by, AFF_CHARM ) )
        return;

    if ( !IS_NPC( ch ) )
    if ( ( sn = skill_lookup( arg1 ) ) < 0
	|| !can_use_skpell( ch, sn ) )
    {
	send_to_char(AT_BLUE, "You can't do that.\n\r", ch );
	return;
    }
  
    if ( IS_NPC( ch ) )
     if ( ( sn = skill_lookup( arg1 ) ) < 0 )
       return;
  
    if ( ch->position < skill_table[sn].minimum_position )
    {
	send_to_char(AT_BLUE, "You can't concentrate enough.\n\r", ch );
	return;
    }

    if ( skill_table[sn].touch_fun == touch_null )
    {
     send_to_char(AT_BLUE, "That isn't an invokation.\n\r", ch );
     return;
    }

    if ( skill_table[sn].touch_fun == skill_table[sn].spell_fun )
    {
     if ( !IS_NPC(ch) )
     {
      if ( is_class( ch, CLASS_HERETIC ) )
      { 
       send_to_char(AT_BLUE, "You're not heretic, you cast spells, you don't invoke them.\n\r", 
       ch );
       return;
      }
     }
    }

    if ( !IS_NPC( ch ) )
    {
    mana = SPELL_COST( ch, sn );
    if ( ch->race == RACE_ELF || ch->race == RACE_AQUINIS )
       mana -= mana / 4;
    }
    else
    mana = 0;
    /*
     * Locate targets.
     */
    victim	= NULL;
    obj		= NULL;
    vo		= NULL;
      
    switch ( skill_table[sn].target )
    {
    default:
	bug( "Do_touch: bad target for sn %d.", sn );
	return;

    case TAR_GROUP_OFFENSIVE:
    case TAR_GROUP_DEFENSIVE:
    case TAR_GROUP_ALL:
    case TAR_GROUP_OBJ:
    case TAR_GROUP_IGNORE:
	group_cast( sn, URANGE( 1, ch->level, LEVEL_HERO ), ch, arg2 );
	return;

    case TAR_IGNORE:
	break;

    case TAR_CHAR_OFFENSIVE:
	if ( IS_SET( ch->in_room->room_flags, ROOM_NO_OFFENSIVE ) )
	{
		send_to_char( AT_BLUE, "You failed.\n\r", ch );
		return;
	}

	if ( arg2[0] == '\0' )
	{
	    if ( !( victim = ch->fighting ) )
	    {
		send_to_char(AT_BLUE, "Touch whom?\n\r", ch );
		return;
	    }
	}
	else
	{
	    if ( !( victim = get_char_room( ch, arg2 ) ) )
	    {
		send_to_char(AT_BLUE, "They aren't here.\n\r", ch );
		return;
	    }
	}

    if ( IS_AFFECTED(victim, AFF_PEACE) )
    {
      send_to_char(AT_WHITE, "A wave of peace overcomes you.\n\r", ch);
      return;
    }

    if ( IS_AFFECTED( ch, AFF_PEACE) )
    {
     affect_strip( ch, skill_lookup("aura of peace") );
     REMOVE_BIT( ch->affected_by, AFF_PEACE );
    }

   /* if (is_safe(ch, victim ) )
    {
     send_to_char( AT_BLUE, "You failed.\n\r",ch);
     return;
    }*/

     vo = (void *) victim;
    break;

    case TAR_CHAR_DEFENSIVE:
	if ( arg2[0] == '\0' )
	{
	    victim = ch;
	}
	else
	{
	    if ( !( victim = get_char_room( ch, arg2 ) ) )
	    {
		send_to_char(AT_BLUE, "They aren't here.\n\r", ch );
		return;
	    }
	}

	vo = (void *) victim;
	break;

    case TAR_CHAR_SELF:
	if ( arg2[0] != '\0' && !is_name( ch, arg2, ch->name ) )
	{
	    send_to_char(AT_BLUE, "This was not meant for others.\n\r", ch );
	    return;
	}

	vo = (void *) ch;
	break;

    case TAR_OBJ_INV:
	if ( arg2[0] == '\0' )
	{
	    send_to_char(AT_BLUE, "What should be touched?\n\r", ch );
	    return;
	}

        if ( !(obj = get_obj_here( ch, arg2 ) ) )
        {
	  send_to_char( AT_BLUE, "You can't find that.\n\r", ch );
          return;
        }
	vo = (void *) obj;
	break;
    }
    if ( !IS_NPC( ch ) )
    if ( ch->mana < mana )
       {
   	send_to_char(AT_BLUE, "You don't have enough mana.\n\r", ch );
	return;
       }
         

    if ( ( IS_SET( ch->in_room->room_flags, ROOM_NO_OFFENSIVE ) ) && ( skill_table[sn].target == TAR_CHAR_OFFENSIVE ) )
      {
       send_to_char( AT_BLUE, "You failed.\n\r", ch );
       return;
      }	   
    WAIT_STATE( ch, skill_table[sn].beats );

      if ( IS_AFFECTED2( ch, AFF_GRASPING) )
	{
	 send_to_char(AT_BLUE, "You can't invoke while grasping.\n\r", ch );
       return;
      }
      
   if ( !IS_NPC( ch ) )
   {
    if ( number_percent( ) > (ch->pcdata->learned[sn] + (get_curr_wis(ch) / 5 ) ) )
    {
	send_to_char(AT_BLUE, "You lost your concentration.\n\r", ch );
	MT( ch ) -= mana / 2;
    }
    else
    {
	MT( ch ) -= mana;
	if ( ( IS_AFFECTED2( ch, AFF_CONFUSED ) )
	    && number_percent( ) < 10 )
	{
	   act(AT_YELLOW, "$n looks around confused at what's going on.", ch, NULL, NULL, TO_ROOM );
	   send_to_char( AT_YELLOW, "You become confused and botch the invokation.\n\r", ch );
	   return;
	} 

       if ( skill_table[sn].grasp == TRUE && !IS_AFFECTED2( ch, AFF_GRASPING)
         && !IS_AFFECTED2( ch, AFF_GRASPED ) && victim->fighting != ch )
       {
	    af.type      = sn;
	    af.level     = ch->level;
	    af.duration  = 1;
	    af.location  = APPLY_NONE;
	    af.modifier  = 0;
	    af.bitvector = AFF_GRASPING;
	    affect_to_char2( ch, &af );

	    af.type      = sn;
	    af.level     = ch->level;
	    af.duration  = 1;
	    af.location  = APPLY_NONE;
	    af.modifier  = 0;
	    af.bitvector = AFF_GRASPED;
	    affect_to_char2( victim, &af );
      }
	update_skpell( ch, sn );   
	(*skill_table[sn].touch_fun) ( sn,
				      IS_DIVINE ?
				      URANGE( 1, ch->level, LEVEL_HERO )*3 :
				      URANGE( 1, ch->level, LEVEL_HERO ),
				      ch, vo );


    if ( IS_NPC( ch ) )
    return;
 
    if ( vo )
    {
      if ( skill_table[sn].target == TAR_OBJ_INV )
	oprog_cast_sn_trigger( obj, ch, sn, vo );
      rprog_cast_sn_trigger( ch->in_room, ch, sn, vo );
    }

    if ( skill_table[sn].target == TAR_CHAR_OFFENSIVE
	&& victim->master != ch && victim != ch && IS_AWAKE( victim ) )
    {
	CHAR_DATA *vch;

	for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
	{
	    if ( vch->deleted )
	        continue;
	    if ( victim == vch && !victim->fighting )
	    {
		multi_hit( victim, ch, TYPE_UNDEFINED );
		break;
	    }
	}
    }
  }
 }
    return;
}

void do_cast( CHAR_DATA *ch, char *argument )
{
    void      *vo;
    OBJ_DATA  *obj;
    EXIT_DATA *exit;
    CHAR_DATA *victim;
    char       arg1 [ MAX_INPUT_LENGTH ];
    char       arg2 [ MAX_INPUT_LENGTH ];
    char       arg3 [ MAX_INPUT_LENGTH ];
    int        mana;
    int        sn = 0;
    int		dir = 0;
    bool	IS_DIVINE;
    bool	ranged = FALSE;

    IS_DIVINE = FALSE;

    target_name = one_argument( argument, arg1 );

    if ( arg1[0] != '\0' )
    {
     if ( !str_prefix( arg1, "divine" ) && ch->level >= LEVEL_IMMORTAL )
     {
       IS_DIVINE = TRUE;
       target_name = one_argument( target_name, arg1 );
     }
    }

    target_name = one_argument( target_name, arg2 );
    target_name = one_argument( target_name, arg3 );

    if ( time_info.phase_white == 0
      && time_info.phase_shadow == 0
      && time_info.phase_blood == 0 )
    {
	send_to_char(AT_BLUE, "The eclipse fizzles your spell.\n\r", ch );
	return;
    }


    if ( arg1[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Cast which what where?\n\r", ch );
	return;
    }

    if ( IS_NPC( ch ) )
      if ( IS_SET( ch->affected_by, AFF_CHARM ) )
        return;

    if ( !IS_NPC( ch ) )
    if ( ( sn = skill_lookup( arg1 ) ) < 0
	|| !can_use_skpell( ch, sn ) )
    {
	send_to_char(AT_BLUE, "You can't do that.\n\r", ch );
	return;
    }
  
    if ( IS_NPC( ch ) )
     if ( ( sn = skill_lookup( arg1 ) ) < 0 )
       return;
  
    if ( ch->position < skill_table[sn].minimum_position )
    {
	send_to_char(AT_BLUE, "You can't concentrate enough.\n\r", ch );
	return;
    }

    if ( skill_table[sn].spell_fun == spell_null )
    {
     send_to_char(AT_BLUE, "That isn't a spell.\n\r", ch );
     return;
    }

    if ( IS_STUNNED( ch, STUN_MAGIC ) )
    {
      send_to_char(AT_LBLUE, "You're too stunned to cast spells.\n\r", ch );
      return;
    }

    if ( !IS_NPC( ch ) )
    {
    mana = SPELL_COST( ch, sn );
    if ( ch->race == RACE_ELF || ch->race == RACE_AQUINIS )
       mana -= mana / 4;
    }
    else
    mana = 0;
    /*
     * Locate targets.
     */
    victim	= NULL;
    obj		= NULL;
    exit	= NULL;
    vo		= NULL;
      
    switch ( skill_table[sn].target )
    {
    default:
	bug( "Do_cast: bad target for sn %d.", sn );
	return;

    case TAR_GROUP_OFFENSIVE:
    case TAR_GROUP_DEFENSIVE:
    case TAR_GROUP_ALL:
    case TAR_GROUP_OBJ:
    case TAR_GROUP_IGNORE:
	group_cast( sn, URANGE( 1, ch->level, LEVEL_HERO ), ch, arg2 );
	return;

    case TAR_IGNORE:
	break;

    case TAR_CHAR_OFFENSIVE:
	if ( IS_SET( ch->in_room->room_flags, ROOM_NO_OFFENSIVE ) )
	{
		send_to_char( AT_BLUE, "You failed.\n\r", ch );
		return;
	}

     if ( skill_table[sn].range == 0 )
     {
	if ( arg2[0] == '\0' )
	{
	    if ( !( victim = ch->fighting ) )
	    {
		send_to_char(AT_BLUE, "Cast the spell on whom?\n\r", ch );
		return;
	    }
	}
	else
	{
	    if ( !( victim = get_char_room( ch, arg2 ) ) )
	    {
		send_to_char(AT_BLUE, "They aren't here.\n\r", ch );
		return;
	    }
	}
     }
     else
     {
      if ( arg2[0] == '\0' )
      {
       send_to_char(AT_BLUE,
        "What direction do you intend to cast the spell?\n\r", ch );
       return;
      }

       for ( dir = 0; dir < 6; dir++ )
	  if ( arg2[0] == dir_name[dir][0] && !str_prefix( arg2,
							 dir_name[dir] ) )
           break;

       if ( dir == 6 )
       {
	  send_to_char( C_DEFAULT, "That isn't a proper direction.\n\r", ch );
	  return;
       }

      if ( ( ch->in_room->exit[dir] ) == NULL ||
	   (ch->in_room->exit[dir]->to_room ) == NULL )
      {
       send_to_char( C_DEFAULT,
        "You cannot throw spells in that direction.\n\r", ch );
	return;
      }

      if ( exit_blocked(ch->in_room->exit[dir], ch->in_room) > EXIT_STATUS_OPEN )
      {
       send_to_char( C_DEFAULT,
        "You cannot throw spells through a barrier.\n\r", ch );
       return;
      }


      if ( arg3[0] == '\0' )
      {
       send_to_char(AT_BLUE,
        "Whom did you intend to throw the spell at?\n\r", ch );
       return;
      }

       ranged = TRUE;

       if ( !( victim = get_char_closest( ch, dir, arg3 ) ) )
       { 
        send_to_char(AT_BLUE, "That target isn't anywhere near you.\n\r", ch );
        return;
       }

       if ( ch->in_room == victim->in_room )
       { 
        send_to_char(AT_BLUE, "This is a ranged-only spell.\n\r", ch );
        return;
       }
      }

    if ( IS_AFFECTED(victim, AFF_PEACE) )
    {
      send_to_char(AT_WHITE, "A wave of peace overcomes you.\n\r", ch);
      return;
    }

    if ( IS_AFFECTED( ch, AFF_PEACE) )
    {
     affect_strip( ch, skill_lookup("aura of peace") );
     REMOVE_BIT( ch->affected_by, AFF_PEACE );
    }

    if (is_safe(ch, victim ) )
    {
     send_to_char( AT_BLUE, "You failed.\n\r",ch);
     return;
    }

    if ( IS_AFFECTED2( victim, AFF_MMIRROR) && !ranged )
    {
     if ( !IS_NPC(victim) && number_percent( ) > 85 )
     {
      send_to_char(AT_BLOOD,"Your spell is reflected back to you !\n\r",ch);
      send_to_char(AT_YELLOW,"You reflect their spell!\n\r", victim);
      vo = (void *) ch;
     }
     else
	vo = (void *) victim;
    }    
    else
	vo = (void *) victim;
    break;

    case TAR_CHAR_DEFENSIVE:
	if ( arg2[0] == '\0' )
	    victim = ch;
	else
	{
	    if ( !( victim = get_char_room( ch, arg2 ) ) )
	    {
		send_to_char(AT_BLUE, "They aren't here.\n\r", ch );
		return;
	    }
	}

	vo = (void *) victim;
	break;

    case TAR_CHAR_SELF:
	if ( arg2[0] != '\0' && !is_name( ch, arg2, ch->name ) )
	{
	    send_to_char(AT_BLUE, "You cannot cast this spell on another.\n\r", ch );
	    return;
	}

	vo = (void *) ch;
	break;

    case TAR_EXIT:
     if ( arg2[0] == '\0' )
     {
      send_to_char(AT_BLUE, "What should the spell be cast upon?\n\r", ch );
      return;
     }
     
     if ( !(exit = (ch->in_room->exit[flag_value(direction_flags, arg2)])))
     {
      send_to_char( AT_BLUE, "That isn't a valid exit.\n\r", ch );
      return;
     }

     vo = (void *) exit;
     break;

    case TAR_OBJ_INV:
	if ( arg2[0] == '\0' )
	{
	    send_to_char(AT_BLUE, "What should the spell be cast upon?\n\r", ch );
	    return;
	}

        if ( !(obj = get_obj_here( ch, arg2 ) ) )
        {
	  send_to_char( AT_BLUE, "You can't find that.\n\r", ch );
          return;
        }
	vo = (void *) obj;
	break;
    }

    if ( !IS_NPC( ch ) )
     if ( ch->mana < mana )
       {
   	send_to_char(AT_BLUE, "You don't have enough mana.\n\r", ch );
	return;
       }         

    if ( str_cmp( skill_table[sn].name, "ventriloquate" ) )
	say_spell( ch, sn );
      
    if ( IS_SET( ch->in_room->room_flags, ROOM_NO_MAGIC ) )
      {
       send_to_char( AT_BLUE, "You failed.\n\r", ch );
       return;
      }	   

    if ( ( IS_SET( ch->in_room->room_flags, ROOM_NO_OFFENSIVE ) ) 
      && ( skill_table[sn].target == TAR_CHAR_OFFENSIVE ) )
      {
       send_to_char( AT_BLUE, "You failed.\n\r", ch );
       return;
      }	   

   if ( IS_NPC(ch) && ch->pIndexData->vnum == MOB_VNUM_SUPERMOB )
    ;
   else
    WAIT_STATE( ch, skill_table[sn].beats );
      
   if ( !IS_NPC( ch ) )
   {
    if ( number_percent( ) > (ch->pcdata->learned[sn] + (get_curr_wis(ch) / 5 ) ) )
    {
	send_to_char(AT_BLUE, "You lost your concentration.\n\r", ch );
	MT( ch ) -= mana / 2;
    }
    else
    {
	MT( ch ) -= mana;
	if ( ( IS_AFFECTED2( ch, AFF_CONFUSED ) )
	    && number_percent( ) < 10 )
	{
	   act(AT_YELLOW, "$n looks around confused at what's going on.", ch, NULL, NULL, TO_ROOM );
	   send_to_char( AT_YELLOW, "You become confused and botch the spell.\n\r", ch );
	   return;
	} 
	update_skpell( ch, sn );

      if ( ranged )
       ranged_spell( ch, victim, sn, dir );
      else
	 (*skill_table[sn].spell_fun) ( sn,
				      IS_DIVINE ?
				      URANGE( 1, ch->level, LEVEL_HERO )*3 :
				      URANGE( 1, ch->level, LEVEL_HERO ) ,
				      ch, vo );
    }
   }

    if ( IS_NPC( ch ) )
    {
      if ( ranged )
       ranged_spell( ch, victim, sn, dir );
      else
      (*skill_table[sn].spell_fun) ( sn,
				      IS_DIVINE ?
				      URANGE( 1, ch->level, LEVEL_HERO )*3 :
				      URANGE( 1, ch->level, LEVEL_HERO ) ,
            			      ch, vo );
    } 

   if ( vo )
    {
      if ( skill_table[sn].target == TAR_OBJ_INV )
	oprog_cast_sn_trigger( obj, ch, sn, vo );
      rprog_cast_sn_trigger( ch->in_room, ch, sn, vo );
    }

    mprog_spell_trigger( ch, sn );

    if ( skill_table[sn].target == TAR_CHAR_OFFENSIVE
	&& victim->master != ch && victim != ch && IS_AWAKE( victim ) )
    {
	CHAR_DATA *vch;

     if ( IS_AFFECTED2( victim, AFF_MANANET ) && victim != ch )
     {
      act(AT_YELLOW, "$n's mana net glows and shimmers.",victim, NULL,
       NULL, TO_ROOM );
      send_to_char( AT_YELLOW, "Your mana net absorbs some of the spell's energies.\n\r", victim );
      victim->mana += ( mana / 2 );
     }

       if ( IS_NPC(ch) && ch->pIndexData->vnum == MOB_VNUM_SUPERMOB )
        ;
       else
	for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
	{
	    if ( vch->deleted )
	        continue;
	    if ( victim == vch && !victim->fighting )
	    {
		set_fighting( victim, ch );
		break;
	    }
	}
    }

    return;
}



/*
 * Cast spells at targets using a magical object.
 */
void obj_cast_spell( int sn, int level, CHAR_DATA *ch, CHAR_DATA *victim,
		    OBJ_DATA *obj )
{
    void *vo;

    if ( sn <= 0 )
	return;
    
    if ( !is_sn(sn) || skill_table[sn].spell_fun == 0 )
    {
	bug( "Obj_cast_spell: bad sn %d.", sn );
	return;
    }

    if ( IS_SET( ch->in_room->room_flags, ROOM_NO_MAGIC ) )
      {
       send_to_char( AT_BLUE, "The magic of the item fizzles.\n\r", ch );
       return;
      }	   

    switch ( skill_table[sn].target )
    {
    default:
	bug( "Obj_cast_spell: bad target for sn %d.", sn );
	return;

    case TAR_GROUP_OFFENSIVE:
    case TAR_GROUP_DEFENSIVE:
    case TAR_GROUP_ALL:
    case TAR_GROUP_OBJ:
    case TAR_GROUP_IGNORE:
	group_cast( sn, URANGE( 1, level, LEVEL_HERO ), ch,
		    victim ? (void *)victim : (void *)obj );
	return;

    case TAR_IGNORE:
	vo = NULL;
	break;

    case TAR_CHAR_OFFENSIVE:
    if ( IS_SET( ch->in_room->room_flags, ROOM_NO_OFFENSIVE ) )
      {
       send_to_char( AT_BLUE, "The magic of the item fizzles.\n\r", ch );
       return;
      }	   
	if ( !victim )
	    victim = ch->fighting;
	if ( !victim || ( !IS_NPC( victim ) && ch != victim ) )
	{
	    send_to_char(AT_BLUE, "You can't do that.\n\r", ch );
	    return;
	}
	if ( ( ( ch->clan == 0 ) || ( ch->clan == 0 ) ) && ( !IS_NPC( victim ) ) )
 	   return;
    if ( IS_AFFECTED(victim, AFF_PEACE) )
    {
      send_to_char(AT_WHITE, "A wave of peace overcomes you.\n\r", ch);
      return;
    }
    if ( IS_AFFECTED( ch, AFF_PEACE) )
    {
	    affect_strip( ch, skill_lookup("aura of peace") );
	    REMOVE_BIT( ch->affected_by, AFF_PEACE );
    }
    if ( ( ( ch->level - 9 > victim->level ) 
      || ( ch->level + 9 < victim->level ) )
      && ( !IS_NPC(victim) ) )
    {
	send_to_char(AT_WHITE, "That is not in the pkill range... valid range is +/- 8 levels.\n\r", ch );
	return;
    }
	vo = (void *) victim;
	break;

    case TAR_CHAR_DEFENSIVE:
	if ( !victim )
	    victim = ch;
	vo = (void *) victim;
	break;

    case TAR_CHAR_SELF:
	vo = (void *) ch;
	break;

    case TAR_OBJ_INV:
	if ( !obj )
	{
	    send_to_char(AT_BLUE, "You can't do that.\n\r", ch );
	    return;
	}
	vo = (void *) obj;
	break;
    }
    (*skill_table[sn].spell_fun) ( sn, level, ch, vo );

    if ( vo )
    {
      if ( skill_table[sn].target == TAR_OBJ_INV )
	oprog_cast_sn_trigger( obj, ch, sn, vo );
      rprog_cast_sn_trigger( ch->in_room, ch, sn, vo );
    }

    if ( skill_table[sn].target == TAR_CHAR_OFFENSIVE
	&& victim->master != ch && ch != victim ) 
    {
	CHAR_DATA *vch;

	for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
	{
	    if ( vch->deleted )
	        continue;
	    if ( victim == vch && !victim->fighting )
	    {
		multi_hit( victim, ch, TYPE_UNDEFINED );
		break;
	    }
	}
    }

    return;
}

void ranged_spell( CHAR_DATA *ch, CHAR_DATA *victim, int sn, int dir )
{
 OBJ_DATA        *ball;
 char            buf[MAX_STRING_LENGTH];
 void            *vo;
 ROOM_INDEX_DATA *to_room = ch->in_room;
 EXIT_DATA       *pexit;
 int             dist = 0;
 int             MAX_DIST = 0;
 extern char     *dir_noun []; 
 
 vo = (void *) victim;

 sprintf( buf, "%s", skill_table[sn].name );

 ball = create_object( get_obj_index( 1 ), 1 );

 free_string( ball->short_descr );
 ball->short_descr = str_dup(buf);

 obj_to_room( ball, ch->in_room );

      if ( ( pexit = to_room->exit[dir] ) == NULL ||
 	   ( to_room = pexit->to_room ) == NULL )
       return;

     MAX_DIST = skill_table[sn].range;
     for ( dist = 1; dist <= MAX_DIST; dist++ )
     {
	obj_from_room( ball );
	obj_to_room( ball, to_room );

	if ( target_available( ch, victim, ball ) )
	 break;

	if ( ( pexit = to_room->exit[dir] ) == NULL ||
	     ( to_room = pexit->to_room ) == NULL ||
               exit_blocked(pexit, ball->in_room) > EXIT_STATUS_OPEN )
	{
	  sprintf( buf, "$p flys in from $T and hits the %s wall.", 
		   dir_name[dir] );
	  act( AT_WHITE, buf, ch, ball, dir_noun[rev_dir[dir]], TO_ROOM );
	  sprintf( buf, "You cast your $p %d room%s $T, where it hits a wall.",
		   dist, dist > 1 ? "s" : "" );

	 act( AT_WHITE, buf, ch, ball, dir_name[dir], TO_CHAR );
	 obj_from_room( ball );
         extract_obj( ball );
	 return;
	}
     }

      if ( !target_available( ch, victim, ball ) )
      {
	act( AT_WHITE, 
	    "A $p flies in from $T and fizzles.",
	    ch, ball, dir_noun[rev_dir[dir]], TO_ROOM );
	sprintf( buf,
	"Your $p harmlessly fizzles %d room%s $T of here.",
		dist, dist > 1 ? "s" : "" );
	act( AT_WHITE, buf, ch, ball, dir_name[dir], TO_CHAR );
	 obj_from_room( ball );
       extract_obj( ball );
       return;
      }


     if ( dist > 0 )
     {
      obj_from_room( ball );
      act( AT_WHITE, "A $p flys in from $T and hits $n!", victim, ball,
	  dir_noun[rev_dir[dir]], TO_NOTVICT );
      act( AT_WHITE, "A $p flys in from $T and hits you!", victim, ball,
	  dir_noun[rev_dir[dir]], TO_CHAR );
      sprintf( buf, "Your $p flew %d rooms %s and hit $N!", dist,
	      dir_name[dir] );
      act( AT_WHITE, buf, ch, ball, victim, TO_CHAR );
      extract_obj( ball );

 	(*skill_table[sn].spell_fun) ( sn,
				      URANGE( 1, ch->level, LEVEL_HERO ) ,
				      ch, vo );

      if ( IS_NPC( victim ) )
      {
         if ( victim->level > 3 )
             victim->hunting = ch;
      }  
      return;
    }

  act( AT_WHITE, "$n casts a $p at $N!", ch, ball, victim, TO_ROOM );
  act( AT_WHITE, "You cast your $p at $N.", ch, ball, victim, TO_CHAR );
  (*skill_table[sn].spell_fun) ( sn,
				      URANGE( 1, ch->level, LEVEL_HERO ) ,
				      ch, vo );

  extract_obj( ball );
 return;
}

/*
 * Spell functions.
 */
void spell_wake_dead( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA      *obj = (OBJ_DATA *) vo;
    OBJ_DATA      *obj_next;
   
 if ( obj->item_type != ITEM_CORPSE_NPC )
 {
  send_to_char(AT_BLUE, "You cannot animate that.\n\r", ch );
  return;
 }

 obj_next = obj->next;
 if (obj->deleted)
    return;
 magic_mob( ch, obj, obj->invoke_vnum );
 extract_obj(obj);
 return;
}
    
void spell_armor( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
	return;
    af.type      = sn;
    af.level	 = level;
    af.duration  = 24;
    af.location  = APPLY_PDAMP;
    af.modifier  = 2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    if ( ch != victim )
	send_to_char(AT_BLUE, "Ok.\n\r", ch );
    send_to_char(AT_BLUE, "You feel someone protecting you.\n\r", victim );
    return;
}

void spell_astral_rift( int sn, int level, CHAR_DATA *ch, void *vo )
{
  OBJ_DATA *gate;

  if ( IS_NPC(ch) )
   return;

  gate = create_object( get_obj_index( OBJ_VNUM_PORTAL ), 0 );
  gate->timer = 1;
  gate->value[0] = 900;
  act(AT_BLUE, "A huge shimmering gate rises from the ground.", ch, NULL, NULL, TO_CHAR );
  act(AT_BLUE, "$n utters a few incantations and a gate rises from the ground.", ch, NULL, NULL, TO_ROOM );
  obj_to_room( gate, ch->in_room );
  return;
}

void spell_pass_plant ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim ;
    CHAR_DATA *pet;
    int		sector = rsector_check( ch->in_room );
    int		vict_sector;

    if ( IS_AFFECTED( ch, AFF_ANTI_FLEE ) )
    {
      send_to_char( AT_WHITE, "You cannot walk through the plants in your condition!\n\r", ch );
      return;
    }

    if (    sector == SECT_INSIDE  || sector == SECT_PAVED
         || sector == SECT_BADLAND || sector == SECT_SHADOW
         || sector == SECT_HELL    || sector == SECT_ASTRAL
         || sector == SECT_AIR )
    {
	send_to_char(AT_GREEN, "There are no plants here.\n\r", ch );
	return;
    }

    if ( IS_SET( ch->in_room->room_flags, ROOM_NO_SHADOW ) )
    {
        send_to_char(AT_GREEN, "The plants do not respond to your call.\n\r", ch);
        return;
    }

    if ( !( victim = get_char_world( ch, target_name ) ) )
    {
        send_to_char(AT_GREEN, "That person is not in the paradox.\n\r", ch );
        return;
    }

	
    vict_sector = rsector_check( victim->in_room );

    if( IS_SET( victim->in_room->room_flags, ROOM_PRIVATE   )
	|| IS_SET( victim->in_room->room_flags, ROOM_SOLITARY  )
        || vict_sector == SECT_INSIDE || vict_sector == SECT_PAVED
        || vict_sector == SECT_HELL   || vict_sector == SECT_BADLAND
        || vict_sector == SECT_SHADOW || vict_sector == SECT_AIR
        || IS_AFFECTED( victim, AFF_NOASTRAL )  )
    {
        send_to_char(AT_GREEN, "The plants do not reach there.\n\r", ch );
        return;
    }


    for ( pet = ch->in_room->people; pet; pet = pet->next_in_room )
    {
      if ( IS_NPC( pet ) )
        if ( IS_SET( pet->act, ACT_PET ) && ( pet->master == ch ) )
          break;
    }


    if ( ch != victim )
    {
     act(AT_GREEN, "$n melts into the surrounding plant life.", ch, NULL, NULL, TO_ROOM );
     char_from_room( ch );
     char_to_room( ch, victim->in_room );
     act(AT_GREEN, "$n steps out of the surrounding plant life.", ch, NULL, NULL, TO_ROOM );

     if ( pet )
     {
      act( AT_BLUE, "$n melts into the surrounding plant life.", pet, NULL, NULL, TO_ROOM );
      char_from_room( pet );
      char_to_room( pet, victim->in_room );
      act( AT_BLUE, "$n steps out of the surrounding plant life.", pet, NULL, NULL, TO_ROOM );
     }
    }

    do_look( ch, "auto" );
    return;
}


void spell_aura( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_PEACE) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 8 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_PEACE;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "You feel a wave of peace flow lightly over your body.\n\r", victim );
    act(AT_BLUE, "$n looks very peaceful.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_bless( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( victim->position == POS_FIGHTING || is_affected( victim, sn ) )
	return;
    af.type      = sn;
    af.level	 = level;
    af.duration  = 6 + level;
    af.location  = APPLY_HITROLL;
    af.modifier  = level / 8;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_IMM_UNHOLY;
    af.modifier  = -10;
    af.bitvector = 0;
    affect_to_char( victim, &af );
    if ( ch != victim )
	send_to_char(AT_BLUE, "Ok.\n\r", ch );
    send_to_char(AT_BLUE, "You feel righteous.\n\r", victim );
    return;
}

void spell_darkbless( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( victim->position == POS_FIGHTING || is_affected( victim, sn ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = 20 + level;
    af.location  = APPLY_IMM_UNHOLY;
    af.modifier  = -20;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_MDAMP;
    af.modifier  = 10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_PDAMP;
    af.modifier  = 5;
    af.bitvector = 0;
    affect_to_char( victim, &af );
    if ( ch != victim )
	send_to_char(AT_BLUE, "You call forth the hand of oblivion.\n\r", ch );
    send_to_char(AT_BLUE, "The hand of oblivion rests upon you.\n\r", victim );
    return;
}

void spell_crusade( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( victim->position == POS_FIGHTING || is_affected( victim, sn ) )
	return;
    af.type      = sn;
    af.level	 = level;
    af.duration  = 20 + level;
    af.location  = APPLY_STR;
    af.modifier  = 5;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_AGI;
    af.modifier  = 3;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_DAMROLL;
    af.modifier  = 25;
    af.bitvector = 0;
    affect_to_char( victim, &af );
    send_to_char(AT_BLUE, "You begin to savour the taste of war.\n\r", victim );
    return;
}

void spell_restoration( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    int         heal;

    if ( victim->position == POS_FIGHTING )
	return;

    heal = 200;
    if ( (victim->hit + heal) > MAX_HIT(victim) )
    heal = MAX_HIT(victim) - victim->hit;  

    if ( IS_NPC(victim) 
     && !IS_SET(victim->act, ACT_UNDEAD))
    {
    victim->hit += heal;
    send_to_char( AT_WHITE, "Ok.\n\r", ch );
    }
    else 
    damage( ch, victim, heal, sn, DAM_REGEN, TRUE );

    update_pos( victim, ch );
    return;
}


void spell_bio_acceleration( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( victim->position == POS_FIGHTING || is_affected( victim, sn ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = 20 + level;
    af.location  = APPLY_CON;
    af.modifier  = 3;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_MOVE;
    af.modifier  = level * 2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_AGI;
    af.modifier  = 2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char( AT_BLUE, "You greatly enhance your bio-functions.\n\r", ch );
    act(AT_BLUE, "$n's body shudders briefly.", ch, NULL, NULL, TO_ROOM);
    return;
}

/*Decklarean*/
void spell_draw_strength( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( victim->position == POS_FIGHTING || is_affected( victim, sn ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = 20 + level;
    af.location  = APPLY_MANA;
    af.modifier  = number_fuzzy ( level * 4 );
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_HIT;
    af.modifier  = -af.modifier;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    /* They still have hitpoints over there max hit points
       get ride of them */
    if ( MAX_HIT(victim) < victim->hit )
     victim->hit = MAX_HIT(victim);

    send_to_char( AT_BLUE, "You draw from your physical strength and increase your energy reserve.\n\r", ch );
    act(AT_BLUE, "$n's body weakens.", ch, NULL, NULL, TO_ROOM);
    return;
}

void spell_blindness( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    if ( IS_AFFECTED( victim, AFF_BLIND ) || saves_spell( level, victim ))
    {
     if ( !(ch->fighting) )
	send_to_char(AT_BLUE, "You have failed.\n\r", ch );
	return;
    }

    af.type      = sn;
    af.level	 = level;
    af.duration  = 5;
    af.location  = APPLY_DEX;
    af.modifier  = -4;
    af.bitvector = AFF_BLIND;
    affect_to_char( victim, &af );

    act(AT_WHITE, "$N is blinded!", ch, NULL, victim, TO_CHAR    );
    send_to_char(AT_WHITE, "You are blinded!\n\r", victim );
    act(AT_WHITE, "$N is blinded!", ch, NULL, victim, TO_NOTVICT );
    return;
}

void spell_call_lightning( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    int        dam;


    if ( outdoor_check( ch->in_room ) )
    {
	send_to_char(AT_WHITE, "You must be out of doors.\n\r", ch );
	return;
    }

/*    if ( ch->in_room->area->weather_info.sky < SKY_OVERCAST )
    {
	send_to_char(AT_WHITE, "You need bad weather.\n\r", ch );
	return;
    } */

    dam = dice( level / 2, 8 );
    send_to_char(AT_WHITE, "Lightning slashes out of the sky to strike your foes!\n\r", ch );
    act(AT_WHITE, "$n calls lightning from the sky to strike $s foes!",
	ch, NULL, NULL, TO_ROOM );

    for ( vch = char_list; vch; vch = vch->next )
    {
	if ( vch->deleted || !vch->in_room )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

	if ( vch->in_room == ch->in_room )
	{
	dam = sc_dam( ch, dam, DAM_NEGATIVE );
	damage( ch, vch, saves_spell( level, vch ) ? dam/2 : dam, sn, DAM_NEGATIVE, TRUE  );
	}

	if ( vch->in_room->area == ch->in_room->area
	    && outdoor_check( vch->in_room )
	    && IS_AWAKE( vch ) )
	    send_to_char(AT_LBLUE, "Lightning flashes in the sky.\n\r", vch );
    }

    return;
}

void spell_touch_of_darkness( int sn, int level, CHAR_DATA *ch, void *vo )
{
    int dam = dice( 25, 50 ) + level;
    dam = sc_dam( ch, dam, DAM_UNHOLY );
    damage( ch, (CHAR_DATA *) vo, dam, sn, DAM_UNHOLY, TRUE  );
    return;
}

void spell_harmonic_homicide( int sn, int level, CHAR_DATA *ch, void *vo )
{
    int dam = dice( 25, 50 ) + level;
    dam = sc_dam( ch, dam, DAM_DYNAMIC );
    damage( ch, (CHAR_DATA *) vo, dam, sn, DAM_DYNAMIC, TRUE  );
    return;
}

void spell_silver_missile( int sn, int level, CHAR_DATA *ch, void *vo )
{
    int dam = dice( 1, (ch->mana / 2) );
    dam = sc_dam( ch, dam, DAM_POSITIVE );
    damage( ch, (CHAR_DATA *) vo, dam, sn, DAM_POSITIVE, TRUE  );
    return;
}
/* used for the technomancer skills, Flux. */

void spell_fireammo( int sn, int level, CHAR_DATA *ch, void *vo )
{
    int dam = dice( 5, 100 );
    damage( ch, (CHAR_DATA *) vo, dam, sn, DAM_HEAT, TRUE  );
    return;
}
void spell_rocketammo( int sn, int level, CHAR_DATA *ch, void *vo )
{
    int dam = dice( 5, 100 );
    damage( ch, (CHAR_DATA *) vo, dam, sn, DAM_HEAT, TRUE  );
    return;
}
void spell_laserammo( int sn, int level, CHAR_DATA *ch, void *vo )
{
    int dam = dice( 5, 100 );
    damage( ch, (CHAR_DATA *) vo, dam, sn, DAM_POSITIVE, TRUE  );
    return;
}


void spell_sweet_destruction( int sn, int level, CHAR_DATA *ch, void *vo )
{
    int dam = dice( 25, 50 ) + level;
    dam = sc_dam( ch, dam, DAM_DYNAMIC );
    damage( ch, (CHAR_DATA *) vo, dam, sn, DAM_DYNAMIC, TRUE  );
    return;
}
void spell_brimstone_shockwave( int sn, int level, CHAR_DATA *ch, void *vo )
{
    int dam = dice( 25, 50 ) + level;
    dam = sc_dam( ch, dam, DAM_HEAT );
    damage( ch, (CHAR_DATA *) vo, dam, sn, DAM_HEAT, TRUE  );
    return;
}

void spell_phantom_razor( int sn, int level, CHAR_DATA *ch, void *vo )
{
    int dam = dice( 25, 75 ) + level;
    dam = sc_dam( ch, dam, DAM_SLASH );
    damage( ch, (CHAR_DATA *) vo, dam, sn, DAM_SLASH, TRUE  );
    return;
}

void spell_change_sex( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    af.type      = sn;
    af.level     = level;
    af.duration  = 10 * level;
    af.location  = APPLY_SEX;
    do
    {
	af.modifier  = number_range( 0, 2 ) - victim->sex;
    }
    while ( af.modifier == 0 );
    af.bitvector = 0;
    affect_to_char( victim, &af );
    if ( ch != victim )
	send_to_char(AT_WHITE, "Ok.\n\r", ch );
    send_to_char(AT_BLUE, "You feel different.\n\r", victim );
    return;
}
void spell_polymorph_red_dragon( int sn, int level, CHAR_DATA *ch, void *vo )
{
  char                buf [MAX_STRING_LENGTH]; 
    AFFECT_DATA af;
    RACE_DATA *race_o;
    RACE_DATA *race_n;

    if ( IS_AFFECTED2( ch, AFF_POLYMORPH ) )
     return;

        race_o = get_race_data(ch->race);
        
    af.type      = sn;
    af.level	 = level;
    af.duration  = 10;
    af.location  = APPLY_RACE;
    af.modifier  = RACE_RDRAGON - ch->race;
    af.bitvector = AFF_POLYMORPH;
    affect_to_char2( ch, &af );

    af.location  = APPLY_MOVE;
    af.modifier  = level * 2;
    af.bitvector = 0;
    affect_to_char( ch, &af );

    af.location  = APPLY_HIT;
    af.modifier  = level * 5;
    af.bitvector = 0;
    affect_to_char( ch, &af );

       race_n = get_race_data(ch->race);

       polymorph_char( ch, race_o, race_n );

       sprintf( buf, "The &RRed &GDragon, &X%s %s is here.", ch->name, ch->pcdata->title);
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);

    send_to_char(AT_WHITE, "You polymorph into a Red Dragon.\n\r", ch );
    act(AT_WHITE, "$n's form wavers and takes on the form of a Red Dragon.", ch, NULL, NULL, TO_ROOM );

       sprintf( buf, "The &RRed &GDragon&X, %s", ch->name );
       free_string( ch->oname );
       ch->oname = str_dup(buf);

    if ( MAX_HIT(ch) > ch->hit )
     ch->hit = MAX_HIT(ch);
    if ( MAX_MOVE(ch) > ch->move )
     ch->move = MAX_MOVE(ch);

    return;
}
void spell_polymorph_great_dragon( int sn, int level, CHAR_DATA *ch, void *vo )
{
  char                buf [MAX_STRING_LENGTH]; 
    AFFECT_DATA af;

    RACE_DATA *race_o;
    RACE_DATA *race_n;

    if ( IS_AFFECTED2( ch, AFF_POLYMORPH ) )
     return;

        race_o = get_race_data(ch->race);


    af.type      = sn;
    af.level	 = level;
    af.duration  = 10;
    af.location  = APPLY_RACE;
    af.modifier  = RACE_GDRAGON - ch->race;
    af.bitvector = AFF_POLYMORPH;
    affect_to_char2( ch, &af );

    af.location  = APPLY_MOVE;
    af.modifier  = level * 7;
    af.bitvector = 0;
    affect_to_char( ch, &af );

    af.location  = APPLY_HIT;
    af.modifier  = level * 7;
    af.bitvector = 0;
    affect_to_char( ch, &af );

       race_n = get_race_data(ch->race);

       polymorph_char( ch, race_o, race_n );


       sprintf( buf, "The &RGREAT &GDragon, &X%s %s is here.", ch->name, ch->pcdata->title);
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);

    send_to_char(AT_WHITE, "You polymorph into a Great Dragon.\n\r", ch );
    act(AT_WHITE, "$n's form wavers and takes on the form of a Great Dragon.", ch, NULL, NULL, TO_ROOM );

       sprintf( buf, "The &RGreat &GDragon&X, %s", ch->name );
       free_string( ch->oname );
       ch->oname = str_dup(buf);

    if ( MAX_HIT(ch) > ch->hit )
     ch->hit = MAX_HIT(ch);
    if ( MAX_MOVE(ch) > ch->move )
     ch->move = MAX_MOVE(ch);

    return;
}
void spell_polymorph_demon( int sn, int level, CHAR_DATA *ch, void *vo )
{
  char                buf [MAX_STRING_LENGTH]; 
    AFFECT_DATA af;

    RACE_DATA *race_o;
    RACE_DATA *race_n;

    if ( IS_AFFECTED2( ch, AFF_POLYMORPH ) )
     return;

        race_o = get_race_data(ch->race);


    af.type      = sn;
    af.level	 = level;
    af.duration  = 10;
    af.location  = APPLY_RACE;
    af.modifier  = RACE_DEMON - ch->race;
    af.bitvector = AFF_POLYMORPH;
    affect_to_char2( ch, &af );

    af.location  = APPLY_MOVE;
    af.modifier  = level * 5;
    af.bitvector = 0;
    affect_to_char( ch, &af );

    af.location  = APPLY_HIT;
    af.modifier  = level * 5;
    af.bitvector = 0;
    affect_to_char( ch, &af );


       race_n = get_race_data(ch->race);

       polymorph_char( ch, race_o, race_n );

       sprintf( buf, "The &RGreater &rdemon, &X%s %s is here.", ch->name, ch->pcdata->title);
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);

    send_to_char(AT_WHITE, "You polymorph into a Greater Demon.\n\r", ch );
    act(AT_WHITE, "$n's form wavers and takes on the form of a Greater Demon.", ch, NULL, NULL, TO_ROOM );

       sprintf( buf, "The &RGreater &rDemon&X, %s", ch->name );
       free_string( ch->oname );
       ch->oname = str_dup(buf);

    if ( MAX_HIT(ch) > ch->hit )
     ch->hit = MAX_HIT(ch);
    if ( MAX_MOVE(ch) > ch->move )
     ch->move = MAX_MOVE(ch);

    return;
}
void spell_polymorph_siren( int sn, int level, CHAR_DATA *ch, void *vo )
{
  char                buf [MAX_STRING_LENGTH]; 
    AFFECT_DATA af;

    RACE_DATA *race_o;
    RACE_DATA *race_n;

    if ( IS_AFFECTED2( ch, AFF_POLYMORPH ) )
     return;

        race_o = get_race_data(ch->race);


    af.type      = sn;
    af.level	 = level;
    af.duration  = 10;
    af.location  = APPLY_RACE;
    af.modifier  = RACE_SIREN - ch->race;
    af.bitvector = AFF_POLYMORPH;
    affect_to_char2( ch, &af );

    af.location  = APPLY_HITROLL;
    af.modifier  = level;
    af.bitvector = 0;
    affect_to_char( ch, &af );

    af.location  = APPLY_HIT;
    af.modifier  = level * 2;
    af.bitvector = 0;
    affect_to_char( ch, &af );

       race_n = get_race_data(ch->race);

       polymorph_char( ch, race_o, race_n );


       sprintf( buf, "The &CSiren, &X%s %s is here.", ch->name, ch->pcdata->title);
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);

    send_to_char(AT_WHITE, "You polymorph into a Beautiful Siren.\n\r", ch );
    act(AT_WHITE, "$n's form wavers and takes on the form of a Beautiful Siren.", ch, NULL, NULL, TO_ROOM );

       sprintf( buf, "The &CSiren&X, %s", ch->name );
       free_string( ch->oname );
       ch->oname = str_dup(buf);

    if ( MAX_HIT(ch) > ch->hit )
     ch->hit = MAX_HIT(ch);
    if ( MAX_MOVE(ch) > ch->move )
     ch->move = MAX_MOVE(ch);

    return;
}
void spell_polymorph_beholder( int sn, int level, CHAR_DATA *ch, void *vo )
{
  char                buf [MAX_STRING_LENGTH]; 
    AFFECT_DATA af;

    RACE_DATA *race_o;
    RACE_DATA *race_n;

    if ( IS_AFFECTED2( ch, AFF_POLYMORPH ) )
     return;

        race_o = get_race_data(ch->race);


    af.type      = sn;
    af.level	 = level;
    af.duration  = 10;
    af.location  = APPLY_RACE;
    af.modifier  = RACE_BEHOLDER - ch->race;
    af.bitvector = AFF_POLYMORPH;
    affect_to_char2( ch, &af );

    af.location	 = APPLY_PDAMP;
    af.modifier	 = -10;
    af.bitvector = 0;
    affect_to_char( ch, &af );

    af.location  = APPLY_MDAMP;
    af.modifier  = 30;
    af.bitvector = 0;
    affect_to_char( ch, &af );

    af.location  = APPLY_HIT;
    af.modifier  = level * 3;
    af.bitvector = 0;
    affect_to_char( ch, &af );

       race_n = get_race_data(ch->race);

       polymorph_char( ch, race_o, race_n );


       sprintf( buf, "The &CBeholder, &X%s %s is here.", ch->name, ch->pcdata->title);
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);

    send_to_char(AT_WHITE, "You polymorph into a Chaotic being.\n\r", ch );
    act(AT_WHITE, "$n's form wavers and takes on the form of a Beholder.", ch, NULL, NULL, TO_ROOM );

       sprintf( buf, "The &CBeholder&X, %s", ch->name );
       free_string( ch->oname );
       ch->oname = str_dup(buf);

    if ( MAX_HIT(ch) > ch->hit )
     ch->hit = MAX_HIT(ch);
    if ( MAX_MOVE(ch) > ch->move )
     ch->move = MAX_MOVE(ch);

    return;
}

void spell_polymorph_angel( int sn, int level, CHAR_DATA *ch, void *vo )
{
  char                buf [MAX_STRING_LENGTH]; 
    AFFECT_DATA af;

    RACE_DATA *race_o;
    RACE_DATA *race_n;

    if ( IS_AFFECTED2( ch, AFF_POLYMORPH ) )
     return;

        race_o = get_race_data(ch->race);


    af.type      = sn;
    af.level	 = level;
    af.duration  = 10;
    af.location  = APPLY_RACE;
    af.modifier  = RACE_ANGEL - ch->race;
    af.bitvector = AFF_POLYMORPH;
    affect_to_char2( ch, &af );

    af.location	 = APPLY_PDAMP;
    af.modifier	 = 20;
    af.bitvector = AFF_MANANET;
    affect_to_char( ch, &af );

    af.location  = APPLY_MDAMP;
    af.modifier  = 20;
    af.bitvector = 0;
    affect_to_char( ch, &af );

    af.location  = APPLY_HIT;
    af.modifier  = level * 3;
    af.bitvector = 0;
    affect_to_char( ch, &af );

       race_n = get_race_data(ch->race);

       polymorph_char( ch, race_o, race_n );

       sprintf( buf, "The &WAngel, &X%s %s is here.", ch->name, ch->pcdata->title);
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);

    send_to_char(AT_WHITE, "You polymorph into a divine being.\n\r", ch );
    act(AT_WHITE, "$n's form wavers and takes on the form of an Angel.", ch, NULL, NULL, TO_ROOM );

       sprintf( buf, "The &WAngel&X, %s", ch->name );
       free_string( ch->oname );
       ch->oname = str_dup(buf);

    if ( MAX_HIT(ch) > ch->hit )
     ch->hit = MAX_HIT(ch);
    if ( MAX_MOVE(ch) > ch->move )
     ch->move = MAX_MOVE(ch);

    return;
}
void spell_polymorph_gaiaen_paladin( int sn, int level, CHAR_DATA *ch, void *vo )
{
  char                buf [MAX_STRING_LENGTH]; 
    AFFECT_DATA af;

    RACE_DATA *race_o;
    RACE_DATA *race_n;

    if ( IS_AFFECTED2( ch, AFF_POLYMORPH ) )
     return;

        race_o = get_race_data(ch->race);

    af.type      = sn;
    af.level	 = level;
    af.duration  = 10;
    af.location  = APPLY_RACE;
    af.modifier  = RACE_GPALADIN - ch->race;
    af.bitvector = AFF_POLYMORPH;
    affect_to_char2( ch, &af );

    af.location  = APPLY_MOVE;
    af.modifier  = level;
    af.bitvector = AFF_MANANET;
    affect_to_char( ch, &af );

    af.location  = APPLY_HIT;
    af.modifier  = level * 5;
    af.bitvector = 0;
    affect_to_char( ch, &af );

       race_n = get_race_data(ch->race);

       polymorph_char( ch, race_o, race_n );

       sprintf( buf, "The &RCrusader &Bof &GNature, &X%s %s is here.", ch->name, ch->pcdata->title);
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);

    send_to_char(AT_WHITE, "You polymorph into the champion of nature.\n\r", ch );
    act(AT_WHITE, "$n's form wavers and takes on the form of the champion of nature!", ch, NULL, NULL, TO_ROOM );

       sprintf( buf, "The &RCrusader &Bof &GNature&X, %s", ch->name );
       free_string( ch->oname );
       ch->oname = str_dup(buf);

    if ( MAX_HIT(ch) > ch->hit )
     ch->hit = MAX_HIT(ch);
    if ( MAX_MOVE(ch) > ch->move )
     ch->move = MAX_MOVE(ch);

    return;
}

void spell_charm_person( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( victim == ch )
    {
	send_to_char(AT_BLUE, "You like yourself even better!\n\r", ch );
	return;
    }

    if ( !IS_NPC( victim ) )
       return;
    if ( level < victim->level
	|| saves_spell( level, victim ) )
	return;

    if ( victim->master )
	stop_follower( victim );
    add_follower( victim, ch );
    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 6 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_CHARM;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "Ok.\n\r", ch );
    act(AT_BLUE, "Isn't $n just so nice?", ch, NULL, victim, TO_VICT );
    return;
}

void spell_tattoo( int sn, int level, CHAR_DATA *ch, void *vo )
{
    TATTOO_DATA *tattoo;
    AFFECT_DATA *paf;

    tattoo = create_tattoo( vo );
    tattoo->short_descr = "A snake";
    tattoo->wear_loc = TATTOO_FACE;
    tattoo->magic_boost = 0;

    if ( !affect_free )
    {
	paf		= alloc_perm( sizeof( *paf ) );
    }
    else
    {
	paf		= affect_free;
	affect_free	= affect_free->next;
    }

    paf->type		= sn;
    paf->duration	= -1;
    paf->location	= APPLY_HITROLL;
    paf->modifier	= 25;
    paf->bitvector	= 0;
    paf->next		= tattoo->affected;
    tattoo->affected	= paf;

    tattoo_to_char( tattoo, vo, FALSE );

 return;
}

void spell_continual_light( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA *light;

    light = create_object( get_obj_index( OBJ_VNUM_LIGHT_BALL ), 0 );
    obj_to_room( light, ch->in_room );

    act(AT_BLUE, "You twiddle your thumbs and $p appears.", ch, light, NULL, TO_CHAR );
    act(AT_BLUE, "$n twiddles $s thumbs and $p appears.",   ch, light, NULL, TO_ROOM );
    return;
}



void spell_control_weather( int sn, int level, CHAR_DATA *ch, void *vo )
{
/*    if ( !str_cmp( target_name, "better" ) )
	ch->in_room->area->weather_info.change += dice( level / 3, 4 );
    else if ( !str_cmp( target_name, "worse" ) )
	ch->in_room->area->weather_info.change -= dice( level / 3, 4 );
    else
	send_to_char (AT_BLUE, "Do you want it to get better or worse?\n\r", ch );
*/
    send_to_char(AT_BLUE, "Ok.\n\r", ch );
    return;
}



void spell_create_food( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA *mushroom;

    mushroom = create_object( get_obj_index( OBJ_VNUM_MUSHROOM ), 0 );
    mushroom->value[0] = 5 + level;
    obj_to_room( mushroom, ch->in_room );

    act(AT_ORANGE, "$p suddenly appears.", ch, mushroom, NULL, TO_CHAR );
    act(AT_ORANGE, "$p suddenly appears.", ch, mushroom, NULL, TO_ROOM );
    return;
}



void spell_create_spring( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA *spring;

    spring = create_object( get_obj_index( OBJ_VNUM_SPRING ), 0 );
    spring->timer = level;
    obj_to_room( spring, ch->in_room );

    act(AT_BLUE, "$p flows from the ground.", ch, spring, NULL, TO_CHAR );
    act(AT_BLUE, "$p flows from the ground.", ch, spring, NULL, TO_ROOM );
    return;
}



void spell_create_water( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA *obj   = (OBJ_DATA *) vo;
    int       water;

    if ( obj->item_type != ITEM_DRINK_CON )
    {
	send_to_char(AT_BLUE, "It is unable to hold water.\n\r", ch );
	return;
    }

    if ( obj->value[2] != LIQ_WATER && obj->value[1] != 0 )
    {
	send_to_char(AT_BLUE, "It contains some other liquid.\n\r", ch );
	return;
    }

    water = UMIN( level,
		 obj->value[0] - obj->value[1] );
  
    if ( water > 0 )
    {
	obj->value[2] = LIQ_WATER;
	obj->value[1] += water;
	if ( !is_name( NULL, "water", obj->name ) )
	{
	    char buf [ MAX_STRING_LENGTH ];

	    sprintf( buf, "%s water", obj->name );
	    free_string( obj->name );
	    obj->name = str_dup( buf );
	}
	act(AT_BLUE, "$p is filled.", ch, obj, NULL, TO_CHAR );
    }

    return;
}

void spell_detect_hidden( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_DETECT_HIDDEN ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_DETECT_HIDDEN;
    affect_to_char( victim, &af );

    if ( ch != victim )
	send_to_char(AT_BLUE, "Ok.\n\r", ch );
    send_to_char(AT_BLUE, "Your awareness improves.\n\r", victim );
    return;
}



void spell_detect_invis( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_DETECT_INVIS ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_DETECT_INVIS;
    affect_to_char( victim, &af );

    if ( ch != victim )
	send_to_char(AT_BLUE, "Ok.\n\r", ch );
    send_to_char(AT_BLUE, "Your eyes tingle.\n\r", victim );
    return;
}

void spell_detect_magic( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA *paf;
    char         buf  [ MAX_STRING_LENGTH ];
    char         buf1 [ MAX_STRING_LENGTH ];
    bool printed = FALSE;

    buf1[0] = '\0';

  if ( !victim->affected && !victim->affected2 
    && !victim->affected_by && !victim->affected_by2 ) 
  {
   send_to_char( AT_CYAN, "They are not affected by anything.\n\r", ch); 
   return;
  }

   if ( IS_NPC( victim ) )
   {
    if ( victim->affected_by || victim->affected_by2 )
    {
     sprintf( buf, "Affected by&w:&W %s %s\n\r",
      affect_bit_name(victim->affected_by ),
      affect_bit_name2(victim->affected_by2 ) ); 
     send_to_char(AT_CYAN, buf, ch);
     return;
    }
   }

    if ( victim->affected )
    {
	for ( paf = victim->affected; paf; paf = paf->next )
	{
		    if ( paf->deleted )
	        continue;

	    if ( !printed )
	    {
		send_to_char( AT_CYAN, "They are affected by:\n\r", ch );
		printed = TRUE;
	    }

	    sprintf( buf, "&BSpell&W: '&G%s&W'", skill_table[paf->type].name );
            send_to_char( AT_WHITE, buf, ch );

		sprintf( buf,
			" &Wmodifies &G%s&W by %d for %d hours",
			affect_loc_name( paf->location ),
			paf->modifier,
			paf->duration );
		send_to_char(AT_WHITE, buf, ch );

	    send_to_char( AT_WHITE, ".\n\r", ch );
	}
    }
    if ( victim->affected2 )
    {
	for ( paf = victim->affected2; paf; paf = paf->next )
	{
		    if ( paf->deleted )
		        continue;

	    if ( !printed )
	    {
		send_to_char( AT_CYAN, "They are affected by:\n\r", ch );
		printed = TRUE;
	    }

	    sprintf( buf, "&BSpell&W: '&G%s&W'", skill_table[paf->type].name );
            send_to_char( AT_WHITE, buf, ch );

		sprintf( buf,
			" &Wmodifies &G%s&W by %d for %d hours",
			affect_loc_name( paf->location ),
			paf->modifier,
			paf->duration );
		send_to_char(AT_WHITE, buf, ch );

	    send_to_char( AT_WHITE, ".\n\r", ch );
	}
    }

    return;
}



void spell_detect_poison( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA *obj = (OBJ_DATA *) vo;

    if ( obj->item_type == ITEM_DRINK_CON || obj->item_type == ITEM_FOOD )
    {
	if ( obj->value[3] != 0 )
	    send_to_char(AT_GREEN, "You smell poisonous fumes.\n\r", ch );
	else
	    send_to_char(AT_GREEN, "It looks very delicious.\n\r", ch );
    }
    else
    {
	send_to_char(AT_GREEN, "It looks very delicious.\n\r", ch );
    }

    return;
}



void spell_earthquake( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;

    send_to_char(AT_ORANGE, "The earth trembles beneath your feet!\n\r", ch );
    act(AT_ORANGE, "$n makes the earth tremble and shiver.", ch, NULL, NULL, TO_ROOM );

    for ( vch = char_list; vch; vch = vch->next )
    {
        if ( vch->deleted || !vch->in_room )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;
        if ( IS_AFFECTED( vch, AFF_FLYING ) )
        continue;
        if ( vch->race == RACE_PIXIE
        ||   vch->race == RACE_RDRAGON
        ||   vch->race == RACE_GDRAGON
        ||   vch->race == RACE_ANGEL
        ||   vch->race == RACE_GPALADIN
        ||   vch->race == RACE_SIREN
        ||   vch->race == RACE_AQUINIS )
         continue;
         
	if ( vch->in_room == ch->in_room )
        {
		damage( ch, vch, level + dice( 2, 8 ), sn, DAM_BASH, TRUE  );
	    continue;
        }

	if ( vch->in_room->area == ch->in_room->area )
	    send_to_char(AT_ORANGE, "The earth trembles and shivers.\n\r", vch );
    }

    return;
}

void spell_chain_lightning( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    
    int dam = 0;

    send_to_char(AT_BLUE, "Bolts of electricity arc from your hands!\n\r", ch );
    act(AT_BLUE, "Electrical energy bursts from $n's hands.", ch, NULL, NULL, TO_ROOM );

    for ( vch = char_list; vch; vch = vch->next )
    {
	if ( vch->deleted || !vch->in_room )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;
	if ( vch->in_room == ch->in_room )
	{
    dam = level + dice(level, 6);
		damage( ch, vch, dam, sn, DAM_NEGATIVE, TRUE  );
	    continue;
	}

	if ( vch->in_room->area == ch->in_room->area )
	    send_to_char(AT_BLUE, "The air fills with static.\n\r", vch );
    }

    return;
}


void spell_meteor_swarm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    
    int dam = 0;
    AFFECT_DATA af;
   
    send_to_char(AT_RED, "Flaming meteors fly forth from your outstreched hands!\n\r", ch );
    act(AT_RED, "Hundreds of flaming meteors fly forth from $n's hands.", ch, NULL, NULL, TO_ROOM );

    for ( vch = char_list; vch; vch = vch->next )
    {
        if ( vch->deleted || !vch->in_room )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

	if ( vch->in_room == ch->in_room )
        {
         dam = dice(8, level);

	 dam = sc_dam( ch, dam, DAM_HEAT );

         if ( IS_AFFECTED( vch, AFF_FLAMING ) )
          dam /= 4;

         damage( ch, vch, dam, sn, DAM_HEAT, TRUE  );

         if ( !IS_AFFECTED( vch, AFF_FLAMING ) )
         {
                af.type      = sn;
                af.level     = level;
                af.duration  = level / 8;
                af.location  = APPLY_NONE;
                af.modifier  = 0;
                af.bitvector = AFF_FLAMING;
                affect_join( vch, &af );
	        send_to_char(AT_RED, "Your body bursts into flame!\n\r", vch);
         }
	 continue;
	}

    }

    return;
}


void spell_enchant_weapon( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;
    AFFECT_DATA *paf;

    if ( obj->item_type != ITEM_WEAPON
	|| IS_OBJ_STAT( obj, ITEM_MAGIC )
	|| ( obj->affected && !IS_OBJ_STAT( obj, ITEM_DWARVEN ) ) )
    {
	send_to_char(AT_BLUE, "That item cannot be enchanted.\n\r", ch );
	return;
    }

    if ( !affect_free )
    {
	paf		= alloc_perm( sizeof( *paf ) );
    }
    else
    {
	paf		= affect_free;
	affect_free	= affect_free->next;
    }

    paf->type		= sn;
    paf->duration	= -1;
    paf->location	= APPLY_HITROLL;
    paf->modifier	= 1 + (level >= 18) + (level >= 25) + (level >= 45) + (level >= 65) +(level >= 90);
    paf->bitvector	= 0;
    paf->next		= obj->affected;
    obj->affected	= paf;

    if ( !affect_free )
    {
	paf		= alloc_perm( sizeof( *paf ) );
    }
    else
    {
	paf		= affect_free;
	affect_free	= affect_free->next;
    }

    paf->type		= sn;
    paf->duration	= -1;
    paf->location	= APPLY_DAMROLL;
    paf->modifier	= 1 + (level >= 18) + (level >= 25) + (level >= 45) + (level >= 65) +(level >= 90);;
    paf->bitvector	= 0;
    paf->next		= obj->affected;
    obj->affected	= paf;

    send_to_char(AT_BLUE, "Ok.\n\r", ch );
    return;
}

void spell_rainbow_blade( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;

    if ( obj->item_type != ITEM_WEAPON
	|| IS_OBJ_STAT( obj, ITEM_MAGIC )
	|| IS_OBJ_STAT( obj, ITEM_RAINBOW )
	|| ( obj->affected && !IS_OBJ_STAT( obj, ITEM_DWARVEN ) ) )
    {
	send_to_char(AT_RED, "That item cannot be enchanted.\n\r", ch );
	return;
    }
    SET_BIT( obj->extra_flags, ITEM_MAGIC);
    SET_BIT( obj->extra_flags, ITEM_RAINBOW );
    send_to_char(AT_RED, "You wrap a rainbow around the weapon.\n\r", ch );
    return;
}

void spell_shock_blade( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;

    if ( IS_OBJ_STAT( obj, ITEM_SHOCK ) )
    {
	send_to_char(AT_RED, "That item cannot be enchanted.\n\r", ch );
	return;
    }
    SET_BIT( obj->extra_flags, ITEM_SHOCK );
    send_to_char(AT_RED, "Electricity arcs about the weapon.\n\r", ch );
    return;
}

void spell_chaos_blade( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;

    if ( obj->item_type != ITEM_WEAPON
	|| IS_OBJ_STAT( obj, ITEM_MAGIC )
	|| IS_OBJ_STAT( obj, ITEM_CHAOS )
	|| ( obj->affected && !IS_OBJ_STAT( obj, ITEM_DWARVEN ) ) )
    {
	send_to_char(AT_YELLOW, "That item cannot be enchanted.\n\r", ch );
	return;
    }
    SET_BIT( obj->extra_flags, ITEM_MAGIC);
    SET_BIT( obj->extra_flags, ITEM_CHAOS );
    send_to_char(AT_RED, "The weapon shimmers chaotically.\n\r", ch );
    return;
}

void spell_wretch_bone( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;
    AFFECT_DATA *pAf;

    if ( obj->composition != MATERIAL_BONE &&
         obj->pIndexData->vnum != OBJ_VNUM_BONE )
    {
     send_to_char(AT_YELLOW, "You need a fresh arm or leg bone.\n\r", ch );
     return;
    }

    if ( !IS_OBJ_STAT( obj, ITEM_POISONED ))
     SET_BIT( obj->extra_flags, ITEM_POISONED );

    switch( dice( 1, 3 ) )
    {
     case 1:
      if ( !IS_OBJ_STAT( obj, ITEM_SHOCK ))
       SET_BIT( obj->extra_flags, ITEM_SHOCK );
     break;

     case 2:
      if ( !IS_OBJ_STAT( obj, ITEM_FLAME ))
       SET_BIT( obj->extra_flags, ITEM_FLAME );
     break;

     case 3:
      if ( !IS_OBJ_STAT( obj, ITEM_CHAOS ))
       SET_BIT( obj->extra_flags, ITEM_CHAOS );
     break;
    }

    pAf             =   new_affect();
    pAf->location   =   APPLY_DAMROLL;
    pAf->modifier   =   number_range( get_curr_int(ch)/2, ch->level/2 );
    pAf->type       =   -1;
    pAf->duration   =   -1;
    pAf->next       =   obj->affected;
    obj->affected  =   pAf;

    pAf             =   new_affect();
    pAf->location   =   APPLY_HITROLL;
    pAf->modifier   =   number_range( get_curr_int(ch), ch->level );
    pAf->type       =   -1;
    pAf->duration   =   -1;
    pAf->next       =   obj->affected;
    obj->affected  =   pAf;

    obj->value[8] = WEAPON_TEAR;
    obj->value[3] = DAMNOUN_TEAR;
    obj->value[4] = 1;
    obj->durability = dice( 1, 15 );

    act( AT_YELLOW,
     "You concentrate on $p and it contorts with the powers of death and decay.",
     ch, obj, NULL, TO_CHAR );

    act( AT_YELLOW,
     "$n gazes strongly at $p and it contorts.",
     ch, obj, NULL, TO_ROOM );

 return;
}

void spell_bind_weapon( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;

    if ( obj->item_type != ITEM_WEAPON
	|| IS_OBJ_STAT( obj, ITEM_SOUL_BOUND ) )
    {
	send_to_char(AT_YELLOW, "That item cannot be enchanted.\n\r", ch );
	return;
    }

    SET_BIT( obj->extra_flags, ITEM_SOUL_BOUND );
    send_to_char(AT_RED, "The weapon glows, and now feels like an extension of your hand.\n\r", ch );
    return;
}

void spell_flame_blade( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;

    if ( IS_OBJ_STAT( obj, ITEM_FLAME ) )
    {
	send_to_char(AT_YELLOW, "That item cannot be enchanted.\n\r", ch );
	return;
    }

    SET_BIT( obj->extra_flags, ITEM_FLAME );
    send_to_char(AT_RED, "Flames lick the weapon.\n\r", ch );
    return;
}

void spell_frost_blade( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;

    if ( IS_OBJ_STAT( obj, ITEM_ICY ) )
    {
	send_to_char(AT_LBLUE, "That item cannot be enchanted.\n\r", ch );
	return;
    }

    SET_BIT( obj->extra_flags, ITEM_ICY );
    send_to_char(AT_YELLOW, "Ice crystals form about the weapon.\n\r", ch );
    return;
}
void spell_holysword( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;
    AFFECT_DATA *paf;

    if ( obj->item_type != ITEM_WEAPON
	|| IS_OBJ_STAT( obj, ITEM_BLESS ) )
    {
	send_to_char(AT_BLUE, "That item cannot be consecrated.\n\r", ch );
	return;
    }

    if ( !affect_free )
    {
	paf		= alloc_perm( sizeof( *paf ) );
    }
    else
    {
	paf		= affect_free;
	affect_free	= affect_free->next;
    }

    paf->type		= sn;
    paf->duration	= -1;
    paf->location	= APPLY_HITROLL;
    paf->modifier	= 6 + (level >= 18) + (level >= 25) + (level >= 40) + (level >= 60) +(level >= 90);
    paf->bitvector	= 0;
    paf->next		= obj->affected;
    obj->affected	= paf;

    if ( !affect_free )
    {
	paf		= alloc_perm( sizeof( *paf ));
    }
    else
    {
	paf		= affect_free;
	affect_free	= affect_free->next;
    }

    paf->type		= sn;
    paf->duration	= -1;
    paf->location	= APPLY_DAMROLL;
    paf->modifier	= 6 + (level >= 18) + (level >= 25) + (level >= 45) + (level >= 65) +(level >= 90);;
    paf->bitvector	= 0;
    paf->next		= obj->affected;
    obj->affected	= paf;

    SET_BIT( obj->extra_flags, ITEM_BLESS );
    send_to_char(AT_YELLOW, "You bless the weapon.\n\r", ch );
    return;
}


/*
 * Drain XP, MANA, HP.
 * Caster gains HP.
 */
void spell_energy_drain( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        dam;
    

    if ( ch == victim )
    return;

    if ( saves_spell( level, victim ) && !(ch->fighting))
    {
	send_to_char(AT_BLUE, "Your energy drain fizzles.\n\r", ch );
	return;
    }

    if ( victim->level <= 2 )
    {
	dam		 = ch->hit + 1;
    }
    else
    {
	victim->mana	/= 10;
	victim->move	/= 10;
	dam		 = dice( 4, level );
	if ( ( ch->hit + dam ) > ( MAX_HIT(ch) + 200 ) ) 
	    ch->hit = ( MAX_HIT(ch) + 200 );
	 else
	  ch->hit		+= dam;
    }
    dam = sc_dam( ch, dam, DAM_VOID );
    damage( ch, victim, dam, sn, DAM_VOID, TRUE  );

    return;
}


void spell_farsight( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim;
   ROOM_INDEX_DATA *blah;
    
    if ( !( victim = get_char_world( ch, target_name ) )
	|| IS_SET( victim->in_room->room_flags, ROOM_SAFE      )
	|| IS_SET( victim->in_room->room_flags, ROOM_PRIVATE   )
	|| IS_SET( victim->in_room->room_flags, ROOM_SOLITARY  )
	|| IS_SET( victim->in_room->room_flags, ROOM_NO_ASTRAL_IN )
	|| IS_SET( ch->in_room->room_flags, ROOM_NO_ASTRAL_OUT ) )
    {
	send_to_char(AT_BLUE, "You failed.\n\r", ch );
	return;
    }

    blah = ch->in_room;
    if ( ch != victim )
    {
     char_from_room( ch );
     char_to_room( ch, victim->in_room );
    }
    do_look( ch, "auto" );
    if (ch != victim )
    {
      char_from_room( ch );
      char_to_room( ch, blah );
     }
    return;
}


void spell_molecular_unbind( int sn, int level, CHAR_DATA *ch, void *vo )
{
   CHAR_DATA *victim       = (CHAR_DATA *) vo;
   OBJ_DATA  *obj_lose;
   OBJ_DATA  *obj_next;
   
   if(saves_spell ( level, victim ))
     {
       send_to_char(AT_BLUE, "You get your mind in a knot and fail to unbind those darned molecules.\n\r", ch );
       return;
     }
     
	for ( obj_lose = victim->carrying; obj_lose; obj_lose = obj_next )
	{
            char *msg;

	    obj_next = obj_lose->next_content;
	    if ( obj_lose->deleted )
	        continue;
	    if ( IS_SET( obj_lose->wear_loc, WEAR_NONE ) )
	        continue;
	    if ( IS_SET( obj_lose->extra_flags, ITEM_NO_DAMAGE ) )
	        continue;
       	    switch ( obj_lose->item_type )
	    {
	    default:
	      msg = "Your $p gets ruined!";
	      extract_obj( obj_lose );
	      break;
	    case ITEM_DRINK_CON:
	    case ITEM_POTION:
	    case ITEM_CONTAINER:
            case ITEM_WEAPON:
	    case ITEM_ARMOR:
              {
              OBJ_DATA       *pObj;
              OBJ_INDEX_DATA *pObjIndex;
              char           *name;
              char           buf[MAX_STRING_LENGTH];
              	    
              	    pObjIndex = get_obj_index(4);
              	    pObj = create_object(pObjIndex, obj_lose->level);
              	    name = obj_lose->short_descr;
              	    sprintf(buf, pObj->description, name);
              	    free_string(pObj->description);
              	    pObj->description = str_dup(buf);
              	    pObj->weight = obj_lose->weight;
              	    pObj->timer = obj_lose->level;
              	    msg = "$p has been destroyed!";
              	    extract_obj( obj_lose );
              	    obj_to_room ( pObj, victim->in_room );
                 	  break;
              	    
	    act(AT_YELLOW, msg, victim, obj_lose, NULL, TO_CHAR );
  	}
       }
      }
    return;
}

void spell_shatter( int sn, int level, CHAR_DATA *ch, void *vo )
{
   CHAR_DATA *victim       = (CHAR_DATA *) vo;
   OBJ_DATA  *obj_lose;
   OBJ_DATA  *obj_next;
   
   if(saves_spell ( level, victim ))
     {
       send_to_char(AT_BLUE, "You failed.\n\r", ch );
       return;
     }
     
	for ( obj_lose = victim->carrying; obj_lose; obj_lose = obj_next )
	{

	    obj_next = obj_lose->next_content;

	    if ( obj_lose->deleted )
	        continue;

	    if ( IS_SET( obj_lose->wear_loc, WEAR_NONE ) )
	        continue;

	    if ( IS_SET( obj_lose->extra_flags, ITEM_NO_DAMAGE ) )
	        continue;

       	    switch ( obj_lose->item_type )
	    {
	    default:
	      break;
            case ITEM_WEAPON:
	    case ITEM_ARMOR:
            if ( number_percent( ) > 98 )
            item_damage( 200, obj_lose, victim );
            break;
     	    }
         }
    return;
}
 
void spell_fireshield( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_FIRESHIELD ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 8 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_FIRESHIELD;
    affect_to_char( victim, &af );

    send_to_char(AT_RED, "Your body is engulfed by unfelt flame.\n\r", victim );
    act(AT_RED, "$n's body is engulfed in flames.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_faerie_fire( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_FAERIE_FIRE ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_MDAMP;
    af.modifier  = -5;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_HITROLL;
    af.modifier  = 0 - (level/10);
    af.bitvector = 0;
    affect_to_char( victim, &af );
    
    send_to_char(AT_PINK, "You are surrounded by a pink outline.\n\r", victim );
    act(AT_PINK, "$n is surrounded by a pink outline.", victim, NULL, NULL, TO_ROOM );
    return;
}



void spell_faerie_fog( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *ich;

    send_to_char(AT_PURPLE, "You conjure a cloud of purple smoke.\n\r", ch );
    act(AT_PURPLE, "$n conjures a cloud of purple smoke.", ch, NULL, NULL, TO_ROOM );

    for ( ich = ch->in_room->people; ich; ich = ich->next_in_room )
    {
	if ( !IS_NPC( ich ) && IS_SET( ich->act, PLR_WIZINVIS ) )
	    continue;

	if ( ich == ch || saves_spell( level, ich ) )
	    continue;

	affect_strip ( ich, gsn_invis			);
	affect_strip ( ich, gsn_mass_invis		);
	affect_strip ( ich, gsn_sneak			);
	affect_strip ( ich, gsn_shadow			);
        affect_strip ( ich, skill_lookup("phase shift") );
        affect_strip ( ich, skill_lookup("mist form")   );
        affect_strip ( ich, gsn_hide                    );
        affect_strip ( ich, gsn_chameleon               );
	REMOVE_BIT   ( ich->affected_by, AFF_HIDE	);
	REMOVE_BIT   ( ich->affected_by, AFF_INVISIBLE	);
	REMOVE_BIT   ( ich->affected_by, AFF_SNEAK	);
	REMOVE_BIT   ( ich->affected_by2, AFF_PHASED    );
	
	act(AT_PURPLE, "$n is revealed!", ich, NULL, NULL, TO_ROOM );
	send_to_char(AT_PURPLE, "You are revealed!\n\r", ich );
    }

    return;
}


void spell_fly( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_FLYING ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level + 3;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_FLYING;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "Your feet rise off the ground.\n\r", victim );
    act(AT_BLUE, "$n's feet rise off the ground.", victim, NULL, NULL, TO_ROOM );
    return;
}
void spell_temporal_field( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_TEMPORAL ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level + 3;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_TEMPORAL;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "Your body falls out of time for a second.\n\r", victim );
    act(AT_BLUE, "$n is surrounded by a temporal field.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_breathe_water( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_BREATHE_WATER ) ||
         victim->race == RACE_AQUASAPIEN )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level + 3;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_BREATHE_WATER;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "Magical gills form on your neck.\n\r", victim );
    act(AT_BLUE, "Gills take shape on $n's neck.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_gate( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *gch;
    int        npccount  = 0;
    int        pccount   = 0;

    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
        if ( IS_NPC( gch ) && !IS_AFFECTED( gch, AFF_CHARM ) )
	    npccount++;
	if ( !IS_NPC( gch ) ||
	    ( IS_NPC( gch ) && IS_AFFECTED( gch, AFF_CHARM ) ) )
	    pccount++;
    }

    if ( npccount > pccount )
    {
	do_say( ch, "There are too many of us here!  One must die!" );
        return;
    }

    do_say( ch, "Come brothers!  Join me in this glorious bloodbath!" );
    char_to_room( create_mobile( get_mob_index( MOB_VNUM_DEMON1 ) ),
		 ch->in_room );

    return;
}



/*
 * Spell for mega1.are from Glop/Erkenbrand.
 */
void spell_general_purpose( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        dam;

    dam = number_range( 25, 100 );
    dam = sc_dam( ch, dam, DAM_SCRATCH );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_SCRATCH, TRUE  );
    return;
}



void spell_giant_strength( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_STR;
    af.modifier  = 1 + (level >= 18) + (level >= 25);
    af.bitvector = 0;
    affect_to_char( victim, &af );

    if ( ch != victim )
    {
    if (victim->sex == 0)
	send_to_char(AT_BLUE, "You make it HUGE!\n\r", ch );
    if (victim->sex == 1)
	send_to_char(AT_BLUE, "You make him HUGE!\n\r", ch );
    if (victim->sex == 2)
	send_to_char(AT_BLUE, "You make her HUGE!\n\r", ch );
    }
    send_to_char(AT_BLUE, "You feel stronger.\n\r", victim );
    return;
}
void spell_eternal_intellect( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
 
    if ( is_affected( victim, sn ) )
        return;
   
    af.type      = sn;
    af.level     = level;
    af.duration  = level/2;
    af.location  = APPLY_INT;
    af.modifier  = 1 + (level >= 18) + (level >= 25);
    af.bitvector = 0;
    affect_to_char( victim, &af );

    if ( ch != victim )
    send_to_char(AT_BLUE, "You increase their intellect.\n\r", ch );
    send_to_char(AT_BLUE, "You feel an unsurpased intelligence.\n\r", victim );
    return;
}
void spell_golden_aura( int sn, int level, CHAR_DATA *ch, void *vo )
{
   CHAR_DATA *victim = (CHAR_DATA *)vo;
   AFFECT_DATA af;

   if ( !IS_NPC( ch ) 
   && !can_use_skpell( ch, sn ) )
   {
    send_to_char(AT_BLUE, "Nothing happens.\n\r", ch );
    return;
   }

   af.type	 = sn;
   af.level	 = level;
   af.duration	 = number_fuzzy( level / 8 );
   af.location	 = APPLY_PDAMP;
   af.modifier	 = 10;
   af.bitvector	 = 0;
   affect_to_char2( victim, &af );

   af.location	 = APPLY_MDAMP;
   af.modifier	 = 10;
    af.bitvector = 0;
   affect_to_char2( victim, &af );

   send_to_char( AT_YELLOW, "You are surrounded by a golden aura.\n\r", victim );
   act(AT_YELLOW, "$n is surrounded by a golden aura.", victim, NULL, NULL, TO_ROOM );
   return;
}

void spell_call_rod_of_thorns( int sn, int level, CHAR_DATA *ch, void *vo )
{
   OBJ_DATA *rod;

    rod = create_object( get_obj_index( OBJ_VNUM_THORNROD ), 0 );
    rod->timer = ch->level;
    rod->level = ch->level;
    rod->value[1]   = number_fuzzy( number_fuzzy( 1 * ch->level / 3 + 2 ) );
    rod->value[2]   = number_fuzzy( number_fuzzy( 2 * ch->level / 3 + 6 ) );
    SET_BIT( rod->extra_flags, ITEM_SOUL_BOUND );     
    act(AT_BLUE, "You thrust your hands into the earth and the great mother grants you $p.", ch, rod, NULL, TO_CHAR );
    act(AT_BLUE, "$n thrusts $s hands into the ground and pulls out $p.", ch, rod, NULL, TO_ROOM );     
    obj_to_char( rod, ch );
    return;
}                                   

void spell_call_blazing_sword( int sn, int level, CHAR_DATA *ch, void *vo )
{
   OBJ_DATA *sword;

    sword = create_object( get_obj_index( OBJ_VNUM_BLAZESWORD ), 0 );
    sword->timer = ch->level;
    sword->level = ch->level;
    sword->value[1]   = number_fuzzy( number_fuzzy( 1 * ch->level / 3 + 2 ) );
    sword->value[2]   = number_fuzzy( number_fuzzy( 2 * ch->level / 3 + 6 ) );
    SET_BIT( sword->extra_flags, ITEM_SOUL_BOUND );     
    act(AT_BLUE, "You raise you hands and the great mother grants you $p.", ch, sword, NULL, TO_CHAR );
    act(AT_BLUE, "$n raises $s hands and a torrent of flames comes together to form $p.", ch, sword, NULL, TO_ROOM );     
    obj_to_char( sword, ch );
    return;
}                                   


void spell_goodberry( int sn, int level, CHAR_DATA *ch, void *vo )
{
   OBJ_DATA    *obj = (OBJ_DATA *) vo;
   OBJ_DATA     *berry;    

    if ( obj->item_type != ITEM_FOOD
	|| IS_OBJ_STAT( obj, ITEM_MAGIC ) )
    {
	send_to_char(AT_BLUE, "You can do nothing to that item.\n\r", ch );
	return;
    }

    act(AT_BLUE, "You pass your hand over $p slowly.", ch, obj, NULL, TO_CHAR );
    act(AT_BLUE, "$n has created a goodberry.", ch, NULL, NULL, TO_ROOM );
    berry = create_object( get_obj_index( OBJ_VNUM_BERRY ), 0 );
    berry->timer = ch->level;
    berry->value[0] = ch->level * 3;
    berry->value[1] = ch->level * 8;
    extract_obj( obj );
    obj_to_char( berry, ch );
    return;
}

void spell_heal( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int         heal;

    heal = 100;
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

    update_pos( victim, ch );

    return;
}



/*
 * Spell for mega1.are from Glop/Erkenbrand.
 */
void spell_high_explosive( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int dam;

    dam = number_range( 30, 120 );
    dam = sc_dam( ch, dam, DAM_SCRATCH );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_SCRATCH, TRUE  );
    return;
}

void spell_iceshield( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_ICESHIELD ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 3 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_ICESHIELD;
    affect_to_char( victim, &af );

    send_to_char(AT_LBLUE, "An Icy crust forms about your body.\n\r", victim );
    act(AT_LBLUE, "An icy crust forms about $n's body.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_identify( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;
    AFFECT_DATA *paf;
    char         buf [ MAX_STRING_LENGTH ];
    int          spn;
    bool         cansee = FALSE;

    if ( !IS_SET( obj->wear_loc, WEAR_NONE ) && can_see_obj( ch, obj ) )
     cansee = TRUE;

    if ( cansee )
     {
	send_to_char(AT_BLUE, "You can not identify an object you are wearing or wielding.\n\r", ch );
	return;
     }


    sprintf( buf,
	    "Object '%s' is type %s, extra flags %s.\n\r",
	    obj->name,
	    item_type_name( obj ),
	    obj->extra_flags ? extra_bit_name( obj->extra_flags ) : "" );
           
    send_to_char(AT_CYAN, buf, ch );
    sprintf( buf, "Weight : %d, level : %d.\n\r", obj->weight, obj->level );
    send_to_char( AT_CYAN, buf, ch );
    sprintf( buf, "Gold Value: %d  Silver Value: %d  Copper Value: %d\n\r",
	    obj->cost.gold, obj->cost.silver, obj->cost.copper );
    send_to_char(AT_CYAN, buf, ch );

    switch ( obj->item_type )
    {
    case ITEM_PILL:  
    case ITEM_SCROLL: 
    case ITEM_POTION:
	sprintf( buf, "Level %d spells of:", obj->value[0] );
	send_to_char(AT_CYAN, buf, ch );

	if ( is_sn(obj->value[1]) )
	{
	    send_to_char(AT_CYAN, " '", ch );
	    send_to_char(AT_WHITE, skill_table[obj->value[1]].name, ch );
	    send_to_char(AT_CYAN, "'", ch );
	}

	if ( is_sn(obj->value[2]) )
	{
	    send_to_char(AT_CYAN, " '", ch );
	    send_to_char(AT_WHITE, skill_table[obj->value[2]].name, ch );
	    send_to_char(AT_CYAN, "'", ch );
	}

	if ( is_sn(obj->value[3]) )
	{
	    send_to_char(AT_CYAN, " '", ch );
	    send_to_char(AT_WHITE, skill_table[obj->value[3]].name, ch );
	    send_to_char(AT_CYAN, "'", ch );
	}

	send_to_char(AT_CYAN, ".\n\r", ch );
	break;

    case ITEM_WAND: 
    case ITEM_LENSE:
    case ITEM_STAFF: 
	if (!(obj->value[1] == -1 ) )
	    sprintf( buf, "Has %d(%d) charges of level %d",
		   obj->value[1], obj->value[2], obj->value[0] );
	else 
	    sprintf( buf, "Has unlimited charges of level %d", obj->value[0] );
	
	send_to_char(AT_CYAN, buf, ch );
      
	if ( is_sn(obj->value[3]) )
	{
	    send_to_char(AT_CYAN, " '", ch );
	    send_to_char(AT_WHITE, skill_table[obj->value[3]].name, ch );
	    send_to_char(AT_CYAN, "'", ch );
	}

	send_to_char(AT_CYAN, ".\n\r", ch );
	break;
      
    case ITEM_WEAPON:
        sprintf ( buf, "Weapon is of type: %s.\n\r",
                flag_string( weaponclass_flags, obj->value[8] ) );
        send_to_char(AT_RED, buf, ch );
	sprintf( buf, "Damage is %d to %d (average %d).\n\r",
		obj->value[1], obj->value[2],
		( obj->value[1] + obj->value[2] ) / 2 );
	send_to_char(AT_RED, buf, ch );
	break;

    case ITEM_ARMOR:
	sprintf( buf, "Armor class is %d.\n\r", obj->value[0] );
	send_to_char(AT_CYAN, buf, ch );
	break;
    }
    if ( obj->invoke_type != 0 )
    {
      switch( obj->invoke_type )
      {
       default:  send_to_char(AT_CYAN, "Invoke Type Unknown.\n\r", ch ); break;
       case 1 :
         {
           if ( obj->invoke_charge[1] != -1 )
              sprintf( buf, "Object creation invoke
              , with [%d/%d] charges.\n\r",
                    obj->invoke_charge[0], obj->invoke_charge[1] );
           else
              sprintf( buf, "Object creation invoke, with unlimited charges.\n\r" );
           send_to_char(AT_CYAN, buf, ch );
           break;
         }
       case 2 :
         {
           if ( obj->invoke_charge[1] != -1 )
              sprintf( buf, "Monster creation invoke, with [%d/%d] charges.\n\r",
                    obj->invoke_charge[0], obj->invoke_charge[1] );
           else
              sprintf( buf, "Monster creation invoke, with unlimited charges.\n\r" );
           send_to_char(AT_CYAN, buf, ch );
           break;        
         }
       case 3 :
         {
           if ( obj->invoke_charge[1] != -1 )
              sprintf( buf, "Transfer invoke, with [%d/%d] charges.\n\r",
                    obj->invoke_charge[0], obj->invoke_charge[1] );
           else
              sprintf( buf, "Transfer invoke, with unlimited charges.\n\r" );
           send_to_char(AT_CYAN, buf, ch );
           break;
         }
       case 4 :
         {
           if ( obj->invoke_charge[1] != -1 )
              sprintf( buf, "Object morph invoke, with [%d/%d] charges.\n\r",
                    obj->invoke_charge[0], obj->invoke_charge[1] );
           else
              sprintf( buf, "Object morph invoke, with unlimited charges.\n\r" );
           send_to_char(AT_CYAN, buf, ch );
           break;
         }
       case 5 :
         {
           if ( obj->invoke_charge[1] != -1 )
              sprintf( buf, "Spell invoke, has [%d/%d] charges of ",
                    obj->invoke_charge[0], obj->invoke_charge[1] );
           else
              sprintf( buf, "Spell invoke, with unlimited charges of " );
           send_to_char(AT_CYAN, buf, ch );
	   spn = skill_lookup( obj->invoke_spell );
	   if ( is_sn(spn) )
	   {
	    send_to_char(AT_CYAN, " '", ch );
	    send_to_char(AT_WHITE, spn ? obj->invoke_spell : "(none)", ch );
	    send_to_char(AT_CYAN, "'\n\r", ch );
	   }
	   break;
         }
      }   
    } 
    for ( paf = obj->pIndexData->affected; paf; paf = paf->next )
    {
	if ( paf->location != APPLY_NONE && paf->modifier != 0 )
	{
	    sprintf( buf, "Affects %s by %d.\n\r",
		    affect_loc_name( paf->location ), paf->modifier );
	    send_to_char(AT_BLUE, buf, ch );
	}
    }

    for ( paf = obj->affected; paf; paf = paf->next )
    {
	if ( paf->location != APPLY_NONE && paf->modifier != 0 )
	{
	    sprintf( buf, "Affects %s by %d.\n\r",
		    affect_loc_name( paf->location ), paf->modifier );
	    send_to_char(AT_BLUE, buf, ch );
	}
    }

    return;
}

void spell_vibrate( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_VIBRATING ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 8 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_VIBRATING;
    affect_to_char( victim, &af );

    send_to_char(AT_LBLUE, "You set up a complex set of vibrations around your body.\n\r", victim );
    act(AT_LBLUE, "$n's body begins to vibrate.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_infravision( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_INFRARED ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = 2 * level;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_INFRARED;
    affect_to_char( victim, &af );

    send_to_char(AT_RED, "Your eyes glow.\n\r", victim );
    act(AT_RED, "$n's eyes glow.\n\r", ch, NULL, NULL, TO_ROOM );
    return;
}


void spell_incinerate( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_FLAMING ) )
    {
     send_to_char( AT_YELLOW, "They are already on fire.\n\r", ch );
     return;
    }

    if ( saves_spell( level, victim ) )
    {
     send_to_char(AT_RED, "They resist the flames!\n\r", ch );
     return;
    }

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_FLAMING;
    affect_join( victim, &af );

    send_to_char(AT_RED, "Your body bursts into flames!\n\r", victim );
    return;
}

void spell_plasma_burst( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_PLASMA ) )
    return;


    if ( saves_spell( level, victim ) )
    {
     send_to_char(AT_RED, "They resist the plasma!\n\r", ch );
     return;
    }

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_PLASMA;
    affect_join2( victim, &af );

    send_to_char(AT_RED, "A mass of hot, sticky plasma forms on your body!\n\r", victim );
    send_to_char(AT_RED, "You cast a web of sticky heat upon your foe.\n\r", ch );
    return;
}
void spell_spread_disease( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED2( victim, AFF_DISEASED ) )
    return;

    if ( saves_spell( level, victim ) )
    {
     send_to_char(AT_RED, "They resist the sickness!\n\r", ch );
     return;
    }

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_STR;
    af.modifier  = -2;
    af.bitvector = AFF_DISEASED;
    affect_join2( victim, &af );


    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_CON;
    af.modifier  = -2;
    af.bitvector = AFF_DISEASED;
    affect_join2( victim, &af );


    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_DEX;
    af.modifier  = -2;
    af.bitvector = AFF_DISEASED;
    affect_join2( victim, &af );

    send_to_char(AT_RED, "Plague sores erupt on your skin!\n\r", victim );

   if ( ch != victim )
    send_to_char(AT_RED, "Plague sores erupt on their skin!\n\r", ch );
    return;
}

void spell_invis( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_INVISIBLE ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = 24;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_INVISIBLE;
    affect_to_char( victim, &af );

    send_to_char(AT_GREY, "You fade out of existence.\n\r", victim );
    act(AT_GREY, "$n fades out of existence.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_phase_shift( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED2( victim, AFF_PHASED ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = ch->level/6;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_PHASED;
    affect_to_char2( victim, &af );

    send_to_char(AT_GREY, "You phase into another plane.\n\r", victim );
    act(AT_GREY, "$n phases out of reality.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_mist_form( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    
    if ( IS_AFFECTED2( victim, AFF_PHASED ) )
        return;
    
    af.type      = sn;
    af.level     = level;
    af.duration  = ch->level/6;   
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_PHASED;
    affect_to_char2( victim, &af );
 
    send_to_char(AT_GREY, "You seem to feel transparent.\n\r", victim );
    act(AT_GREY, "$n takes on the form of a mist.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_locate_object( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA *obj;
    OBJ_DATA *in_obj;
    char      buf [ MAX_INPUT_LENGTH ];
    bool      found;

    found = FALSE;
    for ( obj = object_list; obj; obj = obj->next )
    {
	if ( !can_see_obj( ch, obj ) || !is_name( ch, target_name, obj->name ) )
	    continue;

	if ( IS_SET( obj->extra_flags, ITEM_NO_LOCATE) && ( get_trust( ch ) < L_APP ) )
	    continue;
	    
	found = TRUE;

	for ( in_obj = obj; in_obj->in_obj; in_obj = in_obj->in_obj )
	    ;

	if ( in_obj->carried_by )
	{
	    sprintf( buf, "%s carried by %s.\n\r",
		    obj->short_descr, PERS( in_obj->carried_by, ch ) );
	}
	else if ( in_obj->stored_by )
	{
	    sprintf( buf, "%s in storage.\n\r",
		    obj->short_descr );
	}
	else
	{
	    sprintf( buf, "%s in %s.\n\r",
		    obj->short_descr, !in_obj->in_room

		    ? "somewhere" : in_obj->in_room->name );
	}

	buf[0] = UPPER( buf[0] );
	send_to_char(AT_BLUE, buf, ch );
    }

    if ( !found )
	send_to_char(AT_WHITE, "Nothing like that in hell, earth, or heaven.\n\r", ch );

    return;
}

void spell_color_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;
    AFFECT_DATA af;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_POSITIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_POSITIVE, TRUE  );
 
   if ( IS_AFFECTED( victim, AFF_BLIND ) )
      return;

    af.type      = sn;
    af.level       = level;
    af.duration  = level / 50;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector  = AFF_BLIND;
    affect_to_char( victim, &af );

  act( AT_YELLOW, 
       "$N's eyes are stricken with color!",
       ch, NULL, victim, TO_CHAR    );

  send_to_char(AT_YELLOW,
       "Your eyes have become stricken with color!\n\r",
        victim );

  act(AT_YELLOW,
       "$N is blinded by color!",
        ch, NULL, victim, TO_NOTVICT );
    return;
}

void spell_color_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;
    AFFECT_DATA af;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_POSITIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_POSITIVE, TRUE  );
   if ( IS_AFFECTED( victim, AFF_BLIND ) )
      return;

    af.type      = sn;
    af.level       = level;
    af.duration  = level / 50;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector  = AFF_BLIND;
    affect_to_char( victim, &af );

  act( AT_YELLOW, 
       "$N's eyes are stricken with color!",
       ch, NULL, victim, TO_CHAR    );

  send_to_char(AT_YELLOW,
       "Your eyes have become stricken with color!\n\r",
        victim );

  act(AT_YELLOW,
       "$N is blinded by color!",
        ch, NULL, victim, TO_NOTVICT );

    return;
}

void spell_color_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;
    AFFECT_DATA af;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_POSITIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_POSITIVE, TRUE  );

   if ( IS_AFFECTED( victim, AFF_BLIND ) )
      return;

    af.type      = sn;
    af.level       = level;
    af.duration  = level / 50;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector  = AFF_BLIND;
    affect_to_char( victim, &af );

  act( AT_YELLOW, 
       "$N's eyes are stricken with color!",
       ch, NULL, victim, TO_CHAR    );

  send_to_char(AT_YELLOW,
       "Your eyes have become stricken with color!\n\r",
        victim );

  act(AT_YELLOW,
       "$N is blinded by color!",
        ch, NULL, victim, TO_NOTVICT );

    if ( IS_AFFECTED2( victim, AFF_CONFUSED ) )
      return;

  af.type      = sn;
  af.level     = level;
  af.duration  = 2;
  af.location  = APPLY_NONE;
  af.modifier  = 0;
  af.bitvector  = AFF_CONFUSED;
  affect_to_char2( victim, &af );

  STUN_CHAR( victim, 5, STUN_COMMAND );

  act(AT_WHITE, 
       "$N is confused by the swirling color!",
        ch,NULL, victim, TO_CHAR ); 

  send_to_char(AT_WHITE, 
       "The vortex of color leaves you confused!\n\r",
        victim );

  act(AT_WHITE, 
      "$N looks confused!", 
       ch, NULL, victim, TO_NOTVICT );

    return;
}

void spell_positronic_flash( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 1, 8 ) + level / 5;
     dam = sc_dam( ch, dam, DAM_POSITIVE );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_POSITIVE, TRUE  );
    }
    return;
}


void spell_positronic_stream( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 2, 8 ) + level / 3;
     dam = sc_dam( ch, dam, DAM_POSITIVE );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_POSITIVE, TRUE  );
    }
    return;
}


void spell_positronic_storm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 3, 8 ) + level / 2;
     dam = sc_dam( ch, dam, DAM_POSITIVE );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_POSITIVE, TRUE  );
    }
    return;
}

void spell_positronic_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_POSITIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_POSITIVE, TRUE  );
 
    return;
}

void spell_positronic_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_POSITIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_POSITIVE, TRUE  );

    return;
}

void spell_positronic_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_POSITIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_POSITIVE, TRUE  );

    return;
}

void spell_fire_flash( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 1, 8 ) + level / 5;
     dam = sc_dam( ch, dam, DAM_HEAT );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_HEAT, TRUE  );
    }
    return;
}


void spell_fire_stream( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 2, 8 ) + level / 3;
     dam = sc_dam( ch, dam, DAM_HEAT );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_HEAT, TRUE  );
    }
    return;
}


void spell_fire_storm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 3, 8 ) + level / 2;
     dam = sc_dam( ch, dam, DAM_HEAT );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_HEAT, TRUE  );
    }
    return;
}

void spell_chaos_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;
        int	damtype = number_range(DAM_HEAT, DAM_INTERNAL);

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, damtype );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, damtype, TRUE  );
 
    return;
}

void spell_fire_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_HEAT );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_HEAT, TRUE  );
 
    return;
}

void spell_fire_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_HEAT );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_HEAT, TRUE  );

    return;
}

void spell_fire_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_HEAT );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_HEAT, TRUE  );

    return;
}

void spell_icy_flash( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 1, 8 ) + level / 5;
     dam = sc_dam( ch, dam, DAM_COLD );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_COLD, TRUE  );
    }
    return;
}


void spell_icy_stream( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 2, 8 ) + level / 3;
     dam = sc_dam( ch, dam, DAM_COLD );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_COLD, TRUE  );
    }
    return;
}


void spell_icy_storm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 3, 8 ) + level / 2;
     dam = sc_dam( ch, dam, DAM_COLD );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_COLD, TRUE  );
    }
    return;
}

void spell_icy_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_COLD );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_COLD, TRUE  );
 
    return;
}

void spell_icy_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_COLD );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_COLD, TRUE  );

    return;
}

void spell_icy_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_COLD );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_COLD, TRUE  );

    return;
}

void spell_electric_flash( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 1, 8 ) + level / 5;
     dam = sc_dam( ch, dam, DAM_NEGATIVE );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_NEGATIVE, TRUE  );
    }
    return;
}


void spell_electric_stream( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 2, 8 ) + level / 3;
     dam = sc_dam( ch, dam, DAM_NEGATIVE );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_NEGATIVE, TRUE  );
    }
    return;
}


void spell_electric_storm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 3, 8 ) + level / 2;
     dam = sc_dam( ch, dam, DAM_NEGATIVE );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_NEGATIVE, TRUE  );
    }
    return;
}

void spell_electric_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_NEGATIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_NEGATIVE, TRUE  );
 
    return;
}

void spell_electric_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_NEGATIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_NEGATIVE, TRUE  );

    return;
}

void spell_electric_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_NEGATIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_NEGATIVE, TRUE  );

    return;
}

void spell_divine_flash( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 1, 8 ) + level / 5;
     dam = sc_dam( ch, dam, DAM_HOLY );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_HOLY, TRUE  );
    }
    return;
}


void spell_divine_stream( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 2, 8 ) + level / 3;
     dam = sc_dam( ch, dam, DAM_HOLY );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_HOLY, TRUE  );
    }
    return;
}


void spell_divine_storm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 3, 8 ) + level / 2;
     dam = sc_dam( ch, dam, DAM_HOLY );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_HOLY, TRUE  );
    }
    return;
}

void spell_divine_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_HOLY );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_HOLY, TRUE  );
 
    return;
}

void spell_divine_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_HOLY );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_HOLY, TRUE  );

    return;
}

void spell_divine_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_HOLY );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_HOLY, TRUE  );

    return;
}

void spell_demonic_flash( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 1, 8 ) + level / 5;
     dam = sc_dam( ch, dam, DAM_UNHOLY );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_UNHOLY, TRUE  );
    }
    return;
}


void spell_demonic_stream( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 2, 8 ) + level / 3;
     dam = sc_dam( ch, dam, DAM_UNHOLY );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_UNHOLY, TRUE  );
    }
    return;
}


void spell_demonic_storm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 3, 8 ) + level / 2;
     dam = sc_dam( ch, dam, DAM_UNHOLY );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_UNHOLY, TRUE  );
    }
    return;
}

void spell_demonic_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_UNHOLY );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_UNHOLY, TRUE  );
 
    return;
}

void spell_demonic_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_UNHOLY );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_UNHOLY, TRUE  );

    return;
}

void spell_demonic_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_UNHOLY );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_UNHOLY, TRUE  );

    return;
}

void spell_acidic_flash( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 1, 8 ) + level / 5;
     dam = sc_dam( ch, dam, DAM_DEGEN );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_DEGEN, TRUE  );
    }
    return;
}


void spell_acidic_stream( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 2, 8 ) + level / 3;
     dam = sc_dam( ch, dam, DAM_DEGEN );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_DEGEN, TRUE  );
    }
    return;
}


void spell_acidic_storm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 3, 8 ) + level / 2;
     dam = sc_dam( ch, dam, DAM_DEGEN );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_DEGEN, TRUE );
    }
    return;
}

void spell_acidic_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_DEGEN );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_DEGEN, TRUE  );
 
    return;
}

void spell_acidic_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_DEGEN );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_DEGEN, TRUE  );

    return;
}

void spell_acidic_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_DEGEN );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_DEGEN, TRUE  );

    return;
}

void spell_entropic_flash( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 1, 8 ) + level / 5;
     dam = sc_dam( ch, dam, DAM_DYNAMIC );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_DYNAMIC, TRUE  );
    }
    return;
}


void spell_entropic_stream( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 2, 8 ) + level / 3;
     dam = sc_dam( ch, dam, DAM_DYNAMIC );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_DYNAMIC, TRUE  );
    }
    return;
}


void spell_entropic_storm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 3, 8 ) + level / 2;
     dam = sc_dam( ch, dam, DAM_DYNAMIC );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_DYNAMIC, TRUE);
    }
    return;
}

void spell_entropic_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_DYNAMIC );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_DYNAMIC, TRUE  );
 
    return;
}

void spell_entropic_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_DYNAMIC );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_DYNAMIC, TRUE  );

    return;
}

void spell_entropic_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_DYNAMIC );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_DYNAMIC, TRUE  );

    return;
}

void spell_gravity_flash( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 1, 8 ) + level / 5;
     dam = sc_dam( ch, dam, DAM_VOID );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_VOID, TRUE  );
    }
    return;
}


void spell_gravity_stream( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 2, 8 ) + level / 3;
     dam = sc_dam( ch, dam, DAM_VOID );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_VOID, TRUE );
    }
    return;
}


void spell_gravity_storm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

     dam = dice( 3, 8 ) + level / 2;
     dam = sc_dam( ch, dam, DAM_VOID );
     if ( saves_spell( level, vch ) )
	dam /= 2;
     damage( ch, vch, dam, sn, DAM_VOID, TRUE  );
    }
    return;
}

void spell_gravity_snap( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 1, 8 ) + level / 5;
    dam = sc_dam( ch, dam, DAM_VOID );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_VOID, TRUE  );
 
    return;
}

void spell_gravity_bolt( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 2, 8 ) + level / 3;
    dam = sc_dam( ch, dam, DAM_VOID );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_VOID, TRUE  );

    return;
}

void spell_gravity_blast( int sn, int level, CHAR_DATA *ch, void *vo )
{
	CHAR_DATA *victim = (CHAR_DATA *) vo;
	int       dam;

    dam = dice( 3, 8 ) + level / 2;
    dam = sc_dam( ch, dam, DAM_VOID );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_VOID, TRUE  );

    return;
}


void spell_mana( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    victim->mana = UMIN( victim->mana + 70, MAX_MANA(victim) );
    update_pos( victim, ch );

    if ( ch != victim )
	send_to_char(AT_BLUE, "You restore their mana.\n\r", ch );
    send_to_char(AT_BLUE, "You feel a surge of energy.\n\r", victim );
    return;
}


void spell_mass_invis( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *gch;
    AFFECT_DATA af;

    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
	if ( !is_same_group( gch, ch ) || IS_AFFECTED( gch, AFF_INVISIBLE ) )
	    continue;

	send_to_char(AT_GREY, "You slowly fade out of existence.\n\r", gch );
	act(AT_GREY, "$n slowly fades out of existence.", gch, NULL, NULL, TO_ROOM );

	af.type      = sn;
        af.level     = level;
	af.duration  = 24;
	af.location  = APPLY_NONE;
	af.modifier  = 0;
	af.bitvector = AFF_INVISIBLE;
	affect_to_char( gch, &af );
    }
    send_to_char(AT_BLUE, "Ok.\n\r", ch );

    return;
}

void spell_null( int sn, int level, CHAR_DATA *ch, void *vo )
{
    send_to_char(AT_WHITE, "That's not a spell!\n\r", ch );
    return;
}

void spell_pass_door( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED(victim, AFF_PASS_DOOR) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 4 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_PASS_DOOR;
    affect_to_char( victim, &af );

    send_to_char(AT_GREY, "You turn translucent.\n\r", victim );
    act(AT_GREY, "$n turns translucent.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_permenancy( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;


    if ( obj->item_type != ITEM_WAND
	&& obj->item_type != ITEM_STAFF
	&& obj->item_type != ITEM_LENSE )
    {
	send_to_char(AT_BLUE, "You cannot make that item permenant.\n\r", ch );
	return;
    }

    obj->value[2] = -1;
    obj->value[1] = -1;
    act(AT_BLUE, "You run your finger up $p, you can feel it's power growing.", ch, obj, NULL, TO_CHAR );
    act(AT_BLUE, "$n slowly runs $s finger up $p.", ch, obj, NULL, TO_ROOM );
    return;
}

void spell_poison( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_POISON ) )
     return;

    if ( saves_spell( level, victim ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_STR;
    af.modifier  = -2;
    af.bitvector = AFF_POISON;
    affect_join( victim, &af );

    if ( ch != victim )
    {
    if (victim->sex == 0)
	send_to_char(AT_GREEN, "You poison it.\n\r", ch );
    if (victim->sex == 1)
	send_to_char(AT_GREEN, "You poison him.\n\r", ch );
    if (victim->sex == 2)
	send_to_char(AT_GREEN, "You poison her.\n\r", ch );
    }
    send_to_char(AT_GREEN, "You feel very sick.\n\r", victim );
    return;
}

void spell_facade( int sn, int level, CHAR_DATA *ch, void *vo )
{
   CHAR_DATA          *victim = (CHAR_DATA *) vo;
   char                buf [MAX_STRING_LENGTH]; 
    AFFECT_DATA af;

   if ( !(victim = get_char_world( ch, target_name ) )
          || victim == ch
          || saves_spell( level, victim)
          || IS_AFFECTED2( ch, AFF_POLYMORPH ) )
      {
         send_to_char( AT_BLUE, "You failed.\n\r", ch );
         return;
      }

    af.type      = sn;
    af.level	 = level;
    af.duration  = level/5;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_POLYMORPH;
    affect_to_char2( ch, &af );
    
    if (!IS_NPC(victim))
      {
       sprintf( buf, "%s %s", victim->name, victim->pcdata->title);
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);
      }
    else
      {
       sprintf( buf, "%s", victim->long_descr );
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);
      }
    act(AT_BLUE, "$n's form wavers and then resolidifies.", ch, NULL, NULL, TO_ROOM);
    send_to_char(AT_BLUE, "You have succesfully disguised yourself.\n\r", ch );
    return;
}

void spell_portal( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA          *victim = (CHAR_DATA *) vo;
    OBJ_DATA           *gate1;
    OBJ_DATA           *gate2;
    int                duration;
    
    if ( !( victim = get_char_world( ch, target_name ) )
	|| victim == ch
	|| !victim->in_room
	|| IS_SET( victim->in_room->room_flags, ROOM_SAFE      )
	|| IS_SET( victim->in_room->room_flags, ROOM_PRIVATE   )
	|| IS_SET( victim->in_room->room_flags, ROOM_SOLITARY  )
	|| IS_SET( victim->in_room->room_flags, ROOM_NO_ASTRAL_IN  )
	|| IS_SET( ch->in_room->room_flags, ROOM_NO_ASTRAL_OUT )
	|| IS_ARENA(ch)
	|| victim->in_room->area == arena.area
	|| IS_SET( victim->in_room->area->area_flags, AREA_PROTOTYPE ) )
          {
        	send_to_char(AT_BLUE, "You failed.\n\r", ch );
        	return;
          }
       
    duration = level/10;
    gate1 = create_object( get_obj_index( OBJ_VNUM_PORTAL ), 0 );
    gate2 = create_object( get_obj_index( OBJ_VNUM_PORTAL ), 0 );
    gate1->timer = duration;
    gate2->timer = duration;
    gate2->value[0] = ch->in_room->vnum;
    gate1->value[0] = victim->in_room->vnum;
    act(AT_BLUE, "A huge shimmering gate rises from the ground.", ch, NULL, NULL, TO_CHAR );
    act(AT_BLUE, "$n utters a few incantations and a gate rises from the ground.", ch, NULL, NULL, TO_ROOM );
    obj_to_room( gate1, ch->in_room );
    act(AT_BLUE, "A huge shimmering gate rises from the ground.", victim, NULL, NULL, TO_CHAR );
    act(AT_BLUE, "A huge shimmering gate rises from the ground.", victim, NULL, NULL, TO_ROOM );
    obj_to_room( gate2, victim->in_room );
    return;
}

void spell_protection( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    af.type      = sn;
    af.level	 = level;
    af.duration  = 24;
    af.location  = APPLY_IMM_SLASH;
    af.modifier  = -10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_IMM_BASH;
    af.modifier  = -10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_IMM_PIERCE;
    af.modifier  = -10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    if ( ch != victim )
    send_to_char(AT_BLUE, "Ok.\n\r", ch );
    send_to_char(AT_BLUE, "You feel protected.\n\r", victim );
    return;
}

void spell_refresh( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int stun;

    victim->move = UMIN( victim->move + level + 50, MAX_MOVE(victim));

     for (stun = 0; stun < STUN_MAX; stun++)
     {
      if ( IS_STUNNED( ch, stun ) )
       victim->stunned[stun] = 0;
     }

    if ( ch != victim )
     send_to_char(AT_BLUE, "Ok.\n\r", ch );
    send_to_char(AT_BLUE, "You feel less tired.\n\r", victim );
    return;
}

/* Expulsion of ITEM_NOREMOVE addition by Katrina */
void spell_remove_curse( int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA  *obj;
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        iWear, SkNum;
    bool        yesno  = FALSE;

    for ( iWear = 2; iWear < MAX_WEAR; iWear *= 2 )
    {
	if ( !( obj = get_eq_char( victim, iWear ) ) )
	    continue;

        if ( IS_SET( obj->extra_flags, ITEM_NODROP ) )
        {
            REMOVE_BIT( obj->extra_flags, ITEM_NODROP );
            send_to_char( AT_BLUE, "You feel a burden relieved.\n\r", ch );
            yesno = TRUE;
        }
	if ( IS_SET( obj->extra_flags, ITEM_NOREMOVE ) )
	{
	    unequip_char( victim, obj );
	    obj_from_char( obj );
	    obj_to_room( obj, victim->in_room );
	    act(AT_BLUE, "You toss $p to the ground.",  victim, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n tosses $p to the ground.", victim, obj, NULL, TO_ROOM );
	    yesno = TRUE;
	}
    }
    SkNum=skill_lookup("incinerate");
    if ( is_affected( victim, SkNum))
    {   
        affect_strip( victim, SkNum);
        send_to_char(AT_BLUE, "Your body has been extinguished.\n\r", ch);
        yesno = TRUE;
    }    
    SkNum=skill_lookup("curse");
    if ( is_affected( victim, SkNum))
    {
	affect_strip( victim, SkNum);
	send_to_char(AT_BLUE, "You feel better.\n\r", victim );
	yesno = TRUE;
    }
    
    if ( ch != victim && yesno )
        send_to_char(AT_BLUE, "Ok.\n\r", ch );
    return;
}

void spell_sanctuary( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 8 );
    af.location  = APPLY_PDAMP;
    af.modifier  = 10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_MDAMP;
    af.modifier  = 15;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_WHITE, "You are surrounded by a white aura.\n\r", victim );
    act(AT_WHITE, "$n is surrounded by a white aura.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_web( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    char        buf[MAX_STRING_LENGTH];
    
    if ( IS_AFFECTED( victim, AFF_ANTI_FLEE ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 10 );
    af.location  = APPLY_DEX;
    af.modifier  = -5;
    af.bitvector = AFF_ANTI_FLEE;
    affect_to_char( victim, &af );
    
    sprintf( buf, "%s lifts his hands and webs entanle you!\n\r", ch->name );
    send_to_char(AT_WHITE, buf, victim );
    act(AT_WHITE, "$n has been immobilized by a plethora of sticky webs.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_hold( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    char        buf[MAX_STRING_LENGTH];
    
    af.type      = 0;
    af.level	 = 0;
    af.duration  = 0;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = 0;
    affect_to_char( victim, &af );
    STUN_CHAR( ch, 10, STUN_TO_STUN );
    STUN_CHAR( victim, 10, STUN_MAGIC );
    STUN_CHAR( victim, 10, STUN_NON_MAGIC );
    victim->position = POS_STUNNED;

    sprintf( buf, "%s lifts his hands and you are HELD!\n\r", ch->oname );
    send_to_char(AT_WHITE, buf, victim );
    act(AT_WHITE, "$n has been immobilized by a magical force.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_prism_cell( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    char        buf[MAX_STRING_LENGTH];
    
    af.type      = 0;
    af.level	 = 0;
    af.duration  = 0;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = 0;
    affect_to_char( victim, &af );
    STUN_CHAR( ch, 10, STUN_TO_STUN );
    STUN_CHAR( victim, 20, STUN_COMMAND );
    STUN_CHAR( victim, 10, STUN_NON_MAGIC );
    victim->position = POS_STUNNED;

    sprintf( buf, "%s smiles at you as a giant prism encases you!\n\r", ch->oname );
    send_to_char(AT_WHITE, buf, victim );
    act(AT_WHITE, "$n has been encased in a giant prism!", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_mute( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    char        buf[MAX_STRING_LENGTH];
    
    af.type      = 0;
    af.level	 = 0;
    af.duration  = 0;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = 0;
    affect_to_char( victim, &af );
    STUN_CHAR( ch, 10, STUN_TO_STUN );
    STUN_CHAR( victim, 20, STUN_MAGIC );

    sprintf( buf, "%s lifts his hands and you are MUTED!\n\r", ch->oname );
    send_to_char(AT_WHITE, buf, victim );
    act(AT_WHITE, "$n has been muted.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_confusion( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    char        buf[MAX_STRING_LENGTH];
    
    if ( IS_AFFECTED2( victim, AFF_CONFUSED ) )
	return;
if ( saves_spell( level, victim ) )
   {
     send_to_char( AT_BLUE, "You failed.\n\r", ch );
     return;
   }

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 10 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_CONFUSED;
    affect_to_char2( victim, &af );
    
    sprintf( buf, "You feel disorientated.\n\r" );
    send_to_char(AT_WHITE, buf, victim );
    act(AT_WHITE, "$n stares around blankly.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_fumble( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    char        buf[MAX_STRING_LENGTH];
    
   if ( IS_AFFECTED2( victim, AFF_FUMBLE ) )
    return;

   if ( saves_spell( level, victim ) )
   {
    send_to_char( AT_BLUE, "You failed.\n\r", ch );
    return;
   }

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 10 );
    af.location  = APPLY_HITROLL;
    af.modifier  = 0 - level / 5;
    af.bitvector = AFF_FUMBLE;
    affect_to_char2( victim, &af );
 
    sprintf( buf, "You feel clumsy.\n\r" );
    send_to_char(AT_WHITE, buf, victim );
    act(AT_WHITE, "$n looks very clumsy.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_mind_probe( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA   *victim;
    char         buf  [ MAX_STRING_LENGTH ];
    char         buf1 [ MAX_STRING_LENGTH ];
    
    
    if (!(victim = get_char_room( ch, target_name ) ) )
    {
      send_to_char(AT_BLUE, "You cannot find them.\n\r", ch ); 
      return;
    }

    
    if (IS_NPC(victim))
    {
     send_to_char (AT_WHITE, "The mind is to chaotic to merge with.\n\r", ch );
     return;
    }

    if ( victim->level >= LEVEL_IMMORTAL )
    {
      act( AT_YELLOW, "$n attempted to probe your mind.", ch, NULL, victim, TO_VICT );
      send_to_char(AT_BLUE, "The mind of an immortal is beyond your understanding.", ch );
      return;
    }

    act( AT_YELLOW, "You send your conciousness into $N's mind.", ch, NULL, victim, TO_CHAR );
    send_to_char(AT_RED, "You feel someone touch your mind.\n\r", victim );

    buf1[0] = '\0';
sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~~~~Score Information~~~~~~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf,
	    " &cYou are &G%s&c, ",
	    victim->name );
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf, "&Y%s&c &Y%s&c (%d hours)\n\r",
             (get_race_data(victim->race))->race_full, class_long( victim ),
	    (get_age( victim ) - 17) * 4 );

    send_to_char( AT_CYAN, buf, ch );

    switch ( victim->position )
    {
    case POS_DEAD:     
	send_to_char( (AT_RED + AT_BLINK), " You are DEAD!!!", ch ); break;
    case POS_MORTAL:
	send_to_char( AT_RED, " You are mortally wounded", ch ); break;
    case POS_INCAP:
	send_to_char( AT_RED, " You are incapacitated", ch ); break;
    case POS_STUNNED:
	send_to_char( AT_RED, " You are stunned", ch ); break;
    case POS_SLEEPING:
	send_to_char( AT_LBLUE, " You are sleeping", ch ); break;
    case POS_RESTING:
	send_to_char( AT_LBLUE, " You are resting", ch ); break;
    case POS_STANDING:
	send_to_char( AT_GREEN, " You are standing", ch); break;
    case POS_FIGHTING:
	send_to_char( AT_BLOOD, " You are fighting", ch ); break;
    }

    if ( !IS_NPC( victim ) && victim->pcdata->condition[COND_DRUNK]   > 10 )
	send_to_char( AT_GREY, ", drunk ", ch );
    if ( !IS_NPC( victim ) && victim->pcdata->condition[COND_THIRST] ==  0
	&& victim->level >= LEVEL_IMMORTAL )
	send_to_char( AT_BLUE, ", thirsty ", ch );
    if ( !IS_NPC( victim ) && victim->pcdata->condition[COND_FULL]   ==  0
	&& victim->level >= LEVEL_IMMORTAL )
	send_to_char( AT_ORANGE, ", starving ", ch  );

    send_to_char( AT_CYAN, " and supposedly ", ch );
         if ( victim->alignment == 1000 ) send_to_char( AT_BLUE, "framed.\n\r",ch );
    else if ( victim->alignment == 0 ) send_to_char( AT_YELLOW, "LitS.\n\r",ch );
    else if ( victim->alignment == -1000 ) send_to_char( AT_RED, "guilty.\n\r",ch );

    if ( victim->clan )
    {
        CLAN_DATA *clan;
        
        clan = get_clan_index( victim->clan );

sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~~~~Clan Information~~~~~~~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

        sprintf( buf, " You are %s of the clan %s.\n\r",
        victim->clev == 0 ? "a member" :
        victim->clev == 1 ? "the centurion" :
        victim->clev == 2 ? "the council" :
        victim->clev == 3 ? "the leader" :
        victim->clev == 4 ? "the champion" : "the warden",
        clan->name );
    send_to_char( AT_CYAN, buf, ch );

	if ( victim->ctimer )
        {
	  sprintf( buf + strlen(buf), " Your clan skill timer reads: %d", victim->ctimer );
        send_to_char( AT_WHITE, buf, ch );
        }
        if ( victim->cquestpnts > 0 )
        {
          sprintf( buf, ", you currently have %d Clan Quest Points.\n\r", victim->cquestpnts );
          send_to_char( AT_WHITE, buf, ch );
        }
        else if ( victim->ctimer )
        send_to_char( AT_WHITE, "\n\r", ch );
    }
    
    if ( get_trust( victim ) != victim->level )
    {
	sprintf( buf, "You have been granted the powers of a level &R%d&W.\n\r",
		get_trust( victim ) );
        send_to_char( AT_WHITE, buf, ch );
    }

sprintf( buf, 
"I~~~~~~~~~~~~~~~~~Character Stats && Status~~~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );
    
    send_to_char( AT_WHITE, "| &cHitpoints: ", ch );
    sprintf ( buf, "%5d/%5d ", victim->hit, MAX_HIT(victim) );
    send_to_char( AT_YELLOW, buf, ch );
     
    sprintf( buf, 
    "Str: &P%2d&p(&P%2d&p)&P  &cDex: &P%2d&p(&P%2d&p)&P  &cAgi: &P%2d&p(&P%2d&p)&P &W|\n\r",
	victim->perm_str,  get_curr_str( victim ),
	victim->perm_dex,  get_curr_dex( victim ),
	victim->perm_agi,  get_curr_agi( victim ) );
    send_to_char( AT_CYAN, buf, ch );

         send_to_char( AT_WHITE, "| &cMana:      ", ch );
         sprintf ( buf, "%5d/%5d ", victim->mana, MAX_MANA(victim) );
         send_to_char( AT_LBLUE, buf, ch );

    sprintf( buf,
"Int: &P%2d&p(&P%2d&p)&P  &cWis: &P%2d&p(&P%2d&p)&P  &cCon: &P%2d&p(&P%2d&p)&P &W|\n\r",
	victim->perm_int,  get_curr_int( victim ),
	victim->perm_wis,  get_curr_wis( victim ),
	victim->perm_con,  get_curr_con( victim ) );
    send_to_char( AT_CYAN, buf, ch );

    send_to_char( AT_WHITE, "| &cMovement:  ", ch );
    sprintf ( buf, "%5d/%5d ", victim->move, MAX_MOVE(victim));
    send_to_char( AT_GREEN, buf, ch );

    send_to_char( AT_CYAN, "Pracs: ", ch );
    sprintf ( buf, "%3d   ", victim->practice );
    send_to_char( AT_PINK, buf, ch );

    sprintf( buf, "Cha:  &P%2d&p(&P%2d&p)               &W|\n\r",
        victim->perm_cha, get_curr_cha( ch ) );
    send_to_char( AT_CYAN, buf, ch );


sprintf( buf, 
"I~~~~~~~~~~~~~~Character && Combat Information~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf, "| &cGold:          &Y%7d ",
	    victim->money.gold );
    send_to_char( AT_WHITE, buf, ch );

	sprintf( buf, " Hitroll: " );
	send_to_char(AT_CYAN, buf, ch );
	sprintf( buf, "%3d ", GET_HITROLL( victim ) );
	send_to_char(AT_RED, buf, ch);

	sprintf( buf, " Magical Damp:  " );
	send_to_char( AT_CYAN, buf, ch );
	sprintf( buf, "%3d     &W|\n\r", victim->m_damp );
	send_to_char( AT_RED, buf, ch );


    sprintf( buf, "| &cSilver:        &w%7d ",
            victim->money.silver );
    send_to_char( AT_WHITE, buf, ch );

	sprintf( buf, " Damroll: " );
	send_to_char( AT_CYAN, buf, ch );
	sprintf( buf, "%3d ", GET_DAMROLL( victim ) );
	send_to_char( AT_RED, buf, ch );

	sprintf( buf, " Physical Damp: " );
	send_to_char( AT_CYAN, buf, ch );
	sprintf( buf, "%3d     &W|\n\r", victim->p_damp );
	send_to_char( AT_RED, buf, ch);

    sprintf( buf, "| &cCopper:        &O%7d ",
            victim->money.copper );
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf, " Level:   &G%3d&c  Age:          &G%4d&c     &W|\n\r",
	    victim->level,   get_age( victim ) );
    send_to_char( AT_CYAN, buf, ch );

    send_to_char( AT_WHITE, "| &cExp:          ", ch );
    sprintf ( buf, "%8d ", victim->exp );
    send_to_char( AT_LBLUE, buf, ch );

    sprintf( buf, " Wimpy:  &P%4d ", victim->wimpy );
    send_to_char( AT_CYAN, buf, ch );

	sprintf( buf, " Page pausing:   &P%2d&c     &W|\n\r",
		victim->pcdata->pagelen );
	send_to_char( AT_CYAN, buf, ch );


    sprintf( buf,
	    "| &cCarry Items: &P%4d&c/&P%4d  &cCarry Weight:    &P%7d&c/&P%7d&ckg   &W|\n\r",
	    victim->carry_number, can_carry_n( victim ),
	    victim->carry_weight, can_carry_w( victim ) );
    send_to_char( AT_WHITE, buf, ch );

sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~Config && Character Status~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf,
    " Autoloot:  %3s Autocoins: %3s  Autosplit: %3s  Autosac:  %3s\n\r",
	    ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_AUTOLOOT ) ) ? "&Ryes&c"
	                                                         : "&Bno &c",
	    ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_AUTOCOINS ) ) ? "&Ryes&c"
	                                                          : "&Bno &c",
	    ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_AUTOSPLIT ) ) ? "&Ryes&c"
	                                                          : "&Bno &c",
	    ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_AUTOSAC  ) ) ? "&Ryes&c"
	                                                  : "&Bno &c" );
    send_to_char( AT_CYAN, buf, ch );    

    sprintf( buf, " Flying:    %3s Invis:     %3s  Sneak:     %3s  Hide:     %3s\n\r",
	    ( IS_SET( victim->affected_by, AFF_FLYING  ) ) ? "&Ryes&c"
	                                   : "&Bno &c",
	    ( IS_SET( victim->affected_by, AFF_INVISIBLE   ) ) ? "&Ryes&c"
	                                   : "&Bno &c",
	    ( IS_SET( victim->affected_by, AFF_SNEAK  ) ) ? "&Ryes&c"
	                                   : "&Bno &c",
	    ( IS_SET( victim->affected_by, AFF_HIDE  ) ) ? "&Ryes&c"
	                                   : "&Bno &c" );
    send_to_char( AT_CYAN, buf, ch );    

        if (  victim->pcdata->attitude < -10 || victim->pcdata->attitude > 10 )
    sprintf( buf, " Autoexit:  %s Attitude:  &G%9s&c[&G%2d&c]\n\r",
	    ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_AUTOEXIT  ) ) ? "&Ryes&c"
	                                                  : "&Bno &c",         
            victim->pcdata->attitude == -11 ? "panther" :
            victim->pcdata->attitude == -12 ? "crane" :
            victim->pcdata->attitude == -13 ? "sparrow" :
            victim->pcdata->attitude == -14 ? "monkey" :
            victim->pcdata->attitude == 11 ? "snake" :
            victim->pcdata->attitude == 12 ? "dragon" :
            victim->pcdata->attitude == 13 ? "tiger" : 
            victim->pcdata->attitude == 14 ? "bull" : "",
          victim->pcdata->attitude );
         else
    sprintf( buf, " Autoexit:  %s Attitude:  &G%9s&c[&G%2d&c]\n\r",
	    ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_AUTOEXIT  ) ) ? "&Ryes&c"
	                                                  : "&Bno &c",         
            victim->pcdata->attitude > 0 ? "offensive" : "defensive",
          victim->pcdata->attitude );
    send_to_char( AT_CYAN, buf, ch );    
 
sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

    send_to_char( AT_RED, "The presence lifts from your mind.\n\r", victim );
    return;
}

void spell_entangle( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    
    if ( IS_AFFECTED( victim, AFF_ANTI_FLEE ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 10 );
    af.location  = APPLY_DEX;
    af.modifier  = -5;
    af.bitvector = AFF_ANTI_FLEE;
    affect_to_char( victim, &af );
    
    act(AT_GREEN, "$n calls forth nature to hold you in place.", ch, NULL, victim, TO_VICT );
    act(AT_GREEN, "Hundreds of vines reach from the ground to entangle $n.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_scry( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_SCRY ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 4 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_SCRY;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "Your vision improves.\n\r", victim );
    return;
}


void spell_shield( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = 8 + level;
    af.location  = APPLY_PDAMP;
    af.modifier  = 5;
    af.bitvector = 0;
    affect_to_char( victim, &af );
    send_to_char(AT_BLUE, "You are surrounded by a force shield.\n\r", victim );
    act(AT_BLUE, "$n is surrounded by a force shield.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_shockshield( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_SHOCKSHIELD ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 6 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_SHOCKSHIELD;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "Sparks of electricity flow into your body.\n\r", victim );
    act(AT_BLUE, "Bolts of electricity flow from the ground into $n's body.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_sleep( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_SLEEP )
	|| level < victim->level )
    {
	send_to_char(AT_BLUE, "You failed.\n\r", ch );
	return;
    }

    af.type      = sn;
    af.level	 = level;
    af.duration  = 4 + level;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_SLEEP;
    affect_join( victim, &af );

    if ( IS_AWAKE( victim ) )
    {
	send_to_char(AT_BLUE, "You feel very sleepy ..... zzzzzz.\n\r", victim );
	if ( victim->position == POS_FIGHTING )
	   stop_fighting( victim, TRUE );
	do_sleep( victim, "" );
    }

    return;
}

void spell_spell_bind( int sn, int level, CHAR_DATA *ch, void *vo )
{
    bool Charged = 0;
    OBJ_DATA    *obj = (OBJ_DATA *) vo;


    if ( obj->item_type == ITEM_WAND 
	|| obj->item_type == ITEM_STAFF
	|| obj->item_type == ITEM_LENSE )
    {
	if(obj->value[2] < obj->value[1])
	{
	     obj->value[2]=obj->value[1];
	     Charged++;
	}
    }
    else if (obj->invoke_type==5 && obj->invoke_spell)
    {
	if(obj->invoke_charge[0] < obj->invoke_charge[1] && obj->invoke_charge[1]!=-1)
	{
	     obj->invoke_charge[0]++;
	     Charged++;
	}
    }
	
    else 
    {
	send_to_char(AT_BLUE, "You cannot bind magic to that item.\n\r", ch );
	return;
    }
	
    if (!Charged)
    {
        send_to_char(AT_BLUE, "That item is at full charge.\n\r", ch );
        return;
    }

    act(AT_BLUE, "You slowly pass your hand over $p, it vibrates slowly.", ch, obj, NULL, TO_CHAR );
    act(AT_BLUE, "$n slowly passes $s hand over $p, it vibrates slowly.", ch, obj, NULL, TO_ROOM );
    return;
}

void spell_stone_skin( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( ch, sn ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_IMM_PIERCE;
    af.modifier  = -10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_GREY, "Your skin turns to stone.\n\r", victim );
    act(AT_GREY, "$n's skin turns to stone.", victim, NULL, NULL, TO_ROOM );
    return;
}

/*Decklarean*/
void spell_bark_skin( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( ch, sn ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level;
    af.location  = APPLY_IMM_SLASH;
    af.modifier  = -5;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_GREY, "Your skin turns to bark.\n\r", victim );
    act(AT_GREY, "$n's skin turns to bark.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_summon( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim;

    if ( !( victim = get_char_world( ch, target_name ) )
	|| victim == ch
	|| ch->in_room->area == arena.area
	|| IS_ARENA(victim)
	|| !victim->in_room
	|| IS_SET( victim->in_room->room_flags, ROOM_SAFE      )
	|| IS_SET( victim->in_room->room_flags, ROOM_PRIVATE   )
	|| IS_SET( victim->in_room->room_flags, ROOM_SOLITARY  )
	|| IS_SET( victim->in_room->room_flags, ROOM_NO_RECALL )
        || IS_SET( victim->in_room->room_flags, ROOM_NO_ASTRAL_OUT )
	|| victim->level >= level + 3
	|| victim->fighting
	|| ( IS_NPC( victim ) && saves_spell( level, victim ) ) 
	|| IS_SET( victim->in_room->area->area_flags, AREA_PROTOTYPE )
	|| IS_AFFECTED( victim, AFF_NOASTRAL ) )
    {
	send_to_char(AT_BLUE, "You failed.\n\r", ch );
	return;
    }

    act(AT_BLUE, "$n disappears suddenly.", victim, NULL, NULL,     TO_ROOM );
    char_from_room( victim );
    char_to_room( victim, ch->in_room );
    act(AT_BLUE, "$n has summoned you!",    ch,     NULL, victim,   TO_VICT );
    act(AT_BLUE, "$n arrives suddenly.",    victim, NULL, NULL,     TO_ROOM );
    send_to_char( AT_BLUE, "You feel a wave of nausia come over you.\n\r", ch );
    ch->position = POS_STUNNED;
    update_pos( ch, ch );
    STUN_CHAR( ch, 3, STUN_COMMAND );
    do_look( victim, "auto" );
    return;
}



void spell_teleport( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA       *victim = (CHAR_DATA *) vo;
    CHAR_DATA *pet;
    ROOM_INDEX_DATA *pRoomIndex;

    if ( !victim->in_room
	|| IS_SET( victim->in_room->room_flags, ROOM_NO_RECALL)
	|| IS_SET( victim->in_room->room_flags, ROOM_NO_ASTRAL_OUT)
	|| IS_SET( victim->in_room->area->area_flags, AREA_PROTOTYPE )
	|| ( !IS_NPC( ch ) && victim->fighting )
	|| ( victim != ch
	    && ( saves_spell( level, victim )
		|| saves_spell( level, victim ) ) ) )
    {
	send_to_char(AT_BLUE, "You failed.\n\r", ch );
	return;
    }

    for ( ; ; )
    {
	pRoomIndex = get_room_index( number_range( 0, 32767 ) );
	if ( pRoomIndex )
	    if (   !IS_SET( pRoomIndex->room_flags, ROOM_PRIVATE  )
		&& !IS_SET( pRoomIndex->room_flags, ROOM_SOLITARY )
		&& !IS_SET( pRoomIndex->room_flags, ROOM_NO_ASTRAL_IN    )
		&& !IS_SET( pRoomIndex->room_flags, ROOM_NO_RECALL) 
		&& !IS_SET( pRoomIndex->area->area_flags, AREA_PROTOTYPE ) )
	    break;
    }

    for ( pet = victim->in_room->people; pet; pet = pet->next_in_room )
    {
      if ( IS_NPC( pet ) )
        if ( IS_SET( pet->act, ACT_PET ) && ( pet->master == victim ) )
          break;
    }
    
    act(AT_BLUE, "$n glimmers briefly, then is gone.", victim, NULL, NULL, TO_ROOM );
    if ( pet )
    {
      act( AT_BLUE, "$n glimmers briefly, then is gone.", pet, NULL, NULL, TO_ROOM );
      char_from_room( pet );
    }
    char_from_room( victim );
    char_to_room( victim, pRoomIndex );
    act(AT_BLUE, "The air starts to sparkle, then $n appears from nowhere.",   victim, NULL, NULL, TO_ROOM );
    do_look( victim, "auto" );
    if ( pet )
    {
      char_to_room( pet, pRoomIndex );
      act( AT_BLUE, "The air starts to sparkle, then $n appears from nowhere.", pet, NULL, NULL, TO_ROOM );
    }
    return;
}



void spell_ventriloquate( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    char       buf1    [ MAX_STRING_LENGTH ];
    char       buf2    [ MAX_STRING_LENGTH ];
    char       speaker [ MAX_INPUT_LENGTH  ];

    target_name = one_argument( target_name, speaker );

    sprintf( buf1, "%s says '%s'.\n\r",              speaker, target_name );
    sprintf( buf2, "Someone makes %s say '%s'.\n\r", speaker, target_name );
    buf1[0] = UPPER( buf1[0] );

    for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
    {
	if ( !is_name( NULL, speaker, vch->name ) )
	    send_to_char(AT_CYAN, saves_spell( level, vch ) ? buf2 : buf1, vch );
    }

    return;
}



void spell_weaken( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) || saves_spell( level, victim ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = level / 2;
    af.location  = APPLY_STR;
    af.modifier  = -2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    if ( ch != victim )
	send_to_char(AT_GREEN, "Ok.\n\r", ch );
    send_to_char(AT_GREEN, "You feel weaker.\n\r", victim );
    return;
}


/*
 * This is for muds that want scrolls of recall.
 */
void spell_word_of_recall( int sn, int level, CHAR_DATA *ch, void *vo )
{
    do_recall( (CHAR_DATA *) vo, "" );
    return;
}

void spell_acid_spray( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    int        dam;
    
  if ( number_percent( ) < 2 ) 
    {
    int location = number_range( 0, 2 );
    switch( location )
     {
     case 0:
	  {
	  act( AT_DGREEN, "The acid blinds $S!", ch, NULL, victim, TO_CHAR );
	  act( AT_DGREEN, "$n's acid blinds $N!", ch, NULL, victim, TO_NOTVICT );
	  act( AT_DGREEN, "$n's acid gets into your eyes!", ch, NULL, victim, TO_VICT );
	  if ( !IS_AFFECTED( victim, AFF_BLIND ) )
	    {
  	    send_to_char( AT_WHITE, "You are blinded!\n\r", victim );
            act( AT_WHITE, "$n is blinded!", victim, NULL, NULL, TO_ROOM );
	    af.type	 = sn;
	    af.level	 = ch->level;
	    af.duration	 = 0;
	    af.location	 = APPLY_HITROLL;
	    af.modifier	 = -10;
	    af.bitvector = AFF_BLIND;
	    affect_to_char( victim, &af );
	    af.location  = APPLY_DEX;
	    af.modifier	 = -2;
            af.bitvector = 0;
	    affect_to_char( victim, &af );
	    }
	  }
	break;
     case 1:
	  act( AT_DGREEN, "The acid mutes $S!", ch, NULL, victim, TO_CHAR );
	  act( AT_DGREEN, "$n's acid mutes $N!", ch, NULL, victim, TO_NOTVICT );
	  act( AT_DGREEN, "$n's acid gets into your mouth!", ch, NULL, victim, TO_VICT );
        if ( !IS_STUNNED( victim, STUN_MAGIC ) )
        {
 	send_to_char( AT_WHITE, "The acid burns your mouth and throat.\n\r", victim );
        STUN_CHAR( victim, 5, STUN_MAGIC );
        }
	break;
     case 2:
      if ( !saves_spell( ch->level, victim ) )
      {
      af.type 	  =  gsn_poison;
      af.level	  =  ch->level;
      af.duration =  1;
      af.location =  APPLY_STR;
      af.modifier = -1;
      af.bitvector = AFF_POISON;
      affect_join( victim, &af );
      send_to_char(AT_GREEN, "You feel very sick.\n\r", victim );
      }
     break;	
      }
    }

    dam = number_range( 1, ch->level );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_DEGEN, TRUE  );
    return;
}


/*
 * NPC spells.
 */
void spell_acid_breath( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        dam;
    int        hpch;
    
    hpch = UMAX( 10, ch->hit );
    dam  = number_range( hpch / 8 + 1, hpch / 4 );
    dam = sc_dam( ch, dam , DAM_DEGEN);
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_DEGEN, TRUE  );
    return;
}



void spell_fire_breath( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        dam;
    int        hpch;
    
    hpch = UMAX( 10, ch->hit );
    dam  = number_range( hpch / 8 + 1, hpch / 4 );
    dam = sc_dam( ch, dam, DAM_HEAT );
    if ( saves_spell( level, victim ) )
	dam /= 2;
       damage( ch, victim, dam, sn, DAM_HEAT, TRUE  );
    return;
}



void spell_frost_breath( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        dam;
    int        hpch;
    
    hpch = UMAX( 10, ch->hit );
    dam  = number_range( hpch / 8 + 1, hpch / 4 );
    dam = sc_dam( ch, dam, DAM_COLD );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_COLD, TRUE  );
    return;
}



void spell_gas_breath( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;
    int        hpch;
    

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

	    hpch = UMAX( 10, ch->hit );
	    dam  = number_range( hpch / 8 + 1, hpch / 4 );
	    dam = sc_dam( ch, dam, DAM_DEGEN );
	    if ( saves_spell( level, vch ) )
		dam /= 2;
	    spell_poison( gsn_poison, level, ch, vch );
    damage( ch, vch, dam, sn, DAM_DEGEN, TRUE  );
    }
    return;
}

void spell_cataclysm( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;
    int        dam;

    for ( vch = ch->in_room->people; vch; vch = vch_next )
    {
        vch_next = vch->next_in_room;
        if ( vch->deleted )
	    continue;
        if ( is_same_group( ch, vch ) )
            continue;
        if ( ch == vch )
            continue;

	    dam  = dice( get_curr_int(ch), ch->level );
	    dam = sc_dam( ch, dam, DAM_POSITIVE );

	    if ( saves_spell( level, vch ) )
		dam /= 8;

       damage( ch, vch, dam, sn, DAM_POSITIVE, TRUE  );
    }
    return;
}

void spell_strike_of_thorns( int sn, int level, CHAR_DATA *ch, void *vo )
{
            CHAR_DATA *victim = (CHAR_DATA *) vo;
            int        dam;
            int        hpch;

	    hpch = dice( 5, 50 );
	    dam  = number_range( hpch / 8 + 1, hpch / 4 );
	    dam  = sc_dam( ch, dam, DAM_DEGEN );

          if ( !(IS_AFFECTED( victim, AFF_POISON )) )
          {
            AFFECT_DATA af;

            af.type      = sn;
            af.level     = level;
            af.duration  = level;
            af.location  = APPLY_STR;
            af.modifier  = -2;
            af.bitvector = AFF_POISON;
            affect_join( victim, &af );
           }

            spell_blindness( gsn_blindness, level, ch, victim);
            dam -= (dam / 4);
	    damage( ch, victim, dam, sn, DAM_DEGEN, TRUE  );

    return;
}

void spell_lightning_breath( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        dam;
    int        hpch;
    
 
    hpch = UMAX( 10, ch->hit );
    dam = number_range( hpch / 8 + 1, hpch / 4 );
    dam = sc_dam( ch, dam, DAM_NEGATIVE );
    if ( saves_spell( level, victim ) )
	dam /= 2;
    damage( ch, victim, dam, sn, DAM_NEGATIVE, TRUE  );
    return;
}

/*
 * Code for Psionicist spells/skills by Thelonius
 */
void spell_adrenaline_control ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level - 5;
    af.location	 = APPLY_DEX;
    af.modifier	 = 2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	= APPLY_CON;
    af.modifier	 = 2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "You have given yourself an adrenaline rush!\n\r", ch );
    act(AT_BLUE, "$n has given $mself an adrenaline rush!", ch, NULL, NULL,
	TO_ROOM );
   
    return;
}

void spell_gaiaen_power ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level;
    af.location	 = APPLY_DEX;
    af.modifier	 = 5;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	= APPLY_CON;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	= APPLY_STR;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	= APPLY_WIS;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	= APPLY_INT;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	= APPLY_AGI;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	= APPLY_CHA;
    af.bitvector = 0;
    affect_to_char( victim, &af );


    send_to_char(AT_BLUE, "Nature lends you strength.\n\r", ch );
    act(AT_BLUE, "$n has recieved the power of nature!", ch, NULL, NULL,
	TO_ROOM );
   
    return;
}


    
void spell_serenity( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *rch;

    /* Yes, we are reusing rch.  -Kahn */
      for ( rch = ch->in_room->people; rch; rch = rch->next_in_room )
      {
	if ( rch->fighting )
	    stop_fighting( rch, TRUE );
      }
   
      send_to_char(AT_BLUE, "You bring peace to the room.\n\r", ch );

    return;
}

void spell_awe ( int sn, int level, CHAR_DATA *ch, void *vo )
  {
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( victim->fighting == ch && !saves_spell( level, victim ) )
    {
	stop_fighting ( victim, TRUE);
	act(AT_BLUE, "$N is in AWE of you!", ch, NULL, victim, TO_CHAR    );
	act(AT_BLUE, "You are in AWE of $n!",ch, NULL, victim, TO_VICT    );
	act(AT_BLUE, "$N is in AWE of $n!",  ch, NULL, victim, TO_NOTVICT );
    }
    return;
}


void spell_biofeedback ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
   
    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 8 );
    af.location  = APPLY_IMM_SCRATCH;
    af.modifier  = -8;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_IMM_SLASH;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_IMM_BASH;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_IMM_PIERCE;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_WHITE, "You are surrounded by a white aura.\n\r", victim );
    act(AT_WHITE, "$n is surrounded by a white aura.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_crystal_flesh ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
   
    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 8 );
    af.location  = APPLY_IMM_SCRATCH;
    af.modifier  = -20;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_IMM_SLASH;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_IMM_BASH;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_IMM_PIERCE;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_WHITE, "Your flesh crystallizes.\n\r", victim );
    act(AT_WHITE, "$n's becomes crystallized.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_cell_adjustment ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int SkNum;

    if ( is_affected( victim, gsn_poison ) )
    {
	affect_strip( victim, gsn_poison );
	send_to_char(AT_BLUE, "A warm feeling runs through your body.\n\r", victim );
	act(AT_BLUE, "$N looks better.", ch, NULL, victim, TO_NOTVICT );
    }

    if ( is_affected( victim, gsn_plague ) )
    {
    affect_strip( victim, gsn_plague );
    REMOVE_BIT( victim->affected_by2, AFF_DISEASED );
    }

    SkNum = skill_lookup("curse");
  
    if ( is_affected( victim, SkNum  ) ) 
    {
	affect_strip( victim, SkNum  );
	send_to_char(AT_BLUE, "You feel better.\n\r", victim );
    }	
    send_to_char(AT_BLUE, "Ok.\n\r", ch );
    return;
}

void spell_chaosfield( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_CHAOS ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 6 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_CHAOS;
    affect_to_char( victim, &af );

    send_to_char(AT_YELLOW, "You call forth an instance of chaos from the order around you.\n\r", victim );
    act(AT_YELLOW, "$n's body is veiled in an instance or pure chaos.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_bladebarrier( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED2( victim, AFF_BLADE ) )
	return;

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 6 );
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_BLADE;
    affect_to_char2( victim, &af );

    send_to_char(AT_GREY, "You bring forth thousands of tiny spinning blades about your body.\n\r", victim );
    act(AT_GREY, "$n's body is surrounded by thousands of spinning blades.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_lighten_mind( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED2( victim, AFF_HALLUCINATING ) )
	return;
    
    if ( IS_AFFECTED( victim, AFF_BLIND ) || saves_spell( level, victim ))
    {
	send_to_char(AT_BLUE, "You have failed.\n\r", ch );
	return;
    }

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 6 );
    af.location  = APPLY_INT;
    af.modifier  = -4;
    af.bitvector = AFF_HALLUCINATING;
    affect_to_char2( victim, &af );

    act(AT_WHITE, "&.Thou&.sand&.s &.of &.danci&.ng &.ligh&.ts &.surr&.ound &.you&.!&w", victim, NULL, victim, TO_VICT );
    act(AT_GREY, "&W$n's &.body &.is &.surr&.ounded &.by d&.anci&.ng l&.ights.", victim, NULL, NULL, TO_ROOM );
    return;
}

void spell_taint_mind( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_INSANE) )
	return;
    
    if ( saves_spell( level, victim ))
    {
	send_to_char(AT_BLUE, "You have failed.\n\r", ch );
	return;
    }

    af.type      = sn;
    af.level	 = level;
    af.duration  = number_fuzzy( level / 6 );
    af.location  = APPLY_INT;
    af.modifier  = -6;
    af.bitvector = AFF_INSANE;
    affect_to_char( victim, &af );

    act(AT_WHITE, "&.Thou&.sand&.s &.of &.danci&.ng &.ligh&.ts &.surr&.ound &.you&.!&w", victim, NULL, victim, TO_VICT );
    act(AT_GREY, "&W$n's &.body &.is &.surr&.ounded &.by d&.anci&.ng l&.ights.", victim, NULL, NULL, TO_ROOM );
    return;
}


void spell_combat_mind ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
    {
	if ( victim == ch )
	  send_to_char(AT_BLUE, "You already understand battle tactics.\n\r",
		       victim );
	else
	  act(AT_BLUE, "$N already understands battle tactics.",
	      ch, NULL, victim, TO_CHAR );
	return;
    }

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level + 3;
    af.location	 = APPLY_HITROLL;
    af.modifier	 = level / 5;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	 = APPLY_DEX;
    af.modifier	 = 2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    if ( victim != ch )
	send_to_char(AT_BLUE, "OK.\n\r", ch );
    send_to_char(AT_BLUE, "You gain a keen understanding of battle tactics.\n\r",
		 victim );
    return;
}

void spell_complete_healing ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    victim->hit = MAX_HIT(victim);
    update_pos( victim, ch );
    if ( ch != victim )
        send_to_char(AT_BLUE, "Ok.\n\r", ch );
    send_to_char(AT_BLUE, "Ahhhhhh...You feel MUCH better!\n\r", victim );
    send_to_char(AT_BLUE, "Have a nice day.\n\r", victim);
    return;
}

void spell_create_sound ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    char       buf1    [ MAX_STRING_LENGTH ];
    char       buf2    [ MAX_STRING_LENGTH ];
    char       speaker [ MAX_INPUT_LENGTH  ];

    target_name = one_argument( target_name, speaker );

    sprintf( buf1, "%s says '%s'.\n\r", speaker, target_name );
    sprintf( buf2, "Someone makes %s say '%s'.\n\r", speaker, target_name );
    buf1[0] = UPPER( buf1[0] );

    for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
    {
	if ( !is_name( NULL, speaker, vch->name ) )
	    send_to_char(AT_RED, saves_spell( level, vch ) ? buf2 : buf1, vch );
    }
    return;
}



void spell_death_field ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *vch;
    int        dam;
    int        hpch;
    send_to_char(AT_DGREY, "A black haze emanates from you!\n\r", ch );
    act (AT_DGREY, "A black haze emanates from $n!", ch, NULL, ch, TO_ROOM );

    for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
    {
	if ( vch->deleted )
	  continue;
	if ( IS_NPC( ch ) )
	  continue;
    	if ( ch == vch )
	  continue;

	if ( !IS_NPC( ch ) ? IS_NPC( vch ) : IS_NPC( vch ) )
	{
	    hpch = URANGE( 10, ch->hit, 999 );
	    if ( !saves_spell( level, vch )
		&& (   level <= vch->level + 5
		    && level >= vch->level - 5 ) )
            {
		send_to_char(AT_DGREY, "The haze envelops you!\n\r", vch );
		act(AT_DGREY, "The haze envelops $N!", 
		    ch, NULL, vch, TO_NOTVICT );
		dam = 4; /* Enough to compensate for sanct. and prot. */
		vch->hit = 1;
		damage( ch, vch, dam, sn, DAM_UNHOLY, TRUE  );
		update_pos( vch, ch );
            }
	    else
	    {
    		dam = number_range( hpch / 16 + 1, hpch / 8 );
		dam = sc_dam( ch, dam, DAM_UNHOLY );
   		damage( ch, vch, dam, sn, DAM_UNHOLY, TRUE  );
	    }
	}
    }
    return;
}


void spell_disrupt( int sn, int level, CHAR_DATA *ch, void *vo )
{
  CHAR_DATA *victim = (CHAR_DATA *) vo;
  CHAR_DATA *vch;
  AFFECT_DATA af;
  bool negchar = FALSE; /* Is a negative value good..? */
  int val = 0;

  af.type = sn;
  af.duration = (level * 2) / 3;
  af.level = level;
  af.bitvector = 0;

  while ( !val )
  {
    af.location = number_range( 1, 11 );

    switch (af.location)
    {
    case APPLY_STR:
    case APPLY_DEX:
    case APPLY_INT:
    case APPLY_WIS:
    case APPLY_CON:
    case APPLY_AGI:
    case APPLY_CHA:
      val = (level / 34) + 1;
      break;
    case APPLY_MANA:
      val = ((level * 3) / 2) + 1;
      break;
    case APPLY_MDAMP:
    case APPLY_PDAMP:
      val = ((level * 3) / 2) + 1;
      break;
    case APPLY_HITROLL:
    case APPLY_DAMROLL:
      val = (level / 2) + 1;
      break;
    }
  }

  for ( af.modifier = number_range( -val, val ); af.modifier == 0;
        af.modifier = number_range( -val, val ) );

  if ( negchar )
  {
    if ( af.modifier < 0 )
      vch = ch;
    else
      vch = victim;
  }
  else
  {
    if ( af.modifier < 0 )
      vch = victim;
    else
      vch = ch;
  }
  if ( !is_affected( vch, sn ) && (vch != victim ||
				   !saves_spell( level,  victim )) &&
       number_bits( 8 ) == 0 )
  {
    affect_to_char( vch, &af );
    if ( ch == vch )
    {
      act( AT_PINK, "You disrupt yourself!", ch, NULL, NULL, TO_CHAR );
      act( AT_PINK, "$n disrupts $mself!", ch, NULL, NULL, TO_ROOM );
    }
    else
    {
      act( AT_PURPLE, "You disrupt $N!", ch, NULL, victim, TO_CHAR );
      act( AT_PURPLE, "$n disrupts $N!", ch, NULL, victim, TO_NOTVICT );
      act( AT_PURPLE, "$n disrupts you!", ch, NULL, victim, TO_VICT );
      switch( af.location )
      {
      case APPLY_MANA:
	vch->mana = URANGE( 0, vch->mana, MAX_MANA(vch));
	break;
      }
    }
  }

  damage( ch, victim, number_range( (level * 5) / 3, level * 8 ), sn, DAM_DYNAMIC, TRUE  );
  return;
}



void spell_displacement ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
	return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level - 4;
    af.location	 = APPLY_PDAMP;
    af.modifier	 = 5;
    af.bitvector   = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_GREY, "Your form shimmers, and you appear displaced.\n\r",
		 victim );
    act(AT_GREY, "$N shimmers and appears in a different location.",
	ch, NULL, victim, TO_NOTVICT );
    return;
}



void spell_domination ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( victim == ch )
    {
	send_to_char(C_DEFAULT, "Dominate yourself?  You're weird.\n\r", ch );
	return;
    }
    if ( !IS_NPC( victim ) )
       return;
       
    if (   IS_AFFECTED( victim, AFF_CHARM )
	|| IS_AFFECTED( ch,     AFF_CHARM )
	|| level < victim->level
	|| saves_spell( level, victim ) )
	return;

    if ( victim->master )
        stop_follower( victim );
    add_follower( victim, ch );

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = number_fuzzy( level / 4 );
    af.location	 = APPLY_NONE;
    af.modifier	 = 0;
    af.bitvector = AFF_CHARM;
    affect_to_char( victim, &af );

    act(AT_BLUE, "Your will dominates $N!", ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "Your will is dominated by $n!", ch, NULL, victim, TO_VICT );
    return;
}



void spell_ectoplasmic_form ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_PASS_DOOR ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = number_fuzzy( level / 4 );
    af.location	 = APPLY_NONE;
    af.modifier	 = 0;
    af.bitvector = AFF_PASS_DOOR;
    affect_to_char( victim, &af );

    send_to_char(AT_GREY, "You turn translucent.\n\r", victim );
    act(AT_GREY, "$n turns translucent.", victim, NULL, NULL, TO_ROOM );
    return;
}



void spell_ego_whip ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) || saves_spell( level, victim ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level;
    af.location	 = APPLY_HITROLL;
    af.modifier	 = -10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	 = APPLY_DEX;
    af.modifier	 = -2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location	 = APPLY_AGI;
    af.modifier	 = -2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    act(AT_BLUE, "You ridicule $N about $S childhood.", ch, NULL, victim, TO_CHAR    );
    send_to_char(AT_BLUE, "Your ego takes a beating.\n\r", victim );
    act(AT_BLUE, "$N's ego is crushed by $n!",          ch, NULL, victim, TO_NOTVICT );

    return;
}



void spell_energy_containment ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level / 2 + 7;
    af.modifier	 = -10;
    af.location    = APPLY_IMM_POSITIVE;
    af.bitvector   = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "You can now absorb some forms of energy.\n\r", ch );
    return;
}



void spell_enhance_armor (int sn, int level, CHAR_DATA *ch, void *vo )
{
    OBJ_DATA    *obj = (OBJ_DATA *) vo;
    AFFECT_DATA *paf;

    if ( obj->item_type != ITEM_ARMOR
	|| IS_OBJ_STAT( obj, ITEM_MAGIC )
	|| obj->affected )
    {
	send_to_char(AT_BLUE, "That item cannot be enhanced.\n\r", ch );
	return;
    }

    if ( !affect_free )
    {
	paf	    = alloc_perm( sizeof( *paf ) );
    }
    else
    {
	paf         = affect_free;
	affect_free = affect_free->next;
    }

    paf->type	   = sn;
    paf->duration  = -1;
    paf->location  = APPLY_PDAMP;
    paf->bitvector = 0;
    paf->next	 = obj->affected;
    obj->affected  = paf;

    if ( number_percent() < ch->pcdata->learned[sn]/2
	+ 3 * ( ch->level - obj->level ) )

    /* Good enhancement */
    {
	paf->modifier   = -level / 5;
       
	send_to_char(AT_BLUE, "Ok.\n\r", ch );
    }
    else
    /* Bad Enhancement ... opps! :) */
    {
	paf->modifier   = level / 8;
	obj->cost.gold = obj->cost.silver = obj->cost.copper = 0;
	SET_BIT( obj->extra_flags, ITEM_NODROP );
	act(AT_DGREY, "$p turns black.", ch, obj, NULL, TO_CHAR );
    }

    return;
}



void spell_enhanced_strength ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level;
    af.location	 = APPLY_STR;
    af.modifier	 = 1 + ( level >= 15 ) + ( level >= 25 );
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "You are HUGE!\n\r", victim );
    return;
}



void spell_flesh_armor ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level;
    af.location	 = APPLY_IMM_BASH;
    af.modifier	 = -10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "Your flesh turns to steel.\n\r", victim );
    act(AT_BLUE, "$N's flesh turns to steel.", ch, NULL, victim, TO_NOTVICT);
    return;
}

void spell_inertial_barrier ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *gch;
    AFFECT_DATA af;

    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
	if ( !is_same_group( gch, ch ) || IS_AFFECTED2( gch, AFF_INERTIAL ) )
	    continue;

	act(AT_BLUE, "An inertial barrier forms around $n.", gch, NULL, NULL,
	    TO_ROOM );
	send_to_char(AT_BLUE, "An inertial barrier forms around you.\n\r", gch );

	af.type	 = sn;
      af.level     = level;
	af.duration  = 24;
	af.modifier  = 5;
	af.location  = APPLY_PDAMP;
	af.bitvector = AFF_INERTIAL;
	affect_to_char2( gch, &af );
    }
    return;
}


void spell_intellect_fortress ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *gch;
    AFFECT_DATA af;

    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
	if ( !is_same_group( gch, ch ) || is_affected( gch, sn ) )
	    continue;

	send_to_char(AT_BLUE, "A virtual fortress forms around you.\n\r", gch );
	act(AT_BLUE, "A virtual fortress forms around $N.", gch, NULL, gch, TO_ROOM );

	af.type      = sn;
      af.level     = level;
	af.duration  = 24;
	af.location  = APPLY_PDAMP;
	af.modifier  = 5;
	af.bitvector = 0;
	affect_to_char( gch, &af );
    }
    return;
}


void spell_holy_wrath ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *gch;
    AFFECT_DATA af;

    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
	if ( !is_same_group( gch, ch ) || is_affected( gch, sn ) )
	    continue;

	send_to_char(AT_BLUE, "The wrath of a vengeful God fills you!\n\r", gch );
	act(AT_BLUE, "$N is filled with holy wrath!", gch, NULL, gch, TO_ROOM );

	af.type	     = sn;
        af.level     = level;
	af.duration  = 5;
	af.location  = APPLY_DAMROLL;
	af.modifier  = 35;
	af.bitvector = 0;
	affect_to_char( gch, &af );
    }
    return;
}

void spell_lend_health ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int        hpch;

    if ( ch == victim )
    {
	send_to_char(AT_BLUE, "Lend health to yourself?  Easily done.\n\r", ch );
	return;
    }
    hpch = UMIN( 50, MAX_HIT(victim) - victim->hit );
    if ( hpch == 0 )
    {
	act(AT_BLUE, "Nice thought, but $N doesn't need healing.", ch, NULL,
	    victim, TO_CHAR );
	return;
    }
    if ( ch->hit-hpch < 50 )
    {
	send_to_char(AT_BLUE, "You aren't healthy enough yourself!\n\r", ch );
	return;
    }
    if ( IS_NPC(victim) 
     && !IS_SET(victim->act, ACT_UNDEAD))
    {
    victim->hit += hpch;
    send_to_char( AT_WHITE, "Ok.\n\r", ch );
    }
    else 
     damage( ch, victim, hpch, sn, DAM_REGEN, TRUE  );
    ch->hit     -= hpch;
    update_pos( victim, ch);
    update_pos( ch, ch );

    act(AT_BLUE, "You lend some of your health to $N.", ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n lends you some of $s health.",     ch, NULL, victim, TO_VICT );

    return;
}



void spell_levitation ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( IS_AFFECTED( victim, AFF_FLYING ) )
        return; 

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level + 3;
    af.location	 = APPLY_NONE;
    af.modifier	 = 0;
    af.bitvector = AFF_FLYING;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "Your feet rise off the ground.\n\r", victim );
    act(AT_BLUE, "$n's feet rise off the ground.", victim, NULL, NULL, TO_ROOM );
    return;
}



void spell_mental_barrier ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = 24;
    af.location	 = APPLY_IMM_DYNAMIC;
    af.modifier	 = -15;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "You erect a mental barrier around yourself.\n\r",
		 victim );
    return;
}


void spell_sonicvibe( int sn, int level, CHAR_DATA *ch, void *vo )
{
                 
                 CHAR_DATA *victim       = (CHAR_DATA *) vo;
    static const int        dam_each [ ] =
    {
	  0,
	  0,   0,   0,   0,   0,        0,   0,   0,   0,   0,
	  0,   0,   0,   0,   0,        0,  45,  50,  55,  60,
	 64,  68,  72,  76,  80,       82,  84,  86,  88,  90,
	 92,  94,  96,  98, 100,      102, 104, 106, 108, 100,
	112, 114, 116, 118, 120,      122, 124, 126, 128, 130,
	132, 134, 136, 138, 140,      142, 144, 146, 148, 150,
	152, 154, 156, 158, 160,      162, 164, 166, 168, 170,
	182, 184, 186, 188, 190,      192, 194, 196, 198, 200,
	202, 204, 206, 208, 210,      212, 214, 216, 218, 220,
	222, 224, 226, 228, 230,      232, 234, 236, 238, 240
    };
		 int        dam;

    level    = UMIN( level, sizeof( dam_each ) / sizeof( dam_each[0] ) - 1 );
    level    = UMAX( 0, level );
    dam	     = number_range( dam_each[level] / 2, dam_each[level] );
    dam = sc_dam( ch, dam, DAM_DYNAMIC );
    if ( saves_spell( level, victim ) )
        dam /= 2;
    damage( ch, victim, dam, sn, DAM_DYNAMIC, TRUE  );
    return;
}

void spell_psychic_drain ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) || saves_spell( level, victim ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level / 2;
    af.location	 = APPLY_STR;
    af.modifier	 = -1 - ( level >= 10 ) - ( level >= 20 ) - ( level >= 30 );
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_GREEN, "You feel drained.\n\r", victim );
    act(AT_BLUE, "$n appears drained of strength.", victim, NULL, NULL, TO_ROOM );
    return;
}



void spell_psychic_healing ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int heal;

    heal = dice( 3, 6 ) + 2 * level / 3 ;
    victim->hit = UMIN( victim->hit + heal, MAX_HIT(victim) );
    update_pos( victim, ch );

    send_to_char(AT_BLUE, "You feel better!\n\r", victim );
    return;
}



void spell_share_strength ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( victim == ch )
    {
	send_to_char(AT_BLUE, "You can't share strength with yourself.\n\r", ch );
	return;
    }
    if ( is_affected( victim, sn ) )
    {
	act(AT_BLUE, "$N already shares someone's strength.", ch, NULL, victim,
	    TO_CHAR );
	return;
    }
    if ( get_curr_str( ch ) <= 5 )
    {
	send_to_char(AT_BLUE, "You are too weak to share your strength.\n\r", ch );
	return;
    }

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level;
    af.location	 = APPLY_STR;
    af.modifier	 =  1 + ( level >= 20 ) + ( level >= 30 );
    af.bitvector = 0;
    affect_to_char( victim, &af );
    
    af.modifier	 = -1 - ( level >= 20 ) - ( level >= 30 );
    af.bitvector = 0;
    affect_to_char( ch,     &af );

    act(AT_BLUE, "You share your strength with $N.", ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n shares $s strength with you.",  ch, NULL, victim, TO_VICT );
    return;
}

void spell_thought_shield ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
        return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = level;
    af.location	 = APPLY_PDAMP;
    af.modifier	 = 5;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_BLUE, "You have created a shield around yourself.\n\r", ch );
    return;
}

void spell_raise_skeleton(int sn, int level, CHAR_DATA *ch, void *vo)
{
  OBJ_DATA      *obj = (OBJ_DATA *) vo;
  OBJ_DATA      *obj_next;
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;

  if ( obj->item_type != ITEM_SKELETON )
  {
   send_to_char(AT_BLUE, "You need a skeleton.\n\r", ch );
   return;
  }

  obj_next = obj->next;

  if (obj->deleted)
   return;

  extract_obj(obj);

  if(ch->summon_timer > 0)
  {
   send_to_char(AT_BLUE,
    "You casted the spell, but nothing appears.\n\r", ch);
   return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_SKELETON));
  mob->level = URANGE(15, level, 55) - 5;
  mob->perm_hit = mob->level * 20 + dice(1,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level;
  ch->summon_timer = 2;
  char_to_room(mob, ch->in_room);
  act(AT_BLUE, "You raise $N.", ch, NULL, mob, TO_CHAR);

  act(AT_GREEN, "$n raises $N.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_BLUE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_BLUE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    fch->fighting = mob;
    mob->fighting = fch;
    set_fighting(mob, fch); 
    set_fighting(fch, mob);
  }

 return;
}

void spell_summon_mummy(int sn, int level, CHAR_DATA *ch, void *vo)
{
  OBJ_DATA      *obj = (OBJ_DATA *) vo;
  OBJ_DATA      *obj_next;
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;

  if ( obj->item_type != ITEM_MUMMY )
  {
   send_to_char(AT_BLUE, "You need a mummified corpse.\n\r", ch );
   return;
  }

  obj_next = obj->next;

  if (obj->deleted)
   return;

  extract_obj(obj);

  if(ch->summon_timer > 0)
  {
   send_to_char(AT_BLUE,
    "You casted the spell, but nothing appears.\n\r", ch);
   return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_MUMMY));
  mob->level = URANGE(15, level, 55) - 5;
  mob->perm_hit = mob->level * 20 + dice(1,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level + dice( 5, 3 );
  ch->summon_timer = 10;
  char_to_room(mob, ch->in_room);
  act(AT_BLUE, "You raise $N.", ch, NULL, mob, TO_CHAR);

  act(AT_GREEN, "$n raises $N.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_BLUE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_BLUE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    fch->fighting = mob;
    mob->fighting = fch;
    set_fighting(mob, fch); 
    set_fighting(fch, mob);
  }

 return;
}

void spell_raise_lich(int sn, int level, CHAR_DATA *ch, void *vo)
{
  OBJ_DATA      *obj = (OBJ_DATA *) vo;
  OBJ_DATA      *obj_next;
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;

  if ( obj->item_type != ITEM_SKELETON )
  {
   send_to_char(AT_BLUE, "You need a skeleton.\n\r", ch );
   return;
  }

  obj_next = obj->next;

  if (obj->deleted)
   return;

  extract_obj(obj);

  if(ch->summon_timer > 0)
  {
   send_to_char(AT_BLUE,
    "You casted the spell, but nothing appears.\n\r", ch);
   return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_LICH));
  mob->level = URANGE(15, level, 55) - 5;
  mob->perm_hit = mob->level * 40 + dice(2,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level * 2;
  ch->summon_timer = 20;
  char_to_room(mob, ch->in_room);
  act(AT_BLUE, "You raise $N.", ch, NULL, mob, TO_CHAR);

  act(AT_GREEN, "$n raises $N.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_BLUE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_BLUE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    fch->fighting = mob;
    mob->fighting = fch;
    set_fighting(mob, fch); 
    set_fighting(fch, mob);
  }

 return;
}

void spell_summon_pharaoh(int sn, int level, CHAR_DATA *ch, void *vo)
{
  OBJ_DATA      *obj = (OBJ_DATA *) vo;
  OBJ_DATA      *obj_next;
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;

  if ( obj->item_type != ITEM_MUMMY )
  {
   send_to_char(AT_BLUE, "You need a mummified corpse.\n\r", ch );
   return;
  }

  obj_next = obj->next;

  if (obj->deleted)
   return;

  extract_obj(obj);

  if(ch->summon_timer > 0)
  {
   send_to_char(AT_BLUE,
    "You casted the spell, but nothing appears.\n\r", ch);
   return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_PHARAOH));
  mob->level = URANGE(15, level, 55) - 5;
  mob->perm_hit = mob->level * 50 + dice(3,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level* 2;
  ch->summon_timer = 20;
  char_to_room(mob, ch->in_room);
  act(AT_BLUE, "You raise $N.", ch, NULL, mob, TO_CHAR);

  act(AT_GREEN, "$n raises $N.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_BLUE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_BLUE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    fch->fighting = mob;
    mob->fighting = fch;
    set_fighting(mob, fch); 
    set_fighting(fch, mob);
  }

 return;
}

void spell_raise_bloodborn(int sn, int level, CHAR_DATA *ch, void *vo)
{
  OBJ_DATA      *obj = (OBJ_DATA *) vo;
  OBJ_DATA      *obj_next;
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;

  if ( obj->item_type != ITEM_BLOOD )
  {
   send_to_char(AT_BLUE, "You need a pool of blood.\n\r", ch );
   return;
  }

  obj_next = obj->next;

  if (obj->deleted)
   return;

  extract_obj(obj);

  if(ch->summon_timer > 0)
  {
   send_to_char(AT_BLUE,
    "You casted the spell, but nothing appears.\n\r", ch);
   return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_BLOODBORN));
  mob->level = URANGE(15, level, 55) - 5;
  mob->perm_hit = MAX_HIT(ch);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level;
  ch->summon_timer = level;
  char_to_room(mob, ch->in_room);
  act(AT_BLUE, "You raise $N.", ch, NULL, mob, TO_CHAR);

  act(AT_GREEN, "$n raises $N.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  af.type      = skill_lookup("siamese soul");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_SIAMESE;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_BLUE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_BLUE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    fch->fighting = mob;
    mob->fighting = fch;
    set_fighting(mob, fch); 
    set_fighting(fch, mob);
  }

 return;
}

/* XORPHOX summon mobs */
void spell_summon_swarm(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;
  char buf [ MAX_STRING_LENGTH ];
  int mana;

  if(ch->summon_timer > 0)
  {
    send_to_char(AT_BLUE,
     "You casted the spell, but nothing appears.\n\r", ch);
    return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_INSECTS));
  mob->level = URANGE(15, level, 55) - 5;
  mob->perm_hit = mob->level * 20 + dice(1,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level;
  ch->summon_timer = 10;
  char_to_room(mob, ch->in_room);
  act(AT_BLUE, "You summon $N.", ch, NULL, mob, TO_CHAR);
  mana = level * 2;

  if ( MT( ch ) < mana )
  {
   sprintf( buf, "&RYou don't have enough mana to bind $N!" );
   act(AT_WHITE, buf, ch, NULL, mob, TO_CHAR );
   extract_char( mob, TRUE );
   return;
  }    

  MT( ch ) -= mana;
  act(AT_GREEN, "$n summons $N.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_BLUE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_BLUE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    fch->fighting = mob;
    mob->fighting = fch;
    set_fighting(mob, fch); 
    set_fighting(fch, mob);
  }
  return;
}

void spell_carbon_copy(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *fch;
  CHAR_DATA *mob;
  AFFECT_DATA af;
  char buf [ MAX_STRING_LENGTH ];
  int mana;

  if ( IS_NPC(ch) )
   return;

  if(ch->summon_timer > 0)
  {
    send_to_char(AT_BLUE,
     "You casted the spell, but nothing appears.\n\r", ch);
    return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_COPY));
  sprintf(buf, "%s", ch->name);
  free_string( mob->name );
  mob->name = str_dup(buf);
  free_string( mob->short_descr );
  mob->short_descr = str_dup(buf);
  sprintf(buf, "%s is here.\n\r", ch->name );
  free_string( mob->long_descr );
  mob->long_descr = str_dup(buf);
  mob->level = (ch->level - 10);
  mob->perm_hit = ch->perm_hit;
  mob->hit = MAX_HIT(ch);
  mob->perm_str = ch->perm_str;
  mob->perm_wis = ch->perm_wis;
  mob->perm_int = ch->perm_int;
  mob->perm_con = ch->perm_con;
  mob->perm_dex = ch->perm_dex;
  mob->perm_agi = ch->perm_agi;
  mob->summon_timer = level*10;
  ch->summon_timer = 10;
  char_to_room(mob, ch->in_room);
  act(AT_BLUE, "You create a copy of yourself.", ch, NULL, mob, TO_CHAR);
  mana = level * 2;
  if ( MT( ch ) < mana )
    {
    sprintf( buf, "%sYou don't have enough %s to bind $N!",
	   "&R", "mana" );
    act(AT_WHITE, buf, ch, NULL, mob, TO_CHAR );
    extract_char( mob, TRUE );
    return;
    }    
  MT( ch ) -= mana;
  act(AT_GREEN, "$n creates a clone of $mself.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_BLUE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_BLUE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    set_fighting(mob, fch);
    set_fighting(fch, mob);
  }

  return;
}

void spell_sphere_of_solitude(int sn, int level, CHAR_DATA *ch, void *vo )
{
  CHAR_DATA *vch;
  CHAR_DATA *vch_next;
  OBJ_DATA *sphere;
  EXIT_DATA *pexit;
  CHAR_DATA *victim = (CHAR_DATA *) vo;
  int door;

  for( door = 0; door < 6;door++ )
  {
   if ( (pexit = ch->in_room->exit[door]) )
    break;
   else
    continue;
  }

 if ( !(pexit) )
 {
  send_to_char( AT_YELLOW, "There is no proper exit.\n\r", ch );
  return;
 }

 sphere = create_object( get_obj_index( OBJ_VNUM_SPHERE_OF_SOLITUDE ), 0 );
 sphere->timer = 10;
 obj_to_room( sphere, ch->in_room );

 act(AT_BLUE, "You motion and everyone except $N is thrown from the room!",
  ch, sphere, victim, TO_CHAR );
 act(AT_BLUE, "$n motions and everyone except yourself is thrown from the room!",
  ch, sphere, victim, TO_VICT );

  for ( vch = ch->in_room->people; vch; vch = vch_next )
  {
   vch_next = vch->next_in_room;
   if ( vch->deleted )
    continue;
   if ( is_same_group( ch, vch ) )
    continue;
   if ( ch == vch )
    continue;
   if ( victim == vch )
    continue;

  char_from_room(vch);
  char_to_room(vch, pexit->to_room);
  act(AT_BLUE, "$n comes flying into the room.", vch, NULL, NULL, TO_ROOM);
  act(AT_BLUE, "$n motions and you are thrown from the room!",
   ch, sphere, vch, TO_VICT );
 }
 return; 
}

void spell_forcefield(int sn, int level, CHAR_DATA *ch, void *vo )
{
  EXIT_DATA *pexit = (EXIT_DATA *) vo;
  EXIT_DATA *revexit;
  EXIT_AFFECT_DATA exitaf;  
  ROOM_INDEX_DATA *room;
  int door;

  exitaf.type		= EXIT_FORCEFIELD;
  exitaf.duration	= 7;
  exitaf.location	= sn;
  exitaf.exit		= pexit;
  exit_affect_to_room(pexit, &exitaf);

  room = pexit->to_room;
  
  for( door = 0; door < 6;door++)
   if( (revexit = room->exit[door]) )
    if ( revexit->to_room == ch->in_room )
     break;

  if ( revexit )
  {
   exitaf.type		= EXIT_FORCEFIELD;
   exitaf.duration	= 7;
   exitaf.location	= sn;
   exitaf.exit		= revexit;
   exit_affect_to_room(revexit, &exitaf);
  }
  
  act(AT_BLUE, "You motion and a barrier forms, barring entry.",
   ch, NULL, NULL, TO_CHAR );
  act(AT_BLUE, "$n motions and a barrier forms, barring entry.",
   ch, NULL, NULL, TO_ROOM );

 return;
}

void spell_army_of_illusion(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *fch;
  CHAR_DATA *mob;
  CHAR_DATA *vch;
  AFFECT_DATA af;
  char buf[ MAX_STRING_LENGTH ];
  int mana;

  if ( IS_NPC(ch) )
   return;

  if(ch->summon_timer > 0)
  {
    send_to_char(AT_BLUE,
     "You casted the spell, but nothing appears.\n\r", ch);
    return;
  }

  mana = level * 2;

  if ( MT( ch ) < mana )
  {
   sprintf( buf, "You don't have enough mana to bind your army!" );
   act(AT_YELLOW, buf, ch, NULL, NULL, TO_CHAR );
   return;
  }    

  MT( ch ) -= mana;

  for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
  {
        if ( vch->deleted )
	    continue;
        if ( ch == vch )
            continue;

  mob = create_mobile(get_mob_index(MOB_VNUM_COPY));
  sprintf(buf, "%s", vch->name);
  free_string( mob->name );
  mob->name = str_dup(buf);

  if ( IS_NPC(vch) )
   sprintf(buf, "%s", vch->short_descr);
  free_string( mob->short_descr );
  mob->short_descr = str_dup(buf);

  if ( !IS_NPC(vch) )
   sprintf(buf, "%s is here.\n\r", vch->name );
  else
   sprintf(buf, "%s", vch->long_descr);
  free_string( mob->long_descr );
  mob->long_descr = str_dup(buf);

  mob->level = vch->level;
  mob->perm_hit = vch->perm_hit;
  mob->sex = vch->sex;
  mob->hit = 1;
  mob->perm_str = vch->perm_str;
  mob->perm_wis = vch->perm_wis;
  mob->perm_int = vch->perm_int;
  mob->perm_con = vch->perm_con;
  mob->perm_dex = vch->perm_dex;
  mob->perm_agi = vch->perm_agi;
  mob->summon_timer = level*10;
  SET_BIT( mob->act, ACT_ILLUSION );
  char_to_room(mob, ch->in_room);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if ( ch->fighting )
  {
    act(AT_BLUE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_BLUE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );
   fch = ch->fighting;
   do_rescue( mob, fch->name );
  }
 }

  ch->summon_timer = 15;
  act(AT_BLUE, "You create a copy of everyone in the room!", ch, NULL, ch, TO_CHAR);
  act(AT_BLUE, "Suddenly, everyone splits into two people!",
   ch, NULL, ch, TO_ROOM);
  return;
}

void spell_summon_pack(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;
  char buf [ MAX_STRING_LENGTH ];
  int mana;

  if(ch->summon_timer > 0)
  {
    send_to_char(AT_BLUE,
     "You casted the spell, but nothing appears.\n\r", ch);
    return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_WOLFS));
  mob->level = URANGE(31, level, 90) - 5;
  mob->perm_hit = mob->level * 20 + dice(1,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level;
  ch->summon_timer = 10;
  char_to_room(mob, ch->in_room);
  act(AT_GREEN, "You summon $N.", ch, NULL, mob, TO_CHAR);
  mana = level * 2;
  if ( MT( ch ) < mana )
    {
    sprintf( buf, "%sYou don't have enough %s to bind $N!",
	   "&R", "mana" );
    act(AT_WHITE, buf, ch, NULL, mob, TO_CHAR );
    extract_char( mob, TRUE );
    return;
    }    
  MT( ch ) -= mana;
  act(AT_GREEN, "$N comes to $n aid.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_BLUE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_BLUE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    set_fighting(mob, fch);
    set_fighting(fch, mob);
  }
  return;
}

void spell_summon_demon(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;
  char buf [ MAX_STRING_LENGTH ];
  int mana;
  if(ch->summon_timer > 0)
  {
    send_to_char(AT_RED,
     "You casted the spell, but nothing appears.\n\r", ch);
    return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_DEMON));
  mob->level = URANGE(51, level, 100) - 5;
  mob->perm_hit = mob->level * 20 + dice(1,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level*2;
  ch->summon_timer = 15;
  char_to_room(mob, ch->in_room);
  act(AT_RED, "You summon $N from the abyss.", ch, NULL, mob, TO_CHAR);
  mana = level * 2;
  if ( MT( ch ) < mana )
    {
    sprintf( buf, "%sYou don't have enough %s to bind $N!",
	   "&R", "mana" );
    act(AT_WHITE, buf, ch, NULL, mob, TO_CHAR );
    extract_char( mob, TRUE );
    return;
    }    
  MT( ch ) -= mana;
  act(AT_RED, "$n summons $N from the abyss.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_RED, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_RED, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    set_fighting(mob, fch);
    set_fighting(fch, mob);
  }
  return;
}

void spell_summon_angel(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;
  char buf[ MAX_STRING_LENGTH ];
  int mana;
  if(ch->summon_timer > 0)
  {
    send_to_char(AT_WHITE,
     "You casted the spell, but nothing appears.\n\r", ch);
    return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_ANGEL));
  mob->level = URANGE(51, level, 100) - 5;
  mob->perm_hit = mob->level * 20 + dice(10,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level*2;
  ch->summon_timer = 16;
  char_to_room(mob, ch->in_room);
  act(AT_WHITE, "You summon $N from heaven.", ch, NULL, mob, TO_CHAR);
  mana = level * 2;
  if ( MT( ch ) < mana )
    {
    sprintf( buf, "%sYou don't have enough %s to bind $N!",
	   "&R", "mana" );
    act(AT_WHITE, buf, ch, NULL, mob, TO_CHAR );
    extract_char( mob, TRUE );
    return;
    }    
  MT( ch ) -= mana;
  act(AT_WHITE, "$n calls forth $N from Heaven.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_WHITE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_WHITE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    set_fighting(mob, fch);
    set_fighting(fch, mob);
  }
  return;
}

void spell_summon_shadow(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;
  char buf[MAX_STRING_LENGTH];
  int mana;
  if(ch->summon_timer > 0)
  {
    send_to_char(AT_WHITE,
     "You casted the spell, but nothing appears.\n\r", ch);
    return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_SHADOW));
  mob->level = URANGE(51, level, 100) - 20;
  mob->perm_hit = mob->level * 20 + dice(10,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level*2;
  ch->summon_timer = 16;
  char_to_room(mob, ch->in_room);
  act(AT_GREY, "You summon $N from the shadow plane.", ch, NULL, mob, TO_CHAR);
  mana = level * 2;
  if ( MT( ch ) < mana )
    {
    sprintf( buf, "%sYou don't have enough %s to bind $N!",
	   "&R", "mana" );
    act(AT_WHITE, buf, ch, NULL, mob, TO_CHAR );
    extract_char( mob, TRUE );
    return;
    }    
  MT( ch ) -= mana;
  act(AT_GREY, "$n calls forth $N from the shadow plane.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_WHITE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_WHITE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    set_fighting(mob, fch);
    set_fighting(fch, mob);
  }
  return;
}


void spell_summon_trent(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;
  char buf[MAX_STRING_LENGTH];
  int mana;
  if(ch->summon_timer > 0)
  {
    send_to_char(AT_WHITE,
     "You casted the spell, but nothing appears.\n\r", ch);
    return;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_TRENT));
  mob->level = URANGE(51, level, 100) - 10;
  mob->perm_hit = mob->level * 20 + dice(20,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level*2;
  ch->summon_timer = 16;
  char_to_room(mob, ch->in_room);
  act(AT_ORANGE, "You summon $N from the plane of nature.", ch, NULL, mob, TO_CHAR);
  mana = level * 2;
  if ( MT( ch ) < mana )
    {
    sprintf( buf, "%sYou don't have enough %s to bind $N!",
	   "&R", "mana" );
    act(AT_WHITE, buf, ch, NULL, mob, TO_CHAR );
    extract_char( mob, TRUE );
    return;
    }    
  MT( ch ) -= mana;
  act(AT_ORANGE, "$n calls forth $N from the plane of nature.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_WHITE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_WHITE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    set_fighting(mob, fch);
    set_fighting(fch, mob);
  }
  return;
}

void spell_summon_beast(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *mob;
  CHAR_DATA *fch;
  AFFECT_DATA af;
  char        buf[MAX_STRING_LENGTH];
  int	      mana;
  char       *beast;

  if(ch->summon_timer > 0)
  {
    send_to_char(AT_WHITE,
     "You casted the spell, but nothing appears.\n\r", ch);
    return;
  }
  switch (number_bits( 4 ) )
  {
    case 0: beast = "horse"; break;
    case 1: beast = "cow"; break;
    case 2: beast = "bear"; break;
    case 3: beast = "lion"; break;
    case 4: beast = "bobcat"; break;
    case 5: beast = "mongoose"; break;
    case 6: beast = "rattle snake"; break;
    case 7: beast = "monkey"; break;
    default: beast = "tigeress"; break;
  }

  mob = create_mobile(get_mob_index(MOB_VNUM_BEAST));
  sprintf(buf, mob->short_descr, beast);
  free_string( mob->short_descr );
  mob->short_descr = str_dup(buf);
  sprintf(buf, mob->long_descr, beast, ch->name);
  free_string( mob->long_descr );
  mob->long_descr = str_dup(buf);
  mob->level = URANGE(51, level, 100) - 20;
  mob->perm_hit = mob->level * 20 + dice(10,mob->level);
  mob->hit = MAX_HIT(mob);
  mob->summon_timer = level/10;
  ch->summon_timer = 16;
  char_to_room(mob, ch->in_room);
  act(AT_GREEN, "You call $N from the forests.", ch, NULL, mob, TO_CHAR);
  mana = level * 2;
  if ( MT( ch ) < mana )
    {
    sprintf( buf, "%sYou don't have enough %s to bind $N!",
	   "&R", "mana" );
    act(AT_WHITE, buf, ch, NULL, mob, TO_CHAR );
    extract_char( mob, TRUE );
    return;
    }    
  MT( ch ) -= mana;
  act(AT_GREEN, "$n calls forth $N from the forests.", ch, NULL, mob, TO_ROOM);

  mob->master = ch;
  mob->leader = ch;
  af.type      = skill_lookup("charm person");
  af.level     = level;
  af.duration  = -1;
  af.location  = 0;
  af.modifier  = 0;
  af.bitvector = AFF_CHARM;
  affect_to_char(mob, &af);

  if(ch->position == POS_FIGHTING)
  {
    act(AT_WHITE, "$n rescues you!", mob, NULL, ch, TO_VICT    );
    act(AT_WHITE, "$n rescues $N!",  mob, NULL, ch, TO_NOTVICT );

    fch = ch->fighting;
    stop_fighting(fch, FALSE );
    stop_fighting( ch, FALSE );
    set_fighting(mob, fch);
    set_fighting(fch, mob);
  }
  return;
}

void perm_spell(CHAR_DATA *victim, int sn)
{
  AFFECT_DATA *af;

  if(is_affected(victim, sn))
  {
    for(af = victim->affected; af != NULL; af = af->next)
    {
      if(af->type == sn)
      {
        af->duration = -1;
      }
    }
  }
  return;
}

int spell_duration(CHAR_DATA *victim, int sn)
{
  AFFECT_DATA *af;

  if(is_affected(victim, sn))
  {
    for(af = victim->affected; af != NULL; af = af->next)
    {
      if(af->type == sn)
      {
        return af->duration;
      }
    }
  }
  return -2;
}
/* RT save for dispels */
/* modified for envy -XOR */
bool saves_dispel(int dis_level, int spell_level, int duration)
{
  int save;

  if(duration == -1)
    spell_level += 5;/* very hard to dispel permanent effects */
  save = 50 + (spell_level - dis_level) * 5;
  save = URANGE( 5, save, 95 );
  return number_percent() < save;
}

/* co-routine for dispel magic and cancellation */
bool check_dispel(int dis_level, CHAR_DATA *victim, int sn)
{
  AFFECT_DATA *af;

  if (is_affected(victim, sn))
  {
    for(af = victim->affected; af != NULL; af = af->next)
    {
     if ( af->duration == -1 )
      continue;
     
      if(af->type == sn)
      {
	if ( !saves_spell(dis_level,victim) )
        {
          affect_strip(victim,sn);
          if(skill_table[sn].msg_off)
          {
            send_to_char(C_DEFAULT, skill_table[sn].msg_off, victim );
            send_to_char(C_DEFAULT, "\n\r", victim );
            if(skill_table[sn].room_msg_off)
            {
              act(C_DEFAULT, skill_table[sn].room_msg_off,
	      victim, NULL, NULL, TO_ROOM);
            }
          }
	return TRUE;
	}
	else
          af->level--;
      }
    }
  }
  return FALSE;
}

/* Mobs built with spells only have the flag.
 * These function dispels those spells
 *  -Decklarean
 */

void check_dispel_aff( CHAR_DATA *victim, bool * found, int level, const char * spell, long vector )
{
  int sn;
  sn = skill_lookup(spell);
  if(IS_AFFECTED(victim,vector)
   && !saves_spell(level, victim)
/*   && !saves_dispel(level, victim->level,1) */
   && !is_affected(victim,sn) )
  {
    *found = TRUE;
    REMOVE_BIT(victim->affected_by,vector);
    if(skill_table[sn].msg_off)
    {
      act(C_DEFAULT, skill_table[sn].msg_off,
            victim, NULL, NULL, TO_CHAR);
      if(skill_table[sn].room_msg_off)
      {
        act(C_DEFAULT, skill_table[sn].room_msg_off,
            victim, NULL, NULL, TO_ROOM);
      }
    }
    if ( vector == AFF_FLYING )
     check_nofloor( victim );
  }
}


void check_dispel_aff2( CHAR_DATA *victim, bool * found, int level, const char * spell, long vector )
{
  int sn;
  sn = skill_lookup(spell);
  if(IS_AFFECTED2(victim,vector)
   && !saves_dispel(level, victim->level,-1)
   && !is_affected(victim,sn) )
  {
    *found = TRUE;
    REMOVE_BIT(victim->affected_by2,vector);
    if(skill_table[sn].msg_off)
    {
      act(C_DEFAULT, skill_table[sn].msg_off,
            victim, NULL, NULL, TO_CHAR);
      if(skill_table[sn].room_msg_off)
      {
        act(C_DEFAULT, skill_table[sn].room_msg_off,
        victim, NULL, NULL, TO_ROOM);
      }
    }
    if ( vector == AFF_FLYING )
     check_nofloor( victim );
  }
} 

bool dispel_flag_only_spells( int level,  CHAR_DATA * victim )
{
bool found;
found = FALSE;

check_dispel_aff ( victim, &found, level, "blindness",AFF_BLIND);
check_dispel_aff ( victim, &found, level, "charm person",AFF_CHARM);
check_dispel_aff ( victim, &found, level, "curse",AFF_CURSE);
check_dispel_aff ( victim, &found, level, "detect hidden",AFF_DETECT_HIDDEN);
check_dispel_aff ( victim, &found, level, "detect invis",AFF_DETECT_INVIS);
check_dispel_aff ( victim, &found, level, "faerie fire",AFF_FAERIE_FIRE	);
check_dispel_aff ( victim, &found, level, "fireshield",	AFF_FIRESHIELD	);
check_dispel_aff ( victim, &found, level, "flaming",AFF_FLAMING		);
check_dispel_aff ( victim, &found, level, "fly",AFF_FLYING);
check_dispel_aff ( victim, &found, level, "haste",AFF_HASTE);
check_dispel_aff ( victim, &found, level, "iceshield",AFF_ICESHIELD		);
check_dispel_aff ( victim, &found, level, "infravision",AFF_INFRARED		);
check_dispel_aff ( victim, &found, level, "invis",AFF_INVISIBLE		);
check_dispel_aff ( victim, &found, level, "pass door",AFF_PASS_DOOR		);
check_dispel_aff ( victim, &found, level, "curative aura",AFF_CUREAURA );
check_dispel_aff ( victim, &found, level, "shockshield",AFF_SHOCKSHIELD		);
check_dispel_aff ( victim, &found, level, "sleep",AFF_SLEEP);
check_dispel_aff2( victim, &found, level, "true sight",AFF_TRUESIGHT		);

return found;
}

/* New dispel magic by Decklarean
 * The old way was just to stupid. :>
 * This will dispel all magic spells.
 */

void spell_dispel_magic ( int sn, int level, CHAR_DATA *ch, void *vo )
{
  CHAR_DATA *victim = (CHAR_DATA *) vo;
  AFFECT_DATA *paf;
  bool found;
  if (saves_spell(level, victim))
  {   
    send_to_char(AT_RED, "You feel a brief tingling sensation.\n\r",victim);
    send_to_char(AT_RED, "The spell failed.\n\r",ch);
    return;
  }

  found = FALSE;
  
  /* Check dispel of spells that mobs where built with */
  if (IS_NPC( victim ) )
   found = dispel_flag_only_spells( level, victim );   

  /* Check dispel of spells cast */
  for( paf = victim->affected; paf; paf = paf->next )
  {
     if (paf->deleted)
      continue;
     if ( skill_table[paf->type].spell_fun != spell_null 
     && skill_table[paf->type].dispelable == TRUE )
     if(check_dispel(level,victim,paf->type))
       found = TRUE;
  } 

  for( paf = victim->affected2; paf; paf = paf->next )
  {
     if (paf->deleted)
      continue;
     if ( skill_table[paf->type].spell_fun != spell_null 
     && skill_table[paf->type].dispelable == TRUE )
     if(check_dispel(level,victim,paf->type))
       found = TRUE;
  }

  if(found)
  {
    send_to_char(AT_RED, "You feel a brief tingling sensation.\n\r",victim);
    send_to_char(AT_YELLOW, 
     "Unraveled magical energies ripple away at your success.\n\r",ch);
  }
  else
    send_to_char(AT_RED, "The spell failed.\n\r",ch);

}

/* New cancellation by Decklarean
 * The old way was just to stupid. :>
 * This will dispell all magic spells.
 */
void spell_cancellation(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *victim = (CHAR_DATA *) vo;
  AFFECT_DATA *paf;
  bool found;
  if((!IS_NPC(ch) && IS_NPC(victim)
   && !(IS_AFFECTED(ch, AFF_CHARM) && ch->master == victim))
   || (IS_NPC(ch) && !IS_NPC(victim))
   || !is_same_group(ch, victim))
  {
    send_to_char(C_DEFAULT, "You failed, try dispel magic.\n\r",ch);
    return;
  }
  found = FALSE;

  /* Check dispel of spells that mobs where built with */
  if (IS_NPC( victim ) )
   found = dispel_flag_only_spells( level, victim ); 

  /* Check dispel of spells cast */
  for( paf = victim->affected; paf; paf = paf->next )
  {
    if ( skill_table[paf->type].spell_fun != spell_null 
          && skill_table[paf->type].spell_fun != spell_poison
        )
    if( check_dispel(level,victim,paf->type) )
     found = TRUE;
  } 

  for( paf = victim->affected2; paf; paf = paf->next )
  {
    if ( skill_table[paf->type].spell_fun != spell_null 
          && skill_table[paf->type].spell_fun != spell_poison
        )
    if( check_dispel(level,victim,paf->type) )
     found = TRUE;
  }

  if(found)
    send_to_char(AT_YELLOW, 
     "Unraveled magical energies ripple away at your success.\n\r",ch);
  else
    send_to_char(AT_RED, "The spell failed.\n\r",ch);

}

/*
 * Turn undead and mental block by Altrag
 */
void spell_turn_undead( int sn, int level, CHAR_DATA *ch, void *vo )
{
  CHAR_DATA *victim = (CHAR_DATA *) vo;
  int chance;

  if ( !IS_NPC(victim) || !IS_SET(victim->act, ACT_UNDEAD))
  {
    send_to_char(C_DEFAULT, "Spell failed.\n\r", ch );
    return;
  }

  chance = (level * (10 + IS_FRAMED(ch) ? 15 : IS_GUILTY(ch) ? 0 : 10) );
  chance /= victim->level;
  if (number_percent( ) < chance && !saves_spell( level, victim ))
  {
    act(AT_WHITE,"$n has turned $N!",ch,NULL,victim,TO_ROOM);
    act(AT_WHITE,"You have turned $N!",ch,NULL,victim,TO_CHAR);
    raw_kill(ch,victim);
    return;
  }

  send_to_char(C_DEFAULT,"Spell failed.\n\r",ch);
  return;
}

void spell_mental_block( int sn, int level, CHAR_DATA *ch, void *vo )
{
  CHAR_DATA *victim = (CHAR_DATA *) vo;
  AFFECT_DATA af;

  if ( is_affected(victim,sn) )
    return;

  af.type = sn;
  af.level = level;
  af.duration = number_range( level / 4, level / 2 );
  af.location = APPLY_NONE;
  af.modifier = 0;
  af.bitvector = AFF_NOASTRAL;
  affect_to_char( victim, &af );

  send_to_char( AT_BLUE, "Your mind feels free of instrusion.\n\r",victim);
  if ( ch != victim )
    send_to_char(AT_BLUE, "Ok.\n\r",ch);
}
/* END */

void spell_holy_strength(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA  *victim = (CHAR_DATA *) vo;
  AFFECT_DATA af;

  if(victim->position == POS_FIGHTING || is_affected(victim, sn))
    return;
  af.type       = sn;
  af.level	= level;
  af.duration   = 6 + level;
  af.location   = APPLY_HITROLL;
  af.modifier   = level / 4;
  af.bitvector  = 0;
  affect_to_char( victim, &af );

  af.location  = APPLY_AGI;
  af.modifier  = 2;
    af.bitvector = 0;
  affect_to_char( victim, &af );

  af.location  = APPLY_STR;
  af.modifier  = level / 50;
    af.bitvector = 0;
  affect_to_char( victim, &af );

  if(ch != victim)
    send_to_char(AT_BLUE, "Ok.\n\r", ch );
  send_to_char(AT_BLUE, "The strength of the gods fills you.\n\r", victim);
  return;
}

void spell_curse_of_nature(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA  *victim = (CHAR_DATA *) vo;
  AFFECT_DATA af;

  if ( is_affected(victim, sn) )
  {
    send_to_char(AT_RED, "Your curse of nature is repelled.\n\r", ch );
    return;
  }

  af.type       = sn;
  af.level	    = level;
  af.duration   = 6 + level;
  af.location   = APPLY_IMM_HEAT;
  af.modifier   = 25;
  af.bitvector  = 0;
  affect_to_char( victim, &af );

  af.location  = APPLY_IMM_COLD;
  af.modifier  = 25;
    af.bitvector = 0;
  affect_to_char( victim, &af );

  af.location  = APPLY_IMM_NEGATIVE;
  af.modifier  = 25;
    af.bitvector = 0;
  affect_to_char( victim, &af );

  af.location  = APPLY_IMM_DEGEN;
  af.modifier  = 25;
    af.bitvector = 0;
  affect_to_char( victim, &af );

  if(ch != victim)
    send_to_char(AT_GREEN, "You call upon all the rabbits, and smurfs, and glowing berries to CURSE your opponent.\n\r", ch );
  send_to_char(AT_GREEN, "The wrath of nature wrecks you.\n\r", victim);
  return;
}

void spell_enchanted_song(int sn, int level, CHAR_DATA *ch, void *vo)
{
  CHAR_DATA *victim = (CHAR_DATA *)vo;
  CHAR_DATA *rch = get_char(ch);

  if ( ch == victim )
  {
    act( AT_BLUE, "$n sings an enchanting song.", ch, NULL, NULL, TO_ROOM );
    send_to_char(AT_BLUE, "You sing a song.\n\r", ch );
  }
  act( AT_BLUE, "Your song pacifies $N.", ch, NULL, victim, TO_CHAR );
  act( AT_BLUE, "$n's song pacifies $N.", ch, NULL, victim, TO_NOTVICT );
  act( AT_BLUE, "$n's song slows your reactions.", ch, NULL, victim, TO_VICT );
  STUN_CHAR( victim, 1, STUN_TOTAL );
    for ( rch = ch->in_room->people; rch; rch = rch->next_in_room )
    {
	if ( rch->fighting )
	    stop_fighting( rch, TRUE );
    }
  
  return;
}

/* RT haste spell */

void spell_haste( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
    {
	if (victim == ch)
	  send_to_char(C_DEFAULT, "You can't move any faster!\n\r",ch);
	else
	  act(C_DEFAULT, "$N is already moving as fast as $e can.",
	      ch,NULL,victim,TO_CHAR);
	return;
    }

    af.type      = sn;
    af.level     = level;
    if (victim == ch)
     af.duration  = level/2;
    else
     af.duration  = level/4;
    af.location  = APPLY_AGI;
    af.modifier  = 5;
    af.bitvector = 0;
    affect_to_char( victim, &af );
    send_to_char(C_DEFAULT,
     "You feel yourself moving more quickly.\n\r", victim );
    act(C_DEFAULT, "$n is moving more quickly.",victim,NULL,NULL,TO_ROOM);
    if ( ch != victim )
	send_to_char(C_DEFAULT, "Ok.\n\r", ch );
    return;
}

void spell_hex( int sn, int level, CHAR_DATA *ch, void *vo )
{
  CHAR_DATA *victim = (CHAR_DATA *) vo;
  AFFECT_DATA af;

  if ( IS_AFFECTED( victim, AFF_BLIND + AFF_CURSE )
  && IS_AFFECTED2( victim, AFF_CONFUSED ) )
	{
	act( AT_DGREY, "$N is already hexed.", ch, NULL, victim, TO_CHAR );
	return;
	}
  if ( saves_spell( level, victim ) )
	{
	act( AT_DGREY, "$N resists the hex.", ch, NULL, victim, TO_CHAR );
	return;
	}

  af.type	= sn;
  af.level	= level;
  af.duration	= ( 2 * level / 3 + 20 ) / 2;
  af.location	= APPLY_HITROLL;
  af.modifier	= -40;
  af.bitvector	= AFF_BLIND;
  affect_to_char( victim, &af );

  af.location	= APPLY_PDAMP;
  af.modifier	= -10;
    af.bitvector = 0;
  affect_to_char( victim, &af );

  af.location	= APPLY_MDAMP;
  af.modifier	= -10;
  af.bitvector	= AFF_CURSE;
  affect_to_char( victim, &af );

  af.location	= APPLY_IMM_UNHOLY;
  af.modifier	= 30;
    af.bitvector = 0;
  affect_to_char( victim, &af );

  af.location	= APPLY_DAMROLL;
  af.modifier	= -15;
  af.bitvector	= AFF_CONFUSED;
  affect_to_char2( victim, &af );

  send_to_char( AT_DGREY, "You have hexed your opponent.\n\r", ch );
  send_to_char( AT_DGREY, "A hex has been placed upon your soul.\n\r", victim );
  return;
}

void spell_dark_ritual( int sn, int level, CHAR_DATA *ch, void *vo )
{
  int		mana;
  OBJ_DATA      *obj = (OBJ_DATA *) vo;
  OBJ_DATA      *obj_next;

  if ( obj->item_type != ITEM_CORPSE_NPC )
  {
   send_to_char( AT_DGREY, "You must have a corpse to sacrifice to perform a dark ritual.\n\r", ch );
   return;
  }

  obj_next = obj->next;

  if (obj->deleted)
   return;

    mana = UMAX( 30, number_fuzzy( level / 2 ) );
    if ( mana < 45 )
     mana = 45;
    ch->mana += mana;
    ch->mana = UMIN( MAX_MANA(ch), ch->mana );
    send_to_char( AT_DGREY, "You extract the last of the energy from the corpse.\n\r", 
		  ch );
    act( AT_DGREY, "$n saps away the last of the mystical energies from the $p.",
	 ch, obj, NULL, TO_ROOM );
    extract_obj( obj );

  return;
}

/* Spell named by Nizza */
void spell_necro_glycerine( int sn, int level, CHAR_DATA *ch, void *vo )
{
  OBJ_DATA	*obj;
  bool		found = FALSE;

  for ( obj = ch->in_room->contents; obj; obj = obj->next )
  {
   if ( !obj || obj->deleted )
    continue;

   if ( obj->item_type == ITEM_CORPSE_NPC )
   {
    found = TRUE;
    break;
   }
  }

  if ( found == TRUE )
  {
   act( AT_DGREY, "$n kneels besides $p. $e places $s hands on $p and then rises again.",
    ch, obj, NULL, TO_ROOM );
   act( AT_DGREY, "You kneel beside $p and convert it's death energies.",
    ch, obj, NULL, TO_CHAR );
   act( AT_DGREY, "$p begins to smoke and fume.",
    ch, obj, NULL, TO_ROOM );
   obj->item_type = ITEM_BOMB;
   obj->timer = 30;
   obj->owner = ch;
   obj->value[1] = BOMB_EXPLOSIVE;
   obj->value[2] = PROP_NONE;

   if ( IS_NPC( ch ) )
    obj->value[3] =
     get_curr_int(ch) + get_curr_wis(ch) + obj->weight + dice(5, 30);
   else
    obj->value[3] =
     get_curr_int(ch) + get_curr_wis(ch) + obj->weight + dice(5, 30)
     + ch->pcdata->learned[sn];
  }
  else
   send_to_char( AT_DGREY, "You must have a corpse to convert for necro-glycerine.\n\r", ch );

 return;
}

void spell_lokian_revenge( int sn, int level, CHAR_DATA *ch, void *vo )
{
  OBJ_DATA	*obj;
  bool		found = FALSE;

  for ( obj = ch->in_room->contents; obj; obj = obj->next )
  {
   if ( !obj || obj->deleted )
    continue;

   if ( obj->item_type == ITEM_CORPSE_NPC )
   {
    found = TRUE;
    break;
   }
  }

  if ( found == TRUE )
  {
   act( AT_DGREY, "$n kneels besides $p. $e places $s hands on $p and then rises again.",
    ch, obj, NULL, TO_ROOM );
   act( AT_DGREY, "You kneel beside $p and convert it's death energies.",
    ch, obj, NULL, TO_CHAR );
   act( AT_DGREY, "$p begins to smoke and fume.",
    ch, obj, NULL, TO_ROOM );
   obj->item_type = ITEM_BOMB;
   obj->timer = 15;
   obj->owner = ch;
   obj->value[1] = dice( 1, 7 ) - 1;
   obj->value[2] = PROP_NONE;

   if ( IS_NPC( ch ) )
    obj->value[3] =
     get_curr_int(ch) + get_curr_wis(ch) + dice(1, 30);
   else
    obj->value[3] =
     get_curr_int(ch) + get_curr_wis(ch) + dice(1, 30)
     + ch->pcdata->learned[sn];
  }
  else
   send_to_char( AT_DGREY, "You must have a corpse to convert for Lokian revenge.\n\r", ch );

 return;
}

void spell_field_of_decay( int sn, int level, CHAR_DATA *ch, void *vo )
{
   OBJ_DATA *cloud;

    cloud = create_object( get_obj_index( OBJ_VNUM_GAS_CLOUD ), 0 );

    free_string( cloud->short_descr );
    cloud->short_descr = str_dup("a black haze");

    free_string( cloud->name );
    cloud->name = str_dup("black haze");

    free_string( cloud->description );
    cloud->description = str_dup("An ominous black haze lingers here.");

    cloud->timer = number_range( 3, 7 );
    cloud->level = ch->level;
    cloud->value[1] = DAM_DEGEN;
    cloud->value[2] = number_range( get_curr_int(ch)*2, ch->level*2 );
    cloud->value[3] = GAS_AFFECT_DISEASED;
    act(AT_BLUE, "You kneel and channel the energies of death into $p.",
     ch, cloud, NULL, TO_CHAR );
    act(AT_BLUE, "$n kneels and $p takes form in the room.",
     ch, cloud, NULL, TO_ROOM );
    obj_to_room( cloud, ch->in_room );
  return;
}

void spell_armor_of_thorns( int sn, int level, CHAR_DATA *ch, void *vo )
{
  AFFECT_DATA af;

  if ( IS_AFFECTED2( ch, AFF_THORNY ) )
	return;
  af.type	= sn;
  af.level	= level;
  af.duration	= number_fuzzy( level / 5 );
  af.location	= APPLY_NONE;
  af.modifier	= 0;
  af.bitvector	= AFF_THORNY;
  affect_to_char2( ch, &af );

  send_to_char( AT_DGREY, "Poisonous thorns erupt from your body.\n\r", ch);
  act( AT_DGREY, "Thorns erupt from $n's body!",
       ch, NULL, NULL, TO_ROOM );
  return;
}

void spell_blink( int sn, int level, CHAR_DATA *ch, void *vo )
{
  AFFECT_DATA af;

  if ( IS_AFFECTED2( ch, AFF_BLINK ) )
	return;
  af.type	= sn;
  af.level	= level;
  af.duration	= number_fuzzy( level / 5 );
  af.location	= APPLY_NONE;
  af.modifier	= 0;
  af.bitvector	= AFF_BLINK;
  affect_to_char2( ch, &af );

  send_to_char( AT_DGREY, "Your apperance starts to blink.\n\r", ch );
  act( AT_DGREY, "$n form blinks into and out of sight.",
       ch, NULL, NULL, TO_ROOM );
  return;
}
void spell_physical_mirror( int sn, int level, CHAR_DATA *ch, void *vo )
{
  AFFECT_DATA af;

  if ( IS_AFFECTED2( ch, AFF_PMIRROR ) )
	return;
  af.type	= sn;
  af.level	= level;
  af.duration	= number_fuzzy( level / 5 );
  af.location	= APPLY_NONE;
  af.modifier	= 0;
  af.bitvector	= AFF_PMIRROR;
  affect_to_char2( ch, &af );

  send_to_char( AT_DGREY, "You raise a physical mirror.\n\r", ch );
  act( AT_DGREY, "$n is surrounded by a faint mirror.",
       ch, NULL, NULL, TO_ROOM );
  return;
}

void spell_deadly_vein( int sn, int level, CHAR_DATA *ch, void *vo )
{
  AFFECT_DATA af;

  CHAR_DATA *victim = (CHAR_DATA *) vo;

  if ( IS_AFFECTED2( victim, AFF_ACIDBLOOD ) 
     || victim->race == RACE_DRACONI || victim->race == RACE_AQUASAPIEN )
	return;

  af.type	= sn;
  af.level	= level;
  af.duration	= number_fuzzy( level / 5 );
  af.location	= APPLY_CON;
  af.modifier	= 1;
  af.bitvector	= AFF_ACIDBLOOD;
  affect_to_char2( victim, &af );

  if ( victim != ch )
  send_to_char( AT_DGREY, "You cause acid to surge through their veins.\n\r", ch );
  send_to_char( AT_DGREY, "Acid surges through your veins.\n\r", victim );
  return;
}

void spell_mana_net( int sn, int level, CHAR_DATA *ch, void *vo)
{
  AFFECT_DATA af;

  if ( IS_AFFECTED2( ch, AFF_MANANET ) 
     || ch->race == RACE_GPALADIN || ch->race == RACE_ANGEL )
	return;

  af.type	= sn;
  af.level	= level;
  af.duration	= number_fuzzy( level / 5 );
  af.location	= APPLY_NONE;
  af.modifier	= 0;
  af.bitvector	= AFF_MANANET;
  affect_to_char2( ch, &af );

  send_to_char( AT_DGREY, "You weave a mana net about your body.\n\r", ch );
  return;
}void spell_mana_shield( int sn, int level, CHAR_DATA *ch, void *vo)
{
  AFFECT_DATA af;

  if ( IS_AFFECTED2( ch, AFF_MANASHIELD ) )
	return;

  af.type	= sn;
  af.level	= level;
  af.duration	= number_fuzzy( level / 5 );
  af.location	= APPLY_NONE;
  af.modifier	= 0;
  af.bitvector	= AFF_MANASHIELD;
  affect_to_char2( ch, &af );

  send_to_char( AT_DGREY, "You form a mana sheild about your body.\n\r", ch );
  return;
}
void spell_magical_mirror( int sn, int level, CHAR_DATA *ch, void *vo )
{
  AFFECT_DATA af;

  if ( IS_AFFECTED2( ch, AFF_MMIRROR ) )
	return;
  af.type	= sn;
  af.level	= level;
  af.duration	= number_fuzzy( level / 5 );
  af.location	= APPLY_NONE;
  af.modifier	= 0;
  af.bitvector	= AFF_MMIRROR;
  affect_to_char2( ch, &af );

  send_to_char( AT_DGREY, "You raise a magical mirror.\n\r", ch );
  act( AT_DGREY, "$n is surrounded by a faint mirror.",
       ch, NULL, NULL, TO_ROOM );
  return;
}

void spell_mirror_images( int sn, int level, CHAR_DATA *ch, void *vo )
{
  AFFECT_DATA af;

  if ( IS_AFFECTED2( ch, AFF_MIRROR_IMAGES ) )
	return;
  af.type	= sn;
  af.level	= level;
  af.duration	= number_fuzzy( level / 5 );
  af.location	= APPLY_NONE;
  af.modifier	= 0;
  af.bitvector	= AFF_MIRROR_IMAGES;
  affect_to_char2( ch, &af );

  send_to_char( AT_DGREY, "You create a mirror image of yourself.\n\r", ch );
  act( AT_DGREY, "$n generates a mirror image of $mself.",
       ch, NULL, NULL, TO_ROOM );
  return;
}

void spell_stench_of_decay( int sn, int level, CHAR_DATA *ch, void *vo )
{
   OBJ_DATA *cloud;

    cloud = create_object( get_obj_index( OBJ_VNUM_GAS_CLOUD ), 0 );

    free_string( cloud->short_descr );
    cloud->short_descr = str_dup("a greenish-brown gas");

    free_string( cloud->name );
    cloud->name = str_dup("gas cloud");

    free_string( cloud->description );
    cloud->description = str_dup("An ominous greenish-brown gas lingers here.");

    cloud->timer = number_range( 3, 7 );
    cloud->level = ch->level;
    cloud->value[1] = DAM_DEGEN;
    cloud->value[2] = number_range( get_curr_int(ch), ch->level );
    cloud->value[3] = GAS_AFFECT_POISONED;
    act(AT_BLUE, "You kneel and channel the energies of death into $p.",
     ch, cloud, NULL, TO_CHAR );
    act(AT_BLUE, "$n kneels and $p takes form in the room.",
     ch, cloud, NULL, TO_ROOM );
    obj_to_room( cloud, ch->in_room );
    return;
}

void spell_soul_bind( int sn, int level, CHAR_DATA *ch, void *vo )
{
   CHAR_DATA *victim = (CHAR_DATA *) vo;
   OBJ_DATA  *soulgem;

    if ( !IS_NPC(victim) || saves_spell( level, victim ) )
    {
      send_to_char(AT_BLUE, "You failed.\n\r", ch);
      return;
    }

   soulgem = create_object( get_obj_index( OBJ_VNUM_SOULGEM ), 0 );
   soulgem->invoke_vnum = victim->pIndexData->vnum;
   soulgem->level = ch->level;
   soulgem->timer = ch->level / 4;
   soulgem->cost.silver = soulgem->cost.copper = 0;
   soulgem->cost.gold = victim->level * 1000;
   soulgem->invoke_charge[0] = soulgem->invoke_charge[1] = 1;
   obj_to_char( soulgem , ch );
    
    act(AT_BLUE, "You tear out $Ns soul, binding it to form a Soulgem.", ch, NULL, victim, TO_CHAR);
    act(AT_BLUE, "$n tears out $Ns soul, binding it to form a Soulgem.", ch, NULL, victim, TO_ROOM);
    act(AT_BLUE, "$N screams in agony as it slowly dissipates into nothingness!", ch, NULL, victim, TO_CHAR);
    act(AT_BLUE, "$N screams in agony as it slowly dissipates into nothingness!", ch, NULL, victim, TO_ROOM);
    act(AT_WHITE, "Your SOUL is STOLEN by $n!", ch, NULL, victim, TO_VICT);

    if ( IS_NPC( victim ) )
       extract_char( victim, TRUE );
    else
       extract_char( victim, FALSE );

   return;
}
/* MONK SPELLS */
void spell_iron_skin( int sn, int level, CHAR_DATA *ch, void *vo )
{
  AFFECT_DATA af;
  if ( is_affected( ch, sn ) )
	return;
  send_to_char( AT_GREY, "Your skin turns to iron.\n\r", ch );
  act( AT_GREY, "$n's skin turns to iron.", ch, NULL, NULL, TO_ROOM );
  af.type	 = sn;
  af.level	 = ch->level;
  af.duration	 = ch->level / 6;
  af.location	 = APPLY_IMM_BASH;
  af.modifier	 = -15;
  af.bitvector   =   0;
  affect_to_char2( ch, &af );
  return;
}
void spell_chi_shield( int sn, int level, CHAR_DATA *ch, void *vo )
{
  AFFECT_DATA af;
  if ( is_affected( ch, sn ) )
	return;
  send_to_char( AT_BLUE, "You tap into your chi and use it to raise a chi shield.\n\r", ch );
  act( AT_BLUE, "$n is surrounded by a chi shield.", ch, NULL, NULL, TO_ROOM );
  af.type	 = sn;
  af.level	 = ch->level * 2;
  af.duration	 = ch->level / 4;
  af.location	 = APPLY_MDAMP;
  af.modifier	 = 5;
  af.bitvector   = 0;
  affect_to_char( ch, &af );
  return;
}
  
/* Adds + dam to spells for having spellcraft skill and other stuff too*/
int sc_dam( CHAR_DATA *ch, int dam, int element )
{
  TATTOO_DATA *tattoo;
  int   iWear = 0;
  float mod;
  float mod2;
  if ( ch->level < 50 )
	mod = 82.6;		/* x1.15 */
  else if ( ch->level < 60 )
	mod = 73.07;		/* x1.3  */ 
  else if ( ch->level < 70 )
	mod = 65.51;		/* x1.45 */
  else if ( ch->level < 80 )
	mod = 55.88; 		/* x1.7  */
  else if ( ch->level < 90 )
	mod = 51.35;		/* x1.85 */
  else if ( ch->level < 95 )
	mod = 47.5;		/* x2    */
  else
	mod = 38; 		/* x2.5  */

  mod2 = ((dam * get_curr_int(ch)) / 18);
  dam = mod2;

     for ( iWear = 0; iWear < MAX_TATTOO; iWear++ )
     {
	if ( ( tattoo = get_tattoo_char( ch, iWear ) ) != NULL )
	 {
          if ( element == DAM_HEAT
            && tattoo->magic_boost == MAGBOO_HEAT )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_POSITIVE
            && tattoo->magic_boost == MAGBOO_POSITIVE )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_COLD
            && tattoo->magic_boost == MAGBOO_COLD )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_NEGATIVE
            && tattoo->magic_boost == MAGBOO_NEGATIVE )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_HOLY
            && tattoo->magic_boost == MAGBOO_HOLY )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_UNHOLY
            && tattoo->magic_boost == MAGBOO_UNHOLY )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_REGEN
            && tattoo->magic_boost == MAGBOO_REGEN )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_DEGEN
            && tattoo->magic_boost == MAGBOO_DEGEN )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_DYNAMIC
            && tattoo->magic_boost == MAGBOO_DYNAMIC )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_DYNAMIC
            && tattoo->magic_boost == MAGBOO_DYNAMIC )
            {
             dam += (dam / 2);
             break;
            }
          if ( element == DAM_VOID
            && tattoo->magic_boost == MAGBOO_VOID )
            {
             dam += (dam / 2);
             break;
            }
         }
     }

  if ( element != DAM_UNHOLY )
  switch( time_info.phase_white )
  {
   default: break;
   case 0:
    dam += 2;
    break;
   case 1: case 7:
    dam += dam / 3;
    break;
   case 3: case 5:
    dam -= dam / 3;
    break;
   case 4:
    dam -= dam / 2;
    break;
   }

  if ( element == DAM_UNHOLY )
  switch( time_info.phase_shadow )
  {
   default: break;
   case 0:
    dam += 2;
    break;
   case 1: case 7:
    dam += dam / 3;
    break;
   case 3: case 5:
    dam -= dam / 3;
    break;
   case 4:
    dam -= dam / 2;
    break;
   }

  if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_spellcraft] > 0 )
	dam += (dam * ch->pcdata->learned[gsn_spellcraft]) / (mod * 2);
  if ( !IS_NPC( ch ) && ch->race == RACE_AQUINIS )
        dam += dam / 4;

  /* haha, weather interferes with spell casting! -Flux */
  /* basically, the hotter it is, the more damage for heat
     the colder, the better for cold. Water and snow also
     absorb heat, while helping cold out */
  if ( element == DAM_HEAT )
   dam += (ch->in_room->temp - 75) - ch->in_room->accumulation;

  if ( element == DAM_COLD )
   dam += (50 - ch->in_room->temp) + ch->in_room->accumulation;

  /* electricity THRIVES on moisture! */
  if ( element == DAM_NEGATIVE )
   dam += (ch->in_room->accumulation * dice(1, 3));

  if ( dam < 0 )
   dam = 0;

  return dam;
}

/*
 * Bard spells -- Thanks Arkhane :>
 */
void spell_inspiration( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;
    char buf [ MAX_STRING_LENGTH ];

    act( AT_YELLOW, "$n sings a song about great battles and brave heros.", 
	 ch, NULL, NULL, TO_ROOM );

    for( victim = ch->in_room->people; victim; victim = victim->next_in_room )
    {

	if ( ( victim->deleted ) || !is_same_group( ch, victim ) )
	    continue;

	if ( IS_NPC( victim ) )
	    continue;

    	if( is_affected( victim, sn ) )
	{
	  if( victim == ch )
	    send_to_char( AT_WHITE, "You have already been inspired.\n\r", victim );
	  else
	    act( AT_WHITE, "$N has been inspired already.", ch, NULL, victim,
		 TO_CHAR );
/*	  return;  (still want to loop through rest of chars so just continue) */
	  continue;
	}
   	
	af.type 	 = sn;
	af.level 	 = level;
	af.duration	 = level;
	af.location	 = APPLY_HITROLL;
	af.modifier	 = level / 8;
	af.bitvector = 0;
	affect_to_char( victim, &af );

	af.location = APPLY_DAMROLL;
	af.modifier	= level / 8;
    af.bitvector = 0;
	affect_to_char( victim, &af );

	af.location	= APPLY_AGI;
	af.modifier = 2;
    af.bitvector = 0;
	affect_to_char( victim, &af );

	if ( victim != ch)
	{
	  sprintf( buf, "You become inspired by %s's song.\n\r", ch->name );
	  send_to_char( AT_CYAN, buf, victim );
	  sprintf( buf, "%s becomes inspired by your song.\n\r", victim->name );
	  send_to_char( AT_CYAN, buf, ch );
	}
	else
	  send_to_char( AT_CYAN, "You inspire yourself to greater heights.\n\r",
			ch );
    }

    return;

}

void spell_war_cry( int sn, int level, CHAR_DATA *ch, void *vo )
{

    CHAR_DATA *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    act( AT_BLOOD, 
	 "$n's cry for the death of $s enemies pierces through the room!",
	 ch, NULL, NULL, TO_ROOM );

    if ( saves_spell( level, victim ) )
    {
	act( AT_WHITE, "$N ignores your cry for blood.",
	     ch, NULL, victim, TO_CHAR );
	return;
    }
    af.type	     = sn;
    af.level     = level;
    af.duration  = 3;
    af.location  = APPLY_HITROLL;
    af.modifier  = - level / 5;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location = APPLY_AGI;
    af.modifier = - 2;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    if ( ch != victim )
	send_to_char( AT_BLOOD,
		      "Your cry for war sends fear into your opponent!\n\r",
		      ch );
    send_to_char( AT_BLOOD, "A bloodthirsty cry sends shivers down your spine.\n\r",
		      victim );

    return;
}

void spell_group_healing( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;
    int heal;

    act( AT_WHITE, "$n emits a soothing light.", ch, NULL, NULL, TO_ROOM );

    for( victim = ch->in_room->people; victim; victim = victim->next_in_room )
    {
	if ( IS_NPC( victim ) || ( victim->deleted ) ||
	   ( !is_same_group( ch, victim ) ) )
	   continue;



    heal = 100;
    if ( (victim->hit + heal) > MAX_HIT(victim) )
    heal = MAX_HIT(victim) - victim->hit;  
    damage( ch, victim, heal, sn, DAM_REGEN, TRUE  );
	update_pos( victim, ch );
    }
    send_to_char( AT_WHITE, "You have healed your group.\n\r", ch );
    return;
}

void spell_chant( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    send_to_char(AT_BLUE, "You begin your loud chant of destruction!\n\r", ch );
    act(AT_BLUE, "$n's chant wreaks havoc everywhere!", ch, NULL, NULL, TO_ROOM );

    for ( victim = ch->in_room->people; victim; victim = victim->next_in_room )
    {
	if ( victim->deleted )
             continue;
        if ( is_same_group( ch, victim ) )
            continue;
        if ( ch == victim )
            continue;

	     damage( ch, victim, 1.5 * level + dice( level, 6 ), sn, DAM_DYNAMIC, TRUE  );
    }

    return;

}

/*Decklarean*/
void spell_blur ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
	return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = 3 + level;
    af.location	 = APPLY_MDAMP;
    af.modifier	 = 10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_GREY, "Your form blurs.\n\r",
		 victim );
    act(AT_GREY, "$N form becomes blurred, shifting and wavering before you.",
	ch, NULL, victim, TO_ROOM );
    return;
}

void spell_holy_shield ( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA  *victim = (CHAR_DATA *) vo;
    AFFECT_DATA af;

    if ( is_affected( victim, sn ) )
	return;

    af.type	 = sn;
    af.level	 = level;
    af.duration	 = 3 + level;
    af.location	 = APPLY_PDAMP;
    af.modifier	 = 10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    af.location  = APPLY_MDAMP;
    af.modifier  = 10;
    af.bitvector = 0;
    affect_to_char( victim, &af );

    send_to_char(AT_GREY, "God lends you protection.\n\r",
		 victim );
    act(AT_GREY, "$N gains God's favor.",
	ch, NULL, victim, TO_NOTVICT );
    return;
}

/* These spells are reserved for objects */

void spell_heal_critical( int sn, int level, CHAR_DATA *ch, void *vo )
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

    update_pos( victim, ch );
    return;
}

void spell_heal_light( int sn, int level, CHAR_DATA *ch, void *vo )
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

    update_pos( victim, ch );
 return;
}

void spell_heal_serious( int sn, int level, CHAR_DATA *ch, void *vo )
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

    update_pos( victim, ch );
 return;
}

void spell_heal_blindness( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( !is_affected( victim, gsn_blindness ) )
	return;

    affect_strip( victim, gsn_blindness );

    if ( ch != victim )
	send_to_char(AT_BLUE, "Ok.\n\r", ch );
    send_to_char(AT_WHITE, "Your vision returns!\n\r", victim );
    return;
}

void spell_heal_poison( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( !is_affected( victim, gsn_poison ) )
        return;

    affect_strip( victim, gsn_poison );

    send_to_char(AT_GREEN, "Ok.\n\r",                                    ch     );
    send_to_char(AT_GREEN, "A warm feeling runs through your body.\n\r", victim );
    act(AT_GREEN, "$N looks better.", ch, NULL, victim, TO_NOTVICT );

  return;
}

void spell_heal_clap( int sn, int level, CHAR_DATA *ch, void *vo )
{
 CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( !IS_SET( victim->affected_by2, AFF_CLAP ) )
        return;

    REMOVE_BIT( victim->affected_by2, AFF_CLAP );

    send_to_char(AT_GREEN, "You relieve them of the clap.\n\r", ch );
    send_to_char(AT_GREEN, "A warm feeling runs through your body.\n\r", victim );
    act(AT_GREEN, "$N looks better.", ch, NULL, victim, TO_NOTVICT );

  return;
} 

void spell_heal_disease( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( !is_affected( victim, gsn_plague ) )
        return;

    affect_strip( victim, gsn_plague );
    REMOVE_BIT( victim->affected_by2, AFF_DISEASED );

    send_to_char(AT_GREEN, "You relieve them of the plague.\n\r", ch );
    send_to_char(AT_GREEN, "A warm feeling runs through your body.\n\r", victim );
    act(AT_GREEN, "$N looks better.", ch, NULL, victim, TO_NOTVICT );

  return;
}

void spell_heal_plasma( int sn, int level, CHAR_DATA *ch, void *vo )
{
    CHAR_DATA *victim = (CHAR_DATA *) vo;

    if ( !is_affected( victim, gsn_plasma) )
        return;

    affect_strip( victim, gsn_plasma );
    REMOVE_BIT( victim->affected_by2, AFF_PLASMA );

    send_to_char(AT_GREEN, "You relieve them of the sticky heat.\n\r", ch );
    send_to_char(AT_GREEN, "The plasma about your skin vanishes.\n\r",victim );
    act(AT_GREEN, "The plasma about $N evaporates.", ch, NULL, victim, TO_NOTVICT );

 return;
}

