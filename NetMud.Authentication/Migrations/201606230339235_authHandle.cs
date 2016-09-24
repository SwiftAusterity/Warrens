namespace NetMud.Authentication.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class authHandle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "GlobalIdentityHandle", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "GlobalIdentityHandle");
        }
    }
}
