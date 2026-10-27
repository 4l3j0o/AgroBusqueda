using Microsoft.EntityFrameworkCore;
using AgroBusca.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddSingleton<AgroBusca.Api.Interfaces.IEmbeddingService, AgroBusca.Api.Services.EmbeddingService>();
builder.Services.AddScoped<AgroBusca.Api.Interfaces.IBusquedaService, AgroBusca.Api.Services.BusquedaService>();
builder.Services.AddScoped<AgroBusca.Api.Interfaces.IDocumentoService, AgroBusca.Api.Services.DocumentoService>();
builder.Services.AddScoped<AgroBusca.Api.Interfaces.IFichaTecnicaService, AgroBusca.Api.Services.FichaTecnicaService>();
builder.Services.AddScoped<AgroBusca.Api.Interfaces.IParcelaViticolaService, AgroBusca.Api.Services.ParcelaViticolaService>();
builder.Services.AddHttpClient<AgroBusca.Api.Interfaces.IRagService, AgroBusca.Api.Services.GroqRagService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

app.UseMiddleware<AgroBusca.Api.Middlewares.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await context.Database.MigrateAsync();
        }

        var documentoService = services.GetRequiredService<AgroBusca.Api.Interfaces.IDocumentoService>();
        await documentoService.InicializarDatosSemillaAsync();

        var fichaService = services.GetRequiredService<AgroBusca.Api.Interfaces.IFichaTecnicaService>();
        await fichaService.InicializarFichasSemillaAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Error conectando a la base de datos: {Message}", ex.Message);
    }
}

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    Aplicacion = "AgroBusca API",
    Version = "1.0.0",
    Estado = "En línea",
    Descripcion = "Buscador semántico vitícola de Tarija, Bolivia"
}));

app.Run();
