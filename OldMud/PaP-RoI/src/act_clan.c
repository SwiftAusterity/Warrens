/*****************************************************************************
 * Clan routines for clan funtions.. I hope to move the ones created prior   *
 * to this file in here eventually as well.                                  *
 * -- Altrag Dalosein, Lord of the Dragons                                   *
 *****************************************************************************/

#define linux 1
#if defined( macintosh )
#include <types.h>
#else
#include <sys/types.h>
#endif
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "merc.h"


/*
 * Illuminati bestow command, for deity only.
 */
void do_bestow( CHAR_DATA *ch, char *argument )
{
  char arg[MAX_INPUT_LENGTH];
  CHAR_DATA *victim;

  if ( ch->clan != 1 || ch->clev < 5 )
  {
    send_to_char(C_DEFAULT, "Huh?\n\r", ch );
    return;
  }

  argument = one_argument( argument, arg );

  if ( !( victim = get_char_room( ch, arg ) ) || IS_NPC(victim) )
  {
    send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
    return;
  }

  if ( victim->clan != ch->clan )
  {
    send_to_char(C_DEFAULT, "They aren't in your clan!\n\r", ch );
    return;
  }

  if ( !IS_SET( victim->act, PLR_CSKILL ) )
  {
    SET_BIT( victim->act, PLR_CSKILL );
    act(AT_PINK, "$N bestowed with the Transmute skill.", ch, NULL, victim,
	TO_CHAR );
    send_to_char( AT_PINK, "You have been given the Transmute skill.\n\r",
		  victim );
  }
  else
  {
    REMOVE_BIT( victim->act, PLR_CSKILL );
    act(AT_PINK, "Transmute denied from $N.", ch, NULL, victim, TO_CHAR );
    send_to_char( AT_PINK, "You have been denied the Transmute skill.\n\r",
		  victim );
  }
}
