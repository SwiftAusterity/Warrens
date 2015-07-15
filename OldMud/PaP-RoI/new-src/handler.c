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
#include <string.h>
#include <time.h>
#include <math.h>
#include "merc.h"

AFFECT_DATA *		affect_free;

/*
 * Local functions.
 */
int	spell_duration	args( ( CHAR_DATA *victim, int sn ) );
void 	perm_spell	args( ( CHAR_DATA *victim, int sn ) );
void    affect_modify   args( ( CHAR_DATA *ch, AFFECT_DATA *paf, bool fAdd ) );
void    affect_modify2  args( ( CHAR_DATA *ch, AFFECT_DATA *paf, bool fAdd ) );
bool	is_colcode	args( ( char code ) );
int	equip_char_values args( ( CHAR_DATA *ch, OBJ_DATA *obj, bool fReplace ) );
OBJ_DATA * get_bionic_ch args(( CHAR_DATA *ch, int bionic ));
int	determine_immunity args(( int dam_type ));

/* Whee, skill tree stuff, this one returns the UID
   Unused for now, this is the "easy way" */
int get_skill_tree( char *name )
{
 int skill;

 if ( name == '\0' )
 return -1;

 for ( skill = 0; skill < MAX_SKILL_TREE; skill += 1)
  if ( !str_cmp( skill_tree_table[skill].name, name ) )
   return skill;

 return -1;
}

 /* This s-tree function parses the names out and returns the last leaf UID */
 /* If you add levels of depth to the tree, you must read this and possibly
    recode it. I'm using easy workarounds so abbreviations in game are
    possible -Flux */
int parse_stree_leaf( char *fulltext )
{
  int skill;
  int parent_counter = 0;
  char name[MAX_INPUT_LENGTH];
  char parent[MAX_INPUT_LENGTH];
  char parent_parent[MAX_INPUT_LENGTH];
  int  parent_value = -1;
  int  parent_parent_value = -1;
  char *str;
  int i = 0;

  if ( *fulltext == '\0' )
   return -1;

  for ( str = fulltext; *str != '\0'; str++ )
  {
   name[i] = *str;

   if ( name[i] == '.' )
   {
    name[i] = '\0';

    if ( parent_counter >= 1 )
     sprintf( parent_parent, parent );

    sprintf( parent, name );

    i = 0;

    parent_counter++;
    continue;
   }

   i++;
  }

  if ( name[i] != '\0' )
   name[i] = '\0';

 if ( parent_parent[0] != '\0' )
  for ( skill = 0; skill < MAX_SKILL_TREE; skill += 1)
   if ( !str_prefix( parent_parent, skill_tree_table[skill].name ) )
   {
    parent_parent_value = skill;
    break;
   }

 if ( parent[0] != '\0' )
  for ( skill = 0; skill < MAX_SKILL_TREE; skill += 1)
   if ( !str_prefix( parent, skill_tree_table[skill].name )
         && skill_tree_table[skill].direct_parent == parent_parent_value )
   {
    parent_value = skill;
    break;
   }

 if ( name[0] != '\0' )
  for ( skill = 0; skill < MAX_SKILL_TREE; skill += 1)
   if ( !str_prefix( name, skill_tree_table[skill].name )
         && skill_tree_table[skill].direct_parent == parent_value )
    return skill;

 return -1;
}

 /* S-tree - returns the total cost of advancement for 1 level */
int get_skill_cost( CHAR_DATA *ch, CHAR_DATA *instructor, int skill )
{
 int cost;
 int skill_counter;
 int family_counter = 0;
 int discount = 1;
 int discipline = 0;

 if ( (instructor) != NULL )
 {
  discount = get_skill_mod(instructor, instructor->skill_tree[skill])
              / get_skill_mod(ch, ch->skill_tree[skill]);

  if ( discount > 2 )
   discount = 2;
 }

 for ( skill_counter = 0; skill_counter < MAX_SKILL_TREE; skill_counter++ )
 {
  if ( skill_counter == skill )
   continue;

  if ( skill_tree_table[skill_counter].family == -1 )
   if ( skill_tree_table[skill].family != -1
    && skill_tree_table[skill].family != skill_counter )
   {
    discipline += ch->skill_tree[skill_counter];
    family_counter++;
   }
 }
   
 discipline = (discipline / family_counter)
                - ch->skill_tree[skill_tree_table[skill].family];

 cost = (ch->skill_tree[skill]^2) * skill_tree_table[skill].training_cost;
 cost += discipline;
 cost /= discount;

 return cost;
}

/* returns the cost for multiple levels of advancement
   I seperated this "for" statement out to make the equations clearer
   in the single shot skill cost function -Flux */
int get_total_skill_cost( CHAR_DATA *ch, CHAR_DATA *instructor, int skill, 
                     int target_level )
{
 int cost = 0;
 int counter;

 if ( !(ch) || skill <= -1 || skill >= MAX_SKILL_TREE )
  return -1;

 counter = target_level - ch->skill_tree[skill];

 if ( counter <= 0 )
  return -1;

 for( ; counter > 0; counter-- )
  cost += get_skill_cost( ch, instructor, skill );

 return cost;
}

 /* This one determines the actual modifier used by the system based on 
      char stats -Flux */
int get_skill_mod( CHAR_DATA *ch, int skill )
{
 int mod;
 int str;
 int dex;
 int con;
 int wis;
 int agi;
 int intel;
 int charm;
 double chskill;

 if ( !ch || skill < 0 || skill >= MAX_SKILL_TREE )
  return -1;

 str = (get_curr_str(ch) * (skill_tree_table[skill].str_mod)) / 5;
 wis = (get_curr_wis(ch) * (skill_tree_table[skill].wis_mod)) / 5;
 intel = (get_curr_int(ch) * (skill_tree_table[skill].int_mod)) / 5;
 charm = (get_curr_cha(ch) * (skill_tree_table[skill].cha_mod)) / 5;
 dex = (get_curr_dex(ch) * (skill_tree_table[skill].dex_mod)) / 5;
 agi = (get_curr_agi(ch) * (skill_tree_table[skill].agi_mod)) / 5;
 con = (get_curr_con(ch) * (skill_tree_table[skill].con_mod)) / 5;

 chskill = sqrt(ch->skill_tree[skill]);

 mod = (str + wis + intel + charm + dex + agi + con) * chskill;

 return mod;
}

/* Determines if a skill can be advanced -Flux */
bool can_advance_skill( CHAR_DATA *ch, int skill )
{
 if ( !(ch) || skill < 0 || skill >= MAX_SKILL_TREE )
  return FALSE;

 if ( skill_tree_table[skill].direct_parent == -1 )
  return TRUE;

 if ( ch->skill_tree[skill_tree_table[skill].direct_parent] <
       skill_tree_table[skill].parent_req )
  return FALSE;

 return TRUE;
}


 /* Final skill tree function, actually advances the skills.
    It utilizes the next two functions as well.
    Progeny advances, well, progeny and retro advances parents. */
void advance_skill( CHAR_DATA *ch, int skill, int target_level )
{
 int skill_counter;
 int difference = target_level - ch->skill_tree[skill];

 ch->skill_tree[skill] += difference;

 for( skill_counter = 0; skill_counter < MAX_SKILL_TREE; skill_counter++ )
 {
  if ( skill_counter == skill )
   continue;

  if ( skill_tree_table[skill_counter].direct_parent == skill )
   advance_skill_progeny( ch, skill_counter, target_level );
 }

 retroadvance_skill( ch, skill );
 return;
}

void advance_skill_progeny( CHAR_DATA *ch, int skill, int target_level )
{
 int skill_counter;
 int difference = target_level - ch->skill_tree[skill];

 ch->skill_tree[skill] += difference;

 for( skill_counter = 0; skill_counter < MAX_SKILL_TREE; skill_counter++ )
 {
  if ( skill_counter == skill )
   continue;

  if ( skill_tree_table[skill_counter].direct_parent == skill )
   advance_skill_progeny( ch, skill_counter, target_level );
 }

 return;
}

void retroadvance_skill( CHAR_DATA *ch, int skill )
{
 int parent;
 int parent_advance_count = 0;
 double parent_advance = 0;
 int skill_counter;

 parent = skill_tree_table[skill].direct_parent;

 if ( parent == -1 )
  return;

  for( skill_counter = 0; skill_counter < MAX_SKILL_TREE; skill_counter++ )
   if ( skill_tree_table[skill_counter].direct_parent == parent )
   {
    parent_advance += ch->skill_tree[skill_counter];
    parent_advance_count++;
   }
 
  parent_advance /= parent_advance_count;

  ch->skill_tree[parent] = floor(parent_advance);

 if ( skill_tree_table[parent].direct_parent != -1 )
  retroadvance_skill( ch, parent );

 return;
}

bool can_use_skpell( CHAR_DATA *ch, int sn )
{
  return TRUE;
}

/* This returns the base retail value of an object based on way
   too many factors. This is what I do for consitency. -Flux */
MONEY_DATA *base_value( OBJ_DATA *obj )
{
  AFFECT_DATA *af;
  static MONEY_DATA base_cost;

    if ( !obj )
     return NULL;

    base_cost.gold = base_cost.silver = base_cost.copper = 0;

   /* First we check the basic values of the object */
  switch( obj->item_type )
  {
   default:	/* No value for nothing */
    break;      /* This is for stuff that shouldn't be sold really */

   case ITEM_WAND:
   case ITEM_LENSE:
   case ITEM_STAFF:
    base_cost.copper += 50;

    base_cost.silver += obj->value[0];

    if ( obj->value[1] == -1 )
     base_cost.gold += 5;
    else
    {
     base_cost.silver += (obj->value[1] * 2);
     base_cost.silver += obj->value[2];
    }

     if ( !str_cmp( skill_table[obj->value[3]].name, "sanctuary" ) )
       base_cost.gold += 5;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "golden aura" ) )
       base_cost.gold += 3;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "word of recall" ) )
       base_cost.gold += 1;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "temporal field" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "armor" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "shield" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "bless" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "fireshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "iceshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "shockshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "vibrate" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "chaosfield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "armor of thorns" ) )
       base_cost.gold += 2;

    if ( obj->value[3] == -1 )
    {
     base_cost.gold = 0; 
     base_cost.silver = 0;
     base_cost.copper = 0;
    }
    break;

    case ITEM_SCROLL:
    case ITEM_POTION:
    case ITEM_PILL:
     base_cost.copper += 15;

     base_cost.silver += obj->value[0];

     if ( !str_cmp( skill_table[obj->value[1]].name, "sanctuary" ) )
       base_cost.gold += 5;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "golden aura" ) )
       base_cost.gold += 3;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "word of recall" ) )
       base_cost.gold += 1;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "temporal field" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "armor" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "shield" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "bless" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "fireshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "iceshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "shockshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "vibrate" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "chaosfield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[1]].name, "armor of thorns" ) )
       base_cost.gold += 2;

     if ( !str_cmp( skill_table[obj->value[2]].name, "sanctuary" ) )
       base_cost.gold += 5;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "golden aura" ) )
       base_cost.gold += 3;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "word of recall" ) )
       base_cost.gold += 1;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "temporal field" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "armor" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "shield" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "bless" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "fireshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "iceshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "shockshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "vibrate" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "chaosfield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[2]].name, "armor of thorns" ) )
       base_cost.gold += 2;

     if ( !str_cmp( skill_table[obj->value[3]].name, "sanctuary" ) )
       base_cost.gold += 5;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "golden aura" ) )
       base_cost.gold += 3;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "word of recall" ) )
       base_cost.gold += 1;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "temporal field" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "armor" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "shield" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "bless" ) )
       base_cost.silver += 10;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "fireshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "iceshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "shockshield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "vibrate" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "chaosfield" ) )
       base_cost.gold += 2;
     else if ( !str_cmp( skill_table[obj->value[3]].name, "armor of thorns" ) )
       base_cost.gold += 2;

     if ( obj->value[1] == -1 && obj->value[2] == -1 && obj->value[3] == -1 )
     {
      base_cost.gold = 0;
      base_cost.silver = 0;
      base_cost.copper = 0;
     }

     break;

    case ITEM_BULLET:
     base_cost.copper += 15;

     base_cost.silver += obj->value[0];
     base_cost.copper += obj->value[1];
     break;

    case ITEM_ARROW:
     base_cost.copper += 5;

     base_cost.copper += (obj->value[0] * 2);
     base_cost.copper += obj->value[1];
     base_cost.silver += obj->value[2];
     break;

    case ITEM_CLIP:
     base_cost.silver += 5;

     base_cost.copper += (obj->value[0] * 2);
     base_cost.copper += obj->value[1];
     break;

    case ITEM_RANGED_WEAPON:
     base_cost.gold += 10;

     base_cost.silver += (obj->value[0] * 2);
     base_cost.copper += obj->value[1];

     if ( obj->value[0] == RANGED_WEAPON_FIREARM )
      base_cost.gold *= (obj->value[3] + 1);
     else if ( obj->value[0] == RANGED_WEAPON_BOW )
      base_cost.silver += obj->value[2];

     base_cost.silver += (obj->value[4] * 5);
     break;

    case ITEM_WEAPON:
     base_cost.gold += 2;

     base_cost.copper += (obj->value[1] * 3);
     base_cost.silver += obj->value[2];

     base_cost.copper += (obj->value[4] * 5);

     if ( obj->value[8] == WEAPON_BLADE )
      base_cost.gold += 1;

     base_cost.silver *= (obj->composition + 1);
     break;

    case ITEM_ORE:
     base_cost.gold += 1;
     base_cost.silver += 1;
     base_cost.copper += 1;

     base_cost.gold *= (obj->composition + 1);
     base_cost.silver *= (obj->composition + 1);
     base_cost.copper *= (obj->composition + 1);
     break;

    case ITEM_FURNITURE:
     if ( obj->value[1] == FURNITURE_CHAIR )
      base_cost.silver = 50;    
     else if ( obj->value[1] == FURNITURE_SOFA )
      base_cost.silver = 150;    
     else if ( obj->value[1] == FURNITURE_BED )
      base_cost.gold = 25;
     else if ( obj->value[1] == FURNITURE_DESK )
      base_cost.silver = 25;    
     else if ( obj->value[1] == FURNITURE_ARMOIR )
      base_cost.gold = 10;
     else
      base_cost.silver = 10;    
     break;

    case ITEM_ARMOR:
     base_cost.gold += 1;

     if ( obj->value[1] == TRUE )
      base_cost.silver += obj->value[2];

     base_cost.silver *= (obj->composition + 1);
     break;

    case ITEM_BOMB:
     base_cost.silver += 25;

     base_cost.silver *= (obj->value[1] + 1);
     base_cost.silver *= (obj->value[2] + 1);
     base_cost.copper *= obj->value[3];
     break;

    case ITEM_CONTAINER:
     base_cost.silver += 5;

     base_cost.copper += obj->value[0];

     if ( IS_SET( obj->value[1], CONT_CLOSEABLE ) )
      base_cost.silver += 7;
     break;

    case ITEM_DRINK_CON:
     base_cost.silver += 2;

     base_cost.copper += obj->value[0];

     if ( obj->value[3] == 1 )
      base_cost.silver = 0;
     break;

    case ITEM_FOOD:
     base_cost.silver += 1;

     base_cost.copper += obj->value[0];

     if ( obj->value[3] != FOOD_PURE )
      base_cost.silver = 0;
     break;

    case ITEM_CORPSE_NPC:
     base_cost.silver += 200;
     break;

    case ITEM_SKELETON:
    case ITEM_RIGOR:
    case ITEM_MUMMY:
     base_cost.silver += 50;
     break;

    case ITEM_TREASURE:
     base_cost.silver += 100;
     break;

    case ITEM_BOAT:
     base_cost.silver += 25;
     break;

    case ITEM_VODOO:
     base_cost.gold += 1;
     break;

    case ITEM_BERRY:
     base_cost.silver += 5;
     break;

    case ITEM_BODY_PART:
     base_cost.copper += 5;
     break;

    case ITEM_SCROLLPAPER:
    case ITEM_BEAKER:
    case ITEM_POSTALPAPER:
    case ITEM_LETTER:
    case ITEM_WICK:
    case ITEM_TIMER:
    case ITEM_PILEWIRE:
    case ITEM_TRIPWIRE:
    case ITEM_CHERRYCONTAINER:
     base_cost.copper += 15;
     break;

    case ITEM_PEN:
    case ITEM_INKCART:
     base_cost.copper += 30;
     break;

    case ITEM_CHEMSET:
    case ITEM_BUNSEN:
     base_cost.silver += 15;
     break;

    case ITEM_SMOKEBOMB:
    case ITEM_GUNPOWDER:
     base_cost.silver += 5;
     break;

    case ITEM_POISONCHEM:
    case ITEM_MOLOTOVCHEM:
    case ITEM_EMBALMING_FLUID:
     base_cost.silver += 25;
     break;

    case ITEM_TEARCHEM:
    case ITEM_PIPEBOMBCHEM:
    case ITEM_CHEMBOMBCHEM:
     base_cost.gold += 1;
     break;

    case ITEM_TOOLPACK:
    case ITEM_SMITHYPACK:
    case ITEM_TECHNOSOTTER:
     base_cost.gold += 50;
     break;
   }

  /* Now we calculate in any extra flags */
  if ( IS_SET( obj->extra_flags, ITEM_GLOW ) )
   base_cost.silver += 15;

  if ( IS_SET( obj->extra_flags, ITEM_NO_DAMAGE ) )
   base_cost.gold += 15;

  if ( obj->item_type == ITEM_WEAPON )
  {
   if ( IS_SET( obj->extra_flags, ITEM_SHARP ) )
    base_cost.silver += 2;
   if ( IS_SET( obj->extra_flags, ITEM_POISONED ) )
    base_cost.silver += 5;
   if ( IS_SET( obj->extra_flags, ITEM_WIRED ) )
    base_cost.silver += 25;
   if ( IS_SET( obj->extra_flags, ITEM_SOUL_BOUND ) )
    base_cost.silver += 50;
   if ( IS_SET( obj->extra_flags, ITEM_BALANCED ) )
    base_cost.silver += 2;
   if ( IS_SET( obj->extra_flags, ITEM_SHOCK ) )
    base_cost.silver += 5;
   if ( IS_SET( obj->extra_flags, ITEM_RAINBOW ) )
    base_cost.silver += 5;
   if ( IS_SET( obj->extra_flags, ITEM_FLAME ) )
    base_cost.silver += 5;
   if ( IS_SET( obj->extra_flags, ITEM_CHAOS ) )
    base_cost.silver += 5;
   if ( IS_SET( obj->extra_flags, ITEM_ICY ) )
    base_cost.silver += 5;
  }

  if ( IS_SET( obj->extra_flags, ITEM_NOEXIT ) )
  {
   base_cost.gold = 0;
   base_cost.silver = 0;
   base_cost.copper = 0;
  }  
  
  /* Add in values for affects */
  for ( af = obj->pIndexData->affected; af; af = af->next )
  {
   if ( !af )
    continue;
    
    switch ( af->location )
    {
     default:
      break;

     case APPLY_STR:
     case APPLY_DEX:
     case APPLY_INT:
     case APPLY_WIS:
     case APPLY_CON:
     case APPLY_AGI:
     case APPLY_CHA:
      base_cost.gold += af->modifier;
      break;

     case APPLY_MDAMP:
     case APPLY_PDAMP:
      base_cost.silver += (af->modifier * 5);
      break;

     case APPLY_SIZE:
      base_cost.silver += af->modifier;
      break;
     
     case APPLY_HIT:
     case APPLY_MANA:
      base_cost.copper += (af->modifier * 2);
      break;
     case APPLY_MOVE:
      base_cost.copper += af->modifier;
      break;

    case APPLY_HITROLL:
      base_cost.copper += (af->modifier * 4);
      break;

    case APPLY_DAMROLL:
      base_cost.silver += af->modifier;
      break;

    case APPLY_IMM_HEAT:
    case APPLY_IMM_POSITIVE:
    case APPLY_IMM_COLD:
    case APPLY_IMM_NEGATIVE:
    case APPLY_IMM_HOLY:
    case APPLY_IMM_UNHOLY:
    case APPLY_IMM_REGEN:
    case APPLY_IMM_DEGEN:
    case APPLY_IMM_DYNAMIC:
    case APPLY_IMM_VOID:
    case APPLY_IMM_PIERCE:
    case APPLY_IMM_SLASH:
    case APPLY_IMM_SCRATCH:
    case APPLY_IMM_BASH:
    case APPLY_IMM_INTERNAL:
     base_cost.copper -= af->modifier;
     break;

    case APPLY_INVISIBLE:
    case APPLY_HEIGHTEN_SENSES:
    case APPLY_PASS_DOOR:
    case APPLY_TEMPORAL_FIELD:
    case APPLY_FIRESHIELD:
    case APPLY_SHOCKSHIELD:
    case APPLY_ICESHIELD:
    case APPLY_CHAOS:
     base_cost.gold += 1;       
     break;

    case APPLY_DETECT_INVIS:
    case APPLY_DETECT_HIDDEN:
    case APPLY_INFRARED:
    case APPLY_SNEAK:
    case APPLY_HIDE:
    case APPLY_FLYING:
    case APPLY_BREATHE_WATER:
     base_cost.silver += 15;       
     break;
   }
  }

  /* Invokes */
  if ( obj->invoke_type == 5 && (obj->invoke_spell)
   && obj->invoke_spell != '\0' )
  {
   if ( obj->invoke_charge[0] == -1 && obj->invoke_charge[1] == -1 )
    base_cost.gold += 15;
   else
   {
    base_cost.copper += obj->invoke_charge[0] * 15;
    base_cost.silver += obj->invoke_charge[1] * 5;
   }

   if ( obj->invoke_charge[0] != 0 && obj->invoke_charge[1] != 0 )
   {
   if ( !str_cmp( obj->invoke_spell, "sanctuary" ) )
     base_cost.gold += 5;
   else if ( !str_cmp( obj->invoke_spell, "golden aura" ) )
     base_cost.gold += 3;
   else if ( !str_cmp( obj->invoke_spell, "word of recall" ) )
     base_cost.gold += 1;
   else if ( !str_cmp( obj->invoke_spell, "temporal field" ) )
     base_cost.gold += 2;
   else if ( !str_cmp( obj->invoke_spell, "armor" ) )
     base_cost.silver += 10;
   else if ( !str_cmp( obj->invoke_spell, "shield" ) )
     base_cost.silver += 10;
   else if ( !str_cmp( obj->invoke_spell, "bless" ) )
     base_cost.silver += 10;
   else if ( !str_cmp( obj->invoke_spell, "fireshield" ) )
     base_cost.gold += 2;
   else if ( !str_cmp( obj->invoke_spell, "iceshield" ) )
     base_cost.gold += 2;
   else if ( !str_cmp( obj->invoke_spell, "shockshield" ) )
     base_cost.gold += 2;
   else if ( !str_cmp( obj->invoke_spell, "vibrate" ) )
     base_cost.gold += 2;
   else if ( !str_cmp( obj->invoke_spell, "chaosfield" ) )
     base_cost.gold += 2;
   else if ( !str_cmp( obj->invoke_spell, "armor of thorns" ) )
     base_cost.gold += 2;
   }
  }

  /* Who wants broken objects? */
   base_cost.gold = ((base_cost.gold * obj->durability) / 100);
   base_cost.silver = ((base_cost.silver * obj->durability) / 100);
   base_cost.copper = ((base_cost.copper * obj->durability) / 100);


  /* Degrading objects aren't any good */
  if ( obj->timer >= 0 )
  {
   base_cost.gold = 0;
   base_cost.silver = 0;
   base_cost.copper = 0;
  }

  /* Filter for negative costs */  
  if ( base_cost.gold < 0 || base_cost.silver < 0 || base_cost.copper < 0 )
     return NULL;

  /* And we're done, finally -Flux */
  return &base_cost;
}

/* This converts weapon classes to damage types */
int weapon_to_damage( int damclass )
{
 if ( damclass == WEAPON_SLASH )
  return DAM_SLASH;
 else if ( damclass == WEAPON_PIERCE )
  return DAM_PIERCE;
 else if ( damclass == WEAPON_BASH )
  return DAM_BASH;
 else if ( damclass == WEAPON_TEAR )
  return DAM_SCRATCH;
 else if ( damclass == WEAPON_EXOTIC )
  return number_range( DAM_PIERCE, DAM_BASH );
 else if ( damclass == WEAPON_CHOP )
  return DAM_SLASH;
 else
  return number_range( DAM_PIERCE, DAM_BASH );
}


/* I decided to make this its own function to save trouble.
   It calculates the immunity modifier for PC's and mobs
   Making it easier to include an immunity filter -Flux */
int immune_calc( int dam_type, CHAR_DATA *ch )
{
 if ( IS_NPC(ch) )
 {
  int immunity_rating;
  
  if ( (immunity_rating = determine_immunity( dam_type )) == -99 )
   return 100;

  if ( IS_SET( ch->imm_flags, immunity_rating ) )
   return 0;
  else if ( IS_SET( ch->res_flags, immunity_rating ) )
   return 50;
  else if ( IS_SET( ch->vul_flags, immunity_rating ) )
   return 150;
  else
   return 100;
 }
 else
  return (ch->pcdata->pimm[dam_type] + ch->pcdata->mimm[dam_type]);

 return 100;
}

