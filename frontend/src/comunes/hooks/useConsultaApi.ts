import { useCallback, useEffect, useState } from 'react';

import { traducirError, type ErrorApi } from '../api/ErrorApi';

export interface EstadoDeConsulta<TDatos> {
  datos: TDatos | null;
  estaCargando: boolean;
  error: ErrorApi | null;

  /** Vuelve a ejecutar la consulta. Es lo que usa el botón "Reintentar". */
  recargar: () => void;
}

/**
 * Lo que quedó de la última consulta terminada, junto con la marca de a QUÉ consulta
 * pertenece.
 *
 * Guardar esa marca es lo que permite deducir si el resultado sigue siendo válido, en vez de
 * llevar un booleano `estaCargando` aparte que haya que acordarse de encender y apagar.
 */
interface ResultadoDeConsulta<TDatos> {
  /** La función que produjo este resultado. Se compara por identidad. */
  origen: () => Promise<TDatos>;

  /** Cuántas recargas se habían pedido cuando se lanzó. */
  intento: number;

  datos: TDatos | null;
  error: ErrorApi | null;
}

/**
 * Ejecuta una consulta a la API y expone sus tres estados posibles.
 *
 * Existe porque las tres pantallas de consulta —empleados, entidades y reporte— necesitan
 * exactamente lo mismo: cargar, mostrar el error si lo hay, mostrar el dato si llegó y poder
 * reintentar. Escrito a mano en cada pantalla, ese patrón se copia con variaciones y alguna
 * de las tres acaba olvidando apagar el indicador de carga cuando falla, dejando el girador
 * dando vueltas para siempre.
 *
 * `estaCargando` NO es un estado propio: se DEDUCE de si el resultado guardado corresponde a
 * la consulta vigente. Además de eliminar una variable que se puede desincronizar, evita
 * encender el indicador desde el cuerpo del efecto, que provoca un dibujado en cascada —React
 * pinta, el efecto cambia el estado, React vuelve a pintar— y que la regla
 * `react-hooks/set-state-in-effect` señala precisamente por eso.
 *
 * @param ejecutarConsulta Debe venir memorizada con `useCallback`. Su identidad es lo que
 *   marca "esta es otra consulta": si fuera una función nueva en cada dibujado, el resultado
 *   nunca se consideraría vigente y el efecto se repetiría sin fin.
 */
export function useConsultaApi<TDatos>(
  ejecutarConsulta: () => Promise<TDatos>,
): EstadoDeConsulta<TDatos> {
  const [intento, establecerIntento] = useState(0);
  const [resultado, establecerResultado] = useState<ResultadoDeConsulta<TDatos> | null>(null);

  const recargar = useCallback(() => {
    establecerIntento((intentoActual) => intentoActual + 1);
  }, []);

  useEffect(() => {
    let laConsultaSigueVigente = true;

    ejecutarConsulta()
      .then((datos) => {
        if (laConsultaSigueVigente) {
          establecerResultado({ origen: ejecutarConsulta, intento, datos, error: null });
        }
      })
      .catch((errorAtrapado: unknown) => {
        if (laConsultaSigueVigente) {
          establecerResultado({
            origen: ejecutarConsulta,
            intento,
            datos: null,
            error: traducirError(errorAtrapado),
          });
        }
      });

    // Descarta la respuesta si entretanto cambió el filtro o el componente se desmontó. Sin
    // esta bandera aparece la condición de carrera clásica de los buscadores: se pide la
    // página 1, se pide la 2, la 1 tarda más y llega después, y la tabla acaba mostrando la
    // página vieja como si fuera la nueva.
    return () => {
      laConsultaSigueVigente = false;
    };
  }, [ejecutarConsulta, intento]);

  const elResultadoEstaAlDia =
    resultado !== null && resultado.origen === ejecutarConsulta && resultado.intento === intento;

  return {
    datos: elResultadoEstaAlDia ? resultado.datos : null,
    error: elResultadoEstaAlDia ? resultado.error : null,
    estaCargando: !elResultadoEstaAlDia,
    recargar,
  };
}
