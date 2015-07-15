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



char *  const   dir_noun        [ ]             =
{
    "the north", "the east", "the south", "the west", "above", "below"
};

char *	const	dir_name	[ ]		=
{
    "north", "east", "south", "west", "up", "down"
};

const	int	rev_dir		[ ]		=
{
    2, 3, 0, 1, 5, 4
};

const	int	movement_loss	[ SECT_MAX ]	=
{
    0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0
};



/*
 * Local functions.
 */
int	find_door	args( ( CHAR_DATA *ch, char *arg, bool pMsg ) );
OBJ_DATA *has_key	args( ( CHAR_DATA *ch, int key ) );


void move_char( CHAR_DATA *ch, int door, bool Fall, bool flee ) 
{
    AFFECT_DATA af;
    OBJ_DATA        *obj;
    OBJ_DATA	    *bomb;
    CHAR_DATA       *fch;
    CHAR_DATA       *fch_next;
    EXIT_DATA       *pexit;
    ROOM_INDEX_DATA *in_room;
    ROOM_INDEX_DATA *to_room;
    ROOM_INDEX_DATA *location;
    ROOM_INDEX_DATA *pRoomIndex;
    char            buf[MAX_STRING_LENGTH];
    int             door2 = 0;
    int		    nd = 0;
    bool	    haveboat = FALSE;
    bool	    needboat = FALSE;

    bool     retreat = FALSE;
    bool     havesmokebomb = FALSE;
    OBJ_DATA *smokebomb = NULL;


    if ( (ch->engaged) )
    {
     send_to_char( AT_YELLOW, "You must disengage firing to move.\n\r", ch );
     return;
    }

    /* This is for sphere_of_solitude -Flux */
    for ( obj = ch->in_room->contents; obj; obj = obj->next_content )
    {
	if ( obj->deleted )
	    continue;

     if ( obj->pIndexData->vnum == OBJ_VNUM_SPHERE_OF_SOLITUDE )
     {
      send_to_char(AT_WHITE, "You cannot break through the sphere of solitude.\n\r", ch);
      return;
     }
    }

    if ( ( IS_AFFECTED(ch, AFF_ANTI_FLEE) ) && ( !Fall ) )
     {
      send_to_char(AT_WHITE, "You cannot move.\n\r", ch);
      return;
     }

     if ( IS_STUNNED( ch, STUN_TOTAL ) || IS_STUNNED( ch, STUN_COMMAND ) )
     {
      send_to_char(AT_WHITE, "You are too stunned to move.\n\r", ch);
      return;
     }


    if ( IS_AFFECTED2(ch, AFF_TORTURING) )
    {
     send_to_char(AT_WHITE, "You are torturing someone and cannot leave the room.\n\r", ch);
     return;
    }

    if ( door < 0 || door > 5 )
    {
	bug( "Do_move: bad door %d.", door );
	return;
    }

    if ( (ch->fighting) && (ch->fighting)->in_room == ch->in_room )
    {
     int chstat;
     int victstat;

     if ( !flee )
     {
      chstat = get_curr_dex(ch) + get_curr_agi(ch) +
              get_curr_str(ch) + dice(1, 20);
      victstat = get_curr_dex(ch->fighting) + get_curr_agi(ch->fighting) +
              (get_curr_str(ch->fighting)/2) + dice(3, 10);

      if ( chstat < victstat )
      {
       STUN_CHAR(ch, 2, STUN_TOTAL );
       act(AT_RED,
        "$N jumps in your way, blocking your exit!",
        ch, NULL, ch->fighting, TO_CHAR );
       act(AT_YELLOW,
        "$N jumps in $n's way, blocking $s exit!",
        ch, NULL, ch->fighting, TO_NOTVICT );
       act(AT_YELLOW,
        "You jump in $n's way, blocking $s exit!",
        ch, NULL, ch->fighting, TO_VICT );
       return;
      }
     }
     else
     {
      chstat = get_curr_dex(ch) + get_curr_agi(ch) +
              get_curr_str(ch) + dice(1, 30);
      victstat = get_curr_dex(ch->fighting) + get_curr_agi(ch->fighting) +
              (get_curr_str(ch->fighting)/2) + dice(2, 10);

      if ( !IS_NPC(ch) && can_use_skpell( ch, gsn_retreat ) )
      {
       for ( smokebomb = ch->carrying; smokebomb;
        smokebomb = smokebomb->next_content )
       {
        if ( !smokebomb || smokebomb->deleted )
         continue;
        if ( smokebomb->item_type == ITEM_SMOKEBOMB )
        {
         havesmokebomb = TRUE;
         break;
        }
       }

       if ( havesmokebomb == TRUE && (smokebomb) )
       {
        chstat += ch->pcdata->learned[gsn_retreat]/5;
        retreat = TRUE;
        update_skpell( ch, gsn_retreat ); 
       }

       if ( chstat < victstat )
       {
        STUN_CHAR(ch, 2, STUN_TOTAL );
        act(AT_RED,
         "$N jumps in your way, blocking your exit!",
         ch, NULL, ch->fighting, TO_CHAR );
        act(AT_YELLOW,
         "$N jumps in $n's way, blocking $s exit!",
         ch, NULL, ch->fighting, TO_NOTVICT );
        act(AT_YELLOW,
         "You jump in $n's way, blocking $s exit!",
         ch, NULL, ch->fighting, TO_VICT );
        return;
       }
      }
     }
    }

    if ( (ch->fighting) )
     flee = TRUE;

    in_room = ch->in_room;
    
    if ( !IS_NPC( ch ) && ( !Fall ) )
    {
      int drunk = 0;
      drunk = ch->pcdata->condition[COND_DRUNK];

      if ( number_percent() < drunk )
      {
        for ( nd = door; nd == door; nd = number_door() );
        door = nd;
        send_to_char( AT_BLUE, "You're too think to drunk clearly! You wander"
        " off in the wrong direction.\n\r", ch );
      }
      else
      if ( IS_AFFECTED( ch, AFF_INSANE ) && number_percent() / 4 > get_curr_int( ch ) )
      {
        for ( nd = door; nd == door; nd = number_door() );
        door = nd;
      }
    }

    if ( !IS_NPC( ch ) && ( !Fall ) )
    {
      
      if ( ch->pcdata->condition[COND_INSOMNIA] == 0 &&
           number_percent() > get_curr_con(ch) * 3 )
      {
       send_to_char( AT_BLUE, "You close your eyes for a moment to try and get some sleep and you end up wandering in the wrong direction.\n\r", ch );
        for ( nd = door; nd == door; nd = number_door() );
        door = nd;
      }
    }
    
    if ( !( pexit = in_room->exit[door] ) || !( to_room = pexit->to_room ) )
    {
	send_to_char(AT_GREY, "Alas, you cannot go that way.\n\r", ch );
	return;
    }

    if ( exit_blocked(pexit, ch->in_room) == EXIT_STATUS_PHYSICAL )
    {
     if ( (IS_NPC(ch) && !IS_SET( ch->act, ACT_ILLUSION )) ||
          (!IS_NPC(ch) && !IS_SET( ch->act, PLR_ILLUSION )) )
     {
        if ( !IS_AFFECTED( ch, AFF_PASS_DOOR ) )
        {
	    act(AT_GREY, "The &W$d&w is closed or blocked.",
		ch, NULL, pexit->keyword, TO_CHAR );
	    return;
	}
	if ( IS_SET( pexit->exit_info, EX_PASSPROOF ) )
        {
	    act(AT_GREY, "You are unable to pass through the &W$d&w.",
		ch, NULL, pexit->keyword, TO_CHAR );
	    return;
	}
     }
    }

    if ( IS_AFFECTED( ch, AFF_CHARM )
	&& ch->master
	&& in_room == ch->master->in_room && ( !Fall ) )
    {
	send_to_char(AT_GREY, "What? And leave your beloved master?\n\r", ch );
	return;
    }

    if ( room_is_private( to_room ) )
    {
	send_to_char(AT_GREY, "That room is private right now.\n\r", ch );
	return;
    }

    if ( exit_blocked( pexit, ch->in_room ) == EXIT_STATUS_MAGICAL )
    {
     send_to_char( AT_WHITE, "An magical field bars your entry.\n\r", ch );
     return;
    }

    if ( exit_blocked( pexit, ch->in_room ) == EXIT_STATUS_SNOWIN )
    {
     send_to_char( AT_WHITE, "There's too much snow, you can not go that way.\n\r", ch );
     return;
    }

    if ( !IS_NPC( ch ) )
    {
	int move;

	if ( ( rsector_check( in_room ) == SECT_AIR
	    || rsector_check( to_room ) == SECT_AIR ) && ( !Fall ) )
	{
	    if ( !is_flying )
	    {
		send_to_char(AT_GREY, "You can't fly.\n\r", ch );
		return;
	    }
	}

	if ( rsector_check( in_room ) == SECT_WATER_SURFACE
	    || rsector_check( to_room ) == SECT_WATER_SURFACE )
	{
	 OBJ_DATA		*obj;
	 bool			found;
       ROOM_INDEX_DATA	*deepwater;
       EXIT_DATA		*waterexit;

       if ( ( waterexit = to_room->exit[5] ) != NULL )
       {
        if ( ( deepwater = waterexit->to_room ) != NULL )
        {
          needboat = FALSE;
        }
        else
        {
	   /* Suggestion for flying above water by Sludge */
         if ( rsector_check( deepwater ) == SECT_UNDERWATER )
          if ( (get_race_data(ch->race))->swimming == 0 || 
               !is_flying(ch) )
           needboat = TRUE;
        }
       }

	    /*
	     * Look for a boat.
	     */
	    found = FALSE;

	    for ( obj = ch->carrying; obj; obj = obj->next_content )
	    {
		if ( obj->item_type == ITEM_BOAT )
		{
		    found = TRUE;
		    break;
		}
	    }

	    if ( ( !found ) && ( !Fall ) )
             ;
            else
             haveboat = TRUE;
	}

        if ( in_room->area != to_room->area )
        {
         if ( !IS_NPC( ch ) )
         {
          if ( in_room->area->temporal != to_room->area->temporal
           && ( !IS_AFFECTED( ch, AFF_TEMPORAL ) &&
                ch->race != RACE_CHRONOSAPIEN ) )
          {
	     send_to_char(AT_GREY, "Alas, you cannot go that way.\n\r", ch );
	     return;
          }
         }
         else
         {
	   send_to_char(AT_GREY, "Alas, you cannot go that way.\n\r", ch );
	   return;
         }
        }

	  move = movement_loss[UMIN( SECT_MAX-1, rsector_check ( to_room ) )];

        if ( !IS_NPC( ch ) )
        {
         if ( needboat && haveboat )
          move = 1;
 
         if ( rsector_check( to_room ) == SECT_UNDERWATER &&
             (get_race_data(ch->race))->swimming == 1 )
           move = 1;
        }

	if ( ( ch->move < move ) && ( !Fall ) )
	{
	    send_to_char(AT_GREY, "You are too exhausted.\n\r", ch );
	    return;
	}

	WAIT_STATE( ch, 1 );
	if ( !is_flying(ch) )
       ch->move -= move;
    }

     if ( ch->hit < MAX_HIT(ch) /2 )
     {
      char      buf[MAX_STRING_LENGTH];

      char *	const	dir_blood[ ] =
       { "north", "east", "south", "west", "above", "below" };

      send_to_char( AT_BLOOD, "Blood trickles from your wounds.\n\r", ch );
      act( AT_RED, "$n leaves a trail of blood.", ch , NULL, NULL, TO_ROOM );

      obj		= create_object(get_obj_index( OBJ_VNUM_BLOOD ), 0 );
      obj->timer	= number_range( 2, 4 );

      sprintf( buf , obj->description, dir_blood[door] );
      free_string( obj->description );
      obj->description = str_dup( buf );

      obj_to_room( obj, ch->in_room );
     }

   if ( !flee )
   {
    if ( !IS_AFFECTED( ch, AFF_SNEAK )
	&& ( IS_NPC( ch ) || !IS_SET( ch->act, PLR_WIZINVIS ) )
	&& ( ch->race != RACE_KENDER ) && ( !Fall ) )
    {
     if ( !IS_NPC( ch ) && ch->pcdata->walkout[0] != '\0' )
      sprintf( buf, "%s to %s", ch->pcdata->walkout, dir_noun[door] ); 
     else
      sprintf( buf, "$n leaves %s.", dir_name[door] );

     act(AT_GREY, buf, ch, NULL, NULL, TO_ROOM );
    }

    eprog_enter_trigger( pexit, ch->in_room, ch );
    if ( ch->in_room != to_room )
    {
      char_from_room( ch );
      char_to_room( ch, to_room );
    }

    if ( door == 0 || door == 1 )
    door2 = door + 2;   
    else if ( door == 2 || door == 3 )
    door2 = door - 2;   
    else if ( door == 4 )
    door2 = door + 1;
    else if ( door == 5 )
    door2 = door - 1;

    if ( !IS_AFFECTED( ch, AFF_SNEAK )
	&& ( IS_NPC( ch ) || !IS_SET( ch->act, PLR_WIZINVIS ) )
	&& ( ch->race != RACE_KENDER ) && ( !Fall ) )
    {
     if ( !IS_NPC( ch ) && ch->pcdata && ch->pcdata->walkin[0] != '\0' )
      sprintf( buf, "%s from %s", ch->pcdata->walkin, dir_noun[door2] ); 
     else
      sprintf( buf, "$n arrives from %s.", dir_noun[door2] );

     act(AT_GREY, buf, ch, NULL, NULL, TO_ROOM );
    }
   }
   else
   {
    if ( (havesmokebomb) && (smokebomb) )
    {
     act( AT_DGREY, "$n throws $p to the ground and...", ch,
      smokebomb, NULL, TO_ROOM );
     act( AT_DGREY, "You throw $p to the ground and...", ch,
      smokebomb, NULL, TO_CHAR );

     sprintf( buf, "You retreat to %s before the smoke clears.\n\r",
      dir_noun[door] );
     send_to_char( AT_DGREY, buf, ch );
     act(AT_DGREY, 
      "Smoke rises from $p... when it clears, $n is gone!", 
      ch, smokebomb, NULL, TO_ROOM );
     extract_obj( smokebomb );
    }
    else
    {
     sprintf( buf, "$n has fled to %s!\n\r", dir_noun[door] );
     act( AT_DGREY, buf, ch, NULL, NULL, TO_ROOM );
     sprintf( buf, "You flee to %s!\n\r", dir_noun[door] );
     act( AT_DGREY, buf, ch, NULL, NULL, TO_CHAR );
    }

    if ( ch->in_room != to_room )
    {
     char_from_room( ch );
     char_to_room( ch, to_room );
    }

    if ( door == 0 || door == 1 )
     door2 = door + 2;   
    else if ( door == 2 || door == 3 )
     door2 = door - 2;   
    else if ( door == 4 )
     door2 = door + 1;
    else if ( door == 5 )
     door2 = door - 1;

    sprintf( buf, "$n comes running in from %s!\n\r", dir_noun[door2] );
    act( AT_DGREY, buf, ch, NULL, NULL, TO_ROOM );
   }


    do_look( ch, "auto" );


    /* She's gone from suck, to suck, hey wait.. Swift */
   if ( !IS_NPC(ch) )
   {
    pRoomIndex = ch->in_room;
    for ( obj = pRoomIndex->contents; obj; obj = obj->next_content )
    {
	if ( obj->deleted )
	    continue;

     if ( IS_NPC( ch ) )
     {
	if ( obj->item_type == ITEM_PORTAL
        && ( location = get_room_index( obj->value[0] ) )
        && obj->value[1] > get_curr_str(ch) )
       {
        act( AT_WHITE, "$n is sucked into the $p.", ch, obj, NULL, TO_ROOM ); 
        act( AT_WHITE, "You are sucked into the $p.", ch, obj, NULL, TO_CHAR ); 
          char_from_room( ch );
          char_to_room( ch, location );
        act( AT_WHITE, "$n appears suddenly.", ch, NULL, NULL, TO_ROOM ); 
        return;
       }
      }
      else
      {
	if ( obj->item_type == ITEM_PORTAL
        && ( location = get_room_index( obj->value[0] ) )
        && obj->value[1] > get_curr_str(ch) && ch->race != RACE_CHRONOSAPIEN )
       {
        act( AT_WHITE, "$n is sucked into the $p.", ch, obj, NULL, TO_ROOM ); 
        act( AT_WHITE, "You are sucked into the $p.", ch, obj, NULL, TO_CHAR ); 
          char_from_room( ch );
          char_to_room( ch, location );
        act( AT_WHITE, "$n appears suddenly.", ch, NULL, NULL, TO_ROOM ); 
        return;
       }
      }
    }
   }

    if ( Fall )
        act( AT_WHITE, "$n falls down from above.", ch, NULL, NULL, TO_ROOM ); 

    if ( to_room->exit[rev_dir[door]] &&
	 to_room->exit[rev_dir[door]]->to_room == in_room )
      eprog_exit_trigger( to_room->exit[rev_dir[door]], ch->in_room, ch );
    else
      rprog_enter_trigger( ch->in_room, ch );

    for ( fch = in_room->people; fch; fch = fch_next )
    {
        fch_next = fch->next_in_room;

        if ( fch->deleted )
	    continue;
      
	if ( fch->master == ch && fch->position == POS_STANDING )
	{
	    act(AT_GREY, "You follow $N.", fch, NULL, ch, TO_CHAR );
	    move_char( fch, door, FALSE, FALSE );
	}
    }
    

      /* Is the room under the fall room full of water? 
       * Then momentum keeps going. -Flux 
       */
       if ( Fall )
       {
        if ( rsector_check( ch->in_room ) == SECT_UNDERWATER ||
             rsector_check( ch->in_room ) == SECT_WATER_SURFACE )
        {
         if ( rsector_check( ch->in_room ) == SECT_WATER_SURFACE )
         {
          act( AT_WHITE, "$n plunges into the water.\n\r", ch,
           NULL, NULL, TO_ROOM );
          act( AT_RED, "You plunge into the water!\n\r",
           ch, NULL, NULL, TO_CHAR );
         }
         else
         {
          act( AT_WHITE, "$n plunges deeper into the water.\n\r", ch,
           NULL, NULL, TO_ROOM );
          act( AT_RED, "You plunge deeper into the water!\n\r",
           ch, NULL, NULL, TO_CHAR );
         }

         move_char( ch, 5, TRUE, FALSE ); 
        }
       }

    if ( IS_SET( to_room->room_flags, ROOM_NOFLOOR ) &&
     !is_flying(ch) && ( ( pexit = to_room->exit[5] ) != NULL )
       && ( ( to_room = pexit->to_room ) != NULL ) )
    {
     act( AT_WHITE, "$n falls down to the room below.\n\r", ch,
      NULL, NULL, TO_ROOM );
     act( AT_RED, "You fall through where the floor should have been!\n\r",
      ch, NULL, NULL, TO_CHAR );

     move_char( ch, 5, TRUE, FALSE );
     /* Water now breaks your fall -Flux */

     if ( rsector_check( ch->in_room ) != SECT_UNDERWATER )
      damage( ch, ch, 5, gsn_wrack, DAM_INTERNAL, TRUE );
    } 


    /* Is this a trapped room? Swift.*/
    if ( IS_SET( to_room->room_flags, ROOM_TRAPPED ))
    {
     if ( number_percent( ) > 50 )
     {
	  sprintf( buf, "The booby trap is tripped, and the room seems to explode!\n\r" );
	  send_to_char( AT_WHITE, buf, ch );

          /* Death and carnage! Swift. */
	    for ( bomb = to_room->trap_bomb; bomb; bomb = bomb->next_content )
	    {

             if (bomb->deleted)
              continue;

             if ( bomb->item_type == ITEM_BOMB )            
             {
	      bomb_explode( bomb, to_room );
              break;
             }
            }

                  /* Room isn't trapped anymore, Swift.*/
              REMOVE_BIT( to_room->room_flags, ROOM_TRAPPED );
     else
     {
	  sprintf( buf, "You step over a thin wire as you enter the room.\n\r" );
	  send_to_char( AT_WHITE, buf, ch );
     }
    }      
    else
    {
	  sprintf( buf, "You trip over a thin wire as you enter the room.\n\r" );
	  send_to_char( AT_WHITE, buf, ch );

     /* Room isn't trapped anymore, Swift.*/
     REMOVE_BIT( to_room->room_flags, ROOM_TRAPPED );
    }
   }     

   /* plasma and disease spreading, and the clap, and insanity -Flux */
    if ( IS_AFFECTED( ch, AFF_INSANE ) && number_percent() / 2 > get_curr_int( ch ) )
    {
     switch( dice( 1, 6 ) )
     {
      case 1:
       interpret( ch, "clap" ); break;
      case 2:
       interpret( ch, "smile" ); break;
      case 3:
       interpret( ch, "cry" ); break;
      case 4:
       interpret( ch, "laugh" ); break;
      case 5:
       interpret( ch, "muhaha" ); break;
      case 6:
       interpret( ch, "nanoo" ); break;
     }
    }

    if ( IS_AFFECTED2( ch, AFF_CLAP ) )
    {
     for ( fch = to_room->people; fch; fch = fch->next_in_room )
     {
        if ( fch->deleted )
	    continue;
     
        if ( fch == ch )
        continue;

       interpret( fch, "clap" );
     }
    }  

   if ( IS_AFFECTED2( ch, AFF_PLASMA ) )
   {
     for( fch = to_room->people; fch; fch = fch_next)
     {
	fch_next = fch->next_in_room;
        if ( fch == ch )
         continue;
	if(fch->deleted)
	 continue;
        if ( IS_AFFECTED2( fch, AFF_PLASMA ) )
         continue;      

      if (number_percent( ) > 85)
      {
	af.type      = gsn_plasma;
        af.level     = 1 /*skill check here and below*/;
	af.duration  = 1;
	af.location  = APPLY_NONE;
	af.modifier  = 0;
	af.bitvector = AFF_PLASMA;
	affect_to_char2( fch, &af );
       send_to_char( AT_WHITE, "A sticky, hot film forms on your skin!\n\r", fch );
      }
     }
   }

   if ( IS_AFFECTED2( ch, AFF_DISEASED ) )
   {
     for( fch = to_room->people; fch; fch = fch_next)
     {
	      fch_next = fch->next_in_room;
              if ( fch == ch )
               continue;
	      if(fch->deleted)
	       continue;
        if ( IS_AFFECTED2( fch, AFF_DISEASED ) )
         continue;      

      if (number_percent( ) > 85)
      {
	af.type      = gsn_plague;
        af.level     = 1 /*skill check here and below*/;
	af.duration  = 1;
	af.location  = APPLY_NONE;
	af.modifier  = 0;
	af.bitvector = AFF_DISEASED;
	affect_to_char2( fch, &af );
       send_to_char( AT_WHITE, "Plague sores erupt on your skin!\n\r", fch );
      }
     }
   }

    /* Kender's bump skill its automatic, obviously ;), Swift.*/
if ( !IS_NPC(ch))
{
 if ( ch->race == RACE_KENDER )
 {
  if ( number_percent( ) <= get_curr_dex( ch )  )
  {
	MONEY_DATA amount;
     for( fch = to_room->people; fch; fch = fch_next)
     {
	      fch_next = fch->next_in_room;
              if ( fch == ch )
               continue;
	      if(fch->deleted)
	       continue;
        if ( fch->money.gold <= 0 && fch->money.silver <= 0 && fch->money.copper <= 0 )
        continue;

        if ( !(fch->money.gold <= 2) )
	amount.gold = number_range( 1, 2 );
        else 
        amount.gold = 0;
        if ( !(fch->money.silver <= 3) )
  	amount.silver = number_range( 1, 3 );
        else 
        amount.silver = 0;
        if ( !(fch->money.copper <= 5) )
	amount.copper = number_range( 1, 5 );	
        else 
        amount.copper = 0;

        if ( amount.gold <= 0 && amount.silver <= 0 && amount.copper <= 0 )
        continue;
        if ( amount.gold >= 3 && amount.silver >= 4 && amount.copper >= 5 )
        continue;

        if (number_percent( ) < (100 - (70 + get_curr_dex(ch))))
        {
	/*
	 * Failure.
	 */
	send_to_char(AT_RED, "Oops.\n\r", ch );
	act(AT_RED, "$n tried to steal from you.\n\r", ch, NULL, fch, TO_VICT    );
	act(AT_RED, "$n tried to steal from $N.\n\r",  ch, NULL, fch, TO_NOTVICT );
	sprintf( buf, "%s is a bloody thief!", ch->name );
	do_shout( fch, buf );
	if ( !IS_NPC( ch ) )
	{
	    if ( IS_NPC( fch ) )
	    {
		multi_hit( fch, ch, TYPE_UNDEFINED );
	    }
	}
	continue;
      }

	add_money( &ch->money, &amount);  
	sub_money( &fch->money, &amount);

        if ( !IS_NPC( fch ))
	sprintf( buf, "You bump into %s.  You got %s\n\r", fch->oname, money_string( &amount ) );
        else
	sprintf( buf, "You bump into %s.  You got %s\n\r", fch->name, money_string( &amount ) );
	send_to_char(AT_RED, buf, ch );
       }
      }
     }  
    }

   if( !IS_NPC( ch ) && !( Fall ) )
   {
    pRoomIndex = ch->in_room;
    if ( ch->pcdata->condition[COND_INSOMNIA] == 0 )
     for ( obj = pRoomIndex->contents; obj; obj = obj->next_content )
     {
 	if ( obj->deleted )
	    continue;

        if ( obj->item_type == ITEM_FURNITURE &&
	     obj->value[1] == FURNITURE_BED )
        {
         act( AT_YELLOW, "The $p looks so inviting that you decide to have a little nap.", ch, obj, NULL, TO_CHAR );
         do_sleep( ch, obj->name );
	 break;
        }
     }
    }

    if ( !IS_NPC( ch ) )
    {
      if ( !IS_SET( ch->act, PLR_WIZINVIS ) )
      {
	mprog_greet_trigger( ch );
	return;
      }
      else return;
    }
    
    mprog_entry_trigger( ch );
    mprog_greet_trigger( ch );
    return;
}

