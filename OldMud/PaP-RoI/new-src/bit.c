/***************************************************************************
 *  File: bit.c                                                            *
 *                                                                         *
 *  Much time and thought has gone into this software and you are          *
 *  benefitting.  We hope that you share your changes too.  What goes      *
 *  around, comes around.                                                  *
 *                                                                         *
 *  This code was written by Jason Dinkel and inspired by Russ Taylor,     *
 *  and has been used here for OLC - OLC would not be what it is without   *
 *  all the previous coders who released their source code.                *
 *                                                                         *
 ***************************************************************************/
/*
 The code below uses a table lookup system that is based on suggestions
 from Russ Taylor.  There are many routines in handler.c that would benefit
 with the use of tables.  You may consider simplifying your code base by
 implementing a system like below with such functions. -Jason Dinkel
*/


#define linux 1
#if defined(macintosh)
#include <types.h>
#else
#include <sys/types.h>
#endif
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include "merc.h"



struct flag_stat_type
{
    const struct flag_type *structure;
    bool stat;
};



/*****************************************************************************
 Name:		flag_stat_table
 Purpose:	This table catagorizes the tables following the lookup
 		functions below into stats and flags.  Flags can be toggled
 		but stats can only be assigned.  Update this table when a
 		new set of flags is installed.
 ****************************************************************************/
const struct flag_stat_type flag_stat_table[] =
{
/*  {	  structure			stat	}, */
    {	area_flags,		FALSE	},
    {   race_type_flags,	TRUE	},
    {   attitude_flags,		TRUE	},
    {   sex_flags,		TRUE	},
    {   exit_flags,		FALSE	},
    {	direction_flags,	TRUE	},
    {	switch_affect_types,	TRUE	},
    {	activation_commands,	TRUE	},
    {	door_action_flags,	TRUE	},
    {   door_resets,		TRUE	},
    {   room_flags,		FALSE	},
    {   sector_flags,		TRUE	},
    {   type_flags,		TRUE	},
    {   extra_flags,		FALSE	},
    {   wear_flags,		FALSE	},
    {   act_flags,		FALSE	},
    {   mpaffect_flags,       	FALSE 	},
    {   affect_flags,		FALSE	},
    {   affect2_flags,        	FALSE	},
    {   apply_flags,		TRUE	},
    {   fighting_styles,	TRUE	},
    {   wear_loc_flags,		TRUE	},
    {   wear_loc_strings,	TRUE	},
    {   warhead_flags,		TRUE	},
    {   propulsion_flags,	TRUE	},
    {   casino_games,		TRUE	},
    {   card_suits,		TRUE	},
    {   card_faces,		TRUE	},
    {   language_types,		TRUE	},
    {   area_weather_flags,  	TRUE	},
    {   weather_flags,  	TRUE	},
    {   sky_flags,	  	TRUE	},
    {   furniture_flags,	TRUE	},
    {   weapon_flags,		TRUE	},
    {	morph_armor,		TRUE	},
    {	morph_types,		TRUE	},
    {	morph_weapons,		TRUE	},
    {	assimilate_loc,		TRUE	},
    {	weaponclass_flags,	TRUE	},
    {   temporal_flags,  	TRUE	},
    {   tattoo_flags,		TRUE	},
    {   wear_tattoo,		TRUE	},
    {   object_materials,	TRUE	},
    {   arrow_types,		TRUE	},
    {   armor_types,    	TRUE	},
    {   container_flags,	FALSE	},
    {   liquid_flags,		TRUE	},
    {   food_condition,		TRUE	},
    {   gas_affects,		TRUE	},
    {   claw_flags,		TRUE	},
    {   immune_flags,         	FALSE	},
    {   mprog_types,          	TRUE	},
    {   oprog_types,          	FALSE	},
    {   rprog_types,          	FALSE	},
    {   eprog_types,          	FALSE	},
    {   size_flags,		TRUE	},
    {   mobhp_flags,		TRUE	},
    {	skin_flags,		TRUE	},
    {	gun_type_flags,		TRUE	},
    {	ranged_weapon_flags,	TRUE	},
    {	ranged_weapon_flags,	TRUE	},
    {	bionic_flags,		TRUE	},
    {	bionic_loc_strings,	TRUE	},
    {	damage_flags,		TRUE	},

    {   0,			0	}
};
    
/*****************************************************************************
 Name:		is_stat( table )
 Purpose:	Returns TRUE if the table is a stat table and FALSE if flag.
 Called by:	flag_value and flag_string.
 Note:		This function is local and used only in bit.c.
 ****************************************************************************/
bool is_stat( const struct flag_type *flag_table )
{
    int flag;

    for (flag = 0; flag_stat_table[flag].structure; flag++)
    {
	if ( flag_stat_table[flag].structure == flag_table
	  && flag_stat_table[flag].stat )
	    return TRUE;
    }
    return FALSE;
}



/*
 * This function is Russ Taylor's creation.  Thanks Russ!
 * All code copyright (C) Russ Taylor, permission to use and/or distribute
 * has NOT been granted.  Use only in this OLC package has been granted.
 */
/*****************************************************************************
 Name:		flag_lookup( flag, table )
 Purpose:	Returns the value of a single, settable flag from the table.
 Called by:	flag_value and flag_string.
 Note:		This function is local and used only in bit.c.
 ****************************************************************************/
int flag_lookup(const char *name, const struct flag_type *flag_table)
{
    int flag;
 
    for (flag = 0; *flag_table[flag].name; flag++)	/* OLC 1.1b */
    {
        if ( !str_cmp( name, flag_table[flag].name )
          && flag_table[flag].settable )
            return flag_table[flag].bit;
    }
 
    return NO_FLAG;
}



/*****************************************************************************
 Name:		flag_value( table, flag )
 Purpose:	Returns the value of the flags entered.  Multi-flags accepted.
 Called by:	olc.c and olc_act.c.
 ****************************************************************************/
int flag_value( const struct flag_type *flag_table, char *argument)
{
    char word[MAX_INPUT_LENGTH];
    int  bit;
    int  marked = 0;
    bool found = FALSE;

    if ( is_stat( flag_table ) )
    {
	one_argument( argument, word );

	if ( ( bit = flag_lookup( word, flag_table ) ) != NO_FLAG )
	    return bit;
	else
	    return NO_FLAG;
    }

    /*
     * Accept multiple flags.
     */
    for (; ;)
    {
        argument = one_argument( argument, word );

        if ( word[0] == '\0' )
	    break;

        if ( ( bit = flag_lookup( word, flag_table ) ) != NO_FLAG )
        {
            SET_BIT( marked, bit );
            found = TRUE;
        }
    }

    if ( found )
	return marked;
    else
	return NO_FLAG;
}



/*****************************************************************************
 Name:		flag_string( table, flags/stat )
 Purpose:	Returns string with name(s) of the flags or stat entered.
 Called by:	act_olc.c, olc.c, and olc_save.c.
 ****************************************************************************/
char *flag_string( const struct flag_type *flag_table, int bits )
{
    static char buf[512];
    int  flag;

    buf[0] = '\0';

    for (flag = 0; *flag_table[flag].name; flag++)	/* OLC 1.1b */
    {
	if ( !is_stat( flag_table ) && IS_SET(bits, flag_table[flag].bit) )
	{
	    strcat( buf, " " );
	    strcat( buf, flag_table[flag].name );
	}
	else
	if ( flag_table[flag].bit == bits )
	{
	    strcat( buf, " " );
	    strcat( buf, flag_table[flag].name );
	    break;
	}
    }
    return (buf[0] != '\0') ? buf+1 : "none";
}



const struct flag_type area_flags[] =
{
    {	"none",			AREA_NONE,		FALSE	},
    {	"changed",		AREA_CHANGED,		FALSE	},
    {	"added",		AREA_ADDED,		FALSE	},
    {	"loading",		AREA_LOADING,		FALSE	},
    {	"verbose",		AREA_VERBOSE,		FALSE	},
    {   "prototype",            AREA_PROTOTYPE,         FALSE   },
    {   "clan_hq",		AREA_CLAN_HQ,		FALSE	},
    {   "mudschool",		AREA_MUDSCHOOL,		FALSE   },
    {	"",			0,			0	}
};

const struct flag_type arrow_types[] =
{
    {	"standard",		ARROW_STANDARD,		TRUE	},
    {	"barbed",		ARROW_BARBED,		TRUE	},
    {	"",			0,			0	}
};

