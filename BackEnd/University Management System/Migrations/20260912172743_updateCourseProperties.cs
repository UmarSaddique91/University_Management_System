using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class updateCourseProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Courses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
