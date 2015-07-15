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
#include "merc.h"

#if !defined( macintosh )
extern	int	_filbuf		args( (FILE *) );
#endif

#if defined( ultrix ) || defined( sequent )
void    system          args( ( char *string ) );
#endif

/*
 * Array of containers read for proper re-nesting of objects.
 */
#define MAX_NEST	100
static	OBJ_DATA *	rgObjNest	[ MAX_NEST ];

/*
 *  playerlist function  --Decklarean
 */

void update_playerlist  args( ( CHAR_DATA *ch ));

/*
 * Local functions.
 */
void	fwrite_char	args( ( CHAR_DATA *ch,  FILE *fp ) );
void	fwrite_obj	args( ( CHAR_DATA *ch,  OBJ_DATA  *obj,
			       FILE *fp, int iNest, bool storage ) );
void	fwrite_tattoo	args( ( CHAR_DATA *ch,  TATTOO_DATA  *tattoo,
			       FILE *fp ) );
void	fread_tattoo	args( ( CHAR_DATA *ch,  FILE *fp ) );
void	fread_char	args( ( CHAR_DATA *ch,  FILE *fp ) );
void	fread_obj	args( ( CHAR_DATA *ch,  FILE *fp, bool storage ) );
void    fread_pet       args( ( CHAR_DATA *ch, FILE *fp ) );
void    save_pet        args( ( CHAR_DATA *ch, FILE *fp, CHAR_DATA *pet ) );
void    add_alias       args( ( CHAR_DATA *ch, ALIAS_DATA *pAl, char *old,
				char *new ) );
void    fwrite_alias    args( ( CHAR_DATA *ch, FILE *fp ) );
void    fread_alias     args( ( CHAR_DATA *ch, FILE *fp ) );

/* Check to see if a player exists */
bool pstat( char *name )
{
  bool found;
  char strsave[256];
  FILE *fp;

  sprintf( strsave, "%s%c/%s", PLAYER_DIR, LOWER(name[0]),
       capitalize( name ) );
  
  found = ( fp = fopen( strsave, "r" ) ) != NULL;
  fclose( fp );
  fpReserve = fopen( NULL_FILE, "r" );
  return found;
}

/*
 *   Save a character and inventory.
 *   Would be cool to save NPC's too for quest purposes,
 *   some of the infrastructure is provided.
 */
void save_char_obj( CHAR_DATA *ch, bool leftgame )
{
    FILE *fp;
    CHAR_DATA *pet;
    char  buf     [ MAX_STRING_LENGTH ];
    char  strsave [ MAX_INPUT_LENGTH  ];

    if ( IS_NPC( ch ) /*|| ch->level < 2*/ )
	return;

    if ( ch->desc && ch->desc->original )
	ch = ch->desc->original;

    if (!IS_NPC( ch ) );
    update_playerlist( ch );

    ch->save_time = current_time;
    fclose( fpReserve );

    /* player files parsed directories by Yaz 4th Realm */
#if !defined( macintosh ) && !defined( MSDOS )
    sprintf( strsave, "%s%c/%s", PLAYER_DIR, LOWER(ch->name[0]),
	    capitalize( ch->name ) );
#else
    sprintf( strsave, "%s%s", PLAYER_DIR, capitalize( ch->name ) );
#endif
    if ( !( fp = fopen( strsave, "w" ) ) )
    {
        sprintf( buf, "Save_char_obj: fopen %s: ", ch->name );
	bug( buf, 0 );
	perror( strsave );
    }
    else
    {
      fwrite_char( ch, fp );
      if ( ch->carrying )
	fwrite_obj( ch, ch->carrying, fp, 0, FALSE );
      if ( !IS_NPC( ch ) && ch->pcdata->storage )
       	fwrite_obj( ch, ch->pcdata->storage, fp, 0, TRUE );

      if ( ch->tattoo )
      fwrite_tattoo( ch, ch->tattoo, fp );

      for ( pet = ch->in_room->people; pet; pet = pet->next_in_room )
      {
	if (IS_NPC( pet ) )
	  if ( IS_SET( pet->act, ACT_PET ) && ( pet->master == ch ) )
	  {
	    save_pet( ch, fp, pet );
	    break;
	  }
      }
      tail_chain();
      fprintf( fp, "#END\n" );
    }
    fclose( fp );

#if !defined( macintosh ) && !defined( MSDOS )
    if ( leftgame )
    {
        sprintf( buf, "gzip -1fq %s", strsave );
        system( buf );
    }
#endif

    fpReserve = fopen( NULL_FILE, "r" );
/*    tail_chain();*/
    return;
}



/*
 * Write the char.
 */
