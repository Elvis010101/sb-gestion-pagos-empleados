import type { ButtonHTMLAttributes, ReactNode } from 'react';

import estilos from './Boton.module.css';

type VarianteBoton = 'primario' | 'secundario' | 'peligro';

/**
 * Hereda los atributos nativos de `<button>` en lugar de declarar uno por uno los que hagan
 * falta. Así `type`, `disabled`, `aria-label` o `form` funcionan sin que nadie tenga que
 * acordarse de agregarlos al tipo cada vez que aparece un caso nuevo.
 */
interface PropiedadesBoton extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: VarianteBoton;
  anchoCompleto?: boolean;

  /**
   * Bloquea el botón y cambia su rótulo mientras la operación está en curso. Es lo que
   * impide el doble envío de un formulario, que en una alta significa dos empleados creados.
   */
  estaProcesando?: boolean;

  children: ReactNode;
}

export function Boton({
  variante = 'primario',
  anchoCompleto = false,
  estaProcesando = false,
  disabled,
  children,
  className,
  ...atributosNativos
}: PropiedadesBoton) {
  const clases = [
    estilos.boton,
    estilos[variante],
    anchoCompleto ? estilos.anchoCompleto : '',
    className ?? '',
  ]
    .filter((clase) => clase !== '')
    .join(' ');

  return (
    <button
      {...atributosNativos}
      className={clases}
      disabled={disabled === true || estaProcesando}
      // Le dice a un lector de pantalla que el control está ocupado, no simplemente inerte.
      aria-busy={estaProcesando}
    >
      {estaProcesando ? 'Procesando…' : children}
    </button>
  );
}
