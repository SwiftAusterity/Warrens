/***************************************************************************
 *  File: olc_save.c                                                       *
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
/* OLC_SAVE.C
 * This takes care of saving all the .are information.
 * Notes:
 * -If a good syntax checker is used for setting vnum ranges of areas
 *  then it would become possible to just cycle through vnums instead
 *  of using the iHash stuff and checking that the room or reset or
 *  mob etc is part of that area.
 */

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
#include "merc.h"
#include "olc.h"



/*****************************************************************************
 Name:		fix_string
 Purpose:	Returns a string without \r and ~.
 ****************************************************************************/
char *fix_string( const char *str )
{
    static char strfix[MAX_STRING_LENGTH];
    int i;
    int o;

    if ( str == NULL )
        return '\0';

    for ( o = i = 0; str[i+o] != '\0'; i++ )
    {
        if (str[i+o] == '\r' || str[i+o] == '~')
            o++;
        strfix[i] = str[i+o];
    }
    strfix[i] = '\0';
    return strfix;
}

/* This function exists for returning the name of a mobprog type when the
 * value of the type is given
 * TRI
 */

extern char *mprog_type_to_name( int type );
/*
{
   switch( type )
   {
    case IN_FILE_PROG:
      return "in_file_prog";
    case ACT_PROG:
      return "act_prog";
    case SPEECH_PROG:
      return "speech_prog";
    case RAND_PROG:
      return "rand_prog";
    case RAND_VICT_PROG:
      return "rand_vict_prog";
    case FIGHT_PROG:
      return "fight_prog";
    case HITPRCNT_PROG:
      return "hitprcnt_prog";
    case DEATH_PROG:
      return "death_prog";
    case ENTRY_PROG:
      return "entry_prog";
    case GREET_PROG:
      return "greet_prog";
    case GIVE_PROG:
      return "give_prog";
    case BRIBE_PROG:
      return "bribe_prog";
    default:
      return "error_prog";
  }
  return "error_prog";
}
*/

/* TRI
 * This proc is the one that is originally called, it handles going from
 * one mob to the next until there are no more mobs with mobprogs from
 * the current pArea to have their mobprogs saved
 */
/*
 * Heh.. totally redid it so that it <should> work.. :)
 * Sorry TRI!.. :)
 * -- Altrag
 */
void save_mobprogs( FILE *fp, MOB_INDEX_DATA *pMob )
{
  MPROG_DATA *mprog;

  if ( !pMob->mobprogs )
    return;
  
  for ( mprog = pMob->mobprogs; mprog != NULL; mprog = mprog->next )
  {
    if ( !str_cmp( mprog_type_to_name( mprog->type ), "error_prog" ) )
      continue;
    fprintf( fp, ">%s %s~\n", mprog_type_to_name( mprog->type ),
	    mprog->arglist );
    fprintf( fp, "%s~\n", fix_string( mprog->comlist ) );
  }
  fprintf( fp, "|\n" );
}

void save_traps( FILE *fp, TRAP_DATA *pFirst,
		 const struct flag_type *flags )
{
  TRAP_DATA *pTrap;

  if ( !pFirst )
    return;

  for ( pTrap = pFirst; pTrap; pTrap = pTrap->next_here )
  {
    if ( !str_cmp( flag_string( flags, pTrap->type ), "error_prog" ) ||
	 !str_cmp( flag_string( flags, pTrap->type ), "none" ) )
      continue;
    fprintf( fp, ">%s %s~\n", flag_string( flags, pTrap->type ),
	     pTrap->arglist );
    fprintf( fp, "%d\n", pTrap->disarmable );
    fprintf( fp, "%s~\n", fix_string( pTrap->comlist ) );
  }
  fprintf( fp, "|\n" );
}

/*****************************************************************************
 Name:		save_area_list
 Purpose:	Saves the listing of files to be loaded at startup.
 Called by:	do_asave(olc_save.c).
 ****************************************************************************/
void save_area_list()
{
    FILE *fp;
    AREA_DATA *pArea;
    char	area_list_file[MAX_INPUT_LENGTH];

    sprintf( area_list_file, "%s%s", AREA_DIR, AREA_LIST );

    if ( ( fp = fopen( area_list_file, "w" ) ) == NULL )
    {
	bug( "Save_area_list: fopen", 0 );
	perror( "area.lst" );
    }
    else
    {
	/*
	 * Add any help files that need to be loaded at
	 * startup to this section.
	 */
        fprintf( fp, "help.dat\n" );   /* all help files are here now-Deck */
	for( pArea = area_first; pArea; pArea = pArea->next )
	{
	    fprintf( fp, "%s\n", pArea->filename );
	}

	fprintf( fp, "$\n" );
	fclose( fp );
    }

    return;
}



/*****************************************************************************
 Name:		save_mobiles
 Purpose:	Save #MOBILES secion of an area file.
 Called by:	save_area(olc_save.c).
 ****************************************************************************/
void save_mobiles( FILE *fp, AREA_DATA *pArea )
{
    int vnum;
    MOB_INDEX_DATA *pMobIndex;

    fprintf( fp, "#MOBILES\n" );
    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pMobIndex = get_mob_index(vnum) ) )
	{
	    if ( pMobIndex->area == pArea )
            {
                fprintf( fp, "#%d\n",		pMobIndex->vnum );
                fprintf( fp, "%s~\n",		pMobIndex->player_name );
                fprintf( fp, "%s~\n",		pMobIndex->short_descr );
                fprintf( fp, "%s~\n",		fix_string( pMobIndex->long_descr ) );
                fprintf( fp, "%s~\n",		fix_string( pMobIndex->description ) );
                fprintf( fp, "%d ",		pMobIndex->act );
                fprintf( fp, "%ld ",		pMobIndex->affected_by );
                fprintf( fp, "%ld ", 		pMobIndex->affected_by2 );
                fprintf( fp, "%d S\n",		pMobIndex->alignment );
                fprintf( fp, "%d ",		pMobIndex->ally_vnum );
                fprintf( fp, "%d ",		pMobIndex->ally_level );
                fprintf( fp, "%d ",		pMobIndex->race_type );
                fprintf( fp, "%d ",		pMobIndex->fighting_style );
                fprintf( fp, "%d ",		pMobIndex->size );
                fprintf( fp, "%d ",		pMobIndex->level );
                fprintf( fp, "%d ",		pMobIndex->hitroll );

                fprintf( fp, "%d ",		pMobIndex->p_damp );
                fprintf( fp, "%d ",		pMobIndex->m_damp );
                fprintf( fp, "%d ",		pMobIndex->hit_modi );
                fprintf( fp, "%d\n",		pMobIndex->skin );

		fprintf( fp, "%d ",		pMobIndex->money.gold );
		fprintf( fp, "%d ", 		pMobIndex->money.silver );
		fprintf( fp, "%d ",		pMobIndex->money.copper );
                fprintf( fp, "%d\n",  		pMobIndex->sex );
		fprintf( fp, "%ld %ld %ld\n",	pMobIndex->imm_flags,
			                    	pMobIndex->res_flags,
			                    	pMobIndex->vul_flags );
		fprintf( fp, "%d %d %d %d %d %d %d\n",
		pMobIndex->perm_str,
		pMobIndex->perm_int,
		pMobIndex->perm_wis,
		pMobIndex->perm_dex,
		pMobIndex->perm_con,
		pMobIndex->perm_agi,
 	      pMobIndex->perm_cha );

		save_mobprogs( fp, pMobIndex );
            }
        }
    }
    fprintf( fp, "#0\n\n\n\n" );
    return;
}

