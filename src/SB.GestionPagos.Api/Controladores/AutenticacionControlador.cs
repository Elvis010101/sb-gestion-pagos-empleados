using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SB.GestionPagos.Api.Seguridad;
using SB.GestionPagos.Aplicacion.Autenticacion;
using SB.GestionPagos.Aplicacion.Autenticacion.Dtos;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Dominio.Enumeraciones;
using SB.GestionPagos.Infraestructura.Seguridad;

namespace SB.GestionPagos.Api.Controladores;

/// <summary>
/// Inicio de sesión y consulta de la sesión vigente (RF-07).
/// </summary>
/// <remarks>
/// El controlador no verifica contraseñas ni firma tokens: recibe, delega en
/// <see cref="IAutenticacionServicio"/> y traduce el resultado a un código HTTP. Esa es toda
/// su responsabilidad, y por eso el proyecto Api puede llamarse "solo host".
/// </remarks>
[Route("api/autenticacion")]
public sealed class AutenticacionControlador : ControladorApi
{
    private readonly IAutenticacionServicio _autenticacionServicio;

    public AutenticacionControlador(IAutenticacionServicio autenticacionServicio)
    {
        _autenticacionServicio = autenticacionServicio;
    }

    /// <summary>
    /// Verifica las credenciales y devuelve el token de acceso.
    /// </summary>
    /// <remarks>
    /// Es el ÚNICO endpoint anónimo del sistema. Todo lo demás queda cerrado por la política
    /// de reserva del host, así que un controlador nuevo nace protegido aunque su autor no
    /// escriba ningún atributo.
    /// </remarks>
    [HttpPost("inicio-sesion")]
    [AllowAnonymous]
    [EnableRateLimiting(PoliticasLimiteDePeticiones.INICIO_SESION)]
    [ProducesResponseType(typeof(RespuestaInicioSesionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<RespuestaInicioSesionDto>> IniciarSesionAsync(
        SolicitudInicioSesionDto solicitud,
        CancellationToken cancelacion)
    {
        Resultado<RespuestaInicioSesionDto> resultado =
            await _autenticacionServicio.IniciarSesionAsync(solicitud, cancelacion);

        // 401 y no 400: el dato venía bien formado, lo que falla es la identidad. Ese código
        // no se elige aquí — lo decide la traducción única de ControladorApi a partir del tipo
        // de error que clasificó el servicio. El mensaje es el mismo tanto si el usuario no
        // existe como si la contraseña no coincide, y esa decisión también se tomó allá.
        return resultado.EsExitoso ? Ok(resultado.Valor) : ProblemaDesde(resultado);
    }

    /// <summary>
    /// Devuelve la identidad del usuario dueño del token enviado.
    /// </summary>
    /// <remarks>
    /// No consulta la base de datos: todo lo que responde sale de los claims del token, que
    /// el middleware ya validó y firmó el propio servidor. Es lo que usa el frontend para
    /// saber, al recargar, si su token sigue siendo válido.
    /// </remarks>
    [HttpGet("sesion")]
    [Authorize(Policy = PoliticasAutorizacion.LECTURA)]
    [ProducesResponseType(typeof(SesionActualDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<SesionActualDto> ObtenerSesionActual()
    {
        // User.Identity.Name sale del claim "name" porque así se declaró NameClaimType en los
        // parámetros de validación; el rol, del claim "role".
        string nombreUsuario = User.Identity?.Name ?? string.Empty;

        // El Parse no puede fallar: la política LECTURA ya exigió que el rol sea uno de los
        // dos nombres del enum, así que un token con un rol desconocido no llega hasta aquí.
        RolUsuario rol = Enum.Parse<RolUsuario>(User.FindFirstValue(NombresDeClaim.ROL)!);

        return Ok(new SesionActualDto(nombreUsuario, rol));
    }
}