const struct flag_type attitude_flags[] =
{
    {	"offensive",		ATTITUDE_OFFENSIVE,	FALSE	},
    {	"protective",		ATTITUDE_PROTECTIVE,	FALSE	},
    {	"defensive",		ATTITUDE_DEFENSIVE,	FALSE	},
    {	"normal",		ATTITUDE_NORMAL,	FALSE	},
    {	"snake",		ATTITUDE_SNAKE,		FALSE	},
    {	"tiger",		ATTITUDE_TIGER,		FALSE	},
    {	"dragon",		ATTITUDE_DRAGON,	FALSE	},
    {	"panther",		ATTITUDE_PANTHER,	FALSE	},
    {	"sparrow",		ATTITUDE_SPARROW,	FALSE	},
    {	"bull",			ATTITUDE_BULL,		FALSE	},
    {	"monkey",		ATTITUDE_MONKEY,	FALSE	},
    {	"crane",		ATTITUDE_CRANE,		FALSE	},
    {	"",			0,			0	}
};

const struct flag_type sex_flags[] =
{
    {	"male",			SEX_MALE,		TRUE	},
    {	"female",		SEX_FEMALE,		TRUE	},
    {	"neutral",		SEX_NEUTRAL,		TRUE	},
    {	"",			0,			0	}
};

const struct flag_type switch_affect_types[] =
{
    {	"none",			SWITCH_NONE,	TRUE	},
    {	"door",			SWITCH_DOOR,	TRUE	},
    {	"objload",		SWITCH_OLOAD,	TRUE	},
    {	"mobload",		SWITCH_MLOAD,	TRUE	},
    {	"",			0,			0	}
};

const struct flag_type activation_commands[] =
{
    {	"none",		ACTIVATION_NONE,	TRUE	},
    {	"press",	ACTIVATION_PRESS,	TRUE	},
    {	"pull",		ACTIVATION_PULL,	TRUE	},
    {	"lift",		ACTIVATION_LIFT,	TRUE	},
    {	"lower",	ACTIVATION_LOWER,	TRUE	},
    {	"raise",	ACTIVATION_RAISE,	TRUE	},
    {	"",		0,			0	}
};

const struct flag_type door_action_flags[] =
{
    {	"none",		DOOR_ACTION_NONE,		TRUE	},
    {	"open",		DOOR_ACTION_OPEN,		TRUE	},
    {	"close",	DOOR_ACTION_CLOSE,		TRUE	},
    {	"lock",		DOOR_ACTION_LOCK,		TRUE	},
    {	"unlock",	DOOR_ACTION_UNLOCK,		TRUE	},
    {	"",		0,				0	}
};


const struct flag_type direction_flags[] =
{
    {	"north",	DIR_NORTH,		TRUE	},
    {	"south",	DIR_SOUTH,		TRUE	},
    {	"east",		DIR_EAST,		TRUE	},
    {	"west",		DIR_WEST,		TRUE	},
    {	"up",		DIR_UP,			TRUE	},
    {	"down",		DIR_DOWN,		TRUE	},
    {	"",		0,			0	}
};

const struct flag_type gun_type_flags[] =
{
    {	"standard",		GUN_NORMAL,		TRUE	},
    {	"semi-automatic",	GUN_SEMIAUTOMATIC,	TRUE	},
    {	"automatic",		GUN_AUTOMATIC,		TRUE	},
    {	"",			0,			0	}
};

const struct flag_type ranged_weapon_flags[] =
{
    {	"bow",		RANGED_WEAPON_BOW,	TRUE	},
    {	"firearm",	RANGED_WEAPON_FIREARM,	TRUE	},
    {	"bazooka",	RANGED_WEAPON_BAZOOKA,	TRUE	},
    {	"",		0,			0	}
};

const struct flag_type fighting_styles[] =
{
    {	"normal",	FIGHTING_STYLE_NORMAL,			TRUE	},
    {	"martial",	FIGHTING_STYLE_MARTIAL,			TRUE	},
    {	"street",	FIGHTING_STYLE_STREET,			TRUE	},
    {	"wimpy",	FIGHTING_STYLE_WIMPY,			TRUE	},
    {	"sneak",	FIGHTING_STYLE_SNEAK,			TRUE	},
    {	"beast-aggro",	FIGHTING_STYLE_BEAST_AGGRESSIVE,	TRUE	},
    {	"beast-wimpy",	FIGHTING_STYLE_BEAST_WIMPY,		TRUE	},
    {	"barbarian",	FIGHTING_STYLE_BARBARIAN,		TRUE	},
    {	"cleric",	FIGHTING_STYLE_CLERIC,			TRUE	},
    {	"warlock",	FIGHTING_STYLE_WARLOCK,			TRUE	},
    {	"",		0,					0	}
};

const struct flag_type bionic_flags[] =
{
    {	"eye",			ITEM_BIONIC_EYE,	TRUE	},
    {	"body",			ITEM_BIONIC_BODY,	TRUE	},
    {	"arm",			ITEM_BIONIC_ARM,	TRUE	},
    {	"hand",			ITEM_BIONIC_HAND,	TRUE	},
    {	"leg",			ITEM_BIONIC_LEG,	TRUE	},
    {	"implant",		ITEM_BIONIC_IMPLANT,	TRUE	},
    {	"memory",		ITEM_BIONIC_MEMORY,	TRUE	},
    {	"",			0,				}
};		

const struct flag_type language_types[] =
{
    {	"human",		LANGUAGE_HUMAN,			TRUE	},
    {	"elf",			LANGUAGE_ELF,			TRUE	},
    {	"dwarf",		LANGUAGE_DWARF,			TRUE	},
    {	"quicksilver",		LANGUAGE_QUICKSILVER,		TRUE	},
    {	"maudlin",		LANGUAGE_MAUDLIN,		TRUE	},
    {	"pixie",		LANGUAGE_PIXIE,			TRUE	},
    {	"felixi",		LANGUAGE_FELIXI,		TRUE	},
    {	"draconi",		LANGUAGE_DRACONI,		TRUE	},
    {	"gremlin",		LANGUAGE_GREMLIN,		TRUE	},
    {	"centaur",		LANGUAGE_CENTAUR,		TRUE	},
    {	"kender",		LANGUAGE_KENDER,		TRUE	},
    {	"minotaur",		LANGUAGE_MINOTAUR,		TRUE	},
    {	"drow",			LANGUAGE_DROW,			TRUE	},
    {	"aquinis",		LANGUAGE_AQUINIS,		TRUE	},
    {	"troll",		LANGUAGE_TROLL,			TRUE	},
    {	"",			0,			0	}
};

const struct flag_type race_type_flags[] =
{
    {	"humanoid",		RACE_TYPE_HUMANOID,		TRUE	},
    {	"lobster",		RACE_TYPE_LOBSTER,		TRUE	},
    {	"snake",		RACE_TYPE_SNAKE,		TRUE	},
    {	"bird",			RACE_TYPE_BIRD,			TRUE	},
    {	"dog",			RACE_TYPE_DOG,			TRUE	},
    {   "spider",		RACE_TYPE_SPIDER,		TRUE	},
    {   "flying-insect",	RACE_TYPE_FLYING_INSECT,	TRUE	},
    {	"",			0,				0	}
};

const struct flag_type exit_flags[] =
{
    {   "door",			EX_ISDOOR,		TRUE    },
    {	"closed",		EX_CLOSED,		TRUE	},
    {	"locked",		EX_LOCKED,		TRUE	},
    {	"bashed",		EX_BASHED,		FALSE	},
    {	"bashproof",		EX_BASHPROOF,		TRUE	},
    {	"pickproof",		EX_PICKPROOF,		TRUE	},
    {	"passproof",		EX_PASSPROOF,		TRUE	},
    {   "random",		EX_RANDOM,              TRUE    },
    {   "magiclock",            EX_MAGICLOCK,           TRUE    },
    {   "secret",               EX_SECRET,              TRUE    },
    {	"",			0,			0	}
};



const struct flag_type door_resets[] =
{
    {	"open and unlocked",	0,		TRUE	},
    {	"closed and unlocked",	1,		TRUE	},
    {	"closed and locked",	2,		TRUE	},
    {	"",			0,		0	}
};



