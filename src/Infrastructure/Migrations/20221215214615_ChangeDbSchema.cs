using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ChangeDbSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "family");

            migrationBuilder.RenameTable(
                name: "UserFamilyRoles",
                newName: "UserFamilyRoles",
                newSchema: "family");

            migrationBuilder.RenameTable(
                name: "FamilyRoles",
                newName: "FamilyRoles",
                newSchema: "family");

            migrationBuilder.RenameTable(
                name: "Families",
                newName: "Families",
                newSchema: "family");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserFamilyRoles",
                schema: "family",
                newName: "UserFamilyRoles");

            migrationBuilder.RenameTable(
                name: "FamilyRoles",
                schema: "family",
                newName: "FamilyRoles");

            migrationBuilder.RenameTable(
                name: "Families",
                schema: "family",
                newName: "Families");
        }
    }
}
