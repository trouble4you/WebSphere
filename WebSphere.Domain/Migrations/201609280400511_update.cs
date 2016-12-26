namespace WebSphere.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SignalsAnalogs", "TagId", c => c.Int(nullable: false));
            AlterColumn("dbo.SignalsAnalogs", "Value", c => c.Single(nullable: false));
            AlterColumn("dbo.SignalsAnalogs", "Datetime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.SignalsAnalogs", "RegTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.SignalsDiscretes", "TagId", c => c.Int(nullable: false));
            AlterColumn("dbo.SignalsDiscretes", "Value", c => c.Boolean(nullable: false));
            AlterColumn("dbo.SignalsDiscretes", "Datetime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.SignalsDiscretes", "RegTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SignalsDiscretes", "RegTime", c => c.DateTime());
            AlterColumn("dbo.SignalsDiscretes", "Datetime", c => c.DateTime());
            AlterColumn("dbo.SignalsDiscretes", "Value", c => c.Boolean());
            AlterColumn("dbo.SignalsDiscretes", "TagId", c => c.Int());
            AlterColumn("dbo.SignalsAnalogs", "RegTime", c => c.DateTime());
            AlterColumn("dbo.SignalsAnalogs", "Datetime", c => c.DateTime());
            AlterColumn("dbo.SignalsAnalogs", "Value", c => c.Single());
            AlterColumn("dbo.SignalsAnalogs", "TagId", c => c.Int());
        }
    }
}