const struct flag_type room_flags[] =
{
    {	"dark",			ROOM_DARK,		TRUE	},
    {	"no_mob",		ROOM_NO_MOB,		TRUE	},
    {	"indoors",		ROOM_INDOORS,		TRUE	},
    {	"underground",		ROOM_UNDERGROUND,	TRUE	},
    {	"private",		ROOM_PRIVATE,		TRUE	},
    {	"safe",			ROOM_SAFE,		TRUE	},
    {	"solitary",		ROOM_SOLITARY,		TRUE	},
    {	"pet_shop",		ROOM_PET_SHOP,		TRUE	},
    {	"no_recall",		ROOM_NO_RECALL,		TRUE	},
    {	"cone_of_silence",	ROOM_CONE_OF_SILENCE,	TRUE	},
    {   "no_in",                ROOM_NO_ASTRAL_IN,      TRUE    },
    {   "no_out",               ROOM_NO_ASTRAL_OUT,     TRUE    },
    {   "tele_area",            ROOM_TELEPORT_AREA,     TRUE    },
    {   "tele_world",           ROOM_TELEPORT_WORLD,    TRUE    },
    {   "no_magic",             ROOM_NO_MAGIC,          TRUE    },
    {   "no_offensive",         ROOM_NO_OFFENSIVE,      TRUE    },
    {   "no_flee",              ROOM_NO_FLEE,           TRUE    },
    {   "silent",               ROOM_SILENT,            TRUE    },
    {   "bank",		        ROOM_BANK,              TRUE    },
    {   "vault",	        ROOM_BANK_VAULT,        TRUE    },
    {   "nofloor",              ROOM_NOFLOOR,           TRUE    },
    {   "smithy",               ROOM_SMITHY,            TRUE    },
    {   "noscry",               ROOM_NOSCRY,            TRUE    },
    {   "void",                 ROOM_VOID,              TRUE    },
    {   "pkill",                ROOM_PKILL,             TRUE    },
    {   "no_shadow",		ROOM_NO_SHADOW,		TRUE	},
    {   "trapped",		ROOM_TRAPPED,		TRUE	},
    {	"",			0,			0	}
};



const struct flag_type sector_flags[] =
{
    {	"inside",	SECT_INSIDE,		TRUE	},
    {	"paved",	SECT_PAVED,		TRUE	},
    {	"field",	SECT_FIELD,		TRUE	},
    {	"forest",	SECT_FOREST,		TRUE	},
    {	"hills",	SECT_HILLS,		TRUE	},
    {	"mountain",	SECT_MOUNTAIN,		TRUE	},
    {	"surfacewater",	SECT_WATER_SURFACE,	TRUE	},
    {	"underwater",	SECT_UNDERWATER,	TRUE	},
    {	"desert",	SECT_DESERT,		TRUE	},
    {   "badland",      SECT_BADLAND,           TRUE    },
    {   "swamp",        SECT_SWAMP,             TRUE    },
    {   "cavern",       SECT_CAVERN,            TRUE    },
    {   "heaven",       SECT_HEAVEN,            TRUE    },
    {   "hell",         SECT_HELL,              TRUE    },
    {   "shadow",       SECT_SHADOW,            TRUE    },
    {	"air",		SECT_AIR,		TRUE	},
    {	"arctic",	SECT_ARCTIC,		TRUE	},
    {	"astral",	SECT_ASTRAL,		TRUE	},
    {	"",		0,			0	}
};



const struct flag_type type_flags[] =
{
    {	"scroll",		ITEM_SCROLL,		TRUE	},
    {	"wand",			ITEM_WAND,		TRUE	},
    {	"staff",		ITEM_STAFF,		TRUE	},
    {	"weapon",		ITEM_WEAPON,		TRUE	},
    {	"treasure",		ITEM_TREASURE,		TRUE	},
    {	"armor",		ITEM_ARMOR,		TRUE	},
    {	"potion",		ITEM_POTION,		TRUE	},
    {	"noteboard",            ITEM_NOTEBOARD,		TRUE	},
    {	"furniture",		ITEM_FURNITURE,		TRUE	},
    {	"trash",		ITEM_TRASH,		TRUE	},
    {	"container",		ITEM_CONTAINER,		TRUE	},
    {	"drink-container",	ITEM_DRINK_CON,		TRUE	},
    {	"misc",			ITEM_MISC,		TRUE	},
    {	"food",			ITEM_FOOD,		TRUE	},
    {	"money",		ITEM_MONEY,		TRUE	},
    {	"boat",			ITEM_BOAT,		TRUE	},
    {	"npc-corpse",		ITEM_CORPSE_NPC,	TRUE	},
    {	"rigor-corpse",		ITEM_RIGOR,	      	TRUE	},
    {	"skeleton",		ITEM_SKELETON,  	TRUE	},
    {	"pc-corpse",		ITEM_CORPSE_PC,		FALSE	},
    {	"fountain",		ITEM_FOUNTAIN,		TRUE	},
    {	"pill",			ITEM_PILL,		TRUE	},
    {	"contact",             	ITEM_LENSE,       	TRUE	},
    {	"portal",               ITEM_PORTAL,      	TRUE	},
    {	"doll",                 ITEM_VODOO,       	TRUE	},
    {	"berry",                ITEM_BERRY,       	TRUE	},
    {	"bomb",                 ITEM_BOMB,        	TRUE	},
    {	"ore",                  ITEM_ORE,         	TRUE	},
    {	"blood",                ITEM_BLOOD,       	TRUE	},
    {	"body-part",            ITEM_BODY_PART,   	TRUE	},
    {	"switch",		ITEM_SWITCH,		TRUE	},
    {	"scroll-paper",		ITEM_SCROLLPAPER,	TRUE	},
    {	"ballpoint-pen",	ITEM_PEN,		TRUE	},
    {	"beaker",		ITEM_BEAKER,		TRUE	},
    {	"chemistry-set",	ITEM_CHEMSET,		TRUE	},
    {	"bunsen-burner",	ITEM_BUNSEN,		TRUE	},
    {	"ink-catridge",		ITEM_INKCART,		TRUE	},
    {	"postalpaper",		ITEM_POSTALPAPER,	TRUE	},
    {	"letter",		ITEM_LETTER,		TRUE	},
    {	"smokebomb",		ITEM_SMOKEBOMB,		TRUE	},
    {	"smithy-hammer",	ITEM_SMITHYHAMMER,	TRUE	},
    {	"weapon-poison",	ITEM_POISONCHEM,	TRUE	},
    {	"napalm",		ITEM_MOLOTOVCHEM,	TRUE	},
    {	"wick",			ITEM_WICK,		TRUE	},
    {	"teargas",		ITEM_TEARCHEM,		TRUE	},
    {	"plastique",		ITEM_PIPEBOMBCHEM,	TRUE	},
    {	"chemical-gas",		ITEM_CHEMBOMBCHEM,	TRUE	},
    {	"timing-device",	ITEM_TIMER,		TRUE	},
    {	"pile-of-wires",	ITEM_PILEWIRE,		TRUE	},
    {	"tripwire",		ITEM_TRIPWIRE,		TRUE	},
    {	"tool-pack",		ITEM_TOOLPACK,		TRUE	},
    {	"cherrybomb-container",	ITEM_CHERRYCONTAINER,	TRUE	},
    {	"gun-powder",		ITEM_GUNPOWDER,		TRUE	},
    {	"smithy-pack",		ITEM_SMITHYPACK,	TRUE	},
    {	"smithy-anvil",		ITEM_SMITHYANVIL,	TRUE	},
    {	"sottering-iron",	ITEM_TECHNOSOTTER,	TRUE	},
    {	"techno-workstation",	ITEM_TECHNOWORKSTATION,	TRUE	},
    {   "ranged-weapon",	ITEM_RANGED_WEAPON,	TRUE	},
    {   "bullet",		ITEM_BULLET,		TRUE	},
    {   "clip",			ITEM_CLIP,		TRUE	},
    {   "gas-cloud",		ITEM_GAS_CLOUD,		TRUE	},
    {   "mummy-corpse",		ITEM_MUMMY,		TRUE	},
    {   "embalming-fluid",	ITEM_EMBALMING_FLUID,	TRUE	},
    {   "arrow",		ITEM_ARROW,		TRUE	},

    {	"",			0,			0	}
};


const struct flag_type extra_flags[] =
{
    {	"glow",			ITEM_GLOW,		TRUE	},
    {	"hum",			ITEM_HUM,		TRUE	},
    {	"dark",			ITEM_DARK,		TRUE	},
    {	"lock",			ITEM_LOCK,		TRUE	},
    {	"sharp",		ITEM_SHARP,		TRUE	},
    {	"balanced",		ITEM_BALANCED,		TRUE	},
    {	"invis",		ITEM_INVIS,		TRUE	},
    {	"magic",		ITEM_MAGIC,		TRUE	},
    {	"nodrop",		ITEM_NODROP,		TRUE	},
    {	"bless",		ITEM_BLESS,		TRUE	},
    {	"noremove",		ITEM_NOREMOVE,		TRUE	},
    {	"inventory",		ITEM_INVENTORY,		TRUE	},
    {	"poisoned",		ITEM_POISONED,		TRUE	},
    {   "dwarven",		ITEM_DWARVEN,		TRUE	},
    {   "nolocate",             ITEM_NO_LOCATE,	        TRUE    },
    {   "nodamage",             ITEM_NO_DAMAGE,         TRUE    },
    {   "flame",                ITEM_FLAME,             TRUE    },
    {   "shock",                ITEM_SHOCK,             TRUE    },
    {   "rainbow",              ITEM_RAINBOW,           TRUE    },
    {   "chaos",                ITEM_CHAOS,             TRUE    },
    {   "technological",        ITEM_TECHNOLOGY,        TRUE    },
    {   "nopreserve",		ITEM_NOEXIT,		TRUE    },
    {	"",			0,			0	}
};

