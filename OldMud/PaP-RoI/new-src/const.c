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
#include <stdio.h>
#include <time.h>
#include "merc.h"

const	struct	material_type	material_table	[MAX_MATERIAL]	=
{
/*  {	"name", type,
	imm_heat,	imm_positive,	imm_cold,	imm_negative,
	imm_holy,	imm_unholy,	imm_regen	imm_degen,
	imm_dynamic,	imm_void,	imm_pierce,	imm_slash,
	imm_scratch,	imm_bash,	imm_internal }, */

  {	"leather", MATERIAL_TYPE_CLOTH,
	{ 10,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	125,	125,	125,	15,	0 } }, 

  {	"stone", MATERIAL_TYPE_NATURAL,
	{ 0,	10,	0,	0,	0,	0,	0,	10,
	0,	100,	85,	85,	85,	100,	0 } }, 

  {	"wood", MATERIAL_TYPE_NATURAL,
	{ 50,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	100,	100,	100,	100,	0 } }, 

  {	"gold", MATERIAL_TYPE_METAL,
	{ 10,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	75,	75,	75,	120,	0 } }, 

  {	"silver", MATERIAL_TYPE_METAL,
	{ 10,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	75,	75,	75,	120,	0 } }, 

  {	"copper", MATERIAL_TYPE_METAL,
	{ 10,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	125,	125,	125,	15,	0 } }, 

  {	"plastic", MATERIAL_TYPE_UNNATURAL,
	{ 10,	10,	0,	0,	0,	0,	0,	10,
	0,	100,	110,	110,	110,	110,	0 } }, 

  {	"glass", MATERIAL_TYPE_UNNATURAL,
	{ 10,	10,	0,	0,	0,	0,	0,	0,
	0,	100,	35,	35,	35,	150,	0 } }, 

  {	"iron", MATERIAL_TYPE_METAL,
	{ 5,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	70,	70,	70,	85,	0 } }, 

  {	"lead", MATERIAL_TYPE_METAL,
	{ 0,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	65,	65,	65,	80,	0 } }, 

  {	"fire", MATERIAL_TYPE_MYSTIC,
	{ 0,	100,	0,	0,	0,	0,	0,	0,
	100,	100,	0,	0,	0,	0,	0 } }, 

  {	"ice", MATERIAL_TYPE_MYSTIC,
	{ 100,	0,	0,	0,	0,	0,	0,	0,
	100,	100,	0,	0,	0,	0,	0 } }, 

  {	"energy", MATERIAL_TYPE_MYSTIC,
	{ 0,	0,	0,	0,	0,	0,	0,	0,
	100,	100,	0,	0,	0,	0,	0 } }, 

  {	"crystal", MATERIAL_TYPE_NATURAL,
	{ 0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	25,	25,	25,	150,	0 } }, 

  {	"bone", MATERIAL_TYPE_NATURAL,
	{ 10,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	100,	100,	100,	100,	0 } }, 

  {	"steel", MATERIAL_TYPE_METAL,
	{ 2,	2,	0,	0,	0,	0,	0,	5,
	0,	100,	65,	65,	65,	70,	0 } }, 

  {	"bronze", MATERIAL_TYPE_METAL,
	{ 10,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	70,	70,	70,	85,	0 } }, 

  {	"brass", MATERIAL_TYPE_METAL,
	{ 10,	0,	0,	0,	0,	0,	0,	10,
	0,	100,	65,	65,	65,	75,	0 } }, 

  {	"omni crystal", MATERIAL_TYPE_NATURAL,
	{ 0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	10,	10,	10,	10,	0 } }, 

  {	"plasma", MATERIAL_TYPE_MYSTIC,
	{ 0,	0,	0,	100,	0,	0,	0,	0,
	100,	100,	0,	0,	0,	0,	0 } }, 

  {	"omni steel", MATERIAL_TYPE_METAL,
	{ 0,	0,	0,	0,	0,	0,	0,	0,
	0,	100,	1,	1,	1,	1,	0 } }, 

  {	"living metal", MATERIAL_TYPE_METAL,
	{ 0,	0,	0,	0,	0,	0,	0,	0,
	100,	100,	0,	0,	0,	0,	0 } }, 

  {	"cloth", MATERIAL_TYPE_CLOTH,
	{ 100,	0,	0,	0,	0,	0,	0,	100,
	0,	100,	200,	200,	200,	0,	0 } }, 

  {	"kevlar", MATERIAL_TYPE_CLOTH,
	{ 1,	0,	0,	0,	0,	0,	0,	5,
	0,	100,	75,	75,	75,	5,	0 } }

};

