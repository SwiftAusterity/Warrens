namespace NetMud.Authentication.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class authLinkedAccount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Accounts", "LogSubs", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.Accounts", "LogSubs");
        }
    }
}
