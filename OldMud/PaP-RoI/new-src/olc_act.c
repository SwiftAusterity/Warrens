/***************************************************************************
 *  File: olc_act.c                                                        *
 *                                                                         *
 *  Much time and thought has gone into this software and you are          *
 *  benefitting.  We hope that you share your changes too.  What goes      *
 *  around, comes around.                                                  *
 *                                                                         *
 *  This code was freely distributed with the The Isles 1.1 source code,   *
 *  and has been used here for OLC - OLC would not be what it is without   *
 *  all the previous coders who released their source code.                *
 *                                                                         *
 ***************************************************************************/

#define linux 1
#if defined(macintosh)
#include <types.h>
#else
#include <sys/types.h>
#endif
#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <limits.h>	/* OLC 1.1b */
#include <sys/stat.h>
#include "merc.h"
#include "olc.h"

extern  int               num_mob_progs;
extern  int               num_trap_progs;

/*
 * External functions.
 */
void 		clan_sort	args( ( CLAN_DATA *pClan ) );
char 	*mprog_type_to_name   	args( ( int type ) );
HELP_DATA 	*get_help      	args( ( char *argument ) );
HELP_DATA 	*get_quest     	args( ( char *argument ) );
SOCIAL_DATA	*get_social	args( ( char *argument ) );

struct olc_help_type
{
    char *command;
    const void *structure;
    char *desc;
};

bool show_version( CHAR_DATA *ch, char *argument )
{
    editor_send_to_char(C_DEFAULT, VERSION, ch );
    editor_send_to_char(C_DEFAULT, "\n\r", ch );
    editor_send_to_char(C_DEFAULT, AUTHOR, ch );
    editor_send_to_char(C_DEFAULT, "\n\r", ch );
    editor_send_to_char(C_DEFAULT, DATE, ch );
    editor_send_to_char(C_DEFAULT, "\n\r", ch );
    editor_send_to_char(C_DEFAULT, CREDITS, ch );
    editor_send_to_char(C_DEFAULT, "\n\r", ch );

    return FALSE;
}    

/*
 * This table contains help commands and a brief description of each.
 * ------------------------------------------------------------------
 */
const struct olc_help_type help_table[] =
{
    {	"area",		area_flags,	 	"Area attributes."	},
    {	"temporal",	temporal_flags,		"Area temporal flags."	},
    {	"weather",	area_weather_flags,	"Area weather flags."	},
    {	"room",		room_flags,	 	"Room attributes."	},
    {	"sector",	sector_flags,		"Sector types, terrain."},
    {	"exit",		exit_flags,	 	"Exit types."		},
    {	"type",		type_flags,	 	"Types of objects."	},
    {	"extra",	extra_flags,		"Object attributes."	},
    {	"addaffect",	apply_flags,		"Object addaffects."	},
    {	"weapon",	weapon_flags,		"Type of weapon." 	},
    {	"wclass",	weaponclass_flags,	"Overall weapon class flags."},
    {	"rweapon",	ranged_weapon_flags,	"Types of ranged weapons."},
    {	"guntype",	gun_type_flags,		"Firing rate for gun types."},
    {	"warhead",	warhead_flags,		"Type of bomb warhead."	},
    {	"propulsion",	propulsion_flags,	"Type of bomb propulsion."},
    {	"wear",		wear_flags,	 	"Where to wear object."	},
    {   "bionic",	bionic_flags,	 	"Where to equip bionic"	},
    {	"armor",	armor_types,		"Types of Armor."	},
    {	"container",	container_flags,	"Container status."	},
    {	"liquid",	liquid_flags,		"Types of liquids."	},
    {	"food",		food_condition,		"Food conditions."	},
    {	"gas",		gas_affects,		"Gas affects."		},
    {	"materials",	object_materials,	"Stuff objects can be made of."},
    {	"spec",		spec_table,	 	"Available special programs."},
    {	"sex",		sex_flags,	 	"Sexes."		},
    {	"act",		act_flags,	 	"Mobile attributes."	},
    {	"mpaffect",	mpaffect_flags,		"Mpaffect affects."	},     
    {	"affect",	affect_flags,		"Mobile affects."	},
    {	"affect2",	affect2_flags,		"Mobile affects 2."  	},
    {	"wear-loc",	wear_loc_flags,		"Where mobile wears object."},
    {	"spells",	skill_table,		"Names of current spells."},
    {	"games",	casino_games,		"Casino games."     	},
    {	"tattoo",	wear_tattoo,		"Tattoo wear_loc's."	},
    {	"booster",	tattoo_flags,		"List of magical boosters"},
    {	"immune",	immune_flags,		"Types of immunities."	},
    {	"vuln",		immune_flags,		"Types of vulnerabilities."},
    {	"resist",	immune_flags,		"Types of resistances."	},
    {	"racetype",	race_type_flags,	"Races for mobs."	},
    {	"fightstyle",	fighting_styles,	"Fighting styles for mobs."},
    {	"size",		size_flags,		"Sizes for mobs."	},
    {	"mprogs",	mprog_types,		"Types of MobProgs."	},
    {	"oprogs",	oprog_types,		"Types of ObjProgs."	},
    {	"rprogs",	rprog_types,		"Types of RoomProgs."	},
    {	"eprogs",	eprog_types,		"Types of ExitProgs."	},
    {	"direction",	direction_flags,	"Exit direction flags."	},
    {	"switch",	switch_affect_types,	"Types of things switches affect."	},
    {	"activate",	activation_commands,	"Command that activates a switch."	},
    {	"dooract",	door_action_flags,	"Types switches can do to doors."	},
    {	"mobhp",	mobhp_flags,		"Mobile hp modifier."	},
    {	"skin",		skin_flags,		"Mobile skin types."	},

    {	"",		0,		 	""			}
};



/*****************************************************************************
 Name:		show_flag_cmds
 Purpose:	Displays settable flags and stats.
 Called by:	show_help(olc_act.c).
 ****************************************************************************/
void show_flag_cmds( CHAR_DATA *ch, const struct flag_type *flag_table )
{
    char buf  [ MAX_STRING_LENGTH ];
    char buf1 [ MAX_STRING_LENGTH ];
    int  flag;
    int  col;
 
    buf1[0] = '\0';
    col = 0;
    for (flag = 0; *flag_table[flag].name; flag++)
    {
	if ( flag_table[flag].settable )
	{
	    sprintf( buf, "%-19.18s", flag_table[flag].name );
	    strcat( buf1, buf );
	    if ( ++col % 4 == 0 )
		strcat( buf1, "\n\r" );
	}
    }
 
    if ( col % 4 != 0 )
	strcat( buf1, "\n\r" );

    editor_send_to_char(C_DEFAULT, buf1, ch );
    return;
}



/*****************************************************************************
 Name:		show_skill_cmds
 Purpose:	Displays all skill functions.
 		Does remove those damn immortal commands from the list.
 		Could be improved by:
 		(1) Adding a check for a level range.
 Called by:	show_help(olc_act.c).
 ****************************************************************************/
void show_skill_cmds( CHAR_DATA *ch, int tar )
{
    char buf  [ MAX_STRING_LENGTH ];
    char buf1 [ MAX_STRING_LENGTH*2 ];
    int  sn;
    int  col;
 
    buf1[0] = '\0';
    col = 0;
    for (sn = 0; skill_table[sn].name[0] != '\0'; sn++)
    {
	if ( !skill_table[sn].name )
	    break;

	if ( !str_cmp( skill_table[sn].name, "reserved" )
	  || skill_table[sn].spell_fun == spell_null )
	    continue;

	if ( tar == -1 || skill_table[sn].target == tar )
	{
	    sprintf( buf, "%-19.18s", skill_table[sn].name );
	    strcat( buf1, buf );
	    if ( ++col % 4 == 0 )
		strcat( buf1, "\n\r" );
	}
    }
 
    if ( col % 4 != 0 )
	strcat( buf1, "\n\r" );

    editor_send_to_char(C_DEFAULT, buf1, ch );
    return;
}



/*****************************************************************************
 Name:		show_spec_cmds
 Purpose:	Displays settable special functions.
 Called by:	show_help(olc_act.c).
 ****************************************************************************/
void show_spec_cmds( CHAR_DATA *ch )
{
    char buf  [ MAX_STRING_LENGTH ];
    char buf1 [ MAX_STRING_LENGTH ];
    int  spec;
    int  col;
 
    buf1[0] = '\0';
    col = 0;
    editor_send_to_char(C_DEFAULT, "Preceed special functions with 'spec_'\n\r\n\r", ch );
    for (spec = 0; *spec_table[spec].spec_fun; spec++)
    {
	sprintf( buf, "%-30s", &spec_table[spec].spec_name[5] );
	strcat( buf1, buf );
	if ( ++col % 3 == 0 )
	    strcat( buf1, "\n\r" );
    }
 
    if ( col % 4 != 0 )
	strcat( buf1, "\n\r" );

    editor_send_to_char(C_DEFAULT, buf1, ch );
    return;
}



/*****************************************************************************
 Name:		show_help
 Purpose:	Displays help for many tables used in OLC.
 Called by:	olc interpreters.
 ****************************************************************************/
bool show_help( CHAR_DATA *ch, char *argument )
{
    char buf[MAX_STRING_LENGTH];
    char buf2[MAX_STRING_LENGTH];
    char arg[MAX_INPUT_LENGTH];
    char spell[MAX_INPUT_LENGTH];
    int cnt;

    argument = one_argument( argument, arg );
    one_argument( argument, spell );

    /*
     * Display syntax.
     */
    if ( arg[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  ? [command]\n\r\n\r", ch );
	editor_send_to_char(C_DEFAULT, "[command]  [description]\n\r", ch );

      strcpy( buf, "" );

	for (cnt = 0; help_table[cnt].command[0] != '\0'; cnt++)
	{
          sprintf( buf2, "%-10.10s -%s\n\r",
	        capitalize( help_table[cnt].command ),
		help_table[cnt].desc );
	    strcat( buf, buf2 );
	}
	editor_send_to_char(C_DEFAULT, buf, ch );
	return FALSE;
    }

    /*
     * Find the command, show changeable data.
     * ---------------------------------------
     */
    for (cnt = 0; *help_table[cnt].command; cnt++)
    {
        if (  arg[0] == help_table[cnt].command[0]
          && !str_prefix( arg, help_table[cnt].command ) )
	{
	    if ( help_table[cnt].structure == spec_table )
	    {
		show_spec_cmds( ch );
		return FALSE;
	    }
	    else
	    if ( help_table[cnt].structure == skill_table )
	    {

		if ( spell[0] == '\0' )
		{
		    editor_send_to_char(C_DEFAULT, "Syntax:  ? spells "
		        "[ignore/attack/defend/self/object/all]\n\r", ch );
		    return FALSE;
		}

		if ( !str_prefix( spell, "all" ) )
		    show_skill_cmds( ch, -1 );
		else if ( !str_prefix( spell, "ignore" ) )
		    show_skill_cmds( ch, TAR_IGNORE );
		else if ( !str_prefix( spell, "attack" ) )
		    show_skill_cmds( ch, TAR_CHAR_OFFENSIVE );
		else if ( !str_prefix( spell, "defend" ) )
		    show_skill_cmds( ch, TAR_CHAR_DEFENSIVE );
		else if ( !str_prefix( spell, "self" ) )
		    show_skill_cmds( ch, TAR_CHAR_SELF );
		else if ( !str_prefix( spell, "object" ) )
		    show_skill_cmds( ch, TAR_OBJ_INV );
		else
		    editor_send_to_char(C_DEFAULT, "Syntax:  ? spell "
		        "[ignore/attack/defend/self/object/all]\n\r", ch );
		    
		return FALSE;
	    }
	    else
	    {
		show_flag_cmds( ch, help_table[cnt].structure );
		return FALSE;
	    }
	}
    }

    show_help( ch, "" );
    return FALSE;
}

bool redit_proglist( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA      *pMobIndex;
    OBJ_INDEX_DATA      *pObjIndex;
    ROOM_INDEX_DATA	*pRoomIndex;
    AREA_DATA           *pArea;
    char                buf  [ MAX_STRING_LENGTH   ];
    char                buf1 [ MAX_STRING_LENGTH*2 ];
    char		buf2 [ MAX_STRING_LENGTH   ];
    char                arg  [ MAX_INPUT_LENGTH    ];
    bool fAll, found;
    int vnum;
    int  col = 0;

    one_argument( argument, arg );
    if ( arg[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  proglist <all/obj/mob/room>\n\r", ch );
        return FALSE;
    }

    pArea = ch->in_room->area;
    buf1[0] = '\0';
    fAll    = !str_cmp( arg, "all" );
    found   = FALSE;

    if ( ( fAll ) || ( !str_cmp( arg, "obj" ) ) )
    {
      found = TRUE;
      editor_send_to_char( AT_WHITE, "Objects with programs:\n\r", ch );
      for ( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
      {
	if ( ( pObjIndex = get_obj_index( vnum ) ) )
 	{
	   if ( pObjIndex->traptypes ) 
	   {
               sprintf( buf, "&z[&R%5d&z] &w%-17.16s",
                    pObjIndex->vnum,
                    capitalize( strip_color(pObjIndex->short_descr) ) );
                strcat( buf1, buf );
                if ( ++col % 3 == 0 )
                    strcat( buf1, "\n\r" );
	   }
	}
      }
      if ( col % 3 != 0 )
        strcat( buf1, "\n\r" );

      editor_send_to_char(C_DEFAULT, buf1, ch );
      buf1[0] = '\0';
      col = 0;
    }

    if ( ( fAll ) || ( !str_cmp( arg, "room" ) ) )
    {
      found = TRUE;
      editor_send_to_char( AT_WHITE, "Rooms with programs:\n\r", ch );
      for ( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
      {
        if ( ( pRoomIndex = get_room_index( vnum ) ) )
	{
           if ( pRoomIndex->traptypes )
           {
               sprintf( buf, "&z[&R%5d&z] &w%-17.16s",
                    pRoomIndex->vnum,
                    capitalize( strip_color(pRoomIndex->name) ) );
                strcat( buf1, buf );
                if ( ++col % 3 == 0 )
                    strcat( buf1, "\n\r" );
           }
	}
      }
      if ( col % 3 != 0 )
        strcat( buf1, "\n\r" );

      editor_send_to_char(C_DEFAULT, buf1, ch );
      buf1[0] = '\0';
      col = 0;
    }
    if ( ( fAll ) || ( !str_cmp( arg, "mob" ) ) )
    {
      found = TRUE;
      editor_send_to_char( AT_WHITE, "Mobs with programs:\n\r", ch );
      for ( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
      {
        if ( ( pMobIndex = get_mob_index( vnum ) ) )
	{
           if ( pMobIndex->progtypes )
           {
               sprintf( buf, "&z[&R%5d&z] &w%-17.16s",
                    pMobIndex->vnum,
                    capitalize( strip_color(pMobIndex->short_descr) ) );
                strcat( buf1, buf );
                if ( ++col % 3 == 0 )
                    strcat( buf1, "\n\r" );
           }
	}
      }
      if ( col % 3 != 0 )
        strcat( buf1, "\n\r" );

      editor_send_to_char(C_DEFAULT, buf1, ch );
      buf1[0] = '\0';
      col = 0;
    }
   if ( !found )
    {
	if ( !str_cmp( arg, "all" ) )
	{
          editor_send_to_char(C_DEFAULT, "Programs not found in this area.\n\r", ch);
          return FALSE;
	}
	else {
	  sprintf( buf2, "Programs on %ss not found in this area.\n\r", arg );
	  editor_send_to_char( C_DEFAULT, buf2, ch );
	  return FALSE;
	}
    }

    return FALSE;
}


bool redit_mlist( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA	*pMobIndex;
    AREA_DATA		*pArea;
    char		buf  [ MAX_STRING_LENGTH   ];
    char		buf1 [ MAX_STRING_LENGTH*2 ];
    char		arg  [ MAX_INPUT_LENGTH    ];
    bool fAll, found;
    int vnum;
    int  col = 0;

    one_argument( argument, arg );
    if ( arg[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  mlist <all/name>\n\r", ch );
	return FALSE;
    }

    pArea = ch->in_room->area;
    buf1[0] = '\0';
    fAll    = !str_cmp( arg, "all" );
    found   = FALSE;

    for ( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if ( ( pMobIndex = get_mob_index( vnum ) ) )
	{
	    if ( fAll || is_name( ch, arg, pMobIndex->player_name ) 
		|| (pMobIndex->level == atoi(arg)) )
	    {
		found = TRUE;
		sprintf( buf, "&z[&R%5d&z] &w%-17.16s",
		    pMobIndex->vnum,
		    capitalize( strip_color(pMobIndex->short_descr) ) );
		strcat( buf1, buf );
		if ( ++col % 3 == 0 )
		    strcat( buf1, "\n\r" );
	    }
	}
    }

    if ( !found )
    {
	editor_send_to_char(C_DEFAULT, "Mobile(s) not found in this area.\n\r", ch);
	return FALSE;
    }

    if ( col % 3 != 0 )
	strcat( buf1, "\n\r" );

    editor_send_to_char(C_DEFAULT, buf1, ch );
    return FALSE;
}



bool redit_olist( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA	*pObjIndex;
    AREA_DATA		*pArea;
    char		buf  [ MAX_STRING_LENGTH   ];
    char		buf1 [ MAX_STRING_LENGTH*2 ];
    char		arg  [ MAX_INPUT_LENGTH    ];
    bool fAll, found;
    int vnum;
    int  col = 0;

    one_argument( argument, arg );
    if ( arg[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  olist <all/name/item_type/object level>\n\r", ch );
	return FALSE;
    }

    pArea = ch->in_room->area;
    buf1[0] = '\0';
    fAll    = !str_cmp( arg, "all" );
    found   = FALSE;

    for ( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if ( ( pObjIndex = get_obj_index( vnum ) ) )
	{
	    if ( fAll || is_name( ch, arg, pObjIndex->name )
	    || ( flag_value( type_flags, arg ) == pObjIndex->item_type ) ||
	       ( pObjIndex->level == atoi(arg) ) )
	    {
		found = TRUE;
		sprintf( buf, "&z[&R%5d&z] &w%-17.16s",
		    pObjIndex->vnum, 
		    capitalize( strip_color(pObjIndex->short_descr) ) );
		strcat( buf1, buf );
		if ( ++col % 3 == 0 )
		    strcat( buf1, "\n\r" );
	    }
	}
    }

    if ( !found )
    {
	editor_send_to_char(C_DEFAULT, "Object(s) not found in this area.\n\r", ch);
	return FALSE;
    }

    if ( col % 3 != 0 )
	strcat( buf1, "\n\r" );

    editor_send_to_char(C_DEFAULT, buf1, ch );
    return FALSE;
}

bool redit_rlist( CHAR_DATA *ch, char *argument )
{
    ROOM_INDEX_DATA	*pRoomIndex;
    AREA_DATA		*pArea;
    char		buf  [ MAX_STRING_LENGTH   ];
    char		buf1 [ MAX_STRING_LENGTH*2 ];
    bool found;
    int vnum;
    int  col = 0;

    pArea = ch->in_room->area;
    buf1[0] = '\0';
    found   = FALSE;

    for ( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if ( ( pRoomIndex = get_room_index( vnum ) ) )
	{
		found = TRUE;
		sprintf( buf, "&z[&R%5d&z] &w%-17.16s",
		    pRoomIndex->vnum, 
		    capitalize( strip_color(pRoomIndex->name) ) );
		strcat( buf1, buf );
		if ( ++col % 3 == 0 )
		    strcat( buf1, "\n\r" );
	}
    }

    if ( !found )
    {
	editor_send_to_char(C_DEFAULT, "Room(s) not found in this area.\n\r", ch);
	return FALSE;
    }

    if ( col % 3 != 0 )
	strcat( buf1, "\n\r" );

    editor_send_to_char(C_DEFAULT, buf1, ch );
    return FALSE;
}



bool redit_mshow( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    int value;

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  mshow <vnum>\n\r", ch );
	return FALSE;
    }

    if ( is_number( argument ) )
    {
	value = atoi( argument );
	if ( !( pMob = get_mob_index( value ) ))
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  That mobile does not exist.\n\r", ch );
	    return FALSE;
	}

	ch->desc->pEdit = (void *)pMob;
    }
 
    medit_show( ch, argument );
    ch->desc->pEdit = (void *)ch->in_room;
    return FALSE; 
}

bool redit_oshow( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;
    int value;

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  oshow <vnum>\n\r", ch );
	return FALSE;
    }

    if ( is_number( argument ) )
    {
	value = atoi( argument );
	if ( !( pObj = get_obj_index( value ) ))
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  That object does not exist.\n\r", ch );
	    return FALSE;
	}

	ch->desc->pEdit = (void *)pObj;
    }
 
    oedit_show( ch, argument );
    ch->desc->pEdit = (void *)ch->in_room;
    return FALSE; 
}



/*****************************************************************************
 Name:		check_range( lower vnum, upper vnum )
 Purpose:	Ensures the range spans only one area.
 Called by:	aedit_vnum(olc_act.c).
 ****************************************************************************/
bool check_range( int lower, int upper )
{
    AREA_DATA *pArea;
    int cnt = 0;

    for ( pArea = area_first; pArea; pArea = pArea->next )
    {
	/*
	 * lower < area < upper
	 */
	if ( ( lower <= pArea->lvnum && upper >= pArea->lvnum )
	||   ( upper >= pArea->uvnum && lower <= pArea->uvnum ) )
	    cnt++;

	if ( cnt > 1 )
	    return FALSE;
    }
    return TRUE;
}



AREA_DATA *get_vnum_area( int vnum )
{
    AREA_DATA *pArea;

    for ( pArea = area_first; pArea; pArea = pArea->next )
    {
        if ( vnum >= pArea->lvnum
          && vnum <= pArea->uvnum )
            return pArea;
    }

    return 0;
}



/*
 * Area Editor Functions.
 */
bool aedit_show( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char buf  [MAX_STRING_LENGTH];
    struct stat statis;

    EDIT_AREA(ch, pArea);
    sprintf( buf, "&WName:     &z[&W%5d&z] &w%s\n\r",pArea->vnum,pArea->name );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&WLevel:    &z{&W%d&z-&W%d&z}\n\r", pArea->llevel, pArea->ulevel );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&WRecall:   &z[&W%5d&z]&w %s\n\r", pArea->recall,
	get_room_index( pArea->recall )
	? get_room_index( pArea->recall )->name : "none" );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WFile:     &C%s\n\r", pArea->filename );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "../../EOSBUILD/area/%s", pArea->filename );
    if ( !stat( buf, &statis ) )
    {
        sprintf( buf, "&WLast modifed on Build: &C%s\r",(char*)ctime(&statis.st_mtime ) );
        editor_send_to_char( C_DEFAULT, buf, ch );
    }
    else
        editor_send_to_char( C_DEFAULT, "&WLast modifed on Build: &C(No such area)\n\r", ch );

    sprintf( buf, "../../EOS/area/%s", pArea->filename );
    if ( !stat( buf, &statis ) )
    {
        sprintf( buf, "&WLast modifed on PaP: &C%s\r",(char*)ctime(&statis.st_mtime ) );
        editor_send_to_char( C_DEFAULT, buf, ch );
    }
    else
        editor_send_to_char( C_DEFAULT, "&WLast modifed on PaP: &C(No such area)\n\r", ch );


    sprintf( buf, "&WVnums:    &z[&W%d-%d&z]\n\r", pArea->lvnum,pArea->uvnum );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WColor:    &z[&W%d&z]\n\r", pArea->def_color );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WAge:      &z[&W%d&z]\n\r",	pArea->age );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WPlayers:  &z[&W%d&z]\n\r", pArea->nplayer );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WSecurity: &z[&W%d&z]\n\r", pArea->security );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WBuilders: &z[&B%s&z]\n\r", pArea->builders );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WFlags:    &z[&R%s&z]\n\r", flag_string( area_flags,pArea->area_flags ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WWeather chaos factor: &z[&W%s&z]\n\r", 
     flag_string( area_weather_flags, pArea->weather ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WAverage temperature:  &z[&W%d&z]\n\r", 
     pArea->average_temp );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WAverage Pressure:     &z[&W%d&z]\n\r", 
     pArea->average_humidity );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&WTemporal Status:      &z[&B%s&z]\n\r",
     flag_string( temporal_flags,pArea->temporal ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    return FALSE;
}

bool aedit_links( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    ROOM_INDEX_DATA *pRoomIndex;
    char buf  [MAX_STRING_LENGTH];
    int vnum;
    int nMatch = 0;
    int DIR;   

    EDIT_AREA(ch, pArea);

    editor_send_to_char( AT_RED, "Rooms that have exits into this area:\n\r", ch );
    for ( vnum = 0; nMatch < top_room; vnum++ )
    {
	if ( ( pRoomIndex = get_room_index( vnum ) ) )
	{
	  nMatch++;
        if ( pRoomIndex->area != pArea )  
          for ( DIR = 0; DIR <= DIR_MAX; DIR++ )
          {
            if ( pRoomIndex->exit[DIR] )
            {
              if (pRoomIndex->exit[DIR]->to_room)
              {
                if (    pRoomIndex->exit[DIR]->to_room->vnum >= pArea->lvnum 
                     && pRoomIndex->exit[DIR]->to_room->vnum <= pArea->uvnum )
                {
           	    sprintf( buf, "&z[&W%5d&z] &w%s\n\r",
               	    pRoomIndex->vnum, pRoomIndex->name);
       	            editor_send_to_char(AT_RED, buf, ch );
                }
              }
              else
               bug( "Room with a exit to room 0: %d", pRoomIndex->vnum );
            }
          }
	}
    }

    editor_send_to_char( AT_RED, "Rooms that have exits leaving this area:\n\r", ch );

    for ( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if ( ( pRoomIndex = get_room_index( vnum ) ) )
	{
          for ( DIR = 0; DIR <= DIR_MAX; DIR++ )
          {
            if ( pRoomIndex->exit[DIR] )
              if (pRoomIndex->exit[DIR]->to_room)
              if (    pRoomIndex->exit[DIR]->to_room->vnum < pArea->lvnum 
                   || pRoomIndex->exit[DIR]->to_room->vnum > pArea->uvnum )
              {
      	    sprintf( buf, "&z[&W%5d&z] &w%s\n\r",
          	    pRoomIndex->vnum, pRoomIndex->name);
       	    editor_send_to_char(AT_RED, buf, ch );
              }
          }
	}
    }  

    return FALSE;
}

bool aedit_reset( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;

    EDIT_AREA(ch, pArea);

    reset_area( pArea );
    editor_send_to_char(C_DEFAULT, "Area reset.\n\r", ch );

    return FALSE;
}



bool aedit_create( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;

    if ( top_area >= INT_MAX )	/* OLC 1.1b */
    {
	editor_send_to_char(C_DEFAULT, "We're out of vnums for new areas.\n\r", ch );
	return FALSE;
    }

    pArea               =   new_area();
    area_last->next     =   pArea;
    area_last		=   pArea;	/* Thanks, Walker. */
    ch->desc->pEdit     =   (void *)pArea;
/*    pArea->recall       =   25001;*/
    SET_BIT( pArea->area_flags, AREA_ADDED );
    SET_BIT( pArea->area_flags, AREA_PROTOTYPE );
    editor_send_to_char(C_DEFAULT, "Area Created.\n\r", ch );
    return TRUE;	/* OLC 1.1b */
}



bool aedit_name( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;

    EDIT_AREA(ch, pArea);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:   name [$name]\n\r", ch );
	return FALSE;
    }

    free_string( pArea->name );
    pArea->name = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Name set.\n\r", ch );
    return TRUE;
}



bool aedit_file( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char file[MAX_STRING_LENGTH];
    int i, length;

    EDIT_AREA(ch, pArea);

    one_argument( argument, file );	/* Forces Lowercase */

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  filename [$file]\n\r", ch );
	return FALSE;
    }

    /*
     * Simple Syntax Check.
     */
    length = strlen( argument );
    if ( length > 8 )
    {
	editor_send_to_char(C_DEFAULT, "No more than eight characters allowed.\n\r", ch );
	return FALSE;
    }

    /*
     * Allow only letters and numbers.
     */
    for ( i = 0; i < length; i++ )
    {
	if ( !isalnum( file[i] ) )
	{
	    editor_send_to_char(C_DEFAULT, "Only letters and numbers are valid.\n\r", ch );
	    return FALSE;
	}
    }    

    free_string( pArea->filename );
    strcat( file, ".are" );
    pArea->filename = str_dup( file );

    editor_send_to_char(C_DEFAULT, "Filename set.\n\r", ch );
    return TRUE;
}

