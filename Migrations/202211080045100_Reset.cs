namespace ePaperLive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Reset : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.printandsubrates",
                c => new
                    {
                        Rateid = c.Int(nullable: false, identity: true),
                        Market = c.String(),
                        Type = c.String(),
                        RateDescr = c.String(),
                        PrintDayPattern = c.String(),
                        PrintTerm = c.Int(),
                        PrintTermUnit = c.String(),
                        EDayPattern = c.String(),
                        ETerm = c.Int(),
                        ETermUnit = c.String(),
                        Curr = c.String(),
                        Rate = c.Double(),
                        SortOrder = c.Int(),
                        Active = c.Int(),
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
                        SubscriberID = c.String(nullable: false, maxLength: 128),
                        AddressType = c.String(),
                        EmailAddress = c.String(),
                        AddressLine1 = c.String(),
                        AddressLine2 = c.String(),
                        CityTown = c.String(),
                        StateParish = c.String(),
                        ZipCode = c.String(),
                        CountryCode = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AddressID)
                .ForeignKey("dbo.Subscribers", t => t.SubscriberID, cascadeDelete: true)
                .Index(t => t.SubscriberID);
            
            CreateTable(
                "dbo.Subscribers",
                c => new
                    {
                        SubscriberID = c.String(nullable: false, maxLength: 128),
                        Id = c.Int(nullable: false, identity: true),
                        EmailAddress = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        DateOfBirth = c.DateTime(),
                        Secretquestion = c.String(),
                        Secretans = c.String(),
                        IpAddress = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        AddressID = c.Int(),
                        Newsletter = c.Boolean(),
                        CreatedAt = c.DateTime(nullable: false),
                        RoleID = c.Int(),
                        Token = c.String(),
                        CCHashID = c.Int(),
                        LastLogin = c.DateTime(),
                        Subscriber_Roles_Subscriber_RolesID = c.Int(),
                    })
                .PrimaryKey(t => t.SubscriberID)
                .ForeignKey("dbo.AspNetUsers", t => t.SubscriberID)
                .ForeignKey("dbo.Subscriber_Roles", t => t.Subscriber_Roles_Subscriber_RolesID)
                .Index(t => t.SubscriberID)
                .Index(t => t.Id, unique: true)
                .Index(t => t.Subscriber_Roles_Subscriber_RolesID);
            
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
                        SubscriberID = c.String(nullable: false, maxLength: 128),
                        EmailAddress = c.String(),
                        RateID = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        SubType = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        NotificationEmail = c.String(),
                    })
                .PrimaryKey(t => t.Subscriber_EpaperID)
                .ForeignKey("dbo.Subscribers", t => t.SubscriberID, cascadeDelete: true)
                .Index(t => t.SubscriberID);
            
            CreateTable(
                "dbo.Subscriber_Print",
                c => new
                    {
                        Subscriber_PrintID = c.Int(nullable: false, identity: true),
                        SubscriberID = c.String(nullable: false, maxLength: 128),
                        EmailAddress = c.String(),
                        RateID = c.Int(nullable: false),
                        AddressID = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DeliveryInstructions = c.String(),
                        Circprosubid = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Subscriber_PrintID)
                .ForeignKey("dbo.Subscribers", t => t.SubscriberID, cascadeDelete: true)
                .Index(t => t.SubscriberID);
            
            CreateTable(
                "dbo.Subscriber_Roles",
                c => new
                    {
                        Subscriber_RolesID = c.Int(nullable: false, identity: true),
                        RoleDescription = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Subscriber_RolesID);
            
            CreateTable(
                "dbo.Subscriber_Tranx",
                c => new
                    {
                        Subscriber_TranxID = c.Int(nullable: false, identity: true),
                        SubscriberID = c.String(nullable: false, maxLength: 128),
                        EmailAddress = c.String(),
                        CardOwner = c.String(),
                        CardType = c.String(),
                        CardExp = c.String(),
                        CardLastFour = c.String(),
                        TranxAmount = c.Double(),
                        TranxDate = c.DateTime(),
                        RateID = c.Int(),
                        TranxType = c.String(),
                        OrderID = c.String(),
                        TranxNotes = c.String(),
                        IpAddress = c.String(),
                    })
                .PrimaryKey(t => t.Subscriber_TranxID)
                .ForeignKey("dbo.Subscribers", t => t.SubscriberID, cascadeDelete: true)
                .Index(t => t.SubscriberID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Subscriber_Address", "SubscriberID", "dbo.Subscribers");
            DropForeignKey("dbo.Subscriber_Tranx", "SubscriberID", "dbo.Subscribers");
            DropForeignKey("dbo.Subscribers", "Subscriber_Roles_Subscriber_RolesID", "dbo.Subscriber_Roles");
            DropForeignKey("dbo.Subscriber_Print", "SubscriberID", "dbo.Subscribers");
            DropForeignKey("dbo.Subscriber_Epaper", "SubscriberID", "dbo.Subscribers");
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
            DropIndex("dbo.Subscribers", new[] { "Subscriber_Roles_Subscriber_RolesID" });
            DropIndex("dbo.Subscribers", new[] { "Id" });
            DropIndex("dbo.Subscribers", new[] { "SubscriberID" });
            DropIndex("dbo.Subscriber_Address", new[] { "SubscriberID" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.Subscriber_Tranx");
            DropTable("dbo.Subscriber_Roles");
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
