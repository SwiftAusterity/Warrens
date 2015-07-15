#if defined( macintosh )
#include <types.h>
#else
#include <sys/types.h>
#include <sys/time.h>
#endif
#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#include <fcntl.h>
#include <netdb.h>
#include <signal.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <arpa/telnet.h>
#include <errno.h>
#include "merc.h"

#define AUTH_UNSENT	0
#define AUTH_SENT	1
#define AUTH_RETRY	2

struct auth_data
{
  struct auth_data *next;
  struct auth_data *prev;
  DESCRIPTOR_DATA *d;
  int auth_fd;
  char abuf[MAX_INPUT_LENGTH];
  int auth_inc;
  int auth_state;
  int atimes;
};

struct auth_data *first_auth;
struct auth_data *last_auth;
struct auth_data *auth_free;

void send_auth( struct auth_data *auth );
void read_auth( struct auth_data *auth );

int read( int fd, char *buf, int nbyte );
int write( int fd, char *buf, int nbyte );
int close( int fd );

#define LINK(link, first, last, next, prev)	\
do {						\
  if ( (first) == NULL )			\
    (first) = (link);				\
  else						\
    (last)->next = (link);			\
  (link)->next = NULL;				\
  (link)->prev = (last);			\
  (last) = (link);				\
} while(0)

#define UNLINK( link, first, last, next, prev )	\
do {						\
  if ( !(link)->next )				\
    (last)		= (link)->prev;		\
  else						\
    (link)->next->prev	= (link)->prev;		\
  if ( !(link)->prev )				\
    (first)		= (link)->next;		\
  else						\
    (link)->prev->next	= (link)->next;		\
} while(0)

#define GET_FREE(point, freelist) \
do { \
  if ( freelist != NULL ) \
  { \
    point = freelist; \
    freelist = freelist->next; \
  } \
  else \
    point = alloc_perm(sizeof(*point)); \
  memset(point, 0, sizeof(*point)); \
} while(0)

#define PUT_FREE(point, freelist) \
do { \
  point->next = freelist; \
  freelist = point; \
} while(0)

#define RLSTRIP(str) \
do { \
  int pst; \
  while ( isspace(*(str)) ) \
    (str)++; \
  for ( pst = strlen(str)-1; pst >= 0 && isspace((str)[pst]); pst-- ) \
    (str)[pst] = '\0'; \
} while(0)

bool start_auth( DESCRIPTOR_DATA *d )
{
  struct sockaddr_in sock;
  int tlen;
  struct auth_data *auth;
  int desc;
  
  desc = socket(AF_INET, SOCK_STREAM, 0);
  if ( desc < 0 && errno == EAGAIN )
  {
    bug( "Can't allocate fd for authorization check on %s.", (int)d->host );
    free_string(d->user);
    d->user = str_dup("(no auth_fd)");
    return FALSE;
  }
  if ( fcntl(desc, F_SETFL, O_NDELAY) == -1 )
  {
    perror("Nonblock");
    close(desc);
    free_string(d->user);
    d->user = str_dup("(nonblock)");
    return FALSE;
  }
  tlen = sizeof(sock);
  getpeername(d->descriptor, (struct sockaddr *)&sock, &tlen);
  sock.sin_port = htons(113);
  sock.sin_family = AF_INET;
  
  if ( connect(desc, (struct sockaddr *)&sock, sizeof(sock)) == -1 &&
       errno != EINPROGRESS )
  {
    bug( "Identd denied for %s.", (int)d->host );
    close(desc);
    free_string(d->user);
    d->user = str_dup("(no verify)");
    return FALSE;
  }
  if ( errno == ECONNREFUSED )
  {
    close(desc);
    free_string(d->user);
    d->user = str_dup("(no identd)");
    return FALSE;
  }
  for ( auth = first_auth; auth; auth = auth->next )
    if ( auth->d == d )
      break;
  if ( !auth )
  {
    GET_FREE(auth, auth_free);
    auth->d = d;
    auth->atimes = 70;
    LINK(auth, first_auth, last_auth, next, prev);
  }
  auth->auth_fd = desc;
  auth->abuf[0] = '\0';
  auth->auth_state = AUTH_UNSENT;
  auth->auth_inc = 0;
  return TRUE;
}

#define END_AUTH(auth, username) \
do { \
  free_string(auth->d->user); \
  auth->d->user = str_dup(username); \
  close(auth->auth_fd); \
  UNLINK(auth, first_auth, last_auth, next, prev); \
  PUT_FREE(auth, auth_free); \
  return; \
} while(0)
#define RESET_AUTH(auth) \
do { \
  close(auth->auth_fd); \
  auth->auth_fd = -1; \
  auth->auth_state = AUTH_RETRY; \
  return; \
} while(0)

void send_auth( struct auth_data *auth )
{
  struct sockaddr_in us, them;
  char authbuf[32];
  int ulen, tlen, z;

  tlen = ulen = sizeof(us);
  if ( --auth->atimes == 0 )
    END_AUTH(auth, "(auth failed)");
  if ( getsockname(auth->d->descriptor, (struct sockaddr *)&us, &ulen) ||
       getpeername(auth->d->descriptor, (struct sockaddr *)&them, &tlen) )
  {
    bug( "auth getsockname/getpeername error", 0 );
    RESET_AUTH(auth);
  }
  sprintf(authbuf, "%u , %u\r\n", ntohs(them.sin_port), ntohs(us.sin_port));
  z = write(auth->auth_fd, authbuf, strlen(authbuf));
  if ( errno == ECONNREFUSED )
    END_AUTH(auth, "(no identd)");
  if ( z != strlen(authbuf) )
  {
    if ( auth->atimes == 1 )
    {
      sprintf(log_buf, "auth request, broken pipe [%d/%d]", z, errno);
      log_string(log_buf, CHANNEL_CODER, -1);
    }
    RESET_AUTH(auth);
  }
  auth->auth_state = AUTH_SENT;
  return;
}

