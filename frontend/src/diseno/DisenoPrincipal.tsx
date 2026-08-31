import { Outlet, useLocation, matchPath } from 'react-router-dom';

import { useSesion } from '../caracteristicas/autenticacion/useSesion';
import { ETIQUETAS_ROL_USUARIO } from '../caracteristicas/autenticacion/tipos';
import { DEFINICIONES_DE_PAGINA } from '../rutas/rutas';

import { BarraLateral } from './BarraLateral';
import estilos from './DisenoPrincipal.module.css';

const TITULO_DE_RESERVA = 'Gestión de Pagos';

/**
 * Armazón visual de la aplicación: barra lateral azul, encabezado y panel gris con la
 * tarjeta blanca, tal como en `Maqueta.jpeg`.
 *
 * El diseño no recibe el título por propiedad. Lo resuelve a partir de la ruta actual
 * consultando la misma tabla de la que se alimenta la barra lateral. La alternativa —que
 * cada pantalla le pasara su título hacia arriba— obligaría a un efecto por pantalla y a un
 * estado más, y abriría la puerta a que el menú y el encabezado dijeran cosas distintas.
 */
export function DisenoPrincipal() {
  const { sesion, cerrarSesion } = useSesion();
  const ubicacion = useLocation();

  // `matchPath` y no una comparación de textos: la ruta de edición lleva un parámetro
  // (`/empleados/7/editar`), y ninguna igualdad literal la reconocería.
  const paginaActual = DEFINICIONES_DE_PAGINA.find(
    (pagina) => matchPath(pagina.ruta, ubicacion.pathname) !== null,
  );

  return (
    <div className={estilos.contenedor}>
      <BarraLateral />

      <div className={estilos.areaDeContenido}>
        <header className={estilos.encabezado}>
          <h1 className={estilos.tituloDePagina}>{paginaActual?.titulo ?? TITULO_DE_RESERVA}</h1>

          {sesion !== null ? (
            <div className={estilos.datosDeSesion}>
              <span>
                <span className={estilos.nombreDeUsuario}>{sesion.nombreUsuario}</span>
                {' · '}
                {ETIQUETAS_ROL_USUARIO[sesion.rol]}
              </span>
              <button type="button" className={estilos.botonCerrarSesion} onClick={cerrarSesion}>
                Cerrar sesión
              </button>
            </div>
          ) : null}
        </header>

        {/* El panel gris es el `main`: es la región que cambia con la navegación, y marcarla
            así permite a un lector de pantalla saltar directamente al contenido. */}
        <main className={estilos.panel}>
          <Outlet />
        </main>
      </div>
    </div>
  );
}
