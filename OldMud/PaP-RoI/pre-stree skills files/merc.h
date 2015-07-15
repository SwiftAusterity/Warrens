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

/* To turn on and off certain features off the mud */
#define linux 1
#define NOCRYPT
#define HOTREBOOT 0
#include "colors.h"   /* Include the ansi color routines. */

/* This is for twin coding -Flux */
#define GAMEPORT_PAP 9292
#define GAMEPORT_ROI 2929
#define PORT_PAP 0
#define PORT_ROI 1
#define PORT_ALL 2


/*
 * Accommodate old non-Ansi compilers.
 */
#if defined( TRADITIONAL )
#define const
#define args( list )			( )
#define DECLARE_DO_FUN( fun )		void fun( )
#define DECLARE_SPEC_FUN( fun )		bool fun( )
#define DECLARE_SPELL_FUN( fun )	void fun( )
#define DECLARE_TOUCH_FUN( fun )	void fun( )

#else
#define args( list )			list
#define DECLARE_DO_FUN( fun )		DO_FUN    fun
#define DECLARE_SPEC_FUN( fun )		SPEC_FUN  fun
#define DECLARE_SPELL_FUN( fun )	SPELL_FUN fun
#define DECLARE_TOUCH_FUN( fun )	TOUCH_FUN fun
#endif



/*
 * Short scalar types.
 * Diavolo reports AIX compiler has bugs with short types.
 */
#if	!defined( FALSE )
#define FALSE	 0
#endif

#if	!defined( TRUE )
#define TRUE	 1
#endif

#if	defined( _AIX )
#if	!defined( const )
#define const
#endif
typedef int				bool;
#define unix
#else
typedef unsigned char			bool;
#endif



/*
 * Structure types.
 */
typedef struct	affect_data		AFFECT_DATA;
typedef struct	area_data		AREA_DATA;
typedef struct	arena_data		ARENA_DATA;
typedef struct  new_clan_data		CLAN_DATA;
typedef struct	ban_data		BAN_DATA;
typedef struct  gskill_data		GSPELL_DATA;
typedef struct	char_data		CHAR_DATA;
typedef struct  social_data		SOCIAL_DATA;
typedef struct  race_data		RACE_DATA;
typedef struct	descriptor_data		DESCRIPTOR_DATA;
typedef struct	exit_data		EXIT_DATA;
typedef struct	exit_affect_data	EXIT_AFFECT_DATA;
typedef struct	extra_descr_data	EXTRA_DESCR_DATA;
typedef struct	help_data		HELP_DATA;
typedef struct	kill_data		KILL_DATA;
typedef struct	mob_index_data		MOB_INDEX_DATA;
typedef struct	note_data		NOTE_DATA;
typedef struct	obj_data		OBJ_DATA;
typedef struct	obj_index_data		OBJ_INDEX_DATA;
typedef struct	pc_data			PC_DATA;
typedef struct	tattoo_data		TATTOO_DATA;
typedef struct	reset_data		RESET_DATA;
typedef struct	room_affect_data	ROOM_AFFECT_DATA;
typedef struct	powered_data		POWERED_DATA;
typedef struct	room_index_data		ROOM_INDEX_DATA;
typedef struct  casino_data		CASINO_DATA;
typedef struct	shop_data		SHOP_DATA;
typedef struct	tattoo_artist_data	TATTOO_ARTIST_DATA;
typedef struct	time_info_data		TIME_INFO_DATA;
typedef struct  mob_prog_data           MPROG_DATA;
typedef struct  mob_prog_act_list       MPROG_ACT_LIST;
typedef struct  quest_data              QUEST_DATA;
typedef struct  alias_data              ALIAS_DATA;     /* Altrag */
typedef struct  trap_data               TRAP_DATA;      /* Altrag */
typedef struct	playerlist_data		PLAYERLIST_DATA; /*Decklarean*/
typedef struct  skill_type		SKILL_TYPE;
typedef struct	queue_data		Q_DATA; /* Flux */
typedef struct	skill_data		SKILL_DATA;
typedef struct	skill_tree_data		SKILL_TREE_DATA;

typedef struct money_data	 	MONEY_DATA; 
typedef struct card_data	 	CARD_DATA; 
typedef struct economy_data		ECONOMY_DATA;

#define S_PER_G				10
#define C_PER_S				10
#define C_PER_G				(S_PER_G * C_PER_S)

#define SILVER_PER_GOLD			10
#define COPPER_PER_SILVER		10
#define COPPER_PER_GOLD			(SILVER_PER_GOLD*COPPER_PER_SILVER)


/*
 * Function types.
 */
typedef	void DO_FUN                     args( ( CHAR_DATA *ch,
					       char *argument ) );
typedef bool SPEC_FUN                   args( ( CHAR_DATA *ch ) );
typedef void SPELL_FUN                  args( ( int sn, int level,
					       CHAR_DATA *ch, void *vo ) );
typedef void TOUCH_FUN                  args( ( int sn, int level, 
					       CHAR_DATA *ch, void *vo ) );

/*
 * String and memory management parameters.
 */
#define	MAX_KEY_HASH		 16384  /*8192*/ /*1024*/
#define MAX_STRING_LENGTH	 8192
#define MAX_INPUT_LENGTH	  256



/*
 * Game parameters.
 * Increase the max'es if you add more of something.
 * Adjust the pulse numbers to suit yourself.
 */
#define MAX_ITEM_TYPE		66
#define MAX_SKILL		452
#define MAX_GSPELL		2
#define MAX_CLASS		56

/* This is for twin coding, allows the system to screen
 * what classes are what port quickly. As the name suggests
 * it is the last class associated with the first port.
 */
#define LAST_FIRSTPORTCLASS 10
#define MAX_RACE        25
#define MAX_CLAN        21 /*max 20 clans + 1 for clan 0*/
#define MAX_LEVEL	163
#define STUN_MAX	5
#define L_IMP		MAX_LEVEL
#define L_CON		( L_IMP - 1 )
#define L_DIR		( L_CON - 1 )
#define L_SEN  	        ( L_DIR - 1 )
#define L_GOD		( L_SEN - 1 )
#define L_DEM		( L_GOD - 1 )
#define L_JUN	        ( L_DEM - 1 )
#define L_APP		( L_JUN - 1 )
#define L_CHAMP5	( L_APP - 1 )
#define L_CHAMP4	( L_CHAMP5 - 1 )
#define L_CHAMP3	( L_CHAMP4 - 1 )
#define L_CHAMP2	( L_CHAMP3 - 1 )
#define L_CHAMP1	( L_CHAMP2 - 1 )
#define LEVEL_HERO	150

#define LEVEL_IMMORTAL	156
#define LEVEL_MORTAL 	155

#define PULSE_PER_SECOND	  6
#define PULSE_TIMEBOMB		  ( PULSE_PER_SECOND )
#define PULSE_VIOLENCE		  (  2 * PULSE_PER_SECOND )
#define PULSE_MOBILE		  (  6 * PULSE_PER_SECOND )
#define PULSE_OBJECT_DECAY	  ( 20 * PULSE_PER_SECOND )
#define PULSE_TICK		  ( 45 * PULSE_PER_SECOND )
#define PULSE_AREA		  ( 60 * PULSE_PER_SECOND )

/* Save the database - OLC 1.1b */
#define PULSE_DB_DUMP		  (1800* PULSE_PER_SECOND ) /* 30 minutes  */

/* Skill tree player numerical index */
#define CRAFT		0
#define CRAFT_TAILOR	1
#define CRAFT_JEWEL	2
#define	CRAFT_MINING	3
#define CRAFT_CARPEN	4
#define CRAFT_SMITH	5
#define	CRAFT_SMITH_WEAPON	6
#define CRAFT_SMITH_ARMOR	7
#define CRAFT_SMITH_LOCKS	8

#define	EVAL		9
#define EVAL_MEDICAL	10
#define	EVAL_WEAPON	11
#define EVAL_ARMOR	12
#define EVAL_JEWEL	13
#define EVAL_TACTICS	14

#define TECH		15
#define TECH_WEAPON		16
#define TECH_WEAPON_FIREARM	17
#define TECH_WEAPON_EXPLOSIVE	18
#define TECH_POWER	19
#define TECH_CIRCUIT	20
#define TECH_AUTO	21
#define TECH_ENERGY	22
#define TECH_BIO	23
#define TECH_CHEM	24

#define COVERT		25
#define COVERT_STEALTH		26
#define COVERT_STEALTH_SNEAK	27
#define COVERT_STEALTH_HIDE	28
#define COVERT_THEFT	29
#define COVERT_DECEPTION	30
#define COVERT_PERCEPTION	31
#define COVERT_TRAP	32

#define PHYS		33
#define PHYS_MEDICINE	34
#define PHYS_ATHLETE		35
#define PHYS_ATHLETE_SWIM	36
#define PHYS_ATHLETE_CLIMB	37
#define PHYS_ATHLETE_RIDE	38
#define PHYS_ATHLETE_BODY	39
#define PHYS_COMBAT	40
#define PHYS_COMBAT_AMBI	41
#define PHYS_COMBAT_WEAPON	42
#define PHYS_COMBAT_WEAPON_SLASH	43
#define PHYS_COMBAT_WEAPON_PIERCE	44
#define PHYS_COMBAT_WEAPON_BASH		45
#define PHYS_COMBAT_WEAPON_TEAR		46
#define PHYS_COMBAT_WEAPON_CHOP		47
#define PHYS_COMBAT_WEAPON_LONG		48
#define PHYS_COMBAT_WEAPON_MEDIUM	49
#define PHYS_COMBAT_WEAPON_SHORT	50
#define PHYS_COMBAT_OFFENSIVE	51
#define PHYS_COMBAT_OFFENSIVE_UNARMED	52
#define PHYS_COMBAT_OFFENSIVE_WEAPON	53
#define PHYS_COMBAT_DEFENSIVE	54
#define PHYS_COMBAT_DEFENSIVE_UNARMED	55
#define PHYS_COMBAT_DEFENSIVE_WEAPON	56
#define PHYS_COMBAT_DEFENSIVE_DODGE	57
#define PHYS_COMBAT_MARTIAL	58
#define PHYS_COMBAT_MARTIAL_OFFENSIVE	59
#define PHYS_COMBAT_MARTIAL_DEFENSIVE	60
#define PHYS_COMBAT_MARTIAL_DISC	61
#define PHYS_COMBAT_MARTIAL_DISC_VIPER		62
#define PHYS_COMBAT_MARTIAL_DISC_CRANE		63
#define PHYS_COMBAT_MARTIAL_DISC_MONKEY		64
#define PHYS_COMBAT_MARTIAL_DISC_BULL		65
#define PHYS_COMBAT_MARTIAL_DISC_TIGER		66
#define PHYS_COMBAT_MARTIAL_DISC_DRAGON		67
#define PHYS_COMBAT_MARTIAL_DISC_PANTHER	68
#define PHYS_COMBAT_MARTIAL_DISC_SPARROW	69
#define PHYS_COMBAT_RANGED		70
#define PHYS_COMBAT_RANGED_ARCHERY	71
#define PHYS_COMBAT_RANGED_THROWN		72
#define PHYS_COMBAT_RANGED_THROWN_SHORT	73
#define PHYS_COMBAT_RANGED_THROWN_MEDIUM	74
#define PHYS_COMBAT_RANGED_THROWN_LONG		75

#define MAGIC	76
#define MAGIC_VOODOO	77
#define MAGIC_PRESTI	78
#define MAGIC_SPELLCRAFT	79
#define MAGIC_TEMPORAL	80
#define MAGIC_ALCHEMY	81
#define MAGIC_PERCEPTION	82
#define MAGIC_ARCANE	83
#define MAGIC_ARCANE_RECITE	84
#define MAGIC_ARCANE_INVOKE	85
#define MAGIC_ARTI	86
#define MAGIC_ARTI_ENCHANT	87
#define MAGIC_ARTI_CRAFT	88
#define MAGIC_ARTI_IMBUE	89
#define MAGIC_ARTI_IMBUE_SCROLL	90
#define MAGIC_ARTI_IMBUE_WAND	91
#define MAGIC_ARTI_IMBUE_STAFF	92
#define MAGIC_NATURAL	93
#define MAGIC_NATURAL_HERBAL	94
#define MAGIC_NATURAL_ECOMANCY	95
#define MAGIC_AUG	96
#define MAGIC_AUG_ENHANCE	97
#define MAGIC_AUG_ENHANCE_ENERGY	98
#define MAGIC_AUG_ENHANCE_PHYSICAL	99
#define MAGIC_AUG_CURSE	100
#define MAGIC_AUG_CURSE_ENERGY	101
#define MAGIC_AUG_CURSE_PHYSICAL	102
#define MAGIC_ELEMENTAL	103
#define MAGIC_ELEMENTAL_HEAT	104
#define MAGIC_ELEMENTAL_COLD	105
#define MAGIC_ELEMENTAL_NEG	106
#define MAGIC_ELEMENTAL_POS	107
#define MAGIC_ELEMENTAL_HOLY	108
#define MAGIC_ELEMENTAL_UNHOLY	109
#define MAGIC_ELEMENTAL_REGEN	110
#define MAGIC_ELEMENTAL_DEGEN	111
#define MAGIC_ELEMENTAL_DYN	112
#define MAGIC_ELEMENTAL_VOID	113
#define MAGIC_TRANS	114
#define MAGIC_TRANS_MYTHICAL	115
#define MAGIC_TRANS_NATURAL	116
#define MAX_SKILL_TREE		117

/* Not used -Deck */
/* Now they are... -Flux */
#define SIZE_TINY                 0
#define SIZE_SMALL                1
#define SIZE_MEDIUM               2
#define SIZE_LARGE                3
#define SIZE_GIANT                4
#define SIZE_HUGE                 5
#define SIZE_GARGANTUAN           6
#define SIZE_TITANIC              7

/* Mob hp modifier -Flux */
#define MOBHP_VERY_WEAK		1
#define MOBHP_WEAK		2
#define MOBHP_AVERAGE		3
#define MOBHP_STRONG		4
#define MOBHP_VERY_STRONG	5

/* Mob skin modifier -Flux */
#define SKIN_HUMAN		1
#define SKIN_HORSE		2
#define SKIN_COW			3
#define SKIN_WOOD			4
#define SKIN_STONE		5
#define SKIN_STEEL		6

struct card_data
{
  int		suit;
  int		value;
};

#define CARD_SPADES	1
#define CARD_HEARTS	2
#define CARD_CLUBS	3
#define CARD_DIAMONDS	4

#define CARD_ACE		1
#define CARD_TWO		2
#define CARD_THREE	3
#define CARD_FOUR		4
#define CARD_FIVE		5
#define CARD_SIX		6
#define CARD_SEVEN	7
#define CARD_EIGHT	8
#define CARD_NINE		9
#define CARD_TEN		10
#define CARD_JACK		11
#define CARD_QUEEN	12
#define CARD_KING		13

struct money_data
{
  int           gold;
  int           silver;
  int           copper;
};

struct arena_data
{
  AREA_DATA *area;	/* Arena area */
  CHAR_DATA *cch;		/* Challenger char */
  CHAR_DATA *och;		/* optional challengee char */
  CHAR_DATA *fch;		/* First char in arena */
  CHAR_DATA *sch;		/* Second char in arena */
  int award;		/* Money in the pot */
  int count;		/* Update ticker */
};


struct wiznet_type
{
    char *      name;
    long        flag;
    int         level;
};
            
/*
 * Site ban structure.
 */
struct	ban_data
{
    BAN_DATA *	next;
    char	type;
    char *	name;
    char *      user;
};


bool    MOBtrigger;

#define ERROR_PROG        -1
#define IN_FILE_PROG       0
#define ACT_PROG           1
#define SPEECH_PROG        2
#define RAND_PROG          4
#define FIGHT_PROG         8
#define DEATH_PROG        16
#define HITPRCNT_PROG     32
#define ENTRY_PROG        64
#define GREET_PROG       128
#define ALL_GREET_PROG   256
#define GIVE_PROG        512
#define BRIBE_PROG      1024
#define SPELL_PROG	2048
#define RAND_VICT_PROG  4096

/*
 * Time and weather stuff.
 */
#define SUN_EVENING			0
#define SUN_MIDNIGHT			1
#define SUN_DARK			2
#define SUN_DAWN			3
#define SUN_MORNING			4
#define SUN_NOON			5
#define SUN_AFTERNOON			6
#define SUN_DUSK			7

#define MOON_FULL			0
#define MOON_FTHIRD			1
#define MOON_FHALF			2
#define MOON_FCRESCENT			3
#define MOON_NEW			4
#define MOON_NCRESCENT			5
#define MOON_NHALF			6
#define MOON_NTHIRD			7

#define AREA_WEATHER_STAGNANT		0
#define AREA_WEATHER_NORMAL		1
#define AREA_WEATHER_STORMY		2
#define AREA_WEATHER_TORRENTIAL		3

#define SKY_CLEAR			0
#define SKY_PARTIAL			1
#define SKY_CLOUDY			2
#define SKY_OVERCAST			3

#define WEATHER_CLEAR			0
#define WEATHER_RAIN			1
#define WEATHER_SNOW			2
 
#define TEMPORAL_PAST			-1
#define TEMPORAL_PRESENT		0
#define TEMPORAL_FUTURE			1

struct	time_info_data
{
    int		sunlight;
    int		phase_white;
    int		phase_shadow;
    int		phase_blood;
    int		moon_white;
    int		moon_shadow;
    int		moon_blood;
    int		hour;
    int		day;
    int		month;
    int		year;
};

struct  economy_data
{
    int		item_type[MAX_ITEM_TYPE];
    int		cost_modifier[MAX_ITEM_TYPE];
    int		market_type;
};

/* 
 * WIZnet flags 
 */
#define WIZ_ON			      1
#define WIZ_TICKS		      2
#define WIZ_LOGINS		      4
#define WIZ_SITES		      8
#define WIZ_LINKS		     16
#define WIZ_DEATHS		     32
#define WIZ_RESETS		     64
#define WIZ_MOBDEATHS		    128 
#define WIZ_FLAGS		    256
#define WIZ_PENALTIES		    512
#define WIZ_SACCING		   1024
#define WIZ_LEVELS		   2048 
#define WIZ_SECURE		   4096
#define WIZ_SWITCHES		   8192
#define WIZ_SNOOPS		  16384
#define WIZ_RESTORE	          32768
#define WIZ_LOAD		  65536
#define WIZ_NEWBIE		 131072
#define WIZ_PREFIX		 262144
#define WIZ_SPAM		 524288
#define WIZ_GENERAL		1048576
#define WIZ_OLDLOG		2097152


/*
 * Connected state for a channel.
 */
#define CON_PLAYING			0
#define CON_GET_NAME			1
#define CON_GET_OLD_PASSWORD		2
#define CON_CONFIRM_NEW_NAME		3
#define CON_GET_NEW_PASSWORD		4
#define CON_CONFIRM_NEW_PASSWORD	5
#define CON_GET_NEW_SEX			6
#define CON_GET_NEW_CLASS		7
#define CON_READ_MOTD			8
#define CON_GET_NEW_RACE		9
#define CON_CONFIRM_RACE           	10
#define CON_CONFIRM_CLASS          	11
#define CON_CHECK_AUTHORIZE		12
#define CON_REMORT_FIRST	      21
#define CON_GET_REMORT  	      29
#define CON_WANT_REMORT 	      23
#define CON_CHECK_DEPUTY	      24
#define CON_GET_DEPUTY  	      25
#define CON_CONFIRM_DEPUTY	      26
#define CON_CONFIRM_WANTED	      27
#define CON_CONFIRM_REMORT	      28
#define CON_REMORT_SECOND	      22

#ifdef HOTREBOOT
#define CON_HOTREBOOT_RECOVER	       30
#endif

#define CON_GET_ANSI			105
#define CON_AUTHORIZE_NAME		100
#define CON_AUTHORIZE_NAME1		101
#define CON_AUTHORIZE_NAME2		102
#define CON_AUTHORIZE_NAME3		103
#define CON_AUTHORIZE_LOGOUT		104
#define CON_CHATTING                200

/*
 * Descriptor (channel) structure.
 */
struct	descriptor_data
{
    DESCRIPTOR_DATA *	next;
    DESCRIPTOR_DATA *	snoop_by;
    CHAR_DATA *		character;
    CHAR_DATA *		original;
    char *		host;
    char *              user;
    int		        descriptor;
    int		        connected;
    bool		fcommand;
    char		inbuf		[ MAX_INPUT_LENGTH*4 ];
    char		incomm		[ MAX_INPUT_LENGTH   ];
    char		inlast		[ MAX_INPUT_LENGTH   ];
    int			repeat;
    char *              showstr_head;
    char *              showstr_point;
    char *		outbuf;
    int			outsize;
    int			outtop;
    void *              pEdit;		/* OLC */
    void *              inEdit;         /* Altrag, for nested editors */
    char **             pString;	/* OLC */
    int			editor;		/* OLC */
    int                 editin;         /* Altrag, again for nesting */
    bool		ansi;
    int		port;
};



/*
 * Attribute bonus structures.
 */
struct	str_app_type
{
    int 	        tohit;
    int         	todam;
    int                 carry;
    int         	wield;
};

struct	int_app_type
{
    int         	learn;
};

struct	wis_app_type
{
    int         	practice;
};

struct	dex_app_type
{
    int         	defensive;
};

struct	con_app_type
{
    int         	hitp;
    int         	shock;
};

struct	agi_app_type
{
    int         	speed;
};

struct	cha_app_type
{
    int         	charm;
};



/*
 * TO types for act.
 */
#define TO_ROOM		    0
#define TO_NOTVICT	    1
#define TO_VICT		    2
#define TO_CHAR		    3
#define TO_COMBAT           4



/*
 * Help table types.
 */
struct	help_data
{
    HELP_DATA * 	next;
    int 	        level;
    char *      	keyword;
    char *      	text;
};


/*
 * Structure for social in the socials list.
 */
struct  social_data
{
    SOCIAL_DATA	*	next;
    char *              name;
    char *              char_no_arg;
    char *  	        others_no_arg;
    char * 	        char_found;
    char * 	        others_found;
    char * 	        vict_found;
    char * 	        char_auto;
    char * 	        others_auto;
};

/*
 * Shop types.
 */
#define MAX_TRADE	 5

struct	shop_data
{
    SHOP_DATA *	next;			/* Next shop in list		*/
    int 	keeper;			/* Vnum of shop keeper mob	*/
    int 	buy_type [ MAX_TRADE ];	/* Item types shop will buy	*/
    int 	profit_buy;		/* Cost multiplier for buying	*/
    int 	profit_sell;		/* Cost multiplier for selling	*/
    int 	open_hour;		/* First opening hour		*/
    int 	close_hour;		/* First closing hour		*/
};

struct  casino_data
{
    CASINO_DATA	*next;
    int		game;
    int		dealer;
    int		pot;
    int		ante_min;
    int		ante_max;
};

struct	tattoo_artist_data
{
    TATTOO_ARTIST_DATA *next;
    int 		artist;
    MONEY_DATA		cost;
    int 		wear_loc;
    int                 magic_boost;
    AFFECT_DATA *	affected;
};

/*
 *   Player list structure.
 */
struct playerlist_data
{
  PLAYERLIST_DATA * next;
  char * name;
  unsigned char level;
  char * clan_name;
  unsigned char clan_rank;
};
/* Languages */
#define LANGUAGE_NONE		-1
#define LANGUAGE_HUMAN		0
#define LANGUAGE_ELF		1
#define LANGUAGE_DWARF		2
#define LANGUAGE_QUICKSILVER	3
#define LANGUAGE_MAUDLIN	4
#define LANGUAGE_PIXIE		5
#define LANGUAGE_FELIXI		6
#define LANGUAGE_DRACONI	7
#define LANGUAGE_GREMLIN	8
#define LANGUAGE_CENTAUR	9
#define LANGUAGE_KENDER		10
#define LANGUAGE_MINOTAUR	11
#define LANGUAGE_DROW		12
#define LANGUAGE_AQUINIS	13
#define LANGUAGE_TROLL		14
#define MAX_LANGUAGE		15

