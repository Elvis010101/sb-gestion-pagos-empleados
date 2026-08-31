/**
 * Formato de presentación de números y fechas.
 *
 * Está centralizado porque el formato es una decisión de la institución, no de cada pantalla:
 * si la nómina se muestra con símbolo de peso en un sitio y sin él en otro, el reporte parece
 * de dos sistemas distintos.
 */

const LOCALIZACION = 'es-DO';
const CODIGO_MONEDA = 'DOP';
const DECIMALES_DE_MONEDA = 2;

/**
 * Se construyen una sola vez, fuera de las funciones. Crear un `Intl.NumberFormat` es caro y
 * hacerlo dentro de una función lo repetiría por cada celda de una tabla de mil filas.
 */
const formateadorDeMoneda = new Intl.NumberFormat(LOCALIZACION, {
  style: 'currency',
  currency: CODIGO_MONEDA,
  minimumFractionDigits: DECIMALES_DE_MONEDA,
  maximumFractionDigits: DECIMALES_DE_MONEDA,
});

const formateadorDeFechaYHora = new Intl.DateTimeFormat(LOCALIZACION, {
  dateStyle: 'medium',
  timeStyle: 'short',
});

export function formatearMoneda(monto: number): string {
  return formateadorDeMoneda.format(monto);
}

export function formatearFechaYHora(fechaEnTextoIso: string): string {
  return formateadorDeFechaYHora.format(new Date(fechaEnTextoIso));
}
