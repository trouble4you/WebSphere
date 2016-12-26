namespace WebSphere.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NEW : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Alarms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TagId = c.Int(nullable: false),
                        SRes = c.Int(nullable: false),
                        STime = c.DateTime(nullable: false),
                        SVal = c.Single(nullable: false),
                        ERes = c.Int(),
                        ETime = c.DateTime(),
                        EVal = c.Single(),
                        AckTime = c.DateTime(),
                        Ack = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Objects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Type = c.Int(nullable: false),
                        ParentId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ObjectTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Properties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ObjectId = c.Int(nullable: false),
                        PropId = c.Int(nullable: false),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PropTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SignalsAnalogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TagId = c.Int(),
                        Value = c.Single(),
                        Datetime = c.DateTime(),
                        RegTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SignalsDiscretes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TagId = c.Int(),
                        Value = c.Boolean(),
                        Datetime = c.DateTime(),
                        RegTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SignalsDiscretes");
            DropTable("dbo.SignalsAnalogs");
            DropTable("dbo.PropTypes");
            DropTable("dbo.Properties");
            DropTable("dbo.ObjectTypes");
            DropTable("dbo.Objects");
            DropTable("dbo.Alarms");
        }
    }
}
