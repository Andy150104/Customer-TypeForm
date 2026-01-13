using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixLogicType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LogicGroupId",
                table: "logic",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "logic",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "logic",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "condition", "LogicGroupId", "Order" },
                values: new object[] { "Is", null, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogicGroupId",
                table: "logic");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "logic");

            migrationBuilder.UpdateData(
                table: "logic",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "condition",
                value: "equals");
        }
    }
}
