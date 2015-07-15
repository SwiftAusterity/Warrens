void death_xp_loss( CHAR_DATA *victim )
{
 int xp_loss;

 xp_loss = victim->exp / 2;
 gain_exp( victim, xp_loss );

  return;
}
