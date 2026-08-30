using FluentValidation;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;

namespace SB.GestionPagos.Aplicacion.Empleados.Validadores;

/// <summary>
/// Valida la forma del DTO de alta de un Empleado Asalariado.
/// </summary>
/// <remarks>
/// Comprueba lo mismo que el constructor de la entidad, pero con otro objetivo: reunir TODOS
/// los errores de la petición y devolverlos juntos, para que el formulario los marque de una
/// sola vez. La entidad, en cambio, se detiene en el primero, porque su trabajo no es
/// informar sino impedir que exista un objeto inválido.
/// </remarks>
public sealed class CrearEmpleadoAsalariadoValidador : AbstractValidator<CrearEmpleadoAsalariadoDto>
{
    public CrearEmpleadoAsalariadoValidador()
    {
        RuleFor(solicitud => solicitud.PrimerNombre).TextoObligatorio(LongitudMaxima.PRIMER_NOMBRE);
        RuleFor(solicitud => solicitud.ApellidoPaterno).TextoObligatorio(LongitudMaxima.APELLIDO_PATERNO);
        RuleFor(solicitud => solicitud.NumeroSeguroSocial).TextoObligatorio(LongitudMaxima.NUMERO_SEGURO_SOCIAL);
        RuleFor(solicitud => solicitud.Departamento).TextoObligatorio(LongitudMaxima.DEPARTAMENTO);
        RuleFor(solicitud => solicitud.SalarioSemanal).MontoNoNegativo();
    }
}
