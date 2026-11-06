using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudyProgramId",
                table: "Groups",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudyPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPrograms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_StudyProgramId",
                table: "Groups",
                column: "StudyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPrograms_Name",
                table: "StudyPrograms",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_StudyPrograms_StudyProgramId",
                table: "Groups",
                column: "StudyProgramId",
                principalTable: "StudyPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_StudyPrograms_StudyProgramId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "StudyPrograms");

            migrationBuilder.DropIndex(
                name: "IX_Groups_StudyProgramId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "StudyProgramId",
                table: "Groups");
        }
    }
}
