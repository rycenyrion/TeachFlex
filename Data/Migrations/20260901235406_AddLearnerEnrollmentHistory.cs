using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLearnerEnrollmentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EnrollmentDate",
                table: "Learners",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnrollmentType",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExitDate",
                table: "Learners",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExitReason",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NextSchoolName",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousSchoolName",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrollmentDate",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "EnrollmentType",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "ExitDate",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "ExitReason",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "NextSchoolName",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "PreviousSchoolName",
                table: "Learners");
        }
    }
}
