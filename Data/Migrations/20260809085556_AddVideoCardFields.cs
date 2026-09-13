using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareIT.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoCardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server can't cast 'ShareITVideos' -> int on its own, so convert
            // through a temporary column: add int, translate, drop text, rename.
            migrationBuilder.AddColumn<int>(
                name: "SectionTmp",
                table: "Videos",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE Videos SET SectionTmp =
                    CASE Section
                        WHEN 'NewCase'       THEN 0
                        WHEN 'TrackCase'     THEN 1
                        WHEN 'ShareITVideos' THEN 2
                        ELSE 0
                    END;");

            migrationBuilder.DropColumn(name: "Section", table: "Videos");

            migrationBuilder.RenameColumn(
                name: "SectionTmp",
                table: "Videos",
                newName: "Section");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Videos",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "Videos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Videos",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Videos");

            migrationBuilder.AlterColumn<string>(
                name: "Section",
                table: "Videos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
