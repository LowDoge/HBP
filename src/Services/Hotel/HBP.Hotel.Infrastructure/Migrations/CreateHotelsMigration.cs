using System.Data;
using FluentMigrator;

namespace HBP.Hotel.Infrastructure.Migrations;

[Migration(0001, "Create hotels and rooms tables")]
public sealed class CreateHotelsMigration : Migration
{
    public override void Up()
    {
        Create
            .Table("hotels")
            .WithColumn("id")
            .AsGuid()
            .PrimaryKey()
            .WithColumn("name")
            .AsString(255)
            .NotNullable()
            .WithColumn("country")
            .AsString(100)
            .NotNullable()
            .WithColumn("city")
            .AsString(100)
            .NotNullable()
            .WithColumn("street")
            .AsString(255)
            .NotNullable()
            .WithColumn("postal_code")
            .AsString(20)
            .Nullable()
            .WithColumn("created_at")
            .AsDateTimeOffset()
            .NotNullable()
            .WithDefault(SystemMethods.CurrentDateTimeOffset)
            .WithColumn("updated_at")
            .AsDateTimeOffset()
            .NotNullable()
            .WithDefault(SystemMethods.CurrentDateTimeOffset);

        Create
            .Table("rooms")
            .WithColumn("id")
            .AsGuid()
            .PrimaryKey()
            .WithColumn("hotel_id")
            .AsGuid()
            .NotNullable()
            .ForeignKey("hotels", "id")
            .OnDelete(Rule.Cascade)
            .WithColumn("type")
            .AsString(20)
            .NotNullable()
            .WithColumn("capacity")
            .AsInt32()
            .NotNullable()
            .WithColumn("price_per_night")
            .AsDecimal(12, 2)
            .NotNullable()
            .WithColumn("currency")
            .AsString(3)
            .NotNullable()
            .WithColumn("status")
            .AsString(20)
            .NotNullable()
            .WithDefaultValue("active");

        Create.Index("ix_hotels_name").OnTable("hotels").OnColumn("name");
        Create.Index("ix_rooms_hotel_id").OnTable("rooms").OnColumn("hotel_id");
    }

    public override void Down()
    {
        Delete.Table("rooms");
        Delete.Table("hotels");
    }
}
