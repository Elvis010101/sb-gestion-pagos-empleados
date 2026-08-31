import { forwardRef, useId, type InputHTMLAttributes } from 'react';

import estilos from './Campos.module.css';

interface PropiedadesCampoDeTexto extends InputHTMLAttributes<HTMLInputElement> {
  etiqueta: string;

  /** Mensaje de validación. Si viene, el campo se pinta en rojo y lo anuncia. */
  mensajeDeError?: string;
}

/**
 * Campo de texto con rótulo y mensaje de error.
 *
 * Usa `forwardRef` porque react-hook-form trabaja con componentes NO controlados: su función
 * `register` devuelve una referencia al elemento del DOM y lee su valor directamente, en vez
 * de guardar cada pulsación de tecla en el estado de React. Sin `forwardRef`, esa referencia
 * se quedaría apuntando a este envoltorio y el formulario nunca vería lo que el usuario
 * escribió. Esa decisión de la librería es también la que evita que un formulario de doce
 * campos se vuelva a dibujar entero con cada letra.
 */
export const CampoDeTexto = forwardRef<HTMLInputElement, PropiedadesCampoDeTexto>(
  function CampoDeTexto({ etiqueta, mensajeDeError, id, className, ...atributosNativos }, ref) {
    // `useId` genera un identificador único y estable. Hace falta uno de verdad para que el
    // `htmlFor` de la etiqueta funcione: si dos campos compartieran identificador, hacer clic
    // en un rótulo enfocaría el campo equivocado.
    const idGenerado = useId();
    const idDelCampo = id ?? idGenerado;
    const idDelError = `${idDelCampo}-error`;
    const tieneError = mensajeDeError !== undefined;

    return (
      <div className={[estilos.campo, className ?? ''].filter((clase) => clase !== '').join(' ')}>
        <label className={estilos.etiqueta} htmlFor={idDelCampo}>
          {etiqueta}
        </label>

        <input
          {...atributosNativos}
          id={idDelCampo}
          ref={ref}
          className={[estilos.control, tieneError ? estilos.controlConError : '']
            .filter((clase) => clase !== '')
            .join(' ')}
          // Marca el campo como inválido para las tecnologías de asistencia y lo enlaza con
          // su mensaje: sin esto, un lector de pantalla anuncia el rótulo pero nunca el
          // motivo del rechazo, y el usuario no sabe qué corregir.
          aria-invalid={tieneError}
          aria-describedby={tieneError ? idDelError : undefined}
        />

        {tieneError ? (
          <span className={estilos.error} id={idDelError} role="alert">
            {mensajeDeError}
          </span>
        ) : null}
      </div>
    );
  },
);
