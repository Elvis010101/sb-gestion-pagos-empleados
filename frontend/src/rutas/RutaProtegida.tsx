import { Navigate, Outlet, useLocation } from 'react-router-dom';

import { useSesion } from '../caracteristicas/autenticacion/useSesion';
import { IndicadorDeCarga } from '../comunes/componentes/IndicadorDeCarga';

import { Rutas } from './rutas';

/**
 * Guardia de las rutas que exigen sesión.
 *
 * Se implementa como ruta de diseño con `<Outlet />` en lugar de un componente envolvente
 * repetido en cada pantalla: así la protección se declara UNA vez en el árbol de rutas y una
 * pantalla nueva nace protegida por estar donde está, no por acordarse de envolverla.
 */
export function RutaProtegida() {
  const { sesion, estaComprobandoSesion } = useSesion();
  const ubicacion = useLocation();

  // Este orden importa. Si se preguntara primero por la sesión, al recargar la página se
  // vería `null` durante el instante en que aún se está comprobando el token contra el
  // servidor, y el usuario saldría expulsado a la pantalla de acceso teniendo sesión válida.
  if (estaComprobandoSesion) {
    return <IndicadorDeCarga mensaje="Verificando su sesión…" />;
  }

  if (sesion === null) {
    // Se recuerda a dónde quería ir para devolverlo ahí tras autenticarse. Sin esto, alguien
    // que abre un enlace directo a un empleado acaba en la pantalla de inicio y tiene que
    // volver a buscarlo. `replace` evita que la pantalla de acceso quede en el historial: el
    // botón "atrás" del navegador no debe devolver a un formulario ya usado.
    return <Navigate to={Rutas.IniciarSesion} replace state={{ destino: ubicacion.pathname }} />;
  }

  return <Outlet />;
}
