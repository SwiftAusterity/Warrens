void death_xp_loss( CHAR_DATA *victim )
{
  int xp_lastlvl;
  int xp_loss;
  if ( victim->level < LEVEL_HERO )
    {
    xp_lastlvl = number_classes( victim ) == 1 ? 1000
	       : number_classes( victim ) * 2000; 
    if ( victim->exp > xp_lastlvl )
      gain_exp( victim, ( xp_lastlvl - victim->exp ) / 2 );
    }
  else if ( victim->level < L_CHAMP3 )
    {
    if ( victim->level >= LEVEL_HERO )
	xp_lastlvl = number_classes( victim ) == 1 ? 100000
		   : number_classes( victim ) * 200000;
    if ( victim->level >= L_CHAMP1 )
	xp_lastlvl = xp_lastlvl + ( 3 * xp_lastlvl );
    if ( victim->level >= L_CHAMP2 )
	xp_lastlvl = xp_lastlvl * 2 + xp_lastlvl / 2;
    if ( victim->exp > xp_lastlvl )
      {
      xp_loss = (xp_lastlvl - victim->exp ) / 2;
      xp_loss = UMAX( -10000 * number_classes( victim ), xp_loss );
      gain_exp( victim, xp_loss );
      }
    }
  return;
}
