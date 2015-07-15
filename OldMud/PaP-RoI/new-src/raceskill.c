/*****************************************************************************
 * All the racial skills		                                     *
 * -- Hannibal                                                               *
 *****************************************************************************/

#define linux 1
#if defined( macintosh )
#include <types.h>
#else
#include <sys/types.h>
#endif
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "merc.h"
/*
 * External functions.
 */
extern void    set_fighting     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
extern void    forge_obj	args( ( CHAR_DATA *ch, OBJ_DATA *to_forge ) );

void do_crush( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim;
  char arg[ MAX_INPUT_LENGTH ];
  int  dam = 0;

 one_argument( argument, arg );

   if ( IS_NPC( ch ) )
   {
    typo_message( ch );
    return;
   }

   if ( ch->race != RACE_MAUDLIN )
   {
    typo_message( ch );
    return;
   }

   if ( ch->pcdata->assimilate[ASSIM_RARM] != OBJ_VNUM_LOBSTER_CLAW &&
        ch->pcdata->assimilate[ASSIM_LARM] != OBJ_VNUM_LOBSTER_CLAW )
   {
    send_to_char( AT_YELLOW, "Your hands are not suited for this skill.\n\r", ch );
    return;
   }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

 if ( get_curr_dex(ch) + get_curr_agi(ch) >
      get_curr_dex(victim) + get_curr_agi(victim) + (number_percent()/25))
 {
  dam = number_range( get_curr_str( ch ), get_curr_str( ch ) * 10 );

  act( AT_YELLOW, "You grab $N and crush $M!", ch, NULL, victim, TO_CHAR);
  act( AT_RED, "$n grabs you and crushes you!", ch, NULL, victim, TO_VICT );
  act( AT_YELLOW, "$n grabs $N and crushes $M!", ch, NULL, victim, TO_NOTVICT );
  damage( ch, victim, dam, gsn_crush, DAM_INTERNAL, TRUE );

  if ( dam > 0 &&
   !IS_STUNNED( victim, STUN_TOTAL ) && !IS_STUNNED( ch, STUN_TO_STUN ) )
  {
   STUN_CHAR( ch, 4, STUN_TO_STUN );
   STUN_CHAR( victim, 2, STUN_TOTAL );
   victim->position = POS_STUNNED;
  }
 }
 else
  act( AT_YELLOW, "$N evaded your mighty claw!", ch, NULL, victim, TO_CHAR );

 return;
}

void do_webspin( CHAR_DATA *ch, char *argument )
{
   if ( IS_NPC( ch ) )
   {
    typo_message( ch );
    return;
   }

   if ( ch->race != RACE_MAUDLIN )
   {
    typo_message( ch );
    return;
   }
 return;
}

void do_assimilate( CHAR_DATA *ch, char *argument )
{
   OBJ_DATA *part;
   char buf[MAX_STRING_LENGTH];
   char arg1[ MAX_INPUT_LENGTH ];
   char arg2[ MAX_INPUT_LENGTH ];
   int  location = 0;

   argument = one_argument( argument, arg1 );
   argument = one_argument( argument, arg2 );

   if ( IS_NPC( ch ) )
   {
    typo_message( ch );
    return;
   }

   if ( ch->race != RACE_MAUDLIN )
   {
    typo_message( ch );
    return;
   }

   if ( arg1[0] == '\0' )
   {
    send_to_char( AT_YELLOW, "Syntax: &Cassimilate <location> <object>\n\r", ch );
   send_to_char( AT_YELLOW, "Syntax: &WType help assimilate for a list of locations\n\r", ch );
    send_to_char( AT_YELLOW, "Syntax: &Cassimilate status\n\r", ch );
    return;
   }

   if ( !str_cmp( arg1, "status" ))
   {
    send_to_char( AT_WHITE, "DNA anatomical status map:\n\r", ch );

    for ( location = 0; location < 10; location++)
    {
     if ( ch->pcdata->assimilate[location] == 0 )
      continue;

      if ( (part =
       create_object( get_obj_index(ch->pcdata->assimilate[location]), 1 )) )
      {
       sprintf( buf, "&W<&G%s&W>		%s\n\r",
        flag_string( assimilate_loc, location ),
        bodypartdesc(part->pIndexData->vnum) );
       send_to_char( AT_GREEN, buf, ch );
      }
      else
       continue;
    }
    return;
   }

   if ( (location = flag_value( assimilate_loc, arg1 )) != -99 )
   {
    if ( !( part = get_obj_carry( ch, arg2 ) ) )
    {
     send_to_char(AT_BLUE, "You do not have that part.\n\r", ch );
     return;
    }
    
    if ( !(assimilate_vnum_check( location, part->pIndexData->vnum )) )
    {
     send_to_char( AT_YELLOW, "That part does not match that location, your body rejects the new DNA.\n\r", ch );
     return;
    }

    assimilate_part( ch, part, location );
    extract_obj(part);
    return;
   }

   send_to_char( AT_YELLOW, "Syntax: &Cassimilate <location> <object>\n\r", ch );
   send_to_char( AT_YELLOW, "Syntax: &WType help assimilate for a list of locations\n\r", ch );
   send_to_char( AT_YELLOW, "Syntax: &Cassimilate status\n\r", ch );

 return;
}

