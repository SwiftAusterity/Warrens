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
#include <string.h>
#include <time.h>
#include "merc.h"

/*
 * Externals
 */ 
extern  char *  const   month_name[];
extern  char *  const   day_name[];
extern  char *  const   month2_name[];
extern  char *  const   day2_name[];
extern  bool            merc_down;
extern  int             auc_count;
extern  void            auc_channel( char *auction );
extern  void            chat_update( void );
extern  int             port;

/*
 * Globals
 */
bool    delete_obj;
bool    delete_tattoo;
bool    delete_char;

/*
 * Local functions.
 */
int	hit_gain        args( ( CHAR_DATA *ch ) );
int	mana_gain       args( ( CHAR_DATA *ch ) );
int	move_gain       args( ( CHAR_DATA *ch ) );
void	mobile_update   args( ( void ) );
void	wtime_update     args( ( void ) );
void	economy_update     args( ( void ) );
void	weather_update  args( ( AREA_DATA *pArea ) );
void	char_update     args( ( void ) );
void	obj_update      args( ( void ) );
void	aggr_update     args( ( void ) );
void	comb_update     args( ( void ) );	/* XOR */
void    auc_update      args( ( void ) );       /* Altrag */
void    rdam_update     args( ( void ) );       /* Altrag */
void	arena_update	args( ( void ) );	/* Altrag */
void    strew_corpse    args( ( OBJ_DATA *obj, AREA_DATA *inarea ) );
void    orprog_update   args( ( void ) );
void    trap_update     args( ( void ) );
void    rtime_update    args( ( void ) );   /* Timed room progs */
void    room_exit_update args( ( void ) );   /* exit_affect updater */
void    timebomb_update    args( ( void ) );   /* does the timebomb fuse */
void	sky_update
 args( ( ROOM_INDEX_DATA *pRoom, int weather_state, int cloud_state ) ); 
void	char_queue_update args ( ( CHAR_DATA *ch ) );
void	near_per_second_update args ( ( CHAR_DATA *ch ) );

 /* Cloud cover for weather_update */
void	astral_portal_update args( ( ROOM_INDEX_DATA *in_room ) );
int	find_astral_dest args(( ROOM_INDEX_DATA *startroom ));

/*
 * Advancement stuff.
 */
void advance_level( CHAR_DATA *ch )
{
    char buf [ MAX_STRING_LENGTH ];
    int  add_hp;
    int  add_mana;
    int  add_move;
    int  add_prac;

    if ( IS_NPC(ch) )
     return;

    add_hp      = number_range( get_curr_con(ch) / 3, get_curr_con(ch) );
    add_mana    = 
     number_range(2, ( 2 * get_curr_wis( ch ) + get_curr_int( ch ) ) / 4);

    add_move    =
      number_range( 5, ( get_curr_con( ch ) + get_curr_dex( ch ) ) / 4 );
    add_prac    = wis_app[get_curr_wis( ch )].practice;

    add_hp               = UMAX(  1, add_hp   );
    add_mana             = UMAX(  0, add_mana );
    add_move             = UMAX( 10, add_move );

    ch->perm_hit 	+= add_hp;
    ch->perm_mana	+= add_mana;
    ch->perm_move	+= add_move;
    ch->practice	+= add_prac;

    if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_BOUGHT_PET) )
	REMOVE_BIT( ch->act, PLR_BOUGHT_PET );

    sprintf( buf, 
	"Your gain is: %d/%d hp, %d/%d m, %d/%d mv %d/%d prac.\n\r",
	add_hp,		MAX_HIT( ch ),
	add_mana,	MT_MAX( ch ),
	add_move,	MAX_MOVE( ch ),
	add_prac,	ch->practice );
    send_to_char(AT_WHITE, buf, ch );
    save_char_obj( ch, FALSE );

    return;
}   

void gain_exp( CHAR_DATA *ch, int gain )
{
    char buf [ MAX_STRING_LENGTH ];

    if ( IS_NPC( ch ) || ch->level >= L_CHAMP3 )
	return;

    ch->exp += gain;

    while ( ch->exp >= xp_tolvl( ch ) ) 
    {
     send_to_char(AT_BLUE, "You raise a level!!  ", ch );
     ch->level += 1;
     sprintf( buf, "%s just made level %d!", ch->name, ch->level);
     wiznet(buf,ch,NULL,WIZ_LEVELS,0,0);
     if ( ch->pcdata->port == PORT_PAP )
      info( "%s advances to level %d!", (int)ch->name, ch->level, PORT_PAP );
     else
      info( "%s advances to level %d!", (int)ch->name, ch->level, PORT_ROI );

     advance_level( ch );

     if ( ch->level == 200 && ch->pcdata->remort == 0 )
      ch->desc->connected = CON_REMORT_FIRST;
    } 
    return;
}

/*
 * Regeneration stuff.
 */
int hit_gain( CHAR_DATA *ch )
{
    int iWear;
    OBJ_DATA *armor;
    OBJ_DATA	*seat;
    int		gain;
    int		regen;

    if ( IS_NPC( ch ) )
	gain = get_curr_con(ch) * 2 + ch->level * 3;
    else
    {
	gain = number_range( get_curr_con(ch) *3, get_curr_con(ch) * 6 );

	switch ( ch->position )
	{
	case POS_SLEEPING: gain += ( ch->pcdata->learned[gsn_fastheal] > 0
				&& ch->pcdata->learned[gsn_fastheal] > number_percent( ) ) ?
				get_curr_con( ch ) * 2 :
				get_curr_con( ch );		break;
	case POS_RESTING:  gain += ( ch->pcdata->learned[gsn_fastheal] > 0
				&& ch->pcdata->learned[gsn_fastheal] > number_percent( ) ) ?
				get_curr_con( ch ) :
				get_curr_con( ch ) / 2;		break;
	default: gain += ( ch->pcdata->learned[gsn_fastheal] > 0
		      && ch->pcdata->learned[gsn_fastheal] > number_percent( ) ) ?
		      get_curr_con( ch ) : 0;
	}

    for ( seat = ch->in_room->contents; seat; seat = seat->next_content )
    {
	if ( seat->deleted )
	    continue;

	if ( ch->resting_on == seat )
        {
         if ( seat->value[1] == FURNITURE_CHAIR ||
	      seat->value[1] == FURNITURE_SOFA )
         gain *= 2;
         else if ( seat->value[1] == FURNITURE_BED )
         gain *= 4;
         break;
        }
    }

    if ( rsector_check( ch->in_room ) == SECT_HEAVEN )
    gain *= 2;
    else
    if ( rsector_check( ch->in_room ) == SECT_BADLAND )
    gain /= 2;

    for ( iWear = 2; iWear < MAX_WEAR; iWear *= 2 )
    {
     if ( ( armor = get_eq_char( ch, iWear ) ) )
     {
      if ( armor->composition == MATERIAL_PLASMA )
       gain += 15; 
     }
    }

    regen = ch->pcdata->regen;
    
    if ( ch->pcdata->morph[2] > 0 )
     regen -= 15;
    if ( ch->pcdata->morph[4] > 0 )
     regen -= 15;

    if ( regen < 0 )
     regen = 0;

    gain = ( ( gain * regen ) / 100 );

   if ( !IS_NPC( ch ) )
     if ( ch->pcdata->condition[COND_FULL] == 0 )
	    gain = 0;

    if ( !IS_NPC(ch) )
	if ( ch->pcdata->condition[COND_THIRST] == 0 )
	    gain = 0;  

    if ( ch->race == RACE_TROLL )
       gain *= 5;
   }

    if ( IS_AFFECTED( ch, AFF_POISON ) && gain > 0 ) 
	gain /= 10;

/* Ward of Healing */
    if ( is_raffected( ch->in_room, gsn_ward_heal ) )
      {
      send_to_char(AT_WHITE, "The wards of healing soothe your wounds.\n\r", ch );
      gain += 100;
      }

    return UMIN( gain, MAX_HIT(ch) - ch->hit );
}

int mana_gain( CHAR_DATA *ch )
{
    int iWear;
    OBJ_DATA *armor;
    OBJ_DATA	*seat;
    int gain;

    if ( IS_NPC( ch ) )
    {
	gain = ch->level;
    }
    else
    {
	gain = UMIN( 5, ch->level / 2 );

	switch ( ch->position )
	{
	case POS_SLEEPING: gain += get_curr_int( ch ) * 2;	break;
	case POS_RESTING:  gain += get_curr_int( ch );		break;
	}

    for ( seat = ch->in_room->contents; seat; seat = seat->next_content )
    {
	if ( seat->deleted )
	    continue;

	if ( ch->resting_on == seat )
        {
         if ( seat->value[1] == FURNITURE_CHAIR ||
	      seat->value[1] == FURNITURE_SOFA )
         gain *= 2;
         else if ( seat->value[1] == FURNITURE_BED )
         gain *= 4;
         break;
        }
    }

    if ( rsector_check( ch->in_room ) == SECT_HEAVEN )
    gain *= 2;
    else
    if ( rsector_check( ch->in_room ) == SECT_BADLAND )
    gain /= 2;

    for ( iWear = 2; iWear < MAX_WEAR; iWear *= 2 )
    {
     if ( ( armor = get_eq_char( ch, iWear ) ) )
     {
      if ( armor->composition == MATERIAL_PLASMA )
       gain += 10; 
     }
    }

    gain = ( ( gain * ch->pcdata->regen ) / 100 );

    if ( ch->pcdata->condition[COND_FULL  ] == 0 )
	    gain = 0;

    if ( !IS_NPC(ch) )
	if ( ch->pcdata->condition[COND_THIRST] == 0 )
	    gain = 0;
	if ( ch->race == RACE_ELF || ch->race == RACE_AQUINIS )
	    gain *= 2;
    }

    if ( IS_AFFECTED( ch, AFF_POISON ) )
	gain /= 10;

    return UMIN( gain, MAX_MANA(ch) - ch->mana );
}



int move_gain( CHAR_DATA *ch )
{
    int iWear;
    OBJ_DATA *armor;
    OBJ_DATA	*seat;
    int gain;

    if ( IS_NPC( ch ) )
    {
	gain = ch->level;
    }
    else
    {
	gain = UMAX( 15, 2 * ch->level );

	switch ( ch->position )
	{
	case POS_SLEEPING: gain += get_curr_dex( ch );		break;
	case POS_RESTING:  gain += get_curr_dex( ch ) / 2;	break;
	}

    for ( seat = ch->in_room->contents; seat; seat = seat->next_content )
    {
	if ( seat->deleted )
	    continue;

	if ( ch->resting_on == seat )
        {
         if ( seat->value[1] == FURNITURE_CHAIR ||
	      seat->value[1] == FURNITURE_SOFA )
         gain *= 5;
         else if ( seat->value[1] == FURNITURE_BED )
         gain *= 10;
         break;
        }
    }

    if ( rsector_check( ch->in_room ) == SECT_HEAVEN )
    gain *= 2;
    else
    if ( rsector_check( ch->in_room ) == SECT_BADLAND )
    gain /= 2;

    for ( iWear = 2; iWear < MAX_WEAR; iWear *= 2 )
    {
     if ( ( armor = get_eq_char( ch, iWear ) ) )
     {
      if ( armor->composition == MATERIAL_PLASMA )
       gain += 25; 
     }
    }

    gain = ( ( gain * ch->pcdata->regen ) / 100 );

    if ( ch->pcdata->condition[COND_FULL  ] == 0 )
            gain /= 2;
	
    if ( !IS_NPC(ch) )
        if ( ch->pcdata->condition[COND_THIRST] == 0 )
	    gain /= 2;
    }

    if ( IS_AFFECTED( ch, AFF_POISON ) )
	gain /= 10;

    return UMIN( gain, MAX_MOVE(ch) - ch->move );
}



