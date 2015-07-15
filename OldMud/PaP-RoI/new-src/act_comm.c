/***************************************************************************
 *  Original Diku Mud copyright (C) 1990, 1991 by Sebastian Hammer,        *
 *  Michael Seifert, Hans Henrik St{rfeldt, Tom Madsen, and Katja Nyboe.   *
 *                                                                         *
 *  Merc Diku Mud improvements copyright (C) 1992, 1993 by Michael         *
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

/* Auction variables */
OBJ_DATA *auc_obj;
CHAR_DATA *auc_bid;

/* Due to new money format gold/silver/copper */
  MONEY_DATA auc_cost;

int auc_count = -1;
CHAR_DATA *auc_held;

/* Auction semi-local */
void auc_channel    args( ( char *auction ) );

/*
 * Local functions.
 */
bool	is_note_to	args( ( CHAR_DATA *ch, NOTE_DATA *pnote ) );
void	note_attach	args( ( CHAR_DATA *ch ) );
void	note_remove	args( ( CHAR_DATA *ch, NOTE_DATA *pnote ) );
void	talk_channel	args( ( CHAR_DATA *ch, char *argument,
			    int channel, const char *verb ) );
char    *drunk_talk	args( ( CHAR_DATA *ch, char *argument ) );


char    *drunk_talk ( CHAR_DATA *ch, char *argument )
{
			char       *nbuf;
			char       buf       [ MAX_STRING_LENGTH ];
			static char buf2       [ 512 ];
			int        iSyl;
			int        length;

                        struct syl_type
			{
			char *old;
			char *new;
			};


    static const struct syl_type   syl_table [ ] =
    {
	{ " ", " "	},
	{ "a", "aaa"	},
	{ "b", "buh"	}, 
	{ "c", "cee"	}, 
	{ "d", "dee"	},
	{ "e", "e e"	}, 
	{ "f", "fff"	}, 
	{ "g", "gee"	}, 
	{ "h", "h"	},
	{ "i", "i"	}, 
	{ "j", "jay"	}, 
	{ "k", "k"	}, 
	{ "l", "el"	},
	{ "m", "mm"	}, 
	{ "n", "n"	}, 
	{ "o", "oooo"	}, 
	{ "p", "pee"	},
	{ "q", "q"	}, 
	{ "r", "rr"	}, 
	{ "s", "sh"	}, 
	{ "t", "tea"	},
	{ "u", "ooo"	}, 
	{ "v", "v v"	}, 
	{ "w", "wu"	}, 
	{ "x", "esh "	},
	{ "y", ""	}, 
	{ "z", "zzz"	},
	{ "", ""	}
    };

    buf[0] = '\0';
    for ( nbuf = argument ; *nbuf != '\0'; nbuf += length )
    {
	for ( iSyl = 0;
	     ( length = strlen( syl_table[iSyl].old ) ) != 0;
	     iSyl++ )
	{
	    if ( !str_prefix( syl_table[iSyl].old, nbuf ) )
	    {
		strcat( buf, syl_table[iSyl].new );
		break;
	    }
	}

	if ( length == 0 )
	    length = 1;
    }

      sprintf( buf2, "%s", buf);
      return buf2;
}


/*
 *  playerlist   -- Decklarean
 */
void delete_playerlist  args( ( char * name ) );

/* Sigh.. this is really a slow way of purging notes..
 * but efficiency isnt as useful if it doesnt work anyways.. -- Altrag 
 */
void    note_delete     args( ( NOTE_DATA *pnote ) );
/* More note stuff from the Alt man.. :).. -- Altrag */
bool    check_note_room args( ( CHAR_DATA *ch, NOTE_DATA *pnote ) );
extern EXTRA_DESCR_DATA *new_extra_descr args( (void) );
void note_delete( NOTE_DATA *pnote )
{
  NOTE_DATA *prev;

  if (pnote == note_list )
    note_list = pnote->next;
  else
  {
    for ( prev = note_list; prev; prev = prev->next )
    {
      if ( prev->next == pnote )
	break;
    }
    if ( !prev )
    {
      bug( "Note_delete: no note.", 0 );
      return;
    }
    prev->next = pnote->next;
  }
  free_string( pnote->text );
  free_string( pnote->subject );
  free_string( pnote->to_list );
  free_string( pnote->date );
  free_string( pnote->sender );
  free_mem( pnote, sizeof( *pnote ) );
}

/*
 * Get rid of old notes
 * -- Altrag
 */
void note_cleanup( void )
{
  NOTE_DATA *pnote;
  NOTE_DATA *pnote_next;
  FILE *fp;

  for ( pnote = note_list;    /* 60s*60m*24h*7d -- Altrag */
        pnote && pnote->date_stamp + 604800 < current_time;
        pnote = pnote_next )
  {
    pnote_next = pnote->next;
    if ( pnote->protected )
      continue;
    note_delete( pnote );
  }
  fclose( fpReserve );
  if ( !( fp = fopen( NOTE_FILE, "w" ) ) )
  {
    perror( NOTE_FILE );
  }
  else
  {
    for ( pnote = note_list; pnote; pnote = pnote->next )
    {
      fprintf( fp, "Sender  %s~\n", pnote->sender );
      fprintf( fp, "Date    %s~\n", pnote->date );
      fprintf( fp, "Stamp   %ld\n", pnote->date_stamp );
      fprintf( fp, "To      %s~\n", pnote->to_list );
      fprintf( fp, "Subject %s~\n", pnote->subject );
      fprintf( fp, "Protect %d\n",  pnote->protected );
      fprintf( fp, "Board   %d\n",  pnote->on_board );
      fprintf( fp, "Text\n%s~\n\n", pnote->text );
    }
    fclose( fp );
  }
  fpReserve = fopen( NULL_FILE, "r" );
  return;
}
    
bool check_note_room( CHAR_DATA *ch, NOTE_DATA *pnote )
{
  OBJ_DATA *pObj;

  if ( !ch->in_room )
    return (pnote->on_board == 0);

  for ( pObj = ch->in_room->contents; pObj; pObj = pObj->next_content )
    if ( pObj->item_type == ITEM_NOTEBOARD )
      break;

  if ( !pObj )
    return (pnote->on_board == 0);

  if ( pnote->on_board != pObj->pIndexData->vnum )
    return FALSE;

  if ( pObj->value[1] > get_trust(ch) )
  {
    OBJ_DATA *decoder;

    for ( decoder = ch->carrying; decoder; decoder = decoder->next_content )
      if ( decoder->pIndexData->vnum == pObj->value[0] )
	break;

    if ( decoder == NULL )
      return FALSE;
  }

  return TRUE;
}

bool is_note_to( CHAR_DATA *ch, NOTE_DATA *pnote )
{
    CLAN_DATA  *pClan; 

    if ( !check_note_room( ch, pnote ) )
        return FALSE;

    if ( !str_cmp( ch->name, pnote->sender ) )
	return TRUE;

    for ( pClan = clan_first->next; pClan; pClan = pClan->next )
    {
        if ( ( ch->clan == pClan->vnum ) && 
             ( !str_cmp( pnote->to_list, strip_color( pClan->name ) ) ) )
        {
           return TRUE;  /* search for clan name in arg1 */
           break;
        }
    }

    if ( is_name( NULL, "all", pnote->to_list ) )
	return TRUE;

    if ( ( get_trust( ch ) >= LEVEL_IMMORTAL ) 
		     && ( (   is_name(NULL, "immortal", pnote->to_list )
			   || is_name(NULL, "immortals", pnote->to_list )
			   || is_name(NULL, "imm",       pnote->to_list )
			   || is_name(NULL, "immort",    pnote->to_list ) ) ) )
	
	return TRUE;
    
    if ( ( get_trust( ch ) > L_CON ) &&
    			    is_name(NULL, "council", pnote->to_list ) )
    	return TRUE;

    if ( is_name(NULL, "IMP", pnote->to_list ) &&
         ( ch->pcdata->rank == RANK_BOSS ) )
       return TRUE;
    if ( is_name(NULL, ch->name, pnote->to_list ) )
	return TRUE;

    return FALSE;
}



void note_attach( CHAR_DATA *ch )
{
    NOTE_DATA *pnote;

    if ( ch->pnote )
	return;

    if ( !note_free )
    {
	pnote	  = alloc_perm( sizeof( *ch->pnote ) );
    }
    else
    {
	pnote	  = note_free;
	note_free = note_free->next;
    }

    pnote->next		= NULL;
    pnote->sender	= str_dup( ch->name );
    pnote->date		= str_dup( "" );
    pnote->to_list	= str_dup( "" );
    pnote->subject	= str_dup( "" );
    pnote->text		= str_dup( "" );
    pnote->protected    = FALSE;
    pnote->on_board     = 0;
    ch->pnote		= pnote;
    return;
}



void note_remove( CHAR_DATA *ch, NOTE_DATA *pnote )
{
    FILE      *fp;
    NOTE_DATA *prev;
    char      *to_list;
    char       to_new [ MAX_INPUT_LENGTH ];
    char       to_one [ MAX_INPUT_LENGTH ];

    /*
     * Build a new to_list.
     * Strip out this recipient.
     */
    to_new[0]	= '\0';
    to_list	= pnote->to_list;
    while ( *to_list != '\0' )
    {
	to_list	= one_argument( to_list, to_one );
	if ( to_one[0] != '\0' && str_cmp( ch->name, to_one ) )
	{
	    strcat( to_new, " "    );
	    strcat( to_new, to_one );
	}
    }

    /*
     * Just a simple recipient removal?
     */
    if ( str_cmp( ch->name, pnote->sender ) && to_new[0] != '\0' && 
	 get_trust(ch) < L_DEM )
    {
	free_string( pnote->to_list );
	pnote->to_list = str_dup( to_new + 1 );
	return;
    }

    /*
     * Remove note from linked list.
     */
    if ( pnote == note_list )
    {
	note_list = pnote->next;
    }
    else
    {
	for ( prev = note_list; prev; prev = prev->next )
	{
	    if ( prev->next == pnote )
		break;
	}

	if ( !prev )
	{
	    bug( "Note_remove: pnote not found.", 0 );
	    return;
	}

	prev->next = pnote->next;
    }

    free_string( pnote->text    );
    free_string( pnote->subject );
    free_string( pnote->to_list );
    free_string( pnote->date    );
    free_string( pnote->sender  );
    free_mem( pnote, sizeof( *pnote ) );

    /*
     * Rewrite entire list.
     */
    fclose( fpReserve );
    if ( !( fp = fopen( NOTE_FILE, "w" ) ) )
    {
	perror( NOTE_FILE );
    }
    else
    {
	for ( pnote = note_list; pnote; pnote = pnote->next )
	{
	    fprintf( fp, "Sender  %s~\n", pnote->sender     );
	    fprintf( fp, "Date    %s~\n", pnote->date       );
	    fprintf( fp, "Stamp   %ld\n", pnote->date_stamp );
	    fprintf( fp, "To      %s~\n", pnote->to_list    );
	    fprintf( fp, "Subject %s~\n", pnote->subject    );
	    fprintf( fp, "Protect %d\n",  pnote->protected  );
	    fprintf( fp, "Board   %d\n",  pnote->on_board   );
	    fprintf( fp, "Text\n%s~\n\n", pnote->text       );
	}
	fclose( fp );
    }
    fpReserve = fopen( NULL_FILE, "r" );
    return;
}

