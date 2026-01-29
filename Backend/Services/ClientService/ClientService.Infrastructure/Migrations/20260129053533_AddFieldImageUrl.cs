using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "fields",
                type: "varchar",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "fields",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "image_url",
                value: null);

            migrationBuilder.UpdateData(
                table: "fields",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222223"),
                column: "image_url",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_url",
                table: "fields");
        }
    }
}