void save_clans( )
{
    FILE *fp; 
    CLAN_DATA *pClan;

    if ( ( fp = fopen( "clan.dat", "w" ) ) == NULL )
    {
	bug( "Save_clans: fopen", 0 );
	perror( "area.lst" );
    }
    else
    {
	for( pClan = clan_first; pClan; pClan = pClan->next )
	{
	    fprintf( fp, "#%d\n",      pClan->vnum );
	    fprintf( fp, "%d %d %d\n",
		     pClan->bankaccount.gold,
		     pClan->bankaccount.silver,
		     pClan->bankaccount.copper ); 
	    fprintf( fp, "%s~\n",      pClan->name );
	    fprintf( fp, "%s~\n",      pClan->init );
	    fprintf( fp, "%s~\n",      pClan->warden );
	    fprintf( fp, "%s~\n",      pClan->description );
	    fprintf( fp, "%s~ %s~ %s~ %s~\n", 
	             pClan->champ, pClan->leader,
	             pClan->first, pClan->second );
	    fprintf( fp, "%d %d %d %d\n",
	             pClan->ischamp, pClan->isleader,
	             pClan->isfirst, pClan->issecond );
	    fprintf( fp, "%d\n",       pClan->recall );
	    fprintf( fp, "%d %d ",      pClan->pkills, pClan->mkills );
	    fprintf( fp, "%d\n",       pClan->members );
	    fprintf( fp, "%d %d\n",    pClan->pdeaths, pClan->mdeaths );
	    fprintf( fp, "%d %d %d\n", pClan->obj_vnum_1, pClan->obj_vnum_2, pClan->obj_vnum_3 );
	    fprintf( fp, "%d\n",     pClan->settings );
	}

	fprintf( fp, "#999\n\n" );
	fclose( fp );
    }
    return;
}

void save_race( )
{
    FILE      *fp;
    RACE_DATA *pRace;


    if ( ( fp = fopen( RACE_FILE, "w" ) ) == NULL ) 
    {
        bug( "Save_Race: fopen", 0 ); 
        perror( "Race.dat" ); 
    }

    for ( pRace = first_race;  pRace; pRace = pRace->next )
    {
       fprintf( fp, "VNum           %d\n", pRace->vnum      );
       fprintf( fp, "Polymorph      %d\n", pRace->polymorph );
       fprintf( fp, "Size	      %d\n", pRace->size );
       fprintf( fp, "Claws	      %d\n", pRace->claws );
       fprintf( fp, "Gills	      %d\n", pRace->gills );
       fprintf( fp, "Flying	      %d\n", pRace->flying );
       fprintf( fp, "Acidblood	%d\n", pRace->acidblood );
       fprintf( fp, "Truesight	%d\n", pRace->truesight );
       fprintf( fp, "Infrared		%d\n", pRace->infrared );
       fprintf( fp, "Swimming		%d\n", pRace->swimming );
       fprintf( fp, "Race_Full      %s~\n", pRace->race_full);
       if (pRace->race_name)
       fprintf( fp, "Race_Name      %s~\n", pRace->race_name);
       if (pRace->mstr)
       fprintf( fp, "MStr           %d\n", pRace->mstr     );
       if (pRace->mint)
       fprintf( fp, "MInt           %d\n", pRace->mint     );
       if (pRace->mwis)
       fprintf( fp, "MWis           %d\n", pRace->mwis     );
       if (pRace->mdex)
       fprintf( fp, "MDex           %d\n", pRace->mdex     );
       if (pRace->mcon)
       fprintf( fp, "MCon           %d\n", pRace->mcon     );
       if (pRace->magi)
       fprintf( fp, "MAgi           %d\n", pRace->magi     );
       if (pRace->mcha)
       fprintf( fp, "MCha           %d\n", pRace->mcha     );


       if (pRace->mimm[0])
       fprintf( fp, "MIheat         %d\n", pRace->mimm[0]     );
       if (pRace->mimm[1])
       fprintf( fp, "MIpositive     %d\n", pRace->mimm[1]     );
       if (pRace->mimm[2])
       fprintf( fp, "MIcold         %d\n", pRace->mimm[2]     );
       if (pRace->mimm[3])
       fprintf( fp, "MInegative     %d\n", pRace->mimm[3]     );
       if (pRace->mimm[4])
       fprintf( fp, "MIholy         %d\n", pRace->mimm[4]     );
       if (pRace->mimm[5])
       fprintf( fp, "MIunholy       %d\n", pRace->mimm[5]     );
       if (pRace->mimm[6])
       fprintf( fp, "MIregen        %d\n", pRace->mimm[6]     );
       if (pRace->mimm[7])
       fprintf( fp, "MIdegen        %d\n", pRace->mimm[7]     );
       if (pRace->mimm[8])
       fprintf( fp, "MIdynamic      %d\n", pRace->mimm[8]     );
       if (pRace->mimm[9])
       fprintf( fp, "MIvoid         %d\n", pRace->mimm[9]     );
       if (pRace->mimm[10])
       fprintf( fp, "MIpierce       %d\n", pRace->mimm[10]     );
       if (pRace->mimm[11])
       fprintf( fp, "MIslash        %d\n", pRace->mimm[11]     );
       if (pRace->mimm[12])
       fprintf( fp, "MIscratch      %d\n", pRace->mimm[12]     );
       if (pRace->mimm[13])
       fprintf( fp, "MIbash         %d\n", pRace->mimm[13]     );
       if (pRace->mimm[14])
       fprintf( fp, "MIinternal     %d\n", pRace->mimm[14]     );

       if (pRace->language[0])
       fprintf( fp, "LHuman         %d\n", pRace->language[0]  );
       if (pRace->language[1])
       fprintf( fp, "LElf           %d\n", pRace->language[1]  );
       if (pRace->language[2])
       fprintf( fp, "LDwarf         %d\n", pRace->language[2]  );
       if (pRace->language[3])
       fprintf( fp, "LQuicksilver   %d\n", pRace->language[3]  );
       if (pRace->language[4])
       fprintf( fp, "LMaudlin       %d\n", pRace->language[4]  );
       if (pRace->language[5])
       fprintf( fp, "LPixie         %d\n", pRace->language[5]  );
       if (pRace->language[6])
       fprintf( fp, "LFelixi        %d\n", pRace->language[6]  );
       if (pRace->language[7])
       fprintf( fp, "LDraconi       %d\n", pRace->language[7]  );
       if (pRace->language[8])
       fprintf( fp, "LGoblin        %d\n", pRace->language[8]  );
       if (pRace->language[9])
       fprintf( fp, "LCentaur       %d\n", pRace->language[9]  );
       if (pRace->language[10])
       fprintf( fp, "LKender        %d\n", pRace->language[10]  );
       if (pRace->language[11])
       fprintf( fp, "LMinotaur      %d\n", pRace->language[11]  );
       if (pRace->language[12])
       fprintf( fp, "LDrow          %d\n", pRace->language[12]  );
       if (pRace->language[13])
       fprintf( fp, "LAquinis       %d\n", pRace->language[13]  );
       if (pRace->language[14])
       fprintf( fp, "LTroll         %d\n", pRace->language[14]  );

      fprintf( fp, "END\n" ); 
    }
    fprintf( fp, "#END\n" ); 
    fclose( fp );
}


