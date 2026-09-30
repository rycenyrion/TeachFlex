using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeniorHighEcrMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PerformanceTasksWeight",
                table: "Subjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SubjectCluster",
                table: "Subjects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SummativeTermExamWeight",
                table: "Subjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SummativeTestOneShare",
                table: "Subjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SummativeTestTwoShare",
                table: "Subjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TermExamShare",
                table: "Subjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TermsTaught",
                table: "Subjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalHours",
                table: "Subjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitsPerTerm",
                table: "Subjects",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitsPerYear",
                table: "Subjects",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "WrittenOralWorksWeight",
                table: "Subjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerformanceTasksWeight",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SubjectCluster",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SummativeTermExamWeight",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SummativeTestOneShare",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SummativeTestTwoShare",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "TermExamShare",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "TermsTaught",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "UnitsPerTerm",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "UnitsPerYear",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "WrittenOralWorksWeight",
                table: "Subjects");
        }
    }
}
