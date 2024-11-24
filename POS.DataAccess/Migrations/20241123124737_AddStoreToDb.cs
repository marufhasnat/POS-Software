using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_AspNetUsers_UserId",
                table: "Stores");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Stores",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Stores_UserId",
                table: "Stores",
                newName: "IX_Stores_ManagerId");

            migrationBuilder.AddColumn<string>(
                name: "CashierId",
                table: "Stores",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CashierId",
                table: "Stores",
                column: "CashierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_AspNetUsers_CashierId",
                table: "Stores",
                column: "CashierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_AspNetUsers_ManagerId",
                table: "Stores",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_AspNetUsers_CashierId",
                table: "Stores");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_AspNetUsers_ManagerId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_CashierId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "CashierId",
                table: "Stores");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Stores",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Stores_ManagerId",
                table: "Stores",
                newName: "IX_Stores_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_AspNetUsers_UserId",
                table: "Stores",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
