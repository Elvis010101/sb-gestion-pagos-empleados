/**
 * Guarda y recupera el token de acceso del navegador.
 *
 * Está aislado en un módulo con tres funciones, y no repartido en `localStorage.getItem(...)`
 * por toda la aplicación, por una razón concreta: el día que se decida mudar el token a una
 * cookie httpOnly o a memoria, se reescribe este archivo y ningún otro.
 *
 * Se guarda ÚNICAMENTE el token, nunca el nombre ni el rol. El rol decide qué botones se
 * dibujan, y cualquiera puede editar el almacenamiento local del navegador: si el rol
 * saliera de aquí, escribir `"Administrador"` a mano haría aparecer los botones de escritura.
 * Esos botones seguirían chocando contra el 403 del servidor —la autorización real la hace el
 * backend leyendo el claim firmado—, pero la interfaz estaría mintiendo. Por eso la identidad
 * se le pregunta al servidor con `GET /autenticacion/sesion`, que la lee del token firmado.
 */

const CLAVE_TOKEN_ACCESO = 'sb.gestionPagos.tokenAcceso';

export function obtenerToken(): string | null {
  return window.localStorage.getItem(CLAVE_TOKEN_ACCESO);
}

export function guardarToken(token: string): void {
  window.localStorage.setItem(CLAVE_TOKEN_ACCESO, token);
}

export function borrarToken(): void {
  window.localStorage.removeItem(CLAVE_TOKEN_ACCESO);
}
