using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCurrentSemesterIdAndUniqueEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments");

            migrationBuilder.AddColumn<int>(
                name: "CurrentSemesterId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Students_CurrentSemesterId",
                table: "Students",
                column: "CurrentSemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_CourseOfferingId",
                table: "Enrollments",
                columns: new[] { "StudentId", "CourseOfferingId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Semesters_CurrentSemesterId",
                table: "Students",
                column: "CurrentSemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                UPDATE st
                SET st.CurrentSemesterId = s.Id
                FROM [Students] st
                JOIN [Semesters] s ON st.AcademicProgramId = s.AcademicProgramId AND st.Semester = s.SemesterNumber
                JOIN [AcademicSessions] sess ON s.AcademicSessionId = sess.Id AND sess.IsActive = 1
                WHERE st.CurrentSemesterId IS NULL AND st.AcademicProgramId IS NOT NULL AND st.Semester IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Semesters_CurrentSemesterId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_CurrentSemesterId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId_CourseOfferingId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CurrentSemesterId",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");
        }
    }
}
