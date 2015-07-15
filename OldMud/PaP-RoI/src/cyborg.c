/*****************************************************
                                     FuBaR 1.3.1X
alpha version cyborg code
by: kjodo
ok this is a hack of wear locations, i just added a new
field to obj_data much like wear_loc, called bionic_loc
cyborgs will be the only class to utilize this
*****************************************************/

#define unix 1
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

/* locals */
bool remove_bionic args(( CHAR_DATA *ch, int iWear, bool fReplace ));
void wear_bionic   args(( CHAR_DATA *ch, OBJ_DATA *obj, bool fReplace ));
void do_addbionic  args(( CHAR_DATA *ch, char *argument ));
void do_rembionic  args(( CHAR_DATA *ch, char *argument ));
void affect_modify args(( CHAR_DATA *ch, AFFECT_DATA *paf, bool fAdd ));

bool remove_bionic( CHAR_DATA *ch, int iWear, bool fReplace )
{
    OBJ_DATA *obj;

    if ( !( obj = get_bionic_char( ch, iWear ) ) )
	return TRUE;

    if ( !fReplace )
	return FALSE;

    act(AT_WHITE, "$n stops using $p.", ch, obj, NULL, TO_ROOM );
    act(AT_WHITE, "You stop using $p.", ch, obj, NULL, TO_CHAR );

    unequip_bionic( ch, obj );

    return TRUE;
}

/*
 * Wear one object.
 * Optional replacement of existing objects.
 * Big repetitive code, ick.
 */
