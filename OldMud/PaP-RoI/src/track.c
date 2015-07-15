/* ************************************************************************
*   File: graph.c                                       Part of CircleMUD *
*  Usage: various graph algorithms                                        *
*                                                                         *
*  All rights reserved.  See license.doc for complete information.        *
*                                                                         *
*  Copyright (C) 1993 by the Trustees of the Johns Hopkins University     *
*  CircleMUD is based on DikuMUD, Copyright (C) 1990, 1991.               *
************************************************************************ */


/* You can define or not define TRACK_THOUGH_DOORS, above, depending on
   whether or not you want track to find paths which lead through closed
   or hidden doors.
*/

#define linux 1
#if defined(macintosh)
#include <types.h>
#else
#include <sys/types.h>
#endif
#include <stdio.h>
#include <string.h>
#include <time.h>
#include "merc.h"

#define TRACK_THROUGH_DOORS
#define TRACK_IS_SKILL

struct track_queue_struct
{
   ROOM_INDEX_DATA *room;
   char   dir;
   struct track_queue_struct *next;
};

static struct track_queue_struct *queue_head = NULL, *queue_tail = NULL;

#define TRACK_NO_PATH          -1
#define TRACK_ALREADY_THERE    -2
#define TRACK_ERROR            -3


/* Utility macros */
#define MARK(room)      (SET_BIT((room)->room_flags, ROOM_MARK))
#define UNMARK(room)    (REMOVE_BIT((room)->room_flags, ROOM_MARK))
#define IS_MARKED(room) (IS_SET((room)->room_flags, ROOM_MARK))
#define TOROOM(x, y)    ((x)->exit[(y)] ? (x)->exit[(y)]->to_room : NULL)
#define IS_CLOSED(x, y) (exit_blocked((x)->exit[(y)], (x)) > EXIT_STATUS_OPEN)
#define IS_SAME_AREA(x,y)  ((x)->area == (y)->area)

#ifdef TRACK_THROUGH_DOORS
#define VALID_EDGE(x, y) ((x)->exit[(y)] && \
			  (TOROOM((x), (y)) != NULL) &&	\
			  (!IS_MARKED(TOROOM((x), (y)))) &&  \
                          (IS_SAME_AREA((x), TOROOM((x),(y)))))
#else
#define VALID_EDGE(x, y) ((x)->exit[(y)] && \
			  (TOROOM((x), (y)) != NULL) &&	\
			  (!IS_CLOSED((x), (y))) &&		\
			  (!IS_MARKED(TOROOM((x), (y)))) && \
                          (IS_SAME_AREA((x), TOROOM((x),(y)))))
#endif

bool can_go( CHAR_DATA *ch, int dir )
{
  ROOM_INDEX_DATA *room;
  EXIT_DATA *exit;

  if ( !(room = ch->in_room) )
    return FALSE;

  if ( !(exit = ch->in_room->exit[dir]) )
    return FALSE;

  if ( !exit->to_room )
    return FALSE;

/*  if ( !IS_SAME_AREA(room, exit->to_room) )
    return FALSE;*/

  if ( IS_SET(exit->exit_info, EX_BASHED) )
    return TRUE;

#ifdef TRACK_THROUGH_DOORS
  if ( IS_CLOSED(room, dir) )
    return FALSE;
#endif

  return TRUE;
}
  

void track_enqueue(ROOM_INDEX_DATA *room, char dir)
{
  struct track_queue_struct *curr;

  curr = alloc_mem( sizeof( *curr ));
  curr->room = room;
  curr->dir = dir;
  curr->next = NULL;

  if (queue_tail)
  {
    queue_tail->next = curr;
    queue_tail = curr;
  }
  else
    queue_head = queue_tail = curr;
}


void track_dequeue(void)
{
  struct track_queue_struct *curr;

  curr = queue_head;

  if (!(queue_head = queue_head->next))
    queue_tail = NULL;
  free_mem(curr, sizeof( *curr ));
}


void track_clear_queue(void) 
{
  while (queue_head)
    track_dequeue();
}


/* find_first_step: given a source room and a target room, find the first
   step on the shortest path from the source to the target.

   Intended usage: in mobile_activity, give a mob a dir to go if they're
   tracking another mob or a PC.  Or, a 'track' skill for PCs.
*/

