namespace SB.GestionPagos.Infraestructura.DatosPlanos;

/// <summary>
/// Una línea del archivo de entidades gubernamentales, tal como está escrita en disco.
/// </summary>
/// <remarks>
/// No es la entidad del Dominio, y esa separación es intencional. Este tipo es el FORMATO
/// del archivo: sus nombres de propiedad son las claves JSON que ya están grabadas en las
/// 181 líneas existentes. La entidad del Dominio, en cambio, es libre de renombrar sus
/// propiedades, ganar invariantes o cambiar de forma, porque nadie la serializa.
///
/// Si se fusionaran en un solo tipo, renombrar una propiedad del Dominio dejaría el archivo
/// ilegible en silencio: la deserialización no falla, simplemente devuelve null en el campo
/// que ya no encuentra. El mismo motivo por el que Aplicación tiene DTOs en vez de exponer
/// entidades, aplicado a la frontera de persistencia.
///
/// Las propiedades son anulables a propósito: describen lo que el archivo PUEDE traer, no lo
/// que es válido. Si una línea viene incompleta, quien valida es el constructor del Dominio.
/// </remarks>
internal sealed record RegistroEntidadGubernamental(
    int Id,
    string? Nombre,
    string? Categoria,
    string? PoderDelEstado,
    string? Sector);
