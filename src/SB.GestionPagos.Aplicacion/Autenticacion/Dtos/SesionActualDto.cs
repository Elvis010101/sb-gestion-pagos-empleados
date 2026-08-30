using SB.GestionPagos.Dominio.Enumeraciones;

namespace SB.GestionPagos.Aplicacion.Autenticacion.Dtos;

/// <summary>
/// Identidad del usuario dueño del token con el que se hizo la petición.
/// </summary>
/// <param name="NombreUsuario">Nombre a mostrar en la interfaz.</param>
/// <param name="Rol">Rol con el que el servidor está tratando a este usuario.</param>
/// <remarks>
/// Lo consume el frontend al recargar la página: el token está guardado en el navegador, pero
/// no se puede confiar en lo que el navegador diga sobre él —cualquiera puede editar lo que
/// hay en el almacenamiento local—. Preguntarle al servidor es la única forma de saber si el
/// token sigue vigente y qué rol reconoce de verdad.
/// </remarks>
public sealed record SesionActualDto(string NombreUsuario, RolUsuario Rol);
