using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Aplicacion.Autenticacion.Dtos;

/// <summary>
/// Respuesta de un inicio de sesión exitoso.
/// </summary>
/// <param name="Token">JWT que el cliente enviará en la cabecera <c>Authorization: Bearer</c>.</param>
/// <param name="FechaExpiracionUtc">Instante en que el token deja de ser válido.</param>
/// <param name="NombreUsuario">Nombre a mostrar en la interfaz.</param>
/// <param name="Rol">Rol del usuario, para que el frontend decida qué opciones dibuja.</param>
/// <remarks>
/// El rol viaja además dentro del token: el que va aquí es una conveniencia de presentación.
/// La autorización real la resuelve el servidor leyendo el claim firmado, nunca este campo,
/// porque cualquier cliente puede alterar lo que recibió.
/// </remarks>
public sealed record RespuestaInicioSesionDto(
    string Token,
    DateTime FechaExpiracionUtc,
    string NombreUsuario,
    RolUsuario Rol);