void save_social( )
{
    FILE *fp;
    SOCIAL_DATA *pSocial;

    if ( ( fp = fopen( SOCIAL_FILE, "w" ) ) == NULL )
    {
	bug( "Save_social: fopen", 0 );
	perror( "social.dat" );
    }
    else
    {
	for( pSocial = social_first; pSocial; pSocial = pSocial->next )
	{
	    fprintf( fp, "#%s~\n",     	pSocial->name 		);
	    fprintf( fp, "%s~\n", 	pSocial->char_no_arg	);
	    fprintf( fp, "%s~\n", 	pSocial->others_no_arg	);
	    fprintf( fp, "%s~\n", 	pSocial->char_found	);
	    fprintf( fp, "%s~\n", 	pSocial->others_found	);
	    fprintf( fp, "%s~\n", 	pSocial->vict_found	);
	    fprintf( fp, "%s~\n", 	pSocial->char_auto	);
	    fprintf( fp, "%s~\n", 	pSocial->others_auto	);
	}

	fprintf( fp, "#END~\n" );
	fclose( fp );
    }
    return;
}

/*****************************************************************************
 Name:		save_objects
 Purpose:	Save #OBJECTS section of an area file.
 Called by:	save_area(olc_save.c).
 ****************************************************************************/
void save_objects( FILE *fp, AREA_DATA *pArea )
{
    int vnum;
    OBJ_INDEX_DATA *pObjIndex;
    AFFECT_DATA *pAf;
    EXTRA_DESCR_DATA *pEd;

    fprintf( fp, "#OBJECTS\n" );
    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pObjIndex = get_obj_index(vnum) ) )
	{
	    if ( pObjIndex->area == pArea )
            {
                fprintf( fp, "#%d\n",    pObjIndex->vnum );
                fprintf( fp, "%s~\n",    pObjIndex->name );
                fprintf( fp, "%s~\n",    pObjIndex->short_descr );
                fprintf( fp, "%s~\n",    fix_string( pObjIndex->description ) );
                fprintf( fp, "%d ",      pObjIndex->item_type );
                fprintf( fp, "%d ",  pObjIndex->extra_flags );
                fprintf( fp, "%d ",  pObjIndex->wear_flags);
                fprintf( fp, "%d ",  pObjIndex->bionic_flags);
                fprintf( fp, "%d ",  pObjIndex->composition);
                fprintf( fp, "%d ",  pObjIndex->durability);
		fprintf( fp, "%d\n", pObjIndex->level );

                switch ( pObjIndex->item_type )
                {
                default:
                fprintf( fp, "%d~ %d~ %d~ %d~ %d~ %d~ %d~ %d~ %d~ %d~\n",
			pObjIndex->value[0],
			pObjIndex->value[1],
			pObjIndex->value[2],
			pObjIndex->value[3],
			pObjIndex->value[4],
			pObjIndex->value[5],
			pObjIndex->value[6],
			pObjIndex->value[7],
			pObjIndex->value[8],
			pObjIndex->value[9] );
                    break;

                case ITEM_PILL:
                case ITEM_POTION:
                case ITEM_SCROLL:
                fprintf( fp, "%d~ %s~ %s~ %s~ 0~ 0~ 0~ 0~ 0~ 0~\n",
			pObjIndex->value[0],
			pObjIndex->value[1] != -1 ?
			      skill_table[pObjIndex->value[1]].name
			    : " ",
			pObjIndex->value[2] != -1 ?
			      skill_table[pObjIndex->value[2]].name
			    : " ",
			pObjIndex->value[3] != -1 ?
			      skill_table[pObjIndex->value[3]].name
			    : " " );
                    break;

                case ITEM_STAFF:
                case ITEM_LENSE:
                case ITEM_WAND:
                fprintf( fp, "%d~ %d~ %d~ %s~ 0~ 0~ 0~ 0~ 0~ 0~\n",
			pObjIndex->value[0],
			pObjIndex->value[1],
			pObjIndex->value[2],
			pObjIndex->value[3] != -1 ?
			      skill_table[pObjIndex->value[3]].name
			    : " " );
                    break;
                }
                fprintf( fp, "%d ", pObjIndex->weight );
		fprintf( fp, "%d %d %d\n", pObjIndex->cost.gold,
		 pObjIndex->cost.silver, pObjIndex->cost.copper );
                fprintf( fp, "%d ", pObjIndex->invoke_type );
                fprintf( fp, "%d\n", pObjIndex->invoke_vnum );
                fprintf( fp, "%s~\n", pObjIndex->invoke_spell );
                fprintf( fp, "%d ", pObjIndex->invoke_charge[0] );
                fprintf( fp, "%d\n", pObjIndex->invoke_charge[1] );
		fprintf( fp, "%d %d %d\n", pObjIndex->join,
			 pObjIndex->sep_one, pObjIndex->sep_two );
                for( pAf = pObjIndex->affected; pAf; pAf = pAf->next )
                {
                 if ( !pAf || pAf->deleted )
                  break;

                 fprintf( fp, "A\n%d %d\n",  pAf->location, pAf->modifier );
                }

                for( pEd = pObjIndex->extra_descr; pEd; pEd = pEd->next )
                {
                 if ( !pEd || pEd->deleted )
                  break;
                 fprintf( fp, "E\n%s~\n%s~\n", pEd->keyword,
                  fix_string( pEd->description ) );
                }
		save_traps( fp, pObjIndex->traps, oprog_types );
            }
        }
    }
    fprintf( fp, "#0\n\n\n\n" );
    return;
}