bool aedit_llevel ( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char llevel[MAX_STRING_LENGTH];

    EDIT_AREA(ch, pArea);

    one_argument( argument, llevel );

    if ( !is_number( llevel ) || llevel[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  llevel [#lowest sugested level]\n\r", ch);
	return FALSE;
    }

    editor_send_to_char( C_DEFAULT, "Lowest sugested level set.\n\r", ch);

    pArea->llevel = atoi ( llevel );
    return TRUE;
}

bool aedit_ulevel ( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char ulevel[MAX_STRING_LENGTH];

    EDIT_AREA(ch, pArea);

    one_argument( argument, ulevel );

    if ( !is_number( ulevel ) || ulevel[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  ulevel [#upper most sugested level]\n\r", ch );
        return FALSE;
    }

    pArea->ulevel = atoi ( ulevel );

    editor_send_to_char(C_DEFAULT, "Upper most suggested level set.\n\r", ch );
    return TRUE;
}


bool aedit_age( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char age[MAX_STRING_LENGTH];

    EDIT_AREA(ch, pArea);

    one_argument( argument, age );

    if ( !is_number( age ) || age[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  age [#age]\n\r", ch );
	return FALSE;
    }

    pArea->age = atoi( age );

    editor_send_to_char(C_DEFAULT, "Age set.\n\r", ch );
    return TRUE;
}

bool aedit_clan_hq( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    
    EDIT_AREA( ch, pArea );
    
    if ( get_trust( ch ) >= L_CON )
    {
        TOGGLE_BIT( pArea->area_flags, AREA_CLAN_HQ );
	editor_send_to_char(C_DEFAULT, "Ok.\n\r", ch );
	return TRUE;
    }
     else
    {
	editor_send_to_char(C_DEFAULT, "You are &B*&wtoo&B*&w low of trust to do this.\n\r", ch );
	return FALSE;
    }
return TRUE;
}
bool aedit_prototype( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;

    EDIT_AREA( ch, pArea );

	pArea->area_flags ^= AREA_PROTOTYPE;
	editor_send_to_char(C_DEFAULT, "Ok.\n\r", ch );
	return TRUE;
}

bool aedit_mudschool( CHAR_DATA *ch, char *argument )
{

    AREA_DATA *pArea;
        
    EDIT_AREA( ch, pArea );
     
    if ( get_trust( ch ) >= L_CON )
    {
        TOGGLE_BIT( pArea->area_flags, AREA_MUDSCHOOL );
        editor_send_to_char(C_DEFAULT, "Ok.\n\r", ch );  
        return TRUE;
    }
     else
    {
        editor_send_to_char(C_DEFAULT, "You are &B*&wtoo&B*&w low of trust to do this.\n\r", ch );
        return FALSE;
    }
    
    return TRUE;

}

bool aedit_color( CHAR_DATA *ch, char *argument )
{

    AREA_DATA *pArea;
    int i = 0;
    
    EDIT_AREA( ch, pArea );
     
    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  color[color number] \n\r", ch );
	editor_send_to_char(C_DEFAULT, "&WType &Rhelp AT_COLOR&W for list of colors.\n\r",ch);
	return FALSE;
    }

    if(!(is_number(argument)))
    {
    	editor_send_to_char(C_DEFAULT, "Color choices must be numeric (1-15).\n\r", ch );
	editor_send_to_char(C_DEFAULT, "&WType &Rhelp AT_COLOR&W for list of colors.\n\r", ch);
	return FALSE;
    }
    i = atoi(argument);
    if( i < 1 || i > 15)
    {
    	editor_send_to_char(C_DEFAULT, "Color choices are from 1-15 only.\n\r", ch );
	editor_send_to_char(C_DEFAULT, "&WType &Rhelp AT_COLOR&W for list of colors.\n\r", ch);
	return FALSE;
    }    
    pArea->def_color = i;

    return TRUE;
}

bool aedit_recall( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char room[MAX_STRING_LENGTH];
    int  value;

    EDIT_AREA(ch, pArea);

    one_argument( argument, room );

    if ( !is_number( argument ) || argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  recall [#rvnum]\n\r", ch );
	return FALSE;
    }

    value = atoi( room );

    if ( !get_room_index( value ) )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Room vnum does not exist.\n\r", ch );
	return FALSE;
    }

    pArea->recall = value;

    editor_send_to_char(C_DEFAULT, "Recall set.\n\r", ch );
    return TRUE;
}



bool aedit_security( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char sec[MAX_STRING_LENGTH];
    char buf[MAX_STRING_LENGTH];
    int  value;

    EDIT_AREA(ch, pArea);

    one_argument( argument, sec );

    if ( !is_number( sec ) || sec[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  security [#level]\n\r", ch );
	return FALSE;
    }

    value = atoi( sec );

    if ( value > ch->pcdata->security || value < 0 )
    {
	if ( ch->pcdata->security != 0 )
	{
	    sprintf( buf, "Security is 0-%d.\n\r", ch->pcdata->security );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	}
	else
	    editor_send_to_char(C_DEFAULT, "Security is 0 only.\n\r", ch );
	return FALSE;
    }

    pArea->security = value;

    editor_send_to_char(C_DEFAULT, "Security set.\n\r", ch );
    return TRUE;
}



bool aedit_builder( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char name[MAX_STRING_LENGTH];
    char buf[MAX_STRING_LENGTH];

    EDIT_AREA(ch, pArea);

    one_argument( argument, name );

    if ( name[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  builder [$name]  -toggles builder\n\r", ch );
	editor_send_to_char(C_DEFAULT, "Syntax:  builder All      -allows everyone\n\r", ch );
	return FALSE;
    }

    name[0] = UPPER( name[0] );

    if ( strstr( pArea->builders, name ) != '\0' )
    {
	pArea->builders = string_replace( pArea->builders, name, "\0" );
	pArea->builders = string_unpad( pArea->builders );

	if ( pArea->builders[0] == '\0' )
	{
	    free_string( pArea->builders );
	    pArea->builders = str_dup( "None" );
	}
	editor_send_to_char(C_DEFAULT, "Builder removed.\n\r", ch );
	return TRUE;
    }
    else
    {
	buf[0] = '\0';
	if ( strstr( pArea->builders, "None" ) != '\0' )
	{
	    pArea->builders = string_replace( pArea->builders, "None", "\0" );
	    pArea->builders = string_unpad( pArea->builders );
	}

	if (pArea->builders[0] != '\0' )
	{
	    strcat( buf, pArea->builders );
	    strcat( buf, " " );
	}
	strcat( buf, name );
	free_string( pArea->builders );
	pArea->builders = string_proper( str_dup( buf ) );

	editor_send_to_char(C_DEFAULT, "Builder added.\n\r", ch );
	return TRUE;
    }

    return FALSE;
}



bool aedit_vnum( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char lower[MAX_STRING_LENGTH];
    char upper[MAX_STRING_LENGTH];
    int  ilower;
    int  iupper;

    EDIT_AREA(ch, pArea);

    argument = one_argument( argument, lower );
    one_argument( argument, upper );

    if ( !is_number( lower ) || lower[0] == '\0'
    || !is_number( upper ) || upper[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  vnum [#lower] [#upper]\n\r", ch );
	return FALSE;
    }

    if ( ( ilower = atoi( lower ) ) > ( iupper = atoi( upper ) ) )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Upper must be larger then lower.\n\r", ch );
	return FALSE;
    }

    /* OLC 1.1b */
    if ( ilower <= 0 || ilower >= INT_MAX || iupper <= 0 || iupper >= INT_MAX )
    {
	char output[MAX_STRING_LENGTH];

	sprintf( output, "AEdit: vnum must be between 0 and %d.\n\r", INT_MAX );
	editor_send_to_char(C_DEFAULT, output, ch );
	return FALSE;
    }

    if ( !check_range( ilower, iupper ) )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Range must include only this area.\n\r", ch );
	return FALSE;
    }

    if ( get_vnum_area( ilower )
    && get_vnum_area( ilower ) != pArea )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Lower vnum already assigned.\n\r", ch );
	return FALSE;
    }

    pArea->lvnum = ilower;
    editor_send_to_char(C_DEFAULT, "Lower vnum set.\n\r", ch );

    if ( get_vnum_area( iupper )
    && get_vnum_area( iupper ) != pArea )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Upper vnum already assigned.\n\r", ch );
	return TRUE;	/* The lower value has been set. */
    }

    pArea->uvnum = iupper;
    editor_send_to_char(C_DEFAULT, "Upper vnum set.\n\r", ch );

    return TRUE;
}



bool aedit_lvnum( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char lower[MAX_STRING_LENGTH];
    int  ilower;
    int  iupper;

    EDIT_AREA(ch, pArea);

    one_argument( argument, lower );

    if ( !is_number( lower ) || lower[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  lvnum [#lower]\n\r", ch );
	return FALSE;
    }

    if ( ( ilower = atoi( lower ) ) > ( iupper = pArea->uvnum ) )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Value must be less than the uvnum.\n\r", ch );
	return FALSE;
    }

    /* OLC 1.1b */
    if ( ilower <= 0 || ilower >= INT_MAX || iupper <= 0 || iupper >= INT_MAX )
    {
	char output[MAX_STRING_LENGTH];

	sprintf( output, "AEdit: vnum must be between 0 and %d.\n\r", INT_MAX );
	editor_send_to_char(C_DEFAULT, output, ch );
	return FALSE;
    }

    if ( !check_range( ilower, iupper ) )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Range must include only this area.\n\r", ch );
	return FALSE;
    }

    if ( get_vnum_area( ilower )
    && get_vnum_area( ilower ) != pArea )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Lower vnum already assigned.\n\r", ch );
	return FALSE;
    }

    pArea->lvnum = ilower;
    editor_send_to_char(C_DEFAULT, "Lower vnum set.\n\r", ch );
    return TRUE;
}



bool aedit_uvnum( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    char upper[MAX_STRING_LENGTH];
    int  ilower;
    int  iupper;

    EDIT_AREA(ch, pArea);

    one_argument( argument, upper );

    if ( !is_number( upper ) || upper[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  uvnum [#upper]\n\r", ch );
	return FALSE;
    }

    if ( ( ilower = pArea->lvnum ) > ( iupper = atoi( upper ) ) )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Upper must be larger then lower.\n\r", ch );
	return FALSE;
    }

    /* OLC 1.1b */
    if ( ilower < 0 || ilower >= INT_MAX || iupper <= 0 || iupper >= INT_MAX )
    {
	char output[MAX_STRING_LENGTH];

	sprintf( output, "AEdit: vnum must be between 0 and %d.\n\r", INT_MAX );
	editor_send_to_char(C_DEFAULT, output, ch );
	return FALSE;
    }

    if ( !check_range( ilower, iupper ) )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Range must include only this area.\n\r", ch );
	return FALSE;
    }

    if ( get_vnum_area( iupper )
    && get_vnum_area( iupper ) != pArea )
    {
	editor_send_to_char(C_DEFAULT, "AEdit:  Upper vnum already assigned.\n\r", ch );
	return FALSE;
    }

    pArea->uvnum = iupper;
    editor_send_to_char(C_DEFAULT, "Upper vnum set.\n\r", ch );

    return TRUE;
}



/*
 * Room Editor Functions.
 */
bool redit_show( CHAR_DATA *ch, char *argument )
{
    ROOM_INDEX_DATA	*pRoom;
    OBJ_INDEX_DATA		*pOre;
    char		buf  [MAX_STRING_LENGTH];
    char		buf1 [2*MAX_STRING_LENGTH];
    int			door;

    EDIT_ROOM(ch, pRoom);

    buf1[0] = '\0';

    sprintf( buf, "&WDescription:\n\r&Y%s&w", pRoom->description );
    strcat( buf1, buf );

    sprintf( buf, "&WName:          &z[&W%s&z]\n\r&WArea:          &z[&W%5d&z] &w%s&w\n\r",
	    pRoom->name, pRoom->area->vnum, pRoom->area->name );
    strcat( buf1, buf );

    sprintf( buf, "&WVnum:          &z[&W%5d&z]\n\r&WSector:        &z[&G%s&z]\n\r",
	    pRoom->vnum, flag_string( sector_flags, pRoom->sector_type ) );
    strcat( buf1, buf );

    if ( pRoom->rd > 0 )
    {
     sprintf( buf, "&WRoom damage:   &z[&R%d&z]\n\r", pRoom->rd );
     strcat( buf1, buf );
    }

    sprintf( buf, "&WRoom flags:    &z[&C%s&z]\n\r",
	    flag_string( room_flags, pRoom->room_flags ) );
    strcat( buf1, buf );
    
    if ( pRoom->ore_type > 0 )
    {
     sprintf( buf, "&WOre abundance: &z[&B%d&z]\n\r", 
      pRoom->ore_fertility );
     strcat( buf1, buf );
      
     pOre = get_obj_index( pRoom->ore_type );
     sprintf( buf, "&WOre type&w:      &z[&B%d&z] "
		    "[&W%s&z]\n\r", pRoom->ore_type, 
	     pOre ? pOre->short_descr : "&RError: No such object."  );
     strcat( buf1, buf );
    }

    if ( pRoom->extra_descr )
    {
	EXTRA_DESCR_DATA *ed;

	strcat( buf1, "&WDesc Kwds:     &z[&W" );
	for ( ed = pRoom->extra_descr; ed; ed = ed->next )
	{
	    strcat( buf1, ed->keyword );
	    if ( ed->next )
		strcat( buf1, ", " );
	}
	strcat( buf1, "&z]\n\r" );
    }

    for ( door = 0; door < MAX_DIR; door++ )
    {
	EXIT_DATA *pexit;

	if ( ( pexit = pRoom->exit[door] ) )
	{
	    char word[MAX_INPUT_LENGTH];
	    char reset_state[MAX_STRING_LENGTH];
	    char *state;
	    int i, length;

	    sprintf( buf, "&W-%-5s to &z[&W%5d&z] &WKey: &z[&W%5d&z]",
		capitalize(dir_name[door]),
		pexit->to_room ? pexit->to_room->vnum : 0,
		pexit->key );
	    strcat( buf1, buf );

	    /*
	     * Format up the exit info.
	     * Capitalize all flags that are not part of the reset info.
	     */
	    strcpy( reset_state, flag_string( exit_flags, pexit->rs_flags ) );
	    state = flag_string( exit_flags, pexit->exit_info );
	    strcat( buf1, " &WExit flags: &z[&R" );
	    for (; ;)
	    {
		state = one_argument( state, word );

		if ( word[0] == '\0' )
		{
		    int end;

		    end = strlen(buf1) - 1;
		    buf1[end] = '&';
   		    strcat( buf1, "z]&g\n\r" );
		    break;
		}

		if ( str_infix( word, reset_state ) )
		{
		    length = strlen(word);
		    for (i = 0; i < length; i++)
			word[i] = toupper(word[i]);
		}
		strcat( buf1, word );
		strcat( buf1, " " );
	    }

	    if ( pexit->keyword && pexit->keyword[0] != '\0' )
	    {
		sprintf( buf, "&WKwds: &z[&W%s&z]&g\n\r", pexit->keyword);
		strcat( buf1, buf );
	    }
	    if ( pexit->description && pexit->description[0] != '\0' )
	    {
		sprintf( buf, "%s", pexit->description );
		strcat( buf1, buf );
	    }
	}
    }

    editor_send_to_char(C_DEFAULT, buf1, ch );
    return FALSE;
}

bool redit_rdamage( CHAR_DATA *ch, char *argument )
{
  ROOM_INDEX_DATA *pRoom;
  char arg[MAX_STRING_LENGTH];
  int damage;
  
  EDIT_ROOM( ch, pRoom );
  
  if ( argument[0] == '\0' )
  {
    editor_send_to_char( AT_PURPLE, "Syntax: rdamage [amount]\n\r", ch );
    return FALSE;
  }
  
  argument = one_argument( argument, arg );
  if ( !is_number( arg ) )
  {
    editor_send_to_char( AT_WHITE, "Amount must be the number of hp the room is to damage the CH per update.\n\r", ch );
    return FALSE;
  }
  
  damage = atoi( arg );
  pRoom->rd = damage;
  editor_send_to_char( AT_WHITE, "Ok.\n\r", ch );
  return TRUE;
}

bool redit_ore_type( CHAR_DATA *ch, char *argument )
{
  ROOM_INDEX_DATA *pRoom;
  OBJ_INDEX_DATA  *pOre;
  char arg[MAX_STRING_LENGTH];
  char buf  [ MAX_STRING_LENGTH ];
  int  value = 0;
  
  EDIT_ROOM( ch, pRoom );
  
  if ( argument[0] == '\0' )
  {
    editor_send_to_char( AT_PURPLE, "Syntax: oretype (ore vnum)\n\r", ch );
    return FALSE;
  }
  
  argument = one_argument( argument, arg );
  if ( !is_number( arg ) )
  {
    editor_send_to_char( AT_PURPLE, "Syntax: oretype (ore vnum)\n\r", ch );
    return FALSE;
  }

  value = atoi(arg);
  
  if ( value < 0 || value > 33000 )
  {
    editor_send_to_char( AT_WHITE, "Invalid vnum.\n\r", ch );
    return FALSE;
  }

 if ( value != 0 )
 {
  if ( !( pOre = get_obj_index( value )) )
  {
   sprintf( buf, "No object with vnum %d exists.\n\r", value );
   editor_send_to_char( AT_GREY, buf, ch );
   return FALSE;
  }

  if ( pOre->item_type != ITEM_ORE )
  {
   sprintf( buf, "That object is not of type ore.\n\r" );
   editor_send_to_char( AT_GREY, buf, ch );
   return FALSE;
  }
 }

  pRoom->ore_type = value;
  editor_send_to_char( AT_WHITE, "Ok.\n\r", ch );
  return TRUE;
}

bool redit_ore_fertility( CHAR_DATA *ch, char *argument )
{
  ROOM_INDEX_DATA *pRoom;
  char arg[MAX_STRING_LENGTH];
  
  EDIT_ROOM( ch, pRoom );
  
  if ( argument[0] == '\0' )
  {
    editor_send_to_char( AT_PURPLE, "Syntax: oreabundance (% value reflecting how fast ore 'grows' in a room)\n\r", ch );
    return FALSE;
  }
  
  argument = one_argument( argument, arg );
  if ( !is_number( arg ) )
  {
    editor_send_to_char( AT_PURPLE, "Syntax: oreabundance (% value reflecting how fast ore 'grows' in a room)\n\r", ch );
    return FALSE;
  }
  
  pRoom->ore_fertility = atoi( arg );
  editor_send_to_char( AT_WHITE, "Ok.\n\r", ch );
  return TRUE;
}


/* OLC 1.1b */
/*****************************************************************************
 Name:		change_exit
 Purpose:	Command interpreter for changing exits.
 Called by:	redit_<dir>.  This is a local function.
 ****************************************************************************/
bool change_exit( CHAR_DATA *ch, char *argument, int door )
{
    ROOM_INDEX_DATA *pRoom;
    char command[MAX_INPUT_LENGTH];
    char arg[MAX_INPUT_LENGTH];
    char total_arg[MAX_STRING_LENGTH];
    int  rev;
    int  value = 0;

    EDIT_ROOM(ch, pRoom);

    /* Often used data. */
    rev = rev_dir[door];
    
    if ( argument[0] == '\0' )
    {
	do_help( ch, "EXIT" );
	return FALSE;
    }

    /*
     * Now parse the arguments.
     */
    strcpy( total_arg, argument );
    argument = one_argument( argument, command );
    one_argument( argument, arg );

    if ( !str_cmp( command, "delete" ) )
    {
	if ( !pRoom->exit[door] || pRoom->exit[door]->to_room == 0 )
	{
	   editor_send_to_char( C_DEFAULT, "REdit:  Bad Room number 0.. don't delete it :)", ch );
	   return FALSE;
	}
	if ( !pRoom->exit[door] )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Exit does not exist.\n\r", ch );
	    return FALSE;
	}

	/*
	 * Remove To Room Exit.
	 */
	if ( pRoom->exit[door]->to_room->exit[rev] )
	{
	    free_exit( pRoom->exit[door]->to_room->exit[rev] );
	    pRoom->exit[door]->to_room->exit[rev] = NULL;
	}

	/*
	 * Remove this exit.
	 */
	free_exit( pRoom->exit[door] );
	pRoom->exit[door] = NULL;

	editor_send_to_char(C_DEFAULT, "Exit unlinked.\n\r", ch );
	return TRUE;
    }

    /*
     * Create a two-way exit.
     */
    if ( !str_cmp( command, "link" ) )
    {
	EXIT_DATA	*pExit;
	ROOM_INDEX_DATA	*pLinkRoom;

	if ( arg[0] == '\0' || !is_number( arg ) )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  [direction] link [vnum]\n\r", ch );
	    return FALSE;
	}

	if ( !( pLinkRoom = get_room_index( atoi(arg) ) ) )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Non-existant room.\n\r", ch );
	    return FALSE;
	}

	if ( pLinkRoom->exit[rev] )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Remote side's exit exists.\n\r", ch );
	    return FALSE;
	}

	if ( !pRoom->exit[door] )		/* No exit.		*/
	    pRoom->exit[door] = new_exit();

	pRoom->exit[door]->to_room = pLinkRoom;	/* Assign data.		*/
	pRoom->exit[door]->vnum = value;

	pExit			= new_exit();	/* No remote exit.	*/

	pExit->to_room		= ch->in_room;	/* Assign data.		*/
	pExit->vnum		= ch->in_room->vnum;

	pLinkRoom->exit[rev]	= pExit;	/* Link exit to room.	*/

	editor_send_to_char(C_DEFAULT, "Two-way link established.\n\r", ch );
	return TRUE;
    }

    /*
     * Create room and make two-way exit.
     */
    if ( !str_cmp( command, "dig" ) )
    {
	char buf[MAX_INPUT_LENGTH];

	if ( arg[0] != '\0' && !is_number( arg ) )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax: [direction] dig <vnum>\n\r", ch );
	    return FALSE;
	}

/* Decklarean did this..  not pretty but gets the job done
 * for finding blank vnums for this dig command
 */
    if ( arg[0] == '\0' )
    {
      int value;
      AREA_DATA * pArea;
      
      pArea = ch->in_room->area;
      for ( value = pArea->lvnum; pArea->uvnum >= value; value++ )
      {
        if ( !get_room_index( value ) )
         break;
      }
      if ( value > pArea->uvnum )
      {
        editor_send_to_char( C_DEFAULT, "REdit:  No free room vnums in this area.\n\r", ch );
        return FALSE;
      }
      sprintf( arg, "%d", value);
    }

	
	redit_create( ch, arg );		/* Create the room.	*/
	sprintf( buf, "link %s", arg );
	change_exit( ch, buf, door);		/* Create the exits.	*/
        sprintf( buf, "New room vnum: %s", arg );
	editor_send_to_char( AT_WHITE, buf, ch );
	return TRUE;
    }

    /*
     * Create one-way exit.
     */
    if ( !str_cmp( command, "room" ) )
    {
	ROOM_INDEX_DATA *pLinkRoom;

	if ( arg[0] == '\0' || !is_number( arg ) )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  [direction] room [vnum]\n\r", ch );
	    return FALSE;
	}

	if ( !( pLinkRoom = get_room_index( atoi( arg ) ) ) )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Non-existant room.\n\r", ch );
	    return FALSE;
	}

	if ( !pRoom->exit[door] )
	    pRoom->exit[door] = new_exit();

	pRoom->exit[door]->to_room = pLinkRoom;
	pRoom->exit[door]->vnum = value;

	editor_send_to_char(C_DEFAULT, "One-way link established.\n\r", ch );
	return TRUE;
    }

    if ( !str_cmp( command, "remove" ) )
    {
	if ( arg[0] == '\0' )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  [direction] remove [key/name/desc]\n\r", ch );
	    return FALSE;
	}

	if ( !pRoom->exit[door] )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Exit does not exist.\n\r", ch );
	    return FALSE;
	}

	if ( !str_cmp( argument, "key" ) )
	{
	    pRoom->exit[door]->key = 0;
            editor_send_to_char(C_DEFAULT, "Exit key removed.\n\r", ch );                        
	    return TRUE;
	}

	if ( !str_cmp( argument, "name" ) )
	{
	    free_string( pRoom->exit[door]->keyword );
	    pRoom->exit[door]->keyword = &str_empty[0];
            editor_send_to_char(C_DEFAULT, "Exit name removed.\n\r", ch );                        
            return TRUE;
	}

	if ( argument[0] == 'd' && !str_prefix( argument, "description" ) )
	{
	    free_string( pRoom->exit[door]->description );
	    pRoom->exit[door]->description = &str_empty[0];
            editor_send_to_char(C_DEFAULT, "Exit description removed.\n\r", ch );                        
            return TRUE;
	}

	editor_send_to_char(C_DEFAULT, "Syntax:  [direction] remove [key/name/desc]\n\r", ch );
	return FALSE;
    }

    if ( !str_cmp( command, "key" ) )
    {
	OBJ_INDEX_DATA *pObjIndex;

	if ( arg[0] == '\0' || !is_number( arg ) )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  [direction] key [vnum]\n\r", ch );
	    return FALSE;
	}

	if ( !( pObjIndex = get_obj_index( atoi( arg ) ) ) )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Item does not exist.\n\r", ch );
	    return FALSE;
	}

	if ( !pRoom->exit[door] )
	    pRoom->exit[door] = new_exit();

	pRoom->exit[door]->key = pObjIndex->vnum;

	editor_send_to_char(C_DEFAULT, "Exit key set.\n\r", ch );
	return TRUE;
    }

    if ( !str_cmp( command, "name" ) )
    {
	if ( arg[0] == '\0' )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  [direction] name [string]\n\r", ch );
	    return FALSE;
	}

	if ( !pRoom->exit[door] )
	    pRoom->exit[door] = new_exit();

	free_string( pRoom->exit[door]->keyword );
	pRoom->exit[door]->keyword = str_dup( argument );

	editor_send_to_char(C_DEFAULT, "Exit name set.\n\r", ch );
	return TRUE;
    }

    if ( command[0] == 'd' && !str_prefix( command, "description" ) )
    {
	if ( arg[0] == '\0' )
	{
	    if ( !pRoom->exit[door] )
	        pRoom->exit[door] = new_exit();

	    string_append( ch, &pRoom->exit[door]->description );
	    return TRUE;
	}

	editor_send_to_char(C_DEFAULT, "Syntax:  [direction] desc\n\r", ch );
	return FALSE;
    }

    /*
     * Set the exit flags, needs full argument.
     * ----------------------------------------
     */
    if ( ( value = flag_value( exit_flags, total_arg ) ) != NO_FLAG )
    {
	ROOM_INDEX_DATA *pToRoom;

	/*
	 * Create an exit if none exists.
	 */
	if ( !pRoom->exit[door] )
	    pRoom->exit[door] = new_exit();

	/*
	 * Set door bits for this room.
	 */
	TOGGLE_BIT(pRoom->exit[door]->rs_flags, value);
	pRoom->exit[door]->exit_info = pRoom->exit[door]->rs_flags;

	/*
	 * Set door bits of connected room.
	 * Skip one-way exits and non-existant rooms.
	 */
	if ( ( pToRoom = pRoom->exit[door]->to_room ) && pToRoom->exit[rev] )
	{
	    TOGGLE_BIT(pToRoom->exit[rev]->rs_flags, value);
	    pToRoom->exit[rev]->exit_info =  pToRoom->exit[rev]->rs_flags;
	}

	editor_send_to_char(C_DEFAULT, "Exit flag toggled.\n\r", ch );
	return TRUE;
    }

    return FALSE;
}



bool redit_north( CHAR_DATA *ch, char *argument )
{
    if ( argument[0] == '\0' )
    {
     move_char( ch, DIR_NORTH, FALSE, FALSE );
     return FALSE;
    }

    if ( change_exit( ch, argument, DIR_NORTH ) )
	return TRUE;

    return FALSE;
}



bool redit_south( CHAR_DATA *ch, char *argument )
{
    if ( argument[0] == '\0' )
    {
     move_char( ch, DIR_SOUTH, FALSE, FALSE );
     return FALSE;
    }

    if ( change_exit( ch, argument, DIR_SOUTH ) )
	return TRUE;

    return FALSE;
}



bool redit_east( CHAR_DATA *ch, char *argument )
{
    if ( argument[0] == '\0' )
    {
     move_char( ch, DIR_EAST, FALSE, FALSE );
     return FALSE;
    }

    if ( change_exit( ch, argument, DIR_EAST ) )
	return TRUE;

    return FALSE;
}



bool redit_west( CHAR_DATA *ch, char *argument )
{
    if ( argument[0] == '\0' )
    {
     move_char( ch, DIR_WEST, FALSE, FALSE );
     return FALSE;
    }

    if ( change_exit( ch, argument, DIR_WEST ) )
	return TRUE;

    return FALSE;
}



bool redit_up( CHAR_DATA *ch, char *argument )
{
    if ( argument[0] == '\0' )
    {
     move_char( ch, DIR_UP, FALSE, FALSE );
     return FALSE;
    }

    if ( change_exit( ch, argument, DIR_UP ) )
	return TRUE;

    return FALSE;
}



bool redit_down( CHAR_DATA *ch, char *argument )
{
    if ( argument[0] == '\0' )
    {
     move_char( ch, DIR_DOWN, FALSE, FALSE );
     return FALSE;
    }

    if ( change_exit( ch, argument, DIR_DOWN ) )
	return TRUE;

    return FALSE;
}


/* OLC 1.1b */
bool redit_move( CHAR_DATA *ch, char *argument )
{
    interpret( ch, argument );
    return FALSE;
}



bool redit_ed( CHAR_DATA *ch, char *argument )
{
    ROOM_INDEX_DATA *pRoom;
    EXTRA_DESCR_DATA *ed;
    char command  [ MAX_INPUT_LENGTH ];
/*    char keyword  [ MAX_INPUT_LENGTH ]; */
    EDIT_ROOM(ch, pRoom);

    argument = one_argument( argument, command );
/*     one_argument( argument, keyword );   */

    if ( command[0] == '\0' || /*keyword*/ argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  ed add [keyword]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         ed edit [keyword]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         ed delete [keyword]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         ed format [keyword]\n\r", ch );
	return FALSE;
    }

    if ( !str_cmp( command, "add" ) )
    {
	if ( /*keyword*/ argument[0] == '\0' )  
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  ed add [keyword]\n\r", ch );
	    return FALSE;
	}

	ed			=   new_extra_descr();
	ed->keyword		=   str_dup( /*keyword*/ argument );
 /*	ed->description		=   str_dup( "" );*/
	ed->next		=   pRoom->extra_descr;
	pRoom->extra_descr	=   ed;

	string_append( ch, &ed->description );

	return TRUE;
    }


    if ( !str_cmp( command, "edit" ) )
    {
	if ( /*keyword*/ argument[0] == '\0' ) 
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  ed edit [keyword]\n\r", ch );
	    return FALSE;
	}

	for ( ed = pRoom->extra_descr; ed; ed = ed->next )
	{
	    if ( is_name( ch, /*keyword*/  argument, ed->keyword ) )
		break;
	}

	if ( !ed )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Extra description keyword not found.\n\r", ch );
	    return FALSE;
	}

	string_append( ch, &ed->description );

	return TRUE;
    }


    if ( !str_cmp( command, "delete" ) )
    {
	EXTRA_DESCR_DATA *ped = NULL;

	if (/* keyword*/ argument[0] == '\0' )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  ed delete [keyword]\n\r", ch );
	    return FALSE;
	}

	for ( ed = pRoom->extra_descr; ed; ed = ed->next )
	{
	    if ( is_name( ch, /*keyword*/ argument, ed->keyword ) )
		break;
	    ped = ed;
	}

	if ( !ed )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Extra description keyword not found.\n\r", ch );
	    return FALSE;
	}

	if ( !ped )
	    pRoom->extra_descr = ed->next;
	else
	    ped->next = ed->next;

	free_extra_descr( ed );

	editor_send_to_char(C_DEFAULT, "Extra description deleted.\n\r", ch );
	return TRUE;
    }


    if ( !str_cmp( command, "format" ) )
    {
	if ( /*keyword*/ argument[0] == '\0' )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  ed format [keyword]\n\r", ch );
	    return FALSE;
	}

	for ( ed = pRoom->extra_descr; ed; ed = ed->next )
	{
	    if ( is_name( ch,/*keyword*/ argument, ed->keyword ) )
		break;
	}

	if ( !ed )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Extra description keyword not found.\n\r", ch );
	    return FALSE;
	}

	/* OLC 1.1b */
	if ( strlen(ed->description) >= (MAX_STRING_LENGTH - 4) )
	{
	    editor_send_to_char(C_DEFAULT, "String too long to be formatted.\n\r", ch );
	    return FALSE;
	}

	ed->description = format_string( ed->description );

	editor_send_to_char(C_DEFAULT, "Extra description formatted.\n\r", ch );
	return TRUE;
    }

    redit_ed( ch, "" );
    return FALSE;
}



bool redit_create( CHAR_DATA *ch, char *argument )
{
    AREA_DATA *pArea;
    ROOM_INDEX_DATA *pRoom;
    int value;
    int iHash;

    EDIT_ROOM(ch, pRoom);

    value = atoi( argument );

/* Decklarean did this */
    if ( argument[0] == '\0' )
    {
      pArea = ch->in_room->area;
      for ( value = pArea->lvnum; pArea->uvnum >= value; value++ )
      {
	if ( !get_room_index( value ) )
	 break;
      }
      if ( value > pArea->uvnum )
      {
	editor_send_to_char( C_DEFAULT, "REdit:  No free room vnums in this area.\n\r", ch );
	return FALSE;
      }
    }



    /* OLC 1.1b */
    if ( /*argument[0] == '\0' ||*/ value <= 0 || value >= INT_MAX )
    {
	char output[MAX_STRING_LENGTH];

	sprintf( output, "Syntax:  create [0 < vnum < %d]\n\r", INT_MAX );
	editor_send_to_char(C_DEFAULT, output, ch );
	return FALSE;
    }

    if (/* argument[0] == '\0' ||*/ value <= 0 )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  create [vnum > 0]\n\r", ch );
	return FALSE;
    }

    pArea = get_vnum_area( value );
    if ( !pArea )
    {
	editor_send_to_char(C_DEFAULT, "REdit:  That vnum is not assigned an area.\n\r", ch );
	return FALSE;
    }

    if ( get_room_index( value ) )
    {
	editor_send_to_char(C_DEFAULT, "REdit:  Room vnum already exists.\n\r", ch );
	return FALSE;
    }

    pRoom			= new_room_index();
    pRoom->area			= pArea;
    pRoom->vnum			= value;

/*    if ( value > top_vnum_room )
	top_vnum_room = value;*/

    iHash			= value % MAX_KEY_HASH;
    pRoom->next			= room_index_hash[iHash];
    room_index_hash[iHash]	= pRoom;
    ch->desc->pEdit		= (void *)pRoom;

    editor_send_to_char(C_DEFAULT, "Room created.\n\r", ch );
    return TRUE;
}

bool redit_delet( CHAR_DATA *ch, char *argument )
{
   editor_send_to_char(AT_GREY, "If you want to DELETE the room, spell it out.\n\r", ch );
   return FALSE;
}

bool redit_delete( CHAR_DATA *ch, char *argument )
{
   ROOM_INDEX_DATA   *pRoom;
   ROOM_INDEX_DATA   *prev;
   ROOM_INDEX_DATA   *location;
   ROOM_INDEX_DATA   *pRoomIndex;  	/*used to delete exits*/
   OBJ_DATA  *obj;
   CHAR_DATA *victim;
   OBJ_DATA *obj_next;
   CHAR_DATA *vnext;
   int DIR;				/*used to delete exits*/
   int index;				/*used to delete exits*/

   EDIT_ROOM(ch, pRoom);

   editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);

   /* CLEAR THE ROOM */
       location = get_room_index( ROOM_VNUM_LIMBO );

       for ( victim = ch->in_room->people; victim; victim = vnext )
       {
	   vnext = victim->next_in_room;
	   if ( victim->deleted )
	       continue;

	   if ( IS_NPC( victim ) && ch != victim )
	       extract_char( victim, TRUE );
	   else
	   {
	     char_from_room( victim );
	     char_to_room( victim, location );
	     do_look( victim, "auto" );
	   }
       }

       for ( obj = ch->in_room->contents; obj; obj = obj_next )
       {
	   obj_next = obj->next_content;
	   if ( obj->deleted )
	       continue;
	   extract_obj( obj );
       }



   /* begin remove room from index */
   prev = room_index_hash[pRoom->vnum % MAX_KEY_HASH];

   if ( pRoom == prev )
   {
      room_index_hash[pRoom->vnum % MAX_KEY_HASH] = pRoom->next;
   }
   else
   {
      for ( ; prev; prev = prev->next )
      {
	 if ( prev->next == pRoom )
	 {
	    prev->next = pRoom->next;
	    break;
	 }
      }

      if ( !prev )
      {
	 bug( "redit_delete: room %d not found.",
	       pRoom->vnum );
      }
   }
   /* end remove room from index*/

   /* begin remove connecting exits */
   for ( index = 0; index < MAX_KEY_HASH; index++ )
   {
     if (room_index_hash[index])
     {
       for ( pRoomIndex = room_index_hash[index];
             pRoomIndex;
             pRoomIndex = pRoomIndex->next )
       {
         for ( DIR = 0; DIR <= DIR_MAX; DIR++ )
         {
           if ( pRoomIndex->exit[DIR] )
             if ( pRoomIndex->exit[DIR]->to_room == pRoom )
             {
               free_exit( pRoomIndex->exit[DIR] );
               pRoomIndex->exit[DIR] = NULL;
               SET_BIT(pRoomIndex->area->area_flags, AREA_CHANGED);
             }
         }
       }
     }
   }


   /* end of remove connecting exits */

   /* delete room */
   free_room_index( pRoom );

   ch->desc->pEdit = NULL;
   ch->desc->editor = 0;
   return TRUE;

}

