/**
 * Contratos del catálogo de entidades gubernamentales (RF-09).
 */

/** Representación de lectura de una entidad del catálogo. */
export interface EntidadGubernamentalDto {
  id: number;
  nombre: string;
  categoria: string;
  poderDelEstado: string;
  sector: string;
}

/**
 * Criterios de búsqueda del catálogo.
 *
 * No trae paginación, y no es un olvido: el endpoint tampoco la tiene. El catálogo son 181
 * registros con techo conocido, así que la lista completa cabe en una respuesta.
 */
export interface FiltroEntidadesGubernamentales {
  nombre?: string;
  sector?: string;
}

/**
 * Datos de alta. Sin `id`: en este módulo lo asigna el repositorio de archivo plano, y
 * dejar que el cliente lo proponga permitiría pisar una entidad existente.
 */
export interface CrearEntidadGubernamentalDto {
  nombre: string;
  categoria: string;
  poderDelEstado: string;
  sector: string;
}

/**
 * Datos de edición. Hoy tiene los mismos campos que el alta y aun así es un tipo aparte,
 * igual que en el backend: son dos contratos con vidas distintas.
 */
export interface ActualizarEntidadGubernamentalDto {
  nombre: string;
  categoria: string;
  poderDelEstado: string;
  sector: string;
}
