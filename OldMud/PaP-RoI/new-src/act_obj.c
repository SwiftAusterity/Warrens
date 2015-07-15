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
void show_list_to_char( OBJ_DATA *list, CHAR_DATA *ch, bool fShort, 
		        bool fShowNothing );


/*
 * Local functions.
 */
#define CD CHAR_DATA
void	get		args( ( CHAR_DATA *ch, char *arg ) );
bool	get_obj		args( ( CHAR_DATA *ch, OBJ_DATA *obj,
			       OBJ_DATA *container, bool palming, bool getall ) );
void	wear_obj	args( ( CHAR_DATA *ch, OBJ_DATA *obj,
			       bool fReplace ) );
void  activate_switch args( ( CHAR_DATA *ch, OBJ_DATA *obj ) );
CD *	find_keeper	args( ( CHAR_DATA *ch ) );
CD *	find_artist	args( ( CHAR_DATA *ch ) );
CD *	find_dealer	args( ( CHAR_DATA *ch ) );
CD *	find_artifactor	args( ( CHAR_DATA *ch ) );
CD *	find_o_surgeon	args( ( CHAR_DATA *ch ) );
CD *	find_l_surgeon	args( ( CHAR_DATA *ch ) );
CD *	find_smith	args( ( CHAR_DATA *ch ) );
MONEY_DATA *get_cost    args( ( CHAR_DATA *keeper, OBJ_DATA *obj, bool fBuy ) );
void    do_acoload      args( ( CHAR_DATA *ch, OBJ_DATA *obj, int vnum ) );
void    do_acmload      args( ( CHAR_DATA *ch, OBJ_DATA *obj, int vnum ) );
void    do_actrans      args( ( CHAR_DATA *ch, OBJ_DATA *obj, int vnum ) );
void    do_acmorph      args( ( CHAR_DATA *ch, OBJ_DATA *obj, int vnum ) );
#undef	CD

bool get_obj( CHAR_DATA *ch, OBJ_DATA *obj, OBJ_DATA *container, bool palming, bool getall )
{
  MONEY_DATA amount;

    if ( !CAN_WEAR( obj, ITEM_TAKE ) && obj->item_type != ITEM_CORPSE_PC )
    {
     if ( !getall )
	send_to_char(AT_WHITE, "You can't take that.\n\r", ch );
	oprog_get_trigger( obj, ch ); /* So items w/o take flag can 
				         still have obj progs */
	return 0;
    }

    if ( obj->item_type == ITEM_CORPSE_PC && get_trust(ch) < LEVEL_IMMORTAL )
    {
	send_to_char(AT_WHITE, "You can't take that.\n\r", ch );
	return 0;
    }

    if ( obj->item_type == ITEM_BOMB && obj->timer > -1 )
          {
           bomb_explode( obj, obj->in_room );
           return 0;    
          }

    if ( ch->carry_number + get_obj_number( obj ) > can_carry_n( ch ) )
    {
	act(AT_WHITE, "$d: you can't carry that many items.",
	    ch, NULL, obj->name, TO_CHAR );
	return 0;
    }

    if ( ch->carry_weight + get_obj_weight( obj ) > can_carry_w( ch ) )
    {
	act(AT_WHITE, "$d: you can't carry that much weight.",
	    ch, NULL, obj->name, TO_CHAR );
	return 0;
    }

    if ( container )
    {   if (!palming)
	{	
	   act(AT_GREEN, "You get $p from $P.", ch, obj, container, TO_CHAR );
	   act(AT_GREEN, "$n gets $p from $P.", ch, obj, container, TO_ROOM );
        }
    	if ( palming == 1 )
	   act(AT_GREEN, "You palm $p from $P.", ch, obj, container, TO_CHAR );
	oprog_get_from_trigger( obj, ch, container );
	obj_from_obj( obj, FALSE );
        if ( ch->in_room->vnum == ROOM_HUMAN_DONATION
         ||  ch->in_room->vnum == ROOM_ELF_DONATION
         || obj->item_type == ITEM_CORPSE_PC )
         obj->timer = -1;
    }
    else
    {
	if ( !palming ) 
	{
	   act(AT_GREEN, "You get $p.", ch, obj, container, TO_CHAR );
	   act(AT_GREEN, "$n gets $p.", ch, obj, container, TO_ROOM );
	}
	if ( palming == 1 ) 
	   act(AT_GREEN, "You palm $p.", ch, obj, container, TO_CHAR );
	oprog_get_trigger( obj, ch );
	obj_from_room( obj );
        if ( ch->in_room->vnum == ROOM_HUMAN_DONATION
         ||  ch->in_room->vnum == ROOM_ELF_DONATION
          || obj->item_type == ITEM_CORPSE_PC )
         obj->timer = -1;
    }

    if ( obj->item_type == ITEM_MONEY )
    {
        char buf [ MAX_STRING_LENGTH ];
	amount.gold   = obj->value[0];
	amount.silver = obj->value[1];
	amount.copper = obj->value[2];
	
	add_money( &ch->money, &amount );
	sprintf( buf, "You counted %s\n\r", money_string( &amount ) );
	send_to_char( AT_YELLOW, buf, ch );
	if ( IS_SET( ch->act, PLR_AUTOSPLIT ) && ( amount.gold +
	     amount.silver + amount.copper > 1 ) )
	{
	  if ( amount.gold > 1 )
	  {
	    sprintf( buf, "%d gold", amount.gold );
	    do_split( ch, buf );
	  }
	  if ( amount.silver > 1 )
	  {
	    sprintf( buf, "%d silver", amount.silver );
	    do_split( ch, buf );
	  }
	  if ( amount.copper > 1 )
	  {
	    sprintf( buf, "%d copper", amount.copper );
	    do_split( ch, buf );
	  }
	}
	extract_obj( obj );
	return 0;
    }
    else
    {
	obj_to_char( obj, ch );
        if ( ch->in_room->vnum == ROOM_HUMAN_DONATION
         ||  ch->in_room->vnum == ROOM_ELF_DONATION
          || obj->item_type == ITEM_CORPSE_PC )
         obj->timer = -1;
    }

    return 1;
}

void do_get( CHAR_DATA *ch, char *argument )
{
    get( ch, argument );
    return;
}


void get( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    OBJ_DATA *container;
    bool      palming = FALSE;
    char      arg1 [ MAX_INPUT_LENGTH ];
    char      arg2 [ MAX_INPUT_LENGTH ];
    char      buf [ MAX_STRING_LENGTH ];
    bool      found;
    int	      ObjCount=0;
    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( IS_NPC( ch ) )
    {
     if ( IS_SET( ch->act, ACT_ILLUSION ) )
      return;
    }
    else
    {
     if ( IS_SET( ch->act, PLR_ILLUSION ) )
     {
      send_to_char( AT_YELLOW, "Illusions can not pick stuff up.\n\r", ch );
      return;
     }
    }

   if ( get_curr_dex( ch ) > 22 && !IS_NPC(ch) )
   {
    if ( ch->race == RACE_KENDER ) 
     palming = TRUE;
    else
    {
     if ( number_percent( ) > ch->pcdata->learned[gsn_palm] )
      palming = FALSE;
     else
     {
      palming = TRUE;
      update_skpell( ch, gsn_palm );
     }
    }
   }
   else 
    palming = FALSE;

    /* Get type. */
    if ( arg1[0] == '\0' )
    {
	if ( palming )
	send_to_char(AT_WHITE, "Palm what?\n\r", ch );
	else
	send_to_char(AT_WHITE, "Get what?\n\r", ch );
	return;
    }

    if ( arg2[0] == '\0' )
    {
	if ( str_cmp( arg1, "all" ) && str_prefix( "all.", arg1 ) )
	{
	    /* 'get obj' */
	    obj = get_obj_list( ch, arg1, ch->in_room->contents );
	    if ( !obj )
	    {
		act(AT_WHITE, "I see no $T here.", ch, NULL, arg1, TO_CHAR );
		return;
	    }

	    get_obj( ch, obj, NULL, palming, FALSE );
	}
	else
	{
	    /* 'get all' or 'get all.obj' */
            OBJ_DATA *obj_next;
	    found = FALSE;
	    for ( obj = ch->in_room->contents; obj; obj = obj_next )
	    {
	        obj_next = obj->next_content; 

		if ( ( arg1[3] == '\0' || is_name(ch, &arg1[4], obj->name ) )
		    && can_see_obj( ch, obj ) )
		{
		    found = TRUE;

    		    ObjCount+=get_obj( ch, obj, NULL, 2, TRUE);
		   
		    if ((!obj_next || obj->pIndexData->vnum != 
			obj_next->pIndexData->vnum)&& ObjCount)
		    {
			sprintf(buf,"You get %d $p%s.",
			ObjCount, ObjCount > 1 ? "s" : "" );
			act(AT_WHITE, buf,ch,obj,NULL,TO_CHAR);
			sprintf(buf,"$n gets %d $p%s.",
			ObjCount, ObjCount > 1 ? "s" : "" );
			act(AT_WHITE, buf,ch,obj,NULL,TO_ROOM);
		    	ObjCount=0;
		    }
		    
		}
	    }
	    
	    if ( !found ) 
	    {
		if ( arg1[3] == '\0' )
		    send_to_char(AT_WHITE, "I see nothing here.\n\r", ch );
		else
		    act(AT_WHITE, "I see no $T here.", ch, NULL, &arg1[4], TO_CHAR );
	    }
		
	}
    }
    else
    {
	/* 'get ... container' */
	if ( !str_cmp( arg2, "all" ) || !str_prefix( "all.", arg2 ) )
	{
	    send_to_char(AT_WHITE, "You can't do that.\n\r", ch );
	    return;
	}

	if ( !( container = get_obj_here( ch, arg2 ) ) )
	{
	    act(AT_WHITE, "I see no $T here.", ch, NULL, arg2, TO_CHAR );
	    return;
	}

	switch ( container->item_type )
	{
	default:
	    send_to_char(AT_WHITE, "That's not a container.\n\r", ch );
	    return;

        case ITEM_FURNITURE:
         if ( container->value[1] == FURNITURE_DESK ||
              container->value[1] == FURNITURE_ARMOIR )
          break;
         else
         {
	    send_to_char(AT_WHITE, "That's not a container.\n\r", ch );
	    return;
         }

        case ITEM_ARMOR:
         if ( container->value[1] == TRUE )
          break;
         else
         {
	    send_to_char(AT_WHITE, "That has no pockets.\n\r", ch );
	    return;
         }

	case ITEM_CONTAINER:
	case ITEM_CORPSE_NPC:
	    break;

	case ITEM_CORPSE_PC:
	    {
		/*CHAR_DATA *gch;*/
		char      *pd;
		char       name[ MAX_INPUT_LENGTH ];

		if ( IS_NPC( ch ) )
		{
		    send_to_char(AT_WHITE, "You can't do that.\n\r", ch );
		    return;
		}

		pd = container->short_descr;
		pd = one_argument( pd, name );

		/*if ( str_cmp( name, ch->name ) && !IS_IMMORTAL( ch ) )
		{
		    bool fGroup;

		    fGroup = FALSE;
		    for ( gch = char_list; gch; gch = gch->next )
		    {
			if ( !IS_NPC( gch )
			    && is_same_group( ch, gch )
			    && !str_cmp( name, gch->name ) )
			{
			    fGroup = TRUE;
			    break;
			}
		    }
                if ( ch->clan != 0 )
                   fGroup = TRUE;

                    if ( !str_cmp( name, ch->name ) )
                    fGroup = TRUE;
                   
		    if ( !fGroup )
		    {
			send_to_char(AT_WHITE, "You can't do that.\n\r", ch );
			return;
		    }
		}*/
	    }
	}

	if ( IS_SET( container->value[1], CONT_CLOSED ) 
	    && container->item_type != ITEM_FURNITURE )
	{
	    act(AT_GREEN, "The $d is closed.", ch, NULL, container->name, TO_CHAR );
	    return;
	}

	if ( str_cmp( arg1, "all" ) && str_prefix( "all.", arg1 ) )
	{
	    /* 'get obj container' */
	    obj = get_obj_list( ch, arg1, container->contains );
	    if ( !obj )
	    {
		act(AT_GREEN, "I see nothing like that in the $T.",
		    ch, NULL, arg2, TO_CHAR );
		return;
	    }
	    get_obj( ch, obj, container, palming, FALSE );
	}
	else
	{
	    /* 'get all container' or 'get all.obj container' */
            OBJ_DATA *obj_next;
	    found = FALSE;
	    for ( obj = container->contains; obj; obj = obj_next )
	    {
                obj_next = obj->next_content;

		if ( ( arg1[3] == '\0' || is_name(ch, &arg1[4], obj->name ) )
		    && can_see_obj( ch, obj ) )
		{
		    found = TRUE;
		    ObjCount+=get_obj( ch, obj, container, 2, TRUE);
		   
		    if ((!obj_next || obj->pIndexData->vnum !=
			obj_next->pIndexData->vnum)&& ObjCount)
		    {
			sprintf(buf,"You get %d $p%s from $P.",
			ObjCount, ObjCount > 1 ? "s" : "" );
			act(AT_WHITE, buf,ch,obj,container,TO_CHAR);
			sprintf(buf,"$n gets %d $p%s from $P.",
			ObjCount, ObjCount > 1 ? "s" : "" );
			act(AT_WHITE, buf,ch,obj,container,TO_ROOM);

		    	ObjCount=0;
		    }
		    
		}
	    }

	    if ( !found )
	    {
		if ( arg1[3] == '\0' )
		    act(AT_GREEN, "I see nothing in the $T.",
			ch, NULL, arg2, TO_CHAR );
		else
		    act(AT_GREEN, "I see nothing like that in the $T.",
			ch, NULL, arg2, TO_CHAR );
	    }
	}
    }

    return;
}

void do_put( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *container;
    OBJ_DATA *obj;
    char      arg1 [ MAX_INPUT_LENGTH ];
    char      arg2 [ MAX_INPUT_LENGTH ];
    char      buf [ MAX_STRING_LENGTH ];
    int	      ObjCount=0;
    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );
    if ( arg1[0] == '\0' || arg2[0] == '\0' )
    {
	send_to_char(AT_DGREEN, "Put what in what?\n\r", ch );
	return;
    }

    if ( !str_cmp( arg2, "all" ) || !str_prefix( "all.", arg2 ) )
    {
	send_to_char(AT_DGREEN, "You can't do that.\n\r", ch );
	return;
    }

    if ( !( container = get_obj_here( ch, arg2 ) ) )
    {
	act(AT_DGREEN, "I see no $T here.", ch, NULL, arg2, TO_CHAR );
	return;
    }

    if ( container->item_type != ITEM_CONTAINER )
    {
     if ( container->item_type == ITEM_FURNITURE )
     {
      if ( container->value[1] != FURNITURE_DESK &&
           container->value[1] != FURNITURE_ARMOIR )
      {
	send_to_char(AT_DGREEN, "That's not a container.\n\r", ch );
	return;
      }
     }
     else if ( container->item_type == ITEM_ARMOR )
     {
      if ( container->value[1] != TRUE )
      {
	send_to_char(AT_DGREEN, "That's not a container.\n\r", ch );
	return;
      }
     }
     else
     {
	send_to_char(AT_DGREEN, "That's not a container.\n\r", ch );
	return;
     }
    }

    if ( container->durability <= 0 )
    {
     send_to_char(AT_DGREEN, "That container is broken.\n\r", ch );
     return;
    }
   

    if ( IS_SET( container->value[1], CONT_CLOSED )
       && container->item_type != ITEM_FURNITURE )
    {
	act(AT_DGREEN, "The $d is closed.", ch, NULL, container->name, TO_CHAR );
	return;
    }

    if ( str_cmp( arg1, "all" ) && str_prefix( "all.", arg1 ) )
    {
	/* 'put obj container' */
	if ( !( obj = get_obj_carry( ch, arg1 ) ) )
	{
	    send_to_char(AT_DGREEN, "You do not have that item.\n\r", ch );
	    return;
	}

	if ( obj == container )
	{
	    send_to_char(AT_DGREEN, "You can't fold it into itself.\n\r", ch );
	    return;
	}

	if ( !can_drop_obj( ch, obj ) )
	{
	    send_to_char(AT_DGREEN, "You can't let go of it.\n\r", ch );
	    return;
	}

       if ( container->item_type == ITEM_CONTAINER )
       {
	if ( get_obj_weight( obj ) + get_obj_weight( container )
	     > container->value[0] )
	{
	    send_to_char(AT_DGREEN, "It won't fit.\n\r", ch );
	    return;
	}
       }
       else if ( container->item_type == ITEM_FURNITURE )
       {
        if ( container->value[1] == FURNITURE_DESK &&
            get_obj_weight( obj ) > 2 )
	{
	    send_to_char(AT_DGREEN, "It won't fit.\n\r", ch );
	    return;
	}
        else if ( container->value[1] == FURNITURE_ARMOIR &&
                  get_obj_weight( obj ) > 50 )

	{
	    send_to_char(AT_DGREEN, "It won't fit.\n\r", ch );
	    return;
	}
       }
       else if ( container->item_type == ITEM_ARMOR )
       {
	if ( get_obj_weight( obj ) > container->value[2] )
	{
	    send_to_char(AT_DGREEN, "It won't fit.\n\r", ch );
	    return;
	}
       }
        
       
	obj_from_char( obj );
	obj_to_obj( obj, container );
	act(AT_GREEN, "You put $p in $P.", ch, obj, container, TO_CHAR );
	act(AT_GREEN, "$n puts $p in $P.", ch, obj, container, TO_ROOM );
	oprog_put_trigger( obj, ch, container );
    }
    else
    {
	/* 'put all container' or 'put all.obj container' */
        OBJ_DATA *obj_next;

	for ( obj = ch->carrying; obj; obj = obj_next )
	{
            obj_next = obj->next_content;

	    if ( ( arg1[3] == '\0' || is_name(ch, &arg1[4], obj->name ) )
		&& can_see_obj( ch, obj )
		&& IS_SET( obj->wear_loc, WEAR_NONE )
		&& obj != container
		&& can_drop_obj( ch, obj )
		&& get_obj_weight( obj ) + get_obj_weight( container )
		   <= container->value[0] )
	    {
		obj_from_char( obj );
		obj_to_obj( obj, container );
		oprog_put_trigger( obj, ch, container );
		ObjCount++;

		if (!obj_next || obj->pIndexData->vnum !=
    		obj_next->pIndexData->vnum)
		{
    		   sprintf(buf,"You put %d $ps in $P.",ObjCount);
    		   act(AT_WHITE, buf,ch,obj,container,TO_CHAR);
    		   sprintf(buf,"$n puts %d $ps in $P.",ObjCount);
    		   act(AT_WHITE, buf,ch,obj,container,TO_ROOM);
    		   ObjCount=0;
		}
		
	    }
	}
    }

    return;
}



void do_drop( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    char      arg2 [ MAX_INPUT_LENGTH ];
    char      buf [ MAX_STRING_LENGTH ];
    bool      found;
    int	      ObjCount=0;
    argument = one_argument( argument, arg );
    argument = one_argument( argument, arg2 );

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_DGREEN, "Drop what?\n\r", ch );
	return;
    }

    if ( is_number( arg ) )
    {
	/* 'drop NNNN coins' */
        OBJ_DATA *obj_next;

	MONEY_DATA *howmuch;
	int amount;

/* drop <amount> <currency type> */

	amount  = atoi( arg );
	howmuch = take_money( ch, amount, arg2, "drop" );

	if ( !howmuch )
	{
	   sprintf( buf, "You don't have that much %s.\n\r", arg2 );
	   send_to_char( AT_WHITE, buf, ch );
	   return;
	}

        for ( obj = ch->in_room->contents; obj; obj = obj_next )
        {
            obj_next = obj->next_content;

            if ( obj->deleted )
                continue;

            switch ( obj->pIndexData->vnum )
            {
            case OBJ_VNUM_MONEY_ONE:
		if ( obj->value[0] == 1 )
		   howmuch->gold   += 1;
		if ( obj->value[1] == 1 )
		   howmuch->silver += 1;
		if ( obj->value[2] == 1 )
		   howmuch->copper += 1;
		extract_obj( obj );
		break;
  	    case OBJ_VNUM_MONEY_SOME:
                howmuch->gold   += obj->value[0];
		howmuch->silver += obj->value[1];
		howmuch->copper += obj->value[2];
                extract_obj( obj );
                break;
            }
        }             

/* Change the parameters of create_money to MONEY_DATA *amount */
        obj_to_room( create_money( howmuch ), ch->in_room );
        send_to_char(AT_YELLOW, "OK.\n\r", ch );
        act(AT_YELLOW, "$n drops some coins.", ch, NULL, NULL, TO_ROOM );
        return;
    }                

    if ( str_cmp( arg, "all" ) && str_prefix( "all.", arg ) )
    {
	/* 'drop obj' */
	if ( !( obj = get_obj_carry( ch, arg ) ) )
	{
	    send_to_char(AT_DGREEN, "You do not have that item.\n\r", ch );	    return;
	}

	if ( !can_drop_obj( ch, obj ) )
	{
	    send_to_char(AT_DGREEN, "You can't let go of it.\n\r", ch );
	    return;
	}

	obj_from_char( obj );
        if ( ch->in_room->vnum == ROOM_HUMAN_DONATION
         ||  ch->in_room->vnum == ROOM_ELF_DONATION
	  || obj->item_type == ITEM_CORPSE_PC )
	 obj->timer = 30;
	obj_to_room( obj, ch->in_room );
	act(AT_GREEN, "You drop $p.", ch, obj, NULL, TO_CHAR );
	act(AT_GREEN, "$n drops $p.", ch, obj, NULL, TO_ROOM );
	oprog_drop_trigger( obj, ch );
    }
    else
    {
	/* 'drop all' or 'drop all.obj' */
	OBJ_DATA *obj_next;

	found = FALSE;
	for ( obj = ch->carrying; obj; obj = obj_next )
	{
            obj_next = obj->next_content;

	    if ( ( arg[3] == '\0' || is_name(ch, &arg[4], obj->name ) )
		&& can_see_obj( ch, obj )
		&& IS_SET( obj->wear_loc, WEAR_NONE )
		&& can_drop_obj( ch, obj ) )
	    {
		found = TRUE;
		obj_from_char( obj );
                if ( ch->in_room->vnum == ROOM_HUMAN_DONATION
                 ||  ch->in_room->vnum == ROOM_ELF_DONATION
	          || obj->item_type == ITEM_CORPSE_PC )
	         obj->timer = 20;

		obj_to_room( obj, ch->in_room );
		oprog_drop_trigger( obj, ch );
		ObjCount++;

		if (!obj_next || obj->pIndexData->vnum !=
    		   obj_next->pIndexData->vnum)
		{
    		   sprintf(buf,"You drop %d $ps.",ObjCount);
    		   act(AT_WHITE, buf, ch, obj, NULL, TO_CHAR);
    		   sprintf(buf,"$n drops %d $ps.", ObjCount);
    		   act(AT_WHITE, buf, ch, obj, NULL, TO_ROOM);
    		   ObjCount=0;
}

	    }
	}

	if ( !found )
	{
	    if ( arg[3] == '\0' )
	        send_to_char(AT_DGREEN, "You are not carrying anything.", ch );
	    else
		act(AT_DGREEN, "You are not carrying any $T.",
		    ch, NULL, &arg[4], TO_CHAR );
	}
    }

    return;
}



