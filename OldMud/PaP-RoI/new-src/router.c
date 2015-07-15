/*
 * IMC2 - an inter-mud communications protocol
 *
 * router.c: a simple router/hub for IMC2
 *
 * Copyright (C) 1996,1997 Oliver Jowett <oliver@jowett.manawatu.planet.co.nz>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program (see the file COPYING); if not, write to the
 * Free Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 */

/* This is the code for a standalone hub for IMC2. You do NOT need to
 * link this if you're running a normal mud using IMC!
 */

#include <stdlib.h> 
#include <stdio.h>
#include <sys/time.h>
#include <sys/types.h>
#include <unistd.h>
#include <string.h>
#include <signal.h>

#include "imc.h"

#define USE_ICED

#ifdef USE_ICED
#include "iced.h"
#endif

/* commands:
 *
 * password connect name
 * password disconnect name
 * password reboot
 * password sockets
 * password list
 * password (command as in imc_command)
 *
 */

char *password;
int logging;

#ifndef CONFIG
#define CONFIG "hub/"
#endif

#ifndef EMAIL
#define EMAIL "oliver@jowett.manawatu.planet.co.nz"
#endif

#define onearg(x,y) imc_getarg(x,y,IMC_DATA_LENGTH)

int rebooting;
int reloading;

void *imc_malloc(int size)
{
  return malloc(size);
}

void imc_free(void *block, int size)
{
  free(block);
}

char *imc_strdup(const char *orig)
{
  return strdup(orig);
}

void imc_strfree(char *str)
{
  free(str);
}

int imc_readconfighook(const char *type, const char *value)
{
  if (!strcasecmp(type, "password"))
  {
    if (password)
      imc_strfree(password);
    password=imc_strdup(value);
    return 1;
  }
  else if (!strcasecmp(type, "logging"))
  {
    logging=atoi(value);
    return 1;
  }

  return 0;
}

void imc_saveconfighook(FILE *fp)
{
  if (password)
    fprintf(fp, "Password %s\n", password);
  fprintf(fp, "Logging %d\n", logging);
}

void ev_reboot(void *data)
{
  rebooting=1;
}

void ev_reload(void *data)
{
  reloading=1;
}

const char *docommand(const char *name, const char *arg)
{
  char buf[IMC_DATA_LENGTH];
  const char *orig;
  int r;

  orig=arg;
  arg=onearg(arg,buf);

  if (!password || strcmp(buf, password))
  {
    imc_logstring("bad password on command from %s: %s", name, orig);
    return NULL;
  }

  imc_logstring("command from %s: %s", name, arg);

  orig=arg;
  arg=onearg(arg,buf);

  if (!strcasecmp(buf, "reboot"))
  {
    imc_add_event(10, ev_reboot, NULL, 1);
    if (logging)
      imc_send_chat(NULL, 2, "Rebooting in 10 seconds", "*");
    return "'k";
  }

  if (!strcasecmp(buf, "reload"))
  {
    imc_add_event(10, ev_reload, NULL, 1);
    if (logging)
      imc_send_chat(NULL, 2, "Reloading config in 10 seconds", "*");
    return "'k";
  }
  
  if (!strcasecmp(buf, "logging"))
  {
    logging=!logging;
    return logging ? "Logging now enabled" : "Logging now disabled";
  }

  if (!strcasecmp(buf, "connect"))
  {
    arg=onearg(arg,buf);
    if (imc_connect_to(buf))
      return "'k";
    else
      return imc_error();
  }

  if (!strcasecmp(buf, "disconnect"))
  {
    arg=onearg(arg,buf);
    if (imc_disconnect(buf))
      return "'k";
    else
      return imc_error();
  }

  if (!strcasecmp(buf, "sockets"))
    return imc_sockets();

  if (!strcasecmp(buf, "list"))
    return imc_list(2);

  if (!strcasecmp(buf, "config"))
    return imc_list(5);

#ifdef USE_ICED
  if (!strcasecmp(buf, "iced"))
  {
    char chan[IMC_DATA_LENGTH], cmd[IMC_DATA_LENGTH];
    const char *argument;

    argument=imc_getarg(buf, cmd, IMC_DATA_LENGTH);
    argument=imc_getarg(argument, chan, IMC_DATA_LENGTH);

    iced_recv_command(name, chan, cmd, argument, 1);
    return "Command executed.";
  }
#endif
  
  if ((r=imc_command(orig))>0)
    return "'k";

  if (!r)
    return "Unknown command";
  else
    return imc_error();
}

