/*
 * Vehicle.c - ROM/ROT vehicle code. Ver. 2.05
 *
 * Code is Copyright 1997 by Dominic J. Eidson, code may be freely 
 * distributed and modified.
 * 
 */

#if defined(macintosh)
#include <types.h>
#else
#include <sys/types.h>
#include <sys/time.h>
#endif
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <ctype.h>
#include <time.h>
#include "merc.h"

/* command procedures needed */
/*
 * Local functions.
 */
int	find_hack_door	args( ( CHAR_DATA *ch, char *arg ) );
void	enter_hack_exit	args( ( CHAR_DATA *ch, OBJ_DATA *obj, char *arg )
);
void    move_vehicle	args( ( CHAR_DATA *ch, OBJ_DATA *obj, int door )
);
bool	check_blind	args( ( CHAR_DATA *ch ) );
void    show_char_to_char args( ( CHAR_DATA *list, CHAR_DATA *ch ) );
void	do_hack_exits	args( ( CHAR_DATA *ch, ROOM_INDEX_DATA *room, char
*argument ) );
BUFFER * show_list_to_char      args( ( OBJ_DATA *list, CHAR_DATA *ch,
                                    bool fShort, bool fShowNothing ) );
void	enter_hack_exit args( ( CHAR_DATA *ch, OBJ_DATA *obj, char *arg)
);
void 	do_hack_look	args( (CHAR_DATA *ch, char *argument ) );
void	do_look		args( (CHAR_DATA *ch, char *argument ) );


DECLARE_DO_FUN(do_stand         );

/* New function needed for proper working */

OBJ_DATA *get_obj_exit( char *argument, OBJ_DATA *list )
{
    char arg[MAX_INPUT_LENGTH];
    OBJ_DATA *obj;
    int number;
    int count;

    number = number_argument( argument, arg );
    count  = 0;
    for ( obj = list; obj != NULL; obj = obj->next_content )
    {
        if ( (obj->item_type == ITEM_EXIT) && is_name(arg, obj->name) )
        {
            if ( ++count == number )
                return obj;
        }
    }
    return NULL;
}

/*
 * Take care of exiting a vehicle
 */