const	struct	skill_tree_data	skill_tree_table [ MAX_SKILL_TREE ]	=
{
/*
 * Skill tree data:
 *  Name is the alpha identifier, used within the MUD by players mostly.
 *  UID - Unique Identification, since many have the same name.
 *  FID - Family Identification, to show what it belongs to overall.
 *  PID - Parental ID, what skill is directly above it in level.
 *  Cost - Training cost modifier.
 *  P-req - A workaround, how many levels of the PID one needs to train 
 *          this particular skill.
 *  Stats - What stats affect what skills.
 * -Flux
 */

 /* { "Name",		UID, 	FID, 	PID,	cost,	parent req,  
      Str,	Dex,	Agi,	Int,	Wis,	Con,	Charm }, */
 { "Craftsmanship",	0,	-1,	-1,	7,	0,
	2,	2,	0,	1,	1,	1,	0 },
 { "Tailoring",		1,	0,	0,	7,	5,
	0,	4,	1,	1,	1,	0,	0 },
 { "Jewelery",		2,	0,	0,	7,	5,
	0,	4,	1,	1,	1,	0,	0 },
 { "Mining",		3,	0,	0,	7,	5,
	3,	1,	0,	1,	1,	1,	0 },
 { "Carpentry",		4,	0,	0,	7,	5,
	3,	2,	0,	1,	1,	0,	0 },
 { "Smithing",		5,	0,	0,	7,	5,
	2,	2,	0,	1,	1,	1,	0 },
 { "Weapons",		6,	0,	5,	7,	10,
	2,	2,	0,	1,	1,	1,	0 },
 { "Armor",		7,	0,	5,	7,	10,
	2,	2,	0,	1,	1,	1,	0 },
 { "Locks",		8,	0,	5,	7,	10,
	0,	4,	0,	1,	2,	0,	0 },

 /* { "Name",		UID, 	FID, 	PID,	cost,	parent req,  
      Str,	Dex,	Agi,	Int,	Wis,	Con,	Charm }, */
 { "Evaluation",	9,	-1,	-1,	1,	0,
	0,	0,	0,	4,	3,	0,	0 },
 { "Medical",		10,	9,	9,	1,	5,
	0,	0,	0,	3,	4,	0,	0 },
 { "Weapons",		11,	9,	9,	1,	5,
	0,	0,	0,	3,	4,	0,	0 },
 { "Armor",		12,	9,	9,	1,	5,
	0,	0,	0,	3,	4,	0,	0 },
 { "Jewelery",		13,	9,	9,	1,	5,
	0,	0,	0,	3,	4,	0,	0 },
 { "Tactical",		14,	9,	9,	5,	5,
	0,	1,	1,	3,	2,	0,	0 },

 /* { "Name",		UID, 	FID, 	PID,	cost,	parent req,  
      Str,	Dex,	Agi,	Int,	Wis,	Con,	Charm }, */
 { "Technology",	15,	-1,	-1,	10,	0,
	0,	3,	0,	2,	2,	0,	0 },
 { "Weapons",		16,	15,	15,	10,	5,
	0,	3,	0,	3,	1,	0,	0 },
 { "Firearms",		17,	15,	16,	10,	10,
	0,	3,	0,	3,	1,	0,	0 },
 { "Explosives",	18,	15,	16,	10,	10,
	0,	3,	0,	3,	1,	0,	0 },
 { "Power",		19,	15,	15,	10,	5,
	0,	3,	0,	1,	3,	0,	0 },
 { "Circuitry",		20,	15,	15,	10,	5,
	0,	3,	0,	2,	2,	0,	0 },
 { "Automation",	21,	15,	15,	10,	5,
	0,	3,	0,	3,	1,	0,	0 },
 { "Energy",		22,	15,	15,	10,	5,
	0,	3,	0,	1,	3,	0,	0 },
 { "Biological",	23,	15,	15,	10,	5,
	0,	2,	0,	3,	2,	0,	0 },
 { "Chemistry",		24,	15,	15,	10,	5,
	0,	2,	0,	2,	3,	0,	0 },

 /* { "Name",		UID, 	FID, 	PID,	cost,	parent req,  
      Str,	Dex,	Agi,	Int,	Wis,	Con,	Charm }, */
 { "Covert",		25,	-1,	-1,	7,	0,
	0,	3,	2,	1,	1,	0,	0 },
 { "Stealth",		26,	25,	25,	7,	5,
	0,	3,	2,	1,	1,	0,	0 },
 { "Sneaking",		27,	25,	26,	7,	10,
	0,	2,	2,	2,	1,	0,	0 },
 { "Hiding",		28,	25,	26,	7,	10,
	0,	3,	0,	2,	2,	0,	0 },
 { "Theft",		29,	25,	25,	7,	5,
	0,	4,	1,	1,	1,	0,	0 },
 { "Deception",		30,	25,	25,	7,	5,
	0,	0,	0,	3,	1,	0,	3 },
 { "Perception",	31,	25,	25,	3,	5,
	0,	0,	0,	5,	2,	0,	0 },
 { "Trapping",		32,	25,	25,	7,	5,
	0,	3,	1,	1,	2,	0,	0 },

 /* { "Name",		UID, 	FID, 	PID,	cost,	parent req,  
      Str,	Dex,	Agi,	Int,	Wis,	Con,	Charm }, */
 { "Physical",		33,	-1,	-1,	5,	0,
	3,	1,	1,	0,	0,	2,	0 },
 { "Medicine",		34,	33,	33,	5,	5,
	0,	2,	0,	2,	3,	0,	0 },
 { "Athletics",		35,	33,	33,	2,	5,
	1,	2,	2,	0,	0,	2,	0 },
 { "Swimming",		36,	33,	35,	2,	10,
	0,	2,	2,	0,	0,	3,	0 },
 { "Climbing",		37,	33,	35,	2,	10,
	2,	2,	1,	1,	0,	1,	0 },
 { "Riding",		38,	33,	35,	2,	10,
	1,	2,	1,	0,	1,	0,	2 },
 { "Body Building",	39,	33,	35,	2,	10,
	1,	0,	0,	0,	0,	6,	0 },
 { "Combat",		40,	33,	33,	5,	5,
	2,	2,	2,	0,	0,	1,	0 },
 { "Ambidexterity",	41,	33,	40,	5,	10,
	1,	4,	1,	1,	0,	0,	0 },
 { "Weapons",		42,	33,	40,	5,	10,
	3,	2,	0,	1,	1,	0,	0 },
 { "Slash",		43,	33,	42,	5,	15,
	3,	2,	0,	1,	1,	0,	0 },
 { "Pierce",		44,	33,	42,	5,	15,
	2,	2,	1,	1,	1,	0,	0 },
 { "Bash",		45,	33,	42,	5,	15,
	4,	1,	0,	1,	1,	0,	0 },
 { "Tear",		46,	33,	42,	5,	15,
	4,	1,	0,	1,	1,	0,	0 },
 { "Chop",		47,	33,	42,	5,	15,
	4,	1,	0,	1,	1,	0,	0 },
 { "Long",		48,	33,	42,	5,	15,
	3,	2,	0,	1,	1,	0,	0 },
 { "Medium",		49,	33,	42,	5,	15,
	3,	2,	0,	1,	1,	0,	0 },
 { "Short",		50,	33,	42,	5,	15,
	1,	2,	2,	1,	1,	0,	0 },
 { "Offensive",		51,	33,	40,	5,	10,
	2,	2,	1,	1,	1,	0,	0 },
 { "Unarmed",		52,	33,	51,	5,	15,
	2,	2,	1,	0,	0,	2,	0 },
 { "Weapons",		53,	33,	51,	5,	15,
	2,	2,	1,	1,	1,	0,	0 },
 { "Defensive",		54,	33,	40,	5,	10,
	1,	3,	1,	1,	1,	0,	0 },
 { "Unarmed",		55,	33,	54,	5,	15,
	1,	3,	1,	0,	0,	2,	0 },
 { "Weapons",		56,	33,	55,	5,	15,
	1,	3,	1,	1,	1,	0,	0 },
 { "Dodge",		57,	33,	55,	5,	15,
	0,	2,	5,	0,	0,	0,	0 },
 { "Martial Arts",	58,	33,	40,	5,	10,
	1,	1,	2,	1,	1,	1,	0 },
 { "Offensive",		59,	33,	58,	5,	15,
	2,	1,	1,	1,	1,	1,	0 },
 { "Defensive",		60,	33,	58,	5,	15,
	1,	1,	2,	1,	1,	1,	0 },
 { "Disciplines",	61,	33,	58,	5,	15,
	0,	2,	1,	1,	2,	1,	0 },
 { "Viper",		62,	33,	61,	5,	20,
	0,	1,	2,	1,	2,	1,	0 },
 { "Crane",		63,	33,	61,	5,	20,
	0,	2,	1,	1,	2,	1,	0 },
 { "Monkey",		64,	33,	61,	5,	20,
	0,	3,	0,	1,	2,	1,	0 },
 { "Bull",		65,	33,	61,	5,	20,
	1,	1,	1,	1,	2,	1,	0 },
 { "Tiger",		66,	33,	61,	5,	20,
	1,	1,	1,	1,	1,	1,	1 },
 { "Dragon",		67,	33,	61,	5,	20,
	1,	1,	1,	1,	2,	1,	0 },
 { "Panther",		68,	33,	61,	5,	20,
	1,	1,	1,	1,	2,	1,	0 },
 { "Sparrow",		69,	33,	61,	5,	20,
	0,	2,	1,	1,	2,	1,	0 },
 { "Ranged",		70,	33,	40,	5,	10,
	1,	4,	1,	1,	0,	0,	0 },
 { "Archery",		71,	33,	70,	5,	15,
	1,	4,	1,	1,	0,	0,	0 },
 { "Thrown",		72,	33,	70,	5,	15,
	2,	3,	1,	1,	0,	0,	0 },
 { "Short",		73,	33,	72,	5,	20,
	1,	4,	1,	1,	0,	0,	0 },
 { "Medium",		74,	33,	72,	5,	20,
	2,	3,	1,	1,	0,	0,	0 },
 { "Long",		75,	33,	72,	5,	20,
	3,	2,	1,	1,	0,	0,	0 },

 /* { "Name",		UID, 	FID, 	PID,	cost,	parent req,  
      Str,	Dex,	Agi,	Int,	Wis,	Con,	Charm }, */
 { "Magic",		76,	-1,	-1,	10,	0,
	0,	1,	0,	3,	3,	0,	0 },
 { "Voodoo",		77,	76,	76,	10,	5,
	0,	0,	0,	2,	4,	0,	1 },
 { "Prestidigitation",	78,	76,	76,	10,	5,
	0,	2,	2,	3,	0,	0,	0 },
 { "Spellcraft",	79,	76,	76,	10,	5,
	0,	0,	0,	1,	5,	1,	0 },
 { "Temporal",		80,	76,	76,	10,	5,
	0,	0,	0,	6,	1,	0,	0 },
 { "Alchemy",		81,	76,	76,	10,	5,
	0,	2,	0,	2,	2,	1,	0 },
 { "Perception",	82,	76,	76,	10,	5,
	0,	0,	0,	5,	2,	0,	0 },
 { "Arcane",		83,	76,	76,	10,	5,
	0,	0,	0,	4,	3,	0,	0 },
 { "Recite",		84,	76,	83,	10,	10,
	0,	0,	0,	4,	3,	0,	0 },
 { "Invoke",		85,	76,	83,	10,	10,
	0,	0,	0,	5,	2,	0,	0 },
 { "Artification",	86,	76,	76,	10,	5,
	0,	1,	0,	2,	4,	0,	0 },
 { "Enchantment",	87,	76,	86,	10,	10,
	0,	0,	0,	4,	3,	0,	0 },
 { "Crafting",		88,	76,	86,	10,	10,
	0,	3,	0,	3,	1,	0,	0 },
 { "Imbuing",		89,	76,	86,	10,	10,
	0,	0,	0,	4,	3,	0,	0 },
 { "Scroll",		90,	76,	89,	10,	15,
	0,	0,	0,	2,	5,	0,	0 },
 { "Wand",		91,	76,	89,	10,	15,
	0,	0,	0,	3,	4,	0,	0 },
 { "Staff",		92,	76,	89,	10,	15,
	0,	0,	0,	3,	4,	0,	0 },
 { "Natural",		93,	76,	76,	10,	5,
	1,	0,	0,	1,	3,	1,	1 },
 { "Herbal",		94,	76,	93,	10,	10,
	0,	0,	0,	1,	6,	0,	0 },
 { "Ecomancy",		95,	76,	93,	10,	10,
	0,	0,	0,	1,	3,	1,	2 },
 { "Augmentation",	96,	76,	76,	10,	5,
	0,	0,	0,	4,	2,	1,	0 },
 { "Enhancement",	97,	76,	96,	10,	10,
	0,	0,	0,	3,	4,	0,	0 },
 { "Energy",		98,	76,	97,	10,	15,
	0,	0,	0,	5,	2,	0,	0 },
 { "Physical",		99,	76,	97,	10,	15,
	0,	0,	0,	4,	2,	1,	0 },
 { "Cursing",		100,	76,	96,	10,	10,
	0,	0,	0,	4,	2,	1,	0 },
 { "Energy",		101,	76,	100,	10,	15,
	0,	0,	0,	5,	2,	0,	0 },
 { "Physical",		102,	76,	100,	10,	15,
	0,	0,	0,	4,	2,	1,	0 },
 { "Elemental",		103,	76,	76,	10,	5,
	0,	0,	0,	5,	2,	0,	0 },
 { "Heat",		104,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Cold",		105,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Negative",		106,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Positive",		107,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Holy",		108,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Unholy",		109,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Regenerative",	110,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Degenerative",	111,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Dynamic",		112,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Void",		113,	76,	103,	10,	10,
	0,	0,	0,	6,	1,	0,	0 },
 { "Transformation",	114,	76,	76,	10,	5,
	0,	0,	0,	2,	2,	3,	0 },
 { "Mythical",		115,	76,	114,	10,	10,
	0,	0,	0,	1,	3,	3,	0 },
 { "Natural",		116,	76,	114,	10,	10,
	0,	0,	0,	2,	1,	3,	1 }

 /* { "Name",		UID, 	FID, 	PID,	cost,	parent req,  
      Str,	Dex,	Agi,	Int,	Wis,	Con,	Charm }, */
};

