/*
 * This is my version of the language coding. If you want an explanation,
 * e-mail me. -Flux.
 */

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

struct syl_type
{
 char *old;
 char *new;
};


const struct syl_type *translate_human args( ( void ) );
const struct syl_type *translate_elf args( ( void ) );
const struct syl_type *translate_dwarf args( ( void ) );
const struct syl_type *translate_quicksilver args( ( void ) );
const struct syl_type *translate_maudlin args( ( void ) );
const struct syl_type *translate_pixie args( ( void ) );
const struct syl_type *translate_felixi args( ( void ) );
const struct syl_type *translate_draconi args( ( void ) );
const struct syl_type *translate_gremlin args( ( void ) );
const struct syl_type *translate_centaur args( ( void ) );
const struct syl_type *translate_kender args( ( void ) );
const struct syl_type *translate_minotaur args( ( void ) );
const struct syl_type *translate_drow args( ( void ) );
const struct syl_type *translate_aquinis args( ( void ) );
const struct syl_type *translate_troll args( ( void ) );



char* translate( CHAR_DATA *ch, char *argument )
{
	char       *nbuf;
	static char buf[ 512 ];
	int        iSyl;
	int        length;

   static const struct syl_type *syl_table;

   switch( ch->pcdata->speaking )
   {
    case LANGUAGE_HUMAN:
     syl_table = translate_human(); break;
    case LANGUAGE_ELF:
     syl_table = translate_elf(); break;
    case LANGUAGE_DWARF:
     syl_table = translate_dwarf(); break;
    case LANGUAGE_QUICKSILVER:
     syl_table = translate_quicksilver(); break;
    case LANGUAGE_MAUDLIN:
     syl_table = translate_maudlin(); break;
    case LANGUAGE_PIXIE:
     syl_table = translate_pixie(); break;
    case LANGUAGE_FELIXI:
     syl_table = translate_felixi(); break;
    case LANGUAGE_DRACONI:
     syl_table = translate_draconi(); break;
    case LANGUAGE_GREMLIN:
     syl_table = translate_gremlin(); break;
    case LANGUAGE_CENTAUR:
     syl_table = translate_centaur(); break;
    case LANGUAGE_KENDER:
     syl_table = translate_kender(); break;
    case LANGUAGE_MINOTAUR:
     syl_table = translate_minotaur(); break;
    case LANGUAGE_DROW:
     syl_table = translate_drow(); break;
    case LANGUAGE_AQUINIS:
     syl_table = translate_aquinis(); break;
    case LANGUAGE_TROLL:
     syl_table = translate_troll(); break;

   }

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

 return buf;
}

void do_instruct( CHAR_DATA *ch, char *argument )
{
 CHAR_DATA	*victim;
 char		arg[MAX_INPUT_LENGTH];
 char		arg2[MAX_INPUT_LENGTH];
 int		language;

 if ( IS_NPC( ch ) )
 {
  typo_message( ch );
  return;
 }

    argument = one_argument( argument, arg );
    argument = one_argument( argument, arg2 );

 if ( arg[0] == '\0' || arg2[0] == '\0' )
 {
  send_to_char( AT_WHITE, "Syntax: instruct <victim> <language>\n\r", ch );
  return;
 }
 
 if ( !(victim = get_char_world( ch, arg ) ) )
 {
  send_to_char( AT_WHITE, "That person is not here.\n\r", ch );
  return;
 }

 if ( victim->in_room != ch->in_room )
 {
  send_to_char( AT_WHITE, "That person is not here.\n\r", ch );
  return;
 }

 if ( IS_NPC( victim ) )
 {
  send_to_char( AT_WHITE, "You can't instruct NPC's.\n\r", ch );
  return;
 }

 if ( ch == victim )
 {
  send_to_char( AT_WHITE, "No self teaching.\n\r", ch );
  return;
 }

 language = flag_value( language_types, arg2 );

 if ( language <= LANGUAGE_NONE || language >= MAX_LANGUAGE )
 {
  send_to_char( AT_WHITE, "That is not a language, check your 'score lang' for the list.\n\r", ch );
  return;
 }

 if ( ch->pcdata->language[language] != 100 )
 {
  send_to_char( AT_WHITE, "You aren't completely fluent in that language yourself.\n\r", ch );
  return;
 }

 if ( victim->pcdata->language[language] == 100 )
 {
  send_to_char( AT_WHITE, "They have no need for your instruction.\n\r", ch );
  return;
 }

 act( AT_WHITE, "You instruct $N on the finer points of the language.",
	ch, victim, NULL, TO_CHAR );
 act( AT_WHITE, "$n instructs you on the finer points of the language.",
	ch, victim, NULL, TO_VICT );
 act( AT_WHITE, "$n instructs $N on the finer points of the language.",
	ch, victim, NULL, TO_ROOM );

 victim->pcdata->language[language] += dice( 3, 15 );
 if ( victim->pcdata->language[language] > 100 )
  victim->pcdata->language[language] = 100;

 return;
}

