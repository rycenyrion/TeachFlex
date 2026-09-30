using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLearnerTermRemarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearnerTermRemarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SchoolClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    LearnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    TermNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    GradeLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    PerformanceLevel = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    SourceGeneralAverage = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    SuggestedRemark = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    FinalRemark = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SuggestionVariation = table.Column<int>(type: "INTEGER", nullable: false),
                    IsTeacherApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    NeedsReview = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerTermRemarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnerTermRemarks_Classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearnerTermRemarks_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearnerTermRemarks_LearnerId",
                table: "LearnerTermRemarks",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerTermRemarks_SchoolClassId_LearnerId_TermNumber",
                table: "LearnerTermRemarks",
                columns: new[] { "SchoolClassId", "LearnerId", "TermNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerTermRemarks");
        }
    }
}
