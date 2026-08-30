namespace SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;

/// <summary>
/// Criterios de búsqueda del catálogo de entidades gubernamentales (RF-09).
/// </summary>
/// <remarks>
/// Se declara con propiedades <c>init</c>, igual que <see cref="Empleados.Dtos.FiltroEmpleados"/>,
/// porque el enlazador de modelos de ASP.NET Core lo construye desde la cadena de consulta y
/// necesita un constructor sin parámetros.
///
/// No trae paginación, y esa es la diferencia deliberada con el filtro de empleados: el
/// catálogo tiene 181 registros con un techo conocido, así que la lista completa cabe en una
/// respuesta sin comprometer el RNF-04. La tabla de empleados no tiene techo.
/// </remarks>
public sealed record FiltroEntidadesGubernamentales
{
    /// <summary>Coincidencia parcial contra el nombre, ignorando mayúsculas y acentos.</summary>
    public string? Nombre { get; init; }

    /// <summary>Sector exacto, ignorando mayúsculas y acentos.</summary>
    public string? Sector { get; init; }
}
