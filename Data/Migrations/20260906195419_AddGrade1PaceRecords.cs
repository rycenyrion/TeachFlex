using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGrade1PaceRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearnerPaceSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LearnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SchoolClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    TermNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    WhatLearnerCanDo = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    WhatLearnerNeedsToImprove = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TeacherRemarks = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerPaceSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnerPaceSummaries_Classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearnerPaceSummaries_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaceCompetencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    TermNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    LearningArea = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    DomainName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CompetencyCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaceCompetencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaceCompetencies_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearnerPaceRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LearnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaceCompetencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerPaceRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnerPaceRatings_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearnerPaceRatings_PaceCompetencies_PaceCompetencyId",
                        column: x => x.PaceCompetencyId,
                        principalTable: "PaceCompetencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPaceRatings_LearnerId_PaceCompetencyId",
                table: "LearnerPaceRatings",
                columns: new[] { "LearnerId", "PaceCompetencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPaceRatings_PaceCompetencyId",
                table: "LearnerPaceRatings",
                column: "PaceCompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPaceSummaries_LearnerId_SchoolClassId_TermNumber",
                table: "LearnerPaceSummaries",
                columns: new[] { "LearnerId", "SchoolClassId", "TermNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPaceSummaries_SchoolClassId",
                table: "LearnerPaceSummaries",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PaceCompetencies_SubjectId_TermNumber_DomainName_CompetencyCode",
                table: "PaceCompetencies",
                columns: new[] { "SubjectId", "TermNumber", "DomainName", "CompetencyCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerPaceRatings");

            migrationBuilder.DropTable(
                name: "LearnerPaceSummaries");

            migrationBuilder.DropTable(
                name: "PaceCompetencies");
        }
    }
}
