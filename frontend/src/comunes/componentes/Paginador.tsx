import { Boton } from './Boton';
import estilos from './Paginador.module.css';

interface PropiedadesPaginador {
  pagina: number;
  totalPaginas: number;
  totalRegistros: number;

  /**
   * Notifica hacia arriba la página pedida. El paginador NO guarda la página en un estado
   * propio: si lo hiciera, habría dos verdades sobre en qué página estamos —la suya y la de
   * la pantalla que hace la consulta— y bastaría con limpiar los filtros para que
   * discreparan. Aquí el estado vive en el padre y este componente solo lo pinta y avisa.
   */
  alCambiarPagina: (paginaNueva: number) => void;

  estaCargando: boolean;
}

const PRIMERA_PAGINA = 1;

export function Paginador({
  pagina,
  totalPaginas,
  totalRegistros,
  alCambiarPagina,
  estaCargando,
}: PropiedadesPaginador) {
  const hayPaginaAnterior = pagina > PRIMERA_PAGINA;
  const hayPaginaSiguiente = pagina < totalPaginas;

  return (
    <div className={estilos.paginador}>
      <span className={estilos.resumen}>
        {totalRegistros} {totalRegistros === 1 ? 'registro encontrado' : 'registros encontrados'}
      </span>

      <div className={estilos.controles}>
        <Boton
          variante="secundario"
          type="button"
          onClick={() => alCambiarPagina(pagina - 1)}
          disabled={!hayPaginaAnterior || estaCargando}
        >
          Anterior
        </Boton>

        {/* `aria-live` hace que un lector de pantalla anuncie el cambio de página: sin esto,
            quien navega sin ver pulsa "Siguiente" y no recibe ninguna confirmación. */}
        <span className={estilos.indicadorDePagina} aria-live="polite">
          Página {pagina} de {Math.max(totalPaginas, PRIMERA_PAGINA)}
        </span>

        <Boton
          variante="secundario"
          type="button"
          onClick={() => alCambiarPagina(pagina + 1)}
          disabled={!hayPaginaSiguiente || estaCargando}
        >
          Siguiente
        </Boton>
      </div>
    </div>
  );
}
