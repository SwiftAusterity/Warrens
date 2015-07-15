{

if ( !str_comp( argument1, "cd" ))
{
if ( argument2[0] == '\0' )
return

ch->pcdata->shellfile = argument2
return;
}

if ( !str_cmp( argument1, "ls" ))
{
list_shell_contents_to_char()
return;
}

if ( !str_cmp( argument, "mkdir" ))
{

return;
}

send_to_char( AT_GREEN, "Syntax: shell <shell command> <directory>.\n\r",ch );
send_to_char( AT_GREEN, "Shell command being, 'ls', 'cd', or 'mkdir'.\n\r",ch );

return;
}

argument = one_argument( argument, arg1 );