void do_leave( CHAR_DATA *ch, char *argument )
{
    ROOM_INDEX_DATA *room;
    char buf[MAX_STRING_LENGTH];

    /* Are we in a vehicle? */
    if(ch->in_room->inside_of == NULL)
    {
	send_to_char("You are not riding anything.",ch);
	return;
    }

    if (IS_SET(ch->act2, PLR_LOOKOUT) )
	{
	REMOVE_BIT(ch->act2, PLR_LOOKOUT );
	}

    if (IS_SET( ch->act2, PLR_HELM) )
	{
	REMOVE_BIT(ch->act2, PLR_HELM);
	}

    if (IS_SET( ch->act2, PLR_DRIVER) )
	{
	REMOVE_BIT(ch->act2, PLR_DRIVER);
	}

    sprintf( buf, "%s leaves %s.", IS_NPC(ch) ? ch->short_descr :
capitalize(ch->name),
		ch->in_room->inside_of->short_descr);
         send_to_char( buf, ch );
    act("You leave
$T.",ch,NULL,ch->in_room->inside_of->short_descr,TO_CHAR);
    act(buf,ch,NULL,NULL,TO_ROOM);
    room = ch->in_room->inside_of->in_room;
    char_from_room(ch);
    char_to_room(ch,room);
    act(buf, ch, NULL, NULL, TO_ROOM);
    do_look(ch, "auto");
}

/*
 * the fullowing function is tricky. We are using a portal object as 
 * vehicle. This means, we must dereference everything through 
 *  ch->in_room->inside_of, which is the vehicle object.
 * I am not yet sure how well this will work.
 */
void do_hack_look( CHAR_DATA *ch, char *argument )
{
    char buf  [MAX_STRING_LENGTH];
    char arg1 [MAX_INPUT_LENGTH];
    char arg2 [MAX_INPUT_LENGTH];
    char arg3 [MAX_INPUT_LENGTH];
    BUFFER *outlist;
    int number,count;

    if (!IS_SET( ch->act2, PLR_LOOKOUT) && !IS_SET(ch->act2, PLR_DRIVER))
{
	return;
}

    if(IS_NPC(ch) && !ch->in_room->inside_of)
{
	return;
}
    if ( ch->position < POS_SLEEPING )
    {
	send_to_char( "You can't see anything but stars!\n\r", ch );
	return;
    }

    if ( ch->position == POS_SLEEPING )
    {
	send_to_char( "You can't see anything, you're sleeping!\n\r", ch);
	return;
    }

    if ( !check_blind( ch ) )
{
	return;
}

    if ( !IS_NPC(ch)
    &&   !IS_SET(ch->act, PLR_HOLYLIGHT)
    &&   room_is_dark( ch->in_room->inside_of->in_room ) )
    {
	send_to_char( "It is pitch black ... \n\r", ch );
	show_char_to_char( ch->in_room->inside_of->in_room->people, ch );
	return;
    }

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );
    number = number_argument(arg1,arg3);
    count = 0;

    if ( arg1[0] == '\0' || !str_cmp( arg1, "auto" ) )
    {
	/* 'look' or 'look auto' */
	send_to_char( "{e", ch);
	send_to_char( ch->in_room->inside_of->in_room->name, ch);
	send_to_char( "{x", ch);

	if (IS_NPC(ch) || IS_SET(ch->act,PLR_HOLYLIGHT))
	{
	    sprintf(buf," [Room
%d]",ch->in_room->inside_of->in_room->vnum);
	    send_to_char(buf,ch);
	}

	send_to_char( "\n\r", ch );

	if ( arg1[0] == '\0'
	|| ( !IS_NPC(ch) && !IS_SET(ch->act, PLR_BRIEF) ) )
	{
	    send_to_char( "  ",ch);
	    send_to_char( ch->in_room->inside_of->in_room->description, ch
);
	}

        if ( !IS_NPC(ch) && IS_SET(ch->act, PLR_AUTOEXIT) )
	{
	    send_to_char("\n\r",ch);
            do_hack_exits( ch, ch->in_room->inside_of->in_room, "auto" ); 
	}

	outlist = show_list_to_char(
ch->in_room->inside_of->in_room->contents, ch, FALSE, FALSE );
	send_to_char( buf, ch );
/*	free_buf(outlist);   */
	show_char_to_char( ch->in_room->inside_of->in_room->people,ch );
	return;
    }
    return;
}

/*
 * Thanks to Zrin for auto-exit part.
 */
void do_hack_exits(CHAR_DATA *ch, ROOM_INDEX_DATA *room, char *argument )
{
    extern char * const dir_name[];
    char buf[MAX_STRING_LENGTH];
    EXIT_DATA *pexit;
    bool found;
    bool round;
    bool fAuto;
    int door;
    int outlet;

    fAuto  = !str_cmp( argument, "auto" );

    if ( !check_blind( ch ) )
	return;


     if (IS_IMMORTAL(ch))
	sprintf(buf,"Obvious exits from room %d:\n\r [", room->vnum);
    else
	sprintf(buf,"Obvious exits:\n\r [");

    found = FALSE;
    for ( door = 0; door < 6; door++ )
    {
	round = FALSE;
	outlet = door;
	if ( ( pexit = room->exit[outlet] ) != NULL
	&&   pexit->to_room != NULL
	&&   can_see_room(ch,pexit->to_room) 
	&&   !IS_SET(pexit->exit_info, EX_CLOSED) )
	{
	    found = TRUE;
	    round = TRUE;
	    if ( fAuto )
	    {
		strcat( buf, " " );
		strcat( buf, dir_name[outlet] );
	    }
	    else
	    {
		sprintf( buf + strlen(buf), "%-5s - %s",
		    capitalize( dir_name[outlet] ),
		    room_is_dark( pexit->to_room )
			?  "Too dark to tell"
			: pexit->to_room->name
		    );
		if (IS_IMMORTAL(ch))
		    sprintf(buf + strlen(buf), 
			" (room %d)\n\r",pexit->to_room->vnum);
		else
		    sprintf(buf + strlen(buf), "\n\r");
	    }
	}
	/*if (!round)
	{
	    OBJ_DATA *portal;
	    ROOM_INDEX_DATA *to_room;

	    portal = get_obj_exit( dir_name[door], room->contents );
	    if (portal != NULL)
	    {
		found = TRUE;
		round = TRUE;
		if ( fAuto )
		{
		    strcat( buf, " " );
		    strcat( buf, dir_name[door] );
		}
		else
		{
		    to_room = get_room_index(portal->value[0]);
		    sprintf( buf + strlen(buf), "%-5s - %s",
			capitalize( dir_name[door] ),
			room_is_dark( to_room )
			    ?  "Too dark to tell"
			    : to_room->name
			);
		    if (IS_IMMORTAL(ch))
			sprintf(buf + strlen(buf), 
			    " (room %d)\n\r",to_room->vnum);
		    else
			sprintf(buf + strlen(buf), "\n\r");
		}
	    }
	}
*/
    }

    if ( !found )
	strcat( buf, fAuto ? " none" : "None.\n\r" );

    if ( fAuto )
	strcat( buf, " ]\n\r" );

    send_to_char( buf, ch );
    return;
}

