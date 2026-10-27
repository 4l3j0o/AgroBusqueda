export interface BusquedaRequest {
  pregunta: string;
  categoria?: string;
  topK: number;
  umbralMinimo: number;
  generarRespuestaIa?: boolean;
}

export interface ResultadoBusqueda {
  fragmentoId: number;
  documentoId: number;
  tituloDocumento: string;
  autorInstitucion: string;
  categoria: string;
  urlFuente?: string;
  subseccion?: string;
  contenido: string;
  similitud: number;
  fechaPublicacion: string;
}

export interface BusquedaResponse {
  consulta: string;
  totalRecuperados: number;
  tiempoRespuestaMs: number;
  sintesisIa?: string;
  modeloIa?: string;
  tiempoIaMs?: number;
  resultados: ResultadoBusqueda[];
  avisoAgronomico: string;
}

export interface DocumentoResponse {
  id: number;
  titulo: string;
  autorInstitucion: string;
  categoria: string;
  urlFuente?: string;
  fechaPublicacion: string;
  fechaRegistro: string;
  cantidadFragmentos: number;
  fragmentos?: {
    id: number;
    indiceChunk: number;
    subseccion?: string;
    contenido: string;
  }[];
}

export interface DocumentoCreateDto {
  titulo: string;
  autorInstitucion: string;
  categoria: string;
  urlFuente?: string;
  subseccion?: string;
  contenidoCompleto: string;
  fechaPublicacion?: string;
}

export interface EstadisticasResponse {
  totalDocumentos: number;
  totalFragmentosVectorizados: number;
  zonaGeografica: string;
  rubroPrincipal: string;
}

export interface FichaTecnica {
  id: number;
  titulo: string;
  cultivoVariedad: string;
  tipo: string;
  zona: string;
  descripcion: string;
  sintomasOIdentificacion: string;
  medidasManejo: string;
  requerimientosEdafoclimaticos: string;
  fuentesReferencia: string;
  fechaActualizacion: string;
}

export interface CrearFichaTecnica {
  titulo: string;
  cultivoVariedad: string;
  tipo: string;
  zona: string;
  descripcion: string;
  sintomasOIdentificacion: string;
  medidasManejo: string;
  requerimientosEdafoclimaticos: string;
  fuentesReferencia: string;
}

export interface FiltroFichas {
  terminoBusqueda?: string;
  tipo?: string;
  cultivoVariedad?: string;
  zona?: string;
}

export interface Coordenada {
  lng: number;
  lat: number;
}

export interface DisenoParcelaRequest {
  nombre: string;
  variedadVid: string;
  ubicacion: string;
  poligono: Coordenada[];
  distanciaEntreFilas: number;
  distanciaEntrePlantas: number;
  orientacionGrados: number;
  margenCabeceraMetros: number;
  guardarEnBaseDatos: boolean;
}

export interface HileraViticola {
  indice: number;
  longitudMetros: number;
  cantidadPlantas: number;
  segmento: Coordenada[];
}

export interface PlantaViticola {
  hileraIndice: number;
  plantaIndice: number;
  lng: number;
  lat: number;
}

export interface MetricasViticola {
  areaMetrosCuadrados: number;
  areaHectareas: number;
  totalFilas: number;
  totalPlantas: number;
  densidadPlantasPorHectarea: number;
  longitudLinealTotalMetros: number;
  alambreEspalderaEstimadoMetros: number;
  postesIntermediosEstimados: number;
  postesCabecerosEstimados: number;
}

export interface DisenoParcelaResponse {
  id?: number;
  nombre: string;
  variedadVid: string;
  ubicacion: string;
  distanciaEntreFilas: number;
  distanciaEntrePlantas: number;
  orientacionGrados: number;
  margenCabeceraMetros: number;
  metricas: MetricasViticola;
  hileras: HileraViticola[];
  plantas: PlantaViticola[];
  poligonoGeoJson: string;
  filasGeoJson: string;
  plantasGeoJson: string;
  fechaCreacion: string;
}

export interface ParcelaResumen {
  id: number;
  nombre: string;
  variedadVid: string;
  ubicacion: string;
  areaMetrosCuadrados: number;
  areaHectareas: number;
  totalFilas: number;
  totalPlantas: number;
  distanciaEntreFilas: number;
  distanciaEntrePlantas: number;
  orientacionGrados: number;
  fechaCreacion: string;
}
