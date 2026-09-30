using Microsoft.EntityFrameworkCore;
using TeachFlex.Models;

namespace TeachFlex.Data
{
    public class TeachFlexDbContext :
        DbContext
    {
        public TeachFlexDbContext(
            DbContextOptions<TeachFlexDbContext>
                options)
            : base(
                options)
        {
        }

        public DbSet<School> Schools =>
            Set<School>();

        public DbSet<Teacher> Teachers =>
            Set<Teacher>();

        public DbSet<AcademicYear> AcademicYears =>
            Set<AcademicYear>();

        public DbSet<SchoolClass> Classes =>
            Set<SchoolClass>();

        public DbSet<Learner> Learners =>
            Set<Learner>();

        public DbSet<Subject> Subjects =>
            Set<Subject>();

        public DbSet<ClassSubject> ClassSubjects =>
            Set<ClassSubject>();

        public DbSet<AttendanceDay> AttendanceDays =>
            Set<AttendanceDay>();

        public DbSet<LearnerAttendance>
            LearnerAttendances =>
                Set<LearnerAttendance>();

        public DbSet<AssessmentItem>
            AssessmentItems =>
                Set<AssessmentItem>();

        public DbSet<LearnerAssessmentScore>
            LearnerAssessmentScores =>
                Set<LearnerAssessmentScore>();

        public DbSet<PaceCompetency>
            PaceCompetencies =>
                Set<PaceCompetency>();

        public DbSet<LearnerPaceRating>
            LearnerPaceRatings =>
                Set<LearnerPaceRating>();

        public DbSet<LearnerPaceSummary>
            LearnerPaceSummaries =>
                Set<LearnerPaceSummary>();

        public DbSet<LearnerTermRemark>
            LearnerTermRemarks =>
                Set<LearnerTermRemark>();

        public DbSet<KindergartenCompetency>
    KindergartenCompetencies =>
        Set<KindergartenCompetency>();

        public DbSet<KindergartenLearnerRating>
            KindergartenLearnerRatings =>
                Set<KindergartenLearnerRating>();

        public DbSet<KindergartenTermRemark>
            KindergartenTermRemarks =>
                Set<KindergartenTermRemark>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(
                modelBuilder);

            ConfigureSchool(
                modelBuilder);

            ConfigureTeacher(
                modelBuilder);

            ConfigureAcademicYear(
                modelBuilder);

            ConfigureSchoolClass(
                modelBuilder);

            ConfigureLearner(
                modelBuilder);

            ConfigureSubject(
                modelBuilder);

            ConfigureClassSubject(
                modelBuilder);

            ConfigureAttendanceDay(
                modelBuilder);

            ConfigureLearnerAttendance(
                modelBuilder);

            ConfigureAssessmentItem(
                modelBuilder);

            ConfigureLearnerAssessmentScore(
                modelBuilder);

            ConfigurePaceCompetency(
                modelBuilder);

            ConfigureLearnerPaceRating(
                modelBuilder);

            ConfigureLearnerPaceSummary(
                modelBuilder);

            ConfigureLearnerTermRemark(
                modelBuilder);

            ConfigureKindergartenCompetency(
    modelBuilder);

            ConfigureKindergartenLearnerRating(
                modelBuilder);

            ConfigureKindergartenTermRemark(
                modelBuilder);
        }

        private static void ConfigureSchool(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<School>(
                entity =>
                {
                    entity.HasKey(
                        school =>
                            school.Id);

                    entity.Property(
                            school =>
                                school.SchoolName)
                        .IsRequired()
                        .HasMaxLength(
                            200);

                    entity.Property(
                            school =>
                                school.SchoolId)
                        .IsRequired()
                        .HasMaxLength(
                            30);

                    entity.HasIndex(
                            school =>
                                school.SchoolId)
                        .IsUnique();
                });
        }

