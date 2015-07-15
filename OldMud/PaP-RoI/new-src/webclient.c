/*
 * IMC2 - an inter-mud communications protocol
 *
 * webclient.c: CGI interface to webserver.c
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

/* Consider this alpha code. 'nuff said */

#include <stdlib.h>
#include <stdio.h>
#include <unistd.h>
#include <sys/socket.h>
#include <sys/un.h>
#include <string.h>
#include <signal.h>
#include <errno.h>

#define PAGE "http://www.toof.net/~imc/webclient.cgi"
#define SOCKET "/home/imc/webserver-socket"
#define EMAIL "oliver@jowett.manawatu.planet.co.nz"

int listflag;
int whoflag;
int infoflag;
char *mudname;
int invalid;
int color;

char retrypath[1000];

void showerror(void)
{
  printf("<html>\n"
	 "<head>\n"
	 "<link rev=\"made\" href=\"mailto:" EMAIL "\">\n"
	 "<meta name=\"robots\" content=\"noindex\">\n"
	 "<title>Error</title>\n"
	 "</head>\n"
	 "<body>\n"
	 "<h1>Error</h1>\n"
	 "<p>An error occured when reading from the IMC interface server.\n"
	 "The server may be down, or your query may be invalid.\n"
	 "You may want to <a href=\"%s\">try again</a> later.\n"
	 "<hr>\n"
	 "<p>Return to the <a href=\"" PAGE "?type=list\">IMC list page</a>.\n"
	 "<p>Please contact <a href=\"mailto:" EMAIL "\">" EMAIL "</a> "
	 "with any problems with this gateway.\n"
	 "</body>\n"
	 "</html>\n", retrypath);
}

void showinternalerror(void)
{
  printf("<html>\n"
	 "<head>\n"
	 "<link rev=\"made\" href=\"mailto:" EMAIL "\">\n"
	 "<meta name=\"robots\" content=\"noindex\">\n"
	 "<title>Internal error</title>\n"
	 "</head>\n"
	 "<body>\n"
	 "<h1>Internal Error</h1>\n"
	 "<p>An internal error occured within the CGI software.\n"
	 "The server may be overloaded. You may want to "
	 "<a href=\"%s\">try again</a> later.\n"
	 "<hr>\n"
	 "<p>Return to the <a href=\"" PAGE "?type=list\">IMC list page</a>.\n"
	 "<p>Please contact <a href=\"mailto:" EMAIL "\">" EMAIL "</a> "
	 "with any problems with this gateway.\n"
	 "</body>\n"
	 "</html>\n", retrypath);
}

struct conv_type {
  char *imc;
  char *mud;
};

struct conv_type color_table[]=
{
  { "~~", "~"  },  /* escape raw tildes */

  { "~b", "</font><font color=#0000DF>" },  /* blue    */
  { "~g", "</font><font color=#00DF00>" },  /* green   */
  { "~r", "</font><font color=#DF0000>" },  /* red     */
  { "~y", "</font><font color=#DFDF00>" },  /* yellow  */
  { "~m", "</font><font color=#DF00DF>" },  /* magenta */
  { "~c", "</font><font color=#00DFDF>" },  /* cyan    */
  { "~w", "</font><font color=#DFDFDF>" },  /* white   */

  { "~D", "</font><font color=#6F6F6F>" },  /* grey        */
  { "~B", "</font><font color=#0000FF>" },  /* lt. blue    */
  { "~G", "</font><font color=#00FF00>" },  /* lt. green   */
  { "~R", "</font><font color=#FF0000>" },  /* lt. red     */
  { "~Y", "</font><font color=#FFFF00>" },  /* lt. yellow  */
  { "~M", "</font><font color=#FF00FF>" },  /* lt. magenta */
  { "~C", "</font><font color=#00FFFF>" },  /* lt. cyan    */
  { "~W", "</font><font color=#FFFFFF>" },  /* lt. white   */

  { "~!", "</font><font color=#DFDFDF>" },  /* reset   */
  { "~d", "</font><font color=#DFDFDF>" },  /* default */
};

struct conv_type nocolor_table[]=
{
  { "~~", "~"  },  /* escape raw tildes */

  { "~b", "" },  /* blue    */
  { "~g", "" },  /* green   */
  { "~r", "" },  /* red     */
  { "~y", "" },  /* yellow  */
  { "~m", "" },  /* magenta */
  { "~c", "" },  /* cyan    */
  { "~w", "" },  /* white   */

