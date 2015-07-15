/***************************************************************************
 *  Original Diku Mud copyright (C) 1990, 1991 by Sebastian Hammer,        *
 *  Michael Seifert, Hans Henrik St{rfeldt, Tom Madsen, and Katja Nyboe.   *
 *                                                                         *
 *  Merc Diku Mud improvments copyright (C) 1992, 1993 by Michael          *
 *  Chastain, Michael Quan, and Mitchell Tse.                              *
 *                                                                         *
 *  In order to use any part of this Merc Diku Mud, you must comply with   *
 *  both the original Diku license in 'license.doc' as well the Merc       *
 *  license in 'license.txt'.  In particular, you may not remove either of *
 *  these copyright notices.                                               *
 *                                                                         *
 *  Much time and thought has gone into this software and you are          *
 *  benefitting.  We hope that you share your changes too.  What goes      *
 *  around, comes around.                                                  *
 ***************************************************************************/

/***************************************************************************
*       ROM 2.4 is copyright 1993-1995 Russ Taylor                         *
*       ROM has been brought to you by the ROM consortium                  *
*           Russ Taylor (rtaylor@pacinfo.com)                              *
*           Gabrielle Taylor (gtaylor@pacinfo.com)                         *
*           Brian Moore (rom@rom.efn.org)                                  *
*       By using this code, you have agreed to follow the terms of the     *
*       ROM license, in the file Rom24/doc/rom.license                     *
***************************************************************************/

/***************************************************************************
*  Automated Quest code written by Vassago of MOONGATE, moongate.ams.com   *
*  4000. Copyright (c) 1996 Ryan Addams, All Rights Reserved. Use of this  * 
*  code is allowed provided you add a credit line to the effect of:        *
*  "Quest Code (c) 1996 Ryan Addams" to your logon screen with the rest    *
*  of the standard diku/rom credits. If you use this or a modified version *
*  of this code, let me know via email: moongate@moongate.ams.com. Further *
*  updates will be posted to the rom mailing list. If you'd like to get    *
*  the latest version of quest.c, please send a request to the above add-  *
*  ress. Quest Code v2.00.                                                 *
***************************************************************************/

#include <sys/types.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include "merc.h"

/* moved to merc.h - Decklarean
#define IS_QUESTOR( ch )    		( IS_SET( ( ch )->act, PLR_QUESTOR ) )
*/

/* Object vnums for object quest 'tokens'. These items are worthless and
   are type trash, as they are placed into the world when a player
   receives an object quest. */

#define QUEST_OBJQUEST1 2400     /* 2400 - 2405 Quest Objects */
#define QUEST_OBJQUEST2 2401     /* Nolocate, [QUEST] flag    */
#define QUEST_OBJQUEST3 2402
#define QUEST_OBJQUEST4 2403
#define QUEST_OBJQUEST5 2404
#define QUEST_OBJQUEST6 2405

/* Local functions */

void generate_quest	args( ( CHAR_DATA *ch, CHAR_DATA *questman ) );
void quest_update	args( ( void ) );
/*bool chance		args( ( int num ) ); */
ROOM_INDEX_DATA         *room;
int                     cnt  =  0;

/* CHANCE function. I use this everywhere in my code, very handy :> */

bool chance( int num )
{
    if ( number_range( 1,100 ) <= num ) return TRUE;
    else return FALSE;
}

/* The main quest function */
void do_quest( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA        *questman;
    OBJ_DATA         *obj=NULL, *obj_next;

/*  OBJ_INDEX_DATA   *questinfoobj;
    MOB_INDEX_DATA   *questinfo; */

    char buf         [MAX_STRING_LENGTH];
    char result      [MAX_STRING_LENGTH*2];
    char arg1        [MAX_INPUT_LENGTH];
    char arg2        [MAX_INPUT_LENGTH];
    char arg3        [MAX_INPUT_LENGTH];
    int amt = 0;

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );
    argument = one_argument( argument, arg3 );
    
    if ( arg3[0] != '\0' )
       amt = is_number( arg3 ) ? atoi( arg3 ) : 1;
    else
       amt = 1;

    if ( !strcmp( arg1, "info" ) )
    {
	if ( IS_SET( ch->act, PLR_QUESTOR ) )
	{
	    if ( ( !ch->questmob && !ch->questobj )
	    && ch->questgiver->short_descr != NULL )
	    {
		sprintf( buf, "&YYour quest is ALMOST complete!\n\rGet back to %s before your time runs out!\n\r",ch->questgiver->short_descr );
		send_to_char(AT_YELLOW, buf, ch);
	    }
	    else if ( ch->questobj )
	    {
	        sprintf( buf, "You are on a quest to find %s!\n\r",
			ch->questobj->short_descr );
	        send_to_char(AT_WHITE, buf, ch);
          	return;
	    }
	    else if ( ch->questmob )
	    {
                sprintf( buf, "You are on a quest to kill %s!\n\r",
		ch->questmob->short_descr );
		send_to_char( AT_WHITE, buf, ch );
		return;
	    }
	}
	else
                send_to_char( AT_WHITE,"You aren't currently on a quest.\n\r",ch );
	return;
    }
