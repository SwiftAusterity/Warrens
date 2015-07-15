/*****************************************************************************
 * Chatmode.h  -- Online chat mode for Merc-based MUD games                  *
 *                                                                           *
 * Interface stuff for Chatmode.c (and related files if I make some)         *
 * -- Altrag                                                                 *
 *****************************************************************************/

#include "merc.h"  /* Delcare_do_fun, ect commands. */

typedef struct chat_room CHAT_ROOM;

struct chat_room
{
  CHAT_ROOM *next;
  ROOM_INDEX_DATA *pRoom;
  char *invited;
};

extern CHAT_ROOM *chat_rooms;
extern CHAT_ROOM *last_chat_room;
extern CHAR_DATA *chat_list;
extern CHAR_DATA *old_chars;

bool chat_command( CHAR_DATA *ch, char *argument );
void stop_chat_mode( CHAR_DATA *ch );
void chat_interp( CHAR_DATA *ch, char *argument );
#define CD CHAR_DATA *
CD   get_char_chat( CHAR_DATA *ch, char *argument );
#undef CD
void send_room_stuff( CHAR_DATA *ch );
int  num_color( CHAR_DATA *ch );
char *get_color( CHAR_DATA *ch );

DECLARE_DO_FUN( chat_page     );
DECLARE_DO_FUN( chat_show     );
DECLARE_DO_FUN( chat_who      );
DECLARE_DO_FUN( chat_topic    );
DECLARE_DO_FUN( chat_help     );
DECLARE_DO_FUN( chat_join     );
DECLARE_DO_FUN( chat_invite   );
DECLARE_DO_FUN( chat_uninvite );
