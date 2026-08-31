import axios from 'axios';

import type { ProblemaApi } from '../tipos/comunes';

/**
 * Códigos de estado HTTP que el frontend distingue. Nombrarlos evita que aparezcan literales
 * como `401` sueltos en los componentes, donde nadie recuerda qué significaba cada número.
 */
export const CodigoEstadoHttp = {
  SolicitudIncorrecta: 400,
  NoAutorizado: 401,
  Prohibido: 403,
  NoEncontrado: 404,
  Conflicto: 409,
  DemasiadasPeticiones: 429,
  ErrorInterno: 500,
} as const;

/**
 * Código convencional para "la petición nunca llegó al servidor". No existe en HTTP: axios
 * deja `response` sin definir cuando falla la red, se agota el tiempo de espera o el
 * navegador bloquea la respuesta por CORS.
 */
export const SIN_RESPUESTA_DEL_SERVIDOR = 0;

const MENSAJE_SIN_CONEXION =
  'No se pudo contactar el servidor. Verifique que la API esté en ejecución e intente de nuevo.';
const MENSAJE_PROHIBIDO = 'Su usuario no tiene permiso para realizar esta operación.';
const MENSAJE_DEMASIADAS_PETICIONES =
  'Se hicieron demasiados intentos seguidos. Espere un momento y vuelva a intentarlo.';
const MENSAJE_ERROR_INTERNO =
  'Ocurrió un error inesperado en el servidor. Si el problema persiste, reporte el identificador de correlación.';
const MENSAJE_GENERICO = 'No se pudo completar la operación.';

/**
 * Cabecera con la que el backend devuelve el identificador de correlación. El servidor la
 * declara en `WithExposedHeaders` precisamente para que el navegador nos deje leerla: por
 * omisión oculta toda cabecera que no sea una de las pocas estándar, aunque haya llegado.
 */
const CABECERA_ID_CORRELACION = 'x-id-correlacion';

/**
 * Error normalizado de la API.
 *
 * Existe para que ningún componente tenga que saber qué es axios. Un componente que atrapara
 * un `AxiosError` tendría que conocer su forma —`error.response?.data?.errors`— y quedaría
 * amarrado a la librería de red; con esta clase, cambiar axios por `fetch` no toca ni una
 * pantalla.
 */
export class ErrorApi extends Error {
  /** Código HTTP, o `SIN_RESPUESTA_DEL_SERVIDOR` si la petición no llegó a destino. */
  readonly codigoEstado: number;

  /**
   * Errores de validación por campo, tal como los nombró FluentValidation en el backend.
   * Vacío cuando el error no es de validación.
   */
  readonly erroresPorCampo: Record<string, string[]>;

  /** Identificador con el que se encuentra este mismo error en el registro de Serilog. */
  readonly idCorrelacion?: string;

  constructor(
    mensaje: string,
    codigoEstado: number,
    erroresPorCampo: Record<string, string[]> = {},
    idCorrelacion?: string,
  ) {
    super(mensaje);
    this.name = 'ErrorApi';
    this.codigoEstado = codigoEstado;
    this.erroresPorCampo = erroresPorCampo;
    this.idCorrelacion = idCorrelacion;
  }

  get esErrorDeValidacion(): boolean {
    return Object.keys(this.erroresPorCampo).length > 0;
  }
}

/**
 * Convierte cualquier cosa que se haya lanzado en un `ErrorApi` con un mensaje legible.
 *
 * El parámetro es `unknown` y no `Error` porque en JavaScript se puede lanzar cualquier
 * valor —un texto, un número, `undefined`—, y este es el último punto donde eso se puede
 * contener antes de que llegue a la interfaz.
 */
export function traducirError(error: unknown): ErrorApi {
  if (error instanceof ErrorApi) {
    return error;
  }

  if (!axios.isAxiosError(error)) {
    return new ErrorApi(MENSAJE_GENERICO, SIN_RESPUESTA_DEL_SERVIDOR);
  }

  if (error.response === undefined) {
    return new ErrorApi(MENSAJE_SIN_CONEXION, SIN_RESPUESTA_DEL_SERVIDOR);
  }

  const codigoEstado = error.response.status;
  const problema = extraerProblema(error.response.data);

  // El cuerpo es la fuente preferida, pero no siempre existe: un 401 emitido por el
  // middleware de JwtBearer viene sin cuerpo. La cabecera sí viaja en TODA respuesta, así
  // que es la red de seguridad para que el usuario nunca se quede sin un identificador que
  // reportar.
  const idCorrelacion = problema?.idCorrelacion ?? leerIdCorrelacion(error.response.headers);

  return new ErrorApi(
    componerMensaje(codigoEstado, problema),
    codigoEstado,
    problema?.errors ?? {},
    idCorrelacion,
  );
}

function leerIdCorrelacion(cabeceras: unknown): string | undefined {
  if (typeof cabeceras !== 'object' || cabeceras === null) {
    return undefined;
  }

  const valor = (cabeceras as Record<string, unknown>)[CABECERA_ID_CORRELACION];

  return typeof valor === 'string' ? valor : undefined;
}

/**
 * El cuerpo de una respuesta de error llega tipado como `unknown`: es lo que mandó otro
 * proceso, y creerle la forma sin comprobarla es exactamente el agujero por el que entra un
 * `undefined` que revienta tres capas más arriba.
 */
function extraerProblema(cuerpo: unknown): ProblemaApi | null {
  if (typeof cuerpo !== 'object' || cuerpo === null) {
    return null;
  }

  return cuerpo as ProblemaApi;
}

/**
 * Elige el mensaje que verá el usuario.
 *
 * Para los códigos donde el servidor redacta un motivo útil —400, 404, 409— se prefiere el
 * suyo, porque es el único que conoce la regla de negocio que se violó. Para el resto se usa
 * un texto propio: el `title` que genera ASP.NET Core por omisión llega en inglés
 * ("Forbidden", "Too Many Requests") y no le dice nada a quien está usando el sistema.
 */
function componerMensaje(codigoEstado: number, problema: ProblemaApi | null): string {
  switch (codigoEstado) {
    case CodigoEstadoHttp.Prohibido:
      return MENSAJE_PROHIBIDO;

    case CodigoEstadoHttp.DemasiadasPeticiones:
      return MENSAJE_DEMASIADAS_PETICIONES;

    case CodigoEstadoHttp.ErrorInterno:
      return MENSAJE_ERROR_INTERNO;

    default:
      return problema?.detail ?? problema?.title ?? MENSAJE_GENERICO;
  }
}
