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
#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h> 
#include <sys/stat.h>
#include "merc.h"



bool	can_use_cmd	args( ( int cmd, CHAR_DATA *ch, int level ) );
OBJ_DATA * get_bionic_char args(( CHAR_DATA *ch, int iWear ));

char *	const	where_name	[] =
{
    "<worn on finger>     ",
    "<worn on finger>     ",
    "<worn around neck>   ",
    "<body layer 1>       ",
    "<body layer 2>       ",
    "<body layer 3>       ",
    "<worn on head>       ",
    "<worn in eyes>       ",
    "<worn on face>       ",
    "<orbiting>           ",
    "<worn on legs>       ",
    "<worn on feet>       ",
    "<worn on hands>      ",
    "<worn on arms>       ",
    "<worn as shield>     ",
    "<worn about body>    ",
    "<worn about waist>   ",
    "<worn around wrist>  ",
    "<worn around wrist>  ",
    "<held primary>       ",
    "<held secondary>     ",
    "<worn on ear>        ",
    "<worn on ear>        ",
    "<worn around ankle>  ",
    "<worn around ankle>  ",
    "<worn as decoration> ",
    "<worn as sheath>     ",
    "<worn as sheath>     ",
};

char *	const	tattoo_name	[] =
{
    "{face}               ",
    "{back of neck}       ",
    "{front of neck}      ",
    "{left shoulder}      ",
    "{right shoulder}     ",
    "{left arm}           ",
    "{right arm}          ",
    "{left hand}          ",
    "{right hand}         ",
    "{chest}              ",
    "{back}               ",
    "{left leg}           ",
    "{right leg}          ",
    "{left ankle}         ",
    "{right ankle}        ",
    "{forehead}           ",
};

char * const bionic_where [] =
{
    "<bionic eye left>   ",
    "<bionic eye right>  ",
    "<bionic body>       ",
    "<bionic arm left>   ",
    "<bionic arm right>  ",
    "<bionic hand left>  ",
    "<bionic hand right> ",
    "<bionic leg left>   ",
    "<bionic leg right>  ",
    "<bionic implant>    ", // 1
    "<bionic implant>    ", // 2
    "<bionic implant>    ", // 3
    "<bionic implant>    ", // 4
    "<bionic implant>    ", // 5
    "<bionic implant>    ", // 6
    "<bionic implant>    ", // 7
    "<bionic implant>    ", // 8
    "<bionic implant>    ", // 9
    "<bionic implant>    ", // 10
    "<memory chip>       ", // 1
    "<memory chip>       ", // 2
    "<memory chip>       ", // 3
    "<memory chip>       "  // 4
};


/*
 * Local functions.
 */
char *	format_obj_to_char	args( ( OBJ_DATA *obj, CHAR_DATA *ch,
				       bool fShort ) );
char *	format_tattoo_to_char	args( ( TATTOO_DATA *tattoo, CHAR_DATA *ch ) );
void	show_list_to_char	args( ( OBJ_DATA *list, CHAR_DATA *ch,
				       bool fShort, bool fShowNothing ) );
void	show_char_to_char_0	args( ( CHAR_DATA *victim, CHAR_DATA *ch ) );

void show_char_to_char_1     args( ( CHAR_DATA *victim, CHAR_DATA *ch, char *argument ) );

void	show_char_to_char	args( ( CHAR_DATA *list, CHAR_DATA *ch ) );
void    do_scry_exits           args( ( CHAR_DATA *ch, ROOM_INDEX_DATA  *scryer ) );


char *format_tattoo_to_char( TATTOO_DATA *tattoo, CHAR_DATA *ch )
{
    static char buf [ MAX_STRING_LENGTH ];
    buf[0] = '\0';

	if ( tattoo->short_descr )
	    strcat( buf, tattoo->short_descr );

    return buf;
}


char *format_obj_to_char( OBJ_DATA *obj, CHAR_DATA *ch, bool fShort )
{
    static char buf [ MAX_STRING_LENGTH ];

    buf[0] = '\0';
    if ( IS_OBJ_STAT( obj, ITEM_INVIS)     )   strcat( buf, "&W(&zinvis&W)&X " );
    if ( IS_OBJ_STAT( obj, ITEM_MAGIC ) 
    ||   IS_OBJ_STAT( obj, ITEM_POISONED )    
    ||   IS_OBJ_STAT( obj, ITEM_FLAME )        
    ||   IS_OBJ_STAT( obj, ITEM_CHAOS )        
    ||   IS_OBJ_STAT( obj, ITEM_SHOCK )        
    ||   IS_OBJ_STAT( obj, ITEM_RAINBOW )   
    ||   IS_OBJ_STAT( obj, ITEM_ICY   )    )   strcat( buf,
    "&W(&Ym&Ca&Pg&Gi&Rc&Ba&Yl&W)&X " );

    if ( IS_OBJ_STAT( obj, ITEM_GLOW )     )   strcat( buf,
    "&Y(&Wglowing&Y)&X ");
    if ( IS_OBJ_STAT( obj, ITEM_HUM )      )   strcat( buf, 
    "&W(&wh&zu&wm&zm&wi&zn&wg&W)&X " );
    if ( IS_OBJ_STAT( obj, ITEM_TECHNOLOGY )      )   strcat( buf, 
    "&W(&Btechnological&W)&X " );
    if ( IS_OBJ_STAT( obj, ITEM_BLESS )      )   strcat( buf, 
    "&z(&Wblessed&z)&X " );
    if ( IS_OBJ_STAT( obj, ITEM_BALANCED )      )   strcat( buf, 
    "&W(&Cbala&Gnced&W)&X " );
    if ( IS_OBJ_STAT( obj, ITEM_SHARP )      )   strcat( buf, 
    "&W(&rs&Oh&Ya&Or&rp&W)&X " );
    if ( fShort )
    {
	if ( obj->short_descr )
	    strcat( buf, obj->short_descr );
    }
    else
    {
	if ( obj->description )
	    strcat( buf, obj->description );
    }

    return buf;
}



/*
 * Show a list to a character.
 * Can coalesce duplicated items.
 */
void show_list_to_char( OBJ_DATA *list, CHAR_DATA *ch, bool fShort, bool fShowNothing )
{
    OBJ_DATA  *obj;
    int		figchance = 0;
    char       buf [ MAX_STRING_LENGTH ];
    char     **prgpstrShow;
    char      *pstrShow;
    int       *prgnShow;
    int       *prgnType;
    int        nShow;
    int        iShow;
    int        count;
    bool       fCombine;

    if ( !ch->desc )
	return;

    /*
     * Alloc space for output lines.
     */
    count = 0;
    for ( obj = list; obj; obj = obj->next_content )
    {
	if ( obj->deleted == TRUE )
	    continue;
	count++;
    }

    prgpstrShow	= alloc_mem( count * sizeof( char * ) );
    prgnShow    = alloc_mem( count * sizeof( int )    );
    prgnType    = alloc_mem( count * sizeof( int )    );
    nShow	= 0;

    /*
     * Format the list of objects.
     */
    for ( obj = list; obj; obj = obj->next_content )
    { 
	if ( IS_SET( obj->wear_loc, WEAR_NONE ) &&
	     obj->bionic_loc == -1 && can_see_obj( ch, obj ) )
	{
	    pstrShow = format_obj_to_char( obj, ch, fShort );
	    fCombine = FALSE;

	    if ( IS_NPC( ch ) || IS_SET( ch->act, PLR_COMBINE ) )
	    {
		/*
		 * Look for duplicates, case sensitive.
		 * Matches tend to be near end so run loop backwords.
		 */
		for ( iShow = nShow - 1; iShow >= 0; iShow-- )
		{
		    if ( !strcmp( prgpstrShow[iShow], pstrShow ) )
		    {
			prgnShow[iShow]++;
			fCombine = TRUE;
			break;
		    }
		}
	    }

	    /*
	     * Couldn't combine, or didn't want to.
	     */
	    if ( !fCombine )
	    {
		prgpstrShow [nShow] = str_dup( pstrShow );
		prgnType    [nShow] = obj->item_type;
		prgnShow    [nShow] = 1;
		nShow++;
	    }
	}
    }

    /*
     * Output the formatted list.
     */
    for ( iShow = 0; iShow < nShow; iShow++ )
    {
	if ( IS_NPC( ch ) || IS_SET( ch->act, PLR_COMBINE ) )
	{
	    if ( prgnShow[iShow] != 1 )
	    {
		switch( prgnType[iShow] )
		{
		default:
		     sprintf( buf, "(%2d) ", prgnShow[iShow] );
		     send_to_char(AT_GREEN, buf, ch );
		     break;
		case ITEM_FOOD:
		case ITEM_BERRY:
		case ITEM_PILL:
		     sprintf( buf, "(%2d) ", prgnShow[iShow] );
		     send_to_char(AT_ORANGE, buf, ch );
		     break;
		case ITEM_FOUNTAIN:
		case ITEM_DRINK_CON:
		case ITEM_POTION:
		     sprintf( buf, "(%2d) ", prgnShow[iShow] );
		     send_to_char(AT_BLUE, buf, ch );
		     break;
		case ITEM_MONEY:
		     sprintf( buf, "(%2d) ", prgnShow[iShow] );
		     send_to_char(AT_YELLOW, buf, ch );
		     break;
		}   
	    }
	    else
	    {
		send_to_char(C_DEFAULT, "     ", ch );
	    }
	}
		switch( prgnType[iShow] )
                {
		default:
	             send_to_char(AT_GREEN, prgpstrShow[iShow], ch );
	             send_to_char(C_DEFAULT, "\n\r", ch );
		     break;
		case ITEM_FOOD:
		case ITEM_PILL:
	             send_to_char(AT_ORANGE, prgpstrShow[iShow], ch );
	             send_to_char(C_DEFAULT, "\n\r", ch );
		     break;
		case ITEM_FOUNTAIN:
		case ITEM_DRINK_CON:
		case ITEM_POTION:
	             send_to_char(AT_BLUE, prgpstrShow[iShow], ch );
	             send_to_char(C_DEFAULT, "\n\r", ch );
		     break;
		case ITEM_MONEY:
	             send_to_char(AT_YELLOW, prgpstrShow[iShow], ch );
	             send_to_char(C_DEFAULT, "\n\r", ch );
		     break;
		}   
	free_string( prgpstrShow[iShow] );
    }

   while( figchance != 3 )
   {
    if ( (IS_AFFECTED2( ch, AFF_HALLUCINATING ) || 
     IS_AFFECTED( ch, AFF_INSANE )) && number_percent() > 55 )
    {
     OBJ_INDEX_DATA	*figment;
     char		figbuf[MAX_STRING_LENGTH];
     
     figment = rand_figment_obj( ch );

    if ( fShowNothing )
     sprintf( figbuf, "     %s\n\r", figment->short_descr );
    else
     sprintf( figbuf, "     %s\n\r", figment->description );

     send_to_char( AT_GREEN, figbuf, ch );
     nShow += 1;
    }
    figchance += 1;
   }

    if ( fShowNothing && nShow == 0 )
    {
	if ( IS_NPC( ch ) || IS_SET( ch->act, PLR_COMBINE ) )
	    send_to_char(C_DEFAULT, "     ", ch );
	send_to_char(AT_DGREEN, "Nothing.\n\r", ch );
    }

    /*
     * Clean up.
     */
    free_mem( prgpstrShow, count * sizeof( char * ) );
    free_mem( prgnShow,    count * sizeof( int )    );
    free_mem( prgnType,    count * sizeof( int )    );
    return;
}



void show_char_to_char_0( CHAR_DATA *victim, CHAR_DATA *ch )
{
    char buf [ MAX_STRING_LENGTH ];
    char buf2 [MAX_STRING_LENGTH ];
    char bufseat [MAX_STRING_LENGTH];
    OBJ_DATA   *seat;    

    buf[0] = '\0';
    buf2[0] = '\0';
    if (!victim->desc && !IS_NPC(victim))
        strcat( buf, "(Link-dead) " );
    if ( !IS_NPC( victim ) )
      {
      if ( IS_SET( victim->act, PLR_AFK ) )
	 strcat( buf, "&Y<AFK>&P " );

        if ( victim->pcdata->rank < RANK_STAFF
         && IS_SET( victim->act, PLR_WIZINVIS )
         && IS_SET( victim->act, PLR_CLOAKED ) )
        {
        sprintf( buf, "%s", "&z(Shrouded) " );
        }
        else
        {
      if ( IS_SET( victim->act, PLR_WIZINVIS ) )
	 {
	 sprintf( buf2, "%s %d%s", "&w(Wizinvis", victim->wizinvis,
	 	  ")&P " );
	 strcat( buf, buf2 );
	 }
      if ( IS_SET( victim->act, PLR_CLOAKED ) )
         {
         sprintf( buf2, "%s %d%s", "&w(Cloaked", victim->cloaked,
                  ")&P " );
         strcat( buf, buf2 );
         }
        }
      }
    if ( victim->desc && victim->desc->editor != 0 && get_trust(ch) > LEVEL_IMMORTAL)
      strcat( buf, "&R<&BEDITING&R>&P " );
    if ( IS_AFFECTED( victim, AFF_INVISIBLE )   )
                                                strcat( buf, "(Invis) "      );
    if ( IS_AFFECTED( victim, AFF_HIDE )        )
                                                strcat( buf, "(Hide) "       );
    if ( IS_AFFECTED( victim, AFF_CHARM ) 
    && ( !IS_SET( victim->act, UNDEAD_TYPE( victim ) ) ) )
                                                strcat( buf, "(Charmed) "    );
    if ( IS_AFFECTED( victim, AFF_PEACE )       )
                                                strcat( buf, "(Peaceful) "    );
    if ( IS_AFFECTED( victim, AFF_PASS_DOOR )   )
                                                strcat( buf, "(Translucent) ");
    if ( IS_AFFECTED( victim, AFF_FAERIE_FIRE ) )
                                                strcat( buf, "(Pink Aura) "  );
    if ( is_affected( victim, gsn_drowfire ) ) 
	strcat( buf, "&p(Purple Aura)&X " );
    if ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_KILLER )  )
						strcat( buf, "(KILLER) "     );
    if ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_OUTCAST ) )
                                                strcat( buf, "(OUTCAST) "    );
    if ( IS_NPC(victim ) && IS_SET( victim->act, UNDEAD_TYPE( victim ) ) )
                                                strcat( buf, "(Undead) "     );
    if ( victim->position == POS_STANDING && victim->long_descr[0] != '\0' )
    {
	strcat( buf, victim->long_descr );
        if ( IS_NPC(victim) )
	send_to_char(AT_PINK, buf, ch );
        else
        {
        send_to_char(AT_PINK, buf, ch );
        send_to_char(AT_PINK, "\n\r", ch );
        }
    }
    else
    {
      strcat( buf, PERS( victim, ch ) );
      if ( !IS_NPC( victim ) && !IS_SET( ch->act, PLR_BRIEF ) )
	strcat( buf, victim->pcdata->title );

      switch ( victim->position )
      {
	case POS_DEAD:     strcat( buf, " is DEAD!!"              ); break;
	case POS_MORTAL:   strcat( buf, " is mortally wounded."   ); break;
	case POS_INCAP:    strcat( buf, " is incapacitated."      ); break;
	case POS_STUNNED:  strcat( buf, " is lying here stunned." ); break;
	case POS_SLEEPING: 
               if ( (seat = victim->resting_on) )
		sprintf( bufseat, " is sleeping on %s here.",
                 seat->short_descr );
               else
		sprintf( bufseat, " is sleeping here."       );
               strcat( buf, bufseat ); break;

	case POS_RESTING: 
               if ( (seat = victim->resting_on) )
		sprintf( bufseat, " is resting on %s here.",
                 seat->short_descr );
               else
		sprintf( bufseat, " is resting here."       );
               strcat( buf, bufseat ); break;
	case POS_STANDING: strcat( buf, " is here."               ); break;
	case POS_FIGHTING:
	  strcat( buf, " is here, fighting " );
	  if ( !victim->fighting )
	    strcat( buf, "thin air??" );
	  else if ( victim->fighting == ch )
	    strcat( buf, "YOU!" );
	  else if ( victim->in_room == victim->fighting->in_room )
	  {
	    strcat( buf, PERS( victim->fighting, ch ) );
	    strcat( buf, "." );
	  }
	  else
	    strcat( buf, "someone who left??" );
	  break;
      }

      strcat( buf, "\n\r" );
      buf[0] = UPPER( buf[0] );
      send_to_char(AT_PINK, buf, ch );
    }
    buf2[0] = '\0';
    return;
}


void do_shroud( CHAR_DATA *ch, char *argument )
{

  if ( !can_use_skpell( ch, gsn_shroud ) )
	{
        typo_message( ch );
	return;
	}

    if ( IS_NPC(ch) || ch->pcdata->rank > RANK_STAFF )
    {
     send_to_char( AT_WHITE, "Use Wizinvis or Cloak.\n\r", ch );
     return;
    }

    ch->cloaked = ch->pcdata->rank;
    ch->wizinvis = ch->pcdata->rank;
    if ( IS_SET(ch->act, PLR_CLOAKED) && IS_SET(ch->act, PLR_CLOAKED) )
    {
        REMOVE_BIT(ch->act, PLR_CLOAKED);
        REMOVE_BIT(ch->act, PLR_WIZINVIS); 
    act( AT_YELLOW, "$n removes $s shroud.", ch, NULL, NULL, TO_ROOM );
        send_to_char( AT_WHITE, "You are no longer shrouded.\n\r", ch );
    }
    else
    {
        SET_BIT(ch->act, PLR_WIZINVIS); 
        SET_BIT(ch->act, PLR_CLOAKED);
    act( AT_YELLOW, "$n shrouds $mself.", ch, NULL, NULL, TO_ROOM );
        send_to_char( AT_WHITE, "You shroud yourself.\n\r", ch );
    }
    return;

}

