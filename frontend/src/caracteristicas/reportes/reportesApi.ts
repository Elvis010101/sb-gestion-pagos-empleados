import { clienteHttp } from '../../comunes/api/clienteHttp';
import { limpiarParametros } from '../../comunes/api/parametrosDeConsulta';

import type { FiltroReporteSemanal, ReporteSemanalDto } from './tipos';

const RUTA_NOMINA_SEMANAL = '/reportes/nomina-semanal';

/**
 * Genera el reporte de la nómina semanal (RF-06).
 *
 * No recibe paginación porque el endpoint no la tiene, y eso es intencional del lado del
 * servidor: el total de un reporte tiene que ser el total. Una "página del reporte" daría una
 * suma parcial presentada como si fuera la nómina completa.
 */
export async function generarReporteSemanal(
  filtro: FiltroReporteSemanal,
): Promise<ReporteSemanalDto> {
  const respuesta = await clienteHttp.get<ReporteSemanalDto>(RUTA_NOMINA_SEMANAL, {
    params: limpiarParametros({
      departamento: filtro.departamento,
      incluirInactivos: filtro.incluirInactivos,
    }),
  });

  return respuesta.data;
}
