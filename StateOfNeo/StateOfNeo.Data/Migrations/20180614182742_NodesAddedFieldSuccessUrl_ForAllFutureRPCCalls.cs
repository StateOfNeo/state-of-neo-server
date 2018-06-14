using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class NodesAddedFieldSuccessUrl_ForAllFutureRPCCalls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Nodes",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuccessUrl",
                table: "Nodes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuccessUrl",
                table: "Nodes");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Nodes",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