void show_char_to_char_1( CHAR_DATA *victim, CHAR_DATA *ch, char *argument ) 
{
    TATTOO_DATA *tattoo;
    OBJ_DATA *obj;
    char      buf  [ MAX_STRING_LENGTH ];
    int       iWear;
    int       iWearFormat = 0;
    int       percent;
    bool      found;
    bool      chest;
    bool      neck;

    if ( ( can_see( victim, ch ) ) && ( argument[0] == '\0' ) )  
    {
	act(AT_GREY, "$n looks at you.", ch, NULL, victim, TO_VICT    );
	act(AT_GREY, "$n looks at $N.",  ch, NULL, victim, TO_NOTVICT );
    }

/* Put the check here for argument = storage.. display their storage 
   and then return w/o showing char stuff   --Angi */

    if ( ( !str_cmp( argument, "storage" ) ) && ( get_trust( ch ) > L_DIR ) )
    {
      if ( IS_NPC( victim ) )
      {
         send_to_char( AT_WHITE, "NPC's do not have items in storage.\n\r", ch );
      }
      else
      {
	 sprintf( buf, "%s's storage box contains:\n\r", victim->name );  
         send_to_char( AT_WHITE, buf, ch );
   	 show_list_to_char( victim->pcdata->storage, ch, TRUE, TRUE ); 
      }
      return;
    }

    if ( ( argument[0] != '\0' ) && ( get_trust( ch ) > L_DIR ) )
    {
      if ( !( obj = get_obj_here( victim, argument ) ) )
        {    
	  sprintf( buf, "%s is not carrying that item.\n\r", victim->name );
          send_to_char(AT_WHITE, buf, ch );
          return;    
	  }
        else
        {
          switch ( obj->item_type )
          {
            default:
            send_to_char(AT_DGREEN, "That is not a container.\n\r", ch );
            break;

            case ITEM_DRINK_CON:
            if ( obj->value[1] <= 0 )
            {
                send_to_char(AT_BLUE, "It is empty.\n\r", ch );
                oprog_look_in_trigger( obj, ch );
                break;
            }

            sprintf( buf, "%s %s has is %s full of a %s liquid.\n\r",
	 	capitalize(obj->short_descr), victim->name,
                obj->value[1] <     obj->value[0] / 4
                    ? "less than" :
                obj->value[1] < 3 * obj->value[0] / 4
                    ? "about"     : "more than",
                liq_table[obj->value[2]].liq_color
                );

            send_to_char(AT_BLUE, buf, ch );  
            oprog_look_in_trigger( obj, ch );
            break;
        
        case ITEM_FURNITURE:
         if ( obj->value[1] == FURNITURE_DESK ||
              obj->value[1] == FURNITURE_ARMOIR )
         {
	    sprintf( buf, "%s %s has contains:\n\r",
 		capitalize(obj->short_descr), victim->name );
	    send_to_char( AT_WHITE, buf, ch );
            show_list_to_char( obj->contains, ch, TRUE, TRUE );
            oprog_look_in_trigger( obj, ch );
         }
         break;

        case ITEM_RANGED_WEAPON:
         if ( obj->value[0] == RANGED_WEAPON_FIREARM )
         {
	    sprintf( buf, "%s %s has contains:\n\r",
 		capitalize(obj->short_descr), victim->name );
	    send_to_char( AT_WHITE, buf, ch );
            show_list_to_char( obj->contains, ch, TRUE, TRUE );
            oprog_look_in_trigger( obj, ch );
         }
         break;

        case ITEM_CLIP:
	    sprintf( buf, "%s %s has contains:\n\r",
 		capitalize(obj->short_descr), victim->name );
	    send_to_char( AT_WHITE, buf, ch );
            show_list_to_char( obj->contains, ch, TRUE, TRUE );
            oprog_look_in_trigger( obj, ch );
         break;

        case ITEM_ARMOR:
         if ( obj->value[1] == TRUE )
         {
	    sprintf( buf, "%s %s has contains:\n\r",
 		capitalize(obj->short_descr), victim->name );
	    send_to_char( AT_WHITE, buf, ch );
            show_list_to_char( obj->contains, ch, TRUE, TRUE );
            oprog_look_in_trigger( obj, ch );
         }
         break;

        case ITEM_CONTAINER:
        case ITEM_CORPSE_NPC:
        case ITEM_CORPSE_PC:

	    sprintf( buf, "%s %s has contains:\n\r",
 		capitalize(obj->short_descr), victim->name );
	    send_to_char( AT_WHITE, buf, ch );
            show_list_to_char( obj->contains, ch, TRUE, TRUE );
            oprog_look_in_trigger( obj, ch );
            break; 
        }
      return;
      }
     }

    if ( victim->description[0] != '\0' )
    {
	send_to_char(AT_GREEN, victim->description, ch );
    }
    else
    {
	act(AT_GREY, "You see nothing special about $M.", ch, NULL, victim, TO_CHAR );
    }

    if ( MAX_HIT(victim) > 0 )
	percent = ( 100 * victim->hit ) / MAX_HIT(victim);
    else
	percent = -1;

    strcpy( buf, PERS( victim, ch ) );

         if ( percent >= 100 ) strcat( buf, " is in perfect health.\n\r"  );
    else if ( percent >=  90 ) strcat( buf, " is slightly scratched.\n\r" );
    else if ( percent >=  80 ) strcat( buf, " has a few bruises.\n\r"     );
    else if ( percent >=  70 ) strcat( buf, " has some cuts.\n\r"         );
    else if ( percent >=  60 ) strcat( buf, " has several wounds.\n\r"    );
    else if ( percent >=  50 ) strcat( buf, " has many nasty wounds.\n\r" );
    else if ( percent >=  40 ) strcat( buf, " is bleeding freely.\n\r"    );
    else if ( percent >=  30 ) strcat( buf, " is covered in blood.\n\r"   );
    else if ( percent >=  20 ) strcat( buf, " is leaking guts.\n\r"       );
    else if ( percent >=  10 ) strcat( buf, " is almost dead.\n\r"        );
    else                       strcat( buf, " is DYING.\n\r"              );

    buf[0] = UPPER( buf[0] );

         if ( percent >= 100 ) send_to_char( AT_WHITE, buf, ch );
    else if ( percent >=  90 ) send_to_char( AT_WHITE, buf, ch );
    else if ( percent >=  80 ) send_to_char( AT_WHITE, buf, ch );
    else if ( percent >=  70 ) send_to_char( AT_BLUE,  buf, ch );
    else if ( percent >=  60 ) send_to_char( AT_BLUE,  buf, ch );
    else if ( percent >=  50 ) send_to_char( AT_BLUE,  buf, ch );
    else if ( percent >=  40 ) send_to_char( AT_RED,   buf, ch );
    else if ( percent >=  30 ) send_to_char( AT_RED,   buf, ch );
    else if ( percent >=  20 ) send_to_char( AT_RED,   buf, ch );
    else if ( percent >=  10 ) send_to_char( AT_BLOOD, buf, ch );
    else                       send_to_char( (AT_BLOOD+AT_BLINK),  buf, ch );

    found = FALSE;
    chest = FALSE;
    neck = FALSE;
    for ( iWear = 2; iWear < MAX_WEAR; iWear *= 2 )
    {
	    if ( !found )
	    {
		send_to_char(AT_GREY, "\n\r", ch );
		act(AT_WHITE, "$N is using:", ch, NULL, victim, TO_CHAR );
		found = TRUE;
	    }

	if ( ( obj = get_eq_char( victim, iWear ) )
	    && can_see_obj( ch, obj ) )
	{
         if ( iWear == WEAR_BODY_1 || iWear == WEAR_BODY_2 )
         chest = TRUE;
         if ( iWear == WEAR_NECK )
         neck = TRUE;
	    send_to_char(AT_GREEN, where_name[iWearFormat], ch );
	    send_to_char(AT_CYAN, format_obj_to_char( obj, ch, TRUE ), ch );
	    send_to_char(AT_CYAN, "\n\r", ch );
	}
	else if ( !IS_NPC( victim ) && iWear == WEAR_WIELD )
                {
                 if (get_tattoo_char( victim, TATTOO_RIGHT_HAND ) != NULL) 
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_RIGHT_HAND );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_RIGHT_HAND], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
  		  }
                }

	else if ( !IS_NPC( victim ) && iWear == WEAR_WIELD_2 )
                {
                 if (get_tattoo_char( victim, TATTOO_LEFT_HAND ) != NULL) 
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_LEFT_HAND );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_LEFT_HAND], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
  		  }
                }

	else if ( !IS_NPC( victim ) && iWear == WEAR_BODY_3 && chest == FALSE )
                {
                 if (get_tattoo_char( victim, TATTOO_CHEST ) != NULL) 
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_CHEST );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_CHEST], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
  		  }

                 if (get_tattoo_char( victim, TATTOO_BACK ) != NULL) 
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_BACK );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_BACK], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
                  }
                }              

	else if ( !IS_NPC( victim ) && neck == FALSE && iWear == WEAR_NECK )
                {
                 if (get_tattoo_char( victim, TATTOO_FRONT_NECK ) != NULL) 
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_FRONT_NECK );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_FRONT_NECK], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
  		  }

                 if (get_tattoo_char( victim, TATTOO_BACK_NECK ) != NULL)
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_BACK_NECK );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_BACK_NECK], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
                  }
                }              

	else if ( !IS_NPC( victim ) && iWear == WEAR_ANKLE_L )
                {
                 if (get_tattoo_char( victim, TATTOO_LEFT_ANKLE ) != NULL) 
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_LEFT_ANKLE );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_LEFT_ANKLE], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
  		  }
                }

	else if ( !IS_NPC( victim ) && iWear == WEAR_ANKLE_R )
                {
                 if (get_tattoo_char( victim, TATTOO_RIGHT_ANKLE ) != NULL)
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_RIGHT_ANKLE );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_RIGHT_ANKLE], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
                  }
                }              

	else if ( !IS_NPC( victim ) && iWear == WEAR_ON_FACE )
                {
                 if (get_tattoo_char( victim, TATTOO_FACE ) != NULL)
                  {
                  tattoo = get_tattoo_char( victim, TATTOO_FACE );
                  send_to_char(AT_LBLUE, tattoo_name[TATTOO_FACE], ch );
                  send_to_char(AT_CYAN,
                   format_tattoo_to_char( tattoo, victim ), ch );
                  send_to_char(AT_CYAN, "\n\r", ch );
                  }
                }

	else if ( !IS_NPC( victim ) && iWear == WEAR_ARMS )
                {
                 if (get_tattoo_char( victim, TATTOO_LEFT_SH ) != NULL)
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_LEFT_SH );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_LEFT_SH], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
                  }

                 if (get_tattoo_char( victim, TATTOO_RIGHT_SH ) != NULL)
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_RIGHT_SH );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_RIGHT_SH], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
                  }

                 if (get_tattoo_char( victim, TATTOO_LEFT_ARM ) != NULL)
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_LEFT_ARM );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_LEFT_ARM], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
                  }

                 if (get_tattoo_char( victim, TATTOO_RIGHT_ARM ) != NULL)
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_RIGHT_ARM );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_RIGHT_ARM], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
                  }
                }

	else if ( !IS_NPC( victim ) && iWear == WEAR_LEGS )
                {
                 if (get_tattoo_char( victim, TATTOO_LEFT_LEG ) != NULL)
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_LEFT_LEG );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_LEFT_LEG], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
                  }

                 if (get_tattoo_char( victim, TATTOO_RIGHT_LEG ) != NULL)
                  {
                   tattoo = get_tattoo_char( victim, TATTOO_RIGHT_LEG );
                   send_to_char(AT_LBLUE, tattoo_name[TATTOO_RIGHT_LEG], ch );
                   send_to_char(AT_CYAN,
                    format_tattoo_to_char( tattoo, victim ), ch );
                   send_to_char(AT_CYAN, "\n\r", ch );
                  }
                }
     iWearFormat += 1;
    }

    if ( victim != ch
	&& !IS_NPC( ch ) 
	&& ( number_percent( ) < ch->pcdata->learned[gsn_peek]  || ( ch->race == RACE_KENDER ) ) )
    {
      if ( ( IS_IMMORTAL( victim ) ) && ( !IS_IMMORTAL( ch ) ) )
        send_to_char( AT_WHITE, "\n\rYou cannot peek into an Immortal's inventory.\n\r", ch );
      else
      {
	send_to_char(AT_WHITE, "\n\rYou peek at the inventory:\n\r", ch );
	show_list_to_char( victim->carrying, ch, TRUE, TRUE );
      }
    }

    return;
}



void show_char_to_char( CHAR_DATA *list, CHAR_DATA *ch )
{
    CHAR_DATA	*rch;
    CHAR_DATA	*rch2;
    int		figchance = 0;

    for ( rch = list; rch; rch = rch->next_in_room )
    {
        if ( rch->deleted || rch == ch )
	    continue;
	
	if ( !(rch->desc) 
         && !IS_NPC(rch)
         && get_trust( ch ) < L_APP )
	    continue;

	if ( !IS_NPC( rch )
	    && IS_SET( rch->act, PLR_WIZINVIS )
	    && get_trust( ch ) < rch->wizinvis )
	    continue;

	if ( can_see( ch, rch ) )
	{

         if ( (IS_AFFECTED2( ch, AFF_HALLUCINATING ) ||
             IS_AFFECTED( ch, AFF_INSANE )) && number_percent() > 55 )
         {
           rch2 = rch;
           rch = rand_figment( ch );
	    show_char_to_char_0( rch, ch );
          rch = rch2;
         }
         else
	    show_char_to_char_0( rch, ch );

	}
	else if ( room_is_dark( ch->in_room )
		 && IS_AFFECTED( rch, AFF_INFRARED ) )
	{
	    send_to_char(AT_RED, "You see glowing red eyes watching YOU!\n\r", ch );
	}
    }

   while( figchance != 5 )
   {
    if ( (IS_AFFECTED2( ch, AFF_HALLUCINATING ) || 
     IS_AFFECTED( ch, AFF_INSANE )) && number_percent() > 55 )
    {
     MOB_INDEX_DATA	*figment;
     char		figbuf[MAX_STRING_LENGTH];
     
     figment = rand_figment_mob( ch );

     sprintf( figbuf, "%s", figment->long_descr );
     send_to_char( AT_PINK, figbuf, ch );
    }
    figchance += 1;
   }
    return;
} 



bool check_blind( CHAR_DATA *ch )
{
    if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_HOLYLIGHT ) )
	return TRUE;

    if ( IS_AFFECTED( ch, AFF_BLIND ) )
    {
	send_to_char(AT_WHITE, "You can't see a thing!\n\r", ch );
	return FALSE;
    }

    return TRUE;
}


void do_revert( CHAR_DATA *ch, char *argument )
{
    char       buf [MAX_STRING_LENGTH]; 

  if ( IS_NPC(ch) )
	{
        typo_message( ch );
	return;
	}

       sprintf( buf, "%s", ch->name); 
       free_string( ch->oname );
       ch->oname = str_dup(buf);

       sprintf( buf, "%s", ch->pcdata->otitle); 
       free_string( ch->pcdata->title );
       ch->pcdata->title = str_dup(buf);

       free_string( ch->long_descr );
       ch->long_descr = str_dup( "" );
    send_to_char(AT_BLUE, "You have succesfully reverted.\n\r", ch );
 return;
}
void do_disguise( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       buf [MAX_STRING_LENGTH]; 
    char       arg1 [ MAX_INPUT_LENGTH ];

  if ( IS_NPC(ch ) )
	return;

  if ( !can_use_skpell( ch, gsn_disguise ) )
  {
   typo_message( ch );
   return;
  }
  
    argument = one_argument( argument, arg1  );
    if ( arg1[0] == '\0' )
    {
	send_to_char(AT_WHITE, "Syntax: disguise <name>\n\r" , ch );
	return;
    }
    if ( !( victim = get_char_world( ch, arg1 ) ) )
    {
	send_to_char(AT_WHITE, "They aren't here.\n\r" , ch );
	return;
    }
          if ( victim == ch )
      {
         send_to_char( AT_BLUE, "A bit narcissistic are we?\n\r", ch );
         return;
      }
      if (ch->pcdata->rank < RANK_BOSS && victim->pcdata->rank > RANK_STAFF )
      {
         send_to_char( AT_BLUE, "You aspirations are too high.\n\r", ch );
         return;
      }
    
    if (!IS_NPC(victim))
      {
       sprintf( buf, "%s", ch->pcdata->title); 
       free_string( ch->pcdata->otitle );
       ch->pcdata->otitle = str_dup(buf);

       sprintf( buf, "%s", victim->name); 
       free_string( ch->oname );
       ch->oname = str_dup(buf);

       sprintf( buf, "%s", victim->pcdata->title); 
       free_string( ch->pcdata->title );
       ch->pcdata->title = str_dup(buf);

       sprintf( buf, "%s %s is here.", victim->name, victim->pcdata->title );
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);
      }
    else
      {
       sprintf( buf, "%s", victim->long_descr );
       free_string( ch->long_descr );
       ch->long_descr = str_dup(buf);

       sprintf( buf, "%s", victim->short_descr); 
       free_string( ch->oname );
       ch->oname = str_dup(buf);

       sprintf( buf, "%s", ch->pcdata->title); 
       free_string( ch->pcdata->otitle );
       ch->pcdata->otitle = str_dup(buf);

       sprintf( buf, "."); 
       free_string( ch->pcdata->title );
       ch->pcdata->title = str_dup(buf);

      }
    send_to_char(AT_BLUE, "You have succesfully disguised yourself.\n\r", ch );
    return;
}

void do_look( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *obj;
    CHAR_DATA *victim;
    EXIT_DATA *pexit;
    char       buf  [ MAX_STRING_LENGTH ];
    char       arg1 [ MAX_INPUT_LENGTH  ];
    char       arg2 [ MAX_INPUT_LENGTH  ];
    char      *pdesc;
    ROOM_INDEX_DATA *portroom;
    int        door;
    extern OBJ_DATA *auc_obj;
    extern MONEY_DATA auc_cost;

    if ( !IS_NPC( ch ) && !ch->desc ) 
	return;

    if ( ch->position < POS_SLEEPING )
    {
	send_to_char(AT_CYAN, "You can't see anything but stars!\n\r", ch );
	return;
    }

    if ( ch->position == POS_SLEEPING )
    {
	send_to_char(AT_CYAN, "You can't see anything, you're sleeping!\n\r", ch );
	return;
    }

    if ( is_raffected( ch->in_room, gsn_globedark )
     && !IS_SET( ch->act, PLR_HOLYLIGHT )
     && ch->race != RACE_DROW )
    {
     send_to_char(AT_DGREY, "It's completely and utterly black!\n\r", ch );
     return;
    }

    if ( !check_blind( ch ) )
     return;

    if ( !IS_NPC( ch )
	&& !IS_SET( ch->act, PLR_HOLYLIGHT )
	&& room_is_dark( ch->in_room ) 
        && ( (get_race_data(ch->race))->truesight == 0 )
        && ( (get_race_data(ch->race))->infrared == 0 ) )
    {
     send_to_char(AT_DGREY, "It is pitch black...\n\r", ch );
     show_char_to_char( ch->in_room->people, ch );
     return;
    }

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( arg1[0] == '\0' || !str_cmp( arg1, "auto" ) )
    {
	/* 'look' or 'look auto' */
	send_to_char(AT_WHITE, ch->in_room->name, ch );
	send_to_char(AT_WHITE, "\n\r", ch );

	if ( arg1[0] == '\0'
	    || ( !IS_NPC( ch ) && !IS_SET( ch->act, PLR_BRIEF ) ) )
/* Thalador room color change */
	    send_to_char(ch->in_room->area->def_color, ch->in_room->description, ch );   

	if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_AUTOEXIT ) )
	    do_exits( ch, "auto" );

	show_list_to_char( ch->in_room->contents, ch, FALSE, FALSE );
	show_char_to_char( ch->in_room->people,   ch );
	return;
    }

    if ( !str_prefix( arg1, "in" ) )
    {
	/* 'look in' */
	if ( arg2[0] == '\0' )
	{
	    send_to_char(AT_DGREEN, "Look in what?\n\r", ch );
	    return;
	}

	if ( !( obj = get_obj_here( ch, arg2 ) ) )
	{
	    if ( !str_prefix( arg2, "auction" ) )
	    {
	      int objcount = 1;
	      char buf[MAX_INPUT_LENGTH];

	      if ( !auc_obj )
	      {
		send_to_char(C_DEFAULT, "There is no object being auctioned.\n\r",ch);
		return;
	      }
	      obj_to_char( auc_obj, ch );
	      for ( obj = ch->carrying; obj; obj = obj->next )
	      {
		if ( obj == auc_obj )
		  break;
		objcount++;
	      }
	      sprintf(buf, "in %d.%s", objcount, auc_obj->name );
	      do_look(ch, buf );
	      obj_from_char(auc_obj);
	      return;
	    }
	      
	    send_to_char(AT_DGREEN, "You do not see that here.\n\r", ch );
	    return;
	}

	switch ( obj->item_type )
	{
	default:
	    send_to_char(AT_DGREEN, "That is not a container.\n\r", ch );
	    break;

        case ITEM_ARMOR:
         if ( obj->value[6] != 0 || obj->value[7] != 0 || obj->value[8] != 0 )
         {
	    act(AT_WHITE, "$p has sheathed:", ch, obj, NULL, TO_CHAR );
	    show_list_to_char( obj->sheath, ch, TRUE, TRUE );
	    oprog_look_in_trigger( obj, ch );
         }
         else if ( obj->value[1] == TRUE )
         {
	    act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
	    show_list_to_char( obj->contains, ch, TRUE, TRUE );
	    oprog_look_in_trigger( obj, ch );
         }
         else
          send_to_char(AT_DGREEN, "That is not a container.\n\r", ch );
        break;          

	case ITEM_DRINK_CON:
	    if ( obj->value[1] <= 0 )
	    {
		send_to_char(AT_BLUE, "It is empty.\n\r", ch );
		oprog_look_in_trigger( obj, ch );
		break;
	    }

	    sprintf( buf, "It's %s full of a %s liquid.\n\r",
		obj->value[1] <     obj->value[0] / 4
		    ? "less than" :
		obj->value[1] < 3 * obj->value[0] / 4
		    ? "about"     : "more than",
		liq_table[obj->value[2]].liq_color
		);

	    send_to_char(AT_BLUE, buf, ch );
	    oprog_look_in_trigger( obj, ch );
	    break;

        case ITEM_FURNITURE:
         if ( obj->value[1] == FURNITURE_DESK ||
              obj->value[1] == FURNITURE_ARMOIR )
         {
	    act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
	    show_list_to_char( obj->contains, ch, TRUE, TRUE );
	    oprog_look_in_trigger( obj, ch );
         }
         else
          send_to_char(AT_DGREEN, "That is not a container.\n\r", ch );
         break;

        case ITEM_RANGED_WEAPON:
         if ( obj->value[0] == RANGED_WEAPON_FIREARM )
         {
          OBJ_DATA *clip;
	    act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
            show_list_to_char( obj->contains, ch, TRUE, TRUE );
            oprog_look_in_trigger( obj, ch );
          for ( clip = obj->contains; clip; clip = clip->next_content )
          {
           if ( clip->item_type != ITEM_CLIP )
            continue;

           if ( !clip->contains )
            break;

	   act(AT_WHITE, "You have:", ch, NULL, NULL, TO_CHAR );
           show_list_to_char( clip->contains, ch, TRUE, TRUE );
	   act(AT_WHITE, " left in your clip.", ch, NULL, NULL, TO_CHAR );
           break;
          }
         }
         else
          send_to_char(AT_DGREEN, "That is not a container.\n\r", ch );
         break;

        case ITEM_CLIP:
	    act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
            show_list_to_char( obj->contains, ch, TRUE, TRUE );
            oprog_look_in_trigger( obj, ch );
         break;

	case ITEM_CONTAINER:
	case ITEM_CORPSE_NPC:
	case ITEM_CORPSE_PC:
	    if ( IS_SET( obj->value[1], CONT_CLOSED ) )
	    {
		send_to_char(AT_GREEN, "It is closed.\n\r", ch );
		break;
	    }

	    act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
	    show_list_to_char( obj->contains, ch, TRUE, TRUE );
	    oprog_look_in_trigger( obj, ch );
	    break;

	case ITEM_PORTAL:
	    if ( !( portroom = get_room_index( obj->value[0] ) ) )
	     { 
	      act(AT_WHITE, "You cannot see anything through $p", ch, obj, NULL, TO_CHAR );
	      break;
	     }
	act(AT_GREEN, "You look into $p and see...", ch, obj, NULL, TO_CHAR );
	act(AT_GREEN, "$n looks into $p.", ch, obj, NULL, TO_ROOM);
	send_to_char(AT_WHITE, portroom->name, ch );
	send_to_char(AT_WHITE, "\n\r", ch );

	if ( arg1[0] == '\0'
	    || ( !IS_NPC( ch ) && !IS_SET( ch->act, PLR_BRIEF ) ) )
	    send_to_char(AT_CYAN, portroom->description, ch );
	do_scry_exits( ch, portroom );
	show_list_to_char( portroom->contents, ch, FALSE, FALSE );
	show_char_to_char( portroom->people,   ch );
	    oprog_look_in_trigger( obj, ch );
	break;
	     
	}
	return;
    }

    if ( ( victim = get_char_room( ch, arg1 ) ) )
    {

        if ( (!IS_NPC(ch) && !IS_NPC(victim))
        || (!IS_NPC(ch) && IS_NPC(victim))
        || (IS_NPC(ch) && !IS_NPC(victim)) )
        show_char_to_char_1( victim, ch, arg2 ); 
	return;
    }

    for ( obj = ch->carrying; obj; obj = obj->next_content )
    {
	if ( can_see_obj( ch, obj ) )
	{
	    pdesc = get_extra_descr( ch, arg1, obj->extra_descr );
	    if ( pdesc )
	    {
		send_to_char(AT_BLUE, pdesc, ch );
		oprog_look_trigger( obj, ch );
		return;
	    }

	    pdesc = get_extra_descr( ch, arg1, obj->pIndexData->extra_descr );
	    if ( pdesc )
	    {
		send_to_char(AT_BLUE, pdesc, ch );
		oprog_look_trigger( obj, ch );
		return;
	    }
	}

	if ( is_name(ch, arg1, obj->name ) )
	{
	    send_to_char(AT_GREEN, obj->description, ch );
	    send_to_char(AT_GREEN, "\n\r", ch );
	    oprog_look_trigger( obj, ch );
	    return;
	}
    }

    for ( obj = ch->in_room->contents; obj; obj = obj->next_content )
    {
	if ( can_see_obj( ch, obj ) )
	{
	    pdesc = get_extra_descr( ch, arg1, obj->extra_descr );
	    if ( pdesc )
	    {
		send_to_char(AT_GREEN, pdesc, ch );
		oprog_look_trigger( obj, ch );
		return;
	    }

	    pdesc = get_extra_descr( ch, arg1, obj->pIndexData->extra_descr );
	    if ( pdesc )
	    {
		send_to_char(AT_GREEN, pdesc, ch );
		oprog_look_trigger( obj, ch );
		return;
	    }
	}

	if ( is_name(ch, arg1, obj->name ) )
	{
	    send_to_char(AT_GREEN, obj->description, ch );
	    send_to_char(AT_GREEN, "\n\r", ch );
	    oprog_look_trigger( obj, ch );
	    return;
	}
    }

    pdesc = get_extra_descr( ch, arg1, ch->in_room->extra_descr );
    if ( pdesc )
    {
	send_to_char(AT_WHITE, pdesc, ch );
	return;
    }

         if ( !str_prefix( arg1, "north" ) ) door = 0;
    else if ( !str_prefix( arg1, "east"  ) ) door = 1;
    else if ( !str_prefix( arg1, "south" ) ) door = 2;
    else if ( !str_prefix( arg1, "west"  ) ) door = 3;
    else if ( !str_prefix( arg1, "up"    ) ) door = 4;
    else if ( !str_prefix( arg1, "down"  ) ) door = 5;
    else if ( !str_prefix( arg1, "auction" ) )
    {
      char buf[MAX_STRING_LENGTH];

      if ( !auc_obj )
      {
        send_to_char( C_DEFAULT, "There is no object being auctioned.\n\r",ch);
        return;
      }
      sprintf( buf, "Object: %s\n\r", auc_obj->short_descr );
      send_to_char( AT_WHITE, buf, ch );
      sprintf( buf, "Type: %s   Level: %d\n\r",
               item_type_name( auc_obj ), auc_obj->level );
      send_to_char( AT_WHITE, buf, ch );
      sprintf( buf, "Value: %s  Price: %s\n\r", money_string( &auc_obj->cost ),
		money_string( &auc_cost ) );
      send_to_char( AT_WHITE, buf, ch );
      return;
    }
    else if ( !str_prefix( arg1, "arena" ) && !IS_ARENA(ch) )
    {
      char buf[MAX_STRING_LENGTH];
      
      if ( !arena.cch && !(arena.fch || arena.sch) )
      {
        send_to_char( C_DEFAULT, "There is no challenge being offered.\n\r",
                      ch );
        return;
      }
      if ( arena.cch )
      {
        sprintf(buf, "Challenger: %s.\n\r",
                arena.cch->name );
	sprintf(buf+strlen(buf), "CHALLENGING\n\r" );
	if ( arena.och )
        	sprintf(buf+strlen(buf), 
		"Challenged: %s.\n\r",
		arena.och->name );
	else
		sprintf(buf+strlen(buf), "Challenged: ANYONE\n\r" );
        sprintf(buf+strlen(buf), "Award is %d coins.\n\r", arena.award);
      }
      else
      {
        int fp, sp;
        
        fp = (arena.fch->hit*100)/MAX_HIT(arena.fch);
        sp = (arena.sch->hit*100)/MAX_HIT(arena.sch);
        sprintf(buf, "Challenger: %s.\n\r",
                arena.fch->name );
	sprintf(buf+strlen(buf), "FIGHTING\n\r" );
       	sprintf(buf+strlen(buf), "Challenged: %s.\n\r",
		arena.sch->name );
	sprintf(buf+strlen(buf), "Award is %d coins.\n\r", arena.award );
        send_to_char(AT_WHITE, buf, ch);
        if ( fp > sp )
          sprintf(buf, "%s appears to be winning at the moment.\n\r",
                  arena.fch->name);
        else if ( sp > fp )
          sprintf(buf, "%s appears to be winning at the moment.\n\r",
                  arena.sch->name);
        else
          strcpy(buf, "They appear to be evenly matched at the moment.\n\r");
      }
      send_to_char(AT_WHITE, buf, ch);
      return;
    }
    else
    {
      send_to_char(AT_GREY, "You do not see that here.\n\r", ch );
      return;
    }

    /* 'look direction' */
    if ( !( pexit = ch->in_room->exit[door] ) || !pexit->to_room )
    {
	send_to_char(AT_GREY, "Nothing special there.\n\r", ch );
	return;
    }

    if ( pexit->description && pexit->description[0] != '\0' )
	send_to_char(AT_GREY, pexit->description, ch );
    else
	send_to_char(AT_GREY, "Nothing special there.\n\r", ch );
    if ( ( IS_AFFECTED( ch, AFF_SCRY ) ) || ch->race == RACE_CENTAUR )
    {
        ROOM_INDEX_DATA *rid;
	if ( IS_SET( ch->in_room->room_flags, ROOM_NO_MAGIC ) 
	   || IS_SET( pexit->to_room->room_flags, ROOM_NOSCRY ) )
	  {
	   send_to_char( AT_BLUE, "You failed.\n\r", ch );
	   return;
	  }
	act( AT_BLUE, "You scry to the $T.", ch, NULL, dir_name[door], TO_CHAR );
/*	send_to_char(AT_WHITE, pexit->to_room->name, ch );
	send_to_char(AT_WHITE, "\n\r", ch );

	if ( arg1[0] == '\0'
	    || ( !IS_NPC( ch ) && !IS_SET( ch->act, PLR_BRIEF ) ) )
	    send_to_char(AT_YELLOW, pexit->to_room->description, ch );
	do_scry_exits( ch, pexit->to_room );
	show_list_to_char( pexit->to_room->contents, ch, FALSE, FALSE );
	show_char_to_char( pexit->to_room->people,   ch );*/
	rid = ch->in_room;
	ch->in_room = pexit->to_room;
	do_look( ch, "" );
	ch->in_room = rid;
	eprog_scry_trigger( pexit, ch->in_room, ch );
	return;
    }
    if (   pexit->keyword
	&& pexit->keyword[0] != '\0'
	&& pexit->keyword[0] != ' ' )
    {
      if ( IS_SET( pexit->exit_info, EX_BASHED ) )
	act(AT_GREY, "The $d has been bashed from its hinges.",
	    ch, NULL, pexit->keyword, TO_CHAR );
      else if ( IS_SET( pexit->exit_info, EX_CLOSED ) )
	act(AT_GREY, "The $d is closed.", ch, NULL, pexit->keyword, TO_CHAR );
      else if ( IS_SET( pexit->exit_info, EX_ISDOOR ) )
	act(AT_GREY, "The $d is open.",   ch, NULL, pexit->keyword, TO_CHAR );
    }
    eprog_look_trigger( pexit, ch->in_room, ch );
    return;
}