/* Date stamp idea comes from Alander of ROM */
void do_note( CHAR_DATA *ch, char *argument )
{
    NOTE_DATA *pnote;
    CHAR_DATA *to_ch;
    char       buf  [ MAX_STRING_LENGTH   ];
    char       buf1 [ MAX_STRING_LENGTH*7 ];
    char       arg  [ MAX_INPUT_LENGTH    ];
    int        vnum;
    int        anum;

    if ( IS_NPC( ch ) )
	return;

    argument = one_argument( argument, arg );
    smash_tilde( argument );

    if ( arg[0] == '\0' )
    {
	  do_note( ch, "read" );
	return;
    }
/* POSTMAN THINGY */
    if ( !str_cmp( arg, "take" ) )
      {
      CHAR_DATA *postman;
      OBJ_DATA *paper;
      OBJ_DATA *quill;
      OBJ_DATA *letter;
      EXTRA_DESCR_DATA *note;
      char buf[ MAX_INPUT_LENGTH ];


      if ( !is_number( argument ) )
	{
	send_to_char(AT_WHITE, "Take down which note?\n\r", ch );
	return;
	}
      for ( postman = char_list; postman; postman = postman->next )
	{
	if ( IS_NPC( postman ) && IS_SET( postman->act, ACT_POSTMAN ) )
	  break;
	}
      if ( !postman )
	{
	send_to_char( AT_WHITE, "You need a postman to take notes.\n\r", ch );
	return;
	}
      for ( paper = ch->carrying; paper; paper = paper->next_content )
	{
	if ( paper->item_type == ITEM_POSTALPAPER )
		break;
	}
      if ( !paper )
	{
	send_to_char( AT_WHITE, "You need some postal paper to take the note on.\n\r", ch );
	return;
	}
      for ( quill = ch->carrying; quill; quill = quill->next )
	{
	if ( quill->item_type == ITEM_PEN )
	  break;
	}
      if ( !quill )
	{
	send_to_char( AT_WHITE, "You need a quill to write the note down.\n\r", ch );
	return;
	}
      buf1[0] = '\0';
      vnum = 0;
      anum = atoi( argument );
      for ( pnote = note_list; pnote; pnote = pnote->next )
	{
	if ( is_note_to( ch, pnote ) && vnum++ == anum )
	  {
	  sprintf( buf, "&W%s&c: &w%s\n\r&c%s&w\n\r",
		   pnote->sender,
		   pnote->subject,
		   pnote->date );
	  strcat( buf1, buf );
	  strcat( buf1, pnote->text );
	  letter = create_object( get_obj_index(1), 0 );
	  letter->item_type = ITEM_LETTER;
	  sprintf( buf, "letter" );
	  free_string( letter->name );
	  letter->name = str_dup(buf);
	  sprintf( buf, letter->short_descr, pnote->sender );
	  free_string( letter->short_descr );
	  letter->short_descr = str_dup(buf);
	  sprintf( buf, letter->description, pnote->sender );
	  free_string( letter->description );
	  letter->description = str_dup(buf);
	  note			= new_extra_descr();
	  note->keyword 	= str_dup( "note" ); 
	  note->next		= letter->extra_descr;  
	  note->description	= str_dup( buf1 );
	  letter->extra_descr	= note;
	  obj_to_char( letter, ch );
	  paper->value[0]--;
	  if ( paper->value[0] == 0 )
	    {
	    send_to_char(AT_WHITE, "You have used the last of your postal paper.\n\r", ch );
	    extract_obj( paper );
	    }
	  if ( !str_cmp( pnote->to_list, ch->name ) )
	    {
	    sprintf( buf, "remove %d", anum );
	    do_note( ch, buf );
	    }
	  send_to_char(AT_WHITE, "Note Taken.\n\r", ch );
	  return;
	  }
	}
	send_to_char(AT_WHITE, "No such note.\n\r", ch );
	return;
      }
    if ( !str_cmp( arg, "test" ) )
    {
      time_t time_n;
      char fst[MAX_STRING_LENGTH];
      char sst[MAX_STRING_LENGTH];

      time_n = current_time + (60*60*24*180);
      act(AT_BLUE, "$t", ch, ctime(&time_n), NULL, TO_CHAR);
      strcpy(fst, ctime(&time_n));
      strcpy(sst, fst + 11);
      sst[8] = '\0';
      send_to_char(AT_BLUE,sst,ch);
      sprintf(fst, "\n\r%d %d %d", ("aA" < "aB"), ("Aa" > "aB"), ("A" < "b"));
      send_to_char(AT_BLUE,fst,ch);
      return;
    }

    if ( !str_cmp( arg, "list" ) )
    {
        char arg1[MAX_STRING_LENGTH];
        char arg2[MAX_STRING_LENGTH];
        int fn = 0;
        int ln = 0;
        
        vnum = 0;
        buf1[0] = '\0';

        if ( argument[0] != '\0' )
        {
          argument = one_argument( argument, arg1 );
          argument = one_argument( argument, arg2 );
          fn = is_number( arg1 ) ? atoi( arg1 ) : 0;
          ln = is_number( arg2 ) ? atoi( arg2 ) : 0;
          
          if ( ( fn == 0 && ln == 0 ) || ( fn < 1 ) || ( ln < 0 )
            || ( ln < fn ) )
          {
            send_to_char( AT_DGREEN, "Invalid note range.\n\r", ch );
            return;
          }
        }
	
	for ( pnote = note_list; pnote; pnote = pnote->next )
	{
	    if ( ( is_note_to( ch, pnote ) && vnum >= fn && vnum <= ln )
	     || ( fn == 0 && ln == 0 && is_note_to( ch, pnote ) ) )
	    {
		sprintf( buf, "&G[%3d%s%s] %s: %s\n\r",
			vnum,
			( pnote->date_stamp > ch->last_note
			 && str_cmp( pnote->sender, ch->name ) ) ? "N" : " ",
			(get_trust(ch) > L_CON ) ?
			 pnote->protected ? "P" : " " : "",
			pnote->sender, pnote->subject );
		strcat( buf1, buf );
	    }
	    
	    if ( is_note_to( ch, pnote ) )
	      vnum++;

	}
	send_to_char(AT_GREEN, buf1, ch );
	return;
    }

    if ( !str_cmp( arg, "read" ) )
    {
	bool fAll;

	if ( IS_NPC(ch) )
	  return;

	else if ( argument[0] == '\0' || !str_prefix( argument, "next" ) )
	  /* read next unread note */
	{
	    vnum    = 0;
	    buf1[0] = '\0';
	    for ( pnote = note_list; pnote; pnote = pnote->next )
	    {
		if ( is_note_to( ch, pnote )
		    && str_cmp( ch->name, pnote->sender )
		    && ch->last_note < pnote->date_stamp )
		{
		    sprintf( buf, "[%3d] %s: %s\n\r&G%s\n\rTo: %s\n\r",
			    vnum,
			    pnote->sender,
			    pnote->subject,
			    pnote->date,
			    pnote->to_list );
		    strcat( buf1, buf );
		    strcat( buf1, pnote->text );
		    ch->last_note = UMAX( ch->last_note, pnote->date_stamp );
		    send_to_char(AT_GREEN, buf1, ch );
		    return;
		}
		else
		  if ( is_note_to( ch, pnote ) )
		    vnum++;
	    }
	    send_to_char(AT_DGREEN, "You have no unread notes.\n\r", ch );
	    return;
	}
	else if ( is_number( argument ) )
	{
	    fAll = FALSE;
	    anum = atoi( argument );
	}
	else
	{
	    send_to_char(AT_DGREEN, "Note read which number?\n\r", ch );
	    return;
	}

	vnum    = 0;
	buf1[0] = '\0';
	for ( pnote = note_list; pnote; pnote = pnote->next )
	{
	    if ( is_note_to( ch, pnote ) && ( vnum++ == anum || fAll ) )
	    {
		sprintf( buf, "[%3d] %s: %s\n\r&G%s\n\rTo: %s\n\r",
			vnum - 1,
			pnote->sender,
			pnote->subject,
			pnote->date,
			pnote->to_list );
		strcat( buf1, buf );
		strcat( buf1, pnote->text );
		if ( !fAll )
		    send_to_char(AT_GREEN, buf1, ch );
		else
		    strcat( buf1, "\n\r" );
		ch->last_note = UMAX( ch->last_note, pnote->date_stamp );
		if ( !fAll )
		    return;
	    }
	}

	if ( !fAll )
	    send_to_char(AT_DGREEN, "No such note.\n\r", ch );
	else
	    send_to_char(AT_GREEN, buf1, ch );
	return;
    }

    if ( !str_cmp( arg, "+" ) )
    {
	note_attach( ch );
	strcpy( buf, ch->pnote->text );
	if ( strlen( buf ) + strlen( argument ) >= MAX_STRING_LENGTH - 100 )
	{
	    send_to_char(AT_DGREEN, "Note too long.\n\r", ch );
	    return;
	}

	strcat( buf, argument );
	strcat( buf, "\n\r"   );
	free_string( ch->pnote->text );
	ch->pnote->text = str_dup( buf );
	send_to_char(AT_WHITE, "Ok.\n\r", ch );
	return;
    }

    if ( !str_cmp( arg, "write" ) )
    {
      if ( IS_NPC(ch) )
	return;
      note_attach( ch );
      string_append( ch, &ch->pnote->text );
      return;
    }

    if ( !str_cmp( arg, "subject" ) )
    {
	note_attach( ch );
	free_string( ch->pnote->subject );
	ch->pnote->subject = str_dup( argument );
	send_to_char(AT_WHITE, "Ok.\n\r", ch );
	return;
    }

    if ( !str_cmp( arg, "namechange" ) && ch->pcdata->rank > RANK_STAFF )
    {
     note_attach( ch );
     free_string( ch->pnote->sender );
     ch->pnote->sender = str_dup( argument );
     send_to_char( AT_WHITE, "ok.\n\r", ch );
     return;
    }

    if ( !str_cmp( arg, "to" ) )
    {
	note_attach( ch );
	free_string( ch->pnote->to_list );
	ch->pnote->to_list = str_dup( argument );
	send_to_char(AT_WHITE, "Ok.\n\r", ch );
	return;
    }

    if ( !str_cmp( arg, "clear" ) )
    {
	if ( ch->pnote )
	{
	    free_string( ch->pnote->text    );
	    free_string( ch->pnote->subject );
	    free_string( ch->pnote->to_list );
	    free_string( ch->pnote->date    );
	    free_string( ch->pnote->sender  );
	    free_mem( ch->pnote, sizeof( *ch->pnote ) );
	    ch->pnote		= NULL;
	}

	send_to_char(AT_WHITE, "Ok.\n\r", ch );
	return;
    }

    if ( !str_cmp( arg, "show" ) )
    {
        if ( IS_NPC(ch))
	  return;

	if ( !ch->pnote )
	{
	    send_to_char(AT_DGREEN, "You have no note in progress.\n\r", ch );
	    return;
	}

	sprintf( buf, "%s: %s\n\r&GTo: %s\n\r",
		ch->pnote->sender,
		ch->pnote->subject,
		ch->pnote->to_list );
	send_to_char(AT_GREEN, buf, ch );
	send_to_char(AT_GREEN, ch->pnote->text, ch );
	return;
    }

    if ( !str_cmp( arg, "post" ) || !str_prefix( arg, "send" ) )
    {
	FILE *fp;
	char *strtime;
	OBJ_DATA *board;

	if ( !ch->pnote )
	{
	    send_to_char(AT_DGREEN, "You have no note in progress.\n\r", ch );
	    return;
	}

	if ( !str_cmp( ch->pnote->to_list, "" ) )
	{
	    send_to_char(AT_DGREEN,
	      "You need to provide a recipient (name, all, or immortal).\n\r",
			 ch );
	    return;
	}

	if ( !str_cmp( ch->pnote->subject, "" ) )
	{
	    send_to_char(AT_DGREEN, "You need to provide a subject.\n\r", ch );
	    return;
	}

	ch->pnote->on_board = 0;
	if ( ch->in_room )
	{
	  for ( board = ch->in_room->contents; board;
	        board = board->next_content )
	    if ( board->item_type == ITEM_NOTEBOARD )
	      break;
	  
	  if ( board )
	  if ( board->value[2] > get_trust(ch) )
	  {
	    OBJ_DATA *decoder;

	    for ( decoder = ch->carrying; decoder;
		  decoder = decoder->next_content )
	      if ( decoder->pIndexData->vnum == board->value[0] )
		break;
	    if ( decoder == NULL )
	    {
	      send_to_char( AT_WHITE, "You may not post on this board.\n\r",ch);
	      return;
	    }
	  }
	  if ( board )
	    ch->pnote->on_board = board->pIndexData->vnum;
	}

	if ( IS_NPC(ch) && ch->pnote->on_board == 0 )
	  return;

	ch->pnote->next			= NULL;
	strtime				= ctime( &current_time );
	strtime[strlen(strtime)-1]	= '\0';
	free_string( ch->pnote->date );
	ch->pnote->date			= str_dup( strtime );
	ch->pnote->date_stamp           = current_time;

	if ( !note_list )
	{
	    note_list	= ch->pnote;
	}
	else
	{
	    for ( pnote = note_list; pnote->next; pnote = pnote->next )
		;
	    pnote->next	= ch->pnote;
	}
	pnote		= ch->pnote;
	ch->pnote       = NULL;

	fclose( fpReserve );
	if ( !( fp = fopen( NOTE_FILE, "a" ) ) )
	{
	    perror( NOTE_FILE );
	}
	else
	{
	    fprintf( fp, "Sender  %s~\n", pnote->sender     );
	    fprintf( fp, "Date    %s~\n", pnote->date       );
	    fprintf( fp, "Stamp   %ld\n", pnote->date_stamp );
	    fprintf( fp, "To      %s~\n", pnote->to_list    );
	    fprintf( fp, "Subject %s~\n", pnote->subject    );
	    fprintf( fp, "Protect %d\n",  pnote->protected  );
	    fprintf( fp, "Board   %d\n",  pnote->on_board   );
	    fprintf( fp, "Text\n%s~\n\n", pnote->text       );
	    fclose( fp );
	}
	fpReserve = fopen( NULL_FILE, "r" );

	send_to_char(AT_WHITE, "Ok.\n\r", ch );

	for ( to_ch = char_list; to_ch; to_ch = to_ch->next )
       	{
          if ( !to_ch->in_room || to_ch->deleted )
            continue;
	  if ( is_note_to( to_ch, pnote ) && to_ch != ch )
	    send_to_char( C_DEFAULT, "New note.\n\r", to_ch );	    
	}

	return;
    }

    if ( !str_cmp( arg, "remove" ) )
    {
        if ( IS_NPC(ch) )
	  return;

	if ( !is_number( argument ) )
	{
	    send_to_char(AT_DGREEN, "Note remove which number?\n\r", ch );
	    return;
	}

	anum = atoi( argument );
	vnum = 0;
	for ( pnote = note_list; pnote; pnote = pnote->next )
	{
	    if ( is_note_to( ch, pnote ) && vnum++ == anum )
	    {
		note_remove( ch, pnote );
		send_to_char(AT_WHITE, "Ok.\n\r", ch );
		return;
	    }
	}

	send_to_char(AT_DGREEN, "No such note.\n\r", ch );
	return;
    }

    /*
     * "Permanent" note flag.
     * -- Altrag
     */
    if ( !str_cmp( arg, "protect" ) )
    {
      if ( IS_NPC(ch) )
	return;

      if ( get_trust( ch ) < L_CON )
      {
	send_to_char( AT_DGREEN, "Huh?  Type 'help note' for usage.\n\r", ch );
	return;
      }
      if ( argument[0] == '\0' || !is_number( argument ) )
      {
	send_to_char( AT_DGREEN, "Syntax:  note protect <#>\n\r", ch );
	return;
      }
      anum = atoi( argument );
      vnum = 0;
      for ( pnote = note_list; pnote; pnote = pnote->next )
      {
	if ( is_note_to( ch, pnote ) && vnum++ == anum )
	{
	  if ( pnote->protected )
	    pnote->protected = FALSE;
	  else
	    pnote->protected = TRUE;
	  note_cleanup ();
	  send_to_char( AT_WHITE, "Ok.\n\r", ch );
	  return;
	}
      }
      send_to_char( AT_WHITE, "No such note.\n\r", ch );
      return;
    }

    send_to_char(AT_DGREEN, "Huh?  Type 'help note' for usage.\n\r", ch );
    return;
}



