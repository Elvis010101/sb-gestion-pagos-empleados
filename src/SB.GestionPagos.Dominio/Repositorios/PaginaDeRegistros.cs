namespace SB.GestionPagos.Dominio.Repositorios;

/// <summary>
/// Tramo de registros devuelto por una consulta paginada, junto al total que existe
/// detrás del filtro aplicado.
/// </summary>
/// <param name="Elementos">Los registros de la página solicitada.</param>
/// <param name="TotalRegistros">
/// Cuántos registros satisfacen el filtro en total, ignorando la paginación.
/// </param>
/// <remarks>
/// El total viaja junto a los elementos porque la interfaz necesita ambos para dibujar el
/// paginador, y pedirlos en dos llamadas separadas abriría una ventana en la que el total
/// y la página provienen de estados distintos de la tabla.
/// </remarks>
public sealed record PaginaDeRegistros<T>(IReadOnlyList<T> Elementos, int TotalRegistros);
