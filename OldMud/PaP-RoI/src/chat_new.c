/*****************************************************************************
 * Chatmode.c  - Merc-based confrence system.                                *
 *                                                                           *
 * Full chat-mode setup for Merc based MUDS, or could be used separately if  *
 * you cut and pasted the needed parts from Merc code.                       *
 * Created as an addon for Eye of the Storm MUD (network.sos.on.ca 1234)     *
 * -- Altrag Dalosein, Lord of the Dragons..                                 *
 *****************************************************************************/
#define linux 1
#if defined(macintosh)
#include <types.h>
#else
#include <sys/types.h>
#endif
#include <ctype.h>
#include <stdio.h>
#include <string.h>
#include <time.h>
#include "chatmode.h"

/*
 * Externals
 */
bool  check_social    args( ( CHAR_DATA *ch, char *command, char *argument ) );
extern int port;

/*
 * Locals
 */
void    start_chat_mode     args( ( DESCRIPTOR_DATA *d ) );
void    init_chat           args( ( void ) );
void    dispose_room        args( ( CHAT_ROOM *room ) );

CHAT_ROOM *chat_rooms;
CHAT_ROOM *last_chat_room;
CHAR_DATA *chat_list;
CHAR_DATA *old_chars;

void start_chat_mode( DESCRIPTOR_DATA *d )
{
  CHAR_DATA *ch;

  if ( !d || !d->character )
    return;

  if ( !chat_rooms )
    init_chat( );

  if ( d->original )
    do_return( d->character, "" );

  if ( d->pEdit )
  {
    d->pEdit = NULL;
    d->editor = 0;
  }

  if ( d->inEdit )
  {
    d->inEdit = NULL;
    d->editin = 0;
  }

  for ( ch = char_list; ch; ch = ch->next )
  {
    if ( ch->reply == d->character )
      ch->reply = NULL;
    if ( ch->master == d->character )
      stop_follower( ch );
    if ( ch->leader == d->character )
      ch->leader = NULL;
    if ( ch->hunting == d->character )
      ch->hunting = NULL;
    if ( ch->fighting == d->character )
      stop_fighting( ch, FALSE );
  }

  if ( d->character == char_list )
    char_list = char_list->next;
  else
  {
    for ( ch = char_list; ch; ch = ch->next )
      if ( ch->next == d->character )
	break;
    if ( ch )
      ch->next = d->character->next;
  }

  if ( d->character->was_in_room )
  {
    if ( !d->character->in_room )
      char_to_room( d->character, d->character->was_in_room );
  }

  if ( d->character->in_room )
  {
    d->character->was_in_room = d->character->in_room;
    char_from_room( d->character );
  }

  d->original = d->character;

  /*
   * These are the freaks who go link_dead (and lose their descriptor)
   * from inside chat mode.
   * -- Altrag
   */
  d->character->next = old_chars;
  old_chars = d->character;

  ch = alloc_mem( sizeof( *ch ) );
  ch->desc = d;
  ch->name = str_dup( d->character->name );
  ch->act = d->character->act;
  ch->sex = d->character->sex;
  ch->position = POS_STANDING;
  ch->hit = 1;
  ch->perm_hit = 1;
  ch->mod_hit = 0;

  ch->next = chat_list;
  chat_list = ch;

  d->character = ch;
  d->connected = CON_CHATTING;
  char_to_room(ch, chat_rooms->pRoom);
  act(num_color(ch), "$n &Yhas entered the room.", ch, NULL, NULL, TO_ROOM);
  send_room_stuff( ch );
  return;
}