        private static void ConfigureTeacher(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Teacher>(
                entity =>
                {
                    entity.HasKey(
                        teacher =>
                            teacher.Id);

                    entity.Property(
                            teacher =>
                                teacher.FirstName)
                        .IsRequired()
                        .HasMaxLength(
                            100);

                    entity.Property(
                            teacher =>
                                teacher.LastName)
                        .IsRequired()
                        .HasMaxLength(
                            100);

                    entity.HasOne(
                            teacher =>
                                teacher.School)
                        .WithMany()
                        .HasForeignKey(
                            teacher =>
                                teacher.SchoolId)
                        .OnDelete(
                            DeleteBehavior.Cascade);
                });
        }

        private static void ConfigureAcademicYear(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AcademicYear>(
                entity =>
                {
                    entity.HasKey(
                        academicYear =>
                            academicYear.Id);

                    entity.HasIndex(
                            academicYear =>
                                new
                                {
                                    academicYear.SchoolId,
                                    academicYear.StartYear,
                                    academicYear.EndYear
                                })
                        .IsUnique();

                    entity.HasOne(
                            academicYear =>
                                academicYear.School)
                        .WithMany()
                        .HasForeignKey(
                            academicYear =>
                                academicYear.SchoolId)
                        .OnDelete(
                            DeleteBehavior.Cascade);
                });
        }

        private static void ConfigureSchoolClass(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SchoolClass>(
                entity =>
                {
                    entity.HasKey(
                        schoolClass =>
                            schoolClass.Id);

                    entity.Property(
                            schoolClass =>
                                schoolClass.GradeLevel)
                        .IsRequired()
                        .HasMaxLength(
                            30);

                    entity.Property(
                            schoolClass =>
                                schoolClass.SectionName)
                        .IsRequired()
                        .HasMaxLength(
                            100);

                    entity.HasIndex(
                            schoolClass =>
                                new
                                {
                                    schoolClass.SchoolId,
                                    schoolClass.AcademicYearId,
                                    schoolClass.GradeLevel,
                                    schoolClass.SectionName
                                })
                        .IsUnique();

                    entity.HasOne(
                            schoolClass =>
                                schoolClass.School)
                        .WithMany()
                        .HasForeignKey(
                            schoolClass =>
                                schoolClass.SchoolId)
                        .OnDelete(
                            DeleteBehavior.Restrict);

                    entity.HasOne(
                            schoolClass =>
                                schoolClass.AcademicYear)
                        .WithMany()
                        .HasForeignKey(
                            schoolClass =>
                                schoolClass.AcademicYearId)
                        .OnDelete(
                            DeleteBehavior.Restrict);

                    entity.HasOne(
                            schoolClass =>
                                schoolClass.Adviser)
                        .WithMany()
                        .HasForeignKey(
                            schoolClass =>
                                schoolClass.AdviserId)
                        .OnDelete(
                            DeleteBehavior.SetNull);
                });
        }

        private static void ConfigureLearner(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Learner>(
                entity =>
                {
                    entity.HasKey(
                        learner =>
                            learner.Id);

                    entity.Property(
                            learner =>
                                learner.Lrn)
                        .IsRequired()
                        .HasMaxLength(
                            20);

                    entity.Property(
                            learner =>
                                learner.FirstName)
                        .IsRequired()
                        .HasMaxLength(
                            100);

                    entity.Property(
                            learner =>
                                learner.LastName)
                        .IsRequired()
                        .HasMaxLength(
                            100);

                    entity.HasIndex(
                            learner =>
                                new
                                {
                                    learner.SchoolClassId,
                                    learner.Lrn
                                })
                        .IsUnique();

                    entity.HasOne(
                            learner =>
                                learner.SchoolClass)
                        .WithMany()
                        .HasForeignKey(
                            learner =>
                                learner.SchoolClassId)
                        .OnDelete(
                            DeleteBehavior.Cascade);
                });
        }

