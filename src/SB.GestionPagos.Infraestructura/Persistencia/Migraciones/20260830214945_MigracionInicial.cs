using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SB.GestionPagos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrimerNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NumeroSeguroSocial = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoEmpleado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SalarioSemanal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    VentasBrutas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TarifaComision = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    SalarioBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SueldoPorHora = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    HorasTrabajadas = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HashContrasena = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Empleados",
                columns: new[] { "Id", "ApellidoPaterno", "Departamento", "Estado", "FechaCreacion", "NumeroSeguroSocial", "PrimerNombre", "SalarioSemanal", "TipoEmpleado" },
                values: new object[,]
                {
                    { 1, "Reyes", "Tecnología", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000001-1", "Ana", 32500.00m, "Asalariado" },
                    { 2, "Mejía", "Finanzas", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000002-2", "Carlos", 28000.00m, "Asalariado" },
                    { 3, "Fernández", "Recursos Humanos", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000003-3", "Lucía", 24500.00m, "Asalariado" },
                    { 4, "Guzmán", "Operaciones", 2, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000004-4", "Pedro", 21000.00m, "Asalariado" }
                });

            migrationBuilder.InsertData(
                table: "Empleados",
                columns: new[] { "Id", "ApellidoPaterno", "Departamento", "Estado", "FechaCreacion", "HorasTrabajadas", "NumeroSeguroSocial", "PrimerNombre", "SueldoPorHora", "TipoEmpleado" },
                values: new object[,]
                {
                    { 5, "Santos", "Operaciones", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 40.00m, "001-0000005-5", "María", 350.00m, "PorHoras" },
                    { 6, "Peña", "Operaciones", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 46.50m, "001-0000006-6", "José", 420.00m, "PorHoras" },
                    { 7, "Jiménez", "Tecnología", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 38.00m, "001-0000007-7", "Rosa", 550.00m, "PorHoras" },
                    { 8, "Castillo", "Operaciones", 2, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 40.00m, "001-0000008-8", "Miguel", 310.00m, "PorHoras" }
                });

            migrationBuilder.InsertData(
                table: "Empleados",
                columns: new[] { "Id", "ApellidoPaterno", "Departamento", "Estado", "FechaCreacion", "NumeroSeguroSocial", "PrimerNombre", "TarifaComision", "TipoEmpleado", "VentasBrutas" },
                values: new object[,]
                {
                    { 9, "Vargas", "Ventas", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000009-9", "Elena", 0.0800m, "PorComision", 185000.00m },
                    { 10, "Núñez", "Ventas", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000010-0", "Rafael", 0.0650m, "PorComision", 240500.00m },
                    { 11, "Ortiz", "Ventas", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000011-1", "Carmen", 0.1000m, "PorComision", 98750.50m },
                    { 12, "Polanco", "Ventas", 2, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000012-2", "Andrés", 0.0750m, "PorComision", 132000.00m }
                });

            migrationBuilder.InsertData(
                table: "Empleados",
                columns: new[] { "Id", "ApellidoPaterno", "Departamento", "Estado", "FechaCreacion", "NumeroSeguroSocial", "PrimerNombre", "SalarioBase", "TarifaComision", "TipoEmpleado", "VentasBrutas" },
                values: new object[,]
                {
                    { 13, "Rosario", "Ventas", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000013-3", "Patricia", 18000.00m, 0.0500m, "AsalariadoPorComision", 210000.00m },
                    { 14, "Almonte", "Ventas", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000014-4", "Luis", 20000.00m, 0.0450m, "AsalariadoPorComision", 156300.00m },
                    { 15, "Batista", "Ventas", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000015-5", "Sofía", 16500.00m, 0.0625m, "AsalariadoPorComision", 87400.25m },
                    { 16, "Encarnación", "Ventas", 1, new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "001-0000016-6", "Ramón", 25000.00m, 0.0350m, "AsalariadoPorComision", 305000.00m }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "HashContrasena", "NombreUsuario", "Rol" },
                values: new object[,]
                {
                    { 1, "$2a$12$iA7cg2RZfZbgY6/shX4Py.bPfze.s9Pv.YDJ7sJHKa9ds/MBsXHVa", "admin", 1 },
                    { 2, "$2a$12$RM7LaYWnevG9wPyWUyxQSe4C/3G3F0Ek7i.50I0zSurYpSIPlnWRa", "usuario", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_ApellidoPaterno",
                table: "Empleados",
                column: "ApellidoPaterno");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Departamento",
                table: "Empleados",
                column: "Departamento");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Estado",
                table: "Empleados",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_NumeroSeguroSocial",
                table: "Empleados",
                column: "NumeroSeguroSocial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