  { "~D", "" },  /* grey        */
  { "~B", "" },  /* lt. blue    */
  { "~G", "" },  /* lt. green   */
  { "~R", "" },  /* lt. red     */
  { "~Y", "" },  /* lt. yellow  */
  { "~M", "" },  /* lt. magenta */
  { "~C", "" },  /* lt. cyan    */
  { "~W", "" },  /* lt. white   */

  { "~!", "" },  /* reset   */
  { "~d", "" },  /* default */
};

#define numtrans (sizeof(color_table)/sizeof(color_table[0]))

int handled_alarm;

void alarm_handler(int sig)
{
  handled_alarm=1;
}

void readfromserver(int fd)
{
  char result[20000];
  int r=0;

  handled_alarm=0;
  alarm(30);

  while (r<19999)
  {
    int i;

    i=read(fd, result+r, 19999-r);
    if (i<0)
    {
#ifndef NO_EINTR
      if (errno==EINTR)
        break;
#endif
      showerror();
      return;
    }
    
    if (!i)
    {
      result[r]=0;
      break;
    }

    r+=i;
  }

  alarm(0);

  if (!r || handled_alarm)
  {
    printf("<html>\n"
	   "<head>\n"
	   "<link rev=\"made\" href=\"mailto:" EMAIL "\">\n"
	   "<meta name=\"robots\" content=\"noindex\">\n"
	   "<title>Timeout</title>\n"
	   "</head>\n"
	   "<body>\n"
	   "<h1>Timeout</h1>\n"
	   "<p>No response was received from the specified mud within\n"
	   "30 seconds. The mud may be down, or the network may be having\n"
	   "problems. You may want to <a href=\"%s\">try again</a> later.\n"
	   "<hr>\n"
	   "<p>Return to the <a href=\"" PAGE "?type=list\">IMC list page</a>.\n"
	   "<p>Please contact <a href=\"mailto:" EMAIL "\">" EMAIL "</a> "
	   "with any problems with this gateway.\n"
	   "</body>\n"
	   "</html>\n", retrypath);
  }
  else if (whoflag || infoflag)
  {
    char *in;
    struct conv_type *trans_table;

    printf("<html>\n"
	   "<head>\n"
	   "<link rev=\"made\" href=\"mailto:" EMAIL "\">\n"
	   "<meta name=\"robots\" content=\"noindex\">\n"
           "<meta http-equiv=\"refresh\" content=\"900\">\n"
	   "<title>%s %s</title>\n"
	   "</head>\n"
	   "<body%s>\n"
	   "<h1>Results</h1>\n"
	   "<pre>%s\n",
	   whoflag ? "Results for RWHO" : "Info on",
	   mudname,
	   color ? " bgcolor=#000000 text=#FFFFFF" : "",
	   color ? "<font color=#DFDFDF>" : "");

    trans_table = color ? color_table : nocolor_table;

    for (in=result; *in; )
    {
      if (*in=='&')
      {
	printf("&amp;");
	in++;
	continue;
      }
      else if (*in=='<')
      {
	printf("&lt;");
	in++;
	continue;
      }
      else if (*in=='>')
      {
	printf("&gt;");
	in++;
	continue;
      }
      else if (*in=='~')
      {
	int i, l;

	for (i=0; i<numtrans; i++)
	{
	  l=strlen(trans_table[i].imc);
	  if (l && !strncmp(in, trans_table[i].imc, l))
	    break;
	}
      
	if (i!=numtrans)       /* match */
	{
	  printf(trans_table[i].mud);
	  in+=l;
	  continue;
	}
      }
      else if (*in=='\r')
      {
	in++;
	continue;
      }

      putchar(*in++);
    }
  
    printf("%s</pre><hr>\n"
	   "<p>Return to the <a href=\"" PAGE "?type=list\">IMC list page</a>.\n"
	   "<p>Please contact <a href=\"mailto:" EMAIL "\">" EMAIL "</a> "
	   "with any problems with this gateway.\n"
	   "</body></html>\n",
	   color ? "</font>" : "");
  }
  else /* list */
  {
    char name[100], version[100];
    int i, count, ping;
    char *in;

    printf("<html>\n"
	   "<head>\n"
	   "<link rev=\"made\" href=\"mailto:" EMAIL "\">\n"
	   "<meta name=\"robots\" content=\"noindex\">\n"
           "<meta http-equiv=\"refresh\" content=\"600\">\n"
	   "<title>IMC List</title>\n"
	   "</head>\n"
	   "<body>\n"
	   "<h1>Active muds on IMC</h1>\n"
           "<hr>\n"
           "<p>Select a mud below to get information on.\n"
	   "<p><form action=\"" PAGE "\" method=\"get\">\n"
           "Query type: <select name=\"type\" size=1>\n"
           "<p><option>Help\n"
           "<option selected>Who\n"
           "<option>Info\n"
           "</select>\n"
           "<p><input type=\"checkbox\" name=\"color\" value=\"yes\" checked>"
           "Use color in output\n<p>");

    in=result;
    while(1)
    {
      i=sscanf(in, "%s %s %d\n%n", name, version, &ping, &count);
      if (i<3)
	break;

      in+=count;

      printf("<input type=\"radio\" name=\"mud\" value=\"%s\">%-20s %s<br>\n",
	     name, name, version);
    }

    printf("<p><input type=\"submit\" value=\"Send request\">\n"
           "</form><hr>\n"
	   "<p>Please contact <a href=\"mailto:" EMAIL "\">" EMAIL "</a> "
	   "with any problems with this gateway.\n"
	   "<p>Go to the <a href=\"http://www.toof.net/~imc/\">"
	   "IMC2 homepage</a>.\n"
	   "</body></html>\n");
  }
}