/* returns mob immunity values based on dam values */
int determine_immunity( int dam_type )
{
 if ( dam_type == DAM_HEAT )
  return IMM_HEAT;
 else if ( dam_type == DAM_POSITIVE )
  return IMM_POSITIVE;
 else if ( dam_type == DAM_COLD )
  return IMM_COLD;
 else if ( dam_type == DAM_NEGATIVE )
  return IMM_NEGATIVE;
 else if ( dam_type == DAM_HOLY )
  return IMM_HOLY;
 else if ( dam_type == DAM_UNHOLY )
  return IMM_UNHOLY;
 else if ( dam_type == DAM_REGEN )
  return IMM_REGEN;
 else if ( dam_type == DAM_DEGEN )
  return IMM_DEGEN;
 else if ( dam_type == DAM_DYNAMIC )
  return IMM_DYNAMIC;
 else if ( dam_type == DAM_VOID )
  return IMM_VOID;
 else if ( dam_type == DAM_PIERCE )
  return IMM_PIERCE;
 else if ( dam_type == DAM_SLASH )
  return IMM_SLASH;
 else if ( dam_type == DAM_SCRATCH )
  return IMM_SCRATCH;
 else if ( dam_type == DAM_BASH )
  return IMM_BASH;
 else if ( dam_type == DAM_INTERNAL )
  return IMM_INTERNAL;
 else
  return -99;
}

void damage_object( CHAR_DATA *ch, OBJ_DATA *obj, int damtype, int dam )
{
 char		 buf[MAX_STRING_LENGTH];
 
   dam = ((dam * 
       material_table[obj->composition].imm_damage[damtype]) / 100);

    if ( dam > 0 &&
     !IS_SET( obj->extra_flags, ITEM_NO_DAMAGE ) )
    {
     sprintf( buf, "Your %s has been damaged!\n\r", obj->short_descr );
     send_to_char( AT_WHITE, buf, ch );
     dam = ( dam / 5 );
     item_damage( dam, obj, ch );
    }

 return;
}

/* Handles what gasses actually do to people on update */
void gasaffect( OBJ_DATA *gas )
{
 ROOM_INDEX_DATA *in_room;
 AFFECT_DATA af;
 CHAR_DATA *victim;

 if ( !(in_room = gas->in_room ) )
  return;

 for ( victim = in_room->people; victim; victim = victim->next_in_room )
 {
  if ( !victim || victim->deleted || victim->position == POS_DEAD )
   continue;

  if ( !IS_NPC(victim) && victim->level < LEVEL_IMMORTAL )
   if ( is_safe( victim, victim ) )
    return;

  act( C_DEFAULT, "$p enters your lungs.", victim, gas, NULL,
   TO_CHAR );

  if ( gas->value[2] > 0 )
   damage( victim, victim, gas->value[2], gsn_wrack, gas->value[1], TRUE );

 /* Yeah so this is a direct copy of my food taint code,
    but it works, and I wrote the code, so get off my back -Flux */
  switch( gas->value[3] )
  {
   default: break;
   case GAS_AFFECT_POISONED:
    if ( !IS_AFFECTED( victim, AFF_POISON ) )
    {
     af.type      = gsn_poison;
     af.level     = gas->value[0];
     af.duration  = gas->value[0];
     af.location  = APPLY_STR;
     af.modifier  = -2;
     af.bitvector = AFF_POISON;
     affect_to_char( victim, &af );

     act(AT_GREEN, "$n chokes and gags.", victim, 0, 0, TO_ROOM );
     send_to_char(AT_GREEN, "You choke and gag.\n\r", victim );
    }
   break;

   case GAS_AFFECT_DISEASED:
    if ( !IS_AFFECTED2( victim, AFF_DISEASED ) )
    {
     act(AT_GREEN, "$n chokes and gags.", victim, 0, 0, TO_ROOM );
     send_to_char(AT_GREEN, "You choke and gag.\n\r", victim );

     af.type      = gsn_plague;
     af.level     = gas->value[0];
     af.duration  = gas->value[0];
     af.location  = APPLY_CON;
     af.modifier  = -2;
     af.bitvector = AFF_DISEASED;
     affect_to_char2( victim, &af );
    }
   break;

   case GAS_AFFECT_INSANE:
    if ( !IS_AFFECTED( victim, AFF_INSANE ) )
    {
     act(AT_GREEN, "$n gets a strange look in $s eyes.",
      victim, 0, 0, TO_ROOM );
     send_to_char(AT_GREEN, "You begin to feel 'strange'.\n\r", victim );

     af.type      = gsn_insane;
     af.level     = gas->value[0];
     af.duration  = gas->value[0];
     af.location  = APPLY_INT;
     af.modifier  = -2;
     af.bitvector = AFF_INSANE;
     affect_to_char( victim, &af );
    }
   break;

   case GAS_AFFECT_HALLUCINATORY:
    if ( !IS_AFFECTED2( victim, AFF_HALLUCINATING ) )
    {
     act(AT_GREEN, "$n gets a strange look in $s eyes.",
      victim, 0, 0, TO_ROOM );
     send_to_char(AT_GREEN, "You begin to feel 'strange'.\n\r", victim );
 
     af.type      = gsn_hallucinate;
     af.level     = gas->value[0];
     af.duration  = gas->value[0];
     af.location  = APPLY_INT;
     af.modifier  = -2;
     af.bitvector = AFF_HALLUCINATING;
     affect_to_char2( victim, &af );
    }
   break;
   case GAS_AFFECT_STUN:
   if ( !IS_STUNNED( victim, STUN_TOTAL ) )
    STUN_CHAR( victim, number_range( 5, 10 ), STUN_TOTAL );
   break;
  }
 }
 return;
}

/* Spreads gasses for the gas item_type. More info in update.c */
void gas_spread( OBJ_DATA *gas )
{
 ROOM_INDEX_DATA *original_room;
 ROOM_INDEX_DATA *to_room;
 EXIT_DATA	 *pexit;
 OBJ_DATA	 *isgas;
 char		 buf[MAX_STRING_LENGTH];
 bool		 yesgas = FALSE;
 bool		 noobj = TRUE;
 int		 dir;

 if ( !(original_room = gas->in_room) )
  return;

 if ( !gas )
  return;

    for (dir = 0; dir < 6; dir++) /* look in every direction */
    {
     pexit = original_room->exit[dir];

     if ((pexit == NULL) || (pexit->to_room == NULL) ||
	 (exit_blocked(pexit, original_room) > EXIT_STATUS_OPEN))
      continue;

     to_room = pexit->to_room;

     if ( original_room->area->temporal != to_room->area->temporal )
      continue;

     if ( !(IS_SET( to_room->room_flags, ROOM_SAFE )) )
     {
      noobj = TRUE;

      for ( isgas = to_room->contents; isgas; isgas = isgas->next_content )
      {
	if ( !isgas || isgas->deleted )
	    continue;

        noobj = FALSE;

	if ( isgas->item_type == gas->item_type )
         yesgas = TRUE;
      }
      
      if ( !yesgas || noobj )
      {
       OBJ_DATA *new_gas =
        create_object( get_obj_index( OBJ_VNUM_GAS_CLOUD ), 0 );

       new_gas->timer = gas->timer - 1;

       if ( new_gas->timer < 0 )
        new_gas->timer = 0;
    
       sprintf( buf, gas->short_descr );
       free_string( new_gas->short_descr );
       new_gas->short_descr = str_dup( buf );

       sprintf( buf, gas->description );
       free_string( new_gas->description );
       new_gas->description = str_dup( buf );

       sprintf( buf, gas->name );
       free_string( new_gas->name );
       new_gas->name = str_dup( buf );

       new_gas->level = gas->level;
       new_gas->value[0] = gas->value[0];
       new_gas->value[1] = gas->value[1];
       new_gas->value[2] = gas->value[2];
       new_gas->value[3] = gas->value[3];

       obj_to_room( new_gas, to_room );
       act( C_DEFAULT, "$p slowly creeps into the room, filling it.",
        NULL, new_gas, NULL, TO_ROOM );
      }
     }
    }

 return;
}

/* Returns a value that shows the status of an exit -Flux */
int exit_blocked( EXIT_DATA *exit, ROOM_INDEX_DATA *in_room )
{
 OBJ_DATA *obj;

  /* This is for sphere_of_solitude -Flux */
  for ( obj = exit->to_room->contents; obj; obj = obj->next_content )
  {
   if ( !obj || obj->deleted )
    continue;

   if ( obj->pIndexData->vnum == OBJ_VNUM_SPHERE_OF_SOLITUDE )
    return EXIT_STATUS_MAGICAL;
  }

  for ( obj = in_room->contents; obj; obj = obj->next_content )
  {
   if ( !obj || obj->deleted )
    continue;

   if ( obj->pIndexData->vnum == OBJ_VNUM_SPHERE_OF_SOLITUDE )
    return EXIT_STATUS_MAGICAL;
  }

  if ( is_exit_affected( exit, EXIT_FORCEFIELD ) )
   return EXIT_STATUS_MAGICAL;

  if ( IS_SET( exit->exit_info, EX_CLOSED ) )
   return EXIT_STATUS_PHYSICAL;

  if ( exit->to_room->accumulation >= 120 && exit->to_room->temp <= 32 )
   return EXIT_STATUS_SNOWIN;

  if ( in_room->accumulation >= 120 && in_room->temp <= 32 )
   return EXIT_STATUS_SNOWIN;

 return EXIT_STATUS_OPEN;
}

/* Returns the distance between two chars, or -1 if there's
   no straight-line route -Flux */
int distancebetween( CHAR_DATA *ch, CHAR_DATA *victim, int dir )
{
 int dist;
 EXIT_DATA *pexit;
 ROOM_INDEX_DATA *next_room;

 if ( !victim )
  return -1;

 if ( !victim->in_room )
  return -1;

 next_room = ch->in_room;

 if ( !next_room )
  return -1;

 for ( dist = 0; dist <= 32000; dist++ )
 {
  if ( victim->in_room == next_room )
   return dist;

  if ( !( pexit = next_room->exit[dir] ) 
   || !( next_room = pexit->to_room ) )
    break;
 }

 return -1;
}

void weather_magic( int type, ROOM_INDEX_DATA *pRoom )
{
 if ( type == DAM_HEAT )
  pRoom->temp++;
 else if ( type == DAM_COLD )
  pRoom->temp--;

 return;
}

bool outdoor_check( ROOM_INDEX_DATA *pRoom )
{
 if ( !(pRoom) )
  return FALSE;

 if ( !IS_SET( pRoom->room_flags, ROOM_INDOORS )
  && !IS_SET( pRoom->room_flags, ROOM_UNDERGROUND )
  && !IS_SET( pRoom->room_flags, ROOM_VOID ) )
  return TRUE;

 return FALSE;
}

int rsector_check( ROOM_INDEX_DATA *pRoom )
{
 int	weather_state = WEATHER_CLEAR;
 int	sector = pRoom->sector_type;

/* These never change */
 if ( pRoom->sector_type == SECT_ASTRAL ||
      pRoom->sector_type == SECT_SHADOW ||
      pRoom->sector_type == SECT_UNDERWATER ||
      pRoom->sector_type == SECT_WATER_SURFACE )
  return sector;

 if ( pRoom->humidity <= 35 )
 {
  if ( pRoom->temp > 32 )
   weather_state = WEATHER_RAIN;
  else
   weather_state = WEATHER_SNOW;
 }

 if ( pRoom->accumulation > 120 && pRoom->temp > 32 )
  return SECT_WATER_SURFACE;

 if ( pRoom->accumulation > 60 && pRoom->temp > 32 )
  return SECT_SWAMP;

 if ( pRoom->temp < 0 )
  return SECT_ARCTIC;

 if ( pRoom->temp > 125 )
  return SECT_DESERT;

 return sector;
}

int get_weather( ROOM_INDEX_DATA *pRoom )
{
 int	weather_state = WEATHER_CLEAR;

 if ( pRoom->humidity <= 35 )
 {
  if ( pRoom->temp > 32 )
   weather_state = WEATHER_RAIN;
  else
   weather_state = WEATHER_SNOW;
 }

 return weather_state;
}

int get_clouds( ROOM_INDEX_DATA *pRoom )
{
 int	cloud_state = SKY_CLEAR;

 if ( pRoom->humidity <= 75 )
  cloud_state = SKY_PARTIAL;
 if ( pRoom->humidity <= 50 )
  cloud_state = SKY_CLOUDY;
 if ( pRoom->humidity <= 25 )
  cloud_state = SKY_OVERCAST;

 return cloud_state;
}

/* This is also for assimilate, it is used with the status command */
char *bodypartdesc( int partnum )
{
 char *buf = "Contact the administration, you are bugged.";

 switch( partnum )
 {
  default:
   buf = "Contact the administration, you are bugged.";
  break;

  case OBJ_VNUM_SLICED_RIGHT_ARM:
   buf = "A humanoid arm";
  break;

  case OBJ_VNUM_SLICED_LEFT_ARM:
   buf = "A humanoid arm";
  break;

  case OBJ_VNUM_SLICED_RIGHT_LEG:
   buf = "A humanoid leg";
  break;

  case OBJ_VNUM_SLICED_LEFT_LEG:
   buf = "A humanoid leg";
  break;

  case OBJ_VNUM_LOBSTER_CLAW:
   buf = "A large pincer-claw";
  break;

  case OBJ_VNUM_TAIL:
   buf = "A tail";
  break;

  case OBJ_VNUM_WING:
   buf = "An expansive set of wings";
  break;

  case OBJ_VNUM_BIRD_CLAW:
   buf = "A thin clawed hand";
  break;

  case OBJ_VNUM_INSECT_ABDOMEN:
   buf = "An arachnid-like abdomen";
  break;

  case OBJ_VNUM_INSECT_WINGS:
   buf = "Tiny insect wings";
  break;

  case OBJ_VNUM_THIN_LEG:
   buf = "An insect-like leg";
  break;

  case OBJ_VNUM_SNAKE_SKIN:
   buf = "Leathery tough skin";
  break;
 }

 return buf;
}

bool is_flying( CHAR_DATA *ch )
{
 if ( IS_AFFECTED( ch, AFF_FLYING ) )
  return TRUE;

 if ( IS_NPC(ch) )
  return FALSE;

 if ((get_race_data(ch->race))->flying == 1 )
  return TRUE;
 
 if ( ch->pcdata->assimilate[ASSIM_EXTRA_1] == OBJ_VNUM_WING ||
  ch->pcdata->assimilate[ASSIM_EXTRA_2] == OBJ_VNUM_WING ||
  ch->pcdata->assimilate[ASSIM_EXTRA_3] == OBJ_VNUM_WING ||
  ch->pcdata->assimilate[ASSIM_EXTRA_4] == OBJ_VNUM_WING ||
  ch->pcdata->assimilate[ASSIM_EXTRA_5] == OBJ_VNUM_WING )
  return TRUE;

 return FALSE;
}

/*
 * This is for maudlin code, it checks if the selected
 * body part actually can go with the selected location.
 * IE: lobster claw cant be added as a torso, but it can be as
 * an arm. -Flux
 * Granted, this is the long way of going about this, but
 * if you really want to change it, go ahead.
 */
bool assimilate_vnum_check( int location, int part )
{
 switch( location )
 {
  case ASSIM_TORSO:
   if ( part == OBJ_VNUM_INSECT_ABDOMEN )
    return TRUE;
  break;

  case ASSIM_RARM:
  case ASSIM_LARM:
   if ( part == OBJ_VNUM_SLICED_RIGHT_ARM 
     || part == OBJ_VNUM_SLICED_LEFT_ARM
     || part == OBJ_VNUM_LOBSTER_CLAW
     || part == OBJ_VNUM_BIRD_CLAW )
    return TRUE;
  break;

  case ASSIM_RLEG:
  case ASSIM_LLEG:
   if ( part == OBJ_VNUM_SLICED_RIGHT_LEG 
     || part == OBJ_VNUM_SLICED_LEFT_LEG
     || part == OBJ_VNUM_THIN_LEG )
    return TRUE;
  break;

  case ASSIM_EXTRA_1:
  case ASSIM_EXTRA_2:
  case ASSIM_EXTRA_3:
  case ASSIM_EXTRA_4:
  case ASSIM_EXTRA_5:
   if ( part == OBJ_VNUM_TAIL ||
        part == OBJ_VNUM_WING ||
        part == OBJ_VNUM_SNAKE_SKIN )
    return TRUE;
  break;
 }

 return FALSE;
}

/*
 * This function actually assimilates the part for the maudlin
 * racial skill assimilate. -Flux.
 * It is interesting to note that there are and should be
 * no addaffects associated with the assimilation process.
 * That is, unless you are really wanting to create a code hassle
 * for yourself...
 */
void assimilate_part( CHAR_DATA *ch, OBJ_DATA *part, int location )
{
 char buf[MAX_STRING_LENGTH];
 int  partnum = part->pIndexData->vnum;

 if ( (location == ASSIM_RARM || location == ASSIM_LARM) )
 {
  if ( partnum == OBJ_VNUM_BIRD_CLAW )
   ch->pcdata->claws = CLAW_NORMAL;
  else
  if ( ch->pcdata->assimilate[ASSIM_RARM] == OBJ_VNUM_BIRD_CLAW &&
       ch->pcdata->assimilate[ASSIM_LARM] == OBJ_VNUM_BIRD_CLAW )
   ;
  else
  if ( ch->pcdata->assimilate[ASSIM_RARM] != OBJ_VNUM_BIRD_CLAW &&
       ch->pcdata->assimilate[ASSIM_LARM] != OBJ_VNUM_BIRD_CLAW )
   ;
  else
  if ( ch->pcdata->assimilate[ASSIM_RARM] == OBJ_VNUM_BIRD_CLAW ||
       ch->pcdata->assimilate[ASSIM_LARM] == OBJ_VNUM_BIRD_CLAW )
   if ( ch->pcdata->assimilate[location] == partnum )
    ch->pcdata->claws = CLAW_NONE;
 }

 ch->pcdata->assimilate[location] = partnum;

 act( AT_WHITE,
  "You grasp $p and assimilate it's DNA patterns, adding them to your own.",
   ch, part, NULL, TO_CHAR );

 act( AT_YELLOW,
  "$n grasps $p and seems to absorb it.", ch, part, NULL, TO_ROOM );

 ch->hit += 15;

 switch( partnum )
 {
  case OBJ_VNUM_SLICED_RIGHT_ARM:
  case OBJ_VNUM_SLICED_LEFT_ARM:
  case OBJ_VNUM_SLICED_LEFT_LEG:
  case OBJ_VNUM_SLICED_RIGHT_LEG:
   sprintf( buf, "You feel more normal.\n\r" );
  break;

  case OBJ_VNUM_LOBSTER_CLAW:
   sprintf( buf, "You feel stronger and have an overwhelming desire to &Gcrush&X something, or someone...\n\r" );
  break;

  case OBJ_VNUM_TAIL:
   sprintf( buf, "You feel lighter on your feet and seem to have found an increased sense of balance.\n\r" );
  break;

  case OBJ_VNUM_WING:
   sprintf( buf, "You feel the need to take flight!\n\r" );
  break;

  case OBJ_VNUM_BIRD_CLAW:
   sprintf( buf, "You're new claws appear sharp and fearsome!\n\r" );
  break;

  case OBJ_VNUM_INSECT_ABDOMEN:
   sprintf( buf, "You feel the need to &Gwebspin&X.\n\r" );
  break;

  case OBJ_VNUM_THIN_LEG:
   sprintf( buf, "You feel more agile.\n\r" );
  break;

  case OBJ_VNUM_SNAKE_SKIN:
   sprintf( buf, "You feel more protected.\n\r" );
  break;
 }
 send_to_char( AT_WHITE, buf, ch );

 return;
}

bool is_wearable( OBJ_DATA *obj )
{
   switch ( obj->item_type )
   {
    default:
    case ITEM_SKELETON:
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
    case ITEM_CLIP: 
    case ITEM_BULLET: 
    case ITEM_SCROLL:
    case ITEM_CORPSE_NPC:
    case ITEM_PORTAL:
    case ITEM_POTION:
    case ITEM_PILL:
    case ITEM_MONEY:
    case ITEM_BOMB:
    case ITEM_GAS_CLOUD:
    case ITEM_MUMMY:
    case ITEM_ARROW:
    case ITEM_EMBALMING_FLUID:
     return FALSE;

    case ITEM_ARMOR:
    case ITEM_WAND:
    case ITEM_LENSE:
    case ITEM_STAFF:
    case ITEM_WEAPON:
    case ITEM_RANGED_WEAPON: 
     return TRUE;
   }

 return FALSE;
}

int get_trust( CHAR_DATA *ch )
{
    if ( ch->desc && ch->desc->original )
	ch = ch->desc->original;

    if ( ch->trust != 0 )
	return ch->trust;

    if ( IS_NPC( ch ) && ch->level >= LEVEL_HERO )
	return LEVEL_HERO - 1;
    else
	return ch->level;
}

OBJ_DATA *get_bionic_ch( CHAR_DATA *ch, int bionic )
{
    OBJ_DATA *obj;
    for( obj = ch->carrying; obj; obj = obj->next_content )
    {
   if ( !obj || obj->deleted )
          continue;
       if ( obj->bionic_loc == bionic )
          return obj;
    }
   return NULL;
}

/* shows the skill tree values. The integer "skill" determines if the
   tree is truncated or not. People don't always want to view that
   monster thing. -Flux */
