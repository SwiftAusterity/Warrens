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
#include <sys/stat.h>
#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include "merc.h"

#if !defined( macintosh )
extern  int     _filbuf	        args( (FILE *) );
#endif

#if !defined( ultrix )
#include <memory.h>
#endif

/*
 * Globals.
 */
HELP_DATA *		help_first;
HELP_DATA *		help_last;

QUEST_DATA *		first_quest;

SOCIAL_DATA *		social_first;
SOCIAL_DATA *		social_last;

RACE_DATA *		first_race;

SHOP_DATA *		shop_first;
SHOP_DATA *		shop_last;

CASINO_DATA *		casino_first;
CASINO_DATA *		casino_last;

TATTOO_ARTIST_DATA *	tattoo_artist_first;
TATTOO_ARTIST_DATA *	tattoo_artist_last;

CHAR_DATA *		char_free;
EXTRA_DESCR_DATA *	extra_descr_free;
NOTE_DATA *		note_free;
OBJ_DATA *		obj_free;
TATTOO_DATA *		tattoo_free;
PC_DATA *		pcdata_free;

char                    bug_buf                 [ MAX_INPUT_LENGTH*2 ];
CHAR_DATA *		char_list;
PLAYERLIST_DATA	*	playerlist; /*Decklarean */
char *			help_greeting;
char *			help_second_port_greeting;
char	            	log_buf                 [ MAX_INPUT_LENGTH*2 ];
KILL_DATA	      	kill_table              [ MAX_LEVEL          ];
NOTE_DATA *		note_list;
OBJ_DATA *		object_list;
TATTOO_DATA *		tattoo_list;
ECONOMY_DATA		economy;
TIME_INFO_DATA		time_info;
ARENA_DATA		arena;
char *                  down_time;
char *                  warning1;
char *                  warning2;
int                     stype;
int                     port;

/* gsn values go here -Flux */


/*
 * Locals.
 */
MOB_INDEX_DATA *	mob_index_hash	        [ MAX_KEY_HASH       ];
OBJ_INDEX_DATA *	obj_index_hash	        [ MAX_KEY_HASH       ];
ROOM_INDEX_DATA *	room_index_hash         [ MAX_KEY_HASH       ];
char *			string_hash	        [ MAX_KEY_HASH       ];

AREA_DATA *		area_first;
AREA_DATA *		area_last;
CLAN_DATA *             clan_first;

char *			string_space;
char *			top_string;
char			str_empty	        [ 1                  ];


int      		num_mob_progs;
int			num_trap_progs;

int			mobs_in_game;
int			top_affect;
int			top_area;
int			top_ed;
int			top_exit;
int			top_help;
int			top_mob_index;
int			top_obj_index;
int			top_clan;
int			top_social;
int			top_race;
int			top_quest;
int			top_reset;
int			top_room;
int			top_shop;
int			top_casino;
int			top_tattoo_artist;
int 			top_trap;
int			top_mob_prog;
int 		 	mprog_name_to_type	args ( ( char* name ) );
MPROG_DATA *		mprog_file_read 	args ( ( char* f, MPROG_DATA* mprg, 
						MOB_INDEX_DATA *pMobIndex ) );
void			load_mobprogs           args ( ( FILE* fp ) );
void   			mprog_read_programs     args ( ( FILE* fp,
					        MOB_INDEX_DATA *pMobIndex ) );
void                    load_traps              args ( ( FILE* fp,
							OBJ_INDEX_DATA *pObj,
							ROOM_INDEX_DATA *pRoom,
							EXIT_DATA *pExit ) );
void                    area_sort               args ( ( AREA_DATA *pArea ) );
void                    clan_sort               args ( ( CLAN_DATA *pClan ) );
void add_playerlist    args ( (CHAR_DATA *ch) );
void update_playerlist args ( ( CHAR_DATA *ch ) );
void load_player_list  args ( ( ) );
void load_race 		args ( ( ) );
void load_quests	args ( ( ) );

/*
 * Memory management.
 * Increase MAX_STRING from 1500000 if you have too.
 * Tune the others only if you understand what you're doing.
 */
#define			MAX_STRING      3500000

#if defined( machintosh )
#define			MAX_PERM_BLOCK  131072
#define			MAX_MEM_LIST    11

void *			rgFreeList              [ MAX_MEM_LIST       ];
const int		rgSizeList              [ MAX_MEM_LIST       ]  =
{
    16, 32, 64, 128, 256, 1024, 2048, 4096, 8192, 16384, 32768-64
};
#else
#define			MAX_PERM_BLOCK  131072
#define			MAX_MEM_LIST    12

void *			rgFreeList              [ MAX_MEM_LIST       ];
const int		rgSizeList              [ MAX_MEM_LIST       ]  =
{
    16, 32, 64, 128, 256, 1024, 2048, 4096, 8192, 16384, 32768, 65536
};
#endif


int			nAllocString;
int			sAllocString;
int			nAllocPerm;
int			sAllocPerm;
int 			MemAllocated;
int 			MemFreed;



/*
 * Semi-locals.
 */
bool			fBootDb;
FILE *			fpArea;
char			strArea                 [ MAX_INPUT_LENGTH   ];


/*
 * Local booting procedures.
 */
void	init_mm		args( ( void ) );

void	load_helps      args( ( FILE *fp ) );
void    load_recall     args( ( FILE *fp ) );
void	load_mobiles    args( ( FILE *fp ) );
void	load_objects    args( ( FILE *fp ) );
void	load_resets     args( ( FILE *fp ) );
void	load_shops      args( ( FILE *fp ) );
void	load_casinos    args( ( FILE *fp ) );
void	load_tattoo_artists args( ( FILE *fp ) );
void	load_specials   args( ( FILE *fp ) );
void	load_notes      args( ( void ) );
void    load_clans      args( ( void ) );
void    load_down_time  args( ( void ) );
void	fix_exits       args( ( void ) );

void	reset_area      args( ( AREA_DATA * pArea ) );

/*
 * Non-Envy Loading procedures.
 * Put any new loading function in this section.
 */
void	new_load_area	args( ( FILE *fp ) );	/* OLC */
void	new_load_rooms	args( ( FILE *fp ) );	/* OLC 1.1b */
void	load_banlist	args( ( void ) );
void	load_socials	args( ( void ) );       /* Decklarean */


/*
 * Big mama top level function.
 */
void boot_db( void )
{

char buf[MAX_STRING_LENGTH];
int debug = 0;
    /*
     * Init some data space stuff.
     */
    {
	if ( !( string_space = calloc( 1, MAX_STRING ) ) )
	{
	    bug( "Boot_db: can't alloc %d string space.", MAX_STRING );
	    exit( 1 );
	}
	top_string	= string_space;
	fBootDb		= TRUE;
    }

    /*
     * Init random number generator.
     */
    {
	init_mm( );
    }

    /*
     * Set time
     */
    {
	long lhour, lday, lmonth;

	lhour		= ( current_time - 650336715 )
			   / ( PULSE_TICK / PULSE_PER_SECOND );
	time_info.hour  = lhour  % 24;
	lday		= lhour  / 24;
	time_info.day	= lday   % 30;
	lmonth		= lday   / 30;
	time_info.month	= lmonth % 12;
	time_info.year	= lmonth / 12;

        time_info.phase_white  = dice( 1, 7 );
        time_info.phase_shadow = dice( 1, 7 );
        time_info.phase_blood  = dice( 1, 7 );
        time_info.moon_white   = 0;
        time_info.moon_shadow  = 0;
        time_info.moon_blood   = 0;
  

	     if ( time_info.hour < 1 ) time_info.sunlight = SUN_MIDNIGHT;
	else if ( time_info.hour < 5 ) time_info.sunlight = SUN_DARK;
	else if ( time_info.hour < 6 ) time_info.sunlight = SUN_DAWN;
	else if ( time_info.hour < 11) time_info.sunlight = SUN_MORNING;
	else if ( time_info.hour < 13) time_info.sunlight = SUN_NOON;
	else if ( time_info.hour < 18) time_info.sunlight = SUN_AFTERNOON;
	else if ( time_info.hour < 19) time_info.sunlight = SUN_DUSK;
	else if ( time_info.hour < 24) time_info.sunlight = SUN_EVENING;
	else                           time_info.sunlight = SUN_DARK;
    }

    /* Set the global economy in motion -Flux. */
    {
     int type = 1;
     while( type <= MAX_ITEM_TYPE )
     {
      economy.item_type[type] = 0;
      economy.cost_modifier[type] = dice( 50, 2 );
      type += 1;
     }
     economy.market_type = 0;
    }

    /*
     * Assign gsn's for skills which have them.
     */
    {
	int sn;

	for ( sn = 0; skill_table[sn].name[0] != '\0'; sn++ )
	{
	    if ( skill_table[sn].pgsn )
		*skill_table[sn].pgsn = sn;
	}
    }

    /*
     * Read in all the area files.
     */
    {
	FILE *fpList;
        char area_list_file[ MAX_INPUT_LENGTH ];

        sprintf( area_list_file, "%s%s", AREA_DIR, AREA_LIST );

	if ( !( fpList = fopen( area_list_file, "r" ) ) )
	{
	    perror( area_list_file );
	    exit( 1 );
	}
	
	for ( ; ; )
	{
	    fpArea = fpList;
	    strcpy(strArea, area_list_file);
	    strcpy( strArea, fread_word( fpList ) );
 
            if ( debug )
            { 
                sprintf(buf, "loading area %s", strArea);
 	        bug(buf, 0);
            }

	    if ( strArea[0] == '$' )
		break;

	    if ( strArea[0] == '-' )
	    {
		fpArea = stdin;
	    }
	    else
	    {
             char	area_fle[MAX_STRING_LENGTH];

             if ( str_cmp( strArea, HELP_FILE ) )
              sprintf( area_fle, "%s%s", AREA_DIR, strArea );
             else
              sprintf( area_fle, "%s", strArea );

		if ( !( fpArea = fopen( area_fle, "r" ) ) )
		{
		       perror( strArea );
		       continue;
		}
	    }

	    for ( ; ; )
	    {
		char *word;

		if ( fread_letter( fpArea ) != '#' )
		{
		    bug( "Boot_db: # not found.", 0 );
		    break;
		}

		word = fread_word( fpArea );

		     if ( word[0] == '$'               )
                    break;
		else if ( !str_cmp( word, "HELPS"    ) ) 
		    load_helps   ( fpArea );
		else if ( !str_cmp( word, "RECALL"   ) )
		    load_recall  ( fpArea );
		else if ( !str_cmp( word, "MOBILES"  ) )
	    	     load_mobiles ( fpArea );
	        else if ( !str_cmp( word, "MOBPROGS" ) ) 
	            load_mobprogs( fpArea );
		else if ( !str_cmp( word, "OBJECTS"  ) )
		    load_objects ( fpArea );
		else if ( !str_cmp( word, "RESETS"   ) )
		    load_resets  ( fpArea );
		else if ( !str_cmp( word, "SHOPS"    ) )
		    load_shops   ( fpArea );
		else if ( !str_cmp( word, "CASINOS"    ) )
		    load_casinos   ( fpArea );
		else if ( !str_cmp( word, "ARTISTS"  ) )
		    load_tattoo_artists( fpArea );
		else if ( !str_cmp( word, "SPECIALS" ) )
		    load_specials( fpArea );
		else if ( !str_cmp( word, "AREADATA" ) )	/* OLC */
		    new_load_area( fpArea );
		else if ( !str_cmp( word, "ROOMDATA" ) )	/* OLC 1.1b */
		    new_load_rooms( fpArea );
		else
		{
		    bug( "Boot_db: bad section name.", 0 );
		    break;
		}
	    }
{
char buf[MAX_STRING_LENGTH];
sprintf(buf, "loading %s...", strArea );
log_string( buf, CHANNEL_NONE , -1 );
}
	    if ( fpArea != stdin )
		fclose( fpArea );
	    fpArea = NULL;
	}
	fclose( fpList );
    }

    /*
     * Fix up exits.
     * Declare db booting over.
     * Reset all areas once.
     * Load up the notes file.
     */
     {
       fix_exits( );
       load_banlist( ); 
       load_clans( ); 
       load_socials( ); 
       load_player_list( ); 
       load_race( ); 
       load_quests( ); 
       load_notes( );
       load_down_time( );

       fpArea = NULL;
       strcpy(strArea, "$");
       fBootDb = FALSE;

       area_update( );
}
MOBtrigger = TRUE;
return;
}


int mprog_name_to_type ( char *name )
{
   if ( !str_cmp( name, "in_file_prog"   ) )	return IN_FILE_PROG;
   if ( !str_cmp( name, "act_prog"       ) )    return ACT_PROG;
   if ( !str_cmp( name, "speech_prog"    ) )	return SPEECH_PROG;
   if ( !str_cmp( name, "rand_prog"      ) ) 	return RAND_PROG;
   if ( !str_cmp( name, "rand_vict_prog" ) ) 	return RAND_VICT_PROG;
   if ( !str_cmp( name, "fight_prog"     ) )	return FIGHT_PROG;
   if ( !str_cmp( name, "hitprcnt_prog"  ) )	return HITPRCNT_PROG;
   if ( !str_cmp( name, "death_prog"     ) )	return DEATH_PROG;
   if ( !str_cmp( name, "entry_prog"     ) )	return ENTRY_PROG;
   if ( !str_cmp( name, "greet_prog"     ) )	return GREET_PROG;
   if ( !str_cmp( name, "all_greet_prog" ) )	return ALL_GREET_PROG;
   if ( !str_cmp( name, "give_prog"      ) ) 	return GIVE_PROG;
   if ( !str_cmp( name, "bribe_prog"     ) )	return BRIBE_PROG;
   if ( !str_cmp( name, "spell_prog"     ) )	return SPELL_PROG;

   return( ERROR_PROG );
}

/* This routine reads in scripts of MOBprograms from a file */

MPROG_DATA* mprog_file_read( char *f, MPROG_DATA *mprg,
			    MOB_INDEX_DATA *pMobIndex )
{

  char        MOBProgfile[ MAX_INPUT_LENGTH ];
  MPROG_DATA *mprg2;
  FILE       *progfile;
  char        letter;
  bool        done = FALSE;

/*  sprintf( MOBProgfile, "%s%s", MOB_DIR, f );
*/
  progfile = fopen( MOBProgfile, "r" );
  if ( !progfile )
  {
     bug( "Mob: %d couldnt open mobprog file", pMobIndex->vnum );
     exit( 1 );
  }
  fpArea = progfile;
  strcpy(strArea, MOBProgfile);

  mprg2 = mprg;
  switch ( letter = fread_letter( progfile ) )
  {
    case '>':
     break;
    case '|':
       bug( "empty mobprog file.", 0 );
       exit( 1 );
     break;
    default:
       bug( "in mobprog file syntax error.", 0 );
       exit( 1 );
     break;
  }

  while ( !done )
  {
    mprg2->type = mprog_name_to_type( fread_word( progfile ) );
    switch ( mprg2->type )
    {
     case ERROR_PROG:
        bug( "mobprog file type error", 0 );
        exit( 1 );
      break;
     case IN_FILE_PROG:
        bug( "mprog file contains a call to file.", 0 );
        exit( 1 );
      break;
     default:
        pMobIndex->progtypes = pMobIndex->progtypes | mprg2->type;
        mprg2->arglist       = fread_string( progfile );
        mprg2->comlist       = fread_string( progfile );
        num_mob_progs++;
        switch ( letter = fread_letter( progfile ) )
        {
          case '>':
             mprg2->next = (MPROG_DATA *)alloc_perm( sizeof( MPROG_DATA ) );
             mprg2       = mprg2->next;
             mprg2->next = NULL;
           break;
          case '|':
             done = TRUE;
           break;
          default:
             bug( "in mobprog file syntax error.", 0 );
             exit( 1 );
           break;
        }
      break;
    }
  }
  fclose( progfile );
  return mprg2;
}

/* Snarf a MOBprogram section from the area file.
 */
void load_mobprogs( FILE *fp )
{
  MOB_INDEX_DATA *iMob;
  MPROG_DATA     *original;
  MPROG_DATA     *working;
  char            letter;
  int             value;

  for ( ; ; )
    switch ( letter = fread_letter( fp ) )
    {
    default:
      bug( "Load_mobprogs: bad command '%c'.",letter);
      exit(1);
      break;
    case 'S':
    case 's':
      fread_to_eol( fp ); 
      return;
    case '*':
      fread_to_eol( fp ); 
      break;
    case 'M':
    case 'm':
      value = fread_number( fp );
      if ( ( iMob = get_mob_index( value ) ) == NULL )
      {
	bug( "Load_mobprogs: vnum %d doesnt exist", value );
	exit( 1 );
      }
    
      /* Go to the end of the prog command list if other commands
         exist */

      if ( ( original = iMob->mobprogs ) )
	for ( ; original->next != NULL; original = original->next );

      working = (MPROG_DATA *)alloc_perm( sizeof( MPROG_DATA ) );
      if ( original )
	original->next = working;
      else
	iMob->mobprogs = working;
      working       = mprog_file_read( fread_word( fp ), working, iMob );
      working->next = NULL;
      fread_to_eol( fp );
      break;
    }

  return;

} 

/* This procedure is responsible for reading any in_file MOBprograms.
 */