void do_north( CHAR_DATA *ch, char *argument )
{
    move_char( ch, DIR_NORTH, FALSE, FALSE );
    return;
}

void do_east( CHAR_DATA *ch, char *argument )
{
    move_char( ch, DIR_EAST, FALSE, FALSE );
    return;
}

void do_south( CHAR_DATA *ch, char *argument )
{
    move_char( ch, DIR_SOUTH, FALSE, FALSE );
    return;
}

void do_west( CHAR_DATA *ch, char *argument )
{
    move_char( ch, DIR_WEST, FALSE, FALSE );
    return;
}

void do_up( CHAR_DATA *ch, char *argument )
{
    move_char( ch, DIR_UP, FALSE, FALSE );
    return;
}

void do_down( CHAR_DATA *ch, char *argument )
{
    move_char( ch, DIR_DOWN, FALSE, FALSE );
    return;
}

int find_door( CHAR_DATA *ch, char *arg, bool pMsg )
{
    EXIT_DATA *pexit;
    int        door;

	 if ( !str_prefix( arg, "north" ) ) door = 0;
    else if ( !str_prefix( arg, "east"  ) ) door = 1;
    else if ( !str_prefix( arg, "south" ) ) door = 2;
    else if ( !str_prefix( arg, "west"  ) ) door = 3;
    else if ( !str_prefix( arg, "up"    ) ) door = 4;
    else if ( !str_prefix( arg, "down"  ) ) door = 5;
    else
    {
	for ( door = 0; door <= 5; door++ )
	{
	    if ( ( pexit = ch->in_room->exit[door] )
		&& IS_SET( pexit->exit_info, EX_ISDOOR )
		&& pexit->keyword
		&& is_name( ch, arg, pexit->keyword ) )
		return door;
	}
	if ( pMsg )
	  act(AT_GREY, "I see no $T here.", ch, NULL, arg, TO_CHAR );
	return -1;
    }

    if ( !( pexit = ch->in_room->exit[door] ) )
    {
	if ( pMsg )
	  act(AT_GREY, "I see no door $T here.", ch, NULL, arg, TO_CHAR );
	return -1;
    }

    if ( !IS_SET( pexit->exit_info, EX_ISDOOR ) )
    {
	if ( pMsg )
	  send_to_char(AT_GREY, "You can't do that.\n\r", ch );
	return -1;
    }

    return door;
}