void do_speak( CHAR_DATA *ch, char *argument )
{
 char buf[MAX_STRING_LENGTH];
 char arg[MAX_INPUT_LENGTH];
 int  language = 0;

 if ( IS_NPC( ch ) )
 {
  typo_message( ch );
  return;
 }

 argument = one_argument( argument, arg );

 if ( arg[0] == '\0' )
 {
  send_to_char( AT_WHITE, "You need to specify what language you wish to be speaking.\n\r", ch );
  return;
 }

 language = flag_value( language_types, arg );

 if ( language >= MAX_LANGUAGE || language <= LANGUAGE_NONE )
 {
  send_to_char( AT_WHITE, "That isn't a language.\n\r", ch );
  return;
 }

 if ( ch->pcdata->language[language] < 100 )
 {
  send_to_char( AT_WHITE, "You must be 100 percent efficient in a language to speak it.\n\r", ch );
  return;
 }

  ch->pcdata->speaking = language;
  sprintf( buf, "You twist your tounge and begin speaking %s.\n\r",
	flag_string( language_types, language ) );
  send_to_char( AT_WHITE, buf, ch );

return;
}

const struct syl_type *translate_human( void )
{
     static const struct syl_type syl_table [ ] =
     {
	{ " ", " "	},
	{ "a", "a"	},
	{ "b", "b"	}, 
	{ "c", "c"	}, 
	{ "d", "d"	},
	{ "e", "e"	}, 
	{ "f", "f"	}, 
	{ "g", "g"	}, 
	{ "h", "h"	},
	{ "i", "i"	}, 
	{ "j", "j"	}, 
	{ "k", "k"	}, 
	{ "l", "l"	},
	{ "m", "m"	}, 
	{ "n", "n"	}, 
	{ "o", "o"	}, 
	{ "p", "p"	},
	{ "q", "q"	}, 
	{ "r", "r"	}, 
	{ "s", "s"	}, 
	{ "t", "t"	},
	{ "u", "u"	}, 
	{ "v", "v"	}, 
	{ "w", "w"	}, 
	{ "x", "x"	},
	{ "y", "y"	}, 
	{ "z", "z"	},
	{ "", ""	}
     };

  return syl_table;
}

const struct syl_type *translate_elf( void )
{

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

 return syl_table;
}

const struct syl_type *translate_dwarf( void )
{

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

 return syl_table;
}

const struct syl_type *translate_quicksilver( void )
{

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

 return syl_table;
}

const struct syl_type *translate_maudlin( void )
{

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

 return syl_table;
}

const struct syl_type *translate_pixie( void )
{

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

 return syl_table;
}

const struct syl_type *translate_felixi( void )
{

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

 return syl_table;
}

const struct syl_type *translate_draconi( void )
{

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

 return syl_table;
}

const struct syl_type *translate_gremlin( void )
{

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

 return syl_table;
}

const struct syl_type *translate_centaur( void )
{

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

 return syl_table;
}

const struct syl_type *translate_kender( void )
{

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

 return syl_table;
}

const struct syl_type *translate_minotaur( void )
{

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

 return syl_table;
}

