

To: "Tim A. Seever" <tseever@freenet.calgary.ab.ca>

/*************************************************************************
* LodMUD Beta MUD  0.0                                                   *
**************************************************************************

**************************************************************************
* (c) 1993,1994,1995 Erik Jensen, Petter N Hagen, Jan Ove Saevik,        *
* Bjoern Hammer, Edward Kmett, Lars Soepstad, Espen Loevaas, and         *
* Helger Lipmaa.                                                         *
**************************************************************************

**************************************************************************
* land.c (main.c) - main() & communications module (all socket ops. here)*
* Last update: 4 jan 95 - Simkin - added who daemon and authd code       *
*************************************************************************/

/* The headers need to be cleaned up BAD */

#ifdef __sgi__
    #define _BSD_SIGNALS
    #ifdef MIPSEB
        #define _MIPSEB
    #endif
    #include <sys/endian.h>
#endif

#include <sys/types.h>
#include <stdlib.h>
#include <strings.h>
#include <string.h>
#include <stdio.h>
#include <math.h>
#ifdef __sgi__
#include <bstring.h>
#include <sys/socket.h>
#endif

/**** No <unistd.h> or <malloc.h> on the NeXT ****/
/* #if !defined(NeXT) */
    #include <stdarg.h>
    #include <unistd.h>
/*    #include <malloc.h>*/
/*#endif*/
/**************************************************/

#include <memory.h>
#include <errno.h>
#include <ctype.h>
#include <sys/socket.h>
#include <sys/wait.h>
#include <netdb.h>
#include <sys/time.h>
#include <sys/resource.h>
#include <sys/file.h>
#include <sys/ioctl.h>
/*#include <netinet/in.h>*/
#include <fcntl.h>
#include <signal.h>
#include <limits.h>
#include "structs.h"

#include "mob.h"
#include "obj.h"
#include "utils.h"
#include "interp.h"
#include "handler.h"
#include "db.h"
#include "matrix.h"

/* All over global */
int MOBtrigger = TRUE;
int pulse = 0;

#if defined(NeXT)
/* System functions */
extern int socket( int domain, int type, int protocol );

extern int setsockopt( int s, int level, int optname, void *optval, int optlen )
;
extern int bind( int s, struct sockaddr *name, int namelen );
extern int select( int width, fd_set *readfds, fd_set *writefds, fd_set *exceptf
ds, struct timeval *timeout );
extern int recv ( int s, char *buf, int len, int flags );
extern int listen( int s, int backlog );


extern int getsockname( int s, struct sockaddr *name, int *namelen );
extern int accept( int s, struct sockaddr *addr, int *addrlen );
extern int getpeername( int s, struct sockaddr *name, int *namelen );
extern int gettimeofday( struct timeval *tp, struct timezone *tzp );
#endif

extern char * inet_ntoa( struct in_addr in );
char * who_list;
int who_flag;

#define DFLT_PORT         5000
#define DFLT_WHO_PORT     5001
#define DFLT_SPLIT_PORT   5002
#define DFLT_EVIL_PORT    5666
#define DFLT_EVIL_SPLIT   5667
#define MAX_NAME_LENGTH     15
#define MAX_HOSTNAME       256
#define OPT_USEC        250000      /* Microseconds per pass    */

extern int errno;

/* externs */


extern char *greetings[2];

extern struct room_data *world;
extern int top_of_world;
extern struct time_info_data time_info;
extern char *help;
extern char *glossary;
extern struct ban_t *ban_list, *ncsa_list;
extern char *vtcolors[];
extern struct obj_index_data *obj_index;

/* local globals */

struct descriptor_data *descriptor_list, *next_to_process;

int god        = 0;  /* all new chars are gods! */
int slow_death = 0;  /* Time of shutdown, active with one of the two below */
int shut_down  = 0;  /* clean shutdown */
int re_boot    = 0;  /* start her up again ? */
int death_msg  = 0;  /* Warning indicator, 100 = shut her down */
char down_by[20];    /* Name of player who scheduled shutdown */
char down_reason[200]; /* Reason given for shutdown */
int spam_o_meter = 0;  

int maxdesc;

void shutdown_request(void);
void wizlogsig(void);
void hupsig(void);

void check_ban(struct descriptor_data *d);
char * get_from_q(struct txt_q *queue);
void game_loop(int control, int who_control, int split_control, int evil_control
 , int evil_split_control);
int init_socket(int port);
int new_connection(int s);
#ifdef NeXT
extern int connect(int s, struct sockaddr *name, int namelen);
#endif

int new_descriptor(int s, int x, int evil); /* socket and display type */
int process_output(struct descriptor_data *t);
int process_input(struct descriptor_data *t, bool newline);

#define FLAG_WRAUTH    1  /* Auth unsent yet, send if able         */
#define FLAG_AUTH      2  /* Authorization in progress             */


void    start_auth(struct descriptor_data *d); /* Open Socket           */
void    send_auth(struct descriptor_data *d);  /* Send to Socket        */
void    read_auth(struct descriptor_data *d);  /* Read Auth Reply       */

void close_socket(struct descriptor_data *d);
void flush_queues(struct descriptor_data *d);
void nonblock(int s);
void parse_name(struct descriptor_data *desc, char *arg);
/* int number_playing(void); */

/* extern functions */
#ifdef EXPLICIT
extern int sendto(int s, const char *msg, int len, int flags,
                  const struct sockaddr *to, int tolen);
extern int recvfrom(int s, char *buf, int len, int flags,
                  struct sockaddr *from, int *fromlen);
#endif
struct char_data *make_char(char *name, struct descriptor_data *desc);
void boot_db( int port );
void zone_update(void);
void bleeding_update(void);
void affect_update( void ); /* In spells.c */
void point_update( int pulse );  /* In limits.c */

void mobile_activity(void);
void move_activity(int);
void string_add(struct descriptor_data *d, char *str);
void editor(struct descriptor_data *d, char *str);
void perform_violence(void);
void show_string(struct descriptor_data *d, char *input, bool first);
void make_prompt( char *buff, struct char_data *ch) ;


int main( int argc, char *argv[] )
{
    int pos;
    int port    = DFLT_PORT;
    int who_port= DFLT_WHO_PORT;
    int split_port = DFLT_SPLIT_PORT;
    int evil_port = DFLT_EVIL_PORT;
    int evil_split_port = DFLT_EVIL_SPLIT;
    char *dir   = DFLT_DIR;
    int control, who_control, split_control, evil_control, evil_split_control;
    char buf[200];

    for ( pos = 1; pos < argc && argv[pos][0] == '-' ; pos++ )
    {


        switch (*(argv[pos] + 1))
        {
        case 'g':
            god = 1;
            wizlog( "God creation mode selected." );
            break;

        case 'd':
            if ( argv[pos][2] != '\0' )
                dir = &argv[pos][2];
            else if (++pos < argc)
                dir = &argv[pos][0];
            else
            {
                fprintf( stderr, "Directory arg expected after -d.\n" );
                exit( 2 );
            }
            break;
        case 'w':
            if ( argv[pos][2] != '\0' )
                who_port=atoi(&argv[pos][2]);
            else if (++pos < argc)
                who_port=atoi(&argv[pos][0]);

            else
            {
                fprintf( stderr, "Port Arg expected after -w.\n" );
                exit( 2 );
            }
            if (who_port<1024)
            {
                fprintf( stderr, "Illegal Port\n");
                exit( 2 );
            }
            break;

        case 's':
            if ( argv[pos][2] != '\0' )
                split_port=atoi(&argv[pos][2]);
            else if (++pos < argc)
                split_port=atoi(&argv[pos][0]);
            else
            {
                fprintf( stderr, "Port Arg expected after -s.\n" );
                exit( 2 );
            }
            if (split_port<1024)


            {
                fprintf( stderr, "Illegal Port\n");
                exit( 2 );
            }
            break;
        case 'e':
            if ( argv[pos][2] != '\0')
                evil_port=atoi(&argv[pos][2]);
            else if (++pos < argc)
                evil_port=atoi(&argv[pos][0]);
            else
            {
                fprintf( stderr, "Port Arg expected after -e.\n" );
                exit( 2 );
            }
            if (evil_port<1024)
            {
                fprintf( stderr, "Illegal Port\n");
                exit( 2 );
            }
            break;
        case 'f':
            if ( argv[pos][2] != '\0')


                evil_split_port=atoi(&argv[pos][2]);
            else if (++pos < argc)
                evil_split_port=atoi(&argv[pos][0]);
            else
            {
                fprintf( stderr, "Port Arg expected after -e.\n" );
                exit( 2 );
            }
            if (evil_split_port<1024)
            {
                fprintf( stderr, "Illegal Port\n");
                exit( 2 );
            }
            break;

        default:
            fprintf( stderr, "Unknown option -%c.\n", argv[pos][1] );
            exit( 2 );
            break;
        }
    }

    if (pos < argc)

    {
        if (!isdigit(argv[pos][0]))
        {
            fprintf( stderr, "Usage: %s [-g][-d pathname][-s split][-w who][-e e
vil][-f evilsplit][port #]\n",
                argv[0] );
            exit( 2 );
        }
        else if ( ( port = atoi(argv[pos]) ) < 1024 )
        {
            printf( "Illegal port #\n" );
            exit( 2 );
        }
    }

    wizlogf("[Who %d] [Split %d] [Port %d] [Dir %s].", who_port, split_port, por
t, dir );

    if ( chdir( dir ) < 0 )
    {
        perror( dir );
        exit( 2 );
    }


    srandom( time(0) );

    /*
     * Optional memory tuning.
     */
#if defined(sun)
    mallopt( M_MXFAST, 96 );
    mallopt( M_NLBLKS, 1024 );
#endif

    signal( SIGPIPE, SIG_IGN );

    who_control = init_socket( who_port );
    split_control = init_socket( split_port );
    control = init_socket( port );
    evil_control = init_socket (evil_port);
    evil_split_control = init_socket (evil_split_port);
    alarm(3*60);       /* If boot's longer that 3 mins, somethings wrong */
    boot_db( port );
    game_loop( control, who_control, split_control, evil_control, evil_split_con
trol );


    sprintf(buf,"%s by %s, Reason: %s\n\n",re_boot ? "Reboot" : "Shutdown",down_
by, down_reason);
    while ( descriptor_list ) {
        if (descriptor_list->character) {
            save_char_obj(descriptor_list->character);
            write_to_q(buf,&descriptor_list->output);
            write_to_q(VT_Normal(descriptor_list->character), &descriptor_list->
output) ;
            write_to_q("\n\n", &descriptor_list->output) ;
        }
        close_socket( descriptor_list );
    }
    wizlog(buf);
    if (re_boot) exit(10) ;
    else exit(0);
    return 0;
}


