using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtinEnrollmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnrollmentDate",
                table: "Enrollments",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Enrollments",
                newName: "EnrollmentDate");
        }
    }
}
