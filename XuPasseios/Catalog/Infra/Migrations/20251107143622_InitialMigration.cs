using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Catalog.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProductDescription = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ProductWeight = table.Column<string>(type: "TEXT", nullable: false),
                    ProductHeight = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProductWidth = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProductDepth = table.Column<decimal>(type: "TEXT", nullable: false),
                    Sku = table.Column<decimal>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    DateInclusion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProductActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "DateInclusion", "Price", "ProductActive", "ProductDepth", "ProductDescription", "ProductHeight", "ProductName", "ProductWeight", "ProductWidth", "Sku" },
                values: new object[,]
                {
                    { new Guid("102b566b-ba1f-404c-b2df-e2cde39ade09"), new DateTime(2025, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Happiness when you laugh about some silly thing", 1m, "Smile", "1ton", 1m, 1m },
                    { new Guid("164a2e03-bd51-4e0f-9e53-b7c181b00104"), new DateTime(2025, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Baffled moments of life when you don't know what to say", 1m, "Baffled moment 2", "1ton", 1m, 1m },
                    { new Guid("17f181fa-d2c7-4a1a-9848-163958e8b771"), new DateTime(2025, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "A good memory that has been lost", 1m, "Memories 3", "1ton", 1m, 1m },
                    { new Guid("20c1bfd6-1241-4a33-befd-d6392e23f5d6"), new DateTime(2025, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "That feeling once experienced but you can't find it anywhere at all", 1m, "Good Feeling 3", "1ton", 1m, 1m },
                    { new Guid("2902b665-1190-4c70-9915-b9c2d7680450"), new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "The first night that you've spent awake without any special reason and you regret it doing so", 1m, "Night Sleep", "1ton", 1m, 1m },
                    { new Guid("2aadd2df-7caf-45ab-9355-7f6332985a87"), new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "A small piece of childhood when there weren't any worried", 1m, "Childhood", "1ton", 1m, 1m },
                    { new Guid("2ee49fe3-edf2-4f91-8409-3eb25ce6ca51"), new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Magic artifact that grant many power to its carrier", 1m, "Unicorn Horn", "1ton", 1m, 1m },
                    { new Guid("34489196-0766-48bb-9918-f36e6ac81da8"), new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "That feeling of hope you've missed during you long walk until this moment in your life", 1m, "Lost Hope 2", "1ton", 1m, 1m },
                    { new Guid("375f3d27-caec-4416-a4e6-6d9e5685815e"), new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "That feeling of hope you've missed during you long walk until this moment in your life", 1m, "Lost Hope 3", "1ton", 1m, 1m },
                    { new Guid("42ed050d-37ec-4c1f-9bff-84990ac3adda"), new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "The first night that you've spent awake without any special reason and you regret it doing so", 1m, "Night Sleep 2", "1ton", 1m, 1m },
                    { new Guid("498f0ef6-e542-434b-8277-992f770e9118"), new DateTime(2025, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Happiness when you laugh about some silly thing", 1m, "Smile 2", "1ton", 1m, 1m },
                    { new Guid("5b1c2b4d-48c7-402a-80c3-cc796ad49c6b"), new DateTime(2025, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "A good memory that has been lost", 1m, "Memories", "1ton", 1m, 1m },
                    { new Guid("5b3621c0-7b12-4e80-9c8b-3398cba7ee05"), new DateTime(2025, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Baffled moments of life when you don't know what to say", 1m, "Baffled moment", "1ton", 1m, 1m },
                    { new Guid("73e1b95e-2754-40de-947e-c1cb4f1e7f67"), new DateTime(2025, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "That feeling once experienced but you can't find it anywhere at all", 1m, "Good Feeling 2", "1ton", 1m, 1m },
                    { new Guid("7e3f11f6-3493-4799-8d24-fc5c7ea31f6a"), new DateTime(2025, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Happiness when you laugh about some silly thing", 1m, "Smile 3", "1ton", 1m, 1m },
                    { new Guid("94635dc2-0cc8-4ab5-b8c0-dd01ab4d30f4"), new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "A small piece of childhood when there weren't any worried", 1m, "Childhood 3", "1ton", 1m, 1m },
                    { new Guid("be056cb4-fb6f-40a6-81ec-4b896072dee6"), new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "That feeling of hope you've missed during you long walk until this moment in your life", 1m, "Lost Hope", "1ton", 1m, 1m },
                    { new Guid("c8705360-1fc4-4a1a-8380-77c7a2d18b6e"), new DateTime(2025, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Rain drops during a sleepy night at a cozy home", 1m, "Rain Drops 2", "1ton", 1m, 1m },
                    { new Guid("d8663e5e-7494-4f81-8739-6e0de1bea7ee"), new DateTime(2025, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Rain drops during a sleepy night at a cozy home", 1m, "Rain Drops", "1ton", 1m, 1m },
                    { new Guid("da2fd609-d754-4feb-8acd-c4f9ff13ba96"), new DateTime(2025, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "That feeling once experienced but you can't find it anywhere at all", 1m, "Good Feeling", "1ton", 1m, 1m },
                    { new Guid("dae0cedb-e1bd-40ee-a6cd-c32732446114"), new DateTime(2025, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Baffled moments of life when you don't know what to say", 1m, "Baffled moment 3", "1ton", 1m, 1m },
                    { new Guid("e3b711c8-9e11-4b6d-bdd9-1cf4eb40bade"), new DateTime(2025, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Rain drops during a sleepy night at a cozy home", 1m, "Rain Drops 3", "1ton", 1m, 1m },
                    { new Guid("e6af373b-c907-4c48-968e-3879a234892b"), new DateTime(2025, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "The first night that you've spent awake without any special reason and you regret it doing so", 1m, "Night Sleep 3", "1ton", 1m, 1m },
                    { new Guid("e96bcc8a-4696-4cd9-a86c-32c8421146f7"), new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Magic artifact that grant many power to its carrier", 1m, "Unicorn Horn 2", "1ton", 1m, 1m },
                    { new Guid("ee215607-f2c7-4e48-8df6-3442d00bdbc7"), new DateTime(2025, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "A good memory that has been lost", 1m, "Memories 2", "1ton", 1m, 1m },
                    { new Guid("ef9a2698-a31e-416e-ac92-f2ca02b5a9d3"), new DateTime(2025, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "A small piece of childhood when there weren't any worried", 1m, "Childhood 2", "1ton", 1m, 1m },
                    { new Guid("f93923a5-3d34-4df0-be32-4237dda10d41"), new DateTime(2025, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 1m, false, 1m, "Magic artifact that grant many power to its carrier", 1m, "Unicorn Horn 3", "1ton", 1m, 1m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
