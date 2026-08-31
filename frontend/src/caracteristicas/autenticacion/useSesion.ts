import { useContext } from 'react';

import { ContextoSesion, type EstadoSesion } from './ContextoSesion';

/**
 * Acceso a la sesión desde cualquier componente.
 *
 * Se expone un hook y no el contexto crudo para que ninguna pantalla escriba
 * `useContext(ContextoSesion)` y tenga que lidiar con el `undefined`. El tipo de retorno no
 * es anulable: después de la comprobación, quien llama recibe siempre un estado válido.
 */
export function useSesion(): EstadoSesion {
  const contexto = useContext(ContextoSesion);

  if (contexto === undefined) {
    throw new Error('useSesion solo puede usarse dentro de un <ProveedorSesion>.');
  }

  return contexto;
}