void do_examine( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA    *obj;
    OBJ_INDEX_DATA    *pObj;
    AFFECT_DATA *paf;
    char        buf [ MAX_STRING_LENGTH ];
    char        arg [ MAX_INPUT_LENGTH  ];
    char        msg [ MAX_INPUT_LENGTH  ];
    int         brk;
    int         sn = skill_lookup( "detect magic" );

    one_argument( argument, arg );

    if( IS_NPC(ch) )
    return;

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Examine what?\n\r", ch );
	return;
    }

  if ( ( obj = get_obj_here( ch, arg ) ) )
  {
   pObj = obj->pIndexData;

   if ( (ch->pcdata->learned[sn]) >= 80 )
   {
    sprintf( buf,
     "\n\r&C%s&W, a.k.a '&C%s&W'\n\r",
     obj->short_descr, obj->name );
    send_to_char(AT_WHITE, buf, ch );

    sprintf( buf,
     "A level &C%d %s &Wowned by &C%s&W.\n\r",
     obj->level, item_type_name(obj), obj->ownedby ? obj->ownedby : "no one" );
    send_to_char(AT_WHITE, buf, ch );

    sprintf( buf,
     "It weighs appx &C%d &Wlbs and is composed of &C%s&W.\n\r",
     obj->weight, flag_string( object_materials, obj->composition ) );
    send_to_char(AT_WHITE, buf, ch );

    brk = obj->durability;

    if ( brk ==  0 ) strcpy( msg, "is utterly destroyed!" );
     else if ( brk <= 10 ) strcpy( msg, "is almost useless." );
     else if ( brk <= 20 ) strcpy( msg, "should be replaced soon." );
     else if ( brk <= 30 ) strcpy( msg, "is in pretty bad shape." );
     else if ( brk <= 40 ) strcpy( msg, "has seen better days." );
     else if ( brk <= 50 ) strcpy( msg, "could use some repairs." );
     else if ( brk <= 60 ) strcpy( msg, "is in average condition." );
     else if ( brk <= 70 ) strcpy( msg, "has the odd dent." );
     else if ( brk <= 80 ) strcpy( msg, "needs a bit of polishing." );
     else if ( brk <= 90 ) strcpy( msg, "looks almost new." );
     else if ( brk <=100 ) strcpy( msg, "is in perfect condition." );
     act(AT_WHITE,"Looking closer, you see that $p $T",ch,obj,msg, TO_CHAR);

    if ( obj->extra_flags )
    {
     sprintf( buf,
      "It is affected by: &C%s&W.\n\r",
     extra_bit_name( obj->extra_flags ) );
     send_to_char(AT_WHITE, buf, ch );
    }

    if ( pObj->invoke_type )
    {
     sprintf( buf,
      "It looks as though %s could be invoked in some way.\n\r",
      obj->short_descr );
     send_to_char(AT_WHITE, buf, ch );
    }

    if ( pObj->join )
    {
     OBJ_INDEX_DATA *joiner;

     if ( !(joiner = get_obj_index( pObj->join )) )
      ;
     else
     {
      sprintf( buf,
      "It looks as though %s could be joined with &C%s &Wto create a new object.\n\r",
      obj->short_descr, joiner->short_descr );
      send_to_char(AT_WHITE, buf, ch );
     }
    }

    if ( (pObj->sep_one) && (pObj->sep_two) )
    {
     sprintf( buf,
      "It looks as though %s could be separated into two different objects.\n\r",
      obj->short_descr );
     send_to_char(AT_WHITE, buf, ch );
    }
   }
   else
   {
    sprintf( buf,
     "\n\r&C%s&W, a.k.a '&C%s&W' is a level &C%d %s.\n\r",
     obj->short_descr, obj->name, obj->level, item_type_name(obj) );
    send_to_char(AT_WHITE, buf, ch );

    sprintf( buf,
     "It weighs appx &C%d &Wlbs and is composed of &C%s&W.\n\r",
     obj->weight, flag_string( object_materials, obj->composition ) );
    send_to_char(AT_WHITE, buf, ch );

    brk = obj->durability;

    if ( brk ==  0 ) strcpy( msg, "is utterly destroyed!" );
     else if ( brk <= 10 ) strcpy( msg, "is almost useless." );
     else if ( brk <= 20 ) strcpy( msg, "should be replaced soon." );
     else if ( brk <= 30 ) strcpy( msg, "is in pretty bad shape." );
     else if ( brk <= 40 ) strcpy( msg, "has seen better days." );
     else if ( brk <= 50 ) strcpy( msg, "could use some repairs." );
     else if ( brk <= 60 ) strcpy( msg, "is in average condition." );
     else if ( brk <= 70 ) strcpy( msg, "has the odd dent." );
     else if ( brk <= 80 ) strcpy( msg, "needs a bit of polishing." );
     else if ( brk <= 90 ) strcpy( msg, "looks almost new." );
     else if ( brk <=100 ) strcpy( msg, "is in perfect condition." );
     act(AT_WHITE,"Looking closer, you see that $p $T",ch,obj,msg, TO_CHAR);

    if ( pObj->invoke_type )
    {
     sprintf( buf,
      "It looks as though %s could be invoked in some way.\n\r",
      obj->short_descr );
     send_to_char(AT_WHITE, buf, ch );
    }

    if ( (pObj->join) )
    {
     sprintf( buf,
      "It looks as though %s could be joined with another object to create a new object.\n\r",
      obj->short_descr );
     send_to_char(AT_WHITE, buf, ch );
    }

    if ( (pObj->sep_one) && (pObj->sep_two) )
    {
     sprintf( buf,
      "It looks as though %s could be separated into two different objects.\n\r",
      obj->short_descr );
     send_to_char(AT_WHITE, buf, ch );
    }
   }

   send_to_char( C_DEFAULT, "\n\r", ch );

  /* Contents */
   switch ( obj->item_type )
   {
    default: break;

    case ITEM_ARMOR:
     if ( obj->value[6] != 0 || obj->value[7] != 0 || obj->value[8] != 0 )
     {
      act(AT_WHITE, "$p has sheathed:", ch, obj, NULL, TO_CHAR );
      show_list_to_char( obj->sheath, ch, TRUE, TRUE );
      oprog_look_in_trigger( obj, ch );
     }

     if ( obj->value[1] == TRUE )
     {
      act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
      show_list_to_char( obj->contains, ch, TRUE, TRUE );
      oprog_look_in_trigger( obj, ch );
     }
      send_to_char( C_DEFAULT, "\n\r", ch );
     break;          

     case ITEM_DRINK_CON:
      if ( obj->value[1] <= 0 )
      {
       send_to_char(AT_BLUE, "It is empty.\n\r", ch );
       oprog_look_in_trigger( obj, ch );
       send_to_char( C_DEFAULT, "\n\r", ch );
       break;
      }

      sprintf( buf, "It's %s full of a %s liquid.\n\r",
 	obj->value[1] <     obj->value[0] / 4
	    ? "less than" :
	obj->value[1] < 3 * obj->value[0] / 4
	    ? "about"     : "more than",
	liq_table[obj->value[2]].liq_color );

      send_to_char(AT_BLUE, buf, ch );
      oprog_look_in_trigger( obj, ch );
      send_to_char( C_DEFAULT, "\n\r", ch );
      break;

     case ITEM_FURNITURE:
      if ( obj->value[1] == FURNITURE_DESK ||
           obj->value[1] == FURNITURE_ARMOIR )
      {
       act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
       show_list_to_char( obj->contains, ch, TRUE, TRUE );
       oprog_look_in_trigger( obj, ch );
      }
      send_to_char( C_DEFAULT, "\n\r", ch );
      break;

     case ITEM_RANGED_WEAPON:
      if ( obj->value[0] == RANGED_WEAPON_FIREARM )
      {
       OBJ_DATA *clip;
       act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
       show_list_to_char( obj->contains, ch, TRUE, TRUE );
       oprog_look_in_trigger( obj, ch );
       for ( clip = obj->contains; clip; clip = clip->next_content )
       {
        if ( clip->item_type != ITEM_CLIP )
         continue;

        if ( !clip->contains )
         break;

        act(AT_WHITE, "You have:", ch, NULL, NULL, TO_CHAR );
        show_list_to_char( clip->contains, ch, TRUE, TRUE );
        act(AT_WHITE, " left in your clip.", ch, NULL, NULL, TO_CHAR );
        break;
       }
      }
      send_to_char( C_DEFAULT, "\n\r", ch );
      break;

      case ITEM_CLIP:
       act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
       show_list_to_char( obj->contains, ch, TRUE, TRUE );
       oprog_look_in_trigger( obj, ch );
       send_to_char( C_DEFAULT, "\n\r", ch );
       break;

	case ITEM_CONTAINER:
	case ITEM_CORPSE_NPC:
	case ITEM_CORPSE_PC:
	    if ( IS_SET( obj->value[1], CONT_CLOSED ) )
	    {
		send_to_char(AT_GREEN, "It is closed.\n\r", ch );
                send_to_char( C_DEFAULT, "\n\r", ch );
		break;
	    }

	    act(AT_WHITE, "$p contains:", ch, obj, NULL, TO_CHAR );
	    show_list_to_char( obj->contains, ch, TRUE, TRUE );
	    oprog_look_in_trigger( obj, ch );
            send_to_char( C_DEFAULT, "\n\r", ch );
	    break;
       }

    if ( (ch->pcdata->learned[sn]) >= 80 )
     show_obj_values( ch, obj, 1 );
    else
     show_obj_values( ch, obj, 2 );

    if ( (ch->pcdata->learned[sn]) >= 80 )
    {
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
      }
    }
    else
    send_to_char(AT_WHITE, "That object isn't here.", ch);
    return;
}



/*
 * Thanks to Zrin for auto-exit part.
 */
void do_scry_exits( CHAR_DATA *ch, ROOM_INDEX_DATA  *scryer )
{
           EXIT_DATA       *pexit;
    extern char *    const  dir_name [ ];
           char             buf      [ MAX_STRING_LENGTH ];
           int              door;
           bool             found;
           bool             fAuto;
    
    fAuto = TRUE;
    strcpy( buf, "&z[&RExits&w:" );
    found = FALSE;
    for ( door = 0; door <= 5; door++ )
    {
	if ( ( pexit = scryer->exit[door] )
	    && pexit->to_room
	    && !IS_SET( pexit->exit_info, EX_CLOSED ) )
	{
	    found = TRUE;
	    if ( fAuto )
	    {
		strcat( buf, "&W " );
		strcat( buf, dir_name[door] );
	    }
        }
    }
    if ( !found )
	strcat( buf, "&WNone" );

    if ( fAuto )
	strcat( buf, "&z]\n\r" );

    send_to_char(AT_WHITE, buf, ch );
    return;
}

void do_exits( CHAR_DATA *ch, char *argument )
{
           EXIT_DATA       *pexit;
           OBJ_DATA	   *obj;
    extern char *    const  dir_name [ ];
           char             buf      [ MAX_STRING_LENGTH ];
           int              door;
           bool             found;
           bool             fAuto;
           bool             secret;
 
    /* This is for that whole secret door listing nonesense, Flux */
    /* The idea is that if it finds a visible exit, it wont print */
    /* "none", if it never finds a visible exit, it will print    */
    secret = FALSE;
    buf[0] = '\0';
    fAuto  = !str_cmp( argument, "auto" );

    if ( !check_blind( ch ) )
	return;

    strcpy( buf, fAuto ? "&z[&WE&Rx&Bi&Wt&Rs&w:" : "&cObvious exits&w:\n\r" );

    found = FALSE;
    for ( door = 0; door <= 5; door++ )
    {
	if ( ( pexit = ch->in_room->exit[door] ) && pexit->to_room )
	{

         if ( ch->in_room->area->temporal != pexit->to_room->area->temporal )
         {
          if ( !IS_NPC( ch ) )
          {
           if ( !IS_AFFECTED( ch, AFF_TEMPORAL )
             && ch->race != RACE_CHRONOSAPIEN )          
            continue;
          }
          else
           continue;
         }

	    found = TRUE;

	    if ( fAuto )
	    {
               if ( !IS_SET( pexit->exit_info, EX_CLOSED )
                &&  !IS_SET( pexit->exit_info, EX_SECRET ) )
               {
		strcat( buf, "&W " );
		strcat( buf, dir_name[door] );
                secret = TRUE;
               }
               else if ( !IS_SET( pexit->exit_info, EX_SECRET ) )
               {
                if ( str_cmp( pexit->keyword, "" ) )
                {
                 strcat( buf, "&b (&B" );
                 strcat( buf, pexit->keyword );
                 strcat( buf, "&b-&B" );
                }
                else
                 strcat( buf, " &B" );
		strcat( buf, dir_name[door] );
                if ( str_cmp( pexit->keyword, "" ) )
                 strcat( buf, "&b)" );
                secret = TRUE;
               }
               else if ( can_see_thing( ch, THING_HIDDEN ) )
               {
                if ( str_cmp( pexit->keyword, "" ) )
                {
                 strcat( buf, "&r (&R" );
                 strcat( buf, pexit->keyword );
                 strcat( buf, "&r-&R" );
                }
                else
                 strcat( buf, " &R" );
		strcat( buf, dir_name[door] );
                if ( str_cmp( pexit->keyword, "" ) )
                 strcat( buf, "&r)" );
                secret = TRUE;
               }
               else
                 found = FALSE;
	    }
	    else
	    {
               if ( !IS_SET( pexit->exit_info, EX_CLOSED )
                &&  !IS_SET( pexit->exit_info, EX_SECRET ) )
               {
		sprintf( buf + strlen( buf ), "&W%-5s&w - &W%s\n\r",
		    capitalize( dir_name[door] ),
		    room_is_dark( pexit->to_room )
			?  "&zToo dark to tell"
			: pexit->to_room->name
		    );
               }
               else if ( !IS_SET( pexit->exit_info, EX_SECRET ) )
               {
		sprintf( buf + strlen( buf ), "&W%-5s&w - &W%s\n\r",
		    capitalize( dir_name[door] ),
		    room_is_dark( pexit->to_room )
			?  "&zToo dark to tell"
			: pexit->keyword
		    );
               }
               else if ( can_see_thing( ch, THING_HIDDEN ) )
               {
		sprintf( buf + strlen( buf ), "&W%-5s&w - &W%s\n\r",
		    capitalize( dir_name[door] ),
		    room_is_dark( pexit->to_room )
			?  "&zToo dark to tell"
			: pexit->keyword
		    );
               }
	    }
	}
    }

    if ( fAuto )
    {
     for ( obj = ch->in_room->contents; obj; obj = obj->next_content )
     {
      if ( !obj || obj->deleted )
       continue;

      if ( !can_see_obj( ch, obj ) )
       continue;

      if ( obj->item_type == ITEM_PORTAL )
      {
       strcat( buf, "&g (&G" );
       strcat( buf, obj->short_descr );
       strcat( buf, "&g)" );
       secret = TRUE;
      }
     }
    } 

    if ( !found && !secret )
	strcat( buf, fAuto ? "&W none" : "&WNone.\n\r" );

    if ( fAuto )
	strcat( buf, "&z]\n\r" );

    send_to_char(AT_WHITE, buf, ch );
    return;
}

