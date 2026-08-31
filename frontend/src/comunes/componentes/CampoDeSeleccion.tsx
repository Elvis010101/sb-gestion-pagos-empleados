import { forwardRef, useId, type SelectHTMLAttributes } from 'react';

import estilos from './Campos.module.css';

export interface OpcionDeSeleccion {
  valor: string;
  etiqueta: string;
}

interface PropiedadesCampoDeSeleccion extends SelectHTMLAttributes<HTMLSelectElement> {
  etiqueta: string;
  opciones: readonly OpcionDeSeleccion[];

  /** Texto de la opción vacía. Si no se pasa, el selector no ofrece "sin elegir". */
  etiquetaDeOpcionVacia?: string;

  mensajeDeError?: string;
}

/**
 * Selector con rótulo y mensaje de error, hermano de `CampoDeTexto`.
 *
 * Las opciones llegan como datos y no como hijos `<option>` escritos a mano: así el filtro de
 * estado, el selector de tipo de contrato y el de departamento comparten componente aunque
 * sus opciones vengan de sitios distintos —una constante, un registro, el servidor—.
 */
export const CampoDeSeleccion = forwardRef<HTMLSelectElement, PropiedadesCampoDeSeleccion>(
  function CampoDeSeleccion(
    {
      etiqueta,
      opciones,
      etiquetaDeOpcionVacia,
      mensajeDeError,
      id,
      className,
      ...atributosNativos
    },
    ref,
  ) {
    const idGenerado = useId();
    const idDelCampo = id ?? idGenerado;
    const idDelError = `${idDelCampo}-error`;
    const tieneError = mensajeDeError !== undefined;

    return (
      <div className={[estilos.campo, className ?? ''].filter((clase) => clase !== '').join(' ')}>
        <label className={estilos.etiqueta} htmlFor={idDelCampo}>
          {etiqueta}
        </label>

        <select
          {...atributosNativos}
          id={idDelCampo}
          ref={ref}
          className={[estilos.control, tieneError ? estilos.controlConError : '']
            .filter((clase) => clase !== '')
            .join(' ')}
          aria-invalid={tieneError}
          aria-describedby={tieneError ? idDelError : undefined}
        >
          {etiquetaDeOpcionVacia !== undefined ? (
            <option value="">{etiquetaDeOpcionVacia}</option>
          ) : null}

          {opciones.map((opcion) => (
            <option key={opcion.valor} value={opcion.valor}>
              {opcion.etiqueta}
            </option>
          ))}
        </select>

        {tieneError ? (
          <span className={estilos.error} id={idDelError} role="alert">
            {mensajeDeError}
          </span>
        ) : null}
      </div>
    );
  },
);