bool redit_name( CHAR_DATA *ch, char *argument )
{
    ROOM_INDEX_DATA *pRoom;

    EDIT_ROOM(ch, pRoom);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  name [name]\n\r", ch );
	return FALSE;
    }

    free_string( pRoom->name );
    pRoom->name = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Name set.\n\r", ch );
    return TRUE;
}



bool redit_desc( CHAR_DATA *ch, char *argument )
{
    ROOM_INDEX_DATA *pRoom;

    EDIT_ROOM(ch, pRoom);

    if ( argument[0] == '\0' )
    {
	string_append( ch, &pRoom->description );
	return TRUE;
    }

    editor_send_to_char(C_DEFAULT, "Syntax:  desc\n\r", ch );
    return FALSE;
}




bool redit_format( CHAR_DATA *ch, char *argument )
{
    ROOM_INDEX_DATA *pRoom;

    EDIT_ROOM(ch, pRoom);

    /* OLC 1.1b */
    if ( strlen(pRoom->description) >= (MAX_STRING_LENGTH - 4) )
    {
	editor_send_to_char(C_DEFAULT, "String too long to be formatted.\n\r", ch );
	return FALSE;
    }

    pRoom->description = format_string( pRoom->description );

    editor_send_to_char(C_DEFAULT, "String formatted.\n\r", ch );
    return TRUE;
}



bool redit_mreset( CHAR_DATA *ch, char *argument )
{
    ROOM_INDEX_DATA	*pRoom;
    MOB_INDEX_DATA	*pMobIndex;
    CHAR_DATA		*newmob;
    char		arg [ MAX_INPUT_LENGTH ];

    RESET_DATA		*pReset;
    char		output [ MAX_STRING_LENGTH ];

    EDIT_ROOM(ch, pRoom);

    argument = one_argument( argument, arg );

    if ( arg[0] == '\0' || !is_number( arg ) )
    {
	editor_send_to_char (C_DEFAULT, "Syntax:  mreset <vnum> <max #>\n\r", ch );
	return FALSE;
    }

    if ( !( pMobIndex = get_mob_index( atoi( arg ) ) ) )
    {
	editor_send_to_char(C_DEFAULT, "REdit: No mobile has that vnum.\n\r", ch );
	return FALSE;
    }

    if ( pMobIndex->area != pRoom->area ) 
    {
	editor_send_to_char(C_DEFAULT, "REdit: No such mobile in this area.\n\r", ch );
	return FALSE;
    }

    /*
     * Create the mobile reset.
     */
    pReset = new_reset_data();
    pReset->command	= 'M';
    pReset->arg1	= pMobIndex->vnum;
    pReset->arg2	= is_number( argument ) ? atoi( argument ) : MAX_MOB;
    pReset->arg3	= pRoom->vnum;
    add_reset( pRoom, pReset, 0/* Last slot*/ );

    /*
     * Create the mobile.
     */
    newmob = create_mobile( pMobIndex );
    char_to_room( newmob, pRoom );

    sprintf( output, "%s (%d) has been loaded and added to resets.\n\r"
	"There will be a maximum of %d loaded to this room.\n\r",
	capitalize( pMobIndex->short_descr ),
	pMobIndex->vnum,
	pReset->arg2 );
    editor_send_to_char(C_DEFAULT, output, ch );
    act(C_DEFAULT, "$n has created $N!", ch, NULL, newmob, TO_ROOM );
    return TRUE;
}

struct wear_type
{
    int	wear_loc;
    int	wear_bit;
};



const struct wear_type wear_table[] =
{
    {	WEAR_NONE,		ITEM_TAKE		},
    {	WEAR_FINGER_L,	ITEM_WEAR_FINGER		},
    {	WEAR_FINGER_R,	ITEM_WEAR_FINGER		},
    {	WEAR_NECK,		ITEM_WEAR_NECK		},
    {	WEAR_BODY_1,	ITEM_WEAR_BODY  		},
    {	WEAR_BODY_2,	ITEM_WEAR_BODY  		},
    {	WEAR_BODY_3,	ITEM_WEAR_BODY  		},
    {	WEAR_HEAD,		ITEM_WEAR_HEAD		},
    { WEAR_IN_EYES,   	ITEM_WEAR_CONTACT       	},
    { WEAR_ORBIT,     	ITEM_WEAR_ORBIT         	},
    { WEAR_ON_FACE,   	ITEM_WEAR_FACE          	},
    {	WEAR_LEGS,		ITEM_WEAR_LEGS		},
    {	WEAR_FEET,		ITEM_WEAR_FEET		},
    {	WEAR_HANDS,		ITEM_WEAR_HANDS		},
    {	WEAR_ARMS,		ITEM_WEAR_ARMS		},
    {	WEAR_SHIELD,	ITEM_WEAR_SHIELD		},
    {	WEAR_ABOUT,		ITEM_WEAR_ABOUT		},
    {	WEAR_WAIST,		ITEM_WEAR_WAIST		},
    {	WEAR_WRIST_L,	ITEM_WEAR_WRIST			},
    {	WEAR_WRIST_R,	ITEM_WEAR_WRIST			},
    {	WEAR_WIELD,		ITEM_WIELD		},
    { 	WEAR_WIELD_2,   	ITEM_WIELD              },
    { 	WEAR_ANKLE_L,   	ITEM_WEAR_ANKLE         },
    { 	WEAR_ANKLE_R,   	ITEM_WEAR_ANKLE         },
    { 	WEAR_EAR_L,     	ITEM_WEAR_EARS          },
    { 	WEAR_EAR_R,    	ITEM_WEAR_EARS          	},
    {	WEAR_SHEATH_1,	ITEM_WEAR_SHEATH		},
    {	WEAR_SHEATH_2,	ITEM_WEAR_SHEATH		},
    {	NO_FLAG,		NO_FLAG			}
};



/*****************************************************************************
 Name:		wear_loc
 Purpose:	Returns the location of the bit that matches the count.
 		1 = first match, 2 = second match etc.
 Called by:	oedit_reset(olc_act.c).
 ****************************************************************************/
int wear_loc(int bits, int count)
{
    int flag;
 
    for (flag = 0; wear_table[flag].wear_bit != NO_FLAG; flag++)
    {
        if ( IS_SET(bits, wear_table[flag].wear_bit) && --count < 1)
            return wear_table[flag].wear_loc;
    }
 
    return NO_FLAG;
}



/*****************************************************************************
 Name:		wear_bit
 Purpose:	Converts a wear_loc into a bit.
 Called by:	redit_oreset(olc_act.c).
 ****************************************************************************/
int wear_bit(int loc)
{
    int flag;
 
    for (flag = 0; wear_table[flag].wear_loc != NO_FLAG; flag++)
    {
        if ( loc == wear_table[flag].wear_loc )
            return wear_table[flag].wear_bit;
    }
 
    return 0;
}



bool redit_oreset( CHAR_DATA *ch, char *argument )
{
    ROOM_INDEX_DATA	*pRoom;
    OBJ_INDEX_DATA	*pObjIndex;
    OBJ_DATA		*newobj;
    OBJ_DATA		*to_obj;
    CHAR_DATA		*to_mob;
    char		arg1 [ MAX_INPUT_LENGTH ];
    char		arg2 [ MAX_INPUT_LENGTH ];
    char		arg3 [ MAX_INPUT_LENGTH ];
    char		arg4 [ MAX_INPUT_LENGTH ];
    int			olevel = 0;
    int			amount;
    int			counter;

    RESET_DATA		*pReset;
    char		output [ MAX_STRING_LENGTH ];

    EDIT_ROOM(ch, pRoom);

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );
    argument = one_argument( argument, arg3 );
    argument = one_argument( argument, arg4 );


    if ( arg1[0] == '\0' || !is_number( arg1 ) )
    {
	editor_send_to_char (C_DEFAULT, "Syntax:  oreset # <vnum> <args>\n\r", ch );
	editor_send_to_char (C_DEFAULT, "        -no_args               = into room\n\r", ch );
	editor_send_to_char (C_DEFAULT, "        -<obj_name>            = into obj\n\r", ch );
	editor_send_to_char (C_DEFAULT, "        -<mob_name>            = into mob's eq\n\r", ch );
	editor_send_to_char (C_DEFAULT, "        -<mob_name> inventory  = into mob's inventory\n\r", ch );
	return FALSE;
    }

    if ( !is_number(arg1) )
    {
     editor_send_to_char(C_DEFAULT, "REdit: First argument is how many will be reset at a time.\n\r", ch );
     return FALSE;
    }

    amount = atoi(arg1);

    if ( amount < 1 )
    {
     editor_send_to_char(C_DEFAULT, "REdit: First argument must be over 0.\n\r", ch );
     return FALSE;
    }

    if ( !( pObjIndex = get_obj_index( atoi( arg2 ) ) ) )
    {
	editor_send_to_char(C_DEFAULT, "REdit: No object has that vnum.\n\r", ch );
	return FALSE;
    }

    if ( pObjIndex->area != pRoom->area ) 
    {
	editor_send_to_char(C_DEFAULT, "REdit: No such object in this area.\n\r", ch );
	return FALSE;
    }

    /*
     * Load into room.
     */
    if ( arg3[0] == '\0' )
    {
	pReset		= new_reset_data();
	pReset->command	= 'O';
	pReset->arg1	= pObjIndex->vnum;
	pReset->arg2	= amount;
	pReset->arg3	= pRoom->vnum;
	add_reset( pRoom, pReset, 0/* Last slot*/ );

       counter = amount;
       while( counter != 0 )
       {
	newobj = create_object( pObjIndex, pObjIndex->level ); /* Angi */
	obj_to_room( newobj, pRoom );
        counter -= 1;
       }

	sprintf( output, "%s (%d) has been loaded and added to resets.\n\r",
	    capitalize( pObjIndex->short_descr ),
	    pObjIndex->vnum );
	editor_send_to_char(C_DEFAULT, output, ch );
    }
    else
    /*
     * Load into object's inventory.
     */
    if ( arg4[0] == '\0'
    && ( ( to_obj = get_obj_list( ch, arg3, pRoom->contents ) ) != NULL ) )
    {
	pReset		= new_reset_data();
	pReset->command	= 'P';
	pReset->arg1	= pObjIndex->vnum;
	pReset->arg2	= amount;
	pReset->arg3	= to_obj->pIndexData->vnum;
	add_reset( pRoom, pReset, 0/* Last slot*/ );

       counter = amount;
       while( counter != 0 )
       {
	newobj = create_object( pObjIndex, number_fuzzy( olevel ) );
	newobj->cost.gold = newobj->cost.silver = newobj->cost.copper = 0;
	obj_to_obj( newobj, to_obj );
        counter -= 1;
       }

	newobj = create_object( pObjIndex, number_fuzzy( olevel ) );

	sprintf( output, "%s (%d) has been loaded into "
	    "%s (%d) and added to resets.\n\r",
	    capitalize( newobj->short_descr ),
	    newobj->pIndexData->vnum,
	    to_obj->short_descr,
	    to_obj->pIndexData->vnum );
	editor_send_to_char(C_DEFAULT, output, ch );
    }
    else
    /*
     * Load into mobile's inventory.
     */
    if ( ( to_mob = get_char_room( ch, arg3 ) ) != NULL )
    {
	/* Find specific reset to load AFTER */
	RESET_DATA *pMob;
	int	wearlocs = 0;
	int     reset_loc = 1;
	int     mob_num;
	int     counter = 0;
        bool inventory = FALSE;

	mob_num = number_argument( arg3, arg3 );
	for ( pMob = ch->in_room->reset_first; pMob; pMob = pMob->next )
	{
	  ++reset_loc;
	  if ( pMob->arg1 == to_mob->pIndexData->vnum && ++counter == mob_num )
	    break;
	}
	if ( !pMob )
	{
	  editor_send_to_char(C_DEFAULT, "Mobile not reset in this room.\n\r",ch);
	  return FALSE;
	}
	/* Load after all other worn/held items, but before next reset
	 * of any other type. */
	for ( pMob = pMob->next; pMob; pMob = pMob->next )
	{
	  ++reset_loc;
	  if ( pMob->command != 'G' && pMob->command != 'E' )
	    break;
	}
	if ( !pMob )
	  reset_loc = 0;

	/* load it to a spot in the equ if it has a take flag - Deck */
 	if ( !IS_SET( pObjIndex->wear_flags, ITEM_TAKE ) )
	{
 	    editor_send_to_char(C_DEFAULT, "REdit:  Object needs take flag.\n\r", ch );
	    return FALSE;
	}

        if ( !strcmp( arg4, "inventory" ) )
	 inventory = TRUE;

	/*
	 * Can't load into same position.
	 */
       if ( !inventory )
       {
	SET_BIT( wearlocs, pObjIndex->wear_flags );
        REMOVE_BIT( wearlocs, ITEM_TAKE);

	if ( get_eq_char( to_mob, wearlocs ) )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Object already equipped.\n\r", ch );
	    return FALSE;
	}
       }
       
	pReset		= new_reset_data();
	pReset->arg1	= pObjIndex->vnum;
	pReset->arg2	= amount;
	if ( inventory )
	    pReset->command = 'G';
	else
	    pReset->command = 'E';
	pReset->arg3	= 0;

	add_reset( pRoom, pReset, reset_loc );

       counter = amount;
       while ( counter != 0 )
       {
	olevel  = URANGE( 0, to_mob->level - 2, LEVEL_HERO );
        newobj = create_object( pObjIndex, number_fuzzy( olevel ) );

	if ( to_mob->pIndexData->pShop )	/* Shop-keeper? */
	{
	    switch ( pObjIndex->item_type )
	    {
	    default:		olevel = 0;				break;
	    case ITEM_PILL:	olevel = number_range(  0, 10 );	break;
	    case ITEM_POTION:	olevel = number_range(  0, 10 );	break;
	    case ITEM_SCROLL:	olevel = number_range(  5, 15 );	break;
	    case ITEM_WAND:	olevel = number_range( 10, 20 );	break;
            case ITEM_LENSE:    olevel = number_range( 10, 20 );        break;
	    case ITEM_STAFF:	olevel = number_range( 15, 25 );	break;
	    case ITEM_ARMOR:	olevel = number_range(  5, 15 );	break;
	    case ITEM_WEAPON:	if ( pReset->command == 'G' )
	    			    olevel = number_range( 5, 15 );
				else
				    olevel = number_fuzzy( olevel );
		break;
	    }

	    newobj = create_object( pObjIndex, olevel );
	       if ( inventory )
		SET_BIT( newobj->extra_flags, ITEM_INVENTORY );
	}
	else
	    newobj = create_object( pObjIndex, number_fuzzy( olevel ) );

	obj_to_char( newobj, to_mob );
	if ( pReset->command == 'E' )
	    equip_char( to_mob, newobj, FALSE );
        counter -= 1;
       }

	sprintf( output, "%s (%d) has been loaded "
	    "to %s (%d) and added to resets.\n\r",
	    capitalize( pObjIndex->short_descr ),
	    pObjIndex->vnum,
	    to_mob->short_descr,
	    to_mob->pIndexData->vnum );
	editor_send_to_char(C_DEFAULT, output, ch );
    }
    else	/* Display Syntax */
    {
	editor_send_to_char(C_DEFAULT, "REdit:  That mobile isn't here.\n\r", ch );
	return FALSE;
    }

    newobj = create_object( pObjIndex, number_fuzzy( olevel ) );

    act(C_DEFAULT, "$n has created $p!", ch, newobj, NULL, TO_ROOM );
    return TRUE;
}

/*
 * Randomize Exits.
 * Added by Altrag.
 */
bool redit_rreset( CHAR_DATA *ch, char *argument )
{
	static const char * dir_name[6] =
	{ "North\0", "East\0", "South\0", "West\0", "Up\0", "Down\0" };

	char arg[MAX_STRING_LENGTH];
	char output[MAX_STRING_LENGTH];
	RESET_DATA *pReset;
	ROOM_INDEX_DATA *pRoom;
	int direc;
	
	EDIT_ROOM(ch, pRoom);

	one_argument( argument, arg );
	if ( arg[0] == '\0' )
	{
		editor_send_to_char( C_DEFAULT, "Syntax: rreset <last-door>\n\r", ch );
		return FALSE;
	}
	
	if ( is_number( arg ) )
		direc = atoi(arg);
	else
	{
		for ( direc = 0; direc < 6; direc++ )
			if ( UPPER(arg[0]) == dir_name[direc][0] ) break;
	}

	if ( direc < 0 || direc > 5 )
	{
		editor_send_to_char( C_DEFAULT, "That is not a direction.\n\r", ch );
		return FALSE;
	}
	
	pReset = new_reset_data();
	pReset->command   = 'R';
	pReset->arg1      = pRoom->vnum;
	pReset->arg2      = direc;
	add_reset( pRoom, pReset, 0/* Last slot*/ );

	sprintf( output, "Exits North (0) to %s (%d) randomized.\n\r",
			 dir_name[direc], direc );
	editor_send_to_char( C_DEFAULT, output, ch );
	return TRUE;
}

/*
 * Object Editor Functions.
 */
bool set_invoke_type ( CHAR_DATA *ch, char *argument )
{    

    OBJ_INDEX_DATA *pObj;

    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(AT_WHITE, "Syntax:  invoke_type [type #]\n\r", ch );
	editor_send_to_char(AT_WHITE, "         type #1 = oload item.\n\r", ch );
	editor_send_to_char(AT_WHITE, "              #2 = mload mob.\n\r", ch );
	editor_send_to_char(AT_WHITE, "              #3 = transfer character.\n\r", ch );
	editor_send_to_char(AT_WHITE, "              #4 = item morph.\n\r", ch );
	editor_send_to_char(AT_WHITE, "              #5 = item cast spell.\n\r", ch );
	return FALSE;
    }
    if ( atoi( argument ) < -1 || atoi( argument ) > 5 )
	{
	set_invoke_type( ch, "" );
	return FALSE;
	}
    pObj->invoke_type = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Invoke type set.\n\r", ch);
    return TRUE;

}

bool set_invoke_vnum ( CHAR_DATA *ch, char *argument )
{    

    OBJ_INDEX_DATA *pObj;
    int value;
    char buf [MAX_STRING_LENGTH];
    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(AT_WHITE, "Syntax:  invoke_vnum [ # ]\n\r", ch );
	return FALSE;
    }
    value = atoi( argument );
    switch( pObj->invoke_type )
      {
      default: return FALSE;
      case 0: 
	editor_send_to_char( AT_GREY, "Obj invoke_type does not require a vnum.\n\r", ch );
	return FALSE;
      case 1:
	if ( !get_obj_index( value ) )
	  {
	  sprintf( buf, "No ojbect with vnum %d exists.\n\r", value );
	  editor_send_to_char( AT_GREY, buf, ch );
	  return FALSE;
	  }
	break;
      case 2:
	if ( !get_mob_index( value ) )
	  {
	  sprintf( buf, "No mobile with vnum %d exists.\n\r", value );
	  editor_send_to_char( AT_GREY, buf, ch );
	  return FALSE;
	  }
	break;
      case 3:
	if ( !get_room_index( value ) )
	  {
	  sprintf( buf, "Room vnum %d does not exist.\n\r", value );
	  editor_send_to_char( AT_GREY, buf, ch );
	  return FALSE;
	  }
	break;
      case 4:
	if ( !get_obj_index( value ) )
	  {
	  sprintf( buf, "No ojbect with vnum %d exists.\n\r", value );
	  editor_send_to_char( AT_GREY, buf, ch );
	  return FALSE;
	  }
      }
    pObj->invoke_vnum = value;
    sprintf( buf, "Invoke vnum set to: %d\n\r", value );
    editor_send_to_char(C_DEFAULT, buf, ch);
    return TRUE;

}

bool set_invoke_v1 ( CHAR_DATA *ch, char *argument )
{    

    OBJ_INDEX_DATA *pObj;

    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(AT_WHITE, "Syntax:  invoke_v1 [ current charges ]\n\r", ch );
	return FALSE;
    }

    pObj->invoke_charge[0] = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Current charge set.\n\r", ch);
    return TRUE;

}

bool set_invoke_v2 ( CHAR_DATA *ch, char *argument )
{    

    OBJ_INDEX_DATA *pObj;
    int value;
    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(AT_WHITE, "Syntax:  invoke_v2 [ max charges (-1 unlimited) ]\n\r", ch );
	return FALSE;
    }
    value = atoi( argument );
    if ( value < -1 ) value = -1;
    pObj->invoke_charge[1] = value;

    editor_send_to_char(C_DEFAULT, "Max Charge set.\n\r", ch);
    return TRUE;

}


bool set_invoke_setspell ( CHAR_DATA *ch, char *argument )
{    

    OBJ_INDEX_DATA *pObj;
    int            spn;
    
    EDIT_OBJ(ch, pObj);

    spn = skill_lookup( argument );
    
    if ( argument[0] == '\0'
    || spn == -1 
    || (*skill_table[spn].spell_fun) == (*spell_null) )
    {
	editor_send_to_char(AT_WHITE, "Syntax:  invoke_setspell [ valid spell name ]\n\r", ch );
	return FALSE;
    }

    free_string( pObj->invoke_spell );
    pObj->invoke_spell = str_dup(skill_table[spn].name);

    editor_send_to_char(C_DEFAULT, "Spell set.\n\r", ch);
    return TRUE;

}

bool set_obj_values( CHAR_DATA *ch, OBJ_INDEX_DATA *pObj, int value_num, char *argument)
{
    switch( pObj->item_type )
    {
	default:
	    break;

	case ITEM_PORTAL:
	    switch ( value_num )
	    {
	    default: return FALSE;
	    case 0:
	       editor_send_to_char(C_DEFAULT, "DESTINATION SET.\n\r\n\r", ch );
	       pObj->value[0] = atoi( argument );
	       break;
	    case 1:
	       editor_send_to_char(C_DEFAULT, "Suction level set.\n\r\n\r", ch );
	       pObj->value[1] = atoi( argument );
	       break;
	    }
	    break;

	case ITEM_WAND:
	case ITEM_LENSE:
	case ITEM_STAFF:
	    switch ( value_num )
	    {
		default:
		    do_help( ch, "'OEDIT STAFF'" );
		    return FALSE;
		case 0:
		    editor_send_to_char(C_DEFAULT, "SPELL LEVEL SET.\n\r\n\r", ch );
		    pObj->value[0] = atoi( argument );
		    break;
		case 1:
		    editor_send_to_char(C_DEFAULT, "TOTAL NUMBER OF CHARGES SET.\n\r\n\r", ch );
		    pObj->value[1] = atoi( argument );
		    break;
		case 2:
		    editor_send_to_char(C_DEFAULT, "CURRENT NUMBER OF CHARGES SET.\n\r\n\r", ch );
		    pObj->value[2] = atoi( argument );
		    break;
		case 3:
		    editor_send_to_char(C_DEFAULT, "SPELL TYPE SET.\n\r", ch );
		    pObj->value[3] = skill_lookup( argument );
		    break;
	    }
	    break;

	case ITEM_SCROLL:
	case ITEM_POTION:
	case ITEM_PILL:
	    switch ( value_num )
	    {
		default:
		    do_help( ch, "'OEDIT PILL'" );
	            return FALSE;
	        case 0:
	            editor_send_to_char(C_DEFAULT, "SPELL LEVEL SET.\n\r\n\r", ch );
	            pObj->value[0] = atoi( argument );
	            break;
	        case 1:
	            editor_send_to_char(C_DEFAULT, "SPELL TYPE 1 SET.\n\r\n\r", ch );
	            pObj->value[1] = skill_lookup( argument );
	            break;
	        case 2:
	            editor_send_to_char(C_DEFAULT, "SPELL TYPE 2 SET.\n\r\n\r", ch );
	            pObj->value[2] = skill_lookup( argument );
	            break;
	        case 3:
	            editor_send_to_char(C_DEFAULT, "SPELL TYPE 3 SET.\n\r\n\r", ch );
	            pObj->value[3] = skill_lookup( argument );
	            break;
	    }
	    break;

        case ITEM_WEAPON:
	    switch ( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT WEAPON'" );
	            return FALSE;
                case 7:
                   editor_send_to_char(C_DEFAULT, "Initial wield state.\n\r\n\r", ch );
                   pObj->value[7] = flag_value(blade_flags, argument );
                   break;
                case 8:
                    editor_send_to_char(C_DEFAULT, "Weapon Class Set.\n\r\n\r", ch );
                    pObj->value[8] = flag_value(weaponclass_flags, argument );
                    break;
	        case 3:
	            editor_send_to_char(C_DEFAULT, "Weapon Type Set.\n\r\n\r", ch );
	            pObj->value[3] = flag_value( weapon_flags, argument );
	            break;
	        case 4:
                    if ( atoi(argument) < 1 || atoi(argument) > 3 )
                    {
	     editor_send_to_char(C_DEFAULT, "Length is between 1 and 3.\n\r\n\r", ch );
	            return FALSE;
                    }
	            editor_send_to_char(C_DEFAULT, "Length Set.\n\r\n\r", ch );
	            pObj->value[4] = atoi( argument );
	            break;
	    }
            break;

        case ITEM_BULLET:
            switch( value_num )
            {
             default:
              do_help( ch, "HELP_OEDIT_BULLET" );
              return FALSE;
             case 0:
                    send_to_char(C_DEFAULT, "Damage type set.\n\r\n\r", ch );
	            pObj->value[0] = flag_value( weapon_flags, argument );
                    break;
             case 1:
                    send_to_char(C_DEFAULT, "Bullet calibur set.\n\r\n\r", ch );
                    pObj->value[1] = atoi( argument );
                    break;
             }
             break;

        case ITEM_ARROW:
            switch( value_num )
            {
             default:
              do_help( ch, "HELP_OEDIT_ARROW" );
              return FALSE;
             case 1:
              send_to_char(C_DEFAULT, "Arrow size set.\n\r\n\r", ch );
              pObj->value[1] = atoi( argument );
              break;
             case 2:
              send_to_char(C_DEFAULT, "Arrow type set.\n\r\n\r", ch );
	      pObj->value[2] = flag_value( arrow_types, argument );
              break;
             }
             break;

        case ITEM_CLIP:
            switch( value_num )
            {
             default:
              do_help( ch, "HELP_OEDIT_CLIP" );
              return FALSE;
             case 0:
              send_to_char(C_DEFAULT, "Clip capacity set.\n\r\n\r", ch );
              pObj->value[0] = atoi( argument );
              break;
             case 1:
              send_to_char(C_DEFAULT, "Bullet calibur set.\n\r\n\r", ch );
              pObj->value[1] = atoi( argument );
              break;
             case 2:
              send_to_char(C_DEFAULT, "Weapon type set.\n\r\n\r", ch );
	      pObj->value[2] = flag_value( ranged_weapon_flags, argument );
              break;
             }
             break;

        case ITEM_RANGED_WEAPON:
            switch( value_num )
            {
             default:
              do_help( ch, "HELP_OEDIT_RANGED_WEAPON" );
              return FALSE;
             case 0:
              send_to_char(C_DEFAULT, "Weapon type set.\n\r\n\r", ch );
	      pObj->value[0] = flag_value( ranged_weapon_flags, argument );
              break;
             case 4:
              send_to_char(C_DEFAULT, "Range set.\n\r\n\r", ch );
              pObj->value[4] = atoi( argument );
              break;
             case 1:
              if ( pObj->value[0] == RANGED_WEAPON_FIREARM )
              {
               send_to_char(C_DEFAULT, "Gun calibur set.\n\r\n\r", ch );
               pObj->value[1] = atoi( argument );
              }
              else
              if ( pObj->value[0] == RANGED_WEAPON_BOW )
              {
               send_to_char(C_DEFAULT, "Max arrow size set.\n\r\n\r", ch );
               pObj->value[1] = atoi( argument );
              }
              else
              if ( pObj->value[0] == RANGED_WEAPON_BAZOOKA )
              {
               send_to_char(C_DEFAULT, "Max warhead size set.\n\r\n\r", ch );
               pObj->value[1] = atoi( argument );
              }
              break;
             case 2:
              if ( pObj->value[0] == RANGED_WEAPON_FIREARM )
              {
               send_to_char(C_DEFAULT, "Clip vnum set.\n\r\n\r", ch );
               pObj->value[2] = atoi( argument );
              }
              else
              if ( pObj->value[0] == RANGED_WEAPON_BOW )
              {
               send_to_char(C_DEFAULT, "Bow test strength set.\n\r\n\r", ch );
               pObj->value[2] = atoi( argument );
              }
              break;
             case 3:
              if ( pObj->value[0] == RANGED_WEAPON_FIREARM )
              {
               send_to_char(C_DEFAULT, "Firing rate.\n\r\n\r", ch );
               pObj->value[3] = flag_value( gun_type_flags, argument );
              }
              break;
             }
             break;

	case ITEM_FURNITURE:
	    switch( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT FURNITURE'" );
		    return FALSE;
	        case 1:
	            editor_send_to_char(C_DEFAULT, "Furniture type SET.\n\r\n\r", ch );
	            pObj->value[1] = flag_value( furniture_flags, argument );
	            break;
	    }
	    break;

	case ITEM_SWITCH:
	    switch( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT SWITCH'" );
		    return FALSE;
	        case 0:
	            editor_send_to_char(C_DEFAULT, "Activation command set.\n\r\n\r", ch );
	            pObj->value[0] = flag_value( activation_commands, argument );
	            break;
	        case 1:
	            editor_send_to_char(C_DEFAULT, "Affect type set.\n\r\n\r", ch );
	            pObj->value[1] = flag_value( switch_affect_types, argument );
	            break;
	        case 2:
	            editor_send_to_char(C_DEFAULT, "Room vnum set.\n\r\n\r", ch );
	            pObj->value[2] = atoi(argument);
	            break;
	        case 3:
                 if ( pObj->value[1] == SWITCH_DOOR )
                 {
	            editor_send_to_char(C_DEFAULT, "Direction set.\n\r\n\r", ch );
	            pObj->value[3] = flag_value( direction_flags, argument );
                 }
                 else
                 {
	            editor_send_to_char(C_DEFAULT, "Thing vnum set.\n\r\n\r", ch );
	            pObj->value[3] = atoi(argument);
                 }
	            break;
	        case 4:
	            editor_send_to_char(C_DEFAULT, "Door Action set.\n\r\n\r", ch );
	            pObj->value[4] = flag_value( door_action_flags, argument );
	            break;
	    }
	    break;


	case ITEM_ARMOR:
	    switch( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT ARMOR'" );
		    return FALSE;
                case 1:
                    if ( !str_cmp( argument, "yes" ) )
		     pObj->value[1] = 1;
                    else if ( !str_cmp( argument, "no" ) )
		     pObj->value[1] = 0;
           	    else
		    {
	            editor_send_to_char(C_DEFAULT, "Yes or no.\n\r\n\r", ch );
		    return FALSE;
                    }
		    editor_send_to_char(C_DEFAULT, "Pocket status set.\n\r\n\r", ch );
		    break;
		case 2:
		    editor_send_to_char(C_DEFAULT, "Max pocket carry weight set.\n\r\n\r", ch );
		    pObj->value[2] = atoi( argument );
		    break;
	        case 4:
	            editor_send_to_char(C_DEFAULT, "Armor type SET.\n\r\n\r", ch );
	            pObj->value[4] = flag_value( armor_types, argument );
	            break;
                case 6:
                    editor_send_to_char(C_DEFAULT, "Weapon Type Set.\n\r\n\r", ch );
                    pObj->value[6] = flag_value(weaponclass_flags, argument );
                  break;                  
                case 7:
                    if ( atoi(argument) < 1 || atoi(argument) > 3 )
                    {
	     editor_send_to_char(C_DEFAULT, "Length is between 1 and 3.\n\r\n\r", ch );
	            return FALSE;
                    }

	            editor_send_to_char(C_DEFAULT, "Weapon Length SET.\n\r\n\r", ch );
	            pObj->value[7] = atoi( argument );
                  break;                  
                case 8:
	            editor_send_to_char(C_DEFAULT, "Weapon Vnum SET.\n\r\n\r", ch );
	            pObj->value[8] = atoi( argument );
                  break;                  
	    }
	    break;

	case ITEM_BOMB:
	    switch( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT BOMB'" );
		    return FALSE;
	        case 1:
	            editor_send_to_char(C_DEFAULT, "Warhead type SET.\n\r\n\r", ch );
	            pObj->value[1] = flag_value( warhead_flags, argument );
	            break;
	        case 2:
	            editor_send_to_char(C_DEFAULT, "Propulsion type SET.\n\r\n\r", ch );
	            pObj->value[2] = flag_value( propulsion_flags, argument );
	            break;
	        case 3:
	            editor_send_to_char(C_DEFAULT, "Payload SET.\n\r\n\r", ch );
	            pObj->value[3] = atoi(argument);
	            break;
	    }
	    break;

	case ITEM_GAS_CLOUD:
	    switch( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT GAS CLOUD'" );
		    return FALSE;
	        case 0:
	            editor_send_to_char(C_DEFAULT, "Initial timer SET.\n\r\n\r", ch );
	            pObj->value[0] = atoi(argument);
	            break;
	        case 1:
	            editor_send_to_char(C_DEFAULT, "Dam type SET.\n\r\n\r", ch );
	            pObj->value[1] = flag_value( damage_flags, argument );
	            break;
	        case 2:
	            editor_send_to_char(C_DEFAULT, "Dam amount SET.\n\r\n\r", ch );
	            pObj->value[2] = atoi(argument);
	            break;
	        case 3:
	            editor_send_to_char(C_DEFAULT, "Special affect SET.\n\r\n\r", ch );
	            pObj->value[3] = flag_value( gas_affects, argument );
	            break;
	    }
	    break;

	case ITEM_NOTEBOARD:
	    switch ( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT NOTEBOARD'" );
		    return FALSE;
		case 0:
		    editor_send_to_char(C_DEFAULT, "DECODER VNUM SET.\n\r\n\r", ch );
		    pObj->value[0] = atoi( argument );
		    break;
		case 1:
		    if ( atoi(argument) > get_trust(ch) )
		    {
		      editor_send_to_char(C_DEFAULT, "Limited by your trust.\n\r",ch);
		      return FALSE;
		    }
		    editor_send_to_char(C_DEFAULT, "MINIMUM READ LEVEL SET.\n\r\n\r",ch);
		    pObj->value[1] = atoi( argument );
		    break;
		case 2:
		    if ( atoi(argument) > get_trust(ch) )
		    {
		      editor_send_to_char(C_DEFAULT, "Limited by your trust.\n\r",ch);
		      return FALSE;
		    }
		    editor_send_to_char(C_DEFAULT, "MINIMUM WRITE LEVEL SET.\n\r\n\r",ch);
		    pObj->value[2] = atoi( argument );
		    break;
	    }
	    break;

        case ITEM_CONTAINER:
	    switch ( value_num )
	    {
		int value;
		
	        default:
		    do_help( ch, "'OEDIT CONTAINER'" );
	            return FALSE;
		case 0:
	            editor_send_to_char(C_DEFAULT, "WEIGHT CAPACITY SET.\n\r\n\r", ch );
	            pObj->value[0] = atoi( argument );
	            break;
		case 1:
		    if ( ( value = flag_value( container_flags, argument ) )
	              != NO_FLAG )
	        	TOGGLE_BIT(pObj->value[1], value);
		    else
		    {
			do_help ( ch, "'OEDIT CONTAINER'" );
			return FALSE;
		    }
	            editor_send_to_char(C_DEFAULT, "CONTAINER TYPE SET.\n\r\n\r", ch );
	            break;
		case 2:
		    if ( atoi(argument) != 0 )
		    {
			if ( !get_obj_index( atoi( argument ) ) )
			{
			    editor_send_to_char(C_DEFAULT, "THERE IS NO SUCH ITEM.\n\r\n\r", ch );
			    return FALSE;
			}

		    }
		    editor_send_to_char(C_DEFAULT, "CONTAINER KEY SET.\n\r\n\r", ch );
		    pObj->value[2] = atoi( argument );
		    break;
	    }
	    break;

	case ITEM_DRINK_CON:
	    switch ( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT DRINK-CONTAINER'" );
	            return FALSE;
	        case 0:
	            editor_send_to_char(C_DEFAULT, "MAXIMUM AMOUT OF LIQUID HOURS SET.\n\r\n\r", ch );
	            pObj->value[0] = atoi( argument );
		    break;
	        case 1:
	            editor_send_to_char(C_DEFAULT, "CURRENT AMOUNT OF LIQUID HOURS SET.\n\r\n\r", ch );
	            pObj->value[1] = atoi( argument );
	            break;
	        case 2:
	            editor_send_to_char(C_DEFAULT, "LIQUID TYPE SET.\n\r\n\r", ch );
	            pObj->value[2] = flag_value( liquid_flags, argument );
	            break;
	        case 3:
	            editor_send_to_char(C_DEFAULT, "POISON VALUE TOGGLED.\n\r\n\r", ch );
	            pObj->value[3] = ( pObj->value[3] == 0 ) ? 1 : 0;
	            break;
	    }
            break;

	case ITEM_FOOD:
	    switch ( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT FOOD'" );
	            return FALSE;
	        case 0:
		    editor_send_to_char(C_DEFAULT, "HOURS OF FOOD SET.\n\r\n\r", ch );
	            pObj->value[0] = atoi( argument );
	            break;
	        case 3:
	            editor_send_to_char(C_DEFAULT, "Condition set.\n\r\n\r", ch );
	            pObj->value[3] = flag_value( food_condition, argument );
	            break;
	    }
            break;

	case ITEM_MONEY:
	    switch ( value_num )
	    {
	        default:
		    do_help( ch, "'OEDIT MONEY'" );
	            return FALSE;
	        case 0:
	            editor_send_to_char(C_DEFAULT, "GOLD AMOUNT SET.\n\r\n\r", ch );
	            pObj->value[0] = atoi( argument );
		    pObj->cost.gold = pObj->value[0];
	            break;
		case 1:
		    editor_send_to_char(C_DEFAULT, "SILVER AMOUNT SET.\n\r\n\r", ch );
		    pObj->value[1] = atoi( argument );
		    pObj->cost.silver = pObj->value[1];
		    break;
		case 2:
		    editor_send_to_char(C_DEFAULT, "COPPER AMOUNT SET.\n\r\n\r", ch );
		    pObj->value[2] = atoi( argument );
		    pObj->cost.copper = pObj->value[2];
		    break;
	    }
            break;
    }

    show_obj_values( ch, create_object(pObj, 1), 0 );
    return TRUE;
}