const struct flag_type wear_tattoo[] =
{
    {	"face",			TATTOO_FACE,		TRUE	},
    {	"fneck",		TATTOO_FRONT_NECK,	TRUE	},
    {	"bneck",		TATTOO_BACK_NECK,	TRUE	},
    {	"lshoul",		TATTOO_LEFT_SH,		TRUE	},
    {	"rshoul",		TATTOO_RIGHT_SH,	TRUE	},
    {	"larm",			TATTOO_LEFT_ARM,	TRUE	},
    {	"rarm",			TATTOO_RIGHT_ARM,	TRUE	},
    {	"lhand",		TATTOO_LEFT_HAND,	TRUE	},
    {	"rhand",		TATTOO_RIGHT_HAND,	TRUE	},
    {	"chest",		TATTOO_CHEST,		TRUE	},
    {	"back",			TATTOO_BACK,		TRUE	},
    {	"lleg",			TATTOO_LEFT_LEG,	TRUE	},
    {	"rleg",			TATTOO_RIGHT_LEG,	TRUE	},
    {	"lankle",		TATTOO_LEFT_ANKLE,	TRUE	},
    {	"rankle",		TATTOO_RIGHT_ANKLE,	TRUE	},
    {	"forehead",		TATTOO_FOREHEAD,	TRUE	},
    {	"",			0,			0	}
};

const struct flag_type wear_flags[] =
{
    {	"take",			ITEM_TAKE,		TRUE	},
    {	"double",		ITEM_DOUBLE,		TRUE	},
    {	"finger",		ITEM_WEAR_FINGER,	TRUE	},
    {	"neck",			ITEM_WEAR_NECK,		TRUE	},
    {	"body",			ITEM_WEAR_BODY, 	TRUE	},
    {	"head",			ITEM_WEAR_HEAD,		TRUE	},
    {	"legs",			ITEM_WEAR_LEGS,		TRUE	},
    {	"feet",			ITEM_WEAR_FEET,		TRUE	},
    {	"hands",		ITEM_WEAR_HANDS,	TRUE	},
    {	"arms",			ITEM_WEAR_ARMS,		TRUE	},
    {	"shield",		ITEM_WEAR_SHIELD,	TRUE	},
    {	"about",		ITEM_WEAR_ABOUT,	TRUE	},
    {	"waist",		ITEM_WEAR_WAIST,	TRUE	},
    {	"wrist",		ITEM_WEAR_WRIST,	TRUE	},
    {	"wield",		ITEM_WIELD,		TRUE	},
    {	"lense",              	ITEM_WEAR_CONTACT,	TRUE	},
    {	"orbit",              	ITEM_WEAR_ORBIT,        TRUE	},
    {	"mask",               	ITEM_WEAR_FACE,         TRUE	},
    {	"ears",               	ITEM_WEAR_EARS,         TRUE	},
    {	"ankle",              	ITEM_WEAR_ANKLE,        TRUE	},
    {	"medallion",          	ITEM_MEDALLION,         TRUE	},
    {	"sheath",		ITEM_WEAR_SHEATH,	TRUE	},
    {	"",				0,		0	}
};

const struct flag_type bionic_loc_strings[] =
{
    {   "in the inventory",	BIONIC_NONE,	TRUE	},
    {   "left eye",		BIONIC_EYE_L,	TRUE	},
    {	"right eye",		BIONIC_EYE_R,	TRUE	},
    {	"body",			BIONIC_BODY,	TRUE	},
    {	"left arm",		BIONIC_ARM_L,	TRUE	},
    {	"right arm",		BIONIC_ARM_R,	TRUE	},
    {	"left hand",		BIONIC_HAND_L,	TRUE	},
    {	"right hand",		BIONIC_HAND_R,	TRUE	},
    {	"left leg",		BIONIC_LEG_L,	TRUE	},
    {	"right leg",		BIONIC_LEG_R,	TRUE	},
    {	"implant(1)",		BIONIC_IMPLANT1,TRUE	},
    {	"implant(2)",		BIONIC_IMPLANT2,TRUE	},
    {	"implant(3)",		BIONIC_IMPLANT3,TRUE	},
    {	"implant(4)",		BIONIC_IMPLANT4,TRUE	},
    {	"implant(5)",		BIONIC_IMPLANT5,TRUE	},
    {	"implant(6)",		BIONIC_IMPLANT6,TRUE	},
    {	"implant(7)",		BIONIC_IMPLANT7,TRUE	},
    {	"implant(8)",		BIONIC_IMPLANT8,TRUE	},
    {	"implant(9)",		BIONIC_IMPLANT9,TRUE	},
    {	"implant(10)",		BIONIC_IMPLANT10,TRUE	},
    {	"memory(1)",		BIONIC_MEMORY1,	TRUE	},
    {	"memory(2)",		BIONIC_MEMORY2,	TRUE	},
    {	"memory(3)",		BIONIC_MEMORY3, TRUE	},
    {	"memory(4)",		BIONIC_MEMORY4, TRUE	},
    {	"",			0			}
};	

const struct flag_type act_flags[] =
{
    {	"npc",			ACT_IS_NPC,		FALSE	},
    {	"sentinel",		ACT_SENTINEL,		TRUE	},
    {	"scavenger",		ACT_SCAVENGER,		TRUE	},
    {	"aggressive",		ACT_AGGRESSIVE,		TRUE	},
    {	"stay_area",		ACT_STAY_AREA,		TRUE	},
    {	"wimpy",		ACT_WIMPY,		TRUE	},
    {	"pet",			ACT_PET,		TRUE	},
    {	"train",		ACT_TRAIN,		TRUE	},
    {	"practice",		ACT_PRACTICE,		TRUE	},
    {	"gamble",		ACT_GAMBLE,		TRUE	},
    {   "undead",               ACT_UNDEAD,             TRUE    },
    {   "track",                ACT_TRACK,              TRUE    },
    {	"postman",		ACT_POSTMAN,		TRUE	},
    {	"nopush",		ACT_NOPUSH,		TRUE	},
    {	"nodrag",		ACT_NODRAG,		TRUE	},
    {   "noshadow",		ACT_NOSHADOW,		TRUE    },
    {   "noastral",		ACT_NOASTRAL,		TRUE	},
    {   "illusion",		ACT_ILLUSION,		TRUE	},
    {	"",			0,			0	}
};

const struct flag_type mpaffect_flags[] =
{
    {   "plasma",               AFF_PLASMA,             TRUE    },
    {   "hallucinatory",        AFF_HALLUCINATING,      TRUE    },
    {   "insanity",             AFF_INSANE,             TRUE    },
    {   "disease",              AFF_DISEASED,           TRUE    },
    {   "poison",               AFF_POISON,             TRUE    },             
    {   "",                     0,                      0       }
};

const struct flag_type affect_flags[] =
{
    {	"blind",		AFF_BLIND,		TRUE	},
    {	"invisible",		AFF_INVISIBLE,		TRUE	},
    {	"detect-invis",		AFF_DETECT_INVIS,	TRUE	},
    {	"detect-hidden",	AFF_DETECT_HIDDEN,	TRUE	},
    {	"haste",		AFF_HASTE,		TRUE	},
    {	"cureaura",		AFF_CUREAURA,		TRUE	},
    {   "fireshield",           AFF_FIRESHIELD,         TRUE    },
    {   "shockshield",          AFF_SHOCKSHIELD,        TRUE    },
    {   "iceshield",            AFF_ICESHIELD,          TRUE    },
    {   "chaos-field",          AFF_CHAOS,              TRUE    },
    {	"vibrating",		AFF_VIBRATING,		TRUE	},
    {	"faerie-fire",		AFF_FAERIE_FIRE,	TRUE	},
    {	"infrared",		AFF_INFRARED,		TRUE	},
    {	"curse",		AFF_CURSE,		TRUE	},
    {	"flaming",		AFF_FLAMING,		FALSE	},
    {	"poison",		AFF_POISON,		TRUE	},
    {	"sneak",		AFF_SNEAK,		TRUE	},
    {	"hide",			AFF_HIDE,		TRUE	},
    {	"sleep",		AFF_SLEEP,		TRUE	},
    {	"charm",		AFF_CHARM,		TRUE	},
    {	"flying",		AFF_FLYING,		TRUE	},
    {	"breathe-water",	AFF_BREATHE_WATER,	TRUE	},
    {	"pass-door",		AFF_PASS_DOOR,		TRUE	},
    {	"temporal",		AFF_TEMPORAL,		TRUE	},
    {	"siamese-soul",		AFF_SIAMESE,		FALSE	},
    {	"summoned",		AFF_SUMMONED,		TRUE	},
    {	"mute",			AFF_MUTE,		TRUE	},
    {	"aura-of-peace",	AFF_PEACE,		TRUE	},
    {	"",			0,			0	}
};

