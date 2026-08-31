import { Boton } from './Boton';
import { Dialogo } from './Dialogo';
import estilos from './Dialogo.module.css';

interface PropiedadesDialogoDeConfirmacion {
  estaAbierto: boolean;
  titulo: string;
  mensaje: string;
  etiquetaDeConfirmacion: string;
  estaProcesando?: boolean;
  alConfirmar: () => void;
  alCancelar: () => void;
}

/**
 * Confirmación de una acción irreversible o de consecuencias visibles.
 *
 * Se compone sobre `Dialogo` en lugar de repetir su mecánica: si mañana hay que cambiar cómo
 * se cierra una ventana modal, se cambia en un solo sitio.
 *
 * Se descartó `window.confirm` porque bloquea el hilo del navegador, no se puede estilizar y
 * no admite un estado "procesando" mientras la operación viaja al servidor: el usuario
 * pulsaría "Aceptar" dos veces sin saber que la primera ya salió.
 */
export function DialogoDeConfirmacion({
  estaAbierto,
  titulo,
  mensaje,
  etiquetaDeConfirmacion,
  estaProcesando = false,
  alConfirmar,
  alCancelar,
}: PropiedadesDialogoDeConfirmacion) {
  return (
    <Dialogo estaAbierto={estaAbierto} titulo={titulo} alCerrar={alCancelar}>
      <p className={estilos.mensaje}>{mensaje}</p>

      <div className={estilos.acciones}>
        <Boton variante="secundario" type="button" onClick={alCancelar} disabled={estaProcesando}>
          Cancelar
        </Boton>
        <Boton
          variante="peligro"
          type="button"
          onClick={alConfirmar}
          estaProcesando={estaProcesando}
        >
          {etiquetaDeConfirmacion}
        </Boton>
      </div>
    </Dialogo>
  );
}