/* parse the input */

char x2c(const char *what)
{
  char digit;
  
  digit = (what[0] >= 'A' ? ((what[0] & 0xdf) - 'A')+10 : (what[0] - '0'));
  digit *= 16;
  digit += (what[1] >= 'A' ? ((what[1] & 0xdf) - 'A')+10 : (what[1] - '0'));
  return(digit);
}

void parse_value(const char *name, const char *value)
{
  if (!strcasecmp(name, "type"))
  {
    if (!strcasecmp(value, "list"))
      listflag=1;
    else if (!strcasecmp(value, "who"))
      whoflag=1;
    else if (!strcasecmp(value, "info"))
      infoflag=1;
    else
      invalid=1;
  }
  else if (!strcasecmp(name, "mud"))
    mudname=strdup(value);
  else if (!strcasecmp(name, "color"))
    color=!strcasecmp(value, "yes");
  else
    invalid=1;
}

void parse(const char *string)
{
  char buf1[100], buf2[100];
  char *out=buf1;
  int state=0;
  
  while (*string)
  {
    switch(*string)
    {
    case '%':
      *out++=x2c(string);
      string+=3;
      break;
    case '+':
      *out++=' ';
      string++;
      break;
    case '=':
      if (state==0)
      {
	*out=0;
	out=buf2;
	state=1;
      }
      else
	*out++='=';
      
      string++;
      break;
    case '&':
      if (state==0)
      {
	*out=0;
	parse_value(buf1, out);
      }
      else
      {
	*out=0;
	parse_value(buf1, buf2);
	out=buf1;
	state=0;
      }
      
      string++;
      break;
    default:
      *out++=*string++;
      break;
    }
  }

  if (state==1)
  {
    *out=0;
    parse_value(buf1, buf2);
    out=buf1;
    state=0;
  }
}    

void main(int argc, char *argv[])
{
  int fd;
  struct sockaddr_un sa;
  char buf[200];
  char *temp;

  signal(SIGPIPE, SIG_IGN);
  signal(SIGALRM, alarm_handler);

  printf("Content-type: text/html\n\n");

  temp=getenv("QUERY_STRING");
  if (!temp)
  {
    invalid=1;
    sprintf(retrypath, "%s?type=list", PAGE);
  }
  else
  {
    sprintf(retrypath, "%s?%s", PAGE, temp);
    parse(temp);
  }

  if (invalid || ((whoflag || infoflag) && !mudname))
  {
    showerror();
    exit(1);
  }

  if (!whoflag && !listflag && !infoflag)
    listflag=1;

  fd=socket(AF_UNIX, SOCK_STREAM, 0); /* DOH! and again */
  if (fd<0)
  {
    showinternalerror();
    exit(1);
  }

  sa.sun_family=AF_UNIX;
  strcpy(sa.sun_path, SOCKET);
  if (connect(fd, (struct sockaddr *)&sa, sizeof(sa))<0)
  {
    showerror();
    exit(1);
  }

  sprintf(buf,
	  "%s%s%s\n",
	  listflag ? "list" : (whoflag ? "who" : "info"),
	  listflag ? "" : " ",
	  listflag ? "" : mudname);

  if (write(fd, buf, strlen(buf))<0)
  {
    showerror();
    exit(1);
  }

  readfromserver(fd);

  close(fd);

  exit(0);  
}

