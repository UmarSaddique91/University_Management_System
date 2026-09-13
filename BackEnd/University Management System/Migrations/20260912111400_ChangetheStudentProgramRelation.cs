using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class ChangetheStudentProgramRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Program",
                table: "Students");

            migrationBuilder.AddColumn<int>(
                name: "AcademicProgramId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_AcademicProgramId",
                table: "Students",
                column: "AcademicProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AcademicPrograms_AcademicProgramId",
                table: "Students",
                column: "AcademicProgramId",
                principalTable: "AcademicPrograms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_AcademicPrograms_AcademicProgramId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_AcademicProgramId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AcademicProgramId",
                table: "Students");

            migrationBuilder.AddColumn<string>(
                name: "Program",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