void gain_condition( CHAR_DATA *ch, int iCond, int value )
{
    int condition;
    int hploss = 1;

    if ( value == 0 || IS_NPC( ch ) || ch->level >= L_APP )
	return;

    if ( ch->level >= LEVEL_HERO && iCond != COND_DRUNK )
      return;

    condition				= ch->pcdata->condition[ iCond ];
					 /* Used to be 48 */

     ch->pcdata->condition[iCond]	= URANGE( 0, condition + value, 96  );

    hploss = (ch->level < 10) ? 1 : (ch->level < 20) ? 2 : 3;

    if ( ch->pcdata->condition[iCond] > 0 &&
         ch->pcdata->condition[iCond] < 5 )
    {
	switch ( iCond )
	{
	case COND_INSOMNIA:
	    send_to_char(AT_ORANGE, "Your eyelids begin to droop.\n\r", ch );
            interpret( ch, "yawn" );
	    break;

	case COND_FULL:
	    send_to_char(AT_ORANGE, "Your stomach rumbles.\n\r", ch );
	    break;

	case COND_THIRST:
		send_to_char(AT_BLUE, "You feel a bit parched.\n\r", ch );
	    break;
        }
    }
    else
    if ( ch->pcdata->condition[iCond] == 0 )
    {
	switch ( iCond )
	{
	case COND_INSOMNIA:
	    send_to_char(AT_ORANGE, "Your eyelids feel quite heavy.\n\r", ch );
            interpret( ch, "yawn" );
	    break;

	case COND_FULL:
	    send_to_char(AT_ORANGE, "You are famished.\n\r", ch );
	    break;

	case COND_THIRST:
		send_to_char(AT_BLUE, "Your throat aches for drink.\n\r", ch );
	    break;

	case COND_DRUNK:
	    if ( condition != 0 )
		send_to_char(AT_BLUE, "You are sober.\n\r", ch );
	    break;
	}

	if ( (iCond != COND_DRUNK) && (ch->hit - hploss) > 5 )
	      ch->hit = (ch->hit - hploss);
    }

    return;
}



/*
 * Mob autonomous action.
 * This function takes 25% of ALL Merc cpu time.
 * -- Furey
 */
/* Now includes all near-per-second updates too.
 * It probably takes up more than 25% now -- Flux
 */
void mobile_update( void )
{
    CHAR_DATA *ch;
    EXIT_DATA *pexit;
    int        door;

    /* Examine all mobs. */
    for ( ch = char_list; ch; ch = ch->next )
    {
        if ( ch->deleted )
	    continue;

      near_per_second_update ( ch );

/*	if ( IS_NPC(ch) && (ch->wait -= PULSE_MOBILE) < 0 )
	  ch->wait = 0; */

	if ( !IS_NPC( ch )
	    || !ch->in_room
	    || IS_AFFECTED( ch, AFF_CHARM )
	    || ch->wait > 0 )
	    continue;

	/* Examine call for special procedure */
	if ( ch->spec_fun != 0 )
         if ( ( *ch->spec_fun ) ( ch ) )
          continue;

        if ( IS_AFFECTED( ch, AFF_SLEEP ) )
         ch->position = POS_SLEEPING;

	/* That's all for sleeping / busy monster */
	if ( ch->position < POS_STANDING )
	    continue;
        
        if ( ch->in_room->area->nplayer > 0 ) 
          { 
           mprog_random_trigger( ch );
           mprog_random_victim_trigger( ch );
	   if ( ch->position < POS_STANDING )
	        continue;
	  }
	  
           
 
	/* Scavenge */
	if ( IS_SET( ch->act, ACT_SCAVENGER )
	    && ch->in_room->contents
	    && number_bits( 2 ) == 0 )
	{
	    OBJ_DATA *obj;
	    OBJ_DATA *obj_best;
	    int       max;

	    max         = 1;
	    obj_best    = 0;
	    for ( obj = ch->in_room->contents; obj; obj = obj->next_content )
	    {
		if ( CAN_WEAR( obj, ITEM_TAKE ) && 
		   ( ( (obj->cost.gold*C_PER_G) + (obj->cost.silver*S_PER_G) +
		     (obj->cost.copper) ) > max ) && can_see_obj(ch, obj) )
		{
		   obj_best	= obj;
		   max		= ( (obj->cost.gold*C_PER_G) + (obj->cost.silver*S_PER_G) +
				    (obj->cost.copper) );
		}
	    }

	    if ( obj_best )
	    {
		obj_from_room( obj_best );
		obj_to_char( obj_best, ch );
		act(AT_GREY, "$n gets $p.", ch, obj_best, NULL, TO_ROOM );
	    }
	}

	/* Wander */
	if ( !IS_SET( ch->act, ACT_SENTINEL )
	    && ( door = number_bits( 5 ) ) <= 5
	    && ( pexit = ch->in_room->exit[door] )
	    &&   pexit->to_room
	    &&   exit_blocked( pexit, ch->in_room ) == EXIT_STATUS_OPEN
	    &&   !IS_SET( pexit->to_room->room_flags, ROOM_NO_MOB )
	    &&   !(ch->hunting)
	    &&   !(ch->fighting)
	    &&   !(ch->engaged)
	    &&   !(ch->master)
	    &&   !(ch->leader)
            &&   ch->position != POS_FIGHTING
	    && ( !IS_SET( ch->act, ACT_STAY_AREA )
		||   pexit->to_room->area == ch->in_room->area ) )
	{
	    move_char( ch, door, FALSE, FALSE );
	    if ( ch->position < POS_STANDING )
	        continue;
	}

      if ( (ch->fighting) && ch->in_room != (ch->fighting)->in_room )
       ch->hunting = ch->fighting;

	if ( ch->hunting )
	  hunt_victim( ch ); 

    }
    MOBtrigger = TRUE;
    return;
}

/* Global economy */
void economy_update( void )
{
 int type = 1;

 if ( economy.market_type < 0 )
  economy.market_type += 1;
 else
 if ( economy.market_type > 0 )
  economy.market_type -= 1;

 while( type < MAX_ITEM_TYPE )
 {
  economy.cost_modifier[type] += economy.item_type[type];
  economy.item_type[type] = 0;
  economy.cost_modifier[type] += economy.market_type;

  if ( economy.cost_modifier[type] <= 25 )
   economy.cost_modifier[type] = 50;

  if ( economy.cost_modifier[type] >= 150 )
   economy.cost_modifier[type] = 125;

  type += 1;
 }
 return;
}

/*
 * Update the solar & lunar calander.
 */
void wtime_update( void )
{
    DESCRIPTOR_DATA *d;
    char             buf [ MAX_STRING_LENGTH ];
    char *suf;
    int   day;

    buf[0] = '\0';

    switch ( ++time_info.hour )
    {
    case  1:
	time_info.sunlight = SUN_DARK;
	break;

    case  5:
	time_info.sunlight = SUN_DAWN;
	strcat( buf, "&CThe &zDarkness&C begins to lift.\n\r" );
	break;

    case 6:
	time_info.sunlight = SUN_MORNING;
	strcat( buf, "&CThe &Psun&C rises triumphantly in the east.\n\r" );
	break;

    case 7:
        if ( time_info.phase_white == MOON_FULL )
	strcat( buf, "&cThe full &Wmoon&c vanishes with the coming day.&w\n\r" );
        if ( time_info.phase_white == MOON_FHALF
          || time_info.phase_white == MOON_NHALF )
	strcat( buf, "&cThe half &Wmoon&c vanishes with the coming day.&w\n\r" );
        if ( time_info.phase_white == MOON_FCRESCENT
          || time_info.phase_white == MOON_NCRESCENT )
	strcat( buf, "&cThe crescent &Wmoon&c vanishes with the coming day.&w\n\r" );
        if ( time_info.phase_white == MOON_NEW )
	strcat( buf, "&cThe shadow of the new &Wmoon&c vanishes within the piercing rays of sunlight.&w\n\r" );
        if ( time_info.phase_white == MOON_FTHIRD
          || time_info.phase_white == MOON_NTHIRD )
	strcat( buf, "&cThe fat crescent &Wmoon&c vanishes with the coming day.&w\n\r" );
        break;

    case 11:
	time_info.sunlight = SUN_NOON;
	strcat( buf, "&CThe &Ysun&C sits high in its &Wheavenly&C cradle.&w\n\r" );
	break;

    case 13:
	time_info.sunlight = SUN_AFTERNOON;
	strcat( buf, "&cThe afternoon &Ysun &Rb&ra&Rk&re&Rs&c the land.&w\n\r" );
	break;

    case 17:
        if ( time_info.phase_white == MOON_FULL )
	strcat( buf, "&cThe full &Wmoon&c peaks over the horizon.&w\n\r" );
        if ( time_info.phase_white == MOON_FHALF
          || time_info.phase_white == MOON_NHALF )
	strcat( buf, "&cThe half &Wmoon&c peaks over the horizon.&w\n\r" );
        if ( time_info.phase_white == MOON_FCRESCENT
          || time_info.phase_white == MOON_NCRESCENT ) 
	strcat( buf, "&cThe crescent &Wmoon&c peaks over the horizon.&w\n\r" );
        if ( time_info.phase_white == MOON_NEW )
	strcat( buf, "&cThe shadow of the new &Wmoon&c appears over the horizon.&w\n\r" );
        if ( time_info.phase_white == MOON_FTHIRD
          || time_info.phase_white == MOON_NTHIRD )
	strcat( buf, "&cThe fat crescent &Wmoon&c peaks over the horizon.&w\n\r" );
        break;

    case 18:
	time_info.sunlight = SUN_DUSK;
	strcat( buf, "&cThe strength of the &Psun&c wavers as the &znight&c slowly takes hold.&w\n\r" );
        break;

    case 21:
	time_info.sunlight = SUN_DARK;
	strcat( buf, "&zDarkness &cspreads its embrace about the land.&w\n\r" );
	break;

    case 24:
	time_info.sunlight = SUN_MIDNIGHT;
        if ( time_info.phase_white == MOON_FULL )
	strcat( buf, "&cThe full &Wmoon&c smiles down upon you.&w\n\r" );
        if ( time_info.phase_white == MOON_FHALF
          || time_info.phase_white == MOON_NHALF )
	strcat( buf, "&cThe half &Wmoon&c rests overhead.&w\n\r" );
        if ( time_info.phase_white == MOON_FCRESCENT
          || time_info.phase_white == MOON_NCRESCENT ) 
	strcat( buf, "&cThe crescent &Wmoon&c sits high within its midnight throne.&w\n\r" );
        if ( time_info.phase_white == MOON_NEW )
	strcat( buf, "&cThe shadow of the new &Wmoon&c looms high above you.&w\n\r" );
        if ( time_info.phase_white == MOON_FTHIRD
          || time_info.phase_white == MOON_NTHIRD )
	strcat( buf, "&cThe fat crescent &Wmoon&c sits high within its midnight throne.&w\n\r" );
	time_info.hour = 0;
	time_info.day++;
	time_info.moon_white++;
	time_info.moon_shadow++;
	time_info.moon_blood++;

        day = time_info.day + 1;
             if ( day > 4 && day <  20 ) suf = "th";
        else if ( day % 10 ==  1       ) suf = "st";
        else if ( day % 10 ==  2       ) suf = "nd";
        else if ( day % 10 ==  3       ) suf = "rd";
        else                             suf = "th";

        sprintf( buf,
            "The day of &G%s, &Y%d%s&C of the Month of &G%s&C has begun.",
            day_name[day % 7],
            day, suf,
            month_name[time_info.month] );
        info( buf, 0, 0, PORT_PAP );

        sprintf( buf,
            "&W%s, &Othe &Y%d%s&O day in the Month of &W%s&O begins now.",
            day2_name[day % 7],
            day, suf,
            month2_name[time_info.month] );
        info( buf, 0, 0, PORT_ROI );
        buf[0] = '\0';
	break;
    }

    if ( time_info.moon_white >= 10 )
    {
        time_info.moon_white = 0;
        time_info.phase_white++;
        if (time_info.phase_white == 8)
        time_info.phase_white = 0;
    }

    if ( time_info.moon_shadow >= 20 )
    {
        time_info.moon_shadow = 0;
        time_info.phase_shadow++;
        if (time_info.phase_shadow == 8)
        time_info.phase_shadow = 0;
    }

    if ( time_info.moon_blood >= 30 )
    {
        time_info.moon_blood = 0;
        time_info.phase_blood++;
        if (time_info.phase_blood == 8)
        time_info.phase_blood = 0;
    }

    if ( time_info.day   >= 30 )
    {
	time_info.day = 0;
	time_info.month++;
    }

    if ( time_info.month >= 12 )
    {
	time_info.month = 0;
	time_info.year++;
    }

    if ( buf[0] != '\0' )
    {
	for ( d = descriptor_list; d; d = d->next )
	{
	    if ( d->connected == CON_PLAYING
		&& outdoor_check( d->character->in_room )
		&& IS_AWAKE  ( d->character ))
		send_to_char(AT_BLUE, buf, d->character );
	}
    }

    return;
}

