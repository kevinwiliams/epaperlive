namespace ePaperLive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Subscribers", new[] { "ApplicationUser_Id" });
            RenameColumn(table: "dbo.AspNetUsers", name: "ApplicationUser_Id", newName: "Subscriber_SubscriberID");
            CreateIndex("dbo.AspNetUsers", "Subscriber_SubscriberID");
            DropColumn("dbo.Subscribers", "ApplicationUser_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Subscribers", "ApplicationUser_Id", c => c.String(nullable: false, maxLength: 128));
            DropIndex("dbo.AspNetUsers", new[] { "Subscriber_SubscriberID" });
            RenameColumn(table: "dbo.AspNetUsers", name: "Subscriber_SubscriberID", newName: "ApplicationUser_Id");
            CreateIndex("dbo.Subscribers", "ApplicationUser_Id");
        }
    }
}
