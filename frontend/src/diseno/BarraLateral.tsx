import { NavLink } from 'react-router-dom';

import iconoInicio from '../activos/inicio.svg';
import logoSuperintendencia from '../activos/logo-superintendencia-de-bancos.png';
import { useSesion } from '../caracteristicas/autenticacion/useSesion';
import { DEFINICIONES_DE_PAGINA, Rutas } from '../rutas/rutas';

import estilos from './BarraLateral.module.css';

/**
 * Barra lateral de navegación de la maqueta.
 *
 * No recibe ninguna propiedad, y es deliberado: lo que necesita —el rol, para decidir qué
 * ítems muestra, y la ruta actual, para resaltar el activo— lo toma del contexto de sesión y
 * del enrutador. Pasárselo desde arriba obligaría al diseño principal a conocer datos que no
 * usa, solo para reenviarlos: eso es el "prop drilling" que se quiere evitar.
 */
export function BarraLateral() {
  const { esAdministrador } = useSesion();

  const paginasVisibles = DEFINICIONES_DE_PAGINA.filter(
    (pagina) => pagina.apareceEnNavegacion && (!pagina.requiereAdministrador || esAdministrador),
  );

  return (
    <aside className={estilos.barraLateral}>
      <img
        className={estilos.logo}
        src={logoSuperintendencia}
        alt="Superintendencia de Bancos de la República Dominicana"
      />

      <nav className={estilos.navegacion} aria-label="Navegación principal">
        {paginasVisibles.map((pagina) => (
          <NavLink
            key={pagina.ruta}
            to={pagina.ruta}
            // `NavLink` resuelve solo qué enlace está activo comparando con la ruta actual;
            // reproducir esa comparación a mano con `useLocation` fallaría en las rutas
            // anidadas y en las que llevan parámetros.
            className={({ isActive }) =>
              [estilos.enlace, isActive ? estilos.enlaceActivo : ''].filter(Boolean).join(' ')
            }
            // Sin `end`, la ruta raíz "/" se consideraría activa en TODAS las pantallas,
            // porque toda dirección empieza por ella.
            end={pagina.ruta === Rutas.Inicio}
          >
            {pagina.ruta === Rutas.Inicio ? (
              // El ícono es decorativo: el rótulo de al lado ya dice "Inicio", así que
              // anunciarlo otra vez solo repetiría la palabra en un lector de pantalla.
              <img className={estilos.icono} src={iconoInicio} alt="" aria-hidden="true" />
            ) : null}
            {pagina.titulo}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
