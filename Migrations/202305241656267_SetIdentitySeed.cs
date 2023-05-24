namespace ePaperLive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SetIdentitySeed : DbMigration
    {
        public override void Up()
        {
            Sql("DBCC CHECKIDENT ('dbo.school_govt_rates', RESEED, 1000)");
        }
        
        public override void Down()
        {
        }
    }
}