void list_skill_tree( CHAR_DATA *ch, CHAR_DATA *showto, int skill )
{
 char buf[MAX_STRING_LENGTH];

 if ( skill == 0 )
 {
  sprintf( buf, "&WCraftsmanship&w:&z [&C%4d&z|&G%4d&z]\n\r"
                "&G-Tailoring&w:&z    [&C%4d&z|&G%4d&z]\n\r"
                "&G-Jewelery&w:&z     [&C%4d&z|&G%4d&z]\n\r"
                "&G-Mining&w:&z       [&C%4d&z|&G%4d&z]\n\r"
                "&G-Carpentry&w:&z    [&C%4d&z|&G%4d&z]\n\r"
                "&G-Smithing&w:&z     [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Weapons&w:&z     [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Armor&w:&z       [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Locks&w:&z       [&C%4d&z|&G%4d&z]\n\r",
   ch->skill_tree[0], get_skill_mod( ch, 0 ),
   ch->skill_tree[1], get_skill_mod( ch, 1 ),
   ch->skill_tree[2], get_skill_mod( ch, 2 ),
   ch->skill_tree[3], get_skill_mod( ch, 3 ),
   ch->skill_tree[4], get_skill_mod( ch, 4 ),
   ch->skill_tree[5], get_skill_mod( ch, 5 ),
   ch->skill_tree[6], get_skill_mod( ch, 6 ),
   ch->skill_tree[7], get_skill_mod( ch, 7 ),
   ch->skill_tree[8], get_skill_mod( ch, 8 ) );
  editor_send_to_char( AT_BLUE, buf, showto );
 }
 else if ( skill == 9 )
 {
  sprintf( buf, "&WEvaluation&w:&z [&C%4d&z|&G%4d&z]\n\r"
                "&G-Medical&w:&z   [&C%4d&z|&G%4d&z]\n\r"
                "&G-Weapons&w:&z   [&C%4d&z|&G%4d&z]\n\r"
                "&G-Armor&w:&z     [&C%4d&z|&G%4d&z]\n\r"
                "&G-Jewelery&w:&z  [&C%4d&z|&G%4d&z]\n\r"
                "&G-Tactical&w:&z  [&C%4d&z|&G%4d&z]\n\r",
   ch->skill_tree[9], get_skill_mod( ch, 9 ),
   ch->skill_tree[10], get_skill_mod( ch, 10 ),
   ch->skill_tree[11], get_skill_mod( ch, 11 ),
   ch->skill_tree[12], get_skill_mod( ch, 12 ),
   ch->skill_tree[13], get_skill_mod( ch, 13 ),
   ch->skill_tree[14], get_skill_mod( ch, 14 ) );
  editor_send_to_char( AT_BLUE, buf, showto );
 }
 else if ( skill == 15 )
 {
  sprintf( buf, "&WTechnology&w:&z   [&C%4d&z|&G%4d&z]\n\r"
                "&G-Weapons&w:&z     [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Firearms&w:&z   [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Explosives&w:&z [&C%4d&z|&G%4d&z]\n\r"
                "&G-Power&w:&z       [&C%4d&z|&G%4d&z]\n\r"
                "&G-Circuitry&w:&z   [&C%4d&z|&G%4d&z]\n\r"
                "&G-Automation&w:&z  [&C%4d&z|&G%4d&z]\n\r"
                "&G-Energy&w:&z      [&C%4d&z|&G%4d&z]\n\r"
                "&G-Biological&w:&z  [&C%4d&z|&G%4d&z]\n\r"
                "&G-Chemistry&w:&z   [&C%4d&z|&G%4d&z]\n\r",
   ch->skill_tree[15], get_skill_mod( ch, 15 ),
   ch->skill_tree[16], get_skill_mod( ch, 16 ),
   ch->skill_tree[17], get_skill_mod( ch, 17 ),
   ch->skill_tree[18], get_skill_mod( ch, 18 ),
   ch->skill_tree[19], get_skill_mod( ch, 19 ),
   ch->skill_tree[20], get_skill_mod( ch, 20 ),
   ch->skill_tree[21], get_skill_mod( ch, 21 ),
   ch->skill_tree[22], get_skill_mod( ch, 22 ),
   ch->skill_tree[23], get_skill_mod( ch, 23 ),
   ch->skill_tree[24], get_skill_mod( ch, 24 ) );
  editor_send_to_char( AT_BLUE, buf, showto );
 }
 else if ( skill == 25 )
 {
  sprintf( buf, "&WCovert&w:&z       [&C%4d&z|&G%4d&z]\n\r"
                "&G-Stealth&w:&z     [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Sneaking&w:&z   [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Hiding&w:&z     [&C%4d&z|&G%4d&z]\n\r"
                "&G-Theft&w:&z       [&C%4d&z|&G%4d&z]\n\r"
                "&G-Deception&w:&z   [&C%4d&z|&G%4d&z]\n\r"
                "&G-Perception&w:&z  [&C%4d&z|&G%4d&z]\n\r"
                "&G-Trapping&w:&z    [&C%4d&z|&G%4d&z]\n\r",
   ch->skill_tree[25], get_skill_mod( ch, 25 ),
   ch->skill_tree[26], get_skill_mod( ch, 26 ),
   ch->skill_tree[27], get_skill_mod( ch, 27 ),
   ch->skill_tree[28], get_skill_mod( ch, 28 ),
   ch->skill_tree[29], get_skill_mod( ch, 29 ),
   ch->skill_tree[30], get_skill_mod( ch, 30 ),
   ch->skill_tree[31], get_skill_mod( ch, 31 ),
   ch->skill_tree[32], get_skill_mod( ch, 32 ) );
  editor_send_to_char( AT_BLUE, buf, showto );
 }
 else if ( skill == 33 )
 {
  sprintf( buf, "&WPhysical&w:&z          [&C%4d&z|&G%4d&z]\n\r"
                "&G-Medicine&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-Athletics&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Swimming&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Climbing&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Riding&w:&z          [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Body Building&w:&z   [&C%4d&z|&G%4d&z]\n\r"
                "&G-Combat&w:&z           [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Ambidexterity&w:&z   [&C%4d&z|&G%4d&z]    "
                "&G-&B-Martial Arts&w:&z    [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Weapons&w:&z         [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-Offensive&w:&z      [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Slash&w:&z          [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-Defensive&w:&z      [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Pierce&w:&z         [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-Disciplines&w:&z    [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Bash&w:&z           [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Crane&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Tear&w:&z           [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Viper&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Chop&w:&z           [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Monkey&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Long&w:&z           [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Bull&w:&z          [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Medium&w:&z         [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Tiger&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Short&w:&z          [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Dragon&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Offensive&w:&z       [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Panther&w:&z       [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Unarmed&w:&z        [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Sparrow&w:&z       [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Weapons&w:&z        [&C%4d&z|&G%4d&z]    "
                "&G-&B-Ranged&w:&z          [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Defensive&w:&z       [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-Archery&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Unarmed&w:&z        [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-Thrown&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Weapons&w:&z        [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Short&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Dodge&w:&z          [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-&R-Medium&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "                                  "
                "&G-&B-&Y-&R-Long&w:&z          [&C%4d&z|&G%4d&z]\n\r",
   ch->skill_tree[33], get_skill_mod( ch, 33 ),
   ch->skill_tree[34], get_skill_mod( ch, 34 ),
   ch->skill_tree[35], get_skill_mod( ch, 35 ),
   ch->skill_tree[36], get_skill_mod( ch, 36 ),
   ch->skill_tree[37], get_skill_mod( ch, 37 ),
   ch->skill_tree[38], get_skill_mod( ch, 38 ),
   ch->skill_tree[39], get_skill_mod( ch, 39 ),
   ch->skill_tree[40], get_skill_mod( ch, 40 ),
   ch->skill_tree[41], get_skill_mod( ch, 41 ),
   ch->skill_tree[58], get_skill_mod( ch, 58 ),
   ch->skill_tree[42], get_skill_mod( ch, 42 ),
   ch->skill_tree[59], get_skill_mod( ch, 59 ),
   ch->skill_tree[43], get_skill_mod( ch, 43 ),
   ch->skill_tree[60], get_skill_mod( ch, 60 ),
   ch->skill_tree[44], get_skill_mod( ch, 44 ),
   ch->skill_tree[61], get_skill_mod( ch, 61 ),
   ch->skill_tree[45], get_skill_mod( ch, 45 ),
   ch->skill_tree[62], get_skill_mod( ch, 62 ),
   ch->skill_tree[46], get_skill_mod( ch, 46 ),
   ch->skill_tree[63], get_skill_mod( ch, 63 ),
   ch->skill_tree[47], get_skill_mod( ch, 47 ),
   ch->skill_tree[64], get_skill_mod( ch, 64 ),
   ch->skill_tree[48], get_skill_mod( ch, 48 ),
   ch->skill_tree[65], get_skill_mod( ch, 65 ),
   ch->skill_tree[49], get_skill_mod( ch, 49 ),
   ch->skill_tree[66], get_skill_mod( ch, 66 ),
   ch->skill_tree[50], get_skill_mod( ch, 50 ),
   ch->skill_tree[67], get_skill_mod( ch, 67 ),
   ch->skill_tree[51], get_skill_mod( ch, 51 ),
   ch->skill_tree[68], get_skill_mod( ch, 68 ),
   ch->skill_tree[52], get_skill_mod( ch, 52 ),
   ch->skill_tree[69], get_skill_mod( ch, 69 ),
   ch->skill_tree[53], get_skill_mod( ch, 53 ),
   ch->skill_tree[70], get_skill_mod( ch, 70 ),
   ch->skill_tree[54], get_skill_mod( ch, 54 ),
   ch->skill_tree[71], get_skill_mod( ch, 71 ),
   ch->skill_tree[55], get_skill_mod( ch, 55 ),
   ch->skill_tree[72], get_skill_mod( ch, 72 ),
   ch->skill_tree[56], get_skill_mod( ch, 56 ),
   ch->skill_tree[73], get_skill_mod( ch, 73 ),
   ch->skill_tree[57], get_skill_mod( ch, 57 ),
   ch->skill_tree[74], get_skill_mod( ch, 74 ),
   ch->skill_tree[75], get_skill_mod( ch, 75 ) );
  editor_send_to_char( AT_BLUE, buf, showto );
 }
 else if ( skill == 76 )
 {
  sprintf( buf, "&WMagic&w:&z             [&C%4d&z|&G%4d&z]\n\r"
                "&G-Voodoo&w:&z           [&C%4d&z|&G%4d&z]    "
                "&G-Augmentation&w:&z     [&C%4d&z|&G%4d&z]\n\r"
                "&G-Prestidigitation&w:&z [&C%4d&z|&G%4d&z]    "
                "&G-&B-Enhancement&w:&z     [&C%4d&z|&G%4d&z]\n\r"
                "&G-Spellcraft&w:&z       [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-Energy&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-Temporal&w:&z         [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-Physical&w:&z       [&C%4d&z|&G%4d&z]\n\r"
                "&G-Alchemy&w:&z          [&C%4d&z|&G%4d&z]    "
                "&G-&B-Cursing&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-Perception&w:&z       [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-Energy&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-Arcane&w:&z           [&C%4d&z|&G%4d&z]    "
                "&G-&B-&Y-Physical&w:&z       [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Recite&w:&z          [&C%4d&z|&G%4d&z]    "
                "&G-Elemental&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Invoke&w:&z          [&C%4d&z|&G%4d&z]    "
                "&G-&B-Heat&w:&z            [&C%4d&z|&G%4d&z]\n\r"
                "&G-Artification&w:&z     [&C%4d&z|&G%4d&z]    "
                "&G-&B-Cold&w:&z            [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Enchantment&w:&z     [&C%4d&z|&G%4d&z]    "
                "&G-&B-Negative&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Crafting&w:&z        [&C%4d&z|&G%4d&z]    "
                "&G-&B-Positive&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Imbuing&w:&z         [&C%4d&z|&G%4d&z]    "
                "&G-&B-Holy&w:&z            [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Scroll&w:&z         [&C%4d&z|&G%4d&z]    "
                "&G-&B-Unholy&w:&z          [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Wand&w:&z           [&C%4d&z|&G%4d&z]    "
                "&G-&B-Regenerative&w:&z    [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-&Y-Staff&w:&z          [&C%4d&z|&G%4d&z]    "
                "&G-&B-Degenerative&w:&z    [&C%4d&z|&G%4d&z]\n\r"
                "&G-Natural&w:&z          [&C%4d&z|&G%4d&z]    "
                "&G-&B-Dynamic&w:&z         [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Herbal&w:&z          [&C%4d&z|&G%4d&z]    "
                "&G-&B-Void&w:&z            [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Ecomancy&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-Transformation&w:&z   [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Mythical&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&G-&B-Natural&w:&z         [&C%4d&z|&G%4d&z]\n\r",
   ch->skill_tree[76], get_skill_mod( ch, 76 ),
   ch->skill_tree[77], get_skill_mod( ch, 77 ),
   ch->skill_tree[96], get_skill_mod( ch, 96 ),
   ch->skill_tree[78], get_skill_mod( ch, 78 ),
   ch->skill_tree[97], get_skill_mod( ch, 97 ),
   ch->skill_tree[79], get_skill_mod( ch, 79 ),
   ch->skill_tree[98], get_skill_mod( ch, 98 ),
   ch->skill_tree[80], get_skill_mod( ch, 80 ),
   ch->skill_tree[99], get_skill_mod( ch, 99 ),
   ch->skill_tree[81], get_skill_mod( ch, 81 ),
   ch->skill_tree[100], get_skill_mod( ch, 100 ),
   ch->skill_tree[82], get_skill_mod( ch, 82 ),
   ch->skill_tree[101], get_skill_mod( ch, 101 ),
   ch->skill_tree[83], get_skill_mod( ch, 83 ),
   ch->skill_tree[102], get_skill_mod( ch, 102 ),
   ch->skill_tree[84], get_skill_mod( ch, 84 ),
   ch->skill_tree[103], get_skill_mod( ch, 103 ),
   ch->skill_tree[85], get_skill_mod( ch, 85 ),
   ch->skill_tree[104], get_skill_mod( ch, 104 ),
   ch->skill_tree[86], get_skill_mod( ch, 86 ),
   ch->skill_tree[105], get_skill_mod( ch, 105 ),
   ch->skill_tree[87], get_skill_mod( ch, 87 ),
   ch->skill_tree[106], get_skill_mod( ch, 106 ),
   ch->skill_tree[88], get_skill_mod( ch, 88 ),
   ch->skill_tree[107], get_skill_mod( ch, 107 ),
   ch->skill_tree[89], get_skill_mod( ch, 89 ),
   ch->skill_tree[108], get_skill_mod( ch, 108 ),
   ch->skill_tree[90], get_skill_mod( ch, 90 ),
   ch->skill_tree[109], get_skill_mod( ch, 109 ),
   ch->skill_tree[91], get_skill_mod( ch, 91 ),
   ch->skill_tree[110], get_skill_mod( ch, 110 ),
   ch->skill_tree[92], get_skill_mod( ch, 92 ),
   ch->skill_tree[111], get_skill_mod( ch, 111 ),
   ch->skill_tree[93], get_skill_mod( ch, 93 ),
   ch->skill_tree[112], get_skill_mod( ch, 112 ),
   ch->skill_tree[94], get_skill_mod( ch, 94 ),
   ch->skill_tree[113], get_skill_mod( ch, 113 ),
   ch->skill_tree[95], get_skill_mod( ch, 95 ),
   ch->skill_tree[114], get_skill_mod( ch, 114 ),
   ch->skill_tree[115], get_skill_mod( ch, 115 ),
   ch->skill_tree[116], get_skill_mod( ch, 116 ) );
  editor_send_to_char( AT_BLUE, buf, showto );
 }
 else
 {
  sprintf( buf, "&WCraftsmanship&w:&z [&C%4d&z|&G%4d&z]    "
                "&WEvaluation&w:&z    [&C%4d&z|&G%4d&z]\n\r"
                "&WTechnology&w:&z    [&C%4d&z|&G%4d&z]    "
                "&WCovert&w:&z        [&C%4d&z|&G%4d&z]\n\r"
                "&WPhysical&w:&z      [&C%4d&z|&G%4d&z]    "
                "&WMagic&w:&z         [&C%4d&z|&G%4d&z]\n\r",
   ch->skill_tree[0], get_skill_mod( ch, 0 ),
   ch->skill_tree[9], get_skill_mod( ch, 9 ),
   ch->skill_tree[15], get_skill_mod( ch, 15 ),
   ch->skill_tree[25], get_skill_mod( ch, 25 ),
   ch->skill_tree[33], get_skill_mod( ch, 33 ),
   ch->skill_tree[76], get_skill_mod( ch, 76 ) );
  editor_send_to_char( AT_BLUE, buf, showto );
 }

 return;
}