void sky_update( ROOM_INDEX_DATA *pRoom, int weather_state, int cloud_state )
{
    DESCRIPTOR_DATA *d;
    char             buf [ MAX_STRING_LENGTH ];
    int		     current_weather = get_weather(pRoom);
    int		     current_clouds = get_clouds(pRoom);
    int	             door;

    if ( weather_state != current_weather )
    { 
     buf[0] = '\0';
    
	if ( cloud_state == SKY_CLEAR )
        {
         if ( current_clouds == SKY_PARTIAL )
	  strcat( buf, "&CSome &Wclouds&C move into the sky.\n\r" );
         else if ( current_clouds == SKY_CLOUDY )
	  strcat( buf, "&CLarge &wclouds&C roam across the sky.\n\r" );
         else if ( current_clouds == SKY_OVERCAST )
          strcat( buf, "&CLarge &zclouds&C darken the sky.\n\r" );
        }

	if ( cloud_state == SKY_PARTIAL )
        {
         if ( current_clouds == SKY_CLEAR )
          strcat( buf, "&CThe &Wclouds&C disappear.\n\r" );
         else if ( current_clouds == SKY_CLOUDY )
	  strcat( buf, "&CLarge &wclouds&C roam across the sky.\n\r" );
         else if ( current_clouds == SKY_OVERCAST )
          strcat( buf, "&CLarge &zclouds&C darken the sky.\n\r" );
        }

	if ( cloud_state == SKY_CLOUDY )
        {
         if ( current_clouds == SKY_PARTIAL )
          strcat( buf, "&CThe &wclouds&C begin to disappear.\n\r" );
         else if ( current_clouds == SKY_CLEAR )
          strcat( buf, "&CThe &Wclouds&C disappear.\n\r" );
         else if ( current_clouds == SKY_OVERCAST )
          strcat( buf, "&CLarge &zclouds&C darken the sky.\n\r" );
        }

	if ( cloud_state == SKY_OVERCAST )
        {
         if ( current_clouds == SKY_PARTIAL )
          strcat( buf, "&CThe &wclouds&C begin to disappear.\n\r" );
         else if ( current_clouds == SKY_CLOUDY )
          strcat( buf, "&CThe &wclouds&C begin to disappear.\n\r" );
         else if ( current_clouds == SKY_CLEAR )
          strcat( buf, "&CThe &Wclouds&C disappear.\n\r" );
        }

    if ( buf[0] != '\0' )
    {
	for ( d = descriptor_list; d; d = d->next )
	{
	if ( d->connected != CON_PLAYING )
	    continue;

         if ( d->character->in_room != pRoom )
         continue;

	    if ( d->connected == CON_PLAYING
		&& outdoor_check( d->character->in_room )
		&& IS_AWAKE  ( d->character ))
		send_to_char(AT_BLUE, buf, d->character );
	}
    }
   }
    if ( current_clouds != cloud_state )
    {
     buf[0] = '\0';

	if ( weather_state == WEATHER_CLEAR )
        {
         if ( current_weather == WEATHER_RAIN )
          strcat( buf, "&CIt starts to &Br&ca&Bi&cn&C.\n\r" );
         else if ( current_weather == WEATHER_SNOW )
          strcat( buf, "&CIt starts &Ws&Bn&Wo&Bw&Wi&Bn&Wg&C.\n\r" );
        }

	if ( weather_state == WEATHER_RAIN )
        {
         if ( current_weather == WEATHER_CLEAR )
          strcat( buf, "&CIt stops &Br&ca&Bi&cn&Bi&cn&Bg&C.\n\r" );
         else if ( current_weather == WEATHER_SNOW )
          strcat( buf, "&CIt starts &Ws&Bn&Wo&Bw&Wi&Bn&Wg&C.\n\r" );
        }


	if ( weather_state == WEATHER_SNOW )
        {
         if ( current_weather == WEATHER_RAIN )
          strcat( buf, "&CIt starts to &Br&ca&Bi&cn&C.\n\r" );
         else if ( current_weather == WEATHER_CLEAR )
          strcat( buf, "&CIt stops &Ws&Bn&Wo&Bw&Wi&Bn&Wg&C.\n\r" );
        }

    if ( buf[0] != '\0' )
    {
	for ( d = descriptor_list; d; d = d->next )
	{
	if ( d->connected != CON_PLAYING )
	    continue;

         if ( d->character->in_room != pRoom )
         continue;

	    if ( d->connected == CON_PLAYING
		&& outdoor_check( d->character->in_room )
		&& IS_AWAKE  ( d->character ))
		send_to_char(AT_BLUE, buf, d->character );
	}
    }
   }

    if ( weather_state >= WEATHER_RAIN && outdoor_check(pRoom) )
     pRoom->accumulation += (dice(1,3)+pRoom->area->weather);

    for ( door = 0; door <= 5; door++ )
    {
     EXIT_DATA       *texit;

	if ( ( texit = pRoom->exit[door] )
	    && texit->to_room
	    && texit->to_room != pRoom )
	{
         if ( texit->to_room->temp < pRoom->temp )
          texit->to_room->temp++;
         else if ( texit->to_room->temp > pRoom->temp )
          texit->to_room->temp--;
         if ( texit->to_room->humidity < pRoom->humidity )
          texit->to_room->humidity++;
         else if ( texit->to_room->humidity > pRoom->humidity )
          texit->to_room->humidity--;

         if ( pRoom->accumulation > 3 && pRoom->temp > 32 )
         {
          pRoom->accumulation--;
          texit->to_room->accumulation++;
         }
	}
    }

 return;
}

/*
 * Weather change.
 */
void weather_update( AREA_DATA *pArea )
{
    ROOM_INDEX_DATA *pRoom;
    int		     chaos = 0;
    int		     room = 0;
    int		     cloud_state;
    int		     weather_state;

    pArea->tfront_length++;
    pArea->pfront_length++;
    pArea->winddir_length++;

    chaos = dice(6, 10) / (pArea->weather + 1);

    if ( chaos <= pArea->tfront_length )
    {
     if ( pArea->temperature_front == 0 )
     {
      pArea->temperature_front = 1;
      pArea->tfront_length = 0;
     }
     else
     {
      pArea->temperature_front = 0;
      pArea->tfront_length = 0;
     }
    }

    if ( chaos <= pArea->pfront_length )
    {
     if ( pArea->pressure_front == 0 )
     {
      pArea->pressure_front = 1;
      pArea->pfront_length = 0;
     }
     else
     {
      pArea->pressure_front = 0;
      pArea->pfront_length = 0;
     }
    }

    if ( (chaos/2) <= pArea->winddir_length )
    {
     pArea->winddir = (dice(2, 3) - 2);
     pArea->winddir_length = 0;
    }

/* The rest is rooms */

    for ( room = pArea->lvnum; room <= pArea->uvnum; room++ )
    {
     if ( !(pRoom = get_room_index( room )) )
      continue;

     weather_state = get_weather(pRoom);
     cloud_state = get_clouds(pRoom);

     if ( pArea->weather > (dice(1,10)-4) )
     {
      if ( pArea->pressure_front == 0 )
       pRoom->humidity--;
      else
       pRoom->humidity++;

      if ( pArea->temperature_front == 0 )
       pRoom->temp--;
      else
       pRoom->temp++;
     }

      if ( pRoom->temp <
       (pArea->average_temp - ((pArea->weather + 1) * 15)) )
       pRoom->temp += dice(1, 10);

      if ( pRoom->temp >
       (pArea->average_temp + ((pArea->weather + 1) * 15)) )
       pRoom->temp -= dice(1, 10);

     sky_update( pRoom, weather_state, cloud_state);
    }
    return;
}



/*
 * Update all chars, including mobs.
 * This function is performance sensitive.
 */