/* Quest area: info on area only for object quests */
    
    if ( !strcmp( arg1, "area" ) ) 
    {
       if( IS_SET( ch->act, PLR_QUESTOR ) )
       {
          if( ch->questobj )
          {
            sprintf( buf, "AREA: %s\n\r", room->area->name );
            send_to_char( AT_WHITE, buf, ch );
          }
        else send_to_char( AT_WHITE, "You aren't currently on an object quest.\n\r", ch );
        return;
       } 
        else send_to_char( AT_WHITE, "You aren't currently on a quest.\n\r", ch );
        return;
    }

    if ( !strcmp( arg1, "points" ) )
    {
	sprintf( buf, "You have %d quest points.\n\r",ch->questpoints );
	send_to_char( AT_WHITE, buf, ch );
	return;
    }
    else if ( !strcmp( arg1, "time" ) )
    {
	if ( !IS_SET( ch->act, PLR_QUESTOR ) )
	{
             send_to_char( AT_WHITE,"You aren't currently on a quest.\n\r",ch );

	    if ( ch->nextquest > 1 )
	    {
		sprintf( buf, "There are %d minutes remaining until you can go on another quest.\n\r",ch->nextquest );
		send_to_char( AT_WHITE, buf, ch );
	    }
	    else if ( ch->nextquest == 1 )
	    {
		sprintf( buf, "There is less than a minute remaining until you can go on another quest.\n\r" );
		send_to_char( AT_WHITE, buf, ch );
	    }
	}
        else if ( ch->countdown > 0 )
        {
	    sprintf( buf, "Time left for current quest: %d\n\r",ch->countdown );
	    send_to_char( AT_WHITE, buf, ch );
	}
	return;
    }

/* Checks for a Questmaster in the room */

   for ( questman = ch->in_room->people; questman; questman =questman->next_in_room )
   {
     if ( IS_NPC( questman ) && IS_SET( questman->act,ACT_QUESTMASTER ) ) 
        break; 
   }

   if ( !questman )
   {
        send_to_char( AT_WHITE, "You can't do that here.\n\r", ch );
        return;
   }
   
   if( questman->fighting != NULL )
   {
        send_to_char( AT_WHITE, "Wait until the fighting stops.\n\r", ch );
        return;
   }

/* Can have multiple questmasters, ch must report back to the one who gave quest */
   
   ch->questgiver = questman;

     