void do_score( CHAR_DATA *ch, char *argument )
{
    char         buf  [ MAX_STRING_LENGTH ];
    char         buf1 [ MAX_STRING_LENGTH ];
    char         arg1 [ MAX_INPUT_LENGTH ];
    char         arg2 [ MAX_INPUT_LENGTH ];

    buf1[0] = '\0';

    if (IS_NPC(ch))
      return;

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( !str_cmp( arg1, "stree" ) )
    {
     sprintf( buf, 
      "&P~~~~~~~~~~~~~~&CSkill Level&z/&GSkill Modifier&P~~~~~~~~~~~~~~~\n\r" );
     send_to_char( AT_WHITE, buf, ch );

     if ( !str_cmp( arg2, "craftsmanship" ) )
      list_skill_tree( ch, ch, 0 );
     else if ( !str_cmp( arg2, "evaluation" ) )
      list_skill_tree( ch, ch, 9 );
     else if ( !str_cmp( arg2, "technology" ) )
      list_skill_tree( ch, ch, 15 );
     else if ( !str_cmp( arg2, "covert" ) )
      list_skill_tree( ch, ch, 25 );
     else if ( !str_cmp( arg2, "physical" ) )
      list_skill_tree( ch, ch, 33 );
     else if ( !str_cmp( arg2, "magic" ) )
      list_skill_tree( ch, ch, 76 );
     else
      list_skill_tree( ch, ch, -1 );
     return;
    }

    if ( !str_cmp( arg1, "lang" ) )
    {
     sprintf( buf, 
      "I~~~~~~~~~~~~~~~~~~Languages Learned~~~~~~~~~~~~~~~~~~~~I\n\r" );
    send_to_char( AT_WHITE, buf, ch );

     sprintf( buf, 
"&W| &cHuman&w:        &R%3d&w  &cElf&w:        &R%3d&w  &cDwarf&w:        &R%3d&w |\n\r"
"&W| &cQuicksilver&w:  &R%3d&w  &cMaudlin&w:    &R%3d&w  &cPixie&w:        &R%3d&w |\n\r"
"&W| &cFelixi&w:       &R%3d&w  &cDraconi&w:    &R%3d&w  &cGremlin&w:      &R%3d&w |\n\r"
"&W| &cCentaur&w:      &R%3d&w  &cKender&w:     &R%3d&w  &cMinotaur&w:     &R%3d&w |\n\r"
"&W| &cDrow&w:         &R%3d&w  &cAquinis&w:    &R%3d&w  &cTroll&w:        &R%3d&w |\n\r",
ch->pcdata->language[LANGUAGE_HUMAN], ch->pcdata->language[LANGUAGE_ELF],
ch->pcdata->language[LANGUAGE_DWARF], ch->pcdata->language[LANGUAGE_QUICKSILVER],
ch->pcdata->language[LANGUAGE_MAUDLIN], ch->pcdata->language[LANGUAGE_PIXIE],
ch->pcdata->language[LANGUAGE_FELIXI], ch->pcdata->language[LANGUAGE_DRACONI],
ch->pcdata->language[LANGUAGE_GREMLIN], ch->pcdata->language[LANGUAGE_CENTAUR],
ch->pcdata->language[LANGUAGE_KENDER], ch->pcdata->language[LANGUAGE_MINOTAUR],
ch->pcdata->language[LANGUAGE_DROW], ch->pcdata->language[LANGUAGE_AQUINIS],
ch->pcdata->language[LANGUAGE_TROLL] );
       send_to_char( AT_WHITE, buf, ch );

sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

    return;
}

    if ( !str_cmp( arg1, "imm" ) )
    {
sprintf( buf, 
"I~~~~~~~~~~~~~Permenant Immunity/Immunity Modifier~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );
    
     sprintf( buf, 
"&W| &cHeat&w:      &R%4d&w/&R%4d  &cPositive&w: &R%4d&w/&R%4d &cCold&w:     &R%4d&w/&R%4d&W|\n\r"
"&W| &cNegative&w:  &R%4d&w/&R%4d  &cHoly&w:     &R%4d&w/&R%4d &cUnholy&w:   &R%4d&w/&R%4d&W|\n\r"
"&W| &cRegen&w:     &R%4d&w/&R%4d  &cDegen&w:    &R%4d&w/&R%4d &cDynamic&w:  &R%4d&w/&R%4d&W|\n\r"
"&W| &cVoid&w:      &R%4d&w/&R%4d  &cPierce&w:   &R%4d&w/&R%4d &cSlash&w:    &R%4d&w/&R%4d&W|\n\r"
"&W| &cScratch&w:   &R%4d&w/&R%4d  &cBash&w:     &R%4d&w/&R%4d &cInternal&w: &R%4d&w/&R%4d&W|\n\r",
ch->pcdata->pimm[0],      ch->pcdata->mimm[0],
ch->pcdata->pimm[1],      ch->pcdata->mimm[1],
ch->pcdata->pimm[2],      ch->pcdata->mimm[2],
ch->pcdata->pimm[3],      ch->pcdata->mimm[3],
ch->pcdata->pimm[4],      ch->pcdata->mimm[4],
ch->pcdata->pimm[5],      ch->pcdata->mimm[5],
ch->pcdata->pimm[6],      ch->pcdata->mimm[6],
ch->pcdata->pimm[7],      ch->pcdata->mimm[7],
ch->pcdata->pimm[8],      ch->pcdata->mimm[8],
ch->pcdata->pimm[9],      ch->pcdata->mimm[9],
ch->pcdata->pimm[10],      ch->pcdata->mimm[10],
ch->pcdata->pimm[11],      ch->pcdata->mimm[11],
ch->pcdata->pimm[12],      ch->pcdata->mimm[12],
ch->pcdata->pimm[13],      ch->pcdata->mimm[13],
ch->pcdata->pimm[14],      ch->pcdata->mimm[14]
);
       send_to_char( AT_WHITE, buf, ch );

sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

    return;
}

    if ( !str_cmp( arg1, "mes" ))
    {
	send_to_char( AT_WHITE, "Your personalized messages are as follows:\n\r", ch );

    if ( get_trust( ch ) >= LEVEL_IMMORTAL)
    {
        sprintf( buf, "Bamfin&r:    &w%s\n\r", ch->pcdata->bamfin );
        send_to_char( AT_GREEN, buf, ch );
        sprintf( buf, "Bamfout&r:   &w%s\n\r", ch->pcdata->bamfout );
        send_to_char( AT_GREEN, buf, ch );
        sprintf( buf, "Bamfusee&r:  &w%s\n\r", ch->pcdata->bamfusee );
        send_to_char( AT_GREEN, buf, ch );
        sprintf( buf, "Restout&r:   &w%s\n\r", ch->pcdata->restout );
        send_to_char( AT_PINK, buf, ch );
        sprintf( buf, "Restusee&r:  &w%s\n\r", ch->pcdata->restusee );
        send_to_char( AT_PINK, buf, ch );
        sprintf( buf, "Transto&r:   &w%s\n\r", ch->pcdata->transto );
        send_to_char( AT_LBLUE, buf, ch );
        sprintf( buf, "Transfrom&r: &w%s\n\r", ch->pcdata->transfrom );
        send_to_char( AT_LBLUE, buf, ch );
        sprintf( buf, "Transvict&r: &w%s\n\r", ch->pcdata->transvict );
        send_to_char( AT_LBLUE, buf, ch );
        sprintf( buf, "Slayusee&r:  &w%s\n\r", ch->pcdata->slayusee );
        send_to_char( AT_RED, buf, ch );
        sprintf( buf, "Slayroom&r:  &w%s\n\r", ch->pcdata->slayroom );
        send_to_char( AT_RED, buf, ch );
        sprintf( buf, "Slayvict&r:  &w%s\n\r", ch->pcdata->slayvict );
        send_to_char( AT_RED, buf, ch );
      }

        sprintf( buf, "Walkin&r:    &w%s from the <dir>.\n\r", ch->pcdata->walkin );
        send_to_char( AT_WHITE, buf, ch );
        sprintf( buf, "Walkout&r:   &w%s to the <dir>.\n\r", ch->pcdata->walkout );
        send_to_char( AT_WHITE, buf, ch );
        sprintf( buf, "Afkmes&r:    &w%s\n\r\n\r", ch->pcdata->afkchar );
        send_to_char( AT_WHITE, buf, ch );

    if ( get_trust( ch ) >= LEVEL_IMMORTAL)
{
      sprintf( buf, "WizInvis level: %3d   WizInvis is %s\n\r",
                      ch->wizinvis,
                      IS_SET( ch->act, PLR_WIZINVIS ) ? "ON " : "OFF" );
      send_to_char( AT_YELLOW, buf, ch );
      sprintf( buf, "Cloaked level:  %3d   Cloaked is  %s\n\r",
                      ch->cloaked,
                      IS_SET( ch->act, PLR_CLOAKED ) ? "ON " : "OFF" );
      send_to_char( AT_YELLOW, buf, ch );

      sprintf( buf, "Avatar level:  %3d\n\r",
                      ch->pcdata->avatar );
      send_to_char( AT_YELLOW, buf, ch );
}

	return;
    }
sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~~~~~Basic Information~~~~~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf,
	    " &cYou are &G%s&c, ",
	    ch->name );
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf, "&Y%s&c &Y%s&c (%d hours)\n\r",
             (get_race_data(ch->race))->race_full, "class here",
	    (get_age( ch ) - 17) * 4 );

    send_to_char( AT_CYAN, buf, ch );

    sprintf( buf, " &cYou are speaking the language of the &C%ss.\n\r",
     flag_string( language_types, ch->pcdata->speaking ) );
     send_to_char( AT_CYAN, buf, ch );
   
    switch ( ch->position )
    {
    case POS_DEAD:     
	send_to_char( (AT_RED + AT_BLINK), " You are DEAD!!!", ch ); break;
    case POS_MORTAL:
	send_to_char( AT_RED, " You are mortally wounded", ch ); break;
    case POS_INCAP:
	send_to_char( AT_RED, " You are incapacitated", ch ); break;
    case POS_STUNNED:
	send_to_char( AT_RED, " You are stunned", ch ); break;
    case POS_SLEEPING:
	send_to_char( AT_LBLUE, " You are sleeping", ch ); break;
    case POS_RESTING:
	send_to_char( AT_LBLUE, " You are resting", ch ); break;
    case POS_STANDING:
	send_to_char( AT_GREEN, " You are standing", ch ); break;
    case POS_FIGHTING:
	send_to_char( AT_BLOOD, " You are fighting", ch ); break;
    }

    if ( ch->resting_on )
    {
     OBJ_DATA *seat;
      seat = ch->resting_on;
     sprintf( buf, " on %s", seat->short_descr );
     send_to_char( AT_LBLUE, buf, ch );
    }

    if ( !IS_NPC( ch ) && ch->pcdata->condition[COND_DRUNK]   > 10 )
	send_to_char( AT_GREY, ", drunk ", ch );
    if ( !IS_NPC( ch ) && ch->pcdata->condition[COND_INSOMNIA] ==  0
	&& ch->pcdata->rank < RANK_STAFF )
	send_to_char( AT_BLUE, ", sleepy ", ch );

    send_to_char( AT_CYAN, "\n\r", ch );

    if ( ch->clan )
    {
        CLAN_DATA *clan;
        
        clan = get_clan_index( ch->clan );

sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~~~~~~Clan Information~~~~~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

        sprintf( buf, " You are %s of the clan %s.\n\r",
        ch->clev == 0 ? "a member" :
        ch->clev == 1 ? "the centurion" :
        ch->clev == 2 ? "the council" :
        ch->clev == 3 ? "the leader" :
        ch->clev == 4 ? "the champion" : "the warden",
        clan->name );
    send_to_char( AT_CYAN, buf, ch );

	if ( ch->ctimer )
        {
	  sprintf( buf + strlen(buf), " Your clan skill timer reads: %d", ch->ctimer );
        send_to_char( AT_WHITE, buf, ch );
        }
        if ( ch->cquestpnts > 0 )
        {
          sprintf( buf, ", you currently have %d Clan Quest Points.\n\r", ch->cquestpnts );
          send_to_char( AT_WHITE, buf, ch );
        }
        else if ( ch->ctimer )
        send_to_char( AT_WHITE, "\n\r", ch );
    }
    
sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~~Character Stats && Status~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );
    
    send_to_char( AT_WHITE, "| &cBLP:       ", ch );
    sprintf ( buf, "%5d/%5d ", ch->hit, MAX_HIT(ch) );
    send_to_char( AT_YELLOW, buf, ch );
     
    sprintf( buf, 
    "Str: &P%2d&p(&P%2d&p)  &cDex: &P%2d&p(&P%2d&p)  &cAgi: &P%2d&p(&P%2d&p) &W|\n\r",
	ch->perm_str,  get_curr_str( ch ),
	ch->perm_dex,  get_curr_dex( ch ),
	ch->perm_agi,  get_curr_agi( ch ) );
    send_to_char( AT_CYAN, buf, ch );

         send_to_char( AT_WHITE, "| &cMana:      ", ch );
         sprintf ( buf, "%5d/%5d ", ch->mana, MAX_MANA(ch) );
         send_to_char( AT_LBLUE, buf, ch );

    sprintf( buf,
"Int: &P%2d&p(&P%2d&p)  &cWis: &P%2d&p(&P%2d&p)  &cCon: &P%2d&p(&P%2d&p) &W|\n\r",
	ch->perm_int,  get_curr_int( ch ),
	ch->perm_wis,  get_curr_wis( ch ),
	ch->perm_con,  get_curr_con( ch ) );
    send_to_char( AT_CYAN, buf, ch );

    send_to_char( AT_WHITE, "| &cStamina:   ", ch );
    sprintf ( buf, "%5d/%5d ", ch->move, MAX_MOVE(ch));
    send_to_char( AT_GREEN, buf, ch );

    sprintf( buf, "Cha: &P%2d&p(&P%2d&p)  &cQuest: &P%3d  ",
        ch->perm_cha, get_curr_cha( ch ),  ch->questpoints );
    send_to_char( AT_CYAN, buf, ch );

    send_to_char( AT_CYAN, "             &W|\n\r", ch );

    sprintf( buf, 
"I~~~~~~~~~~~~~~~~Character && Combat Information~~~~~~~~~~~~~~~~I\n\r" );
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf, "| &cGold:          &Y%7d ",
	    ch->money.gold );
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf, " Magical Damp:  " );
    send_to_char( AT_CYAN, buf, ch );
    sprintf( buf, "%3d                   &W|\n\r", ch->m_damp );
    send_to_char( AT_RED, buf, ch );


    sprintf( buf, "| &cSilver:        &w%7d ",
            ch->money.silver );
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf, " Physical Damp: " );
    send_to_char( AT_CYAN, buf, ch );
    sprintf( buf, "%3d                   &W|\n\r", ch->p_damp );
    send_to_char( AT_RED, buf, ch );

    sprintf( buf, "| &cCopper:        &O%7d ",
            ch->money.copper );
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf, " Wimpy:        &G%4d&c                   &W|\n\r",
	    ch->wimpy );
    send_to_char( AT_CYAN, buf, ch );

    send_to_char( AT_WHITE, "| &cExp:          ", ch );
    sprintf ( buf, "%8d ", ch->exp );
    send_to_char( AT_LBLUE, buf, ch );

    sprintf( buf, " Page pausing:   &P%2d&c                   &W|\n\r",
	ch->pcdata->pagelen );
    send_to_char( AT_CYAN, buf, ch );


    sprintf( buf,
	    "| &cCarry Items: &P%4d&c/&P%4d  &cCarry Weight:    &P%7d&c/&P%7d&ckg   &W|\n\r",
	    ch->carry_number, can_carry_n( ch ),
	    ch->carry_weight, can_carry_w( ch ) );
    send_to_char( AT_WHITE, buf, ch );


sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~Config && Character Status~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

    sprintf( buf,
    " Autoloot:  %-3s Autocoins: %-3s  Autosplit: %-3s  Autosac:  %-3s\n\r",
	    ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_AUTOLOOT ) ) ? "&Ryes&c"
	                                                         : "&Bno &c",
	    ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_AUTOCOINS ) ) ? "&Ryes&c"
	                                                          : "&Bno &c",
	    ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_AUTOSPLIT ) ) ? "&Ryes&c"
	                                                          : "&Bno &c",
	    ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_AUTOSAC  ) ) ? "&Ryes&c"
	                                                  : "&Bno &c" );
    send_to_char( AT_CYAN, buf, ch );    

    sprintf( buf, " Flying:    %-3s Invis:     %-3s  Sneak:     %-3s  Hide: %4s%3s\n\r",
	    ( is_flying(ch) ) ? "&Ryes&c"
	                                   : "&Bno &c",
	    ( IS_SET( ch->affected_by, AFF_INVISIBLE   ) ) ? "&Ryes&c"
	                                   : "&Bno &c",
	    ( IS_SET( ch->affected_by, AFF_SNEAK  ) ) ? "&Ryes&c"
	                                   : "&Bno &c", "",
	    ( IS_SET( ch->affected_by, AFF_HIDE  ) ) ? "&Ryes&c"
	                                   : "&Bno &c" );
    send_to_char( AT_CYAN, buf, ch );    

        if ( ch->pcdata->attitude < ATTITUDE_DEFENSIVE 
		|| ch->pcdata->attitude > ATTITUDE_OFFENSIVE
		|| ch->pcdata->attitude == ATTITUDE_NORMAL )
    sprintf( buf, " Autoexit:  %-3s Attitude:  &G%-9s&c[&G%2d&c] ",
	    ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_AUTOEXIT  ) ) ? "&Ryes&c"
	                                                  : "&Bno &c",         
            flag_string( attitude_flags, ch->pcdata->attitude ), ch->pcdata->attitude );
         else
    sprintf( buf, " Autoexit:  %s Attitude:  &G%9s&c[&G%2d&c]   ",
	    ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_AUTOEXIT  ) ) ? "&Ryes&c"
	                                                  : "&Bno &c",         
            ch->pcdata->attitude > 0 ? "offensive" : "defensive",
          ch->pcdata->attitude );
    send_to_char( AT_CYAN, buf, ch );

    sprintf( buf, "  Size: &P%s&c\n\r",
      flag_string( size_flags, ch->size ) );
    send_to_char( AT_CYAN, buf, ch );    

 
sprintf( buf, 
"I~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~I\n\r"
);
    send_to_char( AT_WHITE, buf, ch );

    return;
}

void do_affectedby( CHAR_DATA *ch, char *argument )
{
    AFFECT_DATA *paf;
    char         buf  [ MAX_STRING_LENGTH ];
    char         buf1 [ MAX_STRING_LENGTH ];
    bool printed = FALSE;

    buf1[0] = '\0';

  if ( IS_NPC( ch ) )
    return;
    if ( !ch->affected && !ch->affected2 ) 
      { send_to_char( AT_CYAN, "You are not affected by anything\n\r", ch); } 
    if ( ch->affected )
    {

	for ( paf = ch->affected; paf; paf = paf->next )
	{
		    if ( paf->deleted )
	        continue;

	    if ( !printed )
	    {
		send_to_char( AT_CYAN, "You are affected by:\n\r", ch );
		printed = TRUE;
	    }

	    sprintf( buf, "&BSpell&W: '&G%s&W'", skill_table[paf->type].name );
            send_to_char( AT_WHITE, buf, ch );
	    if ( /*skill check here*/ )
	    {
		sprintf( buf,
			" &Wmodifies &G%s&W by %d for %d hours",
			affect_loc_name( paf->location ),
			paf->modifier,
			paf->duration );
		send_to_char(AT_WHITE, buf, ch );
	    }

	    send_to_char( AT_WHITE, ".\n\r", ch );
	}
    }
    if ( ch->affected2 )
    {
	for ( paf = ch->affected2; paf; paf = paf->next )
	{
		    if ( paf->deleted )
		        continue;

	    if ( !printed )
	    {
		send_to_char( AT_CYAN, "You are affected by:\n\r", ch );
		printed = TRUE;
	    }

	    sprintf( buf, "&BSpell&W: '&G%s&W'", skill_table[paf->type].name );
            send_to_char( AT_WHITE, buf, ch );
	    if ( /* skill check here */ )
	    {
		sprintf( buf,
			" &Wmodifies &G%s&W by %d for %d hours",
			affect_loc_name( paf->location ),
			paf->modifier,
			paf->duration );
		send_to_char(AT_WHITE, buf, ch );
	    }

	    send_to_char( AT_WHITE, ".\n\r", ch );
	}
    }
    return;
}


char *	const	day_name	[] =
{
    "Stability", "Reformation", "Rebirth", "Ceasar", 
    "Justice",   "Labor",       "Order"
};

char *	const	month_name	[] =
{
    "Death", "Ceasar", "Struggle",  "Peace",   "Futility", "Justice",
    "Heat",  "Battle", "Beginning", "Shadows", "Darkness", "Evil"
};

char *	const	day2_name	[] =
{
    "Veryanil", "Fenral", "Martisel", "Viellal", 
    "Tieriel", "Darial", "Lindical"
};

char *	const	month2_name	[] =
{
 "Phases", "Star Shine", "Crescents",  "Moonbows",   "Shooting Stars", 
 "Secrets", "Thunder",  "Howling Wolf", "Moonlight", "Starflight",
 "Storms", "Eclipse"
};

void do_time( CHAR_DATA *ch, char *argument )
{
           char   buf           [ MAX_STRING_LENGTH ];
    extern char   str_boot_time[];
    extern time_t exe_comp_time;
    extern char * exe_file;
           char  *suf;
           int    day;
    struct stat   statis;

    day     = time_info.day + 1;
         if ( day > 4 && day <  20 ) suf = "th";
    else if ( day % 10 ==  1       ) suf = "st";
    else if ( day % 10 ==  2       ) suf = "nd";
    else if ( day % 10 ==  3       ) suf = "rd";
    else                             suf = "th";

    if ( IS_NPC(ch) )
     return;

    sprintf( buf,
	    "It is %d o'clock %s, Day of %s, %d%s the Month of %s.\n\r",
	    ( time_info.hour % 12 == 0 ) ? 12 : time_info.hour % 12,
	    time_info.hour >= 12 ? "pm" : "am",
	    (ch->pcdata->port == PORT_PAP) ?
            day_name[day % 7] : day2_name[day % 7],
	    day, suf,
	    (ch->pcdata->port == PORT_PAP) ?
            month_name[time_info.month] : month2_name[time_info.month] );
    send_to_char(AT_YELLOW, buf, ch );
    if (ch->pcdata->port == PORT_PAP)
    sprintf( buf,
	    "PaP awoke at %s\rThe system time is %s\r",
	    str_boot_time,
	    (char *) ctime( &current_time ));
    else
    sprintf( buf,
	    "RoI awoke at %s\rThe system time is %s\r",
	    str_boot_time,
	    (char *) ctime( &current_time ));
    send_to_char(AT_RED, buf, ch );
if IS_IMMORTAL( ch )
{

    sprintf( buf, "Running copy compiled at %s\r", (char *) ctime(&exe_comp_time) );
    send_to_char(AT_RED, buf, ch );

    if ( !stat( exe_file, &statis ) )
    {
      sprintf( buf, "PaP compiled at %s\r", (char*)ctime(&statis.st_mtime
) );
      send_to_char( AT_RED, buf, ch );
    }
    else
    {
      if IS_IMMORTAL( ch )
       send_to_char( AT_RED, "No executable avable!!\r\n", ch );
    }
}
    return;
}



void do_weather( CHAR_DATA *ch, char *argument )
{
    char         buf     [ MAX_STRING_LENGTH ];

    sprintf( buf, "It feels to be around %dF ", ch->in_room->temp);
    send_to_char(AT_BLUE, buf, ch );

    sprintf( buf, "and there %s %d inch%s of %s here.\n\r", 
     ch->in_room->accumulation == 1 ? "is" : "are",
     ch->in_room->accumulation == 1,
     ch->in_room->accumulation == 1 ? "" : "es",
     ch->in_room->temp <= 32 ? "snow" : "rain water" );
    send_to_char(AT_BLUE, buf, ch );

    if ( !outdoor_check(ch->in_room) )
     return;

    sprintf( buf, "The weather is %s ",
     flag_string( weather_flags, get_weather(ch->in_room) ) );
    send_to_char(AT_BLUE, buf, ch );

    sprintf( buf, "and the sky appears %s.\n\r",
     flag_string( sky_flags, get_clouds(ch->in_room) ) );
    send_to_char(AT_BLUE, buf, ch );

    sprintf( buf, "The wind is coming from the %s.\n\r",
     flag_string( direction_flags, ch->in_room->area->winddir ) );
    send_to_char(AT_BLUE, buf, ch );

    if ( time_info.phase_white == MOON_FULL )
    sprintf( buf, "The &Wmoon&X is in its full phase this day.\n\r" );
    if ( time_info.phase_white == MOON_FTHIRD
      || time_info.phase_white == MOON_NTHIRD )
    sprintf( buf, "The &Wmoon&X is in its fat crescent phase this day.\n\r" );
    if ( time_info.phase_white == MOON_FCRESCENT
      || time_info.phase_white == MOON_NCRESCENT )
    sprintf( buf, "The &Wmoon&X is in crescent phase this day.\n\r" );
    if ( time_info.phase_white == MOON_FHALF
      || time_info.phase_white == MOON_NHALF )
    sprintf( buf, "The &Wmoon&X is in half phase this day.\n\r" );
    if ( time_info.phase_white == MOON_NEW )
    sprintf( buf, "The &Wmoon&X is in its new phase this day.\n\r" );
    send_to_char(AT_LBLUE, buf, ch );

    if ( time_info.phase_shadow == MOON_FULL )
    sprintf( buf, "The &zmoon&X is in its full phase this day.\n\r" );
    if ( time_info.phase_shadow == MOON_FTHIRD
      || time_info.phase_shadow == MOON_NTHIRD )
    sprintf( buf, "The &zmoon&X is in its fat crescent phase this day.\n\r" );
    if ( time_info.phase_shadow == MOON_FCRESCENT
      || time_info.phase_shadow == MOON_NCRESCENT )
    sprintf( buf, "The &zmoon&X is in crescent phase this day.\n\r" );
    if ( time_info.phase_shadow == MOON_FHALF
      || time_info.phase_shadow == MOON_NHALF )
    sprintf( buf, "The &zmoon&X is in half phase this day.\n\r" );
    if ( time_info.phase_shadow == MOON_NEW )
    sprintf( buf, "The &zmoon&X is in its new phase this day.\n\r" );
    send_to_char(AT_LBLUE, buf, ch );

    if ( time_info.phase_blood == MOON_FULL )
    sprintf( buf, "The &Rmoon&X is in its full phase this day.\n\r" );
    if ( time_info.phase_blood == MOON_FTHIRD
      || time_info.phase_blood == MOON_NTHIRD )
    sprintf( buf, "The &Rmoon&X is in its fat crescent phase this day.\n\r" );
    if ( time_info.phase_blood == MOON_FCRESCENT
      || time_info.phase_blood == MOON_NCRESCENT )
    sprintf( buf, "The &Rmoon&X is in crescent phase this day.\n\r" );
    if ( time_info.phase_blood == MOON_FHALF
      || time_info.phase_blood == MOON_NHALF )
    sprintf( buf, "The &Rmoon&X is in half phase this day.\n\r" );
    if ( time_info.phase_blood == MOON_NEW )
    sprintf( buf, "The &Rmoon&X is in its new phase this day.\n\r" );
    send_to_char(AT_LBLUE, buf, ch );

    return;
}

void do_help( CHAR_DATA *ch, char *argument )
{
    HELP_DATA *pHelp;

    if ( argument[0] == '\0' )
	argument = "summary";

    for ( pHelp = help_first; pHelp; pHelp = pHelp->next )
    {
      if ( !pHelp )
       continue;

	if ( pHelp->level > ch->pcdata->rank )
	    continue;

        if ( !(pHelp->text) )
        {
         send_to_char(AT_WHITE, "No help on that word.\n\r", ch );
         return;
        }

	if ( is_name(ch, argument, pHelp->keyword ) )
	{
	    /*
	     * Strip leading '.' to allow initial blanks.
	     */
	    if ( pHelp->text[0] == '.' )
		send_to_char(AT_GREY, pHelp->text+1, ch );
	    else
		send_to_char(AT_GREY, pHelp->text  , ch );
	    return;
	}
    }

    send_to_char(AT_WHITE, "No help on that word.\n\r", ch );
    return;
}



/*
 * New 'who' command originally by Alander of Rivers of Mud.
 */