bool oedit_show( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;
    OBJ_INDEX_DATA *pJoinObj;  /* Used to displaying join obj names -Deck */
    char buf[MAX_STRING_LENGTH];
    AFFECT_DATA *paf;
    int cnt;

    EDIT_OBJ(ch, pObj);

    sprintf( buf,
"&Y---------------------------&GGeneral Info&Y---------------------------\n\r"
);
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cArea&w:&z [&R%3d&z] &W%s ",
	!pObj->area ? -1        : pObj->area->vnum,
	!pObj->area ? "No Area" : pObj->area->name );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cName&w:&z [&W%s&z]\n\r", pObj->name );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cVnum&w:&z [&R%5d&z]  &cLevel&w:&z  [&R%3d&z] ",
        pObj->vnum, pObj->level );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cWeight&w:&z [&R%3d&z]\n\r", pObj->weight ); 
    editor_send_to_char(C_DEFAULT, buf, ch);

    sprintf( buf, "&cDurability&w:&z  [&W%d&z] ", pObj->durability );
    editor_send_to_char(C_DEFAULT, buf, ch );     

    sprintf( buf, "&cComposition&w:&z  [&W%s&z]\n\r",
	flag_string( object_materials, pObj->composition ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cGold Cost&w:&z [&R%d&z] &cSilver Cost&w:&z [&R%d&z] "
                  "&cCopper Cost&w:&z [&R%d&z]\n\r",
	pObj->cost.gold, pObj->cost.silver, pObj->cost.copper );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf,
"&Y-------------------------------&GFlags&Y------------------------------\n\r"
);
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cWear flags&w:&z  [&W%s&z]\n\r",
	flag_string( wear_flags, pObj->wear_flags ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cBionic flags&w:&z [&W%s&z]\n\r",
	flag_string( bionic_flags, pObj->bionic_flags ) );
    editor_send_to_char(C_DEFAULT, buf, ch );


    sprintf( buf, "&cExtra flags&w:&z [&W%s&z]\n\r",
	flag_string( extra_flags, pObj->extra_flags ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    if ( pObj->extra_descr )
    {
	EXTRA_DESCR_DATA *ed;

	editor_send_to_char(C_DEFAULT, "&cEx desc kwd&w: ", ch );

	for ( ed = pObj->extra_descr; ed; ed = ed->next )
	{
	    editor_send_to_char(AT_DGREY, "[", ch );
	    editor_send_to_char(AT_WHITE, ed->keyword, ch );
	    editor_send_to_char(AT_DGREY, "]", ch );
	}

	editor_send_to_char(AT_YELLOW, "\n\r", ch );
    }

    sprintf( buf, "&cLong desc&C| &cShort desc&w:&z  [&W%s&z]\n\r"
                  " &W%s\n\r",
	pObj->short_descr, pObj->description );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf,
"&Y-------------------------------&GExtra&Y------------------------------\n\r"
);
    editor_send_to_char(C_DEFAULT, buf, ch );

    if ( pObj->join )
    {
      pJoinObj = get_obj_index( pObj->join );
      sprintf( buf, "&cJoins to create&w: &z[&R%d&z] "
		    "[&W%s&z]\n\r", pObj->join, 
	     pJoinObj ? pJoinObj->short_descr : "&RError: No such object."  );
      editor_send_to_char( C_DEFAULT, buf, ch );
    }
    if ( pObj->sep_one )
    {
      pJoinObj = get_obj_index( pObj->sep_one );
      sprintf( buf, "&cFirst seperated vnum is&w:  &z[&R%d&z] "
		    "[&W%s&z]\n\r", pObj->sep_one, 
	     pJoinObj ? pJoinObj->short_descr : "&RError: No such object."  );
      editor_send_to_char( C_DEFAULT, buf, ch );
    }
    if ( pObj->sep_two )
    {
      pJoinObj = get_obj_index( pObj->sep_two );
      sprintf( buf, "&cSecond seperated vnum is&w:&z [&R%d&z] "
		    "[&W%s&z]\n\r", pObj->sep_two,
	     pJoinObj ? pJoinObj->short_descr : "&RError: No such object."  );
      editor_send_to_char( C_DEFAULT, buf, ch );
    }
    sprintf( buf, "&cInvoke Type&w:&z [&R%d&z] "
		  "&cInvoke Vnum&w:&z [&R%d&z] ", 
             pObj->invoke_type, pObj->invoke_vnum );
    editor_send_to_char(C_DEFAULT, buf, ch );
    if ( pObj->invoke_charge[1] == -1 )
        sprintf( buf, "&cInvoke is&w:   &WPERMANENT\n\r" );
    else
        sprintf( buf, "&cInvoke charges&w:&z[&R%d&w/&R%d&z]\n\r",
                 pObj->invoke_charge[0], pObj->invoke_charge[1] );
    editor_send_to_char( C_DEFAULT, buf, ch );
    if ( ( pObj->invoke_type == 5 ) && ( pObj->invoke_spell != '\0' ) )
        sprintf( buf, "&cInvoke Spell&w:&z[&W%s&z]\n\r",
		 pObj->invoke_spell );
    else sprintf( buf,"&cInvoke Spell&w:&z[&W!NONE!&z]\n\r" );
    editor_send_to_char( C_DEFAULT, buf, ch );

    for ( cnt = 0, paf = pObj->affected; paf; paf = paf->next )
    {
	if ( cnt == 0 )
	{
	editor_send_to_char(C_DEFAULT, "&cNumber Modifier     Affects\n\r", ch );
	editor_send_to_char(C_DEFAULT, "&z------ -------- ---------------\n\r", ch );
	}
	sprintf( buf, "&z[&R%4d&z] [&R%6d&z] &z[&W%-13s&z]\n\r", cnt,
	    paf->modifier,
	    flag_string( apply_flags, paf->location ) );
	editor_send_to_char(C_DEFAULT, buf, ch );
	cnt++;
    }

    sprintf( buf,
"&Y------------------------------&GValues&Y------------------------------\n\r"
);
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf,		  "&cType&w:&z        [&W%s&z]\n\r",
	flag_string( type_flags, pObj->item_type ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    show_obj_values( ch, create_object(pObj, 1), 0 );
    return FALSE;
}

bool oedit_duplicate( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA	*pObj;
    OBJ_INDEX_DATA	*pObj2; /* Object to copy */
    char arg[MAX_STRING_LENGTH];
    int vnum;

    EDIT_OBJ( ch, pObj );

    if ( argument[0] == '\0' )
    {
      editor_send_to_char(C_DEFAULT, "Syntax: duplicate <vnum> \n\r",ch);
      return FALSE;
    }

    argument = one_argument( argument, arg );

    if ( !is_number(arg) )
    {
      editor_send_to_char(C_DEFAULT, "REdit: You must enter a number (vnum).\n\r",ch);
      return FALSE;
    }
    else /* argument is a number */
    {
      vnum = atoi(arg);
      if( !( pObj2 = get_obj_index(vnum) ) )
      {
	editor_send_to_char(C_DEFAULT, "OEdit: That object does not exist.\n\r",ch);
	return FALSE;
      }
    }

   if ( (pObj2->name) )
   {
    free_string( pObj->name );
    pObj->name = str_dup( pObj2->name );
   }

   if ( (pObj2->short_descr) )
   {
    free_string( pObj->short_descr );
    pObj->short_descr = str_dup( pObj2->short_descr );
   }

   if ( (pObj2->description) )
   {
    free_string( pObj->description );
    pObj->description = str_dup( pObj2->description );
   }

    pObj->item_type = pObj2->item_type;
    pObj->extra_flags = pObj2->extra_flags;
    pObj->wear_flags = pObj2->wear_flags;
    pObj->bionic_flags = pObj2->bionic_flags;
    pObj->level = pObj2->level;
    pObj->durability = pObj2->durability;
    pObj->composition = pObj2->composition;

    pObj->value[0] = pObj2->value[0];
    pObj->value[1] = pObj2->value[1];
    pObj->value[2] = pObj2->value[2];
    pObj->value[3] = pObj2->value[3];
    pObj->value[4] = pObj2->value[4];
    pObj->value[5] = pObj2->value[5];
    pObj->value[6] = pObj2->value[6];
    pObj->value[7] = pObj2->value[7];
    pObj->value[8] = pObj2->value[8];
    pObj->value[9] = pObj2->value[9];

    pObj->weight = pObj2->weight;

    pObj->cost.gold = pObj2->cost.gold;
    pObj->cost.silver = pObj2->cost.silver;
    pObj->cost.copper = pObj2->cost.copper;

    pObj->invoke_vnum = pObj2->invoke_vnum;
    pObj->invoke_type = pObj2->invoke_type;
    pObj->invoke_charge[1] = pObj2->invoke_charge[1];
    pObj->invoke_charge[0] = pObj2->invoke_charge[0];

    free_string( pObj->invoke_spell );
    pObj->invoke_spell = str_dup( pObj2->invoke_spell );

    pObj->join = pObj2->join;
    pObj->sep_one = pObj2->sep_one;
    pObj->sep_two = pObj2->sep_two;

    pObj->affected = pObj2->affected;

    editor_send_to_char(C_DEFAULT, "Object info copied.\n\r", ch );
    return TRUE;
}

bool oedit_composition( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;
    int value;

    EDIT_OBJ(ch, pObj );

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax: composition [object material]\n\r", ch );
	return FALSE;
    }

    if ( ( value = flag_value( object_materials, argument ) ) == NO_FLAG )
    {
	editor_send_to_char(C_DEFAULT, "See ? materials for a list.\n\r", ch);
	return FALSE;
    }

    editor_send_to_char(C_DEFAULT, "Composition set.\n\r", ch );
    pObj->composition = value;

 return TRUE;
}

bool oedit_bionic( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;
    int value;

    EDIT_OBJ(ch, pObj );

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax: bionic [bionic flag]\n\r", ch );
	return FALSE;
    }

    if ( ( value = flag_value( bionic_flags, argument ) ) != NO_FLAG )
    {
	TOGGLE_BIT(pObj->bionic_flags, value);

	editor_send_to_char(C_DEFAULT, "Bionic flag toggled.\n\r", ch);
	return TRUE;
    }

    editor_send_to_char(C_DEFAULT, "Syntax: bionic [bionic flag]\n\r", ch );
    return FALSE;

 return FALSE;
}

/*
 * Need to issue warning if flag isn't valid.
 */
bool oedit_addaffect( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;
    AFFECT_DATA *pAf;
    char buf[MAX_STRING_LENGTH];
    char loc[MAX_STRING_LENGTH];
    char mod[MAX_STRING_LENGTH];

    EDIT_OBJ(ch, pObj);

    argument = one_argument( argument, loc );
    one_argument( argument, mod );

    if ( loc[0] == '\0' || mod[0] == '\0' || !is_number( mod ) )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  addaffect [location] [#mod]\n\r", ch );
	return FALSE;
    }
    if ( !str_cmp(
     flag_string( apply_flags, flag_value( apply_flags, loc ) ), "none" ) )
    {
     sprintf( buf, "Unknown affect %s.\n\r", loc );
     editor_send_to_char( AT_GREY, buf, ch );
     return FALSE;
    }

    pAf             =   new_affect();
    pAf->location   =   flag_value( apply_flags, loc );
    pAf->modifier   =   atoi( mod );
    pAf->type       =   -1;
    pAf->duration   =   -1;
    pAf->next       =   pObj->affected;
    pObj->affected  =   pAf;

    editor_send_to_char(C_DEFAULT, "Affect added.\n\r", ch);
    return TRUE;
}

/*
 * My thanks to Hans Hvidsten Birkeland and Noam Krendel(Walker)
 * for really teaching me how to manipulate pointers.
 */
bool oedit_delaffect( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;
    AFFECT_DATA *pAf;
    AFFECT_DATA *pAf_next;
    char affect[MAX_STRING_LENGTH];
    int  value;
    int  cnt = 0;

    EDIT_OBJ(ch, pObj);

    one_argument( argument, affect );

    if ( !is_number( affect ) || affect[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  delaffect [#affect]\n\r", ch );
	return FALSE;
    }

    value = atoi( affect );

    if ( value < 0 )
    {
	editor_send_to_char(C_DEFAULT, "Only non-negative affect-numbers allowed.\n\r", ch );
	return FALSE;
    }

    if ( !( pAf = pObj->affected ) )
    {
	editor_send_to_char(C_DEFAULT, "OEdit:  Non-existant affect.\n\r", ch );
	return FALSE;
    }

    if( value == 0 )	/* First case: Remove first affect */
    {
	pAf = pObj->affected;
	pObj->affected = pAf->next;
	free_affect( pAf );
    }
    else		/* Affect to remove is not the first */
    {
	while ( ( pAf_next = pAf->next ) && ( ++cnt < value ) )
	     pAf = pAf_next;

	if( pAf_next )		/* See if it's the next affect */
	{
	    pAf->next = pAf_next->next;
	    free_affect( pAf_next );
	}
	else                                 /* Doesn't exist */
	{
	     editor_send_to_char(C_DEFAULT, "No such affect.\n\r", ch );
	     return FALSE;
	}
    }

    editor_send_to_char(C_DEFAULT, "Affect removed.\n\r", ch);
    return TRUE;
}



bool oedit_name( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;

    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  name [string]\n\r", ch );
	return FALSE;
    }

    free_string( pObj->name );
    pObj->name = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Name set.\n\r", ch);
    return TRUE;
}

bool oedit_short( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;

    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  short [string]\n\r", ch );
	return FALSE;
    }

    free_string( pObj->short_descr );
    pObj->short_descr = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Short description set.\n\r", ch);
    return TRUE;
}



bool oedit_long( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;

    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  long [string]\n\r", ch );
	return FALSE;
    }

    free_string( pObj->description );
    pObj->description = str_dup( argument );
    pObj->description[0] = UPPER( pObj->description[0] );

    editor_send_to_char(C_DEFAULT, "Long description set.\n\r", ch);
    return TRUE;
}



bool set_value( CHAR_DATA *ch, OBJ_INDEX_DATA *pObj, char *argument, int value )
{
    if ( argument[0] == '\0' )
    {
	set_obj_values( ch, pObj, -1, '\0' );
	return FALSE;
    }

    if ( set_obj_values( ch, pObj, value, argument ) )
	return TRUE;

    return FALSE;
}

/* oedit_join is for setting the vnum of which an object can be joined to */
bool oedit_join( CHAR_DATA *ch, char *argument )
{
   OBJ_INDEX_DATA *pObj;
   char arg[MAX_STRING_LENGTH];
   int value = 0;

   EDIT_OBJ( ch, pObj );

   argument = one_argument( argument, arg );

   if ( arg[0] == '\0' || !is_number( arg ) )
   {
      editor_send_to_char( AT_WHITE, " &pSyntax: ojoin [vnum]\n\r", ch );
      return FALSE;
   }

   value = atoi( arg );

   if ( value < 0 || value > 33000 )
   {
      editor_send_to_char(AT_WHITE, "Invalid vnum.\n\r", ch );
      return FALSE;
   }

   pObj->join = value;
   editor_send_to_char( AT_WHITE, "Ok.\n\r", ch );
   return TRUE;
}

/* oedit_sepone is for setting the first vnum which an object can seperate
 * into.
 */
bool oedit_sepone( CHAR_DATA *ch, char *argument )
{
  OBJ_INDEX_DATA *pObj;
  char arg[MAX_STRING_LENGTH];
  int value = 0;

  EDIT_OBJ( ch, pObj );

  argument = one_argument( argument, arg );

  if ( arg[0] == '\0' || !is_number( arg ) )
  {
     editor_send_to_char( AT_WHITE, " &pSyntax:  osepone [vnum]\n\r", ch );
     return FALSE;
  }

  value = atoi( arg );
  if ( value < 0 || value > 33000 )
  {
    editor_send_to_char( AT_WHITE, "Invalid vnum.\n\r", ch );
    return FALSE;
  }
  pObj->sep_one = value;
  editor_send_to_char( AT_WHITE, "Ok.\n\r", ch );
  return TRUE;
}

/* oedit_septwo is for setting the second vnum which an object splits into */
bool oedit_septwo( CHAR_DATA *ch, char *argument )
{
  OBJ_INDEX_DATA *pObj;
  char arg[MAX_STRING_LENGTH];
  int value = 0;

  EDIT_OBJ( ch, pObj );

  argument = one_argument( argument, arg );

  if ( arg[0] == '\0' || !is_number( arg ) )
  {
     editor_send_to_char( AT_WHITE, " &pSyntax: oseptwo [vnum]\n\r", ch );
     return FALSE;
  }

  value = atoi( arg );
  if ( value < 0 || value > 33000 )
  {
    editor_send_to_char( AT_WHITE, "Invalid vnum.\n\r", ch );
    return FALSE;
  }
  pObj->sep_two = value;
  editor_send_to_char( AT_WHITE, "Ok.\n\r", ch );
  return TRUE;
}

/*****************************************************************************
 Name:		oedit_values
 Purpose:	Finds the object and sets its value.
 Called by:	The four valueX functions below.
 ****************************************************************************/
bool oedit_values( CHAR_DATA *ch, char *argument, int value )
{
    OBJ_INDEX_DATA *pObj;

    EDIT_OBJ(ch, pObj);

    if ( set_value( ch, pObj, argument, value ) )
	return TRUE;

    return FALSE;
}


bool oedit_value0( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 0 ) )
	return TRUE;

    return FALSE;
}



bool oedit_value1( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 1 ) )
	return TRUE;

    return FALSE;
}



bool oedit_value2( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 2 ) )
	return TRUE;

    return FALSE;
}



bool oedit_value3( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 3 ) )
	return TRUE;

    return FALSE;
}

bool oedit_value4( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 4 ) )
	return TRUE;

    return FALSE;
}
bool oedit_value5( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 5 ) )
	return TRUE;

    return FALSE;
}
bool oedit_value6( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 6 ) )
	return TRUE;

    return FALSE;
}
bool oedit_value7( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 7 ) )
	return TRUE;

    return FALSE;
}
bool oedit_value8( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 8 ) )
	return TRUE;

    return FALSE;
}
bool oedit_value9( CHAR_DATA *ch, char *argument )
{
    if ( oedit_values( ch, argument, 9 ) )
	return TRUE;

    return FALSE;
}


bool oedit_weight( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;

    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  weight [number]\n\r", ch );
	return FALSE;
    }

    pObj->weight = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Weight set.\n\r", ch);
    return TRUE;
}



bool oedit_cost( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;
    char arg  [ MAX_STRING_LENGTH ];
    char arg2 [ MAX_STRING_LENGTH ];
    EDIT_OBJ(ch, pObj);
 
    argument = one_argument( argument, arg );
    one_argument( argument, arg2 );
    
    if( is_number( arg2 ) )
    {
      	if( !str_cmp( arg, "gold" ) )
	{
          if ( atoi(arg2) > (pObj->level * 10) )
          {
	  editor_send_to_char( C_DEFAULT, "Gold cost can't be more than 10 times the level of the object.\n\r", ch );
	  return FALSE;
          }

	  pObj->cost.gold = atoi( arg2 );
	  editor_send_to_char( C_DEFAULT, "Gold cost set.\n\r", ch );
	}
	else if ( !str_cmp( arg, "silver" ) )
	{
          if ( atoi(arg2) > (pObj->level * 50) )
          {
	  editor_send_to_char( C_DEFAULT, "Silver cost can't be more than 50 times the level of the object.\n\r", ch );
	  return FALSE;
          }

	  pObj->cost.silver = atoi( arg2 );
	  editor_send_to_char( C_DEFAULT, "Silver cost set.\n\r", ch );
	}
	else if ( !str_cmp( arg, "copper" ) )
	{
          if ( atoi(arg2) > (pObj->level * 75) )
          {
	  editor_send_to_char( C_DEFAULT, "Silver cost can't be more than 75 times the level of the object.\n\r", ch );
	  return FALSE;
          }

	  pObj->cost.copper = atoi( arg2 );
	  editor_send_to_char( C_DEFAULT, "Copper cost set.\n\r", ch );
	}
  	else
	{
	  editor_send_to_char( C_DEFAULT, "Invalid currency type.\n\r", ch );
	  return FALSE;
	}
    }
    else
    {
  	editor_send_to_char( C_DEFAULT, "Invalid amount.\n\r", ch );
	return FALSE;
    }

    return TRUE;
}

bool oedit_level( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;

    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  level [number]\n\r", ch );
	return FALSE;
    }

    pObj->level = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Level set.\n\r", ch);
    return TRUE;
}

bool oedit_durability( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;

    EDIT_OBJ(ch, pObj);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  durability [number]\n\r", ch );
	return FALSE;
    }

    if ( atoi( argument ) < 0 || atoi( argument ) > 100 )
    {
	editor_send_to_char(C_DEFAULT, "Durability is between 0 and 100\n\r", ch );
	return FALSE;
    }
   

    pObj->durability = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Durability set.\n\r", ch);
    return TRUE;
}


bool oedit_create( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;
    AREA_DATA *pArea;
    int  value;
    int  iHash;

    value = atoi( argument );

    if ( argument[0] == '\0' )
    {
      pArea = ch->in_room->area;
      for ( value = pArea->lvnum; pArea->uvnum >= value; value++ )
      {
	if ( !get_obj_index( value ) )
	 break;
      }
      if ( value > pArea->uvnum )
      {
	editor_send_to_char( C_DEFAULT, "OEdit:  No free object vnums in this area.\n\r", ch );
	return FALSE;
      }
    }

    /* OLC 1.1b */

    if ( /*argument[0] == '\0' ||*/ value <= 0 || value >= INT_MAX )
    {
	char output[MAX_STRING_LENGTH];

	sprintf( output, "Syntax:  create [0 < vnum < %d]\n\r", INT_MAX );
	editor_send_to_char(C_DEFAULT, output, ch );
	return FALSE;
    }

    pArea = get_vnum_area( value );
    if ( !pArea )
    {
	editor_send_to_char(C_DEFAULT, "OEdit:  That vnum is not assigned an area.\n\r", ch );
	return FALSE;
    }

    if ( get_obj_index( value ) )
    {
	editor_send_to_char(C_DEFAULT, "OEdit:  Object vnum already exists.\n\r", ch );
	return FALSE;
    }

    pObj			= new_obj_index();
    pObj->vnum			= value;
    pObj->area			= pArea;
    free_string( pObj->invoke_spell );
    pObj->invoke_spell 		= str_dup(skill_table[0].name);


    iHash			= value % MAX_KEY_HASH;
    pObj->next			= obj_index_hash[iHash];
    obj_index_hash[iHash]	= pObj;
    ch->desc->pEdit		= (void *)pObj;

    editor_send_to_char(C_DEFAULT, "Object Created.\n\r", ch );
    return TRUE;
}

bool oedit_delet( CHAR_DATA *ch, char *argument )
{
   editor_send_to_char( AT_GREY, "If you want to DELETE this object, spell it out.\n\r", ch );
   return FALSE;
}

bool oedit_delete( CHAR_DATA *ch, char *argument )
{
   OBJ_INDEX_DATA   *pObj;
   OBJ_INDEX_DATA   *prev;

   EDIT_OBJ(ch, pObj);

   /* begin remove obj from index */
   prev = obj_index_hash[pObj->vnum % MAX_KEY_HASH];

   if ( pObj == prev )
   {
      obj_index_hash[pObj->vnum % MAX_KEY_HASH] = pObj->next;
   }
   else
   {
      for ( ; prev; prev = prev->next )
      {
	 if ( prev->next == pObj )
	 {
	    prev->next = pObj->next;
	    break;
	 }
      }

      if ( !prev )
      {
	 bug( "oedit_delete: obj %d not found.",
	       pObj->vnum );
      }
   }
   /* end remove obj from index*/

   /* delete obj */
   free_obj_index( pObj );

   ch->desc->pEdit = NULL;
   ch->desc->editor = 0;
   editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
   return TRUE;

}

bool oedit_ed( CHAR_DATA *ch, char *argument )
{
    OBJ_INDEX_DATA *pObj;
    EXTRA_DESCR_DATA *ed;
    char command[MAX_INPUT_LENGTH];

    EDIT_OBJ(ch, pObj);

    argument = one_argument( argument, command );

    if ( command[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  ed add [keyword]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         ed delete [keyword]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         ed edit [keyword]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         ed format [keyword]\n\r", ch );
	return FALSE;
    }

    if ( !str_cmp( command, "add" ) )
    {
	if ( /*keyword*/ argument[0] == '\0' )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  ed add [keyword]\n\r", ch );
	    return FALSE;
	}

	ed                  =   new_extra_descr();
	ed->keyword         =   str_dup( /*keyword*/ argument );
	ed->next            =   pObj->extra_descr;
	pObj->extra_descr   =   ed;

	string_append( ch, &ed->description );

	return TRUE;
    }

    if ( !str_cmp( command, "edit" ) )
    {
	if ( /*keyword*/ argument[0] == '\0' )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  ed edit [keyword]\n\r", ch );
	    return FALSE;
	}

	for ( ed = pObj->extra_descr; ed; ed = ed->next )
	{
	    if ( is_name( ch, /*keyword*/ argument, ed->keyword ) )
		break;
	}

	if ( !ed )
	{
	    editor_send_to_char(C_DEFAULT, "OEdit:  Extra description keyword not found.\n\r", ch );
	    return FALSE;
	}

	string_append( ch, &ed->description );

	return TRUE;
    }

    if ( !str_cmp( command, "delete" ) )
    {
	EXTRA_DESCR_DATA *ped = NULL;

	if ( /*keyword*/ argument[0] == '\0' )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  ed delete [keyword]\n\r", ch );
	    return FALSE;
	}

	for ( ed = pObj->extra_descr; ed; ed = ed->next )
	{
	    if ( is_name( ch, /*keyword*/ argument, ed->keyword ) )
		break;
	    ped = ed;
	}

	if ( !ed )
	{
	    editor_send_to_char(C_DEFAULT, "OEdit:  Extra description keyword not found.\n\r", ch );
	    return FALSE;
	}

	if ( !ped )
	    pObj->extra_descr = ed->next;
	else
	    ped->next = ed->next;

	free_extra_descr( ed );

	editor_send_to_char(C_DEFAULT, "Extra description deleted.\n\r", ch );
	return TRUE;
    }


    if ( !str_cmp( command, "format" ) )
    {
	EXTRA_DESCR_DATA *ped = NULL;

	if ( /*keyword*/ argument[0] == '\0' )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  ed format [keyword]\n\r", ch );
	    return FALSE;
	}

	for ( ed = pObj->extra_descr; ed; ed = ed->next )
	{
	    if ( is_name( ch, /*keyword*/ argument, ed->keyword ) )
		break;
	    ped = ed;
	}

	if ( !ed )
	{
                editor_send_to_char(C_DEFAULT, "OEdit:  Extra description keyword not found.\n\r", ch );
                return FALSE;
	}

	/* OLC 1.1b */
	if ( strlen(ed->description) >= (MAX_STRING_LENGTH - 4) )
	{
	    editor_send_to_char(C_DEFAULT, "String too long to be formatted.\n\r", ch );
	    return FALSE;
	}

	ed->description = format_string( ed->description );

	editor_send_to_char(C_DEFAULT, "Extra description formatted.\n\r", ch );
	return TRUE;
    }

    oedit_ed( ch, "" );
    return FALSE;
}


