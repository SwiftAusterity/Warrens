/*****************************************************************************
 * Room affect stuff.                                                        *
 * Put skills that use room affects in here.                                 *
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

/* Globals */
ROOM_AFFECT_DATA *	raffect_free;
EXIT_AFFECT_DATA *	exit_affect_free;
POWERED_DATA *		pd_free;

/*
 * Slap a rAffect on a room and update ch->powered.
 */
void raffect_to_room( ROOM_INDEX_DATA *room, 
		      CHAR_DATA *ch, ROOM_AFFECT_DATA *raf )
{
  ROOM_AFFECT_DATA *raf_new;
  POWERED_DATA *pd;

  if ( !raffect_free )
    raf_new	 = alloc_perm( sizeof( *raf_new ) );
  else
  {
    raf_new	 = raffect_free;
    raffect_free = raffect_free->next;
  }

  if ( !pd_free )
    pd		 = alloc_perm( sizeof( *pd ) );
  else
  {
    pd		 = pd_free;
    pd_free	 = pd_free->next;
  }

  *raf_new	 = *raf;
  raf_new->next	 = room->rAffect;
  room->rAffect	 = raf_new;
  pd->room	 = room;
  pd->raf	 = raf_new;
  pd->next	 = ch->powered;
  pd->type	 = raf_new->type;
  pd->cost	 = SPELL_COST( ch, pd->type );

  if ( ch->race == RACE_AQUINIS || ch->race == RACE_ELF )
    	pd->cost *= .75;

  ch->powered	 = pd;
  obj_to_room( raf->material, room );

  if ( raf_new->location != ROOM_NONE )
	TOGGLE_BIT( room->room_flags, raf->location );

  return;

}
/*
 * Remove and delete a rAffect and update ch->powered.
 */
void raffect_remove( ROOM_INDEX_DATA *room, 
                     CHAR_DATA *ch, ROOM_AFFECT_DATA *raf )
{
  ROOM_AFFECT_DATA *rAf, *prAf;
  POWERED_DATA *pd, *ppd;
  char buf[ MAX_STRING_LENGTH ];

  if ( !room->rAffect )
  {
    bug( "rAffect_remove: no affect.", 0 );
    return;
  }

  if ( raf->location != ROOM_NONE )
    TOGGLE_BIT( room->room_flags, raf->location );

  sprintf( buf, "%s", skill_table[raf->type].room_msg_off );

  if ( ch->in_room == room )
    act( AT_CYAN, buf, ch, NULL, NULL, TO_ROOM );
  else if ( room->people )
    act( AT_CYAN, buf, room->people, NULL, NULL, TO_ROOM );

  ppd = NULL;
  for ( pd = ch->powered; pd; ppd = pd, pd = pd->next )
  {
   if ( pd->room == room && pd->raf == raf )
   {
     if ( !ppd )
	ch->powered = pd->next;
     else
	ppd->next = pd->next;
     if ( MT( ch ) < pd->cost )
     {
        sprintf( buf, "%s %s.\n\r", skill_table[raf->type].msg_off,
		 raf->room->name );
	send_to_char( AT_CYAN, buf, ch );
     }

     break;

   }
  }

  prAf = NULL;
  for ( rAf = room->rAffect; rAf; prAf = rAf, rAf = rAf->next )
  {
    if ( raf == rAf )
    {
      if ( !prAf )
	room->rAffect = rAf->next;
      else
	prAf->next = rAf->next;
    
      break;
    }
  }

  extract_obj( raf->material );
  free_mem( raf, sizeof( *raf ) );
  free_mem( pd, sizeof( *pd ) );

  return;

}
/*
 * Check if room is rAffected by something.
 */
bool is_raffected( ROOM_INDEX_DATA *room, int sn )
{
  ROOM_AFFECT_DATA *raf;

  for ( raf = room->rAffect; raf; raf = raf->next )
  {
    if ( raf->type == sn )
      return TRUE;
  }

  return FALSE;

}

/*
 * To see what you're powering ( imms see room's rAffects as well ).
 */
