using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgroBusca.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFichasTecnicasYParcelas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FichasTecnicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CultivoVariedad = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Zona = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    SintomasOIdentificacion = table.Column<string>(type: "text", nullable: false),
                    MedidasManejo = table.Column<string>(type: "text", nullable: false),
                    RequerimientosEdafoclimaticos = table.Column<string>(type: "text", nullable: false),
                    FuentesReferencia = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasTecnicas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParcelasViticolas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VariedadVid = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Ubicacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PoligonoGeoJson = table.Column<string>(type: "text", nullable: false),
                    DistanciaEntreFilas = table.Column<double>(type: "double precision", nullable: false),
                    DistanciaEntrePlantas = table.Column<double>(type: "double precision", nullable: false),
                    OrientacionGrados = table.Column<double>(type: "double precision", nullable: false),
                    MargenCabeceraMetros = table.Column<double>(type: "double precision", nullable: false),
                    AreaMetrosCuadrados = table.Column<double>(type: "double precision", nullable: false),
                    TotalFilas = table.Column<int>(type: "integer", nullable: false),
                    TotalPlantas = table.Column<int>(type: "integer", nullable: false),
                    MetrosLinealesTotales = table.Column<double>(type: "double precision", nullable: false),
                    FilasGeoJson = table.Column<string>(type: "text", nullable: false),
                    PlantasGeoJson = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelasViticolas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FichasTecnicas");

            migrationBuilder.DropTable(
                name: "ParcelasViticolas");
        }
    }
}
