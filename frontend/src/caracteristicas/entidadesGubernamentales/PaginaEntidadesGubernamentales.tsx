import { useCallback, useState, type FormEvent } from 'react';

import { traducirError, type ErrorApi } from '../../comunes/api/ErrorApi';
import { Boton } from '../../comunes/componentes/Boton';
import { CampoDeTexto } from '../../comunes/componentes/CampoDeTexto';
import { Dialogo } from '../../comunes/componentes/Dialogo';
import { DialogoDeConfirmacion } from '../../comunes/componentes/DialogoDeConfirmacion';
import { EstadoVacio } from '../../comunes/componentes/EstadoVacio';
import { IndicadorDeCarga } from '../../comunes/componentes/IndicadorDeCarga';
import { MensajeDeError } from '../../comunes/componentes/MensajeDeError';
import estilosDeTabla from '../../comunes/componentes/Tabla.module.css';
import { useConsultaApi } from '../../comunes/hooks/useConsultaApi';
import { Tarjeta } from '../../diseno/Tarjeta';
import { useSesion } from '../autenticacion/useSesion';

import {
  FormularioEntidadGubernamental,
  type ValoresFormularioEntidad,
} from './FormularioEntidadGubernamental';
import {
  actualizarEntidadGubernamental,
  buscarEntidadesGubernamentales,
  crearEntidadGubernamental,
  eliminarEntidadGubernamental,
} from './entidadesGubernamentalesApi';
import estilos from './PaginaEntidadesGubernamentales.module.css';
import type { EntidadGubernamentalDto } from './tipos';

interface CriteriosDeBusqueda {
  nombre: string;
  sector: string;
}

const CRITERIOS_VACIOS: CriteriosDeBusqueda = { nombre: '', sector: '' };

/**
 * Qué está haciendo el formulario en este momento.
 *
 * Se modela como un solo estado con tres formas posibles y no como dos booleanos sueltos
 * (`estaCreando`, `estaEditando`): con booleanos independientes existe el estado imposible
 * "creando y editando a la vez", y basta un `setState` mal puesto para caer en él. Aquí ese
 * estado no se puede ni escribir.
 */
type ModoDelFormulario =
  | { tipo: 'cerrado' }
  | { tipo: 'creando' }
  | { tipo: 'editando'; entidad: EntidadGubernamentalDto };

