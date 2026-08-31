import type { ReactNode } from 'react';

import estilos from './Tarjeta.module.css';

interface PropiedadesTarjeta {
  titulo?: string;
  descripcion?: string;

  /**
   * Acciones del encabezado, por ejemplo el botón de crear. Se recibe como nodo y no como una
   * lista de configuraciones de botón: la tarjeta no tiene por qué saber qué acciones existen
   * ni quién puede verlas. Quien la usa decide qué pone ahí, incluso nada.
   */
  acciones?: ReactNode;

  children: ReactNode;
}

/**
 * La tarjeta blanca de esquinas redondeadas y sombra suave de la maqueta.
 *
 * Es un componente de composición: no sabe qué muestra, solo cómo se ve el recipiente. Por
 * eso sirve igual para la tabla de empleados, para un formulario y para el reporte, y por eso
 * el estilo del contenedor se define una sola vez en todo el sistema.
 */
export function Tarjeta({ titulo, descripcion, acciones, children }: PropiedadesTarjeta) {
  const tieneEncabezado = titulo !== undefined || acciones !== undefined;

  return (
    <section className={estilos.tarjeta}>
      {tieneEncabezado ? (
        <div className={estilos.encabezado}>
          <div>
            {titulo !== undefined ? <h2 className={estilos.titulo}>{titulo}</h2> : null}
            {descripcion !== undefined ? (
              <p className={estilos.descripcion}>{descripcion}</p>
            ) : null}
          </div>
          {acciones}
        </div>
      ) : null}

      {children}
    </section>
  );
}
