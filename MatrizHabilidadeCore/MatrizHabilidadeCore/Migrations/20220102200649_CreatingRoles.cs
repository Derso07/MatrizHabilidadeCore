using Microsoft.EntityFrameworkCore.Migrations;

namespace MatrizHabilidadeCore.Migrations
{
    public partial class CreatingRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsColaborador",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "error",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "MASTER", "94fc8159-4b1f-4c72-adb6-45d2e3a5c3e4", "Master", "MASTER" },
                    { "ADMINISTRADOR", "3cdd034c-5f32-4215-a5ca-7af461d49a35", "Administrador", "ADMINISTRADOR" },
                    { "COORDENADOR", "e6705d01-9598-4365-9fdc-eb0667e23eee", "Coordenador", "COORDENADOR" },
                    { "FUNCIONARIO", "f0b48c09-75bc-48c8-af82-9a8d42537e70", "Funcionario", "FUNCIONARIO" },
                    { "NONE", "648eadd8-a130-4425-9ce7-97ec8b83b403", "None", "NONE" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ColaboradorId",
                table: "AspNetUsers",
                column: "ColaboradorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CoordenadorId",
                table: "AspNetUsers",
                column: "CoordenadorId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Colaborador_ColaboradorId",
                table: "AspNetUsers",
                column: "ColaboradorId",
                principalTable: "Colaborador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Coordenador_CoordenadorId",
                table: "AspNetUsers",
                column: "CoordenadorId",
                principalTable: "Coordenador",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Colaborador_ColaboradorId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Coordenador_CoordenadorId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ColaboradorId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CoordenadorId",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ADMINISTRADOR");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "COORDENADOR");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "FUNCIONARIO");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "MASTER");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "NONE");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "error");

            migrationBuilder.AddColumn<bool>(
                name: "IsColaborador",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: true);
        }
    }
}
