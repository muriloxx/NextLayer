using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NextLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddFaqItensTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chamados_Clientes_ClienteId",
                table: "Chamados");

            migrationBuilder.DropForeignKey(
                name: "FK_Chamados_Funcionarios_AnalistaId",
                table: "Chamados");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagensChat_Clientes_ClienteRemetenteId",
                table: "MensagensChat");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagensChat_Funcionarios_FuncionarioRemetenteId",
                table: "MensagensChat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Funcionarios",
                table: "Funcionarios");

            migrationBuilder.DropIndex(
                name: "IX_Funcionarios_Email",
                table: "Funcionarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email",
                table: "Clientes");

            migrationBuilder.RenameTable(
                name: "Funcionarios",
                newName: "Employees");

            migrationBuilder.RenameTable(
                name: "Clientes",
                newName: "Clients");

            migrationBuilder.AlterColumn<bool>(
                name: "AnalistaInteragiu",
                table: "Chamados",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Employees",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Clients",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clients",
                table: "Clients",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FaqItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Pergunta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Resposta = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqItens", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Chamados_Clients_ClienteId",
                table: "Chamados",
                column: "ClienteId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chamados_Employees_AnalistaId",
                table: "Chamados",
                column: "AnalistaId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensChat_Clients_ClienteRemetenteId",
                table: "MensagensChat",
                column: "ClienteRemetenteId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensChat_Employees_FuncionarioRemetenteId",
                table: "MensagensChat",
                column: "FuncionarioRemetenteId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chamados_Clients_ClienteId",
                table: "Chamados");

            migrationBuilder.DropForeignKey(
                name: "FK_Chamados_Employees_AnalistaId",
                table: "Chamados");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagensChat_Clients_ClienteRemetenteId",
                table: "MensagensChat");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagensChat_Employees_FuncionarioRemetenteId",
                table: "MensagensChat");

            migrationBuilder.DropTable(
                name: "FaqItens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clients",
                table: "Clients");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "Funcionarios");

            migrationBuilder.RenameTable(
                name: "Clients",
                newName: "Clientes");

            migrationBuilder.AlterColumn<bool>(
                name: "AnalistaInteragiu",
                table: "Chamados",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Funcionarios",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Clientes",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Funcionarios",
                table: "Funcionarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_Email",
                table: "Funcionarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chamados_Clientes_ClienteId",
                table: "Chamados",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chamados_Funcionarios_AnalistaId",
                table: "Chamados",
                column: "AnalistaId",
                principalTable: "Funcionarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensChat_Clientes_ClienteRemetenteId",
                table: "MensagensChat",
                column: "ClienteRemetenteId",
                principalTable: "Clientes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensChat_Funcionarios_FuncionarioRemetenteId",
                table: "MensagensChat",
                column: "FuncionarioRemetenteId",
                principalTable: "Funcionarios",
                principalColumn: "Id");
        }
    }
}