        private static void ConfigureSubject(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subject>(
                entity =>
                {
                    entity.HasKey(
                        subject =>
                            subject.Id);

                    entity.Property(
                            subject =>
                                subject.SubjectCode)
                        .IsRequired()
                        .HasMaxLength(
                            50);

                    entity.Property(
                            subject =>
                                subject.SubjectName)
                        .IsRequired()
                        .HasMaxLength(
                            150);

                    entity.Property(
                            subject =>
                                subject.GradeLevel)
                        .IsRequired()
                        .HasMaxLength(
                            30);

                    entity.HasIndex(
                            subject =>
                                new
                                {
                                    subject.GradeLevel,
                                    subject.SubjectCode
                                })
                        .IsUnique();
                });
        }

        private static void ConfigureClassSubject(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClassSubject>(
                entity =>
                {
                    entity.HasKey(
                        classSubject =>
                            classSubject.Id);

                    entity.HasIndex(
                            classSubject =>
                                new
                                {
                                    classSubject.SchoolClassId,
                                    classSubject.SubjectId
                                })
                        .IsUnique();

                    entity.HasOne(
                            classSubject =>
                                classSubject.SchoolClass)
                        .WithMany()
                        .HasForeignKey(
                            classSubject =>
                                classSubject.SchoolClassId)
                        .OnDelete(
                            DeleteBehavior.Cascade);

                    entity.HasOne(
                            classSubject =>
                                classSubject.Subject)
                        .WithMany()
                        .HasForeignKey(
                            classSubject =>
                                classSubject.SubjectId)
                        .OnDelete(
                            DeleteBehavior.Restrict);
                });
        }

        private static void ConfigureAttendanceDay(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttendanceDay>(
                entity =>
                {
                    entity.HasKey(
                        attendanceDay =>
                            attendanceDay.Id);

                    entity.Property(
                            attendanceDay =>
                                attendanceDay.DayStatus)
                        .IsRequired()
                        .HasMaxLength(
                            50);

                    entity.Property(
                            attendanceDay =>
                                attendanceDay.Description)
                        .HasMaxLength(
                            250);

                    entity.HasIndex(
                            attendanceDay =>
                                new
                                {
                                    attendanceDay.SchoolClassId,
                                    attendanceDay.AttendanceDate
                                })
                        .IsUnique();

                    entity.HasOne(
                            attendanceDay =>
                                attendanceDay.SchoolClass)
                        .WithMany()
                        .HasForeignKey(
                            attendanceDay =>
                                attendanceDay.SchoolClassId)
                        .OnDelete(
                            DeleteBehavior.Cascade);
                });
        }

        private static void ConfigureLearnerAttendance(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LearnerAttendance>(
                entity =>
                {
                    entity.HasKey(
                        learnerAttendance =>
                            learnerAttendance.Id);

                    entity.Property(
                            learnerAttendance =>
                                learnerAttendance
                                    .AttendanceStatus)
                        .IsRequired()
                        .HasMaxLength(
                            20);

                    entity.Property(
                            learnerAttendance =>
                                learnerAttendance.Remarks)
                        .HasMaxLength(
                            500);

                    entity.HasIndex(
                            learnerAttendance =>
                                new
                                {
                                    learnerAttendance.AttendanceDayId,
                                    learnerAttendance.LearnerId
                                })
                        .IsUnique();

                    entity.HasOne(
                            learnerAttendance =>
                                learnerAttendance.AttendanceDay)
                        .WithMany()
                        .HasForeignKey(
                            learnerAttendance =>
                                learnerAttendance.AttendanceDayId)
                        .OnDelete(
                            DeleteBehavior.Cascade);

                    entity.HasOne(
                            learnerAttendance =>
                                learnerAttendance.Learner)
                        .WithMany()
                        .HasForeignKey(
                            learnerAttendance =>
                                learnerAttendance.LearnerId)
                        .OnDelete(
                            DeleteBehavior.Restrict);
                });
        }

