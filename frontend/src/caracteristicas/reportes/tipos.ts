/**
 * Contratos del reporte semanal de nómina (RF-06).
 */

/** Un renglón del desglose del pago semanal de un empleado. */
export interface LineaCalculoDto {
  concepto: string;
  monto: number;
}

/** Fila del reporte correspondiente a un empleado. */
export interface LineaReporteEmpleadoDto {
  id: number;

  /** Nombre ya compuesto por el servidor: el reporte es un documento que se lee, no un formulario. */
  nombreCompleto: string;

  departamento: string;
  tipoContrato: string;
  pagoSemanal: number;

  /** Conceptos que componen el total. Es lo que satisface la exigencia de "detallar los cálculos". */
  desglosePago: LineaCalculoDto[];
}

/** El reporte completo. */
export interface ReporteSemanalDto {
  fechaGeneracionUtc: string;

  /**
   * Frase lista para el encabezado que describe a quiénes cubre el reporte. Viaja pegada al
   * total a propósito: un total de nómina sin decir de quiénes es no se puede interpretar en
   * cuanto el reporte se imprime o se pega en un correo.
   */
  poblacionIncluida: string;

  departamento: string | null;
  incluyeInactivos: boolean;
  cantidadEmpleados: number;

  /** Suma calculada por el servidor. El frontend la muestra, nunca la suma por su cuenta. */
  totalNominaSemanal: number;

  empleados: LineaReporteEmpleadoDto[];
}

/**
 * Criterios del reporte.
 *
 * `incluirInactivos` es booleano y no un estado opcional porque el valor por omisión —no
 * enviarlo— tiene que ser el comportamiento seguro: a un empleado dado de baja no se le paga
 * la semana, así que incluirlo sin querer inflaría la nómina.
 */
export interface FiltroReporteSemanal {
  departamento?: string;
  incluirInactivos?: boolean;
}
