import { clienteHttp } from '../../comunes/api/clienteHttp';
import { RUTA_INICIO_SESION } from '../../comunes/api/configuracionApi';

import type { RespuestaInicioSesionDto, SesionActualDto, SolicitudInicioSesionDto } from './tipos';

const RUTA_SESION_ACTUAL = '/autenticacion/sesion';

/**
 * Verifica las credenciales contra `POST /api/autenticacion/inicio-sesion`.
 *
 * Devuelve el dato ya desenvuelto y no la respuesta de axios completa: quien llama necesita
 * el token, no los encabezados ni el código de estado. Es la frontera donde termina el
 * vocabulario de HTTP y empieza el del negocio.
 */
export async function iniciarSesion(
  solicitud: SolicitudInicioSesionDto,
): Promise<RespuestaInicioSesionDto> {
  const respuesta = await clienteHttp.post<RespuestaInicioSesionDto>(RUTA_INICIO_SESION, solicitud);

  return respuesta.data;
}

/**
 * Pregunta al servidor de quién es el token guardado.
 *
 * Es lo que se llama al recargar la página. No se decodifica el token en el navegador: un
 * JWT se puede leer sin la clave, así que decodificarlo diría lo que el token AFIRMA, no lo
 * que el servidor acepta. Solo el servidor sabe si la firma es válida y si ya venció.
 */
export async function obtenerSesionActual(): Promise<SesionActualDto> {
  const respuesta = await clienteHttp.get<SesionActualDto>(RUTA_SESION_ACTUAL);

  return respuesta.data;
}