void mprog_read_programs( FILE *fp, MOB_INDEX_DATA *pMobIndex)
{
  MPROG_DATA *mprg;
  char        letter;
  bool        done = FALSE;

  if ( ( letter = fread_letter( fp ) ) != '>' )
  {
      bug( "Mprog_read_programs: vnum %d MOBPROG char", pMobIndex->vnum );
      exit( 1 );
  }
  pMobIndex->mobprogs = (MPROG_DATA *)alloc_perm( sizeof( MPROG_DATA ) );
  mprg = pMobIndex->mobprogs;

  while ( !done )
  {
    mprg->type = mprog_name_to_type( fread_word( fp ) );
    switch ( mprg->type )
    {
     case ERROR_PROG:
        bug( "Load_mobiles: vnum %d MOBPROG type.", pMobIndex->vnum );
        exit( 1 );
      break;
     case IN_FILE_PROG:
        mprg = mprog_file_read( fread_string( fp ), mprg,pMobIndex );
        fread_to_eol( fp );
        switch ( letter = fread_letter( fp ) )
        {
          case '>':
             mprg->next = (MPROG_DATA *)alloc_perm( sizeof( MPROG_DATA ) );
             mprg       = mprg->next;
             mprg->next = NULL;
           break;
          case '|':
             mprg->next = NULL;
             fread_to_eol( fp );
             done = TRUE;
           break;
          default:
             bug( "Load_mobiles: vnum %d bad MOBPROG.", pMobIndex->vnum );
             exit( 1 );
           break;
        }
      break;
     default:
        pMobIndex->progtypes = pMobIndex->progtypes | mprg->type;
        mprg->arglist        = fread_string( fp );
        fread_to_eol( fp );
        mprg->comlist        = fread_string( fp );
        fread_to_eol( fp );
        num_mob_progs++;
        switch ( letter = fread_letter( fp ) )
        {
          case '>':
	     mprg->next = (MPROG_DATA *)alloc_perm( sizeof( MPROG_DATA ) );
             mprg       = mprg->next;
             mprg->next = NULL;
           break;
          case '|':
             mprg->next = NULL;
             fread_to_eol( fp );
             done = TRUE;
           break;
          default:
             bug( "Load_mobiles: vnum %d bad MOBPROG.", pMobIndex->vnum );
             exit( 1 );
           break;
        }
      break;
    }
  }

  return;

}

void load_traps( FILE *fp, OBJ_INDEX_DATA *pObj, ROOM_INDEX_DATA *pRoom,
	         EXIT_DATA *pExit )
{
  TRAP_DATA *pTrap;
  TRAP_DATA *pFirst;
  int *traptypes;
  char letter;
  bool done = FALSE;

  if ( ( letter = fread_letter( fp ) ) != '>' )
  {
    bug("Load_traps:  No Traps", 0);
    exit(1);
  }

  if ( !trap_list )
  {
    trap_list = (TRAP_DATA *)alloc_perm( sizeof( TRAP_DATA ));
    pFirst = trap_list;
  }
  else
  {
    for( pFirst = trap_list; pFirst->next; pFirst = pFirst->next );
    pFirst->next = (TRAP_DATA *)alloc_perm( sizeof( TRAP_DATA ));
    pFirst = pFirst->next;
  }
  pFirst->next = NULL;
  pFirst->next_here = NULL;
    
  if ( pObj )
  {
    pObj->traps = pFirst;
    traptypes = &pObj->traptypes;
  }
  else if ( pRoom )
  {
    pRoom->traps = pFirst;
    traptypes = &pRoom->traptypes;
  }
  else if ( pExit )
  {
    pExit->traps = pFirst;
    traptypes = &pExit->traptypes;
  }
  else
  {
    bug("Load_traps:  Nothing to load to!", 0);
    exit( 1 );
  }
  pTrap = pFirst;

  while( !done )
  {
    pTrap->type = flag_value( (pObj ? oprog_types : (pRoom ? rprog_types :
			       eprog_types)), fread_word( fp ) );
    switch( pTrap->type )
    {
    case (pObj ? OBJ_TRAP_ERROR : (pRoom ? ROOM_TRAP_ERROR : EXIT_TRAP_ERROR)):
    case NO_FLAG:
      bug( "Load_traps: No flag found.", 0 );
      exit( 1 );
    default:
      pTrap->on_obj = pObj;
      pTrap->in_room = pRoom;
      pTrap->on_exit = pExit;
      *traptypes |= pTrap->type;
      pTrap->arglist = fread_string( fp );
      fread_to_eol( fp );
      pTrap->disarmable = fread_number( fp );
      fread_to_eol( fp );
      pTrap->comlist = fread_string( fp );
      fread_to_eol( fp );
      num_trap_progs++;
      
      switch ( letter = fread_letter( fp ) )
      {
      case '>':
	pTrap->next = alloc_perm( sizeof( TRAP_DATA ));
	pTrap->next_here = pTrap->next;
	pTrap = pTrap->next;
	pTrap->next = NULL;
	pTrap->next_here = NULL;
	break;
      case '|':
	pTrap->next = NULL;
	pTrap->next_here = NULL;
	fread_to_eol( fp );
	done = TRUE;
	break;
      default:
	bug( "Load_traps:  bad TRAP", 0);
	break;
      }
      break;
    }
  }
  return;
}

/*
 * OLC
 * Use these macros to load any new area formats that you choose to
 * support on your MUD.  See the new_load_area format below for
 * a short example.
 */
#if defined(KEY)
#undef KEY
#endif

#define KEY( literal, field, value )                \
		if ( !str_cmp( word, literal ) )    \
                {                                   \
                    field  = value;                 \
                    fMatch = TRUE;                  \
                    break;                          \
		}

#define SKEY( string, field )                       \
                if ( !str_cmp( word, string ) )     \
                {                                   \
                    free_string( field );           \
                    field = fread_string( fp );     \
                    fMatch = TRUE;                  \
                    break;                          \
		}



/* OLC
 * Snarf an 'area' header line.   Check this format.  MUCH better.  Add fields
 * too.
 *
 * #AREAFILE
 * Name   { All } Locke    Newbie School~
 * Repop  A teacher pops in the room and says, 'Repop coming!'~
 * Recall 3001
 * End
 */
void new_load_area( FILE *fp )
{
    AREA_DATA *pArea;
    char      *word;
    bool      fMatch;

    pArea               = alloc_perm( sizeof(*pArea) );
    pArea->age          = 15;
    pArea->nplayer      = 0;
    pArea->filename     = str_dup( strArea );
    pArea->vnum         = top_area;
    pArea->name         = str_dup( "New Area" );
    pArea->builders     = str_dup( "" );
    pArea->security     = 1;
    pArea->lvnum        = 0;
    pArea->uvnum        = 0;
    pArea->llevel	= 0;
    pArea->ulevel	= 0;
    pArea->area_flags   = 0;
    pArea->recall       = ROOM_ELF_TEMPLE;
    pArea->def_color 	= 6;  /* Angi - AT_CYAN */
    pArea->temporal     = 1;
    pArea->weather      = 1;
    pArea->average_humidity      = 50;
    pArea->average_temp          = 75;
    pArea->temperature_front     = ( dice( 1, 2 ) - 1 );
    pArea->pressure_front        = ( dice( 1, 2 ) - 1 );
    pArea->tfront_length         = 0;
    pArea->pfront_length         = 0;
    pArea->winddir               = 0;
    pArea->winddir_length        = 0;
    pArea->owner	         = 1;

    for ( ; ; )
    {
       word   = feof( fp ) ? "End" : fread_word( fp );
       fMatch = FALSE;

       switch ( UPPER(word[0]) )
       {
	   case 'L':
             if ( !str_cmp( word, "Levels" ) )
             {
                 pArea->llevel = fread_number( fp );
		 pArea->ulevel = fread_number( fp );
	     }
	    break;
           case 'N':
            SKEY( "Name", pArea->name );
            break;
           case 'S':
             KEY( "Security", pArea->security, fread_number( fp ) );
            break;
           case 'V':
            if ( !str_cmp( word, "VNUMs" ) )
            {
                pArea->lvnum = fread_number( fp );
                pArea->uvnum = fread_number( fp );
                /* Set up the arena. */
                if ( pArea->lvnum <= ROOM_ARENA_VNUM &&
                     pArea->uvnum >= ROOM_ARENA_VNUM )
                  arena.area = pArea;
            }
            break;
           case 'F':
            KEY( "Flags", pArea->area_flags, fread_number( fp ) );
            break;
           case 'E':
             if ( !str_cmp( word, "End" ) )
             {
                 fMatch = TRUE;
		 area_sort(pArea);
                 top_area++;
                 return;
            }
            break;
           case 'B':
            SKEY( "Builders", pArea->builders );
            break;
	   case 'C':
	    KEY( "Color", pArea->def_color, fread_number( fp ) );
	    break;
           case 'H':
             KEY( "Humidity", pArea->average_humidity, fread_number( fp ) );
            break;
           case 'O':
             KEY( "Owner", pArea->owner, fread_number( fp ) );
            break;
           case 'R':
             KEY( "Recall", pArea->recall, fread_number( fp ) );
            break;
           case 'T':
             KEY( "Temp", pArea->average_temp, fread_number( fp ) );
             KEY( "Temporal", pArea->temporal, fread_number( fp ) );
            break;
           case 'W':
             KEY( "Weather", pArea->weather, fread_number( fp ) );
            break;
	}
    }
}



/*
 * Sets vnum range for area using OLC protection features.
 */
void assign_area_vnum( int vnum )
{
    if ( area_last->lvnum == 0 || area_last->uvnum == 0 )
	area_last->lvnum = area_last->uvnum = vnum;

    if ( vnum != URANGE( area_last->lvnum, vnum, area_last->uvnum ) )
    {
	if ( vnum < area_last->lvnum )
	    area_last->lvnum = vnum;
	else
	    area_last->uvnum = vnum;
    }
    return;
}

/*
 * Snarf a help section.
 */
void load_helps( FILE *fp )
{
    HELP_DATA *pHelp;

    for ( ; ; )
    {
	pHelp		= alloc_perm( sizeof( *pHelp ) );
	pHelp->level	= fread_number( fp );
	pHelp->keyword	= fread_string( fp );
	if ( pHelp->keyword[0] == '$' )
	    break;
	pHelp->text	= fread_string( fp );

	if ( !str_cmp( pHelp->keyword, "greeting" ) )
	    help_greeting = pHelp->text;

	if ( !str_cmp( pHelp->keyword, "secondportgreeting" ) )
	    help_second_port_greeting = pHelp->text;

	if ( !help_first )
	    help_first = pHelp;
	if (  help_last  )
	    help_last->next = pHelp;

	help_last	= pHelp;
	pHelp->next	= NULL;
	top_help++;
    }

    return;
}

/*
 * Snarf the quest file.
 */
void load_quests( void )
{
    QUEST_DATA	*pQuest;
    FILE	*fp;

    /* Check if we have a quest list. */
    if ( !( fp = fopen( QUEST_FILE, "r" ) ) )
        return;

    fpArea = fp;
    strcpy(strArea, QUEST_FILE);

     for ( ; ; )
     {
        char*  name;

        pQuest = new_quest_data( );

        /* Load a character */
	for ( ; ; )
        {
            name   = fread_word( fp );

            /* Check if we are at the end of the quest list */
            if ( !str_cmp( name, "#END"    ) )
            {
               fclose( fp );
               return;
	    }
            else if ( !str_cmp( name, "End" ) )
              break;
            else if ( !str_cmp( name, "Name" ) )       
              pQuest->name       = fread_string( fp );
            else if ( !str_cmp( name, "Vnum" ) )       
              pQuest->vnum       = fread_number( fp );
 	}
        quest_sort( pQuest );
    }
  fclose(fp);
}

/*
 * Snarf a recall point.
 */
void load_recall( FILE *fp )
{
    AREA_DATA *pArea;
    char       buf [ MAX_STRING_LENGTH ];

    pArea         = area_last;
    pArea->recall = fread_number( fp );

    if ( pArea->recall < 1 )
    {
        sprintf( buf, "Load_recall:  %s invalid recall point", pArea->name );
	bug( buf, 0 );
	pArea->recall = ROOM_ELF_TEMPLE;
    }

    return;

}

/*
 * Snarf a mob section.
 */
void load_mobiles( FILE *fp )
{
    MOB_INDEX_DATA *pMobIndex;
    char letter;
    int num;

    if ( !area_last )	/* OLC */
    {
	bug( "Load_mobiles: no #AREA seen yet.", 0 );
	exit( 1 );
    }

    for ( ; ; )
    {
	int  vnum;
	int  iHash;

	letter				= fread_letter( fp );
	if ( letter != '#' )
	{
	    bug( "Load_mobiles: # not found.", 0 );
	    exit( 1 );
	}

	vnum				= fread_number( fp );
	if ( vnum == 0 )
	    break;

	fBootDb = FALSE;
	if ( get_mob_index( vnum ) )
	{
	    bug( "Load_mobiles: vnum %d duplicated.", vnum );
	    exit( 1 );
	}
	fBootDb = TRUE;

	pMobIndex			= alloc_perm( sizeof( *pMobIndex ) );
	pMobIndex->vnum			= vnum;
	pMobIndex->area			= area_last;		/* OLC */
	pMobIndex->player_name		= fread_string( fp );
	pMobIndex->short_descr		= fread_string( fp );
	pMobIndex->long_descr		= fread_string( fp );
	pMobIndex->description		= fread_string( fp );

	pMobIndex->long_descr[0]	= UPPER( pMobIndex->long_descr[0]  );
	pMobIndex->description[0]	= UPPER( pMobIndex->description[0] );

	pMobIndex->act			= fread_number( fp ) | ACT_IS_NPC;
	pMobIndex->affected_by		= fread_number( fp );
	pMobIndex->affected_by2       	= fread_number( fp );
	pMobIndex->pShop		= NULL;
	pMobIndex->pTattoo		= NULL;
	pMobIndex->alignment		= fread_number( fp );
	letter				= fread_letter( fp );
 	pMobIndex->ally_vnum		= fread_number( fp );
 	pMobIndex->ally_level		= fread_number( fp );
 	pMobIndex->race_type		= fread_number( fp );
 	pMobIndex->fighting_style	= fread_number( fp );
 	pMobIndex->size			= fread_number( fp );
	pMobIndex->level		= fread_number( fp );

	pMobIndex->hitroll            	= fread_number( fp );   
	pMobIndex->p_damp             	= fread_number( fp );   
	pMobIndex->m_damp		= fread_number( fp );   
	pMobIndex->hit_modi           	= fread_number( fp );   
	pMobIndex->skin               	= fread_number( fp );   
        pMobIndex->money.gold         	= fread_number( fp );
        pMobIndex->money.silver       	= fread_number( fp );
        pMobIndex->money.copper       	= fread_number( fp );
	pMobIndex->sex			= fread_number( fp );
	pMobIndex->imm_flags          	= fread_number( fp );
	pMobIndex->res_flags          	= fread_number( fp );
	pMobIndex->vul_flags          	= fread_number( fp );

	pMobIndex->perm_str          	= fread_number( fp );
	pMobIndex->perm_int          	= fread_number( fp );
	pMobIndex->perm_wis          	= fread_number( fp );
	pMobIndex->perm_dex          	= fread_number( fp );
	pMobIndex->perm_con          	= fread_number( fp );
	pMobIndex->perm_agi          	= fread_number( fp );
	pMobIndex->perm_cha          	= fread_number( fp );

	if ( letter != 'S' )
	{
	    bug( "Load_mobiles: vnum %d non-S.", vnum );
	    exit( 1 );
	}
        letter = fread_letter( fp );
        if ( letter == '>' )
        {
          ungetc( letter, fp );
          mprog_read_programs( fp, pMobIndex );
        }
        else ungetc( letter,fp );
/* XORPHOX */
/* This stuff is already in there Xor.. *POKE*
 * Grumble.. now we hafta leave it here or it'll cause errors
 * Sheesh.. -- Altrag
 */
	for ( ; ; ) /* only way so far */
	{
	    letter = fread_letter( fp );

	    if ( letter == 'F' )
	    {
              num = fread_number(fp);
              switch(num)
              {
                case 1: /* imm_flags */
                  pMobIndex->imm_flags		= fread_number(fp);
                break;
                case 2: /* res_flags */
                  pMobIndex->res_flags		= fread_number(fp);
                break;
                case 3: /* vul_flags */
                  pMobIndex->vul_flags		= fread_number(fp);
                break;
              }
	    }
	    else
	    {
		ungetc(letter, fp);
		break;
	    }
	}
/* END */

	iHash			= vnum % MAX_KEY_HASH;
	pMobIndex->next		= mob_index_hash[iHash];
	mob_index_hash[iHash]	= pMobIndex;
	top_mob_index++;
	assign_area_vnum( vnum );				   /* OLC */
	kill_table[URANGE( 0, pMobIndex->level, MAX_LEVEL-1 )].number++;
    }

    return;
}



/*
 * Snarf an obj section.
 */