void do_give( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    OBJ_DATA  *obj;
    char       buf [ MAX_STRING_LENGTH ];
    char       arg1 [ MAX_INPUT_LENGTH ];
    char       arg2 [ MAX_INPUT_LENGTH ];
    MONEY_DATA *howmuch;
    char       arg3 [ MAX_INPUT_LENGTH ];
    int        amount;
    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( arg1[0] == '\0' || arg2[0] == '\0' )
    {
	send_to_char(AT_DGREEN, "Give what to whom?\n\r", ch );
	return;
    }

    if ( is_number( arg1 ) )
    {
	/* 'give NNNN coins victim' */
	/* 'give NNNN type victim' */

	amount = atoi( arg1 );
 	argument = one_argument( argument, arg3 );

        if ( arg3[0] == '\0' )
        {
            send_to_char(AT_DGREEN, "Give what to whom?\n\r", ch );
            return;
        }

        if ( !( victim = get_char_room( ch, arg3 ) ) )
        {
            send_to_char(AT_DGREEN, "They aren't here.\n\r", ch );
            return;
        }      

       if ( IS_NPC(victim) )
       {
        if ( IS_SET(victim->act, ACT_ILLUSION ) )
        {
         send_to_char( AT_YELLOW, "You can not give stuff to illusions.\n\r", ch );
         return;
        }
       }
       else
       {
        if ( IS_SET(victim->act, PLR_ILLUSION ) )
        {
         send_to_char( AT_YELLOW, "You can not give stuff to illusions.\n\r", ch );
         return;
        }
       }

 	howmuch = take_money( ch, amount, arg2, "give" );

	if ( !howmuch )
 	{
 	   send_to_char(AT_YELLOW, "You haven't got that much money.\n\r", ch );
           return; 
	}
	
	add_money( &victim->money, howmuch );
 
 	sprintf( buf, "You give %s %s", victim->name, money_string( howmuch ) );
	act(AT_YELLOW, buf, ch, NULL, victim, TO_CHAR );
	sprintf( buf, "%s gives you %s",
           can_see( victim, ch ) ? ch->name : "someone", money_string( howmuch ) );
        act(AT_YELLOW, buf, ch, NULL, victim, TO_VICT );
        act(AT_YELLOW, "$n gives $N some coins.",  ch, NULL, victim, TO_NOTVICT );

	mprog_bribe_trigger( victim, ch, howmuch ); 
	return;
    }

    if ( !( obj = get_obj_carry( ch, arg1 ) ) )
    {
	send_to_char(AT_DGREEN, "You do not have that item.\n\r", ch );
	return;
    }

    if ( !IS_SET( obj->wear_loc, WEAR_NONE ) )
    {
	send_to_char(AT_DGREEN, "You must remove it first.\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg2 ) ) )
    {
	send_to_char(AT_DGREEN, "They aren't here.\n\r", ch );
	return;
    }

       if ( IS_NPC(victim) )
       {
        if ( IS_SET(victim->act, ACT_ILLUSION ) )
        {
         send_to_char( AT_YELLOW, "You can not give stuff to illusions.\n\r", ch );
         return;
        }
       }
       else
       {
        if ( IS_SET(victim->act, PLR_ILLUSION ) )
        {
         send_to_char( AT_YELLOW, "You can not give stuff to illusions.\n\r", ch );
         return;
        }
       }

    if ( !can_drop_obj( ch, obj ) )
    {
	send_to_char(AT_DGREEN, "You can't let go of it.\n\r", ch );
	return;
    }

    if ( victim->carry_number + get_obj_number( obj ) > can_carry_n( victim ) )
    {
	act(AT_DGREEN, "$N has $S hands full.", ch, NULL, victim, TO_CHAR );
	return;
    }

    if ( victim->carry_weight + get_obj_weight( obj ) > can_carry_w( victim ) )
    {
	act(AT_DGREEN, "$N can't carry that much weight.", ch, NULL, victim, TO_CHAR );
	return;
    }

    if ( !can_see_obj( victim, obj ) )
    {
	act(AT_DGREEN, "$N can't see it.", ch, NULL, victim, TO_CHAR );
	return;
    }

    obj_from_char( obj );
    obj_to_char( obj, victim );
    act(AT_DGREEN, "You give $p to $N.", ch, obj, victim, TO_CHAR    );
    act(AT_DGREEN, "$n gives you $p.",   ch, obj, victim, TO_VICT    );
    act(AT_DGREEN, "$n gives $p to $N.", ch, obj, victim, TO_NOTVICT );
    oprog_give_trigger( obj, ch, victim );
    if ( !obj || !ch || !victim )
      return;
    mprog_give_trigger( victim, ch, obj );
    return;
}

/* Transport item skill here */

void do_transport( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    OBJ_DATA  *obj;
    char       arg1 [ MAX_INPUT_LENGTH ];
    char       arg2 [ MAX_INPUT_LENGTH ];

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( arg1[0] == '\0' || arg2[0] == '\0' )
    {
	send_to_char(AT_DGREEN, "Transport what to whom?\n\r", ch );
	return;
    }


    if ( !( obj = get_obj_carry( ch, arg1 ) ) )
    {
	send_to_char(AT_DGREEN, "You do not have that item.\n\r", ch );
	return;
    }

    if ( !IS_SET( obj->wear_loc, WEAR_NONE ) )
    {
	send_to_char(AT_DGREEN, "You must remove it first.\n\r", ch );
	return;
    }

    if ( !( victim = get_pc_world( ch, arg2 ) ) )
    {
	send_to_char(AT_DGREEN, "They aren't here.\n\r", ch );
	return;
    }

    if ( !can_drop_obj( ch, obj ) )
    {
	send_to_char(AT_DGREEN, "You can't let go of it.\n\r", ch );
	return;
    }

    if ( victim->carry_number + get_obj_number( obj ) > can_carry_n( victim ) )
    {
	act(AT_DGREEN, "$N has $S hands full.", ch, NULL, victim, TO_CHAR );
	return;
    }

    if ( victim->carry_weight + get_obj_weight( obj ) > can_carry_w( victim ) )
    {
	act(AT_DGREEN, "$N can't carry that much weight.", ch, NULL, victim, TO_CHAR );
	return;
    }

    if ( !can_see_obj( victim, obj ) )
    {
	act(AT_DGREEN, "$N can't see it.", ch, NULL, victim, TO_CHAR );
	return;
    }

    act(AT_DGREEN, "You ring up the prison transport service to transport $p to $N.", ch, obj, victim, TO_CHAR );
    if ( ch->money.silver > 25 )
    {
    act(AT_DGREEN, "The delivery boy states 'That'll be 25 silver, thanks.'", ch, obj, victim, TO_CHAR );
    ch->money.silver -= 25;
    act(AT_DGREEN, "The prison transport service hands you $p, its from $n.", ch, obj, victim, TO_VICT );
    act(AT_DGREEN, "The prison transport service hands $n something from $N.", victim , NULL, ch, TO_NOTVICT );
    obj_from_char( obj );
    obj_to_char( obj, victim );
    }
    else
    act(AT_DGREEN, "The delivery boy states 'You can't afford to deliver this package, sorry buddy.'", ch, obj, victim, TO_CHAR );
    return;
}

void do_fill( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    OBJ_DATA *fountain;
    char      arg [ MAX_INPUT_LENGTH ];
    bool      found;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Fill what?\n\r", ch );
	return;
    }

    if ( !( obj = get_obj_carry( ch, arg ) ) )
    {
	send_to_char(AT_BLUE, "You do not have that item.\n\r", ch );
	return;
    }

    found = FALSE;
    for ( fountain = ch->in_room->contents; fountain;
	fountain = fountain->next_content )
    {
	if ( fountain->item_type == ITEM_FOUNTAIN )
	{
	    found = TRUE;
	    break;
	}
    }

    if ( !found )
    {
	send_to_char(AT_BLUE, "There is no fountain here!\n\r", ch );
	return;
    }

    if ( obj->item_type != ITEM_DRINK_CON )
    {
	send_to_char(AT_BLUE, "You can't fill that.\n\r", ch );
	return;
    }

    if ( obj->value[1] != 0 && obj->value[2] != 0 )
    {
	send_to_char(AT_BLUE, "There is already another liquid in it.\n\r", ch );
	return;
    }

    if ( obj->value[1] >= obj->value[0] )
    {
	send_to_char(AT_BLUE, "Your container is full.\n\r", ch );
	return;
    }

    act(AT_LBLUE, "You fill $p.", ch, obj, NULL, TO_CHAR );
    obj->value[2] = 0;
    obj->value[1] = obj->value[0];
    oprog_fill_trigger( obj, ch, fountain );
    return;
}

void do_drink( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    int       amount;
    int       liquid;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	for ( obj = ch->in_room->contents; obj; obj = obj->next_content )
	{
	    if ( obj->item_type == ITEM_FOUNTAIN ) 
		break;
	}

	if ( !obj )
	{
	    send_to_char(AT_BLUE, "Drink what?\n\r", ch );
	    return;
	}
    }
    else
    {
	if ( !( obj = get_obj_here( ch, arg ) ) )
	{
	    send_to_char(AT_BLUE, "You can't find it.\n\r", ch );
	    return;
	}
    }

    if ( obj->durability <= 0 )
    {
     send_to_char( AT_BLUE, "That is broken, you can not drink from it.\n\r", ch );
     return;
    }

    if ( !IS_NPC( ch ) && ch->pcdata->condition[COND_DRUNK] >= 90 )
    {
	send_to_char(AT_BLUE, "You fail to reach your mouth.  *Hic*\n\r", ch );
	return;
    }

    switch ( obj->item_type )
    {
    default:
	send_to_char(AT_BLUE, "You can't drink from that.\n\r", ch );
	break;

    case ITEM_FOUNTAIN:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->condition[COND_THIRST] = 58;  /*  48  */
	act(AT_LBLUE, "You drink from the $p.\n\r", ch, obj, NULL, TO_CHAR );
	send_to_char(AT_BLUE, "You are not thirsty.\n\r", ch );
	act(AT_LBLUE, "$n drinks from the $p.", ch, obj, NULL, TO_ROOM );
	    oprog_use_trigger( obj, ch, ch );
	break;

    case ITEM_DRINK_CON:
	if ( obj->value[1] <= 0 )
	{
	    send_to_char(AT_BLUE, "It is already empty.\n\r", ch );
	    return;
	}

	if ( ( liquid = obj->value[2] ) >= LIQ_MAX )
	{
	    bug( "Do_drink: bad liquid number %d.", liquid );
	    liquid = obj->value[2] = 0;
	}

	act(AT_LBLUE, "You drink $T from $p.",
	    ch, obj, liq_table[liquid].liq_name, TO_CHAR );
	act(AT_LBLUE, "$n drinks $T from $p.",
	    ch, obj, liq_table[liquid].liq_name, TO_ROOM );
	    oprog_use_trigger( obj, ch, ch );

	amount = number_range( 3, 8 );
	amount = UMIN( amount, obj->value[1] );
	
	gain_condition( ch, COND_DRUNK,
	    liq_table[liquid].liq_affect[COND_DRUNK  ] );
	gain_condition( ch, COND_FULL,
	    amount * liq_table[liquid].liq_affect[COND_FULL   ] );
	gain_condition( ch, COND_THIRST,
	    amount * liq_table[liquid].liq_affect[COND_THIRST ] );
	if ( !IS_NPC( ch ) && ch->pcdata->condition[COND_DRUNK ] > 100 )
	    ch->pcdata->condition[COND_DRUNK ] = 100;

	if ( !IS_NPC( ch ) && ch->pcdata->condition[COND_DRUNK ] > 10 )
	    send_to_char(AT_ORANGE, "You feel drunk.\n\r", ch );
	if ( !IS_NPC( ch ) && ch->pcdata->condition[COND_FULL  ] > MAX_FULL )
	    send_to_char(AT_BLUE, "You are full.\n\r", ch );
	if ( !IS_NPC( ch ) && ch->pcdata->condition[COND_THIRST] > MAX_THIRST )
	    send_to_char(AT_BLUE, "You do not feel thirsty.\n\r", ch );
	
	if ( obj->value[3] != 0 )
	{
	    /* The shit was poisoned ! */
	    AFFECT_DATA af;

	    send_to_char(AT_GREEN, "You choke and gag.\n\r", ch );
	    act(AT_GREEN, "$n chokes and gags.", ch, NULL, NULL, TO_ROOM );
	    af.type      = gsn_poison;
	    af.duration  = 3 * amount;
	    af.location  = APPLY_STR;
	    af.modifier  = -2;
	    af.bitvector = AFF_POISON;
	    affect_join( ch, &af );
	}
	
	obj->value[1] -= amount;
	break;
    }

    return;
}



void do_eat( CHAR_DATA *ch, char *argument )
{
    AFFECT_DATA af;
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    int       amnt;
    one_argument( argument, arg );
    if ( arg[0] == '\0' )
    {
	send_to_char(AT_ORANGE, "Eat what?\n\r", ch );
	return;
    }

    if ( !( obj = get_obj_carry( ch, arg ) ) )
    {
	send_to_char(AT_ORANGE, "You do not have that item.\n\r", ch );
	return;
    }

    if ( !IS_IMMORTAL( ch ) )
    {
	if ( obj->item_type != ITEM_FOOD 
	&& obj->item_type != ITEM_PILL 
	&& obj->item_type != ITEM_BERRY
	&& obj->item_type != ITEM_CORPSE_NPC )
	{
	    send_to_char(AT_ORANGE, "That's not edible.\n\r", ch );
	    return;
	}

	if ( !IS_NPC( ch ) && ch->pcdata->condition[COND_FULL] > MAX_FULL )
	{   
	    send_to_char(AT_ORANGE, "You are too full to eat more.\n\r", ch );
	    return;
	}
    }
    act(AT_ORANGE, "You eat $p.", ch, obj, NULL, TO_CHAR );
    act(AT_ORANGE, "$n eats $p.", ch, obj, NULL, TO_ROOM );

    switch ( obj->item_type )
    {

    case ITEM_FOOD:
	if ( !IS_NPC( ch ) )
	{
	    int condition;

	    condition = ch->pcdata->condition[COND_FULL];
	    gain_condition( ch, COND_FULL, obj->value[0] );
	    if ( ch->pcdata->condition[COND_FULL] > MAX_FULL )
	        send_to_char(AT_ORANGE, "You are full.\n\r", ch );
	    else if ( condition == 0 && ch->pcdata->condition[COND_FULL] > 0 )
		send_to_char(AT_ORANGE, "You are no longer hungry.\n\r", ch );
	    oprog_use_trigger( obj, ch, ch );
	}

	    /* Is it tainted? -Flux. */
	switch( obj->value[3] )
	{
         default: break;
         case FOOD_POISONED:
	    act(AT_GREEN, "$n chokes and gags.", ch, 0, 0, TO_ROOM );
	    send_to_char(AT_GREEN, "You choke and gag.\n\r", ch );

          if ( !IS_AFFECTED( ch, AFF_POISON ) )
          {
	    af.type      = gsn_poison;
            af.level     = obj->level;
	    af.duration  = 2 * obj->value[0];
	    af.location  = APPLY_STR;
	    af.modifier  = -2;
	    af.bitvector = AFF_POISON;
	    affect_to_char( ch, &af ); } break;
         case FOOD_DISEASED:
	    act(AT_GREEN, "$n chokes and gags.", ch, 0, 0, TO_ROOM );
	    send_to_char(AT_GREEN, "You choke and gag.\n\r", ch );

          if ( !IS_AFFECTED2( ch, AFF_DISEASED ) )
          {
	    af.type      = gsn_plague;
            af.level     = obj->level;
	    af.duration  = obj->value[0];
	    af.location  = APPLY_CON;
	    af.modifier  = -2;
	    af.bitvector = AFF_DISEASED;
	    affect_to_char2( ch, &af ); } break;
         case FOOD_INSANE:
	    act(AT_GREEN, "$n licks $s lips.", ch, 0, 0, TO_ROOM );
	    send_to_char(AT_GREEN, "That tasted kinda good.\n\r", ch );

          if ( !IS_AFFECTED( ch, AFF_INSANE ) )
          {
	    af.type      = gsn_insane;
            af.level     = obj->level;
	    af.duration  = obj->value[0];
	    af.location  = APPLY_INT;
	    af.modifier  = -2;
	    af.bitvector = AFF_INSANE;
	    affect_to_char( ch, &af ); } break;
         case FOOD_HALLUCINATORY:
	    act(AT_GREEN, "$n licks $s lips.", ch, 0, 0, TO_ROOM );
	    send_to_char(AT_GREEN, "That tasted kinda good.\n\r", ch );

          if ( !IS_AFFECTED2( ch, AFF_HALLUCINATING ) )
          {
	    af.type      = gsn_hallucinate;
            af.level     = obj->level;
	    af.duration  = obj->value[0];
	    af.location  = APPLY_INT;
	    af.modifier  = -2;
	    af.bitvector = AFF_HALLUCINATING;
	    affect_to_char2( ch, &af ); } break;
          } break;

    case ITEM_BERRY:
       amnt = number_range( obj->value[0], obj->value[1] );
       ch->hit = UMIN( ch->hit + 200, MAX_HIT(ch) );
       update_pos( ch, ch );
       send_to_char(AT_ORANGE, "You feel warm all over.\n\r", ch);
	    oprog_use_trigger( obj, ch, ch );
       break;        
    case ITEM_PILL:
    if ( 1 < obj->level ) /* skill check */
    {
     if ( IS_NPC( ch ) )
     {
	act( AT_ORANGE, "$p is too high level for you.", ch, obj, NULL,
		 TO_CHAR );
	break;
     }
    }

    obj_cast_spell( obj->value[1], obj->value[0], ch, ch, NULL );
    obj_cast_spell( obj->value[2], obj->value[0], ch, ch, NULL );
    obj_cast_spell( obj->value[3], obj->value[0], ch, ch, NULL );
    oprog_use_trigger( obj, ch, ch );
    break;
   }

    extract_obj( obj );
    return;
}



/*
 * Remove an object.
 */
bool remove_obj( CHAR_DATA *ch, int iWear, bool fReplace )
{
    OBJ_DATA *obj;

    if ( !( obj = get_eq_char( ch, iWear ) ) )
	return TRUE;

    if ( !fReplace )
	return FALSE;

    if ( IS_SET( obj->extra_flags, ITEM_NOREMOVE ) )
    {
	act(AT_RED, "You can't remove $p.", ch, obj, NULL, TO_CHAR );
	return FALSE;
    }

    act(AT_WHITE, "$n stops using $p.", ch, obj, NULL, TO_ROOM );
    act(AT_WHITE, "You stop using $p.", ch, obj, NULL, TO_CHAR );
    unequip_char( ch, obj );

	if ( (get_eq_char( ch, WEAR_BODY_3 )) == NULL &&
             (get_eq_char( ch, WEAR_BODY_2 )) == NULL &&
             (get_eq_char( ch, WEAR_BODY_1 )) == NULL &&
             (get_eq_char( ch, WEAR_MEDALLION )) != NULL )
        {
         obj = get_eq_char( ch, WEAR_MEDALLION );
         act(AT_WHITE, "$n stops using $p.", ch, obj, NULL, TO_ROOM );
         act(AT_WHITE, "You stop using $p.", ch, obj, NULL, TO_CHAR );
         unequip_char( ch, obj );
        }

	if ( (get_eq_char( ch, WEAR_WAIST )) == NULL &&
             (get_eq_char( ch, WEAR_SHEATH_1 )) != NULL )
        {
         obj = get_eq_char( ch, WEAR_SHEATH_1 );
         act(AT_WHITE, "$n stops using $p.", ch, obj, NULL, TO_ROOM );
         act(AT_WHITE, "You stop using $p.", ch, obj, NULL, TO_CHAR );
         unequip_char( ch, obj );
        }

	if ( (get_eq_char( ch, WEAR_WAIST )) == NULL &&
             (get_eq_char( ch, WEAR_SHEATH_2 )) != NULL )
        {
         obj = get_eq_char( ch, WEAR_SHEATH_2 );
         act(AT_WHITE, "$n stops using $p.", ch, obj, NULL, TO_ROOM );
         act(AT_WHITE, "You stop using $p.", ch, obj, NULL, TO_CHAR );
         unequip_char( ch, obj );
        }

    return TRUE;
}



/*
 * Wear one object.
 * Optional replacement of existing objects.
 * Big repetitive code, ick.
 */
void wear_obj( CHAR_DATA *ch, OBJ_DATA *obj, bool fReplace )
{
    AFFECT_DATA *paf;
    int  numaff = 0;
    char buf [ MAX_STRING_LENGTH ];

  for ( paf = obj->affected; paf; paf = paf->next )
   if ( (paf) && paf->location != APPLY_NONE && paf->modifier != 0 )
    numaff += 1;

   if ( get_curr_dex(ch) < (10 + (obj->level / 9) ) )
   {
    sprintf( buf, "You try wield %s, but the weapon is far too complicated for you.\n\r",
	obj->short_descr );
    send_to_char( AT_YELLOW, buf, ch );
    return;
   }

   if ( get_curr_int(ch) < (10 + (numaff * 2) ) )
   {
    sprintf( buf,
     "You try wield %s, but its magical properties are too strong for you to handle.\n\r",
	obj->short_descr );
    send_to_char( AT_YELLOW, buf, ch );
    return;
   }

   if ( get_curr_str( ch ) < ( get_obj_weight( obj ) - 10 ) )
   {
    sprintf( buf, "You try to lift %s, but it is far too heavy.\n\r",
	obj->short_descr );
    send_to_char( AT_YELLOW, buf, ch );
    return;
   }

   equip_char( ch, obj, fReplace );

   return;
}

void do_wire( CHAR_DATA *ch, char *argument )
{
	OBJ_DATA	*weapon;
	OBJ_DATA	*wire;
	char		arg[MAX_INPUT_LENGTH];

    argument = one_argument( argument, arg );

    if ( IS_NPC( ch ) )
    {
     typo_message( ch );
     return;
    }

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Wire what?\n\r", ch );
	return;
    }

	if ( !( weapon = get_obj_carry( ch, arg ) ) )
	{
	    send_to_char(AT_BLUE, "You do not have that weapon.\n\r", ch );
	    return;
	}

    if ( weapon->item_type != ITEM_WEAPON )
    {
	send_to_char(AT_YELLOW, "That isn't a weapon.\n\r", ch );
	return;
    }

	if ( IS_OBJ_STAT( weapon, ITEM_WIRED ) )
    {
	send_to_char(AT_YELLOW, "That weapon is already wired.\n\r", ch );
	return;
    }

    if ( weapon->durability  <= 0 )
    {
     send_to_char(AT_BLUE, "You can't wire broken weapons.\n\r", ch );
     return;
    }

  for ( wire = ch->carrying; wire; wire = wire->next_content )
   {
    if ( wire->durability  <= 0 )
     continue;

    if ( wire->item_type == ITEM_TRIPWIRE )
     break;
   }

  if ( !wire )
  {
   send_to_char( AT_GREY, "You need a wire to wire your weapon.\n\r", ch );
   return;
  }

   send_to_char( AT_WHITE, "You tie a thin metal wire to your weapon.\n\r", ch );
   extract_obj( wire );
   SET_BIT( weapon->extra_flags, ITEM_WIRED );

 return;
}

void do_sheath( CHAR_DATA *ch, char *argument )
{
	OBJ_DATA	*weapon;
	OBJ_DATA	*sheath;
	char		arg[MAX_INPUT_LENGTH];
	char		arg2[MAX_INPUT_LENGTH];
        int		iWear;

    argument = one_argument( argument, arg );
    argument = one_argument( argument, arg2 );

    if ( IS_NPC( ch ) )
    {
     typo_message( ch );
     return;
    }

    if ( arg[0] == '\0' )
    {
     send_to_char(AT_BLUE, "Sheath what?\n\r", ch );
     return;
    }

    if ( !( weapon = get_obj_carry( ch, arg ) ) )
    {
     if ( !( weapon = get_eq_char( ch, WEAR_WIELD ) ) )
     {
      if ( ( weapon = get_eq_char( ch, WEAR_WIELD_2 ) ) )
      {
       if ( !is_name( ch, arg, weapon->name ) )
       {
        send_to_char(AT_BLUE, "You do not have that weapon.\n\r", ch );
        return;
       }
      }
     }
     else if ( !is_name( ch, arg, weapon->name ) )
     {
      send_to_char(AT_BLUE, "You do not have that weapon.\n\r", ch );
      return;
     }
    }
 
    if ( !( weapon ) )
    {
     send_to_char(AT_BLUE, "You do not have that weapon.\n\r", ch );
     return;
    }

    if ( weapon->durability <= 0 )
    {
     send_to_char(AT_BLUE, "You can't sheath a broken weapon.\n\r", ch );
     return;
    }

    if ( arg2[0] == '\0' )
    {
     send_to_char(AT_BLUE, "What do you intend to sheath your weapon into?\n\r", ch );
     return;
    }

    if ( !( sheath = get_obj_carry( ch, arg2 ) ) )
    {
     for ( iWear = 2; iWear < MAX_WEAR; iWear *= 2 )
     {
      if ( ( sheath = get_eq_char( ch, iWear ) ) )
      {
       if ( is_name( ch, arg2, sheath->name ) )
        break;
       else
        sheath = NULL;
      }
     }

    if ( !(sheath) )
    {
     send_to_char(AT_BLUE, "You don't have that sheath.\n\r", ch );
     return;
    }
   }

    if ( sheath->durability <= 0 )
    {
     send_to_char(AT_BLUE, "You can't use a broken sheath.\n\r", ch );
     return;
    }

   if ( sheath->value[8] != 0 )
   {
    if ( sheath->value[8] != weapon->pIndexData->vnum )
    {
      send_to_char( AT_BLUE, "That weapon does not fit in that sheath.\n\r", ch );
      return;
    }
   }
   else
   {
    if ( sheath->value[6] != weapon->value[8] ||
         sheath->value[7] != weapon->value[4] )
    {
      send_to_char( AT_BLUE, "That weapon does not fit in that sheath.\n\r", ch );
      return;
    }
   }

    if ( !sheath->sheath )
    {
     send_to_char( AT_BLUE, "You sheath your weapon.\n\r", ch );
     act( AT_BLUE, "$n sheaths $s $p.", ch, weapon, NULL, TO_ROOM );
     obj_from_char( weapon );
     obj_to_sheath( weapon, sheath );
     return;
    }

   send_to_char( AT_BLUE, "Your sheath is full.\n\r", ch );
 return;
}

void do_unsheath( CHAR_DATA *ch, char *argument )
{
	OBJ_DATA	*weapon = NULL;
	OBJ_DATA	*sheath;
	char		arg[MAX_INPUT_LENGTH];
	char		arg2[MAX_INPUT_LENGTH];
        int		iWear;

    argument = one_argument( argument, arg );
    argument = one_argument( argument, arg2 );

    if ( IS_NPC( ch ) )
    {
     typo_message( ch );
     return;
    }

    if ( arg[0] == '\0' || arg2[0] == '\0' )
    {
     send_to_char(AT_BLUE, "Unsheath what from what?\n\r", ch );
     return;
    }

    if ( !( sheath = get_obj_carry( ch, arg2 ) ) )
    {
     for ( iWear = 2; iWear < MAX_WEAR; iWear *= 2 )
     {
      if ( ( sheath = get_eq_char( ch, iWear ) ) )
      {
       if ( is_name( ch, arg2, sheath->name ) )
        break;
       else
        sheath = NULL;
      }
     }

    if ( !(sheath) )
    {
     send_to_char(AT_BLUE, "You don't have that sheath.\n\r", ch );
     return;
    }
   }

     if ( !( weapon = get_obj_list( ch, arg, sheath->sheath ) ) )
     {
      send_to_char( AT_BLUE, "That sheath does not contain that weapon.\n\r", ch );
      return;
     }

    send_to_char( AT_BLUE, "You unsheath your weapon.\n\r", ch );
    act( AT_BLUE, "$n unsheaths $s $p.", ch, weapon, NULL, TO_ROOM );
    obj_from_obj( weapon, TRUE );
    obj_to_char( weapon, ch );
    return;

 return;
}

void do_wield( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg  [ MAX_INPUT_LENGTH ];
    char      arg2 [ MAX_INPUT_LENGTH ];

    argument = one_argument( argument, arg  );
    argument = one_argument( argument, arg2 );     

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Wield what?\n\r", ch );
	return;
    }

    if ( !( obj = get_obj_carry( ch, arg ) ) )
    {
     send_to_char(AT_BLUE, "You do not have that item.\n\r", ch );
     return;
    }

    if ( obj->durability <= 0 )
    {
     send_to_char(AT_BLUE, "You can't wield a broken object.\n\r", ch );
     return;
    }

    if ( !IS_SET( obj->wear_flags, ITEM_WIELD ) )
    {
     send_to_char(AT_BLUE, "Use the wear command.\n\r", ch );
     return;
    }

    if ( IS_AFFECTED2( ch, AFF_RAGE ) && obj->item_type == ITEM_WEAPON )
    {
     send_to_char( AT_RED,
      "You are too enraged to wield a weapon.\n\r", ch );
     return;
    }

    if ( obj->item_type == ITEM_WEAPON
      && obj->value[8] == WEAPON_BLADE )
    {
     if ( arg2[0] == '\0' )
     {
      send_to_char( AT_WHITE,
       "How would you like to wield this blade weapon?(pierce or slash)\n\r",
       ch );
      return;
     }

     if ( !str_cmp( arg2, "pierce" ) )
      obj->value[7] = 1;
     else if ( !str_cmp( arg2, "slash" ) )
      obj->value[7] = 0;
     else
     {
      send_to_char( AT_WHITE,
       "How would you like to wield this blade weapon?(pierce or slash)\n\r",
       ch );
      return;
     }
    }
  wear_obj( ch, obj, TRUE );
 return;
}

void do_dual( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg  [ MAX_INPUT_LENGTH ];
    char      arg2 [ MAX_INPUT_LENGTH ];

    argument = one_argument( argument, arg  );
    argument = one_argument( argument, arg2 );     

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Wield what?\n\r", ch );
	return;
    }

    if ( !( obj = get_obj_carry( ch, arg ) ) )
    {
     send_to_char(AT_BLUE, "You do not have that item.\n\r", ch );
     return;
    }

    if ( !IS_SET( obj->wear_flags, ITEM_WIELD ) )
    {
     send_to_char(AT_BLUE, "Use the wear command.\n\r", ch );
     return;
    }

    if ( obj->durability <= 0 )
    {
     send_to_char(AT_BLUE, "You can't wield a broken object.\n\r", ch );
     return;
    }


    if ( IS_AFFECTED2( ch, AFF_RAGE ) && obj->item_type == ITEM_WEAPON )
    {
     send_to_char( AT_RED,
      "You are too enraged to wield a weapon.\n\r", ch );
     return;
    }

    if ( obj->item_type == ITEM_WEAPON
      && obj->value[8] == WEAPON_BLADE )
    {
     if ( arg2[0] == '\0' )
     {
      send_to_char( AT_WHITE,
       "How would you like to wield this blade weapon?(pierce or slash)\n\r",
       ch );
      return;
     }

     if ( !str_cmp( arg2, "pierce" ) )
      obj->value[7] = 1;
     else if ( !str_cmp( arg2, "slash" ) )
      obj->value[7] = 0;
     else
     {
      send_to_char( AT_WHITE,
       "How would you like to wield this blade weapon?(pierce or slash)\n\r",
       ch );
      return;
     }
    }
  wear_obj( ch, obj, FALSE );
 return;
}

void do_wear( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg  [ MAX_INPUT_LENGTH ];
    char      arg2 [ MAX_INPUT_LENGTH ];

    argument = one_argument( argument, arg  );
    argument = one_argument( argument, arg2 );     

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Wear what?\n\r", ch );
	return;
    }

    if ( !str_cmp( arg, "all" ) )
    {
        OBJ_DATA *obj_next;

        for ( obj = ch->carrying; obj; obj = obj_next )
	{
	    obj_next = obj->next_content;

         if ( obj->durability <= 0 )
          continue;

         if ( IS_SET( obj->wear_flags, ITEM_WIELD ) )
	  continue;

         if ( !is_wearable( obj ) )
          continue;

	    if ( IS_SET( obj->wear_loc, WEAR_NONE ) && can_see_obj( ch, obj ) )
		wear_obj( ch, obj, FALSE );
	}
	return;
    }
    else
    {
	if ( !( obj = get_obj_carry( ch, arg ) ) )
	{
	    send_to_char(AT_BLUE, "You do not have that item.\n\r", ch );
	    return;
	}

        if ( IS_SET( obj->wear_flags, ITEM_WIELD ) )
	{
	 send_to_char(AT_BLUE,
          "Use the wield or dual commands for wielded/held items.\n\r",
           ch );
	 return;
	}

    if ( obj->durability <= 0 )
	{
	    send_to_char(AT_BLUE, "You can't wear broken armor.\n\r", ch );
	    return;
	}

     wear_obj( ch, obj, TRUE );
    }
  return;
}



void do_remove( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );


    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Remove what?\n\r", ch );
	return;
    }

    if ( !str_cmp( arg, "all" ) )
    {
	for ( obj = ch->carrying; obj; obj = obj->next_content )
	{      
          if ( obj == NULL || obj->deleted )
          continue;

	    if ( !IS_SET( obj->wear_loc, WEAR_NONE ) && can_see_obj( ch, obj ) )
	        remove_obj( ch, obj->wear_loc, TRUE );
	}
	return;
    }

    if ( !( obj = get_obj_wear( ch, arg ) ) )
    {
	send_to_char(AT_BLUE, "You do not have that item.\n\r", ch );
	return;
    }

    if ( !(can_see_obj( ch, obj )) || obj->deleted )
    {
	send_to_char(AT_BLUE, "You do not have that item.\n\r", ch );
	return;
    }

    remove_obj( ch, obj->wear_loc, TRUE );
    oprog_remove_trigger( obj, ch );
    return;
}