void char_update( void )
{   
    CHAR_DATA *ch;
    CHAR_DATA *ch_save;
    CHAR_DATA *ch_quit;
    CHAR_DATA *ch_next;	/* XOR */
    CHAR_DATA *vch;	
    RACE_DATA *race_o;
    RACE_DATA *race_n;
    time_t     save_time;
    char       buf[MAX_STRING_LENGTH];

    ch_save	= NULL;
    ch_quit	= NULL;
    save_time	= current_time;

    for ( ch = char_list; ch; ch = ch->next )
    {
	AFFECT_DATA *paf;
	ROOM_AFFECT_DATA *raf;
	POWERED_DATA *pd, *pd_next;

	if ( ch->deleted )
	    continue;

	/*
	 * Find dude with oldest save time.
	 */
	if ( !IS_NPC( ch )
	    && ( !ch->desc || ch->desc->connected == CON_PLAYING )
	    &&   ch->level >= 1
	    &&   ch->save_time < save_time )
	{
	    ch_save	= ch;
	    save_time	= ch->save_time;
	}

	if ( ch->position >= POS_SLEEPING )
	{
          if ( !IS_NPC( ch ) )
          {
           if ( ( ch->race == RACE_AVIASAPIEN
                 || ch->race == RACE_AQUASAPIEN
                 || ch->race == RACE_CHRONOSAPIEN ) 
              && ch->mana < ( MAX_MANA(ch) / 2 ) )
           {
            send_to_char( AT_RED, "You can feel your superform slipping away as your "
                                  "energy falls below its limit.\n\r", ch );
            act( AT_WHITE, "$n appears drained of energy, $s body undergoes a change.",
                 ch, NULL, NULL, TO_ROOM );
            polymorph_char( ch, get_race_data(ch->race), RACE_HUMAN );
           }
          }

	    if ( ch->position == POS_STUNNED )
	      { 
	       ch->position = POS_STANDING; 
	       update_pos( ch, ch );
	      }
	}

        if (is_affected(ch, gsn_berserk)
          && !ch->fighting )
          affect_strip( ch, gsn_berserk );

        if (IS_AFFECTED2(ch, AFF_GRASPING)
          && !ch->fighting )
        {
          REMOVE_BIT( ch->affected_by2, AFF_GRASPING );
	    if ( ch->position == POS_STUNNED )
	      { 
	       ch->position = POS_FIGHTING; 
	       update_pos( ch, ch );
	      }
	}    

        if (IS_AFFECTED2(ch, AFF_GRASPED)
          && !ch->fighting )
        {
          REMOVE_BIT( ch->affected_by2, AFF_GRASPED );
	    if ( ch->position == POS_STUNNED )
	      { 
	       ch->position = POS_FIGHTING; 
	       update_pos( ch, ch );
	      }
	}

        if (IS_AFFECTED2(ch, AFF_BERSERK)
          && !ch->fighting )
        {
          send_to_char(AT_WHITE, "The rage leaves you.\n\r", ch );
          REMOVE_BIT( ch->affected_by2, AFF_BERSERK );
        }

	if ( ch->position == POS_STUNNED )
	    update_pos( ch, ch );

	if ( !IS_NPC( ch ) && ( ch->level < LEVEL_IMMORTAL
			       || ( !ch->desc && !IS_SWITCHED( ch ) ) ) )
	{

	    if ( ++ch->timer >= 10 )
	    {
		if ( !ch->was_in_room && ch->in_room )
		{
		    ch->was_in_room = ch->in_room;
		    if ( ch->fighting )
			stop_fighting( ch, TRUE );
		    if ( ch->engaged )
			stop_shooting( ch, TRUE );
		    send_to_char(AT_GREEN, "You disappear into the void.\n\r", ch );
		    act(AT_GREEN, "$n disappears into the void.",
			ch, NULL, NULL, TO_ROOM );
		    save_char_obj( ch, FALSE );
		    char_from_room( ch );
		    char_to_room( ch, get_room_index( ROOM_VNUM_LIMBO ) );
		}
	    }

	    if ( ch->timer > 20 && !IS_SWITCHED( ch ) )
		ch_quit = ch;

	    gain_condition( ch, COND_DRUNK,  -8 );
	    gain_condition( ch, COND_FULL,   -1 );
	    gain_condition( ch, COND_THIRST, -1 );

         if ( ch->position == POS_RESTING )
         {
	    gain_condition( ch, COND_INSOMNIA, 5 );
         }
         else
         {
          if ( ch->position == POS_SLEEPING )
          {
	     gain_condition( ch, COND_INSOMNIA, 24 );
          }
          else
          {
	     gain_condition( ch, COND_INSOMNIA, -1 );
          }
         }
	}

	for ( paf = ch->affected; paf; paf = paf->next )
	{
	    if ( paf->deleted )
	        continue;
	    if ( paf->duration > 0 )
            {
		if ( paf->bitvector == AFF_FIRESHIELD && paf->duration == 2 ) 
            send_to_char( AT_YELLOW, "The fire about your body flickers and begins to slowly fade away.\n\r", ch );
		if ( paf->bitvector == AFF_ICESHIELD && paf->duration == 2 ) 
            send_to_char( AT_YELLOW, "The ice about your body cracks and begins to melt.\n\r", ch );
		if ( paf->bitvector == AFF_SHOCKSHIELD && paf->duration == 2 ) 
            send_to_char( AT_YELLOW, "The electricity about your body flickers and begins to die down.\n\r", ch );
		if ( paf->bitvector == AFF_CHAOS && paf->duration == 2 ) 
            send_to_char( AT_YELLOW, "The chaos about your body flickers and begins to succumb to the order of the world.\n\r", ch );
		if ( paf->bitvector == AFF_VIBRATING && paf->duration == 2 ) 
            send_to_char( AT_YELLOW, "The vibrations about your body hum and begin to die down.\n\r", ch );
		if ( paf->bitvector == AFF_CUREAURA && paf->duration == 2 ) 
            send_to_char( AT_YELLOW, "Your aura of healing shimmers and begins to fade away.\n\r", ch );
		paf->duration--;
            }
	    else if ( paf->duration < 0 )
            {
		;
            }
	    else
	    {
		if ( !paf->next
		    || paf->next->type != paf->type
		    || paf->next->duration > 0 )
		{
		    if ( paf->type > 0 && skill_table[paf->type].msg_off )
		    {
			send_to_char(C_DEFAULT, skill_table[paf->type].msg_off, ch );
			send_to_char(C_DEFAULT, "\n\r", ch );
		        if ( skill_table[paf->type].room_msg_off )
		        {
			 act(C_DEFAULT,skill_table[paf->type].room_msg_off,
			     ch, NULL, NULL, TO_ROOM );
			}
		    }
		}
	  
		affect_remove( ch, paf );
	    }
	}
	for ( paf = ch->affected2; paf; paf = paf->next )
	{
	    if ( paf->deleted )
	        continue;
	    if ( paf->duration > 0 )
            {
		if ( paf->bitvector == AFF_THORNY && paf->duration == 3 ) 
            send_to_char( AT_YELLOW, "Your armor of thorns begins to decay.\n\r", ch );
		paf->duration--;
             }
	    else if ( paf->duration < 0 )
		;
	    else
	    {
		if ( !paf->next
		    || paf->next->type != paf->type
		    || paf->next->duration > 0 )
		{
		    if ( paf->type > 0 && skill_table[paf->type].msg_off )
		    {
			send_to_char(C_DEFAULT, skill_table[paf->type].msg_off, ch );
			send_to_char(C_DEFAULT, "\n\r", ch );
		        if ( skill_table[paf->type].room_msg_off )
		        {
			 act(C_DEFAULT,skill_table[paf->type].room_msg_off,
			     ch, NULL, NULL, TO_ROOM );
			}
		    }
		}

       if ( paf->bitvector == AFF_GRASPED )
       {
        send_to_char( AT_RED, "The magical grasp over you is broken!\n\r", ch );
	       ch->position = POS_FIGHTING; 
	       update_pos( ch, ch );
	}

       if ( paf->bitvector == AFF_GRASPING )
       {
	       ch->position = POS_FIGHTING; 
	       update_pos( ch, ch );
	}

       if ( paf->bitvector == AFF_POLYMORPH ) 
       {
        sprintf( buf, "%s", ch->name); 
        free_string( ch->oname );
        ch->oname = str_dup(buf);

        free_string( ch->long_descr );
        ch->long_descr = str_dup( "" );

        race_o = get_race_data(ch->race);
  
		affect_remove2( ch, paf );

        race_n = get_race_data(ch->race);
        polymorph_char(ch, race_o, race_n);
        continue;
        }

         affect_remove2(ch, paf );
             }
	  }
	for ( pd = ch->powered; pd; pd = pd_next )
	  {
	  if ( !pd )
	    break;
	  pd_next = pd->next;
	  raf = pd->raf;
	  if ( MT( ch ) < pd->cost )
	    raffect_remove( raf->room, ch, raf );
	  else
	    MT( ch ) -= pd->cost;
	  }

	if ( ch->gspell && --ch->gspell->timer <= 0 )
	{
	  send_to_char(AT_BLUE, "You slowly lose your concentration.\n\r",ch);
	  end_gspell( ch );
	}

	if ( ch->ctimer > 0 )
	  ch->ctimer--;

	/*
	 * Careful with the damages here,
	 *   MUST NOT refer to ch after damage taken,
	 *   as it may be lethal damage (on NPC).
	 */

     if ( IS_AFFECTED( ch, AFF_POISON ) )
     {
      if ( !IS_NPC( ch ) && 
           ch->pcdata->learned[gsn_pain_tolerance] < number_percent() )
      {
       if ( (ch->pcdata->pimm[DAM_DEGEN] + ch->pcdata->mimm[DAM_DEGEN]) > 0 )
       {
        send_to_char(AT_GREEN, "You shiver and suffer.\n\r", ch );
        act(AT_GREEN, "$n shivers and suffers.", ch, NULL, NULL, TO_ROOM );
       }
       damage( ch, ch, 10, gsn_poison, DAM_DEGEN, TRUE );
      }
      else
      if ( IS_NPC( ch ) )
      {
       act(AT_GREEN, "$n shivers and suffers.", ch, NULL, NULL, TO_ROOM );
       damage( ch, ch, 10, gsn_poison, DAM_DEGEN, TRUE );
      }
      else
       update_skpell( ch, gsn_pain_tolerance );

     if ( !ch || ch->position == POS_DEAD )
      return;
	}

	if ( is_affected( ch, gsn_drowfire ) )
	{
	    send_to_char(AT_RED, "The drow fire burns your skin.\n\r", ch );
	    damage( ch, ch, 25, gsn_drowfire, DAM_HEAT, TRUE );
     if ( !ch || ch->position == POS_DEAD )
      return;
	}

	if ( IS_AFFECTED( ch, AFF_FLAMING ))
	{
       if ( !IS_NPC( ch ) )
       {
        if ( (ch->pcdata->pimm[DAM_HEAT] + ch->pcdata->mimm[DAM_HEAT]) > 0 ) 
        {
         send_to_char(AT_RED, "Your skin blisters and burns.\n\r", ch );
         act(AT_RED,
          "$n's body blisters and burns as it is licked in flames.",
          ch, NULL, NULL, TO_ROOM );
        }
        else
         act(AT_RED,
          "$n's body blisters and burns as it is licked in flames.",
          ch, NULL, NULL, TO_ROOM );
       }
        else
         act(AT_RED,
          "$n's body blisters and burns as it is licked in flames.",
          ch, NULL, NULL, TO_ROOM );

        damage( ch, ch, ch->level/5, gsn_incinerate, DAM_HEAT, TRUE );
        if ( !ch || ch->position == POS_DEAD )
         return;
	}

       if ( IS_AFFECTED2( ch, AFF_PLASMA ) )
       {
        if ( ch->in_room != NULL )
	    for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
	    {
             if ( vch == NULL )
             break;
             if ( vch->deleted )
             continue;
             if ( vch == ch )
             continue;

             if ( number_percent( ) > (73 + get_curr_con(vch)) )
             {
	      AFFECT_DATA af;

	      af.type      = gsn_plasma;
              af.level	   = 1;
	      af.duration  = 5;
	      af.location  = APPLY_NONE;
              af.modifier  = 0;
 	      af.bitvector = AFF_PLASMA;
	      affect_join2( vch, &af );
             
              send_to_char( AT_YELLOW, "A film of sticky heat forms on your skin.\n\r", vch );
              }
             if ( vch->next_in_room == NULL )
             break;
             }

       if ( !IS_NPC( ch ) )
       {
        if ( (ch->pcdata->pimm[DAM_POSITIVE] + ch->pcdata->mimm[DAM_POSITIVE]) > 0 ) 
        {
	 send_to_char(AT_RED, "Another layer of your skin melts away under the intense heat of the plasma.\n\r", ch );
	 act(AT_RED, "$n's skin appears to be melting away under the plasmatic pain.", ch, NULL, NULL, TO_ROOM );
        }
        else
	 act(AT_RED, "$n's skin appears to be melting away under the plasmatic pain.", ch, NULL, NULL, TO_ROOM );
       }

	damage( ch, ch, (ch->level / 3), gsn_plasma, DAM_POSITIVE, TRUE );

        if ( !ch || ch->position == POS_DEAD )
         return;
	}

       if ( IS_AFFECTED2( ch, AFF_DISEASED ) )
       {

        if ( ch->in_room != NULL )
        {
         for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
	 {
             if ( vch == NULL )
             continue;
             if ( vch->deleted )
             continue;
             if ( vch == ch )
             continue;

             if ( number_percent( ) > (73 + get_curr_con(vch)) )
             {
	      AFFECT_DATA af;

	      af.type      = gsn_plague;
	      af.level	   = 1;
	      af.duration  = 5;
	      af.location  = APPLY_NONE;
              af.modifier  = 0;
 	      af.bitvector = AFF_DISEASED;
	      affect_join2( vch, &af );
             
              send_to_char( AT_YELLOW, "Plague sores erupt on your skin.\n\r", vch );
              }
             if ( vch->next_in_room == NULL )
             break;
            }
           }

       if ( !IS_NPC( ch ) )
       {
        if ( (ch->pcdata->pimm[DAM_DEGEN] + ch->pcdata->mimm[DAM_DEGEN]) > 0 ) 
        {
	 send_to_char(AT_RED, "You double over in pain from your horrible ailment.\n\r", ch );
	 act(AT_RED, "$n doubles over, wailing.", ch, NULL, NULL, TO_ROOM );
        }
        else
	 act(AT_RED, "$n doubles over, wailing.", ch, NULL, NULL, TO_ROOM );
       }

	 damage( ch, ch, (ch->level / 3), gsn_plague, DAM_DEGEN, TRUE );
     

         if ( !ch || ch->position == POS_DEAD )
          return;
	}

        if ( IS_AFFECTED( ch, AFF_INSANE ) ||
             IS_AFFECTED2( ch, AFF_HALLUCINATING ) )
        {
         MOB_INDEX_DATA *figmentmob;
         CHAR_DATA	*figment;
         char		figbuf[MAX_STRING_LENGTH];
         bool		fig = FALSE;

         figment = rand_figment( ch );
         figmentmob = rand_figment_mob( ch );

         if ( number_percent( ) > 50 )
          fig = TRUE;

         if ( !fig )
         {
          switch( dice( 1, 6 ) )
          {
           case 1:
            switch( dice( 1, 6 ) )
            {
             case 1: act( AT_LBLUE,
              "$N says, 'Hey, what are you doing here?'",
              ch, NULL, figment, TO_CHAR );           break;
             case 2: act( AT_LBLUE,
              "$N says, 'You stole my pink elephant!'",
              ch, NULL, figment, TO_CHAR );           break;
             case 3: act( AT_LBLUE,
              "$N says, 'We could group and go kill that dragon!'",
              ch, NULL, figment, TO_CHAR );           break;
             case 4: act( AT_LBLUE,
              "$N says, 'Communism was just a red herring.'",
              ch, NULL, figment, TO_CHAR );            break;
             case 5: act( AT_LBLUE,
              "$N says, 'I'm late, I'm late, for a very important date!'",
              ch, NULL, figment, TO_CHAR );            break;
             case 6: act( AT_LBLUE,
              "$N says, 'How about them dodgers eh?'",
              ch, NULL, figment, TO_CHAR );            break;
            } break;
           case 2:
            switch( dice( 1, 4 ) )
            {
             case 1: sprintf( figbuf, "%s gives you 50 gold coins.\n\r",
	      figment->oname );
		break;
             case 2: sprintf( figbuf, "%s gives you 500 gold coins.\n\r",
	      figment->oname );
		break;
             case 3: sprintf( figbuf, "%s gives you 5 gold coins.\n\r",
	      figment->oname );
		break;
             case 4: sprintf( figbuf, "%s gives you 1000 gold coins.\n\r",
	      figment->oname );
		break;
            }
            send_to_char( AT_YELLOW, figbuf, ch ); break;
           case 3:
            switch( dice( 1, 6 ) )
            {
             case 1: sprintf( figbuf, "%s leaves east.\n\r",
	      figment->oname );
		break;
             case 2: sprintf( figbuf, "%s leaves west.\n\r",
	      figment->oname );
		break;
             case 3: sprintf( figbuf, "%s leaves south.\n\r",
	      figment->oname );
		break;
             case 4: sprintf( figbuf, "%s leaves north.\n\r",
	      figment->oname );
		break;
             case 5: sprintf( figbuf, "%s leaves up.\n\r",
	      figment->oname );
		break;
             case 6: sprintf( figbuf, "%s leaves down.\n\r",
	      figment->oname );
		break;
            }
            send_to_char( AT_GREY, figbuf, ch ); break;
           case 4:
            switch( dice( 1, 6 ) )
            {
             case 1: sprintf( figbuf, "%s arrives from the east.\n\r",
	      figment->oname );
		break;
             case 2: sprintf( figbuf, "%s arrives from the west.\n\r",
	      figment->oname );
		break;
             case 3: sprintf( figbuf, "%s arrives from the south.\n\r",
	      figment->oname );
		break;
             case 4: sprintf( figbuf, "%s arrives from the north.\n\r",
	      figment->oname );
		break;
             case 5: sprintf( figbuf, "%s arrives from above.\n\r",
	      figment->oname );
		break;
             case 6: sprintf( figbuf, "%s arrives from below.\n\r",
	      figment->oname );
		break;
            }
            send_to_char( AT_GREY, figbuf, ch ); break;
           case 5:
            switch( dice( 1, 2 ) )
            {
             case 1: sprintf( figbuf, "%s &Gmisses &Wyou!&X\n\r",
	      figment->oname );
		break;
             case 2: sprintf( figbuf, "%s's smash &Rscratches &Wyou.\n\r",
	      figment->oname );
		break;
            }
            send_to_char( AT_GREY, figbuf, ch ); break;
           case 6:
            if ( number_percent() > 75 )
             send_to_char( AT_GREEN, "From a great distance, ", ch );
            switch( dice( 1, 4 ) )
            {
             case 1: sprintf( figbuf, "%s smiles at you.\n\r",
	      figment->oname );
		break;
             case 2: sprintf( figbuf, "%s hugs you.\n\r",
	      figment->oname );
		break;
             case 3: sprintf( figbuf, "%s kisses you.\n\r",
	      figment->oname );
		break;
             case 4: sprintf( figbuf, "%s farts in your direction. You gasp for air.\n\r",
	      figment->oname );
		break;
            }
            send_to_char( AT_GREEN, figbuf, ch ); break;
           }
          }
          else
          {
          switch( dice( 1, 6 ) )
          {
           case 1:
            switch( dice( 1, 6 ) )
            {
             case 1: sprintf( figbuf,
              "%s says, 'Hey, what are you doing here?'\n\r",
              figmentmob->short_descr ); break;
             case 2: sprintf( figbuf,
              "%s says, 'You stole my pink elephant!'\n\r",
              figmentmob->short_descr); break;
             case 3: sprintf( figbuf,
              "%s says, 'We could group and go kill that dragon!'\n\r",
              figmentmob->short_descr ); break;
             case 4: sprintf( figbuf,
              "%s says, 'Communism was just a red herring.'\n\r",
              figmentmob->short_descr ); break;
             case 5: sprintf( figbuf,
              "%s says, 'I'm late, I'm late for a very important date!'\n\r",
              figmentmob->short_descr ); break;
             case 6: sprintf( figbuf,
              "%s says, 'How about them Dodgers eh?'\n\r",
              figmentmob->short_descr ); break;
            } send_to_char( AT_LBLUE, figbuf, ch ); break;
           case 2:
            switch( dice( 1, 4 ) )
            {
             case 1: sprintf( figbuf, "%s gives you 50 gold coins.\n\r",
	      figmentmob->short_descr );
		break;
             case 2: sprintf( figbuf, "%s gives you 500 gold coins.\n\r",
	      figmentmob->short_descr );
		break;
             case 3: sprintf( figbuf, "%s gives you 5 gold coins.\n\r",
	      figmentmob->short_descr );
		break;
             case 4: sprintf( figbuf, "%s gives you 1000 gold coins.\n\r",
	      figmentmob->short_descr );
		break;
            }
            send_to_char( AT_YELLOW, figbuf, ch ); break;
           case 3:
            switch( dice( 1, 6 ) )
            {
             case 1: sprintf( figbuf, "%s leaves east.\n\r",
	      figmentmob->short_descr );
		break;
             case 2: sprintf( figbuf, "%s leaves west.\n\r",
	      figmentmob->short_descr );
		break;
             case 3: sprintf( figbuf, "%s leaves south.\n\r",
	      figmentmob->short_descr );
		break;
             case 4: sprintf( figbuf, "%s leaves north.\n\r",
	      figmentmob->short_descr );
		break;
             case 5: sprintf( figbuf, "%s leaves up.\n\r",
	      figmentmob->short_descr );
		break;
             case 6: sprintf( figbuf, "%s leaves down.\n\r",
	      figmentmob->short_descr );
		break;
            }
            send_to_char( AT_GREY, figbuf, ch ); break;
           case 4:
            switch( dice( 1, 6 ) )
            {
             case 1: sprintf( figbuf, "%s arrives from the east.\n\r",
	      figmentmob->short_descr );
		break;
             case 2: sprintf( figbuf, "%s arrives from the west.\n\r",
	      figmentmob->short_descr );
		break;
             case 3: sprintf( figbuf, "%s arrives from the south.\n\r",
	      figmentmob->short_descr );
		break;
             case 4: sprintf( figbuf, "%s arrives from the north.\n\r",
	      figmentmob->short_descr );
		break;
             case 5: sprintf( figbuf, "%s arrives from above.\n\r",
	      figmentmob->short_descr );
		break;
             case 6: sprintf( figbuf, "%s arrives from below.\n\r",
	      figmentmob->short_descr );
		break;
            }
            send_to_char( AT_GREY, figbuf, ch ); break;
           case 5:
            switch( dice( 1, 2 ) )
            {
             case 1: sprintf( figbuf, "%s &Gmisses &Wyou!&X\n\r",
	      figmentmob->short_descr );
		break;
             case 2: sprintf( figbuf, "%s's smash &Rscratches &Wyou.\n\r",
	      figmentmob->short_descr );
		break;
            }
            send_to_char( AT_GREY, figbuf, ch ); break;
           case 6:
            if ( number_percent() > 75 )
             send_to_char( AT_GREEN, "From a great distance, ", ch );
            switch( dice( 1, 4 ) )
            {
             case 1: sprintf( figbuf, "%s smiles at you.\n\r",
	      figmentmob->short_descr );
		break;
             case 2: sprintf( figbuf, "%s hugs you.\n\r",
	      figmentmob->short_descr );
		break;
             case 3: sprintf( figbuf, "%s kisses you.\n\r",
	      figmentmob->short_descr );
		break;
             case 4: sprintf( figbuf, "%s farts in your direction. You gasp for air.\n\r",
	      figmentmob->short_descr );
		break;
            }
            send_to_char( AT_GREEN, figbuf, ch ); break;
           }
          }
        }            

       if ( !ch->fighting )
       {
	if ( ch->position == POS_INCAP )
	{
	    damage( ch, ch, 1, TYPE_UNDEFINED, 0, TRUE );
	}
	else if ( ch->position == POS_MORTAL )
	{
	    damage( ch, ch, 2, TYPE_UNDEFINED, 0, TRUE );
	}
       }
    }

    /*
     * Autosave and autoquit.
     * Check that these chars still exist.
     */
    if ( ch_save || ch_quit )
    {
	for ( ch = char_list; ch; ch = ch->next )
	{
	    if ( ch->deleted )
	        continue;
	    if ( ch == ch_save )
	    {
		save_char_obj( ch, FALSE );
	    }
	    if ( ch == ch_quit )
		do_quit( ch, "" );
	}
    }
/* XOR */
    for(ch = char_list;ch != NULL;ch = ch_next)
    {
      ch_next = ch->next;
      if(ch->summon_timer <= 0)
        ;
      else
        ch->summon_timer--;
      if(IS_NPC(ch) && ch->summon_timer == 0)
      {
        act(AT_BLUE, "$n is consumed by a swirling vortex.", 
         ch, NULL, NULL, TO_ROOM);
        extract_char(ch, TRUE);
      }
    }
/* END */
    return;
}

