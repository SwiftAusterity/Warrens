/***********************************
 * Do NOT delete this file!        *
 * -- Hannibal			   *
 ***********************************/

bool can_use_skpell( CHAR_DATA *ch, int sn )
{
  int iClass = 0;

  if ( IS_NPC( ch ) )
      return TRUE;
      
  for ( iClass = 0; ch->class[iClass] != -1; iClass++ )
    {
    if ( ch->level >= skill_table[sn].skill_level[ch->class[iClass]] ||
         ( number_classes( ch ) == 2 && 
	   prime_class( ch ) == ch->class[iClass] &&
           skill_table[sn].skill_level[ch->class[iClass]] < L_APP ) )
      return TRUE;
    }

  /* dualclass coding -Flux */

   if ( number_classes( ch ) == 2 )
   {
         if ( ( skill_table[sn].dualclass_one != ch->class[0]
                && skill_table[sn].dualclass_one != ch->class[1] )
             || ( skill_table[sn].dualclass_two !=  ch->class[0]
                && skill_table[sn].dualclass_two != ch->class[1] )
             || skill_table[sn].dualclass_level > ch->level )
          return FALSE;
   }
   else
    return FALSE;

  return TRUE;
}

bool is_class( CHAR_DATA *ch, int class )
{
  int iClass; 
  if ( IS_NPC( ch ) )
    return FALSE;

/*  if( ch->class[0] == class )
     return TRUE;  */

  for ( iClass = 0; ch->class[iClass] != -1; iClass++ )
    {
    if ( ch->class[iClass] == class )
	return TRUE;
    }

  return FALSE;
}
int prime_class( CHAR_DATA *ch )
{
  return ch->class[0];
}

int bestskillclass( CHAR_DATA *ch, int sn)
{
  int tempclass = ch->class[0];
  int iClass;

  for (iClass = 0; ch->class[iClass] != -1; iClass++)
  {
    if(skill_table[sn].skill_level[(ch->class[iClass])] < skill_table[sn].skill_level[tempclass] )
          tempclass = ch->class[iClass];
  }
  
  return tempclass;

}  

int number_classes( CHAR_DATA *ch )
{
  int iClass;
  if ( IS_NPC( ch ) )
     return 0;
   for ( iClass = 0; ch->class[iClass] != -1; iClass++ )
    ; 
  return iClass;
}
char *class_long( CHAR_DATA *ch )
{
  static char buf [ 512 ];
  int iClass;
  buf[0] = '\0';
  if ( IS_NPC( ch ) )
    return "Mobile";
  for ( iClass = 0; ch->class[iClass] != -1 ; iClass++ )
	{
	strcat( buf, "/" );
	strcat( buf, class_table[ch->class[iClass]].who_long );
	}
  return buf+1;
}
char *class_short( CHAR_DATA *ch )
{
  static char buf [ 512 ];
  int iClass;
  buf[0] = '\0';
  if ( IS_NPC( ch ) )
    return "Mob";
  for ( iClass = 0; ch->class[iClass] != -1 ; iClass++ )
	{
	strcat( buf, "/" );
	strcat( buf, class_table[ch->class[iClass]].who_name );
	}
  return buf+1;
}
char *class_numbers( CHAR_DATA *ch, bool pSave )
{
  static char buf[ 512 ];
  char buf2[ 10 ];
  int iClass;
  buf[0] = '\0';
  if ( IS_NPC( ch ) )
    return "0";
  for ( iClass = 0; ch->class[ iClass ] != -1; iClass++ )
	{
	strcat( buf, " " );
	sprintf( buf2, "%d", ch->class[iClass] );
	strcat( buf, buf2 );
	}
  if ( pSave )
    strcat( buf, " -1" );
  return buf+1;
}
