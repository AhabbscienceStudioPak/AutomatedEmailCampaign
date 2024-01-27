using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace email.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Recipients_RecipientId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Emails_Recipients_RecipientId",
                table: "Emails");

            migrationBuilder.DropIndex(
                name: "IX_Emails_RecipientId",
                table: "Emails");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_RecipientId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "CC",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Campaigns");

            migrationBuilder.RenameColumn(
                name: "RecipientId",
                table: "Campaigns",
                newName: "EmailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "Campaigns",
                newName: "RecipientId");

            migrationBuilder.AddColumn<string>(
                name: "CC",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Emails_RecipientId",
                table: "Emails",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_RecipientId",
                table: "Campaigns",
                column: "RecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Recipients_RecipientId",
                table: "Campaigns",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "RecipientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_Recipients_RecipientId",
                table: "Emails",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "RecipientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