/*
 * Update all objs.
 * This function is performance sensitive.
 */
void obj_update( void )
{
    OBJ_DATA *obj;
    OBJ_DATA *obj_next;
    OBJ_DATA *corpse;
    int       trash = 0;

    for ( obj = object_list; obj; obj = obj_next )
    {
	CHAR_DATA *rch;
	char      *message;
	obj_next = obj->next;

	if ( !obj || obj->deleted )
	    continue;

	if ( obj->timer < -1 )
	    obj->timer = -1;

	if ( obj->timer < 0 )
	    continue;

        if ( obj->timer == 0 )
         obj->timer++;

        if ( obj->item_type == ITEM_BOMB )
         continue;

	/*
	 *  Bug fix:  used to shift to obj_free if an object whose
	 *  timer ran out contained obj->next.  Bug was reported
	 *  only when the object(s) inside also had timer run out
	 *  during the same tick.     --Thelonius (Monk)
	 */
	if ( --obj->timer == 0 )
	{
	  AREA_DATA *inarea = NULL;
	  bool pccorpse = FALSE;
	  
	    switch ( obj->item_type )
	    {
	    	default:
                 trash = number_range( 0 , 3 );
                 switch( trash )
                 {
                 default:               
                  message = "A glowing hand appears, grabs $p and vanishes.";
                  break;
                 case 1:               
                  message = "$p vanishes.";
                  break;
                 case 2:               
                  message = "A vortex appears, swallows $p and vanishes.";
                  break;
                 case 3:               
                  message = "An imp swoops down, grabs $p and flies away.";
                  break;
                 }
                break;
    		case ITEM_FOUNTAIN:   message = "$p dries up.";         break;
                case ITEM_BLOOD:
		 message = "$p dries up.";
		break;
                case ITEM_BODY_PART:
		 message = "$p rots away.";
		break;
                case ITEM_GAS_CLOUD:
		 message = "$p dissapates.";
		break;
    		case ITEM_CORPSE_NPC: 
                 message = "$p stiffens up and begins to stink.";
                 corpse = create_object(
                 get_obj_index( OBJ_VNUM_RIGOR ), 0 );
                 corpse->timer = number_range( 16, 32 ); ;
                 if ( obj->carried_by )
                  obj_to_room( corpse, obj->carried_by->in_room );
                 else
                  obj_to_room( corpse, obj->in_room );
                  break;
    		case ITEM_RIGOR:  
                 message = "$p's flesh rots away leaving a skeleton.";
                 corpse = create_object(
                 get_obj_index( OBJ_VNUM_SKELETON ), 0 );
                 corpse->timer = number_range( 32, 40 ); ;
                 if ( obj->carried_by )
                  obj_to_room( corpse, obj->carried_by->in_room );
                 else
                  obj_to_room( corpse, obj->in_room );
                  break;
    		case ITEM_SKELETON:   
                message = "$p crumbles to dust and blows away.";
                 break;
    		case ITEM_CORPSE_PC:
                 message = "$p stiffens up and begins to stink.";
                 corpse = create_object(
                 get_obj_index( OBJ_VNUM_RIGOR ), 0 );
                 corpse->timer = number_range( 8 , 16 ); ;
                 if ( obj->carried_by )
                  obj_to_room( corpse, obj->carried_by->in_room );
                 else
                  obj_to_room( corpse, obj->in_room );
    		                      pccorpse = TRUE; break;
    		case ITEM_FOOD:       message = "$p decomposes.";       break;
    		case ITEM_PORTAL:     message = "$p shimmers and is gone."; break;
	        case ITEM_VODOO:      message = "$p slowly fades out of existance."; break;
	        case ITEM_BERRY:      message = "$p rots away."; break;
	    }
    
	    if ( obj->carried_by )
	    {
	        act(C_DEFAULT, message, obj->carried_by, obj, NULL, TO_CHAR );
	    }
	    else
	      if ( obj->in_room
		  && ( rch = obj->in_room->people ) )
	      {
		  act(C_DEFAULT, message, rch, obj, NULL, TO_ROOM );
		  act(C_DEFAULT, message, rch, obj, NULL, TO_CHAR );
	      }
            
            if ( obj->in_room )
              inarea = obj->in_room->area;
            
	    if ( obj == object_list )
	    {
	      if ( !pccorpse || !inarea )
	      {
	        extract_obj( obj );
	        obj_next = object_list;
	      }
	     else
	       strew_corpse( obj, inarea );
	    }
	    else				/* (obj != object_list) */
	    {
	        OBJ_DATA *previous;
   
	        for ( previous = object_list; previous;
		     previous = previous->next )
	        {
		    if ( previous->next == obj )
	     		break;
	        }
   
		if ( !previous )  /* Can't see how, but... */
		    bug( "Obj_update: obj %d no longer in object_list",
    			obj->pIndexData->vnum );
    
              if ( !pccorpse || !inarea )
              {
	        extract_obj( obj );
	        obj_next = previous->next;
	      }
	     else
	       strew_corpse( obj, inarea );
	    }
	}
      if ( (obj) && !obj->deleted &&
            obj->item_type == ITEM_GAS_CLOUD && obj->timer > 0 )
       gas_spread( obj );
    }

    return;
}

