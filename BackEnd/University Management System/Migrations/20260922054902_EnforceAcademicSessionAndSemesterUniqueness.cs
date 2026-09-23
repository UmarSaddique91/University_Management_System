using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class EnforceAcademicSessionAndSemesterUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM [Semesters] WHERE [Id] = 1 AND [AcademicSessionId] = 2)
                BEGIN
                    UPDATE [CourseOfferings] SET [SemesterId] = 3 WHERE [SemesterId] = 1;
                    UPDATE [Students] SET [CurrentSemesterId] = 3 WHERE [CurrentSemesterId] = 1;
                    DELETE FROM [Semesters] WHERE [Id] = 1;
                END

                DELETE FROM [AcademicSessions] WHERE [SessionName] LIKE 'TestSession_%';
                IF EXISTS (SELECT 1 FROM [AcademicSessions] WHERE [Id] = 2 AND [IsActive] = 0)
                BEGIN
                    DELETE FROM [AcademicSessions] WHERE [Id] = 2;
                END
            ");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_AcademicProgramId",
                table: "Semesters");

            migrationBuilder.AlterColumn<string>(
                name: "SessionName",
                table: "AcademicSessions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_AcademicProgramId_AcademicSessionId_SemesterNumber",
                table: "Semesters",
                columns: new[] { "AcademicProgramId", "AcademicSessionId", "SemesterNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicSessions_SessionName",
                table: "AcademicSessions",
                column: "SessionName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Semesters_AcademicProgramId_AcademicSessionId_SemesterNumber",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_AcademicSessions_SessionName",
                table: "AcademicSessions");

            migrationBuilder.AlterColumn<string>(
                name: "SessionName",
                table: "AcademicSessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_AcademicProgramId",
                table: "Semesters",
                column: "AcademicProgramId");
        }
    }
}
