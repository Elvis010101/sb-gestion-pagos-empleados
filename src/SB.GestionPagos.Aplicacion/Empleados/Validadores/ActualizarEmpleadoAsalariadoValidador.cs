using FluentValidation;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;

namespace SB.GestionPagos.Aplicacion.Empleados.Validadores;

/// <summary>Valida la forma del DTO de edición de un Empleado Asalariado.</summary>
public sealed class ActualizarEmpleadoAsalariadoValidador : AbstractValidator<ActualizarEmpleadoAsalariadoDto>
{
    public ActualizarEmpleadoAsalariadoValidador()
    {
        RuleFor(solicitud => solicitud.PrimerNombre).TextoObligatorio(LongitudMaxima.PRIMER_NOMBRE);
        RuleFor(solicitud => solicitud.ApellidoPaterno).TextoObligatorio(LongitudMaxima.APELLIDO_PATERNO);
        RuleFor(solicitud => solicitud.NumeroSeguroSocial).TextoObligatorio(LongitudMaxima.NUMERO_SEGURO_SOCIAL);
        RuleFor(solicitud => solicitud.Departamento).TextoObligatorio(LongitudMaxima.DEPARTAMENTO);
        RuleFor(solicitud => solicitud.Estado).ValorDeEnumeracionDefinido();
        RuleFor(solicitud => solicitud.SalarioSemanal).MontoNoNegativo();
    }
}