/* Accept new connects, relay commands, and call 'heartbeat-funcs' */
void game_loop( int control, int who_control, int split_control, int evil_contro
l, int evil_split_control )
{

    fd_set input_set, output_set, exc_set, who_set;
    int nfds;
    struct timeval last_time, now_time, stall_time;
    time_t now;
    static struct timeval null_time = {0, 0};
    struct descriptor_data *point, *next_point;
    char buf[8192], *pcomm, anim[16]="";
    int mask, z;
    bool fStall;
    extern const char *anim_strings[][20];
    extern void do_who(struct char_data *ch, char *argument, int cmd);
    struct char_data who_daemon; /* Pretend the port is a player */
    extern const sbyte anim_point  [][4];
    void mail(struct descriptor_data *d, char *arg);
    extern void return_pets(CHAR_DATA *ch);

    gettimeofday(&last_time, (struct timezone *) 0);

    maxdesc = control;

    mask = sigmask(SIGUSR1) | sigmask(SIGUSR2) | sigmask(SIGINT)  |
           sigmask(SIGPIPE) | sigmask(SIGALRM) | sigmask(SIGTERM) |
           sigmask(SIGURG)  | sigmask(SIGXCPU) | sigmask(SIGHUP)  |

           sigmask(SIGVTALRM);


    who_flag = 0; /* Not using who hack yet */
    GET_LEVEL(&who_daemon)=1; /* All other flags default off 
                                 May one day read a character file, 
                                 But that could cause complications */

#if 0
    mallocmap();
#endif

    /* Main loop */
    while ( (!shut_down && !re_boot) || (slow_death - time(0) >= 0)) 
      {

        /* Check what's happening out there */

        alarm(40);     /* If not back here in 30 sec... down she goes */
                       /* Timeout -> SIGALRM -> Goodbye */

        now = time(NULL);
        FD_ZERO(&who_set);
        FD_ZERO(&input_set);
        FD_ZERO(&output_set);
        FD_ZERO(&exc_set);
        FD_SET(control, &input_set);
        FD_SET(who_control, &input_set);
        FD_SET(split_control, &input_set);
        FD_SET(evil_control, &input_set);
        FD_SET(evil_split_control, &input_set);
        nfds=0;
        for (point = descriptor_list; point; point = point->next) 
          {
            FD_SET(point->descriptor, &input_set);
            FD_SET(point->descriptor, &exc_set);
            FD_SET(point->descriptor, &output_set);
            if (point->auth_fd!=-1) 
              {
               FD_SET(point->auth_fd, &input_set);
               if (IS_SET(point->auth_state,FLAG_WRAUTH)) FD_SET(point->auth_fd,
 &output_set);
              }
          }
        
        sigsetmask(mask);


        if ((select(maxdesc + 1, &input_set, &output_set, &exc_set, &null_time))
 < 0) {
            perror("Select Poll: Main Port");
            exit(2);
        }

        sigsetmask(0);

        /* Respond to whatever might be happening */
        

        /* Who Daemon */

        if (FD_ISSET(who_control, &input_set )){
            who_flag=1; /** Cheesy hard-coded access to the who list */
            do_who(&who_daemon,"",0); /* Load who buffer */ 
            if ((z=new_connection(who_control))>=0) { 
                write_to_descriptor(z,who_list);
                close(z);
            } else wizlog("who connection bombed.");
            wizlog("Who daemon polled.");
            FREE(who_list);

            who_flag=0;
        }

        /* New connection */ 
        if (FD_ISSET(evil_control, &input_set) && new_descriptor(evil_control,0,
1) < 0) perror("Evil Connection");

        if (FD_ISSET(evil_split_control, &input_set) && new_descriptor(evil_spli
t_control,1,1) < 0) perror("Evil Split connection");

        if (FD_ISSET(split_control, &input_set) && new_descriptor(split_control,
1,0) < 0) perror("Split connection");

        if (FD_ISSET(control, &input_set) && new_descriptor(control,0,0) < 0) pe
rror("New connection");

        /* Handle Authorization */
        for (point = descriptor_list; point; point = next_point) 
          {
            next_point = point->next;
            if (FD_ISSET(point->descriptor, &exc_set))
              {
                FD_CLR(point->descriptor, &input_set);
                FD_CLR(point->descriptor, &output_set);
                if ( point->character ) save_char_obj(point->character);
                close_socket(point);
                continue;
              }
             if (point->auth_fd==-1) continue;
             
             if (FD_ISSET(point->auth_fd, &input_set))
                  {
                    read_auth(point);
                    if (!point->auth_state) check_ban(point);
                  }
             else if ((FD_ISSET(point->auth_fd, &output_set))&&
                  IS_SET(point->auth_state,FLAG_WRAUTH))
                  { 
                    send_auth(point);
                    if (!point->auth_state) check_ban(point);
                  }    
          }
                
        /* Read input */
        for (point = descriptor_list; point; point = next_point) 
          {

            next_point = point->next;
            if (FD_ISSET(point->descriptor, &input_set)) 
              {
                if (point->showstr_point ?
                    (process_input(point, FALSE) < 0) :
                    (process_input(point, TRUE) < 0)) 
                  {
                    if ( point->character ) save_char_obj( point->character );
                    close_socket(point);
                    continue;
                  }
                point->newline = FALSE;
              }
            else point->newline = TRUE;

          }

        /* process_commands; */
        for (point = descriptor_list; point; point = next_to_process) 
          {
            next_to_process = point->next;

            if (--(point->wait) <= 0 &&

                (pcomm = get_from_q( &point->input )) != NULL )
              {
                if (point->character &&
                    (point->connected == CON_PLAYING || point->connected == CON_
EDITOR) &&
                    (point->character->specials.was_in_room != NOWHERE)) 
                  {
                    if (point->character->in_room != NOWHERE)
                      char_from_room(point->character);
                    char_to_room(point->character, point->character->specials.wa
s_in_room);
                    point->character->specials.was_in_room = NOWHERE;
                    act("$n has returned.", TRUE, point->character, 0, 0, TO_ROO
M);
                    affect_total(point->character);
                  }

                point->wait = 1;

                if (point->character) point->character->specials.timer = 0;

                if (point->showstr_point) show_string(point, pcomm, FALSE);
                else if (point->connected == CON_EDITOR) editor(point, pcomm);
                else if (point->connected == CON_MAILER) mail(point, pcomm);
                else if (point->connected != CON_PLAYING) nanny(point, pcomm);
                else command_interpreter(point->character, pcomm, 1);
                
                FREE( pcomm );  /*** DUMPER CORE - EJ */

                /* Cheesy way to force prompts */
                write_to_q( "", &point->output );
              } /*
            else if (point->wait > 0 && point->wait_type == ANIM_NOTHING) 
              {
                sprintf(anim, "\r%3d", point->wait) ;
                write_to_descriptor(point->descriptor,anim) ;
              } */
        }

#define anim_type   (point->wait_type)
#define anim_wait   (point->wait-2)
#define anim_num    anim_point[anim_type][0]
#define anim_freeze anim_point[anim_type][1]
#define anim_loop   anim_point[anim_type][2]
#define anim_length anim_point[anim_type][3]


       /* give the people some prompts */

        for (point = descriptor_list; point; point = next_point) 
          {
            next_point = point->next;
            /* autosave ? */
            if (FD_ISSET(point->descriptor, &output_set) && point->character &&
                point->connected == CON_PLAYING && !point->original)
              if ( (time(0) - point->character->player.time.last_save) >= 300 ) 
                {
                  point->character->player.time.last_save = time(0) ;
                  if(!IS_SET(point->character->specials.act,PLR_BUSY))
                    /* STCf("Autosaving %s.\n", point->character, GET_SHORT(poin
t->character)) ; */
                    save_char_obj( point->character ) ;
                    return_pets(point->character);
                }
            if (FD_ISSET(point->descriptor, &output_set) && point->output.head)
              {
                if ( point->wait <= 1 || !anim_freeze ) 
                  {
                    /* editor */
                    if (point->connected == CON_EDITOR) 

                      {
                        if (!point->showstr_point)
                          switch(point->sub_prompt) {
                           case 0 : write_to_q( "==>", &point->output) ;break;
                           case 1 : write_to_q( "Old String: ", &point->output);
 break;
                           case 2 : write_to_q( "New String: ", &point->output);
 break;
                           default: write_to_q( "Bug? ",&point->output); break;
                          }
                      } 
                    else if (point->connected != CON_PLAYING) ;
                    else if (point->showstr_point) ;
                    /* output string */
                    else 
                      {
                        make_prompt(buf, point->character) ;
                        if ((!point->contype)||(point->connected!=CON_PLAYING)) 
{
                                write_to_q(buf, &point->output) ;
                        } else {
                            if (strcasecmp(buf,point->last_prompt)){ /*dif*/
                                write_to_descriptor(point->descriptor,"\033[23;1

H\033[K");
                                write_to_descriptor(point->descriptor,buf);
                                FREE(point->last_prompt);
                                point->last_prompt=str_dup(buf);
                                point->sending=0;
                                sprintf(buf,"\033[24;1H%s",point->buf);
                                write_to_descriptor(point->descriptor,buf);
                            };
                        }
                      }
                    if (process_output(point) < 0) 
                      {
                        if ( point->character )
                          save_char_obj( point->character );
                        close_socket(point) ;
                      }
                  } 
                else if ( anim_length && anim_type >= ANIM_NOTHING && anim_type 
<= ANIM_PRACTICE
                         && (anim_loop || anim_wait < anim_length)) 
                  {
                    sprintf(anim, "\r%s", anim_strings[anim_num][anim_wait % ani
m_length]) ;

                    write_to_descriptor(point->descriptor,anim) ;
                  }
              }
          }

#undef  anim_num*
#undef  anim_freeze
#undef  anim_loop
#undef  anim_length
#undef  anim_type
#undef  anim_wait

        /* See if everyone wants clock FAST. */
        fStall = TRUE;
/*
        for (point = descriptor_list; point; point = next_to_process)
        {
            if ( point->connected != CON_PLAYING ) continue;
            if ( point->tick_wait > 0 ) continue;
            fStall = TRUE;
        }
*/
        /*

mbox (46%)

         * Heartbeat.
         * All autonomous actions (including fighting and healing)
         * are subdivisions of the basic pulse at OPT_USEC interval.
         */
        
        pulse++;

        if ( !(pulse % PULSE_ZONE) ) zone_update();

        if ( !(pulse % PULSE_BLEED) ) bleeding_update();

        if ( !(pulse % PULSE_WEATHER) ) weather_and_time(1);

        if ( !(pulse % PULSE_MOBILE) ) mobile_activity();

        if ( !(pulse % PULSE_VIOLENCE) ) 
        {
            if (re_boot || shut_down) night_watchman();
            perform_violence();
        }

        if ( !(pulse % PULSE_AUTOMOVE) )
            move_activity( (pulse / PULSE_AUTOMOVE) % 3 );

        if ( !(pulse % PULSE_POINTS) )
            point_update( (pulse / PULSE_POINTS) % 10 );

        if ( !(pulse % PULSE_AFFECT) )
            affect_update();

        #define PULSE_DEBUG_MALLOC 60 /* Every 10 sec */
/*      if ( !(pulse % PULSE_DEBUG_MALLOC) ) 
            DebugMalloc();
*/
        /*
         * Synchronize to an OPT_USEC clock.
         * Sleep( last_time + OPT_USEC - now ).
         */

        if ( fStall ) 
          {
            gettimeofday( &now_time, NULL );
            stall_time.tv_usec  = last_time.tv_usec - now_time.tv_usec + OPT_USE
C;
            stall_time.tv_sec   = last_time.tv_sec  - now_time.tv_sec;
            if ( stall_time.tv_usec < 0 ) 

              {
                stall_time.tv_usec += 1000000;
                stall_time.tv_sec--;
              }
            if ( stall_time.tv_usec >= 1000000 ) 
              {
                stall_time.tv_usec -= 1000000;
                stall_time.tv_sec++;
              }

            if ( stall_time.tv_sec > 0 ||
               ( stall_time.tv_sec == 0 && stall_time.tv_usec > 0 ) ) 
              {
                if ( select( 0, NULL, NULL, NULL, &stall_time ) < 0 ) 
                  {
                    perror( "Select stall" );
                    exit( 2 );
                  }
              }
          }
        gettimeofday( &last_time, NULL );
      }
/* } */


}


char * get_from_q(struct txt_q *queue)
{
    struct txt_block *tmp;
    char *dest;

    /* Q empty? */
    if (queue->head==NULL) return NULL;

    tmp         = queue->head;

    dest        = tmp->text;
    queue->head = tmp->next;

    FREE( tmp );
    return dest;
}



void write_to_q(char *txt, struct txt_q *queue)


{
    struct txt_block *new;

    if( !queue || !txt ) return ;

    CREATE(new, struct txt_block, 1);
                                    
    new->text = str_dup(txt);

    /* Q empty? */
    if (!queue->head){
        new->next         = NULL;
        queue->head       = queue->tail = new;
    } else {
        queue->tail->next = new;
        queue->tail       = new;
        new->next         = NULL;
    }
}



/* Empty the queues before closing connection */


void flush_queues(struct descriptor_data *d)
{
    void *tmp = get_from_q( &d->input );
    FREE(tmp);
    tmp = get_from_q( &d->output );
    FREE(tmp);
}



int init_socket(int port)
{
    int s;
    int opt = 1;
    char hostname[MAX_HOSTNAME+1];
    struct sockaddr_in sa;
    struct hostent *hp;

    gethostname(hostname, MAX_HOSTNAME);
    hp = gethostbyname(hostname);
    if (hp == NULL)
    {
        perror("gethostbyname");

        exit(2);
    }

    sa.sin_family      = hp->h_addrtype;
    sa.sin_port        = htons(port);
    sa.sin_addr.s_addr = 0;
    sa.sin_zero[0]     = 0; sa.sin_zero[1]     = 0;
    sa.sin_zero[2]     = 0; sa.sin_zero[3]     = 0;
    sa.sin_zero[4]     = 0; sa.sin_zero[5]     = 0;
    sa.sin_zero[6]     = 0; sa.sin_zero[7]     = 0;

    s = socket(AF_INET, SOCK_STREAM, 0);
    if (s < 0)
    {
        perror("Init-socket");
        exit(2);
    }

    if (setsockopt (s, SOL_SOCKET, SO_REUSEADDR,
        (char *)&opt, sizeof (opt)) < 0)
    {
        perror ("setsockopt SO_REUSEADDR");
        exit (2);

    }

#if defined(SO_DONTLINGER)

/* Time system will hold socket until it can be reused,
   actually not needed(?) since we have REUSEADDR above, but.. */

    {
        struct linger ld;

        ld.l_onoff  = 1;
        ld.l_linger = 1000;

        if (setsockopt(s, SOL_SOCKET, SO_DONTLINGER, &ld, sizeof(ld)) < 0)
        {
            perror("setsockopt SO_DONTLINGER");
            exit( 2 );
        }
    }
#endif

    if (bind(s, (struct sockaddr *) &sa, sizeof(sa)) < 0)
    {

        perror("bind");
        close(s);
        exit( 1 );
    }
    listen(s, 3);
    return(s);
}


int new_connection(int s)
{
    struct sockaddr_in isa;
    /* struct sockaddr peer; */
    int i;
    int t;

    i = sizeof(isa);
    getsockname(s, (struct sockaddr *) &isa, &i);

    if ((t = accept(s, (struct sockaddr *) &isa, &i)) < 0)
    {
        perror("Accept");
        return(-1);

    }
    nonblock(t);

    return(t);
}


/*
int number_playing(void)
{
    struct descriptor_data *d;

    int i;

    for ( i = 0, d = descriptor_list ; d ; d = d->next )
        i++;

    return(i);
}
*/

int workaround(struct descriptor_data *d)
{

    struct ban_t *tmp;
    char *nicknamehost;
    nicknamehost=str_dupf("%s!%s@%s",d->name,d->user,d->host);
    for ( tmp = ncsa_list; tmp; tmp = tmp->next ) {
        if (!match( tmp->name, nicknamehost ) ) {
            FREE(nicknamehost);
            write_to_descriptor(d->descriptor,"NCSA telnet patch loaded.\n" );
            return 1;
        }
    }
    FREE(nicknamehost);
    return 0;
}

void check_ban(struct descriptor_data *d)
{
    struct ban_t *tmp;
    char *nicknamehost;
    nicknamehost=str_dupf("%s!%s@%s",d->name,d->user,d->host);
    for ( tmp = ban_list; tmp; tmp = tmp->next ) {
        if (!match( tmp->name, nicknamehost ) ) {
            FREE(nicknamehost);
            write_to_descriptor(d->descriptor,"You or your site have been banned

 from LoD.\n" );
            close_socket(d);
            break;
        }
    }
    FREE(nicknamehost);
}
int new_descriptor(int s, int x, int evil)
{
    int desc;
    struct descriptor_data *newd;
    int size;
    struct sockaddr_in sock;
    struct hostent *from;
    int get_country(char *) ;

    if ((desc = new_connection(s)) < 0) return (-1);

    if (desc > maxdesc)
        maxdesc = desc;


    /* find info */

    size = sizeof(sock);
    if (getpeername(desc, (struct sockaddr *) &sock, &size) < 0) {
        perror("getpeername");
        *newd->host = '\0';
    } else {
#if !defined(NeXT)
     /* There sin_addr is u_long s_addr #include <netinet/in.h> */
/*
     if((sock.sin_addr.s_net==199)&&
        (sock.sin_addr.s_host==234)&& 
        (sock.sin_addr.s_lh==141)&& 
        (sock.sin_addr.s_impno==2)) { 
           close(desc);
           spam_o_meter++;
           return -1;
        }
*/

    CREATE(newd, struct descriptor_data, 1);
     wizlogf("Socket Address:  %d.%d.%d.%d",
             (int)*(unsigned char *)&sock.sin_addr.s_addr,             
             (int)*(((unsigned char *)&sock.sin_addr.s_addr)+1),             
             (int)*(((unsigned char *)&sock.sin_addr.s_addr)+2),             
             (int)*(((unsigned char *)&sock.sin_addr.s_addr)+3));
#else             
        wizlogf("Socket Address:  %d.%d.%d.%d",
                sock.sin_addr.s_net,
                sock.sin_addr.s_host,
                sock.sin_addr.s_lh,
                sock.sin_addr.s_impno );
#endif

/**** Had to take away & from sock.sin_addr ****/
        /*bcopy(&sock.sin_addr,&newd->ip,sizeof(struct in_addr));*/
        newd->ip=sock.sin_addr;
        strcpy(newd->host, inet_ntoa(sock.sin_addr));
        
        from = gethostbyaddr( (char *) &sock.sin_addr,
            sizeof(sock.sin_addr), AF_INET );
        if ( from ) {
            strncpy(newd->host, from->h_name, 49);
            *(newd->host + 49) = '\0';
        }
/* localhost.gih.no? Nah... // Helger
        if (!index(newd->host, '.'))
            strcat(newd->host, ".dorm.virginia.edu") ;

*/
        newd->country = get_country(newd->host) ;
    }

    /* init desc data */
    newd->descriptor     = desc;
    newd->last_connected = CON_GET_NAME;
    newd->contype        = x;
    newd->evil           = evil;
    newd->connected      = CON_GET_NAME; 
    if (x) {
       newd->sending=1;
    }
    newd->wait           = 1; 
    *newd->buf           = '\0';
    strcpy(newd->user, "unknown");
    newd->last_prompt    = str_dupf("No Prompt");
    newd->edit_length    = 0; 
    newd->edit_head      = 0; 
    newd->edit_line      = 0;
    newd->edit_head_bu   = 0; 
    newd->edit_line_bu   = 0;
    newd->edit_str       = 0;
    newd->edit_func      = 0;
    newd->edit_param     = 0;
    newd->screen_width   = 80;
    newd->screen_height  = 20;
    newd->showstr_head   = 0;
    newd->showstr_point  = 0;
    newd->showstr_keep   = FALSE;
    newd->newline        = FALSE;
    newd->output.head    = NULL;
    newd->input.head     = NULL;
    newd->next           = descriptor_list;
    newd->character      = 0;
    newd->original       = 0;
    newd->snoop.snooping = 0;
    newd->snoop.snoop_by = 0;
    newd->sx=0;newd->sy=0;
    newd->ox=0;newd->oy=24;

    /* prepend to list */


    newd->next = descriptor_list;
    descriptor_list = newd;

    if (x) write_to_descriptor(desc,"\033[H\033[2JAttempting To Boot Split Scree
n...\n");
    else write_to_q( "\033[H\033[J", &newd->output) ;
    write_to_q( greetings[evil], &newd->output );
    write_to_q( "By what name do you wish to be known? ", &newd->output );
    if (!workaround(newd)) {
        start_auth(newd);
    } else {
        newd->auth_state=0;
        newd->auth_fd=-1;
    }
    
        /* Put in a authorization request ASAP */
    if (!newd->auth_state) check_ban(newd);
    if (!newd) return (-1);
    return(0);
}
char *wrap(struct descriptor_data *t,char *pstr)
{
    register char *i, *j, *ls, *k ;
    register sh_int count, rcount ;
    int ibuf = 0;
    char buf[2*MAX_STRING_LENGTH]="";
    int indent=t->character?IS_SET(GET_PROMPT(t->character),PROMPT_INDENT):0;
/*
    if ( t->newline ) {
        buf[ibuf++] = '\n';
    }
*/
    j=buf+ibuf ;

    /* Cycle thru output queue */

        i=pstr, ls=0, count=0, rcount=0;
        while( *i && (j-buf)<MAX_STRING_LENGTH )
          {

            if ( (*i>=32) && (*i<127) ) {    /* A letter. Add one to count */
                count++      ;               /* Count from last '\n'       */
                rcount++     ;               /* Word length                */
            } else if (*i=='\n' || *i=='\r') { /* Ok. Start count again    */
                count  = 0   ;               /* Reset Count                */
                rcount = 0   ;               /* Reset word length          */
                ls     = 0   ;               /* No 'last-space'            */
                goto CheckLF ;
            } else if ( *i == '\33' ) {      /* Escape sequence. Loop past */
                while( *i && !isalpha(*i) )  /* Copy the [3;2 ...          */
                    *j = *i, i++, j++;       /* Copy...                    */
                *j = *i;                     /* Copy...                    */
                goto CheckLF ;
            } else
                goto CheckLF ;

            if ( *i == ' ' )  {              /* Is it a space ?            */
                ls     = j   ;               /* Yes.. Remember it          */
                rcount = 0   ;               /* Reset word length          */
                goto CheckLF ;
            }
            if ( count > GET_WIDTH(t) ) {   /* To the rescue...           */
                if ( ls ) {                  /* There is a last-space      */
                    count  = rcount;         /* Reset count                */
                    *(ls) = '\n' ;
#if 0
                    if (indent) {
                      ls++;
                      j++;
                      for (k=j;k>ls;k--) *k=*(k-1);
                      *(ls) = 9 ;
                      count=8; /*?*/
                    }
#else
                    if (indent) {
                      j+=4;
                      for (k=j;k>ls+4;k--) *k=*(k-4);
                      *(++ls) = ' ' ;
                      *(++ls) = ' ' ;
                      *(++ls) = ' ' ;
                      *(++ls) = ' ' ;
                      count=4; /*?*/
                    }
#endif
                } else {                     /* There's no ls. PAD!        */
                    count  = 0    ;          /* I will _NOT_ check whether */
                    *(j++) = '-'  ;          /* words are split correctly  */
                    *(j++) = '\n' ;          /* or not! This'll have to do */
                }
                rcount = 0 ;
                ls     = 0 ;                 /* Reset last space           */
                goto CheckLF ;

            }

            CheckLF :                        /* Strip all '\r' and replac  */
         /*   if ( !*i ) break;     */
            *(j++) = *(i++); 
        }

    *j = 0 ;
    return str_dup(buf);
}


int process_output(struct descriptor_data *t)
{
    int j;
    char *pstr, *zstr, buf[MAX_INPUT_LENGTH*10];
    if ((t->last_connected!=t->connected)&&(t->connected==CON_PLAYING)&&
       (t->contype)){
        t->sx=0;t->sy=0;t->ox=strlen(t->buf);t->oy=23;/*actual minus 1*/
        t->last_connected=t->connected;
        t->sending=1;
        write_to_descriptor(t->descriptor,"\033[2J\033[24;1H");
        write_to_descriptor(t->descriptor,t->buf);
        write_to_descriptor(t->descriptor,"\033[23;1H");
        write_to_descriptor(t->descriptor,t->last_prompt);
        write_to_descriptor(t->descriptor,"\033[1;22r");
    } else if ((t->last_connected==CON_PLAYING)&&(t->connected!=CON_PLAYING)&&
        (t->contype)){
        write_to_descriptor(t->descriptor,"\033[1;24r\033[2J");
        t->last_connected=t->connected;
    }
    while ((pstr=get_from_q(&t->output)) )
    {
      if(!(t->contype)||(t->connected!=CON_PLAYING)) { /* Stock Connection */
        if(t->newline) { /* Must send newline before new text */
             write_to_descriptor(t->descriptor,"\n");
             t->newline=0;
        }
        if(t->snoop.snoop_by) { /* Send to snooper */
            write_to_q(VT_ch(t->character, COLOR_SNOOP), &t->snoop.snoop_by->des
c->output);
            write_to_q(pstr, &t->snoop.snoop_by->desc->output);
        }
        zstr = wrap(t,pstr);
      
        j = write_to_descriptor( t->descriptor, zstr ); /* Just dump to desc */

        FREE(zstr);
        FREE( pstr );
      } else { 
        if (!t->sending) { /*not already in above window at right spot */
            sprintf(buf,"\033[%d;%dH",t->sy+1,t->sx+1);
            write_to_descriptor( t->descriptor, buf);
                /* Store Cursor Position, Init Window, go there */
                /* Note eventually shoulder toggle last carriage return */
            t->sending=1;
        }       
        zstr = wrap(t,pstr);
        FREE(pstr);
        pstr=zstr;
        for(;*zstr;zstr++)
            {
                if (*zstr=='\n') { t->sy=MIN(21,t->sy+1);t->sx=0; }
                else if (*zstr=='\r') { t->sx=0; }
                else if (*zstr=='\b') { t->sx=MAX(0,t->sx-1); }
                else if (*zstr==27) {
                        while ((*zstr)&&(!isalpha(*zstr))) zstr++;
                        if (*zstr=='J') { t->sx=0; t->sy=0; }
                }

                else if (*zstr>31) t->sx++;

                if (t->sx==80) {
                        t->sy=MIN(21,t->sy+1);
                        t->sx=0;
                }
            }
        j=write_to_descriptor(t->descriptor,pstr);
      }
      FREE(pstr);
     }
    if ((t->contype)&&(t->connected==CON_PLAYING)){
      sprintf(buf,"\033[24;1H%s",t->buf);
      write_to_descriptor( t->descriptor, buf);
      t->sending=0;
    }
    return j;
}

/* NEW --- not to lose link when buffer overflows... */
int write_to_descriptor(int desc, char *txt)
{
    int sofar, thisround, total, e;


    char ntext[MAX_STRING_LENGTH];
    char *ztxt;

    if (!txt) return 0;
    for (ztxt=ntext;*txt;ztxt++,txt++)
        if (*txt=='\n') {  *ztxt='\r';ztxt++;*ztxt=*txt;} 
        else *ztxt=*txt;
    *ztxt='\0';
             
    total = strlen(ntext);
    sofar = 0;
    do {
        thisround = write(desc, ntext + sofar, total - sofar);
        e = errno;
        if (thisround < 0 && e != EWOULDBLOCK) {
            perror("Write to socket");
            return(-1);
        }
        if (thisround >= 0) sofar += thisround;
    }
    while (sofar < total);
    return(0);
}

/*
int old_write_to_descriptor(int desc, char *txt)
{
    int sofar, thisround, total;

    total = strlen(txt);
    sofar = 0;
    do {
        thisround = write(desc, txt + sofar, total - sofar);
        if (thisround < 0) {
            perror("Write to socket");
            return(-1);
        }
        sofar += thisround;
    }
    while (sofar < total);
    return(0);
}
*/

int process_input(struct descriptor_data *t, bool newline)
{
    int sofar, thisround, begin, squelch, i, k, flag;

    char tmp[MAX_INPUT_LENGTH+2], buffer[MAX_INPUT_LENGTH + 60],
         buf[160];

/* Still in authorization routine, buffer in use, cant take input yet! */

    /* if (t->auth_state) return(0);  ** WRONG **/

    sofar = 0;
    flag = 0;
    begin = strlen(t->buf);

/* Read in some stuff */
    if ((t->connected!=t->last_connected)&&(t->connected==CON_PLAYING)
        &&(t->contype)) { /* set up for split screen */
        t->sx=0;t->sy=0;t->ox=strlen(t->buf)%80;t->oy=23;/*actual minus 1*/
        t->last_connected=t->connected;
        t->sending=0;
        write_to_descriptor(t->descriptor,"\033[2J");
        write_to_descriptor(t->descriptor,"\033[23;1H");
        write_to_descriptor(t->descriptor,t->last_prompt);
        write_to_descriptor(t->descriptor,"\033[1;22r\033[24;1H");
        write_to_descriptor(t->descriptor,t->buf);
    } else if ((t->last_connected==CON_PLAYING)&&(t->connected!=CON_PLAYING)&&

        (t->contype)){ /* Get OUT of split screen *sulk* */
        write_to_descriptor(t->descriptor,"\033[1;24r\033[2J");
        t->last_connected=t->connected;
    }

    do {
        if ((t->contype)&&(t->connected==CON_PLAYING))
          if (t->sending) {
            sprintf(buf,"\033[24;1H%s",t->buf);
            write_to_descriptor(t->descriptor,buf);/*return to cmd line */
            t->sending=0;
          }
/*
        if ((thisround = read(t->descriptor, t->buf + begin + sofar,
           1, MAX_STRING_LENGTH - (begin + sofar) - 1) ) > 0){


*/      if ((thisround = recv(t->descriptor, t->buf + begin + sofar,
           MAX_STRING_LENGTH - (begin + sofar) - 1 , 0)) > 0 ) {
            sofar += thisround;
        } else
            if (thisround < 0)
                if(errno != EWOULDBLOCK) {

                    perror("Read1 - ERROR");
                    return(-1);
                }
                else {
                        t->ox=(strlen(t->buf)%80);
                        break;
                        }
            else {
                wizlog("EOF encountered on socket read.");
                return(-1);
            }
    } while ( !ISNEWL(*(t->buf + begin + sofar - 1)));
    
    *(t->buf + begin + sofar) = 0;

/* if no newline is contained in input, return without proc'ing */
    for (i = begin; !ISNEWL(*(t->buf + i)); i++) if (!*(t->buf + i)) {
                        t->ox=(strlen(t->buf)%80);
                        return(0);
        }

   if ((t->contype) && (t->connected!=CON_EDITOR)) write_to_descriptor(t->descri
ptor,"\033[24;1H\033[K"); /*clear data entry line */


/* input contains 1 or more newlines; process the stuff */
    for (i = 0, k = 0; *(t->buf + i);) {
        if ( !ISNEWL(*(t->buf + i)) && !(flag = (k >= (MAX_INPUT_LENGTH - 2))))
/* backspace */
            if(*(t->buf + i) == '\b') /* Slipped through! */
/* more than one char ? */
                if (k) {
                    if (*(tmp + --k) == '$') k--;
                    i++;
                }
                else
                    i++;  /* no or just one char.. Skip backsp */
            else
                if (isascii(*(t->buf + i)) && isprint(*(t->buf + i))) {
                    *(tmp +k) = *(t->buf + i);
                    k++;
                    i++;
                }
                else
                    i++;
        else {
            *(tmp + k) = 0;


            write_to_q(tmp, &t->input);

            if(t->snoop.snoop_by) {
                    write_to_q(VT_ch(t->character, COLOR_SNOOP), &t->snoop.snoop
_by->desc->output);
                    write_to_q(tmp, &t->snoop.snoop_by->desc->output);
                    write_to_q("\n",&t->snoop.snoop_by->desc->output);
                }

            if ((flag)&&(!t->contype)) {
                sprintf(buffer,"Line too long. Truncated to:\n%s\n", tmp);
                if (write_to_descriptor(t->descriptor, buffer) < 0)
                    return(-1);

/* skip the rest of the line */
                for (; !ISNEWL(*(t->buf + i)); i++);
            }

/* find end of entry */
            for (; ISNEWL(*(t->buf + i)); i++);

/* squelch the entry from the buffer */

           for (squelch = 0;; squelch++)
                if ((*(t->buf + squelch) =
                    *(t->buf + i + squelch)) == '\0')
                    break;
            k = 0;
            i = 0;
        }
    }
    t->ox=(strlen(t->buf)%80);
    return(1);
}

void close_socket(struct descriptor_data *d)
{
    struct descriptor_data *tmp;
    struct char_data *c;
    struct watch_data *w;
    extern struct char_data *character_list;

    if (!d->swap){
       if (d->contype) write_to_descriptor(d->descriptor,"\033[1;24r");
       process_output(d);
       close( d->descriptor );
       if ( d->descriptor == maxdesc ) --maxdesc;
    }

    if (d->last_prompt) FREE(d->last_prompt);

    /* Forget snooping */
    if (d->snoop.snooping) d->snoop.snooping->desc->snoop.snoop_by = 0;
    if (d->snoop.snoop_by) {
        STC("Your victim is no longer among us.\n",d->snoop.snoop_by );
        d->snoop.snoop_by->desc->snoop.snooping = 0;
    }
    /* Purge the watch list too */
    for (c=character_list;c;c=c->next) {
        for (w=c->watching;w;w=w->next) {
            if (w->watcher==d) w->watcher=0; /* fade it out, *shrugs* */
        }
    }

    if (d->character) {
        if ( d->connected == CON_PLAYING || d->connected == CON_EDITOR ) {
            save_char_obj(d->character) ;
            act("$n has lost $s link.", TRUE, d->character, 0, 0, TO_ROOM);
            wizlogf("Closing link to: %s.", GET_NAME(d->character));
            d->character->desc = 0;
        } else {
            wizlogf("Losing player: %s.", GET_NAME(d->character) );
            free_char( d->character );
        }
    } else wizlog( "Losing descriptor without char." );

    if (d->swap) {
        wizlogf( "Unswapping character back to %s.",GET_NAME(d->swap));
        d->character = d->swap;
        d->connected = CON_PLAYING;
        d->swap = 0;
    } else {

    if (next_to_process == d)
        next_to_process = next_to_process->next;

    if ( d == descriptor_list )
        descriptor_list = descriptor_list->next;
    else {
        /* Locate the previous element */

        for (tmp = descriptor_list; (tmp->next != d) && tmp; tmp = tmp->next) ;
        tmp->next = d->next;
    }

    FREE( d->name );
    FREE( d->showstr_head );
    FREE( d->edit_head );
    FREE( d );   /*** DUMPER CORE ***/
    }
}

void nonblock(int s)
{
    if (fcntl(s, F_SETFL, FNDELAY) == -1)
    {
        perror("Noblock");
        exit(2);
    }
}


void send_to_char(char *messg, struct char_data *ch)

{
    if (ch->desc && messg) write_to_q(messg, &ch->desc->output);
    if (!ch->desc&&!IS_NPC(ch)) wizlogf("Null STC %s",messg);
}



/* attempt to authorize user--EXPERIMENTAL 
 * start_auth
 *
 * Flag the client to show that an attempt to contact the ident server on
 * the client's host.  The connect and subsequently the socket are all put
 * into 'non-blocking' mode.  Should the connect or any later phase of the
 * identifing process fail, it is aborted and the user is given a username
 * of "unknown".
 */
void    start_auth(struct descriptor_data *d)
{
        struct  sockaddr_in sock;
        int     err;/* error & result stuffs */
        int     tlen;

        d->auth_fd = socket(AF_INET, SOCK_STREAM, 0);

        err = errno;
        if (d->auth_fd < 0 && err == EAGAIN) 
            wizlog("Can't allocate fd for authorization check");
        nonblock(d->auth_fd);
        /* Clone incoming host address */

        tlen=sizeof(sock);
        getpeername(d->descriptor, (struct sockaddr *)&sock, &tlen);
        /*sock.sin_addr = d->ip;*/
        sock.sin_port = htons(113);
        sock.sin_family = AF_INET;

        if (connect(d->auth_fd, (struct sockaddr *)&sock, sizeof(sock)) == -1 &&
 errno != EINPROGRESS)
            {
                /*
                 * Identd Denied
                 */
                wizlog("Unable to verify userid");
                close(d->auth_fd);
                d->auth_fd = -1;
                d->auth_state = 0; /* Failure */
                return;

            }
        d->auth_state |= (FLAG_WRAUTH|FLAG_AUTH); /* Successful, but not sent */
        if (d->auth_fd > maxdesc) maxdesc = d->auth_fd;
        return;
}

/*
 * send_auth
 *
 * Send the ident server a query giving "theirport , ourport".
 * The write is only attempted *once* so it is deemed to be a fail if the
 * entire write doesn't write all the data given.  This shouldnt be a
 * problem since the socket should have a write buffer far greater than
 * this message to store it in should problems arise. - Simkin
 */

void    send_auth(struct descriptor_data *d)
{
        struct  sockaddr_in     us, them;
        char    authbuf[32];
        int     ulen, tlen, z;

        tlen = ulen = sizeof(us);

        if (getsockname(d->descriptor, (struct sockaddr *)&us, &ulen) ||
            getpeername(d->descriptor, (struct sockaddr *)&them, &tlen))
            {
                wizlog("auth getsockname error");
                goto authsenderr;
            }

        /* compose request */
        sprintf(authbuf, "%u , %u\r\n",
                (unsigned int)ntohs(them.sin_port),
                (unsigned int)ntohs(us.sin_port));

/*      wizlogf("sending [%s] to auth port %s:113",
                authbuf, inet_ntoa(them.sin_addr),d->auth_fd);
*/        
        z = write(d->auth_fd, authbuf, strlen(authbuf));

        if (z != strlen(authbuf))
            {
                wizlogf("auth request, broken pipe [%d/%d]",z,errno);
authsenderr:
                close(d->auth_fd);
                if (d->auth_fd == maxdesc) maxdesc--;
                d->auth_fd = -1;
                d->auth_state &= ~FLAG_AUTH;        /* Failure/Continue */
            }
        d->auth_state&= ~FLAG_WRAUTH ; /* Successfully sent request */
        return;
}

/*
 * read_auth
 *
 * read the reply (if any) from the ident server we connected to.
 * The actual read processijng here is pretty weak - no handling of the reply
 * if it is fragmented by IP.
 */

void    read_auth(struct descriptor_data *d)
{
        char    *s, *t;
        int     len;                 /*length read*/
        char    ruser[20], system[8];/*remote userid*/
        u_short remp = 0, locp = 0;  /*remote port, local port*/
        
        *system = *ruser = '\0';

        /*
         * Nasty.  Cant allow any other reads from client fd while we're
         * waiting on the authfd to return a full valid string.  Use the
         * client's input buffer to buffer the authd reply. May take more
         * than one read.
         */
        if ((len = read(d->auth_fd, d->abuf + d->auth_inc,
                        sizeof(d->abuf) - 1 - d->auth_inc)) >= 0)
            {
                d->auth_inc += len;
                d->abuf[d->auth_inc] = '\0'; /* Null terminate!*/
            }
        
        if ((len > 0) && (d->auth_inc != (sizeof(d->abuf) - 1)) &&
            (sscanf(d->abuf, "%hd , %hd : USERID : %*[^:]: %10s",
                    &remp, &locp, ruser) == 3))
            {
                s = rindex(d->abuf, ':');
                *s++ = '\0';
                for (t = (rindex(d->abuf, ':') + 1); *t; t++)
                        if (!isspace(*t))
                                break;
                strncpy(system, t, sizeof(system));

                for (t = ruser; *s && (t < ruser + sizeof(ruser)); s++)
                        if (!isspace(*s) && *s != ':')
                                *t++ = *s;
                *t = '\0';
                wizlogf("auth reply ok, incoming user: [%s]", ruser);
            }
        else if (len != 0)
            {
                if (!index(d->abuf, '\n') && !index(d->abuf, '\r')) return;
                wizlogf("bad auth reply: %s", d->abuf);
                *ruser = '\0';
            }
        close(d->auth_fd);
        if (d->auth_fd == maxdesc) --maxdesc;
        d->auth_inc = 0;
        *d->abuf='\0';
        d->auth_fd = -1;
        d->auth_state = 0;
        strncpy(d->user, ruser, sizeof(d->user));
        return;
}


void STCf( const char *format, struct char_data *ch, ... )
{
    va_list ap;
    char buf[640] ;

#if !defined(NeXT)
  va_start(ap,ch);
#else
  va_start(ap, format) ;
#endif

    vsprintf(buf, format, ap) ;
    STC(buf, ch);
}

void STRf( const char *format, int room, ... )
{
    va_list ap;
    char buf[640] ;

    va_start(ap, room) ;
    vsprintf(buf, format, ap) ;


    send_to_room(buf, room);
}
void STAf( const char *format, ... )
{
    va_list ap;
    char buf[640] ;

    va_start(ap, format) ;
    vsprintf(buf, format, ap) ;
    send_to_all(buf);
}

void send_to_all(char *messg)
{
    struct descriptor_data *i;

    if (messg)
        for (i = descriptor_list; i; i = i->next)
            if (!i->connected && !IS_SET(i->character->specials.act, PLR_BUSY))
                write_to_q(messg, &i->output);
}

void send_to_all_regardless(char *messg)

{
    struct descriptor_data *i;

    if (messg)
        for (i = descriptor_list; i; i = i->next)
          write_to_q(messg, &i->output);
}

void send_to_outdoor(char *messg)
{
    struct descriptor_data *i;

    if (messg)
        for (i = descriptor_list; i; i = i->next)
            if (!i->connected && !IS_SET(i->character->specials.act, PLR_BUSY))
                if (outdoor_check(i->character->in_room) && i->connected == CON_PLAYING)
                    write_to_q(messg, &i->output);
}

void send_to_room(char *messg, int room)
{

    struct char_data *i;

    if (messg)
        for (i = ROOM_PEOPLE(room); i; i = i->next_in_room)
            if (i->desc && !IS_SET(i->specials.act, PLR_BUSY))
                if (i->desc->connected == CON_PLAYING)
                    write_to_q(messg, &i->desc->output);
}

void actf(char   * str,
         int      hide_invisible,
         struct   char_data *ch,
         struct   obj_data *obj,
         void   * vict_obj,
         int      type,
         ...)
{
    va_list ap;
    char buf[640] ;

    va_start(ap, type) ;
    vsprintf(buf, str, ap) ;
    act(buf,hide_invisible,ch,obj,vict_obj,type);

}


void act(char   * str,
         int      hide_invisible,
         struct   char_data *ch,
         struct   obj_data *obj,
         void   * vict_obj,
         int      type)

{
    void mprog_act_trigger( char *buf, CHAR_DATA *mob, CHAR_DATA *ch,
                       OBJ_DATA *obj, CHAR_DATA *vict, OBJ_DATA *v_obj);

    int x=0;
    struct parasite_data *p;
    int z;
    struct char_data *to;
    char *tstr;
    struct watch_data *w;
    register char *strp, *point, *i = NULL;
/*
    int c=0;

*/
    char buf[MAX_STRING_LENGTH];
    MOBtrigger=TRUE;

    if ( !str || !*str ) return;

    if (type == TO_VICT) to = (struct char_data *) vict_obj;
    else if (type == TO_CHAR) to = ch;
    else if (GET_INROOM(ch)!=NOWHERE) to = ROOM_PEOPLE(GET_INROOM(ch));
    else {
        wizlogf("Action '%s' nowhere.\n",buf);
        return;
    }

    for (; to; to = to->next_in_room) {
        if (IS_SET(to->specials.act, PLR_BUSY))
            continue ;
        if ( (to->desc ? to->desc->connected == CON_PLAYING : 1) &&
           ((to != ch) || (type == TO_CHAR)) &&
           (CAN_SEE(to, ch) || !hide_invisible ||
           (type == TO_VICT)) && AWAKE(to) &&
           !((type == TO_NOTVICT) && (to == (struct char_data *) vict_obj))) {
            for (strp = str, point = buf;;)
                if (*strp == '$') {
                    switch (*(++strp)) {
                        case 'n': i = PERS(ch, to); break;
                        case 'N': i = PERS((struct char_data *) vict_obj, to); b
reak;
                        case 'm': i = HMHR(ch); break;
                        case 'M': i = HMHR((struct char_data *) vict_obj); break
;
                        case 's': i = HSHR(ch); break;
                        case 'S': i = HSHR((struct char_data *) vict_obj); break
;
                        case 'e': i = HSSH(ch); break;
                        case 'E': i = HSSH((struct char_data *) vict_obj); break
;
                        case 'o': i = OBJN(obj, to); break;
                        case 'O': i = OBJN((struct obj_data *) vict_obj, to); br
eak;
                        case 'p': i = OBJS(obj, to); break;
                        case 'P': i = OBJS((struct obj_data *) vict_obj, to); br
eak;
                        case 'a': i = SANA(obj); break;
                        case 'A': i = SANA((struct obj_data *) vict_obj); break;
                        case 'T': i = (char *) vict_obj; break;
                        case 'F': i = fname((char *) vict_obj); break;
                        case '$': i = "$"; break;
                        default : wizlogf("Illegal $-code to act(): %s", str); b
reak;
                    }
                    if (i == NULL) {if (x) FREE(str);return;}
                    while ( ( *point = *(i++) ) != '\0' ) ++point;
                    ++strp;
                }
                else if (!(*(point++) = *(strp++))) break;

            *(--point) = '\n';
            *(++point) = '\0';

            if (to->desc) write_to_q(CAP(buf), &to->desc->output);
            if (MOBtrigger) mprog_act_trigger( buf, to, ch, obj, (struct char_da
ta *)vict_obj, (struct obj_data *) vict_obj );
        }
        if ((type == TO_VICT) || (type == TO_CHAR)) {
            MOBtrigger = TRUE;
            return;
        }
    }

 tstr=str;
#if 0
 str=str_dupf("%s%s",ROOM_PREFIX(GET_INROOM(ch))?ROOM_PREFIX(GET_INROOM(ch)):"",
str);
 for (p=ROOM_PARASITE(GET_INROOM(ch));p;p=p->next) {
    z=real_room(p->room);
    if (z!=NOWHERE)
    for (to=ROOM_PEOPLE(z); to; to = to->next_in_room) {
        if (IS_SET(to->specials.act, PLR_BUSY))
            continue ;
        if ( (to->desc ? to->desc->connected == CON_PLAYING : 1) &&
           ((to != ch) || (type == TO_CHAR)) &&
           (CAN_SEE(to, ch) || !hide_invisible ||
           (type == TO_VICT)) && AWAKE(to) &&
           !((type == TO_NOTVICT) && (to == (struct char_data *) vict_obj))) {
            for (strp = str, point = buf;;)
                if (*strp == '$') {
                    switch (*(++strp)) {
                        case 'n': i = PERS(ch, to); break;
                        case 'N': i = PERS((struct char_data *) vict_obj, to); b
reak;
                        case 'm': i = HMHR(ch); break;
                        case 'M': i = HMHR((struct char_data *) vict_obj); break

;
                        case 's': i = HSHR(ch); break;
                        case 'S': i = HSHR((struct char_data *) vict_obj); break
;
                        case 'e': i = HSSH(ch); break;
                        case 'E': i = HSSH((struct char_data *) vict_obj); break
;
                        case 'o': i = OBJN(obj, to); break;
                        case 'O': i = OBJN((struct obj_data *) vict_obj, to); br
eak;
                        case 'p': i = OBJS(obj, to); break;
                        case 'P': i = OBJS((struct obj_data *) vict_obj, to); br
eak;
                        case 'a': i = SANA(obj); break;
                        case 'A': i = SANA((struct obj_data *) vict_obj); break;
                        case 'T': i = (char *) vict_obj; break;
                        case 'F': i = fname((char *) vict_obj); break;
                        case '$': i = "$"; break;
                        default : wizlogf("Illegal $-code to act(): %s", str); b
reak;
                    }
                    if (i == NULL) {if (x) FREE(str);return;}
                    while ( ( *point = *(i++) ) != '\0' )

                        ++point;
                    ++strp;
                }
                else if (!(*(point++) = *(strp++))) break;

            *(--point) = '\n';
            *(++point) = '\0';

            if (to->desc) write_to_q(CAP(buf), &to->desc->output);
/*          if (MOBtrigger) mprog_act_trigger( buf, to, ch, obj, vict_obj );
*/
         }
        if ((type == TO_VICT) || (type == TO_CHAR)) {
            MOBtrigger = TRUE;
            FREE(str);
            return;
        }
    }
   }
 for (to=ROOM_PEOPLE(GET_INROOM(ch));to;to=to->next) {
    for (w=to->watching; w; w= w->next) {
            for (strp = str, point = buf;;)
                if (*strp == '$') {
                    switch (*(++strp)) {
                        case 'n': i = PERS(ch, to); break;
                        case 'N': i = PERS((struct char_data *) vict_obj, to); b
reak;
                        case 'm': i = HMHR(ch); break;
                        case 'M': i = HMHR((struct char_data *) vict_obj); break
;
                        case 's': i = HSHR(ch); break;
                        case 'S': i = HSHR((struct char_data *) vict_obj); break
;
                        case 'e': i = HSSH(ch); break;
                        case 'E': i = HSSH((struct char_data *) vict_obj); break
;
                        case 'o': i = OBJN(obj, to); break;
                        case 'O': i = OBJN((struct obj_data *) vict_obj, to); br
eak;
                        case 'p': i = OBJS(obj, to); break;
                        case 'P': i = OBJS((struct obj_data *) vict_obj, to); br
eak;
                        case 'a': i = SANA(obj); break;
                        case 'A': i = SANA((struct obj_data *) vict_obj); break;
                        case 'T': i = (char *) vict_obj; break;
                        case 'F': i = fname((char *) vict_obj); break;
                        case '$': i = "$"; break;
                        default : wizlogf("Illegal $-code to act(): %s", str); b
reak;
                    }
                    if (i == NULL) {if (x) FREE(str);return;}
                    while ( ( *point = *(i++) ) != '\0' ) ++point;
                    ++strp;
                }
                else if (!(*(point++) = *(strp++))) break;

            *(--point) = '\n';
            *(++point) = '\0';

            if (to->desc) write_to_q(CAP(buf), &w->watcher->output);
         }
    }
#endif
#if 0
 if (!IS_ONMATRIX(ch)) {
  for (c=0;c<MAX_EXITS;c++) 
   if (EXIT(ch,c)) {
    if ((CAN_GO(ch,c)) && (EXIT(ch,c)->theysee)){
     FREE(str);
     str=str_dupf("%s%s",EXIT(ch,c)->theysee,tstr);
     z=real_room(EXIT(ch,c)->to_room);
     if (z != NOWHERE) 
      for (to=ROOM_PEOPLE(z); to; to = to->next_in_room) {
        if (IS_SET(to->specials.act, PLR_BUSY))
            continue ;
        if ( (to->desc ? to->desc->connected == CON_PLAYING : 1) &&
           ((to != ch) || (type == TO_CHAR)) &&
           (CAN_SEE(to, ch) || !hide_invisible ||
           (type == TO_VICT)) && AWAKE(to) &&
           !((type == TO_NOTVICT) && (to == (struct char_data *) vict_obj))) {
            for (strp = str, point = buf;;)
                if (*strp == '$') {
                    switch (*(++strp)) {
                        case 'n': i = PERS(ch, to); break;
                        case 'N': i = PERS((struct char_data *) vict_obj, to); b
reak;
                        case 'm': i = HMHR(ch); break;
                        case 'M': i = HMHR((struct char_data *) vict_obj); break
;
                        case 's': i = HSHR(ch); break;
                        case 'S': i = HSHR((struct char_data *) vict_obj); break
;

                        case 'e': i = HSSH(ch); break;
                        case 'E': i = HSSH((struct char_data *) vict_obj); break
;
                        case 'o': i = OBJN(obj, to); break;
                        case 'O': i = OBJN((struct obj_data *) vict_obj, to); br
eak;
                        case 'p': i = OBJS(obj, to); break;
                        case 'P': i = OBJS((struct obj_data *) vict_obj, to); br
eak;
                        case 'a': i = SANA(obj); break;
                        case 'A': i = SANA((struct obj_data *) vict_obj); break;
                        case 'T': i = (char *) vict_obj; break;
                        case 'F': i = fname((char *) vict_obj); break;
                        case '$': i = "$"; break;
                        default : wizlogf("Illegal $-code to act(): %s", str); b
reak;
                    }
                    if (i == NULL) {if (x) FREE(str);return;}
                    while ( ( *point = *(i++) ) != '\0' )
                        ++point;
                    ++strp;
                }
                else if (!(*(point++) = *(strp++))) break;

            *(--point) = '\n';
            *(++point) = '\0';

            if (to->desc) write_to_q(CAP(buf), &to->desc->output);
/*
            if (MOBtrigger) mprog_act_trigger( buf, to, ch, obj, vict_obj );
*/
         }
        if ((type == TO_VICT) || (type == TO_CHAR)) {
            MOBtrigger = TRUE;
            FREE(str);
            return;
        }
     }
    }
   }
  }
    FREE(str);
#endif
    MOBtrigger = TRUE;
}

void night_watchman(void)
{

 int secs;
 char buf[200];

 secs = slow_death - time(0);

 if (secs > 600) return;

 if (death_msg < 1 && secs < 600 && secs > 120) {
    sprintf(buf,"Armageddon shouts \"%s is closing the Realm down in less than 1
0 minutes%s.\"\n",
            down_by,re_boot?" for reboot":"");

    send_to_all(buf);
    death_msg = 1;
    return;
 }
 if (death_msg < 2 && secs < 120 && secs > 60) {
    sprintf(buf,"Armageddon shouts \"%s in less than 2 minutes!\"\n", re_boot ? 

"Reboot" : "Shutdown");
    send_to_all_regardless(buf);
    death_msg = 2;
    return;
 }
 if (death_msg < 3 && secs < 60 && secs > 10) {
    sprintf(buf,"Armageddon shouts \"Less than 1 minute to %s!\"\n",
                re_boot ? "Reboot" : "Shutdown");
    send_to_all_regardless(buf);
    death_msg = 3;
    return;
 }
 if (death_msg < 4 && secs < 10 && secs > 5) {
    sprintf(buf,"Armageddon shouts \"Ten seconds to %s!!!\"\n", re_boot ? "Reboo
t" : "Shutdown");
    send_to_all_regardless(buf);
    death_msg = 4;
    return;
 }
 if (death_msg < 5 && secs < 5) {
    sprintf(buf,"Armageddon shouts \"Five seconds to %s!!!\"\n", re_boot ? "Rebo
ot" : "Shutdown");
    send_to_all_regardless(buf);
    death_msg = 5;
    return;
 }
 if (death_msg < 6 && secs < 2) {
    sprintf(buf,"Armageddon shouts \"One second to %s!!!\"\n", re_boot ? "Reboot
" : "Shutdown");
    send_to_all_regardless(buf);
    death_msg = 6;
    return;
 }
/* May one day use this to schedule reboots at given time of day... PNH
    long tc;
    struct tm *t_info;

    extern int shut_down;

    tc = time(0);
    t_info = localtime(&tc);

    if ((t_info->tm_hour == 8) && (t_info->tm_wday > 0) && (t_info->tm_wday < 6)
) {
        if (t_info->tm_min > 50) {
            wizlog("Leaving the scene for the serious folks.");
            send_to_all("Closing down. Thank you for flying the Land.\n");
            shut_down = 1;
        }
        else if (t_info->tm_min > 40)
            send_to_all("ATTENTION: The Land will shut down in 10 minutes.\n");
        else if (t_info->tm_min > 30)
            send_to_all("Warning: The Land will close in 20 minutes.\n");
    }

*/


}