/* Yeah yeah, what a waste of processor time, blah blah, shove it buddy */
void timebomb_update( void )
{
    OBJ_DATA *obj;
    OBJ_DATA *obj_next;

    for ( obj = object_list; obj; obj = obj_next )
    {
	obj_next = obj->next;

	if ( !obj || obj->deleted )
	    continue;

	if ( obj->timer < -1 )
	    obj->timer = -1;

	if ( obj->timer < 0 )
	    continue;

        if ( obj->item_type != ITEM_BOMB )
         continue;

	if ( --obj->timer == 0 )
         bomb_explode( obj, obj->in_room );

 }
 return;
}

/*
 * Aggress.
 *
 * for each mortal PC
 *     for each mob in room
 *         aggress on some random PC
 *
 * This function takes .2% of total CPU time.
 *
 * -Kahn
 */
void aggr_update( void )
{
    CHAR_DATA       *ch;
    CHAR_DATA       *mch;
    CHAR_DATA       *vch;
    CHAR_DATA       *vch_next;
    CHAR_DATA       *victim;
    DESCRIPTOR_DATA *d;
    /*
     * Let's not worry about link dead characters. -Kahn
     */
    for ( d = descriptor_list; d; d = d->next )
    {
	ch = d->character;

	if ( d->connected != CON_PLAYING
	    || !ch
	    || !ch->in_room )
	    continue;

	/* mch wont get hurt */
	for ( mch = ch->in_room->people; mch; mch = mch->next_in_room )
	{
	    int count;

	    if ( !IS_NPC( mch )
		|| mch->deleted
		|| mch->fighting
		|| IS_AFFECTED( mch, AFF_CHARM )
		|| !IS_AWAKE( mch )
		|| !can_see( mch, ch )
		|| mch->wait > 0
		|| ch->wait > 0 )
		continue;

	    if ( IS_NPC( mch ) && mch->mpactnum > 0
		&& mch->in_room->area->nplayer > 0 )
	    {
	      MPROG_ACT_LIST * tmp_act, *tmp2_act;
	      for ( tmp_act = mch->mpact; tmp_act != NULL;
		    tmp_act = tmp_act->next )
	      {
		mprog_wordlist_check( tmp_act->buf,mch, tmp_act->ch,
				      tmp_act->obj, tmp_act->vo, ACT_PROG );
		free_string( tmp_act->buf );
	      }
	      for ( tmp_act = mch->mpact; tmp_act != NULL; tmp_act = tmp2_act )
	      {
		tmp2_act = tmp_act->next;
		free_mem( tmp_act, sizeof( MPROG_ACT_LIST ) );
	      }
	      mch->mpactnum = 0;
	      mch->mpact    = NULL;
	    }

	    if ( !IS_SET( mch->act, ACT_AGGRESSIVE )
	      || (IS_SET( mch->act, ACT_WIMPY ) && IS_AWAKE( ch ) )
	      || IS_AFFECTED( ch, AFF_PEACE ) )
	      continue;

	    /*
	     * Ok we have a 'ch' player character and a 'mch' npc aggressor.
	     * Now make the aggressor fight a RANDOM pc victim in the room,
	     *   giving each 'vch' an equal chance of selection.
	     */
            count = 0;
	    victim = NULL;
	    for ( vch = mch->in_room->people; vch; vch = vch->next_in_room )
	    {
	        if ( IS_NPC( vch )
		    || vch->deleted
		    || vch->level >= LEVEL_IMMORTAL )
		    continue;

		if ( ( !IS_SET( mch->act, ACT_WIMPY ) || !IS_AWAKE( vch ) )
		    && can_see( mch, vch ) )
		{
		    if ( number_range( 0, count ) == 0 )
			victim = vch;
		    count++;
		}
	    }

	    if ( !victim )
	        continue;

	    multi_hit( mch, victim, TYPE_UNDEFINED );


	} /* mch loop */

    } /* descriptor loop */

    /* I hope this helps that CrAzY assed memory leak, Swift. */
    for ( ch = char_list; ch!= NULL; ch = vch_next )
    {
     vch_next = ch->next;
     /* MOBprogram ACT_PROG trigger */
     if ( IS_NPC( ch ) && ch->mpactnum > 0 )
     {
      MPROG_ACT_LIST * tmp_act, *tmp2_act;
      for ( tmp_act = ch->mpact; tmp_act !=NULL; tmp_act = tmp_act->next )
      {
       mprog_wordlist_check( tmp_act->buf, ch, tmp_act->ch,
       tmp_act->obj, tmp_act->vo, ACT_PROG );
       free_string (tmp_act->buf);
      }

      for (tmp_act = ch->mpact; tmp_act !=NULL; tmp_act = tmp2_act )
      {
       tmp2_act = tmp_act->next;
       free_mem( tmp_act, sizeof(MPROG_ACT_LIST) );
      } 

      ch->mpactnum = 0;
      ch->mpact = NULL;
     }
    }
    /* End of fixing the memork leak, Swift. */
    return;
}

/* Update the check on time for autoshutdown */
void time_update( void )
{
    FILE            *fp;
    char            *curr_time;
    char             buf [ MAX_STRING_LENGTH ];
    
    if ( down_time == "*" )
        return;
    curr_time = ctime( &current_time );
    if ( !str_infix( warning1, curr_time ) )
    {
	sprintf( buf, "First Warning!\n\r%s at %s system time\n\r"
		      "Current time is %s\n\r",
		(stype == 0 ? "Reboot" : "Shutdown"),
		down_time,
		curr_time );
	send_to_all_char( buf, FALSE );
	free_string( warning1 );
	warning1 = str_dup( "*" );
    }
    if ( !str_infix( warning2, curr_time ) )
    {
	sprintf( buf, "Second Warning!\n\r%s at %s system time\n\r"
		      "Current time is %s\n\r",
		(stype == 0 ? "Reboot" : "Shutdown"),
		down_time,
		curr_time );
	send_to_all_char( buf, FALSE );
	free_string( warning2 );
	warning2 = str_dup( "*" );
    }
    if ( !str_infix( down_time, curr_time ) )
    {
	/* OLC 1.1b */
	do_asave( NULL, "" );

        if ( stype == 1 )
        {
	send_to_all_char( "Shutdown by system.\n\r", FALSE );
	log_string( "Shutdown by system.", CHANNEL_GOD, -1 );
	
	end_of_game( );
	}
       else
        {
          send_to_all_char( "Reboot by system.\n\r", FALSE );
          log_string( "Reboot by system.", CHANNEL_GOD, -1 );
          end_of_game( );
          merc_down = TRUE;
          return;
        }

	fclose( fpReserve );
	if ( !( fp = fopen( SHUTDOWN_FILE, "a" ) ) )
	{
	    perror( SHUTDOWN_FILE );
	    bug( "Could not open the Shutdown file!", 0 );
	}
	else
	{
	    fprintf( fp, "Shutdown by System\n" );
	    fclose ( fp );
	}
	fpReserve = fopen ( NULL_FILE, "r" );
	merc_down = TRUE;
    }
    
    return;

}

/*
 * Remove deleted EXTRA_DESCR_DATA from objects.
 * Remove deleted AFFECT_DATA from chars and objects.
 * Remove deleted CHAR_DATA and OBJ_DATA from char_list and object_list.
 */
