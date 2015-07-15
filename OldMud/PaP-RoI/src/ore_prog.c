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
 *  The MOBprograms have been contributed by N'Atas-ha.  Any support for   *
 *  these routines should not be expected from Merc Industries.  However,  *
 *  under no circumstances should the blame for bugs, etc be placed on     *
 *  Merc Industries.  They are not guaranteed to work on all systems due   *
 *  to their frequent use of strxxx functions.  They are also not the most *
 *  efficient way to perform their tasks, but hopefully should be in the   *
 *  easiest possible way to install and begin using. Documentation for     *
 *  such installation can be found in INSTALL.  Enjoy...         N'Atas-Ha *
 ***************************************************************************/

#include <sys/types.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <ctype.h>
#include "merc.h"

/* Globals */
TRAP_DATA *trap_list;

/* Local globals */
CHAR_DATA   *smob;


/*
 * External function prototype (from mob_prog.c)
 */

void	mprog_driver		args( ( char* com_list, CHAR_DATA* mob,
				       CHAR_DATA* actor, OBJ_DATA* obj,
				       void* vo ) );

/*
 * Local function prototypes
 */

bool    tprog_wordlist_check    args( ( char *arg, CHAR_DATA *actor,
				       OBJ_DATA *obj, void *vo, int type,
				       TRAP_DATA *tprogs ) );
bool    tprog_spell_check       args( ( int sn, CHAR_DATA *actor,
				       OBJ_DATA *obj, void *vo, int type,
				       TRAP_DATA *tprogs ) );
bool    tprog_percent_check     args( ( CHAR_DATA *actor, OBJ_DATA *obj,
				       void *vo, int type,
				       TRAP_DATA *tprogs ) );

void    check_smob              args( ( void ) );
void    oprog_mob               args( ( OBJ_DATA *obj ) );
void    rprog_mob               args( ( ROOM_INDEX_DATA *room ) );
void    eprog_mob               args( ( EXIT_DATA *pExit,
				       ROOM_INDEX_DATA *room ) );
void    tprog_cleanup           args( ( void ) );

/***************************************************************************
 * Local function code and brief comments.
 */

/* if you dont have these functions, you damn well should... */

#ifdef DUNNO_STRSTR
char * strstr(const char *s1, const char *s2);
#endif

/***************************************************************************
 * Global function code and brief comments.
 */

/* The next two routines are the basic trigger types. Either trigger
 *  on a certain percent, or trigger on a keyword or word phrase.
 *  To see how this works, look at the various trigger routines..
 */

/* I added tprog_spell_check to check for certain sn's..
 * Also, i made them return a boolean so that there can be triggers
 * which only go off based on other triggers status..
 * This was mainly for the enter/exit/pass routines on the
 * rooms and exits.
 * few other minor changes from the MPROG stuff to support our
 * extended checks, and also to support the fact that objs/rooms/exits
 * need a specialized mob created for them.. (smob)
 * -- Altrag
 */
bool tprog_wordlist_check( char *arg, CHAR_DATA *actor, OBJ_DATA *obj,
			   void *vo, int type, TRAP_DATA *tprogs )
{

  char        temp1[ MAX_STRING_LENGTH ];
  char        temp2[ MAX_INPUT_LENGTH ];
  char        word[ MAX_INPUT_LENGTH ];
  TRAP_DATA  *tprg;
  char       *list;
  char       *start;
  char       *dupl;
  char       *end;
  int         i;

  if ( smob == actor )
    return FALSE;

  for ( tprg = tprogs; tprg != NULL; tprg = tprg->next_here )
  {
   if ( !tprg )
    continue;

    if ( tprg->type & type )
      {
	strcpy( temp1, tprg->arglist );
	list = temp1;
	for ( i = 0; i < strlen( list ); i++ )
	  list[i] = LOWER( list[i] );
	strcpy( temp2, arg );
	dupl = temp2;
	for ( i = 0; i < strlen( dupl ); i++ )
	  dupl[i] = LOWER( dupl[i] );
	if ( ( list[0] == 'p' ) && ( list[1] == ' ' ) )
	  {
	    list += 2;
	    while ( ( start = strstr( dupl, list ) ) )
	      if ( (start == dupl || *(start-1) == ' ' )
		  && ( *(end = start + strlen( list ) ) == ' '
		      || *end == '\n'
		      || *end == '\r'
		      || *end == '\0' ) )
		{
		  mprog_driver( tprg->comlist, smob, actor, obj, vo );
		  return TRUE;
		}
	      else
		dupl = start+1;
	  }
	else
	  {
	    list = one_argument( list, word );
	    for( ; word[0] != '\0'; list = one_argument( list, word ) )
	      while ( ( start = strstr( dupl, word ) ) )
		if ( ( start == dupl || *(start-1) == ' ' )
		    && ( *(end = start + strlen( word ) ) == ' '
			|| *end == '\n'
			|| *end == '\r'
			|| *end == '\0' ) )
		  {
		    mprog_driver( tprg->comlist, smob, actor, obj, vo );
		    return TRUE;
		  }
		else
		  dupl = start+1;
	  }
      }
     }

  return FALSE;

}