/*
 * Clan Editor Functions.
 */
bool cedit_show( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;
    char buf[MAX_STRING_LENGTH];
    OBJ_INDEX_DATA *pObjIndex;

    EDIT_CLAN(ch, pClan);

    sprintf( buf, "&PName:&Y        [%s&w] &PInitials:&Y [%s&w]\n\r"
                  "&PWarden:&Y       [%s]\n\r",
	pClan->name, pClan->init, pClan->warden );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&PClan:&Y        [%3d]\n\r",
	pClan->vnum );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&PRecall:&Y      [%5d] %s\n\r", pClan->recall,
	get_room_index( pClan->recall )
	? get_room_index( pClan->recall )->name : "none" );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&PCivil Pkill:&Y [%3s]\n\r",
             IS_SET(pClan->settings, CLAN_CIVIL_PKILL) ? "YES" : "NO" );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&PPkill:&Y       [%3s]\n\r",
 	     IS_SET(pClan->settings, CLAN_PKILL) ? "YES" : "NO" );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&PMembers:&Y     [%8d]\n\r", pClan->members );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&PPkills:&Y      [%8d]\n\r", pClan->pkills  );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&PPkilled:&Y     [%8d]\n\r", pClan->pdeaths );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&PMkills:&Y      [%8d]\n\r", pClan->mkills  );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&PLeader:&Y      [%10s]&Y%s&R%s\n\r",
	     pClan->leader,
             IS_SET(pClan->settings, CLAN_LEADER_INDUCT) ? " Can induct." : "",
             pClan->isleader ? "" : " Position open." );
    editor_send_to_char( C_DEFAULT, buf, ch );
    sprintf( buf, "&PCouncil:&Y     [%10s]&Y%s&R%s\n\r",
	     pClan->first,
             IS_SET(pClan->settings, CLAN_FIRST_INDUCT) ? " Can induct." : "",
	     pClan->isfirst ? "" : " Position open." );
    editor_send_to_char( C_DEFAULT, buf, ch );
    sprintf( buf, "&PCenturion:&Y   [%10s]&Y%s&R%s\n\r", 
	     pClan->second,
             IS_SET(pClan->settings, CLAN_SECOND_INDUCT) ? " Can induct." : "",
             pClan->issecond ? "" : " Position open." );
    editor_send_to_char( C_DEFAULT, buf, ch );
    sprintf( buf, "&PChampion:&Y    [%10s]&Y%s&R%s\n\r",
	     pClan->champ,
             IS_SET(pClan->settings, CLAN_CHAMP_INDUCT) ? " Can induct." : "",
	     pClan->ischamp ? "" : " Position open." );
    editor_send_to_char( C_DEFAULT, buf, ch );  
/*    sprintf( buf,
        "Object:      [%5d], [%5d], [%5d]\n\r",
        pClan->obj_vnum_1, pClan->obj_vnum_2, pClan->obj_vnum_3  ); */
    sprintf ( buf, "&PObjects:\n\r" );
    editor_send_to_char(C_DEFAULT, buf, ch);
    if IS_SET(pClan->settings,CLAN_PKILL)
    {
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_1 )))
      sprintf( buf, "&YLevel 50:&w  [%5d] %s\n\r",
               pClan->obj_vnum_1, pObjIndex->short_descr );
     else
      sprintf( buf, "&YLevel 50:&w  [%5d] &RNo such object!!\n\r",
               pClan->obj_vnum_1);
     editor_send_to_char(C_DEFAULT, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_2 )))
      sprintf( buf, "&YLevel 75:&w  [%5d] %s\n\r",
               pClan->obj_vnum_2, pObjIndex->short_descr );
     else
      sprintf( buf, "&YLevel 75:&w  [%5d] &RNo such object!!\n\r",
               pClan->obj_vnum_1);
     editor_send_to_char(C_DEFAULT, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_3 )))
      sprintf( buf, "&YLevel 100:&w [%5d] %s\n\r",
               pClan->obj_vnum_3, pObjIndex->short_descr );
     else
      sprintf( buf, "&YLevel 100:&w [%5d] &RNo such object!!\n\r",
               pClan->obj_vnum_1);
     editor_send_to_char(C_DEFAULT, buf, ch );
    }
    else
    {
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_1 )))
      sprintf( buf, "&YLevel 30:&w  [%5d] %s\n\r",
               pClan->obj_vnum_1, pObjIndex->short_descr );
     else
      sprintf( buf, "&YLevel 30:&w  [%5d] &RNo such object!!\n\r",
               pClan->obj_vnum_1);
     editor_send_to_char(C_DEFAULT, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_2 )))
      sprintf( buf, "&YLevel 65:&w  [%5d] %s\n\r",
               pClan->obj_vnum_2, pObjIndex->short_descr );
     else
      sprintf( buf, "&YLevel 65:&w  [%5d] &RNo such object!!\n\r",
               pClan->obj_vnum_1);
     editor_send_to_char(C_DEFAULT, buf, ch );
     if ((pObjIndex = get_obj_index ( pClan->obj_vnum_3 )))
      sprintf( buf, "&YLevel 100:&w [%5d] %s\n\r",
               pClan->obj_vnum_3, pObjIndex->short_descr );
     else
      sprintf( buf, "&YLevel 100:&w [%5d] &RNo such object!!\n\r",
               pClan->obj_vnum_1);
     editor_send_to_char(C_DEFAULT, buf, ch );
    }

    sprintf( buf, "&PDescription:\n\r%s\n\r", pClan->description );
    editor_send_to_char(C_DEFAULT, buf, ch );
    
    return FALSE;
}



bool cedit_create( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;
    int  value;

    value = atoi( argument );

    /* OLC 1.1b */
    if ( argument[0] == '\0' || value <= 0 || value >= MAX_CLAN )
    {
	char output[MAX_STRING_LENGTH];

	sprintf( output, "Syntax:  cedit create [1 < vnum < %d]\n\r",
		 MAX_CLAN );
	editor_send_to_char(C_DEFAULT, output, ch );
	return FALSE;
    }


    if ( get_clan_index( value ) )
    {
	editor_send_to_char(C_DEFAULT, "CEdit:  Clan vnum already exists.\n\r", ch );
	return FALSE;
    }

    pClan			= new_clan_index();
    pClan->vnum			= value;
    clan_sort(pClan);
    ch->desc->pEdit		= (void *)pClan;

    editor_send_to_char(C_DEFAULT, "Clan Created.\n\r", ch );
    return TRUE;
}

bool cedit_members( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(AT_WHITE, "Syntax: &Bmembers [number]\n\r", ch );
	return FALSE;
    }

    pClan->members = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Members set.\n\r", ch);
    return TRUE;
}

bool cedit_pkills( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(AT_WHITE, "Syntax: &Bpkills [number]\n\r", ch );
	return FALSE;
    }

    pClan->pkills = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Pkills set.\n\r", ch);
    return TRUE;
}

bool cedit_pkilled( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(AT_WHITE, "Syntax: &Bpkilled [number]\n\r", ch );
	return FALSE;
    }

    pClan->pdeaths = atoi( argument );

    editor_send_to_char(C_DEFAULT, "PKilled set.\n\r", ch);
    return TRUE;
}

bool cedit_mkills( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(AT_WHITE, "Syntax: &Bmkills [number]\n\r", ch );
	return FALSE;
    }

    pClan->mkills = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Mkills set.\n\r", ch);
    return TRUE;
}

bool cedit_name( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  name [string]\n\r", ch );
	return FALSE;
    }

    free_string( pClan->name );
    pClan->name = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Name set.\n\r", ch);
    return TRUE;
}

bool cedit_init( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  init [string]\n\r", ch );
	return FALSE;
    }

    free_string( pClan->name );
    pClan->init = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Init set.\n\r", ch);
    return TRUE;
}

bool cedit_warden( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  warden [string]\n\r", ch );
	return FALSE;
    }

    free_string( pClan->warden );
    pClan->warden = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Warden set.\n\r", ch);
    return TRUE;
}

bool cedit_civil( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    TOGGLE_BIT (pClan->settings, CLAN_CIVIL_PKILL);

    if IS_SET( pClan->settings, CLAN_CIVIL_PKILL)
	editor_send_to_char(C_DEFAULT, "Clan switched to Civil Pkill.\n\r", ch);
    else
	editor_send_to_char(C_DEFAULT, "Clan switched to No Civil Pkill.\n\r", ch);

    return TRUE;
}


bool cedit_pkill( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    TOGGLE_BIT (pClan->settings, CLAN_PKILL);

    if IS_SET( pClan->settings, CLAN_PKILL)
	editor_send_to_char(C_DEFAULT, "Clan switched to Pkill.\n\r", ch);
    else
	editor_send_to_char(C_DEFAULT, "Clan switched to Peace.\n\r", ch);

    return TRUE;
}

bool cedit_object( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;
    char number[MAX_INPUT_LENGTH];
    char onum[MAX_INPUT_LENGTH];
    int  value;
    int  vnum;

    argument = one_argument( argument, number );
    argument = one_argument( argument, onum );

    EDIT_CLAN(ch, pClan);

    if ( number[0] == '\0' )
    {
     editor_send_to_char( C_DEFAULT, "Syntax: object <1/2/3> <vnum>\n\r", ch );
     return FALSE;
    }

    value = atoi( number );
    vnum  = atoi( onum );

    if ( ( value < 1 ) || ( value > 3 ) )
       return FALSE;

    switch ( value )
    {
     case 1:
	pClan->obj_vnum_1 = vnum;
	break;
     case 2:
	pClan->obj_vnum_2 = vnum;
	break;
     case 3:
	pClan->obj_vnum_3 = vnum;
	break;
     default:
	break;
    }

    editor_send_to_char( C_DEFAULT, "Object vnum set.\n\r", ch );
    return TRUE;
}

bool cedit_power( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;
    char number[MAX_INPUT_LENGTH];
    char onum[MAX_INPUT_LENGTH];
    int  value;

    argument = one_argument( argument, number );
    argument = one_argument( argument, onum );

    EDIT_CLAN(ch, pClan);

    if ( number[0] == '\0' )
    {
     editor_send_to_char( C_DEFAULT, "Syntax: power <1/2/3/4> <name>\n\r", ch );
     editor_send_to_char(AT_WHITE, "Valid Levels are as follows\n\r", ch );
     editor_send_to_char(AT_WHITE, "       1 -> Centurion of Clan.\n\r", ch );
     editor_send_to_char(AT_WHITE, "       2 -> Council of Clan.\n\r", ch );
     editor_send_to_char(AT_WHITE, "       3 -> Leader of Clan.\n\r", ch );
     editor_send_to_char(AT_WHITE, "       4 -> Champion of Clan.\n\r", ch );
     editor_send_to_char(AT_WHITE, "To clear a position, in the <name> field type: none", ch );
     return FALSE;
    }

    value = atoi( number );

    if ( ( value < 1 ) || ( value > 4 ) )
       return FALSE;

    switch ( value )
    {
     case 4:
        free_string( pClan->champ );
	if (!str_cmp(onum, "none" ))
	{
	 pClan->champ = str_dup( "none" );
	 pClan->ischamp = FALSE;
	}
	else
	{
	 pClan->champ = str_dup( onum );
	 pClan->ischamp = TRUE;
	}
	break;
     case 3:
        free_string( pClan->leader );
	if (!str_cmp(onum, "none" ))
	{
	 pClan->leader = str_dup( "none" );
	 pClan->isleader = FALSE;
	}
	else
	{
	 pClan->leader = str_dup( onum );
	 pClan->isleader = TRUE;
	}
	break;
     case 2:
        free_string( pClan->first );
	if (!str_cmp(onum, "none" ))
	{
	 pClan->first = str_dup( "none" );
	 pClan->isfirst = FALSE;
	}
	else
	{
	 pClan->first = str_dup( onum );
	 pClan->isfirst = TRUE;
	}
	break;
     case 1:
        free_string( pClan->second );
	if (!str_cmp(onum, "none" ))
	{
	 pClan->second = str_dup( "none" );
	 pClan->issecond = FALSE;
	}
	else
	{
	 pClan->second = str_dup( onum );
	 pClan->issecond = TRUE;
	}
	break;
     default:
	break;
    }

    editor_send_to_char( C_DEFAULT, "Power seat set.\n\r", ch );
    return TRUE;
}

bool cedit_induct ( CHAR_DATA *ch, char *argument )
{
     CLAN_DATA *pClan;
     int       value;

     EDIT_CLAN(ch, pClan);

     value = atoi(argument);

     if ( ( argument[0] == '\0' ) || ( ( value < 1 ) || ( value > 4 ) ) )
     {
      editor_send_to_char(C_DEFAULT, "Sets which clan position can induct.", ch );
      editor_send_to_char(C_DEFAULT, "Syntax: induct <1/2/3/4>\n\r", ch );
      editor_send_to_char(AT_WHITE, "Valid positions are as follows\n\r", ch );
      editor_send_to_char(AT_WHITE, "       1 -> Centurion of Clan.\n\r", ch );
      editor_send_to_char(AT_WHITE, "       2 -> Council of Clan.\n\r", ch );
      editor_send_to_char(AT_WHITE, "       3 -> Leader of Clan.\n\r", ch );
      editor_send_to_char(AT_WHITE, "       4 -> Champion of Clan.\n\r", ch );
      return FALSE;
     }

     switch ( value )
     {
      case 4: TOGGLE_BIT(pClan->settings, CLAN_CHAMP_INDUCT  ); break;
      case 3: TOGGLE_BIT(pClan->settings, CLAN_LEADER_INDUCT ); break;
      case 2: TOGGLE_BIT(pClan->settings, CLAN_FIRST_INDUCT  ); break;
      case 1: TOGGLE_BIT(pClan->settings, CLAN_SECOND_INDUCT ); break;
      default: break;
     }

     return TRUE;
}

bool cedit_clist( CHAR_DATA *ch, char *argument )
{
  return TRUE;
}

bool cedit_desc( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

    EDIT_CLAN(ch, pClan);

    if ( argument[0] == '\0' )
    {
	string_append( ch, &pClan->description );
	return TRUE;
    }

    editor_send_to_char(C_DEFAULT, "Syntax:  desc    - line edit\n\r", ch );
    return FALSE;
}

void do_clandesc ( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;

   if ( (pClan=get_clan_index(ch->clan)) != NULL )
   {
    string_append( ch, &pClan->description );
    save_clans( );
   }
}

bool cedit_recall( CHAR_DATA *ch, char *argument )
{
    CLAN_DATA *pClan;
    char room[MAX_STRING_LENGTH];
    int  value;

    EDIT_CLAN(ch, pClan);

    one_argument( argument, room );

    if ( !is_number( argument ) || argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  recall [#rvnum]\n\r", ch );
	return FALSE;
    }

    value = atoi( room );

    if ( !get_room_index( value ) )
    {
	editor_send_to_char(C_DEFAULT, "CEdit:  Room vnum does not exist.\n\r", ch );
	return FALSE;
    }

    pClan->recall = value;

    editor_send_to_char(C_DEFAULT, "Recall set.\n\r", ch );
    return TRUE;
}
  
/*
 * Mobile Editor Functions.
 */
bool medit_show( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    AFFECT_DATA *paf;
    char buf[MAX_STRING_LENGTH];
    int  cnt = 0;

    EDIT_MOB(ch, pMob);

    sprintf( buf,
"&Y---------------------------&GGeneral Info&Y---------------------------\n\r"
);
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cVnum&w:&z  [&R%5d&z] &cArea&w:&z [&R%5d&z] &W%s\n\r",
	pMob->vnum,
	!pMob->area ? -1        : pMob->area->vnum,
	!pMob->area ? "No Area" : pMob->area->name );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cSex&w:&z   [&W%5s&z] &cName&w:&z [&W%s&z]\n\r",
	pMob->sex == SEX_MALE    ? " male"   :
	pMob->sex == SEX_FEMALE  ? "femme" : "neutr", pMob->player_name );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cRace_type&w:&z [&W%s&z] ",
	flag_string( race_type_flags, pMob->race_type ) ); 
    editor_send_to_char(C_DEFAULT, buf, ch );   

    sprintf( buf, "&cFighting style&w:&z [&W%s&z]\n\r",
	flag_string( fighting_styles, pMob->fighting_style ) ); 
    editor_send_to_char(C_DEFAULT, buf, ch );   

    if ( pMob->spec_fun )
    {
	sprintf( buf, "&cSpec fun&w:&z      [&W%s&z]\n\r",
	spec_string(pMob->spec_fun ) );
	editor_send_to_char(C_DEFAULT, buf, ch );
    }

    sprintf( buf, "&cAlly mob's vnum&w:&z [&W%d&z] ", pMob->ally_vnum ); 
    editor_send_to_char(C_DEFAULT, buf, ch );   

    sprintf( buf, "&cAlly mob's level&w:&z [&W%d&z]\n\r",
     pMob->ally_level );
    editor_send_to_char(C_DEFAULT, buf, ch );   

    sprintf( buf,
"&Y-------------------------------&GStats&Y------------------------------\n\r"
);
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf,
	"&cLevel&w:&z [&R%5d&z] &cAlign&w:&z  [&R%4d&z] ",
	pMob->level,       pMob->alignment );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf,
        "&cHit Points&w:&z [&R%s&z]\n\r",
        flag_string( mobhp_flags ,pMob->hit_modi ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

   sprintf( buf,
      "&PStr&w:&z [&R%2d&z] &PDex&w:&z [&R%2d&z] &PAgi&w:&z [&R%2d&z] &cSize&w:&z       [&R%s&z]\n\r"
      "&PInt&w:&z [&R%2d&z] &PCon&w:&z [&R%2d&z] "
      "&PWis&w:&z [&R%2d&z] ",
        pMob->perm_str, pMob->perm_dex, pMob->perm_agi,
	flag_string( size_flags, pMob->size ), 
        pMob->perm_int, pMob->perm_con, pMob->perm_wis );
    editor_send_to_char(C_DEFAULT, buf, ch);

   sprintf( buf,
      "&cSkin tough&w:&z [&R%s&z]\n\r",
        flag_string( skin_flags, pMob->skin) );
    editor_send_to_char(C_DEFAULT, buf, ch);

    sprintf( buf,
      "&PCharisma&w:&z [&R%2d&z]                "
      "&cAdded HR&w:&z   [&R%3d&z]\n\r",
        pMob->perm_cha, pMob->hitroll );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf,
	"&gPhysical dampener&w:&z [&R%3d&z] &gMagic dampener&w:&z  [&R%3d&z]\n\r",
	pMob->p_damp,       pMob->m_damp );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf,
      "&YGold&w:&z [&R%5d&z]  &WSilver&w:&z [&R%5d&z] &OCopper&w:&z [&R%5d&z]\n\r",
        pMob->money.gold, pMob->money.silver, pMob->money.copper );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf,
"&Y-------------------------------&GFlags&Y------------------------------\n\r"
);
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cAct&w:&z           [&W%s&z]\n\r",
	flag_string( act_flags, pMob->act ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cAffected by&w:&z   [&W%s&z]\n\r",
	flag_string( affect_flags, pMob->affected_by ) );
    editor_send_to_char(C_DEFAULT, buf, ch );
    sprintf( buf, "&cAffected by 2&w:&z [&W%s&z]\n\r",
        flag_string( affect2_flags, pMob->affected_by2 ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&gImmune to&w:&z     [&W%s&z]\n\r",
	flag_string( immune_flags, pMob->imm_flags ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&gResistant to&w:&z  [&W%s&z]\n\r",
      flag_string( immune_flags, pMob->res_flags )   );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&gVulnerable to&w:&z [&W%s&z]\n\r",
      flag_string( immune_flags, pMob->vul_flags ) );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf,
"&Y-------------------------------&GDescs&Y------------------------------\n\r"
);
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&clong_desc| &cShort descr&w:&z   [&W%s&z]\n\r"
		  " &W%s",
	pMob->short_descr, pMob->long_descr );
    editor_send_to_char(C_DEFAULT, buf, ch );

    sprintf( buf, "&cDescription&w:\n\r &W%s", pMob->description );
    editor_send_to_char(C_DEFAULT, buf, ch );

    if ( pMob->pTattoo )
    {

    TATTOO_ARTIST_DATA *pTattoo = pMob->pTattoo;
    

        sprintf( buf,
          "&CTattoo artistry data:\n\r"
          "&cMagical Booster&w:&z [&R%s&z] ",
  	   flag_string( tattoo_flags, pMob->pTattoo->magic_boost ) );
	editor_send_to_char(C_DEFAULT, buf, ch );

        sprintf( buf,
          "&cWear Location&w:&z [&R%s&z]\n\r"
          "&cGold Cost&w:&z       [&R%d&z] "
          "   &cSilver Cost&w:&z [&R%d&z] "
          "&cCopper Cost&w:&z [&R%d&z]\n\r",
  	   flag_string( wear_tattoo, pMob->pTattoo->wear_loc ),
           pMob->pTattoo->cost.gold, pMob->pTattoo->cost.silver,
           pMob->pTattoo->cost.copper );
	editor_send_to_char(C_DEFAULT, buf, ch );


   for ( cnt = 0, paf = pTattoo->affected; paf; paf = paf->next )
    {
	if ( cnt == 0 )
	{
	editor_send_to_char(C_DEFAULT, "&cNumber Modifier     Affects\n\r", ch );
	editor_send_to_char(C_DEFAULT, "&z------ -------- ---------------\n\r", ch );
	}
	sprintf( buf, "&z[&R%4d&z] [&R%6d&z] &z[&W%-13s&z]\n\r", cnt,
	    paf->modifier,
	    flag_string( apply_flags, paf->location ) );
	editor_send_to_char(C_DEFAULT, buf, ch );
	cnt++;
    }
   }


    if ( pMob->pCasino )
    {    
        sprintf( buf,
          "&GCasino Dealer data:\n\r"
          " &cGame type&w:&z [&R%s&z]\n\r",
  	   flag_string( casino_games, pMob->pCasino->game ) );
	editor_send_to_char(C_DEFAULT, buf, ch );

        sprintf( buf,
          " &cHouse Pot&w:&z       [&R%d&z] "
          " &cMinimum Ante&w:&z    [&R%d&z] "
          " &cMaximum Ante&w:&z    [&R%d&z]\n\r",
	   pMob->pCasino->pot,	   pMob->pCasino->ante_min,
	   pMob->pCasino->ante_max );
	editor_send_to_char(C_DEFAULT, buf, ch );
   }

    if ( pMob->pShop )
    {
	SHOP_DATA *pShop;
	int iTrade;

	pShop = pMob->pShop;

	sprintf( buf,
	  "&PShop data for&z [&R%5d&z]&w:\n\r"
	  "&cMarkup for purchaser&w:&z [&R%4d%%&z] "
	  "&cMarkdown for seller&w:&z  [&R%4d%%&z] ",
	    pShop->keeper, pShop->profit_buy, pShop->profit_sell );
	editor_send_to_char(C_DEFAULT, buf, ch );
	sprintf( buf, "&cHours&w: &z[&R%2d&z] &cto &z[&R%2d&z]\n\r",
	    pShop->open_hour, pShop->close_hour );
	editor_send_to_char(C_DEFAULT, buf, ch );

	for ( iTrade = 0; iTrade < MAX_TRADE; iTrade++ )
	{
	    if ( pShop->buy_type[iTrade] != 0 )
	    {
		if ( iTrade == 0 ) {
		    editor_send_to_char(C_DEFAULT, " &c Number Trades Type\n\r",ch );
		    editor_send_to_char(C_DEFAULT, " &z ------ -----------\n\r",ch );
		}
		sprintf( buf, "    &z[&R%4d&z] [&W%s&z]\n\r", iTrade,
		    flag_string( type_flags, pShop->buy_type[iTrade] ) );
		editor_send_to_char(C_DEFAULT, buf, ch );
	    }
	}
    }

    return FALSE;
}

bool medit_duplicate( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA	*pMob;
    MOB_INDEX_DATA	*pMob2; /* Mobile to copy */
    char arg[MAX_STRING_LENGTH];
    int vnum;

    EDIT_MOB( ch, pMob );

    if ( argument[0] == '\0' )
    {
      editor_send_to_char(C_DEFAULT, "Syntax: duplicate <vnum> \n\r",ch);
      return FALSE;
    }

    argument = one_argument( argument, arg );

    if ( !is_number(arg) )
    {
      editor_send_to_char(C_DEFAULT, "MEdit: You must enter a number (vnum).\n\r",ch);
      return FALSE;
    }
    else /* argument is a number */
    {
      vnum = atoi(arg);
      if( !( pMob2 = get_mob_index(vnum) ) )
      {
	editor_send_to_char(C_DEFAULT, "MEdit: That mobile does not exist.\n\r",ch);
	return FALSE;
      }
    }

   if ( (pMob2->player_name) )
   {
    free_string( pMob->player_name );
    pMob->player_name = str_dup( pMob2->player_name );
   }

   if ( (pMob2->short_descr) )
   {
    free_string( pMob->short_descr );
    pMob->short_descr = str_dup( pMob2->short_descr );
   }

   if ( (pMob2->long_descr) )
   {
    free_string( pMob->long_descr );
    pMob->long_descr = str_dup( pMob2->long_descr );
   }

    pMob->level = pMob2->level;
    pMob->act = pMob2->act;
    pMob->affected_by = pMob2->affected_by;
    pMob->affected_by2 = pMob2->affected_by2;
    pMob->alignment = pMob2->alignment;
    pMob->race_type = pMob2->race_type;
    pMob->fighting_style = pMob2->fighting_style;
    pMob->size = pMob2->size;
    pMob->hitroll = pMob2->hitroll;
    pMob->p_damp = pMob2->p_damp;
    pMob->m_damp = pMob2->m_damp;
    pMob->hit_modi = pMob2->hit_modi;
    pMob->skin = pMob2->skin;
    pMob->money.gold = pMob2->money.gold;
    pMob->money.silver = pMob2->money.silver;
    pMob->money.copper = pMob2->money.copper;
    pMob->sex = pMob2->sex;
    pMob->imm_flags = pMob2->imm_flags;
    pMob->res_flags = pMob2->res_flags;
    pMob->vul_flags = pMob2->vul_flags;

    pMob->perm_str = pMob2->perm_str;
    pMob->perm_wis = pMob2->perm_wis;
    pMob->perm_con = pMob2->perm_con;
    pMob->perm_agi = pMob2->perm_agi;
    pMob->perm_dex = pMob2->perm_dex;
    pMob->perm_int = pMob2->perm_int;
    pMob->perm_cha = pMob2->perm_cha;

    editor_send_to_char(C_DEFAULT, "Mobile info copied.", ch );
    return TRUE;
}

bool medit_create( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    AREA_DATA *pArea;
    int  value;
    int  iHash;

    value = atoi( argument );

    if ( argument[0] == '\0' )
    {
      pArea = ch->in_room->area;
      for ( value = pArea->lvnum; pArea->uvnum >= value; value++ )
      {
        if ( !get_mob_index( value ) )
         break;
      }
      if ( value > pArea->uvnum )
      {
	editor_send_to_char( C_DEFAULT, "MEdit:  No free mob vnums in this area.\n\r", ch );
	return FALSE;
      }
    }



    /* OLC 1.1b */
    if ( /*argument[0] == '\0' ||*/ value <= 0 || value >= INT_MAX )
    {
	char output[MAX_STRING_LENGTH];

	sprintf( output, "Syntax:  create [0 < vnum < %d]\n\r", INT_MAX );
	editor_send_to_char(C_DEFAULT, output, ch );
	return FALSE;
    }

    pArea = get_vnum_area( value );

    if ( !pArea )
    {
	editor_send_to_char(C_DEFAULT, "MEdit:  That vnum is not assigned an area.\n\r", ch );
	return FALSE;
    }

    if ( get_mob_index( value ) )
    {
	editor_send_to_char(C_DEFAULT, "MEdit:  Mobile vnum already exists.\n\r", ch );
	return FALSE;
    }

    pMob			= new_mob_index();
    pMob->vnum			= value;
    pMob->area			= pArea;

    iHash			= value % MAX_KEY_HASH;
    pMob->next			= mob_index_hash[iHash];
    mob_index_hash[iHash]	= pMob;
    ch->desc->pEdit		= (void *)pMob;

    editor_send_to_char(C_DEFAULT, "Mobile Created.\n\r", ch );
    return TRUE;
}

bool medit_delet( CHAR_DATA *ch, char *argument )
{
   editor_send_to_char( AT_GREY, "If you want to DELETE this mob, spell it out.\n\r", ch );
   return FALSE;
}

bool medit_delete( CHAR_DATA *ch, char *argument )
{
   MOB_INDEX_DATA   *pMob;
   MOB_INDEX_DATA   *prev;

   EDIT_MOB(ch, pMob);

   /* begin remove mob from index */
   prev = mob_index_hash[pMob->vnum % MAX_KEY_HASH];

   if ( pMob == prev )
   {
      mob_index_hash[pMob->vnum % MAX_KEY_HASH] = pMob->next;
   }
   else
   {
      for ( ; prev; prev = prev->next )
      {
	 if ( prev->next == pMob )
	 {
	    prev->next = pMob->next;
	    break;
	 }
      }

      if ( !prev )
      {
	 bug( "medit_delete: mob %d not found.",
	       pMob->vnum );
      }
   }
   /* end remove mob from index*/

   /* delete mob */
   free_mob_index( pMob );

   ch->desc->pEdit = NULL;
   ch->desc->editor = 0;
   editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
   return TRUE;

}

bool medit_spec( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  spec [special function]\n\r", ch );
	return FALSE;
    }


    if ( !str_cmp( argument, "none" ) )
    {
        pMob->spec_fun = NULL;

        editor_send_to_char(C_DEFAULT, "Spec removed.\n\r", ch);
        return TRUE;
    }

    if ( spec_lookup( argument ) )
    {
	pMob->spec_fun = spec_lookup( argument );
	editor_send_to_char(C_DEFAULT, "Spec set.\n\r", ch);
	return TRUE;
    }

    editor_send_to_char(C_DEFAULT, "MEdit: No such special function.\n\r", ch );
    return FALSE;
}



bool medit_align( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  alignment [number]\n\r", ch );
	return FALSE;
    }

    pMob->alignment = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Alignment set.\n\r", ch);
    return TRUE;
}



bool medit_level( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  level [number]\n\r", ch );
	return FALSE;
    }

    pMob->level = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Level set.\n\r", ch);
    return TRUE;
}

bool medit_gold( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  gold [amount]\n\r", ch );
	return FALSE;
    }

    if (atoi( argument ) > pMob->level)
    {
     editor_send_to_char(C_DEFAULT, "Gold can not exceed the mob's level.\n\r",ch);
     return FALSE;
    }

    pMob->money.gold = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Gold coins set.\n\r", ch);
    return TRUE;
}

bool medit_silver( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  silver [amount]\n\r", ch );
        return FALSE;
    }

    if (atoi( argument ) > (pMob->level * 2))
    {
     editor_send_to_char(C_DEFAULT, "Silver can not exceed twice the mob's level.\n\r",ch);
     return FALSE;
    }

    pMob->money.silver = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Silver coins set.\n\r", ch);
    return TRUE;
}

bool medit_copper( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  copper [amount]\n\r", ch );
        return FALSE;
    }

    if (atoi( argument ) > (pMob->level * 4))
    {
     editor_send_to_char(C_DEFAULT, "Copper can not exceed 4 times the mob's level.\n\r",ch);
     return FALSE;
    }

    pMob->money.copper = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Copper coins set.\n\r", ch);
    return TRUE;
}

bool medit_hit( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    int value = 0;

    EDIT_MOB(ch, pMob);
     
  if ( ( value = flag_value( mobhp_flags, argument ) ) != NO_FLAG )
  {
    pMob->hit_modi = value;
    editor_send_to_char(C_DEFAULT, "Mobile hitpoint modifier set.\n\r", ch);
    return TRUE;
  }

  editor_send_to_char(C_DEFAULT, "Type ? mobhp for a list.\n\r", ch);
  return FALSE;
}
bool medit_skin( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    int value = 0;

    EDIT_MOB(ch, pMob);
     
  if ( ( value = flag_value( skin_flags, argument ) ) != NO_FLAG )
  {
    pMob->skin = value;
    editor_send_to_char(C_DEFAULT, "Mobile skin type set.\n\r", ch);
    return TRUE;
  }

  editor_send_to_char(C_DEFAULT, "Type ? skin for a list.\n\r", ch);
  return FALSE;
}