/*
 * Immort Levels
*/
#define L_HER            LEVEL_HERO

/* 
 * Wiznet table and prototype for future flag setting
 */
const   struct wiznet_type      wiznet_table    []              =
{
   {    "on",           WIZ_ON,      	LEVEL_HERO },
   {    "prefix",	WIZ_PREFIX,  	LEVEL_HERO },
   {    "ticks",        WIZ_TICKS,   	LEVEL_IMMORTAL },
   {    "general",      WIZ_GENERAL,   	LEVEL_HERO },
   {    "logins",       WIZ_LOGINS,  	LEVEL_HERO },
   {    "sites",        WIZ_SITES,   	LEVEL_IMMORTAL },
   {    "links",        WIZ_LINKS,   	LEVEL_IMMORTAL },
   {	"newbies",	WIZ_NEWBIE,  	LEVEL_HERO },
   {	"spam",		WIZ_SPAM,    	LEVEL_IMMORTAL },
   {    "deaths",       WIZ_DEATHS,  	LEVEL_HERO },
   {    "resets",       WIZ_RESETS,  	LEVEL_IMMORTAL },
   {    "mobdeaths",    WIZ_MOBDEATHS,  LEVEL_IMMORTAL },
   {    "flags",	WIZ_FLAGS,	LEVEL_IMMORTAL },
   {	"penalties",	WIZ_PENALTIES,	L_GOD },
   {	"saccing",	WIZ_SACCING,	L_CON },
   {	"levels",	WIZ_LEVELS,	LEVEL_HERO },
   {	"load",		WIZ_LOAD,	L_CON },
   {	"restore",	WIZ_RESTORE,	L_CON },
   {	"snoops",	WIZ_SNOOPS,	L_CON },
   {	"switches",	WIZ_SWITCHES,	L_SEN },
   {	"secure",	WIZ_SECURE,	L_CON },
   {	"oldlog",	WIZ_OLDLOG,	L_DIR },
   {	NULL,		0,		0  }
};

