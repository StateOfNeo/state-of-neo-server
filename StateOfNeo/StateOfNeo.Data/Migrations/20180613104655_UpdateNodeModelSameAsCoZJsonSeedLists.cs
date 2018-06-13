using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class UpdateNodeModelSameAsCoZJsonSeedLists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Nodes",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Nodes",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Nodes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Protocol",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Nodes");
        }
    }
}