void do_open( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    int       door;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Open what?\n\r", ch );
	return;
    }
    if ( ( obj = get_obj_here( ch, arg ) )
    && find_door( ch, arg, FALSE ) == -1 )
    {
	/* 'open object' */
	if ( obj->item_type != ITEM_CONTAINER )
	    { send_to_char(C_DEFAULT, "That's not a container.\n\r", ch ); return; }
	if ( !IS_SET( obj->value[1], CONT_CLOSED )    )
	    { send_to_char(C_DEFAULT, "It's already open.\n\r",      ch ); return; }
	if ( !IS_SET( obj->value[1], CONT_CLOSEABLE ) )
	    { send_to_char(C_DEFAULT, "You can't do that.\n\r",      ch ); return; }
	if (  IS_SET( obj->value[1], CONT_LOCKED )    )
	    { send_to_char(C_DEFAULT, "It's locked.\n\r",            ch ); return; }

	REMOVE_BIT( obj->value[1], CONT_CLOSED );
	send_to_char(C_DEFAULT, "Ok.\n\r", ch );
	act(C_DEFAULT, "$n opens $p.", ch, obj, NULL, TO_ROOM );
	oprog_open_trigger( obj, ch );
	return;
    }

    if ( ( door = find_door( ch, arg, TRUE ) ) >= 0 )
    {
	/* 'open door' */
	EXIT_DATA       *pexit;
	EXIT_DATA       *pexit_rev;
	ROOM_INDEX_DATA *to_room;
        char		 buf[MAX_STRING_LENGTH];

	pexit = ch->in_room->exit[door];
	if ( !IS_SET( pexit->exit_info, EX_CLOSED )  )
	    { send_to_char(C_DEFAULT, "It's already open.\n\r",     ch ); return; }
	if (  IS_SET( pexit->exit_info, EX_LOCKED )  )
	    { send_to_char(C_DEFAULT, "It's locked.\n\r",           ch ); return; }

	REMOVE_BIT( pexit->exit_info, EX_CLOSED );
	act(C_DEFAULT, "$n opens the $d.", ch, NULL, pexit->keyword, TO_ROOM );
        sprintf( buf, "You open the %s to the %s.", pexit->keyword,
         dir_name[door] );
	send_to_char(C_DEFAULT, buf, ch );
        send_to_char( C_DEFAULT, "\n\r", ch );
	eprog_open_trigger( pexit, ch->in_room, ch );

	/* open the other side */
	if (   ( to_room   = pexit->to_room               )
	    && ( pexit_rev = to_room->exit[rev_dir[door]] )
	    && pexit_rev->to_room == ch->in_room )
	{
	    CHAR_DATA *rch;

	    REMOVE_BIT( pexit_rev->exit_info, EX_CLOSED );
	    for ( rch = to_room->people; rch; rch = rch->next_in_room )
	    {
		if ( rch->deleted )
		    continue;
		act(C_DEFAULT, "The $d opens.", rch, NULL, pexit_rev->keyword, TO_CHAR );
	    }
	}
    }

    return;
}

void do_close( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    int       door;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Close what?\n\r", ch );
	return;
    }

    if ( ( door = find_door( ch, arg, TRUE ) ) >= 0 )
    {
	/* 'close door' */
	EXIT_DATA       *pexit;
	EXIT_DATA       *pexit_rev;
	ROOM_INDEX_DATA *to_room;

	pexit	= ch->in_room->exit[door];
	if ( IS_SET( pexit->exit_info, EX_CLOSED ) )
	{
	    send_to_char(C_DEFAULT, "It's already closed.\n\r",    ch );
	    return;
	}

	if ( IS_SET( pexit->exit_info, EX_BASHED ) )
	{
	    act(C_DEFAULT, "The $d has been bashed open and cannot be closed.",
		ch, NULL, pexit->keyword, TO_CHAR );
	    return;
	}

	SET_BIT( pexit->exit_info, EX_CLOSED );
	act(C_DEFAULT, "$n closes the $d.", ch, NULL, pexit->keyword, TO_ROOM );
	send_to_char(C_DEFAULT, "Ok.\n\r", ch );
	eprog_close_trigger( pexit, ch->in_room, ch );

	/* close the other side */
	if (   ( to_room   = pexit->to_room               )
	    && ( pexit_rev = to_room->exit[rev_dir[door]] )
	    && pexit_rev->to_room == ch->in_room )
	{
	    CHAR_DATA *rch;

	    SET_BIT( pexit_rev->exit_info, EX_CLOSED );
	    for ( rch = to_room->people; rch; rch = rch->next_in_room )
	    {
		if ( rch->deleted )
		    continue;
		act(C_DEFAULT, "The $d closes.", rch, NULL, pexit_rev->keyword, TO_CHAR );
	    }
	}
    return;
    }

    if ( ( obj = get_obj_here( ch, arg ) ) )
    {
	/* 'close object' */
	if ( obj->item_type != ITEM_CONTAINER )
	    { send_to_char(C_DEFAULT, "That's not a container.\n\r", ch ); return; }
	if (  IS_SET( obj->value[1], CONT_CLOSED )    )
	    { send_to_char(C_DEFAULT, "It's already closed.\n\r",    ch ); return; }
	if ( !IS_SET( obj->value[1], CONT_CLOSEABLE ) )
	    { send_to_char(C_DEFAULT, "You can't do that.\n\r",      ch ); return; }

	SET_BIT( obj->value[1], CONT_CLOSED );
	send_to_char(C_DEFAULT, "Ok.\n\r", ch );
	act(C_DEFAULT, "$n closes $p.", ch, obj, NULL, TO_ROOM );
	oprog_close_trigger( obj, ch );
    }
    return;
}

OBJ_DATA *has_key( CHAR_DATA *ch, int key )
{
    OBJ_DATA *obj;

    for ( obj = ch->carrying; obj; obj = obj->next_content )
    {
	if ( obj->pIndexData->vnum == key )
	    return obj;
    }

    return NULL;
}

