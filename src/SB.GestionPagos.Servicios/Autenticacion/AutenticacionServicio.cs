using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Autenticacion;
using SB.GestionPagos.Aplicacion.Autenticacion.Dtos;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Seguridad;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Servicios.Autenticacion;

/// <summary>
/// Verificación de credenciales y emisión del token de acceso (RF-07).
/// </summary>
/// <remarks>
/// El servicio no sabe qué algoritmo protege las contraseñas ni cómo se firma un JWT: pide
/// ambas cosas a través de <see cref="IServicioHash"/> e <see cref="IGeneradorTokenJwt"/>,
/// declaradas en la capa Aplicación e implementadas en Infraestructura. Cambiar BCrypt por
/// otro algoritmo no toca esta clase.
/// </remarks>
public sealed class AutenticacionServicio : IAutenticacionServicio
{
    /// <summary>
    /// Un único mensaje para "el usuario no existe" y para "la contraseña no coincide".
    /// </summary>
    /// <remarks>
    /// Distinguirlos le confirmaría a un atacante qué nombres de usuario son válidos, y a
    /// partir de ahí solo le quedaría probar contraseñas contra una cuenta que sabe que existe.
    /// </remarks>
    private const string MENSAJE_CREDENCIALES_INVALIDAS = "El usuario o la contraseña no son correctos.";

    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly IServicioHash _servicioHash;
    private readonly IGeneradorTokenJwt _generadorTokenJwt;
    private readonly ILogger<AutenticacionServicio> _registrador;

    public AutenticacionServicio(
        IUsuarioRepositorio usuarioRepositorio,
        IServicioHash servicioHash,
        IGeneradorTokenJwt generadorTokenJwt,
        ILogger<AutenticacionServicio> registrador)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _servicioHash = servicioHash;
        _generadorTokenJwt = generadorTokenJwt;
        _registrador = registrador;
    }

    public async Task<Resultado<RespuestaInicioSesionDto>> IniciarSesionAsync(
        SolicitudInicioSesionDto solicitud,
        CancellationToken cancelacion)
    {
        Usuario? usuario = await _usuarioRepositorio.ObtenerPorNombreUsuarioAsync(
            solicitud.NombreUsuario,
            cancelacion);

        if (usuario is null || !_servicioHash.Verificar(solicitud.Contrasena, usuario.HashContrasena))
        {
            // Se registra el intento fallido —es exactamente lo que hay que poder auditar—
            // pero jamás la contraseña recibida.
            _registrador.LogWarning(
                "Intento de inicio de sesión fallido. Nombre de usuario: {NombreUsuario}.",
                solicitud.NombreUsuario);

            return Resultado<RespuestaInicioSesionDto>.CredencialesInvalidas(MENSAJE_CREDENCIALES_INVALIDAS);
        }

        TokenGenerado token = _generadorTokenJwt.Generar(usuario);

        // Se registra QUIÉN entró y con qué rol; el token no se registra nunca: quien leyera
        // el archivo de log podría usarlo para suplantar al usuario hasta que expire.
        _registrador.LogInformation(
            "Inicio de sesión exitoso. Nombre de usuario: {NombreUsuario}. Rol: {RolUsuario}. " +
            "Vencimiento del token: {FechaExpiracionUtc}.",
            usuario.NombreUsuario,
            usuario.Rol,
            token.FechaExpiracionUtc);

        RespuestaInicioSesionDto respuesta = new(
            token.Token,
            token.FechaExpiracionUtc,
            usuario.NombreUsuario,
            usuario.Rol);

        return Resultado<RespuestaInicioSesionDto>.Exitoso(respuesta);
    }
}