#define CLASS_MURDERER         0
#define CLASS_VANDILIER        1
#define CLASS_THIEF            2
#define CLASS_ASSASSIN         3
#define CLASS_HERETIC          4
#define CLASS_WARLOCK          5
#define CLASS_CHAOMANCER       6
#define CLASS_OCCULTIST        7
#define CLASS_CONMAN           8
#define CLASS_PAGAN            9
#define CLASS_TECHNOMANCER    10
/*
 * Second port classes(RoI tree) -Flux
 */ 
#define CLASS_WARRIOR			11
#define CLASS_ROGUE			12
#define CLASS_ADVENTURER		13
#define CLASS_CLERIC			14
#define CLASS_APPRENTICE		15
#define CLASS_NATURALIST		16
/* Second Tier classes */
#define CLASS_BRAWLER			17
#define CLASS_KNIGHT			18
#define CLASS_FIGHTER			19
#define CLASS_TERRORIST			20
#define CLASS_PICKPOCKET		21
#define CLASS_FOOTPAD			22
#define CLASS_HUNTER			23
#define CLASS_MINSTREL			24
#define CLASS_CRUSADER			25
#define CLASS_PRIEST			26
#define CLASS_MONK			27
#define CLASS_DARK_PRIEST		28
#define CLASS_SORCERER			29
#define CLASS_MAGE			30
#define CLASS_MAGICIAN			31
#define CLASS_HERBALIST			32
#define CLASS_DRUID			33
/* Third Tier classes */
#define CLASS_MARTIAL_ARTIST		34
#define CLASS_WRESTLER			35
#define CLASS_PALADIN			36
#define CLASS_BLACK_KNIGHT		37
#define CLASS_CHAMPION			38
#define CLASS_MERCENARY			39
#define CLASS_FREEDOM_FIGHTER		40
#define CLASS_CULTIST			41
#define CLASS_MASTER_THIEF		42
#define CLASS_NINJA			43
#define CLASS_RANGER			44
#define CLASS_SONG_WEAVER		45
#define CLASS_WHITE_ROBE		46
#define CLASS_NECROMANCER		47
#define CLASS_DEMONOLIGIST		48
#define CLASS_WIZARD			49
#define CLASS_WAR_WIZARD		50
#define CLASS_ELEMENTALIST		51
#define CLASS_ILLUSIONIST		52
#define CLASS_ALCHEMIST			53
#define CLASS_HIEROPHANT		54
#define CLASS_SYLVAN_CHAMPION		55

#define RACE_HUMAN		0
#define RACE_ELF			1
#define RACE_DWARF		2
#define RACE_QUICKSILVER	3
#define RACE_MAUDLIN		4
#define RACE_PIXIE		5
#define RACE_FELIXI		6
#define RACE_DRACONI		7
#define RACE_GREMLIN		8
#define RACE_CENTAUR		9
#define RACE_KENDER		10
#define RACE_MINOTAUR		11
#define RACE_DROW		12
#define RACE_AQUINIS		13
#define RACE_TROLL		14
#define RACE_AQUASAPIEN		15
#define RACE_AVIASAPIEN		16
#define RACE_CHRONOSAPIEN	17
#define RACE_RDRAGON		18
#define RACE_GDRAGON		19
#define RACE_DEMON		20
#define RACE_SIREN		21
#define RACE_ANGEL		22
#define RACE_GPALADIN		23
#define RACE_BEHOLDER		24

struct  material_type
{
    char	name[15];		/* Name of material */
    int	type;
    int	imm_damage[15];	/* Damage from magical elements */
};

/*
 * Per-class stuff.
 */
struct	class_type
{
    char 	who_name[ 4 ];		/* Three-letter name for 'who'	*/
    char	who_long[ 15 ]; 	/* Long name of Class           */
    int 	attr_prime;		/* Prime attribute		*/
    int 	skill_adept;		/* Maximum skill level		*/
    int  	hp_min;			/* Min hp gained on leveling	*/
    int		hp_max;			/* Max hp gained on leveling	*/
    bool	fightrank;		/* In a fight, is class the tank? */
    int		tree;			/* For second class set tree	*/
    bool	races[ MAX_RACE ];	/* Can a race be a class?	*/
    bool	multi[ MAX_CLASS ];	/* Which classes can multiclass */
};

struct  race_data
{
    RACE_DATA * next;
    int		vnum;
    char *     	race_name;
    char *     	race_full;
    bool     	polymorph;
    int		claws;
    int		flying;
    int		gills;
    int		acidblood;
    int		infrared;
    int		truesight;
    int		swimming;
    int		size;
    int		mstr;
    int		mint;
    int		mwis;
    int		mdex;
    int		mcon;
    int		mcha;
    int		magi;
    int         mimm[16];
    int	      language[MAX_LANGUAGE];
};

/*
 * Data structure for notes.
 */
struct	note_data
{
    NOTE_DATA *	next;
    char *		sender;
    char *		date;
    char *		to_list;
    char *		subject;
    char *		text;
    time_t		date_stamp;
    bool		protected;
    int			on_board;
};



/*
 * An affect.
 */
struct	affect_data
{
    AFFECT_DATA *	next;
    int 		type;
    int			level;
    int 		duration;
    int 		location;
    int 		modifier;
    int			bitvector;
    bool		deleted;
};



/*
 * A kill structure (indexed by level).
 */
struct	kill_data
{
    int	number;
    int	killed;
};

/* Bitvector values.. replaces the old A,B,C system. */
#define BV00    0x00000001
#define BV01    0x00000002
#define BV02    0x00000004
#define BV03    0x00000008
#define BV04    0x00000010
#define BV05    0x00000020
#define BV06    0x00000040
#define BV07    0x00000080
#define BV08    0x00000100
#define BV09    0x00000200
#define BV10    0x00000400
#define BV11    0x00000800
#define BV12    0x00001000
#define BV13    0x00002000
#define BV14    0x00004000
#define BV15    0x00008000
#define BV16    0x00010000
#define BV17    0x00020000
#define BV18    0x00040000
#define BV19    0x00080000
#define BV20    0x00100000
#define BV21    0x00200000
#define BV22    0x00400000
#define BV23    0x00800000
#define BV24    0x01000000
#define BV25    0x02000000
#define BV26    0x04000000
#define BV27    0x08000000
#define BV28    0x10000000
#define BV29    0x20000000
#define BV30    0x40000000
#define BV31    0x80000000

/* RT ASCII conversions -- used so we can have letters in this file */

#define A		  	1
#define B			2
#define C			4
#define D			8
#define E			16
#define F			32
#define G			64
#define H			128

#define I			256
#define J			512
#define K		      1024
#define L		 	2048
#define M			4096
#define N		 	8192
#define O			16384
#define P			32768

#define Q			65536
#define R			131072
#define S			262144
#define T			524288
#define U			1048576
#define V			2097152
#define W			4194304
#define X			8388608

#define Y			16777216
#define Z			33554432
#define aa			67108864 	/* doubled due to conflicts */
#define bb			134217728
#define cc			268435456    
#define dd			536870912
#define ee			1073741824

/***************************************************************************
 *                                                                         *
 *                   VALUES OF INTEREST TO AREA BUILDERS                   *
 *                   (Start of section ... start here)                     *
 *                                                                         *
 ***************************************************************************/

/*
 * Well known mob virtual numbers.
 * Defined in #MOBILES.
 */
#define MOB_VNUM_CITYGUARD	     127
#define MOB_VNUM_SKELETON	     5
#define MOB_VNUM_LICH		     8
#define MOB_VNUM_PHARAOH	     9
#define MOB_VNUM_MUMMY		     6
#define MOB_VNUM_BLOODBORN	    10
#define MOB_VNUM_DEMON1 	     4
#define MOB_VNUM_DEMON2            4
#define MOB_VNUM_SUPERMOB          7
#define MOB_VNUM_ULT               3160

#define MOB_VNUM_AIR_ELEMENTAL     8914
#define MOB_VNUM_EARTH_ELEMENTAL   8915
#define MOB_VNUM_WATER_ELEMENTAL   8916
#define MOB_VNUM_FIRE_ELEMENTAL    8917
#define MOB_VNUM_DUST_ELEMENTAL    8918

/* XOR */
#define MOB_VNUM_DEMON		   80
#define MOB_VNUM_INSECTS	   81
#define MOB_VNUM_WOLFS		   82
#define MOB_VNUM_COPY            88

/*ELVIS*/
#define MOB_VNUM_ANGEL             83
#define MOB_VNUM_SHADOW            84
#define MOB_VNUM_BEAST             85
#define MOB_VNUM_TRENT             86

/* CLANS */
#define CLAN_PKILL		BV00
#define CLAN_CIVIL_PKILL	BV01
#define CLAN_CHAMP_INDUCT	BV02
#define CLAN_LEADER_INDUCT	BV03
#define CLAN_FIRST_INDUCT	BV04
#define CLAN_SECOND_INDUCT	BV05

/*
 * ACT bits for mobs.
 * Used in #MOBILES.
 */
#define ACT_IS_NPC		BV00		/* Auto set for mobs */
#define ACT_SENTINEL		BV01		/* Stays in one room	*/
#define ACT_SCAVENGER		BV02		/* Picks up objects	*/
#define ACT_ILLUSION		BV03		/* Is an illusion	*/
#define ACT_AGGRESSIVE		BV05		/* Attacks PC's		*/
#define ACT_STAY_AREA		BV06		/* Won't leave area	*/
#define ACT_WIMPY		BV07		/* Flees when hurt	*/
#define ACT_PET			BV08		/* Auto set for pets	*/
#define ACT_TRAIN		BV09		/* Can train PC's	*/
#define ACT_PRACTICE		BV10		/* Can practice PC's	*/
#define ACT_GAMBLE		BV11            /* Runs a gambling game */
#define ACT_PROTOTYPE		BV12            /* Prototype flag       */
#define ACT_UNDEAD		BV13            /* Can be turned        */
#define ACT_TRACK		BV14            /* Track players        */
#define ACT_FREEFLAG		BV15
#define ACT_POSTMAN		BV16		/* Hello Mr. Postman!   */
#define ACT_NODRAG		BV17		/* No drag mob		*/
#define ACT_NOPUSH		BV18		/* No push mob		*/
#define ACT_NOSHADOW		BV19		/* No shadow mob	*/
#define ACT_NOASTRAL		BV20		/* No astral mob	*/

#define DAMNOUN_STRIKE		901
#define DAMNOUN_SLASH		902
#define DAMNOUN_PIERCE		903
#define DAMNOUN_BASH		904
#define DAMNOUN_TEAR		905
#define DAMNOUN_CHOP		906
#define DAMNOUN_CLAW		907
#define DAMNOUN_SLAP		908
#define DAMNOUN_BITE		909
#define DAMNOUN_SMASH		910
#define DAMNOUN_CRUSH		911
#define DAMNOUN_OSMASH		912

/* These are for generic elemental damage -Flux */
#define DAMNOUN_HEAT		913
#define DAMNOUN_POSITIVE	914
#define DAMNOUN_COLD		915
#define DAMNOUN_NEGATIVE	916
#define DAMNOUN_HOLY		917
#define DAMNOUN_UNHOLY		918
#define DAMNOUN_REGEN		919
#define DAMNOUN_DEGEN		920
#define DAMNOUN_DYNAMIC		921
#define DAMNOUN_VOID		922

/* Furniture types, -Flux */
#define FURNITURE_MISC		0
#define FURNITURE_CHAIR		1
#define FURNITURE_SOFA		2
#define FURNITURE_BED		3
#define FURNITURE_DESK		4
#define FURNITURE_ARMOIR	5

/* These make up the weapon class, the damage noun is set by the builder */
#define WEAPON_SLASH		0
#define WEAPON_PIERCE		1
#define WEAPON_BASH		2
#define WEAPON_TEAR		3
#define WEAPON_CHOP		4
#define WEAPON_EXOTIC		5
#define WEAPON_BLADE		6

/* all of this is for quicksilver -Flux */
#define MORPH_WEAPON		0
#define MORPH_ARMOR		1
#define MORPH_AVOIDANCE		2
#define MORPH_SIZE		3
#define MORPH_CAMO		4

#define MORPH_WEAPON_CLAW	1
#define MORPH_WEAPON_SLASH      2
#define MORPH_WEAPON_BASH       3
#define MORPH_WEAPON_CHOP       4
#define MORPH_WEAPON_PIERCE     5

/* For the Can_see_thing function in handler.c --Flux. */
#define THING_HIDDEN	0
#define THING_INVIS	1

/*
 * Bits for 'affected_by'.
 * Used in #MOBILES.
 */
#define AFF_BLIND		BV00
#define AFF_INVISIBLE		BV01	      
#define AFF_TEMPORAL		BV02
#define AFF_DETECT_INVIS	BV03
#define AFF_BREATHE_WATER	BV04    
#define AFF_DETECT_HIDDEN	BV05
#define AFF_HASTE		BV06
#define AFF_CUREAURA		BV07
#define AFF_FAERIE_FIRE		BV08
#define AFF_INFRARED		BV09
#define AFF_CURSE		BV10
#define AFF_FLAMING             BV11
#define AFF_POISON		BV12
#define AFF_INSANE		BV13
#define AFF_VIBRATING		BV14
#define AFF_SNEAK		BV15
#define AFF_HIDE		BV16
#define AFF_SLEEP		BV17
#define AFF_CHARM		BV18
#define AFF_FLYING		BV19
#define AFF_PASS_DOOR		BV20
#define AFF_SIAMESE             BV21
#define AFF_SUMMONED            BV22
#define AFF_MUTE                BV23
#define AFF_PEACE              	BV24
#define AFF_FIRESHIELD         	BV25
#define AFF_SHOCKSHIELD        	BV26
#define AFF_ICESHIELD         	BV27
#define AFF_CHAOS             	BV28
#define AFF_SCRY              	BV29
#define AFF_ANTI_FLEE        	BV30
#define AFF_DISJUNCTION      	BV31

#define AFF_POLYMORPH	         1
#define AFF_CLAP	 	 2
#define AFF_NOASTRAL             4
#define AFF_TORTURED             8 
#define AFF_TRUESIGHT           16
#define AFF_BLADE               32
#define AFF_GRASPING            64 
#define AFF_GRASPED            128 
#define AFF_BERSERK            256
#define AFF_TORTURING          512
#define AFF_CONFUSED          1024
#define AFF_FUMBLE            2048
#define AFF_HALLUCINATING     4096
#define AFF_DISEASED          8192
#define AFF_PHASED           16384
#define AFF_FREE_FLAG	     32768
#define AFF_RAGE	     65536
#define AFF_RUSH	    131072
#define AFF_INERTIAL	    262144
#define AFF_VAMPIRIC	    524288
#define AFF_BLINK          1048576
#define AFF_THORNY         2097152
#define AFF_MIRROR_IMAGES  4194304
#define AFF_PMIRROR        8388608
#define AFF_MMIRROR       16777216 
#define AFF_ACIDBLOOD     33554432 
#define AFF_PLASMA        67108864 
#define AFF_MANANET      134217728 
#define AFF_MANASHIELD   268435456
#define AFF_CHLOROFORM	 536870912

#define AFF_PLOADED     1073741824

/* damage classes */
#define DAM_NONE                -1
#define DAM_HEAT                0
#define DAM_POSITIVE            1
#define DAM_COLD                2
#define DAM_NEGATIVE            3
#define DAM_HOLY                4
#define DAM_UNHOLY              5
#define DAM_REGEN               6
#define DAM_DEGEN               7
#define DAM_DYNAMIC             8
#define DAM_VOID                9
#define DAM_PIERCE              10
#define DAM_SLASH               11
#define DAM_SCRATCH             12
#define DAM_BASH                13
#define DAM_INTERNAL            14

/* Tattoo magic booster */
#define MAGBOO_NONE                -1
#define MAGBOO_HEAT                0
#define MAGBOO_POSITIVE            1
#define MAGBOO_COLD                2
#define MAGBOO_NEGATIVE            3
#define MAGBOO_HOLY                4
#define MAGBOO_UNHOLY              5
#define MAGBOO_REGEN               6
#define MAGBOO_DEGEN               7
#define MAGBOO_DYNAMIC             8
#define MAGBOO_VOID                9

/* IMM bits for mobs */
#define IMM_HEAT	(A)
#define IMM_POSITIVE	(B)
#define IMM_COLD	(C)
#define IMM_NEGATIVE	(D)
#define IMM_HOLY	(E)
#define IMM_UNHOLY	(F)
#define IMM_REGEN	(G)
#define IMM_DEGEN	(H)
#define IMM_DYNAMIC	(I)
#define IMM_VOID	(J)
#define IMM_PIERCE	(K)
#define IMM_SLASH	(L)
#define IMM_SCRATCH	(M)
#define IMM_BASH	(N)
#define IMM_INTERNAL	(O)
 
/* gate flags */
#define GATE_NORMAL_EXIT	(A)
#define GATE_NOCURSE		(B)
#define GATE_GOWITH		(C)
#define GATE_BUGGY		(D)
#define GATE_RANDOM		(E)

#define EXIT_STATUS_OPEN	0
#define EXIT_STATUS_DOOR	1
#define EXIT_STATUS_PHYSICAL	2
#define EXIT_STATUS_MAGICAL	3
#define EXIT_STATUS_SNOWIN	4

/*
 * Types of attacks.
 * Must be non-overlapping with spell/skill types,
 * but may be arbitrary beyond that.
 */
#define TYPE_UNDEFINED               -1
#define TYPE_HIT                     1000

/* For the switches coding, Flux. */
#define SWITCH_NONE			0
#define SWITCH_DOOR			1
#define SWITCH_OLOAD			2
#define SWITCH_MLOAD			3

/* More Switch coding stuff, -Flux. */
#define DOOR_ACTION_NONE		0
#define DOOR_ACTION_OPEN		1
#define DOOR_ACTION_CLOSE		2
#define DOOR_ACTION_LOCK		3
#define DOOR_ACTION_UNLOCK		4

/* More Switch coding stuff, -Flux. */
#define ACTIVATION_NONE			0
#define ACTIVATION_PULL			1
#define ACTIVATION_PRESS		2
#define ACTIVATION_LIFT			3
#define ACTIVATION_LOWER		4
#define ACTIVATION_RAISE		5

/*
 * Sex.
 * Used in #MOBILES.
 */
#define SEX_NEUTRAL		      0
#define SEX_MALE		      	1
#define SEX_FEMALE		      2



/*
 * Well known object virtual numbers.
 * Defined in #OBJECTS.
 */
#define OBJ_VNUM_MONEY_ONE	      2
#define OBJ_VNUM_MONEY_SOME	      3
#define OBJ_VNUM_PILE_JUNK	      4
#define OBJ_VNUM_BLOOD		      9
#define OBJ_VNUM_BONE		      7
#define OBJ_VNUM_CORPSE_NPC	     10
#define OBJ_VNUM_RIGOR	             26
#define OBJ_VNUM_SKELETON	     25
#define OBJ_VNUM_CORPSE_PC	     11
#define OBJ_VNUM_SEVERED_HEAD	     12
#define OBJ_VNUM_TORN_HEART	     13
#define OBJ_VNUM_SLICED_RIGHT_ARM    14
#define OBJ_VNUM_SLICED_RIGHT_LEG    15
#define OBJ_VNUM_SLICED_LEFT_ARM     30
#define OBJ_VNUM_SLICED_LEFT_LEG     31
#define OBJ_VNUM_FINAL_TURD	     16
#define OBJ_VNUM_PORTAL              17
#define OBJ_VNUM_DOLL                18
#define OBJ_VNUM_BERRY               19
#define OBJ_VNUM_LOBSTER_CLAW	     33
#define OBJ_VNUM_TAIL		     34
#define OBJ_VNUM_WING		     35
#define OBJ_VNUM_NOHAIR_HEAD	     36
#define OBJ_VNUM_BIRD_CLAW	     37
#define OBJ_VNUM_INSECT_ABDOMEN	     38
#define OBJ_VNUM_THIN_LEG	     39
#define OBJ_VNUM_NEWWEAPON           40
#define OBJ_VNUM_NEWARMOR            41
#define OBJ_VNUM_NEWMEDAL            42
#define OBJ_VNUM_THORNROD	     43
#define OBJ_VNUM_BLAZESWORD	     44
#define OBJ_VNUM_BULLET_SHELL	     45
#define OBJ_VNUM_SNAKE_SKIN	     46
#define OBJ_VNUM_SPIDER_WEB	     47
#define OBJ_VNUM_SPHERE_OF_SOLITUDE  48
#define OBJ_VNUM_INSECT_WINGS	     49
#define OBJ_VNUM_GAS_CLOUD	     50
#define OBJ_VNUM_ASTRAL_PORTAL	     51
#define OBJ_VNUM_BLACK_STAR	     52


#define OBJ_VNUM_MUSHROOM	     20
#define OBJ_VNUM_LIGHT_BALL	     21
#define OBJ_VNUM_SPRING		     22
#define OBJ_VNUM_SOULGEM	     23
#define OBJ_VNUM_TO_FORGE_A	  25065 /* armor */
#define OBJ_VNUM_TO_FORGE_W	  25066 /* weapon */
#define OBJ_VNUM_WARD_PHYS	  25067

/*
 * Item types.
 * Used in #OBJECTS.
 */
#define ITEM_SCROLL			1
#define ITEM_WAND			2
#define ITEM_STAFF			3
#define ITEM_WEAPON		      	4
#define ITEM_TREASURE		      	5
#define ITEM_ARMOR		      	6
#define ITEM_POTION		      	7
#define ITEM_NOTEBOARD			8
#define ITEM_FURNITURE			9
#define ITEM_TRASH			10
#define ITEM_CONTAINER			11
#define ITEM_DRINK_CON			12
#define ITEM_MISC			13
#define ITEM_FOOD			14
#define ITEM_MONEY			15
#define ITEM_BOAT			16
#define ITEM_CORPSE_NPC			17
#define ITEM_RIGOR			18
#define ITEM_SKELETON			19
#define ITEM_CORPSE_PC			20
#define ITEM_FOUNTAIN			21
#define ITEM_PILL			22
#define ITEM_LENSE			23
#define ITEM_BLOOD			24
#define ITEM_PORTAL			25
#define ITEM_VODOO			26
#define ITEM_BERRY			27
#define ITEM_BOMB			28
#define ITEM_ORE			29
#define ITEM_BODY_PART			30
#define ITEM_SWITCH			31
#define ITEM_SCROLLPAPER		32
#define ITEM_PEN			33
#define ITEM_BEAKER			34
#define ITEM_CHEMSET			35
#define ITEM_BUNSEN			36
#define ITEM_INKCART			37
#define ITEM_POSTALPAPER		38
#define ITEM_LETTER			39
#define ITEM_SMOKEBOMB			40
#define ITEM_SMITHYHAMMER		41
#define ITEM_POISONCHEM			42
#define ITEM_MOLOTOVCHEM		43
#define ITEM_WICK			44
#define ITEM_TEARCHEM			45
#define ITEM_PIPEBOMBCHEM		46
#define ITEM_CHEMBOMBCHEM		47
#define ITEM_TIMER			49
#define ITEM_PILEWIRE			50
#define ITEM_TRIPWIRE			51
#define ITEM_TOOLPACK			52
#define ITEM_CHERRYCONTAINER		53
#define ITEM_GUNPOWDER			54
#define ITEM_SMITHYPACK			55
#define ITEM_SMITHYANVIL		56
#define ITEM_TECHNOSOTTER		57
#define ITEM_TECHNOWORKSTATION		58
#define ITEM_RANGED_WEAPON		59
#define ITEM_BULLET			60
#define ITEM_CLIP			61
#define ITEM_GAS_CLOUD			62
#define ITEM_MUMMY			63
#define ITEM_EMBALMING_FLUID		64
#define ITEM_ARROW			65
//#define ITEM_POTION_BAG		66

#define RANGED_WEAPON_BOW		0
#define RANGED_WEAPON_FIREARM		1
#define RANGED_WEAPON_BAZOOKA		2

#define ARROW_STANDARD			0
#define ARROW_BARBED			1

#define GUN_NORMAL			0
#define GUN_SEMIAUTOMATIC		1
#define GUN_AUTOMATIC			2