void stop_chat_mode( CHAR_DATA *ch )
{
  DESCRIPTOR_DATA *d;
  CHAT_ROOM *room;

  d = ch->desc;

  act(num_color(ch), "$n &Yhas left the room.", ch, NULL, NULL, TO_ROOM);
  char_from_room( ch );

  if ( ch == chat_list )
    chat_list = ch->next;
  else
  {
    CHAR_DATA *gch;

    for ( gch = chat_list; gch; gch = gch->next )
      if ( gch->next == ch )
	break;
    if ( gch )
      gch->next = ch->next;
  }

  for ( room = chat_rooms; room; room = room->next )
    if (!str_prefix( ch->name, room->pRoom->name ) ||
	 is_name( NULL, ch->name, room->pRoom->name ))
      break;
  if ( room )
  {
    if ( room == chat_rooms )
      chat_rooms = room->next;
    else
    {
      CHAT_ROOM *rprev;
	
      for ( rprev = chat_rooms; rprev; rprev = rprev->next )
	if ( rprev->next == room )
	  break;
      if ( rprev )
	rprev->next = room->next;
      if ( !rprev->next )
	last_chat_room = rprev;
    }
    dispose_room(room);
  }

/*  save_char_chat( ch );*/
  free_char( ch );

  if ( !d )
  {
    return;
  }

  if ( !d->original )
  {
    close_socket( d );
    return;
  }

  d->character = d->original;
  d->original = NULL;

  if ( d->character == old_chars )
    old_chars = old_chars->next;
  else
  {
    CHAR_DATA *gch;

    for ( gch = old_chars; gch; gch = gch->next )
      if ( gch->next == d->character )
	break;
    if ( gch )
      gch->next = d->character->next;
  }

  d->character->next = char_list;
  char_list = d->character;

  if ( d->character->was_in_room )
    char_to_room(d->character, d->character->was_in_room);
  else
    char_to_room(d->character, get_room_index(ROOM_ELF_TEMPLE) );

  d->character->was_in_room = NULL;
  d->connected = CON_PLAYING;
  do_look(d->character, "");

  return;
}

void chat_interp( CHAR_DATA *ch, char *argument )
{
  while ( isspace(*argument) )
    ++argument;

  if ( !*argument )
  {
    send_room_stuff( ch );
    return;
  }

  if ( chat_command( ch, argument ) )
    return;

  if ( *argument == ',' )
  {
    do_emote(ch, argument + 1);
    return;
  }

  if ( *argument == '.' )
  {
    char arg[MAX_INPUT_LENGTH];
    char *p_arg;

    p_arg = one_argument( argument, arg );
    if ( check_social( ch, arg + 1, p_arg ) )
      return;
  }

  act( num_color(ch), "$n&Y: &G$t", ch, argument, NULL, TO_ROOM );
  send_to_char( AT_RED, "-- &CMessage sent &R--\n\r",ch);
  return;
}

CHAR_DATA *get_char_chat( CHAR_DATA *ch, char *name )
{
  CHAR_DATA *vch;

  if ( !str_prefix( name, ch->name ) || is_name( NULL, name, ch->name ) )
    return ch;

  for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
    if ( !str_prefix( name, vch->name ) || is_name( NULL, name, vch->name ) )
      return vch;

  for ( vch = chat_list; vch; vch = vch->next )
    if ( !str_prefix( name, vch->name ) || is_name( NULL, name, vch->name ) )
      return vch;

  return NULL;
}

void send_room_stuff( CHAR_DATA *ch )
{
  int width = 0;
  CHAR_DATA *vch;
  int num = 0;

  if ( !ch->in_room )
    return;

  send_to_char( AT_WHITE, ch->in_room->name, ch );
  send_to_char( C_DEFAULT, "\n\r", ch );
  if ( ch->in_room->description[0] != '\0' )
    send_to_char(AT_YELLOW, ch->in_room->description, ch );
  send_to_char( C_DEFAULT, "\n\r", ch );
  for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
  {
    if ( vch == ch )
      continue;

    num++;
    width += strlen( vch->name );
    if ( width >= 79 )
    {
      send_to_char(C_DEFAULT, "\n\r", ch );
      width = 0;
    }
    send_to_char(num_color(vch), vch->name, ch );
    send_to_char(C_DEFAULT, " ", ch );
  }
  if ( num == 0 )
    send_to_char(AT_PURPLE, "You are all alone.",ch);
  send_to_char(C_DEFAULT, "\n\r\n\r", ch );
  return;
}

