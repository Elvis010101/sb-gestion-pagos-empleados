using SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Servicios.Mapeos;

/// <summary>
/// Traduce entidades gubernamentales a su DTO de lectura.
/// </summary>
internal static class MapeadorEntidadGubernamental
{
    internal static EntidadGubernamentalDto ADto(EntidadGubernamental entidadGubernamental)
        => new(
            entidadGubernamental.Id,
            entidadGubernamental.Nombre,
            entidadGubernamental.Categoria,
            entidadGubernamental.PoderDelEstado,
            entidadGubernamental.Sector);
}
