using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Migrations
{
    /// <inheritdoc />
    public partial class AddKayitTarihiToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "KayitTarihi",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KayitTarihi",
                table: "Users");
        }
    }
}