        private static void ConfigureAssessmentItem(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssessmentItem>(
                entity =>
                {
                    entity.HasKey(
                        assessmentItem =>
                            assessmentItem.Id);

                    entity.Property(
                            assessmentItem =>
                                assessmentItem.Category)
                        .IsRequired()
                        .HasMaxLength(
                            50);

                    entity.Property(
                            assessmentItem =>
                                assessmentItem.AssessmentDomain)
                        .IsRequired()
                        .HasMaxLength(
                            30);

                    entity.Property(
                            assessmentItem =>
                                assessmentItem.AssessmentName)
                        .IsRequired()
                        .HasMaxLength(
                            150);

                    entity.Property(
                            assessmentItem =>
                                assessmentItem
                                    .HighestPossibleScore)
                        .HasPrecision(
                            10,
                            2);

                    entity.HasIndex(
                        assessmentItem =>
                            new
                            {
                                assessmentItem.SchoolClassId,
                                assessmentItem.SubjectId,
                                assessmentItem.TermNumber,
                                assessmentItem.Category,
                                assessmentItem.AssessmentDomain,
                                assessmentItem.DisplayOrder
                            });

                    entity.HasOne(
                            assessmentItem =>
                                assessmentItem.SchoolClass)
                        .WithMany()
                        .HasForeignKey(
                            assessmentItem =>
                                assessmentItem.SchoolClassId)
                        .OnDelete(
                            DeleteBehavior.Cascade);

                    entity.HasOne(
                            assessmentItem =>
                                assessmentItem.Subject)
                        .WithMany()
                        .HasForeignKey(
                            assessmentItem =>
                                assessmentItem.SubjectId)
                        .OnDelete(
                            DeleteBehavior.Restrict);
                });
        }

        private static void
            ConfigureLearnerAssessmentScore(
                ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<
                LearnerAssessmentScore>(
                entity =>
                {
                    entity.HasKey(
                        learnerScore =>
                            learnerScore.Id);

                    entity.Property(
                            learnerScore =>
                                learnerScore.Score)
                        .HasPrecision(
                            10,
                            2);

                    entity.Property(
                            learnerScore =>
                                learnerScore.Remarks)
                        .HasMaxLength(
                            250);

                    entity.HasIndex(
                            learnerScore =>
                                new
                                {
                                    learnerScore
                                        .AssessmentItemId,

                                    learnerScore.LearnerId
                                })
                        .IsUnique();

                    entity.HasOne(
                            learnerScore =>
                                learnerScore.AssessmentItem)
                        .WithMany()
                        .HasForeignKey(
                            learnerScore =>
                                learnerScore.AssessmentItemId)
                        .OnDelete(
                            DeleteBehavior.Cascade);

                    entity.HasOne(
                            learnerScore =>
                                learnerScore.Learner)
                        .WithMany()
                        .HasForeignKey(
                            learnerScore =>
                                learnerScore.LearnerId)
                        .OnDelete(
                            DeleteBehavior.Restrict);
                });
        }

        private static void ConfigurePaceCompetency(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaceCompetency>(
                entity =>
                {
                    entity.HasKey(
                        competency =>
                            competency.Id);

                    entity.Property(
                            competency =>
                                competency.LearningArea)
                        .IsRequired()
                        .HasMaxLength(
                            150);

                    entity.Property(
                            competency =>
                                competency.DomainName)
                        .IsRequired()
                        .HasMaxLength(
                            200);

                    entity.Property(
                            competency =>
                                competency.CompetencyCode)
                        .IsRequired()
                        .HasMaxLength(
                            50);

                    entity.Property(
                            competency =>
                                competency.Description)
                        .IsRequired()
                        .HasMaxLength(
                            1000);

                    entity.HasIndex(
                            competency =>
                                new
                                {
                                    competency.SubjectId,
                                    competency.TermNumber,
                                    competency.DomainName,
                                    competency.CompetencyCode
                                })
                        .IsUnique();

                    entity.HasOne(
                            competency =>
                                competency.Subject)
                        .WithMany()
                        .HasForeignKey(
                            competency =>
                                competency.SubjectId)
                        .OnDelete(
                            DeleteBehavior.Restrict);
                });
        }