void load_objects( FILE *fp )
{
    OBJ_INDEX_DATA *pObjIndex;

    if ( !area_last )	/* OLC */
    {
	bug( "Load_objects: no #AREA seen yet.", 0 );
	exit( 1 );
    }

    for ( ; ; )
    {
        char *value [ 10 ];
	char  letter;
	int   vnum;
	int   iHash;

	letter				= fread_letter( fp );
	if ( letter != '#' )
	{
	    bug( "Load_objects: # not found.", 0 );
	    exit( 1 );
	}

	vnum				= fread_number( fp );
	if ( vnum == 0 )
	    break;

	fBootDb = FALSE;
	if ( get_obj_index( vnum ) )
	{
	    bug( "Load_objects: vnum %d duplicated.", vnum );
	    exit( 1 );
	}
	fBootDb = TRUE;

	pObjIndex=alloc_perm(sizeof(*pObjIndex )); 
	pObjIndex->vnum			= vnum;
        pObjIndex->area			= area_last;		/* OLC */
	pObjIndex->name			= fread_string( fp );
	pObjIndex->short_descr		= fread_string( fp );
	pObjIndex->description		= fread_string( fp );

	pObjIndex->short_descr[0]	= LOWER( pObjIndex->short_descr[0] );
	pObjIndex->description[0]	= UPPER( pObjIndex->description[0] );

	pObjIndex->item_type		= fread_number( fp );
	pObjIndex->extra_flags		= fread_number( fp );
	pObjIndex->wear_flags		= fread_number( fp );
	pObjIndex->bionic_flags		= fread_number( fp );
        pObjIndex->composition		= fread_number( fp );
        pObjIndex->durability		= fread_number( fp );
        pObjIndex->level		= fread_number( fp );
	value[0]		        = fread_string( fp );
	value[1]		        = fread_string( fp );
	value[2]		        = fread_string( fp );
	value[3]		        = fread_string( fp );
	value[4]		        = fread_string( fp );
	value[5]		        = fread_string( fp );
	value[6]		        = fread_string( fp );
	value[7]		        = fread_string( fp );
	value[8]		        = fread_string( fp );
	value[9]		        = fread_string( fp );
	pObjIndex->weight		= fread_number( fp );
	pObjIndex->cost.gold		= fread_number( fp );
	pObjIndex->cost.silver		= fread_number( fp );
	pObjIndex->cost.copper		= fread_number( fp );
	pObjIndex->invoke_type              = fread_number( fp );
	pObjIndex->invoke_vnum              = fread_number( fp );
	pObjIndex->invoke_spell             = fread_string( fp );
	pObjIndex->invoke_charge[0]         = fread_number( fp );
	pObjIndex->invoke_charge[1]         = fread_number( fp ); 
	pObjIndex->join                 = fread_number( fp );
	pObjIndex->sep_one              = fread_number( fp );
	pObjIndex->sep_two              = fread_number( fp );

	if ( pObjIndex->item_type == ITEM_POTION )
	    SET_BIT( pObjIndex->extra_flags, ITEM_NO_LOCATE );
	    
	for ( ; ; )
	{
	    char letter;

	    letter = fread_letter( fp );

	    if ( letter == 'A' )
	    {
		AFFECT_DATA *paf;

		paf			= new_affect();
		paf->type		= -1;
		paf->level		= pObjIndex->level;
		paf->duration		= -1;
		paf->location		= fread_number( fp );
		paf->modifier		= fread_number( fp );
		paf->bitvector		= 0;
		paf->next		= pObjIndex->affected;
		pObjIndex->affected	= paf;
	    }

	    else if ( letter == 'E' )
	    {
		EXTRA_DESCR_DATA *ed;

		ed			= new_extra_descr();
		ed->keyword		= fread_string( fp );
		ed->description		= fread_string( fp );
		ed->next		= pObjIndex->extra_descr;
		pObjIndex->extra_descr	= ed;
	    }

	    else
	    {
		ungetc( letter, fp );
		break;
	    }
	}

	/*
	 * Translate character strings *value[] into integers:  sn's for
	 * items with spells, or straight conversion for other items.
	 * - Thelonius
	 */
	switch ( pObjIndex->item_type )
	{
	default:
	    pObjIndex->value[0] = atoi( value[0] );
	    pObjIndex->value[1] = atoi( value[1] );
	    pObjIndex->value[2] = atoi( value[2] );
	    pObjIndex->value[3] = atoi( value[3] );
	    pObjIndex->value[4] = atoi( value[4] );
	    pObjIndex->value[5] = atoi( value[5] );
	    pObjIndex->value[6] = atoi( value[6] );
	    pObjIndex->value[7] = atoi( value[7] );
	    pObjIndex->value[8] = atoi( value[8] );
	    pObjIndex->value[9] = atoi( value[9] );
	    break;

	case ITEM_PILL:
	case ITEM_POTION:
	case ITEM_SCROLL:
	    pObjIndex->value[0] = atoi( value[0] );
	    pObjIndex->value[1] = skill_lookup( value[1] );
	    pObjIndex->value[2] = skill_lookup( value[2] );
	    pObjIndex->value[3] = skill_lookup( value[3] );
	    break;

	case ITEM_PORTAL:
	    pObjIndex->value[0] = atoi( value[0] );
	    pObjIndex->value[1] = atoi( value[1] );
	    break;
	    
	case ITEM_STAFF:
	case ITEM_LENSE:
	case ITEM_WAND:
	    pObjIndex->value[0] = atoi( value[0] );
	    pObjIndex->value[1] = atoi( value[1] );
	    pObjIndex->value[2] = atoi( value[2] );
	    pObjIndex->value[3] = skill_lookup( value[3] );
	    break;
	}

	if ( (letter = fread_letter( fp )) == '>' )
	{
	  ungetc( letter, fp );
	  load_traps( fp, pObjIndex, NULL, NULL );
	}
	else
	  ungetc( letter, fp );

	iHash			= vnum % MAX_KEY_HASH;
	pObjIndex->next		= obj_index_hash[iHash];
	obj_index_hash[iHash]	= pObjIndex;
	top_obj_index++; 
	assign_area_vnum( vnum );				   /* OLC */
    }

    return;
}



/*
 * Adds a reset to a room.  OLC
 * Similar to add_reset in olc.c
 */
void new_reset( ROOM_INDEX_DATA *pR, RESET_DATA *pReset )
{
    RESET_DATA *pr;

    if ( !pR )
       return;

    pr = pR->reset_last;

    if ( !pr )
    {
        pR->reset_first = pReset;
        pR->reset_last  = pReset;
    }
    else
    {
        pR->reset_last->next = pReset;
        pR->reset_last       = pReset;
        pR->reset_last->next = NULL;
    }

    top_reset++;
    return;
}



/*
 * Snarf a reset section.	Changed for OLC.
 */
void load_resets( FILE *fp )
{
    RESET_DATA	*pReset;
    int 	iLastRoom = 0;
    int 	iLastObj  = 0;

    if ( !area_last )
    {
	bug( "Load_resets: no #AREA seen yet.", 0 );
	exit( 1 );
    }

    for ( ; ; )
    {
	EXIT_DATA       *pexit;
	ROOM_INDEX_DATA *pRoomIndex;
	char             letter;

	if ( ( letter = fread_letter( fp ) ) == 'S' )
	    break;

	if ( letter == '*' )
	{
	    fread_to_eol( fp );
	    continue;
	}

	pReset		= new_reset_data();
	pReset->command	= letter;
	pReset->arg1	= fread_number( fp );
	pReset->arg2	= fread_number( fp );
	pReset->arg3	= ( letter == 'G' || letter == 'R' )
			    ? 0 : fread_number( fp );
			  fread_to_eol( fp );

	/*
	 * Validate parameters.
	 * We're calling the index functions for the side effect.
	 */
	switch ( letter )
	{
	default:
	    bug( "Load_resets: bad command '%c'.", letter );
	    exit( 1 );
	    break;

	case 'M':
	    get_mob_index  ( pReset->arg1 );
	    if ( ( pRoomIndex = get_room_index ( pReset->arg3 ) ) )
	    {
		new_reset( pRoomIndex, pReset );
		iLastRoom = pReset->arg3;
	    }
	    break;

	case 'O':
	    get_obj_index  ( pReset->arg1 );
	    if ( ( pRoomIndex = get_room_index ( pReset->arg3 ) ) )
	    {
		new_reset( pRoomIndex, pReset );
		iLastObj = pReset->arg3;
	    }
	    break;

	case 'P':
	    get_obj_index  ( pReset->arg1 );
	    if ( ( pRoomIndex = get_room_index ( iLastObj ) ) )
	    {
		new_reset( pRoomIndex, pReset );
	    }
	    break;

	case 'G':
	case 'E':
	    get_obj_index  ( pReset->arg1 );
	    if ( ( pRoomIndex = get_room_index ( iLastRoom ) ) )
	    {
		new_reset( pRoomIndex, pReset );
		iLastObj = iLastRoom;
	    }
	    break;

	case 'D':
	    pRoomIndex = get_room_index( pReset->arg1 );

	    if (   pReset->arg2 < 0
		|| pReset->arg2 > 5
		|| !pRoomIndex
		|| !( pexit = pRoomIndex->exit[pReset->arg2] )
		|| !IS_SET( pexit->rs_flags, EX_ISDOOR ) )
	    {
		bug( "Load_resets: 'D': exit %d not door.", pReset->arg2 );
		exit( 1 );
	    }

	    switch ( pReset->arg3 )	/* OLC 1.1b */
	    {
		default:
		    bug( "Load_resets: 'D': bad 'locks': %d." , pReset->arg3);
		case 0:
		    break;
		case 1: SET_BIT( pexit->rs_flags, EX_CLOSED );
		    break;
		case 2: SET_BIT( pexit->rs_flags, EX_CLOSED | EX_LOCKED );
		    break;
	    }
	    break;

	case 'R':
	    if ( pReset->arg2 < 0 || pReset->arg2 > 6 )	/* Last Door. */
	    {
		bug( "Load_resets: 'R': bad exit %d.", pReset->arg2 );
		exit( 1 );
	    }

	    if ( ( pRoomIndex = get_room_index( pReset->arg1 ) ) )
		new_reset( pRoomIndex, pReset );

	    break;
	}
    }

    return;
}

/*****************************************************************************
 Name:		new_load_rooms
 Purpose:	Loads rooms without the anoying case sequence.
 ****************************************************************************/
/* OLC 1.1b */
void new_load_rooms( FILE *fp )
{
    ROOM_INDEX_DATA *pRoomIndex;

    if ( !area_last )
    {
	bug( "Load_rooms: no #AREA seen yet.", 0 );
	exit( 1 );
    }

    for ( ; ; )
    {
	char letter;
	int  vnum;
	int  door;
	int  iHash;

	letter				= fread_letter( fp );
	if ( letter != '#' )
	{
	    bug( "Load_rooms: # not found.", 0 );
	    exit( 1 );
	}

	vnum				= fread_number( fp );
	if ( vnum == 0 )
	    break;

	fBootDb = FALSE;
	if ( get_room_index( vnum ) )
	{
	    bug( "Load_rooms: vnum %d duplicated.", vnum );
	    exit( 1 );
	}
	fBootDb = TRUE;

	pRoomIndex			= alloc_perm( sizeof( *pRoomIndex ) );
	pRoomIndex->people		= NULL;
	pRoomIndex->contents		= NULL;
	pRoomIndex->extra_descr		= NULL;
	pRoomIndex->area		= area_last;
	pRoomIndex->vnum		= vnum;
	pRoomIndex->name		= fread_string( fp );
	pRoomIndex->description		= fread_string( fp );
	pRoomIndex->room_flags		= fread_number( fp );
	pRoomIndex->sector_type		= fread_number( fp );

	pRoomIndex->ore_type		= fread_number( fp );
	pRoomIndex->ore_fertility	= fread_number( fp );
        pRoomIndex->ore_quantity =
         (pRoomIndex->ore_fertility * 5);

	pRoomIndex->light		= 0;
	pRoomIndex->rd                  = 0;

        pRoomIndex->temp		=
         area_last->average_temp + (dice(1,5) - 3);
        pRoomIndex->humidity		=
         area_last->average_humidity + (dice(1,5) - 3);
	
	for ( door = 0; door <= 5; door++ )
	    pRoomIndex->exit[door] = NULL;

	for ( ; ; )
	{
	    letter = fread_letter( fp );

	    if ( letter == 'R' )
	    {
	      char *word;

	      ungetc( letter, fp );
	      word = fread_word( fp );
	      if ( !str_cmp( word, "Rd" ) )
	      {
		if ( pRoomIndex->rd != 0 )
		  bug( "New_load_rooms: rd already assigned for room #%d; updating.", pRoomIndex->vnum );
		pRoomIndex->rd = fread_number( fp );
	      }
	      else
	      {
		bug( "Load_rooms: vnum %d has flag not 'DES'.", vnum );
		exit( 1 );
	      }
	      continue;
	    }

	    if ( letter == 'S' || letter == 's' )
	    {
		if ( letter == 's' )
		    bug( "Load_rooms: vnum %d has lowercase 's'", vnum );
		break;
	    }

	    if ( letter == 'D' )
	    {
		EXIT_DATA *pexit;
		int        locks;
		char       tLetter;

		door = fread_number( fp );
		if ( door < 0 || door > 5 )
		{
		    bug( "Fread_rooms: vnum %d has bad door number.", vnum );
		    exit( 1 );
		}

		pexit			= alloc_perm( sizeof( *pexit ));
		pexit->description	= fread_string( fp );
		pexit->keyword		= fread_string( fp );
		locks			= fread_number( fp );
		pexit->exit_info	= locks;
		pexit->rs_flags		= locks;
		pexit->key		= fread_number( fp );
		pexit->vnum		= fread_number( fp );

		if ( (tLetter = fread_letter( fp )) == '>' )
		{
		  ungetc( tLetter, fp );
		  load_traps( fp, NULL, NULL, pexit );
                  /*bug ( "Trap on exit in room %d.", pRoomIndex->vnum);*/
		}
		else
		  ungetc( tLetter, fp );

		pRoomIndex->exit[door]		= pexit;
		top_exit++;
	    }
	    else if ( letter == 'E' )
	    {
		EXTRA_DESCR_DATA *ed;

		ed			= alloc_perm( sizeof( *ed ));
		ed->keyword		= fread_string( fp );
		ed->description		= fread_string( fp );
		ed->next		= pRoomIndex->extra_descr;
		pRoomIndex->extra_descr	= ed;
		top_ed++;
	    }
	    else
	    {
		bug( "Load_rooms: vnum %d has flag not 'DES'.", vnum );
		exit( 1 );
	    }
	}

	if ( (letter = fread_letter( fp )) == '>' )
	{
	  ungetc( letter, fp );
	  load_traps( fp, NULL, pRoomIndex, NULL );
	  /*bug ( "Trap on room %d.", pRoomIndex->vnum );*/
	}
	else
	  ungetc( letter, fp );

	iHash			= vnum % MAX_KEY_HASH;
	pRoomIndex->next	= room_index_hash[iHash];
	room_index_hash[iHash]	= pRoomIndex;
	top_room++;
	assign_area_vnum( vnum );
    }

    return;
}



/*
 * Snarf a shop section.
 */
void load_shops( FILE *fp )
{
    SHOP_DATA *pShop;

    for ( ; ; )
    {
	MOB_INDEX_DATA *pMobIndex;
	int iTrade;

	pShop			= alloc_perm( sizeof( *pShop ) );
	pShop->keeper		= fread_number( fp );
	if ( pShop->keeper == 0 )
	    break;
	for ( iTrade = 0; iTrade < MAX_TRADE; iTrade++ )
	    pShop->buy_type[iTrade] = fread_number( fp );
	pShop->profit_buy	= fread_number( fp );
	pShop->profit_sell	= fread_number( fp );
	pShop->open_hour	= fread_number( fp );
	pShop->close_hour	= fread_number( fp );

	pMobIndex		= get_mob_index( pShop->keeper );
	pMobIndex->pShop	= pShop;

	if ( !shop_first )
	    shop_first = pShop;
	if (  shop_last  )
	    shop_last = pShop;

	shop_last	= pShop;
	pShop->next	= NULL;
	top_shop++;
    }

    return;
}

void load_casinos( FILE *fp )
{
    CASINO_DATA *pCasino;

    for ( ; ; )
    {
	MOB_INDEX_DATA *pMobIndex;

	pCasino			= alloc_perm( sizeof( *pCasino ) );
	pCasino->dealer		= fread_number( fp );
	if ( pCasino->dealer == 0 )
	    break;

	pCasino->game			= fread_number( fp );
	pCasino->pot			= fread_number( fp );
	pCasino->ante_min		= fread_number( fp );
	pCasino->ante_max		= fread_number( fp );

	pMobIndex		= get_mob_index( pCasino->dealer );
	pMobIndex->pCasino	= pCasino;

	if ( !casino_first )
	    casino_first = pCasino;
	if (  casino_last  )
	    casino_last = pCasino;

	casino_last	= pCasino;
	pCasino->next	= NULL;
	top_casino++;
    }

    return;
}