/* OLC 1.1b */
/*****************************************************************************
 Name:		save_rooms
 Purpose:	Save #ROOMDATA section of an area file.
 Called by:	save_area(olc_save.c).
 ****************************************************************************/
void save_rooms( FILE *fp, AREA_DATA *pArea )
{
    ROOM_INDEX_DATA *pRoomIndex;
    EXTRA_DESCR_DATA *pEd;
    EXIT_DATA *pExit;
    int vnum;
    int door;

    fprintf( fp, "#ROOMDATA\n" );
    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pRoomIndex = get_room_index(vnum) ) )
	{
	    if ( pRoomIndex->area == pArea )
	    {
		fprintf( fp, "#%d\n", pRoomIndex->vnum );
		fprintf( fp, "%s~\n", pRoomIndex->name );
		fprintf( fp, "%s~\n", fix_string( pRoomIndex->description ) );
		toggle_raffects( pRoomIndex );
		fprintf( fp, "%d ", pRoomIndex->room_flags );
		toggle_raffects( pRoomIndex );
		fprintf( fp, "%d ", pRoomIndex->sector_type );
		fprintf( fp, "%d ", pRoomIndex->ore_type );
		fprintf( fp, "%d\n", pRoomIndex->ore_fertility );
		fprintf( fp, "Rd     %d\n", pRoomIndex->rd );

		for ( pEd = pRoomIndex->extra_descr; pEd; pEd = pEd->next )
                {
		    fprintf( fp, "E\n%s~\n%s~\n", pEd->keyword,
			fix_string( pEd->description ) );
                }

		for( door = 0; door < MAX_DIR; door++ )
		{
		    if ( ( pExit = pRoomIndex->exit[door] ) )
		    {
			fprintf( fp, "D%d\n", door );
			fprintf( fp, "%s~\n",
			    fix_string( pExit->description ) );
			fprintf( fp, "%s~\n", pExit->keyword );
			fprintf( fp, "%d %d %d\n", pExit->rs_flags,
			    pExit->key,
			    pExit->to_room ? pExit->to_room->vnum : 0 );
			save_traps( fp, pExit->traps, eprog_types );
		    }
		}
		fprintf( fp, "S\n" );
		save_traps( fp, pRoomIndex->traps, rprog_types );
	    }
	}
    }
    fprintf( fp, "#0\n\n\n\n" );
    return;
}


/* OLC 1.1b */
/*****************************************************************************
 Name:		save_specials
 Purpose:	Save #SPECIALS section of area file.
 Called by:	save_area(olc_save.c).
 ****************************************************************************/
void save_specials( FILE *fp, AREA_DATA *pArea )
{
    int vnum;
    MOB_INDEX_DATA *pMobIndex;
    
    fprintf( fp, "#SPECIALS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pMobIndex = get_mob_index(vnum) ) )
	{
	    if ( pMobIndex->area == pArea && pMobIndex->spec_fun )
            {
                fprintf( fp, "M %d %s\n", pMobIndex->vnum,
                                          spec_string( pMobIndex->spec_fun ) );
            }
        }
    }

    fprintf( fp, "S\n\n\n\n" );
    return;
}


/* OLC 1.1b */
/*****************************************************************************
 Name:		vsave_specials
 Purpose:	Save #SPECIALS section of area file.
 		New formating thanks to Rac.
 Called by:	save_area(olc_save.c).
 ****************************************************************************/
void vsave_specials( FILE *fp, AREA_DATA *pArea )
{
    int vnum;
    MOB_INDEX_DATA *pMobIndex;
    
    fprintf( fp, "#SPECIALS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pMobIndex = get_mob_index(vnum) ) )
	{
	    if ( pMobIndex->area == pArea && pMobIndex->spec_fun )
            {
                fprintf( fp, "M %d %s \t; %s\n", pMobIndex->vnum,
                                                 spec_string( pMobIndex->spec_fun ),
                                                 pMobIndex->short_descr );
            }
        }
    }

    fprintf( fp, "S\n\n\n\n" );
    return;
}



/* OLC 1.1b */
/*****************************************************************************
 Name:		save_resets
 Purpose:	Saves the #RESETS section of an area file.
                New format thanks to Rac.
 Called by:	save_area(olc_save.c)
 ****************************************************************************/