        private static void
            ConfigureLearnerPaceRating(
                ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LearnerPaceRating>(
                entity =>
                {
                    entity.HasKey(
                        rating =>
                            rating.Id);

                    entity.Property(
                            rating =>
                                rating.Rating)
                        .IsRequired()
                        .HasMaxLength(
                            1);

                    entity.Property(
                            rating =>
                                rating.Remarks)
                        .HasMaxLength(
                            500);

                    entity.HasIndex(
                            rating =>
                                new
                                {
                                    rating.LearnerId,
                                    rating.PaceCompetencyId
                                })
                        .IsUnique();

                    entity.HasOne(
                            rating =>
                                rating.Learner)
                        .WithMany()
                        .HasForeignKey(
                            rating =>
                                rating.LearnerId)
                        .OnDelete(
                            DeleteBehavior.Restrict);

                    entity.HasOne(
                            rating =>
                                rating.PaceCompetency)
                        .WithMany(
                            competency =>
                                competency.LearnerRatings)
                        .HasForeignKey(
                            rating =>
                                rating.PaceCompetencyId)
                        .OnDelete(
                            DeleteBehavior.Cascade);
                });
        }

        private static void
            ConfigureLearnerPaceSummary(
                ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LearnerPaceSummary>(
                entity =>
                {
                    entity.HasKey(
                        summary =>
                            summary.Id);

                    entity.Property(
                            summary =>
                                summary.WhatLearnerCanDo)
                        .HasMaxLength(
                            2000);

                    entity.Property(
                            summary =>
                                summary
                                    .WhatLearnerNeedsToImprove)
                        .HasMaxLength(
                            2000);

                    entity.Property(
                            summary =>
                                summary.TeacherRemarks)
                        .HasMaxLength(
                            2000);

                    entity.HasIndex(
                            summary =>
                                new
                                {
                                    summary.LearnerId,
                                    summary.SchoolClassId,
                                    summary.TermNumber
                                })
                        .IsUnique();

                    entity.HasOne(
                            summary =>
                                summary.Learner)
                        .WithMany()
                        .HasForeignKey(
                            summary =>
                                summary.LearnerId)
                        .OnDelete(
                            DeleteBehavior.Restrict);

                    entity.HasOne(
                            summary =>
                                summary.SchoolClass)
                        .WithMany()
                        .HasForeignKey(
                            summary =>
                                summary.SchoolClassId)
                        .OnDelete(
                            DeleteBehavior.Cascade);
                });
        }
        private static void
            ConfigureLearnerTermRemark(
                ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LearnerTermRemark>(
                entity =>
                {
                    entity.HasKey(
                        remark =>
                            remark.Id);

                    entity.Property(
                            remark =>
                                remark.PerformanceLevel)
                        .HasConversion<string>()
                        .IsRequired()
                        .HasMaxLength(30);

                    entity.Property(
                            remark =>
                                remark.SourceGeneralAverage)
                        .HasPrecision(5, 2);

                    entity.Property(
                            remark =>
                                remark.SuggestedRemark)
                        .HasMaxLength(2000);

                    entity.Property(
                            remark =>
                                remark.FinalRemark)
                        .HasMaxLength(2000);

                    entity.HasIndex(
                            remark =>
                                new
                                {
                                    remark.SchoolClassId,
                                    remark.LearnerId,
                                    remark.TermNumber
                                })
                        .IsUnique();

                    entity.HasOne(
                            remark =>
                                remark.SchoolClass)
                        .WithMany()
                        .HasForeignKey(
                            remark =>
                                remark.SchoolClassId)
                        .OnDelete(
                            DeleteBehavior.Cascade);

                    entity.HasOne(
                            remark =>
                                remark.Learner)
                        .WithMany()
                        .HasForeignKey(
                            remark =>
                                remark.LearnerId)
                        .OnDelete(
                            DeleteBehavior.Restrict);
                });
        }

