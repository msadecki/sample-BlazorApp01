using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp01.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddEventStoreModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    OutboxMessageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.OutboxMessageId);
                });

            migrationBuilder.CreateTable(
                name: "StoredEvents",
                columns: table => new
                {
                    StoredEventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AggregateId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    StoredAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CausationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredEvents", x => x.StoredEventId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "StoredEvents");
        }
    }
}