const struct flag_type affect2_flags [] =
{
    {   "mental-block",         AFF_NOASTRAL,           TRUE    },
    {   "clap",                 AFF_CLAP,		TRUE	},
    {   "true-sight",           AFF_TRUESIGHT,          TRUE    },
    {   "blade-barrier",        AFF_BLADE,              TRUE    },
    {   "berserk",              AFF_BERSERK,            FALSE   },
    {   "rage",			AFF_RAGE,		TRUE	},
    {   "adrenaline-rush",	AFF_RUSH,		TRUE	},
    {   "inertial",		AFF_INERTIAL,		TRUE	},
    {   "acid-blood",		AFF_ACIDBLOOD,		TRUE	},
    {   "mana-net",		AFF_MANANET,		TRUE	},
    {   "mana-shield",		AFF_MANASHIELD,		TRUE	},
    {   "vampiric-aspect",	AFF_VAMPIRIC,		TRUE	},
    {   "grasping",		AFF_GRASPING,		FALSE	},
    {   "grasped",		AFF_GRASPED,		FALSE	},
    {   "",                     0,                      0       }
};


/*
 * Used when adding an affect to tell where it goes.
 * See addaffect and delaffect in act_olc.c
 */
const struct flag_type apply_flags[] =
{
    {	"none",			APPLY_NONE,		TRUE	},
    {	"strength",		APPLY_STR,		TRUE	},
    {	"dexterity",		APPLY_DEX,		TRUE	},
    {	"intelligence",		APPLY_INT,		TRUE	},
    {	"wisdom",		APPLY_WIS,		TRUE	},
    {	"constitution",		APPLY_CON,		TRUE	},
    {	"agility",		APPLY_AGI,		TRUE	},
    {	"charisma",		APPLY_CHA,		TRUE	},
    {	"pdamp",		APPLY_PDAMP,		TRUE	},
    {	"mdamp",		APPLY_MDAMP,		TRUE	},
    {	"sex",			APPLY_SEX,		TRUE	},
    {	"race", 		APPLY_RACE,		TRUE	},
    {	"age",			APPLY_AGE,		TRUE	},
    {	"hp",			APPLY_HIT,		TRUE	},
    {	"mana",			APPLY_MANA,		TRUE	},
    {	"move",			APPLY_MOVE,		TRUE	},
    {	"hitroll",		APPLY_HITROLL,		TRUE	},
    {	"damroll",		APPLY_DAMROLL,		TRUE	},

    {	"imm-heat",		APPLY_IMM_HEAT,		TRUE	},
    {	"imm-positive",		APPLY_IMM_POSITIVE,	TRUE	},
    {	"imm-cold",		APPLY_IMM_COLD,		TRUE	},
    {	"imm-negative",		APPLY_IMM_NEGATIVE,	TRUE	},
    {	"imm-holy",		APPLY_IMM_HOLY,		TRUE	},
    {	"imm-unholy",		APPLY_IMM_UNHOLY,	TRUE	},
    {	"imm-regen",		APPLY_IMM_REGEN,	TRUE	},
    {	"imm-degen",		APPLY_IMM_DEGEN,	TRUE	},
    {	"imm-dynamic",		APPLY_IMM_DYNAMIC,	TRUE	},
    {	"imm-void",		APPLY_IMM_VOID,		TRUE	},
    {	"imm-pierce",		APPLY_IMM_PIERCE,	TRUE	},
    {	"imm-slash",		APPLY_IMM_SLASH,	TRUE	},
    {	"imm-scratch",		APPLY_IMM_SCRATCH,	TRUE	},
    {	"imm-bash",		APPLY_IMM_BASH,		TRUE	},
    {	"imm-internal",		APPLY_IMM_INTERNAL,	TRUE	},
   
    {	"invis",		APPLY_INVISIBLE,	TRUE	},
    {	"detect-invis",		APPLY_DETECT_INVIS,	TRUE	},
    {	"hide",	        	APPLY_HIDE,     	TRUE	},
    {	"sneak",		APPLY_SNEAK,		TRUE	},
    {	"scry",	        	APPLY_SCRY,		TRUE	},
    {	"detect-hide",	 	APPLY_DETECT_HIDDEN,	TRUE	},
    {	"fly",   		APPLY_FLYING,		TRUE	},
    {	"breathe water",	APPLY_BREATHE_WATER,	TRUE	},
    {	"infrared",		APPLY_INFRARED,		TRUE	},
    {	"pass-door",		APPLY_PASS_DOOR,	TRUE	},
    {	"temporal-field",	APPLY_TEMPORAL_FIELD,	TRUE	},
    {	"poison",		APPLY_POISON,		TRUE	},
    {	"heighten-senses",      APPLY_HEIGHTEN_SENSES,  TRUE	},
    {	"fireshield",		APPLY_ICESHIELD,  	TRUE	},
    {	"iceshield",      	APPLY_FIRESHIELD,  	TRUE	},
    {	"throwplus",      	APPLY_THROWPLUS,  	TRUE	},
    {	"",			0,			0	}
};



/*
 * What is seen.
 */
const struct flag_type wear_loc_strings[] =
{
    { "in the inventory",	WEAR_NONE,	TRUE	},
    { "on the left finger",	WEAR_FINGER_L,	TRUE	},
    { "on the right finger",	WEAR_FINGER_R,	TRUE	},
    { "around the neck",	WEAR_NECK,	TRUE	},
    { "body layer 1",		WEAR_BODY_1,	TRUE	},
    { "body layer 2",		WEAR_BODY_2,	TRUE	},
    { "body layer 3",		WEAR_BODY_3,	TRUE	},
    { "as a medal",		WEAR_MEDALLION,	TRUE	},
    { "over the head",		WEAR_HEAD,	TRUE	},
    { "in the eyes",        	WEAR_IN_EYES,   TRUE    },
    { "on the face",		WEAR_ON_FACE,   TRUE    },
    { "spinning around",	WEAR_ORBIT,     TRUE    },
    { "on the legs",		WEAR_LEGS,	TRUE	},
    { "on the feet",		WEAR_FEET,	TRUE	},
    { "on the hands",		WEAR_HANDS,	TRUE	},
    { "on the arms",		WEAR_ARMS,	TRUE	},
    { "as a shield",		WEAR_SHIELD,	TRUE	},
    { "about the shoulders",	WEAR_ABOUT,	TRUE	},
    { "around the waist",	WEAR_WAIST,	TRUE	},
    { "on the left wrist",	WEAR_WRIST_L,	TRUE	},
    { "on the right wrist",	WEAR_WRIST_R,	TRUE	},
    { "held primary",		WEAR_WIELD,	TRUE	},
    { "held secondary",		WEAR_WIELD_2,   TRUE	},
    { "on the left ankle",    	WEAR_ANKLE_L,   TRUE	},
    { "on the right ankle",   	WEAR_ANKLE_R,   TRUE	},
    { "in the left ear",      	WEAR_EAR_L,     TRUE	},
    { "in the right ear",     	WEAR_EAR_R,     TRUE	},
    { "attached to waist (1)",	WEAR_SHEATH_1,	TRUE	},
    { "attached to waist (2)",	WEAR_SHEATH_2,	TRUE	},
    { "",			0			}
};


/*
 * What is typed.
 * Neck2 should not be settable for loaded mobiles.
 */