/*
 * Liquid properties.
 * Used in world.obj.
 */
const	struct	liq_type	liq_table	[LIQ_MAX]	=
{
    { "water",			"clear",		{  0, 0, 10 }	},  /*  0 */
    { "beer",			"amber",		{  3, 2,  5 }	},
    { "wine",			"rose",		{  4, 2,  5 }	},
    { "ale",			"brown",		{  2, 2,  5 }	},
    { "dark ale",			"dark",		{  1, 2,  5 }	},

    { "whisky",			"golden",		{  8, 1,  4 }	},  /*  5 */
    { "lemonade",			"pink",		{  0, 1,  8 }	},
    { "firebreather",		"boiling",		{ 10, 0,  0 }	},
    { "local specialty",	"everclear",	{  3, 3,  3 }	},
    { "slime mold juice",	"green",		{  0, 4, -8 }	},

    { "milk",			"white",		{  0, 3,  6 }	},  /* 10 */
    { "tea",			"tan",		{  0, 1,  6 }	},
    { "coffee",			"black",		{  0, 1,  6 }	},
    { "blood",			"red",		{  0, 2, -1 }	},
    { "salt water",		"clear",		{  0, 1, -2 }	},

    { "cola",			"cherry",		{  0, 1,  5 }	}   /* 15 */
};


