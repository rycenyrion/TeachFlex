using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSf1LearnerProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcceleratedLevel",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Barangay",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CctReferenceNumber",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuardianName",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuardianRelationship",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HouseStreetSitioPurok",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IndigenousPeopleEthnicGroup",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccelerated",
                table: "Learners",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBalikAral",
                table: "Learners",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCctRecipient",
                table: "Learners",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpecialNeedsEducation",
                table: "Learners",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastSchoolAttended",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastSchoolYearAttended",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LearningModality",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotherMaidenName",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotherTongue",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityCity",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Religion",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sf1Remarks",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialNeedsDetails",
                table: "Learners",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceleratedLevel",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "Barangay",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "CctReferenceNumber",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "GuardianName",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "GuardianRelationship",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "HouseStreetSitioPurok",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "IndigenousPeopleEthnicGroup",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "IsAccelerated",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "IsBalikAral",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "IsCctRecipient",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "IsSpecialNeedsEducation",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "LastSchoolAttended",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "LastSchoolYearAttended",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "LearningModality",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "MotherMaidenName",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "MotherTongue",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "MunicipalityCity",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "Religion",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "Sf1Remarks",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "SpecialNeedsDetails",
                table: "Learners");
        }
    }
}
