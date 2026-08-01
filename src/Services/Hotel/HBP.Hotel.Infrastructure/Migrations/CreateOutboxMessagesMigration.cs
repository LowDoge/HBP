using FluentMigrator;

namespace HBP.Hotel.Infrastructure.Migrations;

[Migration(0002, "Create outbox_messages table for transactional outbox")]
public sealed class CreateOutboxMessagesMigration : Migration
{
    public override void Up()
    {
        Create
            .Table("outbox_messages")
            .WithColumn("id")
            .AsGuid()
            .PrimaryKey()
            .WithColumn("topic")
            .AsString(512)
            .NotNullable()
            .WithColumn("key")
            .AsString(512)
            .Nullable()
            .WithColumn("created_at")
            .AsDateTimeOffset()
            .NotNullable()
            .WithColumn("type")
            .AsString(512)
            .NotNullable()
            .WithColumn("payload")
            .AsCustom("jsonb")
            .NotNullable()
            .WithColumn("processed_at")
            .AsDateTimeOffset()
            .Nullable()
            .WithColumn("retry_count")
            .AsInt32()
            .NotNullable()
            .WithDefaultValue(0)
            .WithColumn("error")
            .AsString(int.MaxValue)
            .Nullable();

        Create
            .Table("outbox_dead_letter_messages")
            .WithColumn("id")
            .AsGuid()
            .PrimaryKey()
            .WithColumn("topic")
            .AsString(512)
            .NotNullable()
            .WithColumn("key")
            .AsString(512)
            .Nullable()
            .WithColumn("type")
            .AsString(512)
            .NotNullable()
            .WithColumn("payload")
            .AsCustom("jsonb")
            .NotNullable()
            .WithColumn("retry_count")
            .AsInt32()
            .NotNullable()
            .WithColumn("error")
            .AsString(int.MaxValue)
            .Nullable()
            .WithColumn("dead_lettered_at")
            .AsDateTimeOffset()
            .NotNullable();

        Execute.Sql(
            """
            CREATE INDEX ix_outbox_unprocessed
                ON outbox_messages (created_at)
                WHERE processed_at IS NULL;
            """
        );
    }

    public override void Down()
    {
        Delete.Table("dead_letter_messages");
        Delete.Table("outbox_messages");
    }
}
