import { useEffect, useRef, type ReactNode } from 'react';

import estilos from './Dialogo.module.css';

interface PropiedadesDialogo {
  estaAbierto: boolean;
  titulo: string;

  /** Ensancha el diálogo para el contenido que no es un simple mensaje, como un formulario. */
  esAncho?: boolean;

  /** Se invoca al pulsar Escape. Quien lo usa decide si eso equivale a cancelar. */
  alCerrar: () => void;

  children: ReactNode;
}

/**
 * Ventana modal, construida sobre el elemento nativo `<dialog>`.
 *
 * `showModal()` trae resuelto lo que un `<div>` flotante hace mal casi siempre: atrapa el
 * foco dentro de la ventana, lo devuelve al elemento que la abrió al cerrarse, inertiza el
 * fondo para los lectores de pantalla y cierra con Escape. Reimplementarlo a mano son unas
 * cien líneas y varios errores de accesibilidad que nadie nota hasta la auditoría.
 */
export function Dialogo({
  estaAbierto,
  titulo,
  esAncho = false,
  alCerrar,
  children,
}: PropiedadesDialogo) {
  const referenciaAlDialogo = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialogo = referenciaAlDialogo.current;

    if (dialogo === null) {
      return;
    }

    // La comprobación de `open` no es cosmética: `showModal()` sobre un diálogo ya abierto
    // lanza una excepción, y `close()` sobre uno cerrado dispara eventos de más.
    if (estaAbierto && !dialogo.open) {
      dialogo.showModal();
    } else if (!estaAbierto && dialogo.open) {
      dialogo.close();
    }
  }, [estaAbierto]);

  return (
    <dialog
      ref={referenciaAlDialogo}
      className={[estilos.dialogo, esAncho ? estilos.dialogoAncho : ''].filter(Boolean).join(' ')}
      // `cancel` es el evento de la tecla Escape. Sin interceptarlo, el navegador cerraría el
      // diálogo por su cuenta mientras el estado de React lo sigue creyendo abierto, y ya no
      // volvería a abrirse nunca.
      onCancel={(evento) => {
        evento.preventDefault();
        alCerrar();
      }}
    >
      <h2 className={estilos.titulo}>{titulo}</h2>
      {children}
    </dialog>
  );
}