int find_first_step(ROOM_INDEX_DATA *src, ROOM_INDEX_DATA *target)
{
   int curr_dir;
   int curr_room;

   if (!src || !target)
   {
      bug("Illegal value passed to find_first_step (track.c)", 0 );
      return TRACK_ERROR;
   }

   if (src == target)
      return TRACK_ALREADY_THERE;

   if ( !IS_SAME_AREA( src, target ) )
     return TRACK_NO_PATH;

   /* clear marks first */
   for (curr_room = src->area->lvnum; curr_room <= src->area->uvnum;
	curr_room++)
     if ( get_room_index(curr_room) )
       UNMARK(get_room_index(curr_room));

   MARK(src);

   /* first, enqueue the first steps, saving which direction we're going. */
   for (curr_dir = 0; curr_dir < 6; curr_dir++)
      if (VALID_EDGE(src, curr_dir))
      {
         MARK(TOROOM(src, curr_dir));
         track_enqueue(TOROOM(src, curr_dir), curr_dir);
      }

   /* now, do the track. */
   while (queue_head)
   {
      if (queue_head->room == target)
      {
	 curr_dir = queue_head->dir;
	 track_clear_queue();
	 return curr_dir;
      }
      else
      {
	for (curr_dir = 0; curr_dir < 6; curr_dir++)
	  if (VALID_EDGE(queue_head->room, curr_dir))
	  {
	    MARK(TOROOM(queue_head->room, curr_dir));
	    track_enqueue(TOROOM(queue_head->room, curr_dir),queue_head->dir);
	  }
	track_dequeue();
      }
   }

   return TRACK_NO_PATH;
}


/************************************************************************
*  Functions and Commands which use the above fns		        *
************************************************************************/

void do_track( CHAR_DATA *ch, char *argument )
{
   CHAR_DATA *vict;
   int dir;
   char arg[MAX_INPUT_LENGTH];
   char buf[MAX_STRING_LENGTH];

   one_argument(argument, arg);
   
   if ( !(ch->race == RACE_FELIXI) )
   {
    if ( !IS_NPC( ch )
        && !can_use_skpell( ch, gsn_track ) )
    {
	typo_message( ch );
        return;
    }
   }
    
   if (!*arg)
   {
     if ( ch->hunting )
     {
       send_to_char(AT_RED, "No longer tracking.\n\r", ch );
       ch->hunting = NULL;
     }
     else
     {
       send_to_char(AT_BLUE, "Whom are you trying to track?\n\r", ch);
     }
     return;
   }

   if (!(vict = get_char_world(ch, arg)) || !IS_SAME_AREA(ch->in_room,
							  vict->in_room))
   {
      send_to_char(AT_BLUE, "You can't find them.\n\r", ch);
      return;
   }

   dir = find_first_step(ch->in_room, vict->in_room);

   switch(dir)
   {
      case TRACK_ERROR:
         send_to_char(AT_BLUE, "Hmm.. something seems to be wrong.\n\r", ch);
	 ch->hunting = NULL;
         break;
      case TRACK_ALREADY_THERE:
         send_to_char(AT_BLUE, "You're already in the same room!!\n\r", ch);
	 ch->hunting = NULL;
         break;
      case TRACK_NO_PATH:
         sprintf(buf, "You can't sense a trail to %s from here.\n\r",
		 vict->sex == SEX_FEMALE ? "her" :
		 vict->sex == SEX_MALE ? "him" : "it" );
         send_to_char(AT_BLUE, buf, ch);
	 ch->hunting = NULL;
         break;
      default:
         /* if you want to make this into a skill instead of a command,
            the next few lines make it give you a random direction if you
            fail the random skill roll.
         */

#ifdef TRACK_IS_SKILL
	 {
/*	   int counter;*/
	   if( !IS_NPC(ch)
	   && ( ch->pcdata->learned[gsn_track] < number_percent() || ch->race == RACE_FELIXI ) )
	   {
	     int numv = 0;
	     CHAR_DATA *vch = NULL;

	     for ( vch = char_list; vch; vch = vch->next )
	     {
	       if ( vch->deleted || !IS_NPC(vch) || !can_see(ch, vch) ||
		    !IS_SAME_AREA(ch->in_room, vch->in_room) ||
		    (dir = find_first_step(ch->in_room, vch->in_room)) < 0 )
		 continue;
	       if ( number_range(0,numv) == 0 )
		 vict = vch;
	       numv++;
	     }
	     if ( !vict )
	     {
	       send_to_char(AT_BLUE, "You can't sense a trail from here.\n\r",ch);
	       return;
	     }
/*	     for( counter = 0; counter < 50; counter++ )
	     {
	       dir = number_door();
	       if ( can_go( ch, dir ) &&
		    IS_SAME_AREA(ch->in_room, ch->in_room->exit[dir]->to_room))
		 break;
	       dir = -1;
	     }
	     for ( vch = char_list; vch; vch = vch->next )
	     {
	       if ( vch->deleted || !IS_NPC(vch) )
		 continue;
	       if ( number_range(0,numv) == 0 )
		 vict = vch;
	       numv++;
	     }*/
	   }
	   else if ( ch->race != RACE_FELIXI )
	     update_skpell( ch, gsn_track );
	   if ( dir < 0 )
	   {
	     sprintf(buf, "You can't sense a trail to %s from here.\n\r",
		     vict->sex == SEX_FEMALE ? "her" :
		     vict->sex == SEX_MALE ? "him" : "it" );
	     send_to_char(AT_BLUE, buf, ch );
	     return;
	   }
	 }
#endif

	 if ( !IS_NPC(vict) )
	 {
	   sprintf(buf, "You sense a trail %s from here!\n\r", dir_name[dir]);
	   send_to_char(AT_RED, buf, ch);
	 }
	 else
	   ch->hunting = vict;
         break;
   }
}