void do_raffect( CHAR_DATA *ch, char *argument )
{
  POWERED_DATA *pd;
  ROOM_AFFECT_DATA *raf;
  char buf[ MAX_INPUT_LENGTH ];

  if ( IS_NPC( ch ) )
	return;

  if ( !ch->powered )
    send_to_char( AT_CYAN, "You are not powering any room affects.\n\r", ch );

  for ( pd = ch->powered; pd; pd = pd->next )
  {
    sprintf( buf, "%s&w, &cMana Cost&w: &R%d&w, &cRoom&w: &W%s&w.\n\r",
		 skill_table[pd->type].name,
		 pd->cost,
		 pd->room->name );
    buf[0] = UPPER(buf[0]);
    send_to_char( AT_WHITE, buf, ch );

  }

  if ( ch->level >= L_APP )
  {
    send_to_char(AT_CYAN, "This room is affected by:\n\r", ch );
    for ( raf = ch->in_room->rAffect; raf; raf = raf->next )
    {
	sprintf( buf, "%s &cpowered by&w: &W%s&w.\n\r",
		 skill_table[raf->type].name,
		 raf->powered_by->name );
	buf[0] = UPPER(buf[0]);
	send_to_char(AT_WHITE, buf, ch );
    }
  }

  return;

}
/*
 * Remove all rAffects ch is powering. (mostly for quitting)
 */
void raffect_remall( CHAR_DATA *ch )
{
  POWERED_DATA *pd, *pd_next;

  for ( pd = ch->powered; pd; pd = pd_next )
  {
    if ( !pd )
      return;
    pd_next = pd->next;
    raffect_remove( pd->room, ch, pd->raf );

  }

  return;

}
/*
 * Toggle a room's rAffect location. (for asave_area)
 */
void toggle_raffects( ROOM_INDEX_DATA *room )
{
  ROOM_AFFECT_DATA *raf;

  for ( raf = room->rAffect; raf; raf = raf->next )
  {
     if ( !raf )
        break;
     if ( raf->location != ROOM_NONE )
	TOGGLE_BIT( room->room_flags, raf->location );

  }

  return;

}
/*
 * Locate or turn off a specific type of rAffect skill.
 */
void loc_off_raf( CHAR_DATA *ch, int gsn_raf, bool rOff )
{
  POWERED_DATA *pd;
  char buf [ MAX_STRING_LENGTH ];
  bool found = FALSE;

  if ( !rOff ) /* Locate a rAffect of type gsn_raf. */
  {
    for ( pd = ch->powered; pd; pd = pd->next )
    {
      if ( pd->type == gsn_raf )
      {
        sprintf( buf, "%s&w, &W%s&w; &cCost&w: &R%d&w.\n\r", 
		skill_table[gsn_raf].name, pd->room->name, pd->cost );
		send_to_char( AT_WHITE, buf, ch );
		found = TRUE;
      }
    }
      if ( !found )
      {
        sprintf( buf, "You are not sustaining any %s&w.\n\r",
		   skill_table[gsn_raf].name );
	send_to_char( AT_CYAN, buf, ch );
      }

      return;

    }

   /* Delete a rAffect of gsn_raf from room. */
   if ( !is_raffected( ch->in_room, gsn_raf ) )
   {
     sprintf( buf, "There is no %s &cin this room.\n\r",
	      skill_table[gsn_raf].name );
     send_to_char( AT_CYAN, buf, ch );

     return;

   }

   for ( pd = ch->powered; pd; pd = pd->next )
   {
     if ( !pd )
       break;
     if ( pd->type == gsn_raf )
     {
       found = TRUE;
       if ( pd->room == ch->in_room )
       {
	 sprintf( buf, "%s\n\r", skill_table[gsn_raf].noun_damage );
	 send_to_char( AT_WHITE, buf, ch );
         raffect_remove( ch->in_room, ch, pd->raf );
         return;
       }
     }
   }

   if ( !found )
   {
     sprintf( buf, "You are not sustaining any %s&w.\n\r",
		skill_table[gsn_raf].name );
     return;
   }

   sprintf( buf, "You are not powering the %s &cin this room.\n\r",
	      skill_table[gsn_raf].name );
   send_to_char( AT_CYAN, buf, ch ); 

   return;

}
/*
 * Main ward skill.  Does NOTHING if no ward is known by ch.
 */
