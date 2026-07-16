using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace APMoodle.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementReads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Results_Quizzes_QuizID",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_QuizID",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "QuizID",
                table: "Results");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Quizzes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Announcements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Announcements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Announcements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                table: "Announcements",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnnouncementReads",
                columns: table => new
                {
                    AnnouncementReadID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    AnnouncementID = table.Column<int>(type: "integer", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementReads", x => x.AnnouncementReadID);
                    table.ForeignKey(
                        name: "FK_AnnouncementReads_Announcements_AnnouncementID",
                        column: x => x.AnnouncementID,
                        principalTable: "Announcements",
                        principalColumn: "AnnouncementID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_DeletedBy",
                table: "Announcements",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_LastModifiedBy",
                table: "Announcements",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_Status",
                table: "Announcements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementReads_AnnouncementID",
                table: "AnnouncementReads",
                column: "AnnouncementID");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementReads_UserID_AnnouncementID",
                table: "AnnouncementReads",
                columns: new[] { "UserID", "AnnouncementID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Admins_DeletedBy",
                table: "Announcements",
                column: "DeletedBy",
                principalTable: "Admins",
                principalColumn: "AdminID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Admins_LastModifiedBy",
                table: "Announcements",
                column: "LastModifiedBy",
                principalTable: "Admins",
                principalColumn: "AdminID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Admins_DeletedBy",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Admins_LastModifiedBy",
                table: "Announcements");

            migrationBuilder.DropTable(
                name: "AnnouncementReads");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_DeletedBy",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_LastModifiedBy",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_Status",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Announcements");

            migrationBuilder.AddColumn<int>(
                name: "QuizID",
                table: "Results",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_QuizID",
                table: "Results",
                column: "QuizID");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Quizzes_QuizID",
                table: "Results",
                column: "QuizID",
                principalTable: "Quizzes",
                principalColumn: "QuizID");
        }
    }
}
