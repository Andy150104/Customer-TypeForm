using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldOptionIdToAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "field_option_id",
                table: "answers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_answers_field_option_id",
                table: "answers",
                column: "field_option_id");

            migrationBuilder.AddForeignKey(
                name: "FK_answers_field_options_field_option_id",
                table: "answers",
                column: "field_option_id",
                principalTable: "field_options",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_answers_field_options_field_option_id",
                table: "answers");

            migrationBuilder.DropIndex(
                name: "IX_answers_field_option_id",
                table: "answers");

            migrationBuilder.DropColumn(
                name: "field_option_id",
                table: "answers");
        }
    }
}