void save_resets( FILE *fp, AREA_DATA *pArea )
{
    RESET_DATA *pReset;
    MOB_INDEX_DATA *pLastMob = NULL;
    OBJ_INDEX_DATA *pLastObj;
    ROOM_INDEX_DATA *pRoomIndex;
    char buf[MAX_STRING_LENGTH];
    int vnum;

    fprintf( fp, "#RESETS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pRoomIndex = get_room_index(vnum) ) )
	{
	    if ( pRoomIndex->area == pArea )
	    {
    for ( pReset = pRoomIndex->reset_first; pReset; pReset = pReset->next )
    {
	switch ( pReset->command )
	{
	default:
	    bug( "Save_resets: bad command %c.", pReset->command );
	    break;

	case 'M':
            pLastMob = get_mob_index( pReset->arg1 );
	    fprintf( fp, "M %d %d %d\n", 
	        pReset->arg1,
                pReset->arg2,
                pReset->arg3 );
            break;

	case 'O':
            pLastObj = get_obj_index( pReset->arg1 );
	    fprintf( fp, "O %d %d %d\n", 
	        pReset->arg1, pReset->arg2,
                pReset->arg3 );
            break;

	case 'P':
            pLastObj = get_obj_index( pReset->arg1 );
	    fprintf( fp, "P %d %d %d\n", 
	        pReset->arg1, pReset->arg2,
                pReset->arg3  );
            break;

	case 'G':
	    fprintf( fp, "G %d %d\n", pReset->arg1, pReset->arg2 );
            if ( !pLastMob )
            {
                sprintf( buf,
                    "Save_resets: !NO_MOB! in [%s]", pArea->filename );
                bug( buf, 0 );
            }
            break;

	case 'E':
	    fprintf( fp, "E %d %d %d\n",
	        pReset->arg1, pReset->arg2,
                pReset->arg3 );
            if ( !pLastMob )
            {
                sprintf( buf,
                    "Save_resets: !NO_MOB! in [%s]", pArea->filename );
                bug( buf, 0 );
            }
            break;

	case 'D':
            break;

	case 'R':
	    fprintf( fp, "R %d %d\n", 
	        pReset->arg1,
                pReset->arg2 );
            break;
        }	/* End switch */
    }	/* End for pReset */
	    }	/* End if correct area */
	}	/* End if pRoomIndex */
    }	/* End for vnum */
    fprintf( fp, "S\n\n\n\n" );
    return;
}



/* OLC 1.1b */
/*****************************************************************************
 Name:		save_resets
 Purpose:	Saves the #RESETS section of an area file.
                New format thanks to Rac.
 Called by:	save_area(olc_save.c)
 ****************************************************************************/
void vsave_resets( FILE *fp, AREA_DATA *pArea )
{
    RESET_DATA *pReset;
    MOB_INDEX_DATA *pLastMob = NULL;
    OBJ_INDEX_DATA *pLastObj;
    ROOM_INDEX_DATA *pRoomIndex;
    char buf[MAX_STRING_LENGTH];
    int vnum;

    fprintf( fp, "#RESETS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pRoomIndex = get_room_index(vnum) ) )
	{
	    if ( pRoomIndex->area == pArea )
	    {
    for ( pReset = pRoomIndex->reset_first; pReset; pReset = pReset->next )
    {
	switch ( pReset->command )
	{
	default:
	    bug( "Save_resets: bad command %c.", pReset->command );
	    break;

	case 'M':
            pLastMob = get_mob_index( pReset->arg1 );
	    fprintf( fp, "M %d %2d %-5d \t; %s to %s\n", 
	        pReset->arg1,
                pReset->arg2,
                pReset->arg3,
                pLastMob->short_descr,
                pRoomIndex->name );
            break;

	case 'O':
            pLastObj = get_obj_index( pReset->arg1 );
	    fprintf( fp, "O %d  0 %-5d \t; %s to %s\n", 
	        pReset->arg1,
                pReset->arg3,
                capitalize(pLastObj->short_descr),
                pRoomIndex->name );
            break;

	case 'P':
            pLastObj = get_obj_index( pReset->arg1 );
	    fprintf( fp, "P %d  0 %-5d \t; %s inside %s\n", 
	        pReset->arg1,
                pReset->arg3,
                capitalize(get_obj_index( pReset->arg1 )->short_descr),
                pLastObj ? pLastObj->short_descr : "!NO_OBJ!" );

            if ( !pLastObj )	/* Thanks Rac! */
            {
                sprintf( buf, "Save_resets: P with !NO_OBJ! in [%s]", pArea->filename );
                bug( buf, 0 );
            }

            break;

	case 'G':
            pLastObj = get_obj_index( pReset->arg1 );
	    fprintf( fp, "G %d  0      \t;   %s\n",
	        pReset->arg1,
	        capitalize(pLastObj->short_descr) );

            if ( !pLastMob )
            {
                sprintf( buf, "Save_resets: !NO_MOB! in [%s]", pArea->filename );
                bug( buf, 0 );
            }
            break;

	case 'E':
	    fprintf( fp, "E %d  0 %-5d \t;   %s %s\n",
	        pReset->arg1,
                pReset->arg3,
                capitalize(get_obj_index( pReset->arg1 )->short_descr),
                flag_string( wear_loc_strings, pReset->arg3 ) );
            if ( !pLastMob )
            {
                sprintf( buf, "Save_resets: !NO_MOB! in [%s]", pArea->filename );
                bug( buf, 0 );
            }
            break;

	case 'D':
            break;

	case 'R':
	    fprintf( fp, "R %d %2d      \t; Randomize %s\n", 
	        pReset->arg1,
                pReset->arg2,
                pRoomIndex->name );
            break;
        }	/* End switch */
    }	/* End for pReset */
	    }	/* End if correct area */
	}	/* End if pRoomIndex */
    }	/* End for vnum */
    fprintf( fp, "S\n\n\n\n" );
    return;
}



/* OLC 1.1b */
/*****************************************************************************
 Name:		save_shops
 Purpose:	Saves the #SHOPS section of an area file.
 Called by:	save_area(olc_save.c)
 ****************************************************************************/
void save_shops( FILE *fp, AREA_DATA *pArea )
{
    SHOP_DATA *pShopIndex;
    MOB_INDEX_DATA *pMobIndex;
    int iTrade;
    int vnum;
    
    fprintf( fp, "#SHOPS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pMobIndex = get_mob_index(vnum) ) )
	{
	    if ( pMobIndex->area == pArea && pMobIndex->pShop )
            {
                pShopIndex = pMobIndex->pShop;

                fprintf( fp, "%d ", pShopIndex->keeper );
                for ( iTrade = 0; iTrade < MAX_TRADE; iTrade++ )
                {
                    if ( pShopIndex->buy_type[iTrade] != 0 )
                    {
                       fprintf( fp, "%d ", pShopIndex->buy_type[iTrade] );
                    }
                    else
                       fprintf( fp, "0 ");
                }
                fprintf( fp, "%d %d ", pShopIndex->profit_buy, pShopIndex->profit_sell );
                fprintf( fp, "%d %d\n", pShopIndex->open_hour, pShopIndex->close_hour );
            }
        }
    }

    fprintf( fp, "0\n\n\n\n" );
    return;
}