void hunt_victim(CHAR_DATA *ch)
{
   int dir;

   if (!ch || !ch->hunting)
      return;

   if ( !can_see(ch, ch->hunting))
   {
     if ( ch->desc )
       write_to_buffer(ch->desc, "&rTrack1: &RYou have lost the trail.\n\r",0);
     ch->hunting = NULL;
     if ( (ch->fighting) )
      stop_fighting( ch, FALSE );
     return;
   }

   dir = find_first_step(ch->in_room, ch->hunting->in_room);
   if (dir < 0)
   {
     if ( ch->desc )
     {
       if ( dir == TRACK_ALREADY_THERE )
	 write_to_buffer(ch->desc,
			 "\n\r&rTrack: &RAhh.. You have found your prey.",0);
       else
	 write_to_buffer(ch->desc, "\n\r&rTrack2: &RYou have lost the trail.",
			 0);
     }
     ch->hunting = NULL;
     return;
   }
   else
   {
     if ( IS_NPC( ch ) )
     {
       move_char(ch, dir, FALSE, FALSE);
       sprintf( log_buf, "%s hunting %s at %d.", ch->short_descr,
	      ( IS_NPC( ch->hunting ) ) ? ch->hunting->short_descr : 
	        ch->hunting->name, ch->hunting->in_room->vnum );
       log_string( log_buf, 1 , -1 );
     }
     else
     {
       sprintf(log_buf,"\n\r&rTrack: &RYou sense a trail %s of here!",
	       dir_name[dir]);
       write_to_buffer(ch->desc, log_buf, 0);
     }
      if (ch->in_room == ch->hunting->in_room)
      {
	if ( IS_NPC( ch ) )
	{
         char victnamebuf[MAX_STRING_LENGTH];
         char buf[MAX_STRING_LENGTH];

         switch ( ch->fighting_style )
         {
          case FIGHTING_STYLE_STREET:
	   multi_hit(ch, ch->hunting, TYPE_UNDEFINED);
           break;
          case FIGHTING_STYLE_NORMAL:
	   multi_hit(ch, ch->hunting, TYPE_UNDEFINED);
           break;
          case FIGHTING_STYLE_MARTIAL:
           sprintf( buf, "%s", ch->hunting->name );
           one_argument( buf, victnamebuf );
 	   sprintf( buf, "anklestrike %s", victnamebuf );
           interpret( ch, buf );
           break;
          case FIGHTING_STYLE_WIMPY:
          case FIGHTING_STYLE_BEAST_WIMPY:
           break;
          case FIGHTING_STYLE_SNEAK:
           sprintf( buf, "%s", ch->hunting->name );
           one_argument( buf, victnamebuf );
 	   sprintf( buf, "backstab %s", victnamebuf );
           interpret( ch, buf );
           break;
          case FIGHTING_STYLE_BEAST_AGGRESSIVE:
           sprintf( buf, "%s", ch->hunting->name );
           one_argument( buf, victnamebuf );
 	   sprintf( buf, "growl %s", victnamebuf );
           interpret( ch, buf );
 	   sprintf( buf, "bite %s", victnamebuf );
           interpret( ch, buf );
           break;
          case FIGHTING_STYLE_WARLOCK:
           if ( ch->level > 100 )
           {
            sprintf( buf, "%s", ch->hunting->name );
            one_argument( buf, victnamebuf );
 	    sprintf( buf, "cast 'hold' %s", victnamebuf );
            interpret( ch, buf );
           }
           break;
          case FIGHTING_STYLE_CLERIC:
           if ( ch->level > 100 )
           {
            sprintf( buf, "%s", ch->hunting->name );
            one_argument( buf, victnamebuf );
 	    sprintf( buf, "cast 'curse' %s", victnamebuf );
            interpret( ch, buf );
           }
           break;
          case FIGHTING_STYLE_BARBARIAN:
            sprintf( buf, "%s", ch->hunting->name );
            one_argument( buf, victnamebuf );
 	    sprintf( buf, "slam %s", victnamebuf );
            interpret( ch, buf );
           break;
          }
	}
	else if ( ch->desc )
	  write_to_buffer(ch->desc,
			  "\n\r&rTrack: &RAhh.. you have found your prey.",0);
	ch->hunting = NULL;
      }	 
      return;
   }
}
