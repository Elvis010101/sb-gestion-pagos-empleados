import axios, { type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';

import { obtenerToken } from './almacenamientoSesion';
import {
  MILISEGUNDOS_TIEMPO_DE_ESPERA,
  RUTA_INICIO_SESION,
  URL_BASE_API,
} from './configuracionApi';
import { CodigoEstadoHttp, ErrorApi, traducirError } from './ErrorApi';

const MENSAJE_SESION_EXPIRADA =
  'Su sesión expiró o el token dejó de ser válido. Inicie sesión nuevamente.';

/**
 * Instancia ÚNICA de axios para toda la aplicación.
 *
 * Es una instancia propia y no el axios global (`axios.get(...)`) porque los interceptores
 * que se le cuelgan al global afectan a cualquier librería del proyecto que también use
 * axios: se le estaría inyectando nuestro token a peticiones que no son nuestras.
 */
export const clienteHttp: AxiosInstance = axios.create({
  baseURL: URL_BASE_API,
  timeout: MILISEGUNDOS_TIEMPO_DE_ESPERA,
  headers: { 'Content-Type': 'application/json' },
});

/**
 * Qué hacer cuando el servidor rechaza el token.
 *
 * Es una función que se registra desde afuera, y no un `window.location.href = '/acceso'`
 * escrito aquí, por dos motivos. Primero, porque este módulo no debe conocer el enrutador:
 * si importara `react-router`, la capa de red dependería de la capa de presentación y el
 * flujo de dependencias quedaría al revés. Segundo, porque una redirección con
 * `window.location` recarga la página entera y tira el estado de la aplicación; el proveedor
 * de sesión, en cambio, puede limpiar y navegar sin recargar.
 */
type ManejadorDeSesionExpirada = () => void;

let manejarSesionExpirada: ManejadorDeSesionExpirada | null = null;

export function registrarManejadorDeSesionExpirada(manejador: ManejadorDeSesionExpirada): void {
  manejarSesionExpirada = manejador;
}

/**
 * Interceptor de PETICIÓN: adjunta el token a todo lo que salga.
 *
 * Es la pieza que responde a "¿por qué no poner el token en cada llamada?". Con setenta
 * llamadas repartidas en cuatro módulos, basta con que una sola se olvide del encabezado
 * para tener un 401 intermitente que nadie reproduce. Aquí la regla se escribe UNA vez y no
 * hay forma de que una petición se le escape.
 */
clienteHttp.interceptors.request.use((configuracion: InternalAxiosRequestConfig) => {
  const token = obtenerToken();

  if (token !== null) {
    configuracion.headers.set('Authorization', `Bearer ${token}`);
  }

  return configuracion;
});

/**
 * Interceptor de RESPUESTA: normaliza todo fallo a `ErrorApi` y centraliza el 401.
 *
 * La respuesta exitosa se deja pasar tal cual. Lo que cambia es el camino del error: ningún
 * componente vuelve a ver un `AxiosError`, y el vencimiento del token se resuelve en un solo
 * lugar en vez de en cada pantalla.
 */
clienteHttp.interceptors.response.use(
  (respuesta) => respuesta,
  (error: unknown) => {
    const errorTraducido = traducirError(error);

    if (esSesionExpirada(error, errorTraducido)) {
      manejarSesionExpirada?.();

      return Promise.reject(
        new ErrorApi(
          MENSAJE_SESION_EXPIRADA,
          errorTraducido.codigoEstado,
          errorTraducido.erroresPorCampo,
          errorTraducido.idCorrelacion,
        ),
      );
    }

    return Promise.reject(errorTraducido);
  },
);

/**
 * Distingue los dos 401 que existen en este sistema.
 *
 * Uno significa "tu sesión venció" y debe echar al usuario a la pantalla de acceso. El otro
 * lo devuelve el propio inicio de sesión cuando la contraseña no coincide, y ahí sacar al
 * usuario de la pantalla en la que está sería absurdo: se quedaría sin ver por qué falló.
 * Sin esta distinción, un error de tecleo en la contraseña provocaría una redirección.
 */
function esSesionExpirada(errorOriginal: unknown, errorTraducido: ErrorApi): boolean {
  if (errorTraducido.codigoEstado !== CodigoEstadoHttp.NoAutorizado) {
    return false;
  }

  const rutaDeLaPeticion = axios.isAxiosError(errorOriginal)
    ? (errorOriginal.config?.url ?? '')
    : '';

  return rutaDeLaPeticion !== RUTA_INICIO_SESION;
}