bool medit_hitroll( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: Hitroll <number>\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > pMob->level )
    {
        editor_send_to_char(C_DEFAULT, "The extra hitroll can be no more than the mob's level.\n\r", ch );
        return FALSE;
    }

    pMob->hitroll = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Extra hitroll set.\n\r", ch);
    return TRUE;
}
bool medit_pdamp( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: pdamp <number>\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > pMob->level / 4 )
    {
        editor_send_to_char(C_DEFAULT, "The physical dampener can be no more than the 1/4 of the mob's level.\n\r", ch );
        return FALSE;
    }

    pMob->p_damp = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Physical dampener set.\n\r", ch);
    return TRUE;
}
bool medit_mdamp( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: mdamp <number>\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > pMob->level / 4 )
    {
        editor_send_to_char(C_DEFAULT, "The magical dampener can be no more than the 1/4 of the mob's level.\n\r", ch );
        return FALSE;
    }

    pMob->m_damp = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Magical dampener set.\n\r", ch);
    return TRUE;
}

bool medit_str( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  str [amount]\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > 25 || atoi(argument) < 10 )
    {
        editor_send_to_char(C_DEFAULT, "Stas can only be numbers between 10 and 25.\n\r", ch );
        return FALSE;
    }

    pMob->perm_str = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Strength set.\n\r", ch);
    return TRUE;
}

bool medit_cha( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  cha [amount]\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > 25 || atoi(argument) < 10 )
    {
        editor_send_to_char(C_DEFAULT, "Stas can only be numbers between 10 and 25.\n\r", ch );
        return FALSE;
    }

    pMob->perm_cha = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Charisma set.\n\r", ch);
    return TRUE;
}

bool medit_int( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  int [amount]\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > 25 || atoi(argument) < 10 )
    {
        editor_send_to_char(C_DEFAULT, "Stas can only be numbers between 10 and 25.\n\r", ch );
        return FALSE;
    }

    pMob->perm_int = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Intelligence set.\n\r", ch);
    return TRUE;
}
bool medit_wis( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  wis [amount]\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > 25 || atoi(argument) < 10 )
    {
        editor_send_to_char(C_DEFAULT, "Stas can only be numbers between 10 and 25.\n\r", ch );
        return FALSE;
    }

    pMob->perm_wis = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Wisdom set.\n\r", ch);
    return TRUE;
}
bool medit_dex( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  dex [amount]\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > 25 || atoi(argument) < 10 )
    {
        editor_send_to_char(C_DEFAULT, "Stas can only be numbers between 10 and 25.\n\r", ch );
        return FALSE;
    }

    pMob->perm_dex = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Dexterity set.\n\r", ch);
    return TRUE;
}
bool medit_agi( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  agi [amount]\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > 25 || atoi(argument) < 10 )
    {
        editor_send_to_char(C_DEFAULT, "Stas can only be numbers between 10 and 25.\n\r", ch );
        return FALSE;
    }

    pMob->perm_agi = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Agility set.\n\r", ch);
    return TRUE;
}
bool medit_con( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  con [amount]\n\r", ch );
        return FALSE;
    }

    if ( atoi(argument) > 25 || atoi(argument) < 10 )
    {
        editor_send_to_char(C_DEFAULT, "Stas can only be numbers between 10 and 25.\n\r", ch );
        return FALSE;
    }

    pMob->perm_con = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Constitution set.\n\r", ch);
    return TRUE;
}

bool medit_desc( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' )
    {
	string_append( ch, &pMob->description );
	return TRUE;
    }

    editor_send_to_char(C_DEFAULT, "Syntax:  desc    - line edit\n\r", ch );
    return FALSE;
}




bool medit_long( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  long [string]\n\r", ch );
	return FALSE;
    }

    free_string( pMob->long_descr );
    strcat( argument, "\n\r" );
    pMob->long_descr = str_dup( argument );
    pMob->long_descr[0] = UPPER( pMob->long_descr[0]  );

    editor_send_to_char(C_DEFAULT, "Long description set.\n\r", ch);
    return TRUE;
}



bool medit_short( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  short [string]\n\r", ch );
	return FALSE;
    }

    free_string( pMob->short_descr );
    pMob->short_descr = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Short description set.\n\r", ch);
    return TRUE;
}

bool medit_racetype( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    int value = 0;

    EDIT_MOB(ch, pMob);
     
  if ( ( value = flag_value( race_type_flags, argument ) ) != NO_FLAG )
  {
    pMob->race_type = value;
    editor_send_to_char(C_DEFAULT, "Racial type set.\n\r", ch);
    return TRUE;
  }

  editor_send_to_char(C_DEFAULT, "Type ? racetype for a list.\n\r", ch);
  return FALSE;
}

bool medit_style( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    int value = 0;

    EDIT_MOB(ch, pMob);
     
  if ( ( value = flag_value( fighting_styles, argument ) ) != NO_FLAG )
  {
    pMob->fighting_style = value;
    editor_send_to_char(C_DEFAULT, "Fighting style set.\n\r", ch);
    return TRUE;
  }

  editor_send_to_char(C_DEFAULT, "Type ? fightstyle for a list.\n\r", ch);
  return FALSE;
}

bool medit_size( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    int value = 0;

    EDIT_MOB(ch, pMob);
     
  if ( ( value = flag_value( size_flags, argument ) ) != NO_FLAG )
  {
    pMob->size = value;
    editor_send_to_char(C_DEFAULT, "Size set.\n\r", ch);
    return TRUE;
  }

  editor_send_to_char(C_DEFAULT, "Type ? size for a list.\n\r", ch);
  return FALSE;
}

bool medit_name( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;

    EDIT_MOB(ch, pMob);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  name [string]\n\r", ch );
	return FALSE;
    }

    free_string( pMob->player_name );
    pMob->player_name = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Name set.\n\r", ch);
    return TRUE;
}