void do_who( CHAR_DATA *ch, char *argument )
{
    DESCRIPTOR_DATA *d;
    char             arg1[ MAX_STRING_LENGTH ];
    char             buf      [ MAX_STRING_LENGTH*3 ];
    char             buf2     [ MAX_STRING_LENGTH   ]; 
    char             buf3     [ MAX_STRING_LENGTH   ]; 
    int              nNumber;
    int              nMatch;
    bool             rgfClass [ MAX_CLASS ];
    bool             fImmortalOnly;
    bool         lng = FALSE;
    int              num_of_imm = 0;

    if ( IS_NPC(ch) )
    {
     typo_message(ch);
     return;
    }
    
 
    /*
     * Set default arguments.
     */
    fImmortalOnly  = FALSE;

    /*
     * Parse arguments.
     */
    nNumber = 0;
    for ( ;; )
    {
	char arg [ MAX_STRING_LENGTH ];

	argument = one_argument( argument, arg );
	if ( arg[0] == '\0' )
	    break;
	strcpy( arg1, arg );

      if ( !str_cmp( arg, "wdn" ) || !str_cmp( arg, "imm" ) )
       fImmortalOnly = TRUE;

    /*
     * Now show matching chars.
     */
    nMatch = 0;
    buf[0] = '\0';
    rgfClan[0] = FALSE;
    if ( fNameRestrict )
      send_to_char( C_DEFAULT,
"\n\r\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\"
"/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\n\r", ch );  
    for ( d = descriptor_list; d; d = d->next )
    {
	CHAR_DATA       *wch;
        
	wch   = ( d->original ) ? d->original : d->character;

	/*
	 * Check for match against restrictions.
	 * Don't use trust as that exposes trusted mortals.
	 */
      if ( d->connected != CON_PLAYING || !can_see( ch, wch ) )
       continue;

      if ( ch->pcdata->rank < RANK_PLAYTESTER )
       if ( ch->pcdata->port != wch->pcdata->port )
        continue;

	if ( fImmortalOnly  && wch->pcdata->rank <= RANK_PLAYTESTER )
         continue;

	nMatch++;

        if ( wch->rank > RANK_PLAYTESTER )
          num_of_imm++;

        if ( IS_SET( wch->act, PLR_AFK ) )
           send_to_char( AT_YELLOW, "<AFK> ", ch );
	buf[0] = '\0';
	buf[0] = '\0';

        if ( wch->pcdata->rank <= RANK_PLAYTESTER
         && IS_SET( wch->act, PLR_WIZINVIS )
         && IS_SET( wch->act, PLR_CLOAKED ) )
        {
        sprintf( buf, "%s", "&z(Shrouded) " );
	  send_to_char( AT_WHITE, buf, ch );
        }
        else
        {
	if ( IS_SET( wch->act, PLR_WIZINVIS ) )
	  {
	  sprintf( buf, "%s %d%s", "&w(Wizinvis", wch->wizinvis, ") ");
	  send_to_char( AT_WHITE, buf, ch );
	  }
        if ( IS_SET( wch->act, PLR_CLOAKED ) )
          {
          sprintf( buf, "%s %d%s", "&w(Cloaked", wch->cloaked, ") ");
          send_to_char( AT_WHITE, buf, ch );
          }
        }
	if ( wch->desc && wch->desc->editor != 0 )
	  {
	  if ( wch->desc->editor == 13 ) /* forging eq */
	    send_to_char( AT_DGREY, "&z[&OForging&z] ", ch );
	  else
          {
           if ( str_cmp( ch->oname, ch->name ) )
            ;
	    send_to_char( AT_WHITE, "&R<&CBuilding&R> ", ch );
           }
	  }

	buf[0] = '\0';

	  sprintf( buf + strlen( buf ), "%s%s ", 
	    wch->oname, wch->pcdata->title );
        send_to_char( AT_GREEN, buf, ch);
	buf[0] = '\0';
       sprintf( buf2, "%s", wch->name); 
       sprintf( buf3, "%s", wch->oname); 
       if ( buf2[0] == buf3[0] )
        {
        if (wch->clan != 0)
          sprintf( buf + strlen( buf ), "%s\n\r", clan );
        else sprintf( buf, "\n\r" );
        }
        else sprintf( buf, "\n\r" );
       sprintf( buf2, "%s", wch->name); 
       sprintf( buf3, "%s", wch->oname); 
       if ( buf2[0] == buf3[0] )
        {
        if (wch->clan != 0)
          {
            pClan=get_clan_index(wch->clan);
            if IS_SET(pClan->settings,CLAN_PKILL)
              send_to_char( AT_RED, buf, ch );
	    else
	      send_to_char( AT_LBLUE, buf, ch );
	  }
	else
	  send_to_char(C_DEFAULT, buf, ch );
        }
	else
	  send_to_char(C_DEFAULT, buf, ch );
	buf[0] = '\0';
	if ( fNameRestrict && !str_cmp( arg1, wch->name ) )
	   break;
    }

  if ( nMatch > 0 )
  {
    if ( fNameRestrict )
      send_to_char( C_DEFAULT,
"\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/"
"\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\\/\n\r", ch );

   if ( ch->pcdata->port == PORT_PAP )
   {
    sprintf( buf, "\n\r&RWardens&r[&R%d&r]\n\r",
     num_of_imm );
    send_to_char ( AT_RED, buf, ch );
    
    sprintf( buf, "&G<&YTo see all of the wardens currently in the paradox type who wdn&G>\n\r" ); 
    send_to_char ( AT_BLUE, buf, ch );

    sprintf( buf, "You see %d %s in the paradox.\n\r",
	    nMatch, nMatch == 1 ? "person" : "people" );
    send_to_char(AT_GREEN, buf, ch );  
   }
   else
   {
    sprintf( buf, "\n\r&RImmortals&r[&R%d&r]\n\r",
     num_of_imm );
    send_to_char ( AT_RED, buf, ch );
    
    sprintf( buf, "&G<&YTo see all of the ones most high in the realm type who imm&G>\n\r" ); 
    send_to_char ( AT_BLUE, buf, ch );

    sprintf( buf, "You can sense that %d %s inhabit%s the realm.\n\r",
	    nMatch, nMatch == 1 ? "person" : "people", 
                  nMatch == 1 ? "s" : ""  );
    send_to_char(AT_GREEN, buf, ch );  
   }
  }

  return;
}


void do_inventory( CHAR_DATA *ch, char *argument )
{
    send_to_char(AT_WHITE, "You are carrying:\n\r", ch );
    show_list_to_char( ch->carrying, ch, TRUE, TRUE );
    return;
}


void do_bionic( CHAR_DATA *ch, char *argument )
{
	OBJ_DATA *obj;
	int bionic;
	bool found;

    send_to_char( AT_RED, "Bionics Online:\n\r", ch);

    found = FALSE;
    for ( bionic = 0; bionic < MAX_BIONIC; bionic++ )
    {
	if (!(obj = get_bionic_char( ch, bionic )))
	continue;
  
      send_to_char( AT_BLUE, bionic_where[bionic], ch );
      if ( can_see_obj( ch, obj ))
      {
	send_to_char( AT_WHITE, format_obj_to_char(obj, ch, TRUE), ch );
	send_to_char( C_DEFAULT, "\n\r", ch );
      }else{
      send_to_char( AT_WHITE, "something\n\r", ch );
      }
      found = TRUE;
    }
    if (!found)
	send_to_char(AT_WHITE, "nothing.\n\r", ch);
return;
}

void do_equipment( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    int       iWear;
    int	  iWearFormat = 0;
    bool      found;
    char      buf[ MAX_INPUT_LENGTH ];

    if ( !str_cmp( argument, "tat" ) && !IS_NPC( ch ) )
    {

     TATTOO_DATA *tattoo;

     send_to_char(AT_WHITE, "You have mutilated your body with tattooes of:\n\r", ch );

     found = FALSE;

     for ( iWear = 0; iWear < MAX_TATTOO; iWear++ )
     {
	if ( ( tattoo = get_tattoo_char( ch, iWear ) ) != NULL )
	 {
     send_to_char(AT_LBLUE, tattoo_name[iWear], ch );

     send_to_char(AT_CYAN, format_tattoo_to_char( tattoo, ch ), ch );
     send_to_char(AT_CYAN, "\n\r", ch );
		found = TRUE;
         }
     }
     if ( !found )
	send_to_char(AT_RED, "Nothing.\n\r", ch );

     return;
    }

    send_to_char(AT_WHITE, "You are using:\n\r", ch );
    found = FALSE;
    for ( iWear = 2; iWear != MAX_WEAR; iWear *= 2 )
    {
     if ( !( obj = get_eq_char( ch, iWear ) ) )
     {
      if ( !IS_NPC(ch) ) 
      {
	    if ( ch->pcdata->claws >= 1 && ch->pcdata->claws <= 3
	     && ( (iWear == WEAR_WIELD && !get_eq_char( ch, iWear ))
	     || (iWear == WEAR_WIELD_2 && !get_eq_char( ch, iWear )) ) )
		{
		send_to_char(AT_GREEN, where_name[iWearFormat], ch );
		sprintf( buf, "%s%s%s%s%s%s%s&cYour Claw",
		is_affected( ch, gsn_flamehand ) ? "&r(Burn) " : "",
		is_affected( ch, gsn_chaoshand ) ? "&Y(Chaos) " : "",
		is_affected( ch, gsn_frosthand ) ? "&B(Frost) " : "",
		is_affected( ch, gsn_darkhand ) ? "&z(Dark) " : "",
		is_affected( ch, gsn_colorhand ) ? "&P(&RC&Bo&Gl&Yo&Cr&P) " : "",
                is_affected( ch, gsn_shockhand ) ? "&Y(Electric) " : "",
                is_affected( ch, gsn_quiverpalm ) ? "&W(Q&wu&Wi&wv&We&wr&W) " : "" );  
		send_to_char(AT_CYAN, buf, ch );
		send_to_char(AT_CYAN, "\n\r", ch );
		found = TRUE;
		}
          else
	    if ( ch->pcdata->claws == CLAW_SLASH
	     && ( (iWear == WEAR_WIELD && !get_eq_char( ch, iWear ))
	     || (iWear == WEAR_WIELD_2 && !get_eq_char( ch, iWear )) ) )
		{
		send_to_char(AT_GREEN, where_name[iWearFormat], ch );
		sprintf( buf, "%s%s%s%s%s%s%s&cYour long, thin blade",
		is_affected( ch, gsn_flamehand ) ? "&r(Burn) " : "",
		is_affected( ch, gsn_chaoshand ) ? "&Y(Chaos) " : "",
		is_affected( ch, gsn_frosthand ) ? "&B(Frost) " : "",
		is_affected( ch, gsn_darkhand ) ? "&z(Dark) " : "",
		is_affected( ch, gsn_colorhand ) ? "&P(&RC&Bo&Gl&Yo&Cr&P) " : "",
                is_affected( ch, gsn_shockhand ) ? "&Y(Electric) " : "",
                is_affected( ch, gsn_quiverpalm ) ? "&W(Q&wu&Wi&wv&We&wr&W) " : "" );  
		send_to_char(AT_CYAN, buf, ch );
		send_to_char(AT_CYAN, "\n\r", ch );
		found = TRUE;
		}
          else
	    if ( ch->pcdata->claws == CLAW_CHOP
	     && ( (iWear == WEAR_WIELD && !get_eq_char( ch, iWear ))
	     || (iWear == WEAR_WIELD_2 && !get_eq_char( ch, iWear )) ) )
		{
		send_to_char(AT_GREEN, where_name[iWearFormat], ch );
		sprintf( buf, "%s%s%s%s%s%s%s&cYour axe head",
		is_affected( ch, gsn_flamehand ) ? "&r(Burn) " : "",
		is_affected( ch, gsn_chaoshand ) ? "&Y(Chaos) " : "",
		is_affected( ch, gsn_frosthand ) ? "&B(Frost) " : "",
		is_affected( ch, gsn_darkhand ) ? "&z(Dark) " : "",
		is_affected( ch, gsn_colorhand ) ? "&P(&RC&Bo&Gl&Yo&Cr&P) " : "",
                is_affected( ch, gsn_shockhand ) ? "&Y(Electric) " : "",
                is_affected( ch, gsn_quiverpalm ) ? "&W(Q&wu&Wi&wv&We&wr&W) " : "" );  
		send_to_char(AT_CYAN, buf, ch );
		send_to_char(AT_CYAN, "\n\r", ch );
		found = TRUE;
		}
          else
	    if ( ch->pcdata->claws == CLAW_BASH
	     && ( (iWear == WEAR_WIELD && !get_eq_char( ch, iWear ))
	     || (iWear == WEAR_WIELD_2 && !get_eq_char( ch, iWear )) ) )
		{
		send_to_char(AT_GREEN, where_name[iWearFormat], ch );
		sprintf( buf, "%s%s%s%s%s%s%s&cYour giant hammer",
		is_affected( ch, gsn_flamehand ) ? "&r(Burn) " : "",
		is_affected( ch, gsn_chaoshand ) ? "&Y(Chaos) " : "",
		is_affected( ch, gsn_frosthand ) ? "&B(Frost) " : "",
		is_affected( ch, gsn_darkhand ) ? "&z(Dark) " : "",
		is_affected( ch, gsn_colorhand ) ? "&P(&RC&Bo&Gl&Yo&Cr&P) " : "",
                is_affected( ch, gsn_shockhand ) ? "&Y(Electric) " : "",
                is_affected( ch, gsn_quiverpalm ) ? "&W(Q&wu&Wi&wv&We&wr&W) " : "" );  
		send_to_char(AT_CYAN, buf, ch );
		send_to_char(AT_CYAN, "\n\r", ch );
		found = TRUE;
		}
          else
	    if ( ch->pcdata->claws == CLAW_PIERCE
	     && ( (iWear == WEAR_WIELD && !get_eq_char( ch, iWear ))
	     || (iWear == WEAR_WIELD_2 && !get_eq_char( ch, iWear )) ) )
		{
		send_to_char(AT_GREEN, where_name[iWearFormat], ch );
		sprintf( buf, "%s%s%s%s%s%s%s&cYour long pick",
		is_affected( ch, gsn_flamehand ) ? "&r(Burn) " : "",
		is_affected( ch, gsn_chaoshand ) ? "&Y(Chaos) " : "",
		is_affected( ch, gsn_frosthand ) ? "&B(Frost) " : "",
		is_affected( ch, gsn_darkhand ) ? "&z(Dark) " : "",
		is_affected( ch, gsn_colorhand ) ? "&P(&RC&Bo&Gl&Yo&Cr&P) " : "",
                is_affected( ch, gsn_shockhand ) ? "&Y(Electric) " : "",
                is_affected( ch, gsn_quiverpalm ) ? "&W(Q&wu&Wi&wv&We&wr&W) " : "" );  
		send_to_char(AT_CYAN, buf, ch );
		send_to_char(AT_CYAN, "\n\r", ch );
		found = TRUE;
		}
    	     else
	     if ( (iWear == WEAR_WIELD && !get_eq_char( ch, iWear ))
	     || (iWear == WEAR_WIELD_2 && !get_eq_char( ch, iWear )) ) 
		{
		send_to_char(AT_GREEN, where_name[iWearFormat], ch );
		sprintf( buf, "%s%s%s%s%s%s%s&cYour Fist", 
		is_affected( ch, gsn_flamehand ) ? "&r(Burn) " : "",
		is_affected( ch, gsn_chaoshand ) ? "&Y(Chaos) " : "",
		is_affected( ch, gsn_frosthand ) ? "&B(Frost) " : "",
		is_affected( ch, gsn_darkhand ) ? "&z(Dark) " : "",
		is_affected( ch, gsn_colorhand ) ? "&P(&RC&Bo&Gl&Yo&Cr&P) " : "",
                is_affected( ch, gsn_shockhand ) ? "&Y(Electric) " : "",
                is_affected( ch, gsn_quiverpalm ) ? "&W(Q&wu&Wi&wv&We&wr&W) " : "" );  
		send_to_char(AT_CYAN, buf, ch );
		send_to_char(AT_CYAN, "\n\r", ch );
		found = TRUE;
		}
           }
           else
           {
	     if ( (iWear == WEAR_WIELD && !get_eq_char( ch, iWear ))
	     || (iWear == WEAR_WIELD_2 && !get_eq_char( ch, iWear )) ) 
		{
		send_to_char(AT_GREEN, where_name[iWearFormat], ch );
		sprintf( buf, "&cYour Fist" );
		send_to_char(AT_CYAN, buf, ch );
		send_to_char(AT_CYAN, "\n\r", ch );
		found = TRUE;
		}
           }
          iWearFormat += 1;
	    continue;
	    }
	send_to_char(AT_GREEN, where_name[iWearFormat], ch );
	iWearFormat += 1;
	if ( can_see_obj( ch, obj ) )
	{
	    send_to_char(AT_CYAN, format_obj_to_char( obj, ch, TRUE ), ch );
	    send_to_char(AT_CYAN, "\n\r", ch );
	}
	else
	{
	    send_to_char(AT_RED, "something.\n\r", ch );
	}
	found = TRUE;
    }

    if ( !found )
	send_to_char(AT_RED, "Nothing.\n\r", ch );

    return;
}



void do_compare( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj1;
    OBJ_DATA *obj2;
    char     *msg;
    char      arg1 [ MAX_INPUT_LENGTH ];
    char      arg2 [ MAX_INPUT_LENGTH ];
    int       value1;
    int       value2;

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );
    if ( arg1[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Compare what to what?\n\r", ch );
	return;
    }

    if ( !( obj1 = get_obj_carry( ch, arg1 ) ) )
    {
	send_to_char(C_DEFAULT, "You do not have that item.\n\r", ch );
	return;
    }

    if ( arg2[0] == '\0' )
    {
	for ( obj2 = ch->carrying; obj2; obj2 = obj2->next_content )
	{
	    if ( !IS_SET( obj2->wear_loc, WEAR_NONE )
		&& can_see_obj( ch, obj2 )
		&& obj1->item_type == obj2->item_type
		&& ( obj1->wear_flags & obj2->wear_flags & ~ITEM_TAKE) != 0 )
		break;
	}

	if ( !obj2 )
	{
	    send_to_char( C_DEFAULT, "You aren't wearing anything comparable.\n\r", ch );
	    return;
	}
    }
    else
    {
	if ( !( obj2 = get_obj_carry( ch, arg2 ) ) )
	{
	    send_to_char(C_DEFAULT, "You do not have that item.\n\r", ch );
	    return;
	}
    }
	    
    msg		= NULL;
    value1	= 0;
    value2	= 0;

    if ( obj1 == obj2 )
    {
	msg = "You compare $p to itself.  It looks about the same.";
    }
    else if ( obj1->item_type != obj2->item_type )
    {
	msg = "You can't compare $p and $P.";
    }
    else
    {
	switch ( obj1->item_type )
	{
	default:
	    msg = "You can't compare $p and $P.";
	    break;

	case ITEM_ARMOR:
	    value1 = obj1->value[0];
	    value2 = obj2->value[0];
	    break;

	case ITEM_WEAPON:
	    value1 = obj1->value[1] + obj1->value[2];
	    value2 = obj2->value[1] + obj2->value[2];
	    break;
	}
    }

    if ( !msg )
    {
	     if ( value1 == value2 ) msg = "$p and $P look about the same.";
	else if ( value1  > value2 ) msg = "$p looks better than $P.";
	else                         msg = "$p looks worse than $P.";
    }

    act(C_DEFAULT, msg, ch, obj1, obj2, TO_CHAR );
    return;
}



void do_credits( CHAR_DATA *ch, char *argument )
{
    do_help( ch, "CREDITS" );
    return;
}



void do_where( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA       *victim;
    DESCRIPTOR_DATA *d;
    char             buf [ MAX_STRING_LENGTH ];
    char             arg [ MAX_INPUT_LENGTH  ];
    bool             found;

    if ( !check_blind( ch ) )
        return;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
     AREA_DATA *pArea;
     CLAN_DATA *pClan;
     pArea = ch->in_room->area;

    send_to_char( AT_WHITE,
     "&WYou are in:                                |\n\r", ch );

     pClan = get_clan_index( pArea->owner );
     if ( pArea->ulevel == LEVEL_MORTAL && pArea->llevel == 0 )
     sprintf( buf, "&z<  &R%s  &z> &z{&W  ALL  &z} &C%s\n\r",
	pClan->init, pArea->name );
     else
     sprintf( buf, "&z<  &R%s  &z> &z{&W%3d %3d&z} &C%s\n\r",
      pClan->init, pArea->llevel, pArea->ulevel, pArea->name );
     send_to_char( AT_WHITE, buf, ch );

	send_to_char(AT_WHITE, "People near you:\n\r", ch );
	found = FALSE;
	for ( d = descriptor_list; d; d = d->next )
	{
	    if ( d->connected == CON_PLAYING
		&& ( victim = d->character )
		&& !IS_NPC( victim )
		&& victim->in_room
		&& ( victim->in_room->area == ch->in_room->area
		|| ch->pcdata->rank > RANK_PLAYTESTER )
		&& can_see( ch, victim ) )
	    {
		found = TRUE;
		if ( ch->pcdata->rank > RANK_PLAYTESTER )
	  	   sprintf( buf, "%-18s [%c][%5d] %s\n\r",
			 victim->name, victim->fighting ? 'F' : 
			 (victim->desc && victim->desc->editor != 0) ? 'E' :
                         (victim->desc && victim->desc->pString) ? 'W' : ' ', 
			 victim->in_room->vnum, victim->in_room->name );
		else
			sprintf( buf, "%-28s %s\n\r",
			victim->name, victim->in_room->name );
		send_to_char(AT_PINK, buf, ch );
	    }
	}
	if ( !found )
	    send_to_char(AT_PINK, "None\n\r", ch );
    }
    else
    {
	found = FALSE;
	for ( victim = char_list; victim; victim = victim->next )
	{      	    
	    if (( (!victim->in_room
		|| IS_AFFECTED( victim, AFF_HIDE ) 
		|| IS_AFFECTED( victim, AFF_SNEAK )
		|| victim->race == RACE_KENDER ) &&
                     ch->pcdata->rank < RANK_STAFF ) 
		|| (( ch->pcdata->rank >= RANK_STAFF ) && 
                      (!victim->in_room)) )
	        continue;

	    if (( ( victim->in_room->area == ch->in_room->area) 
	    || ( ch->pcdata->rank >= RANK_STAFF ))
	    && can_see( ch, victim )
	    && is_name(ch, arg, victim->name ) )
	    {
		found = TRUE;
		if ( ch->pcdata->rank >= RANK_STAFF )
			sprintf( buf, "%-18s [%c][%5d] %s\n\r",
			PERS( victim, ch ), victim->fighting ? 'F' : 
			( victim->desc && victim->desc->editor !=0 ) ? 'E' : ' ',
			victim->in_room->vnum, victim->in_room->name );
		else 
			sprintf( buf, "%-28s %s\n\r",
			PERS( victim, ch ), victim->in_room->name );
		send_to_char(AT_PINK, buf, ch );
		break;
	    }
	  
	}
	if ( !found )
	    act(AT_PINK, "You didn't find any $T.", ch, NULL, arg, TO_CHAR );
    }

    return;
}




