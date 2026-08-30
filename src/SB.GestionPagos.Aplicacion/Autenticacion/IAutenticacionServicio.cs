using SB.GestionPagos.Aplicacion.Autenticacion.Dtos;
using SB.GestionPagos.Aplicacion.Comunes;

namespace SB.GestionPagos.Aplicacion.Autenticacion;

/// <summary>
/// Caso de uso de inicio de sesión (RF-07).
/// </summary>
public interface IAutenticacionServicio
{
    /// <summary>
    /// Verifica las credenciales y, si son correctas, emite un token.
    /// </summary>
    /// <remarks>
    /// Devuelve <c>Resultado</c> y no lanza una excepción cuando las credenciales fallan
    /// porque un intento fallido de inicio de sesión no es una anomalía: es una de las dos
    /// salidas normales de la operación, y ocurrirá miles de veces.
    /// </remarks>
    Task<Resultado<RespuestaInicioSesionDto>> IniciarSesionAsync(
        SolicitudInicioSesionDto solicitud,
        CancellationToken cancelacion);
}