bool medit_casino( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    char command[MAX_INPUT_LENGTH];
    char arg1[MAX_INPUT_LENGTH];
    char arg2[MAX_INPUT_LENGTH];

    EDIT_MOB(ch, pMob);

    argument = one_argument( argument, command );
    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( command[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT,
         "Syntax:  casino game    [game]\n\r",ch );
	editor_send_to_char(C_DEFAULT,
         "         casino pot     [maxpot]\n\r",ch );
	editor_send_to_char(C_DEFAULT,
         "         casino minante [value]\n\r", ch );
	editor_send_to_char(C_DEFAULT,
         "         casino maxante [value]\n\r", ch );
	editor_send_to_char(C_DEFAULT,
         "         casino remove\n\r", ch );
	return FALSE;
    }

    if ( !str_cmp( command, "game" ) )
    {
	if ( arg1[0] == '\0' )
	{
	editor_send_to_char(C_DEFAULT, "Syntax:  casino game [game]\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pCasino )
        {
	pMob->pCasino		= new_casino();
	pMob->pCasino->dealer	= pMob->vnum;
	casino_last		= pMob->pCasino;
        }


    if ( !str_cmp( 
	 flag_string( casino_games, flag_value( casino_games, arg1 ) ), 
	 "none" ) )
	{
	editor_send_to_char(C_DEFAULT, "? games for a list of casino games.\n\r", ch );
	return FALSE;
	}
    else
	{
	pMob->pCasino->game = flag_value( casino_games, arg1 );
        editor_send_to_char(C_DEFAULT, "Casino game set.\n\r", ch);
	return TRUE;
        }
        return FALSE;
    }

    if ( !str_cmp( command, "pot" ) )
    {
	if ( arg1[0] == '\0' || !is_number( arg1 ) )
	{
	editor_send_to_char(C_DEFAULT, "Syntax:  casino pot [amount]\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pCasino )
        {
	pMob->pCasino		= new_casino();
	pMob->pCasino->dealer	= pMob->vnum;
	casino_last		= pMob->pCasino;
        }

	editor_send_to_char(C_DEFAULT, "Casino pot set.\n\r", ch );
         pMob->pCasino->pot = atoi( arg1 );
	    return TRUE;
    }

    if ( !str_cmp( command, "minante" ) )
    {
	if ( arg1[0] == '\0' || !is_number( arg1 ) )
	{
	editor_send_to_char(C_DEFAULT, "Syntax:  casino minante [amount]\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pCasino )
        {
	pMob->pCasino		= new_casino();
	pMob->pCasino->dealer	= pMob->vnum;
	casino_last		= pMob->pCasino;
        }

	editor_send_to_char(C_DEFAULT, "Casino minimum ante set.\n\r", ch );
         pMob->pCasino->ante_min = atoi( arg1 );
	    return TRUE;
    }

    if ( !str_cmp( command, "maxante" ) )
    {
	if ( arg1[0] == '\0' || !is_number( arg1 ) )
	{
	editor_send_to_char(C_DEFAULT, "Syntax:  casino maxante [amount]\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pCasino )
        {
	pMob->pCasino		= new_casino();
	pMob->pCasino->dealer	= pMob->vnum;
	casino_last		= pMob->pCasino;
        }

	editor_send_to_char(C_DEFAULT, "Casino maximum ante set.\n\r", ch );
         pMob->pCasino->ante_max = atoi( arg1 );
	    return TRUE;
    }

    if ( !str_cmp( command, "remove" ) )
    {
        CASINO_DATA *pCasino;
	CASINO_DATA *pPrev;

	if ( !pMob->pCasino )
	{
	    editor_send_to_char(C_DEFAULT, "MEdit:  No casino to remove.\n\r", ch );
	    return FALSE;
	}

	for ( pCasino = casino_first, pPrev = NULL; pCasino; 
              pPrev = pCasino, pCasino = pCasino->next )
	{
	    if ( pCasino == pMob->pCasino )
	      break;
	}

	if ( pPrev == NULL && casino_first->next != NULL )
	  casino_first = casino_first->next;
	else
	  pPrev = pCasino;

	free_casino( pCasino );
	pMob->pCasino = NULL;
	editor_send_to_char( C_DEFAULT, "Casino removed.\n\r", ch );
	return TRUE;
    }


    medit_casino( ch, "" );
    return FALSE;
}

bool medit_artist( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    char command[MAX_INPUT_LENGTH];
    char buf[MAX_STRING_LENGTH];
    char arg1[MAX_INPUT_LENGTH];
    char arg2[MAX_INPUT_LENGTH];
    int  cnt = 0;
    int  value;

    EDIT_MOB(ch, pMob);

    argument = one_argument( argument, command );
    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );

    if ( command[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  artist cost\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         artist wear  [location]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         artist boost [booster]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         artist addaffect\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         artist delaffect\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         artist remove\n\r", ch );
	return FALSE;
    }

    if ( !str_cmp( command, "boost" ) )
    {
	if ( arg1[0] == '\0' )
	{
	editor_send_to_char(C_DEFAULT, "Syntax:  artist boost [flag]\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pTattoo )
        {
	pMob->pTattoo         = new_tattoo_artist();
	pMob->pTattoo->artist = pMob->vnum;
	tattoo_artist_last    = pMob->pTattoo;
        }


    if ( !str_cmp( 
	 flag_string( tattoo_flags, flag_value( tattoo_flags, arg1 ) ), 
	 "none" ) )
	{
	editor_send_to_char(C_DEFAULT, "? boost for a list of boosters.\n\r", ch );
	return FALSE;
	}
    else
	{
	pMob->pTattoo->magic_boost = flag_value( tattoo_flags, arg1 );
        editor_send_to_char(C_DEFAULT, "Magic booster set.\n\r", ch);
	return TRUE;
        }
        return FALSE;
    }

    if ( !str_cmp( command, "cost" ) )
    {
	if ( arg1[0] == '\0' || arg2[0] == '\0' || !is_number( arg2 ) )
	{
	editor_send_to_char(C_DEFAULT, "Syntax:  artist cost [type] [amount]\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pTattoo )
        {
	pMob->pTattoo         = new_tattoo_artist();
	pMob->pTattoo->artist = pMob->vnum;
	tattoo_artist_last    = pMob->pTattoo;
        }


        if ( !strcmp( arg1, "gold" ) )
	{
	editor_send_to_char(C_DEFAULT, "Gold cost set.\n\r", ch );
         pMob->pTattoo->cost.gold = atoi( arg2 );
	    return TRUE;
	}
        else if ( !strcmp( arg1, "silver" ) )
	{
	editor_send_to_char(C_DEFAULT, "Silver cost set.\n\r", ch );
         pMob->pTattoo->cost.silver = atoi( arg2 );
	    return TRUE;
	}
        else if ( !strcmp( arg1, "copper" ) )
	{
	editor_send_to_char(C_DEFAULT, "Copper cost set.\n\r", ch );
         pMob->pTattoo->cost.copper = atoi( arg2 );
	    return TRUE;
	}
        else
	{
	editor_send_to_char(C_DEFAULT, "Syntax:  artist cost [type] [amount]\n\r", ch );
	    return FALSE;
	}
    }

    if ( !str_cmp( command, "addaffect" ) )
    {
    AFFECT_DATA *pAf;

    if ( arg1[0] == '\0' || arg2[0] == '\0' || !is_number( arg2 ) )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  addaffect [location] [#mod]\n\r", ch );
	return FALSE;
    }

	if ( !pMob->pTattoo )
        {
	pMob->pTattoo         = new_tattoo_artist();
	pMob->pTattoo->artist = pMob->vnum;
	tattoo_artist_last    = pMob->pTattoo;
        }


    if ( !str_cmp(
	 flag_string( apply_flags, flag_value( apply_flags, arg1 ) ),
	"none" ) )
	{
	sprintf( buf, "Unknown affect %s.\n\r", arg1 );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}

      pAf             =   new_affect();
      pAf->location   =   flag_value( apply_flags, arg1 );
      pAf->modifier   =   atoi( arg2 );
      pAf->type       =   -1;
      pAf->duration   =   -1;
      pAf->next       =   pMob->pTattoo->affected;
      pMob->pTattoo->affected  =   pAf;

      editor_send_to_char(C_DEFAULT, "Affect added.\n\r", ch);
      return TRUE;
     }

    if ( !str_cmp( command, "delaffect" ) )
    {
    AFFECT_DATA *pAf_next;
    AFFECT_DATA *pAf;

    if ( !is_number( arg1 ) || arg1[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  delaffect [#affect]\n\r", ch );
	return FALSE;
    }

	if ( !pMob->pTattoo )
        {
	pMob->pTattoo         = new_tattoo_artist();
	pMob->pTattoo->artist = pMob->vnum;
	tattoo_artist_last    = pMob->pTattoo;
        }


    value = atoi( arg1 );

    if ( value < 0 )
    {
	editor_send_to_char(C_DEFAULT, "Only non-negative affect-numbers allowed.\n\r", ch );
	return FALSE;
    }

    if ( !( pAf = pMob->pTattoo->affected ) )
    {
	editor_send_to_char(C_DEFAULT, "Artist:  Non-existant affect.\n\r", ch );
	return FALSE;
    }

    if( value == 0 )	/* First case: Remove first affect */
    {
	pAf = pMob->pTattoo->affected;
	pMob->pTattoo->affected = pAf->next;
	free_affect( pAf );
    }
    else		/* Affect to remove is not the first */
    {
	while ( ( pAf_next = pAf->next ) && ( ++cnt < value ) )
	     pAf = pAf_next;

	if( pAf_next )		/* See if it's the next affect */
	{
	    pAf->next = pAf_next->next;
	    free_affect( pAf_next );
	}
	else                                 /* Doesn't exist */
	{
	     editor_send_to_char(C_DEFAULT, "No such affect.\n\r", ch );
	     return FALSE;
	}
    }

    editor_send_to_char(C_DEFAULT, "Affect removed.\n\r", ch);
    return TRUE;
  }

    if ( !str_cmp( command, "wear" ) )
    {
	if ( arg1[0] == '\0' )
	{
	editor_send_to_char(C_DEFAULT, "Syntax:  artist wear [wear_flag]\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pTattoo )
        {
	pMob->pTattoo         = new_tattoo_artist();
	pMob->pTattoo->artist = pMob->vnum;
	tattoo_artist_last    = pMob->pTattoo;
        }


    if ( !str_cmp( 
	 flag_string( wear_tattoo, flag_value( wear_tattoo, arg1 ) ), 
	 "none" ) )
	{
	editor_send_to_char(C_DEFAULT, "? tattoo for a list of wear_loc's.\n\r", ch );
	return FALSE;
	}
    else
	{
	pMob->pTattoo->wear_loc = flag_value( wear_tattoo, arg1 );
        editor_send_to_char(C_DEFAULT, "Wear loc set.\n\r", ch);
	return TRUE;
        }
        return FALSE;
    }

    if ( !str_cmp( command, "remove" ) )
    {
        TATTOO_ARTIST_DATA *pTattoo;
	TATTOO_ARTIST_DATA *pPrev;

	if ( !pMob->pTattoo )
	{
	    editor_send_to_char(C_DEFAULT, "MEdit:  No artist to remove.\n\r", ch );
	    return FALSE;
	}

	for ( pTattoo = tattoo_artist_first, pPrev = NULL; pTattoo; 
              pPrev = pTattoo, pTattoo = pTattoo->next )
	{
	    if ( pTattoo == pMob->pTattoo )
	      break;
	}

	if ( pPrev == NULL )
	  tattoo_artist_first = tattoo_artist_first->next;
	else
	  pPrev = pTattoo;

	free_tattoo_artist( pTattoo );
	pMob->pTattoo = NULL;
	editor_send_to_char( C_DEFAULT, "Artist removed.\n\r", ch );
	return TRUE;
    }


    medit_artist( ch, "" );
    return FALSE;
}

bool medit_ally( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    char command[MAX_INPUT_LENGTH];
    char arg1[MAX_INPUT_LENGTH];
    int  value;

    argument = one_argument( argument, command );
    argument = one_argument( argument, arg1 );

    EDIT_MOB(ch, pMob);

    if ( command[0] == '\0' )
    {
     editor_send_to_char(C_DEFAULT, "Syntax:  ally vnum  <mob's vnum>\n\r",
     ch );
     editor_send_to_char(C_DEFAULT, "         ally level <ally's maximum level>\n\r",
     ch );
     return FALSE;
    }

    if ( !str_cmp( command, "vnum" ) )
    {
     if ( arg1[0] == '\0' || !is_number( arg1 ) )
     {
      editor_send_to_char(C_DEFAULT, "Syntax: ally vnum <mob's vnum>\n\r",
       ch );
      return FALSE;
     }

     value = atoi(arg1);
     if( !(get_mob_index(value)) )
     {
      editor_send_to_char(C_DEFAULT, "MEdit: That mobile does not exist.\n\r",ch);
      return FALSE;
     }

     pMob->ally_vnum = value;
     editor_send_to_char(C_DEFAULT, "Alliance mob vnum set.\n\r",ch);
     return TRUE;
    }

    if ( !str_cmp( command, "level" ) )
    {
     if ( arg1[0] == '\0' || !is_number( arg1 ) )
     {
      editor_send_to_char(C_DEFAULT, "Syntax: ally level <maximum ally level>\n\r",
       ch );
      return FALSE;
     }

     value = atoi(arg1);

     if( value > 200 )
     {
      editor_send_to_char(C_DEFAULT, "Level is set too high.\n\r",ch);
      return FALSE;
     }

     if( value < pMob->level )
     {
      editor_send_to_char(C_DEFAULT, "Maximum level must be higher than mob's level.\n\r",ch);
      return FALSE;
     }

     pMob->ally_level = value;
     editor_send_to_char(C_DEFAULT, "Alliance mob max level set.\n\r",ch);
     return TRUE;
    }

    if ( command[0] == '\0' )
    {
     editor_send_to_char(C_DEFAULT, "Syntax:  ally vnum  <mob's vnum>\n\r",
     ch );
     editor_send_to_char(C_DEFAULT, "         ally level <ally's maximum level>\n\r",
     ch );
    }

 return FALSE;
}

bool medit_shop( CHAR_DATA *ch, char *argument )
{
    MOB_INDEX_DATA *pMob;
    char command[MAX_INPUT_LENGTH];
    char arg1[MAX_INPUT_LENGTH];

    argument = one_argument( argument, command );
    argument = one_argument( argument, arg1 );

    EDIT_MOB(ch, pMob);

    if ( command[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  shop hours [#opening] [#closing]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         shop profit [#buying%] [#selling%]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         shop type [#0-4] [item type]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         shop delete [#0-4]\n\r", ch );
	editor_send_to_char(C_DEFAULT, "         shop remove\n\r", ch );
	return FALSE;
    }

    if ( !str_cmp( command, "hours" ) )
    {
	if ( arg1[0] == '\0' || !is_number( arg1 )
	|| argument[0] == '\0' || !is_number( argument ) )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  shop hours [opening] [closing]\n\r", ch );
	    return FALSE;
	}


	if ( !pMob->pShop )
	{
	    pMob->pShop         = new_shop();
	    pMob->pShop->keeper = pMob->vnum;
	    shop_last           = pMob->pShop;
	}

	pMob->pShop->open_hour = atoi( arg1 );
	pMob->pShop->close_hour = atoi( argument );

	editor_send_to_char(C_DEFAULT, "Shop hours set.\n\r", ch);
	return TRUE;
    }


    if ( !str_cmp( command, "profit" ) )
    {
	if ( arg1[0] == '\0' || !is_number( arg1 )
	|| argument[0] == '\0' || !is_number( argument ) )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  shop profit [#buying%] [#selling%]\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pShop )
	{
	    pMob->pShop         = new_shop();
	    pMob->pShop->keeper = pMob->vnum;
	    shop_last           = pMob->pShop;
	}

	pMob->pShop->profit_buy     = atoi( arg1 );
	pMob->pShop->profit_sell    = atoi( argument );

	editor_send_to_char(C_DEFAULT, "Shop profit set.\n\r", ch);
	return TRUE;
    }


    if ( !str_cmp( command, "type" ) )
    {
	char buf[MAX_INPUT_LENGTH];
	int value;

	if ( arg1[0] == '\0' || !is_number( arg1 )
	|| argument[0] == '\0' )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  shop type [#0-4] [item type]\n\r", ch );
	    return FALSE;
	}

	if ( atoi( arg1 ) >= MAX_TRADE )
	{
	    sprintf( buf, "REdit:  May buy %d items max.\n\r", MAX_TRADE );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    return FALSE;
	}

	if ( ( value = flag_value( type_flags, argument ) ) == NO_FLAG )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  That type of item is not known.\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pShop )
	{
	    pMob->pShop         = new_shop();
	    pMob->pShop->keeper = pMob->vnum;
	    shop_last           = pMob->pShop;
	}

	pMob->pShop->buy_type[atoi( arg1 )] = value;

	editor_send_to_char(C_DEFAULT, "Shop type set.\n\r", ch);
	return TRUE;
    }


    if ( !str_cmp( command, "delete" ) )
    {
	if ( arg1[0] == '\0' || !is_number( arg1 ) )
	{
	    editor_send_to_char(C_DEFAULT, "Syntax:  shop delete [#0-4]\n\r", ch );
	    return FALSE;
	}

	if ( !pMob->pShop )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  Non-existant shop.\n\r", ch );
	    return FALSE;
	}

	if ( atoi( arg1 ) >= MAX_TRADE )
	{
	    char buf[MAX_STRING_LENGTH];

	    sprintf( buf, "REdit:  May buy %d items max.\n\r", MAX_TRADE );
	    editor_send_to_char(C_DEFAULT, buf, ch);
	    return FALSE;
	}

	pMob->pShop->buy_type[atoi( arg1 )] = 0;
	editor_send_to_char(C_DEFAULT, "Shop type deleted.\n\r", ch );
	return TRUE;
    }

    else if ( !str_cmp( command, "remove" ) )
    {
        SHOP_DATA *pShop;
	SHOP_DATA *pPrev;

	if ( !pMob->pShop )
	{
	    editor_send_to_char(C_DEFAULT, "REdit:  No shop to remove.\n\r", ch );
	    return FALSE;
	}

	for ( pShop = shop_first, pPrev = NULL; pShop; pPrev = pShop,
	      pShop = pShop->next )
	{
	    if ( pShop == pMob->pShop )
	      break;
	}

	if ( pPrev == NULL && shop_first->next != NULL )
	  shop_first = shop_first->next;
	else
	  pPrev = pShop;

	free_shop( pShop );
	pMob->pShop = NULL;
	editor_send_to_char( C_DEFAULT, "Shop removed.\n\r", ch );
	return TRUE;
    }

    medit_shop( ch, "" );
    return FALSE;
}

bool aedit_sectorize( CHAR_DATA *ch, char *argument)
{
    AREA_DATA *pArea;
    ROOM_INDEX_DATA *room;
    char lower[MAX_STRING_LENGTH];
    char upper[MAX_STRING_LENGTH];
    char sector[MAX_STRING_LENGTH];
    char buf[MAX_STRING_LENGTH];
    int  isector;
    int  ilower;
    int  iupper;
    int  counter;

    EDIT_AREA(ch, pArea);

    argument = one_argument( argument, lower );
    argument = one_argument( argument, upper );
    one_argument( argument, sector );

    if ( !is_number( lower ) || lower[0] == '\0'
     || !is_number( upper ) || upper[0] == '\0' || sector[0] == '\0' )
    {
     editor_send_to_char(C_DEFAULT,
      "Syntax: sectorize <first room vnum> <last room vnum> <sector type>\n\r", ch );
     return FALSE;
    }

    if ( ( ilower = atoi( lower ) ) > ( iupper = atoi( upper ) ) )
    {
     editor_send_to_char(C_DEFAULT,
      "AEdit:  Last room vnum must be larger than the first room vnum.\n\r", ch );
     return FALSE;
    }

    if ( ilower < pArea->lvnum || ilower > pArea->uvnum )
    {
     editor_send_to_char(C_DEFAULT,
      "AEdit: First room vnum is out of current area's range.\n\r", ch );
     return FALSE;
    }   

    if ( iupper < pArea->lvnum || iupper > pArea->uvnum )
    {
     editor_send_to_char(C_DEFAULT,
      "AEdit: Last room vnum is out of current area's range.\n\r", ch );
     return FALSE;
    }   

    if ( ( isector = flag_value( sector_flags, sector ) ) == NO_FLAG )
    {
     editor_send_to_char(C_DEFAULT, "That is not a proper sector.\n\r", ch);
     return FALSE;
    }

    for ( counter = ilower; counter <= iupper; counter++ )
    {
     if ( !(room = get_room_index( counter )) )
      continue;

     room->sector_type = isector;
    }

  sprintf( buf, "Rooms %d through %d sector changed to %s.\n\r",
   ilower, iupper, flag_string( sector_flags, isector ) );
  editor_send_to_char(C_DEFAULT, buf, ch);
  
 return TRUE;
}

bool aedit_weather(CHAR_DATA *ch, char *argument)
{
    AREA_DATA *pArea;
    int value;
    
    EDIT_AREA( ch, pArea );
     
  if ( ( value = flag_value( weather_flags, argument ) ) != NO_FLAG )
  {
    pArea->weather = value;
    editor_send_to_char(C_DEFAULT, "Area weather set.\n\r", ch);
    return TRUE;
  }

  editor_send_to_char(C_DEFAULT, "Type ? weather for a list.\n\r", ch);
  return FALSE;
}

bool aedit_temp(CHAR_DATA *ch, char *argument)
{
    AREA_DATA *pArea;
    int value;
    
    EDIT_AREA( ch, pArea );
     

    if ( !is_number( argument ) || argument[0] == '\0' )
    {
     editor_send_to_char(C_DEFAULT,
      "Syntax: temp <average area temperature in farenheight>\n\r", ch );
     return FALSE;
    }

    value = atoi(argument);

    if ( value < 0 || value > 100 )
    {
     editor_send_to_char(C_DEFAULT,
      "The ave temp can not be below 0 or above 100.\n\r", ch );
     return FALSE;
    }

    pArea->average_temp = value;
    editor_send_to_char(C_DEFAULT, "Area temperature set.\n\r", ch);
    return TRUE;    
}

bool aedit_pressure(CHAR_DATA *ch, char *argument)
{
    AREA_DATA *pArea;
    int value;
    
    EDIT_AREA( ch, pArea );
     

    if ( !is_number( argument ) || argument[0] == '\0' )
    {
     editor_send_to_char(C_DEFAULT,
      "Syntax: pressure <average area pressure in a %>\n\r", ch );
     return FALSE;
    }

    value = atoi(argument);

    if ( value < 0 || value > 100 )
    {
     editor_send_to_char(C_DEFAULT,
      "The ave pressure can not be below 0 or above 100.\n\r", ch );
     return FALSE;
    }

    pArea->average_humidity = value;
    editor_send_to_char(C_DEFAULT, "Area pressure set.\n\r", ch);
    return TRUE;    
}

bool aedit_temporal(CHAR_DATA *ch, char *argument)
{
    AREA_DATA *pArea;
    int value;
    
    EDIT_AREA( ch, pArea );
     
  if ( ( value = flag_value( temporal_flags, argument ) ) != NO_FLAG )
  {
    pArea->temporal = value;
    editor_send_to_char(C_DEFAULT, "Area temporal set.\n\r", ch);
    return TRUE;
  }

  editor_send_to_char(C_DEFAULT, "Type ? temporal for a list.\n\r", ch);
  return FALSE;
}

bool medit_immune(CHAR_DATA *ch, char *argument)
{
  MOB_INDEX_DATA *pMob;
  int value;

  EDIT_MOB(ch, pMob);

  if ( ( value = flag_value( immune_flags, argument ) ) != NO_FLAG )
  {
    TOGGLE_BIT(pMob->imm_flags, value);
    editor_send_to_char(C_DEFAULT, "Immune toggled.\n\r", ch);

    if ( IS_SET( pMob->res_flags, value ) )
    TOGGLE_BIT(pMob->res_flags, value);

    if ( IS_SET( pMob->vul_flags, value ) )
    TOGGLE_BIT(pMob->vul_flags, value);
    
    return TRUE;
  }

  editor_send_to_char(C_DEFAULT, "Immune not found.\n\r", ch);
  return FALSE;
}

bool medit_resist(CHAR_DATA *ch, char *argument)
{
  MOB_INDEX_DATA *pMob;
  int value;

  EDIT_MOB(ch, pMob);

  if ( ( value = flag_value( immune_flags, argument ) ) != NO_FLAG )
  {
    TOGGLE_BIT(pMob->res_flags, value);
    editor_send_to_char(C_DEFAULT, "Resistance toggled.\n\r", ch);

    if ( IS_SET( pMob->imm_flags, value ) )
    TOGGLE_BIT(pMob->imm_flags, value);

    if ( IS_SET( pMob->vul_flags, value ) )
    TOGGLE_BIT(pMob->vul_flags, value);

    return TRUE;
  }
  editor_send_to_char(C_DEFAULT, "Resistance not found.\n\r", ch);
  return FALSE;
}

bool medit_vuln(CHAR_DATA *ch, char *argument)
{
  MOB_INDEX_DATA *pMob;
  int value;

  EDIT_MOB(ch, pMob);

  if ( ( value = flag_value( immune_flags, argument ) ) != NO_FLAG )
  {
    TOGGLE_BIT(pMob->vul_flags, value);
    editor_send_to_char(C_DEFAULT, "Vuln toggled.\n\r", ch);

    if ( IS_SET( pMob->res_flags, value ) )
    TOGGLE_BIT(pMob->res_flags, value);

    if ( IS_SET( pMob->imm_flags, value ) )
    TOGGLE_BIT(pMob->imm_flags, value);

    return TRUE;
  }
  editor_send_to_char(C_DEFAULT, "Vuln not found.\n\r", ch);
  return FALSE;
}


/*
 * MobProg editor functions.
 * -- Altrag
 */
bool medit_mplist( CHAR_DATA *ch, char *argument )
{
  MPROG_DATA *pMProg;
  MOB_INDEX_DATA *pMob;
  char buf[MAX_STRING_LENGTH];
  int value = 0;

  EDIT_MOB(ch, pMob);

  for ( pMProg = pMob->mobprogs; pMProg; pMProg = pMProg->next, value++ )
  {
    sprintf( buf, "[%2d] (%14s)  %s\n\r", value,
	    mprog_type_to_name( pMProg->type ), pMProg->arglist );
    editor_send_to_char(C_DEFAULT, buf, ch );
  }
  return FALSE;
}

bool oedit_oplist( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;
  OBJ_INDEX_DATA *pObj;
  char buf[MAX_STRING_LENGTH];
  int value = 0;

  EDIT_OBJ( ch, pObj );

  for ( pTrap = pObj->traps; pTrap; pTrap = pTrap->next_here, value++ )
  {
    sprintf(buf, "[%2d] (%13s)  %s\n\r", value,
	    flag_string(oprog_types, pTrap->type), pTrap->arglist);
    editor_send_to_char(C_DEFAULT, buf, ch);
  }
  return FALSE;
}

bool redit_rplist( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;
  ROOM_INDEX_DATA *pRoom;
  char buf[MAX_STRING_LENGTH];
  int value = 0;

  EDIT_ROOM(ch, pRoom);

  for ( pTrap = pRoom->traps; pTrap; pTrap = pTrap->next_here, value++ )
  {
    sprintf(buf, "[%2d] (%12s)  %s\n\r", value,
	    flag_string(rprog_types, pTrap->type), pTrap->arglist);
    editor_send_to_char(C_DEFAULT, buf, ch );
  }
  return FALSE;
}

bool redit_eplist( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;
  ROOM_INDEX_DATA *pRoom;
  int dir;
  EXIT_DATA *pExit = NULL;
  char buf[MAX_STRING_LENGTH];
  int value = 0;

  EDIT_ROOM(ch, pRoom);

  for ( dir = 0; dir < 6; dir++ )
    if ( !str_prefix( argument, dir_name[dir] ) &&
	(pExit = pRoom->exit[dir]) )
      break;
  if ( dir == 6 )
  {
    editor_send_to_char(C_DEFAULT, "Exit does not exist in this room.\n\r",ch);
    return FALSE;
  }
  for ( pTrap = pExit->traps; pTrap; pTrap = pTrap->next_here, value++ )
  {
    sprintf(buf, "[%2d] (%11s)  %s\n\r", value,
	    flag_string(eprog_types, pTrap->type), pTrap->arglist);
    editor_send_to_char(C_DEFAULT, buf, ch);
  }
  return FALSE;
}

bool medit_mpremove( CHAR_DATA *ch, char *argument )
{
  MPROG_DATA *pMProg;
  MPROG_DATA *pMPrev;
  MOB_INDEX_DATA *pMob;
  int value = 0;
  int vnum;

  if ( !is_number( argument ) )
  {
    editor_send_to_char( C_DEFAULT, "Syntax:  mpremove #\n\r", ch );
    return FALSE;
  }

  vnum = atoi( argument );

  EDIT_MOB(ch, pMob);


 for ( pMProg = pMob->mobprogs, pMPrev = NULL; value < vnum;
        pMPrev = pMProg, pMProg = pMProg->next, value++ )
  {
    if ( !pMProg )
    {
      editor_send_to_char( C_DEFAULT, "No such MobProg.\n\r", ch );
      return FALSE;
    }
  } 

 /* so we don't crash deleting nonexisting progs */
  if ( !pMProg )
  {
    editor_send_to_char( C_DEFAULT, "No such MobProg.\n\r", ch );
    return FALSE;
  }

  if ( pMPrev == NULL )
    pMob->mobprogs = pMob->mobprogs->next;
  else
    pMPrev->next = pMProg->next;

  free_mprog_data( pMProg );

  num_mob_progs--;
  editor_send_to_char(C_DEFAULT, "Ok.\n\r", ch );
  return TRUE;
}

bool oedit_opremove( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;
  TRAP_DATA *pPrev;
  OBJ_INDEX_DATA *pObj;
  int value = 0;
  int vnum;

  if ( !is_number( argument ) )
  {
    editor_send_to_char(C_DEFAULT, "Syntax:  opremove #\n\r", ch );
    return FALSE;
  }

  vnum = atoi( argument );

  EDIT_OBJ( ch, pObj );

  for ( pTrap = pObj->traps, pPrev = NULL; value < vnum;
        pPrev = pTrap, pTrap = pTrap->next_here, value++ )
  {
    if ( !pTrap )
    {
      editor_send_to_char(C_DEFAULT, "No such ObjProg.\n\r", ch );
      return FALSE;
    }
  }
    /* So we don't crash when deleteing nonexisting progs */
    if ( !pTrap )
    {
      editor_send_to_char(C_DEFAULT, "No such ObjProg.\n\r", ch );
      return FALSE;
    }

  if ( !pPrev )
    pObj->traps = pObj->traps->next_here;
  else
    pPrev->next_here = pTrap->next_here;

  if ( pTrap == trap_list )
    trap_list = pTrap->next;
  else
  {
    for ( pPrev = trap_list; pPrev; pPrev = pPrev->next )
      if ( pPrev->next == pTrap )
	break;
    if ( pPrev )
      pPrev->next = pTrap->next;
  }

  free_trap_data( pTrap );

  num_trap_progs--;
  editor_send_to_char(C_DEFAULT, "Ok.\n\r", ch);
  return TRUE;
}

bool redit_rpremove( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;
  TRAP_DATA *pPrev;
  ROOM_INDEX_DATA *pRoom;
  int value = 0;
  int vnum;

  if ( !is_number( argument ) )
  {
    editor_send_to_char(C_DEFAULT, "Syntax:  rpremove #", ch);
    return FALSE;
  }

  vnum = atoi( argument );

  EDIT_ROOM(ch, pRoom);

  for ( pTrap = pRoom->traps, pPrev = NULL; value < vnum;
	pPrev = pTrap, pTrap = pTrap->next_here, value++ )
  {
    if ( !pTrap )
    {
      editor_send_to_char(C_DEFAULT, "No such RoomProg.\n\r", ch);
      return FALSE;
    }
  }

    /* So we don't crash deleting nonexisting progs */
    if ( !pTrap )
    {
      editor_send_to_char(C_DEFAULT, "No such RoomProg.\n\r", ch);
      return FALSE;
    }

  if ( !pPrev )
    pRoom->traps = pRoom->traps->next_here;
  else
    pPrev->next_here = pTrap->next_here;

  if ( pTrap == trap_list )
    trap_list = pTrap->next;
  else
  {
    for ( pPrev = trap_list; pPrev; pPrev = pPrev->next )
      if ( pPrev->next == pTrap )
	break;
    if ( pPrev )
      pPrev->next = pTrap->next;
  }
  free_trap_data( pTrap );
  num_trap_progs--;
  editor_send_to_char(C_DEFAULT, "Ok.\n\r",ch);
  return TRUE;
}

bool redit_epremove( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;
  TRAP_DATA *pPrev;
  ROOM_INDEX_DATA *pRoom;
  int dir;
  EXIT_DATA *pExit = NULL;
  char arg[MAX_INPUT_LENGTH];
  int value = 0;
  int vnum;

  argument = one_argument(argument, arg);
  if ( arg[0] == '\0' || !is_number( argument ) )
  {
    editor_send_to_char(C_DEFAULT, "Syntax:  epremove <direction> #\n\r",ch);
    return FALSE;
  }

  vnum = atoi(argument);

  EDIT_ROOM(ch, pRoom);

  for ( dir = 0; dir < 6; dir++ )
    if ( !str_prefix(arg, dir_name[dir]) && (pExit = pRoom->exit[dir]) )
      break;
  if ( dir == 6 )
  {
    editor_send_to_char(C_DEFAULT, "Exit does not exist in this room.\n\r", ch);
    return FALSE;
  }

  for ( pTrap = pExit->traps, pPrev = NULL; value < vnum;
        pPrev = pTrap, pTrap = pTrap->next_here, value++ )
  {
    if ( !pPrev )
    {
      editor_send_to_char(C_DEFAULT, "No such ExitProg.\n\r", ch);
      return FALSE;
    }
  }
   /* so we don't crash deleting nonexisting progs */
    if ( !pPrev )
    {
      editor_send_to_char(C_DEFAULT, "No such ExitProg.\n\r", ch);
      return FALSE;
    }

  if ( !pPrev )
    pExit->traps = pExit->traps->next_here;
  else
    pPrev->next_here = pTrap->next_here;

  if ( pTrap == trap_list )
    trap_list = pTrap->next;
  else
  {
    for ( pPrev = trap_list; pPrev; pPrev = pPrev->next )
      if ( pPrev->next == pTrap )
	break;
    if ( pPrev )
      pPrev->next = pTrap->next;
  }
  free_trap_data( pTrap );
  num_trap_progs--;
  editor_send_to_char(C_DEFAULT, "Ok.\n\r", ch);
  return TRUE;
}

bool mpedit_show( CHAR_DATA *ch, char *argument )
{
  MPROG_DATA *pMProg;
  MOB_INDEX_DATA *pMob;
  char buf[MAX_STRING_LENGTH];

  EDIT_MPROG(ch, pMProg);
  pMob = (MOB_INDEX_DATA *)ch->desc->inEdit;

  sprintf(buf, "Mobile: [%5d] %s\n\r", pMob->vnum, pMob->player_name );
  editor_send_to_char( C_DEFAULT, buf, ch );

  sprintf(buf, "MobProg type: %s\n\r", mprog_type_to_name( pMProg->type ) );
  editor_send_to_char(C_DEFAULT, buf, ch );

  sprintf(buf, "Arguments: %s\n\r", pMProg->arglist );
  editor_send_to_char(C_DEFAULT, buf, ch );

  sprintf(buf, "Commands:\n\r%s", pMProg->comlist );
  editor_send_to_char(C_DEFAULT, buf, ch );

  return TRUE;
}

bool tedit_show( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;
  char buf[MAX_STRING_LENGTH];

  EDIT_TRAP( ch, pTrap );

  if ( pTrap->on_obj )
  {
    sprintf(buf, "Object: [%5d] %s\n\r", pTrap->on_obj->vnum,
	    pTrap->on_obj->short_descr);
    editor_send_to_char(C_DEFAULT, buf, ch);
  }

  if ( pTrap->in_room )
  {
    sprintf(buf, "Room: [%5d] %s\n\r", pTrap->in_room->vnum,
	    pTrap->in_room->name);
    editor_send_to_char(C_DEFAULT, buf, ch);
  }

  if ( pTrap->on_exit )
  {
    int dir;
    EXIT_DATA *pExit;
    TRAP_DATA *trap;

    for ( dir = 0; dir < 6; dir++ )
      if ( (pExit = ch->in_room->exit[dir]) )
      {
	for ( trap = pExit->traps; trap; trap = trap->next_here )
	  if ( trap == pTrap )
	    break;
	if ( trap )
	  break;
      }

    sprintf(buf, "Exit: [%5s] %s\n\r", (dir == 6 ? "none" : dir_name[dir]),
	   (dir == 6 ? "Not found in room" : pExit->description));
    editor_send_to_char(C_DEFAULT, buf, ch);
  }

  switch(ch->desc->editor)
  {
  case ED_OPROG:
    sprintf(buf, "ObjProg type: %s\n\r", flag_string(oprog_types,pTrap->type));
    break;
  case ED_RPROG:
    sprintf(buf, "RoomProg type: %s\n\r",flag_string(rprog_types,pTrap->type));
    break;
  case ED_EPROG:
    sprintf(buf, "ExitProg type: %s\n\r",flag_string(eprog_types,pTrap->type));
    break;
  default:
    bug("Tedit_show: Invalid editor %d",ch->desc->editor);
    sprintf(buf, "Unknown TrapProg type\n\r");
    break;
  }
  editor_send_to_char(C_DEFAULT, buf, ch);

/*  sprintf(buf, "Disarmable: %s\n\r", (pTrap->disarmable ? "Yes" : "No"));
  editor_send_to_char(C_DEFAULT, buf, ch);*/

  sprintf(buf, "Arguments: %s\n\r", pTrap->arglist);
  editor_send_to_char(C_DEFAULT, buf, ch);

  sprintf(buf, "Commands:\n\r%s", pTrap->comlist);
  editor_send_to_char(C_DEFAULT, buf, ch);

  return TRUE;
}

bool mpedit_create( CHAR_DATA *ch, char *argument )
{
  MPROG_DATA *pMProg;
  MPROG_DATA *pMLast;
  MOB_INDEX_DATA *pMob;

  EDIT_MOB(ch, pMob);

  pMProg = new_mprog_data( );

  if ( !pMob->mobprogs )
    pMob->mobprogs = pMProg;
  else
  {
    /* No purpose except to find end of list. -- Altrag */
    for ( pMLast = pMob->mobprogs; pMLast->next; pMLast = pMLast->next );
    pMLast->next = pMProg;
  }

  SET_BIT( pMob->progtypes, 1 );

  ch->desc->inEdit = (void *)ch->desc->pEdit;
  ch->desc->pEdit  = (void *)pMProg;
  num_mob_progs++;

  editor_send_to_char(C_DEFAULT, "MobProg created.\n\r", ch );
  return TRUE;
}

bool tedit_create( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;
  TRAP_DATA *pLast;
  TRAP_DATA **pFirst;
  OBJ_INDEX_DATA *pObj = NULL;
  ROOM_INDEX_DATA *pRoom = NULL;
  EXIT_DATA *pExit = NULL;

  switch(ch->desc->editor)
  {
    int dir;
    char arg[MAX_STRING_LENGTH];
  case ED_OPROG:
    pExit = NULL;
    EDIT_OBJ(ch, pObj);
    pFirst = &pObj->traps;
    SET_BIT(pObj->traptypes, 1);
    break;
  case ED_RPROG:
    pExit = NULL;
    EDIT_ROOM(ch, pRoom);
    pFirst = &pRoom->traps;
    SET_BIT(pRoom->traptypes, 1);
    break;
  case ED_EPROG:
    pExit = NULL;
    EDIT_ROOM(ch, pRoom);
    argument = one_argument(argument, arg);
    for ( dir = 0; dir < 6; dir++ )
      if ( !str_prefix(arg, dir_name[dir]) && (pExit = pRoom->exit[dir]) )
	break;
    if ( dir == 6 )
    {
      bug("Tedit_create: No exit",0);
      return FALSE;
    }
    pRoom = NULL;
    pFirst = &pExit->traps;
    SET_BIT(pExit->traptypes, 1);
    break;
  default:
    pExit = NULL;
    bug("Tedit_create: Invalid editor %d", ch->desc->editor);
    return FALSE;
  }

  pTrap = new_trap_data( );
  pTrap->on_obj = pObj;
  pTrap->in_room = pRoom;
  pTrap->on_exit = pExit;

  if ( !trap_list )
    trap_list = pTrap;
  else
  {
    for ( pLast = trap_list; pLast->next; pLast = pLast->next );
    pLast->next = pTrap;
  }


  if ( !*pFirst )
    *pFirst = pTrap;
  else
  {
    for ( pLast = *pFirst; pLast->next_here; pLast = pLast->next_here );
    pLast->next_here = pTrap;
  }

  if ( ch->desc->editor == ED_OPROG )
    ch->desc->inEdit = (void *)ch->desc->pEdit;
  ch->desc->pEdit = (void *)pTrap;

  switch(ch->desc->editor)
  {
  case ED_OPROG:
    editor_send_to_char(C_DEFAULT, "ObjProg created\n\r",ch);
    break;
  case ED_RPROG:
    editor_send_to_char(C_DEFAULT, "RoomProg created\n\r",ch);
    break;
  case ED_EPROG:
    editor_send_to_char(C_DEFAULT, "ExitProg created\n\r",ch);
    break;
  }
  num_trap_progs++;
  return TRUE;
}

bool mpedit_arglist( CHAR_DATA *ch, char *argument )
{
  MPROG_DATA *pMProg;
  int prc = 0;

  EDIT_MPROG(ch, pMProg);

  if ( argument[0] == '\0' )
  {
    editor_send_to_char( C_DEFAULT, "Syntax:  arglist [string]\n\r", ch );
    return FALSE;
  }

  prc = ( is_number( argument ) ? atoi( argument ) : 0 );
  if ( pMProg->type == RAND_PROG && prc > 95 )
  {
    editor_send_to_char( C_DEFAULT, "You can't set the percentage that high on a rand_prog.\n\r", ch );
    return FALSE;
  }
  free_string(pMProg->arglist);
  pMProg->arglist = str_dup(argument);

  editor_send_to_char( C_DEFAULT, "Arglist set.\n\r", ch );
  return TRUE;
}

bool tedit_arglist( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;

  EDIT_TRAP(ch, pTrap);

  if ( argument[0] == '\0' )
  {
    editor_send_to_char(C_DEFAULT, "Syntax:  arglist [string]\n\r",ch);
    return FALSE;
  }
  free_string(pTrap->arglist);
  pTrap->arglist = str_dup(argument);

  editor_send_to_char(C_DEFAULT, "Arglist set.\n\r",ch);
  return TRUE;
}

bool mpedit_comlist( CHAR_DATA *ch, char *argument )
{
  MPROG_DATA *pMProg;

  EDIT_MPROG(ch, pMProg);

  if ( argument[0] == '\0' )
  {
    string_append( ch, &pMProg->comlist );
    return TRUE;
  }

  editor_send_to_char( C_DEFAULT, "Syntax:  comlist    - line edit\n\r", ch );
  return FALSE;
}

bool tedit_comlist( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;

  EDIT_TRAP(ch, pTrap);

  if ( argument[0] == '\0' )
  {
    string_append( ch, &pTrap->comlist );
    return TRUE;
  }

  editor_send_to_char( C_DEFAULT, "Syntax:  comlist    - line edit\n\r", ch );
  return FALSE;
}

bool tedit_disarmable( CHAR_DATA *ch, char *argument )
{
  TRAP_DATA *pTrap;

  EDIT_TRAP(ch, pTrap);

  pTrap->disarmable = !pTrap->disarmable;

  if ( pTrap->disarmable )
    editor_send_to_char(C_DEFAULT, "Trap is now disarmable.\n\r",ch);
  else
    editor_send_to_char(C_DEFAULT, "Trap is no longer disarmable.\n\r",ch);
  return TRUE;
}

/* Decklarean */

bool sedit_show(CHAR_DATA *ch, char *argument)
{
  SOCIAL_DATA *pSocial;
  char buf[MAX_STRING_LENGTH];

  EDIT_SOCIAL(ch, pSocial);

  sprintf( buf, "&YKeyword: &W%s\n\r", pSocial->name );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, 
           "&Y(1) Char_no_arg:    &B<social>          &wYou see.\n\r&W%s\n\r",
           pSocial->char_no_arg );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf,
           "&Y(2) Others_no_arg:  &B<social>          &wRoom see.\n\r&W%s\n\r",
           pSocial->others_no_arg );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf,
           "&Y(3) Char_found:     &B<social> <victim> &wYou see.\n\r&W%s\n\r",
           pSocial->char_found );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf,
           "&Y(4) Others_found:   &B<social> <victim> &wRoom see.\n\r&W%s\n\r",
	   pSocial->others_found );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf,
           "&Y(5) Victim_found:   &B<social> <victim> &wVictim see.\n\r&W%s\n\r",
           pSocial->vict_found );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf,
           "&Y(6) Char_auto:      &B<social> self     &wYou see.\n\r&W%s\n\r", 
           pSocial->char_auto );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf,
           "&Y(7) Others_auto:    &B<social> self     &wRoom see\n\r&W%s\n\r",
           pSocial->others_auto );
  editor_send_to_char( C_DEFAULT, buf, ch );

  return FALSE;
}

bool sedit_name(CHAR_DATA *ch, char *argument)
{
  SOCIAL_DATA *pSocial;
  EDIT_SOCIAL(ch, pSocial);

  if(argument[0] == '\0')
  {
    editor_send_to_char(C_DEFAULT, "Syntax:  keyword [string]\n\r", ch);
    return FALSE;
  }

  if( get_social(argument) != NULL )
  {
    editor_send_to_char(C_DEFAULT, "Keyword already taken.\n\r", ch);
    return FALSE;
  }

  free_string( pSocial->name);
  pSocial->name = str_dup(argument);

  editor_send_to_char(C_DEFAULT, "Keyword set.\n\r", ch);
  return TRUE;
}

bool sedit_char_no_arg( CHAR_DATA *ch, char *argument )
{
    SOCIAL_DATA *pSocial;

    EDIT_SOCIAL(ch, pSocial);

    if ( argument[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:   char_no_arg [string]\n\r", ch );
        return FALSE;
    }

    free_string( pSocial->char_no_arg );
    if (str_cmp(argument, "clear" ))
     pSocial->char_no_arg = str_dup( argument );
    else
     pSocial->char_no_arg = &str_empty[0];
    editor_send_to_char(C_DEFAULT, "Set.\n\r", ch );
    return TRUE;
}

bool sedit_others_no_arg( CHAR_DATA *ch, char *argument )
{
    SOCIAL_DATA *pSocial;

    EDIT_SOCIAL(ch, pSocial);

    if ( argument[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:   others_no_arg [string]\n\r", ch );
        return FALSE;
    }

    free_string( pSocial->others_no_arg );
    if (str_cmp(argument, "clear" ))
      pSocial->others_no_arg = str_dup( argument );
    else pSocial->others_no_arg = &str_empty[0];
      editor_send_to_char(C_DEFAULT, "Set.\n\r", ch );
    return TRUE;
}

bool sedit_char_found( CHAR_DATA *ch, char *argument )
{
    SOCIAL_DATA *pSocial;

    EDIT_SOCIAL(ch, pSocial);

    if ( argument[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:   char_found [string]\n\r", ch );
        return FALSE;
    }

    free_string( pSocial->char_found );
if (str_cmp(argument, "clear" ))
    pSocial->char_found = str_dup( argument );
else pSocial->char_found = &str_empty[0];
    editor_send_to_char(C_DEFAULT, "Set.\n\r", ch );
    return TRUE;
}

bool sedit_others_found( CHAR_DATA *ch, char *argument )
{
    SOCIAL_DATA *pSocial;

    EDIT_SOCIAL(ch, pSocial);

    if ( argument[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:   others_found [string]\n\r", ch );
        return FALSE;
    }

    free_string( pSocial->others_found );
if (str_cmp(argument, "clear" ))
    pSocial->others_found = str_dup( argument );
else pSocial->others_found = &str_empty[0];
    editor_send_to_char(C_DEFAULT, "Set.\n\r", ch );
    return TRUE;
}

bool sedit_vict_found( CHAR_DATA *ch, char *argument )
{
    SOCIAL_DATA *pSocial;

    EDIT_SOCIAL(ch, pSocial);

    if ( argument[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:   vict_found [string]\n\r", ch );
        return FALSE;
    }

    free_string( pSocial->vict_found );
if (str_cmp(argument, "clear" ))
    pSocial->vict_found = str_dup( argument );
else pSocial->vict_found = &str_empty[0];
    editor_send_to_char(C_DEFAULT, "Set.\n\r", ch );
    return TRUE;
}

bool sedit_char_auto( CHAR_DATA *ch, char *argument )
{
    SOCIAL_DATA *pSocial;

    EDIT_SOCIAL(ch, pSocial);

    if ( argument[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:   char_auto [string]\n\r", ch );
        return FALSE;
    }

    free_string( pSocial->char_auto );
if (str_cmp(argument, "clear" ))
    pSocial->char_auto = str_dup( argument );
else pSocial->char_auto = &str_empty[0];
    editor_send_to_char(C_DEFAULT, "Set.\n\r", ch );
    return TRUE;
}

bool sedit_others_auto( CHAR_DATA *ch, char *argument )
{
    SOCIAL_DATA *pSocial;

    EDIT_SOCIAL(ch, pSocial);

    if ( argument[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:   others_auto [string]\n\r", ch );
        return FALSE;
    }

    free_string( pSocial->others_auto );
if (str_cmp(argument, "clear" ))
    pSocial->others_auto = str_dup( argument );
else pSocial->others_auto = &str_empty[0];
    editor_send_to_char(C_DEFAULT, "Set.\n\r", ch );
    return TRUE;
}

bool sedit_delete(CHAR_DATA *ch, char *argument)
{
  SOCIAL_DATA *pSocial;
  SOCIAL_DATA *pMark;

  EDIT_SOCIAL(ch, pSocial);

  if(argument[0] != '\0')
  {
    editor_send_to_char(C_DEFAULT, "Type delete by itself.\n\r", ch);
    return FALSE;
  }

  if(pSocial == social_first)
  {
    social_first = pSocial->next;
    free_social_index( pSocial );

    ch->desc->pEdit = NULL;
    ch->desc->editor = 0;
    editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
    return TRUE;
  }

  for(pMark = social_first;pMark;pMark = pMark->next)
  {
    if(pSocial == pMark->next)
    {
      pMark->next = pSocial->next;
      free_social_index( pSocial );

      ch->desc->pEdit = NULL;
      ch->desc->editor = 0;
      editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
      return TRUE;
    }
  }
  return FALSE;
}

bool rename_show(CHAR_DATA *ch, char *argument)
{
  char buf[MAX_STRING_LENGTH];
  OBJ_DATA  *pObj;

  RENAME_OBJ( ch, pObj );

  editor_send_to_char( AT_WHITE, "REMAKE AN ITEM: Type done when finished.\n\r\n\r", ch );
  sprintf( buf, "&Y1) &WKeyword(s):        &zSyntax: &R1 <new keyword>\n\r&B[&G%s&B]\n\r", pObj->name );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Y2) &WShort:             &zSyntax: &R2 <Looks in inv or equiped.>\n\r&B[&G%s&B]\n\r",
                 pObj->short_descr );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Y3) &WLong:              &zSyntax: &R3 <Looks when on the ground or looked at.>\n\r&B[&G%s&B]\n\r",
	         pObj->description );
  editor_send_to_char( C_DEFAULT, buf, ch );

  return FALSE;
}

bool rename_keyword( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *pObj;

    RENAME_OBJ(ch, pObj);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  keyword [string]\n\r", ch );
	return FALSE;
    }

    free_string( pObj->name );
    pObj->name = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Keyword set.\n\r", ch);
    return TRUE;
}

bool rename_short( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *pObj;

    RENAME_OBJ(ch, pObj);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  short [string]\n\r", ch );
	return FALSE;
    }

    free_string( pObj->short_descr );
    pObj->short_descr = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Short description set.\n\r", ch);
    return TRUE;
}

bool rename_long( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *pObj;

    RENAME_OBJ(ch, pObj);

    if ( argument[0] == '\0' )
    {
	editor_send_to_char(C_DEFAULT, "Syntax:  long [string]\n\r", ch );
	return FALSE;
    }
        
    free_string( pObj->description );
    pObj->description = str_dup( argument );
    pObj->description[0] = UPPER( pObj->description[0] );

    editor_send_to_char(C_DEFAULT, "Long description set.\n\r", ch);
    return TRUE;
}

/*
bool qedit_show(CHAR_DATA *ch, char *argument)
{
  QUEST_DATA *pQuest;
  char buf[MAX_STRING_LENGTH];
  MOB_INDEX_DATA *questmaster;
  int counter;

  EDIT_QUEST(ch, pQuest);

  if(pQuest == NULL)
  {
    editor_send_to_char(C_DEFAULT, "bug 1", ch);
    return FALSE;
  }

  sprintf(buf, "&CName:  &G%s\n\r",
   pQuest->name ? pQuest->name : "none");
  editor_send_to_char(C_DEFAULT, buf, ch);

  sprintf(buf, "&CVnum: &w[&G%-3d&w] &CType: &G%s\n\r",
   pQuest->vnum, flag_string( quest_types, pQuest->quest_type) );
  editor_send_to_char(C_DEFAULT, buf, ch);

  sprintf(buf, "&CQP value: &w[&G%-5d&w] &CLevel range: &G%d-%d\n\r",
   pQuest->qp_value, pQuest->suggested_level[0],
   pQuest->suggested_level[1] );
  editor_send_to_char(C_DEFAULT, buf, ch);

  if ( !(questmaster = get_mob_index( pQuest->questmaster_vnum )) )
   sprintf(buf, "&CQuestmaster mob:  &GNone &w(&P%d&w)\n\r",
    pQuest->questmaster_vnum);
  else
   sprintf(buf, "&CQuestmaster mob:  &G%s &w(&P%d&w)\n\r",
    questmaster->short_descr, pQuest->questmaster_vnum);

  editor_send_to_char(C_DEFAULT, buf, ch);

  sprintf(buf, "&GQuest mob vnums and location room vnums:\n\r" );
  editor_send_to_char(C_DEFAULT, buf, ch);

  for ( counter = 0; counter < 20; counter++ )
  {
   sprintf(buf, "&C%-5d/%-5d", pQuest->quest_mob[counter],
    pQuest->quest_mob_loc[counter] );
   editor_send_to_char(C_DEFAULT, buf, ch);

   if ( counter == 9 || counter == 4 || counter == 14 )
    editor_send_to_char(C_DEFAULT, "\n\r", ch);
  }

  sprintf(buf, "\n\r&GQuest obj vnums and location room vnums:\n\r" );
  editor_send_to_char(C_DEFAULT, buf, ch);

  for ( counter = 0; counter < 20; counter++ )
  {
   sprintf(buf, "&P%-5d/%-5d", pQuest->quest_obj[counter],
    pQuest->quest_obj_loc[counter] );
   editor_send_to_char(C_DEFAULT, buf, ch);

   if ( counter == 9 || counter == 4 || counter == 14 )
    editor_send_to_char(C_DEFAULT, "\n\r", ch);
  }

  return FALSE;
}
*/

bool qedit_name(CHAR_DATA *ch, char *argument)
{
  QUEST_DATA *pQuest;
  EDIT_QUEST(ch, pQuest);

  if(argument[0] == '\0')
  {
    editor_send_to_char(C_DEFAULT, "Syntax: name [string]\n\r", ch);
    return FALSE;
  }

  free_string( pQuest->name);

  pQuest->name = str_dup(argument);

  editor_send_to_char(C_DEFAULT, "Name set.\n\r", ch);
  return TRUE;
}

/* XOR */
bool hedit_show(CHAR_DATA *ch, char *argument)
{
  HELP_DATA *pHelp;
  char buf[MAX_STRING_LENGTH];

  EDIT_HELP(ch, pHelp);
  if(pHelp == NULL)
  {
    editor_send_to_char(C_DEFAULT, "bug 1", ch);
    return FALSE;
  }
  sprintf(buf, "Keyword(s):    [%s]\n\r",
   pHelp->keyword ? pHelp->keyword : "none");
  editor_send_to_char(C_DEFAULT, buf, ch);

  sprintf(buf, "Level:         [%d]\n\r", pHelp->level);
  editor_send_to_char(C_DEFAULT, buf, ch);

  sprintf(buf, "Description:\n\r%s\n\r",
   pHelp->text ? pHelp->text : "none.");
  editor_send_to_char(C_DEFAULT, buf, ch);
  return FALSE;
}

bool hedit_desc(CHAR_DATA *ch, char *argument)
{
  HELP_DATA *pHelp;
  EDIT_HELP(ch, pHelp);

  if(argument[0] == '\0')
  {
    string_append( ch, &pHelp->text);
    return TRUE;
  }
  editor_send_to_char(C_DEFAULT, "Syntax:  desc    - line edit\n\r", ch);
  return FALSE;
}

bool hedit_level(CHAR_DATA *ch, char *argument)
{
  HELP_DATA *pHelp;
  EDIT_HELP(ch, pHelp);
  
  if(argument[0] == '\0' || !is_number(argument))
  {
    editor_send_to_char(C_DEFAULT, "Syntax:  level [number]\n\r", ch );
    return FALSE;
  }

  pHelp->level = atoi(argument);

  editor_send_to_char(C_DEFAULT, "Level set.\n\r", ch);
  return TRUE;
}

bool hedit_name(CHAR_DATA *ch, char *argument)
{
  HELP_DATA *pHelp;
  EDIT_HELP(ch, pHelp);

  if(argument[0] == '\0')
  {
    editor_send_to_char(C_DEFAULT, "Syntax:  keyword [string]\n\r", ch);
    return FALSE;
  }

  if( get_help(argument) != NULL )
  {
    editor_send_to_char(C_DEFAULT, "Keyword already taken.\n\r", ch);
    return FALSE;
  }

  free_string( pHelp->keyword);

  pHelp->keyword = str_dup(argument);

  editor_send_to_char(C_DEFAULT, "Keyword(s) set.\n\r", ch);
  return TRUE;
}

bool edit_delet(CHAR_DATA *ch, char *argument)
{
  editor_send_to_char(C_DEFAULT, "If you want to delete, spell it out.\n\r", ch);
  return FALSE;
}

bool hedit_delete(CHAR_DATA *ch, char *argument)
{
  HELP_DATA *pHelp;
  HELP_DATA *pMark;
  EDIT_HELP(ch, pHelp);

  if(argument[0] != '\0')
  {
    editor_send_to_char(C_DEFAULT, "Type delete by itself.\n\r", ch);
    return FALSE;
  }

  if(pHelp == help_first)
  {
     help_first = pHelp->next;
     free_string( pHelp->keyword );
     free_string( pHelp->text );
     free_mem( pHelp, sizeof( *pHelp ) );
     top_help--;
     ch->desc->pEdit = NULL;
     ch->desc->editor = 0;
     editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
     return TRUE;
  }

  for(pMark = help_first;pMark;pMark = pMark->next)
  {
    if(pHelp == pMark->next)
    {
      pMark->next = pHelp->next;
/*      pHelp->next = help_free;
      help_free = pHelp;*/
      free_string( pHelp->keyword );
      free_string( pHelp->text );
      free_mem( pHelp, sizeof( *pHelp ) );
      top_help--;
      ch->desc->pEdit = NULL;
      ch->desc->editor = 0;
      editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
      return TRUE;
    }
  }
  return FALSE;
}

/* END */
bool forge_show(CHAR_DATA *ch, char *argument)
{
  char buf[MAX_STRING_LENGTH];
  OBJ_DATA  *pObj;
  AFFECT_DATA *paf;
  AFFECT_DATA *paf_next = NULL;
  int cnt, max_stat, max_dam, max_hit, max_hp, max_ac, max_mana;
  int max_saves, max_saveb, max_ad;
  FORGE_OBJ( ch, pObj );
  max_stat = ( pObj->level > 100 ) ? 3 : 2;
  if ( pObj->item_type == ITEM_WEAPON )
	max_dam = pObj->level / 2.5;
  else
	max_dam = ( IS_SET( pObj->wear_flags, ITEM_WEAR_BODY ) )
		? pObj->level / 3 : pObj->level / 8;
  max_hit = max_dam * 2 / 3;
  max_hp = max_mana = pObj->level;
  max_saves = max_saveb = 0 - UMAX( 1, pObj->level / 7 );
  max_ad = pObj->level * 0.4;
  max_ac = 0 - ( pObj->level * 3 / 4);
  editor_send_to_char( AT_GREY, "Forging an item; type &RDONE &wonly when finished.\n\r", ch );
  editor_send_to_char( AT_DGREY, "Name and descriptions:\n\r", ch );
  sprintf( buf, "Keywords&w:    &z[&W%s&z]\n\r", pObj->name );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "Short Desc&w:  &z[&W%s&z]\n\r", pObj->short_descr );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "Long Desc&w:   &z[&W%s&z]\n\r", pObj->description );
  editor_send_to_char( AT_WHITE, buf, ch );
  if ( pObj->item_type == ITEM_WEAPON )
   {
   sprintf( buf, "Weapon Type&w: &z[&W%s&z]\n\r",
  	    flag_string( weapon_flags, pObj->value[3] ) );
   editor_send_to_char( AT_WHITE, buf, ch );
   }
  editor_send_to_char( AT_DGREY, "Availble stats and affects:\n\r", ch );
  sprintf( buf, "[&WStat 1&w:       &R+%d&z] [&WDamroll&w:      &R+%d&z] [&WHitroll&w:       &R+%d&z]\n\r",
	   max_stat, max_dam, max_hit );
  editor_send_to_char( AT_DGREY, buf, ch );
  sprintf( buf, "[&WHit Points&w: &R+%d&z] [&WMana&w:        &R+%d&z] [&WArmor Class&w:  &R%d&z]",
	   max_hp, max_mana, max_ac );
  editor_send_to_char( AT_DGREY, buf, ch );
  if ( pObj->level >= 40 )
    {
    sprintf( buf,  "[&WSaving-Spell&w: &R%d&z] [&WSaving-Breath&w: &R%d&z]\n\r",
 	     max_saves, max_saveb );
    editor_send_to_char( AT_DGREY, buf, ch );
    }
  if ( pObj->level >= 45 )
    {
    sprintf( buf, "[&WStat 2&w:        &R+%d&z]", max_stat );
    editor_send_to_char( AT_DGREY, buf, ch );
    }
  if ( pObj->level >= 60 )
    {
    sprintf( buf, " [&WAnti-Disarm&w: &R+%d&z]", max_ad );
    editor_send_to_char( AT_DGREY, buf, ch );
    }
  if ( pObj->level >= 101 )
   {
   sprintf( buf, " [&WStat 3&w:        &R+%d&z]", max_stat );
   editor_send_to_char( AT_DGREY, buf, ch );
   }
  editor_send_to_char( AT_GREY, "\n\r", ch );
  editor_send_to_char( AT_DGREY, "Added stats and affects:\n\r", ch );
  editor_send_to_char( AT_WHITE, "#&w- &z[&W  affect  &z] [&Wmodifier&z]\n\r", ch );
  for ( cnt = 1, paf = pObj->affected; cnt <= pObj->level / 10; cnt++, paf = paf_next )
	{
	if ( cnt == 7 )
	  break;
	if ( paf )
	  {
	  paf_next = paf->next;
	  sprintf( buf, "%d&w- &z[&W%10s&z] [&R%8d&z]\n\r", cnt,
		   flag_string( apply_flags, paf->location ),
		   paf->modifier );
	  }
	else
	  sprintf( buf, "%d&w- &z[      &Wnone&z] [       &R0&z]\n\r", cnt );
	editor_send_to_char( AT_WHITE, buf, ch );
	}
  sprintf( buf, "Gold Cost&w:   &z[&R%d&z]\n\r"
	        "Silver Cost&w: &z[&R%d&z]\n\r"
		"Copper Cost&w: &z[&R%d&z]\n\r",
          pObj->cost.gold, pObj->cost.silver, pObj->cost.copper );
  editor_send_to_char( AT_WHITE, buf, ch );
  return FALSE;
}
bool forge_addaffect( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *pObj;
    AFFECT_DATA *pAf;
    char loc[MAX_STRING_LENGTH];
    char mod[MAX_STRING_LENGTH];
    char buf[MAX_INPUT_LENGTH];
  int cnt, max_stat, max_dam, max_hit, max_hp, max_ac, max_mana;
  int max_saves, max_saveb, max_ad, stat_cnt, max_statn;
  int cost = 0;
  int Mod = 0;
  bool legal = FALSE;
  FORGE_OBJ( ch, pObj );
  max_statn = 1 + ( pObj->level >= 45 ) + ( pObj->level >= 101 );
  max_stat = ( pObj->level > 100 ) ? 3 : 2;
  if ( pObj->item_type == ITEM_WEAPON )
	max_dam = pObj->level / 2.5;
  else
	max_dam = ( IS_SET( pObj->wear_flags, ITEM_WEAR_BODY ) )
		? pObj->level / 3 : pObj->level / 8;
  max_hit = max_dam * 2 / 3;
  max_hp = max_mana = pObj->level;
  max_saves = max_saveb = 0 - UMAX( 1, pObj->level / 7 );
  max_ad = pObj->level * 0.4;
  max_ac = 0 - ( pObj->level * 3 / 4 );

  argument = one_argument( argument, loc );
  one_argument( argument, mod );

  if ( loc[0] == '\0' || mod[0] == '\0' || !is_number( mod ) )
    {
    editor_send_to_char(C_DEFAULT, "Syntax: # [affect] [modifier]\n\r", ch );
    return FALSE;
    }
  if ( !str_prefix( loc, "strength" ) )
    {
    strcpy( loc, "strength" );
    if ( (Mod=atoi( mod )) > max_stat )
	{
	sprintf( buf, "You may not add more than %d to %s.\n\r",
		 max_stat, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 75;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "intelligence" ) )
    {
    strcpy( loc, "intelligence" );
    if ( (Mod=atoi( mod )) > max_stat )
	{
	sprintf( buf, "You may not add more than %d to %s.\n\r", 
		 Mod, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 75;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "wisdom" ) )
    {
    strcpy( loc, "wisdom" );
    if ( (Mod=atoi( mod )) > max_stat )
	{
	sprintf( buf, "You may not add more than %d to %s.\n\r", 
		 max_stat, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 75;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "dexterity" ) )
    {
    strcpy( loc, "dexterity" );
    if ( (Mod=atoi( mod )) > max_stat )
	{
	sprintf( buf, "You may not add more than %d to %s.\n\r", 
		 max_stat, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 75;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "constitution" ) )
    {
    strcpy( loc, "constitution" );
    if ( (Mod=atoi( mod )) > max_stat )
	{
	sprintf( buf, "You may not add more than %d to %s.\n\r", 
		 max_stat, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 75;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "damroll" ) )
    {
    strcpy( loc, "damroll" );
    if ( (Mod=atoi( mod )) > max_dam )
	{
	sprintf( buf, "You may not add more than %d %s.\n\r", 
		 max_dam, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 100;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "hitroll" ) )
    {
    strcpy( loc, "hitroll" );
    if ( (Mod=atoi( mod )) > max_hit )
	{
	sprintf( buf, "You may not add more than %d %s.\n\r", 
		 max_hit, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 100;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "hitpoints" ) || !str_cmp( loc, "hp" ) )
    {
    strcpy( loc, "hp" );
    if ( (Mod=atoi( mod )) > max_hp )
	{
	sprintf( buf, "You may not add more than %d %ss.\n\r", 
		 max_hp, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 50;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "armorclass" ) || !str_cmp( loc, "ac" ) )
    {
    strcpy( loc, "ac" );
    if ( (Mod=atoi( mod )) < max_ac )
	{
	sprintf( buf, "You may not add more than %d %ss.\n\r", 
		 max_ac, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = abs(Mod) * 25;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "mana" ) )
    {
    strcpy( loc, "mana" );
    if ( (Mod=atoi( mod )) > max_mana )
	{
	sprintf( buf, "You may not add more than %d %s.\n\r", 
		 max_mana, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 50;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "anti-disarm" ) )
    {
    strcpy( loc, "anti-disarm" );
    if ( (Mod=atoi( mod )) > max_ad )
	{
	sprintf( buf, "You may not add more than %d %s.\n\r", 
		 max_ad, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = Mod * 50;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "saving-spell" ) )
    {
    strcpy( loc, "saving-spell" );
    if ( (Mod=atoi( mod )) < max_saves )
	{
	sprintf( buf, "You may not add more than %d %s.\n\r", 
		 max_saves, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = abs(Mod) * 75;
    legal = TRUE;
    }
  if ( !str_prefix( loc, "saving-breath" ) )
    {
    strcpy( loc, "saving-breath" );
    if ( (Mod=atoi( mod )) < max_saveb )
	{
	sprintf( buf, "You may not add more than %d %s.\n\r", 
		 max_saveb, loc );
	editor_send_to_char( AT_GREY, buf, ch );
	return FALSE;
	}
    cost = abs(Mod) * 75;
    legal = TRUE;
    }
    if ( !legal )
	{
	sprintf( buf, "Unknown affect %s, please choose from the list.\n\r", loc );
	editor_send_to_char(AT_GREY, buf, ch );
	return FALSE;
	}
    cnt = 0;
    stat_cnt = 0;
    for ( pAf = pObj->affected; pAf; pAf = pAf->next )
	{
	cnt++;
	if ( cnt >= pObj->level / 10 || cnt >= 6 )
	  {
	  editor_send_to_char( AT_GREY, "You can no longer add anything to this item.\n\r", ch );
	  return FALSE;
	  }
	if ( pAf->location == flag_value( apply_flags, loc ) )
	  {
	  sprintf( buf, "You have already added %d %s to this item.\n\r",
		   pAf->modifier, loc );
	  editor_send_to_char( AT_GREY, buf, ch );
	  return FALSE;
	  }
        if ( pAf->location == APPLY_STR
	|| pAf->location == APPLY_DEX
	|| pAf->location == APPLY_INT
	|| pAf->location == APPLY_WIS
	|| pAf->location == APPLY_CON )
	  stat_cnt++;
        if ( stat_cnt >= max_statn
	&& (   !str_cmp( loc, "strength" )
	    || !str_cmp( loc, "dexterity" )
	    || !str_cmp( loc, "intelligence" )
	    || !str_cmp( loc, "wisdom" )
	    || !str_cmp( loc, "constitution" ) ) )
	  {
	  editor_send_to_char( AT_GREY, "You have already added the maximum number of stats possible for your experience.\n\r", ch );
	  return FALSE;
	  }
    }	
    pAf             =   new_affect();
    pAf->location   =   flag_value( apply_flags, loc );
    pAf->modifier   =   Mod;
    pAf->type       =   -1;
    pAf->duration   =   -1;
    pAf->bitvector  =   0;
    pAf->next       =   pObj->affected;
    pObj->affected  =   pAf;
    sprintf( buf, "Added %d %s for a cost of %d.\n\r", Mod, loc, cost );
    editor_send_to_char(C_DEFAULT, buf, ch);
    pObj->cost.gold += cost;
    return TRUE;
}
bool forge_type( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *pObj;
  char buf[ MAX_INPUT_LENGTH ];
  FORGE_OBJ(ch, pObj);
  if ( pObj->item_type != ITEM_WEAPON )
    {
    editor_send_to_char( AT_GREY, "You are not forging a weapon.\n\r", ch );
    return FALSE;
    }
  if ( argument[0] == '\0' )
    {
    sprintf( buf, "Legal values:\n\r%s %s %s %s %s %s %s\n\r",
	   weapon_flags[0].name, weapon_flags[1].name, weapon_flags[2].name,
	   weapon_flags[3].name, weapon_flags[4].name, weapon_flags[5].name,
	   weapon_flags[6].name );
    editor_send_to_char( AT_GREY, buf, ch );
    sprintf( buf, "%s %s %s %s %s %s %s\n\r",
	   weapon_flags[7].name, weapon_flags[8].name, weapon_flags[9].name,
	   weapon_flags[10].name, weapon_flags[11].name, weapon_flags[12].name,
	   weapon_flags[13].name );
    editor_send_to_char( AT_GREY, buf, ch );
    return FALSE;
    }
    if ( !str_cmp( 
	 flag_string( weapon_flags, flag_value( weapon_flags, argument ) ), 
	 "none" ) )
	{
	forge_type( ch, "" );
	return FALSE;
	}
    else
	{
	pObj->value[3] = flag_value( weapon_flags, argument );
	editor_send_to_char( AT_GREY, "Weapon type set.\n\r", ch );
	return TRUE;
	}
  return FALSE;
}

void do_rpstat( CHAR_DATA *ch, char *argument )
{
    TRAP_DATA *pTrap; 
    ROOM_INDEX_DATA *location;
    char             buf  [ MAX_STRING_LENGTH ];
    char             arg  [ MAX_INPUT_LENGTH  ];


    one_argument( argument, arg );
    location = ( arg[0] == '\0' ) ? ch->in_room : find_location( ch, arg );
    if ( !location )
    {
        editor_send_to_char(AT_RED, "No such location.\n\r", ch );
        return;
    }

    if ( !( location->traptypes) )
    {
        editor_send_to_char(AT_WHITE, "That room has no Programs set.\n\r", ch );
        return;
    }

    sprintf( buf, "&WName: [%s&W]\n\rArea: [%s&W]\n\r",
	     location->name, location->area->name );
    editor_send_to_char( AT_WHITE, buf, ch );
    sprintf( buf, "&WVnum: %d  &WSector: %d  &WLight: %d\n\r",
	     location->vnum, location->sector_type, location->light );
    editor_send_to_char( AT_WHITE, buf, ch );

    for ( pTrap = location->traps; pTrap; pTrap = pTrap->next_here )
    {
        sprintf(buf, ">%s %s\n\r%s\n\r\n\r",
            flag_string(rprog_types, pTrap->type), pTrap->arglist, pTrap->comlist );
        editor_send_to_char(C_DEFAULT, buf, ch );
    }
 
   return;

}

void do_opstat( CHAR_DATA *ch, char *argument )
{
    TRAP_DATA   *pTrap;
    OBJ_DATA    *obj;
    char         buf  [ MAX_STRING_LENGTH ];
    char         arg  [ MAX_INPUT_LENGTH  ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
        editor_send_to_char(AT_WHITE, "ObjProg stat what?\n\r", ch );
        return;
    }

    if ( !( obj = get_obj_world( ch, arg ) ) )
    {
        editor_send_to_char(AT_WHITE, "Nothing like that in hell, earth, or heaven.\n\r", ch);
        return;
    }

    if ( !( obj->pIndexData->traptypes ) )
    {
 	editor_send_to_char(AT_WHITE, "That object has no Programs set.\n\r", ch );
	return;
    }

    sprintf( buf, "Name: %s.\n\r",
            obj->name );
    editor_send_to_char(AT_RED, buf, ch);

    sprintf( buf, "Vnum: %d.  Type: %s.\n\r",
            obj->pIndexData->vnum, item_type_name( obj ) );
    editor_send_to_char(AT_RED, buf, ch);

    for ( pTrap = obj->pIndexData->traps; pTrap != NULL; pTrap = pTrap->next_here )
    {
      sprintf( buf, ">%s %s\n\r%s\n\r\n\r",
             flag_string(oprog_types, pTrap->type), 
	     pTrap->arglist, pTrap->comlist );
      editor_send_to_char(C_DEFAULT, buf, ch );
    }

    return;
}


bool mreset_show(CHAR_DATA *ch, char *argument)
{
  ROOM_INDEX_DATA *pRoom;
  RESET_DATA *pReset;
  MOB_INDEX_DATA *pMobIndex;
  char buf[MAX_STRING_LENGTH];
  int count = 0;

  EDIT_ROOM(ch, pRoom);

  sprintf( buf, "Mobile Reset: &z[&B%d&z] &W%s\n\r", pRoom->vnum, pRoom->name );
  editor_send_to_char( AT_YELLOW, buf, ch );
  editor_send_to_char( C_DEFAULT, "&z[ &G#&z] [&RMVnum&z] [&YNum&z] &BMob Short Description\n\r", ch );

  for ( pReset = pRoom->reset_first; pReset; pReset = pReset->next )
  {
    if ( pReset->command == 'M' )
    {
      pMobIndex = get_mob_index( pReset->arg1 );
      sprintf( buf, "&z[&G%2d&z] [&R%5d&z] [&Y%3d&z] &B%s\n\r", 
               count++, pReset->arg1, pReset->arg2,
               pMobIndex ? pMobIndex->short_descr : "&RMob does not exist!!");
      editor_send_to_char( C_DEFAULT, buf, ch );
    }
  }
  return FALSE;  
}

/*
 *  Race editor by Decklarean
 */
bool race_edit_show(CHAR_DATA *ch, char *argument)
{
  RACE_DATA *pRace;
  char buf[MAX_STRING_LENGTH];

  EDIT_RACE(ch, pRace);

  sprintf( buf, "&WFull:        &z[&W%3d&z][&W&W%-20s&z]\n\r",
   pRace->vnum, pRace->race_full );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&WName:        &z[&W%3s&z] ", pRace->race_name );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&WPolymorph:       &z[&W%2d&z] " 
                "&WSize:   &z[&W%s&z]\n\r",
   pRace->polymorph, flag_string( size_flags, pRace->size) );
  editor_send_to_char( AT_WHITE, buf, ch );

  sprintf( buf,
"&P------------------------------&GStats&P-----------------------------------&X\n\r"
);
  editor_send_to_char( C_DEFAULT, buf, ch );

  sprintf( buf, "&WClaws:       &z[&W%2d&z] ", pRace->claws );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&WGills:       &z[&W%2d&z] ", pRace->gills );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&WFlying:      &z[&W%2d&z] ", pRace->flying );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&WSwimming:    &z[&W%2d&z]\n\r", pRace->swimming );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&WInfrared:    &z[&W%2d&z] ", pRace->infrared );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&WTrue Sight:  &z[&W%2d&z] ", pRace->truesight );
  editor_send_to_char( AT_WHITE, buf, ch );
  sprintf( buf, "&WAcidBlood:   &z[&W%2d&z]\n\r", pRace->acidblood );
  editor_send_to_char( AT_WHITE, buf, ch );

  sprintf( buf, "&YMStr:        &z[&W%2d&z] ", pRace->mstr );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&YMInt:        &z[&W%2d&z] ", pRace->mint );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&YMWis:        &z[&W%2d&z] ", pRace->mwis );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&YMDex:        &z[&W%2d&z]\n\r", pRace->mdex );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&BMCon:        &z[&W%2d&z] ", pRace->mcon );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&BMAgi:        &z[&W%2d&z] ", pRace->magi );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&BMCha:        &z[&W%2d&z]\n\r", pRace->mcha );
  editor_send_to_char( C_DEFAULT, buf, ch );


  sprintf( buf,
"&P-----------------------------&GImmunity&P---------------------------------&X\n\r" );
  editor_send_to_char( C_DEFAULT, buf, ch );

  sprintf( buf, "&Gheat:            &z[&W%4d&z] ", pRace->mimm[0] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Gpositive:        &z[&W%4d&z] ", pRace->mimm[1] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Gcold:           &z[&W%4d&z]\n\r", pRace->mimm[2] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Cnegative:        &z[&W%4d&z] ", pRace->mimm[3] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Choly:            &z[&W%4d&z] ", pRace->mimm[4] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Cunholy:         &z[&W%4d&z]\n\r", pRace->mimm[5] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Gregen:           &z[&W%4d&z] ", pRace->mimm[6] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Gdegen:           &z[&W%4d&z] ", pRace->mimm[7] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Gdynamic:        &z[&W%4d&z]\n\r", pRace->mimm[8] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Cvoid:            &z[&W%4d&z] ", pRace->mimm[9] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Cpierce:          &z[&W%4d&z] ", pRace->mimm[10] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Cslash:          &z[&W%4d&z]\n\r", pRace->mimm[11] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Gscratch:         &z[&W%4d&z] ", pRace->mimm[12] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Gbash:            &z[&W%4d&z] ", pRace->mimm[13] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Ginternal:       &z[&W%4d&z]\n\r", pRace->mimm[14] );
  editor_send_to_char( C_DEFAULT, buf, ch );

  sprintf( buf,
"&P-----------------------------&GLanguage&P---------------------------------&X\n\r" );
  editor_send_to_char( C_DEFAULT, buf, ch );

  sprintf( buf, "&Rhuman:           &z[&W%4d&z] ", pRace->language[0] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Relf:             &z[&W%4d&z] ", pRace->language[1] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Rdwarf:          &z[&W%4d&z]\n\r", pRace->language[2] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Bquicksilver:     &z[&W%4d&z] ", pRace->language[3] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Bmaudlin:         &z[&W%4d&z] ", pRace->language[4]);
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Bpixie:          &z[&W%4d&z]\n\r", pRace->language[5] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Rfelixi:          &z[&W%4d&z] ", pRace->language[6] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Rdraconi:         &z[&W%4d&z] ", pRace->language[7] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Rgremlin:        &z[&W%4d&z]\n\r", pRace->language[8] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Bcentaur:         &z[&W%4d&z] ", pRace->language[9] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Bkender:          &z[&W%4d&z] ", pRace->language[10]);
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Bminotaur:       &z[&W%4d&z]\n\r", pRace->language[11] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Rdrow:            &z[&W%4d&z] ", pRace->language[12] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Raquinis:         &z[&W%4d&z] ", pRace->language[13] );
  editor_send_to_char( C_DEFAULT, buf, ch );
  sprintf( buf, "&Rtroll:          &z[&W%4d&z]\n\r", pRace->language[14] );
  editor_send_to_char( C_DEFAULT, buf, ch );


  return FALSE;
}

bool race_edit_mstr( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  mstr [number]\n\r", ch );
	return FALSE;
    }

    pRace->mstr = atoi( argument );

    editor_send_to_char(C_DEFAULT, "MStr set.\n\r", ch);
    return TRUE;
}

bool race_edit_mint( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  mint [number]\n\r", ch );
	return FALSE;
    }

    pRace->mint = atoi( argument );

    editor_send_to_char(C_DEFAULT, "MInt set.\n\r", ch);
    return TRUE;
}

bool race_edit_mwis( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  mwis [number]\n\r", ch );
	return FALSE;
    }

    pRace->mwis = atoi( argument );

    editor_send_to_char(C_DEFAULT, "MWis set.\n\r", ch);
    return TRUE;
}

bool race_edit_mcha( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  mcha [number]\n\r", ch );
	return FALSE;
    }

    pRace->mcha = atoi( argument );

    editor_send_to_char(C_DEFAULT, "MCha set.\n\r", ch);
    return TRUE;
}

bool race_edit_mdex( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  mdex [number]\n\r", ch );
	return FALSE;
    }

    pRace->mdex = atoi( argument );

    editor_send_to_char(C_DEFAULT, "MDex set.\n\r", ch);
    return TRUE;
}

bool race_edit_mcon( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  mcon [number]\n\r", ch );
	return FALSE;
    }

    pRace->mcon = atoi( argument );

    editor_send_to_char(C_DEFAULT, "MCon set.\n\r", ch);
    return TRUE;
}

bool race_edit_magi( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number( argument ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  magi [number]\n\r", ch );
	return FALSE;
    }

    pRace->magi = atoi( argument );

    editor_send_to_char(C_DEFAULT, "MAgi set.\n\r", ch);
    return TRUE;
}

bool race_edit_immunity( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;
    char arg[MAX_INPUT_LENGTH];
    char arg2[MAX_INPUT_LENGTH];
    int  immunity;
    int  value;

    EDIT_RACE(ch, pRace);

    argument = one_argument( argument, arg );
    argument = one_argument( argument, arg2 );

    if ( arg[0] == '\0' || arg2[0] == '\0' || !is_number( arg2 ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: immunity [immunity] [number]\n\r", ch );
	return FALSE;
    }

    immunity = flag_value( damage_flags, arg );

    if ( immunity <= -1 || immunity >= 15 )
    {
     editor_send_to_char( C_DEFAULT, "That isn't an immunity.\n\r", ch );
     return FALSE;
    }

    value = atoi( arg2 );

    pRace->mimm[immunity] = value;
    editor_send_to_char(C_DEFAULT, "Immunity set.\n\r", ch);
    return TRUE;

}

bool race_edit_language( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;
    char arg[MAX_INPUT_LENGTH];
    char arg2[MAX_INPUT_LENGTH];
    int  language;
    int  value;

    EDIT_RACE(ch, pRace);

    argument = one_argument( argument, arg );
    argument = one_argument( argument, arg2 );

    if ( arg[0] == '\0' || arg2[0] == '\0' || !is_number( arg2 ) )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: language [language] [number]\n\r", ch );
	return FALSE;
    }

    language = flag_value( language_types, arg );

    if ( language <= LANGUAGE_NONE || language >= MAX_LANGUAGE )
    {
     editor_send_to_char( C_DEFAULT, "That isn't a language.\n\r", ch );
     return FALSE;
    }

    value = atoi( arg2 );

    if ( value < 0 || value > 100 )
    {
     editor_send_to_char( C_DEFAULT, "Value must be between 0 and 100.\n\r", ch );
     return FALSE;
    }
    
    pRace->language[language] = value;
    editor_send_to_char(C_DEFAULT, "Language set.\n\r", ch);
    return TRUE;
}

bool race_edit_name( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || strlen( argument ) != 3 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  name [string] (string has to be 3 characters long)\n\r", ch );
	return FALSE;
    }

    free_string( pRace->race_name );
    pRace->race_name = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Name set.\n\r", ch);
    return TRUE;
}

bool race_edit_polymorph( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number(argument))
    {
        editor_send_to_char(C_DEFAULT, "Syntax: polymorph (0,1)\n\r", ch );
	return FALSE;
    }

    if ( atoi(argument) < 0 || atoi(argument) > 1 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: polymorph (0,1)\n\r", ch );
	return FALSE;
    }

    pRace->polymorph = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Polymorph set.\n\r", ch);
    return TRUE;
}

bool race_edit_gills( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number(argument))
    {
        editor_send_to_char(C_DEFAULT, "Syntax: gills (0,1)\n\r", ch );
	return FALSE;
    }

    if ( atoi(argument) < 0 || atoi(argument) > 1 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: gills (0,1)\n\r", ch );
	return FALSE;
    }

    pRace->gills = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Gills set.\n\r", ch);
    return TRUE;
}

bool race_edit_flying( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number(argument))
    {
        editor_send_to_char(C_DEFAULT, "Syntax: flying (0,1)\n\r", ch );
	return FALSE;
    }

    if ( atoi(argument) < 0 || atoi(argument) > 1 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: flying (0,1)\n\r", ch );
	return FALSE;
    }

    pRace->flying = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Flying set.\n\r", ch);
    return TRUE;
}

bool race_edit_swimming( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number(argument))
    {
        editor_send_to_char(C_DEFAULT, "Syntax: swimming (0,1)\n\r", ch );
	return FALSE;
    }

    if ( atoi(argument) < 0 || atoi(argument) > 1 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: swimming (0,1)\n\r", ch );
	return FALSE;
    }

    pRace->swimming = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Swimming set.\n\r", ch);
    return TRUE;
}

bool race_edit_infrared( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number(argument))
    {
        editor_send_to_char(C_DEFAULT, "Syntax: infrared (0,1)\n\r", ch );
	return FALSE;
    }

    if ( atoi(argument) < 0 || atoi(argument) > 1 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: infrared (0,1)\n\r", ch );
	return FALSE;
    }

    pRace->infrared = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Infrared set.\n\r", ch);
    return TRUE;
}

bool race_edit_truesight( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number(argument))
    {
        editor_send_to_char(C_DEFAULT, "Syntax: truesight (0,1)\n\r", ch );
	return FALSE;
    }

    if ( atoi(argument) < 0 || atoi(argument) > 1 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: truesight (0,1)\n\r", ch );
	return FALSE;
    }

    pRace->truesight = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Truesight set.\n\r", ch);
    return TRUE;
}

bool race_edit_acidblood( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || !is_number(argument))
    {
        editor_send_to_char(C_DEFAULT, "Syntax: acidblood (0,1)\n\r", ch );
	return FALSE;
    }

    if ( atoi(argument) < 0 || atoi(argument) > 1 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: acidblood (0,1)\n\r", ch );
	return FALSE;
    }

    pRace->acidblood = atoi( argument );

    editor_send_to_char(C_DEFAULT, "Acidblood set.\n\r", ch);
    return TRUE;
}

