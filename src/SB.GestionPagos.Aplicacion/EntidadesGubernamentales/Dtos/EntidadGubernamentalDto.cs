namespace SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;

/// <summary>
/// Representación de lectura de una entidad gubernamental (RF-09).
/// </summary>
/// <remarks>
/// Su forma es idéntica a la de la entidad del Dominio, y aun así existe: el DTO congela el
/// contrato publicado. Si mañana la entidad gana un campo interno —un sello de auditoría, por
/// ejemplo— este DTO no cambia, y ningún cliente se entera de algo que no le corresponde.
/// </remarks>
public sealed record EntidadGubernamentalDto(
    int Id,
    string Nombre,
    string Categoria,
    string PoderDelEstado,
    string Sector);