void fwrite_char( CHAR_DATA *ch, FILE *fp )
{
    AFFECT_DATA *paf;
    int          sn;

    fprintf( fp, "#%s\n", IS_NPC( ch ) ? "MOB" : "PLAYER"	);

    fprintf( fp, "Nm          %s~\n",	ch->name		);
    fprintf( fp, "Nm          %s~\n",	ch->oname		);
    fprintf( fp, "ShtDsc      %s~\n",	ch->short_descr		);
    fprintf( fp, "LngDsc      %s~\n",	ch->long_descr		);
    fprintf( fp, "Dscr        %s~\n",	ch->description		);
    fprintf( fp, "Prmpt       %s~\n",	ch->prompt		);
    fprintf( fp, "Spou        %s~\n",	ch->pcdata->spouse	);
    fprintf( fp, "Sx          %d\n",	ch->sex			);
    fprintf( fp, "Rce         %d\n",	ch->race		);
    fprintf( fp, "Port        %d\n",	ch->pcdata->port		);
    fprintf( fp, "Size        %d\n",	ch->size		);
    fprintf( fp, "Regen	      %d\n",	ch->pcdata->regen	);
    fprintf( fp, "Claws	      %d\n",	ch->pcdata->claws	);
    fprintf( fp, "Remort      %d\n",	ch->pcdata->remort	);
    fprintf( fp, "Wiznet      %d\n",    ch->wiznet		);
    fprintf( fp, "Clan        %d\n",    ch->clan                );
    fprintf( fp, "Clvl        %d\n",    ch->clev                );
    fprintf( fp, "Ctmr        %d\n",    ch->ctimer              );
    fprintf( fp, "Stun        %d %d %d %d %d\n",
	     ch->stunned[0], ch->stunned[1], ch->stunned[2],
	     ch->stunned[3], ch->stunned[4] );
    fprintf( fp, "WizLev      %d\n",    ch->wizinvis            );
    fprintf( fp, "ClkLev      %d\n",    ch->cloaked		);
    fprintf( fp, "Lvl         %d\n",	ch->level		);
    fprintf( fp, "Avatar      %d\n",	ch->pcdata->avatar	);
    fprintf( fp, "Antidisarm  %d\n",    ch->antidisarm          );
    fprintf( fp, "Trst        %d\n",	ch->trust		);
    fprintf( fp, "Security    %d\n",    ch->pcdata->security	);  /* OLC */
    fprintf( fp, "Wizbt       %d\n",	ch->wizbit		);
    fprintf( fp, "Playd       %d\n",
	ch->played + (int) ( current_time - ch->logon )		);
    fprintf( fp, "Note        %ld\n",   ch->last_note           );
    fprintf( fp, "Room        %d\n",
	    (  ch->in_room == get_room_index( ROOM_VNUM_LIMBO )
	     && ch->was_in_room )
	    ? ch->was_in_room->vnum
	    : ch->in_room->vnum );

    fprintf( fp, "HpMnMv    %d %d %d %d %d %d %d %d %d\n",
	ch->hit, ch->perm_hit, ch->mod_hit, ch->mana, ch->perm_mana,
	ch->mod_mana, ch->move, ch->perm_move, ch->mod_move );
    fprintf( fp, "Gold	      %d\n",	ch->money.gold		);
    fprintf( fp, "Silver      %d\n",  	ch->money.silver	);
    fprintf( fp, "Copper      %d\n",	ch->money.copper	);
    fprintf( fp, "Exp         %d\n",	ch->exp			);
    fprintf( fp, "Act         %d\n",    ch->act			);
    fprintf( fp, "AffdBy      %ld\n",	ch->affected_by		);
    fprintf( fp, "AffdBy2     %ld\n",    ch->affected_by2        ); 
    fprintf( fp, "ImmBits     %ld\n",    ch->imm_flags		);
    fprintf( fp, "ResBits     %ld\n",	ch->res_flags		);
    fprintf( fp, "VulBits     %ld\n",	ch->vul_flags		);
    /* Bug fix from Alander */
    fprintf( fp, "Pos         %d\n",
	    ch->position == POS_FIGHTING ? POS_STANDING : ch->position );

    fprintf( fp, "Prac        %d\n",	ch->practice		);
    fprintf( fp, "SavThr      %d\n",	ch->saving_throw	);
    fprintf( fp, "Align       %d\n",	ch->alignment		);
    fprintf( fp, "SAlign      %c\n",	ch->start_align		); 
    fprintf( fp, "Hit         %d\n",	ch->hitroll		);
    fprintf( fp, "Dam         %d\n",	ch->damroll		);
    fprintf( fp, "Pdamp       %d\n",	ch->p_damp		);
    fprintf( fp, "Mdamp       %d\n",	ch->m_damp		);
    fprintf( fp, "Wimp        %d\n",	ch->wimpy		);
    fprintf( fp, "Deaf        %d\n",	ch->deaf		);
    fprintf( fp, "Email	      %s~\n",   ch->pcdata->email	);
    fprintf( fp, "Plan	      %s~\n",   ch->pcdata->plan	);
    fprintf( fp, "Corpses     %d\n",    ch->pcdata->corpses	);
    
    if ( ch->cquestpnts  != 0 )
        fprintf( fp, "Cquestpnts  %d\n",        ch->cquestpnts          );

    if ( ch->questpoints != 0 )
        fprintf( fp, "QuestPnts   %d\n",        ch->questpoints         );
/* 30 */
    if ( IS_NPC( ch ) )
    {
	fprintf( fp, "Vnum        %d\n",	ch->pIndexData->vnum	);
    }
    else
    {
	fprintf( fp, "Paswd       %s~\n",	ch->pcdata->pwd		);
	fprintf( fp, "Bmfin       %s~\n",	ch->pcdata->bamfin	);
	fprintf( fp, "Bmfout      %s~\n",	ch->pcdata->bamfout	);
        fprintf( fp, "Bmfsee      %s~\n",       ch->pcdata->bamfusee	);
	fprintf( fp, "Restout     %s~\n",	ch->pcdata->restout	);
        fprintf( fp, "Restsee     %s~\n",       ch->pcdata->restusee	);
        fprintf( fp, "Trnsto      %s~\n",       ch->pcdata->transto	);
        fprintf( fp, "Trnsfrom    %s~\n",       ch->pcdata->transfrom	);
        fprintf( fp, "Trnvict     %s~\n",       ch->pcdata->transvict	);
        fprintf( fp, "Slyuc       %s~\n",       ch->pcdata->slayusee	);
        fprintf( fp, "Slyrm       %s~\n",       ch->pcdata->slayroom	);
        fprintf( fp, "Slyvict     %s~\n",       ch->pcdata->slayvict  	);
        fprintf( fp, "Afkmes      %s~\n",       ch->pcdata->afkchar     );
        fprintf( fp, "Walkin      %s~\n",       ch->pcdata->walkin      );
        fprintf( fp, "Walkout     %s~\n",       ch->pcdata->walkout     );
	fprintf( fp, "Bank   	  %d %d %d\n",	
		ch->pcdata->bankaccount.gold,
		ch->pcdata->bankaccount.silver,
		ch->pcdata->bankaccount.copper );
	fprintf( fp, "Ttle        %s~\n",	ch->pcdata->title	);
	fprintf( fp, "oTtle       %s~\n",	ch->pcdata->otitle	);
	fprintf( fp, "AtrPrm      %d %d %d %d %d %d %d\n",
		ch->perm_str,
		ch->perm_int,
		ch->perm_wis,
		ch->perm_dex,
		ch->perm_con,
		ch->perm_agi,
		ch->perm_cha );

        fprintf( fp, "Language    %d %d %d %d %d %d %d %d %d %d %d %d %d %d %d\n",
		ch->pcdata->language[LANGUAGE_HUMAN],
		ch->pcdata->language[LANGUAGE_ELF],
		ch->pcdata->language[LANGUAGE_DWARF],
		ch->pcdata->language[LANGUAGE_QUICKSILVER],
		ch->pcdata->language[LANGUAGE_MAUDLIN],
		ch->pcdata->language[LANGUAGE_PIXIE],
		ch->pcdata->language[LANGUAGE_FELIXI],
		ch->pcdata->language[LANGUAGE_DRACONI],
		ch->pcdata->language[LANGUAGE_GREMLIN],
		ch->pcdata->language[LANGUAGE_CENTAUR],
		ch->pcdata->language[LANGUAGE_KENDER],
		ch->pcdata->language[LANGUAGE_MINOTAUR],
		ch->pcdata->language[LANGUAGE_DROW],
		ch->pcdata->language[LANGUAGE_AQUINIS],
		ch->pcdata->language[LANGUAGE_TROLL] );

        fprintf( fp, "Weapon	  %d %d %d %d %d %d\n",
		ch->pcdata->weapon[0], 
		ch->pcdata->weapon[1], 
		ch->pcdata->weapon[2], 
		ch->pcdata->weapon[3], 
		ch->pcdata->weapon[4], 
		ch->pcdata->weapon[5] );

        fprintf( fp, "Morph	  %d %d %d %d %d\n",
		ch->pcdata->morph[0], 
		ch->pcdata->morph[1], 
		ch->pcdata->morph[2], 
		ch->pcdata->morph[3], 
		ch->pcdata->morph[4] );

        fprintf( fp, "Assimilate	 %d %d %d %d %d %d %d %d %d %d\n",
		ch->pcdata->assimilate[0], 
		ch->pcdata->assimilate[1], 
		ch->pcdata->assimilate[2], 
		ch->pcdata->assimilate[3], 
		ch->pcdata->assimilate[4], 
		ch->pcdata->assimilate[5], 
		ch->pcdata->assimilate[6], 
		ch->pcdata->assimilate[7], 
		ch->pcdata->assimilate[8], 
		ch->pcdata->assimilate[9] );

	fprintf( fp, "AtrMd       %d %d %d %d %d %d %d\n",
		ch->pcdata->mod_str, 
		ch->pcdata->mod_int, 
		ch->pcdata->mod_wis,
		ch->pcdata->mod_dex, 
		ch->pcdata->mod_con,
		ch->pcdata->mod_agi,
		ch->pcdata->mod_cha ); 
 

        fprintf( fp, "ImmPrm	%d %d %d %d %d %d %d %d %d %d %d %d %d %d %d\n",
         ch->pcdata->pimm[0],
         ch->pcdata->pimm[1], 
         ch->pcdata->pimm[2], 
         ch->pcdata->pimm[3], 
         ch->pcdata->pimm[4], 
         ch->pcdata->pimm[5], 
         ch->pcdata->pimm[6], 
         ch->pcdata->pimm[7],
         ch->pcdata->pimm[8], 
         ch->pcdata->pimm[9], 
         ch->pcdata->pimm[10], 
         ch->pcdata->pimm[11], 
         ch->pcdata->pimm[12], 
         ch->pcdata->pimm[13], 
         ch->pcdata->pimm[14] ); 

        fprintf( fp, "ImmMd	%d %d %d %d %d %d %d %d %d %d %d %d %d %d %d\n",
         ch->pcdata->mimm[0],
         ch->pcdata->mimm[1], 
         ch->pcdata->mimm[2], 
         ch->pcdata->mimm[3], 
         ch->pcdata->mimm[4], 
         ch->pcdata->mimm[5], 
         ch->pcdata->mimm[6], 
         ch->pcdata->mimm[7],
         ch->pcdata->mimm[8], 
         ch->pcdata->mimm[9], 
         ch->pcdata->mimm[10], 
         ch->pcdata->mimm[11], 
         ch->pcdata->mimm[12], 
         ch->pcdata->mimm[13], 
         ch->pcdata->mimm[14] ); 

        fprintf( fp, "StreeCraft %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[0],
         ch->skill_tree[1],
         ch->skill_tree[2],
         ch->skill_tree[3],
         ch->skill_tree[4],
         ch->skill_tree[5],
         ch->skill_tree[6],
         ch->skill_tree[7],
         ch->skill_tree[8] );

        fprintf( fp, "StreeEval %d %d %d %d %d %d\n",
         ch->skill_tree[9],
         ch->skill_tree[10],
         ch->skill_tree[11],
         ch->skill_tree[12],
         ch->skill_tree[13],
         ch->skill_tree[14] );

        fprintf( fp, "StreeTech %d %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[15],
         ch->skill_tree[16],
         ch->skill_tree[17],
         ch->skill_tree[18],
         ch->skill_tree[19],
         ch->skill_tree[20],
         ch->skill_tree[21],
         ch->skill_tree[22],
         ch->skill_tree[23],
         ch->skill_tree[24] );

        fprintf( fp, "StreeCovert %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[25],
         ch->skill_tree[26],
         ch->skill_tree[27],
         ch->skill_tree[28],
         ch->skill_tree[29],
         ch->skill_tree[30],
         ch->skill_tree[31],
         ch->skill_tree[32] );

        fprintf( fp, "StreePhysA %d %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[33],
         ch->skill_tree[34],
         ch->skill_tree[35],
         ch->skill_tree[36],
         ch->skill_tree[37],
         ch->skill_tree[38],
         ch->skill_tree[39],
         ch->skill_tree[40],
         ch->skill_tree[41],
         ch->skill_tree[42] );

        fprintf( fp, "StreePhysB %d %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[43],
         ch->skill_tree[44],
         ch->skill_tree[45],
         ch->skill_tree[46],
         ch->skill_tree[47],
         ch->skill_tree[48],
         ch->skill_tree[49],
         ch->skill_tree[50],
         ch->skill_tree[51],
         ch->skill_tree[52] );

        fprintf( fp, "StreePhysC %d %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[53],
         ch->skill_tree[54],
         ch->skill_tree[55],
         ch->skill_tree[56],
         ch->skill_tree[57],
         ch->skill_tree[58],
         ch->skill_tree[59],
         ch->skill_tree[60],
         ch->skill_tree[61],
         ch->skill_tree[62] );

        fprintf( fp, "StreePhysD %d %d %d %d %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[63],
         ch->skill_tree[64],
         ch->skill_tree[65],
         ch->skill_tree[66],
         ch->skill_tree[67],
         ch->skill_tree[68],
         ch->skill_tree[69],
         ch->skill_tree[70],
         ch->skill_tree[71],
         ch->skill_tree[72],
         ch->skill_tree[73],
         ch->skill_tree[74],
         ch->skill_tree[75] );

        fprintf( fp, "StreeMagicA %d %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[76],
         ch->skill_tree[77],
         ch->skill_tree[78],
         ch->skill_tree[79],
         ch->skill_tree[80],
         ch->skill_tree[81],
         ch->skill_tree[82],
         ch->skill_tree[83],
         ch->skill_tree[84],
         ch->skill_tree[85] );

        fprintf( fp, "StreeMagicB %d %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[86],
         ch->skill_tree[87],
         ch->skill_tree[88],
         ch->skill_tree[89],
         ch->skill_tree[90],
         ch->skill_tree[91],
         ch->skill_tree[92],
         ch->skill_tree[93],
         ch->skill_tree[94],
         ch->skill_tree[95] );

        fprintf( fp, "StreeMagicC %d %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[96],
         ch->skill_tree[97],
         ch->skill_tree[98],
         ch->skill_tree[99],
         ch->skill_tree[100],
         ch->skill_tree[101],
         ch->skill_tree[102],
         ch->skill_tree[103],
         ch->skill_tree[104],
         ch->skill_tree[105] );

        fprintf( fp, "StreeMagicD %d %d %d %d %d %d %d %d %d %d %d\n",
         ch->skill_tree[106],
         ch->skill_tree[107],
         ch->skill_tree[108],
         ch->skill_tree[109],
         ch->skill_tree[110],
         ch->skill_tree[111],
         ch->skill_tree[112],
         ch->skill_tree[113],
         ch->skill_tree[114],
         ch->skill_tree[115],
         ch->skill_tree[116] );

	fprintf( fp, "Cond        %d %d %d %d\n",
		ch->pcdata->condition[0],
		ch->pcdata->condition[1],
		ch->pcdata->condition[2],
		ch->pcdata->condition[3] );

	fprintf( fp, "Pglen       %d\n",   ch->pcdata->pagelen     );
	fprintf( fp, "Empower     %s~\n",  ch->pcdata->empowerments);
	fprintf( fp, "Detract     %s~\n",  ch->pcdata->detractments);

	for ( sn = 0; skill_table[sn].name[0] != '\0'; sn++ )
	{
	    if ( skill_table[sn].name && ch->pcdata->learned[sn] > 0 )
	    {
		fprintf( fp, "Skll        %d '%s'\n",
		    ch->pcdata->learned[sn], skill_table[sn].name );
	    }
	}
    }

    for ( paf = ch->affected; paf; paf = paf->next )
    {
        if ( paf->deleted )
	    continue;

	fprintf( fp, "Aff       %3d %3d %3d %3d %10d %3d\n",
		skill_table[paf->type].slot,
		paf->duration,
		paf->modifier,
		paf->location,
		paf->bitvector,
                paf->level );
    }
    for ( paf = ch->affected2; paf; paf = paf->next )
    {
        if ( paf->deleted )
	    continue;

	fprintf( fp, "Aff2       %3d %3d %3d %3d %10d %3d\n",
		skill_table[paf->type].slot,
		paf->duration,
		paf->modifier,
		paf->location,
		paf->bitvector,
                paf->level );
    }

    if ( ch->pcdata && ch->pcdata->alias_list )
    {
      ALIAS_DATA *pAl;

      for ( pAl = ch->pcdata->alias_list; pAl; pAl = pAl->next )
	fprintf( fp, "Alias      %s~ %s~\n",
		pAl->old, pAl->new );
    }
    
    fprintf( fp, "End\n\n" );
    return;

    fprintf( fp, "Titl %s~\n",		ch->pcdata->title   );

}

