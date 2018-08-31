namespace WebSphere.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Events_upd : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Events", "Value", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Events", "Value", c => c.Int(nullable: false));
        }
    }
}
