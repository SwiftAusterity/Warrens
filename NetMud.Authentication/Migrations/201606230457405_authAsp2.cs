namespace NetMud.Authentication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class authAsp2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "GlobalIdentityHandle", c => c.String());
            DropColumn("dbo.AspNetUsers", "GameAccountId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "GameAccountId", c => c.String());
            DropColumn("dbo.AspNetUsers", "GlobalIdentityHandle");
        }
    }
}
