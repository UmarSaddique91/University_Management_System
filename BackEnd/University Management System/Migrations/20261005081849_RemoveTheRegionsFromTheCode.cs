using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace University_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTheRegionsFromTheCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferings_Teachers_TeacherId",
                table: "CourseOfferings");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "CourseOfferings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferings_Teachers_TeacherId",
                table: "CourseOfferings",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferings_Teachers_TeacherId",
                table: "CourseOfferings");

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "CourseOfferings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentModule = table.Column<bool>(type: "bit", nullable: false),
                    Authentication = table.Column<bool>(type: "bit", nullable: false),
                    ContactaEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CourseModule = table.Column<bool>(type: "bit", nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultTimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinPasswordLength = table.Column<int>(type: "int", nullable: false),
                    PasswordComplexity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentModule = table.Column<bool>(type: "bit", nullable: false),
                    PrimaryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeacherModule = table.Column<bool>(type: "bit", nullable: false),
                    UniversityLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UniversityName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferings_Teachers_TeacherId",
                table: "CourseOfferings",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
