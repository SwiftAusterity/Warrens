namespace NetMud.Authentication.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class one : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "GameAccount_GlobalIdentityHandle");
            DropColumn("dbo.AspNetUsers", "GameAccount_CurrentlySelectedCharacter");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "GameAccount_CurrentlySelectedCharacter", c => c.Long(nullable: false));
            AddColumn("dbo.AspNetUsers", "GameAccount_GlobalIdentityHandle", c => c.String());
        }
    }
}