void do_morph( CHAR_DATA *ch, char *argument )
{
   char	buf[MAX_STRING_LENGTH];
   char arg1[ MAX_INPUT_LENGTH ];
   char arg2[ MAX_INPUT_LENGTH ];

   argument = one_argument( argument, arg1 );
   argument = one_argument( argument, arg2 );

   if ( IS_NPC( ch ) )
   {
    typo_message( ch );
    return;
   }

   if ( ch->race != RACE_QUICKSILVER )
   {
    typo_message( ch );
    return;
   }

    if ( arg1[0] == '\0' )
    {
     send_to_char( AT_YELLOW, "Syntax: &Cmorph <type> <type value>\n\r"
      "&WType being: &Cweapon, armor, avoidance, size, camoflauge\n\r", ch );
     send_to_char( AT_YELLOW, "Syntax: &Cmorph stat\n\r", ch );
     return;
    }

    if ( !str_cmp( arg1, "stat" ) )
    {
     sprintf( buf, "Your are: &P%s&X.\n\r",
      flag_string( size_flags, ch->size ) );
     send_to_char( AT_WHITE, buf, ch );

     sprintf( buf, "Your hands are in the form of: &P%s&X.\n\r",
      flag_string( morph_weapons, ch->pcdata->morph[0] ) );
     send_to_char( AT_GREEN, buf, ch );

     sprintf( buf, "Your body is more resistant to: &P%s&X.\n\r",
      flag_string( morph_armor, ch->pcdata->morph[1] ) );
     send_to_char( AT_WHITE, buf, ch );

     if ( ch->pcdata->morph[2] == 1 )
      sprintf( buf, "You &Pare&X concentrating on avoiding attacks.\n\r" );
     else
      sprintf( buf, "You &Pare not&X concentrating on avoiding attacks.\n\r" );
     send_to_char( AT_GREEN, buf, ch );

     if ( ch->pcdata->morph[4] == 1 )
      sprintf( buf, "You &Pare&X concentrating on remaining unseen.\n\r" );
     else
      sprintf( buf, "You &Pare not&X concentrating on remaining unseen.\n\r" );
     send_to_char( AT_WHITE, buf, ch );
     return;
    }

    if ( !str_cmp( arg1, "weapon" ) )
    {
     if ( !str_cmp( arg2, "claws" ) )
     {
      act( AT_WHITE, "You form claws out of your hands.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n forms claws out of $s hands.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_WEAPON] = MORPH_WEAPON_CLAW;
      ch->pcdata->claws = CLAW_NORMAL;
      return;
     }

     if ( !str_cmp( arg2, "blade" ) )
     {
      act( AT_WHITE, "Your arms elongate and form into blades.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's arms elongate and form into blades.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_WEAPON] = MORPH_WEAPON_SLASH;
      ch->pcdata->claws = CLAW_SLASH;
      return;
     } 
     if ( !str_cmp( arg2, "hammer" ) )
     {
      act( AT_WHITE, "Your clentch your fists and your hands form into hammers.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's hands morph into hammers.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_WEAPON] = MORPH_WEAPON_BASH;
      ch->pcdata->claws = CLAW_BASH;
      return;
     } 
     if ( !str_cmp( arg2, "axe" ) )
     {
      act( AT_WHITE, "Your hands morph into axe blades.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's hands morph into axe blades.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_WEAPON] = MORPH_WEAPON_CHOP;
      ch->pcdata->claws = CLAW_CHOP;
      return;
     } 
     if ( !str_cmp( arg2, "stab" ) )
     {
      act( AT_WHITE, "You morph your hands into ice picks.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's hands morph into ice picks.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_WEAPON] = MORPH_WEAPON_PIERCE;
      ch->pcdata->claws = CLAW_PIERCE;
      return;
     } 

     if ( !str_cmp( arg2, "none" ) )
     {
      act( AT_WHITE, "Your hands revert.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's hands return to normal.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_WEAPON] = 0;
      ch->pcdata->claws = 0;
      return;
     }
   
     {
      send_to_char( AT_YELLOW, "Type values for weapon: blade, hammer, axe, stab, claws, none\n\r", ch );
      return;
     }
    }

    if ( !str_cmp( arg1, "armor" ) )
    {
     if ( !str_cmp( arg2, "slash" ) )
     {
      act( AT_WHITE, "Your skin hardens and takes on the appearance of scales.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's skin takes on the appearance of scales.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_ARMOR] = MORPH_WEAPON_SLASH;
      return;
     }

     if ( !str_cmp( arg2, "chop" ) )
     {
      act( AT_WHITE, "Your skin hardens and takes on the appearance of splintered armor.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's skin takes on the appearance of splint mail.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_ARMOR] = MORPH_WEAPON_CHOP;
      return;
     }

     if ( !str_cmp( arg2, "bash" ) )
     {
      act( AT_WHITE, "Your skin hardens and takes on a layered appearance.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's skin takes on a layered appearance.", ch, NULL,  NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_ARMOR] = MORPH_WEAPON_BASH;
      return;
     }

     if ( !str_cmp( arg2, "tear" ) )
     {
      act( AT_WHITE, "Your skin hardens and takes on the appearance of plate armor.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's skin takes on the appearance of plate armor.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_ARMOR] = MORPH_WEAPON_CLAW;
      return;
     }

     if ( !str_cmp( arg2, "pierce" ) )
     {
      act( AT_WHITE, "Your skin takes on the appearance of chain armor.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's skin takes on the appearance of chain armor.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_ARMOR] = MORPH_WEAPON_PIERCE;
      return;
     }

     if ( !str_cmp( arg2, "none" ) )
     {
      act( AT_WHITE, "Your body reverts.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's skin returns to normal.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_ARMOR] = 0;
      return;
     }

     {
      send_to_char( AT_YELLOW, "Type values for armor: slash, chop, tear, bash, pierce\n\r", ch );
      return;
     }
    }

    if ( !str_cmp( arg1, "avoidance" ) )
    {
     if ( !str_cmp( arg2, "no" ) )
     {
      act( AT_WHITE, "You stop concentrating on avoiding attacks.", ch, NULL, NULL, TO_CHAR );
      ch->pcdata->morph[MORPH_AVOIDANCE] = 0;
      return;
     }

     if ( !str_cmp( arg2, "yes" ) )
     {
      act( AT_WHITE, "You start concentrating on avoiding attacks.", ch, NULL, NULL, TO_CHAR );
      ch->pcdata->morph[MORPH_AVOIDANCE] = 1;
      return;
     }

     {
      send_to_char( AT_YELLOW, "Type values for avoidance: yes, no\n\r", ch );
      return;
     }
    }

    if ( !str_cmp( arg1, "size" ) )
    {
     if ( flag_value( size_flags, arg2 ) != -99 )
     {
      act( AT_WHITE, "You alter your physical size.", ch, NULL, NULL, TO_CHAR );
      act( AT_WHITE, "$n's body contorts and changes.", ch, NULL, NULL, TO_ROOM );
      ch->pcdata->morph[MORPH_SIZE] = flag_value ( size_flags, arg2 );
      ch->size = flag_value( size_flags, arg2);
     }
     else
      send_to_char( AT_YELLOW,
       "Type values for size are: tiny, small, average, large, giant, huge, gargantuan, titanic\n\r"
       , ch );
     return;
    }

    if ( !str_cmp( arg1, "camoflauge" ) )
    {
     if ( !str_cmp( arg2, "no" ) )
     {
      act( AT_WHITE, "You stop concentrating on remaining unseen.", ch, 
       NULL, NULL, TO_CHAR );
      ch->pcdata->morph[MORPH_CAMO] = 0;
      return;
     }

     if ( !str_cmp( arg2, "yes" ) )
     {
      act( AT_WHITE, "You start concentrating on remaining unseen.", ch,
       NULL, NULL, TO_CHAR );
      ch->pcdata->morph[MORPH_CAMO] = 1;
      return;
     }

     {
      send_to_char( AT_YELLOW, "Type values for camoflauge: yes, no\n\r", ch );
      return;
     }
    }

     send_to_char( AT_YELLOW, "Syntax: &Cmorph <type> <type value>\n\r"
      "&WType being: &Cweapon, armor, avoidance, size, camoflauge\n\r", ch );
     send_to_char( AT_YELLOW, "Syntax: &Cmorph stat\n\r", ch );

 return;
}

void do_energysurge( CHAR_DATA *ch, char *argument )
{
   char arg [ MAX_INPUT_LENGTH ];
   int  amount = 0;

    one_argument( argument, arg );

   amount = atoi(arg);

   if ( ch->race != RACE_PIXIE )
        {
        send_to_char( AT_GREY, "What are you doing, you aren't a pixie.\n\r", ch );
        return;
        }

    if ( amount <= 0 )
	{
	    send_to_char(C_DEFAULT, "You can't surge 0.\n\r", ch );
	    return;
	}

    if ( amount >= ch->hit )
	{
	    send_to_char(C_DEFAULT, "You don't have enough health.\n\r", ch );
	    return;
	}

    ch->hit -= amount;
    ch->mana += amount;
    STUN_CHAR( ch, 2, STUN_TOTAL );
    ch->position = POS_STUNNED;

    send_to_char(AT_GREY, "You surge your mana!\n\r", ch );
    act(AT_GREY, "$n concentrates and starts glowing.", ch, NULL, NULL, TO_ROOM );

    return;
}
void do_healthsurge( CHAR_DATA *ch, char *argument )
{
   char arg [ MAX_INPUT_LENGTH ];
   int  amount = 0;

    one_argument( argument, arg );

   amount = atoi(arg);

   if ( ch->race != RACE_PIXIE )
   {
    typo_message(ch);
    return;
   }

   if ( amount <= 0 )
   {
    send_to_char(C_DEFAULT, "You can't surge 0.\n\r", ch );
    return;
   }

   if ( amount >= ch->mana )
   {
    send_to_char(C_DEFAULT, "You don't have enough mana.\n\r", ch );
    return;
   }


    ch->hit += amount;
    ch->mana -= amount;
    STUN_CHAR( ch, 2, STUN_TOTAL );
    ch->position = POS_STUNNED;

    send_to_char(AT_GREY, "You surge your health!\n\r", ch );
    act(AT_GREY, "$n concentrates and starts glowing.", ch, NULL, NULL, TO_ROOM );

    return;
}

void do_burst( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int dmg = 0;

    if ( IS_NPC( ch ) )
     return;

   if ( ch->race != RACE_PIXIE )
   {
    typo_message(ch);
    return;
   }

    if ( !ch->fighting )
    {
     send_to_char(C_DEFAULT, "You aren't fighting anyone.\n\r", ch );
     return;
    }

    if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_blindfight] > 0
    && ch->pcdata->learned[gsn_blindfight] < number_percent( ) )
    {
     if ( !check_blind( ch ) )
      return;
    }

    one_argument( argument, arg );

    victim = ch->fighting;

    if ( arg[0] != '\0' )
     if ( !( victim = get_char_room( ch, arg ) ) )
     {
      send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
      return;
     }

    dmg = number_range( 1, ch->level );
    if (ch->move < dmg)
    {
     send_to_char(C_DEFAULT, "You're too tired to burst.", ch );
     return;
    }
    ch->move -= dmg;
    damage( ch, victim, dmg, gsn_burst, DAM_POSITIVE, TRUE );
    return;
}

void do_headbutt( CHAR_DATA *ch, char *argument )
{
   CHAR_DATA *victim;
   char arg [ MAX_INPUT_LENGTH ];
   int timer = 24;
   int mod;
   int dmg;

   if ( ch->race != RACE_MINOTAUR )
   {
    typo_message(ch);
    return;
   }

   if ( ch->race_wait > 0 )
    return;

    if ( !ch->fighting )
    {
     send_to_char(C_DEFAULT, "You aren't fighting anyone.\n\r", ch );
     return;
    }

    one_argument( argument, arg );

    victim = ch->fighting;

    if ( arg[0] != '\0' )
     if ( !( victim = get_char_room( ch, arg ) ) )
     {
      send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
       return;
     }

   timer = 240;

   mod = ch->level / 5;
   mod = UMAX( 1, mod );
   timer = timer / mod;
   timer = ( timer < 24 ) ? 24 : timer;
   ch->race_wait = timer;
   act( AT_YELLOW, "You slam your head into $N's!", ch, NULL, victim, TO_CHAR );
   act( AT_YELLOW, "$n slams his head into yours!", ch, NULL, victim, TO_VICT );
   act( AT_YELLOW, "$n slams his head into $N's!", ch, NULL, victim, TO_NOTVICT );
   dmg = number_range( ch->level, ch->level * 5 );
   damage( ch, victim, dmg, gsn_headbutt, DAM_BASH, TRUE );
   STUN_CHAR( ch, 2, STUN_MAGIC );
   if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
        return;
   if ( number_percent() < 15 && victim->position != POS_STUNNED )
   {
    act( AT_WHITE, "$N reels from the blow...", ch, NULL, victim, TO_CHAR );
    act( AT_WHITE, "$N reels from the blow...", ch, NULL, victim, TO_NOTVICT );
    act( AT_WHITE, "You real from the blow and feel disoriented.", 
     ch, NULL, victim, TO_VICT );
    STUN_CHAR( victim, 2, STUN_TOTAL );
    victim->position = POS_STUNNED;
   }
   return;
}

void do_drowfire( CHAR_DATA *ch, char *argument )
{
  AFFECT_DATA af;
  CHAR_DATA   *victim;
  
  if ( ch->race != RACE_DROW )
  {
   typo_message(ch);
   return;
  }
  
  if(argument[0] != '\0')
  {
   if(!(victim = get_char_room(ch, argument)))
   {
    send_to_char(C_DEFAULT, "They aren't here.\n\r", ch);
    return;
   }
  }
  else
  {
   if(!(victim = ch->fighting))
   {
    send_to_char(C_DEFAULT, "You aren't fighting anyone.\n\r", ch);
    return;
   }
  }

  if ( is_affected( victim, gsn_drowfire ) )
   return;

  WAIT_STATE( ch, skill_table[gsn_drowfire].beats );
  af.type	= gsn_drowfire;
  af.level	= ch->level;
  af.duration	= 10;
  af.location	= APPLY_MDAMP;
  af.modifier	= -5;
  af.bitvector	= 0;
  affect_to_char( victim, &af );
  af.location	= APPLY_PDAMP;
  af.modifier	= -5;
  af.bitvector	= 0;
  affect_to_char( victim, &af );
  
  send_to_char(AT_PINK, "You are surrounded by a purple outline.\n\r", victim );
  act(AT_PINK, "$n is surrounded by a purple outline.", victim, NULL, NULL, TO_ROOM );

  if ( !victim->fighting )
   set_fighting( victim, ch );

  return;
}

void do_globedarkness( CHAR_DATA *ch, char *argument )
{
  ROOM_AFFECT_DATA raf;
  POWERED_DATA *pd;
  OBJ_DATA *globe;
  char buf[MAX_STRING_LENGTH];
  bool found = FALSE;

  if ( ch->race != RACE_DROW )
  {
   typo_message( ch );
   return;
  }

  if ( argument[0] == '\0' )
  {
   send_to_char( AT_DGREY, "Syntax: globe <create|dissipate|locate>\n\r", 
    ch );
   return;
  }

  if ( !str_cmp( argument, "locate" ) )
  {
   for ( pd = ch->powered; pd; pd = pd->next )
   {
    if ( pd->type == gsn_globedark )
    {
     sprintf( buf, "Globe of Darkness&w, &W%s&w; &cCost&w: &R%d&w.\n\r", 
      pd->room->name, pd->cost );
     send_to_char( AT_DGREY, buf, ch );
     found = TRUE;
    }
   }
   if ( !found )
    send_to_char( AT_CYAN, "You are not sustaining any &zGlobes&w.\n\r",
     ch );
   return;
  }
  if ( !str_cmp( argument, "dissipate" ) )
  {
   if ( !is_raffected( ch->in_room, gsn_globedark ) )
   {
    send_to_char( AT_CYAN, "There is no &zGlobe &cin this room&w.\n\r", ch);
    return;
   }
   for ( pd = ch->powered; pd; pd = pd->next )
   {
    if ( !pd )
     break;
    if ( pd->type == gsn_globedark )
    {
     found = TRUE;
    if ( pd->room == ch->in_room )
    {
     send_to_char( AT_DGREY, "You wave your hand and the globe dissipates.\n\r", ch );
     act( AT_DGREY, "The globe of darkness dissipates.", 
      ch, NULL, NULL, TO_ROOM );
     raffect_remove( ch->in_room, ch, pd->raf );
     return;
    }
   }
  }
  if ( !found )
  {
   send_to_char( AT_CYAN, "You are not sustaining any &zGlobes&w.\n\r", ch);
   return;
  }
  send_to_char( AT_CYAN, "You are not powering the &zGlobe&c in this room.\n\r", ch ); 
  return;
 }
 if ( !str_cmp( argument, "create" ) )
 {
  if ( is_raffected( ch->in_room, gsn_globedark ) )
  {
   send_to_char(AT_GREY, "A globe of darkness already sits in this room.\n\r", ch );
   return;
  }
	globe = create_object( get_obj_index( OBJ_VNUM_WARD_PHYS ), 1 );
        free_string( globe->name );
	free_string( globe->short_descr );
	free_string( globe->description );
	globe->name = str_dup( "globe darkness" );
	globe->short_descr = str_dup( "&za Globe of Darkness" );
	globe->description = str_dup( "A &zGlobe of Darkness &Xsits upon the room." );
	raf.type	= gsn_globedark;
	raf.location	= ROOM_NONE;
	raf.room	= ch->in_room;
	raf.material	= globe;
	raf.powered_by = ch;
	raffect_to_room( ch->in_room, ch, &raf );
	send_to_char( AT_DGREY, "You raise your hand and place of globe of darkness upon the area!\n\r", ch );
	act( AT_DGREY, "$n raises his hand and everything goes pitch black!",
	ch, NULL, NULL, TO_ROOM );
	return;
	}
  do_globedarkness( ch, "" );
}

void do_forge( CHAR_DATA *ch, char *argument )
{
  char arg[ MAX_INPUT_LENGTH ];
  char arg2[ MAX_INPUT_LENGTH ];
  char buf[ MAX_INPUT_LENGTH ];
  OBJ_DATA *obj, *hammer;
  int wear, lvl;
  wear = 0;
  if ( ch->race != RACE_DWARF )
  {
   typo_message(ch);
   return;
  }

  if ( argument[0] == '\0' )
	{
	send_to_char( AT_WHITE, "Syntax: Forge <obj> <race> <lvl>\n\r", ch );
	send_to_char( AT_WHITE, "  obj = ring necklace armor helm\n\r", ch );
	send_to_char( AT_WHITE, "        mask leggings boots gauntlets\n\r", ch );
	send_to_char( AT_WHITE, "        gauntlets armplates shield\n\r", ch );
	send_to_char( AT_WHITE, "        belt bracer anklet weapon\n\r", ch );
        send_to_char( AT_WHITE, "  race= any valid race. HELP FORGE RACES\n\r", ch );
	send_to_char( AT_WHITE, "        to see race groupings.\n\r", ch );
	sprintf( buf, "  lvl = minimum 30, maximum %d.\n\r", ch->level );
	send_to_char( AT_WHITE, buf, ch );
        send_to_char( AT_WHITE, "  BASE cost to make item is: 100 gold * lvl\n\r", ch );
	return;
	}

   for ( hammer = ch->carrying; hammer; hammer = hammer->next )
	{
	if ( hammer->item_type == ITEM_SMITHYHAMMER
	&& IS_SET( hammer->wear_loc, WEAR_WIELD_2 ) )
	  break;
	}
   if ( !hammer )
	{
	send_to_char( AT_GREY,
 "You must hold a smithy hammer in your secondary hand to forge something.\n\r", ch );  
	return;
	}
   argument = one_argument( argument, arg );
   argument = one_argument( argument, arg2 );
   if ( !str_prefix( arg, "ring" ) )
	wear = ITEM_WEAR_FINGER;
   if ( !str_prefix( arg, "necklace" ) )
	wear = ITEM_WEAR_NECK;
   if ( !str_prefix( arg, "armor" ) )
	wear = ITEM_WEAR_BODY;
   if ( !str_prefix( arg, "helm" ) )
	wear = ITEM_WEAR_HEAD;
   if ( !str_prefix( arg, "mask" ) )
	wear = ITEM_WEAR_FACE;
   if ( !str_prefix( arg, "leggings" ) )
	wear = ITEM_WEAR_LEGS;
   if ( !str_prefix( arg, "boots" ) )
	wear = ITEM_WEAR_FEET;
   if ( !str_prefix( arg, "gauntlets" ) )
	wear = ITEM_WEAR_HANDS;
   if ( !str_prefix( arg, "armplates" ) )
	wear = ITEM_WEAR_ARMS;
   if ( !str_prefix( arg, "shield" ) )
	wear = ITEM_WEAR_SHIELD;
   if ( !str_prefix( arg, "belt" ) )
	wear = ITEM_WEAR_WAIST;
   if ( !str_prefix( arg, "bracer" ) )
	wear = ITEM_WEAR_WRIST;
   if ( !str_prefix( arg, "anklet" ) )
	wear = ITEM_WEAR_ANKLE;
   if ( !str_prefix( arg, "weapon" ) )
	wear = ITEM_WIELD;
   if ( is_number( argument ) )
	lvl = atoi( argument );	
   else
	lvl = 0;
   if ( wear && ( lvl < 30 || lvl > ch->level ) )
	{
	sprintf( buf, "Illegal level.  Valid levels are 30 to %d.\n\r", ch->level );
	send_to_char( AT_GREY, buf, ch );
	return;
	}
   if ( wear )
	{
	if ( (ch->money.gold + (ch->money.silver/SILVER_PER_GOLD) +
	     (ch->money.copper/COPPER_PER_GOLD) ) < (lvl * 50) )
	{
	  send_to_char( AT_GREY, "You do not have enough money to create the base item of this level.\n\r", ch );
	  return;
	  }
	else
	if ( wear == ITEM_WIELD )
	obj = create_object( get_obj_index( OBJ_VNUM_TO_FORGE_W ), lvl );
	else
	obj = create_object( get_obj_index( OBJ_VNUM_TO_FORGE_A ), lvl );
	obj->cost.silver = obj->cost.copper = 0;
	obj->cost.gold = lvl * 50;
	obj->weight = lvl * 0.15;
	obj->level = lvl;
	if ( obj->level >= 101 )
	  obj->extra_flags += ITEM_NO_DAMAGE;
	obj->wear_flags += wear;
	forge_obj( ch, obj );
	}
   else
	do_forge( ch, "" );
   return;
}
void do_spit( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim;
  char arg[ MAX_INPUT_LENGTH ];
  int dam;
  if ( ch->race != RACE_DRACONI && ch->race != RACE_AQUASAPIEN )
    {
     typo_message( ch );
     return;
    }
   if ( ch->race_wait > 0 )
        return;
    if ( !ch->fighting )
    {
        send_to_char(C_DEFAULT, "You aren't fighting anyone.\n\r", ch );
        return;
    }
    one_argument( argument, arg );

    victim = ch->fighting;

    if ( arg[0] != '\0' )
        if ( !( victim = get_char_room( ch, arg ) ) )
        {
            send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
            return;
        }
  dam = ch->level + number_range( ch->level, ch->level * 4 );
  spell_acid_spray( skill_lookup("acid spray"), ch->level, ch, victim );
  ch->race_wait = 20;
  return;
}