void do_sacrifice( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      	arg  [ MAX_INPUT_LENGTH ];
    char      	buf  [ MAX_STRING_LENGTH];
    char      	buf1 [ MAX_STRING_LENGTH];
    int		iname;
    int		award;
    int		chance;
    int		diff;		

	/* immortal names go here */
	char * msgbuf[]=	{
	"Swift", "Ceasar", "Onegative", "Flux", "Aloran", "Sin", "Tsuke" };

    iname = number_range( 0 , 6 ); /* change second # if you add/delete
				       immortal names */
    one_argument( argument, arg );

    if ( arg[0] == '\0' || !str_cmp( arg, ch->name ) )
    {
	send_to_char( AT_WHITE,
            "The wardens appreciates your offer and may accept it later.", ch );
        act( AT_WHITE, 
             "$n offers $mself to the wardens, who graciously decline.",
	    ch, NULL, NULL, TO_ROOM );
	return;
    }

    if ( !str_cmp( arg, "all"  ) )
	{
	OBJ_DATA *obj_next;
	for ( obj = ch->in_room->contents; obj; obj = obj_next )
		{
		obj_next = obj->next_content;

		if ( obj->deleted )
		   continue;

		if ( CAN_WEAR( obj, ITEM_TAKE ) 
		&& obj->item_type != ITEM_MONEY)
		   do_sacrifice( ch, strdup( obj->name ) );
		}
	return;
	}
	
    obj = get_obj_list( ch, arg, ch->in_room->contents );
    if ( !obj )
    {
	send_to_char( AT_WHITE, "You can't find it.\n\r", ch );
	return;
    }

    if ( obj->item_type == ITEM_BOMB || !CAN_WEAR( obj, ITEM_TAKE ) )
    {
	act( AT_WHITE,
	     "$p is not an acceptable sacrifice.", ch, obj, NULL, TO_CHAR );
	return;
    }
    
    if ( obj->ownedby != NULL && strcmp( obj->ownedby, "(null)" )
         && obj->item_type != ITEM_CORPSE_NPC )
    {
     if ( str_cmp( obj->ownedby, ch->name ) )
     {
	act( AT_WHITE,
	     "$p is not yours.", ch, obj, NULL, TO_CHAR );
	return;
     }
    }     

    if ( obj->item_type == ITEM_MONEY )
    {
    	send_to_char( AT_WHITE, "Sorry, you cannot sacrifice money!\n\r", ch );
    	return;
    }
    
    chance = number_range (1 , 4 );
    switch ( chance )
    {
    case 1:
     	award = number_range ( 1, 6 );
	sprintf( buf1, "%d copper coin%s", award, (award != 1) ? "s" : "" );
	ch->money.copper += award;
	break;
    case 2:	
        if ( ch->hit >= MAX_HIT(ch) )
        diff = 0;
        else
        diff = 6; 
      	award = number_range ( 0 , diff );
	sprintf( buf1, "%d hit point%s", award, (award != 1) ? "s" : "" );
        ch->hit += award;
	break;
    case 3:	
		if ( ch->mana >= MAX_MANA(ch))
			diff = 0;
		else
			diff = 6;
	      	award = number_range ( 0 , diff );
		sprintf( buf1, "%d mana", award );
       		ch->mana += award;
	break;
    case 4:	
	if ( ch->move >= MAX_MOVE(ch) )
	diff = 0;
	else
	diff = 6;
      	award = number_range ( 0 , diff );
	sprintf( buf1, "%d movement point%s", award, (award != 1) ? "s" : "" );
        ch->move += award;
	break;
    default:
        sprintf (buf1, "3 silver coins" );
	ch->money.silver += 3;
        break;
    }	   
        
    sprintf(buf,  "%s gives you %s for your sacrifice.\n\r",
             msgbuf[iname], buf1 );
    
    send_to_char( AT_WHITE, buf , ch );

    act( AT_WHITE, "$n sacrifices $p to the wardens.", ch, obj, NULL, TO_ROOM );
    wiznet( "$N sends up $p as a burnt offering.", ch, obj, WIZ_SACCING, 0, 0 );
    extract_obj( obj );
    return;
}  




void do_quaff( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Quaff what?\n\r", ch );
	return;
    }

    if ( !( obj = get_obj_carry( ch, arg ) ) )
    {
	send_to_char(AT_BLUE, "You do not have that potion.\n\r", ch );
	return;
    }

    if ( obj->item_type != ITEM_POTION )
    {
	send_to_char(AT_BLUE, "You can quaff only potions.\n\r", ch );
	return;
    }

    act(AT_BLUE, "You quaff $p.", ch, obj, NULL ,TO_CHAR );
    act(AT_BLUE, "$n quaffs $p.", ch, obj, NULL, TO_ROOM );
    if ( obj->value[1] == skill_lookup ( "aura of peace" )
      || obj->value[2] == skill_lookup ( "aura of peace" )
      || obj->value[3] == skill_lookup ( "aura of peace" ) )
    {
      extract_obj ( obj );
      return;
    }
    if ( obj->value[1] == skill_lookup ( "chaos field" )
      || obj->value[2] == skill_lookup ( "chaos field" )
      || obj->value[3] == skill_lookup ( "chaos field" ) )
    {
      if ( obj->value[0] > 50 )
       {
        extract_obj ( obj );
        return;
       }
    }
    if ( obj->value[1] == skill_lookup ( "blade barrier" )
      || obj->value[2] == skill_lookup ( "blade barrier" )
      || obj->value[3] == skill_lookup ( "blade barrier" ) )
    {
      if ( obj->value[0] > 50 )
      {
       extract_obj ( obj );
       return;
      }
    }
    if ( obj->value[1] == skill_lookup ( "vibrate" )
      || obj->value[2] == skill_lookup ( "vibrate" )
      || obj->value[3] == skill_lookup ( "vibrate" ) )
    {
      if ( obj->value[0] > 50 )
       {
       extract_obj ( obj );
       return;
       }
    }
    if ( obj->value[1] == skill_lookup ( "iceshield" )
      || obj->value[2] == skill_lookup ( "iceshield" )
      || obj->value[3] == skill_lookup ( "iceshield" ) )
    {
      if ( obj->value[0] > 50 )
       {
       extract_obj ( obj );
       return;
       }
    }

     if ( IS_NPC( ch ) )
     {
      act(AT_BLUE, "$p is too high level for you.", ch, obj, NULL, TO_CHAR );
      extract_obj( obj );
      return;
     }

        /* obj->value[0] is not used for potions */
	obj_cast_spell( obj->value[1], obj->value[0], ch, ch, NULL );
	obj_cast_spell( obj->value[2], obj->value[0], ch, ch, NULL );
	obj_cast_spell( obj->value[3], obj->value[0], ch, ch, NULL );
	oprog_use_trigger( obj, ch, ch );

    extract_obj( obj );
    return;
}



void do_recite( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *scroll;
    OBJ_DATA  *obj;
    CHAR_DATA *victim;
    char       arg1 [ MAX_INPUT_LENGTH ];
    char       arg2 [ MAX_INPUT_LENGTH ];

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( !( scroll = get_obj_carry( ch, arg1 ) ) )
    {
	send_to_char(AT_BLUE, "You do not have that scroll.\n\r", ch );
	return;
    }

    if ( scroll->item_type != ITEM_SCROLL )
    {
	send_to_char(AT_BLUE, "You can recite only scrolls.\n\r", ch );
	return;
    }

    obj = NULL;
    if ( arg2[0] == '\0' )
    {
	victim = ch;
    }
    else
    {
	if ( !( victim = get_char_room ( ch, arg2 ) )
	    && !( obj  = get_obj_here  ( ch, arg2 ) ) )
	{
	    send_to_char(AT_BLUE, "You can't find it.\n\r", ch );
	    return;
	}
    }

    act(AT_BLUE, "You recite $p.", ch, scroll, NULL, TO_CHAR );
    act(AT_BLUE, "$n recites $p.", ch, scroll, NULL, TO_ROOM );

    /* Scrolls skill Thalador */
	if ( !IS_NPC( ch ) && ch->race != RACE_AQUINIS
	&& !( number_percent( ) < ch->pcdata->learned[gsn_scrolls] ) )
    {
	switch ( number_bits( 3 ) )
	{
	case 0: 
	case 1:                      
	case 2:
	case 3:
	    act( AT_WHITE, "You can't understand $p at all.",
		ch, scroll, NULL, TO_CHAR );
	    act( AT_WHITE, "$n can't understand $p at all.",
		ch, scroll, NULL, TO_ROOM );
	    return;                    
	case 4:                
	case 5:                      
	case 6:                      
	    send_to_char(AT_CYAN, "You must have said something incorrectly.\n\r",
			 ch );
	    act(AT_CYAN,  "$n must have said something incorrectly.",
		ch, NULL,   NULL, TO_ROOM );
	    act(AT_CYAN,  "$p blazes brightly, then is gone.",
		ch, scroll, NULL, TO_CHAR );
	    act(AT_CYAN,  "$p blazes brightly and disappears.",
		ch, scroll, NULL, TO_ROOM );
	    extract_obj( scroll );
	    return;
	case 7:
	    act(AT_RED,  
	"You completely botch the recitation, and $p bursts into flames!!", 
		ch, scroll, NULL, TO_CHAR );
	    act(AT_RED,  "$p glows and then bursts into flame!", 
		ch, scroll, NULL, TO_ROOM );
	    /*
	     * damage( ) call after extract_obj in case the damage would
	     * have extracted ch.  This is okay because we merely mark
	     * obj->deleted; it still retains all values until list_update.
	     * Sloppy?  Okay, create another integer variable. ---Thelonius
	     */
	    extract_obj( scroll );
	    damage( ch, ch, scroll->level, gsn_scrolls, DAM_HEAT, TRUE );
	    return;
	}
    }


        /* scroll->value[0] is not used for scrolls */
	obj_cast_spell( scroll->value[1], scroll->level, ch, victim, obj );
	obj_cast_spell( scroll->value[2], scroll->level, ch, victim, obj );
	obj_cast_spell( scroll->value[3], scroll->level, ch, victim, obj );
	if ( victim )
	  oprog_use_trigger( scroll, ch, victim );
	else
	  oprog_use_trigger( scroll, ch, obj );

    update_skpell(ch, gsn_scrolls);
    extract_obj( scroll );
    return;
}



void do_brandish( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *staff;
    CHAR_DATA *vch;
    int        sn;

    if ( !( staff = get_eq_char( ch, WEAR_WIELD ) ) )
     if ( !( staff = get_eq_char( ch, WEAR_WIELD_2 ) ) )
    {
	send_to_char(AT_BLUE, "You hold nothing in your hand.\n\r", ch );
	return;
    }

    if ( staff->item_type != ITEM_STAFF )
    {
	send_to_char(AT_BLUE, "You can brandish only with a staff.\n\r", ch );
	return;
    }

    if ( ( sn = staff->value[3] ) < 0
	|| !is_sn(sn)
	|| skill_table[sn].spell_fun == 0 )
    {
	bug( "Do_brandish: bad sn %d.", sn );
	return;
    }

    WAIT_STATE( ch, 2 * PULSE_VIOLENCE );

    if ( ( staff->value[2] > 0 ) || ( staff->value[1] == -1 ) )
    {
        CHAR_DATA *vch_next;

	act(AT_BLUE, "You brandish $p.",  ch, staff, NULL, TO_CHAR );
	act(AT_BLUE, "$n brandishes $p.", ch, staff, NULL, TO_ROOM );

	/* Staves skill by Thalador */
	if ( !IS_NPC( ch ) && ch->race != RACE_AQUINIS
	    && !( number_percent( ) < ch->pcdata->learned[gsn_staves] ) )
	{ 
	    switch ( number_bits( 3 ) )
	    {
	    case 0: 
	    case 1:                      
	    case 2:                      
	    case 3: 
	        act( AT_CYAN, "You are unable to invoke the power of $p.",
		    ch, staff, NULL, TO_CHAR );
		act( AT_CYAN, "$n is unable to invoke the power of $p.",
		    ch, staff, NULL, TO_ROOM );
		return;                    
	    case 4:                
	    case 5:                      
	    case 6:                      
		act( AT_CYAN, "You summon the power of $p, but it fizzles away.",
		    ch, staff, NULL, TO_CHAR );
		act( AT_CYAN, "$n summons the power of $p, but it fizzles away.",
		    ch, staff, NULL, TO_ROOM );
		if ( --staff->value[2] <= 0 )
		{
		    act( AT_CYAN, "$p blazes bright and is gone.",
			ch, staff, NULL, TO_CHAR );
		    act( AT_CYAN, "$p blazes bright and is gone.",
			ch, staff, NULL, TO_ROOM );
		    extract_obj( staff );
		}
		return;
	    case 7:
		act( AT_CYAN, "You can't control the power of $p, and it surges with energy!",
		    ch, staff, NULL, TO_CHAR );
		/*
		 * damage( ) call after extract_obj in case the damage would
		 * have extracted ch.  This is okay because we merely mark
		 * obj->deleted; it still retains all values until list_update.
		 * Sloppy?  Okay, create another integer variable. ---Thelonius
		 */

                /* I changed this, it damages the obj now, doesn't destroy
                   though, it may still cause extraction anyways */
		damage_object( ch, staff, DAM_VOID, dice( 1, 250 ) );
		damage( ch, ch, staff->level, gsn_staves, DAM_SCRATCH, TRUE  );
		return;
	    }
	}
	for ( vch = ch->in_room->people; vch; vch = vch_next )
	{
	    vch_next = vch->next_in_room;

	    if ( vch->deleted )
	        continue;

	    switch ( skill_table[sn].target )
	    {
	    default:
		bug( "Do_brandish: bad target for sn %d.", sn );
		return;

	    case TAR_IGNORE:
		if ( vch != ch )
		    continue;
		break;

	    case TAR_CHAR_OFFENSIVE:
		if ( IS_NPC( ch ) ? IS_NPC( vch ) : !IS_NPC( vch ) )
		    continue;
		break;
		
	    case TAR_CHAR_DEFENSIVE:
		if ( IS_NPC( ch ) ? !IS_NPC( vch ) : IS_NPC( vch ) )
		    continue;
		break;

	    case TAR_CHAR_SELF:
		if ( vch != ch )
		    continue;
		break;
	    }

	    /* staff->value[0] is not used for staves */
	    obj_cast_spell( staff->value[3], staff->level, ch, vch, NULL );
	    oprog_use_trigger( staff, ch, vch );
	}
    }
    if (!(staff->value[1] == -1 ))
    if ( --staff->value[2] <= 0 )
    {
	act(AT_WHITE, "Your $p blazes bright and is gone.", ch, staff, NULL, TO_CHAR );
	act(AT_WHITE, "$n's $p blazes bright and is gone.", ch, staff, NULL, TO_ROOM );
	extract_obj( staff );
    }

    update_skpell(ch,gsn_staves);
    return;
}


void do_stare ( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *wand;
    OBJ_DATA  *obj;
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );
    if ( arg[0] == '\0' && !ch->fighting )
    {
	send_to_char(AT_BLUE, "Stare at whom or what?\n\r", ch );
	return;
    }

    if ( !( wand = get_eq_char( ch, WEAR_IN_EYES ) ) )
    {
	send_to_char(AT_BLUE, "You have no lenses in your eyes.\n\r", ch );
	return;
    }

    if ( wand->item_type != ITEM_LENSE )
    {
	send_to_char(AT_BLUE, "You can only stare with magical lenses.\n\r", ch );
	return;
    }

    obj = NULL;
    if ( arg[0] == '\0' )
    {
	if ( ch->fighting )
	{
	    victim = ch->fighting;
	}
	else
	{
	    send_to_char(AT_BLUE, "Stare at whom or what?\n\r", ch );
	    return;
	}
    }
    else
    {
	if ( !( victim = get_char_room ( ch, arg ) )
	    && !( obj  = get_obj_here  ( ch, arg ) ) )
	{
	    send_to_char(AT_BLUE, "You can't find it.\n\r", ch );
	    return;
	}
    }

    WAIT_STATE( ch, 2 * PULSE_VIOLENCE );

    if ( ( wand->value[2] > 0 ) || ( wand->value[1] == -1 ) )
    {
	if ( victim )
	{
	    act(AT_BLUE, "You stare at $N with $p.", ch, wand, victim, TO_CHAR );
	    act(AT_BLUE, "$n stares at $N with $p.", ch, wand, victim, TO_ROOM );
	}
	else
	{
	    act(AT_BLUE, "You stare at $P with $p.", ch, wand, obj, TO_CHAR );
	    act(AT_BLUE, "$n stares at $P with $p.", ch, wand, obj, TO_ROOM );
	}

	/* wand->value[0] is not used for lenses */
	obj_cast_spell( wand->value[3], wand->level, ch, victim, obj );
	if ( victim )
	  oprog_use_trigger( wand, ch, victim );
	else
	  oprog_use_trigger( wand, ch, obj );
    }

    if (!(wand->value[1] == -1 ) )
    if ( --wand->value[2] <= 0 )
    {
	act(AT_WHITE, "Your $p melts in your eyes.", ch, wand, NULL, TO_CHAR );
	act(AT_WHITE, "$n's $p melts in $s eyes.", ch, wand, NULL, TO_ROOM );
	extract_obj( wand );
    }

    return;
}

void do_zap( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *wand;
    OBJ_DATA  *obj;
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );
    if ( arg[0] == '\0' && !ch->fighting )
    {
	send_to_char(AT_BLUE, "Zap whom or what?\n\r", ch );
	return;
    }

    if ( !( wand = get_eq_char( ch, WEAR_WIELD ) ) )
     if ( !( wand = get_eq_char( ch, WEAR_WIELD_2 ) ) )
    {
	send_to_char(AT_BLUE, "You hold nothing in your hand.\n\r", ch );
	return;
    }

    if ( wand->item_type != ITEM_WAND )
    {
	send_to_char(AT_BLUE, "You can zap only with a wand.\n\r", ch );
	return;
    }

    obj = NULL;
    if ( arg[0] == '\0' )
    {
	if ( ch->fighting )
	{
	    victim = ch->fighting;
	}
	else
	{
	    send_to_char(AT_BLUE, "Zap whom or what?\n\r", ch );
	    return;
	}
    }
    else
    {
	if ( !( victim = get_char_room ( ch, arg ) )
	    && !( obj  = get_obj_here  ( ch, arg ) ) )
	{
	    send_to_char(AT_BLUE, "You can't find it.\n\r", ch );
	    return;
	}
    }

    WAIT_STATE( ch, 2 * PULSE_VIOLENCE );

    if ( ( wand->value[2] > 0 ) || ( wand->value[1] == -1 ) )
    {
	if ( victim )
	{
	    act(AT_BLUE, "You zap $N with $p.", ch, wand, victim, TO_CHAR );
	    act(AT_BLUE, "$n zaps $N with $p.", ch, wand, victim, TO_ROOM );
	}
	else
	{
	    act(AT_BLUE, "You zap $P with $p.", ch, wand, obj, TO_CHAR );
	    act(AT_BLUE, "$n zaps $P with $p.", ch, wand, obj, TO_ROOM );
	}

	/* Wands skill by Thalador */
	if ( !IS_NPC( ch ) && ch->race != RACE_AQUINIS
	    && !( number_percent( ) < ch->pcdata->learned[gsn_wands] ) )
	{ 
	    switch ( number_bits( 3 ) )
	    {
	    case 0: 
	    case 1:                      
	    case 2:                      
	    case 3: 
	        act( AT_CYAN, "You are unable to invoke the power of $p.",
		    ch, wand, NULL, TO_CHAR );
		act( AT_CYAN, "$n is unable to invoke the power of $p.",
		    ch, wand, NULL, TO_ROOM );
		return;                    
	    case 4:                
	    case 5:                      
	    case 6:                      
		act( AT_CYAN, "You summon the power of $p, but it fizzles away.",
		    ch, wand, NULL, TO_CHAR );
		act( AT_CYAN, "$n summons the power of $p, but it fizzles away.",
		    ch, wand, NULL, TO_ROOM );
		if ( --wand->value[2] <= 0 )
		{
		    act( AT_CYAN, "$p blazes bright and is gone.",
			ch, wand, NULL, TO_CHAR );
		    act( AT_CYAN, "$p blazes bright and is gone.",
			ch, wand, NULL, TO_ROOM );
		    extract_obj( wand );
		}
		return;
	    case 7:
		act( AT_CYAN, "You can't control the power of $p, and it surges with energy!",
		    ch, wand, NULL, TO_CHAR );

                /* I changed this, it damages the obj now, doesn't destroy
                   though, it may still cause extraction anyways */
		damage_object( ch, wand, DAM_VOID, dice( 1, 250 ) );
		damage( ch, ch, wand->level, gsn_wands, DAM_SCRATCH, TRUE  );
		return;
	    }
	}


	/* wand->value[0] is not used for wands */
	obj_cast_spell( wand->value[3], wand->level, ch, victim, obj );
	if ( victim )
	  oprog_use_trigger(wand, ch, victim );
	else
	  oprog_use_trigger( wand, ch, obj );
    }

    if (!(wand->value[1] == -1 ) )
    if ( --wand->value[2] <= 0 )
    {
	act(AT_WHITE, "Your $p explodes into fragments.", ch, wand, NULL, TO_CHAR );
	act(AT_WHITE, "$n's $p explodes into fragments.", ch, wand, NULL, TO_ROOM );
	extract_obj( wand );
    }
    update_skpell(ch, gsn_wands);
    return;
}