/*
 * Generic channel function.
 */
void talk_channel( CHAR_DATA *ch, char *argument, int channel, const char *verb )
{
    DESCRIPTOR_DATA *d;
    char             buf [ MAX_STRING_LENGTH ];
    int              position;

    if ( argument[0] == '\0' )
    {
	sprintf( buf, "%s what?\n\r", verb );
	buf[0] = UPPER( buf[0] );
	return;
    }

    if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_SILENCE ) )
    {
	sprintf( buf, "You can't %s.\n\r", verb );
	send_to_char(AT_WHITE, buf, ch );
	return;
    }
    
    if ( IS_SET( ch->in_room->room_flags, ROOM_SILENT ) 
         && (get_trust(ch)<L_DIR) )
    {
        send_to_char(AT_WHITE, "You can't do that here.\n\r", ch );
        return;
    }

    REMOVE_BIT( ch->deaf, channel );

    switch ( channel )
    {
    default:
	sprintf( buf, "You %s '%s'\n\r", verb, argument );
	send_to_char(AT_LBLUE, buf, ch );
	sprintf( buf, "$n&X %ss '$t'", verb );
	break;
    case CHANNEL_IMMTALK:
	sprintf( buf, "$n&X: $t");
	position	= ch->position;
	ch->position	= POS_STANDING;
	act(AT_YELLOW, buf, ch, argument, NULL, TO_CHAR );
	ch->position	= position;
	break;
    case CHANNEL_CLAN:
        sprintf( buf, "<%s&R> $n&X: '$t'",
	       ( get_clan_index(ch->clan) && (get_clan_index(ch->clan))->name ?
		(get_clan_index(ch->clan))->name : "Unclanned") );
        position        = ch->position;
        ch->position   = POS_STANDING;
        act(AT_RED, buf, ch, argument, NULL, TO_CHAR );
        ch->position    = position;
        break;
    case CHANNEL_OOC:
	sprintf( buf, "OOC - $n&X: '$t'" );
	position        = ch->position;
	ch->position    = POS_STANDING;
	act(AT_PINK, buf, ch, argument, NULL, TO_CHAR );
	ch->position    = position;
	break;
    case CHANNEL_GRATZ:
        sprintf( buf, "&RG&PR&BA&CT&GZ&W -&Y $n&X: '$t'" );
        position        = ch->position;
        ch->position    = POS_STANDING;
        act(AT_BLUE, buf, ch, argument, NULL, TO_CHAR );
        ch->position    = position;
        break; 
    }

    for ( d = descriptor_list; d; d = d->next )
    {
	CHAR_DATA *och;
	CHAR_DATA *vch;

	och = d->original ? d->original : d->character;
	vch = d->character;

	if ( d->connected == CON_PLAYING
	    && vch != ch
	    && !IS_SET( och->deaf, channel ) 
	    && !IS_SET( och->in_room->room_flags, ROOM_SILENT ) )
	{
	    if ( channel == CHANNEL_IMMTALK && !IS_IMMORTAL( och ) )
		continue;
            if ( ( channel == CHANNEL_CLAN )
                && ( vch->clan != ch->clan ) )
                {
                  if ( IS_SET( och->deaf, CHANNEL_CLAN_MASTER ) ||
                      get_trust( och ) < L_IMP )
                    continue;
                }
	    if ( channel == CHANNEL_YELL
		&& vch->in_room->area != ch->in_room->area )
	        continue;


	    position		= vch->position;
	    if ( channel != CHANNEL_SHOUT && channel != CHANNEL_YELL )
		vch->position	= POS_STANDING;
     
            switch ( channel )
            {
             default:
	             act(AT_LBLUE, buf, ch, argument, vch, TO_VICT ); break;
             case CHANNEL_IMMTALK:
	             act(AT_YELLOW, buf, ch, argument, vch, TO_VICT ); break;
             case CHANNEL_CLAN:
                     act(AT_RED, buf, ch, argument, vch, TO_VICT ); break;
	     case CHANNEL_OOC:
		     act(AT_PINK, buf, ch, argument, vch, TO_VICT );
		     break;
             case CHANNEL_GRATZ:
             sprintf( buf, "&RG&PR&BA&CT&GZ&W -&Y $n: '$t'" );
                     act(AT_YELLOW, buf, ch, argument, vch, TO_VICT );
                     break; 
            }
	    vch->position	= position;
	}
    }

    return;
}