bool race_edit_claws( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;
    int	      value;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: claws  none/normal/poisoned/diseased\n\r", ch );
	return FALSE;
    }

    value = flag_value( claw_flags, argument );

    if ( value < 0 || value > 3 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: claws none, normal, poisoned, diseased\n\r", ch );
	return FALSE;
    }

    pRace->claws = value;

    editor_send_to_char(C_DEFAULT, "Claws set.\n\r", ch);
    return TRUE;
}

bool race_edit_size( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;
    int	      value;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: size\n\r", ch );
	return FALSE;
    }

    value = flag_value( size_flags, argument );

    if ( value < 0 || value > 7 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax: size\n\r", ch );
	return FALSE;
    }

    pRace->size = value;

    editor_send_to_char(C_DEFAULT, "Size set.\n\r", ch);
    return TRUE;
}

bool race_edit_full( CHAR_DATA *ch, char *argument )
{
    RACE_DATA *pRace;

    EDIT_RACE(ch, pRace);

    if ( argument[0] == '\0' || strlen( argument ) > 20 )
    {
        editor_send_to_char(C_DEFAULT, "Syntax:  full [string] (String can't be more than 20 characters long.)\n\r", ch );
	return FALSE;
    }

    free_string( pRace->race_full );
    pRace->race_full = str_dup( argument );

    editor_send_to_char(C_DEFAULT, "Name set.\n\r", ch);
    return TRUE;
}

bool race_edit_delete(CHAR_DATA *ch, char *argument)
{
  RACE_DATA *pRace;
  RACE_DATA *pMark;

  EDIT_RACE(ch, pRace);

  if(argument[0] != '\0')
  {
    editor_send_to_char(C_DEFAULT, "Type delete by itself.\n\r", ch);
    return FALSE;
  }

  if(pRace == first_race)
  { 
    first_race = pRace->next;
    free_race_data( pRace );

    ch->desc->pEdit = NULL;
    ch->desc->editor = 0;
    editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
    return TRUE;
  }

  for(pMark = first_race;pMark;pMark = pMark->next)
  {
    if(pRace == pMark->next)
    {
      pMark->next = pRace->next;
      free_race_data( pRace );

      ch->desc->pEdit = NULL;
      ch->desc->editor = 0;
      editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
      return TRUE;
    }
  }
  return FALSE;
}

bool qedit_delete(CHAR_DATA *ch, char *argument)
{
  QUEST_DATA *pQuest;
  QUEST_DATA *pMark;

  EDIT_QUEST(ch, pQuest);

  if(argument[0] != '\0')
  {
    editor_send_to_char(C_DEFAULT, "Type delete by itself.\n\r", ch);
    return FALSE;
  }

  if(pQuest == first_quest)
  { 
    first_quest = pQuest->next;
    free_quest_data( pQuest );

    ch->desc->pEdit = NULL;
    ch->desc->editor = 0;
    editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
    return TRUE;
  }

  for(pMark = first_quest;pMark;pMark = pMark->next)
  {
    if(pQuest == pMark->next)
    {
      pMark->next = pQuest->next;
      free_quest_data( pQuest );

      ch->desc->pEdit = NULL;
      ch->desc->editor = 0;
      editor_send_to_char(C_DEFAULT, "Deleted.\n\r", ch);
      return TRUE;
    }
  }
  return FALSE;
}