#define BOMB_EXPLOSIVE			0
#define BOMB_NAPALM			1
#define BOMB_PLASMA			2
#define BOMB_TEAR		     	3
#define BOMB_CHEMICAL			4
#define BOMB_NUKE		     	5
#define BOMB_CHERRY			6

#define CASINO_NONE			0
#define CASINO_CRAPS			1
#define CASINO_POKER			2
#define CASINO_BLACKJACK		3
#define CASINO_ROULETTE			4
#define CASINO_SIMPLE_DICE		5
#define CASINO_THREE_CARD_MONTY		6

#define PROP_NONE			0
#define PROP_LOCAL		     	1
#define PROP_AREA			2
#define PROP_GLOBAL		     	3
#define PROP_TEMPORAL		     	4
#define PROP_DIMENSIONAL	     	5

/*
 * Extra flags.
 * Used in #OBJECTS.
 */
#define ITEM_GLOW		1		/*	1	*/
#define ITEM_HUM		2
#define ITEM_DARK		4
#define ITEM_LOCK		8
#define ITEM_SHARP		16
#define ITEM_INVIS		32
#define ITEM_MAGIC		64
#define ITEM_NODROP		128
#define ITEM_BLESS		256
#define ITEM_FREE_FLAG6		512
#define ITEM_FREE_FLAG		1024
#define ITEM_FREE_FLAG5	    	2048
#define ITEM_NOREMOVE	 	4096
#define ITEM_INVENTORY	   	8192
#define ITEM_POISONED		16384
#define ITEM_DWARVEN		32768
#define ITEM_WIRED		65536
#define ITEM_SOUL_BOUND		131072
#define ITEM_TECHNOLOGY		262144
#define ITEM_BALANCED		524288		/*	20	*/
#define ITEM_NOEXIT		2097152
#define ITEM_FREE_FLAG2		4194304
#define ITEM_SHOCK		8388608
#define ITEM_RAINBOW          	16777216
#define ITEM_FLAME            	33554432
#define ITEM_CHAOS            	67108864
#define ITEM_NO_LOCATE		(cc)
#define ITEM_NO_DAMAGE        	(dd)
#define ITEM_FREE_FLAG_4	(ee)
#define ITEM_ICY              	(bb)
#define ITEM_FREE_FLAG3		(aa)

/*
 * Race_type determines what the mob is, why did i put this here?
 * Cause it was the first thing my find function hit ;) -Flux
 */
#define RACE_TYPE_HUMANOID	0
#define RACE_TYPE_LOBSTER	1
#define RACE_TYPE_SNAKE		2
#define RACE_TYPE_BIRD		3
#define RACE_TYPE_DOG		4
#define RACE_TYPE_SPIDER	5
#define RACE_TYPE_FLYING_INSECT	6

/*
 * Wear flags.
 * Used in #OBJECTS.
 */
#define ITEM_TAKE		      	1
#define ITEM_WEAR_FINGER		2
#define ITEM_WEAR_NECK		      	4
#define ITEM_WEAR_BODY			8
#define ITEM_WEAR_HEAD		     	16
#define ITEM_WEAR_LEGS		     	32
#define ITEM_WEAR_FEET		     	64
#define ITEM_WEAR_HANDS		    	128 
#define ITEM_WEAR_ARMS		    	256
#define ITEM_WEAR_SHIELD	    	512
#define ITEM_WEAR_ABOUT		   	1024 
#define ITEM_WEAR_WAIST		   	2048
#define ITEM_WEAR_WRIST			4096
#define ITEM_WIELD			8192
#define ITEM_WEAR_ORBIT           	32768
#define ITEM_WEAR_FACE            	65536
#define ITEM_WEAR_CONTACT        	131072
/* For double equipment, like worn on both wrists(IE: shackles) - Flux. */
#define ITEM_DOUBLE           		262144
#define ITEM_WEAR_EARS           	524288
#define ITEM_WEAR_ANKLE         	1048576
#define ITEM_MEDALLION          	2097152
#define ITEM_WEAR_SHEATH		8388608


/*
 * for bionic_flags
 */
#define ITEM_BIONIC_EYE               1
#define ITEM_BIONIC_BODY	      2
#define ITEM_BIONIC_ARM		      4
#define ITEM_BIONIC_HAND	      8
#define ITEM_BIONIC_LEG		     16
#define ITEM_BIONIC_IMPLANT	     32
#define ITEM_BIONIC_MEMORY	     64

#define BIONIC_NONE		-1
#define BIONIC_EYE_L		 0
#define BIONIC_EYE_R		 1
#define BIONIC_BODY		 2
#define BIONIC_ARM_L		 3
#define BIONIC_ARM_R		 4
#define BIONIC_HAND_L		 5
#define BIONIC_HAND_R		 6
#define BIONIC_LEG_L		 7
#define BIONIC_LEG_R		 8
#define BIONIC_IMPLANT1		 9
#define BIONIC_IMPLANT2		10
#define BIONIC_IMPLANT3		11
#define BIONIC_IMPLANT4		12
#define BIONIC_IMPLANT5		13
#define BIONIC_IMPLANT6		14
#define BIONIC_IMPLANT7		15
#define BIONIC_IMPLANT8		16
#define BIONIC_IMPLANT9		17
#define BIONIC_IMPLANT10	18
#define BIONIC_MEMORY1		19
#define BIONIC_MEMORY2		20
#define BIONIC_MEMORY3		21
#define BIONIC_MEMORY4		22
#define MAX_BIONIC		23

/*
 * Equpiment wear locations.
 * Used in #RESETS.
 */
#define WEAR_NONE			1
#define WEAR_FINGER_L		      	2
#define WEAR_FINGER_R		      	4
#define WEAR_NECK			8
#define WEAR_BODY_1			16
#define WEAR_BODY_2		    	32
#define WEAR_BODY_3		    	64 
#define WEAR_HEAD		    	128
#define WEAR_IN_EYES		    	256
#define WEAR_ON_FACE		    	512
#define WEAR_ORBIT                  	1024
#define WEAR_LEGS		    	2048
#define WEAR_FEET		    	4096
#define WEAR_HANDS		    	8192
#define WEAR_ARMS		    	16384
#define WEAR_SHIELD		    	32768
#define WEAR_ABOUT		    	65536
#define WEAR_WAIST		   	131072
#define WEAR_WRIST_L		   	262144
#define WEAR_WRIST_R		   	524288
#define WEAR_WIELD		    	1048576
#define WEAR_WIELD_2		    	2097152
#define WEAR_EAR_R		    	4194304
#define WEAR_EAR_L                  	8388608
#define WEAR_ANKLE_L		     	16777216
#define WEAR_ANKLE_R                	33554432
#define WEAR_MEDALLION                	67108864
#define WEAR_SHEATH_1              	134217728
#define WEAR_SHEATH_2		     	268435456
#define MAX_WEAR		     	536870912
/* Added this due to conflicts with the new bit structure, it represents
   the maximum amount of pieces of eq one can wear - Flux. */
#define MAX_EQUIP			29

/* Locations for tatoo stuff */
#define TATTOO_NONE		     -1
#define TATTOO_FACE		      0
#define TATTOO_BACK_NECK	      1
#define TATTOO_FRONT_NECK	      2
#define TATTOO_LEFT_SH		      3
#define TATTOO_RIGHT_SH		      4
#define TATTOO_LEFT_ARM		      5
#define TATTOO_RIGHT_ARM	      6
#define TATTOO_LEFT_HAND	      7
#define TATTOO_RIGHT_HAND	      8
#define TATTOO_CHEST		      9
#define TATTOO_BACK		     10
#define TATTOO_LEFT_LEG		     11
#define TATTOO_RIGHT_LEG	     12
#define TATTOO_LEFT_ANKLE	     13
#define TATTOO_RIGHT_ANKLE	     14
#define TATTOO_FOREHEAD		     15
#define MAX_TATTOO		     16

/* materials for objects, Swift. */
#define MATERIAL_LEATHER		0
#define MATERIAL_STONE			1
#define MATERIAL_WOOD			2
#define MATERIAL_GOLD			3
#define MATERIAL_SILVER			4
#define MATERIAL_COPPER			5
#define MATERIAL_PLASTIC		6
#define MATERIAL_GLASS			7
#define MATERIAL_IRON			8
#define MATERIAL_LEAD			9
#define MATERIAL_FIRE			10
#define MATERIAL_ICE			11
#define MATERIAL_ENERGY			12
#define MATERIAL_CRYSTAL		13
#define MATERIAL_BONE			14
#define MATERIAL_STEEL			15
#define MATERIAL_BRONZE			16
#define MATERIAL_BRASS			17
#define MATERIAL_OMNI_CRYSTAL		18
#define MATERIAL_PLASMA			19
#define MATERIAL_OMNI_STEEL		20
#define MATERIAL_LIVING_METAL		21
#define MATERIAL_CLOTH			22
#define MATERIAL_KEVLAR			23
#define MAX_MATERIAL			24

/* Material-types -Flux */
#define MATERIAL_TYPE_NONE		-1
#define MATERIAL_TYPE_CLOTH		0
#define MATERIAL_TYPE_NATURAL		1
#define MATERIAL_TYPE_UNNATURAL		2
#define MATERIAL_TYPE_METAL		3
#define MATERIAL_TYPE_MYSTIC		4


/*
 * Apply types (for affects).
 * Used in #OBJECTS.
 */
#define APPLY_NONE		    	0
#define APPLY_STR		      	1
#define APPLY_DEX		      	2
#define APPLY_INT		      	3
#define APPLY_WIS		      	4
#define APPLY_CON		      	5
#define APPLY_AGI                   	6
#define APPLY_MDAMP		      	7
#define APPLY_PDAMP		      	8
#define APPLY_HITROLL		     	9
#define APPLY_DAMROLL		     	10
#define APPLY_HIT		     	11
#define APPLY_MANA		     	12
#define APPLY_MOVE		     	13
#define APPLY_SIZE	     		14
#define APPLY_SEX			15
#define APPLY_AGE		    	16
#define APPLY_RACE                  	17
#define APPLY_IMM_HEAT              	18
#define APPLY_IMM_POSITIVE          	19
#define APPLY_IMM_COLD              	20
#define APPLY_IMM_NEGATIVE          	21
#define APPLY_IMM_HOLY              	22
#define APPLY_IMM_UNHOLY            	23
#define APPLY_IMM_REGEN             	24
#define APPLY_IMM_DEGEN             	25
#define APPLY_IMM_DYNAMIC           	26
#define APPLY_IMM_VOID              	27
#define APPLY_IMM_PIERCE            	28
#define APPLY_IMM_SLASH             	29
#define APPLY_IMM_SCRATCH           	30
#define APPLY_IMM_BASH              	31
#define APPLY_IMM_INTERNAL          	32
#define APPLY_CHA                   	33

#define APPLY_THROWPLUS          	34

/* X */
#define PERM_SPELL_BEGIN		100
#define APPLY_INVISIBLE			(PERM_SPELL_BEGIN + 0)
#define APPLY_TEMPORAL_FIELD		(PERM_SPELL_BEGIN + 1)
#define APPLY_DETECT_INVIS		(PERM_SPELL_BEGIN + 2)
#define APPLY_DETECT_HIDDEN		(PERM_SPELL_BEGIN + 4)

#define APPLY_INFRARED			(PERM_SPELL_BEGIN + 6)
#define APPLY_SNEAK			(PERM_SPELL_BEGIN + 8)
#define APPLY_HIDE			(PERM_SPELL_BEGIN + 9)
#define APPLY_FLYING			(PERM_SPELL_BEGIN + 10)

#define APPLY_PASS_DOOR			(PERM_SPELL_BEGIN + 11)

#define APPLY_FIRESHIELD        	(PERM_SPELL_BEGIN + 13)
#define APPLY_SHOCKSHIELD       	(PERM_SPELL_BEGIN + 14)
#define APPLY_ICESHIELD         	(PERM_SPELL_BEGIN + 15)
#define APPLY_CHAOS             	(PERM_SPELL_BEGIN + 16)

#define APPLY_SCRY              	(PERM_SPELL_BEGIN + 17)
#define APPLY_BREATHE_WATER		(PERM_SPELL_BEGIN + 18)
#define APPLY_POISON			(PERM_SPELL_BEGIN + 20)
#define APPLY_HEIGHTEN_SENSES   	(PERM_SPELL_BEGIN + 24)
#define APPLY_THORNY            	(PERM_SPELL_BEGIN + 26)
/* END */

/*
 * Values for containers (value[1]).
 * Used in #OBJECTS.
 */
#define CONT_CLOSEABLE		      1
#define CONT_PICKPROOF		      2
#define CONT_CLOSED		      4
#define CONT_LOCKED		      8



/*
 * Well known room virtual numbers.
 * Defined in #ROOMS.
 */
#define ROOM_VNUM_RJAIL			2
#define ROOM_VNUM_LIMBO			2
#define ROOM_ELF_SCHOOL			2
#define ROOM_HUMAN_SCHOOL		2
#define ROOM_HUMAN_NEWBIE_MORGUE	4364
#define ROOM_ELF_NEWBIE_MORGUE		4364
#define ROOM_HUMAN_TEMPLE		25000
#define ROOM_ELF_TEMPLE			1308
#define ROOM_HUMAN_MORGUE		25001
#define ROOM_ELF_MORGUE			1328
#define ROOM_ELF_DONATION		1329
#define ROOM_HUMAN_DONATION		25005
#define ROOM_ARENA_VNUM			1101
#define ROOM_ARENA_ENTER_F		1101
#define ROOM_ARENA_ENTER_S		1101
#define ROOM_ARENA_HALL_SHAME		1102
#define ROOM_VNUM_ASTRAL		900
#define ROOM_VNUM_SHADOW		800
#define ROOM_VNUM_SHADOW_PATTERN_CENTER	831

/*
 * Room flags.
 * Used in #ROOMS.
 */
#define ROOM_NONE		      0
#define ROOM_DARK		      1
#define ROOM_TRAPPED                  2
#define ROOM_NO_MOB		      4
#define ROOM_INDOORS		      8
#define ROOM_NO_SHADOW	             16
#define ROOM_UNDERGROUND             32
#define ROOM_BANK_VAULT	             64
#define ROOM_FREE_FLAG1             128
#define ROOM_FREE_FLAG2             256
#define ROOM_PRIVATE		    512
#define ROOM_SAFE		   1024
#define ROOM_SOLITARY		   2048
#define ROOM_PET_SHOP		   4096
#define ROOM_NO_RECALL		   8192
#define ROOM_CONE_OF_SILENCE      16384
#define ROOM_NO_MAGIC          	32768
#define ROOM_NO_PKILL          	65536
#define ROOM_NO_ASTRAL_IN      	131072
#define ROOM_NO_ASTRAL_OUT     	262144
#define ROOM_TELEPORT_AREA     	524288
#define ROOM_TELEPORT_WORLD     1048576
#define ROOM_NO_OFFENSIVE       2097152 
#define ROOM_NO_FLEE            4194304
#define ROOM_SILENT            	8388608
#define ROOM_BANK		16777216
#define ROOM_NOFLOOR            33554432
#define ROOM_SMITHY             67108864
#define ROOM_NOSCRY	      	134217728
#define ROOM_VOID             	268435456
#define ROOM_PKILL            	536870912
#define ROOM_MARK             	1073741824

/*
 * Directions.
 * Used in #ROOMS.
 */
#define DIR_MAX			      5

#define DIR_NORTH			0
#define DIR_EAST			1
#define DIR_SOUTH			2
#define DIR_WEST			3
#define DIR_UP				4
#define DIR_DOWN			5

/* Used for stance/attitude coding */

#define ATTITUDE_NORMAL		0
#define ATTITUDE_OFFENSIVE	10
#define ATTITUDE_SNAKE		11
#define ATTITUDE_DRAGON		12
#define ATTITUDE_TIGER		13
#define ATTITUDE_BULL		14
#define ATTITUDE_DEFENSIVE	-10
#define ATTITUDE_PANTHER	-11
#define ATTITUDE_CRANE		-12
#define ATTITUDE_SPARROW	-13
#define ATTITUDE_MONKEY		-14
/*This is for bullet dodging -Flux */
#define ATTITUDE_PROTECTIVE	1000

/*
 * Exit flags.
 * Used in #ROOMS.
 */
#define EX_ISDOOR		      1
#define EX_CLOSED		      2
#define EX_LOCKED		      4
#define EX_BASHED                     8
#define EX_BASHPROOF                 16
#define EX_PICKPROOF		     32
#define EX_PASSPROOF                 64
#define EX_RANDOM                   128
#define EX_MAGICLOCK		    256
#define EX_SECRET		    512

/*
 * Sector types.
 * Used in #ROOMS.
 */
#define SECT_INSIDE			0
#define SECT_PAVED			1
#define SECT_FIELD			2
#define SECT_FOREST			3
#define SECT_HILLS			4
#define SECT_MOUNTAIN			5
#define SECT_WATER_SURFACE		6
#define SECT_UNDERWATER			7
#define SECT_DESERT			8
#define SECT_BADLAND			9
#define SECT_SWAMP                 	10
#define SECT_CAVERN                	11
#define SECT_AIR			12
#define SECT_HEAVEN		     	13
#define SECT_HELL		     	14
#define SECT_SHADOW			15
#define SECT_ARCTIC			16
#define SECT_ASTRAL			17
#define SECT_MAX		     	18


/***************************************************************************
 *                                                                         *
 *                   VALUES OF INTEREST TO AREA BUILDERS                   *
 *                   (End of this section ... stop here)                   *
 *                                                                         *
 ***************************************************************************/

/*
 * Conditions.
 */
#define COND_DRUNK		      0
#define COND_FULL		      1
#define COND_THIRST		      2
#define COND_INSOMNIA		      3
/*
 *  Maxes for conditions.
 */
#define MAX_FULL		     96
#define MAX_THIRST		     96
#define MAX_DRUNK		     24
#define MAX_INSOMNIA		     96

/*
 * Positions.
 */
#define POS_DEAD		      0
#define POS_MORTAL		      1
#define POS_INCAP		      2
#define POS_STUNNED		      4
#define POS_SLEEPING		      3
#define POS_RESTING		      5
#define POS_FIGHTING		      6
#define POS_STANDING		      7
#define POS_MEDITATING                8


/*
 * ACT bits for players.
 */
#define PLR_IS_NPC		BV00		/* Don't EVER set.	*/
#define PLR_BOUGHT_PET		BV01
#define PLR_AFK			BV02    
#define PLR_AUTOEXIT		BV03
#define PLR_AUTOLOOT		BV04
#define PLR_AUTOSAC		BV05
#define PLR_ILLUSION		BV06
#define PLR_BRIEF		BV07
#define PLR_FULLNAME		BV08
#define PLR_COMBINE		BV09
#define PLR_PROMPT		BV10
#define PLR_TELNET_GA		BV11
#define PLR_HOLYLIGHT		BV12
#define PLR_WIZINVIS		BV13
#define PLR_FREEFLAG		BV14	
#define PLR_SILENCE		BV15
#define PLR_NO_EMOTE		BV16
#define PLR_CLOAKED		BV17
#define PLR_NO_TELL		BV18
#define PLR_LOG			BV19
#define PLR_DENY		BV20
#define PLR_FREEZE		BV21
#define PLR_AUTOASSIST		BV22
#define PLR_KILLER		BV23
#define PLR_ANSI		BV24
#define PLR_AUTOCOINS		BV25
#define PLR_AUTOSPLIT		BV26
#define PLR_UNDEAD		BV27
#define PLR_FREEFLAG2		BV28
#define PLR_COMBAT		BV29
#define PLR_OUTCAST		BV30
#define PLR_CONSENT		BV31

#define STUN_TOTAL            0   /* Commands and combat halted. Normal stun */
#define STUN_COMMAND          1   /* Commands halted. Combat goes through */
#define STUN_MAGIC            2   /* Can't cast spells */
#define STUN_NON_MAGIC        3   /* No weapon attacks */
#define STUN_TO_STUN          4   /* Requested. Stop continuous stunning */

/*
 * Channel bits.
 */
#define CHANNEL_NONE                  0
#define	CHANNEL_AUCTION		      1
#define	CHANNEL_CHAT		      2
#define CHANNEL_OOC		      4
#define	CHANNEL_IMMTALK	              8
#define	CHANNEL_MUSIC		     16
#define	CHANNEL_QUESTION	     32
#define	CHANNEL_SHOUT		     64
#define	CHANNEL_YELL		    128
#define CHANNEL_CLAN                256
#define CHANNEL_CLASS               512
#define CHANNEL_HERO               1024

/*
 * Log Channels
 * Added by Altrag.
 */
#define CHANNEL_LOG		   2048 
#define CHANNEL_BUILD		   4096 
#define CHANNEL_GOD                8192 
#define CHANNEL_GUARDIAN          16384  
#define CHANNEL_CODER             65536
#define CHANNEL_INFO		 131072
#define CHANNEL_CHALLENGE        262144


/* Master Channels
 * Added by Decklarean
 */
#define CHANNEL_CLASS_MASTER    1048576
#define CHANNEL_CLAN_MASTER     2097152

/* Vent and Arena --Angi */
#define CHANNEL_GRATZ           8388608
#define CHANNEL_ARENA  	       16777216

/* Maudlin assimilate locations */
#define ASSIM_TORSO	0
#define ASSIM_RLEG	1
#define ASSIM_RARM	2
#define ASSIM_LARM	3
#define ASSIM_LLEG	4
#define ASSIM_EXTRA_1	5
#define ASSIM_EXTRA_2	6
#define ASSIM_EXTRA_3	7
#define ASSIM_EXTRA_4	8
#define ASSIM_EXTRA_5	9

/*
 * Prototype for a mob.
 * This is the in-memory version of #MOBILES.
 */
struct	mob_index_data
{
    MOB_INDEX_DATA *	next;
    SPEC_FUN *		spec_fun;
    SHOP_DATA *		pShop;
    TATTOO_ARTIST_DATA *pTattoo;
    CASINO_DATA *	pCasino;
    AREA_DATA *		area;			/* OLC */
    MPROG_DATA *        mobprogs;
    char *		player_name;
    char *		short_descr;
    char *		long_descr;
    char *		description;

    int			ally_vnum;
    int			ally_level;

    int			race_type;
    int			fighting_style;
    int			size;
    int 		vnum;
    int			progtypes;
    int 		count;
    int 		killed;
    short 		sex;
    short		class;
    int		 	level;
    int			act;
    long		affected_by;
    long		affected_by2;
    long		imm_flags;	/* XOR */
    long		res_flags;
    long		vul_flags;	/* XOR */
    int 		alignment;
    int 		hitroll;	
    int 		p_damp;			
    int 		hit_modi;		
    int 		skin;		
    int 	      	m_damp;
    int 		perm_str;
    int 		perm_int;
    int 		perm_wis;
    int 		perm_dex;
    int 		perm_con;
    int 		perm_agi;
    int 		perm_cha;
    MONEY_DATA		money;
    int			skill_tree[MAX_SKILL_TREE];
};

/*
 * -- Altrag
 */
struct  gskill_data
{
    int                 sn;
    void *              victim;
    int                 level;
    int                 timer;
};

#define FIGHTING_STYLE_NORMAL		0
#define FIGHTING_STYLE_MARTIAL		1
#define FIGHTING_STYLE_STREET		2
#define FIGHTING_STYLE_WIMPY		3
#define FIGHTING_STYLE_SNEAK		4
#define FIGHTING_STYLE_BEAST_AGGRESSIVE	5
#define FIGHTING_STYLE_BEAST_WIMPY	6
#define FIGHTING_STYLE_WARLOCK		7
#define FIGHTING_STYLE_CLERIC		8
#define FIGHTING_STYLE_BARBARIAN	9

/*
 * One character (PC or NPC).
 */
