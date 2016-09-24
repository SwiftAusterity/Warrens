namespace NetMud.Authentication.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class authAsp3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Accounts", "GlobalIdentityHandle", "dbo.AspNetUsers");
            DropIndex("dbo.Accounts", new[] { "GlobalIdentityHandle" });
            AlterColumn("dbo.AspNetUsers", "GlobalIdentityHandle", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.AspNetUsers", "GlobalIdentityHandle");
            AddForeignKey("dbo.AspNetUsers", "GlobalIdentityHandle", "dbo.Accounts", "GlobalIdentityHandle", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "GlobalIdentityHandle", "dbo.Accounts");
            DropIndex("dbo.AspNetUsers", new[] { "GlobalIdentityHandle" });
            AlterColumn("dbo.AspNetUsers", "GlobalIdentityHandle", c => c.String());
            CreateIndex("dbo.Accounts", "GlobalIdentityHandle");
            AddForeignKey("dbo.Accounts", "GlobalIdentityHandle", "dbo.AspNetUsers", "Id");
        }
    }
}
