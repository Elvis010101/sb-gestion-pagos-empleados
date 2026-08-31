import { createContext } from 'react';

import type { SesionActualDto, SolicitudInicioSesionDto } from './tipos';

/**
 * Lo que el contexto de sesión ofrece a toda la aplicación.
 */
export interface EstadoSesion {
  /** Usuario autenticado, o `null` si no hay sesión. */
  sesion: SesionActualDto | null;

  /**
   * Verdadero mientras se comprueba contra el servidor el token guardado, al arrancar.
   * Sin esta bandera, la ruta protegida vería `sesion === null` en el primer dibujado y
   * echaría a la pantalla de acceso a un usuario que sí tenía sesión válida.
   */
  estaComprobandoSesion: boolean;

  /**
   * Atajo de lectura para la interfaz. Vive aquí y no repetido en cada pantalla para que la
   * regla "quién puede escribir" se defina una sola vez.
   */
  esAdministrador: boolean;

  iniciarSesion: (solicitud: SolicitudInicioSesionDto) => Promise<void>;
  cerrarSesion: () => void;
}

/**
 * El valor por omisión es `undefined` a propósito, y no un objeto vacío de relleno.
 *
 * Con un relleno, usar el hook fuera del proveedor devolvería una sesión nula perfectamente
 * plausible y la aplicación se comportaría como si nadie hubiera iniciado sesión: un error
 * de montaje disfrazado de estado normal. Con `undefined`, el hook puede detectarlo y fallar
 * de inmediato con un mensaje que dice exactamente qué falta.
 */
export const ContextoSesion = createContext<EstadoSesion | undefined>(undefined);