void wear_bionic( CHAR_DATA *ch, OBJ_DATA *obj, bool fReplace )
{
    char buf [ MAX_STRING_LENGTH ];

    if ( ch->level < obj->level )
    {
	sprintf( buf, "You cannot implant this until level %d.\n\r",
	    obj->level );
	send_to_char(AT_BLUE, buf, ch );
	act(AT_BLUE, "$n tries to implant $p, but is too inexperienced.",
	    ch, obj, NULL, TO_ROOM );
	return;
    }

    if ( CAN_ADDBIONIC( obj, ITEM_BIONIC_EYE ) )
    {
	if ( get_bionic_char( ch, BIONIC_EYE_L )
	&&   get_bionic_char( ch, BIONIC_EYE_R )
	&&   !remove_bionic( ch, BIONIC_EYE_L, fReplace )
	&&   !remove_bionic( ch, BIONIC_EYE_R, fReplace ) )
	    return;

	if ( !get_bionic_char( ch, BIONIC_EYE_L ) )
	{
	    act(AT_BLUE, "A $p is implanted in your left eye socket.",
	     ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted in $s left eye socket.",
	     ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_EYE_L );
	    return;
	}

	if ( !get_bionic_char( ch, BIONIC_EYE_R ) )
	{
	    act(AT_BLUE, "A $p is implanted in your right eye socket.",
	     ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted in $s right eye socket.",
	     ch, obj, NULL, TO_ROOM );
	    equip_char( ch, obj, BIONIC_EYE_R );
	    return;
	}

	bug( "Wear_bionic: no free eye socket.", 0 );
	send_to_char(AT_BLUE, "You already have two bionic eyes.\n\r", ch );
	return;
    }


    if ( CAN_ADDBIONIC( obj, ITEM_BIONIC_BODY ) )
    {
	if ( !remove_bionic( ch, BIONIC_BODY, fReplace ) )
	    return;
	act(AT_BLUE, "A $p is implanted as your body.", ch, obj, NULL, TO_CHAR );
	act(AT_BLUE, "$n gets $p implanted as $s body.", ch, obj, NULL, TO_ROOM );
	equip_bionic( ch, obj, BIONIC_BODY );
	return;
    }

    if ( CAN_ADDBIONIC( obj, ITEM_BIONIC_ARM ) )
    {
	if ( get_bionic_char( ch, BIONIC_ARM_L )
	&&   get_bionic_char( ch, BIONIC_ARM_R )
	&&   !remove_bionic( ch, BIONIC_ARM_L, fReplace )
	&&   !remove_bionic( ch, BIONIC_ARM_R, fReplace ) )
	    return;

	if ( !get_eq_char( ch, BIONIC_ARM_L ) )
	{
	    act(AT_BLUE, "A $p is implanted in your left arm socket.",
	     ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted in $s left arm socket.",
	     ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_ARM_L );
	    return;
	}

	if ( !get_bionic_char( ch, BIONIC_ARM_R ) )
	{
	    act(AT_BLUE, "A $p is implanted in your right arm socket.",
	     ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted in $s right arm socket.",
	     ch, obj, NULL, TO_ROOM );
	    equip_char( ch, obj, BIONIC_ARM_R );
	    return;
	}

	bug( "Wear_bionic: no free arm socket.", 0 );
	send_to_char(AT_BLUE, "You already have two bionic arms.\n\r", ch );
	return;
    }

    if ( CAN_ADDBIONIC( obj, ITEM_BIONIC_HAND ) )
    {
	if ( get_bionic_char( ch, BIONIC_HAND_L )
	&&   get_bionic_char( ch, BIONIC_HAND_R )
	&&   !remove_bionic( ch, BIONIC_HAND_L, fReplace )
	&&   !remove_bionic( ch, BIONIC_HAND_R, fReplace ) )
	    return;

	if ( !get_eq_char( ch, BIONIC_HAND_L ) )
	{
	    act(AT_BLUE, "A $p is implanted in your left hand socket.",
	     ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted in $s left hand socket.",
	     ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_HAND_L );
	    return;
	}

	if ( !get_bionic_char( ch, BIONIC_HAND_R ) )
	{
	    act(AT_BLUE, "A $p is implanted in your right hand socket.",
	     ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted in $s right hand socket.",
	     ch, obj, NULL, TO_ROOM );
	    equip_char( ch, obj, BIONIC_HAND_R );
	    return;
	}

	bug( "Wear_bionic: no free hand socket.", 0 );
	send_to_char(AT_BLUE, "You already have two bionic hands.\n\r", ch );
	return;
    }

    if ( CAN_ADDBIONIC( obj, ITEM_BIONIC_LEG ) )
    {
	if ( get_bionic_char( ch, BIONIC_LEG_L )
	&&   get_bionic_char( ch, BIONIC_LEG_R )
	&&   !remove_bionic( ch, BIONIC_LEG_L, fReplace )
	&&   !remove_bionic( ch, BIONIC_LEG_R, fReplace ) )
	    return;

	if ( !get_eq_char( ch, BIONIC_LEG_L ) )
	{
	    act(AT_BLUE, "A $p is implanted in your left leg socket.",
	     ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted in $s left leg socket.",
	     ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_LEG_L );
	    return;
	}

	if ( !get_bionic_char( ch, BIONIC_LEG_R ) )
	{
	    act(AT_BLUE, "A $p is implanted in your right leg socket.",
	     ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted in $s right leg socket.",
	     ch, obj, NULL, TO_ROOM );
	    equip_char( ch, obj, BIONIC_LEG_R );
	    return;
	}

	bug( "Wear_bionic: no free leg socket.", 0 );
	send_to_char(AT_BLUE, "You already have two bionic legs.\n\r", ch );
	return;
    }

    if ( CAN_ADDBIONIC( obj, ITEM_BIONIC_IMPLANT ) )
    {
	if ( get_bionic_char( ch, BIONIC_IMPLANT1 )
	&&   get_bionic_char( ch, BIONIC_IMPLANT2)
	&&   get_bionic_char( ch, BIONIC_IMPLANT3)
	&&   get_bionic_char( ch, BIONIC_IMPLANT4)
	&&   get_bionic_char( ch, BIONIC_IMPLANT5)
	&&   get_bionic_char( ch, BIONIC_IMPLANT6)
	&&   get_bionic_char( ch, BIONIC_IMPLANT7)
	&&   get_bionic_char( ch, BIONIC_IMPLANT8)
	&&   get_bionic_char( ch, BIONIC_IMPLANT9)
	&&   get_bionic_char( ch, BIONIC_IMPLANT10)
	&&   !remove_bionic( ch, BIONIC_IMPLANT1, fReplace )
	&&   !remove_bionic( ch, BIONIC_IMPLANT2, fReplace )
	&&   !remove_bionic( ch, BIONIC_IMPLANT3, fReplace )
	&&   !remove_bionic( ch, BIONIC_IMPLANT4, fReplace )
	&&   !remove_bionic( ch, BIONIC_IMPLANT5, fReplace )
	&&   !remove_bionic( ch, BIONIC_IMPLANT6, fReplace )
	&&   !remove_bionic( ch, BIONIC_IMPLANT7, fReplace )
	&&   !remove_bionic( ch, BIONIC_IMPLANT8, fReplace )
	&&   !remove_bionic( ch, BIONIC_IMPLANT9, fReplace )
	&&   !remove_bionic( ch, BIONIC_IMPLANT10, fReplace ))
	    return;

	if ( !get_bionic_char( ch, BIONIC_IMPLANT1 ) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT1 );
	    return;
	}
	if ( !get_bionic_char( ch, BIONIC_IMPLANT2 ) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT2 );
	    return;
	}
	if ( !get_bionic_char( ch, BIONIC_IMPLANT3 ) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT3 );
	    return;
	}
	if ( !get_bionic_char( ch, BIONIC_IMPLANT4 ) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT4 );
	    return;
	}
	if ( !get_bionic_char( ch, BIONIC_IMPLANT5 ) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT5 );
	    return;
	}
	if ( !get_bionic_char( ch, BIONIC_IMPLANT6 ) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT6 );
	    return;
	}
	if ( !get_bionic_char( ch, BIONIC_IMPLANT7 ) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT7 );
	    return;
	}
	if ( !get_bionic_char( ch, BIONIC_IMPLANT8 ) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT8 );
	    return;
	}
	if ( !get_bionic_char( ch, BIONIC_IMPLANT9 ) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT9);
	    return;
	}
	if ( !get_bionic_char( ch, BIONIC_IMPLANT10) )
	{
	    act(AT_BLUE, "A $p is implanted.",  ch, obj, NULL, TO_CHAR );
	    act(AT_BLUE, "$n gets $p implanted.",    ch, obj, NULL, TO_ROOM );
	    equip_bionic( ch, obj, BIONIC_IMPLANT10);
	    return;
	}


	bug( "Wear_bionic: no free implant", 0 );
	send_to_char(AT_BLUE, "You already have ten bionic implants.\n\r", ch );
	return;
    }

    if ( fReplace )
	send_to_char(AT_BLUE, "You cannot bionically implant that.\n\r", ch );

    return;
}