/* OLC 1.1b */
/*****************************************************************************
 Name:		vsave_shops
 Purpose:	Saves the #SHOPS section of an area file.
                New formating thanks to Rac.
 Called by:	save_area(olc_save.c)
 ****************************************************************************/
void vsave_shops( FILE *fp, AREA_DATA *pArea )
{
    SHOP_DATA *pShopIndex;
    MOB_INDEX_DATA *pMobIndex;
    int iTrade;
    int vnum;
    
    fprintf( fp, "#SHOPS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pMobIndex = get_mob_index(vnum) ) )
	{
	    if ( pMobIndex->area == pArea && pMobIndex->pShop )
            {
                pShopIndex = pMobIndex->pShop;

                fprintf( fp, "%d ", pShopIndex->keeper );
                for ( iTrade = 0; iTrade < MAX_TRADE; iTrade++ )
                {
                    if ( pShopIndex->buy_type[iTrade] != 0 )
                    {
                       fprintf( fp, "%2d ", pShopIndex->buy_type[iTrade] );
                    }
                    else
                       fprintf( fp, " 0 ");
                }
                fprintf( fp, "%3d %3d ", pShopIndex->profit_buy, pShopIndex->profit_sell );
                fprintf( fp, "%2d %2d ", pShopIndex->open_hour, pShopIndex->close_hour );
                fprintf( fp, "\t; %s\n", get_mob_index( pShopIndex->keeper )->short_descr );
            }
        }
    }

    fprintf( fp, "0\n\n\n\n" );
    return;
}

void save_casinos( FILE *fp, AREA_DATA *pArea )
{
    CASINO_DATA *pCasinoIndex;
    MOB_INDEX_DATA *pMobIndex;
    int vnum;
    
    fprintf( fp, "#CASINOS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pMobIndex = get_mob_index(vnum) ) )
	{
	    if ( pMobIndex->area == pArea && pMobIndex->pCasino )
            {
                pCasinoIndex = pMobIndex->pCasino;

                fprintf( fp, "%d ", pCasinoIndex->dealer );
                fprintf( fp, "%d ", pCasinoIndex->game );
                fprintf( fp, "%d ", pCasinoIndex->pot );
                fprintf( fp, "%d %d\n", pCasinoIndex->ante_min,
		 pCasinoIndex->ante_max );
            }
        }
    }

    fprintf( fp, "0\n\n\n\n" );
    return;
}



/* OLC 1.1b */
/*****************************************************************************
 Name:		vsave_casinos
 Purpose:	Saves the #SCASINOS section of an area file.
                New formating thanks to Rac.
 Called by:	save_area(olc_save.c)
 ****************************************************************************/
void vsave_casinos( FILE *fp, AREA_DATA *pArea )
{
    CASINO_DATA *pCasinoIndex;
    MOB_INDEX_DATA *pMobIndex;
    int vnum;
    
    fprintf( fp, "#CASINOS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pMobIndex = get_mob_index(vnum) ) )
	{
	    if ( pMobIndex->area == pArea && pMobIndex->pCasino )
            {
                pCasinoIndex = pMobIndex->pCasino;

                fprintf( fp, "%d ", pCasinoIndex->dealer );
                fprintf( fp, "%d ", pCasinoIndex->game );
                fprintf( fp, "%d ", pCasinoIndex->pot );
                fprintf( fp, "%d %d ", pCasinoIndex->ante_min,
		 pCasinoIndex->ante_max );
                fprintf( fp, "\t; %s\n", get_mob_index( pCasinoIndex->dealer )->short_descr );
            }
        }
    }

    fprintf( fp, "0\n\n\n\n" );
    return;
}

void save_tattoo_artists( FILE *fp, AREA_DATA *pArea )
{
    TATTOO_ARTIST_DATA *pTattooIndex;
    MOB_INDEX_DATA *pMobIndex;
    AFFECT_DATA *pAf;
    int vnum;
    
    fprintf( fp, "#ARTISTS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pMobIndex = get_mob_index(vnum) ) )
	{
	    if ( pMobIndex->area == pArea && pMobIndex->pTattoo )
            {
                pTattooIndex = pMobIndex->pTattoo;

                fprintf( fp, "%d ", pTattooIndex->artist );
		fprintf( fp, "%d %d %d ", pTattooIndex->cost.gold,
		 pTattooIndex->cost.silver, pTattooIndex->cost.copper );
                fprintf( fp, "%d %d\n", pTattooIndex->magic_boost,
                pTattooIndex->wear_loc );
                for( pAf = pTattooIndex->affected; pAf; pAf = pAf->next )
                {
                fprintf( fp, "A\n%d %d\n",  pAf->location, pAf->modifier );
                }

            }
        }
    }

    fprintf( fp, "0\n\n\n\n" );
    return;
}



/* OLC 1.1b */
/*****************************************************************************
 Name:		vsave_tattoo_artists
 Purpose:	Saves the #ARTISTS section of an area file.
                New formating thanks to Rac.
 Called by:	save_area(olc_save.c)
 ****************************************************************************/
void vsave_tattoo_artists( FILE *fp, AREA_DATA *pArea )
{
    TATTOO_ARTIST_DATA *pTattooIndex;
    MOB_INDEX_DATA *pMobIndex;
    AFFECT_DATA *pAf;
    int vnum;
    
    fprintf( fp, "#ARTISTS\n" );

    for( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if( ( pMobIndex = get_mob_index(vnum) ) )
	{
	    if ( pMobIndex->area == pArea && pMobIndex->pTattoo )
            {
                pTattooIndex = pMobIndex->pTattoo;

                fprintf( fp, "%d ", pTattooIndex->artist );
		fprintf( fp, "%d %d %d ", pTattooIndex->cost.gold,
		 pTattooIndex->cost.silver, pTattooIndex->cost.copper );
                fprintf( fp, "%d %d\n", pTattooIndex->magic_boost,
                pTattooIndex->wear_loc );

                for( pAf = pTattooIndex->affected; pAf; pAf = pAf->next )
                {
                fprintf( fp, "A\n%d %d\n",  pAf->location, pAf->modifier );
                }
                fprintf( fp, "\t; %s\n", get_mob_index( pTattooIndex->artist )->short_descr );

            }
        }
    }

    fprintf( fp, "0\n\n\n\n" );
    return;
}

