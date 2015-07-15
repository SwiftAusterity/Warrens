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
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include "merc.h"

/*
 * Local functions.
 */ 
void mobile_fight_engine args( ( CHAR_DATA *ch ) );
void mobile_escape args( ( CHAR_DATA *ch ) );
void mobile_flee args( ( CHAR_DATA *ch ) );
void ranged_caster_spec args( ( CHAR_DATA *ch, bool healer ) );
void group_spec_caster args( ( CHAR_DATA *ch ) );
bool group_tactics	args( ( CHAR_DATA *ch ) );
CHAR_DATA *group_ai_find_fighter args( ( CHAR_DATA *ch ) );
CHAR_DATA *group_ai_find_random  args( ( CHAR_DATA *ch ) );
int fight_style_type	args ( ( CHAR_DATA *ch ) );

/* These first two are for fist and foot damage, it adds for eq -Flux */
int  punch_modifier    args( ( CHAR_DATA *ch, CHAR_DATA *victim, int dam ) );
int  kick_modifier    args( ( CHAR_DATA *ch, CHAR_DATA *victim, int dam ) );

/* handles weapon flags like flaming, frosty */
void special_weapon_damage
 args( (CHAR_DATA *ch, CHAR_DATA *victim, OBJ_DATA *wield, int dam ) );
void mystic_damage    args( ( CHAR_DATA *ch, CHAR_DATA *victim, int material ) );
bool damage_shields  args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
bool fight_check_tail args( ( CHAR_DATA *ch, CHAR_DATA *victim, int dam  ) );
bool fight_check_head args( ( CHAR_DATA *ch, CHAR_DATA *victim, int damtype ) );
bool racial_attacks  args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );

/* Master strike sets for stances */
bool tiger_strike    args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool tiger_claw      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool tiger_rush      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool tiger_tail      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool crane_bill      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool crane_wing      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool crane_claw      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool panther_paw     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool panther_scratch args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool panther_tail    args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool dragon_roar     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool dragon_blast    args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool dragon_grab     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool snake_fang      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool snake_bite      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool snake_rush      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool bull_rush       args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool sparrow_flower  args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool sparrow_song    args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool sparrow_wing    args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool sparrow_hop     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool sparrow_claw    args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool sparrow_smash   args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  

/* Dodge skills */
bool check_dodge         args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool check_sidestep      args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool check_blink         args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool check_mirror_images args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
bool check_parry         args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  
bool check_feint         args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  

void check_killer args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );  

/* Special determines whether damage or one_hit called it out */
void dam_message  args( ( CHAR_DATA *ch, CHAR_DATA *victim, int dam,
                         int dt, int miss, int numhit , int special ) );
/* For outputting a death info -Flux */
void  death_message	args( ( CHAR_DATA *killer, CHAR_DATA *victim ) );

void	death_cry	     args( ( CHAR_DATA *ch ) );
void	death_xp_loss	     args( ( CHAR_DATA *victim ) );
void	group_gain	     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
int	xp_compute	     args( ( CHAR_DATA *gch, CHAR_DATA *victim ) );
bool	is_safe		     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
bool	is_bare_hand	     args( ( CHAR_DATA *ch ) );
bool    is_wielding_poisoned args( ( CHAR_DATA *ch ) );
bool    is_wielding_flaming  args( ( CHAR_DATA *ch ) );
bool    is_wielding_shock    args( ( CHAR_DATA *ch ) );
bool    is_wielding_rainbow  args( ( CHAR_DATA *ch ) );
bool    is_wielding_chaos    args( ( CHAR_DATA *ch ) );
bool    is_wielding_icy      args( ( CHAR_DATA *ch ) );

void	make_corpse	     args( ( CHAR_DATA *ch ) );
void	one_hit		     args( ( CHAR_DATA *ch, CHAR_DATA *victim,
				    int dt, int numhit ) );
void	one_dual	     args( ( CHAR_DATA *ch, CHAR_DATA *victim,
				    int dt, int numhit ) );
void	raw_kill	     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
void	set_fighting	     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
void	disarm		     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
void	trip		     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
void	anklestrike	     args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
int     check_item_damage    args( ( CHAR_DATA *ch, CHAR_DATA *victim,
				    OBJ_DATA *weapon, int dam, int damtype ) );

/* These two cover random inventory item breakage from spells */
void	inventory_damage     args( ( CHAR_DATA *ch, int damtype, int dam ) );


/* Gun coding -Flux */
void    set_shooting		args( ( CHAR_DATA *ch, CHAR_DATA *victim ) );
void    ranged_multi		args(( CHAR_DATA *ch, CHAR_DATA *victim ));
void    ranged_hit		args((CHAR_DATA *ch, CHAR_DATA *victim ));
void    ranged_hit_dual		args((CHAR_DATA *ch, CHAR_DATA *victim ));
CHAR_DATA * crossfire_check	args((CHAR_DATA *ch, CHAR_DATA *fch ));

/* For mobs checking if guns are still loaded */
bool 	ammocheck		args((OBJ_DATA *gun));

/* Used to show fighting status during a fight, used to be in comm.c */
void    fight_update		args((CHAR_DATA *ch, CHAR_DATA *victim ));

/* Used to connect tertiary hits to their originator skills. */
void 	extra_hit 		args((CHAR_DATA *ch, CHAR_DATA *victim, int dt ));

/* Update fighting information for combatants */
void fight_update( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int  percent;
 char buf[MAX_STRING_LENGTH];
 char wound[MAX_STRING_LENGTH];

 if ( IS_NPC(ch) )
  return;

 if (!(ch->fighting) && !(ch->engaged))
  return;

  if (MAX_HIT(victim) > 0)
   percent = victim->hit * 100 / MAX_HIT(victim);
  else
   percent = -1;

 if ( ch->in_room != victim->in_room && (ch->in_room) && (victim->in_room) )
 {
  int dir = find_first_step( ch->in_room, victim->in_room );

  if ( dir > -1 )
  {
   sprintf( buf, "$N is to the %s of you.", dir_name[dir] );
   act( AT_RED, buf, ch, NULL, victim, TO_CHAR );
  }

  if (percent >= 90)
   sprintf(wound,"has a few scratches.");
  else if (percent >= 50)
   sprintf(wound,"has quite a few wounds.");
  else if (percent >= 0)
   sprintf(wound,"is in awful condition.");
 }          
 else
 {
  if ( percent >= 100 )      sprintf( wound, "is in perfect health."  );
  else if ( percent >=  90 ) sprintf( wound, "is slightly scratched." );
  else if ( percent >=  80 ) sprintf( wound, "has a few bruises."     );
  else if ( percent >=  70 ) sprintf( wound, "has some cuts."         );
  else if ( percent >=  60 ) sprintf( wound, "has several wounds."    );
  else if ( percent >=  50 ) sprintf( wound, "has many nasty wounds." );
  else if ( percent >=  40 ) sprintf( wound, "is bleeding freely."    );
  else if ( percent >=  30 ) sprintf( wound, "is covered in blood."   );
  else if ( percent >=  20 ) sprintf( wound, "is leaking guts."       );
  else if ( percent >=  10 ) sprintf( wound, "is almost dead."        );
  else                       sprintf( wound, "is DYING."              );
 }

  sprintf(buf,"&Y-&G<&C>&Y- &P$N %s &Y-&C<&G>&Y-", wound);
  act( AT_RED, buf, ch, NULL, victim, TO_CHAR );

 return;
}

/*
 * Control the fights going on.
 * Called periodically by update_handler.
 * Slightly less efficient than Merc 2.2.  Takes 10% of 
 *  total CPU time.
 */
void violence_update( void )
{
    CHAR_DATA *ch;
    CHAR_DATA *victim;
    CHAR_DATA *shootingvictim;
    CHAR_DATA *rch;
    bool       mobfighting;
    int        stun;

    for ( ch = char_list; ch; ch = ch->next )
    {
     if ( !ch->in_room || ch->deleted )
      continue;

     for (stun = 0; stun < STUN_MAX; stun++)
     {
      if ( IS_STUNNED( ch, stun ) )
       ch->stunned[stun]--;
     }

     if ( IS_NPC( ch ) && ch->pIndexData->pShop )
      continue;
     if ( IS_NPC( ch ) && ch->pIndexData->pCasino )
      continue;
     if ( IS_NPC( ch ) && ch->pIndexData->pTattoo )
      continue;

      if ( IS_NPC( ch ) )
       if ( (ch->fleeing_from) )
        mobile_flee( ch );

       if ( (victim = ch->fighting) )
       {
        if ( victim->position == POS_DEAD )
        {
         stop_fighting(ch, FALSE);
         continue;
        }

        if ( ch->in_room == victim->in_room )
        {
         multi_hit( ch, victim, TYPE_UNDEFINED );
         fight_update( ch, victim );

	 for ( rch = ch->in_room->people; rch; rch = rch->next_in_room )
	 {
          if ( !rch )
           continue;

	  if ( rch->deleted )
           continue;

          if ( rch == ch || rch == victim )
           continue;

          if ( (rch->fighting) )
           continue;

          if ( is_same_group( ch, rch ) && (ch->fighting) &&
           ch->in_room == rch->in_room &&
           (IS_SET( rch->act, PLR_AUTOASSIST ) || IS_NPC(rch)) )
          {
	   act( AT_RED, "You jump into the fray to assist $N!", rch, NULL,
            ch, TO_CHAR );
	   act( AT_YELLOW, "$n jumps into the fight to assist you!", rch,
            NULL, ch, TO_VICT );
	   act( AT_YELLOW, "$n assists $N.", rch, NULL,
	    ch, TO_NOTVICT );
	   set_fighting( rch, ch->fighting );
          }

          if ( !rch->next_in_room )
           break;
         }
         if ( IS_NPC( ch ) )
          mobile_fight_engine( ch );
        }
        else
        {
         if ( IS_NPC( ch ) )
          mobile_fight_engine( ch );

         if ( (shootingvictim = ch->engaged) )
          ranged_multi( ch, shootingvictim );

         fight_update( ch, victim );
        }
        continue;
       }

       /* Ok. So ch is not fighting anyone.
	* Is there a fight going on?
	*/
	mobfighting = FALSE;
	for ( rch = ch->in_room->people; rch; rch = rch->next_in_room )
	{
         if ( !rch )
          continue;

         if ( (rch->deleted) )
          continue;

         if ( !(victim = rch->fighting) )
          continue;

         if ( ch == rch || rch == victim )
          continue;

	 if ( !IS_NPC( ch )
	  && ( !IS_NPC( rch ) || IS_AFFECTED( rch, AFF_CHARM ) )
	  && is_same_group( ch, rch )
	  && IS_NPC( victim ) )
	   break;

	 if ( IS_NPC( ch )
	  && IS_NPC( rch )
	  && !IS_NPC( victim ) )
	 {
	  mobfighting = TRUE;
	  break;
	 }

          if ( !rch->next_in_room )
           break;
	}

	if ( !victim || !rch )
	    continue;

	/*
	 * Now that someone is fighting, consider fighting another pc
	 * or not at all.
	 */
	if ( mobfighting )
	{
	    CHAR_DATA *vch;
	    int        number;

	    number = 0;
	    for ( vch = ch->in_room->people; vch; vch = vch->next_in_room )
	    {
             if ( !vch )
              break;

             if ( vch->deleted )
              continue;

	     if ( can_see( ch, vch )
	      && ( vch->level > 10 )
	      && is_same_group( vch, victim )
	      && number_range( 0, number ) == 0 )
	     {
	      victim = vch;
	      number++;
	     }

             if ( !vch->next_in_room )
              break;
	    }

	    if ( ( rch->pIndexData != ch->pIndexData && number_bits( 3 ) != 0 )
	     || abs( victim->level - ch->level ) > 3 )
             continue;
	}

     /* Boy is this silly and long, but nonetheless necessary I believe */
      if ( IS_NPC(ch) && !ch->fighting )
      {
       if ( !is_same_group( ch, victim ) &&
        ch->in_room == victim->in_room &&
        ((victim->fighting) && is_same_group( ch, victim->fighting)) )
        set_fighting( ch, victim );
      }
      else if ( !ch->fighting )
      {
       if ( !is_same_group( ch, victim ) &&
        ch->in_room == victim->in_room &&
        ((victim->fighting) && is_same_group( ch, victim->fighting) &&
         IS_SET( ch->act, PLR_AUTOASSIST )) )
        set_fighting( ch, victim );
      }

     if ( !ch->next )
      break;
    }
 return;
}

/*
 * Do one group of attacks.
 */
void multi_hit( CHAR_DATA *ch, CHAR_DATA *victim, int dt )
{
    int numhit = 0;
    int dnumhit = 0;
    AFFECT_DATA *paf;

    if ( IS_NPC( ch ) )
    {
      mprog_hitprcnt_trigger( ch, victim );
      mprog_fight_trigger( ch, victim );
    }

  if ( IS_AFFECTED2( ch, AFF_GRASPING ) && IS_AFFECTED2( victim, AFF_GRASPED ) )
  {

   if ( is_affected( victim, gsn_grapple ) )
   {
    if ( get_curr_str(victim) + number_range(0,10)
          > get_curr_str(ch) + number_range(0,5) )
    {
     act( AT_RED, "$N breaks free of your grasp!", ch, NULL, victim, TO_CHAR );
     act( AT_YELLOW, "You break free of $n's grasp!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$N breaks free of $n's grasp.", ch, NULL, victim, TO_NOTVICT );

     if ( is_affected( ch, gsn_grapple ) )	    
     affect_strip( ch, gsn_grapple );

     if ( is_affected( victim, gsn_grapple ) )	    
     affect_strip( victim, gsn_grapple );

     REMOVE_BIT( ch->affected_by2, AFF_GRASPING );
     REMOVE_BIT( victim->affected_by2, AFF_GRASPED );

     ch->position = POS_FIGHTING;
     victim->position = POS_FIGHTING;

     return;
    }
    else
    {
     act( AT_YELLOW, "Your grasp holds $N in place!", ch, NULL, victim, TO_CHAR );
     act( AT_YELLOW, "You can't break free of $n's grasp!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$n's grasp holds $N in place.", ch, NULL, victim, TO_NOTVICT );
    }
    damage( ch, victim, ( get_curr_str( ch ) * dice( 1, 2 ) + dice( 2, 10 ) ),
     gsn_grapple, DAM_INTERNAL, TRUE );
    return;
   }
   else
   {
    if ( number_range(get_curr_str(victim), get_curr_con(victim)) + number_range(1,3)
          > get_curr_int(ch) + number_range(0,2) )
     {
     act( AT_RED, "$N breaks free of your magical grasp!", ch, NULL, victim, TO_CHAR );
     act( AT_YELLOW, "You break free of $n's magical grasp!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$N breaks free of $n's magical grasp.", ch, NULL, victim, TO_NOTVICT );
      REMOVE_BIT( ch->affected_by2, AFF_GRASPING );
	REMOVE_BIT( victim->affected_by2, AFF_GRASPED );
      ch->position = POS_FIGHTING;
      victim->position = POS_FIGHTING;

      return;
     }
     else
     {
     act( AT_YELLOW, "Your magical grasp holds $N in place!", ch, NULL, victim, TO_CHAR );
     act( AT_YELLOW, "You can't break free of $n's magical grasp!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$n's magical grasp holds $N in place.", ch, NULL, victim, TO_NOTVICT );
     }

    for( paf = victim->affected2; paf; paf = paf->next )
    {
     if (paf->deleted)
      continue;
     if ( skill_table[paf->type].grasp == TRUE )
     break;     
    }       
    (*skill_table[paf->type].touch_fun) (paf->type,
                                       URANGE( 1, ch->level, LEVEL_HERO),
                                       ch, victim ); 
      return;
   }
  }     
    if ( IS_AFFECTED2( ch, AFF_GRASPED ) )
     return;

    update_pos( ch, victim );

    if ( ch->position <= POS_STUNNED )
    {
     send_to_char(AT_WHITE,
	"You are stunned, but will probably recover.\n\r", ch );
     act(AT_WHITE, "$n is stunned, but will probably recover.",
	    ch, NULL, NULL, TO_ROOM );
     return;
    }

    if ( (vision_impared(ch)) && number_percent() > 85 )
    {
     send_to_char( C_DEFAULT,
      "You grope around blindly, unable to retaliate!\n\r", ch );
     act( AT_YELLOW,
      "$n gropes around blindly, unable to retaliate!",
      ch, NULL, NULL, TO_ROOM );
     return;
    }

    if ( ( IS_AFFECTED2( ch, AFF_CONFUSED ) )
       && number_percent ( ) < 10 )
    {
      act(AT_YELLOW, "$n looks around confused at what's going on.", ch, NULL, NULL, TO_ROOM );
      send_to_char( AT_YELLOW, "You stand confused.\n\r", ch );
      return;
    }
    
    if ( !IS_NPC( ch ) )
    {
     if ( ch->pcdata->condition[COND_INSOMNIA] == 0 && number_percent() < 25 )
     {
      send_to_char( AT_WHITE, "You close your eyes for a second and try to catch some z's.\n\r", ch );
      act(AT_YELLOW, "$n closes $s eyes and appears to be trying to sleep.", ch, NULL, NULL, TO_ROOM );
      return;
     }
    }

    numhit += (get_curr_agi( ch ) / 5);
    dnumhit += (get_curr_dex( ch ) / 5);

    if ( IS_NPC( ch ) )
    {
      numhit += (ch->level / 30);
      dnumhit += (ch->level / 50);
    }

   if(!IS_NPC(ch) )
   {
    if ( number_percent( ) > 90 &&
         number_percent( ) < ch->pcdata->learned[gsn_chain_attack] &&
         ch->pcdata->learned[gsn_chain_attack] > 0 )
    {
     act( AT_YELLOW, "You see an opening and launch a chain of attacks!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n launches into a chain of attacks!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$n launches into a chain of attacks.", ch, NULL, victim, TO_NOTVICT );
        numhit += (get_curr_agi(ch) / 4);
	update_skpell( ch, gsn_chain_attack );
    }
   }

  if ( !(racial_attacks( ch, victim ) ) )
   return;

  one_hit( ch, victim, dt, numhit );    

 /* This gives PC's an early advantage over mobs */
  if ( IS_NPC(ch) && ch->level <= 10 )
   return;

  one_dual( ch, victim, dt, dnumhit );
  
  return;
}

/* Handles all damage shields for both one_hit and one_dual */
bool damage_shields( CHAR_DATA *ch, CHAR_DATA *victim )
{
 if (ch != victim )
 {
  if ( IS_AFFECTED( victim, AFF_FIRESHIELD ) )
  {
   if ( number_percent( ) < 50 )
   {
    if ( IS_AFFECTED2( ch, AFF_MMIRROR ) )
    {
     if ( number_percent( ) < 15 )
     {
	if ( !IS_NPC(victim) )
       spell_fire_snap ( skill_lookup("fire snap"), 15, victim, victim );
	else
       spell_fire_snap ( skill_lookup("fire snap"), 25, victim, victim );
     }
    }
    else
    {
     if ( !IS_NPC(victim) )
      spell_fire_snap ( skill_lookup("fire snap"), 15, victim, ch );
     else
      spell_fire_snap ( skill_lookup("fire snap"), 25, victim, ch );
    }
   }
  }

  if ( !victim || victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return FALSE;

	if ( IS_AFFECTED( victim, AFF_ICESHIELD ) )
	{
	 if ( ( number_percent( ) < 50 ) || ( number_percent( ) < 17 ) )
	 {
        if ( IS_AFFECTED2( ch, AFF_MMIRROR ) )
        {
         if ( number_percent( ) < 15 )
         {
	    if ( !IS_NPC(victim) )
	     spell_icy_snap ( skill_lookup("icy snap"), 20, victim, victim );
	    else
	     spell_icy_snap ( skill_lookup("icy snap"), 40, victim, victim );
         }
        }
        else
        {
	   if ( !IS_NPC(victim) )
	    spell_icy_snap ( skill_lookup("icy snap"), 20, victim, ch );
	   else
	    spell_icy_snap ( skill_lookup("icy snap"), 40, victim, ch );
        }
       }
      }

  if ( !victim || victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return FALSE;

	if ( IS_AFFECTED( victim, AFF_SHOCKSHIELD ) )
      {
       if ( ( number_percent( ) < 50 )
	  || ( number_percent( ) < 17 ) )
	 {
        if ( IS_AFFECTED2( ch, AFF_MMIRROR ) )
        {
         if ( number_percent( ) < 15 )
         {
          if ( !IS_NPC(victim) )
	     spell_electric_snap ( skill_lookup("electric snap"), 15, victim, victim );
	    else
	     spell_electric_snap ( skill_lookup("electric snap"), 35, victim, victim );
         }
        }
        else
        {
	   if ( !IS_NPC(victim) )
	    spell_electric_snap ( skill_lookup("electric snap"), 15, victim, ch );
	   else
          spell_electric_snap ( skill_lookup("electric snap"),35, victim, ch );
        }
       }
      }

  if ( !victim || victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return FALSE;

	if ( IS_AFFECTED( victim, AFF_CHAOS ) )  
      {
       if ( number_percent( ) < 50 )
	 {
        if ( IS_AFFECTED2( ch, AFF_MMIRROR ) )
        {
         if ( number_percent( ) < 15 )
         {
	    if ( !IS_NPC(victim) )
	     spell_chaos_snap ( skill_lookup("chaos snap"), 30, victim, victim );
	    else
	     spell_chaos_snap ( skill_lookup("chaos snap"), 40, victim, victim );
         }
        }
        else
        {
	   if ( !IS_NPC(victim) )
	    spell_chaos_snap ( skill_lookup("chaos snap"), 30, victim, ch );
	   else
          spell_chaos_snap ( skill_lookup("chaos snap"), 40, victim, ch );
	  }
       }
      }

  if ( !victim || victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return FALSE;

	if ( IS_AFFECTED( victim, AFF_VIBRATING ) )
      {
       if ( ( number_percent( ) < 50 )
	  || ( number_percent( ) < 17 ) )
       {  
        if ( IS_AFFECTED2( ch, AFF_MMIRROR ) )
        {
         if ( number_percent( ) < 15 )
          spell_sonicvibe( skill_lookup("sonic wave"), 25, victim, victim );
        }
        else
         spell_sonicvibe( skill_lookup("sonic wave"), 25, victim, ch );
       }
      }

  if ( !victim || victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return FALSE;

	if ( IS_AFFECTED2( victim, AFF_BLADE ) )
      {
       if ( number_percent( ) < 15 )
       { 
        if ( IS_AFFECTED2( ch, AFF_MMIRROR ) )
        {
         if ( number_percent( ) < 15 )
          spell_phantom_razor( skill_lookup("phantom razor"), 45, victim, victim );
        }
        else
         spell_phantom_razor( skill_lookup("phantom razor"), 45, victim, ch );
       }
      }

  if ( !victim || victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return FALSE;

	if ( IS_AFFECTED2( victim, AFF_THORNY ) )
	   {
	    if ( number_percent( ) < 10 )
          {
           if ( IS_AFFECTED2( ch, AFF_MMIRROR ) )
           {
         	if ( number_percent( ) < 10 )
             spell_strike_of_thorns( skill_lookup("strike of thorns"),
              ch->level, victim, victim );
           }
            else
             spell_strike_of_thorns( skill_lookup("strike of thorns"),
              victim->level, victim, ch );
          }
         }

  if ( !victim || victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return FALSE;
 }

 return TRUE;
}

/* This appears at the beginning of one_hit, one_dual and damage */
bool fight_check_head( CHAR_DATA *ch, CHAR_DATA *victim, int damtype )
{
 update_pos( ch, victim );

 if ( !ch || ch->position == POS_DEAD
   || !victim || victim->position == POS_DEAD )
  return FALSE;
   
    /*
     * Stop up any residual loopholes.
     */
    if ( victim != ch )
    {
    /*
     * Certain attacks are forbidden.
     * Most other attacks are returned.
     */

   if ( immune_calc( damtype, victim ) > 0 )
    if ( is_safe( ch, victim ) )
     return FALSE;

    if ( immune_calc( damtype, victim ) > 0 )
     if(!IS_NPC(ch) && !IS_NPC(victim))
     {
      if (!(ch == victim) )
      {
       ch->combat_timer = 90;
       victim->combat_timer = 90;
      }
     }

     if ( immune_calc( damtype, victim ) > 0 )
     {
      if ( !(IS_AFFECTED2( victim, AFF_TORTURED )) )
      {
       if ( !ch->fighting )
	set_fighting( ch, victim );

       if ( !victim->fighting )
        set_fighting( victim, ch );
      }
          
	    /*
	     * If victim is charmed, ch might attack victim's master.
	     */
	    if (   IS_NPC( ch )
		&& IS_NPC( victim )
		&& IS_AFFECTED( victim, AFF_CHARM )
		&& victim->master
		&& victim->master->in_room == ch->in_room
		&& number_bits( 3 ) == 0 )
	    {
		stop_fighting( ch, FALSE );
		set_fighting( ch, victim->master );
		return TRUE;
	    }

	/*
	 * More charm stuff.
	 */
	if ( victim->master == ch )
	    stop_follower( victim );
       }

	/*
	 * Inviso attacks ... not.
	 */
	if ( IS_AFFECTED( ch, AFF_INVISIBLE ) )
	{
	    affect_strip( ch, gsn_invis      );
	    affect_strip( ch, gsn_mass_invis );
	    REMOVE_BIT( ch->affected_by, AFF_INVISIBLE );
	    act(AT_GREY, "$n fades into existence.", ch, NULL, NULL, TO_ROOM );
	}
	if (IS_AFFECTED2( ch, AFF_PHASED ) )
	{
            affect_strip ( ch, skill_lookup("phase shift") );
	    affect_strip ( ch, skill_lookup("mist form") );
	    REMOVE_BIT( ch->affected_by2, AFF_PHASED );
	    act(AT_GREY, "$n returns from an alternate plane.", ch, NULL, NULL, TO_ROOM );
	}
    }

    if ( IS_STUNNED( ch, STUN_NON_MAGIC ) ||
	 IS_STUNNED( ch, STUN_TOTAL ) )
      return FALSE;

 return TRUE;
}

/* Ends all damage functions */
bool fight_check_tail( CHAR_DATA *ch, CHAR_DATA *victim, int dam )
{
    char      buf [ MAX_STRING_LENGTH ];

    if ( ( IS_AFFECTED2( victim, AFF_MANASHIELD ) )
     && victim->mana > 0 )
    {
     victim->mana -= dam;
     if ( victim->mana < 0 )
     {
     REMOVE_BIT( victim->affected_by2, AFF_MANASHIELD );
     act( AT_RED, "Your mana shield absorbs too much damage and leaves you paralyzed!", victim, NULL, NULL, TO_CHAR );
     act( AT_RED, "$n's mana shield recoils and paralyzes $m!", victim, NULL, NULL, TO_ROOM );
     STUN_CHAR( victim, 2, STUN_TOTAL );
     victim->position = POS_STUNNED;
     victim->mana = 0;     
     }
    }
    else
    {
     if ( IS_AFFECTED( victim, AFF_SIAMESE ) )
     {
      if ( (victim->master) && dam > 0 )
      {
       act( AT_RED, "You feel your life drain as $N is harmed.",
        victim->master, NULL, victim, TO_CHAR );
       victim->master->hit -= (dam / 4);
       victim->hit -= ((3*dam)/4);
      }
      else if ( (victim->master) && dam < 0 )
      {
       act( AT_RED, "You feel your life strengthen as $N is healed.",
        victim->master, NULL, victim, TO_CHAR );
       victim->master->hit += ((3*dam)/64);
       victim->hit += ((5*dam)/64);
      }
      else
       victim->hit -= dam;
     }
     else
      victim->hit -= dam;

    if ( IS_AFFECTED2( ch, AFF_VAMPIRIC ) && dam > 0 && ch != victim )
    {
     act( AT_RED, "You feel the life energies being sucked out of $N as they flow into you.",
      ch, NULL, victim, TO_CHAR );

     if ( IS_AFFECTED( ch, AFF_SIAMESE ) )
     {
      if ( (ch->master) )
      {
       act( AT_RED, "You feel your life strengthen as $N is healed.",
        ch->master, NULL, ch, TO_CHAR );
       ch->master->hit += ((3*dam)/64);
       ch->hit += ((5*dam)/64);
      }
      else
       ch->hit += ( dam / 8 );
     }
     else
      ch->hit += ( dam / 8 );
    }

  if ( !IS_NPC( victim ) )
  {
   if ( dam > 0
    && ( victim->pcdata->learned[gsn_pain_tolerance] < dice( 5, 20 ) 
    || ( victim->pcdata->attitude <= 10 && victim->pcdata->attitude >= -10 )))
     {
     if ( number_percent() > 95 )
      {
       if ( victim->class[0] == CLASS_MURDERER
         || victim->class[0] == CLASS_THIEF
         || victim->class[0] == CLASS_ASSASSIN
         || victim->class[0] == CLASS_TERRORIST
         || victim->class[0] == CLASS_VANDILIER )
        {
        if ( victim->pcdata->attitude <= 9 )
         victim->pcdata->attitude += 1;
        }
        else
        {
        if ( victim->pcdata->attitude >= -9 )
         victim->pcdata->attitude -= 1;
        }
      }
     }
     else
     if  ( victim->pcdata->learned[gsn_pain_tolerance] > 0 )
      update_skpell( ch, gsn_pain_tolerance );
    }
   }

    if ( ( ( !IS_NPC( victim )                  /* so imms only die by */
     && IS_NPC( ch )                      /* the hands of a PC   */
     && victim->level >= LEVEL_IMMORTAL )
      ||
     ( !IS_NPC( victim )                   /* so imms dont die  */
     && victim->level >= LEVEL_IMMORTAL    /* by poison type dmg */
     && ch == victim ) )                   /* since an imm == pc */
     && victim->hit < 1 )
    victim->hit = 1;

       
    update_pos( victim, ch );	

   if ( !victim->deleted )
    switch( victim->position )
    {
    case POS_MORTAL:
	send_to_char(AT_RED, 
	    "You are mortally wounded, and will die soon, if not aided.\n\r",
	    victim );
	act(AT_RED, "$n is mortally wounded, and will die soon, if not aided.",
	    victim, NULL, NULL, TO_ROOM );
	break;

    case POS_INCAP:
	send_to_char(AT_RED,
	    "You are incapacitated and will slowly die, if not aided.\n\r",
	    victim );
	act(AT_RED, "$n is incapacitated and will slowly die, if not aided.",
	    victim, NULL, NULL, TO_ROOM );
	break;

    case POS_DEAD:
	send_to_char(AT_BLOOD, "You have been KILLED!!\n\r\n\r", victim );
	act(AT_BLOOD, "$n is DEAD!!", victim, NULL, NULL, TO_ROOM );
	break;

    default:
	if ( dam > MAX_HIT(victim) / 4 )
	    send_to_char(AT_RED, "That really did HURT!\n\r", victim );
	if ( victim->hit < MAX_HIT(victim) / 4 )
	    send_to_char(AT_RED, "You sure are BLEEDING!\n\r", victim );
	break;
    }

    /*
     * Sleep spells and extremely wounded folks.
     */
    if ( !IS_AWAKE( victim ) )
	stop_fighting( victim, FALSE );

    /*
     * Payoff for killing things.
     */
    if ( victim->position == POS_DEAD )
    {
     bool is_ranged = !(ch->in_room == victim->in_room);
     stop_fighting( victim, TRUE );
     stop_shooting( victim, TRUE );

      if ( IS_NPC(victim) )
       if ( IS_SET( victim->act, ACT_ILLUSION ) )
       {
        act( AT_RED,
         "$N's form wavers and dispurses into little bits of light, vanishing before your eyes.",
         ch, NULL, victim, TO_NOTVICT );
        act( AT_RED,
         "$N's form wavers and dispurses into little bits of light, vanishing before your eyes.",
         ch, NULL, victim, TO_CHAR );
        extract_char(victim, FALSE);
        return TRUE;
       }

        if ( !IS_ARENA(ch) )
        {
	  group_gain( ch, victim );

          if ( ( !IS_NPC(ch) ) && ( !IS_NPC(victim) ) )
          {
            CLAN_DATA  *pClan;
            CLAN_DATA  *Cland;
            if ( ch->clan != victim->clan )
            {
              if ( (pClan = get_clan_index(ch->clan)) != NULL )
                pClan->pkills++;
              if ( (Cland = get_clan_index(victim->clan)) != NULL )
                Cland->pdeaths++;
            }
          }

          if ( ( !IS_NPC(ch) ) && ( IS_NPC(victim) ) )
          {
           CLAN_DATA    *pClan;
           if ( (pClan=get_clan_index(ch->clan)) != NULL )
             pClan->mkills++;
          }

          if ( ( IS_NPC(ch) ) && (!IS_NPC(victim)) )
          {
           CLAN_DATA   *pClan;
           if ( (pClan=get_clan_index(victim->clan)) != NULL )
             pClan->mdeaths++;
          }
        
	  if ( !IS_NPC( victim ) )
	  {
	    /*
	     * Dying penalty:
	     * 1/2 way back to previous level.
	     */
	    if ( victim->level < LEVEL_HERO 
	    || ( victim->level >= LEVEL_HERO && IS_NPC( ch ) ) )
		death_xp_loss( victim );
	    sprintf( log_buf, "%s killed by %s at %d.", victim->name,
	        ch->name, victim->in_room->vnum );

          death_message( ch, victim );

	    log_string( log_buf, CHANNEL_LOG, -1 );
            wiznet(log_buf,NULL,NULL,WIZ_DEATHS,0,0);
	  }
	}
	else
	{
	  sprintf(log_buf, "&C%s &chas defeated &C%s &cin the arena!",
	          ch->name, victim->name);
	  wiznet(log_buf, NULL, NULL, WIZ_DEATHS, 0, 0);
	  log_string(log_buf, CHANNEL_LOG, -1);
	  challenge(log_buf, 0, 0);
	}

	raw_kill( ch, victim );

	if (!IS_NPC(ch)
	&& !IS_NPC(victim)
	&& victim->pcdata->bounty > 0)
	{
	   sprintf(buf, "You receive a %d gold bounty, for killing %s.\n\r",
	   victim->pcdata->bounty, victim->name);
	   send_to_char(AT_WHITE, buf, ch);
	   ch->money.gold += victim->pcdata->bounty;
	   victim->pcdata->bounty =0;
	}

	/* Ok, now we want to remove the deleted flag from the
	 * PC victim.
	 */
	if ( !IS_NPC( victim ) )
	    victim->deleted = FALSE;

	if ( !IS_NPC( ch ) && IS_NPC( victim ) )
	{
         if ( !is_ranged )
         {
	    if ( IS_SET( ch->act, PLR_AUTOLOOT ) )
		do_get( ch, "all corpse" );
	    else
		do_look( ch, "in corpse" );

	    if ( IS_SET( ch->act, PLR_AUTOCOINS ) )
	        do_get( ch, "all.coin corpse" );
	    if ( IS_SET( ch->act, PLR_AUTOSAC  ) )
		do_sacrifice( ch, "corpse" );
         }
	}

	return FALSE;
    }

    if ( victim == ch )
	return FALSE;

    /*
     * Take care of link dead people.
     */
    if ( !IS_NPC( victim ) && !victim->desc )
    {
	if ( number_range( 0, victim->wait ) == 0 )
	{
	    do_recall( victim, "" );
	    return FALSE;
	}
    }

    /*
     * Wimp out?
     */
    /* mobs are taken care of by the fighting_engine */

      if ( !IS_NPC( victim )
	&& victim->hit   > 0
	&& victim->hit  <= victim->wimpy
	&& victim->wait == 0 )
        {
         if ( victim->position == POS_DEAD
         ||   ch->position == POS_DEAD )
	 return FALSE;
	 do_flee( victim, "" );
        }

 return TRUE;
}

/*
 * Hit one guy once.
 */
void one_hit( CHAR_DATA *ch, CHAR_DATA *victim, int dt, int numhit )
{
    OBJ_DATA *wield;
    AFFECT_DATA af;
    char      buf [ MAX_STRING_LENGTH ];
    int       dampchance;
    int	      randamp = 0;
    int       dam = 0;
    int       cdam = 0;
    int       numhit2 = numhit;
    int       miss = 1;
    int	      damclass = 0;

    if ( !(fight_check_head( ch, victim, 0 )) )
     return;

    /*
     * Figure out the type of damage message.
     */
   if ( !IS_NPC(ch) && !is_class( ch, CLASS_ASSASSIN ) )
    wield = get_eq_char( ch, WEAR_WIELD );
   else if ( IS_NPC(ch) )
    wield = get_eq_char( ch, WEAR_WIELD );
   else
    wield = NULL;

   if ( !IS_NPC(ch) && !is_class( ch, CLASS_ASSASSIN ) )
    wield = get_eq_char( ch, WEAR_WIELD );
   else if ( IS_NPC(ch) )
    wield = get_eq_char( ch, WEAR_WIELD );
   else
    wield = NULL;

    if ( dt == TYPE_UNDEFINED )
    {
     damclass = DAM_BASH;
     dt = TYPE_HIT;

	if ( (wield) && wield->item_type == ITEM_WEAPON )
        {
         if ( wield->value[8] == WEAPON_BLADE )
         {
           dt = wield->value[7] + 902;
           damclass = weapon_to_damage( wield->value[7]);
         }
         else
         {
	  dt = wield->value[3];
          damclass = weapon_to_damage(wield->value[8]);
         }
        }
        else if ( wield )
        {
	    dt = DAMNOUN_STRIKE;
            damclass = DAM_BASH;
        }          
        else if ( !IS_NPC(ch) )
        {
	 if ( is_class( ch, CLASS_ASSASSIN ) && ch->pcdata->claws == 0 )
         {
	    dt = DAMNOUN_STRIKE;
            damclass = DAM_BASH;
         }          
         else if ( ch->pcdata->claws >= 1 && !wield )
         {
          if ( ch->pcdata->claws < 4 )
          {
	   dt = DAMNOUN_TEAR;
           damclass = DAM_SCRATCH;
          }
          else if ( ch->pcdata->claws == CLAW_PIERCE )
          {
           dt = DAMNOUN_PIERCE;
           damclass = DAM_PIERCE;
          }
	  else if ( ch->pcdata->claws == CLAW_CHOP )
          {
	   dt = DAMNOUN_CHOP;
           damclass = DAM_SLASH;
          }
          else if ( ch->pcdata->claws == CLAW_BASH )
          {
           dt = DAMNOUN_SMASH;
           damclass = DAM_BASH;
          }
          else if ( ch->pcdata->claws == CLAW_SLASH )
          {
           dt = DAMNOUN_SLASH;
           damclass = DAM_SLASH;
          }
         }
        }
        else
        {
         if ( ch->race_type == RACE_TYPE_LOBSTER
           || ch->race_type == RACE_TYPE_BIRD 
           || ch->race_type == RACE_TYPE_DOG )
         {
         dt = DAMNOUN_TEAR;
         damclass = DAM_SCRATCH;
         }
         else if ( ch->race_type == RACE_TYPE_HUMANOID )
         {
         dt = TYPE_HIT;
         damclass = DAM_BASH;
         }
         else if ( ch->race_type == RACE_TYPE_SNAKE ||
                   ch->race_type == RACE_TYPE_SPIDER ||
                   ch->race_type == RACE_TYPE_FLYING_INSECT )
         {
         dt = DAMNOUN_BITE;
         damclass = DAM_SCRATCH;
         }
        }
    }

    /*
     * Calculate chance to avoid.
     */
     dampchance = victim->p_damp;

     dampchance -= ( ch->hitroll / 6 );

     dampchance -= victim->size;

     if ( !IS_NPC(ch) )
     {
      if ( ch->pcdata->attitude == ATTITUDE_PROTECTIVE )
      {
       send_to_char( AT_YELLOW,
        "You drop into a fighting stance.\n\r", ch );
       ch->pcdata->attitude = ATTITUDE_DEFENSIVE;
      }
      dampchance -= ( (ch->pcdata->attitude * 2) / 3 );
     }

     if ( !IS_NPC( ch ) && wield && wield->item_type == ITEM_WEAPON )
     {
      if ( wield->value[8] != WEAPON_BLADE )
      {
       if ( ch->pcdata->learned[gsn_weapons_mastery] > number_percent() )
       {
        dampchance -= ( (ch->pcdata->weapon[wield->value[8]] + 15) / 20 );
        update_skpell( ch, gsn_weapons_mastery );
       }
       else
        dampchance -= ( ch->pcdata->weapon[wield->value[8]] / 20 );

       if ( (number_percent() - dice( 5, 3 ) ) > ch->pcdata->weapon[wield->value[8]] &&
            ch->pcdata->weapon[wield->value[8]] < 95 )
       {
        send_to_char( AT_WHITE, "You feel more confident with your weapon.\n\r", ch );
        ch->pcdata->weapon[wield->value[8]] += dice(1, 3);
        if ( ch->pcdata->weapon[wield->value[8]] > 95 && ch->level < LEVEL_IMMORTAL )
        ch->pcdata->weapon[wield->value[8]] = 95;
       }
      }
      else
      {
       if ( ch->pcdata->learned[gsn_weapons_mastery] > number_percent() )
       {
        dampchance -= ( (ch->pcdata->weapon[wield->value[7]] + 15) / 20 );
        update_skpell( ch, gsn_weapons_mastery );
       }
       else
        dampchance -= ( ch->pcdata->weapon[wield->value[7]] / 20 );
 
       if ( (number_percent() - dice( 5, 3 ) ) >
             ch->pcdata->weapon[wield->value[7]] &&
            ch->pcdata->weapon[wield->value[7]] < 95 )
       {
        send_to_char( AT_WHITE, "You feel more confident with your weapon.\n\r", ch );
        ch->pcdata->weapon[wield->value[7]] += dice(1, 8);
        if ( ch->pcdata->weapon[wield->value[7]] > 95 && ch->level < LEVEL_IMMORTAL )
        ch->pcdata->weapon[wield->value[7]] = 95;
       }
      }
     }
     else if ( !IS_NPC( ch ) && ( ch->pcdata->learned[gsn_fisticuffs] > 0 ) )
     {
       dampchance -= ( ch->pcdata->learned[gsn_fisticuffs] / 20 );
       update_skpell( ch, gsn_fisticuffs );
     }

    if ( ( !IS_NPC( ch ) ) && ( ch->pcdata->learned[gsn_enhanced_hit] > 0 ) ) 
    {
       dampchance -= (ch->pcdata->learned[gsn_enhanced_hit] / 15);
       update_skpell( ch, gsn_enhanced_hit );
    }

    while( numhit > 0 )
    {
    /*
     * The moment of excitement!
     */
  
    randamp = number_percent( );

    if ( dampchance > number_range( randamp / 2, randamp ) )
    {
	/* Miss due to dampener */
        numhit -= 1;
        numhit2 -= 1;
	continue;
    }

	/*
	 * Check for disarm, trip, parry, and dodge.
	 */
         if ( ch != victim )
         {
	   if ( !IS_STUNNED( victim, STUN_NON_MAGIC ) ||
	        !IS_STUNNED( victim, STUN_TOTAL ) )
           {

	    if ( check_parry( ch, victim ) )
	    {
		/* Miss. */
    		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }

	    if ( check_dodge( ch, victim ) )
	    {
      		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }

	    if ( check_sidestep( ch, victim ) )
	    {
      		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }

	    if ( check_feint( ch, victim ) )
	    {
      		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }

	    if ( check_blink( ch, victim ) )
	    {
      		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }

	    if ( check_mirror_images( ch, victim ) )
	    {
      		 numhit -= 1;
		continue;
	    }

   if(!IS_NPC(victim) && !IS_AFFECTED2( victim, AFF_PMIRROR) )
   {
    int damred = 0;
     /* for monkey and sparrow, the rest are 90 */
    if ( number_percent( ) > 85 && number_percent( )
     < victim->pcdata->learned[gsn_redirect] &&
     ( vision_impared( victim ) &&
       victim->pcdata->learned[gsn_blindfight] > number_percent( ) ) )
    {
     if ( victim->pcdata->attitude == -14 )
     {
     act( AT_RED, "$N avoids your attack and lands a hard blow to your back!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N sidesteps $n's attack and lands a massive blow to $s back.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You avoid $n's attack and land a massive blow to $s back.", ch, NULL, victim, TO_VICT );

           damred = ( get_curr_str(victim) * number_range( 2, 5 ) );
           damage( victim, ch, damred, gsn_monkey_stance, DAM_BASH, TRUE );
     ch->position = POS_RESTING;

     if ( number_percent() > 75 && damred >= 0 )
     {
     act( AT_RED, "You feel your spine crack, the pain blinds and paralyzes you!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "You hear a loud cracking sound as $n doubles over.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You feel $n's spine crack from your tremendous blow.", ch, NULL, victim, TO_VICT );

          if ( !is_affected( ch, gsn_blindness ) )
           {
		af.type      = gsn_blindness;
		af.duration  = 5;
		af.location  = APPLY_HITROLL;
		af.modifier  = -50;
		af.bitvector = AFF_BLIND;
		affect_join( ch, &af );
           }

           dam = ( get_curr_str(victim) * number_range( 2, 4 ) );
           damage( victim, ch, dam, gsn_monkey_stance, DAM_INTERNAL, TRUE  );
          }
     numhit = 0;
     numhit2 -= 1;
     if ( numhit2 < 0 )
     numhit2 = 0;
     dam = damred;
     update_skpell( victim, gsn_redirect );
     continue;
     }

     if ( victim->pcdata->attitude == -13 )
     {
     act( AT_RED, "$N catches your attack and throws your arms to the side.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N catches $n's attack and throws $s hands to the side.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You catch $n's attack and toss his hands to the side.", ch, NULL, victim, TO_VICT );
     STUN_CHAR( ch, 2, STUN_TOTAL );
     ch->position = POS_STUNNED;
     numhit = 0;
     numhit2 -= 1;
     if ( numhit2 < 0 )
     numhit2 = 0;
     dam = damred;
     update_skpell( victim, gsn_redirect ); 
     continue;
     }
    }

    if ( number_percent( ) > 90 && number_percent( )
     < victim->pcdata->learned[gsn_redirect] &&
     ( vision_impared( victim ) &&
       victim->pcdata->learned[gsn_blindfight] > number_percent( ) ) )
    {
     if ( victim->pcdata->attitude == 13 )
     {
     act( AT_RED, "$N catches your attack and throws your arms to the side.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N catches $n's attack and throws $s hands to the side.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You catch $n's attack and toss his hands to the side.", ch, NULL, victim, TO_VICT );
     numhit = 0;
     numhit2 -= 1;
     if ( numhit2 < 0 )
     numhit2 = 0;
     dam = damred; 
     update_skpell( victim, gsn_redirect ); 
     continue;
     }

     if ( victim->pcdata->attitude == -12 )
     {
     act( AT_RED, "$N sidesteps your attack and kicks you into the ground.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N sidesteps $n's attack and kicks $m into the ground.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You sidestep $n's attack and land a kick to $s back that sends $m flying!", ch, NULL, victim, TO_VICT );
     damred = ( get_curr_str(victim) * number_range( 1, 3 ) );
     damage( victim, ch, damred, gsn_crane_stance, DAM_BASH, TRUE );
     ch->position = POS_RESTING;
     numhit = 0;
     numhit2 -= 1;
     if ( numhit2 < 0 )
     numhit2 = 0;
     dam = damred;
     update_skpell( victim, gsn_redirect ); 
     continue;
     }      

     if ( victim->pcdata->attitude <= 10 &&
          victim->pcdata->attitude >= -10 )
     {
     act( AT_RED, "$N sidesteps your attack and hits you.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N sidesteps $n's attack and hits $m.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You sidestep $n's attack and hit $m!", ch, NULL, victim, TO_VICT );
     one_hit( victim, ch, dt, 1 );
     numhit -= 1;
     numhit2 -= 1;
     update_skpell( victim, gsn_redirect ); 
     continue;
     }
    }
   }
 }
    if(!IS_NPC(victim) && IS_AFFECTED2( victim, AFF_PMIRROR) )
    {
     if ( number_percent( ) > 90 )
     {
     act( AT_RED, "You attack $N, but your hand vanishes and comes right back at you!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n attacks $N, but $s hand turns around and hits $mself.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "Your physical mirror redirects $n's attack!", ch, NULL, victim, TO_VICT );
     one_hit( victim, ch, dt, 1 );
     numhit -= 1;
     numhit2 -= 1;
     continue;
     }
    }

    /*
     * Hit.
     * Calc damage.
     */
   }
    if ( IS_NPC( ch ) )
    {
      dam = number_range(get_curr_str(ch), get_curr_str( ch ) * 2 );

	if ( wield && wield->item_type == ITEM_WEAPON )
	    dam += number_range( wield->value[1]/3, wield->value[2]/3 );
    }
    else
    {
    if ( !IS_NPC( ch ))
    {
	if ( wield && wield->item_type == ITEM_WEAPON )
	    dam += number_range( wield->value[1], wield->value[2] );
 
	    dam += number_range( get_curr_str(ch)/2, get_curr_str(ch) );
    }
	if ( wield && dam > 1000 && !IS_IMMORTAL(ch) )
	{
	    sprintf( buf, "One_hit dam range > 1000 from %d to %d",
		    wield->value[1], wield->value[2] );
	    bug( buf, 0 );
	    if ( wield->name )
	      bug( wield->name, 0 );
	}
    }

	/*
	 * Damage modifiers.
	 */
    dam += (ch->size * 2);

    if ( !IS_NPC( ch ) )
        if ( ch->pcdata->claws >= 1 && !wield )
 	 dam += dam / 8;

    if ( (wield != NULL) && ch->race == RACE_MINOTAUR
	&& wield->value[8] == WEAPON_CHOP )
        dam += (dam / 4);


    dam += ( GET_DAMROLL( ch ) );

    if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_enhanced_damage] > 0 )
    {
	dam += dam / 6;
	update_skpell( ch, gsn_enhanced_damage );
    }

    if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_superior_damage] > 0 )
    {
        dam += dam / 6;
	update_skpell( ch, gsn_superior_damage );
    }

    if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_desperation] > 0 
       && ch->hit < MAX_HIT(ch) / 4 )
    {
        dam += dam / 2;
	update_skpell( ch, gsn_desperation );
    }

    if ( IS_NPC( ch ) || ( !IS_NPC( ch ) &&
         ( ch->pcdata->learned[gsn_place_force] < number_percent() ||
           ch->pcdata->learned[gsn_armor_knowledge] < number_percent() ) ) )
    {
     dam = (check_item_damage( ch, victim, wield, dam, damclass ));

     if ( !IS_NPC( ch ) )
     {
      if ( ch->pcdata->learned[gsn_place_force] > 0 )
       update_skpell( ch, gsn_place_force );

      if ( ch->pcdata->learned[gsn_armor_knowledge] > 0 )
       update_skpell( ch, gsn_armor_knowledge );
     }
    }
    else if ( !IS_NPC( ch ) )
    {
     if ( ch->pcdata->learned[gsn_place_force] > 0 )
      update_skpell( ch, gsn_place_force );

     if ( ch->pcdata->learned[gsn_armor_knowledge] > 0 )
      update_skpell( ch, gsn_armor_knowledge );
    }

      if ( !IS_NPC( victim ) )
       if ( victim->pcdata->remort > 0 )
        dam *= 2;

    if ( !IS_NPC( ch )
    && ch->pcdata->learned[gsn_anatomyknow] > 0
    && number_percent( ) <= ch->pcdata->learned[gsn_anatomyknow] / 9 )
    {
	update_skpell( ch, gsn_anatomyknow );

	send_to_char( AT_RED, "You hit a pressure point!\n\r", ch );
	act( AT_RED, "$n hit one of $N's pressure points!",
		     ch, NULL, victim, TO_NOTVICT );

	act( AT_RED, "$n hit you with a precise shot.", 
		     ch, NULL, victim, TO_VICT );

	if ( number_percent( ) < 10 )
	{
 	 STUN_CHAR( victim, 2, STUN_TOTAL );
	 victim->position = POS_STUNNED;
	 dam += 50;
	}
	else
	 dam += 25;
       }

    if ( IS_NPC(victim) )
     dam -= ( (dam * victim->skin) / 12);

    else if ( !IS_NPC(victim) )
    {
     if ( victim->race == RACE_QUICKSILVER )
     {
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_SLASH &&
           damclass == DAM_SLASH )
       dam = ( ( dam * 8 ) / 10 );
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_CLAW &&
           damclass == DAM_SCRATCH )
       dam = ( ( dam * 8 ) / 10 );
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_BASH &&
           damclass == DAM_BASH )
       dam = ( ( dam * 8 ) / 10 );
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_CHOP &&
           damclass == DAM_SLASH )
       dam = ( ( dam * 8 ) / 10 );
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_PIERCE &&
           damclass == DAM_PIERCE )
       dam = ( ( dam * 8 ) / 10 );
     }
    }


     /* Inspired by Critical_strike skill by Brian Babey aKa Varen
      * Coded by -Flux
      */
     if ( !IS_NPC( ch ) && wield )
     {
      if ( wield->value[8] != WEAPON_BLADE )
      {
       if ( number_percent() * 20 < ch->pcdata->weapon[wield->value[8]] )
       {
   act( AT_YELLOW, "You critically strike $N!", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n critically strikes $N!", ch, NULL, victim, TO_NOTVICT );
   act( AT_RED, "$n critically strikes you!", ch, NULL, victim, TO_VICT );
        dam *= 1.4;
       }
      }
      else
      {
       if ( number_percent() * 20 < ch->pcdata->weapon[wield->value[7]] )
       {
   act( AT_YELLOW, "You critically strike $N!", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n critically strikes $N!", ch, NULL, victim, TO_NOTVICT );
   act( AT_RED, "$n critically strikes you!", ch, NULL, victim, TO_VICT );
        dam *= 1.4;
       }
      }
     }
     else if ( !IS_NPC( ch ) )
     {
      if ( number_percent() * 30 < ch->pcdata->learned[gsn_fisticuffs] )
       {
   act( AT_YELLOW, "You critically strike $N!", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n critically strikes $N!", ch, NULL, victim, TO_NOTVICT );
   act( AT_RED, "$n critically strikes you!", ch, NULL, victim, TO_VICT );
        dam *= 1.4;
       update_skpell( ch, gsn_fisticuffs );
       }
     }     

    if ( !(wield) )
     dam = punch_modifier(ch, victim, dam );

    dam = ((dam * immune_calc( damclass, victim )) / 100);

   if ( !IS_NPC( ch ) )
    if ( dam > 0 && !saves_spell( ch->level, victim ) &&
	!IS_AFFECTED( victim, AFF_POISON ) &&
	( ( wield && is_wielding_poisoned( ch ) )
        || ( ch->pcdata->claws == 2 && is_bare_hand( ch ) ) ) )
    {
	AFFECT_DATA af;

        af.level     = ch->level;
	af.type      = gsn_poison;
	af.duration  = 1;
	af.location  = APPLY_STR;
	af.modifier  = -2;
	af.bitvector = AFF_POISON;
	affect_to_char( victim, &af );
    }

    if ( !IS_NPC( ch ))
     if ( dam > 0 && !saves_spell( ch->level, victim ) && 
	!IS_AFFECTED2( victim, AFF_DISEASED ) &&
        ch->pcdata->claws == 3 && is_bare_hand( ch ) )
     {
	AFFECT_DATA af;

        af.level     = ch->level;
	af.type      = gsn_plague;
	af.duration  = 1;
	af.location  = APPLY_CON;
	af.modifier  = -2;
	af.bitvector = AFF_DISEASED;
	affect_to_char2( victim, &af );
     }

    if ( IS_NPC( ch ) )
     if ( !IS_AFFECTED( victim, AFF_POISON ) &&
	( ch->race_type == RACE_TYPE_SNAKE ||
          ch->race_type == RACE_TYPE_SPIDER ) )
     {
	AFFECT_DATA af;

        af.level     = ch->level;
	af.type      = gsn_poison;
	af.duration  = 1;
	af.location  = APPLY_STR;
	af.modifier  = -2;
	af.bitvector = AFF_POISON;
	affect_to_char( victim, &af );
     }

    if (  IS_SET( ch->act, UNDEAD_TYPE( ch ) )
       && !saves_spell(ch->level, victim ) )
    {
	AFFECT_DATA af;

        af.level     = ch->level;
	af.type      = gsn_poison;
	af.duration  = 2;
	af.location  = APPLY_CON;
	af.modifier  = -1;
	af.bitvector = AFF_POISON;
	affect_join( victim, &af );
    }

    if ( dam > 0 
     && is_affected( ch, gsn_flamehand )
     && number_percent( ) < 33 )
     damage( ch, victim, dice( 5, 15 ), gsn_flamehand, DAM_HEAT, FALSE );
    if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
     return;

    if ( dam > 0
     && is_affected( ch, gsn_frosthand )
     && number_percent( ) < 20 )
     damage( ch, victim, dice( 5, 15 ), gsn_frosthand, DAM_COLD, FALSE );
    if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
     return;

   if ( dam > 0 
    && is_affected( ch, gsn_chaoshand )
    && number_percent( ) < 10 )
     damage( ch, victim, dice( 5, 15 ), gsn_chaoshand, DAM_DYNAMIC, FALSE );
    if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return;

   if ( dam > 0
    && is_affected( ch, gsn_shockhand )
    && number_percent( ) < 10 )
     damage( ch, victim, dice( 5, 15 ), gsn_shockhand, DAM_NEGATIVE, FALSE );
    if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return;

   if ( dam > 0
    && is_affected( ch, gsn_darkhand )
    && number_percent( ) < 10 )
     damage( ch, victim, dice( 5, 15 ), gsn_darkhand, DAM_VOID, FALSE ); 
   if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return;

   if ( dam > 0
    && is_affected( ch, gsn_quiverpalm )
    && number_percent( ) < 40 )
     damage( ch, victim, dice( 5, 15 ), gsn_quiverpalm, DAM_DYNAMIC, FALSE );
   if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return;

   if ( dam > 0 
    && is_affected( ch, gsn_colorhand )
    && number_percent( ) < 10 )
     damage( ch, victim, dice( 5, 15 ), gsn_colorhand, DAM_POSITIVE, FALSE );
   if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return;

    if ( wield )
    {
     special_weapon_damage( ch, victim, wield, dam );
     mystic_damage( ch, victim, wield->composition );
    }

    numhit -= 1;
    cdam += dam;
    miss = 0;
    dam = 0;
    }
    dam = cdam;

    if ( dt != TYPE_UNDEFINED )
     dam_message( ch, victim, dam, dt, miss, numhit2, 0 );
    
    if ( !(fight_check_tail( ch, victim, dam )) )
     return;

    if ( (wield) && wield->item_type == ITEM_WEAPON )
     oprog_use_trigger( wield, ch, victim );

    if ( ch->in_room == victim->in_room && dam > 0 )
     if ( !(damage_shields( ch, victim ) ) )
      return;

    tail_chain( );
    return;
}

void one_dual( CHAR_DATA *ch, CHAR_DATA *victim, int dt, int numhit )
{
    OBJ_DATA *wield;
    AFFECT_DATA af;
    CHAR_DATA *was_vict = NULL;
    char      buf [ MAX_STRING_LENGTH ];
    int       dampchance;
    int	      randamp = 0;
    int       dam = 0;
    int       cdam = 0;
    int       numhit2 = numhit;
    int       miss = 1;
    int	      damclass = 0;

    if ( !IS_NPC(ch) )
    {
     was_vict = victim;
     if ( (ch->pcdata->twin_fighting) )
      victim = ch->pcdata->twin_fighting;
    }

   if ( !IS_NPC(ch) )
   {
    if ( !(fight_check_head( ch, victim, 0 ) ) )
    {
     victim = was_vict;
     if ( !(fight_check_head( ch, victim, 0 ) ) )
      return;
    }
   }
   else
    if ( !(fight_check_head( ch, victim, 0 ) ) )
     return;

    /*
     * Figure out the type of damage message.
     */
   if ( !IS_NPC(ch) && !is_class( ch, CLASS_ASSASSIN ) )
    wield = get_eq_char( ch, WEAR_WIELD_2 );
   else if ( IS_NPC(ch) )
    wield = get_eq_char( ch, WEAR_WIELD_2 );
   else
    wield = NULL;

   if ( !IS_NPC(ch) && !is_class( ch, CLASS_ASSASSIN ) )
    wield = get_eq_char( ch, WEAR_WIELD_2 );
   else if ( IS_NPC(ch) )
    wield = get_eq_char( ch, WEAR_WIELD_2 );
   else
    wield = NULL;

    if ( dt == TYPE_UNDEFINED )
    {
        damclass = DAM_BASH;
	dt = TYPE_HIT;
	if ( wield && wield->item_type == ITEM_WEAPON )
        {
         if ( wield->value[8] == WEAPON_BLADE )
         {
           dt = wield->value[7] + 902;
           damclass = weapon_to_damage(wield->value[7]);
         }
         else
         {
	  dt = wield->value[3];
          damclass = weapon_to_damage(wield->value[8]);
         }
        }
        else if ( wield )
        {
	    dt = DAMNOUN_STRIKE;
            damclass = DAM_BASH;
        }          
        else if ( !IS_NPC(ch) )
        {
	 if ( is_class( ch, CLASS_ASSASSIN ) && ch->pcdata->claws == 0 )
         {
	    dt = DAMNOUN_STRIKE;
            damclass = DAM_BASH;
         }          
         else if ( ch->pcdata->claws >= 1 && !wield )
         {
          if ( ch->pcdata->claws < 4 )
          {
	   dt = DAMNOUN_TEAR;
           damclass = DAM_SCRATCH;
          }
          else if ( ch->pcdata->claws == CLAW_PIERCE )
          {
           dt = DAMNOUN_PIERCE;
           damclass = DAM_PIERCE;
          }
	  else if ( ch->pcdata->claws == CLAW_CHOP )
          {
	   dt = DAMNOUN_CHOP;
           damclass = DAM_SLASH;
          }
          else if ( ch->pcdata->claws == CLAW_BASH )
          {
           dt = DAMNOUN_SMASH;
           damclass = DAM_BASH;
          }
          else if ( ch->pcdata->claws == CLAW_SLASH )
          {
           dt = DAMNOUN_SLASH;
           damclass = DAM_SLASH;
          }
         }
        }
        else
        {
         if ( ch->race_type == RACE_TYPE_LOBSTER
           || ch->race_type == RACE_TYPE_BIRD 
           || ch->race_type == RACE_TYPE_DOG )
         {
         dt = DAMNOUN_TEAR;
         damclass = DAM_SCRATCH;
         }
         else if ( ch->race_type == RACE_TYPE_HUMANOID )
         {
         dt = TYPE_HIT;
         damclass = DAM_BASH;
         }
         else if ( ch->race_type == RACE_TYPE_SNAKE ||
                   ch->race_type == RACE_TYPE_SPIDER ||
                   ch->race_type == RACE_TYPE_FLYING_INSECT )
         {
         dt = DAMNOUN_BITE;
         damclass = DAM_SCRATCH;
         }
        }
    }

    /*
     * Calculate p_damp.
     */
    dampchance = victim->p_damp;

    dampchance -= ( ch->hitroll / 6 );

    dampchance -= victim->size;

     if ( !IS_NPC( ch ) && wield && wield->item_type == ITEM_WEAPON )
     {
      if ( wield->value[8] != WEAPON_BLADE )
      {
       if ( ch->pcdata->learned[gsn_weapons_mastery] > number_percent() )
       {
        dampchance -= ( (ch->pcdata->weapon[wield->value[8]] + 15) / 20 );
        update_skpell( ch, gsn_weapons_mastery );
       }
       else
        dampchance -= ( ch->pcdata->weapon[wield->value[8]] / 20 );

       if ( (number_percent() - dice( 5, 3 )) > ch->pcdata->weapon[wield->value[8]] &&
            ch->pcdata->weapon[wield->value[8]] < 95 )
       {
        send_to_char( AT_WHITE, "You feel more confident with your weapon.\n\r", ch );
        ch->pcdata->weapon[wield->value[8]] += dice(1, 8);
        if ( ch->pcdata->weapon[wield->value[8]] > 95 && ch->level < LEVEL_IMMORTAL )
        ch->pcdata->weapon[wield->value[8]] = 95;
       }
      }
      else
      {
       if ( ch->pcdata->learned[gsn_weapons_mastery] > number_percent() )
       {
        dampchance -= ( (ch->pcdata->weapon[wield->value[7]] + 15) / 20 );
        update_skpell( ch, gsn_weapons_mastery );
       }
       else
        dampchance -= ( ch->pcdata->weapon[wield->value[7]] / 20 );

       if ( (number_percent() - dice( 5, 3 )) > ch->pcdata->weapon[wield->value[7]] &&
            ch->pcdata->weapon[wield->value[7]] < 95 )
       {
        send_to_char( AT_WHITE, "You feel more confident with your weapon.\n\r", ch );
        ch->pcdata->weapon[wield->value[7]] += dice(1, 8);
        if ( ch->pcdata->weapon[wield->value[7]] > 95 && ch->level < LEVEL_IMMORTAL )
        ch->pcdata->weapon[wield->value[7]] = 95;
       }
      }
     }
     else if ( !IS_NPC( ch ) && ( ch->pcdata->learned[gsn_fisticuffs] > 0 ) )
     {
       dampchance -= ( ch->pcdata->learned[gsn_fisticuffs] / 20 );
       update_skpell( ch, gsn_fisticuffs );
     }

    if ( !IS_NPC(ch) )
    {
      if ( ch->pcdata->attitude == ATTITUDE_PROTECTIVE )
      {
       send_to_char( AT_YELLOW,
        "You drop into a fighting stance.\n\r", ch );
       ch->pcdata->attitude = ATTITUDE_DEFENSIVE;
      }
     dampchance += ch->pcdata->attitude;
    }
    if ( ( !IS_NPC( ch ) ) && ( ch->pcdata->learned[gsn_enhanced_hit] > 0 ) ) 
    {
       dampchance -= (ch->pcdata->learned[gsn_enhanced_hit] / 10);
       update_skpell( ch, gsn_enhanced_hit );
    }


    while( numhit > 0 )
    {
    /*
     * The moment of excitement!
     */
  

   randamp = number_percent();

    if ( dampchance > number_range( randamp / 2, randamp ) )
    {
	/* Miss due to dampener */
        numhit -= 1;
	continue;
    }

	/*
	 * Check for disarm, trip, parry, and dodge.
	 */
         if ( ch != victim )
         {
	   if ( !IS_STUNNED( victim, STUN_NON_MAGIC ) ||
	        !IS_STUNNED( victim, STUN_TOTAL ) )
           {

	    if ( check_parry( ch, victim ) )
	    {
		/* Miss. */
      		numhit -= 1;
                numhit2 -= 1;
		continue;
	    }

	    if ( check_dodge( ch, victim ) )
	    {
      		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }

	    if ( check_sidestep( ch, victim ) )
	    {
      		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }
	    if ( check_feint( ch, victim ) )
	    {
      		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }


	    if ( check_blink( ch, victim ) )
	    {
      		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }

	    if ( check_mirror_images( ch, victim ) )
	    {
      		 numhit -= 1;
                numhit2 -= 1;
		continue;
	    }

   if(!IS_NPC(victim) && !IS_AFFECTED2( victim, AFF_PMIRROR) )
   {
    int damred = 0;
     /* for monkey and sparrow, the rest are 90 */
    if ( number_percent( ) > 85 && number_percent( )
     < victim->pcdata->learned[gsn_redirect] &&
     ( vision_impared( victim ) &&
       victim->pcdata->learned[gsn_blindfight] > number_percent( ) ) )
    {
     if ( victim->pcdata->attitude == -14 )
     {
     act( AT_RED, "$N avoids your attack and lands a hard blow to your back!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N sidesteps $n's attack and lands a massive blow to $s back.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You avoid $n's attack and land a massive blow to $s back.", ch, NULL, victim, TO_VICT );

           damred = ( get_curr_str(victim) * number_range( 2, 5 ) );
           damage( victim, ch, damred, gsn_monkey_stance, DAM_INTERNAL,TRUE );
     ch->position = POS_RESTING;

     if ( number_percent() > 75 && damred >= 0 )
     {
     act( AT_RED, "You feel your spine crack, the pain blinds and paralyzes you!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "You hear a loud cracking sound as $n doubles over.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You feel $n's spine crack from your tremendous blow.", ch, NULL, victim, TO_VICT );

          if ( !is_affected( ch, gsn_blindness ) )
           {
		af.type      = gsn_blindness;
		af.duration  = 5;
		af.location  = APPLY_HITROLL;
		af.modifier  = -50;
		af.bitvector = AFF_BLIND;
		affect_join( ch, &af );
           }

           damred = ( get_curr_str(victim) * number_range( 2, 5 ) );
           damage( victim, ch, damred, gsn_monkey_stance, 0, TRUE );
     }
     numhit = 0;
     numhit2 -= 1;
     if ( numhit2 < 0 )
     numhit2 = 0;
     dam = damred;
     update_skpell( victim, gsn_redirect ); 
     continue;
     }
     if ( victim->pcdata->attitude == -13 )
     {
     act( AT_RED, "$N catches your attack and throws your arms to the side.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N catches $n's attack and throws $s hands to the side.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You catch $n's attack and toss his hands to the side.", ch, NULL, victim, TO_VICT );
     numhit = 0;
     numhit2 -= 1;
     if ( numhit2 < 0 )
     numhit2 = 0;
     dam = damred;
     update_skpell( victim, gsn_redirect ); 
     continue;
     }
    }

    if ( number_percent( ) > 90 && number_percent( )
     < victim->pcdata->learned[gsn_redirect] &&
     ( vision_impared( victim ) &&
       victim->pcdata->learned[gsn_blindfight] > number_percent( ) ) )
    {
     if ( victim->pcdata->attitude == 13 )
     {
     act( AT_RED, "$N catches your attack and throws your arms to the side.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N catches $n's attack and throws $s hands to the side.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You catch $n's attack and toss his hands to the side.", ch, NULL, victim, TO_VICT );
     STUN_CHAR( ch, 2, STUN_TOTAL );
     ch->position = POS_STUNNED;
     numhit = 0;
     numhit2 -= 1;
     if ( numhit2 < 0 )
     numhit2 = 0;
     dam = damred;
     update_skpell( victim, gsn_redirect ); 
     continue;
     }
     if ( victim->pcdata->attitude == -12 )
     {
     act( AT_RED, "$N sidesteps your attack and kicks you into the ground.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N sidesteps $n's attack and kicks $m into the ground.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You sidestep $n's attack and land a kick to $s back that sends $m flying!", ch, NULL, victim, TO_VICT );
     damred = ( get_curr_str(victim) * number_range( 1, 3 ) );
     damage( victim, ch, damred, gsn_crane_stance, 0, TRUE );
     ch->position = POS_RESTING;
     numhit = 0;
     numhit2 -= 1;
     if ( numhit2 < 0 )
     numhit2 = 0;
     dam = damred;
     update_skpell( victim, gsn_redirect ); 
     continue;
     }      

     if ( victim->pcdata->attitude <= 10 &&
          victim->pcdata->attitude >= -10 )
     {
     act( AT_RED, "$N sidesteps your attack and hits you.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N sidesteps $n's attack and hits $m.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You sidestep $n's attack and hit $m!", ch, NULL, victim, TO_VICT );
     one_hit( victim, ch, dt, 1 );
     numhit -= 1;
     numhit2 -= 1;
     update_skpell( victim, gsn_redirect ); 
     continue;
     }
    }
   }
  }
    if(!IS_NPC(victim) && IS_AFFECTED2( victim, AFF_PMIRROR) )
    {
     if ( number_percent( ) > 90 )
     {
     act( AT_RED, "You attack $N, but your hand vanishes and comes right back at you!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n attacks $N, but $s hand turns around and hits $mself.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "Your physical mirror redirects $n's attack!", ch, NULL, victim, TO_VICT );
     one_hit( victim, ch, dt, 1 );
     numhit -= 1;
     numhit2 -= 1;
     if ( numhit2 < 0 )
     numhit2 = 0;
     continue;
     }
    }
   }
    /*
     * Hit.
     * Calc damage.
     */
    if ( IS_NPC( ch ) )
    {
      dam = number_range(get_curr_str(ch)/2, get_curr_str( ch ) );

	if ( wield && wield->item_type == ITEM_WEAPON )
	    dam += number_range( wield->value[1]/4, wield->value[2]/4 );
    }
    else
    {
    if ( !IS_NPC( ch ))
    {
	if ( wield && wield->item_type == ITEM_WEAPON )
	    dam += number_range( wield->value[1], wield->value[2] );

	    dam += number_range( get_curr_str(ch)/3,get_curr_str(ch) );
    }

	if ( wield && dam > 1000 && !IS_IMMORTAL(ch) )
	{
	    sprintf( buf, "One_hit dam range > 1000 from %d to %d",
		    wield->value[1], wield->value[2] );
	    bug( buf, 0 );
	    if ( wield->name )
	      bug( wield->name, 0 );
	}
    }

	/*
	 * Damage modifiers.
	 */
    dam += (ch->size * 10);

    if ( !IS_NPC( ch ))
    if ( ch->pcdata->claws >= 1 && !wield )
	dam += dam / 8;

    if ( (wield != NULL) && ch->race == RACE_MINOTAUR
	&& wield->value[8] == WEAPON_CHOP )
        dam += (dam / 4);


    dam += ( GET_DAMROLL( ch ) );

    if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_enhanced_damage] > 0 )
    {
	dam += dam / 4;
	update_skpell( ch, gsn_enhanced_damage );
    }

    if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_superior_damage] > 0 )
    {
        dam += dam / 3;
	update_skpell( ch, gsn_superior_damage );
    }

    if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_desperation] > 0 && ch->hit < MAX_HIT(ch) / 4 )
    {
        dam += dam / 2;
	update_skpell( ch, gsn_desperation );
    }

    if ( IS_NPC( ch ) || ( !IS_NPC( ch ) &&
         ( ch->pcdata->learned[gsn_place_force] < number_percent() ||
           ch->pcdata->learned[gsn_armor_knowledge] < number_percent() ) ) )
    {
     dam = (check_item_damage( ch, victim, wield, dam, damclass ));

     if ( !IS_NPC( ch ) )
     {
      if ( ch->pcdata->learned[gsn_place_force] > 0 )
       update_skpell( ch, gsn_place_force );

      if ( ch->pcdata->learned[gsn_armor_knowledge] > 0 )
       update_skpell( ch, gsn_armor_knowledge );
     }
    }
    else if ( !IS_NPC( ch ) )
    {
     if ( ch->pcdata->learned[gsn_place_force] > 0 )
      update_skpell( ch, gsn_place_force );

     if ( ch->pcdata->learned[gsn_armor_knowledge] > 0 )
      update_skpell( ch, gsn_armor_knowledge );
    }

    if ( !IS_NPC( ch )
     && ch->pcdata->learned[gsn_anatomyknow] > 0
     && number_percent( ) <= ch->pcdata->learned[gsn_anatomyknow] / 9 )
    {
     update_skpell( ch, gsn_anatomyknow );

     send_to_char( AT_RED, "You hit a pressure point!\n\r", ch );
     act( AT_RED, "$n hit one of $N's pressure points!",
      ch, NULL, victim, TO_NOTVICT );


     act( AT_RED, "$n hit you with a precise shot.", 
      ch, NULL, victim, TO_VICT );

     if ( number_percent( ) < 10 )
     {
      STUN_CHAR( victim, 2, STUN_TOTAL );
      victim->position = POS_STUNNED;
      dam += 50;
     }
     else
      dam += 25;
    }

    if ( IS_NPC(victim) )
     dam -= ( (dam * victim->pIndexData->skin) / 12);
    else if ( !IS_NPC(victim) )
    {
     if ( victim->race == RACE_QUICKSILVER )
     {
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_SLASH &&
           damclass == DAM_SLASH )
       dam = ( ( dam * 8 ) / 10 );
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_CLAW &&
           damclass == DAM_SCRATCH )
       dam = ( ( dam * 8 ) / 10 );
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_BASH &&
           damclass == DAM_BASH )
       dam = ( ( dam * 8 ) / 10 );
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_CHOP &&
           damclass == DAM_SLASH )
       dam = ( ( dam * 8 ) / 10 );
      if ( victim->pcdata->morph[MORPH_ARMOR] == MORPH_WEAPON_PIERCE &&
           damclass == DAM_PIERCE )
       dam = ( ( dam * 8 ) / 10 );
     }
    }

     /* Inspired by Critical_strike skill by Brian Babey aKa Varen
      * Coded by -Flux
      */
     if ( !IS_NPC( ch ) && wield )
     {
      if ( wield->value[8] != WEAPON_BLADE )
      {
       if ( number_percent() * 20 < ch->pcdata->weapon[wield->value[8]] )
       {
   act( AT_YELLOW, "You critically strike $N!", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n critically strikes $N!", ch, NULL, victim, TO_NOTVICT );
   act( AT_RED, "$n critically strikes you!", ch, NULL, victim, TO_VICT );
        dam *= 1.4;
       }
      }
      else
      {
       if ( number_percent() * 20 < ch->pcdata->weapon[wield->value[7]] )
       {
   act( AT_YELLOW, "You critically strike $N!", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n critically strikes $N!", ch, NULL, victim, TO_NOTVICT );
   act( AT_RED, "$n critically strikes you!", ch, NULL, victim, TO_VICT );
        dam *= 1.4;
       }
      }
     }
     else if ( !IS_NPC( ch ) )
     {
      if ( number_percent() * 20 < ch->pcdata->learned[gsn_fisticuffs] )
       {
   act( AT_YELLOW, "You critically strike $N!", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n critically strikes $N!", ch, NULL, victim, TO_NOTVICT );
   act( AT_RED, "$n critically strikes you!", ch, NULL, victim, TO_VICT );
        dam *= 1.4;
       update_skpell( ch, gsn_fisticuffs );
       }
     }

    if ( !(wield) )
     dam = punch_modifier(ch, victim, dam );

    dam = ((dam * immune_calc( damclass, victim )) / 100);

    if ( !IS_NPC( ch ))
     if ( dam > 0 && !saves_spell( ch->level, victim ) &&
	!IS_AFFECTED( victim, AFF_POISON ) &&
	( ( wield && is_wielding_poisoned( ch ) )
        || ( ch->pcdata->claws == 2 && is_bare_hand( ch ) ) ) )
     {
	AFFECT_DATA af;

        af.level     = ch->level;
	af.type      = gsn_poison;
	af.duration  = 1;
	af.location  = APPLY_STR;
	af.modifier  = -2;
	af.bitvector = AFF_POISON;
	affect_to_char( victim, &af );
     }

    if ( !IS_NPC( ch ))
     if ( dam > 0 && !saves_spell( ch->level, victim ) &&
	!IS_AFFECTED2( victim, AFF_DISEASED ) &&
          ch->pcdata->claws == 3 && is_bare_hand( ch ) )
     {
	AFFECT_DATA af;

        af.level     = ch->level;
	af.type      = gsn_plague;
	af.duration  = 1;
	af.location  = APPLY_CON;
	af.modifier  = -2;
	af.bitvector = AFF_DISEASED;
	affect_to_char2( victim, &af );
     }

    if ( IS_NPC( ch ) )
     if ( ( ch->race_type == RACE_TYPE_SNAKE ||
          ch->race_type == RACE_TYPE_SPIDER ) &&
	!IS_AFFECTED( victim, AFF_POISON ) )
    {
	AFFECT_DATA af;

        af.level     = ch->level;
	af.type      = gsn_poison;
	af.duration  = 1;
	af.location  = APPLY_STR;
	af.modifier  = -2;
	af.bitvector = AFF_POISON;
	affect_to_char( victim, &af );
    }
 
    if ( wield )
    {
     special_weapon_damage( ch, victim, wield, dam );
     mystic_damage( ch, victim, wield->composition );
    }

    numhit -= 1;
    cdam += dam;
    miss = 0;
    dam = 0;
   }

    dam = cdam;
    if ( dt != TYPE_UNDEFINED )
     dam_message( ch, victim, dam, dt, miss, numhit2, 0 );

    if ( !(fight_check_tail( ch, victim, dam )) )
    return;

    if ( (wield) && wield->item_type == ITEM_WEAPON )
     oprog_use_trigger( wield, ch, victim );

    if ( ch->in_room == victim->in_room && dam > 0)
     if ( !(damage_shields( ch, victim ) ) )
      return;

    tail_chain( );
    return;
}

/*
 * Inflict damage from a spell or skill, not from actual combat damage.
 */
void damage( CHAR_DATA *ch, CHAR_DATA *victim, int dam, int dt, int immune, bool skill )
{
    AFFECT_DATA af;

    if ( !(fight_check_head( ch, victim, immune ) ) )
     return;

    /*
     * Check for dodge-skills, only if the damage is physical, Swift.
     * Also check for shield damage & redirection.
     */
     if ( immune == DAM_SLASH || immune == DAM_PIERCE || 
          immune == DAM_SCRATCH || immune == DAM_BASH )
     {

	   if ( !IS_STUNNED( victim, STUN_NON_MAGIC ) ||
	        !IS_STUNNED( victim, STUN_TOTAL ) )
           {
	    if ( check_parry( ch, victim ) )
	    {
		/* Miss. */
             dam_message( ch, victim, 0, dt, 1, 1, 1 );
             return;
	    }

	    if ( check_dodge( ch, victim ) )
	    {
             dam_message( ch, victim, 0, dt, 1, 1, 1 );
             return;
	    }

	    if ( check_sidestep( ch, victim ) )
	    {
             dam_message( ch, victim, 0, dt, 1, 1, 1 );
             return;
	    }

	    if ( check_feint( ch, victim ) )
	    {
             dam_message( ch, victim, 0, dt, 1, 1, 1 );
             return;
	    }

	    if ( check_blink( ch, victim ) )
	    {
             dam_message( ch, victim, 0, dt, 1, 1, 1 );
             return;
	    }

	    if ( check_mirror_images( ch, victim ) )
	    {
             dam_message( ch, victim, 0, dt, 1, 1, 1 );
             return;
	    }

   if ( ch->in_room == victim->in_room )
   {
    if(!IS_NPC(victim) && !IS_AFFECTED2( victim, AFF_PMIRROR) )
    {
     /* for monkey and sparrow, the rest are 90 */
    if ( number_percent( ) > 85 && number_percent( )
     < victim->pcdata->learned[gsn_redirect] &&
     ( vision_impared( victim ) &&
       victim->pcdata->learned[gsn_blindfight] > number_percent( ) ) )
    {
     if ( victim->pcdata->attitude == -14 )
     {
     act( AT_RED, "$N avoids your attack and lands a hard blow to your back!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N sidesteps $n's attack and lands a massive blow to $s back.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You avoid $n's attack and land a massive blow to $s back.", ch, NULL, victim, TO_VICT );

           dam = ( get_curr_str(victim) * number_range( 8, 11 ) );
           damage( victim, ch, dam, gsn_monkey_stance, DAM_INTERNAL, TRUE );
     ch->position = POS_RESTING;

     if ( number_percent() > 75 )
     {
     act( AT_RED, "You feel your spine crack, the pain blinds and paralyzes you!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "You hear a loud cracking sound as $n doubles over.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You feel $n's spine crack from your tremendous blow.", ch, NULL, victim, TO_VICT );

          if ( !is_affected( ch, gsn_blindness ) )
           {
		af.type      = gsn_blindness;
		af.duration  = 5;
		af.location  = APPLY_HITROLL;
		af.modifier  = -50;
		af.bitvector = AFF_BLIND;
		affect_join( ch, &af );
           }

           dam = ( get_curr_str(victim) * number_range( 8, 11 ) );
           damage( victim, ch, dam, gsn_monkey_stance, 0, TRUE );
     }
     return;
     }

     if ( victim->pcdata->attitude == -13 )
     {
     act( AT_RED, "$N catches your attack and throws your arms to the side.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N catches $n's attack and throws $s hands to the side.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You catch $n's attack and toss his hands to the side.", ch, NULL, victim, TO_VICT );
     return;
     }
    }

    if ( number_percent( ) > 90 && number_percent( )
     < victim->pcdata->learned[gsn_redirect] &&
     ( vision_impared( victim ) &&
       victim->pcdata->learned[gsn_blindfight] > number_percent( ) ) )
    {
     if ( victim->pcdata->attitude == 13 )
     {
     act( AT_RED, "$N catches your attack and throws your arms to the side.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N catches $n's attack and throws $s hands to the side.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You catch $n's attack and toss his hands to the side.", ch, NULL, victim, TO_VICT );
     return;
     }
     if ( victim->pcdata->attitude == -12 )
     {
     act( AT_RED, "$N sidesteps your attack and kicks you into the ground.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$N sidesteps $n's attack and kicks $m into the ground.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "You sidestep $n's attack and land a kick to $s back that sends $m flying!", ch, NULL, victim, TO_VICT );
     dam = ( get_curr_str(victim) * number_range( 5, 8 ) );
     damage( victim, ch, dam, gsn_crane_stance, 0, TRUE );
     ch->position = POS_RESTING;
     return;
     }      
    }
   }
  }

    if(!IS_NPC(victim) && IS_AFFECTED2( victim, AFF_PMIRROR) )
    {
     if ( number_percent( ) > 90 )
     {
     act( AT_RED, "You attack $N, but your hand vanishes and comes right back at you!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n attacks $N, but $s hand turns around and hits $mself.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "Your physical mirror redirects $n's attack!", ch, NULL, victim, TO_VICT );
     damage( victim, ch, dam, dt, immune, TRUE );
     }
    }
   }
  }

    /*
     * Hit.
     * Calc damage.
     */
   if ( dam > 0 )
   {
    dam = (check_item_damage( ch, victim, NULL, dam, immune ));
    inventory_damage( victim, immune, dam );
   }

    if ( ( IS_AFFECTED2( victim, AFF_MANANET ) )
     && ch != victim )    
    {
     victim->mana += dam / 4;
     dam = ( dam / (number_range( 2, 4 )) );
    }

    dam = ((dam * immune_calc( immune, victim )) / 100);

    if ( immune == DAM_HEAT || immune == DAM_COLD )
     weather_magic( immune, ch->in_room );

    if ( dt != TYPE_UNDEFINED )
    {
     if ( (skill) || IS_SET( ch->act, PLR_COMBAT ) )
      dam_message( ch, victim, dam, dt, 0, 1, 1 );
    }

    if ( !(fight_check_tail( ch, victim, dam )) )
     return;

    if( dam > 0 )
     extra_hit( ch, victim, dt );  

    tail_chain( );
    return;
}

void item_damage( int armordam, OBJ_DATA *obj, CHAR_DATA *ch )
{
    char         buf [ MAX_STRING_LENGTH ];
    OBJ_DATA    *obj2;

  obj->durability = (obj->durability - armordam);

  if ( obj->durability >= -5 && obj->durability <= 0 )
  {
   act( AT_RED, "$p breaks apart and falls to the ground!", ch, obj, NULL, TO_CHAR );
   act( AT_RED, "$n's $p cracks apart and falls to the floor!", ch, obj, NULL, TO_ROOM );
   obj_from_char( obj );
   obj_to_room( obj, ch->in_room );
  }
  else if (obj->durability < -5)
  {
   act( AT_RED, "$p shatters, leaving you with a pile of junk.", ch, obj, NULL, TO_CHAR );
   act( AT_RED, "$n's $p shatters, leaving $n with a pile of junk.", ch, obj, NULL, TO_ROOM );
   obj2 = obj;
   extract_obj( obj );  

   obj = create_object(get_obj_index(OBJ_VNUM_PILE_JUNK), 0 );

   sprintf( buf, obj->description, obj2->short_descr );
   free_string( obj->description );
   obj->description = str_dup( buf );

   obj_to_room( obj, ch->in_room );
  }

  return;
}

int check_item_damage( CHAR_DATA *ch, CHAR_DATA *victim, OBJ_DATA *weapon,
 int dam, int damtype )
{
     OBJ_DATA 	*wear;
     char	buf[MAX_STRING_LENGTH];
     int        armordam = 1;
     int        bodyloc = number_percent( );     
     bool	sharp;

   /* I only ask for weapon here so weapons can take damage too */
    if ( (weapon) )
    {
      armordam = (((dam / 10) * 
       material_table[weapon->composition].imm_damage[DAM_BASH]) / 125);
     
      if ( armordam > 0 &&
       !IS_SET( weapon->extra_flags, ITEM_NO_DAMAGE ) )
      {
       sprintf( buf, "Your %s has been damaged!\n\r", weapon->short_descr );
       send_to_char( AT_WHITE, buf, ch );
       armordam = ( armordam / 5);
       item_damage( armordam, weapon, ch );
      }
    }

    if ( damtype == DAM_PIERCE || damtype == DAM_SLASH 
     || damtype == DAM_SCRATCH )
     sharp = TRUE;
    else
     sharp = FALSE;

     if ( bodyloc >= 51 )
     {
      wear = get_eq_char( victim, WEAR_HEAD );

      if ( bodyloc >= 71 )
       wear = get_eq_char( victim, WEAR_ARMS );

        if ( bodyloc >= 81 )
        wear = get_eq_char( victim, WEAR_LEGS );

        if ( bodyloc >= 91 )
        wear = get_eq_char( victim, WEAR_NECK );

        if ( bodyloc >= 95 )
        wear = get_eq_char( victim, WEAR_FEET );

        if ( bodyloc >= 98 )
        wear = get_eq_char( victim, WEAR_WAIST );

        if ( wear == NULL )
         return dam;

        armordam = 1;

       if ( sharp == FALSE )
       {
        if ( wear->value[4] == 0 || wear->value[4] == 2
          || wear->value[4] == 3 || wear->value[4] == 5 )
        {
         armordam = ( ( armordam * ( number_percent() / 2 ) * dam ) / 50);
         dam -= armordam;
        }
        else
         if ( wear->value[4] == 1 || wear->value[4] == 4 
           || wear->value[4] == 6 )
        {
         armordam = ( ( armordam * dam ) / 20);
         if ( damtype < 10 )
          dam -= armordam;
         else      
          dam = 0;
        }
        else
        if ( wear->value[4] == 7 && number_percent() > 50 )
        {
         armordam = ( ( armordam * dam ) / 20);
         if ( damtype < 10 )
          dam -= armordam;
         else      
          dam = 0;
        }
        else
         armordam = 0;
       }
       else
       {
        if ( wear->value[4] == 0 || wear->value[4] == 2
          || wear->value[4] == 3 || wear->value[4] == 4
          || wear->value[4] == 5 )
        {
         armordam = ( ( armordam * dam ) / 20);
         if ( damtype < 10 )
          dam -= armordam;
         else      
         dam = 0;
        }
        else
        if ( (wear->value[4] == 1 || wear->value[4] == 6) &&
             number_percent() > 20 )
         {
          armordam = ( ( armordam * dam ) / 20);
        if ( damtype < 10 )
         dam -= armordam;
        else      
          dam = 0;
         }
         else
        if ( wear->value[4] == 7 && number_percent() > 50 )
         {
          armordam = ( ( armordam * dam ) / 20);
        if ( damtype < 10 )
         dam -= armordam;
        else      
          dam = 0;
         }
         else
          armordam = 0;
       }

        armordam = ((armordam * 
         material_table[wear->composition].imm_damage[damtype]) / 125);
     
         if ( armordam > 0 &&
  	  !IS_SET( wear->extra_flags, ITEM_NO_DAMAGE ) )
         {
          sprintf( buf, "Your %s has been damaged!\n\r", wear->short_descr );
          send_to_char( AT_WHITE, buf, victim );
          armordam = ( armordam / 5);
          item_damage( armordam, wear, victim );
         }
       }
      else
       {
        bodyloc = WEAR_BODY_3;
        while( bodyloc != -1 )
        {
         wear = get_eq_char( victim, bodyloc );

         if ( bodyloc == WEAR_BODY_3 )
          bodyloc = WEAR_BODY_2;
         else
         if ( bodyloc == WEAR_BODY_2 )
          bodyloc = WEAR_BODY_1;
         else
         if ( bodyloc == WEAR_BODY_1 )
          bodyloc = -1;

         if ( wear == NULL )
          continue;

         if ( dam <= 0 )
         {
          dam = 0;
          return dam;
         }
 
        armordam = 1;
        if ( sharp == FALSE )
       {
        if ( wear->value[4] == 0 || wear->value[4] == 2
          || wear->value[4] == 3 || wear->value[4] == 5 )
        {
        armordam = ( ( armordam * ( number_percent() / 2 ) * dam ) / 50);
        dam -= armordam;
        armordam = ( armordam / 5);
        }
        else
        if ( wear->value[4] == 1 || wear->value[4] == 4 
          || wear->value[4] == 6 )
        {
        armordam = ( ( armordam * dam ) / 20);
        if ( damtype < 10 )
         dam -= armordam;
        else      
        dam = 0;
        }
        else
        if ( wear->value[4] == 7 && number_percent() > 50 )
         {
          armordam = ( ( armordam * dam ) / 20);
        if ( damtype < 10 )
         dam -= armordam;
        else      
          dam = 0;
         }
         else
          armordam = 0;
       }
       else
       {
        if ( wear->value[4] == 0 || wear->value[4] == 2
          || wear->value[4] == 3 || wear->value[4] == 4
          || wear->value[4] == 5 )
        {
        armordam = ( ( armordam * dam ) / 20);
        if ( damtype < 10 )
         dam -= armordam;
        else      
        dam = 0;
        }
        else
        if ( (wear->value[4] == 1 || wear->value[4] == 6) &&
             number_percent() > 20 )
         {
          armordam = ( ( armordam * dam ) / 20);
        if ( damtype < 10 )
         dam -= armordam;
        else      
          dam = 0;
         }
         else
        if ( wear->value[4] == 7 && number_percent() > 50 )
         {
          armordam = ( ( armordam * dam ) / 20);
        if ( damtype < 10 )
         dam -= armordam;
        else      
          dam = 0;
         }
         else
          armordam = 0;
       }

        armordam = ((armordam * 
         material_table[wear->composition].imm_damage[damtype]) / 125);

         if ( armordam > 0 &&
  	  !IS_SET( wear->extra_flags, ITEM_NO_DAMAGE ) )
         {
          sprintf( buf, "Your %s has been damaged!\n\r", wear->short_descr );
          send_to_char( AT_WHITE, buf, victim );
          item_damage( armordam, wear, victim );
         }
       }
      }

 return dam;
}

bool is_safe( CHAR_DATA *ch, CHAR_DATA *victim )
{
  CLAN_DATA *pClan;

  if ( IS_ARENA( ch ) || IS_ARENA( victim ) )
   return FALSE;

  if ( IS_NPC(ch) && ch->pIndexData->vnum == MOB_VNUM_SUPERMOB )
   return TRUE;

  if ( (ch->in_room) && (victim->in_room) )
  if ( IS_SET( ch->in_room->room_flags, ROOM_SAFE ) ||
       IS_SET( victim->in_room->room_flags, ROOM_SAFE ) )
    return TRUE;

  if ( !IS_NPC( ch ) && !IS_NPC( victim ) )
   return TRUE;

  if ( !IS_NPC(ch) && !IS_NPC(victim) &&
       (IS_SET(ch->in_room->room_flags, ROOM_NO_PKILL) ||
	IS_SET(victim->in_room->room_flags, ROOM_NO_PKILL)) )
    return TRUE;

  if ( IS_AFFECTED( ch, AFF_PEACE ) )
      return TRUE;

  if ( IS_AFFECTED( victim, AFF_PEACE ) )
      return TRUE;

  if ( IS_SET( ch->in_room->room_flags, ROOM_PKILL ) &&
       IS_SET( victim->in_room->room_flags, ROOM_PKILL ) )
    return FALSE;

  if ( IS_NPC( victim ) )
    return FALSE;

/* SIGH
  if ( !(IS_SET(ch->act, PLR_PKILLER)) || ( (IS_SET(ch->act, PLR_PKILLER)) &&
       !(IS_SET(victim->act, PLR_PKILLER)) ) )
  {
    send_to_char(AT_WHITE, "You cannot pkill unless you are BOTH pkillers!\n\r", ch );
    return TRUE;
  }
*/

/*
  if ( abs(ch->level - victim->level) > 5 && ( !IS_NPC(ch) ) )
  {
    send_to_char(AT_WHITE, "That is not in the pkill range... valid range is +/- 5 levels.\n\r", ch );
    return TRUE;
  }*/

  if ( IS_NPC( ch ) )
  {
    if ( IS_SET(ch->affected_by, AFF_CHARM) && ch->master )
    {
      CHAR_DATA *nch;

      for ( nch = ch->in_room->people; nch; nch = nch->next )
	if ( nch == ch->master )
	  break;

      if ( nch == NULL )
	return FALSE;
      else
	ch = nch; /* Check person who ordered mob for clan stuff.. */
    }
    else
      return FALSE;
  }

  pClan = get_clan_index( ch->clan );

  if ( ( ch->clan == 0 ) && ( !IS_SET(pClan->settings, CLAN_PKILL) ) )
  {
    send_to_char(AT_WHITE, "You must be clanned to murder.\n\r", ch );
    return TRUE;
  }

  pClan = get_clan_index( victim->clan);

  if ( ( victim->clan == 0 ) && ( !IS_SET(pClan->settings, CLAN_PKILL) ) )
  {
    send_to_char(AT_WHITE, "You can only murder clanned players.\n\r",ch);
    return TRUE;
  }

  pClan = get_clan_index( ch->clan );

  if ( ch->clan == victim->clan &&
       IS_SET( pClan->settings, CLAN_CIVIL_PKILL) )
  {
   return FALSE;
  }

/* can murder self for testing =) */
  if ( ch->clan == victim->clan && ch != victim && ch->clan != 0 )
  {
    send_to_char(AT_WHITE, "You cannot murder your own clan member.\n\r",ch);
    return TRUE;
  }

  if ( !IS_SET(pClan->settings, CLAN_PKILL) )
  {
    send_to_char(AT_WHITE, "Peaceful clan members cannot murder.\n\r",ch);
    return TRUE;
  }

  pClan = get_clan_index( victim->clan );
  if ( !IS_SET(pClan->settings, CLAN_PKILL ))
  {
    send_to_char(AT_WHITE, "You may not murder peaceful clan members.\n\r",ch);
    return TRUE;
  }

  if ( IS_SET( victim->act, PLR_KILLER ) )
    return FALSE;

  return FALSE;
}



/*
 * See if an attack justifies a KILLER flag.
 */
void check_killer( CHAR_DATA *ch, CHAR_DATA *victim )
{
    CLAN_DATA *pClan;
    char 	buf [ MAX_STRING_LENGTH ];
    
    if ( !victim )
     return;

    if ( IS_ARENA(victim) )
      return;

    /*
     * NPC's are fair game.
     * So are killers and thieves.
     */
    if (   IS_NPC( victim )
	|| IS_SET( victim->act, PLR_KILLER ) )
	return;
     
    /*
     * NPC's are cool of course
     * Hitting yourself is cool too (bleeding).
     * And current killers stay as they are.
     */
    if ( IS_NPC( ch )
	|| ch == victim
	|| IS_SET( ch->act, PLR_KILLER ) )
	return;
    pClan = get_clan_index( ch->clan );
    if ( /*ch->clan != 0 ||*/ ( IS_SET(pClan->settings, CLAN_PKILL) ) ||
( ch->clan == victim->clan && IS_SET(pClan->settings, CLAN_CIVIL_PKILL)) )
       return;
    send_to_char(AT_RED, "*** You are now a KILLER!! ***\n\r", ch );
    sprintf(buf,"%s is attempting to murder %s",ch->oname, victim->name);
    wiznet(buf,ch,NULL,WIZ_FLAGS,0,0);
    SET_BIT(ch->act, PLR_KILLER);
    save_char_obj( ch, FALSE );
    return;
}

bool is_bare_hand( CHAR_DATA *ch )
{
    if ( !get_eq_char( ch, WEAR_WIELD ) 
    && !get_eq_char( ch, WEAR_WIELD_2 ) )
	return TRUE;
    return FALSE;
}

/*
 * Check to see if weapon is poisoned.
 */
bool is_wielding_poisoned( CHAR_DATA *ch )
{
    OBJ_DATA *obj;

    if ( ( obj = get_eq_char( ch, WEAR_WIELD ) )
	&& IS_SET( obj->extra_flags, ITEM_POISONED ) )
        return TRUE;

    return FALSE;

}
bool is_wielding_flaming( CHAR_DATA *ch )
{
    OBJ_DATA *obj;

    if ( ( obj = get_eq_char( ch, WEAR_WIELD ) )
	&& IS_SET( obj->extra_flags, ITEM_FLAME ) )
        return TRUE;

    return FALSE;

}
bool is_wielding_shock( CHAR_DATA *ch )
{
    OBJ_DATA *obj;

    if ( ( obj = get_eq_char( ch, WEAR_WIELD ) )
	&& IS_SET( obj->extra_flags, ITEM_SHOCK ) )
        return TRUE;

    return FALSE;

}
bool is_wielding_rainbow( CHAR_DATA *ch )
{
    OBJ_DATA *obj;

    if ( ( obj = get_eq_char( ch, WEAR_WIELD ) )
	&& IS_SET( obj->extra_flags, ITEM_RAINBOW ) )
        return TRUE;

    return FALSE;

}

bool is_wielding_icy( CHAR_DATA *ch )
{
    OBJ_DATA *obj;

    if ( ( obj = get_eq_char( ch, WEAR_WIELD ) )
	&& IS_SET( obj->extra_flags, ITEM_ICY ) )
        return TRUE;

    return FALSE;

}
bool is_wielding_chaos( CHAR_DATA *ch )
{
    OBJ_DATA *obj;

    if ( ( obj = get_eq_char( ch, WEAR_WIELD ) )
	&& IS_SET( obj->extra_flags, ITEM_CHAOS ) )
        return TRUE;

    return FALSE;

}


/*
 * Check for parry.
 */
bool check_parry( CHAR_DATA *ch, CHAR_DATA *victim )
{
    char buf[MAX_STRING_LENGTH];
    char buf2[MAX_STRING_LENGTH];
    OBJ_DATA *wield;
    OBJ_DATA *wield2;
    int chance;

    if ( !IS_AWAKE( victim ) )
	return FALSE;

    if ( victim == ch )
     return FALSE;

    if ( IS_NPC( victim ) )
    {
     if ( !(wield = get_eq_char( victim, WEAR_WIELD ) )  )
      return FALSE;
    }
    else
    {
     if ( !(wield = get_eq_char( victim, WEAR_WIELD ) ) &&
         victim->pcdata->learned[gsn_blackbelt] < number_percent()
      && victim->pcdata->claws == 0 )
      return FALSE;
    }

   chance = (( get_curr_agi(victim) + get_curr_dex(victim) ) / 5);

   if ( !IS_NPC( victim ) ) 
   {
    if ( wield )
    {
     if ( wield->value[8] != WEAPON_BLADE )
      chance += ( victim->pcdata->weapon[wield->value[8]] / 10 );
     else
      chance += ( victim->pcdata->weapon[wield->value[7]] / 10 );
    }
   }

    if ( wield && (wield2 = get_eq_char( ch, WEAR_WIELD ) ) )
     chance += ( ( wield->value[4] - wield2->value[4] ) * 2 );


    if ( !IS_NPC(victim) )
    chance -= victim->pcdata->attitude;

    if ( ch->wait != 0 )
      chance /= 4;

    chance += ((get_curr_dex(victim) / 2) - (get_curr_dex(ch) / 2 ));
    chance += ((get_curr_agi(victim) / 2) - (get_curr_agi(ch) / 2 ));

    if ( number_percent( ) >= chance )
	return FALSE;

    if ( IS_NPC(victim) )
    sprintf( buf, "$N parries your attack." );
    else
    sprintf( buf, "%s parries your attack.", victim->oname );

    if ( IS_NPC(ch) )
    sprintf( buf2, "You parry $n's attack." );
    else
    sprintf( buf2, "You parry %s's attack.", ch->oname );

    if ( IS_SET( ch->act, PLR_COMBAT ) )
     act(AT_GREEN, buf, ch, NULL, victim, TO_CHAR    );
    if ( IS_SET( victim->act, PLR_COMBAT ) )
     act(AT_GREEN, buf2, ch, NULL, victim, TO_VICT    );

    if ( !IS_NPC( victim ) )
     if ( victim->pcdata->learned[gsn_blackbelt] > 0 )
      update_skpell( victim, gsn_blackbelt );

   if ( ch->in_room == victim->in_room )
   {
    if ( !IS_NPC( victim ) && wield && wield->item_type == ITEM_WEAPON)
    {
     if ( victim->pcdata->learned[gsn_blade_clash] > number_percent() )
     {
      if ( ( get_curr_str( ch ) + dice( 1, 4 ) ) > 
           ( get_curr_str( victim ) + dice( 1, 3 ) ) )
       disarm( victim, ch );
      update_skpell( victim, gsn_blade_clash );
     }
    }

     if ( !IS_NPC( victim ) && wield && wield->item_type == ITEM_WEAPON)
     {
      if ( victim->pcdata->learned[gsn_ripotse] > dice( 20, 10 ) )
      {
      act(AT_RED, "$N exacts a ripotse!", ch, NULL, victim, TO_CHAR    );
      act(AT_YELLOW, "$N ripotses $n!", ch, NULL, victim, TO_NOTVICT );
      act(AT_YELLOW, "You ripotse!" , ch, NULL, victim, TO_VICT    );

      if ( wield->value[8] == WEAPON_BLADE )
      one_hit( victim, ch, wield->value[7], 1 );
         else
      one_hit( victim, ch, wield->value[3], 1 );
      update_skpell( victim, gsn_ripotse );
      }
     }
    }

    return TRUE;
}



/*
 * Check for dodge.
 */
bool check_dodge( CHAR_DATA *ch, CHAR_DATA *victim )
{
    char buf[MAX_STRING_LENGTH];
    char buf2[MAX_STRING_LENGTH];
    OBJ_DATA *wield;
    int chance;

    if ( !IS_AWAKE( victim ) )
	return FALSE;

    if ( victim == ch )
     return FALSE;

    chance = UMIN( 10, 100 );

    if ( ch->wait != 0 )
      chance /= 4;

    chance += (victim->level - ch->level);
    chance += ((get_curr_dex(victim) / 2) - (get_curr_dex(ch) / 2 ));

    if ( (wield = get_eq_char( ch, WEAR_WIELD )) )
     chance -= (wield->value[4] * 2);

    if ( !IS_NPC( victim ) )
    {
     if ( victim->pcdata->morph[2] == 1 )
     {
      chance += 10;
     }
   
     if ( victim->pcdata->assimilate[ASSIM_EXTRA_1] == OBJ_VNUM_TAIL ||
      victim->pcdata->assimilate[ASSIM_EXTRA_2] == OBJ_VNUM_TAIL ||
      victim->pcdata->assimilate[ASSIM_EXTRA_3] == OBJ_VNUM_TAIL ||
      victim->pcdata->assimilate[ASSIM_EXTRA_4] == OBJ_VNUM_TAIL ||
      victim->pcdata->assimilate[ASSIM_EXTRA_5] == OBJ_VNUM_TAIL )
      chance += 5;
    }

    if ( IS_NPC( victim ) && !IS_NPC( ch ) )
    {
     if ( ch->pcdata->remort > 0 )
     chance += 20;
    }

    if ( number_percent( ) > chance )
        return FALSE;

    if ( IS_NPC(victim) )
    sprintf( buf, "$N dodges your attack." );
    else
    sprintf( buf, "%s dodges your attack.", victim->oname  );
    if ( IS_NPC(ch) )
    sprintf( buf2, "You dodge $n's attack." );
    else
    sprintf( buf2, "You dodge %s's attack.", ch->oname );

    if ( IS_SET( ch->act, PLR_COMBAT ) )
      act(AT_GREEN, buf, ch, NULL, victim, TO_CHAR    );
    if ( IS_SET( victim->act, PLR_COMBAT ) )
      act(AT_GREEN, buf2, ch, NULL, victim, TO_VICT    );
    return TRUE;
}

bool check_sidestep( CHAR_DATA *ch, CHAR_DATA *victim )
{
    char buf[MAX_STRING_LENGTH];
    char buf2[MAX_STRING_LENGTH];
    int chance;

    if ( !IS_AWAKE( victim ) )
        return FALSE;

    if ( victim == ch )
     return FALSE;
    
    if ( IS_NPC( victim ) )
        return FALSE;
    
    if ( victim->pcdata->attitude != -14
     &&  victim->pcdata->attitude != -13 )
        return FALSE;
    
    chance = 20;

    if ( number_percent( ) >= victim->level - ch->level 
    + (get_curr_dex(victim) + chance ) )
        return FALSE;         

    if ( victim->pcdata->attitude == -14 )
    {
    sprintf( buf, "$N wavers about like a drunkard, and avoids your attack." );
    sprintf( buf2, "You waver and avoid $n's attack." );
    }
    else
    if ( victim->pcdata->attitude == -13 )
    {
    sprintf( buf, "$N sidesteps your attack." );
    sprintf( buf2, "You sidestep $n's attack." );
    }

    if ( IS_SET( ch->act, PLR_COMBAT ) )
      act(AT_GREEN, buf, ch, NULL, victim, TO_CHAR    );
    if ( IS_SET( victim->act, PLR_COMBAT ) )
      act(AT_GREEN, buf2, ch, NULL, victim, TO_VICT    );
    return TRUE;
}

bool check_feint( CHAR_DATA *ch, CHAR_DATA *victim )
{
    char buf[MAX_STRING_LENGTH];
    char buf2[MAX_STRING_LENGTH];
    int chance;

    if ( !IS_AWAKE( victim ) )
        return FALSE;
    
    if ( IS_NPC( victim ) )
        return FALSE;

    if ( victim == ch )
     return FALSE;
    
    if ( victim->pcdata->attitude != -14)
        return FALSE;
    
    chance = 20;

    if ( number_percent( ) >= victim->level - ch->level 
    + (get_curr_dex(victim) + chance ))
        return FALSE;         

    sprintf( buf, "$N feints, and you miss him completly." );
    sprintf( buf2, "You feint and avoid $n's attack." );

    if ( IS_SET( ch->act, PLR_COMBAT ) )
      act(AT_GREEN, buf, ch, NULL, victim, TO_CHAR    );
    if ( IS_SET( victim->act, PLR_COMBAT ) )
      act(AT_GREEN, buf2, ch, NULL, victim, TO_VICT    );
    return TRUE;
}
bool check_blink( CHAR_DATA *ch, CHAR_DATA *victim )
{
    char buf[MAX_STRING_LENGTH];
    char buf2[MAX_STRING_LENGTH];
    int chance;

    if ( !IS_AFFECTED2( victim, AFF_BLINK ) )
        return FALSE;

    if ( IS_NPC( victim ) )
        /* Tuan was here.  :) */
        return FALSE;

    if ( victim == ch )
     return FALSE;

    chance = 50;
    
    if ( number_percent( ) >= victim->level - ch->level + chance -
    (get_curr_int(ch) / 2) - (get_curr_wis(ch) / 4) )
        return FALSE;         

    if ( IS_NPC(victim) )
    sprintf( buf, "You swipe at $N, but you hit an illusion!" );
    else
    sprintf( buf, "You swipe at %s, but you hit an illusion!", victim->oname );
    if ( IS_NPC(ch) )
    sprintf( buf2, "$n hits one of your illusions!" );
    else
    sprintf( buf2, "%s hits one of your illusions!", ch->oname );

            
    if ( IS_SET( ch->act, PLR_COMBAT ) )
      act(AT_GREEN, buf, ch, NULL, victim, TO_CHAR );
    if ( IS_SET( victim->act, PLR_COMBAT ) )
      act(AT_GREEN, buf2, ch, NULL, victim, TO_VICT );
    return TRUE;
}
bool check_mirror_images( CHAR_DATA *ch, CHAR_DATA *victim )
{
    char buf[MAX_STRING_LENGTH];
    char buf2[MAX_STRING_LENGTH];
    int chance;

    if ( !IS_AFFECTED2( victim, AFF_MIRROR_IMAGES ) )
        return FALSE;

    if ( victim == ch )
     return FALSE;

    if ( IS_NPC( victim ) )
        /* Tuan was here.  :) */
        return FALSE;

    chance  = 50;
    
    
    if ( number_percent( ) >= ch->level - victim->level + chance +
    (get_curr_int(ch) / 2) - (get_curr_int(victim) / 4) -
    (get_curr_wis(victim) / 4) )
        return FALSE;         

    if ( IS_NPC(victim) )
    sprintf( buf, "You swipe at $N, but you hit an illusion!" );
    else
    sprintf( buf, "You swipe at %s, but you hit an illusion!", victim->oname );
    if ( IS_NPC(ch) )
    sprintf( buf2, "$n hits one of your illusions!" );
    else
    sprintf( buf2, "%s hits one of your illusions!", ch->oname );
            
    if ( IS_SET( ch->act, PLR_COMBAT ) )
      act(AT_GREEN, buf, ch, NULL, victim, TO_CHAR );
    if ( IS_SET( victim->act, PLR_COMBAT ) )
      act(AT_GREEN, buf2, ch, NULL, victim, TO_VICT );
    return TRUE;
}

/*
 * Set position of a victim.
 */
void update_pos( CHAR_DATA *victim, CHAR_DATA *ch )
{
    if ( victim->hit > 0 )
    {
     if ( victim->position <= POS_STUNNED )
      {
       if ( victim->fighting )
        victim->position = POS_FIGHTING;
       else
	victim->position = POS_STANDING;
      }

        if (IS_STUNNED( victim, STUN_TOTAL ) )
        {
         victim->position = POS_STUNNED;
         return;
        }

        if ( IS_AFFECTED2( victim, AFF_TORTURED ) &&
             IS_AFFECTED2( ch, AFF_TORTURING) )
        {

        if (IS_STUNNED( victim, STUN_TOTAL ) )
        {
         victim->position = POS_STUNNED;
         return;
        }

        REMOVE_BIT( victim->affected_by2, AFF_TORTURED );
        REMOVE_BIT( ch->affected_by2, AFF_TORTURING );
        send_to_char( AT_YELLOW, "You break free of your bonds and attack your torturer!\n\r", victim);
        send_to_char( AT_YELLOW, "They break free of their bonds and attack you, WATCH OUT!\n\r", ch);
        damage(victim, ch, 75, gsn_torture, 0, TRUE );
        }
	return;
    }

    if ( IS_NPC( victim ) || victim->hit <= -11 )
    {
	victim->position = POS_DEAD;
	return;
    }

         if ( victim->hit <= -6 ) victim->position = POS_MORTAL;
    else if ( victim->hit <= -3 ) victim->position = POS_INCAP;
    else                          victim->position = POS_STUNNED;

    return;
}

/*
 * Start fights.
 */
void set_fighting( CHAR_DATA *ch, CHAR_DATA *victim )
{
   if ( IS_AFFECTED( ch, AFF_SLEEP ) )
    affect_strip( ch, gsn_sleep );

   ch->fighting = victim;

   if ( !IS_STUNNED( ch, STUN_TOTAL ) )
    ch->position = POS_FIGHTING;

   update_pos( ch, victim );
 return;
}

/*
 * Stop fights.
 */
void stop_fighting( CHAR_DATA *ch, bool fBoth )
{
    CHAR_DATA *fch;

    for ( fch = char_list; fch; fch = fch->next )
    {
     if ( fch == ch || ( fBoth && fch->fighting == ch ) )
     {
         bool		gfound = FALSE;
         AFFECT_DATA	*paf;

	    fch->fighting		= NULL;
	    fch->hunting        	= NULL;
	    fch->position		= POS_STANDING;

           if ( !IS_NPC(fch) )
	    fch->pcdata->twin_fighting	= NULL;

	    if ( is_affected( fch, gsn_grapple ) )	    
	      affect_strip( fch, gsn_grapple );

	    for( paf = fch->affected2; paf; paf = paf->next )
    	    {
	     if (paf->deleted)
	      continue;

	     if ( skill_table[paf->type].grasp == TRUE )
             {
              gfound = TRUE;
	      break;
             }
	    }       

            if ( gfound )
             affect_remove( fch, paf );

	    if ( IS_AFFECTED2(fch, AFF_GRASPING) )
              REMOVE_BIT(fch->affected_by2, AFF_GRASPING);

	    if ( IS_AFFECTED2(fch, AFF_GRASPED) )
              REMOVE_BIT(fch->affected_by2, AFF_GRASPED);

	    if ( is_affected( fch, gsn_berserk ) )
	    {
	      affect_strip( fch, gsn_berserk );
	      send_to_char(C_DEFAULT, skill_table[gsn_berserk].msg_off,fch);
	      send_to_char(C_DEFAULT, "\n\r",fch);
	      
	      act(C_DEFAULT, skill_table[gsn_berserk].room_msg_off,fch,
		  NULL, NULL, TO_ROOM);
	    }

	    if ( IS_AFFECTED2(fch, AFF_BERSERK) )
              REMOVE_BIT(fch->affected_by2, AFF_BERSERK);
	    update_pos( fch, ch );
	}
    }

    return;
}

/*
 * Make a corpse out of a character.
 */
void make_corpse( CHAR_DATA *ch )
{
    OBJ_DATA        *corpse;
    OBJ_DATA        *obj;
    OBJ_DATA        *obj_next;
    char            *name;
    char             buf [ MAX_STRING_LENGTH ];

    /* No corpses in the arena. -- Altrag */
    if ( IS_ARENA(ch) )
    {
      CHAR_DATA *gch = (ch == arena.fch ? arena.sch : arena.fch);
      int award;
     
      if ( gch == NULL )
       ;
      else
      { 
      /* Arena master takes 1/5.. *wink* */
      award = (arena.award * 4) / 5;
      sprintf(log_buf, "&C%s &chas been awarded &W%d &ccoins for %s victory.",
              gch->name, award, (gch->sex == SEX_NEUTRAL ? "its" :
              (gch->sex == SEX_MALE ? "his" : "her")));
      log_string(log_buf, CHANNEL_LOG, -1);
      challenge(log_buf, 0, 0);
      gch->money.gold += award;
      sprintf(log_buf, "You have been awarded %d gold coins for your victory."
              "\n\r", award);
      send_to_char(AT_YELLOW, log_buf, gch);
      char_from_room(gch);
      if ( gch->race == RACE_HUMAN )
       char_to_room(gch, get_room_index(ROOM_HUMAN_TEMPLE));
      else
       char_to_room(gch, get_room_index(ROOM_ELF_TEMPLE));
      return;
     }
    }

    if ( !IS_NPC( ch ) && ch->level <= 20 )
    {
     /* newbie area morgue */
     if ( ch->in_room->vnum > 4299 && ch->in_room->vnum < 4400 )
     {
      char_from_room( ch );
      char_to_room( ch, get_room_index(ROOM_HUMAN_NEWBIE_MORGUE) );
     }
     else
     {
      char_from_room( ch );
      if ( ch->race != RACE_HUMAN )
       char_to_room( ch, get_room_index(ROOM_ELF_MORGUE) );
      else
       char_to_room( ch, get_room_index(ROOM_HUMAN_MORGUE) );
     }
    } 
    if ( IS_NPC( ch ) )
    {
        /*
	 * This longwinded corpse creation routine comes about because
	 * we dont want anything created AFTER a corpse to be placed  
	 * INSIDE a corpse.  This had caused crashes from obj_update()
	 * in extract_obj() when the updating list got shifted from
	 * object_list to obj_free.          --- Thelonius (Monk)
	 */
	if ( (ch->money.gold > 0) || (ch->money.silver > 0) ||
	     (ch->money.copper > 0) )
	{
	    OBJ_DATA * coins;
	    coins	  = create_money( &ch->money );
	    name	  = ch->short_descr;
	    corpse	  = create_object(
					  get_obj_index( OBJ_VNUM_CORPSE_NPC ),
					  0 );
	    corpse->timer = number_range( 15, 30 );
	    obj_to_obj( coins, corpse );
	    ch->money.gold = ch->money.silver = ch->money.copper = 0;
	}
	else
	{
	    name	  = ch->short_descr;
	    corpse	  = create_object(
					  get_obj_index( OBJ_VNUM_CORPSE_NPC ),
					  0 );
	    corpse->timer = number_range( 25, 50 );
	}
     corpse->value[0] = ch->race_type;
     corpse->value[1] = ch->size;
     sprintf( buf, ch->short_descr );
     free_string( corpse->ownedby );
     corpse->ownedby = str_dup( buf );
    }
    else
    {
	name		= ch->name;
	corpse		= create_object(
					get_obj_index( OBJ_VNUM_CORPSE_PC ),
					0 );
	corpse->timer	= number_range( 25, 40 );
/* Check if ch has any money, doesn't matter about converting */

	if ( ( ( ch->money.gold + ch->money.silver + 
		 ch->money.copper ) > 0 ) &&
	        ( ch->level > 5 ) )
	{
	  OBJ_DATA * coins;
	  coins = create_money( &ch->money );
	  obj_to_obj( coins, corpse );
	  ch->money.gold = ch->money.silver = ch->money.copper = 0;
	}
    }

    sprintf( buf, corpse->short_descr, name );
    free_string( corpse->short_descr );
    corpse->short_descr = str_dup( buf );

    sprintf( buf, corpse->description, name );
    free_string( corpse->description );
    corpse->description = str_dup( buf );

    for ( obj = ch->carrying; obj; obj = obj_next )
    {
	obj_next = obj->next_content;

        if ( obj->deleted )
	    continue;

        if ( obj->ownedby != NULL )
        {
         if ( str_cmp( obj->ownedby, ch->name ) )
         {
 	  obj_from_char( obj );

	  if ( IS_SET( obj->extra_flags, ITEM_INVENTORY ) )
	    extract_obj( obj );
	  else
	    obj_to_obj( obj, corpse );
         }
         else
         {
          obj_from_char( obj );
          if ( ch->race == RACE_HUMAN )
           obj_to_room( obj, get_room_index( ROOM_HUMAN_MORGUE ) );
          else
           obj_to_room( obj, get_room_index( ROOM_ELF_MORGUE ) );
         }
        }
        else
         {
 	  obj_from_char( obj );

	  if ( IS_SET( obj->extra_flags, ITEM_INVENTORY ) )
	    extract_obj( obj );
	  else
	    obj_to_obj( obj, corpse );
         }

    }

    if ( ( IS_NPC( ch ) ) && ( !IS_SET( ch->act, UNDEAD_TYPE( ch ) ) ) )
       corpse->invoke_vnum=ch->pIndexData->vnum;
    obj_to_room( corpse, ch->in_room );
    if ( !IS_NPC( ch ) )
    corpse_back( ch, corpse );
    
    return;
}

/*
 * Improved Death_cry contributed by Diavolo.
 */
void death_cry( CHAR_DATA *ch )
{
    ROOM_INDEX_DATA *was_in_room;
    char            *msg;
    int              vnum;
    int              door;
    OBJ_DATA *obj;

    if ( IS_ARENA(ch) )
      return;

    vnum = 0;

  msg = "";

    switch ( number_bits( 4 ) )
    {
    default: msg  = "You hear $n's death cry.";				break;
    case  0: msg  = "$n hits the ground ... DEAD.";			break;
    case  1: msg  = "$n splatters blood on your armor.";		break;
    }

    act(AT_BLOOD, msg, ch, NULL, NULL, TO_ROOM );

    if ( vnum != 0 )
    {
	char     *name;
	char      buf [ MAX_STRING_LENGTH ];

	name		= IS_NPC( ch ) ? ch->short_descr : ch->name;
	obj		= create_object( get_obj_index( vnum ), 0 );
	obj->timer	= number_range( 4, 7 );

	sprintf( buf, obj->short_descr, name );
	free_string( obj->short_descr );
	obj->short_descr = str_dup( buf );

	sprintf( buf, obj->description, name );
	free_string( obj->description );
	obj->description = str_dup( buf );

	obj_to_room( obj, ch->in_room );
    }

    obj		= create_object( get_obj_index(OBJ_VNUM_FINAL_TURD), 0 );
    obj->timer	= number_range( 3, 5 );
    obj_to_room( obj, ch->in_room );
    if ( IS_NPC( ch ) )
	msg = "You hear something's death cry.";
    else
	msg = "You hear someone's death cry.";

   if ( ch->in_room != NULL )
    was_in_room = ch->in_room;
   else
    was_in_room = get_room_index( 1 );

    for ( door = 0; door <= 5; door++ )
    {
	EXIT_DATA *pexit;

	if ( ( pexit = was_in_room->exit[door] )
	    && pexit->to_room
	    && pexit->to_room != was_in_room )
	{
	    ch->in_room = pexit->to_room;
	    act(AT_BLOOD, msg, ch, NULL, NULL, TO_ROOM );
	}
    }
    ch->in_room = was_in_room;

    return;
}



void raw_kill( CHAR_DATA *ch, CHAR_DATA *victim )
{
    AFFECT_DATA *paf;
    AFFECT_DATA *paf_next;
    bool is_arena = IS_ARENA(ch);

    stop_fighting( victim, TRUE );
    stop_shooting( victim, TRUE );
    if ( ch != victim )
        mprog_death_trigger( victim, ch );
    rprog_death_trigger( victim->in_room, victim );
    make_corpse( victim );

    for ( paf = victim->affected; paf; paf = paf_next )
    {
      paf_next = paf->next;
      affect_remove( victim, paf );
    }

    for ( paf = victim->affected2; paf; paf = paf_next )
    {
     paf_next = paf->next;
     affect_remove2( victim, paf );
    }

    victim->affected_by	= 0;
    victim->affected_by2 = 0;

    if ( !IS_NPC( victim ) )
    {
     victim->pcdata->condition[COND_FULL] = 96;
     victim->pcdata->condition[COND_THIRST] = 96;
     victim->pcdata->condition[COND_INSOMNIA] = 96;
     victim->pcdata->regen = 100;
    }

    

    if ( IS_AFFECTED2( ch, AFF_GRASPING ) )
	REMOVE_BIT( ch->affected_by2, AFF_GRASPING );

    if ( IS_AFFECTED2( ch, AFF_GRASPED ) )
 	REMOVE_BIT( ch->affected_by2, AFF_GRASPED );

    if ( IS_NPC( victim ) )
    {
	victim->pIndexData->killed++;
	kill_table[URANGE( 0, victim->level, MAX_LEVEL-1 )].killed++;
	extract_char( victim, TRUE );
	return;
    }

    extract_char( victim, FALSE );
    if ( !is_arena )
    {
      victim->saving_throw = 0;
      victim->carry_weight = 0;
      victim->carry_number = 0;
    }

    victim->position     = POS_RESTING;
    victim->hit	         = UMAX( 1, victim->hit  );
    victim->mana         = UMAX( 1, victim->mana );
    victim->move         = UMAX( 1, victim->move );
    save_char_obj( victim, FALSE );
    return;
}

void group_gain( CHAR_DATA *ch, CHAR_DATA *victim )
{
    CHAR_DATA *gch;
    CHAR_DATA *lch;
    char       buf[ MAX_STRING_LENGTH ];
    int        members;
    int        xp;
    float      xp2;

    /*
     * Monsters don't get kill xp's or alignment changes.
     * P-killing doesn't help either.
     * Dying of mortal wounds or poison doesn't give xp to anyone!
     */
    if ( victim == ch )
	return;
    
    members = 0;
    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
	if ( is_same_group( gch, ch ) )
	 members += 1;;
    }

    if ( members == 0 )
    {
	bug( "Group_gain: members.", members );
	members = 1;
    }

    lch = ( ch->leader ) ? ch->leader : ch;

    for ( gch = ch->in_room->people; gch; gch = gch->next_in_room )
    {
	OBJ_DATA *obj;
	OBJ_DATA *obj_next;

	if ( !is_same_group( gch, ch ) )
	    continue;

        if ( IS_NPC(gch) )
         continue;

	if ( gch->level - lch->level >= 10 )
	{
	    send_to_char(AT_BLUE, "You are too high level for this group.\n\r", gch );
	    continue;
	}

	if ( gch->level - lch->level <= -15 )
	{
	    send_to_char(AT_BLUE, "You are too low level for this group.\n\r",  gch );
	    continue;
	}

        xp2 = (4 / (3 * members));
	xp = (xp_compute( gch, victim ) / members);
	sprintf( buf, "You receive %d experience points.\n\r", xp );
	send_to_char(AT_WHITE, buf, gch );
	gain_exp( gch, xp );

	for ( obj = ch->carrying; obj; obj = obj_next )
	{
	    obj_next = obj->next_content;
	    if ( obj->deleted )
	        continue;
	    if ( IS_SET( obj->wear_loc, WEAR_NONE) )
		continue;
	}
    }

    return;
}



/*
 * Compute xp for a kill.
 * alginment has been taken out <> Swift <>
 * Edit this function to change xp computations.
 * NOTE: New xp system by Hannibal
 */
int xp_compute( CHAR_DATA *gch, CHAR_DATA *victim )
{
    int xp;
    int xp_cap = 850;

    /* mob lvl is 5 lvls lower than pc or more */
    if ( victim->level + 5 <= gch->level )
     return 0;

    /* 3-4 levels lower */
    if ( victim->level + 3 == gch->level 
     || victim->level + 4 == gch->level )
    {
     xp = ( gch->level < 10 ) ? number_range( 25, 50 ) + 10 : 0;
     return xp;
    }

    /* if same lvl or up to 2 lvls lower */
    if ( victim->level > gch->level - 3 
     && victim->level <= gch->level )
    {
     xp = number_range( 10, 50 ) + number_range( 25, 75 );
     return xp;
    }

    /* if higher lvl then... */
    xp = ( victim->level - gch->level ) * dice(30, 7);

    /* if they kill 5 lvls bigger then them or more add 0-100xp */
    xp = ( victim->level >= gch->level + 5 ) ? xp + number_range( 0, 100 )
					     : xp;
    /* Enforce xp cap */
    xp = UMIN( xp_cap, xp );
    xp = UMAX( 0, xp );

    return xp;
}


void dam_message( CHAR_DATA *ch, CHAR_DATA *victim, int dam, int dt, int miss, int numhit, int special )
{
    int perdam = 0;
    const  char         *vs;
    const  char         *vp;
    const  char         *attack;
           char          buf1         [ 256 ];
           char          buf2         [ 256 ];
           char          buf3         [ 256 ];
           char          buf4         [ 256 ];
           char          buf5         [ 256 ];
           char          punct;

    if ( victim->hit >= 1 )
     perdam = ((dam * 100000) / victim->hit);
    else
     perdam += 150000;

     if ( miss == 0 )
     {
         if ( perdam <      0 ) { vs = "healed"; vp = "healed"; }
    else if ( perdam ==     0 ) { vs = "&Ydidn't even scratch";
                                  vp = "didn't even scratch";           }
    else if ( perdam <=   400 ) { vs = "scratch";	vp = "scratches";}
    else if ( perdam <=   800 ) { vs = "graze";	vp = "grazes";		}
    else if ( perdam <=  1200 ) { vs = "hit";	vp = "hits";		}
    else if ( perdam <=  1600 ) { vs = "injure";	vp = "injures";}
    else if ( perdam <=  2000 ) { vs = "wound";	vp = "wounds";		}
    else if ( perdam <=  2400 ) { vs = "maul";       vp = "mauls";}
    else if ( perdam <=  2800 ) { vs = "decimate";	vp = "decimates";}
    else if ( perdam <=  3200 ) { vs = "devastate";	vp ="devastates";}
    else if ( perdam <=  3600 ) { vs = "maim";	vp = "maims";		}
    else if ( perdam <=  4000 ) { vs = "&RMUTILATE&X";vp="&RMUTILATES&X";}
    else if ( perdam <=  4400 ) { vs ="&RDISEMBOWEL&X";vp="&RDISEMBOWELS&X";	}
    else if ( perdam <=  4800 ) { vs = "&REVISCERATE&X";vp="&REVISCERATES&X";	}
    else if ( perdam <=  5200 ) { vs = "&RMASSACRE&X";vp ="&RMASSACRES&X";}
    else if ( perdam <= 10000 ) { vs = "&r*** &RDEMOLISH&r ***&X";
			     vp = "&r*** &RDEMOLISHES&r ***&X"; }
    else if ( perdam <= 15000 ) { vs = "&c*** &RDEVASTATE&c ***&X";
			     vp = "&c*** &RDEVASTATES&c ***&X";}
    else if ( perdam <= 25000 ) { vs = "&g*** &ROBLITERATE&g ***&X";
			     vp = "&g*** &ROBLITERATES&g ***&X";}
    else if ( perdam <= 30000 ) { vs = "&z*** &RANNIHILATE&z ***&X";
			     vp = "&z*** &RANNIHILATES&z ***&X";}
    else if ( perdam <= 50000 ) { vs = "&B<&G> &RSLA&rUGH&RTER &G<&B>&X";
			     vp = "&B<&G> &RSLA&rUGHT&RERS &G<&B>&X";}
    else if ( perdam <= 60000 ) { vs = "&C--> &RN&YU&GK&BE &C<--&X";
			     vp = "&C--> &RN&YU&GK&BE&PS &C<--&X";}
    else if ( perdam <= 70000 ) { vs = "&R<-- &YERADICATE &R-->&X";
			     vp = "&R<-- &YERADICATES &R-->&X"; }
    else if ( perdam <= 80000 ) { vs = "&Pkick the &YPISS&P out of&X";
			     vp = "&Pkicks the &YPISS&P out of&X";}
    else if ( perdam <= 90000 ) { vs = "&Blay &Gsome &RHURT &Gdown &Bon&X"; 
                             vp = "&Blays &Gsome &RHURT &Gdown &Bon&X";}
    else if ( perdam <= 100000) { vs = "&Yrip &Gt&gh&Ge &Rg&ru&Rt&rs &Go&gu&Gt &Yof&X";
                             vp = "&Yrips &Gt&gh&Ge &Rg&rut&Rs &Go&gu&Gt &Yof&X";}
    else if ( perdam <= 150000) { vs = "&Bo&CO&Bo &RLi&rq&Ru&ri&rfy &Bo&CO&Bo";
                             vp = "&Bo&CO&Bo &RLi&rq&Ru&ri&Rfys &Bo&CO&Bo";}
    else                   { vs = "&W+-&RI&z===>&YCastrate&z<===&RI&W-+";                    
                             vp = "&W+-&RI&z===>&YCastrates&z<===&RI&W-+";} 
    }
    else  { vs = "miss"; vp = "misses"; }

    if ( perdam == 0 )
    numhit = 0;

    punct   = ( dam <= 24 ) ? '.' : '!';

    if ( dt == TYPE_HIT )
    {
     if ( ch->level > LEVEL_MORTAL )
	sprintf( buf1, "&Y{%d}&X You &r%s&X $N%c &G{%d}", dam, vs, punct, numhit );
     else
	sprintf( buf1, "&XYou &r%s&X $N%c &G{%d}", vs, punct, numhit );

     if ( victim->level > LEVEL_MORTAL )
	sprintf( buf2, "&R{%d}&X $n &G%s&X you%c &G{%d}", dam, vp, punct, numhit );
     else
	sprintf( buf2, "&X$n &G%s&X you%c &G{%d}", vp, punct, numhit );

	sprintf( buf3, "$n &z%s&X $N%c &G{%d}", vp, punct, numhit );

     if ( ch->level > LEVEL_MORTAL )
	sprintf( buf4, "&R{%d}&X You &G%s&X yourself%c &G{%d}", dam, vs, punct, numhit );
     else
	sprintf( buf4, "&XYou &G%s&X yourself%c &G{%d}", vs, punct, numhit );

	sprintf( buf5, "&X$n &z%s&X $mself%c &G{%d}", vp, punct, numhit );
    }
    else
    {
     if ( dt < 900 )
     {
      if ( is_sn(dt) && special != 0 )
       attack = skill_table[dt].noun_damage;
      else
       attack = flag_string( weapon_flags, dt );
     }
     else
      attack = flag_string( weapon_flags, dt );

     if ( ch->level > LEVEL_MORTAL )
      sprintf( buf1, "&Y{%d}&X Your %s &r%s&X $N%c &G{%d}", dam,
       attack, vp, punct, numhit );
     else
      sprintf( buf1, "&XYour %s &r%s&X $N%c &G{%d}",
       attack, vp, punct, numhit );

     if ( victim->level > LEVEL_MORTAL )
      sprintf( buf2, "&R{%d}&X $n's %s &G%s&X you%c &G{%d}", dam, 
       attack, vp, punct, numhit );
     else
      sprintf( buf2, "$n's %s &G%s&X you%c &G{%d}", 
       attack, vp, punct, numhit );

      sprintf( buf3, "$n's %s &z%s&X $N%c &G{%d}", attack, vp,
       punct, numhit );

     if ( ch->level > LEVEL_MORTAL )
      sprintf( buf4, "&R{%d}&X &WThe %s &G%s&X you%c &G{%d}", dam, 
       attack, vp, punct, numhit );
     else
      sprintf( buf4, "&WThe %s &G%s&X you%c &G{%d}", 
       attack, vp, punct, numhit );

      sprintf( buf5, "&WThe %s &z%s&X $n%c &G{%d}", 
       attack, vp, punct, numhit );
    }

    if ( victim != ch )
    {
     act(AT_WHITE, buf1, ch, NULL, victim, TO_CHAR    );
     act(AT_WHITE, buf2, ch, NULL, victim, TO_VICT    );
     act(AT_GREY, buf3, ch, NULL, victim,  TO_NOTVICT );
    }
    else
    {
     act(AT_WHITE, buf4, ch, NULL, victim, TO_CHAR    );
     act(AT_GREY, buf5, ch, NULL, victim,  TO_NOTVICT );
    }

    return;
}

/*
 * Disarm a creature.
 * Caller must check for successful attack.
 */
void disarm( CHAR_DATA *ch, CHAR_DATA *victim )
{
    OBJ_DATA *obj;

    if ( !( obj = get_eq_char( victim, WEAR_WIELD ) ) )
	if ( !( obj = get_eq_char( victim, WEAR_WIELD_2 ) ) )
	   return;


   if ( IS_NPC( ch ) )
   {
    if ( !get_eq_char( ch, WEAR_WIELD ) && number_bits( 1 ) == 0 )
	if ( !get_eq_char( ch, WEAR_WIELD_2 ) && number_bits( 1 ) == 0 )
	   return;
   }
   else if ( !is_class( ch, CLASS_ASSASSIN ) )
   {
    if ( !get_eq_char( ch, WEAR_WIELD ) && number_bits( 1 ) == 0 )
	if ( !get_eq_char( ch, WEAR_WIELD_2 ) && number_bits( 1 ) == 0 )
	   return;
   }

    act(AT_YELLOW, "You disarm $N!",  ch, NULL, victim, TO_CHAR    );
    act(AT_YELLOW, "$n DISARMS you!", ch, NULL, victim, TO_VICT    );
    act(AT_GREY, "$n DISARMS $N!",  ch, NULL, victim, TO_NOTVICT );


    if ( IS_SET( obj->extra_flags, ITEM_SOUL_BOUND ) )
    {
     act(AT_RED,
      "$N's weapon jumps back into $S hand!",
	ch, NULL, victim, TO_CHAR );
     act(AT_YELLOW,
      "Your weapon leaps back into your hand!",
	ch, NULL, victim, TO_VICT );
     act(AT_YELLOW,
      "$N's weapon jumps back into $S hand!",
	ch, NULL, victim, TO_NOTVICT );
     return;
    }

    if ( IS_SET( obj->extra_flags, ITEM_WIRED ) )
    {
     if ( number_percent() > 95 )
     {
      act(AT_YELLOW,
       "The wire on $N's blade snaps and it falls to the ground!",
	ch, NULL, victim, TO_CHAR );
      act(AT_RED,
       "The wire on your weapon snaps and it falls to the ground!",
	ch, NULL, victim, TO_VICT );
      act(AT_YELLOW,
       "The wire on $N's weapon snaps and it falls to the ground!",
	ch, NULL, victim, TO_NOTVICT );
     }
     else
     {
      act(AT_RED,
       "The wire on $N's blade recoils and it pops back into $S hand!",
	ch, NULL, victim, TO_CHAR );
      act(AT_YELLOW,
       "The wire on your weapon recoils and it hops back into your hand!",
	ch, NULL, victim, TO_VICT );
      act(AT_YELLOW,
       "The wire on $N's weapon recoils and it pops back into $S hand!",
	ch, NULL, victim, TO_NOTVICT );
      return;
     }
    }    

    obj_from_char( obj );
    obj_to_room( obj, victim->in_room );

    return;
}



/*
 * Trip a creature.
 * Caller must check for successful attack.
 */
void trip( CHAR_DATA *ch, CHAR_DATA *victim )
{
    if ( ( IS_AFFECTED( victim, AFF_FLYING ) )
       || ( (get_race_data(ch->race))->flying == 1 ) )
       return;
       
    if ( !IS_STUNNED( victim, STUN_COMMAND ) && !IS_STUNNED(ch, STUN_TO_STUN) )
    {
	act(AT_CYAN, "You trip $N and $N goes down!", ch, NULL, victim, TO_CHAR    );
	act(AT_CYAN, "$n trips you and you go down!", ch, NULL, victim, TO_VICT    );
	act(AT_GREY, "$n trips $N and $N goes down!", ch, NULL, victim, TO_NOTVICT );

	WAIT_STATE( ch, PULSE_VIOLENCE );
	STUN_CHAR(victim, 2, STUN_COMMAND);
	STUN_CHAR(ch, 4, STUN_TO_STUN);
	victim->position = POS_RESTING;
    }

    return;
}
void anklestrike( CHAR_DATA *ch, CHAR_DATA *victim )
{
    if ( ( IS_AFFECTED( victim, AFF_FLYING ) )
       || ( (get_race_data(ch->race))->flying == 1 ) )
       return;
       
    if ( !IS_STUNNED( victim, STUN_COMMAND ) && !IS_STUNNED(ch, STUN_TO_STUN) )
    {
	act(AT_CYAN, "You kick $N's ankle and $E crumbles to the floor.", ch, NULL, victim, TO_CHAR );
	act(AT_CYAN, "$n kicks your ankle and you hit the ground!", ch, NULL, victim, TO_VICT );
	act(AT_GREY, "$n kicks $N's ankle and $E goes down!", ch, NULL, victim, TO_NOTVICT );

	WAIT_STATE( ch, PULSE_VIOLENCE );
	STUN_CHAR(victim, 2, STUN_COMMAND);
	STUN_CHAR(ch, 4, STUN_TO_STUN);
	victim->position = POS_RESTING;
    }

    return;
}

void do_kill( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );
 
    if ( !( victim = get_char_room( ch, arg ) ) )
    {
      send_to_char( AT_WHITE, "That person is not here.\n\r", ch );
      return;
    }

   if ( IS_ARENA( ch ) || IS_ARENA( victim ) )
    ;
   else
    if ( is_safe( ch, victim ) )
      {
       send_to_char( AT_WHITE, "You cannot.\n\r", ch );
       return;
      }	   
    
    if ( arg[0] == '\0' )
    {
	send_to_char(AT_WHITE, "Kill whom?\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
	send_to_char(AT_WHITE, "They aren't here.\n\r", ch );
	return;
    }

    if ( IS_AFFECTED(victim, AFF_PEACE ))
    {
      send_to_char(AT_WHITE, "A wave of peace overcomes you.\n\r", ch);
      return;
    }
    if ( IS_AFFECTED( ch, AFF_PEACE ) )
    {
	    affect_strip( ch, skill_lookup("aura of peace" ));
	    REMOVE_BIT( ch->affected_by, AFF_PEACE );
    }
    if ( !IS_NPC( victim ) && !IS_ARENA(victim) )
    {
	if (   !IS_SET( victim->act, PLR_KILLER ) )
	{
	    send_to_char(AT_WHITE, "You must MURDER a player.\n\r", ch );
	    return;
	}
    }
    else
    {
	if ( IS_AFFECTED( victim, AFF_CHARM ) && victim->master )
	{
	    send_to_char(AT_WHITE, "You must MURDER a charmed creature.\n\r", ch );
	    return;
	}
    }

    if ( victim == ch )
    {
	send_to_char(AT_RED, "You hit yourself.  Stupid!\n\r", ch );
	multi_hit( ch, ch, TYPE_UNDEFINED );
	return;
    }

    if ( IS_AFFECTED( ch, AFF_CHARM ) && ch->master == victim )
    {
	act(AT_BLUE, "$N is your beloved master!", ch, NULL, victim, TO_CHAR );
	return;
    }

    if ( ch->position == POS_FIGHTING )
    {
	send_to_char(C_DEFAULT, "You do the best you can!\n\r", ch );
	return;
    }

    WAIT_STATE( ch, PULSE_VIOLENCE );
    check_killer( ch, victim );
    if ( IS_NPC( victim ) && ch != victim )
    victim->memory = ch;
    multi_hit( ch, victim, TYPE_UNDEFINED );
    return;
}

void do_parlay( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( IS_NPC(ch) )
    {
     typo_message(ch);
     return;
    }

	if ( !can_use_skpell( ch, gsn_parlay ) )
    {
     typo_message( ch );
	return;
    }

    if ( arg[0] == '\0' )
    {
	send_to_char(AT_WHITE, "Parlay with whom?\n\r", ch );
	return;
    }
 
    if ( !( victim = get_char_room( ch, arg ) ) )
    {
      send_to_char( AT_WHITE, "That person is not here.\n\r", ch );
      return;
    }

   if ( IS_ARENA( ch ) || IS_ARENA( victim ) )
    ;
   else
    if ( is_safe( ch, victim ) )
    {
     send_to_char( AT_WHITE, "You cannot.\n\r", ch );
     return;
    }	   
    
    if ( victim == ch )
    {
	send_to_char(AT_RED, "You hit yourself.  Stupid!\n\r", ch );
	return;
    }

    if ( !(ch->fighting) )
    {
	send_to_char(C_DEFAULT, "You must already be fighting.\n\r", ch );
	return;
    }

    if ( victim == ch->fighting )
    {
     send_to_char(AT_RED, "That's a bit redundant.\n\r", ch );
     return;
    }


   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {       
     send_to_char( AT_GREY, "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, PULSE_VIOLENCE );
    if ( IS_NPC( victim ) )
     victim->memory = ch;
    ch->pcdata->twin_fighting = victim;

   act( AT_YELLOW,
    "You inflate your ego and take aim for $N with your off-hand.",
    ch, NULL, victim, TO_CHAR );
   act( AT_RED,
    "$n starts to hit you with $s off-hand, the audacity!",
    ch, NULL, victim, TO_VICT );
   act( AT_YELLOW,
    "$n starts to hit $N with $s off-hand, how egotistical.",
    ch, NULL, victim, TO_NOTVICT );
    update_skpell( ch, gsn_parlay );
    return;
}

void do_murde( CHAR_DATA *ch, char *argument )
{
    send_to_char(C_DEFAULT, "If you want to MURDER, spell it out.\n\r", ch );
    return;
}

void do_murder( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       buf [ MAX_STRING_LENGTH ];
    char       arg [ MAX_INPUT_LENGTH  ];

    one_argument( argument, arg );

    send_to_char( AT_WHITE, "No PK right now, there will be in the future.", ch );
    return;

    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Murder whom?\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
	send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
	return;
    }

    if ( IS_AFFECTED(victim, AFF_PEACE ))
    {
      send_to_char(AT_WHITE, "A wave of peace overcomes you.\n\r", ch);
      return;
    }
    if ( IS_AFFECTED( ch, AFF_PEACE ))
    {
	    affect_strip( ch, skill_lookup("aura of peace") );
	    REMOVE_BIT( ch->affected_by, AFF_PEACE );
    }

    if ( victim == ch )
    {
	send_to_char(C_DEFAULT, "Suicide is a mortal sin.\n\r", ch );
	return;
    }
    if ( is_safe( ch, victim ) )
	return;
    if ( IS_AFFECTED( ch, AFF_CHARM ) && ch->master == victim )
    {
	act(C_DEFAULT, "$N is your beloved master!", ch, NULL, victim, TO_CHAR );
	return;
    }

/*    if ( !IS_SET( ch->act, PLR_PKILLER ) && !IS_ARENA( ch ) &&
	 !IS_NPC( victim ) )  chars can murder mobs 
    {
	send_to_char(C_DEFAULT, 
 		"You must be a Pkiller to kill another mortal!\n\r", ch );
	return;
    }

    if ( !IS_SET( victim->act, PLR_PKILLER ) && !IS_ARENA( victim ) 
	 && !IS_NPC( victim ) ) chars can murder mobs
    {
	send_to_char(C_DEFAULT, "You can only pkill other Pkillers.\n\r", ch );
	return;
    }*/

    if ( ch->position == POS_FIGHTING )
    {
	send_to_char(C_DEFAULT, "You do the best you can!\n\r", ch );
	return;
    }

    WAIT_STATE( ch, PULSE_VIOLENCE );
    if ( !IS_NPC( victim ) )
    {
        sprintf( buf, "Help!  I am being attacked by %s!", ch->name );
        do_shout( victim, buf );
    }
    multi_hit( ch, victim, TYPE_UNDEFINED );
    return;
}


void do_backstab( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *wield;
    OBJ_DATA  *wield2;
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        dam = 0;

      if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_backstab ) )
      {
       typo_message( ch );
	return;
      }

    one_argument( argument, arg );
    
    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Backstab whom?\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
	send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
	return;
    }

    if ( victim == ch )
    {
	send_to_char(C_DEFAULT, "How can you sneak up on yourself?\n\r", ch );
	return;
    }

    if ( is_safe( ch, victim ) )
	return;

    if ( !( wield = get_eq_char( ch, WEAR_WIELD ) )
	||  ( wield->value[8] != WEAPON_PIERCE 
             && ( wield->value[8] == WEAPON_BLADE && wield->value[7] != 1 ) ) )
    {
	send_to_char(C_DEFAULT, "You need to wield a piercing weapon.\n\r", ch );
	return;
    }

    if ( victim->fighting )
    {
	send_to_char(C_DEFAULT, "You can't backstab a fighting person.\n\r", ch );
	return;
    }

    if ( victim->hit < MAX_HIT(victim) * 0.7 )
    {
	act(C_DEFAULT, "$N is hurt and suspicious ... you can't sneak up.",
	    ch, NULL, victim, TO_CHAR );
	return;
    }

    check_killer( ch, victim );
    WAIT_STATE( ch, skill_table[gsn_backstab].beats );
    if ( !IS_AWAKE( victim )
	|| IS_NPC( ch )
	|| number_percent( ) < ch->pcdata->learned[gsn_backstab] )
    {
     dam = number_range( wield->value[1], wield->value[2] );
     dam += ( GET_DAMROLL( ch ) );
     dam *= 2 + UMIN( (get_curr_str(ch) / 8), 4 );
     damage( ch, victim, dam, gsn_backstab, DAM_PIERCE, TRUE);
     update_skpell( ch, gsn_backstab );

     if ( ( wield2 = get_eq_char( ch, WEAR_WIELD_2 ) )
      && ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_backstab_2] > 0 )
      &&  ( wield2->value[8] == WEAPON_PIERCE 
      || ( wield2->value[8] == WEAPON_BLADE && wield2->value[7] == 1 ) ) )
     {
      dam = number_range( wield2->value[1], wield2->value[2] );
      dam += ( GET_DAMROLL( ch ) );
      dam *= 2 + UMIN( ( get_curr_str(ch) / 8), 4 );
      damage( ch, victim, dam, gsn_backstab, DAM_PIERCE, TRUE );
      update_skpell( ch, gsn_backstab_2 );
     }
    }
    else
	damage( ch, victim, 0, gsn_backstab, DAM_PIERCE, TRUE );

    return;
}

void do_assasinate( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *obj;
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );
    
    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Assasinate whom?\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
	send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
	return;
    }

    if ( victim == ch )
    {
	send_to_char(C_DEFAULT, "How can you sneak up on yourself?\n\r", ch );
	return;
    }

    if ( is_safe( ch, victim ) )
      return;

    if ( !( obj = get_eq_char( ch, WEAR_WIELD ) )
	|| obj->value[3] != 11 )
    {
	send_to_char(C_DEFAULT, "You need to wield a piercing weapon.\n\r", ch );
	return;
    }

    if ( victim->fighting )
    {
	send_to_char(C_DEFAULT, "You can't assasinate a fighting person.\n\r", ch );
	return;
    }

    if ( victim->hit < (MAX_HIT(victim)-50) )
    {
	act(C_DEFAULT, "$N is hurt and suspicious ... you can't sneak up.",
	    ch, NULL, victim, TO_CHAR );
	return;
    }

    WAIT_STATE( ch, skill_table[gsn_backstab].beats );
    multi_hit( ch, victim, gsn_backstab );

    return;
}

void do_flee( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA       *victim;
    ROOM_INDEX_DATA *was_in;
    ROOM_INDEX_DATA *now_in;
    int              attempt;

    if ( IS_AFFECTED( ch, AFF_ANTI_FLEE ) )
    {
     send_to_char( AT_RED, "You cannot!", ch );
     return;
    }
    
    if ( !( victim = ch->fighting ) )
    {
     if ( ch->position == POS_FIGHTING )
      ch->position = POS_STANDING;
     send_to_char(C_DEFAULT, "You aren't fighting anyone.\n\r", ch );
     return;
    }

    if ( ch->in_room != victim->in_room )
    {
     send_to_char(C_DEFAULT, "You abandon the battle.\n\r", ch );
     stop_fighting( ch, FALSE );
     stop_shooting( ch, FALSE );
     return;
    }

    if ( IS_SET( ch->in_room->room_flags, ROOM_NO_FLEE ) )
    {
     send_to_char(C_DEFAULT, "You can not escape!\n\r", ch);
     return;
    }	   

 was_in = ch->in_room;

 for ( attempt = 0; attempt < 6; attempt++ )
 {
  EXIT_DATA *pexit;
  int        door;

  door = number_door( );

  if ( ( pexit = was_in->exit[door] ) == 0 || !pexit->to_room
   ||   exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
   continue;

  move_char( ch, door, FALSE, TRUE);

  if ( ( now_in = ch->in_room ) == was_in )
   continue;

  stop_fighting( ch, FALSE );
  return;
 }
}


void do_rescue( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    CHAR_DATA *fch;
    char       arg [ MAX_INPUT_LENGTH ];

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_rescue ) )
    {
     typo_message( ch );
     return;
    }

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Rescue whom?\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
	send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
	return;
    }

    if ( victim == ch )
    {
	send_to_char(C_DEFAULT, "What about fleeing instead?\n\r", ch );
	return;
    }

    if ( !IS_NPC( ch ) && IS_NPC( victim ) )
    {
	send_to_char(C_DEFAULT, "Doesn't need your help!\n\r", ch );
	return;
    }

    if ( ch->fighting == victim )
    {
	send_to_char(C_DEFAULT, "Too late.\n\r", ch );
	return;
    }

    if ( !( fch = victim->fighting ) )
    {
	send_to_char(C_DEFAULT, "That person is not fighting right now.\n\r", ch );
	return;
    }


   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {       
     send_to_char( AT_GREY, "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_rescue].beats );
    if ( !IS_NPC( ch ) && number_percent( ) > ch->pcdata->learned[gsn_rescue] )
    {
	send_to_char(C_DEFAULT, "You fail the rescue.\n\r", ch );
	return;
    }

    update_skpell( ch, gsn_rescue );

    act(C_DEFAULT, "You rescue $N!",  ch, NULL, victim, TO_CHAR    );
    act(C_DEFAULT, "$n rescues you!", ch, NULL, victim, TO_VICT    );
    act(C_DEFAULT, "$n rescues $N!",  ch, NULL, victim, TO_NOTVICT );

    stop_fighting( fch, FALSE );

    set_fighting( fch, ch );

    return;
}

void do_gouge( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_gouge ) )
    {
     typo_message(ch);
     return;
    }


 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {       
     send_to_char( AT_GREY, "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_gouge].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ( ch->pcdata->learned[gsn_gouge] + (get_curr_dex(ch) / 3)) )
    {
	update_skpell( ch, gsn_gouge );
	damage( ch, victim, number_range( 20, get_curr_str(ch) ),
	 gsn_gouge, DAM_INTERNAL, TRUE );
    }
    else
	damage( ch, victim, 0, gsn_gouge, DAM_INTERNAL, TRUE );

    return;
}

void do_eyestrike( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_eyestrike ) )
    {
     typo_message(ch);
     return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
    
   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {       
     send_to_char( AT_GREY, "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_eyestrike].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_eyestrike] )
    {
	update_skpell( ch, gsn_eyestrike );
	damage( ch, victim, number_range( 30, get_curr_str(ch) * 2),
	 gsn_eyestrike, DAM_INTERNAL, TRUE );
    }
    else
	damage( ch, victim, 0, gsn_eyestrike, DAM_INTERNAL, TRUE );

    return;
}

void do_neckstrike( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_neckstrike ) )
    {
     typo_message(ch);
	return;
    }
 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }


    WAIT_STATE( ch, skill_table[gsn_neckstrike].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_neckstrike] )
    {
     update_skpell( ch, gsn_neckstrike );
     damage( ch, victim,
      number_range( 30, get_curr_str(ch) + get_curr_con(victim) ),
      gsn_neckstrike, DAM_INTERNAL, TRUE );

    }
    else
	damage( ch, victim, 0, gsn_neckstrike, DAM_INTERNAL, TRUE );

    return;
}


void do_circle( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *obj;
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        dam = 0;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_circle ) )
    {
     typo_message(ch);
	return;
    }
 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

    if ( !( obj = get_eq_char( ch, WEAR_WIELD ) )
	||  ( obj->value[8] != WEAPON_PIERCE 
             && ( obj->value[8] == WEAPON_BLADE && obj->value[7] != 1 ) ) )
    {
	send_to_char(C_DEFAULT, "You need to wield a piercing weapon.\n\r", ch );
	return;
    }


   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

   if ( victim->fighting == ch )
   {
    send_to_char( AT_RED, "You can't be tanking to circle.\n\r", ch );
    return;
   }

    WAIT_STATE( ch, skill_table[gsn_circle].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_circle] )
    {
     update_skpell( ch, gsn_circle );
     dam = number_range( obj->value[1], obj->value[2] );
     dam += ( GET_DAMROLL( ch ) );
     dam *= 2 + UMIN( (get_curr_str(ch) / 8), 4 );
     damage( ch, victim, dam, gsn_circle, DAM_PIERCE, TRUE);
     update_pos( victim, ch );	

     if ( ( obj = get_eq_char( ch, WEAR_WIELD_2 ) )
      && ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_circle_2] > 0 )
      &&  ( obj->value[8] == WEAPON_PIERCE 
      || ( obj->value[8] == WEAPON_BLADE && obj->value[7] == 1 ) ) )
     {
      dam = number_range( obj->value[1], obj->value[2] );
      dam += ( GET_DAMROLL( ch ) );
      dam *= 2 + UMIN( (get_curr_str(ch) / 8), 4 );
      damage( ch, victim, dam, gsn_circle_2, DAM_PIERCE, TRUE);
      update_skpell( ch, gsn_circle_2 );
     }
    }
    else
	damage( ch, victim, 0, gsn_circle, DAM_PIERCE, TRUE );

    return;
}

void do_kick( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int	       dam;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_kick ) )
    {
     typo_message( ch );
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_kick].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_kick] )
    {
     dam = number_range( get_curr_str(ch), get_curr_str(ch) * 3 );
     dam = kick_modifier( ch, victim, dam );
	damage( ch, victim, dam, gsn_kick, DAM_BASH, TRUE );
	update_skpell( ch, gsn_kick );
    }
    else
	damage( ch, victim, 0, gsn_kick, DAM_BASH, TRUE );

    return;
}

void do_wrack( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_wrack ) )
    {
     typo_message(ch);
	return;
    }

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
	{
	    send_to_char(C_DEFAULT, "Whom do you intend to torture?\n\r", ch );
	    return;
	}

        if ( !( victim = get_char_room( ch, arg ) ) )
	{
	    send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
	    return;
	}

    victim = get_char_room( ch, arg );

 if ( !IS_AFFECTED2( ch, AFF_TORTURING )
  || !IS_AFFECTED2( victim, AFF_TORTURED ) )
 {
  send_to_char( AT_WHITE, "You need to be torturing someone to use wrack.\n\r", ch );
  return;
 }

  damage( ch, victim, number_range( 100, ch->level*5 ), gsn_wrack,
   DAM_INTERNAL, TRUE );
  update_skpell( ch, gsn_wrack );

  WAIT_STATE( ch, skill_table[gsn_wrack].beats );

 return;
}

void do_gaze( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_gaze ) )
    {
     typo_message( ch );
	return;
    }

    one_argument( argument, arg );

    if ( arg[0] == '\0' )
	{
	    send_to_char(C_DEFAULT, "Who do you want to gaze at?\n\r", ch );
	    return;
	}

        if ( !( victim = get_char_room( ch, arg ) ) )
	{
	    send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
	    return;
	}

    victim = get_char_room( ch, arg );

    if ( IS_STUNNED( ch, STUN_TO_STUN ) )
    return;

    STUN_CHAR( victim, 3, STUN_TOTAL );
    STUN_CHAR( ch, 10, STUN_TO_STUN );
    victim->position = POS_STUNNED;
	update_skpell( ch, gsn_gaze );
	    send_to_char(C_DEFAULT, "You immobilize them with your gaze.\n\r", ch );
	    send_to_char(C_DEFAULT, "You are immobilized!\n\r", victim );

    return;
}

void do_whirl( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *vch;
    CHAR_DATA *vch_next;

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_whirl ) )
    {
     typo_message( ch );
	return;
    }

    if ( !ch->fighting )
    {
	send_to_char(C_DEFAULT, "You aren't fighting anyone.\n\r", ch );
	return;
    }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    vch = ch->fighting;
    
    if ( ch->in_room != vch->in_room )
    {
     send_to_char( AT_YELLOW, "Your victim isn't in the room.\n\r", ch );
     return;
    }
     
    WAIT_STATE( ch, skill_table[gsn_whirl].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_whirl] )
    {
        send_to_char( AT_YELLOW, "You spin around, flailing at everyone!\n\r", ch);
     for( vch = ch->in_room->people; vch; vch = vch_next)
     {
      vch_next = vch->next_in_room;
      if(vch->deleted)
       continue;
      if ( vch == ch )
       continue;
      if ( ch->clan == vch->clan )
       continue;

	damage( ch, vch, number_range( 5, get_curr_str(ch) * 3 ), gsn_whirl, DAM_BASH, TRUE );
	update_skpell( ch, gsn_whirl );
     }
    }
    else
	damage( ch, vch, 0, gsn_whirl, DAM_BASH, TRUE );

    return;
}

void do_high_kick( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int		dam;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_high_kick ) )
    {
     typo_message(ch);
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    one_argument( argument, arg );

    WAIT_STATE( ch, skill_table[gsn_high_kick].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_high_kick] )
    {
     dam = number_range( get_curr_str(ch), get_curr_str(ch) * 2 );
     dam = kick_modifier( ch, victim, dam );
     damage( ch, victim, dam, gsn_high_kick, DAM_BASH, TRUE );
     update_skpell( ch, gsn_high_kick );
    }
    else
    {
     damage( ch, victim, 0, gsn_high_kick, DAM_BASH, TRUE );
     dam = 0;
    }
    return;
}

void do_jump_kick( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int		dam;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_jump_kick ) )
    {
     typo_message( ch );
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    one_argument( argument, arg );

    WAIT_STATE( ch, skill_table[gsn_jump_kick].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_jump_kick] )
    {
     dam = number_range( get_curr_str(ch), get_curr_str(ch) * 3 );
     dam = kick_modifier( ch, victim, dam );
	damage( ch, victim, dam, gsn_jump_kick, DAM_BASH, TRUE );
	update_skpell( ch, gsn_jump_kick );
    }
    else
    {
     damage( ch, victim, 0, gsn_jump_kick, DAM_BASH, TRUE );
     dam = 0;
    }
    return;
}

void do_spin_kick( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        dam = 0;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_spin_kick ) )
    {
     typo_message( ch );
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    one_argument( argument, arg );

    WAIT_STATE( ch, skill_table[gsn_spin_kick].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_spin_kick] )
    {
     dam = number_range( get_curr_str(ch), get_curr_str(ch) * 3 );
     dam = kick_modifier( ch, victim, dam );
	damage( ch, victim, dam, gsn_spin_kick, DAM_BASH, TRUE );
	update_skpell( ch, gsn_spin_kick );
    }
    else
	damage( ch, victim, 0, gsn_spin_kick, DAM_BASH, TRUE );

 return;
}

void do_punch( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        dam = 0;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_punch ) )
    {
     typo_message( ch );
     return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_punch].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_punch] )
    {
     dam = number_range( 10, get_curr_str(ch) * 3 );
     dam = punch_modifier( ch, victim, dam );
	damage( ch, victim, dam, gsn_punch, DAM_BASH, TRUE );
	update_skpell( ch, gsn_punch );
    }
    else
	damage( ch, victim, 0, gsn_punch, DAM_BASH, TRUE );
 return;
}

void do_jab_punch( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        dam = 0;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_jab_punch ) )
    {
     typo_message(ch);
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_jab_punch].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_jab_punch] )
    {
     dam = number_range( get_curr_str(ch), get_curr_str(ch) * 3 );
     dam = punch_modifier( ch, victim, dam );
	damage( ch, victim, dam, gsn_jab_punch, DAM_BASH, TRUE );
	update_skpell( ch, gsn_jab_punch );
    }
    else
	damage( ch, victim, 0, gsn_jab_punch, DAM_BASH, TRUE );
    return;
}

void do_kidney_punch( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        dam = 0;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_kidney_punch ) )
    {
     typo_message(ch);
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_kidney_punch].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_kidney_punch] )
    {
     dam = number_range( 50, get_curr_str(ch) * 3 );
     dam = punch_modifier( ch, victim, dam );
	damage( ch, victim, dam, gsn_kidney_punch, DAM_BASH, TRUE );
	update_skpell( ch, gsn_kidney_punch );
    }
    else
	damage( ch, victim, 0, gsn_kidney_punch, DAM_BASH, TRUE );

    return;
}

void do_cross_punch( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        dam = 0;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_cross_punch ) )
    {
     typo_message(ch);
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_cross_punch].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_cross_punch] )
    {
     dam = number_range( get_curr_str(ch), get_curr_str(ch) * 3 );
     dam = punch_modifier( ch, victim, dam );
	damage( ch, victim, dam, gsn_cross_punch, DAM_BASH, TRUE );
	update_skpell( ch, gsn_cross_punch );
    }
    else
	damage( ch, victim, 0, gsn_cross_punch, DAM_BASH, TRUE );
    return;
}

void do_roundhouse_punch( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        dam = 0;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_roundhouse_punch ) )
    {
     typo_message(ch);
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_roundhouse_punch].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_roundhouse_punch] )
    {
     dam = number_range( get_curr_str(ch) * 2, get_curr_str(ch) * 5 );
     dam = punch_modifier( ch, victim, dam );
	damage( ch, victim, dam, gsn_roundhouse_punch, DAM_BASH, TRUE );
	update_skpell( ch, gsn_roundhouse_punch );
    }
    else
	damage( ch, victim, 0, gsn_roundhouse_punch, DAM_BASH, TRUE );
    return;
}

void do_uppercut_punch( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        dam = 0;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_uppercut_punch ) )
    {
     typo_message(ch);
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_uppercut_punch].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_uppercut_punch] )
    {
     dam = number_range( get_curr_str(ch) * 2, get_curr_str(ch) * 4 );
     dam = punch_modifier( ch, victim, dam );
	damage( ch, victim, dam, gsn_uppercut_punch, DAM_BASH, TRUE );
	update_skpell( ch, gsn_uppercut_punch );
    }
    else
	damage( ch, victim, 0, gsn_uppercut_punch, DAM_BASH, TRUE );
    return;
}

void do_flury( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        count = 0;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_flury ) )
    {
     typo_message(ch);
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    if ( ( get_eq_char( ch, WEAR_WIELD ) ) ||
         ( get_eq_char( ch, WEAR_WIELD_2 ) ) )
    {
	send_to_char(C_DEFAULT, "You cannot begin such a flury while your hands are full!\n\r",  ch);
	return;
    }


    WAIT_STATE( ch, skill_table[gsn_flury].beats );

    act( AT_RED, "You throw a flury of punches and kicks at $N!", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n throws a flury of punches and kicks at you!", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n throws a flury of punches and kicks at $N!", ch, NULL, victim, TO_ROOM );

    if ( can_use_skpell( ch, gsn_punch ) && number_percent( ) < 25 )
    {
      do_punch( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    if ( can_use_skpell( ch, gsn_kick ) && number_percent( ) < 25 )
    {
      do_kick( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    if ( can_use_skpell( ch, gsn_jab_punch ) && number_percent( ) < 25 )
    {
      do_jab_punch( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    if ( can_use_skpell( ch, gsn_kidney_punch ) && number_percent( ) < 25 )
    {
      do_kidney_punch( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    if ( can_use_skpell( ch, gsn_cross_punch ) && number_percent( ) < 25 )
    {
      do_cross_punch( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    if ( can_use_skpell( ch, gsn_high_kick ) && number_percent( ) < 25 )
    {
      do_high_kick( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    if ( can_use_skpell( ch, gsn_roundhouse_punch ) && number_percent( ) < 25 )
    {
      do_roundhouse_punch( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    if ( can_use_skpell( ch, gsn_jump_kick ) && number_percent( ) < 25 )
    {
      do_jump_kick( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    if ( can_use_skpell( ch, gsn_uppercut_punch ) && number_percent( ) < 25 )
    {
      do_uppercut_punch( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    if ( can_use_skpell( ch, gsn_spin_kick ) && number_percent( ) < 25 )
    {
      do_spin_kick( ch, arg );
      count++;
    }

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room || count > 4 )
      return;

    /* nothing hit */
    if ( count == 0 )
    {
    act( AT_RED, "But none of your attacks hit $N!", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "But none of $n attacks hit you!", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "But none of $n attacks hit $N!", ch, NULL, victim, TO_ROOM );
    }
    return;
}

bool tiger_strike( CHAR_DATA *ch, CHAR_DATA *victim )
{
  int dam = 0;
  if ( ch->pcdata->learned[gsn_tiger_strike] > number_percent( ) )
  {
    act( AT_RED, "You smash $N's head into the ground.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n smashes your head into the ground.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n smashes $N's head into the ground.", ch, NULL, victim, TO_ROOM );
 
    dam = number_range( ch->pcdata->learned[gsn_tiger_strike] / 5,
       get_curr_str(ch) );
 
     damage( ch, victim, dam, gsn_tiger_strike, DAM_BASH, TRUE );
     STUN_CHAR( victim, 1, STUN_TOTAL );
    update_skpell( ch, gsn_tiger_strike );
     return TRUE;
  }

 return FALSE;
}

bool tiger_claw( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;

  if ( ch->pcdata->learned[gsn_tiger_claw] > number_percent( ) )
  {
    act( AT_RED, "You swipe across $N's face.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n swipes $s hand across your face.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n swipes $s hand across $N's face.", ch, NULL, victim, TO_ROOM );
    dam = number_range( get_curr_str(ch),
     ch->pcdata->learned[gsn_tiger_claw] / 3 );
      damage( ch, victim, dam, gsn_tiger_claw, DAM_BASH, TRUE );
    update_skpell( ch, gsn_tiger_claw );
  return TRUE;
 }
 return FALSE;
}
bool tiger_rush( CHAR_DATA *ch, CHAR_DATA *victim )
{ 
 int dam = 0;
  if ( ch->pcdata->learned[gsn_tiger_rush] > number_percent( ) )
  {
    act( AT_RED, "You rush in, pummeling $N with no remorse!", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n rushes at you like a crazy person!", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n rushes $N, pounding $m into the ground!", ch, NULL, victim, TO_ROOM );
    if ( number_percent() > 75 )
    {
    dam = number_range( ch->pcdata->learned[gsn_tiger_rush] / 7,
       get_curr_str(ch) );
      damage( ch, victim, dam, gsn_tiger_rush, DAM_BASH, TRUE );
    }
    if ( number_percent() > 50 )
    {
    dam = number_range( ch->pcdata->learned[gsn_tiger_rush] / 7,
       get_curr_str(ch) );
      damage( ch, victim, dam, gsn_tiger_rush, DAM_BASH, TRUE );
    }
    if ( number_percent() > 50 )
    {
    dam = number_range( ch->pcdata->learned[gsn_tiger_rush] / 7,
       get_curr_str(ch) );
      damage( ch, victim, dam, gsn_tiger_rush, DAM_BASH, TRUE );
    }
    if ( number_percent() > 25 )
    {
    dam = number_range( ch->pcdata->learned[gsn_tiger_rush] / 7,
       get_curr_str(ch) );
      damage( ch, victim, dam, gsn_tiger_rush, DAM_BASH, TRUE );
    }
    if ( dam >= 0 )
    update_skpell( ch, gsn_tiger_rush );
     return TRUE;
  }

 return FALSE;
}
bool tiger_tail( CHAR_DATA *ch, CHAR_DATA *victim )
{
  if ( ch->pcdata->learned[gsn_tiger_tail] > number_percent( ) )
  {
     anklestrike( ch, victim );
    update_skpell( ch, gsn_tiger_tail );
     return TRUE;
  }
 return FALSE;
}

bool crane_bill( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_crane_bill] > number_percent( ) )
  {
    act( AT_RED, "You throw your weight into your fist and land a crushing blow to $N's chest.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n lands a crushing punch to your chest.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n lands a crushing punch to $N's chest.", ch, NULL, victim, TO_ROOM );
    dam = number_range( ch->pcdata->learned[gsn_crane_bill] / 5,
       get_curr_str(ch) );
      damage( ch, victim, dam, gsn_crane_bill, DAM_BASH, TRUE );
    update_skpell( ch, gsn_crane_bill );
     return TRUE;
  }

 return FALSE;
}

bool crane_wing( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_crane_wing] > number_percent( ) )
  {
    act( AT_RED, "You spin around and land a solid backhand to $N's cheek.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n spins around and lands a backhand to your cheek.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n spins around and lands a solid backhand to $N's cheek.", ch, NULL, victim, TO_ROOM );
     dam = number_range( ch->pcdata->learned[gsn_crane_wing] / 7,
       get_curr_str(ch) );
     damage( ch, victim, dam, gsn_crane_wing, DAM_BASH, TRUE );

    update_skpell( ch, gsn_crane_wing );
     return TRUE;
  }

 return FALSE;
}

bool crane_claw( CHAR_DATA *ch, CHAR_DATA *victim )
{
  int dam = 0;
  if ( ch->pcdata->learned[gsn_crane_claw] > number_percent( ) )
  {
     act( AT_RED, "You kick $n into the ground.", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n kicks $N into the ground.", ch, NULL, victim, TO_NOTVICT );
     act( AT_RED, "$n kicks you into the ground.", ch, NULL, victim, TO_VICT );
     dam = number_range( get_curr_str(ch), 
      ch->pcdata->learned[gsn_crane_claw] / 3 );
     damage( ch, victim, dam, gsn_crane_claw, DAM_INTERNAL, TRUE );
    update_skpell( ch, gsn_crane_claw );
     return TRUE;
  }

 return FALSE;
}

bool panther_paw( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_panther_paw] > number_percent( ) )
  {
    act( AT_RED, "You duck in and connect a devastating uppercut to $N's jaw.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n ducks towards you and lands a devastating uppercut to your jaw.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n lands a massive uppercut to $N's jaw.", ch, NULL, victim, TO_ROOM );
    dam = number_range( ch->pcdata->learned[gsn_panther_paw] / 4,
       get_curr_str(ch) );
      damage( ch, victim, dam, gsn_panther_paw, DAM_BASH, TRUE );
    update_skpell( ch, gsn_panther_paw );
     return TRUE;
  }

 return FALSE;
}

bool panther_scratch( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_panther_scratch] > number_percent( ) )
  {
    act( AT_RED, "You twist your body and swipe at $N with both hands.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n twists around and swipes at you with both hands.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n twists around and swipes at $N with both hands.", ch, NULL, victim, TO_ROOM );

    dam = number_range( ch->pcdata->learned[gsn_panther_scratch] / 4,
       get_curr_str(ch) );
      damage( ch, victim, dam, gsn_panther_scratch, DAM_BASH, TRUE );

    dam = number_range( ch->pcdata->learned[gsn_panther_scratch] / 4,
       get_curr_str(ch) );
      damage( ch, victim, dam, gsn_panther_scratch, DAM_BASH, TRUE );
    update_skpell( ch, gsn_panther_scratch );
     return TRUE;
  }

 return FALSE;
}
bool panther_tail( CHAR_DATA *ch, CHAR_DATA *victim )
{
  if ( ch->pcdata->learned[gsn_panther_tail] > number_percent( ) )
  {
     anklestrike( ch, victim );
    update_skpell( ch, gsn_panther_tail );
     return TRUE;
  }
 return FALSE;
}

bool dragon_roar( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_dragon_roar] > number_percent( ) )
  {
    act( AT_RED, "You spin around and land a crushing roundhouse kick on $N's jaw.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n spins around and lands a crushing roundhouse kick on your jaw.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n spins around and lands a crushing roundhouse kick on $N's jaw.", ch, NULL, victim, TO_ROOM );

    dam = number_range( get_curr_str(ch) * 2, 
     ch->pcdata->learned[gsn_dragon_roar]);
      damage( ch, victim, dam, gsn_dragon_roar, DAM_BASH, TRUE );

    update_skpell( ch, gsn_dragon_roar );
     return TRUE;
  }

 return FALSE;
}
bool dragon_blast( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_dragon_blast] > number_percent( ) )
  {
    act( AT_RED, "You step foward and land a two fisted blast to $N's chest.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n steps towards you and lands a two fisted blast to your chest.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n steps foward and lands a two fisted blast to $N's chest.", ch, NULL, victim, TO_ROOM );

    dam = number_range( get_curr_str(ch) *3,
     ch->pcdata->learned[gsn_dragon_blast]);
      damage( ch, victim, dam, gsn_dragon_blast, DAM_BASH, TRUE );
    update_skpell( ch, gsn_dragon_blast );
     return TRUE;
  }

 return FALSE;
}
bool dragon_grab( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_dragon_grab] >  number_percent( ) )
  {
    act( AT_RED, "You grapple with $N and slam $M to the ground.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n grabs you and slams you to the ground.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n grabs $N and slams $M to the ground.", ch, NULL, victim, TO_ROOM );

    dam = number_range( get_curr_str(ch),
     ch->pcdata->learned[gsn_dragon_grab]);
      damage( ch, victim, dam, gsn_dragon_grab, DAM_BASH, TRUE );

    update_skpell( ch, gsn_dragon_grab );
     return TRUE;
  }

 return FALSE;
}

bool snake_fang( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_snake_fang] > number_percent( ) )
  {
    act( AT_RED, "You kick $N's shin and $E falls to $S knees.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n kicks your shin and you fall to your knees.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n kicks $N's shin and $E crumbles to $S knees.", ch, NULL, victim, TO_ROOM );

    dam = number_range( get_curr_str(ch) / 2,
     ch->pcdata->learned[gsn_snake_fang] / 4);
      damage( ch, victim, dam, gsn_snake_fang, DAM_BASH, TRUE );

    victim->position = POS_RESTING;

    update_skpell( ch, gsn_snake_fang );
     return TRUE;
  }

 return FALSE;
}
bool snake_bite( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_snake_bite] > number_percent( ) )
  {
    act( AT_RED, "You duck in and land a solid blow to $N's abdomen.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n ducks towards you and hits you with a hard sucker punch.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n ducks in and sucker punches $N.", ch, NULL, victim, TO_ROOM );

    dam = number_range( get_curr_str(ch),
     ch->pcdata->learned[gsn_snake_bite] / 2);
      damage( ch, victim, dam, gsn_snake_bite, DAM_BASH, TRUE );
    STUN_CHAR( victim, 1, STUN_TOTAL );
    victim->position = POS_STUNNED;

    update_skpell( ch, gsn_snake_bite );
     return TRUE;
  }

 return FALSE;
}

bool snake_rush( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;

  if ( ch->pcdata->learned[gsn_snake_rush] > number_percent( ) )
  {
    act( AT_RED, "You rush in and shower $N with lightning fast punches.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n rushes towards you and showeres you with lightning fast punches!", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n rushes towards $N and showers $M with punches.", ch, NULL, victim, TO_ROOM );

    dam = number_range( ch->pcdata->learned[gsn_snake_rush] / 5,
       get_curr_str(ch) / 2 );
      damage( ch, victim, dam, gsn_snake_rush, DAM_BASH, TRUE );

    dam = number_range( ch->pcdata->learned[gsn_snake_rush] / 5,
       get_curr_str(ch) / 2 );
      damage( ch, victim, dam, gsn_snake_rush, DAM_BASH, TRUE );

    dam = number_range( ch->pcdata->learned[gsn_snake_rush] / 5,
       get_curr_str(ch) / 2 );
      damage( ch, victim, dam, gsn_snake_rush, DAM_BASH, TRUE );

    dam = number_range( ch->pcdata->learned[gsn_snake_rush] / 5,
       get_curr_str(ch) / 2 );
      damage( ch, victim, dam, gsn_snake_rush, DAM_BASH, TRUE );

    dam = number_range( ch->pcdata->learned[gsn_snake_rush] / 5,
       get_curr_str(ch) / 2 );
      damage( ch, victim, dam, gsn_snake_rush, DAM_BASH, TRUE );

    dam = number_range( ch->pcdata->learned[gsn_snake_rush] / 5,
       get_curr_str(ch) / 2 );
      damage( ch, victim, dam, gsn_snake_rush, DAM_BASH, TRUE );

    update_skpell( ch, gsn_snake_rush );
     return TRUE;
  }

 return FALSE;
}

bool bull_rush( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_bull_rush] > number_percent( ) )
  {
    act( AT_RED, "You ram into $N like a freight train.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n rams you with the force of a freight train!", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n rams $N like a freight train.", ch, NULL, victim, TO_ROOM );

    dam = number_range( ch->pcdata->learned[gsn_bull_rush] / 2,
       (get_curr_str(ch) * number_range(3,8)));
      damage( ch, victim, dam, gsn_bull_rush, DAM_BASH, TRUE );

    STUN_CHAR( victim, 1, STUN_TOTAL );
    victim->position = POS_STUNNED;

    update_skpell( ch, gsn_bull_rush );
    return TRUE;
  }

 return FALSE;
}

bool sparrow_flower( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_sparrow_flower] > number_percent( ) )
  {
    act( AT_RED, "You rush forward, striking $N's chest.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n rushes forward and strikes your chest", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n rushes forward and strikes $N's chest.", ch, NULL, victim, TO_ROOM );

    dam = number_range( ch->pcdata->learned[gsn_sparrow_flower] / 7,
       get_curr_str(ch) );
      damage( ch, victim, dam, gsn_sparrow_flower, DAM_BASH, TRUE );

    update_skpell( ch, gsn_sparrow_flower );
     return TRUE;
  }

 return FALSE;
}
bool sparrow_song( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_sparrow_song] > number_percent( ) )
  {
    act( AT_RED, "You spin around and cartwheel towards $N, kicking $M in the face.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n flips over and does a cartwheel on your face!", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n flips over and cartwheels $N right in the face.", ch, NULL, victim, TO_ROOM );

    dam = number_range( get_curr_str(ch), 
     ch->pcdata->learned[gsn_sparrow_song] / 4);
      damage( ch, victim, dam, gsn_sparrow_song, DAM_BASH, TRUE );

    dam = number_range( get_curr_str(ch), 
     ch->pcdata->learned[gsn_sparrow_song] / 4);
      damage( ch, victim, dam, gsn_sparrow_song, DAM_BASH, TRUE );

    update_skpell( ch, gsn_sparrow_song );
     return TRUE;
  }

 return FALSE;
}
bool sparrow_wing( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_sparrow_wing] > number_percent( ) )
  {
    act( AT_RED, "You turn around and kick $N into the air.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n turns around and kicks you into the air!", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n turns around and kicks $N into the air.", ch, NULL, victim, TO_ROOM );

    dam = number_range( ch->pcdata->learned[gsn_sparrow_wing] / 2,
       get_curr_str(ch) * 2 );
      damage( ch, victim, dam, gsn_sparrow_wing, DAM_BASH, TRUE );
    STUN_CHAR( victim, 1, STUN_TOTAL );
    victim->position = POS_STUNNED;

    update_skpell( ch, gsn_sparrow_wing );
     return TRUE;
  }

 return FALSE;
}
bool sparrow_hop( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_sparrow_hop] > number_percent( ) )
  {
    act( AT_RED, "You hop forward and strike $N.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n hops forward and strikes you.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n hops forward and strikes $N.", ch, NULL, victim, TO_ROOM );

    dam = number_range( get_curr_str(ch),
     ch->pcdata->learned[gsn_sparrow_hop] / 3);
      damage( ch, victim, dam, gsn_sparrow_hop, DAM_BASH, TRUE );

    update_skpell( ch, gsn_sparrow_hop );
     return TRUE;
  }

 return FALSE;
}

bool sparrow_claw( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_sparrow_claw] > number_percent( ) )
  {
    act( AT_RED, "You arc your hands down and slap $N's face into the ground.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n slaps your face into the ground.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n slaps $N's face into the ground.", ch, NULL, victim, TO_ROOM );

    dam = number_range( ch->pcdata->learned[gsn_sparrow_claw] / 2,
       get_curr_str(ch) * 3 );
      damage( ch, victim, dam, gsn_sparrow_claw, DAM_BASH, TRUE );
    victim->position = POS_RESTING;
    update_skpell( ch, gsn_sparrow_claw );
     return TRUE;
  }

 return FALSE;
}
bool sparrow_smash( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_sparrow_smash] > number_percent( )
     && ( victim->position == POS_RESTING || victim->position == POS_STUNNED ))
  {
    act( AT_RED, "You flip over, and land on $N's chest.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n flips over and lands on your chest.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n flips into the air and lands on $N's chest.", ch, NULL, victim, TO_ROOM );

    dam = number_range( ch->pcdata->learned[gsn_sparrow_smash] / 2,
       get_curr_str(ch) * 2 );
      damage( ch, victim, dam, gsn_sparrow_smash, DAM_BASH, TRUE );

    update_skpell( ch, gsn_sparrow_smash );
     return TRUE;
  }

 return FALSE;
}

bool monkey_headbutt( CHAR_DATA *ch, CHAR_DATA *victim )
{
 int dam = 0;
  if ( ch->pcdata->learned[gsn_monkey_headbutt] > number_percent( ) )
  {
    act( AT_RED, "You waver back and charge forward, smashing into $N's  chest.", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n wavers back and throws $s whole body into you.", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n wavers back and charges forward, ramming $N in the chest.", ch, NULL, victim, TO_ROOM );

    dam = number_range( get_curr_str(ch),
     ch->pcdata->learned[gsn_monkey_headbutt] / 3);
      damage( ch, victim, dam, gsn_monkey_headbutt, DAM_BASH, TRUE );
    victim->position = POS_RESTING;
    update_skpell( ch, gsn_monkey_headbutt );
     return TRUE;
  }

 return FALSE;
}

void do_masterstrike( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    bool       hit = FALSE;

    one_argument( argument, arg );

    if ( IS_NPC(ch) )
    return;  

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_masterstrike ) )
    {
     typo_message(ch);
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    WAIT_STATE( ch, skill_table[gsn_masterstrike].beats );

    act( AT_RED, "You tense your muscles and strike at $N!", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n strikes at you!", ch, NULL, victim, TO_VICT );
    act( C_DEFAULT, "$n strikes at $N!", ch, NULL, victim, TO_ROOM );

    update_skpell( ch, gsn_masterstrike );

    if ( ch->pcdata->attitude == 13 )
    {
     if ( number_percent( ) <
     (( ch->pcdata->learned[gsn_tiger_strike] / 2 ) +
     (ch->pcdata->learned[gsn_tiger_strike] / 4) ) )
     hit = tiger_strike( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_tiger_claw] / 2 ) +
     (ch->pcdata->learned[gsn_tiger_claw] / 4) ) )
     hit = tiger_claw( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_tiger_rush] / 2 ) +
     (ch->pcdata->learned[gsn_tiger_rush] / 4) ) )
     hit = tiger_rush( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_tiger_tail] / 2 ) +
     (ch->pcdata->learned[gsn_tiger_tail] / 4) ) )
     tiger_tail( ch, victim );     
    update_skpell( ch, gsn_tiger_stance );
     return;
    }

    if ( ch->pcdata->attitude == -12 )
    {
     if ( number_percent( ) <
     (( ch->pcdata->learned[gsn_crane_bill] / 2 ) +
     (ch->pcdata->learned[gsn_crane_bill] / 4) ) )
     hit = crane_bill( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_crane_wing] / 2 ) +
     (ch->pcdata->learned[gsn_crane_wing] / 4) ) )
     hit = crane_wing( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_crane_claw] / 2 ) +
     (ch->pcdata->learned[gsn_crane_claw] / 4) ) )
     crane_claw( ch, victim );     
    update_skpell( ch, gsn_crane_stance );
     return;
    }

    if ( ch->pcdata->attitude == -11 )
    {
     if ( number_percent( ) <
     (( ch->pcdata->learned[gsn_panther_scratch] / 2 ) +
     (ch->pcdata->learned[gsn_panther_scratch] / 4) ) )
     hit = panther_scratch( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_panther_paw] / 2 ) +
     (ch->pcdata->learned[gsn_panther_paw] / 4) ) )
     hit = panther_paw( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_panther_tail] / 2 ) +
     (ch->pcdata->learned[gsn_panther_tail] / 4) ) )
     panther_tail( ch, victim );     
    update_skpell( ch, gsn_panther_stance );
     return;
    }
    if ( ch->pcdata->attitude == 12 )
    {
     if ( number_percent( ) <
     (( ch->pcdata->learned[gsn_dragon_roar] / 2 ) +
     (ch->pcdata->learned[gsn_dragon_roar] / 4) ) )
     hit = dragon_roar( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_dragon_blast] / 2 ) +
     (ch->pcdata->learned[gsn_dragon_blast] / 4) ) )
     hit = dragon_blast( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_dragon_grab] / 2 ) +
     (ch->pcdata->learned[gsn_dragon_grab] / 4) ) )
     dragon_grab( ch, victim );     
    update_skpell( ch, gsn_dragon_stance );
     return;
    }

    if ( ch->pcdata->attitude == 14 )
    {
     if ( number_percent( ) <
     (( ch->pcdata->learned[gsn_bull_rush] / 2 ) +
     (ch->pcdata->learned[gsn_bull_rush] / 4) ) )
     bull_rush( ch, victim );     
     update_skpell( ch, gsn_bull_stance );
     return;
    }
    if ( ch->pcdata->attitude == -14 )
    {
     if ( number_percent( ) <
     (( ch->pcdata->learned[gsn_monkey_headbutt] / 2 ) +
     (ch->pcdata->learned[gsn_monkey_headbutt] / 4) ) )
     monkey_headbutt( ch, victim );     
     update_skpell( ch, gsn_monkey_stance );
     return;
    }

    if ( ch->pcdata->attitude == 11 )
    {
     if ( number_percent( ) <
     (( ch->pcdata->learned[gsn_snake_fang] / 2 ) +
     (ch->pcdata->learned[gsn_snake_fang] / 4) ) )
     hit = snake_fang( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_snake_bite] / 2 ) +
     (ch->pcdata->learned[gsn_snake_bite] / 4) ) )
     hit = snake_bite( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_snake_rush] / 2 ) +
     (ch->pcdata->learned[gsn_snake_rush] / 4) ) )
     snake_rush( ch, victim );     
    update_skpell( ch, gsn_snake_stance );
     return;
    }

    if ( ch->pcdata->attitude == -13 )
    {
     if ( number_percent( ) <
     (( ch->pcdata->learned[gsn_sparrow_flower] / 2 ) +
     (ch->pcdata->learned[gsn_sparrow_flower] / 4) ) )
     hit = sparrow_flower( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_sparrow_song] / 2 ) +
     (ch->pcdata->learned[gsn_sparrow_song] / 4) ) )
     hit = sparrow_song( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_sparrow_wing] / 2 ) +
     (ch->pcdata->learned[gsn_sparrow_wing] / 4) ) )
     hit = sparrow_wing( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_sparrow_hop] / 2 ) +
     (ch->pcdata->learned[gsn_sparrow_hop] / 4) ) )
     hit = sparrow_hop( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_sparrow_claw] / 2 ) +
     (ch->pcdata->learned[gsn_sparrow_claw] / 4) ) )
     hit = sparrow_claw( ch, victim );     

    if ( !victim || victim->position == POS_DEAD || !victim->in_room
	 || victim->in_room != ch->in_room )
      return;

     if ( hit && number_percent( ) <
     (( ch->pcdata->learned[gsn_sparrow_smash] / 2 ) +
     (ch->pcdata->learned[gsn_sparrow_smash] / 4) ) )
     sparrow_smash( ch, victim );     
    update_skpell( ch, gsn_sparrow_stance );
     return;
    }

    return;
}

void do_disarm( CHAR_DATA *ch, char *argument )
{
    OBJ_DATA  *obj;
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    int        percent;

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_disarm ) )
    {
     typo_message( ch );
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
    if ( !IS_NPC( ch ) && !is_class( ch, CLASS_ASSASSIN ) )
    if ( ( !get_eq_char( ch, WEAR_WIELD ) ) 
    && ( !get_eq_char( ch, WEAR_WIELD_2 ) ) )
    {
	send_to_char(C_DEFAULT, "You must wield a weapon to disarm.\n\r", ch );
	return;
    }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   }

    if ( victim->fighting != ch && ch->fighting != victim )
    {
	act(C_DEFAULT, "$E is not fighting you!", ch, NULL, victim, TO_CHAR );
	return;
    }

    if ( !( obj = get_eq_char( victim, WEAR_WIELD ) ) )
    {
	if ( !( obj = get_eq_char( victim, WEAR_WIELD_2 ) ) )
	{
	  send_to_char(C_DEFAULT, "Your opponent is not wielding a weapon.\n\r", ch );
	  return;
        }
    }
        
    WAIT_STATE( ch, skill_table[gsn_disarm].beats );
    percent = number_percent( ) + victim->level - ch->level;
    if ( ( IS_NPC( ch ) && percent < 20 ) || ( ( !IS_NPC(ch) ) && 
	( percent < ch->pcdata->learned[gsn_disarm] * 2 / 3 ) ) )
    {
	disarm( ch, victim );
 	update_skpell( ch, gsn_disarm ); 
    }
    else
	send_to_char(C_DEFAULT, "You failed.\n\r", ch );
    return;
}

void do_sla( CHAR_DATA *ch, char *argument )
{
    send_to_char(C_DEFAULT, "If you want to SLAY, spell it out.\n\r", ch );
    return;
}

void do_slay( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];
    char       buf [ MAX_STRING_LENGTH ];

    one_argument( argument, arg );
    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Slay whom?\n\r", ch );
	return;
    }

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
	send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
	return;
    }

    if ( ( !IS_NPC( victim ) && victim->level >= ch->level && victim != ch ) ||
	    (IS_NPC( ch ) && !IS_NPC( victim )) )
    {
	send_to_char(C_DEFAULT, "You failed.\n\r", ch );
	return;
    }

    if ( ch->pcdata && ch->pcdata->slayusee[0] != '\0' )
    sprintf( buf, "%s.", ch->pcdata->slayusee );
    else
    sprintf( buf, "You slay $N in cold blood." );

    act( AT_RED, buf, ch, NULL, victim, TO_CHAR );

    if ( ch->pcdata && ch->pcdata->slayvict[0] != '\0' )
    sprintf( buf, "%s.", ch->pcdata->slayvict );
    else
    sprintf( buf, "$n slays you in cold blood." );

    act(AT_RED, buf, ch, NULL, victim, TO_VICT );

    if ( ch->pcdata && ch->pcdata->slayroom[0] != '\0' )
    sprintf( buf, "%s.", ch->pcdata->slayroom );
    else
    sprintf( buf, "$n slays $N in cold blood." );

    act(AT_RED, buf, ch, NULL, victim, TO_NOTVICT );

    sprintf( log_buf, "%s slays %s at %d.\n\r", ch->name, victim->name,
             victim->in_room->vnum );
    log_string( log_buf, CHANNEL_LOG, ch->level - 1 );
    if ( !IS_NPC( victim ) )
      wiznet( log_buf,ch, NULL, WIZ_DEATHS, 0, 0 );
    raw_kill( ch, victim );
    return;
}

void do_blasto( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *Obj;
  ROOM_INDEX_DATA *to_room;
  char buf[MAX_STRING_LENGTH];

  to_room = ch->in_room;

  Obj = get_eq_char( ch, WEAR_WIELD_2 );

  if  ( Obj == NULL )
  {
   send_to_char( C_DEFAULT, "You aren't holding anything in your secondary hand.\n\r", ch );
   return;
  }

  if  ( Obj->value[1] != skill_lookup( "xpipebomb" ))
  {
   send_to_char( C_DEFAULT, "You aren't holding a pipebomb.\n\r", ch );
   return;
  }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   } 
  
  send_to_char( C_DEFAULT, "You set the bomb, RUN!\n\r", ch );
	  sprintf( buf, 
      "%s drops a small flask, which begins to smoke and fume", ch->oname );
	act( AT_WHITE, buf, ch, Obj, NULL, TO_ROOM );
  unequip_char( ch, Obj );
  Obj->timer = 1;
  obj_from_char( Obj );
  obj_to_room( Obj, to_room );

  return;
}

void do_torture( CHAR_DATA *ch, char *argument )
{
 CHAR_DATA    *victim;
 char         buf[MAX_STRING_LENGTH];
 char         arg[MAX_INPUT_LENGTH];
 AFFECT_DATA  af;
   
    one_argument( argument, arg );
    
    if ( arg[0] == '\0' )
    {
	send_to_char(C_DEFAULT, "Torture whom?\n\r", ch );
	return;
    }
 

    if ( !( victim = get_char_room( ch, arg ) ) )
    {
	send_to_char(C_DEFAULT, "They aren't here.\n\r", ch );
	return;
    }

    victim = get_char_room( ch, arg );

    if ( victim == ch )
    {
	send_to_char(C_DEFAULT, "You can't torture yourself.\n\r", ch );
	return;
    }

    if ( is_safe( ch, victim ) )
    {
     send_to_char( AT_WHITE, "You can't torture a safe person.\n\r",ch);
     return;
    }

 if ( !IS_STUNNED( victim, STUN_TOTAL ) )
 {
  send_to_char( AT_WHITE, "You can't torture someone unless they are stunned.\n\r", ch);
  return;
 }

 if ( IS_AFFECTED2( ch, AFF_TORTURING ) || IS_AFFECTED2( victim, AFF_TORTURED ) )
 {
  send_to_char( AT_WHITE, "You are already torturing or they are already being tortured.\n\r", ch );
  return;
 }

 if ( IS_SET( ch->in_room->room_flags, ROOM_SAFE ) )
 {
  send_to_char( C_DEFAULT, "You can't torture in a safe room.\n\r", ch );
  return;
 }

  SET_BIT( victim->affected_by2, AFF_TORTURED );
  SET_BIT( ch->affected_by2, AFF_TORTURING );

  af.type	= gsn_torture;
  af.level	= ch->level;
  af.duration	= ch->level;
  af.location	= APPLY_NONE;
  af.modifier	= 0;
  af.bitvector	= AFF_BLIND;
  affect_to_char( victim, &af );

    send_to_char(AT_GREY, "You stake your opponent to the ground and torture them!\n\r", ch );
    send_to_char(AT_GREY, "Someone knocks you down, stakes your wrists to the ground and starts to torture you!\n\r", victim );
    if ( !IS_NPC(ch) && !IS_NPC(victim) )
    sprintf( buf, "%s tortures %s.\n\r", ch->oname, victim->oname );
    if ( IS_NPC(ch) && !IS_NPC(victim) )
    sprintf( buf, "$n tortures %s.\n\r", victim->oname );
    if ( !IS_NPC(ch) && IS_NPC(victim) )
    sprintf( buf, "%s tortures $N.\n\r", ch->oname );
    if ( IS_NPC(ch) && IS_NPC(victim) )
    sprintf( buf, "$n tortures $N.\n\r" );
    act(AT_GREY, buf, ch, victim, NULL, TO_ROOM );

 return;
}
    
void do_throw( CHAR_DATA *ch, char *argument )
{
  char arg1[MAX_INPUT_LENGTH];
  char arg2[MAX_INPUT_LENGTH];
  char arg3[MAX_INPUT_LENGTH];
  char buf[MAX_STRING_LENGTH];
  CHAR_DATA *victim;
  ROOM_INDEX_DATA *to_room;
  OBJ_DATA *Obj;
  EXIT_DATA *pexit;
  AFFECT_DATA *paf;
  int dir = 0;
  int dist = 0;
  int MAX_DIST = 2;
  int dam;
  extern char *dir_noun [];

  argument = one_argument( argument, arg1 );
  argument = one_argument( argument, arg2 );
  argument = one_argument( argument, arg3 );

  if ( arg1[0] == '\0' )
  {
    send_to_char( C_DEFAULT, "Throw what item?\n\r", ch );
    return;
  }

  if ( ( Obj = get_obj_wear( ch, arg1 ) ) == NULL )
  {
    send_to_char( C_DEFAULT,
		 "You are not wearing, wielding, or holding that item.\n\r",
		 ch );
    return;
  }

  if ( !IS_SET( Obj->wear_loc, WEAR_WIELD )
    && !IS_SET( Obj->wear_loc, WEAR_WIELD_2 ) )
  {
    send_to_char( C_DEFAULT,
		 "You are not wielding or holding that item.\n\r", ch );
    return;
  }

  if ( IS_SET( Obj->extra_flags, ITEM_NOREMOVE ) || IS_SET( Obj->extra_flags,
							    ITEM_NODROP ) )
  {
    send_to_char( C_DEFAULT, "You can't let go of it!\n\r", ch );
    return;
  }

  to_room = ch->in_room;

    if ( arg2[0] == '\0' )
    {
      send_to_char( C_DEFAULT, "Throw it in which direction?\n\r", ch );
      return;
    }

    if ( arg3[0] == '\0' )
    {
	send_to_char( C_DEFAULT, "Throw it at whom?\n\r", ch );
	return;
    }

      if ( get_curr_str( ch ) >= 20 )
      {
        MAX_DIST = 3;
        if ( get_curr_str( ch ) == 25 )
          MAX_DIST = 4;
      }
      
      for ( dir = 0; dir < 6; dir++ )
	if ( arg2[0] == dir_name[dir][0] && !str_prefix( arg2,
							 dir_name[dir] ) )
	  break;

      if ( dir == 6 )
      {
	send_to_char( C_DEFAULT, "Throw in which direction?\n\r", ch );
	return;
      }

      if ( ( pexit = to_room->exit[dir] ) == NULL ||
	   ( to_room = pexit->to_room ) == NULL )
      {
	send_to_char( C_DEFAULT, "You cannot throw in that direction.\n\r",
		     ch );
	return;
      }

      if ( exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
      {
	send_to_char( C_DEFAULT, "That exit is blocked.\n\r", ch );
	return;
      }

      if ( ( victim = get_char_closest( ch, dir, arg3 ) ) == NULL )
      {
       send_to_char( C_DEFAULT, "They aren't anywhere.\n\r", ch );
       return;
      }


	unequip_char( ch, Obj );
	obj_from_char( Obj );

      for ( dist = 1; dist <= MAX_DIST; dist++ )
      {
	obj_from_room( Obj );
	obj_to_room( Obj, to_room );

	if ( target_available( ch, victim, Obj ) )
	  break;

	if ( ( pexit = to_room->exit[dir] ) == NULL ||
	     ( to_room = pexit->to_room ) == NULL ||
               exit_blocked( pexit, Obj->in_room ) > EXIT_STATUS_OPEN )
	{
	  sprintf( buf, "A $p flys in from $T and hits the %s wall.", 
		   dir_name[dir] );
	  act( AT_WHITE, buf, NULL, Obj, dir_noun[rev_dir[dir]], TO_ROOM );
	  sprintf( buf, "You throw $p %d room%s $T, where it hits a wall.",
		   dist, dist > 1 ? "s" : "" );
	  act( AT_WHITE, buf, ch, Obj, dir_name[dir], TO_CHAR );
	  oprog_throw_trigger( Obj, ch );
          obj_from_room( Obj );
	  obj_to_room( Obj, to_room );

         if ( Obj->item_type == ITEM_BOMB )
         {
          Obj->owner = ch;
          bomb_explode( Obj, Obj->in_room );
	  return;
         }

         if ( Obj->timer > -1 )
          extract_obj( Obj );

	  return;
	}
      }

      if ( victim == NULL )
      {
	act( AT_WHITE, 
	    "A $p flies in from $T and falls harmlessly to the ground.",
	    NULL, Obj, dir_noun[rev_dir[dir]], TO_ROOM );
	sprintf( buf,
		"$p falls harmlessly to the ground %d room%s $T of here.",
		dist, dist > 1 ? "s" : "" );
	act( AT_WHITE, buf, ch, Obj, dir_name[dir], TO_CHAR );
	oprog_throw_trigger( Obj, ch );
        obj_from_room( Obj );
	obj_to_room( Obj, to_room );

         if ( Obj->item_type == ITEM_BOMB )
         {
          Obj->owner = ch;
          bomb_explode( Obj, Obj->in_room );
          return;
         }

         if ( Obj->timer > -1 )
          extract_obj( Obj );

	return;
      }

      act( AT_WHITE, "$p flys in from $T and hits $n!", victim, Obj,
	  dir_noun[rev_dir[dir]], TO_NOTVICT );
      act( AT_WHITE, "$p flys in from $T and hits you!", victim, Obj,
	  dir_noun[rev_dir[dir]], TO_CHAR );
      sprintf( buf, "$p flew %d rooms %s and hit $N!", dist,
	      dir_name[dir] );
      act( AT_WHITE, buf, ch, Obj, victim, TO_CHAR );
      oprog_throw_trigger( Obj, ch );
        obj_from_room( Obj );
	obj_to_room( Obj, to_room );

     if ( Obj->item_type == ITEM_BOMB )
     {
      Obj->owner = ch;
      bomb_explode( Obj, Obj->in_room );
     }
     else
     {
      dam = Obj->weight + dice( 10, 4 );

      for ( paf = Obj->pIndexData->affected; paf; paf = paf->next )
       if ( paf->location == APPLY_THROWPLUS )
        dam += paf->modifier;

      damage( ch, victim, dam, DAMNOUN_STRIKE, DAM_BASH, TRUE );

      if ( Obj->timer > -1 )
       extract_obj( Obj );
     }

 return;
}

void do_shoot( CHAR_DATA *ch, char *argument )
{
  char arg1[MAX_INPUT_LENGTH];
  char arg2[MAX_INPUT_LENGTH];
  char arg3[MAX_INPUT_LENGTH];
  char arg4[MAX_INPUT_LENGTH];
  char buf[MAX_STRING_LENGTH];
  CHAR_DATA *victim;
  ROOM_INDEX_DATA *to_room;
  OBJ_DATA *ammo;
  OBJ_DATA *ammobox;
  OBJ_DATA *weapon;
  EXIT_DATA *pexit;
  AFFECT_DATA *paf;
  int dir = 0;
  int dist = 0;
  int dam = 0;
  int MAX_DIST = 2;
  bool ammocheck = FALSE;
  extern char *dir_noun [];

  argument = one_argument( argument, arg1 );
  argument = one_argument( argument, arg2 );
  argument = one_argument( argument, arg3 );
  argument = one_argument( argument, arg4 );

    if ( arg1[0] == '\0' )
    {
     send_to_char( C_DEFAULT, "Shoot what ranged weapon?\n\r", ch );
     return;
    }

    if ( !(weapon = get_obj_wear( ch, arg1 )) )
    {
     send_to_char( C_DEFAULT, "You aren't wielding that weapon.\n\r", ch );
     return;
    }

    if ( weapon->item_type != ITEM_RANGED_WEAPON )
    {
     send_to_char( C_DEFAULT, "That isn't a ranged weapon.\n\r", ch );
     return;
    }

    if ( weapon->value[0] == RANGED_WEAPON_FIREARM )
    {
     send_to_char( C_DEFAULT, "That isn't a bow or a bazooka.\n\r", ch );
     return;
    }

    if ( arg2[0] == '\0' )
    {
      send_to_char( C_DEFAULT, "Use which unit of ammunition?\n\r", ch );
      return;
    }

    if ( !(ammo = get_obj_carry( ch, arg2 )) )
    {
     if ( !(ammo = get_obj_wear( ch, arg2 )) )
     {
      send_to_char( C_DEFAULT, "You don't have that ammo.\n\r", ch );
      return;
     }
    }

    if ( ammo->durability <= 0 )
    {
     send_to_char( C_DEFAULT, "That ammunition is broken.\n\r", ch );
     return;
    }

    if ( ammo->item_type == ITEM_CLIP )
    {
     ammobox = ammo;

     for ( ammo = ammobox->contains; ammo; ammo = ammo->next_content )
     {
      if ( !ammo )
       continue;
  
       ammocheck = TRUE;
       break;
     }

     if ( ammocheck == FALSE )
     {
      if ( weapon->value[0] == RANGED_WEAPON_BOW )
       send_to_char( C_DEFAULT, "That quiver is empty.\n\r", ch );
      if ( weapon->value[0] == RANGED_WEAPON_BAZOOKA )
       send_to_char( C_DEFAULT, "That ammo box is empty.\n\r", ch );
      return;
     }

     obj_from_obj( ammo, FALSE );
     obj_to_char( ammo, ch );
    }

    if ( weapon->value[0] == RANGED_WEAPON_BOW )
    {
     if ( ammo->item_type != ITEM_ARROW )
     {
      send_to_char( C_DEFAULT, "That ammo isn't an arrow.\n\r", ch );
      return;
     }

     if ( weapon->value[1] < ammo->value[1] )
     {
      send_to_char( C_DEFAULT, "That ammo is too large for the weapon.\n\r", ch );
      return;
     }
    }
    else
     if ( weapon->value[0] == RANGED_WEAPON_BAZOOKA )
    {
     if ( ammo->item_type != ITEM_BOMB )
     {
      send_to_char( C_DEFAULT, "That ammo isn't a bomb.\n\r", ch );
      return;
     }

     if ( ammo->value[2] != PROP_LOCAL )
     {
      send_to_char( C_DEFAULT, "Bazooka ammo needs to have a local propulsion system.\n\r", ch );
      return;
     }

     if ( weapon->value[1] < ammo->value[3] )
     {
      send_to_char( C_DEFAULT, "That ammo is too large for the weapon.\n\r", ch );
      return;
     }
    }

    if ( arg3[0] == '\0' )
    {
	send_to_char( C_DEFAULT, "Shoot in which direction?\n\r", ch );
	return;
    }

    if ( arg4[0] == '\0' )
    {
	send_to_char( C_DEFAULT, "Shoot it at whom?\n\r", ch );
	return;
    }
      
    for ( dir = 0; dir < 6; dir++ )
     if ( arg3[0] == dir_name[dir][0] && !str_prefix( arg3, dir_name[dir] ) )
	  break;

      if ( dir == 6 )
      {
	send_to_char( C_DEFAULT, "That is an invalid direction.\n\r", ch );
	return;
      }

      to_room = ch->in_room;

      if ( ( pexit = to_room->exit[dir] ) == NULL ||
	   ( to_room = pexit->to_room ) == NULL )
      {
	send_to_char( C_DEFAULT, "You cannot shoot in that direction.\n\r", ch );
	return;
      }

      if ( exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
      {
	send_to_char( C_DEFAULT, "That exit is blocked.\n\r", ch );
	return;
      }

      if ( ( victim = get_char_closest( ch, dir, arg4 ) ) == NULL )
      {
       send_to_char( C_DEFAULT, "They aren't anywhere.\n\r", ch );
       return;
      }

      obj_from_char( ammo );
      obj_to_room( ammo, ch->in_room );

      MAX_DIST = weapon->value[4];

     if ( ammo->item_type == ITEM_ARROW )
     {
      sprintf( buf,
       "You pull back the string on $p and release %s screaming to %s towards $N.",
       ammo->short_descr, dir_name[dir] );
      act( AT_WHITE, buf, ch, weapon, victim, TO_CHAR );

      sprintf( buf, 
       "$n pulls back the string of $p and sends %s screaming to %s.",
       ammo->short_descr, dir_name[dir] );
      act( AT_WHITE, buf, ch, weapon, victim, TO_ROOM );
     }
     else
     if ( ammo->item_type == ITEM_BOMB )
     {
      sprintf( buf,
       "You kneel down and load %s into $p.", ammo->short_descr );
      act( AT_WHITE, buf, ch, weapon, victim, TO_CHAR );
      sprintf( buf,
       "You pull the trigger on $p, sending %s screaming to %s towards $N.",
       ammo->short_descr, dir_name[dir] );
      act( AT_WHITE, buf, ch, weapon, victim, TO_CHAR );

      sprintf( buf,
       "$n kneels down and loads %s into $p.", ammo->short_descr );
      act( AT_WHITE, buf, ch, weapon, victim, TO_ROOM );
      sprintf( buf,
       "$n pulls a trigger on $p, sending %s screaming to %s.",
       ammo->short_descr, dir_name[dir] );
      act( AT_WHITE, buf, ch, weapon, victim, TO_ROOM );
     }

      for ( dist = 1; dist <= MAX_DIST; dist++ )
      {
	obj_from_room( ammo );
	obj_to_room( ammo, to_room );

	if ( target_available( ch, victim, ammo ) )
	  break;

	if ( ( pexit = to_room->exit[dir] ) == NULL ||
	     ( to_room = pexit->to_room ) == NULL ||
               exit_blocked( pexit, ammo->in_room ) > EXIT_STATUS_OPEN )
	{
	  sprintf( buf, "$p flys in from $T and hits the %s wall.", 
		   dir_name[dir] );
	  act( AT_WHITE, buf, NULL, ammo, dir_noun[rev_dir[dir]], TO_ROOM );
	  sprintf( buf, "You shoot $p %d room%s $T, where it hits a wall.",
		   dist, dist > 1 ? "s" : "" );
	  act( AT_WHITE, buf, ch, ammo, dir_name[dir], TO_CHAR );
	  oprog_throw_trigger( ammo, ch );
          obj_from_room( ammo );
	  obj_to_room( ammo, to_room );

         if ( ammo->item_type == ITEM_BOMB )
         {
          ammo->owner = ch;
          bomb_explode( ammo, ammo->in_room );
         }

	  return;
	}
      }

      if ( victim == NULL )
      {
	act( AT_WHITE, 
	    "$p flies in from $T and falls to the ground.",
	    NULL, ammo, dir_noun[rev_dir[dir]], TO_ROOM );
	sprintf( buf,
		"$p falls to the ground %d room%s $T of here.",
		dist, dist > 1 ? "s" : "" );
	act( AT_WHITE, buf, ch, ammo, dir_name[dir], TO_CHAR );
	oprog_throw_trigger( ammo, ch );
        obj_from_room( ammo );
	obj_to_room( ammo, to_room );

         if ( ammo->item_type == ITEM_BOMB )
         {
          ammo->owner = ch;
          bomb_explode( ammo, ammo->in_room );
         }

	return;
      }

      act( AT_WHITE, "$p flys in from $T and hits $n!", victim, ammo,
	  dir_noun[rev_dir[dir]], TO_NOTVICT );
      act( AT_WHITE, "$p flys in from $T and hits you!", victim, ammo,
	  dir_noun[rev_dir[dir]], TO_CHAR );
      sprintf( buf, "$p was shot %d rooms %s and hit $N!", dist,
	      dir_name[dir] );
      act( AT_WHITE, buf, ch, ammo, victim, TO_CHAR );
      obj_from_room( ammo );

     if ( ammo->item_type == ITEM_ARROW )
     {
      if ( material_table[ammo->composition].type != MATERIAL_TYPE_METAL )
       extract_obj( ammo );
      else
       obj_to_room( ammo, to_room );
     }
     else
      obj_to_room( ammo, to_room );
 
     if ( ammo->item_type == ITEM_BOMB )
     {
      ammo->owner = ch;
      bomb_explode( ammo, ammo->in_room );
     }
     else
     {
      dam = ((3*number_range( ammo->value[1], weapon->value[2] ))/2);
      dam += (ammo->value[2] * 25);

      for ( paf = ammo->pIndexData->affected; paf; paf = paf->next )
       if ( paf->location == APPLY_THROWPLUS )
        dam += paf->modifier;

      damage( ch, victim, dam, DAMNOUN_PIERCE, DAM_PIERCE, TRUE );
      mystic_damage( ch, victim, ammo->composition );
     }

      if ( IS_NPC( victim ) )
      {
         if ( victim->level > 3 )
             victim->hunting = ch;
      }  
  return;
}

void do_charge( CHAR_DATA *ch, char *argument )
{
  char arg1[MAX_INPUT_LENGTH];
  char arg2[MAX_INPUT_LENGTH];
  char buf[MAX_STRING_LENGTH];
  CHAR_DATA *victim;
  ROOM_INDEX_DATA *to_room;
  EXIT_DATA *pexit;
  int dir = 0;
  int dist = 0;
  int MAX_DIST = 2;
  extern char *dir_noun [];

  argument = one_argument( argument, arg1 );
  argument = one_argument( argument, arg2 );

  if ( arg1[0] == '\0' )
  {
    send_to_char( C_DEFAULT, "Charge at whom?\n\r", ch );
    return;
  }

  to_room = ch->in_room;

  if ( arg2[0] == '\0' )
  {
   send_to_char( C_DEFAULT, "Charge in which direction?\n\r", ch );
   return;
  }

  if ( get_curr_str( ch ) >= 20 )
   MAX_DIST = 3;

  if ( get_curr_str( ch ) == 25 )
   MAX_DIST = 4;
      
  for ( dir = 0; dir < 6; dir++ )
   if ( arg2[0] == dir_name[dir][0] && !str_prefix( arg2, dir_name[dir] ) )
    break;

  if ( dir == 6 )
  {
   send_to_char( C_DEFAULT, "Charge in which direction?\n\r", ch );
   return;
  }

  if ( ( pexit = to_room->exit[dir] ) == NULL ||
       ( to_room = pexit->to_room ) == NULL )
  {
   send_to_char( C_DEFAULT, "You cannot charge in that direction.\n\r", ch );
   return;
  }

  if ( exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
  {
   send_to_char( C_DEFAULT, "That direction is blocked.\n\r", ch );
   return;
  }


  if ( ( victim = get_char_closest( ch, dir, arg1 ) ) == NULL )
  {
   send_to_char( C_DEFAULT, "They aren't anywhere.\n\r", ch );
   return;
  }

  if ( victim->in_room == ch->in_room )
  {
   send_to_char( C_DEFAULT, "You can only charge at someone from a different room.\n\r", ch );
   return;
  }

  if ( distancebetween( ch, victim, dir ) > MAX_DIST )
  {
   send_to_char( C_DEFAULT, "That person isn't close enough or isn't truly in that direction.\n\r", ch );
   return;
  }
  
  WAIT_STATE( ch, skill_table[gsn_charge].beats );

  for ( dist = 1; dist <= MAX_DIST; dist++ )
  {
   move_char( ch, dir, FALSE, FALSE );

   if ( ch->in_room == victim->in_room )
    break;

   if ( ( pexit = to_room->exit[dir] ) == NULL ||
        ( to_room = pexit->to_room ) == NULL ||
          exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
   {
    sprintf( buf, "You charge $p %d room%s $T, where you hit a wall.",
     dist, dist > 1 ? "s" : "" );
    act( AT_WHITE, buf, ch, NULL, dir_name[dir], TO_CHAR );
    sprintf( buf, "$n charges in from $T and hits the %s wall.", dir_name[dir] );
    act( AT_WHITE, buf, ch, NULL, dir_noun[rev_dir[dir]], TO_ROOM );
    return;
   }
  }

  if ( ch->in_room != victim->in_room )
  {
   sprintf( buf, "You charge $p %d room%s $T, where you stop.",
    dist, dist > 1 ? "s" : "" );
   act( AT_WHITE, buf, ch, NULL, dir_name[dir], TO_CHAR );
   return;
  }  

   sprintf( buf, "You charge %d rooms %s and hit $N!", dist,
    dir_name[dir] );
   act( AT_WHITE, buf, ch, NULL, victim, TO_CHAR );
   sprintf( buf, "$n charges in from %s and hits $N!",
    dir_name[rev_dir[dir]] );
   act( AT_WHITE, buf, ch, NULL, victim, TO_NOTVICT );
   sprintf( buf, "$n charges in from %s and hits you!",
    dir_name[rev_dir[dir]] );
   act( AT_WHITE, buf, ch, NULL, victim, TO_VICT );
   damage( ch, victim,
    dice(45, (get_curr_str(ch) / (7-dist))),
    DAMNOUN_SMASH, DAM_BASH, TRUE );

 update_skpell( ch, gsn_charge );
 return;
}

/*
void do_track( CHAR_DATA *ch, char *argument )
{
   ROOM_INDEX_DATA *room[30];
   ROOM_INDEX_DATA *to_room;
   ROOM_INDEX_DATA *in_room;
   EXIT_DATA *pexit[30];
   char arg[MAX_STRING_LENGTH];
   int vnums[30];
   int dist[30];
   int sdir[30];
   int dir[6];
   int nsdir;
  
   nsdir = 1;
   
   argument = one_argument( argument, arg );
   
   if ( is_number( arg ) || arg[0] == \0' )
   {
      send_to_char( AT_WHITE, "Track what?\n\r", ch );
      return;
   }
   
   in_room = ch->in_room;
   to_room = ch->in_room;
   
   for ( dir = 0; dir != -1; dir++ )
   {
      if ( !( pexit[nsdir] = ch->in_room->exit[dir] ) 
        || ( !( to_room = pexit[nsdir]->to_room ) ) )
      {
        if ( dir == 6 )
          dir = -1;
        continue;
        
        char_from_room( ch );
        char_to_room( ch, to_room );
        
        if ( get_char_room( ch, arg ) )
           break;
        
        
      }
  
   }
   
   if ( dir != -1 )
   {
     sprintf( log_buf, "You sense the trail of %s to the %s.\n\r",
          arg, dir_name[dir] );
     send_to_char( AT_WHITE, log_buf, ch );
     char_from_room( ch );
     char_to_room( ch, in_room );
   }
   
   if ( dir == -1 )
   {
     sprintf( log_buf, "You can't sense any %s from here.\n\r", arg );
     send_to_char( AT_WHITE, log_buf, ch );
     return;
   }
   
   return;
}
*/
  
void do_stun( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim;
  char   arg[MAX_INPUT_LENGTH];

    one_argument( argument, arg );

  if (!IS_NPC(ch) && !can_use_skpell( ch, gsn_stun ) )
  {
    send_to_char(C_DEFAULT, "You failed.\n\r", ch );
    return;
  }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   } 

  if ( victim->position == POS_STUNNED || IS_STUNNED( ch, STUN_TO_STUN ) )
    return;

  WAIT_STATE( ch, skill_table[gsn_stun].beats );

  if ( ( IS_NPC(ch) || number_percent() < ch->pcdata->learned[gsn_stun] ) &&
      number_percent() < (ch->level * 75) / victim->level )
  {
    STUN_CHAR( ch, 6, STUN_TO_STUN );
    STUN_CHAR( victim, 3, STUN_TOTAL );
    victim->position = POS_STUNNED;
    act( AT_WHITE, "You stun $N!", ch, NULL, victim, TO_CHAR );
    act( AT_WHITE, "$n stuns $N!", ch, NULL, victim, TO_NOTVICT );
    act( AT_WHITE, "$n stuns you!", ch, NULL, victim, TO_VICT );
    update_skpell(ch, gsn_stun);
    return;
  }

  send_to_char(C_DEFAULT, "You failed.\n\r", ch );
  return;
}

void do_berserk( CHAR_DATA *ch, char *argument )
{
  AFFECT_DATA af;

  if ( !ch->fighting )
    return;

  if ( !IS_NPC(ch) && ch->pcdata->learned[gsn_berserk] < number_percent() )
  {
    send_to_char(C_DEFAULT, "You failed.\n\r",ch);
    return;
  }

  af.type = gsn_berserk;
  af.level = ch->level;
  af.duration = ch->level / 10;
  af.bitvector = AFF_BERSERK;

  af.location = APPLY_STR;
  af.modifier = 3;
  affect_to_char2(ch, &af);

  af.location = APPLY_AGI;
  af.modifier = 2;
  affect_to_char2(ch, &af);

  af.location = APPLY_DEX;
  af.modifier = -2;
  affect_to_char2(ch, &af);

  send_to_char(AT_WHITE, "You suddenly go berserk.\n\r",ch);
  act(AT_WHITE, "$n suddenly goes berserk!", ch, NULL, NULL, TO_ROOM );
  update_skpell( ch, gsn_berserk );
  return;
}

void do_soulstrike( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim = ch->fighting;
  int dam;

  if ( !victim )
  {
    send_to_char( AT_WHITE, "You aren't fighting anyone!\n\r", ch );
    return;
  }

  if (!IS_NPC(ch) && ch->pcdata->learned[gsn_soulstrike] < number_percent( ))
  {
    send_to_char(AT_BLUE, "You failed.\n\r", ch );
    return;
  }

  dam = number_range( ch->level / 3, (ch->level * 2) / 3 );
  if ( ch->hit < dam * 2 )
  {
    send_to_char(AT_WHITE, "You do not have the strength.\n\r", ch );
    return;
  }

/* Don't need a check for update_pos, because of the previous check. */
  ch->hit -= dam;

  dam = number_range( dam * 3, dam * 5 );
  WAIT_STATE(ch, 2*PULSE_VIOLENCE);

  act( AT_BLUE, "Your soul strikes deep into $N.", ch, NULL, victim, TO_CHAR );
  act( AT_BLUE, "$n's soul strikes deep into you.", ch, NULL, victim, TO_VICT );
  act( AT_BLUE, "$n's soul strikes deep into $N.", ch, NULL, victim, TO_NOTVICT );
  damage(ch, victim, dam, gsn_soulstrike, DAM_HOLY, TRUE);

  update_skpell( ch, gsn_soulstrike );
  return;
}

void do_martyrize( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim = ch->fighting;
  int dam;

  if ( !victim )
  {
    send_to_char( AT_WHITE, "You aren't fighting anyone!\n\r", ch );
    return;
  }

  dam = ch->hit;
  ch->hit -= dam;

  WAIT_STATE(ch, 2*PULSE_VIOLENCE);

  act( AT_BLUE, "You martyrize yourself, they better appreciate this.", ch, NULL, victim, TO_CHAR );
  act( AT_BLUE, "$n martyrizes $mself, bye bye.", ch, NULL, victim, TO_VICT );
  act( AT_BLUE, "$n martyrizes $mself, what a nice guy.", ch, NULL, victim, TO_NOTVICT );
  dam *= 3;
  damage(ch, victim, dam, gsn_martyrize, DAM_HOLY, TRUE);

  update_skpell( ch, gsn_martyrize );

  return;
}

void do_multiburst( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim = ch->fighting;
  char	arg1 [ MAX_INPUT_LENGTH ];
  char	arg2 [ MAX_INPUT_LENGTH ];
  int	sn1;
  int	sn2;
  bool	legal1 = FALSE;
  bool	legal2 = FALSE;
  int	mana = 0;

  if ( IS_NPC( ch ) )
	return;
  if ( !can_use_skpell( ch, gsn_multiburst ) )
	{
	send_to_char( C_DEFAULT, "You're not enough of a mage to do multibursts.\n\r" , ch );
	return;
	}
  if ( IS_STUNNED( ch, STUN_MAGIC ) )
	{
	send_to_char( AT_LBLUE, "You are too stunned to multiburst.\n\r", ch );
	return;
	}

  if ( ch->pcdata->learned[gsn_multiburst] < number_percent( ) )
	{
	send_to_char( C_DEFAULT, "You fail your multiburst.\n\r", ch );
	return;
	}

  if ( !ch->fighting )
  {
   send_to_char( C_DEFAULT, "You aren't fighting.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( C_DEFAULT, "Your victim isn't here.\n\r", ch );
   return;
  }
 
  
  argument = one_argument( argument, arg1 );
  argument = one_argument( argument, arg2 );
  sn1 = skill_lookup( arg1 );
  sn2 = skill_lookup( arg2 );

  if ( sn1 != -1
  && can_use_skpell( ch, sn1 )
  && skill_table[sn1].target != TAR_CHAR_SELF
  && (*skill_table[sn1].spell_fun) != (*spell_null) )
	legal1 = TRUE;
  if ( sn2 != -1
  && can_use_skpell( ch, sn2 )
  && skill_table[sn2].target != TAR_CHAR_SELF
  && (*skill_table[sn2].spell_fun) != (*spell_null) )
	legal2 = TRUE;

  if ( !legal1 && !legal2 )
	{
	WAIT_STATE( ch, skill_table[gsn_multiburst].beats );
	send_to_char( C_DEFAULT, "Your multiburst fails.\n\r", ch );
	return;
	}
  if ( legal1 )
	{
	mana += SPELL_COST( ch, sn1 );
	mana += SPELL_COST( ch, sn1 ) * 0.2;
	}
  if ( legal2 )
	{
	mana += SPELL_COST( ch, sn2 );
	mana += SPELL_COST( ch, sn2 ) * 0.2;
	}
  mana += mana * 0.1;
  if ( ch->mana < mana && ch->level < LEVEL_IMMORTAL )
	{
	WAIT_STATE( ch, skill_table[gsn_multiburst].beats );
	send_to_char( C_DEFAULT, "You don't have enough mana to multiburst these spells.\n\r", ch );
	return;
	}
  WAIT_STATE( ch, skill_table[gsn_multiburst].beats );
  send_to_char( AT_RED, "You release a burst of energy!\n\r", ch );
  act( AT_RED, "$n releases a burst of energy.", ch, NULL, NULL, TO_ROOM );

  update_skpell( ch, gsn_multiburst );

  if ( legal1 )
	(*skill_table[sn1].spell_fun) ( sn1, 
					URANGE( 1, ch->level, LEVEL_HERO ),
					ch, victim );
  if ( victim->position != POS_DEAD && ch->in_room == victim->in_room )
  {
  if ( legal2 )
	(*skill_table[sn2].spell_fun) ( sn2,
					URANGE( 1, ch->level, LEVEL_HERO ),
					ch, victim );
  }
  if ( ch->level < LEVEL_IMMORTAL )
	MT( ch ) -= mana;
  return;
}
	
void do_challenge(CHAR_DATA *ch, char *argument)
{
  int award;
  char arg1[MAX_INPUT_LENGTH];
  char arg2[MAX_INPUT_LENGTH];
  MONEY_DATA amount;    

  if ( IS_NPC(ch) )
  {
    send_to_char( C_DEFAULT, "NPC's can't fight in the arena.\n\r", ch );
    return;
  }
  if ( arena.fch && arena.sch )
  {
    send_to_char( C_DEFAULT, "There are already two people fighting in the"
                  " arena.\n\r", ch );
    return;
  }
  if ( arena.cch )
  {
    sprintf(arg1, "%s is offering a challenge.  Type accept to accept it.\n\r",
            arena.cch->name);
    send_to_char( C_DEFAULT, arg1, ch );
    return;
  }
  argument = one_argument(argument, arg1);
  argument = one_argument(argument, arg2);
  
  if ( (is_number(arg1) && (award = atoi(arg1)) < 10 )
  || ( !is_number(arg1) && !is_number(arg2) )
  || ( is_number(arg2) && (award = atoi(arg2)) < 10 ) )
  {
    send_to_char( C_DEFAULT, "Syntax: challenge [player] <award>\n\r", ch );
    send_to_char( C_DEFAULT, " Player is the optional name of a specific person to challenge.\n\r", ch);
    send_to_char( C_DEFAULT, " Award is at least 10 gold coins.\n\r", ch );
    return;
  }
  else if ( (award = atoi(arg2)) )
  {
    DESCRIPTOR_DATA *d;
    bool found = FALSE;
    for ( d = descriptor_list; d; d = d->next )
	{
	if ( d->connected == CON_PLAYING
	&& is_name( NULL, arg1, d->character->name )
	&& d->character && ch ) 
		{
		found = TRUE;
		arena.och = d->character;
		break;
		}
	}
  if ( !found )
    {
    send_to_char( C_DEFAULT, "They aren't here.\n\r", ch );
    return;
    }
  }
  else
    award = atoi(arg1);
/* Convert and compare copper values: */
  if ( award*100 > ( ch->money.gold*C_PER_G + ch->money.silver*S_PER_G +
                     ch->money.copper ) )
  {
    send_to_char( C_DEFAULT, "You can't afford that.\n\r", ch );
    arena.och = NULL;
    return;
  }
  if ( !get_room_index(ROOM_ARENA_ENTER_F) ||
       !get_room_index(ROOM_ARENA_ENTER_S) )
  {
    send_to_char( C_DEFAULT, "An error has occured.  Please inform an Immortal.\n\r", ch );
    return;
  }
  arena.cch = ch;
  arena.count = 0;
  arena.award = award;
  amount.silver = amount.copper = 0;
  amount.gold = award;
  spend_money( &ch->money, &amount );
  if ( arena.och )
  sprintf(log_buf, "&C%s &cchallenges &C%s &cto a fight in the arena for &W%d &cgold coins.", 
	  ch->name, arena.och->name, award );
  else
  sprintf(log_buf, "&C%s &coffers a challenge in the arena for &W%d &cgold coins.",
          ch->name, award);
  log_string(log_buf, CHANNEL_LOG, -1);
  challenge(log_buf, 0, 0);
  send_to_char( C_DEFAULT, "Your challenge has been offered.\n\r", ch );
  return;
}

void do_accept(CHAR_DATA *ch, char *argument)
{
  CHAR_DATA *cch, *och;
  MONEY_DATA amount;
  
  if ( IS_NPC(ch) )
  {
    send_to_char( C_DEFAULT, "NPC's may not fight in the arena.\n\r", ch );
    return;
  }
  if ( !(cch = arena.cch) )
  {
    send_to_char( C_DEFAULT, "There is no challenge being offered.\n\r", ch );
    return;
  }
  if ( ch->fighting )
  {
    send_to_char( C_DEFAULT, "You are already fighting.\n\r", ch );
    return;
  }
  if ( ch == cch )
  {
    send_to_char( C_DEFAULT, "You can't accept your own challenge!\n\r", ch );
    return;
  }
  if ( ( och = arena.och ) && och != ch )
  {
    send_to_char( C_DEFAULT, "You are not the one being challenged.\n\r", ch );
    return;
  }
  if ( ( ch->money.gold*C_PER_G + ch->money.silver*S_PER_G +
         ch->money.copper ) < arena.award*100 )
  {
    send_to_char( C_DEFAULT, "You cannot afford that.\n\r", ch );
    return;
  }
 
  amount.silver = amount.copper = 0;
  amount.gold = arena.award;
  spend_money( &ch->money, &amount );
  arena.award *= 2;
  arena.fch = cch;
  arena.sch = ch;
  arena.cch = NULL;
  arena.och = NULL;
  send_to_char( C_DEFAULT, "Your challenge has been accepted.\n\r", cch );
  stop_fighting(cch, FALSE);
  act( AT_LBLUE, "A pentagram forms around $n and he slowly dissipates.",
       cch, NULL, NULL, TO_ROOM );
  char_from_room(cch);
  char_to_room(cch, get_room_index(number_range( 1104, 1122 )));
  do_look(cch, "auto");
  act( AT_LBLUE, "A pentagram forms around $n and he slowly dissipates.",
       ch, NULL, NULL, TO_ROOM );
  char_from_room(ch);
  char_to_room(ch, get_room_index(number_range( 1104, 1122 )));
  do_look(ch, "auto");
  sprintf(log_buf, "&C%s &chas accepted &C%s&c's challenge.",
          ch->name, cch->name);
  log_string(log_buf, CHANNEL_LOG, -1);
  challenge(log_buf, 0, 0);
  send_to_char(AT_RED, "Be prepared.\n\r", ch);
  send_to_char(AT_RED, "Be prepared.\n\r", cch);
  return;
}

void do_bite(CHAR_DATA *ch, char *argument)
{
  CHAR_DATA *victim;
  int	     dam;
  float	     wait_mod;
  char       arg[MAX_INPUT_LENGTH];

    one_argument( argument, arg );
  
  if(!IS_NPC(ch)
   && !can_use_skpell( ch, gsn_bite ) )
  {
   typo_message(ch);
    return;
  }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   } 

  if ( ch->level < 25 )
	wait_mod = 0.5;
  else if ( ch->level < 40 )
	wait_mod = 1;
  else if ( ch->level < 60 )
	wait_mod = 1.5;
  else if ( ch->level < 80 )
	wait_mod = 2;
  else
	wait_mod = 3;
  WAIT_STATE(ch, skill_table[gsn_bite].beats / wait_mod);
  if(IS_NPC(ch) || number_percent() < ch->pcdata->learned[gsn_bite])
  {
    dam = (get_curr_str(ch)*2);
    damage(ch, victim, dam, gsn_bite, DAM_SCRATCH, TRUE);
    update_skpell(ch, gsn_bite);
  }
  else
  {
    send_to_char(C_DEFAULT, "You failed.", ch);
    damage(ch, victim, 0, gsn_bite, DAM_SCRATCH, TRUE);
  }

  return;
}

void do_terrorize(CHAR_DATA *ch, char *argument)
{
  CHAR_DATA *victim;
  char      arg[MAX_INPUT_LENGTH];

    one_argument( argument, arg );

  if ( !IS_NPC( ch ) && !can_use_skpell( ch, gsn_terrorize ) )
  {
   typo_message( ch );
   return;
  }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

  if(is_safe(ch, victim))
    return;

  if (is_affected(victim, gsn_terrorize) )
    {
    act( AT_RED, "$N is already scared witless.", ch, NULL, victim, TO_CHAR );
    return;
    }

  WAIT_STATE(ch, skill_table[gsn_terrorize].beats);
  if(number_percent() < ch->pcdata->learned[gsn_terrorize])
  {
    AFFECT_DATA af;
    af.type	 = gsn_terrorize;
    af.level	 = ch->level;
    af.duration	 = 10;
    af.location	 = APPLY_DAMROLL;
    af.modifier	 = 0 - (get_curr_con( ch ) * 2);
    af.bitvector = 0;
    affect_to_char( victim, &af );
   
    af.location	 = APPLY_HITROLL;
    af.modifier	 = 0 - (get_curr_con( ch ) * 2);
    af.bitvector = AFF_ANTI_FLEE;
    affect_to_char( victim, &af );
  
    act( AT_RED, "Your blood curdling scream instills fear in $N.", 
         ch, NULL, victim, TO_CHAR );
    act( AT_RED, "You cringe in terror at the sound of $n's scream.",
	 ch, NULL, victim, TO_VICT );
    act( AT_RED, "$n screams and $N cringes back in terror.",
	 ch, NULL, victim, TO_NOTVICT );

    if ( !victim->fighting )
	set_fighting( victim, ch );
  }
  update_skpell( ch, gsn_terrorize );

  return;
}

void do_reflex( CHAR_DATA *ch, char *argument )
{
  AFFECT_DATA af;

  if ( IS_NPC( ch ) )
	return;

  if ( IS_AFFECTED( ch, AFF_HASTE ) || ch->race == RACE_QUICKSILVER )
	return;

  if ( !can_use_skpell( ch, gsn_reflex ) )
	return;

  WAIT_STATE( ch, skill_table[gsn_reflex].beats );
  if ( ch->pcdata->learned[gsn_reflex] < number_percent( ) )
	{
	send_to_char( C_DEFAULT, "You failed.\n\r", ch );
	return;
	}
  send_to_char( AT_RED, "You feel yourself more agile.\n\r", ch );
  act( AT_WHITE, "$n is moving more fluidly.", ch, NULL, NULL, TO_ROOM );
  af.type	 = gsn_reflex;
  af.level	 = ch->level;
  af.duration	 = ch->level / 4;
  af.location	 = APPLY_AGI;
  af.modifier	 = 3;
  af.bitvector	 = AFF_HASTE;
  affect_to_char( ch, &af );

  update_skpell( ch, gsn_reflex );

  return;
}

void do_rake( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA *victim;
    char       arg [ MAX_INPUT_LENGTH ];

    one_argument( argument, arg );

    if ( !IS_NPC( ch )
	&& !can_use_skpell( ch, gsn_rake ) )
    {
     typo_message(ch);
	return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   } 

    WAIT_STATE( ch, skill_table[gsn_rake].beats );
    if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_rake] )
    {
	damage( ch, victim, number_range( 1, get_curr_str(ch) *2 ), gsn_rake, DAM_SCRATCH, TRUE );
  	update_skpell( ch, gsn_rake );
    }
    else
	damage( ch, victim, 0, gsn_rake, DAM_SCRATCH, TRUE );

    return;
}

void do_facestrike( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim;
  int dam = 0;
  char   arg[MAX_INPUT_LENGTH];

    one_argument( argument, arg );

  if ( IS_NPC( ch ) )
	return;
  if ( !can_use_skpell( ch, gsn_facestrike ) )
  {
   typo_message(ch);
   return;
  }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
  
   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   } 

  WAIT_STATE(ch, skill_table[gsn_facestrike].beats);
  if ( ch->pcdata->learned[gsn_facestrike] > number_percent( ) )
	{
	int anatomy = ch->pcdata->learned[gsn_anatomyknow];
	if ( anatomy > 0
	&& number_percent( ) < number_range( anatomy / 9, anatomy / 4.5 ) )
		{
		update_skpell( ch, gsn_anatomyknow );	
	send_to_char( AT_RED, "You hit a pressure point!\n\r", ch );
	act( AT_RED, "$n hit one of $N's pressure points!",
		     ch, NULL, victim, TO_NOTVICT );
	act( AT_RED, "$n hit you with a precise shot.", 
		     ch, NULL, victim, TO_VICT );
		if ( number_percent( ) < 5 )
			{
			victim->hit = 1;
			}
		else if ( number_percent( ) < 25 )
			{
			STUN_CHAR( victim, 3, STUN_TOTAL );
			victim->position = POS_STUNNED;
			dam += 50;
			}
		else
			dam += 100;
		}
	dam += number_range( get_curr_str(ch) * 2, get_curr_str(ch) * 3 );
	if ( ch->pcdata->learned[gsn_enhanced_damage] > 0 )
		dam += dam / 4 * ch->pcdata->learned[gsn_enhanced_damage] / 150;
	damage( ch, victim, dam, gsn_facestrike, DAM_INTERNAL, TRUE );

	update_skpell( ch, gsn_facestrike );

	}
  else
	damage( ch, victim, 0, gsn_facestrike, DAM_INTERNAL, TRUE );
  return;
}

void do_black_star( CHAR_DATA *ch, char *argument )
{
  OBJ_DATA *star;
  AFFECT_DATA *pAf;

  if ( !can_use_skpell( ch, gsn_black_star ) )
  {
   typo_message(ch);
   return;
  }

  WAIT_STATE(ch, skill_table[gsn_black_star].beats);

  if ( IS_NPC(ch) || (!IS_NPC(ch) &&
   ch->pcdata->learned[gsn_black_star] > number_percent()) )
  {
   star = create_object( get_obj_index( OBJ_VNUM_BLACK_STAR ), 0 );
   star->timer = ch->level;
   star->level = ch->level;

   pAf             =   new_affect();
   pAf->location   =   APPLY_THROWPLUS;

   if ( !IS_NPC(ch) )
    pAf->modifier   =   dice( 2, ch->pcdata->learned[gsn_black_star] );
   else
    pAf->modifier   =   dice( 5, get_curr_int(ch) );

   pAf->type       =   -1;
   pAf->duration   =   -1;
   pAf->next       =   star->affected;
   star->affected  =   pAf;

   act(AT_BLUE, "You concentrate, channeling your chi energy into $p.",
    ch, star, NULL, TO_CHAR );
   act(AT_BLUE, "$n concentrates deeply and a $p takes shape in $s hands.",
    ch, star, NULL, TO_ROOM );
   obj_to_char( star, ch );
  }
  else
   act(AT_BLUE, "You lost your concentration.",
    ch, NULL, NULL, TO_CHAR );

   update_skpell( ch, gsn_black_star);

  return;
}

void do_flyingkick( CHAR_DATA *ch, char *argument )
{
  char arg1[MAX_INPUT_LENGTH];
  char arg2[MAX_INPUT_LENGTH];
  char buf[MAX_STRING_LENGTH];
  CHAR_DATA *victim;
  ROOM_INDEX_DATA *to_room;
  EXIT_DATA *pexit;
  int dir = 0;
  int dam = 0;
  int dist = 0;
  int MAX_DIST = 2;
  extern char *dir_noun [];

  argument = one_argument( argument, arg1 );
  argument = one_argument( argument, arg2 );

  if ( arg1[0] == '\0' )
  {
    send_to_char( C_DEFAULT, "Flying kick at whom?\n\r", ch );
    return;
  }

  to_room = ch->in_room;

  if ( arg2[0] == '\0' )
  {
   send_to_char( C_DEFAULT, "Kick in which direction?\n\r", ch );
   return;
  }

  if ( get_curr_str( ch ) >= 20 )
   MAX_DIST = 3;

  if ( get_curr_str( ch ) == 25 )
   MAX_DIST = 4;
      
  for ( dir = 0; dir < 6; dir++ )
   if ( arg2[0] == dir_name[dir][0] && !str_prefix( arg2, dir_name[dir] ) )
    break;

  if ( dir == 6 )
  {
   send_to_char( C_DEFAULT, "Kick in which direction?\n\r", ch );
   return;
  }

  if ( ( pexit = to_room->exit[dir] ) == NULL ||
       ( to_room = pexit->to_room ) == NULL )
  {
   send_to_char( C_DEFAULT, "You cannot kick in that direction.\n\r", ch );
   return;
  }

  if ( exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
  {
   send_to_char( C_DEFAULT, "That direction is blocked.\n\r", ch );
   return;
  }


  if ( ( victim = get_char_closest( ch, dir, arg1 ) ) == NULL )
  {
   send_to_char( C_DEFAULT, "They aren't anywhere.\n\r", ch );
   return;
  }

  if ( victim->in_room == ch->in_room )
  {
   send_to_char( C_DEFAULT, "You can only kick at someone from a different room.\n\r", ch );
   return;
  }

  if ( distancebetween( ch, victim, dir ) > MAX_DIST )
  {
   send_to_char( C_DEFAULT, "That person isn't close enough or isn't truly in that direction.\n\r", ch );
   return;
  }
  


  WAIT_STATE( ch, skill_table[gsn_flykick].beats );

  for ( dist = 1; dist <= MAX_DIST; dist++ )
  {
   move_char( ch, dir, FALSE, FALSE );

   if ( ch->in_room == victim->in_room )
    break;

   if ( ( pexit = to_room->exit[dir] ) == NULL ||
        ( to_room = pexit->to_room ) == NULL ||
          exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
   {
    sprintf( buf, "You fly $p %d room%s $T, where you hit a wall.",
     dist, dist > 1 ? "s" : "" );
    act( AT_WHITE, buf, ch, NULL, dir_name[dir], TO_CHAR );
    sprintf( buf, "$n flies in from $T and hits the %s wall.",
     dir_name[dir] );
    act( AT_WHITE, buf, ch, NULL, dir_noun[rev_dir[dir]], TO_ROOM );
    return;
   }
  }

  if ( ch->in_room != victim->in_room )
  {
   sprintf( buf, "You fly $p %d room%s $T, where you stop.",
    dist, dist > 1 ? "s" : "" );
   act( AT_WHITE, buf, ch, NULL, dir_name[dir], TO_CHAR );
   return;
  }  

   sprintf( buf, "You fly %d rooms %s and kick $N!", dist,
    dir_name[dir] );
   act( AT_WHITE, buf, ch, NULL, victim, TO_CHAR );
   sprintf( buf, "$n flies in from %s and kicks $N!",
    dir_name[rev_dir[dir]] );
   act( AT_WHITE, buf, ch, NULL, victim, TO_NOTVICT );
   sprintf( buf, "$n flies in from %s and kicks you!",
    dir_name[rev_dir[dir]] );
   act( AT_WHITE, buf, ch, NULL, victim, TO_VICT );

   dam = dice( 45, (get_curr_str(ch) / (7-dist)) );
   dam = kick_modifier( ch, victim, dam );
   damage( ch, victim, dam, gsn_flykick, DAM_BASH, TRUE );

   update_skpell( ch, gsn_flykick );
 return;
}

void do_slam( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim;
  char arg[ MAX_INPUT_LENGTH ];
  int  dam = 0;

 one_argument( argument, arg );

  if ( !IS_NPC( ch )
   && !can_use_skpell( ch, gsn_slam ) )
  {
   typo_message( ch );
   return;
  }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   } 

 WAIT_STATE( ch, skill_table[gsn_slam].beats );
 if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_slam] )
 {

   dam = number_range( get_curr_str( ch), get_curr_str( ch ) * 4 );

   act( AT_YELLOW, "You grab $N and try to slam $M!", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n grabs you and attempts to slam you to the ground!",
    ch, NULL, victim, TO_VICT );
   act( AT_RED, "$n grabs $N and attempts to slam $M to the ground!",
    ch, NULL, victim, TO_NOTVICT );
   damage( ch, victim, dam, gsn_slam, DAM_BASH, TRUE );
   update_skpell( ch, gsn_slam );
  return;
 }

 act( AT_YELLOW, "You go to grab $N, but $E squirms away!", ch, NULL,
  victim, TO_CHAR );
 act( AT_YELLOW, "$n goes to grab you, but you squirm away!", ch, NULL,
  victim, TO_VICT );
 act( AT_YELLOW, "$n goes to grab $N, but $E squirms away!", ch, NULL,
  victim, TO_NOTVICT );

 return;
}

void do_grapple( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim;
  char arg[ MAX_INPUT_LENGTH ];

 one_argument( argument, arg );

  if ( !IS_NPC( ch )
  && !can_use_skpell( ch, gsn_grapple ) )
    {
     typo_message( ch );
     return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

 if ( victim == ch )
 {
  send_to_char( AT_YELLOW, "You can not grapple yourself.\n\r", ch );
  return;
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   } 


 if ( IS_AFFECTED2( victim, AFF_GRASPED ) ||
      IS_AFFECTED2( victim, AFF_GRASPING) ||
      IS_AFFECTED2( ch, AFF_GRASPED ) ||
      IS_AFFECTED2( ch, AFF_GRASPING) )
 {
  send_to_char( AT_GREEN, "You can not grapple one who is grappling or being grappled.\n\r", ch );
  return;
 }

 WAIT_STATE( ch, skill_table[gsn_grapple].beats );

 if ( IS_NPC( ch ) || number_percent( ) < ch->pcdata->learned[gsn_grapple] )
 {
  if ( get_curr_agi( ch ) > get_curr_dex( victim ) )
  {
	AFFECT_DATA af;

   act( AT_YELLOW, "You grab $N and start to choke $M!",
    ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n grabs your neck and starts choking you violently!",
    ch, NULL, victim, TO_VICT );
   act( AT_YELLOW, "$n chokes $N.",
    ch, NULL, victim, TO_NOTVICT );
   
   damage( ch, victim,
    number_range(0, get_curr_str(ch) ),
    gsn_grapple, DAM_INTERNAL, TRUE );

	af.type      = gsn_grapple;
	af.duration  = 1;
	af.location  = APPLY_DEX;
	af.modifier  = -5;
	af.bitvector = AFF_GRASPED;
	affect_to_char2( victim, &af );

	af.type      = gsn_grapple;
	af.duration  = 1;
	af.location  = APPLY_STR;
	af.modifier  = 2;
	af.bitvector = AFF_GRASPING;
	affect_to_char2( ch, &af );
   update_skpell( ch, gsn_grapple );
  }
  else
  {
   act( AT_RED, "$N evades your grapple.",
    ch, NULL, victim, TO_CHAR );
   act( AT_YELLOW, "You evade $n's grapple.",
    ch, NULL, victim, TO_VICT );
  }       
 }
 return;
}

void do_stance( CHAR_DATA *ch, char *argument )
{
 char  arg[MAX_INPUT_LENGTH];

  argument = one_argument( argument, arg );

  if ( IS_NPC( ch ) )
	return;

  if ( ch->fighting )
  {
   send_to_char( AT_WHITE, "You can't change your stance mid-fight.\n\r", ch );
   return;
  }

  if ( arg[0] == '\0' )
  {
    send_to_char(AT_BLUE, "What stance would you like to fall into?\n\r", ch);
    return;
  }
  
  if ( !str_cmp( arg, "offensive" ) )
  {
  act( AT_RED, "You tense yourself into an offensive stance.", ch, NULL, NULL, TO_CHAR );
  act( AT_RED, "$n tenses $s muscles.", ch, NULL, NULL, TO_ROOM );
  ch->pcdata->attitude = 10;
   return;
  }
  else
  if ( !str_cmp( arg, "defensive" ) )
  {
  act( AT_RED, "You stand defensively.", ch, NULL, NULL, TO_CHAR );
  act( AT_RED, "$n tenses $s muscles.", ch, NULL, NULL, TO_ROOM );
  ch->pcdata->attitude = -10;
   return;
  }
  else
  if ( !str_cmp( arg, "normal" ) )
  {
  act( AT_RED, "You fall into a normal stance.", ch, NULL, NULL, TO_CHAR );
  act( AT_RED, "$n relaxes $mself.", ch, NULL, NULL, TO_ROOM );
  ch->pcdata->attitude = 0;
   return;
  }
  else
  if ( !str_cmp( arg, "protective" ) )
  {
  act( AT_RED, "You prepare yourself for incoming projectiles.",
   ch, NULL, NULL, TO_CHAR );
  act( AT_RED, "$n puts up $s shield for protection.",
   ch, NULL, NULL, TO_ROOM );
  ch->pcdata->attitude = ATTITUDE_PROTECTIVE;
   return;
  }
  else
  if ( !str_cmp( arg, "sparrow" ) )
  {
   if ( ch->pcdata->learned[gsn_sparrow_stance] )
   {
   act( AT_RED, "You drop into the sparrow stance.", ch, NULL, NULL, TO_CHAR );
   act( AT_RED, "$n crouches low to the ground, much like a sparrow.", ch, NULL, NULL, TO_ROOM );
   ch->pcdata->attitude = -13;
   return;
   }
   else
   {
   send_to_char( AT_WHITE, "Stance choices are: Normal, Offensive and Defensive.\n\r", ch);
   return;
   }
  }
  else
  if ( !str_cmp( arg, "crane" ) )
  {
   if ( ch->pcdata->learned[gsn_crane_stance] )
   {
   act( AT_RED, "You arch yourself into the crane stance.", ch, NULL, NULL, TO_CHAR );
   act( AT_RED, "$n arches $s back and stands as a crane.", ch, NULL, NULL, TO_ROOM );
   ch->pcdata->attitude = -12;
   return;
   }
   else
   {
   send_to_char( AT_WHITE, "Stance choices are: Normal, Offensive and Defensive.\n\r", ch);
   return;
   }
  }
  else
  if ( !str_cmp( arg, "monkey" ) )
  {
   if ( ch->pcdata->learned[gsn_monkey_stance] )
   {
   act( AT_RED, "You waver and fall into the drunken monkey stance.", ch, NULL, NULL, TO_CHAR );
   act( AT_RED, "$n trips and falls over $mself.", ch, NULL, NULL, TO_ROOM );
   ch->pcdata->attitude = -14;
   return;
   }
   else
   {
   send_to_char( AT_WHITE, "Stance choices are: Normal, Offensive and Defensive.\n\r", ch);
   return;
   }
  }
  else
  if ( !str_cmp( arg, "snake" ) )
  {
   if ( ch->pcdata->learned[gsn_snake_stance] )
   {
   act( AT_RED, "You tense your fists and stand as the snake.", ch, NULL, NULL, TO_CHAR );
   act( AT_RED, "$n tenses $s fists and stands as the snake.", ch, NULL, NULL, TO_ROOM );
   ch->pcdata->attitude = 11;
   return;
   }
   else
   {
   send_to_char( AT_WHITE, "Stance choices are: Normal, Offensive and Defensive.\n\r", ch);
   return;
   }
  }
  else
  if ( !str_cmp( arg, "dragon" ) )
  {
   if ( ch->pcdata->learned[gsn_dragon_stance] )
   {
   act( AT_RED, "You lean foward, standing as the dragon.", ch, NULL, NULL, TO_CHAR );
   act( AT_RED, "$n stands as the dragon.", ch, NULL, NULL, TO_ROOM );
   ch->pcdata->attitude = 12;
   return;
   }
   else
   {
   send_to_char( AT_WHITE, "Stance choices are: Normal, Offensive and Defensive.\n\r", ch);
   return;
   }
  }
  else
  if ( !str_cmp( arg, "tiger" ) )
  {
   if ( ch->pcdata->learned[gsn_tiger_stance] )
   {
   act( AT_RED, "You open your palms, standing as the tiger.", ch, NULL, NULL, TO_CHAR );
   act( AT_RED, "$n stands with open palms, like a tiger.", ch, NULL, NULL, TO_ROOM );
   ch->pcdata->attitude = 13;
   return;
   }
   else
   {
   send_to_char( AT_WHITE, "Stance choices are: Normal, Offensive and Defensive.\n\r", ch);
   return;
   }
  }
  else
  if ( !str_cmp( arg, "panther" ) )
  {
   if ( ch->pcdata->learned[gsn_panther_stance] )
   {
   act( AT_RED, "You crouch down, standing as the panther.", ch, NULL, NULL, TO_CHAR );
   act( AT_RED, "$n crouches down, standing as the panther.", ch, NULL, NULL, TO_ROOM );
   ch->pcdata->attitude = -11;
   return;
   }
   else
   {
   send_to_char( AT_WHITE, "Stance choices are: Normal, Offensive and Defensive.\n\r", ch);
   return;
   }
  }
  else
  if ( !str_cmp( arg, "bull" ) )
  {
   if ( ch->pcdata->learned[gsn_bull_stance] )
   {
   act( AT_RED, "You lean forward, standing as the bull.", ch, NULL, NULL, TO_CHAR );
   act( AT_RED, "$n leans forward, standing as the bull.", ch, NULL, NULL, TO_ROOM );
   ch->pcdata->attitude = 14;
   return;
   }
   else
   {
   send_to_char( AT_WHITE, "Stance choices are: Normal, Offensive and Defensive.\n\r", ch);
   return;
   }
  }
  else
  {
  send_to_char( AT_WHITE, "Stance choices are: Normal, Offensive and Defensive.\n\r", ch);
  return;
  }

 return;
}

void do_trip( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim;
  char arg[MAX_INPUT_LENGTH];

  one_argument( argument, arg );

  if ( IS_NPC( ch ) )
	return;

  if ( !can_use_skpell( ch, gsn_trip ) )
    {
    typo_message( ch );
    return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   } 

  if ( ch->pcdata->learned[gsn_trip] > number_percent( ) )
  {
   trip( ch, victim );
   update_skpell( ch, gsn_trip );
  }

  return;

}

void do_anklestrike( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim;
  char arg[MAX_INPUT_LENGTH];

  one_argument( argument, arg );

  if ( IS_NPC( ch ) )
	return;
  if ( !can_use_skpell( ch, gsn_anklestrike ) )
    {
    typo_message( ch );
    return;
    }

 if ( arg[0] != '\0' )
 {
  if ( !(victim=get_char_room( ch, arg )) )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }
 else
 {
  if ( !(victim = ch->fighting) )
  {
   send_to_char( AT_GREY, "You aren't fighting anyone.\n\r", ch );
   return;
  }

  if ( victim->in_room != ch->in_room )
  {
   send_to_char( AT_GREY, "They aren't here.\n\r", ch );
   return;
  }
 }

   if ( vision_impared( ch ) )
   {
    if ( !IS_NPC(ch) &&
     ch->pcdata->learned[gsn_blindfight] > number_percent( ) )
     update_skpell( ch, gsn_blindfight );
    else
    if ( IS_NPC(ch) && number_percent() > 85 )
     ;
    else
    {
     send_to_char( AT_GREY,
      "You grope around blindly, unable to do anything!\n\r", ch );
     return;
    }
   } 

  if ( ch->pcdata->learned[gsn_anklestrike] > number_percent( ) )
  {
	anklestrike( ch, victim );
	update_skpell( ch, gsn_anklestrike );
  }

  return;

}

void do_shriek( CHAR_DATA *ch, char *argument )
{
  CHAR_DATA *victim;
  OBJ_DATA *obj, *obj_next;
  OBJ_DATA *potion, *potion_next;
  int dam;
  int blown = 0;
  int max_blown = 0;

  if ( !IS_NPC( ch )
  &&   !can_use_skpell( ch, gsn_shriek ) )
	{
	typo_message( ch );
	return;
	}

  if ( !(victim = ch->fighting) )
	{
	send_to_char(C_DEFAULT, "You aren't fighting anyone.\n\r", ch );
	return;
	}
  max_blown = ( IS_NPC( ch ) ) ? 10 : 25;
  if ( ( obj = victim->carrying ) && !saves_spell( ch->level, victim ) )
    {
    for ( obj = victim->carrying; obj; obj = obj_next )
	{
	obj_next = obj->next_content;
	if ( blown >= max_blown )
	  break;
	if ( obj->deleted )
	  continue;
	switch ( obj->item_type )
	  {
	  default: continue;
	  case ITEM_POTION:
	  case ITEM_CONTAINER:
	  }
	if ( obj->item_type == ITEM_CONTAINER )
	  {
	  if ( !obj->contains )
	    continue;
	  for ( potion = obj->contains; potion; potion = potion_next )
	    {
	    potion_next = potion->next_content;
	    if ( blown >= max_blown )
	        break;
	    if ( potion->deleted )
		continue;
	    if ( potion->item_type != ITEM_POTION )
		continue;
	    if ( number_bits( 2 ) != 0 )
		{
		extract_obj( potion );
		act(AT_BLUE, "You feel something explode from within $p.",
		    victim, obj, NULL, TO_CHAR );
		blown++;
		}
	    }
	  continue;
	  }
	if ( number_bits( 2 ) != 0 )
	  {
	  act(AT_BLUE, "$p vibrates and explodes!", victim, obj, NULL, TO_CHAR );
	  extract_obj( obj );
	  blown++;
	  }
	}
    }
  dam = number_range( 10, get_curr_int(ch) * 2 );
  dam += blown * 10;
  damage( ch, victim, dam, gsn_shriek, DAM_DYNAMIC, TRUE );
  update_skpell( ch, gsn_shriek );
  WAIT_STATE( ch, skill_table[gsn_shriek].beats );		
}

void do_gust_of_wind ( CHAR_DATA *ch, char *argument )
{
  char arg1[MAX_INPUT_LENGTH];
  char arg2[MAX_INPUT_LENGTH];
  EXIT_DATA *pexit;
  CHAR_DATA *victim;
  ROOM_INDEX_DATA *from_room;
  int door;
  char buf1[256], buf2[256], buf3[256];

  argument = one_argument(argument, arg1);
  one_argument(argument, arg2);

  if ( !IS_NPC( ch )
  &&   !can_use_skpell( ch, gsn_gust_of_wind ) )
	{
	typo_message( ch );
	return;
	}

  if(arg1[0] == '\0')
  {
    send_to_char(AT_BLUE, "Push who what where?", ch);
    return;
  }

  if((victim = get_char_room(ch, arg1)) == NULL)
  {
    send_to_char(AT_BLUE, "They aren't here.\n\r", ch);
    return;
  }

  if (    ( victim->level >= LEVEL_IMMORTAL )                        
       || (    IS_NPC( victim )                                      
            && (    ( victim->pIndexData->pShop )                  
                 || IS_SET( ch->in_room->room_flags, ROOM_SMITHY )
                 || IS_SET( ch->in_room->room_flags, ROOM_BANK )   
                 || IS_SET( victim->act, ACT_NOPUSH )
               )
          )
     )
  {
    act(AT_BLUE, "$N stands firm against your torrents.", ch, NULL, victim, TO_CHAR);
    return;
  }

  if ( !str_cmp( arg2, "n" ) || !str_cmp( arg2, "north" ) ) door = 0;
  else if ( !str_cmp( arg2, "e" ) || !str_cmp( arg2, "east"  ) ) door = 1;
  else if ( !str_cmp( arg2, "s" ) || !str_cmp( arg2, "south" ) ) door = 2;
  else if ( !str_cmp( arg2, "w" ) || !str_cmp( arg2, "west"  ) ) door = 3;
  else if ( !str_cmp( arg2, "u" ) || !str_cmp( arg2, "up"    ) ) door = 4;
  else if ( !str_cmp( arg2, "d" ) || !str_cmp( arg2, "down"  ) ) door = 5;
  else door = dice(1,6) - 1;

  pexit = ch->in_room->exit[door];
  if( pexit == NULL || exit_blocked(pexit, ch->in_room) == EXIT_STATUS_PHYSICAL 
   || exit_blocked(pexit, ch->in_room) == EXIT_STATUS_SNOWIN )
  {
    act(AT_BLUE, "You call up a gust of wind and $N is thrown against a wall.", ch, NULL, victim, TO_CHAR);
    act(AT_BLUE, "A gust of wind slams $N against a wall.", ch, NULL, victim, TO_NOTVICT);
    act(AT_BLUE, "A gust of wind slams you against a wall, ouch.", ch, NULL, victim, TO_VICT);
    STUN_CHAR( ch, 10, STUN_TO_STUN);
    STUN_CHAR( victim, 5, STUN_TOTAL);
    victim->position = POS_STUNNED;
    return;
  }

  if ( exit_blocked( pexit, ch->in_room ) == EXIT_STATUS_MAGICAL )
 {
  act(AT_BLUE, "Your gust of wind pushes $N, but a force field prevents $S entry.", ch, NULL, victim, TO_CHAR );
  act(AT_BLUE, "A gust of wind pushes $N, but a force field makes $M bounce back.", ch, NULL, victim, TO_NOTVICT );
  act(AT_BLUE, "A gust of wind pushes you, but you bounce off a force field.", ch, NULL, victim, TO_VICT );
  return;
 }

  sprintf(buf1, "You call up a gust of wind $N is taken up and thrown %s.", dir_name[door]);
  sprintf(buf2, "A gust of wind takes up $N, pushing $M %s.", dir_name[door]);
  sprintf(buf3, "A gust of wind picks you up taking you %s.", dir_name[door]);
  act(AT_BLUE, buf2, ch, NULL, victim, TO_NOTVICT );
  act(AT_BLUE, buf1, ch, NULL, victim, TO_CHAR );
  act(AT_BLUE, buf3, ch, NULL, victim, TO_VICT );
  from_room = victim->in_room;
  eprog_enter_trigger( pexit, victim->in_room, victim );
  char_from_room(victim);
  char_to_room(victim, pexit->to_room);
  STUN_CHAR( ch, 10, STUN_TO_STUN);
  STUN_CHAR( victim, 5, STUN_TOTAL);
  victim->position = POS_STUNNED;
  act(AT_BLUE, "$n comes flying into the room.", victim, NULL, NULL, TO_ROOM);
  if ( (pexit = pexit->to_room->exit[rev_dir[door]]) &&
       pexit->to_room == from_room )
    eprog_exit_trigger( pexit, victim->in_room, victim );
  else
    rprog_enter_trigger( victim->in_room, victim );
    
  update_skpell( ch, gsn_gust_of_wind );
  return;
}

bool racial_attacks( CHAR_DATA *ch, CHAR_DATA *victim )
{
 if ( IS_NPC(ch) )
 {
  if ( ch->race_type == RACE_TYPE_DOG &&
   number_percent() < 15 )
  do_bite( ch, victim->name );
 }
 else if( !IS_NPC(ch) )
 {
  if ( ch->race == RACE_RDRAGON )
  {
   if ( ( number_percent( ) < 15 ) )
   {
    act( AT_YELLOW, "You belch a torrent of flames at $N!", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n engulfs you in a cone of flames!", ch, NULL, victim, TO_VICT );
    act( AT_YELLOW, "$n spits fire at $N.", ch, NULL, victim, TO_NOTVICT );

    if ( !IS_NPC(victim) )
     spell_fire_breath ( skill_lookup("fire breath"), 20, ch, victim );
    else
     spell_fire_breath ( skill_lookup("fire breath"), 40, ch, victim );
   }
  }
  else if (ch->race == RACE_DEMON )
  {
   if ( ( number_percent( ) < 20 ) )
   {
	AFFECT_DATA af;

	af.type      = gsn_blindness;
	af.duration  = 5;
	af.location  = APPLY_HITROLL;
	af.modifier  = -25;
	af.bitvector = AFF_BLIND;
	affect_join( victim, &af );
	act( AT_GREY, "$N is blinded by fire!", ch, NULL, victim, TO_CHAR );

    act( AT_YELLOW, "You lay down a brimstone shockwave at $N!", ch, NULL, victim, TO_CHAR );
    act( AT_RED, "$n launches a brimstone shockwave at you!", ch, NULL, victim, TO_VICT );
    act( AT_YELLOW, "A shockwave of fire and brimstone erupts from $n.", ch, NULL, victim, TO_NOTVICT );

    if ( !IS_NPC(victim) )
     spell_brimstone_shockwave ( skill_lookup("brimstone shockwave"),
      20, ch, victim );
    else
     spell_brimstone_shockwave ( skill_lookup("brimstone shockwave"), 40,
      ch, victim );
   }
  }
  else if (ch->race == RACE_SIREN )
  {
   if ( ( number_percent( ) < 20 ) )
   {
    if ( !IS_STUNNED( victim, STUN_COMMAND ) && !IS_STUNNED(ch, STUN_TO_STUN) )
    {
	AFFECT_DATA af;

	af.type      = 0;
	af.duration  = 2;
	af.location  = APPLY_HITROLL;
	af.modifier  = -50;
	af.bitvector = AFF_CONFUSED;
	affect_join( victim, &af );
	act( AT_GREY, "$N is confused!", ch, NULL, victim, TO_CHAR );
        STUN_CHAR( ch, 10, STUN_TO_STUN);     
        STUN_CHAR( victim, 5, STUN_TOTAL);
        victim->position = POS_STUNNED;

     act( AT_YELLOW, "You sing a sweet song of destruction!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n sings a sweet song of destruction!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$n sings a sweet song of destruction.", ch, NULL, victim, TO_NOTVICT );

     if ( !IS_NPC(victim) )
      spell_sweet_destruction ( skill_lookup("sweet destruction"), 20, ch,
	victim );
     else
      spell_sweet_destruction ( skill_lookup("sweet destruction"), 40, ch,
       victim );
    }
   }
  }
  else if (ch->race == RACE_GDRAGON )
  {
   if ( ( number_percent( ) < 23 ) )
   {
    if ( (number_percent( ) < 20 ) )
    {
     act( AT_YELLOW, "You breathe fire, engulfing $N!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n engulfs you in a torrent of flames!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$n spits fire at $N.", ch, NULL, victim, TO_NOTVICT );

     if ( !IS_NPC(victim) )
      spell_fire_breath ( skill_lookup("fire breath"), 20, ch, victim );
     else
      spell_fire_breath ( skill_lookup("fire breath"), 40, ch, victim );
    }
    else if ( (number_percent( ) < 40 ) )
    {
     act( AT_YELLOW, "You breathe gas, engulfing $N!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n engulfs you in a cloud of noxious gas!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$n breathes gas.", ch, NULL, victim, TO_NOTVICT );

     if ( !IS_NPC(victim) )
      spell_gas_breath ( skill_lookup("gas breath"), 20, ch, victim );
     else
      spell_gas_breath ( skill_lookup("gas breath"), 40, ch, victim );
    }
    else if ( (number_percent( ) < 60 ) )
    {
     act( AT_YELLOW, "You belch a bolt of lightning, engulfing $N!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n spits a bolt of lightning at you!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$n spits lightning at $N.", ch, NULL, victim, TO_NOTVICT );

     if ( !IS_NPC(victim) )
      spell_lightning_breath ( skill_lookup("lightning breath"), 20, ch,
       victim );
     else
      spell_lightning_breath ( skill_lookup("lightning breath"), 40, ch,
       victim );
    }
    else if ( (number_percent( ) < 80 ) )
    {
     act( AT_YELLOW, "You spit a stream of acid at $N!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n covers you in acid!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$n spits acid at $N.", ch, NULL, victim, TO_NOTVICT );

     if ( !IS_NPC(victim) )
      spell_acid_breath ( skill_lookup("acid breath"), 20, ch, victim );
     else
      spell_acid_breath ( skill_lookup("acid breath"), 40, ch, victim );
    }
    else if ( (number_percent( ) < 100 ) )
    {
     act( AT_YELLOW, "You breathe ice shards and biting winds at $N!", ch, NULL, victim, TO_CHAR );
     act( AT_RED, "$n engulfs you in a freezing wind!", ch, NULL, victim, TO_VICT );
     act( AT_YELLOW, "$n spits frost at $N.", ch, NULL, victim, TO_NOTVICT );

     if ( !IS_NPC(victim) )
      spell_frost_breath ( skill_lookup("frost breath"), 20, ch, victim );
     else
      spell_frost_breath ( skill_lookup("frost breath"), 40, ch, victim );
    }
   }
  }
 }

 if ( !victim || victim->position == POS_DEAD
    || ch->in_room != victim->in_room )
  return FALSE;

 return TRUE;
}

int punch_modifier( CHAR_DATA *ch, CHAR_DATA *victim, int dam )
{
 OBJ_DATA *glove;

 if ( !(glove = get_eq_char( ch, WEAR_HANDS ) ) )
  return dam;

 dam += get_obj_weight( glove );

 mystic_damage( ch, victim, glove->composition );

 return dam;
}

int kick_modifier( CHAR_DATA *ch, CHAR_DATA *victim, int dam )
{
 OBJ_DATA *shoe;

 if ( !(shoe = get_eq_char( ch, WEAR_FEET ) ) )
  return dam;

 dam += get_obj_weight( shoe );

  mystic_damage( ch, victim, shoe->composition );

 return dam;
}

void special_weapon_damage (CHAR_DATA *ch, CHAR_DATA *victim, OBJ_DATA *wield, int dam )
{
    if ( dam > 0 && wield
       && is_wielding_flaming( ch )
       && number_percent( ) < 10 )
     damage( ch, victim, dice( 5, 15 ), gsn_flamehand, DAM_HEAT, FALSE );
    if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
     return;

    if ( dam > 0 && wield
       && is_wielding_icy( ch )
       && number_percent( ) < 20 )
     damage( ch, victim, dice( 5, 15 ), gsn_frosthand, DAM_COLD, FALSE  );
    if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
     return;

    if ( dam > 0 && wield
       && is_wielding_chaos( ch )
       && number_percent( ) < 20 )
     damage( ch, victim, dice( 5, 15 ), gsn_chaoshand, DAM_DYNAMIC, FALSE  );
    if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
     return;

    if ( dam > 0 && wield
       && is_wielding_shock( ch )
       && number_percent( ) < 20 )
     damage( ch, victim, dice( 5, 15 ), gsn_shockhand, DAM_NEGATIVE, FALSE  );
    if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return;

    if ( dam > 0 && wield
       && is_wielding_rainbow( ch )
       && number_percent( ) < 20 )
     damage( ch, victim, dice( 5, 15 ), gsn_colorhand, DAM_POSITIVE, FALSE  );
    if ( victim->position == POS_DEAD || ch->in_room != victim->in_room )
	return;

 return;
}

void mystic_damage( CHAR_DATA *ch, CHAR_DATA *victim, int material )
{
 int damtype = 0;
 int sn = 0;

 if ( material_table[material].type != MATERIAL_TYPE_MYSTIC )
  return;

 if ( material == MATERIAL_FIRE || material == MATERIAL_ENERGY )
 {
  damtype = DAM_HEAT;
  sn = gsn_flamehand;
 }

 if ( material == MATERIAL_PLASMA )
 {
  damtype = DAM_POSITIVE;
  sn = gsn_colorhand;
 }

 if ( material == MATERIAL_ICE )
 {
  damtype = DAM_COLD;
  sn = gsn_frosthand;
 }

 if ( number_percent() < dice( 3, 10 ) )
  damage( ch, victim, dice( 5, 15 ), sn, damtype, FALSE  );

 return;
}

void death_xp_loss( CHAR_DATA *victim )
{
  int base_xp, xp_lastlvl, xp_loss, classes, mod;
  classes = number_classes( victim );
  if ( victim->level < LEVEL_HERO )
  {
   xp_lastlvl = 200 * victim->level;
   xp_loss = xp_lastlvl / 4;
   gain_exp( victim, xp_loss );
  }
  else if ( victim->level < L_CHAMP3 )
  {
    mod = 1;
    base_xp = classes == 1 ? 100000 : 200000;
    xp_lastlvl = base_xp * classes;
    switch ( victim->level )
      {
      case LEVEL_HERO: mod = 1;
      case L_CHAMP1:   mod = 4;
      case L_CHAMP2:   mod = 10;
      }
    xp_lastlvl = xp_lastlvl * mod;

    if ( victim->exp > xp_lastlvl )
    {
     xp_loss = (xp_lastlvl - victim->exp ) / 2;
     xp_loss = UMAX( -10000 * classes, xp_loss );
     gain_exp( victim, xp_loss );
    }
   }
  return;
}

void death_message( CHAR_DATA *killer, CHAR_DATA *victim )
{

/* This is a long and drawn out process with the dual ports, all to get one stupid
   info message through ;) -Flux */

    if ( IS_NPC(killer) )
    {  
     if ( victim->pcdata->port == PORT_PAP )
     {
      switch( dice(1,3) )
      {
       case 1:
        info("%s has been brutally killed by %s, whatever.", (int)victim->name,
	   (int)killer->short_descr, PORT_PAP );
       break;

       case 2:
        info("%s has been killed by %s, let's party!", (int)victim->name,
	   (int)killer->short_descr, PORT_PAP  );
       break;

       case 3:
        info("%s has been slain by %s, does anyone really care?", (int)victim->name,
	   (int)killer->short_descr, PORT_PAP  );
       break;
      }
     }
     else if ( victim->pcdata->port == PORT_ROI )
     {
        info("%s has been senselessly killed by %s.", (int)victim->name,
	   (int)killer->short_descr, PORT_ROI );
     }
    }
    else
    {
     if ( victim->pcdata->port != killer->pcdata->port )
     { 
      if ( victim->pcdata->port == PORT_PAP )
      {
       switch( dice(1,3) )
       {
        case 1:
         info("%s has been MURDERED by %s, whatever.", (int)victim->name,
	    (int)killer->name, PORT_PAP );
        break;

        case 2:
         info("%s has been MURDERED by %s, let's party!", (int)victim->name,
	    (int)killer->name, PORT_PAP  );
        break;

        case 3:
         info("%s has been MURDERED by %s, does anyone really care?", (int)victim->name,
	    (int)killer->name, PORT_PAP  );
        break;
       }
      }
      else if ( victim->pcdata->port == PORT_ROI )
      {
       info("%s has been senselessly MURDERED by %s, this is unspeakable!", (int)victim->name,
	  (int)killer->name, PORT_ROI );
      }

      if ( killer->pcdata->port == PORT_PAP )
       info("%s has MURDERED by %s, let's party!", (int)killer->name,
	  (int)victim->name, PORT_PAP  );
      else if ( killer->pcdata->port == PORT_ROI )
       info("%s has MURDERED %s, down with the intruders!", (int)victim->name,
	  (int)killer->name, PORT_ROI );
     }
     else
     {
      if ( victim->pcdata->port == PORT_PAP )
      {
       switch( dice(1,3) )
       {
        case 1:
         info("%s has been MURDERED by %s, whatever.", (int)victim->name,
	    (int)killer->name, PORT_PAP );
        break;

        case 2:
         info("%s has been MURDERED by %s, let's party!", (int)victim->name,
	    (int)killer->name, PORT_PAP  );
        break;

        case 3:
         info("%s has been MURDERED by %s, does anyone really care?", (int)victim->name,
	    (int)killer->name, PORT_PAP  );
        break;
       }
      }
      else if ( victim->pcdata->port == PORT_ROI )
      {
       info("%s has been senselessly MURDERED by %s, this is unspeakable!", (int)victim->name,
	  (int)killer->name, PORT_ROI );
      }
     }
    }

 return;
}

/* Gun coding functions -Flux */
void ranged_multi( CHAR_DATA *ch, CHAR_DATA *victim )
{
    OBJ_DATA *gun = NULL;
    OBJ_DATA *gun2 = NULL;
    int shot;
    int numshot;
    OBJ_DATA *obj;
    bool gunfound = FALSE;
    bool gun2found = FALSE;

    if ( !ch->engaged )
     set_shooting( ch, victim );

    if ( !ch->fighting )
     set_fighting( ch, victim );

    if ( ch->in_room == victim->in_room )
    {
     stop_shooting( ch, TRUE );
     return;
    }

    if ( ch->position <= POS_STUNNED )
    {
     send_to_char(AT_WHITE,
	"You are stunned, but will probably recover.\n\r", ch );
     act(AT_WHITE, "$n is stunned, but will probably recover.",
	    ch, NULL, NULL, TO_ROOM );
     return;
    }

    if ( ( IS_AFFECTED2( ch, AFF_CONFUSED ) )
       && number_percent ( ) < 10 )
    {
      act(AT_YELLOW, "$n looks around confused at what's going on.", ch, NULL, NULL, TO_ROOM );
      send_to_char( AT_YELLOW, "You stand confused.\n\r", ch );
      return;
    }
    
    if ( !IS_NPC( ch ) )
    {
     if ( ch->pcdata->condition[COND_INSOMNIA] == 0 && number_percent() < 25 )
     {
      send_to_char( AT_WHITE, "You close your eyes for a second and try to catch some z's.\n\r", ch );
      act(AT_YELLOW, "$n closes $s eyes and appears to be trying to sleep.", ch, NULL, NULL, TO_ROOM );
      return;
     }
    }

     for ( obj = ch->carrying; obj; obj = obj->next_content )
     {
      if ( obj->deleted )
       continue;

      if ( IS_SET( obj->wear_loc, WEAR_NONE) )
       continue;

      if ( obj->item_type != ITEM_RANGED_WEAPON )
       continue;

      if ( IS_SET( obj->wear_loc, WEAR_WIELD ) )
      {
        gunfound = TRUE;
        gun = obj;
      }
      else if ( IS_SET( obj->wear_loc, WEAR_WIELD_2 ) )
      {
        gun2found = TRUE;
        gun2 = obj;
      }
      else
       continue;
     }

     if (!gunfound && !gun2found)
     {
      send_to_char(AT_BLUE,
       "You no longer have a ranged weapon equipped.\n\r", ch);
      stop_shooting( ch, FALSE );
      return;
     }

   if ( gunfound )
   {
    if ( !(gun) )
     return;

    if ( gun->value[3] == GUN_NORMAL )
     numshot = 1;
    else
     numshot = gun->value[3] * 2;

     for ( shot = 0; shot < numshot; shot++ )
      ranged_hit( ch, victim );
   }
   
   if ( gun2found )
   {
    if ( !(gun2) )
     return; 

    if ( gun2->value[3] == GUN_NORMAL )
     numshot = 1;
    else
     numshot = gun2->value[3] * 2;

     for ( shot = 0; shot < numshot; shot++ )
      ranged_hit_dual( ch, victim );
   }

 return;
}

void do_engage( CHAR_DATA *ch, char *argument )
{
    CHAR_DATA 		*victim;
    OBJ_DATA 		*ammo = NULL;
    OBJ_DATA 		*ammo2 = NULL;
    OBJ_DATA 		*gun = NULL;
    OBJ_DATA 		*gun2 = NULL;
    OBJ_DATA 		*clip = NULL;
    OBJ_DATA		*obj;
    ROOM_INDEX_DATA 	*to_room;
    ROOM_INDEX_DATA 	*in_room;
    EXIT_DATA 		*pexit;
    char       		arg1[ MAX_INPUT_LENGTH ];
    char       		arg2[ MAX_INPUT_LENGTH ];
    int 		MAX_DIST = 0;
    int  		dir;
    bool 		ammocheck  = FALSE;
    bool 		ammo2check = FALSE;
    bool		gunfound   = FALSE;
    bool		gun2found  = FALSE;

    argument = one_argument( argument, arg1 );
    argument = one_argument( argument, arg2 );


    if ( ch->engaged != NULL )
    {
     send_to_char( C_DEFAULT,
      "You are already shooting at someone.\n\r", ch );
     return;
    }

     for ( obj = ch->carrying; obj; obj = obj->next_content )
     {
      if ( obj->deleted )
       continue;

      if ( IS_SET( obj->wear_loc, WEAR_NONE) )
       continue;

      if ( obj->item_type != ITEM_RANGED_WEAPON )
       continue;

      if ( IS_SET( obj->wear_loc, WEAR_WIELD ) )
      {
        gunfound = TRUE;
        gun = obj;
      }
      else if ( IS_SET( obj->wear_loc, WEAR_WIELD_2 ) )
      {
        gun2found = TRUE;
        gun2 = obj;
      }
      else
       continue;
     }

     if (!gunfound && !gun2found)
     {
      send_to_char(AT_BLUE,
       "You do not have a ranged weapon equipped.\n\r", ch);
      stop_shooting( ch, FALSE );
      return;
     }

   if ( gunfound )
   {
    for ( ammo = gun->contains; ammo; ammo = ammo->next_content )
    {
     if ( !ammo || ammo->deleted )
      continue;

     if ( ammo->item_type != ITEM_BULLET && ammo->item_type != ITEM_CLIP )
      continue;

     if ( ammo->item_type == ITEM_BULLET )
     {
      ammocheck = TRUE;
      break;
     }

     if ( ammo->item_type == ITEM_CLIP )
     {
      clip = ammo;

      if ( !clip->contains )
       continue;

      for ( ammo = clip->contains; ammo; ammo = ammo->next_content )
      {
       if ( !ammo )
        continue;
  
       if ( ammo->item_type == ITEM_BULLET )
       {
        ammocheck = TRUE;
        break;
       }
      }
      if ( ammocheck == TRUE )
       break;
     }
    }
   }

   if ( gun2found )
   {
    for ( ammo2 = gun2->contains; ammo2; ammo2 = ammo2->next_content )
    {
     if ( !ammo2 || ammo2->deleted )
      continue;

     if ( ammo2->item_type != ITEM_BULLET && ammo2->item_type != ITEM_CLIP )
      continue;

     if ( ammo2->item_type == ITEM_BULLET )
     {
      ammo2check = TRUE;
      break;
     }

     if ( ammo2->item_type == ITEM_CLIP )
     {
      clip = ammo2;

      if ( !clip->contains )
       continue;

      for ( ammo2 = clip->contains; ammo2; ammo2 = ammo2->next_content )
      {
       if ( !ammo2 )
        continue;
  
       if ( ammo2->item_type == ITEM_BULLET )
       {
        ammo2check = TRUE;
        break;
       }
      }
      if ( ammo2check == TRUE )
       break;
     }
    }
   }

     if ( (!ammocheck || ammo->item_type == ITEM_CLIP) &&
          (!ammo2check || ammo2->item_type == ITEM_CLIP) )
     {
      send_to_char( AT_WHITE, "You are out of ammo.\n\r", ch );
      return;
     }
 
    in_room = ch->in_room;
    to_room = ch->in_room;
   if ( gunfound )
    MAX_DIST = gun->value[4];
   else if ( gun2found )
    MAX_DIST = gun2->value[4];

    if ( arg1[0] == '\0' )
    {
      send_to_char( C_DEFAULT, "Shoot at whom?\n\r", ch );
      return;
    }

    if ( arg2[0] == '\0' )
    {
	send_to_char( C_DEFAULT, "Shoot in which direction?\n\r", ch );
	return;
    }

      for ( dir = 0; dir < 6; dir++ )
       if ( arg2[0] == dir_name[dir][0]
           && !str_prefix( arg2, dir_name[dir] ) )
	  break;

      ch->rangedir = dir;

      if ( dir == 6 )
      {
	send_to_char( C_DEFAULT, "Shoot in which direction?\n\r", ch );
	return;
      }

      if ( ( pexit = to_room->exit[dir] ) == NULL ||
	   ( to_room = pexit->to_room ) == NULL )
      {
	send_to_char( C_DEFAULT, "You cannot shoot in that direction.\n\r", ch );
	return;
      }

      if ( exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
      {
	send_to_char( C_DEFAULT, "You cannot shoot through a door.\n\r", ch );
	return;
      }

    if ( ( victim = get_char_closest( ch, dir, arg1 ) ) == NULL )
    {
     send_to_char( C_DEFAULT, "They aren't anywhere.\n\r", ch );
     return;
    }

    if ( ( victim = get_char_closest( ch, dir, arg1 ) ) == NULL )
    {
     send_to_char( C_DEFAULT, "They aren't anywhere.\n\r", ch );
     return;
    }


    if ( IS_AFFECTED( ch, AFF_CHARM ) && ch->master == victim )
    {
        act(AT_BLUE, "$N is your beloved master!", ch, NULL, victim, TO_CHAR );
        return;
    }

    if ( is_safe( ch, victim ) )
    {
     send_to_char( C_DEFAULT, "That person is safe.\n\r", ch );
     return;
    }

   ranged_multi( ch, victim );
 return;
}

void ranged_hit( CHAR_DATA *ch, CHAR_DATA *victim )
{
    int       dir;
    OBJ_DATA *gun = NULL;
    OBJ_DATA *ammo;
    OBJ_DATA *shell;
    OBJ_DATA *clip = NULL;
    OBJ_DATA *obj;
    ROOM_INDEX_DATA *to_room;
    EXIT_DATA *pexit;
    AFFECT_DATA *paf;
    int dam = 0;
    int dist = 0;
    int MAX_DIST = 0;
    char buf[MAX_STRING_LENGTH];
    extern char *dir_noun []; 
    bool ammocheck = FALSE;    
    CHAR_DATA *xfirevict;
    bool gunfound = FALSE;

  if ( !ch->engaged )
   set_shooting( ch, victim );

  if ( !ch->fighting )
   set_fighting( ch, victim );

  for ( obj = ch->carrying; obj; obj = obj->next_content )
  {
   if ( obj->deleted )
    continue;

   if ( IS_SET( obj->wear_loc, WEAR_NONE) )
    continue;

   if ( obj->item_type == ITEM_RANGED_WEAPON &&
        IS_SET( obj->wear_loc, WEAR_WIELD ) )
   {
    gunfound = TRUE;
    gun = obj;
   }
  }


   if (!gunfound)
    return;

    for ( ammo = gun->contains; ammo; ammo = ammo->next_content )
    {
     if ( !ammo )
      continue;

     if ( ammo->item_type != ITEM_BULLET && ammo->item_type != ITEM_CLIP )
      continue;

     if ( ammo->item_type == ITEM_BULLET )
     {
	ammocheck = TRUE;
      break;
     }

     if ( ammo->item_type == ITEM_CLIP )
     {
      clip = ammo;

      if ( !clip->contains )
       continue;

      for ( ammo = clip->contains; ammo; ammo = ammo->next_content )
      {
       if ( ammo->item_type == ITEM_BULLET )
       {
        ammocheck = TRUE;
        break;
       }
      }
      if ( ammocheck == TRUE )
       break;
     }
    }

     if ( !ammocheck || ammo->item_type == ITEM_CLIP )
     {
      act( AT_RED,
       "You move to fire $p, but find that it's out of ammo!",
        ch, gun, NULL, TO_CHAR );
      act( AT_YELLOW,
       "$n's $p makes a clicking noise.", ch, gun, NULL, TO_ROOM );
      stop_shooting( ch, FALSE );
      return;
     }

     if ( !(fight_check_head( ch, victim, 0  )) )
      return;

     dam = (ammo->weight + (ammo->value[1] * 2));

      for ( paf = ammo->pIndexData->affected; paf; paf = paf->next )
       if ( paf->location == APPLY_THROWPLUS )
        dam += paf->modifier;

     if ( dam > 1000 && !IS_IMMORTAL(ch) )
     {
      sprintf( buf, "ranged_hit dam range > 1000 from %d to %d",
       ammo->value[1], ammo->value[2] );
      bug( buf, 0 );
      if ( ammo->name )
       bug( ammo->name, 0 );
     }

     to_room = ch->in_room;
     dir = ch->rangedir;

      if ( ( pexit = to_room->exit[dir] ) == NULL ||
	   ( to_room = pexit->to_room ) == NULL )
      {
	send_to_char( C_DEFAULT, "You cannot shoot in that direction.\n\r", ch );
       stop_shooting( ch, FALSE );
	return;
      }

      if ( exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
      {
	send_to_char( C_DEFAULT, "You cannot shoot through a barrier.\n\r", ch );
       stop_shooting( ch, FALSE );
	return;
      }

      shell = create_object( get_obj_index(OBJ_VNUM_BULLET_SHELL), 0 );
      shell->timer = number_range( 5, 10 );
      obj_to_room( shell, ch->in_room );

      MAX_DIST = gun->value[4];
        obj_from_obj( ammo, FALSE );
      for ( dist = 1; dist <= MAX_DIST; dist++ )
      {
        obj_from_room( ammo );
	obj_to_room( ammo, to_room );


	if ( target_available( ch, victim, ammo ) )
	  break;

	if ( ( pexit = to_room->exit[dir] ) == NULL ||
	     ( to_room = pexit->to_room ) == NULL ||
               exit_blocked( pexit, ammo->in_room ) > EXIT_STATUS_OPEN )
	{
	  sprintf( buf, "A $p flys in from $T and hits the %s wall.", 
		   dir_name[dir] );
	  act( AT_WHITE, buf, ch, ammo, dir_noun[rev_dir[dir]], TO_ROOM );
	  sprintf( buf, "You shoot $p %d room%s $T, where it hits a wall.",
		   dist, dist > 1 ? "s" : "" );

	  act( AT_WHITE, buf, ch, ammo, dir_name[dir], TO_CHAR );
          oprog_throw_trigger( ammo, ch );
	  obj_from_room( ammo );
          extract_obj( ammo );
          stop_shooting( ch, FALSE );
	  return;
	}
      }

      if ( !target_available( ch, victim, ammo ) )
      {
	act( AT_WHITE, 
	    "A $p flies in from $T and falls harmlessly to the ground.",
	    ch, ammo, dir_noun[rev_dir[dir]], TO_ROOM );
	sprintf( buf,
	"$p falls harmlessly to the ground %d room%s $T of here.",
		dist, dist > 1 ? "s" : "" );
	act( AT_WHITE, buf, ch, ammo, dir_name[dir], TO_CHAR );
	oprog_throw_trigger( ammo, ch );
	  obj_from_room( ammo );
          extract_obj( ammo );
        stop_shooting( ch, FALSE );
	return;
      }

     if (dist > 0 && (xfirevict = crossfire_check(ch, victim)) != NULL)
     {
      act( AT_WHITE, "$p flys in from $T and hits $n!", xfirevict, ammo,
          dir_noun[rev_dir[dir]], TO_NOTVICT );
      act( AT_WHITE, "$p flys in from $T and hits you!", xfirevict, ammo,
          dir_noun[rev_dir[dir]], TO_CHAR );
      sprintf( buf, "$p flew %d rooms %s and hit $N!", dist,
              dir_name[dir] );
      act( AT_WHITE, buf, ch, ammo, xfirevict, TO_CHAR );
      oprog_throw_trigger( ammo, ch );
      obj_from_room( ammo );
      extract_obj( ammo );
      damage( ch, xfirevict, dam, ammo->value[0], DAM_PIERCE, TRUE);
      if ( IS_NPC( xfirevict ) )
      {
         if ( xfirevict->level > 3 )
             xfirevict->hunting = ch;
      }
      return;
     }

      act( AT_WHITE, "$p flys in from $T and hits $n!", victim, ammo,
	  dir_noun[rev_dir[dir]], TO_NOTVICT );
      act( AT_WHITE, "$p flys in from $T and hits you!", victim, ammo,
	  dir_noun[rev_dir[dir]], TO_CHAR );
      sprintf( buf, "$p flew %d rooms %s and hit $N!", dist,
	      dir_name[dir] );
      act( AT_WHITE, buf, ch, ammo, victim, TO_CHAR );
      oprog_throw_trigger( ammo, ch );
      obj_from_room( ammo );
      extract_obj( ammo );
      damage( ch, victim, dam, ammo->value[0], DAM_PIERCE, TRUE);
 
      if ( IS_NPC( victim ) )
      {
         if ( victim->level > 3 )
             victim->hunting = ch;
      }  

 if ( (number_percent() - dice( 5, 3 ) ) > ch->pcdata->weapon[WEAPON_EXOTIC]
  && ch->pcdata->weapon[WEAPON_EXOTIC] < 95 )
 {
  send_to_char( AT_WHITE, "You feel more confident with your weapon.\n\r", ch );
  ch->pcdata->weapon[WEAPON_EXOTIC] += dice(1, 3);
  if ( ch->pcdata->weapon[WEAPON_EXOTIC] > 95 && ch->level < LEVEL_IMMORTAL )
   ch->pcdata->weapon[WEAPON_EXOTIC] = 95;
 }

 return;
}

void ranged_hit_dual( CHAR_DATA *ch, CHAR_DATA *victim )
{
    int       dir;
    OBJ_DATA *gun = NULL;
    OBJ_DATA *ammo;
    OBJ_DATA *shell;
    OBJ_DATA *clip = NULL;
    OBJ_DATA *obj;
    ROOM_INDEX_DATA *to_room;
    EXIT_DATA *pexit;
    AFFECT_DATA *paf;
    int dam = 0;
    int dist = 0;
    int MAX_DIST = 0;
    char buf[MAX_STRING_LENGTH];
    extern char *dir_noun []; 
    bool ammocheck = FALSE;    
    CHAR_DATA *xfirevict;
    bool gunfound = FALSE;

  if ( !ch->engaged )
   set_shooting( ch, victim );

  if ( !ch->fighting )
   set_fighting( ch, victim );

  for ( obj = ch->carrying; obj; obj = obj->next_content )
  {
   if ( obj->deleted )
    continue;

   if ( IS_SET( obj->wear_loc, WEAR_NONE) )
    continue;

   if ( obj->item_type == ITEM_RANGED_WEAPON &&
        IS_SET( obj->wear_loc, WEAR_WIELD_2 ) )
   {
    gunfound = TRUE;
    gun = obj;
   }
  }

   if (!gunfound)
    return;

    for ( ammo = gun->contains; ammo; ammo = ammo->next_content )
    {
     if ( !ammo )
      continue;

     if ( ammo->item_type != ITEM_BULLET && ammo->item_type != ITEM_CLIP )
      continue;

     if ( ammo->item_type == ITEM_BULLET )
     {
	ammocheck = TRUE;
      break;
     }

     if ( ammo->item_type == ITEM_CLIP )
     {
      clip = ammo;

      if ( !clip->contains )
       continue;

      for ( ammo = clip->contains; ammo; ammo = ammo->next_content )
      {
       if ( ammo->item_type == ITEM_BULLET )
       {
        ammocheck = TRUE;
        break;
       }
      }
      if ( ammocheck == TRUE )
       break;
     }
    }

     if ( !ammocheck || ammo->item_type == ITEM_CLIP )
     {
      act( AT_RED,
       "You move to fire $p, but find that it's out of ammo!",
        ch, gun, NULL, TO_CHAR );
      act( AT_YELLOW,
       "$n's $p makes a clicking noise.", ch, gun, NULL, TO_ROOM );
      stop_shooting( ch, FALSE );
      return;
     }

     if ( !(fight_check_head( ch, victim, 0 )) )
      return;

     dam = (ammo->weight + (ammo->value[1] * 2));

      for ( paf = ammo->pIndexData->affected; paf; paf = paf->next )
       if ( paf->location == APPLY_THROWPLUS )
        dam += paf->modifier;

     if ( dam > 1000 && !IS_IMMORTAL(ch) )
     {
      sprintf( buf, "ranged_hit dam range > 1000 from %d to %d",
       ammo->value[1], ammo->value[2] );
      bug( buf, 0 );
      if ( ammo->name )
       bug( ammo->name, 0 );
     }

     to_room = ch->in_room;
     dir = ch->rangedir;

      if ( ( pexit = to_room->exit[dir] ) == NULL ||
	   ( to_room = pexit->to_room ) == NULL )
      {
	send_to_char( C_DEFAULT, "You cannot shoot in that direction.\n\r", ch );
       stop_shooting( ch, FALSE );
	return;
      }

      if ( exit_blocked( pexit, ch->in_room ) > EXIT_STATUS_OPEN )
      {
	send_to_char( C_DEFAULT, "You cannot shoot through a barrier.\n\r", ch );
       stop_shooting( ch, FALSE );
	return;
      }

      shell = create_object( get_obj_index(OBJ_VNUM_BULLET_SHELL), 0 );
      shell->timer = number_range( 5, 10 );
      obj_to_room( shell, ch->in_room );

      MAX_DIST = gun->value[4];
       obj_from_obj( ammo, FALSE );
      for ( dist = 1; dist <= MAX_DIST; dist++ )
      {
        obj_from_room ( ammo );
	obj_to_room( ammo, to_room );


	if ( target_available( ch, victim, ammo ) )
	  break;

	if ( ( pexit = to_room->exit[dir] ) == NULL ||
	     ( to_room = pexit->to_room ) == NULL ||
               exit_blocked( pexit , ammo->in_room) > EXIT_STATUS_OPEN )
	{
	  sprintf( buf, "$p flys in from $T and hits the %s wall.", 
		   dir_name[dir] );
	  act( AT_WHITE, buf, ch, ammo, dir_noun[rev_dir[dir]], TO_ROOM );
	  sprintf( buf, "You shoot $p %d room%s $T, where it hits a wall.",
		   dist, dist > 1 ? "s" : "" );

	  act( AT_WHITE, buf, ch, ammo, dir_name[dir], TO_CHAR );
          oprog_throw_trigger( ammo, ch );
          obj_from_room( ammo );
          extract_obj( ammo );
          stop_shooting( ch, FALSE );
	  return;
	}
      }

      if ( !target_available( ch, victim, ammo ) )
      {
	act( AT_WHITE, 
	    "A $p flies in from $T and falls harmlessly to the ground.",
	    ch, ammo, dir_noun[rev_dir[dir]], TO_ROOM );
	sprintf( buf,
	"$p falls harmlessly to the ground %d room%s $T of here.",
		dist, dist > 1 ? "s" : "" );
	act( AT_WHITE, buf, ch, ammo, dir_name[dir], TO_CHAR );
	oprog_throw_trigger( ammo, ch );
          obj_from_room( ammo );
          extract_obj( ammo );
        stop_shooting( ch, FALSE );
	return;
      }
     if (dist > 0 && (xfirevict = crossfire_check(ch, victim)) != NULL)
     {
      act( AT_WHITE, "$p flys in from $T and hits $n!", xfirevict, ammo,
          dir_noun[rev_dir[dir]], TO_NOTVICT );
      act( AT_WHITE, "$p flys in from $T and hits you!", xfirevict, ammo,
          dir_noun[rev_dir[dir]], TO_CHAR );
      sprintf( buf, "$p flew %d rooms %s and hit $N!", dist,
              dir_name[dir] );
      act( AT_WHITE, buf, ch, ammo, xfirevict, TO_CHAR );
      oprog_throw_trigger( ammo, ch );
      obj_from_room( ammo );
      extract_obj( ammo );
      damage( ch, xfirevict, dam, ammo->value[0], DAM_PIERCE, TRUE);
      if ( IS_NPC( xfirevict ) )
      {
         if ( xfirevict->level > 3 )
             xfirevict->hunting = ch;
      }
      return;
     }

      act( AT_WHITE, "$p flys in from $T and hits $n!", victim, ammo,
	  dir_noun[rev_dir[dir]], TO_NOTVICT );
      act( AT_WHITE, "$p flys in from $T and hits you!", victim, ammo,
	  dir_noun[rev_dir[dir]], TO_CHAR );
      sprintf( buf, "$p flew %d rooms %s and hit $N!", dist,
	      dir_name[dir] );
      act( AT_WHITE, buf, ch, ammo, victim, TO_CHAR );
      oprog_throw_trigger( ammo, ch );
      obj_from_room( ammo );
      extract_obj( ammo );
      damage( ch, victim, dam, ammo->value[0], DAM_PIERCE, TRUE);
 
      if ( IS_NPC( victim ) )
      {
         if ( victim->level > 3 )
             victim->hunting = ch;
      }  

 if ( (number_percent() - dice( 5, 3 ) ) > ch->pcdata->weapon[WEAPON_EXOTIC]
  && ch->pcdata->weapon[WEAPON_EXOTIC] < 95 )
 {
  send_to_char( AT_WHITE, "You feel more confident with your weapon.\n\r", ch );
  ch->pcdata->weapon[WEAPON_EXOTIC] += dice(1, 3);
  if ( ch->pcdata->weapon[WEAPON_EXOTIC] > 95 && ch->level < LEVEL_IMMORTAL )
   ch->pcdata->weapon[WEAPON_EXOTIC] = 95;
 }

 return;
}

void set_shooting( CHAR_DATA *ch, CHAR_DATA *victim )
{
    char buf [ MAX_STRING_LENGTH ];

    if ( ch->engaged )
    {
        bug( "Set_shooting: already fighting", 0 );
        sprintf( buf, "...%s shooting %s at %d",
                ( IS_NPC( ch )     ? ch->short_descr     : ch->name     ),
                ( IS_NPC( victim ) ? victim->short_descr : victim->name ),
                victim->in_room->vnum );
        bug( buf , 0 );
        return;
    }

    if ( IS_AFFECTED( ch, AFF_SLEEP ) )
        affect_strip( ch, gsn_sleep );

 ch->engaged = victim;
 return;
}

void do_disengage( CHAR_DATA *ch, char *argument )
{
    if ( ch->engaged == NULL )
    {
     send_to_char( C_DEFAULT, "You are not shooting at anyone.\n\r", ch );
     return;
    }

    stop_shooting( ch, FALSE );

return;
}

void stop_shooting( CHAR_DATA *ch, bool fBoth )
{
    CHAR_DATA *fch;

    for ( fch = char_list; fch; fch = fch->next )
    {
        if ( fch == ch || ( fBoth && fch->engaged == ch ) )
        {
            fch->engaged       = NULL;
            fch->hunting        = NULL;
        }
    }

    return;
}


CHAR_DATA *crossfire_check( CHAR_DATA *ch, CHAR_DATA *fch )
{
    CHAR_DATA *rch;
    int randmiss, aim, number;   

    randmiss = number_percent();

    if ( randmiss > 10 )
	return NULL;

   if ( !IS_NPC(ch) )
    aim = get_curr_dex( ch ) + ch->pcdata->weapon[WEAPON_EXOTIC];
   else
    aim = get_curr_dex( ch ) + (ch->level / 10);
    number = number_percent();

    if ( number <= aim )
	return NULL;

    for ( rch = ch->in_room->people; rch; rch = rch->next_in_room )
    {
     if ( rch->deleted )
      continue;

     if ( rch->position == POS_DEAD )
      continue;

     if ( ch == rch )
      continue;

     if ( rch->level >= LEVEL_IMMORTAL )
      continue;

     if ( fch != rch )
      return rch;
    }

 return NULL;
}


bool target_available( CHAR_DATA *ch, CHAR_DATA *fch, OBJ_DATA *obj )
{
    CHAR_DATA *rch;

    for ( rch = obj->in_room->people; rch; rch = rch->next_in_room )
    {
     if( !rch || rch->position == POS_DEAD )
      continue;

     if ( !can_see( ch, rch ) || rch != fch )
      continue;

     if ( fch == rch )
      return TRUE;
    }

 return FALSE;
}

/* Handles the fighting AI for the mobs -Flux
 * Works in conjunction with:
 * The specials system (special.c) and group_tactics function (fight.c)
 */
void mobile_fight_engine( CHAR_DATA *ch )
{
 CHAR_DATA *victim;
 OBJ_DATA  *getobj;
 OBJ_DATA  *roomcont;
 int       leveldiff;
 char      namebuf[ MAX_INPUT_LENGTH ];
 char      buf[ MAX_INPUT_LENGTH ];
 char      victnamebuf[ MAX_INPUT_LENGTH ];
 int	   dir;
 int	   percent;


 if ( !ch || !IS_NPC(ch) )
  return;

 if ( !(victim = ch->fighting) )
 {
  stop_fighting( ch, FALSE );
  return;
 }

 if ( in_group( ch ) )
  if ( (group_tactics( ch )) )
   return;

 leveldiff = ch->level - victim->level;
 dir = find_first_step( ch->in_room, victim->in_room );

 switch( ch->fighting_style )
 {
/*  CASE_FIGHTING_STYLE_NORMAL: */
  default:
   if (MAX_HIT(ch) > 0)
    percent = (ch->hit * 100) / MAX_HIT(ch);
   else
    percent = -1;

   if ( ch->in_room != victim->in_room )
   {
    if ( !ch->hunting )
     ch->hunting = ch->fighting;
    hunt_victim( ch );
   }
   else
   {
    if ( number_percent( )
     < ( leveldiff < -5 ? ch->level / 4 : UMAX( 10, leveldiff ) )
     && number_bits(4) == 0
     && ((get_eq_char( victim, WEAR_WIELD ) != NULL)
     ||(get_eq_char( victim, WEAR_WIELD_2 ) != NULL)) )
      disarm( ch, victim );
   }
  break;

  case FIGHTING_STYLE_WIMPY:
   if (MAX_HIT(ch) > 0)
    percent = (ch->hit * 100) / MAX_HIT(ch);
   else
    percent = -1;

   if ( ch->in_room != victim->in_room )
    mobile_escape( ch );
   else
   {
    if ( percent < 50 )
    {
     do_trip( ch, "" );
     mobile_escape(ch);
     return;
    }
    else
     switch( dice( 1, 7 ) )
     {
      default: break;
      case 1:
       do_trip( ch, "" ); break;
      case 2:
       do_gouge( ch, "" ); break;
     }
   }
  break;

  case FIGHTING_STYLE_BEAST_WIMPY:
   if (MAX_HIT(ch) > 0)
    percent = (ch->hit * 100) / MAX_HIT(ch);
   else
    percent = -1;

   if ( ch->in_room != victim->in_room )
    mobile_escape( ch );
   else
   {
    if ( percent < 50 )
    {
     do_rake( ch, "" );
     mobile_escape(ch);
     return;
    }
    else
     switch( dice( 1, 7 ) )
     {
      default: break;
      case 1:
       do_rake( ch, "" ); break;
      case 2:
       do_bite( ch, "" ); break;
     }
   }
  break;

 case FIGHTING_STYLE_BEAST_AGGRESSIVE:
   if (MAX_HIT(ch) > 0)
    percent = (ch->hit * 100) / MAX_HIT(ch);
   else
    percent = -1;

   if ( ch->in_room != victim->in_room )
   {
    if ( !ch->hunting )
     ch->hunting = ch->fighting;

    if ( number_percent() > 85 &&
     ( distancebetween( ch, victim, dir ) < 3
     && distancebetween( ch, victim, dir ) != -1 ) )
    {
     sprintf( buf, "%s", victim->name );
     one_argument( buf, victnamebuf );
     sprintf( buf, "charge %s %s", victnamebuf, dir_name[dir] ); 
     interpret( ch, buf );
    }
    else   
     hunt_victim( ch );
   }
   else
   {
    switch( dice( 1, 4 ) )
    {
     default: break;
     case 1:
      do_rake( ch, "" ); break;
     case 2:
      do_bite( ch, "" ); break;
    }
   }
  break;

  case FIGHTING_STYLE_BARBARIAN:
   if (MAX_HIT(ch) > 0)
    percent = (ch->hit * 100) / MAX_HIT(ch);
   else
    percent = -1;

   if ( ch->in_room != victim->in_room )
   {
    if ( !ch->hunting )
     ch->hunting = ch->fighting;

    if ( number_percent() > 80 )
     if ( (getobj = get_eq_char(ch, WEAR_WIELD_2)) )
     {
      if ( dir > -1 )
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "%s", victim->name );
       one_argument( buf, victnamebuf );
       sprintf( buf, "throw %s %s %s", namebuf, dir_name[dir],
        victnamebuf ); 
       interpret( ch, buf );
      }
     }

     roomcont = ch->in_room->contents;
     for ( getobj = roomcont; getobj; getobj = getobj->next_content )
     { 
      if ( getobj->deleted )
       continue;
      if ( !getobj )
       continue;

      if( IS_SET( getobj->wear_flags, ITEM_WIELD ) &&
          IS_SET( getobj->wear_flags, ITEM_TAKE ) &&
          number_percent() > 75 )
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "get %s", namebuf );
       interpret( ch, buf );

       if ( getobj->item_type == ITEM_WEAPON )
       {
        if ( !(get_eq_char(ch, WEAR_WIELD)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "wield %s", namebuf );
         interpret( ch, buf );
        }
        else
        if ( !(get_eq_char(ch, WEAR_WIELD_2)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "dual %s", namebuf );
         interpret( ch, buf );
        }
       }
      }
      if ( !getobj->next_content )
       break;
     }

    if ( number_percent() > 85 &&
     ( distancebetween( ch, victim, dir ) < 3
     && distancebetween( ch, victim, dir ) != -1 ) )
    {
     sprintf( buf, "%s", victim->name );
     one_argument( buf, victnamebuf );
     sprintf( buf, "charge %s %s", victnamebuf, dir_name[dir] ); 
     interpret( ch, buf );
    }
    else   
     hunt_victim( ch );
   }
   else
   {
    if ( ch->level < 100 )
    {
     switch( dice( 1, 9 ) )
     {
      default: break;
      case 1:
       do_rake( ch, "" ); break;
      case 2:
       do_kick( ch, "" ); break;
      case 3:
       do_bite( ch, "" ); break;
      case 6:
       do_punch( ch, "" ); break;
      case 7:
       do_kick( ch, "" ); break;
     }
    }
    else
     switch( dice( 1, 9 ) )
     {
      default: break;
      case 1:
       do_rake( ch, "" ); break;
      case 2:
       do_kick( ch, "" ); break;
      case 3:
       do_bite( ch, "" ); break;
      case 4:
       do_slam( ch, "" ); break;
      case 5:
       do_slam( ch, "" ); break;
      case 6:
       do_punch( ch, "" ); break;
      case 7:
       do_kick( ch, "" ); break;
     }

     roomcont = ch->in_room->contents;
     for ( getobj = roomcont; getobj; getobj = getobj->next_content )
     { 
      if ( getobj->deleted )
       continue;
      if ( !getobj )
       continue;

      if( IS_SET( getobj->wear_flags, ITEM_WIELD ) &&
          IS_SET( getobj->wear_flags, ITEM_TAKE ) &&
          number_percent() > 75 )
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "get %s", namebuf );
       interpret( ch, buf );
       if ( getobj->item_type == ITEM_WEAPON )
       {
        if ( !(get_eq_char(ch, WEAR_WIELD)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "wield %s", namebuf );
         interpret( ch, buf );
        }
        else
        if ( !(get_eq_char(ch, WEAR_WIELD_2)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "dual %s", namebuf );
         interpret( ch, buf );
        }
       }
      }
      if ( !getobj->next_content )
      break;
     }
   }
  break;

 case FIGHTING_STYLE_MARTIAL:
  if (MAX_HIT(ch) > 0)
   percent = (ch->hit * 100) / MAX_HIT(ch);
  else
   percent = -1;

  if ( ch->in_room != victim->in_room )
  {
   if ( percent > 25 )
   {
    if ( !ch->hunting )
     ch->hunting = ch->fighting;

   if ( number_percent() > 65 )
   {
    do_black_star( ch, "" );
    interpret( ch, "dual black" );
   }  

   if ( number_percent() > 55 )
    if ( (getobj = get_eq_char(ch, WEAR_WIELD_2)) )
    {
     if ( dir > -1 )
     {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "%s", victim->name );
       one_argument( buf, victnamebuf );
      sprintf( buf, "throw %s %s %s", namebuf, dir_name[dir],
       victnamebuf ); 
      interpret( ch, buf );
     }
    }

      if ( number_percent() > 85 &&
       ( distancebetween( ch, victim, dir ) < 3
       && distancebetween( ch, victim, dir ) != -1 ) )
      {
       sprintf( buf, "%s", victim->name );
       one_argument( buf, victnamebuf );
       sprintf( buf, "flying %s %s", victnamebuf, dir_name[dir] ); 
       interpret( ch, buf );
      }
      else
       hunt_victim( ch );
     }
     else
      mobile_escape( ch );
   }
   else
   {
    if ( percent < 25 )
    {
     if ( ch->level < 75 )
     {
      if ( !IS_STUNNED( victim, STUN_TOTAL ) )
       do_stun( ch, "" );
     }
     else
      do_trip( ch, "" );

     mobile_escape( ch );
    }
    else
    {
     if ( ch->level < 75 )
     {
      switch( dice( 1, 11 ) )
      {
       default: break;
       case 1:
        do_trip( ch, "" ); break;
       case 2:
        do_kick( ch, "" ); break;
       case 3:
        do_punch( ch, "" ); break;
       case 4:
        do_uppercut_punch( ch, "" ); break;
       case 5:
        do_kidney_punch( ch, "" ); break;
       case 6:
        do_stun( ch, "" ); break;
       case 7:
        do_eyestrike( ch, "" ); break;
       case 8:
        do_anklestrike( ch, "" ); break;
       case 9:
        do_facestrike( ch, "" ); break;
      }
     }
     else
      switch( dice( 1, 11 ) )
      {
       default: break;
       case 1:
        do_trip( ch, "" ); break;
       case 2:
        do_kick( ch, "" ); break;
       case 3:
        do_punch( ch, "" ); break;
       case 4:
        do_uppercut_punch( ch, "" ); break;
       case 5:
        do_kidney_punch( ch, "" ); break;
       case 9:
        do_facestrike( ch, "" ); break;
      }
    }
   }
  break;

 case FIGHTING_STYLE_SNEAK:
  if (MAX_HIT(ch) > 0)
   percent = (ch->hit * 100) / MAX_HIT(ch);
  else
   percent = -1;

  if ( ch->in_room != victim->in_room )
  {
   if ( percent > 30 )
   {
    if ( !ch->hunting )
     ch->hunting = ch->fighting;

   if ( number_percent() > 75 )
    if ( (getobj = get_eq_char(ch, WEAR_WIELD)) )
    {
     if ( dir > -1 )
     {
      if ( (getobj->item_type == ITEM_WEAPON
       && getobj->value[8] != WEAPON_PIERCE)
       || getobj->item_type != ITEM_WEAPON )
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "%s", victim->name );
       one_argument( buf, victnamebuf );
       sprintf( buf, "throw %s %s %s", namebuf, dir_name[dir],
        victnamebuf ); 
       interpret( ch, buf );
      }
     }
    }

   if ( number_percent() > 80 )
    if ( (getobj = get_eq_char(ch, WEAR_WIELD_2)) )
    {
     if ( dir > -1 )
     {
      if ( (getobj->item_type == ITEM_WEAPON
       && getobj->value[8] != WEAPON_PIERCE)
       || getobj->item_type != ITEM_WEAPON )
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "%s", victim->name );
       one_argument( buf, victnamebuf );
      sprintf( buf, "throw %s %s %s", namebuf, dir_name[dir],
       victnamebuf ); 
      interpret( ch, buf );
     }
    }
   }

     roomcont = ch->in_room->contents;
     for ( getobj = roomcont; getobj; getobj = getobj->next_content )
     { 
      if ( getobj->deleted )
       continue;
      if ( !getobj )
       continue;

      if( IS_SET( getobj->wear_flags, ITEM_WIELD ) &&
          IS_SET( getobj->wear_flags, ITEM_TAKE ) &&
          number_percent() > 75 )
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "get %s", namebuf );
       interpret( ch, buf );
       if ( getobj->item_type == ITEM_WEAPON )
       {
        if ( !(get_eq_char(ch, WEAR_WIELD)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "wield %s", namebuf );
         interpret( ch, buf );
        }
        else
        if ( !(get_eq_char(ch, WEAR_WIELD_2)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "dual %s", namebuf );
         interpret( ch, buf );
        }
       }
      }
      if ( !getobj->next_content )
       break;
     }
     if ( !ch->engaged )
       hunt_victim( ch );
    }
    else
     mobile_escape( ch );
   }
   else
   {
    if ( percent < 30 )
    {
     if ( !IS_STUNNED( victim, STUN_TOTAL ) )
      do_trip( ch, "" );

     mobile_escape( ch );
    }
    else
    {
     switch( dice( 1, 9 ) )
     {
      default: break;
      case 1:
       do_trip( ch, "" ); break;
      case 2:
       do_kick( ch, "" ); break;
      case 3:
       do_punch( ch, "" ); break;
      case 4:
       do_gouge( ch, "" ); break;
      case 5:
       do_kidney_punch( ch, "" ); break;
     }

     roomcont = ch->in_room->contents;
     for ( getobj = roomcont; getobj; getobj = getobj->next_content )
     { 
      if ( getobj->deleted )
       continue;
      if ( !getobj )
       continue;

      if( IS_SET( getobj->wear_flags, ITEM_WIELD ) &&
          IS_SET( getobj->wear_flags, ITEM_TAKE ) &&
          number_percent() > 75 )
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "get %s", namebuf );
       interpret( ch, buf );
       if ( getobj->item_type == ITEM_WEAPON )
       {
        if ( !(get_eq_char(ch, WEAR_WIELD)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "wield %s", namebuf );
         interpret( ch, buf );
        }
        else
        if ( !(get_eq_char(ch, WEAR_WIELD_2)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "dual %s", namebuf );
         interpret( ch, buf );
        }
       }
      }
      if ( !getobj->next_content )
      break;
     }
    }
   }
  break;

 case FIGHTING_STYLE_WARLOCK:
  if (MAX_HIT(ch) > 0)
   percent = (ch->hit * 100) / MAX_HIT(ch);
  else
   percent = -1;

  if ( ch->in_room != victim->in_room )
  {
   if ( percent > 25 )
   {
    if ( !ch->hunting )
     ch->hunting = ch->fighting;

     if ( number_percent() > 75 )
      ranged_caster_spec( ch, TRUE );
     else
      hunt_victim( ch );
    }
    else
     mobile_escape( ch );
   }
   else
   {
    if ( percent < 25 )
    {
     interpret( ch, "cast 'blindness'" );
     mobile_escape( ch );
    }
    else
    {
     /* Examine call for special procedure */
     if ( ch->spec_fun != 0 )
      if ( ( *ch->spec_fun ) ( ch ) )
       break;
    }
   }
  break;

 case FIGHTING_STYLE_CLERIC:
  if (MAX_HIT(ch) > 0)
   percent = (ch->hit * 100) / MAX_HIT(ch);
  else
   percent = -1;

  if ( ch->in_room != victim->in_room )
  {
    if ( percent > 35 )
     ranged_caster_spec( ch, TRUE );
    else
     mobile_escape( ch );
   }
   else
   {
    if ( percent < 25 )
    {
     interpret( ch, "cast 'blindness'" );
     mobile_escape( ch );
    }
    else
    {
     /* Examine call for special procedure */
     if ( ch->spec_fun != 0 )
      if ( ( *ch->spec_fun ) ( ch ) )
       break;
    }
   }
  break;

 case FIGHTING_STYLE_STREET:
  if (MAX_HIT(ch) > 0)
   percent = (ch->hit * 100) / MAX_HIT(ch);
  else
   percent = -1;

  if ( ch->in_room != victim->in_room )
  {
   if ( percent > 25 )
   {
    if ( !ch->hunting )
     ch->hunting = ch->fighting;

   if ( ch->engaged && !(ammocheck((get_eq_char(ch, WEAR_WIELD)))) )
    interpret( ch, "disengage" );

   if ( number_percent() > 75 )
    if ( (getobj = get_eq_char(ch, WEAR_WIELD)) )
    {
     if ( dir > -1 )
     {
      if ( getobj->item_type == ITEM_RANGED_WEAPON
       && ammocheck( getobj ) )
      {
       sprintf( buf, "engage %s %s", victim->name, dir_name[dir] ); 
       interpret( ch, buf );
      }
      else
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "%s", victim->name );
       one_argument( buf, victnamebuf );
       sprintf( buf, "throw %s %s %s", namebuf, dir_name[dir],
        victnamebuf ); 
       interpret( ch, buf );
      }
     }
    }

   if ( number_percent() > 80 )
    if ( (getobj = get_eq_char(ch, WEAR_WIELD_2)) )
    {
     if ( dir > -1 )
     {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "%s", victim->name );
       one_argument( buf, victnamebuf );
      sprintf( buf, "throw %s %s %s", namebuf, dir_name[dir],
       victnamebuf ); 
      interpret( ch, buf );
     }
    }

     roomcont = ch->in_room->contents;
     for ( getobj = roomcont; getobj; getobj = getobj->next_content )
     { 
      if ( getobj->deleted )
       continue;
      if ( !getobj )
       continue;

      if( IS_SET( getobj->wear_flags, ITEM_WIELD ) &&
          IS_SET( getobj->wear_flags, ITEM_TAKE ) &&
          number_percent() > 75 )
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "get %s", namebuf );
       interpret( ch, buf );
       if ( getobj->item_type == ITEM_WEAPON )
       {
        if ( !(get_eq_char(ch, WEAR_WIELD)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "wield %s", namebuf );
         interpret( ch, buf );
        }
        else
        if ( !(get_eq_char(ch, WEAR_WIELD_2)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "dual %s", namebuf );
         interpret( ch, buf );
        }
       }
      }
      if ( !getobj->next_content )
       break;
     }
     if ( !ch->engaged )
     {
      if ( number_percent() > 85 &&
          ( distancebetween( ch, victim, dir ) < 3
            && distancebetween( ch, victim, dir ) != -1 ) )
      {
       sprintf( buf, "%s", victim->name );
       one_argument( buf, victnamebuf );
       sprintf( buf, "charge %s %s", victnamebuf, dir_name[dir] ); 
       interpret( ch, buf );
      }
      else
       hunt_victim( ch );
     }
    }
    else
     mobile_escape( ch );
   }
   else
   {
    if ( percent < 25 )
    {
     if ( ch->level < 100 )
     {
      if ( !IS_STUNNED( victim, STUN_TOTAL ) )
       do_slam( ch, "" );
     }
     else
      do_trip( ch, "" );

     mobile_escape( ch );
    }
    else
    {
     switch( dice( 1, 9 ) )
     {
      default: break;
      case 1:
       do_trip( ch, "" ); break;
      case 2:
       do_kick( ch, "" ); break;
      case 3:
       do_punch( ch, "" ); break;
      case 4:
       do_uppercut_punch( ch, "" ); break;
      case 5:
       do_kidney_punch( ch, "" ); break;
     }

     roomcont = ch->in_room->contents;
     for ( getobj = roomcont; getobj; getobj = getobj->next_content )
     { 
      if ( getobj->deleted )
       continue;
      if ( !getobj )
       continue;

      if( IS_SET( getobj->wear_flags, ITEM_WIELD ) &&
          IS_SET( getobj->wear_flags, ITEM_TAKE ) &&
          number_percent() > 75 )
      {
       sprintf( buf, "%s", getobj->name );
       one_argument( buf, namebuf );
       sprintf( buf, "get %s", namebuf );
       interpret( ch, buf );
       if ( getobj->item_type == ITEM_WEAPON )
       {
        if ( !(get_eq_char(ch, WEAR_WIELD)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "wield %s", namebuf );
         interpret( ch, buf );
        }
        else
        if ( !(get_eq_char(ch, WEAR_WIELD_2)) )
        {
         sprintf( buf, "%s", getobj->name );
         one_argument( buf, namebuf );
         sprintf( buf, "dual %s", namebuf );
         interpret( ch, buf );
        }
       }
      }
      if ( !getobj->next_content )
      break;
     }
    }
   } break;
  }

 return;
}

/* Handles first step of mobiles fleeing -Flux */
void mobile_escape( CHAR_DATA *ch )
{
 CHAR_DATA *victim;
 int       attempt;
 int       door;
 int       chstat;
 int       victstat;
 EXIT_DATA *pexit;
 char	   buf[MAX_INPUT_LENGTH];

 if ( !ch )
  return;

 if ( ch->position == POS_DEAD )
  return;

 if ( !(victim = ch->fighting) )
 {
  stop_fighting( ch, FALSE );
  return;
 }

 if ( ch->in_room == victim->in_room )
 {
  for ( attempt = 0; attempt < 6; attempt++ )
  {
   door = number_door( );

   if ( ( pexit = ch->in_room->exit[door] ) == 0
    || !pexit->to_room
    || IS_SET( pexit->to_room->room_flags, ROOM_NO_MOB ) )
    continue;

     chstat = get_curr_dex(ch) + get_curr_agi(ch) +
              get_curr_str(ch) + dice(2, 20);
     victstat = get_curr_dex(victim) + get_curr_agi(victim) +
              (get_curr_str(victim)/2) + dice(2, 10);

    if ( chstat < victstat )
     return;

    if ( IS_SET( pexit->exit_info, EX_CLOSED ) )
    {
     sprintf( buf, "open %s", dir_name[door] );
     interpret( ch, buf );
    }

    act(C_DEFAULT, "$n has fled!", ch, NULL, NULL, TO_ROOM );
    ch->fleeing_from = victim;
    ch->memory = NULL;
    interpret( ch, "flee" );
    break;
  }
 }

 return;
}

bool ammocheck( OBJ_DATA *gun )
{
 OBJ_DATA *ammo;
 OBJ_DATA *clip;
 bool	  ammocheck = FALSE;

  for ( ammo = gun->contains; ammo; ammo = ammo->next_content )
  {
     if ( !ammo )
      continue;

     if ( ammo->item_type != ITEM_BULLET && ammo->item_type != ITEM_CLIP )
      continue;

     if ( ammo->item_type == ITEM_BULLET )
     {
	ammocheck = TRUE;
      break;
     }

     if ( ammo->item_type == ITEM_CLIP )
     {
      clip = ammo;

      if ( !clip->contains )
       continue;

      for ( ammo = clip->contains; ammo; ammo = ammo->next_content )
      {
       if ( ammo->item_type == ITEM_BULLET )
       {
        ammocheck = TRUE;
        break;
       }
      }
      if ( ammocheck == TRUE )
       break;
     }
    }

 return ammocheck;
}

/* handles the constant fleeing for mobiles -Flux */
void mobile_flee( CHAR_DATA *ch )
{
 CHAR_DATA *victim;
 int dir;
 int door;
 int revdir;
 EXIT_DATA *pexit;
 bool      exitfound = FALSE;

 if ( !(victim = ch->fleeing_from) )
  return;

 dir = find_first_step( ch->in_room, victim->in_room );

 if ( dir == 0 )
  revdir = 2;
 if ( dir == 1 )
  revdir = 3;
 if ( dir == 2 )
  revdir = 0;
 if ( dir == 3 )
  revdir = 1;
 if ( dir == 4 )
  revdir = 5;
 if ( dir == 5 )
  revdir = 4;
 else
  revdir = -99;

 pexit = ch->in_room->exit[revdir];

 if ( !pexit || !pexit->to_room 
  || IS_SET( pexit->to_room->room_flags, ROOM_NO_MOB ) )
 {
  for( door = 0; door < 6; door++ )
  {
   if ( door == dir )
    continue;

   pexit = ch->in_room->exit[door];

   if ( !pexit || !pexit->to_room 
    || IS_SET( pexit->to_room->room_flags, ROOM_NO_MOB ) )
    continue;
   else
   {
    exitfound = TRUE;
    break;
   }
  }

  if ( !exitfound )
  {
   if ( ch->level > 15 )
    interpret( ch, "hide" );  
   interpret( ch, "rest" );
  }
 }
 else
  move_char( ch, revdir, FALSE, FALSE );

 return;
}

/* Handles extra hits from certain skills.
   Ensures that the first part of the blow actually hits -Flux */
void extra_hit( CHAR_DATA *ch, CHAR_DATA *victim, int dt )
{
 AFFECT_DATA af;
 OBJ_DATA    *obj;
 int chance;
 int dam;

 if ( dt == gsn_high_kick )
 {
  chance = get_curr_dex( victim ) - get_curr_dex(ch);

  if ( number_percent( ) < chance )
  {
   act( AT_RED, "You hear a crunch as you connect with $N's head.", ch,
    NULL, victim, TO_CHAR );
   act( AT_RED, "$n's high kick connects firmly with your head!", ch,
    NULL, victim, TO_VICT );
   act( C_DEFAULT, "$n's high kick hit $N's head!", ch, NULL, victim,
    TO_NOTVICT );
   damage( ch, victim, number_range( 20, ch->level ), gsn_high_kick,
    DAM_BASH, TRUE);
  }
 }
 else if ( dt == gsn_jump_kick )
 {
  chance = get_curr_dex(victim) - get_curr_dex(ch);
  if ( number_percent( ) < chance )
  {
   act( AT_RED, "You hear a crunch as you connect with $N's head.", ch,
    NULL, victim, TO_CHAR );
   act( AT_RED, "$n's jump kick connects firmly with your head!", ch,
    NULL, victim, TO_VICT );
   act( C_DEFAULT, "$n's jump kick hit $N's head!", ch, NULL, victim,
    TO_NOTVICT );
   damage( ch, victim, number_range( 30, ch->level ), gsn_jump_kick,
    DAM_BASH, TRUE );
  }
 }
 else if ( dt == gsn_spin_kick )
 {
  chance = get_curr_dex(victim) - get_curr_dex(ch);
  if ( number_percent( ) < chance )
  {
   act( AT_RED, "You hear a crunch as you connect with $N's head.", ch,
    NULL, victim, TO_CHAR );
   act( AT_RED, "$n's spin kick connects firmly with your head!", ch,
    NULL, victim, TO_VICT );
   act( C_DEFAULT, "$n's spin kick hit $N's head!", ch, NULL, victim,
    TO_NOTVICT );
   damage( ch, victim, number_range( 40, ch->level ), gsn_spin_kick,
    DAM_BASH, TRUE );
  }
 }
 else if ( dt == gsn_punch )
 {
  chance = get_curr_dex(victim) - get_curr_dex(ch);
  if ( number_percent( ) < chance )
  {
   act( AT_RED, "You hear a crunch as you connect with $N's head.", ch,
    NULL, victim, TO_CHAR );
   act( AT_RED, "$n's punch connects firmly with your head!", ch, NULL,
    victim, TO_VICT );
   act( C_DEFAULT, "$n's punch hit's home!", ch, NULL, victim, TO_NOTVICT );
   damage( ch, victim, number_range( 1, ch->level ), gsn_punch, DAM_BASH,
    TRUE );
  }
 }
 else if ( dt == gsn_jab_punch )
 {
  chance = get_curr_dex(victim) - get_curr_dex(ch);
  if ( number_percent( ) < chance )
  {
   act( AT_RED, "You hear a crunch as you connect with $N's head.", ch,
    NULL, victim, TO_CHAR );
   act( AT_RED, "$n's jab connects firmly with your head!", ch, NULL,
    victim, TO_VICT );
   act( C_DEFAULT, "$n's jab hit $N's head!", ch, NULL, victim, TO_NOTVICT );
   damage( ch, victim, number_range( 1, ch->level ), gsn_jab_punch,
    DAM_BASH, TRUE );
  }
 }
 else if ( dt == gsn_cross_punch )
 {
  chance = get_curr_dex(victim) - get_curr_dex(ch);
  if ( number_percent( ) < chance )
  {
   act( AT_RED, "You hear a crunch as you connect with $N's head.", ch,
    NULL, victim, TO_CHAR );
   act( AT_RED, "$n's cross connects firmly with your head!", ch, NULL,
    victim, TO_VICT );
   act( C_DEFAULT, "$n's cross hit $N's head!", ch, NULL, victim,
    TO_NOTVICT );
   damage( ch, victim, number_range( 1, ch->level ), gsn_cross_punch,
    DAM_BASH, TRUE );
  }
 }
 else if ( dt == gsn_roundhouse_punch )
 {
  chance = get_curr_dex(victim) - get_curr_dex(ch);
  if ( number_percent( ) < chance )
  {
   act( AT_RED, "You hear a crunch as you connect with $N's head.", ch,
    NULL, victim, TO_CHAR );
   act( AT_RED, "$n's roundhouse connects firmly with your head!", ch,
    NULL, victim, TO_VICT );
   act( C_DEFAULT, "$n's roundhouse hit $N's head!", ch, NULL, victim,
    TO_NOTVICT );
   damage( ch, victim, number_range( 1, ch->level ), gsn_roundhouse_punch,
    DAM_BASH, TRUE );
  }
 }
 else if ( dt == gsn_uppercut_punch )
 {
  chance = get_curr_dex(victim) - get_curr_dex(ch);
  if ( number_percent( ) < chance )
  {
   act( AT_RED, "You hear a crunch as you connect with $N's head.", ch,
    NULL, victim, TO_CHAR );
   act( AT_RED, "$n's uppercut connects firmly with your head!", ch,
    NULL, victim, TO_VICT );
   act( C_DEFAULT, "$n's uppercut hit $N's head!", ch, NULL, victim,
    TO_NOTVICT );
   damage( ch, victim, number_range( 1, ch->level ), gsn_uppercut_punch,
    DAM_BASH, TRUE );
  }
 }
 else if ( dt == gsn_tiger_claw )
 {
  if ( number_percent( ) > 95 )
  {
   OBJ_DATA *obj;
   act( AT_RED, "You break $N's nose, blood gets all over $M!", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n breaks your nose, you bleed all over yourself!", ch, NULL, victim, TO_VICT );
   act( C_DEFAULT, "$n breaks $N's nose, blood goes everywhere!", ch, NULL, victim, TO_ROOM );
   damage( victim, victim, (victim->hit / 50) + 1, gsn_wrack, DAM_INTERNAL, TRUE);

   obj		= create_object( get_obj_index(OBJ_VNUM_FINAL_TURD), 0 );
   obj->timer	= number_range( 3, 5 );
   obj_to_room( obj, victim->in_room );

   if ( ( (get_race_data(ch->race))->acidblood == 1 ) 
     || IS_AFFECTED2( victim, AFF_ACIDBLOOD) )
   {      
    act( AT_RED, "$N's skin begins to fizzle and melt from $N's acidic blood, eww.", ch, NULL, victim, TO_NOTVICT );
    act( AT_RED, "Your blood melts your skin, gross!", ch, NULL,victim, TO_VICT );
    spell_acid_spray( skill_lookup("acid spray"), 20, victim, victim );
   }
  }
 }
 else if ( dt == gsn_crane_bill )
 {
  act( AT_RED, "Your blow knocks the wind out of $N.", ch, NULL, victim,
   TO_CHAR );
  act( AT_RED, "$n's blow knocks the wind out of you!", ch, NULL,victim,
   TO_VICT );
  STUN_CHAR( victim, 2, STUN_MAGIC );
 }
 else if ( dt == gsn_crane_wing )
 {
  if ( number_percent() > 95 )
  {
   act( AT_RED, "You feel one of $N's teeth being broken under your tremendous blow.", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n breaks one of your teeth!", ch, NULL, victim, TO_VICT );
   dam = number_range( ch->pcdata->learned[gsn_crane_wing] / 7,
    get_curr_str(ch) );
   damage( ch, victim, dam, gsn_crane_wing, DAM_INTERNAL, TRUE );
  }
 }
 else if ( dt == gsn_crane_claw )
 {
  victim->position = POS_RESTING;
 }
 else if ( dt == gsn_panther_paw )
 {
  if ( number_percent() > 85 )
  {
   act( AT_RED, "You feel $N's jaw being broken under your tremendous blow.", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n breaks your jaw!", ch, NULL, victim, TO_VICT );
   dam = number_range( get_curr_str(ch), 
    ch->pcdata->learned[gsn_panther_paw] / 2 );
   damage( ch, victim, dam, gsn_panther_paw, DAM_INTERNAL, TRUE );
  }
 }
 else if ( dt == gsn_dragon_roar )
 {
  if ( number_percent() > 85 )
  {
   act( AT_RED, "You feel $N's jaw being broken under your tremendous blow.", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n breaks your jaw!", ch, NULL, victim, TO_VICT );
   dam = number_range( get_curr_str(ch) * 2,
    ch->pcdata->learned[gsn_dragon_roar]);
   damage( ch, victim, dam, gsn_dragon_roar, DAM_INTERNAL, TRUE );
  }
 }
 else if ( dt == gsn_dragon_blast )
 {
  if ( number_percent() > 85 )
  {
   act( AT_RED, "You feel $N's ribs being broken under your tremendous blow.", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n breaks one of your ribs!", ch, NULL, victim, TO_VICT );
   dam = number_range( get_curr_str(ch) * 2,
    ch->pcdata->learned[gsn_dragon_blast]);
   damage( ch, victim, dam, gsn_dragon_blast, DAM_INTERNAL, TRUE );
  }
 }
 else if ( dt == gsn_dragon_grab )
  victim->position = POS_RESTING;
 else if ( dt == gsn_sparrow_smash )
 {
  if ( number_percent() > 25 )
  {
   act( AT_RED, "You feel $N's ribs being broken under your tremendous blow.", ch, NULL, victim, TO_CHAR );
   act( AT_RED, "$n breaks one of your ribs!", ch, NULL, victim, TO_VICT );
   dam = number_range( ch->pcdata->learned[gsn_sparrow_smash],
    get_curr_str(ch) * 4 );
   damage( ch, victim,dam, gsn_sparrow_smash, DAM_INTERNAL, TRUE );
  }
 }
 else if ( dt == gsn_slam )
 {
  if ( !IS_STUNNED( victim, STUN_TOTAL ) && !IS_STUNNED( ch, STUN_TO_STUN ) )
  {
   STUN_CHAR( ch, 4, STUN_TO_STUN );
   STUN_CHAR( victim, 2, STUN_TOTAL );
   victim->position = POS_STUNNED;
  }
 }
 else if ( dt == gsn_circle )
 {
  if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_organ_donor] > 0
   && !IS_SET( victim->imm_flags, IMM_PIERCE ))
  {
   obj = get_eq_char( ch, WEAR_WIELD );
   if ( number_percent( ) > 90 )
   {
    act(C_DEFAULT, "You commit a spinal tap on $N!", ch, NULL,
     victim, TO_CHAR );

    af.type      = gsn_organ_donor;
    af.duration  = 5;
    af.location  = APPLY_HITROLL;
    af.modifier  = -50;
    af.bitvector = AFF_BLIND;
    affect_join( victim, &af );

    act( AT_GREY, "You are blinded by pain!", ch, NULL, victim, TO_VICT );
    update_skpell( ch, gsn_organ_donor );
   }
   else if ( number_percent( ) > 85 )
   {
    act(C_DEFAULT, "You puncture $N's lung!", ch, NULL, victim, TO_CHAR );

    if ( !IS_STUNNED( ch, STUN_TO_STUN ) )
    {
     STUN_CHAR( victim, 5, STUN_MAGIC );
     STUN_CHAR( ch, 5, STUN_TO_STUN );
     victim->position = POS_STUNNED;
    }

    act( AT_GREY, "Your lung is pierced, you can't breathe!", ch, NULL,
     victim, TO_VICT );
    update_skpell( ch, gsn_organ_donor );
   }
   else if ( number_percent( ) < 2 )
   {
    act(C_DEFAULT, "You pierce $N's heart!", ch, NULL, victim, TO_CHAR );
    act( AT_GREY, "$n punctures your heart, bye bye!", ch, NULL, victim,
     TO_VICT );
    update_skpell( ch, gsn_organ_donor );
    victim->hit = 1;
   }
  }

  if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_throttle] > 0
   && number_percent( ) > 75 )
  {
   if ( !IS_STUNNED( victim, STUN_COMMAND ) && !IS_STUNNED(ch, STUN_TO_STUN) )
   {
    act(C_DEFAULT, "You throttle $N and throw $M to the ground!",
     ch, NULL, victim, TO_CHAR );
    STUN_CHAR( ch, 20, STUN_TO_STUN);
    STUN_CHAR( victim, 10, STUN_COMMAND);
    STUN_CHAR( victim, 10, STUN_MAGIC);
    victim->position =  POS_STUNNED;
   }
  }
 }
 else if ( dt == gsn_backstab )
 {
  if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_organ_donor] > 0
   && !IS_SET( victim->imm_flags, IMM_PIERCE ))
  {
   obj = get_eq_char( ch, WEAR_WIELD );
   if ( number_percent( ) > 90 )
   {
    act(C_DEFAULT, "You commit a spinal tap on $N!", ch, NULL,
     victim, TO_CHAR );

    af.type      = gsn_organ_donor;
    af.duration  = 5;
    af.location  = APPLY_HITROLL;
    af.modifier  = -50;
    af.bitvector = AFF_BLIND;
    affect_join( victim, &af );

    act( AT_GREY, "You are blinded by pain!", ch, NULL, victim, TO_VICT );
    update_skpell( ch, gsn_organ_donor );
   }
   else if ( number_percent( ) > 85 )
   {
    act(C_DEFAULT, "You puncture $N's lung!", ch, NULL, victim, TO_CHAR );

    if ( !IS_STUNNED( ch, STUN_TO_STUN ) )
    {
     STUN_CHAR( victim, 5, STUN_MAGIC );
     STUN_CHAR( ch, 5, STUN_TO_STUN );
     victim->position = POS_STUNNED;
    }

    act( AT_GREY, "Your lung is pierced, you can't breathe!", ch, NULL,
     victim, TO_VICT );
    update_skpell( ch, gsn_organ_donor );
   }
   else if ( number_percent( ) < 2 )
   {
    act(C_DEFAULT, "You pierce $N's heart!", ch, NULL, victim, TO_CHAR );
    act( AT_GREY, "$n punctures your heart, bye bye!", ch, NULL, victim,
     TO_VICT );
    update_skpell( ch, gsn_organ_donor );
    victim->hit = 1;
   }
  }

  if ( !IS_NPC( ch ) && ch->pcdata->learned[gsn_throttle] > 0
   && number_percent( ) > 75 )
  {
   if ( !IS_STUNNED( victim, STUN_COMMAND ) && !IS_STUNNED(ch, STUN_TO_STUN) )
   {
    act(C_DEFAULT, "You throttle $N and throw $M to the ground!",
     ch, NULL, victim, TO_CHAR );
    STUN_CHAR( ch, 20, STUN_TO_STUN);
    STUN_CHAR( victim, 10, STUN_COMMAND);
    STUN_CHAR( victim, 10, STUN_MAGIC);
    victim->position =  POS_STUNNED;
   }
  }
 }
 else if ( dt == gsn_neckstrike )
 {
  if ( number_percent( ) < 65 )
  {
   if ( !IS_STUNNED(victim, STUN_MAGIC) )
   { 
    af.type      = gsn_neckstrike;
    af.duration  = 0;
    af.location  = APPLY_HITROLL;
    af.modifier  = -5;
    af.bitvector = 0;
    affect_join( victim, &af );
    act( AT_GREY, "$N is choking!", ch, NULL, victim, TO_CHAR );
    STUN_CHAR( victim, 20, STUN_MAGIC );
   }
  }
 }
 else if ( dt == gsn_eyestrike )
 {
  if ( number_percent( ) < 65 )
  {
   af.type      = gsn_blindness;
   af.duration  = 20;
   af.location  = APPLY_HITROLL;
   af.modifier  = -50;
   af.bitvector = AFF_BLIND;
   affect_join( victim, &af );
   act( AT_GREY, "$N is blinded!", ch, NULL, victim, TO_CHAR );
  }
 }
 else if ( dt == gsn_rake )
 {
  if ( number_percent( ) < 75 )
  {
   af.type      = gsn_blindness;
   af.duration  = 5;
   af.location  = APPLY_HITROLL;
   af.modifier  = -10;
   af.bitvector = AFF_BLIND;
   affect_join( victim, &af );
  }
 }
 else if ( dt == gsn_gouge )
 {
  if ( number_percent( ) < (10 + (get_curr_dex(ch) / 3)) )
  {
   af.type      = gsn_blindness;
   af.duration  = 5;
   af.location  = APPLY_HITROLL;
   af.modifier  = -10;
   af.bitvector = AFF_BLIND;
   affect_join( victim, &af );
   act( AT_GREY, "$N is blinded!", ch, NULL, victim, TO_CHAR );
  }
 }
 else if ( dt == gsn_flykick )
 {
  act( AT_GREY, "You knock $N down!", ch, NULL, victim, TO_CHAR );
  act( AT_GREY, "$n's flying kick knocks you down, stunning you!",
   ch, NULL, victim, TO_VICT );
  act( AT_GREY, "$n's flying kick knocks $N down!",
   ch, NULL, victim, TO_ROOM );
  STUN_CHAR( victim, 5, STUN_TOTAL );
  victim->position = POS_RESTING;
 }

 return;
}

void inventory_damage( CHAR_DATA *ch, int damtype, int dam )
{
 OBJ_DATA *obj;

  if ( damtype == DAM_SLASH || damtype == DAM_PIERCE
   ||  damtype == DAM_BASH || damtype == DAM_INTERNAL
   ||  damtype == DAM_REGEN || damtype == DAM_UNHOLY
   ||  damtype == DAM_SCRATCH || damtype == DAM_HOLY )
   return;

  if ( dam < 25 )
   return;

  for ( obj = ch->carrying; obj; obj = obj->next_content )
  {
   if (!obj || obj->deleted)
    continue;

   damage_object( ch, obj, damtype, dam );
  }

 return;
}

void ranged_caster_spec( CHAR_DATA *ch, bool healer )
{
   CHAR_DATA *victim;
   char      *spell = NULL;
   char      buf[ MAX_INPUT_LENGTH ];
   char      victnamebuf[ MAX_INPUT_LENGTH ];
   int        sn;
   int        levelswitch;
   int	      dir;  
  
   if ( !(victim = ch->fighting ) )
    return;

   dir = find_first_step( ch->in_room, victim->in_room );

   if ( ch->level < 100 )
    levelswitch = 5;
   else
    levelswitch = 6;

   if ( ch->level < 75 )
    levelswitch = 4;
   if ( ch->level < 50 )
    levelswitch = 3;
   if ( ch->level < 25 )
    levelswitch = 2;
   if ( ch->level < 10 )
    levelswitch = 1;
   if ( ch->level < 5 )
    levelswitch = 0;

  if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_wizard" ) )
  {
   if ( healer )
   {
    switch( levelswitch )
    {
     case 0:
      spell = "armor"; break;

     case 1:
      spell = "shield"; break;

     case 2:
      spell = "haste"; break;

     case 3:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "armor"; break;
       case 2: spell = "shield"; break;
       case 3: spell = "haste"; break;
      }

     case 4:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "fireshield"; break;
       case 2: spell = "shield"; break;
       case 3: spell = "haste"; break;
      }

     case 5:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "fireshield"; break;
       case 2: spell = "shield"; break;
       case 3: spell = "haste"; break;
      }

     case 6:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "fireshield"; break;
       case 2: spell = "iceshield"; break;
       case 3: spell = "haste"; break;
      }
     }
    }
    else
     spell = "fire bolt";
   }
   else if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_cleric" ) )
   {
   if ( healer )
   {
    switch( levelswitch )
    {
     case 0:
      spell = "heal light"; break;

     case 1:
      spell = "heal light"; break;

     case 2:
      spell = "heal serious"; break;

     case 3:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "bless"; break;
       case 2: spell = "heal serious"; break;
       case 3: spell = "armor"; break;
      }

     case 4:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "heal serious"; break;
       case 2: spell = "armor"; break;
       case 3: spell = "bless"; break;
      }

     case 5:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "heal critical"; break;
       case 2: spell = "bless"; break;
       case 3: spell = "holy strength"; break;
      }

     case 6:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "heal critical"; break;
       case 2: spell = "sanctuary"; break;
       case 3: spell = "holy strength"; break;
      }
     }
    }
    else
     spell = "electric bolt";
   }
   else if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_druid" ) )
   {
   if ( healer )
   {
    switch( levelswitch )
    {
     case 0:
      spell = "refresh"; break;

     case 1:
      spell = "bark skin"; break;

     case 2:
      spell = "stone skin"; break;

     case 3:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "stone skin"; break;
       case 2: spell = "refresh"; break;
       case 3: spell = "bark skin"; break;
      }

     case 4:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "stone skin"; break;
       case 2: spell = "summon pack"; break;
       case 3: spell = "refresh"; break;
      }

     case 5:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "flesh armor"; break;
       case 2: spell = "summon pack"; break;
       case 3: spell = "iceshield"; break;
      }

     case 6:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "iceshield"; break;
       case 2: spell = "fauna to flora"; break;
       case 3: spell = "armor of thorns"; break;
      }
     }
    }
    else
     spell = "icy bolt";
   }
   else if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_illusionist" ) )
   {
   if ( healer )
   {
    switch( levelswitch )
    {
     case 0:
      spell = "armor"; break;

     case 1:
      spell = "shield"; break;

     case 2:
      spell = "haste"; break;

     case 3:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "armor"; break;
       case 2: spell = "shield"; break;
       case 3: spell = "haste"; break;
      }

     case 4:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "inertial barrier"; break;
       case 2: spell = "shield"; break;
       case 3: spell = "giant strength"; break;
      }

     case 5:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "blink"; break;
       case 2: spell = "magical mirror"; break;
       case 3: spell = "bladebarrier"; break;
      }

     case 6:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "blink"; break;
       case 2: spell = "magical mirror"; break;
       case 3: spell = "physical mirror"; break;
      }
     }
    }
    else
     spell = "color bolt";
   }
   else if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_necromancer" ) )
   {
   if ( healer )
   {
    switch( levelswitch )
    {
     case 0:
      spell = ""; break;

     case 1:
      spell = "stench of decay"; break;

     case 2:
      spell = "stench of decay"; break;

     case 3:
      spell = "stench of decay"; break;

     case 4:
      spell = "stench of decay"; break;

     case 5:
      spell = "stench of decay"; break;

     case 6:
      spell = "field of decay"; break;
     }
    }
    else
     spell = "acidic bolt";
   }   

  if ( !(spell) )
   return;

  if ( ( sn = skill_lookup( spell ) ) < 0 )
   return;

 if ( healer )
  (*skill_table[sn].spell_fun) ( sn, ch->level, ch, ch );
 else
  if ( dir > -1 )
  {
   sprintf( buf, "%s", victim->name );
   one_argument( buf, victnamebuf );
   sprintf( buf, "cast '%s' %s %s", spell, dir_name[dir],
    victnamebuf ); 
   interpret( ch, buf );
  }

 return;
}

/* Does the caster style commands for group fighting */
void group_spec_caster( CHAR_DATA *ch )
{
   CHAR_DATA *victim;
   char      *spell = NULL;
   char      buf[ MAX_INPUT_LENGTH ];
   char      victnamebuf[ MAX_INPUT_LENGTH ];
   int       sn;
   int       levelswitch;
  
   if ( !(victim = ch->fighting ) )
    return;

   if ( ch->level < 100 )
    levelswitch = 5;
   else
    levelswitch = 6;

   if ( ch->level < 75 )
    levelswitch = 4;
   if ( ch->level < 50 )
    levelswitch = 3;
   if ( ch->level < 25 )
    levelswitch = 2;
   if ( ch->level < 10 )
    levelswitch = 1;
   if ( ch->level < 5 )
    levelswitch = 0;

  if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_wizard" ) )
  {
    switch( levelswitch )
    {
     case 0:
      spell = "armor"; break;

     case 1:
      spell = "shield"; break;

     case 2:
      spell = "haste"; break;

     case 3:
     case 4:
     case 5:
     case 6:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "armor"; break;
       case 2: spell = "shield"; break;
       case 3: spell = "haste"; break;
      }
     }
   }
   else if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_cleric" ) )
   {
    switch( levelswitch )
    {
     case 0:
      spell = "heal light"; break;

     case 1:
      spell = "heal light"; break;

     case 2:
      spell = "heal serious"; break;

     case 3:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "bless"; break;
       case 2: spell = "heal serious"; break;
       case 3: spell = "armor"; break;
      }

     case 4:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "heal serious"; break;
       case 2: spell = "armor"; break;
       case 3: spell = "bless"; break;
      }

     case 5:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "heal critical"; break;
       case 2: spell = "bless"; break;
       case 3: spell = "holy strength"; break;
      }

     case 6:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "heal critical"; break;
       case 2: spell = "sanctuary"; break;
       case 3: spell = "holy strength"; break;
      }
     }
   }
   else if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_druid" ) )
   {
    switch( levelswitch )
    {
     case 0:
      spell = "refresh"; break;

     case 1:
      spell = "bark skin"; break;

     case 2:
      spell = "stone skin"; break;

     case 3:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "stone skin"; break;
       case 2: spell = "refresh"; break;
       case 3: spell = "bark skin"; break;
      }

     case 4:
     case 5:
     case 6:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "stone skin"; break;
       case 2: spell = "summon pack"; break;
       case 3: spell = "refresh"; break;
      }
     }
   }
   else if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_illusionist" ) )
   {
    switch( levelswitch )
    {
     case 0:
      spell = "armor"; break;

     case 1:
      spell = "shield"; break;

     case 2:
      spell = "haste"; break;

     case 3:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "armor"; break;
       case 2: spell = "shield"; break;
       case 3: spell = "haste"; break;
      }

     case 4:
     case 5:
     case 6:
      switch( dice( 1, 7 ) )
      {
       default: break;
       case 1: spell = "inertial barrier"; break;
       case 2: spell = "shield"; break;
       case 3: spell = "giant strength"; break;
      }
     }
   }
   else if ( ch->spec_fun == spec_lookup( "spec_fighting_caster_necromancer" ) )
   {
    sprintf( buf, "%s", victim->name );
    one_argument( buf, victnamebuf );
    sprintf( buf, "cast 'poison' %s", victnamebuf ); 
    interpret( ch, buf );
    return;
   }

  if ( !(spell) )
   return;

  if ( ( sn = skill_lookup( spell ) ) < 0 )
   return;

  if ( !(victim = group_ai_find_random( ch ) ) )
   return;

   sprintf( buf, "%s", victim->name );
   one_argument( buf, victnamebuf );
   sprintf( buf, "cast '%s' %s", spell, victnamebuf ); 
   interpret( ch, buf );

 return;
}

/* If mobs are grouped, they fight differently.
 * Sure I could have just included the group
 * fighting code in the mobile_fight_engine, but
 * this keeps it easier to read, that mess was too
 * jumbled anyways. -Flux
 * Is returned false only to give AI control back
 * to the fight_engine function.
 */
bool group_tactics( CHAR_DATA *ch )
{
 CHAR_DATA *victim;
 CHAR_DATA *randomchar;
 OBJ_DATA  *getobj;
 char      buf[ MAX_INPUT_LENGTH ];
 char      victnamebuf[ MAX_INPUT_LENGTH ];
 char      namebuf[ MAX_INPUT_LENGTH ];
 int       leveldiff;
 bool	   istank = FALSE;

 if ( !(victim = ch->fighting) )
 {
  stop_fighting( ch, FALSE );
  return FALSE;
 }

 if ( victim->fighting == ch )
  istank = TRUE;

 if ( victim->in_room != ch->in_room )
  return FALSE;

 leveldiff = ch->level - victim->level;

 switch( ch->fighting_style )
 {
/*  CASE_FIGHTING_STYLE_NORMAL: */
  default:
    if ( number_percent( )
     < ( leveldiff < -5 ? ch->level / 4 : UMAX( 10, leveldiff ) )
     && number_bits(4) == 0
     && ((get_eq_char( victim, WEAR_WIELD ) != NULL)
     ||(get_eq_char( victim, WEAR_WIELD_2 ) != NULL)) )
      disarm( ch, victim );
  break;

  case FIGHTING_STYLE_WIMPY:
    if ( istank )
    {
     if ( !(randomchar = group_ai_find_fighter(ch)) )
      randomchar = group_ai_find_random(ch);

     sprintf( buf, "%s", randomchar->name );
     one_argument( buf, victnamebuf );
     sprintf( buf, "%s", ch->name );
     one_argument( buf, namebuf );
     sprintf( buf, "mpforce %s rescue %s", victnamebuf, namebuf ); 
     interpret( ch, buf );
    }
    else
     switch( dice( 1, 7 ) )
     {
      default: break;
      case 1:
       do_trip( ch, "" ); break;
      case 2:
       do_gouge( ch, "" ); break;
     }
  break;

  case FIGHTING_STYLE_BEAST_WIMPY:
    if ( istank )
    {
     if ( !(randomchar = group_ai_find_fighter(ch)) )
      randomchar = group_ai_find_random(ch);

     sprintf( buf, "%s", randomchar->name );
     one_argument( buf, victnamebuf );
     sprintf( buf, "%s", ch->name );
     one_argument( buf, namebuf );
     sprintf( buf, "mpforce %s rescue %s", victnamebuf, namebuf ); 
     interpret( ch, buf );
    }
    else
     switch( dice( 1, 7 ) )
     {
      default: break;
      case 1:
       do_rake( ch, "" ); break;
      case 2:
       do_bite( ch, "" ); break;
     }
  break;

 case FIGHTING_STYLE_BEAST_AGGRESSIVE:
  if ( !istank )
  {
   randomchar = victim->fighting;

   if ( fight_style_type( randomchar ) != 1 )
   {
    sprintf( buf, "%s", randomchar->name );
    one_argument( buf, victnamebuf );
    sprintf( buf, "rescue %s", victnamebuf ); 
    interpret( ch, buf );
   }
   else
    switch( dice( 1, 4 ) )
    {
     default: break;
     case 1:
      do_rake( ch, "" ); break;
     case 2:
      do_bite( ch, "" ); break;
    }
   }
   else
    switch( dice( 1, 4 ) )
    {
     default: break;
     case 1:
      do_rake( ch, "" ); break;
     case 2:
      do_bite( ch, "" ); break;
    }
  break;

  case FIGHTING_STYLE_BARBARIAN:
  if ( !istank )
  {
   randomchar = victim->fighting;

   if ( fight_style_type( randomchar ) != 1 )
   {
    sprintf( buf, "%s", randomchar->name );
    one_argument( buf, victnamebuf );
    sprintf( buf, "rescue %s", victnamebuf ); 
    interpret( ch, buf );
   }
   else
   {
    if ( ch->level < 100 )
    {
     switch( dice( 1, 9 ) )
     {
      default: break;
      case 1:
       do_rake( ch, "" ); break;
      case 2:
       do_kick( ch, "" ); break;
      case 3:
       do_bite( ch, "" ); break;
      case 6:
       do_punch( ch, "" ); break;
      case 7:
       do_kick( ch, "" ); break;
     }
    }
    else
     switch( dice( 1, 9 ) )
     {
      default: break;
      case 1:
       do_grapple( ch, "" ); break;
      case 2:
       do_grapple( ch, "" ); break;
      case 3:
       do_grapple( ch, "" ); break;
      case 4:
       do_slam( ch, "" ); break;
      case 5:
       do_slam( ch, "" ); break;
      case 6:
       do_grapple( ch, "" ); break;
      case 7:
       do_kick( ch, "" ); break;
     }
    }
   }
   else
   {
    if ( ch->level < 100 )
    {
     switch( dice( 1, 9 ) )
     {
      default: break;
      case 1:
       do_rake( ch, "" ); break;
      case 2:
       do_kick( ch, "" ); break;
      case 3:
       do_bite( ch, "" ); break;
      case 6:
       do_punch( ch, "" ); break;
      case 7:
       do_kick( ch, "" ); break;
     }
    }
    else
     switch( dice( 1, 9 ) )
     {
      default: break;
      case 1:
       do_grapple( ch, "" ); break;
      case 2:
       do_grapple( ch, "" ); break;
      case 3:
       do_grapple( ch, "" ); break;
      case 4:
       do_slam( ch, "" ); break;
      case 5:
       do_slam( ch, "" ); break;
      case 6:
       do_grapple( ch, "" ); break;
      case 7:
       do_kick( ch, "" ); break;
     }
    }
   break;

 case FIGHTING_STYLE_MARTIAL:
  if ( !istank )
  {
   randomchar = victim->fighting;

   if ( fight_style_type( randomchar ) != 1 )
   {
    sprintf( buf, "%s", randomchar->name );
    one_argument( buf, victnamebuf );
    sprintf( buf, "rescue %s", victnamebuf ); 
    interpret( ch, buf );
   }
   else
   {
     if ( ch->level > 75 )
     {
      switch( dice( 1, 11 ) )
      {
       default: break;
       case 1:
        do_trip( ch, "" ); break;
       case 2:
        do_grapple( ch, "" ); break;
       case 3:
        do_punch( ch, "" ); break;
       case 4:
        do_uppercut_punch( ch, "" ); break;
       case 5:
        do_kidney_punch( ch, "" ); break;
       case 6:
        do_stun( ch, "" ); break;
       case 7:
        do_eyestrike( ch, "" ); break;
       case 8:
        do_anklestrike( ch, "" ); break;
       case 9:
        do_facestrike( ch, "" ); break;
      }
     }
     else
      switch( dice( 1, 11 ) )
      {
       default: break;
       case 1:
        do_trip( ch, "" ); break;
       case 2:
        do_kick( ch, "" ); break;
       case 3:
        do_punch( ch, "" ); break;
       case 4:
        do_uppercut_punch( ch, "" ); break;
       case 5:
        do_kidney_punch( ch, "" ); break;
       case 9:
        do_facestrike( ch, "" ); break;
      }
     }
    }
    else
    {
     if ( ch->level > 75 )
     {
      switch( dice( 1, 11 ) )
      {
       default: break;
       case 1:
        do_trip( ch, "" ); break;
       case 2:
        do_grapple( ch, "" ); break;
       case 3:
        do_punch( ch, "" ); break;
       case 4:
        do_uppercut_punch( ch, "" ); break;
       case 5:
        do_kidney_punch( ch, "" ); break;
       case 6:
        do_stun( ch, "" ); break;
       case 7:
        do_eyestrike( ch, "" ); break;
       case 8:
        do_anklestrike( ch, "" ); break;
       case 9:
        do_facestrike( ch, "" ); break;
      }
     }
     else
      switch( dice( 1, 11 ) )
      {
       default: break;
       case 1:
        do_grapple( ch, "" ); break;
       case 2:
        do_kick( ch, "" ); break;
       case 3:
        do_punch( ch, "" ); break;
       case 4:
        do_uppercut_punch( ch, "" ); break;
       case 5:
        do_kidney_punch( ch, "" ); break;
       case 9:
        do_facestrike( ch, "" ); break;
      }
    }
  break;

 case FIGHTING_STYLE_SNEAK:
    if ( istank )
    {
     switch( dice( 1, 9 ) )
     {
      default: break;
      case 1:
       do_trip( ch, "" ); break;
      case 2:
       do_kick( ch, "" ); break;
      case 3:
       do_punch( ch, "" ); break;
      case 4:
       do_gouge( ch, "" ); break;
      case 5:
       do_kidney_punch( ch, "" ); break;
     }
    }
    else
    {
     if ( !(getobj = get_eq_char( ch, WEAR_WIELD )) )
     {
      switch( dice( 1, 9 ) )
      {
       default: break;
       case 1:
        do_trip( ch, "" ); break;
       case 2:
        do_kick( ch, "" ); break;
       case 3:
        do_punch( ch, "" ); break;
       case 4:
        do_gouge( ch, "" ); break;
       case 5:
        do_kidney_punch( ch, "" ); break;
      }
     }
      else
       if ( (getobj->item_type == ITEM_WEAPON
        && getobj->value[8] != WEAPON_PIERCE)
        || getobj->item_type != ITEM_WEAPON )
       {
        switch( dice( 1, 9 ) )
        {
         default: break;
         case 1:
          do_trip( ch, "" ); break;
         case 2:
          do_kick( ch, "" ); break;
         case 3:
          do_punch( ch, "" ); break;
         case 4:
          do_gouge( ch, "" ); break;
         case 5:
          do_kidney_punch( ch, "" ); break;
        }
       }
       else
        switch( dice( 1, 9 ) )
        {
         default: break;
         case 1:
          do_circle( ch, "" ); break;
         case 2:
          do_circle( ch, "" ); break;
         case 3:
          do_circle( ch, "" ); break;
         case 4:
          do_gouge( ch, "" ); break;
         case 5:
          do_kidney_punch( ch, "" ); break;
        }
      }
  break;

 case FIGHTING_STYLE_WARLOCK:
  if ( istank )
  {
   if ( ch->spec_fun != 0 )
    if ( ( *ch->spec_fun ) ( ch ) )
     ;
  }
  else
   group_spec_caster( ch );
  break;

 case FIGHTING_STYLE_CLERIC:
  if ( istank )
  {
   if ( ch->spec_fun != 0 )
    if ( ( *ch->spec_fun ) ( ch ) )
     ;
  }
  else
   group_spec_caster( ch );
  break;

 case FIGHTING_STYLE_STREET:
  if ( !istank )
  {
   randomchar = victim->fighting;

   if ( fight_style_type( randomchar ) != 1 )
   {
    sprintf( buf, "%s", randomchar->name );
    one_argument( buf, victnamebuf );
    sprintf( buf, "rescue %s", victnamebuf ); 
    interpret( ch, buf );
   }
  }
  else 
   switch( dice( 1, 11 ) )
   {
    default: break;
    case 1:
     do_trip( ch, "" ); break;
    case 2:
     do_kick( ch, "" ); break;
    case 3:
     do_punch( ch, "" ); break;
    case 4:
     do_uppercut_punch( ch, "" ); break;
    case 5:
     do_kidney_punch( ch, "" ); break;
   }
   break;
  }

 return TRUE;
}

/* Finds a random group member of ch that is a physical style */
CHAR_DATA *group_ai_find_fighter( CHAR_DATA *ch )
{
 CHAR_DATA *member;
 bool      ingroup = FALSE;

  for ( member = ch->in_room->people; member; member = member->next_in_room )
  {
   if ( !member || member->deleted )
    continue;

   if ( ch == member )
    continue;

   if ( is_same_group( member, ch ) )
    if ( fight_style_type( ch ) == 1 )
     ingroup = TRUE;
  }
 if ( ingroup )
  return member;
 else
  return NULL;
}

/* Finds a random group member of ch */
CHAR_DATA *group_ai_find_random( CHAR_DATA *ch )
{
 CHAR_DATA *member;
 bool      ingroup = FALSE;

  for ( member = ch->in_room->people; member; member = member->next_in_room )
  {
   if ( !member || member->deleted )
    continue;

   if ( ch == member )
    continue;

   if ( is_same_group( member, ch ) )
    ingroup = TRUE;
  }
 if ( ingroup )
  return member;
 else
  return NULL;
}

/* Returns either 1 for a primary fight style or 2 for secondary
 * Primary means it likes to be the tank, secondary means
 * support style, such as magic user or thief.
 * works for both PC's and NPC's -Flux 
 */
int fight_style_type( CHAR_DATA *ch )
{
 int style = 2;

 if ( IS_NPC( ch ) )
 {
  if ( ch->fighting_style == FIGHTING_STYLE_NORMAL )
   style = 1;
  if ( ch->fighting_style == FIGHTING_STYLE_MARTIAL )
   style = 1;
  if ( ch->fighting_style == FIGHTING_STYLE_CLERIC )
   style = 2;
  if ( ch->fighting_style == FIGHTING_STYLE_WARLOCK )
   style = 2;
  if ( ch->fighting_style == FIGHTING_STYLE_SNEAK )
   style = 2;
  if ( ch->fighting_style == FIGHTING_STYLE_STREET )
   style = 1;
  if ( ch->fighting_style == FIGHTING_STYLE_WIMPY )
   style = 2;
  if ( ch->fighting_style == FIGHTING_STYLE_BEAST_WIMPY )
   style = 2;
  if ( ch->fighting_style == FIGHTING_STYLE_BEAST_AGGRESSIVE )
   style = 1;
  if ( ch->fighting_style == FIGHTING_STYLE_BARBARIAN )
   style = 1;
 }
 else if ( class_table[prime_class(ch)].fightrank == TRUE )
  style = 1;
 else
  style = 2;

 return style;
}
