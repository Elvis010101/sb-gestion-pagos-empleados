/**
 * Criterios de búsqueda de la consulta de empleados, tal como los captura la interfaz.
 *
 * Viven en su propio módulo y no junto al componente de filtros por una razón concreta: un
 * archivo que exporta a la vez componentes y valores rompe la recarga en caliente de React,
 * que solo puede reemplazar un módulo en vivo si todo lo que exporta es un componente. Al
 * mezclarlos, cada cambio en el filtro recargaría la página entera y se perdería el estado.
 */
export interface CriteriosDeFiltro {
  nombre: string;
  departamento: string;

  /**
   * Texto vacío significa "cualquier estado". Viaja como texto porque sale de un `<select>`,
   * y se convierte a número justo antes de armar la petición.
   */
  estado: string;
}

export const CRITERIOS_DE_FILTRO_VACIOS: CriteriosDeFiltro = {
  nombre: '',
  departamento: '',
  estado: '',
};
