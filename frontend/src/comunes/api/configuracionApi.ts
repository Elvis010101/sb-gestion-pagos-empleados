/**
 * Configuración de la conexión con la API.
 */

/**
 * URL de reserva para el entorno de desarrollo local: es el puerto del perfil `http` del
 * `launchSettings.json` del proyecto Api.
 *
 * Está en el código y no en el archivo de configuración porque NO es un secreto —es una
 * dirección de localhost— y porque tener un valor de reserva hace que el proyecto arranque
 * recién clonado, sin pasos previos. La URL real de cada entorno llega por la variable de
 * entorno; esta solo cubre el caso en que nadie la definió.
 */
const URL_BASE_API_DESARROLLO = 'http://localhost:5122/api';

export const URL_BASE_API: string = import.meta.env.VITE_URL_BASE_API ?? URL_BASE_API_DESARROLLO;

/**
 * Tope de espera de una petición.
 *
 * Sin él, axios espera indefinidamente: si el backend está caído de una forma que no cierra
 * la conexión, la interfaz se queda girando para siempre y el usuario no recibe ningún error.
 */
export const MILISEGUNDOS_TIEMPO_DE_ESPERA = 15_000;

/**
 * Ruta del inicio de sesión, relativa a la URL base.
 *
 * El interceptor de respuestas la necesita para distinguir dos 401 que significan cosas
 * distintas: el de "tu token venció" —que debe echar al usuario a la pantalla de acceso— y
 * el de "esa contraseña no es", que debe quedarse en el formulario mostrando el error.
 */
export const RUTA_INICIO_SESION = '/autenticacion/inicio-sesion';