bool tprog_spell_check( int sn, CHAR_DATA *actor, OBJ_DATA *obj,
		        void *vo, int type, TRAP_DATA *tprogs )
{
 TRAP_DATA * tprg;

 for ( tprg = tprogs; tprg != NULL; tprg = tprg->next_here )
 {
  if ( !tprg )
   continue;

   if ( ( tprg->type & type )
       && ( is_sn( sn ) )
       && ( sn == slot_lookup(atoi(tprg->arglist)) ) )
     {
       mprog_driver( tprg->comlist, smob, actor, obj, vo );
       return TRUE;
     }
 }

 return FALSE;
}

bool tprog_percent_check( CHAR_DATA *actor, OBJ_DATA *obj, void *vo,
			  int type, TRAP_DATA *tprogs )
{
 TRAP_DATA * tprg;

 for ( tprg = tprogs; tprg != NULL; tprg = tprg->next_here )
 {
  if ( !tprg )
   continue;

  if ( !is_number( tprg->arglist ) )
   continue;

   if ( ( tprg->type & type )
       && ( number_percent( ) < atoi( tprg->arglist ) ) )
     {
       mprog_driver( tprg->comlist, smob, actor, obj, vo );
       return TRUE;
     }
 }

 return FALSE;
}

/* These are for creating the super-mob so that the t_progs can use
 * mprog stuff..
 * -- Altrag
 */
void check_smob( void )
{

    if ( !smob )
    {
     smob = create_mobile( get_mob_index( MOB_VNUM_SUPERMOB ) );
     char_list = char_list->next;  /*Don't want it in the char_list */
     mobs_in_game--;  /* Doesn't count as a mob in the game*/
    }

  if ( smob->in_room )
    char_from_room( smob );

  free_string(smob->name);
  free_string(smob->short_descr);
}

void rprog_mob( ROOM_INDEX_DATA *room )
{
  check_smob( );
  smob->name = str_dup(room->name);
  smob->short_descr = str_dup(room->name);
  char_to_room(smob, room);
}

void oprog_mob( OBJ_DATA *obj )
{
  check_smob( );
  smob->name = str_dup(obj->name);
  smob->short_descr = str_dup(obj->short_descr);
  if ( obj->in_room )
    char_to_room(smob, obj->in_room);
  else if ( obj->carried_by && obj->carried_by->in_room )
    char_to_room(smob, obj->carried_by->in_room);
  else if ( obj->stored_by && obj->stored_by->in_room )
    char_to_room(smob, obj->stored_by->in_room);
  else if ( obj->in_obj )
  {
    OBJ_DATA *in_obj;

    for ( in_obj = obj->in_obj; in_obj->in_obj; in_obj = in_obj->in_obj )
      ;
    if ( in_obj->in_room )
      char_to_room(smob, in_obj->in_room);
    else if ( in_obj->carried_by && in_obj->carried_by->in_room )
      char_to_room(smob, in_obj->carried_by->in_room);
    else if ( in_obj->stored_by && in_obj->stored_by->in_room )
      char_to_room(smob, in_obj->stored_by->in_room);
    else
      char_to_room(smob, get_room_index(ROOM_VNUM_LIMBO));
  }
  else
    char_to_room(smob, get_room_index(ROOM_VNUM_LIMBO));
  return;
}

