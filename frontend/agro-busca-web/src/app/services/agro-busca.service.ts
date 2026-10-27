import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import {
  BusquedaRequest,
  BusquedaResponse,
  DocumentoResponse,
  DocumentoCreateDto,
  EstadisticasResponse,
  FichaTecnica,
  CrearFichaTecnica,
  FiltroFichas,
  DisenoParcelaRequest,
  DisenoParcelaResponse,
  ParcelaResumen
} from '../models/agro-busca.models';

@Injectable({
  providedIn: 'root'
})
export class AgroBuscaService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5082/api';

  buscar(request: BusquedaRequest): Observable<BusquedaResponse> {
    return this.http.post<BusquedaResponse>(`${this.apiUrl}/busqueda`, request);
  }

  obtenerCategorias(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/busqueda/categorias`).pipe(
      catchError(() => of([
        'Nutrición Vegetal',
        'Sanidad Vegetal',
        'Riego y Fertirriego',
        'Manejo del Viñedo'
      ]))
    );
  }

  obtenerSugerencias(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/busqueda/sugerencias`).pipe(
      catchError(() => of([
        'las hojas de la vid se ponen amarillas',
        'polvillo blanco ceniciento en las hojas y racimos',
        'manchas de aceite en las hojas después de lluvias',
        'riego por goteo y estrés hídrico en época de maduración',
        'poda en espaldera para uva Moscatel de Alejandría'
      ]))
    );
  }

  obtenerDocumentos(): Observable<DocumentoResponse[]> {
    return this.http.get<DocumentoResponse[]>(`${this.apiUrl}/documentos`);
  }

  obtenerDocumentoPorId(id: number): Observable<DocumentoResponse> {
    return this.http.get<DocumentoResponse>(`${this.apiUrl}/documentos/${id}`);
  }

  crearDocumento(dto: DocumentoCreateDto): Observable<DocumentoResponse> {
    return this.http.post<DocumentoResponse>(`${this.apiUrl}/documentos`, dto);
  }

  eliminarDocumento(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/documentos/${id}`);
  }

  obtenerEstadisticas(): Observable<EstadisticasResponse> {
    return this.http.get<EstadisticasResponse>(`${this.apiUrl}/documentos/estadisticas`).pipe(
      catchError(() => of({
        totalDocumentos: 5,
        totalFragmentosVectorizados: 15,
        zonaGeografica: 'Valle Central de Tarija, Bolivia',
        rubroPrincipal: 'Viticultura y Enología'
      }))
    );
  }

  inicializarSemilla(): Observable<any> {
    return this.http.post(`${this.apiUrl}/documentos/inicializar-semilla`, {});
  }

  obtenerFichas(filtro?: FiltroFichas): Observable<FichaTecnica[]> {
    let params = new HttpParams();
    if (filtro?.terminoBusqueda) params = params.set('terminoBusqueda', filtro.terminoBusqueda);
    if (filtro?.tipo) params = params.set('tipo', filtro.tipo);
    if (filtro?.cultivoVariedad) params = params.set('cultivoVariedad', filtro.cultivoVariedad);
    if (filtro?.zona) params = params.set('zona', filtro.zona);

    return this.http.get<FichaTecnica[]>(`${this.apiUrl}/fichastecnicas`, { params });
  }

  obtenerFichaPorId(id: number): Observable<FichaTecnica> {
    return this.http.get<FichaTecnica>(`${this.apiUrl}/fichastecnicas/${id}`);
  }

  crearFicha(dto: CrearFichaTecnica): Observable<FichaTecnica> {
    return this.http.post<FichaTecnica>(`${this.apiUrl}/fichastecnicas`, dto);
  }

  eliminarFicha(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/fichastecnicas/${id}`);
  }

  inicializarFichasSemilla(): Observable<any> {
    return this.http.post(`${this.apiUrl}/fichastecnicas/inicializar-semilla`, {});
  }

  disenarParcela(request: DisenoParcelaRequest): Observable<DisenoParcelaResponse> {
    return this.http.post<DisenoParcelaResponse>(`${this.apiUrl}/parcelas/disenar`, request);
  }

  obtenerParcelas(): Observable<ParcelaResumen[]> {
    return this.http.get<ParcelaResumen[]>(`${this.apiUrl}/parcelas`);
  }

  obtenerParcelaPorId(id: number): Observable<DisenoParcelaResponse> {
    return this.http.get<DisenoParcelaResponse>(`${this.apiUrl}/parcelas/${id}`);
  }

  eliminarParcela(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/parcelas/${id}`);
  }
}
