#include <sys/types.h>

#define F_ULOCK 0       /* Unlock a previously locked region.  */
#define F_LOCK  1       /* Lock a region for exclusive use.  */
#define F_TLOCK 2       /* Test and lock a region for exclusive use. */
#define F_TEST  3       /* Test a region for other processes locks. */

extern int lockf __P ((int __fd, int __cmd, __off_t __len));