void eprog_mob( EXIT_DATA *pExit, ROOM_INDEX_DATA *room )
{
  char buf[MAX_STRING_LENGTH];

  check_smob( );
  if ( pExit->keyword )
  {
    char buf1[MAX_INPUT_LENGTH];

    smob->name = str_dup( pExit->keyword );
    one_argument( pExit->keyword, buf1 );
    sprintf( buf, "The %s", buf1 );
  }
  else
  {
    int door;

    for ( door = 0; door < 6; door++ )
      if ( pExit == room->exit[door] )
	break;
    if ( door == 6 )
    {
      bug( "EProg_mob:  Door not in room %d", room->vnum );
      smob->name = str_dup( "door" );
      sprintf( buf, "The door" );
    }
    else
    {
      const char *new_dir[] = {"northern", "eastern", "southern", "western",
			       "roof", "ground"};

      smob->name = str_dup( dir_name[door] );
      sprintf(buf, "The %s exit", new_dir[door] );
    }
  }
  smob->short_descr = str_dup(buf);
  if ( room )
    char_to_room(smob, room);
  else
    char_to_room(smob, get_room_index(ROOM_VNUM_LIMBO));
  return;
}

void tprog_cleanup( void )
{
  if ( !smob )
    return;

  extract_char( smob, TRUE );
  mobs_in_game++; /* extract_char subtracts a mob but smob doesn't count*/
  smob->deleted = FALSE; /* Save reallocing all the time */
}

/* The triggers.. These are really basic, and since most appear only
 * once in the code (hmm. i think they all do) it would be more efficient
 * to substitute the code in and make the mprog_xxx_check routines global.
 * However, they are all here in one nice place at the moment to make it
 * easier to see what they look like. If you do substitute them back in,
 * make sure you remember to modify the variable names to the ones in the
 * trigger calls.
 */
void oprog_get_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_GET )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_GET,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_get_from_trigger( OBJ_DATA *obj, CHAR_DATA *ch,
			     OBJ_DATA *secondary )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_GET_FROM )
  {
    oprog_mob( obj );
    if ( !tprog_percent_check( ch, obj, secondary, OBJ_TRAP_GET_FROM,
			       obj->pIndexData->traps ) )
      oprog_get_trigger( obj, ch );
    tprog_cleanup( );
  }
  else
    oprog_get_trigger( obj, ch );
  return;
}

void oprog_give_trigger( OBJ_DATA *obj, CHAR_DATA *ch, CHAR_DATA *victim )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_GIVE )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, victim, OBJ_TRAP_GIVE,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_fill_trigger( OBJ_DATA *obj, CHAR_DATA *ch, OBJ_DATA *spring )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_FILL )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, spring, OBJ_TRAP_FILL,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_drop_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_DROP )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_DROP,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_put_trigger( OBJ_DATA *obj, CHAR_DATA *ch, OBJ_DATA *secondary )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_PUT )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, secondary, OBJ_TRAP_PUT,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_wear_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_WEAR )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_WEAR,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_remove_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_REMOVE )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_REMOVE,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}


void oprog_look_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_LOOK )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_LOOK,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_look_in_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_LOOK_IN )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_LOOK_IN,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_invoke_trigger( OBJ_DATA *obj, CHAR_DATA *ch, void *vo )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_INVOKE )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, vo, OBJ_TRAP_INVOKE,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_use_trigger( OBJ_DATA *obj, CHAR_DATA *ch, void *vo )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_USE )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, vo, OBJ_TRAP_USE,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_cast_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_CAST )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_CAST,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_cast_sn_trigger( OBJ_DATA *obj, CHAR_DATA *ch, int sn, void *vo )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_CAST_SN )
  {
    oprog_mob( obj );
    if ( !tprog_spell_check( sn, ch, obj, vo, OBJ_TRAP_CAST_SN,
			     obj->pIndexData->traps ) )
      oprog_cast_trigger( obj, ch );
    tprog_cleanup( );
  }
  else
    oprog_cast_trigger( obj, ch );
  return;
}

