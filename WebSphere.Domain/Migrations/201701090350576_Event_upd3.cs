namespace WebSphere.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Event_upd3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "Value", c => c.Single(nullable: false));
            DropColumn("dbo.Events", "Message");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Events", "Message", c => c.String(nullable: false));
            DropColumn("dbo.Events", "Value");
        }
    }
}
