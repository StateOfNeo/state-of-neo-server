﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace StateOfNeo.Data.Migrations
{
    public partial class AddedHeightPeersAndMemoryPool_ToNodeEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemoryPool",
                table: "Nodes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Peers",
                table: "Nodes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "MemoryPool",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Peers",
                table: "Nodes");
        }
    }
}
