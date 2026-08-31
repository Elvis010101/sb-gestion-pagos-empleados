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
 * Cuando el error trae detalle por campo —las respuestas 400 de FluentValidation— se listan
 * TODOS los mensajes. El título de esas respuestas es genérico ("La solicitud contiene
 * errores de validación") y por sí solo no dice qué corregir; el detalle está en `errors`, y
 * descartarlo dejaría al usuario mirando un rechazo sin motivo.
 *
 * Se muestran los mensajes y no los nombres de propiedad que los acompañan: llegan en
 * PascalCase, como los declara C# —"SueldoPorHora"—, y ese no es vocabulario para nadie que
 * use el sistema. El mensaje ya nombra el campo en español.
 *
 * También se muestra el identificador de correlación cuando existe: es el mismo valor que
 * aparece en el archivo de registro de Serilog, así que un usuario que lo copia en su reporte
 * le ahorra al soporte tener que adivinar cuál de las mil peticiones del día fue la suya.
 */
export function MensajeDeError({ error, alReintentar }: PropiedadesMensajeDeError) {
  const mensajesDeValidacion = Object.values(error.erroresPorCampo).flat();

  return (
    <div className={`${estilos.aviso} ${estilos.avisoDeError}`} role="alert">
      <span className={estilos.tituloDelAviso}>No se pudo completar la operación</span>
      <span>{error.message}</span>

      {mensajesDeValidacion.length > 0 ? (
        <ul className={estilos.listaDeMensajes}>
          {mensajesDeValidacion.map((mensaje) => (
            <li key={mensaje}>{mensaje}</li>
          ))}
        </ul>
      ) : null}

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
