/*****************************************************************************
 * Interface for group spell casting.                                        *
 * -- Altrag                                                                 *
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
 * Local functions
 */
void add_gspell  args ( ( CHAR_DATA *ch, int sn, int level, void *vo ) );
int gslot_lookup  args ( ( int sn ) );
bool is_same_gspell  args ( ( CHAR_DATA *ch, CHAR_DATA *victim ) );



bool is_same_gspell( CHAR_DATA *ch, CHAR_DATA *victim )
{
  if ( !is_same_group( ch, victim ) )
    return FALSE;
  if ( !ch->gspell || !victim->gspell )
    return FALSE;
  if ( ch->gspell->sn != victim->gspell->sn )
    return FALSE;
  if ( ch->gspell->victim != victim->gspell->victim )
    return FALSE;
  return TRUE;
}

void set_gspell( CHAR_DATA *ch, GSPELL_DATA *gsp )
{
  GSPELL_DATA *gsp_new;

  gsp_new = alloc_mem( sizeof(*gsp_new) );

  *gsp_new = *gsp;
  ch->gspell = gsp_new;
}

void end_gspell( CHAR_DATA *ch )
{
  if ( !ch->gspell )
  {
    bug( "end_gspell: no gspell", 0 );
    return;
  }
  free_mem( ch->gspell, sizeof(*ch->gspell) );
  ch->gspell = NULL;
}

/*
 * Implementation stuff
 */
void check_gcast( CHAR_DATA *ch )
{
  CHAR_DATA *gch;
  int looper;
  int casters[MAX_CLASS];
  int sn;
  int level = 0;
  int total = 0;

  if ( !ch->gspell || ch->gspell->timer <= 0 )
    return;

  sn = ch->gspell->sn;

  for ( looper = 0; looper < MAX_CLASS; looper++ )
    casters[looper] = 0;

  for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
  {
    if ( is_same_gspell( ch, gch ) )
    {
      casters[prime_class(gch)]++;
      total++;
      level = (level / 4) + gch->gspell->level;
    }
  }

  for ( looper = 0; looper < MAX_CLASS; looper++ )
    if ( casters[looper] < gskill_table[gslot_lookup(sn)].casters[looper] )
    {
      send_to_char(AT_BLUE, "Ok.\n\r", ch );
      return;
    }

  sn = slot_lookup(sn);
  (* skill_table[sn].spell_fun) ( sn, level, ch->leader ? ch->leader : ch,
				  ch->gspell->victim );
  for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
  {
    if ( is_same_gspell( ch, gch ) && ch != gch )
      end_gspell( gch );
  }
  switch (skill_table[sn].target)
  {
  case TAR_GROUP_OFFENSIVE:
    {
      CHAR_DATA *victim = (CHAR_DATA *)ch->gspell->victim;

      rprog_cast_sn_trigger( ch->in_room, ch, sn, victim );
      if ( !victim->fighting )
	if ( IS_NPC( victim ) || (ch->leader ? ch->leader : ch) == victim )
	  multi_hit( victim, ch->leader ? ch->leader : ch, TYPE_UNDEFINED );
    }
    break;
  case TAR_GROUP_DEFENSIVE:
    rprog_cast_sn_trigger( ch->in_room, ch, sn, ch->gspell->victim );
    break;
  case TAR_GROUP_ALL:
    rprog_cast_sn_trigger( ch->in_room, ch, sn, ch );
    break;
  case TAR_GROUP_IGNORE:
    rprog_cast_sn_trigger( ch->in_room, ch, sn,
			  (ch->gspell->victim ? ch->gspell->victim : ch) );
    break;
  case TAR_GROUP_OBJ:
    if ( ch->gspell->victim )
    {
      oprog_cast_sn_trigger( ch->gspell->victim, ch, sn, ch->gspell->victim );
      rprog_cast_sn_trigger( ch->in_room, ch, sn, ch->gspell->victim );
    }
    break;
  }
  end_gspell( ch );
  return;
}

int gslot_lookup( int sn )
{
  int count;

  for ( count = 0; count < MAX_GSPELL; count++ )
    if ( gskill_table[count].slot == sn )
      return count;

  bug( "Gslot_lookup: sn not found #%d", sn );
  return 0;
}

