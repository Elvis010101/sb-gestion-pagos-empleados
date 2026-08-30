namespace SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;

/// <summary>
/// Datos de alta de una entidad gubernamental.
/// </summary>
/// <remarks>
/// No lleva <c>Id</c>: en este módulo lo asigna el repositorio de archivo plano, porque no
/// hay motor de base de datos que lo genere. Que el cliente pudiera proponerlo permitiría
/// pisar una entidad existente.
/// </remarks>
public sealed record CrearEntidadGubernamentalDto(
    string Nombre,
    string Categoria,
    string PoderDelEstado,
    string Sector);
