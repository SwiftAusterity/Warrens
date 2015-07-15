/*
 * IMC2 - an inter-mud communications protocol
 *
 * webserver.c: the server for a who-list-on-web-page thing
 *
 * Copyright (C) 1997 Oliver Jowett <oliver@jowett.manawatu.planet.co.nz>
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

/* Consider this alpha code. 'nuff said */

#include <stdlib.h> 
#include <stdio.h>
#include <sys/time.h>
#include <sys/types.h>
#include <unistd.h>
#include <string.h>
#include <signal.h>
#include <sys/socket.h>
#include <fcntl.h>
#include <sys/un.h>
#include <ctype.h>
#include <sys/stat.h>
#include <errno.h>

#include "imc.h"

#ifndef CONFIG
#define CONFIG "cgi/"
#endif

int requests;

char *url;

/* MM stubs */

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

int imc_readconfighook(const char *word, const char *value)
{
  if (!strcasecmp(word, "URL"))
  {
    if (url)
      imc_strfree(url);
    url=imc_strdup(value);
    return 1;
  }

  return 0;
}

void imc_saveconfighook(FILE *fp)
{
  if (url)
    fprintf(fp, "URL %s\n", url);
}

/* logging functions */

void imc_debug(const imc_connect *c, int out, const char *string)
{
#if 0
  /* this is rarely useful */

  char *dir;

  dir=out ? "<" : ">";

  fprintf(stdout, "%s %s %s\n", imc_getconnectname(c), dir, string);
#endif
}

void imc_log(const char *string)
{
  char buf[IMC_DATA_LENGTH];

  strcpy(buf, ctime(&imc_now));
  buf[strlen(buf)-1]=' ';
  fprintf(stderr, "%s%s\n", buf, string);
}

void imc_recv_who(const imc_char_data *from, const char *type)
{
  char arg[IMC_DATA_LENGTH];
  char buf[IMC_DATA_LENGTH];
  
  type=imc_getarg(type, arg, IMC_DATA_LENGTH);

  if (!strcasecmp(arg, "who") || !strcasecmp(arg, "info"))
  {
    if (!url)
      imc_send_whoreply(from->name, "This is a CGI interface site.\n\r", -1);
    else
    {
      sprintf(buf,
	      "This is a CGI interface site.\n\r"
	      "Its output can be accessed at %s.\n\r", url);
      imc_send_whoreply(from->name, buf, -1);
    }
  }
  else if (!strcasecmp(arg, "direct"))
    imc_send_whoreply(from->name, imc_list(0), -1);
  else if (!strcasecmp(arg, "list"))
    imc_send_whoreply(from->name, imc_list(3), -1);
  else if (!strcasecmp(arg, "config"))
    imc_send_whoreply(from->name, imc_list(4), -1);
  else if (!strcasecmp(arg, "istats"))
  {
    strcpy(buf, imc_getstats());
    sprintf(buf+strlen(buf),
	    "\n\rProcessed requests: %d\n\r", requests);
    imc_send_whoreply(from->name, buf, -1);
  }
  else if (!strcasecmp(arg, "help") || !strcasecmp(arg, "services") ||
	   !strcasecmp(arg, "help"))
    imc_send_whoreply(from->name,
		      "Available rquery types:\n\r"
		      "help       - this list\n\r"
		      "info       - server information\n\r"
		      "list       - active IMC connections\n\r"
		      "istats     - IMC statistics\n\r", -1);
 
  else
    imc_send_whoreply(from->name,
		      "Sorry, no information of that type is available", -1);
}

void imc_recv_whois(const imc_char_data *from, const char *to)
{
}

void imc_recv_whoisreply(const char *to, const char *text)
{
}

void imc_recv_tell(const imc_char_data *from, const char *to, const char *text,
                   int isreply)
{
}

void imc_recv_chat(const imc_char_data *from, int channel, const char *text)
{
}

void imc_recv_emote(const imc_char_data *from, int channel, const char *text)
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
  return "This is a webserver interface only, and does not accept mail.";
}



