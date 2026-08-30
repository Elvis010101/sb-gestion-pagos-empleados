using SB.GestionPagos.Dominio.Enumeraciones;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Aplicacion.Empleados.Dtos;

/// <summary>
/// Criterios de búsqueda y tramo de página que llegan desde la cadena de consulta (RF-03).
/// </summary>
/// <remarks>
/// Se declara con propiedades <c>init</c> y no como <c>record</c> posicional porque el
/// enlazador de modelos de ASP.NET Core construye este tipo desde la query string y necesita
/// un constructor sin parámetros.
///
/// Los criterios son anulables: ausente significa "no filtrar por esto". La paginación, en
/// cambio, siempre tiene valor, porque una consulta sin página es una consulta sin límite.
/// </remarks>
public sealed record FiltroEmpleados
{
    public const int PAGINA_PREDETERMINADA = Paginacion.PAGINA_MINIMA;

    /// <summary>
    /// Tamaño de página cuando el cliente no pide uno. Es una decisión de presentación
    /// —cuántas filas caben cómodamente en la tabla de la maqueta—, por eso vive aquí y no
    /// en el Dominio, que solo fija el máximo tolerable.
    /// </summary>
    public const int TAMANO_PAGINA_PREDETERMINADO = 20;

    /// <summary>Coincidencia parcial contra el primer nombre o el apellido paterno.</summary>
    public string? Nombre { get; init; }

    public string? Departamento { get; init; }

    public EstadoEmpleado? Estado { get; init; }

    public int Pagina { get; init; } = PAGINA_PREDETERMINADA;

    public int TamanoPagina { get; init; } = TAMANO_PAGINA_PREDETERMINADO;
}