/* List displaying items one can buy with quest points */

    if ( !strcmp( arg1, "list" ) )
    {
        act( AT_WHITE, "$n asks $N for a list of quest items.", ch,NULL,questman, TO_ROOM ); 
	act( AT_WHITE, "You ask $N for a list of quest items.",ch,NULL,questman, TO_CHAR );
        sprintf( result, "&W[&R%5s %10s&W] [&R%30s&W]\n\r", "Lvl", "Points", "Item");
/*
        for ( cnt = 0; quest_table[cnt].name[0] != '\0'; cnt++ )
        {
            sprintf( buf, "[%5d %10d] [%*s]\n\r", 
                   quest_table[cnt].level,
                   quest_table[cnt].qp, 
                   30 + strlen( quest_table[cnt].name ) - strlen_wo_col( quest_table[cnt].name ),
    	           quest_table[cnt].name );
            strcat( result, buf );
        }        
        send_to_char( AT_WHITE, result, ch );*/
        return;
    }

    else if ( !strcmp( arg1, "buy" ) )
    {
	if ( arg2[0] == '\0' )
	{
	    send_to_char( AT_WHITE, "To buy an item, type 'QUEST BUY <item> <amount>'.\n\rAmount is optional.\n\r",ch );
	    return;
	}
/*
        for ( cnt = 0; quest_table[cnt].name[0] != '\0'; cnt++ )
        {
            if ( is_name( ch, arg2, quest_table[cnt].name ) )
            {
               if ( ch->questpoints >= quest_table[cnt].qp * amt )
               {
                  if ( quest_table[cnt].level <= ch->level )
                  {
                     ch->questpoints -= quest_table[cnt].qp * amt;
                     if ( is_name( ch, arg2, "prac pracs practice practices" ) )
                     {
                        ch->practice += amt;
                        if ( amt > 1 )
                        {                       
                            act( AT_WHITE, "$N gives some practice sessions to $n.", ch, NULL,questman, TO_ROOM );
                            act( AT_WHITE, "$N gives you some practice sessions.", ch, NULL, questman, TO_CHAR );   
                        }
                        else
                        {
                            act( AT_WHITE, "$N gives a practice session to $n.", ch, NULL, questman, TO_ROOM);
                            act( AT_WHITE, "$N gives you a practice session.", ch, NULL, questman, TO_CHAR );
                        }
                     }
                     else  
                     {   
                         while ( amt > 0 )                     
                         {
                            obj = create_object( get_obj_index(
				  quest_table[cnt].vnum ), quest_table[cnt].level );
                            act( AT_WHITE, "$N gives $p to $n.", ch, obj, questman, TO_ROOM );
                            act( AT_WHITE, "$N gives you $p.",   ch, obj, questman, TO_CHAR );
                            obj_to_char( obj, ch );         
                            amt--;
                         }
                     }
                  }
                  else
                  {
                     sprintf( buf, "Sorry, %s, but you are too inexperienced to use that item.\n\r", ch->name );
                     do_say( questman, buf );
                     return;
                  }
               }
               else
               {
                  sprintf( buf, "Sorry, %s, but you don't have enough quest points for that.\n\r", ch->name );
                  do_say( questman, buf );
                  return;
               }
               break;
            }
        }

        if ( ( obj == NULL ) && !( is_name( ch, arg2, "prac pracs practice practices" ) ) )
        {
           sprintf( buf, "I don't have that item, %s.\n\r", ch->name );
           do_say( questman, buf );
        }*/
	return;
    }
    else if ( !strcmp( arg1, "request" ) )
    {
        act( AT_WHITE, "$n asks $N for a quest.", ch, NULL, questman,TO_ROOM ); 
	act( AT_WHITE, "You ask $N for a quest.",ch, NULL, questman,TO_CHAR );
	if ( IS_SET( ch->act, PLR_QUESTOR ) )
	{
	    sprintf( buf, "But you're already on a quest!" );
	    do_say( questman, buf );
	    return;
	}
	if ( ch->nextquest > 0 )
	{
	    sprintf( buf, "You're very brave, %s, but let someone else have a chance.",ch->name );
	    do_say( questman, buf );
	    sprintf( buf, "Come back later." );
	    do_say( questman, buf );
	    return;
	}

	sprintf( buf, "Thank you, brave %s!",ch->name );
	do_say( questman, buf );

	generate_quest( ch, questman );

        if ( ch->questmob || ch->questobj )
	{
          if ( ch->questmob )
            ch->countdown = number_range( 10,30 ); /* time to complete quest */

/* Allow longer chance for an object quest */

          if ( ch->questobj )
            ch->countdown = number_range( 20,45 );

	    SET_BIT( ch->act, PLR_QUESTOR );
	    sprintf( buf, "You have %d minutes to complete this quest.",ch->countdown );
	    do_say( questman, buf );
	    sprintf( buf, "May the gods go with you!" );
	    do_say( questman, buf );
	}
	return;
    }
    else if ( !strcmp( arg1, "complete" ) )
    {
        act( AT_WHITE, "$n informs $N $e has completed $S quest.", ch,NULL,questman, TO_ROOM ); 
	act( AT_WHITE, "You inform $N you have completed $S quest.",ch,NULL, questman, TO_CHAR );

/* Check if ch returned to correct QuestMaster */
	if ( ch->questgiver != questman )
	{
	    sprintf( buf, "I never sent you on a quest! Perhaps you're thinking of someone else." );
	    do_say( questman,buf );
	    return;
	}

	if ( IS_SET( ch->act, PLR_QUESTOR ) )
	{
	    if ( ( !ch->questmob && !ch->questobj )
	    && ch->countdown > 0)
	    {
		int pointreward, pracreward;

	      /*reward = number_range(1500,25000);*/
		pointreward = number_range(10,30);

		sprintf(buf, "Congratulations on completing your quest!");
		do_say(questman,buf);
		sprintf(buf,"As a reward, I am giving you %d quest points!",pointreward);
		do_say(questman,buf);

	     /* 5% chance of getting between 1 and 6 practices :> */
		if (chance(5))
		{
		    pracreward = number_range(1,6);
		    sprintf(buf, "You gain %d practices!\n\r",pracreward);
		    send_to_char(AT_WHITE, buf, ch);
		    ch->practice += pracreward;
		}

	        REMOVE_BIT( ch->act, PLR_QUESTOR );
	        ch->questgiver = NULL;
	        ch->countdown = 0;
	        ch->questmob = NULL;
		ch->questobj = NULL;
	        ch->nextquest = 30;  /* 30 */
	   /*	ch->gold += reward;*/
		ch->questpoints += pointreward;

	        return;
	    }
/* Look to see if ch has quest obj in inventory */
	    else if ( ch->questobj && ch->countdown > 0)
	    {
		bool obj_found = FALSE;

    		for (obj = ch->carrying; obj != NULL; obj= obj_next)
    		{
        	    obj_next = obj->next_content;
	        
		    if ( obj == ch->questobj )
		    {
			obj_found = TRUE;
            	        break;
		    }
        	}
	  /* If ch returned without quest obj... */
		if ( !obj_found )
		{
		    sprintf(buf, "You haven't completed the quest yet, but there is still time!");
		    do_say(questman, buf);
		    return;
		}
		{
	        int pointreward, pracreward;

	 /* Ch receives between 10 and 30 qp for completing quest :> */
		pointreward = number_range(10,30);

	 /* Player doesn't keep quest object */
		act(AT_WHITE, "You hand $p to $N.",ch, obj, questman,TO_CHAR);
		act(AT_WHITE, "$n hands $p to $N.",ch, obj, questman,TO_ROOM);

	    	sprintf(buf, "Congratulations on completing your quest!");
		do_say(questman,buf);
		sprintf(buf,"As a reward, I am giving you %d quest points!",pointreward);
		do_say(questman,buf);

	 /* 5% chance to get pracs.. */
		if (chance(5))
		    {
		    pracreward = number_range(1,6);
		    sprintf(buf, "You gain %d practices!\n\r",pracreward);
		    send_to_char(AT_WHITE, buf, ch);
		    ch->practice += pracreward;
		    }

	        REMOVE_BIT( ch->act, PLR_QUESTOR );
	        ch->questgiver = NULL;
	        ch->countdown = 0;
	        ch->questmob = NULL;
		ch->questobj = NULL;
	        ch->nextquest = 30; /* 30 min till ch can quest again */
		ch->questpoints += pointreward;
		extract_obj(obj);
		return;
		}
	    }
/* Quest not complete, but still time left */
	    else if ( ( ch->questmob || ch->questobj ) 
		 && ch->countdown > 0 )
	    {
		sprintf(buf, "You haven't completed the quest yet, but there is still time!");
		do_say(questman, buf);
		return;
	    }
	}
	if ( ch->nextquest > 0)
	    sprintf(buf,"But you didn't complete your quest in time!");
	else 
	    sprintf(buf, "You have to REQUEST a quest first, %s.",ch->name);
	do_say(questman, buf);
	return;
    }

    if ( arg1[0] == '\0' )
    {   
      if ( IS_SET( ch->act, PLR_QUESTOR ) )
      {
         if ( ch->questobj )
         {
            sprintf( buf, "You are on a quest to find %s!\n\r", 
		     ch->questobj->short_descr );
            send_to_char(AT_WHITE, buf, ch);        
         }
         else if ( ch->questmob )
         {
              sprintf( buf, "You are on a quest to kill %s!\n\r",
		        ch->questmob->short_descr );
              send_to_char( AT_WHITE, buf, ch );       
         }
    
         sprintf( buf, "You have %d quest points.\n\r",ch->questpoints );
         send_to_char( AT_WHITE, buf, ch );
         if ( ch->countdown > 0 )
         {
            sprintf( buf, "Time left for current quest: %d\n\r", ch->countdown );
            send_to_char( AT_WHITE, buf, ch );
         }  
    }
    if ( ch->nextquest > 1 )
    {
       sprintf( buf, "There are %d minutes remaining until you can go on another quest.\n\r", ch->nextquest );
       send_to_char( AT_WHITE, buf, ch );
    }
    else if ( ch->nextquest == 1 )
    {
       sprintf( buf, "There is less than a minute remaining until you can go on another quest.\n\r" );
       send_to_char( AT_WHITE, buf, ch );
    }        
   
    send_to_char(AT_RED, "QUEST commands: POINTS INFO TIME REQUEST COMPLETE LIST BUY AREA.\n\r",ch);
    send_to_char(AT_WHITE, "For more information, type 'HELP AUTOQUEST'.\n\r",ch);
    return;
 }
}

