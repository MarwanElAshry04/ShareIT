using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareIT.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketTypesAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TicketTypes_TicketTypeId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "TicketTypes");

            migrationBuilder.RenameColumn(
                name: "TicketTypeId",
                table: "Tickets",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "Kind",
                table: "Tickets",
                newName: "TicketType");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_TicketTypeId",
                table: "Tickets",
                newName: "IX_Tickets_CategoryId");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplainType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    definition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComplainType_ar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    priority = table.Column<double>(type: "float", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackDetails",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    ResponsibleDepartmentId = table.Column<int>(type: "int", nullable: true),
                    ImpactArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredResolution = table.Column<int>(type: "int", nullable: false),
                    Confidentiality = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackDetails", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_FeedbackDetails_Departments_ResponsibleDepartmentId",
                        column: x => x.ResponsibleDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeedbackDetails_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdeaDetails",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    ImpactType = table.Column<int>(type: "int", nullable: false),
                    ResponsibleDepartmentId = table.Column<int>(type: "int", nullable: true),
                    ImpactDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sustainability = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeaDetails", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_IdeaDetails_Departments_ResponsibleDepartmentId",
                        column: x => x.ResponsibleDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdeaDetails_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueDetails",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    IssueType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImpactArea = table.Column<int>(type: "int", nullable: false),
                    ResponsibleDepartmentId = table.Column<int>(type: "int", nullable: true),
                    RootCause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProposedSolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolutionCategory = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueDetails", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_IssueDetails_Departments_ResponsibleDepartmentId",
                        column: x => x.ResponsibleDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IssueDetails_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectProposalDetails",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    BusinessNeed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedBenefits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoiReach = table.Column<int>(type: "int", nullable: false),
                    KeyStakeholders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimelineStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimelineEnd = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectProposalDetails", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_ProjectProposalDetails_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SafetyDetails",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    PotentialImpact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskLevel = table.Column<int>(type: "int", nullable: false),
                    HazardCategory = table.Column<int>(type: "int", nullable: false),
                    SuggestedAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsibleDepartmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyDetails", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_SafetyDetails_Departments_ResponsibleDepartmentId",
                        column: x => x.ResponsibleDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SafetyDetails_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackDetails_ResponsibleDepartmentId",
                table: "FeedbackDetails",
                column: "ResponsibleDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IdeaDetails_ResponsibleDepartmentId",
                table: "IdeaDetails",
                column: "ResponsibleDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueDetails_ResponsibleDepartmentId",
                table: "IssueDetails",
                column: "ResponsibleDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyDetails_ResponsibleDepartmentId",
                table: "SafetyDetails",
                column: "ResponsibleDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Categories_CategoryId",
                table: "Tickets",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Categories_CategoryId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "FeedbackDetails");

            migrationBuilder.DropTable(
                name: "IdeaDetails");

            migrationBuilder.DropTable(
                name: "IssueDetails");

            migrationBuilder.DropTable(
                name: "ProjectProposalDetails");

            migrationBuilder.DropTable(
                name: "SafetyDetails");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "TicketType",
                table: "Tickets",
                newName: "Kind");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Tickets",
                newName: "TicketTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_CategoryId",
                table: "Tickets",
                newName: "IX_Tickets_TicketTypeId");

            migrationBuilder.CreateTable(
                name: "TicketTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplainType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComplainType_ar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    definition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    priority = table.Column<double>(type: "float", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTypes", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TicketTypes_TicketTypeId",
                table: "Tickets",
                column: "TicketTypeId",
                principalTable: "TicketTypes",
                principalColumn: "Id");
        }
    }
}