void show_obj_values( CHAR_DATA *ch, OBJ_DATA *obj, int imm )
{
    char buf[MAX_STRING_LENGTH];

    if ( imm == 0 )
    {
     switch( obj->item_type )
     {
	default:	/* No values. */
	    break;

	case ITEM_PORTAL:
	    sprintf( buf, "&z[&Wv0&z] &cDestination&w:&z [&Wvnum &R%d&z]\n\r", obj->value[0] );
            editor_send_to_char( AT_BLUE, buf, ch );
	    sprintf( buf, "&z[&Wv1&z] &cSuction&w:&z     [&R%d&z]\n\r", obj->value[1] );
            editor_send_to_char( AT_BLUE, buf, ch );
            break;
	case ITEM_WAND:
	case ITEM_LENSE:
	case ITEM_STAFF:
	    if (obj->value[1] == -1 )
                sprintf( buf,
                    "&z[&Wv0&z] &cLevel&w:          &z[&R%d&z]\n\r"
                    "&z[&Wv1&z] &cCharges&w:        &z[&WInfinite&w(&R-1&w)&z]\n\r"
                    "&z[&Wv3&z] &cSpell&w:          &z[&W%s&z]\n\r",
                    obj->value[0],
		    obj->value[3] != -1 ? skill_table[obj->value[3]].name
		                    : "none" );
            else        
                sprintf( buf,
        	    "&z[&Wv0&z] &cLevel&w:          &z[&R%d&z]\n\r"
        	    "&z[&Wv1&z] &cCharges Total&w:  &z[&R%d&z]\n\r"
        	    "&z[&Wv2&z] &cCharges Left&w:   &z[&R%d&z]\n\r" 
        	    "&z[&Wv3&z] &cSpell&w:          &z[&W%s&z]\n\r",
        	    obj->value[0],
        	    obj->value[1],
        	    obj->value[2],
        	    obj->value[3] != -1 ? skill_table[obj->value[3]].name
		                    : "none" );
	    editor_send_to_char(AT_BLUE, buf, ch );
	    break;

	case ITEM_SCROLL:
	case ITEM_POTION:
	case ITEM_PILL:
            sprintf( buf,
		"&z[&Wv0&z] &cLevel&w:  &z[&R%d&z]\n\r"
		"&z[&Wv1&z] &cSpell&w:  &z[&W%s&z]\n\r"
		"&z[&Wv2&z] &cSpell&w:  &z[&W%s&z]\n\r"
		"&z[&Wv3&z] &cSpell&w:  &z[&W%s&z]\n\r",
		obj->value[0],
		obj->value[1] != -1 ? skill_table[obj->value[1]].name
		                    : "none",
		obj->value[2] != -1 ? skill_table[obj->value[2]].name
                                    : "none",
		obj->value[3] != -1 ? skill_table[obj->value[3]].name
		                    : "none" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

        case ITEM_BULLET:
            sprintf( buf,
                "&z[&Wv0&z] &CBullet Type&W:		&z[&W%s&z]%d&X\n\r"
                "&z[&Wv1&z] &CCalibur&W:		&z[&W%d&z]&X\n\r",
		flag_string( weapon_flags, obj->value[0] ), obj->value[0],
                obj->value[1] );
            editor_send_to_char(C_DEFAULT, buf, ch );
         break;

        case ITEM_ARROW:
         sprintf( buf,
          "&z[&Wv1&z] &CArrow size&W:           &z[&W%d&z]&X\n\r",
           obj->value[1] );
         editor_send_to_char(C_DEFAULT, buf, ch );
         sprintf( buf,
          "&z[&Wv2&z] &CArrow type&W:           &z[&W%s&z]%d&X\n\r",
	   flag_string( arrow_types, obj->value[2] ), obj->value[2] );
         editor_send_to_char(C_DEFAULT, buf, ch );
         break;

        case ITEM_CLIP:
            sprintf( buf,
                "&z[&Wv0&z] &CCapacity&W:         &z[&W%d&z]&X\n\r"
                "&z[&Wv1&z] &CCalibur&W:          &z[&W%d&z]&X\n\r"
                "&z[&Wv2&z] &CWeapon type&W:      &z[&W%s&z]%d&X\n\r",
                obj->value[0], obj->value[1],
		flag_string( ranged_weapon_flags, obj->value[2] ),
		obj->value[2] );
            editor_send_to_char(C_DEFAULT, buf, ch );
         break;

        case ITEM_RANGED_WEAPON:
            sprintf( buf,
                "&z[&Wv0&z] &CRanged Weapon Type&W:  &z[&W%s&z]%d&X\n\r",
                flag_string( ranged_weapon_flags, obj->value[0] ),
                obj->value[0] );
            editor_send_to_char(C_DEFAULT, buf, ch );

		if ( obj->value[0] == RANGED_WEAPON_FIREARM )
		{
                 sprintf( buf,
                 "&z[&Wv1&z] &CCalibur&W:            &z[&W%d&z]&X\n\r"
                 "&z[&Wv2&z] &CClip vnum&W:          &z[&W%d&z]&X\n\r"
                 "&z[&Wv3&z] &CRate of firing&W:     &z[&W%s&z]%d&X\n\r",
                 obj->value[1], obj->value[2],
                 flag_string( gun_type_flags, obj->value[3] ),
                 obj->value[3] );
                 editor_send_to_char(C_DEFAULT, buf, ch );
		}
                else
		if ( obj->value[0] == RANGED_WEAPON_BOW )
		{
                 sprintf( buf,
                 "&z[&Wv1&z] &CMax arrow size&W:     &z[&W%d&z]&X\n\r"
                 "&z[&Wv2&z] &CBow test&W:           &z[&W%d&z]&X\n\r",
                 obj->value[1], obj->value[2] );
                 editor_send_to_char(C_DEFAULT, buf, ch );
		}
                else
		if ( obj->value[0] == RANGED_WEAPON_BAZOOKA )
		{
                 sprintf( buf,
                 "&z[&Wv1&z] &CMax warhead size&W:   &z[&W%d&z]&X\n\r",
                 obj->value[1] );
                 editor_send_to_char(C_DEFAULT, buf, ch );
		}
            sprintf( buf,
		"&z[&Wv4&z] &CRange&W:   		&z[&W%d&z]&X\n\r",
		obj->value[4] );
            editor_send_to_char(C_DEFAULT, buf, ch );
            break;

	case ITEM_WEAPON:
            sprintf( buf,
                "&z[&Wv8&z] &cDamage Class&w:          &z[&W%s&z]%d\n\r",
                flag_string( weaponclass_flags, obj->value[8] ), obj->value[8]);
            editor_send_to_char(C_DEFAULT, buf, ch );
           if ( obj->value[8] == WEAPON_BLADE )
            sprintf (buf,
       		"&z[&Wv7&z] &cInitial wield state&w:   &z[&W%s&z]%d\n\r",
		flag_string( blade_flags, obj->value[7] ), obj->value[7] );
           else 
            sprintf( buf,
		"&z[&Wv3&z] &cDamage Noun&w:           &z[&W%s&z]%d\n\r",
		flag_string( weapon_flags, obj->value[3] ), obj->value[3]);
	    editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
		"&z[&Wv4&z] &cLength&w:                &z[&W%s&z]%d\n\r",
                obj->value[4] == 1 ? "short" :
                obj->value[4] == 2 ? "medium" : "long", obj->value[4]);
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_SWITCH:
	    sprintf( buf,
		"&z[&Wv0&z] &cActivation Command&w:    &z[&R%s&z]%d\n\r",
            flag_string( activation_commands, obj->value[0] ), obj->value[0]  );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&z[&Wv1&z] &cAffect Type&w:           &z[&R%s&z]%d\n\r",
            flag_string( switch_affect_types, obj->value[1] ), obj->value[1]  );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&z[&Wv2&z] &cRoom Vnum&w:             &z[&R%d&z]\n\r",
            obj->value[2]  );
	    editor_send_to_char(C_DEFAULT, buf, ch );

         if ( obj->value[1] == SWITCH_DOOR )
         {
	    sprintf( buf,
		"&z[&Wv3&z] &cDirection&w:             &z[&R%s&z]%d\n\r",
            flag_string( direction_flags, obj->value[3] ), obj->value[3]  );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&z[&Wv4&z] &cAction&w:                &z[&R%s&z]%d\n\r",
            flag_string( door_action_flags, obj->value[4] ), obj->value[4]  );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	   }
         if ( obj->value[1] == SWITCH_OLOAD )
         {
	    sprintf( buf,
		"&z[&Wv3&z] &cObj Vnum&w:              &z[&R%d&z]\n\r",
             obj->value[3]  );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	 }

         if ( obj->value[1] == SWITCH_MLOAD )
         {
	    sprintf( buf,
		"&z[&Wv3&z] &cMob Vnum&w:              &z[&R%d&z]\n\r",
             obj->value[3]  );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	 }
	    break;

	case ITEM_FURNITURE:
	    sprintf( buf,
		"&z[&Wv1&z] &cType&w:                  &z[&R%s&z]%d\n\r",
            flag_string( furniture_flags, obj->value[1] ), obj->value[1] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_ARMOR:
	    sprintf( buf,
		"&z[&Wv1&z] &cPockets&w:               &z[&W%s&z]\n\r",
                obj->value[1] == TRUE ? "yes" : "no" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&z[&Wv2&z] &cMax pocket carry&w:      &z[&W%dkg&z]\n\r",
                obj->value[2]);
	    editor_send_to_char(C_DEFAULT, buf, ch );

	    sprintf( buf,
		"&z[&Wv4&z] &cArmor Type&w:            &z[&R%s&z]%d\n\r",
		flag_string( armor_types, obj->value[4] ), obj->value[4]);
	    editor_send_to_char(C_DEFAULT, buf, ch );

	    sprintf( buf,
		"&z-----------------Sheath data--------------------\n\r" );
	    editor_send_to_char(C_DEFAULT, buf, ch );

            sprintf( buf,
		"&z[&Wv6&z] &cWeapon Type&w:           &z[&R%s&z]%d\n\r",
            flag_string( weaponclass_flags, obj->value[6] ), obj->value[6] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
		"&z[&Wv7&z] &cWeapon Length&w:         &z[&R%s&z]%d\n\r",
                obj->value[7] == 1 ? "short" :
                obj->value[7] == 2 ? "medium" : "long", obj->value[7]);
	    editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
		"&z[&Wv8&z] &cWeapon Vnum&w:           &z[&R%d&z]\n\r",
            obj->value[8] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_BOMB:
	    sprintf( buf,
		"&z[&Wv1&z] &cWarhead Type&w:          &z[&R%s&z]%d\n\r",
		flag_string( warhead_flags, obj->value[1] ), obj->value[1]);
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&z[&Wv2&z] &cPropulsion system&w:     &z[&W%s&z]%d\n\r",
		flag_string( propulsion_flags, obj->value[2] ),obj->value[2]);
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&z[&Wv3&z] &cPayload&w:               &z[&W%d&z]\n\r", 
                obj->value[3]);
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_GAS_CLOUD:
            sprintf( buf,
                "&z[&Wv0&z] &CInitial timer&W:		&W%d&X\n\r",
		 obj->value[0] );
            editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
		"&z[&Wv1&z] &cDamage type&w:		&z[&W%s&z]%d\n\r",
		flag_string( damage_flags, obj->value[1]),
                 obj->value[1]);
            editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
                "&z[&Wv2&z] &CDamage amount&W:		&W%d&X\n\r",
		 obj->value[2] );
            editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
		"&z[&Wv3&z] &cSpecial affect&w:		&z[&W%s&z]%d\n\r",
		flag_string( gas_affects, obj->value[3] ),
                 obj->value[3]);
            editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_NOTEBOARD:
	    sprintf( buf,
		"&z[&Wv0&z] &cDecoder item&w:        &z[&R%d&z] [&W%s&z]\n\r"
		"&z[&Wv1&z] &cMinimum read level&w:  &z[&R%d&z]\n\r"
		"&z[&Wv2&z] &cMinimum write level&w: &z[&R%d&z]\n\r",
		obj->value[0], 
		get_obj_index(obj->value[0])
		    ? get_obj_index(obj->value[0])->short_descr
		    : "none",
		obj->value[1],
		obj->value[2] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_CONTAINER:
	    sprintf( buf,
		"&z[&Wv0&z] &cWeight&w: &z[&R%d &Wkg&z]\n\r"
		"&z[&Wv1&z] &cFlags&w:  &z[&W%s&z]\n\r"
		"&z[&Wv2&z] &cKey&w:    &z[&W%s&z] [&R%d&z]\n\r",
		obj->value[0],
		flag_string( container_flags, obj->value[1] ),
                get_obj_index(obj->value[2])
		    ? get_obj_index(obj->value[2])->short_descr
                    : "none", obj->value[2]);
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_DRINK_CON:
	    sprintf( buf,
	        "&z[&Wv0&z] &cLiquid Total&w: &z[&R%d&z]\n\r"
	        "&z[&Wv1&z] &cLiquid Left&w:  &z[&R%d&z]\n\r"
	        "&z[&Wv2&z] &cLiquid&w:       &z[&W%s&z]\n\r"
	        "&z[&Wv3&z] &cPoisoned&w:     &z[&W%s&z]\n\r",
	        obj->value[0],
	        obj->value[1],
	        flag_string( liquid_flags, obj->value[2] ),
	        obj->value[3] != 0 ? "Yes" : "No" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;


	case ITEM_FOOD:
	    sprintf( buf,
		"&z[&Wv0&z] &cFood hours&w:  &z[&R%d&z]\n\r"
		"&z[&Wv3&z] &cCondition&w:   &z[&W%s&z]\n\r",
		obj->value[0],
		flag_string( food_condition, obj->value[3] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_MONEY:
            sprintf( buf, "&z[&Wv0&z] &cGold&w:   &z[&R%d&z]\n\r"
	                  "&z[&Wv1&z] &cSilver&w: &z[&R%d&z]\n\r",
		    obj->value[0], obj->value[1] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf, "&z[&Wv2&z] &cCopper&w: &z[&R%d&z]\n\r",
		    obj->value[2] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_CORPSE_PC:
	case ITEM_CORPSE_NPC:
	    sprintf( buf, "&z[&Wv0&z] &cRace type&w: &z[&R%s&z]&C%d&z\n\r",
	     flag_string( race_type_flags, obj->value[0] ), obj->value[0] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;
    }
   }
   else if ( imm == 1 )
   {
     switch( obj->item_type )
     {
	default:	/* No values. */
	    break;

	case ITEM_WAND:
	case ITEM_LENSE:
	case ITEM_STAFF:
	    if (obj->value[1] == -1 )
                sprintf( buf,
                    "&cLevel&w:          &z[&R%d&z]\n\r"
                    "&cCharges&w:        &z[&WInfinite&w(&R-1&w)&z]\n\r"
                    "&cSpell&w:          &z[&W%s&z]\n\r",
                    obj->value[0],
		    obj->value[3] != -1 ? skill_table[obj->value[3]].name
		                    : "none" );
            else        
                sprintf( buf,
        	    "&cLevel&w:          &z[&R%d&z]\n\r"
        	    "&cCharges Total&w:  &z[&R%d&z]\n\r"
        	    "&cCharges Left&w:   &z[&R%d&z]\n\r" 
        	    "&cSpell&w:          &z[&W%s&z]\n\r",
        	    obj->value[0],
        	    obj->value[1],
        	    obj->value[2],
        	    obj->value[3] != -1 ? skill_table[obj->value[3]].name
		                    : "none" );
	    editor_send_to_char(AT_BLUE, buf, ch );
	    break;

	case ITEM_SCROLL:
	case ITEM_POTION:
	case ITEM_PILL:
            sprintf( buf,
		"&cLevel&w:  &z[&R%d&z]\n\r"
		"&cSpell&w:  &z[&W%s&z]\n\r"
		"&cSpell&w:  &z[&W%s&z]\n\r"
		"&cSpell&w:  &z[&W%s&z]\n\r",
		obj->value[0],
		obj->value[1] != -1 ? skill_table[obj->value[1]].name
		                    : "none",
		obj->value[2] != -1 ? skill_table[obj->value[2]].name
                                    : "none",
		obj->value[3] != -1 ? skill_table[obj->value[3]].name
		                    : "none" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

        case ITEM_BULLET:
            sprintf( buf,
                "&CBullet Type&W:	&z[&W%s&z]&X\n\r"
                "&CCalibur&W:		&z[&W%d&z]&X\n\r",
		flag_string( weapon_flags, obj->value[0] ),
                obj->value[1] );
            editor_send_to_char(C_DEFAULT, buf, ch );
         break;

        case ITEM_ARROW:
         sprintf( buf,
          "&CArrow size&W:           &z[&W%d&z]&X\n\r",
           obj->value[1] );
         editor_send_to_char(C_DEFAULT, buf, ch );
         sprintf( buf,
          "&CArrow type&W:           &z[&W%s&z]&X\n\r",
	   flag_string( arrow_types, obj->value[2] ) );
         editor_send_to_char(C_DEFAULT, buf, ch );
         break;

        case ITEM_CLIP:
            sprintf( buf,
                "&CCapacity&W:         &z[&W%d&z]&X\n\r"
                "&CCalibur&W:          &z[&W%d&z]&X\n\r"
                "&CWeapon type&W:      &z[&W%s&z]&X\n\r",
                obj->value[0], obj->value[1],
		flag_string( ranged_weapon_flags, obj->value[2] ) );
            editor_send_to_char(C_DEFAULT, buf, ch );
         break;

        case ITEM_RANGED_WEAPON:
            sprintf( buf,
                "&CRanged Weapon Type&W:  &z[&W%s&z]&X\n\r",
                flag_string( ranged_weapon_flags, obj->value[0] ) );
            editor_send_to_char(C_DEFAULT, buf, ch );

		if ( obj->value[0] == RANGED_WEAPON_FIREARM )
		{
                 sprintf( buf,
                 "&CCalibur&W:            &z[&W%d&z]&X\n\r"
                 "&CClip vnum&W:          &z[&W%d&z]&X\n\r"
                 "&CRate of firing&W:     &z[&W%s&z]&X\n\r",
                 obj->value[1], obj->value[2],
                 flag_string( gun_type_flags, obj->value[3] ) );
                 editor_send_to_char(C_DEFAULT, buf, ch );
		}
                else
		if ( obj->value[0] == RANGED_WEAPON_BOW )
		{
                 sprintf( buf,
                 "&CMax arrow size&W:     &z[&W%d&z]&X\n\r"
                 "&CBow test&W:           &z[&W%d&z]&X\n\r",
                 obj->value[1], obj->value[2] );
                 editor_send_to_char(C_DEFAULT, buf, ch );
		}
                else
		if ( obj->value[0] == RANGED_WEAPON_BAZOOKA )
		{
                 sprintf( buf,
                 "&CMax warhead size&W:   &z[&W%d&z]&X\n\r",
                 obj->value[1] );
                 editor_send_to_char(C_DEFAULT, buf, ch );
		}
            sprintf( buf,
		"&CRange&W:   		&z[&W%d&z]&X\n\r",
		obj->value[4] );
            editor_send_to_char(C_DEFAULT, buf, ch );
            break;

	case ITEM_WEAPON:
            sprintf( buf,
                "&cDamage Class&w:          &z[&W%s&z]\n\r",
                flag_string( weaponclass_flags, obj->value[8] ) );
            editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
		"&cLength&w:                &z[&W%s&z]\n\r",
                obj->value[4] == 1 ? "short" :
                obj->value[4] == 2 ? "medium" : "long" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_FURNITURE:
	    sprintf( buf,
		"&cType&w:                  &z[&R%s&z]\n\r",
            flag_string( furniture_flags, obj->value[1] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_ARMOR:
	    sprintf( buf,
		"&cPockets&w:               &z[&W%s&z]\n\r",
                obj->value[1] == TRUE ? "yes" : "no" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&cMax pocket carry&w:      &z[&W%dkg&z]\n\r",
                obj->value[2]);
	    editor_send_to_char(C_DEFAULT, buf, ch );

	    sprintf( buf,
		"&cArmor Type&w:            &z[&R%s&z]\n\r",
		flag_string( armor_types, obj->value[4] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );

	    sprintf( buf,
		"&z-----------------Sheath data--------------------\n\r" );
	    editor_send_to_char(C_DEFAULT, buf, ch );

            sprintf( buf,
		"&cWeapon Type&w:           &z[&R%s&z]\n\r",
            flag_string( weaponclass_flags, obj->value[6] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
		"&cWeapon Length&w:         &z[&R%s&z]\n\r",
                obj->value[7] == 1 ? "short" :
                obj->value[7] == 2 ? "medium" : "long" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
            break;

	case ITEM_BOMB:
	    sprintf( buf,
		"&cWarhead Type&w:          &z[&R%s&z]\n\r",
		flag_string( warhead_flags, obj->value[1] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&cPropulsion system&w:     &z[&W%s&z]\n\r",
		flag_string( propulsion_flags, obj->value[2] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&cPayload&w:               &z[&W%d&z]\n\r", 
                obj->value[3]);
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_CONTAINER:
	    sprintf( buf,
		"&cWeight&w: &z[&R%d &Wkg&z]\n\r",
		obj->value[0] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_DRINK_CON:
	    sprintf( buf,
	        "&cLiquid Total&w: &z[&R%d&z]\n\r"
	        "&cLiquid Left&w:  &z[&R%d&z]\n\r"
	        "&cLiquid&w:       &z[&W%s&z]\n\r"
	        "&cPoisoned&w:     &z[&W%s&z]\n\r",
	        obj->value[0],
	        obj->value[1],
	        flag_string( liquid_flags, obj->value[2] ),
	        obj->value[3] != 0 ? "Yes" : "No" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;


	case ITEM_FOOD:
	    sprintf( buf,
		"&cCondition&w:   &z[&W%s&z]\n\r",
		flag_string( food_condition, obj->value[3] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_MONEY:
            sprintf( buf, "&cGold&w:   &z[&R%d&z]\n\r"
	                  "&cSilver&w: &z[&R%d&z]\n\r",
		    obj->value[0], obj->value[1] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf, "&cCopper&w: &z[&R%d&z]\n\r",
		    obj->value[2] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_CORPSE_PC:
	case ITEM_CORPSE_NPC:
	    sprintf( buf, "&cRace type&w: &z[&R%s&z]\n\r",
	     flag_string( race_type_flags, obj->value[0] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;
     }
    }
    else if ( imm == 2 )
    {
     switch( obj->item_type )
     {
	default:	/* No values. */
	    break;

        case ITEM_BULLET:
            sprintf( buf,
                "&CBullet Type&W:	&z[&W%s&z]&X\n\r"
                "&CCalibur&W:		&z[&W%d&z]&X\n\r",
		flag_string( weapon_flags, obj->value[0] ),
                obj->value[1] );
            editor_send_to_char(C_DEFAULT, buf, ch );
         break;

        case ITEM_ARROW:
         sprintf( buf,
          "&CArrow size&W:           &z[&W%d&z]&X\n\r",
           obj->value[1] );
         editor_send_to_char(C_DEFAULT, buf, ch );
         sprintf( buf,
          "&CArrow type&W:           &z[&W%s&z]&X\n\r",
	   flag_string( arrow_types, obj->value[2] ) );
         editor_send_to_char(C_DEFAULT, buf, ch );
         break;

        case ITEM_CLIP:
            sprintf( buf,
                "&CCapacity&W:         &z[&W%d&z]&X\n\r"
                "&CCalibur&W:          &z[&W%d&z]&X\n\r"
                "&CWeapon type&W:      &z[&W%s&z]&X\n\r",
                obj->value[0], obj->value[1],
		flag_string( ranged_weapon_flags, obj->value[2] ) );
            editor_send_to_char(C_DEFAULT, buf, ch );
         break;

        case ITEM_RANGED_WEAPON:
            sprintf( buf,
                "&CRanged Weapon Type&W:  &z[&W%s&z]&X\n\r",
                flag_string( ranged_weapon_flags, obj->value[0] ) );
            editor_send_to_char(C_DEFAULT, buf, ch );

		if ( obj->value[0] == RANGED_WEAPON_FIREARM )
		{
                 sprintf( buf,
                 "&CCalibur&W:            &z[&W%d&z]&X\n\r"
                 "&CClip vnum&W:          &z[&W%d&z]&X\n\r"
                 "&CRate of firing&W:     &z[&W%s&z]&X\n\r",
                 obj->value[1], obj->value[2],
                 flag_string( gun_type_flags, obj->value[3] ) );
                 editor_send_to_char(C_DEFAULT, buf, ch );
		}
                else
		if ( obj->value[0] == RANGED_WEAPON_BOW )
		{
                 sprintf( buf,
                 "&CMax arrow size&W:     &z[&W%d&z]&X\n\r"
                 "&CBow test&W:           &z[&W%d&z]&X\n\r",
                 obj->value[1], obj->value[2] );
                 editor_send_to_char(C_DEFAULT, buf, ch );
		}
                else
		if ( obj->value[0] == RANGED_WEAPON_BAZOOKA )
		{
                 sprintf( buf,
                 "&CMax warhead size&W:   &z[&W%d&z]&X\n\r",
                 obj->value[1] );
                 editor_send_to_char(C_DEFAULT, buf, ch );
		}
            sprintf( buf,
		"&CRange&W:   		&z[&W%d&z]&X\n\r",
		obj->value[4] );
            editor_send_to_char(C_DEFAULT, buf, ch );
            break;

	case ITEM_WEAPON:
            sprintf( buf,
                "&cDamage Class&w:          &z[&W%s&z]\n\r",
                flag_string( weaponclass_flags, obj->value[8] ) );
            editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
		"&cLength&w:                &z[&W%s&z]\n\r",
                obj->value[4] == 1 ? "short" :
                obj->value[4] == 2 ? "medium" : "long" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_FURNITURE:
	    sprintf( buf,
		"&cType&w:                  &z[&R%s&z]\n\r",
            flag_string( furniture_flags, obj->value[1] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_ARMOR:
	    sprintf( buf,
		"&cPockets&w:               &z[&W%s&z]\n\r",
                obj->value[1] == TRUE ? "yes" : "no" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&cMax pocket carry&w:      &z[&W%dkg&z]\n\r",
                obj->value[2]);
	    editor_send_to_char(C_DEFAULT, buf, ch );

	    sprintf( buf,
		"&cArmor Type&w:            &z[&R%s&z]\n\r",
		flag_string( armor_types, obj->value[4] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );

	    sprintf( buf,
		"&z-----------------Sheath data--------------------\n\r" );
	    editor_send_to_char(C_DEFAULT, buf, ch );

            sprintf( buf,
		"&cWeapon Type&w:           &z[&R%s&z]\n\r",
            flag_string( weaponclass_flags, obj->value[6] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
            sprintf( buf,
		"&cWeapon Length&w:         &z[&R%s&z]\n\r",
                obj->value[7] == 1 ? "short" :
                obj->value[7] == 2 ? "medium" : "long" );
	    editor_send_to_char(C_DEFAULT, buf, ch );
            break;

	case ITEM_BOMB:
	    sprintf( buf,
		"&cWarhead Type&w:          &z[&R%s&z]\n\r",
		flag_string( warhead_flags, obj->value[1] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&cPropulsion system&w:     &z[&W%s&z]\n\r",
		flag_string( propulsion_flags, obj->value[2] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf,
		"&cPayload&w:               &z[&W%d&z]\n\r", 
                obj->value[3]);
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_CONTAINER:
	    sprintf( buf,
		"&cWeight&w: &z[&R%d &Wkg&z]\n\r",
		obj->value[0] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_DRINK_CON:
	    sprintf( buf,
	        "&cLiquid Total&w: &z[&R%d&z]\n\r"
	        "&cLiquid Left&w:  &z[&R%d&z]\n\r"
	        "&cLiquid&w:       &z[&W%s&z]\n\r",
	        obj->value[0],
	        obj->value[1],
	        flag_string( liquid_flags, obj->value[2] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_MONEY:
            sprintf( buf, "&cGold&w:   &z[&R%d&z]\n\r"
	                  "&cSilver&w: &z[&R%d&z]\n\r",
		    obj->value[0], obj->value[1] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    sprintf( buf, "&cCopper&w: &z[&R%d&z]\n\r",
		    obj->value[2] );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;

	case ITEM_CORPSE_PC:
	case ITEM_CORPSE_NPC:
	    sprintf( buf, "&cRace type&w: &z[&R%s&z]&z\n\r",
	     flag_string( race_type_flags, obj->value[0] ) );
	    editor_send_to_char(C_DEFAULT, buf, ch );
	    break;
     }
    }
    return;
}

/* rand_figment is for insanity/hallucinatory coding -Flux. */
CHAR_DATA *rand_figment( CHAR_DATA *ch )
{
         DESCRIPTOR_DATA *d;
         CHAR_DATA	*figment;
         bool		dfig = FALSE;

  	  for ( d = descriptor_list; d; d = d->next )
	  {
	   if ( !d )
           continue;

	    if ( d->connected == CON_PLAYING
		&& d->character != ch
		&& d->character->in_room
		&& can_see( ch, d->character ) )
            {
             dfig = TRUE;
	     break;
            }
	   if ( !d->next )
            break;
	  }

         if ( !dfig )
          figment = ch;
         else
          figment = d->character;

 return figment;
}

MOB_INDEX_DATA *rand_figment_mob( CHAR_DATA *ch )
{
 MOB_INDEX_DATA *figmentmob;
 int		figmentnum = 0;
 int		lfig = 0;
 int		ufig = 0;


          lfig = ch->in_room->area->lvnum;
          ufig = ch->in_room->area->uvnum;

          while( !(figmentmob = get_mob_index( figmentnum ) ) )
           figmentnum = number_range( lfig, ufig );

 return figmentmob;
}

OBJ_INDEX_DATA *rand_figment_obj( CHAR_DATA *ch )
{
 OBJ_INDEX_DATA *figmentobj;
 int		figmentnum = 0;
 int		lfig = 0;
 int		ufig = 0;


          lfig = ch->in_room->area->lvnum;
          ufig = ch->in_room->area->uvnum;

          while( !(figmentobj = get_obj_index( figmentnum ) ) )
           figmentnum = number_range( lfig, ufig );

 return figmentobj;
}

void typo_message( CHAR_DATA *ch )
{
 switch( dice( 1, 11 ) )
 {
  case 1:
   send_to_char( AT_YELLOW, "You have mistyped.\n\r", ch ); break;
  case 2:
   send_to_char( AT_YELLOW, "Huh?\n\r", ch ); break;
  case 3:
   send_to_char( AT_YELLOW, "What?\n\r", ch ); break;
  case 4:
   send_to_char( AT_YELLOW, "What are you doing Dave?\n\r", ch ); break;
  case 5:
   send_to_char( AT_YELLOW, "TYPO!!!\n\r", ch ); break;
  case 6:
   send_to_char( AT_YELLOW, "Does not compute.\n\r", ch ); break;
  case 7:
   send_to_char( AT_YELLOW, "Error: Abort, Retry, Fail\n\r", ch ); break;
  case 8:
   send_to_char( AT_YELLOW, "It's the keyboard, really...\n\r", ch ); break;
  case 9:
   send_to_char( AT_YELLOW, "And on your right, the typo demon...\n\r", ch ); break;
  case 10:
   send_to_char( AT_YELLOW, "You sure you don't need typing lessons?\n\r", ch ); break;
  case 11:
   send_to_char( AT_YELLOW, "You know, drinking and typing don't mix...\n\r", ch ); break;
 }

  return;
}

int randroom( void )
{
 int 			room = 0;
 bool			loop = FALSE;
 ROOM_INDEX_DATA 	*rcheck;


 while( loop == FALSE )
 {
  room = dice( 1, 31999 );

  if ( ( rcheck = get_room_index( room ) ) == NULL )
   continue;

  if ( IS_SET( rcheck->room_flags, ROOM_PRIVATE   )
	|| IS_SET( rcheck->room_flags, ROOM_SOLITARY  )
	|| IS_SET( rcheck->room_flags, ROOM_NO_ASTRAL_IN )
	|| IS_SET( rcheck->room_flags, ROOM_NO_ASTRAL_OUT ) 
	|| IS_SET( rcheck->area->area_flags, AREA_PROTOTYPE )
	|| IS_SET( rcheck->area->area_flags, AREA_MUDSCHOOL ) )
   continue;

  loop = TRUE;
 }

 return room;
}

void polymorph_char( CHAR_DATA *ch, RACE_DATA *race_o, RACE_DATA *race_n )
{
 int counter;

 if ( race_o->vnum == RACE_QUICKSILVER )
  for( counter = 0; counter < 5; counter++ ) 
   ch->pcdata->morph[counter] = 0;

 if ( race_o->vnum == RACE_MAUDLIN )
  for( counter = 0; counter < 10; counter++ ) 
   ch->pcdata->assimilate[counter] = 0;

 ch->size = race_n->size;
 ch->pcdata->claws = race_n->claws;


        ch->pcdata->mod_str -= race_o->mstr;
	ch->pcdata->mod_int -= race_o->mint;
	ch->pcdata->mod_wis -= race_o->mwis;
	ch->pcdata->mod_dex -= race_o->mdex;
	ch->pcdata->mod_con -= race_o->mcon;
	ch->pcdata->mod_agi -= race_o->magi;
	ch->pcdata->mod_cha -= race_o->mcha;
        ch->pcdata->pimm[0]     -= race_o->mimm[0];
        ch->pcdata->pimm[1]     -= race_o->mimm[1];
        ch->pcdata->pimm[2]     -= race_o->mimm[2];
        ch->pcdata->pimm[3]     -= race_o->mimm[3];
        ch->pcdata->pimm[4]     -= race_o->mimm[4];
        ch->pcdata->pimm[5]     -= race_o->mimm[5];
        ch->pcdata->pimm[6]     -= race_o->mimm[6];
        ch->pcdata->pimm[7]     -= race_o->mimm[7];
        ch->pcdata->pimm[8]     -= race_o->mimm[8];
        ch->pcdata->pimm[9]     -= race_o->mimm[9];
        ch->pcdata->pimm[10]     -= race_o->mimm[10];
        ch->pcdata->pimm[11]     -= race_o->mimm[11];
        ch->pcdata->pimm[12]     -= race_o->mimm[12];
        ch->pcdata->pimm[13]     -= race_o->mimm[13];
        ch->pcdata->pimm[14]     -= race_o->mimm[14];

	ch->pcdata->mod_str += race_n->mstr;
	ch->pcdata->mod_int += race_n->mint;
	ch->pcdata->mod_wis += race_n->mwis;
	ch->pcdata->mod_dex += race_n->mdex;
	ch->pcdata->mod_con += race_n->mcon;
	ch->pcdata->mod_agi += race_n->magi;
	ch->pcdata->mod_cha += race_n->mcha;
        ch->pcdata->pimm[0]     += race_n->mimm[0];
        ch->pcdata->pimm[1]     += race_n->mimm[1];
        ch->pcdata->pimm[2]     += race_n->mimm[2];
        ch->pcdata->pimm[3]     += race_n->mimm[3];
        ch->pcdata->pimm[4]     += race_n->mimm[4];
        ch->pcdata->pimm[5]     += race_n->mimm[5];
        ch->pcdata->pimm[6]     += race_n->mimm[6];
        ch->pcdata->pimm[7]     += race_n->mimm[7];
        ch->pcdata->pimm[8]     += race_n->mimm[8];
        ch->pcdata->pimm[9]     += race_n->mimm[9];
        ch->pcdata->pimm[10]     += race_n->mimm[10];
        ch->pcdata->pimm[11]     += race_n->mimm[11];
        ch->pcdata->pimm[12]     += race_n->mimm[12];
        ch->pcdata->pimm[13]     += race_n->mimm[13];
        ch->pcdata->pimm[14]     += race_n->mimm[14];
 return;
}

/*
 * Retrieve a character's age.
 */
int get_age( CHAR_DATA *ch )
{
    return 17 + ( ch->played + (int) ( current_time - ch->logon ) ) / 21600;

    /* 14400 assumes 30 second hours, 24 hours a day, 20 day - Kahn */
}



/*
 * Retrieve character's current strength.
 */
int get_curr_str( CHAR_DATA *ch )
{
    int max;
    int addon = 0;

    if ( IS_NPC( ch ) && !IS_STUNNED( ch, STUN_COMMAND )
      && !IS_STUNNED( ch, STUN_TOTAL ) )
     return ch->pIndexData->perm_str;
    else if ( IS_NPC(ch) )
     return 10;

	max = 25 + (get_race_data(ch->race))->mstr;

    if ( IS_STUNNED( ch, STUN_COMMAND ) )
     max = 10;

   if ( ch->pcdata->assimilate[ASSIM_RARM] == OBJ_VNUM_LOBSTER_CLAW )
    addon += 2;

   if ( ch->pcdata->assimilate[ASSIM_LARM] == OBJ_VNUM_LOBSTER_CLAW )
    addon += 2;

    return URANGE( 3, ch->perm_str + ch->pcdata->mod_str + addon, max );
}



/*
 * Retrieve character's current intelligence.
 */
int get_curr_int( CHAR_DATA *ch )
{
    int max;
    int addon = 0;

    if ( IS_NPC( ch ) )
    return ch->pIndexData->perm_int;

	max = 25 + (get_race_data(ch->race))->mint;

    return URANGE( 3, ch->perm_int + ch->pcdata->mod_int + addon, max );
}



/*
 * Retrieve character's current wisdom.
 */
int get_curr_wis( CHAR_DATA *ch )
{
    int max;
    int addon = 0;

    if ( IS_NPC( ch ) )
    return ch->pIndexData->perm_wis;

	max = 25 + (get_race_data(ch->race))->mwis;

    return URANGE( 3, ch->perm_wis + ch->pcdata->mod_wis + addon, max );
}



/*
 * Retrieve character's current dexterity.
 */
int get_curr_dex( CHAR_DATA *ch )
{
    int max;
    int addon = 0;

    if ( IS_NPC( ch ) && !IS_STUNNED( ch, STUN_COMMAND )
      && !IS_STUNNED( ch, STUN_TOTAL ) )
     return ch->pIndexData->perm_dex;
    else if ( IS_NPC(ch) )
     return 10;

    max = 25 + (get_race_data(ch->race))->mdex;

    if ( IS_STUNNED( ch, STUN_COMMAND ) )
     max = 10;

    if ( vision_impared(ch) )    
     max = 10;

    if ( ch->pcdata->assimilate[ASSIM_EXTRA_1] == OBJ_VNUM_TAIL ||
        ch->pcdata->assimilate[ASSIM_EXTRA_2] == OBJ_VNUM_TAIL ||
        ch->pcdata->assimilate[ASSIM_EXTRA_3] == OBJ_VNUM_TAIL ||
        ch->pcdata->assimilate[ASSIM_EXTRA_4] == OBJ_VNUM_TAIL ||
        ch->pcdata->assimilate[ASSIM_EXTRA_5] == OBJ_VNUM_TAIL )
    addon += 2;

    return URANGE( 3, ch->perm_dex + ch->pcdata->mod_dex + addon, max );
}

/*
 * Retrieve character's current constitution.
 */
int get_curr_con( CHAR_DATA *ch )
{
    int max;
    int addon = 0;

    if ( IS_NPC( ch ) )
    return ch->pIndexData->perm_con;

	max = 25 + (get_race_data(ch->race))->mcon;

   if ( ch->pcdata->assimilate[ASSIM_EXTRA_1] == OBJ_VNUM_SNAKE_SKIN ||
        ch->pcdata->assimilate[ASSIM_EXTRA_2] == OBJ_VNUM_SNAKE_SKIN ||
        ch->pcdata->assimilate[ASSIM_EXTRA_3] == OBJ_VNUM_SNAKE_SKIN ||
        ch->pcdata->assimilate[ASSIM_EXTRA_4] == OBJ_VNUM_SNAKE_SKIN ||
        ch->pcdata->assimilate[ASSIM_EXTRA_5] == OBJ_VNUM_SNAKE_SKIN )
    addon += 2;

    return URANGE( 3, ch->perm_con + ch->pcdata->mod_con + addon, max );
}

int get_curr_agi( CHAR_DATA *ch )
{
    int max;
    int addon = 0;

    if ( IS_NPC( ch ) && !IS_STUNNED( ch, STUN_COMMAND )
      && !IS_STUNNED( ch, STUN_TOTAL ) )
     return ch->pIndexData->perm_agi;
    else if ( IS_NPC(ch) )
     return 10;

	max = 25 + (get_race_data(ch->race))->magi;

    if ( IS_STUNNED( ch, STUN_COMMAND ) )
     max = 10;

    if ( vision_impared(ch) )    
     max = 10;

    if ( ch->pcdata->assimilate[ASSIM_RLEG] == OBJ_VNUM_THIN_LEG )
     addon += 1;

    if ( ch->pcdata->assimilate[ASSIM_LLEG] == OBJ_VNUM_THIN_LEG )
     addon += 1;

    return URANGE( 3, ch->perm_agi + ch->pcdata->mod_agi + addon, max );
}

int get_curr_cha( CHAR_DATA *ch )
{
    int max;
    int addon = 0;

    if ( IS_NPC( ch ) )
     return ch->pIndexData->perm_cha;

    max = 25 + (get_race_data(ch->race))->mcha;

    return URANGE( 3, ch->perm_cha + ch->pcdata->mod_cha + addon, max );
}


/*
 * Retrieve a character's carry capacity.
 */
int can_carry_n( CHAR_DATA *ch )
{
    if ( !IS_NPC( ch ) && ch->level >= LEVEL_IMMORTAL )
	return 1000;

    if ( IS_NPC( ch ) && IS_SET( ch->act, ACT_PET ) )
	return 0;

    return MAX_EQUIP + (2 * get_curr_dex( ch )) + (get_curr_str( ch ) * 2);
}

/*
 * Retrieve a character's carry capacity.
 */
int can_carry_w( CHAR_DATA *ch )
{
    int max_weight = 0;

    if ( !IS_NPC( ch ) && ch->level >= LEVEL_IMMORTAL )
	return 1000000;

    if ( IS_NPC( ch ) && IS_SET( ch->act, ACT_PET ) )
	return 0;

/* Angi: Make max carry weight less than the weight of your coins. */

    max_weight = ( ( str_app[get_curr_str( ch )].carry ) -
		 ( ch->money.gold/1000 + ch->money.silver/1000 + ch->money.copper/1000 ) );
    if ( max_weight < 0 )
      max_weight = 0;

   return max_weight;

}

/*
 * See if a string is one of the names of an object.
 * New is_name by Alander.
 */
bool is_name ( CHAR_DATA *ch, char *str, char *namelist )
{
    char name[MAX_INPUT_LENGTH], part[MAX_INPUT_LENGTH];
    char *list, *string;
    bool (*cfun)(const char *astr, const char *bstr);

    /* fix crash on NULL namelist */
    if (namelist == NULL || namelist[0] == '\0')
    	return FALSE;

    /* fixed to prevent is_name on "" returning TRUE */
    if (str[0] == '\0')
	return FALSE;

    string = str;
    if ( ch && ch->desc && ch->desc->original )
      ch = ch->desc->original;
    if ( !ch )
      cfun = &str_cmp;
    else if ( !IS_NPC(ch) && !IS_SET(ch->act, PLR_FULLNAME) )
      cfun = &str_prefix;
    else
      cfun = &str_cmp_ast;
    /* we need ALL parts of string to match part of namelist */
    for ( ; ; )  /* start parsing string */
    {
	str = one_argument(str,part);

	if (part[0] == '\0' )
	    return TRUE;

	/* check to see if this is part of namelist */
	list = namelist;
	for ( ; ; )  /* start parsing namelist */
	{
	    list = one_argument(list,name);
	    if (name[0] == '\0')  /* this name was not found */
		return FALSE;

	    if (!(*cfun)(string,name))
		return TRUE; /* full pattern match */

	    if (!(*cfun)(part,name))
		break;
	}

    }
}

bool is_exact_name(char *str, char *namelist )
{
    char name[MAX_INPUT_LENGTH];

    if (namelist == NULL)
	return FALSE;

    for ( ; ; )
    {
	namelist = one_argument( namelist, name );
	if ( name[0] == '\0' )
	    return FALSE;
	if ( !str_cmp( str, name ) )
	    return TRUE;
    }
}


/*
 * Apply or remove an affect to a character.
 */
void affect_modify( CHAR_DATA *ch, AFFECT_DATA *paf, bool fAdd )
{
    OBJ_DATA *wield;
    char      buf1 [ MAX_STRING_LENGTH ];
    int       mod;
/* XORPHOX */
    AFFECT_DATA af;
    int sn;
    int psn = -1;
    char buf[MAX_STRING_LENGTH];
    buf[0] = '\0';
/* END */

    mod = paf->modifier;

    if ( fAdd )
    {
	SET_BIT   ( ch->affected_by, paf->bitvector );
    }
    else
    {
	REMOVE_BIT( ch->affected_by, paf->bitvector );
	mod = 0 - mod;
    }


    switch ( paf->location )
    {
    default:
        sprintf( buf1, "Affect_modify: unknown location %d on %s.",
		paf->location, ch->name );
	bug ( buf1, 0 );
	return;

    case APPLY_NONE:						break;
    case APPLY_STR:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_str += mod;                         break;
    case APPLY_DEX:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_dex += mod;                         break;
    case APPLY_INT:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_int += mod;                         break;
    case APPLY_WIS:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_wis += mod;                         break;
    case APPLY_CON:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_con += mod;                         break;
    case APPLY_AGI:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_agi += mod;                         break;
    case APPLY_CHA:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_cha += mod;                         break;
    case APPLY_SEX:           ch->sex                   += mod; break;
    case APPLY_PDAMP:         ch->p_damp                += mod; break;
    case APPLY_MDAMP:         ch->m_damp                += mod; break;
    case APPLY_RACE:          ch->race                  += mod; break;
    case APPLY_SIZE:          ch->size                  += mod; break;
    case APPLY_AGE:						break;
    case APPLY_HIT:           ch->mod_hit               += mod; break;
    case APPLY_MANA:          ch->mod_mana              += mod; break;
    case APPLY_MOVE:          ch->mod_move              += mod; break;
    case APPLY_HITROLL:       ch->hitroll               += mod; break;
    case APPLY_DAMROLL:       ch->damroll               += mod; break;

    case APPLY_IMM_HEAT        :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[0]         += mod; break;
    case APPLY_IMM_POSITIVE    :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[1]         += mod; break;
    case APPLY_IMM_COLD        :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[2]         += mod; break;
    case APPLY_IMM_NEGATIVE    :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[3]   += mod; break;
    case APPLY_IMM_HOLY        :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[4]       += mod; break;
    case APPLY_IMM_UNHOLY      :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[5]     += mod; break;
    case APPLY_IMM_REGEN       :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[6]      += mod; break;
    case APPLY_IMM_DEGEN       :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[7]      += mod; break;
    case APPLY_IMM_DYNAMIC     :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[8]    += mod; break;
    case APPLY_IMM_VOID        :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[9]       += mod; break;
    case APPLY_IMM_PIERCE      :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[10]     += mod; break;
    case APPLY_IMM_SLASH       :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[11]      += mod; break;
    case APPLY_IMM_SCRATCH     :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[12]    += mod; break;
    case APPLY_IMM_BASH        :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[13]       += mod; break;
    case APPLY_IMM_INTERNAL    :   if ( !IS_NPC( ch ) )
    ch->pcdata->mimm[14]   += mod; break;
   
   /* This is for thrown objects, has nothing to do with chars */
    case APPLY_THROWPLUS: break;
   
/* XORPHOX perm spells */
      case APPLY_INVISIBLE:
        psn = skill_lookup("invis");
        strcpy(buf, "$n slowly fades into existence.");
      break;
      case APPLY_DETECT_INVIS:
        psn = skill_lookup("detect invis");
      break;
      case APPLY_DETECT_HIDDEN:
        psn = skill_lookup("detect hidden");
      break;
      case APPLY_INFRARED:
        psn = skill_lookup("infravision");
      break;
      case APPLY_HEIGHTEN_SENSES:
        sn = gsn_heighten;
        if(fAdd)
        {
          affect_strip(ch, sn);
          af.type     = sn;
          af.level     = ch->level;          
          af.duration  = mod;
          af.location  = APPLY_NONE;
          af.modifier  = 0;
          af.bitvector = AFF_DETECT_INVIS;
          affect_to_char(ch, &af);
          af.bitvector = AFF_DETECT_HIDDEN;
          affect_to_char(ch, &af);
          af.bitvector = AFF_INFRARED;
          affect_to_char(ch, &af);
          send_to_char(AT_BLUE, "Your senses are heightened.\n\r", ch );
        }
        else if ((IS_AFFECTED(ch, AFF_DETECT_INVIS)) && (IS_AFFECTED(ch, AFF_DETECT_HIDDEN) )
          && (IS_AFFECTED(ch, AFF_INFRARED ) ) )
        {
          affect_strip( ch, sn );
          send_to_char(AT_BLUE, skill_table[sn].msg_off, ch);
          send_to_char(AT_BLUE, "\n\r", ch);
	  if (skill_table[sn].room_msg_off)
           act(AT_BLUE, skill_table[sn].room_msg_off, ch, NULL, NULL, TO_ROOM);
        }
        break;
      case APPLY_SNEAK:
        sn = gsn_sneak;
        if(fAdd)
        {
          affect_strip(ch, sn);
          af.type      = sn;
          af.level     = ch->level;
          af.duration  = mod;
          af.location  = APPLY_NONE;
          af.modifier  = 0;
          af.bitvector = AFF_SNEAK;
          affect_to_char(ch, &af);
          send_to_char(AT_BLUE, "You move silently.\n\r", ch);
        }
        else if(IS_AFFECTED(ch, AFF_SNEAK))
        {
          affect_strip(ch, sn);
          send_to_char(AT_BLUE, skill_table[sn].msg_off, ch);
          send_to_char(AT_BLUE, "\n\r", ch);
          if (skill_table[sn].room_msg_off)
           act(AT_BLUE, skill_table[sn].room_msg_off, ch, NULL, NULL,TO_ROOM);
        }
      break;
      case APPLY_HIDE:
        if(fAdd)
        {
          if(IS_AFFECTED(ch, AFF_HIDE))
          REMOVE_BIT(ch->affected_by, AFF_HIDE);
          SET_BIT(ch->affected_by, AFF_HIDE); 
          act(AT_BLUE, "$n blends into the shadows.\n\r", ch, NULL, NULL, TO_ROOM);
          send_to_char(AT_BLUE, "You blend into the shadows.\n\r", ch);
        }
        else if(IS_AFFECTED(ch, AFF_HIDE))
        {
          REMOVE_BIT(ch->affected_by, AFF_HIDE);
          send_to_char(AT_BLUE, "You fade out of the shadows.\n\r", ch);
        }
      break;
      case APPLY_FLYING:
        psn = skill_lookup("fly");
        strcpy(buf, "$n floats slowly to the ground.");
      break;
      case APPLY_BREATHE_WATER:
        psn = skill_lookup("breathe water");
        strcpy(buf, "$n's gills melt away.");
      break;
      case APPLY_PASS_DOOR:
        psn = skill_lookup("pass door");
      break;
      case APPLY_TEMPORAL_FIELD:
        psn = skill_lookup("temporal field");
      break;
      case APPLY_FIRESHIELD:
        psn = skill_lookup("fireshield");
        strcpy(buf, "The flames about $n's body burn out.");
      break;
      case APPLY_SHOCKSHIELD:
        psn = skill_lookup("shockshield");
        strcpy(buf, "The electricity about $n's body flee's into the ground.");
      break;
      case APPLY_ICESHIELD:
        psn = skill_lookup("iceshield");
        strcpy(buf, "The icy crust about $n's body melts to a puddle.");
      break;
      case APPLY_CHAOS:
        psn = skill_lookup("chaos field");
        strcpy(buf, "The chaos around $n fades away.");
      break;
      case APPLY_SCRY:
        psn = skill_lookup("scry");
      break;
      case APPLY_POISON:
        psn = skill_lookup("poison");
      break;
/* END */
    }

    if(psn != -1)
    {
      if(fAdd && !is_affected(ch, psn))
      {
        obj_cast_spell(psn, paf->level, ch, ch, NULL);
        perm_spell(ch, psn);
      }
      else if(!fAdd && spell_duration(ch, psn) == -1)
      {
        affect_strip(ch, psn);
        if(buf[0] != '\0')
          act(AT_BLUE, buf, ch, NULL, NULL, TO_ROOM);
        if(skill_table[psn].msg_off)
        {
          send_to_char(AT_BLUE, skill_table[psn].msg_off, ch);
          send_to_char(AT_BLUE, "\n\r", ch);
          if(skill_table[psn].room_msg_off)
          {
            act(AT_BLUE, skill_table[psn].room_msg_off, ch, NULL,NULL,TO_ROOM);
          }
        }
      }
    }

    if ( IS_NPC( ch ) )
        return;

    /*
     * Check for weapon wielding.
     * Guard against recursion (for weapons with affects).
     */
    if ( ( wield = get_eq_char( ch, WEAR_WIELD ) )
	&& get_obj_weight( wield ) >
        get_curr_str( ch ) && ch->level < LEVEL_IMMORTAL )
    {
	static int depth;

	if ( depth == 0 )
	{
	    depth++;
	    act(AT_GREY, "You drop $p.", ch, wield, NULL, TO_CHAR );
	    act(AT_GREY, "$n drops $p.", ch, wield, NULL, TO_ROOM );
	    obj_from_char( wield );
	    obj_to_room( wield, ch->in_room );

	    depth--;
	}
    }

    return;
}

void affect_modify2( CHAR_DATA *ch, AFFECT_DATA *paf, bool fAdd )
{
    OBJ_DATA *wield;
    char      buf [ MAX_STRING_LENGTH ];
    int       mod;

    mod = paf->modifier;

    if ( fAdd )
    {
	SET_BIT   ( ch->affected_by2, paf->bitvector );
    }
    else
    {
	REMOVE_BIT( ch->affected_by2, paf->bitvector );
	mod = 0 - mod;
    }

    switch ( paf->location )
    {
    default:
        sprintf( buf, "Affect_modify2: unknown location %d on %s.",
		paf->location, ch->name );
	bug ( buf, 0 );
	return;

    case APPLY_NONE:						break;
    case APPLY_STR:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_str += mod;                         break;
    case APPLY_DEX:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_dex += mod;                         break;
    case APPLY_INT:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_int += mod;                         break;
    case APPLY_WIS:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_wis += mod;                         break;
    case APPLY_CON:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_con += mod;                         break;
    case APPLY_AGI:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_agi += mod;                         break;
    case APPLY_CHA:
	if ( !IS_NPC( ch ) )
	    ch->pcdata->mod_cha += mod;                         break;
    case APPLY_SEX:           ch->sex                   += mod; break;
    case APPLY_PDAMP:         ch->p_damp                += mod; break;
    case APPLY_MDAMP:         ch->m_damp                += mod; break;
    case APPLY_RACE:          ch->race                  += mod; break;
    case APPLY_SIZE:          ch->size                  += mod; break;
    case APPLY_AGE:						break;
    case APPLY_MANA:          ch->mod_mana              += mod; break;
    case APPLY_HIT:           ch->mod_hit               += mod; break;
    case APPLY_MOVE:          ch->mod_move              += mod; break;
    case APPLY_HITROLL:       ch->hitroll               += mod; break;
    case APPLY_DAMROLL:       ch->damroll               += mod; break;

    case APPLY_IMM_HEAT        :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[0]       += mod; break;
    case APPLY_IMM_POSITIVE    :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[1]   += mod; break;
    case APPLY_IMM_COLD        :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[2]       += mod; break;
    case APPLY_IMM_NEGATIVE    :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[3]   += mod; break;
    case APPLY_IMM_HOLY        :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[4]       += mod; break;
    case APPLY_IMM_UNHOLY      :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[5]     += mod; break;
    case APPLY_IMM_REGEN       :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[6]      += mod; break;
    case APPLY_IMM_DEGEN       :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[7]      += mod; break;
    case APPLY_IMM_DYNAMIC     :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[8]    += mod; break;
    case APPLY_IMM_VOID        :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[9]       += mod; break;
    case APPLY_IMM_PIERCE      :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[10]     += mod; break;
    case APPLY_IMM_SLASH       :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[11]      += mod; break;
    case APPLY_IMM_SCRATCH     :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[12]    += mod; break;
    case APPLY_IMM_BASH        :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[13]       += mod; break;
    case APPLY_IMM_INTERNAL    :    if( !IS_NPC( ch ) )
    ch->pcdata->mimm[14]   += mod; break;

   /* This is for thrown objects, has nothing to do with chars */
    case APPLY_THROWPLUS: break;
   }

    if ( IS_NPC( ch ) )
        return;

    /*
     * Check for weapon wielding.
     * Guard against recursion (for weapons with affects).
     */
    if ( ( wield = get_eq_char( ch, WEAR_WIELD ) )
	&& get_obj_weight( wield ) > str_app[get_curr_str( ch )].wield )
    {
	static int depth;

	if ( depth == 0 )
	{
	    depth++;
	    act(AT_GREY, "You drop $p.", ch, wield, NULL, TO_CHAR );
	    act(AT_GREY, "$n drops $p.", ch, wield, NULL, TO_ROOM );
	    obj_from_char( wield );
	    obj_to_room( wield, ch->in_room );
	    depth--;
	}
    }

    return;
}


/*
 * Give an affect to a char.
 */
void affect_to_char( CHAR_DATA *ch, AFFECT_DATA *paf )
{
    AFFECT_DATA *paf_new;

    paf_new = new_affect( );

    *paf_new		= *paf;
    paf_new->deleted    = FALSE;
    paf_new->next	= ch->affected;
    ch->affected	= paf_new;

    affect_modify( ch, paf_new, TRUE );
    return;
}

void affect_to_char2( CHAR_DATA *ch, AFFECT_DATA *paf )
{
    AFFECT_DATA *paf_new;

    paf_new = new_affect();

    *paf_new		= *paf;
    paf_new->deleted    = FALSE;
    paf_new->next	= ch->affected2;
    ch->affected2	= paf_new;

    affect_modify2( ch, paf_new, TRUE );
    return;
}



/*
 * Remove an affect from a char.
 */
void affect_remove( CHAR_DATA *ch, AFFECT_DATA *paf )
{
    if ( paf->deleted )
	return;

    if ( !ch->affected )
    {
	bug( "Affect_remove: no affect.", 0 );
	return;
    }

    if ( paf->bitvector == AFF_FLYING ) 
    {
       affect_modify( ch, paf, FALSE );
       check_nofloor( ch ); 
    }
    else
       affect_modify( ch, paf, FALSE );

    paf->deleted = TRUE;

    return;
}

void affect_remove2( CHAR_DATA *ch, AFFECT_DATA *paf )
{
  if ( paf->deleted )
    return;
  
  if ( !ch->affected2 )
  {
    bug( "Affect_remove2: no affect.", 0 );
    return;
  }

  affect_modify2( ch, paf, FALSE );
  paf->deleted = TRUE;

  return;
}

/*
 * Strip all affects of a given sn.
 */
void affect_strip( CHAR_DATA *ch, int sn )
{
    AFFECT_DATA *paf;
    RACE_DATA   *race_o;
    RACE_DATA   *race_n;
    char        buf[MAX_STRING_LENGTH];

    for ( paf = ch->affected; paf; paf = paf->next )
    {
        if ( paf->deleted )
	    continue;
	if ( paf->type == sn )
	    affect_remove( ch, paf );
    }

    for ( paf = ch->affected2; paf; paf = paf->next )
    {
      if ( paf->deleted )
	continue;

      if ( paf->type == sn )
      {
       if ( paf->bitvector == AFF_POLYMORPH ) 
       {
        sprintf( buf, "%s", ch->name); 
        free_string( ch->oname );
        ch->oname = str_dup(buf);

        free_string( ch->long_descr );
        ch->long_descr = str_dup( "" );

        race_o = get_race_data(ch->race);

        affect_remove2( ch, paf );

        race_n = get_race_data(ch->race);
        polymorph_char( ch, race_o, race_n );
       }
       else
        affect_remove2( ch, paf );
      }
    }

    return;
}



/*
 * Return true if a char is affected by a spell.
 */
bool is_affected( CHAR_DATA *ch, int sn )
{
    AFFECT_DATA *paf;

    for ( paf = ch->affected; paf; paf = paf->next )
    {
        if ( paf->deleted )
	    continue;
	if ( paf->type == sn )
	    return TRUE;
    }
    for ( paf = ch->affected2; paf; paf = paf->next )
    {
      if ( paf->deleted )
	continue;
      if ( paf->type == sn )
	return TRUE;
    }
    return FALSE;
}

/*
 * Add or enhance an affect.
 */
void affect_join( CHAR_DATA *ch, AFFECT_DATA *paf )
{
    AFFECT_DATA *paf_old;
    bool         found;

    found = FALSE;
    for ( paf_old = ch->affected; paf_old; paf_old = paf_old->next )
    {
        if ( paf_old->deleted )
	    continue;
	if ( paf_old->type == paf->type )
	{
	   if ( paf_old->location == paf->location )
           {
              paf->duration += paf_old->duration;
	      paf->modifier += paf_old->modifier;
	      affect_remove( ch, paf_old );
	      break;
           }
	}
    }

    affect_to_char( ch, paf );
    return;
}

void affect_join2( CHAR_DATA *ch, AFFECT_DATA *paf )
{
  AFFECT_DATA *paf_old;
  bool         found;

  found = FALSE;
  for ( paf_old = ch->affected2; paf_old; paf_old = paf_old->next )
  {
    if ( paf_old->deleted )
      continue;
    if ( paf_old->type == paf->type )
    {
      if ( paf_old->location == paf->location )
      {
        paf->duration += paf_old->duration;
        paf->modifier += paf_old->modifier;
        affect_remove2( ch, paf_old );
        break;
      }
    }
  }

  affect_to_char2( ch, paf );
  return;
}

/*
 * Move a char out of a room.
 */
void char_from_room( CHAR_DATA *ch )
{
    char	buf[MAX_STRING_LENGTH];
    OBJ_DATA *obj;
    int      loc = 0;

    if ( !ch )
    {
        sprintf( buf, "Ch_from_room: ch is NULL, name: %s.",
		ch->name );
	bug( buf, 0 );
	return;
    }

    if ( !ch->in_room )
    {
        sprintf( buf, "Ch_from_room: in_room is NULL, name: %s.",
		ch->name );
	bug( buf, 0 );
	return;
    }

    if ( !IS_NPC( ch ) && ch->in_room->area )
	--ch->in_room->area->nplayer;


    for ( obj = get_eq_char( ch, loc ); obj; obj = get_eq_char( ch, ++loc ))
    {
		if ( IS_SET( obj->extra_flags, ITEM_GLOW ) )
                {
		   --ch->in_room->light;
                   break;
 	        }
    }

    if ( ch == ch->in_room->people )
    {
	ch->in_room->people = ch->next_in_room;
    }
    else
    {
	CHAR_DATA *prev;

	for ( prev = ch->in_room->people; prev; prev = prev->next_in_room )
	{
	    if ( prev->next_in_room == ch )
	    {
		prev->next_in_room = ch->next_in_room;
		break;
	    }
	}

	if ( !prev )
        {
         sprintf( buf, "Ch_from_room: ch not found, name: %s.",
		ch->name );
	 bug( buf, 0 );
        }
    }

    ch->in_room      = NULL;
    ch->next_in_room = NULL;
    return;
}


/*
 * Move a char into a room.
 */
void char_to_room( CHAR_DATA *ch, ROOM_INDEX_DATA *pRoomIndex )
{
    OBJ_DATA *obj;
    int      loc = 0;

    if ( !pRoomIndex )
    {
	bug( "Char_to_room: NULL.", 0 );
	pRoomIndex = get_room_index(ROOM_VNUM_LIMBO);
    }

    ch->in_room		= pRoomIndex;
    ch->next_in_room	= pRoomIndex->people;
    pRoomIndex->people	= ch;

    if ( !IS_NPC( ch ) && ch->in_room->area )
	++ch->in_room->area->nplayer;


    for ( obj = get_eq_char( ch, loc ); obj; obj = get_eq_char( ch, ++loc ))
    {
		if ( IS_SET( obj->extra_flags, ITEM_GLOW ) )
                {
		   ++ch->in_room->light;
                   break;
 	        }
    }

    return;
}

void tattoo_to_char( TATTOO_DATA *tattoo, CHAR_DATA *ch, bool login )
{
    AFFECT_DATA *paf;

    tattoo->next_content	 = ch->tattoo;
    ch->tattoo			 = tattoo;
    tattoo->carried_by	 	 = ch;

    if ( login == FALSE ) 
    {
    for ( paf = tattoo->affected; paf; paf = paf->next )
	affect_modify( ch, paf, TRUE );
    }
}

void tattoo_from_char( TATTOO_DATA *tattoo, CHAR_DATA *ch )
{
    AFFECT_DATA *paf;

    if ( ch != tattoo->carried_by ) 
    {
	bug( "Tattoo_from_char: null ch.", 0 );
	return;
    }

    if ( ch->tattoo == tattoo )
    {
	ch->tattoo = tattoo->next_content;
    }
    else
    {
	TATTOO_DATA *prev;

	for ( prev = ch->tattoo; prev; prev = prev->next_content )
	{
	    if ( prev->next_content == tattoo )
	    {
		prev->next_content = tattoo->next_content;
		break;
	    }
	}

	if ( !prev )
	    bug( "Tattoo_from_char: tattoo not in list.", 0 );
    }

    for ( paf = tattoo->affected; paf; paf = paf->next )
	affect_modify( ch, paf, FALSE );

    tattoo->carried_by		 = NULL;
    tattoo->next_content	 = NULL;
    return;
}

/*
 * Give an obj to a char.
 */
void obj_to_char( OBJ_DATA *obj, CHAR_DATA *ch )
{
    obj->next_content	 = ch->carrying;
    ch->carrying	 = obj;
    obj->carried_by	 = ch;
    obj->in_room	 = NULL;
    obj->in_obj		 = NULL;
    obj->stored_by       = NULL;
    ch->carry_number	+= get_obj_number( obj );
    ch->carry_weight	+= get_obj_weight( obj );

	if ( IS_SET( obj->extra_flags, ITEM_GLOW ) )
	++ch->in_room->light;
}

/*
 * -- Altrag
 */
void obj_to_storage( OBJ_DATA *obj, CHAR_DATA *ch )
{
    if ( IS_NPC( ch ) )
    {
      bug( "obj_to_storage: NPC storage from %d", ch->pIndexData->vnum );
      obj_to_char( obj, ch );
      return;
    }
    obj->next_content    = ch->pcdata->storage;
    ch->pcdata->storage  = obj;
    obj->carried_by      = NULL;
    obj->in_room         = NULL;
    obj->in_obj          = NULL;
    obj->stored_by       = ch;
    ch->pcdata->storcount++;
}

/*
 * Take an obj from its character.
 */
void obj_from_char( OBJ_DATA *obj )
{
    CHAR_DATA *ch;

    if ( !( ch = obj->carried_by ) )
    {
	bug( "Obj_from_char: null ch.", 0 );
	return;
    }

    if ( !IS_SET( obj->wear_loc, WEAR_NONE ) )
	unequip_char( ch, obj );

    if ( !IS_SET( obj->bionic_loc, BIONIC_NONE ) )
	unequip_bionic( ch, obj );

    if ( ch->carrying == obj )
    {
	ch->carrying = obj->next_content;
    }
    else
    {
	OBJ_DATA *prev;

	for ( prev = ch->carrying; prev; prev = prev->next_content )
	{
	    if ( prev->next_content == obj )
	    {
		prev->next_content = obj->next_content;
		break;
	    }
	}

	if ( !prev )
	    bug( "Obj_from_char: obj not in list.", 0 );
    }

    obj->carried_by      = NULL;
    obj->next_content	 = NULL;
    ch->carry_number	-= get_obj_number( obj );
    ch->carry_weight	-= get_obj_weight( obj );
    return;
}

/*
 * -- Altrag
 */
void obj_from_storage( OBJ_DATA *obj )
{
  CHAR_DATA *ch;

  if ( !( ch = obj->stored_by ) )
  {
    bug( "obj_from_storage: NULL ch.", 0 );
    return;
  }

  if ( IS_NPC( ch ) )
  {
    bug( "obj_from_storage: NPC storage by %d.", ch->pIndexData->vnum );
    return;
  }

  if ( ch->pcdata->storage == obj )
  {
    ch->pcdata->storage = obj->next_content;
  }
  else
  {
    OBJ_DATA *prev;

    for ( prev = ch->pcdata->storage; prev; prev = prev->next_content )
    {
      if ( prev->next_content == obj )
      {
	prev->next_content = obj->next_content;
	break;
      }
    }

    if ( !prev )
      bug( "Obj_from_storage: obj not in list.", 0 );
  }

  obj->stored_by = NULL;
  obj->next_content = NULL;
  ch->pcdata->storcount--;
  return;
}

TATTOO_DATA *get_tattoo_char( CHAR_DATA *ch, int iWear )
{
    TATTOO_DATA *tattoo;

    for ( tattoo = ch->tattoo; tattoo; tattoo = tattoo->next_content )
    {
        if ( tattoo->deleted )
	    continue;
	if ( tattoo->wear_loc == iWear )
	    return tattoo;
    }

    return NULL;
}

/*
 * Find a piece of eq on a character.
 */
OBJ_DATA *get_eq_char( CHAR_DATA *ch, int iWear )
{
    OBJ_DATA *obj;

    for ( obj = ch->carrying; obj; obj = obj->next_content )
    {
   if ( !obj || obj->deleted )
	    continue;

	if ( IS_SET( obj->wear_loc, iWear ) )
	    return obj;
    }

    return NULL;
}

int equip_char_values( CHAR_DATA *ch, OBJ_DATA *obj, bool fReplace )
{
    char buf[MAX_STRING_LENGTH];
    char roombuf[MAX_STRING_LENGTH];
    int	 iWear = 0;

    strcpy( buf, "" );
    strcpy( roombuf, "" );

    if ( CAN_WEAR( obj, ITEM_WEAR_FINGER ) )
    {
	if ( get_eq_char( ch, WEAR_FINGER_L )
	&&   get_eq_char( ch, WEAR_FINGER_R )
	&&   !remove_obj( ch, WEAR_FINGER_L, fReplace )
	&&   !remove_obj( ch, WEAR_FINGER_R, fReplace ) )
	    return 0;

	if ( !IS_SET( obj->wear_flags, ITEM_DOUBLE ) )
      {
	 if ( !get_eq_char( ch, WEAR_FINGER_L ) )
	 {
	  SET_BIT( iWear, WEAR_FINGER_L );
	  strcat( buf,
	   "You wear $p on your left finger.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on $s left finger.\n\r" );
	 }
       else
	 if ( !get_eq_char( ch, WEAR_FINGER_R ) )
	 {	
	  SET_BIT( iWear, WEAR_FINGER_R );
	  strcat( buf,
	   "You wear $p on your right finger.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on $s right finger.\n\r" );
	 }
      }
      else
      {
	 if ( !get_eq_char( ch, WEAR_FINGER_L ) && 
		!get_eq_char( ch, WEAR_FINGER_R ) )
	 {
	
	  SET_BIT( iWear, WEAR_FINGER_L );
	  SET_BIT( iWear, WEAR_FINGER_R );
	  strcat( buf,
	   "You wear $p on both of your fingers.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on both of $s fingers.\n\r" );
	 }
      }
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_NECK ) )
    {
	if ( !remove_obj( ch, WEAR_NECK, fReplace ) )
	    return 0;
	
	 SET_BIT( iWear, WEAR_NECK );
	  strcat( buf,
	   "You wear $p around your neck.\n\r" );
	  strcat( roombuf,
	   "$n wears $p around $s neck.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_BODY ) )
    {

     if ( obj->value[4] == 0 || obj->value[4] == 3 || obj->value[4] == 7 )
     {
	if ( get_eq_char( ch, WEAR_BODY_1 )
	&&   !remove_obj( ch, WEAR_BODY_1, fReplace ))
	    return 0;

	if ( !get_eq_char( ch, WEAR_BODY_1 ) )
	{
	
       
	 SET_BIT( iWear, WEAR_BODY_1 );
	  strcat( buf,
	   "You put $p on.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on $s body.\n\r" );
	}
     }
     if ( obj->value[4] == 2 || obj->value[4] == 4 || obj->value[4] == 5 )
     {
	if ( get_eq_char( ch, WEAR_BODY_2 )
	&&   !remove_obj( ch, WEAR_BODY_2, fReplace ))
	    return 0;

	if ( !get_eq_char( ch, WEAR_BODY_2 ) )
	{
	
       
	 SET_BIT( iWear, WEAR_BODY_2 );
	  strcat( buf,
	   "You put $p on.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on $s body.\n\r" );
	}
     }
     if ( obj->value[4] == 1 || obj->value[4] == 6 )
     {
	if ( get_eq_char( ch, WEAR_BODY_3 )
	&&   !remove_obj( ch, WEAR_BODY_3, fReplace ))
	    return 0;

	if ( !get_eq_char( ch, WEAR_BODY_3 ) )
	{
	
       
	 SET_BIT( iWear, WEAR_BODY_3 );
	  strcat( buf,
	   "You put $p on.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on $s body.\n\r" );
	}
     }


    }

    if ( CAN_WEAR( obj, ITEM_WEAR_SHEATH ) )
    {
	if ( get_eq_char( ch, WEAR_SHEATH_1 )
	&&   get_eq_char( ch, WEAR_SHEATH_2 )
	&&   !remove_obj( ch, WEAR_SHEATH_1, fReplace )
	&&   !remove_obj( ch, WEAR_SHEATH_2, fReplace ) )
	    return 0;
     
	if ( ( get_eq_char( ch, WEAR_WAIST )) == NULL )
        {
         send_to_char( AT_YELLOW,
    "You can't wear a sheath if you aren't wearing waist armor.\n\r", ch );
         return 0;
        }

	if ( !IS_SET( obj->wear_flags, ITEM_DOUBLE ) )
      {
	 if ( !get_eq_char( ch, WEAR_SHEATH_1 ) )
	 {
        
	
	  SET_BIT( iWear, WEAR_SHEATH_1 );
	  strcat( buf,
	   "You affix $p to your belt.\n\r" );
	  strcat( roombuf,
	   "$n affixes $p to $s belt.\n\r" );
	 }
	 else
	 if ( !get_eq_char( ch, WEAR_SHEATH_2 ) )
	 {
	
        
	  SET_BIT( iWear, WEAR_SHEATH_2 );
	  strcat( buf,
	   "You affix $p to your belt.\n\r" );
	  strcat( roombuf,
	   "$n affixes $p to $s belt.\n\r" );
	 }
      }
      else
      {
	 if ( !get_eq_char( ch, WEAR_SHEATH_1 ) && !get_eq_char( ch, WEAR_SHEATH_2 ) )
	 {
        
	
	  SET_BIT( iWear, WEAR_SHEATH_1 );
	  SET_BIT( iWear, WEAR_SHEATH_2 );
	  strcat( buf,
	   "You affix $p to your belt.\n\r");
	  strcat( roombuf,
	   "$n affixes $p to $s belt.\n\r" );
	 }
      }
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_HEAD ) )
    {
	if ( !remove_obj( ch, WEAR_HEAD, fReplace ) )
	    return 0;
       
	
	 SET_BIT( iWear, WEAR_HEAD );
	  strcat( buf,
	   "You wear $p on your head.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on $s head.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_ORBIT ) )
    {
	if ( !remove_obj( ch, WEAR_ORBIT, fReplace ) )
	    return 0;

       
	
	 SET_BIT( iWear, WEAR_ORBIT );
	  strcat( buf,
	   "You start $p spinning about your head.\n\r" );
	  strcat( roombuf,
	   "$n starts $p spinning around $s head.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_FACE ) )
    {
	if ( !remove_obj( ch, WEAR_ON_FACE, fReplace ) )
	    return 0;
       
	
	 SET_BIT( iWear, WEAR_ON_FACE );
	  strcat( buf,
           "You place $p on your face.\n\r" );
	  strcat( roombuf,
	   "$n places $p on $s face.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_CONTACT ) )
    {
	if ( !remove_obj( ch, WEAR_IN_EYES, fReplace ) )
	    return 0;
       
	 SET_BIT( iWear, WEAR_IN_EYES );
	
	  strcat( buf,
	   "You stick $p into your eyes.\n\r" );
	  strcat( roombuf,
	   "$n sticks $p into $s eyes.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_LEGS ) )
    {
	if ( !remove_obj( ch, WEAR_LEGS, fReplace ) )
	    return 0;
       
	
	 SET_BIT( iWear, WEAR_LEGS );
	  strcat( buf,
	   "You wear $p on your legs.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on $s legs.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_FEET ) )
    {
	if ( !remove_obj( ch, WEAR_FEET, fReplace ) )
	    return 0;
       
	
	 SET_BIT( iWear, WEAR_FEET );
	  strcat( buf,
	   "You wear $p on your feet.\n\r" );
	  strcat( roombuf,
           "$n wears $p on $s feet.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_HANDS ) )
    {
	if ( !remove_obj( ch, WEAR_HANDS, fReplace ) )
	    return 0;
	       
	 SET_BIT( iWear, WEAR_HANDS );
	  strcat( buf,
	   "You wear $p on your hands.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on $s hands.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_ARMS ) )
    {
	if ( !remove_obj( ch, WEAR_ARMS, fReplace ) )
	    return 0;
       
	
	 SET_BIT( iWear, WEAR_ARMS );
	  strcat( buf,
	   "You wear $p on your arms.\n\r" );
	  strcat( roombuf,
	   "$n wears $p on $s arms.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_ABOUT ) )
    {
	if ( !remove_obj( ch, WEAR_ABOUT, fReplace ) )
	    return 0;
       
	
	 SET_BIT( iWear, WEAR_ABOUT );
	  strcat( buf,
	   "You wear $p about your body.\n\r" );
	  strcat( roombuf,
	   "$n wears $p about $s body.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_WAIST ) )
    {
	if ( !remove_obj( ch, WEAR_WAIST, fReplace ) )
	    return 0;
	
	 SET_BIT( iWear, WEAR_WAIST );
	  strcat( buf,
	   "You wear $p about your waist.\n\r" );
	  strcat( roombuf,
	   "$n wears $p about $s waist.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_WRIST ) )
    {
	if ( get_eq_char( ch, WEAR_WRIST_L )
	&&   get_eq_char( ch, WEAR_WRIST_R )
	&&   !remove_obj( ch, WEAR_WRIST_L, fReplace )
	&&   !remove_obj( ch, WEAR_WRIST_R, fReplace ) )
	    return 0;

	if ( !IS_SET( obj->wear_flags, ITEM_DOUBLE ) )
      {
 	 if ( !get_eq_char( ch, WEAR_WRIST_L ) )
	 {
	
	  SET_BIT( iWear, WEAR_WRIST_L );
	  strcat( buf,
	   "You wear $p around your left wrist.\n\r" );
	  strcat( roombuf,
	   "$n wears $p around $s left wrist.\n\r" );
	 }
	 else
	 if ( !get_eq_char( ch, WEAR_WRIST_R ) )
	 {
	
	  SET_BIT( iWear, WEAR_WRIST_R );
	  strcat( buf,
	    "You wear $p around your right wrist.\n\r" );
	  strcat( roombuf,
	   "$n wears $p around $s right wrist.\n\r" );
	 }
      }
      else
      {
 	 if ( !get_eq_char( ch, WEAR_WRIST_L ) &&
            !get_eq_char( ch, WEAR_WRIST_R ) )
	 {
	
	  SET_BIT( iWear, WEAR_WRIST_L );
	  SET_BIT( iWear, WEAR_WRIST_R );
	  strcat( buf,
	   "You wear $p around both of your wrists.\n\r" );
	  strcat( roombuf,
	   "$n wears $p around both of $s wrists.\n\r" );
	 }
      }
    }

    if ( CAN_WEAR( obj, ITEM_WEAR_SHIELD ) )
    {
	if ( !remove_obj( ch, WEAR_SHIELD, fReplace ) )
	    return 0;

	
	 SET_BIT( iWear, WEAR_SHIELD );
	 strcat( buf,
	  "You wear $p as a shield.\n\r" );
	 strcat( roombuf,
	  "$n wears $p as a shield.\n\r" );
    }

    if ( CAN_WEAR( obj, ITEM_WIELD ) )
    {
     /* You see, do_wield calls in with freplace while dual calls
	with it being false, thats how we tell the difference -Flux */

     if ( fReplace )
     {
	if ( get_eq_char( ch, WEAR_WIELD )
	&&   !remove_obj( ch, WEAR_WIELD, TRUE ) )
	    return 0;
     }
     else
     {
	if ( get_eq_char( ch, WEAR_WIELD_2 )
	&&   !remove_obj( ch, WEAR_WIELD_2, TRUE ) )
	    return 0;
     }

    if ( !IS_SET( obj->wear_flags, ITEM_DOUBLE ) )
    {
     if ( !fReplace )
     {
	 strcat( buf,
          "You dual wield $p.\n\r");

      if ( !IS_NPC( ch ) &&
          (obj->item_type == ITEM_WEAPON ||
            obj->item_type == ITEM_RANGED_WEAPON) )
      {
       if ( obj->value[8] != WEAPON_BLADE )
       {
        if ( ch->pcdata->weapon[obj->value[8]] <= 25 )
	 strcat( buf,
          "You fumble around with $p.\n\r" );
        else if ( ch->pcdata->weapon[obj->value[8]] >= 26 &&
                  ch->pcdata->weapon[obj->value[8]] <= 74 )
	 strcat( buf,
          "You feel ok wielding $p.\n\r" );
        else
	 strcat( buf,
          "You wield $p with confidence.\n\r" );
       }
       else
       {
        if ( ch->pcdata->weapon[obj->value[7]] <= 25 )
	 strcat( buf,
          "You fumble around with $p.\n\r" );
        else if ( ch->pcdata->weapon[obj->value[7]] >= 26 &&
                  ch->pcdata->weapon[obj->value[7]] <= 74 )
	 strcat( buf,
          "You feel ok wielding $p.\n\r" );
        else
	 strcat( buf,
          "You wield $p with confidence.\n\r" );
       }
      }
	 strcat( roombuf,
          "$n dual wields $p.\n\r" );
	
	 SET_BIT( iWear, WEAR_WIELD_2 );
   }
   else
   {
	 strcat( buf,
	  "You wield $p.\n\r" );

      if ( !IS_NPC( ch ) &&
          (obj->item_type == ITEM_WEAPON ||
            obj->item_type == ITEM_RANGED_WEAPON) )
      {
       if ( obj->value[8] != WEAPON_BLADE )
       {
        if ( ch->pcdata->weapon[obj->value[8]] <= 25 )
	 strcat( buf,
          "You fumble around with $p.\n\r" );
        else if ( ch->pcdata->weapon[obj->value[8]] >= 26 &&
                  ch->pcdata->weapon[obj->value[8]] <= 74 )
	 strcat( buf,
          "You feel ok wielding $p.\n\r" );
        else
	 strcat( buf,
          "You wield $p with confidence.\n\r" );
       }
       else 
       {
        if ( ch->pcdata->weapon[obj->value[7]] <= 25 )
	 strcat( buf,
          "You fumble around with $p.\n\r" );
        else if ( ch->pcdata->weapon[obj->value[7]] >= 26 &&
                  ch->pcdata->weapon[obj->value[7]] <= 74 )
	 strcat( buf,
          "You feel ok wielding $p.\n\r" );
        else
	 strcat( buf,
          "You wield $p with confidence.\n\r" );
       }
      }
	 strcat( roombuf,
	  "$n wields $p.\n\r" );
	
	 SET_BIT( iWear, WEAR_WIELD );
     }
    }
    else
    {
     if ( get_eq_char( ch, WEAR_WIELD )
      &&   !remove_obj( ch, WEAR_WIELD, TRUE ) )
      return 0;

     if ( get_eq_char( ch, WEAR_WIELD_2 )
      &&   !remove_obj( ch, WEAR_WIELD_2, TRUE ) )
      return 0;

      strcat( buf, "You wield $p with two hands.\n\r" );

      if ( !IS_NPC( ch ) &&
          (obj->item_type == ITEM_WEAPON ||
            obj->item_type == ITEM_RANGED_WEAPON) )
      {
       if ( obj->value[8] != WEAPON_BLADE )
       {
        if ( ch->pcdata->weapon[obj->value[8]] <= 25 )
	 strcat( buf,
          "You fumble around with $p.\n\r" );
        else if ( ch->pcdata->weapon[obj->value[8]] >= 26 &&
                  ch->pcdata->weapon[obj->value[8]] <= 74 )
	 strcat( buf,
          "You feel ok wielding $p.\n\r" );
        else
	 strcat( buf,
          "You wield $p with confidence.\n\r" );
       }
       else 
       {
        if ( ch->pcdata->weapon[obj->value[7]] <= 25 )
	 strcat( buf,
          "You fumble around with $p.\n\r" );
        else if ( ch->pcdata->weapon[obj->value[7]] >= 26 &&
                  ch->pcdata->weapon[obj->value[7]] <= 74 )
	 strcat( buf,
          "You feel ok wielding $p.\n\r" );
        else
	 strcat( buf,
          "You wield $p with confidence.\n\r" );
       }
      }

     strcat( roombuf, "$n wields $p in both hands.\n\r" );
     SET_BIT( iWear, WEAR_WIELD );
     SET_BIT( iWear, WEAR_WIELD_2 );
   }
  }
    if ( CAN_WEAR( obj, ITEM_WEAR_EARS ) )
    {
        if ( get_eq_char( ch, WEAR_EAR_L )
        &&   get_eq_char( ch, WEAR_EAR_R )
        &&   !remove_obj( ch, WEAR_EAR_L, fReplace )
        &&   !remove_obj( ch, WEAR_EAR_R, fReplace ) )
            return 0;

      if ( !IS_SET( obj->wear_flags, ITEM_DOUBLE ) )
      {
 	 if ( !get_eq_char( ch, WEAR_EAR_L ) )
	 {
	
	  SET_BIT( iWear, WEAR_EAR_L );
	 strcat( buf,
          "You place $p on your left ear.\n\r" );
	 strcat( roombuf,
	  "$n places $p on $s left ear.\n\r" );
	 }
	 else
	 if ( !get_eq_char( ch, WEAR_EAR_R ) )
	 {
	
	  SET_BIT( iWear, WEAR_EAR_R );
	 strcat( buf,
          "You place $p on your right ear.\n\r" );
	 strcat( roombuf,
          "$n places $p on $s right ear.\n\r" );
	 }
      }
      else
      {
       if ( !get_eq_char( ch, WEAR_EAR_L ) && !get_eq_char( ch, WEAR_EAR_R ) )
       {
	
	  SET_BIT( iWear, WEAR_EAR_L );
	  SET_BIT( iWear, WEAR_EAR_R );
	 strcat( buf,
          "You place $p on both of your ears.\n\r" );
	 strcat( roombuf,
          "$n places $p on both of $s ears.\n\r" );
	 }
      }
    }


    if ( CAN_WEAR( obj, ITEM_WEAR_ANKLE ) )
    {
        if ( get_eq_char( ch, WEAR_ANKLE_L )
	&&   get_eq_char( ch, WEAR_ANKLE_R )
	&&   !remove_obj( ch, WEAR_ANKLE_L, fReplace )
	&&   !remove_obj( ch, WEAR_ANKLE_R, fReplace ) )
	    return 0;

	if ( !IS_SET( obj->wear_flags, ITEM_DOUBLE ) )
      {
	 if ( !get_eq_char( ch, WEAR_ANKLE_L ) )
	 {
	
	  SET_BIT( iWear, WEAR_ANKLE_L );
	strcat( buf,
          "You wear $p on your left ankle.\n\r" );
	strcat( roombuf,
	  "$n wears $p on $s left ankle.\n\r" );
	 }
	 else
	 if ( !get_eq_char( ch, WEAR_ANKLE_R ) )
	 {
	
	  SET_BIT( iWear, WEAR_ANKLE_R);
	strcat( buf,
          "You wear $p on your right ankle.\n\r" );
	strcat( roombuf,
          "$n wears $p on $s right ankle.\n\r" );
	 }
      }
      else
      {
	 if ( !get_eq_char( ch, WEAR_ANKLE_L ) && 
            !get_eq_char( ch, WEAR_ANKLE_R ) )
	 {
	
	  SET_BIT( iWear, WEAR_ANKLE_L );
	  SET_BIT( iWear, WEAR_ANKLE_R);
	strcat( buf,
          "You wear $p on both of your ankles.\n\r" );
	strcat( roombuf,
          "$n wears $p on both of $s ankles.\n\r" );
	 }
      }
    }

    if ( CAN_WEAR( obj, ITEM_MEDALLION ) )
    {
     if ( !remove_obj( ch, WEAR_MEDALLION, fReplace ) )
      return 0;
     
	if ( ( get_eq_char( ch, WEAR_BODY_3 )) == NULL &&
             ( get_eq_char( ch, WEAR_BODY_2 )) == NULL &&
             ( get_eq_char( ch, WEAR_BODY_1 )) == NULL )
        {
         send_to_char( AT_YELLOW,
    "You can't wear a medallion if you aren't wearing body armor.\n\r", ch );
         return 0;
        }
	
	 SET_BIT( iWear, WEAR_MEDALLION );
	strcat( buf,
         "You affix $p to your body armor.\n\r" );
	strcat( roombuf,
         "$n attaches $p to $s body armor.\n\r" );
    }

 act( AT_BLUE, buf, ch, obj, NULL, TO_CHAR );
 act( AT_BLUE, roombuf, ch, obj, NULL, TO_ROOM );

 return iWear;
}

/*
 * Equip a char with an obj.
 */
void equip_char( CHAR_DATA *ch, OBJ_DATA *obj, bool fReplace )
{
    AFFECT_DATA *paf;
    int	     wear_loc;

    wear_loc = equip_char_values( ch, obj, fReplace );

    if ( wear_loc == 0 )
    {
     if (fReplace)
      send_to_char(AT_BLUE, "You can't wear that.\n\r", ch );
     return;
    }

    SET_BIT( obj->wear_loc, wear_loc );

    if ( IS_SET( obj->wear_loc, WEAR_NONE) )
     REMOVE_BIT( obj->wear_loc, WEAR_NONE );

    for ( paf = obj->pIndexData->affected; paf; paf = paf->next )
	affect_modify( ch, paf, TRUE );
    for ( paf = obj->affected; paf; paf = paf->next )
	affect_modify( ch, paf, TRUE );

	if ( IS_SET( obj->extra_flags, ITEM_GLOW ) )
	++ch->in_room->light;

    ch->carry_number -= get_obj_number( obj );
    oprog_wear_trigger( obj, ch );

 return;
}



/*
 * Unequip a char with an obj.
 */
void unequip_char( CHAR_DATA *ch, OBJ_DATA *obj )
{
    AFFECT_DATA *paf;
    char buf [ MAX_STRING_LENGTH ];


    if ( IS_SET( obj->wear_loc, WEAR_NONE ) )
    {
        sprintf( buf, "Unequip_char: %s already unequipped with %d.",
		ch->name, obj->pIndexData->vnum );
	bug( buf, 0 );
	return;
    }

    REMOVE_BIT( obj->wear_loc, obj->wear_loc );
    SET_BIT( obj->wear_loc, WEAR_NONE);

    for ( paf = obj->pIndexData->affected; paf; paf = paf->next )
	affect_modify( ch, paf, FALSE );
    for ( paf = obj->affected; paf; paf = paf->next )
	affect_modify( ch, paf, FALSE );

	if ( IS_SET( obj->extra_flags, ITEM_GLOW ) )
	--ch->in_room->light;

    ch->carry_number += get_obj_number( obj );
    return;
}

/*
 * Count occurrences of an obj in a list.
 */
int count_obj_list( OBJ_INDEX_DATA *pObjIndex, OBJ_DATA *list )
{
    OBJ_DATA *obj;
    int       nMatch;

    nMatch = 0;
    for ( obj = list; obj; obj = obj->next_content )
    {
   if ( !obj || obj->deleted )
	    continue;
	if ( obj->pIndexData == pObjIndex )
	    nMatch++;
    }

    return nMatch;
}



/*
 * Move an obj out of a room.
 */
void obj_from_room( OBJ_DATA *obj )
{
    CHAR_DATA *rch;
    ROOM_INDEX_DATA *in_room;

    if ( !( in_room = obj->in_room ) )
    {
	bug( "obj_from_room: NULL.", 0 );
	return;
    }

    if ( obj == in_room->contents )
    {
	in_room->contents = obj->next_content;
    }
    else
    {
	OBJ_DATA *prev;

	for ( prev = in_room->contents; prev; prev = prev->next_content )
	{
	    if ( prev->next_content == obj )
	    {
		prev->next_content = obj->next_content;
		break;
	    }
	}

	if ( !prev )
	{
	    bug( "Obj_from_room: obj not found.", 0 );
	    return;
	}
    }


    for ( rch = obj->in_room->people; rch; rch = rch->next_in_room )
    {
	if ( rch->deleted )
	    continue;

	if ( rch->resting_on == obj )
        {
         act( AT_LBLUE, "You fall off of $p.", rch, obj, NULL, TO_CHAR );
         act( AT_LBLUE, "$n falls off of $p.", rch, obj, NULL, TO_ROOM );
	    rch->resting_on = NULL;
        }
    }

    obj->in_room      = NULL;
    obj->next_content = NULL;
    return;
}



/*
 * Move an obj into a room.
 */
void obj_to_room( OBJ_DATA *obj, ROOM_INDEX_DATA *pRoomIndex )
{
 if ( pRoomIndex == NULL )
  pRoomIndex = get_room_index( 1 );

    if ( pRoomIndex->vnum > 32000 )
       pRoomIndex->vnum = 1;
    obj->next_content		= pRoomIndex->contents;
    pRoomIndex->contents	= obj;
    obj->in_room		= pRoomIndex;
    obj->carried_by		= NULL;
    obj->in_obj			= NULL;
    obj->stored_by              = NULL;
    return;
}


/*
 * Move a bomb into a room. (traps) -Flux.
 */
void bomb_to_room( OBJ_DATA *obj, ROOM_INDEX_DATA *pRoomIndex )
{
    if ( pRoomIndex->vnum > 32000 )
       pRoomIndex->vnum = 1;
    obj->next_content		= pRoomIndex->trap_bomb;
    pRoomIndex->trap_bomb	= obj;
    obj->in_room		= pRoomIndex;
    obj->carried_by		= NULL;
    obj->in_obj			= NULL;
    obj->stored_by              = NULL;
    return;
}

/*
 * Move an object into an object.
 */
void obj_to_obj( OBJ_DATA *obj, OBJ_DATA *obj_to )
{
    if ( !obj || obj->deleted )
    {
	bug( "Obj_to_obj:  Obj already deleted", 0 );
        return;
    }

    if ( obj_to->deleted )
    {
	bug( "Obj_to_obj:  Obj_to already deleted", 0 );
        return;
    }

    obj->next_content		= obj_to->contains;
    obj_to->contains		= obj;
    obj->in_obj			= obj_to;
    obj->in_room		= NULL;
    obj->carried_by		= NULL;
    obj->stored_by              = NULL;

    for ( ; obj_to; obj_to = obj_to->in_obj )
    {
        if ( obj_to->deleted )
            continue;
	if ( obj_to->carried_by )
   	{
            obj_to->carried_by->carry_weight += get_obj_weight( obj );
	    break;
	}
    }
    return;
}

/*
 * Move an object out of an object.
 */
void obj_from_obj( OBJ_DATA *obj, bool sheath )
{
    OBJ_DATA *obj_from;

    if ( !( obj_from = obj->in_obj ) )
    {
	bug( "Obj_from_obj: null obj_from.", 0 );
	return;
    }

    
   if ( (sheath) )
   {
    if ( obj == obj_from->sheath )
     obj_from->sheath = NULL;
    else
    {
     bug( "Obj_from_obj (sheath): obj not found.", 0 );
     return;
    }

    obj->in_obj       = NULL;

    for ( ; obj_from; obj_from = obj_from->in_obj )
    {
        if ( obj_from->deleted )
	    continue;
	if ( obj_from->carried_by )
	{
            obj_from->carried_by->carry_weight -= get_obj_weight( obj );
	    break;
	}
    }

    return;
   }

    if ( obj == obj_from->contains )
    {
	obj_from->contains = obj->next_content;
    }
    else
    {
	OBJ_DATA *prev;

	for ( prev = obj_from->contains; prev; prev = prev->next_content )
	{
	    if ( prev->next_content == obj )
	    {
		prev->next_content = obj->next_content;
		break;
	    }
	}

	if ( !prev )
	{
	    bug( "Obj_from_obj: obj not found.", 0 );
	    return;
	}
    }

    obj->next_content = NULL;
    obj->in_obj       = NULL;

    for ( ; obj_from; obj_from = obj_from->in_obj )
    {
        if ( obj_from->deleted )
	    continue;
	if ( obj_from->carried_by )
	{
            obj_from->carried_by->carry_weight -= get_obj_weight( obj );
	    break;
	}
    }

    return;
}

/*
 * Move an object into a sheath.
 */
void obj_to_sheath( OBJ_DATA *obj, OBJ_DATA *obj_to )
{
    if ( !obj || obj->deleted )
    {
	bug( "Obj_to_obj:  Obj already deleted", 0 );
        return;
    }

    if ( obj_to->deleted )
    {
	bug( "Obj_to_obj:  Obj_to already deleted", 0 );
        return;
    }

    obj_to->sheath		= obj;
    obj->in_obj			= obj_to;
    obj->in_room		= NULL;
    obj->carried_by		= NULL;
    obj->stored_by              = NULL;

    for ( ; obj_to; obj_to = obj_to->in_obj )
    {
        if ( obj_to->deleted )
            continue;
	if ( obj_to->carried_by )
   	{
            obj_to->carried_by->carry_weight += get_obj_weight( obj );
	    break;
	}
    }
    return;
}

/*
 * Extract an obj from the world.
 */
void extract_obj( OBJ_DATA *obj )
{
  OBJ_DATA *obj_content;
  OBJ_DATA *obj_next;
  extern bool delete_obj;

  if ( !obj || obj->deleted )
  {
    bug( "Extract_obj:  Obj already deleted", 0 );
    return;
  }

  if ( obj->in_room    )
    obj_from_room( obj );
  else if ( obj->carried_by )
    obj_from_char( obj );
  else if ( obj->in_obj )
  {
   if ( (obj->in_obj->sheath) )
    obj_from_obj( obj, TRUE );

   if ( obj->in_obj )
    obj_from_obj( obj, FALSE );
  }
  else if ( obj->stored_by )
    obj_from_storage( obj );

    for ( obj_content = obj->contains; obj_content; obj_content = obj_next )
    {
        obj_next = obj_content->next_content;
	if( obj_content->deleted )
	    continue;
	extract_obj( obj_content );
    }

    if ( obj->sheath )
     extract_obj( obj->sheath );

    obj->deleted = TRUE;

    delete_obj   = TRUE;

    return;
}

void extract_tattoo( TATTOO_DATA *tattoo )
{
  extern bool      delete_tattoo;

  if ( tattoo->deleted )
  {
    bug( "Extract_tattoo:  Tattoo already deleted", 0 );
    return;
  }

  if ( tattoo->carried_by )
    tattoo_from_char( tattoo, tattoo->carried_by );

    tattoo->deleted = TRUE;
    delete_tattoo   = TRUE;
    return;
}

/*
 * Extract a char from the world.
 */
void extract_char( CHAR_DATA *ch, bool fPull )
{
           CHAR_DATA *wch;
           OBJ_DATA  *obj;
           OBJ_DATA  *obj_next;
    extern bool       delete_char;
    	   bool       is_arena = IS_ARENA(ch);

    if ( !ch->in_room )
     ch->in_room = get_room_index(2);

    if ( fPull )
    {
	char* name;

	if ( IS_NPC ( ch ) )
	    name = ch->short_descr;
	else
	    name = ch->name;

	die_follower( ch, name );
    }

    stop_fighting( ch, TRUE );

    if ( fPull || !is_arena )
     for ( obj = ch->carrying; obj; obj = obj_next )
     {
      obj_next = obj->next_content;
      if ( !obj || obj->deleted )
       continue;
      extract_obj( obj );
     }

    if ( is_arena )
    {
      arena.fch = NULL;
      arena.sch = NULL;
      arena.award = 0;
    }
    
    char_from_room( ch );

    if ( !fPull )
    {
        ROOM_INDEX_DATA *location = NULL;

	if ( is_arena && (location = get_room_index(ROOM_ARENA_HALL_SHAME)) )
	  char_to_room(ch, location);
	else if ( ch->race == RACE_HUMAN && 
         !( location = get_room_index( ROOM_HUMAN_MORGUE ) ) )
	{
	 bug( "Morgue does not exist!", 0 );
	 char_to_room( ch, get_room_index( ROOM_HUMAN_TEMPLE ) );
	}
	else if ( ch->race != RACE_HUMAN && 
         !( location = get_room_index( ROOM_ELF_MORGUE ) ) )
	{
	 bug( "Morgue does not exist!", 0 );
	 char_to_room( ch, get_room_index( ROOM_ELF_TEMPLE ) );
	}
	else
	{
	 char buf [ MAX_INPUT_LENGTH ];
	 char_to_room( ch, location );
	 sprintf( buf, "You awaken in the morgue%s",
	  ch->level <= 20 ? ", your battered corpse next to you.\n\r" :
          ".\n\r" );
	 send_to_char( AT_BLUE, buf, ch );
	}
	return;
    }

    if ( IS_NPC( ch ) )
	--ch->pIndexData->count;

    if ( ch->desc && ch->desc->original )
	do_return( ch, "" );

    for ( wch = char_list; wch; wch = wch->next )
    {
	if ( wch->reply == ch )
	    wch->reply = NULL;
	if ( wch->hunting == ch )
	    wch->hunting = NULL;
    }

    ch->deleted = TRUE;

    if ( ch->desc )
	ch->desc->character = NULL;

    if (IS_NPC ( ch ) )
       mobs_in_game--;

    delete_char = TRUE;
    return;
}

/*
 * Find a char in the room.
 */
CHAR_DATA *get_char_room( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *rch;
    char       arg [ MAX_INPUT_LENGTH ];
    int        number;
    int        count;

    number = number_argument( argument, arg );
    count  = 0;
    if ( !str_cmp( arg, "self" ) )
	return ch;
    for ( rch = ch->in_room->people; rch; rch = rch->next_in_room )
    {
	if ( !can_see( ch, rch ) || !is_name( ch, arg, rch->name ) )
	    continue;
	if ( ++count == number )
	    return rch;
    }

    return NULL;
}

/* This bit of nastiness returns the closest instance of a char.
   The only reason we need this is because there can be two foxes
   in the world, and who the hell knows with get_char_world
   which one is actually the one right next to you? -Flux */ 
CHAR_DATA *get_char_closest( CHAR_DATA *ch, int dir, char *argument )
{
    CHAR_DATA *wch;
    CHAR_DATA *rch;
    CHAR_DATA *cch;
    char namechoice[MAX_STRING_LENGTH];
    int choice;

    sprintf( namechoice, "%s", argument );

    if ( ( cch = get_char_room( ch, argument ) ) )
     return cch;

    for ( choice = 0; choice < 11; choice++ )
    {
     if ( choice > 0 )
      sprintf( namechoice, "%d.%s", choice, argument );

     if ( !(wch = get_char_world(ch, namechoice)) )
      break;
     
     if ( distancebetween( ch, wch, dir ) == -1 )
      continue;

     rch = wch;

     if ( distancebetween( ch, wch, dir ) >
           distancebetween( ch, rch, dir ) )
      cch = rch;
     else
      cch = wch;
    }

   if ( (cch) )
    return cch;
   else
    return NULL;
}

/*
 * Find a char in the world.
 */
CHAR_DATA *get_char_world( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *wch;
    char       arg [ MAX_INPUT_LENGTH ];
    int        number;
    int        count;

    if ( ( wch = get_char_room( ch, argument ) ) )
	return wch;

    number = number_argument( argument, arg );
    count  = 0;
    for ( wch = char_list; wch ; wch = wch->next )
    {
	if ( !can_see( ch, wch ) || !is_name( ch, arg, wch->name ) )
	    continue;
	if ( ++count == number )
	    return wch;
    }

    return NULL;
}

/*
 * Find a PC in the world.
 */
CHAR_DATA *get_pc_world( CHAR_DATA *ch, char *argument )
{
  DESCRIPTOR_DATA *d;
  for ( d = descriptor_list; d; d = d->next )
	{
	if ( d->connected == CON_PLAYING
	&& is_name( ch, argument, d->character->name ) 
	&& can_see( ch, d->character ) )
		return d->character;
	}
  return NULL;
}

/*
 * Find some object with a given index data.
 * Used by area-reset 'P' command.
 */
OBJ_DATA *get_obj_type( OBJ_INDEX_DATA *pObjIndex )
{
    OBJ_DATA *obj;

    for ( obj = object_list; obj; obj = obj->next )
    {
      if ( !obj || obj->deleted )
	    continue;

	if ( obj->pIndexData == pObjIndex )
	    return obj;
    }

    return NULL;
}


/*
 * Find an obj in a list.
 */
OBJ_DATA *get_obj_list( CHAR_DATA *ch, char *argument, OBJ_DATA *list )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    int       number;
    int       count;

    number = number_argument( argument, arg );
    count  = 0;
    for ( obj = list; obj; obj = obj->next_content )
    {
	if ( can_see_obj( ch, obj ) && is_name( ch, arg, obj->name ) )
	{
	    if ( ++count == number )
		return obj;
	}
    }

    return NULL;
}


/*
 * Find an obj in player's inventory.
 */
OBJ_DATA *get_obj_carry( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    int       number;
    int       count;

    number = number_argument( argument, arg );
    count  = 0;
    for ( obj = ch->carrying; obj; obj = obj->next_content )
    {
	if ( IS_SET( obj->wear_loc, WEAR_NONE )
            && obj->bionic_loc == -1
	    && can_see_obj( ch, obj )
	    && is_name( ch, arg, obj->name ) )
	{
	    if ( ++count == number )
		return obj;
	}
    }

    return NULL;
}

OBJ_DATA *get_obj_storage( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *obj;
  char arg[MAX_INPUT_LENGTH];
  int number;
  int count;

  if ( IS_NPC( ch ) )
  {
    bug( "get_obj_storage: NPC storage from %d", ch->pIndexData->vnum );
    return NULL;
  }

  number = number_argument( argument, arg );
  count = 0;

  for ( obj = ch->pcdata->storage; obj; obj = obj->next_content )
  {
    if ( can_see_obj( ch, obj )
	 && is_name( ch, arg, obj->name ) )
    {
      if ( ++count == number )
	return obj;
    }
  }
  return NULL;
}

/*
 * Find an obj in player's equipment.
 */
OBJ_DATA *get_obj_wear( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    int       number;
    int       count;

    number = number_argument( argument, arg );
    count  = 0;
    for ( obj = ch->carrying; obj; obj = obj->next_content )
    {
	if ( !IS_SET( obj->wear_loc, WEAR_NONE)
	    && can_see_obj( ch, obj )
	    && is_name( ch, arg, obj->name ) )
	{
	    if ( ++count == number )
		return obj;
	}
    }

    return NULL;
}

/*
 * Find an obj in the room or in inventory.
 */
OBJ_DATA *get_obj_here( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;

    obj = get_obj_list( ch, argument, ch->in_room->contents );
    if ( obj )
	return obj;

    if ( ( obj = get_obj_carry( ch, argument ) ) )
	return obj;

    if ( ( obj = get_obj_wear( ch, argument ) ) )
	return obj;

    return NULL;
}

/*
 * Find an obj in the world.
 */
OBJ_DATA *get_obj_world( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    int       number;
    int       count;

    if ( ( obj = get_obj_here( ch, argument ) ) )
	return obj;

    number = number_argument( argument, arg );
    count  = 0;
    for ( obj = object_list; obj; obj = obj->next )
    {
	if ( can_see_obj( ch, obj ) && is_name( ch, arg, obj->name ) )
	{
	    if ( ++count == number )
		return obj;
	}
    }

    return NULL;
}

/*
 * Create a 'money' obj.
 */
OBJ_DATA *create_money( MONEY_DATA *amount )
{
    OBJ_DATA *obj;

    if (  ( ( amount->gold <= 0 ) && ( amount->silver <= 0 ) && ( amount->copper <= 0 ) ) ||
	  ( amount->gold < 0 ) || ( amount->silver < 0 ) || ( amount->copper < 0 )  )
    {
       char buf [ MAX_STRING_LENGTH ];
       sprintf( buf, "Create_money: zero or negative money %d %d %d.", amount->gold, amount->silver, amount->copper );
       bug( buf, 0 );
       amount->copper = 1;
       amount->gold = 0;
       amount->silver = 0;
    }
    if ( ( amount->gold == 1 ) && ( amount->silver <= 0 ) && ( amount->copper <= 0 ) )
        obj = create_object( get_obj_index( OBJ_VNUM_MONEY_ONE  ), 0 );
    else 
    if ( ( amount->silver == 1 ) && ( amount->gold <= 0 ) && ( amount->copper <= 0 ) )
	obj = create_object( get_obj_index( OBJ_VNUM_MONEY_ONE  ), 0 );
    else
    if ( ( amount->copper == 1 ) && ( amount->gold <= 0 ) && ( amount->silver <= 0 ) )
 	obj = create_object( get_obj_index( OBJ_VNUM_MONEY_ONE  ), 0 );
    else
 	obj = create_object( get_obj_index( OBJ_VNUM_MONEY_SOME ), 0 );    
    
    obj->value[0]               = amount->gold;
    obj->value[1]		= amount->silver;
    obj->value[2]		= amount->copper;

    return obj;  

}

/*
 * Return # of objects which an object counts as.
 * Thanks to Tony Chamberlain for the correct recursive code here.
 */
int get_obj_number( OBJ_DATA *obj )
{
    int number;

    number = 0;
	number = 1;

    return number;
}

/*
 * Return weight of an object, including weight of contents.
 */
int get_obj_weight( OBJ_DATA *obj )
{
    int weight;
    

    weight = obj->weight;
    for ( obj = obj->contains; obj; obj = obj->next_content )
    {
      if ( !obj || obj->deleted )
	    continue;
	weight += get_obj_weight( obj );
    }

    return weight;
}

/*
 * True if room is dark.
 */
bool room_is_dark( ROOM_INDEX_DATA *pRoomIndex )
{
    OBJ_DATA *obj;
    CHAR_DATA *rch;

    if ( pRoomIndex == NULL )
    {
      bug( "pRoomIndex equal to NULL", 0 );
      return TRUE; 
    }

    if ( pRoomIndex->light > 0 )
	return FALSE;

    for ( obj = pRoomIndex->contents; obj; obj = obj->next_content )
    {
      if ( !obj || obj->deleted )
	    continue;

	if ( IS_SET( obj->extra_flags, ITEM_GLOW ) )
	    return FALSE;
    }

    for ( rch = pRoomIndex->people; rch; rch = rch->next_in_room )
    {
	if ( rch->deleted )
	    continue;

     for ( obj = rch->carrying; obj; obj = obj->next_content )
     {
      if ( !obj || obj->deleted )
	    continue;

	if ( IS_SET( obj->extra_flags, ITEM_GLOW ) )
	    return FALSE;
     }

    }

    if ( IS_SET( pRoomIndex->room_flags, ROOM_DARK ) )
	return TRUE;

    if ( time_info.sunlight == SUN_DUSK
	|| time_info.sunlight == SUN_DARK )
	return TRUE;

    return FALSE;
}

/*
 * True if room is private.
 */
bool room_is_private( ROOM_INDEX_DATA *pRoomIndex )
{
    CHAR_DATA *rch;
    int        count;

    count = 0;
    for ( rch = pRoomIndex->people; rch; rch = rch->next_in_room )
    {
	if ( rch->deleted )
	    continue;

	count++;
    }

    if ( IS_SET( pRoomIndex->room_flags, ROOM_PRIVATE  ) && count >= 2 )
	return TRUE;

    if ( IS_SET( pRoomIndex->room_flags, ROOM_SOLITARY ) && count >= 1 )
	return TRUE;

    return FALSE;
}

/* 
 *  I added this function for secret doors and anything else that you might
 *  need that isn't an object or a person. DO NOT call this out for mobs,
 *  EVER! --Flux.
 */
bool can_see_thing( CHAR_DATA *ch, int type )
{
    if ( ch->deleted )
        return FALSE;
    
    if ( IS_SET( ch->act, PLR_HOLYLIGHT ) )
	return TRUE;

    if ( IS_AFFECTED( ch, AFF_BLIND ) )
	return FALSE;

    if ( IS_AFFECTED2( ch, AFF_TRUESIGHT )
	|| (get_race_data(ch->race))->truesight == 1 )
     return TRUE;

   if ( type == THING_HIDDEN )
   {
    if ( IS_AFFECTED( ch, AFF_INFRARED )
	|| (get_race_data(ch->race))->infrared == 1 )
     return TRUE;

    if ( IS_AFFECTED( ch, AFF_DETECT_HIDDEN ) )
	return TRUE;
   }
   else if ( type == THING_INVIS )
   {
    if ( IS_AFFECTED( ch, AFF_DETECT_INVIS ) )
	return TRUE;
   }

    return FALSE;
}

/* True if ch can not see his hand in front of his face */
bool vision_impared( CHAR_DATA *ch )
{
    if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_HOLYLIGHT ) )
	return FALSE;

    if ( IS_AFFECTED2( ch, AFF_TRUESIGHT ) || 
        ( !IS_NPC(ch) && (get_race_data(ch->race))->truesight == 1 ) )
     return FALSE;

    if ( IS_AFFECTED( ch, AFF_BLIND ) )
     return TRUE;

    if ( IS_AFFECTED( ch, AFF_INFRARED ) || 
     ( !IS_NPC(ch) && (get_race_data(ch->race))->infrared == 1 ) )
     return FALSE;

    if ( is_raffected( ch->in_room, gsn_globedark )
     && ch->race != RACE_DROW )
     return TRUE;

    if ( !IS_NPC(ch) && room_is_dark(ch->in_room) )
     return TRUE;

    return FALSE;
}

/*
 * True if char can see victim.
 */
bool can_see( CHAR_DATA *ch, CHAR_DATA *victim )
{
    if ( victim->deleted )
        return FALSE;

    if ( ch == victim )
	return TRUE;
    
    if ( !IS_NPC( victim ) && !IS_NPC(ch) )
    {
     if ( IS_SET( victim->act, PLR_WIZINVIS )
	&& get_trust( ch ) < victim->wizinvis )
	return FALSE;
   
     if ( IS_SET( victim->act, PLR_CLOAKED )
      && get_trust( ch ) < victim->cloaked 
      && ( ch->in_room->vnum != victim->in_room->vnum ) )
         return FALSE;
    }

    if ( IS_NPC( ch )
     && ( IS_SET( victim->act, PLR_CLOAKED ) 
     || IS_SET( victim->act, PLR_WIZINVIS ) ) )
     return FALSE;

    if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_HOLYLIGHT ) )
	return TRUE;

    if ( IS_AFFECTED2( ch, AFF_TRUESIGHT ) || 
        ( !IS_NPC(ch) && (get_race_data(ch->race))->truesight == 1 ) )
     return TRUE;

    if ( IS_AFFECTED( ch, AFF_BLIND ) )
     return FALSE;

    if ( IS_AFFECTED2( victim, AFF_PHASED ) )
     return FALSE; 

    if ( IS_AFFECTED( ch, AFF_INFRARED ) || 
     ( !IS_NPC(ch) && (get_race_data(ch->race))->infrared == 1 ) )
     return TRUE;

    if ( is_raffected( ch->in_room, gsn_globedark )
     && ch->race != RACE_DROW )
     return FALSE;

    if ( !IS_NPC(ch) && ( room_is_dark(ch->in_room) || 
                          room_is_dark( victim->in_room ) ) )
     return FALSE;

    if ( victim->position == POS_DEAD )
     return TRUE;

    if ( IS_AFFECTED( victim, AFF_INVISIBLE )
	&& !IS_AFFECTED( ch, AFF_DETECT_INVIS ) )
	return FALSE;

    if ( IS_AFFECTED( victim, AFF_HIDE )
	&& !IS_AFFECTED( ch, AFF_DETECT_HIDDEN ) )
	return FALSE;

    if ( !IS_NPC( victim ) )
     if ( victim->pcdata->morph[4] == 1 )
      return FALSE;

    return TRUE;
}

/*
 * True if char can see obj.
 */
bool can_see_obj( CHAR_DATA *ch, OBJ_DATA *obj )
{
      if ( !obj || obj->deleted )
        return FALSE;

    if ( !IS_NPC( ch ) && IS_SET( ch->act, PLR_HOLYLIGHT ) )
	return TRUE;

    if ( IS_AFFECTED( ch, AFF_BLIND ) )
	return FALSE;

    if ( !IS_NPC( ch )
       && ( IS_AFFECTED2( ch, AFF_TRUESIGHT )
          || (get_race_data(ch->race))->truesight == 1 ) )
        return TRUE;

    if ( !IS_NPC( ch )
       && ( IS_AFFECTED( ch, AFF_INFRARED )
          || (get_race_data(ch->race))->infrared == 1 ) )
        return TRUE;

    if ( IS_SET( obj->extra_flags, ITEM_INVIS )
	&& !IS_AFFECTED( ch, AFF_DETECT_INVIS ) )
	return FALSE;

    if ( IS_SET( obj->extra_flags, ITEM_GLOW ) )
	return TRUE;

    if ( is_raffected( ch->in_room, gsn_globedark ) 
    && ch->race != RACE_DROW )
	return FALSE;

    if ( room_is_dark( ch->in_room ) )
	return FALSE;

    return TRUE;
}



/*
 * True if char can drop obj.
 */
bool can_drop_obj( CHAR_DATA *ch, OBJ_DATA *obj )
{
    if ( !IS_SET( obj->extra_flags, ITEM_NODROP ) )
	return TRUE;

    if ( !IS_NPC( ch ) && ch->level >= LEVEL_IMMORTAL )
	return TRUE;

    return FALSE;
}



/*
 * Return ascii name of an item type.
 */
char *item_type_name( OBJ_DATA *obj )
{
    OBJ_DATA *in_obj;
    char      buf [ MAX_STRING_LENGTH ];

    switch ( obj->item_type )
    {
    case ITEM_SCROLL:			return "scroll";
    case ITEM_WAND:			return "wand";
    case ITEM_STAFF:			return "staff";
    case ITEM_WEAPON:			return "weapon";
    case ITEM_TREASURE:			return "treasure";
    case ITEM_ARMOR:			return "armor";
    case ITEM_POTION:			return "potion";
    case ITEM_FURNITURE:		return "furniture";
    case ITEM_TRASH:			return "trash";
    case ITEM_CONTAINER:		return "container";
    case ITEM_DRINK_CON:		return "drink container";
    case ITEM_MISC:			return "misc";
    case ITEM_FOOD:			return "food";
    case ITEM_MONEY:			return "money";
    case ITEM_BOAT:			return "boat";
    case ITEM_CORPSE_NPC:		return "npc corpse";
    case ITEM_RIGOR:    		return "stiff corpse";
    case ITEM_SKELETON: 		return "skeleton";
    case ITEM_CORPSE_PC:		return "pc corpse";
    case ITEM_FOUNTAIN:			return "fountain";
    case ITEM_PILL:			return "pill";
    case ITEM_LENSE:          		return "contacts";
    case ITEM_PORTAL:         		return "portal";
    case ITEM_VODOO:          		return "voodoo doll";
    case ITEM_BERRY:          		return "berry";
    case ITEM_BOMB:           		return "explosive device";
    case ITEM_BLOOD:          		return "blood";
    case ITEM_SWITCH:			return "switch";
    case ITEM_BODY_PART:      		return "body part";
    case ITEM_ORE:			return "ore";
    case ITEM_SCROLLPAPER:		return "parchment";
    case ITEM_PEN:			return "ballpoint pen";
    case ITEM_BEAKER:			return "beaker";
    case ITEM_CHEMSET:			return "chemistry set";
    case ITEM_BUNSEN:			return "bunsen burner";
    case ITEM_INKCART:			return "ink catridge";
    case ITEM_POSTALPAPER:		return "postal paper";
    case ITEM_LETTER:			return "letter";
    case ITEM_SMOKEBOMB:		return "smokebomb";
    case ITEM_SMITHYHAMMER:		return "smithy hammer";
    case ITEM_POISONCHEM:		return "poison";
    case ITEM_MOLOTOVCHEM:		return "napalm";
    case ITEM_WICK:			return "wick";
    case ITEM_TEARCHEM:			return "teargas chemical";
    case ITEM_PIPEBOMBCHEM:		return "plastique";
    case ITEM_CHEMBOMBCHEM:		return "chemical gas";
    case ITEM_TIMER:			return "clock";
    case ITEM_PILEWIRE:			return "pile of wires";
    case ITEM_TRIPWIRE:			return "tripwire";
    case ITEM_TOOLPACK:			return "tool pack";
    case ITEM_CHERRYCONTAINER:		return "cherrybomb container";
    case ITEM_GUNPOWDER:		return "gun powder";
    case ITEM_SMITHYPACK:		return "smithy toolpack";
    case ITEM_SMITHYANVIL:		return "anvil";
    case ITEM_TECHNOSOTTER:		return "sottering iron";
    case ITEM_TECHNOWORKSTATION:	return "workstation";
    case ITEM_RANGED_WEAPON:		return "ranged weapon";
    case ITEM_CLIP:			return "ammunition clip";
    case ITEM_BULLET:			return "bullet";
    case ITEM_GAS_CLOUD:		return "cloud of gas";
    case ITEM_EMBALMING_FLUID:		return "embalming fluid";
    case ITEM_MUMMY:			return "mummified corpse";
    case ITEM_ARROW:			return "arrow";
    }

    for ( in_obj = obj; in_obj->in_obj; in_obj = in_obj->in_obj )
      ;

    if ( in_obj->carried_by )
      sprintf( buf, "Item_type_name: unknown type %d from %s owned by %s.",
	      obj->item_type, obj->name, obj->carried_by->name );
    else
      sprintf( buf,
	      "Item_type_name: unknown type %d from %s owned by (unknown).",
	      obj->item_type, obj->name );

    bug( buf, 0 );
    return "(unknown)";
}



/*
 * Return ascii name of an affect location.
 */
char *affect_loc_name( int location )
{
    switch ( location )
    {
    case APPLY_NONE:		return "none";
    case APPLY_STR:		return "strength";
    case APPLY_DEX:		return "dexterity";
    case APPLY_INT:		return "intelligence";
    case APPLY_WIS:		return "wisdom";
    case APPLY_CON:		return "constitution";
    case APPLY_AGI:		return "agility";
    case APPLY_CHA:		return "charisma";
    case APPLY_MDAMP:		return "magical dampener";
    case APPLY_PDAMP:		return "physical dampener";
    case APPLY_SEX:		return "sex";
    case APPLY_RACE:		return "race";
    case APPLY_SIZE:		return "size";
    case APPLY_AGE:		return "age";
    case APPLY_MANA:		return "mana";
    case APPLY_HIT:		return "hp";
    case APPLY_MOVE:		return "moves";
    case APPLY_HITROLL:		return "hit roll";
    case APPLY_DAMROLL:		return "damage roll";

    case APPLY_THROWPLUS:	return "throwing plus";

    case APPLY_IMM_HEAT        :    return "heat immunity";
    case APPLY_IMM_POSITIVE    :    return "positive energy immunity";
    case APPLY_IMM_COLD        :    return "cold immunity";
    case APPLY_IMM_NEGATIVE    :    return "negative energy immunity";
    case APPLY_IMM_HOLY        :    return "holy immunity";
    case APPLY_IMM_UNHOLY      :    return "unholy immunity";
    case APPLY_IMM_REGEN       :    return "regen immunity";
    case APPLY_IMM_DEGEN       :    return "degen immunity";
    case APPLY_IMM_DYNAMIC     :    return "dynamic immunity";
    case APPLY_IMM_VOID        :    return "void immunity";
    case APPLY_IMM_PIERCE      :    return "pierce immunity";
    case APPLY_IMM_SLASH       :    return "slash immunity";
    case APPLY_IMM_SCRATCH     :    return "scratch immunity";
    case APPLY_IMM_BASH        :    return "bash immunity";
    case APPLY_IMM_INTERNAL    :    return "internal damage immunity";

/* X */
      case APPLY_INVISIBLE:		return "'invisible'";
      case APPLY_DETECT_INVIS:	return "'detect invis'";
      case APPLY_DETECT_HIDDEN:	return "'detect hidden'";
      case APPLY_INFRARED:		return "'infrared'";
      case APPLY_SNEAK:			return "'sneak'";
      case APPLY_HIDE:			return "'hide'";
      case APPLY_FLYING:		return "'fly'";
      case APPLY_BREATHE_WATER:		return "'breathe water'";
      case APPLY_PASS_DOOR:		return "'pass door'";
      case APPLY_TEMPORAL_FIELD:	return "'temporal field'";
      case APPLY_FIRESHIELD:    	return "'fireshield'";
      case APPLY_SHOCKSHIELD:   	return "'shockshield'";
      case APPLY_ICESHIELD:     	return "'iceshield'";  
      case APPLY_CHAOS:         	return "'chaos field'";
      case APPLY_SCRY:          	return "'scry'";
      case APPLY_HEIGHTEN_SENSES:	return "'heighten-senses'";
      case APPLY_POISON:		return "'poison'";
      case AFF_SIAMESE:                 return "'siamese-soul'";
/* END */
    }

    bug( "Affect_location_name: unknown location %d.", location );
    return "(unknown)";
}



/*
 * Return ascii name of an affect bit vector.
 */
char *affect_bit_name( int vector )
{
    static char buf [ 512 ];

    buf[0] = '\0';
    if ( vector & AFF_BLIND         ) strcat( buf, " blind"         );
    if ( vector & AFF_INVISIBLE     ) strcat( buf, " invisible"     );
    if ( vector & AFF_DETECT_INVIS  ) strcat( buf, " detect_invis"  );
    if ( vector & AFF_DETECT_HIDDEN ) strcat( buf, " detect_hidden" );
    if ( vector & AFF_HASTE         ) strcat( buf, " haste"         );
    if ( vector & AFF_CUREAURA      ) strcat( buf, " curative aura" );
    if ( vector & AFF_FIRESHIELD    ) strcat( buf, " fireshield"    );
    if ( vector & AFF_SHOCKSHIELD   ) strcat( buf, " shockshield"   );
    if ( vector & AFF_ICESHIELD     ) strcat( buf, " iceshield"     );
    if ( vector & AFF_CHAOS         ) strcat( buf, " chaos_field"   );
    if ( vector & AFF_FAERIE_FIRE   ) strcat( buf, " faerie_fire"   );
    if ( vector & AFF_INFRARED      ) strcat( buf, " infrared"      );
    if ( vector & AFF_CURSE         ) strcat( buf, " curse"         );
    if ( vector & AFF_FLAMING       ) strcat( buf, " flaming"       );
    if ( vector & AFF_POISON        ) strcat( buf, " poison"        );
    if ( vector & AFF_SLEEP         ) strcat( buf, " sleep"         );
    if ( vector & AFF_SNEAK         ) strcat( buf, " sneak"         );
    if ( vector & AFF_HIDE          ) strcat( buf, " hide"          );
    if ( vector & AFF_CHARM         ) strcat( buf, " charm"         );
    if ( vector & AFF_FLYING        ) strcat( buf, " flying"        );
    if ( vector & AFF_BREATHE_WATER ) strcat( buf, " breathe water" );
    if ( vector & AFF_PASS_DOOR     ) strcat( buf, " pass_door"     );
    if ( vector & AFF_TEMPORAL      ) strcat( buf, " temporal"      );
    if ( vector & AFF_PEACE         ) strcat( buf, " peace"         );
    if ( vector & AFF_VIBRATING     ) strcat( buf, " vibrating"     );
    if ( vector & AFF_SIAMESE       ) strcat( buf, " siamese_soul"  );
    return ( buf[0] != '\0' ) ? buf+1 : "none";
}

char *affect_bit_name2( int vector )
{
    static char buf [ 512 ];

    buf[0] = '\0';
    if ( vector & AFF_INERTIAL      ) strcat( buf, " inertial"      );
    if ( vector & AFF_POLYMORPH     ) strcat( buf, " polymorph"     );
    if ( vector & AFF_NOASTRAL      ) strcat( buf, " noastral"      );
    if ( vector & AFF_TRUESIGHT     ) strcat( buf, " truesight"     );
    if ( vector & AFF_BLADE         ) strcat( buf, " bladebarrier"  );
    if ( vector & AFF_BERSERK       ) strcat( buf, " berserk"       );
    if ( vector & AFF_RAGE	    ) strcat( buf, " rage"	    );
    if ( vector & AFF_RUSH	    ) strcat(buf, " adrenaline_rush");
    if ( vector & AFF_PHASED        ) strcat( buf, " phase_shift"   );
    if ( vector & AFF_VAMPIRIC	    ) strcat(buf, " vampiric_aspect");
    if ( vector & AFF_MANASHIELD    ) strcat( buf, " mana_shield"   );
    if ( vector & AFF_MANANET	    ) strcat( buf, " mana_net"      );
    if ( vector & AFF_ACIDBLOOD	    ) strcat( buf, " acidic_blood"  );
    if ( vector & AFF_GRASPING	    ) strcat( buf, " grasping"      );
    if ( vector & AFF_GRASPED	    ) strcat( buf, " grasped"       );
    if ( vector & AFF_CLAP	    ) strcat( buf, " clap"	    );
    return ( buf[0] != '\0' ) ? buf+1 : "none";
}



/*
 * Return ascii name of extra flags vector.
 */
char *extra_bit_name( int extra_flags )
{
    static char buf [ 512 ];

    buf[0] = '\0';
    if ( extra_flags & ITEM_GLOW         ) strcat( buf, " glow"         );
    if ( extra_flags & ITEM_SHARP        ) strcat( buf, " sharp"	);
    if ( extra_flags & ITEM_BALANCED     ) strcat( buf, " balanced"	);
    if ( extra_flags & ITEM_HUM          ) strcat( buf, " hum"          );
    if ( extra_flags & ITEM_DARK         ) strcat( buf, " dark"         );
    if ( extra_flags & ITEM_LOCK         ) strcat( buf, " lock"         );
    if ( extra_flags & ITEM_INVIS        ) strcat( buf, " invis"        );
    if ( extra_flags & ITEM_MAGIC        ) strcat( buf, " magic"        );
    if ( extra_flags & ITEM_NODROP       ) strcat( buf, " nodrop"       );
    if ( extra_flags & ITEM_BLESS        ) strcat( buf, " bless"        );
    if ( extra_flags & ITEM_NOREMOVE     ) strcat( buf, " noremove"     );
    if ( extra_flags & ITEM_INVENTORY    ) strcat( buf, " inventory"    );
    if ( extra_flags & ITEM_POISONED     ) strcat( buf, " poisoned"     );
    if ( extra_flags & ITEM_DWARVEN	 ) strcat( buf, " dwarven"	);
    if ( extra_flags & ITEM_FLAME        ) strcat( buf, " burning"      );
    if ( extra_flags & ITEM_CHAOS        ) strcat( buf, " chaotic"      );
    if ( extra_flags & ITEM_NO_DAMAGE    ) strcat( buf, " indestructable");
    if ( extra_flags & ITEM_ICY          ) strcat( buf, " frosty"       );
    if ( extra_flags & ITEM_RAINBOW      ) strcat( buf, " colorful"     );
    if ( extra_flags & ITEM_SHOCK        ) strcat( buf, " electric"     );
    if ( extra_flags & ITEM_WIRED        ) strcat( buf, " wired"        );
    if ( extra_flags & ITEM_SOUL_BOUND   ) strcat( buf, " bound"	);
    if ( extra_flags & ITEM_TECHNOLOGY   ) strcat( buf, " technological");
    return ( buf[0] != '\0' ) ? buf+1 : "none";
}


char *act_bit_name( int act )
{
    static char buf [ 512 ];
    
    buf[0] = '\0';
    if ( act & 1 ) 
    {
      strcat( buf, " npc" );
      if ( act & ACT_PROTOTYPE )      strcat( buf, " prototype" );
      if ( act & ACT_SENTINEL )       strcat( buf, " sentinel" );
      if ( act & ACT_SCAVENGER )      strcat( buf, " scavenger" );
      if ( act & ACT_AGGRESSIVE )     strcat( buf, " aggressive" );
      if ( act & ACT_STAY_AREA )      strcat( buf, " stayarea" );
      if ( act & ACT_PET )            strcat( buf, " pet" );
      if ( act & ACT_TRAIN )          strcat( buf, " trainer" );
      if ( act & ACT_PRACTICE )       strcat( buf, " practicer" );
      if ( act & ACT_GAMBLE )         strcat( buf, " gambler" );
      if ( act & ACT_TRACK )          strcat( buf, " track" );
      if ( act & ACT_UNDEAD )	      strcat( buf, " undead" );
      if ( act & ACT_POSTMAN )        strcat( buf, " postman" );
      if ( act & ACT_NOPUSH )	      strcat( buf, " nopush" );
      if ( act & ACT_NODRAG )	      strcat( buf, " nodrag" );
      if ( act & ACT_NOSHADOW )	      strcat( buf, " noshadow" );
      if ( act & ACT_NOASTRAL )	      strcat( buf, " noastral" );
      if ( act & ACT_ILLUSION )	      strcat( buf, " illusion" );
    }
    else
    {
      strcat( buf, " pc" );
      if ( act & PLR_AFK ) strcat( buf, " AFK" );
      if ( act & PLR_BOUGHT_PET ) strcat( buf, " boughtpet" );
      if ( act & PLR_AUTOEXIT ) strcat( buf, " autoexit" );
      if ( act & PLR_AUTOLOOT ) strcat( buf, " autoloot" );
      if ( act & PLR_AUTOSAC ) strcat( buf, " autosac" );
      if ( act & PLR_BRIEF ) strcat( buf, " brief" );
      if ( act & PLR_COMBINE ) strcat( buf, " combine" );
      if ( act & PLR_PROMPT ) strcat( buf, " prompt" );
      if ( act & PLR_TELNET_GA ) strcat( buf, " telnetga" );
      if ( act & PLR_HOLYLIGHT ) strcat( buf, " holylight" );
      if ( act & PLR_WIZINVIS ) strcat( buf, " wizinvis" );
      if ( act & PLR_CLOAKED ) strcat( buf, " cloaked" );
      if ( act & PLR_SILENCE ) strcat( buf, " silence" );
      if ( act & PLR_NO_EMOTE ) strcat( buf, " noemote" );
      if ( act & PLR_NO_TELL ) strcat( buf, " notell" );
      if ( act & PLR_LOG ) strcat( buf, " log" );
      if ( act & PLR_FREEZE ) strcat( buf, " freeze" );
      if ( act & PLR_AUTOASSIST ) strcat( buf, " autoassist" );
      if ( act & PLR_KILLER ) strcat( buf, " killer" );
      if ( act & PLR_OUTCAST ) strcat( buf, " outcast" );
      if ( act & PLR_ANSI ) strcat( buf, " ansi" );
      if ( act & PLR_AUTOCOINS ) strcat( buf, " autocoins" );
      if ( act & PLR_AUTOSPLIT ) strcat( buf, " autosplit" );
      if ( act & PLR_UNDEAD ) strcat( buf, " undead" );
      if ( act & PLR_ILLUSION ) strcat( buf, " illusion" );
    }
    return ( buf[0] != '\0' ) ? buf+1 : "none";
}
        
CHAR_DATA *get_char( CHAR_DATA *ch )
{
    if ( !ch->pcdata )
        return ch->desc->original;
    else
        return ch;
}

bool longstring( CHAR_DATA *ch, char *argument )
{
    if ( strlen( argument ) > 75 )
    {
	send_to_char(C_DEFAULT, "No more than 75 characters in this field.\n\r", ch );
	return TRUE;
    }
    else
        return FALSE;
}

void end_of_game( void )
{
    DESCRIPTOR_DATA *d;
    DESCRIPTOR_DATA *d_next;

    save_player_list();

    for ( d = descriptor_list; d; d = d_next )
    {
	d_next = d->next;
	if ( d->connected == CON_PLAYING )
	{
	    if ( d->character->position == POS_FIGHTING )
	      interpret( d->character, "save" );
	    else
	      interpret( d->character, "quit" );
	}
    }

    return;

}

/* XOR */
char *imm_bit_name(int imm_flags)
{
    static char buf[512];

    buf[0] = '\0';

    if (imm_flags & IMM_HEAT			) strcat(buf, " heat");
    if (imm_flags & IMM_POSITIVE		) strcat(buf, " positive");
    if (imm_flags & IMM_COLD			) strcat(buf, " cold");
    if (imm_flags & IMM_NEGATIVE		) strcat(buf, " negative");
    if (imm_flags & IMM_HOLY			) strcat(buf, " holy");
    if (imm_flags & IMM_UNHOLY		) strcat(buf, " unholy");
    if (imm_flags & IMM_REGEN			) strcat(buf, " regen");
    if (imm_flags & IMM_DEGEN			) strcat(buf, " degen");
    if (imm_flags & IMM_DYNAMIC		) strcat(buf, " dynamic");
    if (imm_flags & IMM_VOID			) strcat(buf, " void");
    if (imm_flags & IMM_PIERCE		) strcat(buf, " pierce");
    if (imm_flags & IMM_SLASH			) strcat(buf, " slash");
    if (imm_flags & IMM_SCRATCH		) strcat(buf, " scratch");
    if (imm_flags & IMM_BASH			) strcat(buf, " bash");
    if (imm_flags & IMM_INTERNAL		) strcat(buf, " internal");

    return ( buf[0] != '\0' ) ? buf+1 : "none" ; 
}

/* 
 * Returns a flag for wiznet 
 */
long wiznet_lookup (const char *name)
{
    int flag;

    for (flag = 0; wiznet_table[flag].name != NULL; flag++)
    {
	if (LOWER(name[0]) == LOWER(wiznet_table[flag].name[0])
	&& !str_prefix(name,wiznet_table[flag].name))
	    return flag;
    }

    return -1;
}

/* Copies a string, removing color codes */
char *strcpy_wo_col( char *dest, char *src )
{
    char *p = src;
    char *q = dest;
    char c;

    while ( ( c = *p++ ) != '\0' )
    {
        if ( c != '&' )
	{
	    *q++ = c;
	    continue;
	}
	if ( *p != '&' && *p != '\0' )
	    p++;
    }
    *q = '\0';
    return dest;
}

int strlen_wo_col( char *argument )
{
  char *str;
  bool found = FALSE;
  int colfound = 0;
  int ampfound = 0;
  int len;
  for ( str = argument; *str != '\0'; str++ )
    {
    if ( found && is_colcode( *str ) )
	{
	colfound++;
	found = FALSE;
	}
    if ( found && *str == '&' )
	ampfound++;
    if ( *str == '&' )
	found = found ? FALSE : TRUE;
    else
	found = FALSE;
    }
  len = strlen( argument );
  len = len - ampfound - ( colfound * 2 );
  return len;
}
char *strip_color( char *argument )
{
  char *str;
  char new_str [ MAX_INPUT_LENGTH ];
  int i = 0;
  for ( str = argument; *str != '\0'; str++ )
    {
    if ( new_str[ i-1 ] == '&' && is_colcode( *str ) )
	{
	i--;
	continue;
	}
    if ( new_str[ i-1 ] == '&' && *str == '&' )
	continue;
    new_str[i] = *str;
    i++;
    }
  if ( new_str[i] != '\0' )
	new_str[i] = '\0';
  str = str_dup( (char *)new_str );
  return str;
}

bool is_colcode( char code )
{ /* It ain't pretty, but it works :) -- Hannibal */
  if ( code == 'r'
    || code == 'R'
    || code == 'b'
    || code == 'B'
    || code == 'g'
    || code == 'G'
    || code == 'w'
    || code == 'W'
    || code == 'p'
    || code == 'P'
    || code == 'Y'
    || code == 'O'
    || code == 'c'
    || code == 'C'
    || code == 'z' 
    || code == '.' )
	return TRUE;
  return FALSE;
}

/* Ugh, graduated xp! Woo hoo! Flux */
int xp_tolvl( CHAR_DATA *ch ) 
{
 int tolvl = 0;
 int counter;

 if ( IS_NPC( ch ) )
  return ch->exp;

 for ( counter = 1; counter < ch->level + 2; counter++ )
  tolvl += counter * 200;

 return tolvl;
}

/*
 * Equip a char with an obj.
 */
void equip_bionic( CHAR_DATA *ch, OBJ_DATA *obj, int iWear )
{
    AFFECT_DATA *paf;
    char         buf [ MAX_STRING_LENGTH ];

    if ( get_bionic_char( ch, iWear ) )
    {
        sprintf( buf, "Equip_bionic: %s already equipped at %d.",
		ch->name, iWear );
	bug( buf, 0 );
	return;
    }
    obj->bionic_loc	 = iWear;

    for ( paf = obj->pIndexData->affected; paf; paf = paf->next )
	affect_modify( ch, paf, TRUE );
    for ( paf = obj->affected; paf; paf = paf->next )
	affect_modify( ch, paf, TRUE );

    ch->carry_number -= get_obj_number( obj );
    oprog_wear_trigger( obj, ch );
    return;
}

/*
 * Unequip a char with an obj.
 */
void unequip_bionic( CHAR_DATA *ch, OBJ_DATA *obj )
{
    AFFECT_DATA *paf;
    char         buf [ MAX_STRING_LENGTH ];

    if ( obj->bionic_loc == BIONIC_NONE )
    {
        sprintf( buf, "Unequip_bioinic: %s already unequipped with %d.",
		ch->name, obj->pIndexData->vnum );
	bug( buf, 0 );
	return;
    }
    obj->bionic_loc	 = -1;

    for ( paf = obj->pIndexData->affected; paf; paf = paf->next )
	affect_modify( ch, paf, FALSE );
    for ( paf = obj->affected; paf; paf = paf->next )
	affect_modify( ch, paf, FALSE );

    ch->carry_number += get_obj_number( obj );
    return;
}

OBJ_DATA *get_bionic_char( CHAR_DATA *ch, int iWear )
{
    OBJ_DATA *obj;

    for ( obj = ch->carrying; obj; obj = obj->next_content )
    {
        if ( obj->deleted )
	    continue;
	if ( obj->bionic_loc == iWear )
	    return obj;
    }

    return NULL;
}

OBJ_DATA *get_bionic_wear( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    int       number;
    int       count;

    number = number_argument( argument, arg );
    count  = 0;
    for ( obj = ch->carrying; obj; obj = obj->next_content )
    {
	if ( obj->bionic_loc != BIONIC_NONE
	    && can_see_obj( ch, obj )
	    && is_name( ch, arg, obj->name ) )
	{
	    if ( ++count == number )
		return obj;
	}
    }

    return NULL;
}