void do_lock( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    OBJ_DATA *key;
    char      arg [ MAX_INPUT_LENGTH ];
    int       door;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Lock what?\n\r", ch );
	return;
    }
    if ( ( door = find_door( ch, arg, TRUE ) ) >= 0 )
    {
	/* 'lock door' */
	EXIT_DATA       *pexit;
	EXIT_DATA       *pexit_rev;
	ROOM_INDEX_DATA *to_room;

	pexit	= ch->in_room->exit[door];
	if ( !IS_SET( pexit->exit_info, EX_CLOSED ) )
	    { send_to_char(C_DEFAULT, "It's not closed.\n\r",        ch ); return; }
	if ( pexit->key <= 0 )
	    { send_to_char(C_DEFAULT, "It can't be locked.\n\r",     ch ); return; }
	if ( !(key = has_key( ch, pexit->key )) )
	    { send_to_char(C_DEFAULT, "You lack the key.\n\r",       ch ); return; }
	if (  IS_SET( pexit->exit_info, EX_LOCKED ) )
	    { send_to_char(C_DEFAULT, "It's already locked.\n\r",    ch ); return; }

	SET_BIT( pexit->exit_info, EX_LOCKED );
	send_to_char(C_DEFAULT, "*Click*\n\r", ch );
	act(C_DEFAULT, "$n locks the $d.", ch, NULL, pexit->keyword, TO_ROOM );
	eprog_lock_trigger( pexit, ch->in_room, ch, key );

	/* lock the other side */
	if (   ( to_room   = pexit->to_room               )
	    && ( pexit_rev = to_room->exit[rev_dir[door]] )
	    && pexit_rev->to_room == ch->in_room )
	{
	    SET_BIT( pexit_rev->exit_info, EX_LOCKED );
	}
    return;
    }

    if ( ( obj = get_obj_here( ch, arg ) ) )
    {
	/* 'lock object' */
	if ( obj->item_type != ITEM_CONTAINER )
	    { send_to_char(C_DEFAULT, "That's not a container.\n\r", ch ); return; }
	if ( !IS_SET( obj->value[1], CONT_CLOSED ) )
	    { send_to_char(C_DEFAULT, "It's not closed.\n\r",        ch ); return; }
	if ( obj->value[2] < 0 )
	    { send_to_char(C_DEFAULT, "It can't be locked.\n\r",     ch ); return; }
	if ( !(key = has_key( ch, obj->value[2] )) )
	    { send_to_char(C_DEFAULT, "You lack the key.\n\r",       ch ); return; }
	if (  IS_SET( obj->value[1], CONT_LOCKED ) )
	    { send_to_char(C_DEFAULT, "It's already locked.\n\r",    ch ); return; }

	SET_BIT( obj->value[1], CONT_LOCKED );
	send_to_char(C_DEFAULT, "*Click*\n\r", ch );
	act(C_DEFAULT, "$n locks $p.", ch, obj, NULL, TO_ROOM );
	oprog_lock_trigger( obj, ch, key );
    }
    return;
}

void do_unlock( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    OBJ_DATA *key;
    char      arg [ MAX_INPUT_LENGTH ];
    int       door;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Unlock what?\n\r", ch );
	return;
    }
    if ( ( door = find_door( ch, arg, FALSE ) ) >= 0 )
    {
	/* 'unlock door' */
	EXIT_DATA       *pexit;
	EXIT_DATA       *pexit_rev;
	ROOM_INDEX_DATA *to_room;

	pexit = ch->in_room->exit[door];
	if ( !IS_SET( pexit->exit_info, EX_CLOSED ) )
	    { send_to_char(C_DEFAULT, "It's not closed.\n\r",        ch ); return; }
	if ( pexit->key <= 0 )
	    { send_to_char(C_DEFAULT, "It can't be unlocked.\n\r",   ch ); return; }
	if ( !(key = has_key( ch, pexit->key )) )
	    { send_to_char(C_DEFAULT, "You lack the key.\n\r",       ch ); return; }
	if ( !IS_SET( pexit->exit_info, EX_LOCKED ) )
	    { send_to_char(C_DEFAULT, "It's already unlocked.\n\r",  ch ); return; }

	REMOVE_BIT( pexit->exit_info, EX_LOCKED );
	send_to_char(C_DEFAULT, "*Click*\n\r", ch );
	act(C_DEFAULT, "$n unlocks the $d.", ch, NULL, pexit->keyword, TO_ROOM );
	eprog_unlock_trigger( pexit, ch->in_room, ch, key );

	/* unlock the other side */
	if (   ( to_room   = pexit->to_room               )
	    && ( pexit_rev = to_room->exit[rev_dir[door]] )
	    && pexit_rev->to_room == ch->in_room )
	{
	    REMOVE_BIT( pexit_rev->exit_info, EX_LOCKED );
	}
    return;
    }

    if ( ( obj = get_obj_here( ch, arg ) ) )
    {
	/* 'unlock object' */
	if ( obj->item_type != ITEM_CONTAINER )
	    { send_to_char(C_DEFAULT, "That's not a container.\n\r", ch ); return; }
	if ( !IS_SET( obj->value[1], CONT_CLOSED ) )
	    { send_to_char(C_DEFAULT, "It's not closed.\n\r",        ch ); return; }
	if ( obj->value[2] < 0 )
	    { send_to_char(C_DEFAULT, "It can't be unlocked.\n\r",   ch ); return; }
	if ( !(key = has_key( ch, obj->value[2] )) )
	    { send_to_char(C_DEFAULT, "You lack the key.\n\r",       ch ); return; }
	if ( !IS_SET( obj->value[1], CONT_LOCKED ) )
	    { send_to_char(C_DEFAULT, "It's already unlocked.\n\r",  ch ); return; }

	REMOVE_BIT( obj->value[1], CONT_LOCKED );
	send_to_char(C_DEFAULT, "*Click*\n\r", ch );
	act(C_DEFAULT, "$n unlocks $p.", ch, obj, NULL, TO_ROOM );
	oprog_unlock_trigger( obj, ch, key );
    }
    return;
}

void do_pick( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *obj;
    CHAR_DATA *gch;
    char       arg [ MAX_INPUT_LENGTH ];
    int        door;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Pick what?\n\r", ch );
	return;
    }

    WAIT_STATE( ch, skill_table[gsn_pick_lock].beats );

    /* look for guards */
    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
        if ( gch->deleted )
	    continue;
	if ( IS_NPC( gch ) && IS_AWAKE( gch ) /* skill check */ )
	{
	    act(C_DEFAULT, "$N is standing too close to the lock.",
		ch, NULL, gch, TO_CHAR );
	    return;
	}
    }

    if ( !IS_NPC( ch ) && number_percent( ) > ch->pcdata->learned[gsn_pick_lock] )
    {
	send_to_char(C_DEFAULT, "You failed.\n\r", ch);
	return;
    }
    if ( ( door = find_door( ch, arg, TRUE ) ) >= 0 )
    {
	/* 'pick door' */
	EXIT_DATA       *pexit;
	EXIT_DATA       *pexit_rev;
	ROOM_INDEX_DATA *to_room;

	pexit = ch->in_room->exit[door];
	if ( !IS_SET( pexit->exit_info, EX_CLOSED )    )
	    { send_to_char(C_DEFAULT, "It's not closed.\n\r",        ch ); return; }
	if ( pexit->key < 0 )
	    { send_to_char(C_DEFAULT, "It can't be picked.\n\r",     ch ); return; }
	if ( !IS_SET( pexit->exit_info, EX_LOCKED )    )
	    { send_to_char(C_DEFAULT, "It's already unlocked.\n\r",  ch ); return; }
	if (  IS_SET( pexit->exit_info, EX_PICKPROOF ) )
	    { send_to_char(C_DEFAULT, "You failed.\n\r",             ch ); return; }

	REMOVE_BIT( pexit->exit_info, EX_LOCKED );
	send_to_char(C_DEFAULT, "*Click*\n\r", ch );
	if ( !IS_AFFECTED(ch,AFF_HIDE) )
          act(C_DEFAULT, "$n picks the $d.", ch, NULL, pexit->keyword, TO_ROOM );
	eprog_pick_trigger( pexit, ch->in_room, ch );

	/* pick the other side */
	if (   ( to_room   = pexit->to_room               )
	    && ( pexit_rev = to_room->exit[rev_dir[door]] )
	    && pexit_rev->to_room == ch->in_room )
	{
	    REMOVE_BIT( pexit_rev->exit_info, EX_LOCKED );
	}
    return;
    }

    if ( ( obj = get_obj_here( ch, arg ) ) )
    {
	/* 'pick object' */
	if ( obj->item_type != ITEM_CONTAINER )
	    { send_to_char(C_DEFAULT, "That's not a container.\n\r", ch ); return; }
	if ( !IS_SET( obj->value[1], CONT_CLOSED )    )
	    { send_to_char(C_DEFAULT, "It's not closed.\n\r",        ch ); return; }
	if ( obj->value[2] < 0 )
	    { send_to_char(C_DEFAULT, "It can't be unlocked.\n\r",   ch ); return; }
	if ( !IS_SET( obj->value[1], CONT_LOCKED )    )
	    { send_to_char(C_DEFAULT, "It's already unlocked.\n\r",  ch ); return; }
	if (  IS_SET( obj->value[1], CONT_PICKPROOF ) )
	    { send_to_char(C_DEFAULT, "You failed.\n\r",             ch ); return; }

	REMOVE_BIT( obj->value[1], CONT_LOCKED );
	send_to_char(C_DEFAULT, "*Click*\n\r", ch );
        if ( !IS_AFFECTED(ch,AFF_HIDE) )
          act(C_DEFAULT, "$n picks $p.", ch, obj, NULL, TO_ROOM );
	oprog_pick_trigger( obj, ch );
    }

    update_skpell( ch, gsn_pick_lock );
    return;
}

void do_stand( CHAR_DATA *ch, char *argument )
{
    switch ( ch->position )
    {
    case POS_SLEEPING:
	if ( IS_AFFECTED( ch, AFF_SLEEP ) )
	    { send_to_char(AT_CYAN, "You can't wake up!\n\r", ch ); return; }

	send_to_char(AT_CYAN, "You wake and stand up.\n\r", ch );
        if ( !IS_AFFECTED(ch,AFF_HIDE) )
	  act(AT_CYAN, "$n wakes and stands up.", ch, NULL, NULL, TO_ROOM );
        ch->resting_on = NULL;
	ch->position = POS_STANDING;
	rprog_wake_trigger( ch->in_room, ch );
	break;

    case POS_RESTING:
	send_to_char(AT_CYAN, "You stand up.\n\r", ch );
	if ( !IS_AFFECTED(ch,AFF_HIDE) )
          act(AT_CYAN, "$n stands up.", ch, NULL, NULL, TO_ROOM );
        ch->resting_on = NULL;
	ch->position = POS_STANDING;
	rprog_wake_trigger( ch->in_room, ch );
	break;

    case POS_FIGHTING:
	send_to_char(AT_CYAN, "You are already fighting!\n\r",  ch );
	break;

    case POS_STANDING:
	send_to_char(AT_CYAN, "You are already standing.\n\r",  ch );
	break;
    }

    return;
}

void do_rest( CHAR_DATA *ch, char *argument )
{
    char       arg [ MAX_INPUT_LENGTH ];
    CHAR_DATA  *rch;
    OBJ_DATA   *seat;
    bool       target = FALSE;
    bool       sitting = FALSE;
    one_argument( argument, arg );

    seat = get_obj_here( ch, arg );

   if ( seat )
   {
    for ( rch = ch->in_room->people; rch; rch = rch->next_in_room )
    {
	if ( rch->deleted )
	    continue;

	if ( rch->resting_on == seat )
         sitting = TRUE;
    }

    if ( seat->item_type == ITEM_FURNITURE )
    {
     if ( ( seat->value[1] == FURNITURE_CHAIR && sitting == FALSE ) ||
          seat->value[1] == FURNITURE_SOFA )
       target = TRUE;
      else if ( seat->value[1] == FURNITURE_CHAIR && sitting == TRUE )
       send_to_char( AT_YELLOW, "Someone is already sitting on that chair.\n\r", ch );
    }
   }
    switch ( ch->position )
    {
    case POS_SLEEPING:

         if ( target == FALSE )
         {
       	  act(AT_CYAN, "You wake up and start resting.", ch, NULL, NULL, TO_CHAR );
          ch->resting_on = NULL;
         }
         else
         {
          act(AT_CYAN, "You wake up and rest on $p.", ch, seat, NULL, TO_CHAR );
          ch->resting_on = seat;
          oprog_use_trigger( seat, ch, ch );
         }        

  	if ( !IS_AFFECTED(ch,AFF_HIDE) )
        {
         if ( target == FALSE )
       	  act(AT_CYAN, "$n wakes up and rests.", ch, NULL, NULL, TO_ROOM );
         else
          act(AT_CYAN, "$n wakes up and rests on $p.", ch, seat, NULL, TO_ROOM );
        }
	ch->position = POS_RESTING;

	rprog_rest_trigger( ch->in_room, ch );
	break;

    case POS_RESTING:
	send_to_char(AT_CYAN, "You are already resting.\n\r",   ch );
	break;

    case POS_FIGHTING:
	send_to_char(AT_CYAN, "Not while you're fighting!\n\r", ch );
	break;

    case POS_STANDING:

         if ( target == FALSE )
         {
       	  act(AT_CYAN, "You rest.", ch, NULL, NULL, TO_CHAR );
          ch->resting_on = NULL;
         }
         else
         {
          act(AT_CYAN, "You rest on $p.", ch, seat, NULL, TO_CHAR );
          ch->resting_on = seat;
          oprog_use_trigger( seat, ch, ch );
         }        
        
  	if ( !IS_AFFECTED(ch,AFF_HIDE) )
        {
         if ( target == FALSE )
       	  act(AT_CYAN, "$n rests.", ch, NULL, NULL, TO_ROOM );
         else
          act(AT_CYAN, "$n rests on $p.", ch, seat, NULL, TO_ROOM );
        }
	rprog_rest_trigger( ch->in_room, ch );
	ch->position = POS_RESTING;
	break;
    }

    return;
}