void do_consider( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char      *msg                      = '\0';
    char      *buf                      = '\0';
    char       arg [ MAX_INPUT_LENGTH ];
    int        diff;
    int        hpdiff;
    int        iWear = 0;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
     send_to_char(C_DEFAULT, "Consider killing whom?\n\r", ch );
     return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
     send_to_char(C_DEFAULT, "They're not here.\n\r", ch );
     return;
    }

    diff = 1;

    if ( IS_AFFECTED( victim, AFF_FIRESHIELD ) )
    diff += 1;
    if ( IS_AFFECTED( victim, AFF_VIBRATING ) )
    diff += 1;
    if ( IS_AFFECTED( victim, AFF_SHOCKSHIELD ) )
    diff += 1;
    if ( IS_AFFECTED( victim, AFF_ICESHIELD ) )
    diff += 1;
    if ( IS_AFFECTED( victim, AFF_CHAOS ) )
    diff += 1;
    if ( IS_AFFECTED2( victim, AFF_THORNY ) )
    diff += 2;
    if ( IS_AFFECTED2( victim, AFF_BLADE ) )
    diff += 1;

    if ( iWear == WEAR_WIELD && get_eq_char( victim, iWear ) )
    diff += 2;
    if ( iWear == WEAR_WIELD_2 && get_eq_char( victim, iWear ) )
    diff += 2;

    diff += (get_curr_str(victim) - get_curr_str(ch));
    diff += (get_curr_dex(victim) - get_curr_dex(ch));
    diff += (get_curr_agi(victim) - get_curr_agi(ch));

    diff += (victim->p_damp - ch->p_damp);
    diff += ((victim->m_damp - ch->m_damp) / 2);

         if ( diff <= -50 ) msg = "$N almost died from your mere gaze!";
    else if ( diff <= -25 ) msg = "$N is a complete wimp."; 
    else if ( diff <= -15 ) msg = "You can kill $N naked and weaponless.";
    else if ( diff <=  -5 ) msg = "$N is no match for you.";
    else if ( diff <=  -2 ) msg = "$N looks like an easy kill.";
    else if ( diff <=   1 ) msg = "The perfect match!";
    else if ( diff <=   4 ) msg = "This might be slightly challenging.";
    else if ( diff <=   9 ) msg = "$N says 'Do you feel lucky, punk?'.";
    else if ( diff <=  12 ) msg = "$N laughs at you mercilessly.";
    else if ( diff <=  25 ) msg = "Oh boy, this is gonna be tough.";
    else if ( diff <=  50 ) msg = "You got to be kidding!";
    else                    msg = "&RDont try it, you WILL die!"; 
    act(C_DEFAULT, msg, ch, NULL, victim, TO_CHAR );

    /* additions by king@tinuviel.cs.wcu.edu */
    hpdiff = ( ch->hit - victim->hit );

    if ( ( ( diff >= 0) && ( hpdiff <= 0 ) )
	|| ( ( diff <= 0 ) && ( hpdiff >= 0 ) ) )
     send_to_char(C_DEFAULT, "Also,", ch );
    else
     send_to_char(C_DEFAULT, "However,", ch );
    
    if ( hpdiff >= 2501 )
        buf = " $E is of very fragile constitution.";
    if ( hpdiff <= 2500 )
        buf = " you are currently much healthier than $E.";
    if ( hpdiff <= 500 )
        buf = " you are currently healthier than $E.";
    if ( hpdiff <= 200 ) 
        buf = " you are currently slightly healthier than $E.";
    if ( hpdiff <= 50 )
        buf = " you are a teensy bit healthier than $E.";
    if ( hpdiff <= 0 )
        buf = " $E is a teensy bit healthier than you.";
    if ( hpdiff <= -50 )
        buf = " $E is slightly healthier than you.";
    if ( hpdiff <= -200 )
        buf = " $E is healthier than you.";
    if ( hpdiff <= -500 )
        buf = " $E is much healthier than you.";
    if ( hpdiff <= -2500 )
        buf = " $E ridicules your hitpoints.";
    if ( hpdiff <= -10000 ) 
        buf = " $E is built like a TANK!.";
             
    act(C_DEFAULT, buf, ch, NULL, victim, TO_CHAR );
    return;
}



void set_title( CHAR_DATA *ch, char *title )
{
    char buf [ MAX_STRING_LENGTH ];

    if ( IS_NPC( ch ) )
    {
	bug( "Set_title: NPC.", 0 );
	return;
    }

    buf[0] = '\0';
    
    if ( !str_cmp( "none", title ) )
    {
       free_string( ch->pcdata->title );
       ch->pcdata->title = str_dup( " " );
       return;
     }

    if ( isalpha( title[0] ) || isdigit( title[0] ) )
    {
	buf[0] = ' ';
	strcpy( buf+1, title );
    }
    else
    {
	strcpy( buf, title );
    }

    free_string( ch->pcdata->title );
    ch->pcdata->title = str_dup( buf );
    return;
}



void do_title( CHAR_DATA *ch, char *argument )
{
    if ( IS_NPC( ch ) )
	return;

    if ( argument[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Change your title to what?\n\r", ch );
	return;
    }

    if ( strlen_wo_col( argument ) > 50 )
	{
	send_to_char( C_DEFAULT, "Max title length is 50 excluding color codes.",
		      ch );
	return;
	}
    smash_tilde( argument );
    set_title( ch, argument );
    send_to_char(C_DEFAULT, "Ok.\n\r", ch );
}



void do_description( CHAR_DATA *ch, char *argument )
{
    string_append( ch, &ch->description );
    return;
}



void do_report( CHAR_DATA *ch, char *argument )
{
    char buf [ MAX_INPUT_LENGTH ];

    if ( !IS_NPC(ch) ) 
        sprintf( buf,
	       "You report: %d/%d hp %d/%d mana %d/%d mv %d xp.\n\r",
	       ch->hit,  MAX_HIT(ch),
	       ch->mana, MAX_MANA(ch),
	       ch->move, MAX_MOVE(ch),
	       ch->exp );
	       
    send_to_char(AT_RED, buf, ch );

    if ( !IS_NPC(ch) ) 
         sprintf( buf,
	            "$n reports: %d/%d hp %d/%d mana %d/%d mv %d xp.",
        	    ch->hit,  MAX_HIT(ch),
        	    ch->mana, MAX_MANA(ch),
         	    ch->move, MAX_MOVE(ch),
        	    ch->exp );
    act(AT_RED, buf, ch, NULL, NULL, TO_ROOM );

    return;
}



void do_practice( CHAR_DATA *ch, char *argument )
{
    char buf  [ MAX_STRING_LENGTH   ];
    char buf1 [ MAX_STRING_LENGTH*2 ];
    int  sn;

    if ( IS_NPC( ch ) )
	return;

    buf1[0] = '\0';

    if ( argument[0] == '\0' )
    {
	CHAR_DATA *mob;
	int        col;

	for ( mob = ch->in_room->people; mob; mob = mob->next_in_room )
	{
	    if ( mob->deleted )
	        continue;
	    if ( IS_NPC( mob ) && IS_SET( mob->act, ACT_PRACTICE ) )
	        break;
	}

	col    = 0;
	for ( sn = 0; skill_table[sn].name[0] != '\0'; sn++ )
	{
	    if ( !skill_table[sn].name )
		break;
	    if ( !can_use_skpell( ch, sn ) )
		continue;

	    if ( ( mob ) || ( ch->pcdata->learned[sn] > 0 ) )
	    {
		sprintf( buf, "&W%21s &Y%3d%%",
			skill_table[sn].name, ch->pcdata->learned[sn] );
		strcat( buf1, buf );
		if ( ++col % 3 == 0 )
		    strcat( buf1, "\n\r" );
		else
		    strcat( buf1, " " );
	    }
	}

	if ( col % 3 != 0 )
	    strcat( buf1, "\n\r" );

	sprintf( buf, "You have %d practice sessions left.\n\r",
		ch->practice );
	strcat( buf1, buf );
	send_to_char(C_DEFAULT, buf1, ch );
    }
    else
    {
	CHAR_DATA *mob;
	int        adept;

	if ( !IS_AWAKE( ch ) )
	{
	    send_to_char(C_DEFAULT, "In your dreams, or what?\n\r", ch );
	    return;
	}

	for ( mob = ch->in_room->people; mob; mob = mob->next_in_room )
	{
	    if ( mob->deleted )
	        continue;
	    if ( IS_NPC( mob ) && IS_SET( mob->act, ACT_PRACTICE ) )
		break;
	}

	if ( !mob )
	{
	    send_to_char(C_DEFAULT, "You can't do that here.\n\r", ch );
	    return;
	}

	if ( ch->practice <= 0 )
	{
	    send_to_char(C_DEFAULT, "You have no practice sessions left.\n\r", ch );
	    return;
	}

	if ( ( sn = skill_lookup( argument ) ) < 0
	    || ( !IS_NPC( ch )
		&& !can_use_skpell( ch, sn ) ) )
	{
	    send_to_char(C_DEFAULT, "You can't practice that.\n\r", ch );
	    return;
	}

/* Practice to approx 50% then use to learn */
        if ( IS_NPC( ch ) || ch->pcdata->learned[sn] > 35 )
        {
            sprintf( buf, "&W%s tells you &c'&CI've already trained you all I can in %s&c'&w.\n\r",
                   capitalize( mob->name ), skill_table[sn].name );
            send_to_char( C_DEFAULT, buf, ch );
            sprintf( buf, "&W%s tells you &c'&CPractice in the real world for more experience&c'&w.\n\r",
                   capitalize( mob->name ) );
            send_to_char( C_DEFAULT, buf, ch );
            return;
        }

	if ( ch->pcdata->learned[sn] >= adept )
	{
	    sprintf( buf, "You are already an adept of %s.\n\r",
		skill_table[sn].name );
	    send_to_char(C_DEFAULT, buf, ch );
	}
	else
	{
	    ch->practice--;
	    ch->pcdata->learned[sn] += int_app[get_curr_int( ch )].learn; 
/*	    ch->pcdata->learned[sn] += ( ( get_curr_int( ch ) * 4 ) / 3 );*/
	    if ( ch->pcdata->learned[sn] < adept )
	    {
		act(C_DEFAULT, "You practice $T.",
		    ch, NULL, skill_table[sn].name, TO_CHAR );
		act(C_DEFAULT, "$n practices $T.",
		    ch, NULL, skill_table[sn].name, TO_ROOM );
	    }
	    else
	    {
		ch->pcdata->learned[sn] = adept;
		act(C_DEFAULT, "You are now an adept of $T.",
		    ch, NULL, skill_table[sn].name, TO_CHAR );
		act(C_DEFAULT, "$n is now an adept of $T.",
		    ch, NULL, skill_table[sn].name, TO_ROOM );
	    }
	}
    }
    return;
}



/*
 * 'Wimpy' originally by Dionysos.
 */
void do_wimpy( CHAR_DATA *ch, char *argument )
{
    char buf [ MAX_STRING_LENGTH ];
    char arg [ MAX_INPUT_LENGTH  ];
    int  wimpy;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
	wimpy = MAX_HIT(ch) / 5;
    else
	wimpy = atoi( arg );

    if ( wimpy < 0 )
    {
	send_to_char(C_DEFAULT, "Your courage exceeds your wisdom.\n\r", ch );
	return;
    }

    if ( wimpy > MAX_HIT(ch) )
    {
	send_to_char(C_DEFAULT, "Such cowardice ill becomes you.\n\r", ch );
	return;
    }

    ch->wimpy	= wimpy;
    sprintf( buf, "Wimpy set to %d hit points.\n\r", wimpy );
    send_to_char(C_DEFAULT, buf, ch );
    return;
}



void do_password( CHAR_DATA *ch, char *argument )
{
    char *pArg;
    char *pwdnew;
    char *p;
    char  arg1 [ MAX_INPUT_LENGTH ];
    char  arg2 [ MAX_INPUT_LENGTH ];
    char  cEnd;

    if ( IS_NPC( ch ) )
	return;

    /*
     * Can't use one_argument here because it smashes case.
     * So we just steal all its code.  Bleagh.
     */
    pArg = arg1;
    while ( isspace( *argument ) )
	argument++;

    cEnd = ' ';
    if ( *argument == '\'' || *argument == '"' )
	cEnd = *argument++;

    while ( *argument != '\0' )
    {
	if ( *argument == cEnd )
	{
	    argument++;
	    break;
	}
	*pArg++ = *argument++;
    }
    *pArg = '\0';

    pArg = arg2;
    while ( isspace( *argument ) )
	argument++;

    cEnd = ' ';
    if ( *argument == '\'' || *argument == '"' )
	cEnd = *argument++;

    while ( *argument != '\0' )
    {
	if ( *argument == cEnd )
	{
	    argument++;
	    break;
	}
	*pArg++ = *argument++;
    }
    *pArg = '\0';

    if ( arg1[0] == '\0' || arg2[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Syntax: password <old> <new>.\n\r", ch );
	return;
    }

    if ( strcmp( crypt( arg1, ch->pcdata->pwd ), ch->pcdata->pwd ) )
    {
	WAIT_STATE( ch, 40 );
	send_to_char(C_DEFAULT, "Wrong password.  Wait 10 seconds.\n\r", ch );
	return;
    }

    if ( strlen( arg2 ) < 5 )
    {
	send_to_char(C_DEFAULT,
	    "New password must be at least five characters long.\n\r", ch );
	return;
    }

    /*
     * No tilde allowed because of player file format.
     */
    pwdnew = crypt( arg2, ch->name );
    for ( p = pwdnew; *p != '\0'; p++ )
    {
	if ( *p == '~' )
	{
	    send_to_char(C_DEFAULT,
		"New password not acceptable, try again.\n\r", ch );
	    return;
	}
    }

    free_string( ch->pcdata->pwd );
    ch->pcdata->pwd = str_dup( pwdnew );
    save_char_obj( ch, FALSE );
    send_to_char(C_DEFAULT, "Ok.\n\r", ch );
    return;
}

void do_socials( CHAR_DATA *ch, char *argument )
{
    char buf  [ MAX_STRING_LENGTH ];
    char buf1 [ MAX_STRING_LENGTH ];
    int  col;
    SOCIAL_DATA * pSocial;

    buf1[0] = '\0';
    col = 0;

    for(pSocial = social_first;pSocial;pSocial = pSocial->next)
    {
	sprintf( buf, "%-12s", pSocial->name );
	strcat( buf1, buf );
	if ( ++col % 6 == 0 )
	    strcat( buf1, "\n\r" );
    }
 
    if ( col % 6 != 0 )
	strcat( buf1, "\n\r" );

    send_to_char(C_DEFAULT, buf1, ch );
    return;
}


/*
 * Contributed by Alander.
 */
void do_commands( CHAR_DATA *ch, char *argument )
{
  char buf[MAX_STRING_LENGTH];
  int  cmd;
  int  col = 0;
  int  trust = get_trust(ch);
 
  for ( cmd = 0; cmd_table[cmd].name[0] != '\0'; cmd++ )
  {
    if ( cmd_table[cmd].level < LEVEL_HERO &&
         str_prefix("mp", cmd_table[cmd].name) &&
         can_use_cmd(cmd, ch, trust) )
    {
      sprintf( buf, "%-16s", cmd_table[cmd].name );
      if ( ++col % 5 == 0 )
        strcat( buf, "\n\r" );
      send_to_char(C_DEFAULT, buf, ch);
    }
  }
 
  if ( col % 5 != 0 )
    send_to_char(C_DEFAULT, "\n\r", ch);

  return;
}



void do_channels( CHAR_DATA *ch, char *argument )
{
    char arg [ MAX_INPUT_LENGTH  ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_SILENCE ) )
	{
	    send_to_char(AT_PURPLE, "You are silenced.\n\r", ch );
	    return;
	}

	send_to_char(AT_PURPLE, "Channels:", ch );

	send_to_char(AT_PINK, !IS_SET( ch->deaf, CHANNEL_AUCTION  )
		     ? " +AUCTION"
		     : " -auction",
		     ch );

	send_to_char(AT_PINK, !IS_SET( ch->deaf, CHANNEL_CHAT     )
		     ? " +CHAT"
		     : " -chat",
		     ch );

        send_to_char(AT_PINK, !IS_SET( ch->deaf, CHANNEL_OOC )
		     ? " +OOC"
		     : " -ooc",
		     ch );

        send_to_char(AT_PINK, !IS_SET( ch->deaf, CHANNEL_CLAN )
                     ? " +CLAN"
                     : " -clan",
                     ch );
	if(IS_HERO(ch)) /* XOR */
	{
	  send_to_char(AT_PINK, !IS_SET(ch->deaf, CHANNEL_HERO)
	   ? " +HERO"
	   : " -hero", ch);
	}

	if ( IS_IMMORTAL( ch ) )
	{
	    send_to_char(AT_PINK, !IS_SET( ch->deaf, CHANNEL_IMMTALK )
			 ? " +IMMTALK"
			 : " -immtalk",
			 ch );
	}

	send_to_char(AT_PINK, !IS_SET( ch->deaf, CHANNEL_MUSIC    )
		     ? " +MUSIC"
		     : " -music",
		     ch );

	send_to_char(AT_PINK, !IS_SET( ch->deaf, CHANNEL_QUESTION )
		     ? " +QUESTION"
		     : " -question",
		     ch );

	send_to_char(AT_PINK, !IS_SET( ch->deaf, CHANNEL_SHOUT    )
		     ? " +SHOUT"
		     : " -shout",
		     ch );

	send_to_char(AT_PINK, !IS_SET( ch->deaf, CHANNEL_YELL     )
		     ? " +YELL"
		     : " -yell",
		     ch );

        send_to_char( AT_LBLUE, !IS_SET(ch->deaf, CHANNEL_INFO)
                        ? " +INFO" : " -info", ch );

        send_to_char( AT_PURPLE, !IS_SET(ch->deaf, CHANNEL_CHALLENGE)
                        ? " +CHALLENGE" : " -challenge", ch );
  
	send_to_char( AT_RED, !IS_SET(ch->deaf, CHANNEL_ARENA)
			? " +ARENA" : " -arena", ch );


	/*
	 * Log Channel Display.
	 * Added by Altrag.
	 */
	if ( get_trust( ch ) >= L_APP )
	{
                send_to_char(AT_DGREY, !IS_SET( ch->deaf, CHANNEL_LOG )
				 ? " +LOG"
				 : " -log",
				 ch );
	}
	
	if ( get_trust( ch ) >= L_SEN )
	{
                send_to_char(AT_DGREY, !IS_SET( ch->deaf, CHANNEL_BUILD )
				 ? " +BUILD"
				 : " -build",
				 ch );
	}
	
	if ( get_trust( ch ) >= L_DIR )
	{
                send_to_char(AT_DGREY, !IS_SET( ch->deaf, CHANNEL_GOD )
				 ? " +GOD"
				 : " -god",
				 ch );
	}
	
	if ( get_trust( ch ) >= L_IMP )
	{
               send_to_char(AT_DGREY, !IS_SET( ch->deaf, CHANNEL_GUARDIAN )
	       			 ? " +GUARD"
	       			 : " -guard",
	       			 ch );
	}

        /* master channels added by Decklarean */
        if ( get_trust( ch ) >= L_IMP )
        {
               send_to_char(AT_DGREY, !IS_SET( ch->deaf, CHANNEL_CLAN_MASTER )
                                 ? " +CLANMASTER"
                                 : " -clanmaster",
                                 ch );
        }
		
	send_to_char(AT_PINK, ".\n\r", ch );
	
    }
    else
    {
	int  bit;
	bool fClear;

	     if ( arg[0] == '+' ) fClear = TRUE;
	else if ( arg[0] == '-' ) fClear = FALSE;
	else
	{
	    send_to_char(AT_PURPLE, "Channels -channel or +channel?\n\r", ch );
	    return;
	}

	     if ( !str_cmp( arg+1, "auction"  ) ) bit = CHANNEL_AUCTION;
	else if ( !str_cmp( arg+1, "ooc"      ) ) bit = CHANNEL_OOC;
        else if ( !str_cmp( arg+1, "chat"     ) ) bit = CHANNEL_CHAT;
	else if ( !str_cmp( arg+1, "hero"     ) ) bit = CHANNEL_HERO;
	else if ( !str_cmp( arg+1, "immtalk"  ) ) bit = CHANNEL_IMMTALK;
	else if ( !str_cmp( arg+1, "music"    ) ) bit = CHANNEL_MUSIC;
	else if ( !str_cmp( arg+1, "question" ) ) bit = CHANNEL_QUESTION;
	else if ( !str_cmp( arg+1, "shout"    ) ) bit = CHANNEL_SHOUT;
	else if ( !str_cmp( arg+1, "yell"     ) ) bit = CHANNEL_YELL;
	else if ( !str_cmp( arg+1, "log"      ) ) bit = CHANNEL_LOG;
	else if ( !str_cmp( arg+1, "build"    ) ) bit = CHANNEL_BUILD;
	else if ( !str_cmp( arg+1, "god"      ) ) bit = CHANNEL_GOD;
	else if ( !str_cmp( arg+1, "guard"    ) ) bit = CHANNEL_GUARDIAN;
	else if ( !str_cmp( arg+1, "info"     ) ) bit = CHANNEL_INFO;
	else if ( !str_cmp( arg+1, "challenge") ) bit = CHANNEL_CHALLENGE;
        else if ( !str_cmp( arg+1, "clan"     ) ) bit = CHANNEL_CLAN;
        else if ( !str_cmp( arg+1,"clanmaster") ) bit = CHANNEL_CLAN_MASTER;
	else if ( !str_cmp( arg+1,"arena"     ) ) bit = CHANNEL_ARENA;
	else if ( !str_cmp( arg+1, "all"      ) ) bit = ~0;
	else
	{
	    send_to_char(AT_PURPLE, "Set or clear which channel?\n\r", ch );
	    return;
	}

	if ( fClear )
	    REMOVE_BIT ( ch->deaf, bit );
	else
	    SET_BIT    ( ch->deaf, bit );

	send_to_char(AT_PURPLE, "Ok.\n\r", ch );
    }

    return;
}



/*
 * Contributed by Grodyn.
 */
