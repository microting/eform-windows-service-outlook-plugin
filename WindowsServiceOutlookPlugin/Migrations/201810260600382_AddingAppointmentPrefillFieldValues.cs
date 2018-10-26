namespace OutlookSql.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingAppointmentPrefillFieldValues : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.appointment_prefill_field_value_versions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        appointment_id = c.Int(),
                        microting_site_uid = c.Int(nullable: false),
                        workflow_state = c.String(maxLength: 255),
                        version = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        field_id = c.Int(nullable: false),
                        field_value = c.String(),
                        appointment_fv_id = c.Int(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.appointment_prefill_field_values",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        appointment_id = c.Int(),
                        workflow_state = c.String(maxLength: 255),
                        version = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        field_id = c.Int(nullable: false),
                        field_value = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.appointments", t => t.appointment_id)
                .Index(t => t.appointment_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.appointment_prefill_field_values", "appointment_id", "dbo.appointments");
            DropIndex("dbo.appointment_prefill_field_values", new[] { "appointment_id" });
            DropTable("dbo.appointment_prefill_field_values");
            DropTable("dbo.appointment_prefill_field_value_versions");
        }
    }
}
