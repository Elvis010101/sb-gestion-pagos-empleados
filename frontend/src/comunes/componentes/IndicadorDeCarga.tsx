import estilos from './EstadosDeInterfaz.module.css';

interface PropiedadesIndicadorDeCarga {
  mensaje?: string;
}

/**
 * Estado de carga visible.
 *
 * Existe como componente propio, y no como un `{cargando && <p>Cargando</p>}` escrito a mano
 * en cada pantalla, para cumplir el requisito de que no haya pantallas en blanco de una sola
 * forma en todo el sistema. Una pantalla en blanco durante una espera es indistinguible de
 * una pantalla rota.
 */
export function IndicadorDeCarga({ mensaje = 'Cargando…' }: PropiedadesIndicadorDeCarga) {
  return (
    <div className={estilos.contenedorCentrado}>
      {/* `role="status"` hace que un lector de pantalla anuncie el cambio sin robar el foco. */}
      <div className={estilos.girador} role="status" aria-live="polite" />
      <p>{mensaje}</p>
    </div>
  );
}