void do_steal( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *obj;
    CHAR_DATA *victim;
    char       buf  [ MAX_STRING_LENGTH ];
    char       arg1 [ MAX_INPUT_LENGTH  ];
    char       arg2 [ MAX_INPUT_LENGTH  ];
    int        percent;

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( arg1[0] == '\0' || arg2[0] == '\0' )
    {
	send_to_char(AT_BLOOD, "Steal what from whom?\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg2 ) ) )
    {
	send_to_char(AT_BLOOD, "They aren't here.\n\r", ch );
	return;
    }
    if ( IS_SET(victim->in_room->room_flags, ROOM_SAFE) )
    {
       send_to_char(AT_BLOOD, "You cannot steal in a safe room.\n\r", ch );
       return;
    }

    if ( victim == ch )
    {
	send_to_char(AT_BLOOD, "That's pointless.\n\r", ch );
	return;
    }

    WAIT_STATE( ch, skill_table[gsn_steal].beats );
    percent  = number_percent( ) + ( IS_AWAKE( victim ) ? 10 : -50 );

    if ( ch->race == RACE_KENDER )
    percent  = number_percent( ) + ( IS_AWAKE( victim ) ? -10 : -75 );

    if ( ( victim->position == POS_FIGHTING && victim->fighting != ch )
	|| ( !IS_NPC( ch ) && percent > ch->pcdata->learned[gsn_steal] ) )
    {
	/*
	 * Failure.
	 */
	send_to_char(AT_RED, "Oops.\n\r", ch );
	act(AT_RED, "$n tried to steal from you.\n\r", ch, NULL, victim, TO_VICT    );
	act(AT_RED, "$n tried to steal from $N.\n\r",  ch, NULL, victim, TO_NOTVICT );
	sprintf( buf, "%s is a bloody thief!", ch->name );
	do_shout( victim, buf );
	if ( !IS_NPC( ch ) )
	{
	    if ( IS_NPC( victim ) )
	    {
		multi_hit( victim, ch, TYPE_UNDEFINED );
	    }
	}

	return;
    }

    if (   !str_prefix( arg1, "coins" ) )
    {
	MONEY_DATA amount;
        if ( victim->money.gold <= 0 && victim->money.silver <= 0 && victim->money.copper <= 0 ) 
        {
        send_to_char( AT_WHITE, "You couldn't grab any cash.\n\r", ch);
        return;
        }

        if ( !(victim->money.gold <= 20) )
	amount.gold = number_range( 1, 2 ) * 10;
        else 
        amount.gold = 0;
        if ( !(victim->money.silver <= 30) )
  	amount.silver = number_range( 1, 3 ) * 10;
        else 
        amount.silver = 0;
        if ( !(victim->money.copper <= 50) )
	amount.copper = number_range( 1, 5 ) * 10;	
        else 
        amount.copper = 0;

        if ( amount.gold <= 0 && amount.silver <= 0 && amount.copper <= 0 )
        {
        send_to_char( AT_WHITE, "You couldn't grab any cash.\n\r", ch);
        return;
        }
        if ( amount.gold >= 20 && amount.silver >= 30 && amount.copper >= 50 )
        {
        send_to_char( AT_WHITE, "You couldn't grab any cash.\n\r", ch);
        return;
        }

	add_money( &ch->money, &amount );  
	sub_money( &victim->money, &amount );

	sprintf( buf, "Jackpot!  You got %s\n\r", money_string( &amount ) );
	send_to_char(AT_RED, buf, ch );
	return;
   }

    if ( !( obj = get_obj_carry( victim, arg1 ) ) )
    {
	send_to_char(AT_BLOOD, "You can't find it.\n\r", ch );
	return;
    }
	
    if ( !can_drop_obj( ch, obj )
	|| IS_SET( obj->extra_flags, ITEM_INVENTORY )
    {
	send_to_char(AT_BLOOD, "You can't pry it away.\n\r", ch );
	return;
    }

    if ( ch->carry_number + get_obj_number( obj ) > can_carry_n( ch ) )
    {
	send_to_char(AT_BLOOD, "You have your hands full.\n\r", ch );
	return;
    }

    if ( ch->carry_weight + get_obj_weight( obj ) > can_carry_w( ch ) )
    {
	send_to_char(AT_BLOOD, "You can't carry that much weight.\n\r", ch );
	return;
    }

    obj_from_char( obj );
    obj_to_char( obj, ch );
    send_to_char(AT_RED, "Ok.\n\r", ch );

    update_skpell( ch, gsn_steal );

    return;
}



/*
 * Shopping commands.
 */
CHAR_DATA *find_keeper( CHAR_DATA *ch )
{
    CHAR_DATA *keeper;
    SHOP_DATA *pShop;

    pShop = NULL;
    for ( keeper = ch->in_room->people; keeper; keeper = keeper->next_in_room )
    {
	if ( IS_NPC( keeper ) && ( pShop = keeper->pIndexData->pShop ) )
	    break;
    }

    if ( !pShop || IS_AFFECTED( keeper, AFF_CHARM ) )
    {
	send_to_char(C_DEFAULT, "You can't do that here.\n\r", ch );
	return NULL;
    }

    /*
     * Shop hours.
     */
    if ( time_info.hour < pShop->open_hour )
    {
	do_say( keeper, "Sorry, come back later." );
	return NULL;
    }
    
    if ( time_info.hour > pShop->close_hour )
    {
	do_say( keeper, "Sorry, come back tomorrow." );
	return NULL;
    }

    return keeper;
}

CHAR_DATA *find_artist( CHAR_DATA *ch )
{
    CHAR_DATA *artist;
    TATTOO_ARTIST_DATA *pTattoo;

    pTattoo = NULL;
    for ( artist = ch->in_room->people; artist; artist = artist->next_in_room )
    {
	if ( IS_NPC( artist ) && ( pTattoo = artist->pIndexData->pTattoo ) )
	    break;
    }

    if ( !pTattoo || IS_AFFECTED( artist, AFF_CHARM ) )
    {
	send_to_char(C_DEFAULT, "You can't do that here.\n\r", ch );
	return NULL;
    }

    return artist;
}

CHAR_DATA *find_dealer( CHAR_DATA *ch )
{
    CHAR_DATA *dealer;
    CASINO_DATA *pCasino;

    pCasino = NULL;
    for ( dealer = ch->in_room->people; dealer; dealer = dealer->next_in_room )
    {
	if ( IS_NPC( dealer ) && ( pCasino = dealer->pIndexData->pCasino ) )
	    break;
    }

    if ( !pCasino || IS_AFFECTED( dealer, AFF_CHARM ) )
    {
	send_to_char(C_DEFAULT, "You can't do that here.\n\r", ch );
	return NULL;
    }

    return dealer;
}

CHAR_DATA *find_artifactor( CHAR_DATA *ch )
{
    CHAR_DATA *arti;
    bool      yes = FALSE;

    for ( arti = ch->in_room->people; arti; arti = arti->next_in_room )
    {
      if ( arti->deleted )
        continue;

	if ( IS_NPC( arti ) &&
             arti->spec_fun == spec_lookup( "spec_utility_artifactor" ) )
        {
         yes = TRUE;
         break;
        }
    }

    if ( !yes || IS_AFFECTED( arti, AFF_CHARM ) )
	return NULL;

    return arti;
}

CHAR_DATA *find_o_surgeon( CHAR_DATA *ch )
{
    CHAR_DATA *surgeon;
    bool      yes = FALSE;

    for ( surgeon = ch->in_room->people; surgeon; surgeon = surgeon->next_in_room )
    {
      if ( surgeon->deleted )
        continue;

	if ( IS_NPC( surgeon ) &&
         surgeon->spec_fun == spec_lookup( "spec_utility_orthopedic_surgeon" ) )
        {
         yes = TRUE;
         break;
        }
    }

    if ( !yes || IS_AFFECTED( surgeon, AFF_CHARM ) )
    {
	return NULL;
    }

    return surgeon;
}

CHAR_DATA *find_l_surgeon( CHAR_DATA *ch )
{
    CHAR_DATA *surgeon;
    bool      yes = FALSE;

    for ( surgeon = ch->in_room->people; surgeon; surgeon = surgeon->next_in_room )
    {
      if ( surgeon->deleted )
        continue;

	if ( IS_NPC( surgeon ) &&
             surgeon->spec_fun == spec_lookup( "spec_utility_laser_surgeon" ) )
        {
         yes = TRUE;
         break;
        }
    }

    if ( !yes || IS_AFFECTED( surgeon, AFF_CHARM ) )
	return NULL;

    return surgeon;
}

CHAR_DATA *find_smith( CHAR_DATA *ch )
{
    CHAR_DATA *smith;
    bool      yes = FALSE;

    for ( smith = ch->in_room->people; smith; smith = smith->next_in_room )
    {
      if ( smith->deleted )
        continue;

	if ( IS_NPC( smith ) &&
             smith->spec_fun == spec_lookup( "spec_utility_smith" ) )
        {
         yes = TRUE;
         break;
        }
    }

    if ( !yes || IS_AFFECTED( smith, AFF_CHARM ) )
    {
	return NULL;
    }

    return smith;
}

MONEY_DATA *get_cost( CHAR_DATA *keeper, OBJ_DATA *obj, bool fBuy )
{
    SHOP_DATA *pShop;
    static MONEY_DATA new_cost;
    MONEY_DATA *base_cost;

    new_cost.gold = new_cost.silver = new_cost.copper = 0;

    if ( !obj || !( pShop = keeper->pIndexData->pShop ) )
        return NULL;

    base_cost = base_value( obj );


  /* Add in the "additional" object values */
    base_cost->gold += obj->cost.gold;
    base_cost->copper += obj->cost.copper;
    base_cost->silver += obj->cost.silver;


    if ( fBuy )
    {
     new_cost.gold   = base_cost->gold   * pShop->profit_buy / 100;
     new_cost.silver = base_cost->silver * pShop->profit_buy / 100;
     new_cost.copper = base_cost->copper * pShop->profit_buy / 100;

     new_cost.gold   =
      ((new_cost.gold * economy.cost_modifier[obj->item_type])/100);
     new_cost.silver =
      ((new_cost.silver * economy.cost_modifier[obj->item_type])/100);
     new_cost.copper = 
      ((new_cost.copper * economy.cost_modifier[obj->item_type])/100);
    }
    else 
    {
     int       itype;

     for ( itype = 0; itype < MAX_TRADE; itype++ )
     {
      if ( obj->item_type == pShop->buy_type[itype] )
      {
       new_cost.gold   = base_cost->gold   * pShop->profit_sell / 100;
       new_cost.silver = base_cost->silver * pShop->profit_sell / 100;
       new_cost.copper = base_cost->copper * pShop->profit_sell / 100;

       new_cost.gold   =
        ((new_cost.gold * economy.cost_modifier[obj->item_type])/100);
       new_cost.silver =
        ((new_cost.silver * economy.cost_modifier[obj->item_type])/100);
       new_cost.copper = 
        ((new_cost.copper * economy.cost_modifier[obj->item_type])/100);

       break;
      }
     }
    }

  /* Check for negative costs */  
  if ( new_cost.gold < 0 || new_cost.silver < 0 || new_cost.copper < 0 )
     return NULL;

  return &new_cost;
}

void do_gamble( CHAR_DATA *ch, char *argument )
{
    CASINO_DATA *pCasino;
    CHAR_DATA          *dealer;
    int		        wager;
    MONEY_DATA          cost;
    char buf[MAX_STRING_LENGTH];
    char arg1[ MAX_INPUT_LENGTH ];
    bool winner = FALSE;

    argument = one_argument( argument, arg1 );

    if ( arg1[0] == '\0' || !is_number( arg1 ) )
    {
     send_to_char( AT_YELLOW, "Syntax: gamble <wager>.\n\r", ch );
     return;
    }

    wager = atoi( arg1 );

    if ( !( dealer = find_dealer( ch ) ) )
    {
     send_to_char( AT_CYAN, "You need a casino dealer present to gamble.\n\r", ch );
     return;
    }

    pCasino = dealer->pIndexData->pCasino;

   if ( wager > pCasino->ante_max || wager < pCasino->ante_min )
   {
    sprintf( buf, "You can only wager %d to %d gold.\n\r",
	pCasino->ante_min, pCasino->ante_max );
    send_to_char( AT_WHITE, buf, ch );
    return;
   }

    if ( ( ch->money.gold*C_PER_G + ch->money.silver*S_PER_G + ch->money.copper )
       < ( wager*C_PER_G ) )
        {
 	    send_to_char(AT_CYAN, "You don't have that much money.\n\r", ch );
            return;  
        }

  

   if ( pCasino->game == CASINO_SIMPLE_DICE )
   {
    char arg2[ MAX_INPUT_LENGTH ];

    argument = one_argument( argument, arg2 );

    if ( arg2[0] == '\0' || !is_number( arg2 ) )
    {
     send_to_char( AT_YELLOW, "Syntax: gamble <wager> <dice value>.\n\r", ch );
     return;
    }

    winner = casino_simple_dice( ch, dealer, atoi( arg2 ) );

     if ( winner )
     {
      wager *= 2;
      send_to_char( AT_YELLOW, "We have a winner!\n\r", ch );
      sprintf( buf, "You have won %d gold.\n\r", wager );
      send_to_char( AT_YELLOW, buf, ch );

      cost.gold     = wager;
      cost.silver   = 0;
      cost.copper   = 0;

	add_money( &ch->money, &cost );
	spend_money( &dealer->money, &cost );	
     }
     else
     {
      send_to_char( AT_YELLOW, "House wins.\n\r", ch );
      sprintf( buf, "You have lost %d gold.\n\r", wager );
      send_to_char( AT_YELLOW, buf, ch );

      cost.gold     = wager;
      cost.silver   = 0;
      cost.copper   = 0;

	add_money( &dealer->money, &cost );
	spend_money( &ch->money, &cost );	
     }
    }

   if ( pCasino->game == CASINO_THREE_CARD_MONTY )
   {
    char arg2[ MAX_INPUT_LENGTH ];
    int  card;

    argument = one_argument( argument, arg2 );

    if ( arg2[0] == '\0' )
    {
     send_to_char( AT_YELLOW, "Syntax: gamble <wager> <card(left, middle, right)>.\n\r", ch );
     return;
    }

    if ( !str_cmp( arg2, "left" ) )
     card = 1;
    else if ( !str_cmp( arg2, "middle" ) )
     card = 2;
    else if ( !str_cmp( arg2, "right" ) )
     card = 3;
    else
    {
     send_to_char( AT_YELLOW, "Syntax: gamble <wager> <card(left, middle, right)>.\n\r", ch );
     return;
    }

    winner = casino_three_card_monty( ch, dealer, card );

     if ( winner )
     {
      wager *= 2;
      send_to_char( AT_YELLOW, "We have a winner!\n\r", ch );
      sprintf( buf, "You have won %d gold.\n\r", wager );
      send_to_char( AT_YELLOW, buf, ch );

      cost.gold     = wager;
      cost.silver   = 0;
      cost.copper   = 0;

	add_money( &ch->money, &cost );
	spend_money( &dealer->money, &cost );	
     }
     else
     {
      send_to_char( AT_YELLOW, "House wins.\n\r", ch );
      sprintf( buf, "You have lost %d gold.\n\r", wager );
      send_to_char( AT_YELLOW, buf, ch );

      cost.gold     = wager;
      cost.silver   = 0;
      cost.copper   = 0;

	add_money( &dealer->money, &cost );
	spend_money( &ch->money, &cost );	
     }
    }
   
 return;
}

void do_purchase( CHAR_DATA *ch, char *argument )
{
    TATTOO_ARTIST_DATA *pTattoo;
    TATTOO_DATA        *tattoo;
    CHAR_DATA          *artist;
    MONEY_DATA          cost;
    char arg [ MAX_INPUT_LENGTH ];

    smash_tilde( argument );
    strcpy( arg, argument );

    if ( longstring( ch, arg ) )
     return;

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_CYAN, 
"You need to include a short description of what you want the tattoo to look like.\n\r",
        ch );
	return;
    }

    if ( !( artist = find_artist( ch ) ) )
    {
     send_to_char( AT_CYAN, "You need a tattoo artist present to purchase a tattoo.\n\r", ch );
     return;
    }

    pTattoo = artist->pIndexData->pTattoo;

    if ( get_tattoo_char( ch, pTattoo->wear_loc ) != NULL )
    {
     send_to_char( AT_CYAN, "You already have a tattoo there.\n\r", ch );
     return;
    }

    cost.gold   = pTattoo->cost.gold;
    cost.silver = pTattoo->cost.silver;
    cost.copper = pTattoo->cost.copper;

    if ( ( ch->money.gold*C_PER_G + ch->money.silver*S_PER_G + ch->money.copper )
       < ( pTattoo->cost.gold*C_PER_G + pTattoo->cost.silver*S_PER_G + pTattoo->cost.copper ) )
        {
 	    send_to_char(AT_CYAN, "You can't afford the tattoo.\n\r", ch );
            return;  
        }


    tattoo = new_tattoo();
    tattoo->wear_loc = pTattoo->wear_loc;    
    tattoo->magic_boost = pTattoo->magic_boost;

    free_string( tattoo->short_descr );
    tattoo->short_descr = str_dup( arg );


    tattoo->affected = pTattoo->affected;

    act(AT_WHITE, "You have your body mutilated.", ch, NULL, NULL, TO_CHAR );
    act(AT_WHITE, "$n voluntarily has $s body mutilated.", ch, NULL, NULL, TO_ROOM );

    tattoo_to_char( tattoo, ch, FALSE );        
       
	add_money( &artist->money, &cost );
	spend_money( &ch->money, &cost );	

 return;
}


void do_buy( CHAR_DATA *ch, char *argument )
{
    char arg [ MAX_INPUT_LENGTH ];
    char arg1[MAX_STRING_LENGTH];
    int noi = 1;
    int in = 1;
    MONEY_DATA pet_cost;
    argument = one_argument( argument, arg );
    argument = one_argument( argument, arg1 );

    if ( arg[0] == '\0' )
    {
     send_to_char(AT_CYAN, "Buy what?\n\r", ch );
     return;
    }
    
    if ( arg1[0] == '\0' )
      noi = 1;
    else
      noi = atoi( arg1 );

    if ( IS_SET( ch->in_room->room_flags, ROOM_PET_SHOP ) )
    {
	CHAR_DATA       *pet;
	ROOM_INDEX_DATA *pRoomIndexNext;
	ROOM_INDEX_DATA *in_room;
	char             buf [ MAX_STRING_LENGTH ];

	if ( IS_NPC( ch ) )
	    return;

	if ( noi > 1 )
	{
	  send_to_char( AT_CYAN, "You can only buy one pet at a time.\n\r",ch);
	  return;
	}

	pRoomIndexNext = get_room_index( ch->in_room->vnum + 1 );
	if ( !pRoomIndexNext )
	{
	    bug( "Do_buy: bad pet shop at vnum %d.", ch->in_room->vnum );
	    send_to_char(AT_CYAN, "Sorry, you can't buy that here.\n\r", ch );
	    return;
	}

	in_room     = ch->in_room;
	ch->in_room = pRoomIndexNext;
	pet         = get_char_room( ch, arg );
	ch->in_room = in_room;

	if ( !pet || !IS_SET( pet->act, ACT_PET ) )
	{
	    send_to_char(AT_CYAN, "Sorry, you can't buy that here.\n\r", ch );
	    return;
	}

	if ( IS_SET( ch->act, PLR_BOUGHT_PET ) )
	{
	    send_to_char(AT_CYAN, "You already bought one pet this level.\n\r", ch );
	    return;
	}

	pet_cost.silver = pet_cost.copper = 0;	

	pet_cost.gold = ( 10 * pet->level * pet->level );
	
/* Convert ch coins and pet cost to copper to compare */
	if ( ( ch->money.gold*C_PER_G + ch->money.silver*S_PER_G + 
	       ch->money.copper ) < (pet_cost.gold*100) )
	{
 	    send_to_char(AT_CYAN, "You can't afford it.\n\r", ch );
            return;  
	}

	spend_money( &ch->money, &pet_cost );
        
	pet	  = create_mobile( pet->pIndexData );

	SET_BIT( ch->act,          PLR_BOUGHT_PET );
	SET_BIT( pet->act,         ACT_PET        );
	SET_BIT( pet->affected_by, AFF_CHARM      );

	argument = one_argument( argument, arg );
	if ( arg[0] != '\0' )
	{
	    sprintf( buf, "%s %s", pet->name, arg );
	    free_string( pet->name );
	    pet->name = str_dup( buf );
	}

	sprintf( buf, "%sA neck tag says 'I belong to %s'.\n\r",
		pet->description, ch->name );
	free_string( pet->description );
	pet->description = str_dup( buf );

	char_to_room( pet, ch->in_room );
	add_follower( pet, ch );
	send_to_char(AT_WHITE, "Enjoy your pet.\n\r", ch );
	act(AT_WHITE, "$n bought $N as a pet.", ch, NULL, pet, TO_ROOM );
	return;
    }
    else
    {
	OBJ_DATA  *obj;
	CHAR_DATA *keeper;
	MONEY_DATA *cost;
	bool	   haggled = FALSE;

 	if ( !( keeper = find_keeper( ch ) ) )
          return;

	obj  = get_obj_carry( keeper, arg );
	cost = get_cost( keeper, obj, TRUE );

    	if ( !cost
         || ( cost->gold == 0 && cost->silver == 0 && cost->copper == 0 )
         || !can_see_obj( ch, obj ) )
        {
         act(AT_CYAN, "$n tells you 'I don't sell that -- try 'list''.",
          keeper, NULL, ch, TO_VICT );
         ch->reply = keeper;
         return;
        }

 	cost->gold   *= noi;
        cost->silver *= noi;
        cost->copper *= noi; 
	        
	if ( !IS_NPC( ch )
         && ch->pcdata->learned[gsn_haggle] > 0
         && number_percent( ) < 35 )
        {                    
	 cost->copper *= 0.85;
	 cost->copper += (cost->silver * 0.15); 
         cost->silver *= 0.85;
         cost->silver += (cost->gold   * 0.15); 
         cost->gold   *= 0.85;
         haggled = TRUE;
         update_skpell( ch, gsn_haggle );
        }         

	if ( ch->charisma > 24 )
	{
	    cost->copper *= 0.80;
	    cost->silver *= 0.80;
	    cost->gold   *= 0.80;
	}
	else if ( ch->charisma > 19 ) 
	{
	   cost->copper *= 0.85;
	   cost->silver *= 0.85;
	   cost->gold   *= 0.85;
	}
	else if ( ch->charisma < 20 )
	{
	   cost->copper += (cost->copper * 0.15);
	   cost->silver += (cost->silver * 0.15);
	   cost->gold   += (cost->gold   * 0.15);
	}
	else if ( ch->charisma < 15 )
	{
         cost->copper += (cost->copper * 0.20);
	 cost->silver += (cost->silver * 0.20);
         cost->gold   += (cost->gold   * 0.20);
	}

  	if ( !IS_SET( obj->extra_flags, ITEM_INVENTORY ) && noi > 1 )
        {
         send_to_char( AT_WHITE, "You can only buy one of those at a time.\n\r", ch );
         return;
        }

        if ( noi < 1 )
        {
         send_to_char( AT_WHITE, "Buy how many?\n\r", ch );
         return;
        }  

	if ( ( ch->money.gold*C_PER_G + ch->money.silver*S_PER_G +
	       ch->money.copper ) < ( cost->gold*C_PER_G + cost->silver*S_PER_G +
	       cost->copper ) )
        {
          if ( noi == 1 )
            sprintf( log_buf, "$n tells you 'You can't afford to buy $p." );
          else
            sprintf( log_buf, "$n tells you 'You can't afford to buy %d $ps.",
                     noi );
          act(AT_CYAN, log_buf, keeper, obj, ch, TO_VICT );
          ch->reply = keeper;
          return;  
	}

  	if ( ch->carry_number + ( get_obj_number( obj ) * noi ) > can_carry_n( ch ) )
        {
            send_to_char(AT_CYAN, "You can't carry that many items.\n\r", ch );
            return;
        }

        if ( ch->carry_weight + ( get_obj_weight( obj ) * noi ) > can_carry_w( ch ) )
        {
            send_to_char(AT_CYAN, "You can't carry that much weight.\n\r", ch );
            return;   
 	}


        if ( haggled )
        {
          sprintf( log_buf, "You haggle with $N and pay %s", money_string( cost ) );
          act( AT_GREY, log_buf, ch, NULL, keeper, TO_CHAR );
          act( AT_GREY, "$n haggles with $N.", ch , NULL, keeper, TO_ROOM );
        }

        if ( noi == 1 )
        {                
 	  act(AT_WHITE, "You buy $p.", ch, obj, NULL, TO_CHAR );
          act(AT_WHITE, "$n buys $p.", ch, obj, NULL, TO_ROOM );
        }     
   	else
        {
          sprintf( log_buf, "You buy %d $p%s.", noi, ( noi > 1 ) ? "s" : "" );
          act(AT_WHITE, log_buf, ch, obj, NULL, TO_CHAR );
          sprintf( log_buf, "$n buys %d $p%s.", noi, ( noi > 1 ) ? "s" : "" );
          act(AT_WHITE, log_buf, ch, obj, NULL, TO_ROOM );
        }

        economy.item_type[obj->item_type] += noi;
        economy.market_type += noi;

	add_money( &keeper->money, cost );
	spend_money( &ch->money, cost );	
        
	if ( IS_SET( obj->extra_flags, ITEM_INVENTORY ) )
        {
          for ( in = 1; in <= noi; in++ )
          {
	    obj = create_object( obj->pIndexData, obj->level );
	    obj_to_char( obj, ch );
	  }
	}
       else
        {
	  obj_from_char( obj );
	  obj_to_char( obj, ch );
	}

	oprog_buy_trigger( obj, ch, keeper );
	return;
    }
}

void do_list( CHAR_DATA *ch, char *argument )
{
    char buf  [ MAX_STRING_LENGTH   ];
    char buf1 [ MAX_STRING_LENGTH*4 ];
    buf1[0] = '\0';

    if ( !str_cmp( argument, "casino" ) )
    {
	CASINO_DATA  *pCasino;
	CHAR_DATA           *dealer;

	if ( !( dealer = find_dealer( ch ) ) )
	    return;

        pCasino = dealer->pIndexData->pCasino;

	act(AT_WHITE, "$n tells you 'So, you like to gamble eh?'",
	    dealer, NULL, ch, TO_VICT );
	ch->reply = dealer;

        sprintf( buf,
  "%s tells you 'It so happens that I run a little game of &G%s&X here.'\n\r",
         dealer->short_descr, flag_string( casino_games, pCasino->game ) );

        send_to_char( AT_WHITE, buf, ch );

        sprintf( buf,
  "%s tells you 'You can bet anywhere from &C%d&X to &P%d&X &Ygold&X'\n\r",
         dealer->short_descr, pCasino->ante_min, pCasino->ante_max );

        send_to_char( AT_WHITE, buf, ch );

     return;
    }

    if ( !str_cmp( argument, "tattoo" ) )
    {
	AFFECT_DATA *paf;
	TATTOO_ARTIST_DATA  *pTattoo;
	CHAR_DATA           *artist;

	if ( !( artist = find_artist( ch ) ) )
	    return;

        pTattoo = artist->pIndexData->pTattoo;

	act(AT_WHITE, "$n tells you 'Boy have I got a deal for you!'",
	    artist, NULL, ch, TO_VICT );
	ch->reply = artist;


        if ( pTattoo->cost.gold != 0 && pTattoo->cost.copper != 0
          && pTattoo->cost.silver != 0 )
        sprintf( buf,"%s tells you 'For just &Y%d gold&X, &z%d silver&X and &O%d copper&X I can give you a tattoo on",
        artist->short_descr, pTattoo->cost.gold, pTattoo->cost.silver, pTattoo->cost.copper );
        else
        if ( pTattoo->cost.gold == 0 && pTattoo->cost.copper != 0
          && pTattoo->cost.silver != 0 )
        sprintf( buf,"%s tells you 'For just &z%d silver&X and &O%d copper&X I can give you a tattoo on",
        artist->short_descr, pTattoo->cost.silver, pTattoo->cost.copper );
        else
        if ( pTattoo->cost.gold != 0 && pTattoo->cost.copper != 0
          && pTattoo->cost.silver == 0 )
        sprintf( buf,"%s tells you 'For just &Y%d gold&X and &O%d copper&X I can give you a tattoo on",
        artist->short_descr, pTattoo->cost.gold, pTattoo->cost.copper );
        else
        if ( pTattoo->cost.gold == 0 && pTattoo->cost.copper != 0
          && pTattoo->cost.silver == 0 )
        sprintf( buf,"%s tells you 'For just &O%d copper&X I can give you a tattoo on",
        artist->short_descr, pTattoo->cost.copper );
        else
        if ( pTattoo->cost.gold != 0 && pTattoo->cost.copper == 0
          && pTattoo->cost.silver != 0 )
        sprintf( buf,"%s tells you 'For just &Y%d gold&X and &z%d silver&X I can give you a tattoo on",
        artist->short_descr, pTattoo->cost.gold, pTattoo->cost.silver );
        else
        if ( pTattoo->cost.gold != 0 && pTattoo->cost.copper == 0
          && pTattoo->cost.silver == 0 )
        sprintf( buf,"%s tells you 'For just &Y%d gold&X I can give you a tattoo on",
        artist->short_descr, pTattoo->cost.gold );
        else
        if ( pTattoo->cost.gold == 0 && pTattoo->cost.copper == 0
          && pTattoo->cost.silver != 0 )
        sprintf( buf,"%s tells you 'For just &z%d silver&X I can give you a tattoo on",
        artist->short_descr, pTattoo->cost.silver );
	else
        if ( pTattoo->cost.gold == 0 && pTattoo->cost.copper == 0
          && pTattoo->cost.silver == 0 )
        sprintf( buf,"%s tells you 'For free I can give you a tattoo on",
        artist->short_descr );

        send_to_char( AT_WHITE, buf, ch );

        sprintf( buf," your &G%s&X that would:'\n\r",
        flag_string( wear_tattoo, pTattoo->wear_loc ) );
        send_to_char( AT_WHITE, buf, ch );

       if ( pTattoo->magic_boost != 0 )
        {
        sprintf( buf, "Boost your &G%s&X magical power.\n\r",
        flag_string( tattoo_flags, pTattoo->magic_boost ) );
        send_to_char( AT_LBLUE, buf, ch );
        }	

    for ( paf = pTattoo->affected; paf; paf = paf->next )
    {
	if ( paf->location != APPLY_NONE && paf->modifier != 0 )
	{
	    sprintf( buf, "Affect your &G%s&X by &P%d&X.\n\r",
		    affect_loc_name( paf->location ), paf->modifier );
	    send_to_char(AT_LBLUE, buf, ch );
	}
    }
    return;
   }

    if ( find_artifactor( ch ) != NULL )
    {
      send_to_char( AT_YELLOW, "ARTIFACTOR PRICES\n\r", ch);
      send_to_char( AT_WHITE, "\n\rMake item Indestructable:\n\r", ch );
      send_to_char( AT_BLUE, "500 gold coins per item level.\n\r", ch );
      send_to_char( C_DEFAULT, "Indestructable <item>\n\r", ch );
      send_to_char( AT_WHITE, "\n\rRemake item:\n\r", ch );
      send_to_char( AT_BLUE, "5000 gold coins.\n\r", ch);
      send_to_char( C_DEFAULT, "Remake <item>\n\r", ch );
      send_to_char( AT_WHITE, "\n\rIdentify item:\n\r", ch );
      send_to_char( AT_BLUE, "3 gold coins.\n\r", ch);
      send_to_char( C_DEFAULT, "Identify <item>\n\r", ch );
      send_to_char( AT_WHITE, "\n\rBind item:\n\r", ch );
      send_to_char( AT_BLUE, "100 gold coins.\n\r", ch);
      send_to_char( C_DEFAULT, "Bind <item>\n\r", ch );
      return;
    }
    else
    if ( find_l_surgeon( ch ) != NULL )
    {
      send_to_char( AT_YELLOW, "Laser tattoo removal\n\r", ch);
      send_to_char( AT_WHITE, "\n\rSyntax:\n\r", ch );
      send_to_char( AT_BLUE, "surgery laser <body location>.\n\r", ch );
      send_to_char( C_DEFAULT, "Price: 2000 gold per tattoo\n\r", ch );
      send_to_char( AT_WHITE, "\n\rFor body locations, see help tattoo locations:\n\r", ch );
      return;
    }
    else
    if ( find_o_surgeon( ch ) != NULL )
    {
      send_to_char( AT_YELLOW, "Appendage reattachment\n\r", ch);
      send_to_char( AT_WHITE, "\n\rSyntax:\n\r", ch );
      send_to_char( AT_BLUE, "surgery ortho <limb name>.\n\r", ch );
      send_to_char( C_DEFAULT, "Price: 2000 gold per limb\n\r", ch );
      send_to_char( AT_WHITE, "\n\rLimb names: rarm, larm, rleg, lleg\n\r", ch );
      send_to_char( AT_BLUE, "See help surgery before proceeding.\n\r", ch);
      return;
    }
    else
    if ( find_smith( ch ) != NULL )
    {
      send_to_char( AT_YELLOW, "Weapon/armor repair\n\r", ch);
      send_to_char( AT_WHITE, "\n\rSyntax:\n\r", ch );
      send_to_char( AT_BLUE, "smith <object name>.\n\r", ch );
      send_to_char( C_DEFAULT, "Price: 250 gold per item\n\r", ch );
      return;
    }
    else if ( IS_SET( ch->in_room->room_flags, ROOM_PET_SHOP ) )
    {
	CHAR_DATA       *pet;
	ROOM_INDEX_DATA *pRoomIndexNext;
	bool             found;

	pRoomIndexNext = get_room_index( ch->in_room->vnum + 1 );
	if ( !pRoomIndexNext )
	{
	    bug( "Do_list: bad pet shop at vnum %d.", ch->in_room->vnum );
	    send_to_char(AT_CYAN, "You can't do that here.\n\r", ch );
	    return;
	}

	found = FALSE;
	for ( pet = pRoomIndexNext->people; pet; pet = pet->next_in_room )
	{
	    if ( IS_SET( pet->act, ACT_PET ) )
	    {
		if ( !found )
		{
		    found = TRUE;
		    strcat( buf1, "Pets for sale:\n\r" );
		}
		sprintf( buf, "[%2d] %5d Gold - %s\n\r",
			pet->level,
			10 * pet->level * pet->level,
			pet->short_descr );
		strcat( buf1, buf );
	    }
	}
	if ( !found )
	    send_to_char(AT_CYAN, "Sorry, we're out of pets right now.\n\r", ch );

	send_to_char(AT_CYAN, buf1, ch );
	return;
    }
    else
    {
	OBJ_DATA  *obj;
	CHAR_DATA *keeper;
	char       arg [ MAX_INPUT_LENGTH ];
	bool       found;
	MONEY_DATA *amt;
	one_argument( argument, arg );

	if ( !( keeper = find_keeper( ch ) ) )
	    return;

	found = FALSE;
	for ( obj = keeper->carrying; obj; obj = obj->next_content )
	{
	    amt = get_cost( keeper, obj, TRUE );

	    if ( !IS_SET( obj->wear_loc, WEAR_NONE ) || !amt )
		continue;

            if ( obj->item_type == ITEM_MONEY )
             continue;

	    if ( can_see_obj( ch, obj )
		&& ( arg[0] == '\0' || is_name(ch, arg, obj->name ) ) )
	    {
		if ( !found )
		{
		    found = TRUE;
		    strcat( buf1, "[&GLv &YGold &zSilv &OCopp&X] Item\n\r" );
		}
		sprintf( buf, "[&G%2d &Y%4d &z%4d &O%4d&X] %s.\n\r",
		     obj->level, amt->gold, amt->silver, amt->copper,
		     capitalize( obj->short_descr ) );
		strcat( buf1, buf );
	    }
	}

	if ( !found )
	{
	    if ( arg[0] == '\0' )
		send_to_char(AT_CYAN, "You can't buy anything here.\n\r", ch );
	    else
		send_to_char(AT_CYAN, "You can't buy that here.\n\r", ch );
	    return;
	}

	send_to_char(AT_CYAN, buf1, ch );
	return;
    }
}



void do_sell( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *obj;
    CHAR_DATA *keeper;
    char       buf [ MAX_STRING_LENGTH ];
    char       arg [ MAX_INPUT_LENGTH  ];
    MONEY_DATA *cost;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
     send_to_char(AT_CYAN, "Sell what?\n\r", ch );
     return;
    }

    if ( !( keeper = find_keeper( ch ) ) )
	return;

    if ( !( obj = get_obj_carry( ch, arg ) ) )
    {
     act(AT_CYAN, "$n tells you 'You don't have that item'.",
      keeper, NULL, ch, TO_VICT );
     ch->reply = keeper;
     return;
    }

    if ( !can_drop_obj( ch, obj ) )
    {
     send_to_char(AT_CYAN, "You can't let go of it.\n\r", ch );
     return;
    }

    if ( !can_see_obj( keeper, obj ) )
    {
     act(AT_CYAN, "$n tells you 'I can't see that item'.",
      keeper, NULL, ch, TO_VICT );
     ch->reply = keeper;
     return;
    }
    cost = get_cost( keeper, obj, FALSE );

    if ( ( !cost )
     || ( cost->gold == 0 && cost->silver == 0 && cost->copper == 0 )
     || obj->timer >= 0)
    {
     act(AT_CYAN, "$n looks uninterested in $p.", keeper, obj, ch, TO_VICT );
     return;
    }

    if ( !IS_NPC( ch )
     && ch->pcdata->learned[gsn_haggle] > number_percent() )
    {
     cost = base_value( obj );
     update_skpell( ch, gsn_haggle );
    }

    if ( get_curr_cha(ch) > 24 )
    {
     cost->gold   += ( cost->gold * 0.05 );
     cost->silver += ( cost->silver * 0.05 );
     cost->copper += ( cost->copper * 0.05 );
    }   
    else
    if ( get_curr_cha(ch) > 19 )
    {
     cost->gold   += ( cost->gold * 0.02 );
     cost->silver += ( cost->silver * 0.02 );
     cost->copper += ( cost->copper * 0.02 );
    }
    else
    if ( get_curr_cha(ch) < 20 )
    {
     cost->gold   -= ( cost->gold * 0.15 );
     cost->silver -= ( cost->silver * 0.15 );
     cost->copper -= ( cost->copper * 0.15 );
    }
    else
    if ( get_curr_cha(ch) < 15 )
    {
     cost->gold   -= ( cost->gold * 0.20 );
     cost->silver -= ( cost->silver * 0.20 );
     cost->copper -= ( cost->copper * 0.20 );
    }

    if ( cost->gold == 0 && cost->silver == 0 && cost->copper == 0 )
    {
     cost->copper = dice(1, 25);
     cost->silver = dice(1, 5);
    }

    sprintf( buf, "You sell $p for %s", money_string( cost ) );
    act(AT_WHITE, buf, ch, obj, NULL, TO_CHAR );
    act(AT_WHITE, "$n sells $p.", ch, obj, NULL, TO_ROOM );

    economy.item_type[obj->item_type] -= 1;
    economy.market_type -= 1;

    add_money( &ch->money, cost );
    sub_money( &keeper->money, cost );
    if ( keeper->money.gold < 0 ) 
      keeper->money.gold = 0;
    if ( keeper->money.silver < 0 )
      keeper->money.silver = 0;
    if ( keeper->money.copper < 0 )
      keeper->money.copper = 0;

    oprog_sell_trigger( obj, ch, keeper );

    if ( obj->item_type == ITEM_TRASH )
     extract_obj( obj );
    else
    {
	obj_from_char( obj );
	obj_to_char( obj, keeper );
    }

    return;
}