void oprog_join_trigger( OBJ_DATA *obj, CHAR_DATA *ch, OBJ_DATA *secondary )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_JOIN )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, secondary, OBJ_TRAP_JOIN,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_separate_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_SEPARATE )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_SEPARATE,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_buy_trigger( OBJ_DATA *obj, CHAR_DATA *ch, CHAR_DATA *vendor )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_BUY )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, vendor, OBJ_TRAP_BUY,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_sell_trigger( OBJ_DATA *obj, CHAR_DATA *ch, CHAR_DATA *vendor )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_SELL )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, vendor, OBJ_TRAP_SELL,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_store_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_STORE )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_STORE,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_retrieve_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_RETRIEVE )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_RETRIEVE,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_open_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_OPEN )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_OPEN,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_close_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_CLOSE )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_CLOSE,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_lock_trigger( OBJ_DATA *obj, CHAR_DATA *ch, OBJ_DATA *key )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_LOCK )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, key, OBJ_TRAP_LOCK,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_unlock_trigger( OBJ_DATA *obj, CHAR_DATA *ch, OBJ_DATA *key )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_UNLOCK )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, key, OBJ_TRAP_UNLOCK,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_pick_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_PICK )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_PICK,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_random_trigger( OBJ_DATA *obj )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_RANDOM )
  {
    oprog_mob( obj );
    tprog_percent_check( NULL, obj, NULL, OBJ_TRAP_RANDOM,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void oprog_throw_trigger( OBJ_DATA *obj, CHAR_DATA *ch )
{
  if ( obj->pIndexData->traptypes & OBJ_TRAP_THROW )
  {
    oprog_mob( obj );
    tprog_percent_check( ch, obj, NULL, OBJ_TRAP_THROW,
			 obj->pIndexData->traps );
    tprog_cleanup( );
  }
  return;
}

void rprog_enter_trigger( ROOM_INDEX_DATA *room, CHAR_DATA *ch )
{
  if ( room->traptypes & ROOM_TRAP_ENTER )
  {
    rprog_mob( room );
    if ( !tprog_percent_check( ch, NULL, NULL, ROOM_TRAP_ENTER,
			      room->traps ) )
      rprog_pass_trigger( room, ch );
    tprog_cleanup( );
  }
  else
    rprog_pass_trigger( room, ch );
  return;
}

void rprog_exit_trigger( ROOM_INDEX_DATA *room, CHAR_DATA *ch )
{
  if ( room->traptypes & ROOM_TRAP_EXIT )
  {
    rprog_mob( room );
    if ( !tprog_percent_check( ch, NULL, NULL, ROOM_TRAP_EXIT,
			      room->traps ) )
      rprog_pass_trigger( room, ch );
    tprog_cleanup( );
  }
  else
    rprog_pass_trigger( room, ch );
  return;
}

void rprog_pass_trigger( ROOM_INDEX_DATA *room, CHAR_DATA *ch )
{
  if ( room->traptypes & ROOM_TRAP_PASS )
  {
    rprog_mob( room );
    tprog_percent_check( ch, NULL, NULL, ROOM_TRAP_PASS,
			 room->traps );
    tprog_cleanup( );
  }
  return;
}

void rprog_cast_trigger( ROOM_INDEX_DATA *room, CHAR_DATA *ch )
{
  if ( room->traptypes & ROOM_TRAP_CAST )
  {
    rprog_mob( room );
    tprog_percent_check( ch, NULL, NULL, ROOM_TRAP_CAST,
			 room->traps );
    tprog_cleanup( );
  }
  return;
}

void rprog_cast_sn_trigger( ROOM_INDEX_DATA *room, CHAR_DATA *ch, int sn,
			    void *vo )
{
  if ( room->traptypes & ROOM_TRAP_CAST_SN )
  {
    rprog_mob( room );
    if ( !tprog_spell_check( sn, ch, NULL, vo, ROOM_TRAP_CAST_SN,
			     room->traps ) )
      rprog_cast_trigger( room, ch );
    tprog_cleanup( );
  }
  else
    rprog_cast_trigger( room, ch );
  return;
}

void rprog_sleep_trigger( ROOM_INDEX_DATA *room, CHAR_DATA *ch )
{
  if ( room->traptypes & ROOM_TRAP_SLEEP )
  {
    rprog_mob( room );
    tprog_percent_check( ch, NULL, NULL, ROOM_TRAP_SLEEP,
			 room->traps );
    tprog_cleanup( );
  }
  return;
}

void rprog_wake_trigger( ROOM_INDEX_DATA *room, CHAR_DATA *ch )
{
  if ( room->traptypes & ROOM_TRAP_WAKE )
  {
    rprog_mob( room );
    tprog_percent_check( ch, NULL, NULL, ROOM_TRAP_WAKE,
			 room->traps );
    tprog_cleanup( );
  }
  return;
}

void rprog_rest_trigger( ROOM_INDEX_DATA *room, CHAR_DATA *ch )
{
  if ( room->traptypes & ROOM_TRAP_REST )
  {
    rprog_mob( room );
    tprog_percent_check( ch, NULL, NULL, ROOM_TRAP_REST,
			 room->traps );
    tprog_cleanup( );
  }
  return;
}

void rprog_death_trigger( ROOM_INDEX_DATA *room, CHAR_DATA *ch )
{
  if ( room->traptypes & ROOM_TRAP_DEATH )
  {
    rprog_mob( room );
    tprog_percent_check( ch, NULL, NULL, ROOM_TRAP_DEATH,
			 room->traps );
    tprog_cleanup( );
  }
  return;
}

void rprog_time_trigger( ROOM_INDEX_DATA *room, int hour )
{
  if ( room->traptypes & ROOM_TRAP_TIME )
  {
    TRAP_DATA *tprg;

    rprog_mob( room );
    for ( tprg = room->traps; tprg; tprg = tprg->next_here )
    {
      if ( tprg->type & ROOM_TRAP_TIME )
      {
	char stime[MAX_INPUT_LENGTH];
	char etime[MAX_INPUT_LENGTH];
	int  sint;
	int  eint;
	char *arg_p;

	arg_p = one_argument( tprg->arglist, stime );
	one_argument( arg_p, etime );

	if ( !is_number(stime) || !is_number(etime) ||
	      (sint = atoi(stime)) < 0 || sint > 23 ||
	      (eint = atoi(etime)) < 0 || eint > 23 )
	{
	  bug( "Rprog_time_trigger: Invalid time %d", room->vnum );
	  continue;
	}
	if ( eint > sint )
	{
	  if ( hour <= eint && hour >= sint )
	    mprog_driver( tprg->comlist, smob, NULL, NULL, NULL );
	}
	else
	{
/* Example: argument: 10 5 (eint = 5, sint = 10) */
	  if ( hour >= eint && hour <= sint )
	    mprog_driver( tprg->comlist, smob, NULL, NULL, NULL );
	}
      }
    }
   tprog_cleanup( );
  }
  return;
}

void rprog_random_trigger( ROOM_INDEX_DATA *room )
{
  if ( room->traptypes & ROOM_TRAP_RANDOM )
  {
    rprog_mob( room );
    tprog_percent_check( NULL, NULL, NULL, ROOM_TRAP_RANDOM,
			 room->traps );
    tprog_cleanup( );
  }
  return;
}

void eprog_enter_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			  CHAR_DATA *ch )
{
  if ( pExit->traptypes & EXIT_TRAP_ENTER )
  {
    eprog_mob( pExit, room );
    if ( !tprog_percent_check( ch, NULL, NULL, EXIT_TRAP_ENTER,
			       pExit->traps ) )
      eprog_pass_trigger( pExit, room, ch, TRUE );
    tprog_cleanup( );
  }
  else
    eprog_pass_trigger( pExit, room, ch, TRUE );
  return;
}

void eprog_exit_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			 CHAR_DATA *ch )
{
  if ( pExit->traptypes & EXIT_TRAP_EXIT )
  {
    eprog_mob( pExit, room );
    if ( !tprog_percent_check( ch, NULL, NULL, EXIT_TRAP_EXIT,
			       pExit->traps ) )
      eprog_pass_trigger( pExit, room, ch, FALSE );
    tprog_cleanup( );
  }
  else
    eprog_pass_trigger( pExit, room, ch, FALSE );
  return;
}

