using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachFlex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKindergartenEClassRecordTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KindergartenCompetencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompetencyCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DevelopmentArea = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SubDomain = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KindergartenCompetencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KindergartenTermRemarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SchoolClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    LearnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    TermNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TeacherComment = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    LearnerStrengths = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    SuggestedInterventions = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KindergartenTermRemarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KindergartenTermRemarks_Classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KindergartenTermRemarks_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KindergartenLearnerRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SchoolClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    LearnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    KindergartenCompetencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    TermNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Observation = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KindergartenLearnerRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KindergartenLearnerRatings_Classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KindergartenLearnerRatings_KindergartenCompetencies_KindergartenCompetencyId",
                        column: x => x.KindergartenCompetencyId,
                        principalTable: "KindergartenCompetencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KindergartenLearnerRatings_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KindergartenCompetencies_CompetencyCode",
                table: "KindergartenCompetencies",
                column: "CompetencyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KindergartenLearnerRatings_KindergartenCompetencyId",
                table: "KindergartenLearnerRatings",
                column: "KindergartenCompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_KindergartenLearnerRatings_LearnerId",
                table: "KindergartenLearnerRatings",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_KindergartenLearnerRatings_SchoolClassId_LearnerId_KindergartenCompetencyId_TermNumber",
                table: "KindergartenLearnerRatings",
                columns: new[] { "SchoolClassId", "LearnerId", "KindergartenCompetencyId", "TermNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KindergartenTermRemarks_LearnerId",
                table: "KindergartenTermRemarks",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_KindergartenTermRemarks_SchoolClassId_LearnerId_TermNumber",
                table: "KindergartenTermRemarks",
                columns: new[] { "SchoolClassId", "LearnerId", "TermNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KindergartenLearnerRatings");

            migrationBuilder.DropTable(
                name: "KindergartenTermRemarks");

            migrationBuilder.DropTable(
                name: "KindergartenCompetencies");
        }
    }
}