struct	char_data
{
    CHAR_DATA *		next;
    CHAR_DATA *		next_in_room;
    CHAR_DATA *		master;
    CHAR_DATA *		leader;
    CHAR_DATA *		fighting;
    CHAR_DATA *         engaged;
    CHAR_DATA *         hunting;
    CHAR_DATA *		reply;
    CHAR_DATA *		memory;
    CHAR_DATA *		fleeing_from;
    SPEC_FUN *		spec_fun;
    MOB_INDEX_DATA *	pIndexData;
    DESCRIPTOR_DATA *	desc;
    AFFECT_DATA *	affected;
    AFFECT_DATA *	affected2;
    NOTE_DATA *		pnote;
    OBJ_DATA *		carrying;
    OBJ_DATA *		resting_on;
    ROOM_INDEX_DATA *	in_room;
    ROOM_INDEX_DATA *	was_in_room;
    POWERED_DATA *	powered;
    PC_DATA *		pcdata;
    TATTOO_DATA *	tattoo;
    MPROG_ACT_LIST *    mpact;
    GSPELL_DATA *       gspell;
/* New interp engine */
    Q_DATA *		interpqueue;
    char 		doing [MAX_INPUT_LENGTH];
/* New interp engine end */
    char *		name;
    char *		oname;
    char *		short_descr;
    char *		long_descr;
    char *		description;
    char *              prompt;
    int			fighting_style;
    int			race_type;
    int                 cquestpnts;    /* for clan quests */
    int                 questpoints;   /* for autoquests  */
    int                 countdown;
    short 		sex;	
/* FOR MULTICLASS BELOW */
    short 		class[ MAX_CLASS ];
/* FOR MULTICLASS ABOVE */
    int                 mpactnum;
    short 		race;
    short               clan;
    char                clev;
    int                 ctimer;
    int                 wizinvis;
    int			cloaked;
    int			level;
    int                 antidisarm;
    int  		trust;
    bool                wizbit;
    int			played;
    time_t		logon;
    time_t		save_time;
    time_t              last_note;
    int 		timer;
    int 		wait;
    int                 race_wait; 
    int 		hit;
    int			perm_hit;
    int			mod_hit;
    int 		mana;
    int			perm_mana;
    int			mod_mana;
    int 		move;
    int			perm_move;
    int			mod_move;
    int			charisma; 
    MONEY_DATA		money;
    int			exp;
    int			act;
    long		affected_by;
    long		affected_by2;
    long        	imm_flags;
    long 	      	res_flags;
    long  		vul_flags;
    int 		position;
    int 		practice;
    int 		carry_weight;
    int 		carry_number;
    int 		saving_throw;
    short 		alignment;
    char		start_align;
    int 		hitroll;
    int 		damroll;
    int 		p_damp;
    int 		m_damp;
    int 		wimpy;
    int 		deaf;
    bool                deleted;
    int			combat_timer;	/* XOR */
    int			summon_timer;	/* XOR */
    int                 stunned[STUN_MAX];
    int			wiznet;
    int 		perm_str;
    int 		perm_int;
    int 		perm_wis;
    int 		perm_dex;
    int 		perm_con;
    int 		perm_agi;
    int 		perm_cha;
    int                 skin;
    int			size;
    int			rangedir;
    int                 hunttimer;
    int			skill_tree[MAX_SKILL_TREE];
};

struct quest_data
{
  QUEST_DATA *	next;
  int           vnum;
  char *        name;
};

struct queue_data
{
  Q_DATA *	next;
  char		command [MAX_INPUT_LENGTH];
};

struct  mob_prog_act_list
{
    MPROG_ACT_LIST * next;
    char *           buf;
    CHAR_DATA *      ch;
    OBJ_DATA *       obj;
    void *           vo;
};

struct  mob_prog_data
{
    MPROG_DATA *	next;
    int			type;
    char *		arglist;
    char *		comlist;
};

struct	tattoo_data
{
    TATTOO_DATA *	next;
    TATTOO_DATA *	next_content;
    CHAR_DATA *		carried_by;
    char *		short_descr;
    int 		wear_loc;
    int                 magic_boost;
    AFFECT_DATA *	affected;
    bool                deleted;
};

/*
 * Data which only PC's have.
 */
struct	pc_data
{
    PC_DATA *		next;
    ALIAS_DATA *        alias_list;
   /* You can't tell me this isn't cool,
      two opponents per fight to go please -Flux */
    CHAR_DATA *		twin_fighting;
    char *		spouse;
    char *		pwd;
    char *              afkchar; 
    char *		bamfin;
    char *		bamfout;
    char *		walkin;
    char *		walkout;
    char *		bamfusee;
    char *		restout;
    char *		restusee;
    char *              transto;
    char *              transfrom;
    char *              transvict;
    char *              slayusee;
    char *              slayroom;
    char *              slayvict;
    char *		whotype;
    char *		title;
    char *		otitle;
    char *		empowerments;
    char *		detractments;
    int			avatar;
   /* Maudlin code -Flux */
    int			assimilate[10];
   /* Quicksilver code -Flux */
    int			morph[4];
    int			speaking;
    int			language[MAX_LANGUAGE];
    int			regen;
    int			weapon[6];
    int 		remort;    
    int			claws;
    int           	attitude;
    int 		mod_str;
    int 		mod_int;
    int 		mod_wis;
    int 		mod_dex;
    int 		mod_con;
    int 		mod_agi;
    int			mod_cha;
    int                 pimm[16];
    int                 mimm[16];    
    int 		condition	[ 4 ];
    int                 pagelen;
    int 		learned		[ MAX_SKILL ];
    bool                switched;
    int			bounty;
    int 		security;	/* OLC - Builder security */
    MONEY_DATA		bankaccount;
    OBJ_DATA          * storage;
    int                 storcount;
    int                 corpses;
    char *		plan;
    char *		email;
   /* twin coding -Flux */
    int			port;
};

struct  alias_data
{
    ALIAS_DATA *next;
    char       *old;
    char       *new;
};


/*
 * heh.. were discussing obj/room progs.. and then these triggers started
 * looking a helluva lot like em.. :).. so what the hell..? :).. main
 * difference between this struct and the mobprog stuff is that this is
 * implemented as traps.  ie.. the disarmable stuff..
 * also, these are a little more global.. :)
 */

/*
 * The object triggers.. quite a few of em.. :)
 */
#define OBJ_TRAP_ERROR           0  /* error! */
#define OBJ_TRAP_GET             A  /* obj is picked up */
#define OBJ_TRAP_DROP            B  /* obj is dropped */
#define OBJ_TRAP_PUT             C  /* obj is put into something */
#define OBJ_TRAP_WEAR            D  /* obj is worn */
#define OBJ_TRAP_LOOK            E  /* obj is looked at/examined */
#define OBJ_TRAP_LOOK_IN         F  /* obj is looked inside (containers) */
#define OBJ_TRAP_INVOKE          G  /* obj is invoked */
#define OBJ_TRAP_USE             H  /* obj is used (recited, zapped, ect) */
#define OBJ_TRAP_CAST            I  /* spell is cast on obj - percent */
#define OBJ_TRAP_CAST_SN         J  /* spell is cast on obj - by slot */
#define OBJ_TRAP_JOIN            K  /* obj is joined with another */
#define OBJ_TRAP_SEPARATE        L  /* obj is separated into two */
#define OBJ_TRAP_BUY             M  /* obj is bought from store */
#define OBJ_TRAP_SELL            N  /* obj is sold to store */
#define OBJ_TRAP_STORE           O  /* obj is stored in storage boxes */
#define OBJ_TRAP_RETRIEVE        P  /* obj is retrieved from storage */
#define OBJ_TRAP_OPEN            Q  /* obj is opened (containers) */
#define OBJ_TRAP_CLOSE           R  /* obj is closed (containers) */
#define OBJ_TRAP_LOCK            S  /* obj is locked (containers) */
#define OBJ_TRAP_UNLOCK          T  /* obj is unlocked (containers) */
#define OBJ_TRAP_PICK            U  /* obj is picked (containers) */
#define OBJ_TRAP_RANDOM          V  /* random trigger */
#define OBJ_TRAP_THROW           W  /* obj is thrown */
#define OBJ_TRAP_GET_FROM        X  /* to allow secondary obj's in get */
#define OBJ_TRAP_GIVE            Y  /* give an obj away */
#define OBJ_TRAP_FILL            Z  /* obj is filled (drink_cons) */
#define OBJ_TRAP_REMOVE          bb  /* obj is removed */

/*
 * Note that entry/exit/pass are only called if the equivilant exit
 * trap for the exit the person went through failed.
 * Pass is only called if the respective enter or exit trap failed.
 */
#define ROOM_TRAP_ERROR          0  /* error! */
#define ROOM_TRAP_ENTER          A  /* someone enters the room */
#define ROOM_TRAP_EXIT           B  /* someone leaves the room */
#define ROOM_TRAP_PASS           C  /* someone enters or leaves */
#define ROOM_TRAP_CAST           D  /* a spell was cast in room - percent */
#define ROOM_TRAP_CAST_SN        E  /* a spell was cast in room - by slot */
#define ROOM_TRAP_SLEEP          F  /* someone sleeps in the room */
#define ROOM_TRAP_WAKE           G  /* someone wakes up in the room */
#define ROOM_TRAP_REST           H  /* someone rests in the room */
#define ROOM_TRAP_DEATH          I  /* someone dies in the room */
#define ROOM_TRAP_TIME           J  /* depends on the time of day */
#define ROOM_TRAP_RANDOM         K  /* random trigger */

/*
 * enter/exit/pass rules are the same as those for room traps.
 * note that look trap is only called if scry trap fails.
 */
#define EXIT_TRAP_ERROR          0  /* error! */
#define EXIT_TRAP_ENTER          A  /* someone enters through the exit */
#define EXIT_TRAP_EXIT           B  /* someone leaves through the exit */
#define EXIT_TRAP_PASS           C  /* someone enters/leaves through exit */
#define EXIT_TRAP_LOOK           D  /* someone looks through exit */
#define EXIT_TRAP_SCRY           E  /* someone scrys through the exit */
#define EXIT_TRAP_OPEN           F  /* someone opens the exit (door) */
#define EXIT_TRAP_CLOSE          G  /* someone closes the exit (door) */
#define EXIT_TRAP_LOCK           H  /* someone locks the exit (door) */
#define EXIT_TRAP_UNLOCK         I  /* someone unlocks the exit (door) */
#define EXIT_TRAP_PICK           J  /* someone picks the exit (locked door) */

struct trap_data
{
    TRAP_DATA *next;
    TRAP_DATA *next_here;
    OBJ_INDEX_DATA *on_obj;
    ROOM_INDEX_DATA *in_room;
    EXIT_DATA *on_exit;
    int type;
    char *arglist;
    char *comlist;
    bool disarmable;
    bool disarmed;
    int disarm_dur;
};
    

/*
 * Liquids.
 */
#define LIQ_WATER        0
#define LIQ_MAX		16

/*food stuff -Flux. */
#define FOOD_PURE		0
#define FOOD_POISONED		1
#define FOOD_DISEASED		2
#define FOOD_INSANE		3
#define FOOD_HALLUCINATORY	4

/*gas stuff -Flux. */
#define GAS_AFFECT_NONE			0
#define GAS_AFFECT_POISONED		1
#define GAS_AFFECT_DISEASED		2
#define GAS_AFFECT_INSANE		3
#define GAS_AFFECT_HALLUCINATORY	4
#define GAS_AFFECT_STUN			5

/*claws stuff -Flux. */
#define CLAW_NONE		0
#define CLAW_NORMAL		1
#define CLAW_POISONED		2
#define CLAW_DISEASED		3
#define CLAW_PIERCE		4
#define CLAW_SLASH		5
#define CLAW_BASH		6
#define CLAW_CHOP		7

struct	liq_type
{
    char               *liq_name;
    char               *liq_color;
    int                 liq_affect [ 3 ];
};



/*
 * Extra description data for a room or object.
 */
struct	extra_descr_data
{
    EXTRA_DESCR_DATA *next;	   /* Next in list                     */
    char             *keyword;     /* Keyword in look/examine          */
    char             *description; /* What to see                      */
    bool              deleted;
};



/*
 * Prototype for an object.
 */
struct	obj_index_data
{
    OBJ_INDEX_DATA *	next;
    EXTRA_DESCR_DATA *	extra_descr;
    AFFECT_DATA *	affected;
    AREA_DATA *		area;			/* OLC */
    TRAP_DATA *         traps;
    int                 traptypes;
    char *		name;
    char *		short_descr;
    char *		description;
    int 		vnum;
    int 		item_type;
    int 		extra_flags;
/* FOR NEW FLAGS */
    int 		wear_flags;
    int 		count;
    int 		weight;
    int			bionic_flags;
    MONEY_DATA		cost;
    int                 level;
    int			value	[ 10 ];
    int                 invoke_type;
    int                 invoke_vnum;
    char *              invoke_spell;
    int                 invoke_charge [ 2 ];
    int                 join;
    int                 sep_one;
    int                 sep_two;
    int			composition;
    int			durability;
};



/*
 * One object.
 */
struct	obj_data
{
    OBJ_DATA *		next;
    OBJ_DATA *		next_content;
    OBJ_DATA *		contains;
    OBJ_DATA *		sheath;
    OBJ_DATA *		in_obj;
    CHAR_DATA * 	carried_by; 
   /* for bombs and corpse butchering -Flux */
    CHAR_DATA * 	owner; 
   /* For soul_bound stuff */
    char *	 	ownedby; 
    CHAR_DATA *         stored_by;
    EXTRA_DESCR_DATA *	extra_descr;
    AFFECT_DATA *	affected;
    OBJ_INDEX_DATA *	pIndexData;
    ROOM_INDEX_DATA *	in_room;
    char *		name;
    char *		short_descr;
    char *		description;
    int 		item_type;
    int 		extra_flags;
/* FOR NEW FLAGS */
    int 		wear_flags;
    int 		wear_loc;
    int			bionic_flags;
    int			bionic_loc;
    int 		weight;
    MONEY_DATA		cost;
    int 		level;
    int 		timer;
    int			value	[ 10 ];
    int                 invoke_type;
    int                 invoke_vnum;
    char *              invoke_spell;
    int                 invoke_charge [ 2 ];
    bool                deleted;
    int			bump;
    int			composition;
    int			durability;
};

#define EXIT_NONE		0
#define EXIT_PHYSICALBARRIER	1
#define EXIT_FORCEFIELD		2

/*
 * Exit data.
 */
struct	exit_data
{
    ROOM_INDEX_DATA *	to_room;
    EXIT_AFFECT_DATA*	eAffect;
    EXIT_DATA *		next;		/* OLC */
    TRAP_DATA *         traps;
    int                 traptypes;
    int			rs_flags;	/* OLC */
    int 		vnum;
    int 		exit_info;
    int 		key;
    char *		keyword;
    char *		description;
    int 		exit_affect_flags;
};



/*
 * Reset commands:
 *   '*': comment
 *   'M': read a mobile 
 *   'O': read an object
 *   'P': put object in object
 *   'G': give object to mobile
 *   'E': equip object to mobile
 *   'D': set state of door
 *   'R': randomize room exits
 *   'S': stop (end of list)
 */

/*
 * Area-reset definition.
 */
struct	reset_data
{
    RESET_DATA *	next;
    char		command;
    int 		arg1;
    int 		arg2;
    int 		arg3;
};



/*
 * Area definition.
 */
struct	area_data
{
    AREA_DATA *		next;
    char *		name;
    int                 recall;
    int 		age;
    int 		nplayer;
    char *		filename;	/* OLC */
    char *		builders;	/* OLC - Listing of builders */
    int			security;	/* OLC - Value 0-infinity  */
    int			lvnum;		/* OLC - Lower vnum */
    int			uvnum;		/* OLC - Upper vnum */
    int			vnum;		/* OLC - Area vnum  */
    int			area_flags;	/* OLC */
    short		llevel;		/* lowest player level suggested */
    short		ulevel;		/* upper player level suggested */
    int			def_color;
    int			temporal;
    int			weather;
    int			average_temp;
    int			average_humidity;
    bool		pressure_front;
    bool		temperature_front;
    int			pfront_length;
    int			tfront_length;
    int			winddir;
    int			winddir_length;
    int			owner;
};

struct  new_clan_data
{
    CLAN_DATA *         next;
    MONEY_DATA 		bankaccount;
    char *              name;
    char *		init;
    char *              warden;
    char *              description;
    char *              leader;
    char *              first;
    char *              second;
    char *              champ;
    bool                isleader;
    bool                isfirst;
    bool                issecond;
    bool                ischamp;
    int                 vnum;
    int                 recall;
    int                 pkills;
    int                 mkills;
    int                 members;
    int                 pdeaths;
    int                 mdeaths;
    int                 obj_vnum_1;
    int                 obj_vnum_2;
    int                 obj_vnum_3;
    int                 settings;
}; 

struct	exit_affect_data
{
    EXIT_AFFECT_DATA *	next;
    EXIT_DATA *		exit;
    int 		type;
    int			duration;
    int 		location;
};

/* 
 * ROOM AFFECT type 
 */
struct	room_affect_data
{
    ROOM_AFFECT_DATA *	next;
    ROOM_INDEX_DATA *	room;
    CHAR_DATA * 	powered_by;
    OBJ_DATA *		material;
    int 		type;
    int 		location;
};

struct	powered_data
{
    POWERED_DATA *	next;
    ROOM_INDEX_DATA *	room;
    ROOM_AFFECT_DATA *	raf;
    int			type;
    int			cost;
};

/*
 * Room type.
 */
struct	room_index_data
{
    ROOM_INDEX_DATA *	next;
    ROOM_AFFECT_DATA *  rAffect;
    CHAR_DATA *		people;
    OBJ_DATA *		contents;
    /* this is for traproom bombs -Flux */
    OBJ_DATA *		trap_bomb;
    EXTRA_DESCR_DATA *	extra_descr;
    AREA_DATA *		area;
    EXIT_DATA *		exit	[ 6 ];
    RESET_DATA *	reset_first;	/* OLC */
    RESET_DATA *	reset_last;	/* OLC */
    TRAP_DATA *         traps;
    int                 traptypes;
    char *		name;
    char *		description;
    int 		vnum;
    int 		room_flags;
    int 		light;
    char 		sector_type;
    int                 rd;    /* TRI ( Room damage ) */
    /* Weather -Flux */
    int			temp;
    int			humidity;
    int                 accumulation;
    int			ore_quantity;
    int			ore_fertility;
    int			ore_type;
};



/*
 * Types of attacks.
 * Must be non-overlapping with spell/skill types,
 * but may be arbitrary beyond that.
 */
#define TYPE_UNDEFINED             -1
#define TYPE_HIT                 1000



/*
 *  Target types.
 */
#define TAR_IGNORE		    0
#define TAR_CHAR_OFFENSIVE	    1
#define TAR_CHAR_DEFENSIVE	    2
#define TAR_CHAR_SELF		    3
#define TAR_OBJ_INV		    4
#define TAR_GROUP_OFFENSIVE         5
#define TAR_GROUP_DEFENSIVE         6
#define TAR_GROUP_ALL               7
#define TAR_GROUP_OBJ               8
#define TAR_GROUP_IGNORE            9
#define TAR_EXIT	            10

struct	skill_data
{
};

struct	skill_tree_data
{
 char *		name; /* full name of the skill */
 int		id; /* unique identifier number */
 int		family; /* Overall family identifier */
 int		direct_parent; /* skill identifier right above it in level */
 int		training_cost; /* cost modifier to train */
 int		parent_req; /* workaround - for training */
 int		str_mod;
 int		dex_mod;
 int		agi_mod;
 int		int_mod;
 int		wis_mod;
 int		con_mod;
 int		cha_mod;
};

/*
 * Skills include spells as a particular case.
 */
struct	skill_type
{
    char *	name;		   		/* Name of skill */
    int 	skill_level [ MAX_CLASS ];	/* Level needed by class */
    int		range;			/* How many rooms will this move? for spells */
    int		dualclass_one;			/* First of two classes needed for dualclass */
    int	dualclass_two;			/* Second of two classes needed for DC */
    int	dualclass_level;    		/* Level at which the dualclassed gets the skill */
    TOUCH_FUN *	touch_fun;	  		/* Spell pointer (for touch-magic) */
    bool    grasp;                		/* Does the touch-magic hold on? */
    SPELL_FUN *	spell_fun;	  		/* Spell pointer (for spells) */
    int 	target;		   		/* Legal targets */
    int 	minimum_position;			/* Position for caster / user */
    int *	pgsn;	  				/* Pointer to associated gsn	 */
    int 	min_mana;   			/* Minimum mana used	 */
    int 	beats;		   		/* Waiting time after use	 */
    char *	noun_damage;	   		/* Damage message		 */
    char *	msg_off;  				/* Wear off message		 */
    char *      room_msg_off;	   		/* Room Wear off message	 */
    bool	dispelable;
    int         slot;                   	/* For object loading         */
};

/*
 * -- Altrag
 */
struct gskill_type
{
    int         wait;                      /* Wait for casters in ticks  */
    int         slot;                      /* Matching skill_table sn    */
    int         casters[MAX_CLASS];        /* Casters needed by class    */
};


/*
 * These are skill_lookup return values for common skills and spells.
 */
extern  int	gsn_trip;
extern  int	gsn_eyestrike;
extern  int	gsn_slam;
extern  int	gsn_grapple;
extern  int	gsn_neckstrike;
extern  int	gsn_anklestrike;
extern  int	gsn_track;
extern  int	gsn_shield_block;
extern  int	gsn_turn_guilty;
extern  int     gsn_stun;
extern  int     gsn_berserk;

extern  int	gsn_backstab;
extern	int	gsn_backstab_2;
extern	int	gsn_throttle;
extern	int	gsn_organ_donor;
extern	int	gsn_circle_2;
extern	int	gsn_palm;
extern  int	gsn_hide;
extern  int	gsn_shroud;
extern  int	gsn_disguise;
extern  int	gsn_snake_stance;
extern  int	gsn_dragon_stance;
extern  int	gsn_tiger_stance;
extern  int	gsn_bull_stance;
extern  int	gsn_panther_stance;
extern  int	gsn_crane_stance;
extern  int	gsn_sparrow_stance;
extern  int	gsn_monkey_stance;
extern  int	gsn_peek;
extern  int	gsn_pick_lock;
extern  int     gsn_punch;
extern  int     gsn_jab_punch;
extern  int     gsn_cross_punch;
extern  int     gsn_kidney_punch;
extern  int     gsn_roundhouse_punch;
extern  int     gsn_uppercut_punch;
extern  int	gsn_sneak;
extern  int	gsn_steal;
extern  int	gsn_disarm;
extern  int     gsn_poison_weapon;

