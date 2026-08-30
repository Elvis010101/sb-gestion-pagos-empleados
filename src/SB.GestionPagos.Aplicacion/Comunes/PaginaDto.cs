namespace SB.GestionPagos.Aplicacion.Comunes;

/// <summary>
/// Página de resultados tal como la recibe el cliente.
/// </summary>
/// <param name="Elementos">Los registros de la página pedida.</param>
/// <param name="TotalRegistros">Total de registros que cumplen el filtro, sin paginar.</param>
/// <param name="Pagina">Número de la página devuelta.</param>
/// <param name="TamanoPagina">Cuántos registros caben en cada página.</param>
/// <remarks>
/// Es el gemelo de <c>PaginaDeRegistros&lt;T&gt;</c> del Dominio, y la duplicación es
/// deliberada: aquel describe lo que devuelve el repositorio; este describe lo que se
/// serializa hacia el navegador, y por eso agrega <see cref="TotalPaginas"/>, que solo le
/// interesa al paginador de la interfaz.
/// </remarks>
public sealed record PaginaDto<T>(
    IReadOnlyList<T> Elementos,
    int TotalRegistros,
    int Pagina,
    int TamanoPagina)
{
    /// <summary>
    /// Cantidad de páginas disponibles. Se calcula en el servidor para que el frontend no
    /// tenga que reproducir la división con redondeo hacia arriba.
    /// </summary>
    public int TotalPaginas => TamanoPagina <= 0
        ? 0
        : (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
}