/* Core of the code! */

/* now handles rwho sequencing for those muds that support it */

typedef struct _replydata {
  char *text;
  int sequence;
  struct _replydata *next;
} replydata;

typedef struct {
  int fd;
  int id;
  int length;
  int received;
  replydata *reply;
} requestdata;

requestdata clients[20];

int id;
int freeclients;

void sendlist(int fd)
{
  char buf[10000];
  imc_reminfo *r;

  buf[0]=0;
  for (r=imc_reminfo_list; r; r=r->next)
  {
    sprintf(buf+strlen(buf),
            "%s %s %d\n", r->name, r->version, r->ping);
  }

  write(fd, buf, strlen(buf));
}

void complete_request(int index)
{
  replydata *r, *next;
  int expected=0;
  
  for (r=clients[index].reply; r; r=next)
  {
    next=r->next;
    
    if (r->sequence != expected)
    {
      char buf[100];
      while (expected<r->sequence)
      {
	sprintf(buf, "\n\r[missing data for sequence %d]\n\r", expected);
	write(clients[index].fd, buf, strlen(buf));
	expected++;
      }
    }
    
    write(clients[index].fd, r->text, strlen(r->text));
    expected++;

    imc_strfree(r->text);
    imc_free(r, sizeof(*r));
  }

  close (clients[index].fd);
  clients[index].fd=-1;
  
  requests++;
}

void add_request(int index, const char *text, int sequence)
{
  replydata *r, *search;

  if (clients[index].length < sequence)
  {
    imc_logerror("add_request: sequence higher than max?");
    return;
  }
  
  r=imc_malloc(sizeof(*r));
  r->sequence=sequence;
  r->text=imc_strdup(text);
  r->next=NULL;

  if (!clients[index].reply)
    clients[index].reply=r;
  else
  {
    for (search=clients[index].reply;
	 search->next && search->next->sequence < sequence;
	 search=search->next)
      ;

    if (search->next && search->next->sequence == sequence)
    {
      imc_logerror("add_request: duplicate rwho sequence?");
      imc_strfree(r->text);
      imc_free(r, sizeof(*r));
    }
    else
    {
      r->next=search->next;
      search->next=r;
    }
  }

  clients[index].received++;
  if (clients[index].length &&
      clients[index].received == clients[index].length)
    complete_request(index);
}

void free_reply(replydata *r)
{
  replydata *next;

  for (; r; r=next)
  {
    next=r->next;
    imc_strfree(r->text);
    imc_free(r, sizeof(*r));
  }
}

void imc_recv_whoreply(const char *to, const char *text, int sequence)
{
  int i, j;

  i=atoi(to);
  if (!i)
    return;

  for (j=0; j<20; j++)
    if (clients[j].fd!=-1 && clients[j].id==i)
    {
      if (sequence<0)
      {
	clients[j].length=-sequence;
	add_request(j, text, -sequence-1);
      }
      else
	add_request(j, text, sequence);
      return;
    }
}

void runclient(int clientfd)
{
  char buf[1000];
  char arg[100];
  char name[IMC_NAME_LENGTH];
  const char *p;
  int r;
  imc_char_data ch;
  int i;

  /* set up */

  for (i=0; i<20; i++)
    if (clients[i].fd==-1)
      break;

  if (i==20)
  {
    imc_log("runclient: no free clients?!");
    close(clientfd);
    return;
  }

  /* read the request */
  alarm(5);

  /* we're fucked if we don't have EINTR, but too bad */
  r=read(clientfd, buf, 1000);
  if (r<0)
  {
    alarm(0);
#ifndef NO_EINTR
    if (errno!=EINTR)
#endif
      imc_logerror("read");
    return;
  }
  
  alarm(0);

  buf[r--]=0;
  while (r>=0 && !isalnum(buf[r]))
    buf[r--]=0;

  p=imc_getarg(buf, arg, 100);
  imc_getarg(p, name, IMC_NAME_LENGTH);
  
  if (!strcasecmp(arg, "who") || !strcasecmp(arg, "info"))
  {
    clients[i].fd=clientfd;
    clients[i].id=id;
    clients[i].length=0;
    clients[i].received=0;
    clients[i].reply=NULL;
    
    freeclients--;
  
    ch.wizi=ch.invis=ch.level=0;
    sprintf(ch.name, "%d", id);
    
    id++;
    
    imc_send_who(&ch, name, buf);
  }
  else if (!strcasecmp(arg, "list"))
  {
    sendlist(clientfd);
    close(clientfd);
  }
  else
  {
    close(clientfd);
  }
}