void load_tattoo_artists( FILE *fp )
{
	TATTOO_ARTIST_DATA *pTattoo;
	MOB_INDEX_DATA *pMobIndex;


    for ( ; ; )
    {

	pTattoo			= alloc_perm( sizeof( *pTattoo ) );
	pTattoo->artist		= fread_number( fp );
	if ( pTattoo->artist == 0 )
        break;

	pTattoo->cost.gold	= fread_number( fp );
	pTattoo->cost.silver	= fread_number( fp );
	pTattoo->cost.copper	= fread_number( fp );
	pTattoo->magic_boost	= fread_number( fp );
	pTattoo->wear_loc	= fread_number( fp );

	for ( ; ; )
	{
	    char letter;

	    letter = fread_letter( fp );

	    if ( letter == 'A' )
	    {
		AFFECT_DATA *paf;

		paf			= new_affect();
		paf->type		= -1;
		paf->level		=  1;
		paf->duration		= -1;
		paf->location		= fread_number( fp );
		paf->modifier		= fread_number( fp );
		paf->bitvector		= 0;
		paf->next		= pTattoo->affected;
		pTattoo->affected	= paf;
	    }
	    else
	    {
		ungetc( letter, fp );
		break;
	    }
         }
	pMobIndex		= get_mob_index( pTattoo->artist );
	pMobIndex->pTattoo	= pTattoo;

	if ( !tattoo_artist_first )
	    tattoo_artist_first = pTattoo;
	if (  tattoo_artist_last  )
	    tattoo_artist_last = pTattoo;

	tattoo_artist_last	= pTattoo;
	pTattoo->next		= NULL;
	top_tattoo_artist++;
      }
    return;
}



/*
 * Snarf spec proc declarations.
 */
void load_specials( FILE *fp )
{
    for ( ; ; )
    {
	MOB_INDEX_DATA *pMobIndex;
	char letter;

	switch ( letter = fread_letter( fp ) )
	{
	default:
	    bug( "Load_specials: letter '%c' not *MS.", letter );
	    exit( 1 );

	case 'S':
	    return;

	case '*':
	    break;

	case 'M':
	    pMobIndex           = get_mob_index ( fread_number ( fp ) );
	    pMobIndex->spec_fun = spec_lookup   ( fread_word   ( fp ) );
	    if ( pMobIndex->spec_fun == 0 )
	    {
		bug( "Load_specials: 'M': vnum %d.", pMobIndex->vnum );
		exit( 1 );
	    }
	    break;
	}

	fread_to_eol( fp );
    }
}



void load_clans( void )
{
    FILE      *fp;
    CLAN_DATA *pClanIndex;
    char letter;

    if ( !( fp = fopen( CLAN_FILE, "r" ) ) )
	return;
    fpArea = fp; 
    strcpy(strArea, CLAN_FILE);
    for ( ; ; )
    {
	int  vnum;

	letter				= fread_letter( fp );
	if ( letter != '#' )
	{
	    bug( "Load_clans: # not found.", 0 );
	    continue;
	}

	vnum				= fread_number( fp );
	if ( vnum == 999 )
	    break;

	fBootDb = FALSE;
	if ( get_clan_index( vnum ) )
	{
	    bug( "Load_clans: vnum %d duplicated.", vnum );
	    break;
	}
	fBootDb = TRUE;
	pClanIndex			= alloc_perm( sizeof( *pClanIndex ));
	pClanIndex->vnum		= vnum;
	pClanIndex->bankaccount.gold	= fread_number( fp );
	pClanIndex->bankaccount.silver  = fread_number( fp );
	pClanIndex->bankaccount.copper  = fread_number( fp );
	pClanIndex->name		= fread_string( fp );
	pClanIndex->init		= fread_string( fp );
	pClanIndex->warden              = fread_string( fp );
	pClanIndex->description         = fread_string( fp );
	pClanIndex->champ               = fread_string( fp );

	pClanIndex->leader              = fread_string( fp );
	pClanIndex->first               = fread_string( fp );
	pClanIndex->second              = fread_string( fp );
	pClanIndex->ischamp             = fread_number( fp );
	pClanIndex->isleader            = fread_number( fp );
	pClanIndex->isfirst             = fread_number( fp );
	pClanIndex->issecond            = fread_number( fp );
	pClanIndex->recall		= fread_number( fp );
	pClanIndex->pkills		= fread_number( fp );
	pClanIndex->mkills		= fread_number( fp );
	pClanIndex->members		= fread_number( fp );
	pClanIndex->pdeaths		= fread_number( fp );
	pClanIndex->mdeaths		= fread_number( fp );
	pClanIndex->obj_vnum_1		= fread_number( fp );
	pClanIndex->obj_vnum_2		= fread_number( fp );
	pClanIndex->obj_vnum_3		= fread_number( fp );
        pClanIndex->settings            = fread_number( fp );
	clan_sort(pClanIndex);
	top_clan++;
    }
    fclose ( fp );   
 
    return;
}

void social_sort( SOCIAL_DATA *pSocial )
{
  SOCIAL_DATA *fSocial;
  if ( !social_first )
  {
    social_first = pSocial;
    social_last  = pSocial;
    return;
  }

  if ( strncmp( pSocial->name, social_first->name, 256 ) > 0 )
  {
   pSocial->next = social_first->next;
   social_first = pSocial;
   return;
  } 

  for ( fSocial = social_first; fSocial; fSocial = fSocial->next )
  {
    if (    ( strncmp( pSocial->name, fSocial->name, 256 ) < 0 ) )
    {
      if ( fSocial != social_last )
      { 
        pSocial->next = fSocial->next;
        fSocial->next = pSocial;
        return;
       } 
    }
  }

  social_last->next = pSocial;
  social_last = pSocial;
  pSocial->next = NULL;
  return;
}

/* Decklarean */

void load_socials( void )
{
    FILE      *fp;
    SOCIAL_DATA *pSocialIndex;
    char letter;

    if ( !( fp = fopen( SOCIAL_FILE, "r" ) ) )
	return;
    fpArea = fp;
    strcpy(strArea, SOCIAL_FILE);
    for ( ; ; )
    {
	char*  name;

	letter				= fread_letter( fp );
	if ( letter != '#' )
	{
	    bug( "Load_socials: # not found.", 0 );
	    continue;
	}

	name				= fread_string( fp );
	if ( !str_cmp( name, "END"    ) )
	  break;

	pSocialIndex		= 	alloc_perm( sizeof( *pSocialIndex ));
	pSocialIndex->name		=	name;
	pSocialIndex->char_no_arg	=	fread_string( fp );
	pSocialIndex->others_no_arg	=	fread_string( fp );
	pSocialIndex->char_found	=	fread_string( fp );
	pSocialIndex->others_found	=	fread_string( fp );
	pSocialIndex->vict_found	=	fread_string( fp );
	pSocialIndex->char_auto		=	fread_string( fp );
	pSocialIndex->others_auto	=	fread_string( fp );
	social_sort(pSocialIndex);
	top_social++;
    }
    fclose ( fp );

    return;
}


/*
 * Snarf notes file.
 */
void load_notes( void )
{
    FILE      *fp;
    NOTE_DATA *pnotelast;

    if ( !( fp = fopen( NOTE_FILE, "r" ) ) )
	return;
    fpArea = fp;
    strcpy(strArea, NOTE_FILE);

    pnotelast = NULL;
    for ( ; ; )
    {
	NOTE_DATA *pnote;
	char       letter;

	do
	{
	    letter = getc( fp );
	    if ( feof(fp) )
	    {
		fclose( fp );
		return;
	    }
	}
	while ( isspace( letter ) );
	ungetc( letter, fp );

	pnote		  = alloc_perm( sizeof( *pnote ) );

	if ( str_cmp( fread_word( fp ), "sender" ) )
	    break;
	pnote->sender     = fread_string( fp );

	if ( str_cmp( fread_word( fp ), "date" ) )
	    break;
	pnote->date       = fread_string( fp );

	if ( str_cmp( fread_word( fp ), "stamp" ) )
	    break;
	pnote->date_stamp = fread_number( fp );

	if ( str_cmp( fread_word( fp ), "to" ) )
	    break;
	pnote->to_list    = fread_string( fp );

	if ( str_cmp( fread_word( fp ), "subject" ) )
	    break;
	pnote->subject    = fread_string( fp );

	pnote->protected = FALSE;
	pnote->on_board = 0;
	{
	  char letter;

	  letter = fread_letter( fp );
	  ungetc( letter, fp );
	  if ( letter == 'P' && !str_cmp( fread_word( fp ), "protect" ) )
	    pnote->protected  = fread_number( fp );
	  letter = fread_letter( fp );
	  ungetc( letter, fp );
	  if ( letter == 'B' && !str_cmp( fread_word( fp ), "board" ) )
	    pnote->on_board   = fread_number( fp );
	}

	if ( str_cmp( fread_word( fp ), "text" ) )
	    break;
	pnote->text       = fread_string( fp );

	if ( !note_list )
	    note_list           = pnote;
	else
	    pnotelast->next     = pnote;

	pnotelast               = pnote;
    }

    bug( "Load_notes: bad key word.", 0 );
    fclose(fp);
    return;
}


void load_down_time( void )
{
    FILE *fp;

    down_time = str_dup ( "*" );
    warning1  = str_dup ( "*" );
    warning2  = str_dup ( "*" );
    stype     = 1;

    if ( !( fp = fopen( DOWN_TIME_FILE, "r" ) ) )
        return;

    fpArea = fp;
    strcpy(strArea, DOWN_TIME_FILE);
    for ( ; ; )
    {
        char *word;
	char  letter;

	do
	{
	    letter = getc( fp );
	    if ( feof( fp ) )
	    {
		fclose( fp );
		return;
	    }
	}
	while ( isspace( letter ) );
	ungetc( letter, fp );
	
	word = fread_word( fp );

	if ( !str_cmp( word, "DOWNTIME" ) )
	{
	    free_string( down_time );
	    down_time = fread_string( fp );
	}
	if ( !str_cmp( word, "WARNINGA" ) )
	{
	    free_string( warning1 );
	    warning1 = fread_string( fp );
	}
	if ( !str_cmp( word, "WARNINGB" ) )
	{
	    free_string( warning2 );
	    warning2 = fread_string( fp );
	}
    }
}

/*
 * Translate all room exits from virtual to real.
 * Has to be done after all rooms are read in.
 * Check for bad or suspicious reverse exits.
 */
void fix_exits( void )
{
		 EXIT_DATA       *pexit;
		 ROOM_INDEX_DATA *pRoomIndex;
		 int              iHash;
		 int              door;

    for ( iHash = 0; iHash < MAX_KEY_HASH; iHash++ )
    {
	for ( pRoomIndex  = room_index_hash[iHash];
	      pRoomIndex;
	      pRoomIndex  = pRoomIndex->next )
	{
	    bool fexit;

	    fexit = FALSE;
	    for ( door = 0; door <= 5; door++ )
	    {
		if ( ( pexit = pRoomIndex->exit[door] ) )
		{
		    fexit = TRUE;
		    if ( pexit->vnum <= 0 )
			pexit->to_room = NULL;
		    else
			pexit->to_room = get_room_index( pexit->vnum );
		}
	    }

	    if ( !fexit )
		SET_BIT( pRoomIndex->room_flags, ROOM_NO_MOB );
	}
    }

    return;
}



/*
 * Repopulate areas periodically.
 */
void area_update( void )
{
    AREA_DATA *pArea;

    for ( pArea = area_first; pArea; pArea = pArea->next )
    {
        weather_update(pArea);

	if ( ++pArea->age < 3 )
	    continue;
       
	/*
	 * Check age and reset.
	 * Note: Mud School resets every 3 minutes (not 15).
	 */
	if ( pArea->nplayer == 0 || pArea->age >= 15 )
	{
	    reset_area( pArea );
	    pArea->age = number_range( 0, 3 );
	    if ( IS_SET( pArea->area_flags, AREA_MUDSCHOOL) )
		pArea->age = 15 - 3;
	}
    }

    return;
}

/* OLC
 * Reset one room.  Called by reset_area and olc.
 */
void reset_room( ROOM_INDEX_DATA *pRoom )
{
    RESET_DATA	*pReset;
    CHAR_DATA	*pMob;
    OBJ_DATA	*pObj;
    CHAR_DATA	*LastMob = NULL;
    OBJ_DATA	*LastObj = NULL;
    int iExit;
    int level = 0;
    bool last;

    if ( !pRoom )
	return;

    pMob	= NULL;
    last	= FALSE;
    
    for ( iExit = 0;  iExit < MAX_DIR;  iExit++ )
    {
	EXIT_DATA *pExit;
	if ( ( pExit = pRoom->exit[iExit] )
	  && !IS_SET( pExit->exit_info, EX_BASHED ) )	/* Skip Bashed. */
	{
	    pExit->exit_info = pExit->rs_flags;
	    if ( ( pExit->to_room != NULL )
	      && ( ( pExit = pExit->to_room->exit[rev_dir[iExit]] ) ) )
	    {
		/* nail the other side */
		pExit->exit_info = pExit->rs_flags;
	    }
	}
    }

    for ( pReset = pRoom->reset_first; pReset != NULL; pReset = pReset->next )
    {
	MOB_INDEX_DATA	*pMobIndex;
	OBJ_INDEX_DATA	*pObjIndex;
	OBJ_INDEX_DATA	*pObjToIndex;
	ROOM_INDEX_DATA	*pRoomIndex;
        int             counter;

	switch ( pReset->command )
	{
	default:
		sprintf(log_buf, "Reset_room: bad command %c in room %d.", 
		pReset->command, pRoom->vnum );
		bug( log_buf, 0 ); 
		break;

	case 'M':
	    if ( !( pMobIndex = get_mob_index( pReset->arg1 ) ) )
	    {
		sprintf(log_buf,  "Reset_room: 'M': bad vnum %d in room %d.", 
		pReset->arg1, pRoom->vnum );
		bug( log_buf, 0 ); 
		continue;
	    }

	    /*
	     * Some hard coding.
	     */
	    if ( pMobIndex->count >= pReset->arg2 )
	    {
		last = FALSE;
		break;
	    }

	    pMob = create_mobile( pMobIndex );

	    /*
	     * Some more hard coding.
	     */
	    if ( room_is_dark( pRoom ) )
		SET_BIT(pMob->affected_by, AFF_INFRARED);

	    /*
	     * Pet shop mobiles get ACT_PET set.
	     */
	    {
		ROOM_INDEX_DATA *pRoomIndexPrev;

		pRoomIndexPrev = get_room_index( pRoom->vnum - 1 );
		if ( pRoomIndexPrev
		    && IS_SET( pRoomIndexPrev->room_flags, ROOM_PET_SHOP ) )
		    SET_BIT( pMob->act, ACT_PET);
	    }

	    char_to_room( pMob, pRoom );

           /* I feel this is quite keen actually -Flux */
            act( C_DEFAULT, "$n enters.", pMob, NULL, NULL, TO_ROOM );

	    LastMob = pMob;
	    level  = URANGE( 0, pMob->level - 2, LEVEL_HERO );
	    last = TRUE;
	    break;

	case 'O':
	    if ( !( pObjIndex = get_obj_index( pReset->arg1 ) ) )
	    {
		sprintf( log_buf, "Reset_room: 'O': bad vnum %d in room %d.", 
		pReset->arg1, pRoom->vnum );
		bug( log_buf, 0 ); 
		continue;
	    }

	    if ( !( pRoomIndex = get_room_index( pReset->arg3 ) ) )
	    {
		sprintf( log_buf, "Reset_room: 'O': bad vnum %d in room %d.", 
		pReset->arg3, pRoom->vnum );
		bug( log_buf, 0 ); 
		continue;
	    }

	    if ( pRoom->area->nplayer > 0 
	    || count_obj_list( pObjIndex, pRoom->contents ) >= pReset->arg2 )
		break;

           for( counter =
            (pReset->arg2 - count_obj_list( pObjIndex, pRoom->contents ));
            counter > 0; counter-- )
           {
	    pObj = create_object( pObjIndex, level);
	    obj_to_room( pObj, pRoom );
           }
	    break;

	case 'P':
	    if ( !( pObjIndex = get_obj_index( pReset->arg1 ) ) )
	    {
		sprintf( log_buf, "Reset_room: 'P': bad vnum %d in room %d.", 
		pReset->arg1, pRoom->vnum );
		bug( log_buf, 0 ); 
		continue;
	    }

	    if ( !( pObjToIndex = get_obj_index( pReset->arg3 ) ) )
	    {
		sprintf( log_buf, "Reset_room: 'P': bad vnum %d in room %d.", 
		pReset->arg3, pRoom->vnum );		
		bug( log_buf, 0 ); 
		continue;
	    }

	    if ( pRoom->area->nplayer > 0
	    || !( LastObj = get_obj_type( pObjToIndex ) )
	    || count_obj_list( pObjIndex, LastObj->contains ) >= pReset->arg2 )
		break;

           for( counter =
            (pReset->arg2 - count_obj_list(pObjIndex, LastObj->contains));
            counter > 0; counter-- )
           {
	    pObj = create_object(pObjIndex,level);
	    obj_to_obj( pObj, LastObj );
           }

	    /*
	     * Ensure that the container gets reset.	OLC 1.1b
	     */
	    if ( LastObj->item_type == ITEM_CONTAINER )
	    {
		LastObj->value[1] = LastObj->pIndexData->value[1];
	    }
	    else
	    {
	    	    /* THIS SPACE INTENTIONALLY LEFT BLANK */
	    }
	    break;

	case 'G':
	case 'E':
	    if ( !( pObjIndex = get_obj_index( pReset->arg1 ) ) )
	    {
		sprintf( log_buf, "Reset_room: 'E' or 'G': bad vnum %d in room %d.", 
		pReset->arg1, pRoom->vnum );
		bug( log_buf, 0 );
		continue; 
	    }

	    if ( !last )
		break;

	    if ( !LastMob )
	    {
		sprintf( log_buf, "Reset_room: 'E' or 'G': null mob for vnum %d in room %d.",
		    pReset->arg1, pRoom->vnum );
		    bug( log_buf, 0 ); 
		last = FALSE;
		break;
	    }

	    if ( LastMob->pIndexData->pShop )	/* Shop-keeper? */
	    {
		int olevel;

		switch ( pObjIndex->item_type )
		{
		default:                olevel = 0;                      break;
		case ITEM_PILL:         olevel = number_range(  0, 10 ); break;
		case ITEM_POTION:	olevel = number_range(  0, 10 ); break;
		case ITEM_SCROLL:	olevel = number_range(  5, 15 ); break;
		case ITEM_WAND:		olevel = number_range( 10, 20 ); break;
		case ITEM_LENSE:        olevel = number_range( 10, 20 ); break;
		case ITEM_STAFF:	olevel = number_range( 15, 25 ); break;
		case ITEM_ARMOR:	olevel = number_range(  5, 15 ); break;
		case ITEM_WEAPON:	if ( pReset->command == 'G' )
		                            olevel = number_range( 5, 15 );
		                        else
					    olevel = number_fuzzy( level );
		  break;
		}

		pObj = create_object( pObjIndex, olevel );
		if ( pReset->command == 'G' )
		    SET_BIT( pObj->extra_flags, ITEM_INVENTORY );
	    }
	    else
	    {
  		pObj = create_object( pObjIndex, level);
	    }
            obj_to_char( pObj, LastMob );

	    if ( pReset->command == 'E' )
            {
             if ( IS_SET( pObj->wear_flags, ITEM_WIELD ) )
             {
              if ( get_eq_char( LastMob, WEAR_WIELD ) )
	       equip_char( LastMob, pObj, FALSE );
              else
	       equip_char( LastMob, pObj, TRUE );
             }
             else
	     equip_char( LastMob, pObj, FALSE );
            }
	    last = TRUE;
	    break;

	case 'D':
	    break;

	case 'R':
/* OLC 1.1b */
	    if ( !( pRoomIndex = get_room_index( pReset->arg1 ) ) )
	    {
		sprintf( log_buf, "Reset_room: 'R': bad vnum %d in room %d.", 
		pReset->arg1, pRoom->vnum );
		bug( log_buf, 0 ); 
		continue;
	    }

		if ( pRoomIndex->area->builders[0] != '\0' )
			continue;

	    {
		EXIT_DATA *pExit;
		int d0;
		int d1;

		for ( d0 = 0; d0 < pReset->arg2; d0++ )
		{
		    d1                   = number_range( d0, pReset->arg2 );
		    pExit                = pRoomIndex->exit[d0];
		    pRoomIndex->exit[d0] = pRoomIndex->exit[d1];
		    pRoomIndex->exit[d1] = pExit;
		}
	    }
	    break;
	}
    }

    return;
}



