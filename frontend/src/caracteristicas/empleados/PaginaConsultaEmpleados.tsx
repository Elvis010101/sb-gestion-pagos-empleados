import { useCallback, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { traducirError, type ErrorApi } from '../../comunes/api/ErrorApi';
import { Boton } from '../../comunes/componentes/Boton';
import { DialogoDeConfirmacion } from '../../comunes/componentes/DialogoDeConfirmacion';
import { EstadoVacio } from '../../comunes/componentes/EstadoVacio';
import { IndicadorDeCarga } from '../../comunes/componentes/IndicadorDeCarga';
import { MensajeDeError } from '../../comunes/componentes/MensajeDeError';
import { Paginador } from '../../comunes/componentes/Paginador';
import estilosDeTabla from '../../comunes/componentes/Tabla.module.css';
import { useConsultaApi } from '../../comunes/hooks/useConsultaApi';
import { formatearMoneda } from '../../comunes/utilidades/formatos';
import { Tarjeta } from '../../diseno/Tarjeta';
import { Rutas, construirRutaEditarEmpleado } from '../../rutas/rutas';
import { useSesion } from '../autenticacion/useSesion';

import { FiltrosDeEmpleados } from './FiltrosDeEmpleados';
import { CRITERIOS_DE_FILTRO_VACIOS, type CriteriosDeFiltro } from './criteriosDeFiltro';
import { buscarEmpleados, eliminarEmpleado } from './empleadosApi';
import {
  ETIQUETAS_ESTADO_EMPLEADO,
  EstadoEmpleado,
  type EmpleadoDto,
  type FiltroEmpleados,
} from './tipos';

const PRIMERA_PAGINA = 1;

/** Coincide con `FiltroEmpleados.TAMANO_PAGINA_PREDETERMINADO` de la capa Aplicación. */
const TAMANO_PAGINA = 20;

/**
 * Convierte los criterios de la interfaz —todos texto— en el filtro que espera la API.
 *
 * El estado se envía como número porque los enums del backend viajan así; el texto vacío se
 * traduce a `undefined`, que el cliente HTTP omite de la cadena de consulta.
 */
function construirFiltro(criterios: CriteriosDeFiltro, pagina: number): FiltroEmpleados {
  return {
    nombre: criterios.nombre.trim() === '' ? undefined : criterios.nombre.trim(),
    departamento: criterios.departamento.trim() === '' ? undefined : criterios.departamento.trim(),
    estado: criterios.estado === '' ? undefined : (Number(criterios.estado) as EstadoEmpleado),
    pagina,
    tamanoPagina: TAMANO_PAGINA,
  };
}

export function PaginaConsultaEmpleados() {
  const { esAdministrador } = useSesion();
  const navegar = useNavigate();

  const [criterios, establecerCriterios] = useState<CriteriosDeFiltro>(CRITERIOS_DE_FILTRO_VACIOS);
  const [pagina, establecerPagina] = useState(PRIMERA_PAGINA);
  const [empleadoPorDarDeBaja, establecerEmpleadoPorDarDeBaja] = useState<EmpleadoDto | null>(null);
  const [estaDandoDeBaja, establecerEstaDandoDeBaja] = useState(false);
  const [errorDeLaAccion, establecerErrorDeLaAccion] = useState<ErrorApi | null>(null);

  // `useCallback` no es un adorno: `useConsultaApi` tiene esta función en su lista de
  // dependencias, así que una función nueva en cada dibujado provocaría una consulta sin fin.
  const consultar = useCallback(
    () => buscarEmpleados(construirFiltro(criterios, pagina)),
    [criterios, pagina],
  );

  const { datos: paginaDeEmpleados, estaCargando, error, recargar } = useConsultaApi(consultar);

  function aplicarFiltros(criteriosNuevos: CriteriosDeFiltro): void {
    establecerCriterios(criteriosNuevos);

    // Volver a la primera página al cambiar el filtro es obligatorio, no cosmético: si el
    // usuario está en la página 5 y el filtro nuevo devuelve tres registros, la consulta
    // traería una página vacía y parecería que no hay resultados.
    establecerPagina(PRIMERA_PAGINA);
  }

  async function confirmarBaja(): Promise<void> {
    if (empleadoPorDarDeBaja === null) {
      return;
    }

    establecerEstaDandoDeBaja(true);
    establecerErrorDeLaAccion(null);

    try {
      await eliminarEmpleado(empleadoPorDarDeBaja.id);
      establecerEmpleadoPorDarDeBaja(null);
      recargar();
    } catch (errorAtrapado: unknown) {
      establecerErrorDeLaAccion(traducirError(errorAtrapado));
      establecerEmpleadoPorDarDeBaja(null);
    } finally {
      establecerEstaDandoDeBaja(false);
    }
  }

  return (
    <Tarjeta
      titulo="Consulta de empleados"
      descripcion="El pago semanal lo calcula el servidor según el tipo de contrato."
      acciones={
        esAdministrador ? (
          <Boton type="button" onClick={() => navegar(Rutas.CrearRegistro)}>
            Registrar empleado
          </Boton>
        ) : undefined
      }
    >
      <FiltrosDeEmpleados alAplicar={aplicarFiltros} estaCargando={estaCargando} />

      {errorDeLaAccion !== null ? <MensajeDeError error={errorDeLaAccion} /> : null}

      {estaCargando ? <IndicadorDeCarga mensaje="Consultando empleados…" /> : null}

      {!estaCargando && error !== null ? (
        <MensajeDeError error={error} alReintentar={recargar} />
      ) : null}

      {!estaCargando && error === null && paginaDeEmpleados !== null ? (
        paginaDeEmpleados.elementos.length === 0 ? (
          <EstadoVacio
            titulo="No hay empleados que cumplan el filtro"
            descripcion="Pruebe con otros criterios o limpie la búsqueda."
          >
            {/* Un estado vacío que además ofrece la salida evita el callejón sin salida: el
                usuario ve que no hay nada Y qué puede hacer al respecto. */}
            {esAdministrador ? (
              <Boton type="button" onClick={() => navegar(Rutas.CrearRegistro)}>
                Registrar el primer empleado
              </Boton>
            ) : null}
          </EstadoVacio>
        ) : (
          <>
            <div className={estilosDeTabla.envoltorio}>
              <table className={estilosDeTabla.tabla}>
                <caption className="solo-lectores-de-pantalla">
                  Empleados que cumplen el filtro, con su pago semanal calculado
                </caption>
                <thead>
                  <tr>
                    <th scope="col">Nombre</th>
                    <th scope="col">Seguro social</th>
                    <th scope="col">Departamento</th>
                    <th scope="col">Tipo de contrato</th>
                    <th scope="col">Estado</th>
                    <th scope="col" className={estilosDeTabla.columnaNumerica}>
                      Pago semanal
                    </th>
                    {esAdministrador ? (
                      <th scope="col" className={estilosDeTabla.columnaDeAcciones}>
                        Acciones
                      </th>
                    ) : null}
                  </tr>
                </thead>
                <tbody>
                  {paginaDeEmpleados.elementos.map((empleado) => (
                    <tr key={empleado.id}>
                      <td>
                        {empleado.primerNombre} {empleado.apellidoPaterno}
                      </td>
                      <td>{empleado.numeroSeguroSocial}</td>
                      <td>{empleado.departamento}</td>
                      <td>{empleado.tipoContrato}</td>
                      <td>
                        <span
                          className={[
                            estilosDeTabla.insignia,
                            empleado.estado === EstadoEmpleado.Activo
                              ? estilosDeTabla.insigniaActivo
                              : estilosDeTabla.insigniaInactivo,
                          ].join(' ')}
                        >
                          {ETIQUETAS_ESTADO_EMPLEADO[empleado.estado]}
                        </span>
                      </td>
                      <td className={estilosDeTabla.columnaNumerica}>
                        {formatearMoneda(empleado.pagoSemanalCalculado)}
                      </td>

                      {/* Las acciones de escritura no se dibujan para el rol Usuario. */}
                      {esAdministrador ? (
                        <td className={estilosDeTabla.columnaDeAcciones}>
                          <span className={estilosDeTabla.acciones}>
                            <Link to={construirRutaEditarEmpleado(empleado.id)}>
                              <Boton variante="secundario" type="button">
                                Editar
                              </Boton>
                            </Link>

                            {empleado.estado === EstadoEmpleado.Activo ? (
                              <Boton
                                variante="peligro"
                                type="button"
                                onClick={() => establecerEmpleadoPorDarDeBaja(empleado)}
                              >
                                Dar de baja
                              </Boton>
                            ) : null}
                          </span>
                        </td>
                      ) : null}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <Paginador
              pagina={paginaDeEmpleados.pagina}
              totalPaginas={paginaDeEmpleados.totalPaginas}
              totalRegistros={paginaDeEmpleados.totalRegistros}
              alCambiarPagina={establecerPagina}
              estaCargando={estaCargando}
            />
          </>
        )
      ) : null}

      <DialogoDeConfirmacion
        estaAbierto={empleadoPorDarDeBaja !== null}
        titulo="Dar de baja al empleado"
        mensaje={
          empleadoPorDarDeBaja === null
            ? ''
            : `${empleadoPorDarDeBaja.primerNombre} ${empleadoPorDarDeBaja.apellidoPaterno} quedará inactivo y dejará de contar en la nómina semanal. Su historial se conserva.`
        }
        etiquetaDeConfirmacion="Dar de baja"
        estaProcesando={estaDandoDeBaja}
        alConfirmar={confirmarBaja}
        alCancelar={() => establecerEmpleadoPorDarDeBaja(null)}
      />
    </Tarjeta>
  );
}