void generate_quest(CHAR_DATA *ch, CHAR_DATA *questman)
{
    CHAR_DATA *vsearch, *vsearch_next;
    CHAR_DATA *victim = NULL;
  /*  ROOM_INDEX_DATA *room;*/
    OBJ_DATA *questitem;
    char buf [MAX_STRING_LENGTH];
    long mcounter;
    int level_diff;

    /*  Randomly selects a mob from the world mob list. If you don't
	want a mob to be selected, make sure it is immune to summon.
	Or, you could add a new mob flag called ACT_NOQUEST. The mob
	is selected for both mob and obj quests, even tho in the obj
	quest the mob is not used. This is done to assure the level
	of difficulty for the area isn't too great for the player. */

	mcounter = 0;
        for ( vsearch = char_list; vsearch; vsearch = vsearch_next )
	    {
	    vsearch_next = vsearch->next;

/*	    if ( IS_NPC( vsearch )
	    && vsearch->pIndexData->vnum == 7 ) 
		continue;  */

	    if ( vsearch->deleted )
		continue;

	    level_diff = ch->level - vsearch->level;

	    if ( (level_diff <= 3 && level_diff > -5 )
		&& ( IS_NPC( vsearch ) )
                && ( !IS_SET( vsearch->in_room->area->area_flags, AREA_PROTOTYPE ) )
		&& ( !IS_SET( vsearch->in_room->area->area_flags, AREA_NO_QUEST ) )
		&& ( vsearch->pIndexData->pShop == NULL )
		&& ( vsearch->pIndexData->vnum != 1351 ) /* Ravenwood guard */
                && ( vsearch->pIndexData->vnum != 1350 ) /* Ravenwood guard */
/*    	        && ( vsearch->pIndexData->vnum != 7 )     Supermob */
		&& ( ch->level <= vsearch->in_room->area->ulevel )
		&& ( ch->level > vsearch->in_room->area->llevel )
     	        && ( !IS_SET(vsearch->act,ACT_TRAIN) )
    		&& ( !IS_SET(vsearch->act,ACT_PRACTICE) )
		&& ( !IS_SET( vsearch->in_room->room_flags, ROOM_SAFE ) )
		&& ( !IS_SET( vsearch->in_room->room_flags, ROOM_NO_OFFENSIVE) ) )
/*		&& ( !IS_SET( vsearch->in_room->area->area_flags, AREA_PROTOTYPE ) ) */
		{
		   if ( number_range( 0, mcounter) == 0 )
			{
/* 
sprintf( buf, "%s&W->&X%d&W->&X%s&W->&XHQ&W[&X%c&W]\n\r", 
vsearch->name, level_diff, vsearch->in_room->area->name,
( IS_SET( vsearch->in_room->area->area_flags, AREA_NO_QUEST ) ) ? 'Y' : 'N' );
	    send_to_char( AT_DGREY, buf, ch ); 
*/
			victim = vsearch;
			mcounter++;
			}
		}		   
	}
   
    if ( !victim )
    {	
	sprintf(buf, "I'm sorry, but I don't have any quests for you at this time.");
	do_say(questman, buf);
	sprintf(buf, "Try again later.");
	do_say(questman, buf);
	ch->nextquest = 10; /* 10 min until ch can quest again */
        return;
    }

    if ( ( room = find_location( ch, victim->name ) ) == NULL )
    {
	sprintf(buf, "I'm sorry, but I don't have any quests for you at this time.");
	do_say(questman, buf);
	sprintf(buf, "Try again later.");
	do_say(questman, buf);
	ch->nextquest = 10; /* 10 min until ch can quest again */
        return;
    }

/* Player has a quest, turn all channels off except shout and yell */

/*      40% chance it will send the player on a 'recover item' quest. */

    if (chance(40))
    {
	int objvnum = 0;

	switch(number_range(0,4))
	{
	    case 0:
	    objvnum = QUEST_OBJQUEST1;
	    break;

	    case 1:
	    objvnum = QUEST_OBJQUEST2;
	    break;

	    case 2:
	    objvnum = QUEST_OBJQUEST3;
	    break;

	    case 3:
	    objvnum = QUEST_OBJQUEST4;
	    break;

	    case 4:
	    objvnum = QUEST_OBJQUEST5;
	    break;
	}

        questitem = create_object( get_obj_index(objvnum), ch->level );
/*	obj_to_char(questitem, victim); */

/* Place quest obj in room of victim on ground */
        obj_to_room(questitem, room);
	ch->questobj = questitem;

	sprintf(buf, "Vile pilferers have stolen %s from the royal treasury!",questitem->short_descr);
	do_say(questman, buf);
/*	do_say(questman, "My court wizardess, with her magic mirror, has pinpointed its location."); */

	/* I changed my area names so that they have just the name of the area
	   and none of the level stuff. You may want to comment these next two
	   lines. - Vassago */

/*	sprintf(buf, "Look in the general area of %s...\n\r", room->area->name);
	do_say(questman, buf);*/ 
	return;
    }

    /* Quest to kill a mob */

    else 
    {
    switch(number_range(0,1))
    {
	case 0:
        sprintf(buf, "An enemy of mine, %s, is making vile threats against the Oracle!",victim->short_descr);
        do_say(questman, buf);
        sprintf(buf, "This threat must be eliminated!");
        do_say(questman, buf);
	break;

	case 1:
	sprintf(buf, "PaP's most heinous criminal, %s, has escaped from the dungeon!",victim->short_descr);
	do_say(questman, buf);
	sprintf(buf, "Since the escape, %s has murdered %d civillians!",victim->short_descr, number_range(2,20));
	do_say(questman, buf);
	do_say(questman,"The penalty for this crime is death, and you are to deliver the sentence!");
	break;
    }

  if (room->name != NULL)
    {
/*        sprintf(buf, "Seek %s out somewhere in the vicinity of %s!",victim->short_descr,room->name);
         do_say(questman, buf);*/

	/* I changed my area names so that they have just the name of the area
	   and none of the level stuff. You may want to comment these next two
	   lines. - Vassago */

/*	sprintf(buf, "That location is in the general area of %s...\n\r", room->area->name );
	do_say(questman, buf);*/
    }

    ch->questmob = victim;
    }
    return;
}

