namespace NetMud.Authentication.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class auth2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "GlobalIdentityHandle");
        }

        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "GlobalIdentityHandle", c => c.String());
        }
    }
}