extern  int     gsn_bash;  
extern	int	gsn_enhanced_damage;
extern	int	gsn_armor_knowledge;
extern	int	gsn_weapons_mastery;
extern	int	gsn_blade_clash;
extern	int	gsn_place_force;
extern	int	gsn_pain_tolerance;
extern	int	gsn_superior_damage;
extern	int	gsn_desperation;
extern  int	gsn_ripotse;
extern  int	gsn_fisticuffs;
extern	int	gsn_redirect;
extern  int	gsn_parlay;
extern  int     gsn_enhanced_hit;
extern  int	gsn_flury;
extern  int	gsn_masterstrike;
extern	int	gsn_kick;
extern	int	gsn_charge;
extern	int	gsn_wrack;
extern	int	gsn_gaze;
extern	int	gsn_torture;
extern	int	gsn_whirl;
extern  int	gsn_high_kick;
extern  int	gsn_spin_kick;
extern  int	gsn_jump_kick;
extern  int     gsn_circle;
extern	int	gsn_rescue;
extern  int     gsn_repair;
extern  int     gsn_carve;
extern  int     gsn_sharpen;
extern  int     gsn_serrate;
extern  int     gsn_balance;
extern  int     gsn_chain_attack;
extern  int     gsn_gouge;
extern  int     gsn_alchemy;
extern  int     gsn_scribe;
extern	int	gsn_blindness;
extern	int	gsn_charm_person;
extern	int	gsn_curse;
extern	int	gsn_invis;
extern	int	gsn_mass_invis;
extern	int	gsn_poison;
extern	int	gsn_sleep;
extern  int     gsn_prayer;
extern  int     gsn_soulstrike;
extern  int     gsn_martyrize;
extern	int	gsn_scrolls;
extern	int	gsn_staves;
extern	int	gsn_wands;
extern  int	gsn_spellcraft;
extern  int	gsn_gravebind;
extern  int	gsn_mummify;
extern  int	gsn_multiburst;
extern	int	gsn_fastheal;
extern	int	gsn_rage;
extern	int	gsn_focus;
extern	int	gsn_bite;
extern	int	gsn_rush;
extern	int	gsn_terrorize;
extern	int	gsn_scent;
extern	int	gsn_scrytraps;
extern	int	gsn_trapdisarm;
extern	int	gsn_reflex;
extern	int	gsn_rake;
extern  int     gsn_headbutt;
extern  int     gsn_burst;
extern	int	gsn_flamehand;
extern	int	gsn_quiverpalm;
extern	int	gsn_frosthand;
extern	int	gsn_chaoshand;
extern	int	gsn_shockhand;
extern	int	gsn_colorhand;
extern	int	gsn_darkhand;
extern	int	gsn_facestrike;
extern	int	gsn_flykick;
extern	int	gsn_black_star;
extern	int	gsn_anatomyknow;
extern	int	gsn_tiger_strike;
extern	int	gsn_tiger_claw;
extern	int	gsn_tiger_rush;
extern	int	gsn_tiger_tail;
extern	int	gsn_crane_bill;
extern	int	gsn_crane_claw;
extern	int	gsn_crane_wing;
extern	int	gsn_panther_paw;
extern	int	gsn_panther_tail;
extern	int	gsn_panther_scratch;
extern	int	gsn_dragon_roar;
extern	int	gsn_dragon_blast;
extern	int	gsn_dragon_grab;
extern	int	gsn_snake_fang;
extern	int	gsn_snake_bite;
extern	int	gsn_snake_rush;
extern	int	gsn_bull_rush;
extern	int	gsn_sparrow_flower;
extern	int	gsn_sparrow_song;
extern	int	gsn_sparrow_wing;
extern	int	gsn_sparrow_hop;
extern	int	gsn_sparrow_claw;
extern	int	gsn_sparrow_smash;
extern	int	gsn_monkey_headbutt;
extern	int	gsn_blackbelt;
extern	int	gsn_ironfist;
extern	int	gsn_globedark;
extern	int	gsn_drowfire;
extern	int	gsn_snatch;
extern	int	gsn_retreat;
extern	int	gsn_antidote;
extern	int	gsn_molotov;
extern	int	gsn_blasto;
extern	int	gsn_timebomb;
extern	int	gsn_pipebomb;
extern	int	gsn_cherrybomb;
extern	int	gsn_teargas;
extern	int	gsn_chemicalbomb;
extern	int	gsn_fireammo;
extern	int	gsn_rocketammo;
extern	int	gsn_laserammo;
extern	int	gsn_engrave;
extern	int	gsn_haggle;
extern	int	gsn_greed;
extern	int	gsn_blindfight;
extern	int	gsn_shriek;
extern	int	gsn_gust_of_wind;
extern	int	gsn_spit;
extern	int	gsn_webspin;
extern	int	gsn_crush;
extern	int	gsn_ward_safe;
extern	int	gsn_traproom;
extern	int	gsn_ward_heal;
extern  int     gsn_incinerate;
extern  int     gsn_plague;
extern  int     gsn_hallucinate;
extern  int     gsn_insane;
extern  int     gsn_plasma;
extern  int     gsn_grip;
extern  int	gsn_wire;
extern  int     gsn_shadow_walk;
/*
 * Psionicist gsn's.
 */
extern  int     gsn_chameleon;
extern  int     gsn_domination;
extern  int     gsn_heighten;
extern  int     gsn_shadow;

/*
 * Utility macros.
 */
#define UMIN( a, b )		( ( a ) < ( b ) ? ( a ) : ( b ) )
#define UMAX( a, b )		( ( a ) > ( b ) ? ( a ) : ( b ) )
#define URANGE( a, b, c )	( ( b ) < ( a ) ? ( a )                       \
				                : ( ( b ) > ( c ) ? ( c )     \
						                  : ( b ) ) )
#define LOWER( c )		( ( c ) >= 'A' && ( c ) <= 'Z'                \
				                ? ( c ) + 'a' - 'A' : ( c ) )
#define UPPER( c )		( ( c ) >= 'a' && ( c ) <= 'z'                \
				                ? ( c ) + 'A' - 'a' : ( c ) )
#define IS_SET( flag, bit )	( ( flag ) &   ( bit ) )
#define SET_BIT( var, bit )	( ( var )  |=  ( bit ) )
#define REMOVE_BIT( var, bit )	( ( var )  &= ~( bit ) )
#define TOGGLE_BIT( var, bit )  ( ( var )  ^=  ( bit ) )



/*
 * Character macros.
 */
#define IS_NPC( ch )		( IS_SET( ( ch )->act, ACT_IS_NPC ) )
#define IS_IMMORTAL( ch )	( get_trust( ch ) >= LEVEL_IMMORTAL )
#define IS_HERO( ch )		( get_trust( ch ) >= LEVEL_HERO     )

#define IS_AFFECTED( ch, sn )	( IS_SET( ( ch )->affected_by, ( sn ) ) )
#define IS_AFFECTED2(ch, sn)	( IS_SET( ( ch )->affected_by2, (sn)))
#define IS_SIMM(ch, sn)		(IS_SET((ch)->imm_flags, (sn)))
#define IS_SRES(ch, sn)		(IS_SET((ch)->res_flags, (sn)))
#define IS_SVUL(ch, sn)		(IS_SET((ch)->vul_flags, (sn)))

#define IS_FRAMED( ch )		( ch->alignment >=  350 )
#define IS_GUILTY( ch )		( ch->alignment <= -350 )
#define IS_LOS( ch )    	( !IS_FRAMED( ch ) && !IS_GUILTY( ch ) )

#define IS_AWAKE( ch )		( ch->position > POS_SLEEPING )

#define GET_HITROLL( ch )      	( ( ch )->hitroll                            \
				 + str_app[get_curr_str( ch )].tohit )
#define GET_DAMROLL( ch )      	( ( ch )->damroll                            \
				 + str_app[get_curr_str( ch )].todam )

#define IS_OUTSIDE( ch )       	( !IS_SET(				     \
				    ( ch )->in_room->room_flags,       	     \
				    ROOM_INDOORS ) &&                        \
                                  !IS_SET(                                   \
                                    ( ch )->in_room->room_flags,             \
                                    ROOM_UNDERGROUND ) )

#define WAIT_STATE( ch, pulse ) ( ( ch )->wait = UMAX( ( ch )->wait,         \
						      ( pulse ) ) )


#define STUN_CHAR( ch, pulse, type ) ( (ch)->stunned[(type)] =               \
				       UMAX( (ch)->stunned[(type)],          \
					     (pulse) ) )

#define IS_STUNNED( ch, type ) ( (ch)->stunned[(type)] > 0 )
#define MANA_COST( ch, sn )     ( IS_NPC( ch ) ? 0 : UMAX (                  \
				skill_table[sn].min_mana,                    \
				100 / ( 2 + ch->level -                      \
			skill_table[sn].skill_level[bestskillclass(ch,sn)] ) ) )
#define SPELL_COST( ch, sn )	( MANA_COST( ch, sn ) )

#define MT( ch )		( ch->mana )
#define MT_MAX( ch )		( MAX_MANA(ch) )
#define IS_SWITCHED( ch )       ( ch->pcdata->switched )

#define UNDEAD_TYPE( ch )	( IS_NPC( ch ) ? ACT_UNDEAD : PLR_UNDEAD )

#define MAX_HIT( ch ) 		( (ch)->perm_hit + (ch)->mod_hit )
#define MAX_MANA( ch ) 		( (ch)->perm_mana + (ch)->mod_mana )
#define MAX_MOVE( ch ) 		( (ch)->perm_move + (ch)->mod_move )

/*
 * Object macros.
 */
#define CAN_WEAR( obj, part )	( IS_SET( ( obj)->wear_flags,  ( part ) ) )
#define IS_OBJ_STAT( obj, stat )( IS_SET( ( obj)->extra_flags, ( stat ) ) )
#define CAN_ADDBIONIC( obj, part )(IS_SET(( obj)->bionic_flags,  ( part )))

/*
 * Description macros.
 */
#define PERS( ch, looker )	( can_see( looker, ( ch ) ) ?		     \
				( IS_NPC( ch ) ? ( ch )->short_descr	     \
				               : ( ch )->oname ) : "someone" )



/*
 * Arena macro.
 */
#define IS_ARENA(ch)	(!IS_NPC((ch)) && (ch)->in_room->vnum > 1100 && \
			(ch)->in_room->vnum < 1200 )

/*
 * Structure for a command in the command lookup table.
 */
struct	cmd_type
{
    char * const	name;
    DO_FUN *		do_fun;
    int 		position;
    int 		level;
    int 		log;
};

/*
 * Global constants.
 */
extern	const	struct	str_app_type	str_app		[ 31 ];
extern	const	struct	int_app_type	int_app		[ 31 ];
extern	const	struct	wis_app_type	wis_app		[ 31 ];
extern	const	struct	dex_app_type	dex_app		[ 31 ];
extern	const	struct	con_app_type	con_app		[ 31 ];

extern  const	struct	material_type	material_table	[ MAX_MATERIAL ];
extern	const	struct	class_type	class_table	[ MAX_CLASS   ];
extern  const   struct  wiznet_type     wiznet_table    [ ];
extern	struct	cmd_type	cmd_table	[ ];
extern	const	struct	liq_type	liq_table	[ LIQ_MAX     ];
extern	const	struct	skill_type	skill_table	[ MAX_SKILL ];
extern	const	struct	skill_tree_data skill_tree_table [ MAX_SKILL_TREE ];
extern  const   struct  gskill_type     gskill_table    [ MAX_GSPELL  ];
extern	char *	const			title_table	[ MAX_CLASS+3 ]
							[ MAX_LEVEL+1 ]
							[ 2 ];

/*
 * Global variables.
 */

extern		PLAYERLIST_DATA	  *	playerlist; /* Decklarean */

extern		SOCIAL_DATA	  *	social_first; /* Decklarean */
extern		SOCIAL_DATA	  *	social_last;

extern		RACE_DATA	  *     first_race; /* Decklarean */

extern		HELP_DATA	  *	help_first;
extern		HELP_DATA	  *	help_last;
extern		HELP_DATA	  *	help_free;

extern		QUEST_DATA	  *     first_quest;

extern		SHOP_DATA	  *	shop_first;
extern		CASINO_DATA	  *	casino_first;
extern		TATTOO_ARTIST_DATA  *	tattoo_artist_first;

extern		BAN_DATA	  *	ban_list;
extern		CHAR_DATA	  *	char_list;
extern		DESCRIPTOR_DATA   *	descriptor_list;
extern		NOTE_DATA	  *	note_list;
extern		OBJ_DATA	  *	object_list;
extern		TATTOO_DATA	  *	tattoo_list;
extern          TRAP_DATA         *     trap_list;  /* Altech trap stuff */

extern		AFFECT_DATA	  *	affect_free;
extern		BAN_DATA	  *	ban_free;
extern		CHAR_DATA	  *	char_free;
extern		DESCRIPTOR_DATA	  *	descriptor_free;
extern		EXTRA_DESCR_DATA  *	extra_descr_free;
extern		NOTE_DATA	  *	note_free;
extern		OBJ_DATA	  *	obj_free;
extern		TATTOO_DATA	  *	tattoo_free;
extern		PC_DATA		  *	pcdata_free;

extern		char			bug_buf		[ ];
extern		time_t			current_time;
extern		bool			fLogAll;
extern		FILE *			fpReserve;
extern		KILL_DATA		kill_table	[ ];
extern		char			log_buf		[ ];
extern		ECONOMY_DATA		economy;
extern		TIME_INFO_DATA		time_info;
extern		ARENA_DATA		arena;
extern          char              *     down_time;
extern          char              *     warning1;
extern          char              *     warning2;
extern          int                     stype;
extern          int			prtv;
extern          int                     port;

/*
 * Command functions.
 * Defined in act_*.c (mostly).
 */
DECLARE_DO_FUN( do_clanquest    );
DECLARE_DO_FUN( do_mpasound     );
DECLARE_DO_FUN( do_mpat         );
DECLARE_DO_FUN( do_mpecho       );
DECLARE_DO_FUN( do_mpechoaround );
DECLARE_DO_FUN( do_mpechoat     );
DECLARE_DO_FUN( do_mpforce      );
DECLARE_DO_FUN( do_mpgoto       );
DECLARE_DO_FUN( do_mpjunk       );
DECLARE_DO_FUN( do_mpkill       );
DECLARE_DO_FUN( do_mptattoo     );
DECLARE_DO_FUN( do_mpmload      );
DECLARE_DO_FUN( do_mpoload      );
DECLARE_DO_FUN( do_mppurge      );
DECLARE_DO_FUN( do_mpstat       );
DECLARE_DO_FUN( do_mpcommands   );
DECLARE_DO_FUN( do_mpteleport   );
DECLARE_DO_FUN( do_mptransfer   );
DECLARE_DO_FUN( do_mpdamage     );
DECLARE_DO_FUN( do_mpaffect	);
DECLARE_DO_FUN( do_mpjump	);
DECLARE_DO_FUN( do_opstat	);
DECLARE_DO_FUN( do_rpstat	);
DECLARE_DO_FUN( do_accept	);
DECLARE_DO_FUN( do_account      );
DECLARE_DO_FUN(	do_advance	);
DECLARE_DO_FUN(	do_bonus	);
DECLARE_DO_FUN( do_affectedby   );
DECLARE_DO_FUN(	do_raffect	);
DECLARE_DO_FUN( do_afk		);
DECLARE_DO_FUN( do_afkmes       ); 
DECLARE_DO_FUN( do_walkmes      ); 
DECLARE_DO_FUN(	do_allow	);
DECLARE_DO_FUN( do_ansi         );
DECLARE_DO_FUN(	do_answer	);
DECLARE_DO_FUN( do_antidote	);
DECLARE_DO_FUN( do_beep		);
DECLARE_DO_FUN( do_molotov	);
DECLARE_DO_FUN( do_blasto	);
DECLARE_DO_FUN( do_timebomb	);
DECLARE_DO_FUN( do_marry	);
DECLARE_DO_FUN( do_divorce	);
DECLARE_DO_FUN( do_rings	);
DECLARE_DO_FUN( do_pipebomb	);
DECLARE_DO_FUN( do_cherrybomb	);
DECLARE_DO_FUN( do_teargas	);
DECLARE_DO_FUN( do_chemicalbomb	);
DECLARE_DO_FUN( do_engrave	);
DECLARE_DO_FUN(	do_areas	);
DECLARE_DO_FUN( do_assasinate   );
DECLARE_DO_FUN( do_astat        );
DECLARE_DO_FUN( do_astrip       );
DECLARE_DO_FUN(	do_at		);
DECLARE_DO_FUN(	do_auction	);
DECLARE_DO_FUN( do_auto         );
DECLARE_DO_FUN( do_autoexit     );
DECLARE_DO_FUN( do_autocoins    );
DECLARE_DO_FUN( do_autoassist   );
DECLARE_DO_FUN( do_autoloot     );
DECLARE_DO_FUN( do_autosac      );
DECLARE_DO_FUN( do_autosplit    );
DECLARE_DO_FUN(	do_butcher	);
DECLARE_DO_FUN(	do_backstab	);
DECLARE_DO_FUN( do_bamf		);
DECLARE_DO_FUN(	do_bash 	);
DECLARE_DO_FUN( do_berserk      );
DECLARE_DO_FUN( do_bid          );
DECLARE_DO_FUN( do_bodybag      );
DECLARE_DO_FUN(	do_brandish	);
DECLARE_DO_FUN( do_brief        );
DECLARE_DO_FUN(	do_bug		);
DECLARE_DO_FUN(	do_buy		);
DECLARE_DO_FUN(	do_purchase	);
DECLARE_DO_FUN(	do_gamble	);
DECLARE_DO_FUN( do_challenge	);
DECLARE_DO_FUN( do_chameleon    );
DECLARE_DO_FUN( do_champlist 	);
DECLARE_DO_FUN(	do_cast		);
DECLARE_DO_FUN(	do_touch		);
DECLARE_DO_FUN( do_changes	);
DECLARE_DO_FUN(	do_channels	);
DECLARE_DO_FUN(	do_chat		);
DECLARE_DO_FUN( do_clan         );
DECLARE_DO_FUN( do_clone	);
DECLARE_DO_FUN( do_cinfo        );
DECLARE_DO_FUN( do_circle       );
DECLARE_DO_FUN( do_clans        );
DECLARE_DO_FUN( do_clandesc     );
DECLARE_DO_FUN( do_class        );
DECLARE_DO_FUN( do_cloak	);
DECLARE_DO_FUN(	do_close	);
DECLARE_DO_FUN( do_combat       );
DECLARE_DO_FUN( do_combine      );
DECLARE_DO_FUN(	do_commands	);
DECLARE_DO_FUN(	do_compare	);
DECLARE_DO_FUN( do_conference   );
DECLARE_DO_FUN(	do_config	);
DECLARE_DO_FUN(	do_consider	);
DECLARE_DO_FUN( do_countcommands);
DECLARE_DO_FUN(	do_credits	);
DECLARE_DO_FUN( do_cross_punch	);
DECLARE_DO_FUN(	do_cset		);
DECLARE_DO_FUN( do_darkinvis    );
DECLARE_DO_FUN( do_delet	);
DECLARE_DO_FUN( do_delete	);
DECLARE_DO_FUN(	do_deny		);
DECLARE_DO_FUN( do_deposit      );
DECLARE_DO_FUN( do_desc_check	);
DECLARE_DO_FUN( do_descript_clean );
DECLARE_DO_FUN(	do_description	);
DECLARE_DO_FUN( do_detract	);
DECLARE_DO_FUN(	do_disarm	);
DECLARE_DO_FUN(	do_disconnect	);
DECLARE_DO_FUN( do_donate	);
DECLARE_DO_FUN(	do_down		);
DECLARE_DO_FUN(	do_drink	);
DECLARE_DO_FUN(	do_drop		);
DECLARE_DO_FUN(	do_east		);
DECLARE_DO_FUN(	do_eat		);
DECLARE_DO_FUN(	do_echo		);
DECLARE_DO_FUN( do_email	);
DECLARE_DO_FUN(	do_emote	);
DECLARE_DO_FUN( do_empower	);
DECLARE_DO_FUN( do_enter        );
DECLARE_DO_FUN(	do_equipment	);
DECLARE_DO_FUN(	do_examine	);
DECLARE_DO_FUN(	do_exits	);
DECLARE_DO_FUN( do_farsight     );
DECLARE_DO_FUN( do_fighting	);
DECLARE_DO_FUN(	do_fill		);
DECLARE_DO_FUN( do_finger       );
DECLARE_DO_FUN(	do_flee		);
DECLARE_DO_FUN( do_flury 	);
DECLARE_DO_FUN( do_masterstrike	);
DECLARE_DO_FUN(	do_follow	);
DECLARE_DO_FUN(	do_bounty	);
DECLARE_DO_FUN(	do_force	);
DECLARE_DO_FUN(	do_freeze	);
DECLARE_DO_FUN( do_fullname     );
DECLARE_DO_FUN(	do_get		);
/* Switch commands -Flux. */
DECLARE_DO_FUN(	do_pull		);
DECLARE_DO_FUN(	do_press	);
DECLARE_DO_FUN(	do_lift		);
DECLARE_DO_FUN(	do_lower	);
DECLARE_DO_FUN(	do_raise	);