void auc_channel ( char *auction )
{
  char buf[MAX_STRING_LENGTH];
  DESCRIPTOR_DATA *d;

  sprintf( buf, "&GAUCTION: &g%s\n\r", auction );

  for ( d = descriptor_list; d; d = d->next )
  {
    if ( d->connected != CON_PLAYING )
      continue;

    if ( !IS_SET( (d->original ? d->original : d->character)->deaf, CHANNEL_AUCTION ) )
      write_to_buffer( d, buf, 0 );
  }

  return;
}

void do_clan( CHAR_DATA *ch, char *argument )
{
  if(ch->clan == 0)
  {
    send_to_char(AT_BLUE, "You are not clanned.\n\r", ch);
    return;
  }
    talk_channel( ch, argument, CHANNEL_CLAN, "clantalk" );
    return;
}

void do_auction( CHAR_DATA *ch, char *argument )
{
    char arg[MAX_STRING_LENGTH];
    char arg1[MAX_STRING_LENGTH];
    MONEY_DATA aucamt;
    char arg3 [ MAX_STRING_LENGTH ];
    
    argument = one_argument( argument, arg );
    argument = one_argument( argument, arg1 );
/* Synax: auction boots 5 copper,gold, or silver */
  
    aucamt.gold = aucamt.silver = aucamt.copper = 0;
    argument = one_argument( argument, arg3 );
    if ( !str_cmp( arg3, "gold" ) )
      aucamt.gold = is_number( arg1 ) ? atoi( arg1 ) : 0;
    else if ( !str_cmp( arg3, "silver" ) )
      aucamt.silver = is_number( arg1 ) ? atoi( arg1 ) : 0;
    else if ( !str_cmp( arg3, "copper" ) )
      aucamt.copper = is_number( arg1 ) ? atoi( arg1 ) : 0;
    else
    {
      send_to_char( AT_WHITE, "&WSyntax: &Rauction <item> <amount> <currency type>\n\r", ch );
      return;
    }

    if ( IS_NPC( ch ) )
    {
      send_to_char( AT_WHITE, "You can't auction items.\n\r", ch );
      return;
    }
    
    if ( arg[0] == '\0' )
    {
      send_to_char( AT_WHITE, "Auction which item?\n\r", ch );
      return;
    }

    if ( !str_cmp( arg, "remove" ) )
    {
      if ( !auc_obj || !auc_held || auc_held != ch )
      {
	send_to_char(AT_WHITE, "You do not have an item being auctioned.\n\r",ch);
	return;
      }
      if ( auc_bid )
      {
	send_to_char(AT_WHITE, "You may not remove your item after a bid has been made.\n\r", ch );
	return;
      }
      REMOVE_BIT(ch->deaf, CHANNEL_AUCTION);
      sprintf(log_buf, "%s has been removed from the auction.\n\r", auc_obj->short_descr );
      auc_channel( log_buf );
      act( AT_DGREEN, "$p appears suddenly in your hands.", ch, auc_obj, NULL, TO_CHAR );
      act( AT_DGREEN, "$p appears suddenly in the hands of $n.", ch, auc_obj, NULL,
        TO_ROOM );
      obj_to_char( auc_obj, ch );
      auc_obj = NULL;
      auc_held = NULL;

      auc_cost.gold = auc_cost.silver = auc_cost.copper = 0;

      auc_count = -1;
      return;
    }

      if ( ( aucamt.gold <= 0 ) && ( aucamt.silver <= 0 ) &&
	   ( aucamt.copper <= 0 ) )
    {
      send_to_char(AT_WHITE, "Auction it for how much?\n\r",ch );
      return;
    }    
/* Lowest bidding price to start an auction */
    if ( ( aucamt.gold < 100 ) && ( aucamt.silver < 100 )
       && ( aucamt.copper < 50 ) )
    {
      send_to_char( AT_WHITE, "That is too low of a starting bidding price.\n\r",
		   ch );
      return;
    }


    if ( auc_obj )
    {
      send_to_char( AT_WHITE, "There is already an item being auctioned.\n\r", ch );
      return;
    }
   else
    {
      if ( ( auc_obj = get_obj_carry( ch, arg ) ) )
      {
        if ( (auc_obj->pIndexData->vnum > 1 && auc_obj->pIndexData->vnum < 23 ) 
            || ( auc_obj->item_type == ITEM_CONTAINER && auc_obj->contains ) 
            || auc_obj->timer != -1 )
        { 
          send_to_char( AT_DGREEN, "You can't auction that.\n\r", ch );
          auc_obj = NULL;
          return;
        }
        
	REMOVE_BIT( ch->deaf, CHANNEL_AUCTION );
	act( AT_DGREEN, "$p disappears from your inventory.", ch, auc_obj, NULL,
	 TO_CHAR );
	act( AT_DGREEN, "$p disappears from the inventory of $n.", ch, auc_obj,
	 NULL, TO_ROOM );
	obj_from_char( auc_obj );
	auc_held = ch;
	auc_bid = NULL;

/* Only one currency will be above zero */
	auc_cost.gold   = aucamt.gold;
	auc_cost.silver = aucamt.silver;
	auc_cost.copper = aucamt.copper;
	auc_count = 0;
	sprintf( log_buf, "%s a level %d object for %s",
	   auc_obj->short_descr, auc_obj->level, money_string( &auc_cost ));

        auc_channel( log_buf );
        sprintf( log_buf, "%s auctioning %s.", auc_held->name, auc_obj->name );
        log_string( log_buf, CHANNEL_GOD, -1 );
	return;
      }
     else
      {
        send_to_char( AT_WHITE, "You are not carrying that item.\n\r", ch );
        return;
      }
    }
    
    return;
}


void do_bid( CHAR_DATA *ch, char *argument )
{
  char buf[MAX_STRING_LENGTH];
  char arg[MAX_INPUT_LENGTH];
  MONEY_DATA amt;
  char arg2 [ MAX_STRING_LENGTH ];
  int min_bid = 0;
  bool bid_amt = TRUE;

  if ( !auc_obj )
  {
    send_to_char( AT_WHITE, "There is no auction at the moment.\n\r", ch );
    return;
  }

  if ( !auc_held )
  {
    bug( "Do_bid: auc_obj found without auc_held.\n\r",0);
    return;
  }

  if ( ch == auc_held )
  {
    send_to_char( AT_WHITE, "If you want your item back, you should 'auction remove' it.\n\r", ch );
    return;
  }

  if ( auc_bid && ch == auc_bid )
  {
    send_to_char( AT_WHITE, "You already hold the highest bid.\n\r", ch );
    return;
  }

  argument = one_argument( argument, arg );

  amt.gold = amt.silver = amt.copper = 0;

/* New syntax: bid 1000 copper */

  argument = one_argument( argument, arg2 );
  if ( is_number( arg ) )
  {
    if ( !str_cmp( arg2, "gold" ) )
      amt.gold = atoi( arg );
    else if ( !str_cmp( arg2, "silver" ) )
      amt.silver = atoi( arg );
    else if ( !str_cmp( arg2, "copper" ) )
      amt.copper = atoi( arg );
    else
    {
      send_to_char( AT_WHITE, "&WSyntax: bid &R<amount> <currency type>\n\r",
     		   ch );
      return;
    }
  }
  else
  {
    send_to_char( AT_WHITE, "&WSyntax: bid &R<amount> <currency type>\n\r", ch );
    return;
  }

  if ( auc_cost.gold > 0 )        /* has to be 100 plus */
  {
     min_bid = ( auc_cost.gold + 100 );
     if ( ( (amt.gold*C_PER_G) + (amt.silver*S_PER_G) +
 	    (amt.copper) ) < min_bid*C_PER_G )
	bid_amt = FALSE;
  }
  else if ( auc_cost.silver > 0 ) /* has to be 100 plus */
  {
     min_bid = ( auc_cost.silver + 100 );
     if ( ( (amt.gold*C_PER_G) + (amt.silver*S_PER_G) +
            (amt.copper) ) < min_bid*S_PER_G )
	bid_amt = FALSE;
  }
  else if ( auc_cost.copper > 0 ) /* has to be 50 plus */
  {
     min_bid = ( auc_cost.copper + 50 );
     if ( ( (amt.gold*C_PER_G) + (amt.silver*S_PER_G) +
     	    (amt.copper) ) < min_bid )
	bid_amt = FALSE;
   }
  /* else bug, auc_cost has to have a cost */

  if ( !bid_amt )
  {  
   send_to_char( AT_WHITE, "Bid is not high enough.\n\r", ch );
   return;
  }     

  if ( ( (ch->money.gold*C_PER_G) + (ch->money.silver*S_PER_G) +
         (ch->money.copper) ) < ( (amt.gold*C_PER_G) + (amt.silver*S_PER_G) +
         (amt.copper) ) )
  {
    send_to_char( AT_WHITE, "You are not carrying that much money.\n\r", ch );
    return;
  }
  
  REMOVE_BIT(ch->deaf, CHANNEL_AUCTION);
  sprintf( buf, "Amount bid on %s: %s", auc_obj->short_descr, money_string( &amt ) );
  auc_channel( buf );
  auc_cost.gold   = amt.gold;
  auc_cost.silver = amt.silver;
  auc_cost.copper = amt.copper;
  auc_count = 0;
  auc_bid = ch;
  return;
}

