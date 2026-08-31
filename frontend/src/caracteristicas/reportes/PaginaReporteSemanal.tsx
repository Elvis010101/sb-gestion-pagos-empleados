import { Fragment, useCallback, useState, type FormEvent } from 'react';

import { Boton } from '../../comunes/componentes/Boton';
import { CampoDeTexto } from '../../comunes/componentes/CampoDeTexto';
import estilosDeCampos from '../../comunes/componentes/Campos.module.css';
import { EstadoVacio } from '../../comunes/componentes/EstadoVacio';
import { IndicadorDeCarga } from '../../comunes/componentes/IndicadorDeCarga';
import { MensajeDeError } from '../../comunes/componentes/MensajeDeError';
import estilosDeTabla from '../../comunes/componentes/Tabla.module.css';
import { useConsultaApi } from '../../comunes/hooks/useConsultaApi';
import { formatearFechaYHora, formatearMoneda } from '../../comunes/utilidades/formatos';
import { Tarjeta } from '../../diseno/Tarjeta';

import estilos from './PaginaReporteSemanal.module.css';
import { generarReporteSemanal } from './reportesApi';
import type { FiltroReporteSemanal } from './tipos';

const FILTRO_INICIAL: FiltroReporteSemanal = { departamento: '', incluirInactivos: false };

/**
 * Columnas de la tabla. La fila del desglose las abarca todas con `colSpan`, y ese número
 * tiene que seguir a la tabla: si se agrega una columna y aquí sigue diciendo cinco, el
 * desglose se corta a media fila.
 */
const CANTIDAD_DE_COLUMNAS_DEL_REPORTE = 5;