void do_ward( CHAR_DATA *ch, char *argument )
{
  ROOM_AFFECT_DATA raf;
  OBJ_DATA *ward;
  char arg1[ MAX_INPUT_LENGTH ];
  char buf[ MAX_STRING_LENGTH ];
  char *wname, *shortd, *longd;
  int type = 0;
  int location = ROOM_NONE;

  if ( IS_NPC( ch ) )
    return;

  if ( argument[0] == '\0' )
  {
    send_to_char( AT_WHITE, "Syntax: ward <type> <on|off|locate>\n\r", ch );
    return;
  }

  wname = shortd = longd = "";
  argument = one_argument( argument, arg1 );

  if ( !str_cmp( arg1, "safety" ) )
  {
    type = gsn_ward_safe;
    location = ROOM_SAFE;
    wname = "ward safety";
    shortd = "some Wards of Safety";
    longd = "Some wards of safety have been raised in this room.";
  }
  
  if ( !str_cmp( arg1, "healing" ) )
  {
    type = gsn_ward_heal;
    location = ROOM_NONE;
    wname = "ward heal";
    shortd = "some Wards of Healing";
    longd = "Some wards of healing have been raised in this room.";
  }

  if ( type == 0 )
  {
    send_to_char(AT_GREY, "No such ward.\n\r", ch );
    return;
  }

  if ( !can_use_skpell( ch, type ) )
  {
    sprintf( buf, "You aren't skilled in the ward of %s.\n\r", arg1 );
    send_to_char(AT_GREY, buf, ch );
    return;
  }

  if ( !str_cmp( argument, "locate" ) )
  {
    loc_off_raf( ch, type, FALSE );
    return;
  }

  if ( !str_cmp( argument, "off" ) )
  {
    loc_off_raf( ch, type, TRUE );
    return;
  }

  if ( str_cmp( argument, "on" ) )
  {
    do_ward( ch, "" );
    return;
  }

  if ( ch->pcdata->learned[type] < number_percent( ) )
  {
    sprintf( buf, "You fail in raising your ward of %s.\n\r", arg1 );
    send_to_char(AT_GREY, buf, ch );
    return;
  }

  if ( type == gsn_ward_safe
  && IS_SET( ch->in_room->room_flags, ROOM_SAFE ) )
  {
    send_to_char(AT_WHITE, "This room is already safe.\n\r", ch );
    return;
  }

  if ( type == gsn_ward_heal
  && is_raffected( ch->in_room, gsn_ward_heal ) )
  {
    send_to_char( AT_WHITE, "This room is already warded to heal.\n\r", ch );
    return;
  }

  ward = create_object( get_obj_index( OBJ_VNUM_WARD_PHYS ), 1 );

  free_string( ward->short_descr );
  free_string( ward->description );
  free_string( ward->name );
  ward->name	    = str_dup( wname );
  ward->short_descr = str_dup( shortd );
  ward->description = str_dup( longd );
  raf.type	 = type;
  raf.location	 = location;
  raf.material	 = ward;
  raf.room	 = ch->in_room;
  raf.powered_by = ch;
  raffect_to_room( ch->in_room, ch, &raf );

  sprintf( buf, "You raise a %s.\n\r", skill_table[type].name );
  send_to_char( AT_WHITE, buf, ch );
  act( AT_WHITE, "$n raises a $t.", 
       ch, (char *)skill_table[type].name, NULL, TO_ROOM );

  if ( type == gsn_ward_heal )
    update_skpell( ch, gsn_ward_heal );

  if ( type == gsn_ward_safe )
    update_skpell( ch, gsn_ward_safe );

  return;  
}


void do_traproom( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *Obj;
  OBJ_DATA *tripwire;
  OBJ_DATA *pilewire;
  ROOM_INDEX_DATA *to_room;
  char buf[MAX_STRING_LENGTH];

  to_room = ch->in_room;

  Obj = get_eq_char( ch, WEAR_WIELD_2 );

  if IS_SET( to_room->room_flags, ROOM_SAFE )
  {
      send_to_char( C_DEFAULT, "You can not trap a safe room.\n\r", ch );
      return;
  }

  if IS_SET( to_room->room_flags, ROOM_TRAPPED )
  {
      send_to_char( C_DEFAULT, "The room is trapped.\n\r", ch );
      return;
  }

  if  ( Obj == NULL )
  {
      send_to_char( C_DEFAULT, "You aren't holding anything in your secondary hand.\n\r", ch );
      return;
  }

  for ( tripwire = ch->carrying; tripwire; tripwire = tripwire->next_content )
   {
   if ( tripwire->item_type == ITEM_TRIPWIRE )
	break;
   }
  if ( !tripwire )
   {
   send_to_char( AT_GREY, "You need a trip wire to trap a room.\n\r", ch );
   return;
   }

  for ( pilewire = ch->carrying; pilewire; pilewire = pilewire->next_content )
   {
   if ( pilewire->item_type == ITEM_PILEWIRE )
	break;
   }
  if ( !pilewire )
   {
   send_to_char( AT_GREY, "You need circuitry to trap a room.\n\r", ch );
   return;
   }

  if  ( Obj->item_type != ITEM_BOMB )
  {
  send_to_char( C_DEFAULT, "You aren't holding a bomb.\n\r", ch );
  return;
  }
  
  send_to_char( C_DEFAULT, "You trap the room.\n\r", ch );
	  sprintf( buf, 
      "%s lines some thin wire around the entrances of the room.", ch->oname );
	act( AT_WHITE, buf, ch, Obj, NULL, TO_ROOM );
  unequip_char( ch, Obj );
  obj_from_char( Obj );
  Obj->owner = ch;
  bomb_to_room( Obj, to_room );

  SET_BIT( to_room->room_flags, ROOM_TRAPPED );
    update_skpell( ch, gsn_traproom );
  extract_obj( tripwire );
  extract_obj( pilewire );
  return;
}

