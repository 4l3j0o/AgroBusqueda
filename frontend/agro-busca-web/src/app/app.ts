import { Component, OnInit, OnDestroy, inject, signal, effect, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as L from 'leaflet';
import { AgroBuscaService } from './services/agro-busca.service';
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
  ParcelaResumen,
  Coordenada
} from './models/agro-busca.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit, OnDestroy {
  private readonly agroService = inject(AgroBuscaService);

  pestanaActiva: 'inicio' | 'busqueda' | 'fichas' | 'documentos' | 'parcelas' = 'inicio';
  backendOnline: boolean = true;

  pregunta: string = '';
  categoriaSeleccionada: string = 'Todas';
  topK: number = 5;
  umbralMinimo: number = 0.20;
  generarIa: boolean = true;
  cargandoBusqueda: boolean = false;
  errorBusqueda: string | null = null;
  respuestaBusqueda: BusquedaResponse | null = null;
  categorias: string[] = ['Todas'];
  sugerencias: string[] = [];

  fichas: FichaTecnica[] = [];
  cargandoFichas: boolean = false;
  filtroFichas: FiltroFichas = { tipo: '', terminoBusqueda: '', cultivoVariedad: '', zona: '' };
  fichaSeleccionada: FichaTecnica | null = null;
  mostrandoFormFicha: boolean = false;
  nuevaFicha: CrearFichaTecnica = {
    titulo: '',
    cultivoVariedad: 'Moscatel de Alejandría',
    tipo: 'Variedad',
    zona: 'Valle Central de Tarija',
    descripcion: '',
    sintomasOIdentificacion: '',
    medidasManejo: '',
    requerimientosEdafoclimaticos: '',
    fuentesReferencia: 'CENAVIT / INIAF Tarija'
  };
  mensajeFichas: string | null = null;

  documentos: DocumentoResponse[] = [];
  cargandoDocs: boolean = false;
  estadisticas: EstadisticasResponse | null = null;
  mostrandoFormDoc: boolean = false;
  nuevoDoc: DocumentoCreateDto = {
    titulo: '',
    autorInstitucion: 'CENAVIT Tarija',
    categoria: 'Sanidad Vegetal',
    subseccion: 'Recomendaciones Técnicas',
    contenidoCompleto: '',
    urlFuente: ''
  };
  mensajeDocs: string | null = null;

  private map: L.Map | null = null;
  private polygonLayer: L.Polygon | null = null;
  private rowsLayerGroup: L.LayerGroup | null = null;
  private plantsLayerGroup: L.LayerGroup | null = null;
  private drawMarkersGroup: L.LayerGroup | null = null;

  verticesDibujo: Coordenada[] = [];
  modoDibujoActivo: boolean = false;

  parametrosParcela: DisenoParcelaRequest = {
    nombre: 'Cuartel Moscatel - Uriondo',
    variedadVid: 'Moscatel de Alejandría',
    ubicacion: 'Valle de la Concepción, Tarija',
    poligono: [],
    distanciaEntreFilas: 2.5,
    distanciaEntrePlantas: 1.2,
    orientacionGrados: 0.0,
    margenCabeceraMetros: 1.5,
    guardarEnBaseDatos: true
  };

  calculandoParcela: boolean = false;
  errorParcela: string | null = null;
  resultadoParcela: DisenoParcelaResponse | null = null;
  parcelasGuardadas: ParcelaResumen[] = [];
  mostrarPlantasEnMapa: boolean = true;

  parcelasEjemplo = [
    {
      nombre: 'Cuartel Uriondo (Moscatel - 1.2 Ha)',
      variedad: 'Moscatel de Alejandría',
      ubicacion: 'Valle de la Concepción',
      distanciaFilas: 2.5,
      distanciaPlantas: 1.2,
      orientacion: 0,
      coords: [
        { lng: -64.7315, lat: -21.5830 },
        { lng: -64.7295, lat: -21.5828 },
        { lng: -64.7293, lat: -21.5885 },
        { lng: -64.7318, lat: -21.5888 }
      ]
    },
    {
      nombre: 'Finca Santa Ana (Tannat de Altura - 0.8 Ha)',
      variedad: 'Tannat',
      ubicacion: 'Santa Ana la Nueva, Tarija',
      distanciaFilas: 2.3,
      distanciaPlantas: 1.0,
      orientacion: 20,
      coords: [
        { lng: -64.6750, lat: -21.5320 },
        { lng: -64.6725, lat: -21.5315 },
        { lng: -64.6720, lat: -21.5360 },
        { lng: -64.6748, lat: -21.5365 }
      ]
    },
    {
      nombre: 'Viña Calamuchita (Syrah Espaldera)',
      variedad: 'Syrah',
      ubicacion: 'Calamuchita, Uriondo',
      distanciaFilas: 2.6,
      distanciaPlantas: 1.25,
      orientacion: 350,
      coords: [
        { lng: -64.7180, lat: -21.6150 },
        { lng: -64.7150, lat: -21.6145 },
        { lng: -64.7145, lat: -21.6190 },
        { lng: -64.7185, lat: -21.6195 }
      ]
    }
  ];

  ngOnInit(): void {
    this.cargarCategorias();
    this.cargarSugerencias();
    this.cargarEstadisticas();
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
      this.map = null;
    }
  }

  cambiarPestana(pestana: 'inicio' | 'busqueda' | 'fichas' | 'documentos' | 'parcelas'): void {
    this.pestanaActiva = pestana;

    if (pestana === 'fichas' && this.fichas.length === 0) {
      this.cargarFichas();
    } else if (pestana === 'documentos' && this.documentos.length === 0) {
      this.cargarDocumentos();
    } else if (pestana === 'parcelas') {
      this.cargarParcelasGuardadas();
      setTimeout(() => this.inicializarMapa(), 150);
    }
  }

  lanzarBusquedaRapida(consulta: string): void {
    this.pregunta = consulta;
    this.cambiarPestana('busqueda');
    this.ejecutarBusqueda();
  }

  abrirDisenoConEjemplo(indice: number): void {
    this.cambiarPestana('parcelas');
    setTimeout(() => {
      if (this.parcelasEjemplo[indice]) {
        this.cargarParcelaEjemplo(this.parcelasEjemplo[indice]);
      }
    }, 200);
  }

  cargarCategorias(): void {
    this.agroService.obtenerCategorias().subscribe({
      next: (cats) => this.categorias = ['Todas', ...cats]
    });
  }

  cargarSugerencias(): void {
    this.agroService.obtenerSugerencias().subscribe({
      next: (sug) => this.sugerencias = sug
    });
  }

  ejecutarBusqueda(): void {
    if (!this.pregunta.trim() || this.pregunta.trim().length < 3) {
      this.errorBusqueda = 'Por favor escribe una consulta de al menos 3 caracteres.';
      return;
    }

    this.cargandoBusqueda = true;
    this.errorBusqueda = null;

    const request: BusquedaRequest = {
      pregunta: this.pregunta.trim(),
      categoria: this.categoriaSeleccionada === 'Todas' ? undefined : this.categoriaSeleccionada,
      topK: this.topK,
      umbralMinimo: this.umbralMinimo,
      generarRespuestaIa: this.generarIa
    };

    this.agroService.buscar(request).subscribe({
      next: (res) => {
        this.respuestaBusqueda = res;
        this.cargandoBusqueda = false;
      },
      error: () => {
        this.cargandoBusqueda = false;
        this.errorBusqueda = 'No se pudo conectar con el servicio de búsqueda. Verifica que el backend esté en ejecución.';
      }
    });
  }

  aplicarSugerencia(texto: string): void {
    this.pregunta = texto;
    this.pestanaActiva = 'busqueda';
    this.ejecutarBusqueda();
  }

  seleccionarCategoria(cat: string): void {
    this.categoriaSeleccionada = cat;
    if (this.pregunta.trim().length >= 3) {
      this.ejecutarBusqueda();
    }
  }

  limpiarBusqueda(): void {
    this.pregunta = '';
    this.respuestaBusqueda = null;
    this.errorBusqueda = null;
  }

  cargarFichas(): void {
    this.cargandoFichas = true;
    this.agroService.obtenerFichas(this.filtroFichas).subscribe({
      next: (res) => {
        this.fichas = res;
        this.cargandoFichas = false;
      },
      error: () => {
        this.cargandoFichas = false;
      }
    });
  }

  filtrarFichasPorTipo(tipo: string): void {
    this.filtroFichas.tipo = tipo === 'Todos' ? '' : tipo;
    this.cargarFichas();
  }

  verDetalleFicha(ficha: FichaTecnica): void {
    this.fichaSeleccionada = ficha;
  }

  cerrarDetalleFicha(): void {
    this.fichaSeleccionada = null;
  }

  guardarNuevaFicha(): void {
    if (!this.nuevaFicha.titulo.trim() || !this.nuevaFicha.descripcion.trim()) {
      alert('El título y la descripción son obligatorios.');
      return;
    }

    this.agroService.crearFicha(this.nuevaFicha).subscribe({
      next: (creada) => {
        this.mostrandoFormFicha = false;
        this.mensajeFichas = `Ficha "${creada.titulo}" guardada correctamente.`;
        this.cargarFichas();
        setTimeout(() => this.mensajeFichas = null, 3500);
      },
      error: () => {
        alert('Error al guardar la ficha técnica.');
      }
    });
  }

  eliminarFicha(id: number, event: Event): void {
    event.stopPropagation();
    if (!confirm('¿Estás seguro de eliminar esta ficha técnica?')) return;

    this.agroService.eliminarFicha(id).subscribe({
      next: () => {
        if (this.fichaSeleccionada?.id === id) this.fichaSeleccionada = null;
        this.cargarFichas();
      }
    });
  }

  inicializarFichasSemilla(): void {
    this.mensajeFichas = 'Inicializando catálogo oficial de fichas vitícolas...';
    this.agroService.inicializarFichasSemilla().subscribe({
      next: () => {
        this.cargarFichas();
        this.mensajeFichas = 'Catálogo de fichas vitícolas inicializado con éxito.';
        setTimeout(() => this.mensajeFichas = null, 3500);
      }
    });
  }

  cargarDocumentos(): void {
    this.cargandoDocs = true;
    this.agroService.obtenerDocumentos().subscribe({
      next: (docs) => {
        this.documentos = docs;
        this.cargandoDocs = false;
      },
      error: () => this.cargandoDocs = false
    });
  }

  cargarEstadisticas(): void {
    this.agroService.obtenerEstadisticas().subscribe({
      next: (est) => this.estadisticas = est
    });
  }

  guardarNuevoDocumento(): void {
    if (!this.nuevoDoc.titulo.trim() || !this.nuevoDoc.contenidoCompleto.trim()) {
      alert('El título y el contenido completo son obligatorios para fragmentar y vectorizar.');
      return;
    }

    this.agroService.crearDocumento(this.nuevoDoc).subscribe({
      next: (creado) => {
        this.mostrandoFormDoc = false;
        this.mensajeDocs = `Documento "${creado.titulo}" procesado con ${creado.cantidadFragmentos} fragmentos vectorizados.`;
        this.cargarDocumentos();
        this.cargarEstadisticas();
        setTimeout(() => this.mensajeDocs = null, 4000);
      },
      error: () => alert('Error al crear y vectorizar el documento.')
    });
  }

  eliminarDocumento(id: number): void {
    if (!confirm('¿Deseas eliminar este documento y todos sus vectores asociados?')) return;

    this.agroService.eliminarDocumento(id).subscribe({
      next: () => {
        this.cargarDocumentos();
        this.cargarEstadisticas();
      }
    });
  }

  inicializarDatosSemilla(): void {
    this.mensajeDocs = 'Inicializando corpus de viticultura de Tarija...';
    this.agroService.inicializarSemilla().subscribe({
      next: (res) => {
        this.mensajeDocs = res.mensaje || 'Datos semilla cargados con éxito.';
        this.cargarDocumentos();
        this.cargarEstadisticas();
        setTimeout(() => this.mensajeDocs = null, 4000);
      }
    });
  }

  private inicializarMapa(): void {
    const contenedor = document.getElementById('mapa-viticola');
    if (!contenedor) return;

    if (this.map) {
      this.map.invalidateSize();
      return;
    }

    this.map = L.map('mapa-viticola', {
      center: [-21.5850, -64.7300],
      zoom: 15,
      zoomControl: true
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap | AgroBusca Vitícola Tarija',
      maxZoom: 19
    }).addTo(this.map);

    this.rowsLayerGroup = L.layerGroup().addTo(this.map);
    this.plantsLayerGroup = L.layerGroup().addTo(this.map);
    this.drawMarkersGroup = L.layerGroup().addTo(this.map);

    this.map.on('click', (e: L.LeafletMouseEvent) => {
      if (this.modoDibujoActivo) {
        this.agregarVerticeManual(e.latlng.lng, e.latlng.lat);
      }
    });

    this.cargarParcelaEjemplo(this.parcelasEjemplo[0]);
  }

  cargarParcelasGuardadas(): void {
    this.agroService.obtenerParcelas().subscribe({
      next: (list) => this.parcelasGuardadas = list
    });
  }

  cargarParcelaEjemplo(ejemplo: any): void {
    this.parametrosParcela.nombre = ejemplo.nombre;
    this.parametrosParcela.variedadVid = ejemplo.variedad;
    this.parametrosParcela.ubicacion = ejemplo.ubicacion;
    this.parametrosParcela.distanciaEntreFilas = ejemplo.distanciaFilas;
    this.parametrosParcela.distanciaEntrePlantas = ejemplo.distanciaPlantas;
    this.parametrosParcela.orientacionGrados = ejemplo.orientacion;
    this.parametrosParcela.poligono = ejemplo.coords.map((c: any) => ({ lng: c.lng, lat: c.lat }));

    this.modoDibujoActivo = false;
    this.verticesDibujo = [...this.parametrosParcela.poligono];
    this.dibujarPoligonoEnMapa(this.parametrosParcela.poligono);
    this.ejecutarCalculoParcela();
  }

  activarModoDibujo(): void {
    this.modoDibujoActivo = true;
    this.verticesDibujo = [];
    this.parametrosParcela.poligono = [];
    this.limpiarCapasMapa();
    if (this.polygonLayer && this.map) {
      this.map.removeLayer(this.polygonLayer);
      this.polygonLayer = null;
    }
  }

  private agregarVerticeManual(lng: number, lat: number): void {
    this.verticesDibujo.push({ lng, lat });

    if (this.map && this.drawMarkersGroup) {
      const marker = L.circleMarker([lat, lng], {
        radius: 6,
        color: '#581845',
        fillColor: '#d4a373',
        fillOpacity: 0.9,
        weight: 2
      });
      this.drawMarkersGroup.addLayer(marker);
    }

    if (this.verticesDibujo.length >= 3) {
      this.dibujarPoligonoEnMapa(this.verticesDibujo);
      this.parametrosParcela.poligono = [...this.verticesDibujo];
    }
  }

  cerrarPoligonoManual(): void {
    if (this.verticesDibujo.length < 3) {
      alert('Debes marcar al menos 3 puntos en el mapa para formar la parcela.');
      return;
    }
    this.modoDibujoActivo = false;
    this.parametrosParcela.poligono = [...this.verticesDibujo];
    this.dibujarPoligonoEnMapa(this.parametrosParcela.poligono);
    this.ejecutarCalculoParcela();
  }

  limpiarDibujo(): void {
    this.verticesDibujo = [];
    this.parametrosParcela.poligono = [];
    this.limpiarCapasMapa();
    if (this.polygonLayer && this.map) {
      this.map.removeLayer(this.polygonLayer);
      this.polygonLayer = null;
    }
    this.resultadoParcela = null;
  }

  private dibujarPoligonoEnMapa(coords: Coordenada[]): void {
    if (!this.map) return;

    if (this.polygonLayer) {
      this.map.removeLayer(this.polygonLayer);
      this.polygonLayer = null;
    }

    const latLngs = coords.map(c => [c.lat, c.lng] as [number, number]);

    this.polygonLayer = L.polygon(latLngs, {
      color: '#2d6a4f',
      weight: 3,
      fillColor: '#52b788',
      fillOpacity: 0.2
    }).addTo(this.map);

    this.map.fitBounds(this.polygonLayer.getBounds(), { padding: [30, 30] });
  }

  ejecutarCalculoParcela(): void {
    if (this.parametrosParcela.poligono.length < 3) {
      this.errorParcela = 'Define o selecciona una parcela con al menos 3 vértices antes de calcular.';
      return;
    }

    this.calculandoParcela = true;
    this.errorParcela = null;

    this.agroService.disenarParcela(this.parametrosParcela).subscribe({
      next: (res) => {
        this.resultadoParcela = res;
        this.calculandoParcela = false;
        this.renderizarHilerasYPlantas(res);
        this.cargarParcelasGuardadas();
      },
      error: (err) => {
        this.calculandoParcela = false;
        this.errorParcela = err?.error?.mensaje || 'Error al calcular el diseño vitícola de la parcela.';
      }
    });
  }

  private renderizarHilerasYPlantas(res: DisenoParcelaResponse): void {
    if (!this.map) return;
    this.limpiarCapasMapa();

    if (this.rowsLayerGroup && res.hileras) {
      for (const h of res.hileras) {
        if (h.segmento.length >= 2) {
          const latLngs: [number, number][] = h.segmento.map(s => [s.lat, s.lng]);
          const line = L.polyline(latLngs, {
            color: '#7b2cbf',
            weight: 2.5,
            opacity: 0.85
          });
          line.bindTooltip(`Hilera #${h.indice}: ${h.longitudMetros} m (${h.cantidadPlantas} cepas)`, {
            sticky: true,
            className: 'tooltip-hilera'
          });
          this.rowsLayerGroup.addLayer(line);
        }
      }
    }

    if (this.plantsLayerGroup && this.mostrarPlantasEnMapa && res.plantas) {
      for (const p of res.plantas) {
        const marker = L.circleMarker([p.lat, p.lng], {
          radius: 2.5,
          color: '#1b4332',
          fillColor: '#40916c',
          fillOpacity: 0.9,
          weight: 1
        });
        this.plantsLayerGroup.addLayer(marker);
      }
    }
  }

  toggleMostrarPlantas(): void {
    this.mostrarPlantasEnMapa = !this.mostrarPlantasEnMapa;
    if (this.resultadoParcela) {
      this.renderizarHilerasYPlantas(this.resultadoParcela);
    }
  }

  cargarParcelaPorId(id: number): void {
    this.agroService.obtenerParcelaPorId(id).subscribe({
      next: (res) => {
        this.resultadoParcela = res;
        this.parametrosParcela.nombre = res.nombre;
        this.parametrosParcela.variedadVid = res.variedadVid;
        this.parametrosParcela.ubicacion = res.ubicacion;
        this.parametrosParcela.distanciaEntreFilas = res.distanciaEntreFilas;
        this.parametrosParcela.distanciaEntrePlantas = res.distanciaEntrePlantas;
        this.parametrosParcela.orientacionGrados = res.orientacionGrados;

        try {
          const geo = JSON.parse(res.poligonoGeoJson);
          const coords = geo.geometry.coordinates[0];
          this.parametrosParcela.poligono = coords.map((c: any) => ({ lng: c[0], lat: c[1] }));
          this.dibujarPoligonoEnMapa(this.parametrosParcela.poligono);
          this.renderizarHilerasYPlantas(res);
        } catch (e) {
          console.error('Error al parsear polígono guardado', e);
        }
      }
    });
  }

  eliminarParcelaGuardada(id: number, event: Event): void {
    event.stopPropagation();
    if (!confirm('¿Deseas eliminar esta parcela guardada?')) return;

    this.agroService.eliminarParcela(id).subscribe({
      next: () => {
        if (this.resultadoParcela?.id === id) {
          this.limpiarDibujo();
        }
        this.cargarParcelasGuardadas();
      }
    });
  }

  private limpiarCapasMapa(): void {
    if (this.rowsLayerGroup) this.rowsLayerGroup.clearLayers();
    if (this.plantsLayerGroup) this.plantsLayerGroup.clearLayers();
    if (this.drawMarkersGroup) this.drawMarkersGroup.clearLayers();
  }
}
