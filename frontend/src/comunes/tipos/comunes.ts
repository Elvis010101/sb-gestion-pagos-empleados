/**
 * Contratos que la API comparte entre todos sus módulos.
 */

/**
 * Página de resultados. Es el espejo de `PaginaDto<T>` de la capa Aplicación.
 *
 * `totalPaginas` llega calculado desde el servidor: no se recalcula aquí a propósito, porque
 * dos implementaciones de la misma división con redondeo hacia arriba terminan discrepando
 * en el caso borde (cero registros, o un registro exacto de más).
 */
export interface PaginaDto<T> {
  elementos: T[];
  totalRegistros: number;
  pagina: number;
  tamanoPagina: number;
  totalPaginas: number;
}

/**
 * Cuerpo de error de la API, con el formato ProblemDetails de la RFC 7807.
 *
 * Todas las respuestas de error del backend tienen esta forma —las de los controladores, las
 * del filtro de validación y las de los middlewares—, así que el frontend escribe un solo
 * traductor de errores y no uno por endpoint.
 */
export interface ProblemaApi {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;

  /**
   * Errores de validación por campo, presente solo en las respuestas 400 que produce
   * FluentValidation. La clave es el nombre de la propiedad tal como la nombró el validador.
   */
  errors?: Record<string, string[]>;

  /**
   * Identificador de correlación que el backend agrega a todo ProblemDetails. Es el mismo
   * valor que aparece en el archivo de registro de Serilog, así que es lo único que hay que
   * pedirle al usuario cuando reporta un fallo.
   */
  idCorrelacion?: string;

  /** Rastro de pila. El backend solo lo emite en el entorno de Desarrollo. */
  detalleTecnico?: string;
}
