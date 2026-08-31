import type { ReactNode } from 'react';

import estilos from './EstadosDeInterfaz.module.css';

interface PropiedadesEstadoVacio {
  titulo: string;
  descripcion?: string;

  /** Acción sugerida, por ejemplo un botón para crear el primer registro. */
  children?: ReactNode;
}

/**
 * Lo que se dibuja cuando una consulta funcionó pero no devolvió nada.
 *
 * Es un caso distinto del error y del de carga, y confundirlos es lo que produce la pantalla
 * en blanco clásica: la tabla no falla, simplemente no tiene filas, y sin este componente el
 * usuario ve un recuadro vacío sin saber si el filtro no encontró nada o si algo se rompió.
 */
export function EstadoVacio({ titulo, descripcion, children }: PropiedadesEstadoVacio) {
  return (
    <div className={estilos.contenedorCentrado}>
      <p className={estilos.tituloDeEstadoVacio}>{titulo}</p>
      {descripcion !== undefined ? <p>{descripcion}</p> : null}
      {children}
    </div>
  );
}
