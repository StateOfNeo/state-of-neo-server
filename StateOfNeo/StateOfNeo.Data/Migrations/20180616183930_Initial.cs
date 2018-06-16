using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockchainInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SecondsCount = table.Column<decimal>(nullable: false),
                    BlockCount = table.Column<long>(nullable: false),
                    Net = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MainNetBlockInfos",
                columns: table => new
                {
                    SecondsCount = table.Column<int>(nullable: false),
                    TxCount = table.Column<long>(nullable: false),
                    TxSystemFees = table.Column<long>(nullable: false),
                    TxNetworkFees = table.Column<long>(nullable: false),
                    TxOutputValues = table.Column<long>(nullable: false),
                    BlockHeight = table.Column<decimal>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainNetBlockInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Protocol = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Height = table.Column<int>(nullable: true),
                    Peers = table.Column<int>(nullable: true),
                    MemoryPool = table.Column<int>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Locale = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    Latitude = table.Column<double>(nullable: true),
                    FlagUrl = table.Column<string>(nullable: true),
                    SuccessUrl = table.Column<string>(nullable: true),
                    Net = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestNetBlockInfos",
                columns: table => new
                {
                    SecondsCount = table.Column<int>(nullable: false),
                    TxCount = table.Column<long>(nullable: false),
                    TxSystemFees = table.Column<long>(nullable: false),
                    TxNetworkFees = table.Column<long>(nullable: false),
                    TxOutputValues = table.Column<long>(nullable: false),
                    BlockHeight = table.Column<decimal>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestNetBlockInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NodeAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Port = table.Column<long>(nullable: true),
                    Ip = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    NodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeAddresses_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    LastDownTime = table.Column<DateTime>(nullable: true),
                    NodeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeEvents_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeAddresses_NodeId",
                table: "NodeAddresses",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEvents_NodeId",
                table: "TimeEvents",
                column: "NodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockchainInfos");

            migrationBuilder.DropTable(
                name: "MainNetBlockInfos");

            migrationBuilder.DropTable(
                name: "NodeAddresses");

            migrationBuilder.DropTable(
                name: "TestNetBlockInfos");

            migrationBuilder.DropTable(
                name: "TimeEvents");

            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
