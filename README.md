# AgroBusca &mdash; Buscador Semántico Vitícola de Tarija

**AgroBusca** es una plataforma web de búsqueda semántica de información agrícola orientada a la viticultura y producción de uva en el Valle Central de Tarija, Bolivia (Uriondo, Calamuchita, Santa Ana, San Lorenzo).

Permite a productores, técnicos y estudiantes realizar preguntas en lenguaje natural (ej. *"las hojas de la vid se ponen amarillas"*) y recuperar conocimiento agronómico por su **significado semántico** (relacionándolo con clorosis férrica, deficiencia de nutrientes o manejo de suelos), aunque la consulta no comparta las mismas palabras exactas.

> **Aviso Agronómico:** La información presentada proviene de fuentes técnicas y científicas (CENAVIT, INIAF Tarija, FAUTAPO, ANIV) y tiene fines estrictamente orientativos. No sustituye la inspección de campo ni constituye un diagnóstico agrícola definitivo.

---

## 🛠️ Tecnologías Empleadas

- **Frontend**: Angular 22 con TypeScript, interfaz moderna vitícola con tarjeta de síntesis RAG.
- **Backend**: ASP.NET Core Web API con **.NET 10** y C# 13/14.
- **Inteligencia Artificial (RAG)**: Integración con **Groq API** (`openai/gpt-oss-120b` / `qwen/qwen3.8-27b`) para síntesis técnica explicativa citando fuentes.
- **Acceso a Datos**: Entity Framework Core 10 con arquitectura por capas.
- **Base de Datos**: PostgreSQL con almacenamiento vectorial e indexación de fragmentos.
- **Embeddings**: Servicio modular y configurable (`IEmbeddingService`) con afinidad semántica vitícola y normalización L2.
- **Infraestructura**: Docker Compose para PostgreSQL con pgvector y soporte para PostgreSQL local.

---

## 🏛️ Arquitectura del Backend (.NET 10)

```text
backend/AgroBusca.Api/
├── Controllers/
│   ├── BusquedaController.cs       # POST /api/busqueda, GET categorias, sugerencias
│   └── DocumentosController.cs     # GET /api/documentos, POST, estadísticas, inicializar-semilla
├── Data/
│   └── AppDbContext.cs             # DbContext con HasPostgresExtension("vector") e índice HNSW
├── DTOs/
│   ├── BusquedaRequestDto.cs       # Pregunta, categoría, Top-K, umbral
│   ├── ResultadoBusquedaDto.cs     # Fragmento, similitud, fuente, aviso agronómico
│   └── DocumentoDto.cs             # Creación y respuesta de fuentes
├── Interfaces/
│   ├── IEmbeddingService.cs        # Abstracción para generar vectores
│   ├── IBusquedaService.cs         # Lógica con EF.Functions.CosineDistance
│   └── IDocumentoService.cs        # Ingesta, chunking y catálogo técnico
├── Mappings/
│   └── DocumentoMappingExtensions.cs # Mapeo entre entidades de EF y DTOs
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs # Respuestas JSON de error estandarizadas
├── Migrations/
│   └── 20261026153330_MigracionInicial.cs # Migración con pgvector
├── Models/
│   ├── Documento.cs                # Metadatos del documento fuente
│   └── FragmentoDocumento.cs       # Chunk textual y Vector de 384 dimensiones
├── Services/
│   ├── EmbeddingService.cs         # Proveedor determinista con afinidad contextual
│   ├── BusquedaService.cs          # Consulta vectorial optimizada
│   └── DocumentoService.cs         # Fragmentación y datos semilla de Tarija
└── Program.cs                      # Inyección de dependencias, CORS y pipeline HTTP
```

---

## 🚀 Guía de Ejecución

### 1. Base de Datos (PostgreSQL)

Tienes dos opciones:

#### Opción A: Usar PostgreSQL local
Si utilizas tu servicio local de PostgreSQL en el puerto `5432`, ajusta la contraseña de tu usuario `postgres` en `backend/AgroBusca.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=agrobusca_db;Username=postgres;Password=TU_PASSWORD;"
}
```

#### Opción B: Usar Docker con pgvector
```bash
docker compose up -d
```
El contenedor levantará PostgreSQL 16 con `pgvector` preinstalado en el puerto `5433` (configurable en `.env`).

---

### 2. Ejecutar el Backend (.NET 10 Web API)

Abre una terminal en la raíz del proyecto y ejecuta:

```bash
cd backend/AgroBusca.Api
dotnet run --launch-profile http
```

La API estará escuchando en:
- `http://localhost:5082`
- Diagnóstico general: `http://localhost:5082/`
- Sugerencias vitícolas: `http://localhost:5082/api/busqueda/sugerencias`
- Estadísticas del corpus: `http://localhost:5082/api/documentos/estadisticas`

---

### 3. Ejecutar el Frontend (Angular)

En otra terminal, ejecuta:

```bash
cd frontend/agro-busca-web
npm start
```

Abre tu navegador en `http://localhost:4200` para interactuar con la aplicación.