void do_chat( CHAR_DATA *ch, char *argument )
{
    if ( IS_STUNNED( ch, STUN_MAGIC ) )
    {
	send_to_char(AT_BLUE, "You try to speak but can not!\n\r", ch );
        return;
    }

    talk_channel( ch, argument, CHANNEL_CHAT, "chat" );
    return;
}
/* OOC added by Hannibal */
void do_ooc( CHAR_DATA *ch, char *argument )
{    
    talk_channel( ch, argument, CHANNEL_OOC, "OOC" );
    return;
}

/* VENT added by Angi */
void do_gratz( CHAR_DATA *ch, char *argument )
{
    talk_channel( ch, argument, CHANNEL_GRATZ, "&RG&PR&GA&CT&BZ" );
    return;
}

/*
 * Alander's new channels.
 */
void do_music( CHAR_DATA *ch, char *argument )
{
    if ( IS_STUNNED( ch, STUN_MAGIC ) )
    {
	send_to_char(AT_BLUE, "You try to speak but can not!\n\r", ch );
        return;
    }
    talk_channel( ch, argument, CHANNEL_MUSIC, "music" );
    return;
}



void do_question( CHAR_DATA *ch, char *argument )
{
    talk_channel( ch, argument, CHANNEL_QUESTION, "question" );
    return;
}



void do_answer( CHAR_DATA *ch, char *argument )
{
    talk_channel( ch, argument, CHANNEL_QUESTION, "answer" );
    return;
}



void do_shout( CHAR_DATA *ch, char *argument )
{
    if ( IS_STUNNED( ch, STUN_MAGIC ) )
    {
	send_to_char(AT_BLUE, "You try to speak but can not!\n\r", ch );
        return;
    }
    talk_channel( ch, argument, CHANNEL_SHOUT, "shout" );
    WAIT_STATE( ch, 12 );
    return;
}



void do_yell( CHAR_DATA *ch, char *argument )
{
    if ( IS_STUNNED( ch, STUN_MAGIC ) )
    {
	send_to_char(AT_BLUE, "You try to speak but can not!\n\r", ch );
        return;
    }

    talk_channel( ch, argument, CHANNEL_YELL, "yell" );
    return;
}

void do_immtalk( CHAR_DATA *ch, char *argument )
{
    if(!IS_NPC( ch ) && !IS_IMMORTAL(ch))
    {
        send_to_char(AT_WHITE,"You are still but mortal.\n\r", ch );
        return;
    }

    talk_channel( ch, argument, CHANNEL_IMMTALK, "immtalk" );
    return;
}

void do_say( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA       *gch;
    char             buf [ MAX_STRING_LENGTH ];
    char             buf2 [ MAX_STRING_LENGTH ];
    char             buf3 [ MAX_STRING_LENGTH ];

    if ( argument[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Say what?\n\r", ch );
	return;
    }

    if ( IS_STUNNED( ch, STUN_MAGIC ) )
    {
	send_to_char(AT_BLUE, "You try to speak but can not!\n\r", ch );
        return;
    }

    MOBtrigger = FALSE;
  for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
  {
   if ( gch->deleted )
    continue;

   if ( !IS_NPC( ch ) )
   {
    if ( !IS_NPC( gch ) )
    {
     if ( gch->pcdata->language[ch->pcdata->speaking] > number_percent() )
     {
      if ( ch->pcdata->condition[COND_DRUNK] >= 25 )
       sprintf( buf, "$n&X says '%s'", drunk_talk( ch, argument ) );
      else
       sprintf( buf, "$n&X says '%s'", argument );
     }
     else
     {
      if ( ch->pcdata->condition[COND_DRUNK] >= 25 )
       sprintf( buf, "$n&X says '%s'", translate( ch, drunk_talk( ch, argument ) ) );
      else
       sprintf( buf, "$n&X says '%s'", translate( ch, argument ) );
     }
    }
    else
    {
     if ( ch->pcdata->condition[COND_DRUNK] >= 25 )
      sprintf( buf, "$n&X says '%s'", drunk_talk( ch, argument ) );
     else
      sprintf( buf, "$n&X says '%s'", argument );
    }
   }
   else
    sprintf( buf, "$n&X says '%s'", argument );
    act(AT_LBLUE, buf, ch, argument, gch, TO_VICT );

    if ( !IS_NPC( gch ) && !IS_NPC( ch ) )
    {
     if ( gch->pcdata->language[ch->pcdata->speaking] < 75 && 
          gch->pcdata->language[ch->pcdata->speaking] < number_percent() )
     {
      sprintf( buf3, "Your understanding of the %s language grows.\n\r",
	flag_string( language_types, ch->pcdata->speaking ) );
      send_to_char( AT_WHITE, buf3, gch );
      gch->pcdata->language[ch->pcdata->speaking] += dice(2, 5);
     }
    }
  }

    sprintf( buf2, "You say '%s'", argument );
    act(AT_LBLUE, buf2, ch, argument, NULL, TO_CHAR );

    mprog_speech_trigger( argument, ch );
    return;
}



void do_tell( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        position;
    char buf [ MAX_STRING_LENGTH ];
   
    if ( !IS_NPC( ch ) && (   IS_SET( ch->act, PLR_SILENCE )
			   || IS_SET( ch->act, PLR_NO_TELL ) ) )
    {
	send_to_char(AT_WHITE, "Your message didn't get through.\n\r", ch );
	return;
    }

    if ( IS_SET( ch->in_room->room_flags, ROOM_SILENT ) )
    {
        send_to_char(AT_WHITE, "You can't do that here.\n\r", ch );
        return;
    }
    
    argument = one_argument( argument, arg );

    if ( arg[0] == '\0' || argument[0] == '\0' )
    {
	send_to_char(AT_WHITE, "Tell whom what?\n\r", ch );
	return;
    }

    /*
     * Can tell to PC's anywhere, but NPC's only in same room.
     * -- Furey
     */
    if (    (    !( victim = get_pc_world( ch, arg )   )
              && !( victim = get_char_world( ch, arg ) )  )
         || (    IS_NPC( victim ) 
              && victim->in_room != ch->in_room           ) )
    {
	send_to_char(AT_WHITE, "They aren't here.\n\r", ch );
	return;
    }

    if ( IS_SET( victim->in_room->room_flags, ROOM_SILENT ) )
    {
        act( AT_WHITE, "$E can't hear you.", ch, 0, victim, TO_CHAR );
        return;
    }
    
    if ( !IS_IMMORTAL( ch ) && !IS_AWAKE( victim ) )
    {
	act(AT_WHITE, "$E can't hear you.", ch, 0, victim, TO_CHAR );
	return;
    }
    
    if ( !IS_NPC( victim ) && ( !( victim->desc ) ) )
    {
        act(AT_WHITE, "$E is link-dead.", ch, 0, victim, TO_CHAR );
        return;
    }

    if ( !IS_NPC( victim ) && IS_SET( victim->act, PLR_AFK ) )
    {
        sprintf( buf, "%s %s.", victim->name, 
           ( victim->pcdata && victim->pcdata->afkchar[0] != '\0' )
           ? victim->pcdata->afkchar : "is AFK at the moment" );
        act( AT_WHITE, buf, ch, NULL, victim, TO_CHAR );
        return;
    } 
        
    if ( victim->desc && victim->desc->pString )
    {
      act( AT_WHITE, "$E is in a writing buffer.", ch, 0, victim, TO_CHAR );
      return;
    }
    act(AT_WHITE, "You tell $N '$t'", ch, argument, victim, TO_CHAR );
    position		= victim->position;
    victim->position	= POS_STANDING;
    act(AT_WHITE, "$n tells you '$t'", ch, argument, victim, TO_VICT );
    victim->position	= position;
    victim->reply	= ch;

    return;
}


void do_remote( CHAR_DATA *ch, char *argument )
/*** Remote, added by ShayDn 27/6/96 ***/

{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        position;

    if ( !IS_NPC( ch ) && (   IS_SET( ch->act, PLR_SILENCE )
			   || IS_SET( ch->act, PLR_NO_TELL ) ) )
    {
	send_to_char(AT_WHITE, "Your message didn't get through.\n\r", ch );
	return;
    }

    if ( IS_SET( ch->in_room->room_flags, ROOM_SILENT ) )
    {
        send_to_char(AT_WHITE, "You can't do that here.\n\r", ch );
        return;
    }
    
    argument = one_argument( argument, arg );

    if ( arg[0] == '\0' || argument[0] == '\0' )
    {
	send_to_char(AT_WHITE, "Remote what to whom?\n\r", ch );
	return;
    }

    /*
     * Can tell to PC's anywhere, but NPC's only in same room.
     * -- Furey
     */
    if ( !( victim = get_char_world( ch, arg ) )
	|| ( IS_NPC( victim ) && victim->in_room != ch->in_room ) )
    {
	send_to_char(AT_WHITE, "They aren't here.\n\r", ch );
	return;
    }

    if ( IS_SET( victim->in_room->room_flags, ROOM_SILENT ) )
    {
        act( AT_WHITE, "$E can't hear you.", ch, 0, victim, TO_CHAR );
        return;
    }
    
    if ( !IS_IMMORTAL( ch ) && !IS_AWAKE( victim ) )
    {
	act(AT_WHITE, "$E can't hear you.", ch, 0, victim, TO_CHAR );
	return;
    }
    
    if ( !IS_NPC( victim ) && ( !( victim->desc ) ) )
    {
        act(AT_WHITE, "$E is link-dead.", ch, 0, victim, TO_CHAR );
        return;
    }
    if ( victim->desc && victim->desc->pString )
    {
      act( AT_WHITE, "$E is in a writing buffer.", ch, 0, victim, TO_CHAR );
      return;
    }

    act(AT_WHITE, "You emote: '$n $t' to $N.", ch, argument, victim, TO_CHAR );
    position		= victim->position;
    victim->position	= POS_STANDING;
    act(AT_WHITE, "+ $n $t.", ch, argument, victim, TO_VICT );
    victim->position	= position;
    victim->reply	= ch;

    return;
}



void do_reply( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    int        position;

    if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_SILENCE ) )
    {
	send_to_char(AT_WHITE, "Your message didn't get through.\n\r", ch );
	return;
    }

    if ( !( victim = ch->reply ) )
    {
	send_to_char(AT_WHITE, "They aren't here.\n\r", ch );
	return;
    }

    if ( argument[0] == '\0' )
    {
        send_to_char(AT_WHITE, "Reply what?\n\r", ch );
        return;
    }

    if ( ( !IS_IMMORTAL( ch ) && !IS_AWAKE( victim ) ) 
           || ( IS_SET( victim->in_room->room_flags, ROOM_SILENT ) 
           && (get_trust(ch) < L_APP ) ) )
    {
	act(AT_WHITE, "$E can't hear you.", ch, 0, victim, TO_CHAR );
	return;
    }
    if ( victim->desc && victim->desc->pString )
    {
      act( AT_WHITE, "$E is in a writing buffer.", ch, 0, victim, TO_CHAR );
      return;
    }

    act(AT_WHITE, "You tell $N '$t'",  ch, argument, victim, TO_CHAR );
    position		= victim->position;
    victim->position	= POS_STANDING;
    act(AT_WHITE, "$n tells you '$t'", ch, argument, victim, TO_VICT );
    victim->position	= position;
    victim->reply	= ch;

    return;
}