char *getstats(void)
{
  char *buf=imc_getsbuf(IMC_DATA_LENGTH);
  char buf1[IMC_DATA_LENGTH];
  int found=0;
  imc_connect *c;
  imc_reminfo *i;

  sprintf(buf, "Connections to %s (using %s):\n\r", imc_name, IMC_VERSIONID);

  for (c=imc_connect_list; c; c=c->next)
  {
    if (c->state==IMC_CONNECTED)
    {
      found++;
      i=imc_find_reminfo(c->info->name, 0);
      if (i && i->ping)
	sprintf(buf1, " [%4dms] %s@%s\n\r",
		i->ping,
		c->info->name,
		c->info->host);
      else
	sprintf(buf1, " [????ms] %s@%s\n\r",
		c->info->name,
		c->info->host);
      strcat(buf, buf1);
    }
  }

  sprintf(buf1, "%d direct connections total.\n\r", found);
  strcat(buf, buf1);

  imc_shrinksbuf(buf);
  return buf;
}

void imc_debug(const imc_connect *c, int out, const char *string)
{
#if 0
  char *dir;

  dir=out ? "<" : ">";

  printf("%s %s %s\n", imc_getconnectname(c), dir, string);
#endif
}

void imc_log(const char *string)
{
  char buf[IMC_DATA_LENGTH];
  char *msg;
  int log=0;

  strcpy(buf, ctime(&imc_now));
  buf[strlen(buf)-1]=' ';
  fprintf(stderr, "%s%s\n", buf, string);

  if (!strncmp(string, "connect to", 10))
    log=1;
  else
  {
    msg=strchr(string, ':');
    if (msg)
    {
      msg++;
      if (!strncmp(string, "connect to", 10) ||
          !strcmp(msg, " closing link") ||
          !strncmp(msg, " password failure", 17) ||
          !strncmp(msg, " connected ", 11))
       log=1;
    }
  }
 
  if (log && logging)
  {
    sprintf(buf, "%s", string);
    imc_send_chat(NULL, 2, buf, "*"); /* log to rinfo */
  }
}

void imc_recv_who(const imc_char_data *from, const char *type)
{
  char arg[IMC_DATA_LENGTH];

  type=onearg(type, arg);

  if (!strcasecmp(arg, "who"))
    imc_send_whoreply(from->name, getstats(), -1);
  else if (!strcasecmp(arg, "info"))
    imc_send_whoreply(from->name,
		      "This is a stand-alone IMC router based on router.c.\n\r"
		      "It is administered by " EMAIL "\n\r", -1);
  else if (!strcasecmp(arg, "direct"))
    imc_send_whoreply(from->name, imc_list(0), -1);
  else if (!strcasecmp(arg, "list"))
    imc_send_whoreply(from->name, imc_list(3), -1);
  else if (!strcasecmp(arg, "config"))
    imc_send_whoreply(from->name, imc_list(4), -1);
  else if (!strcasecmp(arg, "istats"))
    imc_send_whoreply(from->name, imc_getstats(), -1);
  else if (!strcasecmp(arg, "help") || !strcasecmp(arg, "services") ||
	   !strcasecmp(arg, "help"))
    imc_send_whoreply(from->name,
		      "Available rquery types:\n\r"
		      "help       - this list\n\r"
		      "who        - active muds on IMC\n\r"
		      "info       - router information\n\r"
		      "list       - list known muds on IMC\n\r"
		      "direct     - list directly connected muds\n\r"
		      "config     - show local configuration\n\r"
		      "istats     - IMC statistics\n\r", -1);
  else
    imc_send_whoreply(from->name,
		      "Sorry, no information of that type is available", -1);
}

void imc_recv_tell(const imc_char_data *from, const char *to, const char *text,
                   int isreply)
{
  const char *response;

  if (strcmp(to, "*"))
    return;

  response=docommand(from->name, text);
  if (response)
    imc_send_tell(NULL, from->name, response, 1);
}

void imc_recv_chat(const imc_char_data *from, int channel, const char *text)
{
}

void imc_recv_emote(const imc_char_data *from, int channel, const char *text)
{
}

void imc_recv_whois(const imc_char_data *from, const char *to)
{
}

void imc_recv_whoisreply(const char *to, const char *text)
{
}

void imc_recv_whoreply(const char *to, const char *text, int sequence)
{
}

void imc_recv_beep(const imc_char_data *from, const char *to)
{
}

void imc_traceroute(int ping, const char *pathto, const char *pathfrom)
{
}

char *imc_mail_arrived(const char *from, const char *to, const char *date,
		       const char *subject, const char *text)
{
  char *buf=imc_getsbuf(200);
  sprintf(buf, "%s is a router only, and does not accept mail.", imc_name);
  imc_shrinksbuf(buf);
  return buf;
}

void main(void)
{
  signal(SIGPIPE, SIG_IGN);

  imc_is_router=1;

  imc_startup(CONFIG);
  if ((imc_active < IA_UP) || (imc_lock_file<0))
  {
    imc_logstring("router: giving up");
    exit(1);
  }

#ifdef USE_ICED
  iced_init();
#endif
  
  while (!rebooting)
  {
    imc_idle(imc_get_max_timeout(), 0);
    if (reloading)
    {
      imc_shutdown();
      imc_startup(CONFIG);
      reloading=0;
    }
  }
    
  imc_shutdown();

  exit(0);
}