char *my_onearg( char **str, char cEnd )
{
  char *arg;
/*  char cEnd = ' ';*/
  
  while ( isspace(**str) )
    (*str)++;
/*  if ( **str == ':' )
  {
    cEnd = **str;
    (*str)++;
  }*/
  arg = *str;
  while ( **str != '\0' )
  {
    if ( (cEnd == ' ' ? isspace(**str) : **str == cEnd) )
    {
      **str = '\0';
      (*str)++;
      while ( isspace(**str) )
        (*str)++;
      break;
    }
    (*str)++;
  }
  RLSTRIP(arg);
  return arg;
}

/*char one_char(char **str)
{
  char letter;
  
  while ( isspace(**str) )
    (*str)++;
  letter = **str;
  (*str)++;
  while ( isspace(**str) )
    (*str)++;
  return letter;
}*/

void read_auth( struct auth_data *auth )
{
  int len;
  char ruser[20], system[20];
  char *s;
  extern int port;

  *system = *ruser = '\0';
  if ( (len = read(auth->auth_fd, auth->abuf+auth->auth_inc,
        sizeof(auth->abuf)-1-auth->auth_inc)) > 0 )
  {
    auth->auth_inc += len;
    auth->abuf[auth->auth_inc] = '\0';
  }
  if ( len > 0 && auth->auth_inc != sizeof(auth->abuf)-1 )
  {
    s = auth->abuf;
    RLSTRIP(s);
    bug(s,0);
    if ( !*s )
      END_AUTH(auth, "(blank auth)");
    if ( !atoi(my_onearg(&s, ',')) || atoi(my_onearg(&s, ':')) != port )
      END_AUTH(auth, "(invalid identd)");
    my_onearg(&s, ':');
    strncpy(system, my_onearg(&s, ':'), sizeof(system)-1);
    if ( !str_cmp(system, "OTHER") )
      END_AUTH(auth, "(invalid system)");
    strncpy(ruser, my_onearg(&s, ' '), sizeof(ruser)-1);
    if ( !*ruser )
      END_AUTH(auth, "(no username)");
    sprintf(log_buf, "auth reply ok, incoming user: [%s]", ruser);
    log_string(log_buf, CHANNEL_CODER, -1);
  }
  else if ( len != 0 )
  {
    if ( !index(auth->abuf, '\n') && !index(auth->abuf, '\r') )
      return;
    sprintf(log_buf, "bad auth reply: %s", auth->abuf);
    log_string(log_buf, CHANNEL_CODER, -1);
    strcpy(ruser, "(no auth)");
  }
  END_AUTH(auth, ruser);
}
#undef END_AUTH

void auth_maxdesc( int *maxdesc, fd_set *in_set, fd_set *out_set,
                   fd_set *exc_set )
{
  struct auth_data *auth;

  for ( auth = first_auth; auth; auth = auth->next )
  {
    if ( auth->auth_state == AUTH_RETRY )
      continue;
    *maxdesc = UMAX(*maxdesc, auth->auth_fd);
    FD_SET(auth->auth_fd, in_set );
    FD_SET(auth->auth_fd, out_set);
    FD_SET(auth->auth_fd, exc_set);
  }
  return;
}

void auth_update( fd_set *in_set, fd_set *out_set, fd_set *exc_set )
{
  struct auth_data *auth;
  struct auth_data *a_next;

  for ( auth = first_auth; auth; auth = a_next )
  {
    a_next = auth->next;
   /* if ( FD_ISSET(auth->auth_fd, exc_set) )
    {
      FD_CLR(auth->auth_fd, in_set );
      FD_CLR(auth->auth_fd, out_set);
      close(auth->auth_fd);
      free_string(auth->d->user);
      auth->d->user = str_dup("(fd exception)");
      UNLINK(auth, first_auth, last_auth, next, prev);
      PUT_FREE(auth, auth_free);
      continue;
    }*/
    switch(auth->auth_state)
    {
    case AUTH_UNSENT:
      if ( FD_ISSET(auth->auth_fd, out_set) )
        send_auth(auth);
      break;
    case AUTH_SENT:
      if ( FD_ISSET(auth->auth_fd, in_set ) )
        read_auth(auth);
      break;
    case AUTH_RETRY:
      if ( !start_auth(auth->d) )
      {
        UNLINK(auth, first_auth, last_auth, next, prev);
       /* PUT_FREE(auth, auth_free);*/
        continue;
      }
      break;
    }
  }
  return;
}

void auth_check( DESCRIPTOR_DATA *d )
{
  struct auth_data *auth;

  for ( auth = first_auth; auth; auth = auth->next )
    if ( auth->d == d )
    {
      if ( auth->auth_state != AUTH_RETRY )
        close(auth->auth_fd);
      UNLINK(auth, first_auth, last_auth, next, prev);
      PUT_FREE(auth, auth_free);
      break;
    }
  return;
}