void do_emote( CHAR_DATA *ch, char *argument )
{
    char  buf [ MAX_STRING_LENGTH ];
    char *plast;

    if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_NO_EMOTE ) )
    {
	send_to_char(AT_PURPLE, "You are an emotionless blob.\n\r", ch );
	return;
    }

    if ( argument[0] == '\0' )
    {
	send_to_char(AT_PURPLE, "Emote what?\n\r", ch );
	return;
    }

    for ( plast = argument; *plast != '\0'; plast++ )
	;

    sprintf( buf, "%s%s", (*argument == '\'') ? "" : " ", argument );
    if ( isalpha( plast[-1] ) )
	strcat( buf, "." );

    act(AT_PINK, "$n$T", ch, NULL, buf, TO_ROOM );
    act(AT_PINK, "$n$T", ch, NULL, buf, TO_CHAR );
    return;
}

void do_info( CHAR_DATA *ch, char *argument )
{
    info( argument, 0, 0, PORT_ALL );
    return;
}



void do_bug( CHAR_DATA *ch, char *argument )
{
    append_file( ch, BUG_FILE,  argument );
    send_to_char(AT_WHITE, "Ok.  Thanks.\n\r", ch );
    return;
}



void do_idea( CHAR_DATA *ch, char *argument )
{
    append_file( ch, IDEA_FILE, argument );
    send_to_char(AT_WHITE, "Ok.  Thanks.\n\r", ch );
    return;
}



void do_typo( CHAR_DATA *ch, char *argument )
{
    append_file( ch, TYPO_FILE, argument );
    send_to_char(AT_WHITE, "Ok.  Thanks.\n\r", ch );
    return;
}



void do_rent( CHAR_DATA *ch, char *argument )
{
    send_to_char(AT_WHITE, "Rent?! Ther's no stinkin rent here!  Just save and quit.\n\r", ch );
    return;
}



void do_qui( CHAR_DATA *ch, char *argument )
{
    send_to_char(AT_WHITE, "If you want to QUIT, you have to spell it out.\n\r", ch );
    return;
}



void do_quit( CHAR_DATA *ch, char *argument )
{
    DESCRIPTOR_DATA *d;
    CHAR_DATA *PeT;
    CHAR_DATA *gch;
    MONEY_DATA tax;     

    if ( IS_NPC( ch ) )
	return;

    if ( ch->position == POS_FIGHTING )
    {
	send_to_char(AT_WHITE, "No way! You are fighting.\n\r", ch );
	return;
    }

    if ( ch->position <= POS_INCAP )
    {
	send_to_char(AT_WHITE, "You're not DEAD yet.\n\r", ch );
	return;
    }

    if ( ch->combat_timer )
    {
        send_to_char(AT_WHITE, "Your adreneline is pumping too hard.\n\r",ch);
	return;
    }

    raffect_remall( ch );

    if ( (ch->money.gold > 100000) ||
	 (ch->money.silver/SILVER_PER_GOLD > 100000) ||
         (ch->money.copper/COPPER_PER_GOLD > 100000) )
    {

      tax.gold   = (ch->money.gold > 100000) ? ch->money.gold * 0.1
		   : 0;
      tax.silver = (ch->money.silver/SILVER_PER_GOLD > 100000) ?
		   ch->money.silver * 0.1 : 0;
      tax.copper = (ch->money.copper/COPPER_PER_GOLD > 100000) ?
		   ch->money.copper * 0.1 : 0;

      sprintf( log_buf, "A small man walks up and points at you. Two larger men walk out\n\r"
                        "of the shadows, beat the hell out of you and steal %s\n\r"
                        "The small man says 'Thanks for not keepin yer cash with us, ya punk'.\n\r"
                        "He leaves you with a small buisness card that reads, 'The bank'\n\r",
	       money_string( &tax ) );
      send_to_char( AT_WHITE, log_buf, ch );
      sub_money( &ch->money, &tax );
    }

   
   if ( ch->pcdata->port == PORT_PAP )
   {
    send_to_char(AT_BLUE, "< You look up at the greyish sky and see a glimmer of hope.       > \n\r", ch );
    send_to_char(AT_BLUE, "< You reach higher and higher, hopes of escape fill your mind...  > \n\r", ch );
    send_to_char(AT_BLUE, "< Just as you touch the sky, the sound of a whip cracking reminds > \n\r",ch );
    send_to_char(AT_BLUE, "< you that you will be returning to this horrid place once again. > \n\r\n\r", ch ); 
    send_to_char(C_DEFAULT, "", ch);
    if (   !IS_SET( ch->act, PLR_WIZINVIS )
        && !IS_AFFECTED2( ch, AFF_PLOADED ) )
     {
        act(AT_BLOOD, "$n has left the paradox.", ch, NULL, NULL, TO_ROOM );
        if ( !IS_SET( ch->act, PLR_CLOAKED ) )
        {
        if ( ch->pcdata->rank < RANK_STAFF )
  	 info( "%s has escaped to reality, for now...", (int)(ch->name), 0, PORT_PAP );
        else
  	 info( "%s has left the paradox for the time being.", (int)(ch->name), 0, PORT_PAP );
        }
     }
     else
     {
       for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
       {
           if ( ch != gch )
           if ( get_trust(gch) >= ch->wizinvis )
           {
              act(AT_BLOOD, "$N slightly phased has escaped to reality, for now.", 
                  gch, NULL, ch, TO_CHAR );
           }
       }
    }
   }

   if ( ch->pcdata->port == PORT_ROI )
   {
    send_to_char(AT_BLUE, "< You leave now, knowing that you will return at another time to continue > \n\r", ch );
    send_to_char(AT_BLUE, "< the fight for your homeland. This is not an avoidance of the terrible   > \n\r", ch );
    send_to_char(AT_BLUE, "< suffering thrust upon your people, but a regrouping of force. Fate will > \n\r",ch );
    send_to_char(AT_BLUE, "< prove to be in our favor and time will tell who the true victor will be.> \n\r\n\r", ch ); 
    send_to_char(C_DEFAULT, "", ch);
    if (   !IS_SET( ch->act, PLR_WIZINVIS )
        && !IS_AFFECTED2( ch, AFF_PLOADED ) )
     {
        act(AT_BLOOD, "$n has left the realm.", ch, NULL, NULL, TO_ROOM );
        if ( !IS_SET( ch->act, PLR_CLOAKED ) )
        {
        if ( ch->pcdata->rank < RANK_STAFF )
  	 info( "%s has left the realm.", (int)(ch->name), 0, PORT_ROI );
        else
  	 info( "%s has ascended to a higher realm for the time being.",
        (int)(ch->name), 0, PORT_ROI );
        }
     }
     else
     {
       for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
       {
           if ( ch != gch )
           if ( get_trust(gch) >= ch->wizinvis )
           {
              act(AT_BLOOD, "$N slightly phased has escaped to reality, for now.", 
                  gch, NULL, ch, TO_CHAR );
           }
       }
    }
   }

    if ( IS_AFFECTED2( ch, AFF_PLOADED ) )
	REMOVE_BIT( ch->affected_by2, AFF_PLOADED );

    if ( ch->pcdata->rank < RANK_BOSS )
      { 
       sprintf( log_buf, "%s has quit in room vnum %d.", ch->name, ch->in_room->vnum );
       log_string( log_buf, -1, -1 );
       wiznet( log_buf, ch, NULL, WIZ_LOGINS, 0, get_trust( ch ) );
      }
     
    /*
     * After extract_char the ch is no longer valid!
     */
    if ( auc_held && ch == auc_held && auc_obj )
    {
      if ( auc_bid )
      {
	if ( ( (auc_bid->money.gold*C_PER_G) + (auc_bid->money.silver*S_PER_G) +
	        auc_bid->money.copper ) < ( (auc_cost.gold*C_PER_G) +
	       (auc_cost.silver*S_PER_G) + auc_cost.copper ) )
	{
	   sprintf(log_buf, "Holder of %s has left; bidder cannot pay for item; returning to owner.",
		   auc_obj->short_descr );
          obj_to_char( auc_obj, ch );
        }
        else
        {
          sprintf(log_buf, "Holder of %s has left; selling item to last bidder.", 
                  auc_obj->short_descr );
          obj_to_char( auc_obj, auc_bid );
	  add_money( &ch->money, &auc_cost );
          spend_money( &auc_bid->money, &auc_cost );
        }
      }
      else
      {
        sprintf(log_buf, "Holder of %s has left; removing item from auction.",
                auc_obj->short_descr );
        auc_channel( log_buf );
        obj_to_char( auc_obj, ch );
      }
      auc_obj = NULL;
      auc_bid = NULL;
      auc_held = NULL;
      auc_cost.gold = auc_cost.silver = auc_cost.copper = 0;
      auc_count = -1;
    }

    if ( auc_bid && auc_bid == ch && auc_obj )
    {
      sprintf(log_buf, "Highest bidder for %s has left; returning item to owner.", auc_obj->short_descr );
      if ( auc_held )
	obj_to_char( auc_obj, auc_held );
      auc_channel( log_buf);
      auc_obj = NULL;
      auc_bid = NULL;
      auc_held = NULL;
      auc_cost.gold = auc_cost.silver = auc_cost.copper = 0;
      auc_count = -1;
    }
	
    save_char_obj( ch, TRUE );
    save_finger( ch );
    for ( PeT = ch->in_room->people; PeT; PeT = PeT->next_in_room )
    {
       if ( IS_NPC( PeT ) )
	  if ( IS_SET( PeT->act, ACT_PET ) && ( PeT->master == ch ) )
	  {
	    extract_char( PeT, TRUE );
	    break;
          }
    }       
    d = ch->desc;
    extract_char( ch, TRUE );
    if ( d )
     close_socket( d );

    return;
}