void eprog_pass_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			 CHAR_DATA *ch, bool fEnter )
{
  if ( pExit->traptypes & EXIT_TRAP_PASS )
  {
    eprog_mob( pExit, room );
    if ( !tprog_percent_check( ch, NULL, NULL, EXIT_TRAP_PASS,
			       pExit->traps ) )
    {
      if ( fEnter )
	rprog_exit_trigger( room, ch );
      else
	rprog_enter_trigger( room, ch );
    }
    tprog_cleanup( );
  }
  else
  {
    if ( fEnter )
      rprog_exit_trigger( room, ch );
    else
      rprog_enter_trigger( room, ch );
  }
  return;
}

void eprog_look_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			 CHAR_DATA *ch )
{
  if ( pExit->traptypes & EXIT_TRAP_LOOK )
  {
    eprog_mob( pExit, room );
    tprog_percent_check( ch, NULL, NULL, EXIT_TRAP_LOOK,
			 pExit->traps );
    tprog_cleanup( );
  }
  return;
}

void eprog_scry_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			 CHAR_DATA *ch )
{
  if ( !IS_AFFECTED( ch, AFF_SCRY ) )
  {
    eprog_look_trigger( pExit, room, ch );
    return;
  }

  if ( pExit->traptypes & EXIT_TRAP_SCRY )
  {
    eprog_mob( pExit, room );
    if ( !tprog_percent_check( ch, NULL, NULL, EXIT_TRAP_SCRY,
			       pExit->traps ) )
      eprog_look_trigger( pExit, room, ch );
    tprog_cleanup( );
  }
  else
    eprog_look_trigger( pExit, room, ch );
  return;
}

