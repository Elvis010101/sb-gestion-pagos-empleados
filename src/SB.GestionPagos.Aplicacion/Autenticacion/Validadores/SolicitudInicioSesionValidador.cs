using FluentValidation;
using SB.GestionPagos.Aplicacion.Autenticacion.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;

namespace SB.GestionPagos.Aplicacion.Autenticacion.Validadores;

/// <summary>
/// Valida la forma de las credenciales recibidas.
/// </summary>
/// <remarks>
/// Comprueba únicamente que los campos vengan y tengan un tamaño razonable. NO impone reglas
/// de robustez de contraseña: esas pertenecen al alta de usuarios. Exigirlas al iniciar
/// sesión le diría a un atacante qué forma tienen las contraseñas válidas del sistema.
/// </remarks>
public sealed class SolicitudInicioSesionValidador : AbstractValidator<SolicitudInicioSesionDto>
{
    public SolicitudInicioSesionValidador()
    {
        RuleFor(solicitud => solicitud.NombreUsuario).TextoObligatorio(LongitudMaxima.NOMBRE_USUARIO);
        RuleFor(solicitud => solicitud.Contrasena).TextoObligatorio(LongitudMaxima.CONTRASENA);
    }
}
