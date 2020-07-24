using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TimedHostedServiceExample.Model.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Delay = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDatas", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobDatas");
        }
    }
}
