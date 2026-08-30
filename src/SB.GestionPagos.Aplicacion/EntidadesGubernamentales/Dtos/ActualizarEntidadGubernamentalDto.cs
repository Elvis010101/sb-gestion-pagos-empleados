namespace SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;

/// <summary>
/// Datos de edición de una entidad gubernamental.
/// </summary>
/// <remarks>
/// Hoy tiene los mismos campos que el DTO de alta y, aun así, es un tipo aparte: son dos
/// contratos con vidas distintas. Cuando la edición necesite algo que el alta no tiene
/// —una marca de concurrencia para detectar ediciones simultáneas sobre el archivo—, se
/// agrega aquí sin tocar el contrato de creación ni a quienes ya lo consumen.
/// </remarks>
public sealed record ActualizarEntidadGubernamentalDto(
    string Nombre,
    string Categoria,
    string PoderDelEstado,
    string Sector);
