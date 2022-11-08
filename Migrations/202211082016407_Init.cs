namespace ePaperLive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.printandsubrates",
                c => new
                    {
                        Rateid = c.Int(nullable: false, identity: true),
                        Market = c.String(maxLength: 25),
                        Type = c.String(maxLength: 25),
                        RateDescr = c.String(),
                        PrintDayPattern = c.String(maxLength: 10),
                        PrintTerm = c.Int(),
                        PrintTermUnit = c.String(maxLength: 25),
                        EDayPattern = c.String(maxLength: 10),
                        ETerm = c.Int(),
                        ETermUnit = c.String(maxLength: 25),
                        Curr = c.String(maxLength: 10),
                        Rate = c.Double(),
                        SortOrder = c.Int(),
                        Active = c.Boolean(),
                    })
                .PrimaryKey(t => t.Rateid);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Subscriber_Address",
                c => new
                    {
                        AddressID = c.Int(nullable: false, identity: true),
                        SubscriberID = c.String(maxLength: 128),
                        AddressType = c.String(maxLength: 5),
                        EmailAddress = c.String(maxLength: 50),
                        AddressLine1 = c.String(maxLength: 100),
                        AddressLine2 = c.String(maxLength: 100),
                        CityTown = c.String(maxLength: 50),
                        StateParish = c.String(maxLength: 50),
                        ZipCode = c.String(maxLength: 10),
                        CountryCode = c.String(maxLength: 50),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AddressID)
                .ForeignKey("dbo.Subscribers", t => t.SubscriberID)
                .Index(t => t.SubscriberID);
            
            CreateTable(
                "dbo.Subscribers",
                c => new
                    {
                        SubscriberID = c.String(nullable: false, maxLength: 128),
                        Id = c.Int(nullable: false, identity: true),
                        EmailAddress = c.String(maxLength: 50),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        DateOfBirth = c.DateTime(),
                        Secretquestion = c.String(maxLength: 50),
                        Secretans = c.String(maxLength: 50),
                        IpAddress = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        AddressID = c.Int(),
                        Newsletter = c.Boolean(),
                        CreatedAt = c.DateTime(nullable: false),
                        Token = c.String(),
                        CCHashID = c.Int(),
                        LastLogin = c.DateTime(),
                    })
                .PrimaryKey(t => t.SubscriberID)
                .ForeignKey("dbo.AspNetUsers", t => t.SubscriberID)
                .Index(t => t.SubscriberID)
                .Index(t => t.Id, unique: true);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Subscriber_Epaper",
                c => new
                    {
                        Subscriber_EpaperID = c.Int(nullable: false, identity: true),
                        SubscriberID = c.String(maxLength: 128),
                        EmailAddress = c.String(maxLength: 50),
                        RateID = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        SubType = c.String(maxLength: 50),
                        CreatedAt = c.DateTime(nullable: false),
                        NotificationEmail = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Subscriber_EpaperID)
                .ForeignKey("dbo.Subscribers", t => t.SubscriberID)
                .Index(t => t.SubscriberID);
            
            CreateTable(
                "dbo.Subscriber_Print",
                c => new
                    {
                        Subscriber_PrintID = c.Int(nullable: false, identity: true),
                        SubscriberID = c.String(maxLength: 128),
                        EmailAddress = c.String(maxLength: 50),
                        RateID = c.Int(nullable: false),
                        AddressID = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DeliveryInstructions = c.String(),
                        Circprosubid = c.String(maxLength: 50),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Subscriber_PrintID)
                .ForeignKey("dbo.Subscribers", t => t.SubscriberID)
                .Index(t => t.SubscriberID);
            
            CreateTable(
                "dbo.Subscriber_Tranx",
                c => new
                    {
                        Subscriber_TranxID = c.Int(nullable: false, identity: true),
                        SubscriberID = c.String(maxLength: 128),
                        EmailAddress = c.String(maxLength: 50),
                        CardOwner = c.String(maxLength: 50),
                        CardType = c.String(maxLength: 20),
                        CardExp = c.String(maxLength: 5),
                        CardLastFour = c.String(maxLength: 5),
                        TranxAmount = c.Double(),
                        TranxDate = c.DateTime(),
                        RateID = c.Int(),
                        TranxType = c.String(maxLength: 50),
                        OrderID = c.String(maxLength: 50),
                        TranxNotes = c.String(maxLength: 100),
                        IpAddress = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Subscriber_TranxID)
                .ForeignKey("dbo.Subscribers", t => t.SubscriberID)
                .Index(t => t.SubscriberID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Subscriber_Tranx", "SubscriberID", "dbo.Subscribers");
            DropForeignKey("dbo.Subscriber_Print", "SubscriberID", "dbo.Subscribers");
            DropForeignKey("dbo.Subscriber_Epaper", "SubscriberID", "dbo.Subscribers");
            DropForeignKey("dbo.Subscriber_Address", "SubscriberID", "dbo.Subscribers");
            DropForeignKey("dbo.Subscribers", "SubscriberID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.Subscriber_Tranx", new[] { "SubscriberID" });
            DropIndex("dbo.Subscriber_Print", new[] { "SubscriberID" });
            DropIndex("dbo.Subscriber_Epaper", new[] { "SubscriberID" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Subscribers", new[] { "Id" });
            DropIndex("dbo.Subscribers", new[] { "SubscriberID" });
            DropIndex("dbo.Subscriber_Address", new[] { "SubscriberID" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.Subscriber_Tranx");
            DropTable("dbo.Subscriber_Print");
            DropTable("dbo.Subscriber_Epaper");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Subscribers");
            DropTable("dbo.Subscriber_Address");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.printandsubrates");
        }
    }
}