void fwrite_tattoo( CHAR_DATA *ch, TATTOO_DATA *tattoo, FILE *fp )
{
    AFFECT_DATA      *paf;

    if ( tattoo->next_content )
	fwrite_tattoo( ch, tattoo->next_content, fp );

    fprintf( fp, "#TATTOO\n" );
    fprintf( fp, "ShortDescr   %s~\n",	tattoo->short_descr	     );
    fprintf( fp, "WearLoc      %d\n",	tattoo->wear_loc             );
    fprintf( fp, "MagicBoost   %d\n",	tattoo->magic_boost          );

    for ( paf = tattoo->affected; paf; paf = paf->next )
    {
	fprintf( fp, "Affect       %d %d %d %d %d\n",
		skill_table[paf->type].slot,
		paf->duration,
		paf->modifier,
		paf->location,
		paf->bitvector );
    }

    fprintf( fp, "End\n\n" );

    tail_chain();
    return;
}


/*
 * Write an object and its contents.
 */
void fwrite_obj( CHAR_DATA *ch, OBJ_DATA *obj, FILE *fp, int iNest,
		 bool storage )
{
    AFFECT_DATA      *paf;
    EXTRA_DESCR_DATA *ed;

    /*
     *   Slick recursion to write lists backwards,
     *   so loading them will load in forwards order.
     */
    if ( obj->next_content )
	fwrite_obj( ch, obj->next_content, fp, iNest, storage );

    /*
     * Castrate storage characters.
     */
    if ( !obj || obj->deleted )
     return;

    if ( IS_SET( obj->extra_flags, ITEM_NOEXIT ) )
     return;

    if ( storage )
      fprintf( fp, "#STORAGE\n" );
    else
      fprintf( fp, "#OBJECT\n" );
    fprintf( fp, "Nest         %d\n",	iNest			     );
    fprintf( fp, "Name         %s~\n",	obj->name		     );
    fprintf( fp, "ShortDescr   %s~\n",	obj->short_descr	     );
    fprintf( fp, "Description  %s~\n",	obj->description	     );
    fprintf( fp, "Owned	       %s~\n",  obj->ownedby		     );
    fprintf( fp, "Vnum         %d\n",	obj->pIndexData->vnum	     );
    fprintf( fp, "ExtraFlags   %d\n",	obj->extra_flags	     );
    fprintf( fp, "WearFlags    %d\n",	obj->wear_flags		     );
    fprintf( fp, "WearLoc      %d\n",	obj->wear_loc		     );
    fprintf( fp, "BionicFlags  %d\n",	obj->bionic_flags	     );
    fprintf( fp, "BionicLoc    %d\n",	obj->bionic_loc		     );
    fprintf( fp, "ItemType     %d\n",	obj->item_type		     );
    fprintf( fp, "Weight       %d\n",	obj->weight		     );
    fprintf( fp, "Level        %d\n",	obj->level		     );
    fprintf( fp, "Timer        %d\n",	obj->timer		     );
    fprintf( fp, "Gold         %d\n",	obj->cost.gold		     );
    fprintf( fp, "Silver       %d\n",   obj->cost.silver	     );
    fprintf( fp, "Copper       %d\n",   obj->cost.copper	     );
    fprintf( fp, "Durability   %d\n",   obj->durability		     );
    fprintf( fp, "Composition  %d\n",   obj->composition	     );
    fprintf( fp, "Values       %d %d %d %d %d %d %d %d %d %d\n",
	obj->value[0], obj->value[1], obj->value[2], obj->value[3],
	obj->value[4], obj->value[5], obj->value[6], obj->value[7],
	obj->value[8], obj->value[9] );
    fprintf( fp, "Activates    %d %d %d %d\n",
        obj->invoke_type, obj->invoke_vnum, obj->invoke_charge[0], obj->invoke_charge[1] );
    fprintf( fp, "AcSpell      %s~\n",  obj->invoke_spell );
    switch ( obj->item_type )
    {
    case ITEM_POTION:
    case ITEM_SCROLL:
	if ( obj->value[1] > 0 )
	{
	    fprintf( fp, "Spell 1      '%s'\n", 
		skill_table[obj->value[1]].name );
	}

	if ( obj->value[2] > 0 )
	{
	    fprintf( fp, "Spell 2      '%s'\n", 
		skill_table[obj->value[2]].name );
	}

	if ( obj->value[3] > 0 )
	{
	    fprintf( fp, "Spell 3      '%s'\n", 
		skill_table[obj->value[3]].name );
	}

	break;

    case ITEM_PILL:
    case ITEM_STAFF:
    case ITEM_LENSE:
    case ITEM_WAND:
	if ( obj->value[3] > 0 )
	{
	    fprintf( fp, "Spell 3      '%s'\n", 
		skill_table[obj->value[3]].name );
	}

	break;
    }

    for ( paf = obj->affected; paf; paf = paf->next )
    {
	fprintf( fp, "Affect       %d %d %d %d %d\n",
		skill_table[paf->type].slot,
		paf->duration,
		paf->modifier,
		paf->location,
		paf->bitvector );
    }

    for ( ed = obj->extra_descr; ed; ed = ed->next )
    {
	fprintf( fp, "ExtraDescr   %s~ %s~\n",
		ed->keyword, ed->description );
    }

    fprintf( fp, "End\n\n" );

    if ( obj->contains )
	fwrite_obj( ch, obj->contains, fp, iNest + 1, storage );

    if ( obj->sheath )
	fwrite_obj( ch, obj->sheath, fp, iNest + 1, storage );

    tail_chain();
    return;
}

/*
 * Load a char and inventory into a new ch structure.
 */
