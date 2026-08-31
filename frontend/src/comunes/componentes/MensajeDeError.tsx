import type { ErrorApi } from '../api/ErrorApi';

import { Boton } from './Boton';
import estilos from './EstadosDeInterfaz.module.css';

interface PropiedadesMensajeDeError {
  error: ErrorApi;

  /**
   * Acción para volver a intentar. Es opcional porque no todo error se puede reintentar: un
   * 403 va a fallar igual las veces que se pulse, y ofrecer el botón sería mentirle al
   * usuario.
   */
  alReintentar?: () => void;
}

/**
 * Presenta un error de la API de forma que el usuario pueda hacer algo con él.
 *
 * Muestra el identificador de correlación cuando existe: es el mismo valor que aparece en el
 * archivo de registro de Serilog, así que un usuario que lo copia en su reporte le ahorra al
 * soporte tener que adivinar cuál de las mil peticiones del día fue la suya.
 */
export function MensajeDeError({ error, alReintentar }: PropiedadesMensajeDeError) {
  return (
    <div className={`${estilos.aviso} ${estilos.avisoDeError}`} role="alert">
      <span className={estilos.tituloDelAviso}>No se pudo completar la operación</span>
      <span>{error.message}</span>

      {error.idCorrelacion !== undefined ? (
        <span className={estilos.identificadorDeCorrelacion}>
          Identificador para soporte: {error.idCorrelacion}
        </span>
      ) : null}

      {alReintentar !== undefined ? (
        <div className={estilos.accionesDelAviso}>
          <Boton variante="secundario" type="button" onClick={alReintentar}>
            Reintentar
          </Boton>
        </div>
      ) : null}
    </div>
  );
}
