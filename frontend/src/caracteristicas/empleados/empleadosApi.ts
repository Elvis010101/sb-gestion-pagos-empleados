import { clienteHttp } from '../../comunes/api/clienteHttp';
import { limpiarParametros } from '../../comunes/api/parametrosDeConsulta';
import type { PaginaDto } from '../../comunes/tipos/comunes';

import type { EmpleadoDto, FiltroEmpleados } from './tipos';

const RUTA_EMPLEADOS = '/empleados';

/** Consulta paginada con filtros (RF-03). */
export async function buscarEmpleados(filtro: FiltroEmpleados): Promise<PaginaDto<EmpleadoDto>> {
  const respuesta = await clienteHttp.get<PaginaDto<EmpleadoDto>>(RUTA_EMPLEADOS, {
    params: limpiarParametros({
      nombre: filtro.nombre,
      departamento: filtro.departamento,
      estado: filtro.estado,
      pagina: filtro.pagina,
      tamanoPagina: filtro.tamanoPagina,
    }),
  });

  return respuesta.data;
}

export async function obtenerEmpleadoPorId(identificador: number): Promise<EmpleadoDto> {
  const respuesta = await clienteHttp.get<EmpleadoDto>(`${RUTA_EMPLEADOS}/${identificador}`);

  return respuesta.data;
}

/** Baja lógica: el servidor marca al empleado como inactivo, no borra la fila (RF-05). */
export async function eliminarEmpleado(identificador: number): Promise<void> {
  await clienteHttp.delete(`${RUTA_EMPLEADOS}/${identificador}`);
}

/**
 * Alta de un empleado del tipo indicado por su segmento de ruta.
 *
 * Es UNA función genérica y no cuatro (`crearAsalariado`, `crearPorHoras`…). El backend
 * expone un controlador por tipo, pero todos tienen la misma forma: mismo verbo, mismo
 * código de respuesta, misma representación de vuelta. Lo único que cambia es el segmento y
 * la forma del cuerpo, y ambos los aporta quien llama. Cuatro funciones idénticas salvo por
 * un texto son cuatro sitios donde corregir el día que cambie el manejo de la respuesta.
 */
export async function crearEmpleadoDeTipo<TSolicitud>(
  segmentoDeRuta: string,
  solicitud: TSolicitud,
): Promise<EmpleadoDto> {
  const respuesta = await clienteHttp.post<EmpleadoDto>(
    `${RUTA_EMPLEADOS}/${segmentoDeRuta}`,
    solicitud,
  );

  return respuesta.data;
}

/**
 * Edición de un empleado del tipo indicado (RF-05).
 *
 * Devuelve el empleado actualizado porque el servidor recalcula el pago semanal: el cliente
 * no puede conocer el valor nuevo sin pedirlo, y el backend ya lo devuelve para ahorrar una
 * segunda petición.
 */
export async function actualizarEmpleadoDeTipo<TSolicitud>(
  segmentoDeRuta: string,
  identificador: number,
  solicitud: TSolicitud,
): Promise<EmpleadoDto> {
  const respuesta = await clienteHttp.put<EmpleadoDto>(
    `${RUTA_EMPLEADOS}/${segmentoDeRuta}/${identificador}`,
    solicitud,
  );

  return respuesta.data;
}