DECLARE_DO_FUN(	do_give		);
DECLARE_DO_FUN(	do_goto		);
DECLARE_DO_FUN(	do_group	);
DECLARE_DO_FUN(	do_gtell	);
DECLARE_DO_FUN( do_guard        );
DECLARE_DO_FUN( do_heighten     );
DECLARE_DO_FUN(	do_help		);
DECLARE_DO_FUN( do_hero         );
DECLARE_DO_FUN(	do_hide		);
DECLARE_DO_FUN(	do_shroud	);
DECLARE_DO_FUN(	do_disguise	);
DECLARE_DO_FUN(	do_revert	);
DECLARE_DO_FUN(	do_stance	);
DECLARE_DO_FUN( do_high_kick	);
DECLARE_DO_FUN( do_hlist        );
DECLARE_DO_FUN(	do_holylight	);
#if defined( HOTREBOOT )
DECLARE_DO_FUN(	do_hotreboo	);
DECLARE_DO_FUN(	do_hotreboot	);
#endif
DECLARE_DO_FUN(	do_idea		);
DECLARE_DO_FUN(	do_ideas	);
DECLARE_DO_FUN( do_ownership	);
DECLARE_DO_FUN( do_immlist	);
DECLARE_DO_FUN(	do_immtalk	);
DECLARE_DO_FUN( do_indestructable);
DECLARE_DO_FUN( do_identify	);
DECLARE_DO_FUN( do_induct       );
DECLARE_DO_FUN( do_info		);
DECLARE_DO_FUN(	do_inventory	);
DECLARE_DO_FUN( do_invoke       );
DECLARE_DO_FUN( do_activate     );
DECLARE_DO_FUN(	do_invis	);
DECLARE_DO_FUN( do_jab_punch	);
DECLARE_DO_FUN( do_join         );
DECLARE_DO_FUN( do_jump_kick	);
DECLARE_DO_FUN(	do_kick		);
DECLARE_DO_FUN(	do_shoot	);
DECLARE_DO_FUN(	do_charge	);
DECLARE_DO_FUN(	do_wrack	);
DECLARE_DO_FUN(	do_torture	);
DECLARE_DO_FUN(	do_gaze		);
DECLARE_DO_FUN(	do_whirl	);
DECLARE_DO_FUN( do_kidney_punch	);
DECLARE_DO_FUN(	do_kill		);
DECLARE_DO_FUN(	do_parlay	);
DECLARE_DO_FUN(	do_list		);
DECLARE_DO_FUN(	do_lock		);
DECLARE_DO_FUN(	do_log		);
DECLARE_DO_FUN(	do_look		);
DECLARE_DO_FUN( do_lowrecall ); 
DECLARE_DO_FUN(	do_memory	);
DECLARE_DO_FUN(	do_mfind	);
DECLARE_DO_FUN(	do_artlist	);
DECLARE_DO_FUN(	do_casinolist	);
DECLARE_DO_FUN(	do_shoplist	);
DECLARE_DO_FUN(	do_mload	);
DECLARE_DO_FUN(	do_qset		);
DECLARE_DO_FUN(	do_mset		);
DECLARE_DO_FUN(	do_mstat	);
DECLARE_DO_FUN(	do_econstat	);
DECLARE_DO_FUN(	do_mwhere	);
DECLARE_DO_FUN(	do_murde	);
DECLARE_DO_FUN(	do_murder	);
DECLARE_DO_FUN(	do_music	);
DECLARE_DO_FUN( do_newcorpse	);
DECLARE_DO_FUN(	do_newlock	);
DECLARE_DO_FUN(	do_noemote	);
DECLARE_DO_FUN(	do_north	);
DECLARE_DO_FUN(	do_note		);
DECLARE_DO_FUN(	do_instruct	);
DECLARE_DO_FUN(	do_notell	);
DECLARE_DO_FUN( do_nukerep	);
DECLARE_DO_FUN(	do_numlock	);
DECLARE_DO_FUN(	do_ofind	);
DECLARE_DO_FUN( do_olist	);
DECLARE_DO_FUN(	do_oload	);
DECLARE_DO_FUN( do_ooc		);
DECLARE_DO_FUN(	do_open		);
DECLARE_DO_FUN(	do_order	);
DECLARE_DO_FUN(	do_oset		);
DECLARE_DO_FUN(	do_ostat	);
DECLARE_DO_FUN( do_outcast      );
DECLARE_DO_FUN(	do_owhere	);
DECLARE_DO_FUN( do_pagelen      );
DECLARE_DO_FUN(	do_pardon	);
DECLARE_DO_FUN(	do_password	);
DECLARE_DO_FUN( do_repair       );
DECLARE_DO_FUN( do_carve        );
DECLARE_DO_FUN( do_serrate      );
DECLARE_DO_FUN( do_sharpen      );
DECLARE_DO_FUN( do_balance      );
DECLARE_DO_FUN(	do_peace	);
DECLARE_DO_FUN(	do_pick		);
DECLARE_DO_FUN( do_plan		);
DECLARE_DO_FUN( do_playerlist   );
DECLARE_DO_FUN( do_pload        );
DECLARE_DO_FUN(	do_poison_weapon);
DECLARE_DO_FUN(	do_pose		);
DECLARE_DO_FUN(	do_practice	);
DECLARE_DO_FUN( do_prompt       );
DECLARE_DO_FUN( do_punch        );
DECLARE_DO_FUN(	do_purge	);
DECLARE_DO_FUN(	do_put		);
DECLARE_DO_FUN( do_pwhere       );
DECLARE_DO_FUN(	do_quaff	);
DECLARE_DO_FUN(	do_question	);
DECLARE_DO_FUN(	do_qui		);
DECLARE_DO_FUN(	do_quit		);
DECLARE_DO_FUN(	do_race_edit	);
DECLARE_DO_FUN( do_racelist	);
DECLARE_DO_FUN(	do_reboo	);
DECLARE_DO_FUN(	do_reboot	);
DECLARE_DO_FUN( do_rebuild	);
DECLARE_DO_FUN(	do_recall	);
DECLARE_DO_FUN(	do_recho	);
DECLARE_DO_FUN(	do_recite	);
DECLARE_DO_FUN( do_remake	);
DECLARE_DO_FUN( do_bind		);
DECLARE_DO_FUN( do_remote       );
DECLARE_DO_FUN(	do_remove	);
DECLARE_DO_FUN(	do_rent		);
DECLARE_DO_FUN( do_smith        );
DECLARE_DO_FUN(	do_reply	);
DECLARE_DO_FUN(	do_report	);
DECLARE_DO_FUN(	do_rescue	);
DECLARE_DO_FUN(	do_rest		);
DECLARE_DO_FUN( do_restrict     );
DECLARE_DO_FUN(	do_restore	);
DECLARE_DO_FUN(	do_godbolt	);
DECLARE_DO_FUN( do_retrieve     );
DECLARE_DO_FUN(	do_return	);
DECLARE_DO_FUN( do_roundhouse_punch);
DECLARE_DO_FUN(	do_rset		);
DECLARE_DO_FUN(	do_rstat	);
DECLARE_DO_FUN(	do_sacrifice	);
DECLARE_DO_FUN(	do_save		);
DECLARE_DO_FUN(	do_say		);
DECLARE_DO_FUN(	do_speak	);
DECLARE_DO_FUN(	do_score	);
DECLARE_DO_FUN(	do_sell		);
DECLARE_DO_FUN( do_seize	);
DECLARE_DO_FUN( do_remtattoo	);
DECLARE_DO_FUN( do_separate     );
DECLARE_DO_FUN( do_setlev       );
DECLARE_DO_FUN( do_wire		);
DECLARE_DO_FUN( do_shadow       );
DECLARE_DO_FUN( do_shadow_walk  );
DECLARE_DO_FUN(	do_shout	);
DECLARE_DO_FUN(	do_shutdow	);
DECLARE_DO_FUN(	do_shutdown	);
DECLARE_DO_FUN(	do_silence	);
DECLARE_DO_FUN(	do_sla		);
DECLARE_DO_FUN(	do_slay		);
DECLARE_DO_FUN(	do_diceroll	);
DECLARE_DO_FUN( do_slaymes   	);
DECLARE_DO_FUN(	do_sleep	);
DECLARE_DO_FUN( do_slist        );
DECLARE_DO_FUN(	do_slookup	);
DECLARE_DO_FUN( do_smash        );
DECLARE_DO_FUN( do_snatch	);
DECLARE_DO_FUN(	do_sneak	);
DECLARE_DO_FUN(	do_snoop	);
DECLARE_DO_FUN(	do_avatar	);
DECLARE_DO_FUN(	do_socials	);
DECLARE_DO_FUN( do_soulstrike   );
DECLARE_DO_FUN( do_martyrize    );
DECLARE_DO_FUN(	do_south	);
DECLARE_DO_FUN( do_spells       );
DECLARE_DO_FUN( do_spin_kick	);
DECLARE_DO_FUN(	do_split	);
DECLARE_DO_FUN( do_lset		);
DECLARE_DO_FUN(	do_skset	);
DECLARE_DO_FUN(	do_stset	);
DECLARE_DO_FUN( do_sstat        );
DECLARE_DO_FUN(	do_sstime	);
DECLARE_DO_FUN(	do_stand	);
DECLARE_DO_FUN(	do_steal	);
DECLARE_DO_FUN( do_store        );
DECLARE_DO_FUN( do_stun         );
DECLARE_DO_FUN(	do_switch	);
DECLARE_DO_FUN( do_consent	);
DECLARE_DO_FUN( do_spousetalk	);
DECLARE_DO_FUN(	do_tell		);
DECLARE_DO_FUN( do_telnetga     );
DECLARE_DO_FUN( do_throw        );
DECLARE_DO_FUN( do_trip		);
DECLARE_DO_FUN( do_anklestrike	);
DECLARE_DO_FUN( do_eyestrike	);
DECLARE_DO_FUN( do_neckstrike	);
DECLARE_DO_FUN(	do_time		);
DECLARE_DO_FUN(	do_title	);
DECLARE_DO_FUN( do_todo         );
DECLARE_DO_FUN( do_track        );
DECLARE_DO_FUN(	do_train	);
DECLARE_DO_FUN(	do_transfer	);
DECLARE_DO_FUN( do_transport    );
DECLARE_DO_FUN(	do_trmes	);
DECLARE_DO_FUN(	do_restmes	);
DECLARE_DO_FUN(	do_trust	);
DECLARE_DO_FUN(	do_typo		);
DECLARE_DO_FUN(	do_typos		);
DECLARE_DO_FUN(	do_unlock	);
DECLARE_DO_FUN(	do_up		);
DECLARE_DO_FUN( do_uppercut_punch);
DECLARE_DO_FUN(	do_users	);
DECLARE_DO_FUN(	do_value	);
DECLARE_DO_FUN( do_gratz        );
DECLARE_DO_FUN(	do_visible	);
DECLARE_DO_FUN( do_voodo        );
DECLARE_DO_FUN( do_vused        );
DECLARE_DO_FUN(	do_wake		);
DECLARE_DO_FUN( do_ward		);
DECLARE_DO_FUN( do_traproom	);
DECLARE_DO_FUN(	do_wear		);
DECLARE_DO_FUN(	do_dual		);
DECLARE_DO_FUN(	do_wield	);
DECLARE_DO_FUN(	do_sheath	);
DECLARE_DO_FUN(	do_unsheath	);
DECLARE_DO_FUN(	do_weather	);
DECLARE_DO_FUN(	do_west		);
DECLARE_DO_FUN(	do_where	);
DECLARE_DO_FUN(	do_who		);
DECLARE_DO_FUN( do_whotype	);
DECLARE_DO_FUN(	do_wimpy	);
DECLARE_DO_FUN( do_withdraw     );
DECLARE_DO_FUN(	do_wizhelp	);
DECLARE_DO_FUN( do_wizify       );
/*DECLARE_DO_FUN( do_wizlist      ); */
DECLARE_DO_FUN(	do_wizlock	);
DECLARE_DO_FUN( do_wiznet	);
DECLARE_DO_FUN( do_worth        );
DECLARE_DO_FUN( do_wrlist       );
DECLARE_DO_FUN(	do_yell		);
DECLARE_DO_FUN(	do_zap		);
DECLARE_DO_FUN( do_stare        );
/* XOR */
DECLARE_DO_FUN( do_vnum         );
DECLARE_DO_FUN( do_load		);
DECLARE_DO_FUN( do_push		);
DECLARE_DO_FUN( do_drag		);
DECLARE_DO_FUN( do_authorize	);
DECLARE_DO_FUN( do_gouge        );
DECLARE_DO_FUN( do_alchemy      );
DECLARE_DO_FUN( do_scribe       );
DECLARE_DO_FUN( do_multiburst	);
/* Necro skills by Hannibal */
/* Some skills by Flux */
DECLARE_DO_FUN( do_gravebind	);
DECLARE_DO_FUN( do_mummify	);
DECLARE_DO_FUN( do_bite		);
DECLARE_DO_FUN( do_terrorize	);
DECLARE_DO_FUN( do_scent	);
DECLARE_DO_FUN( do_scan		);
DECLARE_DO_FUN( do_scrytraps	);
DECLARE_DO_FUN( do_trapdisarm	);
DECLARE_DO_FUN( do_reflex	);
DECLARE_DO_FUN(	do_rake		);
/*
 * Racial Skills Start Here -- Hannibal
 */
DECLARE_DO_FUN( do_headbutt 	);  /* Minotaur */
DECLARE_DO_FUN( do_burst 	);  /* Pixie */
DECLARE_DO_FUN( do_healthsurge 	);  /* Pixie */
DECLARE_DO_FUN( do_energysurge 	);  /* Pixie */
DECLARE_DO_FUN( do_globedarkness);  /* Drow */
DECLARE_DO_FUN( do_drowfire	);  /* Drow */
DECLARE_DO_FUN( do_forge	);  /* Dwarf */
DECLARE_DO_FUN( do_spit		);  /* Draconi */
DECLARE_DO_FUN( do_morph	);  /* Quicksilver */
DECLARE_DO_FUN( do_assimilate	);  /* Maudlin */
DECLARE_DO_FUN( do_crush	);  /* Maudlin */
DECLARE_DO_FUN( do_webspin	);  /* Maudlin */
DECLARE_DO_FUN( do_superform	);  /* Human */
/* Monk skills -- Hannibal */
DECLARE_DO_FUN( do_flamehand	);
DECLARE_DO_FUN( do_quiverpalm	);
DECLARE_DO_FUN( do_frosthand	);
DECLARE_DO_FUN( do_chaoshand	);
DECLARE_DO_FUN( do_shockhand	);
DECLARE_DO_FUN( do_colorhand	);
DECLARE_DO_FUN( do_darkhand	);
DECLARE_DO_FUN( do_facestrike	);
DECLARE_DO_FUN( do_flyingkick	);
DECLARE_DO_FUN( do_slam		);
DECLARE_DO_FUN( do_grapple	);
DECLARE_DO_FUN( do_black_star	);
DECLARE_DO_FUN( do_ironfist	);
/* Bard skill */
DECLARE_DO_FUN( do_shriek	);
DECLARE_DO_FUN( do_gust_of_wind	);
/* Ban Functions -- Hannibal */
DECLARE_DO_FUN(	do_permban	);
DECLARE_DO_FUN(	do_tempban	);
DECLARE_DO_FUN(	do_newban	);
DECLARE_DO_FUN( do_banlist	);
DECLARE_DO_FUN( do_engage	);
DECLARE_DO_FUN( do_disengage	);
DECLARE_DO_FUN( do_reload	);
DECLARE_DO_FUN( do_unload	);
DECLARE_DO_FUN( do_bionic	);
DECLARE_DO_FUN( do_addbionic	);
DECLARE_DO_FUN( do_rembionic	);

DECLARE_TOUCH_FUN(	touch_shocking_grasp		);
DECLARE_TOUCH_FUN(	touch_cure_light		);
DECLARE_TOUCH_FUN(	touch_cure_critical		);
DECLARE_TOUCH_FUN(	touch_cure_serious		);
DECLARE_TOUCH_FUN(	touch_curse			);
DECLARE_TOUCH_FUN(	touch_truesight			);
DECLARE_TOUCH_FUN(	touch_remove_plasma		);
DECLARE_TOUCH_FUN(	touch_cure_disease		);
DECLARE_TOUCH_FUN(	touch_cure_poison		);
DECLARE_TOUCH_FUN(	touch_cure_blindness		);
DECLARE_TOUCH_FUN(	touch_null			);

/*
 * Spell functions.
 * Defined in magic.c.
 */
DECLARE_SPELL_FUN(	spell_null		);
DECLARE_SPELL_FUN(	spell_shocking_		);
DECLARE_SPELL_FUN(	spell_restoration	);
DECLARE_SPELL_FUN(      spell_raise_skeleton    );
DECLARE_SPELL_FUN(      spell_wake_dead		);
DECLARE_SPELL_FUN(      spell_summon_mummy	);
DECLARE_SPELL_FUN(      spell_raise_lich	);
DECLARE_SPELL_FUN(      spell_summon_pharaoh	);
DECLARE_SPELL_FUN(      spell_raise_bloodborn	);
DECLARE_SPELL_FUN(      spell_wretch_bone       );
DECLARE_SPELL_FUN(	spell_armor		);
DECLARE_SPELL_FUN(      spell_astral_rift       );
DECLARE_SPELL_FUN(      spell_aura              );
DECLARE_SPELL_FUN(	spell_bark_skin		);
DECLARE_SPELL_FUN(	spell_bless		);
DECLARE_SPELL_FUN(	spell_blindness		);
DECLARE_SPELL_FUN(	spell_blur		);
DECLARE_SPELL_FUN(	spell_holy_shield	);
DECLARE_SPELL_FUN(	spell_call_lightning	);
DECLARE_SPELL_FUN(	spell_change_sex	);
DECLARE_SPELL_FUN(	spell_facade            );
DECLARE_SPELL_FUN(	spell_call_rod_of_thorns );
DECLARE_SPELL_FUN(	spell_call_blazing_sword );
DECLARE_SPELL_FUN(	spell_polymorph_red_dragon );
DECLARE_SPELL_FUN(	spell_polymorph_great_dragon );
DECLARE_SPELL_FUN(	spell_polymorph_demon  );
DECLARE_SPELL_FUN(	spell_polymorph_beholder );
DECLARE_SPELL_FUN(	spell_polymorph_siren   );
DECLARE_SPELL_FUN(	spell_polymorph_angel   );
DECLARE_SPELL_FUN(	spell_polymorph_gaiaen_paladin);
DECLARE_SPELL_FUN(	spell_chant		);
DECLARE_SPELL_FUN(	spell_charm_person	);
DECLARE_SPELL_FUN(	spell_continual_light	);
DECLARE_SPELL_FUN(	spell_tattoo		);
DECLARE_SPELL_FUN(	spell_control_weather	);
DECLARE_SPELL_FUN(	spell_create_food	);
DECLARE_SPELL_FUN(	spell_create_spring	);
DECLARE_SPELL_FUN(	spell_create_water	);
DECLARE_SPELL_FUN(	spell_heal_blindness	);
DECLARE_SPELL_FUN(	spell_heal_poison	);
DECLARE_SPELL_FUN(	spell_heal_clap		);
DECLARE_SPELL_FUN(	spell_heal_disease	);
DECLARE_SPELL_FUN(	spell_heal_plasma	);
DECLARE_SPELL_FUN(	spell_heal_light	);
DECLARE_SPELL_FUN(	spell_heal_serious	);
DECLARE_SPELL_FUN(	spell_heal_critical	);
DECLARE_SPELL_FUN(	spell_detect_hidden	);
DECLARE_SPELL_FUN(	spell_detect_invis	);
DECLARE_SPELL_FUN(	spell_detect_magic	);
DECLARE_SPELL_FUN(	spell_detect_poison	);
DECLARE_SPELL_FUN(	spell_dispel_magic	);
DECLARE_SPELL_FUN(	spell_draw_strength	);
DECLARE_SPELL_FUN(	spell_earthquake	);
DECLARE_SPELL_FUN(	spell_enchant_weapon	);
DECLARE_SPELL_FUN(	spell_energy_drain	);
DECLARE_SPELL_FUN(	spell_faerie_fire	);
DECLARE_SPELL_FUN(	spell_faerie_fog	);
DECLARE_SPELL_FUN(      spell_fireshield        );
DECLARE_SPELL_FUN(	spell_fly		);
DECLARE_SPELL_FUN(	spell_temporal_field	);
DECLARE_SPELL_FUN(	spell_breathe_water	);
DECLARE_SPELL_FUN(	spell_gate		);
DECLARE_SPELL_FUN(	spell_general_purpose	);
DECLARE_SPELL_FUN(	spell_giant_strength	);
DECLARE_SPELL_FUN(      spell_eternal_intellect );
DECLARE_SPELL_FUN(	spell_golden_aura	);
DECLARE_SPELL_FUN(	spell_group_healing	);
DECLARE_SPELL_FUN(	spell_haste		);
DECLARE_SPELL_FUN(      spell_goodberry         );
DECLARE_SPELL_FUN(	spell_heal		);
DECLARE_SPELL_FUN(	spell_high_explosive	);
DECLARE_SPELL_FUN(	spell_iceshield		);
DECLARE_SPELL_FUN(	spell_identify		);
DECLARE_SPELL_FUN(      spell_incinerate        );
DECLARE_SPELL_FUN(      spell_plasma_burst      );
DECLARE_SPELL_FUN(      spell_spread_disease    );
DECLARE_SPELL_FUN(	spell_infravision	);
DECLARE_SPELL_FUN(	spell_inspiration 	);
DECLARE_SPELL_FUN(	spell_invis		);
DECLARE_SPELL_FUN(	spell_locate_object	);
DECLARE_SPELL_FUN(	spell_color_snap   	);
DECLARE_SPELL_FUN(	spell_color_bolt   	);
DECLARE_SPELL_FUN(	spell_color_blast   	);
DECLARE_SPELL_FUN(	spell_positronic_snap  	);
DECLARE_SPELL_FUN(	spell_positronic_bolt   );
DECLARE_SPELL_FUN(	spell_positronic_blast  );
DECLARE_SPELL_FUN(	spell_fire_snap   	);
DECLARE_SPELL_FUN(	spell_chaos_snap   	);
DECLARE_SPELL_FUN(	spell_fire_bolt   	);
DECLARE_SPELL_FUN(	spell_fire_blast   	);
DECLARE_SPELL_FUN(	spell_icy_snap   	);
DECLARE_SPELL_FUN(	spell_icy_bolt 	  	);
DECLARE_SPELL_FUN(	spell_icy_blast   	);
DECLARE_SPELL_FUN(	spell_electric_snap   	);
DECLARE_SPELL_FUN(	spell_electric_bolt   	);
DECLARE_SPELL_FUN(	spell_electric_blast   	);
DECLARE_SPELL_FUN(	spell_divine_snap   	);
DECLARE_SPELL_FUN(	spell_divine_bolt   	);
DECLARE_SPELL_FUN(	spell_divine_blast   	);
DECLARE_SPELL_FUN(	spell_demonic_snap   	);
DECLARE_SPELL_FUN(	spell_demonic_bolt   	);
DECLARE_SPELL_FUN(	spell_demonic_blast   	);
DECLARE_SPELL_FUN(	spell_acidic_snap  	);
DECLARE_SPELL_FUN(	spell_acidic_bolt   );
DECLARE_SPELL_FUN(	spell_acidic_blast  );
DECLARE_SPELL_FUN(	spell_entropic_snap  	);
DECLARE_SPELL_FUN(	spell_entropic_bolt   );
DECLARE_SPELL_FUN(	spell_entropic_blast  );
DECLARE_SPELL_FUN(	spell_gravity_snap  	);
DECLARE_SPELL_FUN(	spell_gravity_bolt   );
DECLARE_SPELL_FUN(	spell_gravity_blast  );
DECLARE_SPELL_FUN(	spell_positronic_flash 	);
DECLARE_SPELL_FUN(	spell_positronic_stream );
DECLARE_SPELL_FUN(	spell_positronic_storm  );
DECLARE_SPELL_FUN(	spell_fire_flash   	);
DECLARE_SPELL_FUN(	spell_fire_stream   	);
DECLARE_SPELL_FUN(	spell_fire_storm   	);
DECLARE_SPELL_FUN(	spell_icy_flash   	);
DECLARE_SPELL_FUN(	spell_icy_stream  	);
DECLARE_SPELL_FUN(	spell_icy_storm   	);
DECLARE_SPELL_FUN(	spell_electric_flash   	);
DECLARE_SPELL_FUN(	spell_electric_stream  	);
DECLARE_SPELL_FUN(	spell_electric_storm   	);
DECLARE_SPELL_FUN(	spell_divine_flash   	);
DECLARE_SPELL_FUN(	spell_divine_stream   	);
DECLARE_SPELL_FUN(	spell_divine_storm   	);
DECLARE_SPELL_FUN(	spell_demonic_flash  	);
DECLARE_SPELL_FUN(	spell_demonic_stream   	);
DECLARE_SPELL_FUN(	spell_demonic_storm   	);
DECLARE_SPELL_FUN(	spell_acidic_flash  	);
DECLARE_SPELL_FUN(	spell_acidic_stream   );
DECLARE_SPELL_FUN(	spell_acidic_storm  );
DECLARE_SPELL_FUN(	spell_entropic_flash  	);
DECLARE_SPELL_FUN(	spell_entropic_stream   );
DECLARE_SPELL_FUN(	spell_entropic_storm  );
DECLARE_SPELL_FUN(	spell_gravity_flash  	);
DECLARE_SPELL_FUN(	spell_gravity_stream   );
DECLARE_SPELL_FUN(	spell_gravity_storm  );
DECLARE_SPELL_FUN(      spell_mana              );
DECLARE_SPELL_FUN(	spell_mass_invis	);
DECLARE_SPELL_FUN(      spell_mental_block      );
DECLARE_SPELL_FUN(	spell_pass_door		);
DECLARE_SPELL_FUN(      spell_permenancy        );
DECLARE_SPELL_FUN(	spell_poison		);
DECLARE_SPELL_FUN(      spell_portal            );
DECLARE_SPELL_FUN(	spell_protection	);
DECLARE_SPELL_FUN(	spell_refresh		);
DECLARE_SPELL_FUN(	spell_remove_curse	);
DECLARE_SPELL_FUN(	spell_sanctuary		);
DECLARE_SPELL_FUN(      spell_scry              );
DECLARE_SPELL_FUN(      spell_shockshield       );
DECLARE_SPELL_FUN(	spell_shield		);
DECLARE_SPELL_FUN(	spell_sleep		);
DECLARE_SPELL_FUN(      spell_spell_bind        );
DECLARE_SPELL_FUN(	spell_stone_skin	);
DECLARE_SPELL_FUN(	spell_summon		);
DECLARE_SPELL_FUN(	spell_teleport		);
DECLARE_SPELL_FUN(	spell_touch_of_darkness	);
DECLARE_SPELL_FUN(	spell_brimstone_shockwave );
DECLARE_SPELL_FUN(	spell_harmonic_homicide	);
DECLARE_SPELL_FUN(	spell_strike_of_thorns	);
DECLARE_SPELL_FUN(	spell_silver_missile	);
DECLARE_SPELL_FUN(	spell_laserammo		);
DECLARE_SPELL_FUN(	spell_fireammo  	);
DECLARE_SPELL_FUN(	spell_rocketammo	);
DECLARE_SPELL_FUN(	spell_sweet_destruction	);
DECLARE_SPELL_FUN(	spell_phantom_razor	);
DECLARE_SPELL_FUN(      spell_turn_undead       );
DECLARE_SPELL_FUN(	spell_ventriloquate	);
DECLARE_SPELL_FUN(      spell_vibrate           );
DECLARE_SPELL_FUN(	spell_war_cry		);
DECLARE_SPELL_FUN(	spell_weaken		);
DECLARE_SPELL_FUN(	spell_word_of_recall	);
DECLARE_SPELL_FUN(	spell_acid_breath	);
DECLARE_SPELL_FUN(	spell_acid_spray	);
DECLARE_SPELL_FUN(	spell_fire_breath	);
DECLARE_SPELL_FUN(	spell_frost_breath	);
DECLARE_SPELL_FUN(	spell_gas_breath	);
DECLARE_SPELL_FUN(	spell_cataclysm 	);
DECLARE_SPELL_FUN(	spell_lightning_breath	);

