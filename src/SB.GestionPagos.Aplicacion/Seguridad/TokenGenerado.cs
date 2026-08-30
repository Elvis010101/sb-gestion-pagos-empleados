namespace SB.GestionPagos.Aplicacion.Seguridad;

/// <summary>
/// Token emitido para un usuario autenticado, junto a su vencimiento.
/// </summary>
/// <remarks>
/// La fecha viaja aparte y no se deja "dentro del token" porque quien la necesita es el
/// servicio de autenticación para armar su respuesta, y obligarlo a abrir el JWT para leerla
/// significaría que la capa Aplicación tiene que entender el formato de un JWT: exactamente
/// el detalle que esta abstracción existe para ocultar.
/// </remarks>
public sealed record TokenGenerado(string Token, DateTime FechaExpiracionUtc);
