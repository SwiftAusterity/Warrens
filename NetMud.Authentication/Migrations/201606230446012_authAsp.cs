namespace NetMud.Authentication.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class authAsp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "GameAccountId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "GameAccountId");
        }
    }
}