DECLARE_SPELL_FUN(	spell_summon_swarm	);	/* XOR */
DECLARE_SPELL_FUN(	spell_carbon_copy	);	/* XOR */
DECLARE_SPELL_FUN(	spell_army_of_illusion  );	/* XOR */
DECLARE_SPELL_FUN(	spell_sphere_of_solitude);	/* XOR */
DECLARE_SPELL_FUN(	spell_forcefield	);	/* XOR */
DECLARE_SPELL_FUN(	spell_summon_pack	);	/* XOR */
DECLARE_SPELL_FUN(	spell_summon_demon	);	/* XOR */
DECLARE_SPELL_FUN(	spell_cancellation	);	/* XOR */
DECLARE_SPELL_FUN(	spell_enchanted_song	);	/* XOR */
DECLARE_SPELL_FUN(	spell_holy_strength	);	/* XOR */
DECLARE_SPELL_FUN(	spell_curse_of_nature	);	/* XOR */
DECLARE_SPELL_FUN(      spell_holysword         );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_summon_angel      );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_bladebarrier      );     /* ELVIS */
DECLARE_SPELL_FUN(	spell_bind_weapon	);
DECLARE_SPELL_FUN(      spell_flame_blade       );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_rainbow_blade     );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_shock_blade       );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_chaos_blade       );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_frost_blade       );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_web               );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_hold              );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_prism_cell        );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_mute              );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_entangle          );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_darkbless         );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_crusade           );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_confusion         );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_bio_acceleration  );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_mind_probe        );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_chain_lightning   );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_meteor_swarm      );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_fumble            );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_lighten_mind      );
DECLARE_SPELL_FUN(      spell_lokian_revenge    );
DECLARE_SPELL_FUN(      spell_necro_glycerine   );
DECLARE_SPELL_FUN(      spell_taint_mind        );
DECLARE_SPELL_FUN(      spell_summon_shadow     );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_summon_beast      );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_summon_trent      );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_shatter           );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_molecular_unbind  );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_phase_shift       );     /* ELVIS */
DECLARE_SPELL_FUN(      spell_mist_form         );     /* Lacey */
/*
 * Psi spell_functions, in magic.c.
 */
DECLARE_SPELL_FUN(      spell_adrenaline_control);
DECLARE_SPELL_FUN(      spell_gaiaen_power      );
DECLARE_SPELL_FUN(      spell_agitation         );
DECLARE_SPELL_FUN(      spell_awe               );
DECLARE_SPELL_FUN(      spell_serenity          );
DECLARE_SPELL_FUN(      spell_biofeedback       );
DECLARE_SPELL_FUN(      spell_crystal_flesh     );
DECLARE_SPELL_FUN(      spell_cell_adjustment   );
DECLARE_SPELL_FUN(      spell_chaosfield        );
DECLARE_SPELL_FUN(      spell_combat_mind       );
DECLARE_SPELL_FUN(      spell_complete_healing  );
DECLARE_SPELL_FUN(      spell_create_sound      );
DECLARE_SPELL_FUN(      spell_death_field       );
DECLARE_SPELL_FUN(      spell_displacement      );
DECLARE_SPELL_FUN(      spell_disrupt           );
DECLARE_SPELL_FUN(      spell_domination        );
DECLARE_SPELL_FUN(      spell_ectoplasmic_form  );
DECLARE_SPELL_FUN(      spell_ego_whip          );
DECLARE_SPELL_FUN(      spell_energy_containment);
DECLARE_SPELL_FUN(      spell_enhance_armor     );
DECLARE_SPELL_FUN(      spell_enhanced_strength );
DECLARE_SPELL_FUN(      spell_flesh_armor       );
DECLARE_SPELL_FUN(      spell_inertial_barrier  );
DECLARE_SPELL_FUN(      spell_intellect_fortress);
DECLARE_SPELL_FUN(      spell_holy_wrath        );
DECLARE_SPELL_FUN(      spell_lend_health       );
DECLARE_SPELL_FUN(      spell_levitation        );
DECLARE_SPELL_FUN(      spell_mental_barrier    );
DECLARE_SPELL_FUN(      spell_sonicvibe         );
DECLARE_SPELL_FUN(      spell_psychic_drain     );
DECLARE_SPELL_FUN(      spell_psychic_healing   );
DECLARE_SPELL_FUN(      spell_share_strength    );
DECLARE_SPELL_FUN(      spell_thought_shield    );
DECLARE_SPELL_FUN(     	gspell_flamesphere      );
DECLARE_SPELL_FUN(     	gspell_mass_shield      );
/* Necromancer & Monk spells by Hannibal. */
DECLARE_SPELL_FUN(	spell_hex		);
DECLARE_SPELL_FUN(	spell_dark_ritual	);
DECLARE_SPELL_FUN(	spell_field_of_decay	);
DECLARE_SPELL_FUN(	spell_armor_of_thorns	);
DECLARE_SPELL_FUN(	spell_blink     	);
DECLARE_SPELL_FUN(	spell_physical_mirror   );
DECLARE_SPELL_FUN(	spell_deadly_vein       );
DECLARE_SPELL_FUN(	spell_mana_net          );
DECLARE_SPELL_FUN(	spell_mana_shield       );
DECLARE_SPELL_FUN(	spell_magical_mirror    );
DECLARE_SPELL_FUN(	spell_mirror_images	);
DECLARE_SPELL_FUN(	spell_stench_of_decay	);
DECLARE_SPELL_FUN(	spell_iron_skin		);
DECLARE_SPELL_FUN(	spell_chi_shield	);
DECLARE_SPELL_FUN(    spell_pass_plant 		); /*Deck*/

DECLARE_SPELL_FUN(    spell_soul_bind 		); /*Malaclypse*/

/*
 * OS-dependent declarations.
 * These are all very standard library functions,
 *   but some systems have incomplete or non-ansi header files.
 */
#if	defined( _AIX )
char *	crypt		args( ( const char *key, const char *salt ) );
#endif

#if	defined( apollo )
int	atoi		args( ( const char *string ) );
void *	calloc		args( ( unsigned nelem, size_t size ) );
char *	crypt		args( ( const char *key, const char *salt ) );
#endif

#if	defined( hpux )
char *	crypt		args( ( const char *key, const char *salt ) );
#endif

#if	defined( linux )
char 	crypt		args( ( const char *key, const char *salt ) );
#endif

#if	defined( macintosh )
#define NOCRYPT
#if	defined( unix )
#undef	unix
#endif
#endif

#if	defined( MIPS_OS )
char *	crypt		args( ( const char *key, const char *salt ) );
#endif

#if	defined( MSDOS )
#define NOCRYPT
#if	defined( unix )
#undef	unix
#endif
#endif

#if	defined( NeXT )
char *	crypt		args( ( const char *key, const char *salt ) );
#endif

#if	defined( sequent )
char *	crypt		args( ( const char *key, const char *salt ) );
int	fclose		args( ( FILE *stream ) );
int	fprintf		args( ( FILE *stream, const char *format, ... ) );
int	fread		args( ( void *ptr, int size, int n, FILE *stream ) );
int	fseek		args( ( FILE *stream, long offset, int ptrname ) );
void	perror		args( ( const char *s ) );
int	ungetc		args( ( int c, FILE *stream ) );
#endif

#if	defined( sun )
char *	crypt		args( ( const char *key, const char *salt ) );
int	fclose		args( ( FILE *stream ) );
int	fprintf		args( ( FILE *stream, const char *format, ... ) );
size_t	fread	args( ( void *ptr, size_t size, size_t n, FILE *stream ) );
int	fseek		args( ( FILE *stream, long offset, int ptrname ) );
void	perror		args( ( const char *s ) );
int	ungetc		args( ( int c, FILE *stream ) );
#endif

#if	defined( ultrix )
char *	crypt		args( ( const char *key, const char *salt ) );
#endif



/*
 * The crypt(3) function is not available on some operating systems.
 * In particular, the U.S. Government prohibits its export from the
 *   United States to foreign countries.
 * Turn on NOCRYPT to keep passwords in plain text.
 */
#if	defined( NOCRYPT )
#define crypt( s1, s2 )	( s1 )
#endif



/*
 * Data files used by the server.
 *
 * AREA_LIST contains a list of areas to boot.
 * All files are read in completely at bootup.
 * Most output files (bug, idea, typo, shutdown) are append-only.
 *
 * The NULL_FILE is held open so that we have a stream handle in reserve,
 *   so players can go ahead and telnet to all the other descriptors.
 * Then we close it whenever we need to open a file (e.g. a save file).
 */
#if defined( macintosh )
#define PLAYER_DIR	""		/* Player files			*/
#define NULL_FILE	"proto.are"	/* To reserve one stream	*/
#endif

#if defined( MSDOS )
#define PLAYER_DIR	""		/* Player files                 */
#define NULL_FILE	"nul"		/* To reserve one stream	*/
#endif

#if defined( unix )
#define PLAYER_DIR	"../data/player/"	/* Player files */
#define NULL_FILE	"/dev/null"	/* To reserve one stream	*/
#define AREA_DIR	"../data/area/" /* Area files */
#define OBJ_DIR		"../data/area/objects/" /* obj file location */
#define MOB_DIR		"../data/area/mobiles/" /* mob file location */
#endif

#if defined( linux )
#define PLAYER_DIR	"../data/player/"	/* Player files  */
#define NULL_FILE	"/dev/null"	/* To reserve one stream	*/
#define PROG_DIR	"../data/area/MOBProgs/"  /* MOBProg files */
#define AREA_DIR	"../data/area/" /* Area files */
#define OBJ_DIR		"../data/area/objects/" /* obj file location */
#define MOB_DIR		"../data/area/mobiles/" /* mob file location */
#endif

#define TEMP_AREA	"temparea.are"	/* For do_asave */
#define AREA_LIST	"area.lst"	/* List of areas		*/
#define BAN_LIST	"../banned.lst" /* List of banned sites & users */
#define BUG_FILE	"bugs.txt"      /* For 'bug' and bug( )		*/
#define IDEA_FILE	"ideas.txt"	/* For 'idea'			*/
#define TYPO_FILE	"typos.txt"     /* For 'typo'			*/
#define NOTE_FILE	"notes.txt"	/* For 'notes'			*/
#define CLAN_FILE       "clan.dat"      /* For 'clans'                  */
#define SOCIAL_FILE	"social.dat"	/* For 'socials'		*/
#define RACE_FILE	"race.dat"	/* For 'races'  		*/
#define HELP_FILE	"help.dat"	/* For 'races'  		*/
#define QUEST_FILE	"quest.dat"	/* For 'quests'  		*/
#define SHUTDOWN_FILE	"shutdown.txt"	/* For 'shutdown'		*/
#define DOWN_TIME_FILE  "time.txt"      /* For automatic shutdown       */
#define USERLIST_FILE   "users.txt"     /* Userlist -- using identd TRI */
#define AUTH_LIST       "auth.txt"      /* List of who auth who         */
#define PLAYERLIST_FILE "player.lst"    /* Player List 			*/
#if defined( HOTREBOOT )
#define HOTREBOOT_FILE 	"hotreboot.dat"  /* temporary data file used 	*/
#endif

/*
 * Our function prototypes.
 * One big lump ... this is every function in Merc.
 */
#define CD	CHAR_DATA
#define TD	TATTOO_DATA
#define MID	MOB_INDEX_DATA
#define OD	OBJ_DATA
#define OID	OBJ_INDEX_DATA
#define RID	ROOM_INDEX_DATA
#define CID     CLAN_DATA
#define SF	SPEC_FUN
#define BD	BAN_DATA
/* language.c */
char *translate		args( ( CHAR_DATA *ch, char *argument ) );

/* act_comm.c */
void	add_follower	args( ( CHAR_DATA *ch, CHAR_DATA *master ) );
void	stop_follower	args( ( CHAR_DATA *ch ) );
void	die_follower	args( ( CHAR_DATA *ch, char *name ) );
bool	is_same_group	args( ( CHAR_DATA *ach, CHAR_DATA *bch ) );
bool	is_note_to	args( ( CHAR_DATA *ch, NOTE_DATA *pnote ) );
int	group_size	args( ( CHAR_DATA *ch ) );
bool	in_group	args( ( CHAR_DATA *ch ) );


/* act_info.c */
void	set_title	args( ( CHAR_DATA *ch, char *title ) );
bool	check_blind	args( ( CHAR_DATA *ch ) );

MONEY_DATA *add_money    args( ( MONEY_DATA *a, MONEY_DATA *b ) );
MONEY_DATA *sub_money	 args( ( MONEY_DATA *a, MONEY_DATA *b ) );
MONEY_DATA *take_money   args( ( CHAR_DATA *ch, int amt, char *type, char *verb ) );
MONEY_DATA *spend_money  args( ( MONEY_DATA *a, MONEY_DATA *b ) );
char       *money_string  args( ( MONEY_DATA *money ) );

/* act_move.c */
void	move_char	args( ( CHAR_DATA *ch, int door, bool Fall, bool flee ) );

/* casino.c */
bool	casino_simple_dice args( ( CHAR_DATA *ch, CHAR_DATA *dealer, int value ) );
bool	casino_three_card_monty args( ( CHAR_DATA *ch, CHAR_DATA *dealer, int card ) );


/* act_obj.c */
void    bomb_explode    args( ( OBJ_DATA *bomb, ROOM_INDEX_DATA *in_room ) );
void	  wear_obj		args( ( CHAR_DATA *ch, OBJ_DATA *obj, bool fReplace ) );
bool	  remove_obj	args( ( CHAR_DATA *ch, int iWear, bool fReplace ) );


/* act_wiz.c */
ROOM_INDEX_DATA *	find_location	args( ( CHAR_DATA *ch, char *arg ) );
void wiznet             args( (char *string, CHAR_DATA *ch, OBJ_DATA *obj,
                               long flag, long flag_skip, int min_level ) );

/* comm.c */
void	close_socket	 args( ( DESCRIPTOR_DATA *dclose ) );
void	write_to_buffer	 args( ( DESCRIPTOR_DATA *d, const char *txt,
				int length ) );
void    send_to_all_char args( ( const char *text, bool immonly ) );
void    send_to_al       args( ( int clr, int level, char *text ) );
/* send to above level---^   TRI */
void	send_to_char	 args( ( int AType, const char *txt, CHAR_DATA *ch ) );
void	editor_send_to_char	 args( ( int AType, const char *txt, CHAR_DATA *ch ) );
void    set_char_color   args( ( int AType, CHAR_DATA *ch ) );
void    show_string      args( ( DESCRIPTOR_DATA *d, char *input ) );
void	act	         args( ( int AType, const char *format, CHAR_DATA *ch,
				const void *arg1, const void *arg2,
				int type ) );

/* db.c */
void	boot_db		args( ( void ) );
void	area_update	args( ( void ) );
CD *	create_mobile	args( ( MOB_INDEX_DATA *pMobIndex ) );
OD *	create_object	args( ( OBJ_INDEX_DATA *pObjIndex, int level ) );
TD *	create_tattoo   args( ( CHAR_DATA *victim ) );
void	clear_char	args( ( CHAR_DATA *ch ) );
void	free_char	args( ( CHAR_DATA *ch ) );
char *	get_extra_descr	args( ( CHAR_DATA *CH, char *name, EXTRA_DESCR_DATA *ed ) );
MID *	get_mob_index	args( ( int vnum ) );
OID *	get_obj_index	args( ( int vnum ) );
RID *	get_room_index	args( ( int vnum ) );
CID *   get_clan_index  args( ( int vnum ) );
char	fread_letter	args( ( FILE *fp ) );
int	fread_number	args( ( FILE *fp ) );
char *	fread_string	args( ( FILE *fp ) );
void	fread_to_eol	args( ( FILE *fp ) );
char *	fread_word	args( ( FILE *fp ) );
void *	alloc_mem	args( ( int sMem ) );
void *	alloc_perm	args( ( int sMem ) );
void	free_mem	args( ( void *pMem, int sMem ) );
char *	str_dup		args( ( const char *str ) );
void	free_string	args( ( char *pstr ) );
int	number_fuzzy	args( ( int number ) );
int	number_range	args( ( int from, int to ) );
int	number_percent	args( ( void ) );
int	number_door	args( ( void ) );
int	number_bits	args( ( int width ) );
int	number_mm	args( ( void ) );
int	dice		args( ( int number, int size ) );
int	interpolate	args( ( int level, int value_00, int value_32 ) );
void	smash_tilde	args( ( char *str ) );
bool	str_cmp		args( ( const char *astr, const char *bstr ) );
bool	str_cmp_ast	args( ( const char *astr, const char *bstr ) );
bool	str_prefix	args( ( const char *astr, const char *bstr ) );
bool	str_infix	args( ( const char *astr, const char *bstr ) );
bool	str_suffix	args( ( const char *astr, const char *bstr ) );
char *	capitalize	args( ( const char *str ) );
void	append_file	args( ( CHAR_DATA *ch, char *file, char *str ) );
void	info		args( ( const char *str, int param1, int param2, int origin ) );
void	challenge	args( ( const char *str, int param1, int param2 ) );
void	bug		args( ( const char *str, int param ) );
void    logch           args( ( char *l_str, int l_type, int lvl ) );
void	log_string	args( ( char *str, int l_type, int level ) );
void	tail_chain	args( ( void ) );
void    clone_mobile    args( ( CHAR_DATA *parent, CHAR_DATA *clone) );
void    clone_object    args( ( OBJ_DATA *parent, OBJ_DATA *clone ) );
void	parse_ban       args( ( char *argument, BAN_DATA *banned ) );
void    arena_chann	args( ( const char *str, int param1, int param2 ) );

/* fight.c */
void	violence_update	args( ( void ) );
void	item_damage	args( ( int armordam, OBJ_DATA *obj, CHAR_DATA *ch ) );
void	multi_hit	args( ( CHAR_DATA *ch, CHAR_DATA *victim, int dt ) );
void	damage		args( ( CHAR_DATA *ch, CHAR_DATA *victim, int dam,
			       int dt, int immune, bool skill ) );
void	update_pos	args( ( CHAR_DATA *victim, CHAR_DATA *ch ) );
void	stop_fighting	args( ( CHAR_DATA *ch, bool fBoth ) );
void	stop_shooting	args( ( CHAR_DATA *ch, bool fBoth ) );
void	raw_kill	args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
void	death_cry	args( ( CHAR_DATA *ch ) );
bool    is_safe         args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
bool    target_available args(( CHAR_DATA *ch, CHAR_DATA *fch, OBJ_DATA *obj ));

/* handler.c */
MONEY_DATA *base_value	args( ( OBJ_DATA *obj ) );
int	weapon_to_damage args( ( int damclass ) );
int	immune_calc	args( ( int dam_type, CHAR_DATA *ch ) );
void	damage_object	args( ( CHAR_DATA *ch, OBJ_DATA *obj, int damtype, 
                                int dam ) );
void	gas_spread	args( ( OBJ_DATA *obj ) );
void	gasaffect	args( ( OBJ_DATA *obj ) );
int	exit_blocked	args( ( EXIT_DATA *exit, ROOM_INDEX_DATA *in_room ) );
int	distancebetween args( ( CHAR_DATA *ch, CHAR_DATA *victim, int dir ) );
char   *bodypartdesc	args( ( int partnum ) );
bool    is_flying	args( ( CHAR_DATA *ch ) );
bool	assimilate_vnum_check	args( ( int location, int part ) );
void	assimilate_part args( ( CHAR_DATA *ch, OBJ_DATA *part, int location ) );
void	typo_message	args( ( CHAR_DATA *ch ) );
bool	is_wearable	args( ( OBJ_DATA *obj ) );
int	randroom	args( ( void ) );
void    polymorph_char  args( ( CHAR_DATA *ch,
                                RACE_DATA *race_o, RACE_DATA *race_n ) );
