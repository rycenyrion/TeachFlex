using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEClassRecordTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SchoolClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    TermNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AssessmentDomain = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    AssessmentName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    HighestPossibleScore = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentItems_Classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentItems_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearnerAssessmentScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssessmentItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    LearnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerAssessmentScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnerAssessmentScores_AssessmentItems_AssessmentItemId",
                        column: x => x.AssessmentItemId,
                        principalTable: "AssessmentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearnerAssessmentScores_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentItems_SchoolClassId_SubjectId_TermNumber_Category_AssessmentDomain_DisplayOrder",
                table: "AssessmentItems",
                columns: new[] { "SchoolClassId", "SubjectId", "TermNumber", "Category", "AssessmentDomain", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentItems_SubjectId",
                table: "AssessmentItems",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAssessmentScores_AssessmentItemId_LearnerId",
                table: "LearnerAssessmentScores",
                columns: new[] { "AssessmentItemId", "LearnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAssessmentScores_LearnerId",
                table: "LearnerAssessmentScores",
                column: "LearnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerAssessmentScores");

            migrationBuilder.DropTable(
                name: "AssessmentItems");
        }
    }
}
