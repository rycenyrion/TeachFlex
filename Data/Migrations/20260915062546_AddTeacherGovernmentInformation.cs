using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherGovernmentInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PagIbigNumber",
                table: "Teachers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhilHealthNumber",
                table: "Teachers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SssNumber",
                table: "Teachers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UmidNumber",
                table: "Teachers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PagIbigNumber",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PhilHealthNumber",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "SssNumber",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "UmidNumber",
                table: "Teachers");
        }
    }
}