        private static void
            ConfigureKindergartenCompetency(
        ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<
                KindergartenCompetency>(
                entity =>
                {
                    entity.HasKey(
                        competency =>
                            competency.Id);

                    entity.Property(
                            competency =>
                                competency.CompetencyCode)
                        .IsRequired()
                        .HasMaxLength(
                            50);

                    entity.Property(
                            competency =>
                                competency.DevelopmentArea)
                        .IsRequired()
                        .HasMaxLength(
                            200);

                    entity.Property(
                            competency =>
                                competency.SubDomain)
                        .HasMaxLength(
                            200);

                    entity.Property(
                            competency =>
                                competency.Description)
                        .IsRequired()
                        .HasMaxLength(
                            1000);

                    entity.HasIndex(
                            competency =>
                                competency.CompetencyCode)
                        .IsUnique();
                });
        }

        private static void
            ConfigureKindergartenLearnerRating(
                ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<
                KindergartenLearnerRating>(
                entity =>
                {
                    entity.HasKey(
                        rating =>
                            rating.Id);

                    entity.Property(
                            rating =>
                                rating.Rating)
                        .IsRequired()
                        .HasMaxLength(
                            2);

                    entity.Property(
                            rating =>
                                rating.Observation)
                        .HasMaxLength(
                            1000);

                    entity.HasIndex(
                            rating =>
                                new
                                {
                                    rating.SchoolClassId,
                                    rating.LearnerId,
                                    rating.KindergartenCompetencyId,
                                    rating.TermNumber
                                })
                        .IsUnique();

                    entity.HasOne(
                            rating =>
                                rating.SchoolClass)
                        .WithMany()
                        .HasForeignKey(
                            rating =>
                                rating.SchoolClassId)
                        .OnDelete(
                            DeleteBehavior.Cascade);

                    entity.HasOne(
                            rating =>
                                rating.Learner)
                        .WithMany()
                        .HasForeignKey(
                            rating =>
                                rating.LearnerId)
                        .OnDelete(
                            DeleteBehavior.Restrict);

                    entity.HasOne(
                            rating =>
                                rating.KindergartenCompetency)
                        .WithMany()
                        .HasForeignKey(
                            rating =>
                                rating.KindergartenCompetencyId)
                        .OnDelete(
                            DeleteBehavior.Restrict);
                });
        }

        private static void
            ConfigureKindergartenTermRemark(
                ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<
                KindergartenTermRemark>(
                entity =>
                {
                    entity.HasKey(
                        remark =>
                            remark.Id);

                    entity.Property(
                            remark =>
                                remark.TeacherComment)
                        .HasMaxLength(
                            4000);

                    entity.Property(
                            remark =>
                                remark.LearnerStrengths)
                        .HasMaxLength(
                            4000);

                    entity.Property(
                            remark =>
                                remark.SuggestedInterventions)
                        .HasMaxLength(
                            4000);

                    entity.HasIndex(
                            remark =>
                                new
                                {
                                    remark.SchoolClassId,
                                    remark.LearnerId,
                                    remark.TermNumber
                                })
                        .IsUnique();

                    entity.HasOne(
                            remark =>
                                remark.SchoolClass)
                        .WithMany()
                        .HasForeignKey(
                            remark =>
                                remark.SchoolClassId)
                        .OnDelete(
                            DeleteBehavior.Cascade);

                    entity.HasOne(
                            remark =>
                                remark.Learner)
                        .WithMany()
                        .HasForeignKey(
                            remark =>
                                remark.LearnerId)
                        .OnDelete(
                            DeleteBehavior.Restrict);
                });
        }
    }
}