/* OLC
 * Reset one area.
 */
void reset_area( AREA_DATA *pArea )
{
    ROOM_INDEX_DATA *pRoom;
    int  vnum;

    for ( vnum = pArea->lvnum; vnum <= pArea->uvnum; vnum++ )
    {
	if ( ( pRoom = get_room_index(vnum) ) )
	    reset_room(pRoom);
    }

    return;
}



/*
 * Create an instance of a mobile.
 */
CHAR_DATA *create_mobile( MOB_INDEX_DATA *pMobIndex )
{
    CHAR_DATA *mob;

    mobs_in_game++;

    if ( !pMobIndex )
    {
	bug( "Create_mobile: NULL pMobIndex.", 0 );
	exit( 1 );
    }

    if ( !char_free )
    {
	mob		= alloc_perm( sizeof( *mob ) );
    }
    else
    {
	mob		= char_free;
	char_free	= char_free->next;
    }

    clear_char( mob );
    mob->pIndexData     = pMobIndex;

    mob->race_type	= pMobIndex->race_type;
    mob->fighting_style	= pMobIndex->fighting_style;
    mob->size		= pMobIndex->size;
    mob->name		= str_dup( pMobIndex->player_name );	/* OLC */
    mob->short_descr	= str_dup( pMobIndex->short_descr );	/* OLC */
    mob->long_descr	= str_dup( pMobIndex->long_descr );	/* OLC */
    mob->description	= str_dup( pMobIndex->description );	/* OLC */
    mob->spec_fun	= pMobIndex->spec_fun;
    mob->prompt         = str_dup( "<%hhp %mm %vmv> " );

    mob->level		= pMobIndex->level;
    mob->act		= pMobIndex->act | ACT_IS_NPC;
    mob->affected_by	= pMobIndex->affected_by;
    mob->affected_by2   = pMobIndex->affected_by2;
    mob->imm_flags	= pMobIndex->imm_flags;
    mob->res_flags      = pMobIndex->res_flags;
    mob->vul_flags      = pMobIndex->vul_flags;

    mob->alignment	= pMobIndex->alignment;
    mob->sex		= pMobIndex->sex;

    mob->skin		= pMobIndex->skin;
    mob->hitroll	= pMobIndex->hitroll;
    mob->p_damp		= pMobIndex->p_damp;
    mob->m_damp		= pMobIndex->m_damp;
    mob->money.gold  	= pMobIndex->money.gold;
    mob->money.silver	= pMobIndex->money.silver;
    mob->money.copper	= pMobIndex->money.copper;
    mob->perm_hit	=
    (pMobIndex->hit_modi * (mob->level * ((2*mob->level)/4) ) ) + 200;
    mob->mod_hit	= 0;
    mob->hit		= MAX_HIT(mob);
    mob->combat_timer	= 0;			/* XOR */
    mob->hunttimer	= 0;			
    mob->summon_timer	= -1;			/* XOR */
    mob->doing[0] 		= '\0';

    mob->start_align = (IS_GUILTY(mob) ? 'E' : IS_FRAMED(mob) ? 'G' : 'N');
    /*
     * Insert in list.
     */
    mob->next		= char_list;
    char_list		= mob;
    pMobIndex->count++;
    return mob;
}

/* duplicate a mobile exactly -- except inventory */
void clone_mobile(CHAR_DATA *parent, CHAR_DATA *clone)
{
    AFFECT_DATA *paf;

    if ( parent == NULL || clone == NULL || !IS_NPC(parent))
	return;

    mobs_in_game++;    
    /* start fixing values */ 
    clone->name 	= str_dup(parent->name);
    clone->short_descr	= str_dup(parent->short_descr);
    clone->long_descr	= str_dup(parent->long_descr);
    clone->description	= str_dup(parent->description);
    clone->sex		= parent->sex;
    clone->race		= parent->race;
    clone->level	= parent->level;
    clone->trust	= 0;
    clone->timer	= parent->timer;
    clone->wait		= parent->wait;
    clone->hit		= parent->hit;
    clone->mod_hit	= parent->mod_hit;
    clone->perm_hit	= parent->perm_hit;
    clone->mana		= parent->mana;
    clone->perm_mana	= parent->perm_mana;
    clone->mod_mana	= parent->mod_mana;
    clone->move		= parent->move;
    clone->perm_move	= parent->perm_move;
    clone->mod_move	= parent->mod_move;
    clone->money.gold   = parent->money.gold;
    clone->money.silver	= parent->money.silver;
    clone->money.copper = parent->money.copper;
    clone->exp		= parent->exp;
    clone->act		= parent->act;
    clone->affected_by	= parent->affected_by;
    clone->position	= parent->position;
    clone->practice	= parent->practice;
    clone->alignment	= parent->alignment;
    clone->hitroll	= parent->hitroll;
    clone->damroll	= parent->damroll;
    clone->wimpy	= parent->wimpy;
    clone->spec_fun	= parent->spec_fun;
    clone->p_damp	= parent->p_damp;    
    clone->m_damp	= parent->m_damp;    
    clone->skin		= parent->skin;    

    /* now add the affects */
    for (paf = parent->affected; paf != NULL; paf = paf->next)
        affect_to_char(clone,paf);

}

TATTOO_DATA *create_tattoo( CHAR_DATA *victim )
{
           TATTOO_DATA *tattoo;

    if ( !tattoo_free )
    {
	tattoo		= alloc_perm( sizeof( *tattoo ) );
    }
    else
    {
	tattoo		= tattoo_free;
	tattoo_free	= tattoo_free->next;
    }

    tattoo		= new_tattoo( );
    tattoo->short_descr	= " ( no short desc ) ";	/* OLC */
    tattoo->wear_loc	= -1;
    tattoo->magic_boost =  0;
    tattoo->deleted     = FALSE;

    tattoo->next	= tattoo_list;
    tattoo_list		= tattoo;

    return tattoo;
}

/*
 * Create an instance of an object.
 */
OBJ_DATA *create_object( OBJ_INDEX_DATA *pObjIndex, int level )
{
    static OBJ_DATA  obj_zero;
           OBJ_DATA *obj;

    if ( !pObjIndex )
    {
	bug( "Create_object: NULL pObjIndex.", 0 );
	bug( "Replacing with dummy object.", 0 );
	pObjIndex = get_obj_index( 1 );
    }

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
    obj->pIndexData	= pObjIndex;
    obj->in_room	= NULL;
    obj->level		= pObjIndex->level;
    obj->wear_loc	= WEAR_NONE;
    obj->bionic_loc	= -1;

    obj->name		= str_dup( pObjIndex->name );		/* OLC */
    obj->short_descr	= str_dup( pObjIndex->short_descr );	/* OLC */
    obj->description	= str_dup( pObjIndex->description );	/* OLC */
    obj->durability	= pObjIndex->durability;
    obj->composition	= pObjIndex->composition;
    obj->item_type	= pObjIndex->item_type;
    obj->extra_flags	= pObjIndex->extra_flags;
    obj->wear_flags	= pObjIndex->wear_flags;
    obj->bionic_flags	= pObjIndex->bionic_flags;
    obj->value[0]		= pObjIndex->value[0];
    obj->value[1]		= pObjIndex->value[1];
    obj->value[2]		= pObjIndex->value[2];
    obj->value[3]		= pObjIndex->value[3];
    obj->value[4]		= pObjIndex->value[4];
    obj->value[5]		= pObjIndex->value[5];
    obj->value[6]		= pObjIndex->value[6];
    obj->value[7]		= pObjIndex->value[7];
    obj->value[8]		= pObjIndex->value[8];
    obj->value[9]		= pObjIndex->value[9];
    obj->weight		= pObjIndex->weight;
    obj->cost.gold	= pObjIndex->cost.gold;
    obj->cost.silver	= pObjIndex->cost.silver;
    obj->cost.copper	= pObjIndex->cost.copper;
    obj->invoke_type        = pObjIndex->invoke_type;
    obj->invoke_vnum        = pObjIndex->invoke_vnum;
    obj->invoke_spell       = str_dup(pObjIndex->invoke_spell);
    obj->invoke_charge[0]   = pObjIndex->invoke_charge[0];
    obj->invoke_charge[1]   = pObjIndex->invoke_charge[1];
    obj->deleted        = FALSE;
    obj->timer		= -1;

    /*
     * Mess with object properties.
     */
    switch ( obj->item_type )
    {
    default:
	bug( "Read_object: vnum %d bad type.", pObjIndex->vnum );
	break;

    case ITEM_BOMB:
    case ITEM_TREASURE:
    case ITEM_FURNITURE:
    case ITEM_TRASH:
    case ITEM_CONTAINER:
    case ITEM_DRINK_CON:
    case ITEM_MISC:
    case ITEM_FOOD:
    case ITEM_BOAT:
    case ITEM_CORPSE_PC:
    case ITEM_FOUNTAIN:
    case ITEM_VODOO:
    case ITEM_BERRY:
    case ITEM_BLOOD:
    case ITEM_BODY_PART:
    case ITEM_RIGOR:
    case ITEM_SKELETON:
    case ITEM_ARMOR:
    case ITEM_NOTEBOARD:
    case ITEM_ORE:
    case ITEM_SWITCH:
    case ITEM_SCROLLPAPER:
    case ITEM_PEN:
    case ITEM_BEAKER:
    case ITEM_CHEMSET:
    case ITEM_BUNSEN:
    case ITEM_INKCART:
    case ITEM_POSTALPAPER:
    case ITEM_LETTER:
    case ITEM_SMOKEBOMB:
    case ITEM_SMITHYHAMMER:
    case ITEM_POISONCHEM:
    case ITEM_MOLOTOVCHEM:
    case ITEM_WICK:
    case ITEM_TEARCHEM:
    case ITEM_PIPEBOMBCHEM:
    case ITEM_CHEMBOMBCHEM:
    case ITEM_TIMER:
    case ITEM_PILEWIRE:
    case ITEM_TRIPWIRE:
    case ITEM_TOOLPACK:
    case ITEM_CHERRYCONTAINER:
    case ITEM_GUNPOWDER:
    case ITEM_SMITHYPACK:
    case ITEM_SMITHYANVIL:
    case ITEM_TECHNOSOTTER:
    case ITEM_TECHNOWORKSTATION:
    case ITEM_RANGED_WEAPON: 
    case ITEM_CLIP: 
    case ITEM_BULLET: 
    case ITEM_ARROW:
	break;

    case ITEM_GAS_CLOUD: 
     obj->timer		= pObjIndex->value[0];

    case ITEM_SCROLL:
	obj->value[0]   = obj->value[0];
	break;

    case ITEM_WAND:
    case ITEM_LENSE:
    case ITEM_STAFF:
	obj->value[0]   = obj->value[0];
	obj->value[1]	= obj->value[1];
	obj->value[2]	= obj->value[1];
	obj->value[3]   = obj->value[3];
	break;
    case ITEM_CORPSE_NPC:
    case ITEM_PORTAL:
        obj->value[0]   = obj->value[0];
        obj->value[1]   = obj->value[1];
        break;
        
    case ITEM_WEAPON:
	obj->value[1]   = number_fuzzy( number_fuzzy( 1 * level / 3 + 2 ) );
	obj->value[2]	= number_fuzzy( number_fuzzy( 2 * level / 3 + 6 ) );
	break;

    case ITEM_POTION:
    case ITEM_PILL:
	obj->value[0]   = obj->value[0];
	break;

    case ITEM_MONEY:
	obj->value[0]	= obj->cost.gold;
	obj->value[1] 	= obj->cost.silver;
	obj->value[2]	= obj->cost.copper;
	break;
    }

    obj->next		= object_list;
    object_list		= obj;
    pObjIndex->count++;

    return obj;
}


/* duplicate an object exactly -- except contents */
void clone_object(OBJ_DATA *parent, OBJ_DATA *clone)
{
    int i;

    if (parent == NULL || clone == NULL)
	return;

    /* start fixing the object */
    clone->name 	= str_dup(parent->name);
    clone->short_descr 	= str_dup(parent->short_descr);
    clone->description	= str_dup(parent->description);

    if ( parent->item_type != ITEM_LETTER )
    {
      if ( parent->extra_descr )
      {
         EXTRA_DESCR_DATA *ed;

         for( ed = parent->extra_descr; ed; ed = ed->next )
         {
	    clone->extra_descr->keyword = str_dup(parent->extra_descr->keyword);
	    clone->extra_descr->description = str_dup(parent->extra_descr->description);
	    clone->extra_descr = clone->extra_descr->next;
         }      
      }
    }

    clone->item_type	= parent->item_type;
    clone->extra_flags	= parent->extra_flags;
    clone->wear_flags	= parent->wear_flags;
    clone->weight	= parent->weight;
    clone->cost.gold	= parent->cost.gold;
    clone->cost.silver	= parent->cost.silver;
    clone->cost.copper	= parent->cost.copper;
    clone->level	= parent->level;
    clone->timer	= parent->timer;

    for (i = 0;  i < 9; i ++)
	clone->value[i]	= parent->value[i];
}


/*
 * Clear a new character.
 */
void clear_char( CHAR_DATA *ch )
{
    static CHAR_DATA ch_zero;

    *ch				= ch_zero;
    ch->name			= &str_empty[0];
    ch->oname			= &str_empty[0];
    ch->short_descr		= &str_empty[0];
    ch->long_descr		= &str_empty[0];
    ch->description		= &str_empty[0];
    ch->prompt                  = &str_empty[0];
    ch->last_note               = 0;
    ch->logon			= current_time;
    ch->p_damp			= 0;
    ch->m_damp			= 0;
    ch->position		= POS_STANDING;
    ch->hit			= 200;
    ch->perm_hit		= 200;
    ch->mod_hit			= 0;
    ch->mana			= 100;
    ch->perm_mana		= 100;
    ch->mod_mana		= 0;
    ch->move			= 100;
    ch->perm_move		= 100;
    ch->mod_move		= 0;
    ch->leader                  = NULL;
    ch->master                  = NULL;
    ch->deleted                 = FALSE;
    ch->start_align		= 'N';
    return;
}



/*
 * Free a character.
 */