const struct flag_type wear_loc_flags[] =
{
    {	"none",		WEAR_NONE,		TRUE	},
    {	"lfinger",		WEAR_FINGER_L,	TRUE	},
    {	"rfinger",		WEAR_FINGER_R,	TRUE	},
    {	"neck",		WEAR_NECK,		TRUE	},
    {	"body1",		WEAR_BODY_1,	TRUE	},
    {	"body2",		WEAR_BODY_2,	TRUE	},
    {	"body3",		WEAR_BODY_3,	TRUE	},
    {	"head",		WEAR_HEAD,		TRUE	},
    { "lense",		WEAR_IN_EYES,   	TRUE  },
    { "mask",		WEAR_ON_FACE,   	TRUE  },
    { "orbit",       	WEAR_ORBIT,     	TRUE  },
    {	"legs",		WEAR_LEGS,		TRUE	},
    {	"feet",		WEAR_FEET,		TRUE	},
    {	"hands",		WEAR_HANDS,		TRUE	},
    {	"arms",		WEAR_ARMS,		TRUE	},
    {	"shield",		WEAR_SHIELD,	TRUE	},
    {	"about",		WEAR_ABOUT,		TRUE	},
    {	"waist",		WEAR_WAIST,		TRUE	},
    {	"lwrist",		WEAR_WRIST_L,	TRUE	},
    {	"rwrist",		WEAR_WRIST_R,	TRUE	},
    {	"wielded",		WEAR_WIELD,		TRUE	},
    { "dual",		WEAR_WIELD_2,   	TRUE  },
    { "lankle",       	WEAR_ANKLE_L,   	TRUE  },
    { "rankle",       	WEAR_ANKLE_R,   	TRUE  },
    { "lear",         	WEAR_EAR_L,     	TRUE  },
    { "rear",         	WEAR_EAR_R,     	TRUE  },
    { "lsheath",		WEAR_SHEATH_1,	TRUE	},
    {	"rsheath",		WEAR_SHEATH_2,	TRUE	},
    {	"",			0,			0	}
};


const struct flag_type tattoo_flags[] =
{
    {	"none",		MAGBOO_NONE,		TRUE	},
    {	"heat",	     	MAGBOO_HEAT,		TRUE	},
    {	"positive",	MAGBOO_POSITIVE,	TRUE	},
    {	"cold",		MAGBOO_COLD,		TRUE	},
    {	"negative",	MAGBOO_NEGATIVE,	TRUE	},
    {	"holy",		MAGBOO_HOLY,		TRUE	},
    {	"unholy",	MAGBOO_UNHOLY,		TRUE	},
    {	"regen", 	MAGBOO_REGEN,		TRUE	},
    {	"degen",	MAGBOO_DEGEN,		TRUE	},
    {	"dynamic",	MAGBOO_DYNAMIC,		TRUE	},
    {	"void",		MAGBOO_VOID,		TRUE	},
    {	"",		0,			0	}
};

const struct flag_type warhead_flags[] =
{
    {	"explosive",	BOMB_EXPLOSIVE,	TRUE	},
    {	"napalm",	BOMB_NAPALM,	TRUE	},
    {	"plasmatic",	BOMB_PLASMA,	TRUE	},
    {	"tear-gas",	BOMB_TEAR,	TRUE	},
    {	"chemical",	BOMB_CHEMICAL,	TRUE	},
    {	"nuclear",	BOMB_NUKE,	TRUE	},
    {	"",		0,		0	}
};

const struct flag_type blade_flags[] =
{
    {	"pierce",	1,			TRUE	},
    {	"slash",	0,			TRUE	},
    {	"",		0,			0	}
};

const struct flag_type assimilate_loc[] =
{
    {   "torso",	ASSIM_TORSO,	TRUE	},
    {   "right-arm",	ASSIM_RARM,	TRUE	},
    {   "left-arm",	ASSIM_LARM,	TRUE	},
    {   "right-leg",	ASSIM_RLEG,	TRUE	},
    {   "left-leg",	ASSIM_LLEG,	TRUE	},
    {   "extra-1",	ASSIM_EXTRA_1,	TRUE	},
    {   "extra-2",	ASSIM_EXTRA_2,	TRUE	},
    {   "extra-3",	ASSIM_EXTRA_3,	TRUE	},
    {   "extra-4",	ASSIM_EXTRA_4,	TRUE	},
    {   "extra-5",	ASSIM_EXTRA_5,	TRUE	},
    {   "",             0,              0       }
};

const struct flag_type morph_types[] =
{
    {   "weapon",       MORPH_WEAPON,          TRUE    },
    {   "armor",        MORPH_ARMOR,           TRUE    },
    {   "avoidance",    MORPH_AVOIDANCE,       TRUE    },
    {   "size",		MORPH_SIZE,	       TRUE    },
    {   "camoflauge",   MORPH_CAMO,	       TRUE    },
    {   "",             0,                     0       }
};

const struct flag_type morph_weapons[] =
{
    {	"hands",	0,			TRUE	},
    {   "claws",        MORPH_WEAPON_CLAW,      TRUE    },
    {   "blades",       MORPH_WEAPON_SLASH,     TRUE    },
    {   "hammers",      MORPH_WEAPON_BASH,      TRUE    },
    {   "axes",         MORPH_WEAPON_CHOP,      TRUE    },
    {   "daggers",      MORPH_WEAPON_PIERCE,    TRUE    },
    {   "",             0,                      0       }
};

const struct flag_type morph_armor[] =
{
    {   "nothing",      0,                      TRUE    },
    {   "tearing",      MORPH_WEAPON_CLAW,      TRUE    },
    {   "slashing",     MORPH_WEAPON_SLASH,     TRUE    },
    {   "bashing",      MORPH_WEAPON_BASH,      TRUE    },
    {   "chopping",     MORPH_WEAPON_CHOP,      TRUE    },
    {   "stabbing",     MORPH_WEAPON_PIERCE,    TRUE    },
    {   "",             0,                      0       }
};

const struct flag_type weaponclass_flags[] =
{
    {	"pierce",	WEAPON_PIERCE,		TRUE	},
    {	"slash",	WEAPON_SLASH,		TRUE	},
    {	"bash",		WEAPON_BASH,		TRUE	},
    {	"chop",		WEAPON_CHOP,		TRUE	},
    {	"tear",		WEAPON_TEAR,		TRUE	},
    {	"exotic",	WEAPON_EXOTIC,		TRUE	},
    {   "blade",	WEAPON_BLADE,		TRUE	},
    {	"",		0,			0	}
};

const struct flag_type furniture_flags[]=
{
    {	"misc",		FURNITURE_MISC,		TRUE	},
    {	"chair",	FURNITURE_CHAIR,	TRUE	},
    {	"sofa",		FURNITURE_SOFA,		TRUE	},
    {	"bed",		FURNITURE_BED,		TRUE	},
    {	"desk",		FURNITURE_DESK,		TRUE	},
    {	"armoir",	FURNITURE_ARMOIR,	TRUE	},
    {	"",		0,			0	}
};

const struct flag_type weapon_flags[] =
{
    {   "strike",	DAMNOUN_STRIKE,		TRUE	},
    {	"pierce",	DAMNOUN_PIERCE,		TRUE	},
    {	"slash",	DAMNOUN_SLASH,		TRUE	},
    {	"bash",		DAMNOUN_BASH,		TRUE	},
    {	"chop",		DAMNOUN_CHOP,		TRUE	},
    {	"tear",		DAMNOUN_TEAR,		TRUE	},
    {	"claw",		DAMNOUN_CLAW,		TRUE	},
    {   "bite",         DAMNOUN_BITE,		TRUE	},
    {   "slap",         DAMNOUN_SLAP,		TRUE	},
    {   "crush",	DAMNOUN_CRUSH,		TRUE	},
    {	"smash",	DAMNOUN_SMASH,		TRUE	},
    {	"overhead smash", DAMNOUN_OSMASH,	TRUE	},

   /* for generic elemental types -Flux */
    {	"firey strike",		DAMNOUN_HEAT,	FALSE},
    {	"frozen smash",		DAMNOUN_COLD,	FALSE},
    {	"positronic strike", 	DAMNOUN_POSITIVE, FALSE},
    {	"electric blast",	DAMNOUN_NEGATIVE, FALSE},
    {	"demonic blast",	DAMNOUN_UNHOLY, FALSE},
    {	"divine strike",	DAMNOUN_HOLY,	FALSE},
    {	"acidic blast",		DAMNOUN_DEGEN,	FALSE},
    {	"healing stream",	DAMNOUN_REGEN,	FALSE},
    {	"chaotic blast",	DAMNOUN_DYNAMIC, FALSE},
    {	"entropic strike",	DAMNOUN_VOID,	FALSE},
    {	"",		0,			0	}
};

const struct flag_type propulsion_flags[] =
{
    {	"none",		PROP_NONE,		TRUE	},
    {	"local",	PROP_LOCAL,		TRUE	},
    {	"areawide",	PROP_AREA,		TRUE	},
    {	"global",	PROP_GLOBAL,		TRUE	},
    {	"temporal",	PROP_TEMPORAL,		TRUE	},
    {	"dimensional",	PROP_DIMENSIONAL,	TRUE	},
    {	"",		0,			0	}
};