void eprog_open_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			 CHAR_DATA *ch )
{
  if ( pExit->traptypes & EXIT_TRAP_OPEN )
  {
    eprog_mob( pExit, room );
    tprog_percent_check( ch, NULL, NULL, EXIT_TRAP_OPEN,
			 pExit->traps );
    tprog_cleanup( );
  }
  return;
}

void eprog_close_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			  CHAR_DATA *ch )
{
  if ( pExit->traptypes & EXIT_TRAP_CLOSE )
  {
    eprog_mob( pExit, room );
    tprog_percent_check( ch, NULL, NULL, EXIT_TRAP_CLOSE,
			 pExit->traps );
    tprog_cleanup( );
  }
  return;
}

void eprog_lock_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			 CHAR_DATA *ch, OBJ_DATA *key )
{
  if ( pExit->traptypes & EXIT_TRAP_LOCK )
  {
    eprog_mob( pExit, room );
    tprog_percent_check( ch, key, NULL, EXIT_TRAP_LOCK,
			 pExit->traps );
    tprog_cleanup( );
  }
  return;
}

void eprog_unlock_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			   CHAR_DATA *ch, OBJ_DATA *key )
{
  if ( pExit->traptypes & EXIT_TRAP_UNLOCK )
  {
    eprog_mob( pExit, room );
    tprog_percent_check( ch, key, NULL, EXIT_TRAP_UNLOCK,
			 pExit->traps );
    tprog_cleanup( );
  }
  return;
}

void eprog_pick_trigger( EXIT_DATA *pExit, ROOM_INDEX_DATA *room,
			 CHAR_DATA *ch )
{
  if ( pExit->traptypes & EXIT_TRAP_PICK )
  {
    eprog_mob( pExit, room );
    tprog_percent_check( ch, NULL, NULL, EXIT_TRAP_PICK,
			 pExit->traps );
    tprog_cleanup( );
  }
  return;
}