void free_char( CHAR_DATA *ch )
{
    OBJ_DATA    *obj;
    OBJ_DATA    *obj_next;
    AFFECT_DATA *paf;
    CHAR_DATA   *PeT;

    if ( !IS_NPC(ch) && ch->in_room )
    {
    for ( PeT = ch->in_room->people; PeT; PeT = PeT->next_in_room )
    {
     if ( IS_NPC( PeT ) )
       if( IS_SET( PeT->act, ACT_PET ) && PeT->master == ch )
       {
	 extract_char( PeT, TRUE );
	 break;
       }
    }
    }

    for ( obj = ch->carrying; obj; obj = obj_next )
    {
        obj_next = obj->next_content;
        if ( obj->deleted )
	    continue;
	extract_obj( obj );
    }
    for ( paf = ch->affected; paf; paf = paf->next )
    {
        if ( paf->deleted )
	    continue;
	affect_remove( ch, paf );
    }
    for ( paf = ch->affected2; paf; paf = paf->next )
    {
        if ( paf->deleted )
	    continue;
	affect_remove2( ch, paf );
    }
    free_string( ch->oname              );
    free_string( ch->name               );
    free_string( ch->short_descr	);
    free_string( ch->long_descr		);
    free_string( ch->description	);
    free_string( ch->prompt             );
    if ( ch->pcdata )
    {
        ALIAS_DATA *pAl;
	ALIAS_DATA *pAl_next;

	free_string( ch->pcdata->pwd		);
	free_string( ch->pcdata->bamfin		);
	free_string( ch->pcdata->bamfout	);
	free_string( ch->pcdata->walkin		);
	free_string( ch->pcdata->walkout	);
        free_string( ch->pcdata->restusee   	);
     	free_string( ch->pcdata->restout	);
        free_string( ch->pcdata->bamfusee   	);
        free_string( ch->pcdata->transto   	);
        free_string( ch->pcdata->transfrom 	);
        free_string( ch->pcdata->transvict 	);
        free_string( ch->pcdata->slayusee  	);
        free_string( ch->pcdata->slayroom  	);
        free_string( ch->pcdata->slayvict  	);
        free_string( ch->pcdata->afkchar        );      
	free_string( ch->pcdata->title		);
	free_string( ch->pcdata->otitle		);
        free_string( ch->pcdata->empowerments	);
	free_string( ch->pcdata->detractments 	);
	free_string( ch->pcdata->plan		);
	free_string( ch->pcdata->email		);
	for ( obj = ch->pcdata->storage; obj; obj = obj_next )
	{
	  obj_next = obj->next_content;
	  if ( obj->deleted )
	    continue;
	  extract_obj( obj );
	}
	for ( pAl = ch->pcdata->alias_list; pAl; pAl = pAl_next )
	{
	  pAl_next = pAl->next;
          free_alias( pAl );
	}
	free_mem( ch->pcdata, sizeof( *ch->pcdata ) );
    }

    if ( ch->pnote )
    {
      free_string(ch->pnote->sender);
      free_string(ch->pnote->to_list);
      free_string(ch->pnote->text);
      free_string(ch->pnote->date);
      free_string(ch->pnote->subject);
      free_mem( ch->pnote, sizeof( *ch->pnote ) );
    }
    if ( ch->gspell )
      end_gspell( ch );
    free_mem( ch, sizeof( *ch ) );
    return;
}



/*
 * Get an extra description from a list.
 */
char *get_extra_descr( CHAR_DATA *ch, char *name, EXTRA_DESCR_DATA *ed )
{
    for ( ; ed; ed = ed->next )
    {
     if (!ed)
      continue;

	if ( is_name( ch, name, ed->keyword ) )
	    return ed->description;
    }
    return NULL;
}



/*
 * Translates mob virtual number to its mob index struct.
 * Hash table lookup.
 */
MOB_INDEX_DATA *get_mob_index( int vnum )
{
    MOB_INDEX_DATA *pMobIndex;

    for ( pMobIndex  = mob_index_hash[vnum % MAX_KEY_HASH];
	  pMobIndex;
	  pMobIndex  = pMobIndex->next )
    {
	if ( pMobIndex->vnum == vnum )
	    return pMobIndex;
    }

    if ( fBootDb )
    {
	bug( "Get_mob_index: bad vnum %d.", vnum );
	exit( 1 );
    }

    return NULL;
}



/*
 * Translates mob virtual number to its obj index struct.
 * Hash table lookup.
 */
OBJ_INDEX_DATA *get_obj_index( int vnum )
{
    OBJ_INDEX_DATA *pObjIndex;

    for ( pObjIndex  = obj_index_hash[vnum % MAX_KEY_HASH];
	  pObjIndex;
	  pObjIndex  = pObjIndex->next )
    {
	if ( pObjIndex->vnum == vnum )
	    return pObjIndex;
    }

    if ( fBootDb )
    {
	bug( "Get_obj_index: bad vnum %d.", vnum );
	exit( 1 );
    }

    return NULL;
}



/*
 * Translates mob virtual number to its room index struct.
 * Hash table lookup.
 */
ROOM_INDEX_DATA *get_room_index( int vnum )
{
    ROOM_INDEX_DATA *pRoomIndex;

    for ( pRoomIndex  = room_index_hash[vnum % MAX_KEY_HASH];
	  pRoomIndex;
	  pRoomIndex  = pRoomIndex->next )
    {
	if ( pRoomIndex->vnum == vnum )
	    return pRoomIndex;
    }

    if ( fBootDb )
    {
	bug( "Get_room_index: bad vnum %d.", vnum );
    }

    return NULL;
}

CLAN_DATA *get_clan_index( int vnum )
{
    CLAN_DATA *pClanIndex;

    for ( pClanIndex  = clan_first;
	  pClanIndex;
	  pClanIndex  = pClanIndex->next )
    {
	if ( pClanIndex->vnum == vnum )
	    return pClanIndex;
    }

    if ( fBootDb )
    {
	bug( "Get_clan_index: bad clan %d.", vnum );
    }

    return NULL;
}



/*
 * Read a letter from a file.
 */
char fread_letter( FILE *fp )
{
    char c;

    do
    {
	c = getc( fp );
    }
    while ( isspace( c ) );

    if ( c == EOF )
    {
      bug( "Fread_letter: EOF", 0 );
      return '\0';
    }
    return c;
}



/*
 * Read a number from a file.
 */
int fread_number( FILE *fp )
{
    char c;
    int  number;
    bool sign;

    do
    {
	c = getc( fp );
    }
    while ( isspace( c ) );

    number = 0;

    sign   = FALSE;
    if ( c == '+' )
    {
	c = getc( fp );
    }
    else if ( c == '-' )
    {
	sign = TRUE;
	c = getc( fp );
    }

    if ( c == EOF )
    {
      bug( "Fread_number: EOF", 0 );
      return 0;
    }
    if ( !isdigit( c ) )
    {
	bug( "Fread_number: bad format.", 0 );
	bug( "   If bad object, check for missing '~' in value[] fields.", 0 );
	return 0;
    }

    while ( isdigit(c) )
    {
	number = number * 10 + c - '0';
	c      = getc( fp );
    }

    if ( sign )
	number = 0 - number;

    if ( c == '|' )
	number += fread_number( fp );
    else if ( c != ' ' )
	ungetc( c, fp );

    return number;
}



/*
 * Read and allocate space for a string from a file.
 * These strings are read-only and shared.
 * Strings are hashed:
 *   each string prepended with hash pointer to prev string,
 *   hash code is simply the string length.
 * This function takes 40% to 50% of boot-up time.
 */
char *fread_string( FILE *fp )
{
    char *plast;
    char  c;

    plast = top_string + sizeof( char * );
    if ( plast > &string_space [ MAX_STRING - MAX_STRING_LENGTH ] )
    {
	bug( "Fread_string: MAX_STRING %d exceeded.", MAX_STRING );
	exit( 1 );
    }

    /*
     * Skip blanks.
     * Read first char.
     */
    do
    {
	c = getc( fp );
    }
    while ( isspace( c ) );

    if ( ( *plast++ = c ) == '~' || c == EOF )
	return &str_empty[0];

    for ( ;; )
    {
	/*
	 * Back off the char type lookup,
	 *   it was too dirty for portability.
	 *   -- Furey
	 */
	switch ( *plast = getc( fp ) )
	{
	default:
	    plast++;
	    break;

	case EOF:
	    bug( "Fread_string: EOF", 0 );
	    ungetc( '~', fp );
	    break;

	case '\n':
	    plast++;
	    *plast++ = '\r';
	    break;

	case '\r':
	    break;

	case '~':
	    plast++;
	    {
		union
		{
		    char *	pc;
		    char	rgc[sizeof( char * )];
		} u1;
		int ic;
		int iHash;
		char *pHash;
		char *pHashPrev;
		char *pString;

		plast[-1] = '\0';
		iHash     = UMIN( MAX_KEY_HASH - 1, plast - 1 - top_string );
		for ( pHash = string_hash[iHash]; pHash; pHash = pHashPrev )
		{
		    for ( ic = 0; ic < sizeof( char * ); ic++ )
			u1.rgc[ic] = pHash[ic];
		    pHashPrev = u1.pc;
		    pHash    += sizeof(char *);

		    if ( top_string[sizeof( char * )] == pHash[0]
			&& !strcmp( top_string+sizeof( char * )+1, pHash+1 ) )
			return pHash;
		}

		if ( fBootDb )
		{
		    pString             = top_string;
		    top_string		= plast;
		    u1.pc		= string_hash[iHash];
		    for ( ic = 0; ic < sizeof( char * ); ic++ )
			pString[ic] = u1.rgc[ic];
		    string_hash[iHash]  = pString;

		    nAllocString += 1;
		    sAllocString += top_string - pString;
		    return pString + sizeof( char * );
		}
		else
		{
		    return str_dup( top_string + sizeof( char * ) );
		}
	    }
	}
    }
}



/*
 * Read to end of line (for comments).
 */
void fread_to_eol( FILE *fp )
{
    char c;

    do
    {
	c = getc( fp );
    }
    while ( c != '\n' && c != '\r' && c != EOF );

    do
    {
	c = getc( fp );
    }
    while ( c == '\n' || c == '\r' );

    ungetc( c, fp );
    return;
}



/*
 * Read one word (into static buffer).
 */
char *fread_word( FILE *fp )
{
    static char  word [ MAX_INPUT_LENGTH ];
           char *pword;
           char  cEnd;

    do
    {
	cEnd = getc( fp );
    }
    while ( isspace( cEnd ) );

    if ( cEnd == '\'' || cEnd == '"' )
    {
	pword   = word;
    }
    else
    {
	word[0] = cEnd;
	pword   = word+1;
	cEnd    = ' ';
    }

    for ( ; pword < word + MAX_INPUT_LENGTH; pword++ )
    {
	*pword = getc( fp );
	if ( *pword == EOF )
	{
	  bug( "Fread_word: EOF", 0 );
	  *pword = '\0';
	  return word;
	}
	if ( cEnd == ' ' ? isspace( *pword ) : *pword == cEnd )
	{
	    if ( cEnd == ' ' )
		ungetc( *pword, fp );
	    *pword = '\0';
	    return word;
	}
    }
    bug( "Fread_word: word too long.", 0 );
    word[MAX_INPUT_LENGTH-1] = '\0';
    {
      char tc;
      
      for ( ; ; )
      {
        tc = getc(fp);
        if ( (cEnd == ' ' ? isspace( tc ) : tc == cEnd) || tc == EOF )
          break;
      }
      if ( cEnd == ' ' && tc != EOF )
        ungetc( tc, fp );
    }
    return word;

}

void *new_mem( int sMem, int isperm)
{
  void *pMem;
  static bool Killit;
  static bool Already;


  isperm=0;
  if (!( pMem = calloc(1, sMem) ) ||
      (((sAllocPerm * 10) / (1024*1024)) > 85 && !Killit))
  {
    if ( !pMem )
    {
      /* Not even enough mem to shutdown decently */
      fprintf( stderr, "New_mem: NULL warning memory reboot.\n" );
      fprintf( stderr, "Memory used: %d\n", sAllocPerm );
      exit( 1 );
    }
    if ( !Already )
    {
      char *strtime;

      strtime = ctime( &current_time );
      strtime[strlen(strtime)-1] = '\0';
      fprintf( stderr, "%s :: New_mem: Out of memory.\n", strtime );
      fprintf( stderr, "Memory used: %d\n", sAllocPerm );
    }
    if ( pMem && !Already )
    {
      char time_buf[MAX_STRING_LENGTH];
      char down_buf[MAX_STRING_LENGTH];
      time_t ntime;
      extern bool sreset;

      Killit = TRUE;
      ntime = current_time + 1;         /*First warning is 1 second from now*/
      strcpy(time_buf, ctime(&ntime));
      strcpy(down_buf, time_buf + 11);
      down_buf[8] = '\0';
      free_string(warning1);
      warning1 = str_dup(down_buf);
      ntime = ntime + 60;               /*Second warning at +61 seconds*/
      strcpy(time_buf, ctime(&ntime));
      strcpy(down_buf, time_buf + 11);
      down_buf[8] = '\0';
      free_string(warning2);
      warning2 = str_dup(down_buf);
      ntime = ntime + 60;               /*Reboot at +121 seconds*/
      strcpy(time_buf, ctime(&ntime));
      strcpy(down_buf, time_buf + 11);
      down_buf[8] = '\0';
      free_string(down_time);
      down_time = str_dup(down_buf);
      stype = 0;                       /*Reboot not shutdown*/
      sreset = FALSE;                  /*SSTime not settable now*/
    }
    else
    {
      fprintf( stderr, "NULL memory reboot.\n" );
      fprintf( stderr, "Memory used: %d\n", sAllocPerm );
      exit( 1 );                       /*No mem for anything. Straight reboot*/
    }
    Already = TRUE;
  }

  sAllocPerm += sMem;
  nAllocPerm++;
  MemAllocated += sMem;
  return pMem;
}

void dispose( void *pMem, int sMem )
{
  if ( !pMem )
  {
    bug( "Dispose: pMem is null.", 0 );
    return;
  }

  free( pMem );
  sAllocPerm -= sMem;
  nAllocPerm--;
  MemFreed += sMem;
  return;
}

/*
 * Allocate some ordinary memory,
 *   with the expectation of freeing it someday.
 */
void *alloc_mem( int sMem )
{
    return new_mem( sMem, 0 );
}

/*
 * Free some memory.
 * Recycle it back onto the free list for blocks of that size.
 */
void free_mem( void *pMem, int sMem )
{
  dispose( pMem, sMem );
  return;
}


/*
 * Allocate some permanent memory.
 * Permanent memory is never freed,
 *   pointers into it may be copied safely.
 */
void *alloc_perm( int sMem )
{ 
  return new_mem(sMem, 1 );
}

/*
 * Duplicate a string into dynamic memory.
 * Fread_strings are read-only and shared.
 */
char *str_dup( const char *str )
{
    char *str_new;

    if ( str[0] == '\0' )
	return &str_empty[0];

    if ( str >= string_space && str < top_string )
	return (char *) str;

    str_new = alloc_mem( strlen(str) + 1 );
  /*  str_new =(char*)malloc( strlen(str) + 1 ); */ 
/* this is a hack so that we can track memory right
   all the memroy stuff needs to be switched back to an
   envy 1.0 most likely have to ask Altrag
       -Decklarean
*/

/*    sAllocPerm += (strlen(str) + 1);
    nAllocPerm++;
    MemAllocated += (strlen(str) + 1);
*/
    strcpy( str_new, str );
    return str_new;
}



/*
 * Free a string.
 * Null is legal here to simplify callers.
 * Read-only shared strings are not touched.
 */
void free_string( char *pstr )
{
    if (  !pstr
	|| pstr == &str_empty[0]
	|| ( pstr >= string_space && pstr < top_string ) )
	return;

    free_mem( pstr, strlen( pstr ) + 1 );
    return;
}