void do_sleep( CHAR_DATA *ch, char *argument )
{
    char       arg [ MAX_INPUT_LENGTH ];
    OBJ_DATA   *seat;
    CHAR_DATA  *rch;
    bool       target = FALSE;
    bool       sitting = FALSE;
    one_argument( argument, arg );

    seat = get_obj_here( ch, arg );


   if ( seat )
   {
    for ( rch = ch->in_room->people; rch; rch = rch->next_in_room )
    {
	if ( rch->deleted )
	    continue;

	if ( rch->resting_on == seat )
         sitting = TRUE;
    }

    if ( seat->item_type == ITEM_FURNITURE )
    {
     if ( seat->value[1] == FURNITURE_BED && sitting == FALSE )
       target = TRUE;
      else if ( seat->value[1] == FURNITURE_BED && sitting == TRUE )
       send_to_char( AT_YELLOW, "Someone is already sleeping in that bed.\n\r", ch );
    }
   }

    switch ( ch->position )
    {
    case POS_SLEEPING:
	send_to_char(AT_CYAN, "You are already sleeping.\n\r",  ch );
	break;

    case POS_RESTING:
    case POS_STANDING: 
         if ( target == FALSE )
         {
       	  act(AT_CYAN, "You sleep.", ch, NULL, NULL, TO_CHAR );
          ch->resting_on = NULL;
         }
         else
         {
          act(AT_CYAN, "You sleep on $p.", ch, seat, NULL, TO_CHAR );
          ch->resting_on = seat;
          oprog_use_trigger( seat, ch, ch );
         }        
        
  	if ( !IS_AFFECTED(ch,AFF_HIDE) )
        {
         if ( target == FALSE )
       	  act(AT_CYAN, "$n sleeps.", ch, NULL, NULL, TO_ROOM );
         else
          act(AT_CYAN, "$n sleeps on $p.", ch, seat, NULL, TO_ROOM );
        }
	rprog_sleep_trigger( ch->in_room, ch );
	ch->position = POS_SLEEPING;
       if ( !IS_NPC( ch ) && number_percent() <= 15 )
        if ( ch->pcdata->condition[COND_INSOMNIA] == 0 )
        {
       	 act(AT_CYAN, "You fall into a deep sleep.", ch, NULL, NULL, TO_CHAR );
       	 act(AT_CYAN, "$n falls into a deep sleep and vanishes!.", ch, NULL, NULL, TO_ROOM );
         char_from_room( ch );
         char_to_room( ch, get_room_index( 1200 ) );
        } 
	break;

    case POS_FIGHTING:
	send_to_char(AT_CYAN, "Not while you're fighting!\n\r", ch );
	break;
    }

    return;
}



void do_wake( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );
    if ( arg[0] == '\0' )
	{ do_stand( ch, argument ); return; }

    if ( !IS_AWAKE( ch ) )
	{ send_to_char(AT_CYAN, "You are asleep yourself!\n\r",       ch ); return; }

    if ( !( victim = get_char_room( ch, arg ) ) )
	{ send_to_char(AT_CYAN, "They aren't here.\n\r",              ch ); return; }

    if ( IS_AWAKE( victim ) )
	{ act(AT_CYAN, "$N is already awake.", ch, NULL, victim, TO_CHAR ); return; }

    if ( IS_AFFECTED( victim, AFF_SLEEP ) )
	{ act(AT_CYAN, "You can't wake $M!",   ch, NULL, victim, TO_CHAR ); return; }

    victim->position = POS_STANDING;
    act(AT_CYAN, "You wake $M.",  ch, NULL, victim, TO_CHAR );
    act(AT_CYAN, "$n wakes you.", ch, NULL, victim, TO_VICT );
    rprog_wake_trigger( victim->in_room, victim );
    return;
}


void do_sneak( CHAR_DATA *ch, char *argument )
{
    AFFECT_DATA af;

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_sneak ) )
    {
     typo_message( ch );
	return;
    }

    send_to_char(AT_LBLUE, "You attempt to move silently.\n\r", ch );
    affect_strip( ch, gsn_sneak );

    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_sneak] )
    {
	af.type      = gsn_sneak;
        af.level     = 1 /* skill check */;
	af.duration  = 1 /* and here */;
	af.location  = APPLY_NONE;
	af.modifier  = 0;
	af.bitvector = AFF_SNEAK;
	affect_to_char( ch, &af );
    }

    update_skpell( ch, gsn_sneak );
    return;
}



void do_hide( CHAR_DATA *ch, char *argument )
{
    AFFECT_DATA af;
    
    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_hide ) )
    {
     typo_message( ch );
	return;
    }

    send_to_char(AT_LBLUE, "You attempt to hide.\n\r", ch );

    if ( IS_AFFECTED( ch, AFF_HIDE ) )
       affect_strip(ch, gsn_hide);
       
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_hide] )
    {
	af.type      = gsn_hide;
        af.level     = 1 /* skill check */;
	af.duration  = 1 /* and here */;
	af.location  = APPLY_NONE;
	af.modifier  = 0;
	af.bitvector = AFF_HIDE;
	affect_to_char( ch, &af );
    }

    update_skpell( ch, gsn_hide );
    return;
}



/*
 * Contributed by Alander.
 */
void do_visible( CHAR_DATA *ch, char *argument )
{
    affect_strip ( ch, gsn_invis			);
    affect_strip ( ch, gsn_mass_invis			);
    affect_strip ( ch, gsn_sneak			);
    affect_strip ( ch, gsn_shadow                       );
    affect_strip ( ch, gsn_hide                         );
    affect_strip ( ch, gsn_chameleon                    );
    affect_strip ( ch, skill_lookup("phase shift")      );
    affect_strip ( ch, skill_lookup("mist form")        );
    REMOVE_BIT   ( ch->affected_by, AFF_HIDE            );
    REMOVE_BIT   ( ch->affected_by, AFF_PHASED          );
    REMOVE_BIT   ( ch->affected_by, AFF_INVISIBLE	);
    REMOVE_BIT   ( ch->affected_by, AFF_SNEAK		);
    send_to_char(AT_WHITE, "Ok.\n\r", ch );
    return;
}


void do_recall( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA       *victim;
    CHAR_DATA       *pet;
    ROOM_INDEX_DATA *location;
    char             buf [ MAX_STRING_LENGTH ];
    int              place;
    char             name[ MAX_STRING_LENGTH ];
    CLAN_DATA       *pClan;

    if (!(pClan=get_clan_index(ch->clan)))
    {
     ch->clan = 0;
     pClan=get_clan_index(ch->clan);
    }

    sprintf( name, "%s", pClan->warden );
    act(C_DEFAULT, "$n prays for transportation!", ch, NULL, NULL, TO_ROOM );

    if ( ( ch->clan != 0 ) && ( ch->combat_timer < 1 ) && !IS_ARENA(ch) )
     place = pClan->recall;
    else
     place = ch->in_room->area->recall;

    if ( !( location = get_room_index( place ) ) )
    {
	send_to_char(C_DEFAULT, "You are completely lost.\n\r", ch );
	return;
    }

    if ( place == ROOM_HUMAN_TEMPLE ||
         place == ROOM_ELF_TEMPLE )
    {
     if ( ch->race == RACE_HUMAN )
     {
      if ( !( location = get_room_index( ROOM_HUMAN_TEMPLE ) ) )
      {
       send_to_char(C_DEFAULT, "You are completely lost.\n\r", ch );
       return;
      }
     }
     else
     {
      if ( !( location = get_room_index( ROOM_ELF_TEMPLE ) ) )
      {
       send_to_char(C_DEFAULT, "You are completely lost.\n\r", ch );
       return;
      }
     }
    }

    if ( ch->in_room == location )
     return;

    if ( IS_SET( ch->in_room->room_flags, ROOM_NO_RECALL )
	|| IS_AFFECTED( ch, AFF_CURSE ) )
    {
     act(C_DEFAULT, "$T has forsaken you.", ch, NULL, name, TO_CHAR);
     return;
    }

    if ( ( victim = ch->fighting ) )
    {
	int lose;

	if ( number_bits( 1 ) == 0 )
	{
	    WAIT_STATE( ch, 4 );
	    if ( !IS_ARENA(ch) )
	    {
	      lose = ( ch->desc ) ? 50 : 100;
	      gain_exp( ch, 0 - lose );
	      sprintf( buf, "You failed!  You lose %d exps.\n\r", lose );
	    }
	    else
	      strcpy(buf, "You failed!");
	    send_to_char(C_DEFAULT, buf, ch );
	    return;
	}

	if ( !IS_ARENA(ch) )
	{
	  lose = ( ch->desc ) ? 100 : 200;
	  gain_exp( ch, 0 - lose );
	  sprintf( buf, "You recall from combat!  You lose %d exps.\n\r", lose );
	}
	else
	  strcpy(buf, "You recall from combat!");
	send_to_char(C_DEFAULT, buf, ch );
	stop_fighting( ch, TRUE );
    }

    for ( pet = ch->in_room->people; pet; pet = pet->next_in_room )
    {
      if ( IS_NPC( pet ) )
        if ( IS_SET( pet->act, ACT_PET ) && ( pet->master == ch ) )
        {
          if ( pet->fighting )
            stop_fighting( pet, TRUE );
          break;
        }
    }
    act(C_DEFAULT, "$n disappears.", ch, NULL, NULL, TO_ROOM );
    if ( victim != NULL && IS_ARENA(ch) )
      act(AT_RED, "$n has escaped!  Hunt $m down!", ch, NULL, victim,
          TO_VICT);
    char_from_room( ch );

    char_to_room( ch, location );
    act(C_DEFAULT, "$n appears in the room.", ch, NULL, NULL, TO_ROOM );
    do_look( ch, "auto" );
    if ( pet )
    {
      act( C_DEFAULT, "$n disappears.", pet, NULL, NULL, TO_ROOM );
      char_from_room( pet );
      char_to_room( pet, location );
      act(C_DEFAULT, "$n appears in the room.", pet, NULL, NULL, TO_ROOM );
    }

    return;
}



