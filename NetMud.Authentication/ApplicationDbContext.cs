using NetMud.Data.Players;
using System.Data.Entity;

namespace NetMud.Authentication
{

    /// <summary>
    /// DB Context for the user manager
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// New up the db context
        /// </summary>
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        /// <summary>
        /// Get a new db context for the user manager
        /// </summary>
        /// <returns>the db context</returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasRequired(u => u.GameAccount);
        }

        public DbSet<Account> GameAccount { get; set; }
    }
}