void do_value( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *obj;
    CHAR_DATA *keeper;
    char       buf [ MAX_STRING_LENGTH ];
    char       arg [ MAX_INPUT_LENGTH  ];
    MONEY_DATA *cost;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_CYAN, "Value what?\n\r", ch );
	return;
    }

    if ( !( keeper = find_keeper( ch ) ) )
	return;

    if ( !( obj = get_obj_carry( ch, arg ) ) )
    {
	act(AT_CYAN, "$n tells you 'You don't have that item'.",
	    keeper, NULL, ch, TO_VICT );
	ch->reply = keeper;
	return;
    }

    if ( !can_drop_obj( ch, obj ) )
    {
	send_to_char(AT_CYAN, "You can't let go of it.\n\r", ch );
	return;
    }

    if ( !can_see_obj( keeper, obj ) )
    {
        act(AT_CYAN, "$n tells you 'You are offering me an imaginary object!?!?'.",
            keeper, NULL, ch, TO_VICT );
        ch->reply = keeper;
        return;
    }
    cost = get_cost( keeper, obj, FALSE );

    if ( ( cost->gold + cost->silver + cost->copper ) <= 0 )
    {
     act(AT_CYAN, "$n looks uninterested in $p.", keeper, obj, ch, TO_VICT );
     return;
    }

    sprintf( buf, "$n tells you 'I'll buy $p for %s'", money_string( cost ) );
    act(AT_WHITE, buf, keeper, obj, ch, TO_VICT );
    ch->reply = keeper;

    return;
}

/* Poison weapon by Thelonius for EnvyMud */
/* I redid it so its now an occultist/necro skill -Flux */
void do_poison_weapon( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    OBJ_DATA *blood;
    char      arg [ MAX_INPUT_LENGTH ];

    if ( !IS_NPC( ch )                                                  
	&& !can_use_skpell( ch, gsn_poison_weapon ) )
    {
     typo_message( ch );                                          
     return;
    }

    one_argument( argument, arg );

    if ( arg[0] == '\0' )                                              
    { send_to_char(AT_DGREEN, "What are you trying to poison?\n\r",    ch ); return; }
    if ( ch->fighting )                                       
    { send_to_char(AT_DGREEN, "While you're fighting?  Nice try.\n\r", ch ); return; }
    if ( !( obj = get_obj_carry( ch, arg ) ) )
    { send_to_char(AT_DGREEN, "You do not have that weapon.\n\r",      ch ); return; }
    if ( obj->item_type != ITEM_WEAPON )
    { send_to_char(AT_DGREEN, "That item is not a weapon.\n\r",        ch ); return; }
    if ( IS_OBJ_STAT( obj, ITEM_POISONED ) )
    { send_to_char(AT_DGREEN, "That weapon is already poisoned.\n\r",  ch ); return; }
    if ( obj->durability <= 0 )
    { send_to_char(AT_DGREEN, "That weapon is broken.\n\r",  ch ); return; }

  if ( obj->value[8] == WEAPON_BASH || obj->value[8] == WEAPON_EXOTIC )
  {
   send_to_char( AT_YELLOW, "It must be a sharp weapon.\n\r", ch );
   return;
  }

  /* check to see if there's a pool of blood in the room */
  for ( blood = ch->in_room->contents; blood; blood = blood->next_content )
   {
    if ( !blood || blood->deleted )
     continue;

    if ( blood->item_type == ITEM_BLOOD )
     break;
   }

   if ( !blood || blood->deleted )
   {
    send_to_char(AT_DGREEN, "You need a pool of blood to poison that weapon.\n\r", ch );
    return;
   }

    WAIT_STATE( ch, skill_table[gsn_poison_weapon].beats );

    act(AT_GREEN, "You place $p into the blood and begin the incantation.",
	ch, obj, NULL, TO_CHAR );
    act(AT_GREEN, "$n places $p into a pool of blood and begins to chant.",
	ch, obj, NULL, TO_ROOM );

    /* Check the skill percentage */
    if ( !IS_NPC( ch )
	&& number_percent( ) > ch->pcdata->learned[gsn_poison_weapon] )
    {
     send_to_char(AT_DGREEN, "You failed the incantation and the weapon dissolves into the blood.\n\r",
      ch );
     act(AT_DGREEN, "$p dissolves into the pool of blood!",
      ch, obj, NULL, TO_ROOM );
     extract_obj( blood );
     extract_obj( obj );
     update_skpell( ch, gsn_poison_weapon );
     return;
    }

    act(AT_GREEN, "The blood is absorbed into $p, tainting it.",
	ch, obj, NULL, TO_CHAR  );
    act(AT_GREEN, "You get $p.",
	ch, obj, NULL, TO_CHAR  );
    act(AT_GREEN, "The pool of blood is absorbed by $p.",
	ch, obj, NULL, TO_ROOM  );
    /* Well, I'm tired of waiting.  Are you? */
    SET_BIT( obj->extra_flags, ITEM_POISONED );
    obj->cost.gold   *= 1 /* skill checks here */;
    obj->cost.silver *= 1;
    obj->cost.copper *= 1;

    /* WHAT?  All of that, just for that one bit?  How lame. ;) */
    update_skpell( ch, gsn_poison_weapon );
    extract_obj( blood );
    return;
}

void do_acmorph ( CHAR_DATA *ch, OBJ_DATA *obj, int  vnum )
{
    OBJ_INDEX_DATA *pObjIndex;
    OBJ_DATA       *nObj;
    int             level;
    
    level = 0;
    act( AT_BLUE, "You invoke $p.", ch, obj, NULL, TO_CHAR );
    act( AT_BLUE, "$n invokes $p.", ch, obj, NULL, TO_ROOM ); 
 
    if ( !(pObjIndex = get_obj_index( vnum ) ) )
       {
         act( AT_BLUE, "$p whines and sparks, but nothing happens", ch, obj, NULL, TO_CHAR );
         return;
       }
    level = pObjIndex->level;
    nObj = create_object( pObjIndex, level );
    if ( CAN_WEAR( nObj, ITEM_TAKE ) )
    {
	obj_to_char( nObj, ch );
    }
    else
    {
	obj_to_room( nObj, ch->in_room );
    }

 /* act(AT_BLUE, "$p's form wavers, then solidifies as $P.", ch, obj, nObj, TO_CHAR );
    act(AT_BLUE, "$n's $p wavers in form. then solidifies as $P.", ch, obj, nObj, TO_ROOM );
 */
    oprog_invoke_trigger( obj, ch, nObj );
    extract_obj( obj );
    return;    
}        
    
void do_acoload( CHAR_DATA *ch, OBJ_DATA *obj, int  vnum )
{
    OBJ_INDEX_DATA *pObjIndex;
    OBJ_DATA       *nObj;
    int             level;
    
    level = 0;
    act( AT_BLUE, "You invoke $p.", ch, obj, NULL, TO_CHAR );
    act( AT_BLUE, "$n invokes $p.", ch, obj, NULL, TO_ROOM ); 
 
    if ( !(pObjIndex = get_obj_index( vnum ) ) )
       {
         act( AT_BLUE, "$p whines and sparks, but nothing happens", ch, obj, NULL, TO_CHAR );
         return;
       }
    level = pObjIndex->level;
    nObj = create_object( pObjIndex, level );
    if ( CAN_WEAR( nObj, ITEM_TAKE ) )
    {
	obj_to_char( nObj, ch );
    }
    else
    {
	obj_to_room( nObj, ch->in_room );
    }
    act(AT_BLUE, "$p spawns $P.", ch, obj, nObj, TO_CHAR );
    act(AT_BLUE, "$n's $p spawns $P.", ch, obj, nObj, TO_ROOM );
    oprog_invoke_trigger( obj, ch, nObj );

    return;    
}        

void do_acmload( CHAR_DATA *ch, OBJ_DATA *obj, int vnum )
{
    CHAR_DATA      *victim;
    MOB_INDEX_DATA *pMobIndex;
    AFFECT_DATA af;
    
    act( AT_BLUE, "You invoke $p.", ch, obj, NULL, TO_CHAR );
    act( AT_BLUE, "$n invokes $p.", ch, obj, NULL, TO_ROOM ); 

    if ( !( pMobIndex = get_mob_index( vnum ) ) )
    {
         act( AT_BLUE, "$p whines and sparks, but nothing happens", ch, obj, NULL, TO_CHAR );
         return;
    }
    victim = create_mobile( pMobIndex );
    char_to_room( victim, ch->in_room );
    
    act(AT_BLUE, "$p spawns $N.", ch, obj, victim, TO_CHAR );
    act(AT_BLUE, "$n's $p spawns $N.", ch, obj, victim, TO_ROOM );
    if ( victim->master )
	stop_follower( victim );
    add_follower( victim, ch );
    af.type      = skill_lookup( "charm person" );
    af.duration  = 50;
    af.location  = APPLY_NONE;
    af.modifier  = 0;
    af.bitvector = AFF_CHARM;
    affect_to_char( victim, &af );
    oprog_invoke_trigger( obj, ch, victim );
    
    return;
}

void do_actrans( CHAR_DATA *ch, OBJ_DATA *obj, int vnum )
{
    ROOM_INDEX_DATA *location;

    act( AT_BLUE, "You invoke $p.", ch, obj, NULL, TO_CHAR );
    act( AT_BLUE, "$n invokes $p.", ch, obj, NULL, TO_ROOM ); 

    if ( ch->in_room->vnum == 8 )
    {
       send_to_char(AT_RED, "Such items do not work for those sent to Hell.\n\r", ch);
       return;
    }

    if ( !( location = get_room_index( vnum ) ) )
    {
	act(AT_BLUE, "$p whines and sparks, but nothing happens.", ch, obj, NULL, TO_CHAR );
	return;
    }

    if ( room_is_private( location ) )
    {
	send_to_char(AT_BLUE, "That room is private right now.\n\r", ch );
	return;
    }

    if ( ch->fighting )
    {
        act( AT_BLUE, "$p pulses lightly, but fail to function.", ch, obj, NULL, TO_CHAR );
        return;
    } 
    act(AT_BLUE, "Everything begins to spin, when it clears you are elsewhere.", ch, obj, NULL, TO_CHAR );
    act(AT_BLUE, "$n invokes $p and vanishes in a swirling red mist.", ch, obj, NULL, TO_ROOM );
    char_from_room( ch );
    char_to_room( ch, location );
    act(AT_BLUE, "$n arrives in a swirling red mist.", ch, obj, NULL, TO_ROOM);
    do_look( ch, "auto" );
    oprog_invoke_trigger( obj, ch, ch );
    return;
}

void do_activate( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA       *obj;
    CHAR_DATA      *rch;
    char            arg1 [ MAX_INPUT_LENGTH ];
    char            arg2 [ MAX_INPUT_LENGTH ];
    
    if ( IS_NPC(ch) )
      return;

    rch = get_char( ch );
    
    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );
 
    if ( !(obj = get_obj_carry( ch, arg1 ) ) && ( !(obj = get_obj_wear( ch, arg1 ) ) ) )
    {
	send_to_char( AT_WHITE, "You can't find it.\n\r", ch );
	return;
    }
    
     if ( IS_NPC( ch ) )
     {
	send_to_char(AT_BLUE, "You have not attained the level of mastery to use this item", ch );
	act(AT_BLUE, "$n tries to use $p, but is too inexperienced.",
	    ch, obj, NULL, TO_ROOM );
	return;
     }

 return;
}

void do_invoke( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA       *obj;
    CHAR_DATA      *rch;
    CHAR_DATA      *victim;
    char            arg1 [ MAX_INPUT_LENGTH ];
    char            arg2 [ MAX_INPUT_LENGTH ];
    char            spellarg [ MAX_INPUT_LENGTH ];
    
    if ( IS_NPC(ch) )
      return;

    rch = get_char( ch );
    
    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );
 
    if ( !(obj = get_obj_carry( ch, arg1 ) ) && ( !(obj = get_obj_wear( ch, arg1 ) ) ) )
    {
	send_to_char( AT_WHITE, "You can't find it.\n\r", ch );
	return;
    }
    
    if ( obj->invoke_type <= 0 || obj->invoke_type >= 6 )
    {
        act( AT_WHITE, "$p cannot be invoked.", ch, obj, NULL, TO_CHAR );
        return;
    }
    if ( obj->invoke_type == 5 && !obj->invoke_spell )
    {
      sprintf( log_buf, "Obj[%d] AcType Spell with no Spellname",
         obj->pIndexData->vnum );
      bug( log_buf, 0 );
      act( AT_WHITE, "$p cannot be invoked.", ch, obj, NULL, TO_CHAR );
      return;
    }
    
    if ( arg2[0] == '\0' )
       victim = rch;
    else
     ;
/*
       if ( !(victim = get_char_world( ch, arg2 ) ) )
          {
           send_to_char( AT_WHITE, "There is no such person in existance.\n\r", ch );
           return;
          }
*/

    switch ( obj->invoke_type )
    {
    default:   break;
    case 1:    do_acoload( ch, obj, obj->invoke_vnum ); break;
    case 2:    do_acmload( ch, obj, obj->invoke_vnum ); break;
    case 3:    do_actrans( ch, obj, obj->invoke_vnum ); break;
    case 4:    do_acmorph( ch, obj, obj->invoke_vnum ); break;
    case 5:    
        {
         spellarg[0] = '\0';
         sprintf( spellarg, "'%s' %s", obj->invoke_spell, arg2 );
         do_acspell( ch, obj, spellarg );
         break;
        }
    }
    if ( obj->invoke_charge[1] != -1 )
    if ( -- obj->invoke_charge[0] <= 0 )
    {
	act(AT_WHITE, "Your $p sputters and sparks.", ch, obj, NULL, TO_CHAR );
	act(AT_WHITE, "$n's $p sputters and sparks..", ch, obj, NULL, TO_ROOM );
	obj->invoke_type = 0;
        free_string( obj->invoke_spell );
	obj->invoke_spell = &str_empty[0];
	obj->invoke_vnum = 0;
    }

    return;
}      

void do_voodo ( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA   *obj;
    CHAR_DATA  *victim;
    char        buf [MAX_STRING_LENGTH];
    char        arg [MAX_INPUT_LENGTH];
    char       *name;
    
    buf[0] = '\0';
    one_argument( argument, arg );
    if ( !(victim = get_char_world( ch, arg ) ) )
       {
        send_to_char( AT_RED, "No such person exists.", ch );
        return;
       }

    if ( ( !IS_NPC(victim) ) && ( victim->clan == 0 ) )
     {
      send_to_char( AT_RED, "Not on the unclanned.\n\r", ch );
      return;
     } 
    if (IS_NPC(victim))
       name	  = victim->short_descr;
    else
       name       = victim->name;
    obj = create_object ( get_obj_index( OBJ_VNUM_DOLL ), 0 );
    sprintf( buf, obj->short_descr, name );
    free_string( obj->short_descr );
    obj->short_descr = str_dup( buf );
    free_string( obj->name );
    obj->name = str_dup( arg );
    obj->timer = 10;
    obj_to_char(obj, ch);
    act(AT_RED, "You call upon the dark forces of Retribution to create $p.", ch, obj, NULL, TO_CHAR );
    act(AT_RED, "$n calls upon the dark forces of Retribution to create $p.", ch, obj, NULL, TO_ROOM );
    return;

}

void do_deposit( CHAR_DATA *ch, char *argument )
{
  char arg[MAX_STRING_LENGTH];
  CLAN_DATA *pClan;
  bool clan_bank = FALSE;
  char arg2 [ MAX_STRING_LENGTH ];
  char arg3 [ MAX_STRING_LENGTH ];
  char buf  [ MAX_STRING_LENGTH ];
  MONEY_DATA amount;
  
  if (IS_NPC( ch ) )
    return;
  
  if ( !IS_SET( ch->in_room->room_flags, ROOM_BANK ) )
  {
    send_to_char( AT_WHITE, "You are not in a bank!\n\r", ch );
    return;
  }

  pClan = get_clan_index( ch->clan );  
  argument = one_argument( argument, arg );
/* deposit gold, silver, copper <amount> or deposits all */

  amount.gold = amount.silver = amount.copper = 0;
  argument = one_argument( argument, arg2 );
  argument = one_argument( argument, arg3 );

  if ( is_number( arg2 ) )
  {
    if ( !str_cmp( arg, "gold" ) )
      amount.gold = atoi( arg2 );
    else if ( !str_cmp( arg, "silver" ) )
      amount.silver = atoi( arg2 );
    else if ( !str_cmp( arg, "copper" ) )
      amount.copper = atoi( arg2 );
    else if ( str_cmp( arg, "all" ) )
    {
      send_to_char( AT_WHITE, "Invalid amount of money.\n\r", ch );
      send_to_char( AT_WHITE, "&WSyntax: deposit <currency_type> <amount>\n\r", ch );
      send_to_char( AT_WHITE, "&W    or: deposit <currency_type> <amount> clan\n\r", ch );
      return;
    }
  }  
  
  if ( ( amount.gold   > ch->money.gold   ) ||
       ( amount.silver > ch->money.silver ) ||
       ( amount.copper > ch->money.copper ) )
  {
    send_to_char( AT_WHITE, "You don't have that much money.\n\r", ch );
    return;
  }

  if ( ( !is_number( arg2 ) && ( str_cmp( arg, "all" ) ) ) ||
     ( atoi( arg2 ) < 0 ) )
  {
    send_to_char( AT_WHITE, "Invalid amount of money.\n\r", ch );
    send_to_char( AT_WHITE, "&WSyntax: deposit <currency_type> <amount>\n\r", ch );
    send_to_char( AT_WHITE, "&W    or: deposit <currency_type> <amount> clan\n\r", ch );
    return;
  }
  else
  {
   if ( !str_cmp( arg, "all" ) )
   {
     amount.gold   = ch->money.gold;
     amount.silver = ch->money.silver;
     amount.copper = ch->money.copper;
   }

  if ( !str_cmp( arg3, "clan" ) )
  {
    if (  ( IS_SET( ch->in_room->area->area_flags, AREA_CLAN_HQ ) ) &&
	  ( pClan != 0 ) )
    {
	add_money( &pClan->bankaccount, &amount );
	clan_bank = TRUE;
    }
    else
    {
	send_to_char( AT_WHITE, 
"You can only deposit into your clan bankaccount while you are at your clan head quarters' bank.\n\r",
	 	       ch );
	return;
    } 
  }
  else  add_money( &ch->pcdata->bankaccount, &amount );

  sub_money( &ch->money, &amount );     
  if ( clan_bank )
  {
    sprintf( buf, "You deposit into your clan bankaccount %s\n\r", money_string( &amount ) );
    sprintf( buf+strlen( buf ), "Your current clan balance is %s\n\r",
  	 money_string( &pClan->bankaccount ) );
    save_clans( );
  }
  else
  {
    sprintf( buf, "You deposit %s\n\r", money_string( &amount ) );
    sprintf( buf+strlen( buf ), "Your current balance is %s\n\r",
  	 money_string( &ch->pcdata->bankaccount ) );
  }   
   send_to_char( AT_WHITE, buf, ch );

  if ( clan_bank )
    sprintf( buf, "&w$n deposits into the clan bankaccount %s", money_string( &amount ) );
  else
    sprintf( buf, "&w$n deposits %s", money_string( &amount ) );

   act( AT_WHITE, buf, ch, NULL, NULL, TO_ROOM );

   return;

  }
  return;
}            

void do_withdraw( CHAR_DATA *ch, char *argument )
{
  char arg2 [ MAX_STRING_LENGTH ];
  char arg3 [ MAX_STRING_LENGTH ];
  char buf  [ MAX_STRING_LENGTH ];
  MONEY_DATA amount;
  CLAN_DATA *pClan;
  bool clan_bank = FALSE;
  char arg[MAX_STRING_LENGTH];
  
  if (IS_NPC( ch ) )
    return;
    
  if ( !IS_SET( ch->in_room->room_flags, ROOM_BANK ) )
  {
    send_to_char(AT_WHITE, "You are not in a bank!\n\r", ch );
    return;
  }

  pClan = get_clan_index( ch->clan );  
  argument = one_argument( argument, arg );
/* Withdraw gold, silver, copper <amount> or withdraw all */

  amount.gold = amount.silver = amount.copper = 0;
  argument = one_argument( argument, arg2 );
  argument = one_argument( argument, arg3 );
  
  if ( is_number( arg2 ) )
  {
    if ( !str_cmp( arg, "gold" ) )
      amount.gold = atoi( arg2 );
    else if ( !str_cmp( arg, "silver" ) )
      amount.silver = atoi( arg2 );
    else if ( !str_cmp( arg, "copper" ) )
      amount.copper = atoi( arg2 );
    else if ( str_cmp( arg, "all" ) )
    {
      send_to_char( AT_WHITE, "Invalid amount of money.\n\r", ch );
      send_to_char( AT_WHITE, "&WSyntax: withdraw <currency_type> <amount>\n\r", ch );
      send_to_char( AT_WHITE, "&W    or: withdraw <currency_type> <amount> clan\n\r", ch );
      return;
    }
  }

  if ( !str_cmp( arg3, "clan" ) )
  {
    if ( ( IS_SET( ch->in_room->area->area_flags, AREA_CLAN_HQ ) ) &&
      	 ( pClan != 0 ) )
    {
    	if ( ( amount.gold   > pClan->bankaccount.gold ) ||
	     ( amount.silver > pClan->bankaccount.silver ) ||
	     ( amount.copper > pClan->bankaccount.copper ) )
   	{
	   send_to_char( AT_WHITE, "You clan bankaccount doesn't hold that much money.\n\r", ch );
	   return;
	}
	else clan_bank = TRUE;
    }
    else
    {
	send_to_char( AT_WHITE, 
"You must be at your clan head quarters to withdraw money from the clan bankaccount.\n\r", ch );
	return;
    }
  }
  else
  {
    if ( ( amount.gold   > ch->pcdata->bankaccount.gold   ) ||
         ( amount.silver > ch->pcdata->bankaccount.silver ) ||
         ( amount.copper > ch->pcdata->bankaccount.copper ) )
    {
      send_to_char( AT_WHITE, "Your bank account doesn't hold that much money.\n\r", ch );
      return;
    }
  }

  if ( ( !is_number( arg2 ) && ( str_cmp( arg, "all" ) ) ) ||
     ( atoi( arg2 ) < 0 ) )
  {
    send_to_char( AT_WHITE, "Invalid amount of money.\n\r", ch );
    send_to_char( AT_WHITE, "&WSyntax: withdraw <currency_type> <amount>\n\r", ch );
    send_to_char( AT_WHITE, "&W    or: withdraw <currency_type> <amount> clan\n\r", ch );
    return;
  }
  else
  {
    if ( ( !str_cmp( arg, "all" ) ) && ( !clan_bank ) )
    {
      amount.gold   = ch->pcdata->bankaccount.gold;
      amount.silver = ch->pcdata->bankaccount.silver;
      amount.copper = ch->pcdata->bankaccount.copper;
    }
    else if ( ( !str_cmp( arg, "all" ) ) && ( clan_bank ) )
    {
      amount.gold   = pClan->bankaccount.gold;
      amount.silver = pClan->bankaccount.silver;
      amount.copper = pClan->bankaccount.copper;
    }

    add_money( &ch->money, &amount );
    if ( clan_bank )
    {
      sub_money( &pClan->bankaccount, &amount );
      sprintf( buf, "You withdraw from the clan bankaccount %s\n\r", money_string( &amount ) );
      send_to_char( AT_WHITE, buf, ch );
      sprintf( buf, "Your current clan balance is %s\n\r", money_string( &pClan->bankaccount ) );
      send_to_char( AT_WHITE, buf, ch );
      sprintf( buf, "&w$n withdraws from the clan bankaccount %s\n\r", money_string( &amount ) );
      act( AT_WHITE, buf, ch, NULL, NULL, TO_ROOM );
      save_clans( );
    }
    else
    {
      sub_money( &ch->pcdata->bankaccount, &amount );
      sprintf( buf, "You withdraw %s\n\r", money_string( &amount ) );
      send_to_char( AT_WHITE, buf, ch );
      sprintf( buf, "Your current balance is %s\n\r", 
	money_string( &ch->pcdata->bankaccount ) ); 
      send_to_char( AT_WHITE, buf, ch );
      sprintf( buf, "&w$n withdraws %s\n\r", money_string( &amount ) );
      act( AT_WHITE, buf, ch, NULL, NULL, TO_ROOM );
    }
    return;
  }

  return;
}