void do_train( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *mob;
    char      arg[ MAX_INPUT_LENGTH ];
    char      arg1[ MAX_STRING_LENGTH ];
    int        amt = 1;
    int        cnt = 1;
    char      *pOutput;
    char       buf [ MAX_STRING_LENGTH ];
    int       *pAbility;
    int        cost;
    int        bone_flag = 0; /*Added for training of hp ma mv */
    int        dispcount = 0;


    if ( IS_NPC( ch ) )
	return;

    argument = one_argument( argument, arg );  
    argument = one_argument( argument, arg1 );
  
    /*
     * Check for trainer.
     */
    for ( mob = ch->in_room->people; mob; mob = mob->next_in_room )
    {
	if ( IS_NPC( mob ) && IS_SET( mob->act, ACT_TRAIN ) )
	    break;
    }

    if ( !mob )
    {
	send_to_char(AT_WHITE, "You can't do that here.\n\r", ch );
	return;
    }

    if ( arg[0] == '\0' )
    {
	sprintf( buf, "You have %d practice sessions.\n\r", ch->practice );
	send_to_char(AT_CYAN, buf, ch );
	argument = "foo";
    }
    if ( arg1[0] == '\0' )
       amt = 1;
    else
       amt = atoi( arg1 );

    cost = 5;

   /* languages -Flux. */
    else if ( !str_cmp( arg, "elf" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_ELF];
        pOutput     = "knowledge of the elven language";
    }

    else if ( !str_cmp( arg, "dwarf" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_DWARF];
        pOutput     = "knowledge of the dwarven language";
    }

    else if ( !str_cmp( arg, "quicksilver" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_QUICKSILVER];
        pOutput     = "knowledge of the quicksilver language";
    }

    else if ( !str_cmp( arg, "maudlin" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_MAUDLIN];
        pOutput     = "knowledge of the maudlin language";
    }

    else if ( !str_cmp( arg, "pixie" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_PIXIE];
        pOutput     = "knowledge of the pixie language";
    }

    else if ( !str_cmp( arg, "felixi" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_FELIXI];
        pOutput     = "knowledge of the felixi language";
    }

    else if ( !str_cmp( arg, "draconi" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_DRACONI];
        pOutput     = "knowledge of the draconi language";
    }

    else if ( !str_cmp( arg, "gremlin" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_GREMLIN];
        pOutput     = "knowledge of the gremlin language";
    }

    else if ( !str_cmp( arg, "centaur" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_CENTAUR];
        pOutput     = "knowledge of the centaur language";
    }

    else if ( !str_cmp( arg, "kender" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_KENDER];
        pOutput     = "knowledge of the kender language";
    }

    else if ( !str_cmp( arg, "minotaur" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_MINOTAUR];
        pOutput     = "knowledge of the minotaur language";
    }

    else if ( !str_cmp( arg, "drow" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_DROW];
        pOutput     = "knowledge of the drow language";
    }

    else if ( !str_cmp( arg, "aquinis" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_AQUINIS];
        pOutput     = "knowledge of the aquinis language";
    }

    else if ( !str_cmp( arg, "troll" ) )
    {
 	    cost    = 1;
        bone_flag   = 3;
        pAbility    = &ch->pcdata->language[LANGUAGE_TROLL];
        pOutput     = "knowledge of the troll language";
    }

    else
    {
	strcpy( buf, "You can train:\n\r" );
      strcat( buf, "------------------------------\n\r" );

      strcat( buf, "&cLanguages:         " );

        if ( ch->pcdata->language[LANGUAGE_ELF] < 50 )
        {
	   strcat( buf, " &Gelf" );
         dispcount += 1;
        }

       if ( dispcount == 6 )
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_DWARF] < 50 )
        {
	   strcat( buf, " &Gdwarf" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_QUICKSILVER] < 50 )
        {
	   strcat( buf, " &Gquicksilver" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_MAUDLIN] < 50 )
        {
	   strcat( buf, " &Gmaudlin" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_PIXIE] < 50 )
        {
	   strcat( buf, " &Gpixie" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_FELIXI] < 50 )
        {
	   strcat( buf, " &Gfelixi" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_DRACONI] < 50 )
        {
	   strcat( buf, " &Gdraconi" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_GREMLIN] < 50 )
        {
	   strcat( buf, " &Ggremlin" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_CENTAUR] < 50 )
        {
	   strcat( buf, " &Gcentaur" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_KENDER] < 50 )
        {
	   strcat( buf, " &Gkender" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_MINOTAUR] < 50 )
        {
	   strcat( buf, " &Gminotaur" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_DROW] < 50 )
        {
	   strcat( buf, " &Gdrow" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_AQUINIS] < 50 )
        {
	   strcat( buf, " &Gaquinis" );
         dispcount += 1;
        }

       if ( dispcount == 5)
       {
        dispcount = 0;
        strcat( buf, "\n\r                   " );
       }

        if ( ch->pcdata->language[LANGUAGE_TROLL] < 50 )
        {
	   strcat( buf, " &Gtroll" );
         dispcount += 1;
        }

	if ( buf[strlen( buf )-1] != ':' )
	{
	    strcat( buf, "\n\r" );
	    send_to_char(AT_CYAN, buf, ch );
	}

	return;
    }

    if ( *pAbility >= 50 && bone_flag == 3 )
    {
	act(AT_CYAN, "I can not train you in that language any further.", ch, NULL, pOutput, TO_CHAR );
	return;
    }

    if ( ( cost*amt ) > ch->practice )
    {
	send_to_char(AT_CYAN, "You don't have enough practices.\n\r", ch );
	return;
    }

    ch->practice        	-= cost*amt;

    for( cnt = 1; cnt <= amt; cnt++ )
    {
        if ( bone_flag == 0 )
            *pAbility		+= 1;
        else if ( bone_flag == 3 )
            *pAbility		    += dice( 5, 10 );
    }

    act(AT_CYAN, "Your $T increases!", ch, NULL, pOutput, TO_CHAR );
    act(AT_CYAN, "$n's $T increases!", ch, NULL, pOutput, TO_ROOM );

    return;
}

void do_enter ( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *fch;
    CHAR_DATA *fch_next;
    ROOM_INDEX_DATA *in_room;
    OBJ_DATA *obj;
    char      arg1 [ MAX_INPUT_LENGTH ];
    int       destination;
    ROOM_INDEX_DATA *location;
 
    argument = one_argument( argument, arg1 );

    if ( arg1[0] == '\0' )
    {
	send_to_char(AT_WHITE, "Enter what?\n\r", ch );
	return;
    }
    if ( !( obj = get_obj_list( ch, arg1, ch->in_room->contents ) ) )
      {
       act(AT_WHITE, "You cannot enter that", ch, NULL, NULL, TO_CHAR );
       return;
      }
    if ( obj->item_type != ITEM_PORTAL )
      {
       send_to_char(AT_WHITE, "There is nothing to enter here.\n\r", ch );
       return;
      }
    in_room = ch->in_room;
    destination = obj->value[0];
    if ( !( location = get_room_index( destination ) ) )
    {
	act(AT_BLUE, "You try to enter $p but can't.", ch, obj, NULL, TO_CHAR );
	return;
    }
    act( AT_BLUE, "You step into the $p.",ch,obj,NULL, TO_CHAR );
    act(AT_BLUE, "$n steps into the $p and is gone.", ch, obj, NULL, TO_ROOM );
    rprog_exit_trigger( ch->in_room, ch );
    char_from_room( ch );
    char_to_room( ch, location );
    act(AT_BLUE, "$n steps out of the $p before you.", ch, obj, NULL, TO_ROOM);
    do_look( ch, "auto" );
    for ( fch = in_room->people; fch; fch = fch_next )
    {
        fch_next = fch->next_in_room;

        if ( fch->deleted )
	    continue;
      
	if ( fch->master == ch && fch->position == POS_STANDING )
	{
	    act(AT_WHITE, "You follow $N through $p.", fch, obj, ch, TO_CHAR );
	    do_enter( fch, arg1 );
	}
    }

    if ( obj->timer > -1 )
    {
     act( AT_YELLOW, "The portal shimmers and closes around its entrants.",
      NULL, obj, NULL, TO_ROOM );
     extract_obj( obj );
    }

    rprog_enter_trigger( ch->in_room, ch );
  return;
}

/*
 * Bash code by Thelonius for EnvyMud (originally bash_door)
 * Damage modified using Morpheus's code
 */
/* I changed this into a command based on str alone -Flux. */
/* And once again I've changed it to accomidate my new door barricade
   skills and spells. Basically it asks for an argument, body,
   weapon or magic. It utilizes whatever is chosen to nail the barricade.
   Magic only works on magical barricades, however, as its "dispel"
   type magic, not aggressive type magic. -Flux */