bool load_char_obj( DESCRIPTOR_DATA *d, char *name )
{
           FILE      *fp;
    static PC_DATA    pcdata_zero;
	   CHAR_DATA *ch;
#if !defined( MSDOS )
	   char       buf     [ MAX_STRING_LENGTH ];
#endif
	   char       strsave [ MAX_INPUT_LENGTH ];
	   bool       found;
	  
    if ( !char_free )
    {
	ch				= alloc_perm( sizeof( *ch ));
    }
    else
    {
	ch				= char_free;
	char_free			= char_free->next;
    }
    clear_char( ch );

    if ( !pcdata_free )
    {
	ch->pcdata			= alloc_perm( sizeof( *ch->pcdata ));
    }
    else
    {
	ch->pcdata			= pcdata_free;
	pcdata_free			= pcdata_free->next;
    }
    *ch->pcdata				= pcdata_zero;

    d->character			= ch;
    ch->desc				= d;

    ch->name				= str_dup( name );
    ch->oname				= str_dup( name );
    
    ch->prompt                          = str_dup( "<%hhp %mm %vmv> " );
    ch->last_note                       = 0;
    ch->wiznet				= WIZ_PREFIX;
    ch->act				= PLR_COMBINE
					| PLR_PROMPT
					| PLR_ANSI
					| PLR_COMBAT;
    ch->pcdata->pwd			= str_dup( "" );
    ch->pcdata->bamfin			= str_dup( "" );
    ch->pcdata->bamfout			= str_dup( "" );
    ch->pcdata->walkin			= str_dup( "" );
    ch->pcdata->walkout			= str_dup( "" );
    ch->pcdata->bamfusee                = str_dup( "" );
    ch->pcdata->restout			= str_dup( "" );
    ch->pcdata->restusee                = str_dup( "" );
    ch->pcdata->transto                 = str_dup( "" );
    ch->pcdata->transfrom               = str_dup( "" );
    ch->pcdata->transvict               = str_dup( "" );
    ch->pcdata->slayusee                = str_dup( "" );
    ch->pcdata->slayroom                = str_dup( "" );
    ch->pcdata->slayvict                = str_dup( "" );
    ch->pcdata->afkchar                 = str_dup( "" );
    ch->pcdata->title			= str_dup( "" );
    ch->pcdata->otitle			= str_dup( "" );
    ch->perm_str			= 13;
    ch->perm_int			= 13; 
    ch->perm_wis			= 13;
    ch->perm_dex			= 13;
    ch->perm_con			= 13;
    ch->perm_agi			= 13;
    ch->perm_cha			= 0;
    ch->pcdata->condition[COND_THIRST]	= MAX_THIRST;  /*  48  */
    ch->pcdata->condition[COND_FULL]	= MAX_FULL;    /*  48  */
    ch->pcdata->condition[COND_INSOMNIA]= MAX_INSOMNIA;
    ch->pcdata->pagelen                 = 20;
    ch->pcdata->security		= 0;	/* OLC */

    ch->pcdata->switched                = FALSE;
    ch->combat_timer			= 0;	/* XOR */
    ch->hunttimer			= 0;
    ch->summon_timer			= 0;	/* XOR */
    ch->imm_flags			= 0;	/* XOR */
    ch->res_flags			= 0;	/* XOR */
    ch->vul_flags			= 0;	/* XOR */
    ch->pcdata->bankaccount.gold	= 0;
    ch->pcdata->bankaccount.silver	= 0;
    ch->pcdata->bankaccount.copper	= 0;
    ch->pcdata->alias_list		= NULL; /* TRI */
    ch->pcdata->corpses			= 0;
    ch->pcdata->empowerments		= str_dup("");
    ch->pcdata->detractments		= str_dup("");
    ch->pcdata->port			= d->port;



    found = FALSE;
    fclose( fpReserve );

    /* parsed player file directories by Yaz of 4th Realm */
    /* decompress if .gz file exists - Thx Alander */
#if !defined( macintosh ) && !defined( MSDOS )
    sprintf( strsave, "%s%c/%s.gz", PLAYER_DIR, LOWER(ch->name[0]),
	    capitalize( name ) );
    if ( ( fp = fopen( strsave, "r" ) ) )
    {
	fclose( fp );
	sprintf( buf, "gzip -dfq %s", strsave );
	system( buf );
    }
#endif


#if !defined( macintosh ) && !defined( MSDOS )
    sprintf( strsave, "%s%c/%s", PLAYER_DIR, LOWER(ch->name[0]),
	    capitalize( name ) );

#else   
    sprintf( strsave, "%s%s", PLAYER_DIR, capitalize( name ) );
#endif
    if ( ( fp = fopen( strsave, "r" ) ) )
    {
	int iNest;

	for ( iNest = 0; iNest < MAX_NEST; iNest++ )
	    rgObjNest[iNest] = NULL;

	found = TRUE;
	for ( ; ; )
	{
	    char  letter;
	    char *word;

	    letter = fread_letter( fp );
	    if ( letter == '*' )
	    {
		fread_to_eol( fp );
		continue;
	    }

	    if ( letter != '#' )
	    {
		bug( "Load_char_obj: # not found.", 0 );
		break;
	    }

	    word = fread_word( fp );
	         if ( !str_cmp( word, "PLAYER" ) ) fread_char ( ch, fp );
	    else if ( !str_cmp( word, "OBJECT" ) ) fread_obj ( ch, fp, FALSE );
	    else if ( !str_cmp( word, "TATTOO" ) ) fread_tattoo ( ch, fp );
	    else if ( !str_cmp( word, "PET"    ) ) fread_pet  ( ch, fp );
	    else if ( !str_cmp( word, "STORAGE" ) ) fread_obj ( ch, fp, TRUE );
	    else if ( !str_cmp( word, "END"    ) ) break;
	    else
	    {
		bug( "Load_char_obj: bad section.", 0 );
		break;
	    }
	}
	    
	fclose( fp );
    }

    fpReserve = fopen( NULL_FILE, "r" );
    return found;
}



/*
 * Read in a char.
 */

#if defined( KEY )
#undef KEY
#endif

#define KEY( literal, field, value )					\
				if ( !str_cmp( word, literal ) )	\
				{					\
				    field  = value;			\
				    fMatch = TRUE;			\
				    break;				\
				}