const struct syl_type *translate_drow( void )
{

     static const struct syl_type   syl_table [ ] =
     {
	{ " ", " "	},
	{ "a", "natha" },
	{ "about", "bauth" },
	{ "accomplish", "xun" },
	{ "achievement", "xundus" },
	{ "agreement", "inthigg" },
	{ "aim", "ilindith" },
	{ "al", "ol" },
	{ "alert", "kyone" },
	{ "alive", "dro" },
	{ "all", "jal" },
	{ "ally", "abban" },
	{ "alone", "maglust" },
	{ "and", "lueth" },
	{ "apart", "maglust" },
	{ "are", "phuul" },
	{ "argument", "qua'laelay" },
	{ "around", "bauth" },
	{ "as", "izil" },
	{ "authority", "quarth" },
	{ "b", "b" },
	{ "back", "rath" },
	{ "backs", "ratha" },
	{ "band", "akh" },
	{ "barrier", "kluggen" },
	{ "battlemight", "sargh" },
	{ "be", "tlu" },
	{ "behind", "rathrae" },
	{ "below", "harl" },
	{ "best", "alurl" },
	{ "beware", "sarn" },
	{ "black", "vel'oloth" },
	{ "blade", "velve" },
	{ "blockage", "klug" },
	{ "both", "tuth" },
	{ "bravery", "honglath" },
	{ "brightness", "ssussun" },
	{ "c", "q" },
	{ "calm", "honglath" },
	{ "careful", "kyone" },
	{ "carrion", "iblith" },
	{ "caution", "olist" },
	{ "climb", "z'orr" },
	{ "coinage", "belaern" },
	{ "coinage", "belaern" },
	{ "commanded", "quarthen" },
	{ "comrade", "abbil" },
	{ "concealment", "veldrin" },
	{ "confidence", "sargh" },
	{ "confrontation", "qua'laelay" },
	{ "conquering", "ultrinnan" },
	{ "conqueror", "ultrin" },
	{ "consider", "talinth" },
	{ "continue", "elendar" },
	{ "continued", "elendar" },
	{ "continuing", "elendar" },
	{ "council", "talthalra" },
	{ "creature", "phindar" },
	{ "d", "e" },
	{ "danger", "sreen" },
	{ "darkness", "oloth" },
	{ "death", "elghinn" },
	{ "destiny", "elamshin" },
	{ "destiny", "ul-Ilindith" },
	{ "destroy", "elgg" },
	{ "disagreement", "qua'laelay" },
	{ "discover", "ragar" },
	{ "do", "xun" },
	{ "dodge", "bautha" },
	{ "doing", "xundus" },
	{ "dominance", "z'ress" },
	{ "down", "harl" },
	{ "e", "z" },
	{ "elves", "darthiir" },
	{ "encounter", "thalra" },
	{ "enduring", "elendar" },
	{ "enemy", "ogglin" },
	{ "er", "uk" },
	{ "es", "ir" },
	{ "ess", "th" },
	{ "excrement", "iblith" },
	{ "expedition", "z'hind" },
	{ "f", "y" },
	{ "facing", "alust" },
	{ "faeries", "darthiir" },
	{ "fearlessness", "streeaka" },
	{ "find", "ragar" },
	{ "first", "ust" },
	{ "food", "cahallin" },
	{ "fool", "wael" },
	{ "foolish", "waela" },
	{ "force", "z'ress" },
	{ "foremost", "alurl" },
	{ "from", "dal" },
	{ "fun", "jivvin" },
	{ "g", "o" },
	{ "gift", "belbol" },
	{ "give", "belbau" },
	{ "goal", "ilindith" },
	{ "goblin", "gol" },
	{ "goddess", "quar'valsharess" },
	{ "grave", "phalar" },
	{ "group", "akh" },
	{ "guard", "kyrol" },
	{ "guarding", "kyorlin" },
	{ "guide", "mrimm" },
	{ "h", "p" },
	{ "hand", "kyonss" },
	{ "her", "usstils" },
	{ "hidden", "velkyn" },
	{ "highest", "ultrin" },
	{ "highpriestess", "yathtallar" },
	{ "him", "usstil" },
	{ "holy", "orthae" },
	{ "honored", "malla" },
	{ "house", "qu'ellar" },
	{ "human", "rivvil" },
	{ "humans", "rivvin" },
	{ "i", "u" },
	{ "illithid", "haszak" },
	{ "illithids", "haszakkin" },
	{ "in", "wun" },
	{ "ing", "in" },
	{ "inspiration", "mrimm" },
	{ "invisible", "velkyn" },
	{ "ion", "mm" },
	{ "is", "zhah" },
	{ "item", "bol" },
	{ "j", "y" },
	{ "journey", "z'hind" },
	{ "k", "t" },
	{ "kill", "ellg" },
	{ "knowledge", "zhaunil" },
	{ "l", "r" },
	{ "learning", "zhaunil" },
	{ "life", "dro" },
	{ "light", "ssussun" },
	{ "lloth", "quarvalsharess" },
	{ "longing", "ssinssrigg" },
	{ "lost", "noamuth" },
	{ "m", "w" },
	{ "magic", "faer" },
	{ "magical", "faerl" },
	{ "master", "jabbuk" },
	{ "matron", "ilharess" },
	{ "matrons", "ilharessen" },
	{ "may", "xal" },
	{ "meet", "thalra" },
	{ "meeting", "talthalra" },
	{ "ment", "lay" },
	{ "might", "xal" },
	{ "monster", "phindar" },
	{ "more", "mzild" },
	{ "mother", "ilhar" },
	{ "n", "i" },
	{ "no", "nau" },
	{ "o", "a" },
	{ "of", "del" },
	{ "on", "pholor" },
	{ "one", "uss" },
	{ "opponent", "ogglin" },
	{ "order", "quarth" },
	{ "ordered", "quarthen" },
	{ "out", "doeb" },
	{ "p", "s" },
	{ "parley", "talthalra" },
	{ "passion", "ssinssrigg" },
	{ "path", "colbauth" },
	{ "patron", "ilharn" },
	{ "perhaps", "xal" },
	{ "play", "jivvin" },
	{ "power", "z'ress" },
	{ "priestess", "yathrin" },
	{ "q", "d" },
	{ "queen", "valsharess" },
	{ "r", "f" },
	{ "raid", "tha-lackz'hind" },
	{ "rampart", "kluggen" },
	{ "recklessness", "streeaka" },
	{ "ride", "z'har" },
	{ "rival", "ogglin" },
	{ "ruse", "golhyrr" },
	{ "s", "g" },
	{ "sacred", "orthae" },
	{ "scheme", "inth" },
	{ "second", "drada" },
	{ "shadows", "veldrin" },
	{ "shield", "kluggen" },
	{ "shit", "iblith" },
	{ "sieze", "plynn" },
	{ "slay", "ellg" },
	{ "spider", "orbb" },
	{ "stealth", "olist" },
	{ "stratagem", "inth" },
	{ "strength", "z'ress" },
	{ "striving", "xund" },
	{ "superior", "alur" },
	{ "supreme", "ultrin" },
	{ "surprise", "brorn" },
	{ "surprises", "brorna" },
	{ "t", "h" },
	{ "take", "plynn" },
	{ "temple", "yath" },
	{ "th", "nn" },
	{ "than", "taga" },
	{ "them", "usstina" },
	{ "they", "usstin" },
	{ "thing", "bol" },
	{ "think", "talinth" },
	{ "third", "llarnbuss" },
	{ "those", "nind" },
	{ "three", "llar" },
	{ "to", "uss" },
	{ "traditional", "elend" },
	{ "traitors", "darthiir" },
	{ "treasure", "belaern" },
	{ "treaty", "inthigg" },
	{ "trick", "golhyrr" },
	{ "trip", "z'hind" },
	{ "trust", "khaless" },
	{ "two", "draa" },
	{ "u", "j" },
	{ "unaware", "waela" },
	{ "uncover", "ragar" },
	{ "under", "harl" },
	{ "unknown", "noamuth" },
	{ "unseen", "velkyn" },
	{ "unwary", "waela" },
	{ "upon", "pholor" },
	{ "us", "usstens" },
	{ "v", "z" },
	{ "valor", "sargh" },
	{ "w", "x" },
	{ "wait", "kyrol" },
	{ "waiting", "kyorlin" },
	{ "walk", "z'hin" },
	{ "wanderer", "noamuth" },
	{ "war", "thalack" },
	{ "warning", "sarn" },
	{ "warrior", "sargtlin" },
	{ "wary", "kyone" },
	{ "watch", "kyrol" },
	{ "watching", "kyorlin" },
	{ "wealth", "belaern" },
	{ "wisdom", "zhaunil" },
	{ "wizard", "faern" },
	{ "x", "n" },
	{ "y", "l" },
	{ "you", "dos" },
	{ "yours", "dosst" },
	{ "yourself", "dosstan" },
	{ "z", "k" },
	{ "", ""	}
     };

 return syl_table;
}

const struct syl_type *translate_aquinis( void )
{

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

 return syl_table;
}

const struct syl_type *translate_troll( void )
{

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

 return syl_table;
}