void do_addbionic( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    one_argument( argument, arg );


    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "What would you like bionically implanted?\n\r", ch ); 
	return;
    }

    if ( !str_cmp( arg, "all" ) )
    {
        OBJ_DATA *obj_next;

        for ( obj = ch->carrying; obj; obj = obj_next )
	{
	    obj_next = obj->next_content;

	    if ( obj->bionic_loc == BIONIC_NONE && can_see_obj( ch, obj ) )
		wear_bionic( ch, obj, FALSE );
	}
	return;
    }
    else
    {
	if ( !( obj = get_obj_carry( ch, arg ) ) )
	{
	    send_to_char(AT_BLUE, "You do not have that item.\n\r", ch );
	    return;
	}
	wear_bionic( ch, obj, TRUE );
    }

    return;
}

void do_rembionic( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA *obj;
    char      arg [ MAX_INPUT_LENGTH ];
    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_BLUE, "Which bionic would you like removed?\n\r", ch );
	return;
    }

    if ( !str_cmp( arg, "all" ) )
    {
	OBJ_DATA *obj_next;
	for ( obj = ch->carrying; obj != NULL; obj = obj_next )
	{
	    obj_next = obj->next_content;
	    if ( obj->bionic_loc != BIONIC_NONE && can_see_obj( ch, obj ) )
	        remove_bionic( ch, obj->wear_loc, TRUE );
	}
	return;
    }
    if ( !( obj = get_bionic_wear( ch, arg ) ) )
    {
	send_to_char(AT_BLUE, "You do not have that item.\n\r", ch );
	return;
    }

    remove_bionic( ch, obj->bionic_loc, TRUE );
    return;
}