void list_update( void )
{
            CHAR_DATA *ch;
            CHAR_DATA *ch_next;
            OBJ_DATA  *obj;
            OBJ_DATA  *obj_next;
            TATTOO_DATA  *tattoo;
            TATTOO_DATA  *tattoo_next;
    extern  bool       delete_obj;
    extern  bool       delete_tattoo;
    extern  bool       delete_char;

    if ( delete_char )
        for ( ch = char_list; ch; ch = ch_next )
	  {
	    AFFECT_DATA *paf;
	    AFFECT_DATA *paf_next;
	    
	    for ( paf = ch->affected; paf; paf = paf_next )
	    {
	      paf_next = paf->next;
		
	      if ( paf->deleted || ch->deleted )
	      {
		if ( ch->affected == paf )
		{
		  ch->affected = paf->next;
		}
		else
		{
		  AFFECT_DATA *prev;
		  
		  for ( prev = ch->affected; prev; prev = prev->next )
		  {
		    if ( prev->next == paf )
		    {
		      prev->next = paf->next;
		      break;
		    }
		  }
			
		  if ( !prev )
		  {
		    bug( "List_update: cannot find paf on ch.", 0 );
		    sprintf( log_buf, "Char: %s", ch->name );
		    bug( log_buf, 0 );
		    continue;
		  }
		}
		    
		free_affect( paf );
	      }
	    }

	    
	    for ( paf = ch->affected2; paf; paf = paf_next )
	      {
		paf_next = paf->next;
		
		if ( paf->deleted || ch->deleted )
		  {
		    if ( ch->affected2 == paf )
		      {
			ch->affected2 = paf->next;
		      }
		    else
		      {
			AFFECT_DATA *prev;
			
			for ( prev = ch->affected2; prev; prev = prev->next )
			  {
			    if ( prev->next == paf )
			      {
				prev->next = paf->next;
				break;
			      }
			  }
			
			if ( !prev )
			  {
			    bug( "List_update2: cannot find paf on ch.", 0 );
			    sprintf( log_buf, "Char: %s", ch->name ? ch->name
			     : "(Unknown)" );
			    bug( log_buf, 0 );
			    continue;
			  }
		      }
		    
 		    free_affect ( paf );
		  }
	      }

	    ch_next = ch->next;
	    
	    if ( ch->deleted )
	      {
		if ( ch == char_list )
		  {
		    char_list = ch->next;
		  }
		else
		  {
		    CHAR_DATA *prev;

		    for ( prev = char_list; prev; prev = prev->next )
		      {
			if ( prev->next == ch )
			  {
			    prev->next = ch->next;
			    break;
			  }
		      }
		    
		    if ( !prev )
		      {
			char buf [ MAX_STRING_LENGTH ];
			
			sprintf( buf, "List_update: char %s not found.",
				ch->name );
			bug( buf, 0 );
			continue;
		      }
		  }
		
		free_char( ch );
	      }
	  }

    if ( delete_obj )
      for ( obj = object_list; obj; obj = obj_next )
	{
	  AFFECT_DATA      *paf;
	  AFFECT_DATA      *paf_next;
	  EXTRA_DESCR_DATA *ed;
	  EXTRA_DESCR_DATA *ed_next;

	  for ( ed = obj->extra_descr; ed; ed = ed_next )
	    {
	      ed_next = ed->next;
	      
	      if ( obj->deleted )
		{
  		  free_extra_descr( ed );
		}
	    }

	  for ( paf = obj->affected; paf; paf = paf_next )
	    {
	      paf_next = paf->next;
	      
	      if ( obj->deleted )
		{
		  if ( obj->affected == paf )
		    {
		      obj->affected = paf->next;
		    }
		  else
		    {
		      AFFECT_DATA *prev;
		      
		      for ( prev = obj->affected; prev; prev = prev->next )
			{
			  if ( prev->next == paf )
			    {
			      prev->next = paf->next;
			      break;
			    }
			}

		      if ( !prev )
			{
			  bug( "List_update: cannot find paf on obj.", 0 );
			  continue;
			}
		    }
		  
		  free_affect ( paf );
		}
	    }

	  obj_next = obj->next;

	  if ( obj->deleted )
	    {
	      if ( obj == object_list )
		{
		  object_list = obj->next;
		}
	      else
		{
		  OBJ_DATA *prev;
		  
		  for ( prev = object_list; prev; prev = prev->next )
		    {
		      if ( prev->next == obj )
			{
			  prev->next = obj->next;
			  break;
			}
		    }
		  
		  if ( !prev )
		    {
		      bug( "List_update: obj %d not found.",
			  obj->pIndexData->vnum );
		      continue;
		    }
		}


	      free_string( obj->name        );
	      free_string( obj->description );
	      free_string( obj->short_descr );
	      --obj->pIndexData->count;

	      free_mem( obj, sizeof( *obj ) );
	    }
	}

    if ( delete_tattoo )
      for ( tattoo = tattoo_list; tattoo; tattoo = tattoo_next )
	{
	  AFFECT_DATA      *paf;
	  AFFECT_DATA      *paf_next;

	  for ( paf = tattoo->affected; paf; paf = paf_next )
	    {
	      paf_next = paf->next;
	      
	      if ( tattoo->deleted )
		{
		  if ( tattoo->affected == paf )
		    {
		      tattoo->affected = paf->next;
		    }
		  else
		    {
		      AFFECT_DATA *prev;
		      
		      for ( prev = tattoo->affected; prev; prev = prev->next )
			{
			  if ( prev->next == paf )
			    {
			      prev->next = paf->next;
			      break;
			    }
			}

		      if ( !prev )
			{
			  bug( "List_update: cannot find paf on tat.", 0 );
			  continue;
			}
		    }
		  
		  free_affect ( paf );
		}
	    }

	  tattoo_next = tattoo->next;

	  if ( tattoo->deleted )
	    {
	      if ( tattoo == tattoo_list )
		{
		  tattoo_list = tattoo->next;
		}
	      else
		{
		  TATTOO_DATA *prev;
		  
		  for ( prev = tattoo_list; prev; prev = prev->next )
		    {
		      if ( prev->next == tattoo )
			{
			  prev->next = tattoo->next;
			  break;
			}
		    }
		  
		}


	      free_string( tattoo->short_descr );

	      free_mem( tattoo, sizeof( *tattoo ) );
	    }
	}

    delete_obj     = FALSE;
    delete_tattoo  = FALSE;
    delete_char    = FALSE;
    return;
}


/*
 * Handle all kinds of updates.
 * Called once per pulse from game loop.
 * Random times to defeat tick-timing clients and players.
 */
void update_handler( void )
{
    static int pulse_area;
    static int pulse_mobile;
    static int pulse_violence;
    static int pulse_point;
    static int pulse_timebomb;
    static int pulse_object_decay;
    static int pulse_db_dump;		/* OLC 1.1b */
    static int pulse_combat;		/* XOR pkill */

    /* OLC 1.1b */
    if ( --pulse_db_dump  <= 0 )
    {
	pulse_db_dump	= PULSE_DB_DUMP;
	do_asave( NULL, "" );
        /*save_player_list(); */
    }

    if ( --pulse_area     <= 0 )
    {
	pulse_area	= number_range( PULSE_AREA / 2, 3 * PULSE_AREA / 2 );
	area_update	( );
    }

    if ( --pulse_violence <= 0 )
    {
	pulse_violence  = PULSE_VIOLENCE;
	violence_update ( );
    }

    if ( --pulse_mobile   <= 0 )
    {
	pulse_mobile    = PULSE_MOBILE;
	mobile_update	( );
        orprog_update	( );
    }

    if ( --pulse_timebomb   <= 0 )
    {
	pulse_timebomb    = PULSE_TIMEBOMB;
        timebomb_update	( );
    }

    if ( --pulse_object_decay   <= 0 )
    {
	pulse_object_decay    = PULSE_OBJECT_DECAY;
	obj_update      ( );
    }

    if ( --pulse_point    <= 0 )
    {
        wiznet("A little mouse runs up your body and whispers in your ear 'tick'",
         NULL,NULL,WIZ_TICKS,0,0);
	pulse_point     = number_range( PULSE_TICK / 2, 3 * PULSE_TICK / 2 );
        economy_update	( );
	wtime_update    ( );
	rtime_update    ( );
	room_exit_update ( );
	time_update     ( );
	char_update     ( );
	list_update     ( );
	rdam_update	( );
    }

/* XOR causing alot of lag */
/* yeah yeah, see if its fixed now =) */
    if(--pulse_combat <= 0)
    {
      pulse_combat = PULSE_PER_SECOND;
      comb_update();
    }

/* END */

/* Auction timer update -- Altrag */
    if ( auc_count >= 0 && ++auc_count % (8 * PULSE_PER_SECOND) == 0 )
      auc_update ( );
    
    if ( arena.cch && ++arena.count % (5 * PULSE_PER_SECOND) == 0 )
      arena_update ( );

    chat_update( );

//    time_update( );
    aggr_update( );
    tail_chain( );
    return;
}

/* X combat timer update */
void comb_update()
{
  CHAR_DATA *ch;
  for ( ch = char_list; ch != NULL; ch = ch->next )
  {
    if ( ch->deleted )
      continue;
    if(--ch->combat_timer < 0)
      ch->combat_timer = 0;
  }
}

void arena_update()
{
  if ( !arena.cch )
    return;
  switch ( arena.count / (5 * PULSE_PER_SECOND) )
  {
  case 1:
  case 2:
  case 3:
    if ( arena.och )
    {
    char buf[MAX_INPUT_LENGTH];
    sprintf(buf, "&C%s &coffering &C%s &ca challenge for &W%d &ccoins.",
	    arena.cch->name, arena.och->name, arena.award);
    challenge(buf, 0, 0);
    }
    else
    challenge("&C%s &coffering challenge for &W%d &ccoins.", 
		(int)(arena.cch->name), arena.award);
    return;
  }
  if ( arena.och )
  challenge("&C%s &cwimps out and refuses &C%s&c's challenge.",
	   (int)(arena.och->name), (int)(arena.cch->name) );
  else
  challenge("&C%s&c's challenge not accepted.  Opening arena for new challenger.",
       (int)(arena.cch->name), 0);
  send_to_char( C_DEFAULT, "Your challenge was not accepted.  Refunding "
                "award money.\n\r", arena.cch );
  /* Arena master takes 1/5.. *wink */
  arena.cch->money.gold += ((arena.award*4)/5);
  arena.cch = NULL;
  arena.och = NULL;
  arena.award = 0;
  return;
}

/* Auctioneer timer update -- Altrag */
void auc_update()
{
  extern OBJ_DATA *auc_obj;
  extern CHAR_DATA *auc_held;
  extern CHAR_DATA *auc_bid;
  extern MONEY_DATA auc_cost;
  char buf[MAX_STRING_LENGTH];

  if ( !auc_obj )
    return;

  if ( !auc_held )
  {
    bug( "Auc_update: auc_obj found without auc_held.",0);
    return;
  }

  switch ( auc_count / (8 * PULSE_PER_SECOND) )
  {
  case 1:
    sprintf( buf, "%s for %s (going ONCE)", auc_obj->short_descr, money_string( &auc_cost ) );
    auc_channel( buf );
    sprintf( buf, "%s auctioning %s.", auc_held->name, auc_obj->name );
    log_string( buf, CHANNEL_GOD, -1 );
    return;
  case 2:
    sprintf( buf, "%s for %s (going TWICE)", auc_obj->short_descr, money_string( &auc_cost ) );
    auc_channel( buf );
    sprintf( buf, "%s auctioning %s.", auc_held->name, auc_obj->name );
    log_string( buf, CHANNEL_GOD, -1 );
    return;
  case 3:
    sprintf( buf, "%s for %s (going THRICE)", auc_obj->short_descr, money_string(&auc_cost) );
    auc_channel( buf );
    sprintf( buf, "%s auctioning %s.", auc_held->name, auc_obj->name );
    log_string( buf, CHANNEL_GOD, -1 );
    return;
  }


  if ( auc_bid && ( ( (auc_bid->money.gold*C_PER_G) + (auc_bid->money.silver*S_PER_G) +
       (auc_bid->money.copper) ) > ( (auc_cost.gold*C_PER_G) + (auc_cost.silver*S_PER_G) +
       (auc_cost.copper) ) ) ) 
  {
    sprintf( buf, "%s SOLD! to %s for %s", auc_obj->short_descr, auc_bid->name,
	     money_string( &auc_cost ) );
    add_money( &auc_held->money, &auc_cost );
    spend_money( &auc_bid->money, &auc_cost );

    obj_to_char( auc_obj, auc_bid );
    act( AT_DGREEN, "$p appears in your hands.", auc_bid, auc_obj, NULL, TO_CHAR );
    act( AT_DGREEN, "$p appears in the hands of $n.", auc_bid, auc_obj, NULL,
     TO_ROOM );
  }
  else if ( auc_bid )
   {
     sprintf( buf, "Amount not carried for %s, ending auction.", auc_obj->short_descr );
     obj_to_char( auc_obj, auc_held );
     act( AT_DGREEN, "$p appears in your hands.", auc_held, auc_obj, NULL, TO_CHAR );
     act( AT_DGREEN, "$p appears in the hands of $n.", auc_held, auc_obj, NULL, TO_ROOM );
   }
  else
   {
     sprintf( buf, "%s not sold, ending auction.", auc_obj->short_descr );
     obj_to_char( auc_obj, auc_held );
     act( AT_DGREEN, "$p appears in your hands.", auc_held, auc_obj, NULL, TO_CHAR );
     act( AT_DGREEN, "$p appears in the hands of $n.", auc_held, auc_obj, NULL,
       TO_ROOM );
   }
  auc_channel( buf );

  auc_count = -1;
  auc_cost.gold = auc_cost.silver = auc_cost.copper = 0;
  auc_obj = NULL;
  auc_held = NULL;
  auc_bid = NULL;
  return;
}

