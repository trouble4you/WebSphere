namespace WebSphere.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Events_upd2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "Message", c => c.String(nullable: false));
            DropColumn("dbo.Events", "Value");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Events", "Value", c => c.Double(nullable: false));
            DropColumn("dbo.Events", "Message");
        }
    }
}