void do_delet( CHAR_DATA *ch, char *argument )
{
  send_to_char( C_DEFAULT,
      "If you want to DELETE yourself, spell it out!\n\r", ch );
  return;
}

void do_delete( CHAR_DATA *ch, char *argument )
{
  DESCRIPTOR_DATA *d;
  
  if ( !ch->desc )
    return;
  if ( str_cmp(ch->desc->incomm, "delete yes") )
  {
    send_to_char( C_DEFAULT,
        "If you want to DELETE yourself, type 'delete yes'\n\r", ch );
    return;
  }
  if ( ch->desc->original || IS_NPC(ch) )
  {
    send_to_char( C_DEFAULT, "You may not delete a switched character.\n\r",
        ch );
    return;
  }
  stop_fighting(ch, TRUE);
  send_to_char( C_DEFAULT, "You are no more.\n\r", ch );
  act(AT_BLOOD, "$n is no more.", ch, NULL, NULL, TO_ROOM );
  if ( ch->pcdata->port == PORT_PAP )
   info( "%s has taken the only route of escape.", (int)(ch->name), 0, PORT_PAP );
  else
   info( "%s has ascended to a more peaceful plane of existance.", (int)(ch->name), 0, PORT_ROI );
  sprintf(log_buf, "%s has DELETED in room vnum %d.", ch->name,
      ch->in_room->vnum);
  log_string(log_buf, -1, -1);
  wiznet(log_buf, ch, NULL, WIZ_LOGINS, 0, 0);
  if ( auc_held && ch == auc_held && auc_obj )
  {
    if ( auc_bid )
    {
     if ( ( (auc_bid->money.gold*C_PER_G) + (auc_bid->money.silver*S_PER_G) +
             auc_bid->money.copper ) < ( (auc_cost.gold*C_PER_G) +
            (auc_cost.silver*S_PER_G) + auc_cost.copper ) )
      {
        sprintf(log_buf, "Holder of %s has left; bidder cannot pay for item; "
            "returning to owner.", auc_obj->short_descr);
        obj_to_char( auc_obj, ch );
      }
      else
      {
        sprintf(log_buf, "Holder of %s has left; selling item to last bidder.",
            auc_obj->short_descr );
        obj_to_char( auc_obj, auc_bid );
        add_money( &ch->money, &auc_cost );
	spend_money( &auc_bid->money, &auc_cost );
      }
    }
    else
    {
      sprintf(log_buf, "Holder of %s has left; removing item from auction.",
        auc_obj->short_descr );
      auc_channel( log_buf );
      obj_to_char( auc_obj, ch );
    }
    auc_obj = NULL;
    auc_bid = NULL;
    auc_held = NULL;
    auc_cost.gold = auc_cost.silver = auc_cost.copper = 0;
    auc_count = -1;
  }

  if ( auc_bid && auc_bid == ch && auc_obj )
  {
    sprintf(log_buf, "Highest bidder for %s has left; returning item to owner.",
	    auc_obj->short_descr );
    if ( auc_held )
      obj_to_char( auc_obj, auc_held );
    auc_channel( log_buf);
    auc_obj = NULL;
    auc_bid = NULL;
    auc_held = NULL;
    auc_cost.gold = auc_cost.silver = auc_cost.copper = 0;
    auc_count = -1;
  }

  sprintf(log_buf, "rm -f %s%c/%s", PLAYER_DIR, LOWER(ch->name[0]),
      capitalize(ch->name));

  system( log_buf );
  strcat(log_buf, ".fng");
  system( log_buf );

  sprintf(log_buf, "rm %s%c/%s.cps", PLAYER_DIR, LOWER(ch->name[0]),
      capitalize(ch->name));

  system( log_buf );

  delete_playerlist( ch->name );

  d = ch->desc;
  extract_char(ch, TRUE);
  if ( d )
    close_socket(d);
  return;
}



void do_save( CHAR_DATA *ch, char *argument )
{
    if ( IS_NPC( ch ) )
	return;

    save_char_obj( ch, FALSE );
    save_finger( ch );

   if ( ch->pcdata->port == PORT_PAP )
   {
    if ( ch->pcdata->rank < RANK_STAFF )
    send_to_char(AT_WHITE, "The wardens snicker as they grant you your request.\n\r", ch );
    else
    send_to_char(AT_WHITE, "Sir, yes sir, your file saved sir, have a good day sir.\n\r", ch );
   }
   else
    send_to_char(AT_WHITE,
     "The gods hear your request and weave your progress into the tapestry of life.\n\r",
     ch );    
    return;
}



void do_follow( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_DGREEN, "Follow whom?\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
	send_to_char(AT_DGREEN, "They aren't here.\n\r", ch );
	return;
    }

    if ( IS_AFFECTED( ch, AFF_CHARM ) && ch->master )
    {
	act(AT_DGREEN, "But you'd rather follow $N!", ch, NULL, ch->master, TO_CHAR );
	return;
    }

    if ( victim == ch )
    {
	if ( !ch->master )
	{
	    send_to_char(AT_DGREEN, "Silly...you already follow yourself.\n\r", ch );
	    return;
	}
	stop_follower( ch );
	return;
    }

    if ( ch->master )
	stop_follower( ch );

    add_follower( ch, victim );
    return;
}



void add_follower( CHAR_DATA *ch, CHAR_DATA *master )
{
  
    if ( ch->master )
    {
	bug( "Add_follower: non-null master.", 0 );
	return;
    }

    ch->master        = master;
    ch->leader        = NULL;

    if ( can_see( master, ch ) )
	act(AT_GREEN, "$n now follows you.", ch, NULL, master, TO_VICT );

    act(AT_GREEN, "You now follow $N.",  ch, NULL, master, TO_CHAR );

    return;
}



void stop_follower( CHAR_DATA *ch )
{

    if ( !ch->master )
    {
	bug( "Stop_follower: null master.", 0 );
	return;
    }

    if ( IS_AFFECTED( ch, AFF_CHARM ) )
    {
	REMOVE_BIT( ch->affected_by, AFF_CHARM );
	affect_strip( ch, gsn_charm_person );
	affect_strip( ch, gsn_domination   );
    }

    if ( can_see( ch->master, ch ) )
	act(AT_GREEN, "$n stops following you.",
	    ch, NULL, ch->master, TO_VICT );
    act(AT_GREEN, "You stop following $N.",
	ch, NULL, ch->master, TO_CHAR );

    ch->master = NULL;
    ch->leader = NULL;
    return;
}



void die_follower( CHAR_DATA *ch, char *name )
{
    CHAR_DATA *fch;

    if ( ch->master )
	stop_follower( ch );

    ch->leader = NULL;

    for ( fch = char_list; fch; fch = fch->next )
    {
        if ( fch->deleted )
	    continue;
	if ( fch->master == ch )
	    stop_follower( fch );
	if ( fch->leader == ch )
	    fch->leader = NULL;
    }

    return;
}



void do_order( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    CHAR_DATA *och;
    CHAR_DATA *och_next;
    char       arg [ MAX_INPUT_LENGTH ];
    bool       found;
    bool       fAll;

    argument = one_argument( argument, arg );

    if ( arg[0] == '\0' || argument[0] == '\0' )
    {
	send_to_char(AT_GREY, "Order whom to do what?\n\r", ch );
	return;
    }

    if ( IS_AFFECTED( ch, AFF_CHARM ) )
    {
	send_to_char(AT_GREY, "You feel like taking, not giving, orders.\n\r", ch );
	return;
    }

    if ( !str_cmp( arg, "all" ) )
    {
	fAll   = TRUE;
	victim = NULL;
    }
    else
    {
	fAll   = FALSE;
	if ( !( victim = get_char_room( ch, arg ) ) )
	{
	    send_to_char(AT_GREY, "They aren't here.\n\r", ch );
	    return;
	}

	if ( victim == ch )
	{
	    send_to_char(AT_GREY, "Aye aye, right away!\n\r", ch );
	    return;
	}

	if ( !IS_AFFECTED( victim, AFF_CHARM ) || victim->master != ch )
	{
	    send_to_char(AT_GREY, "Do it yourself!\n\r", ch );
	    return;
	}
    }

    found = FALSE;
    for ( och = ch->in_room->people; och; och = och_next )
    {
        och_next = och->next_in_room;

        if ( och->deleted )
	    continue;

	if ( IS_AFFECTED( och, AFF_CHARM )
	    && och->master == ch 
	    && ( fAll || och == victim ) )
	{
	    found = TRUE;
	    act(AT_GREY, "$n orders you to '$t'.", ch, argument, och, TO_VICT );
	    interpret( och, argument );
	}
    }

    if ( found )
	send_to_char(AT_GREY, "Ok.\n\r", ch );
    else
	send_to_char(AT_GREY, "You have no followers here.\n\r", ch );
    return;
}