void do_config( CHAR_DATA *ch, char *argument )
{
    char arg [ MAX_INPUT_LENGTH ];

    if ( IS_NPC( ch ) )
	return;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
        send_to_char(AT_BLOOD, "&w[&Y Keyword   &w]&W Option\n\r", ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_AUTOEXIT  )
            ? "&w[&Y+AUTOEXIT  &w]&W You automatically see exits.\n\r"
	    : "&w[&Y-autoexit  &w]&W You don't automatically see exits.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_AUTOCOINS  )
	    ? "&w[&Y+AUTOCOINS &w]&W You automatically get coins from corpses.\n\r" 
	    : "&w[&Y-autocoins &w]&W You don't automatically get coins from corpses.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_AUTOSPLIT  )
	    ? "&w[&Y+AUTOSPLIT &w]&W You automatically split coins with group members.\n\r"
	    : "&w[&Y-autosplit &w]&W You don't automatically split coins with group members.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_AUTOLOOT  )
	    ? "&w[&Y+AUTOLOOT  &w]&W You automatically loot corpses.\n\r"
	    : "&w[&Y-autoloot  &w]&W You don't automatically loot corpses.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_AUTOSAC   )
	    ? "&w[&Y+AUTOSAC   &w]&W You automatically sacrifice corpses.\n\r"
	    : "&w[&Y-autosac   &w]&W You don't automatically sacrifice corpses.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_AUTOASSIST   )
	    ? "&w[&Y+AUTOASSIST   &w]&W You automatically group assist.\n\r"
	    : "&w[&Y-autoassist   &w]&W You don't automatically group assist.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_ANSI     )
	    ? "&w[&Y+ANSI      &w]&W You have ansi color enabled.\n\r"
	    : "&w[&Y-ansi      &w]&W You have ansi color disabled.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_BRIEF     )
	    ? "&w[&Y+BRIEF     &w]&W You see brief descriptions.\n\r"
	    : "&w[&Y-brief     &w]&W You see long descriptions.\n\r"
	    , ch );

	send_to_char(AT_RED, IS_SET( ch->act, PLR_FULLNAME  )
	    ? "&w[&Y+FULLNAME  &w]&W You have name completion off.\n\r"
	    : "&w[&Y-fullname  &w]&W You are using name completion.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_COMBINE   )
	    ? "&w[&Y+COMBINE   &w]&W You see object lists in combined format.\n\r"
	    : "&w[&Y-combine   &w]&W You see object lists in single format.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_PROMPT    )
	    ? "&w[&Y+PROMPT    &w]&W You have a prompt.\n\r"
	    : "&w[&Y-prompt    &w]&W You don't have a prompt.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_TELNET_GA )
	    ? "&w[&Y+TELNETGA  &w]&W You receive a telnet GA sequence.\n\r"
	    : "&w[&Y-telnetga  &w]&W You don't receive a telnet GA sequence.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_COMBAT )
	    ? "&w[&Y+COMBAT    &w]&W You see all combat scroll.\n\r"
	    : "&w[&Y-combat    &w]&W You do not see dodge/parry/miss in combat.\n\r"
	    , ch );

	send_to_char(AT_RED,  IS_SET( ch->act, PLR_SILENCE   )
	    ? "&w[&Y+SILENCE   &w]&W You are silenced.\n\r"
	    : ""
	    , ch );

	send_to_char(AT_RED, !IS_SET( ch->act, PLR_NO_EMOTE  )
	    ? ""
	    : "&w[&Y-emote    &w]&W You can't emote.\n\r"
	    , ch );

	send_to_char(AT_RED, !IS_SET( ch->act, PLR_NO_TELL   )
	    ? ""
	    : "&w[&Y-tell     &w]&W You can't use 'tell'.\n\r"
	    , ch );
    }
    else
    {
	char buf [ MAX_STRING_LENGTH ];
	int  bit;
	bool fSet;

	     if ( arg[0] == '+' ) fSet = TRUE;
	else if ( arg[0] == '-' ) fSet = FALSE;
	else
	{
	    send_to_char(AT_BLOOD, "&WConfig &Y-&Woption or &Y+&Woption?\n\r", ch );
	    return;
	}

             if ( !str_cmp( arg+1, "autoexit" ) ) bit = PLR_AUTOEXIT;
	else if ( !str_cmp( arg+1, "autoloot" ) ) bit = PLR_AUTOLOOT;
	else if ( !str_cmp( arg+1, "autocoins") ) bit = PLR_AUTOCOINS;
	else if ( !str_cmp( arg+1, "autosplit") ) bit = PLR_AUTOSPLIT;
	else if ( !str_cmp( arg+1, "autoassist") ) bit = PLR_AUTOASSIST;
	else if ( !str_cmp( arg+1, "autosac"  ) ) bit = PLR_AUTOSAC;
	else if ( !str_cmp( arg+1, "autosac"  ) ) bit = PLR_AUTOCOINS;
	else if ( !str_cmp( arg+1, "brief"    ) ) bit = PLR_BRIEF;
	else if ( !str_cmp( arg+1, "fullname" ) ) bit = PLR_FULLNAME;
	else if ( !str_cmp( arg+1, "combine"  ) ) bit = PLR_COMBINE;
        else if ( !str_cmp( arg+1, "prompt"   ) ) bit = PLR_PROMPT;
	else if ( !str_cmp( arg+1, "telnetga" ) ) bit = PLR_TELNET_GA;
	else if ( !str_cmp( arg+1, "ansi"     ) ) bit = PLR_ANSI;
	else if ( !str_cmp( arg+1, "combat"   ) ) bit = PLR_COMBAT;
	else
	{
	    send_to_char(AT_BLOOD, "Config which option?\n\r", ch );
	    return;
	}

	if ( fSet )
	{
	    SET_BIT    ( ch->act, bit );
	    sprintf( buf, "%s is now &YON&W.\n\r", arg+1 );
	    buf[0] = UPPER( buf[0] );
	    send_to_char(AT_WHITE, buf, ch );
	}
	else
	{
	    REMOVE_BIT ( ch->act, bit );
	    sprintf( buf, "%s is now &YOFF&W.\n\r", arg+1 );
	    buf[0] = UPPER( buf[0] );
	    send_to_char(AT_WHITE, buf, ch );
	}

    }

    return;
}

void do_spells ( CHAR_DATA *ch, char *argument )
{
    char buf  [ MAX_STRING_LENGTH ];
    int  sn;
    int  col;

    if ( IS_NPC( ch ) )
    {  
     typo_message( ch );
    }

    col = 0;
    for ( sn = 0; skill_table[sn].name[0] != '\0'; sn++ )
    {
        if ( !skill_table[sn].name )
	   break;
	if ( !can_use_skpell( ch, sn ) )
	   continue;
	sprintf( buf, "%18s %3dpts ",
		 skill_table[sn].name,
		 ( ch->race == RACE_ELF || ch->race == RACE_AQUINIS )
		 ? (int)(SPELL_COST( ch, sn ) * .75) 
		 : SPELL_COST( ch, sn ) );
	send_to_char( AT_BLUE, buf, ch );
	if ( ++col % 3 == 0 )
	   send_to_char( AT_BLUE, "\n\r", ch );
    }

    if ( col % 3 != 0 )
      send_to_char( AT_BLUE, "\n\r", ch );

    return;

}

void do_slist ( CHAR_DATA *ch, char *argument )
{
    char buf  [ MAX_STRING_LENGTH ];
    char buf1  [ MAX_STRING_LENGTH ];
    int  sn;

	for ( sn = 0; skill_table[sn].name[0] != '\0'; sn++ )
	{
	    if ( !skill_table[sn].name )
		break;

	    if ( ch->pcdata->learned[sn] > 0 )
	    {
		sprintf( buf, "%s&W%21s", skill_table[sn].color_code, 
                  skill_table[sn].name );
		strcat( buf1, buf );
		if ( ++col % 3 == 0 )
		    strcat( buf1, "\n\r" );
		else
		    strcat( buf1, " " );
	    }
	}

 send_to_char( AT_WHITE, buf1, ch );
 returnl
}

/* bypassing the config command - Kahn */
void do_autoexit ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_AUTOEXIT )
     ? sprintf( buf, "-autoexit" )
     : sprintf( buf, "+autoexit" ) );

    do_config( ch, buf );

    return;

}

void do_autoloot ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_AUTOLOOT )
     ? sprintf( buf, "-autoloot" )
     : sprintf( buf, "+autoloot" ) );

    do_config( ch, buf );

    return;
}

void do_autosac ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_AUTOSAC )
     ? sprintf( buf, "-autosac" )
     : sprintf( buf, "+autosac" ) );

    do_config( ch, buf );

    return;

}

void do_brief ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_BRIEF )
     ? sprintf( buf, "-brief" )
     : sprintf( buf, "+brief" ) ) ; 

    do_config( ch, buf );

    return;

}

void do_combine ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_COMBINE )
     ? sprintf( buf, "-combine" )
     : sprintf( buf, "+combine" ) );

    do_config( ch, buf );

    return;

}

void do_autosplit ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_AUTOSPLIT )
     ? sprintf( buf, "-autosplit" )
     : sprintf( buf, "+autosplit" ) );

    do_config( ch, buf );

    return;

}

void do_ansi ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_ANSI )
     ? sprintf( buf, "-ansi" )
     : sprintf( buf, "+ansi" ) );

    do_config( ch, buf );

    return;

}

void do_fullname ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_FULLNAME )
     ? sprintf( buf, "-fullname" )
     : sprintf( buf, "+fullname" ) );

    do_config( ch, buf );

    return;

}

void do_combat ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_COMBAT )
     ? sprintf( buf, "-combat" )
     : sprintf( buf, "+combat" ) );

    do_config( ch, buf );

    return;

}



void do_telnetga ( CHAR_DATA *ch, char *argument )
{
    char buf[ MAX_STRING_LENGTH ];

    ( IS_SET ( ch->act, PLR_TELNET_GA )
     ? sprintf( buf, "-telnetga" )
     : sprintf( buf, "+telnetga" ) );

    do_config( ch, buf );

    return;

}

 
void do_pagelen ( CHAR_DATA *ch, char *argument )
{
    char buf [ MAX_STRING_LENGTH ];
    char arg [ MAX_INPUT_LENGTH  ];
    int  lines;

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
	lines = 20;
    else
	lines = atoi( arg );

    if ( lines < 1 )
    {
	send_to_char(C_DEFAULT,
		"Negative or Zero values for a page pause are not legal.\n\r",
		     ch );
	return;
    }

    if ( lines > 60 )
    {
	send_to_char(C_DEFAULT,
		"I don't know of a screen that is larger than 60 lines!\n\r",
		     ch );
	lines = 60;
    }

    ch->pcdata->pagelen = lines;
    sprintf( buf, "Page pause set to %d lines.\n\r", lines );
    send_to_char(C_DEFAULT, buf, ch );
    return;
}

/* Do_prompt from Morgenes from Aldara Mud */
void do_prompt( CHAR_DATA *ch, char *argument )
{
   char buf [ MAX_STRING_LENGTH ];

   buf[0] = '\0';
   ch = ( ch->desc->original ? ch->desc->original : ch->desc->character );

   if ( argument[0] == '\0' )
   {
       ( IS_SET ( ch->act, PLR_PROMPT )
	? sprintf( buf, "-prompt" )
	: sprintf( buf, "+prompt" ) );

       do_config( ch, buf );

       return;
   }
   
   if( !strcmp( argument, "all" ) )
      strcat( buf, "<%hhp %mm %vmv> ");
   else
   {
      if ( strlen( argument ) > 50 )
	  argument[50] = '\0';
      smash_tilde( argument );
      if ( strlen( argument ) < 2 )
        sprintf( buf, "%s ", argument );
     else
      strcat( buf, argument );
   }

   free_string( ch->prompt );
   ch->prompt = str_dup( buf );
   send_to_char(C_DEFAULT, "Ok.\n\r", ch );
   return;
} 

void do_auto( CHAR_DATA *ch, char *argument )
{

    do_config( ch, "" );
    return;

}

void do_induct( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA  *victim;
    char        arg [MAX_STRING_LENGTH];
    char const *clname;
    char        buf [MAX_STRING_LENGTH];
    CLAN_DATA  *pClan;
    
    buf[0] = '\0';
    one_argument( argument, arg );

   if ( arg[0] == '\0' )
   {
    send_to_char( AT_WHITE, "Syntax: induct <victim>\n\r", ch );     
    return;
   }

    pClan=get_clan_index(ch->clan);

    if ( !pClan )
    {
     send_to_char( AT_WHITE, "You're not even in a clan.\n\r", ch );    
      return;
    }

    if ( ( ch->clan == 0 )
        || ( ch->clev < 1 ) )
           return;
    if (  ((ch->clev == 1) && (!IS_SET(pClan->settings,CLAN_SECOND_INDUCT)))
        ||((ch->clev == 2) && (!IS_SET(pClan->settings,CLAN_FIRST_INDUCT) ))
        ||((ch->clev == 3) && (!IS_SET(pClan->settings,CLAN_LEADER_INDUCT)))
        ||((ch->clev == 4) && (!IS_SET(pClan->settings,CLAN_CHAMP_INDUCT) ))
       )
    {
     send_to_char( AT_WHITE, "You can't induct in your current clan position.", ch);
     return;
    }

    if ( ! ( victim = get_char_room( ch, arg ) ) )    
       {
        send_to_char( AT_WHITE, "No such person is in the room.\n\r", ch );
        return;
       }

    if ( IS_NPC(victim) )
    {
     send_to_char( AT_WHITE, "Not on NPC's.\n\r", ch );
       return;
    }

    if ( IS_SET ( victim->act, PLR_OUTCAST ) )
    {
     send_to_char( AT_WHITE, "That person is an outcast.\n\r", ch );
     return;
    }

    if ( victim->clan != 0 )
    {
     send_to_char( AT_WHITE, "That person is already clanned.\n\r", ch );
       return;
    }

    pClan->members++;
    clname = pClan->name;
    sprintf( buf + strlen( buf ), "<%s>", clname ); 
    act(AT_RED, "$n has been inducted into $T.", victim, NULL, buf, TO_ROOM);
    act(AT_RED, "You are now a member of $T.", victim, NULL, buf, TO_CHAR);
    act(AT_RED, "You have inducted $N.", ch, NULL, victim, TO_CHAR);
    victim->clan = ch->clan;
    victim->clev = 0;
    return;
}

void do_outcast( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA  *victim;
    char        arg [MAX_STRING_LENGTH];
    char const *clname;
    char        buf [MAX_STRING_LENGTH];
    CLAN_DATA  *pClan;
    
    buf[0] = '\0';
    one_argument( argument, arg );
    if ( ( ch->clan == 0 )
        || ( ch->clev < 2 ) )
           return;
    if ( !( victim = get_char_room( ch, arg ) ) )
       {
        send_to_char( AT_WHITE, "No such person is in the room.\n\r", ch );
        return;
       }
    if IS_NPC(victim)
       return;
    if ( ( victim->clan == 0 ) || ( victim->clan != ch->clan ) )
       return;
    pClan=get_clan_index(ch->clan);
    if ( !pClan )
      return;
    pClan->members--;
    clname = pClan->name;
    sprintf ( buf + strlen( buf ), "<%s>", clname );
    act(AT_RED, "$n has been outcasted from $T.", victim, NULL, buf, TO_ROOM);
    act(AT_RED, "You are no longer a member of $T.", victim, NULL, buf, TO_CHAR);
    act(AT_RED, "You have outcasted $N.", ch, NULL, victim, TO_CHAR);
    victim->clan = 0;
    victim->clev = 0;
    if ( victim->cquestpnts > 0 )     /* delete player's Clan Quest points */
       victim->cquestpnts = 0;
    SET_BIT( victim->act, PLR_OUTCAST );
    return;
}

void do_setlev( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg1 [ MAX_INPUT_LENGTH ];
    char       arg2 [ MAX_INPUT_LENGTH ];
    int        level;
    CLAN_DATA *pClan;
    char const *cltitle;

    cltitle = "NONE"; /* init */    

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( ch->clev < 3 )
       return;
    if ( arg1[0] == '\0' || arg2[0] == '\0' || !is_number( arg2 ) )
    {
	send_to_char(AT_WHITE, "Syntax: &Bsetlev <char> <level>.\n\r", ch );
        send_to_char(AT_WHITE, "Valid Levels are as follows\n\r", ch );
        send_to_char(AT_WHITE, "       0 -> Regular member.\n\r", ch );
        send_to_char(AT_WHITE, "       1 -> Centurion of Clan.\n\r", ch );
        send_to_char(AT_WHITE, "       2 -> Council of Clan.\n\r", ch );
        send_to_char(AT_WHITE, "       3 -> Leader of Clan.\n\r", ch );
        send_to_char(AT_WHITE, "       4 -> Clan Champion.\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg1 ) ) )
    {
	send_to_char(AT_WHITE, "That person is not here.\n\r", ch);
	return;
    }

    if ( IS_NPC( victim ) )
    {
	send_to_char(AT_WHITE, "Not on NPC's.\n\r", ch );
	return;
    }
    if ( (ch->clan != victim->clan) || ( ch->clev < victim->clev ) )
       return;
       
    level = atoi( arg2 );
    if ( level > ch->clev )
      {
        send_to_char(AT_WHITE, "Not above your own level.\n\r", ch);
        return;
      }
    if ( level < 0 || level > 4 )
    {
	send_to_char(AT_WHITE, "Valid Levels are as follows\n\r", ch );
	send_to_char(AT_WHITE, "       0 -> Regular member.\n\r", ch );
	send_to_char(AT_WHITE, "       1 -> Centurion in command.\n\r", ch );
	send_to_char(AT_WHITE, "       2 -> Council in command.\n\r", ch );
	send_to_char(AT_WHITE, "       3 -> Leader of Clan.\n\r", ch );
	send_to_char(AT_WHITE, "       4 -> Clan Champion.\n\r", ch );
	return;
    }
    pClan=get_clan_index(ch->clan);
    /* Lower a player in the Clan */
    switch ( victim->clev )
    {
    case 0: break;
    case 1:
      pClan->issecond=FALSE;
      free_string( pClan->second );
      pClan->second = str_dup( "EMPTY" );
      break;
    case 2:
      pClan->isfirst=FALSE;
      free_string( pClan->first );
      pClan->first= str_dup( "EMPTY" );
      break;
    case 3:
      pClan->isleader=FALSE;
      free_string( pClan->leader );
      pClan->leader=str_dup( "EMPTY" );
      break;
    case 4:
      pClan->ischamp=FALSE;
      free_string( pClan->champ );
      pClan->champ = str_dup( "EMPTY" );
      break;
    default: break;
   }
     switch ( level )
     {
      default: break;
      case 0: break;
      case 1:
        if (pClan->issecond)
          {
            send_to_char(AT_WHITE, "Clan already has a Centurion, defaulting to regular member", ch );
            level = 0;
            break;
          }
        else
          {
            pClan->issecond=TRUE;
            pClan->second = str_dup( victim->name );
            break;
          }            
      case 2:
        if (pClan->isfirst)
          {
            send_to_char(AT_WHITE, "Clan already has a Council, defaulting to regular member", ch );
            level = 0;
            break;
          }
        else
          {
            pClan->isfirst=TRUE;
            pClan->first = str_dup( victim->name );
            break;
          }            
      case 3:
        if (pClan->isleader)
          {
            send_to_char(AT_WHITE, "Clan already has a Leader, defaulting to regular member", ch );
            level = 0;
            break;
          }
        else
          {
            pClan->isleader=TRUE;
            pClan->leader = str_dup( victim->name );
            break;
          }            
      case 4:
        if (pClan->ischamp)
          {
            send_to_char(AT_WHITE, "Clan already has a Champion, defaulting to regular member", ch );
            level = 0;
            break;
          }
        else
          {
            pClan->ischamp=TRUE;
            pClan->champ = str_dup( victim->name );
            break;
          }            
     }
    if ( level <= victim->clev )
    {
     char  buf [MAX_STRING_LENGTH];
     buf[0] = '\0';
     switch( level )
     {
      case 0 :  cltitle = "<"; break;
      case 1 :  cltitle = "<Centurion of"; break;
      case 2 :  cltitle = "<Council of"; break;
      case 3 :  cltitle = "<Leader of"; break;
      case 4 :  cltitle = "<Hero of"; break;
      default:  cltitle = "[bug rep to imm]"; break;
     }
     sprintf( buf + strlen(buf), "%s %s>", cltitle, pClan->name );
     act(AT_BLUE, "You have been lowered to $T.", victim, NULL, buf, TO_CHAR );
     act(AT_BLUE, "Lowering a member's clan level.", ch, NULL, NULL, TO_CHAR );
     act(AT_BLUE, "$n is now $T", victim, NULL, buf, TO_ROOM );
     victim->clev = level;
     return;
    }
    else
    {
     char  buf [MAX_STRING_LENGTH];
     
     buf[0] = '\0';
     switch( level )
     {
      case 0 :  cltitle = "<"; break;
      case 1 :  cltitle = "<Centurion of"; break;
      case 2 :  cltitle = "<Council of"; break;
      case 3 :  cltitle = "<Leader of"; break;
      case 4 :  cltitle = "<Hero of"; break;
     }
     sprintf( buf + strlen(buf), "%s %s>", cltitle, pClan->name );
     act(AT_BLUE, "You have been raised to $T.", victim, NULL, buf, TO_CHAR );
     act(AT_BLUE, "Raising a member's clan level.", ch, NULL, NULL, TO_CHAR );
     act(AT_BLUE, "$n is now $T", victim, NULL, buf, TO_ROOM );
     victim->clev = level;
     return;
    }
    return;
}

void do_smash ( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *obj;
    char       buf [MAX_STRING_LENGTH];
    char       arg [MAX_INPUT_LENGTH];
    CHAR_DATA *victim;
    char      *name;
    
    if ( ch->ctimer )
    {
      send_to_char(AT_BLUE, "You failed.\n\r",ch);
      return;
    }
    buf[0]='\0';
    one_argument( argument, arg );
    if ( !(obj = get_obj_carry( ch, arg ) ) )
    {
     send_to_char(AT_WHITE, "You do not have that doll.\n\r", ch );
     return;
    }
    name = obj->name;
    if ( !(victim = get_char_world(ch, name) ) 
	|| victim->in_room->area != ch->in_room->area )
    {
     send_to_char( AT_WHITE, "That person's life cannot be sensed.\n\r", ch );
     return;
    }
/*    if ( /*skills check here*/ )
    {
      send_to_char(AT_BLUE, "The doll remains undamaged.\n\r",ch);
      return;
    }*/
    act(AT_RED, "You call down the Dark forces of Retribution on $N.", ch, NULL, victim, TO_CHAR);
    act( AT_RED, "$n smashes $p.", ch, obj, NULL, TO_ROOM );
    if ( !victim->wait )
      act( AT_RED, "You feel a wave of nausia come over you.", victim, NULL, NULL, TO_CHAR );
    extract_obj(obj);
    ch->ctimer = 5;
    if ( victim->wait )
      return;
    STUN_CHAR(victim, 10, STUN_TOTAL);
    victim->position = POS_STUNNED;
    update_pos( victim, ch );
    return;
}

void do_racelist( CHAR_DATA *ch, char *argument )
{

    RACE_DATA *pRace;
    char buf[MAX_STRING_LENGTH];

if ( ch->pcdata->rank > RANK_PLAYTESTER )
  sprintf( buf,
"&z[&W%20s&z]     [&WStr&z][&WInt&z][&WWis&z][&WDex&z][&WCon&z][&WAgi&z]\n\r"
, "Races" );
else
  sprintf( buf,
"&z[&W%20s&z][&WStr&z][&WInt&z][&WWis&z][&WDex&z][&WCon&z][&WAgi&z]\n\r"
, "Races" );

  send_to_char( C_DEFAULT, buf, ch );

    for ( pRace  = first_race;
          pRace;
          pRace  = pRace->next )
    {
     if ( pRace->polymorph == TRUE && ch->pcdata->rank < RANK_STAFF)
     continue;

 if ( ch->pcdata->rank >= RANK_STAFF )
  sprintf( buf,
"&z[&W%20s&z][&W%3s&z][&W%3d&z][&W%3d&z][&W%3d&z][&W%3d&z][&W%3d&z][&W%3d&z]\n\r",
	         pRace->race_full, pRace->race_name,
                 pRace->mstr, pRace->mint, pRace->mwis, pRace->mdex,
                 pRace->mcon, pRace->magi );
 else
  sprintf( buf,
"&z[&W%20s&z][&W%3d&z][&W%3d&z][&W%3d&z][&W%3d&z][&W%3d&z][&W%3d&z]\n\r",
	         pRace->race_full, 
                 pRace->mstr, pRace->mint, pRace->mwis, pRace->mdex,
                 pRace->mcon, pRace->magi );

  send_to_char( C_DEFAULT, buf, ch );

    }

}


void do_clans( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA    *pClan;
    char          buf[MAX_STRING_LENGTH];
    char result [ MAX_STRING_LENGTH*2 ];	/* May need tweaking. */

    if ( clan_first == NULL )
      return;

    sprintf( result, 
"&z[&W%3s&z] [&W%16s&z] [&W%12s&z] [&W%7s&z] [&W%6s&z] [&W%7s&z] [&W%6s&z]\n\r",
       "Num", "Clan Initials", "Warden", "Members", "Pkills", "Pkilled", "Mkills" );

    for ( pClan = clan_first->next; pClan; pClan = pClan->next )
    {
	sprintf( buf, 
"&z[&W%3d&z] [&W%16s&z] [&W%12s&z] [&W%7d&z] [&r%6d&z] [&W%7d&z] [&R%6d&z]\n\r",
	     pClan->vnum,             
	     pClan->init,
	     pClan->warden,
	     pClan->members,
	     pClan->pkills,
	     pClan->pdeaths,
	     pClan->mkills );
	     strcat( result, buf );
    }

    send_to_char(AT_WHITE, result, ch );
    return;
}

void do_cinfo( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA    *pClan;
    char          buf[MAX_STRING_LENGTH];
    int           num;
    char          arg1[MAX_INPUT_LENGTH];
    OBJ_INDEX_DATA *pObjIndex;
    PLAYERLIST_DATA *pPlayer;

    argument = one_argument(argument, arg1);
    
    if (arg1[0] == '\0')
    {
      send_to_char(AT_WHITE, "Syntax:  &Rcinfo <clan number>\n\r", ch );
      send_to_char(AT_WHITE, "Syntax:  &Rcinfo <clan name>\n\r", ch );
      send_to_char(AT_WHITE, "Use the command clans to find a clan name or number.\n\r", ch );
      return;
    }  

    num = is_number( arg1 ) ? atoi( arg1 ) : -1; 
    
    for ( pClan = clan_first->next; pClan; pClan = pClan->next )
    {
        if ( !str_cmp( arg1, strip_color( pClan->name ) ) )
        {
         num = pClan->vnum;
         break;
        }
    }

    if ( num == -1 )
    {
     send_to_char( AT_WHITE, "No such clan exists, please try again.\n\r", ch );
     send_to_char( AT_WHITE, "Type clans to see available clan names.\n\r", ch );
     return;
    }

    if (!(pClan = get_clan_index(num)))
    {
     send_to_char( AT_WHITE, "Illegal clan number, please try again.\n\r", ch );
     send_to_char( AT_WHITE, "Type clans to see available clan numbers.\n\r", ch );
     return;
    }
    
    sprintf( buf, 
"&z------------------&WInformation on &w<&R%s&w>&z-----------------\n\r\n\r", pClan->name );
    send_to_char(AT_WHITE, buf, ch );
    sprintf( buf, "&cWarden&w:       &z[&W%12s&z]\n\r", pClan->warden );
    send_to_char(AT_WHITE, buf, ch );
    sprintf( buf, "&cChampion&w:    &z[&W%12s&z]&c%s&R%s\n\r",
             pClan->champ,
             IS_SET(pClan->settings, CLAN_LEADER_INDUCT) ? " Can induct." : "",
	     pClan->ischamp ? "" : " Position open." );
    send_to_char( AT_WHITE, buf, ch );
    sprintf( buf, "&cLeader&w:      &z[&W%12s&z]&c%s&R%s\n\r", 
             pClan->leader,
             IS_SET(pClan->settings, CLAN_LEADER_INDUCT) ? " Can induct." : "",
	 pClan->isleader ? "" : " Position open." );
    send_to_char( AT_WHITE, buf, ch );
    sprintf( buf, "&cCouncil&w:     &z[&W%12s&z]&c%s&R%s\n\r", 
             pClan->first,
             IS_SET(pClan->settings, CLAN_LEADER_INDUCT) ? " Can induct." : "",
	     pClan->isfirst ? "" : " Position open." );
    send_to_char( AT_WHITE, buf, ch );
    sprintf( buf, "&cCenturion&w:   &z[&W%12s&z]&c%s&R%s\n\r", 
             pClan->second, 
             IS_SET(pClan->settings, CLAN_LEADER_INDUCT) ? " Can induct." : "",
	     pClan->issecond ? "" : " Position open." );
    send_to_char( AT_WHITE, buf, ch );
    sprintf( buf, "&cMembers&w:     &z[&R%12d&z]\n\r", pClan->members );
    send_to_char(AT_WHITE, buf, ch );
    sprintf( buf, "&cCivil Pkill&w: &z[&r%12s&z]\n\r",
             IS_SET(pClan->settings, CLAN_CIVIL_PKILL) ? "YES" : "NO" );
    send_to_char(AT_WHITE, buf, ch );
    if IS_SET(pClan->settings, CLAN_PKILL)
    {
     sprintf( buf, "&cPkill&w:       &z[         &rYES&z]\n\r" );
     send_to_char(AT_RED, buf, ch );
     sprintf( buf, "&cPkills&w:      &z[&R%12d&z]\n\r", pClan->pkills );
     send_to_char(AT_WHITE, buf, ch );
     sprintf( buf, "&cPkilled&w:     &z[&R%12d&z]\n\r", pClan->pdeaths );
     send_to_char(AT_WHITE, buf, ch );
     sprintf( buf, "&cMkills&w:      &z[&R%12d&z]\n\r", pClan->mkills );
     send_to_char(AT_WHITE, buf, ch );
     sprintf( buf, "&cEquipment&w:\n\r" );
     send_to_char(AT_WHITE, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_1 )))
      sprintf( buf, "&cLevel &W50&w: &W%s\n\r", pObjIndex->short_descr ); 
     else
      sprintf( buf, "&cLevel &W50&w:\n\r" ); 
     send_to_char(AT_WHITE, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_2 )))
      sprintf( buf, "&cLevel &W75&w: &W%s\n\r", pObjIndex->short_descr );
     else
      sprintf( buf, "&cLevel &W75&w:\n\r" );
     send_to_char(AT_WHITE, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_3 )))
      sprintf( buf, "&cLevel &W100&w: &W%s\n\r", pObjIndex->short_descr );
     else
      sprintf( buf, "&cLevel &W100&w:\n\r" );
     send_to_char(AT_WHITE, buf, ch );
    }
    else
    {
     sprintf( buf, "&cPkill&w:       &z[          &CNO&z]\n\r" );
     send_to_char( AT_LBLUE, buf, ch );
     sprintf( buf, "&cMkills&w:      &z[&R%12d&z]\n\r", pClan->mkills );
     send_to_char(AT_WHITE, buf, ch );
     sprintf( buf, "&cEquipment&w:\n\r" );
     send_to_char(AT_WHITE, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_1 )))
      sprintf( buf, "&cLevel &W30&w: &W%s\n\r", pObjIndex->short_descr );
     else
      sprintf( buf, "&cLevel &W30&w:\n\r" );
     send_to_char(AT_WHITE, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_2 )))
      sprintf( buf, "&cLevel &W65&w: &W%s\n\r", pObjIndex->short_descr );
     else
      sprintf( buf, "&cLevel &W65&w:\n\r" ); 
     send_to_char(AT_WHITE, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_3 )))
      sprintf( buf, "&cLevel &W100&w: &W%s\n\r", pObjIndex->short_descr );
     else
      sprintf( buf, "&cLevel &W100&w:\n\r" );
     send_to_char(AT_WHITE, buf, ch );
    }   
    sprintf( buf, "&cDescription&w:\n\r&W%s", pClan->description );
    send_to_char( AT_WHITE, buf, ch );

    send_to_char( C_DEFAULT, "&cCurrent Members:\n\r", ch );

    for ( pPlayer = playerlist; pPlayer; pPlayer = pPlayer->next )
    {
       if ( pPlayer->clan_name )
       if ( !str_cmp(pPlayer->clan_name, pClan->name )  )
       {
          sprintf( buf, "%s\n\r", pPlayer->name );
          send_to_char( AT_WHITE, buf, ch );
       }
    }

    return;
}  
    