/* Also handles portals, sector-affects and gas affects */
void rdam_update( )
{
  ROOM_INDEX_DATA *pRoomIndex;
  ROOM_INDEX_DATA *location;
  DESCRIPTOR_DATA *d;
  CHAR_DATA *ch;
  OBJ_DATA  *obj;

  for ( d = descriptor_list; d; d = d->next )
  {
    if ( d->connected != CON_PLAYING )
      continue;

   ch = d->original ? d->original : d->character;
    if ( !( ch->in_room ) )
      continue;

   if ( ch->level < L_APP )
   {
    if ( rsector_check( ch->in_room ) == SECT_UNDERWATER )
    {
     if ( ( (get_race_data(ch->race))->gills == 0 ) &&
          !IS_AFFECTED( ch, AFF_BREATHE_WATER ) )
     {
      send_to_char( AT_RED, "Your lungs ache as you gasp for air.\n\r", ch );
      act( AT_RED, "$n gasps for air.", ch, NULL, NULL, TO_ROOM );
      damage( ch, ch, (ch->hit / 25), gsn_organ_donor, DAM_INTERNAL, TRUE );
     }
    }

    if ( rsector_check( ch->in_room ) == SECT_HELL && number_percent() < 50 )
    {
      send_to_char( AT_RED, "Fire leaps up from the ground and scalds you.\n\r", ch );
      act( AT_RED, "Fire leaps up from the ground and scalds $n.", ch, NULL, NULL, TO_ROOM );
      damage( ch, ch, dice( 2, 25 ), gsn_molotov, DAM_HEAT, TRUE );
    }

    if ( rsector_check( ch->in_room ) == SECT_SHADOW && number_percent() < 50 )
    {
      send_to_char( AT_RED, "You can hear strange words being uttered as you "
                            "are assailed by an unknown force.\n\r", ch );
      act( AT_RED, "$n doubles over in pain.", ch, NULL, NULL, TO_ROOM );
      damage( ch, ch, dice( 2, 25 ), gsn_darkhand, DAM_VOID, TRUE );
    }

    if ( rsector_check( ch->in_room ) == SECT_ARCTIC )
    {
      send_to_char( AT_RED, "The cold acrtic winds chill you to the bone.\n\r", ch );
      act( AT_RED, "$n shivers from the biting coldness.", ch, NULL, NULL, TO_ROOM );
      damage( ch, ch, dice( 2, 20 ), gsn_frosthand, DAM_COLD, TRUE );
    }
   }

   if ( !IS_NPC(ch) )
   {
    if ( (pRoomIndex = ch->in_room) )
    for ( obj = pRoomIndex->contents; obj; obj = obj->next_content )
    {
     if ( !obj || obj->deleted )
      continue;

     if ( obj->item_type == ITEM_GAS_CLOUD && number_percent() > 25 )
      gasaffect( obj );

     if ( obj->item_type == ITEM_PORTAL
      && ( location = get_room_index( obj->value[0] ) )
      && obj->value[1] > get_curr_str(ch) )
     {
      act( AT_WHITE, "$n is sucked into the $p.", ch, obj, NULL, TO_ROOM ); 
      act( AT_WHITE, "You are sucked into the $p.", ch, obj, NULL, TO_CHAR ); 
      char_from_room( ch );
      char_to_room( ch, location );
      do_look( ch, "auto" );
      act( AT_WHITE, "$n appears suddenly.", ch, NULL, NULL, TO_ROOM ); 
      return;
     }
    }
   }
  }

  return;
}

void strew_corpse( OBJ_DATA *obj, AREA_DATA *inarea )
{
  OBJ_DATA *currobj;
  ROOM_INDEX_DATA *newroom;
  OBJ_DATA *cobj_next;
  
  for ( currobj = obj->contains; currobj; currobj = cobj_next )
  {
     cobj_next = currobj->next_content;
     switch( currobj->item_type )
     {
       case ITEM_FOOD:
       case ITEM_DRINK_CON:
              currobj->value[3] = 1;
              break;
       case ITEM_POTION:
              if ( number_percent( ) < 20 )
              {
                extract_obj( currobj );
                continue;
              }
              break;
       default: break;
     }
     
     if ( number_percent( ) < 2 )
     {
       extract_obj( currobj );
       continue;
     }
     if ( number_percent( ) < 30 )
     {
       obj_from_obj( currobj, FALSE );
       obj_to_room( currobj, obj->in_room );
     }
    else
     {
       obj_from_obj( currobj, FALSE );
       newroom = get_room_index( number_range( inarea->lvnum, inarea->uvnum ) );
       for ( ; !newroom; )
         newroom = get_room_index( number_range( inarea->lvnum, inarea->uvnum ) );
       obj_to_room( currobj, newroom );
     }
  }
  extract_obj( obj );
  return;
}

void orprog_update( void )
{
  OBJ_DATA *obj;
  OBJ_DATA *obj_next;
  AREA_DATA *pArea;

  for ( obj = object_list; obj; obj = obj_next )
  {
    obj_next = obj->next;
    if ( obj->deleted )
      continue;
    /* ie: carried or in room */
    if ( !obj->in_obj && !obj->stored_by &&
	((obj->in_room && obj->in_room->area->nplayer) ||
	 (obj->carried_by && obj->carried_by->in_room &&
	  obj->carried_by->in_room->area->nplayer)) )
      oprog_random_trigger( obj );
  }

  for ( pArea = area_first; pArea; pArea = pArea->next )
    if ( pArea->nplayer > 0 )
    {
      int room;
      ROOM_INDEX_DATA *pRoom;

      for ( room = pArea->lvnum; room <= pArea->uvnum; room++ )
	if ( (pRoom = get_room_index( room )) )
	  rprog_random_trigger( pRoom );
    }

  return;
}

void trap_update( void )
{
  TRAP_DATA *pTrap;

  for ( pTrap = trap_list; pTrap; pTrap = pTrap->next )
    if ( --pTrap->disarm_dur <= 0 )
    {
      pTrap->disarm_dur = 0;
      pTrap->disarmed = FALSE;
    }
  return;
}

/* This is currently specifically for exit_affects -Flux */
/* This probably eats a lot of proc time doesn't it? ;) */
void room_exit_update(void)
{
  AREA_DATA *pArea;
  ROOM_INDEX_DATA *pRoom;
  EXIT_DATA *exit;
  EXIT_AFFECT_DATA *exitaf_next = NULL;
  EXIT_AFFECT_DATA *exitaf;
  int room;
  int door;

  for ( pArea = area_first; pArea; pArea = pArea->next )
   for ( room = pArea->lvnum; room <= pArea->uvnum; room++ )
    if ( (pRoom = get_room_index( room )) )
     for( door = 0; door < 6; door++ )
      if ( (exit = pRoom->exit[door]) )
       for ( exitaf = exit->eAffect; exitaf; exitaf = exitaf_next )
       {
        if ( !exitaf )
         continue;

        exitaf_next = exitaf->next;

        if ( exitaf->duration > 0 )
         exitaf->duration -= 1;

	if ( exitaf->duration <= 0 )
         exit_affect_remove( exit, exitaf );
       }

 return; 
}

void rtime_update( void )
{
  AREA_DATA *pArea;

  for ( pArea = area_first; pArea; pArea = pArea->next )
    if ( pArea->nplayer )
    {
      int room;
      ROOM_INDEX_DATA *pRoom;

      for ( room = pArea->lvnum; room <= pArea->uvnum; room++ )
      {
       if ( !(pRoom = get_room_index( room )) )
        continue;

       rprog_time_trigger( pRoom, time_info.hour );

       if ( rsector_check( pRoom ) == SECT_ASTRAL && number_percent() > 75 
)
        astral_portal_update( pRoom ); 
      }
    }

  return;
}

void astral_portal_update( ROOM_INDEX_DATA *in_room )
{
 OBJ_DATA	*portal;
 OBJ_DATA	*contains;

 for( contains = in_room->contents; contains; contains =
  contains->next_content )
 {
  if ( !(contains) || contains->deleted )
   continue;

  if ( contains->item_type == ITEM_PORTAL )
   break;
 }

 if ( (contains) && contains->item_type == ITEM_PORTAL )
  extract_obj(contains);

 if ( !(portal = create_object(get_obj_index(OBJ_VNUM_ASTRAL_PORTAL), 1)) )
  return;

 portal->value[0] = randroom();

 obj_to_room( portal, in_room );

 if ( !(contains) )
 {
  act( AT_WHITE, "A knife as stark as a black hole appears.",
   NULL, portal, NULL, TO_ROOM );
  act( AT_WHITE,
   "It slices through the very fabric of reality, creating a large rift before vanishing into nothingness.",
   NULL, portal, NULL, TO_ROOM );
 }
 else
  act( AT_WHITE,
   "$p shimmers, its mystical contents flash and change before your eyes.",
   NULL, portal, NULL, TO_ROOM );

 return;
}

void char_queue_update ( CHAR_DATA *ch )
{
 Q_DATA *	new_q;

 if ( ch->wait > 0 )
  ch->wait -= 1;

 if ( ch->wait < 0 )
  ch->wait = 0;

 if ( ch->wait != 0 )
  return;

 if ( ch->doing[0] != '\0' )
  ch->doing[0] = '\0';

 if ( ch->interpqueue != NULL )
 {
  interpret( ch, ch->interpqueue->command );

  if ( (new_q = ch->interpqueue->next) != NULL )
  {
   free_queue( ch->interpqueue );
   ch->interpqueue = new_q;
  }
  else
  {
   free_queue( ch->interpqueue );
   ch->interpqueue = NULL;
  }
 }

 return;
}

void near_per_second_update ( CHAR_DATA *ch )
{

 char_queue_update( ch );

 if ( time_info.phase_shadow == MOON_FULL
     && time_info.phase_blood  == MOON_FULL )
 {
  ;
 }
 else
 {
  if ( !IS_SET(ch->act, PLR_ILLUSION ) )
  {
   if ( ch->hit  < MAX_HIT(ch) )
    ch->hit  += hit_gain( ch );

   if ( ch->hit  > MAX_HIT(ch) )
    ch->hit  = MAX_HIT(ch);

   if ( ch->hit >= MAX_HIT(ch) && (ch->fleeing_from) )
    ch->fleeing_from = NULL;
		
   if ( ch->mana < MAX_MANA(ch) )
    ch->mana += mana_gain( ch );

   if ( ch->move < MAX_MOVE(ch) )
    ch->move += move_gain( ch );
  }
 }
}