/* Called from update_handler() by pulse_area */

void quest_update(void)
{
    CHAR_DATA *ch, *ch_next;

    for ( ch = char_list; ch; ch = ch_next )
    {
        ch_next = ch->next;

	if ( IS_NPC( ch ) ) continue;

	if ( ch->nextquest > 0 )
	{
	    ch->nextquest--;

	    if ( ch->nextquest == 0 )
	    {
	        send_to_char(AT_WHITE, "You may now quest again.\n\r",ch);
	        return;
	    }
	}
        else if (IS_SET(ch->act,PLR_QUESTOR ))
        {
	    if ( ch->questmob 
	    && ch->questmob->deleted 
	    && ch->countdown > 1 )
		{
		char buf[ MAX_INPUT_LENGTH ];
		sprintf( buf, 
		"%s tell you 'News has it that %s has been eliminated.'\n\r",
		ch->questgiver->short_descr, ch->questmob->short_descr );
		send_to_char( AT_WHITE, buf, ch );
		sprintf( buf,
		"%s tells you 'Thank you for attempting to kill %s.'\n\r",
		ch->questgiver->short_descr, ch->questmob->short_descr );
		send_to_char( AT_WHITE, buf, ch );
		ch->reply = ch->questgiver;
	        REMOVE_BIT(ch->act, PLR_QUESTOR );
		ch->questgiver = NULL;
		ch->questmob = NULL;
		ch->countdown = 0;
		ch->nextquest = 15; /* 15 min until ch can quest again */
		return;
		}
	    if (--ch->countdown <= 0)
	    {
    	        char buf [MAX_STRING_LENGTH];

	        ch->nextquest = 30; /* 30 min until ch can quest again */
	        sprintf(buf, "You have run out of time for your quest!\n\rYou may quest again in %d minutes.\n\r",ch->nextquest);
	        send_to_char(AT_WHITE, buf, ch);
	        REMOVE_BIT(ch->act, PLR_QUESTOR );
		if ( ch->questobj )
			ch->questobj->timer = 1; /* have obj_update
						  * extract it.
						  * -- Hannibal */
                ch->questgiver = NULL;
                ch->countdown = 0;
                ch->questmob = NULL;
		ch->questobj = NULL;
	    }
	    if (ch->countdown > 0 && ch->countdown < 6)
	    {
	        send_to_char(AT_WHITE, "Better hurry, you're almost out of time for your quest!\n\r",ch);
	        return;
	    }
        }
    }
    return;
}