const struct flag_type casino_games[] =
{
    {	"none",			CASINO_NONE,		TRUE	},
    {	"craps",		CASINO_CRAPS,		TRUE	},
    {	"poker",		CASINO_POKER,		TRUE	},
    {	"blackjack",		CASINO_BLACKJACK,	TRUE	},
    {	"roulette",		CASINO_ROULETTE,	TRUE	},
    {	"simple-dice",		CASINO_SIMPLE_DICE,	TRUE	},
    {	"three-card-monty",	CASINO_THREE_CARD_MONTY,TRUE	},
    {	"",			0,			0	}
};

const struct flag_type card_suits[] =
{
    {	"spades",	CARD_SPADES,		TRUE	},
    {	"hearts",	CARD_HEARTS,		TRUE	},
    {	"clubs",	CARD_CLUBS,		TRUE	},
    {	"diamonds",	CARD_DIAMONDS,		TRUE	},
    {	"",		0,			0	}
};

const struct flag_type card_faces[] =
{
    {	"ace",		CARD_ACE,		TRUE	},
    {	"2",		CARD_TWO,		TRUE	},
    {	"3",		CARD_THREE,		TRUE	},
    {	"4",		CARD_FOUR,		TRUE	},
    {	"5",		CARD_FIVE,		TRUE	},
    {	"6",		CARD_SIX,		TRUE	},
    {	"7",		CARD_SEVEN,		TRUE	},
    {	"8",		CARD_EIGHT,		TRUE	},
    {	"9",		CARD_NINE,		TRUE	},
    {	"10",		CARD_TEN,		TRUE	},
    {	"jack",		CARD_JACK,		TRUE	},
    {	"queen",	CARD_QUEEN,		TRUE	},
    {	"king",		CARD_KING,		TRUE	},
    {	"",		0,			0	}
};

const struct flag_type weather_flags[] =
{
    {	"clear",  		WEATHER_CLEAR,		TRUE	},
    {	"rainy",  		WEATHER_RAIN,		TRUE	},
    {	"snowy",  		WEATHER_SNOW,		TRUE	},
    {	"",			0,				0	}
};

const struct flag_type area_weather_flags[] =
{
    {	"stagnant", 	AREA_WEATHER_STAGNANT,		TRUE	},
    {	"normal",  	AREA_WEATHER_NORMAL,		TRUE	},
    {	"stormy",  	AREA_WEATHER_STORMY,		TRUE	},
    {	"torrential",  	AREA_WEATHER_TORRENTIAL,	TRUE	},
    {	"",		0,				0	}
};

const struct flag_type sky_flags[] =
{
    {	"clear",  				SKY_CLEAR,			TRUE	},
    {	"partially cloudy",  		SKY_PARTIAL,		TRUE	},
    {	"cloudy",  				SKY_CLOUDY,			TRUE	},
    {	"overcast",  			SKY_OVERCAST,		TRUE	},
    {	"",					0,				0	}
};

const struct flag_type temporal_flags[] =
{
    {	"past", 	TEMPORAL_PAST,		TRUE	},
    {	"present", 	TEMPORAL_PRESENT,	TRUE	},
    {	"future", 	TEMPORAL_FUTURE,	TRUE	},
    {	"",		0,			0	}
};

const struct flag_type object_materials[] =
{
    {	"cloth",	MATERIAL_CLOTH,		TRUE	},
    {	"kevlar",	MATERIAL_KEVLAR,	TRUE	},
    {	"leather",	MATERIAL_LEATHER,	TRUE	},
    {	"stone",	MATERIAL_STONE,		TRUE	},
    {	"wood",	        MATERIAL_WOOD,		TRUE	},
    {	"gold",		MATERIAL_GOLD,		TRUE	},
    {	"silver",	MATERIAL_SILVER,	TRUE	},
    {	"copper",	MATERIAL_COPPER,	TRUE	},
    {	"plastic",	MATERIAL_PLASTIC,	TRUE	},
    {	"glass",	MATERIAL_GLASS,		TRUE	},
    {	"iron",	        MATERIAL_IRON,		TRUE	},
    {	"lead",	        MATERIAL_LEAD,		TRUE	},
    {	"fire",		MATERIAL_FIRE,		TRUE	},
    {	"ice",		MATERIAL_ICE,		TRUE	},
    {	"energy",	MATERIAL_ENERGY,	TRUE	},
    {	"crystal",	MATERIAL_CRYSTAL,	TRUE	},
    {	"bone",		MATERIAL_BONE,		TRUE	},
    {	"steel",	MATERIAL_STEEL,		TRUE	},
    {	"bronze",	MATERIAL_BRONZE,	TRUE	},
    {	"brass",	MATERIAL_BRASS,		TRUE	},
    {	"omni-crystal",	MATERIAL_OMNI_CRYSTAL,	TRUE	},
    {	"plasma",	MATERIAL_PLASMA,	TRUE	},
    {	"omni-steel",	MATERIAL_OMNI_STEEL,	TRUE	},
    {	"living-metal", MATERIAL_LIVING_METAL,	TRUE	},
    {	"",		0,	0	}
};

const struct flag_type armor_types[] =
{
    {	"padded",	0,	TRUE	},
    {	"splint",	1,	TRUE	},
    {	"ring",		2,	TRUE	},
    {	"fish", 	3,	TRUE	},
    {	"scale",	4,	TRUE	},
    {	"chain",	5,	TRUE	},
    {	"plate",	6,	TRUE	},
    {	"piecemeal",	7,	TRUE	},
    {	"",		0,	0	}
};

const struct flag_type container_flags[] =
{
    {	"closeable",		1,		TRUE	},
    {	"pickproof",		2,		TRUE	},
    {	"closed",		4,		TRUE	},
    {	"locked",		8,		TRUE	},
    {	"",			0,		0	}
};



const struct flag_type liquid_flags[] =
{
    {	"water",		0,	TRUE	},
    {	"beer",			1,	TRUE	},
    {	"wine",			2,	TRUE	},
    {	"ale",			3,	TRUE	},
    {	"dark-ale",		4,	TRUE	},
    {	"whisky",		5,	TRUE	},
    {	"lemonade",		6,	TRUE	},
    {	"firebreather",		7,	TRUE	},
    {	"local-specialty",	8,	TRUE	},
    {	"slime-mold-juice",	9,	TRUE	},
    {	"milk",			10,	TRUE	},
    {	"tea",			11,	TRUE	},
    {	"coffee",		12,	TRUE	},
    {	"blood",		13,	TRUE	},
    {	"salt-water",		14,	TRUE	},
    {	"cola",			15,	TRUE	},
    {	"",			0,	0	}
};

const struct flag_type food_condition[] =
{
    {	"pure",		FOOD_PURE,			TRUE	},
    {	"poisoned",		FOOD_POISONED,		TRUE	},
    {	"diseased",		FOOD_DISEASED,		TRUE	},
    {	"insanity",		FOOD_INSANE,		TRUE	},
    {	"hallucinatory",	FOOD_HALLUCINATORY,	TRUE	},
    {	"",			0,	0	}
};

const struct flag_type gas_affects[] =
{
    {	"pure",			GAS_AFFECT_NONE,		TRUE	},
    {	"poisoned",		GAS_AFFECT_POISONED,		TRUE	},
    {	"diseased",		GAS_AFFECT_DISEASED,		TRUE	},
    {	"insanity",		GAS_AFFECT_INSANE,		TRUE	},
    {	"hallucinatory",	GAS_AFFECT_HALLUCINATORY,	TRUE	},
    {	"stun",			GAS_AFFECT_STUN,		TRUE	},
    {	"",			0,				0	}
};

const struct flag_type claw_flags[] =
{
    {	"none",		CLAW_NONE,	TRUE	},
    {	"normal",	CLAW_NORMAL,	TRUE	},
    {	"poisoned",	CLAW_POISONED,	TRUE	},
    {	"diseased",	CLAW_DISEASED,	TRUE	},
    {	"",		0,		0	}
};

const struct flag_type size_flags[] =
{
    {	"tiny",		SIZE_TINY,			TRUE	},
    {	"small",		SIZE_SMALL,			TRUE	},
    {	"average",		SIZE_MEDIUM,		TRUE	},
    {	"large",		SIZE_LARGE,			TRUE	},
    {	"giant",		SIZE_GIANT,			TRUE	},
    {	"huge",		SIZE_HUGE,			TRUE	},
    {	"gargantuan",	SIZE_GARGANTUAN,		TRUE	},
    {	"titanic",		SIZE_TITANIC,		TRUE	},
    {	"",			0,				0	}
};