/* This is Swift's area command, but I modified it a wee bit -Flux. */
void do_areas( CHAR_DATA *ch, char *argument )
{
  AREA_DATA *pArea;
  CLAN_DATA *pClan;
  char       buf[MAX_STRING_LENGTH];
  char       level[MAX_STRING_LENGTH];
  int        ilevel = 0;
  int        zlevel = 0;


  argument = one_argument( argument, level );

  if ( level[0] != '\0' && is_number( level ) )
     ilevel = atoi( level );

  send_to_char( AT_BLUE, "\n\r&YOur Paradox\n\r\n\r", ch );

  sprintf( buf, "&z< &ROwner &z> &z{ &WLevel &z}        &GBuilders&W: &CArea name\n\r" );
     send_to_char( AT_BLUE, buf, ch );

  send_to_char( AT_WHITE,
   "&P~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n\r", ch );

 while( zlevel < LEVEL_IMMORTAL )
 {
  for ( pArea = area_first; pArea; pArea = pArea->next )
  {
    if ( !IS_SET( pArea->area_flags, AREA_PROTOTYPE )
      && pArea->llevel == zlevel
      && ( ( pArea->llevel <= ilevel && pArea->ulevel >= ilevel )
          || ilevel == 0 ) )
    {
     if ( !(pClan = get_clan_index( pArea->owner )) )
     {
      if ( pArea->ulevel == 0 && pArea->llevel == -1 )
       sprintf( buf, "&z<  &R%s  &z> &z{&W  ALL  &z} &G%15s&W: &C%s\n\r",
	pClan->init, pArea->builders, pArea->name );
      else if ( pArea->ulevel == 5 && pArea->llevel == 0 )
       sprintf( buf, "&z<  &R%s  &z> &z{&WNEWBIE&z} &G%15s&W: &C%s\n\r",
	pClan->init, pArea->builders, pArea->name );
      else
       sprintf( buf, "&z<  &R%s  &z> &z{&W%3d %3d&z} &G%15s&W: &C%s\n\r",
        pClan->init, pArea->llevel, pArea->ulevel, pArea->builders, 
        pArea->name );
      send_to_char( AT_BLUE, buf, ch );
     }
     else
     {
      if ( pArea->ulevel == LEVEL_MORTAL && pArea->llevel == 0 )
       sprintf( buf, "&z<  &R%s  &z> &z{&W  ALL  &z} &G%15s&W: &C%s\n\r",
	"N/A", pArea->builders, pArea->name );
      else if ( pArea->ulevel == 5 && pArea->llevel == 0 )
       sprintf( buf, "&z<  &R%s  &z> &z{&WNEWBIE&z} &G%15s&W: &C%s\n\r",
	"N/A", pArea->builders, pArea->name );
      else
       sprintf( buf, "&z<  &R%s  &z> &z{&W%3d %3d&z} &G%15s&W: &C%s\n\r",
        "N/A", pArea->llevel, pArea->ulevel, pArea->builders, 
        pArea->name );
      send_to_char( AT_BLUE, buf, ch );
     }
    }
  }
  zlevel += 1;
 }
 return;
}

void do_memory( CHAR_DATA *ch, char *argument )
{
    char       buf [ MAX_STRING_LENGTH ];

    if ( ch->pcdata->rank >= RANK_STAFF )
    {
      sprintf( buf, "Affects   %5d\n\r", top_affect    ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "Areas     %5d\n\r", top_area      ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "ExDes     %5d\n\r", top_ed        ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "Exits     %5d\n\r", top_exit      ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "Helps     %5d\n\r", top_help      ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "Mobs      %5d(%d)\n\r", top_mob_index, mobs_in_game ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "Objs      %5d\n\r", top_obj_index ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "Resets    %5d\n\r", top_reset     ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "Rooms     %5d\n\r", top_room      ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "Shops     %5d\n\r", top_shop      ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "Artists   %5d\n\r", top_tattoo_artist ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "MobProgs  %5d\n\r", num_mob_progs ); send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "TrapProgs %5d\n\r", num_trap_progs); send_to_char(C_DEFAULT, buf, ch );

      sprintf( buf, "Strings %5d strings of %7d bytes (max %d).\n\r",
	      nAllocString, sAllocString, MAX_STRING );
      send_to_char(C_DEFAULT, buf, ch );

      sprintf( buf, "Perms   %5d blocks of %7d bytes.  Max %7d bytes.\n\r",
	      nAllocPerm, sAllocPerm, 10*1024*1024 );
      send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "%3d%% of max used.\n\r", (sAllocPerm*10) / (1024*1024));
      send_to_char(C_DEFAULT, buf, ch );

      sprintf( buf, "MemAllocated: %d MemFreed: %d Memory being used: %d\n\r", 
               MemAllocated, MemFreed, MemAllocated-MemFreed );
      send_to_char(C_DEFAULT, buf, ch );
      sprintf( buf, "%3d%% of %d.\n\r",
               (((MemAllocated-MemFreed)*10)/(1024*1024)), (10*1024*1024));
      send_to_char(C_DEFAULT, buf, ch );
      return;
    }

    sprintf( buf, "Mem used: %d bytes of %d max.\n\r",
	     sAllocPerm, 10*1024*1024 );
    sprintf( buf + strlen(buf), "%3d%% of max used.\n\r",
	     (sAllocPerm*10) / (1024*1024) );

    send_to_char(C_DEFAULT, buf, ch );
    return;
}



/*
 * Stick a little fuzz on a number.
 */
int number_fuzzy( int number )
{
    switch ( number_bits( 2 ) )
    {
    case 0:  number -= 1; break;
    case 3:  number += 1; break;
    }

    return UMAX( 1, number );
}



/*
 * Generate a random number.
 */
int number_range( int from, int to )
{
    if ( ( to = to - from + 1 ) <= 1 )
	return from;

    return from+(number_mm() % to);
}



/*
 * Generate a percentile roll.
 */
int number_percent( void )
{
    return number_mm() % 100 + 1;
}



/*
 * Generate a random door.
 */
int number_door( void )
{
    return number_mm() % 6;
}



int number_bits( int width )
{
    return number_mm( ) & ( ( 1 << width ) - 1 );
}



/*
 * I've gotten too many bad reports on OS-supplied random number generators.
 * This is the Mitchell-Moore algorithm from Knuth Volume II.
 * Best to leave the constants alone unless you've read Knuth.
 * -- Furey
 */
static	int	rgiState[2+55];

void init_mm( )
{
    int *piState;
    int  iState;

    piState	= &rgiState[2];

    piState[-2]	= 55 - 55;
    piState[-1]	= 55 - 24;

    piState[0]	= ( (int) current_time ) & ( ( 1 << 30 ) - 1 );
    piState[1]	= 1;
    for ( iState = 2; iState < 55; iState++ )
    {
	piState[iState] = ( piState[iState-1] + piState[iState-2] )
			& ( ( 1 << 30 ) - 1 );
    }
    return;
}



int number_mm( void )
{
    int *piState;
    int  iState1;
    int  iState2;
    int  iRand;

    piState		= &rgiState[2];
    iState1	 	= piState[-2];
    iState2	 	= piState[-1];
    iRand	 	= ( piState[iState1] + piState[iState2] )
			& ( ( 1 << 30 ) - 1 );
    piState[iState1]	= iRand;
    if ( ++iState1 == 55 )
	iState1 = 0;
    if ( ++iState2 == 55 )
	iState2 = 0;
    piState[-2]		= iState1;
    piState[-1]		= iState2;
    return iRand >> 6;
}



/*
 * Roll some dice.
 */
int dice( int number, int size )
{
    int idice;
    int sum;

    switch ( size )
    {
    case 0: return 0;
    case 1: return number;
    }

    for ( idice = 0, sum = 0; idice < number; idice++ )
	sum += number_range( 1, size );

    return sum;
}



/*
 * Simple linear interpolation.
 */
int interpolate( int level, int value_00, int value_47 )
{
    return value_00 + level * ( value_47 - value_00 ) / 47;
}



/*
 * Removes the tildes from a string.
 * Used for player-entered strings that go into disk files.
 */
void smash_tilde( char *str )
{
    for ( ; *str != '\0'; str++ )
    {
	if ( *str == '~' )
	    *str = '-';
    }

    return;
}



/*
 * Compare strings, case insensitive.
 * Return TRUE if different
 *   (compatibility with historical functions).
 */
bool str_cmp( const char *astr, const char *bstr )
{
    if ( !astr )
    {
	bug( "Str_cmp: null astr.", 0 );
	return TRUE;
    }

    if ( !bstr )
    {
	bug( "Str_cmp: null bstr.", 0 );
	return TRUE;
    }

    for ( ; *astr || *bstr; astr++, bstr++ )
    {
	if ( LOWER( *astr ) != LOWER( *bstr ) )
	    return TRUE;
    }

    return FALSE;
}



/*
 * Mixture of str_cmp and str_prefix.  An astrisk (*) in astr will cause
 * the equivilant of str_prefix( (astr up to, but not including *), bstr );
 * -- Alty
 */
bool str_cmp_ast( const char *astr, const char *bstr )
{
    if ( !astr )
    {
	bug( "Str_cmp: null astr.", 0 );
	return TRUE;
    }

    if ( !bstr )
    {
	bug( "Str_cmp: null bstr.", 0 );
	return TRUE;
    }

    for ( ; *astr || *bstr; astr++, bstr++ )
    {
	if ( *astr == '*' )
	    return FALSE;
	if ( LOWER( *astr ) != LOWER( *bstr ) )
	    return TRUE;
    }

    return FALSE;
}



/*
 * Compare strings, case insensitive, for prefix matching.
 * Return TRUE if astr not a prefix of bstr
 *   (compatibility with historical functions).
 */
bool str_prefix( const char *astr, const char *bstr )
{
    if ( !astr )
    {
	bug( "Str_prefix: null astr.", 0 );
	return TRUE;
    }

    if ( !bstr )
    {
	bug( "Str_prefix: null bstr.", 0 );
	return TRUE;
    }

    for ( ; *astr; astr++, bstr++ )
    {
	if ( LOWER( *astr ) != LOWER( *bstr ) )
	    return TRUE;
    }

    return FALSE;
}



/*
 * Compare strings, case insensitive, for match anywhere.
 * Returns TRUE is astr not part of bstr.
 *   (compatibility with historical functions).
 */
bool str_infix( const char *astr, const char *bstr )
{
    char c0;
    int  sstr1;
    int  sstr2;
    int  ichar;

    if ( ( c0 = LOWER( astr[0] ) ) == '\0' )
	return FALSE;

    sstr1 = strlen( astr );
    sstr2 = strlen( bstr );

    for ( ichar = 0; ichar <= sstr2 - sstr1; ichar++ )
    {
	if ( c0 == LOWER( bstr[ichar] ) && !str_prefix( astr, bstr + ichar ) )
	    return FALSE;
    }

    return TRUE;
}



/*
 * Compare strings, case insensitive, for suffix matching.
 * Return TRUE if astr not a suffix of bstr
 *   (compatibility with historical functions).
 */
bool str_suffix( const char *astr, const char *bstr )
{
    int sstr1;
    int sstr2;

    sstr1 = strlen( astr );
    sstr2 = strlen( bstr );
    if ( sstr1 <= sstr2 && !str_cmp( astr, bstr + sstr2 - sstr1 ) )
	return FALSE;
    else
	return TRUE;
}



/*
 * Returns an initial-capped string.
 */
char *capitalize( const char *str )
{
    static char strcap [ MAX_STRING_LENGTH ];
           int  i;

    for ( i = 0; str[i] != '\0'; i++ )
    {
     if ( str[i-1] == '&' )
      strcap[i] = str[i];
     else
      strcap[i] = LOWER( str[i] );
    }
    strcap[i] = '\0';
    strcap[0] = UPPER( strcap[0] );
    return strcap;
}



/*
 * Append a string to a file.
 */
void append_file( CHAR_DATA *ch, char *file, char *str )
{
    FILE *fp;

    if ( IS_NPC( ch ) || str[0] == '\0' )
	return;

    fclose( fpReserve );
    if ( !( fp = fopen( file, "a" ) ) )
    {
	perror( file );
	send_to_char(C_DEFAULT, "Could not open the file!\n\r", ch );
    }
    else
    {
	fprintf( fp, "[%5d] %s: %s\n",
		ch->in_room ? ch->in_room->vnum : 0, ch->name, str );
	fclose( fp );
    }

    fpReserve = fopen( NULL_FILE, "r" );
    return;
}



/*
 * Info channel.. -- Alty
 */
void info( const char *str, int param1, int param2, int origin )
{
  char buf[MAX_STRING_LENGTH];
  DESCRIPTOR_DATA *d;

  for ( d = descriptor_list; d; d = d->next )
  {    
    CHAR_DATA *ch = (d->original ? d->original : d->character);

   if ( d->connected != CON_PLAYING )
    continue;

   if ( ch->pcdata->rank < RANK_STAFF )
   {
    if ( origin == PORT_PAP && d->port != PORT_PAP )
     continue;

    if ( origin == PORT_ROI && d->port != PORT_ROI )
     continue;
   }

    if ( !IS_SET(ch->deaf, CHANNEL_INFO) )
    {
     if ( ch->pcdata->rank >= RANK_STAFF )
     {
      if ( origin == PORT_PAP )
       strcpy(buf, "[&wINFO&B]&w: &C");
      else
       strcpy(buf, "&O<&WInfo&O>&w: &C");  
     }
     else
     {
      if ( d->port == PORT_PAP )
       strcpy(buf, "[&wINFO&B]&w: &C");
      else
       strcpy(buf, "&O<&WInfo&O>&w: &C");  
     }
   
      sprintf(buf+16, str, param1, param2);
      send_to_char( AT_BLUE, buf, d->character );
      send_to_char( C_DEFAULT, "\n\r", d->character );
    }
  }
}

void arena_chann( const char *str, int param1, int param2 )
{
  char buf[MAX_STRING_LENGTH];
  DESCRIPTOR_DATA *d;

  strcpy(buf, "[&cARENA&W]&w: ");
  sprintf(buf+15, str, param1, param2);
  for ( d = descriptor_list; d; d = d->next )
  {
    CHAR_DATA *ch = (d->original ? d->original : d->character);

    if ( d->connected == CON_PLAYING && !IS_SET(ch->deaf, CHANNEL_ARENA) )
    {
      send_to_char( AT_WHITE, buf, d->character );
      send_to_char( C_DEFAULT, "\n\r", d->character );
    }
  }
  return;
}

void challenge( const char *str, int param1, int param2 )
{
  char buf[MAX_STRING_LENGTH];
  DESCRIPTOR_DATA *d;
  
  strcpy(buf, "[&cCHALLENGE&W]&w: ");
  sprintf(buf+19, str, param1, param2);
  for ( d = descriptor_list; d; d = d->next )
  {
    CHAR_DATA *ch = (d->original ? d->original : d->character);
    
    if ( d->connected == CON_PLAYING && !IS_SET(ch->deaf, CHANNEL_CHALLENGE) )
    {
      send_to_char( AT_WHITE, buf, d->character );
      send_to_char( C_DEFAULT, "\n\r", d->character );
    }
  }
}

/*
 * Reports a bug.
 */
void bug( const char *str, int param )
{
    FILE *fp;
    char  buf [ MAX_STRING_LENGTH ];

    if ( fpArea )
    {
	int iLine;
	int iChar;

	if ( fpArea == stdin )
	{
	    iLine = 0;
	}
	else
	{
	    iChar = ftell( fpArea );
	    fseek( fpArea, 0, 0 );
	    for ( iLine = 0; ftell( fpArea ) < iChar; iLine++ )
	    {
		while ( getc( fpArea ) != '\n' && !feof(fpArea) )
		    ;
	    }
	    fseek( fpArea, iChar, 0 );
	}

	sprintf( buf, "[*****] FILE: %s LINE: %d", strArea, iLine );
	log_string( buf, CHANNEL_NONE, -1 );

	if ( ( fp = fopen( "shutdown.txt", "a" ) ) )
	{
	    fprintf( fp, "[*****] %s\n", buf );
	    fclose( fp );
	}
    }

    strcpy( buf, "[*****] BUG: " );
    sprintf( buf + strlen( buf ), str, param );
    log_string( buf, 1 , -1 );

/*    fclose( fpReserve );
    if ( ( fp = fopen( BUG_FILE, "a" ) ) )
    {
	fprintf( fp, "%s\n", buf );
	fclose( fp );
    }
    fpReserve = fopen( NULL_FILE, "r" );*/

    return;
}

/*
 * Send logs to imms.
 * Added by Altrag.
 */
void logch( char *l_str, int l_type, int lvl )
{
	DESCRIPTOR_DATA *d;
	int level;
	char log_str[MAX_STRING_LENGTH];
	
	switch ( l_type )
	{
	default:
		strcpy( log_str, "Unknown: " );
		level = L_CON;
		if ( lvl > level )
		  level = lvl;
		break;
	case 1:
		strcpy( log_str, "Coder: " );
		level = 100000;
		break;
	case CHANNEL_LOG:
		strcpy( log_str, "Log: " );
		level = L_DIR;
		if ( lvl > level )
		  level = lvl;
		break;
	case CHANNEL_BUILD:
		strcpy( log_str, "Build: " );
		level = L_DIR;
		if ( lvl > level )
		  level = lvl;
		break;
	case CHANNEL_GOD:
		strcpy( log_str, "God: " );
		level = L_CON;
		if ( lvl > level )
		  level = lvl;
		break;
	}
	strcat( log_str, l_str );
	
	for ( d = descriptor_list; d; d = d->next )
	{
	  if ( d->connected != CON_PLAYING || IS_SET( d->character->deaf, l_type )
	       || get_trust( d->character ) < level ||
	       !IS_SET(d->character->wiznet,WIZ_OLDLOG) )
	    continue;
		send_to_char( AT_PURPLE, log_str, d->character );
		/*
		 * \n\r could have been added earlier,
		 * but need to send a C_DEFAULT line anywayz
		 * Altrag.
		 */
		send_to_char( C_DEFAULT, "\n\r", d->character );
	}
	return;
}

/*
 * Writes a string to the log.
 */
void log_string( char *str, int l_type, int level )
{
    char *strtime;

    strtime                    = ctime( &current_time );
    strtime[strlen( strtime )-1] = '\0';
    fprintf( stderr, "%s :: %s\n", strtime, str );

	/*
	 * The Actual Implementation of the Log Channels.
	 * Added by Altrag.
	 */
    if ( l_type != CHANNEL_NONE )
    	logch( str, l_type, level );
    return;
}