/*****************************************************************************
 Name:		save_helps
 Purpose:	Save #HELPS section of an area file.
 Written by:	Walker <nkrendel@evans.Denver.Colorado.EDU>
 Called by:	save_area(olc_save.c).
 ****************************************************************************/
/*
 *  This is the new save_helps it saves all the helps in help.dat
 *  -Decklarean
 */
void save_helps( )
{
    HELP_DATA *pHelp;
    FILE 	*fp;

    if ( !help_first )
     return;

    if ( ( fp = fopen( "help.dat", "w" ) ) == NULL )
    {
	bug( "Save_helps: fopen", 0 );
	perror( "help.dat" );
        return;
    }

    fprintf( fp, "#HELPS\n\n" );   

    for( pHelp = help_first; pHelp; pHelp = pHelp->next )
    {
	fprintf( fp, "%d %s~\n%s~\n", 
		pHelp->level,
		all_capitalize( pHelp->keyword ), 
		fix_string( pHelp->text ) );
    }

    fprintf( fp, "\n0 $~\n\n" );

    fprintf( fp, "#$\n" );

    fclose( fp );
    return;
}

/* This saves quests to the quest.dat file */
void save_quests( )
{
    QUEST_DATA *pQuest;
    FILE 	*fp;

    if ( ( fp = fopen( QUEST_FILE, "w" ) ) == NULL ) 
    {
        bug( "Save_Quest: fopen", 0 ); 
        perror( "Quest.dat" ); 
    }

    for ( pQuest = first_quest;  pQuest; pQuest = pQuest->next )
    {
	fprintf( fp, "Name		%s~\n", pQuest->name );
	fprintf( fp, "Vnum		%d\n", pQuest->vnum );

      fprintf( fp, "END\n" ); 
    }
  fprintf( fp, "#END\n" ); 

 fclose( fp );
 return;
}

/*****************************************************************************
 Name:		save_area
 Purpose:	Save an area, note that this format is new.
 Called by:	do_asave(olc_save.c).
 ****************************************************************************/
void save_area( AREA_DATA *pArea )
{
    FILE *fp;

  /* Now then, if the mud screwes up because of a bad area or otherwise
     value, only the temp gets screwed, and the original area is left
     intact -Flux */
    fclose( fpReserve );
    if ( !( fp = fopen( TEMP_AREA, "w" ) ) )
    {
	bug( "Open_area temporary file: fopen", 0 );
	perror( pArea->filename );
    }

    fprintf( fp, "#AREADATA\n" );
    fprintf( fp, "Name        %s~\n",        pArea->name );
    fprintf( fp, "Levels      %d %d\n",	     pArea->llevel, pArea->ulevel );
    fprintf( fp, "Builders    %s~\n",        fix_string( pArea->builders ) );
    fprintf( fp, "VNUMs       %d %d\n",      pArea->lvnum, pArea->uvnum );
    fprintf( fp, "Security    %d\n",         pArea->security );
    fprintf( fp, "Recall      %d\n",         pArea->recall );
    fprintf( fp, "Flags       %d\n",         pArea->area_flags );
/* Angi */
    fprintf( fp, "Color       %d\n",	     pArea->def_color );

    fprintf( fp, "Temporal    %d\n",		pArea->temporal );
    fprintf( fp, "Weather     %d\n",		pArea->weather );
    fprintf( fp, "Humidity    %d\n",		pArea->average_humidity );
    fprintf( fp, "Temp        %d\n",		pArea->average_temp );
    fprintf( fp, "Owner       %d\n",		pArea->owner );
    fprintf( fp, "End\n\n\n\n" );

    save_mobiles( fp, pArea );
    save_objects( fp, pArea );
    save_rooms( fp, pArea );

    if ( IS_SET(pArea->area_flags, AREA_VERBOSE) )	/* OLC 1.1b */
    {
	vsave_specials( fp, pArea );
	vsave_resets( fp, pArea );
	vsave_shops( fp, pArea );
	vsave_tattoo_artists( fp, pArea );
	vsave_casinos( fp, pArea );
    }
    else
    {
	save_specials( fp, pArea );
	save_resets( fp, pArea );
	save_shops( fp, pArea );
	save_tattoo_artists( fp, pArea );
	save_casinos( fp, pArea );
    }

    fprintf( fp, "#$\n" );
    fclose( fp );

    if ( !( fp = fopen( pArea->filename, "w" ) ) )
    {
	bug( "Open_area: fopen", 0 );
	perror( pArea->filename );
    }

    fprintf( fp, "#AREADATA\n" );
    fprintf( fp, "Name        %s~\n",        pArea->name );
    fprintf( fp, "Levels      %d %d\n",	     pArea->llevel, pArea->ulevel );
    fprintf( fp, "Builders    %s~\n",        fix_string( pArea->builders ) );
    fprintf( fp, "VNUMs       %d %d\n",      pArea->lvnum, pArea->uvnum );
    fprintf( fp, "Security    %d\n",         pArea->security );
    fprintf( fp, "Recall      %d\n",         pArea->recall );
    fprintf( fp, "Flags       %d\n",         pArea->area_flags );
/* Angi */
    fprintf( fp, "Color       %d\n",	     pArea->def_color );

    fprintf( fp, "Temporal    %d\n",		pArea->temporal );
    fprintf( fp, "Weather     %d\n",		pArea->weather );
    fprintf( fp, "Owner       %d\n",		pArea->owner );
    fprintf( fp, "End\n\n\n\n" );

    save_mobiles( fp, pArea );
    save_objects( fp, pArea );
    save_rooms( fp, pArea );

    if ( IS_SET(pArea->area_flags, AREA_VERBOSE) )	/* OLC 1.1b */
    {
	vsave_specials( fp, pArea );
	vsave_resets( fp, pArea );
	vsave_shops( fp, pArea );
	vsave_tattoo_artists( fp, pArea );
	vsave_casinos( fp, pArea );
    }
    else
    {
	save_specials( fp, pArea );
	save_resets( fp, pArea );
	save_shops( fp, pArea );
	save_tattoo_artists( fp, pArea );
	save_casinos( fp, pArea );
    }

    fprintf( fp, "#$\n" );
    fclose( fp );

    fpReserve = fopen( NULL_FILE, "r" );
    return;
}


/* OLC 1.1b */
/*****************************************************************************
 Name:		do_asave
 Purpose:	Entry point for saving area data.
 Called by:	interpreter(interp.c)
 ****************************************************************************/
