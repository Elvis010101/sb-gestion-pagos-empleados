import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';

import { borrarToken, guardarToken, obtenerToken } from '../../comunes/api/almacenamientoSesion';
import { registrarManejadorDeSesionExpirada } from '../../comunes/api/clienteHttp';

import { iniciarSesion as iniciarSesionEnApi, obtenerSesionActual } from './autenticacionApi';
import { ContextoSesion, type EstadoSesion } from './ContextoSesion';
import { RolUsuario, type SesionActualDto, type SolicitudInicioSesionDto } from './tipos';

interface PropiedadesProveedorSesion {
  children: ReactNode;
}

/**
 * Mantiene la sesión viva para toda la aplicación.
 *
 * Es el único estado que se guarda en un contexto. El resto —listados, filtros, formularios—
 * se queda en la pantalla que lo usa: meter todo en un contexto global convierte cualquier
 * cambio pequeño en un redibujado de la aplicación entera y hace imposible saber quién
 * modificó qué. La sesión sí lo merece porque la necesitan piezas que no tienen ninguna
 * relación entre sí: la barra lateral, la ruta protegida y cada botón de escritura.
 */
export function ProveedorSesion({ children }: PropiedadesProveedorSesion) {
  const [sesion, establecerSesion] = useState<SesionActualDto | null>(null);
  const [estaComprobandoSesion, establecerEstaComprobandoSesion] = useState(true);

  const cerrarSesion = useCallback(() => {
    borrarToken();
    establecerSesion(null);
  }, []);

  /**
   * Conecta la capa de red con la de sesión.
   *
   * El interceptor detecta el token vencido pero no sabe qué hacer con esa información; aquí
   * se le dice. Basta con limpiar el estado: la ruta protegida verá `sesion === null` y hará
   * la redirección ella sola. No hace falta navegar a mano desde aquí, y eso evita tener dos
   * piezas decidiendo a dónde va el usuario.
   */
  useEffect(() => {
    registrarManejadorDeSesionExpirada(cerrarSesion);
  }, [cerrarSesion]);

  /**
   * Comprobación del token guardado al arrancar la aplicación.
   *
   * Sin esto, recargar la página cerraría la sesión aunque el token siguiera vigente. Se le
   * pregunta al servidor en lugar de confiar en lo que hay en el navegador, porque el
   * almacenamiento local lo puede editar cualquiera.
   */
  useEffect(() => {
    let elComponenteSigueMontado = true;

    async function comprobarTokenGuardado(): Promise<void> {
      if (obtenerToken() === null) {
        establecerEstaComprobandoSesion(false);

        return;
      }

      try {
        const sesionDelServidor = await obtenerSesionActual();

        if (elComponenteSigueMontado) {
          establecerSesion(sesionDelServidor);
        }
      } catch {
        // Cualquier fallo aquí significa que el token no sirve: vencido, manipulado o
        // firmado con otra clave. Se descarta en silencio y el usuario aparece en la
        // pantalla de acceso, que es exactamente lo que corresponde. Registrar un error
        // visible sería alarmar por algo normal: los tokens vencen todos los días.
        if (elComponenteSigueMontado) {
          cerrarSesion();
        }
      } finally {
        if (elComponenteSigueMontado) {
          establecerEstaComprobandoSesion(false);
        }
      }
    }

    void comprobarTokenGuardado();

    // La función de limpieza evita actualizar el estado de un componente ya desmontado, cosa
    // que pasa de verdad en desarrollo: React 18 en modo estricto monta, desmonta y vuelve a
    // montar cada componente para destapar justo esta clase de fugas.
    return () => {
      elComponenteSigueMontado = false;
    };
  }, [cerrarSesion]);

  const iniciarSesion = useCallback(async (solicitud: SolicitudInicioSesionDto): Promise<void> => {
    const respuesta = await iniciarSesionEnApi(solicitud);

    guardarToken(respuesta.token);

    // La identidad se toma de la respuesta y no se pide otra vez con `GET /sesion`: viene del
    // mismo servidor, en el mismo viaje, y repetir la llamada solo agregaría latencia a la
    // pantalla de acceso.
    establecerSesion({ nombreUsuario: respuesta.nombreUsuario, rol: respuesta.rol });
  }, []);

  /**
   * El valor del contexto se memoriza porque un objeto literal sería nuevo en cada dibujado,
   * y React compara por identidad: sin `useMemo`, TODO componente suscrito al contexto se
   * volvería a dibujar cada vez que este proveedor se dibuje, aunque la sesión no cambiara.
   */
  const valorDelContexto = useMemo<EstadoSesion>(
    () => ({
      sesion,
      estaComprobandoSesion,
      esAdministrador: sesion?.rol === RolUsuario.Administrador,
      iniciarSesion,
      cerrarSesion,
    }),
    [sesion, estaComprobandoSesion, iniciarSesion, cerrarSesion],
  );

  return <ContextoSesion.Provider value={valorDelContexto}>{children}</ContextoSesion.Provider>;
}