bool	outdoor_check	args( ( ROOM_INDEX_DATA *pRoom ) );
int	rsector_check	args( ( ROOM_INDEX_DATA *pRoom ) );
int	get_weather	args( ( ROOM_INDEX_DATA *pRoom ) );
int	get_clouds	args( ( ROOM_INDEX_DATA *pRoom ) );
void	weather_magic	args( ( int type, ROOM_INDEX_DATA *pRoom ) );
int	get_trust	args( ( CHAR_DATA *ch ) );
int	get_age		args( ( CHAR_DATA *ch ) );
int	get_curr_str	args( ( CHAR_DATA *ch ) );
int	get_curr_int	args( ( CHAR_DATA *ch ) );
int	get_curr_wis	args( ( CHAR_DATA *ch ) );
int	get_curr_dex	args( ( CHAR_DATA *ch ) );
int	get_curr_con	args( ( CHAR_DATA *ch ) );
int	get_curr_agi	args( ( CHAR_DATA *ch ) );
int	get_curr_cha	args( ( CHAR_DATA *ch ) );
int	can_carry_n	args( ( CHAR_DATA *ch ) );
int	can_carry_w	args( ( CHAR_DATA *ch ) );
int	xp_tolvl	args( ( CHAR_DATA *ch ) );
bool	is_name		args( (  CHAR_DATA *ch, char *str, char *namelist) );
bool    is_exact_name   args( ( char *str, char *namelist ) );
void	affect_to_char	args( ( CHAR_DATA *ch, AFFECT_DATA *paf ) );
void	affect_to_char2	args( ( CHAR_DATA *ch, AFFECT_DATA *paf ) );
void	affect_remove	args( ( CHAR_DATA *ch, AFFECT_DATA *paf ) );
void    affect_remove2  args( ( CHAR_DATA *ch, AFFECT_DATA *paf ) );
void	affect_strip	args( ( CHAR_DATA *ch, int sn ) );
bool	is_affected	args( ( CHAR_DATA *ch, int sn ) );
void	affect_join	args( ( CHAR_DATA *ch, AFFECT_DATA *paf ) );
void    affect_join2    args( ( CHAR_DATA *ch, AFFECT_DATA *paf ) );
void	char_from_room	args( ( CHAR_DATA *ch ) );
void	char_to_room	args( ( CHAR_DATA *ch, ROOM_INDEX_DATA *pRoomIndex ) );
void	obj_to_char	args( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void	tattoo_to_char	args( ( TATTOO_DATA *tattoo, CHAR_DATA *ch, bool login  ) );
void    obj_to_storage  args( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void	obj_from_char	args( ( OBJ_DATA *obj ) );
void	tattoo_from_char args(( TATTOO_DATA *tattoo, CHAR_DATA *ch ) );
void    obj_from_storage args(( OBJ_DATA *obj ) );
OD *	get_eq_char	args( ( CHAR_DATA *ch, int iWear ) );
TD *	get_tattoo_char	args( ( CHAR_DATA *ch, int iWear ) );
TD *	get_tattoo_obj	args( ( OBJ_DATA *obj, int iWear ) );
void	equip_char	args( ( CHAR_DATA *ch, OBJ_DATA *obj, bool fReplace ) );
void	unequip_char	args( ( CHAR_DATA *ch, OBJ_DATA *obj ) );

void	equip_bionic	args( ( CHAR_DATA *ch, OBJ_DATA *obj, int iWear ) );
void	unequip_bionic	args( ( CHAR_DATA *ch, OBJ_DATA *obj ) );
OBJ_DATA * get_bionic_char args(( CHAR_DATA *ch, int iWear ));
OBJ_DATA * get_bionic_wear args(( CHAR_DATA *ch, char *argument ));

int	count_obj_list	args( ( OBJ_INDEX_DATA *obj, OBJ_DATA *list ) );
void	obj_from_room	args( ( OBJ_DATA *obj ) );
void	obj_to_room	args( ( OBJ_DATA *obj, ROOM_INDEX_DATA *pRoomIndex ) );
void	bomb_to_room	args( ( OBJ_DATA *obj, ROOM_INDEX_DATA *pRoomIndex ) );
void	obj_to_obj	args( ( OBJ_DATA *obj, OBJ_DATA *obj_to ) );
void	obj_from_obj	args( ( OBJ_DATA *obj, bool sheath ) );
void	obj_to_sheath	args( ( OBJ_DATA *obj, OBJ_DATA *obj_to ) );
void	extract_tattoo	args( ( TATTOO_DATA *tattoo ) );
void	extract_obj	args( ( OBJ_DATA *obj ) );
void	extract_char	args( ( CHAR_DATA *ch, bool fPull ) );
CD *	get_char_room	args( ( CHAR_DATA *ch, char *argument ) );
CD *	get_char_world	args( ( CHAR_DATA *ch, char *argument ) );
CD *	get_char_closest args( ( CHAR_DATA *ch, int dir, char *argument ) );
CD *	get_pc_world	args( ( CHAR_DATA *ch, char *argument ) );
OD *	get_obj_type	args( ( OBJ_INDEX_DATA *pObjIndexData ) );
OD *	get_obj_list	args( ( CHAR_DATA *ch, char *argument,
			       OBJ_DATA *list ) );
OD *	get_obj_carry	args( ( CHAR_DATA *ch, char *argument ) );
OD *    get_obj_storage args( ( CHAR_DATA *ch, char *argument ) );
OD *	get_obj_wear	args( ( CHAR_DATA *ch, char *argument ) );
OD *	get_obj_here	args( ( CHAR_DATA *ch, char *argument ) );
OD *	get_obj_world	args( ( CHAR_DATA *ch, char *argument ) );

OD *  	create_money	args( ( MONEY_DATA *amount ) );

int	get_obj_number	args( ( OBJ_DATA *obj ) );
int	get_obj_weight	args( ( OBJ_DATA *obj ) );
bool	room_is_dark	args( ( ROOM_INDEX_DATA *pRoomIndex ) );
bool	room_is_private	args( ( ROOM_INDEX_DATA *pRoomIndex ) );
bool	vision_impared	args( ( CHAR_DATA *ch ) );
bool	can_see		args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
bool	can_see_thing	args( ( CHAR_DATA *ch, int type ) );
bool	can_see_obj	args( ( CHAR_DATA *ch, OBJ_DATA *obj ) );
bool	can_drop_obj	args( ( CHAR_DATA *ch, OBJ_DATA *obj ) );
char *	item_type_name	args( ( OBJ_DATA *obj ) );
char *	affect_loc_name	args( ( int location ) );
char *	affect_bit_name	args( ( int vector ) );
char * affect_bit_name2 args( ( int vector ) );
char *	extra_bit_name	args( ( int extra_flags ) );
char *  act_bit_name    args( ( int act ) );
char *  imm_bit_name    args( ( int ) );	/* XOR */
CD   *  get_char        args( ( CHAR_DATA *ch ) );
bool    longstring      args( ( CHAR_DATA *ch, char *argument ) );
void    end_of_game     args( ( void ) );
long    wiznet_lookup   args( ( const char *name) );
char *	strcpy_wo_col	args( ( char *dest, char *src ) );
int	strlen_wo_col	args( ( char *argument ) );
char *	strip_color	args( ( char *argument ) );
void    show_obj_values args( ( CHAR_DATA *ch, OBJ_DATA *obj, int imm ) );
int	get_skill_tree	args( ( char *name ) );
int	get_skill_mod	args( ( CHAR_DATA *ch, int skill ) );
int	get_skill_cost	args( ( CHAR_DATA *ch, CHAR_DATA *instructor,
				int skill ) );
int	get_total_skill_cost args( ( CHAR_DATA *ch, CHAR_DATA *instructor,
				int skill, int target_level ) );
int	parse_stree_leaf args( ( char *fulltext) );
void	advance_skill	args( ( CHAR_DATA *ch, int skill, int target_level ) );
void	advance_skill_progeny	args( ( CHAR_DATA *ch, int skill,
                                int target_level ) );
void	retroadvance_skill	args( ( CHAR_DATA *ch, int skill ) );
bool	can_advance_skill args( ( CHAR_DATA *ch, int skill ) );
void	list_skill_tree args( ( CHAR_DATA *ch, CHAR_DATA *showto,
                                int skill ) );

/* Multiclass stuff -- Hann */
bool	can_use_skpell	args( ( CHAR_DATA *ch, int sn ) );
bool	is_class	args( ( CHAR_DATA *ch, int class ) );
int     bestskillclass  args( ( CHAR_DATA *ch, int sn) );
int	prime_class	args( ( CHAR_DATA *ch ) );
int	number_classes	args( ( CHAR_DATA *ch ) );
char *	class_long	args( ( CHAR_DATA *ch ) );
char *	class_numbers	args( ( CHAR_DATA *ch, bool pSave ) );
char *	class_short	args( ( CHAR_DATA *ch ) );

/* Exit affect stuff (act_room.c) -- Hann */
void    exit_affect_to_room	args( ( EXIT_DATA *exit,
				 EXIT_AFFECT_DATA *exitaf ) );
void	exit_affect_remove	args( ( EXIT_DATA *exit,
				 EXIT_AFFECT_DATA *exitaf ) );
bool	is_exit_affected	args( ( EXIT_DATA *exit, int sn ) );
EXIT_AFFECT_DATA *get_exit_affect args( ( EXIT_DATA *exit, int sn ) );

/* Room affect stuff (act_room.c) -- Hann */
void    raffect_to_room args( ( ROOM_INDEX_DATA *room, CHAR_DATA *ch,
			ROOM_AFFECT_DATA *raf ) );
void	raffect_remove	args( ( ROOM_INDEX_DATA *room, CHAR_DATA *ch,
			ROOM_AFFECT_DATA *raf ) );
void	raffect_remall	args( ( CHAR_DATA *ch ) );
bool	is_raffected	args( ( ROOM_INDEX_DATA *room, int sn ) );
void	toggle_raffects	args( ( ROOM_INDEX_DATA *room ) );
void	loc_off_raf	args( ( CHAR_DATA *ch, int type, bool rOff ) );
/* interp.c */
void	interpret	args( ( CHAR_DATA *ch, char *argument ) );
bool	is_number	args( ( char *arg ) );
int	number_argument	args( ( char *argument, char *arg ) );
char *	one_argument	args( ( char *argument, char *arg_first ) );
bool    IS_SWITCHED     args( ( CHAR_DATA *ch ) );
void	arena_master	args( ( CHAR_DATA *ch, char *argument, char *arg2 ) );

/* magic.c */
int	sc_dam		args( ( CHAR_DATA *ch, int dam, int element ) );
int     slot_lookup     args( ( int slot ) );
bool    is_sn           args( ( int sn ) );
int	skill_lookup	args( ( const char *name ) );
bool	saves_spell	args( ( int level, CHAR_DATA *victim ) );
void	obj_cast_spell	args( ( int sn, int level, CHAR_DATA *ch,
			       CHAR_DATA *victim, OBJ_DATA *obj ) );
void    update_skpell   args( ( CHAR_DATA *ch, int sn ) );
void    do_acspell      args( ( CHAR_DATA *ch, OBJ_DATA *pObj,
			        char *argument ) );

/* save.c */
bool    pstat            args( ( char *name ) );
void	save_char_obj	args( ( CHAR_DATA *ch, bool leftgame ) );
bool	load_char_obj	args( ( DESCRIPTOR_DATA *d, char *name ) );
void    corpse_back     args( ( CHAR_DATA *ch, OBJ_DATA *corpse ) );
void    read_finger	args( ( CHAR_DATA *ch, char *argument ) );
void    save_finger     args( ( CHAR_DATA *ch ) );
void    fwrite_finger   args( ( CHAR_DATA *ch, FILE *fp ) );
void    fread_finger    args( ( CHAR_DATA *ch, FILE *fp ) );
void	save_banlist	args( ( BAN_DATA *ban_list ) );

/* special.c */
SF *	spec_lookup	args( ( const char *name ) );

/* update.c */
void	advance_level	args( ( CHAR_DATA *ch ) );
void	gain_exp	args( ( CHAR_DATA *ch, int gain ) );
void	gain_condition	args( ( CHAR_DATA *ch, int iCond, int value ) );
void	update_handler	args( ( void ) );
void	weather_update	args( ( AREA_DATA *pArea ) );
CHAR_DATA *rand_figment args( ( CHAR_DATA *ch ) );
MOB_INDEX_DATA *rand_figment_mob args( ( CHAR_DATA *ch ) );
OBJ_INDEX_DATA *rand_figment_obj args( ( CHAR_DATA *ch ) );

/* mob_prog.c */
#ifdef DUNNO_STRSTR
char *  strstr                  args ( (const char *s1, const char *s2 ) );
#endif
void    mprog_wordlist_check    args ( ( char * arg, CHAR_DATA *mob,
                			CHAR_DATA* actor, OBJ_DATA* object,
					void* vo, int type ) );
void    mprog_percent_check     args ( ( CHAR_DATA *mob, CHAR_DATA* actor,
					OBJ_DATA* object, void* vo,
					int type ) );
void    mprog_act_trigger       args ( ( char* buf, CHAR_DATA* mob,
		                        CHAR_DATA* ch, OBJ_DATA* obj,
					void* vo ) );
void	mprog_bribe_trigger	args ( ( CHAR_DATA* mob, CHAR_DATA* ch,
					MONEY_DATA *amount ) );
void    mprog_entry_trigger     args ( ( CHAR_DATA* mob ) );
void    mprog_give_trigger      args ( ( CHAR_DATA* mob, CHAR_DATA* ch,
                		        OBJ_DATA* obj ) );
void    mprog_greet_trigger     args ( ( CHAR_DATA* mob ) );
void    mprog_fight_trigger     args ( ( CHAR_DATA* mob, CHAR_DATA* ch ) );
void    mprog_hitprcnt_trigger  args ( ( CHAR_DATA* mob, CHAR_DATA* ch ) );
void    mprog_death_trigger     args ( ( CHAR_DATA* mob, CHAR_DATA* ch ) );
void    mprog_random_trigger    args ( ( CHAR_DATA* mob ) );
void    mprog_random_victim_trigger args ( ( CHAR_DATA* mob ) );
void    mprog_speech_trigger    args ( ( char* txt, CHAR_DATA* mob ) );
void	mprog_spell_trigger	args ( ( CHAR_DATA *ch, int skill ) );

/*
 * Lotsa triggers for ore_progs.. (ore_prog.c)
 * -- Altrag
 */
/*
 * Object triggers
 */
void    oprog_get_trigger       args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_get_from_trigger  args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 OBJ_DATA *secondary ) );
void    oprog_give_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 CHAR_DATA *victim ) );
void    oprog_drop_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_put_trigger       args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 OBJ_DATA *secondary ) );
void    oprog_fill_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 OBJ_DATA *spring ) );
void    oprog_wear_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_look_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_look_in_trigger   args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_invoke_trigger    args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 void *vo ) );
void    oprog_use_trigger       args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 void *vo ) );
void    oprog_cast_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_cast_sn_trigger   args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 int sn, void *vo ) );
void    oprog_join_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 OBJ_DATA *secondary ) );
void    oprog_separate_trigger  args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_buy_trigger       args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 CHAR_DATA *vendor ) );
void    oprog_sell_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 CHAR_DATA *vendor ) );
void    oprog_store_trigger     args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_retrieve_trigger  args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_open_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_close_trigger     args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_lock_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 OBJ_DATA *key ) );
void    oprog_unlock_trigger    args ( ( OBJ_DATA *obj, CHAR_DATA *ch,
					 OBJ_DATA *key ) );
void    oprog_pick_trigger      args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_random_trigger    args ( ( OBJ_DATA *obj ) );
void    oprog_throw_trigger     args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );
void    oprog_remove_trigger     args ( ( OBJ_DATA *obj, CHAR_DATA *ch ) );

/*
 * Room triggers
 */
void    rprog_enter_trigger     args ( ( ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    rprog_exit_trigger      args ( ( ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    rprog_pass_trigger      args ( ( ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    rprog_cast_trigger      args ( ( ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    rprog_cast_sn_trigger   args ( ( ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch, int sn,
					 void *vo ) );
void    rprog_sleep_trigger     args ( ( ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    rprog_wake_trigger      args ( ( ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    rprog_rest_trigger      args ( ( ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    rprog_death_trigger     args ( ( ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    rprog_time_trigger      args ( ( ROOM_INDEX_DATA *room, int hour ) );
void    rprog_random_trigger    args ( ( ROOM_INDEX_DATA *room ) );

/*
 * Exit triggers
 */
void    eprog_enter_trigger     args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    eprog_exit_trigger      args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    eprog_pass_trigger      args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch, bool fEnter ) );
void    eprog_look_trigger      args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    eprog_scry_trigger      args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    eprog_open_trigger      args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    eprog_close_trigger     args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );
void    eprog_lock_trigger      args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch, OBJ_DATA *obj ) );
void    eprog_unlock_trigger    args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch, OBJ_DATA *obj ) );
void    eprog_pick_trigger      args ( ( EXIT_DATA *pExit,
					 ROOM_INDEX_DATA *room,
					 CHAR_DATA *ch ) );

/*
 * gr_magic.c
 * -- Altrag
 */
void    check_gcast             args ( ( CHAR_DATA *ch ) );
void    group_cast              args ( ( int sn, int level, CHAR_DATA *ch,
					 char *argument ) );
void    set_gspell              args ( ( CHAR_DATA *ch, GSPELL_DATA *gsp ) );
void    end_gspell              args ( ( CHAR_DATA *ch ) );

/*
 * track.c
 */
void    hunt_victim             args ( ( CHAR_DATA *ch ) );
bool    can_go                  args ( ( CHAR_DATA *ch, int dir ) );
int     find_first_step         args ( (ROOM_INDEX_DATA *src, ROOM_INDEX_DATA *target) );

/*
 * chatmode.c
 */
void    start_chat_mode         args ( ( DESCRIPTOR_DATA *d ) );
void    chat_interp             args ( ( CHAR_DATA *ch, char *argument ) );

#undef	CD
#undef	MID
#undef	OD
#undef	TD
#undef	OID
#undef	RID
#undef	SF
#undef  BD

/*****************************************************************************
 *                                    OLC                                    *
 *****************************************************************************/

/*
 * This structure is used in special.c to lookup spec funcs and
 * also in olc_act.c to display listings of spec funcs.
 */
struct spec_type
{
    char *	spec_name;
    SPEC_FUN *	spec_fun;
};



/*
 * This structure is used in bit.c to lookup flags and stats.
 */
struct flag_type
{
    char * name;
    int  bit;
    bool settable;
};



/*
 * Object defined in limbo.are
 * Used in save.c to load objects that don't exist.
 */
#define OBJ_VNUM_DUMMY	1



/*
 * Area flags.
 */
#define         AREA_NONE       0
#define         AREA_CHANGED    1	/* Area has been modified. */
#define         AREA_ADDED      2	/* Area has been added to. */
#define         AREA_LOADING    4	/* Used for counting in db.c */
#define		AREA_VERBOSE	8	/* Used for saving in save.c */
#define	 	AREA_PROTOTYPE 16       /* Prototype area(no mortals) */
#define		AREA_CLAN_HQ   32	/* Area is a CLAN HQ */
#define 	AREA_MUDSCHOOL 128	/* Used for mudschool only */

#define MAX_DIR	6
#define NO_FLAG -99	/* Must not be used in flags or stats. */



/*
 * Interp.c
 */
DECLARE_DO_FUN( do_aedit        );	/* OLC 1.1b */
DECLARE_DO_FUN( do_redit        );	/* OLC 1.1b */
DECLARE_DO_FUN( do_oedit        );	/* OLC 1.1b */
DECLARE_DO_FUN( do_qedit       );	/* Flux */
DECLARE_DO_FUN( do_medit        );	/* OLC 1.1b */
DECLARE_DO_FUN( do_cedit        );      /* IchiCode 1.1b */
DECLARE_DO_FUN( do_hedit        );      /* XOR 3.14159265359r^2 */
DECLARE_DO_FUN( do_sedit	);	/* Decklarean */
DECLARE_DO_FUN( do_spedit	);	/* Decklarean */
DECLARE_DO_FUN( do_rename_obj	);	/* Decklarean */
DECLARE_DO_FUN( do_race_edit	);	/* Decklarean */
DECLARE_DO_FUN( do_mreset	);	/* Decklarean */
DECLARE_DO_FUN( do_nedit	);	/* Angi */
DECLARE_DO_FUN( do_asave	);
DECLARE_DO_FUN( do_alist	);
DECLARE_DO_FUN( do_resets	);
DECLARE_DO_FUN( do_alias        );
DECLARE_DO_FUN( do_flush        );	/* Interp queue */
DECLARE_DO_FUN( do_stop         );	/*       ||     */
DECLARE_DO_FUN( do_view         );	/*       \/     */
DECLARE_DO_FUN( do_clear        );      /* Angi */




/*
 * Global Constants
 */
extern	char *	const	dir_name        [];
extern	const	int	rev_dir         [];
extern	const	struct	spec_type	spec_table	[];



/*
 * Global variables
 */
extern          AREA_DATA *             area_first;
extern          AREA_DATA *             area_last;
extern          CLAN_DATA *             clan_first;
extern  	SHOP_DATA *             shop_last;
extern  	CASINO_DATA *           casino_last;
extern  	TATTOO_ARTIST_DATA *    tattoo_artist_last;

extern          int                     top_affect;
extern          int                     top_area;
extern          int                     top_ed;
extern          int                     top_exit;
extern          int                     top_help;
extern          int                     top_mob_index;
extern		int			mobs_in_game;
extern          int                     top_obj_index;
extern          int                     top_reset;
extern          int                     top_room;
extern          int                     top_shop;
extern          int                     top_casino;
extern          int                     top_tattoo_artist;
extern          int                     top_clan;
extern		int			top_social;
extern		int			top_race;
extern		int			top_quest;
extern		int			top_trap;
extern		int			top_mob_prog;

extern          char                    str_empty       [1];

extern  MOB_INDEX_DATA *        mob_index_hash  [MAX_KEY_HASH];
extern  OBJ_INDEX_DATA *        obj_index_hash  [MAX_KEY_HASH];
extern  ROOM_INDEX_DATA *       room_index_hash [MAX_KEY_HASH];


/* db.c */
void	reset_area      args( ( AREA_DATA * pArea ) );
void	reset_room	args( ( ROOM_INDEX_DATA *pRoom ) );

/* string.c */
void	string_edit	args( ( CHAR_DATA *ch, char **pString ) );
void    string_append   args( ( CHAR_DATA *ch, char **pString ) );
char *	string_replace	args( ( char * orig, char * old, char * new ) );
void    string_add      args( ( CHAR_DATA *ch, char *argument ) );
char *  format_string   args( ( char *oldstring /*, bool fSpace */ ) );
char *  first_arg       args( ( char *argument, char *arg_first, bool fCase ) );
char *	string_unpad	args( ( char * argument ) );
char *	string_proper	args( ( char * argument ) );
char *	all_capitalize	args( ( const char *str ) );	/* OLC 1.1b */
char *  string_delline  args( ( CHAR_DATA *ch, char *argument, char *old ) );
char * string_insline   args( ( CHAR_DATA *ch, char *argument, char *old ) );
/* olc.c */
bool	run_olc_editor	args( ( DESCRIPTOR_DATA *d ) );
char	*olc_ed_name	args( ( CHAR_DATA *ch ) );
char	*olc_ed_vnum	args( ( CHAR_DATA *ch ) );
AREA_DATA  *get_area_data       args( ( int vnum ) );
void 	purge_area	args( ( AREA_DATA * pArea ) ); /* Angi */

/* special.c */
char *	spec_string	args( ( SPEC_FUN *fun ) );	/* OLC */

/* bit.c */
extern const struct flag_type fighting_styles[];
extern const struct flag_type gun_type_flags[];
extern const struct flag_type ranged_weapon_flags[];
extern const struct flag_type race_type_flags[];
extern const struct flag_type area_flags[];
extern const struct flag_type attitude_flags[];
extern const struct flag_type	sex_flags[];
extern const struct flag_type	exit_flags[];
extern const struct flag_type	direction_flags[];
extern const struct flag_type	door_action_flags[];
extern const struct flag_type	activation_commands[];
extern const struct flag_type	switch_affect_types[];
extern const struct flag_type	door_resets[];
extern const struct flag_type	room_flags[];
extern const struct flag_type	sector_flags[];
extern const struct flag_type	type_flags[];
extern const struct flag_type	extra_flags[];
extern const struct flag_type	wear_flags[];
extern const struct flag_type   bionic_flags[];
extern const struct flag_type   bionic_loc_strings[];
extern const struct flag_type	act_flags[];
extern const struct flag_type	mpaffect_flags[];
extern const struct flag_type	affect_flags[];
extern const struct flag_type	affect2_flags[];
extern const struct flag_type	apply_flags[];
extern const struct flag_type	wear_loc_strings[];
extern const struct flag_type	wear_loc_flags[];
extern const struct flag_type	language_types[];
extern const struct flag_type   morph_armor[];
extern const struct flag_type   morph_weapons[];
extern const struct flag_type   morph_types[];
extern const struct flag_type   assimilate_loc[];
extern const struct flag_type   weaponclass_flags[];
extern const struct flag_type   blade_flags[];
extern const struct flag_type	furniture_flags[];
extern const struct flag_type	weapon_flags[];
extern const struct flag_type	warhead_flags[];
extern const struct flag_type	propulsion_flags[];
extern const struct flag_type	card_suits[];
extern const struct flag_type	card_faces[];
extern const struct flag_type	casino_games[];
extern const struct flag_type	weather_flags[];
extern const struct flag_type	area_weather_flags[];
extern const struct flag_type	sky_flags[];
extern const struct flag_type	temporal_flags[];
extern const struct flag_type	tattoo_flags[];
extern const struct flag_type	wear_tattoo[];
extern const struct flag_type	object_materials[];
extern const struct flag_type	arrow_types[];
extern const struct flag_type	armor_types[];
extern const struct flag_type	container_flags[];
extern const struct flag_type	liquid_flags[];
extern const struct flag_type	food_condition[];
extern const struct flag_type	gas_affects[];
extern const struct flag_type	claw_flags[];
extern const struct flag_type	size_flags[];
extern const struct flag_type	mobhp_flags[];
extern const struct flag_type	skin_flags[];
extern const struct flag_type   damage_flags[];
extern const struct flag_type   immune_flags[];
extern const struct flag_type   mprog_types[];
extern const struct flag_type   oprog_types[];
extern const struct flag_type   rprog_types[];
extern const struct flag_type   eprog_types[];

/* olc_act.c */
extern int flag_value       args ( ( const struct flag_type *flag_table,
				     char *argument ) );
extern AFFECT_DATA  *new_affect   args ( ( void ) );    
extern OBJ_INDEX_DATA    *new_obj_index          args ( ( void ) );
extern TATTOO_DATA    *new_tattoo          args ( ( void ) );
extern void   free_tattoo          args ( ( TATTOO_DATA *tattoo) );
extern void free_affect		  args ( ( AFFECT_DATA *pAf ) );
extern EXTRA_DESCR_DATA *new_extra_descr args( ( void ) );
extern void free_extra_descr	  args ( ( EXTRA_DESCR_DATA *pExtra ) );
extern void free_obj_data	  args ( ( OBJ_DATA *pObj ) );
extern OBJ_DATA *new_obj_data	  args ( ( void ) );

extern RESET_DATA *new_reset_data args ( ( void ) ) ;

extern void check_nofloor         args ( ( CHAR_DATA *ch ) );
extern char *flag_string     args ( ( const struct flag_type *flag_table,
 				      int bits ) );
extern void save_clans            args ( ( ) );
extern void save_social 	  args ( ( ) );
extern void save_race	 	  args ( ( ) );
extern RACE_DATA *new_race_data	  args ( ( void ) );
extern void race_sort		  args ( ( RACE_DATA *pArea ) );
extern RACE_DATA *get_race_data	args ( ( int vnum ) );
extern void save_quests	 	  args ( ( ) );
extern QUEST_DATA *new_quest_data  args ( ( void ) );
extern void quest_sort		  args ( ( QUEST_DATA *pArea ) );
extern QUEST_DATA *get_quest_data args ( ( int vnum ) );
extern void save_helps		  args ( ( ) );
extern void send_to_area          args ( ( AREA_DATA *pArea, char *txt ) );
extern ALIAS_DATA *new_alias      args ( ( ) );
extern void        free_alias     args ( ( ALIAS_DATA *pAl ) );
extern Q_DATA	   *new_queue      args ( ( ) );
extern void        free_queue     args ( ( Q_DATA *pQueue ) );

extern void save_player_list	  args ( ( ) ); /*Decklarean */
