/* This is where all of the casino games are. -Flux */

#define linux 1
#if defined( macintosh )
#include <types.h>
#else
#include <sys/types.h>
#endif
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include "merc.h"

bool casino_simple_dice( CHAR_DATA *ch, CHAR_DATA *dealer, int value )
{
 char buf[MAX_STRING_LENGTH];
 int  dice1 = dice( 1, 6 );
 int  dice2 = dice( 1, 6 );
 int  dice_total;

 sprintf( buf, "Dice one: &G%d&X, Dice two: &G%d&X\n\r", dice1, dice2 );
 send_to_char( AT_WHITE, buf, ch );

 dice_total = dice1 + dice2;

 if ( dice_total == value )
  return TRUE;

 return FALSE;
}

bool casino_three_card_monty( CHAR_DATA *ch, CHAR_DATA *dealer, int card )
{
 char buf[MAX_STRING_LENGTH];
 CARD_DATA l;
 CARD_DATA m;
 CARD_DATA r;

 CARD_DATA *left;
 CARD_DATA *middle;
 CARD_DATA *right;
 int rand = dice( 1, 4 );

 l.value = 12;
 m.value = 12;
 r.value = 12;

 switch( rand )
 {
  case 1:
   l.suit = 2;
   m.suit = 1;
   r.suit = 3;
   break;
  case 2:
   l.suit = 3;
   m.suit = 2;
   r.suit = 1;
   break;
  case 3:
   l.suit = 1;
   m.suit = 3;
   r.suit = 2;
   break;
  case 4:
   l.suit = 1;
   m.suit = 3;
   r.suit = 1;
   break;
  }

  left   = &l;
  middle = &m;
  right  = &r;

 sprintf( buf, "Left card:   &G%s&X of ",
  flag_string( card_faces, left->value ) );
 send_to_char( AT_WHITE, buf, ch );

 sprintf( buf, "&G%s&X\n\r",
  flag_string( card_suits, left->suit ) );
 send_to_char( AT_WHITE, buf, ch );

 sprintf( buf, "Middle card: &G%s&X of ",
  flag_string( card_faces, middle->value ) );
 send_to_char( AT_WHITE, buf, ch );

 sprintf( buf, "&G%s&X\n\r",
  flag_string( card_suits, middle->suit ) );
 send_to_char( AT_WHITE, buf, ch );

 sprintf( buf, "Right card:  &G%s&X of ",
  flag_string( card_faces, right->value ) );
 send_to_char( AT_WHITE, buf, ch );

 sprintf( buf, "&G%s&X\n\r",
  flag_string( card_suits, right->suit ) );
 send_to_char( AT_WHITE, buf, ch );


 if ( card == rand )
  return TRUE;

 return FALSE;
}

