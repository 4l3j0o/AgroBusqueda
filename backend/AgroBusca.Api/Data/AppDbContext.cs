using Microsoft.EntityFrameworkCore;
using AgroBusca.Api.Models;

namespace AgroBusca.Api.Data;

public class AppDbContext : DbContext
{
    public const int DimensionEmbedding = 384;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<FragmentoDocumento> FragmentosDocumento => Set<FragmentoDocumento>();
    public DbSet<FichaTecnica> FichasTecnicas => Set<FichaTecnica>();
    public DbSet<ParcelaViticola> ParcelasViticolas => Set<ParcelaViticola>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.ToTable("Documentos");
            entity.HasKey(d => d.Id);

            entity.Property(d => d.Titulo)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(d => d.AutorInstitucion)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(d => d.Categoria)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(d => d.UrlFuente)
                .HasMaxLength(500);

            entity.HasMany(d => d.Fragmentos)
                .WithOne(f => f.Documento)
                .HasForeignKey(f => f.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FragmentoDocumento>(entity =>
        {
            entity.ToTable("FragmentosDocumento");
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Contenido)
                .IsRequired();

            entity.Property(f => f.Subseccion)
                .HasMaxLength(200);

            entity.Property(f => f.Embedding)
                .HasColumnType("real[]")
                .IsRequired();
        });

        modelBuilder.Entity<FichaTecnica>(entity =>
        {
            entity.ToTable("FichasTecnicas");
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Titulo).IsRequired().HasMaxLength(250);
            entity.Property(f => f.CultivoVariedad).IsRequired().HasMaxLength(150);
            entity.Property(f => f.Tipo).IsRequired().HasMaxLength(80);
            entity.Property(f => f.Zona).IsRequired().HasMaxLength(150);
            entity.Property(f => f.FuentesReferencia).HasMaxLength(300);
        });

        modelBuilder.Entity<ParcelaViticola>(entity =>
        {
            entity.ToTable("ParcelasViticolas");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(p => p.VariedadVid).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Ubicacion).IsRequired().HasMaxLength(200);
            entity.Property(p => p.PoligonoGeoJson).IsRequired();
            entity.Property(p => p.FilasGeoJson).IsRequired();
            entity.Property(p => p.PlantasGeoJson).IsRequired();
        });
    }
}