export function PaginaReporteSemanal() {
  const [filtro, establecerFiltro] = useState<FiltroReporteSemanal>(FILTRO_INICIAL);
  const [borrador, establecerBorrador] = useState<FiltroReporteSemanal>(FILTRO_INICIAL);

  /**
   * Identificadores de las filas con el desglose desplegado.
   *
   * Es un `Set` y no una lista porque las tres operaciones que hacen falta —¿está?, agregar,
   * quitar— son constantes en un conjunto y lineales en un arreglo. Con mil empleados en el
   * reporte, eso es la diferencia entre abrir una fila al instante y notar el retraso.
   */
  const [filasDesplegadas, establecerFilasDesplegadas] = useState<ReadonlySet<number>>(new Set());

  const consultar = useCallback(() => generarReporteSemanal(filtro), [filtro]);

  const { datos: reporte, estaCargando, error, recargar } = useConsultaApi(consultar);

  function generar(evento: FormEvent<HTMLFormElement>): void {
    evento.preventDefault();
    establecerFiltro(borrador);
    establecerFilasDesplegadas(new Set());
  }

  function alternarDesglose(identificador: number): void {
    establecerFilasDesplegadas((actuales) => {
      // Se construye un conjunto NUEVO en vez de mutar el existente. React compara por
      // identidad: mutar el que ya está guardado no cambiaría la referencia y la pantalla no
      // se volvería a dibujar, aunque el dato sí hubiera cambiado.
      const siguientes = new Set(actuales);

      if (siguientes.has(identificador)) {
        siguientes.delete(identificador);
      } else {
        siguientes.add(identificador);
      }

      return siguientes;
    });
  }

  return (
    <Tarjeta
      titulo="Reporte semanal de nómina"
      descripcion="Total a pagar en la semana, con el desglose del cálculo de cada empleado."
    >
      <form className={estilos.filtros} onSubmit={generar}>
        <CampoDeTexto
          className={estilos.campo}
          etiqueta="Departamento"
          placeholder="Vacío: todos los departamentos"
          value={borrador.departamento ?? ''}
          onChange={(evento) =>
            establecerBorrador((actual) => ({ ...actual, departamento: evento.target.value }))
          }
        />

        <label className={estilosDeCampos.casilla}>
          <input
            type="checkbox"
            checked={borrador.incluirInactivos ?? false}
            onChange={(evento) =>
              establecerBorrador((actual) => ({
                ...actual,
                incluirInactivos: evento.target.checked,
              }))
            }
          />
          Incluir empleados dados de baja
        </label>

        <Boton type="submit" estaProcesando={estaCargando}>
          Generar reporte
        </Boton>
      </form>

      {estaCargando ? <IndicadorDeCarga mensaje="Calculando la nómina…" /> : null}

      {!estaCargando && error !== null ? (
        <MensajeDeError error={error} alReintentar={recargar} />
      ) : null}

      {!estaCargando && error === null && reporte !== null ? (
        <>
          <div className={estilos.resumen}>
            <div className={estilos.indicador}>
              <span className={estilos.etiquetaDelIndicador}>Total de la nómina semanal</span>
              <span className={estilos.valorDelIndicador}>
                {formatearMoneda(reporte.totalNominaSemanal)}
              </span>
            </div>

            <div className={estilos.indicador}>
              <span className={estilos.etiquetaDelIndicador}>Empleados incluidos</span>
              <span className={estilos.valorDelIndicador}>{reporte.cantidadEmpleados}</span>
            </div>

            <div className={estilos.indicador}>
              {/* La descripción de la población viaja PEGADA al total desde el servidor: un
                  total de nómina sin decir de quiénes es no se puede interpretar en cuanto el
                  reporte se imprime o se pega en un correo. */}
              <span className={estilos.etiquetaDelIndicador}>Población incluida</span>
              <span className={estilos.poblacion}>{reporte.poblacionIncluida}</span>
            </div>
          </div>

          {reporte.empleados.length === 0 ? (
            <EstadoVacio
              titulo="No hay empleados en este reporte"
              descripcion="Ningún empleado cumple los criterios seleccionados."
            />
          ) : (
            <div className={estilosDeTabla.envoltorio}>
              <table className={estilosDeTabla.tabla}>
                <caption className="solo-lectores-de-pantalla">
                  Detalle de la nómina semanal por empleado
                </caption>
                <thead>
                  <tr>
                    <th scope="col">Empleado</th>
                    <th scope="col">Departamento</th>
                    <th scope="col">Tipo de contrato</th>
                    <th scope="col" className={estilosDeTabla.columnaNumerica}>
                      Pago semanal
                    </th>
                    <th scope="col" className={estilosDeTabla.columnaDeAcciones}>
                      Desglose
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {reporte.empleados.map((linea) => {
                    const estaDesplegada = filasDesplegadas.has(linea.id);

                    return (
                      // El fragmento lleva la `key` porque agrupa DOS filas hermanas por
                      // empleado: la de datos y la del desglose. Poner la clave en el `<tr>`
                      // interno no serviría: React necesita identificar al elemento que
                      // ocupa la posición en la lista, y ese es el fragmento.
                      <Fragment key={linea.id}>
                        <tr>
                          <td>{linea.nombreCompleto}</td>
                          <td>{linea.departamento}</td>
                          <td>{linea.tipoContrato}</td>
                          <td className={estilosDeTabla.columnaNumerica}>
                            {formatearMoneda(linea.pagoSemanal)}
                          </td>
                          <td className={estilosDeTabla.columnaDeAcciones}>
                            <Boton
                              variante="secundario"
                              type="button"
                              onClick={() => alternarDesglose(linea.id)}
                              // Anuncia a un lector de pantalla que este botón despliega
                              // contenido y en qué estado está ahora.
                              aria-expanded={estaDesplegada}
                            >
                              {estaDesplegada ? 'Ocultar' : 'Ver'}
                            </Boton>
                          </td>
                        </tr>

                        {/* Es lo que satisface la exigencia del RF-06 de "detallar los
                            cálculos según el tipo de contrato": los conceptos vienen
                            calculados por el Dominio, no reconstruidos aquí. */}
                        {estaDesplegada ? (
                          <tr className={estilos.filaDeDesglose}>
                            <td colSpan={CANTIDAD_DE_COLUMNAS_DEL_REPORTE}>
                              <div className={estilos.desglose}>
                                {linea.desglosePago.map((lineaDeCalculo) => (
                                  <span
                                    className={estilos.lineaDeDesglose}
                                    key={lineaDeCalculo.concepto}
                                  >
                                    <span>{lineaDeCalculo.concepto}</span>
                                    <span className={estilos.montoDeLinea}>
                                      {formatearMoneda(lineaDeCalculo.monto)}
                                    </span>
                                  </span>
                                ))}
                              </div>
                            </td>
                          </tr>
                        ) : null}
                      </Fragment>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}

          <p className={estilos.piePersistente}>
            Reporte generado el {formatearFechaYHora(reporte.fechaGeneracionUtc)}. El total lo
            calcula el servidor: la interfaz no suma las filas, para que no existan dos versiones de
            cuánto se paga esta semana.
          </p>
        </>
      ) : null}
    </Tarjeta>
  );
}
