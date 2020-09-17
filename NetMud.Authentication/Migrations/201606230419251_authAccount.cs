namespace NetMud.Authentication.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class authAccount : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                {
                    GlobalIdentityHandle = c.String(nullable: false, maxLength: 128),
                    CurrentlySelectedCharacter = c.Long(nullable: false),
                })
                .PrimaryKey(t => t.GlobalIdentityHandle)
                .ForeignKey("dbo.AspNetUsers", t => t.GlobalIdentityHandle)
                .Index(t => t.GlobalIdentityHandle);

        }

        public override void Down()
        {
            DropForeignKey("dbo.Accounts", "GlobalIdentityHandle", "dbo.AspNetUsers");
            DropIndex("dbo.Accounts", new[] { "GlobalIdentityHandle" });
            DropTable("dbo.Accounts");
        }
    }
}
