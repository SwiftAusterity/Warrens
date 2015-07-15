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

  adept = 100;
  
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
	    act(AT_BLUE, buf, ch, NULL, rch, TO_VICT );
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