export function PaginaEntidadesGubernamentales() {
  const { esAdministrador } = useSesion();

  const [criterios, establecerCriterios] = useState<CriteriosDeBusqueda>(CRITERIOS_VACIOS);
  const [borrador, establecerBorrador] = useState<CriteriosDeBusqueda>(CRITERIOS_VACIOS);
  const [modoDelFormulario, establecerModoDelFormulario] = useState<ModoDelFormulario>({
    tipo: 'cerrado',
  });
  const [entidadPorEliminar, establecerEntidadPorEliminar] =
    useState<EntidadGubernamentalDto | null>(null);
  const [estaEliminando, establecerEstaEliminando] = useState(false);
  const [errorDeLaAccion, establecerErrorDeLaAccion] = useState<ErrorApi | null>(null);

  const consultar = useCallback(
    () =>
      buscarEntidadesGubernamentales({
        nombre: criterios.nombre.trim() === '' ? undefined : criterios.nombre.trim(),
        sector: criterios.sector.trim() === '' ? undefined : criterios.sector.trim(),
      }),
    [criterios],
  );

  const { datos: entidades, estaCargando, error, recargar } = useConsultaApi(consultar);

  function buscar(evento: FormEvent<HTMLFormElement>): void {
    evento.preventDefault();
    establecerCriterios(borrador);
  }

  function limpiar(): void {
    establecerBorrador(CRITERIOS_VACIOS);
    establecerCriterios(CRITERIOS_VACIOS);
  }

  async function guardar(valores: ValoresFormularioEntidad): Promise<void> {
    if (modoDelFormulario.tipo === 'editando') {
      await actualizarEntidadGubernamental(modoDelFormulario.entidad.id, valores);
    } else {
      await crearEntidadGubernamental(valores);
    }

    establecerModoDelFormulario({ tipo: 'cerrado' });
    recargar();
  }

  async function confirmarEliminacion(): Promise<void> {
    if (entidadPorEliminar === null) {
      return;
    }

    establecerEstaEliminando(true);
    establecerErrorDeLaAccion(null);

    try {
      await eliminarEntidadGubernamental(entidadPorEliminar.id);
      establecerEntidadPorEliminar(null);
      recargar();
    } catch (errorAtrapado: unknown) {
      establecerErrorDeLaAccion(traducirError(errorAtrapado));
      establecerEntidadPorEliminar(null);
    } finally {
      establecerEstaEliminando(false);
    }
  }

  return (
    <Tarjeta
      titulo="Entidades gubernamentales"
      descripcion="Catálogo de entidades del Estado dominicano. Se persiste en archivo de texto plano."
      acciones={
        esAdministrador ? (
          <Boton type="button" onClick={() => establecerModoDelFormulario({ tipo: 'creando' })}>
            Agregar entidad
          </Boton>
        ) : undefined
      }
    >
      <form className={estilos.busqueda} onSubmit={buscar} role="search">
        <CampoDeTexto
          className={estilos.campo}
          etiqueta="Nombre"
          placeholder="Coincidencia parcial, sin distinguir acentos"
          value={borrador.nombre}
          onChange={(evento) =>
            establecerBorrador((actual) => ({ ...actual, nombre: evento.target.value }))
          }
        />

        <CampoDeTexto
          className={estilos.campo}
          etiqueta="Sector"
          placeholder="Coincidencia exacta"
          value={borrador.sector}
          onChange={(evento) =>
            establecerBorrador((actual) => ({ ...actual, sector: evento.target.value }))
          }
        />

        <div className={estilos.acciones}>
          <Boton type="submit" estaProcesando={estaCargando}>
            Buscar
          </Boton>
          <Boton variante="secundario" type="button" onClick={limpiar} disabled={estaCargando}>
            Limpiar
          </Boton>
        </div>
      </form>

      {errorDeLaAccion !== null ? <MensajeDeError error={errorDeLaAccion} /> : null}

      {estaCargando ? <IndicadorDeCarga mensaje="Consultando el catálogo…" /> : null}

      {!estaCargando && error !== null ? (
        <MensajeDeError error={error} alReintentar={recargar} />
      ) : null}

      {!estaCargando && error === null && entidades !== null ? (
        entidades.length === 0 ? (
          <EstadoVacio
            titulo="No hay entidades que cumplan la búsqueda"
            descripcion="Pruebe con otro nombre o limpie los criterios."
          />
        ) : (
          <>
            <div className={estilosDeTabla.envoltorio}>
              <table className={estilosDeTabla.tabla}>
                <caption className="solo-lectores-de-pantalla">
                  Entidades gubernamentales que cumplen la búsqueda
                </caption>
                <thead>
                  <tr>
                    <th scope="col">Nombre</th>
                    <th scope="col">Categoría</th>
                    <th scope="col">Poder del Estado</th>
                    <th scope="col">Sector</th>
                    {esAdministrador ? (
                      <th scope="col" className={estilosDeTabla.columnaDeAcciones}>
                        Acciones
                      </th>
                    ) : null}
                  </tr>
                </thead>
                <tbody>
                  {entidades.map((entidad) => (
                    <tr key={entidad.id}>
                      <td>{entidad.nombre}</td>
                      <td>{entidad.categoria}</td>
                      <td>{entidad.poderDelEstado}</td>
                      <td>{entidad.sector}</td>

                      {esAdministrador ? (
                        <td className={estilosDeTabla.columnaDeAcciones}>
                          <span className={estilosDeTabla.acciones}>
                            <Boton
                              variante="secundario"
                              type="button"
                              onClick={() =>
                                establecerModoDelFormulario({ tipo: 'editando', entidad })
                              }
                            >
                              Editar
                            </Boton>
                            <Boton
                              variante="peligro"
                              type="button"
                              onClick={() => establecerEntidadPorEliminar(entidad)}
                            >
                              Eliminar
                            </Boton>
                          </span>
                        </td>
                      ) : null}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <p className={estilos.resumen}>
              {entidades.length} {entidades.length === 1 ? 'entidad' : 'entidades'} en el listado.
            </p>
          </>
        )
      ) : null}

      <Dialogo
        estaAbierto={modoDelFormulario.tipo !== 'cerrado'}
        titulo={
          modoDelFormulario.tipo === 'editando' ? 'Editar entidad' : 'Agregar entidad al catálogo'
        }
        esAncho
        alCerrar={() => establecerModoDelFormulario({ tipo: 'cerrado' })}
      >
        {/* El formulario se monta solo cuando el diálogo está abierto: así react-hook-form
            toma sus valores iniciales de la entidad correcta cada vez, en lugar de conservar
            los de la que se editó antes. */}
        {modoDelFormulario.tipo !== 'cerrado' ? (
          <FormularioEntidadGubernamental
            entidad={modoDelFormulario.tipo === 'editando' ? modoDelFormulario.entidad : null}
            alGuardar={guardar}
            alCancelar={() => establecerModoDelFormulario({ tipo: 'cerrado' })}
          />
        ) : null}
      </Dialogo>

      <DialogoDeConfirmacion
        estaAbierto={entidadPorEliminar !== null}
        titulo="Eliminar entidad"
        mensaje={
          entidadPorEliminar === null
            ? ''
            : `"${entidadPorEliminar.nombre}" se eliminará del catálogo de forma permanente. Esta baja no se puede deshacer.`
        }
        etiquetaDeConfirmacion="Eliminar"
        estaProcesando={estaEliminando}
        alConfirmar={confirmarEliminacion}
        alCancelar={() => establecerEntidadPorEliminar(null)}
      />
    </Tarjeta>
  );
}
