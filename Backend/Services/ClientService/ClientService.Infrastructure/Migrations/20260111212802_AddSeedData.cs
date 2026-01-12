using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClientService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsActive", "Name", "NormalizedName", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("019923ff-5bd5-7622-b805-c53c2525ba21"), null, null, true, "user", "USER", null, null });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Avatar", "CreatedAt", "CreatedBy", "Email", "GoogleId", "IsActive", "Name", "Password", "RoleId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("3538ed8c-b05d-4b58-80ff-2868aba45701"), null, new DateTime(2026, 1, 10, 18, 46, 7, 462, DateTimeKind.Utc), "System", "user@example.com", null, true, "Nguyen Van A", "$2a$12$hgon6fKIh8fKF9NgjefgX.rrDTSyigpvPQ9ZD5hExZJjqXrKbkqAW", new Guid("019923ff-5bd5-7622-b805-c53c2525ba21"), new DateTime(2026, 1, 10, 18, 46, 7, 463, DateTimeKind.Utc), "System" });

            migrationBuilder.InsertData(
                table: "forms",
                columns: new[] { "id", "created_at", "created_by", "is_active", "is_published", "settings", "slug", "theme_config", "title", "updated_at", "updated_by", "user_id" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System", true, true, null, "sample-form", null, "Sample Form", new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System", new Guid("3538ed8c-b05d-4b58-80ff-2868aba45701") });

            migrationBuilder.InsertData(
                table: "fields",
                columns: new[] { "id", "created_at", "created_by", "description", "form_id", "is_active", "is_required", "\"order\"", "properties", "title", "type", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System", "Please enter your full name", new Guid("11111111-1111-1111-1111-111111111111"), true, true, 1, null, "Full Name", "Text", new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System" },
                    { new Guid("22222222-2222-2222-2222-222222222223"), new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System", "Please enter your email", new Guid("11111111-1111-1111-1111-111111111111"), true, true, 2, null, "Email Address", "Email", new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System" }
                });

            migrationBuilder.InsertData(
                table: "submissions",
                columns: new[] { "id", "created_at", "created_by", "form_id", "is_active", "meta_data", "updated_at", "updated_by" },
                values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System", new Guid("11111111-1111-1111-1111-111111111111"), true, null, new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System" });

            migrationBuilder.InsertData(
                table: "logic",
                columns: new[] { "id", "condition", "created_at", "created_by", "destination_field_id", "field_id", "is_active", "updated_at", "updated_by", "value" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), "equals", new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System", new Guid("22222222-2222-2222-2222-222222222223"), new Guid("22222222-2222-2222-2222-222222222222"), true, new DateTime(2026, 1, 11, 1, 46, 7, 0, DateTimeKind.Unspecified), "System", "John Doe" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "logic",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "submissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "fields",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "fields",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222223"));

            migrationBuilder.DeleteData(
                table: "forms",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("3538ed8c-b05d-4b58-80ff-2868aba45701"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("019923ff-5bd5-7622-b805-c53c2525ba21"));
        }
    }
}
