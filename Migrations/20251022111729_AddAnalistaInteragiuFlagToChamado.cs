using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalistaInteragiuFlagToChamado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AnalistaInteragiu",
                table: "Chamados",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalistaInteragiu",
                table: "Chamados");
        }
    }
}
