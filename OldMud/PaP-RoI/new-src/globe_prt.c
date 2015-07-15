  if ( !str_cmp( argument, "locate" ) )
	{
	for ( pd = ch->powered; pd; pd = pd->next )
	  {
	  if ( pd->type == gsn_globedark )
		{
		sprintf( buf, "Globe of Darkness&w, &W%s&w; &cCost&w: &R%d&w.\n\r", 
		pd->room->name, pd->cost );
		send_to_char( AT_DGREY, buf, ch );
		found = TRUE;
		}
	  }
	if ( !found )
	  send_to_char( AT_CYAN, "You are not sustaining any &zGlobes&w.\n\r", ch );
	return;
	}
  if ( !str_cmp( argument, "dissipate" ) )
	{
	if ( !is_raffected( ch->in_room, gsn_globedark ) )
	  {
	  send_to_char( AT_CYAN, "There is no &zGlobe &cin this room&w.\n\r", ch );
	  return;
	  }
        for ( pd = ch->powered; pd; pd = pd->next )
	  {
	  if ( !pd )
	    break;
	  if ( pd->type == gsn_globedark )
	    {
	    found = TRUE;
	    if ( pd->room == ch->in_room )
	      {
	      send_to_char( AT_DGREY, "You wave your hand and the globe dissipates.\n\r", ch );
	      act( AT_DGREY, "The globe of darkness dissipates.", 
		   ch, NULL, NULL, TO_ROOM );
	      raffect_remove( ch->in_room, ch, pd->raf );
	      return;
	      }
	    }
	  }
	if ( !found )
	  {
	  send_to_char( AT_CYAN, "You are not sustaining any &zGlobes&w.\n\r", ch );
	  return;
	  }
	send_to_char( AT_CYAN, "You are not powering the &zGlobe&c in this room.\n\r", ch ); 
	return;
