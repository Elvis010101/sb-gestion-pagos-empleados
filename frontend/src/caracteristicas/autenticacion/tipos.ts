/**
 * Contratos del módulo de autenticación (RF-07).
 */

/**
 * Rol de autorización, espejo del enum `RolUsuario` del Dominio.
 *
 * Viaja como NÚMERO y no como texto porque el host no registra `JsonStringEnumConverter`:
 * `System.Text.Json` serializa los enums por su valor numérico. Los números son los mismos
 * que el enum de C# declara de forma explícita.
 *
 * Se modela como objeto congelado más tipo unión, y no con el `enum` nativo de TypeScript,
 * por tres razones: el `enum` genera un objeto en tiempo de ejecución que el empaquetador no
 * puede podar, obliga a importarlo incluso donde solo se necesita el tipo, y la opción
 * `erasableSyntaxOnly` del tsconfig —que Vite activa porque transpila sin verificar tipos—
 * directamente lo prohíbe.
 */
export const RolUsuario = {
  Administrador: 1,
  Usuario: 2,
} as const;

export type RolUsuario = (typeof RolUsuario)[keyof typeof RolUsuario];

/**
 * Rótulos para mostrar. `Record<RolUsuario, string>` obliga a que estén TODOS: si mañana el
 * backend agrega un rol y alguien lo suma al objeto de arriba, el compilador señala este
 * mapa como incompleto en vez de dejar que la interfaz muestre un hueco.
 */
export const ETIQUETAS_ROL_USUARIO: Record<RolUsuario, string> = {
  [RolUsuario.Administrador]: 'Administrador',
  [RolUsuario.Usuario]: 'Usuario',
};

/** Credenciales que se envían a `POST /api/autenticacion/inicio-sesion`. */
export interface SolicitudInicioSesionDto {
  nombreUsuario: string;
  contrasena: string;
}

/** Respuesta de un inicio de sesión exitoso. */
export interface RespuestaInicioSesionDto {
  token: string;

  /**
   * Instante de expiración en formato ISO 8601. Llega como texto y no como `Date` porque
   * JSON no tiene tipo fecha: la conversión es responsabilidad de quien lo consume.
   */
  fechaExpiracionUtc: string;

  nombreUsuario: string;
  rol: RolUsuario;
}

/** Identidad del dueño del token, según `GET /api/autenticacion/sesion`. */
export interface SesionActualDto {
  nombreUsuario: string;
  rol: RolUsuario;
}
