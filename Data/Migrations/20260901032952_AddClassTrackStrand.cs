using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClassTrackStrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackStrand",
                table: "Classes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackStrand",
                table: "Classes");
        }
    }
}