void move_vehicle( CHAR_DATA *ch, OBJ_DATA *obj, int door )
{
    ROOM_INDEX_DATA *in_room;
    ROOM_INDEX_DATA *to_room;
    EXIT_DATA *pexit;
    char buf[MAX_STRING_LENGTH];
    CHAR_DATA *vch;
    OBJ_DATA   *vehicle;

    if ( door < 0 || door > 5 )
    {
	bug( "Do_move: bad door %d.", door );
	return;
    }

    in_room = obj->in_room;

/*    if ( ( pexit   = in_room->exit[door] ) == NULL
    ||   ( to_room = pexit->to_room   ) == NULL 
    ||	 !can_see_room(ch,pexit->to_room))
    {
	return;
    }
*/
   
    if ( !( pexit = in_room->exit[door] ) || !( to_room = pexit->to_room )
)
    {
	send_to_char( "You may not go in that direction.\n\r", ch );
	return;
    }


    if (IS_SET(pexit->exit_info, EX_CLOSED))
    {
	return;
    }

        vehicle = ch->in_room->inside_of;
     
	        if ( (in_room->sector_type <= 5
            ||   to_room->sector_type <= 5))
	{
	 if (vehicle->value[1] != 2 )
	{
	send_to_char( "This vehicle can't move on land.\n\r", ch );
	return;
	}
	}

	if ( (in_room->sector_type == SECT_AIR
	||   to_room->sector_type == SECT_AIR) )
	{
   if (vehicle->value[1] != 3) 
	{
	    send_to_char( "This vehicle can't fly.\n\r", ch);
	    return;
	}

	}

	if ( in_room->sector_type == SECT_WATER_NOSWIM
	||   to_room->sector_type == SECT_WATER_NOSWIM)
	{
      if (vehicle->value[1] != 1 )
	{
	    send_to_char("This vehicle can't go in the water.", ch);
	    return;
	}

	}

    /* Different exit messages depending on the vehicle type -Goth */
    /* Ships */
    if (vehicle->value[1] == 1)
     {
sprintf(buf, "%s slowly set's sail to the %s.",
capitalize(ch->in_room->inside_of->short_descr), dir_name[door]);
     }
    /* Land Vehicles */
    else if (vehicle->value[1] == 2)
    {
    sprintf(buf, "%s rolls off %s.",
capitalize(ch->in_room->inside_of->short_descr), dir_name[door]);
    }
    /* air ships */
    else if (vehicle->value[1] == 3)
    {
    sprintf(buf, "%s mysteriously floats %s.",
capitalize(ch->in_room->inside_of->short_descr), dir_name[door]);
    }
    /* Golems */
    else if (vehicle->value[1] == 4)
    {
    sprintf(buf, "%s angrily stomps to the %s.",
capitalize(ch->in_room->inside_of->short_descr), dir_name[door]);
    }
    /* If new and not set, display SOMETHING! */
    else
    {
    sprintf(buf, "%s moves to the %s.",
capitalize(ch->in_room->inside_of->short_descr), dir_name[door]);
    }


    for ( vch = ch->in_room->inside_of->in_room->people; vch ; vch =
vch->next_in_room )
    {
	send_to_char(buf,vch);
    }

    obj_from_room( obj );
    obj_to_room( obj, to_room );

       if (vehicle->value[1] == 1)
     {
     sprintf(buf, "%s slowly sails in.",
capitalize(ch->in_room->inside_of->short_descr));
     }  
    /* Land Vehicles */
    else if (vehicle->value[1] == 2)
    {
    sprintf(buf, "%s rolls in.",
capitalize(ch->in_room->inside_of->short_descr));
    }
    /* air ships */
    else if (vehicle->value[1] == 3)
    {
    sprintf(buf, "%s mysteriously floats in.",
capitalize(ch->in_room->inside_of->short_descr));
    }
    /* Golems */
    else if (vehicle->value[1] == 4)
    {
    sprintf(buf, "The ground shakes as %s stomps in.",
capitalize(ch->in_room->inside_of->short_descr));
    }
       /* If new and not set, display SOMETHING! */
    else
    {
    sprintf(buf, "%s enters the room.",
capitalize(ch->in_room->inside_of->short_descr));
    }

    for ( vch = ch->in_room->inside_of->in_room->people; vch ; vch =
vch->next_in_room )
    {
	send_to_char(buf,vch);
    }


    for ( vch = ch->in_room->people; vch ; vch = vch->next_in_room )
{
   if (!IS_SET(vch->act2, PLR_LOOKOUT ) && !IS_SET(vch->act2, PLR_DRIVER))
    {
        if (vehicle->value[1] == 1)
     {
     send_to_char( "You feel the ship lurch as it moves.\n\r", vch );
     }  
    /* Land Vehicles */
    else if (vehicle->value[1] == 2)
    {
    send_to_char( "You bob up and down as the carriage moves.\n\r", vch );
    }
    /* air ships */
    else if (vehicle->value[1] == 3)
    {
    send_to_char( "You barely feel the ship sail through the air
smoothly.\n\r", vch );
    }
    /* Golems - There should be no passangers in Golems */
    else if (vehicle->value[1] == 4)
    {
    send_to_char( "This message is an Error, Please contact an imm about
this Golem.\n\r", vch );
    /* Log It? May get spammy if the person doesn't report it right away.
*/
    }
       /* If new and not set, display SOMETHING! */
    else
    {
    send_to_char( "You feel the vehicle move.\n\r", vch );
    }

    } 
else
{  
   	do_hack_look( vch, "auto" );
}
}
    if (in_room == to_room) /* no circular follows */
	return;
}

/* RW Enter movable exits */
void enter_hack_exit( CHAR_DATA *ch, OBJ_DATA *obj, char *arg)
{    
    ROOM_INDEX_DATA *location; 

    if (arg[0] != '\0')
    {
        ROOM_INDEX_DATA *old_room;
	OBJ_DATA *portal;

        old_room = obj->in_room;

	portal = get_obj_list( ch, arg,  obj->in_room->contents );
	
	if (portal == NULL)
	{
	    send_to_char("And how would you do that?\n\r",ch);
	    return;
	}

	if (portal->item_type != ITEM_EXIT) 
	{
	    send_to_char("And how would you do that?\n\r",ch);
	    return;
	}

	location = get_room_index(portal->value[0]);

	if (location == NULL
	||  location == old_room
	||  !can_see_room(ch,location) 
	||  (room_is_private(location) && (ch->level <=75)))
	{
	    send_to_char("And how would you do that?\n\r",ch);
	    return;
	}

	if ( !IS_NPC(ch) )
	{

	    if ( old_room->sector_type == SECT_AIR
	    ||   location->sector_type == SECT_AIR )
	    {
		send_to_char("Vehicles can't fly.", ch);
		return;
	    }

	    if (( old_room->sector_type == SECT_WATER_NOSWIM
	    ||    location->sector_type == SECT_WATER_NOSWIM ))
	    {
		    send_to_char( "Vehicles can not go there.\n\r", ch );
		    return;
	    }

	}

	if ( !IS_AFFECTED(ch, AFF_SNEAK))
	{
	    act("$n rolls off $T.",ch,NULL,arg,TO_ROOM);
	}

	obj_from_room(obj);
	obj_to_room(obj, location);


	if ( !IS_AFFECTED(ch, AFF_SNEAK))
	{
	    act("$n rolls in.",ch,NULL,NULL,TO_ROOM);
	}

    if (IS_SET( ch->act2, PLR_LOOKOUT ) )
{
	do_hack_look(ch,"auto");
}

	/* protect against circular follows */
	if (old_room == location)
	    return;

	return;
    }

    send_to_char("Alas, you cannot go that way.\n\r",ch);
    return;
}


void do_north( CHAR_DATA *ch, char *argument )
{
    if(ch->in_room->inside_of)
	send_to_char("you are riding.\n\r",ch);
    else
	move_char( ch, DIR_NORTH );
    return;
}

void do_east( CHAR_DATA *ch, char *argument )
{
    if(ch->in_room->inside_of)
	send_to_char("you are riding.\n\r",ch);
    else
	move_char( ch, DIR_EAST );
    return;
}

void do_south( CHAR_DATA *ch, char *argument )
{
    if(ch->in_room->inside_of)
	send_to_char("you are riding.\n\r",ch);
    else
	move_char( ch, DIR_SOUTH );
    return;
}

void do_west( CHAR_DATA *ch, char *argument )
{
    if(ch->in_room->inside_of)
	send_to_char("you are riding.\n\r",ch);
    else
	move_char( ch, DIR_WEST );
    return;
}

void do_up( CHAR_DATA *ch, char *argument )
{
    if(ch->in_room->inside_of)
	send_to_char("you are riding.\n\r",ch);
    else
	move_char( ch, DIR_UP );
    return;
}

void do_down( CHAR_DATA *ch, char *argument )
{
    if(ch->in_room->inside_of)
	send_to_char("you are riding.\n\r",ch);
    else
	move_char( ch, DIR_DOWN );
    return;
}


int hack_scan_room (CHAR_DATA *ch, const ROOM_INDEX_DATA *room,char *buf)
{
    CHAR_DATA *target = room->people;
    int number_found = 0;
 
    while (target != NULL) /* repeat as long more peple in the room */
    {
        if (can_see(ch,target)) /* show only if the character can see the
target */
        {
            strcat (buf, " - ");
            strcat (buf, IS_NPC(target) ? target->short_descr :
target->name);   
            strcat (buf, "\n\r");
            number_found++;
        }
        target = target->next_in_room;
    }
 
    return number_found;
}

/* Lookout commands, Lookout's need to keep an eye out. -Goth */
void do_lookout( CHAR_DATA *ch, char *argument)
{
        OBJ_DATA   *vehicle;
        char       arg [ MAX_INPUT_LENGTH ];

/*	smash_tilde( argument ); */
    	argument = one_argument( argument, arg );

    vehicle = ch->in_room->inside_of;

    if(ch->in_room->inside_of == NULL)
    {
        send_to_char("You are not riding anything.\n\r",ch);
        return;
    }
 
   if (!IS_SET( ch->act2, PLR_LOOKOUT ))
   {
	send_to_char( "You can't use this unless your the Lookout!\n\r",
ch );
	return;
   }
   

   if ( arg[0] == '\0'   )
    {
	send_to_char( "Lookout and do what?\n\r", ch );
	send_to_char( "Commands: Look Scan\n\r", ch );
	return;
    }

  

   /* Look */
    if ( !str_cmp( arg, "look" ) )
    {
    /* Simple enough, It's already defined so we just tack it on. */
        do_hack_look(ch, "auto");
	return;
    }
   else if ( !str_cmp( arg, "scan" ) )
	{
    EXIT_DATA * pexit;
    ROOM_INDEX_DATA * room;
    extern char * const dir_name[];
    char buf[MAX_STRING_LENGTH];
    int dir;  
    int distance;
            
    sprintf (buf, "In this location you see:\n\r");
    if (hack_scan_room(ch,ch->in_room->inside_of->in_room,buf) == 0)
        strcat (buf, "Not a damned thing.\n\r");
    send_to_char (buf,ch);
    
    for (dir = 0; dir < 6; dir++) /* look in every direction */
    {
        room = ch->in_room->inside_of->in_room; /* starting point */
 
        for (distance = 1 ; distance < 4; distance++)
        {
            pexit = room->exit[dir]; /* find the door to the next room */
            if ((pexit == NULL) || (pexit->to_room == NULL) ||
(IS_SET(pexit->exit_info, EX_CLOSED)))
                break; /* exit not there OR points to nothing OR is closed
*/
 
            /* char can see the room */
            sprintf (buf, "%d %s from here you see:\n\r", distance,
dir_name[dir]);
            if (hack_scan_room(ch,pexit->to_room,buf)) /* if there is
something there */
                send_to_char (buf,ch);
            room = pexit->to_room; /* go to the next room */
        } /* for distance */
    } /* for dir */
return;
}
return;
}

/* Appoint, Let's Captains set positions -Goth */
void do_appoint( CHAR_DATA *ch, char *argument)
{
        OBJ_DATA   *vehicle;
        CHAR_DATA *victim;
        char       arg [ MAX_INPUT_LENGTH ];

/*	smash_tilde( argument ); */
    	argument = one_argument( argument, arg );

    vehicle = ch->in_room->inside_of;

    if(ch->in_room->inside_of == NULL)
    {
        send_to_char("You are not riding anything.\n\r",ch);
        return;
    }
 
   act( "Argument: $t", ch, argument, NULL, TO_CHAR );
   act( "Arg: $t", ch, arg, NULL, TO_CHAR );
   if (!IS_SET( ch->act2, PLR_CAPTAIN ))
   {
	send_to_char( "You aren't even a Captain!\n\r", ch );
	return;
   }
   

   if ( arg[0] == '\0' || argument[0] == '\0' )
    {
	send_to_char( "Appoint who to what position?\n\r", ch );
	send_to_char( "Positions: Lookout Helm Driver Pilot\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
        send_to_char( "They aren't here.\n\r", ch );
        return;
    }

    if ( IS_NPC( victim ) )
    {
        send_to_char( "You can't appoint NPC, it only works on PC's.\n\r",
ch );
        return;
    }

    if (IS_SET(victim->act2, PLR_CAPTAIN) )
	{
	send_to_char("That person was trained as a captain, they can't be
your crew!\n\r", ch );
	return;
	}

/* Okay, Finally all the checks are done, victim is valid */

   /* Lookout */
    if ( !str_cmp( argument, "lookout" ) )
    {
   
     if ( IS_SET( victim->act2, PLR_HELM) )
	REMOVE_BIT( victim->act2, PLR_HELM);

     if ( IS_SET( victim->act2, PLR_LOOKOUT ) )
        {
            REMOVE_BIT( victim->act2, PLR_LOOKOUT );
            send_to_char("You no longer the lookout.\n\r", victim );
            send_to_char("That person is no longer the lookout.\n\r", ch
);
            return;
        }
        else
        {
            SET_BIT( victim->act2, PLR_LOOKOUT );
            send_to_char("You have been appointed Lookout by your
Captain\n\r", victim );
            send_to_char( "Okay\n\r", ch );
         return;
        }
    }

       /* Helmsman */
    else if ( !str_cmp( argument, "helm" ) )
    {
	if (IS_SET(victim->act2, PLR_LOOKOUT) )
	REMOVE_BIT(victim->act2, PLR_LOOKOUT);

        if ( IS_SET( victim->act2, PLR_HELM ) )
        {
            REMOVE_BIT( victim->act2, PLR_HELM );
            send_to_char("You no longer the helmsman.\n\r", victim );
            send_to_char("That person is no longer the helmsman.\n\r", ch
);
            return;
        }
        else
        {
            SET_BIT( victim->act2, PLR_HELM );
            send_to_char("You have been appointed helmsman by your
Captain\n\r", victim );
            send_to_char( "Okay\n\r", ch );
         return;
        }
    }

   else if ( !str_cmp( argument, "driver" ) )
    {

     if (vehicle->value[1] != 2 )
	{
	send_to_char(" You can only have drivers in Land Vehicles.\n\r",
ch );
	return;
	}

     if ( IS_SET( victim->act2, PLR_DRIVER ) )
        {
            REMOVE_BIT( victim->act2, PLR_DRIVER );
            send_to_char("You no longer the driver.\n\r", victim );
            send_to_char("That person is no longer the driver.\n\r", ch );
            return;
        }
        else
        {
            SET_BIT( victim->act2, PLR_DRIVER );
            send_to_char("You have been appointed Driver by your
Captain\n\r", victim );
            send_to_char( "Okay\n\r", ch );
         return;
        }
    }

       else if ( !str_cmp( argument, "pilot" ) )
    {
            
     if (vehicle->value[1] != 3 )
        {
        send_to_char(" You can only have pilots in Air Vehicles.\n\r", ch
);
        return;
        }
 /* We'll hack PLR_DRIVER since they do the same thing */
     if ( IS_SET( victim->act2, PLR_DRIVER ) )
        {
            REMOVE_BIT( victim->act2, PLR_DRIVER );
            send_to_char("You no longer the pilot.\n\r", victim );
            send_to_char("That person is no longer the pilot.\n\r", ch );
            return;
        }
        else
        {
            SET_BIT( victim->act2, PLR_DRIVER );
            send_to_char("You have been appointed pilot by your
Captain\n\r", victim );
            send_to_char( "Okay\n\r", ch );
         return;
        }
    }
return;
}

void do_sail(CHAR_DATA *ch, char *argument)
{
    OBJ_DATA   *vehicle;
    int door;
        
    vehicle = ch->in_room->inside_of;
            
    if(ch->in_room->inside_of == NULL)
    {
        send_to_char("You are not riding anything.\n\r",ch);
        return;
    }

 
if ( vehicle->value[1] != 1 )
{
   send_to_char( "That is not a Water Vehicle.\n\r", ch );
   return;
}
    

     if (!IS_SET(ch->act2, PLR_HELM ))
  {
    send_to_char("You aren't the helmsman!!!.\n\r", ch );
        return;
  }
        
   if(argument[0] == '\0')
    {
        send_to_char("Syntax: sail <direction>\n\r",ch);
        return;
    }
   
    door = find_hack_door(ch, argument);
 
    move_vehicle(ch, ch->in_room->inside_of, door);

    return;
} 

void do_fly(CHAR_DATA *ch, char *argument)
{
    OBJ_DATA   *vehicle;
    int door;
        
    vehicle = ch->in_room->inside_of;
            
    if(ch->in_room->inside_of == NULL)
    {
        send_to_char("You are not riding anything.\n\r",ch);
        return;
    }

 
if ( vehicle->value[1] != 3 )
{
   send_to_char( "That is not a Air Vehicle.\n\r", ch );
   return;
}
    

     if (!IS_SET(ch->act2, PLR_HELM ))
  {
    send_to_char("You aren't the pilot!!!.\n\r", ch );
        return;
  }
        
   if(argument[0] == '\0')
    {
        send_to_char("Syntax: fly <direction>\n\r",ch);
        return;
    }
   
    door = find_hack_door(ch, argument);
 
    move_vehicle(ch, ch->in_room->inside_of, door);

    return;
}  

void do_drive(CHAR_DATA *ch, char *argument)
{   
    OBJ_DATA   *vehicle;
    int door;
   
    vehicle = ch->in_room->inside_of;

    if(ch->in_room->inside_of == NULL)
    {
	send_to_char("You are not riding anything.\n\r",ch);
	return;
    }


if ( vehicle->value[1] != 2 )
{
   send_to_char( "That is not a Land Vehicle.\n\r", ch );
   return;
}
 

     if (!IS_SET(ch->act2, PLR_DRIVER ))
  {
    send_to_char("You aren't the driver!!!.\n\r", ch );
	return;
  }

   if(argument[0] == '\0')
    {
	send_to_char("Syntax: drive <direction>\n\r",ch);
	return;
    }

    door = find_hack_door(ch, argument);

    move_vehicle(ch, ch->in_room->inside_of, door);

    return;
}


int find_hack_door( CHAR_DATA *ch, char *arg )
{
    EXIT_DATA *pexit;
    int door;

         if ( !str_cmp( arg, "n" ) || !str_cmp( arg, "north" ) ) door = 0;
    else if ( !str_cmp( arg, "e" ) || !str_cmp( arg, "east"  ) ) door = 1;
    else if ( !str_cmp( arg, "s" ) || !str_cmp( arg, "south" ) ) door = 2;
    else if ( !str_cmp( arg, "w" ) || !str_cmp( arg, "west"  ) ) door = 3;
    else if ( !str_cmp( arg, "u" ) || !str_cmp( arg, "up"    ) ) door = 4;
    else if ( !str_cmp( arg, "d" ) || !str_cmp( arg, "down"  ) ) door = 5;
    else
    {
        for ( door = 0; door <= 5; door++ )
        {
            if ( ( pexit = ch->in_room->inside_of->in_room->exit[door] )
                && IS_SET( pexit->exit_info, EX_ISDOOR )
                && pexit->keyword
                && is_name( arg, pexit->keyword ) )
                return door;
        }
        act( "I see no $T here.", ch, NULL, arg, TO_CHAR );
        return -1;
    }

    if (( pexit = ch->in_room->inside_of->in_room->exit[door] ) == NULL )
    {
        act( "I see no door $T here.", ch, NULL, arg, TO_CHAR );
        return -1;
    }

/*
    if ( !IS_SET( pexit->exit_info, EX_ISDOOR ) )
    {
        send_to_char( "You can't do that.\n\r", ch );
        return -1;
    }
*/         
        
    return door;  
}

void do_load( CHAR_DATA *ch, char *argument )
{
   OBJ_DATA   *cball;   /* cannon ball */
   OBJ_DATA   *gpowder; /* gun powder  */
   OBJ_DATA   *vehicle;
   char      arg[ MAX_INPUT_LENGTH ];
/*   char	     buf[MSL]; */
  argument = one_argument( argument, arg );


   vehicle = ch->in_room->inside_of;

   if (!IS_SET(ch->act2, PLR_MANCANNON) && !IS_IMMORTAL(ch) )
 {
   send_to_char( "You aren't even manning the Cannons!\n\r", ch );
   return;
 }

  cball = get_eq_char( ch, WEAR_WIELD_2 );
      for ( cball = ch->carrying; cball; cball = cball->next_content )
    {
        if ( cball->pIndexData->vnum == OBJ_VNUM_CANNONBALL )
            break;
    }
    if ( !cball )
    {
        send_to_char( "You don't have cannon ball.\n\r", ch );
        return;
    }
    
 /* Cannonball present and accounted for, now for the gunpowder */

  for ( gpowder = ch->carrying; gpowder; gpowder = gpowder->next_content )
    {
     if (vehicle->value[6] >= 2 )
     {
     break;
     }
     if ( gpowder->pIndexData->vnum == OBJ_VNUM_GUNPOWDER )
           {
      break;
         }
    }
  
       if ( !gpowder )
    {
        send_to_char( "You don't have any gunpowder\n\r", ch );
        return;
    }
  
/* Kay it's loaded */
send_to_char( "You sucessfully load the cannon.\n\r", ch );
act( "$n loads the cannon.", ch, NULL, NULL, TO_ROOM );

extract_obj( cball );
extract_obj( gpowder );
vehicle->value[5] = 1;


return;

}


