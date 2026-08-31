import { clienteHttp } from '../../comunes/api/clienteHttp';
import { limpiarParametros } from '../../comunes/api/parametrosDeConsulta';

import type {
  ActualizarEntidadGubernamentalDto,
  CrearEntidadGubernamentalDto,
  EntidadGubernamentalDto,
  FiltroEntidadesGubernamentales,
} from './tipos';

const RUTA_ENTIDADES = '/entidades-gubernamentales';

/**
 * Devuelve las entidades que cumplen el filtro (RF-09).
 *
 * El tipo de retorno es una lista y no una página porque el endpoint no pagina: el catálogo
 * tiene 181 registros con techo conocido. Reflejar aquí una paginación que el servidor no
 * ofrece sería inventar un contrato.
 */
export async function buscarEntidadesGubernamentales(
  filtro: FiltroEntidadesGubernamentales,
): Promise<EntidadGubernamentalDto[]> {
  const respuesta = await clienteHttp.get<EntidadGubernamentalDto[]>(RUTA_ENTIDADES, {
    params: limpiarParametros({ nombre: filtro.nombre, sector: filtro.sector }),
  });

  return respuesta.data;
}

export async function crearEntidadGubernamental(
  solicitud: CrearEntidadGubernamentalDto,
): Promise<EntidadGubernamentalDto> {
  const respuesta = await clienteHttp.post<EntidadGubernamentalDto>(RUTA_ENTIDADES, solicitud);

  return respuesta.data;
}

export async function actualizarEntidadGubernamental(
  identificador: number,
  solicitud: ActualizarEntidadGubernamentalDto,
): Promise<EntidadGubernamentalDto> {
  const respuesta = await clienteHttp.put<EntidadGubernamentalDto>(
    `${RUTA_ENTIDADES}/${identificador}`,
    solicitud,
  );

  return respuesta.data;
}

/**
 * Elimina una entidad del catálogo.
 *
 * Aquí la baja SÍ es física, a diferencia de la de empleados: esto es un catálogo, no un
 * historial, y una entidad retirada no deja pagos detrás que haya que poder rastrear.
 */
export async function eliminarEntidadGubernamental(identificador: number): Promise<void> {
  await clienteHttp.delete(`${RUTA_ENTIDADES}/${identificador}`);
}