void do_autocoins( CHAR_DATA *ch, char *argument )
{
   if ( IS_NPC( ch ) )
   	return;

   if ( IS_SET( ch->act, PLR_AUTOCOINS ) )
   { 
     do_config( ch, "-autocoins" );
     return;
   }
   if ( !IS_SET( ch->act, PLR_AUTOCOINS ) )
   {
     do_config( ch, "+autocoins" );
     return;
   }
   return;
}

void do_autoassist( CHAR_DATA *ch, char *argument )
{
   if ( IS_NPC( ch ) )
   	return;

   if ( IS_SET( ch->act, PLR_AUTOASSIST ) )
   { 
     do_config( ch, "-autoassist" );
     return;
   }
   if ( !IS_SET( ch->act, PLR_AUTOASSIST ) )
   {
     do_config( ch, "+autoassist" );
     return;
   }
   return;
}


void do_farsight( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA       *victim;
    char             target_name[MAX_STRING_LENGTH];
    ROOM_INDEX_DATA *from_room;
    
    if (IS_NPC(ch))
      return;
    if (ch->clan != 7 )
    {
      typo_message( ch );
      return;
    }
    one_argument( argument, target_name );
    
    if ( !( victim = get_char_world( ch, target_name ) )
	|| IS_SET( victim->in_room->room_flags, ROOM_PRIVATE   )
	|| IS_SET( victim->in_room->room_flags, ROOM_SOLITARY  )
	|| IS_SET( victim->in_room->area->area_flags, AREA_PROTOTYPE )
	|| IS_AFFECTED( victim, AFF_NOASTRAL ) )
    {
	send_to_char(AT_BLUE, "You failed.\n\r", ch );
	return;
    }
    from_room = ch->in_room;
    if ( ch != victim )
    {
     char_from_room( ch );
     char_to_room( ch, victim->in_room );
    }
    do_look( ch, "auto" );
    if ( ch != victim )
    {
     char_from_room( ch );
     char_to_room( ch, from_room );
    }
    return;
}

/*
 * Finger routine written by Dman.
 */
void do_finger( CHAR_DATA *ch, char *argument )
{
  char buf[MAX_STRING_LENGTH];
  char arg[MAX_INPUT_LENGTH];
  CHAR_DATA *victim;
  char      const *race;
  
  one_argument( argument, arg );
  
  if ( arg[0] == '\0' )
  {
   send_to_char( AT_WHITE, "Finger whom?\n\r", ch );
   return;
  }
              
  if ( ( victim = get_char_world( ch, arg ) ) == NULL )
  {
   read_finger( ch, argument );
   return;
  }
                                  
  if ( !can_see( ch, victim ) )
  {
   send_to_char( AT_WHITE, "They aren't here.\n\r", ch );
   return;
  }
    
  if  ( IS_NPC( victim ) )
  {
   send_to_char( AT_WHITE, "Not on NPC's.\n\r", ch );
   return;
  } 

  sprintf( buf, "          Finger Info\n\r" );
  send_to_char( AT_WHITE, buf, ch );
                                           
  sprintf( buf, "          ------ ----\n\r\n\r" );
  send_to_char( AT_WHITE, buf, ch );
  
  sprintf( buf, "&CName: &W%-12s\n\r", victim->name);
  send_to_char( AT_WHITE, buf, ch );

  sprintf( buf, "&CMud Age: &W%2d           &CClan: &W%s\n\r",
     get_age( victim ), get_clan_index( victim->clan )->name );
  send_to_char( AT_WHITE, buf, ch );
 
  sprintf( buf, "&CSex:  &W%s\n\r",
                victim->sex == SEX_MALE   ? "male"   :
                victim->sex == SEX_FEMALE ? "female" : "neutral" );
  send_to_char( AT_WHITE, buf, ch );              
                     
  race = (get_race_data(victim->race))->race_name;
  sprintf( buf, "&CRace: &W%s\n\r",race );
  send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&CTitle: &W%s\n\r", 
                victim->pcdata->title );
  send_to_char( AT_WHITE, buf, ch );
                             
  sprintf( buf, "&CEmail: &W%s\n\r", victim->pcdata->email );
  send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&CPlan: &W%s.\n\r", victim->pcdata->plan );
  send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&CLast on: &W%s\n\r", (char *) ctime( &ch->logon ) );
  send_to_char( AT_WHITE, buf, ch );
  return;
}
      

void do_email( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *rch;
    char         buf  [ MAX_STRING_LENGTH ];

    rch = get_char( ch );


    if ( IS_NPC( ch ) )
     return;
     
    if (argument[0] == '\0')
    {
     sprintf(buf,"Email:%s \n\r", ch->pcdata->email);
     send_to_char( AT_WHITE, buf, ch );
     return;
    }
    
    if ( !IS_NPC( ch ) )
    {
        if ( longstring( ch, argument ) )
	    return;

	smash_tilde( argument );
	free_string( ch->pcdata->email );
	ch->pcdata->email = str_dup( argument );
        sprintf(buf,"Email now:%s \n\r",argument);
        send_to_char(AT_WHITE, buf, ch);
    }
    return;
}

void do_plan( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *rch;
    char         buf  [ MAX_STRING_LENGTH ];

    rch = get_char( ch );


    if ( IS_NPC( ch ) )
     return;
     
    if (argument[0] == '\0')
    {
     sprintf(buf,"Plan:%s \n\r", ch->pcdata->plan);
     send_to_char(AT_WHITE, buf, ch);
     return;
    }
    
    if ( !IS_NPC( ch ) )
    {
        if ( longstring( ch, argument ) )
	    return;

	smash_tilde( argument );
	free_string( ch->pcdata->plan );
	ch->pcdata->plan = str_dup( argument );
        sprintf(buf,"Plan now:%s \n\r",argument);
        send_to_char(AT_WHITE, buf, ch);
    }
    return;
}

void do_walkmes(CHAR_DATA *ch, char *argument )
{
   char arg[MAX_INPUT_LENGTH];
   
   if ( IS_NPC( ch ) )
   return;

   argument = one_argument(argument,arg);
      
   if (arg[0] == '\0')
     {
        send_to_char(AT_WHITE, "Syntax:\n\r",ch);
        send_to_char(AT_RED,   
"  walkmes out  <string>  (What everybody in the room you came from see.)\n\r",ch);
        send_to_char(AT_RED,   "  walkmes in   <string>  (What everybody in the room you goto see.)\n\r",ch);
        return;
     }

   if ( !str_cmp( arg,"out" ) )
     {
       char  arg [ MAX_INPUT_LENGTH ];
       char  buf [ MAX_STRING_LENGTH ];
    
       one_argument( argument, arg );
    
       if ( arg[0] == '\0' )
         {
	   sprintf( buf, "Walkout: %s\n\r", ch->pcdata->walkout );
	   send_to_char(AT_WHITE, buf, ch );
	   return;
         }

       if ( !IS_NPC( ch ) )
         {
           if ( longstring( ch, argument ) )
	     return;

	   smash_tilde( argument );
	   free_string( ch->pcdata->walkout );
	   ch->pcdata->walkout = str_dup( argument );
	   send_to_char(AT_WHITE, "Walkout set.\n\r", ch );
         }
       return;

     }

   if ( !str_cmp( arg, "in" ) )
     {
       char  arg [ MAX_INPUT_LENGTH ];
       char  buf [ MAX_STRING_LENGTH ];
    
       one_argument( argument, arg );
    
       if ( arg[0] == '\0' )
         {
	   sprintf( buf, "Walkin: %s\n\r", ch->pcdata->walkin );
	   send_to_char(AT_WHITE, buf, ch );
	   return;
         }

       if ( !IS_NPC( ch ) )
        {
          if ( longstring( ch, argument ) )
	    return;

	  smash_tilde( argument );
	  free_string( ch->pcdata->walkin );
	  ch->pcdata->walkin = str_dup( argument );
	  send_to_char(AT_WHITE, "Walkin set.\n\r", ch );
        }
      return;

    }


}


void do_afkmes( CHAR_DATA *ch, char *argument )
{
   char buf [ MAX_STRING_LENGTH ];

   if ( !IS_NPC( ch ) )
   {
      if ( argument[0] == '\0' )
      {
        send_to_char( AT_WHITE, "Syntax:\n\r", ch );
        send_to_char( AT_YELLOW, "afkmes <string>\n\r", ch );
        sprintf( buf, "&YAFK Message: &W%s\n\r", ch->pcdata->afkchar );
        send_to_char( AT_WHITE, buf, ch );
        return;
      }

      if ( longstring( ch, argument ) )
        return;
    
      smash_tilde( argument );
      free_string( ch->pcdata->afkchar );
      ch->pcdata->afkchar = str_dup( argument );
      sprintf( buf, "&YAFK Message: &W%s\n\r", argument );
      send_to_char( AT_WHITE, buf, ch );
   } 
   return;
} 


/* Money functions, for new money format gold/silver/copper --Angi */
MONEY_DATA *add_money( MONEY_DATA *a,  MONEY_DATA *b )
{
  a->gold   += b->gold;
  a->silver += b->silver;
  a->copper += b->copper;

  return a;
}

MONEY_DATA *sub_money( MONEY_DATA *a, MONEY_DATA *b )
{
  a->gold   -= b->gold;
  a->silver -= b->silver;
  a->copper -= b->copper;

  return a;
}
MONEY_DATA *spend_money( MONEY_DATA *a, MONEY_DATA *b )
{

/* *a is how much money ch has, and *b is the cost of the item.
   Char can already afford, check done before, so just subtract. */

  int tmp_gold, tmp_silver, tmp_copper;

  tmp_gold = b->gold;
  tmp_silver = b->silver;
  tmp_copper = b->copper;

  if ( a->gold <= b->gold )
  {
    b->gold -= a->gold;
    a->gold = 0;
  }
  else
  {
    a->gold -= b->gold;
    b->gold = 0;
  }
  
  if ( a->silver <= b->silver )
  {
    b->silver -= a->silver;
    a->silver = 0;
  }
  else
  {
    a->silver -= b->silver;
    b->silver = 0;
  }

  if ( a->copper <= b->copper )
  {
    b->copper -= a->copper;
    a->copper = 0;
  }
  else
  {
    a->copper -= b->copper;
    b->copper = 0;
  }

  if ( b->gold > 0 )
  {
      if ( a->silver * S_PER_G <= b->gold * C_PER_G )
    {
      b->gold -= ( a->silver / SILVER_PER_GOLD );
      a->silver %= SILVER_PER_GOLD;
    }
    else
    {
      a->silver -= ( b->gold * SILVER_PER_GOLD );
      b->gold = 0;
    }	
    if ( b->gold > 0 )
    {
        if ( a->copper <= b->gold * C_PER_G )
      {
	b->gold -= ( a->copper / COPPER_PER_GOLD );
	a->copper %= COPPER_PER_GOLD;
      }
      else
      {
        a->copper -= ( b->gold * COPPER_PER_GOLD );
	b->gold =0;
      }
    }
  }
/* if b->gold != 0 now, then bug( etc.. ) */

  if ( b->silver > 0 )
  {
    if ( ( a->gold * SILVER_PER_GOLD ) <= b->silver )
    {
      b->silver -= ( a->gold * SILVER_PER_GOLD );
      a->gold = 0;
    }
    else
    {
      a->gold -= ( b->silver / SILVER_PER_GOLD ); 
      b->silver %= SILVER_PER_GOLD;
    }
    if ( b->silver > 0 )
    {
        if ( a->copper <= b->silver * S_PER_G )
      {
        b->silver -= ( a->copper / COPPER_PER_SILVER );
        a->copper %= COPPER_PER_SILVER;
      }
      else
      {
        a->copper -= ( b->silver * COPPER_PER_SILVER );
        b->silver =0;
      }
    }
  }

  if ( b->copper > 0 )
  {
    if ( ( a->silver * COPPER_PER_SILVER ) <= b->copper )
    {
      b->copper -= ( a->silver * COPPER_PER_SILVER );
      a->silver = 0;
    }
    else
    {
      a->silver -= ( b->copper / COPPER_PER_SILVER );
      b->copper %= COPPER_PER_SILVER;
    }
    if ( b->copper > 0 )
    {
      if ( ( a->gold * COPPER_PER_GOLD ) <= b->copper )
      {
        b->copper -= ( a->gold * COPPER_PER_GOLD );
        a->gold = 0;
      }
      else
      {
        a->gold -= ( b->copper / COPPER_PER_GOLD );
        b->copper %= COPPER_PER_GOLD;
      }
    }
  }

  b->gold = tmp_gold;
  b->silver = tmp_silver;
  b->copper = tmp_copper;

  return a;

}

MONEY_DATA *take_money( CHAR_DATA *ch, int amt, char *type, char *verb )
{
  static MONEY_DATA  new_money;

  new_money.gold = new_money.silver = new_money.copper = 0;

  if ( amt <= 0 )
  {
    send_to_char( AT_WHITE, "Sorry, you can't do that.\n\r", ch );
    return NULL;
  }
  
  if ( !str_cmp( type, "gold" ) )
  {
    if ( ch->money.gold < amt )  
       return NULL;
    else
       new_money.gold = amt;
  }
  else
  if ( !str_cmp( type, "silver" ) )
  {
    if ( ch->money.silver < amt )
       return NULL;
    else
       new_money.silver = amt;
  }
  else
  if ( !str_cmp( type, "copper" ) )
  {
    if ( ch->money.copper < amt )
       return NULL;
    else
       new_money.copper = amt;
  }
  else
  {
    send_to_char( AT_WHITE, "There is no such kind of coin.\n\r", ch );
    return NULL;
  }
  
  sub_money( &ch->money, &new_money );

  return &new_money;
}

char *money_string( MONEY_DATA *money )
{
static  char buf  [ MAX_STRING_LENGTH ];
  char *bptr = buf;
  bool gold;
  bool silver;
  bool copper;

  gold   = ( money->gold   > 0 );
  silver = ( money->silver > 0 );
  copper = ( money->copper > 0 );

  if ( gold )
  {
    bptr += sprintf( bptr, "%d gold", money->gold );
    if ( silver != copper )
    {
      strcpy( bptr, " and " );
      bptr += 5;
    }
    else if ( silver || copper )
    {
      strcpy ( bptr, ", " );
      bptr += 2;
    }
    else
    {
      strcpy( bptr, " coins." );
      bptr += 7;
    }
  }
  
  if ( silver )
  {
    bptr += sprintf( bptr, "%d silver", money->silver );
    if ( copper )
    {
      strcpy( bptr, " and " );
      bptr += 5;
    }
    else
    {
      strcpy( bptr, " coins." );
      bptr += 7;
    }
  }
	
  if ( copper )
    bptr += sprintf( bptr, "%d copper coins.", money->copper );

  if ( !gold && !silver && !copper )
  {
    bptr += sprintf( bptr, "0 coins." );
  }
 
  return buf; 
}

void do_scrytraps( CHAR_DATA *ch, char *argument )
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
  EXIT_DATA *pexit;
  ROOM_INDEX_DATA *in_room;
  ROOM_INDEX_DATA *next_room;
  char buf [ MAX_INPUT_LENGTH * 4 ];
  int dir, dis;
  bool found = FALSE;
  bool repeat = FALSE;
  const char *dir_message;
  const char *dis_message;

  if ( IS_NPC( ch ) )
	return;

  if ( !can_use_skpell( ch, gsn_scrytraps ) )
	{
	send_to_char( C_DEFAULT, "You wouldn't know what to look for.\n\r", 
			ch );
	return;
	}

  if ( ch->pcdata->learned[gsn_scrytraps] < number_percent( ) )
	{
	send_to_char( C_DEFAULT, "You can't detect any traps.\n\r",
			ch );
	return;
	}

  in_room = ch->in_room;
  for ( dir = 0; dir <= 5; dir++ )
  {
    if ( !( pexit = in_room->exit[dir] ) 
    || !( next_room = pexit->to_room ) )
        continue;

    for ( dis = 0; dis <= 1; dis++ )
    {
     dir_message = dir_table[dir];
     dis_message = dis_table[dis];

     if ( IS_SET( next_room->room_flags, ROOM_TRAPPED ) )
     {
      found = TRUE;
      if ( repeat == FALSE )
       send_to_char( C_DEFAULT, "You see a trap:\n\r", ch );

      sprintf( buf, "&w%s %s.\n\r", dis_message, dir_message );
      send_to_char( C_DEFAULT, buf, ch );
        repeat = TRUE;
     }

     if ( !( pexit = next_room->exit[dir] ) 
       || !( next_room = pexit->to_room ) )
	     break;
   }
  }
  if ( found == FALSE )
   send_to_char( C_DEFAULT, "You could not detect any traps.\n\r", ch );


  update_skpell( ch, gsn_scrytraps );

  return;

}
