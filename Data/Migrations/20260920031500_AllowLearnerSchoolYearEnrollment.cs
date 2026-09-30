using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowLearnerSchoolYearEnrollment :
        Migration
    {
        /// <inheritdoc />
        protected override void Up(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Learners_Lrn",
                table: "Learners");

            migrationBuilder.DropIndex(
                name: "IX_Learners_SchoolClassId",
                table: "Learners");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_SchoolClassId_Lrn",
                table: "Learners",
                columns: new[]
                {
                    "SchoolClassId",
                    "Lrn"
                },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Learners_SchoolClassId_Lrn",
                table: "Learners");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_SchoolClassId",
                table: "Learners",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_Lrn",
                table: "Learners",
                column: "Lrn",
                unique: true);
        }
    }
}