void fread_char( CHAR_DATA *ch, FILE *fp )
{
    char *word;
    char  buf [ MAX_STRING_LENGTH ];
    bool  fMatch;

    for ( ; ; )
    {
	word   = feof( fp ) ? "End" : fread_word( fp );
	fMatch = FALSE;

	switch ( UPPER( word[0] ) )
	{
	case '*':
	    fMatch = TRUE;
	    fread_to_eol( fp );
	    break;

	case 'A':
	    KEY( "Act",		ch->act,		fread_number( fp ) );
	    KEY( "AffdBy",	ch->affected_by,	fread_number( fp ) );
	    KEY( "AffdBy2",     ch->affected_by2,       fread_number( fp ) );
            KEY( "Afkmes",      ch->pcdata->afkchar,    fread_string( fp ) );
	    KEY( "Align",	ch->alignment,		fread_number( fp ) );
	    KEY( "Antidisarm",  ch->antidisarm,         fread_number( fp ) );
            KEY( "Avatar",	ch->pcdata->avatar,	fread_number( fp ) );	  

	    if ( !str_cmp( word, "Aff" ) )
	    {
		AFFECT_DATA *paf;

		if ( !affect_free )
		{
		    paf		= alloc_perm( sizeof( *paf ) );
		}
		else
		{
		    paf		= affect_free;
		    affect_free	= affect_free->next;
		}

		paf->type	= fread_number( fp );
		paf->type       = slot_lookup(paf->type);
		paf->duration	= fread_number( fp );
		paf->modifier	= fread_number( fp );
		paf->location	= fread_number( fp );
		paf->bitvector	= fread_number( fp );
		paf->level	= fread_number( fp );
		paf->deleted    = FALSE;
		paf->next	= ch->affected;
		ch->affected	= paf;
		if ( !is_sn(paf->type) || paf->type == 0 )
		  paf->deleted = TRUE;
		fMatch = TRUE;
		break;
	    }
	    if ( !str_cmp( word, "Aff2" ) )
	    {
		AFFECT_DATA *paf;

		if ( !affect_free )
		{
		    paf		= alloc_perm( sizeof( *paf ));
		}
		else
		{
		    paf		= affect_free;
		    affect_free	= affect_free->next;
		}

		paf->type	= fread_number( fp );
		paf->type       = slot_lookup(paf->type);
		paf->duration	= fread_number( fp );
		paf->modifier	= fread_number( fp );
		paf->location	= fread_number( fp );
		paf->bitvector	= fread_number( fp );
		paf->level	= fread_number( fp );
		paf->deleted    = FALSE;
		paf->next	= ch->affected2;
		ch->affected2	= paf;
		if ( !is_sn(paf->type) || paf->type == 0 )
		  paf->deleted = TRUE;
		fMatch = TRUE;
		break;
	    }

	    if ( !str_cmp( word, "Alias" ) )
	    {
		ALIAS_DATA *pAl;

		fMatch = TRUE;
		if ( !ch->pcdata )
		{
		  bug("Fread_char: Alias without pcdata",0);
		  fread_string(fp);
		  fread_string(fp);
		  break;
		}
		pAl = alloc_perm( sizeof( *pAl ) );
		pAl->old = fread_string(fp);
		pAl->new = fread_string(fp);
		pAl->next = ch->pcdata->alias_list;
		ch->pcdata->alias_list = pAl;
		break;
	    }

            if ( !strcmp( word, "Assimilate" ) )
            {
	      ch->pcdata->assimilate[0] = fread_number( fp );
	      ch->pcdata->assimilate[1] = fread_number( fp );
	      ch->pcdata->assimilate[2] = fread_number( fp );
	      ch->pcdata->assimilate[3] = fread_number( fp );
	      ch->pcdata->assimilate[4] = fread_number( fp );
	      ch->pcdata->assimilate[5] = fread_number( fp );
	      ch->pcdata->assimilate[6] = fread_number( fp );
	      ch->pcdata->assimilate[7] = fread_number( fp );
	      ch->pcdata->assimilate[8] = fread_number( fp );
	      ch->pcdata->assimilate[9] = fread_number( fp );
	      fMatch = TRUE;
	      break;
            }

	    if ( !str_cmp( word, "AtrMd"  ) )
	    {
		ch->pcdata->mod_str  = fread_number( fp );
		ch->pcdata->mod_int  = fread_number( fp );
		ch->pcdata->mod_wis  = fread_number( fp );
		ch->pcdata->mod_dex  = fread_number( fp );
		ch->pcdata->mod_con  = fread_number( fp );
		ch->pcdata->mod_agi  = fread_number( fp );
		ch->pcdata->mod_cha  = fread_number( fp );
		fMatch = TRUE;
		break;
	    }

	    if ( !str_cmp( word, "AtrPrm" ) )
	    {
		ch->perm_str = fread_number( fp );
		ch->perm_int = fread_number( fp );
		ch->perm_wis = fread_number( fp );
		ch->perm_dex = fread_number( fp );
		ch->perm_con = fread_number( fp );
		ch->perm_agi = fread_number( fp );
		ch->perm_cha = fread_number( fp );
		fMatch = TRUE;
		break;
	    }

        break;
	case 'B':
	    KEY( "Bmfin",	ch->pcdata->bamfin,	fread_string( fp ) );
	    KEY( "Bmfout",	ch->pcdata->bamfout,	fread_string( fp ) );
            KEY( "Bmfsee",      ch->pcdata->bamfusee,   fread_string( fp ) );
	    if ( !str_cmp( word, "Bank" ) )
	    {
		ch->pcdata->bankaccount.gold   = fread_number( fp );
	 	ch->pcdata->bankaccount.silver = fread_number( fp );
		ch->pcdata->bankaccount.copper = fread_number( fp );
	        fMatch = TRUE;
		break;
	    }
	    break;

	case 'C':
	    KEY( "Corpses",	ch->pcdata->corpses,	fread_number( fp ) );
	    KEY( "Clan",        ch->clan,               fread_number( fp ) );
            KEY( "Claws",	ch->pcdata->claws,	fread_number( fp ) );
	    KEY( "Clvl",        ch->clev,               fread_number( fp ) );
	    KEY( "Copper",	ch->money.copper,	fread_number( fp ) );
	    KEY( "Ctmr",        ch->ctimer,             fread_number( fp ) );
	    KEY( "ClkLev",      ch->cloaked,            fread_number( fp ) );
            KEY( "Cquestpnts",  ch->cquestpnts,         fread_number( fp ) );            

	    if ( !str_cmp( word, "Cond" ) )
	    {
		ch->pcdata->condition[0] = fread_number( fp );
		ch->pcdata->condition[1] = fread_number( fp );
		ch->pcdata->condition[2] = fread_number( fp );
		ch->pcdata->condition[3] = fread_number( fp );
		fMatch = TRUE;
		break;
	    }
	    break;

	case 'D':
	    KEY( "Dam", 	ch->damroll,		fread_number( fp ) );
	    KEY( "Deaf",	ch->deaf,		fread_number( fp ) );
	    KEY( "Detract",	ch->pcdata->detractments,fread_string(fp ) );
	    KEY( "Dscr",	ch->description,	fread_string( fp ) );
	    break;

	case 'E':
	    KEY( "Email",	ch->pcdata->email,	fread_string( fp ) );
	    KEY( "Empower",	ch->pcdata->empowerments,fread_string(fp ) );
	    if ( !str_cmp( word, "End" ) )
		return;
	    KEY( "Exp",		ch->exp,		fread_number( fp ) );
	    break;

	case 'G':
	    KEY( "Gold",	ch->money.gold,		fread_number( fp ) );
	    break;

	case 'H':
	    KEY( "Hit", 	ch->hitroll,		fread_number( fp ) );

	    if ( !str_cmp( word, "HpMnMv" ) )
	    {
		ch->hit		= fread_number( fp );
		ch->perm_hit	= fread_number( fp );
		ch->mod_hit	= fread_number( fp );
		ch->mana	= fread_number( fp );
		ch->perm_mana	= fread_number( fp );
		ch->mod_mana	= fread_number( fp );
		ch->move	= fread_number( fp );
		ch->perm_move	= fread_number( fp );
		ch->mod_move	= fread_number( fp );
		fMatch = TRUE;
		break;
	    }
	    break;
        case 'I':
	    KEY( "ImmBits",	ch->imm_flags,		fread_number( fp ) );

	 if ( !str_cmp( word, "ImmMd" ) )
	 {
         ch->pcdata->mimm[0]     = fread_number( fp );
         ch->pcdata->mimm[1]     = fread_number( fp );
         ch->pcdata->mimm[2]     = fread_number( fp );
         ch->pcdata->mimm[3]     = fread_number( fp );
         ch->pcdata->mimm[4]     = fread_number( fp );
         ch->pcdata->mimm[5]     = fread_number( fp );
         ch->pcdata->mimm[6]     = fread_number( fp );
         ch->pcdata->mimm[7]     = fread_number( fp );
         ch->pcdata->mimm[8]     = fread_number( fp );
         ch->pcdata->mimm[9]     = fread_number( fp );
         ch->pcdata->mimm[10]     = fread_number( fp );
         ch->pcdata->mimm[11]     = fread_number( fp );
         ch->pcdata->mimm[12]     = fread_number( fp );
         ch->pcdata->mimm[13]     = fread_number( fp );
         ch->pcdata->mimm[14]     = fread_number( fp );

        		fMatch = TRUE;
		break;
	    }
	 if ( !str_cmp( word, "ImmPrm" ) )
	 {
         ch->pcdata->pimm[0]     = fread_number( fp );
         ch->pcdata->pimm[1]     = fread_number( fp );
         ch->pcdata->pimm[2]     = fread_number( fp );
         ch->pcdata->pimm[3]     = fread_number( fp );
         ch->pcdata->pimm[4]     = fread_number( fp );
         ch->pcdata->pimm[5]     = fread_number( fp );
         ch->pcdata->pimm[6]     = fread_number( fp );
         ch->pcdata->pimm[7]     = fread_number( fp );
         ch->pcdata->pimm[8]     = fread_number( fp );
         ch->pcdata->pimm[9]     = fread_number( fp );
         ch->pcdata->pimm[10]     = fread_number( fp );
         ch->pcdata->pimm[11]     = fread_number( fp );
         ch->pcdata->pimm[12]     = fread_number( fp );
         ch->pcdata->pimm[13]     = fread_number( fp );
         ch->pcdata->pimm[14]     = fread_number( fp );
		fMatch = TRUE;
		break;
	    }

            break;
	case 'L':
	    KEY( "Lvl", 	ch->level,		fread_number( fp ) );

            if ( !str_cmp( word, "Language" ) )
            {
	ch->pcdata->language[LANGUAGE_HUMAN] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_ELF] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_DWARF] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_QUICKSILVER] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_MAUDLIN] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_PIXIE] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_FELIXI] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_DRACONI] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_GREMLIN] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_CENTAUR] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_KENDER] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_MINOTAUR] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_DROW] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_AQUINIS] = fread_number( fp );
	ch->pcdata->language[LANGUAGE_TROLL] = fread_number( fp );

		fMatch = TRUE;
		break;
            }

	    if ( !str_cmp( word, "LngDsc" ) )
	    {
	      fread_to_eol( fp );
	      fMatch = TRUE;
	      break;
	    }
	    break;

	case 'M':
	    KEY( "Mdamp",	ch->m_damp,		fread_number( fp ) );

            if ( !strcmp( word, "Morph" ) )
            {
	      ch->pcdata->morph[0] = fread_number( fp );
	      ch->pcdata->morph[1] = fread_number( fp );
	      ch->pcdata->morph[2] = fread_number( fp );
	      ch->pcdata->morph[3] = fread_number( fp );
	      ch->pcdata->morph[4] = fread_number( fp );
	      fMatch = TRUE;
	      break;
            }
        break;

	case 'N':
	    if ( !str_cmp( word, "Nm" ) )
	    {
		/*
		 * Name already set externally.
		 */
		fread_to_eol( fp );
		fMatch = TRUE;
		break;
	    }
	    KEY( "Note",        ch->last_note,          fread_number( fp ) );
	    break;
        case 'O':
	    if ( !str_cmp( word, "oTtle" ) )
	    {
		ch->pcdata->otitle = fread_string( fp );
		if (   isalpha( ch->pcdata->otitle[0] )
		    || isdigit( ch->pcdata->otitle[0] ) )
		{
		    sprintf( buf, " %s", ch->pcdata->otitle );
		    free_string( ch->pcdata->otitle );
		    ch->pcdata->otitle = str_dup( buf );
		}
		fMatch = TRUE;
		break;
	    }

	    break;

	case 'P':
	    KEY( "Port",        ch->pcdata->port,       fread_number( fp ) );
	    KEY( "Pglen",       ch->pcdata->pagelen,    fread_number( fp ) );
	    KEY( "Paswd",	ch->pcdata->pwd,	fread_string( fp ) );
	    KEY( "Pdamp",	ch->p_damp,		fread_number( fp ) );
	    KEY( "Playd",	ch->played,		fread_number( fp ) );
            KEY( "Plan",	ch->pcdata->plan,	fread_string( fp ) );
	    KEY( "Pos", 	ch->position,		fread_number( fp ) );
	    KEY( "Prac",	ch->practice,		fread_number( fp ) );
	    KEY( "Prmpt",	ch->prompt,		fread_string( fp ) );
	    break;

        case 'Q':
            KEY( "QuestPnts",   ch->questpoints,        fread_number( fp ) );
            break;

	case 'R':
	    KEY( "Rce",         ch->race,		fread_number( fp ) );
	    KEY( "ResBits",	ch->res_flags,		fread_number( fp ) );
	    KEY( "Remort", 	ch->pcdata->remort,     fread_number( fp ) );
          KEY( "Regen",		ch->pcdata->regen,	fread_number( fp ) );
          KEY( "Restsee",	ch->pcdata->restusee,	fread_string( fp ) );
          KEY( "Restout",	ch->pcdata->restout,	fread_string( fp ) );

	    if ( !str_cmp( word, "Room" ) )
	    {
		ch->in_room = get_room_index( fread_number( fp ) );
		if ( !ch->in_room )
		    ch->in_room = get_room_index( ROOM_VNUM_LIMBO );
		fMatch = TRUE;
		break;
	    }

	    break;

	case 'S':
	    KEY( "SavThr",	ch->saving_throw,	fread_number( fp ) );
	    KEY( "Security",    ch->pcdata->security,	fread_number( fp ) );	/* OLC */
	    KEY( "Size",        ch->size,		fread_number( fp ) );
	    KEY( "SAlign",	ch->start_align,	fread_letter( fp ) );
	    KEY( "Sx",		ch->sex,		fread_number( fp ) );
	    KEY( "Silver",	ch->money.silver,	fread_number( fp ) );
            KEY( "Slyuc",	ch->pcdata->slayusee,	fread_string( fp ) );
            KEY( "Slyrm",	ch->pcdata->slayroom,	fread_string( fp ) );
	    KEY( "Slyvict",	ch->pcdata->slayvict,	fread_string( fp ) );
            KEY( "Spou",	ch->pcdata->spouse,	fread_string( fp ) );

	    if ( !str_cmp( word, "Stun" ) )
	    {
	      ch->stunned[0] = fread_number( fp );
	      ch->stunned[1] = fread_number( fp );
	      ch->stunned[2] = fread_number( fp );
	      ch->stunned[3] = fread_number( fp );
	      ch->stunned[4] = fread_number( fp );
	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "ShtDsc" ) )
	    {
	      fread_to_eol( fp );
	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "Skll" ) )
	    {
		int sn;
		int value;
		
		value = fread_number( fp );
		sn    = skill_lookup( fread_word( fp ) );
		if ( sn < 0 )
		    bug( "Fread_char: unknown skill.", 0 );
		else
		    ch->pcdata->learned[sn] = value;
		fMatch = TRUE;
	    }