void do_asave( CHAR_DATA *ch, char *argument )
{
    char arg1 [MAX_INPUT_LENGTH];
    AREA_DATA *pArea;
    int value;

    if ( !ch )       /*  Do an autosave */
    {
        log_string( "Autosaving changed areas.", CHANNEL_BUILD, -1 );
	save_area_list();
	for( pArea = area_first; pArea; pArea = pArea->next )
	{
	  if ( IS_SET( pArea->area_flags, AREA_CHANGED ) ) 
	    {
	       REMOVE_BIT( pArea->area_flags, AREA_CHANGED | AREA_ADDED );
		save_area( pArea );
	    }
	} 
	return;
    } 

    argument = one_argument( argument, arg1 );

    if ( arg1[0] == '\0' )
    {
    editor_send_to_char(C_DEFAULT, "Syntax:\n\r", ch );
    editor_send_to_char(C_DEFAULT, "  asave <vnum>    - saves a particular area\n\r",	 ch );
    editor_send_to_char(C_DEFAULT, "  asave list      - saves the area.lst file\n\r",	 ch );
    editor_send_to_char(C_DEFAULT, "  asave area      - saves the area being edited\n\r", ch );
    editor_send_to_char(C_DEFAULT, "  asave changed   - saves all changed zones\n\r",	 ch );
    editor_send_to_char(C_DEFAULT, "  asave world     - saves the world! (db dump)\n\r",	 ch );
    editor_send_to_char(C_DEFAULT, "  asave ^ verbose - saves in verbose mode\n\r", ch );
    editor_send_to_char(C_DEFAULT, "\n\r", ch );
        return;
    }

    /* Snarf the value (which need not be numeric). */
    value = atoi( arg1 );

    /* Save the area of given vnum. */
    /* ---------------------------- */

    if ( !( pArea = get_area_data( value ) ) && is_number( arg1 ) )
    {
	editor_send_to_char(C_DEFAULT, "That area does not exist.\n\r", ch );
	return;
    }

    if ( is_number( arg1 ) )
    {
	save_area_list();
	if ( !str_cmp( "verbose", argument ) )
	    SET_BIT( pArea->area_flags, AREA_VERBOSE );
	save_area( pArea );
	REMOVE_BIT( pArea->area_flags, AREA_VERBOSE | AREA_CHANGED | AREA_ADDED );
	act( C_DEFAULT, "Area $t saved.", ch, arg1, NULL, TO_CHAR );
	return;
    }

    /* Save the world, only authorized areas. */
    /* -------------------------------------- */

    if ( !str_cmp( "world", arg1 ) )
    {
	save_area_list();
	for( pArea = area_first; pArea; pArea = pArea->next )
	{
	    /* Builder must be assigned this area. */
	    if ( !str_cmp( "verbose", argument ) )
		SET_BIT( pArea->area_flags, AREA_VERBOSE );
	    save_area( pArea );
	    REMOVE_BIT( pArea->area_flags, AREA_CHANGED | AREA_ADDED | AREA_VERBOSE );
	}
	editor_send_to_char(C_DEFAULT, "You saved the world.\n\r", ch );
	save_clans( );
  	save_social( );
	save_race( );
	save_quests( );
        save_helps( );
	save_player_list( );
	return;
    }

    if ( !str_cmp( "clans", arg1 ) )
    {
      save_clans( );
      return;
    }

    if ( !str_cmp( "help", arg1 ) )
    {
      save_helps( );
      return;
    }
      
    if ( !str_cmp( "socials", arg1 ) )
    {
      save_social( );
      return;
    }
    if ( !str_cmp( "race", arg1 ) )
    {
      save_race( );
      return;
    }
    if ( !str_cmp( "quests", arg1 ) )
    {
      save_quests( );
      return;
    }

    /* Save changed areas, only authorized areas. */
    /* ------------------------------------------ */

    if ( !str_cmp( "changed", arg1 ) )
    {
	char buf[MAX_INPUT_LENGTH];

	save_area_list();

	editor_send_to_char(C_DEFAULT, "Saved zones:\n\r", ch );
	sprintf( buf, "None.\n\r" );

	for( pArea = area_first; pArea; pArea = pArea->next )
	{
	  /*   Builder must be assigned this area. */
	  /*   Save changed areas. */
	    if ( IS_SET(pArea->area_flags, AREA_CHANGED)
	      || IS_SET(pArea->area_flags, AREA_ADDED) )
	    {
		if ( !str_cmp( "verbose", argument ) )
		    SET_BIT( pArea->area_flags, AREA_VERBOSE );
		save_area( pArea );
		REMOVE_BIT( pArea->area_flags, AREA_CHANGED | AREA_ADDED | AREA_VERBOSE );
		sprintf( buf, "%24s - '%s'\n\r", pArea->name, pArea->filename );
		editor_send_to_char(C_DEFAULT, buf, ch );
	    }
        } 
	if ( !str_cmp( buf, "None.\n\r" ) )
	    editor_send_to_char(C_DEFAULT, buf, ch );
        return;
    }

    /* Save the area.lst file. */
    /* ----------------------- */
    if ( !str_cmp( arg1, "list" ) )
    {
	save_area_list();
	return;
    }

    /* Save area being edited, if authorized. */
    /* -------------------------------------- */
    if ( !str_cmp( arg1, "area" ) )
    {	
	/* Find the area to save. */
	switch (ch->desc->editor)
	{
	    case ED_AREA:
		pArea = (AREA_DATA *)ch->desc->pEdit;
		break;
	    case ED_ROOM:
		pArea = ch->in_room->area;
		break;
	    case ED_OBJECT:
		pArea = ( (OBJ_INDEX_DATA *)ch->desc->pEdit )->area;
		break;
	    case ED_MOBILE:
		pArea = ( (MOB_INDEX_DATA *)ch->desc->pEdit )->area;
		break;
	    default:
		pArea = ch->in_room->area;
		break;
	}

	save_area_list();
	if ( !str_cmp( "verbose", argument ) )
	    SET_BIT( pArea->area_flags, AREA_VERBOSE );
	save_area( pArea );
	REMOVE_BIT( pArea->area_flags, AREA_CHANGED | AREA_ADDED | AREA_VERBOSE );
	editor_send_to_char(C_DEFAULT, "Area saved.\n\r", ch );
	return;
    }

    /* Show correct syntax. */
    /* -------------------- */
   do_asave( ch, "" );
    return;
}