const struct flag_type mobhp_flags[] =
{
    {	"very-weak",	MOBHP_VERY_WEAK,		TRUE	},
    {	"weak",		MOBHP_WEAK,			TRUE	},
    {	"average",		MOBHP_AVERAGE,		TRUE	},
    {	"strong",		MOBHP_STRONG,		TRUE	},
    {	"very-strong",	MOBHP_VERY_STRONG,	TRUE	},
    {	"",			0,				0	}
};

const struct flag_type skin_flags[] =
{
    {	"human",		SKIN_HUMAN,			TRUE	},
    {	"horse",		SKIN_HORSE,			TRUE	},
    {	"cow",		SKIN_COW,			TRUE	},
    {	"wood",		SKIN_WOOD,			TRUE	},
    {	"stone",		SKIN_STONE,			TRUE	},
    {	"steel",		SKIN_STEEL,			TRUE	},
    {	"",			0,				0	}
};

const struct flag_type damage_flags[] =
{
    {   "heat",		DAM_HEAT,     TRUE   },
    {   "positive",	DAM_POSITIVE, TRUE   },
    {   "cold",		DAM_COLD,     TRUE   },
    {   "negative",	DAM_NEGATIVE, TRUE   },
    {   "holy",		DAM_HOLY,     TRUE   },
    {   "unholy",	DAM_UNHOLY,   TRUE   },
    {   "regen",	DAM_REGEN,    TRUE   },
    {   "degen",	DAM_DEGEN,    TRUE   },
    {   "dynamic",	DAM_DYNAMIC,  TRUE   },
    {   "void",		DAM_VOID,     TRUE   },
    {   "pierce",	DAM_PIERCE,   TRUE   },
    {   "slash",	DAM_SLASH,    TRUE   },
    {   "scratch",	DAM_SCRATCH,  TRUE   },
    {   "bash",		DAM_BASH,     TRUE   },
    {   "internal",	DAM_INTERNAL, TRUE   },
    {   "",          0,             0      }
};

const struct flag_type immune_flags[] =
{
    {   "heat",		IMM_HEAT,     TRUE   },
    {   "positive",	IMM_POSITIVE, TRUE   },
    {   "cold",		IMM_COLD,     TRUE   },
    {   "negative",	IMM_NEGATIVE, TRUE   },
    {   "holy",		IMM_HOLY,     TRUE   },
    {   "unholy",	IMM_UNHOLY,   TRUE   },
    {   "regen",	IMM_REGEN,    TRUE   },
    {   "degen",	IMM_DEGEN,    TRUE   },
    {   "dynamic",	IMM_DYNAMIC,  TRUE   },
    {   "void",		IMM_VOID,     TRUE   },
    {   "pierce",	IMM_PIERCE,   TRUE   },
    {   "slash",	IMM_SLASH,    TRUE   },
    {   "scratch",	IMM_SCRATCH,  TRUE   },
    {   "bash",		IMM_BASH,     TRUE   },
    {   "internal",	IMM_INTERNAL, TRUE   },
    {   "",          0,             0      }
};

const struct flag_type mprog_types[] =
{
    {   "error_prog",     ERROR_PROG,     FALSE  },
    {   "in_file_prog",   IN_FILE_PROG,   FALSE  },
    {   "act_prog",       ACT_PROG,       TRUE   },
    {   "speech_prog",    SPEECH_PROG,    TRUE   },
    {   "rand_prog",      RAND_PROG,      TRUE   },
    {   "rand_vict_prog", RAND_VICT_PROG, TRUE   },
    {   "fight_prog",     FIGHT_PROG,     TRUE   },
    {   "death_prog",     DEATH_PROG,     TRUE   },
    {   "hitprcnt_prog",  HITPRCNT_PROG,  TRUE   },
    {   "entry_prog",     ENTRY_PROG,     TRUE   },
    {   "greet_prog",     GREET_PROG,     TRUE   },
    {   "all_greet_prog", ALL_GREET_PROG, TRUE   },
    {   "give_prog",      GIVE_PROG,      TRUE   },
    {   "bribe_prog",     BRIBE_PROG,     TRUE   },
    {	"spell_prog",	  SPELL_PROG,	  TRUE	 },
    {   "",               0,              0      }
};

const struct flag_type oprog_types[] =
{
    {   "error_prog",     OBJ_TRAP_ERROR,     FALSE  },
    {   "get_prog",       OBJ_TRAP_GET,       TRUE   },
    {   "get_from_prog",  OBJ_TRAP_GET_FROM,  TRUE   },
    {   "drop_prog",      OBJ_TRAP_DROP,      TRUE   },
    {   "put_prog",       OBJ_TRAP_PUT,       TRUE   },
    {   "give_prog",      OBJ_TRAP_GIVE,      TRUE   },
    {   "fill_prog",      OBJ_TRAP_FILL,      TRUE   },
    {   "wear_prog",      OBJ_TRAP_WEAR,      TRUE   },
    {   "look_prog",      OBJ_TRAP_LOOK,      TRUE   },
    {   "look_in_prog",   OBJ_TRAP_LOOK_IN,   TRUE   },
    {   "invoke_prog",    OBJ_TRAP_INVOKE,    TRUE   },
    {   "use_prog",       OBJ_TRAP_USE,       TRUE   },
    {   "cast_prog",      OBJ_TRAP_CAST,      TRUE   },
    {   "cast_sn_prog",   OBJ_TRAP_CAST_SN,   TRUE   },
    {   "join_prog",      OBJ_TRAP_JOIN,      TRUE   },
    {   "separate_prog",  OBJ_TRAP_SEPARATE,  TRUE   },
    {   "buy_prog",       OBJ_TRAP_BUY,       TRUE   },
    {   "sell_prog",      OBJ_TRAP_SELL,      TRUE   },
    {   "store_prog",     OBJ_TRAP_STORE,     TRUE   },
    {   "retrieve_prog",  OBJ_TRAP_RETRIEVE,  TRUE   },
    {   "open_prog",      OBJ_TRAP_OPEN,      TRUE   },
    {   "close_prog",     OBJ_TRAP_CLOSE,     TRUE   },
    {   "lock_prog",      OBJ_TRAP_LOCK,      TRUE   },
    {   "unlock_prog",    OBJ_TRAP_UNLOCK,    TRUE   },
    {   "pick_prog",      OBJ_TRAP_PICK,      TRUE   },
    {   "throw_prog",     OBJ_TRAP_THROW,     TRUE   },
    {   "rand_prog",      OBJ_TRAP_RANDOM,    TRUE   },
    {   "remove_prog",      OBJ_TRAP_REMOVE,      TRUE   },

    {   "",               0,                  0      }
};

const struct flag_type rprog_types[] =
{
    {   "error_prog",     ROOM_TRAP_ERROR,    FALSE  },
    {   "enter_prog",     ROOM_TRAP_ENTER,    TRUE   },
    {   "exit_prog",      ROOM_TRAP_EXIT,     TRUE   },
    {   "pass_prog",      ROOM_TRAP_PASS,     TRUE   },
    {   "cast_prog",      ROOM_TRAP_CAST,     TRUE   },
    {   "cast_sn_prog",   ROOM_TRAP_CAST_SN,  TRUE   },
    {   "sleep_prog",     ROOM_TRAP_SLEEP,    TRUE   },
    {   "wake_prog",      ROOM_TRAP_WAKE,     TRUE   },
    {   "rest_prog",      ROOM_TRAP_REST,     TRUE   },
    {   "death_prog",     ROOM_TRAP_DEATH,    TRUE   },
    {   "time_prog",      ROOM_TRAP_TIME,     TRUE   },
    {   "rand_prog",      ROOM_TRAP_RANDOM,   TRUE   },
    {   "",               0,                  0      }
};

const struct flag_type eprog_types[] =
{
    {   "error_prog",     EXIT_TRAP_ERROR,    FALSE  },
    {   "enter_prog",     EXIT_TRAP_ENTER,    TRUE   },
    {   "exit_prog",      EXIT_TRAP_EXIT,     TRUE   },
    {   "pass_prog",      EXIT_TRAP_PASS,     TRUE   },
    {   "look_prog",      EXIT_TRAP_LOOK,     TRUE   },
    {   "scry_prog",      EXIT_TRAP_SCRY,     TRUE   },
    {   "open_prog",      EXIT_TRAP_OPEN,     TRUE   },
    {   "close_prog",     EXIT_TRAP_CLOSE,    TRUE   },
    {   "lock_prog",      EXIT_TRAP_LOCK,     TRUE   },
    {   "unlock_prog",    EXIT_TRAP_UNLOCK,   TRUE   },
    {   "pick_prog",      EXIT_TRAP_PICK,     TRUE   },
    {   "",               0,                  0      }
};