#define SLOT(s) s
const	struct	skill_type	skill_table	[ MAX_SKILL ]	=
{

/*
    { "Name",
     { skill tree dependencies },
     { racial dependencies },
     SPELL_FUN, gsn, SLOT(slot),
     target, wait cost, stamina cost, mana cost,
     minimum range, maximum range, minimum position,
     arms dep, legs dep, vocal dep, movement dep,
     group size, dispel classification,
     "affect noun", "slist color code",
     "occupied message",
     "wear-off message",
     "to_room wear off message"
    },
*/

  { "reserved",
   { -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1,

     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,

     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },

   { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1 },

    0,	NULL,	SLOT(0),
    TAR_IGNORE,	0,	0,	0,
    0,	0,	POS_STANDING,
    FALSE,	FALSE,	FALSE,	FALSE,
    1,	0,
    "Error",	"&&",
    "Something is wrong, contact Flux immediatly.",
    "Something is wrong, contact Flux immediatly.",
    "Something is wrong, contact Flux immediatly."    
  },

/*
 * Magic spells.
 */

/*
    { "Name",
     { skill tree dependencies },
     { racial dependencies },
     SPELL_FUN, gsn, SLOT(slot),
     target, wait cost, stamina cost, mana cost,
     minimum range, maximum range, minimum position,
     arms dep, legs dep, vocal dep, movement dep,
     group size, dispel classification,
     "affect noun", "slist color code",
     "occupied message",
     "wear-off message",
     "to_room wear off message"
    },
*/

/*
 * Object commands.
 */

/*
    { "Name",
     { skill tree dependencies },
     { racial dependencies },
     SPELL_FUN, gsn, SLOT(slot),
     target, wait cost, stamina cost, mana cost,
     minimum range, maximum range, minimum position,
     arms dep, legs dep, vocal dep, movement dep,
     group size, dispel classification,
     "affect noun", "slist color code",
     "occupied message",
     "wear-off message",
     "to_room wear off message"
    },
*/

/*
 * Fighting commands.
 */

/*
    { "Name",
     { skill tree dependencies },
     { racial dependencies },
     SPELL_FUN, gsn, SLOT(slot),
     target, wait cost, stamina cost, mana cost,
     minimum range, maximum range, minimum position,
     arms dep, legs dep, vocal dep, movement dep,
     group size, dispel classification,
     "affect noun", "slist color code",
     "occupied message",
     "wear-off message",
     "to_room wear off message"
    },
*/

/*
 * Miscellaneous commands.
 */

/*
    { "Name",
     { skill tree dependencies },
     { racial dependencies },
     SPELL_FUN, gsn, SLOT(slot),
     target, wait cost, stamina cost, mana cost,
     minimum range, maximum range, minimum position,
     arms dep, legs dep, vocal dep, movement dep,
     group size, dispel classification,
     "affect noun", "slist color code",
     "occupied message",
     "wear-off message",
     "to_room wear off message"
    },
*/

/*
 * Place all new spells/skills BEFORE this one.  It is used as an index marker
 * in the same way that theres a blank entry at the end of the command table.
 * (in interp.c)
 * -- Altrag
 */

/* Far be it for me to mess with this, don't you either. -Flux */
  { "",
   { -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1,

     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,

     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },

   { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     -1, -1, -1, -1, -1 },

    0,	NULL,	SLOT(0),
    TAR_IGNORE,	0,	0,	0,
    0,	0,	POS_STANDING,
    FALSE,	FALSE,	FALSE,	FALSE,
    1,	0,
    "Error",	"&&",
    "Something is wrong, contact Flux immediatly.",
    "Something is wrong, contact Flux immediatly.",
    "Something is wrong, contact Flux immediatly."    
  }
};

const struct gskill_type gskill_table [MAX_GSPELL] =
{
  /*
   * The globals for group spells..
   * -- Altrag
   */
/*{wait,SLOT(slot),{WAR,HER,THI,MUR,MUT,PAG,RAD,OUT,HIP,VAM,
		    NEC,WWF,ASS,ILL,DEF,TER,PRO},*/
  { 3, SLOT(221), {0,0,0,0,0,0,0,0,0,1,0} },
  { 2, SLOT(227), {2,1,0,0,2,1,0,0,0,0,0} },
};
