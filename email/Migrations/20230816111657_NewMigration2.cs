using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace email.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Recipients_RecipientId",
                table: "Campaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns");

            migrationBuilder.RenameTable(
                name: "Campaigns",
                newName: "Campaign");

            migrationBuilder.RenameIndex(
                name: "IX_Campaigns_RecipientId",
                table: "Campaign",
                newName: "IX_Campaign_RecipientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Campaign",
                table: "Campaign",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaign_Recipients_RecipientId",
                table: "Campaign",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "RecipientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaign_Recipients_RecipientId",
                table: "Campaign");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Campaign",
                table: "Campaign");

            migrationBuilder.RenameTable(
                name: "Campaign",
                newName: "Campaigns");

            migrationBuilder.RenameIndex(
                name: "IX_Campaign_RecipientId",
                table: "Campaigns",
                newName: "IX_Campaigns_RecipientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Recipients_RecipientId",
                table: "Campaigns",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "RecipientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