void main(int argc, char *argv[])
{
  int fd;
  struct sockaddr_un sa;
  int r;
  int maxfd;

  if (argc<2)
  {
    fprintf(stderr, "No socket path specified!\n");
    exit(1);
  }

  signal(SIGPIPE, SIG_IGN);

  imc_startup(CONFIG);
  if ((imc_active < IA_UP) || (imc_lock_file < 0))
  {
    imc_logstring("giving up..");
    /* imc failed to start up, or there's another process on this config */
    imc_shutdown();
    exit(0);
  }

  /* *whap self* why the hell did I have these reversed before? */
  fd=socket(AF_UNIX, SOCK_STREAM, 0);
  if (fd<0)
  {
    imc_lerror("CGI socket creation");
    exit(1);
  }

  sa.sun_family=AF_UNIX;
  strcpy(sa.sun_path, argv[1]);

  unlink(argv[1]); /* toast any old sockets */
  if (bind(fd, (struct sockaddr *)&sa, sizeof(sa))<0)
  {
    imc_lerror("CGI socket bind");
    exit(1);
  }

  if (listen(fd, 5)<0)
  {
    imc_lerror("CGI socket listen");
    exit(1);
  }

  for (r=0; r<20; r++)
    clients[r].fd=-1;

  id=1;
  freeclients=20;

  /* make the socket world-read/write/exec-able */
  chmod(argv[1], 0777);

  while(1)
  {
    int size;
    fd_set in_set, out_set, exc_set;
    struct timeval tv;
    int i;

    /* listen for requests */
    FD_ZERO(&in_set);
    FD_ZERO(&out_set);
    FD_ZERO(&exc_set);

    maxfd=-1;
    
    if (freeclients)
    {
      FD_SET(fd, &in_set);
      maxfd=fd;
    }
    
    for (r=0; r<20; r++)
      if (clients[r].fd!=-1)
      {
	FD_SET(clients[r].fd, &in_set);
	if (clients[r].fd > maxfd)
	  maxfd=clients[r].fd;
      }
    
    tv.tv_sec=imc_get_max_timeout();
    tv.tv_usec=0;
      
    maxfd=imc_fill_fdsets(maxfd, &in_set, &out_set, &exc_set);
    
#if NO_EINTR
    i=select(maxfd+1, &in_set, &out_set, &exc_set, &tv);
#else
    while ((i=select(maxfd+1, &in_set, &out_set, &exc_set, &tv))<0 &&
           errno==EINTR)
      ;
#endif

    if (i<0)
    {
      imc_lerror("select");
      exit(0);
    }

    imc_idle_select(&in_set, &out_set, &exc_set, time(NULL));
      
    if (FD_ISSET(fd, &in_set))
    {
      size=sizeof(sa);
      r=accept(fd, (struct sockaddr *)&sa, &size);
      if (r<0)
	imc_lerror("CGI socket accept"); 
      else
	runclient(r);
    }
    
    for (r=0; r<20; r++)
      if (clients[r].fd!=-1 && FD_ISSET(clients[r].fd, &in_set))
      {
	char dummy[100];
	if (read(clients[r].fd, dummy, 100)<=0)
	{
	  close(clients[r].fd);
	  clients[r].fd=-1;
	  free_reply(clients[r].reply);
	  freeclients++;
	}
      }
  }

  /* never reached */
}