void do_conference( CHAR_DATA *ch, char *argument )
{
  if ( IS_NPC(ch) && (!ch->desc || !ch->desc->original ) )
    return;
  if ( ch->fighting || ch->position == POS_FIGHTING )
  {
    send_to_char( AT_WHITE, "No way!  You are fighting!.\n\r",ch);
    return;
  }
  if ( ch->combat_timer )
  {
    send_to_char(AT_WHITE, "You can't right now.\n\r",ch);
    return;
  }
  if ( ch->position < POS_STUNNED && ch->level < LEVEL_IMMORTAL )
  {
    send_to_char(AT_WHITE, "You're not DEAD yet!\n\r",ch);
    return;
  }
  ch->hunting = NULL;
  save_char_obj( ch, FALSE );
  start_chat_mode( ch->desc );
}

void init_chat( void )
{
  if ( chat_rooms )
    return;

  chat_rooms = alloc_mem( sizeof( *chat_rooms ));
  chat_rooms->pRoom = alloc_mem(sizeof(*chat_rooms->pRoom));
  chat_rooms->invited = str_dup("");
  chat_rooms->pRoom->name = str_dup( "Main" );
  chat_rooms->pRoom->description = str_dup( "&GEye of the &BS&Ct&Wo&Cr&Bm "
					    "&GMain teleconference channel\n\r"
					    "Type /h for help." );
  last_chat_room = chat_rooms;
  return;
}

int num_color( CHAR_DATA *ch )
{
  switch ( ch->sex )
  {
  case SEX_NEUTRAL:
    return AT_GREEN;
  case SEX_MALE:
    return AT_BLUE;
  case SEX_FEMALE:
    return AT_RED;
  }
  return C_DEFAULT;
}

char *get_color( CHAR_DATA *ch )
{
  switch ( ch->sex )
  {
  case SEX_NEUTRAL:
    return "G";
  case SEX_MALE:
    return "B";
  case SEX_FEMALE:
    return "R";
  }
  return "w";
}

/*
 * Update the chat_list and the old_chars lists.  Just kicks out linkdeads
 * if this becomes too much lag, you could just make a modified version
 * of check_reconnect in comm.c to check those lists as well as the char_list
 * list.  -- Altrag
 */
void chat_update( void )
{
  CHAR_DATA *ch;
  CHAR_DATA *ch_next;

  for ( ch = old_chars; ch; ch = ch_next )
  {
    ch_next = ch->next;

    if ( !ch->desc )  /* i.e. They went into chat but dropped link */
    {
      if ( ch == old_chars )
	old_chars = ch->next;
      else
      {
	CHAR_DATA *och;

	for ( och = old_chars; och; och = och->next )
	  if ( och->next == ch )
	    break;
	if ( och )
	  och->next = ch->next;
      }
      ch->next = char_list;
      char_list = ch;
      if ( ch->was_in_room )
	char_to_room( ch, ch->was_in_room );
    }
  }
  for ( ch = chat_list; ch; ch = ch_next )
  {
    ch_next = ch->next;
    if ( !ch->desc )
    {
      stop_chat_mode( ch );
    }
  }
  return;
}

void dispose_room( CHAT_ROOM *room )
{
  CHAR_DATA *ch;
  CHAR_DATA *ch_next;

  for ( ch = room->pRoom->people; ch; ch = ch_next )
  {
    ch_next = ch->next_in_room;
    chat_command(ch, "/join Main");
  }
  free_string(room->pRoom->name);
  free_string(room->pRoom->description);
  free_mem(room->pRoom, sizeof(*room->pRoom));
  free_string(room->invited);
  free_mem(room, sizeof(*room));
  return;
}
