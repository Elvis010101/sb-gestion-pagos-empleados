namespace SB.GestionPagos.Dominio.Repositorios;

/// <summary>
/// Criterios de búsqueda del catálogo de entidades gubernamentales (RF-09).
/// Todo criterio nulo significa "no filtrar".
/// </summary>
/// <param name="Nombre">Coincidencia PARCIAL contra el nombre de la entidad.</param>
/// <param name="Sector">Sector EXACTO al que pertenece la entidad.</param>
/// <remarks>
/// Los dos criterios no se comparan igual, y es deliberado. El nombre es texto libre que el
/// usuario teclea, así que una coincidencia parcial es lo único útil. El sector, en cambio,
/// sale de una lista cerrada de 25 valores que la interfaz presenta como desplegable: ahí una
/// coincidencia parcial sería un defecto, porque buscar "Educación" también devolvería
/// "Educación Superior, Ciencia y Tecnología", que es otro sector distinto.
///
/// En ambos casos la comparación ignora mayúsculas Y acentos: quien escribe "educacion" desde
/// un teclado sin tildes espera encontrar "Educación".
///
/// Es un objeto y no dos parámetros sueltos por el mismo motivo que
/// <see cref="FiltroBusquedaEmpleado"/>: agregar un criterio más adelante no debe cambiar la
/// firma del repositorio ni obligar a tocar las implementaciones existentes.
/// </remarks>
public sealed record FiltroBusquedaEntidadGubernamental(
    string? Nombre = null,
    string? Sector = null);