void add_gspell( CHAR_DATA *ch, int sn, int level, void *vo )
{
  CHAR_DATA *gch;
  GSPELL_DATA gsp;

  for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
  {
    if ( gch == ch || !gch->gspell || gch->gspell->timer <= 0 )
      continue;
    if ( is_same_group( ch, gch ) && gch->gspell->sn == sn )
    {
      switch ( skill_table[sn].target )
      {
      case TAR_GROUP_DEFENSIVE:
      case TAR_GROUP_OFFENSIVE:
      case TAR_GROUP_OBJ:
      case TAR_GROUP_IGNORE:
	if ( gch->gspell->victim != vo )
	  continue;
	break;
      }
      break;
    }
  }
  sn = skill_table[sn].slot;
  gsp.sn = sn;
  gsp.victim = vo;
  gsp.level = level;
  if ( !gch || !gch->gspell )
    gsp.timer = gskill_table[gslot_lookup(sn)].wait;
  else
    gsp.timer = gch->gspell->timer;
  set_gspell( ch, &gsp );
  check_gcast( ch );
  return;
}

void group_cast( int sn, int level, CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim = NULL;
  OBJ_DATA *obj = NULL;
  int mana;

  if ( IS_NPC(ch) )
    return;

  if ( ch->gspell && ch->gspell->timer > 0 )
  {
    send_to_char(AT_BLUE,"You already have a group spell in progress.\n\r",ch);
    return;
  }

  mana = SPELL_COST(ch,sn);

  if ( number_percent( ) > ch->pcdata->learned[sn] )
  {
    send_to_char(AT_BLUE, "You lost your concentration.\n\r",ch);
    MT( ch ) -= mana / 2;
    return;
  }

  MT( ch ) -= mana;
  update_skpell( ch, sn );

  switch ( skill_table[sn].target )
  {
  default:
    bug( "group_cast: non-group target on sn #%d.", sn );
    return;
  case TAR_GROUP_DEFENSIVE:
    if ( argument[0] == '\0' )
    {
      victim = ch;
      break;
    }
    if ( !( victim = get_char_room( ch, argument ) ) )
    {
      send_to_char( AT_BLUE, "They aren't here.\n\r", ch );
      return;
    }
    add_gspell( ch, sn, level, (void *) victim );
    break;
  case TAR_GROUP_OFFENSIVE:
    if ( argument[0] == '\0' )
    {
      if ( ch->fighting )
	victim = ch->fighting;
      else
      {
	send_to_char(AT_BLUE, "Cast the spell on whom?\n\r",ch);
	return;
      }
    }
    else
    {
      if ( !( victim = get_char_room( ch, argument ) ) )
      {
	send_to_char( AT_BLUE, "They aren't here.\n\r", ch );
	return;
      }
    }
    add_gspell( ch, sn, level, (void *) victim );
    break;
  case TAR_GROUP_ALL:
    add_gspell( ch, sn, level, NULL );
    break;
  case TAR_GROUP_OBJ:
    if ( !( obj = get_obj_list( ch, argument, ch->in_room->contents ) ) )
    {
      send_to_char( AT_WHITE, "You don't see that.\n\r", ch );
      return;
    }
    add_gspell( ch, sn, level, (void *) obj );
    break;
  case TAR_IGNORE:
    add_gspell( ch, sn, level, (void *) argument );
    break;
  }

  return;
}

void gspell_flamesphere( int sn, int level, CHAR_DATA *ch, void *vo )
{
  CHAR_DATA *victim = (CHAR_DATA *) vo;
  int dam;

  dam = dice(20, level);
  if ( saves_spell( level, victim ) )
    dam /= 2;
  damage( ch, victim, dam, sn, DAM_HEAT, TRUE );
  return;
}

void gspell_mass_shield( int sn, int level, CHAR_DATA *ch, void *vo )
{
  CHAR_DATA *vch;

  for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
  {
    if ( !is_same_group( ch, vch ) )
      continue;

    switch ( number_range( 1, 6 ) )
    {
    case 1:
      spell_fireshield( skill_lookup( "fireshield" ), level, vch, vch );
      break;
    case 2:
      spell_shockshield( skill_lookup( "shockshield" ), level, vch, vch );
      break;
    case 3:
      spell_iceshield( skill_lookup( "iceshield" ), level, vch, vch );
      break;
    case 4:
      spell_chaosfield( skill_lookup( "chaos field" ), level, vch, vch );
      break;
    case 5:
      spell_vibrate( skill_lookup( "vibrate" ), level, vch, vch );
      break;
    case 6:
      spell_sanctuary( skill_lookup( "sanctuary" ), level, vch, vch );
      break;
    }
  }
  return;
}