void do_bash ( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *gch;
    char       arg [ MAX_INPUT_LENGTH ];
    char       arg2[ MAX_INPUT_LENGTH ];
    int        door;
    OBJ_DATA   *weapon;
    bool       weaponcheck = FALSE;
    bool       bodycheck = FALSE;
    bool       magiccheck = FALSE;


    argument = one_argument( argument, arg );
    one_argument( argument, arg2 );

    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Bash what?\n\r", ch );
	return;
    }

    if ( arg2[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "What do you intend to bash with?\n"
                                "(Body, weapon or magic)\n\r", ch );
	return;
    }

    weapon = get_eq_char( ch, WEAR_WIELD );

    if ( !str_cmp( arg2, "weapon" ) )
    {
     if ( !(weapon) )
     {
      send_to_char( C_DEFAULT, "You aren't wielding a weapon in your main hand.\n\r", ch );
      return;
     }
     weaponcheck = TRUE;
    }
    else
    if ( !str_cmp( arg2, "body" ) )
     bodycheck = TRUE;
    else
    if ( !str_cmp( arg2, "magic" ) )
    {
     if ( ch->mana < 20 )
     {
      send_to_char( C_DEFAULT, "You don't have enough mana for this.\n\r", ch );
      return;
     }
     magiccheck = TRUE;
    }
    else
    {
     send_to_char(C_DEFAULT, "What do you intend to bash with?\n"
                                "(Body, weapon or magic)\n\r", ch );
     return;
    }

    if ( (door = flag_value( direction_flags, arg )) != -99 )
    {
	ROOM_INDEX_DATA 	*to_room;
	EXIT_DATA       	*pexit;
	EXIT_AFFECT_DATA	*exitaf;
	EXIT_DATA       	*pexit_rev;
	int              	chance;

    
    if (  !(pexit = ch->in_room->exit[door]) )
    {
     send_to_char( C_DEFAULT, "There is no exit in that direction.\n\r", ch );
     return;
    }

      if ( (exitaf = get_exit_affect( pexit, EXIT_FORCEFIELD )) )
      {
       if ( bodycheck )
       {
	act(C_DEFAULT, "SIZZLE!!!  You bash against the forcefield, but it throws you back, burning you with energy.",
	 ch, NULL, NULL, TO_CHAR );
	act(C_DEFAULT, "SIZZLE!!!  $n bashes against the forcefield, but it's energy throws $m back.",
	 ch, NULL, NULL, TO_ROOM );
        damage( ch, ch, 25, DAMNOUN_HEAT, DAM_HEAT, TRUE );
        return;
       }

       if ( weaponcheck )
       {
        if ( !IS_SET( weapon->extra_flags, ITEM_MAGIC ) )
        {
	act(AT_BLUE, "BZZZAP!!! Your $p bounces off the forcefield.",
	 ch, weapon, NULL, TO_CHAR );
	act(AT_BLUE, "BZZZAP!!!  $n's $p bounces off the forcefield.",
	 ch, weapon, NULL, TO_ROOM );
         return;
        }

	act(AT_BLUE, "Your $p slices right through the forcefield.",
	 ch, weapon, NULL, TO_CHAR );
	act(AT_BLUE, "$n's $p slices right through the forcefield.",
	 ch, weapon, NULL, TO_ROOM );

	exit_affect_remove( pexit, exitaf );

	if (   ( to_room   = pexit->to_room               )
	 && ( pexit_rev = to_room->exit[rev_dir[door]] )
	 && pexit_rev->to_room == ch->in_room        )
         if ( (exitaf = get_exit_affect( pexit_rev, EXIT_FORCEFIELD )) )
	  exit_affect_remove( pexit_rev, exitaf );
         return;
       }

       if ( magiccheck )
       {
	act(C_DEFAULT, "You release a burst of energy at the forcefield.",
	 ch, NULL, NULL, TO_CHAR );
	act(C_DEFAULT, "$n launches a spike of energy at the forcefield.",
	 ch, NULL, NULL, TO_ROOM );
        ch->mana -= 10;
	exit_affect_remove( pexit, exitaf );

	if (   ( to_room   = pexit->to_room               )
	 && ( pexit_rev = to_room->exit[rev_dir[door]] )
	 && pexit_rev->to_room == ch->in_room        )
         if ( (exitaf = get_exit_affect( pexit_rev, EXIT_FORCEFIELD )) )
	  exit_affect_remove( pexit_rev, exitaf );

         return;
       }
       return;
      }

	if ( !IS_SET( pexit->exit_info, EX_ISDOOR ) )
	{
	    send_to_char(C_DEFAULT, "That isn't a door.\n\r", ch );
	    return;
	}

	if ( !IS_SET( pexit->exit_info, EX_CLOSED ) )
	{
	    send_to_char(C_DEFAULT, "Calm down. It is already open.\n\r", ch );
	    return;
	}

        if ( bodycheck )
	 chance = number_range( get_curr_str( ch ), get_curr_str( ch ) * 4 );
        else
        if ( weaponcheck )
	 chance = number_range( weapon->value[1], weapon->value[2] );
        else
	 chance = number_range( get_curr_str( ch ), get_curr_str( ch ) * 4 );

        if ( magiccheck )
        {
	act(C_DEFAULT, "You release a burst of energy and nothing happens.",
	 ch, NULL, NULL, TO_CHAR );
	act(C_DEFAULT, "$n launches a spike of energy at the door, and nothing happens.",
	 ch, NULL, NULL, TO_ROOM );
         ch->mana -= 10;
         return;
        }         

	if ( IS_SET( pexit->exit_info, EX_LOCKED ) )
	    chance /= 2;

	if ( bodycheck )
        {
	 if ( IS_SET( pexit->exit_info, EX_BASHPROOF ) )
	 {
	    act(C_DEFAULT, "WHAAAAM!!!  You bash against the $d, but it doesn't budge.",
		ch, NULL, pexit->keyword, TO_CHAR );
	    act(C_DEFAULT, "WHAAAAM!!!  $n bashes against the $d, but it holds strong.",
		ch, NULL, pexit->keyword, TO_ROOM );
	    damage( ch, ch, ( MAX_HIT(ch) / 5 ), DAMNOUN_SMASH, DAM_BASH, TRUE );
	    return;
	 }
        }
        else if ( weaponcheck )
        {
	 if ( IS_SET( pexit->exit_info, EX_BASHPROOF ) )
	 {
	    act(C_DEFAULT, "You attack the $d with your $p, but can not break through.",
		ch, weapon, pexit->keyword, TO_CHAR );
	    act(C_DEFAULT, "$n attacks the $d with $s $p, but can not break through.",
		ch, weapon, pexit->keyword, TO_ROOM );
	    return;
	 }
        }

	if ( number_percent( ) < ( chance + 4 ) )
	{
	    /* Success */

	    REMOVE_BIT( pexit->exit_info, EX_CLOSED );
	    if ( IS_SET( pexit->exit_info, EX_LOCKED ) )
	        REMOVE_BIT( pexit->exit_info, EX_LOCKED );
	    
	    SET_BIT( pexit->exit_info, EX_BASHED );

	    act(C_DEFAULT, "Crash!  You bashed open the $d!", ch, NULL, pexit->keyword, TO_CHAR );
	    act(C_DEFAULT, "$n bashes open the $d!",          ch, NULL, pexit->keyword, TO_ROOM );

	    damage( ch, ch, ( MAX_HIT(ch) / 20 ), DAMNOUN_SMASH, DAM_BASH, TRUE );

	    /* Bash through the other side */
	    if (   ( to_room   = pexit->to_room               )
		&& ( pexit_rev = to_room->exit[rev_dir[door]] )
		&& pexit_rev->to_room == ch->in_room        )
	    {
		CHAR_DATA *rch;

		REMOVE_BIT( pexit_rev->exit_info, EX_CLOSED );
		if ( IS_SET( pexit_rev->exit_info, EX_LOCKED ) )
		    REMOVE_BIT( pexit_rev->exit_info, EX_LOCKED );

		SET_BIT( pexit_rev->exit_info, EX_BASHED );

		for ( rch = to_room->people; rch; rch = rch->next_in_room )
		{
		    if ( rch->deleted )
		        continue;
		    act(C_DEFAULT, "The $d crashes open!",
			rch, NULL, pexit_rev->keyword, TO_CHAR );
		}

	    }
	}
	else
	{
	    /* Failure */
	if ( bodycheck )
        {
	    act(C_DEFAULT, "WHAAAAM!!!  You bash against the $d, but it doesn't budge.",
		ch, NULL, pexit->keyword, TO_CHAR );
	    act(C_DEFAULT, "WHAAAAM!!!  $n bashes against the $d, but it holds strong.",
		ch, NULL, pexit->keyword, TO_ROOM );
	    damage( ch, ch, ( MAX_HIT(ch) / 5 ), DAMNOUN_SMASH, DAM_BASH, TRUE );
	    return;
        }
        else if ( weaponcheck )
        {
	    act(C_DEFAULT, "You attack the $d with your $p, but can not break through.",
		ch, weapon, pexit->keyword, TO_CHAR );
	    act(C_DEFAULT, "$n attacks the $d with $s $p, but can not break through.",
		ch, weapon, pexit->keyword, TO_ROOM );
	    return;
        }
       }
    }

    /*
     * Check for "guards"... anyone bashing a door is considered as
     * a potential aggressor, and there's a 25% chance that mobs
     * will do unto before being done unto.
     */
    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
        if ( gch->deleted )
	    continue;
	if ( IS_AWAKE( gch )
	    && !gch->fighting
	    && ( IS_NPC( gch ) && !IS_AFFECTED( gch, AFF_CHARM ) )
	    && ( /* skill check here */ )
	    && number_bits( 2 ) == 0 )
	    multi_hit( gch, ch, TYPE_UNDEFINED );
    }

    return;
}

/* XORPHOX push/drag */
void do_push(CHAR_DATA *ch, char *argument)
{
  char arg1[MAX_INPUT_LENGTH];
  char arg2[MAX_INPUT_LENGTH];
  EXIT_DATA *pexit;
  CHAR_DATA *victim;
  ROOM_INDEX_DATA *from_room;
  int door;
  char buf1[256], buf2[256], buf3[256];

  argument = one_argument(argument, arg1);
  one_argument(argument, arg2);

  if(arg1[0] == '\0')
  {
    send_to_char(AT_BLUE, "Push who what where?", ch);
    return;
  }

  if((victim = get_char_room(ch, arg1)) == NULL)
  {
    send_to_char(AT_BLUE, "They aren't here.\n\r", ch);
    return;
  }

  if ( !IS_NPC(ch) )
  if (    ( victim->pcdata->rank < RANK_STAFF )
       || (    IS_NPC( victim )                                      
            && (    ( victim->pIndexData->pShop )                  
                 || IS_SET( ch->in_room->room_flags, ROOM_SMITHY )
                 || IS_SET( ch->in_room->room_flags, ROOM_BANK )   
                 || IS_SET( victim->act, ACT_NOPUSH )
               )
          )
     )
  {
    act(AT_BLUE, "$N ignores you.", ch, NULL, victim, TO_CHAR);
    return;
  }

  if ( !str_cmp( arg2, "n" ) || !str_cmp( arg2, "north" ) ) door = 0;
  else if ( !str_cmp( arg2, "e" ) || !str_cmp( arg2, "east"  ) ) door = 1;
  else if ( !str_cmp( arg2, "s" ) || !str_cmp( arg2, "south" ) ) door = 2;
  else if ( !str_cmp( arg2, "w" ) || !str_cmp( arg2, "west"  ) ) door = 3;
  else if ( !str_cmp( arg2, "u" ) || !str_cmp( arg2, "up"    ) ) door = 4;
  else if ( !str_cmp( arg2, "d" ) || !str_cmp( arg2, "down"  ) ) door = 5;
  else door = dice(1,6) - 1;

  if(ch == victim)
  {
    send_to_char(AT_BLUE, "You attempt to push yourself, oook.\n\r", ch);
    return;
  }

  pexit = ch->in_room->exit[door];
  if(pexit == NULL || exit_blocked(pexit, ch->in_room) == EXIT_STATUS_PHYSICAL )
  {
    act(AT_BLUE, "There is no exit, but you push $M around anyways.", ch, NULL, victim, TO_CHAR);
    act(AT_BLUE, "$n pushes $N against a wall.", ch, NULL, victim, TO_NOTVICT);
    act(AT_BLUE, "$n pushes you against a wall, ouch.", ch, NULL, victim, TO_VICT);
    return;
  }

  if ( exit_blocked(pexit, ch->in_room) == EXIT_STATUS_MAGICAL )
  {
	act(AT_BLUE, "You slam into $N, but a force field prevents $S entry.", ch, NULL, victim, TO_CHAR );
	act(AT_BLUE, "$n pushes $N, but a force field makes $M bounce back.", ch, NULL, victim, TO_NOTVICT );
	act(AT_BLUE, "$n slams into you, but you bounce off a force field.", ch, NULL, victim, TO_VICT );
   return;
  }

  if ( exit_blocked(pexit, ch->in_room) == EXIT_STATUS_SNOWIN )
  {
	act(AT_BLUE, "You slam into $N, but a large mountain of snow prevents $S entry.", ch, NULL, victim, TO_CHAR );
	act(AT_BLUE, "$n pushes $N against a large mountain of snow.", ch, NULL, victim, TO_NOTVICT );
	act(AT_BLUE, "$n slams into you into a large mountain of snow.", ch, NULL, victim, TO_VICT );
   return;
  }

  sprintf(buf1, "You slam into $N, pushing $M %s.", dir_name[door]);
  sprintf(buf2, "$n slams into $N, pushing $M %s.", dir_name[door]);
  sprintf(buf3, "$n slams into you, pushing you %s.", dir_name[door]);
  act(AT_BLUE, buf2, ch, NULL, victim, TO_NOTVICT );
  act(AT_BLUE, buf1, ch, NULL, victim, TO_CHAR );
  act(AT_BLUE, buf3, ch, NULL, victim, TO_VICT );
  from_room = victim->in_room;
  eprog_enter_trigger( pexit, victim->in_room, victim );
  char_from_room(victim);
  char_to_room(victim, pexit->to_room);
  do_look( victim, "auto" );

  act(AT_BLUE, "$n comes flying into the room.", victim, NULL, NULL, TO_ROOM);
  if ( (pexit = pexit->to_room->exit[rev_dir[door]]) &&
       pexit->to_room == from_room )
    eprog_exit_trigger( pexit, victim->in_room, victim );
  else
    rprog_enter_trigger( victim->in_room, victim );
    
  return;
}

