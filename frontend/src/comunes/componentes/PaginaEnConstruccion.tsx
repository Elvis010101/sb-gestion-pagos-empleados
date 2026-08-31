import { Tarjeta } from '../../diseno/Tarjeta';

import { EstadoVacio } from './EstadoVacio';

interface PropiedadesPaginaEnConstruccion {
  titulo: string;
}

/**
 * Marcador de posición temporal para las pantallas que aún no se han implementado.
 *
 * Existe para que la navegación de la maqueta esté completa y recorrible desde el primer
 * momento, en vez de dejar enlaces que llevan a la nada. Se elimina cuando cada pantalla real
 * ocupe su lugar.
 */
export function PaginaEnConstruccion({ titulo }: PropiedadesPaginaEnConstruccion) {
  return (
    <Tarjeta titulo={titulo}>
      <EstadoVacio
        titulo="Pantalla en construcción"
        descripcion="Esta sección se implementa en el siguiente bloque de trabajo."
      />
    </Tarjeta>
  );
}