/*
 * This function is here to aid in debugging.
 * If the last expression in a function is another function call,
 *   gcc likes to generate a JMP instead of a CALL.
 * This is called "tail chaining."
 * It hoses the debugger call stack for that call.
 * So I make this the last call in certain critical functions,
 *   where I really need the call stack to be right for debugging!
 *
 * If you don't understand this, then LEAVE IT ALONE.
 * Don't remove any calls to tail_chain anywhere.
 *
 * -- Furey
 */
void tail_chain( void )
{
    return;
}

void area_sort( AREA_DATA *pArea )
{
  AREA_DATA *fArea;

  if ( !pArea )
  {
    bug( "area_sort: NULL pArea", 0); /* MAJOR probs if you ever see this.. */
    return;
  }

  area_last = pArea;

  if ( !area_first )
  {
    area_first = pArea;
    return;
  }

  for ( fArea = area_first; fArea; fArea = fArea->next )
  {
    if ( pArea->lvnum == fArea->lvnum ||
       ( pArea->lvnum > fArea->lvnum &&
       (!fArea->next || pArea->lvnum < fArea->next->lvnum) ) )
    {
      pArea->next = fArea->next;
      fArea->next = pArea;
      return;
    }
  }
  pArea->next = area_first;
  area_first = pArea;
  return;
}

void clan_sort( CLAN_DATA *pClan )
{
  CLAN_DATA *fClan;

  if ( !clan_first )
  {
    clan_first = pClan;
    return;
  }
  for ( fClan = clan_first; fClan; fClan = fClan->next )
  {
    if ( pClan->vnum == fClan->vnum ||
       ( pClan->vnum > fClan->vnum &&
       (!fClan->next || pClan->vnum < fClan->next->vnum) ) )
    {
      pClan->next = fClan->next;
      fClan->next = pClan;
      return;
    }
  }
  pClan->next = clan_first;
  clan_first = pClan;
  return;
}
BAN_DATA *ban_free;
BAN_DATA *ban_list;

void load_banlist( void )
{
  BAN_DATA *pban;
  FILE *fp;
  char type;
  char *banned;

  if ( !(fp=fopen( BAN_LIST, "r" )) )
    return;
    fpArea = fp;
    strcpy(strArea, BAN_LIST);

  for ( ; ; )
    {
    type = fread_letter( fp );
    if ( type == '$' )
	break;
    else if ( type != 'P'
	   && type != 'T'
	   && type != 'N' )
	{
	log_string( "Unknown ban type, ignoring entry.", -1, -1 );
        banned = fread_string( fp );
	continue;
	}
    if ( !ban_free )
	pban = alloc_perm( sizeof( *pban ) );
    else
	{
	pban	 = ban_free;
	ban_free = ban_free->next;
	}
    pban->type	= type;
    banned	= fread_string( fp );
    parse_ban( banned, pban );
    pban->next	 = ban_list;
    ban_list	 = pban;
    }
    fclose(fp);
    return;
}

void parse_ban( char *argument, BAN_DATA *banned )
{
  char user [ MAX_INPUT_LENGTH ];
  char name [ MAX_INPUT_LENGTH ];
  char *ban, *is_at;
  bool found = FALSE;
  int u, n;

  is_at = strchr( argument, '@' );
  if ( !is_at )
    {
    banned->name = strdup( argument );
    return;
    }
  u = n = 0;
  for ( ban = argument; *ban != '\0'; ban++ )
    {
    if ( *ban == '@' )
	found = TRUE;
    if ( *ban != '@' && !found )
	{
	user[u] = *ban;
	u++;
	}
    else if ( *ban != '@' && found )
	{
	name[n] = *ban;
	n++;
	}
    }
  if ( name[n] != '\0' )
	name[n] = '\0';
  if ( user[u] != '\0' )
	user[u] = '\0';
  banned->name = str_dup( name );
  banned->user = str_dup( user );
  return;
}

void load_player_list( void )
{
    FILE      *fp;
    PLAYERLIST_DATA *pPlayerList;
    char buf[MAX_STRING_LENGTH];
    struct stat   statis;

    /* Check if we have a player list */
    if ( !( fp = fopen( PLAYERLIST_FILE, "r" ) ) )
        return;

    fpArea = fp;
    strcpy(strArea, PLAYERLIST_FILE);

    /* Load the player list */
    for ( ; ; )
    {
        char*  name;

        pPlayerList             = alloc_perm( sizeof( PLAYERLIST_DATA ));
        pPlayerList->name      	= &str_empty[0];
        pPlayerList->level 	= 0;
        pPlayerList->clan_name 	= NULL; /*&str_empty[0]; */
        pPlayerList->clan_rank 	= 0;

        /* Load a character */
	for ( ; ; )
        {
            name   = fread_word( fp );

            /* Check if we are at the end of the player list */
            if ( !str_cmp( name, "#END"    ) )
            {
               fclose( fp );
               return;
	    }
            else if ( !str_cmp( name, "End" ) )
              break;
	    else if ( !str_cmp( name, "Name" ) )
              pPlayerList->name      	= fread_string( fp );
            else if ( !str_cmp( name, "Lvl" ) )
              pPlayerList->level 	= fread_number( fp );
            else if ( !str_cmp( name, "Clan" ) )       
              pPlayerList->clan_name 	= fread_string( fp );
            else if ( !str_cmp( name, "CRank" ) ) 		
              pPlayerList->clan_rank 	= fread_number( fp );
 	}

        /* check if the player still exits */
        sprintf( buf, "%s%c/%s.gz", PLAYER_DIR, LOWER(pPlayerList->name[0]),
            capitalize( pPlayerList->name ) );

        if ( stat( buf, &statis ) )
        {
           sprintf( buf, "%s%c/%s", PLAYER_DIR, LOWER(pPlayerList->name[0]),
            capitalize( pPlayerList->name ) );
           if ( stat( buf, &statis ) ) 
              continue;
        }

        if ( !playerlist )
        {
          pPlayerList->next = NULL;
          playerlist = pPlayerList;
        }
	else
        {        
          pPlayerList->next = playerlist;
          playerlist = pPlayerList;
        }
   }
   fclose(fp);
}

void save_player_list( )
{
    FILE      *fp;
    PLAYERLIST_DATA *pPlayer;


    if ( ( fp = fopen( PLAYERLIST_FILE, "w" ) ) == NULL ) 
    {
	bug( "Save_Playerlist: fopen", 0 ); 
	perror( "playerlist.lst" ); 
    }

    for ( pPlayer = playerlist;  pPlayer; pPlayer = pPlayer->next )
    {
      fprintf( fp, "Name        %s~\n",   pPlayer->name         );
      fprintf( fp, "Lvl         %d\n" ,   pPlayer->level	);
      if (pPlayer->clan_name)
      fprintf( fp, "Clan        %s~\n" ,   pPlayer->clan_name    );
      if (pPlayer->clan_rank)
      fprintf( fp, "CRank       %d\n" ,   pPlayer->clan_rank    );
      fprintf( fp, "END\n" ); 
    }
    fprintf( fp, "#END\n" ); 
    fclose( fp );
}

void delete_playerlist( char * name )
{
    PLAYERLIST_DATA * pPlayerList;
    PLAYERLIST_DATA * pPrevPlayerList;


    pPlayerList = playerlist;
    if (!str_cmp( name, playerlist->name ) )
    {
      playerlist = playerlist->next;
      free_mem( pPlayerList, sizeof( *pPlayerList ) );
      return;
    }

    for ( pPlayerList = playerlist->next, pPrevPlayerList = playerlist;
          pPlayerList;
          pPrevPlayerList = pPlayerList, pPlayerList = pPlayerList->next)
    {
      if (!str_cmp( name, pPlayerList->name ) )
      {
        pPrevPlayerList->next = pPlayerList->next;
        free_mem( pPlayerList, sizeof( *pPlayerList ) ); 
        return;
      }
    }

}

PLAYERLIST_DATA * name_in_playerlist( char *name )
{
    PLAYERLIST_DATA *pPlayer;

    for ( pPlayer = playerlist; pPlayer; pPlayer = pPlayer->next )
    {
      if (!str_cmp( name, pPlayer->name ) )
       return pPlayer;
    }
   return 0;
}

void update_playerlist( CHAR_DATA *ch )
{
    PLAYERLIST_DATA *pPlayer;

    if ( !(pPlayer = name_in_playerlist( ch->name )))
    {
      add_playerlist(ch);
      return;
    }


      pPlayer->level		= ch->level;

      free_string( pPlayer->clan_name );
      if ( ch->clan )
        pPlayer->clan_name	= str_dup( (get_clan_index(ch->clan))->name );
      else
        pPlayer->clan_name 	= NULL; /*&str_empty[0]; */
      pPlayer->clan_rank	= ch->clev;
}

void add_playerlist(CHAR_DATA *ch)
{
   PLAYERLIST_DATA *pPlayer;

   if ( (pPlayer = name_in_playerlist( ch->name )))
   {
      update_playerlist(ch);
      return;
   }

    pPlayer             = alloc_perm( sizeof( PLAYERLIST_DATA ));

   
   pPlayer->name 	= str_dup( ch->name );
   pPlayer->level	= ch->level;
   if ( ch->clan )
     pPlayer->clan_name      = str_dup((get_clan_index(ch->clan))->name );
   else 
     pPlayer->clan_name      = NULL; /*&str_empty[0]; */
   pPlayer->clan_rank = ch->clev;   

   if ( !playerlist )
   {
      pPlayer->next = NULL;
      playerlist = pPlayer;
   }
   else
   {        
      pPlayer->next = playerlist;
      playerlist = pPlayer;
   }

}

void quest_sort( QUEST_DATA *pQuest )
{
  QUEST_DATA *fQuest;

  if ( !first_quest )
  {
    first_quest = pQuest;
    return;
  }
  for ( fQuest = first_quest; fQuest; fQuest = fQuest->next )
  {
    if (    pQuest->vnum == fQuest->vnum
         || (    pQuest->vnum > fQuest->vnum
              && (    !fQuest->next
                   || pQuest->vnum < fQuest->next->vnum
                 )
            )
       )
    {
      pQuest->next = fQuest->next;
      fQuest->next = pQuest;
      return;
    }
  }
  pQuest->next = first_quest;
  first_quest = pQuest;
  return;
}

/* Decklarean */
void race_sort( RACE_DATA *pRace )
{
  RACE_DATA *fRace;

  if ( !first_race )
  {
    first_race = pRace;
    return;
  }
  for ( fRace = first_race; fRace; fRace = fRace->next )
  {
    if (    pRace->vnum == fRace->vnum
         || (    pRace->vnum > fRace->vnum
              && (    !fRace->next
                   || pRace->vnum < fRace->next->vnum
                 )
            )
       )
    {
      pRace->next = fRace->next;
      fRace->next = pRace;
      return;
    }
  }
  pRace->next = first_race;
  first_race = pRace;
  return;
}

void load_race( void )
{
    FILE      *fp;
    RACE_DATA *pRace;

    /* Check if we have a race list. */
    if ( !( fp = fopen( RACE_FILE, "r" ) ) )
        return;

    fpArea = fp;
    strcpy(strArea, RACE_FILE);

    /* Load the race list */
    for ( ; ; )
    {
        char*  name;

        pRace = new_race_data( );

        /* Load a character */
	for ( ; ; )
        {
            name   = fread_word( fp );

            /* Check if we are at the end of the race list */
            if ( !str_cmp( name, "#END"    ) )
            {
               fclose( fp );
               return;
	    }
            else if ( !str_cmp( name, "End" ) )
              break;
            else if ( !str_cmp( name, "VNum" ) )       
              pRace->vnum       = fread_number( fp );
            else if ( !str_cmp( name, "Polymorph" ) )       
              pRace->polymorph  = fread_number( fp );
            else if ( !str_cmp( name, "Claws" ) )
              pRace->claws  = fread_number( fp );       
            else if ( !str_cmp( name, "Size" ) )       
              pRace->size  = fread_number( fp );
            else if ( !str_cmp( name, "Flying" ) )       
              pRace->flying  = fread_number( fp );
            else if ( !str_cmp( name, "Gills" ) )       
              pRace->gills  = fread_number( fp );
            else if ( !str_cmp( name, "Acidblood" ) )       
              pRace->acidblood  = fread_number( fp );
            else if ( !str_cmp( name, "Infrared" ) )       
              pRace->infrared  = fread_number( fp );
            else if ( !str_cmp( name, "Truesight" ) )       
              pRace->truesight  = fread_number( fp );
            else if ( !str_cmp( name, "Swimming" ) )       
              pRace->swimming  = fread_number( fp );
            else if ( !str_cmp( name, "Race_Full" ) )
              pRace->race_full  = fread_string( fp );
            else if ( !str_cmp( name, "Race_Name" ) )
              pRace->race_name  = fread_string( fp );
            else if ( !str_cmp( name, "MStr" ) )       
              pRace->mstr       = fread_number( fp );
            else if ( !str_cmp( name, "MInt" ) )       
              pRace->mint       = fread_number( fp );
            else if ( !str_cmp( name, "MWis" ) )       
              pRace->mwis       = fread_number( fp );
            else if ( !str_cmp( name, "MDex" ) )       
              pRace->mdex       = fread_number( fp );
            else if ( !str_cmp( name, "MCon" ) )       
              pRace->mcon       = fread_number( fp );
            else if ( !str_cmp( name, "MAgi" ) )       
              pRace->magi       = fread_number( fp );
            else if ( !str_cmp( name, "MCha" ) )       
              pRace->mcha       = fread_number( fp );


            else if ( !str_cmp( name, "MIheat" ) )       
              pRace->mimm[0]       = fread_number( fp );
            else if ( !str_cmp( name, "MIpositive" ) )       
              pRace->mimm[1]       = fread_number( fp );
            else if ( !str_cmp( name, "MIcold" ) )       
              pRace->mimm[2]       = fread_number( fp );
            else if ( !str_cmp( name, "MInegative" ) )       
              pRace->mimm[3]       = fread_number( fp );
            else if ( !str_cmp( name, "MIholy" ) )       
              pRace->mimm[4]       = fread_number( fp );
            else if ( !str_cmp( name, "MIunholy" ) )       
              pRace->mimm[5]       = fread_number( fp );
            else if ( !str_cmp( name, "MIregen" ) )       
              pRace->mimm[6]       = fread_number( fp );
            else if ( !str_cmp( name, "MIdegen" ) )       
              pRace->mimm[7]       = fread_number( fp );
            else if ( !str_cmp( name, "MIdynamic" ) )       
              pRace->mimm[8]       = fread_number( fp );
            else if ( !str_cmp( name, "MIvoid" ) )       
              pRace->mimm[9]       = fread_number( fp );
            else if ( !str_cmp( name, "MIpierce" ) )       
              pRace->mimm[10]       = fread_number( fp );
            else if ( !str_cmp( name, "MIslash" ) )       
              pRace->mimm[11]       = fread_number( fp );
            else if ( !str_cmp( name, "MIscratch" ) )       
              pRace->mimm[12]       = fread_number( fp );
            else if ( !str_cmp( name, "MIbash" ) )       
              pRace->mimm[13]       = fread_number( fp );
            else if ( !str_cmp( name, "MIinternal" ) )       
              pRace->mimm[14]   = fread_number( fp );

            else if ( !str_cmp( name, "Lhuman" ) )       
              pRace->language[0]       = fread_number( fp );
            else if ( !str_cmp( name, "Lelf" ) )       
              pRace->language[1]       = fread_number( fp );
            else if ( !str_cmp( name, "Ldwarf" ) )       
              pRace->language[2]       = fread_number( fp );
            else if ( !str_cmp( name, "Lquicksilver" ) )       
              pRace->language[3]       = fread_number( fp );
            else if ( !str_cmp( name, "Lmaudlin" ) )       
              pRace->language[4]       = fread_number( fp );
            else if ( !str_cmp( name, "Lpixie" ) )       
              pRace->language[5]       = fread_number( fp );
            else if ( !str_cmp( name, "Lfelixi" ) )       
              pRace->language[6]       = fread_number( fp );
            else if ( !str_cmp( name, "Ldraconi" ) )       
              pRace->language[7]       = fread_number( fp );
            else if ( !str_cmp( name, "Lgremlin" ) )       
              pRace->language[8]       = fread_number( fp );
            else if ( !str_cmp( name, "Lcentaur" ) )       
              pRace->language[9]       = fread_number( fp );
            else if ( !str_cmp( name, "Lkender" ) )       
              pRace->language[10]       = fread_number( fp );
            else if ( !str_cmp( name, "Lminotaur" ) )       
              pRace->language[11]       = fread_number( fp );
            else if ( !str_cmp( name, "Ldrow" ) )       
              pRace->language[12]       = fread_number( fp );
            else if ( !str_cmp( name, "Laquinis" ) )       
              pRace->language[13]       = fread_number( fp );
            else if ( !str_cmp( name, "Ltroll" ) )       
              pRace->language[14]       = fread_number( fp );
 	}
        race_sort( pRace );
    }
  fclose(fp);
}

RACE_DATA *get_race_data( int vnum )
{
    RACE_DATA *pRace;

    for ( pRace  = first_race; pRace; pRace  = pRace->next )
    {
        if ( pRace->vnum == vnum )
            return pRace;
    }

    if ( fBootDb )
    {
        bug( "Get_race_data: bad race %d.", vnum );
    }

    return NULL;
}