void do_drag(CHAR_DATA *ch, char *argument)
{
  char arg1[MAX_INPUT_LENGTH];
  char arg2[MAX_INPUT_LENGTH];
  EXIT_DATA *pexit;
  CHAR_DATA *victim;
  ROOM_INDEX_DATA *from_room;
  int door;
  char buf1[256], buf2[256], buf3[256];

  argument = one_argument(argument, arg1);
  one_argument(argument, arg2);

  if(arg1[0] == '\0')
  {
    send_to_char(AT_BLUE, "Drag who what where?", ch);
    return;
  }

  if((victim = get_char_room(ch, arg1)) == NULL)
  {
    send_to_char(AT_BLUE, "They aren't here.\n\r", ch);
    return;
  }

  if (    ( victim->pcdata->rank < RANK_STAFF )
       || (    IS_NPC( victim )
            && (    ( victim->pIndexData->pShop )
                 || IS_SET( ch->in_room->room_flags, ROOM_SMITHY )
                 || IS_SET( ch->in_room->room_flags, ROOM_BANK )
                 || IS_SET( victim->act, ACT_NODRAG )
               )
          )
     )
  {
    act(AT_BLUE, "$N ignores you.", ch, NULL, victim, TO_CHAR);
    return;
  }

  if ( !str_cmp( arg2, "n" ) || !str_cmp( arg2, "north" ) ) door = 0;
  else if ( !str_cmp( arg2, "e" ) || !str_cmp( arg2, "east"  ) ) door = 1;
  else if ( !str_cmp( arg2, "s" ) || !str_cmp( arg2, "south" ) ) door = 2;
  else if ( !str_cmp( arg2, "w" ) || !str_cmp( arg2, "west"  ) ) door = 3;
  else if ( !str_cmp( arg2, "u" ) || !str_cmp( arg2, "up"    ) ) door = 4;
  else if ( !str_cmp( arg2, "d" ) || !str_cmp( arg2, "down"  ) ) door = 5;
  else door = dice(1,6) - 1;

  if(ch == victim)
  {
    send_to_char(AT_BLUE, "You attempt to drag yourself, oook.\n\r", ch);
    return;
  }

  if(victim->position == POS_STANDING)
  {
    send_to_char(AT_BLUE, "Can't drag someone who is standing.\n\r", ch);
    return;
  }
  pexit = ch->in_room->exit[door];
  if(pexit == NULL || exit_blocked( pexit, ch->in_room ) == EXIT_STATUS_PHYSICAL )
  {
    act(AT_BLUE, "There is no exit, but you drag $M around anyways.", ch, NULL, victim, TO_CHAR);
    act(AT_BLUE, "$n drags $N around the room.", ch, NULL, victim, TO_NOTVICT);
    act(AT_BLUE, "$n drags you around the room.", ch, NULL, victim, TO_VICT);
    return;
  }
  if ( exit_blocked(pexit, ch->in_room) == EXIT_STATUS_MAGICAL )
    {
    act(AT_BLUE, "You grap onto $N, but a force field prevents you entry into the smithy.", ch, NULL, victim, TO_CHAR );
    act(AT_BLUE, "$n attempts to drag $N into the smithy, but a force field stops $m.", ch, NULL, victim, TO_NOTVICT );
    act(AT_BLUE, "$n attempts to drag you into the smithy, but a force field stops $m.", ch, NULL, victim, TO_VICT );
    return;
    }
  sprintf(buf1, "You get ahold of $N, dragging $M %s.", dir_name[door]);
  sprintf(buf2, "$n gets ahold of $N, dragging $M %s.", dir_name[door]);
  sprintf(buf3, "$n gets ahold of you, dragging you %s.", dir_name[door]);
  act(AT_BLUE, buf2, ch, NULL, victim, TO_NOTVICT);
  act(AT_BLUE, buf1, ch, NULL, victim, TO_CHAR);
  act(AT_BLUE, buf3, ch, NULL, victim, TO_VICT);

  from_room = ch->in_room;
  eprog_enter_trigger( pexit, ch->in_room, ch );
  eprog_enter_trigger( pexit, victim->in_room, victim );
  char_from_room(victim);
  char_to_room(victim, pexit->to_room);
  act(AT_BLUE, "$N arrives, dragging $n with $M.", victim, NULL, ch, TO_ROOM);
  char_from_room(ch);
  char_to_room(ch, victim->in_room);
  do_look( victim, "auto" );
  do_look( ch, "auto" );
  if ( (pexit = pexit->to_room->exit[rev_dir[door]]) &&
       pexit->to_room == from_room )
  {
    eprog_exit_trigger( pexit, ch->in_room, ch );
    eprog_exit_trigger( pexit, victim->in_room, victim );
  }
  else
  {
    rprog_enter_trigger( ch->in_room, ch );
    rprog_enter_trigger( victim->in_room, victim );
  }

  return;
}
/* END */

void check_nofloor( CHAR_DATA *ch )
{
  EXIT_DATA *pexit;
  ROOM_INDEX_DATA *to_room;

  if ( IS_SET( ch->in_room->room_flags, ROOM_NOFLOOR ) 
      && ( ( pexit = ch->in_room->exit[5] ) != NULL )
      && ( ( to_room = pexit->to_room )  != NULL )
       && ( (get_race_data(ch->race))->flying == 0 ) )
  {
    act( AT_RED, "You fall through where the floor should have been!", ch,
        NULL, NULL, TO_CHAR );
    act( C_DEFAULT, "$n falls down to the room below.", ch, NULL, NULL,
        TO_ROOM );
    damage( ch, ch, 5, TYPE_UNDEFINED, DAM_INTERNAL, TRUE );

    move_char( ch, 5, TRUE, FALSE ); 
  }
  return;
 }


void do_shadow_walk( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg[MAX_STRING_LENGTH];
    bool found = TRUE;

   one_argument( argument, arg );

     if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_shadow_walk ) )
    {
     typo_message( ch );
	return;
    }

    if ( ch->fighting )
    {
	send_to_char(C_DEFAULT, "Not while in combat.\n\r", ch );
	return;
    }
 
    if ( IS_AFFECTED( ch, AFF_ANTI_FLEE ) )
    {
      send_to_char( AT_GREY, "You cannot shadow walk in your condition!\n\r", ch );
      return;
    }

    if ( !strcmp( ch->in_room->area->name, "The Shadow Realm" ) )
    {
     if ( arg[0] == '\0' )
     {
      send_to_char(AT_GREY, "Shadow walk to whom?\n\r", ch );
      return;
     }

	if ( !( victim = get_char_world( ch, arg ) ) )
        found = FALSE;
        else
        if ( get_room_index( ROOM_VNUM_SHADOW_PATTERN_CENTER ) != ch->in_room )
	if ( IS_SET( victim->in_room->room_flags, ROOM_PRIVATE )
	||   IS_SET( victim->in_room->room_flags, ROOM_SOLITARY )
	||   IS_SET( victim->in_room->room_flags, ROOM_NO_SHADOW )
	||   IS_SET( victim->act, ACT_NOSHADOW )
	||   IS_AFFECTED( victim, AFF_NOASTRAL ) )
	found = FALSE;

      if ( !IS_SET( ch->in_room->room_flags, ROOM_NO_SHADOW ) )
      {
       send_to_char( AT_GREY, "The shadows will not obey you in this portion of the realm.\n\r", ch );
       return;
      }

      if ( !found )
      {
	send_to_char(AT_GREY, "The shadows offer no path to that one.\n\r", ch );
	return;
      }

     if ( !IS_AFFECTED(ch,AFF_HIDE) )
      act(AT_GREY, "$n steps into the shadows and is gone.", ch, NULL, NULL, TO_ROOM );
     if ( ch != victim )
     {
      char_from_room( ch );
      char_to_room( ch, victim->in_room );
     }
     if ( !IS_AFFECTED(ch,AFF_HIDE) )
      act(AT_GREY, "$n steps forth from the shadows.", ch, NULL, NULL, TO_ROOM );
     do_look( ch, "auto" );
     update_skpell( ch, gsn_shadow_walk );
     return;
    }
    else
    {
 	if ( IS_SET( ch->in_room->room_flags, ROOM_NO_SHADOW ) )
	{
         send_to_char( AT_WHITE, "You have no shadows to walk from.\n\r", ch );
	 return;
	}


    if ( !IS_AFFECTED(ch,AFF_HIDE) )
      act(AT_GREY, "$n steps into the shadows and is gone.", ch, NULL, NULL, TO_ROOM );

     char_from_room( ch );
     char_to_room( ch, get_room_index( ROOM_VNUM_SHADOW ) );

    if ( !IS_AFFECTED(ch,AFF_HIDE) )
      act(AT_GREY, "$n steps forth from the shadows.", ch, NULL, NULL, TO_ROOM );
    do_look( ch, "auto" );
    update_skpell( ch, gsn_shadow_walk );
    }
    return;
}

void do_scent( CHAR_DATA *ch, char *argument )
{
  static const char *dir_table [ ] =
	{ "to the north",
	  "to the east",
	  "to the south",
	  "to the west",
	  "above",
	  "below" };
  static const char *dis_table [ ] =
	{ "close by",
	  "not far off" };
  CHAR_DATA *sch;
  EXIT_DATA *pexit;
  ROOM_INDEX_DATA *in_room;
  ROOM_INDEX_DATA *next_room;
  char buf [ MAX_INPUT_LENGTH * 4 ];
  int dir, dis;
  bool found = FALSE;
  const char *dir_message;
  const char *dis_message;
  if ( IS_NPC( ch ) )
	return;

  if (ch->race != RACE_FELIXI && ch->race != RACE_CENTAUR )
  {
  if ( !can_use_skpell( ch, gsn_scent ) )
	{
	send_to_char( C_DEFAULT, "Your sense of smell is not keen enough.\n\r", 
			ch );
	return;
	}
  if ( ch->pcdata->learned[gsn_scent] < number_percent( ) )
	{
	send_to_char( C_DEFAULT, "You sniff around and start to sneeze!.\n\r",
			ch );
	return;
	}
  }
  in_room = ch->in_room;
  for ( dir = 0; dir <= 5; dir++ )
    {
    if ( !( pexit = in_room->exit[dir] ) 
    || !( next_room = pexit->to_room ) )
        continue;
    for ( dis = 0; dis <= 1; dis++ )
	{
	for ( sch = next_room->people; sch; sch = sch->next_in_room )
	    {
	    if ( sch->deleted )
		return;
	    if ( !(sch->desc)
	    && !IS_NPC( sch )
	    && get_trust( ch ) < LEVEL_IMMORTAL )
		continue;
	    if ( !found )
		{
		send_to_char( C_DEFAULT, "You pick up the scent of:\n\r", ch );
		found = TRUE;
		}
	    dir_message = dir_table[dir];
	    dis_message = dis_table[dis];
	    sprintf( buf, "%s &w%s %s.\n\r", capitalize (PERS( sch, ch )),
		     dis_message, dir_message );
	    send_to_char( C_DEFAULT, buf, ch );
	    }
	if ( !( pexit = next_room->exit[dir] ) 
	|| !( next_room = pexit->to_room ) )
	     break;
	}
    }

  update_skpell( ch, gsn_scent );

  if ( !found )
     send_to_char( C_DEFAULT, "You did not pick up anything's scent.\n\r", ch );

  return;

}

void do_scan( CHAR_DATA *ch, char *argument )
{
  static const char *dir_table [ ] =
	{ "to the north",
	  "to the east",
	  "to the south",
	  "to the west",
	  "above",
	  "below" };
  static const char *dis_table [ ] =
	{ "close by",
	  "not far off" };
  CHAR_DATA *sch;
  EXIT_DATA *pexit;
  ROOM_INDEX_DATA *in_room;
  ROOM_INDEX_DATA *next_room;
  char buf [ MAX_INPUT_LENGTH * 4 ];
  int dir, dis;
  bool found = FALSE;
  const char *dir_message;
  const char *dis_message;

  if ( IS_NPC( ch ) )
	return;

  in_room = ch->in_room;

  for ( sch = in_room->people; sch; sch = sch->next_in_room )
  {
   if ( !sch || sch->deleted )
    return;

   if ( ch == sch )
    continue;

   if ( !can_see( ch, sch ) )
    continue;

     if ( !found )
     {
      send_to_char( C_DEFAULT, "You can see:\n\r", ch );
      found = TRUE;
     }

   sprintf( buf, "%s &wright here in front of you.\n\r",
    capitalize (PERS( sch, ch )) );
   send_to_char( C_DEFAULT, buf, ch );
  }

  for ( dir = 0; dir <= 5; dir++ )
  {
    if ( !( pexit = in_room->exit[dir] ) 
     || !( next_room = pexit->to_room ) )
     continue;

    for ( dis = 0; dis <= 1; dis++ )
    {
     for ( sch = next_room->people; sch; sch = sch->next_in_room )
     {
      if ( !sch || sch->deleted )
       return;

      if ( !(sch->desc)
       && !IS_NPC( sch )
       && get_trust( ch ) < LEVEL_IMMORTAL )
       continue;

      if ( !can_see( ch, sch ) )
       continue;

      if ( !found )
      {
       send_to_char( C_DEFAULT, "You can see:\n\r", ch );
       found = TRUE;
      }

      dir_message = dir_table[dir];
      dis_message = dis_table[dis];

      sprintf( buf, "%s &w%s %s.\n\r", capitalize (PERS( sch, ch )),
       dis_message, dir_message );
      send_to_char( C_DEFAULT, buf, ch );
     }

     if ( !( pexit = next_room->exit[dir] ) 
      || !( next_room = pexit->to_room ) )
      break;
     }
    }

  if ( !found )
   send_to_char( C_DEFAULT, "You did not see anything.\n\r", ch );

  return;
}