void do_smith( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *smith;
    OBJ_DATA  *obj;
    MONEY_DATA amount;  
    char	buf[MAX_STRING_LENGTH];

    if ( !(smith = find_smith( ch ) ) )
    { 
       send_to_char( AT_RED, "You can't do that here.\n\r", ch );
       return;
    } 

    if ( argument[0] == '\0' )
    {
     sprintf( buf, "%s says, 'What do you want to have repaired?'\n\r",
      smith->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

    if ( !( obj = get_obj_carry( ch, argument ) ) )
    {
     sprintf( buf, "%s says, 'You don't have that item.'\n\r",
      smith->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

  if ( obj->item_type != ITEM_ARMOR &&
       obj->item_type != ITEM_WEAPON )
  {
   sprintf( buf, "%s says, 'Only armor and weapons can be repaired.'\n\r",
    smith->short_descr );
   send_to_char( AT_WHITE, buf, ch );
   return;     
  }

  if ( obj->durability == 100 )
  {
   sprintf( buf, "%s says, 'That isn't even damaged.'\n\r",
    smith->short_descr );
   send_to_char( AT_WHITE, buf, ch );
   return;     
  }

  amount.gold = amount.silver = amount.copper = 0;
     amount.gold = 250;

  if ( ( (ch->money.gold*C_PER_G) + (ch->money.silver*S_PER_G) + 
	 (ch->money.copper) ) < amount.gold*C_PER_G )
  {
     sprintf( buf, "%s says, 'You can't afford to have that repaired.'\n\r",
      smith->short_descr );
        send_to_char(AT_WHITE, buf, ch );
     return;
  }
  spend_money( &ch->money, &amount );

     act(AT_WHITE,
         "$N takes $p from you and examines it.",
         ch, obj, smith, TO_CHAR );
     act(AT_WHITE,
         "$N takes $p from $n and examines it.",
         ch, obj, smith, TO_ROOM);

      act(AT_WHITE, 
          "$N grabs a hammer and starts pounding on $p.", 
          ch, obj, smith, TO_CHAR ); 
      act(AT_WHITE, 
          "$N grabs a hammer and starts pounding on $p.", 
          ch, obj, smith, TO_ROOM ); 
           
      act(AT_WHITE,
          "$N thrusts $p into the flames.",
          ch, obj, smith, TO_CHAR );
      act(AT_WHITE,
          "$N thrusts $p into the flames.",
          ch, obj, smith, TO_ROOM );

      act(AT_WHITE,
          "$N hands you back $p.",
          ch, obj, smith, TO_CHAR );
      act(AT_WHITE,
          "$N hands $n back $p.",
          ch, obj, smith, TO_ROOM );

  obj->durability = 100;

  return;
}

void do_account( CHAR_DATA *ch, char *argument )
{
/*  char arg [ MAX_STRING_LENGTH ]; */
    char buf [ MAX_STRING_LENGTH ];
    CLAN_DATA *clanacct;

  if (IS_NPC( ch ) )
    return;
  else
  {
    if ( !IS_SET( ch->in_room->room_flags, ROOM_BANK ) )
    {
      send_to_char(AT_WHITE, "You are not in a bank!\n\r", ch );
      return;
    }

/* Don't need to convert coins, just checking if char has any money */

    if ( ( ch->pcdata->bankaccount.gold + ch->pcdata->bankaccount.silver +
	   ch->pcdata->bankaccount.copper ) > 0 )
    {
       sprintf( buf, "Your account holds %s\n\r", money_string( &ch->pcdata->bankaccount ) ); 
       send_to_char( AT_WHITE, buf, ch );
    }
    else
    {
       int len = 0;
       len = strlen( ch->name );
       send_to_char(AT_WHITE, "You have nothing in your account!\n\r", ch );
       sprintf( buf, "&wFrom the shocked look on $n'%s face, you can tell that they have nothing in their account.",
                    ch->name[len] == 's' ? "" : "s" );
      act(AT_WHITE, buf, ch, NULL, NULL, TO_ROOM );
    }
  }

    clanacct = get_clan_index( ch->clan );

    if ( ( IS_SET( ch->in_room->area->area_flags, AREA_CLAN_HQ ) )
	&& ( clanacct != 0 ) )
    {
      if ( ( clanacct->bankaccount.gold + clanacct->bankaccount.silver +
	     clanacct->bankaccount.copper ) > 0 )
      {
 	 sprintf( buf, "%s's account holds %s\n\r", clanacct->name, 
		  money_string( &clanacct->bankaccount ) );
	 send_to_char( AT_WHITE, buf, ch );
	 return;
      }
      else
      {
	sprintf( buf, "%s's account is empty.\n\r", clanacct->name );
	send_to_char( AT_WHITE, buf, ch );
	return;
      } 
    }

  return;
}         
  
void do_separate( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *Obj;
  OBJ_DATA *aObj;
  OBJ_DATA *bObj;
  OBJ_INDEX_DATA *pIndex;

  if ( !( Obj = get_obj_carry( ch, argument ) ) )
  {
    send_to_char( AT_WHITE, "You are not carrying that item.\n\r", ch );
    return;
  }

  if ( !get_obj_index( Obj->pIndexData->sep_one ) ||
       !get_obj_index( Obj->pIndexData->sep_two ) )
  {
    send_to_char( AT_WHITE, "It cannot be separated.\n\r", ch );
    return;
  }

  pIndex = get_obj_index( Obj->pIndexData->sep_one );
  aObj = create_object( pIndex, pIndex->level );

  pIndex = get_obj_index( Obj->pIndexData->sep_two );
  bObj = create_object( pIndex, pIndex->level );
  sprintf( log_buf, "$n separates $p into %s and %s.\n\r",
           aObj->short_descr, bObj->short_descr );
  act( AT_WHITE, log_buf, ch, Obj, NULL, TO_ROOM );

  oprog_separate_trigger( Obj, ch );
  obj_from_char( Obj );
  extract_obj( Obj );
  obj_to_char( aObj, ch );
  obj_to_char( bObj, ch );
  send_to_char( AT_WHITE, "The object is now separated.\n\r", ch );
}

void do_join( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *aObj;
  OBJ_DATA *bObj;
  OBJ_DATA *Obj;
  OBJ_INDEX_DATA *pIndex;
  char arg1[MAX_INPUT_LENGTH];
  char arg2[MAX_INPUT_LENGTH];

  argument = one_argument( argument, arg1 );
  argument = one_argument( argument, arg2 );

  if ( !(aObj = get_obj_carry(ch, arg1)) )
  {
    char buf[MAX_STRING_LENGTH];

    sprintf( buf, "You are not carrying any %s.\n\r", arg1 );
    send_to_char( AT_WHITE, buf, ch );
    return;
  }

  if ( !(bObj = get_obj_carry(ch, arg2)) )
  {
    char buf[MAX_STRING_LENGTH];

    if (strlen( arg2 ) > 0 )
    {
    sprintf( buf, "You are not carrying any %s.\n\r", arg2 );
    send_to_char( AT_WHITE, buf, ch );
    return;
    }
    else 
    if (strlen( arg2 ) <= 0 )
    {
    send_to_char( AT_WHITE, "What's that?\n\r", ch );
    return;
    }
  }

  if ( aObj->pIndexData->join != bObj->pIndexData->join ||
       aObj->pIndexData == bObj->pIndexData || 
      !get_obj_index( aObj->pIndexData->join ) )
  {
    char buf[MAX_STRING_LENGTH];

    sprintf( buf, "%s cannot be joined with %s.\n\r",
	    capitalize( aObj->short_descr ), bObj->short_descr );
    send_to_char( AT_WHITE, buf, ch );
    return;
  }
  oprog_join_trigger( aObj, ch, bObj );
  pIndex = get_obj_index( aObj->pIndexData->join );
  Obj = create_object( pIndex, pIndex->level );
  obj_to_char( Obj, ch );
  sprintf( log_buf, "$n joins $p to $P to create %s.\n\r", Obj->short_descr );
  act( AT_WHITE, log_buf, ch, aObj, bObj, TO_ROOM );

  obj_from_char( aObj );
  extract_obj( aObj );
  obj_from_char( bObj );
  extract_obj( bObj );
  send_to_char( AT_WHITE, "Objects joined.\n\r", ch );
}

/*
 * -- Altrag
 */
void do_store( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *obj;
  MONEY_DATA amt;
  int storage = 1000;


  if ( IS_NPC( ch ) )
    return;

  if ( !IS_SET( ch->in_room->room_flags, ROOM_BANK ) )
  {
    send_to_char( AT_WHITE, "You must be in a bank to store items.\n\r", ch );
    return;
  }
  if ( argument[0] == '\0' )
  {
    send_to_char( AT_WHITE, "Your storage box contains:\n\r", ch );
    show_list_to_char( ch->pcdata->storage, ch, TRUE, TRUE );
    return;
  }

  if ( !( obj = get_obj_carry( ch, argument ) ) )
  {
    send_to_char( AT_WHITE, "You are not carrying that item.\n\r", ch );
    return;
  }

  if ( ch->pcdata->storcount >= 9 )
  {
    send_to_char( AT_WHITE,
		 "You may only have 10 items in your storage box.\n\r", ch );
    return;
  }

  if ( ( (ch->pcdata->bankaccount.gold*100) + (ch->pcdata->bankaccount.silver*10) +
         (ch->pcdata->bankaccount.copper) ) < storage*100 )
  {
    send_to_char( AT_WHITE, 
	 "Storing costs 1000 gold coins, which you do not have in your bank account.\n\r",
	  ch );
    return;
  }
  amt.silver = amt.copper = 0;
  amt.gold = 1000;
  spend_money( &ch->pcdata->bankaccount, &amt );  
  oprog_store_trigger( obj, ch );

  obj_from_char( obj );
  obj_to_storage( obj, ch );
  send_to_char( AT_WHITE, "The bank deducts 1000 gold coins from your account.\n\r", ch );
}

void do_retrieve( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *obj;

  if ( IS_NPC( ch ) )
    return;

  if ( argument[0] == '\0' )
  {
    send_to_char( AT_WHITE, "Retrieve what?\n\r", ch );
    return;
  }

  if ( !IS_SET( ch->in_room->room_flags, ROOM_BANK ) )
  {
    send_to_char(AT_WHITE, "You must be in a bank to retrieve items.\n\r", ch);
    return;
  }

  if ( !( obj = get_obj_storage( ch, argument ) ) )
  {
    send_to_char(AT_WHITE, "You do not have that object in storage.\n\r", ch);
    return;
  }
  obj_from_storage( obj );
  obj_to_char( obj, ch );
  oprog_retrieve_trigger( obj, ch );
  send_to_char( AT_WHITE, "You retrieve it from storage.\n\r", ch );
}

void do_carve( CHAR_DATA *ch, char *argument )
{
  AFFECT_DATA *pAf;
  OBJ_DATA *obj;
  OBJ_DATA *tools;    
  char arg[MAX_STRING_LENGTH];

  if ( IS_NPC( ch ) )
    return;

  if ( ch->pcdata->learned[gsn_carve] == 0 )
  {
    typo_message( ch );
    return;
  }

  for ( tools = ch->carrying; tools; tools = tools->next_content )
    {
     if ( !tools )
      continue;

    if ( tools->durability <= 0 )
     continue;

    if  ( (tools->item_type == ITEM_SMITHYPACK ) )
      break;
    }

    if ( !tools )
    {
    send_to_char( AT_GREY, "You need a pack of smithy tools to carve weapons.\n\r", ch );
    return;
    }  

  one_argument( argument, arg );

  if ( !( obj = get_obj_carry( ch, argument ) ) )
  {
    send_to_char(C_DEFAULT, "You do not have that item.\n\r",ch);
    return;
  }

  if ( obj->item_type != ITEM_WEAPON )
  {
    send_to_char(C_DEFAULT, "You may only carve weapons.\n\r",ch);
    return;
  }

  if ( obj->durability <= 0 )
  {
    send_to_char(C_DEFAULT, "You can't carve broken weapons.\n\r",ch);
    return;
  }

  if ( obj->value[8] != WEAPON_BASH )
  {
   send_to_char(C_DEFAULT, "You can only carve bash type weaponry.\n\r",ch);
   return;
  }

  if ( obj->composition != MATERIAL_WOOD &&
       obj->composition != MATERIAL_BONE )
  {
   send_to_char(C_DEFAULT, "You can only carve wood and bone weaponry.\n\r",ch);
   return;
  }

  obj->value[8] = WEAPON_BLADE;
  obj->value[7] = DAMNOUN_SLASH;

    pAf             =   new_affect();
    pAf->location   =   APPLY_DAMROLL;
    pAf->modifier   =   5;
    pAf->type       =   -1;
    pAf->duration   =   -1;
    pAf->next       =   obj->affected;
    obj->affected  =   pAf;

  act(AT_WHITE,"$n carves $p to a sharp edge.",ch,obj,NULL,TO_ROOM);
  act(AT_WHITE,"You carve $p to a sharp edge.",ch,obj,NULL,TO_CHAR);

  if ( number_percent() > ch->pcdata->learned[gsn_carve] )
  {
   act(AT_WHITE,"$n dulls $s tools carving $p.",ch,obj,NULL,TO_ROOM);
   act(AT_WHITE,"You dull your tools carving $p.",ch,obj,NULL,TO_CHAR);
   tools->durability -= dice(1,20);
  }

  update_skpell( ch, gsn_carve );
  return;
}


void do_sharpen( CHAR_DATA *ch, char *argument )
{
  AFFECT_DATA *pAf;
  OBJ_DATA *obj;
  OBJ_DATA *tools;    
  char arg[MAX_STRING_LENGTH];

  if ( IS_NPC( ch ) )
    return;

  if ( ch->pcdata->learned[gsn_sharpen] == 0 )
  {
    typo_message( ch );
    return;
  }

  for ( tools = ch->carrying; tools; tools = tools->next_content )
  {
   if ( !tools )
    continue;

    if ( tools->durability <= 0 )
     continue;

   if  ( (tools->item_type == ITEM_SMITHYPACK ) )
    break;
  }

  if ( !tools )
  {
   send_to_char( AT_GREY, "You need a pack of smithy tools to sharpen weapons.\n\r", ch );
   return;
  }  

  one_argument( argument, arg );

  if ( !( obj = get_obj_carry( ch, argument ) ) )
  {
   send_to_char(C_DEFAULT, "You do not have that item.\n\r",ch);
   return;
  }

  if ( obj->item_type != ITEM_WEAPON )
  {
   send_to_char(C_DEFAULT, "You may only sharpen weapons.\n\r",ch);
   return;
  }

  if ( IS_SET(obj->extra_flags, ITEM_SHARP) )
  {
   send_to_char(C_DEFAULT, "That weapon is as sharp as it can get.\n\r",ch);
   return;
  }

  if ( obj->durability <= 0 )
  {
   send_to_char(C_DEFAULT, "You can't sharpen broken weapons.\n\r",ch);
   return;
  }

  if ( obj->value[8] != WEAPON_SLASH &&
       obj->value[8] != WEAPON_PIERCE &&
       obj->value[8] != WEAPON_BLADE )
  {
   send_to_char(C_DEFAULT, "You can only sharpen slash, pierce or blade type weaponry.\n\r",ch);
   return;
  }

    pAf             =   new_affect();
    pAf->location   =   APPLY_DAMROLL;
    pAf->modifier   =   5;
    pAf->type       =   -1;
    pAf->duration   =   -1;
    pAf->next       =   obj->affected;
    obj->affected  =   pAf;

  SET_BIT(obj->extra_flags, ITEM_SHARP);
  act(AT_WHITE,"$n sharpen $p to a fine edge.",ch,obj,NULL,TO_ROOM);
  act(AT_WHITE,"You sharpen $p to a fine edge.",ch,obj,NULL,TO_CHAR);

  if ( number_percent() > ch->pcdata->learned[gsn_sharpen] )
  {
   act(AT_WHITE,"$n dulls $s tools sharpening $p.",ch,obj,NULL,TO_ROOM);
   act(AT_WHITE,"You dull your tools sharpening $p.",ch,obj,NULL,TO_CHAR);
   tools->durability -= dice(1,20);
  }

  update_skpell( ch, gsn_sharpen );
  return;
}

void do_serrate( CHAR_DATA *ch, char *argument )
{
  AFFECT_DATA *pAf;
  OBJ_DATA *obj;
  OBJ_DATA *tools;    
  char arg[MAX_STRING_LENGTH];

  if ( IS_NPC( ch ) )
    return;

  if ( ch->pcdata->learned[gsn_serrate] == 0 )
  {
    typo_message( ch );
    return;
  }

  for ( tools = ch->carrying; tools; tools = tools->next_content )
    {
     if ( !tools )
      continue;

    if ( tools->durability <= 0 )
     continue;

    if  ( (tools->item_type == ITEM_SMITHYPACK ) )
      break;
    }

    if ( !tools )
    {
    send_to_char( AT_GREY, "You need a pack of smithy tools to serrate weapons.\n\r", ch );
    return;
    }  

  one_argument( argument, arg );

  if ( !( obj = get_obj_carry( ch, argument ) ) )
  {
    send_to_char(C_DEFAULT, "You do not have that item.\n\r",ch);
    return;
  }

  if ( obj->item_type != ITEM_WEAPON )
  {
    send_to_char(C_DEFAULT, "You may only serrate weapons.\n\r",ch);
    return;
  }

  if ( obj->durability <= 0 )
  {
    send_to_char(C_DEFAULT, "You can't serrate broken weapons.\n\r",ch);
    return;
  }

  if ( obj->value[8] != WEAPON_SLASH &&
       obj->value[8] != WEAPON_BLADE )
  {
   send_to_char(C_DEFAULT, "You can only serrate slash and blade type weaponry.\n\r",ch);
   return;
  }

  obj->value[8] = WEAPON_TEAR;
  obj->value[3] = DAMNOUN_TEAR;

    pAf             =   new_affect();
    pAf->location   =   APPLY_DAMROLL;
    pAf->modifier   =   10;
    pAf->type       =   -1;
    pAf->duration   =   -1;
    pAf->next       =   obj->affected;
    obj->affected  =   pAf;

  act(AT_WHITE,"$n serrates $p.",ch,obj,NULL,TO_ROOM);
  act(AT_WHITE,"You serrates $p.",ch,obj,NULL,TO_CHAR);

  if ( number_percent() > ch->pcdata->learned[gsn_serrate] )
  {
   act(AT_WHITE,"$n dulls $s tools serrating $p.",ch,obj,NULL,TO_ROOM);
   act(AT_WHITE,"You dull your tools serrating $p.",ch,obj,NULL,TO_CHAR);
   tools->durability -= dice(1,20);
  }

  update_skpell( ch, gsn_carve );
  return;
}

void do_balance( CHAR_DATA *ch, char *argument )
{
  AFFECT_DATA *pAf;
  OBJ_DATA *obj;
  OBJ_DATA *tools;    
  char arg[MAX_STRING_LENGTH];

  if ( IS_NPC( ch ) )
    return;

  if ( ch->pcdata->learned[gsn_balance] == 0 )
  {
    typo_message( ch );
    return;
  }

  for ( tools = ch->carrying; tools; tools = tools->next_content )
    {
     if ( !tools )
      continue;

    if ( tools->durability <= 0 )
     continue;

    if  ( (tools->item_type == ITEM_SMITHYPACK ) )
      break;
    }

    if ( !tools )
    {
    send_to_char( AT_GREY, "You need a pack of smithy tools to balance weapons.\n\r", ch );
    return;
    }  

  one_argument( argument, arg );

  if ( !( obj = get_obj_carry( ch, argument ) ) )
  {
    send_to_char(C_DEFAULT, "You do not have that item.\n\r",ch);
    return;
  }

  if ( obj->item_type != ITEM_WEAPON )
  {
    send_to_char(C_DEFAULT, "You may only balance weapons.\n\r",ch);
    return;
  }

  if ( IS_SET(obj->extra_flags, ITEM_BALANCED) )
  {
    send_to_char(C_DEFAULT, "That weapon has perfect balance.\n\r",ch);
    return;
  }
 

  if ( obj->durability <= 0 )
  {
    send_to_char(C_DEFAULT, "You can't balance broken weapons.\n\r",ch);
    return;
  }

    pAf             =   new_affect();
    pAf->location   =   APPLY_DAMROLL;
    pAf->modifier   =   5;
    pAf->type       =   -1;
    pAf->duration   =   -1;
    pAf->next       =   obj->affected;
    obj->affected  =   pAf;

    pAf             =   new_affect();
    pAf->location   =   APPLY_HITROLL;
    pAf->modifier   =   10;
    pAf->type       =   -1;
    pAf->duration   =   -1;
    pAf->next       =   obj->affected;
    obj->affected  =   pAf;

  SET_BIT(obj->extra_flags, ITEM_BALANCED);
  act(AT_WHITE,"$n balances $p.",ch,obj,NULL,TO_ROOM);
  act(AT_WHITE,"You balance $p.",ch,obj,NULL,TO_CHAR);

  if ( number_percent() > ch->pcdata->learned[gsn_balance] )
  {
   act(AT_WHITE,"$n dulls $s tools balancing $p.",ch,obj,NULL,TO_ROOM);
   act(AT_WHITE,"You dull your tools balancing $p.",ch,obj,NULL,TO_CHAR);
   tools->durability -= dice(1,20);
  }

  update_skpell( ch, gsn_balance );
  return;
}

void do_repair( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *obj;
  OBJ_DATA *tools;    
  char arg[MAX_STRING_LENGTH];

  if ( IS_NPC( ch ) )
    return;

  if ( ch->pcdata->learned[gsn_repair] == 0 )
  {
    typo_message( ch );
    return;
  }

  for ( tools = ch->carrying; tools; tools = tools->next_content )
    {
     if ( !tools )
      continue;

    if ( tools->durability <= 0 )
     continue;

    if  ( (tools->item_type == ITEM_SMITHYPACK ) )
      break;
    }

    if ( !tools )
    {
    send_to_char( AT_GREY, "You need a pack of smithy tools to repair armor/weapons.\n\r", ch );
    return;
    }  

  one_argument( argument, arg );

  if ( !( obj = get_obj_carry( ch, argument ) ) )
  {
    send_to_char(C_DEFAULT, "You do not have that item.\n\r",ch);
    return;
  }

  if ( obj->item_type != ITEM_WEAPON && obj->item_type != ITEM_ARMOR )
  {
    send_to_char(C_DEFAULT, "You may only repair weapons and armor.\n\r",ch);
    return;
  }

  if ( obj->durability == 100 )
  {
    send_to_char(C_DEFAULT, "It already looks like new.\n\r",ch);
    return;
  }

	obj->durability += ch->pcdata->learned[gsn_repair] / 2;
        if ( obj->durability > 100 )
         obj->durability = 100;

  act(AT_WHITE,"$n repairs his $p a bit.",ch,obj,NULL,TO_ROOM);
  act(AT_WHITE,"You do your best to repair your $p.",ch,obj,NULL,TO_CHAR);

  if ( number_percent() > ch->pcdata->learned[gsn_repair] )
  {
   act(AT_WHITE,"$n dulls $s tools repairing $p.",ch,obj,NULL,TO_ROOM);
   act(AT_WHITE,"You dull your tools repairing $p.",ch,obj,NULL,TO_CHAR);
   tools->durability -= dice(1,20);
  }

  update_skpell( ch, gsn_repair );
  return;
}

void do_alchemy ( CHAR_DATA *ch, char *argument )
{
    char       buf[MAX_STRING_LENGTH];
    AFFECT_DATA af;
    int        sn;
    OBJ_DATA  *pobj;
    char       arg1[MAX_INPUT_LENGTH];
    OBJ_DATA  *cobj;
    OBJ_DATA  *fobj;
    int        mana;
    int        dam;
    int        chance;

        
    if ( IS_NPC(ch) )
       return;
     if ( !IS_NPC( ch )                                                  
	&& !can_use_skpell( ch, gsn_alchemy ) )
    {                                          
	send_to_char(AT_WHITE, "What do you think you are, a heretic?\n\r", ch );
	return;
    }
    one_argument( argument, arg1 );
    if ( arg1[0] == '\0' )                                              
    { send_to_char(AT_WHITE, "What spell do you wish to alchemy?\n\r",    ch ); return; }
    if ( ch->fighting )                                       
    { send_to_char(AT_WHITE, "While you're fighting?  Nice try.\n\r", ch ); return; }

    for ( pobj = ch->carrying; pobj; pobj = pobj->next_content )
    {
     if ( pobj->durability <= 0 )
      continue;

	if ( pobj->item_type == ITEM_BEAKER )
	    break;
    }
    if ( !pobj )
    {
	send_to_char(AT_WHITE, "You do not have the empty beaker.\n\r", ch );
	return;
    }
    for ( cobj = ch->carrying; cobj; cobj = cobj->next_content )
    {
     if ( cobj->durability <= 0 )
      continue;

	if ( cobj->item_type == ITEM_CHEMSET )
	    break;
    }
    if ( !cobj )
    {
	send_to_char(AT_WHITE, "You do not have the chemistry set.\n\r", ch );
	return;
    }
    for ( fobj = ch->carrying; fobj; fobj = fobj->next_content )
    {
     if ( fobj->durability <= 0 )
      continue;

	if  ( fobj->item_type == ITEM_BUNSEN ) 
	    break;
    }
    if ( !fobj )
    {
	send_to_char(AT_WHITE, "You do not have the bunsen burner.\n\r", ch );
	return;
    }
    
    /* K, now we have all the stuff... check to see if the cleric can cast
       the spell he wants.
    */
    
    if ( ( sn = skill_lookup( arg1 ) ) < 0
	|| !can_use_skpell( ch, sn ) 
	|| sn == skill_lookup( "true sight" ) )
    {
	send_to_char(AT_BLUE, "You can't do that.\n\r", ch );
	return;
    }
    mana = SPELL_COST( ch, sn );
    mana *= 2;
    if ( ch->mana < mana )
      {
        send_to_char(AT_WHITE, "You lack the mana to bind the elixer.\n\r", ch );
        return;
      }
    ch->mana -= mana;
    dam = 0 /* skill check here */;
    chance = ch->pcdata->learned[gsn_alchemy] 
	   - 0 /* skill check here */;
    
    if ( sn == skill_lookup( "aura of peace" ) )
       chance = 0;
    if ( sn == skill_lookup( "iceshield" ) )
       chance = 15;
    if ( number_percent( ) > chance )
    {
      sprintf( buf, "The %s potion explodes! causing %d damage!",
               skill_table[sn].name, dam );
      send_to_char(AT_RED, buf, ch );
      act(AT_RED, "$n's elixer explodes!", ch, NULL, NULL, TO_ROOM );
      extract_obj (pobj);

      damage_object( ch, cobj, DAM_HEAT, dice( 1, 250 ) );
      damage_object( ch, fobj, DAM_HEAT, dice( 1, 250 ) );
      damage ( ch, ch, dam, gsn_incinerate, DAM_HEAT, TRUE );
      af.type      = gsn_incinerate;
      af.level	   = 1 /* skill check here */;
      af.duration  = 5;
      af.location  = APPLY_NONE;
      af.modifier  = 0;
      af.bitvector = AFF_FLAMING;
      affect_join( ch, &af );
      return;
    }
    
    pobj->item_type = ITEM_POTION;
    pobj->value[1] = sn;
    pobj->value[0] = /* skill check */ 1; 
    pobj->timer = 60;
    pobj->level = 1 /* skill check here */;
    pobj->cost.gold = 1 /* skill check here */;
    sprintf( buf, "%s potion", skill_table[sn].name );
    free_string( pobj->short_descr );
    pobj->short_descr = str_dup (buf );
    sprintf( buf, "A potion of %s has been left here.", skill_table[sn].name );
    free_string( pobj->description );
    pobj->description = str_dup ( buf );
    if ( !is_name(NULL, pobj->name, skill_table[sn].name ) )
    {
      sprintf( buf, "%s %s", pobj->name, skill_table[sn].name );
      free_string( pobj->name );
      pobj->name = str_dup( buf );
    }
    act( AT_RED, "You deftly mix a $p.", ch, pobj, NULL, TO_CHAR );
    act( AT_RED, "$n mixes a $p.", ch, pobj, NULL, TO_ROOM );
    update_skpell( ch, gsn_alchemy );
    return;
}

void do_scribe( CHAR_DATA *ch, char *argument )
{
    char       buf[MAX_STRING_LENGTH];
    AFFECT_DATA af;
    int        sn;
    OBJ_DATA  *pobj;
    char       arg1[MAX_INPUT_LENGTH];
    OBJ_DATA  *cobj;
    OBJ_DATA  *fobj;
    int        mana;
    int        dam;
    int        chance;

        
    if ( IS_NPC(ch) )
       return;
     if ( !IS_NPC( ch )                                                  
	&& !can_use_skpell( ch, gsn_scribe ) )
    {                                          
	send_to_char(AT_WHITE, "What do you think you are, a mage?\n\r", ch );
	return;
    }
    one_argument( argument, arg1 );
    if ( arg1[0] == '\0' )                                              
    { send_to_char(AT_WHITE, "What spell do you wish to scribe?\n\r",    ch ); return; }
    if ( ch->fighting )                                       
    { send_to_char(AT_WHITE, "While you're fighting?  Nice try.\n\r", ch ); return; }

    for ( pobj = ch->carrying; pobj; pobj = pobj->next_content )
    {
     if ( pobj->durability <= 0 )
      continue;

	if (  pobj->item_type == ITEM_SCROLLPAPER )
	    break;
    }
    if ( !pobj )
    {
	send_to_char(AT_WHITE, "You do not have the blank parchment.\n\r", ch );
	return;
    }
    for ( cobj = ch->carrying; cobj; cobj = cobj->next_content )
    {
     if ( cobj->durability <= 0 )
      continue;

	if  ( cobj->item_type == ITEM_PEN ) 
	    break;
    }
    if ( !cobj )
    {
	send_to_char(AT_WHITE, "You do not have the pen.\n\r", ch );
	return;
    }
    for ( fobj = ch->carrying; fobj; fobj = fobj->next_content )
    {
     if ( fobj->durability <= 0 )
      continue;

	if  ( fobj->item_type == ITEM_INKCART ) 
	    break;
    }
    if ( !fobj )
    {
	send_to_char(AT_WHITE, "You do not have the ink.\n\r", ch );
	return;
    }
    
    /* K, now we have all the stuff... check to see if the mage can cast
       the spell he wants.
    */
    
    if ( ( sn = skill_lookup( arg1 ) ) < 0
	|| !can_use_skpell( ch, sn ) 
	|| sn == skill_lookup( "true sight" ) )
    {
	send_to_char(AT_BLUE, "You can't do that.\n\r", ch );
	return;
    }
    mana = SPELL_COST( ch, sn );
    mana *= 2;
    if ( ch->mana < mana )
      {
        send_to_char(AT_WHITE, "You lack the mana to bind the words to the parchment.\n\r", ch );
        return;
      }
    ch->mana -= mana;
    dam = 0 /* skill check here */;
    chance = ch->pcdata->learned[gsn_scribe] 
	   - 0 /* Skill check here */;
    
    if ( sn == skill_lookup( "shatter" ) )
       chance = 0;
    if ( number_percent( ) > chance )
    {
      sprintf( buf, "The %s scroll explodes! causing %d damage!",
               skill_table[sn].name, dam );
      send_to_char(AT_RED, buf, ch );
      act(AT_RED, "$n's scroll explodes!", ch, NULL, NULL, TO_ROOM );
      extract_obj (pobj);

      damage_object( ch, cobj, DAM_HEAT, dice( 1, 250 ) );
      damage_object( ch, fobj, DAM_HEAT, dice( 1, 250 ) );
      damage ( ch, ch, dam, gsn_incinerate, DAM_HEAT, TRUE );
      af.type      = gsn_incinerate;
      af.level	   = 1 /* skill check here */;
      af.duration  = 5;
      af.location  = APPLY_NONE;
      af.modifier  = 0;
      af.bitvector = AFF_FLAMING;
      affect_join( ch, &af );
      return;
    }
    
    pobj->item_type 	 = ITEM_SCROLL;
    pobj->value[1] = sn;
    pobj->value[0] = 1 /* skill check here */; 
    pobj->timer = 60;
    pobj->level = /* skill check */ 5;
    pobj->cost.gold = 1 /* Skill check here */;
    sprintf( buf, "scroll of %s", skill_table[sn].name );
    free_string( pobj->short_descr );
    pobj->short_descr = str_dup (buf );
    sprintf( buf, "A scroll of %s has been left here.", skill_table[sn].name );
    free_string( pobj->description );
    pobj->description = str_dup ( buf );
    if ( !is_name(NULL, pobj->name, skill_table[sn].name ) )
    {
      sprintf( buf, "scroll %s", skill_table[sn].name );
      free_string( pobj->name );
      pobj->name = str_dup( buf );
    }
    act( AT_RED, "You neatly scribe a $p.", ch, pobj, NULL, TO_CHAR );
    act( AT_RED, "$n scribes a $p.", ch, pobj, NULL, TO_ROOM );
    send_to_char(AT_RED, "You toss aside the empty ink container.\n\r", ch);
    extract_obj(fobj);
    update_skpell( ch, gsn_scribe );
    return;
}

void do_donate( CHAR_DATA *ch,char *argument )
{
    char arg         [MAX_INPUT_LENGTH];
    ROOM_INDEX_DATA  *donation_room;
    OBJ_DATA         *obj;
   
    one_argument( argument, arg );
 
    if ( arg[0] == '\0' )
    {
       send_to_char( AT_WHITE, "Donate What?\n\r", ch );
       return;
    }
 
    
    if ( !( obj = get_obj_carry( ch, argument ) ) )
    {
       send_to_char( AT_WHITE, "You do not have that item. \n\r", ch );
       return;
    }
 
    if ( !can_drop_obj( ch, obj ) )
    {
      send_to_char( AT_WHITE, "You can't let go of it.\n\r", ch );
      return;
    }
     
    if ( ch->race == RACE_HUMAN )
    {
     if ( ( donation_room = get_room_index( ROOM_HUMAN_DONATION ) ) == NULL )
     {
       bug( "Do_donate: invalid vnum for donation room.\n\r", 0 );
       send_to_char( AT_WHITE, "Donation failed.\n\r",ch);
       return;
     }
    }
    else   
    {
     if ( ( donation_room = get_room_index( ROOM_ELF_DONATION ) ) == NULL )
     {
       bug( "Do_donate: invalid vnum for donation room.\n\r", 0 );
       send_to_char( AT_WHITE, "Donation failed.\n\r",ch);
       return;
     }
    }


      obj_from_char( obj );
      obj->timer = 20;
      obj_to_room( obj, donation_room );
      act( AT_WHITE, "$n donates $p.", ch, obj, NULL, TO_ROOM );
      act( AT_WHITE, "You donate $p.", ch, obj, NULL, TO_CHAR );
      return;
}

void do_gravebind( CHAR_DATA *ch, char *argument )
{
   OBJ_DATA	*obj;
   char		arg [ MAX_INPUT_LENGTH ];
   int		hp;

   if ( !IS_NPC( ch )
   && !can_use_skpell( ch, gsn_gravebind ) )
   {
    typo_message(ch);
    return;
   }

   one_argument( argument, arg );

   if ( arg == '\0' )
   {
    send_to_char( AT_DGREY, "You must gravebind something.\n\r", ch );
    return;
   }

   obj = get_obj_list( ch, arg, ch->in_room->contents );

   if ( !obj )
   {
    send_to_char( AT_DGREY, "You can't find it.\n\r", ch );
    return;
   }

   if ( obj->item_type != ITEM_CORPSE_NPC )
   {
    send_to_char( AT_DGREY, "You can only gravebind corpses.\n\r", ch );
    return;
   }

   if ( ch->pcdata->learned[gsn_gravebind] < number_percent() )
   {
    send_to_char( AT_DGREY, 
     "You gravebind the corpse incorrectly and destroy it.\n\r", ch );
    act( AT_DGREY, "$n's gravebind of the $p fails and it is destroyed.",
     ch, obj, NULL, TO_ROOM );
    extract_obj( obj );
    return;
   }

   hp = 1 /* skill check */;
   ch->hit += hp;
   ch->hit = UMIN( MAX_HIT(ch), ch->hit );

   send_to_char( AT_DGREY, 
    "You gravebind the corpse, sucking away it's remaining life force.\n\r", ch );

   act( AT_DGREY,
    "The last of the $p's life force is sucked away by $n and then decays.",
    ch, obj, NULL, TO_ROOM );

   extract_obj( obj );
   update_skpell( ch, gsn_gravebind );
   return;
}

void do_mummify( CHAR_DATA *ch, char *argument )
{
   OBJ_DATA	*obj;
   char		arg [ MAX_INPUT_LENGTH ];
   char		buf [ MAX_STRING_LENGTH ];

   if ( !IS_NPC( ch )
   && !can_use_skpell( ch, gsn_mummify ) )
   {
    typo_message(ch);
    return;
   }

   one_argument( argument, arg );

   if ( arg == '\0' )
   {
    send_to_char( AT_DGREY, "You must target a corpse to mummify.\n\r", ch );
    return;
   }

   obj = get_obj_list( ch, arg, ch->in_room->contents );

   if ( !obj )
   {
    send_to_char( AT_DGREY, "You can't find it.\n\r", ch );
    return;
   }

   if ( obj->item_type != ITEM_RIGOR )
   {
    send_to_char( AT_DGREY, "You can only mummify stiff corpses.\n\r", ch );
    return;
   }

   if ( ch->pcdata->learned[gsn_mummify] < number_percent() )
   {
    send_to_char( AT_DGREY, 
     "You mummify the corpse incorrectly and destroy it.\n\r", ch );
    act( AT_DGREY, "$n's mummification of the $p fails and it is destroyed.",
     ch, obj, NULL, TO_ROOM );
    extract_obj( obj );
    update_skpell( ch, gsn_mummify );
    return;
   }

   send_to_char( AT_DGREY, "You mummify the corpse.\n\r", ch );
   act( AT_DGREY, "$n mummifies the $p.", ch, obj, NULL, TO_ROOM );

  obj->item_type = ITEM_MUMMY;
  obj->timer = dice( 5, ch->pcdata->learned[gsn_mummify] );

  sprintf( buf, "A mummified corpse" );
  free_string( obj->short_descr );
  obj->short_descr = str_dup( buf );

  sprintf( buf, "A mummified corpse lies here." );
  free_string( obj->description );
  obj->description = str_dup( buf );

  sprintf( buf, "mummy corpse" );
  free_string( obj->name );
  obj->name = str_dup( buf );

 update_skpell( ch, gsn_mummify );
 return;
}

void do_indestructable( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *arti;
    OBJ_DATA  *obj;
    MONEY_DATA amount;  
    char	buf[MAX_STRING_LENGTH];

    if ( !(arti = find_artifactor( ch ) ) )
    { 
       send_to_char( AT_RED, "You can't do that here.\n\r", ch );
       return;
    } 

    if ( argument[0] == '\0' )
    {
     sprintf( buf, "%s says, 'What do you want to make indestructable?'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

    if ( !( obj = get_obj_carry( ch, argument ) ) )
    {
     sprintf( buf, "%s says, 'You don't have that item.'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

  amount.gold = amount.silver = amount.copper = 0;
  if ( obj->level )
     amount.gold = obj->level * 500;
  else
     amount.gold = 500;

  if ( ( (ch->money.gold*C_PER_G) + (ch->money.silver*S_PER_G) + 
	 (ch->money.copper) ) < amount.gold*C_PER_G )
  {
     sprintf( buf, "%s says, 'You can't afford to make that indestructable.'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
     return;
  }
  spend_money( &ch->money, &amount );

     act(AT_WHITE,
         "$N takes $p from you and tosses it into a machine.",
         ch, obj, arti, TO_CHAR );
     act(AT_WHITE,
         "$N takes $p from $n and tosses it into a machine.",
         ch, obj, arti, TO_ROOM);

    if ( number_percent( ) < (85-(obj->level * .35)) )
    {
      obj->extra_flags |= ITEM_NO_DAMAGE;
      act(AT_WHITE, 
          "$N punches some buttons on a panel.", 
          ch, obj, arti, TO_CHAR ); 
      act(AT_WHITE, 
          "$N punches some buttons on a panel.", 
          ch, obj, arti, TO_ROOM ); 
           
      act(AT_WHITE,
          "$N opens the machine and removes $p.",
          ch, obj, arti, TO_CHAR );
      act(AT_WHITE,
          "$N opens the machine and removes $p.",
          ch, obj, arti, TO_ROOM );

      act(AT_WHITE,
          "$N hands you back $p.",
          ch, obj, arti, TO_CHAR );
      act(AT_WHITE,
          "$N hands $n back $p.",
          ch, obj, arti, TO_ROOM );

      return;
    }

      act(AT_WHITE,
          "The machine begins to shake.",
          ch, obj, arti, TO_CHAR );
      act(AT_WHITE,
          "The machine begins to shake.",
          ch, obj, arti, TO_ROOM );

      act(AT_RED,
          "An explosion occurs within the machine and it stops shaking.",
          ch, obj, NULL, TO_CHAR );
      act(AT_RED,
          "An explosion occurs within the machine and it stops shaking.",
          ch, obj, NULL, TO_ROOM );

      act(AT_RED,
          "$N curses as he grabs his tool kit and turns to repair the machine.",
          ch, NULL, arti, TO_CHAR );
      act(AT_RED,
          "$N curses as he grabs his tool kit and turns to repair the machine.",
          ch, NULL, arti, TO_ROOM );

  extract_obj(obj);
 return;
}

void do_bind( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *arti;
    OBJ_DATA  *obj;
    MONEY_DATA amount;  
    char	buf[MAX_STRING_LENGTH];

    if ( !(arti = find_artifactor( ch ) ) )
    { 
       send_to_char( AT_RED, "You can't do that here.\n\r", ch );
       return;
    } 

    if ( argument[0] == '\0' )
    {
     sprintf( buf, "%s says, 'What do you want to have bound?'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

    if ( !( obj = get_obj_carry( ch, argument ) ) )
    {
     sprintf( buf, "%s says, 'You don't have that item.'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

  amount.gold = amount.silver = amount.copper = 0;

     amount.gold =  100;

  if ( ( (ch->money.gold*C_PER_G) + (ch->money.silver*S_PER_G) + 
	 (ch->money.copper) ) < amount.gold*C_PER_G )
  {
     sprintf( buf, "%s says, 'You can't afford to have that bound.'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
     return;
  }
  spend_money( &ch->money, &amount );

     act(AT_WHITE,
         "$N takes $p from you and tosses it into a machine.",
         ch, obj, arti, TO_CHAR );
     act(AT_WHITE,
         "$N takes $p from $n and tosses it into a machine.",
         ch, obj, arti, TO_ROOM);

      act(AT_WHITE, 
          "$N pushes some buttons on a panel.", 
          ch, obj, arti, TO_CHAR ); 
      act(AT_WHITE, 
          "$N pushes some buttons on a panel.", 
          ch, obj, arti, TO_ROOM ); 
           
      act(AT_WHITE,
          "$N opens the machine and removes $p.",
          ch, obj, arti, TO_CHAR );
      act(AT_WHITE,
          "$N opens the machine and removes $p.",
          ch, obj, arti, TO_ROOM );

      act(AT_WHITE,
          "$N hands you back $p.",
          ch, obj, arti, TO_CHAR );
      act(AT_WHITE,
          "$N hands $n back $p.",
          ch, obj, arti, TO_ROOM );

      obj->ownedby = ch->name;

 return;
}
void do_identify( CHAR_DATA *ch, char *argument )
{
    AFFECT_DATA *paf;
    CHAR_DATA *arti;
    OBJ_DATA  *obj;
    MONEY_DATA amount;  
    char	buf[MAX_STRING_LENGTH];

    if ( !(arti = find_artifactor( ch ) ) )
    { 
       send_to_char( AT_RED, "You can't do that here.\n\r", ch );
       return;
    } 

    if ( argument[0] == '\0' )
    {
     sprintf( buf, "%s says, 'What do you want to identify'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

    if ( !( obj = get_obj_carry( ch, argument ) ) )
    {
     sprintf( buf, "%s says, 'You don't have that item.'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

  amount.gold = amount.silver = amount.copper = 0;

     amount.gold =  3;

  if ( ( (ch->money.gold*C_PER_G) + (ch->money.silver*S_PER_G) + 
	 (ch->money.copper) ) < amount.gold*C_PER_G )
  {
     sprintf( buf, "%s says, 'You can't afford to identify that.'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
     return;
  }
  spend_money( &ch->money, &amount );

     act(AT_WHITE,
         "$N takes $p from you and tosses it into a machine.",
         ch, obj, arti, TO_CHAR );
     act(AT_WHITE,
         "$N takes $p from $n and tosses it into a machine.",
         ch, obj, arti, TO_ROOM);

      act(AT_WHITE, 
          "$N pushes some buttons on a panel.", 
          ch, obj, arti, TO_CHAR ); 
      act(AT_WHITE, 
          "$N pushes some buttons on a panel.", 
          ch, obj, arti, TO_ROOM ); 
           
      act(AT_WHITE,
          "$N opens the machine and removes $p.",
          ch, obj, arti, TO_CHAR );
      act(AT_WHITE,
          "$N opens the machine and removes $p.",
          ch, obj, arti, TO_ROOM );

      act(AT_WHITE,
          "$N hands you back $p with a printout.",
          ch, obj, arti, TO_CHAR );
      act(AT_WHITE,
          "$N hands $n back $p with a printout.",
          ch, obj, arti, TO_ROOM );

    sprintf( buf,
	    "\n\rObject '&C%s&W' is owned by: &C%s&W and is type &C%s&W.\n\r"
            "Level &C%d&W with extra flags:&C %s\n\r",
	    obj->name, obj->ownedby,
	    item_type_name( obj ), obj->level,
	    obj->extra_flags ? extra_bit_name( obj->extra_flags ) : "" );
    send_to_char(AT_WHITE, buf, ch );

    show_obj_values( ch, obj, 1 );

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

void do_remake( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *arti;
    OBJ_DATA *obj;
    MONEY_DATA amount;
    char	buf[MAX_STRING_LENGTH];

    if ( !(arti = find_artifactor( ch )) )
    { 
       send_to_char( AT_RED, "You can't do that here.\n\r", ch );
       return;
    } 

    if ( argument[0] == '\0' )
    {
     sprintf( buf, "%s says, 'What do you want to remake?'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

    if ( !(obj = get_obj_carry( ch, argument ) ) )
    {
     sprintf( buf, "%s says, 'You don't have that item.'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
        return;
    }

  
  amount.silver = amount.copper = 0;
  amount.gold = 5000;

  if ( ( ch->money.gold*C_PER_G + ch->money.silver*S_PER_G +
	 ch->money.copper ) < amount.gold*C_PER_G )
    {
     sprintf( buf, "%s says, 'You can't afford to remake that.'\n\r",
      arti->short_descr );
        send_to_char(AT_WHITE, buf, ch );
       return;
    }
    spend_money( &ch->money, &amount );

     act(AT_WHITE,
         "$N tosses your $p into a machine.",
         ch, obj, arti, TO_CHAR );
     act(AT_WHITE,
         "$N tosses $n's $p into a machine.",
         ch, obj, arti,TO_ROOM);

     act(AT_WHITE,
         "$N says, '$n, fill out this form. Based on this information, I can change your $p.'",
         ch, obj, arti, TO_CHAR );
     act(AT_WHITE,
         "$N says, '$n, fill out this form. Based on this information, I can change your $p.'",
         ch, obj, arti,TO_ROOM);

     act(AT_YELLOW, 
         "As you begin to fill out the form, $N punches some buttons.",
         ch, obj, arti, TO_CHAR );

     act(AT_YELLOW, 
         "$n starts filling out the form while $N punches some buttons.",
         ch, obj, arti, TO_ROOM );

    do_rename_obj( ch, argument );

    return;
}

void do_antidote( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *flask;
  if ( IS_NPC( ch ) 
  || !can_use_skpell( ch, gsn_antidote ) )
    {
    typo_message( ch );
    return;
    }

  for ( flask = ch->carrying; flask; flask = flask->next_content )
    {
     if ( !flask )
      continue;

    if ( flask->durability <= 0 )
     continue;

    if  ( flask->item_type == ITEM_BEAKER )
      break;
    }

  if ( !flask )
    {
    send_to_char( AT_GREY, "You need a beaker to put your antidote in.\n\r", ch );
    return;
    }

  flask->item_type 	= ITEM_POTION;
  flask->value[1] = skill_lookup( "cure poison" );
  flask->value[0] = 1 /* skill checks */;
  flask->cost.gold = 0;
  free_string( flask->name );
  flask->name	  = str_dup( "antidote" );
  free_string( flask->short_descr );
  flask->short_descr = str_dup( "A flask of antidote" );
  free_string( flask->description );
  flask->description  = str_dup( "A small flask filled with a greenish liquid sets upon the floor." );
  send_to_char( AT_GREY, "You pour the antidote in the empty flask.\n\r", ch );
  act( AT_GREY, "$n pours some liquid into an empty flask.\n\r",
       ch, NULL, NULL, TO_ROOM );
  update_skpell( ch, gsn_antidote );
  return;
}

void do_timebomb( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *Obj;
  OBJ_DATA *timer;
  OBJ_DATA *pilewire;
  ROOM_INDEX_DATA *to_room;
  char buf[MAX_STRING_LENGTH];
  char arg[MAX_INPUT_LENGTH];
  int clock;

  one_argument( argument, arg );
  to_room = ch->in_room;
  clock = atoi(arg);

   if  ( !(Obj = get_eq_char( ch, WEAR_WIELD_2 )) )
   {
      send_to_char( C_DEFAULT, "You aren't holding anything in your secondary hand.\n\r", ch );
      return;
   }

   if ( Obj->item_type != ITEM_BOMB )
   {
    send_to_char( C_DEFAULT, "You aren't holding an explosive.\n\r", ch );
    return;
   }

    if ( IS_SET(ch->in_room->room_flags, ROOM_SAFE) )
    {
       send_to_char(AT_BLOOD, "You cannot set a bomb in a safe room.\n\r", ch );
       return;
    }


  for ( timer = ch->carrying; timer; timer = timer->next_content )
   {
    if ( !timer )
     continue;

    if ( timer->durability <= 0 )
     continue;

   if ( timer->item_type == ITEM_TIMER )
	break;
   }
  if ( !timer )
   {
   send_to_char( AT_GREY, "You need a timer to set a timebomb.\n\r", ch );
   return;
   }
  for ( pilewire = ch->carrying; pilewire; pilewire = pilewire->next_content )
   {
   if ( !pilewire )
    continue;

    if ( pilewire->durability <= 0 )
     continue;

   if ( pilewire->item_type == ITEM_PILEWIRE )
	break;
   }
  if ( !pilewire )
   {
   send_to_char( AT_GREY, "You need some wires and circuitry to set a timebomb.\n\r", ch );
   return;
   }

  if  ( clock < 1 )
  {
      send_to_char( C_DEFAULT, "You can't set a bomb for less than 1.\n\r", ch );
      return;
  }

  send_to_char( C_DEFAULT, "You set the timebomb, RUN!\n\r", ch );
	  sprintf( buf, 
      "%s drops a small device, which begins to making ticking noises.", ch->oname );
	act( AT_WHITE, buf, ch, Obj, NULL, TO_ROOM );
  unequip_char( ch, Obj );
  Obj->timer = clock;
  obj_from_char( Obj );
  obj_to_room( Obj, to_room );
  Obj->owner = ch;
  extract_obj( pilewire );
  extract_obj( timer );
  update_skpell( ch, gsn_timebomb );
  return;
}

void bomb_explode( OBJ_DATA *bomb, ROOM_INDEX_DATA *in_room ) 
{
  char buf[MAX_STRING_LENGTH];
  CHAR_DATA *vch;
  CHAR_DATA *vch_next;
  CHAR_DATA *owner = bomb->owner;
  bool seen_act = FALSE;
 /* This is so the sound of the explosion doesn't hit more than once */
  bool expsound = FALSE;  

  /* kjodo was here  */
    EXIT_DATA * pexit;
    ROOM_INDEX_DATA * room;
    int dir;
    int distance;
    int max_distance = 1;
   ROOM_INDEX_DATA *was_in_room;
    was_in_room = bomb->in_room;
   
    max_distance = bomb->value[3]/100;

    if ( max_distance < 1 )
     max_distance = 1;

    if ( max_distance > 10 )
     max_distance = 10;

    for (dir = 0; dir < 6; dir++) /* look in every direction */
    {
        room = was_in_room; /* starting point */

        for (distance = 1 ; distance <= max_distance; distance++)
        {
            pexit = room->exit[dir]; /* find the door to the next room */
            if ((pexit == NULL) || (pexit->to_room == NULL) ||
	        (exit_blocked(pexit, bomb->in_room) > EXIT_STATUS_OPEN))
                break; /* exit not there OR points to nothing OR is closed */

/*  ! */
            if ( in_room == NULL )
             continue;

     if ( !(IS_SET( in_room->room_flags, ROOM_SAFE )) )
     {
      for( vch = bomb->in_room->people; vch; vch = vch_next)
      {
       vch_next = vch->next_in_room;

       if ( vch == NULL )
        continue;
      
       if(vch->deleted)
        continue;

       if ( !(owner = get_char_world( vch, owner->name )) )
        owner = vch;

   if ( bomb->value[1] == BOMB_NAPALM )
   {
	    AFFECT_DATA af;

	  sprintf( buf, 
      "The $p explodes violently, sending flames in all directions!" );

     if ( !IS_NPC( vch ) )
       damage( owner, vch,
      number_range ( bomb->value[3], bomb->value[3] * 2 ) + number_range( 25, 50 ),
      gsn_molotov, DAM_HEAT, TRUE  );
     else
       damage( owner, vch,
      number_range ( bomb->value[3]*4, bomb->value[3] * 8 ) + number_range( 25, 50 ),
      gsn_molotov, DAM_HEAT, TRUE  );

                af.type      = gsn_incinerate;
                af.level     = bomb->level;
                af.duration  = bomb->level / 8;
                af.location  = APPLY_NONE;
                af.modifier  = 0;
                af.bitvector = AFF_FLAMING;
                affect_join( vch, &af );
                send_to_char(AT_RED, "You body bursts into flame!\n\r", vch);
    }

    if ( bomb->value[1] == BOMB_EXPLOSIVE )
    {
	  sprintf( buf, "The $p explodes violently!" );
     if ( !IS_NPC( vch ) )
     damage( owner, vch,
      number_range ( bomb->value[3], bomb->value[3] * 2 ) + number_range( 25, 50 ),
      gsn_pipebomb, DAM_HEAT, TRUE  );
     else
     damage( owner, vch,
      number_range ( bomb->value[3]*4, bomb->value[3] * 8 ) + number_range( 25, 50 ),
      gsn_pipebomb, DAM_HEAT, TRUE  );

          }

    if ( bomb->value[1] == BOMB_CHERRY )
    {
	  sprintf( buf, "The $p explodes violently!" );
     if ( !IS_NPC( vch ) )
     damage( owner, vch,
      number_range ( bomb->value[3]/2, bomb->value[3] ) + number_range( 25, 50 ),
      gsn_pipebomb, DAM_HEAT, TRUE  );
     else
     damage( owner, vch,
      number_range ( bomb->value[3]*2, bomb->value[3] * 4 ) + number_range( 25, 50 ),
      gsn_pipebomb, DAM_HEAT, TRUE  );
    }

   if (bomb->value[1] == BOMB_TEAR )
   {
     sprintf( buf,
      "The $p explodes, filling the room with a white gas!" );

      STUN_CHAR( vch, STUN_TOTAL, 5 );
      vch->position = POS_STUNNED;
   }

   if ( bomb->value[1] == BOMB_CHEMICAL)
   {
    AFFECT_DATA af;

	  sprintf( buf, 
	"The $p explodes, filling the room with a green gas!" );

      STUN_CHAR( vch, STUN_TOTAL, 5 );
      vch->position = POS_STUNNED;

	send_to_char(AT_GREEN, "You choke and gag.\n\r", vch );
	act(AT_GREEN, "$n chokes and gags.", vch, NULL, NULL, TO_ROOM );

	    af.type      = gsn_poison;
            af.level	 = bomb->level;
	    af.duration  = bomb->level;
	    af.location  = APPLY_STR;
	    af.modifier  = -2;
	    af.bitvector = AFF_POISON;
	    affect_join( vch, &af );

	    af.type      = gsn_plague;
            af.level	 = bomb->level;
	    af.duration  = bomb->level;
	    af.location  = APPLY_CON;
	    af.modifier  = -1;
	    af.bitvector = AFF_DISEASED;
	    affect_to_char2( vch, &af );
          }

     if ( !seen_act )
     {
      if (bomb->value[1] == BOMB_TEAR )
      {
       OBJ_DATA *cloud;

       cloud = create_object( get_obj_index( OBJ_VNUM_GAS_CLOUD ), 0 );

       free_string( cloud->short_descr );
       cloud->short_descr = str_dup("a white gas");
       free_string( cloud->name );
       cloud->name = str_dup("gas cloud");
       free_string( cloud->description );
       cloud->description = str_dup("An white haze lingers here.");

       cloud->timer = number_range( bomb->value[3]/8, bomb->value[3]/4 );
       cloud->level = bomb->level;
       cloud->value[1] = DAM_NONE;
       cloud->value[2] = 0;
       cloud->value[3] = GAS_AFFECT_STUN;
       obj_to_room( cloud, vch->in_room );
      }

      if ( bomb->value[1] == BOMB_CHEMICAL)
      {
       OBJ_DATA *cloud;
       cloud = create_object( get_obj_index( OBJ_VNUM_GAS_CLOUD ), 0 );

       free_string( cloud->short_descr );
       cloud->short_descr = str_dup("a yellow-green gas");
       free_string( cloud->name );
       cloud->name = str_dup("gas cloud");
       free_string( cloud->description );
       cloud->description = str_dup("An yellow-green gas lingers here.");

       cloud->timer = number_range( bomb->value[3]/6, bomb->value[3]/3 );
       cloud->level = bomb->level;
       cloud->value[1] = DAM_DEGEN;
       cloud->value[2] = bomb->value[3];
       cloud->value[3] = GAS_AFFECT_DISEASED;
       obj_to_room( cloud, vch->in_room );
      }
      act( AT_WHITE, buf, vch, bomb, NULL, TO_ROOM );
      seen_act = TRUE;
     }
    }
   }

  if ( bomb->in_room != NULL && !expsound )
  {
   char	     *msg;
   int	     door;
	     
   msg = "The sound of a huge explosion shakes the ground you stand on.";

    for ( door = 0; door <= 5; door++ )
    {
	EXIT_DATA *texit;

	if ( ( texit = was_in_room->exit[door] )
	    && texit->to_room
	    && texit->to_room != was_in_room )
	{
         obj_from_room( bomb );
         obj_to_room( bomb, texit->to_room );
	    act(AT_BLOOD, msg, NULL, bomb, NULL, TO_ROOM );
	}
    }
         obj_from_room( bomb );
         obj_to_room( bomb, was_in_room );
    expsound = TRUE;
   }
         room = pexit->to_room;
         obj_from_room( bomb );
         obj_to_room( bomb, room ); /* go to the next room */
        } /* for distance */
    } /* for dir */

  extract_obj( bomb );
 return;
}

void do_pull( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
     send_to_char( AT_BLUE, "What do you intend to pull?\n\r", ch );
     return;
    }

	obj = get_obj_list( ch, arg, ch->in_room->contents );

	if ( !obj )
	{
	 act(AT_WHITE, "I see no $T here.", ch, NULL, arg, TO_CHAR );
	 return;
	}
   
      if ( obj->item_type != ITEM_SWITCH ) 
      {
       send_to_char( AT_BLUE, "That isn't a switch.\n\r", ch );
       return;
      }

    if ( obj->value[0] != ACTIVATION_PULL )
    {
     send_to_char( AT_BLUE, "You can't pull that, try something else.\n\r", ch );
     return;
    }

    act( AT_WHITE, "You pull $p.", ch, obj, NULL, TO_CHAR );
    act( AT_WHITE, "$n pulls $p.", ch, obj, NULL, TO_ROOM );


   oprog_use_trigger( obj, ch, ch );
   activate_switch( ch, obj );
 return;
}

void do_press( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
     send_to_char( AT_BLUE, "What do you intend to press?\n\r", ch );
     return;
    }

	obj = get_obj_list( ch, arg, ch->in_room->contents );

	if ( !obj )
	{
	 act(AT_WHITE, "I see no $T here.", ch, NULL, arg, TO_CHAR );
	 return;
	}
   
      if ( obj->item_type != ITEM_SWITCH ) 
      {
       send_to_char( AT_BLUE, "That isn't a switch.\n\r", ch );
       return;
      }

    if ( obj->value[0] != ACTIVATION_PRESS )
    {
     send_to_char( AT_BLUE, "You can't press that, try something else.\n\r", ch );
     return;
    }

    act( AT_WHITE, "You press $p.", ch, obj, NULL, TO_CHAR );
    act( AT_WHITE, "$n presses $p.", ch, obj, NULL, TO_ROOM );


   oprog_use_trigger( obj, ch, ch );
   activate_switch( ch, obj );
 return;
}

void do_lift( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
     send_to_char( AT_BLUE, "What do you intend to lift?\n\r", ch );
     return;
    }

	obj = get_obj_list( ch, arg, ch->in_room->contents );

	if ( !obj )
	{
	 act(AT_WHITE, "I see no $T here.", ch, NULL, arg, TO_CHAR );
	 return;
	}
   
      if ( obj->item_type != ITEM_SWITCH ) 
      {
       send_to_char( AT_BLUE, "That isn't a switch.\n\r", ch );
       return;
      }

    if ( obj->value[0] != ACTIVATION_LIFT )
    {
     send_to_char( AT_BLUE, "You can't lift that, try something else.\n\r", ch );
     return;
    }

    act( AT_WHITE, "You lift $p.", ch, obj, NULL, TO_CHAR );
    act( AT_WHITE, "$n lifts $p.", ch, obj, NULL, TO_ROOM );


   oprog_use_trigger( obj, ch, ch );
   activate_switch( ch, obj );
 return;
}

void do_lower( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
     send_to_char( AT_BLUE, "What do you intend to lower?\n\r", ch );
     return;
    }

	obj = get_obj_list( ch, arg, ch->in_room->contents );

	if ( !obj )
	{
	 act(AT_WHITE, "I see no $T here.", ch, NULL, arg, TO_CHAR );
	 return;
	}
   
      if ( obj->item_type != ITEM_SWITCH ) 
      {
       send_to_char( AT_BLUE, "That isn't a switch.\n\r", ch );
       return;
      }

    if ( obj->value[0] != ACTIVATION_LOWER )
    {
     send_to_char( AT_BLUE, "You can't lower that, try something else.\n\r", ch );
     return;
    }

    act( AT_WHITE, "You lower $p.", ch, obj, NULL, TO_CHAR );
    act( AT_WHITE, "$n lowers $p.", ch, obj, NULL, TO_ROOM );


   oprog_use_trigger( obj, ch, ch );
   activate_switch( ch, obj );
 return;
}

void do_raise( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
     send_to_char( AT_BLUE, "What do you intend to raise?\n\r", ch );
     return;
    }

	obj = get_obj_list( ch, arg, ch->in_room->contents );

	if ( !obj )
	{
	 act(AT_WHITE, "I see no $T here.", ch, NULL, arg, TO_CHAR );
	 return;
	}
   
      if ( obj->item_type != ITEM_SWITCH ) 
      {
       send_to_char( AT_BLUE, "That isn't a switch.\n\r", ch );
       return;
      }

    if ( obj->value[0] != ACTIVATION_RAISE )
    {
     send_to_char( AT_BLUE, "You can't raise that, try something else.\n\r", ch );
     return;
    }

    act( AT_WHITE, "You raise $p.", ch, obj, NULL, TO_CHAR );
    act( AT_WHITE, "$n raises $p.", ch, obj, NULL, TO_ROOM );


   oprog_use_trigger( obj, ch, ch );
   activate_switch( ch, obj );
 return;
}

void activate_switch( CHAR_DATA *ch, OBJ_DATA *obj )
{
 ROOM_INDEX_DATA	*room = get_room_index(obj->value[2]);
 EXIT_DATA		*pexit;
 ROOM_INDEX_DATA	*to_room;
 EXIT_DATA		*pexit_rev;
 OBJ_DATA		*loadobj;
 CHAR_DATA		*loadmob;
 int			door;
 /* For noises -Flux. */
 char			*msg;
 bool			worked = FALSE;
 OBJ_DATA		*speaker = create_object( get_obj_index( 1 ), 1 );


 switch( obj->value[1] )
 {
  case SWITCH_NONE:
   send_to_char( AT_WHITE, "Nothing happens.", ch );
   break;

  case SWITCH_OLOAD:
   loadobj = create_object( get_obj_index(obj->value[3]), 1 );

   if ( !room || !loadobj )
    break;

   obj_to_room( loadobj, room );
   break;   

  case SWITCH_MLOAD:
   loadmob = create_mobile( get_mob_index(obj->value[3]) );

   if ( !room || !loadmob )
    break;

   char_to_room( loadmob, room );
   break;   

  case SWITCH_DOOR:
   pexit = room->exit[obj->value[3]];

   if ( !room || !pexit )
    break;

   to_room = pexit->to_room;
   pexit_rev = to_room->exit[rev_dir[obj->value[3]]];

   switch( obj->value[4] )
   {
    case DOOR_ACTION_NONE:
     break;

    case DOOR_ACTION_OPEN:  
     if ( !IS_SET( pexit->exit_info, EX_CLOSED )  )
	break;

	if (  IS_SET( pexit->exit_info, EX_LOCKED ) )
	 REMOVE_BIT( pexit->exit_info, EX_LOCKED );

     REMOVE_BIT( pexit->exit_info, EX_CLOSED );
     worked = TRUE;
     /* open the other side */
     if ( !to_room && !pexit_rev && pexit_rev->to_room == room )
	{
	    CHAR_DATA *rch;

	   if (  IS_SET( pexit_rev->exit_info, EX_LOCKED ) )
  	    REMOVE_BIT( pexit_rev->exit_info, EX_LOCKED );

	    REMOVE_BIT( pexit_rev->exit_info, EX_CLOSED );
	    for ( rch = to_room->people; rch; rch = rch->next_in_room )
	    {
		if ( rch->deleted )
		    continue;
		act(C_DEFAULT, "The $d opens.", rch, NULL, pexit_rev->keyword, TO_CHAR );
	    }
	}
     break;

    case DOOR_ACTION_CLOSE:
	if ( IS_SET( pexit->exit_info, EX_CLOSED ) )
	    break;

	if ( IS_SET( pexit->exit_info, EX_BASHED ) )
	    break;

	SET_BIT( pexit->exit_info, EX_CLOSED );
      worked = TRUE;
	/* close the other side */
     if ( !to_room && !pexit_rev && pexit_rev->to_room == room )
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
     break;

     case DOOR_ACTION_LOCK:
	if ( !IS_SET( pexit->exit_info, EX_CLOSED ) )
	 break; 
	if ( pexit->key <= 0 )
       break;
	if (  IS_SET( pexit->exit_info, EX_LOCKED ) )
       break;

	SET_BIT( pexit->exit_info, EX_LOCKED );
      worked = TRUE;
	/* lock the other side */
     if ( !to_room && !pexit_rev && pexit_rev->to_room == room )
	{
	    SET_BIT( pexit_rev->exit_info, EX_LOCKED );
	}
     break;

    case DOOR_ACTION_UNLOCK:
	if ( !IS_SET( pexit->exit_info, EX_CLOSED ) )
	 break;
	if ( pexit->key <= 0 )
	 break;
	if ( !IS_SET( pexit->exit_info, EX_LOCKED ) )
       break;

	REMOVE_BIT( pexit->exit_info, EX_LOCKED );
      worked = TRUE;
	/* unlock the other side */
     if ( !to_room && !pexit_rev && pexit_rev->to_room == room )
	{
	    REMOVE_BIT( pexit_rev->exit_info, EX_LOCKED );
	}
     break;
   }
   if ( worked )
   {
    msg = "The ground quakes as a low rumbling sound echoes around you.";

    for ( door = 0; door <= 5; door++ )
    {
	if ( ( pexit = room->exit[door] )
	    && pexit->to_room
	    && pexit->to_room != room )
	{
	    speaker->in_room = pexit->to_room;
	    act(AT_BLOOD, msg, NULL, speaker, NULL, TO_ROOM );
	}
    }
    act(AT_BLOOD, msg, ch, NULL, NULL, TO_CHAR );
   }
   break;
  }
 extract_obj( speaker );
 return;
}

/* For ranged weapons -Flux */
void do_reload( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    OBJ_DATA *ammo = NULL;
    OBJ_DATA *ammox;
    OBJ_DATA *clip = NULL;
    char arg[MAX_INPUT_LENGTH];
    char arg2[MAX_INPUT_LENGTH];
    char arg3[MAX_INPUT_LENGTH];
    bool ammocheck = FALSE;
    int  amountbullet = 0;
    int  clipnum = 0;
    int  amount = 0;

  argument = one_argument( argument, arg );
  argument = one_argument( argument, arg2 );
  argument = one_argument( argument, arg3 );

  if ( arg2[0] == '\0' )
  {
   send_to_char( AT_BLUE,
    "What are you trying to load?\n\r", ch );
   return;
  }

  if ( !( obj = get_obj_carry( ch, arg2 ) ) )
  {
   if ( !( obj = get_obj_wear( ch, arg2 ) ) )
   {
    send_to_char(AT_BLUE, "You do not have that item.\n\r", ch );
    return;
   }
  }

 if ( obj->item_type != ITEM_CLIP )
 {
  if ( obj->item_type != ITEM_RANGED_WEAPON )
  {
   send_to_char( C_DEFAULT, "That is not a ranged weapon.\n\r", ch );
   return;
  }

  if ( obj->value[0] != RANGED_WEAPON_FIREARM )
  {
   send_to_char( C_DEFAULT, "The only ranged weapon that can be loaded is a firearm.\n\r", ch );
   return;
  }

  if ( obj->durability <= 0 )
  {
   send_to_char(AT_BLUE,
    "That firearm is broken.\n\r", ch );
   return;
  }

  if ( arg[0] == '\0' )
  {
   send_to_char( AT_BLUE,
    "What do you plan to load into the gun?\n\r", ch );
   return;
  }

  if ( !( ammo = get_obj_carry( ch, arg ) ) )
  {
   send_to_char(AT_BLUE,
    "You are not holding the ammunition you refer to.\n\r", ch );
   return;
  }

   
  if ( ammo->durability <= 0 )
  {
   send_to_char(AT_BLUE,
    "That ammunition is broken.\n\r", ch );
   return;
  }

  if ( ammo->item_type != ITEM_BULLET && ammo->item_type != ITEM_CLIP )
  {
   send_to_char( C_DEFAULT, "That is neither a clip nor a bullet.\n\r", ch );
   return;
  }

  if ( ammo->value[1] != obj->value[1] )
  {
   send_to_char( C_DEFAULT,
    "The calibur of the ammunition does not equal the calibur of the gun.\n\r", ch );
   return;
  }

    for ( ammox = obj->contains; ammox; ammox = ammox->next_content )
    {
     if ( !ammox )
      continue;

     if ( ammox->item_type != ITEM_BULLET && ammox->item_type != ITEM_CLIP )
      continue;

     if ( ammox->item_type == ITEM_BULLET )
     {
      ammocheck = TRUE;
      break;
     }

     if ( ammox->item_type == ITEM_CLIP )
     {
      clip = ammox;

      if ( !clip->contains )
       continue;

      for ( ammox = clip->contains; ammox; ammox = ammox->next_content )
      {
       if ( ammox->item_type == ITEM_BULLET )
       {
        ammocheck = TRUE;
        break;
       }
      }
      if ( ammocheck == TRUE )
       break;
     }
    }

  if ( ammox != NULL )
  {
   if ( ammox->item_type == ITEM_BULLET && 
        ammo->item_type == ITEM_BULLET && !(clip) )
   {
    send_to_char( C_DEFAULT, "There is already a single bullet in the chamber.\n\r", ch );
    return;
   }

   if ( (clip) && ammo->item_type == ITEM_CLIP )
   {
    send_to_char( C_DEFAULT, "There is a clip loaded already.\n\r", ch );
    return;
   }
  }

   if ( ammo->item_type == ITEM_CLIP && ammo->pIndexData->vnum != obj->value[2] )
   {
    send_to_char( C_DEFAULT, "That clip does not go with this gun.\n\r", ch );
    return;
   }    

   obj_from_char( ammo );
   obj_to_obj( ammo, obj );
   act(AT_GREEN, "You load $p into $P.", ch, ammo, obj, TO_CHAR );
   act(AT_GREEN, "$n loads $p into $P.", ch, ammo, obj, TO_ROOM );
  }
  else if ( obj->item_type == ITEM_CLIP )
  {
    char buf[MAX_STRING_LENGTH];

  switch( obj->value[2] )
  {
   default:
    return;
   case RANGED_WEAPON_FIREARM:
    if ( arg[0] == '\0' )
    {
     send_to_char( AT_BLUE,
      "What do you plan to load into the clip?\n\r", ch );
     return;
    }

    if ( !( ammo = get_obj_carry( ch, arg ) ) )
    {
     send_to_char(AT_BLUE,
      "You are not holding the ammunition you refer to.\n\r", ch );
     return;
    }

    if ( ammo->item_type != ITEM_BULLET )
    {
     send_to_char( C_DEFAULT, "Firearm clips only take bullets.\n\r", ch );
     return;
    }

   if ( ammo->value[1] != obj->value[1] )
   {
    send_to_char( C_DEFAULT,
    "The calibur of the ammunition does not equal the calibur of the clip.\n\r", ch );
    return;
   }

    for ( ammox = obj->contains; ammox; ammox = ammox->next_content )
     clipnum += 1;

    if ( clipnum >= obj->value[0] )
    {
     send_to_char( C_DEFAULT, "That clip is full.\n\r", ch );
     return;
    }

    if ( !arg3[0] == '\0' && is_number(arg3) )
     amount = atoi(arg3);
    else
     amount = 1;

    amountbullet = amount;

    if ( amount > ( obj->value[0] - clipnum ) )
    {
     sprintf( buf,
      "You are trying to load too many bullets into the clip,"
      " it has room enough for %d.\n\r", obj->value[0] - clipnum );
    
     send_to_char( C_DEFAULT, buf, ch );
     return;
    }
   break;

   case RANGED_WEAPON_BOW:
    if ( arg[0] == '\0' )
    {
     send_to_char( AT_BLUE,
      "What do you plan to load into the quiver?\n\r", ch );
     return;
    }

    if ( !( ammo = get_obj_carry( ch, arg ) ) )
    {
     send_to_char(AT_BLUE,
      "You are not holding the arrow you refer to.\n\r", ch );
     return;
    }

    if ( ammo->item_type != ITEM_ARROW )
    {
     send_to_char( C_DEFAULT, "Bow quivers only take arrows.\n\r", ch );
     return;
    }

    if ( ammo->value[1] > obj->value[1] )
    {
     send_to_char( C_DEFAULT,
     "This quiver can not accomidate arrows of that size.\n\r", ch );
     return;
    }

    for ( ammox = obj->contains; ammox; ammox = ammox->next_content )
     clipnum += 1;

    if ( clipnum >= obj->value[0] )
    {
     send_to_char( C_DEFAULT, "That quiver is full.\n\r", ch );
     return;
    }

    if ( !arg3[0] == '\0' && is_number(arg3) )
     amount = atoi(arg3);
    else
     amount = 1;

    amountbullet = amount;

    if ( amount > ( obj->value[0] - clipnum ) )
    {
     sprintf( buf,
      "You are trying to load too many arrows into the quiver,"
      " it has room enough for %d.\n\r", obj->value[0] - clipnum );
    
     send_to_char( C_DEFAULT, buf, ch );
     return;
    }
   break;

   case RANGED_WEAPON_BAZOOKA:
    if ( arg[0] == '\0' )
    {
     send_to_char( AT_BLUE,
      "What do you plan to load into the ammo box?\n\r", ch );
     return;
    }

    if ( !( ammo = get_obj_carry( ch, arg ) ) )
    {
     send_to_char(AT_BLUE,
      "You are not holding the rocket you refer to.\n\r", ch );
     return;
    }

    if ( ammo->item_type != ITEM_BOMB || ammo->value[2] != PROP_LOCAL )
    {
     send_to_char( C_DEFAULT, "Bazooka ammo boxes only take bombs with local propulsion systems.\n\r", ch );
     return;
    }

    if ( ammo->value[3] > obj->value[1] )
    {
     send_to_char( C_DEFAULT,
     "This ammunition box can not hold rockets with warheads of that size.\n\r", ch );
     return;
    }

    for ( ammox = obj->contains; ammox; ammox = ammox->next_content )
     clipnum += 1;

    if ( clipnum >= obj->value[0] )
    {
     send_to_char( C_DEFAULT, "That ammo box is full.\n\r", ch );
     return;
    }

    if ( !arg3[0] == '\0' && is_number(arg3) )
     amount = atoi(arg3);
    else
     amount = 1;

    amountbullet = amount;

    if ( amount > ( obj->value[0] - clipnum ) )
    {
     sprintf( buf,
      "You are trying to load too many rockets into the ammo box,"
      " it has room enough for %d.\n\r", obj->value[0] - clipnum );
    
     send_to_char( C_DEFAULT, buf, ch );
     return;
    }
   break;
  }

   while( amount >= 1 )
   {
    if ( (ammo = get_obj_carry( ch, arg )) )
    {
     obj_from_char( ammo );
     obj_to_obj( ammo, obj );
    }
    amount -= 1;
   }
   sprintf( buf, "You load %d $p into $P.", amountbullet );
   act(AT_GREEN, buf, ch, ammo, obj, TO_CHAR );
   sprintf( buf, "$n loads %d $p into $P.", amountbullet );
   act(AT_GREEN, buf, ch, ammo, obj, TO_ROOM );
 }
 return;
}

void do_unload( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *ammo;
    OBJ_DATA *gun;
    char      arg[MAX_INPUT_LENGTH];
    bool      ammocheck = FALSE;

   argument = one_argument( argument, arg );

   if ( !( gun = get_obj_carry( ch, arg ) ) )
   {
    if ( !( gun = get_obj_wear( ch, arg ) ) )
    {
     send_to_char(AT_BLUE, "You do not have that ranged weapon.\n\r", ch );
     return;
    }
   }

    if ( gun->item_type != ITEM_RANGED_WEAPON )
    {
	send_to_char(AT_BLUE,"That isn't a ranged weapon or a clip.\n\r", ch );
	return;
    }

    if ( !gun->contains )
    {
     send_to_char(AT_BLUE,"There isn't anything in that weapon.\n\r", ch );
     return;
    }
 
    for ( ammo = gun->contains; ammo; ammo = ammo->next_content )
    {
     if ( !ammo || ammo->deleted )
      continue;

     if ( !gun->contains )
      break;

     obj_from_obj( ammo, FALSE );
     obj_to_char( ammo, ch );
     ammocheck = TRUE;
    }

   if ( ammocheck )
   {
    act(AT_GREEN, "You unload $p.", ch, gun, NULL, TO_CHAR );
    act(AT_GREEN, "$n unloads $p.", ch, gun, NULL, TO_ROOM );
   }
   else
    act(AT_GREEN, "Your $p is empty.", ch, gun, NULL, TO_CHAR );

 return;
}

void do_butcher( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *corpse;
    OBJ_DATA *part;
    OBJ_DATA *blade;
    char      arg [ MAX_INPUT_LENGTH ];
    char      buf [ MAX_STRING_LENGTH ];
    int	vnum = 0;
    int	countdown = 0;

    argument = one_argument( argument, arg );

    if ( IS_NPC(ch) )
    {
     typo_message( ch );
     return;
    }

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_WHITE, "Butcher what?\n\r", ch );
	return;
    }

    corpse = get_obj_list( ch, arg, ch->in_room->contents );

    if ( !corpse )
    {
     act(AT_WHITE, "I see no $T here.", ch, NULL, arg, TO_CHAR );
      return;
    }

    if( corpse->item_type != ITEM_CORPSE_NPC )
    {
     send_to_char( C_DEFAULT, "That isn't an NPC corpse.\n\r", ch );
     return;
    }
   
 if ( ch->pcdata->claws == 0 || ch->pcdata->claws == CLAW_BASH )
 {
  if( !(blade = get_eq_char( ch, WEAR_WIELD )) )
   if( !(blade = get_eq_char( ch, WEAR_WIELD_2 )) )
   {
    send_to_char( C_DEFAULT, "You need to be wielding a sharp device, IE: a knife or blade, to perform this task.\n\r", ch );
    return;
   }

  if ( blade->durability <= 0 )
  {
   send_to_char( C_DEFAULT, "That object is broken, you can not butcher a corpse with it.\n\r", ch );
   return;
  }

  if ( blade->item_type != ITEM_WEAPON
       && blade->value[8] != WEAPON_SLASH
       && blade->value[8] != WEAPON_BLADE
       && blade->value[8] != WEAPON_CHOP )
  {
   send_to_char( C_DEFAULT, "What you're holding there isn't going to be able to carve that corpse.\n\r", ch );
   return;
  }
 }

  switch( corpse->value[0] )
  {
   default:
     vnum = OBJ_VNUM_SEVERED_HEAD;		
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

     vnum = OBJ_VNUM_TORN_HEART;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

     vnum = OBJ_VNUM_SLICED_RIGHT_ARM;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

     vnum = OBJ_VNUM_SLICED_RIGHT_LEG;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

     vnum = OBJ_VNUM_SLICED_LEFT_ARM;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

     vnum = OBJ_VNUM_SLICED_LEFT_LEG;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
   break;

   case RACE_TYPE_LOBSTER:
    countdown = 2;
    while( countdown > 0 )
    {
     vnum = OBJ_VNUM_LOBSTER_CLAW;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
     countdown -= 1;
    }

    countdown = 6;
    while( countdown > 0 )
    {
     vnum = OBJ_VNUM_THIN_LEG;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
     countdown -= 1;
    }


     vnum = OBJ_VNUM_NOHAIR_HEAD;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
   break;

   case RACE_TYPE_FLYING_INSECT:
     vnum = OBJ_VNUM_INSECT_WINGS;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

    countdown = 8;
    while( countdown > 0 )
    {
     vnum = OBJ_VNUM_THIN_LEG;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
     countdown -= 1;
    }
   break;

   case RACE_TYPE_SPIDER:
     vnum = OBJ_VNUM_INSECT_ABDOMEN;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

    countdown = 8;
    while( countdown > 0 )
    {
     vnum = OBJ_VNUM_THIN_LEG;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
     countdown -= 1;
    }
   break;
  
   case RACE_TYPE_SNAKE:
     vnum = OBJ_VNUM_SNAKE_SKIN;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
   break;

   case RACE_TYPE_BIRD:
    countdown = 2;
    while( countdown > 0 )
    {
     vnum = OBJ_VNUM_BIRD_CLAW;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
     countdown -= 1;
    }

    countdown = 2;
    while( countdown > 0 )
    {
     vnum = OBJ_VNUM_WING;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
     countdown -= 1;
    }

     vnum = OBJ_VNUM_NOHAIR_HEAD;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
   break;

   case RACE_TYPE_DOG:
     vnum = OBJ_VNUM_NOHAIR_HEAD;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

     vnum = OBJ_VNUM_TORN_HEART;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

     vnum = OBJ_VNUM_TAIL;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );

    countdown = 2;
    while( countdown > 0 )
    {
     vnum = OBJ_VNUM_SLICED_RIGHT_LEG;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
     countdown -= 1;
    }

    countdown = 2;
    while( countdown > 0 )
    {
     vnum = OBJ_VNUM_SLICED_LEFT_LEG;
     part = create_object( get_obj_index( vnum ), 0 );

     sprintf( buf, part->short_descr, corpse->ownedby );
     free_string( part->short_descr );
     part->short_descr = str_dup( buf );
     sprintf( buf, part->description, corpse->ownedby );
     free_string( part->description );
     part->description = str_dup( buf );

     obj_to_room( part, ch->in_room );
     countdown -= 1;
    }
   break;
  }

  countdown = corpse->value[1];
  while( countdown > 0 )
  {
   vnum = OBJ_VNUM_MUSHROOM;
   part = create_object( get_obj_index( vnum ), 0 );
   obj_to_room( part, ch->in_room );
   countdown -= 1;
  }

  act( AT_WHITE, "You butcher $p.", ch, corpse, NULL, TO_CHAR );
  act( AT_WHITE, "$n butchers $p.", ch, corpse, NULL, TO_ROOM );
  extract_obj( corpse );
 return;
}
