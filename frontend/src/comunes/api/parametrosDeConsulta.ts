/**
 * Valor admisible en la cadena de consulta. No se acepta `object` a propósito: un objeto
 * anidado se serializaría como "[object Object]" sin que nadie se entere.
 */
type ValorDeParametro = string | number | boolean | undefined;

/**
 * Descarta los criterios vacíos antes de armar la cadena de consulta.
 *
 * Hace falta porque un campo de filtro que el usuario dejó en blanco vale `''`, y axios sí
 * envía la cadena vacía: la petición saldría con `?nombre=` y el backend interpretaría que
 * se está filtrando por un nombre vacío en lugar de no filtrar. `undefined` sí lo omite
 * axios, pero eso obligaría a cada pantalla a acordarse de convertir `''` en `undefined`.
 */
export function limpiarParametros(
  parametros: Record<string, ValorDeParametro>,
): Record<string, string | number | boolean> {
  const parametrosLimpios: Record<string, string | number | boolean> = {};

  for (const [clave, valor] of Object.entries(parametros)) {
    if (valor === undefined) {
      continue;
    }

    if (typeof valor === 'string' && valor.trim() === '') {
      continue;
    }

    parametrosLimpios[clave] = typeof valor === 'string' ? valor.trim() : valor;
  }

  return parametrosLimpios;
}