void do_group( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       buf [ MAX_STRING_LENGTH ];
    char       arg [ MAX_INPUT_LENGTH  ];

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	CHAR_DATA *gch;
	CHAR_DATA *leader;

	leader = ( ch->leader ) ? ch->leader : ch;
	sprintf( buf, "%s's group:\n\r", PERS( leader, ch ) );
	send_to_char(AT_DGREEN, buf, ch );

	for ( gch = char_list; gch; gch = gch->next )
	{
	    if ( gch->deleted )
	        continue;

	    if ( is_same_group( gch, ch ) )
	    {
		  sprintf( buf,
		  "[&R%s&G] %-12s &Y%4d/%4d &Ghp &C%4d/%4d mana &P%4d/%4d &Gmv &R%5d &Gxp\n\r",
			IS_NPC( gch ) ? "Mob"
			              : "",
			capitalize( PERS( gch, ch ) ),
			gch->hit,   MAX_HIT( gch ),
			MT( gch ),  MT_MAX(gch),
			gch->move,  MAX_MOVE(gch),
			IS_NPC(gch) ? 0 : gch->exp );

                if ( gch->gspell && gch->gspell->timer > 0 )
		 send_to_char(AT_YELLOW, buf, ch );
		else
 		 send_to_char(AT_GREEN, buf, ch );
	    }
	}
	return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
	send_to_char(AT_DGREEN, "They aren't here.\n\r", ch );
	return;
    }

    if ( ch->master || ( ch->leader && ch->leader != ch ) )
    {
	send_to_char(AT_DGREEN, "But you are following someone else!\n\r", ch );
	return;
    }

    if ( victim->master != ch && ch != victim )
    {
	act(AT_DGREEN, "$N isn't following you.", ch, NULL, victim, TO_CHAR );
	return;
    }

    if ( is_same_group( victim, ch ) && ch != victim )
    {
	victim->leader = NULL;
	act(AT_GREEN, "You remove $N from your group.", ch, NULL, victim, TO_CHAR    );
	act(AT_GREEN, "$n removes you from $s group.",  ch, NULL, victim, TO_VICT    );
	act(AT_GREEN, "$n removes $N from $s group.",   ch, NULL, victim, TO_NOTVICT );
	return;
    }

    victim->leader = ch;
    act(AT_GREEN, "$N joins your group.", ch, NULL, victim, TO_CHAR    );
    act(AT_GREEN, "You join $n's group.", ch, NULL, victim, TO_VICT    );
    act(AT_GREEN, "$N joins $n's group.", ch, NULL, victim, TO_NOTVICT );
    return;
}

/*
 * 'Split' originally by Gnort, God of Chaos.
 */
void do_split( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *gch;
    char       buf [ MAX_STRING_LENGTH ];
    char       arg [ MAX_INPUT_LENGTH  ];
    int        members;
    int        amount = 0;
    int        share  = 0;
    int        extra  = 0;
    MONEY_DATA amt;
    char arg2 [ MAX_STRING_LENGTH ];

    argument = one_argument( argument, arg );
    argument = one_argument( argument, arg2 );

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_YELLOW, "Split how much?\n\r", ch );
	return;
    }
/* split 5 copper,gold,silver */

    amount = is_number( arg ) ? atoi( arg ) : 0;
    amt.gold = amt.silver = amt.copper = 0;

    if ( !str_cmp( arg2, "gold" ) )
      amt.gold = amount;
    else if ( !str_cmp( arg2, "silver" ) )
      amt.silver = amount;
    else if ( !str_cmp( arg2, "copper" ) )
      amt.copper = amount;
    else
    {
      send_to_char( AT_WHITE, "&WSyntax: &Rsplit <amount> <currency type>\n\r", 
		    ch );
      return;
    }

    if ( ( amt.gold < 0 ) || ( amt.silver < 0 )
	|| ( amt.copper < 0 ) )
    {
        send_to_char(AT_YELLOW, "Your group wouldn't like that.\n\r", ch );
        return;
    }

    if ( ( amt.gold == 0 ) && ( amt.silver == 0 )
	&& ( amt.copper == 0 ) )
    {
        send_to_char(AT_YELLOW, "You hand out zero coins, but no one notices.\n\r", 
		     ch );
        return;
    }

    if ( ( ch->money.gold < amt.gold ) || ( ch->money.silver < amt.silver )
	|| ( ch->money.copper < amt.copper ) )
    {
	sprintf( buf, "You don't have that many %s coins.\n\r", arg2 );
        send_to_char(AT_YELLOW, buf, ch );
        return;
    }

    members = 0;
    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
     if ( !gch || gch->deleted )
      continue;

     if ( IS_NPC(gch) )
      continue;

     if ( is_same_group( gch, ch ) )
      members++;
    }

    if ( members < 2 )
     return;
	    
    share = amount / members;
    extra = amount % members;

    if ( share == 0 )
    {
     send_to_char(AT_YELLOW, "Don't even bother, cheapskate.\n\r", ch );
     return;
    }
    
    if ( !str_cmp( arg2, "gold" ) )
    {
     ch->money.gold -= amount;
     ch->money.gold += share + extra;
    }
    else if ( !str_cmp( arg2, "silver" ) )
    {
     ch->money.silver -= amount;
     ch->money.silver += share + extra;
    }
    else if ( !str_cmp( arg2, "copper" ) )
    {
     ch->money.copper -= amount;
     ch->money.silver += share + extra;
    }
   
    sprintf( buf,
        "You split %s  Your share is %d %s coins.\n\r",
        money_string( &amt ), share + extra, arg2 );
    send_to_char(AT_YELLOW, buf, ch );

    sprintf( buf, "$n splits %s  Your share is %d %s coins.",
        money_string( &amt ), share, arg2 );

    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
        if ( !gch || gch->deleted )
	    continue;

 	if ( gch != ch && is_same_group( gch, ch ) && !IS_NPC(gch) )
	{
	    act(C_DEFAULT, buf, ch, NULL, gch, TO_VICT );
	    gch->money.gold   += ( !str_cmp( arg2, "gold" ) ) ?
			         share : 0;
	    gch->money.silver += ( !str_cmp( arg2, "silver" ) ) ?
				 share : 0;
	    gch->money.copper += ( !str_cmp( arg2, "copper" ) ) ?
				 share : 0;
	}
    }

    return;
}

void do_gtell( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *gch;
    char       buf [ MAX_STRING_LENGTH ];

    if ( argument[0] == '\0' )
    {
	send_to_char(AT_GREEN, "Tell your group what?\n\r", ch );
	return;
    }

    if ( IS_SET( ch->act, PLR_NO_TELL ) )
    {
	send_to_char(AT_GREEN, "Your message didn't get through!\n\r", ch );
	return;
    }

    /*
     * Note use of send_to_char, so gtell works on sleepers.
     */
    sprintf( buf, "%s tells the group '%s'.\n\r", ch->name, argument );
    for ( gch = char_list; gch; gch = gch->next )
    {
     if ( gch->deleted )
      continue;

     if ( is_same_group( gch, ch ) )
      send_to_char(AT_GREEN, buf, gch );
    }

    return;
}

/* Returns the size of a target char's group, NPC's included */
int group_size( CHAR_DATA *ch )
{
 CHAR_DATA *member;
 int size = 0;

    for ( member = ch->in_room->people; member; member = member->next_in_room )
    {
     if ( !member || member->deleted )
      continue;

     if ( is_same_group( member, ch ) )
      size += 1;
    }

 return size;
}

/* Returns if target is in a group or not */
bool in_group( CHAR_DATA *ch )
{
 CHAR_DATA *member;
 bool      ingroup = FALSE;

    for ( member = ch->in_room->people; member; member = member->next_in_room )
    {
     if ( !member || member->deleted )
      continue;

     if ( ch == member )
      continue;

     if ( is_same_group( member, ch ) )
      ingroup = TRUE;
    }

 return ingroup;
}

/*
 * It is very important that this be an equivalence relation:
 * (1) A ~ A
 * (2) if A ~ B then B ~ A
 * (3) if A ~ B  and B ~ C, then A ~ C
 */
bool is_same_group( CHAR_DATA *ach, CHAR_DATA *bch )
{
    if ( ach->deleted || bch->deleted )
      return FALSE;
    if ( (ach->leader) )
     ach = ach->leader;
    if ( (bch->leader) )
     bch = bch->leader;
    if ( ach->deleted || bch->deleted )
      return FALSE;
    return ach == bch;
}

void do_beep ( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char arg[MAX_INPUT_LENGTH];
    char buf[MAX_STRING_LENGTH];

    if (IS_NPC(ch))
        return;

    argument = one_argument( argument, arg );

    if  ( arg[0] == '\0' )
    {
        send_to_char(AT_WHITE, "Beep who?\n\r", ch );
        return;
    }

    if ( !( victim = get_char_world( ch, arg ) ) )
    {
        send_to_char(AT_WHITE, "They are not here.\n\r", ch );
        return;
    }


    if ( IS_NPC(victim))
    {
        send_to_char(AT_WHITE, "They are not beepable.\n\r", ch );
        return;
    }

    sprintf( buf, "\aYou beep %s.\n\r", victim->name );
    send_to_char(AT_WHITE, buf, ch );

    sprintf( buf, "\a%s has beeped you.\n\r", ch->name );
    editor_send_to_char(AT_WHITE, buf, victim );
    return;
}