void do_trapdisarm( CHAR_DATA *ch, char *argument )
{
  ROOM_INDEX_DATA *to_room;
  OBJ_DATA        *tools;
  EXIT_DATA       *pexit;
    char          arg1 [ MAX_INPUT_LENGTH  ];
    int           door = 0;

  to_room = ch->in_room;

    argument = one_argument( argument, arg1 );

  if ( argument[0] == '\0' )
  {
    send_to_char( AT_WHITE, "Which direction is the room you want to disarm?", ch );
    return;
  }


  for ( tools = ch->carrying; tools; tools = tools->next )
   {
   if ( tools->item_type == ITEM_TOOLPACK )
	break;
   }
  if ( !tools )
   {
   send_to_char( AT_GREY, "You need a set of tools to disarm a bomb.\n\r", ch );
   return;
   }


         if ( !str_prefix( arg1, "north" ) ) door = 0;
    else if ( !str_prefix( arg1, "east"  ) ) door = 1;
    else if ( !str_prefix( arg1, "south" ) ) door = 2;
    else if ( !str_prefix( arg1, "west"  ) ) door = 3;
    else if ( !str_prefix( arg1, "up"    ) ) door = 4;
    else if ( !str_prefix( arg1, "down"  ) ) door = 5;

  pexit = to_room->exit[door];

  to_room = pexit->to_room;

  if ( !IS_SET( to_room->room_flags, ROOM_TRAPPED ))
  {
      send_to_char( C_DEFAULT, "The room isn't trapped.\n\r", ch );
      return;
  }


  send_to_char( C_DEFAULT, "You disarm the room.\n\r", ch );

  if (IS_SET( to_room->room_flags, ROOM_TRAPPED ))
  REMOVE_BIT( to_room->room_flags, ROOM_TRAPPED );
    update_skpell( ch, gsn_trapdisarm );

  return;
}

void exit_affect_to_room( EXIT_DATA *exit, EXIT_AFFECT_DATA *exitaf )
{
  EXIT_AFFECT_DATA *exitaf_new;

  if ( !exit_affect_free )
    exitaf_new	 = alloc_perm( sizeof( *exitaf_new ) );
  else
  {
    exitaf_new	 = exit_affect_free;
    exit_affect_free = exit_affect_free->next;
  }

  *exitaf_new	 = *exitaf;
  exitaf_new->next	 = exit->eAffect;
  exit->eAffect	 = exitaf_new;

  if ( exitaf_new->location != EXIT_NONE )
	TOGGLE_BIT( exit->exit_affect_flags, exitaf->location );

  return;
}

void exit_affect_remove( EXIT_DATA *exit, EXIT_AFFECT_DATA *exitaf )
{
  EXIT_AFFECT_DATA *exitAf, *pexitAf;
  char buf[ MAX_STRING_LENGTH ];

  if ( !exit->eAffect )
  {
    bug( "eAffect_remove: no affect.", 0 );
    return;
  }

  sprintf( buf, "%s", skill_table[exitaf->location].room_msg_off );

  if ( exit->to_room->people )
    act( AT_CYAN, buf, exit->to_room->people, NULL, NULL, TO_ROOM );

  pexitAf = NULL;
  for ( exitAf = exit->eAffect; exitAf; pexitAf = exitAf, 
   exitAf = exitAf->next )
  {
    if ( exitaf == exitAf )
    {
      if ( !pexitAf )
	exit->eAffect = exitAf->next;
      else
	pexitAf->next = exitAf->next;
    
      break;
    }
  }

  free_mem( exitaf, sizeof( *exitaf ) );
  return;
}

/*
 * Check if exit is rAffected by something.
 */
bool is_exit_affected( EXIT_DATA *exit, int sn )
{
  EXIT_AFFECT_DATA *exitaf;

  for ( exitaf = exit->eAffect; exitaf; exitaf = exitaf->next )
  {
    if ( exitaf->type == sn )
      return TRUE;
  }

  return FALSE;
}

EXIT_AFFECT_DATA *get_exit_affect( EXIT_DATA *exit, int sn )
{
  EXIT_AFFECT_DATA *exitaf;

  for ( exitaf = exit->eAffect; exitaf; exitaf = exitaf->next )
  {
   if ( !exitaf )
    continue;

    if ( exitaf->type == sn )
      return exitaf;
  }

  return NULL;
}