/* Skills tree reading, long long long -Flux */

	    if ( !str_cmp( word, "StreeCraft" ) )
	    {
             int counter;

             for ( counter = 0 ; counter < 9; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreeEval" ) )
	    {
             int counter;

             for ( counter = 9; counter < 15; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreeTech" ) )
	    {
             int counter;

             for ( counter = 15; counter < 25; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreeCovert" ) )
	    {
             int counter;

             for ( counter = 25; counter < 33; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreePhysA" ) )
	    {
             int counter;

             for ( counter = 33; counter < 43; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreePhysB" ) )
	    {
             int counter;

             for ( counter = 43; counter < 53; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreePhysC" ) )
	    {
             int counter;

             for ( counter = 53; counter < 63; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreePhysD" ) )
	    {
             int counter;

             for ( counter = 63; counter < 76; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreeMagicA" ) )
	    {
             int counter;

             for ( counter = 76; counter < 86; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreeMagicB" ) )
	    {
             int counter;

             for ( counter = 86; counter < 96; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreeMagicC" ) )
	    {
             int counter;

             for ( counter = 96; counter < 106; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    if ( !str_cmp( word, "StreeMagicD" ) )
	    {
             int counter;

             for ( counter = 106; counter < 117; counter += 1 )
	      ch->skill_tree[counter] = fread_number( fp );

	      fMatch = TRUE;
	      break;
	    }

	    break;

	case 'T':
	    KEY( "Trst",	ch->trust,		fread_number( fp ) );
	    KEY( "Trnsto",	ch->pcdata->transto,	fread_string( fp ) );
	    KEY( "Trnsfrom",	ch->pcdata->transfrom,	fread_string( fp ) );
	    KEY( "Trnvict",	ch->pcdata->transvict, 	fread_string( fp ) );
                
	    if ( !str_cmp( word, "Ttle" ) )
	    {
		ch->pcdata->title = fread_string( fp );
		if (   isalpha( ch->pcdata->title[0] )
		    || isdigit( ch->pcdata->title[0] ) )
		{
		    sprintf( buf, " %s", ch->pcdata->title );
		    free_string( ch->pcdata->title );
		    ch->pcdata->title = str_dup( buf );
		}
		fMatch = TRUE;
		break;
	    }

	    break;

	case 'V':
	    KEY( "VulBits",	ch->vul_flags,	fread_number( fp ) );

	    if ( !str_cmp( word, "Vnum" ) )
	    {
		ch->pIndexData = get_mob_index( fread_number( fp ) );
		fMatch = TRUE;
		break;
	    }
	    break;

	case 'W':
            KEY( "Walkin",      ch->pcdata->walkin,    fread_string( fp ) );
            KEY( "Walkout",     ch->pcdata->walkout,    fread_string( fp ) );
	    KEY( "Wiznet",	ch->wiznet,		fread_number( fp ) );
	    KEY( "Wimp",	ch->wimpy,		fread_number( fp ) );
	    KEY( "Wizbt",	ch->wizbit,		fread_number( fp ) );
	    KEY( "WizLev",      ch->wizinvis,           fread_number( fp ) );

            if ( !strcmp( word, "Weapon" ) )
            {
	      ch->pcdata->weapon[0] = fread_number( fp );
	      ch->pcdata->weapon[1] = fread_number( fp );
	      ch->pcdata->weapon[2] = fread_number( fp );
	      ch->pcdata->weapon[3] = fread_number( fp );
	      ch->pcdata->weapon[4] = fread_number( fp );
	      ch->pcdata->weapon[5] = fread_number( fp );
	      fMatch = TRUE;
	      break;
            }
 	    break;
	}

	/* Make sure old chars have this field - Kahn */
	if ( !ch->pcdata->pagelen )
	    ch->pcdata->pagelen = 20;
	if ( !ch->prompt || ch->prompt == '\0' )
	    ch->prompt = str_dup ( "<%hhp %mm %vmv> " );

	/* Make sure old chars do not have pagelen > 60 - Kahn */
	if ( ch->pcdata->pagelen > 60 )
	    ch->pcdata->pagelen = 60;

	if ( !fMatch )
	{
	    sprintf( buf, "Fread_char: %s -> no match.", word );
	    bug( buf, 0 );
	    fread_to_eol( fp );
	}
    }
}



void fread_tattoo( CHAR_DATA *ch, FILE *fp )
{
   static TATTOO_DATA  tattoo_zero;
          TATTOO_DATA *tattoo;
          bool         fMatch;
          char        *word;

    if ( !tattoo_free )
    {
	tattoo		= alloc_perm( sizeof( *tattoo ) );
    }
    else
    {
	tattoo		= tattoo_free;
	tattoo_free	= tattoo_free->next;
    }

    *tattoo		= tattoo_zero;
    tattoo->short_descr	= str_dup( "" );
    tattoo->deleted        = FALSE;

    for ( ; ; )
    {
	word   = feof( fp ) ? "End" : fread_word( fp );
	fMatch = FALSE;

	switch ( UPPER( word[0] ) )
	{
	case '*':
	    fMatch = TRUE;
	    fread_to_eol( fp );
	    break;


	case 'A':

	    if ( !str_cmp( word, "Affect" ) )
	    {
		AFFECT_DATA *paf;

		if ( !affect_free )
		{
		    paf		= alloc_perm( sizeof( *paf ));
		}
		else
		{
		    paf		= affect_free;
		    affect_free	= affect_free->next;
		}

		paf->type	= fread_number( fp );
		paf->type       = slot_lookup(paf->type);
		paf->duration	= fread_number( fp );
		paf->modifier	= fread_number( fp );
		paf->location	= fread_number( fp );
		paf->bitvector	= fread_number( fp );
		paf->next	= tattoo->affected;
		tattoo->affected= paf;
		fMatch		= TRUE;
		break;
	    }
	    break;

   	case 'E':

	    if ( !str_cmp( word, "End" ) )
	    {
		    tattoo->next = tattoo_list;
		    tattoo_list	 = tattoo;
	            tattoo_to_char( tattoo, ch, TRUE );
		    return;
	    }
	    break;


   	case 'M':
    KEY( "MagicBoost",  tattoo->magic_boost,    fread_number( fp ) );
        break;

	case 'S':
    KEY( "ShortDescr",	tattoo->short_descr,	fread_string( fp ) );
        break;

   	case 'W':
    KEY( "WearLoc",     tattoo->wear_loc,       fread_number( fp ) ); 
        break;

	}

	if ( !fMatch )
	{
	    bug( "Fread_tattoo: no match.", 0 );
	    fread_to_eol( fp );
	}
    }
}



void fread_obj( CHAR_DATA *ch, FILE *fp, bool storage )
{
    static OBJ_DATA  obj_zero;
           OBJ_DATA *obj;
           char     *word;
           int       iNest;
           bool      fMatch;
           bool      fNest;
           bool      fVnum;

    if ( !obj_free )
    {
	obj		= alloc_perm( sizeof( *obj ) );
    }
    else
    {
	obj		= obj_free;
	obj_free	= obj_free->next;
    }

    *obj		= obj_zero;
    obj->name		= str_dup( "" );
    obj->short_descr	= str_dup( "" );
    obj->description	= str_dup( "" );
    obj->deleted        = FALSE;

    fNest		= FALSE;
    fVnum		= TRUE;
    iNest		= 0;

    for ( ; ; )
    {
	word   = feof( fp ) ? "End" : fread_word( fp );
	fMatch = FALSE;

	switch ( UPPER( word[0] ) )
	{
	case '*':
	    fMatch = TRUE;
	    fread_to_eol( fp );
	    break;

	case 'A':

            if ( !str_cmp( word, "Activates") )
	    {	
		obj->invoke_type	  = fread_number( fp );
		obj->invoke_vnum	  = fread_number( fp );
		obj->invoke_charge[0] = fread_number( fp );
		obj->invoke_charge[1] = fread_number( fp );
		fMatch		= TRUE;
		break;
	    }
               
            if ( !str_cmp( word, "AcSpell" ) )
               {
                obj->invoke_spell     = fread_string( fp ); 
                fMatch            = TRUE;
                break;
               }

	    if ( !str_cmp( word, "Affect" ) )
	    {
		AFFECT_DATA *paf;

		if ( !affect_free )
		{
		    paf		= alloc_perm( sizeof( *paf ));
		}
		else
		{
		    paf		= affect_free;
		    affect_free	= affect_free->next;
		}

		paf->type	= fread_number( fp );
		paf->type       = slot_lookup(paf->type);
		paf->duration	= fread_number( fp );
		paf->modifier	= fread_number( fp );
		paf->location	= fread_number( fp );
		paf->bitvector	= fread_number( fp );
		paf->next	= obj->affected;
		obj->affected	= paf;
		fMatch		= TRUE;
		break;
	    }
	    break;

	case 'B':
	    KEY( "BionicFlags",	obj->bionic_flags,	fread_number( fp ) );
	    KEY( "BionicLoc",	obj->bionic_loc,        fread_number( fp ) );
	    break;

	case 'C':
	    KEY( "Composition",	obj->composition,	fread_number( fp ) );
	    KEY( "Copper", 	obj->cost.copper,	fread_number( fp ) ); 
	    KEY( "Cost",	obj->cost.copper,	fread_number( fp ) );
	    break;

	case 'D':
	    KEY( "Description",	obj->description,	fread_string( fp ) );
	    KEY( "Durability",	obj->durability,	fread_number( fp ) );
	    break;

	case 'E':
	    KEY( "ExtraFlags",	obj->extra_flags,	fread_number( fp ) );
	    if ( !str_cmp( word, "ExtraDescr" ) )
	    {
		EXTRA_DESCR_DATA *ed;

		if ( !extra_descr_free )
		{
		    ed			= alloc_perm( sizeof( *ed ) );
		}
		else
		{
		    ed			= extra_descr_free;
		    extra_descr_free	= extra_descr_free->next;
		}

		ed->keyword		= fread_string( fp );
		ed->description		= fread_string( fp );
		ed->next		= obj->extra_descr;
		obj->extra_descr	= ed;
		fMatch = TRUE;
	    }

	    if ( !str_cmp( word, "End" ) )
	    {
		if ( !fNest || !fVnum )
		{
		    bug( "Fread_obj: incomplete object.", 0 );
		    free_string( obj->name        );
		    free_string( obj->description );
		    free_string( obj->short_descr );
		    free_mem( obj, sizeof( *obj ) );
		    return;
		}
		else
		{
		    obj->next	= object_list;
		    object_list	= obj;
		    obj->pIndexData->count++;
		    if ( iNest == 0 || !rgObjNest[iNest] )
		    {
		        if ( storage && !IS_NPC( ch ) )
			  obj_to_storage( obj, ch );
			else
			  obj_to_char( obj, ch );
		    }
		    else
                    {
                     if ( rgObjNest[iNest-1]->item_type == ITEM_ARMOR &&
                          rgObjNest[iNest-1]->value[1] == FALSE )
			obj_to_sheath( obj, rgObjNest[iNest-1] );
                     else
			obj_to_obj( obj, rgObjNest[iNest-1] );
                    }
		    if ( obj->item_type == ITEM_POTION )
		      SET_BIT( obj->extra_flags, ITEM_NO_LOCATE );
		    if ( obj->item_type == ITEM_NOTEBOARD
		    || obj->item_type == ITEM_PORTAL )
			extract_obj( obj );
		    return;
		}
	    }
	    break;
	case 'G':
	    KEY( "Gold",	obj->cost.gold,		fread_number( fp ) );
	    break; 
	case 'I':
	    KEY( "ItemType",	obj->item_type,		fread_number( fp ) );
	    break;

	case 'L':
	    KEY( "Level",	obj->level,		fread_number( fp ) );
	    break;

	case 'N':
	    KEY( "Name",	obj->name,		fread_string( fp ) );

	    if ( !str_cmp( word, "Nest" ) )
	    {
		iNest = fread_number( fp );
		if ( iNest < 0 || iNest >= MAX_NEST )
		{
		    bug( "Fread_obj: bad nest %d.", iNest );
		}
		else
		{
		    rgObjNest[iNest] = obj;
		    fNest = TRUE;
		}
		fMatch = TRUE;
	    }
	    break;

	case 'O':
         KEY( "Owned",		obj->ownedby,		fread_string( fp ) );
        break;

	case 'S':
	    KEY( "ShortDescr",	obj->short_descr,	fread_string( fp ) );
	    KEY( "Silver", 	obj->cost.silver,	fread_number( fp ) ); 
	    if ( !str_cmp( word, "Spell" ) )
	    {
		int iValue;
		int sn;

		iValue = fread_number( fp );
		sn     = skill_lookup( fread_word( fp ) );
		if ( iValue < 0 || iValue > 3 )
		{
		    bug( "Fread_obj: bad iValue %d.", iValue );
		}
		else if ( sn < 0 )
		{
		    bug( "Fread_obj: unknown skill.", 0 );
		}
		else
		{
		    obj->value[iValue] = sn;
		}
		fMatch = TRUE;
		break;
	    }

	    break;

	case 'T':
	    KEY( "Timer",	obj->timer,		fread_number( fp ) );
	    break;

	case 'V':
	    if ( !str_cmp( word, "Values" ) )
	    {
		obj->value[0]	= fread_number( fp );
		obj->value[1]	= fread_number( fp );
		obj->value[2]	= fread_number( fp );
		obj->value[3]	= fread_number( fp );
		obj->value[4]	= fread_number( fp );
		obj->value[5]	= fread_number( fp );
		obj->value[6]	= fread_number( fp );
		obj->value[7]	= fread_number( fp );
		obj->value[8]	= fread_number( fp );
		obj->value[9]	= fread_number( fp );
		fMatch		= TRUE;
		break;
	    }

	    if ( !str_cmp( word, "Vnum" ) )	/* OLC */
	    {
		int vnum;

		vnum = fread_number( fp );
		if ( !( obj->pIndexData = get_obj_index( vnum ) ) )
			obj->pIndexData = get_obj_index( OBJ_VNUM_DUMMY );

		fVnum = TRUE;
		fMatch = TRUE;
		break;
	    }
	    break;

	case 'W':
	    KEY( "WearFlags",	obj->wear_flags,	fread_number( fp ) );
	    KEY( "WearLoc",	obj->wear_loc,		fread_number( fp ) );
	    KEY( "Weight",	obj->weight,		fread_number( fp ) );
	    KEY( "Wiznet",	ch->wiznet,		fread_number( fp ) );
	    break;

	}

	if ( !fMatch )
	{
	    bug( "Fread_obj: no match.", 0 );
	    fread_to_eol( fp );
	}
    }
}

void save_pet( CHAR_DATA *ch, FILE *fp, CHAR_DATA *pet )
{
  AFFECT_DATA  *paf;
  fprintf( fp, "#PET\n" );
  fprintf( fp, "%d %d %d %d %d %ld %ld %d %s~ %s~\n", pet->pIndexData->vnum, 
     pet->hit, pet->perm_hit, pet->mod_hit, pet->act, pet->affected_by,pet->affected_by2,
     pet->level, pet->short_descr, pet->long_descr );  

    for ( paf = pet->affected; paf; paf = paf->next )
    {
        if ( paf->deleted )
	    continue;

	fprintf( fp, "Aff       %3d %3d %3d %3d %10d %3d\n",
		skill_table[paf->type].slot,
		paf->duration,
		paf->modifier,
		paf->location,
		paf->bitvector,
                paf->level );
    }
    for ( paf = pet->affected2; paf; paf = paf->next )
    {
        if ( paf->deleted )
	    continue;

	fprintf( fp, "Aff2       %3d %3d %3d %3d %10d %3d\n",
		skill_table[paf->type].slot,
		paf->duration,
		paf->modifier,
		paf->location,
		paf->bitvector,
                paf->level );
    }

  fprintf( fp, "EndOfPet" );

  fprintf( fp, "\n" );
  return;
}  

void fread_pet( CHAR_DATA *ch, FILE *fp )
{
  MOB_INDEX_DATA *pMob;
  CHAR_DATA *pet;
  int vnum;
  char * word;  
  vnum = fread_number( fp );
  if ( ( pMob = get_mob_index( vnum ) ) == NULL ||
       ( pet = create_mobile( pMob ) ) == NULL )
  {
      for ( vnum = 0; vnum < 5; vnum++ )
	fread_number( fp );
      return;
  }
  char_to_room( pet, ch->in_room );
  pet->master = ch;
  pet->hit = fread_number( fp );
  pet->perm_hit = fread_number( fp );
  pet->mod_hit = fread_number( fp );
  pet->act = fread_number( fp );
  pet->affected_by = fread_number( fp ) | AFF_CHARM;
  pet->affected_by2 = fread_number( fp );
  pet->level = fread_number( fp );
  free_string(pet->short_descr); 
  pet->short_descr = str_dup(fread_string( fp ));
  free_string(pet->long_descr);
  pet->long_descr = str_dup(fread_string( fp ));

    for ( ; ; )
    {
	word   =  fread_word( fp );

            if ( !str_cmp( word, "EndOfPet" ) )
		break;

	    else if ( !str_cmp( word, "Aff" ) )
	    {
		AFFECT_DATA *paf;

		if ( !affect_free )
		{
		    paf		= alloc_perm( sizeof( *paf ) );
		}
		else
		{
		    paf		= affect_free;
		    affect_free	= affect_free->next;
		}

		paf->type	= fread_number( fp );
		paf->type       = slot_lookup(paf->type);
		paf->duration	= fread_number( fp );
		paf->modifier	= fread_number( fp );
		paf->location	= fread_number( fp );
		paf->bitvector	= fread_number( fp );
		paf->level	= fread_number( fp );
		paf->deleted    = FALSE;
		paf->next	= pet->affected;
		pet->affected	= paf;
		if ( !is_sn(paf->type) || paf->type == 0 )
		  paf->deleted = TRUE;
	    }
	    else if ( !str_cmp( word, "Aff2" ) )
	    {
		AFFECT_DATA *paf;

		if ( !affect_free )
		{
		    paf		= alloc_perm( sizeof( *paf ));
		}
		else
		{
		    paf		= affect_free;
		    affect_free	= affect_free->next;
		}

		paf->type	= fread_number( fp );
		paf->type       = slot_lookup(paf->type);
		paf->duration	= fread_number( fp );
		paf->modifier	= fread_number( fp );
		paf->location	= fread_number( fp );
		paf->bitvector	= fread_number( fp );
		paf->level	= fread_number( fp );
		paf->deleted    = FALSE;
		paf->next	= pet->affected2;
		pet->affected2	= paf;
		if ( !is_sn(paf->type) || paf->type == 0 )
		  paf->deleted = TRUE;
	    }
  }
 return;
}

void fread_alias( CHAR_DATA *ch, FILE *fp )
{
  ALIAS_DATA *iAl;
  
  iAl = ch->pcdata->alias_list;
  
  for ( ; ; )
  {
    char *word = NULL;
    char *word1 = NULL;
    char letter;
    
    letter = fread_letter( fp );
    bug("letter: %c", letter );

    switch( letter )
    {
      case '~' :
	fread_to_eol( fp );
	return;
      default:
	ungetc( letter, fp );
	word = fread_string( fp );
	word1 = fread_string( fp );
	add_alias( ch, iAl, word, word1 );
	fread_to_eol( fp );
	sprintf( log_buf, "%s %s", word, word1 );
	bug( log_buf, 0 );
	break;
    }
  }
  return;
}

void fwrite_alias( CHAR_DATA *ch, FILE *fp )
{
  ALIAS_DATA *pAl;
  
  fprintf(fp, "#ALIAS\n");
  for ( pAl = ch->pcdata->alias_list; pAl; pAl = pAl->next )
  {
    fprintf( fp, "%s~ %s~\n", pAl->old, pAl->new );
    bug( "Writing alias...", 0 );
  }
  fprintf( fp, "~\n\n" );
  return;
}

/*
 * Assumes ch->pcdata->corpses & that it is initialized to 0.
 * This routine WILL NOT work as written without it.
 * checks to make sure corpse is not empty before
 * reading/writing the corpse file.
 */
void corpse_back( CHAR_DATA *ch, OBJ_DATA *corpse )
{
    FILE      *fp; 	
    OBJ_DATA  *obj, *obj_next;
    OBJ_DATA  *obj_nest,  *objn_next;
    char       strsave[MAX_INPUT_LENGTH ];
    char       buf    [MAX_STRING_LENGTH]; 	 	
    int        corpse_cont[1024];
    int        item_level [1024];
    int        c =1; 
    int        checksum1 =0;
    int        checksum2 =0;
    
    /* Don't do anything if the corpse is empty */
    if (!corpse->contains)
        return;
 
    if ( IS_NPC( ch ) )
        return;         

    /* Ok, it isn't empty determine the # of items in the corpse. 
     * Store the items in the LAST number of the array to  write 
     * it backwards.  Easiest way to do it.
     */
    for ( obj = corpse->contains; obj; obj = obj_next )
    {
        obj_next = obj->next_content;
	if ( obj->item_type == ITEM_POTION )
		continue;
        corpse_cont[c] = obj->pIndexData->vnum;
        item_level[c] = obj->level;
        checksum1 += corpse_cont[c];        
        checksum2 +=  item_level[c];        
        ++c;
        if ( obj->contains) /* get stuff in containers */
        {
           for ( obj_nest = obj->contains; obj_nest; obj_nest = objn_next )
    	   {
              objn_next = obj_nest->next_content;
	      if ( obj_nest->item_type == ITEM_POTION )
		  continue;
              corpse_cont[c] = obj_nest->pIndexData->vnum;
              item_level[c] = obj_nest->level;
              checksum1 += corpse_cont[c];        
              checksum2 +=  item_level[c];        
              ++c;
           }     
        }
    }
    /* Check the corpse for only one item. Assumes if true the the player
     * died trying to retrieve their corpse. Change it if you like.
     */
    if (c <= 2 ) 
     return;
    
    /* Add in the number of items and checksum for validation check */
    corpse_cont[0] = c -1;
    item_level[0] = c -1;
    corpse_cont[c+1] = checksum1;
    item_level[c+1] = checksum2;

    /* Ok now we have a corpse to save, is it the first one? */
    
    fclose( fpReserve );
    
    /* player files parsed directories by Yaz 4th Realm */
#if !defined( macintosh ) && !defined( MSDOS )
    sprintf( strsave, "%s%c/%s.cps", PLAYER_DIR, LOWER(ch->name[0]),
              capitalize( ch->name ) );
#else
    sprintf( strsave, "%s%s.cps", PLAYER_DIR, capitalize( ch->name ) );
#endif

    if ( !( fp = fopen( strsave, "w" ) ) )
    {
     sprintf( buf, "Corpse back: fopen %s: ", ch->name );
     bug( buf, 0 );
     perror( strsave );
    }
    else
    {
     int i;
     for ( i = 0 ; i < c ; i++ )
     {
      fprintf( fp, "%d ", corpse_cont[i] );
      fprintf( fp, "%d ",  item_level[i] );
     }
     fprintf( fp, "%d ",  corpse_cont[i+1] );      
     fprintf( fp, "%d\n",  item_level[i+1] );      
    }
    fclose( fp );        
    fpReserve = fopen( NULL_FILE, "r" );
    ch->pcdata->corpses = 1;
    return;

    fclose( fpReserve );
    
    /* Okay, it isn't the first corpse, read the rest */
}

void save_finger ( CHAR_DATA *ch )
{
  FILE *fp;
  char buf      [ MAX_STRING_LENGTH ];
  char fingsave [ MAX_INPUT_LENGTH  ];
  
   if ( IS_NPC( ch ) )
    return;

   if ( ch->level < 1 )
    return;
  
  if ( ch->desc && ch->desc->original )
    ch = ch->desc->original;
  
  sprintf( fingsave, "%s%c/%s.fng", PLAYER_DIR, LOWER(ch->name[0]),
   capitalize( ch->name ) );

  
  fclose(fpReserve);
  if ( !( fp = fopen ( fingsave, "w" ) ) )
  {
   sprintf( buf, "Save_finger_Info: fopen %s ", ch->name );
   bug( buf, 0 );
   perror( fingsave );
  }
  else
   fwrite_finger( ch, fp );
  
  fclose( fp );
  fpReserve = fopen( NULL_FILE, "r" );
  return;
}

void fwrite_finger( CHAR_DATA *ch, FILE *fp )
{
  fprintf( fp, "&CName: &W%s\n",		ch->name	);
  fprintf( fp, "&CMud Age: &W%-13d",		get_age( ch )	);
  fprintf( fp, "&CClan: &W%s\n", get_clan_index( ch->clan )->name );
  fprintf( fp, "&CLevel: &W%-16d",		ch->level	);
  fprintf( fp, "&CSex: &W%s\n",	  ch->sex == SEX_MALE ? "male" :
   ch->sex == SEX_FEMALE ? "female" : "neutral" );
  fprintf( fp, "&CRace: &W%s\n",	(get_race_data(ch->race))->race_name );
  fprintf( fp, "&CTitle: &W%s\n",			ch->pcdata->title );
  fprintf( fp, "&CEmail: &W%s\n",			ch->pcdata->email );
  fprintf( fp, "&CPlan: &W%s\n",			ch->pcdata->plan );
  fprintf( fp, "&CLast On: &W%s~\n", 		(char * ) ctime(&ch->logon) );
  return;
}

void read_finger ( CHAR_DATA *ch, char *argument )
{
  FILE *fp;
  char buf [ MAX_STRING_LENGTH ];
  char fingload [ MAX_INPUT_LENGTH ];
  char arg [ MAX_INPUT_LENGTH ];
  char arg2 [ MAX_INPUT_LENGTH ];

  argument = one_argument( argument, arg );
  argument = one_argument( argument, arg2 );

  sprintf( fingload, "%s%c/%s.fng", PLAYER_DIR, LOWER(arg[0]),
   capitalize( arg ) );

  fclose(fpReserve);

  if ( !( fp = fopen ( fingload, "r" ) ) )
  {
   sprintf( buf, "Load_finger_Info: fopen %s ", arg );
   bug( buf, 0 );
   perror( fingload );
   sprintf( buf, "The character %s doesn't exist.\n\r", arg );
   send_to_char( AT_WHITE, buf, ch );
  }
  else
   fread_finger ( ch, fp );
  
  fclose( fp );
  fpReserve = fopen( NULL_FILE, "r" );
  return;
}

void fread_finger ( CHAR_DATA *ch, FILE *fp )
{
  char *finger;
  char  buf[MAX_STRING_LENGTH];
  
  sprintf( buf, "          Finger Info             \n\r" );
  send_to_char( AT_WHITE, buf, ch );
    
  sprintf( buf, "          ------ ----\n\r\n\r" );
  send_to_char( AT_WHITE, buf, ch );
        
  finger = fread_string( fp );
  sprintf( buf, "%s", finger );
  send_to_char( AT_WHITE, buf, ch );
  send_to_char( AT_WHITE, "\n\r", ch );
  return;
}  

void save_banlist( BAN_DATA *banlist )
{
  BAN_DATA *ban;
  FILE *fp;
  char buf [ MAX_INPUT_LENGTH ];

  if ( !(fp = fopen( BAN_LIST, "w" )) )
  {
   bug( "Unable to open BAN_LIST", 0 );
   return;
  }
  for ( ban = ban_list; ban; ban = ban->next )
  {
   if ( ban->user )
    sprintf( buf, "%c %s@%s", ban->type, ban->user, ban->name );
   else
    sprintf( buf, "%c %s", ban->type, ban->name );
   fprintf( fp, "%s~\n", buf );
  }
  fprintf( fp, "$\n" );
  fclose( fp );
  return;
}
